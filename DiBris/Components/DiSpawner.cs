using DiBris.Managers;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace DiBris.Components
{
    internal class DiSpawner : IAsyncInitializable, IAffinity, INoteDebrisDidFinishEvent
    {
        private readonly Config _config;
        private readonly SiraLog _siraLog = null!;
        private readonly ProfileManager _profileManager;
        private readonly NoteDebris.Pool _noteDebrisPool;
        private readonly IDifficultyBeatmap _difficultyBeatmap;
        private readonly IReadonlyBeatmapData _readOnlyBeatmapData;
        private readonly List<(bool, Config)> _configs = new List<(bool, Config)>();
        private readonly ConditionalWeakTable<NoteDebris, NoteDebrisRigidbodyPhysics> _physicsTable = new ConditionalWeakTable<NoteDebris, NoteDebrisRigidbodyPhysics>();

        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.Accessor SimplePhysics = FieldAccessor<NoteDebrisRigidbodyPhysics, NoteDebrisSimplePhysics>.GetAccessor("_simplePhysics");
        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.Accessor RigidBody = FieldAccessor<NoteDebrisRigidbodyPhysics, Rigidbody>.GetAccessor("_rigidbody");
        private static readonly FieldAccessor<NoteDebrisRigidbodyPhysics, bool>.Accessor RigidFirstUpdate = FieldAccessor<NoteDebrisRigidbodyPhysics, bool>.GetAccessor("_firstUpdate");
        private static readonly FieldAccessor<NoteDebrisSimplePhysics, bool>.Accessor SimpleFirstUpdate = FieldAccessor<NoteDebrisSimplePhysics, bool>.GetAccessor("_firstUpdate");
        private static readonly FieldAccessor<NoteDebrisSimplePhysics, Vector3>.Accessor Gravity = FieldAccessor<NoteDebrisSimplePhysics, Vector3>.GetAccessor("_gravity");
        private static readonly FieldAccessor<NoteDebris, NoteDebrisPhysics>.Accessor RigidPhysics = FieldAccessor<NoteDebris, NoteDebrisPhysics>.GetAccessor("_physics");

        public async Task InitializeAsync(CancellationToken token)
        {
            var unBrokenConfigs = new List<Config> { this._config };
            unBrokenConfigs.AddRange(await this._profileManager.GetMirrorConfigs());
            foreach (var conf in unBrokenConfigs) {
                var parameters = conf.Parameters;
                var shouldBreak = false;
                var failsLength = false;
                var failsNJS = false;
                var failsNPS = false;
                if (parameters.DoLength) {
                    this._siraLog.Info(this._difficultyBeatmap.level.songDuration);
                    if (this._difficultyBeatmap.level.songDuration >= parameters.Length) {
                        failsLength = true;
                    }
                }
                if (!shouldBreak && parameters.DoNJS && this._difficultyBeatmap.noteJumpMovementSpeed >= parameters.NJS) {
                    failsNJS = true;
                }
                if (parameters.DoNPS) {
                    var data = this._readOnlyBeatmapData;
                    var levelData = this._difficultyBeatmap.level.beatmapLevelData;
                    if (data.allBeatmapDataItems.OfType<NoteData>().Count() / levelData.audioClip.length >= parameters.NPS) {
                        failsNPS = true;
                    }
                }
                if (parameters.Mode == Models.DisableMode.All) {
                    shouldBreak = (!parameters.DoLength || failsLength) && (!parameters.DoNJS || failsNJS) && (!parameters.DoNPS || failsNPS);
                }
                else {
                    shouldBreak = failsLength || failsNJS || failsNPS;
                }
                this._configs.Add((!shouldBreak, conf));
            }
        }

        [Inject]
        public DiSpawner(Config config, SiraLog siraLog, ProfileManager profileManager, NoteDebris.Pool _noteDebrisPool, IDifficultyBeatmap difficultyBeatmap, IReadonlyBeatmapData readonlyBeatmapData)
        {
            this._config = config;
            this._siraLog = siraLog;
            this._profileManager = profileManager;
            this._noteDebrisPool = _noteDebrisPool;
            this._difficultyBeatmap = difficultyBeatmap;
            this._readOnlyBeatmapData = readonlyBeatmapData;
        }

        [AffinityPatch(typeof(NoteDebrisSpawner), nameof(NoteDebrisSpawner.SpawnDebris))]
        [AffinityPrefix]
        protected bool SpawnDebris(NoteDebrisSpawner __instance, Vector3 cutPoint, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, Vector3 notePos, Quaternion noteRotation, Vector3 noteScale, ColorType colorType, float timeToNextColorNote, Vector3 moveVec, float ____fromCenterSpeed, float ____cutDirMultiplier, float ____rotation, float ____moveSpeedMultiplier)
        {
            foreach (var config in this._configs) {
                var conf = config.Item2;
                if (conf.RemoveDebris || !config.Item1) {
                    return false;
                }

                var newPos = notePos;
                newPos *= conf.AbsolutePositionScale;
                newPos += conf.AbsolutePositionOffset;
                var shouldInteract = conf.VelocityMultiplier != 0;

                var noteRot = noteRotation;
                if (conf.FixateRotationToZero) {
                    noteRot = Quaternion.identity;
                }

                if (conf.SnapToGrid) {
                    var snapScale = conf.GridScale / 4f;
                    var snapX = System.Convert.ToSingle(System.Math.Round(newPos.x, System.MidpointRounding.ToEven) * snapScale);
                    var snapY = System.Convert.ToSingle(System.Math.Round(newPos.y, System.MidpointRounding.ToEven) * snapScale);
                    var snapZ = System.Convert.ToSingle(System.Math.Round(newPos.z, System.MidpointRounding.ToEven) * snapScale);
                    newPos = new Vector3(snapX, snapY, snapZ);
                }

                if (conf.FixateZPos) {
                    newPos = new Vector3(newPos.x, newPos.y, conf.AbsolutePositionOffsetZ);
                }

                var noteSize = noteScale * conf.Scale;

                var debrisA = this.DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, ____cutDirMultiplier, ____moveSpeedMultiplier, ____rotation, __instance.transform, out var fixedLifetimeLength, out var b, out var a, out var a2);
                if (conf.FixedLifetime) {
                    fixedLifetimeLength = conf.FixedLifetimeLength;
                }

                debrisA.Init(colorType, newPos, noteRot, moveVec, noteScale, __instance.transform.position, __instance.transform.rotation, cutPoint, -cutNormal, (-a * ____fromCenterSpeed + b) * conf.VelocityMultiplier, -a2 * conf.RotationMultiplier, fixedLifetimeLength * conf.LifetimeMultiplier);
                __instance.StartCoroutine(this.MultiplyGravity(debrisA, conf.GravityMultiplier, shouldInteract));

                var debrisB = this.DebrisDecorator(cutPoint.y, cutNormal, saberSpeed, saberDir, timeToNextColorNote, moveVec, ____cutDirMultiplier, ____moveSpeedMultiplier, ____rotation, __instance.transform, out var fixedLifetimeLength2, out var b2, out var a3, out var a4);
                if (conf.FixedLifetime) {
                    fixedLifetimeLength2 = conf.FixedLifetimeLength;
                }

                debrisB.Init(colorType, newPos, noteRot, moveVec, noteScale, __instance.transform.position, __instance.transform.rotation, cutPoint, cutNormal, (a3 * ____fromCenterSpeed + b2) * conf.VelocityMultiplier, a4 * conf.RotationMultiplier, fixedLifetimeLength2 * conf.LifetimeMultiplier);
                __instance.StartCoroutine(this.MultiplyGravity(debrisB, conf.GravityMultiplier, shouldInteract));
            }
            return false;
        }

        private NoteDebris DebrisDecorator(float cutY, Vector3 cutNormal, float saberSpeed, Vector3 saberDir, float timeToNextColorNote, Vector3 moveVec, float cutDirMult, float moveSpeedMult, float rotation, Transform transform, out float liquid, out Vector3 next, out Vector3 forceEn, out Vector3 torque)
        {
            var debris = this._noteDebrisPool.Spawn();
            debris.didFinishEvent.Add(this);
            debris.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var magnitude = moveVec.magnitude;
            liquid = Mathf.Clamp(timeToNextColorNote + 0.05f, 0.2f, 2f);
            var projection = Vector3.ProjectOnPlane(saberDir, moveVec / magnitude);
            next = projection * (saberSpeed * cutDirMult) + moveVec * moveSpeedMult;
            forceEn = transform.rotation * (cutNormal + Random.onUnitSphere * 0.1f);
            torque = transform.rotation * Vector3.Cross(cutNormal, projection) * rotation / Mathf.Max(1f, timeToNextColorNote * 2f);
            next.y = cutY >= 1.3 ? Mathf.Max(next.y, 0f) : Mathf.Min(next.y, 0);

            if (!this._physicsTable.TryGetValue(debris, out var rigidPhysics)) {
                rigidPhysics = (RigidPhysics(ref debris) as NoteDebrisRigidbodyPhysics)!;
                this._physicsTable.Add(debris, rigidPhysics);
            }

            return debris;
        }

        private IEnumerator MultiplyGravity(NoteDebris noteDebris, float multiplier, bool shouldInteract)
        {
            yield return new WaitForEndOfFrame();
            if (this._physicsTable.TryGetValue(noteDebris, out var rigidPhysics)) {
                this.MultiplyGravityInternal(rigidPhysics, multiplier, shouldInteract);
            }
            else {
                this.MultiplyGravityInternal((RigidPhysics(ref noteDebris) as NoteDebrisRigidbodyPhysics)!, multiplier, shouldInteract);
            }
        }

        private void MultiplyGravityInternal(NoteDebrisRigidbodyPhysics rigidPhysics, float multiplier, bool shouldInteract)
        {
            var simplePhysics = SimplePhysics(ref rigidPhysics);

            SimpleFirstUpdate(ref simplePhysics) = true;
            RigidFirstUpdate(ref rigidPhysics) = true;

            Gravity(ref simplePhysics) *= multiplier;
            if (multiplier == 0) {
                RigidBody(ref rigidPhysics).useGravity = false;
            }
            else if (0 > multiplier) {
                rigidPhysics.enabled = false;
                simplePhysics.enabled = true;
            }
            RigidBody(ref rigidPhysics).isKinematic = !shouldInteract;
        }

        public void HandleNoteDebrisDidFinish(NoteDebris noteDebris)
        {
            noteDebris.didFinishEvent.Remove(this);
            if (this._physicsTable.TryGetValue(noteDebris, out var rigidPhysics)) {
                var simplePhysics = SimplePhysics(ref rigidPhysics);
                RigidBody(ref rigidPhysics).isKinematic = false;
                RigidBody(ref rigidPhysics).useGravity = true;
                Gravity(ref simplePhysics) = Physics.gravity;
            }
            this._noteDebrisPool.Despawn(noteDebris);
        }
    }
}
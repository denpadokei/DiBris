using SiraUtil.Extras;
using Zenject;

namespace DiBris.Managers
{
    internal class Trailer : IInitializable, ITickable
    {
        [Inject]
        private readonly AudioTimeSyncController timeSync = null!;

        [Inject]
        private readonly Config config = null!;

        private bool _done;

        public async void Initialize()
        {
            await Utilities.AwaitSleep(500);
            this.config.RemoveDebris = false;
            this.config.VelocityMultiplier = 1f;
            this.config.GravityMultiplier = 1f;
            this.config.LifetimeMultiplier = 1f;
        }

        public void Tick()
        {
            if (this.timeSync.songTime >= 22f && !this._done) {

                this.config.VelocityMultiplier = 0f;
                this.config.GravityMultiplier = 0f;
                this.config.LifetimeMultiplier = 10f;
                this._done = true;
            }
        }
    }
}

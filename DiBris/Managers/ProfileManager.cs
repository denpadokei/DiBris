using IPA.Config.Data;
using IPA.Config.Stores.Converters;
using IPA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiBris.Managers
{
    internal class ProfileManager
    {
        private bool _loaded;
        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly List<(FileInfo, Config)> _loadedConfigs;
        private readonly DirectoryInfo _profileDirectory;

        public ProfileManager(Config config, SiraLog siraLog)
        {
            this._config = config;
            this._siraLog = siraLog;
            this._loadedConfigs = new List<(FileInfo, Config)>();
            this._profileDirectory = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, "Di", "Bris", "Profiles"));
        }

        public async Task<IEnumerable<Config>> GetMirrorConfigs()
        {
            var theGoodOnes = new List<Config>();
            var profiles = await this.AllSubProfiles();
            foreach (var profile in profiles) {
                if (this._config.MirrorConfigs.Contains(profile.Name)) {
                    theGoodOnes.Add(profile);
                }
            }
            return theGoodOnes;
        }

        public async Task<IEnumerable<Config>> AllSubProfiles()
        {
            if (!this._loaded) {
                this._profileDirectory.Create();

                foreach (var file in this._profileDirectory.EnumerateFiles().Where(f => f.Extension == ".json")) {
                    try {
                        var fs = file.OpenRead();
                        var sr = new StreamReader(fs);
                        var jtr = new JsonTextReader(sr);
                        var token = await JToken.ReadFromAsync(jtr);
                        var config = CustomObjectConverter<Config>.Deserialize(this.VisitToValue(token), null);
                        jtr.Close();
                        sr.Close();
                        fs.Close();
                        this._loadedConfigs.Add((file, config));
                    }
                    catch {
                        this._siraLog.Error($"Error loading profile at {file.Name}");
                    }
                }

                this._loaded = true;
            }
            return this._loadedConfigs.Select(lf => lf.Item2);
        }

        public void Save(Config config)
        {
            var fileConf = this._loadedConfigs.FirstOrDefault(lf => lf.Item2.Name == config.Name);
            try {
                var val = CustomObjectConverter<Config>.Serialize(config, null);
                var file = new FileInfo(Path.Combine(this._profileDirectory.FullName, $"{config.Name}.json"));
                File.WriteAllText(file.FullName, val.ToString());
                if (fileConf == default) {
                    this._loadedConfigs.Add((file, CustomObjectConverter<Config>.Deserialize(val, null)));
                }
                else {
                    this._loadedConfigs.Remove(fileConf);
                    this._loadedConfigs.Add((file, CustomObjectConverter<Config>.Deserialize(val, null)));
                }
            }
            catch (Exception e) {
                this._siraLog.Error($"Could not save config. {e.Message}");
            }
        }

        public void Delete(Config config)
        {
            var fileConf = this._loadedConfigs.FirstOrDefault(lf => lf.Item2.Name == config.Name);
            if (fileConf == default) {
                return;
            }

            this._loadedConfigs.Remove(fileConf);
            if (fileConf.Item1.Exists) {
                fileConf.Item1.Delete();
            }
        }

        // i literally just stole this from danike.
        private Value VisitToValue(JToken tok)
        {
            if (tok == null) {
                return Value.Null();
            }

            switch (tok.Type) {
                case JTokenType.Null:
                    return Value.Null();
                case JTokenType.Boolean:
                    return Value.Bool(((tok as JValue)!.Value as bool?) ?? false);
                case JTokenType.String:
                    var val = (tok as JValue)!.Value;
                    if (val is string s) {
                        return Value.Text(s);
                    }
                    else if (val is char c) {
                        return Value.Text("" + c);
                    }
                    else {
                        return Value.Text(string.Empty);
                    }

                case JTokenType.Integer:
                    val = (tok as JValue)!.Value;
                    if (val is long l) {
                        return Value.Integer(l);
                    }
                    else if (val is ulong u) {
                        return Value.Integer((long)u);
                    }
                    else {
                        return Value.Integer(0);
                    }

                case JTokenType.Float:
                    val = (tok as JValue)!.Value;
                    if (val is decimal dec) {
                        return Value.Float(dec);
                    }
                    else if (val is double dou) {
                        return Value.Float((decimal)dou);
                    }
                    else if (val is float flo) {
                        return Value.Float((decimal)flo);
                    }
                    else {
                        return Value.Float(0);
                    }

                case JTokenType.Array:
                    return Value.From((tok as JArray).Select(this.VisitToValue));
                case JTokenType.Object:
                    return Value.From((tok as IEnumerable<KeyValuePair<string, JToken>>)
                        .Select(kvp => new KeyValuePair<string, Value>(kvp.Key, this.VisitToValue(kvp.Value))));
                default:
                    throw new ArgumentException($"Unknown {nameof(JTokenType)} in parameter");
            }
        }
    }
}
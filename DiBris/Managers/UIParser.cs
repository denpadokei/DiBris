using BeatSaberMarkupLanguage;
using DiBris.UI;
using IPA.Loader;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zenject;

namespace DiBris.Managers
{
    internal class UIParser
    {
        private readonly SiraLog _siraLog;
        private readonly DiContainer _container;
        private readonly List<Parseable> _parseables;
        private readonly PluginMetadata _pluginMetadata;

        public UIParser(SiraLog siraLog, DiContainer container, [Inject(Id = nameof(DiBris))] PluginMetadata pluginMetadata)
        {
            this._siraLog = siraLog;
            this._container = container;
            this._pluginMetadata = pluginMetadata;
            this._parseables = new List<Parseable>();
        }

        public async Task Parse(Parseable parseable)
        {
            if (this._parseables.Contains(parseable)) {
                return;
            }

            // Load BSML Content (Asynchronously)
            var stream = this._pluginMetadata.Assembly.GetManifestResourceStream(parseable.ContentPath);
            var sr = new StreamReader(stream);
            var content = await sr.ReadToEndAsync();
            sr.Dispose();
            stream.Dispose();

            this._parseables.Add(parseable);
            this._container.Inject(parseable);
            BSMLParser.instance.Parse(content, parseable.root.gameObject, parseable);
        }
    }
}
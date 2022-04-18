using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Loader;
using System.IO;
using System.Threading.Tasks;
using Zenject;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.info-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\info-view.bsml")]
    internal class BriInfoView : BSMLAutomaticViewController
    {
        [Inject(Id = nameof(DiBris))]
        protected readonly PluginMetadata _pluginMetadata = null!;

        [Inject]
        protected readonly IPlatformUserModel _platformUserModel = null!;

        private string _infoText = "Loading...";
        [UIValue("info-text")]
        protected string InfoText
        {
            get => this._infoText;
            set
            {
                this._infoText = value;
                this.NotifyPropertyChanged();
            }
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            // Load Text Asset (Asynchronously)
            var stream = this._pluginMetadata.Assembly.GetManifestResourceStream($"{nameof(DiBris)}.Resources.info.txt");
            var sr = new StreamReader(stream);
            var text = await sr.ReadToEndAsync();
            sr.Dispose();
            stream.Dispose();

            var user = await this._platformUserModel.GetUserInfo();
            switch (user.platformUserId) {
                case "76561198064659288":
                    text += "hi denyah";
                    break;
                case "76561198055583703":
                    text += "we are back? back from where?";
                    break;
                case "76561198035698451":
                    text += "dibris? i hardly even know her!";
                    break;
            }
            this.InfoText = text;
        }
    }
}
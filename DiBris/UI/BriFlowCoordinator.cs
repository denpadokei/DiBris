using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace DiBris.UI
{
    internal class BriFlowCoordinator : FlowCoordinator
    {
        private Config _config = null!;
        private BriMainView _briMainView = null!;
        private BriInfoView _briInfoView = null!;
        private BriProfileView _briProfileView = null!;
        private BriSettingsView _briSettingsView = null!;

        private MainFlowCoordinator _mainFlowCoordinator = null!;

        [Inject]
        public void Construct(Config config, BriMainView briMainView, BriInfoView briInfoView, BriProfileView briProfileView, BriSettingsView briSettingsView, MainFlowCoordinator mainFlowCoordinator)
        {
            this._config = config;
            this._briMainView = briMainView;
            this._briInfoView = briInfoView;
            this._briProfileView = briProfileView;
            this._briSettingsView = briSettingsView;
            this._mainFlowCoordinator = mainFlowCoordinator;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool _)
        {
            if (firstActivation) {
                this.showBackButton = true;
                this.SetTitle(nameof(DiBris));
            }
            if (addedToHierarchy) {
                this.ProvideInitialViewControllers(this._briMainView);
            }

            this._briMainView.EventNavigated += this.NavigationReceived;
            this._config.Updated += this.Changed;
        }

        private void Changed(Config _)
        {

        }

        private void NavigationReceived(NavigationEvent navEvent)
        {
            switch (navEvent) {
                case NavigationEvent.Info:
                    this.SetLeftScreenViewController(this._briInfoView, ViewController.AnimationType.In);
                    this.SetRightScreenViewController(null, ViewController.AnimationType.Out);
                    break;
                case NavigationEvent.Profile:
                    this.SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    this.SetRightScreenViewController(this._briProfileView, ViewController.AnimationType.In);
                    break;
                case NavigationEvent.Settings:
                    this.SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    this.SetRightScreenViewController(this._briSettingsView, ViewController.AnimationType.In);
                    break;
                default:
                    this.SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    this.SetRightScreenViewController(null, ViewController.AnimationType.Out);
                    break;
            }
            if (navEvent == NavigationEvent.Reset) {
                var version = this._config.Version;
                this._config.CopyFrom(new Config
                {
                    Version = version
                });
                this._config.Save();
                this._config.Changed();
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            this._briMainView.EventNavigated -= this.NavigationReceived;
            this._config.Updated -= this.Changed;
            this._config.Save();
        }

        protected override void BackButtonWasPressed(ViewController _)
        {
            this._mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        public enum NavigationEvent
        {
            Info,
            Profile,
            Settings,
            Unknown,
            Reset
        }
    }
}
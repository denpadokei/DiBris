using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using DiBris.UI;
using System;
using Zenject;

namespace DiBris.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly BriFlowCoordinator _briFlowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;

        public MenuButtonManager(BriFlowCoordinator briFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
        {
            this._briFlowCoordinator = briFlowCoordinator;
            this._mainFlowCoordinator = mainFlowCoordinator;
            this._menuButton = new MenuButton(nameof(DiBris), this.ShowFlow);
        }

        private void ShowFlow()
        {
            this._mainFlowCoordinator.PresentFlowCoordinator(this._briFlowCoordinator);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(this._menuButton);
        }

        public void Dispose()
        {
            try {
                MenuButtons.instance.UnregisterButton(this._menuButton);
            }
            catch {
            }
        }
    }
}
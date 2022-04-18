using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using DiBris.Managers;
using HMUI;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.main-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\main-view.bsml")]
    internal class BriMainView : BSMLAutomaticViewController
    {
        #region Injections

        [Inject]
        protected readonly TimeTweeningManager _tweeningManager = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [Inject(Id = nameof(DiBris))]
        protected readonly PluginMetadata pluginMetadata = null!;

        [Inject]
        protected readonly Config _config = null!;

        [UIComponent("desc-text")]
        protected CurvedTextMeshPro descText = null!;

        [UIComponent("button-grid")]
        protected RectTransform buttonGrid = null!;

        [UIComponent("logo-image")]
        protected ImageView logoImage = null!;

        [UIValue("version")]
        protected string Version => $"v{this.pluginMetadata.HVersion}";

        [UIParams]
        protected BSMLParserParams parserParams = null!;

        #endregion

        internal Material noGlowMatRound;
        public event Action<BriFlowCoordinator.NavigationEvent> EventNavigated;
        private readonly List<NoTransitionsButton> buttons = new List<NoTransitionsButton>();
        private readonly List<TextTransitioner> _textTransitioners = new List<TextTransitioner>();

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (firstActivation) {
                this.buttons.AddRange(this.buttonGrid.GetComponentsInChildren<NoTransitionsButton>(true));
            }

            this.descText.alpha = 0f;
            this._textTransitioners.Clear();
            this._textTransitioners.Add(new TextTransitioner("Learn more about this mod", this.buttons[0]));
            this._textTransitioners.Add(new TextTransitioner("Create, switch, and remove your config profile(s)", this.buttons[1]));
            this._textTransitioners.Add(new TextTransitioner("Edit the settings for the current profile", this.buttons[2]));
            this._textTransitioners.Add(new TextTransitioner("Donate to the mod creator (opens in browser)", this.buttons[3]));
            this._textTransitioners.Add(new TextTransitioner("Open the GitHub page (in your browser)", this.buttons[4]));
            this._textTransitioners.Add(new TextTransitioner("Reset all your settings.", this.buttons[5]));
            foreach (var transitioner in this._textTransitioners) {
                transitioner.StateChanged += this.ButtonSelectionStateChanged;
            }

            if (firstActivation || this.noGlowMatRound == null) {
                // Yes. It was either this or recursively dig through 3 object. Will be making an API to expose things like this easier in the future.
                this.noGlowMatRound = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlowRoundEdge").First();
                this.logoImage.material = this.noGlowMatRound;
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            foreach (var transitioner in this._textTransitioners) {
                transitioner.StateChanged -= this.ButtonSelectionStateChanged;
                transitioner.Dispose();
            }
            this._textTransitioners.Clear();
        }

        private void ButtonSelectionStateChanged(string descriptionText, NoTransitionsButton.SelectionState selectionState)
        {
            var to = selectionState == NoTransitionsButton.SelectionState.Normal ? 0f : 1f;
            this._tweeningManager.KillAllTweens(this);
            this.descText.text = descriptionText;
            var from = this.descText.alpha;

            this._tweeningManager.AddTween(new FloatTween(from, to, value =>
            {
                this.descText.alpha = value;
            }, 0.2f, EaseType.InSine, 0.1f), this);
        }

        [UIAction("clicked-info-button")]
        protected void ClickedInfoButton()
        {
            EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Info);
        }

        [UIAction("clicked-github-button")]
        protected void ClickedGithubButton()
        {
            Application.OpenURL("https://github.com/Auros/DiBris");
        }

        [UIAction("clicked-donate-button")]
        protected void ClickedDonateButton()
        {
            Application.OpenURL("https://ko-fi.com/aurosnex");
        }

        [UIAction("clicked-profile-button")]
        protected void ClickedProfileButton()
        {
            EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Profile);
        }

        [UIAction("clicked-settings-button")]
        protected void ClickedSettingsButton()
        {
            EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Settings);
        }

        [UIAction("reset")]
        protected async Task Reset()
        {
            var allSubProfiles = await this._profileManager.AllSubProfiles();
            foreach (var profile in allSubProfiles) {
                this._profileManager.Delete(profile);
            }

            var version = this._config.Version;
            this._config.CopyFrom(new Config
            {
                Version = version
            });
            this._config.Save();
            this._config.Changed();
            this.parserParams.EmitEvent("hide-reset");
            EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Reset);
        }

        private class TextTransitioner : IDisposable
        {
            private readonly string _text;
            private readonly NoTransitionsButton _button;
            public event Action<string, NoTransitionsButton.SelectionState> StateChanged;

            public TextTransitioner(string text, NoTransitionsButton button)
            {
                this._text = text;
                this._button = button;
                button.selectionStateDidChangeEvent += this.SelectionDidChange;
            }

            private void SelectionDidChange(NoTransitionsButton.SelectionState state)
            {
                StateChanged?.Invoke(this._text, state);
            }

            public void Dispose()
            {
                this._button.selectionStateDidChangeEvent -= this.SelectionDidChange;
            }
        }
    }
}
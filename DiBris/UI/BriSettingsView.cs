using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using DiBris.Managers;
using DiBris.UI.Settings;
using HMUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.settings-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\settings-view.bsml")]
    internal class BriSettingsView : BSMLAutomaticViewController
    {
        protected Config _config = null!;
        protected UIParser _uiParser = null!;
        protected ProfileManager _profileManager = null!;

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        [UIComponent("title-bar")]
        protected readonly ImageView titleBar = null!;

        [UIComponent("tab-selector")]
        protected readonly TabSelector tabSelector = null!;

        [UIValue("setting-windows")]
        protected readonly List<object> settingWindows = new List<object>();

        [Inject]
        protected void Construct(Config config, UIParser uiParser, ProfileManager profileManager)
        {
            this._config = config;
            this._uiParser = uiParser;
            this._profileManager = profileManager;
            this.settingWindows.Add(new General());
            this.settingWindows.Add(new Multipliers());
            this.settingWindows.Add(new Positioning());
            this.settingWindows.Add(new Conditions());
            this.settingWindows.Add(new Miscellaneous());
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            this.titleBar.color0 = Color.white;
            this.titleBar.color1 = Color.white.ColorWithAlpha(0);
            this.titleBar.SetAllDirty();
            if (this.settingWindows[0] is Parseable parseable) {
                await this._uiParser.Parse(parseable);
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            this.tabSelector.textSegmentedControl.didSelectCellEvent += this.SelectedCell;

            this.parserParams.EmitEvent("refresh");
            foreach (IRefreshable refreshable in this.settingWindows) {
                refreshable.Refresh();
            }
        }

        private async void SelectedCell(SegmentedControl _, int index)
        {
            if (this.settingWindows[index] is Parseable parseable) {
                await this._uiParser.Parse(parseable);
            }
        }

        protected override async void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            await this._profileManager.AllSubProfiles();
            this._profileManager.Save(this._config);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            this.tabSelector.textSegmentedControl.didSelectCellEvent -= this.SelectedCell;
        }
    }
}
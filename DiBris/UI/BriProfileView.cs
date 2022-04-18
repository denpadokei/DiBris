using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using DiBris.Managers;
using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.profile-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\profile-view.bsml")]
    internal class BriProfileView : BSMLAutomaticViewController
    {
        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [UIComponent("profile-list")]
        protected readonly CustomListTableData profileList = null!;

        private string _statusText = " ";
        [UIValue("status-text")]
        public string StatusText
        {
            get => this._statusText;
            set
            {
                this._statusText = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _activeProfile = "![NOT SET]";
        [UIValue("active-profile")]
        public string ActiveProfile
        {
            get => this._activeProfile;
            set
            {
                this._activeProfile = $"Active Profile - {value}";
                this.NotifyPropertyChanged();
            }
        }

        protected int? SelectedIndex
        {
            get
            {
                var selectedIndexies = this.profileList.tableView.GetField<HashSet<int>, TableView>("_selectedCellIdxs");
                if (selectedIndexies.Count == 0) {
                    return null;
                }

                return selectedIndexies.First();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            this.ActiveProfile = this._config.Name;
            _ = this.ReloadProfiles(true);
        }

        private async Task ReloadProfiles(bool updateText = false)
        {
            this.profileList.data.Clear();
            this.profileList.tableView.ReloadData();
            var profiles = await this._profileManager.AllSubProfiles();
            foreach (var profile in profiles) {
                this.profileList.data.Add(new ConfigCellInfo(profile));
            }
            this.profileList.tableView.ReloadData();
            this.profileList.tableView.ClearSelection();

            if (updateText) {
                this.StatusText = $"Loaded {this.profileList.data.Count} profiles.";
            }
        }

        [UIAction("save")]
        protected async Task SaveCurrent()
        {
            var profiles = await this._profileManager.AllSubProfiles();
            var overwrite = profiles.Any(u => u.Name == this._config.Name);
            if (overwrite) {
                this.StatusText = $"<color=#f5b642>Profile overwritten.</color>";
            }
            else {
                this.StatusText = "<color=#32d62f>Profile saved.</color>";
            }

            this._profileManager.Save(this._config);
            await this.ReloadProfiles();
            if (!overwrite) {
                this.profileList.tableView.ScrollToCellWithIdx(this.profileList.data.Count(), TableView.ScrollPositionType.End, true);
            }
        }

        [UIAction("load")]
        protected void Load()
        {
            if (this.SelectedIndex is null) {
                this.StatusText = "<color=#f54242>No Profile Selected.</color>";
                return;
            }
            var config = (this.profileList.data[this.SelectedIndex.Value] as ConfigCellInfo)!;
            this._config.CopyFrom(config.Config);
            this.ActiveProfile = config.Config.Name;
            this.StatusText = "<color=#32d62f>Profile applied!</color>";
        }

        [UIAction("delete")]
        protected async Task Delete()
        {
            if (this.SelectedIndex is null) {
                this.StatusText = "<color=#f54242>No Profile Selected.</color>";
                return;
            }
            var config = (this.profileList.data[this.SelectedIndex.Value] as ConfigCellInfo)!;
            this._profileManager.Delete(config.Config);
            this.StatusText = "<color=#32d62f>Profile deleted!</color>";
            await this.ReloadProfiles();
        }

        private class ConfigCellInfo : CustomCellInfo
        {
            public Config Config { get; }

            public ConfigCellInfo(Config config) : base(config.Name, "", null!)
            {
                this.Config = config;
            }
        }
    }
}
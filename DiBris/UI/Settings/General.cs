using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using DiBris.Managers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace DiBris.UI.Settings
{
    internal class General : Parseable, IRefreshable
    {
        public override string Name => nameof(General);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(General).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [UIComponent("mirror-list")]
        protected CustomCellListTableData mirrorTable = null!;

        [UIComponent("mirror-root")]
        protected RectTransform mirrorRoot = null!;

        [UIValue("profile-name")]
        protected string ProfileName
        {
            get => this._config.Name;
            set => this._config.Name = value;
        }

        [UIValue("remove-all-debris")]
        protected bool RemoveAllDebris
        {
            get => this._config.RemoveDebris;
            set => this._config.RemoveDebris = value;
        }

        public override async void Refresh()
        {
            this.mirrorTable.data.Clear();
            this.mirrorRoot.gameObject.SetActive(true);
            foreach (Transform t in this.mirrorTable.tableView.contentTransform) {
                UnityEngine.Object.Destroy(t.gameObject);
            }
            foreach (var mirror in this._config.MirrorConfigs) {
                this.mirrorTable.data.Add(new Cell(mirror, true, this.MirrorChange));
            }
            foreach (var config in await this._profileManager.AllSubProfiles()) {
                if (!this._config.MirrorConfigs.Contains(config.Name) && this._config.Name != config.Name) {
                    this.mirrorTable.data.Add(new Cell(config.Name, false, this.MirrorChange));
                }
            }
            this.mirrorTable.tableView.ReloadData();
            if (this.mirrorTable.data.Count == 0) {
                this.mirrorRoot.gameObject.SetActive(false);
            }
            base.Refresh();
        }

        public void MirrorChange(string name, bool state)
        {
            if (state) {
                this._config.MirrorConfigs.Add(name);
            }
            else {
                this._config.MirrorConfigs.Remove(name);
            }
        }

        public class Cell : INotifyPropertyChanged
        {
            [UIValue("name")]
            public string Name { get; }

            protected bool _enabled;
            [UIValue("enabled")]
            public bool Enabled
            {
                get => this._enabled;
                set
                {
                    this._enabled = value;
                    this.NotifyPropertyChanged(nameof(this.Enabled));
                    this.NotifyPropertyChanged(nameof(this.Status));
                    this.NotifyPropertyChanged(nameof(this.StatusColor));
                    this.NotifyPropertyChanged(nameof(this.ToggleString));
                }
            }

            [UIValue("status")]
            public string Status => this.Enabled ? "✅" : "❌";

            [UIValue("status-color")]
            public string StatusColor => this.Enabled ? "lime" : "red";

            [UIValue("toggle-string")]
            public string ToggleString => this.Enabled ? "-" : "+";

            private readonly Action<string, bool> Changed;
            public event PropertyChangedEventHandler PropertyChanged;

            public Cell(string name, bool initialState, Action<string, bool> onStateChange)
            {
                this.Name = name;
                this.Changed = onStateChange;
                this._enabled = initialState;
            }

            [UIAction("change-state")]
            protected void ChangedState()
            {
                this.Enabled = !this.Enabled;
                this.Changed?.Invoke(this.Name, this.Enabled);
            }

            protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                try {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                catch { }
            }
        }
    }
}
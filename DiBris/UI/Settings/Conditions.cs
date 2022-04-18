using BeatSaberMarkupLanguage.Attributes;
using DiBris.Models;
using SiraUtil.Extras;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace DiBris.UI.Settings
{
    internal class Conditions : Parseable, INotifyPropertyChanged
    {
        public override string Name => nameof(Conditions);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Conditions).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        public event PropertyChangedEventHandler PropertyChanged;

        [UIValue("on-njs")]
        protected bool OnNJS
        {
            get => this._config.Parameters.DoNJS;
            set => this._config.Parameters.DoNJS = value;
        }

        [UIValue("on-nps")]
        protected bool OnNPS
        {
            get => this._config.Parameters.DoNPS;
            set => this._config.Parameters.DoNPS = value;
        }

        [UIValue("on-length")]
        protected bool OnLength
        {
            get => this._config.Parameters.DoLength;
            set => this._config.Parameters.DoLength = value;
        }

        [UIValue("njs")]
        protected float NJS
        {
            get => this._config.Parameters.NJS;
            set => this._config.Parameters.NJS = value;
        }

        [UIValue("nps")]
        protected float NPS
        {
            get => this._config.Parameters.NPS;
            set => this._config.Parameters.NPS = value;
        }

        [UIValue("length")]
        protected float Length
        {
            get => this._config.Parameters.Length;
            set => this._config.Parameters.Length = value;
        }

        [UIValue("mode")]
        protected DisableMode Mode
        {
            get => this._config.Parameters.Mode;
            set => this._config.Parameters.Mode = value;
        }

        [UIValue("condition-types")]
        protected List<object> Sensitivities => ((DisableMode[])Enum.GetValues(typeof(DisableMode))).Cast<object>().ToList();

        [UIAction("hidden-prop-change")]
        protected async Task HiddenPropertyChanged(bool _)
        {
            await Utilities.AwaitSleep(1);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.OnNJS)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.OnNPS)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.OnLength)));
        }

        [UIAction("njs-formatter")]
        protected string NJSFormatter(float value)
        {
            return $"{value:N} NJS";
        }

        [UIAction("nps-formatter")]
        protected string NPSFormatter(float value)
        {
            return $"{value:N} NPS";
        }

        [UIAction("time-formatter")]
        protected string TimeFormatter(float value)
        {
            return $"{(int)value} seconds";
        }
    }
}
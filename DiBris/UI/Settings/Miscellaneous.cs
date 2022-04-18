using BeatSaberMarkupLanguage.Attributes;
using SiraUtil.Extras;
using System.ComponentModel;
using System.Threading.Tasks;
using Zenject;

namespace DiBris.UI.Settings
{
    internal class Miscellaneous : Parseable, INotifyPropertyChanged
    {
        public override string Name => nameof(Miscellaneous);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Miscellaneous).ToLower()}.bsml";

        public event PropertyChangedEventHandler PropertyChanged;

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("fixate-rotation")]
        protected bool FixateRotation
        {
            get => this._config.FixateRotationToZero;
            set => this._config.FixateRotationToZero = value;
        }

        [UIValue("fixate-zpos")]
        protected bool FixateZPos
        {
            get => this._config.FixateZPos;
            set => this._config.FixateZPos = value;
        }

        [UIValue("do-fixed-lifetime")]
        protected bool DoFixedLifetime
        {
            get => this._config.FixedLifetime;
            set => this._config.FixedLifetime = value;
        }

        [UIValue("fixed-lifetime")]
        protected float FixedLifetime
        {
            get => this._config.FixedLifetimeLength;
            set => this._config.FixedLifetimeLength = value;
        }

        [UIValue("do-grid-snap")]
        protected bool DoGridSnap
        {
            get => this._config.SnapToGrid;
            set => this._config.SnapToGrid = value;
        }

        [UIValue("grid-scale")]
        protected float GridScale
        {
            get => this._config.GridScale;
            set => this._config.GridScale = value;
        }

        [UIAction("hidden-prop-change")]
        protected async Task HiddenPropertyChanged(bool _)
        {
            await Utilities.AwaitSleep(1);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DoGridSnap)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DoFixedLifetime)));
        }

        [UIAction("time-formatter")]
        protected string TimeFormatter(float value)
        {
            return $"{value:0.00} seconds";
        }
    }
}
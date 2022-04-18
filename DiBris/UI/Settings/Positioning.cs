using BeatSaberMarkupLanguage.Attributes;
using Zenject;

namespace DiBris.UI.Settings
{
    internal class Positioning : Parseable
    {
        public override string Name => nameof(Positioning);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Positioning).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("pos-offset-x")]
        protected float PosOffsetX
        {
            get => this._config.AbsolutePositionOffset.x;
            set => this._config.AbsolutePositionOffsetX = value;
        }

        [UIValue("pos-offset-y")]
        protected float PosOffsetY
        {
            get => this._config.AbsolutePositionOffset.y;
            set => this._config.AbsolutePositionOffsetY = value;
        }

        [UIValue("pos-offset-z")]
        protected float PosOffsetZ
        {
            get => this._config.AbsolutePositionOffset.z;
            set => this._config.AbsolutePositionOffsetZ = value;
        }

        [UIValue("pos-scale")]
        protected float PosScale
        {
            get => this._config.AbsolutePositionScale;
            set => this._config.AbsolutePositionScale = value;
        }

        [UIAction("percent-formatter")]
        protected string PercentFormatter(float value)
        {
            return value.ToString("P2");
        }

        [UIAction("length-formatter")]
        protected string LengthFormatter(float value)
        {
            return $"{value:N} m";
        }
    }
}
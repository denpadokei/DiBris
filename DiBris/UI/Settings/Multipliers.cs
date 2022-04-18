using BeatSaberMarkupLanguage.Attributes;
using Zenject;

namespace DiBris.UI.Settings
{
    internal class Multipliers : Parseable
    {
        public override string Name => nameof(Multipliers);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Multipliers).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("lifetime")]
        protected float Lifetime
        {
            get => this._config.LifetimeMultiplier;
            set => this._config.LifetimeMultiplier = value;
        }

        [UIValue("velocity")]
        protected float Velocity
        {
            get => this._config.VelocityMultiplier;
            set => this._config.VelocityMultiplier = value;
        }

        [UIValue("gravity")]
        protected float Gravity
        {
            get => this._config.GravityMultiplier;
            set => this._config.GravityMultiplier = value;
        }

        [UIValue("rotation")]
        protected float Rotation
        {
            get => this._config.RotationMultiplier;
            set => this._config.RotationMultiplier = value;
        }

        [UIValue("scale")]
        protected float Scale
        {
            get => this._config.Scale;
            set => this._config.Scale = value;
        }

        [UIAction("percent-formatter")]
        protected string PercentFormatter(float value)
        {
            return value.ToString("P2");
        }

        [UIAction("multiplier-formatter")]
        protected string MultiplierFormatter(float value)
        {
            return $"{value:N}x";
        }
    }
}
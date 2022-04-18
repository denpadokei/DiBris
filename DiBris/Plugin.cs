using DiBris.Components;
using DiBris.Managers;
using DiBris.UI;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Utilities;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Zenject;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace DiBris
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        internal static readonly FieldAccessor<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>.Accessor DebrisSpawner = FieldAccessor<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>.GetAccessor("_noteDebrisSpawner");

        [Init]
        public Plugin(Conf conf, IPALogger log, Zenjector zenjector, PluginMetadata metadata)
        {
            var config = conf.Generated<Config>();
            config.Version = metadata.HVersion;
            zenjector.UseLogger(log);
            zenjector.Install(Location.App, Container =>
            {
                Container.BindInstance(metadata).WithId(nameof(DiBris)).AsCached();
                Container.Bind<ProfileManager>().AsSingle();
                Container.BindInstance(config).AsSingle();
            });

            zenjector.Install(Location.Menu, Container =>
            {
                Container.Bind<UIParser>().AsSingle();
                Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
                Container.Bind<BriMainView>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<BriInfoView>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<BriProfileView>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<BriSettingsView>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<BriFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            });

            zenjector.Install(Location.Player, Container =>
            {
                Container.BindInterfacesAndSelfTo<DiSpawner>().AsSingle();
            });
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
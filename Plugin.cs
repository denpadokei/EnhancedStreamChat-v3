using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace EnhancedStreamChat
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        private static PluginConfig s_pluginConfig;
        internal static string Name => "EnhancedStreamChat";
        internal static string Version => s_meta.HVersion.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static PluginMetadata s_meta;
        [Init]
        public void Init(IPALogger logger, PluginMetadata meta, Config config, Zenjector zenjector)
        {
            Instance = this;
            s_meta = meta;
            Logger.Log = logger;
            Logger.Log.Debug("Logger initialized.");
            s_pluginConfig = config.Generated<PluginConfig>();
            zenjector.Install(Location.App, container =>
            {
                container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(s_pluginConfig);
            });
            zenjector.Install<ESCAppInstaller>(Location.App);
            zenjector.Install<ESCMenuInstaller>(Location.Menu);
        }
        [OnStart]
        public void OnStart()
        {
        }

        [OnExit]
        public void OnExit()
        {

        }
    }
}

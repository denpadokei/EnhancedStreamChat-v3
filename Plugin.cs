using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Installers;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace EnhancedStreamChat
{
    [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "MessageReceivedHandler")]
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        private static PluginConfig s_pluginConfig;
        internal static string Name => "EnhancedStreamChat";
        internal static string Version => s_meta.HVersion.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static PluginMetadata s_meta;
        private static Harmony s_harmony;
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

        [OnEnable]
        public void OnEnable()
        {
            try {
                s_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception e) {
                Logger.Error(e);
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            try {
                if (s_harmony != null) {
                    s_harmony.UnpatchSelf();
                    s_harmony = null;
                }
            }
            catch (System.Exception e) {
                Logger.Error(e);
            }
        }
        [OnStart]
        public void OnStart()
        {
        }

        [OnExit]
        public void OnExit()
        {

        }

        [HarmonyPrefix]
        public static void Prefix(ref string message)
        {
            Logger.Info(message);
        }
    }
}

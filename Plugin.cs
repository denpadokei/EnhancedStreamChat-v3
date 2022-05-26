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
#if DEBUG
    [HarmonyPatch]
#endif
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        private static PluginConfig s_pluginConfig;
        internal static string Name => "EnhancedStreamChat";
        internal static string Version => s_meta.HVersion.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static PluginMetadata s_meta;
        private static Harmony s_harmony;
#if DEBUG
        //private static readonly string[] s_twitchAuthorizationScope =
        //{
        //    "channel:moderate",
        //    "chat:edit",
        //    "chat:read",
        //    "bits:read",
        //    "user:read:follows",
        //    "channel:manage:broadcast",
        //    "channel:manage:polls",
        //    "channel:manage:predictions",
        //    "channel:manage:redemptions",
        //    "channel:read:subscriptions",
        //    "channel:read:redemptions",
        //    "channel:read:hype_train",
        //    "channel:read:predictions",
        //    "user:edit:broadcast"
        //};
#endif
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
#if DEBUG
        [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "MessageReceivedHandler")]
        [HarmonyPrefix]
        public static void MessageReceivedHandlerPrefix(ref string message)
        {
            try {
                if (message.Contains(@"JOIN #")) {
                    return;
                }
                Logger.Info(message);
            }
            catch (System.Exception r) {
                Logger.Error(r);
            }
        }

        [HarmonyPatch("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent, CatCore", "MessageReceivedHandler")]
        [HarmonyPrefix]
        public static void HandleMessageTypeInternalPrefix(ref string receivedMessage)
        {
            try {
                Logger.Info(receivedMessage);
            }
            catch (System.Exception r) {
                Logger.Error(r);
            }
        }

        [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "HandleParsedIrcMessage")]
        [HarmonyPrefix]
        public static void HandleParsedIrcMessagePrefix(ref string commandType, ref string prefix)
        {
            try {
                Logger.Info(prefix);
                Logger.Info(commandType);
            }
            catch (System.Exception r) {
                Logger.Error(r);
            }
        }

        //[HarmonyPatch("CatCore.Services.Twitch.TwitchAuthService, CatCore", "AuthorizationUrl")]
        //[HarmonyPrefix]
        //public static void StaticConstractPrefix(ref string[] ____twitchAuthorizationScope)
        //{
        //    Logger.Info("StaticConstractPrefix");
        //    ____twitchAuthorizationScope = s_twitchAuthorizationScope;
        //}
        [HarmonyPatch("CatCore.Services.WebSocketConnection, CatCore", "SendMessage")]
        [HarmonyPostfix]
        public static void SendMessagePostfix(ref string message, ref object ____wss)
        {
            try {
                var isConnected = (bool)____wss.GetType().GetProperty("IsConnected", BindingFlags.Public | BindingFlags.Instance).GetValue(____wss);
                Logger.Info($"{isConnected}, {message}");
            }
            catch (System.Exception r) {
                Logger.Error(r);
            }
        }
#endif
    }

    public enum BuildMessageTarget
    {
        Main,
        Sub
    }
}

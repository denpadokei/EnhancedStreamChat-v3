using EnhancedStreamChat.Chat;
using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Installers;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using System;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace EnhancedStreamChat
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        private static PluginConfig s_pluginConfig;
        internal static string Name => "EnhancedStreamChat";
        internal static string Version => _meta.HVersion.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public const string HARMONY_ID = "EnhancedStreamChat.denpadokei.com.github";
        private Harmony harmony;
        private static PluginMetadata _meta;
        [Init]
        public void Init(IPALogger logger, PluginMetadata meta, Config config, Zenjector zenjector)
        {
            Instance = this;
            _meta = meta;
            Logger.Log = logger;
            Logger.Log.Debug("Logger initialized.");
            s_pluginConfig = config.Generated<Configuration.PluginConfig>();
            zenjector.Install(Location.App, container =>
            {
                container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(s_pluginConfig);
            });
            zenjector.Install<ESCAppInstaller>(Location.App);
            zenjector.Install<ESCMenuAndGameInstaller>(Location.Menu);
            this.harmony = new Harmony(HARMONY_ID);
        }
        [OnStart]
        public void OnStart()
        {
        }

        [OnEnable]
        public void OnEnable()
        {
            this.harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            this.harmony.UnpatchSelf();
        }

        [OnExit]
        public void OnExit()
        {

        }
    }
}

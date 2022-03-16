using EnhancedStreamChat.Chat;
using HarmonyLib;
using IPA;
using IPA.Loader;
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
        internal static string Name => "EnhancedStreamChat";
        internal static string Version => _meta.HVersion.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public const string HARMONY_ID = "EnhancedStreamChat.denpadokei.com.github";
        private Harmony harmony;
        private static PluginMetadata _meta;
        [Init]
        public void Init(IPALogger logger, PluginMetadata meta)
        {
            Instance = this;
            _meta = meta;
            Logger.Log = logger;
            Logger.Log.Debug("Logger initialized.");
            var config = ChatConfig.instance;
            Font.textureRebuilt += this.Font_textureRebuilt;
            this.harmony = new Harmony(HARMONY_ID);
        }
        [OnStart]
        public void OnStart()
        {
            ChatManager.TouchInstance();
            ESCFontManager.TouchInstance();
        }


        private void Font_textureRebuilt(Font obj) => Logger.Log.Debug($"FontTexture({obj.name}) width: {obj.material.mainTexture.width}, height: {obj.material.mainTexture.height}");

        [OnEnable]
        public void OnEnable()
        {
            this.harmony.PatchAll(Assembly.GetExecutingAssembly());
            try {
                ChatManager.instance.enabled = true;
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            this.harmony.UnpatchSelf();
            ChatManager.instance.enabled = false;
        }

        [OnExit]
        public void OnExit() => Font.textureRebuilt -= this.Font_textureRebuilt;
    }
}

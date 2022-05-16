using EnhancedStreamChat.Converters;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EnhancedStreamChat.Configuration
{
    public class PluginConfig
    {
        public virtual bool PreCacheAnimatedEmotes { get; set; } = true;
        public virtual string SystemFontName { get; set; } = "Segoe UI";
        [UseConverter(typeof(ColorConverterWithAlpha))]
        public virtual Color BackgroundColor { get; set; } = ((Color)(Vector4.one * 0.3f)).ColorWithAlpha(1f);
        [UseConverter(typeof(ColorConverterWithAlpha))]
        public virtual Color TextColor { get; set; } = Color.white;
        [UseConverter(typeof(ColorConverterWithAlpha))]
        public virtual Color AccentColor { get; set; } = new Color(0.57f, 0.28f, 1f, 1f);
        [UseConverter(typeof(ColorConverterWithAlpha))]
        public virtual Color HighlightColor { get; set; } = new Color(0.57f, 0.28f, 1f, 0.06f);
        [UseConverter(typeof(ColorConverterWithAlpha))]
        public virtual Color PingColor { get; set; } = new Color(1f, 0f, 0f, 0.13f);
        public virtual int ChatWidth { get; set; } = 120;
        public virtual int ChatHeight { get; set; } = 140;
        public virtual float FontSize { get; set; } = 3.4f;
        public virtual bool AllowMovement { get; set; } = false;
        public virtual bool SyncOrientation { get; set; } = false;
        public virtual bool ReverseChatOrder { get; set; } = false;
        [UseConverter(typeof(Vector3Conveter))]
        public virtual Vector3 Menu_ChatPosition { get; set; } = new Vector3(0, 3.75f, 2.5f);
        [UseConverter(typeof(Vector3Conveter))]
        public virtual Vector3 Menu_ChatRotation { get; set; } = new Vector3(325, 0, 0);
        [UseConverter(typeof(Vector3Conveter))]
        public virtual Vector3 Song_ChatPosition { get; set; } = new Vector3(0, 3.75f, 2.5f);
        [UseConverter(typeof(Vector3Conveter))]
        public virtual Vector3 Song_ChatRotation { get; set; } = new Vector3(325, 0, 0);
        public virtual bool ForceAutoReconnect { get; set; } = true;

        public event Action OnConfigChanged;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
            this.OnConfigChanged?.Invoke();
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}

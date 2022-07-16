using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedStreamChat.Chat
{
    public partial class ChatDisplay : BSMLAutomaticViewController
    {
        private bool SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
                return false;
            }
            oldValue = newValue;
            this.OnPropertyChanged(new PropertyChangedEventArgs(name));
            return true;
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged(e.PropertyName);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            // bg
            this._backgroundColorSetting.editButton.onClick.AddListener(this.HideSettings);
            this._backgroundColorSetting.modalColorPicker.cancelEvent += this.ShowSettings;
            this._backgroundColorSetting.CurrentColor = this._chatConfig.BackgroundColor;
            // accent
            this._accentColorSetting.editButton.onClick.AddListener(this.HideSettings);
            this._accentColorSetting.modalColorPicker.cancelEvent += this.ShowSettings;
            this._accentColorSetting.CurrentColor = this._chatConfig.AccentColor;
            // highlight
            this._highlightColorSetting.editButton.onClick.AddListener(this.HideSettings);
            this._highlightColorSetting.modalColorPicker.cancelEvent += this.ShowSettings;
            this._highlightColorSetting.CurrentColor = this._chatConfig.HighlightColor;
            // ping
            this._pingColorSetting.editButton.onClick.AddListener(this.HideSettings);
            this._pingColorSetting.modalColorPicker.cancelEvent += this.ShowSettings;
            this._pingColorSetting.CurrentColor = this._chatConfig.PingColor;
            // text
            this._textColorSetting.editButton.onClick.AddListener(this.HideSettings);
            this._textColorSetting.modalColorPicker.cancelEvent += this.ShowSettings;
            this._textColorSetting.CurrentColor = this._chatConfig.TextColor;

            // Move interactables in front of the screen
            this.settingsModalGameObject.transform.localPosition = new Vector3(this.settingsModalGameObject.transform.localPosition.x, this.settingsModalGameObject.transform.localPosition.y, -2f);
            this.settingsIconGameObject.transform.localPosition = new Vector3(this.settingsIconGameObject.transform.localPosition.x, this.settingsIconGameObject.transform.localPosition.y, -2f);

            this.settingsIconGameObject.layer = 5;
            this.settingsModalGameObject.layer = 5;
        }

        [UIParams]
        internal BSMLParserParams parserParams;

        [UIObject("settings-icon")]
        internal GameObject settingsIconGameObject;

        [UIObject("settings-modal")]
        internal GameObject settingsModalGameObject;

        [UIComponent("background-color-setting")]
        private readonly ColorSetting _backgroundColorSetting;

        [UIComponent("accent-color-setting")]
        private readonly ColorSetting _accentColorSetting;

        [UIComponent("highlight-color-setting")]
        private readonly ColorSetting _highlightColorSetting;

        [UIComponent("ping-color-setting")]
        private readonly ColorSetting _pingColorSetting;

        [UIComponent("text-color-setting")]
        private readonly ColorSetting _textColorSetting;

        [UIValue("accent-color")]
        public Color AccentColor
        {
            get => this._chatConfig.AccentColor;
            set
            {
                this.SetProperty(ref this._chatConfig.AccentColor, value);
                this.UpdateMessages();
            }
        }

        [UIValue("highlight-color")]
        public Color HighlightColor
        {
            get => this._chatConfig.HighlightColor;
            set
            {
                this.SetProperty(ref this._chatConfig.HighlightColor, value);
                this.UpdateMessages();
            }
        }

        [UIValue("ping-color")]
        public Color PingColor
        {
            get => this._chatConfig.PingColor;
            set
            {
                this.SetProperty(ref this._chatConfig.PingColor, value);
                this.UpdateMessages();
            }
        }

        [UIValue("background-color")]
        public Color BackgroundColor
        {
            get => this._chatConfig.BackgroundColor;
            set
            {
                this.SetProperty(ref this._chatConfig.BackgroundColor, value);
                this._chatScreen.GetComponentsInChildren<ImageView>().FirstOrDefault(x => x.name == "bg").color = value;
            }
        }

        [UIValue("text-color")]
        public Color TextColor
        {
            get => this._chatConfig.TextColor;
            set
            {
                this.SetProperty(ref this._chatConfig.TextColor, value);
                this.UpdateMessages();
            }
        }

        [UIValue("font-size")]
        public float FontSize
        {
            get => this._chatConfig.FontSize;
            set
            {
                this.SetProperty(ref this._chatConfig.FontSize, value);
                this.UpdateMessages();
            }
        }

        private int _settingsWidth = 110;
        [UIValue("settings-width")]
        public int SettingsWidth
        {
            get => this._settingsWidth;
            set => this.SetProperty(ref this._settingsWidth, value);
        }

        [UIValue("chat-width")]
        public int ChatWidth
        {
            get => this._chatConfig.ChatWidth;
            set
            {
                this.SetProperty(ref this._chatConfig.ChatWidth, value);
                this._chatScreen.ScreenSize = new Vector2(this.ChatWidth, this.ChatHeight);
                this._chatContainer.GetComponent<RectMask2D>().rectTransform.sizeDelta = new Vector2(this.ChatWidth, this.ChatHeight);
                this.UpdateMessages();
            }
        }

        [UIValue("chat-height")]
        public int ChatHeight
        {
            get => this._chatConfig.ChatHeight;
            set
            {
                this.SetProperty(ref this._chatConfig.ChatHeight, value);
                this._chatScreen.ScreenSize = new Vector2(this.ChatWidth, this.ChatHeight);
                this._chatContainer.GetComponent<RectMask2D>().rectTransform.sizeDelta = new Vector2(this.ChatWidth, this.ChatHeight);
                this.UpdateMessages();
            }
        }

        [UIValue("chat-position")]
        public Vector3 ChatPosition
        {
            get => this._isInGame ? this._chatConfig.Song_ChatPosition : this._chatConfig.Menu_ChatPosition;
            set
            {
                this._chatScreen.ScreenPosition = value;
                if (this._isInGame || this.SyncOrientation) {
                    this.SetProperty(ref this._chatConfig.Song_ChatPosition, value);
                }

                if (!this._isInGame || this.SyncOrientation) {
                    this.SetProperty(ref this._chatConfig.Menu_ChatPosition, value);
                }
            }
        }

        [UIValue("chat-rotation")]
        public Vector3 ChatRotation
        {
            get => this._isInGame ? this._chatConfig.Song_ChatRotation : this._chatConfig.Menu_ChatRotation;
            set
            {
                this._chatScreen.ScreenRotation = Quaternion.Euler(value);
                if (this._isInGame || this.SyncOrientation) {
                    this.SetProperty(ref this._chatConfig.Song_ChatRotation, value);
                }

                if (!this._isInGame || this.SyncOrientation) {
                    this.SetProperty(ref this._chatConfig.Menu_ChatRotation, value);
                }
            }
        }

        [UIValue("allow-movement")]
        public bool AllowMovement
        {
            get => this._chatConfig.AllowMovement;
            set
            {
                this.SetProperty(ref this._chatConfig.AllowMovement, value);
                this._chatScreen.ShowHandle = value;
            }
        }

        [UIValue("sync-orientation")]
        public bool SyncOrientation
        {
            get => this._chatConfig.SyncOrientation;
            set
            {
                this.SetProperty(ref this._chatConfig.SyncOrientation, value);
                if (value) {
                    this.ChatPosition = this.ChatPosition;
                    this.ChatRotation = this.ChatRotation;
                }
            }
        }

        [UIValue("reverse-chat-order")]
        public bool ReverseChatOrder
        {
            get => this._chatConfig.ReverseChatOrder;
            set
            {
                this.SetProperty(ref this._chatConfig.ReverseChatOrder, value);
                this.UpdateMessages();
            }
        }

        [UIValue("mod-version")]
        public string ModVersion => Plugin.Version;

        [UIAction("launch-web-app")]
        private void LaunchWebApp()
        {
            ChatManager.instance._chatCoreInstance.LaunchWebApp();
        }

        [UIAction("launch-kofi")]
        private void LaunchKofi()
        {
            Application.OpenURL("https://ko-fi.com/brian91292");
        }

        [UIAction("launch-github")]
        private void LaunchGitHub()
        {
            Application.OpenURL("https://github.com/Auros/EnhancedStreamChat-v3");
        }

        [UIAction("on-settings-clicked")]
        private void OnSettingsClick()
        {
            Logger.Info("Settings clicked!");
        }

        [UIAction("#hide-settings")]
        private void OnHideSettings()
        {
            Logger.Info("Saving settings!");
            this._chatConfig.Save();
        }

        private void HideSettings()
        {
            this.parserParams.EmitEvent("hide-settings");
        }

        private void ShowSettings()
        {
            this.parserParams.EmitEvent("show-settings");
        }
    }
}

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

        private void OnPropertyChanged(PropertyChangedEventArgs e) => this.NotifyPropertyChanged(e.PropertyName);

        [UIAction("#post-parse")]
        private void PostParse()
        {
            this.Load();
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

        private Color _accentColor;
        [UIValue("accent-color")]
        public Color AccentColor
        {
            get => this._accentColor;

            set
            {
                this.SetProperty(ref this._accentColor, value);
                this.UpdateMessages();
            }
        }
        private Color _highlightColor;
        [UIValue("highlight-color")]
        public Color HighlightColor
        {
            get => this._highlightColor;
            set
            {
                this.SetProperty(ref this._highlightColor, value);
                this.UpdateMessages();
            }
        }

        private Color _pingColor;
        [UIValue("ping-color")]
        public Color PingColor
        {
            get => this._pingColor;
            set
            {
                this.SetProperty(ref this._pingColor, value);
                this.UpdateMessages();
            }
        }

        private Color _backGroundColor;
        [UIValue("background-color")]
        public Color BackgroundColor
        {
            get => this._backGroundColor;
            set
            {
                this.SetProperty(ref this._backGroundColor, value);
                this._chatScreen.GetComponentsInChildren<ImageView>().FirstOrDefault(x => x.name == "bg").color = value;
            }
        }

        private Color _textColor;
        [UIValue("text-color")]
        public Color TextColor
        {
            get => this._textColor;
            set
            {
                this.SetProperty(ref this._textColor, value);
                this.UpdateMessages();
            }
        }

        private float _fontsize;
        [UIValue("font-size")]
        public float FontSize
        {
            get => this._fontsize;
            set
            {
                this.SetProperty(ref this._fontsize, value);
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

        private int _chatWidth;
        [UIValue("chat-width")]
        public int ChatWidth
        {
            get => this._chatWidth;
            set
            {
                this.SetProperty(ref this._chatWidth, value);
                this._chatScreen.ScreenSize = new Vector2(this.ChatWidth, this.ChatHeight);
                this._chatContainer.GetComponent<RectMask2D>().rectTransform.sizeDelta = new Vector2(this.ChatWidth, this.ChatHeight);
                this.UpdateMessages();
            }
        }

        private int _chatHeight;
        [UIValue("chat-height")]
        public int ChatHeight
        {
            get => this._chatHeight;
            set
            {
                this.SetProperty(ref this._chatHeight, value);
                this._chatScreen.ScreenSize = new Vector2(this.ChatWidth, this.ChatHeight);
                this._chatContainer.GetComponent<RectMask2D>().rectTransform.sizeDelta = new Vector2(this.ChatWidth, this.ChatHeight);
                this.UpdateMessages();
            }
        }

        private Vector3 _chatPosition;
        [UIValue("chat-position")]
        public Vector3 ChatPosition
        {
            get => this._chatPosition;
            set
            {
                this.SetProperty(ref this._chatPosition, value);
                this._chatScreen.ScreenPosition = value;
            }
        }

        private Vector3 _chatRotation;
        [UIValue("chat-rotation")]
        public Vector3 ChatRotation
        {
            get => this._chatRotation;
            set
            {
                this.SetProperty(ref _chatRotation, value);
                this._chatScreen.ScreenRotation = Quaternion.Euler(value);
            }
        }

        private bool _allowMovement;
        [UIValue("allow-movement")]
        public bool AllowMovement
        {
            get => this._allowMovement;
            set
            {
                this.SetProperty(ref this._allowMovement, value);
                this._chatScreen.ShowHandle = value;
            }
        }

        private bool _syncOrientation;
        [UIValue("sync-orientation")]
        public bool SyncOrientation
        {
            get => this._syncOrientation;
            set
            {
                this.SetProperty(ref this._syncOrientation, value);
            }
        }

        private bool _reverseChatOrder;
        [UIValue("reverse-chat-order")]
        public bool ReverseChatOrder
        {
            get => this._reverseChatOrder;
            set
            {
                this.SetProperty(ref this._reverseChatOrder, value);
                this.UpdateMessages();
            }
        }

        [UIValue("mod-version")]
        public string ModVersion => Plugin.Version;

        [UIAction("launch-web-app")]
        private void LaunchWebApp() => this._catCoreManager.LaunchWebPortal();

        [UIAction("launch-kofi")]
        private void LaunchKofi() => Application.OpenURL("https://ko-fi.com/brian91292");

        [UIAction("launch-github")]
        private void LaunchGitHub() => Application.OpenURL("https://github.com/Auros/EnhancedStreamChat-v3");

        [UIAction("on-settings-clicked")]
        private void OnSettingsClick() => Logger.Info("Settings clicked!");

        [UIAction("#hide-settings")]
        private void OnHideSettings()
        {
            Logger.Info("Saving settings!");
            this.Save();
        }

        private void HideSettings() => this.parserParams.EmitEvent("hide-settings");

        private void ShowSettings() => this.parserParams.EmitEvent("show-settings");

        private void Save()
        {
            this._chatConfig.AccentColor = this.AccentColor;
            this._chatConfig.TextColor = this.TextColor;
            this._chatConfig.BackgroundColor = this.BackgroundColor;
            this._chatConfig.AllowMovement = this.AllowMovement;
            this._chatConfig.ChatHeight = this.ChatHeight;
            this._chatConfig.ChatWidth = this.ChatWidth;
            if (this._isInGame) {
                this._chatConfig.Song_ChatPosition = this.ChatPosition;
                this._chatConfig.Song_ChatRotation = this.ChatRotation;
            }
            else {
                this._chatConfig.Menu_ChatPosition = this.ChatPosition;
                this._chatConfig.Menu_ChatRotation = this.ChatRotation;
            }
            this._chatConfig.FontSize = this.FontSize;
            this._chatConfig.HighlightColor = this.HighlightColor;
            this._chatConfig.PingColor = this.PingColor;
            this._chatConfig.ReverseChatOrder = this.ReverseChatOrder;
            this._chatConfig.SyncOrientation = this.SyncOrientation;
            if (this.SyncOrientation) {
                if (this._isInGame) {
                    this._chatConfig.Menu_ChatPosition = this._chatConfig.Song_ChatPosition;
                    this._chatConfig.Menu_ChatRotation = this._chatConfig.Song_ChatRotation;
                }
                else {
                    this._chatConfig.Song_ChatPosition = this._chatConfig.Menu_ChatPosition;
                    this._chatConfig.Song_ChatRotation = this._chatConfig.Menu_ChatRotation;
                }
            }
        }

        private void Load()
        {
            this.AccentColor = this._chatConfig.AccentColor;
            this.TextColor = this._chatConfig.TextColor;
            this.BackgroundColor = this._chatConfig.BackgroundColor;
            this.AllowMovement = this._chatConfig.AllowMovement;
            this.ChatHeight = this._chatConfig.ChatHeight;
            this.ChatWidth = this._chatConfig.ChatWidth;
            if (this._isInGame) {
                this.ChatPosition = this._chatConfig.Song_ChatPosition;
                this.ChatRotation = this._chatConfig.Song_ChatRotation;
            }
            else {
                this.ChatPosition = this._chatConfig.Menu_ChatPosition;
                this.ChatRotation = this._chatConfig.Menu_ChatRotation;
            }
            this.FontSize = this._chatConfig.FontSize;
            this.HighlightColor = this._chatConfig.HighlightColor;
            this.PingColor = this._chatConfig.PingColor;
            this.ReverseChatOrder = this._chatConfig.ReverseChatOrder;
            this.SyncOrientation = this._chatConfig.SyncOrientation;
        }
    }
}

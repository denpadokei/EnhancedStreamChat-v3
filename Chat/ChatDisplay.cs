using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.HarmonyPatches;
using EnhancedStreamChat.Interfaces;
using EnhancedStreamChat.Models;
using EnhancedStreamChat.Utilities;
using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Zenject;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRUIControls;
using Zenject;
using Color = UnityEngine.Color;

namespace EnhancedStreamChat.Chat
{
    [HotReload]
    public partial class ChatDisplay : BSMLAutomaticViewController, IAsyncInitializable, IChatDisplay, IDisposable, IAffinity, ILatePreRenderRebuildReciver, IIrcServiceDisconnectReceiver, IPubSubServiceDisconnectReceiver
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // イベント
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public async Task InitializeAsync(CancellationToken token)
        {
            while (!this._fontManager.IsInitialized) {
                await Task.Yield();
            }
            this.SetupScreens();
            foreach (var msg in this._messages) {
                msg.Text.SetAllDirty();
                if (msg.SubTextEnabled) {
                    msg.SubText.SetAllDirty();
                }
            }
            (this.transform as RectTransform).pivot = new Vector2(0.5f, 0f);
            while (s_backupMessageQueue.TryDequeue(out var msg)) {
                await this.OnTextMessageReceived(msg.Value, msg.Key);
            }
            TwitchIrcServicePatch.RegistIrcReceiver(this);
            TwitchIrcServicePatch.RegistPubSubReceiver(this);
            this._chatConfig.OnConfigChanged += this.Instance_OnConfigChanged;
            SceneManager.activeSceneChanged += this.SceneManager_activeSceneChanged;
            this._catCoreManager.OnChatConnected += this.CatCoreManager_OnChatConnected;
            this._catCoreManager.OnJoinChannel += this.CatCoreManager_OnJoinChannel;
            this._catCoreManager.OnLeaveChannel += this.CatCoreManager_OnLeaveChannel;
            this._catCoreManager.OnTwitchTextMessageReceived += this.CatCoreManager_OnTwitchTextMessageReceived;
            this._catCoreManager.OnMessageDeleted += this.OnCatCoreManager_OnMessageDeleted;
            this._catCoreManager.OnChatCleared += this.OnCatCoreManager_OnChatCleared;
            this._catCoreManager.OnFollow += this.OnCatCoreManager_OnFollow;
            this._catCoreManager.OnRewardRedeemed += this.OnCatCoreManager_OnRewardRedeemed;
        }

        public void OnMessageCleared(string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) {
                return;
            }
            MainThreadInvoker.Invoke(() =>
            {
                foreach (var msg in this._messages.Where(x => x.Text.ChatMessage?.Id == messageId)) {
                    this.ClearMessage(msg);
                }
            });
        }
        public void OnChatCleared(string userId)
        {
            if (string.IsNullOrEmpty(userId)) {
                return;
            }
            MainThreadInvoker.Invoke(() =>
            {
                foreach (var msg in this._messages.Where(x => x.Text.ChatMessage?.Sender?.Id == userId)) {
                    this.ClearMessage(msg);
                }
            });
        }

        public async Task OnTextMessageReceived(IESCChatMessage msg, DateTime dateTime)
        {
            var main = await this._chatMessageBuilder.BuildMessage(msg, this._fontManager.FontInfo, BuildMessageTarget.Main);
            var sub = await this._chatMessageBuilder.BuildMessage(msg, this._fontManager.FontInfo, BuildMessageTarget.Sub);
            _ = MainThreadInvoker.Invoke(() => this.CreateMessage(msg, dateTime, main, sub));
        }

        [AffinityPatch(typeof(VRPointer), nameof(VRPointer.OnEnable))]
        [AffinityPostfix]
        public void VRPointerOnEnable(VRPointer __instance)
        {
            this.PointerOnEnabled(__instance);
        }

        public void OnIrcDisconnect(object ircService)
        {
            if (!this._chatConfig.ForceAutoReconnect || !this.ReconnectEnable) {
                return;
            }
            this.ReIrcConnect();
        }

        public void OnPubsubDisconnect(object pubSubService)
        {
            if (!this._chatConfig.ForceAutoReconnect || !this.ReconnectEnable) {
                return;
            }
            this.PubSubReconnect(pubSubService);
        }

        public void LatePreRenderRebuildHandler(object sender, EventArgs e)
        {
            this._updateMessagePositions = true;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void AddMessage(EnhancedTextMeshProUGUIWithBackground newMsg)
        {
            newMsg.AddReciver(this);
            this.UpdateMessage(newMsg, true);
            this._messages.Enqueue(newMsg);
            this.ClearOldMessages();
        }
        private void PointerOnEnabled(VRPointer obj)
        {
            if (obj == null || this._chatScreen == null) {
                return;
            }
            try {
                var mover = this._chatScreen.gameObject.GetComponent<FloatingScreenMoverPointer>();
                if (!mover) {
                    mover = this._chatScreen.gameObject.AddComponent<FloatingScreenMoverPointer>();
                    Destroy(this._chatScreen.screenMover);
                }
                this._chatScreen.screenMover = mover;
                this._chatScreen.screenMover.Init(this._chatScreen, obj);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }
        private void SetupScreens()
        {
            if (this._chatScreen == null) {
                var screenSize = new Vector2(this.ChatWidth, this.ChatHeight);
                this._chatScreen = FloatingScreen.CreateFloatingScreen(screenSize, true, this.ChatPosition, Quaternion.identity, 0f, true);
                this._chatScreen.gameObject.layer = 5;
                var rectMask2D = this._chatScreen.GetComponent<RectMask2D>();
                if (rectMask2D) {
                    Destroy(rectMask2D);
                }

                this._chatContainer.transform.SetParent(this._chatScreen.transform, false);
                this._chatContainer.AddComponent<RectMask2D>().rectTransform.sizeDelta = screenSize;

                var canvas = this._chatScreen.GetComponent<Canvas>();
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 3;

                this._chatScreen.SetRootViewController(this, AnimationType.None);

                this._chatMoverMaterial = Instantiate(BeatSaberUtils.UINoGlowMaterial);
                this._chatMoverMaterial.color = Color.clear;

                var renderer = this._chatScreen.handle.gameObject.GetComponent<Renderer>();
                renderer.material = this._chatMoverMaterial;
                renderer.material.mainTexture = this._chatMoverMaterial.mainTexture;

                this._chatScreen.transform.SetParent(this._rootGameObject.transform);
                DontDestroyOnLoad(this._rootGameObject);
                this._chatScreen.ScreenRotation = Quaternion.Euler(this.ChatRotation);

                this._bg = this._chatScreen.GetComponentsInChildren<ImageView>().FirstOrDefault(x => x.name == "bg");
                this._bg.raycastTarget = false;
                this._bg.material = Instantiate(this._bg.material);
                this._bg.SetField("_gradient", false);
                this._bg.material.color = Color.white.ColorWithAlpha(1);
                this._bg.color = this.BackgroundColor;
                this._bg.SetAllDirty();
                this.AddToVRPointer();
                this.UpdateChatUI();
            }
        }

        private void Instance_OnConfigChanged()
        {
            this.UpdateChatUI();
        }

        private void OnHandleReleased(object sender, FloatingScreenHandleEventArgs e)
        {
            this.FloatingScreenOnRelease(e.Position, e.Rotation);
        }

        private void FloatingScreenOnRelease(in Vector3 pos, in Quaternion rot)
        {
            if (this._isInGame) {
                this._chatConfig.Song_ChatPosition = pos;
                this._chatConfig.Song_ChatRotation = rot.eulerAngles;
            }
            else {
                this._chatConfig.Menu_ChatPosition = pos;
                this._chatConfig.Menu_ChatRotation = rot.eulerAngles;
            }
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name != s_game && arg1.name != s_menu) {
                return;
            }
            if (arg1.name == s_game) {
                this._isInGame = true;
                foreach (var canvas in this._chatScreen.GetComponentsInChildren<Canvas>(true)) {
                    canvas.sortingOrder = 0;
                }
            }
            else if (arg1.name == s_menu) {
                this._isInGame = false;
                foreach (var canvas in this._chatScreen.GetComponentsInChildren<Canvas>(true)) {
                    canvas.sortingOrder = 3;
                }
            }
            this.AddToVRPointer();
            this.UpdateChatUI();
        }

        private void AddToVRPointer()
        {
            if (this._chatScreen.screenMover) {
                this._chatScreen.HandleReleased -= this.OnHandleReleased;
                this._chatScreen.HandleReleased += this.OnHandleReleased;
                this._chatScreen.screenMover.transform.SetAsFirstSibling();
            }
        }
        private void UpdateMessagePositions()
        {
            // TODO: Remove later on
            //float msgPos =  (ReverseChatOrder ?  ChatHeight : 0);
            float? msgPos = this.ChatHeight / (this.ReverseChatOrder ? 2f : -2f);
            foreach (var chatMsg in this._messages.OrderBy(x => x.ReceivedDate).Reverse()) {
                if (chatMsg == null) {
                    continue;
                }
                var msgHeight = (chatMsg.transform as RectTransform)?.sizeDelta.y;
                if (this.ReverseChatOrder) {
                    msgPos -= msgHeight;
                }
                chatMsg.transform.localPosition = new Vector3(0, msgPos ?? 0);
                if (!this.ReverseChatOrder) {
                    msgPos += msgHeight;
                }
            }
        }

        private void UpdateChatUI()
        {
            this.ChatWidth = this._chatConfig.ChatWidth;
            this.ChatHeight = this._chatConfig.ChatHeight;
            this.FontSize = this._chatConfig.FontSize;
            this.AccentColor = this._chatConfig.AccentColor;
            this.HighlightColor = this._chatConfig.HighlightColor;
            this.BackgroundColor = this._chatConfig.BackgroundColor;
            this.PingColor = this._chatConfig.PingColor;
            this.TextColor = this._chatConfig.TextColor;
            this.ReverseChatOrder = this._chatConfig.ReverseChatOrder;
            if (this._isInGame) {
                this.ChatPosition = this._chatConfig.Song_ChatPosition;
                this.ChatRotation = this._chatConfig.Song_ChatRotation;
            }
            else {
                this.ChatPosition = this._chatConfig.Menu_ChatPosition;
                this.ChatRotation = this._chatConfig.Menu_ChatRotation;
            }
            var chatContainerTransform = this._chatContainer.GetComponent<RectMask2D>().rectTransform!;
            chatContainerTransform.sizeDelta = new Vector2(this.ChatWidth, this.ChatHeight);

            this._chatScreen.handle.transform.localScale = new Vector3(this.ChatWidth, this.ChatHeight * 0.9f, 0.01f);
            this._chatScreen.handle.transform.localPosition = Vector3.zero;
            this._chatScreen.handle.transform.localRotation = Quaternion.identity;

            this.AllowMovement = this._chatConfig.AllowMovement;
            this.UpdateMessages();
        }

        private void UpdateMessages()
        {
            foreach (var msg in this._messages) {
                this.UpdateMessage(msg);
            }
            this._updateMessagePositions = true;
        }

        private void UpdateMessage(EnhancedTextMeshProUGUIWithBackground msg, bool setAllDirty = false)
        {
            (msg.transform as RectTransform).sizeDelta = new Vector2(this.ChatWidth, (msg.transform as RectTransform).sizeDelta.y);
            msg.Text.font = this._fontManager.MainFont;
            msg.Text.font.fallbackFontAssetTable = this._fontManager.FallBackFonts;
            msg.Text.overflowMode = TextOverflowModes.Overflow;
            msg.Text.alignment = TextAlignmentOptions.BottomLeft;
            msg.Text.color = this.TextColor;
            msg.Text.fontSize = this.FontSize;
            msg.Text.lineSpacing = 1.5f;

            msg.SubText.font = this._fontManager.MainFont;
            msg.SubText.font.fallbackFontAssetTable = this._fontManager.FallBackFonts;
            msg.SubText.overflowMode = TextOverflowModes.Overflow;
            msg.SubText.alignment = TextAlignmentOptions.BottomLeft;
            msg.SubText.color = this.TextColor;
            msg.SubText.fontSize = this.FontSize;
            msg.SubText.lineSpacing = 1.5f;

            if (msg.Text.ChatMessage != null) {
                msg.HighlightColor = this.HighlightColor;
                msg.AccentColor = this.AccentColor;
                msg.HighlightEnabled = msg.Text.ChatMessage.IsHighlighted;
                msg.AccentEnabled = msg.HighlightEnabled;
            }

            if (setAllDirty) {
                msg.Text.SetAllDirty();
                if (msg.SubTextEnabled) {
                    msg.SubText.SetAllDirty();
                }
            }
        }
        private void ClearOldMessages()
        {
            while (this._messages.TryPeek(out var msg) && this.ReverseChatOrder ? msg.transform.localPosition.y < -this._chatConfig.ChatHeight : msg.transform.localPosition.y >= this._chatConfig.ChatHeight) {
                if (this._messages.TryDequeue(out msg)) {
                    msg.RemoveReciver(this);
                    this._textPoolContaner.Despawn(msg);
                }
            }
        }

        private string BuildClearedMessage(EnhancedTextMeshProUGUI msg)
        {
            var nameColorCode = msg.ChatMessage.Sender.Color;
            if (ColorUtility.TryParseHtmlString(msg.ChatMessage.Sender.Color.Substring(0, 7), out var nameColor)) {
                Color.RGBToHSV(nameColor, out var h, out var s, out var v);
                if (v < 0.85f) {
                    v = 0.85f;
                    nameColor = Color.HSVToRGB(h, s, v);
                }
                nameColorCode = ColorUtility.ToHtmlStringRGB(nameColor);
                nameColorCode = nameColorCode.Insert(0, "#");
            }
            var sb = new StringBuilder($"<color={nameColorCode}>{msg.ChatMessage.Sender.DisplayName}</color>");
            var badgeEndIndex = msg.text.IndexOf("<color=");
            if (badgeEndIndex != -1) {
                sb.Insert(0, msg.text.Substring(0, badgeEndIndex));
            }
            sb.Append(": <color=#bbbbbbff><message deleted></color>");
            return sb.ToString();
        }

        private void ClearMessage(EnhancedTextMeshProUGUIWithBackground msg)
        {
            // Only clear non-system messages
            if (msg.SubTextEnabled) {
                msg.SubText.text = this.BuildClearedMessage(msg.SubText);
            }
            msg.Text.text = this.BuildClearedMessage(msg.Text);
            msg.SubTextEnabled = false;
        }
        private void CreateMessage(IESCChatMessage msg, DateTime date, string mainMessage, string subMassage)
        {
            MainThreadInvoker.Invoke(() =>
            {
                var newMsg = this._textPoolContaner.Spawn();
                newMsg.transform.SetParent(this._chatContainer.transform, false);
                newMsg.Text.ChatMessage = msg;
                newMsg.Text.text = mainMessage;
                // If the last message received had the same id and isn't a system message, then this was a sub-message of the original and may need to be highlighted along with the original message
                newMsg.SubText.text = subMassage;
                newMsg.SubText.ChatMessage = msg;
                newMsg.SubTextEnabled = !string.IsNullOrEmpty(msg.SubMessage);
                newMsg.ReceivedDate = date;
                this.AddMessage(newMsg);
                this._updateMessagePositions = true;
            });
        }

        private void CatCoreManager_OnChatConnected(CatCore.Services.Multiplexer.MultiplexedPlatformService obj)
        {
            var mes = new ESCChatMessage(Guid.NewGuid().ToString(), $"Success connected service. {obj.GetType().Name}")
            {
                IsSystemMessage = true,
                IsHighlighted = false,
            };
            _ = this.OnTextMessageReceived(mes, DateTime.Now);
        }

        private void CatCoreManager_OnJoinChannel(CatCore.Services.Multiplexer.MultiplexedPlatformService arg1, CatCore.Services.Multiplexer.MultiplexedChannel arg2)
        {
            var mes = new ESCChatMessage(Guid.NewGuid().ToString(), $"[{arg2.Name}] Success joining channel {arg2.Id}")
            {
                IsSystemMessage = true,
                IsHighlighted = false,
            };
            _ = this.OnTextMessageReceived(mes, DateTime.Now);
        }

        private void CatCoreManager_OnLeaveChannel(CatCore.Services.Multiplexer.MultiplexedPlatformService arg1, CatCore.Services.Multiplexer.MultiplexedChannel arg2)
        {
            var mes = new ESCChatMessage(Guid.NewGuid().ToString(), $"[{arg2.Name}] Success leaved channel {arg2.Id}")
            {
                IsSystemMessage = true,
                IsHighlighted = false,
            };
            _ = this.OnTextMessageReceived(mes, DateTime.Now);
        }

        private void CatCoreManager_OnTwitchTextMessageReceived(CatCore.Services.Twitch.Interfaces.ITwitchService arg1, CatCore.Models.Twitch.IRC.TwitchMessage arg2)
        {
            _ = this.OnTextMessageReceived(new ESCChatMessage(arg2), DateTime.Now);
        }

        private void OnCatCoreManager_OnMessageDeleted(CatCore.Services.Multiplexer.MultiplexedPlatformService arg1, CatCore.Services.Multiplexer.MultiplexedChannel arg2, string arg3)
        {
            this.OnMessageCleared(arg3);
        }

        private void OnCatCoreManager_OnChatCleared(CatCore.Services.Multiplexer.MultiplexedPlatformService arg1, CatCore.Services.Multiplexer.MultiplexedChannel arg2, string arg3)
        {
            this.OnChatCleared(arg3);
        }

        private void OnCatCoreManager_OnFollow(string channelId, in CatCore.Models.Twitch.PubSub.Responses.Follow data)
        {
            var mes = new ESCChatMessage(Guid.NewGuid().ToString(), $"Thank you for following {data.DisplayName}({data.Username})!")
            {
                IsSystemMessage = true,
                IsHighlighted = true,
            };
            _ = this.OnTextMessageReceived(mes, DateTime.Now);
        }
        private void OnCatCoreManager_OnRewardRedeemed(string channelId, in CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1.RewardRedeemedData data)
        {
            var mes = new ESCChatMessage(Guid.NewGuid().ToString(), $"{data.User.DisplayName} used points {data.Reward.Title}({data.Reward.Cost}).")
            {
                IsSystemMessage = true,
                IsHighlighted = true
            };
            _ = this.OnTextMessageReceived(mes, DateTime.Now);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private readonly ConcurrentQueue<EnhancedTextMeshProUGUIWithBackground> _messages = new ConcurrentQueue<EnhancedTextMeshProUGUIWithBackground>();
        private PluginConfig _chatConfig;
        private bool _isInGame;
        // TODO: eventually figure out a way to make this more modular incase we want to create multiple instances of ChatDisplay
        private static readonly ConcurrentQueue<KeyValuePair<DateTime, IESCChatMessage>> s_backupMessageQueue = new ConcurrentQueue<KeyValuePair<DateTime, IESCChatMessage>>();
        private FloatingScreen _chatScreen;
        private readonly GameObject _chatContainer = new GameObject("chatContainer");
        private readonly GameObject _rootGameObject = new GameObject();
        private Material _chatMoverMaterial;
        private ImageView _bg;
        private bool _updateMessagePositions = false;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private MemoryPoolContainer<EnhancedTextMeshProUGUIWithBackground> _textPoolContaner;
        private ICatCoreManager _catCoreManager;
        private ChatMessageBuilder _chatMessageBuilder;
        private ESCFontManager _fontManager;
        private bool _disposedValue;
        private static readonly string s_menu = "MainMenu";
        private static readonly string s_game = "GameCore";
        private static readonly int s_reconnectDelay = 500;
        private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public void Constarct(EnhancedTextMeshProUGUIWithBackground.Pool pool, PluginConfig config, ICatCoreManager catCoreManager, ChatMessageBuilder chatMessageBuilder, ESCFontManager fontManager)
        {
            this._textPoolContaner = new MemoryPoolContainer<EnhancedTextMeshProUGUIWithBackground>(pool);
            this._chatConfig = config;
            this._catCoreManager = catCoreManager;
            this._chatMessageBuilder = chatMessageBuilder;
            this._fontManager = fontManager;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    try {
                        this._connectSemaphore.Dispose();
                        if (this._rootGameObject != null) {
                            Destroy(this._rootGameObject);
                        }
                    }
                    catch (Exception e) {
                        Logger.Error(e);
                    }
                }
                this._disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // Unity message
        protected void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        protected override void OnDestroy()
        {
            Logger.Debug("OnDestroy()");
            base.OnDestroy();
            this._chatConfig.OnConfigChanged -= this.Instance_OnConfigChanged;
            SceneManager.activeSceneChanged -= this.SceneManager_activeSceneChanged;
            this._catCoreManager.OnChatConnected -= this.CatCoreManager_OnChatConnected;
            this._catCoreManager.OnJoinChannel -= this.CatCoreManager_OnJoinChannel;
            this._catCoreManager.OnLeaveChannel -= this.CatCoreManager_OnLeaveChannel;
            this._catCoreManager.OnTwitchTextMessageReceived -= this.CatCoreManager_OnTwitchTextMessageReceived;
            this._catCoreManager.OnMessageDeleted -= this.OnCatCoreManager_OnMessageDeleted;
            this._catCoreManager.OnChatCleared -= this.OnCatCoreManager_OnChatCleared;
            this._catCoreManager.OnFollow -= this.OnCatCoreManager_OnFollow;
            this._catCoreManager.OnRewardRedeemed -= this.OnCatCoreManager_OnRewardRedeemed;
            TwitchIrcServicePatch.UnRegistIrcReceiver(this);
            TwitchIrcServicePatch.UnRegistPubSubReceiver(this);
            this.StopAllCoroutines();
            while (this._messages.TryDequeue(out var msg)) {
                if (msg != null) {
                    msg.RemoveReciver(this);
                }
                if (msg.Text.ChatMessage != null) {
                    s_backupMessageQueue.Enqueue(new KeyValuePair<DateTime, IESCChatMessage>(msg.ReceivedDate, msg.Text.ChatMessage));
                }
                this._textPoolContaner?.Despawn(msg);
            }

            if (this._chatScreen != null) {
                Destroy(this._chatScreen);
                this._chatScreen = null;
            }
            if (this._chatMoverMaterial != null) {
                Destroy(this._chatMoverMaterial);
                this._chatMoverMaterial = null;
            }
            if (this._bg != null) {
                Destroy(this._bg.material);
            }
        }

        protected void Update()
        {
            if (!this._updateMessagePositions) {
                return;
            }
            this.UpdateMessagePositions();
            this._updateMessagePositions = false;
        }
        #endregion
    }
}
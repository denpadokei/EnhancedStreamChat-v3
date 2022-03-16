using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using ChatCore.Interfaces;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.HarmonyPatches;
using EnhancedStreamChat.Utilities;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Color = UnityEngine.Color;

namespace EnhancedStreamChat.Chat
{
    [HotReload]
    public partial class ChatDisplay : BSMLAutomaticViewController
    {
        public ObjectMemoryComponentPool<EnhancedTextMeshProUGUIWithBackground> TextPool { get; internal set; }
        private readonly ConcurrentQueue<EnhancedTextMeshProUGUIWithBackground> _messages = new ConcurrentQueue<EnhancedTextMeshProUGUIWithBackground>();
        private ChatConfig _chatConfig;

        private bool _isInGame;

        private void Awake()
        {
            this._waitForEndOfFrame = new WaitForEndOfFrame();
            VRPointerOnEnablePatch.OnEnabled += this.PointerOnEnabled;
        }

        private void PointerOnEnabled(VRPointer obj)
        {
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

        // TODO: eventually figure out a way to make this more modular incase we want to create multiple instances of ChatDisplay
        private static readonly ConcurrentQueue<KeyValuePair<DateTime, IChatMessage>> _backupMessageQueue = new ConcurrentQueue<KeyValuePair<DateTime, IChatMessage>>();
        protected override void OnDestroy()
        {
            ChatConfig.instance.OnConfigChanged -= this.Instance_OnConfigChanged;
            BSEvents.menuSceneActive -= this.BSEvents_menuSceneActive;
            BSEvents.gameSceneActive -= this.BSEvents_gameSceneActive;
            VRPointerOnEnablePatch.OnEnabled -= this.PointerOnEnabled;
            this.StopAllCoroutines();
            while (this._messages.TryDequeue(out var msg)) {
                msg.OnLatePreRenderRebuildComplete -= this.OnRenderRebuildComplete;
                if (msg.Text.ChatMessage != null) {
                    _backupMessageQueue.Enqueue(new KeyValuePair<DateTime, IChatMessage>(msg.ReceivedDate, msg.Text.ChatMessage));
                }
                if (msg.SubText.ChatMessage != null) {
                    _backupMessageQueue.Enqueue(new KeyValuePair<DateTime, IChatMessage>(msg.ReceivedDate, msg.SubText.ChatMessage));
                }
                Destroy(msg);
            }
            //_messages.Clear();
            Destroy(this._rootGameObject);
            if (this.TextPool != null) {
                this.TextPool.Dispose();
                this.TextPool = null;
            }
            if (this._chatScreen != null) {
                Destroy(this._chatScreen);
                this._chatScreen = null;
            }
            if (this._chatMoverMaterial != null) {
                Destroy(this._chatMoverMaterial);
                this._chatMoverMaterial = null;
            }
            base.OnDestroy();
        }

        private void Update()
        {
            if (!this._updateMessagePositions) {
                return;
            }
            HMMainThreadDispatcher.instance.Enqueue(this.UpdateMessagePositions());
            this._updateMessagePositions = false;
        }

        private FloatingScreen _chatScreen;
        private GameObject _chatContainer;
        private GameObject _rootGameObject;
        private Material _chatMoverMaterial;
        private ImageView _bg;

        private IEnumerator Start()
        {
            DontDestroyOnLoad(this.gameObject);
            this._chatConfig = ChatConfig.instance;
            yield return new WaitWhile(() => !ESCFontManager.instance.IsInitialized);
            this.SetupScreens();
            foreach (var msg in this._messages.ToArray()) {
                msg.Text.SetAllDirty();
                if (msg.SubTextEnabled) {
                    msg.SubText.SetAllDirty();
                }
            }
            (this.transform as RectTransform).pivot = new Vector2(0.5f, 0f);
            this.TextPool = new ObjectMemoryComponentPool<EnhancedTextMeshProUGUIWithBackground>(64,
                constructor: () =>
                {
                    var go = new GameObject(nameof(EnhancedTextMeshProUGUIWithBackground), typeof(EnhancedTextMeshProUGUIWithBackground));
                    DontDestroyOnLoad(go);
                    go.SetActive(false);
                    var msg = go.GetComponent<EnhancedTextMeshProUGUIWithBackground>();
                    msg.Text.enableWordWrapping = true;
                    msg.Text.autoSizeTextContainer = false;
                    msg.SubText.enableWordWrapping = true;
                    msg.SubText.autoSizeTextContainer = false;
                    (msg.transform as RectTransform).pivot = new Vector2(0.5f, 0);
                    msg.transform.SetParent(this._chatContainer.transform, false);
                    this.UpdateMessage(msg);
                    return msg;
                },
                onFree: (msg) =>
                {
                    try {
                        msg.gameObject.SetActive(false);
                        (msg.transform as RectTransform).localPosition = Vector3.zero;
                        msg.OnLatePreRenderRebuildComplete -= this.OnRenderRebuildComplete;
                        msg.HighlightEnabled = false;
                        msg.AccentEnabled = false;
                        msg.SubTextEnabled = false;
                        msg.Text.text = "";
                        msg.Text.ChatMessage = null;
                        msg.SubText.text = "";
                        msg.SubText.ChatMessage = null;
                        msg.Text.ClearImages();
                        msg.SubText.ClearImages();
                    }
                    catch (Exception ex) {
                        Logger.Error($"An exception occurred while trying to free CustomText object. {ex.ToString()}");
                    }
                }
            );
            ChatConfig.instance.OnConfigChanged += this.Instance_OnConfigChanged;
            BSEvents.menuSceneActive += this.BSEvents_menuSceneActive;
            BSEvents.gameSceneActive += this.BSEvents_gameSceneActive;

            yield return new WaitWhile(() => this._chatScreen == null);
            while (_backupMessageQueue.TryDequeue(out var msg)) {
                var task = this.OnTextMessageReceived(msg.Value, msg.Key).GetAwaiter();
                yield return new WaitWhile(() => !task.IsCompleted);
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

                this._chatContainer = new GameObject("chatContainer");
                this._chatContainer.transform.SetParent(this._chatScreen.transform, false);
                this._chatContainer.AddComponent<RectMask2D>().rectTransform.sizeDelta = screenSize;

                var canvas = this._chatScreen.GetComponent<Canvas>();
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 3;

                this._chatScreen.SetRootViewController(this, AnimationType.None);
                this._rootGameObject = new GameObject();
                DontDestroyOnLoad(this._rootGameObject);

                this._chatMoverMaterial = Instantiate(BeatSaberUtils.UINoGlowMaterial);
                this._chatMoverMaterial.color = Color.clear;

                var renderer = this._chatScreen.handle.gameObject.GetComponent<Renderer>();
                renderer.material = this._chatMoverMaterial;
                renderer.material.mainTexture = this._chatMoverMaterial.mainTexture;

                this._chatScreen.transform.SetParent(this._rootGameObject.transform);
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

        private void Instance_OnConfigChanged(ChatConfig obj) => this.UpdateChatUI();

        private void OnHandleReleased(object sender, FloatingScreenHandleEventArgs e) => this.FloatingScreenOnRelease(e.Position, e.Rotation);

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
            this._chatConfig.Save();
        }

        private void BSEvents_gameSceneActive()
        {
            this._isInGame = true;
            foreach (var canvas in this._chatScreen.GetComponentsInChildren<Canvas>(true)) {
                canvas.sortingOrder = 0;
            }
            this.AddToVRPointer();
            this.UpdateChatUI();
        }

        private void BSEvents_menuSceneActive()
        {
            this._isInGame = false;
            foreach (var canvas in this._chatScreen.GetComponentsInChildren<Canvas>(true)) {
                canvas.sortingOrder = 3;
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

        private bool _updateMessagePositions = false;
        private WaitForEndOfFrame _waitForEndOfFrame;


        private IEnumerator UpdateMessagePositions()
        {
            yield return this._waitForEndOfFrame;
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

        private void OnRenderRebuildComplete() => this._updateMessagePositions = true;

        public void AddMessage(EnhancedTextMeshProUGUIWithBackground newMsg)
        {
            newMsg.OnLatePreRenderRebuildComplete -= this.OnRenderRebuildComplete;
            newMsg.OnLatePreRenderRebuildComplete += this.OnRenderRebuildComplete;
            this.UpdateMessage(newMsg, true);
            this._messages.Enqueue(newMsg);
            this.ClearOldMessages();
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
            foreach (var msg in this._messages.ToArray()) {
                this.UpdateMessage(msg, true);
            }
            this._updateMessagePositions = true;
        }

        private void UpdateMessage(EnhancedTextMeshProUGUIWithBackground msg, bool setAllDirty = false)
        {

            (msg.transform as RectTransform).sizeDelta = new Vector2(this.ChatWidth, (msg.transform as RectTransform).sizeDelta.y);
            msg.Text.font = ESCFontManager.instance.MainFont;
            msg.Text.font.fallbackFontAssetTable = ESCFontManager.instance.FallBackFonts;
            msg.Text.overflowMode = TextOverflowModes.Overflow;
            msg.Text.alignment = TextAlignmentOptions.BottomLeft;
            msg.Text.color = this.TextColor;
            msg.Text.fontSize = this.FontSize;
            msg.Text.lineSpacing = 1.5f;

            msg.SubText.font = ESCFontManager.instance.MainFont;
            msg.SubText.font.fallbackFontAssetTable = ESCFontManager.instance.FallBackFonts;
            msg.SubText.overflowMode = TextOverflowModes.Overflow;
            msg.SubText.alignment = TextAlignmentOptions.BottomLeft;
            msg.SubText.color = this.TextColor;
            msg.SubText.fontSize = this.FontSize;
            msg.SubText.lineSpacing = 1.5f;

            if (msg.Text.ChatMessage != null) {
                msg.HighlightColor = msg.Text.ChatMessage.IsPing ? this.PingColor : this.HighlightColor;
                msg.AccentColor = this.AccentColor;
                msg.HighlightEnabled = msg.Text.ChatMessage.IsHighlighted || msg.Text.ChatMessage.IsPing;
                msg.AccentEnabled = !msg.Text.ChatMessage.IsPing && (msg.HighlightEnabled || msg.SubText.ChatMessage != null);
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
            while (this._messages.TryPeek(out var msg) && this.ReverseChatOrder ? msg.transform.localPosition.y < 0 - (msg.transform as RectTransform).sizeDelta.y : msg.transform.localPosition.y >= ChatConfig.instance.ChatHeight) {
                if (this._messages.TryDequeue(out msg)) {
                    this.TextPool.Free(msg);
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
            sb.Append(": <color=#bbbbbbbb><message deleted></color>");
            return sb.ToString();
        }

        private void ClearMessage(EnhancedTextMeshProUGUIWithBackground msg)
        {
            // Only clear non-system messages
            if (!msg.Text.ChatMessage.IsSystemMessage) {
                msg.Text.text = this.BuildClearedMessage(msg.Text);
                msg.SubTextEnabled = false;
            }
            if (msg.SubText.ChatMessage != null && !msg.SubText.ChatMessage.IsSystemMessage) {
                msg.SubText.text = this.BuildClearedMessage(msg.SubText);
            }
        }

        public void OnMessageCleared(string messageId)
        {
            if (messageId != null) {
                MainThreadInvoker.Invoke(() =>
                {
                    foreach (var msg in this._messages.ToArray()) {
                        if (msg.Text.ChatMessage == null) {
                            continue;
                        }
                        if (msg.Text.ChatMessage.Id == messageId) {
                            this.ClearMessage(msg);
                        }
                    }
                });
            }
        }

        public void OnChatCleared(string userId) => MainThreadInvoker.Invoke(() =>
                                                  {
                                                      foreach (var msg in this._messages.ToArray()) {
                                                          if (msg.Text.ChatMessage == null) {
                                                              continue;
                                                          }
                                                          if (userId == null || msg.Text.ChatMessage.Sender.Id == userId) {
                                                              this.ClearMessage(msg);
                                                          }
                                                      }
                                                  });

        public void OnJoinChannel(IChatService svc, IChatChannel channel) => MainThreadInvoker.Invoke(() =>
                                                                           {
                                                                               var newMsg = this.TextPool.Alloc();
                                                                               newMsg.Text.text = $"<color=#bbbbbbbb>[{svc.DisplayName}] Success joining {channel.Id}</color>";
                                                                               newMsg.HighlightEnabled = true;
                                                                               newMsg.HighlightColor = Color.gray.ColorWithAlpha(0.05f);
                                                                               this.AddMessage(newMsg);
                                                                           });

        public void OnChannelResourceDataCached(IChatChannel channel, Dictionary<string, IChatResourceData> resources) => MainThreadInvoker.Invoke(() =>
                                                                                                                        {
                                                                                                                            var count = 0;
                                                                                                                            if (this._chatConfig.PreCacheAnimatedEmotes) {
                                                                                                                                foreach (var emote in resources) {
                                                                                                                                    if (emote.Value.IsAnimated) {
                                                                                                                                        HMMainThreadDispatcher.instance.Enqueue(ChatImageProvider.instance.PrecacheAnimatedImage(emote.Value.Uri, emote.Key, 110));
                                                                                                                                        count++;
                                                                                                                                    }
                                                                                                                                }
                                                                                                                                Logger.Info($"Pre-cached {count} animated emotes.");
                                                                                                                            }
                                                                                                                            else {
                                                                                                                                Logger.Warn("Pre-caching of animated emotes disabled by the user. If you're experiencing lag, re-enable emote precaching.");
                                                                                                                            }
                                                                                                                        });

        private EnhancedTextMeshProUGUIWithBackground _lastMessage;
        public void OnTextMessageReceived(IChatMessage msg) => _ = this.OnTextMessageReceived(msg, DateTime.Now);
        public async Task OnTextMessageReceived(IChatMessage msg, DateTime dateTime)
        {
            var parsedMessage = await ChatMessageBuilder.BuildMessage(msg, ESCFontManager.instance.FontInfo);
            while (this.TextPool == null) {
                await Task.Delay(100);
            }
            HMMainThreadDispatcher.instance.Enqueue(() => this.CreateMessage(msg, dateTime, parsedMessage));
        }

        private void CreateMessage(IChatMessage msg, DateTime date, string parsedMessage)
        {
            if (this._lastMessage != null && !msg.IsSystemMessage && this._lastMessage.Text.ChatMessage.Id == msg.Id) {
                // If the last message received had the same id and isn't a system message, then this was a sub-message of the original and may need to be highlighted along with the original message
                this._lastMessage.SubText.text = parsedMessage;
                this._lastMessage.SubText.ChatMessage = msg;
                this._lastMessage.SubTextEnabled = true;
                this.UpdateMessage(this._lastMessage, true);
            }
            else {
                var newMsg = this.TextPool.Alloc();
                newMsg.gameObject.SetActive(true);
                newMsg.Text.ChatMessage = msg;
                newMsg.Text.text = parsedMessage;
                newMsg.ReceivedDate = date;
                this.AddMessage(newMsg);
                this._lastMessage = newMsg;
            }
            this._updateMessagePositions = true;
        }
    }
}
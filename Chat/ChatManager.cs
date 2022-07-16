using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using ChatCore;
using ChatCore.Interfaces;
using ChatCore.Logging;
using ChatCore.Services;
using EnhancedStreamChat.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EnhancedStreamChat.Chat
{
    public class ChatManager : PersistentSingleton<ChatManager>
    {
        internal ChatCoreInstance _chatCoreInstance;
        internal ChatServiceMultiplexer _chatServiceMultiplexer;
        private ChatDisplay _chatDisplay;

        #region // Unity message
        private void Awake()
        {
            this._chatCoreInstance = ChatCoreInstance.Create();
#if DEBUG
            this._chatCoreInstance.OnLogReceived += this._sc_OnLogReceived;
#endif
            this._chatServiceMultiplexer = this._chatCoreInstance.RunAllServices();
            this._chatServiceMultiplexer.OnJoinChannel += this.QueueOrSendOnJoinChannel;
            this._chatServiceMultiplexer.OnTextMessageReceived += this.QueueOrSendOnTextMessageReceived;
            this._chatServiceMultiplexer.OnChatCleared += this.QueueOrSendOnClearChat;
            this._chatServiceMultiplexer.OnMessageCleared += this.QueueOrSendOnClearMessage;
            this._chatServiceMultiplexer.OnChannelResourceDataCached += this.QueueOrSendOnChannelResourceDataCached;
            ChatImageProvider.TouchInstance();
            _ = this.HandleOverflowMessageQueue();
            BSEvents.lateMenuSceneLoadedFresh += this.BSEvents_menuSceneLoadedFresh;
        }


        private void Update()
        {
            while (this._chatDisplay && this.ActionQueue.TryDequeue(out var action)) {
                action?.Invoke();
            }
        }

        protected override void OnDestroy()
        {
            if (this._chatServiceMultiplexer != null) {
                try {
                    this._chatServiceMultiplexer.OnJoinChannel -= this.QueueOrSendOnJoinChannel;
                    this._chatServiceMultiplexer.OnTextMessageReceived -= this.QueueOrSendOnTextMessageReceived;
                    this._chatServiceMultiplexer.OnChatCleared -= this.QueueOrSendOnClearChat;
                    this._chatServiceMultiplexer.OnMessageCleared -= this.QueueOrSendOnClearMessage;
                    this._chatServiceMultiplexer.OnChannelResourceDataCached -= this.QueueOrSendOnChannelResourceDataCached;
                }
                catch (Exception e) {
                    Logger.Error(e);
                }
                BSEvents.lateMenuSceneLoadedFresh -= this.BSEvents_menuSceneLoadedFresh;
            }
            if (this._chatCoreInstance != null) {
#if DEBUG
                this._chatCoreInstance.OnLogReceived -= this._sc_OnLogReceived;
#endif
                try {
                    this._chatCoreInstance.StopAllServices();
                }
                catch (Exception e) {
                    Logger.Error(e);
                }
            }

            MainThreadInvoker.ClearQueue();
            ChatImageProvider.ClearCache();
            base.OnDestroy();
        }
        #endregion


        private void _sc_OnLogReceived(CustomLogLevel level, string category, string log)
        {
            var newLevel = level switch
            {
                CustomLogLevel.Critical => IPA.Logging.Logger.Level.Critical,
                CustomLogLevel.Debug => IPA.Logging.Logger.Level.Debug,
                CustomLogLevel.Error => IPA.Logging.Logger.Level.Error,
                CustomLogLevel.Information => IPA.Logging.Logger.Level.Info,
                CustomLogLevel.Trace => IPA.Logging.Logger.Level.Trace,
                CustomLogLevel.Warning => IPA.Logging.Logger.Level.Warning,
                _ => IPA.Logging.Logger.Level.None
            };
            Logger.cclog.Log(newLevel, log);
        }
        private void BSEvents_menuSceneLoadedFresh(ScenesTransitionSetupDataSO scenesTransitionSetupDataSo)
        {
            if (this._chatDisplay) {
                DestroyImmediate(this._chatDisplay.gameObject);
                this._chatDisplay = null;
                MainThreadInvoker.ClearQueue();
            }
            this._chatDisplay = BeatSaberUI.CreateViewController<ChatDisplay>();
            this._chatDisplay.gameObject.SetActive(true);
        }

        private ConcurrentQueue<Action> ActionQueue { get; } = new ConcurrentQueue<Action>();
        //private readonly SemaphoreSlim _msgLock = new SemaphoreSlim(1, 1);
        private async Task HandleOverflowMessageQueue()
        {
            while (!_applicationIsQuitting) {
                try {
                    if (this._chatDisplay == null) {
                        // If _chatViewController isn't instantiated yet, lock the semaphore and wait until it is.
                        //await this._msgLock.WaitAsync();
                        while (this._chatDisplay == null) {
                            await Task.Delay(1000);
                        }
                    }
                    else {
                        // If _chatViewController is instantiated, wait here until the action queue has any actions.
                        while (this.ActionQueue.IsEmpty) {
                            //Logger.Info("Queue is empty.");
                            await Task.Delay(1000);
                        }
                        // Once an action is added to the queue, lock the semaphore before working through the queue.
                        //await this._msgLock.WaitAsync();
                    }
                    var i = 0;
                    var start = DateTime.UtcNow;
                    var stopwatch = Stopwatch.StartNew();
                    // Work through the queue of messages that has piled up one by one until they're all gone.
                    while (this.ActionQueue.TryDequeue(out var action)) {
                        action.Invoke();
                        i++;
                    }
                    stopwatch.Stop();
                    Logger.Warn($"{i} overflowed actions were executed in {stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond}ms.");
                }
                finally {
                    // Release the lock, which will allow messages to pass through without the queue again
                    //this._msgLock.Release();
                }
            }
        }

        private void QueueOrSendMessage<A>(IChatService svc, A a, Action<IChatService, A> action)
        {
            if (this._chatDisplay == null) {
                this.ActionQueue.Enqueue(() => action.Invoke(svc, a));
            }
            else {
                action.Invoke(svc, a);
                //this._msgLock.Release();
            }
        }
        private void QueueOrSendMessage<A, B>(IChatService svc, A a, B b, Action<IChatService, A, B> action)
        {
            if (this._chatDisplay == null) {
                this.ActionQueue.Enqueue(() => action.Invoke(svc, a, b));
            }
            else {
                action.Invoke(svc, a, b);
                //this._msgLock.Release();
            }
        }

        private void QueueOrSendOnChannelResourceDataCached(IChatService svc, IChatChannel channel, Dictionary<string, IChatResourceData> resources)
        {
            this.QueueOrSendMessage(svc, channel, resources, this.OnChannelResourceDataCached);
        }

        private void OnChannelResourceDataCached(IChatService svc, IChatChannel channel, Dictionary<string, IChatResourceData> resources)
        {
            this._chatDisplay.OnChannelResourceDataCached(channel, resources);
        }

        private void QueueOrSendOnTextMessageReceived(IChatService svc, IChatMessage msg)
        {
            this.QueueOrSendMessage(svc, msg, this.OnTextMesssageReceived);
        }

        private void OnTextMesssageReceived(IChatService svc, IChatMessage msg)
        {
            this._chatDisplay.OnTextMessageReceived(msg);
        }

        private void QueueOrSendOnJoinChannel(IChatService svc, IChatChannel channel)
        {
            this.QueueOrSendMessage(svc, channel, this.OnJoinChannel);
        }

        private void OnJoinChannel(IChatService svc, IChatChannel channel)
        {
            this._chatDisplay.OnJoinChannel(svc, channel);
        }

        private void QueueOrSendOnClearMessage(IChatService svc, string messageId)
        {
            this.QueueOrSendMessage(svc, messageId, this.OnClearMessage);
        }

        private void OnClearMessage(IChatService svc, string messageId)
        {
            this._chatDisplay.OnMessageCleared(messageId);
        }

        private void QueueOrSendOnClearChat(IChatService svc, string userId)
        {
            this.QueueOrSendMessage(svc, userId, this.OnClearChat);
        }

        private void OnClearChat(IChatService svc, string userId)
        {
            this._chatDisplay.OnChatCleared(userId);
        }
    }
}

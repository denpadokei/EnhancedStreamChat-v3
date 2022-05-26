using CatCore;
using CatCore.Logging;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.PubSub.Responses;
using CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1;
using CatCore.Services.Multiplexer;
using CatCore.Services.Twitch;
using CatCore.Services.Twitch.Interfaces;
using EnhancedStreamChat.Interfaces;
using IPA.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Zenject;

namespace EnhancedStreamChat.CatCoreWrapper
{
    internal class CatCoreManager : IDisposable, ICatCoreManager, IInitializable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // イベント
        public event Action<MultiplexedPlatformService> OnAuthenticatedStateChanged;
        public event Action<MultiplexedPlatformService> OnChatConnected;
        public event Action<MultiplexedPlatformService, MultiplexedMessage> OnTextMessageReceived;
        public event Action<ITwitchService, TwitchMessage> OnTwitchTextMessageReceived;
        public event Action<MultiplexedPlatformService, MultiplexedChannel> OnJoinChannel;
        public event Action<MultiplexedPlatformService, MultiplexedChannel> OnLeaveChannel;
        public event Action<MultiplexedPlatformService, MultiplexedChannel> OnRoomStateUpdated;
        public event Action<MultiplexedPlatformService, MultiplexedChannel, string> OnMessageDeleted;
        public event Action<MultiplexedPlatformService, MultiplexedChannel, string> OnChatCleared;
        public event OnRewardRedeemedHandler OnRewardRedeemed;
        public event OnFollowHandler OnFollow;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド用メソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            this.RunService();
        }
        public void RunService()
        {
            if (this._instance == null) {
                return;
            }
            this._chatServiceMultiplexer = this._instance.RunAllServices();
            this._twitchPratformService = this._chatServiceMultiplexer.GetTwitchPlatformService();
            this._twitchIrcService = typeof(TwitchService).GetField("_twitchIrcService", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this._twitchPratformService);
            this._twitchPubSubServiceManager = this._twitchPratformService.GetPubSubService();
            this._twitchPubSubServiceManager.OnFollow += this.OnTwitchPubSubServiceManager_OnFollow;
            this._twitchPubSubServiceManager.OnRewardRedeemed += this.OnTwitchPubSubServiceManager_OnRewardRedeemed;
            this._chatServiceMultiplexer.OnAuthenticatedStateChanged += this.ChatServiceMultiplexerOnAuthenticatedStateChanged;
            this._chatServiceMultiplexer.OnChatConnected += this.ChatServiceMultiplexerOnChatConnected;
            this._chatServiceMultiplexer.OnTextMessageReceived += this.ChatServiceMultiplexerOnTextMessageReceived;
            this._twitchPratformService.OnTextMessageReceived += this.OnTwitchPratformService_OnTextMessageReceived;
            this._chatServiceMultiplexer.OnJoinChannel += this.ChatServiceMultiplexerOnJoinChannel;
            this._chatServiceMultiplexer.OnLeaveChannel += this.ChatServiceMultiplexerOnLeaveChannel;
            this._chatServiceMultiplexer.OnRoomStateUpdated += this.ChatServiceMultiplexer_OnRoomStateUpdated;
            this._chatServiceMultiplexer.OnMessageDeleted += this.ChatServiceMultiplexer_OnMessageDeleted;
            this._chatServiceMultiplexer.OnChatCleared += this.ChatServiceMultiplexer_OnChatCleared;
        }

        public void LaunchWebPortal(bool shouldLaunchPortal = true)
        {
            if (!shouldLaunchPortal) {
                return;
            }
            this._instance.LaunchWebPortal();
        }

        public Task IrcStart()
        {
            var task = this._ircstart?.Invoke(this._twitchIrcService, null);
            return task as Task ?? Task.CompletedTask;
        }

        public Task IrcStop()
        {
            var task = this._ircstop?.Invoke(this._twitchIrcService, null);
            return task as Task ?? Task.CompletedTask;
        }

        public Task PubSubStart(object instance)
        {
            var task = this._pubsubstart?.Invoke(instance, new object[] { false });
            return task as Task ?? Task.CompletedTask;
        }

        public Task PubSubStop(object instance)
        {
            var task = this._pubsubstop?.Invoke(instance, new object[] { "Forced to go close" });
            return task as Task ?? Task.CompletedTask;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void ChatServiceMultiplexerOnTextMessageReceived(MultiplexedPlatformService arg1, MultiplexedMessage arg2)
        {
            this.OnTextMessageReceived?.Invoke(arg1, arg2);
        }
        private void OnTwitchPratformService_OnTextMessageReceived(ITwitchService arg1, TwitchMessage arg2)
        {
            this.OnTwitchTextMessageReceived?.Invoke(arg1, arg2);
        }

        private void ChatServiceMultiplexerOnChatConnected(MultiplexedPlatformService obj)
        {
            this.OnChatConnected?.Invoke(obj);
        }

        private void ChatServiceMultiplexerOnJoinChannel(MultiplexedPlatformService arg1, MultiplexedChannel arg2)
        {
            this.OnJoinChannel?.Invoke(arg1, arg2);
        }
        private void ChatServiceMultiplexer_OnChatCleared(MultiplexedPlatformService arg1, MultiplexedChannel arg2, string arg3)
        {
            this.OnChatCleared?.Invoke(arg1, arg2, arg3);
        }

        private void ChatServiceMultiplexer_OnMessageDeleted(MultiplexedPlatformService arg1, MultiplexedChannel arg2, string arg3)
        {
            this.OnMessageDeleted?.Invoke(arg1, arg2, arg3);
        }

        private void ChatServiceMultiplexer_OnRoomStateUpdated(MultiplexedPlatformService arg1, MultiplexedChannel arg2)
        {
            this.OnRoomStateUpdated?.Invoke(arg1, arg2);
        }

        private void ChatServiceMultiplexerOnLeaveChannel(MultiplexedPlatformService arg1, MultiplexedChannel arg2)
        {
            this.OnLeaveChannel?.Invoke(arg1, arg2);
        }

        private void ChatServiceMultiplexerOnAuthenticatedStateChanged(MultiplexedPlatformService obj)
        {
            this.OnAuthenticatedStateChanged?.Invoke(obj);
        }

        private void OnTwitchPubSubServiceManager_OnRewardRedeemed(string arg1, RewardRedeemedData arg2)
        {
            this.OnRewardRedeemed?.Invoke(arg1, arg2);
        }

        private void OnTwitchPubSubServiceManager_OnFollow(string arg1, Follow arg2)
        {
            this.OnFollow?.Invoke(arg1, arg2);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private readonly CatCoreInstance _instance;
        private ChatServiceMultiplexer _chatServiceMultiplexer;
        private ITwitchService _twitchPratformService;
        private ITwitchPubSubServiceManager _twitchPubSubServiceManager;
        private bool _disposedValue;
        private object _twitchIrcService;
        private readonly MethodInfo _ircstart = Type.GetType("CatCore.Services.Twitch.TwitchIrcService, CatCore").GetMethod("CatCore.Services.Twitch.Interfaces.ITwitchIrcService.Start", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly MethodInfo _ircstop = Type.GetType("CatCore.Services.Twitch.TwitchIrcService, CatCore").GetMethod("CatCore.Services.Twitch.Interfaces.ITwitchIrcService.Stop", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly MethodInfo _pubsubstart = Type.GetType("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent, CatCore").GetMethod("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent.Start", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly MethodInfo _pubsubstop = Type.GetType("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent, CatCore").GetMethod("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent.Stop", BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        public CatCoreManager()
        {
            this._instance = CatCoreInstance.
            Create((level, context, message) => Logger.Log
                .GetChildLogger("CatCore")
                .Log(level switch
                {
                    CustomLogLevel.Trace => IPA.Logging.Logger.Level.Trace,
                    CustomLogLevel.Debug => IPA.Logging.Logger.Level.Debug,
                    CustomLogLevel.Information => IPA.Logging.Logger.Level.Info,
                    CustomLogLevel.Warning => IPA.Logging.Logger.Level.Warning,
                    CustomLogLevel.Error => IPA.Logging.Logger.Level.Error,
                    CustomLogLevel.Critical => IPA.Logging.Logger.Level.Critical,
                    _ => IPA.Logging.Logger.Level.Debug
                }, $"{context} | {message}"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    this._chatServiceMultiplexer.OnAuthenticatedStateChanged -= this.ChatServiceMultiplexerOnAuthenticatedStateChanged;
                    this._chatServiceMultiplexer.OnChatConnected -= this.ChatServiceMultiplexerOnChatConnected;
                    this._chatServiceMultiplexer.OnTextMessageReceived -= this.ChatServiceMultiplexerOnTextMessageReceived;
                    this._chatServiceMultiplexer.OnJoinChannel -= this.ChatServiceMultiplexerOnJoinChannel;
                    this._chatServiceMultiplexer.OnLeaveChannel -= this.ChatServiceMultiplexerOnLeaveChannel;
                    this._chatServiceMultiplexer.OnRoomStateUpdated -= this.ChatServiceMultiplexer_OnRoomStateUpdated;
                    this._chatServiceMultiplexer.OnMessageDeleted -= this.ChatServiceMultiplexer_OnMessageDeleted;
                    this._chatServiceMultiplexer.OnChatCleared -= this.ChatServiceMultiplexer_OnChatCleared;
                    this._twitchPubSubServiceManager.OnFollow -= this.OnTwitchPubSubServiceManager_OnFollow;
                    this._twitchPubSubServiceManager.OnRewardRedeemed -= this.OnTwitchPubSubServiceManager_OnRewardRedeemed;
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
    }
}

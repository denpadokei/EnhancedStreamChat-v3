using CatCore.Models.Shared;
using CatCore.Models.Twitch.EventSub.Channel;
using CatCore.Models.Twitch.EventSub.ChannelPoints;
using CatCore.Models.Twitch.EventSub.Charity;
using CatCore.Models.Twitch.EventSub.Goal;
using CatCore.Models.Twitch.EventSub.HypeTrain;
using CatCore.Models.Twitch.EventSub.Moderation;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.PubSub.Responses;
using CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1;
using CatCore.Services.Multiplexer;
using CatCore.Services.Twitch.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnhancedStreamChat.Interfaces
{
    public interface ICatCoreManager
    {
        event Action<MultiplexedPlatformService> OnAuthenticatedStateChanged;
        event Action<MultiplexedPlatformService, MultiplexedChannel, string> OnChatCleared;
        event Action<MultiplexedPlatformService> OnChatConnected;
        event OnFollowHandler OnFollow;
        event Action<MultiplexedPlatformService, MultiplexedChannel> OnJoinChannel;
        event Action<MultiplexedPlatformService, MultiplexedChannel> OnLeaveChannel;
        event Action<MultiplexedPlatformService, MultiplexedChannel, string> OnMessageDeleted;
        event OnRewardRedeemedHandler OnRewardRedeemed;
        event Action<MultiplexedPlatformService, MultiplexedChannel> OnRoomStateUpdated;
        event Action<MultiplexedPlatformService, MultiplexedMessage> OnTextMessageReceived;
        event Action<ITwitchService, TwitchMessage> OnTwitchTextMessageReceived;
        event Action<string, HypeTrainEvent> OnHypeTrainBegin;
        event Action<string, HypeTrainEvent> OnHypeTrainEnd;
        event Action<string, GoalEvent> OnGoalBegin;
        event Action<string, GoalEvent> OnGoalEnd;
        event Action<string, CharityCampaignEvent> OnCharityCampaignStarted;
        event Action<string, CharityCampaignEvent> OnCharityCampaignStopped;
        event Action<string, ShoutoutReceivedEvent> OnShoutoutReceived;
        event Action<string, BanEvent> OnUserBanned;
        event Action<string, UnbanEvent> OnUserUnbanned;
        event Action<string, AutomaticRewardRedemptionEvent> OnAutomaticRewardRedeemed;
        event Action<string, CustomPowerUpRedemptionEvent> OnCustomPowerUpRedeemed;

        void RunService();
        void LaunchWebPortal(bool shouldLaunchPortal = true);
        Task IrcStart();
        Task IrcStop();
        Task PubSubStart(object instance);
        Task PubSubStop(object instance);
        Task<IReadOnlyDictionary<string, IChatBadge>> GetChatBadgesAsync(string channelId);
        Task<string> GetChannelDisplayNameAsync(string channelId);
    }

    public delegate void OnRewardRedeemedHandler(string channelId, in RewardRedeemedData data);
    public delegate void OnFollowHandler(string channelId, in Follow data);
}

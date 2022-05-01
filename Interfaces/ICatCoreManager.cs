using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.PubSub.Responses;
using CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1;
using CatCore.Services.Multiplexer;
using CatCore.Services.Twitch.Interfaces;
using System;

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

        void LaunchWebPortal(bool shouldLaunchPortal = true);
    }


    public delegate void OnRewardRedeemedHandler(string channelId, in RewardRedeemedData data);
    public delegate void OnFollowHandler(string channelId, in Follow data);
}
using CatCore.Models.Twitch;
using EnhancedStreamChat.Interfaces;

namespace EnhancedStreamChat.Models
{
    public class ESCChatChannel : IESCChatChannel
    {
        public string ID => this._channel.Id;

        public string Name => this._channel.Name;

        private readonly TwitchChannel _channel;

        public void SendMessage(string message)
        {
            this._channel.SendMessage(message);
        }

        public ESCChatChannel(TwitchChannel twitchChannel)
        {
            this._channel = twitchChannel;
        }
    }
}

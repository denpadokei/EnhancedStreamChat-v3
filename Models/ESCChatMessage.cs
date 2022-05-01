using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using EnhancedStreamChat.Interfaces;
using System.Collections.ObjectModel;

namespace EnhancedStreamChat.Models
{
    public class ESCChatMessage : IESCChatMessage
    {
        public string Id { get; private set; }

        public bool IsSystemMessage { get; private set; }

        public bool IsActionMessage { get; private set; }

        public bool IsMentioned { get; private set; }

        public string Message { get; private set; }

        public IChatUser Sender { get; private set; }

        public IESCChatChannel Channel { get; private set; }

        public ReadOnlyCollection<IChatEmote> Emotes { get; private set; }

        public ReadOnlyDictionary<string, string> Metadata { get; private set; }

        public ESCChatMessage(TwitchMessage twitchMessage)
        {
            this.Id = twitchMessage.Id;
            this.IsSystemMessage = twitchMessage.IsSystemMessage;
            this.IsActionMessage = twitchMessage.IsActionMessage;
            this.IsMentioned = twitchMessage.IsMentioned;
            this.Message = twitchMessage.Message;
            this.Sender = twitchMessage.Sender;
            this.Channel = new ESCChatChannel(twitchMessage.Channel);
            this.Metadata = twitchMessage.Metadata;
            this.Emotes = twitchMessage.Emotes;
        }
    }
}

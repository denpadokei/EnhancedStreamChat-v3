using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using EnhancedStreamChat.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EnhancedStreamChat.Models
{
    public class ESCChatMessage : IESCChatMessage
    {
        public string Id { get; internal set; }

        public bool IsSystemMessage { get; internal set; }

        public bool IsActionMessage { get; internal set; }

        public bool IsMentioned { get; internal set; }

        public string Message { get; internal set; }

        public IChatUser Sender { get; internal set; }

        public IESCChatChannel Channel { get; internal set; }

        public ReadOnlyCollection<IChatEmote> Emotes { get; internal set; } = new ReadOnlyCollection<IChatEmote>(Array.Empty<IChatEmote>());

        public ReadOnlyDictionary<string, string> Metadata { get; internal set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

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
        public ESCChatMessage(string id, string message)
        {
            this.Id = id;
            this.Message = message;
        }
    }
}

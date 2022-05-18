using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.Media;
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
        public bool IsHighlighted { get; internal set; }
        public string Message { get; internal set; } = "";
        public string SubMessage { get; internal set; } = "";
        public IChatUser Sender { get; internal set; }
        public IESCChatChannel Channel { get; internal set; }
        public ReadOnlyCollection<IChatEmote> Emotes { get; internal set; } = new ReadOnlyCollection<IChatEmote>(Array.Empty<IChatEmote>());
        public ReadOnlyDictionary<string, string> Metadata { get; internal set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        public ESCChatMessage(TwitchMessage twitchMessage)
        {
            this.IsSystemMessage = twitchMessage.IsSystemMessage;
            if (twitchMessage.Metadata != null) {
                this.Metadata = twitchMessage.Metadata;
            }
            if (twitchMessage.Emotes != null) {
                this.Emotes = twitchMessage.Emotes;
            }
            if (this.SystemMessageSetup()) {
                this.SubMessage = twitchMessage.Message;
            }
            else {
                this.Message = twitchMessage.Message;
                this.SubMessage = "";
            }
            this.Id = twitchMessage.Id;
            this.IsActionMessage = twitchMessage.IsActionMessage;
            this.IsMentioned = twitchMessage.IsMentioned;
            this.Sender = twitchMessage.Sender;
            this.Channel = new ESCChatChannel(twitchMessage.Channel);
#if DEBUG
            Logger.Debug($"{this.Message}");
            Logger.Debug($"{this.SubMessage}");
#endif
        }
        public ESCChatMessage(string id, string message)
        {
            this.Id = id;
            this.Message = message;
        }

        private bool SystemMessageSetup()
        {
            var updateMessage = false;
            if (this.Metadata.TryGetValue("msg-id", out var msgIdValue)) {
                switch (msgIdValue) {
                    case "skip-subs-mode-message":
                        this.Message = "Redeemed Send a Message In Sub-Only Mode";
                        this.IsHighlighted = false;
                        this.IsSystemMessage = true;
                        updateMessage = true;
                        break;
                    case "highlighted-message":
                        this.Message = "Redeemed Highlight My Message";
                        this.IsHighlighted = true;
                        this.IsSystemMessage = true;
                        updateMessage = true;
                        break;
                    //case "sub":
                    //case "resub":
                    //case "raid":
                    default:
                        if (this.Metadata.TryGetValue("system-msg", out var systemMsgText)) {
                            systemMsgText = systemMsgText.Replace(@"\s", " ");
                            this.IsHighlighted = true;
                            this.IsSystemMessage = true;
                            if (this.Metadata.TryGetValue("msg-param-sub-plan", out var subPlanName)) {
                                this.Message = subPlanName == "Prime" ? $"👑  {systemMsgText}" : $"⭐  {systemMsgText}";
                                updateMessage = true;
                            }
                            else if (this.Metadata.TryGetValue("msg-param-profileImageURL", out var profileImage) && this.Metadata.TryGetValue("msg-param-login", out var loginUser)) {
                                var emoteId = $"ProfileImage_{loginUser}";
                                this.Emotes = new ReadOnlyCollection<IChatEmote>(new IChatEmote[]
                                {
                                    new TwitchEmote(emoteId, $"[{emoteId}]", 0, emoteId.Length + 1, profileImage, false),
                                });
                                this.Message = $"{this.Emotes[0].Name}  {systemMsgText}";
                                updateMessage = true;
                            }
                        }
                        else {
                            // If there's no system message, the message must be the actual message.
                            // In this case we wipe out the original message and skip it.
                            this.IsHighlighted = true;
                            this.IsSystemMessage = true;
                        }
                        break;
                }
            }
            else {
                this.IsSystemMessage = false;
            }
            return updateMessage;
        }
    }
}


using CatCore.Models.Shared;
using System.Collections.ObjectModel;

namespace EnhancedStreamChat.Interfaces
{
    public interface IESCChatMessage
    {
        string Id
        {
            get;
        }
        bool IsSystemMessage
        {
            get;
        }
        bool IsActionMessage
        {
            get;
        }
        bool IsMentioned
        {
            get;
        }
        string Message
        {
            get;
        }
        IChatUser Sender
        {
            get;
        }
        IESCChatChannel Channel
        {
            get;
        }
        ReadOnlyCollection<IChatEmote> Emotes
        {
            get;
        }
        ReadOnlyDictionary<string, string> Metadata
        {

            get;
        }
    }
}

using CatCore.Models.Shared;
using System.Collections.ObjectModel;

namespace EnhancedStreamChat.Interfaces
{
    public interface IESCChatMessage
    {
        string Id { get; }
        bool IsSystemMessage { get; }
        bool IsActionMessage { get; }
        bool IsMentioned { get; }
        bool IsHighlighted { get; }
        string Message { get; }
        string SubMessage { get; }
        IChatUser Sender { get; }
        ReadOnlyCollection<IChatEmote> Emotes { get; }
        ReadOnlyDictionary<string, string> Metadata { get; }
    }
}

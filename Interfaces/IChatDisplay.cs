using System;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedStreamChat.Interfaces
{
    public interface IChatDisplay
    {
        Color AccentColor { get; set; }
        bool AllowMovement { get; set; }
        Color BackgroundColor { get; set; }
        int ChatHeight { get; set; }
        Vector3 ChatPosition { get; set; }
        Vector3 ChatRotation { get; set; }
        int ChatWidth { get; set; }
        float FontSize { get; set; }
        Color HighlightColor { get; set; }
        string ModVersion { get; }
        Color PingColor { get; set; }
        bool ReverseChatOrder { get; set; }
        int SettingsWidth { get; set; }
        bool SyncOrientation { get; set; }
        Color TextColor { get; set; }
        Task OnTextMessageReceived(IESCChatMessage msg, DateTime dateTime);
        void OnChatCleared(string userId);
        void OnMessageCleared(string messageId);
    }
}
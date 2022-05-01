using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedStreamChat.Chat
{
    public interface IChatDisplaySetting
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

        void AddMessage(EnhancedTextMeshProUGUIWithBackground newMsg);
        Task InitializeAsync(CancellationToken token);
        void OnChatCleared(string userId);
        void OnMessageCleared(string messageId);
        Task OnTextMessageReceived(IESCChatMessage msg, DateTime dateTime);
    }
}
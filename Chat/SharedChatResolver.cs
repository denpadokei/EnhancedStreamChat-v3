using CatCore.Models.Shared;
using EnhancedStreamChat.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnhancedStreamChat.Chat
{
    /// <summary>
    /// Resolves the source channel name and badges of messages relayed through Twitch Shared Chat.
    /// </summary>
    public class SharedChatResolver
    {
        private static readonly TimeSpan s_retryInterval = TimeSpan.FromSeconds(30);

        private readonly ICatCoreManager _catCoreManager;
        private readonly ConcurrentDictionary<string, Task<IReadOnlyDictionary<string, IChatBadge>>> _badgesByChannel = new ConcurrentDictionary<string, Task<IReadOnlyDictionary<string, IChatBadge>>>();
        private readonly ConcurrentDictionary<string, Task<string>> _channelNames = new ConcurrentDictionary<string, Task<string>>();
        private readonly ConcurrentDictionary<string, DateTime> _badgeRequestFailures = new ConcurrentDictionary<string, DateTime>();
        private readonly ConcurrentDictionary<string, DateTime> _channelNameRequestFailures = new ConcurrentDictionary<string, DateTime>();

        public SharedChatResolver(ICatCoreManager catCoreManager)
        {
            this._catCoreManager = catCoreManager;
        }

        /// <summary>
        /// Returns the badges that should be displayed for the given message.
        /// For messages from another channel in a Shared Chat session the badges of the source channel are used.
        /// Returns <see langword="null"/> when the message is not a shared message or the badges could not be resolved.
        /// </summary>
        public async Task<IReadOnlyList<IChatBadge>> GetBadgesAsync(IESCChatMessage msg)
        {
            if (msg == null || !msg.IsSharedChatMessage || string.IsNullOrEmpty(msg.SourceChannelId)) {
                return null;
            }
            if (!msg.Metadata.TryGetValue("source-badges", out var sourceBadges) || sourceBadges == null) {
                return null;
            }
            if (sourceBadges.Length == 0) {
                return Array.Empty<IChatBadge>();
            }
            var badges = await this.GetBadgesByChannelAsync(msg.SourceChannelId).ConfigureAwait(false);
            if (badges == null) {
                return null;
            }
            var result = new List<IChatBadge>();
            foreach (var identifier in sourceBadges.Split(',')) {
                var trimmed = identifier.Trim();
                if (trimmed.Length == 0) {
                    continue;
                }
                if (badges.TryGetValue(trimmed, out var badge)) {
                    result.Add(badge);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the display name of the channel a Shared Chat message originated from.
        /// </summary>
        public async Task<string> GetChannelNameAsync(string channelId)
        {
            if (string.IsNullOrEmpty(channelId)) {
                return null;
            }
            try {
                if (!this._channelNames.TryGetValue(channelId, out var task)) {
                    if (this.IsRecentlyFailed(this._channelNameRequestFailures, channelId)) {
                        return null;
                    }
                    task = this._channelNames.GetOrAdd(channelId, id => this._catCoreManager.GetChannelDisplayNameAsync(id));
                }
                var name = await task.ConfigureAwait(false);
                if (string.IsNullOrEmpty(name)) {
                    _ = this._channelNames.TryRemove(channelId, out _);
                    this._channelNameRequestFailures[channelId] = DateTime.UtcNow;
                    return null;
                }
                _ = this._channelNameRequestFailures.TryRemove(channelId, out _);
                return name;
            }
            catch (Exception e) {
                Logger.Error($"Failed to resolve the name of the Shared Chat channel {channelId}. {e}");
                _ = this._channelNames.TryRemove(channelId, out _);
                this._channelNameRequestFailures[channelId] = DateTime.UtcNow;
                return null;
            }
        }

        private async Task<IReadOnlyDictionary<string, IChatBadge>> GetBadgesByChannelAsync(string channelId)
        {
            try {
                if (!this._badgesByChannel.TryGetValue(channelId, out var task)) {
                    if (this.IsRecentlyFailed(this._badgeRequestFailures, channelId)) {
                        return null;
                    }
                    task = this._badgesByChannel.GetOrAdd(channelId, id => this._catCoreManager.GetChatBadgesAsync(id));
                }
                var badges = await task.ConfigureAwait(false);
                if (badges == null) {
                    _ = this._badgesByChannel.TryRemove(channelId, out _);
                    this._badgeRequestFailures[channelId] = DateTime.UtcNow;
                }
                else {
                    _ = this._badgeRequestFailures.TryRemove(channelId, out _);
                }
                return badges;
            }
            catch (Exception e) {
                Logger.Error($"Failed to resolve the badges of the Shared Chat channel {channelId}. {e}");
                _ = this._badgesByChannel.TryRemove(channelId, out _);
                this._badgeRequestFailures[channelId] = DateTime.UtcNow;
                return null;
            }
        }

        private bool IsRecentlyFailed(ConcurrentDictionary<string, DateTime> failures, string channelId)
        {
            if (!failures.TryGetValue(channelId, out var failedAt)) {
                return false;
            }
            if (DateTime.UtcNow - failedAt < s_retryInterval) {
                return true;
            }
            _ = failures.TryRemove(channelId, out _);
            return false;
        }
    }
}

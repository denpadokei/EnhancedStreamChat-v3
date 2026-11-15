using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.Media;
using EnhancedStreamChat.Configuration;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.Interfaces;
using EnhancedStreamChat.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedStreamChat.Chat
{
    public class ChatMessageBuilder
    {
        private readonly ChatImageProvider _chatImageProvider;
        private readonly SharedChatResolver _sharedChatResolver;
        private readonly PluginConfig _config;
        private static readonly ConcurrentDictionary<string, Color> s_senderColor = new ConcurrentDictionary<string, Color>();
        private readonly System.Random _random = new System.Random(Environment.TickCount);
        private const string SharedChatLabelColor = "#B0B0B0FF";
        private const char GigantifiedEmoteStart = '\u0001';
        private const char GigantifiedEmoteEnd = '\u0002';

        public ChatMessageBuilder(ChatImageProvider chatImageProvider, SharedChatResolver sharedChatResolver, PluginConfig config)
        {
            this._chatImageProvider = chatImageProvider;
            this._sharedChatResolver = sharedChatResolver;
            this._config = config;
        }

        /// <summary>
        /// This function *blocks* the calling thread, and caches all the images required to display the message, then registers them with the provided font.
        /// </summary>
        /// <param name="msg">The chat message to get images from</param>
        /// <param name="font">The font to register these images to</param>

        public async Task<bool> PrepareImages(IESCChatMessage msg, EnhancedFontInfo font)
        {
            var tasks = new List<Task<EnhancedImageInfo>>();
            var pendingEmoteDownloads = new HashSet<string>();

            foreach (var emote in msg.Emotes) {
                if (string.IsNullOrEmpty(emote.Id) || pendingEmoteDownloads.Contains(emote.Id)) {
                    continue;
                }
                if (!font.CharacterLookupTable.ContainsKey(emote.Id)) {
                    await MainThreadInvoker.Invoke(() =>
                    {
                        _ = pendingEmoteDownloads.Add(emote.Id);
                        var tcs = new TaskCompletionSource<EnhancedImageInfo>();
                        _ = SharedCoroutineStarter.Instance.StartCoroutine(this._chatImageProvider.TryCacheSingleImage(emote.Id, emote.Url, emote.Animated ? ChatImageProvider.ESCAnimationType.GIF : ChatImageProvider.ESCAnimationType.MAYBE_GIF, (info) =>
                        {
                            if (info == null || !font.TryRegisterImageInfo(info, out var character)) {
                                Logger.Warn($"Failed to register emote \"{emote.Id}\" in font {font.Font.name}.");
                            }
                            tcs.SetResult(info);
                        }, forcedHeight: 110));
                        tasks.Add(tcs.Task);
                    });
                }
            }

            foreach (var badge in await this.GetDisplayBadgesAsync(msg)) {
                if (string.IsNullOrEmpty(badge.Id) || pendingEmoteDownloads.Contains(badge.Id)) {
                    continue;
                }
                if (!font.CharacterLookupTable.ContainsKey(badge.Id)) {
                    await MainThreadInvoker.Invoke(() =>
                    {
                        _ = pendingEmoteDownloads.Add(badge.Id);
                        var tcs = new TaskCompletionSource<EnhancedImageInfo>();
                        _ = SharedCoroutineStarter.Instance.StartCoroutine(this._chatImageProvider.TryCacheSingleImage(badge.Id, badge.Uri, ChatImageProvider.ESCAnimationType.NONE, (info) =>
                        {
                            if (info != null) {
                                if (!font.TryRegisterImageInfo(info, out var character)) {
                                    Logger.Warn($"Failed to register badge \"{badge.Id}\" in font {font.Font.name}.");
                                }
                            }
                            tcs.SetResult(info);
                        }, forcedHeight: 100));
                        tasks.Add(tcs.Task);
                    });
                }
            }
            // Wait on all the resources to be ready
            var result = await Task.WhenAll(tasks);
            return result.All(x => x != null);
        }

        public async Task<string> BuildMessage(IESCChatMessage msg, EnhancedFontInfo font, BuildMessageTarget buildMessage)
        {
            try {
                if (!await this.PrepareImages(msg, font)) {
                    Logger.Warn($"Failed to prepare some/all images for msg \"{msg.Message}\"!");
                    //return msg.Message;
                }
                var badgeInfos = new List<EnhancedImageInfo>();
                foreach (var badge in await this.GetDisplayBadgesAsync(msg)) {
                    if (!this._chatImageProvider.CachedImageInfo.TryGetValue(badge.Id, out var badgeInfo)) {
                        Logger.Warn($"Failed to find cached image info for badge \"{badge.Id}\"!");
                        continue;
                    }
                    badgeInfos.Add(badgeInfo);
                }
                var sb = buildMessage == BuildMessageTarget.Main ? new StringBuilder(msg.Message) : new StringBuilder(msg.SubMessage);
                IChatEmote gigantifiedEmote = null;
                if (msg.IsGigantifiedEmote) {
                    // Twitch only enlarges the last emote of a gigantified message.
                    foreach (var emote in msg.Emotes) {
                        if (emote is TwitchEmote twitchEmote && twitchEmote.Bits > 0) {
                            continue;
                        }
                        if (gigantifiedEmote == null || emote.EndIndex >= gigantifiedEmote.EndIndex) {
                            gigantifiedEmote = emote;
                        }
                    }
                }
                foreach (var emote in msg.Emotes) {
                    if (emote is TwitchEmote twitchEmote && 0 < twitchEmote.Bits) {
                        continue;
                    }
                    if (!this._chatImageProvider.CachedImageInfo.TryGetValue(emote.Id, out var replace)) {
                        Logger.Warn($"Emote {emote.Name} was missing from the emote dict! The request to {emote.Url} may have timed out?");
                        continue;
                    }
                    //Logger.Info($"replase id {replace.ImageId}");
                    //Logger.Info($"Emote: {emote.Name}, StartIndex: {emote.StartIndex}, EndIndex: {emote.EndIndex}, Len: {sb.Length}");
                    if (!font.TryGetCharacter(replace.ImageId, out var character)) {
                        Logger.Warn($"Emote {emote.Name} was missing from the character dict! Font hay have run out of usable characters.");
                        continue;
                    }
                    //Logger.Info($"target char {character}");
                    var emoteText = char.ConvertFromUtf32((int)character);
                    if (gigantifiedEmote != null && ReferenceEquals(emote, gigantifiedEmote)) {
                        // Use placeholders here so that the size tag is not affected by the rich text escaping below.
                        emoteText = $"{GigantifiedEmoteStart}{emoteText}{GigantifiedEmoteEnd}";
                    }
                    try {
                        // Replace emotes by index, in reverse order (msg.Emotes is sorted by emote.StartIndex in descending order)
                        if (Regex.IsMatch(emote.Id, "^Emoji_")) {
                            var charIndexText = Regex.Replace(emote.Id, "^Emoji_", "");
                            var emojiChars = charIndexText.Split('-').Select(x => char.ConvertFromUtf32(Convert.ToInt32($"0x{x}", 16)));
                            var emojiBuilder = new StringBuilder();
                            foreach (var emojiChar in emojiChars) {
                                _ = emojiBuilder.Append(emojiChar);
                            }
                            _ = sb.Replace(emojiBuilder.ToString(), emoteText);
                        }
                        else {
                            _ = sb.Replace(emote.Name, emoteText);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Error($"An unknown error occurred while trying to swap emote {emote.Name} into string of length {sb.Length} at location ({emote.StartIndex}, {emote.EndIndex})\r\n{ex}");
                    }
                }
                // Replace all instances of < with a zero-width non-breaking character so that user text cannot use rich text tags.
                _ = sb.Replace("<", "<\u2060");
                if (gigantifiedEmote != null) {
                    // Twitch displays the gigantified emote on a new line, separate from the rest of the message.
                    _ = sb.Replace(GigantifiedEmoteStart.ToString(), $"\n<size={this.GetGigantifiedEmoteScale()}%>");
                    _ = sb.Replace(GigantifiedEmoteEnd.ToString(), "</size>");
                }
                foreach (var emote in msg.Emotes) {
                    if (emote is not TwitchEmote twitchEmote || twitchEmote.Bits == 0) {
                        continue;
                    }
                    if (!this._chatImageProvider.CachedImageInfo.TryGetValue(emote.Id, out var replace)) {
                        Logger.Warn($"Emote {emote.Name} was missing from the emote dict! The request to {emote.Url} may have timed out?");
                        continue;
                    }
                    //Logger.Info($"replase id {replace.ImageId}");
                    //Logger.Info($"Emote: {emote.Name}, StartIndex: {emote.StartIndex}, EndIndex: {emote.EndIndex}, Len: {sb.Length}");
                    if (!font.TryGetCharacter(replace.ImageId, out var character)) {
                        Logger.Warn($"Emote {emote.Name} was missing from the character dict! Font hay have run out of usable characters.");
                        continue;
                    }
                    //Logger.Info($"target char {character}");
                    try {
                        // Replace emotes by index, in reverse order (msg.Emotes is sorted by emote.StartIndex in descending order)
                        _ = sb.Replace(emote.Name, $"{char.ConvertFromUtf32((int)character)}\u00A0<color={twitchEmote.Color}><size=77%><b>{twitchEmote.Bits}\u00A0</b></size></color>");
                    }
                    catch (Exception ex) {
                        Logger.Error($"An unknown error occurred while trying to swap emote {emote.Name} into string of length {sb.Length} at location ({emote.StartIndex}, {emote.EndIndex})\r\n{ex}");
                    }
                }
                if (buildMessage == BuildMessageTarget.Main && msg.IsSystemMessage) {
                    // System messages get a grayish color to differenciate them from normal messages in chat, and do not receive a username/badge prefix
                    _ = sb.Insert(0, $"<color=#bbbbbbff>");
                    _ = sb.Append("</color>");
                }
                else {
                    string sharedChatLabel = null;
                    if (buildMessage == BuildMessageTarget.Main && msg.IsSharedChatMessage && this._config.ShowSharedChatChannel) {
                        var sourceChannelName = await this._sharedChatResolver.GetChannelNameAsync(msg.SourceChannelId).ConfigureAwait(false);
                        sharedChatLabel = $"<color={SharedChatLabelColor}>[{sourceChannelName ?? "shared"}]</color> ";
                    }
                    var nameColorCode = msg.Sender?.Color;
                    if (ColorUtility.TryParseHtmlString(nameColorCode?.Substring(0, 7), out var nameColor)) {
                        if (nameColor == Color.white && !s_senderColor.TryGetValue(msg.Sender?.UserName, out nameColor)) {
                            nameColor = new Color((float)this._random.Next(0, 100000) / 100000, (float)this._random.Next(0, 100000) / 100000, (float)this._random.Next(0, 100000) / 100000);
                            _ = s_senderColor.TryAdd(msg.Sender?.UserName, nameColor);
                        }
                        Color.RGBToHSV(nameColor, out var h, out var s, out var v);
                        if (v < 0.85f) {
                            v = 0.85f;
                            nameColor = Color.HSVToRGB(h, s, v);
                        }
                        nameColorCode = ColorUtility.ToHtmlStringRGBA(nameColor);
                        nameColorCode = nameColorCode.Insert(0, "#");
                    }
                    if (msg.IsActionMessage) {
                        // Message becomes the color of their name if it's an action message
                        _ = sb.Insert(0, $"<color={nameColorCode}><b>{msg.Sender?.DisplayName}</b> ");
                        _ = sb.Append("</color>");
                    }
                    else {
                        // Insert username w/ color
                        _ = sb.Insert(0, $"<color={nameColorCode}><b>{msg.Sender?.DisplayName}</b></color>: ");
                    }
                    if (sharedChatLabel != null) {
                        // Inserted after the username and before the badges so the final order is badges, source channel, username.
                        _ = sb.Insert(0, sharedChatLabel);
                    }
                    var parsedBadge = new HashSet<string>();
                    for (var i = badgeInfos.Count - 1; i >= 0; i--) {
                        // Insert user badges at the beginning of the string in reverse order
                        var badge = badgeInfos[i];
                        if (parsedBadge.Contains(badge.ImageId)) {
                            continue;
                        }
                        _ = parsedBadge.Add(badge.ImageId);
                        if (badge != null && font.TryGetCharacter(badge.ImageId, out var character)) {
                            _ = sb.Insert(0, $"{char.ConvertFromUtf32((int)character)} ");
                        }
                        else {
                            Logger.Warn("Undefind badge");
                        }
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex) {
                Logger.Error($"An exception occurred in ChatMessageBuilder while parsing msg with {msg.Emotes.Count} emotes. Msg: \"{msg.Message}\". {ex}");
            }
            return msg.Message;
        }

        private async Task<IReadOnlyList<IChatBadge>> GetDisplayBadgesAsync(IESCChatMessage msg)
        {
            var sharedChatBadges = await this._sharedChatResolver.GetBadgesAsync(msg).ConfigureAwait(false);
            if (sharedChatBadges != null) {
                return sharedChatBadges;
            }
            if (msg.Sender is TwitchUser twitchUser) {
                return twitchUser.Badges;
            }
            return Array.Empty<IChatBadge>();
        }

        private int GetGigantifiedEmoteScale()
        {
            return Math.Min(500, Math.Max(100, this._config.GigantifiedEmoteScale));
        }
    }
}

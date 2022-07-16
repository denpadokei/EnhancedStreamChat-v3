using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Models.Twitch;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedStreamChat.Chat
{
    public class ChatMessageBuilder
    {
        public static ObjectMemoryPool<ConcurrentStack<EnhancedImageInfo>> ImageStackPool { get; }

        static ChatMessageBuilder()
        {
            ImageStackPool = new ObjectMemoryPool<ConcurrentStack<EnhancedImageInfo>>(5, null, null, null, f => f.Clear());
        }


        /// <summary>
        /// This function *blocks* the calling thread, and caches all the images required to display the message, then registers them with the provided font.
        /// </summary>
        /// <param name="msg">The chat message to get images from</param>
        /// <param name="font">The font to register these images to</param>
        public static bool PrepareImages(IChatMessage msg, EnhancedFontInfo font)
        {
            var tasks = new List<Task<EnhancedImageInfo>>();
            var pendingEmoteDownloads = new HashSet<string>();

            foreach (var emote in msg.Emotes) {
                if (string.IsNullOrEmpty(emote.Id) || pendingEmoteDownloads.Contains(emote.Id)) {
                    continue;
                }
                if (!font.CharacterLookupTable.ContainsKey(emote.Id)) {
                    pendingEmoteDownloads.Add(emote.Id);
                    var tcs = new TaskCompletionSource<EnhancedImageInfo>();
                    switch (emote.Type) {
                        case EmoteType.SingleImage:
                            SharedCoroutineStarter.instance.StartCoroutine(ChatImageProvider.instance.TryCacheSingleImage(emote.Id, emote.Uri, ChatImageProvider.ESCAnimationType.MAYBE_GIF, (info) =>
                            {
                                if (info != null) {
                                    if (!font.TryRegisterImageInfo(info, out var character)) {
                                        Logger.Warn($"Failed to register emote \"{emote.Id}\" in font {font.Font.name}.");
                                    }
                                }
                                tcs.SetResult(info);
                            }, forcedHeight: 110));
                            break;
                        case EmoteType.SpriteSheet:
                            SharedCoroutineStarter.instance.StartCoroutine(ChatImageProvider.instance.TryCacheSpriteSheetImage(emote.Id, emote.Uri, emote.UVs, (info) =>
                            {
                                if (info != null) {
                                    if (!font.TryRegisterImageInfo(info, out var character)) {
                                        Logger.Warn($"Failed to register emote \"{emote.Id}\" in font {font.Font.name}.");
                                    }
                                }
                                tcs.SetResult(info);
                            }, forcedHeight: 110));
                            break;
                        default:
                            tcs.SetResult(null);
                            break;
                    }
                    tasks.Add(tcs.Task);
                }
            }

            foreach (var badge in msg.Sender.Badges) {
                if (string.IsNullOrEmpty(badge.Id) || pendingEmoteDownloads.Contains(badge.Id)) {
                    continue;
                }

                if (!font.CharacterLookupTable.ContainsKey(badge.Id)) {
                    pendingEmoteDownloads.Add(badge.Id);
                    var tcs = new TaskCompletionSource<EnhancedImageInfo>();
                    SharedCoroutineStarter.instance.StartCoroutine(ChatImageProvider.instance.TryCacheSingleImage(badge.Id, badge.Uri, ChatImageProvider.ESCAnimationType.NONE, (info) =>
                    {
                        if (info != null) {
                            if (!font.TryRegisterImageInfo(info, out var character)) {
                                Logger.Warn($"Failed to register badge \"{badge.Id}\" in font {font.Font.name}.");
                            }
                        }
                        tcs.SetResult(info);
                    }, forcedHeight: 100));
                    tasks.Add(tcs.Task);
                }
            }

            // Wait on all the resources to be ready
            return Task.WaitAll(tasks.ToArray(), 15000);
        }

        public static Task<string> BuildMessage(IChatMessage msg, EnhancedFontInfo font)
        {
            return Task.Run(() =>
            {
                try {
                    if (!PrepareImages(msg, font)) {
                        Logger.Warn($"Failed to prepare some/all images for msg \"{msg.Message}\"!");
                        //return msg.Message;
                    }

                    var badges = ImageStackPool.Alloc();
                    foreach (var badge in msg.Sender.Badges) {
                        if (!ChatImageProvider.instance.CachedImageInfo.TryGetValue(badge.Id, out var badgeInfo)) {
                            Logger.Warn($"Failed to find cached image info for badge \"{badge.Id}\"!");
                            continue;
                        }
                        badges.Push(badgeInfo);
                    }

                    var sb = new StringBuilder(msg.Message); // Replace all instances of < with a zero-width non-breaking character

                    // Escape all html tags in the message
                    sb.Replace("<", "<\u2060");

                    foreach (var emote in msg.Emotes) {
                        if (!ChatImageProvider.instance.CachedImageInfo.TryGetValue(emote.Id, out var replace)) {
                            Logger.Warn($"Emote {emote.Name} was missing from the emote dict! The request to {emote.Uri} may have timed out?");
                            continue;
                        }
                        //Logger.Info($"Emote: {emote.Name}, StartIndex: {emote.StartIndex}, EndIndex: {emote.EndIndex}, Len: {sb.Length}");
                        if (!font.TryGetCharacter(replace.ImageId, out var character)) {
                            Logger.Warn($"Emote {emote.Name} was missing from the character dict! Font hay have run out of usable characters.");
                            continue;
                        }

                        try {
                            // Replace emotes by index, in reverse order (msg.Emotes is sorted by emote.StartIndex in descending order)
                            sb.Replace(emote.Name, emote switch
                            {
                                TwitchEmote t when t.Bits > 0 => $"{char.ConvertFromUtf32((int)character)}\u00A0<color={t.Color}><size=77%><b>{t.Bits}\u00A0</b></size></color>",
                                _ => char.ConvertFromUtf32((int)character)
                            },
                            emote.StartIndex, emote.EndIndex - emote.StartIndex + 1);
                        }
                        catch (Exception ex) {
                            Logger.Error($"An unknown error occurred while trying to swap emote {emote.Name} into string of length {sb.Length} at location ({emote.StartIndex}, {emote.EndIndex})\r\n{ex}");
                        }
                    }

                    if (msg.IsSystemMessage) {
                        // System messages get a grayish color to differenciate them from normal messages in chat, and do not receive a username/badge prefix
                        sb.Insert(0, $"<color=#bbbbbbbb>");
                        sb.Append("</color>");
                    }
                    else {
                        var nameColorCode = msg.Sender.Color;
                        Logger.Debug(nameColorCode);
                        if (ColorUtility.TryParseHtmlString(msg.Sender.Color.Substring(0, 7), out var nameColor)) {
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
                            sb.Insert(0, $"<color={nameColorCode}><b>{msg.Sender.DisplayName}</b> ");
                            sb.Append("</color>");
                        }
                        else {
                            // Insert username w/ color
                            sb.Insert(0, $"<color={nameColorCode}><b>{msg.Sender.DisplayName}</b></color>: ");
                        }

                        for (var i = 0; i < msg.Sender.Badges.Length; i++) {
                            // Insert user badges at the beginning of the string in reverse order
                            if (badges.TryPop(out var badge) && font.TryGetCharacter(badge.ImageId, out var character)) {
                                sb.Insert(0, $"{char.ConvertFromUtf32((int)character)} ");
                            }
                        }
                        ImageStackPool.Free(badges);
                    }
                    return sb.ToString();
                }
                catch (Exception ex) {
                    Logger.Error($"An exception occurred in ChatMessageBuilder while parsing msg with {msg.Emotes.Length} emotes. Msg: \"{msg.Message}\". {ex.ToString()}");
                }
                return msg.Message;
            });
        }
    }
}

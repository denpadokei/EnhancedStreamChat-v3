using BeatSaberMarkupLanguage.Animations;
using CatCore.Models;
using EnhancedStreamChat.Graphics;
using EnhancedStreamChat.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Networking;

namespace EnhancedStreamChat.Chat
{
    public class ActiveDownload
    {
        public bool IsCompleted = false;
        public UnityWebRequest Request;
        public Action<byte[]> Finally;
    }

    public class ChatImageProvider : PersistentSingleton<ChatImageProvider>
    {
        public ConcurrentDictionary<string, EnhancedImageInfo> CachedImageInfo { get; } = new ConcurrentDictionary<string, EnhancedImageInfo>();
        private readonly ConcurrentDictionary<string, ActiveDownload> _activeDownloads = new ConcurrentDictionary<string, ActiveDownload>();
        private readonly ConcurrentDictionary<string, Texture2D> _cachedSpriteSheets = new ConcurrentDictionary<string, Texture2D>();
        /// <summary>
        /// Retrieves the requested content from the provided Uri. 
        /// <para>
        /// The <paramref name="Finally"/> callback will *always* be called for this function. If it returns an empty byte array, that should be considered a failure.
        /// </para>
        /// </summary>
        /// <param name="uri">The resource location</param>
        /// <param name="Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="isRetry">Retry</param>
        public IEnumerator DownloadContent(string uri, Action<byte[]> Finally, bool isRetry = false)
        {
            if (string.IsNullOrEmpty(uri)) {
                Logger.Error($"URI is null or empty in request for resource {uri}. Aborting!");
                Finally?.Invoke(null);
                yield break;
            }

            if (!isRetry && this._activeDownloads.TryGetValue(uri, out var activeDownload)) {
                Logger.Info($"Request already active for {uri}");
                activeDownload.Finally -= Finally;
                activeDownload.Finally += Finally;
                yield return new WaitUntil(() => activeDownload.IsCompleted);
                yield break;
            }

            using (var wr = UnityWebRequest.Get(uri)) {
                activeDownload = new ActiveDownload()
                {
                    Finally = Finally,
                    Request = wr
                };
                this._activeDownloads.TryAdd(uri, activeDownload);

                yield return wr.SendWebRequest();
                if (wr.isHttpError) {
                    // Failed to download due to http error, don't retry
                    Logger.Error($"An http error occurred during request to {uri}. Aborting! {wr.error}");
                    activeDownload.Finally?.Invoke(new byte[0]);
                    this._activeDownloads.TryRemove(uri, out var d1);
                    yield break;
                }

                if (wr.isNetworkError) {
                    if (!isRetry) {
                        Logger.Error($"A network error occurred during request to {uri}. Retrying in 3 seconds... {wr.error}");
                        yield return new WaitForSeconds(3);
                        this.StartCoroutine(this.DownloadContent(uri, Finally, true));
                        yield break;
                    }
                    activeDownload.Finally?.Invoke(new byte[0]);
                    this._activeDownloads.TryRemove(uri, out var d2);
                    yield break;
                }

                var data = wr.downloadHandler.data;
                activeDownload.Finally?.Invoke(data);
                activeDownload.IsCompleted = true;
                this._activeDownloads.TryRemove(uri, out var d3);
            }
        }

        public IEnumerator PrecacheAnimatedImage(string uri, string id, int forcedHeight = -1)
        {
            yield return this.TryCacheSingleImage(id, uri, true);
        }


        private void SetImageHeight(ref int spriteHeight, ref int spriteWidth, int height)
        {
            var scale = 1.0f;
            if (spriteHeight != (float)height) {
                scale = (float)height / spriteHeight;
            }
            spriteWidth = (int)(scale * spriteWidth);
            spriteHeight = (int)(scale * spriteHeight);
        }

        public IEnumerator TryCacheSingleImage(string id, string uri, bool isAnimated, Action<EnhancedImageInfo> Finally = null, int forcedHeight = -1)
        {
            if (this.CachedImageInfo.TryGetValue(id, out var info)) {
                Finally?.Invoke(info);
                yield break;
            }
            var bytes = new byte[0];
            yield return this.DownloadContent(uri, (b) => bytes = b);
            yield return this.OnSingleImageCached(bytes, id, isAnimated, Finally, forcedHeight);
        }

        public IEnumerator OnSingleImageCached(byte[] bytes, string id, bool isAnimated, Action<EnhancedImageInfo> Finally = null, int forcedHeight = -1)
        {
            if (bytes.Length == 0) {
                Finally(null);
                yield break;
            }

            Sprite sprite = null;
            int spriteWidth = 0, spriteHeight = 0;
            AnimationControllerData animControllerData = null;
            if (isAnimated) {
                AnimationLoader.Process(AnimationType.GIF, bytes, (tex, atlas, delays, width, height) =>
                {
                    animControllerData = AnimationController.instance.Register(id, tex, atlas, delays);
                    sprite = animControllerData.sprite;
                    spriteWidth = width;
                    spriteHeight = height;
                });
                yield return new WaitUntil(() => animControllerData != null);
            }
            else {
                try {
                    sprite = GraphicUtils.LoadSpriteRaw(bytes);
                    spriteWidth = sprite.texture.width;
                    spriteHeight = sprite.texture.height;
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                    sprite = null;
                }
            }
            EnhancedImageInfo ret = null;
            if (sprite != null) {
                if (forcedHeight != -1) {
                    this.SetImageHeight(ref spriteWidth, ref spriteHeight, forcedHeight);
                }
                ret = new EnhancedImageInfo()
                {
                    ImageId = id,
                    Sprite = sprite,
                    Width = spriteWidth,
                    Height = spriteHeight,
                    AnimControllerData = animControllerData
                };
                this.CachedImageInfo.TryAdd(id, ret);
            }
            Finally?.Invoke(ret);
        }

        //public IEnumerator TryCacheSpriteSheetImage(string id, string uri, ImageRect rect, Action<EnhancedImageInfo> Finally = null, int forcedHeight = -1)
        //{
        //    if (this.CachedImageInfo.TryGetValue(id, out var info)) {
        //        Finally?.Invoke(info);
        //        yield break;
        //    }
        //    if (!this._cachedSpriteSheets.TryGetValue(uri, out var tex) || tex == null) {
        //        yield return this.DownloadContent(uri, (bytes) => tex = GraphicUtils.LoadTextureRaw(bytes));
        //        this._cachedSpriteSheets[uri] = tex;
        //    }
        //    this.CacheSpriteSheetImage(id, rect, tex, Finally, forcedHeight);
        //}

        //private void CacheSpriteSheetImage(string id, ImageRect rect, Texture2D tex, Action<EnhancedImageInfo> Finally = null, int forcedHeight = -1)
        //{
        //    if (tex == null) {
        //        Finally?.Invoke(null);
        //        return;
        //    }
        //    int spriteWidth = rect.Width, spriteHeight = rect.Height;
        //    var sprite = Sprite.Create(tex, new Rect(rect.X, tex.height - rect.Y - spriteHeight, spriteWidth, spriteHeight), new Vector2(0, 0));
        //    sprite.texture.wrapMode = TextureWrapMode.Clamp;
        //    EnhancedImageInfo ret = null;
        //    if (sprite != null) {
        //        if (forcedHeight != -1) {
        //            this.SetImageHeight(ref spriteWidth, ref spriteHeight, forcedHeight);
        //        }
        //        ret = new EnhancedImageInfo()
        //        {
        //            ImageId = id,
        //            Sprite = sprite,
        //            Width = spriteWidth,
        //            Height = spriteHeight,
        //            AnimControllerData = null
        //        };
        //        this.CachedImageInfo.TryAdd(id, ret);
        //    }
        //    Finally?.Invoke(ret);
        //}

        internal static void ClearCache()
        {
            if (instance.CachedImageInfo.Count > 0) {
                foreach (var info in instance.CachedImageInfo.Values) {
                    Destroy(info.Sprite);
                }
                instance.CachedImageInfo.Clear();
            }
        }
    }
}

using EnhancedStreamChat.Chat;
using EnhancedStreamChat.Interfaces;
using EnhancedStreamChat.Utilities;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedTextMeshProUGUI : TextMeshProUGUI
    {
        public IESCChatMessage ChatMessage { get; set; } = null;
        public EnhancedFontInfo FontInfo => this._fontManager.FontInfo;

        private MemoryPoolContainer<EnhancedImage> _imagePool;
        private ESCFontManager _fontManager;
        private bool _rebuiled = false;
        private readonly LazyCopyHashSet<ILatePreRenderRebuildReciver> _recivers = new LazyCopyHashSet<ILatePreRenderRebuildReciver>();
        public ILazyCopyHashSet<ILatePreRenderRebuildReciver> LazyCopyHashSet => this._recivers;

        public void Constract(EnhancedImage.Pool image, ESCFontManager fontManager)
        {
            this._imagePool = new MemoryPoolContainer<EnhancedImage>(image);
            this._fontManager = fontManager;
        }

        protected override void Awake()
        {
            base.Awake();
            this.raycastTarget = false;
        }

        public void ClearImages()
        {
            try {
                while (this._imagePool?.activeItems != null && this._imagePool?.activeItems.Any() == true) {
                    this._imagePool?.Despawn(this._imagePool.activeItems[0]);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        public override void Rebuild(CanvasUpdate update)
        {
            switch (update) {
                case CanvasUpdate.LatePreRender:
                    MainThreadInvoker.Invoke(() =>
                    {
                        this.ClearImages();
                        for (var i = 0; i < this.textInfo.characterCount; i++) {
                            var c = this.textInfo.characterInfo[i];
                            if (!c.isVisible || string.IsNullOrEmpty(this.text) || c.index >= this.text.Length) {
                                // Skip invisible/empty/out of range chars
                                continue;
                            }

                            uint character = this.text[c.index];
                            if (c.index + 1 < this.text.Length && char.IsSurrogatePair(this.text[c.index], this.text[c.index + 1])) {
                                // If it's a surrogate pair, convert the character
                                character = (uint)char.ConvertToUtf32(this.text[c.index], this.text[c.index + 1]);
                            }
                            if (this.FontInfo == null || !this.FontInfo.TryGetImageInfo(character, out var imageInfo) || imageInfo is null) {
                                //Logger.Debug($"{c.character}:{character}, Skip characters that have no imageInfo registered");
                                continue;
                            }
                            var img = this._imagePool?.Spawn();
                            if (img == null) {
                                continue;
                            }
                            try {
                                img.rectTransform.SetParent(this.rectTransform, false);
                                if (imageInfo.AnimControllerData != null) {
                                    img.AnimStateUpdater.controllerData = imageInfo.AnimControllerData;
                                    img.sprite = imageInfo.AnimControllerData.sprites[imageInfo.AnimControllerData.uvIndex];
                                }
                                else {
                                    img.sprite = imageInfo.Sprite;
                                }
                                img.rectTransform.localScale = new Vector3(this.fontScale * 1.08f, this.fontScale * 1.08f, this.fontScale * 1.08f);
                                img.rectTransform.sizeDelta = new Vector2(imageInfo.Width, imageInfo.Height);
                                img.rectTransform.localPosition = c.topLeft - new Vector3(0, imageInfo.Height * this.fontScale * 0.558f / 2);
                                img.rectTransform.localRotation = Quaternion.identity;
                                img.material = BeatSaberUtils.UINoGlowMaterial;
                                img.SetAllDirty();
                            }
                            catch (Exception ex) {
                                Logger.Error($"Exception while trying to overlay sprite. {ex}");
                                this._imagePool?.Despawn(img);
                            }
                        }
                        this._rebuiled = true;
                    });
                    break;
                case CanvasUpdate.Prelayout:
                case CanvasUpdate.Layout:
                case CanvasUpdate.PostLayout:
                case CanvasUpdate.PreRender:
                case CanvasUpdate.MaxUpdateValue:
                default:
                    break;
            }
            base.Rebuild(update);
        }

        public void AddReciver(ILatePreRenderRebuildReciver reciver)
        {
            this.LazyCopyHashSet.Add(reciver);
        }

        public void RemoveReciver(ILatePreRenderRebuildReciver reciver)
        {
            this.LazyCopyHashSet.Remove(reciver);
        }

        protected void LateUpdate()
        {
            if (this._rebuiled) {
                foreach (var reciver in this._recivers.items) {
                    reciver?.LatePreRenderRebuildHandler(this, EventArgs.Empty);
                }
                this._rebuiled = false;
            }
        }

        public class Factory : PlaceholderFactory<EnhancedTextMeshProUGUI>
        {
        }
    }
}

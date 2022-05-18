using BeatSaberMarkupLanguage.Animations;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedImage : Image
    {
        public AnimationStateUpdater AnimStateUpdater { get; set; } = null;
        public class Pool : MonoMemoryPool<EnhancedImage>
        {
            protected override void OnCreated(EnhancedImage img)
            {
                base.OnCreated(img);
                img.raycastTarget = false;
                img.color = Color.white;
                img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                img.rectTransform.pivot = new Vector2(0, 0);
                img.AnimStateUpdater = img.gameObject.GetComponent<AnimationStateUpdater>();
                img.AnimStateUpdater.image = img;
                img.SetAllDirty();
            }

            protected override void OnDespawned(EnhancedImage img)
            {
                if (img == null || img.gameObject == null) {
                    return;
                }
                try {
                    img.AnimStateUpdater.controllerData = null;
                    img.sprite = null;
                    img.SetAllDirty();
                    base.OnDespawned(img);
                }
                catch (Exception ex) {
                    Logger.Error($"Exception while freeing EnhancedImage in EnhancedTextMeshProUGUI. {ex}");
                }
            }
        }
    }
}

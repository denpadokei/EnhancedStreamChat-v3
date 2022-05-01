using BeatSaberMarkupLanguage.Animations;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedImage : Image
    {
        public AnimationStateUpdater animStateUpdater { get; set; } = null;

        public class Pool : MonoMemoryPool<EnhancedImage>
        {
            protected override void OnCreated(EnhancedImage img)
            {
                base.Reinitialize(img);
                img.gameObject.SetActive(false);
                img.raycastTarget = false;
                img.color = Color.white;
                img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                img.rectTransform.pivot = new Vector2(0, 0);
                img.animStateUpdater = img.gameObject.GetComponent<AnimationStateUpdater>();
                img.animStateUpdater.image = img;
                img.SetAllDirty();
            }

            protected override void OnDespawned(EnhancedImage img)
            {
                base.OnDespawned(img);
                try {
                    img.gameObject.SetActive(false);
                    img.animStateUpdater.controllerData = null;
                    img.rectTransform.SetParent(null);
                    img.sprite = null;
                }
                catch (Exception ex) {
                    Logger.Error($"Exception while freeing EnhancedImage in EnhancedTextMeshProUGUI. {ex.ToString()}");
                }
            }
        }
    }
}

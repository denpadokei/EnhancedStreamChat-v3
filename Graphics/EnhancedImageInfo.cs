using BeatSaberMarkupLanguage.Animations;
using System.Collections.Concurrent;
using UnityEngine;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedImageInfo
    {
        public string ImageId { get; internal set; }
        public Sprite Sprite { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public AnimationControllerData AnimControllerData { get; internal set; }

        public class Pool : MemoryPool<ConcurrentStack<EnhancedImageInfo>>
        {
            protected override void OnDespawned(ConcurrentStack<EnhancedImageInfo> item)
            {
                base.OnDespawned(item);
                item.Clear();
            }
        }
    }
}

using BeatSaberMarkupLanguage.Animations;
using EnhancedStreamChat.CatCoreWrapper;
using EnhancedStreamChat.Chat;
using EnhancedStreamChat.Graphics;
using HMUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Installers
{
    internal class ESCAppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<CatCoreManager>().AsCached().NonLazy();
            this.Container.BindMemoryPool<EnhancedImageInfo, EnhancedImageInfo.Pool>().WithInitialSize(20);
            this.Container.BindMemoryPool<EnhancedImage, EnhancedImage.Pool>().WithInitialSize(20).FromComponentInNewPrefab(this._image);
            this.Container.BindMemoryPool<EnhancedTextMeshProUGUI, EnhancedTextMeshProUGUI.Pool>().WithInitialSize(20).FromComponentInNewPrefab(this._enhancedTextMeshProUGUI);
            this.Container.BindMemoryPool<EnhancedTextMeshProUGUIWithBackground, EnhancedTextMeshProUGUIWithBackground.Pool>().WithInitialSize(64).FromComponentInNewPrefab(this._enhancedTextMeshProUGUIWithBackground);
            this.Container.BindInterfacesAndSelfTo<ChatMessageBuilder>().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<ESCFontManager>().AsSingle().NonLazy();
            this.Container.BindInterfacesAndSelfTo<ChatImageProvider>().AsSingle();
        }

        private readonly GameObject _image = new GameObject(nameof(EnhancedImage), typeof(AnimationStateUpdater), typeof(EnhancedImage));
        private readonly GameObject _enhancedTextMeshProUGUI = new GameObject(nameof(EnhancedTextMeshProUGUI), typeof(EnhancedTextMeshProUGUI));
        private readonly GameObject _enhancedTextMeshProUGUIWithBackground = new GameObject(nameof(EnhancedTextMeshProUGUIWithBackground), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(ImageView), typeof(EnhancedTextMeshProUGUIWithBackground));
    }
}

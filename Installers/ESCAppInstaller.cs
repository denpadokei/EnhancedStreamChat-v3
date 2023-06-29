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
            _ = this.Container.BindInterfacesAndSelfTo<CatCoreManager>().AsCached().NonLazy();
            _ = this.Container.BindMemoryPool<EnhancedImageInfo, EnhancedImageInfo.Pool>().WithInitialSize(20);
            _ = this.Container.BindMemoryPool<EnhancedImage, EnhancedImage.Pool>().WithInitialSize(50).FromComponentInNewPrefab(this._image);
            _ = this.Container.BindFactory<EnhancedTextMeshProUGUI, EnhancedTextMeshProUGUI.Factory>().FromMethod(this.CreateText);
            _ = this.Container.BindMemoryPool<EnhancedTextMeshProUGUIWithBackground, EnhancedTextMeshProUGUIWithBackground.Pool>().WithInitialSize(64).FromComponentInNewPrefab(this._enhancedTextMeshProUGUIWithBackground);
            _ = this.Container.BindInterfacesAndSelfTo<ChatMessageBuilder>().AsCached().NonLazy();
            _ = this.Container.BindInterfacesAndSelfTo<ESCFontManager>().AsSingle().NonLazy();
            _ = this.Container.BindInterfacesAndSelfTo<ChatImageProvider>().AsSingle();
        }

        private EnhancedTextMeshProUGUI CreateText(DiContainer container)
        {
            var result = Instantiate(this._enhancedTextMeshProUGUI).GetComponent<EnhancedTextMeshProUGUI>();
            result.Constract(container.Resolve<EnhancedImage.Pool>(), container.Resolve<ESCFontManager>());
            return result;
        }

        private readonly GameObject _image = new GameObject(nameof(EnhancedImage), typeof(RectTransform), typeof(AnimationStateUpdater), typeof(EnhancedImage));
        private readonly GameObject _enhancedTextMeshProUGUI = new GameObject(nameof(EnhancedTextMeshProUGUI), typeof(RectTransform), typeof(EnhancedTextMeshProUGUI));
        private readonly GameObject _enhancedTextMeshProUGUIWithBackground = new GameObject(nameof(EnhancedTextMeshProUGUIWithBackground), typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(ImageView), typeof(EnhancedTextMeshProUGUIWithBackground));
    }
}

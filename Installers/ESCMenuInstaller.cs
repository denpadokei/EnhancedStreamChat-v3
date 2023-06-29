using EnhancedStreamChat.Chat;
using Zenject;

namespace EnhancedStreamChat.Installers
{
    internal class ESCMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesAndSelfTo<ChatDisplay>().FromNewComponentAsViewController().AsSingle().NonLazy();
        }
    }
}

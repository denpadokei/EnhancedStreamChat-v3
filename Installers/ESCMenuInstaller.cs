using EnhancedStreamChat.Chat;
using Zenject;

namespace EnhancedStreamChat.Installers
{
    internal class ESCMenuInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesAndSelfTo<ChatDisplay>().FromNewComponentAsViewController().AsSingle().NonLazy();
        }
    }
}

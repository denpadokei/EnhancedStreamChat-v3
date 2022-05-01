using EnhancedStreamChat.Chat;
using Zenject;

namespace EnhancedStreamChat.Installers
{
    internal class ESCMenuAndGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<ChatDisplay>().FromNewComponentAsViewController().AsSingle().NonLazy();
        }
    }
}

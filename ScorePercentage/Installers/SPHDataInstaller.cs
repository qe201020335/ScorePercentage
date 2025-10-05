using ScorePercentage.Data;
using Zenject;

namespace ScorePercentage.Installers;

internal class SPHDataInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SPHDataProvider>().AsSingle();
    }
}
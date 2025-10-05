using ScorePercentage.Data;
using Zenject;

namespace ScorePercentage.Installers;

internal class SongPlayDataInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SongPlayDataProvider>().AsSingle();
    }
}
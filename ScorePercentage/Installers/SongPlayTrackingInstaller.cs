using ScorePercentage.Data;
using Zenject;

namespace ScorePercentage.Installers;

public class SongPlayTrackingInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SongPlayTracker>().AsSingle();
    }
}
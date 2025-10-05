using ScorePercentage.Patches;
using Zenject;

namespace ScorePercentage.Installers;

internal class MenuInstaller : Installer
{
    [Inject]
    private readonly PluginConfig _config = null!;

    public override void InstallBindings()
    {
        if (!_config.EnableScorePercentage) return;

        if (_config.ShowPercentageAtMenuHighScore)
        {
            Container.BindInterfacesTo<LevelStatsViewPatch>().AsSingle();
        }

        if (_config.ShowPercentageAtLevelEnd || _config.ShowScoreDifferenceAtLevelEnd)
        {
            Container.BindInterfacesTo<ResultsViewControllerPatch>().AsSingle();
        }

        if (_config.ShowPercentageAtMultiplayerResults)
        {
            Container.BindInterfacesTo<MultiplayerResultsTablePatch>().AsSingle();
        }
    }
}
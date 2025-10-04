using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using JetBrains.Annotations;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Zenject;

namespace ScorePercentage.Menu;

[Bind(Location.Menu)]
[UsedImplicitly]
internal class SettingsController : IInitializable
{
    [Inject]
    private readonly Plugin _plugin = null!;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly BSMLSettings _bsmlSettings = null!;

    void IInitializable.Initialize()
    {
        _bsmlSettings.AddSettingsMenu("Score Percentage", "ScorePercentage.Menu.Settings.bsml", this);
    }

    [UIValue("enable-score-percentage")]
    public bool EnableScorePercentage
    {
        get => _config.EnableScorePercentage;
        set => _config.EnableScorePercentage = value;
    }

    [UIValue("highscore-percentage")]
    public bool ShowPercentageAtMenuHighScore
    {
        get => _config.ShowPercentageAtMenuHighScore;
        set => _config.ShowPercentageAtMenuHighScore = value;
    }

    [UIValue("result-percentage")]
    public bool ShowPercentageAtLevelEnd
    {
        get => _config.ShowPercentageAtLevelEnd;
        set => _config.ShowPercentageAtLevelEnd = value;
    }

    [UIValue("result-score-diff")]
    public bool ShowScoreDifferenceAtLevelEnd
    {
        get => _config.ShowScoreDifferenceAtLevelEnd;
        set => _config.ShowScoreDifferenceAtLevelEnd = value;
    }

    [UIValue("result-percentage-diff")]
    public bool ShowPercentageDifferenceAtLevelEnd
    {
        get => _config.ShowPercentageDifferenceAtLevelEnd;
        set => _config.ShowPercentageDifferenceAtLevelEnd = value;
    }

    [UIValue("multi-result-percentage")]
    public bool ShowPercentageAtMultiplayerResults
    {
        get => _config.ShowPercentageAtMultiplayerResults;
        set => _config.ShowPercentageAtMultiplayerResults = value;
    }

    [UIValue("multiplayer-info-installed")]
    public bool MultiplayerInfoInstalled => _plugin.MultiplayerInfoInstalled;
}
using System.Runtime.CompilerServices;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace ScorePercentage;

internal class PluginConfig
{
    public static PluginConfig Instance { get; private set; } = null!;

    public static void Initialize(Config config)
    {
        Instance = config.Generated<PluginConfig>();
    }

    public virtual bool EnableScorePercentage { get; set; } = true;

    // make it compatible with old ScorePercentage config
    [SerializedName("EnableMenuHighscore")]
    public virtual bool ShowPercentageAtMenuHighScore { get; set; } = true;

    [SerializedName("EnableLevelEndRank")]
    public virtual bool ShowPercentageAtLevelEnd { get; set; } = true;

    [SerializedName("EnableScoreDifference")]
    public virtual bool ShowScoreDifferenceAtLevelEnd { get; set; } = true;

    [SerializedName("EnableScorePercentageDifference")]
    public virtual bool ShowPercentageDifferenceAtLevelEnd { get; set; } = true;

    [SerializedName("EnableMultiplayerSupport")]
    public virtual bool ShowPercentageAtMultiplayerResults { get; set; } = true;
}
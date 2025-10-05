namespace ScorePercentage.Data;

internal class LevelResultsExtraData
{
    public bool EnergyDidReach0 { get; set; }

    public int ModifiedScore { get; set; }

    public int MaxMultipliedScore { get; set; }

    public int? PreviousHighScore { get; set; }
}
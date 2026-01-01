using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SongPlayHistory.SongPlayData;
using SongPlayHistory.SongPlayTracking;
using Zenject;

namespace ScorePercentage.Data;

internal class SPHDataProvider : IDataProvider
{
    [Inject]
    private readonly IScoringCacheManager _scoringCacheManager = null!;

    [Inject]
    private readonly ExtraCompletionDataManager _extraCompletionDataManager = null!;

    [Inject]
    private readonly IRecordManager _recordManager = null!;

    public int GetHistoryHighScore(BeatmapKey beatmapKey)
    {
        var records = _recordManager.GetRecords(beatmapKey);
        return records.Count == 0 ? 0 : records.Max(record => record.ModifiedScore);
    }

    public async Task<int> GetMaxMultipliedScore(BeatmapKey beatmapKey, CancellationToken token)
    {
        var info = await _scoringCacheManager.GetScoringInfo(beatmapKey, null, token);
        return info.MaxMultipliedScore;
    }

    public void AddLevelCompletionResultsExtraData(LevelCompletionResults results, LevelResultsExtraData extraData)
    {
        // do nothing
    }

    public LevelResultsExtraData? GetLevelCompletionResultsExtraData(LevelCompletionResults results)
    {
        var sphExtraData = _extraCompletionDataManager.GetExtraData(results);
        if (sphExtraData == null) return null;
        var previousStats = sphExtraData.PreviousPlayerLevelStats;
        return new LevelResultsExtraData
        {
            EnergyDidReach0 = sphExtraData.EnergyDidReach0,
            MaxMultipliedScore = sphExtraData.ScoringData.MaxRawScore,
            ModifiedScore = sphExtraData.ScoringData.ModifiedScore,
            PreviousHighScore = previousStats?.validScore == true ? previousStats.highScore : null
        };
    }
}
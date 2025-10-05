using System.Threading;
using System.Threading.Tasks;

namespace ScorePercentage.Data;

internal interface IDataProvider
{
    int GetHistoryHighScore(BeatmapKey beatmapKey);

    Task<int> GetMaxMultipliedScore(BeatmapKey beatmapKey, CancellationToken token);

    LevelResultsExtraData? GetLevelCompletionResultsExtraData(LevelCompletionResults results);
}
using System.Threading;
using System.Threading.Tasks;

namespace ScorePercentage.Data;

internal interface IDataProvider
{
    int GetHistoryHighScore(BeatmapKey beatmapKey);

    Task<int> GetMaxMultipliedScore(BeatmapKey beatmapKey, CancellationToken token);

    void AddLevelCompletionResultsExtraData(LevelCompletionResults results, LevelResultsExtraData extraData);

    LevelResultsExtraData? GetLevelCompletionResultsExtraData(LevelCompletionResults results);
}
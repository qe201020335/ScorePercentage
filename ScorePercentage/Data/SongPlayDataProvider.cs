using System.Threading;
using System.Threading.Tasks;

namespace ScorePercentage.Data;

internal class SongPlayDataProvider : IDataProvider
{
    public int GetHistoryHighScore(BeatmapKey beatmapKey) => 0;

    //TODO
    public Task<int> GetMaxMultipliedScore(BeatmapKey beatmapKey, CancellationToken token) => Task.FromResult(0);

    //TODO
    public LevelResultsExtraData? GetLevelCompletionResultsExtraData(LevelCompletionResults results) => null;
}
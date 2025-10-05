using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IPA.Logging;
using IPA.Utilities;
using Zenject;

namespace ScorePercentage.Data;

internal class SongPlayDataProvider : IDataProvider
{
    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly BeatmapLevelsModel _beatmapLevelsModel = null!;

    [Inject]
    private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel = null!;

    [Inject]
    private readonly BeatmapDataLoader _beatmapDataLoader = null!;

    private readonly Dictionary<BeatmapKey, int> _cache = new();

    public int GetHistoryHighScore(BeatmapKey beatmapKey) => 0;

    public async Task<int> GetMaxMultipliedScore(BeatmapKey beatmapKey, CancellationToken token)
    {
        if (!UnityGame.OnMainThread) throw new InvalidOperationException("Must be called on main thread");
        if (_cache.TryGetValue(beatmapKey, out var cached)) return cached;

#if DEBUG
        var startTime = DateTime.Now;
#endif

        var beatmapLevel = _beatmapLevelsModel.GetBeatmapLevel(beatmapKey.levelId);
        if (beatmapLevel == null)
        {
            _logger.Error("Failed to get BeatmapLevel.");
            throw new Exception("Failed to get BeatmapLevel.");
        }

        _logger.Trace("Loading beat map level data from BeatmapLevelsModel.");
        var dataVersion = await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapKey.levelId, token);
        var loadResult = await _beatmapLevelsModel.LoadBeatmapLevelDataAsync(beatmapKey.levelId, dataVersion, token);
        if (loadResult.isError)
        {
            _logger.Error("Failed to load BeatmapLevelData.");
            throw new Exception("Failed to load BeatmapLevelData.");
        }

        var beatmapLevelData = loadResult.beatmapLevelData!;

        var beatmapData = await _beatmapDataLoader.LoadBeatmapDataAsync(
            beatmapLevelData,
            beatmapKey,
            beatmapLevel.beatsPerMinute,
            false,
            null,
            null,
            dataVersion,
            null,
            null,
            false);

        token.ThrowIfCancellationRequested();

        if (beatmapData == null)
        {
            _logger.Error("Failed to get BeatmapData.");
            throw new Exception("Failed to get BeatmapData.");
        }

        var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);

#if DEBUG
        var timeSpan = DateTime.Now - startTime;
        _logger.Info(
            $"Took {timeSpan.TotalSeconds:F3} sec to load scoring data for {beatmapLevel.songName} ({beatmapKey.beatmapCharacteristic.compoundIdPartName}{beatmapKey.difficulty.SerializedName()})");
#endif

        _cache[beatmapKey] = maxScore;
        return maxScore;
    }

    //TODO
    public LevelResultsExtraData? GetLevelCompletionResultsExtraData(LevelCompletionResults results) => null;
}
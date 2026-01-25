using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Logging;
using IPA.Utilities.Async;
using ScorePercentage.Data;
using SiraUtil.Affinity;
using TMPro;
using Zenject;

namespace ScorePercentage.Patches;

internal class LevelStatsViewPatch : IAffinity, IDisposable
{
    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly IDataProvider _dataProvider = null!;

    private CancellationTokenSource? _cts;

    void IDisposable.Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats), AffinityMethodType.Normal, null,
        typeof(PlayerLevelStatsData))]
    private void SetLevelStats(LevelStatsView __instance, PlayerLevelStatsData playerLevelStats)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        var cts = new CancellationTokenSource();
        _cts = cts;

        var highScore = playerLevelStats.validScore ? playerLevelStats.highScore : 0;
        var beatmapKey = playerLevelStats.GetBeatmapKey();

        if (highScore <= 0)
        {
            // use history data to show high score
            highScore = _dataProvider.GetHistoryHighScore(beatmapKey);
            __instance._highScoreText.text = highScore.ToString();
        }

        if (highScore > 0) ShowScorePercentage(__instance._highScoreText, beatmapKey, highScore, cts.Token);
    }

    private void ShowScorePercentage(TMP_Text text, BeatmapKey key, int highScore, CancellationToken token)
    {
        _logger.Trace($"Showing high score percentage for {key.levelId}");
        _dataProvider.GetMaxMultipliedScore(key, token)
            .ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    _logger.Warn("Failed to show percentage for high score.");
                    _logger.Warn(task.Exception);
                    return;
                }

                var maxScore = task.Result;
                if (maxScore <= 0) return;
                var percentage = (float)highScore / maxScore * 100;
                if (token.IsCancellationRequested) return;
                text.text = $"{highScore} ({percentage:0.00}%)";
            }, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, UnityMainThreadTaskScheduler.Default);
    }
}
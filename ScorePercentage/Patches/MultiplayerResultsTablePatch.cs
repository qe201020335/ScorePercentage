using System;
using System.Threading;
using System.Threading.Tasks;
using HMUI;
using IPA.Logging;
using IPA.Utilities.Async;
using ScorePercentage.Data;
using SiraUtil.Affinity;
using Zenject;

namespace ScorePercentage.Patches;

internal class MultiplayerResultsTablePatch : IAffinity, IDisposable
{
    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly IDataProvider _dataProvider = null!;

    private CancellationTokenSource? _cts;

    private Task<int>? _maxScoreTask;

    void IDisposable.Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    [AffinityPatch(typeof(MultiplayerResultsViewController), nameof(MultiplayerResultsViewController.Init))]
    [AffinityPrefix]
    private void MultiResultsGetBeatmapKey(BeatmapKey beatmapKey)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _maxScoreTask = beatmapKey.IsValid() ? _dataProvider.GetMaxMultipliedScore(beatmapKey, token) : null;
    }

    [AffinityPatch(typeof(ResultsTableView), nameof(ResultsTableView.CellForIdx))]
    [AffinityPostfix]
    private void MultiResultsTableGetCell(ResultsTableView __instance, TableCell? __result, int idx)
    {
        if (__result == null || __result is not ResultsTableCell cell)
        {
            _logger.Warn("Cell is null or not ResultsTableCell");
            return;
        }

        var multiResult = __instance._dataList[idx];
        var cacheTask = _maxScoreTask;

        if (cacheTask == null || cacheTask.IsCanceled) return;

        if (cacheTask.IsFaulted)
        {
            _logger.Warn("Failed to get scoring cache, can't show score percentage");
            if (cacheTask.Exception != null) _logger.Debug(cacheTask.Exception);
            return;
        }

        if (cacheTask.IsCompleted)
        {
            var cache = cacheTask.Result;
            ShowDataOnResultsTableCell(cell, multiResult, cache);
        }
        else
        {
            cacheTask.ContinueWith(task => ShowDataOnResultsTableCell(cell, multiResult, task.Result),
                CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion,
                UnityMainThreadTaskScheduler.Default);
        }
    }

    private void ShowDataOnResultsTableCell(ResultsTableCell cell, MultiplayerPlayerResultsData multiData, int maxScore)
    {
        var multiResults = multiData.multiplayerLevelCompletionResults;
        if (!multiResults.hasAnyResults ||
            multiResults.playerLevelEndState !=
            MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndState.SongFinished
            || multiResults.levelCompletionResults.levelEndStateType !=
            LevelCompletionResults.LevelEndStateType.Cleared)
        {
            return;
        }

        var results = multiData.multiplayerLevelCompletionResults.levelCompletionResults;
        var percentage = results.multipliedScore / (float)maxScore * 100;
        cell._scoreText.text = $"{ScoreFormatter.Format(results.cumulativeScore)} ({percentage:F2}%)";
        var min = cell._scoreText.rectTransform.offsetMin;
        min.x = -42;
        cell._scoreText.rectTransform.offsetMin = min;
    }
}
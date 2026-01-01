using System;
using IPA.Logging;
using ScorePercentage.Utilitilities;
using Zenject;

namespace ScorePercentage.Data;

internal class SongPlayTracker : IInitializable, IDisposable
{
    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly IDataProvider _dataProvider = null!;

    [Inject]
    private readonly GameplayCoreSceneSetupData _sceneSetupData = null!;

    [Inject]
    private readonly IPlayerDataModel _playerDataModel = null!;

    [Inject]
    private readonly IScoreController _scoreController = null!;

    [Inject]
    private readonly IGameEnergyCounter _energyCounter = null!;

    // This type is actually binded in PCInit so it will never be null
    [InjectOptional]
    private readonly StandardLevelScenesTransitionSetupDataSO? _standardTransitionData = null;

    // Use this instead of MultiplayerLevelScenesTransitionSetupDataSO to trigger result gathering
    // as soon as the player fail and/or becomes inactive, instead of waiting for return to lobby.
    [InjectOptional]
    private readonly IMultiplayerLevelEndActionsPublisher? _multiplayerLevelEndActions = null;

    private int? _previousHighScore = null;

    private bool _energyDidReach0 = false;

    void IInitializable.Initialize()
    {
        _logger.Trace("Initializing SongPlayTracker");

        if (ReplayCheck.IsInReplay())
        {
            _logger.Info("This is a replay, not tracking.");
            return;
        }

        if (_standardTransitionData != null) // this will always be true
        {
            _standardTransitionData.didFinishEvent += OnStandardLevelDidFinish;
        }

        if (_multiplayerLevelEndActions != null)
        {
            _multiplayerLevelEndActions.playerDidFinishEvent += OnMultiplayerLevelDidFinish;
        }

        _previousHighScore = _playerDataModel.playerData.TryGetPlayerLevelStatsData(_sceneSetupData.beatmapKey)
            ?.highScore;

        _energyCounter.gameEnergyDidReach0Event -= OnEnergyDidReach0;
        _energyCounter.gameEnergyDidReach0Event += OnEnergyDidReach0;
    }

    void IDisposable.Dispose()
    {
        _logger.Trace("Disposing SongPlayTracker");
        if (_standardTransitionData != null)
        {
            _standardTransitionData.didFinishEvent -= OnStandardLevelDidFinish;
        }

        if (_multiplayerLevelEndActions != null)
        {
            _multiplayerLevelEndActions.playerDidFinishEvent -= OnMultiplayerLevelDidFinish;
        }
    }

    private void OnEnergyDidReach0()
    {
        _energyDidReach0 = true;
    }

    private void OnStandardLevelDidFinish(StandardLevelScenesTransitionSetupDataSO? data,
        LevelCompletionResults? results)
    {
        _logger.Trace("Standard level finished");
        if (data == null)
        {
            _logger.Warn("StandardLevelScenesTransitionSetupDataSO is null.");
            return;
        }

        HandleLevelFinished(results);
    }

    private void OnMultiplayerLevelDidFinish(MultiplayerLevelCompletionResults? results)
    {
        _logger.Trace("Multi level finished");
        if (results == null)
        {
            _logger.Warn("MultiplayerLevelCompletionResults is null.");
            return;
        }

        HandleLevelFinished(results.levelCompletionResults);
    }

    private void HandleLevelFinished(LevelCompletionResults? results)
    {
        if (results == null)
        {
            _logger.Warn("LevelCompletionResults is null.");
            return;
        }

        if (ReplayCheck.IsInReplay())
        {
            _logger.Debug("It was a replay, ignored.");
            return;
        }

        _logger.Info("Standard/Multi level finished, preparing extra results data");

        var extraData = new LevelResultsExtraData
        {
            EnergyDidReach0 = _energyDidReach0,
            ModifiedScore = _scoreController.modifiedScore,
            MaxMultipliedScore = _scoreController.immediateMaxPossibleMultipliedScore,
            PreviousHighScore = _previousHighScore
        };

        _dataProvider.AddLevelCompletionResultsExtraData(results, extraData);
    }
}
using IPA.Logging;
using ScorePercentage.Data;
using SiraUtil.Affinity;
using TMPro;
using Zenject;

namespace ScorePercentage.Patches;

internal class ResultsViewControllerPatch : IAffinity
{
    private const string ColorPositive = "#00B300";
    private const string ColorNegative = "#FF0000";

    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IDataProvider _dataProvider = null!;

    [AffinityPostfix]
    [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.SetDataToUI))]
    private void ShowPercentageAndDiff(ResultsViewController __instance)
    {
        var results = __instance._levelCompletionResults;

        if (results.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) return;

        _logger.Debug("Showing score percentage on results view");

        var extraData = _dataProvider.GetLevelCompletionResultsExtraData(results);

        if (extraData == null)
        {
            _logger.Warn("LevelCompletionResultsExtraData is null. Cannot show score percentage.");
            return;
        }

        if (extraData.ModifiedScore != results.modifiedScore)
        {
            // should not happen but just in case
            _logger.Warn("Score from extra data somehow does not match the results. Cannot show score percentage.");
            _logger.Debug($"ExtraData : {extraData.ModifiedScore}");
            _logger.Debug($"Result: Multiplied ({results.multipliedScore}), Modified ({results.modifiedScore})");
            return;
        }

        int resultScore;

        // Old Score Percentage uses multiplied score for "positive" modifiers so it doesn't exceed 100% and modified score for "negative" modifiers
        // However the high score from player data is always the modified score so there will be cases where the percentage diff doesn't make sense
        // Unless we use SPH's records for high score, but it may not always be consistent with the player data
        // condition copied from old Score Percentage
        if ((results.gameplayModifiers.noFailOn0Energy && extraData.EnergyDidReach0)
            || results.gameplayModifiers.enabledObstacleType != GameplayModifiers.EnabledObstacleType.All
            || results.gameplayModifiers.noArrows
            || results.gameplayModifiers.noBombs
            || results.gameplayModifiers.zenMode
            || results.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower
           )
        {
            resultScore = results.modifiedScore;
        }
        else
        {
            resultScore = results.multipliedScore;
        }

        var maxScore = extraData.MaxMultipliedScore;
        var resultScorePercentage = resultScore / (float)maxScore * 100;
        var previousHighScore = extraData.PreviousHighScore ?? -1;
        _logger.Debug("Previous high score: " + previousHighScore);

        __instance._rankText.autoSizeTextContainer = false;
        __instance._rankText.textWrappingMode = TextWrappingModes.NoWrap;

        if (previousHighScore <= 0)
        {
            // no previous high score, show percentage only
            if (_config.ShowPercentageAtLevelEnd)
            {
                // rich text formatting copied from old Score Percentage
                __instance._rankText.text = $"<line-height=27.5%><size=60%>{resultScorePercentage:F2}<size=45%>%";
            }
        }
        else
        {
            var scoreDiff = resultScore - previousHighScore;
            var color = scoreDiff >= 0 ? ColorPositive : ColorNegative;

            var rankText = __instance._rankText;
            var scoreText = __instance._scoreText;

            // show percentage with difference
            var highScorePercentage = previousHighScore / (float)maxScore * 100;
            var percentDiff = resultScorePercentage - highScorePercentage;


            if (_config.ShowPercentageAtLevelEnd)
            {
                var text = $"<line-height=27.5%><size=60%>{resultScorePercentage:F2}<size=45%>%";
                if (_config.ShowPercentageDifferenceAtLevelEnd)
                {
                    text += $"\n<color={color}><size=40%>{percentDiff:+0.00;-0.00;+0}<size=30%>%";
                }

                rankText.text = text;
            }

            if (_config.ShowScoreDifferenceAtLevelEnd)
            {
                __instance._newHighScoreText.SetActive(false);
                // show score with difference
                scoreText.text =
                    $"<line-height=27.5%><size=60%>{ScoreFormatter.Format(resultScore)}\n<size=40%><color={color}><size=40%>{scoreDiff:+#;-#;+0}";
            }
        }
    }
}
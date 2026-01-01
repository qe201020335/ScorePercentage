using System;
using System.Reflection;

namespace ScorePercentage.Utilitilities;

public static class ReplayCheck
{
    private static readonly Lazy<MethodBase?> ScoreSaber_playbackEnabled = new(() =>
    {
        var meta = Plugin.Instance.SSMetadata;
        if (meta == null)
        {
            Plugin.Logger.Info("ScoreSaber is not installed or disabled");
            return null;
        }

        var method = meta.Assembly.GetType("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted")
            ?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null)
        {
            Plugin.Logger.Warn("ScoreSaber replay check method not found");
            return null;
        }

        return method;
    });

    private static readonly Lazy<MethodBase?> GetBeatLeaderIsStartedAsReplay = new(() =>
    {
        var meta = Plugin.Instance.BLMetadata;
        if (meta == null)
        {
            Plugin.Logger.Info("BeatLeader is not installed or disabled");
            return null;
        }

        var method = meta.Assembly.GetType("BeatLeader.Replayer.ReplayerLauncher")
            ?.GetProperty("IsStartedAsReplay", BindingFlags.Static | BindingFlags.Public)?.GetGetMethod(false);

        if (method == null)
        {
            Plugin.Logger.Warn("BeatLeader ReplayerLauncher.IsStartedAsReplay not found");
            return null;
        }

        return method;
    });

    internal static bool IsInReplay()
    {
        var ssReplay = ScoreSaber_playbackEnabled.Value != null &&
                       (bool)ScoreSaber_playbackEnabled.Value.Invoke(null, null) == false;

        var blReplay = GetBeatLeaderIsStartedAsReplay.Value != null &&
                       (bool)GetBeatLeaderIsStartedAsReplay.Value.Invoke(null, null);

        return ssReplay || blReplay;
    }
}
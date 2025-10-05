using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ScorePercentage.Patches;

//Modified from MappingExtensions
[HarmonyPatch(typeof(BeatmapObjectsInTimeRowProcessor),
    nameof(BeatmapObjectsInTimeRowProcessor.HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice))]
internal static class BeatmapLoadingPatch
{
    private static readonly MethodInfo ClampMethod = AccessTools.Method(typeof(Math), nameof(Math.Clamp),
        new[] { typeof(int), typeof(int), typeof(int) });

    [HarmonyPrepare]
    // MappingExtensions' patch will throw if we happen to patch it first
    private static bool Prepare() => !Plugin.Instance.MappingExtensionsInstalled;

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructs = new List<CodeInstruction>(instructions);
        var patched = instructs.Exists(instruction => instruction.Calls(ClampMethod));
        if (patched)
        {
            Plugin.Logger.Debug("BeatmapObjectsInTimeRowProcessor is already patched. Skipping ScorePercentage patch");
            return instructs;
        }

        Plugin.Logger.Debug("Patching BeatmapObjectsInTimeRowProcessor");
        // Prevents an IndexOutOfRangeException when processing precise line indexes.
        var matcher = new CodeMatcher(instructs);
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldelem_Ref))
            .ThrowIfInvalid("Failed to patch BeatmapObjectsInTimeRowProcessor.")
            .Insert(
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Call, ClampMethod));

        Plugin.Logger.Debug("BeatmapObjectsInTimeRowProcessor patched");
        return matcher.InstructionEnumeration();
    }
}
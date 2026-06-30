using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;

namespace Sts2ConceptMap.Patches;

/// <summary>
/// Prefix on the private <c>StandardActMap.GenerateMap()</c> — lets a concept shape the map's routes
/// (not just node types). The walks use a concept-seeded RNG, so each concept produces a DIFFERENT
/// topology (and choice-rich concepts trace more paths, forced ones fewer). Deterministic per concept+
/// act. On disable/error we fall back to vanilla so a map is always produced; downstream AssignPointTypes
/// / PruneAndRepair / post-processing in the constructor still run, keeping connectivity the game's job.
/// </summary>
[HarmonyPatch(typeof(StandardActMap), "GenerateMap")]
internal static class StandardActMap_GenerateMap_Patch
{
    // BuildConceptPaths returns true when it generated the map (skip vanilla); false → run vanilla.
    private static bool Prefix(StandardActMap __instance)
        => !ConceptMapService.BuildConceptPaths(__instance);
}

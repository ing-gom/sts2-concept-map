using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Sts2ConceptMap.Patches;

/// <summary>
/// Postfix on the <see cref="StandardActMap"/> constructor — the final point after the map has been
/// generated, pruned, and post-processed (so the node types we write are not overwritten). We re-type
/// every non-exempt node to match the act's rolled concept, producing a visible, themed map.
///
/// Constructor matched by its full parameter list (same shape Random Map targets):
/// (Rng, ActModel, bool isMultiplayer, bool shouldReplaceTreasureWithElites, bool hasSecondBoss,
///  MapPointTypeCounts? override, bool enablePruning).
/// </summary>
[HarmonyPatch(typeof(StandardActMap), MethodType.Constructor,
    new Type[]
    {
        typeof(Rng), typeof(ActModel), typeof(bool), typeof(bool), typeof(bool),
        typeof(MapPointTypeCounts), typeof(bool)
    })]
internal static class StandardActMap_Constructor_Patch
{
    private static void Postfix(StandardActMap __instance)
    {
        ConceptMapService.LastStats = "ctor postfix fired (AssignToMap not yet returned)";
        ConceptMapService.AssignToMap(__instance);
    }
}

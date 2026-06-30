using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;

namespace Sts2ConceptMap.Patches;

/// <summary>
/// Postfix on the <see cref="SpoilsActMap"/> constructor — the treasure-map ("Spoils") event builds its
/// own hourglass map (a separate <see cref="ActMap"/> subclass, NOT StandardActMap), so the main
/// constructor patch never fires for it. We re-type its nodes to the act's concept here too. The central
/// treasure node is marked <c>CanBeModified = false</c> by the game, so <c>ConceptMapService.IsExempt</c>
/// leaves it (and its row) intact — only the paths above/below get themed.
/// </summary>
[HarmonyPatch]
internal static class SpoilsActMap_Constructor_Patch
{
    // Resolve the (single) SpoilsActMap constructor dynamically — specifying typeof(IRunState) in the
    // attribute failed to match (Harmony "method null"), so target the declared ctor directly instead.
    private static MethodBase TargetMethod() =>
        AccessTools.GetDeclaredConstructors(typeof(SpoilsActMap)).First();

    private static void Postfix(SpoilsActMap __instance) =>
        ConceptMapService.AssignToMap(__instance);
}

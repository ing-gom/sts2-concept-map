using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Sts2ConceptMap.Patches;

/// <summary>
/// Every time the map screen opens, make sure the concept banner is attached (child of NMapScreen →
/// visibility follows the screen) and refreshed for the current act's concept.
/// </summary>
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
internal static class NMapScreenOpenPatch
{
    private static void Postfix(NMapScreen __instance)
    {
        try
        {
            var banner = __instance.GetNodeOrNull<ConceptBanner>(ConceptBanner.NodeName);
            if (banner == null)
                __instance.AddChild(new ConceptBanner()); // _Ready() refreshes itself
            else
                banner.Refresh();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[{MainFile.ModId}] map-open banner hook failed: {ex.Message}");
        }
    }
}

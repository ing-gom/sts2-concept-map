using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ConceptMap.Patches;

/// <summary>
/// Postfix on <c>ActModel.PullNextEvent(RunState)</c> — the method that resolves which event a "?" node shows
/// (it internally calls <c>Hook.ModifyNextEvent</c> then marks the event visited). We patch PullNextEvent
/// itself rather than the tiny <c>Hook.ModifyNextEvent</c> because that small static method is prone to being
/// JIT-inlined at its call site, which would make a Harmony patch on it silently never run.
///
/// The game has already marked its default pick visited by the time this postfix runs; when we swap in a
/// different event we also mark the chosen one visited so it won't immediately repeat. (The skipped default
/// stays flagged, which is fine — it simply won't reappear.)
/// </summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEvent))]
internal static class ActModel_PullNextEvent_Patch
{
    /// <summary>How many times this postfix has actually executed (for `conceptmap stats` — 0 ⇒ never fired).</summary>
    internal static int Calls;

    private static void Postfix(RunState runState, ref EventModel __result)
    {
        Calls++;
        var original = __result;
        var chosen = EventConceptMatcher.Match(runState, original);
        if (chosen != null && original != null && !chosen.Id.Equals(original.Id))
        {
            runState.AddVisitedEvent(chosen); // keep the shown event from repeating next "?"
            __result = chosen;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ConceptMap;

/// <summary>
/// Biases which EVENT a "?" (Unknown) node resolves to so it leans toward the current map's concept — without
/// ever excluding anything. STS2 draws the next event from a per-act shuffled queue (<c>ActModel.PullNextEvent</c>
/// → <c>Hook.ModifyNextEvent</c>); we postfix that hook and re-pick from the act's full event pool with a
/// WEIGHTED roll: events whose theme fits the concept are more likely, events that clash are less likely, and
/// every event keeps a non-zero floor so ALL of them can still appear.
///
/// Constraints (by design, honest about the game's limits):
///  • The pool is the CURRENT act's events (+ shared events). We never pull events from other acts
///    (assets/act-gating), so weighting only reshuffles within this act's ~10-13 event pool.
///  • Weighted, not filtered: fit = ×<see cref="Boost"/>, clash = ×<see cref="Penalty"/>, everything ≥
///    <see cref="Floor"/> — off-concept events are rarer, never impossible. Unthemed concepts (balanced,
///    crossroads, wild, labyrinth) are left fully vanilla.
///  • Visited events are skipped (like the game) to avoid repeats; on exhaustion we return the game's default.
///  • Deterministic (concept + seed + act + visited-count), so multiplayer clients and reloads agree.
///
/// Event themes were hand-tagged from the decompiled event outcomes (what each option grants: fight / relic /
/// gold / heal / card / potion / gamble / curse). See <see cref="_eventTags"/>.
/// </summary>
internal static class EventConceptMatcher
{
    private static readonly bool _disabled =
        Environment.GetEnvironmentVariable("STS2_CONCEPT_MAP_DISABLED") == "1" ||
        Environment.GetEnvironmentVariable("STS2_CONCEPT_MAP_EVENT_MATCH") == "0";

    [Flags]
    private enum Tag
    {
        None = 0,
        Combat = 1 << 0, Shop = 1 << 1, Heal = 1 << 2, Treasure = 1 << 3, Gold = 1 << 4,
        Card = 1 << 5, Potion = 1 << 6, Gamble = 1 << 7, Curse = 1 << 8, Lore = 1 << 9,
    }

    // Weight multipliers for the concept-fit roll. Fit events are Boost× as likely as neutral; clashing ones
    // Penalty× (rarer, never zero); nothing drops below Floor so every event stays possible.
    private const float Boost = 3.0f;
    private const float Penalty = 0.3f;
    private const float Floor = 0.15f;

    /// <summary>Concept key → (event themes it PREFERS, event themes it AVOIDS). Preferred themes are weighted
    /// up, avoided themes down; everything else is neutral. Only concepts with a clear identity are listed —
    /// the rest (balanced, crossroads, wild, labyrinth) keep fully vanilla event variety.</summary>
    private static readonly Dictionary<string, (Tag prefer, Tag avoid)> _conceptBias = new()
    {
        // combat / danger concepts → prefer fights & HP-risk; avoid cozy shop/heal detours
        ["woods"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["hunt"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["trial"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["beast"] = (Tag.Combat, Tag.Shop),                 // beast has campfires → heal is on-theme, don't avoid
        ["oneway"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["fire"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["grinder"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["gauntlet"] = (Tag.Combat, Tag.Shop | Tag.Heal),
        ["dread"] = (Tag.Combat | Tag.Curse, Tag.Shop | Tag.Heal),
        // economy → prefer shop/gold; a brawl on the market street clashes
        ["market"] = (Tag.Shop | Tag.Potion | Tag.Gold, Tag.Combat),
        ["merchant"] = (Tag.Shop | Tag.Gold, Tag.Combat),
        ["smuggler"] = (Tag.Shop | Tag.Treasure, Tag.Heal),
        // reward
        ["trove"] = (Tag.Treasure | Tag.Gold, Tag.Combat),
        ["hoard"] = (Tag.Treasure | Tag.Combat, Tag.Heal),  // elite-guarded vault → no restful comfort
        // gamble → prefer RNG events; nothing strongly clashes in a den of chance
        ["gamble"] = (Tag.Gamble, Tag.None),
        // calm / recovery → prefer heal/lore; fights & curses break the peace
        ["sanctuary"] = (Tag.Heal, Tag.Combat | Tag.Curse),
        ["pilgrim"] = (Tag.Heal | Tag.Card, Tag.Combat | Tag.Curse),
        // lore / card-craft
        ["story"] = (Tag.Card | Tag.Lore, Tag.Combat),
        ["wanderer"] = (Tag.Card | Tag.Heal, Tag.Combat),
    };

    /// <summary>Event class name → its gameplay themes (hand-tagged from decompiled outcomes).</summary>
    private static readonly Dictionary<string, Tag> _eventTags = new(StringComparer.Ordinal)
    {
        // ── Act 1 ──
        ["AromaOfChaos"] = Tag.Card,
        ["ByrdonisNest"] = Tag.Heal | Tag.Card,
        ["DenseVegetation"] = Tag.Gold | Tag.Combat | Tag.Heal,
        ["JungleMazeAdventure"] = Tag.Gold | Tag.Gamble,
        ["LuminousChoir"] = Tag.Shop | Tag.Treasure | Tag.Curse,
        ["MorphicGrove"] = Tag.Heal | Tag.Card,
        ["SapphireSeed"] = Tag.Heal | Tag.Card,
        ["SunkenStatue"] = Tag.Treasure | Tag.Gold,
        ["TabletOfTruth"] = Tag.Heal | Tag.Card | Tag.Gamble,
        ["UnrestSite"] = Tag.Heal | Tag.Curse | Tag.Treasure,
        ["Wellspring"] = Tag.Potion | Tag.Card | Tag.Curse,
        ["WhisperingHollow"] = Tag.Shop | Tag.Potion | Tag.Card,
        ["WoodCarvings"] = Tag.Card,
        ["AbyssalBaths"] = Tag.Heal | Tag.Gamble,
        ["DrowningBeacon"] = Tag.Potion | Tag.Treasure,
        ["EndlessConveyor"] = Tag.Shop | Tag.Gamble,
        ["PunchOff"] = Tag.Combat | Tag.Treasure | Tag.Curse,
        ["SpiralingWhirlpool"] = Tag.Card | Tag.Heal,
        ["SunkenTreasury"] = Tag.Gold | Tag.Curse,
        ["DoorsOfLightAndDark"] = Tag.Card,
        ["TrashHeap"] = Tag.Treasure | Tag.Gold | Tag.Card,
        ["WaterloggedScriptorium"] = Tag.Shop | Tag.Card | Tag.Heal,
        // ── Act 2 ──
        ["Amalgamator"] = Tag.Card,
        ["Bugslayer"] = Tag.Card,
        ["ColorfulPhilosophers"] = Tag.Card,
        ["ColossalFlower"] = Tag.Gold | Tag.Treasure | Tag.Combat,
        ["FieldOfManSizedHoles"] = Tag.Card | Tag.Curse,
        ["InfestedAutomaton"] = Tag.Card,
        ["LostWisp"] = Tag.Treasure | Tag.Gold | Tag.Curse,
        ["SpiritGrafter"] = Tag.Heal | Tag.Card | Tag.Combat,
        ["TheLanternKey"] = Tag.Combat | Tag.Gold,
        ["ZenWeaver"] = Tag.Shop | Tag.Card,
        // ── Act 3 ──
        ["BattlewornDummy"] = Tag.Combat | Tag.Treasure | Tag.Potion,
        ["GraveOfTheForgotten"] = Tag.Treasure | Tag.Card | Tag.Curse,
        ["HungryForMushrooms"] = Tag.Treasure | Tag.Combat,
        ["Reflections"] = Tag.Card | Tag.Curse,
        ["RoundTeaParty"] = Tag.Treasure | Tag.Heal | Tag.Combat,
        ["Trial"] = Tag.Gamble | Tag.Curse | Tag.Treasure,
        ["TinkerTime"] = Tag.Card,
        // ── Shared (any act) ──
        ["BrainLeech"] = Tag.Card | Tag.Combat,
        ["CrystalSphere"] = Tag.Gamble | Tag.Curse,
        // (Darv is an AncientEvent — act-opener shrine, pulled via PullAncient, never a "?" pool event — so
        //  it is intentionally NOT tagged here; it can't reach the ModifyNextEvent hook.)
        ["DollRoom"] = Tag.Treasure | Tag.Combat,
        ["FakeMerchant"] = Tag.Shop | Tag.Combat,
        ["PotionCourier"] = Tag.Potion,
        ["RanwidTheElder"] = Tag.Treasure | Tag.Potion,
        ["RelicTrader"] = Tag.Treasure,
        ["RoomFullOfCheese"] = Tag.Card | Tag.Treasure,
        ["SelfHelpBook"] = Tag.Card,
        ["SlipperyBridge"] = Tag.Card | Tag.Combat,
        ["StoneOfAllTime"] = Tag.Heal | Tag.Card,
        ["Symbiote"] = Tag.Card,
        ["TeaMaster"] = Tag.Shop | Tag.Treasure,
        ["TheFutureOfPotions"] = Tag.Potion | Tag.Card,
        ["TheLegendsWereTrue"] = Tag.Card | Tag.Potion,
        ["ThisOrThat"] = Tag.Treasure | Tag.Curse,
        ["WarHistorianRepy"] = Tag.Treasure | Tag.Potion,
        ["WelcomeToWongos"] = Tag.Shop | Tag.Treasure,
    };

    /// <summary>Diagnostic: last swap decision (for the `conceptmap stats` console command).</summary>
    public static string LastEvent = "(no event pulled yet)";

    private static Tag TagsOf(EventModel? e) =>
        e != null && _eventTags.TryGetValue(e.GetType().Name, out var t) ? t : Tag.None;

    /// <summary>Concept-fit weight for one event: neutral 1, ×Boost if it fits, ×Penalty if it clashes,
    /// floored so it never reaches zero (every event stays reachable).</summary>
    private static float WeightFor(EventModel e, Tag prefer, Tag avoid)
    {
        var t = TagsOf(e);
        float w = 1f;
        if ((t & prefer) != Tag.None) w *= Boost;
        if ((t & avoid) != Tag.None) w *= Penalty;
        return Math.Max(w, Floor);
    }

    /// <summary>
    /// Given the game's default next event, return one drawn from the act's pool with a concept-weighted roll
    /// (all events possible; fitting ones likelier, clashing ones rarer). Called from the
    /// <c>Hook.ModifyNextEvent</c> postfix. Never throws — any failure returns the default unchanged.
    /// </summary>
    public static EventModel Match(IRunState runState, EventModel defaultEvent)
    {
        if (_disabled) { LastEvent = "disabled (env STS2_CONCEPT_MAP_EVENT_MATCH=0)"; return defaultEvent!; }
        if (defaultEvent == null) { LastEvent = "default event was null"; return defaultEvent!; }
        try
        {
            var concept = ConceptMapService.CurrentConcept();
            if (concept == null) { LastEvent = "no current concept (map not themed / state null)"; return defaultEvent; }
            if (!_conceptBias.TryGetValue(concept.Key, out var bias))
            {
                LastEvent = $"{concept.Key}: no event bias (vanilla variety kept)";
                return defaultEvent; // unthemed concept → leave the game's pick alone
            }

            var state = RunManager.Instance?.State;
            if (state == null) { LastEvent = $"{concept.Key}: RunManager state null"; return defaultEvent; }

            var visited = state.VisitedEventIds;
            // Pool = this act's events + shared, deduped, allowed, unseen — but the game already marked its
            // default pick visited before this runs, so keep the default eligible (so "keep default" is possible).
            var pool = state.Act.AllEvents.Concat(ModelDb.AllSharedEvents)
                .Where(e => e != null)
                .GroupBy(e => e.Id).Select(g => g.First())
                .Where(e => (!visited.Contains(e.Id) || e.Id.Equals(defaultEvent.Id)) && e.IsAllowed(runState))
                .ToList();
            if (pool.Count == 0) { LastEvent = $"{concept.Key}: pool empty, kept {defaultEvent.GetType().Name}"; return defaultEvent; }

            // Weighted, deterministic roll (varies per node via visited-count so consecutive ?s differ).
            uint seed = state.Rng?.Seed ?? 0u;
            int act = state.CurrentActIndex;
            var rng = new Rng(seed, $"sts2conceptmap_event_{concept.Key}_act{act}_{visited.Count}");

            float total = 0f;
            foreach (var e in pool) total += WeightFor(e, bias.prefer, bias.avoid);
            float roll = rng.NextFloat() * total, acc = 0f;
            EventModel chosen = pool[pool.Count - 1];
            foreach (var e in pool)
            {
                acc += WeightFor(e, bias.prefer, bias.avoid);
                if (roll < acc) { chosen = e; break; }
            }

            LastEvent = chosen.Id == defaultEvent.Id
                ? $"{concept.Key}: kept {defaultEvent.GetType().Name} (weighted, prefer {bias.prefer})"
                : $"{concept.Key}: {defaultEvent.GetType().Name} → {chosen.GetType().Name} (weighted, prefer {bias.prefer})";
            return chosen;
        }
        catch (Exception ex)
        {
            LastEvent = $"match FAILED: {ex.GetType().Name}: {ex.Message}";
            MainFile.Logger.Warn($"[{MainFile.ModId}] event match failed: {ex.Message}");
            return defaultEvent;
        }
    }
}

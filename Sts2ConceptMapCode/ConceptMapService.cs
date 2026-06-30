using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ConceptMap;

/// <summary>
/// Core logic for Concept Maps. Each freshly generated act map is given a "concept" — a flavour like
/// Elite Gauntlet, Storyteller's Path, or Merchant's Run — and its nodes are re-typed to match, so the
/// map is VISIBLE and themed (you can see it and plan your route). Concepts carry a difficulty tier and
/// are gated by act: Act 1 draws easy concepts, Act 3 draws hard ones.
///
/// Unlike the sister mod Random Map (which hides every node behind a ? and rolls the room on entry),
/// this assigns concrete <see cref="MapPointType"/>s up front. There is no entry-time roll. Events have
/// no standalone map node in StS2 — the "?" (Unknown) node is the event/mystery tile — so a concept's
/// Event weight is realised as <see cref="MapPointType.Unknown"/> nodes (which the game resolves to an
/// event ~most of the time on entry).
///
/// Determinism / multiplayer: both the concept choice and the per-node assignment are driven by an Rng
/// seeded from the run seed + act index, so every player (and a reloaded save) gets the same themed map.
/// </summary>
internal static class ConceptMapService
{
    private static readonly bool _disabled =
        Environment.GetEnvironmentVariable("STS2_CONCEPT_MAP_DISABLED") == "1";

    public const string SurpriseKey = "surprise";

    // Slot order used by every concept's multiplier array and by BaseTarget.
    // Index:        0 Monster   1 Event(=Unknown)   2 RestSite   3 Elite   4 Shop   5 Treasure
    private static readonly MapPointType[] Slot =
    {
        MapPointType.Monster, MapPointType.Unknown, MapPointType.RestSite,
        MapPointType.Elite, MapPointType.Shop, MapPointType.Treasure,
    };

    // Direct-sampling base distribution (NOT pity-calibrated — this mod samples each node independently).
    // These are the "Balanced" target appearance frequencies; concept multipliers skew from here.
    private static readonly float[] BaseTarget = { 0.40f, 0.18f, 0.14f, 0.12f, 0.08f, 0.08f };

    // Rest anti-drought. Pure independent sampling can leave a map nearly campfire-less, especially on
    // rest-poor concepts (Famine, Meatgrinder…). We deliberately add NO general pity — clumping is what
    // keeps dominant concepts characterful — and only guarantee a small minimum number of rest nodes per
    // map (a post-pass that no-ops when a concept already has enough rest). The pre-boss rest is exempt
    // and always present on top of this.
    private const float MinRestFraction = 0.08f;
    private const int MinRests = 2;

    // Treasure (relic) rooms are powerful — each one entered grants a free relic, and vanilla gives just
    // ONE per act. Cap the number of EXTRA treasure nodes a concept may add (the fixed centre treasure is
    // exempt and always present, so total relic rooms = 1 + this). Excess treasure is turned into shops
    // ("loot route" flavour, and gold-gated so it stays balanced). Prevents a relic flood / grab-bag drain.
    private const int MaxExtraTreasure = 4;

    /// <summary>A map flavour: a unique <see cref="Emoji"/> (stable, language-independent), an English
    /// fallback name, per-slot weight multipliers (× <see cref="BaseTarget"/>), and a difficulty
    /// <see cref="Tier"/> (1 easy … 3 hard) used for act gating. Display name is localized via
    /// <see cref="LabelOf"/>; the emoji is what dropdown selections are matched against.</summary>
    public sealed record MapConcept(string Key, string Emoji, string EnName, int Tier, float[] Mult);

    // Multipliers indexed Monster / Event / Rest / Elite / Shop / Treasure. Each emoji is unique.
    public static readonly MapConcept[] Concepts =
    {
        // ── Tier 1 (easy — Act 1 mostly) ──────────────────────────────────────────────
        new("wanderer", "\U0001F9ED", "Wanderer's Path",    1, new[] { 0.6f, 2.2f, 1.6f, 0.3f, 0.8f, 0.8f }),
        new("market",   "\U0001F3EA", "Market Street",      1, new[] { 0.6f, 1.8f, 1.3f, 0.3f, 2.5f, 1.5f }),
        new("woods",    "\U0001F332", "Quiet Woods",        1, new[] { 1.4f, 0.7f, 1.6f, 0.15f, 0.6f, 0.6f }),
        new("trove",    "\U0001F5FA", "Treasure Trail",     1, new[] { 0.7f, 1.5f, 1.2f, 0.4f, 0.8f, 3f }),
        new("story",    "\U0001F4D6", "Storyteller's Path", 1, new[] { 0.35f, 4.5f, 1.2f, 0.2f, 0.5f, 0.5f }),

        // ── Tier 2 (medium — Act 2 mostly) ────────────────────────────────────────────
        new("balanced", "⚖",      "Balanced",          2, new[] { 1f, 1f, 1f, 1f, 1f, 1f }),
        new("crossroads","\U0001F500","Crossroads",         2, new[] { 1.6f, 1.8f, 0.8f, 0.1f, 0.15f, 0.15f }),
        new("hunt",     "\U0001F3F9", "Hunting Grounds",    2, new[] { 2f, 0.6f, 1.1f, 0.5f, 0.5f, 0.5f }),
        new("merchant", "\U0001F4B0", "Merchant's Run",     2, new[] { 0.7f, 1f, 0.9f, 0.6f, 4.5f, 1.5f }),
        new("wild",     "\U0001F3B2", "Wildlands",          2, new[] { 0.42f, 0.93f, 1.19f, 1.39f, 2.08f, 2.08f }),
        new("trial",    "⚔",      "Trial Road",        2, new[] { 1.2f, 0.7f, 0.8f, 2.2f, 0.6f, 0.8f }),

        // ── Tier 3 (hard — Act 3+ mostly) ─────────────────────────────────────────────
        new("gauntlet", "\U0001F6E1", "Elite Gauntlet",     3, new[] { 0.3f, 0.15f, 1.0f, 6f, 0.7f, 0.4f }),
        new("fire",     "\U0001F525", "Trial by Fire",      3, new[] { 1.3f, 0.6f, 0.45f, 3f, 0.4f, 0.6f }),
        new("grinder",  "⚙",      "The Meatgrinder",   3, new[] { 2.2f, 0.3f, 0.3f, 2.5f, 0.3f, 0.4f }),
        new("famine",   "\U0001F3DC", "Famine",             3, new[] { 2f, 0.8f, 0.2f, 1.8f, 0.1f, 0.3f }),
        new("dread",    "\U0001F480", "Dread Domain",       3, new[] { 1.6f, 0.4f, 0.35f, 3.5f, 0.2f, 0.3f }),
    };

    /// <summary>Display label for a concept: emoji + current-language name (English fallback).</summary>
    public static string LabelOf(MapConcept c) => $"{c.Emoji} {Localization.Strings.Get("name_" + c.Key)}";

    // Path density per concept (number of path-tracing walks; vanilla = 7). Conservative: choice-rich
    // concepts get MORE routes (safe — just more edges); forced concepts get slightly FEWER (down to 6,
    // not lower, to avoid under-connecting the map). Concepts not listed use the vanilla 7.
    private static readonly Dictionary<string, int> _pathCounts = new()
    {
        ["crossroads"] = 10, ["wanderer"] = 10, ["story"] = 9, ["market"] = 9,
        ["merchant"] = 8, ["trove"] = 8, ["wild"] = 8,
        ["gauntlet"] = 6, ["grinder"] = 6, ["dread"] = 6, ["famine"] = 6,
    };

    public static int PathsFor(MapConcept c) => _pathCounts.TryGetValue(c.Key, out var n) ? n : 7;

    // Per-concept step bias for the path walk. Weights are for steps [-1, 0, +1] (left / straight / right).
    // "straight" → narrow, forced-feeling routes; "wide" → sprawling, branchy routes. We only bias the
    // ORDER the three options are tried in — the game's crossover check still validates each — so maps
    // stay connected. Concepts not listed walk neutrally (≈ vanilla).
    private static readonly float[] _wStraight = { 1f, 3f, 1f };
    private static readonly float[] _wWide = { 2f, 1f, 2f };
    private static readonly float[] _wNeutral = { 1f, 1f, 1f };

    private static readonly Dictionary<string, float[]> _stepBias = new()
    {
        ["gauntlet"] = _wStraight, ["famine"] = _wStraight, ["grinder"] = _wStraight,
        ["dread"] = _wStraight, ["fire"] = _wStraight,
        ["crossroads"] = _wWide, ["wanderer"] = _wWide, ["market"] = _wWide,
        ["story"] = _wWide, ["merchant"] = _wWide,
    };

    private static float[] StepWeightsFor(MapConcept c) =>
        _stepBias.TryGetValue(c.Key, out var w) ? w : _wNeutral;

    // Per-concept guaranteed signature room type(s): (slot, absoluteFloor, fraction). The target is
    // max(absoluteFloor, ceil(nodeCount × fraction)), so the signature stays clearly VISIBLE even on big
    // maps (the absolute floor alone got lost among many nodes). Slots: 0 Mon,1 Event,2 Rest,3 Elite,
    // 4 Shop,5 Treasure. Balanced/Wildlands intentionally have none (their identity is variety).
    private static readonly Dictionary<string, (int slot, int minAbs, float frac)[]> _guarantees = new()
    {
        ["market"]     = new[] { (4, 3, 0.15f), (5, 1, 0.0f) },  // moderate shops + a guaranteed treasure
        ["merchant"]   = new[] { (4, 4, 0.30f) },                // shops
        ["trove"]      = new[] { (5, 4, 0.0f) },                 // treasures up to the cap (4)
        ["story"]      = new[] { (1, 5, 0.45f) },                // events (?)
        ["crossroads"] = new[] { (1, 4, 0.30f) },                // events (mobs already dominate)
        ["wanderer"]   = new[] { (1, 4, 0.30f) },                // events
        ["woods"]      = new[] { (0, 5, 0.45f) },                // monsters
        ["hunt"]       = new[] { (0, 6, 0.50f) },                // monsters
        ["trial"]      = new[] { (3, 3, 0.28f) },                // elites
        ["fire"]       = new[] { (3, 3, 0.30f) },                // elites
        ["grinder"]    = new[] { (3, 3, 0.28f) },                // elites
        ["famine"]     = new[] { (3, 2, 0.15f) },                // some elites amid the attrition
        ["gauntlet"]   = new[] { (3, 4, 0.45f), (2, 4, 0.16f) },  // elites + a rest cadence for recovery
        ["dread"]      = new[] { (3, 4, 0.40f) },                // elites
    };

    // Penalty / "X-only" UPPER caps: a concept whose identity is the ABSENCE of a room type caps it so a
    // lucky roll / the rest floor can't hand it back. Each entry is (slot, max, fallbackSlot) — excess of
    // `slot` becomes `fallbackSlot` (concept-appropriate). Slots: 0 Mon,1 Event,2 Rest,3 Elite,4 Shop,5 Trez.
    private static readonly Dictionary<string, (int slot, int max, int fallback)[]> _caps = new()
    {
        ["gauntlet"]   = new[] { (0, 0, 3) },                        // elites only: no monsters → elite (rest/shop kept for recovery)
        ["crossroads"] = new[] { (3, 0, 0), (4, 0, 0), (5, 0, 0) },   // mobs + events only: no elite/shop/treasure → monster
        ["story"]      = new[] { (0, 1, 1), (3, 0, 1) },              // events only: almost no combat → event
        ["famine"]     = new[] { (2, 2, 0), (4, 1, 0) },              // rest & shop scarce → monster
        ["grinder"]    = new[] { (2, 2, 0), (4, 2, 0) },              // rest scarce → monster
        ["dread"]      = new[] { (2, 2, 3), (4, 1, 3) },              // rest & shop scarce → elite
    };

    /// <summary>
    /// Build the act map's paths from the current concept. Uses a concept-seeded RNG for the walks so each
    /// concept yields a DIFFERENT topology — even when two concepts share the same path count — while
    /// staying deterministic per concept+act (multiplayer / reload safe, and stable when forcing the same
    /// concept). Temporarily swaps the map's RNG for the walks, then restores it so the downstream
    /// pruning / type-assignment behave normally. Returns true if it built the paths (caller skips
    /// vanilla generation), false to fall back to vanilla (disabled or error).
    /// </summary>
    // StandardActMap._rng is readonly, so the publicizer can't make it assignable — set it via reflection.
    private static readonly System.Reflection.FieldInfo? _rngField =
        typeof(StandardActMap).GetField("_rng",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    public static bool BuildConceptPaths(StandardActMap m)
    {
        if (_disabled || m == null || _rngField == null) return false;
        int act = CurrentAct();
        uint seed = CurrentSeed();
        var concept = ResolveConcept(act, seed);
        var saved = m._rng;
        try
        {
            _rngField.SetValue(m, new Rng(seed, $"sts2conceptmap_paths_{concept.Key}_act{act}"));
            GeneratePaths(m, PathsFor(concept), StepWeightsFor(concept));
            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[{MainFile.ModId}] concept path-gen failed; using vanilla: {ex.Message}");
            return false;
        }
        finally
        {
            _rngField.SetValue(m, saved); // restore so downstream pruning / type-assignment use the act RNG
        }
    }

    // Tier selection weights per (0-based) act: index 0..2 = Tier 1..3. Act 3+ leans hard.
    private static float[] TierWeightsForAct(int act) => act switch
    {
        <= 0 => new[] { 0.70f, 0.30f, 0.00f },
        1    => new[] { 0.25f, 0.50f, 0.25f },
        _    => new[] { 0.00f, 0.35f, 0.65f },
    };

    // --- Mode (which concept(s) to use) ---------------------------------------------------------
    // _mode == SurpriseKey → roll a concept per map; otherwise it's a concept Key forced on every map.
    private static string _mode = SurpriseKey;

    public static void SetModeByKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (string.Equals(key, SurpriseKey, StringComparison.OrdinalIgnoreCase)) { _mode = SurpriseKey; }
        else
        {
            var c = Concepts.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (c != null) _mode = c.Key;
        }
        MainFile.Logger.Info($"[{MainFile.ModId}] mode = {_mode}");
    }

    /// <summary>Current forced-concept key, or <see cref="SurpriseKey"/>.</summary>
    public static string CurrentModeKey => _mode;

    /// <summary>Type histogram of the most recently themed map (for the `conceptmap stats` console command).</summary>
    public static string LastStats = "(no map generated yet)";

    /// <summary>Harmony init status (for the `conceptmap stats` console command).</summary>
    public static string InitStatus = "(init not run)";

    // Cursor for the `conceptmap next` console command (cycles through every concept in order).
    private static int _cycle = -1;
    public static string CycleNext()
    {
        _cycle = (_cycle + 1) % Concepts.Length;
        return Concepts[_cycle].Key;
    }

    // Per-map concept (for the on-screen label; assignment itself happens once at generation).
    private static readonly ConditionalWeakTable<ActMap, MapConcept> _mapConcept = new();

    /// <summary>
    /// Re-types every non-exempt node of a freshly generated map according to its concept. Called from
    /// the <see cref="StandardActMap"/> constructor postfix.
    /// </summary>
    public static void AssignToMap(ActMap map)
    {
        if (_disabled) { LastStats = "disabled (env STS2_CONCEPT_MAP_DISABLED=1)"; return; }
        if (map == null) { LastStats = "map was null"; return; }
        try
        {
            int act = CurrentAct();
            uint seed = CurrentSeed();
            var concept = ResolveConcept(act, seed);
            _mapConcept.AddOrUpdate(map, concept);

            var rng = new Rng(seed, $"sts2conceptmap_assign_act{act}");
            float[] w = EffectiveWeights(concept);

            var modifiable = new List<MapPoint>();
            foreach (var p in map.GetAllMapPoints())
            {
                if (p == null || IsExempt(p, map)) continue;
                p.PointType = Slot[PickSlot(w, rng)];
                modifiable.Add(p);
            }
            int forcedRest = EnsureMinimumRests(modifiable, rng);
            int forcedSig = EnsureGuarantees(concept, modifiable, rng);
            int cappedTrez = CapExcessTreasure(modifiable, rng);
            int cappedPen = CapTypes(concept, modifiable, rng);

            // Diagnostic: final type histogram of the modifiable nodes (helps confirm assignment took).
            int mon = 0, ev = 0, rs = 0, el = 0, sh = 0, tr = 0;
            foreach (var p in modifiable)
                switch (p.PointType)
                {
                    case MapPointType.Monster: mon++; break;
                    case MapPointType.Unknown: ev++; break;
                    case MapPointType.RestSite: rs++; break;
                    case MapPointType.Elite: el++; break;
                    case MapPointType.Shop: sh++; break;
                    case MapPointType.Treasure: tr++; break;
                }
            LastStats = $"act {act + 1} concept={concept.Key} ({modifiable.Count} free nodes, " +
                        $"+{forcedRest} rest, +{forcedSig} sig, -{cappedTrez} trez→shop, -{cappedPen} cap) → " +
                        $"Mon={mon} Ev={ev} Rest={rs} Elite={el} Shop={sh} Trez={tr}";
            MainFile.Logger.Info($"[{MainFile.ModId}] {LastStats}");
        }
        catch (Exception ex)
        {
            LastStats = $"assign FAILED: {ex.GetType().Name}: {ex.Message}";
            MainFile.Logger.Warn($"[{MainFile.ModId}] assign failed: {ex}");
        }
    }

    /// <summary>The concept governing the current act's map (for UI). Re-derived deterministically if the
    /// map isn't in the table yet (e.g. after a save reload, where assignment didn't re-run).</summary>
    public static MapConcept? CurrentConcept()
    {
        if (_disabled) return null;
        try
        {
            var map = RunManager.Instance?.State?.Map;
            if (map == null) return null;
            if (_mapConcept.TryGetValue(map, out var c)) return c;
            var derived = ResolveConcept(CurrentAct(), CurrentSeed());
            _mapConcept.AddOrUpdate(map, derived);
            return derived;
        }
        catch { return null; }
    }

    // --- internals ------------------------------------------------------------------------------

    private static MapConcept ResolveConcept(int act, uint seed)
    {
        if (_mode != SurpriseKey)
            return Concepts.FirstOrDefault(c => c.Key == _mode) ?? Balanced;

        var rng = new Rng(seed, $"sts2conceptmap_concept_act{act}");
        int tier = PickSlot(TierWeightsForAct(act), rng) + 1; // 1..3
        var pool = Concepts.Where(c => c.Tier == tier).ToArray();
        if (pool.Length == 0) return Balanced;
        return pool[Math.Min(pool.Length - 1, (int)(rng.NextFloat() * pool.Length))];
    }

    private static MapConcept Balanced => Concepts.First(c => c.Key == "balanced");

    /// <summary>BaseTarget × concept multipliers (un-normalised; PickSlot normalises by total).</summary>
    private static float[] EffectiveWeights(MapConcept c)
    {
        var w = new float[BaseTarget.Length];
        for (int i = 0; i < w.Length; i++) w[i] = BaseTarget[i] * c.Mult[i];
        return w;
    }

    /// <summary>
    /// Guarantee a small minimum number of rest nodes on the map (anti-drought). If the independent
    /// assignment already produced enough campfires this is a no-op; otherwise it converts that many
    /// random non-rest modifiable nodes to <see cref="MapPointType.RestSite"/> using the deterministic
    /// <paramref name="rng"/>. Returns how many were forced. Everything else stays purely random.
    /// </summary>
    private static int EnsureMinimumRests(List<MapPoint> modifiable, Rng rng)
    {
        if (modifiable.Count == 0) return 0;
        int target = Math.Max(MinRests, (int)Math.Ceiling(modifiable.Count * MinRestFraction));
        int have = modifiable.Count(p => p.PointType == MapPointType.RestSite);
        int need = target - have;
        if (need <= 0) return 0;

        var candidates = modifiable.Where(p => p.PointType != MapPointType.RestSite).ToList();
        int forced = 0;
        while (forced < need && candidates.Count > 0)
        {
            int idx = Math.Min(candidates.Count - 1, (int)(rng.NextFloat() * candidates.Count));
            candidates[idx].PointType = MapPointType.RestSite;
            candidates.RemoveAt(idx);
            forced++;
        }
        return forced;
    }

    /// <summary>
    /// Guarantee the concept's signature room type(s) reach their minimum count (a no-op when the random
    /// assignment already met them). Converts random "filler" nodes — never rest nodes (preserving the
    /// anti-drought floor) and never another of this concept's guaranteed types — so guarantees don't
    /// cannibalize each other. Returns how many were forced. Uses the deterministic <paramref name="rng"/>.
    /// </summary>
    private static int EnsureGuarantees(MapConcept c, List<MapPoint> modifiable, Rng rng)
    {
        if (!_guarantees.TryGetValue(c.Key, out var reqs)) return 0;

        var protectedTypes = new HashSet<MapPointType> { MapPointType.RestSite };
        foreach (var (s, _, _) in reqs) protectedTypes.Add(Slot[s]);

        int forced = 0;
        foreach (var (slot, minAbs, frac) in reqs)
        {
            var type = Slot[slot];
            int target = Math.Max(minAbs, (int)Math.Ceiling(modifiable.Count * frac));
            int need = target - modifiable.Count(p => p.PointType == type);
            if (need <= 0) continue;

            var cands = modifiable.Where(p => !protectedTypes.Contains(p.PointType)).ToList();
            while (need > 0 && cands.Count > 0)
            {
                int idx = Math.Min(cands.Count - 1, (int)(rng.NextFloat() * cands.Count));
                cands[idx].PointType = type;
                cands.RemoveAt(idx);
                need--; forced++;
            }
        }
        return forced;
    }

    /// <summary>
    /// Cap the number of treasure (relic) nodes to <see cref="MaxExtraTreasure"/>. Each treasure room
    /// grants a free relic, so an unbounded concept (e.g. Treasure Trail) would flood the run with relics
    /// and risk draining the relic grab bag. Excess treasure becomes a Shop (buy relics with gold — keeps
    /// the loot flavour, but gold-gated). Returns how many were converted.
    /// </summary>
    private static int CapExcessTreasure(List<MapPoint> modifiable, Rng rng)
    {
        var treasures = modifiable.Where(p => p.PointType == MapPointType.Treasure).ToList();
        int excess = treasures.Count - MaxExtraTreasure;
        if (excess <= 0) return 0;

        int converted = 0;
        while (converted < excess && treasures.Count > 0)
        {
            int idx = Math.Min(treasures.Count - 1, (int)(rng.NextFloat() * treasures.Count));
            treasures[idx].PointType = MapPointType.Shop;
            treasures.RemoveAt(idx);
            converted++;
        }
        return converted;
    }

    /// <summary>
    /// Enforce a concept's penalty / "X-only" upper caps: any room type over its <c>_caps</c> max is
    /// converted to the concept-appropriate fallback type, so the defining absence (e.g. Elite Gauntlet
    /// has no normal monsters) is reliably felt regardless of the random roll or the rest floor.
    /// Returns how many nodes were converted.
    /// </summary>
    private static int CapTypes(MapConcept c, List<MapPoint> modifiable, Rng rng)
    {
        if (!_caps.TryGetValue(c.Key, out var caps)) return 0;

        int converted = 0;
        foreach (var (slot, max, fallback) in caps)
        {
            var type = Slot[slot];
            var fb = Slot[fallback];
            var have = modifiable.Where(p => p.PointType == type).ToList();
            int excess = have.Count - max;
            while (excess > 0 && have.Count > 0)
            {
                int idx = Math.Min(have.Count - 1, (int)(rng.NextFloat() * have.Count));
                have[idx].PointType = fb;
                have.RemoveAt(idx);
                excess--; converted++;
            }
        }
        return converted;
    }

    /// <summary>Weighted index pick over <paramref name="weights"/> using <paramref name="rng"/>.</summary>
    private static int PickSlot(float[] weights, Rng rng)
    {
        float total = 0f;
        for (int i = 0; i < weights.Length; i++) total += weights[i];
        if (total <= 0f) return 0;
        float roll = rng.NextFloat() * total;
        float acc = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            acc += weights[i];
            if (roll < acc) return i;
        }
        return weights.Length - 1;
    }

    private static int CurrentAct() => RunManager.Instance?.State?.CurrentActIndex ?? 0;

    private static uint CurrentSeed()
    {
        try { return RunManager.Instance?.State?.Rng?.Seed ?? 0u; }
        catch { return 0u; }
    }

    /// <summary>
    /// Faithful re-implementation of <c>StandardActMap.GenerateMap()</c> with a custom number of
    /// path-tracing walks (vanilla traces exactly 7), driven by whatever RNG <c>m._rng</c> currently is
    /// (the caller swaps in a concept-seeded one). Reuses the game's own private members (exposed via the
    /// publicizer) — <c>PathGenerate</c> does each walk with the game's crossover rules — and the
    /// downstream <c>AssignPointTypes</c> + <c>PruneAndRepair</c> + post-processing still run from the
    /// constructor afterwards, so connectivity stays the game's responsibility.
    /// </summary>
    private static void GeneratePaths(StandardActMap m, int pathCount, float[] stepW)
    {
        int cols = m.GetColumnCount();
        int rows = m.GetRowCount();

        for (int i = 0; i < pathCount; i++)
        {
            var sp = m.GetOrCreatePoint(m._rng.NextInt(0, cols), 1);
            if (i == 1)
            {
                while (m.startMapPoints.Contains(sp))
                    sp = m.GetOrCreatePoint(m._rng.NextInt(0, cols), 1);
            }
            m.startMapPoints.Add(sp);
            TracePath(m, sp, stepW);
        }

        // Connect the last grid row to the boss (and a second boss if present).
        for (int c = 0; c < cols; c++)
        {
            var p = m.GetPoint(c, rows - 1);
            if (p != null) p.AddChildPoint(m.BossMapPoint);
        }
        if (m.SecondBossMapPoint != null) m.BossMapPoint.AddChildPoint(m.SecondBossMapPoint);

        // Connect the starting node down to row 1.
        for (int c = 0; c < cols; c++)
        {
            var p = m.GetPoint(c, 1);
            if (p != null) m.StartingMapPoint.AddChildPoint(p);
        }
    }

    /// <summary>
    /// Trace one path from <paramref name="start"/> up to the last grid row, choosing each step among
    /// left/straight/right with the concept's <paramref name="stepW"/> bias. Only the try-ORDER is biased;
    /// the game's <c>HasInvalidCrossover</c> still validates each candidate, and straight is always a valid
    /// fallback, so the walk never dead-ends and the map stays connectable (pruning finishes the job).
    /// </summary>
    private static void TracePath(StandardActMap m, MapPoint start, float[] stepW)
    {
        int lastRow = m.GetRowCount() - 1;
        var cur = start;
        while (cur.coord.row < lastRow)
        {
            int col = cur.coord.col;
            int[] targets = { Math.Max(0, col - 1), col, Math.Min(col + 1, 6) };

            int nextCol = col; // straight = guaranteed-valid fallback
            foreach (int oi in WeightedOrder(stepW, m._rng))
            {
                int tx = targets[oi];
                if (!m.HasInvalidCrossover(cur, tx)) { nextCol = tx; break; }
            }

            var next = m.GetOrCreatePoint(nextCol, cur.coord.row + 1);
            cur.AddChildPoint(next);
            cur = next;
        }
    }

    /// <summary>A biased random permutation of {0,1,2} (weighted sampling without replacement). The first
    /// crossover-valid option in this order is taken, so higher-weighted steps win more often.</summary>
    private static int[] WeightedOrder(float[] w, Rng rng)
    {
        var idx = new List<int> { 0, 1, 2 };
        var ws = new List<float> { w[0], w[1], w[2] };
        var order = new int[3];
        for (int k = 0; k < 3; k++)
        {
            float tot = 0f;
            foreach (var x in ws) tot += x;
            float r = rng.NextFloat() * tot, acc = 0f;
            int pick = ws.Count - 1;
            for (int j = 0; j < ws.Count; j++) { acc += ws[j]; if (r < acc) { pick = j; break; } }
            order[k] = idx[pick];
            idx.RemoveAt(pick);
            ws.RemoveAt(pick);
        }
        return order;
    }

    /// <summary>True when a node keeps its real type: engine-fixed nodes (CanBeModified=false, e.g. the
    /// Spoils/treasure-map event's central treasure), fixed treasure (relic), start, pre-boss rest, boss.
    /// Mirrors the Random Map / Blind Map exemption set.</summary>
    private static bool IsExempt(MapPoint p, ActMap map)
    {
        if (!p.CanBeModified) return true;                        // fixed nodes (e.g. the Spoils map's treasure)
        if (p.PointType == MapPointType.Treasure) return true;
        if (p.PointType == MapPointType.Ancient) return true;
        if (p.PointType == MapPointType.Boss) return true;
        if (SameCoord(p, map.StartingMapPoint)) return true;
        if (FeedsInto(map.BossMapPoint, p)) return true;
        if (map.SecondBossMapPoint != null && FeedsInto(map.SecondBossMapPoint, p)) return true;
        return false;
    }

    private static bool FeedsInto(MapPoint boss, MapPoint p)
    {
        foreach (var parent in boss.parents)
            if (SameCoord(parent, p)) return true;
        return false;
    }

    private static bool SameCoord(MapPoint a, MapPoint b) =>
        a.coord.col == b.coord.col && a.coord.row == b.coord.row;
}

# StS2 Concept Maps

A **Slay the Spire 2** mod that gives every act map a **concept** — a theme like *Elite Gauntlet*, *Storyteller's Path*, or *Merchant's Run*. The map's rooms (and its branching) are shaped to fit, so each map *reads* as something. Concepts are gated by difficulty: **Act 1 draws easy concepts, Act 3 draws hard ones.**

> Sister mod to [Random Map] and [Blind Map]. Unlike Random Map (which hides every node behind a `?`), Concept Maps is **visible** — you can see the themed layout and plan your route.

[한국어 README](README.ko.md)

---

## What it does

Right after an act map is generated, the mod rolls a concept for that map and re-types its nodes to match — a **visible, themed map**. It also shapes the **routes**: choice-rich concepts trace more branches, forced ones fewer, and each concept walks with a slightly different bias (narrow vs sprawling).

Events have no standalone map node in StS2 — the `?` (Unknown) tile *is* the event/mystery room — so a concept's "event" weight shows up as `?` nodes (which the game resolves to an event most of the time).

### The 16 concepts

| Tier | Concept | Character |
|---|---|---|
| 🟢 1 | 🧭 Wanderer's Path | Events & rest, calm |
| 🟢 1 | 🏪 Market Street | Shops (+ events) |
| 🟢 1 | 🌲 Quiet Woods | Normal monsters + rest, few elites |
| 🟢 1 | 🗺 Treasure Trail | Treasure + a shop "loot route" |
| 🟢 1 | 📖 Storyteller's Path | Events only |
| 🟡 2 | ⚖ Balanced | The default tuned mix |
| 🟡 2 | 🔀 Crossroads | Normal mobs + events only |
| 🟡 2 | 🏹 Hunting Grounds | Monster-heavy + rest |
| 🟡 2 | 💰 Merchant's Run | Shops everywhere |
| 🟡 2 | 🎲 Wildlands | Uniform chaos |
| 🟡 2 | ⚔ Trial Road | Elite-laced combat |
| 🔴 3 | 🛡 Elite Gauntlet | Elites only |
| 🔴 3 | 🔥 Trial by Fire | Elites + monsters, few rests |
| 🔴 3 | ⚙ The Meatgrinder | Wall-to-wall combat |
| 🔴 3 | 🏜 Famine | Starved of rest & shops |
| 🔴 3 | 💀 Dread Domain | Act 3's worst — elite/monster barrage |

Act gating: **Act 1** leans Tier 1, **Act 2** centers on Tier 2, **Act 3+** leans Tier 3.

### Fairness safety nets

Assignment is pure per-node randomness (so dominant concepts clump characterfully), with a few floors layered on top so maps stay playable and on-theme:

- **Rest anti-drought** — every map keeps a minimum number of campfires (you're never fully starved; the pre-boss rest is always present on top of this).
- **Signature guarantee** — each concept guarantees a minimum share of its defining room type, so an unlucky roll never erases the theme.
- **Treasure cap** — each treasure room grants a free relic (vanilla gives one per act), so the number of *extra* treasure rooms is capped; the overflow becomes shops (buy relics with gold). This keeps a "loot route" generous without flooding the run with relics.

### On-screen banner

A small banner names the current map's concept, tinted by difficulty tier (green / amber / red). It's **draggable** (left-click drag; position saved) and only shows on the map-node screen.

Names are **localized in 16 languages** (EN, KO, JA, zh-CN, zh-TW, FR, DE, ES, IT, RU, PT-BR, PT, PL, TR, TH).

### Treasure-map (Spoils) event

The treasure-map event builds its own hourglass map; concepts apply there too — the central treasure stays fixed, only the paths above and below are themed.

## Console command

With the dev console enabled (active whenever the game runs modded), the `conceptmap` command lets you test concepts instantly:

| Command | Effect |
|---|---|
| `conceptmap list` | List every concept |
| `conceptmap <key>` | Force a concept and regenerate the current act |
| `conceptmap next` | Cycle to the next concept (rapid "test all") |
| `conceptmap surprise` | Back to auto (a concept per map) |
| `conceptmap stats` | Show the last map's patch/type breakdown |

Default mode is **Surprise** (a concept per map).

## Exceptions (keep their original type)

- Your starting node
- The boss (including the second boss)
- The rest site (fire) right before the boss
- Fixed treasure (relic) nodes and any engine-fixed node

## Determinism / multiplayer

Both the concept choice and the node/path assignment are driven by an RNG seeded from the run seed + act, so every player (and a reloaded save) gets the same themed map. No extra network messages. As with any gameplay mod, all players in a co-op session should run the same version.

> Don't run **Concept Maps** and **Random Map** at the same time — both re-type map nodes.

## Installation

1. Download the latest `Sts2ConceptMap-vX.Y.Z.zip` from [GitHub Releases](../../releases) (or the Steam Workshop / Nexus page).
2. Extract the `Sts2ConceptMap/` folder into `<Slay the Spire 2 install>/mods/` so you end up with:
   ```
   <Slay the Spire 2>/mods/Sts2ConceptMap/Sts2ConceptMap.dll
   <Slay the Spire 2>/mods/Sts2ConceptMap/Sts2ConceptMap.json
   ```
3. Launch the game.

## Building from source

Requirements: .NET SDK 9.0, Godot.NET.Sdk 4.5.1 (auto-resolved), a local Slay the Spire 2 install (auto-detected).

```sh
dotnet build Sts2ConceptMap.csproj -c Release
```

The build copies `Sts2ConceptMap.dll` and `Sts2ConceptMap.json` into `<sts2>/mods/Sts2ConceptMap/`.

## Configuration

- **Disable the mod:** set `STS2_CONCEPT_MAP_DISABLED=1` before launching.
- **Force a concept on every map (dev):** set `STS2_CONCEPT_MAP_MODE=<key>` (e.g. `gauntlet`).
- **Tune concepts:** edit the catalog, guarantees, path counts, step bias, and `MaxExtraTreasure` in `Sts2ConceptMapCode/ConceptMapService.cs` and rebuild.

## How it works

1. **`StandardActMap.GenerateMap` prefix** — rebuilds the map's paths with a concept-seeded RNG (so each concept yields a different topology), a per-concept path count, and a step bias; the game's pruning / post-processing still run, keeping the map connected.
2. **`StandardActMap` / `SpoilsActMap` constructor postfix** — re-types every non-exempt node to the concept's distribution, then applies the rest floor, signature guarantee, and treasure cap.
3. **`NMapScreen.Open` postfix** — attaches the draggable, localized concept banner.

Patches are applied individually (not via `PatchAll`) so a single patch failure can't disable the rest.

## Credits

- **MegaCrit** — for Slay the Spire 2.
- **HarmonyX** — runtime patching library (bundled with the game; not redistributed here).

## License

[MIT](LICENSE).

[Random Map]: https://github.com/ing-gom/sts2-random-map
[Blind Map]: https://github.com/ing-gom/sts2-blind-map

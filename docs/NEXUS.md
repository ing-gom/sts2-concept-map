# Nexus / Workshop listing copy

Reference text for the mod page. Nexus & Steam Workshop accept BBCode in the
long description; the summary is plain text. The blocks below are ready to paste.
The listing is English-only; Korean is available via README.ko.md in the repo.

---

## Summary (≤200 chars)

Every act map gets a concept — Elite Gauntlet, Storyteller's Path, Merchant's Run… Nodes AND paths are shaped to fit, and concepts get harder by act. 16 concepts, seeded RNG, co-op safe.

## Description (BBCode, English)

```bbcode
[size=4][b]What it does[/b][/size]

Right after an act map is generated, [b]Concept Maps[/b] rolls a "concept" for that map — a theme like [b]Elite Gauntlet[/b], [b]Storyteller's Path[/b], or [b]Merchant's Run[/b] — and re-types its nodes to fit. It also shapes the [b]routes[/b]: choice-rich concepts branch more, forced ones less. Each map reads as something, and you can see it and plan your route.

Unlike its sister mod [b]Random Map[/b] (which hides every node behind a [?]), Concept Maps is [b]visible[/b]. Concepts are gated by difficulty: [b]Act 1 draws easy concepts, Act 3 draws hard ones.[/b]

[size=4][b]The 16 concepts[/b][/size]
[list]
[*][b]Tier 1 (easy):[/b] Wanderer's Path, Market Street, Quiet Woods, Treasure Trail, Storyteller's Path
[*][b]Tier 2 (medium):[/b] Balanced, Crossroads, Hunting Grounds, Merchant's Run, Wildlands, Trial Road
[*][b]Tier 3 (hard):[/b] Elite Gauntlet, Trial by Fire, The Meatgrinder, Famine, Dread Domain
[/list]
(Events have no standalone map node in StS2 — the [?] tile is the event/mystery room — so a concept's "event" weight shows up as [?] nodes.)

[size=4][b]Fairness safety nets[/b][/size]
Node assignment is pure per-node randomness (so dominant concepts clump characterfully), with floors on top so maps stay playable and on-theme:
[list]
[*][b]Rest anti-drought[/b] — every map keeps a minimum number of campfires (the pre-boss rest is always present on top of this).
[*][b]Signature guarantee[/b] — each concept guarantees a minimum share of its defining room type, so an unlucky roll never erases the theme.
[*][b]Treasure cap[/b] — each treasure room grants a free relic, so the number of extra treasure rooms is capped; overflow becomes shops (buy relics with gold). Generous "loot routes" without flooding the run with relics.
[/list]

[size=4][b]Concept banner[/b][/size]
A small banner names the current map's concept, tinted by difficulty tier (green / amber / red). It's [b]draggable[/b] (left-click drag; position saved) and shows only on the map screen. Names are localized in [b]16 languages[/b].

[size=4][b]Console command[/b][/size]
With the dev console enabled (active whenever the game runs modded), [b]conceptmap[/b] lets you test concepts instantly:
[list]
[*][i]conceptmap list[/i] — list every concept
[*][i]conceptmap <key>[/i] — force a concept and regenerate the current act
[*][i]conceptmap next[/i] — cycle through every concept
[*][i]conceptmap surprise[/i] — back to auto (a concept per map)
[/list]
Default mode is [b]Surprise[/b] (a concept per map).

[size=4][b]Exceptions (keep their original type)[/b][/size]
[list]
[*]Your starting node
[*]The boss (including the second boss)
[*]The rest site (fire) right before the boss
[*]Fixed treasure (relic) nodes
[/list]
The treasure-map (Spoils) event is themed too — its central treasure stays fixed, only the paths above and below are shaped.

[size=4][b]Installation[/b][/size]
[list=1]
[*]Download the latest [b]Sts2ConceptMap-vX.Y.Z.zip[/b] from the [url=https://github.com/ing-gom/sts2-concept-map/releases]GitHub Releases page[/url] (or the Files tab here).
[*]Extract the [i]Sts2ConceptMap/[/i] folder into [i]<Slay the Spire 2 install>/mods/[/i].
[*]Launch the game.
[/list]

You should end up with:
[code]
<Slay the Spire 2>/mods/Sts2ConceptMap/Sts2ConceptMap.dll
<Slay the Spire 2>/mods/Sts2ConceptMap/Sts2ConceptMap.json
[/code]

[size=4][b]Multiplayer notes[/b][/size]
Concept choice and node/path assignment are driven by the run's seeded RNG, so all clients get the same themed map and there are no extra network messages. Every player in a co-op session should run the same version. Don't run Concept Maps and Random Map at the same time — both re-type map nodes.

[size=4][b]Configuration[/b][/size]
[list]
[*][b]Disable the mod[/b] — set [i]STS2_CONCEPT_MAP_DISABLED=1[/i] before launching.
[*][b]Force a concept (dev)[/b] — set [i]STS2_CONCEPT_MAP_MODE=<key>[/i] (e.g. gauntlet).
[/list]

[size=4][b]Credits / Source[/b][/size]
[list]
[*]MegaCrit — Slay the Spire 2.
[*]HarmonyX — runtime patching library used by this mod (bundled with the game; not redistributed).
[*]Source: [url=https://github.com/ing-gom/sts2-concept-map]github.com/ing-gom/sts2-concept-map[/url] · [url=https://github.com/ing-gom/sts2-concept-map/blob/main/LICENSE]MIT License[/url]
[*]Sister mod (fog-of-war): [url=https://github.com/ing-gom/sts2-random-map]Random Map[/url]
[*]한국어 설명: [url=https://github.com/ing-gom/sts2-concept-map/blob/main/README.ko.md]README.ko.md[/url]
[/list]
```

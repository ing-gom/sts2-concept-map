# Steam Workshop listing copy

Steam Workshop BBCode differs from Nexus: use [h1]/[h2]/[h3] for headings
(NOT [size]), and [olist] for numbered lists. Block below is ready to paste.

```bbcode
[h2]What it does[/h2]
Right after an act map is generated, [b]Concept Maps[/b] rolls a "concept" for that map — a theme like [b]Elite Gauntlet[/b], [b]Storyteller's Path[/b], or [b]Merchant's Run[/b] — and re-types its nodes to fit. It also shapes the [b]routes[/b]: choice-rich concepts branch more, forced ones less. Each map reads as something, and you can see it and plan your route.

Unlike its sister mod [b]Random Map[/b] (which hides every node behind a [?]), Concept Maps is [b]visible[/b]. Concepts are gated by difficulty: [b]Act 1 draws easy concepts, Act 3 draws hard ones.[/b]

[h2]The 16 concepts[/h2]
[list]
[*][b]Tier 1 (easy):[/b] Wanderer's Path, Market Street, Quiet Woods, Treasure Trail, Storyteller's Path
[*][b]Tier 2 (medium):[/b] Balanced, Crossroads, Hunting Grounds, Merchant's Run, Wildlands, Trial Road
[*][b]Tier 3 (hard):[/b] Elite Gauntlet, Trial by Fire, The Meatgrinder, Famine, Dread Domain
[/list]
Events have no standalone map node in StS2 — the [?] tile is the event/mystery room — so a concept's "event" weight shows up as [?] nodes.

[h2]Fairness safety nets[/h2]
Node assignment is pure per-node randomness (so dominant concepts clump characterfully), with floors on top so maps stay playable and on-theme:
[list]
[*][b]Rest anti-drought[/b] — every map keeps a minimum number of campfires (the pre-boss rest is always present on top of this).
[*][b]Signature guarantee[/b] — each concept guarantees a minimum share of its defining room type, so an unlucky roll never erases the theme.
[*][b]Penalty caps[/b] — a concept's defining absence is enforced (Elite Gauntlet has no normal monsters; Crossroads is mobs + events only).
[*][b]Treasure cap[/b] — each treasure room grants a free relic, so the number of extra treasure rooms is capped; overflow becomes shops.
[/list]

[h2]Concept banner[/h2]
A small banner names the current map's concept, tinted by difficulty tier (green / amber / red). It's [b]draggable[/b] (left-click drag; position saved) and shows only on the map screen. Names are localized in [b]16 languages[/b].

[h2]Console command[/h2]
With the dev console enabled (active whenever the game runs modded), [b]conceptmap[/b] lets you test concepts instantly:
[list]
[*][i]conceptmap list[/i] — list every concept
[*][i]conceptmap <key>[/i] — force a concept and regenerate the current act
[*][i]conceptmap next[/i] — cycle through every concept
[*][i]conceptmap surprise[/i] — back to auto (a concept per map)
[/list]
Default mode is [b]Surprise[/b] (a concept per map).

[h2]Exceptions (keep their original type)[/h2]
[list]
[*]Your starting node
[*]The boss (including the second boss)
[*]The rest site (fire) right before the boss
[*]Fixed treasure (relic) nodes
[/list]
The treasure-map (Spoils) event is themed too — its central treasure stays fixed, only the paths above and below are shaped.

[h2]Installation[/h2]
[olist]
[*]Subscribe here, or download the latest zip from the GitHub Releases page.
[*]If installing manually, extract the [i]Sts2ConceptMap/[/i] folder into [i]<Slay the Spire 2 install>/mods/[/i].
[*]Launch the game.
[/olist]

[h2]Multiplayer notes[/h2]
Concept choice and node/path assignment are driven by the run's seeded RNG, so all clients get the same themed map and there are no extra network messages. Every player in a co-op session should run the same version. Don't run Concept Maps and Random Map at the same time — both re-type map nodes.

[h2]Configuration[/h2]
[list]
[*][b]Disable the mod[/b] — set [i]STS2_CONCEPT_MAP_DISABLED=1[/i] before launching.
[*][b]Force a concept (dev)[/b] — set [i]STS2_CONCEPT_MAP_MODE=<key>[/i] (e.g. gauntlet).
[/list]

[h2]Credits / Source[/h2]
[list]
[*]MegaCrit — Slay the Spire 2.
[*]HarmonyX — runtime patching library used by this mod (bundled with the game; not redistributed).
[*]Source & MIT license: [url=https://github.com/ing-gom/sts2-concept-map]github.com/ing-gom/sts2-concept-map[/url]
[*]Sister mod: [url=https://github.com/ing-gom/sts2-random-map]Random Map[/url]
[/list]
```

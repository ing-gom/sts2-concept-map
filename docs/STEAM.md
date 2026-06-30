# Steam Workshop listing copy (bilingual EN + KO)

Steam Workshop BBCode: use [h1]/[h2]/[h3] for headings (NOT [size]),
[olist] for numbered lists, [hr] for a divider. Block below is ready to paste.

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
[*][b]Signature guarantee[/b] — each concept guarantees a minimum share of its defining room type.
[*][b]Penalty caps[/b] — a concept's defining absence is enforced (Elite Gauntlet has no normal monsters; Crossroads is mobs + events only).
[*][b]Treasure cap[/b] — each treasure room grants a free relic, so the number of extra treasure rooms is capped; overflow becomes shops.
[/list]

[h2]Difficulty levels (I / II / III)[/h2]
Each map also rolls a difficulty [b]level[/b], shown as a Roman numeral after the concept name (e.g. [i]Elite Gauntlet III[/i]). The level scales how hard the concept's penalty bites — [b]III[/b] is the full penalty (Elite Gauntlet III has no normal monsters), [b]I[/b] softens it. Rolled randomly but weighted by act, so it trends up over the run while still varying (a level-I Gauntlet in Act 3, a level-III Storyteller in Act 1). Separate from a concept's tier (which gates when it appears).

[h2]Concept banner[/h2]
A small banner names the current map's concept, tinted by difficulty tier (green / amber / red). It's [b]draggable[/b] (left-click drag; position saved) and shows only on the map screen. Names are localized in [b]16 languages[/b].

[h2]Console command[/h2]
With the dev console enabled (active whenever the game runs modded), [b]conceptmap[/b] lets you test concepts instantly: [i]conceptmap list[/i] (list all), [i]conceptmap <key>[/i] (force one), [i]conceptmap next[/i] (cycle), [i]conceptmap surprise[/i] (back to auto). Default mode is [b]Surprise[/b] (a concept per map).

[h2]Exceptions (keep their original type)[/h2]
[list]
[*]Your starting node, the boss (incl. the second boss)
[*]The rest site (fire) right before the boss
[*]Fixed treasure (relic) nodes
[/list]
The treasure-map (Spoils) event is themed too — its central treasure stays fixed, only the paths above and below are shaped.

[h2]Notes[/h2]
[list]
[*][b]Multiplayer:[/b] concept/level and node/path assignment use the run's seeded RNG, so all clients get the same map. Run the same version in co-op.
[*][b]Don't run Concept Maps and Random Map together[/b] — both re-type map nodes.
[*][b]Disable:[/b] set environment variable [i]STS2_CONCEPT_MAP_DISABLED=1[/i].
[/list]

[h2]Source[/h2]
[url=https://github.com/ing-gom/sts2-concept-map]github.com/ing-gom/sts2-concept-map[/url] (MIT) · Sister mod: [url=https://github.com/ing-gom/sts2-random-map]Random Map[/url] · Built on Slay the Spire 2 by MegaCrit.

[hr][/hr]

[h1]한국어[/h1]

[h2]무엇을 하나[/h2]
각 막의 지도가 생성되면 [b]Concept Maps[/b]가 그 맵에 "컨셉"(테마)을 하나 뽑습니다 — [b]정예 시련장[/b], [b]이야기의 장[/b], [b]상인의 길[/b] 같은. 그리고 노드 타입을 그 컨셉대로 바꿉니다. [b]경로[/b]도 컨셉에 맞춰져요: 선택형은 갈래가 많고 강제형은 적습니다. 맵 하나하나가 "성격"을 갖고, 미리 보고 경로를 짤 수 있습니다.

자매 모드 [b]Random Map[/b]([?]로 가림)과 달리 Concept Maps는 [b]보이는[/b] 테마맵입니다. 컨셉엔 난이도가 있어 [b]1막은 쉬운 컨셉, 3막은 어려운 컨셉[/b]이 나옵니다.

[h2]컨셉 16종[/h2]
[list]
[*][b]티어 1 (쉬움):[/b] 방랑자의 길, 시장 거리, 고요한 숲, 보물의 길, 이야기의 장
[*][b]티어 2 (중간):[/b] 균형, 갈림길, 사냥터, 상인의 길, 변덕의 땅, 시험의 길
[*][b]티어 3 (어려움):[/b] 정예 시련장, 시련의 불길, 분쇄기, 기근의 땅, 공포의 영역
[/list]
STS2엔 독립 "이벤트" 노드가 없고 [?] 타일이 곧 이벤트/미스터리 방이라, 컨셉의 "이벤트" 비중은 [?] 노드로 표현됩니다.

[h2]공정성 안전망[/h2]
배정은 노드별 순수 랜덤(지배 컨셉이 컨셉답게 뭉침)이고, 그 위에 최소 보장을 얹어 플레이 가능·테마 유지를 보장합니다:
[list]
[*][b]휴식 가뭄 방지[/b] — 맵마다 모닥불 최소치 보장(보스 직전 휴식은 별도로 항상 존재).
[*][b]시그니처 보장[/b] — 컨셉의 정체성 방 타입 최소 비율 보장.
[*][b]패널티 캡[/b] — 컨셉의 "부재"를 강제(정예 시련장엔 일반몹 없음, 갈림길은 일반몹+이벤트만).
[*][b]보물 캡[/b] — 보물 방은 유물을 무료로 주므로 추가 보물 수에 상한(초과분은 상점으로).
[/list]

[h2]난이도 레벨 (Ⅰ / Ⅱ / Ⅲ)[/h2]
맵마다 난이도 [b]레벨[/b]도 뽑혀 컨셉 이름 뒤에 로마 숫자로 표시됩니다(예: [i]Elite Gauntlet Ⅲ[/i]). 레벨은 패널티 강도를 조절합니다 — [b]Ⅲ[/b]는 최대(정예 시련장 Ⅲ는 일반몹 0), [b]Ⅰ[/b]은 완화. 막 가중 랜덤이라 진행할수록 높아지되 변주가 있습니다(3막에 Ⅰ짜리 정예 시련장, 1막에 Ⅲ짜리 이야기의 장도). 컨셉의 티어(언제 나오나)와는 별개입니다.

[h2]컨셉 배너[/h2]
현재 맵 컨셉 이름을 난이도 티어색(초록/주황/빨강)으로 표시합니다. [b]드래그[/b]로 이동(좌클릭, 위치 저장)하고 맵 화면에서만 보입니다. 이름은 [b]16개 언어[/b]로 현지화됩니다.

[h2]콘솔 명령[/h2]
개발자 콘솔(모드 실행 중 활성)에서 [b]conceptmap[/b]으로 즉석 테스트: [i]conceptmap list[/i](목록), [i]conceptmap <키>[/i](강제), [i]conceptmap next[/i](순환), [i]conceptmap surprise[/i](자동 복귀). 기본은 [b]Surprise[/b](맵마다 자동).

[h2]예외 / 참고[/h2]
[list]
[*]시작 노드·보스·보스 직전 휴식·고정 보물은 원래대로 유지. 보물지도(Spoils) 이벤트도 테마화(중앙 보물 고정, 위아래만).
[*][b]멀티플레이:[/b] 시드 RNG 기반이라 전원 동일 맵. 협동 시 같은 버전 사용.
[*][b]Random Map과 동시 사용 금지[/b] — 둘 다 노드를 바꿈.
[*][b]끄기:[/b] 환경변수 [i]STS2_CONCEPT_MAP_DISABLED=1[/i].
[/list]

[h2]소스[/h2]
[url=https://github.com/ing-gom/sts2-concept-map]github.com/ing-gom/sts2-concept-map[/url] (MIT) · 자매 모드: [url=https://github.com/ing-gom/sts2-random-map]Random Map[/url] · MegaCrit의 Slay the Spire 2 기반.
```

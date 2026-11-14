# Quickstart: Validating Pawn Gear Reroll

How to prove the feature works in a running game. The `rimworld-mod-testing` workflow turns these
into step-by-step Dev Mode checkpoints. Expected log lines are in
[contracts/log-contract.md](contracts/log-contract.md), the UI in
[contracts/ui-contract.md](contracts/ui-contract.md), and the saved data in
[data-model.md](data-model.md).

## Prerequisites

- Built with `dotnet build` from `Source/`. The build publishes `About/`, `LoadFolders.xml`, `1.6/`,
  `1.7/` and the shared content folders to `Mods/Downgrade`.
- Load order in RimSort: Harmony → (Combat Extended) → (Itemization Overhaul) → Downgrade.
- Options → Dev Mode on. Start each session with the **Dev quicktest** map.
- Useful dev tools: *Spawn pawn* (hostile), *Spawn thing*, *Add hediff*, *Force trait*, and the
  inspect pane's gear and health tabs.

## Session A: RimWorld 1.7, no other mods (Stories 1–5)

| # | Do | Expect |
|---|---|---|
| A1 | Load the game | Startup lines with `CE=off IO=off`, and non-zero pool counts |
| A2 | Spawn a hostile pirate or mercenary carrying an Industrial or Spacer gun and armor. Select it, Reroll, pick Medieval | Its gun becomes a Medieval-or-lower ranged weapon and its armor becomes Medieval-or-lower. A `Reroll …` log line lists removed and added defs |
| A3 | Spawn the same kind holding a melee weapon above the era. Reroll to Neolithic | It gets a Neolithic melee weapon. Materials are from the Neolithic list |
| A4 | Reroll a tribal (all Neolithic gear) to Medieval | "has no gear above Medieval" message. Nothing changes |
| A5 | Kill a rerolled pawn | No original above-era items drop (SC-008) |
| A6 | Spawn 20 hostiles plus 2 animals. Box-select all and Reroll once | One era menu. Summary shows 20 rerolled. Animals have no button and aren't counted as rerolled |
| A7 | Reopen the era menu, then save and reload, and reopen it | The last era is marked "(last used)" both times |
| A8 | Reroll a high-value pawn and a low-value pawn to Medieval | The high-value pawn's gear has higher quality, never above Masterwork, plus 0–2 whitelisted traits. The log shows per-category spend, the pooled trait budget, and a discarded remainder |
| A9 | Reroll the same pawn to Medieval 10 times | Every log line shows the same recorded budget. Gear value doesn't trend down (SC-003) |
| A10 | *Add hediff* BionicArm and a brain implant to a hostile, give it go-juice and let it take some, put penoxycyline in its inventory, then Reroll to Medieval | The arm becomes a Medieval-or-lower arm part if one exists, otherwise a natural arm. The brain implant is gone. The go-juice is gone from inventory, its high is removed, and any addiction stays. The penoxycyline is still there (not a combat drug) |
| A10b | Settings → Combat drugs: untick go-juice, then repeat A10's go-juice part | The go-juice stays. Tick it again afterwards |
| A11 | Settings: enable Colonists, then select a colonist | The button appears. Rerolling asks for confirmation, and Cancel changes nothing |
| A12 | Settings: disable Hostile | Hostiles show no button |
| A13 | Settings: add a modded or non-starter trait with a cost, and reroll a rich pawn several times | That trait can now appear |
| A14 | Save, reload, and inspect the pawn from A9 | The next reroll uses the same budget (SC-006) |
| A15 | Whole session | No red errors from Downgrade (SC-007) |

## Session B: RimWorld 1.6 with Combat Extended and Itemization Overhaul (Story 6)

Combat Extended and Itemization Overhaul only ship 1.6 builds (research R10), so their
compatibility is verified here.

| # | Do | Expect |
|---|---|---|
| B1 | Load | Startup line with `CE=on IO=on` |
| B2 | Reroll a hostile with a CE gun to Industrial | It gets an Industrial gun with a full magazine and a `CE ammo …` log line. Old ammo is gone from inventory. In combat it fires |
| B3 | Reroll a hostile to Medieval | No item above Excellent (Rare). Replacements show Itemization Overhaul affixes that fit their tier |
| B4 | Give a hostile a Relic-tier item, then Reroll | The Relic stays. Everything else above the era is replaced |
| B5 | Summon an Apex champion with a beacon and select it | No Reroll button. Enable "Allow named bosses", and the button appears |
| B6 | Whole session, plus a load of Session A's save without CE or IO | No red errors from Downgrade |

## Session C: whichever of these are installed (Story 6, open checks C1–C4)

| # | With | Expect |
|---|---|---|
| C1 | [RH2] V.O.I.D. on 1.6 | A V.O.I.D. soldier rerolled to Medieval keeps none of its V.O.I.D. weapons or armor. Unlabeled V.O.I.D. items appear under "Unlabeled items" as Spacer |
| C2 | Isekai RPG Leveling (+ Guild) | Level, stats, rank and rank traits are unchanged after a reroll. The Guild hero has no button by default |
| C3 | Elite Raid Plus | Death refusal and the shield survive a reroll. Its bionics are replaced by default, and kept with the setting off |
| C4 | Itemization Overhaul Jewelry & Charms | A charm is untouched by default, and era-checked with the setting on |

Items not tested in a session are recorded as **untested** in the release notes (Principle VI).

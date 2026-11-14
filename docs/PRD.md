# Downgrade — Product Requirements

**Status:** Draft, decisions confirmed by the customer on 2026-11-05
**Author:** DaraynLB
**Game versions:** RimWorld 1.6 and 1.7

## 1. Problem

The customer plays era-themed scenarios, such as neolithic, medieval or industrial runs. Some factions break that theme by showing up with gear far beyond the era:

- **V.O.I.D.** from *[RH2] Faction: V.O.I.D.* is an Ultra-tech faction with exclusive weapons, armor and implants. It arrives through its own scripted events. A V.O.I.D. negotiator is scripted to arrive on day 2.
- **The Horax cult** from the Anomaly DLC is a hidden Spacer-tech faction.
- **The Empire** from the Royalty DLC is Ultra tech.

Era-limiting approaches that work at the faction or research level don't catch these pawns, because they come from hidden factions, custom events and custom gear lists.

The customer wants these pawns to stay numerous and aggressive, but with weapons, armor, implants and drugs on **equal footing** with her colony's era.

## 2. Solution summary

A **Reroll** button on pawns. She selects one or more pawns, picks an era, and presses Reroll. Every item on those pawns above the chosen era is replaced with era-appropriate gear. The budget for the new gear is the value of the items being replaced. Leftover budget goes, in order and with limits, into better quality and then combat traits. Whatever remains is discarded.

The mod changes gear, not numbers. Raid size, raid frequency and faction behavior are untouched.

## 3. Users

- **Primary:** the customer, playing era-themed scenarios with heavy mod lists and Combat Extended.
- **Secondary:** public Steam Workshop players with the same problem.

## 4. Requirements

### 4.1 The Reroll action

- **R1. Button:** a Reroll command appears on eligible pawns (see R2). It is always available, not only in Dev Mode.
- **R2. Eligible pawn categories:** set in mod options, with one checkbox per category: **Hostile**, **Visitors**, **Traders**, **Prisoners**, **Colonists**. Default: Hostile only.
- **R3. No gear, no button:** pawns that can't carry gear never show the button. That covers mechanoids, animals, Anomaly entities, V.O.I.D.'s N-4 Undead Horde, Isekai Creatures' monsters and similar.
- **R4. Named bosses exempt:** named bosses and heroes don't get the button by default. That includes Itemization Overhaul's Apex champions and the Isekai Guild's legendary hero. A mod option (default off) allows rerolling them anyway.
- **R5. Multi-select:** selecting several pawns and pressing Reroll rerolls all of them with the same chosen era.
- **R6. Era picker:** a choice of Neolithic, Medieval, Industrial, Spacer or Ultra. The game remembers her last pick.
- **R7. Repeatable and fair:** Downgrade records the budget at a pawn's first reroll (see B1). Every later reroll starts fresh from that same budget, so rerolling never makes a pawn steadily weaker and never stacks leftover budget.
- **R8. No loot farming:** replaced items are destroyed, not dropped.

### 4.2 What gets downgraded

Only items **above** the chosen era are replaced. Items at or below the era are left alone. An item counts as above the era if **either** its own era **or** its material's era (see 4.4) is above it: a hyperweave jacket is Spacer gear, even though a jacket itself is Medieval (decision 15).

| Category | Rule |
|---|---|
| Weapons | Replaced with an era weapon of the same role: ranged stays ranged, melee stays melee. |
| Armor and clothing | Replaced slot for slot with era apparel that the pawn's body can actually wear. This matters for modded races such as Big and Small or serpent races. |
| Implants and bionics | Replaced with the era's version of that body part, or removed if the era has none. Implants stay allowed but never above the era. Bionics added by Elite Raid Plus are included by default (option). |
| Drugs | Above-era combat drugs are removed from inventory, along with their active effects. A combat drug is a drug whose effect improves a pawn in a fight: a combat body capacity or stat (for example consciousness, moving, sight, shooting accuracy, melee dodge, attack speed, incoming damage) or pain reduction. Drugs that only affect mood, social, trade, learning or other non-combat stats are left alone, even above the era. Detected from each drug's effects; adjustable per drug in mod options. |
| Materials | Replacement gear only uses materials allowed in the era (see 4.4). |
| Jewelry and charms | Left alone by default. A mod option era-checks them like apparel. |
| Relic-tier items (Itemization Overhaul) | Always skipped. They stay on the pawn even when above the era, so a faction's reclaim-the-relic storyline keeps working. |

**Unlabeled items:** an item that declares no era is treated as **Spacer**. Mod options list every unlabeled item so she can assign the right era.

### 4.3 Budget and leftovers

- **B1. The budget is the value of what's replaced, for every pawn.** The base market value of the above-era items being replaced, kept separate per category: weapon, apparel, implants. It is the same rule for raiders, visitors, traders, prisoners and colonists. Base value means the item's value before any rarity or quality markup, such as Itemization Overhaul's rarity pricing. Downgrade records this budget at the first reroll (R7).
- **B2.** The budget is spent from the chosen era's items only.
- **B3.** Leftover budget is spent in this order, each step with a limit:
  1. **Gear:** the best era-appropriate items the budget allows.
  2. **Quality:** Downgrade sets the quality from the budget, up to a maximum quality setting. Default: **Masterwork**, or **Excellent** when Itemization Overhaul is loaded. With Itemization Overhaul, Downgrade picks the tier and Itemization Overhaul rolls that tier's bonuses.
  3. **Traits:** combat traits from the whitelist, each with a silver cost, up to a maximum number added per pawn. Default: 2. A trait that conflicts with one the pawn already has is skipped.
  4. **Remainder:** discarded.

### 4.4 Materials by era

| Era | Materials allowed |
|---|---|
| Neolithic | Wood, stone, leathers, plant cloth |
| Medieval | Adds steel, silver, gold, jade, wool |
| Industrial | Adds synthread, devilstrand |
| Spacer and above | Adds plasteel, hyperweave, uranium |

**Unknown materials:** mod options list every material not in this table, each with an era dropdown. The starting value is a guess from the material's market value.

### 4.5 Starter trait whitelist

| Trait | What it does in a fight | Cost (silver) |
|---|---|---|
| Tough | Takes half damage | 600 |
| Nimble | Dodges melee hits much more often | 500 |
| Careful Shooter | Aims longer, hits more often | 400 |
| Brawler | Better in melee (dislikes ranged weapons) | 300 |
| Trigger-happy | Shoots faster, less accurately | 300 |
| Iron-willed | Much less likely to break mentally under pressure | 300 |
| Jogger | Moves faster | 300 |
| Steadfast | Less likely to break mentally | 200 |
| Fast Walker | Moves a bit faster | 200 |
| Bloodlust | Gets a mood boost from combat and killing | 200 |

### 4.6 Mod options (global)

- Eligible pawn categories (R2). Default: Hostile only.
- Allow rerolling named bosses and heroes (R4). Default off.
- Maximum quality from leftover budget. Default Masterwork, or Excellent with Itemization Overhaul.
- Trait whitelist: a searchable list of every trait in the game, including modded traits, each with a silver cost. Starts with the list in 4.5.
- Maximum traits added per pawn. Default 2.
- Reroll bionics added by Elite Raid Plus. Default on.
- Era-check jewelry and charms. Default off.
- Era assignments for unlabeled items (4.2) and unknown materials (4.4).
- Which drugs count as combat drugs (4.2). Default: detected from each drug's effects.

## 5. Compatibility

| Mod | Requirement |
|---|---|
| Harmony | Required dependency. |
| Combat Extended | Required support. A swapped gun comes with matching ammo. |
| Elite Raid Plus | Its elite buffs (Death Refusal, Rapid Regeneration, Hit Count Shield, abilities) are kept. Its added bionics are rerolled by default, controlled by an option. Its raid compression is untouched. |
| [RH2] Faction: V.O.I.D. | A primary target. It is 1.6-only today, so Downgrade must not depend on it. |
| Anomaly (Horax cult) | Its pawns can be rerolled like any other. |
| Big and Small, serpent races | Replacement apparel must fit the pawn's body. |
| Itemization Overhaul – Core (astryl) and its family modules | Supported, no hard dependency. It is 1.6-only today. Its rarity ladder (Broken/Low, White, Magic, Rare, Legendary, Relic) maps one to one onto quality. Downgrade sets the tier and Itemization Overhaul rolls the affixes for it. Its affixes are already tech-aware. Budget math uses base value (B1). Replaced items must be unequipped properly, so their enchantments on the wearer are removed. Relics are skipped (4.2). |
| Isekai RPG Leveling (POUPUN) and its add-ons: Guild Faction, Isekai Creatures | Supported, no hard dependency. Supports 1.6 and 1.7. Pawn levels, RPG stats, ranks F–SSS, rank traits and auras aren't gear, so a reroll never touches them. Its Forge (+N upgrades) and rune enchantments live on items, so they go away with a replaced item. Guild adventurers can be rerolled, the Guild hero is exempt by default (R4), and Isekai Creatures' monsters follow R3. |

**General rule for item overhaul mods:** Downgrade must not hard-code any item list. It decides "above the era" from each item's own tech level and applies quality through the game's standard quality system. Mods that rework items, quality or rarity then work without patches.

## 6. Out of scope

- Any automatic cap per faction or per game. Downgrade is only a manual, per-pawn tool.
- Monsters, mechanoids, animals and entities.
- Raid size, frequency or raid points.
- What traders sell. Confirmed out of scope for the first release; it could be a later, separate feature.
- The player's research or tech level.

## 7. Checks for the planning step

These aren't customer questions. They need the mods' files and can be done during `/speckit-plan`.

1. **V.O.I.D. item eras:** whether V.O.I.D.'s items declare a tech level. This needs the mod subscribed on the dev machine.
2. **Isekai rank traits:** whether Isekai RPG Leveling's rank traits are real RimWorld traits, which matters for the trait limit and conflict rules. This needs the mod subscribed.
3. **Named boss detection:** how to recognize Apex champions and the Guild hero reliably (pawn kind, a marker, or the quest).

## 8. Decision log

All decided by the customer on 2026-11-05. The explainer doc is [Downgrade — Questions for You](https://claude.ai/artifact/QkfwDAcPdLLiXSM2jCwXHt).

| # | Question | Decision |
|---|---|---|
| 1 | Modded materials with no era | Era dropdown in settings, pre-filled from a price-based guess |
| 2 | Budget source | Value of the above-era items being replaced, for every pawn |
| 3 | Starter traits and costs | Proposed list and prices accepted as-is |
| 4 | Unlabeled items | Treated as Spacer, adjustable in settings |
| 5 | Trader stock | Out of scope |
| 6 | Quality cap with Itemization Overhaul | Excellent (Rare), at most 4 bonuses |
| 7 | Who sets the tier | Downgrade, from the budget; Itemization Overhaul rolls the bonuses |
| 8 | Named bosses and heroes | Exempt by default, with a setting to reroll them |
| 9 | Jewelry and charms | Left alone by default, with a toggle to era-check them |
| 10 | Relic items | Skipped, left in the pawn's hands |
| 11 | Which drugs are "combat drugs" (2026-11-10) | Drugs whose effect improves fighting, pain reduction included. Mood, social, trade and learning drugs are left alone. Detected automatically, with a per-drug override in settings |
| 12 | Grenades and one-shot launchers as replacements (2026-11-10) | Never handed out as a replacement weapon; still removed when above the era |
| 13 | Weapons carried in inventory (2026-11-10) | Replaced like the held weapon; the replacement goes back into inventory |
| 14 | Budget across several items in one category (2026-11-10) | Each replaced item gets a share in proportion to its value; unspent shares carry to the next item |
| 15 | Worn gear in an above-era material (2026-11-13) | Counts as above the era and is replaced, e.g. a hyperweave jacket in a Medieval reroll is Spacer gear. Found in testing (quickstart A2) |

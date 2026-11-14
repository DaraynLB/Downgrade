# Feature Specification: Pawn Gear Reroll

**Feature Branch**: `001-pawn-gear-reroll`

**Created**: 2026-11-05

**Status**: Draft

**Input**: User description: "The Reroll feature from docs/PRD.md. The player selects one or more pawns, picks an era, and presses Reroll. Every weapon, armor piece, clothing item, implant and drug above that era is replaced with era-appropriate gear, bought from a budget equal to the value of what was replaced. Leftover budget buys quality, then combat traits from a whitelist, and the rest is discarded. Includes the mod options and compatibility rules in PRD §4–§5."

## Clarifications

### Session 2026-11-05

- Q: When an above-era body-part replacement has no version in the chosen era, what happens to the part? → A: It is restored to a natural body part.
- Q: Is leftover silver pooled across the weapon, apparel and implant budgets before buying quality and traits? → A: Quality stays per category (each budget's leftover upgrades only its own items); whatever remains in all three is then pooled to buy traits.
- Q: If a budget can't afford any era item for a replaced weapon or apparel slot, what does the pawn get? → A: The cheapest era item of that role or slot anyway.
- Q: Should rerolling your own colonists or prisoners ask for confirmation? → A: Yes, only when the selection includes colonists or prisoners. Hostiles, visitors and traders never ask.

### Session 2026-11-10

- Q: Which drugs are "combat drugs"? → A: Drugs that would affect the outcome of a skirmish: faster attacks, more defense, better movement, aim or awareness. Drugs that only make a pawn more charismatic, better at trade or smarter have no effect on combat and are not combat drugs.
- Q: Does pain reduction count as a combat effect? → A: Yes. It stops a pawn being downed by pain, so it changes the outcome.
- Q: Can the player override which drugs count? → A: Yes, a per-drug checkbox in settings, pre-filled by the automatic detection.

### Session 2026-11-13 (found in testing)

- Q: A worn or held item whose own era is at or below the chosen era but whose material is above it (e.g. a hyperweave jacket in a Medieval reroll): is it replaced? → A: Yes. It counts as gear of its material's era (Spacer for hyperweave) and is replaced like any other above-era item (PRD decision 15).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Downgrade one raider's weapon and apparel to an era (Priority: P1)

A V.O.I.D. raider with Ultra-tech weapons and armor arrives in the player's medieval game. The
player selects the raider, picks **Medieval**, and presses **Reroll**. The raider's above-era
weapon and every above-era armor and clothing item are destroyed. In their place the raider gets
medieval items of the same role: a ranged weapon for a ranged weapon, a melee weapon for a melee
weapon, and apparel for the same body slots. The new gear is bought from the value of what was
replaced, and is made only from materials allowed in the Medieval era.

**Why this priority**: this is the whole reason the mod exists (PRD §1). It works without
leftovers, traits, implants or multi-select. One rerolled raider already proves the idea in a
live game.

**Independent Test**: start a 1.7 game, spawn an Ultra-tech raider with Dev Mode, select it,
choose Medieval, and press Reroll. Inspect its gear.

**Acceptance Scenarios**:

1. **Given** a hostile pawn carrying a Spacer ranged weapon and wearing Spacer armor, **When** the
   player rerolls it to Medieval, **Then** its weapon is a Medieval-or-lower ranged weapon, every
   apparel item is Medieval or lower, and the original items no longer exist anywhere on the map.
2. **Given** a hostile pawn with a melee weapon above the chosen era, **When** it is rerolled,
   **Then** the replacement is a melee weapon.
3. **Given** a hostile pawn wearing a Neolithic item and a Spacer item, **When** it is rerolled to
   Medieval, **Then** the Neolithic item is untouched and only the Spacer item is replaced.
4. **Given** a rerolled pawn, **When** the player inspects any replacement item, **Then** its
   material is in the allowed list for the chosen era (PRD §4.4).
5. **Given** a pawn whose gear is all at or below the chosen era, **When** it is rerolled,
   **Then** nothing on it changes and the player is told there was nothing to downgrade.
6. **Given** a modded-race pawn (for example Big and Small), **When** it is rerolled, **Then**
   every replacement apparel item is one that pawn's body can wear.

---

### User Story 2 - Reroll a whole raid at once (Priority: P2)

A raid of 20 pawns arrives. The player box-selects all of them, picks an era once, and presses
Reroll. Every eligible pawn in the selection is rerolled. Ineligible ones (animals, mechanoids,
pawn categories turned off in settings) are skipped, and the player sees one summary of how many
were rerolled and how many were skipped.

**Why this priority**: raids are large. Rerolling one pawn at a time works but is slow, so this is
what makes the tool usable in real play.

**Independent Test**: spawn a 20-pawn raid mixed with animals, select everything, and reroll once.

**Acceptance Scenarios**:

1. **Given** 20 selected hostile humanlike pawns, **When** the player rerolls once, **Then** all
   20 are rerolled to the same era.
2. **Given** a selection that includes a mechanoid and an animal, **When** the player rerolls,
   **Then** those two are unchanged and the summary counts them as skipped.
3. **Given** the player rerolled to Medieval earlier, **When** the player opens the era picker
   again, even after a save and reload, **Then** Medieval is pre-selected.

---

### User Story 3 - Leftover budget buys quality, then traits (Priority: P3)

Era gear is cheaper than the Ultra gear it replaces, so most rerolled elites have budget left.
Each budget's leftover first raises the quality of its own replacement items, up to the quality
cap: the weapon leftover upgrades the weapon, and the apparel leftover upgrades apparel. What
remains in all three budgets is then pooled and buys combat traits from the whitelist, each at its
listed cost, up to the trait limit. Anything still left is discarded.

**Why this priority**: without it, elites and grunts end up with the same gear, which flattens
raids. It builds on Story 1 and isn't needed to prove the core swap.

**Independent Test**: reroll a high-budget pawn and a low-budget pawn to the same era, then
compare the quality of their gear and the traits they gained.

**Acceptance Scenarios**:

1. **Given** a pawn whose leftover budget covers a quality upgrade, **When** it is rerolled,
   **Then** its replacement gear quality is raised but never above the configured cap.
2. **Given** Itemization Overhaul is not loaded and settings are at their defaults, **When** a
   very high-budget pawn is rerolled, **Then** no replacement item exceeds Masterwork.
3. **Given** quality is already at the cap and budget remains, **When** traits are bought,
   **Then** only whitelisted traits are added, each costs its listed price, at most the configured
   maximum (default 2) are added, and a trait that conflicts with one the pawn already has is
   never added.
4. **Given** a pawn with budget left after traits, **When** the reroll finishes, **Then** that
   remainder is not turned into anything else.
5. **Given** a pawn with a large apparel leftover and no weapon leftover, **When** it is rerolled,
   **Then** the apparel leftover raises only apparel quality and never the weapon's. What remains
   of it still counts toward traits.

---

### User Story 4 - Implants and combat drugs follow the era too (Priority: P4)

The rerolled pawn's above-era implants and bionics are replaced with the chosen era's version for
that body part. Above-era combat drugs it carries are removed, along with their active effects.

**Why this priority**: the customer wants implants and drugs on equal footing as well (PRD §1),
but weapons and armor matter most in a fight.

**Independent Test**: give a raider a Spacer bionic arm, an Ultra brain implant, and a stack of an
above-era combat drug under its effect, then reroll to Medieval.

**Acceptance Scenarios**:

1. **Given** a pawn with an above-era body-part replacement, **When** it is rerolled to an era
   that has a version of that part, **Then** the part becomes that era's version.
2. **Given** a pawn with an above-era body-part replacement, **When** it is rerolled to an era
   with no version of that part, **Then** the part is restored to a natural body part, and the
   pawn is never left missing it.
3. **Given** a pawn with an above-era implant that doesn't replace a body part (for example a
   brain implant), **When** it is rerolled, **Then** the implant is removed.
4. **Given** a pawn carrying above-era combat drugs and under their effect, **When** it is
   rerolled, **Then** those drugs are gone from its inventory and their active effects are
   removed.

---

### User Story 5 - The player controls who can be rerolled and how (Priority: P5)

In the mod's settings window, the player chooses which pawn categories get the Reroll button. The
categories are Hostile, Visitors, Traders, Prisoners and Colonists, and the default is Hostile
only. The window also controls:
- the quality cap
- the trait whitelist and each trait's cost
- the trait limit
- whether Elite Raid Plus's bionics are rerolled
- whether jewelry and charms are era-checked
- whether named bosses can be rerolled
- the era of unlabeled items and unknown materials
- which drugs count as combat drugs

**Why this priority**: every default is already chosen (PRD §4.6), so the mod works without this
window. The window lets the customer tune it to her games.

**Independent Test**: change each setting and confirm the next reroll behaves accordingly.

**Acceptance Scenarios**:

1. **Given** only Hostile is enabled, **When** the player selects a visiting trader, **Then** no
   Reroll button appears.
2. **Given** the player enables Colonists, **When** the player selects a colonist, **Then** the
   Reroll button appears.
3. **Given** a modded trait added to the whitelist with a cost, **When** a pawn with enough
   leftover budget is rerolled, **Then** that trait can be bought.
4. **Given** an unknown modded material, **When** the player opens settings, **Then** it is listed
   with an era dropdown pre-filled from a guess based on its market value, and changing the
   dropdown changes which eras may use it.
5. **Given** an item with no era label, **When** the player opens settings, **Then** it is listed
   as Spacer and can be reassigned.
6. **Given** any setting changed, **When** the game is restarted, **Then** the change is still in
   effect.

---

### User Story 6 - Works alongside the customer's mod list (Priority: P6)

Rerolling behaves correctly with the mods the customer plays with:

- **Combat Extended:** rerolled ranged pawns get matching ammo.
- **Itemization Overhaul:** Downgrade sets the tier and Itemization Overhaul rolls its bonuses.
  Relic items are skipped, and jewelry is left alone by default.
- **Elite Raid Plus:** its elite buffs stay, and its bionics are rerolled by default.
- **Isekai RPG Leveling:** levels, stats, ranks and rank traits are untouched.
- **Named bosses** (Apex champions, the Guild hero) show no Reroll button by default.

**Why this priority**: the customer's real games depend on it, but every rule is a narrow
add-on to Stories 1–5. Each mod's rule only takes effect when that mod is loaded.

**Independent Test**: with each mod loaded in turn, reroll a pawn that mod affects and check the
rule for that mod.

**Acceptance Scenarios**:

1. **Given** Combat Extended is loaded, **When** a pawn is rerolled to a ranged weapon that uses
   ammo, **Then** it carries ammo that fits the weapon and fires it in combat.
2. **Given** Combat Extended is not loaded, **When** a pawn is rerolled, **Then** everything works
   and no errors appear.
3. **Given** Itemization Overhaul is loaded and settings are at their defaults, **When** a
   high-budget pawn is rerolled, **Then** no replacement item is above Excellent (Rare), and each
   replacement shows Itemization Overhaul bonuses that fit its tier.
4. **Given** a pawn carrying a Relic-tier item, **When** it is rerolled, **Then** the Relic is
   still in the pawn's hands and everything else above the era is replaced.
5. **Given** a pawn wearing a charm above the era and the jewelry setting off, **When** it is
   rerolled, **Then** the charm is untouched.
6. **Given** an Elite Raid Plus elite with death refusal, a shield and an added bionic, **When** it
   is rerolled with default settings, **Then** death refusal and the shield remain, and the
   above-era bionic is handled as in Story 4.
7. **Given** an Isekai RPG Leveling pawn of rank B, **When** it is rerolled, **Then** its level,
   stats, rank and rank traits are unchanged.
8. **Given** an Apex champion or the Guild hero with the named-boss setting off, **When** the
   player selects it, **Then** no Reroll button appears.

---

### Edge Cases

- **Gear is destroyed, not dropped.** Original items are destroyed on reroll, so killing a
  rerolled pawn never drops the original above-era gear.
- **Second reroll.** A pawn rerolled before starts from its recorded budget. Items and traits
  Downgrade gave it last time are taken away and bought again for the new era. Above-era items it
  picked up since the last reroll are replaced too, and their value is added to the recorded
  budget. A reroll to a higher era can therefore give better era gear, but never more than the
  recorded budget allows.
- **Budget too small for any era item:** for a replaced weapon, or for each replaced apparel slot,
  the pawn still gets the cheapest era item of that role or slot. A reroll never disarms or strips
  a pawn for lack of budget. That category then has no leftover.
- **The era has nothing for a slot:** if the era has no apparel for a slot, or none the pawn's
  body can wear, that slot is left empty. If the era has no weapon of the role at all, the pawn
  gets a weapon of the other role. If it has no weapon at all, the pawn is left unarmed.
- **A reroll fails partway.** If a reroll can't complete for a pawn for any reason, that pawn is
  left exactly as it was and the reason is logged. Other pawns in the same multi-select still
  reroll.
- **Not on the map.** Dead, downed-and-carried, caravanning and off-map pawns are never eligible.
  Only living pawns on the current map are.
- **Own colonists.** Rerolling the player's own colonists or prisoners destroys gear the player
  may value, so the player is asked to confirm first.
- **Already at or below the era.** Choosing an era at or above all of a pawn's gear changes nothing
  (Story 1, scenario 5).
- **Addictions.** Addictions and tolerances the pawn already has stay as they are. Only above-era
  combat drugs in its inventory and their active effects are removed. A pawn addicted to an
  above-era combat drug (for example luciferium) keeps the addiction but loses its supply.
- **Non-combat drugs above the era.** A drug with no combat effect, such as penoxycyline, stays in
  the pawn's inventory even when it is above the era.
- **Pawns that already have several traits.** Traits Downgrade adds come on top of the pawn's
  existing traits, up to the trait limit. Downgrade never removes a trait it didn't add.
- **The same mod item is above the era and in a skipped category.** For example, an above-era
  Relic charm. Skip rules win: the item is left alone.

## Requirements *(mandatory)*

### Functional Requirements

**Reroll action**

- **FR-001**: The game MUST show a Reroll command on every selected pawn that is eligible:
  - a living humanlike pawn on the current map
  - able to carry gear
  - in a pawn category enabled in settings
  - not a named boss or hero, unless the named-boss setting is on

  The command MUST be available without Dev Mode.
- **FR-002**: The Reroll command MUST let the player pick one era from Neolithic, Medieval,
  Industrial, Spacer and Ultra. It MUST pre-select the era the player picked last, remembered
  across saves.
- **FR-003**: When several pawns are selected, one Reroll MUST apply the same era to every
  eligible pawn and skip the rest. The player then sees a summary with the counts rerolled and
  skipped.
- **FR-004**: When the selection includes any of the player's own colonists or prisoners, the
  reroll MUST ask for confirmation once before running. A selection of only hostiles, visitors
  and traders MUST reroll without asking.

**What is replaced**

- **FR-005**: A reroll MUST replace only items whose era is above the chosen era. An item's era
  is the later of its own era and its material's era (Clarifications 2026-11-13). Items at or
  below it MUST be left untouched.
- **FR-006**: An item with no era label MUST be treated as Spacer, unless the player assigned it
  another era in settings.
- **FR-007**: A replaced weapon MUST be replaced with a weapon of the same role (ranged or melee),
  following the fallbacks in Edge Cases.
- **FR-008**: Replaced apparel MUST be replaced slot for slot with era apparel the pawn's body can
  wear. A slot is left empty when no such apparel exists.
- **FR-009**: An above-era body-part replacement MUST become the chosen era's version of that part.
  If the era has no version, the part MUST be restored to a natural one. An above-era implant that
  doesn't replace a body part MUST be removed.
- **FR-010**: Above-era combat drugs in the pawn's inventory MUST be removed, along with their
  active effects. Existing addictions and tolerances MUST be left alone.
  - A **combat drug** is one whose effect improves at least one combat body capacity or combat
    stat (for example consciousness, moving, sight, shooting accuracy, melee dodge, attack speed or
    incoming damage), or reduces pain.
  - Drugs whose effects only touch non-combat stats (mood, social impact, trade, learning,
    research and similar) MUST be left alone, even above the era.
  - Detection MUST work from each drug's own effects, so modded drugs are covered, and the player
    MUST be able to override it per drug in settings.
- **FR-011**: Replacement items MUST only be made from materials allowed in the chosen era: the
  table in PRD §4.4, plus the player's settings for unknown materials.
- **FR-012**: Original items MUST be destroyed. They MUST NOT be dropped, stored in inventory, or
  otherwise recoverable.
- **FR-013**: Removing items MUST also remove any effects those items gave their wearer, including
  effects added by other mods.

**Budget and leftovers**

- **FR-014**: A pawn's budget MUST be the base market value of the above-era items being replaced.
  Base value means before quality or rarity markup. The budget MUST be kept separate for weapons,
  apparel and implants, and the same rule applies to every pawn category.
- **FR-015**: The budget MUST be recorded at a pawn's first reroll and kept with the save. Later
  rerolls MUST follow the "Second reroll" rule in Edge Cases.
- **FR-016**: Each category's budget MUST be spent on the best era items it can afford. If it
  can't afford any era item for a replaced weapon or apparel slot, the cheapest era item of that
  role or slot MUST be given anyway, and that category then has no leftover.
- **FR-017**: Each category's leftover budget MUST then raise the quality of that category's own
  replacement items only, never above the quality cap. The apparel leftover never upgrades the
  weapon, for example. The default cap is Masterwork, or Excellent when Itemization Overhaul is
  loaded.
- **FR-018**: What is left in all three categories after quality MUST be pooled into one amount
  that buys traits from the whitelist at each trait's configured cost, up to the trait limit
  (default 2). A trait that conflicts with one the pawn already has MUST NOT be bought. The pooled
  budget left after traits MUST be discarded.
- **FR-019**: A reroll that can't complete for a pawn MUST leave that pawn exactly as it was and
  log the reason.

**Settings**

- **FR-020**: The settings window MUST offer each of these, with the default shown and kept across
  game restarts:
  - eligible pawn categories (default Hostile only)
  - allow named bosses and heroes (default off)
  - quality cap (default Masterwork, or Excellent with Itemization Overhaul)
  - trait whitelist with per-trait costs (default list in PRD §4.5)
  - trait limit (default 2)
  - reroll Elite Raid Plus bionics (default on)
  - era-check jewelry and charms (default off)
  - eras for unlabeled items (default Spacer)
  - eras for unknown materials (default: guessed from market value)
  - which drugs count as combat drugs (default: detected from each drug's effects)
- **FR-021**: The trait whitelist MUST list every trait in the game, including modded traits, and
  let the player search it.

**Compatibility** (each rule applies only when that mod is loaded; Downgrade MUST work with any of
them absent)

- **FR-022**: With Combat Extended, a pawn given a ranged weapon that uses ammo MUST also get ammo
  that fits that weapon.
- **FR-023**: With Itemization Overhaul, Downgrade MUST set each replacement's tier, and the
  bonuses MUST be rolled by Itemization Overhaul for that tier. Relic-tier items MUST be skipped.
  Jewelry and charms MUST be left alone unless the jewelry setting is on.
- **FR-024**: With Elite Raid Plus, its elite buffs MUST be kept. Bionics it added MUST be
  rerolled unless that setting is off.
- **FR-025**: With Isekai RPG Leveling, pawn levels, stats, ranks, rank traits and auras MUST be
  left unchanged.
- **FR-026**: Named bosses and heroes MUST be recognized, including Itemization Overhaul's Apex
  champions and the Isekai Guild's legendary hero.

**Scope**

- **FR-027**: Rerolling MUST NOT change raid size, raid points, faction tech level or behavior,
  trader stock, or the player's research.

### Key Entities

- **Era**: one of Neolithic, Medieval, Industrial, Spacer or Ultra. Ordered, so that "above the
  era" is well defined.
- **Reroll record** (per pawn, saved with the game): the recorded budget for weapons, apparel and
  implants, plus the items and traits Downgrade gave the pawn on its last reroll.
- **Trait whitelist entry**: a trait, whether it's enabled, and its silver cost.
- **Era assignment**: the player's chosen era for an unlabeled item or an unknown material.
- **Combat-drug assignment**: the player's choice of whether a drug counts as a combat drug,
  overriding the automatic detection.
- **Settings**: the global options in FR-020, plus the last-picked era.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a reroll to any era, 100% of the pawn's weapons, apparel, implants and carried
  combat drugs are at or below that era. Skipped Relics and jewelry left alone by settings are the
  only exceptions.
- **SC-002**: The player can reroll a 20-pawn raid in under 30 seconds of real time, with the game
  showing no visible pause while it does.
- **SC-003**: Rerolling the same pawn to the same era 10 times in a row never spends more than its
  recorded budget, never stacks leftovers, and its total gear value doesn't trend downward.
- **SC-004**: A raid has the same number of pawns before and after a reroll, and every rerolled
  pawn keeps fighting.
- **SC-005**: With Combat Extended loaded, 100% of rerolled pawns with an ammo-using ranged weapon
  can fire it.
- **SC-006**: A rerolled pawn's recorded budget is identical after a save and reload, and its next
  reroll spends from that same budget.
- **SC-007**: Zero dev-console errors from Downgrade in each of: a full test session on 1.7 with
  no compatibility mods, a compatibility session with Combat Extended and Itemization Overhaul on
  the newest version they support (1.6 today), and a load test on 1.6 (constitution v1.1.0,
  Principle VI).
- **SC-008**: Killing a rerolled pawn never yields any of its original above-era items.

## Assumptions

- **New traits are added on top of existing ones.** The pawn's own traits are never removed or
  replaced.
- **The last-picked era is a single global choice,** not one per pawn or per faction.
- **Combat Extended ammo:** the amount given is comparable to what that weapon's users normally
  carry. The exact amount is a planning detail.
- **Era labels:** the five eras match RimWorld's own tech levels. Animal and Archotech aren't
  offered as choices. Items labeled Archotech count as above every offered era.
- **Planning checks:** the PRD §7 checks (V.O.I.D. item eras, Isekai rank traits, named-boss
  detection) are resolved in `/speckit-plan` and don't change this spec's behavior.
- **Out of scope** (PRD §6): automatic caps, monsters, raid scaling, trader stock and research.

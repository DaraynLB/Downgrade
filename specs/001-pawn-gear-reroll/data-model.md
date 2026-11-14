# Data Model: Pawn Gear Reroll

**Feature**: [spec.md](spec.md) | **Research**: [research.md](research.md)

## Era

The game's `RimWorld.TechLevel`, restricted for display to **Neolithic, Medieval, Industrial,
Spacer and Ultra**.

- **"Above the era"**: an item's effective era is greater than the chosen era.
- **Effective era of an item**:
  1. the player's override in settings, if any
  2. otherwise `def.techLevel`
  3. if that's `Undefined`: Spacer
  4. `Archotech` is always above every offered era

## RerollRecord (per pawn, saved with the game)

Held by `CompDowngrade` on the pawn and scribed in `PostExposeData`.

| Field | Type | Rule |
|---|---|---|
| `hasRecord` | bool | False until the first successful reroll |
| `weaponBudget`, `apparelBudget`, `implantBudget` | float (silver) | Base value of above-era items replaced (FR-014). Set at the first reroll, then increased by the value of newly found above-era items on later rerolls (Edge Cases, "Second reroll"). Never decreases |
| `givenThings` | list of Thing refs | Items Downgrade gave on the last reroll. Taken away and bought again on the next one |
| `givenHediffs` | list of (HediffDef, BodyPartRecord) | Era body parts Downgrade installed. Removed and bought again on the next reroll |
| `addedTraits` | list of (TraitDef, degree) | Traits Downgrade added. Removed on the next reroll, then bought again (R13) |
| `lastEra` | TechLevel | For the summary and logs only |

**Rules**

- Budgets are ≥ 0 and are never lowered by spending. Spending happens on a working copy for each
  reroll.
- `givenThings` entries no longer worn, held or carried by the pawn are dropped from the list at
  the next reroll. They're not destroyed, because the player may have taken them.
- **Atomic** (FR-019, constitution Principle III): a reroll computes a full plan, makes every new
  item, then applies the plan through an undo log (`RerollTransaction`):
  1. Originals are taken off through their normal paths but **held aside, not destroyed**. Removed
     `Hediff` and `Trait` objects are kept too.
  2. Additions run in order of how easily they undo: gear, then traits, then implants.
  3. On any exception, completed steps are reversed, the originals are put back, the new items
     are destroyed, and the record is left as it was.
  4. Only after every step succeeds are the originals destroyed and the record written.
- **Limits of the guarantee**: Downgrade restores the pawn's gear, implants, traits and its own
  record. Side effects other mods attach to unequip or equip notifications are best-effort and
  may not be reversed. If putting the originals back also throws, that is an invariant break and
  is logged with `Log.Error`.

**Lifecycle**

```text
no record --first reroll--> recorded (budgets set)
recorded --reroll--> recorded (given items/traits replaced; budgets += newly found above-era value)
```

## RerollPlan (in memory, one per pawn per reroll)

| Field | Content |
|---|---|
| `removeThings` | Above-era weapons, apparel, drugs and matching CE ammo, plus last reroll's `givenThings` |
| `removeHediffs` | Above-era implants, last reroll's `givenHediffs`, and active effects of above-era drugs |
| `removeTraits` | Last reroll's `addedTraits` |
| `addThings` | List of (ThingDef, stuff, quality, slot), plus CE ammo |
| `addHediffs` | List of (HediffDef, part), and parts that end natural (the implant hediff is removed; `RestorePart` only if the part still reads as missing) |
| `addTraits` | List of (TraitDef, degree) |
| `spent` / `leftover` | Per category, plus the pooled trait budget (for the log) |

**Skips** (never placed in `removeThings`):

- Relic-tier items when Itemization Overhaul is loaded (`Legendary` quality, R7)
- jewelry and charms when the jewelry setting is off (C4 rule)
- items at or below the era

## Settings (global, `ModSettings`)

| Field | Type | Default |
|---|---|---|
| `allowHostile`, `allowVisitors`, `allowTraders`, `allowPrisoners`, `allowColonists` | bool | true, false, false, false, false |
| `allowNamedBosses` | bool | false |
| `qualityCap` | QualityCategory? | null, meaning automatic: Masterwork, or Excellent with Itemization Overhaul |
| `maxTraitsAdded` | int, 0–5 | 2 |
| `traitCosts` | dict `"defName:degree"` → (enabled bool, cost int) | from `TraitWhitelistDef` |
| `rerollEliteRaidBionics` | bool | true |
| `eraCheckJewelry` | bool | false |
| `itemEraOverrides` | dict ThingDef defName → TechLevel | empty (Undefined → Spacer) |
| `stuffEraOverrides` | dict stuff defName → TechLevel | empty (table, then price guess) |
| `combatDrugOverrides` | dict drug defName → bool | empty (automatic detection, research R12) |
| `lastPickedEra` | TechLevel | Medieval |

**Validation**

- Costs are ≥ 0.
- Unknown defNames (a mod was removed) are kept in the dictionaries but ignored. They aren't
  shown, and they come back if the mod returns.
- Trait entries whose `TraitDef` no longer exists are hidden.

## XML Defs (shipped data, Principle IV)

| Def type | Instance | Holds |
|---|---|---|
| `Downgrade.MaterialEraDef` | `Downgrade_MaterialEras` | Stuff defNames per era (PRD §4.4), plus per-unit price thresholds for guessing unknown stuff |
| `Downgrade.TraitWhitelistDef` | `Downgrade_StarterTraits` | Starter trait list with costs (PRD §4.5) |
| `Downgrade.TuningDef` | `Downgrade_Tuning` | `affordableBand` 0.5, `ceAmmoMagazines` 4, `maxTraitsCeiling` 5 |
| `Downgrade.CombatEffectsDef` | `Downgrade_CombatEffects` | Combat capacities, combat stats (with `lowerIsBetter`), and `painReductionCounts` true, used to detect combat drugs (research R12) |

## Marker extensions (contract for other mods; see [contracts/](contracts/))

| Extension | Placed on | Effect |
|---|---|---|
| `Downgrade.RerollExemptExtension` | PawnKindDef or HediffDef | The pawn is a named boss or hero (R4 of the PRD, FR-026) |
| `Downgrade.JewelryExtension` | ThingDef (apparel) | The item counts as jewelry or a charm |

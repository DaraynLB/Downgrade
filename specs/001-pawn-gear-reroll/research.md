# Research: Pawn Gear Reroll

**Feature**: [spec.md](spec.md) | **Date**: 2026-11-05

Everything below was checked against decompiled code, not recalled from memory:

- RimWorld **1.7.5000 rev1275**, the installed `Assembly-CSharp.dll`.
- **Itemization Overhaul – Core**, `LootAffixes.dll`.
- **Combat Extended**, `CombatExtended.dll`.
- The game's Core and DLC XML.

Mods that aren't installed on the dev machine are listed as open checks with a fallback.

## R1. Where the Reroll button lives: an injected ThingComp

- **Decision**: add a `ThingComp` (`CompDowngrade`) to every humanlike race ThingDef at startup. Its
  `CompGetGizmosExtra()` yields the Reroll command, and its `PostExposeData()` saves the pawn's
  reroll record (R9).
- **Evidence**: `ThingWithComps.GetGizmos()` yields every comp's `CompGetGizmosExtra()` with no
  faction check. `Pawn.GetGizmos()` calls the base first, and only drafting and attack gizmos are
  gated behind `IsColonistPlayerControlled`. `MapGizmoUtility.MapUIOnGUI` passes
  `Find.Selector.SelectedObjects` to `GizmoGridDrawer.DrawGizmoGridFor`, which collects
  `ISelectable.GetGizmos()` with no faction filter. So a comp gizmo shows on hostile, visitor,
  trader and prisoner pawns alike.
- **Why it covers modded races**: the comp is injected into every ThingDef where
  `race?.Humanlike == true`, so Big and Small races and serpent races get it too. Saved pawns get
  it as well, because `ThingWithComps.ExposeData` calls `InitializeComps()` from `def.comps` while
  loading.
- **Alternatives**:
  - A Harmony postfix on `Pawn.GetGizmos`: works, but it's a patch where none is needed, and the
    record would still need its own storage.
  - An XML patch adding the comp to `Human` only: misses modded races.

## R2. One click rerolls the whole selection

- **Decision**: `Command_Reroll : Command` overrides three things:
  - `GroupsWith`: true for any other `Command_Reroll`.
  - `InheritInteractionsFrom`: false, so grouped siblings don't each run `ProcessInput`.
  - `ProcessGroupInput(ev, group)`: opens one era float menu covering every pawn in the group.
- **Evidence**: on a click, `GizmoGridDrawer` calls `ProcessInput` on the clicked gizmo, calls
  `ProcessInput` on each sibling only when `InheritInteractionsFrom(sibling)` is true, and then
  calls `interactedGiz.ProcessGroupInput(ev, group)`. Both methods are virtual on `Verse.Gizmo`.
- **Alternatives**: a plain `Command_Action` would open one float menu per selected pawn.

## R3. "Above the era" is the item's own tech level

- **Decision**: compare `ThingDef.techLevel` (`RimWorld.TechLevel`) against the chosen era. The
  enum is ordered: Undefined, Animal, Neolithic, Medieval, Industrial, Spacer, Ultra, Archotech.
  - `Undefined` counts as Spacer unless the player overrides it in settings (FR-006).
  - Archotech is above every offered era.
- **Implants**: a hediff's era comes from its `HediffDef.spawnThingOnRemoved` ThingDef. Vanilla
  bionics are `techLevel Spacer` through `BodyPartBionicBase`. An implant hediff with no
  spawn-on-removal ThingDef falls back to that hediff's install recipe's ingredient or product.
  Failing that it is treated as Undefined, which means Spacer.
- **Drugs**: era comes from the drug ThingDef (for example `GoJuice`: `techLevel Industrial`).

## R4. Candidate replacement gear

- **Weapons**:
  - **Pool**: `ThingStuffPair.AllWith(td => td.equipmentType == EquipmentType.Primary &&
    !td.weaponTags.NullOrEmpty())`. This is the same predicate as `PawnWeaponGenerator.IsWeapon`.
  - **Filters**: era at or below the chosen era, same role (`IsRangedWeapon` / `IsMeleeWeapon`),
    no `CompBladelinkWeapon`, not `destroyOnDrop`, stuff allowed in the era (R6), and
    `EquipmentUtility.CanEquip(thing, pawn)`. `CanEquip` is the check that race-restriction
    frameworks patch.
- **Apparel**:
  - **Pool**: `ThingStuffPair.AllWith(td => td.IsApparel)`, the same as
    `PawnApparelGenerator.Reset`.
  - **Filters**: era, stuff, `apparel.PawnCanWear(pawn)`, `ApparelUtility.HasPartsToWear(pawn,
    def)`, and `EquipmentUtility.CanEquip`.
  - **Slot**: a candidate must share at least one body-part group *and* one layer with the item
    it replaces, and must pass `ApparelUtility.CanWearTogether` against every item the pawn keeps.
- **Pawn-kind tags deliberately ignored**: `CanUsePair` filters by the pawn kind's
  `apparelTags`. V.O.I.D. kinds only allow V.O.I.D. gear, which would leave no era candidates.
  Downgrade's whole purpose is to give gear outside that list, so weapon and apparel tag lists
  are not applied.
- **Choosing**: among affordable candidates, pick by weighted random over those priced at least
  half the most expensive affordable one, weighted by `ThingStuffPair.Commonality`. That gives
  "best it can afford" with variety between rerolls. The 0.5 band is a tunable constant
  (Principle IV).
- **Fallback (FR-016)**: if nothing is affordable, take the cheapest candidate.
- **Pools are cached** once per era after startup.
- **Not handed out as replacements** (decided during implement, 2026-11-10): grenades (a verb
  with `ai_IsBuildingDestroyer`) and one-shot launchers (`Verb_ShootOneUse`). They count as ranged
  weapons in the game's data, but a rifle replaced by a frag grenade isn't the same role. They are
  still *replaced* when above the era.
- **Several slots in one category** (analysis U3): replaced items are handled in descending base
  value. Each slot's allowance is the category's remaining budget times that slot's share of the
  remaining replaced value, so unspent shares carry forward to later slots. Inventory weapons
  (sidearms) are replaced too and the replacement goes back to inventory.
- **`CanEquip` checks** (analysis U2): `EquipmentUtility.CanEquip` needs a Thing. One unspawned
  probe per ThingDef (default stuff) is made on first use and cached for the session. Probes are
  never given, spawned or saved, so the planner still changes nothing on the pawn.

## R5. Prices are base values (FR-014)

- **Decision**: every price is `StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(def, stuff,
  quality))`, an abstract request with no Thing.
  - **Original item's budget contribution**: `For(item.def, item.Stuff)` at Normal quality.
  - **Quality step cost**: price at quality *q* minus price at Normal.
- **Evidence**:
  - `ThingStuffPair.Price` is `GetStatValueAbstract(MarketValue, stuff)`.
  - `StatRequest.For(BuildableDef, ThingDef, QualityCategory)` exists.
  - Core `MarketValue` has `StatPart_Quality`: Good ×1.25 (max gain 500), Excellent ×1.5 (1000),
    Masterwork ×2.5 (2000), Legendary ×5 (3000).
  - Itemization Overhaul's `StatPart_Quality.TransformValue` and stat patches only act when
    `req.HasThing`, so abstract requests stay vanilla. This keeps its rarity markup out of
    budgets.

## R6. Materials by era

- **Decision**: a `Downgrade.MaterialEraDef` XML Def lists stuff defNames per era, following PRD
  §4.4. A stuff counts as allowed for an era if it's listed at or below that era.
- **Unknown stuff** (in no list) gets an era guessed from per-unit market value. The thresholds
  are tunable constants in the Def. The player can override any stuff in settings (FR-011,
  FR-020).
- **Why XML**: tunable without recompiling (Principle IV), and other modders can patch it.

## R7. Quality and Itemization Overhaul tiers

- **Decision**: `thing.TryGetComp<CompQuality>()?.SetQuality(q, ArtGenerationContext.Outsider)`.
  The item is made at Normal, and each step up to the cap is bought from that category's leftover
  (FR-017).
- **Itemization Overhaul behavior** (decompiled):
  - `ThingMaker.MakeThing` → `CompAffixes.PostPostMake` → `EnsureRolled()` rolls a random tier.
  - The prefix on `CompQuality.SetQuality` sees `Rolled == true` and calls
    `ForceTierFromExternal(TierForQuality(q))`, which re-rolls affixes for that tier.
  - So "Downgrade sets the tier and Itemization Overhaul rolls the bonuses" (FR-023) needs no
    code that touches Itemization Overhaul.
  - Mapping: Normal = White, Good = Magic, Excellent = Rare, Masterwork = Legendary.
- **Relic detection**: Itemization Overhaul reports Relic or Apex items as `Legendary`
  (`AffixQualityMap.QualityForComp` returns `(QualityCategory)6` for `IsApex`). So with it loaded,
  "skip Relic items" means skip any item whose `TryGetQuality` is Legendary. That uses only the
  vanilla API.
- **Default cap**: `ModsConfig.IsActive("astryl.ItemizationOverhaulCore")` selects Excellent as the
  default instead of Masterwork.

## R8. Body parts and implants (FR-009)

- **Era equivalent**: hediffs with `addedPartProps` whose install recipe (`RecipeDef.addsHediff`)
  lists the same `BodyPartDef` in `appliedOnFixedBodyParts`, whose spawn ThingDef is at or below
  the era, and which are affordable from the implant budget. The most expensive affordable one
  wins.
  - **Install**: `pawn.health.AddHediff(def, part)`.
  - **Examples**: Core ships `Hediffs_BodyParts_Medieval.xml` (peg leg and similar),
    `_Prosthetic.xml` and `_Bionic.xml`.
- **No equivalent**: remove only the implant hediff, which leaves a natural part, as clarified.
  `pawn.health.RestorePart(part)` is a fallback, used only if `hediffSet.PartIsMissing(part)` is
  still true afterwards. It also wipes injuries on the part and the parts below it, so it can't be
  undone exactly, and runs as the last step of a reroll (data-model "Atomic").
- **Implants that don't replace a part** (brain implants and similar): remove the hediff.
- **Not counted as implants**: Elite Raid Plus buffs, Isekai rank effects, drug highs and any
  hediff without a spawnable item. So they are never touched (FR-024, FR-025).

## R9. What is saved per pawn (FR-015, Principle III)

- **Decision**: `CompDowngrade.PostExposeData` scribes a `RerollRecord`:
  - the three recorded budgets
  - references to items Downgrade gave
  - the traits Downgrade added, as def and degree
  - the last era used
- **Why on the pawn**: the record travels with the pawn through caravans, world-pawn storage and
  save/load. A GameComponent dictionary would hold dangling references to garbage-collected
  world pawns.
- **The last-picked era is global** (FR-002), so it lives in mod settings and persists across
  saves and games.

## R10. Combat Extended ammo (FR-022), runtime only

- **Decision**: a reflection-based `CompatCE` module, active when
  `AccessTools.TypeByName("CombatExtended.CompAmmoUser")` resolves. It has no compile-time
  reference.
- **Steps** for each replacement ranged weapon that has a `CompAmmoUser` whose `UseAmmo` is true:
  1. Read `Props.ammoSet.ammoTypes[0].ammo`.
  2. Call `ResetAmmoCount(ammo)` to fill the magazine.
  3. Add `perMag × AmmoMagazines` rounds to inventory. `AmmoMagazines` is a tunable constant,
     default 4. `perMag` matches CE's own pawn generation (`LoadoutPropertiesExtension`):
     `MagSizeOverride` when above 0, else `MagSize`, at least 1. Bows have no magazine and set
     `AmmoGenPerMagOverride` instead (Great bow: 5), so they get 20 arrows rather than 4.
- **Removing old ammo**: inventory stacks whose def is in a *replaced* weapon's
  `ammoSet.ammoTypes` are destroyed with the weapon, so charge cells can't be looted (FR-012).
  Given ammo is recorded in `givenThings`, so the next reroll removes it even if the pawn lost
  the weapon it came with. Inventory adds don't merge stacks, so recorded stacks keep their
  identity.
- **Found while implementing**: `CompAmmoUser.CurAmmoSet` (virtual) is read before
  `Props.ammoSet`, because some weapons override the ammo set. Ammo isn't priced against the
  weapon budget.
- **Evidence**: `CompAmmoUser` has public `Props`, `UseAmmo`, `MagSize`, `CurrentAmmo` and
  `ResetAmmoCount(AmmoDef)`. `CompProperties_AmmoUser.ammoSet` is an `AmmoSetDef` with
  `List<AmmoLink> ammoTypes`, and `AmmoLink.ammo` is an `AmmoDef`.
- **Version finding**: the installed Combat Extended only ships a `<v1.6>` LoadFolders entry, so
  CE can only be tested on RimWorld 1.6. The same goes for Itemization Overhaul (1.6) and
  V.O.I.D. (1.4–1.6). See plan.md, Complexity Tracking.

## R11. Named bosses and heroes (FR-026): data, not code

- **Decision**: a marker `DefModExtension`, `Downgrade.RerollExemptExtension`. A pawn is exempt if
  its `kindDef` carries the marker, or if any of its hediffs' defs does.
- **Itemization Overhaul**: a conditional patch (`PatchOperationFindMod` "Itemization Overhaul -
  Core") adds the marker to:
  - the four `LootAffixes_ApexBoss_*` PawnKindDefs
  - the `LootAffixes_ApexChampion` HediffDef, which also covers quest-customized bosses
    (`QuestPart_ApexDuel.CustomizeBoss`)
- **Isekai Guild hero**: a defName check is needed once the mod is installed. Open check C2.
- **Why**: this follows Principle II (no hard-coded pawn kinds in code), and other modders can
  mark their own bosses.

## R12. Drugs (FR-010)

- **Which drugs count**: only **combat drugs** (clarified 2026-11-10). `CombatDrugs.IsCombatDrug(def)`
  is the player's override in settings if there is one, otherwise this automatic rule:
  - Look at every `IngestionOutcomeDoer_GiveHediff.hediffDef` in `def.ingestible.outcomeDoers`,
    skipping the chemical's addiction and tolerance hediffs.
  - The drug is a combat drug if any stage of any of those hediffs **improves** something listed
    in the `Downgrade_CombatEffects` def:
    - a `capMods` entry for a listed capacity with `offset > 0` or `postFactor > 1`
    - a `statOffsets` or `statFactors` entry for a listed stat that moves it in its better
      direction (higher, or lower for stats marked `lowerIsBetter`)
    - pain reduction (`painFactor < 1` or `painOffset < 0`), while `painReductionCounts` is true
  - Effects that make a pawn worse never count. Beer lowers consciousness and moving, so beer
    alone is not a combat drug.
- **Combat effects list** (`Downgrade_CombatEffects`, all defNames confirmed in Core):
  - capacities: `Consciousness`, `Moving`, `Manipulation`, `Sight`, `BloodPumping`, `Breathing`
  - stats, higher is better: `ShootingAccuracyPawn`, `MeleeHitChance`, `MeleeDodgeChance`,
    `MeleeDamageFactor`, `MoveSpeed`, `PainShockThreshold`, `ArmorRating_Sharp`,
    `ArmorRating_Blunt`, `ArmorRating_Heat`
  - stats, lower is better: `AimingDelayFactor`, `MeleeCooldownFactor`, `RangedCooldownFactor`
    (attack speed), `IncomingDamageFactor`, `MentalBreakThreshold`
  - It's a list of game stats, not of items, factions or pawn kinds, so it fits Principle II. It
    lives in XML so it can be tuned and patched (Principle IV).
- **Vanilla result** (checked against Core 1.7.5000 hediff XML):

  | Drug | Era | Effect that qualifies | Combat drug? |
  |---|---|---|---|
  | Go-juice | Industrial | Consciousness, Moving, Sight ↑, pain ↓ | Yes |
  | Wake-up | Industrial | Consciousness, Moving ↑ | Yes |
  | Yayo | Industrial | Moving ↑, pain ↓ | Yes |
  | Flake | Industrial | pain ↓ (`painFactor 0.5`) | Yes |
  | Luciferium | Ultra | Consciousness, Moving, Sight, BloodPumping, Breathing ↑, pain ↓ | Yes |
  | Penoxycyline | Industrial | none (plague immunity only) | No, kept |
  | Beer, smokeleaf, psychite tea, ambrosia | Neolithic | n/a | Never above an offered era |

- **Removal**: inventory things where `IsCombatDrug(def)` and the drug is above the era. Then the
  active hediffs listed in the outcome doers of *every* above-era combat drug def, since the pawn
  may have dosed before arriving.
- **Kept**: addiction hediffs (`Hediff_Addiction`) and tolerance hediffs, which are the
  chemical's tolerance hediff and not an outcome doer's. Non-combat drugs and their effects.
- **Cached** once per drug after startup; the cache is cleared when the player changes an
  override.

## R13. Traits (FR-018)

- **Add**: `pawn.story.traits.GainTrait(new Trait(def, degree))`.
- **Skip**: a candidate is skipped if `HasTrait(def)` or if any existing trait has
  `def.ConflictsWith(candidate)`.
- **Remove on re-reroll**: `RemoveTrait(trait)`, only for traits recorded in `RerollRecord`.
- **Choosing**: random among whitelisted, affordable, non-conflicting entries, one at a time,
  until the limit or the budget runs out.
- **Whitelist storage**: a dictionary of `traitDef:degree` to cost in mod settings, defaulting
  from a `Downgrade.TraitWhitelistDef` XML Def that holds the PRD §4.5 list.

## R14. Toolchain versions

- **Game**: RimWorld 1.7.5000 rev1275 installed (Steam beta branch).
- **References**:
  - `Krafs.Rimworld.Ref` **1.7.4998-unstable**, the newest on nuget.org on 2026-11-05, pinned for
    `net10.0`.
  - `1.6.*` for `net472`.
  - `Lib.Harmony` 2.3.* compile-only.
- **Harmony patches**: R1 and R2 need none. Harmony stays a declared dependency per the
  constitution, and `PatchAll()` runs so any compatibility patch added later works.

## Open checks (need mod files not on the dev machine)

| # | Check | Needs | Fallback until checked |
|---|---|---|---|
| C1 | Whether V.O.I.D. items declare `techLevel` | V.O.I.D. subscribed | `Undefined` → Spacer rule covers it (FR-006) |
| C2 | Isekai Guild hero pawn kind or marker; whether rank traits are real `TraitDef`s | Isekai RPG Leveling + Guild add-on | Hero not exempt until a marker patch exists; trait conflict check (R13) protects rank traits |
| C3 | How Elite Raid Plus records the bionics it adds | Elite Raid Plus | Setting "reroll Elite Raid Plus bionics" = off leaves *all* implants alone on pawns carrying an Elite Raid Plus elite hediff |
| C4 | How Itemization Overhaul Jewelry & Charms marks its items | that module | Apparel in a ThingCategory whose defName contains "Jewelry" or "Charm" counts as jewelry |

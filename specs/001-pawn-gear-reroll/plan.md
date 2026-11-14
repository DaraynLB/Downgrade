# Implementation Plan: Pawn Gear Reroll

**Branch**: `001-pawn-gear-reroll` | **Date**: 2026-11-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-pawn-gear-reroll/spec.md`

## Summary

A Reroll command on eligible pawns replaces every weapon, apparel item, implant and combat drug
above a chosen era with era gear. The new gear is bought from the base value of what was replaced,
then leftover budget buys quality (per category) and pooled combat traits.

**Approach** (all confirmed against decompiled 1.7.5000 code, [research.md](research.md)):

- **The button and the saved data** come from a `ThingComp` injected into every humanlike race.
  The comp provides the gizmo, which RimWorld shows on any selected pawn regardless of faction,
  and it stores the pawn's saved reroll record.
- **One menu for a multi-selection**: a grouped `Command` opens one era menu for the whole
  selection.
- **Atomic reroll**: each pawn's reroll is computed as a full plan first, then applied, so a
  failure leaves that pawn untouched.
- **Prices** come from abstract market-value requests, which keeps other mods' quality and rarity
  markups out of budgets.
- **Other mods**: Combat Extended through reflection only. Itemization Overhaul needs no code,
  because its own `SetQuality` hook re-rolls affixes to the tier we set. Bosses are exempted
  through conditional XML patches.

## Technical Context

**Language/Version**: C# (`LangVersion latest`), multi-targeted `net472` (RimWorld 1.6) and
`net10.0` (RimWorld 1.7)

**Primary Dependencies**:

- `Krafs.Rimworld.Ref`: `1.6.*` for net472, and `1.7.4998-unstable` for net10.0, the newest
  available on 2026-11-05. The installed game is 1.7.5000.
- `Lib.Harmony` 2.3.*, compile-only (`ExcludeAssets="runtime"`).
- Runtime: the Harmony mod.

**Storage**: save-game data via `Scribe` (per-pawn `RerollRecord` on `CompDowngrade`). Global
options via `ModSettings`. Shipped tuning data in XML Defs.

**Testing**: in-game verification through [quickstart.md](quickstart.md) (Principle VI). Session A
runs on 1.7 vanilla, and Session B on 1.6 with Combat Extended and Itemization Overhaul. A Dev
Mode debug action, "Downgrade: dry-run reroll", logs the plan for the selected pawn without
applying it, which makes budget math checkable from `Player.log`. There's no automated test
project, because RimWorld types can't run outside the game.

**Target Platform**: RimWorld 1.6 (Unity/Mono) and 1.7 (Waylith, .NET 10 CoreCLR) on Windows, one
codebase, with `LoadFolders.xml` selecting `1.6/` or `1.7/`.

**Project Type**: RimWorld mod (C# assembly plus XML Defs and Patches).

**Performance Goals**:

- Rerolling 20 pawns completes within one frame budget, with no visible pause (SC-002).
- Candidate pools are built once at startup and cached per era. Planning is linear in pool size,
  a few hundred pairs.

**Constraints**:

- No compile-time reference to any other mod.
- Never touch raid, faction, trader or research state (Principle V).
- No per-tick work.

**Scale/Scope**: about 15 C# files, 4 XML Defs, 1 conditional patch file, 1 keyed language file,
and 1 icon.

## Constitution Check

*GATE: checked before Phase 0, and re-checked after Phase 1 design.*

| Principle | Status | How the design meets it |
|---|---|---|
| I. Dual-mode CE | PASS | `CompatCE` resolves CE types by name at startup. If they're absent, the module is off and no CE code path runs. Ammo is given and old ammo removed (R10) |
| II. Generic, soft compat | PASS | The era comes from each def's `techLevel`, and quality goes through `CompQuality.SetQuality`. No item, faction or pawn-kind lists in code. Mod-specific behavior sits in `Source/Compat/` behind `ModsConfig`/type checks, plus `Patches/` using `PatchOperationFindMod`. `Undefined` → Spacer with an override |
| III. Fair, repeatable rerolls | PASS | Base-value budgets (R5) recorded on the pawn (R9). The executor plans first, then applies through an undo log: originals are held aside, not destroyed, until every addition succeeds, and any failure puts them back (FR-019). The order is gear → quality → pooled traits → discard. Removal uses `apparel.Remove`, `equipment.Remove` and container removal, then `Destroy()`, so equip/unequip notifications fire for other mods (FR-013) |
| IV. Tunable defaults | PASS | `MaterialEraDef`, `TraitWhitelistDef` and `TuningDef` XML. Settings defaults mirror PRD §4.3–§4.6 |
| V. Scope discipline | PASS | Only the selected pawn's gear, implants, drug hediffs and Downgrade-added traits are touched |
| VI. Verify before done | PASS (constitution v1.1.0) | Stories 1–5 are fully verified on 1.7 with no compatibility mods. Story 6 is verified on 1.6, the newest version Combat Extended and Itemization Overhaul support (R10). Release docs name the tested version for each mod. A 1.6 load smoke test runs before release |

**Post-design re-check**: all PASS. The Principle VI deviation was resolved by amending the constitution to v1.1.0 on 2026-11-10.

## Project Structure

### Documentation (this feature)

```text
specs/001-pawn-gear-reroll/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── ui-contract.md
│   ├── log-contract.md
│   └── mod-extension-contract.md
├── checklists/requirements.md
└── tasks.md            # /speckit-tasks
```

### Source Code (repository root)

```text
About/
  About.xml                         # daraynlb.downgrade, DaraynLB, 1.6 + 1.7, Harmony dependency
  Preview.png
LoadFolders.xml
1.6/Assemblies/                     # net472 build output
1.7/Assemblies/                     # net10.0 build output
Defs/
  Downgrade_MaterialEras.xml        # MaterialEraDef: PRD §4.4 + price thresholds
  Downgrade_StarterTraits.xml       # TraitWhitelistDef: PRD §4.5
  Downgrade_Tuning.xml              # TuningDef: band 0.5, CE magazines 4, trait ceiling 5
  Downgrade_CombatEffects.xml       # CombatEffectsDef: combat capacities and stats for combat-drug detection
Patches/
  Compat_ItemizationOverhaul.xml    # RerollExemptExtension on Apex kinds + ApexChampion hediff
Languages/English/Keyed/
  Downgrade.xml
Textures/UI/Commands/
  Downgrade_Reroll.png
Source/
  Downgrade.csproj                  # dual target + PublishToModsFolder target
  DowngradeMod.cs                   # Mod subclass: settings, Harmony PatchAll, settings window
  Startup.cs                        # [StaticConstructorOnStartup]: inject comp, build pools, detect compat, log
  Settings/
    DowngradeSettings.cs            # data-model "Settings"
    SettingsWindow.cs               # contracts/ui-contract.md, settings section
  Data/
    MaterialEraDef.cs  TraitWhitelistDef.cs  TuningDef.cs  CombatEffectsDef.cs  Extensions.cs
  Core/
    Eras.cs                         # effective era of things, stuff and hediffs (data-model "Era")
    CombatDrugs.cs                  # which drugs count as combat drugs (research R12)
    Eligibility.cs                  # FR-001 + pawn category
    Pricing.cs                      # abstract market value, quality step costs (R5)
    GearPools.cs                    # cached candidate pools per era (R4)
    RerollPlan.cs                   # data-model "RerollPlan"
    RerollPlanner.cs                # builds the plan: weapons, apparel, implants, drugs, quality, traits
    RerollExecutor.cs               # applies a plan atomically, writes the record, logs
    RerollTransaction.cs            # undo log: holds originals, rolls back on failure
  Comps/
    CompProperties_Downgrade.cs  CompDowngrade.cs  RerollRecord.cs
  UI/
    Command_Reroll.cs               # grouped gizmo, era float menu, confirm, summary (R2)
  Compat/
    CompatCE.cs                     # reflection ammo (R10)
    CompatIO.cs                     # detection, Relic skip, jewelry rule, default cap (R7, C4)
    CompatERP.cs                    # detection + bionic rule (C3)
  Debug/
    DebugActions.cs                 # "Downgrade: dry-run reroll"
docs/
  PRD.md  settings-guide.md  steam-workshop-description.txt  updates/
README.md
```

**Structure Decision**: a single mod project with the standard `rimworld-modding` layout. The
reroll is split into a planner (pure decisions, no side effects) and an executor (side effects
only), and the executor applies changes through `RerollTransaction` so they can be undone. That split is what makes FR-019's "unchanged on failure" achievable, and it lets the dry-run
debug action reuse the planner.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Harmony declared but no patches in this feature | The constitution makes Harmony a hard dependency. Later compat fixes will need it | Removing the dependency would require a constitution amendment for no player benefit |

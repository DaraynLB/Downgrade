# Mod Extension Contract

Public, stable XML hooks so other mods (and Downgrade's own conditional patches) can tune
behavior without code. Class names are part of the contract and must not change.

## `Downgrade.RerollExemptExtension`

Marks a pawn as a named boss or hero. It gets no Reroll button unless the player turns on
"Allow named bosses and heroes".

Allowed on: `PawnKindDef`, `HediffDef`. A pawn is exempt if its kind carries it, or if any hediff
it has does.

```xml
<Operation Class="PatchOperationFindMod">
  <mods><li>Itemization Overhaul - Core</li></mods>
  <match Class="PatchOperationAddModExtension">
    <xpath>Defs/HediffDef[defName="LootAffixes_ApexChampion"]</xpath>
    <value><li Class="Downgrade.RerollExemptExtension" /></value>
  </match>
</Operation>
```

Downgrade ships patches like this for:

- Itemization Overhaul: `LootAffixes_ApexChampion` and `LootAffixes_ApexBoss_*`.
- The Isekai Guild hero, once open check C2 identifies it.

## `Downgrade.JewelryExtension`

Marks an apparel ThingDef as jewelry or a charm (PRD §4.2): left alone unless "Era-check jewelry
and charms" is on.

## `Downgrade.MaterialEraDef`

Other mods can add their materials to an era by patching `Downgrade_MaterialEras`:

```xml
<Operation Class="PatchOperationAdd">
  <xpath>Defs/Downgrade.MaterialEraDef[defName="Downgrade_MaterialEras"]/medieval</xpath>
  <value><li>MyMod_Bronze</li></value>
</Operation>
```

Fields: `neolithic`, `medieval`, `industrial`, `spacer`, `ultra` (lists of stuff defNames), and
`priceThresholds` (per-unit market value upper bounds used to guess an unknown material's era).

## `Downgrade.CombatEffectsDef`

Lists what counts as a combat effect when Downgrade detects combat drugs (research R12). Other
mods can add their own capacities or stats by patching `Downgrade_CombatEffects`:

```xml
<Operation Class="PatchOperationAdd">
  <xpath>Defs/Downgrade.CombatEffectsDef[defName="Downgrade_CombatEffects"]/stats</xpath>
  <value><li><stat>MyMod_DodgeBonus</stat></li></value>
</Operation>
```

Fields: `capacities` (list of `PawnCapacityDef`), `stats` (list of entries with `stat` and
`lowerIsBetter`, default false), and `painReductionCounts` (bool, default true).

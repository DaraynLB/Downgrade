# UI Contract: Reroll Command and Settings

What the player sees and does. Text in quotes is the English default; all strings go through
`Languages/English/Keyed/Downgrade.xml`.

## Reroll command (gizmo)

| Property | Value |
|---|---|
| Shown on | Each selected pawn that passes eligibility (FR-001) |
| Label | "Reroll gear" |
| Tooltip | "Replace this pawn's gear above the chosen era with era gear of the same value. Last era: {era}." |
| Icon | `Textures/UI/Commands/Downgrade_Reroll.png` |
| Grouping | All selected pawns' Reroll commands merge into one button (research R2) |
| Disabled (shown greyed) | Never. Ineligible pawns simply have no button |

**Eligible pawn** (FR-001), all of these must hold:

1. Spawned, alive, on the current map.
2. Has `apparel` and `equipment` trackers.
3. Category enabled in settings. A pawn's category is the first that matches:
   - Colonist (`IsColonist`)
   - Prisoner (`IsPrisonerOfColony`)
   - Hostile (`HostileTo(Faction.OfPlayer)`)
   - Trader (`pawn.GetTraderCaravanRole() != TraderCaravanRole.None`)
   - Visitor (anyone else)
4. Not exempt (no `RerollExemptExtension`), or `allowNamedBosses` is on.

## Click flow

1. **Click** opens a float menu with five options, "Neolithic", "Medieval", "Industrial",
   "Spacer" and "Ultra", with the last-picked one marked "(last used)".
2. **Pick an era.**
   - If the group includes any colonist or prisoner: a confirmation dialog, "Reroll {n} pawn(s)
     to {era}? Their above-era gear will be destroyed. This can't be undone." with "Reroll" and
     "Cancel" buttons (FR-004).
   - Otherwise it runs at once.
3. **Run.** Each pawn is rerolled independently. One pawn failing doesn't stop the others
   (FR-019).
4. **Result message** (top-left, neutral): "Rerolled {r} pawn(s) to {era}." plus, if any:
   - " {s} skipped." The skipped count covers pawns with nothing above the era and pawns that
     failed; failures are also logged.
   - With a single pawn and nothing to change: "{pawn} has no gear above {era}."

## Settings window (`Options → Mod Settings → Downgrade`)

Sections in this order, scrollable:

1. **Who can be rerolled**: 5 checkboxes, plus "Allow named bosses and heroes".
2. **Leftover budget**:
   - quality cap dropdown, with "Automatic (Masterwork, or Excellent with Itemization Overhaul)"
     plus each quality
   - max traits added, a slider from 0 to 5
3. **Trait whitelist**: a search box, then one row per trait degree with an enabled checkbox, the
   label and a cost field. "Reset to starter list" button.
4. **Compatibility**:
   - "Reroll bionics added by Elite Raid Plus" (shown only when Elite Raid Plus is loaded)
   - "Era-check jewelry and charms" (shown only when Itemization Overhaul is loaded)
5. **Unlabeled items**: a searchable list of weapons and apparel with no tech level, each with an
   era dropdown (default Spacer).
6. **Unknown materials**: a searchable list of stuff not in the material table, each with an era
   dropdown (default from the price guess).
7. **Combat drugs**: a searchable list of every drug, each with a "Combat drug" checkbox
   pre-filled by the automatic detection, its tech level, and a "(changed)" marker when the player
   overrode it.
8. **Reset all to defaults** button.

Every change applies immediately and is saved when the window closes.

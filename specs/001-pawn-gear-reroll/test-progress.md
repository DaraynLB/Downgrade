# Test progress: 001 Pawn Gear Reroll

One line per step. Verdicts cite Player.log line numbers (L#) or a screenshot.

Build under test: 1.7 DLL deployed 2026-11-11 20:09:14.

| Date | Scenario | Step | Verdict | Evidence |
|---|---|---|---|---|
| 2026-11-13 | T007 / A1 | Clean load, 1.7.5005, Harmony + Core + DLCs + Downgrade | ✅ PASS | L244 `Loaded v1.0.0 … 4 humanlike races. CE=off IO=off`; L270 `Pools: 89 weapons, 699 apparel, 0 implants`; no Downgrade errors (only L198 Steam WorkshopItem error) |
| 2026-11-13 | T023 | Settings open/close, file unchanged | ◐ PARTIAL | Mod_Downgrade_DowngradeMod.xml identical after open/close; restart half still to do. Settings window empty as expected (T055 not built) |
| 2026-11-13 | US1 / FR-001 | Reroll button shown on hostile (Mercenary_Sniper) | ✅ PASS | Screenshot: "Reroll gear" gizmo in command bar |
| 2026-11-13 | A2 | Sniper rifle → Medieval ranged | ✅ PASS | L365 `removed [Gun_SniperRifle…] added [Bow_Recurve/Normal…]`; gear tab shows Recurve bow |
| 2026-11-13 | A2 | Armor → Medieval-or-lower | ✅ PASS | L365 GasMask → CeremonialCultistMask/WoolBison (FullHead/Overhead, Neolithic); FlakVest removed, slot empty per FR-008 (only Medieval Torso+Middle fit is PlateArmor, which needs Shell, blocked by kept cloth parka) |
| 2026-11-13 | A2 / FR-012 | Originals not dropped | ✅ PASS | Screenshot: nothing on the ground at the pawn after reroll |
| 2026-11-13 | US1 | Dry run before reroll | ❓ NOT RUN | Skipped; redo on next pawn |
| 2026-11-13 | US1 | Dry run changes nothing and matches the real reroll | ✅ PASS (budgets/removals) | L405 dry run Pak to Neolithic, L424 reroll: same budgets (W=255 A=533), removed and added lists. Extra: L386 dry run Proz (sniper) to Medieval → NerveSpiker (Anomaly, Medieval def) |
| 2026-11-13 | A3 | Melee above era → Neolithic melee, Neolithic materials | ✅ PASS | L424 LongSword → MeleeWeapon_Spear/WoodLog (Neolithic def, Neolithic stuff); CollarShirt+Pants → TribalA/Leather_Panthera (Neolithic); ShieldBelt removed, no Neolithic belt |
| 2026-11-13 | US5 sc.1 | Non-hostile (visitor) pawn has no button by default | ✅ PASS | Screenshot: Toad (Tribal_Warrior, The Guulogo Treaty, friendly) shows no Reroll gear button |
| 2026-11-13 | A4 | Hostile tribal with nothing above Medieval → message, no change | ✅ PASS | Log `Skip Trabe: nothing above Medieval.`; on-screen "Trabe has no gear above Medieval."; gear tab unchanged (Steel club, Jade x3). Note: Spawn pawn always uses the kind's default faction (friendly tribe); hostile tribal obtained via "Execute raid with faction..." (Thoosod, 20 pts; vanilla L462 "No raid strategy found" error is from low points, not Downgrade) |
| 2026-11-13 | A5 / SC-008 | Killed rerolled pawn drops no original above-era items | ✅ PASS | Screenshot: Garrote corpse wears hood/shirt/parka/pants; ground has Recurve bow, meals, go-juice; no sniper rifle, flak vest or gas mask |
| 2026-11-13 | A15 / SC-007 | No red errors from Downgrade | ❌ FAIL | L(21:35:53) `InvalidCastException: Unable to cast 'Verse.Corpse' to 'Verse.Pawn'` in ThingWithComps.GetGizmos on selecting the corpse: comp injected into corpse defs too (they carry `race`); `CompDowngrade.Pawn => (Pawn)parent`. Same cast in PostExposeData → saving with a humanlike corpse on the map would also throw |
| 2026-11-13 | #1 fix | Built and deployed (1.7 DLL 21:38:29); awaiting re-run | — | tasks.md T036a |
| 2026-11-13 | #1 re-run | Startup race count no longer counts corpse defs | ✅ PASS | Player.log L250 `Comp added to 2 humanlike races` (was 4) |
| 2026-11-13 | #1 re-run | Select corpse of rerolled pawn, no error | ✅ PASS | Screenshot of Big (dead) corpse selected; Player.log ERROR count 0 after reroll L317 |
| 2026-11-13 | #1 re-run | Save with humanlike corpses on map | ✅ PASS | dg-test.rws written 21:41, no errors; Big's corpse saved with downgradeRecord (budgets 530/260, givenThings pruned to the worn headdress, dropped Pila removed) |
| 2026-11-13 | T023 | Settings unchanged across restart | ✅ PASS | Mod_Downgrade_DowngradeMod.xml identical to pre-session snapshot after relaunch |
| 2026-11-13 | A2 (repeat) | Mercenary_Sniper "Big" to Medieval | ✅ PASS (rules) / ⚠ design question | L317 SniperRifle → Pila, AdvancedHelmet → TribalHeaddress/WoolSheep. Kept a Hyperweave **jacket** (Apparel_Jacket def is Medieval; Hyperweave stuff is Spacer) — existing gear is era-checked by def only; open question for the client |
| 2026-11-13 | #1 re-run | Reload save with corpses, select corpse | ✅ PASS | Player.log L338 `Loading game from file dg-test`; no errors or warnings after it (log ends L351); user confirms the corpse is fine |
| 2026-11-13 | US1 sc.6 | B&S loaded: 65 humanlike races, no Downgrade startup errors | ✅ PASS | Player.log L294/L320 |
| 2026-11-13 | US1 | Dry run vs real reroll (Cook, Viper Family, Medieval) | ⚠ NOTE | Same budget W=135 and removed [Gun_Revolver]; dry run picked Bow_Great, real reroll picked Pila. Picks are weighted-random by design, so a dry run predicts budgets and removals, not the exact item |
| 2026-11-13 | US1 sc.6 | Cook (Town_Guard, Viper Family) to Medieval | ◐ PARTIAL | Revolver → Pila; no apparel above Medieval, so body fit not exercised yet |
| 2026-11-13 | US1 sc.6 | Lamia (B&S xenotype, Viper Family) to Neolithic: every replacement wearable | ✅ PASS | Player.log L432: Jacket → Apparel_Parka/Cloth (Shell, Torso), CollarShirt+Pants → Apparel_TribalA/Cloth (OnSkin, Torso+Legs), BowlerHat → BS_Nisse_Cap/Cloth (modded B&S hat, Overhead), Pila → Bow_Great; all Neolithic defs, Cloth is Neolithic. Gear tab screenshot shows all three worn. Note: the Lamia keeps a Legs group (it spawned wearing pants), so the "body part missing" branch wasn't exercised |
| 2026-11-13 | T036b | Decision 15 (material era) built and deployed, 1.7 DLL 21:52:53 | — | awaiting dry-run check |
| 2026-11-13 | T036b / decision 15 | Dry run Medieval on Hákon (BS_Jotun_Hunter, Niflheim giant) | ✅ PASS | L437 removed only [Apparel_BasicShirt] (hyperweave, Spacer material on a Medieval def); steel barbarian armor and megasloth wool pants kept |
| 2026-11-13 | T036b / decision 15 | Dry run Neolithic on Hákon | ✅ PASS | L456 removed [BS_BarbarianArmor (steel), Apparel_BasicShirt, Apparel_Pants (wool)]; added Parka/Leather_Elephant, TribalA/Leather_Bear; Giant bow kept |
| 2026-11-13 | T036b + US1 sc.6 | Real reroll Hákon to Medieval; replacement fits a giant | ✅ PASS | L475 BasicShirt → CollarShirt/Leather_GuineaPig; gear tab shows it worn. Only red lines in session are vanilla (Steam WorkshopItem, low-points raid strategy) |

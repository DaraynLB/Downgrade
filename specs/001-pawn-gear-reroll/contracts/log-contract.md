# Log Contract

In-game testing (the `rimworld-mod-testing` workflow) reads `Player.log` for evidence. Every line
starts with `[Downgrade]`, and the formats below are stable.

| When | Level | Format |
|---|---|---|
| Startup | Message | `[Downgrade] Loaded v{version}. Comp added to {n} humanlike races. CE={on/off} IO={on/off} ERP={on/off} Isekai={on/off}.` |
| Startup, data | Message | `[Downgrade] Pools: {w} weapons, {a} apparel, {i} implants. Unlabeled items: {u}. Unknown materials: {m}.` |
| Each pawn rerolled | Message | `[Downgrade] Reroll {pawnLabel} ({kindDef}, {faction}) to {era}: budget W={w} (gear {g}, quality {q}) A={a} (gear {g}, quality {q}) I={i} (gear {g}, quality {q}); removed [{defs}]; added [{def/stuff/quality}]; traits [{trait cost}, …] from pool {p}; discarded {d}.` |
| Dry run (Dev Mode) | Message | `[Downgrade] Dry run {pawnLabel} ({kindDef}, {faction}) to {era}: ` then the same text as a reroll line, or `nothing above {era}.` Nothing is applied |
| Rollback step failed | Error | `[Downgrade] Rollback of {step} failed for {pawnLabel}: {exception}` (an invariant break: the pawn may not be fully restored) |
| After each Reroll click (one or many pawns) | Message | `[Downgrade] Batch to {era}: {r} rerolled, {s} skipped ({n} with no button) in {ms} ms.` ({s} includes {n}; {ms} covers planning and applying every pawn) |
| Pawn skipped, nothing above era | Message | `[Downgrade] Skip {pawnLabel}: nothing above {era}.` |
| Reroll failed, pawn unchanged | Warning | `[Downgrade] Reroll failed for {pawnLabel}, left unchanged: {exception message}` |
| CE ammo given | Message | `[Downgrade] CE ammo {ammoDef} x{count} for {weaponDef}.` |
| Compat module error | Warning | `[Downgrade] {Module} compat disabled: {reason}` (once per session) |

Logging is always on, since it's one line per reroll and never per tick. Errors are logged with
`Log.Warning`, not `Log.Error`, unless Downgrade's own invariants break.

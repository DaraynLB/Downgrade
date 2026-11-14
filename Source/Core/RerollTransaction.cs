using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>
    /// Undo log for applying one reroll to one pawn (data-model.md "Atomic", FR-019, Principle III).
    /// Originals are taken off through their normal paths, so other mods' unequip hooks fire (FR-013),
    /// but are held aside, not destroyed, until <see cref="Commit"/>. <see cref="Rollback"/> reverses
    /// every completed step, newest first, and puts the originals back.
    /// </summary>
    public class RerollTransaction
    {
        private enum Kind
        {
            TakeOff,
            Put,
            RemoveTrait,
            GainTrait,
            RemoveHediff,
            AddHediff,
        }

        private class Step
        {
            public Kind kind;
            public Thing thing;
            public GearSlot slot;
            public bool wasLocked;
            public Trait trait;
            public Hediff hediff;
        }

        private readonly Pawn pawn;
        private readonly List<Step> done = new List<Step>();
        private readonly List<Thing> held = new List<Thing>();

        public RerollTransaction(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public static GearSlot SlotOf(Pawn pawn, Thing t)
        {
            if (t is Apparel ap && pawn.apparel.Contains(ap))
                return GearSlot.Apparel;
            if (t is ThingWithComps eq && pawn.equipment.Contains(eq))
                return GearSlot.Equipment;
            return GearSlot.Inventory;
        }

        // ---------- Things ----------

        public void TakeOff(Thing t)
        {
            var slot = SlotOf(pawn, t);
            bool locked = t is Apparel ap0 && pawn.apparel.IsLocked(ap0);
            switch (slot)
            {
                case GearSlot.Equipment: pawn.equipment.Remove((ThingWithComps)t); break;
                case GearSlot.Apparel: pawn.apparel.Remove((Apparel)t); break;
                default:
                    if (!pawn.inventory.innerContainer.Remove(t))
                        throw new InvalidOperationException($"{t} is not in {pawn.LabelShort}'s inventory");
                    break;
            }
            held.Add(t);
            done.Add(new Step { kind = Kind.TakeOff, thing = t, slot = slot, wasLocked = locked });
        }

        public void Put(Thing t, GearSlot slot)
        {
            Add(t, slot);
            done.Add(new Step { kind = Kind.Put, thing = t, slot = slot });
        }

        private void Add(Thing t, GearSlot slot)
        {
            switch (slot)
            {
                case GearSlot.Equipment:
                    if (pawn.equipment.Primary != null)
                        throw new InvalidOperationException($"{pawn.LabelShort} already holds {pawn.equipment.Primary}");
                    pawn.equipment.AddEquipment((ThingWithComps)t);
                    break;
                case GearSlot.Apparel:
                    var ap = (Apparel)t;
                    // Wear() with dropReplacedApparel=false would silently drop a conflicting item into nowhere.
                    foreach (var worn in pawn.apparel.WornApparel)
                    {
                        if (!ApparelUtility.CanWearTogether(ap.def, worn.def, pawn.RaceProps.body))
                            throw new InvalidOperationException($"{ap.def.defName} conflicts with worn {worn.def.defName}");
                    }
                    pawn.apparel.Wear(ap, dropReplacedApparel: false);
                    break;
                default:
                    // No merging: a merged stack is destroyed, which would break the record and rollback.
                    if (!pawn.inventory.innerContainer.TryAdd(t, canMergeWithExistingStacks: false))
                        throw new InvalidOperationException($"could not add {t} to {pawn.LabelShort}'s inventory");
                    break;
            }
        }

        private void Remove(Thing t, GearSlot slot)
        {
            switch (slot)
            {
                case GearSlot.Equipment: pawn.equipment.Remove((ThingWithComps)t); break;
                case GearSlot.Apparel: pawn.apparel.Remove((Apparel)t); break;
                default: pawn.inventory.innerContainer.Remove(t); break;
            }
        }

        // ---------- Traits ----------

        public void RemoveTrait(Trait trait)
        {
            pawn.story.traits.RemoveTrait(trait);
            done.Add(new Step { kind = Kind.RemoveTrait, trait = trait });
        }

        public void GainTrait(Trait trait)
        {
            pawn.story.traits.GainTrait(trait);
            done.Add(new Step { kind = Kind.GainTrait, trait = trait });
        }

        // ---------- Hediffs (run last: hardest to undo) ----------

        public void RemoveHediff(Hediff hediff)
        {
            pawn.health.RemoveHediff(hediff);
            done.Add(new Step { kind = Kind.RemoveHediff, hediff = hediff });
        }

        public Hediff AddHediff(HediffDef def, BodyPartRecord part)
        {
            var h = pawn.health.AddHediff(def, part);
            done.Add(new Step { kind = Kind.AddHediff, hediff = h });
            return h;
        }

        // ---------- Finish ----------

        /// <summary>Every step succeeded: destroy the originals (FR-012).</summary>
        public void Commit()
        {
            foreach (var t in held)
            {
                if (!t.Destroyed)
                    t.Destroy(DestroyMode.Vanish);
            }
            held.Clear();
            done.Clear();
        }

        /// <summary>Reverse every completed step, newest first, putting the originals back.</summary>
        public void Rollback()
        {
            for (int i = done.Count - 1; i >= 0; i--)
            {
                var s = done[i];
                try
                {
                    Undo(s);
                }
                catch (Exception e)
                {
                    DLog.InvariantBroken($"Rollback of {s.kind} failed for {pawn.LabelShort}: {e}");
                }
            }
            done.Clear();
            held.Clear();
        }

        private void Undo(Step s)
        {
            switch (s.kind)
            {
                case Kind.TakeOff:
                    Add(s.thing, s.slot);
                    if (s.wasLocked && s.thing is Apparel ap)
                        pawn.apparel.Lock(ap);
                    break;
                case Kind.Put:
                    Remove(s.thing, s.slot);
                    if (!s.thing.Destroyed)
                        s.thing.Destroy(DestroyMode.Vanish);
                    break;
                case Kind.RemoveTrait:
                    pawn.story.traits.GainTrait(s.trait);
                    break;
                case Kind.GainTrait:
                    pawn.story.traits.RemoveTrait(s.trait);
                    break;
                case Kind.RemoveHediff:
                    pawn.health.hediffSet.AddDirect(s.hediff);
                    break;
                case Kind.AddHediff:
                    pawn.health.RemoveHediff(s.hediff);
                    break;
            }
        }
    }
}

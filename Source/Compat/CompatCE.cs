using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Downgrade
{
    /// <summary>
    /// Combat Extended ammo (research R10, FR-022). Reflection only; no compile-time reference
    /// (Principle I). Any missing member or reflection failure turns the module off with one warning.
    /// </summary>
    public static class CompatCE
    {
        public static bool Active { get; private set; }

        private static Type ammoUserType;
        private static PropertyInfo propsProp;       // CompAmmoUser.Props
        private static PropertyInfo curAmmoSetProp;  // CompAmmoUser.CurAmmoSet (virtual; preferred when present)
        private static PropertyInfo useAmmoProp;     // CompAmmoUser.UseAmmo
        private static PropertyInfo magSizeProp;     // CompAmmoUser.MagSize
        private static PropertyInfo magOverrideProp; // CompAmmoUser.MagSizeOverride (optional; bows have no magazine)
        private static MethodInfo resetAmmoMethod;   // CompAmmoUser.ResetAmmoCount(AmmoDef)
        private static FieldInfo ammoSetField;       // CompProperties_AmmoUser.ammoSet
        private static FieldInfo ammoTypesField;     // AmmoSetDef.ammoTypes
        private static FieldInfo ammoField;          // AmmoLink.ammo

        private static int Magazines => DowngradeDefOf.Downgrade_Tuning.ceAmmoMagazines;

        public static void Detect()
        {
            Active = false;
            ammoUserType = AccessTools.TypeByName("CombatExtended.CompAmmoUser");
            if (ammoUserType == null)
                return; // CE not loaded: module off, silently.

            try
            {
                var propsType = AccessTools.TypeByName("CombatExtended.CompProperties_AmmoUser");
                var setType = AccessTools.TypeByName("CombatExtended.AmmoSetDef");
                var linkType = AccessTools.TypeByName("CombatExtended.AmmoLink");
                var ammoDefType = AccessTools.TypeByName("CombatExtended.AmmoDef");

                propsProp = AccessTools.Property(ammoUserType, "Props");
                curAmmoSetProp = AccessTools.Property(ammoUserType, "CurAmmoSet");
                useAmmoProp = AccessTools.Property(ammoUserType, "UseAmmo");
                magSizeProp = AccessTools.Property(ammoUserType, "MagSize");
                magOverrideProp = AccessTools.Property(ammoUserType, "MagSizeOverride");
                resetAmmoMethod = ammoDefType == null ? null
                    : AccessTools.Method(ammoUserType, "ResetAmmoCount", new[] { ammoDefType });
                ammoSetField = propsType == null ? null : AccessTools.Field(propsType, "ammoSet");
                ammoTypesField = setType == null ? null : AccessTools.Field(setType, "ammoTypes");
                ammoField = linkType == null ? null : AccessTools.Field(linkType, "ammo");

                var missing = new List<string>();
                if (propsType == null) missing.Add("CompProperties_AmmoUser");
                if (setType == null) missing.Add("AmmoSetDef");
                if (linkType == null) missing.Add("AmmoLink");
                if (ammoDefType == null) missing.Add("AmmoDef");
                if (propsProp == null) missing.Add("CompAmmoUser.Props");
                if (useAmmoProp == null) missing.Add("CompAmmoUser.UseAmmo");
                if (magSizeProp == null) missing.Add("CompAmmoUser.MagSize");
                if (resetAmmoMethod == null) missing.Add("CompAmmoUser.ResetAmmoCount(AmmoDef)");
                if (ammoSetField == null) missing.Add("CompProperties_AmmoUser.ammoSet");
                if (ammoTypesField == null) missing.Add("AmmoSetDef.ammoTypes");
                if (ammoField == null) missing.Add("AmmoLink.ammo");

                if (missing.Count > 0)
                {
                    DLog.CompatDisabled("CE", "missing " + string.Join(", ", missing));
                    return;
                }
                Active = true;
            }
            catch (Exception e)
            {
                DLog.CompatDisabled("CE", e.Message);
            }
        }

        private static void Disable(Exception e)
        {
            Active = false;
            DLog.CompatDisabled("CE", e.Message);
        }

        /// <summary>The weapon's CompAmmoUser, or null.</summary>
        private static object AmmoUser(Thing weapon) =>
            weapon is ThingWithComps twc ? twc.AllComps.FirstOrDefault(c => ammoUserType.IsInstanceOfType(c)) : null;

        /// <summary>The ammo defs of a comp whose UseAmmo is true, in ammo-set order; empty otherwise.</summary>
        private static List<ThingDef> AmmoDefs(object comp)
        {
            var result = new List<ThingDef>();
            if (comp == null || !(bool)useAmmoProp.GetValue(comp))
                return result;

            object set = curAmmoSetProp != null ? curAmmoSetProp.GetValue(comp) : null;
            if (set == null)
                set = ammoSetField.GetValue(propsProp.GetValue(comp));
            if (set == null || !(ammoTypesField.GetValue(set) is IEnumerable links))
                return result;

            foreach (var link in links)
            {
                if (link != null && ammoField.GetValue(link) is ThingDef ammo && !result.Contains(ammo))
                    result.Add(ammo);
            }
            return result;
        }

        /// <summary>Every ammo def the weapon can fire; empty when CE is off or the weapon uses no ammo.</summary>
        public static List<ThingDef> AmmoDefsOf(Thing weapon)
        {
            if (!Active || weapon == null)
                return new List<ThingDef>();
            try
            {
                return AmmoDefs(AmmoUser(weapon));
            }
            catch (Exception e)
            {
                Disable(e);
                return new List<ThingDef>();
            }
        }

        /// <summary>
        /// Loads the weapon with its first ammo type and returns the spare rounds to give:
        /// rounds per magazine × <c>ceAmmoMagazines</c>. Rounds per magazine follow CE's own pawn
        /// generation: <c>MagSizeOverride</c> if set (bows), else <c>MagSize</c>, at least 1.
        /// False when CE is off or the weapon uses no ammo. Never throws.
        /// </summary>
        public static bool TryFill(Thing weapon, out ThingDef ammo, out int count)
        {
            ammo = null;
            count = 0;
            if (!Active || weapon == null)
                return false;
            try
            {
                var comp = AmmoUser(weapon);
                var defs = AmmoDefs(comp);
                if (defs.Count == 0)
                    return false;

                ammo = defs[0];
                resetAmmoMethod.Invoke(comp, new object[] { ammo });

                int perMag = magOverrideProp != null ? (int)magOverrideProp.GetValue(comp) : 0;
                if (perMag <= 0)
                    perMag = (int)magSizeProp.GetValue(comp);
                count = Mathf.Max(1, perMag) * Mathf.Max(0, Magazines);
                return count > 0;
            }
            catch (Exception e)
            {
                Disable(e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e);
                ammo = null;
                count = 0;
                return false;
            }
        }
    }
}

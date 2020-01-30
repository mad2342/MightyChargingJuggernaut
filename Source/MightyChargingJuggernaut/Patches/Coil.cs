using System;
using BattleTech.UI;
using Harmony;

namespace MightyChargingJuggernaut.Patches
{
    class Coil
    {
        [HarmonyPatch(typeof(CombatHUDEvasiveBarPips), "CacheActorData")]
        public static class CombatHUDEvasiveBarPips_CacheActorData_Patch
        {
            public static void Postfix(CombatHUDEvasiveBarPips __instance, ref bool ___ShouldShowCOILPips)
            {
                try
                {
                    Logger.Info($"[CombatHUDEvasiveBarPips_CacheActorData_POSTFIX] Fields.JuggernautCharges: {Fields.JuggernautCharges}");
                    if (Fields.JuggernautCharges && ___ShouldShowCOILPips)
                    {
                        ___ShouldShowCOILPips = false;
                    }
                    Logger.Info($"[CombatHUDWeaponSlot_RefreshDisplayedWeapon_PREFIX] ___ShouldShowCOILPips: {___ShouldShowCOILPips}");
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(CombatHUDWeaponSlot), "RefreshDisplayedWeapon")]
        public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon_Patch
        {
            public static void Prefix(CombatHUDWeaponSlot __instance, ref bool sprinting)
            {
                try
                {
                    Logger.Info($"[CombatHUDWeaponSlot_RefreshDisplayedWeapon_PREFIX] Fields.JuggernautCharges: {Fields.JuggernautCharges}");
                    if (Fields.JuggernautCharges && !sprinting)
                    {
                        sprinting = true;
                    }
                    Logger.Info($"[CombatHUDWeaponSlot_RefreshDisplayedWeapon_PREFIX] sprinting: {sprinting}");
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}

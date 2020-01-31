using System;
using System.Reflection;
using BattleTech.UI;
using Harmony;
using Localize;

namespace MightyChargingJuggernaut.Patches
{
    class UserInterface
    {
        /*
        [HarmonyPatch(typeof(SelectionStateMove), "SetMeleeDest")]
        public static class SelectionStateMove_SetMeleeDest_Patch
        {
            public static void Postfix(SelectionStateMove __instance)
            {
                try
                {
                    if (Fields.JuggernautCharges)
                    {
                        Logger.Debug($"[SelectionStateMove_SetMeleeDest_POSTFIX] Overriding description...");

                        CombatHUDFireButton.FireMode mode = CombatHUDFireButton.FireMode.Engage;
                        string additionalDetails = "Sprint to TACKLE the target using Piloting skill to hit. Ignores EVASIVE. Hit removes GUARDED, deals damage and stability damage.";

                        // I really need to learn how to call private methods with Harmony
                        //Traverse.Create(__instance).Method("ShowFireButton", new object[] { mode, additionalDetails }).GetValue();

                        // Doesn't work either. Note that the desired method is declared in the base class of __instance (SelectionState)
                        //MethodInfo mi = typeof(SelectionState).GetMethod("ShowFireButton", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                        //mi.Invoke(__instance, new object[] { mode, additionalDetails });
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
        */


        [HarmonyPatch(typeof(SelectionStateMove), "FireButtonString", MethodType.Getter)]
        public static class SelectionStateMove_FireButtonString_Patch
        {
            public static void Postfix(SelectionStateMove __instance, ref string __result)
            {
                try
                {
                    if (__instance.HasDestination && Fields.JuggernautCharges)
                    {
                        Logger.Info($"[SelectionStateMove_FireButtonString_POSTFIX] Overriding description...");
                        __result = Strings.T("Sprint to TACKLE the target using Piloting skill to hit. Ignores EVASIVE. Hit removes GUARDED, deals damage and stability damage.");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(CombatHUDFireButton), "CurrentFireMode", MethodType.Setter)]
        public static class CombatHUDFireButton_CurrentFireMode_Patch
        {
            public static void Postfix(CombatHUDFireButton __instance, CombatHUDFireButton.FireMode value)
            {
                try
                {
                    if (value == CombatHUDFireButton.FireMode.Engage && Fields.JuggernautCharges)
                    {
                        Logger.Info($"[CombatHUDFireButton_CurrentFireMode_POSTFIX] Overriding FireText...");
                        __instance.FireText.SetText("CHARGE!");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}

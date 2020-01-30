using System;
using Harmony;
using BattleTech;
using MightyChargingJuggernaut.Extensions;

namespace MightyChargingJuggernaut.Patches
{
    class DFA
    {
        // DFAs from Juggernaut can directly knock a target down
        [HarmonyPatch(typeof(MechDFASequence), "OnMeleeComplete")]
        public static class MechDFASequence_OnMeleeComplete_Patch
        {
            public static void Prefix(MechDFASequence __instance, MessageCenterMessage message)
            {
                try
                {
                    AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
                    if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
                    {
                        Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] Attack did no damage! Aborting...");
                        return;
                    }

                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut())
                    {
                        ICombatant DFATarget = (ICombatant)AccessTools.Property(typeof(MechDFASequence), "DFATarget").GetValue(__instance, null);

                        if (DFATarget.IsDead || DFATarget.IsFlaggedForDeath)
                        {
                            return;
                        }

                        // IMPORTANT! At this point any stab dmg is already applied to the target, normalized by entrenched or terrain...
                        if (DFATarget is Mech TargetMech)
                        {
                            Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] DFATarget.IsUnsteady: " + TargetMech.IsUnsteady);
                            Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] DFATarget.MaxStability: " + TargetMech.MaxStability);
                            Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] DFATarget.CurrentStability: " + TargetMech.CurrentStability);

                            // Additional stability damage depending on distance jumped?
                            float additionalStabilityDamage = Utilities.GetAdditionalStabilityDamageFromJumpDistance(__instance.OwningMech, TargetMech, false);

                            // Using the attacker from the sequence is more reliable than __instance.OwningMech?
                            //Mech AttackingMech = attackCompleteMessage.attackSequence.attacker as Mech;
                            //float additionalStabilityDamage = Utilities.GetAdditionalStabilityDamageFromJumpDistance(AttackingMech, TargetMech, false);

                            Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] Apply additional stability damage from distance jumped: " + additionalStabilityDamage);

                            TargetMech.AddAbsoluteInstability(additionalStabilityDamage, StabilityChangeSource.NotSet, __instance.owningActor.GUID);
                            Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] DFATarget.CurrentStability: " + TargetMech.CurrentStability);

                            if (TargetMech.CurrentStability >= TargetMech.MaxStability)
                            {
                                Logger.Debug("[MechDFASequence_OnMeleeComplete_PREFIX] Mech should be knocked down regardless of being unsteady before...");
                                TargetMech.FlagForKnockdown();

                                if (!TargetMech.IsUnsteady)
                                {
                                    // Push message out
                                    TargetMech.Combat.MessageCenter.PublishMessage(new FloatieMessage(TargetMech.GUID, TargetMech.GUID, "OFF BALANCE", FloatieMessage.MessageNature.Debuff));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            // Check things
            public static void Postfix(MechDFASequence __instance)
            {
                try
                {
                    ICombatant DFATarget = (ICombatant)AccessTools.Property(typeof(MechDFASequence), "DFATarget").GetValue(__instance, null);
                    if (DFATarget is Mech TargetMech)
                    {
                        Logger.Debug("[MechDFASequence_OnMeleeComplete_POSTFIX] DFATarget.CurrentStability: " + TargetMech.CurrentStability);
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

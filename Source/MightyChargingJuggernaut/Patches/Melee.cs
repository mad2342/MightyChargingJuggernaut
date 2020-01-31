using System;
using Harmony;
using BattleTech;
using MightyChargingJuggernaut.Extensions;

namespace MightyChargingJuggernaut.Patches
{
    class Melee
    {
        [HarmonyPatch(typeof(MechMeleeSequence), "GenerateMeleePath")]
        public static class MechMeleeSequence_GenerateMeleePath_Patch
        {
            static void Postfix(MechMeleeSequence __instance, ref ActorMovementSequence ___moveSequence)
            {
                try
                {
                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut() && Fields.JuggernautCharges)
                    {
                        Logger.Debug("[MechMeleeSequence_GenerateMeleePath_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);

                        // Setting this prevents the footstep effects from Coils to be displayed when a Juggernauts charges
                        new Traverse(___moveSequence).Property("isSprinting").SetValue(true);
                        ___moveSequence.IgnoreEndSmoothing = true;
                        ___moveSequence.meleeType = MeleeAttackType.Charge;

                        Logger.Info("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.isSprinting: " + ___moveSequence.isSprinting);
                        Logger.Info("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.IgnoreEndSmoothing: " + ___moveSequence.IgnoreEndSmoothing);
                        Logger.Info("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.meleeType: " + ___moveSequence.meleeType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(MechMeleeSequence), "BuildMeleeDirectorSequence")]
        public static class MechMeleeSequence_BuildMeleeDirectorSequence_Patch
        {
            static void Prefix(MechMeleeSequence __instance)
            {
                try
                {
                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut() && Fields.JuggernautCharges)
                    {
                        Logger.Debug("[MechMeleeSequence_BuildMeleeDirectorSequence_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);
                        MeleeAttackType selectedMeleeType = (MeleeAttackType)AccessTools.Property(typeof(MechMeleeSequence), "selectedMeleeType").GetValue(__instance, null);
                        Logger.Info("[MechMeleeSequence_BuildMeleeDirectorSequence_PREFIX] BEFORE selectedMeleeType: " + selectedMeleeType);
                        selectedMeleeType = MeleeAttackType.Charge;
                        Logger.Info("[MechMeleeSequence_BuildMeleeDirectorSequence_PREFIX] AFTER selectedMeleeType: " + selectedMeleeType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(MechMeleeSequence), "ExecuteMove")]
        public static class MechMeleeSequence_ExecuteMove_Patch
        {
            static void Prefix(MechMeleeSequence __instance)
            {
                try
                {
                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut() && Fields.JuggernautCharges)
                    {
                        Logger.Debug("[MechMeleeSequence_ExecuteMove_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);

                        // This is to handle instability reduction correctly. If a juggernaut is charging it will be handled as a sprint.
                        // This should also sanitize (that is: disable) the damage multiplier for Coil-S
                        __instance.OwningMech.SprintedLastRound = true;
                        Logger.Debug("[MechMeleeSequence_ExecuteMove_PREFIX] OwningMech.SprintedLastRound: " + __instance.OwningMech.SprintedLastRound);

                        // Push message out
                        AbstractActor actor = __instance.owningActor;
                        actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "CHARGING", FloatieMessage.MessageNature.Buff));
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(MechMeleeSequence), "OnMoveComplete")]
        public static class MechMeleeSequence_OnMoveComplete_Patch
        {
            static void Prefix(MechMeleeSequence __instance)
            {
                try
                {
                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut())
                    {
                        // In some rare occasions DistMovedThisRound is smaller than MaxWalkDistance BUT unit is marked as sprinting via CostLeft from Pathing.
                        // Relying on the mark set at [Pathing_UpdateMeleePath_POSTFIX], DistMovedThisRound is only logged as a reference here.
                        Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] maxWalkDistance: " + __instance.OwningMech.MaxWalkDistance);
                        Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] distMovedThisRound: " + __instance.OwningMech.DistMovedThisRound);
                        Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] SprintedLastRound: " + __instance.owningActor.SprintedLastRound);
                        Logger.Debug("[MechMeleeSequence_ExecuteMove_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);

                        // Juggernauts only gain GUARDED on regular melee attack...
                        if (!__instance.owningActor.SprintedLastRound)
                        {
                            Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut only moved. Apply braced but don't further reduce instability.");
                            __instance.owningActor.BracedLastRound = true;

                            // Include stability reduction only when Mech remained "stationary"
                            if (__instance.OwningMech.DistMovedThisRound < 10f)
                            {
                                Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut did not move at all. Reduce instability.");
                                __instance.OwningMech.ApplyInstabilityReduction(StabilityChangeSource.RemainingStationary);
                            }
                        }
                        // ...but not when charging
                        else
                        {
                            Logger.Debug("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut sprinted. Should not apply instability reduction.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(MechMeleeSequence), "OnMeleeComplete")]
        public static class MechMeleeSequence_OnMeleeComplete_Patch
        {
            static void Postfix(MechMeleeSequence __instance, MessageCenterMessage message)
            {
                try
                {
                    AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
                    if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
                    {
                        Logger.Debug("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Missed! Aborting...");
                        return;
                    }

                    Pilot pilot = __instance.owningActor.GetPilot();
                    if (pilot.IsJuggernaut())
                    {
                        // Get melee target
                        ICombatant MeleeTarget = (ICombatant)AccessTools.Property(typeof(MechMeleeSequence), "MeleeTarget").GetValue(__instance, null);
                        
                        if (MeleeTarget.IsDead || MeleeTarget.IsFlaggedForDeath)
                        {
                            return;
                        }

                        // Reapplying "MeleeHitPushBackPhases" here as it doesn't seem to work anymore when only defined in AbilityDef
                        (MeleeTarget as AbstractActor).ForceUnitOnePhaseDown(__instance.owningActor.GUID, __instance.SequenceGUID, false);

                        Logger.Debug("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);

                        if (Fields.JuggernautCharges)
                        {
                            // IMPORTANT! At this point any stab dmg is already applied to the target, normalized by entrenched or terrain...
                            if (MeleeTarget is Mech TargetMech)
                            {
                                // Remove Entrenched when charging?
                                if (TargetMech.IsEntrenched)
                                {
                                    Logger.Debug("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Removing Entrenched from target");
                                    TargetMech.IsEntrenched = false;
                                    TargetMech.Combat.MessageCenter.PublishMessage(new FloatieMessage(TargetMech.GUID, TargetMech.GUID, "LOST: ENTRENCHED", FloatieMessage.MessageNature.Debuff));
                                }

                                /*
                                // Additional stability damage depending on distance?
                                float additionalStabilityDamage = Utilities.GetAdditionalStabilityDamageFromSprintDistance(__instance.OwningMech, TargetMech, false);

                                // Using the attacker from the sequence is more reliable than __instance.OwningMech?
                                //Mech AttackingMech = attackCompleteMessage.attackSequence.attacker as Mech;
                                //float additionalStabilityDamage = Utilities.GetAdditionalStabilityDamageFromSprintDistance(AttackingMech, TargetMech, false);

                                Logger.Debug("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Apply additional stability damage from distance sprinted: " + additionalStabilityDamage);

                                TargetMech.AddAbsoluteInstability(additionalStabilityDamage, StabilityChangeSource.NotSet, __instance.owningActor.GUID);
                                Logger.Debug("[MechMeleeSequence_OnMeleeComplete_POSTFIX] MeleeTarget.CurrentStability: " + TargetMech.CurrentStability);
                                */
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(MechMeleeSequence), "CompleteOrders")]
        public static class MechMeleeSequence_CompleteOrders_Patch
        {
            static void Postfix(MechMeleeSequence __instance)
            {
                try
                {
                    // Just to be sure
                    Fields.JuggernautCharges = false;
                    Logger.Debug("[MechMeleeSequence_CompleteOrders_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}

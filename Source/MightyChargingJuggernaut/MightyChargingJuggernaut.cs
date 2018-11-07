using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Harmony;
using BattleTech;
using BattleTech.UI;
using System.IO;
using UnityEngine;



namespace MightyChargingJuggernaut
{
    public class MightyChargingJuggernaut
    {
        internal static string ModDirectory;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;
            var harmony = HarmonyInstance.Create("de.ben.MightyChargingJuggernaut");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool IsJuggernaut (Pilot pilot, bool logInfo = false)
        {
            bool isJuggernaut = false;
            if (pilot != null && pilot.PassiveAbilities.Count > 0)
            {
                for (int i = 0; i < pilot.PassiveAbilities.Count; i++)
                {
                    if (pilot.PassiveAbilities[i].Def.Description.Id == "AbilityDefGu8")
                    {
                        isJuggernaut = true;
                        if (logInfo)
                        {
                            Logger.LogLine("[MightyChargingJuggernaut.IsJuggernaut] " + pilot.Name + " is a Juggernaut");
                        } 
                    }
                }
            }
            return isJuggernaut;
        }
    }


    // @ToDo: DFAs from juggernauts can knock down without target beeing unsteady before?
    /*
    [HarmonyPatch(typeof(BattleTech.Mech))]
    [HarmonyPatch("AddInstability")]
    public static class BattleTech_AddInstability_Prefix
    {
        static void Prefix(Mech __instance, float amt, StabilityChangeSource source, string sourceGuid)
        {
            // Check juggernaut 
            Pilot pilot = __instance.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut && amt > 0f)
            {
                float projectedStability = __instance.CurrentStability + amt;
                float maxStability = __instance.MaxStability;
                bool isDFA = source == StabilityChangeSource.DFA;

                if (isDFA && projectedStability > maxStability)
                {
                    __instance.FlagForKnockdown();
                }
            }
        }
    }
    */
    


    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("CompleteOrders")]
    public static class MechMeleeSequence_CompleteOrders_Patch
    {
        static void Postfix(MechMeleeSequence __instance)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                Logger.LogLine("[MechMeleeSequence_CompleteOrders_POSTFIX] Resetting Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());
                Fields.JuggernautCharges = false;
                Logger.LogLine("[MechMeleeSequence_CompleteOrders_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("GenerateMeleePath")]
    public static class MechMeleeSequence_GenerateMeleePath_Patch
    {
        static void Postfix(MechMeleeSequence __instance, ref ActorMovementSequence ___moveSequence)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                Logger.LogLine("[MechMeleeSequence_GenerateMeleePath_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());
                if (Fields.JuggernautCharges)
                {
                    ___moveSequence.IgnoreEndSmoothing = true;
                    ___moveSequence.meleeType = MeleeAttackType.Charge;

                    // @ToDo: Check if this will change anything but animation!
                    /*
                    MovementCapabilitiesDef Capabilities = (MovementCapabilitiesDef)AccessTools.Property(typeof(ActorMovementSequence), "Capabilities").GetValue(___moveSequence, null);
                    float SprintVelocity = Capabilities.SprintVelocity;
                    float SprintAcceleration = Capabilities.SprintAcceleration;

                    MoveType currentMoveType = (MoveType)AccessTools.Field(typeof(ActorMovementSequence), "moveType").GetValue(___moveSequence);
                    currentMoveType = MoveType.Sprinting;
                    Logger.LogLine("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.currentMoveType: " + currentMoveType.ToString());
                    */
                }
                Logger.LogLine("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.IgnoreEndSmoothing: " + ___moveSequence.IgnoreEndSmoothing.ToString());
                Logger.LogLine("[MechMeleeSequence_GenerateMeleePath_POSTFIX] moveSequence.meleeType: " + ___moveSequence.meleeType.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("BuildMeleeDirectorSequence")]
    public static class MechMeleeSequence_BuildMeleeDirectorSequence_Patch
    {
        static void Prefix(MechMeleeSequence __instance)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                Logger.LogLine("[MechMeleeSequence_BuildMeleeDirectorSequence_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());
                if (Fields.JuggernautCharges)
                {
                    MeleeAttackType selectedMeleeType = (MeleeAttackType)AccessTools.Property(typeof(MechMeleeSequence), "selectedMeleeType").GetValue(__instance, null);
                    selectedMeleeType = MeleeAttackType.Charge;
                    Logger.LogLine("[MechMeleeSequence_BuildMeleeDirectorSequence_PREFIX] selectedMeleeType: " + selectedMeleeType.ToString());
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("ExecuteMove")]
    public static class MechMeleeSequence_ExecuteMove_Patch
    {
        static void Prefix(MechMeleeSequence __instance)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                Logger.LogLine("[MechMeleeSequence_ExecuteMove_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());

                if (Fields.JuggernautCharges)
                {
                    // BEN: This is to handle instability reduction correctly. If a juggernaut is charging it will be handled as a sprint.
                    __instance.OwningMech.SprintedLastRound = true;
                    Logger.LogLine("[MechMeleeSequence_ExecuteMove_PREFIX] OwningMech.SprintedLastRound: " + __instance.OwningMech.SprintedLastRound.ToString());

                    // Push message out
                    AbstractActor actor = __instance.owningActor;
                    actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "CHARGING", FloatieMessage.MessageNature.Inspiration));
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("OnMeleeComplete")]
    public static class MechMeleeSequence_OnMeleeComplete_Patch
    {
        static void Postfix(MechMeleeSequence __instance, ref MessageCenterMessage message)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                Logger.LogLine("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());

                if (Fields.JuggernautCharges)
                {
                    // Get melee target and apply additional stability damage
                    ICombatant MeleeTarget = (ICombatant)AccessTools.Property(typeof(MechMeleeSequence), "MeleeTarget").GetValue(__instance, null);

                    //Logger.LogLine("ICombatant MeleeTargets Name: " + MeleeTarget.GetPilot().Name);
                    //AbstractActor TargetAbstractActor = MeleeTarget as AbstractActor;
                    //TargetAbstractActor.FlagForKnockdown();

                    // Instability for attacker on a miss?
                    /*
                    AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
                    if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
                    {
                        __instance.OwningMech.AddAbsoluteInstability(__instance.OwningMech.tonnage / 2, StabilityChangeSource.NotSet, __instance.owningActor.GUID);
                    }
                    */

                    //@ToDo: Test & Finetune
                    if (MeleeTarget is Mech TargetMech)
                    {
                        // Depending on distance?
                        /*
                        float distMovedThisRound = __instance.OwningMech.DistMovedThisRound;
                        float percentMoved = distMovedThisRound / __instance.OwningMech.MaxSprintDistance;
                        float additionalStabilityDamage = __instance.OwningMech.MechDef.Chassis.MeleeInstability * percentMoved;
                        */

                        // Flat percentage
                        float additionalStabilityDamage = __instance.OwningMech.MechDef.Chassis.MeleeInstability / 2;

                        // Knockdown in one turn if stability damage is big enough?
                        /*
                        bool isAlreadyUnsteady = TargetMech.IsUnsteady;
                        float projectedStability = TargetMech.CurrentStability + additionalStabilityDamage;
                        float maxStability = TargetMech.MaxStability;
                        if (!isAlreadyUnsteady && projectedStability > maxStability)
                        {
                            TargetMech.FlagForKnockdown();
                        }
                        */

                        Logger.LogLine("[MechMeleeSequence_OnMeleeComplete_POSTFIX] Apply additional stability damage from charging (50% of OwningMech.MechDef.Chassis.MeleeInstability): " + additionalStabilityDamage);
                        TargetMech.AddAbsoluteInstability(additionalStabilityDamage, StabilityChangeSource.NotSet, __instance.owningActor.GUID);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence))]
    [HarmonyPatch("OnMoveComplete")]
    public static class MechMeleeSequence_OnMoveComplete_Patch
    {
        static void Prefix(MechMeleeSequence __instance)
        {
            // Check juggernaut 
            Pilot pilot = __instance.owningActor.GetPilot();
            bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

            if (pilotIsJuggernaut)
            {
                // BEN: In some rare occasions DistMovedThisRound is smaller than MaxWalkDistance BUT unit is marked as sprinting via CostLeft from Pathing.
                // Relying on the mark set at [Pathing_UpdateMeleePath_POSTFIX], DistMovedThisRound is only logged as a reference here.
                Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] maxWalkDistance: " + __instance.OwningMech.MaxWalkDistance);
                Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] distMovedThisRound: " + __instance.OwningMech.DistMovedThisRound);
                Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] SprintedLastRound: " + __instance.owningActor.SprintedLastRound.ToString());
                Logger.LogLine("[MechMeleeSequence_ExecuteMove_PREFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());

                // BEN: Juggernauts gain GUARDED only on regular melee attack...
                if (!__instance.owningActor.SprintedLastRound)
                {
                    Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut only moved. Apply braced but don't further reduce instability.");
                    __instance.owningActor.BracedLastRound = true;

                    // BEN: Include stability reduction only when Mech remained "stationary" (Taken from class AbstractActor: GuardLevel):
                    // bool flag2 = (this.HasFiredThisRound || !this.HasMovedThisRound) && this.DistMovedThisRound < 10f;
                    // bool flag3 = this.BracedLastRound || (flag2 && this.statCollection.GetValue<bool>("GuardedFromBeingStationary"));
                    
                    if (__instance.OwningMech.DistMovedThisRound < 10f) // This is enough as Juggernauts must have "Bulwark" too
                    {
                        Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut did not move at all. Reduce instability through bracing.");
                        __instance.OwningMech.ApplyInstabilityReduction(StabilityChangeSource.Bracing);
                    }
                }
                // ...not when charging
                else
                {
                    Logger.LogLine("[MechMeleeSequence_OnMoveComplete_PREFIX] Juggernaut sprinted. Should not apply instability reduction.");
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pathing))]
    [HarmonyPatch("GetMeleeDestsForTarget")]
    public static class Pathing_GetMeleeDestsForTarget_Patch
    {
        static bool Prefix(Pathing __instance, ref List<PathNode> __result, ref AbstractActor target)
        {
            try
            {
                // Check juggernaut 
                Pilot pilot = __instance.OwningActor.GetPilot();
                bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot);

                // Check target (Only Mechs can be charged(?))
                //Logger.LogLine("[Pathing_GetMeleeDestsForTarget_PREFIX] target.UnitType: " + target.UnitType.ToString());
                bool targetIsMech = target.UnitType == UnitType.Mech;

                if (!pilotIsJuggernaut || !targetIsMech)
                {
                    // Call original method
                    return true;
                }
                else
                {
                    // Rewrite method and return false

                    // Get private properties
                    CombatGameState Combat = (CombatGameState)AccessTools.Property(typeof(Pathing), "Combat").GetValue(__instance, null);
                    PathNodeGrid SprintingGrid = (PathNodeGrid)AccessTools.Property(typeof(Pathing), "SprintingGrid").GetValue(__instance, null);
                    PathNodeGrid MeleeGrid = (PathNodeGrid)AccessTools.Property(typeof(Pathing), "MeleeGrid").GetValue(__instance, null);

                    // Modify method
                    if (__instance.OwningActor.VisibilityToTargetUnit(target) < VisibilityLevel.LOSFull)
                    {
                        __result = new List<PathNode>();
                        return false;
                    }

                    List<Vector3> adjacentPointsOnGrid = Combat.HexGrid.GetAdjacentPointsOnGrid(target.CurrentPosition);
                    List<PathNode> pathNodesForPoints = Pathing.GetPathNodesForPoints(adjacentPointsOnGrid, SprintingGrid);

                    // Need to check if unit actually could reach the target. If not, fall back to MeleeGrid
                    bool CanSprint = __instance.OwningActor.CanSprint; // && !__instance.OwningActor.StoodUpThisRound;
                    if (!CanSprint)
                    {
                        //Logger.LogLine("[Pathing_GetMeleeDestsForTarget_PREFIX] " + currentPilot.Name + " cannot sprint right now.");
                        pathNodesForPoints = Pathing.GetPathNodesForPoints(adjacentPointsOnGrid, MeleeGrid);
                    }

                    for (int i = pathNodesForPoints.Count - 1; i >= 0; i--)
                    {
                        if (Mathf.Abs(pathNodesForPoints[i].Position.y - target.CurrentPosition.y) > Combat.Constants.MoveConstants.MaxMeleeVerticalOffset || SprintingGrid.FindBlockerReciprocal(pathNodesForPoints[i].Position, target.CurrentPosition))
                        {
                            pathNodesForPoints.RemoveAt(i);
                        }
                    }

                    if (pathNodesForPoints.Count > 1)
                    {
                        if (Combat.Constants.MoveConstants.SortMeleeHexesByPathingCost)
                        {
                            pathNodesForPoints.Sort((PathNode a, PathNode b) => a.CostToThisNode.CompareTo(b.CostToThisNode));
                        }
                        else
                        {
                            pathNodesForPoints.Sort((PathNode a, PathNode b) => Vector3.Distance(a.Position, __instance.OwningActor.CurrentPosition).CompareTo(Vector3.Distance(b.Position, __instance.OwningActor.CurrentPosition)));
                        }
                        int num = Combat.Constants.MoveConstants.NumMeleeDestinationChoices;
                        Vector3 vector = __instance.OwningActor.CurrentPosition - pathNodesForPoints[0].Position;
                        vector.y = 0f;

                        // Open up more melee positions when ignoring this condition. Essentially re-applying the change that Morphyums "MeleeMover" does in its transpiler. 
                        /*
                        if (vector.magnitude < 10f)
                        {
                            num = 1;
                        }
                        */
                        // :NEB

                        while (pathNodesForPoints.Count > num)
                        {
                            pathNodesForPoints.RemoveAt(pathNodesForPoints.Count - 1);
                        }
                    }
                    __result = pathNodesForPoints;
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return true;
            }
        }
    }

    /*
    [HarmonyPatch(typeof(Pathing))]
    [HarmonyPatch("SetMeleeTarget")]
    public static class Pathing_SetMeleeTarget_Patch
    {
        public static void Prefix(Pathing __instance, AbstractActor target)
        {
            try
            {
                Logger.LogLine("[Pathing_SetMeleeTarget_PREFIX] Called");
                Logger.LogLine("[Pathing_SetMeleeTarget_PREFIX] Will build CurrentPath from MeleeGrid and with OwningActor.MaxMeleeEngageRangeDistance");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        public static void Postfix(Pathing __instance, AbstractActor target)
        {
            try
            {
                Logger.LogLine("[Pathing_SetMeleeTarget_POSTFIX] Called");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    */

    [HarmonyPatch(typeof(Pathing))]
    [HarmonyPatch("UpdateMeleePath")]
    public static class Pathing_UpdateMeleePath_Patch
    {
        /*
        public static void Prefix(Pathing __instance)
        {
            try
            {
                Logger.LogLine("[Pathing_UpdateMeleePath_PREFIX] Called");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        */

        public static void Postfix(Pathing __instance)
        {
            try
            {
                // Check juggernaut 
                Pilot pilot = __instance.OwningActor.GetPilot();
                bool pilotIsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot, true);

                if (pilotIsJuggernaut)
                {
                    /**
                     * BEN: MaxCost is essentially the maximal distance the unit could move within current grid.
                     * As i dont actually set the unit to sprinting or change the grid this method is working on
                     * __instance.CostLeft is turning negative when a melee target outside the current grid is pathed to.
                     * This is used to determine if a juggernaut needs to sprint to reach its target.
                     * Selection of target is made possible in [Pathing_GetMeleeDestsForTarget_PREFIX]
                    **/
                    Logger.LogLine("[Pathing_UpdateMeleePath_POSTFIX] Pathing MaxCost: " + __instance.MaxCost.ToString());
                    Logger.LogLine("[Pathing_UpdateMeleePath_POSTFIX] Pathing CostLeft: " + __instance.CostLeft.ToString());
                    Logger.LogLine("[Pathing_UpdateMeleePath_POSTFIX] Units MaxWalkDistance: " + __instance.OwningActor.MaxWalkDistance);
                    Logger.LogLine("[Pathing_UpdateMeleePath_POSTFIX] Units MaxSprintDistance: " + __instance.OwningActor.MaxSprintDistance);

                    if (__instance.CostLeft < 0)
                    {
                        Fields.JuggernautCharges = true;
                    }
                    else
                    {
                        Fields.JuggernautCharges = false;
                    }
                    Logger.LogLine("[Pathing_UpdateMeleePath_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges.ToString());
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }



    /*
    [HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    public static class CombatSelectionHandler_TrySelectActor_Patch
    {
        public static void Postfix(CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection)
        {
            try
            {
                Pilot pilot = actor.GetPilot();
                Fields.IsJuggernaut = MightyChargingJuggernaut.IsJuggernaut(pilot); 
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    */
}


using System;
using System.Collections.Generic;
using Harmony;
using BattleTech;
using UnityEngine;
using MightyChargingJuggernaut.Extensions;

namespace MightyChargingJuggernaut.Patches
{
    class Paths
    {
        [HarmonyPatch(typeof(Pathing))]
        [HarmonyPatch("GetMeleeDestsForTarget")]
        public static class Pathing_GetMeleeDestsForTarget_Patch
        {
            static bool Prefix(Pathing __instance, ref List<PathNode> __result, AbstractActor target)
            {
                try
                {
                    Pilot pilot = __instance.OwningActor.GetPilot();
                    
                    // ToDo: Cannot charge prone targets?
                    // Only Mechs can be charged
                    bool targetIsMech = target.UnitType == UnitType.Mech;

                    if (!pilot.IsJuggernaut() || !targetIsMech)
                    {
                        // Call original method
                        return true;
                    }
                    else
                    {
                        // Rewrite method and return false
                        CombatGameState Combat = (CombatGameState)AccessTools.Property(typeof(Pathing), "Combat").GetValue(__instance, null);
                        PathNodeGrid SprintingGrid = (PathNodeGrid)AccessTools.Property(typeof(Pathing), "SprintingGrid").GetValue(__instance, null);
                        PathNodeGrid MeleeGrid = (PathNodeGrid)AccessTools.Property(typeof(Pathing), "MeleeGrid").GetValue(__instance, null);

                        VisibilityLevel visibilityLevel = __instance.OwningActor.VisibilityToTargetUnit(target);
                        if (visibilityLevel < VisibilityLevel.LOSFull && visibilityLevel != VisibilityLevel.BlipGhost)
                        {
                            __result = new List<PathNode>();
                            return false;
                        }

                        List<Vector3> adjacentPointsOnGrid = Combat.HexGrid.GetAdjacentPointsOnGrid(target.CurrentPosition);

                        // Default to SprintingGrid
                        List<PathNode> pathNodesForPoints = Pathing.GetPathNodesForPoints(adjacentPointsOnGrid, SprintingGrid);

                        // Check if unit actually can sprint to reach the target. If not, fall back to MeleeGrid
                        bool CanCharge = __instance.OwningActor.CanSprint && !__instance.OwningActor.StoodUpThisRound;
                        if (!CanCharge)
                        {
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
                            //if (vector.magnitude < 10f)
                            //{
                            //    num = 1;
                            //}

                            while (pathNodesForPoints.Count > num)
                            {
                                pathNodesForPoints.RemoveAt(pathNodesForPoints.Count - 1);
                            }
                        }
                        __result = pathNodesForPoints;

                        // Skip original method
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return true;
                }
            }
        }



        [HarmonyPatch(typeof(Pathing))]
        [HarmonyPatch("Update")]
        public static class Pathing_Update_Patch
        {
            public static void Postfix(Pathing __instance)
            {
                try
                {
                    if (__instance.CurrentMeleeTarget != null)
                    {
                        Pilot pilot = __instance.OwningActor.GetPilot();
                        if (pilot.IsJuggernaut())
                        {
                            /**
                             * BEN: MaxCost is essentially the maximal distance the unit could move within current grid.
                             * As i dont actually set the unit to sprinting or change the grid this method is working on
                             * __instance.CostLeft is turning negative when a melee target outside the current grid is pathed to.
                             * This is used to determine if a juggernaut needs to sprint to reach its target.
                             * Selection of target is made possible in [Pathing_GetMeleeDestsForTarget_PREFIX]
                            **/
                            Logger.Debug("[Pathing_UpdateMeleePath_POSTFIX] Pathing MaxCost: " + __instance.MaxCost.ToString());
                            Logger.Debug("[Pathing_UpdateMeleePath_POSTFIX] Pathing CostLeft: " + __instance.CostLeft.ToString());
                            Logger.Debug("[Pathing_UpdateMeleePath_POSTFIX] Units MaxWalkDistance: " + __instance.OwningActor.MaxWalkDistance);
                            Logger.Debug("[Pathing_UpdateMeleePath_POSTFIX] Units MaxSprintDistance: " + __instance.OwningActor.MaxSprintDistance);

                            if (__instance.CostLeft < 0)
                            {
                                Fields.JuggernautCharges = true;
                            }
                            else
                            {
                                Fields.JuggernautCharges = false;
                            }
                            //Logger.Debug("[Pathing_Update_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);
                        }
                    }
                    else
                    {
                        Fields.JuggernautCharges = false;
                    }
                    //Logger.Debug("[Pathing_Update_POSTFIX] Fields.JuggernautCharges: " + Fields.JuggernautCharges);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}

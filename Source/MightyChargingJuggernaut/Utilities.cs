using BattleTech;
using UnityEngine;

namespace MightyChargingJuggernaut
{
    class Utilities
    {
        public static float GetAdditionalStabilityDamageFromSprintDistance(Mech attackingMech, Mech targetMech, bool ignoreModifiers = false)
        {
            float result = 0;

            float receivedInstabilityMultiplier = targetMech.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier");
            float entrenchedMultiplier = (targetMech as AbstractActor).EntrenchedMultiplier;
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] targetMech.ReceivedInstabilityMultiplier: " + receivedInstabilityMultiplier);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] targetMech.EntrenchedMultiplier: " + entrenchedMultiplier);

            float distanceSprinted = attackingMech.DistMovedThisRound;
            float percentSprinted = distanceSprinted / attackingMech.MaxSprintDistance;
            float finalMultiplier = Mathf.Clamp((percentSprinted - 0.35f), 0.1f, 0.5f);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] distanceSprinted: " + distanceSprinted);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] percentSprinted: " + percentSprinted);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] finalMultiplier: " + finalMultiplier);

            result = attackingMech.MechDef.Chassis.MeleeInstability * finalMultiplier;

            if (ignoreModifiers)
            {
                Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] additionalStabilityDamage(ignored entrenched & terrain modifiers): " + result);
                return result;
            }
            else
            {
                result *= receivedInstabilityMultiplier;
                result *= entrenchedMultiplier;

                Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromSprintDistance] additionalStabilityDamage(applied entrenched & terrain modifiers): " + result);
                return result;
            }
        }



        public static float GetAdditionalStabilityDamageFromJumpDistance(Mech attackingMech, Mech targetMech, bool ignoreModifiers = false)
        {
            float result = 0;

            float receivedInstabilityMultiplier = targetMech.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier");
            float entrenchedMultiplier = (targetMech as AbstractActor).EntrenchedMultiplier;
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] targetMech.ReceivedInstabilityMultiplier: " + receivedInstabilityMultiplier);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] targetMech.EntrenchedMultiplier: " + entrenchedMultiplier);

            float distanceJumped = attackingMech.DistMovedThisRound;
            int installedJumpjets = attackingMech.jumpjets.Count;
            float maxJumpDistance;

            // Borrowed from Mech.JumpDistance
            if (installedJumpjets >= attackingMech.Combat.Constants.MoveConstants.MoveTable.Length)
            {
                maxJumpDistance = attackingMech.Combat.Constants.MoveConstants.MoveTable[attackingMech.Combat.Constants.MoveConstants.MoveTable.Length - 1] * attackingMech.StatCollection.GetValue<float>("JumpDistanceMultiplier");
            }
            else
            {
                maxJumpDistance = attackingMech.Combat.Constants.MoveConstants.MoveTable[installedJumpjets] * attackingMech.StatCollection.GetValue<float>("JumpDistanceMultiplier");
            }

            float percentJumped = distanceJumped / maxJumpDistance;
            float finalMultiplier = Mathf.Clamp((percentJumped - 0.35f), 0.1f, 0.5f);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] distanceJumped: " + distanceJumped);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] maxJumpDistance: " + maxJumpDistance);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] percentJumped: " + percentJumped);
            Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] finalMultiplier: " + finalMultiplier);

            result = attackingMech.MechDef.Chassis.DFAInstability * finalMultiplier;

            if (ignoreModifiers)
            {
                Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] additionalStabilityDamage(ignored entrenched & terrain modifiers): " + result);
                return result;
            }
            else
            {
                result *= receivedInstabilityMultiplier;
                result *= entrenchedMultiplier;

                Logger.Debug("[Utilities_GetAdditionalStabilityDamageFromJumpDistance] additionalStabilityDamage(applied entrenched & terrain modifiers): " + result);
                return result;
            }
        }
    }
}

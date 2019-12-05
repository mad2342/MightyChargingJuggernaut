using BattleTech;

namespace MightyChargingJuggernaut
{
    class Utilities
    {
        public static float GetAdditionalStabilityDamageFromJumpDistance(Mech attackingMech, Mech targetMech, bool ignoreModifiers = false)
        {
            float result = 0;

            float receivedInstabilityMultiplier = targetMech.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier");
            float entrenchedMultiplier = (targetMech as AbstractActor).EntrenchedMultiplier;
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] targetMech.ReceivedInstabilityMultiplier: " + receivedInstabilityMultiplier);
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] targetMech.EntrenchedMultiplier: " + entrenchedMultiplier);

            float distanceJumped = attackingMech.DistMovedThisRound;
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] distanceJumped: " + distanceJumped);

            int workingJumpjets = attackingMech.WorkingJumpjets;
            float maxJumpDistance;
            if (workingJumpjets >= attackingMech.Combat.Constants.MoveConstants.MoveTable.Length)
            {
                maxJumpDistance = attackingMech.Combat.Constants.MoveConstants.MoveTable[attackingMech.Combat.Constants.MoveConstants.MoveTable.Length - 1] * attackingMech.StatCollection.GetValue<float>("JumpDistanceMultiplier");
            }
            else
            {
                maxJumpDistance = attackingMech.Combat.Constants.MoveConstants.MoveTable[workingJumpjets] * attackingMech.StatCollection.GetValue<float>("JumpDistanceMultiplier");
            }
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] maxJumpDistance: " + maxJumpDistance);

            float percentJumped = distanceJumped / maxJumpDistance;
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] percentJumped: " + percentJumped);

            result = attackingMech.MechDef.Chassis.DFAInstability * percentJumped;

            if (ignoreModifiers)
            {
                Logger.LogLine("[Utilities.GetAdditionalStabilityDamageFromJumpDistance] additionalStabilityDamage(ignored entrenched & terrain modifiers): " + result);
                return result;
            }
            else
            {
                result *= receivedInstabilityMultiplier;
                result *= entrenchedMultiplier;

                Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] additionalStabilityDamage(applied entrenched & terrain modifiers): " + result);
                return result;
            }
        }
    }
}

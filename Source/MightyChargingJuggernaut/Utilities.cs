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
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] targetMech.ReceivedInstabilityMultiplier: " + receivedInstabilityMultiplier);
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] targetMech.EntrenchedMultiplier: " + entrenchedMultiplier);

            float distanceJumped = attackingMech.DistMovedThisRound;
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] distanceJumped: " + distanceJumped);

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
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] maxJumpDistance: " + maxJumpDistance);

            float percentJumped = distanceJumped / maxJumpDistance;
            Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] percentJumped: " + percentJumped);

            result = attackingMech.MechDef.Chassis.DFAInstability * percentJumped;

            if (ignoreModifiers)
            {
                Logger.LogLine("[Utilities.GetAdditionalStabilityDamage] additionalStabilityDamage(ignored entrenched & terrain modifiers): " + result);
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

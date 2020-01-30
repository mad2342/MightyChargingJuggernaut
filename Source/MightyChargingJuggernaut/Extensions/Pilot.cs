using BattleTech;

namespace MightyChargingJuggernaut.Extensions
{
    public static class PilotExtensions
    {
        public static bool IsJuggernaut(this Pilot pilot)
        {
            return pilot.PassiveAbilities.Find((Ability a) => a.Def.Description.Id == "AbilityDefGu8") != null;

            /*
            bool isJuggernaut = false;
            if (pilot != null && pilot.PassiveAbilities.Count > 0)
            {
                for (int i = 0; i < pilot.PassiveAbilities.Count; i++)
                {
                    if (pilot.PassiveAbilities[i].Def.Description.Id == "AbilityDefGu8")
                    {
                        isJuggernaut = true;
                        Logger.Debug("[PilotExtensions_IsJuggernaut] " + pilot.Name + " is a Juggernaut");
                    }
                }
            }
            return isJuggernaut;
            */
        }
    }
}

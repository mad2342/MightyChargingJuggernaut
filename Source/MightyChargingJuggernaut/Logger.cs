using System;
using System.IO;

namespace MightyChargingJuggernaut
{
    public class Logger
    {
        static string filePath = $"{MightyChargingJuggernaut.ModDirectory}/MightyChargingJuggernaut.log";
        public static void LogError(Exception ex)
        {
            if (MightyChargingJuggernaut.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MightyChargingJuggernaut @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line)
        {
            if (MightyChargingJuggernaut.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MightyChargingJuggernaut @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine(prefix + line);
                }
            }
        }
    }
}

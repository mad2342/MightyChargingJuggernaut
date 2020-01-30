using System.Reflection;
using Harmony;
using System.IO;

namespace MightyChargingJuggernaut
{
    public class MightyChargingJuggernaut
    {
        internal static string LogPath;
        internal static string ModDirectory;

        // BEN: DebugLevel (0: nothing, 1: error, 2: debug, 3: info)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settings)
        {
            ModDirectory = directory;
            LogPath = Path.Combine(ModDirectory, "MightyChargingJuggernaut.log");

            Logger.Initialize(LogPath, DebugLevel, ModDirectory, nameof(MightyChargingJuggernaut));

            // Harmony calls need to go last here because their Prepare() methods directly check Settings...
            HarmonyInstance harmony = HarmonyInstance.Create("de.mad.MightyChargingJuggernaut");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}


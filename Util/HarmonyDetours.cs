using Harmony;

namespace CSURToolBox.Util
{
    public class HarmonyDetours
    {
        public const string Id = "csur.toolbox";
        public static void Apply()
        {
            Harmony.Harmony.DEBUG = true;
            var harmony = new Harmony.Harmony(Id);
            harmony.PatchAll(typeof(HarmonyDetours).Assembly);
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            var harmony = new Harmony.Harmony(Id);
            harmony.UnpatchAll(Id);
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}

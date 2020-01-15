using Harmony;
using System.Reflection;
using System;
using UnityEngine;

namespace CSURToolBox.Util
{
    public static class HarmonyDetours
    {
        private static HarmonyInstance harmony = null;

        public static void Apply()
        {
            harmony = HarmonyInstance.Create("CSURToolBox");
            HarmonyInstance.SELF_PATCHING = false;
            harmony.PatchAll();
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            harmony.UnpatchAll("CSURToolBox");
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
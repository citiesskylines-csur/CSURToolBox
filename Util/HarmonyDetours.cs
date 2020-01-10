using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using CSURToolBox.CustomAI;

namespace CSURToolBox.Util
{
    public static class HarmonyDetours
    {
        private static HarmonyInstance harmony = null;
        private static void ConditionalPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (harmony.GetPatchInfo(method)?.Owners?.Contains(harmony.Id) == true)
            {
                DebugLog.LogToFileOnly("Harmony patches already present for {0}" + fullMethodName.ToString());
            }
            else
            {
                DebugLog.LogToFileOnly("Patching {0}..." + fullMethodName.ToString());
                harmony.Patch(method, prefix, postfix);
            }
        }

        private static void ConditionalUnPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (prefix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Prefix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Prefix);
            }
            if (postfix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Postfix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Postfix);
            }
        }

        public static void Apply()
        {
            harmony = HarmonyInstance.Create("CSURToolBox");
            var roadBaseAIUpdateLanes = typeof(RoadBaseAI).GetMethod("UpdateLanes", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new Type[3] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null);
            var roadBaseAIUpdateLanesPostFix = typeof(CustomNetAI).GetMethod("RoadBaseAIUpdateLanesPostFix");
            harmony.ConditionalPatch(roadBaseAIUpdateLanes,
                null,
                new HarmonyMethod(roadBaseAIUpdateLanesPostFix));
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            //1
            var roadBaseAIUpdateLanes = typeof(RoadBaseAI).GetMethod("UpdateLanes", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new Type[3] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null);
            var roadBaseAIUpdateLanesPostFix = typeof(CustomNetAI).GetMethod("RoadBaseAIUpdateLanesPostFix");
            harmony.ConditionalUnPatch(roadBaseAIUpdateLanes,
                null,
                new HarmonyMethod(roadBaseAIUpdateLanesPostFix));
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}

using ColossalFramework;
using CSURToolBox.UI;
using CSURToolBox.Util;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class BuildingFindParentNodePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(Building).GetMethod("FindParentNode", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort) }, null);
        }
        public static void Prefix(ref Building __instance)
        {
            if (__instance.m_position.x > 8655)
            {
                __instance.m_position.x = 8655;
            }
            if (__instance.m_position.z > 8655)
            {
                __instance.m_position.z = 8655;
            }

            if (__instance.m_position.x < -8719)
            {
                __instance.m_position.x = -8719;
            }
            if (__instance.m_position.z < -8719)
            {
                __instance.m_position.z = -8719;
            }
        }
    }
}

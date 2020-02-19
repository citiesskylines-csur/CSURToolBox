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
    public static class NetAIGetCollisionHalfWidthPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetAI).GetMethod("GetCollisionHalfWidth", BindingFlags.Public | BindingFlags.Instance);
        }
        public static bool Prefix(ref NetAI __instance, ref float __result)
        {
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(__instance.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(__instance.m_info))
            {
                if (!IsCSURSLane)
                {
                    __result =(__instance.m_info.m_halfWidth - __instance.m_info.m_pavementWidth) / 2f;
                }
                else
                {
                    float laneNum = CSURUtil.CountCSURSVehicleLanes(__instance.m_info) + CSURUtil.CountCSURSOtherLanes(__instance.m_info);
                    __result =(laneNum * 3.75f / 2f);
                }
                return false;
            }
            return true;
        }
    }
}

using ColossalFramework;
using CSURToolBox.UI;
using CSURToolBox.Util;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSURToolBox.Patch
{
    [HarmonyPatch(typeof(RoadAI), "CreateZoneBlocks")]
    public static class RoadAICreateZoneBlocksPatch
    {
        public static bool[] segmentHalfWidthLock = new bool[65536];
        public static float[] segmentHalfWidth = new float[65536];
        public static void Prefix(ushort segment)
        {
            NetManager instance = Singleton<NetManager>.instance;
            var __instance = instance.m_segments.m_buffer[segment];
            if (OptionUI.alignZone)
            {
                if (CSURUtil.IsCSUR(__instance.Info))
                {
                    if (!segmentHalfWidthLock[__instance.m_infoIndex])
                    {
                        segmentHalfWidth[__instance.m_infoIndex] = __instance.Info.m_halfWidth;
                        if (__instance.Info.m_halfWidth < 9f)
                        {
                            __instance.Info.m_halfWidth = 8f;
                        }
                        else if (__instance.Info.m_halfWidth < 17f)
                        {
                            __instance.Info.m_halfWidth = 16f;
                        }
                        else if (__instance.Info.m_halfWidth < 25f)
                        {
                            __instance.Info.m_halfWidth = 24f;
                        }
                        else if (__instance.Info.m_halfWidth < 33f)
                        {
                            __instance.Info.m_halfWidth = 32f;
                        }
                        else if (__instance.Info.m_halfWidth < 41f)
                        {
                            __instance.Info.m_halfWidth = 40f;
                        }
                        segmentHalfWidthLock[__instance.m_infoIndex] = true;
                    }
                }
            }
        }

        public static void Postfix(ushort segment)
        {
            NetManager instance = Singleton<NetManager>.instance;
            var __instance = instance.m_segments.m_buffer[segment];
            if (OptionUI.alignZone)
            {
                if (CSURUtil.IsCSUR(__instance.Info))
                { 
                    if (segmentHalfWidthLock[__instance.m_infoIndex])
                    {
                        __instance.Info.m_halfWidth = segmentHalfWidth[__instance.m_infoIndex];
                        segmentHalfWidthLock[__instance.m_infoIndex] = false;
                    }
                }
            }
        }
    }
}

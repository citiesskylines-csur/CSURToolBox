using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System.Reflection;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class RoadAICreateZoneBlocksPatch
    {
        public static bool[] segmentHalfWidthLock = new bool[65536];
        public static float[] segmentHalfWidth = new float[65536];
        public static MethodBase TargetMethod()
        {
            return typeof(RoadAI).GetMethod("CreateZoneBlocks");
        }
        public static void Prefix(ref NetSegment data)
        {
            if (OptionUI.alignZone)
            {
                if (CSURUtil.IsCSUR(data.Info))
                {
                    if (!segmentHalfWidthLock[data.m_infoIndex])
                    {
                        segmentHalfWidth[data.m_infoIndex] = data.Info.m_halfWidth;
                        if (data.Info.m_halfWidth < 9f)
                        {
                            data.Info.m_halfWidth = 8f;
                        }
                        else if (data.Info.m_halfWidth < 17f)
                        {
                            data.Info.m_halfWidth = 16f;
                        }
                        else if (data.Info.m_halfWidth < 25f)
                        {
                            data.Info.m_halfWidth = 24f;
                        }
                        else if (data.Info.m_halfWidth < 33f)
                        {
                            data.Info.m_halfWidth = 32f;
                        }
                        else if (data.Info.m_halfWidth < 41f)
                        {
                            data.Info.m_halfWidth = 40f;
                        }
                        segmentHalfWidthLock[data.m_infoIndex] = true;
                    }
                }
            }
        }

        public static void Postfix(ref NetSegment data)
        {
            if (OptionUI.alignZone)
            {
                if (CSURUtil.IsCSUR(data.Info))
                { 
                    if (segmentHalfWidthLock[data.m_infoIndex])
                    {
                        data.Info.m_halfWidth = segmentHalfWidth[data.m_infoIndex];
                        segmentHalfWidthLock[data.m_infoIndex] = false;
                    }
                }
            }
        }
    }
}

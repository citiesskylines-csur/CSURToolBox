using ColossalFramework;
using System;
using System.Reflection;
using UnityEngine;
using CSURToolBox.Util;
using HarmonyLib;
using CSURToolBox.UI;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentCalculateCornerPatch
    {
        public static bool[] segmentOffsetLock = new bool[65536];
        public static float[] segmentOffset = new float[65536];
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("CalculateCorner", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(bool), typeof(bool), typeof(bool), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(bool).MakeByRefType() }, null);
        }
        public static void Prefix(ref NetSegment __instance, bool start)
        {
            if (OptionUI.fixLargeJunction)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num = (!start) ? __instance.m_endNode : __instance.m_startNode;
                // NON-STOCK CODE STARTS
                float m_minCornerOffset = 0f;
                float tempMinCornerOffset = 1000f;
                int segmentCount = 0;
                bool isCSURRoad = false;
                for (int i = 0; i < 8; i++)
                {
                    ushort segment1 = instance.m_nodes.m_buffer[num].GetSegment(i);
                    if (segment1 != 0)
                    {
                        segmentCount++;
                        if (CSURUtil.IsCSUR(Singleton<NetManager>.instance.m_segments.m_buffer[segment1].Info))
                        {
                            isCSURRoad = true;
                        }
                        if (Singleton<NetManager>.instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset < tempMinCornerOffset)
                        {
                            tempMinCornerOffset = Singleton<NetManager>.instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset;
                        }
                    }
                }

                if (isCSURRoad)
                {
                    if (tempMinCornerOffset != 1000f)
                    {
                        m_minCornerOffset = tempMinCornerOffset;
                    }
                    //direct node
                    if (segmentCount == 2)
                    {
                        m_minCornerOffset = m_minCornerOffset / 2f;
                        if (m_minCornerOffset > 24f)
                        {
                            m_minCornerOffset = 24f;
                        }
                    }

                    if (!segmentOffsetLock[__instance.m_infoIndex])
                    {
                        segmentOffset[__instance.m_infoIndex] = __instance.Info.m_minCornerOffset;
                        segmentOffsetLock[__instance.m_infoIndex] = true;
                        __instance.Info.m_minCornerOffset = m_minCornerOffset;
                    }
                }
            }
        }

        public static void Postfix(ref NetSegment __instance)
        {
            if (OptionUI.fixLargeJunction)
            {
                if (segmentOffsetLock[__instance.m_infoIndex])
                {
                    __instance.Info.m_minCornerOffset = segmentOffset[__instance.m_infoIndex];
                    segmentOffsetLock[__instance.m_infoIndex] = false;
                }
            }
        }
    }
}

using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using CSURToolBox.Util;
using Harmony;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentCalculateCornerPatch
    {
        public static bool[] segmentOffsetLock = new bool[36864];
        public static float[] segmentOffset = new float[36864];
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("CalculateCorner", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(bool), typeof(bool), typeof(bool), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(bool).MakeByRefType() }, null);
        }
        public static void Prefix(ref NetSegment __instance, ushort segmentID, bool start)
        {
            NetInfo info = __instance.Info;
            NetManager instance = Singleton<NetManager>.instance;
            ushort num = (!start) ? __instance.m_endNode : __instance.m_startNode;
            ushort num2 = (!start) ? __instance.m_startNode : __instance.m_endNode;
            Vector3 position = instance.m_nodes.m_buffer[(int)num].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[(int)num2].m_position;
            Vector3 startDir = (!start) ? __instance.m_endDirection : __instance.m_startDirection;
            Vector3 endDir = (!start) ? __instance.m_startDirection : __instance.m_endDirection;
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
                    m_minCornerOffset = m_minCornerOffset * 2f / 3f;
                    if (m_minCornerOffset > 28f)
                    {
                        m_minCornerOffset = 28f;
                    }
                }               
            }

            //DebugLog.LogToFileOnly("Pre m_minCornerOffset = " + m_minCornerOffset.ToString());
            if (!segmentOffsetLock[segmentID])
            {
                segmentOffset[segmentID] = __instance.Info.m_minCornerOffset;
                segmentOffsetLock[segmentID] = true;
                __instance.Info.m_minCornerOffset = m_minCornerOffset;
            }
            //DebugLog.LogToFileOnly("Pre m_minCornerOffset = " + __instance.Info.m_minCornerOffset.ToString());
        }

        public static void Postfix(ref NetSegment __instance, ushort segmentID)
        {
            if (segmentOffsetLock[segmentID])
            {
                __instance.Info.m_minCornerOffset = segmentOffset[segmentID];
                segmentOffsetLock[segmentID] = false;
            }
            //DebugLog.LogToFileOnly("Post m_minCornerOffset = " + __instance.Info.m_minCornerOffset.ToString());
        }
    }
}

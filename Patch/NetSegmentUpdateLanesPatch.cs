using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
using CSURToolBox.Util;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentUpdateLanesPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("UpdateLanes");
        }
        public static void Postfix(ref NetSegment __instance)
        {
            if (__instance.m_flags != NetSegment.Flags.None)
            {
                var m_info = __instance.Info;
                if (m_info == null)
                {
                    return;
                }

                if (__instance.m_lanes != 0u || (m_info.m_lanes != null && m_info.m_lanes.Length != 0))
                {
                    //Patch Begin
                    NetManager instance = Singleton<NetManager>.instance;
                    uint firstLane = __instance.m_lanes;
                    float num = 0f;
                    float num2 = 0f;
                    if ((m_info.m_netAI is RoadAI) || (m_info.m_netAI is RoadBridgeAI) || (m_info.m_netAI is RoadTunnelAI))
                    {
                        if (CSURUtil.IsCSURLaneOffset(m_info))
                        {
                            for (int i = 0; i < m_info.m_lanes.Length; i++)
                            {
                                if (firstLane == 0)
                                {
                                    break;
                                }
                                float laneOffset = 0;
                                NetInfo.Lane lane = m_info.m_lanes[i];
                                float laneOffsetUnit = CSURUtil.CSURLaneOffset(m_info, lane);
                                laneOffset = laneOffsetUnit * 3.75f;
                                //DebugLog.LogToFileOnly("lanepostion = " + lane.m_position.ToString() + " laneoffset = " + laneOffset.ToString());
                                float effort = (OptionUI.smoothLevel == 2) ? 0.002f : (OptionUI.smoothLevel == 1) ? 0.01f : 0.05f;
                                //EG: before patch: point1-point4 is 1.5*3.75
                                //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                                var bezier = instance.m_lanes.m_buffer[firstLane].m_bezier;
                                Vector3 newBezierA = bezier.Position(0) + (new Vector3(-bezier.Tangent(0).z, 0, bezier.Tangent(0).x).normalized) * (laneOffset * 0.5f);
                                Vector3 newBezierA1 = bezier.Position(effort) + (new Vector3(-bezier.Tangent(effort).z, 0, bezier.Tangent(effort).x).normalized) * (laneOffset * (0.5f - effort));
                                Vector3 newBezierADir = VectorUtils.NormalizeXZ(newBezierA1 - newBezierA);
                                Vector3 newBezierD = bezier.Position(1) + (new Vector3(bezier.Tangent(1).z, 0, -bezier.Tangent(1).x).normalized) * (laneOffset * 0.5f);
                                Vector3 newBezierD1 = bezier.Position(1f - effort) + (new Vector3(bezier.Tangent(1f - effort).z, 0, -bezier.Tangent(1f - effort).x).normalized) * (laneOffset * (0.5f - effort));
                                Vector3 newBezierDDir = VectorUtils.NormalizeXZ(newBezierD1 - newBezierD);

                                Bezier3 newBezier = default(Bezier3);
                                newBezier.a = newBezierA;
                                newBezier.d = newBezierD;

                                //Try to get smooth bezier as close as real roadmesh
                                NetSegment.CalculateMiddlePoints(newBezierA, newBezierADir, newBezierD, newBezierDDir, true, true, out newBezier.b, out newBezier.c);

                                instance.m_lanes.m_buffer[firstLane].m_bezier = newBezier;
                                num += instance.m_lanes.m_buffer[firstLane].UpdateLength();
                                num2 += 1f;
                                firstLane = instance.m_lanes.m_buffer[firstLane].m_nextLane;
                            }

                            if (num2 != 0f)
                            {
                                __instance.m_averageLength = num / num2;
                            }
                            else
                            {
                                __instance.m_averageLength = 0f;
                            }
                            bool flag7 = false;
                            if (__instance.m_averageLength < 11f && (instance.m_nodes.m_buffer[__instance.m_startNode].m_flags & NetNode.Flags.Junction) != 0 && (instance.m_nodes.m_buffer[__instance.m_endNode].m_flags & NetNode.Flags.Junction) != 0)
                            {
                                flag7 = true;
                            }
                            firstLane = __instance.m_lanes;
                            for (int j = 0; j < m_info.m_lanes.Length; j++)
                            {
                                if (firstLane == 0)
                                {
                                    break;
                                }
                                NetLane.Flags flags4 = (NetLane.Flags)(instance.m_lanes.m_buffer[firstLane].m_flags & -9);
                                if (flag7)
                                {
                                    flags4 |= NetLane.Flags.JoinedJunction;
                                }
                                instance.m_lanes.m_buffer[firstLane].m_flags = (ushort)flags4;
                                firstLane = instance.m_lanes.m_buffer[firstLane].m_nextLane;
                            }
                        }
                        //Patch End
                    }
                }
            }
        }
    }
}
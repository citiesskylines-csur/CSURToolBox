using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
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

                                //Thanks for macsergey to optimize this
                                var laneInfo = m_info.m_lanes[i];
                                var laneOffsetUnit = CSURUtil.CSURLaneOffset(m_info, laneInfo);
                                var laneOffset = laneOffsetUnit * 3.75f;
                                var startDir = __instance.m_startDirection;
                                var endDir = __instance.m_endDirection;
                                var bezier = instance.m_lanes.m_buffer[firstLane].m_bezier;

                                Line2.Intersect(VectorUtils.XZ(bezier.a), VectorUtils.XZ(startDir), VectorUtils.XZ(bezier.Position(0.333f)), VectorUtils.XZ(-bezier.Tangent(0.333f)), out float startALength, out float startBLength);
                                Line2.Intersect(VectorUtils.XZ(bezier.Position(0.667f)), VectorUtils.XZ(bezier.Tangent(0.667f)), VectorUtils.XZ(bezier.d), VectorUtils.XZ(endDir), out float endCLength, out float endDLength);
                                var startPercent = startALength / (startALength + startBLength);
                                var endPercent = endDLength / (endCLength + endDLength);

                                var length = instance.m_lanes.m_buffer[firstLane].m_length;
                                var startAngle = Mathf.Atan((laneOffset / 4) / (length * startPercent));
                                var endAngle = Mathf.Atan((laneOffset / 4) / (length * endPercent));

                                var newStartDir = startDir.Turn(startAngle, true).normalized;
                                var newEndDir = endDir.Turn(endAngle, true).normalized;

                                Bezier3 newBezier = new Bezier3()
                                {
                                    a = bezier.a + startDir.Turn90(false).normalized * (laneOffset / 2),
                                    d = bezier.d + endDir.Turn90(false).normalized * (laneOffset / 2)
                                };
                                NetSegment.CalculateMiddlePoints(newBezier.a, newStartDir, newBezier.d, newEndDir, true, true, out newBezier.b, out newBezier.c);
                                //Thanks end.

                                instance.m_lanes.m_buffer[firstLane].m_bezier = newBezier;
                                instance.m_lanes.m_buffer[firstLane].UpdateLength();
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

        public static Vector3 Turn90(this Vector3 v, bool isClockWise) => isClockWise ? new Vector3(v.z, v.y, -v.x) : new Vector3(-v.z, v.y, v.x);
        public static Vector3 Turn(this Vector3 vector, float turnAngle, bool isClockWise)
        {
            turnAngle *= isClockWise ? -1 : Mathf.Deg2Rad;
            var newX = vector.x * Mathf.Cos(turnAngle) - vector.z * Mathf.Sin(turnAngle);
            var newZ = vector.x * Mathf.Sin(turnAngle) + vector.z * Mathf.Cos(turnAngle);
            return new Vector3(newX, vector.y, newZ);
        }
    }
}
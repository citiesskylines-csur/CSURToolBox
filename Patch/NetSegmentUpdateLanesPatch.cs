using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentUpdateLanesPatch
    {
        public static float middleT1 = 0.1f;
        public static float middleT2 = 0.9f;
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

                                var newBezier = MathLine(startDir, endDir, bezier, laneOffset);
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
        public static Bezier3 MathLine(Vector3 startDir, Vector3 endDir, Bezier3 basic, float shift)
        {
            var shiftStartPos = CalcShift(basic.a, startDir, ShiftAtPoint(shift, 0));
            var shiftStartMiddlePos = CalcShift(basic, middleT1, ShiftAtPoint(shift, middleT1 / 2));
            var shiftMiddleEndPos = CalcShift(basic, middleT2, ShiftAtPoint(shift, 0.5f + middleT2 / 2));
            var shiftEndPos = CalcShift(basic.d, endDir, -ShiftAtPoint(shift, 1));

            var bezier = CalcPerfict(shiftStartPos, shiftEndPos, shiftStartMiddlePos, shiftMiddleEndPos, middleT1, middleT2);

            return bezier;
        }
        public static float ShiftAtPoint(float shift, float point)
        {
            if (point < 0.5f)
                return -shift * (0.5f - point);
            else
                return shift * (point - 0.5f);
        }
        public static Vector3 CalcShift(Bezier3 basic, float t, float shift)
        {
            var pos = basic.Position(t);
            var dir = basic.Tangent(t);
            var shiftPos = CalcShift(pos, dir, shift);
            return shiftPos;
        }
        public static Vector3 CalcShift(Vector3 pos, Vector3 dir, float shift) => pos + dir.Turn90(true).normalized * shift;

        public static Bezier3 CalcPerfict(Vector3 start, Vector3 end, Vector3 point1, Vector3 point2, float t1, float t2)
        {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            CalcU(start, end, point1, a1, d1, out float ux1, out float uy1, out float uz1);
            CalcU(start, end, point2, a2, d2, out float ux2, out float uy2, out float uz2);

            CalcCoordinate(b1, c1, ux1, b2, c2, ux2, out float m1x, out float m2x);
            CalcCoordinate(b1, c1, uy1, b2, c2, uy2, out float m1y, out float m2y);
            CalcCoordinate(b1, c1, uz1, b2, c2, uz2, out float m1z, out float m2z);

            var middle1 = new Vector3(m1x, m1y, m1z);
            var middle2 = new Vector3(m2x, m2y, m2z);

            var bezier = new Bezier3()
            {
                a = start,
                b = middle1,
                c = middle2,
                d = end
            };

            return bezier;
        }
        public static void CalcCoef(float t, out float a, out float b, out float c, out float d)
        {
            var mt = 1 - t;
            a = mt * mt * mt;
            b = 3 * t * mt * mt;
            c = 3 * t * t * mt;
            d = t * t * t;
        }
        public static void CalcU(Vector3 start, Vector3 end, Vector3 point, float a, float d, out float ux, out float uy, out float uz)
        {
            ux = CalcU(start.x, end.x, point.x, a, d);
            uy = CalcU(start.y, end.y, point.y, a, d);
            uz = CalcU(start.z, end.z, point.z, a, d);
        }
        public static float CalcU(float start, float end, float point, float a, float d) => point - (a * start) - (d * end);
        public static void CalcCoordinate(float b1, float c1, float u1, float b2, float c2, float u2, out float coordinate1, out float coordinate2)
        {
            coordinate2 = (u2 - (b2 / b1 * u1)) / (c2 - (b2 / b1 * c1));
            coordinate1 = (u1 - c1 * coordinate2) / b1;
        }
    }
}
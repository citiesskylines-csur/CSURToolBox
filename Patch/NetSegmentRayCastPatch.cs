using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentRayCastPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Segment3), typeof(float), typeof(bool), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null);
        }
        public static bool Prefix(ref NetSegment __instance, ushort segmentID, Segment3 ray, float snapElevation, bool nameOnly, out float t, out float priority, ref bool __result)
        {
            // NON-STOCK CODE STARTS
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(__instance.Info.m_netAI.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(__instance.Info.m_netAI.m_info) && !IsCSURSLane)
            {
                __result = NetSegmentRayCastMasked(__instance, segmentID, ray, -1000f, false, out t, out priority);
                return false;
            }
            // NON-STOCK CODE ENDS
            NetInfo info = __instance.Info;
            t = 0f;
            priority = 0f;
            if (nameOnly && (__instance.m_flags & NetSegment.Flags.NameVisible2) == NetSegment.Flags.None)
            {
                __result = false;
                return false;
            }
            Bounds bounds = __instance.m_bounds;
            bounds.Expand(16f);
            if (!bounds.IntersectRay(new Ray(ray.a, ray.b - ray.a)))
            {
                __result = false;
                return false;
            }
            NetManager instance = Singleton<NetManager>.instance;
            Bezier3 bezier = default(Bezier3);
            bezier.a = instance.m_nodes.m_buffer[__instance.m_startNode].m_position;
            bezier.d = instance.m_nodes.m_buffer[__instance.m_endNode].m_position;
            bool result = false;
            if (nameOnly)
            {
                RenderManager instance2 = Singleton<RenderManager>.instance;
                if (instance2.GetInstanceIndex((uint)(49152 + segmentID), out uint instanceIndex))
                {
                    InstanceManager.NameData nameData = instance2.m_instances[instanceIndex].m_nameData;
                    Vector3 position = instance2.m_instances[instanceIndex].m_position;
                    Matrix4x4 dataMatrix = instance2.m_instances[instanceIndex].m_dataMatrix2;
                    float num = Vector3.Distance(position, ray.a);
                    if (nameData != null && num < 1000f)
                    {
                        float snapElevation2 = info.m_netAI.GetSnapElevation();
                        bezier.a.y += snapElevation2;
                        bezier.d.y += snapElevation2;
                        NetSegment.CalculateMiddlePoints(bezier.a, __instance.m_startDirection, bezier.d, __instance.m_endDirection, true, true, out bezier.b, out bezier.c);
                        float num2 = Mathf.Max(1f, Mathf.Abs(dataMatrix.m33 - dataMatrix.m30));
                        float d = num * 0.0002f + 0.05f / (1f + num * 0.001f);
                        Vector2 vector = nameData.m_size * d;
                        float t2 = Mathf.Max(0f, 0.5f - vector.x / num2 * 0.5f);
                        float t3 = Mathf.Min(1f, 0.5f + vector.x / num2 * 0.5f);
                        bezier = bezier.Cut(t2, t3);
                        float num3 = bezier.DistanceSqr(ray, out float u, out float _);
                        if (num3 < vector.y * vector.y * 0.25f)
                        {
                            Vector3 b = bezier.Position(u);
                            if (Segment1.Intersect(ray.a.y, ray.b.y, b.y, out u))
                            {
                                num3 = Vector3.SqrMagnitude(ray.Position(u) - b);
                                if (num3 < vector.y * vector.y * 0.25f)
                                {
                                    t = u;
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                info.m_netAI.GetRayCastHeights(segmentID, ref __instance, out float leftMin, out float rightMin, out float max);
                bezier.a.y += max;
                bezier.d.y += max;
                bool flag = (instance.m_nodes.m_buffer[__instance.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                bool flag2 = (instance.m_nodes.m_buffer[__instance.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                NetSegment.CalculateMiddlePoints(bezier.a, __instance.m_startDirection, bezier.d, __instance.m_endDirection, flag, flag2, out bezier.b, out bezier.c);
                // NON-STOCK CODE STARTS
                if (IsCSURSLane)
                {
                    float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(info);
                    float otherLaneNum = CSURUtil.CountCSURSOtherLanes(info);
                    float laneNum = vehicleLaneNum + otherLaneNum;
                    startOffset = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                    endOffset = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;

                    if ((__instance.m_flags & NetSegment.Flags.Invert) != 0)
                    {
                        startOffset = -startOffset;
                        endOffset = -endOffset;
                    }
                    //EG: before patch: point1-point4 is 1.5*3.75
                    //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                    Vector3 newBezierA = bezier.a + (new Vector3(__instance.m_startDirection.z, 0, -__instance.m_startDirection.x).normalized) * (startOffset);
                    Vector3 newBezierBDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.333f));
                    Vector3 newBezierB = bezier.b + (new Vector3(newBezierBDir.z, 0, -newBezierBDir.x).normalized) * (startOffset * 0.667f + endOffset * 0.333f);
                    Vector3 newBezierCDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.667f));
                    Vector3 newBezierC = bezier.c + (new Vector3(newBezierCDir.z, 0, -newBezierCDir.x).normalized) * (startOffset * 0.333f + endOffset * 0.667f);
                    Vector3 newBezierD = bezier.d + (new Vector3(-__instance.m_endDirection.z, 0, __instance.m_endDirection.x).normalized) * (endOffset);

                    bezier.a = newBezierA;
                    bezier.b = newBezierB;
                    bezier.c = newBezierC;
                    bezier.d = newBezierD;
                }
                float minNodeDistance = info.GetMinNodeDistance();
                float collisionHalfWidth = info.m_netAI.GetCollisionHalfWidth();
                float num4 = (float)(int)instance.m_nodes.m_buffer[__instance.m_startNode].m_elevation;
                float num5 = (float)(int)instance.m_nodes.m_buffer[__instance.m_endNode].m_elevation;
                if (info.m_netAI.IsUnderground())
                {
                    num4 = 0f - num4;
                    num5 = 0f - num5;
                }
                num4 += info.m_netAI.GetSnapElevation();
                num5 += info.m_netAI.GetSnapElevation();
                float a = Mathf.Lerp(minNodeDistance, collisionHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num4) / 12f));
                float b2 = Mathf.Lerp(minNodeDistance, collisionHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num5) / 12f));
                float num6 = Mathf.Min(leftMin, rightMin);
                t = 1000000f;
                priority = 1000000f;
                Segment3 segment = default(Segment3);
                segment.a = bezier.a;
                for (int i = 1; i <= 16; i++)
                {
                    segment.b = bezier.Position((float)i / 16f);
                    float num7 = ray.DistanceSqr(segment, out float u2, out float v2);
                    float num8 = Mathf.Lerp(a, b2, ((float)(i - 1) + v2) / 16f);
                    Vector3 vector2 = segment.Position(v2);
                    if (num7 < priority && Segment1.Intersect(ray.a.y, ray.b.y, vector2.y, out u2))
                    {
                        Vector3 vector3 = ray.Position(u2);
                        num7 = Vector3.SqrMagnitude(vector3 - vector2);
                        if (num7 < priority && num7 < num8 * num8)
                        {
                            if (flag && i == 1 && v2 < 0.001f)
                            {
                                Vector3 rhs = segment.a - segment.b;
                                u2 += Mathf.Max(0f, Vector3.Dot(vector3, rhs)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs.sqrMagnitude * ray.LengthSqr()));
                            }
                            if (flag2 && i == 16 && v2 > 0.999f)
                            {
                                Vector3 rhs2 = segment.b - segment.a;
                                u2 += Mathf.Max(0f, Vector3.Dot(vector3, rhs2)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs2.sqrMagnitude * ray.LengthSqr()));
                            }
                            priority = num7;
                            t = u2;
                            result = true;
                        }
                    }
                    if (num6 < max)
                    {
                        float num9 = vector2.y + num6 - max;
                        if (Mathf.Max(ray.a.y, ray.b.y) > num9 && Mathf.Min(ray.a.y, ray.b.y) < vector2.y)
                        {
                            Segment2 segment2 = default(Segment2);
                            float num10;
                            if (Segment1.Intersect(ray.a.y, ray.b.y, vector2.y, out u2))
                            {
                                segment2.a = VectorUtils.XZ(ray.Position(u2));
                                num10 = u2;
                            }
                            else
                            {
                                segment2.a = VectorUtils.XZ(ray.a);
                                num10 = 0f;
                            }
                            float num11;
                            if (Segment1.Intersect(ray.a.y, ray.b.y, num9, out u2))
                            {
                                segment2.b = VectorUtils.XZ(ray.Position(u2));
                                num11 = u2;
                            }
                            else
                            {
                                segment2.b = VectorUtils.XZ(ray.b);
                                num11 = 1f;
                            }
                            num7 = segment2.DistanceSqr(VectorUtils.XZ(vector2), out u2);
                            if (num7 < priority && num7 < num8 * num8)
                            {
                                u2 = num10 + (num11 - num10) * u2;
                                Vector3 lhs = ray.Position(u2);
                                if (flag && i == 1 && v2 < 0.001f)
                                {
                                    Vector3 rhs3 = segment.a - segment.b;
                                    u2 += Mathf.Max(0f, Vector3.Dot(lhs, rhs3)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs3.sqrMagnitude * ray.LengthSqr()));
                                }
                                if (flag2 && i == 16 && v2 > 0.999f)
                                {
                                    Vector3 rhs4 = segment.b - segment.a;
                                    u2 += Mathf.Max(0f, Vector3.Dot(lhs, rhs4)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs4.sqrMagnitude * ray.LengthSqr()));
                                }
                                priority = num7;
                                t = u2;
                                result = true;
                            }
                        }
                    }
                    segment.a = segment.b;
                }
                priority = Mathf.Max(0f, Mathf.Sqrt(priority) - collisionHalfWidth);
            }
            __result = result;
            return false;
        }

        public static bool NetSegmentRayCastMasked(NetSegment mysegment, ushort segmentID, Segment3 ray, float snapElevation, bool bothSides, out float t, out float priority)
        {
            bool lht = false;
            //if (SimulationManager.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True) lht = true;
            //Debug.Log(mysegment.m_flags);
            if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
            bool isMasked = false;
            NetInfo info = mysegment.Info;
            t = 0f;
            priority = 0f;
            Bounds bounds = mysegment.m_bounds;
            bounds.Expand(16f);
            if (!bounds.IntersectRay(new Ray(ray.a, ray.b - ray.a)))
            {
                return false;
            }
            NetManager instance = Singleton<NetManager>.instance;
            Bezier3 bezier = default(Bezier3);
            bezier.a = instance.m_nodes.m_buffer[mysegment.m_startNode].m_position;
            bezier.d = instance.m_nodes.m_buffer[mysegment.m_endNode].m_position;
            bool result = false;

            info.m_netAI.GetRayCastHeights(segmentID, ref mysegment, out float leftMin, out float rightMin, out float max);
            bezier.a.y += max;
            bezier.d.y += max;
            bool flag = (instance.m_nodes.m_buffer[mysegment.m_startNode].m_flags & NetNode.Flags.Middle) != 0;
            bool flag2 = (instance.m_nodes.m_buffer[mysegment.m_endNode].m_flags & NetNode.Flags.Middle) != 0;
            NetSegment.CalculateMiddlePoints(bezier.a, mysegment.m_startDirection, bezier.d, mysegment.m_endDirection, flag, flag2, out bezier.b, out bezier.c);
            float minNodeDistance = info.GetMinNodeDistance();
            // 
            float collisionHalfWidth = info.m_halfWidth;
            float maskHalfWidth = info.m_pavementWidth;
            //
            float num4 = (int)instance.m_nodes.m_buffer[mysegment.m_startNode].m_elevation;
            float num5 = (int)instance.m_nodes.m_buffer[mysegment.m_endNode].m_elevation;
            if (info.m_netAI.IsUnderground())
            {
                num4 = 0f - num4;
                num5 = 0f - num5;
            }
            num4 += info.m_netAI.GetSnapElevation();
            num5 += info.m_netAI.GetSnapElevation();
            float a = Mathf.Lerp(minNodeDistance, collisionHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num4) / 12f));
            float b2 = Mathf.Lerp(minNodeDistance, collisionHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num5) / 12f));
            float am = Mathf.Lerp(minNodeDistance, maskHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num4) / 12f));
            float b2m = Mathf.Lerp(minNodeDistance, maskHalfWidth, Mathf.Clamp01(Mathf.Abs(snapElevation - num5) / 12f));
            float num6 = Mathf.Min(leftMin, rightMin);
            t = 1000000f;
            priority = 1000000f;
            Segment3 segment = default(Segment3);
            segment.a = bezier.a;
            Segment2 segment2 = default(Segment2);
            //Debug.Log($"mouse ray: {ray.a} --> {ray.b}");
            //Debug.Log($"segment direction: {bezier.a} --> {bezier.b}");
            for (int i = 1; i <= 16; i++)
            {
                segment.b = bezier.Position((float)i / 16f);
                float num7 = ray.DistanceSqr(segment, out float u2, out float v2);
                float num8 = Mathf.Lerp(a, b2, ((float)(i - 1) + v2) / 16f);
                float num8m = Mathf.Lerp(am, b2m, ((float)(i - 1) + v2) / 16f);
                Vector3 vector2 = segment.Position(v2);
                bool atOffsetSide = bothSides || IsTrafficHandSideOf(segment, ray, u2, lht);
                if (atOffsetSide && num7 < priority && Segment1.Intersect(ray.a.y, ray.b.y, vector2.y, out u2))
                {
                    Vector3 vector3 = ray.Position(u2);
                    num7 = Vector3.SqrMagnitude(vector3 - vector2);
                    //Debug.Log($"num7: {num7}, num8: {num8}, num8m: {num8m}");
                    if (num7 < priority && num7 < num8 * num8)
                    {

                        if (flag && i == 1 && v2 < 0.001f)
                        {
                            Vector3 rhs = segment.a - segment.b;
                            u2 += Mathf.Max(0f, Vector3.Dot(vector3, rhs)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs.sqrMagnitude * ray.LengthSqr()));
                        }
                        if (flag2 && i == 16 && v2 > 0.999f)
                        {
                            Vector3 rhs2 = segment.b - segment.a;
                            u2 += Mathf.Max(0f, Vector3.Dot(vector3, rhs2)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs2.sqrMagnitude * ray.LengthSqr()));
                        }
                        priority = num7;
                        t = u2;
                        result = true;
                        if (num7 < num8m * num8m) isMasked = true;
                    }
                }
                if (atOffsetSide && num6 < max)
                {
                    float num9 = vector2.y + num6 - max;
                    if (Mathf.Max(ray.a.y, ray.b.y) > num9 && Mathf.Min(ray.a.y, ray.b.y) < vector2.y)
                    {
                        float num10;
                        if (Segment1.Intersect(ray.a.y, ray.b.y, vector2.y, out u2))
                        {
                            segment2.a = VectorUtils.XZ(ray.Position(u2));
                            num10 = u2;
                        }
                        else
                        {
                            segment2.a = VectorUtils.XZ(ray.a);
                            num10 = 0f;
                        }
                        float num11;
                        if (Segment1.Intersect(ray.a.y, ray.b.y, num9, out u2))
                        {
                            segment2.b = VectorUtils.XZ(ray.Position(u2));
                            num11 = u2;
                        }
                        else
                        {
                            segment2.b = VectorUtils.XZ(ray.b);
                            num11 = 1f;
                        }
                        num7 = segment2.DistanceSqr(VectorUtils.XZ(vector2), out u2);
                        if (num7 < priority && num7 < num8 * num8)
                        {
                            u2 = num10 + (num11 - num10) * u2;
                            Vector3 lhs = ray.Position(u2);
                            if (flag && i == 1 && v2 < 0.001f)
                            {
                                Vector3 rhs3 = segment.a - segment.b;
                                u2 += Mathf.Max(0f, Vector3.Dot(lhs, rhs3)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs3.sqrMagnitude * ray.LengthSqr()));
                            }
                            if (flag2 && i == 16 && v2 > 0.999f)
                            {
                                Vector3 rhs4 = segment.b - segment.a;
                                u2 += Mathf.Max(0f, Vector3.Dot(lhs, rhs4)) / Mathf.Max(0.001f, Mathf.Sqrt(rhs4.sqrMagnitude * ray.LengthSqr()));
                            }
                            priority = num7;
                            t = u2;
                            result = true;
                            if (num7 < num8m * num8m) isMasked = true;
                        }
                    }
                }
                segment.a = segment.b;
            }
            priority = Mathf.Max(0f, Mathf.Sqrt(priority) - collisionHalfWidth);

            if (isMasked) result = false;
            return result;
        }

        public static bool IsTrafficHandSideOf(Segment3 segment, Segment3 ray, float tRay, bool invert)
        {
            Vector3 segmentVector = segment.b - segment.a;
            Vector3 rayVector = ray.Position(tRay) - segment.a;
            // Debug.Log($"startnode->endnode: {segmentVector}, startnode->ray: {rayVector}");
            float crossProduct = rayVector.x * segmentVector.z - segmentVector.x * rayVector.z;
            // Debug.Log($"cross product: {crossProduct}");
            return invert ? crossProduct < 0 : crossProduct > 0;
        }
    }
}

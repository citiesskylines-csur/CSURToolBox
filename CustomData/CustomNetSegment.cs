using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CSURToolBox.CustomData
{
    public static class CustomNetSegment
    {
        public static void CalculateMiddlePoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool smoothStart, bool smoothEnd, out Vector3 middlePos1, out Vector3 middlePos2)
        {
            if (IsStraight(startPos, startDir, endPos, endDir, out float distance))
            {
                middlePos1 = startPos + startDir * (distance * ((!smoothStart) ? 0.15f : 0.276f));
                middlePos2 = endPos + endDir * (distance * ((!smoothEnd) ? 0.15f : 0.276f));
            }
            else
            {
                float num = startDir.x * endDir.x + startDir.z * endDir.z;
                if (num >= -0.999f && Line2.Intersect(VectorUtils.XZ(startPos), VectorUtils.XZ(startPos + startDir), VectorUtils.XZ(endPos), VectorUtils.XZ(endPos + endDir), out float u, out float v))
                {
                    u = Mathf.Clamp(u, distance * 0.1f, distance);
                    v = Mathf.Clamp(v, distance * 0.1f, distance);
                    float num2 = u + v;
                    middlePos1 = startPos + startDir * Mathf.Min(u, num2 * 0.276f);
                    middlePos2 = endPos + endDir * Mathf.Min(v, num2 * 0.276f);
                }
                else
                {
                    middlePos1 = startPos + startDir * (distance * 0.276f);
                    middlePos2 = endPos + endDir * (distance * 0.276f);
                }
            }
        }

        public static bool IsStraight(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, out float distance)
        {
            Vector3 vector = VectorUtils.NormalizeXZ(endPos - startPos, out distance);
            float num = startDir.x * endDir.x + startDir.z * endDir.z;
            float num2 = startDir.x * vector.x + startDir.z * vector.z;
            return num < -0.999f && num2 > 0.999f;
        }

        public static bool RayCast(ref NetSegment mysegment, ushort segmentID, Segment3 ray, float snapElevation, bool nameOnly, out float t, out float priority)
        {
            if (CSUROffset.IsCSUROffset(mysegment.Info.m_netAI.m_info))
            {
                return NetSegmentRayCastMasked(mysegment, segmentID, ray, -1000f, false, out t, out priority);
            }
            NetInfo info = mysegment.Info;
            t = 0f;
            priority = 0f;
            if (nameOnly && (mysegment.m_flags & NetSegment.Flags.NameVisible2) == NetSegment.Flags.None)
            {
                return false;
            }
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
                        NetSegment.CalculateMiddlePoints(bezier.a, mysegment.m_startDirection, bezier.d, mysegment.m_endDirection, true, true, out bezier.b, out bezier.c);
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
                info.m_netAI.GetRayCastHeights(segmentID, ref mysegment, out float leftMin, out float rightMin, out float max);
                bezier.a.y += max;
                bezier.d.y += max;
                bool flag = (instance.m_nodes.m_buffer[mysegment.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                bool flag2 = (instance.m_nodes.m_buffer[mysegment.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                NetSegment.CalculateMiddlePoints(bezier.a, mysegment.m_startDirection, bezier.d, mysegment.m_endDirection, flag, flag2, out bezier.b, out bezier.c);
                float minNodeDistance = info.GetMinNodeDistance();
                float collisionHalfWidth = info.m_netAI.GetCollisionHalfWidth();
                float num4 = (float)(int)instance.m_nodes.m_buffer[mysegment.m_startNode].m_elevation;
                float num5 = (float)(int)instance.m_nodes.m_buffer[mysegment.m_endNode].m_elevation;
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
            return result;
        }

        public static bool NetSegmentRayCastMasked(NetSegment mysegment, ushort segmentID, Segment3 ray, float snapElevation, bool bothSides, out float t, out float priority)
        {
            bool lht = false;
            //if (SimulationManager.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True) lht = true;
            Debug.Log(mysegment.m_flags);
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
            Debug.Log($"mouse ray: {ray.a} --> {ray.b}");
            Debug.Log($"segment direction: {bezier.a} --> {bezier.b}");
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

        // NON-STOCK CODE ENDS
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

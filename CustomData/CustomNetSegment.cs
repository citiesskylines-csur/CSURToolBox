using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
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
            // NON-STOCK CODE STARTS
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(mysegment.Info.m_netAI.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(mysegment.Info.m_netAI.m_info) && !IsCSURSLane)
            {
                return NetSegmentRayCastMasked(mysegment, segmentID, ray, -1000f, false, out t, out priority);
            }
            // NON-STOCK CODE ENDS
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
                // NON-STOCK CODE STARTS
                if (IsCSURSLane)
                {
                    float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(info);
                    float otherLaneNum = CSURUtil.CountCSURSOtherLanes(info);
                    float laneNum = vehicleLaneNum + otherLaneNum;
                    startOffset = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                    endOffset = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;

                    if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0)
                    {
                        startOffset = -startOffset;
                        endOffset = -endOffset;
                    }
                    //EG: before patch: point1-point4 is 1.5*3.75
                    //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                    Vector3 newBezierA = bezier.a + (new Vector3(mysegment.m_startDirection.z, 0, -mysegment.m_startDirection.x).normalized) * (startOffset);
                    Vector3 newBezierBDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.333f));
                    Vector3 newBezierB = bezier.b + (new Vector3(newBezierBDir.z, 0, -newBezierBDir.x).normalized) * (startOffset * 0.667f + endOffset * 0.333f);
                    Vector3 newBezierCDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.667f));
                    Vector3 newBezierC = bezier.c + (new Vector3(newBezierCDir.z, 0, -newBezierCDir.x).normalized) * (startOffset * 0.333f + endOffset * 0.667f);
                    Vector3 newBezierD = bezier.d + (new Vector3(-mysegment.m_endDirection.z, 0, mysegment.m_endDirection.x).normalized) * (endOffset);

                    bezier.a = newBezierA;
                    bezier.b = newBezierB;
                    bezier.c = newBezierC;
                    bezier.d = newBezierD;
                }
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

        public static bool OverlapQuad(ref NetSegment mysegment, ushort segmentID, Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType)
        {
            if ((mysegment.m_flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) != NetSegment.Flags.Created)
            {
                return false;
            }
            NetInfo info = mysegment.Info;
            if (!info.m_canCollide)
            {
                return false;
            }
            float collisionHalfWidth = info.m_netAI.GetCollisionHalfWidth();
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            Bezier3 bezier = default(Bezier3);
            bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].m_position;
            bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].m_position;
            // NON-STOCK CODE STARTS
            if (CSURUtil.IsCSUROffset(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info))
            {
                var width = (Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info.m_halfWidth + Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info.m_pavementWidth) / 2f;
                bool lht = false;
                Vector3 direction = mysegment.m_startDirection;
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                bezier.a = bezier.a + (lht ? -width : width) * normal;
            }
            if (CSURUtil.IsCSUROffset(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].Info))
            {
                bool lht = false;
                var width = (Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].Info.m_halfWidth + Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].Info.m_pavementWidth) / 2f;
                Vector3 direction = -mysegment.m_endDirection;
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                bezier.d = bezier.d + (lht ? -width : width) * normal;
            }
            // NON-STOCK CODE ENDS
            CalculateMiddlePoints(bezier.a, mysegment.m_startDirection, bezier.d, mysegment.m_endDirection, true, true, out bezier.b, out bezier.c);
            // NON-STOCK CODE STARTS
            float laneOffsetS = 0;
            float startOffsetS = 0;
            float endOffsetS = 0;
            bool IsCSURSLaneS = CSURUtil.IsCSURSLane(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info, ref laneOffsetS, ref startOffsetS, ref endOffsetS);
            float laneOffsetE = 0;
            float startOffsetE = 0;
            float endOffsetE = 0;
            bool IsCSURSLaneE = CSURUtil.IsCSURSLane(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].Info, ref laneOffsetE, ref startOffsetE, ref endOffsetE);
            Vector3 newBezierA1 = Vector3.zero;
            Vector3 newBezierB1 = Vector3.zero;
            Vector3 newBezierC1 = Vector3.zero;
            Vector3 newBezierD1 = Vector3.zero;
            if (IsCSURSLaneS || IsCSURSLaneE)
            {
                float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info);
                float otherLaneNum = CSURUtil.CountCSURSOtherLanes(Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].Info);
                float laneNum = vehicleLaneNum + otherLaneNum;
                startOffsetS = startOffsetS * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                endOffsetE = endOffsetE * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0)
                {
                    startOffsetS = -startOffsetS;
                    endOffsetE = -endOffsetE;
                }

                if (!IsCSURSLaneS) { startOffsetS = 0; }
                if (!IsCSURSLaneE) { endOffsetE = 0; }
                //EG: before patch: point1-point4 is 1.5*3.75
                //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                newBezierA1 = bezier.a + (new Vector3(mysegment.m_startDirection.z, 0, -mysegment.m_startDirection.x).normalized) * (startOffsetS);
                Vector3 newBezierBDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.333f));
                newBezierB1 = bezier.b + (new Vector3(newBezierBDir.z, 0, -newBezierBDir.x).normalized) * (startOffsetS * 0.667f + endOffsetE * 0.333f);
                Vector3 newBezierCDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.667f));
                newBezierC1 = bezier.c + (new Vector3(newBezierCDir.z, 0, -newBezierCDir.x).normalized) * (startOffsetS * 0.333f + endOffsetE * 0.667f);
                newBezierD1 = bezier.d + (new Vector3(-mysegment.m_endDirection.z, 0, mysegment.m_endDirection.x).normalized) * (endOffsetE);

                bezier.a = newBezierA1;
                bezier.b = newBezierB1;
                bezier.c = newBezierC1;
                bezier.d = newBezierD1;
            }
            // NON-STOCK CODE ENDS

            Vector3 vector3 = bezier.Min() + new Vector3(0f - collisionHalfWidth, info.m_minHeight, 0f - collisionHalfWidth);
            Vector3 vector4 = bezier.Max() + new Vector3(collisionHalfWidth, info.m_maxHeight, collisionHalfWidth);
            ItemClass.CollisionType collisionType2 = info.m_netAI.GetCollisionType();
            if (vector3.x <= vector2.x && vector3.z <= vector2.y && vector.x <= vector4.x && vector.y <= vector4.z && ItemClass.CheckCollisionType(minY, maxY, vector3.y, vector4.y, collisionType, collisionType2))
            {
                int num = 16;
                info.m_netAI.GetTerrainModifyRange(out float start, out float end);
                start *= 0.5f;
                end = 1f - (1f - end) * 0.5f;
                float t = start;
                Vector3 vector5 = bezier.Position(t);
                Vector3 vector6 = bezier.Tangent(t);
                vector6 = new Vector3(0f - vector6.z, 0f, vector6.x).normalized * collisionHalfWidth;
                Vector3 a = vector5 + vector6;
                Vector3 vector7 = vector5 - vector6;
                float endRadius = info.m_netAI.GetEndRadius();
                if (info.m_clipSegmentEnds && endRadius != 0f && start == 0f && (Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_startNode].m_flags & (NetNode.Flags.End | NetNode.Flags.Bend | NetNode.Flags.Junction)) != 0)
                {
                    Vector3 vector8 = vector5;
                    vector8.x += vector6.x * 0.8f - vector6.z * 0.6f * endRadius / collisionHalfWidth;
                    vector8.z += vector6.z * 0.8f + vector6.x * 0.6f * endRadius / collisionHalfWidth;
                    Vector3 vector9 = vector5;
                    vector9.x -= vector6.x * 0.8f + vector6.z * 0.6f * endRadius / collisionHalfWidth;
                    vector9.z -= vector6.z * 0.8f - vector6.x * 0.6f * endRadius / collisionHalfWidth;
                    Vector3 d = vector5;
                    d.x += vector6.x * 0.3f - vector6.z * endRadius / collisionHalfWidth;
                    d.z += vector6.z * 0.3f + vector6.x * endRadius / collisionHalfWidth;
                    Vector3 c = vector5;
                    c.x -= vector6.x * 0.3f + vector6.z * endRadius / collisionHalfWidth;
                    c.z -= vector6.z * 0.3f - vector6.x * endRadius / collisionHalfWidth;
                    vector3.y = vector5.y + info.m_minHeight;
                    vector4.y = vector5.y + info.m_maxHeight;
                    if (ItemClass.CheckCollisionType(minY, maxY, vector3.y, vector4.y, collisionType, collisionType2))
                    {
                        if (quad.Intersect(Quad2.XZ(a, vector7, vector9, vector8)))
                        {
                            return true;
                        }
                        if (quad.Intersect(Quad2.XZ(vector8, vector9, c, d)))
                        {
                            return true;
                        }
                    }
                }
                for (int i = 1; i <= num; i++)
                {
                    t = start + (end - start) * (float)i / (float)num;
                    vector5 = bezier.Position(t);
                    vector6 = bezier.Tangent(t);
                    vector6 = new Vector3(0f - vector6.z, 0f, vector6.x).normalized * collisionHalfWidth;
                    Vector3 vector10 = vector5 + vector6;
                    Vector3 vector11 = vector5 - vector6;
                    vector3.y = Mathf.Min(Mathf.Min(a.y, vector10.y), Mathf.Min(vector11.y, vector7.y)) + info.m_minHeight;
                    vector4.y = Mathf.Max(Mathf.Max(a.y, vector10.y), Mathf.Max(vector11.y, vector7.y)) + info.m_maxHeight;
                    if (ItemClass.CheckCollisionType(minY, maxY, vector3.y, vector4.y, collisionType, collisionType2) && quad.Intersect(Quad2.XZ(a, vector10, vector11, vector7)))
                    {
                        return true;
                    }
                    a = vector10;
                    vector7 = vector11;
                }
                if (info.m_clipSegmentEnds && endRadius != 0f && end == 1f && (Singleton<NetManager>.instance.m_nodes.m_buffer[mysegment.m_endNode].m_flags & (NetNode.Flags.End | NetNode.Flags.Bend | NetNode.Flags.Junction)) != 0)
                {
                    Vector3 vector12 = vector5;
                    vector12.x += vector6.x * 0.8f + vector6.z * 0.6f * endRadius / collisionHalfWidth;
                    vector12.z += vector6.z * 0.8f - vector6.x * 0.6f * endRadius / collisionHalfWidth;
                    Vector3 vector13 = vector5;
                    vector13.x -= vector6.x * 0.8f - vector6.z * 0.6f * endRadius / collisionHalfWidth;
                    vector13.z -= vector6.z * 0.8f + vector6.x * 0.6f * endRadius / collisionHalfWidth;
                    Vector3 b = vector5;
                    b.x += vector6.x * 0.3f + vector6.z * endRadius / collisionHalfWidth;
                    b.z += vector6.z * 0.3f - vector6.x * endRadius / collisionHalfWidth;
                    Vector3 c2 = vector5;
                    c2.x -= vector6.x * 0.3f - vector6.z * endRadius / collisionHalfWidth;
                    c2.z -= vector6.z * 0.3f + vector6.x * endRadius / collisionHalfWidth;
                    vector3.y = vector5.y + info.m_minHeight;
                    vector4.y = vector5.y + info.m_maxHeight;
                    if (ItemClass.CheckCollisionType(minY, maxY, vector3.y, vector4.y, collisionType, collisionType2))
                    {
                        if (quad.Intersect(Quad2.XZ(a, vector12, vector13, vector7)))
                        {
                            return true;
                        }
                        if (quad.Intersect(Quad2.XZ(vector12, b, c2, vector13)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void CalculateCorner(ref NetSegment segment, ushort segmentID, bool heightOffset, bool start, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
        {
            NetInfo info = segment.Info;
            NetManager instance = Singleton<NetManager>.instance;
            ushort num = (!start) ? segment.m_endNode : segment.m_startNode;
            ushort num2 = (!start) ? segment.m_startNode : segment.m_endNode;
            Vector3 position = instance.m_nodes.m_buffer[(int)num].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[(int)num2].m_position;
            Vector3 startDir = (!start) ? segment.m_endDirection : segment.m_startDirection;
            Vector3 endDir = (!start) ? segment.m_startDirection : segment.m_endDirection;
            // NON-STOCK CODE STARTS
            float m_minCornerOffset = 0f;
            float tempMinCornerOffset = 1000f;
            for (int i = 0; i < 8; i++)
            {
                ushort segment1 = instance.m_nodes.m_buffer[num].GetSegment(i);
                if (segment1 != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset < tempMinCornerOffset)
                    {
                        tempMinCornerOffset = Singleton<NetManager>.instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset;
                    }
                }
            }
            if (tempMinCornerOffset != 1000f)
            {
                m_minCornerOffset = tempMinCornerOffset;
            }
            // NON-STOCK CODE END
            CalculateCorner(m_minCornerOffset, info, position, position2, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, segmentID, num, heightOffset, leftSide, out cornerPos, out cornerDirection, out smooth);
        }

        public static void CalculateCorner(float minCornerOffset, NetInfo info, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, NetInfo extraInfo1, Vector3 extraEndPos1, Vector3 extraStartDir1, Vector3 extraEndDir1, NetInfo extraInfo2, Vector3 extraEndPos2, Vector3 extraStartDir2, Vector3 extraEndDir2, ushort ignoreSegmentID, ushort startNodeID, bool heightOffset, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Bezier3 bezier = default(Bezier3);
            Bezier3 bezier2 = default(Bezier3);
            NetNode.Flags flags = NetNode.Flags.End;
            ushort num = 0;
            if (startNodeID != 0)
            {
                flags = instance.m_nodes.m_buffer[startNodeID].m_flags;
                num = instance.m_nodes.m_buffer[startNodeID].m_building;
            }
            cornerDirection = startDir;
            float d = (!leftSide) ? (0f - info.m_halfWidth) : info.m_halfWidth;
            smooth = ((flags & NetNode.Flags.Middle) != NetNode.Flags.None);
            if ((object)extraInfo1 != null)
            {
                flags = (((flags & NetNode.Flags.End) == NetNode.Flags.None || !info.IsCombatible(extraInfo1) || (object)extraInfo2 != null) ? ((flags & ~(NetNode.Flags.Middle | NetNode.Flags.Bend)) | NetNode.Flags.Junction) : ((!(startDir.x * extraStartDir1.x + startDir.z * extraStartDir1.z < -0.999f)) ? ((flags & ~NetNode.Flags.End) | NetNode.Flags.Bend) : ((flags & ~NetNode.Flags.End) | NetNode.Flags.Middle)));
            }
            if ((flags & NetNode.Flags.Middle) != 0)
            {
                int num2 = ((object)extraInfo1 != null) ? (-1) : 0;
                int num3 = (startNodeID != 0) ? 8 : 0;
                int num4 = num2;
                while (true)
                {
                    if (num4 < num3)
                    {
                        Vector3 b;
                        if (num4 == -1)
                        {
                            b = extraStartDir1;
                        }
                        else
                        {
                            ushort segment = instance.m_nodes.m_buffer[startNodeID].GetSegment(num4);
                            if (segment == 0 || segment == ignoreSegmentID)
                            {
                                num4++;
                                continue;
                            }
                            ushort startNode = instance.m_segments.m_buffer[segment].m_startNode;
                            b = ((startNodeID == startNode) ? instance.m_segments.m_buffer[segment].m_startDirection : instance.m_segments.m_buffer[segment].m_endDirection);
                        }
                        cornerDirection = VectorUtils.NormalizeXZ(cornerDirection - b);
                    }
                    break;
                }
            }
            Vector3 vector = Vector3.Cross(cornerDirection, Vector3.up).normalized;
            if (info.m_twistSegmentEnds)
            {
                if (num != 0)
                {
                    float angle = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].m_angle;
                    Vector3 vector2 = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    vector = ((!(Vector3.Dot(vector, vector2) >= 0f)) ? (-vector2) : vector2);
                }
                else if ((flags & NetNode.Flags.Junction) != 0 && startNodeID != 0)
                {
                    Vector3 vector3 = Vector3.zero;
                    int num5 = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segment2 = instance.m_nodes.m_buffer[startNodeID].GetSegment(i);
                        if (segment2 != 0 && segment2 != ignoreSegmentID && (instance.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Untouchable) != 0)
                        {
                            Vector3 vector4 = (instance.m_segments.m_buffer[segment2].m_startNode == startNodeID) ? instance.m_segments.m_buffer[segment2].m_startDirection : instance.m_segments.m_buffer[segment2].m_endDirection;
                            vector3 = new Vector3(vector4.z, 0f, 0f - vector4.x);
                            num5++;
                        }
                    }
                    if (num5 == 1)
                    {
                        vector = ((!(Vector3.Dot(vector, vector3) >= 0f)) ? (-vector3) : vector3);
                    }
                }
            }
            bezier.a = startPos + vector * d;
            bezier2.a = startPos - vector * d;
            cornerPos = bezier.a;
            if (((flags & NetNode.Flags.Junction) != 0 && info.m_clipSegmentEnds) || (flags & (NetNode.Flags.Bend | NetNode.Flags.Outside)) != 0)
            {
                vector = Vector3.Cross(endDir, Vector3.up).normalized;
                bezier.d = endPos - vector * d;
                bezier2.d = endPos + vector * d;
                NetSegment.CalculateMiddlePoints(bezier.a, cornerDirection, bezier.d, endDir, false, false, out bezier.b, out bezier.c);
                NetSegment.CalculateMiddlePoints(bezier2.a, cornerDirection, bezier2.d, endDir, false, false, out bezier2.b, out bezier2.c);
                Bezier2 bezier3 = Bezier2.XZ(bezier);
                Bezier2 bezier4 = Bezier2.XZ(bezier2);
                float num6 = -1f;
                float num7 = -1f;
                bool flag = false;
                int num8 = ((object)extraInfo1 != null) ? (((object)extraInfo2 == null) ? (-1) : (-2)) : 0;
                int num9 = (startNodeID != 0) ? 8 : 0;
                float a = info.m_halfWidth * 0.5f;
                int num10 = 0;
                for (int j = num8; j < num9; j++)
                {
                    NetInfo netInfo;
                    Vector3 vector5;
                    switch (j)
                    {
                        case -2:
                            netInfo = extraInfo2;
                            vector5 = extraStartDir2;
                            if (extraEndPos2 == endPos && extraEndDir2 == endDir)
                            {
                                break;
                            }
                            goto IL_05e9;
                        case -1:
                            netInfo = extraInfo1;
                            vector5 = extraStartDir1;
                            if (extraEndPos1 == endPos && extraEndDir1 == endDir)
                            {
                                break;
                            }
                            goto IL_05e9;
                        default:
                            {
                                ushort segment3 = instance.m_nodes.m_buffer[startNodeID].GetSegment(j);
                                if (segment3 == 0 || segment3 == ignoreSegmentID)
                                {
                                    break;
                                }
                                netInfo = instance.m_segments.m_buffer[segment3].Info;
                                vector5 = instance.m_segments.m_buffer[segment3].GetDirection(startNodeID);
                                goto IL_05e9;
                            }
                        IL_05e9:
                            if ((object)netInfo != null && info.m_clipSegmentEnds == netInfo.m_clipSegmentEnds)
                            {
                                if (netInfo.m_netAI.GetSnapElevation() > info.m_netAI.GetSnapElevation())
                                {
                                    float num11 = 0.01f - Mathf.Min(info.m_maxTurnAngleCos, netInfo.m_maxTurnAngleCos);
                                    float num12 = vector5.x * startDir.x + vector5.z * startDir.z;
                                    if ((info.m_vehicleTypes & netInfo.m_vehicleTypes) == VehicleInfo.VehicleType.None || num12 >= num11)
                                    {
                                        break;
                                    }
                                }
                                a = Mathf.Max(a, netInfo.m_halfWidth * 0.5f);
                                num10++;
                            }
                            break;
                    }
                }
                if (num10 >= 1 || (flags & NetNode.Flags.Outside) != 0)
                {
                    for (int k = num8; k < num9; k++)
                    {
                        NetInfo netInfo2;
                        Vector3 vector9;
                        Vector3 vector6;
                        Vector3 vector7;
                        switch (k)
                        {
                            case -2:
                                netInfo2 = extraInfo2;
                                vector9 = extraEndPos2;
                                vector6 = extraStartDir2;
                                vector7 = extraEndDir2;
                                if (vector9 == endPos && vector7 == endDir)
                                {
                                    break;
                                }
                                goto IL_082e;
                            case -1:
                                netInfo2 = extraInfo1;
                                vector9 = extraEndPos1;
                                vector6 = extraStartDir1;
                                vector7 = extraEndDir1;
                                if (vector9 == endPos && vector7 == endDir)
                                {
                                    break;
                                }
                                goto IL_082e;
                            default:
                                {
                                    ushort segment4 = instance.m_nodes.m_buffer[startNodeID].GetSegment(k);
                                    if (segment4 == 0 || segment4 == ignoreSegmentID)
                                    {
                                        break;
                                    }
                                    ushort startNode2 = instance.m_segments.m_buffer[segment4].m_startNode;
                                    ushort num13 = instance.m_segments.m_buffer[segment4].m_endNode;
                                    vector6 = instance.m_segments.m_buffer[segment4].m_startDirection;
                                    vector7 = instance.m_segments.m_buffer[segment4].m_endDirection;
                                    if (startNodeID != startNode2)
                                    {
                                        ushort num14 = startNode2;
                                        startNode2 = num13;
                                        num13 = num14;
                                        Vector3 vector8 = vector6;
                                        vector6 = vector7;
                                        vector7 = vector8;
                                    }
                                    netInfo2 = instance.m_segments.m_buffer[segment4].Info;
                                    vector9 = instance.m_nodes.m_buffer[num13].m_position;
                                    goto IL_082e;
                                }
                            IL_082e:
                                if ((object)netInfo2 != null && info.m_clipSegmentEnds == netInfo2.m_clipSegmentEnds)
                                {
                                    if (netInfo2.m_netAI.GetSnapElevation() > info.m_netAI.GetSnapElevation())
                                    {
                                        float num15 = 0.01f - Mathf.Min(info.m_maxTurnAngleCos, netInfo2.m_maxTurnAngleCos);
                                        float num16 = vector6.x * startDir.x + vector6.z * startDir.z;
                                        if ((info.m_vehicleTypes & netInfo2.m_vehicleTypes) == VehicleInfo.VehicleType.None || num16 >= num15)
                                        {
                                            break;
                                        }
                                    }
                                    if (vector6.z * cornerDirection.x - vector6.x * cornerDirection.z > 0f == leftSide)
                                    {
                                        Bezier3 bezier5 = default(Bezier3);
                                        float num17 = Mathf.Max(a, netInfo2.m_halfWidth);
                                        if (!leftSide)
                                        {
                                            num17 = 0f - num17;
                                        }
                                        vector = Vector3.Cross(vector6, Vector3.up).normalized;
                                        bezier5.a = startPos - vector * num17;
                                        vector = Vector3.Cross(vector7, Vector3.up).normalized;
                                        bezier5.d = vector9 + vector * num17;
                                        NetSegment.CalculateMiddlePoints(bezier5.a, vector6, bezier5.d, vector7, false, false, out bezier5.b, out bezier5.c);
                                        Bezier2 b2 = Bezier2.XZ(bezier5);
                                        if (bezier3.Intersect(b2, out float t, out float t2, 6))
                                        {
                                            num6 = Mathf.Max(num6, t);
                                        }
                                        else if (bezier3.Intersect(b2.a, b2.a - VectorUtils.XZ(vector6) * 16f, out t, out t2, 6))
                                        {
                                            num6 = Mathf.Max(num6, t);
                                        }
                                        else if (b2.Intersect(bezier3.d + (bezier3.d - bezier4.d) * 0.01f, bezier4.d, out t, out t2, 6))
                                        {
                                            num6 = Mathf.Max(num6, 1f);
                                        }
                                        float num18 = cornerDirection.x * vector6.x + cornerDirection.z * vector6.z;
                                        if (num18 >= -0.75f)
                                        {
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        Bezier3 bezier6 = default(Bezier3);
                                        float num19 = cornerDirection.x * vector6.x + cornerDirection.z * vector6.z;
                                        if (num19 >= 0f)
                                        {
                                            vector6.x -= cornerDirection.x * num19 * 2f;
                                            vector6.z -= cornerDirection.z * num19 * 2f;
                                        }
                                        float num20 = Mathf.Max(a, netInfo2.m_halfWidth);
                                        if (!leftSide)
                                        {
                                            num20 = 0f - num20;
                                        }
                                        vector = Vector3.Cross(vector6, Vector3.up).normalized;
                                        bezier6.a = startPos + vector * num20;
                                        vector = Vector3.Cross(vector7, Vector3.up).normalized;
                                        bezier6.d = vector9 - vector * num20;
                                        NetSegment.CalculateMiddlePoints(bezier6.a, vector6, bezier6.d, vector7, false, false, out bezier6.b, out bezier6.c);
                                        Bezier2 b3 = Bezier2.XZ(bezier6);
                                        if (bezier4.Intersect(b3, out float t3, out float t4, 6))
                                        {
                                            num7 = Mathf.Max(num7, t3);
                                        }
                                        else if (bezier4.Intersect(b3.a, b3.a - VectorUtils.XZ(vector6) * 16f, out t3, out t4, 6))
                                        {
                                            num7 = Mathf.Max(num7, t3);
                                        }
                                        else if (b3.Intersect(bezier3.d, bezier4.d + (bezier4.d - bezier3.d) * 0.01f, out t3, out t4, 6))
                                        {
                                            num7 = Mathf.Max(num7, 1f);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    if ((flags & NetNode.Flags.Junction) != 0)
                    {
                        if (!flag)
                        {
                            num6 = Mathf.Max(num6, num7);
                        }
                    }
                    else if ((flags & NetNode.Flags.Bend) != 0 && !flag)
                    {
                        num6 = Mathf.Max(num6, num7);
                    }
                    if ((flags & NetNode.Flags.Outside) != 0)
                    {
                        float num21 = 8640f;
                        Vector2 vector10 = new Vector2(0f - num21, 0f - num21);
                        Vector2 vector11 = new Vector2(0f - num21, num21);
                        Vector2 vector12 = new Vector2(num21, num21);
                        Vector2 vector13 = new Vector2(num21, 0f - num21);
                        if (bezier3.Intersect(vector10, vector11, out float t5, out float t6, 6))
                        {
                            num6 = Mathf.Max(num6, t5);
                        }
                        if (bezier3.Intersect(vector11, vector12, out t5, out t6, 6))
                        {
                            num6 = Mathf.Max(num6, t5);
                        }
                        if (bezier3.Intersect(vector12, vector13, out t5, out t6, 6))
                        {
                            num6 = Mathf.Max(num6, t5);
                        }
                        if (bezier3.Intersect(vector13, vector10, out t5, out t6, 6))
                        {
                            num6 = Mathf.Max(num6, t5);
                        }
                        num6 = Mathf.Clamp01(num6);
                    }
                    else
                    {
                        if (num6 < 0f)
                        {
                            num6 = ((!(info.m_halfWidth < 4f)) ? bezier3.Travel(0f, 8f) : 0f);
                        }
                        float num22 = info.m_minCornerOffset;
                        // NON-STOCK CODE STARTS
                        num22 = minCornerOffset;
                        // NON-STOCK CODE END
                        if ((flags & (NetNode.Flags.AsymForward | NetNode.Flags.AsymBackward)) != 0)
                        {
                            num22 = Mathf.Max(num22, 8f);
                        }
                        num6 = Mathf.Clamp01(num6);
                        float num23 = VectorUtils.LengthXZ(bezier.Position(num6) - bezier.a);
                        num6 = bezier3.Travel(num6, Mathf.Max(num22 - num23, 2f));
                        if (info.m_straightSegmentEnds)
                        {
                            if (num7 < 0f)
                            {
                                num7 = ((!(info.m_halfWidth < 4f)) ? bezier4.Travel(0f, 8f) : 0f);
                            }
                            num7 = Mathf.Clamp01(num7);
                            num23 = VectorUtils.LengthXZ(bezier2.Position(num7) - bezier2.a);
                            // NON-STOCK CODE STARTS
                            num7 = bezier4.Travel(num7, Mathf.Max(minCornerOffset - num23, 2f));
                            // NON-STOCK CODE END
                            num6 = Mathf.Max(num6, num7);
                        }
                    }
                    float y = cornerDirection.y;
                    cornerDirection = bezier.Tangent(num6);
                    cornerDirection.y = 0f;
                    cornerDirection.Normalize();
                    if (!info.m_flatJunctions)
                    {
                        cornerDirection.y = y;
                    }
                    cornerPos = bezier.Position(num6);
                    cornerPos.y = startPos.y;
                }
            }
            else if (((flags & NetNode.Flags.Junction) != 0 && minCornerOffset >= 0.01f) || ((flags & NetNode.Flags.Junction) != 0 && minCornerOffset != 0))
            {
                vector = Vector3.Cross(endDir, Vector3.up).normalized;
                bezier.d = endPos - vector * d;
                bezier2.d = endPos + vector * d;
                NetSegment.CalculateMiddlePoints(bezier.a, cornerDirection, bezier.d, endDir, false, false, out bezier.b, out bezier.c);
                NetSegment.CalculateMiddlePoints(bezier2.a, cornerDirection, bezier2.d, endDir, false, false, out bezier2.b, out bezier2.c);
                Bezier2 bezier7 = Bezier2.XZ(bezier);
                Bezier2 bezier8 = Bezier2.XZ(bezier2);
                float value = (!(info.m_halfWidth < 4f)) ? bezier7.Travel(0f, 8f) : 0f;
                value = Mathf.Clamp01(value);
                float num24 = VectorUtils.LengthXZ(bezier.Position(value) - bezier.a);
                // NON-STOCK CODE STARTS
                value = bezier7.Travel(value, Mathf.Max(minCornerOffset - num24, 2f));
                // NON-STOCK CODE END
                if (info.m_straightSegmentEnds)
                {
                    float value2 = (!(info.m_halfWidth < 4f)) ? bezier8.Travel(0f, 8f) : 0f;
                    value2 = Mathf.Clamp01(value2);
                    num24 = VectorUtils.LengthXZ(bezier2.Position(value2) - bezier2.a);
                    // NON-STOCK CODE STARTS
                    value2 = bezier7.Travel(value2, Mathf.Max(minCornerOffset - num24, 2f));
                    // NON-STOCK CODE END
                    value = Mathf.Max(value, value2);
                }
                float y2 = cornerDirection.y;
                cornerDirection = bezier.Tangent(value);
                cornerDirection.y = 0f;
                cornerDirection.Normalize();
                if (!info.m_flatJunctions)
                {
                    cornerDirection.y = y2;
                }
                cornerPos = bezier.Position(value);
                cornerPos.y = startPos.y;
            }
            if (heightOffset && startNodeID != 0)
            {
                cornerPos.y += (float)(int)instance.m_nodes.m_buffer[startNodeID].m_heightOffset * 0.015625f;
            }
        }
    }
}

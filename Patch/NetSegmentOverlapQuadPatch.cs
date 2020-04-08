using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentOverlapQuadPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Instance);
        }
        public static bool Prefix(ref NetSegment __instance, ushort segmentID, Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType)
        {
            if ((__instance.m_flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) != NetSegment.Flags.Created)
            {
                return false;
            }
            NetInfo info = __instance.Info;
            if (!info.m_canCollide)
            {
                return false;
            }
            float collisionHalfWidth = info.m_netAI.GetCollisionHalfWidth();
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            Bezier3 bezier = default(Bezier3);
            bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].m_position;
            bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].m_position;
            // NON-STOCK CODE STARTS
            if (CSURUtil.IsCSUROffset(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info))
            {
                var width = (Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info.m_halfWidth + Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info.m_pavementWidth) / 2f;
                bool lht = false;
                Vector3 direction = __instance.m_startDirection;
                if ((__instance.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                bezier.a = bezier.a + (lht ? -width : width) * normal;
            }
            if (CSURUtil.IsCSUROffset(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].Info))
            {
                bool lht = false;
                var width = (Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].Info.m_halfWidth + Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].Info.m_pavementWidth) / 2f;
                Vector3 direction = -__instance.m_endDirection;
                if ((__instance.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                bezier.d = bezier.d + (lht ? -width : width) * normal;
            }
            // NON-STOCK CODE ENDS
            NetSegment.CalculateMiddlePoints(bezier.a, __instance.m_startDirection, bezier.d, __instance.m_endDirection, true, true, out bezier.b, out bezier.c);
            // NON-STOCK CODE STARTS
            float laneOffsetS = 0;
            float startOffsetS = 0;
            float endOffsetS = 0;
            bool IsCSURSLaneS = CSURUtil.IsCSURSLane(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info, ref laneOffsetS, ref startOffsetS, ref endOffsetS);
            float laneOffsetE = 0;
            float startOffsetE = 0;
            float endOffsetE = 0;
            bool IsCSURSLaneE = CSURUtil.IsCSURSLane(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].Info, ref laneOffsetE, ref startOffsetE, ref endOffsetE);
            Vector3 newBezierA1 = Vector3.zero;
            Vector3 newBezierB1 = Vector3.zero;
            Vector3 newBezierC1 = Vector3.zero;
            Vector3 newBezierD1 = Vector3.zero;
            if (IsCSURSLaneS || IsCSURSLaneE)
            {
                float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info);
                float otherLaneNum = CSURUtil.CountCSURSOtherLanes(Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].Info);
                float laneNum = vehicleLaneNum + otherLaneNum;
                startOffsetS = startOffsetS * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                endOffsetE = endOffsetE * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                if ((__instance.m_flags & NetSegment.Flags.Invert) != 0)
                {
                    startOffsetS = -startOffsetS;
                    endOffsetE = -endOffsetE;
                }

                if (!IsCSURSLaneS) { startOffsetS = 0; }
                if (!IsCSURSLaneE) { endOffsetE = 0; }
                //EG: before patch: point1-point4 is 1.5*3.75
                //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                newBezierA1 = bezier.a + (new Vector3(__instance.m_startDirection.z, 0, -__instance.m_startDirection.x).normalized) * (startOffsetS);
                Vector3 newBezierBDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.333f));
                newBezierB1 = bezier.b + (new Vector3(newBezierBDir.z, 0, -newBezierBDir.x).normalized) * (startOffsetS * 0.667f + endOffsetE * 0.333f);
                Vector3 newBezierCDir = VectorUtils.NormalizeXZ(bezier.Tangent(0.667f));
                newBezierC1 = bezier.c + (new Vector3(newBezierCDir.z, 0, -newBezierCDir.x).normalized) * (startOffsetS * 0.333f + endOffsetE * 0.667f);
                newBezierD1 = bezier.d + (new Vector3(-__instance.m_endDirection.z, 0, __instance.m_endDirection.x).normalized) * (endOffsetE);

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
                if (info.m_clipSegmentEnds && endRadius != 0f && start == 0f && (Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_startNode].m_flags & (NetNode.Flags.End | NetNode.Flags.Bend | NetNode.Flags.Junction)) != 0)
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
                if (info.m_clipSegmentEnds && endRadius != 0f && end == 1f && (Singleton<NetManager>.instance.m_nodes.m_buffer[__instance.m_endNode].m_flags & (NetNode.Flags.End | NetNode.Flags.Bend | NetNode.Flags.Junction)) != 0)
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
    }
}

using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.CustomData
{
    public static class CustomNetNode
    {
        public static bool RayCast(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
            if (CSUROffset.IsCSUROffset(info.m_netAI.m_info))
            {
                return RayCastNodeMasked(ref node, ray, snapElevation, false, out t, out priority);
            }
            // NON-STOCK CODE ENDS
            if ((node.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Outside)) == NetNode.Flags.None)
            {
                float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
                float t2;
                if (info.m_netAI.IsUnderground())
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
                }
                else
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
                }
                float collisionHalfWidth = Mathf.Max(3f, info.m_netAI.GetCollisionHalfWidth());
                float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            }
            t = 0f;
            priority = 0f;
            return false;
        }
        //to detour move it
        public static bool MoveItRayCastNode(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
            if (CSUROffset.IsCSUROffset(info.m_netAI.m_info))
            {
                return RayCastNodeMasked(ref node, ray, snapElevation, false, out t, out priority);
            }
            // NON-STOCK CODE ENDS
            //if ((node.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Outside)) == NetNode.Flags.None)
            //{
                float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
                float t2;
                if (info.m_netAI.IsUnderground())
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
                }
                else
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
                }
                float collisionHalfWidth = Mathf.Max(3f, info.m_netAI.GetCollisionHalfWidth());
                float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            //}
            t = 0f;
            priority = 0f;
            return false;
        }
        
        public static bool CheckNodeEq(ushort node1, NetNode node2)
        {
            if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_buildIndex == node2.m_buildIndex)
            {
                if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_position == node2.m_position)
                {
                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_nextGridNode == node2.m_nextGridNode)
                    {
                        if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_flags == node2.m_flags)
                        {
                            if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_nextLaneNode == node2.m_nextLaneNode)
                            {
                                if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_elevation == node2.m_elevation)
                                {
                                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment0 == node2.m_segment0)
                                    {
                                        if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment1 == node2.m_segment1)
                                        {
                                            if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment2 == node2.m_segment2)
                                            {
                                                if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment3 == node2.m_segment3)
                                                {
                                                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment4 == node2.m_segment4)
                                                    {
                                                        if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment5 == node2.m_segment5)
                                                        {
                                                            if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment6 == node2.m_segment6)
                                                            {
                                                                if (Singleton<NetManager>.instance.m_nodes.m_buffer[node1].m_segment7 == node2.m_segment7)
                                                                {
                                                                    return true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static NetSegment GetSameInfoSegment(NetNode node)
        {
            for (int i = 0; i < 8; i ++)
            {
                if (node.GetSegment(i) != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[node.GetSegment(i)].Info == node.Info)
                    {
                        return Singleton<NetManager>.instance.m_segments.m_buffer[node.GetSegment(i)];
                    }
                }
            }
            return default(NetSegment);
        }

        public static bool RayCastNodeMasked(ref NetNode node, Segment3 ray, float snapElevation, bool bothSides, out float t, out float priority)
        {
            bool lht = false;
            //if (SimulationManager.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True) lht = true;
            NetInfo info = node.Info;
            float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
            float t2;
            if (info.m_netAI.IsUnderground())
            {
                t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
            }
            else
            {
                t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
            }
            float collisionHalfWidth = Mathf.Max(3f, info.m_halfWidth);
            float maskHalfWidth = Mathf.Min(collisionHalfWidth - 1.5f, info.m_pavementWidth);
            float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
            float num2m = Mathf.Lerp(info.GetMinNodeDistance(), maskHalfWidth, t2);
            float num2delta = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth - maskHalfWidth, t2);
            if (node.CountSegments() != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;
                NetSegment mysegment = GetSameInfoSegment(node);
                Vector3 direction = CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                Debug.Log(direction);
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                Vector3 trueNodeCenter = node.m_position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                Debug.Log($"num2: {num2}, num2m: {num2m}");
                Debug.Log($"node: {node.m_position}, center: {trueNodeCenter}");
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), trueNodeCenter);
                    if (num3 < num2delta)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }

            }
            else
            {
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            }
            t = 0f;
            priority = 0f;
            return false;
        }
    }
}
using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using UnityEngine;

namespace CSURToolBox.CustomData
{
    public static class CustomNetNode
    {
        //to detour move it
        public static bool MoveItRayCastNode(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(info.m_netAI.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(info.m_netAI.m_info) && !IsCSURSLane)
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
                // NON-STOCK CODE STARTS
                if (IsCSURSLane)
                {
                    bool lht = false;
                    NetManager instance = Singleton<NetManager>.instance;
                    NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                    bool isStart = CSURUtil.CheckNodeEq(mysegment.m_startNode, node);
                    Vector3 direction = isStart ? mysegment.m_startDirection : -mysegment.m_endDirection;
                    //Debug.Log(direction);
                    if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                    Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                    float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(info);
                    float otherLaneNum = CSURUtil.CountCSURSOtherLanes(info);
                    float laneNum = otherLaneNum + vehicleLaneNum;
                    startOffset = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                    endOffset = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                    var Offset = isStart ? startOffset : endOffset;
                    Vector3 trueNodeCenter = node.m_position + (lht ? -Offset : Offset) * normal;
                    num3 = Vector3.Distance(ray.Position(t), trueNodeCenter);
                }
                // NON-STOCK CODE ENDS

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
                NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                //Debug.Log(direction);
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                Vector3 trueNodeCenter = node.m_position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                //Debug.Log($"num2: {num2}, num2m: {num2m}");
                //Debug.Log($"node: {node.m_position}, center: {trueNodeCenter}");
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
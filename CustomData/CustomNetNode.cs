using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace CSURToolBox.CustomData
{
    public static class CustomNetNode
    {
        public static bool RayCast(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
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

        public static void UpdateBuilding(ref NetNode node, ushort nodeID, BuildingInfo newBuilding, float heightOffset)
        {
            float num = 0f;
            if ((object)newBuilding != null)
            {
                NetInfo info = node.Info;
                if ((object)info != null)
                {
                    num = info.m_netAI.GetNodeBuildingAngle(nodeID, ref node);
                }
            }
            BuildingInfo buildingInfo = null;
            if (node.m_building != 0)
            {
                buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[node.m_building].Info;
            }
            if ((object)newBuilding != buildingInfo)
            {
                if (node.m_building != 0)
                {
                    Singleton<BuildingManager>.instance.ReleaseBuilding(node.m_building);
                    node.m_building = 0;
                }
                if ((object)newBuilding != null)
                {
                    Vector3 position = node.m_position;
                    position.y += heightOffset;
                    // NON-STOCK CODE STARTS
                    if (CSURUtil.IsCSUROffset(node.Info))
                    {
                        float laneOffset = 0;
                        float startOffset = 0;
                        float endOffset = 0;
                        if (CSURUtil.IsCSURSLane(node.Info, ref laneOffset, ref startOffset, ref endOffset))
                        {
                            bool lht = false;
                            if (node.CountSegments() != 0)
                            {
                                float collisionHalfWidth = 0;
                                float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(node.Info);
                                float otherLaneNum = CSURUtil.CountCSURSOtherLanes(node.Info);
                                float laneNum = otherLaneNum + vehicleLaneNum;
                                if (CSURUtil.isStartNode(nodeID))
                                {
                                    collisionHalfWidth = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                                }
                                else
                                {
                                    collisionHalfWidth = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                                }
                                NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                                Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                                // normal to the right hand side
                                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                                position = position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                            }
                        }
                        else
                        {
                            bool lht = false;
                            if (node.CountSegments() != 0)
                            {
                                float collisionHalfWidth = Mathf.Max(3f, (node.Info.m_halfWidth + node.Info.m_pavementWidth) / 2f);
                                NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                                Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                                // normal to the right hand side
                                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                                position = position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                            }
                        }
                    }
                    // NON-STOCK CODE ENDS
                    num *= 6.28318548f;
                    if ((object)buildingInfo != null || TestNodeBuilding(nodeID, newBuilding, position, num))
                    {
                        Randomizer randomizer = new Randomizer(nodeID);
                        if (Singleton<BuildingManager>.instance.CreateBuilding(out node.m_building, ref randomizer, newBuilding, position, num, 0, node.m_buildIndex + 1))
                        {
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[node.m_building].m_flags |= (Building.Flags.Untouchable | Building.Flags.FixedHeight);
                        }
                    }
                }
            }
            else if (node.m_building != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Vector3 position2 = node.m_position;
                position2.y += heightOffset;
                // NON-STOCK CODE STARTS
                if (CSURUtil.IsCSUROffset(node.Info))
                {
                    float laneOffset = 0;
                    float startOffset = 0;
                    float endOffset = 0;
                    if (CSURUtil.IsCSURSLane(node.Info, ref laneOffset, ref startOffset, ref endOffset))
                    {
                        bool lht = false;
                        if (node.CountSegments() != 0)
                        {
                            float collisionHalfWidth = 0;
                            float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(node.Info);
                            float otherLaneNum = CSURUtil.CountCSURSOtherLanes(node.Info);
                            float laneNum = otherLaneNum + vehicleLaneNum;
                            if (CSURUtil.isStartNode(nodeID))
                            {
                                collisionHalfWidth = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                            }
                            else
                            {
                                collisionHalfWidth = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                            }
                            NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                            Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                            if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                            // normal to the right hand side
                            Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                            position2 = position2 + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                        }
                    }
                    else
                    {
                        bool lht = false;
                        if (node.CountSegments() != 0)
                        {
                            float collisionHalfWidth = Mathf.Max(3f, (node.Info.m_halfWidth + node.Info.m_pavementWidth) / 2f);
                            NetSegment mysegment = CSURUtil.GetSameInfoSegment(node);
                            Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                            if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                            // normal to the right hand side
                            Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                            position2 = position2 + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                        }
                    }
                }
                // NON-STOCK CODE ENDS
                num *= 6.28318548f;
                // NON-STOCK CODE STARTS
                if (CSURUtil.IsCSUROffset(node.Info) && (instance.m_buildings.m_buffer[node.m_building].m_position != position2 || instance.m_buildings.m_buffer[node.m_building].m_angle != num))
                {
                    RemoveFromGrid(node.m_building, ref instance.m_buildings.m_buffer[node.m_building]);
                    instance.m_buildings.m_buffer[node.m_building].m_position = position2;
                    instance.m_buildings.m_buffer[node.m_building].m_angle = num;
                    AddToGrid(node.m_building, ref instance.m_buildings.m_buffer[node.m_building]);
                    instance.m_buildings.m_buffer[node.m_building].CalculateBuilding(node.m_building);
                    Singleton<BuildingManager>.instance.UpdateBuildingRenderer(node.m_building, true);
                }
                else
                {
                    if (instance.m_buildings.m_buffer[node.m_building].m_position.y != position2.y || instance.m_buildings.m_buffer[node.m_building].m_angle != num)
                    {
                        instance.m_buildings.m_buffer[node.m_building].m_position.y = position2.y;
                        instance.m_buildings.m_buffer[node.m_building].m_angle = num;
                        instance.UpdateBuilding(node.m_building);
                    }
                }
                               
                // NON-STOCK CODE ENDS
            }
        }

        private static void RemoveFromGrid(ushort building, ref Building data)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = data.Info;
            int num = Mathf.Clamp((int)(data.m_position.x / 64f + 135f), 0, 269);
            int num2 = Mathf.Clamp((int)(data.m_position.z / 64f + 135f), 0, 269);
            int num3 = num2 * 270 + num;
            while (!Monitor.TryEnter(instance.m_buildingGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                ushort num4 = 0;
                ushort num5 = instance.m_buildingGrid[num3];
                int num6 = 0;
                while (num5 != 0)
                {
                    if (num5 == building)
                    {
                        if (num4 == 0)
                        {
                            instance.m_buildingGrid[num3] = data.m_nextGridBuilding;
                            break;
                        }
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num4].m_nextGridBuilding = data.m_nextGridBuilding;
                        break;
                    }
                    else
                    {
                        num4 = num5;
                        num5 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num5].m_nextGridBuilding;
                        if (++num6 > 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                data.m_nextGridBuilding = 0;
            }
            finally
            {
                Monitor.Exit(instance.m_buildingGrid);
            }
            if (info != null)
            {
                Singleton<RenderManager>.instance.UpdateGroup(num * 45 / 270, num2 * 45 / 270, info.m_prefabDataLayer);
            }
        }

        private static void AddToGrid(ushort building, ref Building data)
        {
            int num = Mathf.Clamp((int)(data.m_position.x / 64f + 135f), 0, 269);
            int num2 = Mathf.Clamp((int)(data.m_position.z / 64f + 135f), 0, 269) * 270 + num;
            while (!Monitor.TryEnter(Singleton<BuildingManager>.instance.m_buildingGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_nextGridBuilding = Singleton<BuildingManager>.instance.m_buildingGrid[num2];
                Singleton<BuildingManager>.instance.m_buildingGrid[num2] = building;
            }
            finally
            {
                Monitor.Exit(Singleton<BuildingManager>.instance.m_buildingGrid);
            }
        }
        public static bool TestNodeBuilding(ushort nodeID, BuildingInfo info, Vector3 position, float angle)
        {
            Vector2 a = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 a2 = new Vector3(a.y, 0f - a.x);
            if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside || info.m_placementMode == BuildingInfo.PlacementMode.PathsideOrGround)
            {
                a *= (float)info.m_cellWidth * 4f - 0.8f;
                a2 *= (float)info.m_cellLength * 4f - 0.8f;
            }
            else
            {
                a *= (float)info.m_cellWidth * 4f;
                a2 *= (float)info.m_cellLength * 4f;
            }
            if (info.m_circular)
            {
                a *= 0.7f;
                a2 *= 0.7f;
            }
            ItemClass.CollisionType collisionType = info.m_buildingAI.GetCollisionType();
            Vector2 a3 = VectorUtils.XZ(position);
            Quad2 quad = default(Quad2);
            quad.a = a3 - a - a2;
            quad.b = a3 - a + a2;
            quad.c = a3 + a + a2;
            quad.d = a3 + a - a2;
            float minY = Mathf.Min(position.y, Singleton<TerrainManager>.instance.SampleRawHeightSmooth(position));
            float maxY = position.y + info.m_generatedInfo.m_size.y;
            if (collisionType == ItemClass.CollisionType.Elevated)
            {
                minY = position.y + info.m_generatedInfo.m_min.y;
            }
            if (Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, nodeID, 0, 0, null))
            {
                return false;
            }
            if (Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, 0, nodeID, 0, null))
            {
                return false;
            }
            return true;
        }
    }
}
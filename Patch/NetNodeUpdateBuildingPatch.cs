using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetNodeUpdateBuildingPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetNode).GetMethod("UpdateBuilding", BindingFlags.Public | BindingFlags.Instance);
        }
        public static bool Prefix(ref NetNode __instance, ushort nodeID, BuildingInfo newBuilding, float heightOffset)
        {
            float num = 0f;
            if ((object)newBuilding != null)
            {
                NetInfo info = __instance.Info;
                if ((object)info != null)
                {
                    num = info.m_netAI.GetNodeBuildingAngle(nodeID, ref __instance);
                }
            }
            BuildingInfo buildingInfo = null;
            if (__instance.m_building != 0)
            {
                buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[__instance.m_building].Info;
            }
            if ((object)newBuilding != buildingInfo)
            {
                if (__instance.m_building != 0)
                {
                    Singleton<BuildingManager>.instance.ReleaseBuilding(__instance.m_building);
                    __instance.m_building = 0;
                }
                if ((object)newBuilding != null)
                {
                    Vector3 position = __instance.m_position;
                    position.y += heightOffset;
                    // NON-STOCK CODE STARTS
                    if (CSURUtil.IsCSUROffset(__instance.Info))
                    {
                        float laneOffset = 0;
                        float startOffset = 0;
                        float endOffset = 0;
                        if (CSURUtil.IsCSURSLane(__instance.Info, ref laneOffset, ref startOffset, ref endOffset))
                        {
                            bool lht = false;
                            if (__instance.CountSegments() != 0)
                            {
                                float collisionHalfWidth = 0;
                                float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(__instance.Info);
                                float otherLaneNum = CSURUtil.CountCSURSOtherLanes(__instance.Info);
                                float laneNum = otherLaneNum + vehicleLaneNum;
                                if (CSURUtil.isStartNode(nodeID))
                                {
                                    collisionHalfWidth = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                                }
                                else
                                {
                                    collisionHalfWidth = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                                }
                                NetSegment mysegment = CSURUtil.GetSameInfoSegment(__instance);
                                Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, __instance) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                                // normal to the right hand side
                                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                                position = position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                            }
                        }
                        else
                        {
                            bool lht = false;
                            if (__instance.CountSegments() != 0)
                            {
                                float collisionHalfWidth = Mathf.Max(3f, (__instance.Info.m_halfWidth + __instance.Info.m_pavementWidth) / 2f);
                                NetSegment mysegment = CSURUtil.GetSameInfoSegment(__instance);
                                Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, __instance) ? mysegment.m_startDirection : -mysegment.m_endDirection;
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
                        if (Singleton<BuildingManager>.instance.CreateBuilding(out __instance.m_building, ref randomizer, newBuilding, position, num, 0, __instance.m_buildIndex + 1))
                        {
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[__instance.m_building].m_flags |= (Building.Flags.Untouchable | Building.Flags.FixedHeight);
                        }
                    }
                }
            }
            else if (__instance.m_building != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Vector3 position2 = __instance.m_position;
                position2.y += heightOffset;
                // NON-STOCK CODE STARTS
                if (CSURUtil.IsCSUROffset(__instance.Info))
                {
                    float laneOffset = 0;
                    float startOffset = 0;
                    float endOffset = 0;
                    if (CSURUtil.IsCSURSLane(__instance.Info, ref laneOffset, ref startOffset, ref endOffset))
                    {
                        bool lht = false;
                        if (__instance.CountSegments() != 0)
                        {
                            float collisionHalfWidth = 0;
                            float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(__instance.Info);
                            float otherLaneNum = CSURUtil.CountCSURSOtherLanes(__instance.Info);
                            float laneNum = otherLaneNum + vehicleLaneNum;
                            if (CSURUtil.isStartNode(nodeID))
                            {
                                collisionHalfWidth = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                            }
                            else
                            {
                                collisionHalfWidth = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                            }
                            NetSegment mysegment = CSURUtil.GetSameInfoSegment(__instance);
                            Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, __instance) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                            if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                            // normal to the right hand side
                            Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                            position2 = position2 + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                        }
                    }
                    else
                    {
                        bool lht = false;
                        if (__instance.CountSegments() != 0)
                        {
                            float collisionHalfWidth = Mathf.Max(3f, (__instance.Info.m_halfWidth + __instance.Info.m_pavementWidth) / 2f);
                            NetSegment mysegment = CSURUtil.GetSameInfoSegment(__instance);
                            Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, __instance) ? mysegment.m_startDirection : -mysegment.m_endDirection;
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
                if (CSURUtil.IsCSUROffset(__instance.Info) && (instance.m_buildings.m_buffer[__instance.m_building].m_position != position2 || instance.m_buildings.m_buffer[__instance.m_building].m_angle != num))
                {
                    RemoveFromGrid(__instance.m_building, ref instance.m_buildings.m_buffer[__instance.m_building]);
                    instance.m_buildings.m_buffer[__instance.m_building].m_position = position2;
                    instance.m_buildings.m_buffer[__instance.m_building].m_angle = num;
                    AddToGrid(__instance.m_building, ref instance.m_buildings.m_buffer[__instance.m_building]);
                    instance.m_buildings.m_buffer[__instance.m_building].CalculateBuilding(__instance.m_building);
                    Singleton<BuildingManager>.instance.UpdateBuildingRenderer(__instance.m_building, true);
                }
                else
                {
                    if (instance.m_buildings.m_buffer[__instance.m_building].m_position.y != position2.y || instance.m_buildings.m_buffer[__instance.m_building].m_angle != num)
                    {
                        instance.m_buildings.m_buffer[__instance.m_building].m_position.y = position2.y;
                        instance.m_buildings.m_buffer[__instance.m_building].m_angle = num;
                        instance.UpdateBuilding(__instance.m_building);
                    }
                }
                // NON-STOCK CODE ENDS
            }
            return false;
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

using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using Harmony;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch(typeof(NetSegment), "UpdateLanes")]
    public static class NetSegmentUpdateLanesPatch
    {
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
                    float laneOffset = 0;
                    int startOffsetIdex = 0;
                    if ((m_info.m_netAI is RoadAI) || (m_info.m_netAI is RoadBridgeAI) || (m_info.m_netAI is RoadTunnelAI))
                    {
                        if (CSUROffset.IsCSURLaneOffset(m_info, ref laneOffset, ref startOffsetIdex))
                        {
                            for (int i = 0; i < m_info.m_lanes.Length; i++)
                            {
                                if (firstLane == 0)
                                {
                                    break;
                                }
                                NetInfo.Lane lane = m_info.m_lanes[i];
                                if (lane.m_laneType.IsFlagSet(NetInfo.LaneType.Pedestrian))
                                {
                                    laneOffset *= 3.75f;
                                }
                                else if (lane.m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle))
                                {
                                    if (lane.m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                                    {
                                        laneOffset *= 3.75f;
                                    }
                                    else if (lane.m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Bicycle))
                                    {
                                        laneOffset *= 2.75f;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                //TODO:Get CSURLaneIndex and if greater than startOffsetIdex, we need to do offset.
                                if (CSUROffset.CSURLaneIndex(m_info, lane) >= startOffsetIdex)
                                {
                                    //EG: before patch: point1-point4 is 1.5*3.75
                                    //After patch, point1-point4 is (1 1.3333 1.6667 2)*3.75
                                    var bezier = instance.m_lanes.m_buffer[firstLane].m_bezier;
                                    Vector3 newBezierA = bezier.Position(0) + (new Vector3(-bezier.Tangent(0).z, 0, bezier.Tangent(0).x).normalized) * (laneOffset * 0.5f);
                                    NetSegment.CalculateMiddlePoints(bezier.Position(0), VectorUtils.NormalizeXZ(bezier.Tangent(0)), bezier.Position(1), -VectorUtils.NormalizeXZ(bezier.Tangent(1)), true, true, out Vector3 middlePos, out Vector3 middlePos2);
                                    Vector3 newBezierB = middlePos + (new Vector3(-bezier.Tangent(0.3333f).z, 0, bezier.Tangent(0.3333f).x).normalized) * (laneOffset * 0.1667f);
                                    Vector3 newBezierC = middlePos2 + (new Vector3(bezier.Tangent(0.6667f).z, 0, -bezier.Tangent(0.6667f).x).normalized) * (laneOffset * 0.1667f);
                                    Vector3 newBezierD = bezier.Position(1) + (new Vector3(bezier.Tangent(1).z, 0, -bezier.Tangent(1).x).normalized) * (laneOffset * 0.5f);
                                    instance.m_lanes.m_buffer[firstLane].m_bezier = new Bezier3(newBezierA, newBezierB, newBezierC, newBezierD);
                                }
                                firstLane = instance.m_lanes.m_buffer[firstLane].m_nextLane;
                            }
                        }
                        //Patch End
                    }
                }
            }
        }
    }
}

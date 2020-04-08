using ColossalFramework;
using System;
using System.Reflection;
using CSURToolBox.Util;
using HarmonyLib;
using CSURToolBox.UI;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentGetLeftAndRightLanesPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetSegment).GetMethod("GetLeftAndRightLanes", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Postfix(ushort nodeID, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, ref NetSegment __instance, ref int leftIndex, ref int rightIndex, ref uint leftLane, ref uint rightLane)
        {
            if (OptionUI.noJunction)
            {
                if (CSURUtil.IsCSURNoJunction(__instance.Info))
                {
                    if (leftIndex == -1 && rightIndex == -1 && leftLane == 0 && rightLane == 0)
                    {
                        int debugPedestrianCount = 0;
                        NetManager instance = Singleton<NetManager>.instance;
                        NetInfo info = __instance.Info;
                        int num = info.m_lanes.Length;
                        uint num2 = __instance.m_lanes;
                        int num10 = 0;
                        while (num10 < num && num2 != 0u)
                        {
                            NetInfo.Lane lane2 = info.m_lanes[num10];
                            if (info.m_lanes[num10].m_laneType.IsFlagSet(NetInfo.LaneType.Pedestrian))
                            {
                                debugPedestrianCount++;
                                if (lane2.m_position > 0)
                                {
                                    //DebugLog.LogToFileOnly($"Fix this case for XR, only one Pedestrian lane {__instance.Info.name}");
                                    //XR case
                                    leftIndex = num10;
                                    leftLane = num2;
                                }
                                else
                                {
                                    //DebugLog.LogToFileOnly($"Fix this case for XL, only one Pedestrian lane {__instance.Info.name}");
                                    //XL case??
                                    rightIndex = num10;
                                    rightLane = num2;
                                }
                            }
                            num2 = instance.m_lanes.m_buffer[(int)((UIntPtr)num2)].m_nextLane;
                            num10++;
                        }

                        if (debugPedestrianCount == 1)
                        {
                            //DebugLog.LogToFileOnly($"Fix this case, only one Pedestrian lane {__instance.Info.name}");
                        }

                        if (nodeID == __instance.m_startNode != ((__instance.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None))
                        {
                            int num12 = leftIndex;
                            leftIndex = rightIndex;
                            rightIndex = num12;
                            uint num13 = leftLane;
                            leftLane = rightLane;
                            rightLane = num13;
                        }
                    }
                }
            }
        }
    }
}

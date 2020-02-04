using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Collections;
using CSURToolBox.Util;
using System.Reflection;
using System;

namespace CSURToolBox.UI
{
    public class StayInLaneUI : UIPanel
    {
        int laterLeftClick = 0;
        bool needLaterLeftClick = false;
        public enum ToolMode
        {
            None,
            SwitchTrafficLight,
            AddPrioritySigns,
            ManualSwitch,
            TimedLightsSelectNode,
            TimedLightsShowLights,
            LaneChange,
            TimedLightsAddNode,
            TimedLightsRemoveNode,
            TimedLightsCopyLights,
            SpeedLimits,
            VehicleRestrictions,
            LaneConnector,
            JunctionRestrictions,
            ParkingRestrictions
        }
        public void OnGUI()
        {
            if ((laterLeftClick == 8) && needLaterLeftClick)
            {
                laterLeftClick = 0 ;
                MouseSimulater.LeftClick();
                needLaterLeftClick = false;
            }
            else if (needLaterLeftClick)
            {
                laterLeftClick++;
            }

            var e = Event.current;
            // Checking key presses
            if (OptionsKeymappingFunction.m_stayinlane.IsPressed(e))
            {
                Assembly TMPE = Assembly.Load("TrafficManager");
                //
                var selectedNodeId = TMPE.GetType("TrafficManager.UI.TrafficManagerTool").GetProperty("SelectedNodeId");
                var TrafficManagerTool = TMPE.CreateInstance("TrafficManager.UI.TrafficManagerTool");
                ushort node = (ushort)selectedNodeId.GetValue(TrafficManagerTool, null);
                DebugLog.LogToFileOnly("TMPE select node = " + node.ToString());
                var LaneConnectionManager = TMPE.CreateInstance("TrafficManager.Manager.Impl.LaneConnectionManager");
                var AddLaneConnection = TMPE.GetType("TrafficManager.Manager.Impl.LaneConnectionManager").GetMethod("AddLaneConnection", BindingFlags.NonPublic | BindingFlags.Instance);
                uint[] incomingLaneID = new uint[16];
                uint[] outgoingLaneID = new uint[16];
                float[] incomingLanePosition = new float[16];
                float[] outgoingLanePosition = new float[16];
                bool[] incomingStartNode = new bool[16];
                bool[] outgoingStartNode = new bool[16];
                byte incomingLaneNum = 0;
                byte outgoingLaneNum = 0;
                for (int i = 0; i < 8; i++)
                {
                    ushort segmentID = Singleton<NetManager>.instance.m_nodes.m_buffer[node].GetSegment(i);
                    if (segmentID != 0)
                    {
                        var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
                        uint firstLane = segment.m_lanes;
                        bool startNode = (segment.m_startNode == node);
                        for (int j = 0; j < segment.Info.m_lanes.Length; j++)
                        {
                            if (firstLane == 0)
                            {
                                break;
                            }
                            if (segment.Info.m_lanes[j].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && segment.Info.m_lanes[j].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                            {
                                bool isOutGoing = IsOutGoing(segmentID, startNode, segment.Info.m_lanes[j]);
                                if (isOutGoing)
                                {
                                    outgoingLaneID[outgoingLaneNum] = firstLane;
                                    if (startNode)
                                    {
                                        outgoingLanePosition[outgoingLaneNum] = segment.Info.m_lanes[j].m_position;
                                    }
                                    else
                                    {
                                        outgoingLanePosition[outgoingLaneNum] = -segment.Info.m_lanes[j].m_position;
                                    }
                                    outgoingStartNode[outgoingLaneNum] = startNode;
                                    outgoingLaneNum++;
                                }
                                else
                                {
                                    incomingLaneID[incomingLaneNum] = firstLane;
                                    if (startNode)
                                    {
                                        incomingLanePosition[incomingLaneNum] = segment.Info.m_lanes[j].m_position;
                                    }
                                    else
                                    {
                                        incomingLanePosition[incomingLaneNum] = -segment.Info.m_lanes[j].m_position;
                                    }
                                    incomingStartNode[incomingLaneNum] = startNode;
                                    incomingLaneNum++;
                                }
                            }

                            firstLane = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_nextLane;
                        }
                    }
                }

                DebugLog.LogToFileOnly("incomingLaneNum = " + incomingLaneNum.ToString() + "outgoingLaneNum = " + outgoingLaneNum.ToString());
                SortLane(ref incomingLanePosition, ref incomingLaneID, ref incomingStartNode, incomingLaneNum);
                SortLane(ref outgoingLanePosition, ref outgoingLaneID, ref outgoingStartNode, outgoingLaneNum);

                for (int i = 0; i < outgoingLaneNum; i ++)
                {
                    //AddLaneConnection(outgoingLaneID[i], incomingLaneID[i], item.StartNode);
                    DebugLog.LogToFileOnly("outgoingLaneID[i] = " + outgoingLaneID[i].ToString() + "incomingLaneID[i] = " + incomingLaneID[i].ToString() + "outgoingStartNode[i] = " + outgoingStartNode[i].ToString());
                    AddLaneConnection.Invoke(LaneConnectionManager, new object[] { outgoingLaneID[i], incomingLaneID[i], outgoingStartNode[i] });
                }

                //RefreshCurrentNodeMarkers.Invoke(LaneConnectorTool, new object[] { node });
                //Do a Refresh
                MouseSimulater.RightClick();
                needLaterLeftClick = true;
            }
        }

        internal void SortLane(ref float[] laneOffset, ref uint[] laneID, ref bool[] startNode, int laneIndex)
        {
            for (int i = 0; i < laneIndex - 1; i++)
            {
                bool isSorted = true;  //假设剩下的元素已经排序好了
                for (int j = 0; j < laneIndex - 1 - i; j++)
                {
                    if (laneOffset[j] > laneOffset[j + 1])
                    {
                        float temp = laneOffset[j];
                        laneOffset[j] = laneOffset[j + 1];
                        laneOffset[j + 1] = temp;
                        uint temp1 = laneID[j];
                        laneID[j] = laneID[j + 1];
                        laneID[j + 1] = temp1;
                        bool temp2 = startNode[j];
                        startNode[j] = startNode[j + 1];
                        startNode[j + 1] = temp2;
                        isSorted = false;  //一旦需要交换数组元素，就说明剩下的元素没有排序好
                    }
                }
                if (isSorted) break; //如果没有发生交换，说明剩下的元素已经排序好了
            }
        }

        internal bool IsOutGoing(ushort segmentId, bool startNode, NetInfo.Lane laneInfo)
        {
            NetInfo.Direction direction = ((Singleton<NetManager>.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? laneInfo.m_finalDirection : NetInfo.InvertDirection(laneInfo.m_finalDirection);
            if (startNode)
            {
                if ((direction & NetInfo.Direction.Backward) != 0)
                {
                    return true;
                }
                if ((direction & NetInfo.Direction.Forward) != 0)
                {
                    return false;
                }
            }
            else
            {
                if ((direction & NetInfo.Direction.Forward) != 0)
                {
                    return true;
                }
                if ((direction & NetInfo.Direction.Backward) != 0)
                {
                    return false;
                }
            }
            DebugLog.LogToFileOnly("Error: unknow lane direction");
            return false;
        }

        public override void Start()
        {
            base.Start();
            base.Hide();
        }
    }
}
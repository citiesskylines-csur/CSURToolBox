using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Collections;
using CSURToolBox.Util;
using System.Reflection;
using System;
using ColossalFramework.Math;

namespace CSURToolBox.UI
{
    public class StayInLaneUI : UIPanel
    {
        int laterLeftClick = 0;
        bool needLaterLeftClick = false;
        public void OnGUI()
        {
            if (!(Loader.is1637663252 || Loader.is1806963141) )
            {
                return;
            }

            if ((laterLeftClick == 8) && needLaterLeftClick)
            {
                laterLeftClick = 0;
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
                var selectedNodeId = TMPE.GetType("TrafficManager.UI.TrafficManagerTool").GetProperty("SelectedNodeId");
                var TrafficManagerTool = TMPE.CreateInstance("TrafficManager.UI.TrafficManagerTool");
                ushort node = (ushort)selectedNodeId.GetValue(TrafficManagerTool, null);
                DebugLog.LogToFileOnly("TMPE select node = " + node.ToString());
                NetInfo asset = Singleton<NetManager>.instance.m_nodes.m_buffer[node].Info;
                if (CSURUtil.IsCSUR(asset))
                {
                    bool IsCSURRLane = false;
                    for (int j = 0; j < 8; j++)
                    {
                        ushort segmentID = Singleton<NetManager>.instance.m_nodes.m_buffer[node].GetSegment(j);
                        if (segmentID != 0)
                        {
                            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
                            if (CSURUtil.IsCSUR(segment.Info))
                            {
                                if (CSURUtil.IsCSURRLaneOffset(segment.Info))
                                {
                                    IsCSURRLane = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (IsCSURRLane)
                    {
                        AddLaneConnectionForCSUR(node, TMPE);
                        //refresh
                        MouseSimulater.RightClick();
                        needLaterLeftClick = true;
                    }
                }
            }

            if (OptionsKeymappingFunction.m_stayinlaneAll.IsPressed(e))
            {
                Assembly TMPE = Assembly.Load("TrafficManager");
                for (ushort i = 0; i < Singleton<NetManager>.instance.m_nodes.m_size; i++)
                {
                    bool IsCSURRLane = false;
                    for (int j = 0; j < 8; j++)
                    {
                        ushort segmentID = Singleton<NetManager>.instance.m_nodes.m_buffer[i].GetSegment(j);
                        if (segmentID != 0)
                        {
                            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
                            if (CSURUtil.IsCSUR(segment.Info))
                            {
                                if (CSURUtil.IsCSURRLaneOffset(segment.Info))
                                {
                                    IsCSURRLane = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (IsCSURRLane)
                    {
                        AddLaneConnectionForCSUR(i, TMPE);
                    }
                }
            }
        }

        internal void AddLaneConnectionForCSUR(ushort node, Assembly TMPE)
        {
            var LaneConnectionManager = TMPE.CreateInstance("TrafficManager.Manager.Impl.LaneConnectionManager");
            var AddLaneConnection = TMPE.GetType("TrafficManager.Manager.Impl.LaneConnectionManager").GetMethod("AddLaneConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            //Loop1: Find all lanes in NODE which is outgoing, and try to AddLaneConnection
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
                                float distance = 10000;
                                uint laneID = 0;
                                //Loop2: Find all incoming lanes in NODE which is the nearest and the same direction to the outgoing lane to AddLaneConnection
                                for (int i1 = 0; i1 < 8; i1++)
                                {
                                    ushort segmentID1 = Singleton<NetManager>.instance.m_nodes.m_buffer[node].GetSegment(i1);
                                    if ((segmentID1 != 0) && (segmentID1 != segmentID))
                                    {
                                        var segment1 = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID1];
                                        uint firstLane1 = segment1.m_lanes;
                                        bool startNode1 = (segment1.m_startNode == node);
                                        for (int k = 0; k < segment1.Info.m_lanes.Length; k++)
                                        {
                                            if (firstLane1 == 0)
                                            {
                                                break;
                                            }
                                            bool isOutGoing1 = IsOutGoing(segmentID1, startNode1, segment1.Info.m_lanes[k]);
                                            if (segment1.Info.m_lanes[k].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && segment1.Info.m_lanes[k].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                                            {
                                                if (!isOutGoing1)
                                                {
                                                    Vector3 tmpLanePos1 = Vector3.zero;
                                                    Vector3 tmpLanePos2 = Vector3.zero;

                                                    if (startNode)
                                                    {
                                                        tmpLanePos2 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_bezier.Position(0);
                                                    }
                                                    else
                                                    {
                                                        tmpLanePos2 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_bezier.Position(1);
                                                    }

                                                    if (startNode1)
                                                    {
                                                        tmpLanePos1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_bezier.Position(0);
                                                    }
                                                    else
                                                    {
                                                        tmpLanePos1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_bezier.Position(1);
                                                    }

                                                    if (distance > Vector3.Distance(tmpLanePos2, tmpLanePos1))
                                                    {
                                                        distance = Vector3.Distance(tmpLanePos2, tmpLanePos1);
                                                        laneID = firstLane1;
                                                    }
                                                }//!isoutgoing
                                            }//car 
                                            firstLane1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_nextLane;
                                        } // for 
                                    }
                                }
                                if (laneID != 0)
                                {
                                    DebugLog.LogToFileOnly("firstLane = " + firstLane.ToString() + "laneID = " + laneID.ToString());
                                    AddLaneConnection.Invoke(LaneConnectionManager, new object[] { firstLane, laneID, startNode });
                                }
                            } //outgoing to match incoming
                            else
                            {
                                float distance = 10000;
                                uint laneID = 0;
                                bool tmpStartNode = false;
                                //Loop2: Find all incoming lanes in NODE which is the nearest and the same direction to the outgoing lane to AddLaneConnection
                                for (int i1 = 0; i1 < 8; i1++)
                                {
                                    ushort segmentID1 = Singleton<NetManager>.instance.m_nodes.m_buffer[node].GetSegment(i1);
                                    if ((segmentID1 != 0) && (segmentID1 != segmentID))
                                    {
                                        var segment1 = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID1];
                                        uint firstLane1 = segment1.m_lanes;
                                        bool startNode1 = (segment1.m_startNode == node);
                                        for (int k = 0; k < segment1.Info.m_lanes.Length; k++)
                                        {
                                            if (firstLane1 == 0)
                                            {
                                                break;
                                            }
                                            bool isOutGoing1 = IsOutGoing(segmentID1, startNode1, segment1.Info.m_lanes[k]);
                                            if (segment1.Info.m_lanes[k].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && segment1.Info.m_lanes[k].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                                            {
                                                if (isOutGoing1)
                                                {
                                                    Vector3 tmpLanePos1 = Vector3.zero;
                                                    Vector3 tmpLanePos2 = Vector3.zero;

                                                    if (startNode)
                                                    {
                                                        tmpLanePos2 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_bezier.Position(0);
                                                    }
                                                    else
                                                    {
                                                        tmpLanePos2 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_bezier.Position(1);
                                                    }

                                                    if (startNode1)
                                                    {
                                                        tmpLanePos1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_bezier.Position(0);
                                                    }
                                                    else
                                                    {
                                                        tmpLanePos1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_bezier.Position(1);
                                                    }

                                                    if (distance > Vector3.Distance(tmpLanePos2, tmpLanePos1))
                                                    {
                                                        distance = Vector3.Distance(tmpLanePos2, tmpLanePos1);
                                                        laneID = firstLane1;
                                                        tmpStartNode = startNode1;
                                                    }
                                                }//!isoutgoing
                                            }//car 
                                            firstLane1 = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane1].m_nextLane;
                                        } // for 
                                    }
                                }
                                if (laneID != 0)
                                {
                                    DebugLog.LogToFileOnly("firstLane = " + firstLane.ToString() + "laneID = " + laneID.ToString());
                                    AddLaneConnection.Invoke(LaneConnectionManager, new object[] { laneID, firstLane, tmpStartNode });
                                }
                            }//incoming to match outgoing
                        } //car 
                        firstLane = Singleton<NetManager>.instance.m_lanes.m_buffer[firstLane].m_nextLane;
                    }//for
                }
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
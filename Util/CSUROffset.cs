using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.Util
{
    public class CSUROffset
    {
        public const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";
        public const string CSUR_DUAL_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";
        public const string CSUR_OFFSET_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9](L|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9](L|R)[1-9]*P?)*";
        public const string CSUR_LANEOFFSET_REGEX = "CSUR-S ([1-9])(R|C)([1-9]?)(P?)=([1-9])(R|C)([1-9]?)(P?)";

        public static bool IsCSUR(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_REGEX, RegexOptions.IgnoreCase);
            return m.Success;
        }

        public static bool IsCSUROffset(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_OFFSET_REGEX, RegexOptions.IgnoreCase);
            return m.Success;
        }

        public static bool IsCSURDual(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_DUAL_REGEX, RegexOptions.IgnoreCase);
            return m.Success;
        }

        //TODO: needOffsetStartIdex is for CSUR-R Road, is laneidex > needOffsetStartIdex, we need to do offset
        //Currently, we only support CSUR-S Road to do offset, all laneOffset is the same value, and needOffsetStartIdex is 0, so all the road need to do offset
        //Return false to avoid to do offset.
        public static bool IsCSURLaneOffset(NetInfo asset, ref float laneOffset, ref int needOffsetStartIdex, ref float startOffset, ref float endOffset)
        {
            laneOffset = 0f;
            needOffsetStartIdex = 0;

            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_LANEOFFSET_REGEX, RegexOptions.IgnoreCase);

            if (!m.Success)
                return false;

            DebugLog.LogToFileOnly(m.Groups[1].Value);
            DebugLog.LogToFileOnly(m.Groups[2].Value);
            DebugLog.LogToFileOnly(m.Groups[3].Value);
            DebugLog.LogToFileOnly(m.Groups[4].Value);
            DebugLog.LogToFileOnly(m.Groups[5].Value);
            DebugLog.LogToFileOnly(m.Groups[6].Value);
            DebugLog.LogToFileOnly(m.Groups[7].Value);
            DebugLog.LogToFileOnly(m.Groups[8].Value);

            startOffset = 0;
            if (m.Groups[2].Value == "C")
            {
            }
            else
            {
                if (m.Groups[3].Value == "")
                {
                    if (m.Groups[1].Value != "")
                        startOffset = int.Parse(m.Groups[1].Value);
                }
                else
                {
                    if (m.Groups[4].Value == "")
                    {
                        if (m.Groups[3].Value != "")
                            startOffset = int.Parse(m.Groups[3].Value);
                    }
                    else
                    {
                        if (m.Groups[3].Value != "")
                            startOffset = int.Parse(m.Groups[3].Value) + 0.5f;
                    }
                }
            }

            endOffset = 0;
            if (m.Groups[6].Value == "C")
            {
            }
            else
            {
                if (m.Groups[7].Value == "")
                {
                    if (m.Groups[5].Value != "")
                        endOffset = int.Parse(m.Groups[5].Value);
                }
                else
                {
                    if (m.Groups[8].Value == "")
                    {
                        if (m.Groups[7].Value != "")
                            endOffset = int.Parse(m.Groups[7].Value);
                    }
                    else
                    {
                        if (m.Groups[7].Value != "")
                            endOffset = int.Parse(m.Groups[7].Value) + 0.5f;
                    }
                }
            }

            DebugLog.LogToFileOnly("startoffset = " + startOffset.ToString());
            DebugLog.LogToFileOnly("endoffset = " + endOffset.ToString());

            if (startOffset!=0 && endOffset!=0)
            {
                laneOffset = endOffset - startOffset;
            }

            if (laneOffset!=0)
                return m.Success;
            else
                return false;
        }
        //TODO: Use name and laneposition to get LaneIndex. the leftest lane is 0;
        public static int CSURLaneIndex(NetInfo asset, NetInfo.Lane lane)
        {
            return 0;
        }

        public static int CountLanes(NetInfo info)
        {
            int count = 0;
            for (int i = 0; i < info.m_lanes.Length; i++)
            {
                if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && info.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                {
                    count++;
                }
            }
            return count;
        }

        public static bool isStartNode(ushort nodeID)
        {
            for (int i = 0; i < 8; i++)
            {
                ushort segmentID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].GetSegment(i);
                if (segmentID != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].Info == Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].Info)
                    {
                        if (Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].m_startNode == nodeID)
                        {
                            return true;
                        }
                        else if (Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].m_endNode == nodeID)
                        {
                            return false;
                        }
                    }
                }
            }
            DebugLog.LogToFileOnly("Error: Did not find StartNode and EndNode match this node " + nodeID.ToString());
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
            for (int i = 0; i < 8; i++)
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
    }
}

using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.Util
{
    public class CSURUtil
    {
        public const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";
        public const string CSUR_DUAL_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";
        public const string CSUR_OFFSET_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9](L|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9](L|R)[1-9]*P?)*";
        public const string CSURR_LANE_REGEX = "CSUR-S ([1-9])(R|C)([1-9]?)(P?)=([1-9])(R|C)([1-9]?)(P?)";
        public const string CSUR_LANEOFFSET_REGEXPREFIX = "CSUR-(T|R|S)? ";
        public const string EachUnit = "[1-9]?[0-9]?D?[C|S|L|R]?[1-9]*P?";
        public const string MustUnit = "[1-9]?[0-9]D?[C|S|L|R][1-9]*P?";
        public const string CSUR_LANEOFFSET_REGEXLEFT = CSUR_LANEOFFSET_REGEXPREFIX + "(" + MustUnit + EachUnit + EachUnit + EachUnit + EachUnit + EachUnit + ")" + "=";
        public const string CSUR_LANEOFFSET_REGEX = CSUR_LANEOFFSET_REGEXLEFT + "(" + MustUnit + EachUnit + EachUnit + EachUnit + EachUnit + EachUnit + ")" + "_";
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

        public static bool IsCSURRLane(NetInfo asset, ref float laneOffset, ref float startOffset, ref float endOffset)
        {
            laneOffset = 0f;

            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSURR_LANE_REGEX, RegexOptions.IgnoreCase);

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

        public static bool IsCSURLaneOffset(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_LANEOFFSET_REGEX, RegexOptions.IgnoreCase);
            if (!m.Success)
                return false;

            DebugLog.LogToFileOnly("CSURLaneOffset1 = " + m.Groups[1].Value);
            DebugLog.LogToFileOnly("CSURLaneOffset2 = " + m.Groups[2].Value);
            DebugLog.LogToFileOnly("CSURLaneOffset3 = " + m.Groups[3].Value);
            return true;
        }

            //TODO: Use name and laneposition to get LaneOffset
        public static float CSURLaneOffset(NetInfo asset, NetInfo.Lane lane)
        {
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_LANEOFFSET_REGEX, RegexOptions.IgnoreCase);
            string pattern = "([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)?([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)?([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)?([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)?([[1-9]?[0-9]D?[C|S|L|R](0P)?(1P)?[2-9]*P?)?=";
            float[] leftStartLaneOffset = new float[8];
            float[] rightStartLaneOffset = new float[8];
            int leftStartLaneIndex = 0;
            int rightStartLaneIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                if (m.Groups[2].Value != "")
                {
                    //add "=" to help match
                    Match m1 = Regex.Match(m.Groups[2].Value + "=", pattern, RegexOptions.IgnoreCase);
                    if (m1.Groups[i+1].Value != "")
                    {
                        ProcessNameUnit(m1.Groups[i+1].Value, ref leftStartLaneOffset, ref rightStartLaneOffset, ref leftStartLaneIndex, ref rightStartLaneIndex);
                    }
                }
            }
            DebugLog.LogToFileOnly("CSURLaneOffset: leftStartLaneIndex = " + leftStartLaneIndex.ToString() + "rightStartLaneIndex = " + rightStartLaneIndex.ToString());

            float[] leftEndLaneOffset = new float[8];
            float[] rightEndLaneOffset = new float[8];
            int leftEndLaneIndex = 0;
            int rightEndLaneIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                if (m.Groups[3].Value != "")
                {
                    //add "=" to help match
                    Match m1 = Regex.Match(m.Groups[3].Value + "=", pattern, RegexOptions.IgnoreCase);
                    if (m1.Groups[i+1].Value != "")
                    {
                        ProcessNameUnit(m1.Groups[i+1].Value, ref leftEndLaneOffset, ref rightEndLaneOffset, ref leftEndLaneIndex, ref rightEndLaneIndex);
                    }
                }
            }
            DebugLog.LogToFileOnly("CSURLaneOffset: leftEndLaneIndex = " + leftEndLaneIndex.ToString() + "rightEndLaneIndex = " + rightEndLaneIndex.ToString());

            if ((leftEndLaneIndex + rightEndLaneIndex) != 0)
            {
                if ((leftStartLaneIndex + rightStartLaneIndex) != 0)
                {
                    if (Regex.Match(savenameStripped, "CSUR-T", RegexOptions.IgnoreCase).Success)
                    {
                        //CSUR-T have diffrent lanenum
                        if ((leftStartLaneIndex + rightStartLaneIndex) != (leftEndLaneIndex + rightEndLaneIndex))
                        {
                            float[] EndLaneOffset = new float[leftEndLaneIndex + rightEndLaneIndex];
                            float[] StartLaneOffset = new float[leftStartLaneIndex + rightStartLaneIndex];
                            for (int i = 0; i < leftStartLaneIndex + rightStartLaneIndex; i++)
                            {
                                if (i < leftStartLaneIndex)
                                {
                                    StartLaneOffset[i] = leftStartLaneOffset[leftStartLaneIndex - i - 1];
                                }
                                else
                                {
                                    StartLaneOffset[i] = rightStartLaneOffset[i - leftStartLaneIndex];
                                }
                                DebugLog.LogToFileOnly("StartLaneOffset, Line" + i.ToString() + " =" + StartLaneOffset[i].ToString());
                            }

                            for (int i = 0; i < leftEndLaneIndex + rightEndLaneIndex; i++)
                            {
                                if (i < leftEndLaneIndex)
                                {
                                    EndLaneOffset[i] = leftEndLaneOffset[leftEndLaneIndex - i - 1];
                                }
                                else
                                {
                                    EndLaneOffset[i] = rightEndLaneOffset[i - leftEndLaneIndex];
                                }
                                DebugLog.LogToFileOnly("EndLaneOffset, Line" + i.ToString() + " =" + EndLaneOffset[i].ToString());
                            }

                            int laneIndex = GetLaneIndex(asset, lane);
                            DebugLog.LogToFileOnly("laneIndex = " + laneIndex.ToString() + "lane position = " + lane.m_position.ToString());
                            if (lane.m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && lane.m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                            {
                                return 0;
                            }
                            else
                            {
                                if (laneIndex != 0)
                                {
                                    return (EndLaneOffset[leftEndLaneIndex + rightEndLaneIndex - 1] - StartLaneOffset[leftStartLaneIndex + rightStartLaneIndex - 1]);
                                }
                                else
                                {
                                    return (EndLaneOffset[laneIndex] - StartLaneOffset[laneIndex]);
                                }
                            }
                        }
                    }
                    else
                    {
                        //CSUR-R CSUR-S must have the same lanecounts
                        if ((leftStartLaneIndex + rightStartLaneIndex) == (leftEndLaneIndex + rightEndLaneIndex))
                        {
                            float[] EndLaneOffset = new float[leftEndLaneIndex + rightEndLaneIndex];
                            float[] StartLaneOffset = new float[leftStartLaneIndex + rightStartLaneIndex];
                            for (int i = 0; i < leftStartLaneIndex + rightStartLaneIndex; i++)
                            {
                                if (i < leftStartLaneIndex)
                                {
                                    StartLaneOffset[i] = leftStartLaneOffset[leftStartLaneIndex - i - 1];
                                }
                                else
                                {
                                    StartLaneOffset[i] = rightStartLaneOffset[i - leftStartLaneIndex];
                                }

                                if (i < leftEndLaneIndex)
                                {
                                    EndLaneOffset[i] = leftEndLaneOffset[leftEndLaneIndex - i - 1];
                                }
                                else
                                {
                                    EndLaneOffset[i] = rightEndLaneOffset[i - leftEndLaneIndex];
                                }

                                DebugLog.LogToFileOnly("StartLaneOffset, Line" + i.ToString() + " =" + StartLaneOffset[i].ToString());
                                DebugLog.LogToFileOnly("EndLaneOffset, Line" + i.ToString() + " =" + EndLaneOffset[i].ToString());
                            }

                            int laneIndex = GetLaneIndex(asset, lane);
                            DebugLog.LogToFileOnly("laneIndex = " + laneIndex.ToString() + "lane position = " + lane.m_position.ToString());
                            if (lane.m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && lane.m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                            {
                                return (EndLaneOffset[laneIndex] - StartLaneOffset[laneIndex]);
                            }
                            else
                            {
                                if (laneIndex != 0)
                                {
                                    return (EndLaneOffset[leftStartLaneIndex + rightStartLaneIndex - 1] - StartLaneOffset[leftStartLaneIndex + rightStartLaneIndex - 1]);
                                }
                                else
                                {
                                    return (EndLaneOffset[laneIndex] - StartLaneOffset[laneIndex]);
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public static int GetLaneIndex(NetInfo asset, NetInfo.Lane lane)
        {
            int laneNum = 0;
            int carLaneIndex = 0;
            for (int i = 0; i < asset.m_lanes.Length; i++)
            {
                if (asset.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && asset.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                {
                    laneNum++;
                    if (asset.m_lanes[i].m_position < lane.m_position)
                    {
                        carLaneIndex++;
                    }
                }
            }

            if (lane.m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && lane.m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
            {
                return carLaneIndex;
            }
            else
            {
                //treat bike and human lane as the leftest carlane or rightest carlane
                if (carLaneIndex == 0)
                {
                    return 0;
                }
                else
                {
                    return laneNum;
                }
            }
        }

        public static void ProcessNameUnit(string name, ref float[] leftLaneOffset, ref float[] rightLaneOffset, ref int leftLaneIndex, ref int rightLaneIndex)
        {
            //name EG:2DR 3DS 1R2 1R2P 3C 3L
            Match m = Regex.Match(name, "([0-9]*)(D?)(S|C|R|L)([0-9]?[0-9]?)(P?)", RegexOptions.IgnoreCase);
            DebugLog.LogToFileOnly("ProcessNameUnit match 1 = " + m.Groups[1].Value);
            DebugLog.LogToFileOnly("ProcessNameUnit match 2 = " + m.Groups[2].Value);
            DebugLog.LogToFileOnly("ProcessNameUnit match 3 = " + m.Groups[3].Value);
            DebugLog.LogToFileOnly("ProcessNameUnit match 4 = " + m.Groups[4].Value);
            DebugLog.LogToFileOnly("ProcessNameUnit match 5 = " + m.Groups[5].Value);
            //Process Dual
            if (m.Groups[2].Value == "D")
            {
                if (m.Groups[3].Value == "S")
                {
                    //DS
                    if (m.Groups[1].Value != "")
                    {
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                        {
                            leftLaneOffset[leftLaneIndex] = -(i + 0.5f);
                            leftLaneIndex++;
                        }
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f) + 1; i++)
                        {
                            rightLaneOffset[rightLaneIndex] = i + 0.5f;
                            rightLaneIndex++;
                        }
                    }
                }
                else if (m.Groups[3].Value == "C")
                {
                    if (m.Groups[1].Value != "")
                    {
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                        {
                            leftLaneOffset[leftLaneIndex] = -(i + 0.5f);
                            leftLaneIndex++;
                        }
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                        {
                            rightLaneOffset[rightLaneIndex] = i + 0.5f;
                            rightLaneIndex++;
                        }
                    }
                }
                else if (m.Groups[3].Value == "R")
                {
                    //DR
                    float num = 0;
                    if (m.Groups[4].Value != "")
                    {
                        //2DR4P  2DR4
                        if (m.Groups[5].Value == "P")
                            num = int.Parse(m.Groups[4].Value) + 0.5f;
                        else
                            num = int.Parse(m.Groups[4].Value);

                        if (m.Groups[1].Value != "")
                        {
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                            {
                                leftLaneOffset[leftLaneIndex] = -(i + 1 + num - int.Parse(m.Groups[1].Value)/2);
                                leftLaneIndex++;
                            }
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                            {
                                rightLaneOffset[rightLaneIndex] = i + 1 + num - int.Parse(m.Groups[1].Value)/2;
                                rightLaneIndex++;
                            }
                        }
                    }
                    else
                    {
                        //4DR 3DR
                        if (m.Groups[1].Value != "")
                        {
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                            {
                                leftLaneOffset[leftLaneIndex] = -(i + 1);
                                leftLaneIndex++;
                            }
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                            {
                                rightLaneOffset[rightLaneIndex] = i + 1;
                                rightLaneIndex++;
                            }
                        }
                    }
                }
            }
            else
            {
                if (m.Groups[3].Value == "C")
                {
                    //3C
                    if (m.Groups[1].Value != "")
                    {
                        //store C road
                        if (int.Parse(m.Groups[1].Value) % 2 != 0)
                        {
                            leftLaneOffset[leftLaneIndex] = 0;
                            leftLaneIndex++;
                        }
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                        {
                            leftLaneOffset[leftLaneIndex] = -(i + 0.5f);
                            leftLaneIndex++;
                        }
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value) / 2f); i++)
                        {
                            rightLaneOffset[rightLaneIndex] = i + 0.5f;
                            rightLaneIndex++;
                        }
                    }
                }
                else if (m.Groups[3].Value == "R")
                {
                    //R
                    float num = 0;
                    if (m.Groups[4].Value != "")
                    {
                        //2R4P  2R4
                        if (m.Groups[5].Value == "P")
                            num = int.Parse(m.Groups[4].Value) + 0.5f;
                        else
                            num = int.Parse(m.Groups[4].Value);

                        if (m.Groups[1].Value != "")
                        {
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value)); i++)
                            {
                                rightLaneOffset[rightLaneIndex] = i + 1 + num - int.Parse(m.Groups[1].Value);
                                rightLaneIndex++;
                            }
                        }
                    }
                    else
                    {
                        //2R
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value)); i++)
                        {
                            rightLaneOffset[rightLaneIndex] = i + 1;
                            rightLaneIndex++;
                        }
                    }
                }
                else if (m.Groups[3].Value == "L")
                {
                    //L
                    float num = 0;
                    if (m.Groups[4].Value != "")
                    {
                        //2L4P  2L4
                        if (m.Groups[5].Value == "P")
                            num = int.Parse(m.Groups[4].Value) + 0.5f;
                        else
                            num = int.Parse(m.Groups[4].Value);

                        if (m.Groups[1].Value != "")
                        {
                            for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value)); i++)
                            {
                                leftLaneOffset[leftLaneIndex] = -(i + 1 + num - int.Parse(m.Groups[1].Value));
                                leftLaneIndex++;
                            }
                        }
                    }
                    else
                    {
                        //2L
                        for (int i = 0; i < (int)(int.Parse(m.Groups[1].Value)); i++)
                        {
                            leftLaneOffset[leftLaneIndex] = -(i + 1);
                            leftLaneIndex++;
                        }
                    }
                }
            }
        }
        
        //bike lane is 2.75, treat as 2.75/3.75 lanes
        public static float CountLanes(NetInfo info)
        {
            float count = 0f;
            for (int i = 0; i < info.m_lanes.Length; i++)
            {
                if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && info.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                {
                    count += 1f;
                }
                else if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && info.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Bicycle))
                {
                    count += (2.75f/3.75f);
                }
                else if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Pedestrian))
                {
                    count += 1f;
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

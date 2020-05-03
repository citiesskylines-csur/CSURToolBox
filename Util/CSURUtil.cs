using ColossalFramework;
using CSURToolBox.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.Util
{
    public class CSURUtil
    {
        public const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*" + "( compact| express)?" + "_";
        public const string CSUR_EXPRESS_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*" + "( express)" + "_";
        public const string CSUR_DUAL_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*" + "( compact| express)?" + "_";
        public const string CSUR_DUAL_REGEX1 = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)-([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*" + "( compact| express)?" + "_";
        public const string CSUR_OFFSET_REGEX =   "CSUR(-(T|R|S))? ([[1-9]?[0-9](L|R)[1-9]*P?)+(=)?([[1-9]?[0-9](L|R)[1-9]*P?)*" + "( compact| express)?" + "_";
        public const string CSUR_NOOFFSET_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9](L|R)[1-9]*P?)+(=)?([[1-9]?[0-9]D?(S|C)[1-9]*P?)+" + "( compact| express)?" + "_";
        public const string CSURS_LANE_REGEX = "CSUR-S ([1-9])(R|C)([1-9]?)(P?)=([1-9])(R|C)([1-9]?)(P?)";
        public const string CSUR_LANEOFFSET_REGEXPREFIX = "CSUR-(T|R|S)? ";
        public const string EachUnit = "[1-9]?[0-9]?D?[C|S|L|R]?[1-9]*P?";
        public const string MustUnit = "[1-9]?[0-9]D?[C|S|L|R][1-9]*P?";
        public const string CSUR_LANEOFFSET_REGEXLEFT = CSUR_LANEOFFSET_REGEXPREFIX + "(" + MustUnit + EachUnit + EachUnit + EachUnit + EachUnit + EachUnit + ")" + "=";
        public const string CSUR_LANEOFFSET_REGEX = CSUR_LANEOFFSET_REGEXLEFT + "(" + MustUnit + EachUnit + EachUnit + EachUnit + EachUnit + EachUnit + ")" + "( compact| express)?" + "_";
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
            Match m1 = Regex.Match(savenameStripped, CSUR_NOOFFSET_REGEX, RegexOptions.IgnoreCase);
            return m.Success && (!m1.Success);
        }

        public static bool IsCSURNoJunction(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            return asset.m_nodes.Length == 0;
        }

        public static bool IsCSURDual(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_DUAL_REGEX, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Success;
            }
            else
            {
                Match m1 = Regex.Match(savenameStripped, CSUR_DUAL_REGEX1, RegexOptions.IgnoreCase);
                return m1.Success;
            }
        }

        public static bool IsCSURExpress(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_EXPRESS_REGEX, RegexOptions.IgnoreCase);
            return m.Success;
        }

        public static bool IsCSURSLane(NetInfo asset, ref float laneOffset, ref float startOffset, ref float endOffset)
        {
            laneOffset = 0f;

            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSURS_LANE_REGEX, RegexOptions.IgnoreCase);

            if (!m.Success)
                return false;

            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"{m.Groups[1].Value} {m.Groups[2].Value} {m.Groups[3].Value} {m.Groups[4].Value} {m.Groups[5].Value} {m.Groups[6].Value} {m.Groups[7].Value} {m.Groups[8].Value}");
            }

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
            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"startoffset = {startOffset} endoffset = {endOffset}");
            }

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

            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"CSURLaneOffset1 {m.Groups[1].Value} CSURLaneOffset2 = {m.Groups[2].Value} CSURLaneOffset3 = {m.Groups[3].Value}");
            }
            return true;
        }

        public static bool IsCSURRLaneOffset(NetInfo asset)
        {
            if (asset == null || (asset.m_netAI.GetType() != typeof(RoadAI) && asset.m_netAI.GetType() != typeof(RoadBridgeAI) && asset.m_netAI.GetType() != typeof(RoadTunnelAI)))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_LANEOFFSET_REGEX, RegexOptions.IgnoreCase);
            if (!m.Success)
                return false;

            Match m1 = Regex.Match(savenameStripped, "CSUR-R", RegexOptions.IgnoreCase);
            return m1.Success;
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
            bool xRxxDSCase = false;

            if (m.Groups[2].Value != "")
            {
                //add "=" to help match
                Match m1 = Regex.Match(m.Groups[2].Value + "=", pattern, RegexOptions.IgnoreCase);
                if ((m1.Groups[1].Value != "") && (m1.Groups[4].Value != ""))
                {
                    if ((!TryParseDSInProcessNameUnit(m1.Groups[1].Value)) && TryParseDSInProcessNameUnit(m1.Groups[4].Value))
                    {
                        xRxxDSCase = true;
                        Regex r = new Regex("R");
                        string s;
                        s = r.Replace(m1.Groups[1].Value, "L", 1);
                        if (OptionUI.isDebug)
                        {
                            DebugLog.LogToFileOnly("CSURLaneOffset: replaced xRxxDSCase = " + s);
                        }
                        ProcessNameUnit(m1.Groups[4].Value, ref leftStartLaneOffset, ref rightStartLaneOffset, ref leftStartLaneIndex, ref rightStartLaneIndex);
                        ProcessNameUnit(s, ref leftStartLaneOffset, ref rightStartLaneOffset, ref leftStartLaneIndex, ref rightStartLaneIndex);
                    }
                }
            }

            if (!xRxxDSCase)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (m.Groups[2].Value != "")
                    {
                        //add "=" to help match
                        Match m1 = Regex.Match(m.Groups[2].Value + "=", pattern, RegexOptions.IgnoreCase);
                        if (m1.Groups[i * 3 + 1].Value != "")
                        {
                            ProcessNameUnit(m1.Groups[i * 3 + 1].Value, ref leftStartLaneOffset, ref rightStartLaneOffset, ref leftStartLaneIndex, ref rightStartLaneIndex);
                        }
                    }
                }
            }
            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"CSURLaneOffset: leftStartLaneIndex = {leftStartLaneIndex} rightStartLaneIndex = {rightStartLaneIndex}");
            }

            float[] leftEndLaneOffset = new float[8];
            float[] rightEndLaneOffset = new float[8];
            int leftEndLaneIndex = 0;
            int rightEndLaneIndex = 0;

            xRxxDSCase = false;

            if (m.Groups[3].Value != "")
            {
                //add "=" to help match
                Match m1 = Regex.Match(m.Groups[3].Value + "=", pattern, RegexOptions.IgnoreCase);
                if ((m1.Groups[1].Value != "") && (m1.Groups[4].Value != ""))
                {
                    if ((!TryParseDSInProcessNameUnit(m1.Groups[1].Value)) && TryParseDSInProcessNameUnit(m1.Groups[4].Value))
                    {
                        xRxxDSCase = true;
                        Regex r = new Regex("R");
                        string s;
                        s = r.Replace(m1.Groups[1].Value, "L", 1);
                        if (OptionUI.isDebug)
                        {
                            DebugLog.LogToFileOnly("CSURLaneOffset: replaced xRxxDSCase = " + s);
                        }
                        ProcessNameUnit(m1.Groups[4].Value, ref leftEndLaneOffset, ref rightEndLaneOffset, ref leftEndLaneIndex, ref rightEndLaneIndex);
                        ProcessNameUnit(s, ref leftEndLaneOffset, ref rightEndLaneOffset, ref leftEndLaneIndex, ref rightEndLaneIndex);
                    }
                }
            }

            if (!xRxxDSCase)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (m.Groups[3].Value != "")
                    {
                        //add "=" to help match
                        Match m1 = Regex.Match(m.Groups[3].Value + "=", pattern, RegexOptions.IgnoreCase);
                        if (m1.Groups[i * 3 + 1].Value != "")
                        {
                            ProcessNameUnit(m1.Groups[i * 3 + 1].Value, ref leftEndLaneOffset, ref rightEndLaneOffset, ref leftEndLaneIndex, ref rightEndLaneIndex);
                        }
                    }
                }
            }
            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"CSURLaneOffset: leftEndLaneIndex = {leftEndLaneIndex} rightEndLaneIndex = {rightEndLaneIndex}");
            }

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
                                if (OptionUI.isDebug)
                                {
                                    DebugLog.LogToFileOnly($"StartLaneOffset: Line {i} = {StartLaneOffset[i]}");
                                }
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
                                if (OptionUI.isDebug)
                                {
                                    DebugLog.LogToFileOnly("EndLaneOffset: Line {i} = {EndLaneOffset[i]}");
                                }
                            }

                            int laneIndex = GetLaneIndex(asset, lane);
                            if (OptionUI.isDebug)
                            {
                                DebugLog.LogToFileOnly("laneIndex = {laneIndex} lanePosition = {lane.m_position}");
                            }
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

                                if (OptionUI.isDebug)
                                {
                                    DebugLog.LogToFileOnly($"StartLaneOffset: Line {i} = {StartLaneOffset[i]} EndLaneOffset: Line {i} = {EndLaneOffset[i]}");
                                }
                            }

                            int laneIndex = GetLaneIndex(asset, lane);
                            if (OptionUI.isDebug)
                            {
                                DebugLog.LogToFileOnly($"laneIndex = {laneIndex} lanePosition = {lane.m_position}");
                            }
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

        //identify 1RXX and XDS case
        public static bool TryParseDSInProcessNameUnit(string name)
        {
            Match m = Regex.Match(name, "([0-9]*)(D?)(S|C|R|L)([0-9]?[0-9]?)(P?)", RegexOptions.IgnoreCase);
            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"TryParseDSInProcessNameUnit match 1 = {m.Groups[1].Value} match 2 = {m.Groups[2].Value} match 3 = {m.Groups[3].Value} match 4 = {m.Groups[4].Value} match 5 = {m.Groups[5].Value}");
            }

            if (m.Groups[2].Value == "D")
            {
                if (m.Groups[3].Value == "S")
                {
                    return true;
                }
            }
            return false;
        }

        public static void ProcessNameUnit(string name, ref float[] leftLaneOffset, ref float[] rightLaneOffset, ref int leftLaneIndex, ref int rightLaneIndex)
        {
            //name EG:2DR 3DS 1R2 1R2P 3C 3L
            Match m = Regex.Match(name, "([0-9]*)(D?)(S|C|R|L)([0-9]?[0-9]?)(P?)", RegexOptions.IgnoreCase);
            if (OptionUI.isDebug)
            {
                DebugLog.LogToFileOnly($"ProcessNameUnit match 1 = {m.Groups[1].Value} match 2 = {m.Groups[2].Value} match 3 = {m.Groups[3].Value} match 4 = {m.Groups[4].Value} match 5 = {m.Groups[5].Value}");
            }
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
        public static float CountCSURSVehicleLanes(NetInfo info)
        {
            float count = 0f;
            for (int i = 0; i < info.m_lanes.Length; i++)
            {
                if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && info.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Car))
                {
                    count += 1f;
                }
            }
            return count;
        }

        public static float CountCSURSOtherLanes(NetInfo info, bool caculatePrice = false)
        {
            float count = 0f;
            for (int i = 0; i < info.m_lanes.Length; i++)
            {
                if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle) && info.m_lanes[i].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Bicycle))
                {
                    if (caculatePrice)
                        count += 1f;
                    else
                        count += (2.75f / 3.75f);
                }
                else if (info.m_lanes[i].m_laneType.IsFlagSet(NetInfo.LaneType.Pedestrian))
                {
                    if (caculatePrice)
                        count += 1f;
                    else
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
            DebugLog.LogToFileOnly($"Error: Did not find StartNode and EndNode match this node {nodeID}");
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

        public static byte GetParameterLoc(MethodInfo method, string name)
        {
            var parameters = method.GetParameters();
            for (byte i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].Name == name)
                {
                    return i;
                }
            }
            throw new Exception($"did not found parameter with name:<{name}>");
        }

        public static CodeInstruction GetLDArg(MethodInfo method, string argName)
        {
            byte idx = (byte)GetParameterLoc(method, argName);
            if (!method.IsStatic)
                idx++; // first argument is object instance.
            if (idx == 0)
            {
                return new CodeInstruction(OpCodes.Ldarg_0);
            }
            else if (idx == 1)
            {
                return new CodeInstruction(OpCodes.Ldarg_1);
            }
            else if (idx == 2)
            {
                return new CodeInstruction(OpCodes.Ldarg_2);
            }
            else if (idx == 3)
            {
                return new CodeInstruction(OpCodes.Ldarg_3);
            }
            else
            {
                return new CodeInstruction(OpCodes.Ldarg_S, idx);
            }
        }

        public static float GetMinCornerOffset(float cornerOffset0, ushort nodeID)
        {
            var instance = Singleton<NetManager>.instance;
            if (OptionUI.fixLargeJunction)
            {
                // NON-STOCK CODE STARTS
                float m_minCornerOffset = 0f;
                float tempMinCornerOffset = 1000f;
                float m_maxCornerOffset = 0f;
                float tempMaxCornerOffset = 0f;
                float finalCornerOffset = 0f;
                int segmentCount = 0;
                bool isCSURRoadFixLargeJunction = false;
                for (int i = 0; i < 8; i++)
                {
                    ushort segment1 = instance.m_nodes.m_buffer[nodeID].GetSegment(i);
                    if (segment1 != 0)
                    {
                        segmentCount++;
                        if (CSURUtil.IsCSUR(instance.m_segments.m_buffer[segment1].Info))
                        {
                            if (instance.m_segments.m_buffer[segment1].Info.m_nodes.Length == 0)
                            {
                                isCSURRoadFixLargeJunction = false;
                                break;
                            }
                            else
                            {
                                isCSURRoadFixLargeJunction = true;
                            }
                        }
                        if (instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset < tempMinCornerOffset)
                        {
                            tempMinCornerOffset = instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset;
                        }
                        if (instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset > tempMaxCornerOffset)
                        {
                            tempMaxCornerOffset = instance.m_segments.m_buffer[segment1].Info.m_minCornerOffset;
                        }
                    }
                }

                if (isCSURRoadFixLargeJunction)
                {
                    if (tempMinCornerOffset != 1000f)
                    {
                        m_minCornerOffset = tempMinCornerOffset;
                    }

                    if (tempMaxCornerOffset != 0f)
                    {
                        m_maxCornerOffset = tempMaxCornerOffset;
                    }

                    //direct node
                    if (segmentCount == 2)
                    {
                        finalCornerOffset = m_minCornerOffset / 2f;
                        if (finalCornerOffset > 24f)
                        {
                            finalCornerOffset = 24f;
                        }
                    }
                    else
                    {
                        if (cornerOffset0 == m_maxCornerOffset)
                        {
                            finalCornerOffset = (m_minCornerOffset + m_maxCornerOffset) / 2f;
                        }
                        else if (cornerOffset0 == m_minCornerOffset)
                        {
                            finalCornerOffset = m_maxCornerOffset;
                        }
                        else
                        {
                            finalCornerOffset = m_maxCornerOffset;
                        }
                    }
                }
                return finalCornerOffset;
            }
            return cornerOffset0;
        }
    }
}

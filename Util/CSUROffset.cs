using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSURToolBox.Util
{
    public class CSUROffset
    {
        public const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";
        public const string CSUR_OFFSET_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9](L|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9](L|R)[1-9]*P?)*";

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

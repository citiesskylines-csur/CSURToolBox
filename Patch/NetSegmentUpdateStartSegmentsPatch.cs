using ColossalFramework;
using System.Reflection;
using UnityEngine;
using CSURToolBox.Util;
using HarmonyLib;
using CSURToolBox.UI;
using ColossalFramework.Math;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetSegmentUpdateStartSegmentsPatch
	{
        public static MethodBase TargetMethod()
        {
			return typeof(NetSegment).GetMethod("UpdateStartSegments", BindingFlags.Public | BindingFlags.Instance);
        }
        public static bool Prefix(ref NetSegment __instance, ushort segmentID)
        {
			if (OptionUI.noJunction)
			{
				NetInfo asset = __instance.Info;
				if (asset != null)
				{
					if (asset.m_netAI is RoadAI)
					{
						if (CSURUtil.IsCSUR(asset))
						{
							CSURUpdateStartSegments(ref __instance, segmentID);
							return false;
						}
					}
				}
			}
			return true;
		}
		public static void CSURUpdateStartSegments(ref NetSegment __instance, ushort segmentID)
		{
			NetManager instance = Singleton<NetManager>.instance;
			NetInfo info = __instance.Info;
			if (info == null)
			{
				return;
			}
			ItemClass connectionClass = info.GetConnectionClass();
			float num = -4f;
			float num2 = -4f;
			ushort startLeftSegment = 0;
			ushort startRightSegment = 0;
			for (int i = 0; i < 8; i++)
			{
				ushort segment = instance.m_nodes.m_buffer[(int)__instance.m_startNode].GetSegment(i);
				if (segment != 0 && segment != segmentID)
				{
					NetInfo info2 = instance.m_segments.m_buffer[(int)segment].Info;
					if (info2 != null)
					{
						ItemClass connectionClass2 = info2.GetConnectionClass();
						if (connectionClass.m_service == connectionClass2.m_service)
						{
							//Non-stock code begin
							Vector3 vector = instance.m_lanes.m_buffer[instance.m_segments.m_buffer[(int)segment].m_lanes].m_bezier.Position(0.5f) - instance.m_lanes.m_buffer[instance.m_segments.m_buffer[(int)segmentID].m_lanes].m_bezier.Position(0.5f);
							vector = VectorUtils.NormalizeXZ(vector);
							//Non-stock code end
							float num3 = __instance.m_startDirection.x * vector.x + __instance.m_startDirection.z * vector.z;
							if (vector.z * __instance.m_startDirection.x - vector.x * __instance.m_startDirection.z < 0f)
							{
								if (num3 > num)
								{
									num = num3;
									startLeftSegment = segment;
								}
								num3 = -2f - num3;
								if (num3 > num2)
								{
									num2 = num3;
									startRightSegment = segment;
								}
							}
							else
							{
								if (num3 > num2)
								{
									num2 = num3;
									startRightSegment = segment;
								}
								num3 = -2f - num3;
								if (num3 > num)
								{
									num = num3;
									startLeftSegment = segment;
								}
							}
						}
					}
				}
			}
			__instance.m_startLeftSegment = startLeftSegment;
			__instance.m_startRightSegment = startRightSegment;
		}
	}
}

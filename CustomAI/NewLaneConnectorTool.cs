using ColossalFramework;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.CustomAI
{
    public class NewLaneConnectorTool
    {
		public const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";

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
		private bool CheckSegmentsTurningAngle(ushort sourceSegmentId, ref NetSegment sourceSegment, bool sourceStartNode, ushort targetSegmentId, ref NetSegment targetSegment, bool targetStartNode)
		{
			NetManager netManager = Singleton<NetManager>.instance;

			NetInfo sourceSegmentInfo = netManager.m_segments.m_buffer[sourceSegmentId].Info;
			NetInfo targetSegmentInfo = netManager.m_segments.m_buffer[targetSegmentId].Info;

			if (IsCSUR(sourceSegmentInfo) || IsCSUR(targetSegmentInfo))
				return true;

			float turningAngle = 0.01f - Mathf.Min(sourceSegmentInfo.m_maxTurnAngleCos, targetSegmentInfo.m_maxTurnAngleCos);
			if (turningAngle < 1f)
			{
				Vector3 sourceDirection;
				if (sourceStartNode)
				{
					sourceDirection = sourceSegment.m_startDirection;
				}
				else
				{
					sourceDirection = sourceSegment.m_endDirection;
				}

				Vector3 targetDirection;
				if (targetStartNode)
				{
					targetDirection = targetSegment.m_startDirection;
				}
				else
				{
					targetDirection = targetSegment.m_endDirection;
				}
				float dirDotProd = sourceDirection.x * targetDirection.x + sourceDirection.z * targetDirection.z;
				return dirDotProd < turningAngle;
			}
			return true;
		}
	}
}

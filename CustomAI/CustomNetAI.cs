using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CSURToolBox.CustomAI
{
    public class CustomNetAI: NetAI
    {
        public virtual float CustomGetCollisionHalfWidth()
        {
            if (CSUROffset.IsCSUROffset(m_info))
            {
                return (m_info.m_halfWidth - m_info.m_pavementWidth) / 2f;
            }
            return m_info.m_halfWidth;
        }

		public static void RoadBaseAIUpdateLanesPostFix(ushort segmentID, ref NetSegment data, bool loading)
		{
			NetManager instance = Singleton<NetManager>.instance;
			var m_info = data.Info;
			uint firstLane = data.m_lanes;
			float laneOffset = 0;
			int startOffsetIdex = 0;
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
					if (CSUROffset.CSURLaneIndex(m_info , lane) >= startOffsetIdex)
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
		}
	}
}

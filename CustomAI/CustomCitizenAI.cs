using ColossalFramework;
using ColossalFramework.Math;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace CSURToolBox.CustomAI
{
    public class CustomCitizenAI: CitizenAI
	{
		protected Vector4 CustomGetPathTargetPosition(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, float minSqrDistance)
		{
			PathManager instance = Singleton<PathManager>.instance;
			NetManager instance2 = Singleton<NetManager>.instance;
			Vector4 vector = citizenData.m_targetPos;
			float num = VectorUtils.LengthSqrXZ((Vector3)citizenData.m_targetPos - frameData.m_position);
			if (num >= minSqrDistance)
			{
				return vector;
			}
			if (citizenData.m_pathPositionIndex == 255)
			{
				citizenData.m_pathPositionIndex = 0;
				if (!Singleton<PathManager>.instance.m_pathUnits.m_buffer[citizenData.m_path].CalculatePathPositionOffset(citizenData.m_pathPositionIndex >> 1, vector, out citizenData.m_lastPathOffset))
				{
					InvalidPath(instanceID, ref citizenData);
					return vector;
				}
			}
			if (!instance.m_pathUnits.m_buffer[citizenData.m_path].GetPosition(citizenData.m_pathPositionIndex >> 1, out PathUnit.Position position))
			{
				InvalidPath(instanceID, ref citizenData);
				return vector;
			}
			if ((citizenData.m_pathPositionIndex & 1) == 0)
			{
				int num2 = (citizenData.m_pathPositionIndex >> 1) + 1;
				uint num3 = citizenData.m_path;
				if (num2 >= instance.m_pathUnits.m_buffer[num3].m_positionCount)
				{
					num2 = 0;
					num3 = instance.m_pathUnits.m_buffer[num3].m_nextPathUnit;
				}
				PathUnit.Position position2;
				if (num3 != 0 && instance.m_pathUnits.m_buffer[num3].GetPosition(num2, out position2) && position2.m_segment == position.m_segment)
				{
					NetInfo info = instance2.m_segments.m_buffer[position.m_segment].Info;
					if (info.m_lanes.Length > position.m_lane && info.m_lanes.Length > position2.m_lane)
					{
						float position3 = info.m_lanes[position.m_lane].m_position;
						float position4 = info.m_lanes[position2.m_lane].m_position;
						if (Mathf.Abs(position3 - position4) < 4f)
						{
							citizenData.m_pathPositionIndex = (byte)(num2 << 1);
							position = position2;
							if (num3 != citizenData.m_path)
							{
								Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
							}
						}
					}
				}
			}
			uint num4 = PathManager.GetLaneID(position);
			float num5 = (float)new Randomizer(instanceID).Int32(-500, 500) * 0.001f;
			Segment3 segment = default(Segment3);
			Vector3 b2;
			while (true)
			{
				NetInfo info2 = instance2.m_segments.m_buffer[position.m_segment].Info;
				if (info2.m_lanes.Length <= position.m_lane)
				{
					InvalidPath(instanceID, ref citizenData);
					return vector;
				}
				float width = info2.m_lanes[position.m_lane].m_width;
				float d = Mathf.Max(0f, width - 1f) * num5;
				float num6 = (!info2.m_lanes[position.m_lane].m_useTerrainHeight && (citizenData.m_flags & CitizenInstance.Flags.OnPath) != 0) ? 0f : 1f;
				if ((citizenData.m_pathPositionIndex & 1) == 0)
				{
					bool flag = true;
					int num7 = position.m_offset - citizenData.m_lastPathOffset;
					while (num7 != 0)
					{
						if (flag)
						{
							flag = false;
						}
						else
						{
							float num8 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3)vector - frameData.m_position);
							int num9 = (!(num8 < 0f)) ? (4 + Mathf.CeilToInt(num8 * 256f / (instance2.m_lanes.m_buffer[num4].m_length + 1f))) : 4;
							if (num7 < 0)
							{
								citizenData.m_lastPathOffset = (byte)Mathf.Max(citizenData.m_lastPathOffset - num9, position.m_offset);
							}
							else if (num7 > 0)
							{
								citizenData.m_lastPathOffset = (byte)Mathf.Min(citizenData.m_lastPathOffset + num9, position.m_offset);
							}
						}
						instance2.m_lanes.m_buffer[num4].CalculatePositionAndDirection((float)(int)citizenData.m_lastPathOffset * 0.003921569f, out Vector3 position5, out Vector3 direction);
						vector = position5;
						vector.w = num6;
						Vector3 vector2 = Vector3.Cross(Vector3.up, direction).normalized * d;
						if (num7 > 0)
						{
							vector.x += vector2.x;
							vector.z += vector2.z;
						}
						else
						{
							vector.x -= vector2.x;
							vector.z -= vector2.z;
						}
						num = VectorUtils.LengthSqrXZ((Vector3)vector - frameData.m_position);
						if (num >= minSqrDistance)
						{
							citizenData.m_flags = ((citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition)) | info2.m_setCitizenFlags);
							return vector;
						}
						num7 = position.m_offset - citizenData.m_lastPathOffset;
						if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
						{
							citizenData.m_flags |= CitizenInstance.Flags.OnPath;
							if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None && (instance2.m_segments.m_buffer[position.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None)
							{
								SpawnBicycle(instanceID, ref citizenData, position);
							}
						}
					}
					citizenData.m_pathPositionIndex++;
					citizenData.m_lastPathOffset = 0;
				}
				int num10 = (citizenData.m_pathPositionIndex >> 1) + 1;
				uint num11 = citizenData.m_path;
				if (num10 >= instance.m_pathUnits.m_buffer[citizenData.m_path].m_positionCount)
				{
					num10 = 0;
					num11 = instance.m_pathUnits.m_buffer[citizenData.m_path].m_nextPathUnit;
					if (num11 == 0)
					{
						Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
						citizenData.m_path = 0u;
						return vector;
					}
				}
				if (!instance.m_pathUnits.m_buffer[num11].GetPosition(num10, out PathUnit.Position position6))
				{
					InvalidPath(instanceID, ref citizenData);
					return vector;
				}
				NetInfo info3 = instance2.m_segments.m_buffer[position6.m_segment].Info;
				if (info3.m_lanes.Length <= position6.m_lane)
				{
					InvalidPath(instanceID, ref citizenData);
					return vector;
				}
				int num12 = num10 + 1;
				uint num13 = num11;
				uint num14 = 0u;
				if (num12 >= instance.m_pathUnits.m_buffer[num11].m_positionCount)
				{
					num12 = 0;
					num13 = instance.m_pathUnits.m_buffer[num11].m_nextPathUnit;
				}
				PathUnit.Position position7;
				if (num13 != 0 && instance.m_pathUnits.m_buffer[num13].GetPosition(num12, out position7) && position7.m_segment == position6.m_segment && info3.m_lanes.Length > position7.m_lane)
				{
					float position8 = info3.m_lanes[position6.m_lane].m_position;
					float position9 = info3.m_lanes[position7.m_lane].m_position;
					if (Mathf.Abs(position8 - position9) < 4f)
					{
						num14 = PathManager.GetLaneID(position6);
						num10 = num12;
						position6 = position7;
						num11 = num13;
					}
				}
				NetInfo.LaneType laneType = info3.m_lanes[position6.m_lane].m_laneType;
				uint laneID = PathManager.GetLaneID(position6);
				float num15 = (!info3.m_lanes[position6.m_lane].m_useTerrainHeight) ? 0f : 1f;
				bool flag2 = false;
				ushort startNode = instance2.m_segments.m_buffer[position.m_segment].m_startNode;
				ushort endNode = instance2.m_segments.m_buffer[position.m_segment].m_endNode;
				ushort startNode2 = instance2.m_segments.m_buffer[position6.m_segment].m_startNode;
				ushort endNode2 = instance2.m_segments.m_buffer[position6.m_segment].m_endNode;
				if (startNode2 != startNode && startNode2 != endNode && endNode2 != startNode && endNode2 != endNode)
				{
					uint lane = instance2.m_nodes.m_buffer[startNode].m_lane;
					uint lane2 = instance2.m_nodes.m_buffer[startNode2].m_lane;
					uint lane3 = instance2.m_nodes.m_buffer[endNode].m_lane;
					uint lane4 = instance2.m_nodes.m_buffer[endNode2].m_lane;
					if (lane != laneID && lane2 != num4 && lane3 != laneID && lane4 != num4 && (num14 == 0 || (lane != num14 && lane3 != num14)))
					{
						InvalidPath(instanceID, ref citizenData);
						return vector;
					}
					if (((instance2.m_nodes.m_buffer[startNode].m_flags | instance2.m_nodes.m_buffer[endNode].m_flags) & NetNode.Flags.Disabled) == NetNode.Flags.None && ((instance2.m_nodes.m_buffer[startNode2].m_flags | instance2.m_nodes.m_buffer[endNode2].m_flags) & NetNode.Flags.Disabled) != 0)
					{
						InvalidPath(instanceID, ref citizenData);
						return vector;
					}
					flag2 = true;
				}
				if ((laneType & (NetInfo.LaneType.PublicTransport | NetInfo.LaneType.EvacuationTransport)) != 0)
				{
					citizenData.m_flags |= CitizenInstance.Flags.WaitingTransport;
					citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
					citizenData.m_waitCounter = 0;
					if (num11 != citizenData.m_path)
					{
						Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
					}
					citizenData.m_pathPositionIndex = (byte)(num10 << 1);
					citizenData.m_lastPathOffset = position6.m_offset;
					citizenData.m_flags = ((citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition | CitizenInstance.Flags.OnBikeLane)) | info2.m_setCitizenFlags);
					if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) != 0)
					{
						if (citizenData.m_citizen != 0)
						{
							Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, 0, 0u);
						}
						citizenData.m_flags &= ~CitizenInstance.Flags.RidingBicycle;
					}
					return vector;
				}
				if ((laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle)) != 0)
				{
					if ((info3.m_lanes[position6.m_lane].m_vehicleType & VehicleInfo.VehicleType.Bicycle) == VehicleInfo.VehicleType.None)
					{
						if (num11 != citizenData.m_path)
						{
							Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
						}
						citizenData.m_pathPositionIndex = (byte)(num10 << 1);
						citizenData.m_lastPathOffset = position6.m_offset;
						if (!SpawnVehicle(instanceID, ref citizenData, position6))
						{
							InvalidPath(instanceID, ref citizenData);
						}
						return vector;
					}
					if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None && !SpawnBicycle(instanceID, ref citizenData, position6))
					{
						InvalidPath(instanceID, ref citizenData);
						return vector;
					}
					citizenData.m_flags |= CitizenInstance.Flags.OnBikeLane;
				}
				else
				{
					if (laneType != NetInfo.LaneType.Pedestrian)
					{
						InvalidPath(instanceID, ref citizenData);
						return vector;
					}
					citizenData.m_flags &= ~CitizenInstance.Flags.OnBikeLane;
					if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) != 0 && (instance2.m_segments.m_buffer[position6.m_segment].m_flags & NetSegment.Flags.BikeBan) != 0)
					{
						if (citizenData.m_citizen != 0)
						{
							Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, 0, 0u);
						}
						citizenData.m_flags &= ~CitizenInstance.Flags.RidingBicycle;
					}
				}
				PathUnit.CalculatePathPositionOffset(laneID, vector, out byte offset);
				if (position6.m_segment != position.m_segment)
				{
					if ((instance2.m_segments.m_buffer[position6.m_segment].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) != 0)
					{
						InvalidPath(instanceID, ref citizenData);
						return vector;
					}
					Bezier3 bezier = default(Bezier3);
					instance2.m_lanes.m_buffer[num4].CalculatePositionAndDirection((float)(int)position.m_offset * 0.003921569f, out bezier.a, out Vector3 direction2);
					instance2.m_lanes.m_buffer[laneID].CalculatePositionAndDirection((float)(int)offset * 0.003921569f, out bezier.d, out Vector3 direction3);
					if (position.m_offset == 0)
					{
						direction2 = -direction2;
					}
					if (offset < position6.m_offset)
					{
						direction3 = -direction3;
					}
					direction2.Normalize();
					direction3.Normalize();
					NetSegment.CalculateMiddlePoints(bezier.a, direction2, bezier.d, direction3, true, true, out bezier.b, out bezier.c, out float distance);
					if (distance >= 1f)
					{
						//Modified
						if (distance > 96f)
						{
							//Modified done
							InvalidPath(instanceID, ref citizenData);
							return vector;
						}
						if (citizenData.m_lastPathOffset == 0 && !CheckSegmentChange(instanceID, ref citizenData, position, position6, position.m_offset, offset, bezier))
						{
							Vector3 b = Vector3.Cross(Vector3.up, direction2).normalized * d;
							return bezier.a + b;
						}
						float min = Mathf.Min(bezier.a.y, bezier.d.y);
						float max = Mathf.Max(bezier.a.y, bezier.d.y);
						bezier.b.y = Mathf.Clamp(bezier.b.y, min, max);
						bezier.c.y = Mathf.Clamp(bezier.c.y, min, max);
						float width2 = info3.m_lanes[position6.m_lane].m_width;
						while (citizenData.m_lastPathOffset < 255)
						{
							float num16 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3)vector - frameData.m_position);
							int num17 = (!(num16 < 0f)) ? (8 + Mathf.CeilToInt(num16 * 256f / (distance + 1f))) : 8;
							citizenData.m_lastPathOffset = (byte)Mathf.Min(citizenData.m_lastPathOffset + num17, 255);
							float num18 = (float)(int)citizenData.m_lastPathOffset * 0.003921569f;
							vector = bezier.Position(num18);
							vector.w = num6 + (num15 - num6) * num18;
							d = Mathf.Max(0f, Mathf.Lerp(width, width2, num18) - 1f) * num5;
							Vector3 rhs = bezier.Tangent(num18);
							Vector3 vector3 = Vector3.Cross(Vector3.up, rhs).normalized * d;
							vector.x += vector3.x;
							vector.z += vector3.z;
							num = VectorUtils.LengthSqrXZ((Vector3)vector - frameData.m_position);
							if (num >= minSqrDistance)
							{
								CitizenInstance.Flags flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition);
								flags |= (info2.m_setCitizenFlags & info3.m_setCitizenFlags);
								flags |= ((info2.m_setCitizenFlags | info3.m_setCitizenFlags) & CitizenInstance.Flags.Transition);
								if ((flags & CitizenInstance.Flags.Underground) == CitizenInstance.Flags.None && ((info2.m_setCitizenFlags | info3.m_setCitizenFlags) & CitizenInstance.Flags.Underground) != 0)
								{
									flags |= CitizenInstance.Flags.Transition;
								}
								citizenData.m_flags = flags;
								return vector;
							}
							if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
							{
								citizenData.m_flags |= CitizenInstance.Flags.OnPath;
								if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None && (instance2.m_segments.m_buffer[position6.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None)
								{
									SpawnBicycle(instanceID, ref citizenData, position);
								}
							}
						}
					}
				}
				else if (laneID != num4)
				{
					int num19 = (position.m_offset >= 128) ? 255 : 0;
					int num20 = (offset >= 128) ? 255 : 0;
					instance2.m_lanes.m_buffer[num4].CalculatePositionAndDirection((float)num19 * 0.003921569f, out segment.a, out Vector3 direction4);
					switch (num19)
					{
						case 0:
							segment.a -= direction4.normalized * 1.5f;
							break;
						case 255:
							segment.a += direction4.normalized * 1.5f;
							break;
					}
					instance2.m_lanes.m_buffer[laneID].CalculatePositionAndDirection((float)num20 * 0.003921569f, out segment.b, out Vector3 direction5);
					switch (num20)
					{
						case 0:
							segment.b -= direction5.normalized * 1.5f;
							break;
						case 255:
							segment.b += direction5.normalized * 1.5f;
							break;
					}
					b2 = Vector3.Cross(Vector3.up, segment.b - segment.a).normalized * num5;
					if (citizenData.m_lastPathOffset == 0 && num4 != laneID && !CheckLaneChange(instanceID, ref citizenData, position, position6, num19, num20))
					{
						break;
					}
					float num21 = Mathf.Abs(info2.m_lanes[position.m_lane].m_position - info3.m_lanes[position6.m_lane].m_position);
					float num22 = (info2.m_halfWidth - info2.m_pavementWidth) / Mathf.Max(1f, num21);
					float num23 = info2.m_surfaceLevel - info2.m_lanes[position.m_lane].m_verticalOffset;
					while (citizenData.m_lastPathOffset < 255)
					{
						float num24 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3)vector - frameData.m_position);
						int num25 = (!(num24 < 0f)) ? (8 + Mathf.CeilToInt(num24 * 256f / (num21 + 1f))) : 8;
						citizenData.m_lastPathOffset = (byte)Mathf.Min(citizenData.m_lastPathOffset + num25, 255);
						float num26 = (float)(int)citizenData.m_lastPathOffset * 0.003921569f;
						vector = segment.Position(num26) + b2;
						vector.w = num6 + (num15 - num6) * num26;
						if (Mathf.Abs(num26 - 0.5f) < num22)
						{
							vector.y += num23;
						}
						num = VectorUtils.LengthSqrXZ((Vector3)vector - frameData.m_position);
						if (num >= minSqrDistance)
						{
							CitizenInstance.Flags flags2 = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition);
							flags2 |= (info2.m_setCitizenFlags & info3.m_setCitizenFlags);
							flags2 |= ((info2.m_setCitizenFlags | info3.m_setCitizenFlags) & CitizenInstance.Flags.Transition);
							if ((flags2 & CitizenInstance.Flags.Underground) == CitizenInstance.Flags.None && ((info2.m_setCitizenFlags | info3.m_setCitizenFlags) & CitizenInstance.Flags.Underground) != 0)
							{
								flags2 |= CitizenInstance.Flags.Transition;
							}
							citizenData.m_flags = flags2;
							return vector;
						}
						if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
						{
							citizenData.m_flags |= CitizenInstance.Flags.OnPath;
							if ((instance2.m_segments.m_buffer[position6.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None && (citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None)
							{
								SpawnBicycle(instanceID, ref citizenData, position);
							}
						}
					}
				}
				if ((instance2.m_segments.m_buffer[position6.m_segment].m_flags & NetSegment.Flags.Untouchable) != 0 && ((instance2.m_segments.m_buffer[position.m_segment].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.None || flag2))
				{
					ushort num27 = NetSegment.FindOwnerBuilding(position6.m_segment, 363f);
					if (num27 != 0)
					{
						ushort num28 = 0;
						if ((instance2.m_segments.m_buffer[position.m_segment].m_flags & NetSegment.Flags.Untouchable) != 0)
						{
							num28 = NetSegment.FindOwnerBuilding(position.m_segment, 363f);
						}
						if (num27 != num28)
						{
							BuildingManager instance3 = Singleton<BuildingManager>.instance;
							BuildingInfo info4 = instance3.m_buildings.m_buffer[num27].Info;
							InstanceID itemID = default(InstanceID);
							itemID.CitizenInstance = instanceID;
							info4.m_buildingAI.EnterBuildingSegment(num27, ref instance3.m_buildings.m_buffer[num27], position6.m_segment, position6.m_offset, itemID);
						}
					}
				}
				if (num11 != citizenData.m_path)
				{
					Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
				}
				citizenData.m_pathPositionIndex = (byte)(num10 << 1);
				citizenData.m_lastPathOffset = offset;
				position = position6;
				num4 = laneID;
			}
			return segment.a + b2;
		}
	}
}

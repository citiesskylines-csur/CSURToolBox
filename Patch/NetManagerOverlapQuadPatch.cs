using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
using CSURToolBox.Util;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetManagerOverlapQuadPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetManager).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.CollisionType), typeof(ItemClass.Layer), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }, null);
        }
        public static bool Prefix(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, ItemClass.Layer requireLayers, ItemClass.Layer forbidLayers, ushort ignoreNode1, ushort ignoreNode2, ushort ignoreSegment, ulong[] segmentMask, ref bool __result)
        {
			NetManager instance = Singleton<NetManager>.instance;
			Vector2 vector = quad.Min();
			Vector2 vector2 = quad.Max();
			int num = Mathf.Max((int)((vector.x - 64f) / 64f + 135f), 0);
			int num2 = Mathf.Max((int)((vector.y - 64f) / 64f + 135f), 0);
			int num3 = Mathf.Min((int)((vector2.x + 64f) / 64f + 135f), 269);
			int num4 = Mathf.Min((int)((vector2.y + 64f) / 64f + 135f), 269);
			ushort num5 = 0;
			ushort num6 = 0;
			if (ignoreSegment != 0)
			{
				ushort startNode = instance.m_segments.m_buffer[ignoreSegment].m_startNode;
				ushort endNode = instance.m_segments.m_buffer[ignoreSegment].m_endNode;
				NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
				NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
				if ((flags & NetNode.Flags.Middle) != 0)
				{
					num5 = startNode;
				}
				if ((flags2 & NetNode.Flags.Middle) != 0)
				{
					num6 = endNode;
				}
			}
			bool result = false;
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num; j <= num3; j++)
				{
					ushort num7 = instance.m_segmentGrid[i * 270 + j];
					int num8 = 0;
					while (num7 != 0)
					{
						if (num7 != ignoreSegment && ((long)instance.m_updatedSegments[num7 >> 6] & (1L << (int)num7)) == 0)
						{
							NetInfo info = instance.m_segments.m_buffer[num7].Info;
							if ((object)info != null)
							{
								ItemClass.Layer collisionLayers = info.m_netAI.GetCollisionLayers();
								if ((object)info != null && (requireLayers == ItemClass.Layer.None || (collisionLayers & requireLayers) != 0) && (collisionLayers & forbidLayers) == ItemClass.Layer.None)
								{
									ushort startNode2 = instance.m_segments.m_buffer[num7].m_startNode;
									ushort endNode2 = instance.m_segments.m_buffer[num7].m_endNode;
									if (startNode2 != ignoreNode1 && startNode2 != ignoreNode2 && startNode2 != num5 && startNode2 != num6 && endNode2 != ignoreNode1 && endNode2 != ignoreNode2 && endNode2 != num5 && endNode2 != num6)
									{
										Vector3 position = instance.m_nodes.m_buffer[startNode2].m_position;
										Vector3 position2 = instance.m_nodes.m_buffer[endNode2].m_position;
										// NON-STOCK CODE STARTS
										if (CSURUtil.IsCSUROffset(instance.m_nodes.m_buffer[startNode2].Info))
										{
											bool lht = false;
											if (instance.m_nodes.m_buffer[startNode2].CountSegments() != 0)
											{
												float collisionHalfWidth = Mathf.Max(3f, (instance.m_nodes.m_buffer[startNode2].Info.m_halfWidth + instance.m_nodes.m_buffer[startNode2].Info.m_pavementWidth) / 2f);
												NetSegment mysegment = CSURUtil.GetSameInfoSegment(instance.m_nodes.m_buffer[startNode2]);
												Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, instance.m_nodes.m_buffer[startNode2]) ? mysegment.m_startDirection : -mysegment.m_endDirection;
												if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
												// normal to the right hand side
												Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
												position = position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
											}
										}
										if (CSURUtil.IsCSUROffset(instance.m_nodes.m_buffer[endNode2].Info))
										{
											bool lht = false;
											if (instance.m_nodes.m_buffer[endNode2].CountSegments() != 0)
											{
												float collisionHalfWidth = Mathf.Max(3f, (instance.m_nodes.m_buffer[endNode2].Info.m_halfWidth + instance.m_nodes.m_buffer[endNode2].Info.m_pavementWidth) / 2f);
												NetSegment mysegment = CSURUtil.GetSameInfoSegment(instance.m_nodes.m_buffer[endNode2]);
												Vector3 direction = CSURUtil.CheckNodeEq(mysegment.m_startNode, instance.m_nodes.m_buffer[endNode2]) ? mysegment.m_startDirection : -mysegment.m_endDirection;
												if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
												// normal to the right hand side
												Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
												position2 = position2 + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
											}
										}
										// NON-STOCK CODE ENDS
										float num9 = Mathf.Max(Mathf.Max(vector.x - 64f - position.x, vector.y - 64f - position.z), Mathf.Max(position.x - vector2.x - 64f, position.z - vector2.y - 64f));
										float num10 = Mathf.Max(Mathf.Max(vector.x - 64f - position2.x, vector.y - 64f - position2.z), Mathf.Max(position2.x - vector2.x - 64f, position2.z - vector2.y - 64f));
										if ((num9 < 0f || num10 < 0f) && instance.m_segments.m_buffer[num7].OverlapQuad(num7, quad, minY, maxY, collisionType))
										{
											if (segmentMask == null)
											{
												__result = true;
												return false;
											}
											segmentMask[num7 >> 6] |= (ulong)(1L << (int)num7);
											result = true;
										}
									}
								}
							}
						}
						num7 = instance.m_segments.m_buffer[num7].m_nextGridSegment;
						if (++num8 >= 36864)
						{
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
			__result = result;
			return false;
		}
	}
}

using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CSURToolBox.CustomData
{
	public static class CustomBuilding
	{
		public static ushort FindParentNode(ref Building buiding, ushort buildingID)
		{
			NetManager instance = Singleton<NetManager>.instance;
			int num = Mathf.Max((int)((buiding.m_position.x - 16f) / 64f + 135f), 0);
			int num2 = Mathf.Max((int)((buiding.m_position.z - 16f) / 64f + 135f), 0);
			int num3 = Mathf.Min((int)((buiding.m_position.x + 16f) / 64f + 135f), 269);
			int num4 = Mathf.Min((int)((buiding.m_position.z + 16f) / 64f + 135f), 269);
			//Fix out of bounds
			num = Mathf.Min(num, 269);
			num2 = Mathf.Min(num2, 269);
			//num3 num4 is unlikely out of bounds, however, also fix them 
			num3 = Mathf.Max(num3, 0);
			num4 = Mathf.Max(num4, 0);
			//End
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num; j <= num3; j++)
				{
					ushort num5 = instance.m_nodeGrid[i * 270 + j];
					int num6 = 0;
					while (num5 != 0)
					{
						if (instance.m_nodes.m_buffer[(int)num5].m_building == buildingID)
						{
							return num5;
						}
						num5 = instance.m_nodes.m_buffer[(int)num5].m_nextGridNode;
						if (++num6 >= 32768)
						{
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
			return 0;
		}
	}
}

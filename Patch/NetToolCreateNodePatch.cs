using ColossalFramework;
using ColossalFramework.UI;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetToolCreateNodePatch
	{
		public static ushort[] needUpdateSegment = new ushort[32768];
		public static ushort needUpdateSegmentCount = 0;
		public static bool needUpdateSegmentFlag = false;
		public static MethodBase TargetMethod()
        {
            return typeof(NetTool).GetMethod("CreateNode", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetInfo), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(FastList<NetTool.NodePosition>), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(ushort), typeof(ushort).MakeByRefType(), typeof(ushort).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }, null);
        }
        public static void Postfix(ref ushort segment, ref ToolBase.ToolErrors __result)
        {
			bool flag = !UIView.IsInsideUI() && Cursor.visible;
			ToolBase currentTool = ToolsModifierControl.GetCurrentTool<ToolBase>();
			if (OptionUI.fixLargeJunction)
			{
				if (flag)
				{
					if (__result == ToolBase.ToolErrors.None)
					{
						if (currentTool is NetTool)
						{
							NetTool netTool = currentTool as NetTool;
							if (netTool.m_mode == NetTool.Mode.Upgrade)
							{
								if (segment != 0)
								{
									needUpdateSegment[needUpdateSegmentCount] = segment;
									needUpdateSegmentCount++;
									needUpdateSegmentFlag = true;
									DebugLog.LogToFileOnly($"Later update segment = {segment}");
									//ColossalFramework.Singleton<NetManager>.instance.UpdateSegment(segment);
								}
							}
						}
					}
				}
			}
        }
	}
}

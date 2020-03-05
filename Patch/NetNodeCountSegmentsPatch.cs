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
using System.Threading;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetNodeCountSegmentsPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetNode).GetMethod("CountSegments", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {}, null);
        }
        public static void Postfix(ref NetNode __instance, ref int __result)
        {
            if (OptionUI.noJunction)
            {
                for (int j = 0; j < 8; j++)
                {
                    ushort segmentID = __instance.GetSegment(j);
                    if (segmentID != 0)
                    {
                        NetInfo asset = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].Info;
                        if (asset != null)
                        {
                            if (asset.m_netAI is RoadAI)
                            {
                                if (CSURUtil.IsCSURNoJunction(asset))
                                {
                                    __result = 2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

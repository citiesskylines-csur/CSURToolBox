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
    public static class NetNodeCalculateNodePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetNode).GetMethod("CalculateNode", BindingFlags.Public | BindingFlags.Instance);
        }
        public static void Postfix(ref NetNode __instance)
        {
            if (OptionUI.noJunction)
            {
                NetInfo asset = __instance.Info;
                if (asset != null)
                {
                    if (asset.m_netAI is RoadAI)
                    {
                        if (CSURUtil.IsCSURNoJunction(asset))
                        {
                            if (__instance.CountSegments() == 2)
                            {
                                __instance.m_flags &= ~NetNode.Flags.Junction;
                            }
                        }
                    }
                }
            }
        }
    }
}

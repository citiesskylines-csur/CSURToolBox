using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.CustomData;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetNodeRayCastPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetNode).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null);
        }
        public static bool Prefix(ref NetNode __instance, Segment3 ray, float snapElevation, out float t, out float priority, ref bool __result)
        {
            NetInfo info = __instance.Info;
            // NON-STOCK CODE STARTS
            // NON-STOCK CODE STARTS
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(info.m_netAI.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(info.m_netAI.m_info) && !IsCSURSLane)
            {
                __result = CustomNetNode.RayCastNodeMasked(ref __instance, ray, snapElevation, false, out t, out priority);
                return false;
            }
            // NON-STOCK CODE ENDS
            if ((__instance.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Outside)) == NetNode.Flags.None)
            {
                float num = (float)__instance.m_elevation + info.m_netAI.GetSnapElevation();
                float t2;
                if (info.m_netAI.IsUnderground())
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
                }
                else
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
                }
                float collisionHalfWidth = Mathf.Max(3f, info.m_netAI.GetCollisionHalfWidth());
                float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
                if (Segment1.Intersect(ray.a.y, ray.b.y, __instance.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), __instance.m_position);
                    // NON-STOCK CODE STARTS
                    if (IsCSURSLane)
                    {
                        bool lht = false;
                        NetManager instance = Singleton<NetManager>.instance;
                        NetSegment mysegment = CSURUtil.GetSameInfoSegment(__instance);
                        bool isStart = CSURUtil.CheckNodeEq(mysegment.m_startNode, __instance);
                        Vector3 direction = isStart ? mysegment.m_startDirection : -mysegment.m_endDirection;
                        //Debug.Log(direction);
                        if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                        Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                        float vehicleLaneNum = CSURUtil.CountCSURSVehicleLanes(info);
                        float otherLaneNum = CSURUtil.CountCSURSOtherLanes(info);
                        float laneNum = otherLaneNum + vehicleLaneNum;
                        startOffset = startOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                        endOffset = endOffset * 3.75f - laneNum * 1.875f + 1.875f + otherLaneNum * 3.75f;
                        var Offset = isStart ? startOffset : endOffset;
                        Vector3 trueNodeCenter = __instance.m_position + (lht ? -Offset : Offset) * normal;
                        num3 = Vector3.Distance(ray.Position(t), trueNodeCenter);
                    }
                    // NON-STOCK CODE ENDS
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        __result = true;
                        return false;
                    }
                }
            }
            t = 0f;
            priority = 0f;
            __result = false;
            return false;
        }
    }
}

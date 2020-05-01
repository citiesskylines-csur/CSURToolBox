using ColossalFramework;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class RoadAICreateZoneBlocksPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(RoadAI).GetMethod("CreateZoneBlocks");
        }

        static FieldInfo f_halfWidth =
            typeof(NetInfo).GetField(nameof(NetInfo.m_halfWidth)) ??
            throw new Exception("f_halfWidth is null");

        static MethodInfo mChangeHalfWidth = AccessTools.DeclaredMethod(
            typeof(RoadAICreateZoneBlocksPatch), nameof(ChangeHalfWidth)) ??
            throw new Exception("mChangeHalfWidth is null");

        static MethodInfo targetMethod_ = TargetMethod() as MethodInfo;

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction ldarg_segment = CSURUtil.GetLDArg(targetMethod_, "segment"); // push segment into stack,
            CodeInstruction call_ChangeHalfWidth = new CodeInstruction(OpCodes.Call, mChangeHalfWidth);

            int n = 0;
            foreach (var innstruction in instructions)
            {
                yield return innstruction;
                bool is_ldfld_f_halfWidth =
                    innstruction.opcode == OpCodes.Ldfld && innstruction.operand == f_halfWidth;
                if (is_ldfld_f_halfWidth)
                {
                    n++;
                    yield return ldarg_segment;
                    yield return call_ChangeHalfWidth;
                }
            }

            DebugLog.LogToFileOnly($"TRANSPILER CreateZoneBlocksPatch: Successfully patched RoadAI.CreateZoneBlocks(). " +
                $"found {n} instances of Ldfld NetInfo.m_halfWidth");
            yield break;
        }

        public static float ChangeHalfWidth(float halfWidth0, ushort segment)
        {
            var data = Singleton<NetManager>.instance.m_segments.m_buffer[segment];
            if (OptionUI.alignZone)
            {
                if (CSURUtil.IsCSUR(data.Info))
                {
                    if (data.Info.m_halfWidth < 9f)
                    {
                        return 8f;
                    }
                    else if (data.Info.m_halfWidth < 17f)
                    {
                        return 16f;
                    }
                    else if (data.Info.m_halfWidth < 25f)
                    {
                        return 24f;
                    }
                    else if (data.Info.m_halfWidth < 33f)
                    {
                        return 32f;
                    }
                    else if (data.Info.m_halfWidth < 41f)
                    {
                        return 40f;
                    }
                }
            }
            return halfWidth0;
        }
    }
}

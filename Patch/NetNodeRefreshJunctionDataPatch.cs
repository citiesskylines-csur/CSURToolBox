using System;
using System.Reflection;
using CSURToolBox.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetNodeRefreshJunctionDataPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(NetNode).GetMethod("RefreshJunctionData", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetInfo), typeof(uint) }, null);
        }

        static FieldInfo f_minCornerOffset =
        typeof(NetInfo).GetField(nameof(NetInfo.m_minCornerOffset)) ??
        throw new Exception("f_minCornerOffset is null");

        static MethodInfo mGetMinCornerOffset = AccessTools.DeclaredMethod(
            typeof(CSURUtil), nameof(CSURUtil.GetMinCornerOffset)) ??
            throw new Exception("mGetMinCornerOffset is null");

        static MethodInfo targetMethod_ = TargetMethod() as MethodInfo;

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction ldarg_nodeID = CSURUtil.GetLDArg(targetMethod_, "nodeID"); // push nodeID into stack,
            CodeInstruction call_GetMinCornerOffset = new CodeInstruction(OpCodes.Call, mGetMinCornerOffset);

            int n = 0;
            foreach (var innstruction in instructions)
            {
                yield return innstruction;
                bool is_ldfld_minCornerOffset =
                    innstruction.opcode == OpCodes.Ldfld && innstruction.operand == f_minCornerOffset;
                if (is_ldfld_minCornerOffset)
                {
                    n++;
                    yield return ldarg_nodeID;
                    yield return call_GetMinCornerOffset;
                }
            }

            DebugLog.LogToFileOnly($"TRANSPILER RefreshJunctionData: Successfully patched NetNode.RefreshJunctionData(). " +
                $"found {n} instances of Ldfld NetInfo.m_minCornerOffset");
            yield break;
        }
    }
}

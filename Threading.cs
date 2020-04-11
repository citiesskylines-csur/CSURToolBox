using ColossalFramework.Math;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection;
using CSURToolBox.Util;
using ColossalFramework.UI;
using CSURToolBox.CustomData;
using CSURToolBox.CustomAI;
using HarmonyLib;

namespace CSURToolBox
{
    public class Threading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public static Assembly MoveIt = null;
        public const int HarmonyPatchNum = 15;

        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame || Loader.CurrentLoadMode == LoadMode.NewMap || Loader.CurrentLoadMode == LoadMode.LoadMap || Loader.CurrentLoadMode == LoadMode.NewAsset || Loader.CurrentLoadMode == LoadMode.LoadAsset)
            {
                if (CSURToolBox.IsEnabled)
                {
                    CheckDetour();
                }
            }
        }

        public void DetourAfterLoad()
        {
            //This is for Detour RealCity method
            DebugLog.LogToFileOnly("Init DetourAfterLoad");
            bool detourFailed = false;

            if (Loader.isMoveItRunning)
            {
                Assembly MoveIt = Assembly.Load("MoveIt");
                //1
                //private static bool RayCastNode(ushort nodeid, ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
                DebugLog.LogToFileOnly("Detour MoveIt::MoveItTool.RayCastNode calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(MoveIt.GetType("MoveIt.MoveItTool").GetMethod("RayCastNode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(NetNode).MakeByRefType(), typeof(ColossalFramework.Math.Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null),
                                           typeof(CustomNetNode).GetMethod("MoveItRayCastNode", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(NetNode).MakeByRefType(), typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour MoveIt::MoveItTool.RayCastNode");
                    detourFailed = true;
                }

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("DetourAfterLoad failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("DetourAfterLoad successful");
                }
            }

            if (Loader.is1637663252 || Loader.is1806963141)
            {
                DebugLog.LogToFileOnly("Detour LaneConnectorTool::CheckSegmentsTurningAngle calls");
                try
                {
                    // Traffic manager is fixed in version 11.1.1 and higher
                    // TODO delete this and NewLaneConnectorTool.cs when TMPE 11.0 [STABLE] has been depricated.
                    Version TMPE_Version = Assembly.Load("TrafficManager").GetName().Version;
                    if(TMPE_Version < new Version(11, 1, 1)) {
                        Loader.Detours.Add(new Loader.Detour(Assembly.Load("TrafficManager").GetType("TrafficManager.UI.SubTools.LaneConnectorTool").GetMethod("CheckSegmentsTurningAngle", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool), typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null),
                                           typeof(NewLaneConnectorTool).GetMethod("CheckSegmentsTurningAngle", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool), typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null)));
                    }
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour LaneConnectorTool::CheckSegmentsTurningAngle");
                    detourFailed = true;
                }

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("DetourAfterLoad failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("DetourAfterLoad successful");
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime)
            { 
                if (Loader.DetourInited && Loader.HarmonyDetourInited)
                {
                    isFirstTime = false;
                    DetourAfterLoad();
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");
                    List<string> list = new List<string>();
                    foreach (Loader.Detour current in Loader.Detours)
                    {
                        if (!RedirectionHelper.IsRedirected(current.OriginalMethod, current.CustomMethod))
                        {
                            list.Add(string.Format("{0}.{1} with {2} parameters ({3})", new object[]
                            {
                    current.OriginalMethod.DeclaringType.Name,
                    current.OriginalMethod.Name,
                    current.OriginalMethod.GetParameters().Length,
                    current.OriginalMethod.DeclaringType.AssemblyQualifiedName
                            }));
                        }
                    }
                    DebugLog.LogToFileOnly(string.Format("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Detours checked. Result: {0} missing detours", list.Count));
                    if (list.Count > 0)
                    {
                        string error = "CSURToolBox detected an incompatibility with another mod! You can continue playing but it's NOT recommended. CSURToolBox will not work as expected. Send CSURToolBox.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        string text = "The following methods were overriden by another mod:";
                        foreach (string current2 in list)
                        {
                            text += string.Format("\n\t{0}", current2);
                        }
                        DebugLog.LogToFileOnly(text);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", text, true);
                    }

                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "CSURToolBox HarmonyDetourInit is failed, Send CSURToolBox.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                    }
                    else
                    {
                        var harmony = new Harmony(HarmonyDetours.Id);
                        var methods = harmony.GetPatchedMethods();
                        int i = 0;
                        foreach (var method in methods)
                        {
                            var info = Harmony.GetPatchInfo(method);
                            if (info.Owners?.Contains(HarmonyDetours.Id) == true)
                            {
                                DebugLog.LogToFileOnly($"Harmony patch method = {method.FullDescription()}");
                                if (info.Prefixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PreFix");
                                }
                                if (info.Postfixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PostFix");
                                }
                                i++;
                            }
                        }

                        if (i != HarmonyPatchNum)
                        {
                            string error = $"CSURToolBox HarmonyDetour Patch Num is {i}, Right Num is {HarmonyPatchNum} Send CSURToolBox.txt to Author.";
                            DebugLog.LogToFileOnly(error);
                            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                        }
                    }
                }
            }
        }
    }
}

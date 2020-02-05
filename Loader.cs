using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using CSURToolBox.CustomAI;
using CSURToolBox.CustomData;
using CSURToolBox.CustomManager;
using CSURToolBox.Patch;
using CSURToolBox.UI;
using CSURToolBox.Util;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox
{
    public class Loader : LoadingExtensionBase
    {
        public static UIView parentGuiView;
        public static MainUI mainUI;
        public static LoadMode CurrentLoadMode;
        public static bool isGuiRunning = false;
        public static MainButton mainButton;
        public static string m_atlasName = "CSUR_UI";
        public static string m_atlasName1 = "CSUR_UI1";
        public static string m_atlasName2 = "CSUR_UI2";
        public static string m_atlasNameHeader = "CSUR_UI_Header";
        public static string m_atlasNameBg = "CSUR_UI_Bg";
        public static string m_atlasNameNoAsset = "CSUR_UI_NoAssert";
        public static bool m_atlasLoaded;
        public static bool is583429740 = false;
        public static bool is1637663252 = false;
        public static bool is1806963141 = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;
        public static StayInLaneUI stayInLaneUI;
        public static bool Done { get; private set; } // Only one Assets installation throughout the application
        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }

        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static bool isMoveItRunning = false;


        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
            base.OnCreated(loading);
        }
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            CurrentLoadMode = mode;
            if (CSURToolBox.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewMap || mode == LoadMode.LoadMap || mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                {
                    OptionUI.LoadSetting();
                    SetupGui();
                    CheckTMPE();
                    InitDetour();
                    HarmonyInitDetour();
                    if (OptionUI.enablePillar)
                    {
                        InstallPillar();
                    }
                    OptionUI.isDebug = false;
                    if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                    {
                        if (OptionUI.disableZone)
                        {
                            DisableZone();
                        }
                    }
                    RefreshSegment();
                    RefreshNode();
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                    if (mode == LoadMode.NewGame)
                    {
                        //InitData();
                        DebugLog.LogToFileOnly("InitData");
                    }
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            is583429740 = false;
            is1637663252 = false;
            is1806963141 = false;
            if (CurrentLoadMode == LoadMode.LoadGame || CurrentLoadMode == LoadMode.NewGame || CurrentLoadMode == LoadMode.LoadMap || CurrentLoadMode == LoadMode.NewMap || CurrentLoadMode == LoadMode.LoadAsset || CurrentLoadMode == LoadMode.NewAsset)
            {
                if (CSURToolBox.IsEnabled)
                {
                    RevertDetour();
                    HarmonyRevertDetour();
                    if (OptionUI.enablePillar)
                    {
                        RemovePillar();
                    }
                    if (isGuiRunning)
                    {
                        RemoveGui();
                    }
                }
            }
        }

        public static void DataInit()
        {
            for (int i = 0; i < NetSegmentCalculateCornerPatch.segmentOffsetLock.Length; i++)
            {
                NetSegmentCalculateCornerPatch.segmentOffsetLock[i] = false;
                NetSegmentCalculateCornerPatch.segmentOffset[i] = 0f;
            }
        }
        private static void LoadSprites()
        {
            if (SpriteUtilities.GetAtlas(m_atlasName) != null) return;
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            m_atlasLoaded = SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR.png"), m_atlasName);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR1.png"), m_atlasName1);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR2.png"), m_atlasName2);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/UIBG.png"), m_atlasNameBg);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/UITOP.png"), m_atlasNameHeader);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/Notfound.png"), m_atlasNameNoAsset);
            if (m_atlasLoaded)
            {
                var spriteSuccess = true;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(30, 30)), "0P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(34, 2), new Vector2(30, 30)), "0P_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(66, 2), new Vector2(30, 30)), "0P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(98, 2), new Vector2(30, 30)), "1_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "1_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "1_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "1P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "1P_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(258, 2), new Vector2(30, 30)), "1P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(290, 2), new Vector2(30, 30)), "2_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(322, 2), new Vector2(30, 30)), "2_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(354, 2), new Vector2(30, 30)), "2_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(386, 2), new Vector2(30, 30)), "2P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(418, 2), new Vector2(30, 30)), "2P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(452, 2), new Vector2(30, 30)), "3_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(482, 2), new Vector2(30, 30)), "3_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(514, 2), new Vector2(30, 30)), "3P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(546, 2), new Vector2(30, 30)), "3P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(578, 2), new Vector2(30, 30)), "4_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(610, 2), new Vector2(30, 30)), "4_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(642, 2), new Vector2(30, 30)), "4P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(674, 2), new Vector2(30, 30)), "4P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(706, 2), new Vector2(30, 30)), "5_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(738, 2), new Vector2(30, 30)), "5_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(770, 2), new Vector2(30, 30)), "5P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(802, 2), new Vector2(30, 30)), "5P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(834, 2), new Vector2(30, 30)), "6_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(866, 2), new Vector2(30, 30)), "6_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(898, 2), new Vector2(30, 30)), "6P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(930, 2), new Vector2(30, 30)), "6P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(962, 2), new Vector2(30, 30)), "7_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(994, 2), new Vector2(30, 30)), "7_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(1026, 2), new Vector2(30, 30)), "C_C", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(1058, 2), new Vector2(30, 30)), "C_S", m_atlasName)
                             && spriteSuccess;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(30, 30)), "+0", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(34, 2), new Vector2(30, 30)), "+1", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(66, 2), new Vector2(30, 30)), "+2", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(98, 2), new Vector2(30, 30)), "SWAP", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "SWAP_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "SIDEWALK", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "0_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "0", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(258, 2), new Vector2(30, 30)), "COPY", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(290, 2), new Vector2(30, 30)), "COPY_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(322, 2), new Vector2(30, 30)), "UTURN", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(354, 2), new Vector2(30, 30)), "UTURN_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(386, 2), new Vector2(30, 30)), "NOSIDEWALK", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(418, 2), new Vector2(30, 30)), "CLEAR", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(450, 2), new Vector2(30, 30)), "CLEAR_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(482, 2), new Vector2(30, 30)), "BIKE", m_atlasName1)
                             && spriteSuccess;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(60, 50)), "CSUR_BUTTON_S", m_atlasName2)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(64, 2), new Vector2(60, 50)), "CSUR_BUTTON", m_atlasName2)
                             && spriteSuccess;
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(566, 210)), "UIBG", m_atlasNameBg);
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(565, 35)), "UITOP", m_atlasNameHeader);
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(150, 150)), "NOASSET", m_atlasNameNoAsset);
                if (!spriteSuccess) DebugLog.LogToFileOnly("Error: Some sprites haven't been loaded. This is abnormal; you should probably report this to the mod creator.");
            }
            else DebugLog.LogToFileOnly("Error: The texture atlas (provides custom icons) has not loaded. All icons have reverted to text prompts.");
        }

        public void CheckTMPE()
        {
            if ((IsSteamWorkshopItemSubscribed(583429740) && IsSteamWorkshopItemSubscribed(1637663252)) || (IsSteamWorkshopItemSubscribed(1806963141) && IsSteamWorkshopItemSubscribed(1637663252)) || (IsSteamWorkshopItemSubscribed(583429740) && IsSteamWorkshopItemSubscribed(1806963141)))
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", "Can not sub two TM:PE, steamID:583429740 or 1637663252 or 1806963141", true);
            }
            else if (IsSteamWorkshopItemSubscribed(583429740))
            {
                is583429740 = true;
            }
            else if (IsSteamWorkshopItemSubscribed(1637663252))
            {
                is1637663252 = true;
            }
            else if (IsSteamWorkshopItemSubscribed(1806963141))
            {
                is1806963141 = true;
            }

            if (!this.Check3rdPartyModLoaded("TrafficManager", true))
            {
                //UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", "Require TM:PE steamID:583429740 or 1637663252 or 1806963141", true);
                DebugLog.LogToFileOnly("We do not found TMPE");
            }
        }

        public static bool IsSteamWorkshopItemSubscribed(ulong itemId)
        {
            return ContentManagerPanel.subscribedItemsTable.Contains(new PublishedFileId(itemId));
        }
        public static void SetupGui()
        {
            LoadSprites();
            if (m_atlasLoaded)
            {
                parentGuiView = null;
                parentGuiView = UIView.GetAView();

                if (mainUI == null)
                {
                    mainUI = (MainUI)parentGuiView.AddUIComponent(typeof(MainUI));
                }

                if (stayInLaneUI == null)
                {
                    stayInLaneUI = (StayInLaneUI)Loader.parentGuiView.AddUIComponent(typeof(StayInLaneUI));
                }

                SetupMainButton();
                isGuiRunning = true;
            }
        }

        public static void SetupMainButton()
        {
            if (mainButton == null)
            {
                mainButton = (parentGuiView.AddUIComponent(typeof(MainButton)) as MainButton);
            }
            mainButton.Show();
        }

        public static void RemoveGui()
        {
            isGuiRunning = false;
            if (parentGuiView != null)
            {
                parentGuiView = null;
                UnityEngine.Object.Destroy(mainUI);
                UnityEngine.Object.Destroy(mainButton);
                UnityEngine.Object.Destroy(stayInLaneUI);
                mainUI = null;
                mainButton = null;
                stayInLaneUI = null;
            }
        }

        public void InitDetour()
        {
            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                bool detourFailed = false;

                //1
                DebugLog.LogToFileOnly("Detour NetAI::GetCollisionHalfWidth calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetAI).GetMethod("GetCollisionHalfWidth", BindingFlags.Public | BindingFlags.Instance),
                                           typeof(CustomNetAI).GetMethod("CustomGetCollisionHalfWidth", BindingFlags.Public | BindingFlags.Instance)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetAI::GetCollisionHalfWidth");
                    detourFailed = true;
                }
                //2
                //public static bool RayCast(ref NetSegment mysegment, ushort segmentID, Segment3 ray, float snapElevation, bool nameOnly, out float t, out float priority)
                DebugLog.LogToFileOnly("Detour NetSegment::RayCast calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetSegment).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Segment3), typeof(float), typeof(bool), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null),
                                           typeof(CustomNetSegment).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetSegment).MakeByRefType(), typeof(ushort), typeof(Segment3), typeof(float), typeof(bool), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetSegment::RayCast");
                    //detourFailed = true;
                }
                //3
                //public static bool RayCast(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
                DebugLog.LogToFileOnly("Detour NetNode::RayCast calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetNode).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null),
                                           typeof(CustomNetNode).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetNode).MakeByRefType(), typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetNode::RayCast");
                    //detourFailed = true;
                }

                //4
                DebugLog.LogToFileOnly("Detour NetNode::UpdateBuilding calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetNode).GetMethod("UpdateBuilding", BindingFlags.Public | BindingFlags.Instance),
                                           typeof(CustomNetNode).GetMethod("UpdateBuilding", BindingFlags.Public | BindingFlags.Static)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetNode::UpdateBuilding");
                    detourFailed = true;
                }
                //5
                //public bool OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, ItemClass.Layer requireLayers, ItemClass.Layer forbidLayers, ushort ignoreNode1, ushort ignoreNode2, ushort ignoreSegment, ulong[] segmentMask)
                DebugLog.LogToFileOnly("Detour NetManager::OverlapQuad calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetManager).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.CollisionType), typeof(ItemClass.Layer), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }, null),
                                           typeof(CustomNetManager).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.CollisionType), typeof(ItemClass.Layer), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetManager::OverlapQuad");
                    //detourFailed = true;
                }

                //6
                DebugLog.LogToFileOnly("Detour NetSegment::OverlapQuad calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetSegment).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Instance),
                                           typeof(CustomNetSegment).GetMethod("OverlapQuad", BindingFlags.Public | BindingFlags.Static)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetSegment::OverlapQuad");
                    detourFailed = true;
                }
                //7
                DebugLog.LogToFileOnly("Detour Building::FindParentNode calls");
                try
                {
                    Detours.Add(new Detour(typeof(Building).GetMethod("FindParentNode", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort) }, null),
                                           typeof(CustomBuilding).GetMethod("FindParentNode", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Building).MakeByRefType(), typeof(ushort) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour Building::FindParentNode");
                    detourFailed = true;
                }

                isMoveItRunning = CheckMoveItIsLoaded();

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("Detours failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("Detours successful");
                }
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Threading.isFirstTime = true;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
        }

        private bool Check3rdPartyModLoaded(string namespaceStr, bool printAll = false)
        {
            bool thirdPartyModLoaded = false;

            var loadingWrapperLoadingExtensionsField = typeof(LoadingWrapper).GetField("m_LoadingExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ILoadingExtension> loadingExtensions = (List<ILoadingExtension>)loadingWrapperLoadingExtensionsField.GetValue(Singleton<LoadingManager>.instance.m_LoadingWrapper);

            if (loadingExtensions != null)
            {
                foreach (ILoadingExtension extension in loadingExtensions)
                {
                    if (printAll)
                        DebugLog.LogToFileOnly($"Detected extension: {extension.GetType().Name} in namespace {extension.GetType().Namespace}");
                    if (extension.GetType().Namespace == null)
                        continue;

                    var nsStr = extension.GetType().Namespace.ToString();
                    if (namespaceStr.Equals(nsStr))
                    {
                        DebugLog.LogToFileOnly($"The mod '{namespaceStr}' has been detected.");
                        thirdPartyModLoaded = true;
                        break;
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Could not get loading extensions");
            }

            return thirdPartyModLoaded;
        }

        private bool CheckMoveItIsLoaded()
        {
            return this.Check3rdPartyModLoaded("MoveIt", true);
        }

        public void HarmonyInitDetour()
        {
            if (!HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Init harmony detours");
                HarmonyDetours.Apply();
                HarmonyDetourInited = true;
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
                HarmonyDetourInited = false;
                HarmonyDetourFailed = true;
            }
        }

        public void InstallPillar()
        {
            for (uint num = 0u; num < PrefabCollection<NetInfo>.LoadedCount(); num++)
            {
                NetInfo loaded = PrefabCollection<NetInfo>.GetLoaded(num);
                if (CSURUtil.IsCSUR(loaded))
                {
                    RoadBridgeAI elevatedAI = null;
                    if ((loaded.m_netAI is RoadBridgeAI) && (Regex.Match(loaded.name, "Elevated", RegexOptions.IgnoreCase)).Success && (loaded.m_segments.Length != 0))
                        elevatedAI = loaded.m_netAI as RoadBridgeAI;
                    else
                        continue;

                    //Caculate lane num
                    int laneNum = (int)CSURUtil.CountCSURSVehicleLanes(loaded);

                    if (!CSURUtil.IsCSURDual(loaded))
                    {
                        if (Regex.Match(loaded.name, "CSUR-T", RegexOptions.IgnoreCase).Success)
                            laneNum = laneNum - 1;

                        if (laneNum < 0)
                            laneNum = 0;

                        switch (laneNum)
                        {
                            case 0:
                            case 1:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = " + laneNum.ToString());
                                DebugLog.LogToFileOnly("Try to Load pillar Ama S-1_Data");
                                elevatedAI.m_bridgePillarOffset = 0.5f;
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama S-1_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama S-1_Data"); 
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama S-1_Data");
                                break;
                            case 2:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 2");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama S-2_Data");
                                elevatedAI.m_bridgePillarOffset = 1f;
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama S-2_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama S-2_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama S-2_Data");
                                break;
                            case 3:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 3");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama S-3_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama S-3_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama S-3_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama S-3_Data");
                                break;
                            case 4:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 4");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama G-3_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama G-3_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama G-3_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama G-3_Data");
                                break;
                            case 5:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 5");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama G-4_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama G-4_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama G-4_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama G-4_Data");
                                break;
                            default:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = " + laneNum.ToString());
                                DebugLog.LogToFileOnly("Try to Load pillar Ama G-5_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama G-5_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama G-5_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama G-5_Data");
                                break;
                        }
                    }
                    else
                    {
                        /*if (Regex.Match(loaded.name, "CSUR-S", RegexOptions.IgnoreCase).Success)
                            laneNum = laneNum - 1;
                        else if (Regex.Match(loaded.name, "CSUR-T", RegexOptions.IgnoreCase).Success)
                            laneNum = laneNum - 1;
                        else if (Regex.Match(loaded.name, "CSUR-R", RegexOptions.IgnoreCase).Success)
                            laneNum = laneNum - 1;*/

                        if (laneNum < 0)
                            laneNum = 0;

                        //laneNum = laneNum * 2;
                        switch (laneNum)
                        {
                            case 0:
                            case 2:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = " + laneNum.ToString());
                                DebugLog.LogToFileOnly("Try to Load pillar Ama S-2_Data");
                                elevatedAI.m_bridgePillarOffset = 1f;
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama S-2_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama S-2_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama S-2_Data");
                                break;
                            case 4:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 4");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama M-2_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama M-2_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama M-2_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama M-2_Data");
                                break;
                            case 6:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 6");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama M-2_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama M-2_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama M-2_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama M-2_Data");
                                break;
                            case 8:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 8");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama M-4_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama M-4_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama M-4_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama M-4_Data");
                                break;
                            case 10:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = 10");
                                DebugLog.LogToFileOnly("Try to Load pillar Ama G-8DR_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama G-8DR_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama G-8DR_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama G-8DR_Data");
                                break;
                            default:
                                DebugLog.LogToFileOnly(loaded.name.ToString() + "lane num = " + laneNum.ToString());
                                DebugLog.LogToFileOnly("Try to Load pillar Ama G-8DR_Data");
                                if (PrefabCollection<BuildingInfo>.FindLoaded("Ama G-8DR_Data") != null)
                                    elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Ama G-8DR_Data");
                                else
                                    DebugLog.LogToFileOnly("Failed Load pillar Ama G-8DR_Data");
                                break;
                        }
                    }                        
                }
            }
        }

        public void RemovePillar()
        {
            for (uint num = 0u; num < PrefabCollection<NetInfo>.LoadedCount(); num++)
            {
                NetInfo loaded = PrefabCollection<NetInfo>.GetLoaded(num);
                if (CSURUtil.IsCSUROffset(loaded))
                {
                    var roadAI = loaded.m_netAI as RoadAI;
                    RoadBridgeAI elevatedAI = null;
                    if ((loaded.m_netAI is RoadBridgeAI) && (Regex.Match(loaded.name, "Elevated", RegexOptions.IgnoreCase)).Success)
                    {
                        elevatedAI = loaded.m_netAI as RoadBridgeAI;
                    }
                    else
                    {
                        continue;
                    }
                    elevatedAI.m_bridgePillarInfo = null;// PrefabCollection<BuildingInfo>.FindLoaded("CSUR 2DC.Ama S-1_Data");
                    DebugLog.LogToFileOnly("Remove pilla for " + loaded.name.ToString());
                }
            }
        }

        public void RefreshSegment()
        {
            for (ushort i = 0; i < Singleton<NetManager>.instance.m_segments.m_size; i++)
            {
                NetInfo asset = Singleton<NetManager>.instance.m_segments.m_buffer[i].Info;
                if (CSURUtil.IsCSUR(asset))
                {
                    Singleton<NetManager>.instance.UpdateSegment(i);
                }
            }
        }

        public void RefreshNode()
        {
            for (ushort i = 0; i < Singleton<NetManager>.instance.m_nodes.m_size; i++)
            {
                NetInfo asset = Singleton<NetManager>.instance.m_nodes.m_buffer[i].Info;
                if (CSURUtil.IsCSUR(asset))
                {
                    Singleton<NetManager>.instance.UpdateNode(i);
                }
            }
        }

        public static void DisableZone()
        {
            for (uint num = 0u; num < PrefabCollection<NetInfo>.LoadedCount(); num++)
            {
                NetInfo loaded = PrefabCollection<NetInfo>.GetLoaded(num);
                if (CSURUtil.IsCSUROffset(loaded) || CSURUtil.IsCSURLaneOffset(loaded) || CSURUtil.IsCSURExpress(loaded))
                {
                    if (loaded.m_netAI is RoadAI)
                    {
                        var AI = loaded.m_netAI as RoadAI;
                        AI.m_enableZoning = false;
                    }
                }
            }


            for (ushort i = 0; i < Singleton<NetManager>.instance.m_segments.m_size; i++)
            {
                NetInfo loaded = Singleton<NetManager>.instance.m_segments.m_buffer[i].Info;
                var segmentData = Singleton<NetManager>.instance.m_segments.m_buffer[i];
                if (CSURUtil.IsCSUROffset(loaded) || CSURUtil.IsCSURLaneOffset(loaded) || CSURUtil.IsCSURExpress(loaded))
                {
                    if (segmentData.m_blockEndLeft != 0)
                    {
                        ZoneManager.instance.ReleaseBlock(segmentData.m_blockEndLeft);
                        segmentData.m_blockEndLeft = 0;
                    }
                    if (segmentData.m_blockEndRight != 0)
                    {
                        ZoneManager.instance.ReleaseBlock(segmentData.m_blockEndRight);
                        segmentData.m_blockEndRight = 0;
                    }
                    if (segmentData.m_blockStartLeft != 0)
                    {
                        ZoneManager.instance.ReleaseBlock(segmentData.m_blockStartLeft);
                        segmentData.m_blockStartLeft = 0;
                    }
                    if (segmentData.m_blockStartRight != 0)
                    {
                        ZoneManager.instance.ReleaseBlock(segmentData.m_blockStartRight);
                        segmentData.m_blockStartRight = 0;
                    }
                }
            }
        }
    }
}

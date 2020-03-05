using ColossalFramework.UI;
using CSURToolBox.Util;
using ICities;
using System.IO;
using UnityEngine;

namespace CSURToolBox.UI
{
    public class OptionUI : MonoBehaviour
    {
        public static bool isShortCutsToPanel = false;
        public static bool isDebug = false;
        public static int smoothLevel = 1;
        public static bool disableZone = false;
        public static bool disableZoneUpdateAll = false;
        public static bool enablePillar = true;
        public static bool fixLargeJunction = true;
        public static bool alignZone = false;
        public static bool noJunction = false;
        public static void MakeSettings(UIHelperBase helper)
        {
            // tabbing code is borrowed from RushHour mod
            // https://github.com/PropaneDragon/RushHour/blob/release/RushHour/Options/OptionHandler.cs
            LoadSetting();
            UIHelper actualHelper = helper as UIHelper;
            UIComponent container = actualHelper.self as UIComponent;

            UITabstrip tabStrip = container.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(container.width - 20, 40);

            UITabContainer tabContainer = container.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector2(container.width - 20, container.height - tabStrip.height - 20);
            tabStrip.tabPages = tabContainer;

            int tabIndex = 0;
            // Lane_ShortCut

            AddOptionTab(tabStrip, Localization.Get("Lane_ShortCut"));
            tabStrip.selectedIndex = tabIndex;

            UIPanel currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            UIHelper panelHelper = new UIHelper(currentPanel);

            var generalGroup = panelHelper.AddGroup(Localization.Get("Lane_Button_ShortCut")) as UIHelper;
            var panel = generalGroup.self as UIPanel;

            panel.gameObject.AddComponent<OptionsKeymappingLane>();

            var generalGroup1 = panelHelper.AddGroup(Localization.Get("ShortCuts_Control")) as UIHelper;
            generalGroup1.AddCheckbox(Localization.Get("ShortCuts_Control_TIPS"), isShortCutsToPanel, (index) => isShortCutsToPanelEnable(index));

            // Function_ShortCut
            ++tabIndex;

            AddOptionTab(tabStrip, Localization.Get("Function_ShortCut"));
            tabStrip.selectedIndex = tabIndex;

            currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            panelHelper = new UIHelper(currentPanel);

            generalGroup = panelHelper.AddGroup(Localization.Get("Function_Button_ShortCut")) as UIHelper;
            panel = generalGroup.self as UIPanel;

            panel.gameObject.AddComponent<OptionsKeymappingFunction>();

            ++tabIndex;

            AddOptionTab(tabStrip, Localization.Get("Experimental_Function"));
            tabStrip.selectedIndex = tabIndex;

            currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            panelHelper = new UIHelper(currentPanel);
            var generalGroup2 = panelHelper.AddGroup(Localization.Get("Experimental_Function")) as UIHelper;
            generalGroup2.AddCheckbox(Localization.Get("Debug_Mode"), isDebug, (index) => isDebugEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("DisableZone"), disableZone, (index) => isDisableZoneEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("UpdateZone"), disableZoneUpdateAll, (index) => isDisableZoneUpdateAllEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("EnablePillar"), enablePillar, (index) => isEnablePillarEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("AlignZone"), alignZone, (index) => isAlignZoneEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("FixLargeJunction"), fixLargeJunction, (index) => isFixLargeJunctionEnable(index));
            generalGroup2.AddCheckbox(Localization.Get("NOJunction"), noJunction, (index) => isNoJunctionEnable(index));
            generalGroup2.AddDropdown(Localization.Get("Lane_Smooth_Level"), new string[] { Localization.Get("Low"), Localization.Get("Medium"), Localization.Get("High") }, smoothLevel, (index) => GetSmoothLevel(index));
            SaveSetting();
        }
        private static UIButton AddOptionTab(UITabstrip tabStrip, string caption)
        {
            UIButton tabButton = tabStrip.AddTab(caption);

            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            tabButton.textPadding = new RectOffset(10, 10, 10, 10);
            tabButton.autoSize = true;
            tabButton.tooltip = caption;

            return tabButton;
        }

        public static void SaveSetting()
        {
            //save langugae
            FileStream fs = File.Create("CSUR_ToolBox_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.WriteLine(isShortCutsToPanel);
            streamWriter.WriteLine(smoothLevel);
            streamWriter.WriteLine(disableZone);
            streamWriter.WriteLine(disableZoneUpdateAll);
            streamWriter.WriteLine(enablePillar);
            streamWriter.WriteLine(alignZone);
            streamWriter.WriteLine(fixLargeJunction);
            streamWriter.WriteLine(noJunction);
            streamWriter.Flush();
            fs.Close();
        }

        public static void LoadSetting()
        {
            if (File.Exists("CSUR_ToolBox_setting.txt"))
            {
                FileStream fs = new FileStream("CSUR_ToolBox_setting.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                isShortCutsToPanel = (sr.ReadLine() == "True") ? true : false;
                var strLine = sr.ReadLine();
                smoothLevel = (strLine == "2") ? 2 : (strLine == "0") ? 0 : 1;
                disableZone = (sr.ReadLine() == "True") ? true : false;
                disableZoneUpdateAll = (sr.ReadLine() == "True") ? true : false;
                enablePillar = (sr.ReadLine() == "False") ? false : true;
                alignZone = (sr.ReadLine() == "True") ? true : false;
                fixLargeJunction = (sr.ReadLine() == "False") ? false : true;
                noJunction = (sr.ReadLine() == "True") ? true : false;

                sr.Close();
                fs.Close();
            }
        }
        public static void isShortCutsToPanelEnable(bool index)
        {
            isShortCutsToPanel = index;
            SaveSetting();
        }
        public static void GetSmoothLevel(int index)
        {
            smoothLevel = index;
            SaveSetting();
        }

        public static void isDebugEnable(bool index)
        {
            isDebug = index;
            SaveSetting();
        }

        public static void isDisableZoneEnable(bool index)
        {
            disableZone = index;
            if (disableZone)
            {
                if (Loader.CurrentLoadMode == LoadMode.NewGame || Loader.CurrentLoadMode == LoadMode.LoadGame)
                {
                    Loader.DisableZone();
                }
            }
            SaveSetting();
        }

        public static void isDisableZoneUpdateAllEnable(bool index)
        {
            disableZoneUpdateAll = index;
            SaveSetting();
        }

        public static void isEnablePillarEnable(bool index)
        {
            enablePillar = index;
            SaveSetting();
        }

        public static void isAlignZoneEnable(bool index)
        {
            alignZone = index;
            SaveSetting();
        }

        public static void isFixLargeJunctionEnable(bool index)
        {
            fixLargeJunction = index;
            SaveSetting();
        }

        public static void isNoJunctionEnable(bool index)
        {
            noJunction = index;
            SaveSetting();
        }
    }
}
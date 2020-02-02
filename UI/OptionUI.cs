using ColossalFramework.UI;
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
        public static void makeSettings(UIHelperBase helper)
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

            AddOptionTab(tabStrip, "Lane ShortCut");
            tabStrip.selectedIndex = tabIndex;

            UIPanel currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            UIHelper panelHelper = new UIHelper(currentPanel);

            var generalGroup = panelHelper.AddGroup("Lane Button ShortCut") as UIHelper;
            var panel = generalGroup.self as UIPanel;

            panel.gameObject.AddComponent<OptionsKeymappingLane>();

            var generalGroup1 = panelHelper.AddGroup("ShortCuts Control") as UIHelper;
            generalGroup1.AddCheckbox("ShortCuts will be used for ToPanel Button", isShortCutsToPanel, (index) => isShortCutsToPanelEnable(index));
            SaveSetting();

            // Function_ShortCut
            ++tabIndex;

            AddOptionTab(tabStrip, "Function ShortCut");
            tabStrip.selectedIndex = tabIndex;

            currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            panelHelper = new UIHelper(currentPanel);

            generalGroup = panelHelper.AddGroup("Function Button ShortCut") as UIHelper;
            panel = generalGroup.self as UIPanel;

            panel.gameObject.AddComponent<OptionsKeymappingFunction>();

            ++tabIndex;

            AddOptionTab(tabStrip, "Experimental Function");
            tabStrip.selectedIndex = tabIndex;

            currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            panelHelper = new UIHelper(currentPanel);
            var generalGroup2 = panelHelper.AddGroup("Experimental Function") as UIHelper;
            generalGroup2.AddCheckbox("Debug Mode", isShortCutsToPanel, (index) => isDebugEnable(index));
            generalGroup2.AddCheckbox("DisableZone for CSUR Shift Ramp and Transition Road", disableZone, (index) => isDisableZoneEnable(index));
            generalGroup2.AddCheckbox("UpdateZone when load game", disableZoneUpdateAll, (index) => isDisableZoneUpdateAllEnable(index));
            generalGroup2.AddDropdown("lane smooth level", new string[] { "Low", "Medium", "High" }, smoothLevel, (index) => GetSmoothLevel(index));
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
            streamWriter.Flush();
            fs.Close();
        }

        public static void LoadSetting()
        {
            if (File.Exists("CSUR_ToolBox_setting.txt"))
            {
                FileStream fs = new FileStream("CSUR_ToolBox_setting.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string strLine = sr.ReadLine();

                if (strLine == "True")
                {
                    isShortCutsToPanel = true;
                }
                else
                {
                    isShortCutsToPanel = false;
                }

                strLine = sr.ReadLine();

                if (strLine == "2")
                {
                    smoothLevel = 2;
                }
                else if (strLine == "0")
                {
                    smoothLevel = 0;
                }
                else
                {
                    smoothLevel = 1;
                }

                strLine = sr.ReadLine();

                if (strLine == "True")
                {
                    disableZone = true;
                }
                else
                {
                    disableZone = false;
                }

                strLine = sr.ReadLine();

                if (strLine == "True")
                {
                    disableZoneUpdateAll = true;
                }
                else
                {
                    disableZoneUpdateAll = false;
                }
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
    }
}
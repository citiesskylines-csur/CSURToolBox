using ICities;
using System.IO;
using ColossalFramework;
using System;
using CSURToolBox.Util;
using CSURToolBox.UI;
using CitiesHarmony.API;

namespace CSURToolBox
{
    public class CSURToolBox : IUserMod
    {
        public static bool IsEnabled = false;
        public string Name
        {
            get { return "CSUR ToolBox"; }
        }
        public string Description
        {
            get { return "Tool Box for CSUR Road"; }
        }
        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("CSURToolBox.txt");
            fs.Close();
            HarmonyHelper.EnsureHarmonyInstalled();
        }
        public void OnDisabled()
        {
            IsEnabled = false;
        }
        public CSURToolBox()
        {
            try
            {
                if (GameSettings.FindSettingsFileByName("CSURToolBox_SETTING") == null)
                {
                    // Creating setting file 
                    GameSettings.AddSettingsFile(new SettingsFile { fileName = "CSURToolBox_SETTING" });
                }
            }
            catch (Exception)
            {
                DebugLog.LogToFileOnly("Could not load/create the setting file.");
            }
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            OptionUI.MakeSettings(helper);
        }
    }
}

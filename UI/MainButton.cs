using ColossalFramework.UI;
using CSURToolBox;
using CSURToolBox.Util;
using UnityEngine;

namespace CSURToolBox.UI
{
    public class MainButton : UIButton
    {
        private UIComponent MainUITrigger_paneltime;
        //private UIComponent MainUITrigger_chirper;
        private UIComponent MainUITrigger_esc;
        //private UIComponent MainUITrigger_infopanel;
        //private UIComponent MainUITrigger_bottombars;
        private UIDragHandle m_DragHandler;
        private static float tmpX;
        private static float tmpY;
        public static void MainUIToggle()
        {
            if (!Loader.mainUI.isVisible)
            {
                MainUI.refreshOnce = true;
                Loader.mainUI.Show();
            }
            else
            {
                Loader.mainUI.Hide();
            }
        }

        public void MainUIOff()
        {
            if (Loader.mainUI.isVisible && !Loader.mainUI.containsMouse && !containsMouse && MainUITrigger_paneltime != null && !MainUITrigger_paneltime.containsMouse)
            {
                Loader.mainUI.Hide();
            }
        }

        public override void Start()
        {
            name = "MainButton";
            Vector2 resolution = UIView.GetAView().GetScreenResolution();
            var pos = new Vector2((resolution.x - 70f), (resolution.y * 3f / 4f));
            Rect rect = new Rect(pos.x, pos.y, 60, 50);
            ClampRectToScreen(ref rect, resolution);
            DebugLog.LogToFileOnly($"Setting main menu button position to [{pos.x},{pos.y}]");
            absolutePosition = rect.position;
            Invalidate();
            //relativePosition = new Vector3((Loader.parentGuiView.fixedWidth - 70f), (Loader.parentGuiView.fixedHeight / 2 + 100f));
            playAudioEvents = true;
            tmpX = base.relativePosition.x;
            tmpY = base.relativePosition.y;
            atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName2);
            normalBgSprite = "CSUR_BUTTON";
            hoveredBgSprite = "CSUR_BUTTON_S";
            focusedBgSprite = "CSUR_BUTTON_S";
            pressedBgSprite = "CSUR_BUTTON_S";
            //UISprite internalSprite = AddUIComponent<UISprite>();
            //internalSprite.atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName);
            //internalSprite.spriteName = "RcButton";
            //internalSprite.relativePosition = new Vector3(0, 0);
            //internalSprite.width = 50f;
            //internalSprite.height = 50f;
            size = new Vector2(60f, 50f);
            zOrder = 11;
            m_DragHandler = AddUIComponent<UIDragHandle>();
            m_DragHandler.target = this;
            m_DragHandler.relativePosition = Vector2.zero;
            m_DragHandler.width = 60;
            m_DragHandler.height = 50;
            m_DragHandler.zOrder = 10;
            m_DragHandler.Start();
            m_DragHandler.enabled = true;
            eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                if (tmpX == base.relativePosition.x && tmpY == base.relativePosition.y)
                {
                    MainUIToggle();
                }
                tmpX = base.relativePosition.x;
                tmpY = base.relativePosition.y;
            };
            //MainUITrigger_chirper = UIView.Find<UIPanel>("ChirperPanel");
            MainUITrigger_esc = UIView.Find<UIButton>("Esc");
            //MainUITrigger_infopanel = UIView.Find<UIPanel>("InfoPanel");
            //MainUITrigger_bottombars = UIView.Find<UISlicedSprite>("TSBar");
            MainUITrigger_paneltime = UIView.Find<UIPanel>("PanelTime");
            /*if (MainUITrigger_chirper != null && MainUITrigger_paneltime != null)
            {
                MainUITrigger_chirper.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
                {
                    MainUIOff();
                };
            }*/
            if (MainUITrigger_esc != null && MainUITrigger_paneltime != null)
            {
                MainUITrigger_esc.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
                {
                    MainUIOff();
                };
            }
            /*if (MainUITrigger_infopanel != null && MainUITrigger_paneltime != null)
            {
                MainUITrigger_infopanel.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
                {
                    MainUIOff();
                };
            }
            if (MainUITrigger_bottombars != null && MainUITrigger_paneltime != null)
            {
                MainUITrigger_bottombars.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
                {
                    MainUIOff();
                };
            }*/
        }

        public override void Update()
        {
            if (Loader.isGuiRunning)
            {
                if (Loader.mainUI.isVisible)
                {
                    //Focus();
                    //Hide();
                }
                else
                {
                    Unfocus();
                    Show();
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            base.Update();
        }

        public static void ClampRectToScreen(ref Rect rect, Vector2 resolution)
        {
            rect.x = Mathf.Clamp(rect.x, 0f, resolution.x - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0f, resolution.y - rect.height);
        }
    }
}

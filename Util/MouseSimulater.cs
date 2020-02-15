using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSURToolBox.Util
{
    //版权声明：本文为CSDN博主「Arvin ZHANG」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
    //原文链接：https://blog.csdn.net/qq_21397217/article/details/78488072
    public class MouseSimulater
    {
        #region DLLs
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        // 方法参数说明
        // VOID mouse_event(
        //     DWORD dwFlags,         // motion and click options
        //     DWORD dx,              // horizontal position or change
        //     DWORD dy,              // vertical position or change
        //     DWORD dwData,          // wheel movement
        //     ULONG_PTR dwExtraInfo  // application-defined information
        // );

        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        #endregion

        // Unity屏幕坐标从左下角开始，向右为X轴，向上为Y轴
        // Windows屏幕坐标从左上角开始，向右为X轴，向下为Y轴

        /// <summary>
        /// 移动鼠标到指定位置（使用Unity屏幕坐标而不是Windows屏幕坐标）
        /// </summary>
        public static bool MoveTo(float x, float y)
        {
            if (x < 0 || y < 0 || x > UnityEngine.Screen.width || y > UnityEngine.Screen.height)
                return true;

            if (!UnityEngine.Screen.fullScreen)
            {
                UnityEngine.Debug.LogError("只能在全屏状态下使用！");
                return false;
            }

            SetCursorPos((int)x, (int)(UnityEngine.Screen.height - y));
            return true;
        }

        // 左键单击
        public static void LeftClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 右键单击
        public static void RightClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 中键单击
        public static void MiddleClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 左键按下
        public static void LeftDown(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 左键抬起
        public static void LeftUp(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 右键按下
        public static void RightDown(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 右键抬起
        public static void RightUp(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 中键按下
        public static void MiddleDown(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 中键抬起
        public static void MiddleUp(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        // 滚轮滚动
        public static void ScrollWheel(float value)
        {
            mouse_event(MouseEventFlag.Wheel, 0, 0, (uint)value, UIntPtr.Zero);
        }
    }
}

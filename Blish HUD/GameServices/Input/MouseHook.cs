using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming

namespace Blish_HUD.Input
{
    public class MouseHook
    {
        public enum MouseMessages
        {
            WM_MouseMove = 512,
            WM_LeftButtonDown = 513,
            WM_LeftButtonUp = 514,
            WM_LeftDblClick = 515,
            WM_RightButtonDown = 516,
            WM_RightButtonUp = 517,
            WM_RightDblClick = 518,
            WM_MiddleButtonDown = 519,
            WM_MiddleButtonUp = 520,
            WM_MiddleButtonDblClick = 521,
            WM_MouseWheel = 522
        }

        private const int WH_MOUSE_LL = 14;

        private static readonly Logger Logger = Logger.GetLogger<MouseHook>();

        private bool _cameraDragging;
        private IntPtr _mouseHook;

        [MarshalAs(UnmanagedType.FunctionPtr)] private readonly MouseHookDelegate _mouseProc;

        public MouseHook()
        {
            this._mouseProc = MouseHookProc;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookExW(int idHook, MouseHookDelegate HookProc, IntPtr hInstance,
            int wParam);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

        public bool HookMouse()
        {
            Logger.Debug("Enabling mouse hook.");

            if (this._mouseHook == IntPtr.Zero)
            {
                this._mouseHook = SetWindowsHookExW(WH_MOUSE_LL, this._mouseProc,
                    HookExtern.GetModuleHandleW(IntPtr.Zero), 0);
            }

            return this._mouseHook != IntPtr.Zero;
        }

        public void UnhookMouse()
        {
            Logger.Debug("Disabling the mouse hook.");

            if (this._mouseHook == IntPtr.Zero) return;

            HookExtern.UnhookWindowsHookEx(this._mouseHook);
            this._mouseHook = IntPtr.Zero;
        }

        private int MouseHookProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam)
        {
            var action = wParam.ToInt32();
            if (this._cameraDragging && (action == 517))
            {
                // If the player has been holding WM_RightButtonDown, then we ignore WM_RightButtonUp (they are just releasing the camera)
                this._cameraDragging = false;
            }
            else if ((action > 512) && GameService.Input.HudFocused && (action < 523) &&
                     !GameService.Input.HookOverride)
            {
                GameService.Input.ClickState = new MouseEvent((MouseMessages) action, lParam);

                if (action != 514)
                    return 1;
            }
            else if (action == 516)
            {
                // If WM_RightButtonDown, we ignore it so that we don't accidentally intercept the player moving the camera
                this._cameraDragging = true;
            }

            return CallNextHookEx(WH_MOUSE_LL, nCode, wParam, ref lParam);
        }

        internal struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr extra;

            public int wheelDelta
            {
                get
                {
                    var v = Convert.ToInt32((this.mouseData & 0xFFFF0000) >> 16);
                    if (v > SystemInformation.MouseWheelScrollDelta) v -= ushort.MaxValue + 1;
                    return v;
                }
            }
        }

        private delegate int MouseHookDelegate(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
    }
}
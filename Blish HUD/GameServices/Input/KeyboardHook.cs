using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input
{
    internal class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static readonly Logger Logger = Logger.GetLogger<KeyboardHook>();
        private readonly LowLevelKeyboardProc _proc;

        private IntPtr _keyboardHook = IntPtr.Zero;

        public KeyboardHook()
        {
            this._proc = HookCallback;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public bool HookKeyboard()
        {
            Logger.Debug("Enabling keyboard hook.");

            if (this._keyboardHook == IntPtr.Zero)
            {
                this._keyboardHook =
                    SetWindowsHookEx(WH_KEYBOARD_LL, this._proc, HookExtern.GetModuleHandleW(IntPtr.Zero), 0);
            }

            return this._keyboardHook != IntPtr.Zero;
        }

        public void UnhookKeyboard()
        {
            Logger.Debug("Disabling the keyboard hook.");

            if (this._keyboardHook == IntPtr.Zero) return;

            HookExtern.UnhookWindowsHookEx(this._keyboardHook);
            this._keyboardHook = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Priority is to get the event into the queue so that Windows doesn't give up waiting on us
            GameService.Input.KeyboardMessages.Enqueue(new KeyboardMessage(nCode, wParam, Marshal.ReadInt32(lParam)));

            // If we are sending input to a control, try to prevent GW2 from getting any keypresses
            if ((nCode >= 0) && GameService.Input.BlockInput)
                return (IntPtr) 1;

            return CallNextHookEx(this._keyboardHook, nCode, wParam, lParam);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
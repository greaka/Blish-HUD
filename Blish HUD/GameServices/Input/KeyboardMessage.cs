using System;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input
{
    public struct KeyboardMessage
    {
        public int uMsg;
        public KeyboardEventType EventType;
        public Keys Key;

        public KeyboardMessage(int _uMsg, IntPtr _wParam, int _lParam)
        {
            this.uMsg = _uMsg;
            this.EventType = (KeyboardEventType) _wParam;
            this.Key = (Keys) _lParam;
        }
    }
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input
{
    public class MouseEventArgs : EventArgs
    {
        public MouseEventArgs(MouseState ms)
        {
            this.MouseState = ms;
        }

        public MouseState MouseState { get; }

        /// <summary>
        ///     The relative mouse position when the event was fired.
        /// </summary>
        public Point MousePosition { get; }
    }
}
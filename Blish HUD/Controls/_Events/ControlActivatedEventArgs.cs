using System;

namespace Blish_HUD.Controls
{
    public class ControlActivatedEventArgs : EventArgs
    {
        public ControlActivatedEventArgs(Control activatedControl)
        {
            this.ActivatedControl = activatedControl;
        }

        public Control ActivatedControl { get; }
    }
}
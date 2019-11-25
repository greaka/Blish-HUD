using System;

namespace Blish_HUD.Modules
{
    public class ModuleRunStateChangedEventArgs : EventArgs
    {
        public ModuleRunStateChangedEventArgs(ModuleRunState runState)
        {
            this.RunState = runState;
        }

        public ModuleRunState RunState { get; }
    }
}
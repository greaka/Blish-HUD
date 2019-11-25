using System;

namespace Blish_HUD.Controls
{
    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(string previousValue, string currentValue)
        {
            this.PreviousValue = previousValue;
            this.CurrentValue = currentValue;
        }

        public string PreviousValue { get; }
        public string CurrentValue { get; }
    }
}
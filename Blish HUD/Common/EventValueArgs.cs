using System;

namespace Blish_HUD.Common
{
    public class EventValueArgs<T> : EventArgs
    {
        public EventValueArgs(T value)
        {
            this.Value = value;
        }

        public T Value { get; }
    }
}
using System;

namespace Blish_HUD
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs(T previousValue, T newValue)
        {
            this.PrevousValue = previousValue;
            this.NewValue = newValue;
        }

        /// <summary>
        ///     The value of the property before it was changed.
        /// </summary>
        public T PrevousValue { get; }

        /// <summary>
        ///     The value of the property now that it has been changed.
        /// </summary>
        public T NewValue { get; }
    }
}
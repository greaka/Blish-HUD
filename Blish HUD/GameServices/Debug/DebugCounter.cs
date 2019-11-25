using System.Diagnostics;

namespace Blish_HUD.Debug
{
    public class DebugCounter
    {
        private readonly RingBuffer<long> _buffer;

        private float? _calculatedAverage;
        private long? _calculatedTotal;

        private long _intervalStartOffset;

        public DebugCounter(int bufferLength)
        {
            this._buffer = new RingBuffer<long>(bufferLength);

            StartInterval();
        }

        public void StartInterval()
        {
            this._intervalStartOffset = _sharedStopwatch.ElapsedMilliseconds;
        }

        public void EndInterval()
        {
            this._buffer.PushValue(_sharedStopwatch.ElapsedMilliseconds - this._intervalStartOffset);
            this._calculatedAverage = null;
            this._calculatedTotal = null;
        }

        public float GetAverage()
        {
            return this._calculatedAverage ??
                   (this._calculatedAverage = (float) GetTotal() / this._buffer.InternalBuffer.Length).Value;
        }

        public long GetTotal()
        {
            if (this._calculatedTotal == null)
            {
                this._calculatedTotal = 0;

                for (var i = 0; i < this._buffer.InternalBuffer.Length; i++)
                {
                    this._calculatedTotal += this._buffer.InternalBuffer[i];
                }
            }

            return this._calculatedTotal.Value;
        }

        #region Load Static

        private static readonly Stopwatch _sharedStopwatch;

        static DebugCounter()
        {
            _sharedStopwatch = new Stopwatch();
            _sharedStopwatch.Start();
        }

        #endregion
    }
}
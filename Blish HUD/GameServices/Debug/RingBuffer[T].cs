namespace Blish_HUD.Debug
{
    /// <summary>
    ///     [NOT THREAD-SAFE] A fixed capacity buffer which overwrites itself as the index wraps.
    /// </summary>
    /// <typeparam name="T">The <c>Type</c> the <see cref="RingBuffer{T}" /> contains.</typeparam>
    public class RingBuffer<T>
    {
        private int _ringIndex;

        /// <summary>
        ///     Creates a
        /// </summary>
        /// <param name="bufferLength"></param>
        public RingBuffer(int bufferLength)
        {
            this.InternalBuffer = new T[bufferLength];
        }

        /// <summary>
        ///     The internal buffer backing this <see cref="RingBuffer{T}" />.
        /// </summary>
        public T[] InternalBuffer { get; }

        /// <summary>
        ///     Pushes a value into the <see cref="RingBuffer{T}" />.
        /// </summary>
        /// <param name="value">The value to push into this <see cref="RingBuffer{T}" />.</param>
        public void PushValue(T value)
        {
            this.InternalBuffer[this._ringIndex] = value;
            this._ringIndex = (this._ringIndex + 1) % this.InternalBuffer.Length;
        }
    }
}
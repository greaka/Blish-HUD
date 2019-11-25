namespace Blish_HUD.Debug
{
    public class FrameCounter
    {
        private readonly RingBuffer<float> _fpsSamples;

        public FrameCounter(int sampleCount)
        {
            this._fpsSamples = new RingBuffer<float>(sampleCount);
        }

        public float CurrentAverage { get; private set; }

        public void Update(float deltaTime)
        {
            this._fpsSamples.PushValue(1 / deltaTime);

            float total = 0;
            for (var i = 0; i < this._fpsSamples.InternalBuffer.Length; i++)
            {
                total += this._fpsSamples.InternalBuffer[i];
            }

            this.CurrentAverage = total / this._fpsSamples.InternalBuffer.Length;
        }
    }
}
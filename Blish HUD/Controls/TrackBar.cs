using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls
{
    public class TrackBar : Control
    {
        private const int BUFFER_WIDTH = 4;

        #region Calculated Layout

        private Rectangle _layoutNubBounds;

        #endregion

        protected int _maxValue = 100;

        protected int _minValue;

        protected float _value = 50;

        private bool Dragging;
        private int DraggingOffset;

        public TrackBar()
        {
            this.Size = new Point(256, 16);

            LeftMouseButtonPressed += TrackBar_LeftMouseButtonPressed;
            Input.LeftMouseButtonReleased += Input_LeftMouseButtonReleased;
        }

        public int MaxValue
        {
            get => this._maxValue;
            set => SetProperty(ref this._maxValue, value, true);
        }

        public int MinValue
        {
            get => this._minValue;
            set => SetProperty(ref this._minValue, value, true);
        }

        public float Value
        {
            get => this._value;
            set
            {
                if (SetProperty(ref this._value, MathHelper.Clamp(value, this._minValue, this._maxValue), true))
                {
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int RoundedValue => (int) Math.Round(this._value, 0);

        public event EventHandler<EventArgs> ValueChanged;

        private void Input_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            this.Dragging = false;
        }

        private void TrackBar_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (this._layoutNubBounds.Contains(this.RelativeMousePosition))
            {
                this.Dragging = true;
                this.DraggingOffset = this.RelativeMousePosition.X - this._layoutNubBounds.X;
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            if (this.Dragging)
            {
                var relMousePos = this.RelativeMousePosition - new Point(this.DraggingOffset, 0);
                this.Value = relMousePos.X / (float) (this.Width - BUFFER_WIDTH * 2 - _textureNub.Width) *
                             (this.MaxValue - this.MinValue);
            }
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        public override void RecalculateLayout()
        {
            var valueOffset = (this.Value - this.MinValue) / (this.MaxValue - this.MinValue) *
                              (_textureTrack.Width - BUFFER_WIDTH * 2 - _textureNub.Width);
            this._layoutNubBounds =
                new Rectangle((int) valueOffset + BUFFER_WIDTH, 0, _textureNub.Width, _textureNub.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _textureTrack, bounds);

            spriteBatch.DrawOnCtrl(this, _textureNub, this._layoutNubBounds);
        }

        #region Load Static

        private static readonly TextureRegion2D _textureTrack;
        private static readonly TextureRegion2D _textureNub;

        static TrackBar()
        {
            _textureTrack = Resources.Control.TextureAtlasControl.GetRegion("trackbar/tb-track");
            _textureNub = Resources.Control.TextureAtlasControl.GetRegion("trackbar/tb-nub");
        }

        #endregion
    }
}
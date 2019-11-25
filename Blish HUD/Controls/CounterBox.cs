using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class CounterBox : Control
    {
        private readonly Texture2D MinusSprite;
        private readonly Texture2D PlusSprite;
        private bool _exponential;
        private int _maxValue;
        private int _minValue;
        private bool _mouseOverMinus;
        private bool _mouseOverPlus;
        private int _numerator;
        private string _prefix = "";
        private string _suffix = "";
        private int _value = 1;
        private int _valueWidth;

        public CounterBox()
        {
            this.MinusSprite = this.MinusSprite ?? Content.GetTexture("minus");
            this.PlusSprite = this.PlusSprite ?? Content.GetTexture("plus");
            MouseMoved += CounterBox_MouseMoved;
            MouseLeft += CounterBox_MouseLeft;
            LeftMouseButtonPressed += CounterBox_LeftMouseButtonPressed;
            this.Size = new Point(150, 20);
        }

        public int ValueWidth
        {
            get => this._valueWidth;
            set
            {
                if (this._valueWidth == value) return;
                this._valueWidth = value;
                Invalidate();
            }
        }

        public string Prefix
        {
            get => this._prefix;
            set
            {
                if (string.Equals(this._prefix, value)) return;
                this._prefix = value;
                Invalidate();
            }
        }

        public string Suffix
        {
            get => this._suffix;
            set
            {
                if (string.Equals(this._suffix, value)) return;
                this._suffix = value;
                Invalidate();
            }
        }

        public int Value
        {
            get => this._value;
            set
            {
                if (this._value == value) return;
                this._value = value;
                Invalidate();
            }
        }

        public int Numerator
        {
            get => this._numerator;
            set
            {
                if (this._numerator == value) return;
                this._numerator = value;
            }
        }

        public int MaxValue
        {
            get => this._maxValue;
            set
            {
                if (this._maxValue == value) return;
                if (value < this._minValue) return;
                this._maxValue = value;
                Invalidate();
            }
        }

        public int MinValue
        {
            get => this._minValue;
            set
            {
                if (this._minValue == value) return;
                if (value > this._maxValue) return;
                this._minValue = value;
                Invalidate();
            }
        }

        public bool Exponential
        {
            get => this._exponential;
            set
            {
                if (this._exponential == value) return;
                this._exponential = value;
            }
        }

        public bool MouseOverPlus
        {
            get => this._mouseOverPlus;
            set
            {
                if (this._mouseOverPlus == value) return;
                this._mouseOverPlus = value;
                Invalidate();
            }
        }

        public bool MouseOverMinus
        {
            get => this._mouseOverMinus;
            set
            {
                if (this._mouseOverMinus == value) return;
                this._mouseOverMinus = value;
                Invalidate();
            }
        }

        private void CounterBox_MouseLeft(object sender, MouseEventArgs e)
        {
            this.MouseOverPlus = false;
            this.MouseOverMinus = false;
        }

        private void CounterBox_MouseMoved(object sender, MouseEventArgs e)
        {
            var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (this.MouseOver)
            {
                this.MouseOverMinus = (relPos.X < 17) && (relPos.X > 0);
                this.MouseOverPlus = (relPos.X < 36 + this.ValueWidth) && (relPos.X > 19 + this.ValueWidth);
            }
            else
            {
                this.MouseOverMinus = false;
                this.MouseOverPlus = false;
            }
        }

        private void CounterBox_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (this.MouseOverMinus)
            {
                if (this.Exponential)
                {
                    var halfed = this.Value / 2;
                    if (halfed >= this.MinValue)
                    {
                        this.Value = halfed;
                    }
                }
                else
                {
                    var difference = this.Value - this.Numerator;
                    if (difference >= this.MinValue)
                    {
                        this.Value = difference;
                    }

                    Invalidate();
                }
            }

            if (this.MouseOverPlus)
            {
                if (this.Exponential)
                {
                    var doubled = this.Value + this.Value;
                    if (doubled <= this.MaxValue)
                    {
                        this.Value = doubled;
                    }
                }
                else
                {
                    var summation = this.Value + this.Numerator;
                    if (summation <= this.MaxValue)
                    {
                        this.Value = summation;
                    }
                }

                Invalidate();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.MouseOverMinus)
            {
                spriteBatch.DrawOnCtrl(this, this.MinusSprite, new Rectangle(2, 2, 15, 15), Color.White);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, this.MinusSprite, new Rectangle(0, 0, 17, 17), Color.White);
            }

            var combine = this.Prefix + this.Value + this.Suffix;
            spriteBatch.DrawStringOnCtrl(this, combine, Content.DefaultFont14,
                new Rectangle(18, 0, this.ValueWidth, 17), Color.White, false, true, 1, HorizontalAlignment.Center);

            if (this.MouseOverPlus)
            {
                spriteBatch.DrawOnCtrl(this, this.PlusSprite, new Rectangle(21 + this.ValueWidth, 2, 15, 15),
                    Color.White);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, this.PlusSprite, new Rectangle(19 + this.ValueWidth, 0, 17, 17),
                    Color.White);
            }
        }
    }
}
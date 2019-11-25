using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls
{
    public abstract class LabelBase : Control
    {
        protected bool _autoSizeHeight = false;

        protected bool _autoSizeWidth = false;

        protected bool _cacheLabel = false;

        protected BitmapFont _font;

        protected HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

        private CachedStringRender _labelRender;

        protected Color _shadowColor = Color.Black;

        protected bool _showShadow = false;

        protected bool _strokeText = false;

        protected string _text;

        protected Color _textColor = Color.White;

        protected VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;

        protected bool _wrapText = false;

        /// <summary>
        ///     If either <see cref="AutoSizeWidth" /> or <see cref="AutoSizeHeight" /> is enabled,
        ///     this will indicate the size of the label region after <see cref="RecalculateLayout" />
        ///     has completed.
        /// </summary>
        protected Point LabelRegion = Point.Zero;

        public LabelBase()
        {
            this._font = Content.DefaultFont14;
        }

        protected override CaptureType CapturesInput()
        {
            //return string.IsNullOrEmpty(this.BasicTooltipText) ? CaptureType.None : CaptureType.Mouse;
            return CaptureType.Filter;
        }

        public override void RecalculateLayout()
        {
            var lblRegionWidth = this._size.X;
            var lblRegionHeight = this._size.Y;

            if (this._autoSizeWidth || this._autoSizeHeight)
            {
                var textSize = GetTextDimensions();

                if (this._autoSizeWidth)
                {
                    lblRegionWidth =
                        (int) Math.Ceiling(textSize.Width + (this._showShadow || this._strokeText ? 1 : 0));
                }

                if (this._autoSizeHeight)
                {
                    lblRegionHeight =
                        (int) Math.Ceiling(textSize.Height + (this._showShadow || this._strokeText ? 1 : 0));
                }
            }

            this.LabelRegion = new Point(lblRegionWidth, lblRegionHeight);

            if (this._cacheLabel)
            {
                this._labelRender = CachedStringRender.GetCachedStringRender(this._text, this._font,
                    new Rectangle(Point.Zero, this.LabelRegion), this._textColor,
                    false, this._strokeText,
                    1, this._horizontalAlignment, this._verticalAlignment);
            }
        }

        protected Size2 GetTextDimensions(string text = null)
        {
            text = text ?? this._text;

            if (!this._autoSizeWidth && this._wrapText)
            {
                text = DrawUtil.WrapText(this._font, text, this.LabelRegion.X > 0 ? this.LabelRegion.X : this._size.X);
            }

            return this._font.MeasureString(text ?? this._text);
        }

        protected void DrawText(SpriteBatch spriteBatch, Rectangle bounds, string text = null)
        {
            text = text ?? this._text;

            if ((this._font == null) || string.IsNullOrEmpty(text)) return;

            if (this._showShadow && !this._strokeText)
            {
                spriteBatch.DrawStringOnCtrl(this, text, this._font, bounds.OffsetBy(1, 1), this._shadowColor,
                    this._wrapText, this._horizontalAlignment, this._verticalAlignment);
            }

            if (this._cacheLabel && (this._labelRender != null))
            {
                spriteBatch.DrawOnCtrl(this, this._labelRender.CachedRender, bounds);
            }
            else
            {
                spriteBatch.DrawStringOnCtrl(this, text, this._font, bounds, this._textColor, this._wrapText,
                    this._strokeText, 1, this._horizontalAlignment, this._verticalAlignment);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            DrawText(spriteBatch, bounds, this._text);
        }

        protected override void DisposeControl()
        {
        }
    }
}
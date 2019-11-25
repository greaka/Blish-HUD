using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls
{
    public class Label : LabelBase
    {
        public Label()
        {
            this._cacheLabel = false;
        }

        /// <summary>
        ///     The text this <see cref="Label" /> should show.
        /// </summary>
        public string Text
        {
            get => this._text;
            set
            {
                if (SetProperty(ref this._text, value, true) && (this._autoSizeWidth || this._autoSizeHeight))
                {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        ///     The font the <see cref="Text" /> will be rendered in.
        /// </summary>
        public BitmapFont Font
        {
            get => this._font;
            set
            {
                if (SetProperty(ref this._font, value, true) && (this._autoSizeWidth || this._autoSizeHeight))
                {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        ///     The color of the <see cref="Text" />.
        /// </summary>
        public Color TextColor
        {
            get => this._textColor;
            set => SetProperty(ref this._textColor, value);
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => this._horizontalAlignment;
            set => SetProperty(ref this._horizontalAlignment, value);
        }

        public VerticalAlignment VerticalAlignment
        {
            get => this._verticalAlignment;
            set => SetProperty(ref this._verticalAlignment, value);
        }

        public bool WrapText
        {
            get => this._wrapText;
            set => SetProperty(ref this._wrapText, value, true);
        }

        /// <summary>
        ///     If enabled, a 1px offset shadow will be applied behind the rendered text.
        /// </summary>
        public bool ShowShadow
        {
            get => this._showShadow;
            set => SetProperty(ref this._showShadow, value, true);
        }

        /// <summary>
        ///     If enabled, a stroke effect will be applied to the text to make it more visible.
        ///     <see cref="ShadowColor" /> will set the color of the stroke.
        /// </summary>
        public bool StrokeText
        {
            get => this._strokeText;
            set => SetProperty(ref this._strokeText, value, true);
        }

        /// <summary>
        ///     If either <see cref="ShowShadow" /> or <see cref="StrokeText" /> is enabled, they will
        ///     be drawn in this color.
        /// </summary>
        public Color ShadowColor
        {
            get => this._shadowColor;
            set => SetProperty(ref this._shadowColor, value);
        }

        /// <summary>
        ///     If enabled, the <see cref="Control.Width" /> of this control will change to match the width of the text.
        /// </summary>
        public bool AutoSizeWidth
        {
            get => this._autoSizeWidth;
            set
            {
                if (SetProperty(ref this._autoSizeWidth, value, true) && (this._autoSizeWidth || this._autoSizeHeight))
                {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        ///     If enabled, the <see cref="Control.Height" /> of this control will change to match the height of the text.
        /// </summary>
        public bool AutoSizeHeight
        {
            get => this._autoSizeHeight;
            set
            {
                if (SetProperty(ref this._autoSizeHeight, value, true) && (this._autoSizeWidth || this._autoSizeHeight))
                {
                    RecalculateLayout();
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            this.Size = this.LabelRegion;
        }
    }
}
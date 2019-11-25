using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class StandardButton : LabelBase
    {
        public const int STANDARD_CONTROL_HEIGHT = 26;
        public const int DEFAULT_CONTROL_WIDTH = 128;

        private const int ICON_SIZE = 16;
        private const int ICON_TEXT_OFFSET = 4;

        private const int ATLAS_SPRITE_WIDTH = 350;
        private const int ATLAS_SPRITE_HEIGHT = 20;
        private const int ANIM_FRAME_TIME = 300 / 9;

        private Rectangle _activeAtlasRegion = new Rectangle(0, 0, 350, 20);

        private EaseAnimation _animHover;

        private AsyncTexture2D _icon;

        private Rectangle _layoutIconBounds = Rectangle.Empty;
        private Rectangle _layoutTextBounds = Rectangle.Empty;

        private bool _resizeIcon;

        public StandardButton()
        {
            this._textColor = Color.Black;
            this._horizontalAlignment = HorizontalAlignment.Left;
            this._verticalAlignment = VerticalAlignment.Middle;

            this.Size = new Point(DEFAULT_CONTROL_WIDTH, STANDARD_CONTROL_HEIGHT);

            InitAnim();
        }

        /// <summary>
        ///     The text shown on the button.
        /// </summary>
        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value, true);
        }

        /// <summary>
        ///     An icon to show on the <see cref="StandardButton" />.  For best results, the <see cref="Icon" /> should be 16x16.
        /// </summary>
        public AsyncTexture2D Icon
        {
            get => this._icon;
            set => SetProperty(ref this._icon, value, true);
        }

        /// <summary>
        ///     If true, the <see cref="Icon" /> texture will be resized to 16x16.
        /// </summary>
        public bool ResizeIcon
        {
            get => this._resizeIcon;
            set => SetProperty(ref this._resizeIcon, value, true);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        private void InitAnim()
        {
            // TODO: Convert button animation from old animation service to glide library
            this._animHover =
                GameService.Animation.Tween(0, 8, ANIM_FRAME_TIME * 9, AnimationService.EasingMethod.Linear);
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            this._animHover?.Start();

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            if (this._animHover != null)
            {
                this._animHover.Reverse();
                this._animHover.AnimationCompleted += delegate { InitAnim(); };
            }

            base.OnMouseLeft(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnClick(e);
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            // TODO: Ensure that these calculations are correctly placing the image in the middle and clean things up
            var textSize = GetTextDimensions();

            var textLeft = (int) (this._size.X / 2 - textSize.Width / 2);

            if (this._icon != null)
            {
                if (textSize.Width > 0)
                {
                    textLeft += ICON_SIZE / 2 + ICON_TEXT_OFFSET / 2;
                }
                else
                {
                    textLeft += ICON_SIZE / 2;
                }

                var iconSize = this._resizeIcon ? new Point(ICON_SIZE) : this._icon.Texture.Bounds.Size;

                this._layoutIconBounds = new Rectangle(textLeft - iconSize.X - ICON_TEXT_OFFSET,
                    this._size.Y / 2 - iconSize.Y / 2, iconSize.X, iconSize.Y);
            }

            this._layoutTextBounds = new Rectangle(textLeft, 0, this._size.X - textLeft, this._size.Y);
        }

        public override void DoUpdate(GameTime gameTime)
        {
            if (this._enabled && this.MouseOver && (this._animHover == null))
            {
                this._animHover = GameService.Animation.Tween(0, 8,
                    ANIM_FRAME_TIME * 9 * (this.Width / ATLAS_SPRITE_WIDTH), AnimationService.EasingMethod.Linear);
            }

            if (this._animHover != null)
            {
                this._activeAtlasRegion = new Rectangle(this._animHover.CurrentValueInt * ATLAS_SPRITE_WIDTH, 0,
                    ATLAS_SPRITE_WIDTH, ATLAS_SPRITE_HEIGHT);

                if (this._animHover.Active) Invalidate();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Button Texture
            if (this._enabled)
            {
                spriteBatch.DrawOnCtrl(this,
                    _textureButtonIdle,
                    new Rectangle(3, 3, this._size.X - 6, this._size.Y - 5), this._activeAtlasRegion);
            }
            else
            {
                // TODO: Use the actual button texture instead
                spriteBatch.DrawOnCtrl(this,
                    ContentService.Textures.Pixel,
                    new Rectangle(3, 3, this._size.X - 6, this._size.Y - 5),
                    Color.FromNonPremultiplied(121, 121, 121, 255));
            }

            // Top Shadow
            spriteBatch.DrawOnCtrl(this,
                _textureButtonBorder,
                new Rectangle(2, 0, this.Width - 5, 4),
                new Rectangle(0, 0, 1, 4));

            // Right Shadow
            spriteBatch.DrawOnCtrl(this,
                _textureButtonBorder,
                new Rectangle(this.Width - 4, 2, 4, this.Height - 3),
                new Rectangle(0, 1, 4, 1));

            // Bottom Shadow
            spriteBatch.DrawOnCtrl(this,
                _textureButtonBorder,
                new Rectangle(3, this.Height - 4, this.Width - 6, 4),
                new Rectangle(1, 0, 1, 4));

            // Left Shadow
            spriteBatch.DrawOnCtrl(this,
                _textureButtonBorder,
                new Rectangle(0, 2, 4, this.Height - 3),
                new Rectangle(0, 3, 4, 1));

            // Draw Icon
            if (this._icon != null)
            {
                spriteBatch.DrawOnCtrl(this, this._icon, this._layoutIconBounds);
            }

            // TODO: Don't set button text color like this
            this._textColor = this._enabled ? Color.Black : Color.FromNonPremultiplied(51, 51, 51, 255);
            // Button Text
            DrawText(spriteBatch, this._layoutTextBounds);
        }

        #region Load Static

        private static readonly Texture2D _textureButtonIdle;
        private static readonly Texture2D _textureButtonBorder;

        static StandardButton()
        {
            _textureButtonIdle = Content.GetTexture(@"common\button-states");
            _textureButtonBorder = Content.GetTexture("button-border");
        }

        #endregion
    }
}
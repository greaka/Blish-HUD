using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls
{
    public class InteractionIndicator : LabelBase
    {
        private const int CONTROL_WIDTH = 256;
        private const int CONTROL_HEIGHT = 64;

        private const float LEFT_OFFSET = 0.65f;
        private const float TOP_OFFSET = 0.67f;

        private Tween _fadeAnimation;

        protected Keys[] _interactionKeys = {Keys.F};
        protected int _verticalIndex = 1;

        public InteractionIndicator()
        {
            this._text = Strings.Controls.InteractionIndicator_Interact;
            this._verticalAlignment = VerticalAlignment.Middle;
            this._showShadow = true;
            this._strokeText = true;
            this._font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18,
                ContentService.FontStyle.Regular);
            this._size = new Point((int) (CONTROL_WIDTH * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)),
                (int) (CONTROL_HEIGHT * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)));
            this._location = new Point((int) (Graphics.WindowWidth * LEFT_OFFSET),
                (int) (Graphics.WindowHeight * TOP_OFFSET) - CONTROL_HEIGHT * this._verticalIndex);
            this._opacity = 0f;
            this._visible = false;
            this.Parent = Graphics.SpriteScreen;

            Graphics.SpriteScreen.Resized += delegate
            {
                this.Location = new Point(
                    (int) (Graphics.WindowWidth * LEFT_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)),
                    (int) (Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)) -
                    CONTROL_HEIGHT * this._verticalIndex);
            };
        }


        /// <summary>
        ///     The text this <see cref="InteractionIndicator" /> should show.
        /// </summary>
        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value, true);
        }

        public int VerticalIndex
        {
            get => this._verticalIndex;
            set
            {
                if (SetProperty(ref this._verticalIndex, value, true))
                    this.Top = (int) (Graphics.WindowHeight * TOP_OFFSET *
                                      Graphics.GetScaleRatio(GraphicsService.UiScale.Large)) -
                               CONTROL_HEIGHT * this._verticalIndex;
            }
        }

        public Keys[] InteractionKeys
        {
            get => this._interactionKeys;
            set => SetProperty(ref this._interactionKeys, value);
        }

        public override void Show()
        {
            this._fadeAnimation?.Cancel();

            this.Visible = true;

            this._fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 1f}, (1f - this.Opacity) / 2);
        }

        public override void Hide()
        {
            this._fadeAnimation?.Cancel();
            this._fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 0f}, this.Opacity / 2).OnComplete(
                delegate { this.Visible = false; });
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, Content.GetTexture("156775"),
                bounds,
                bounds.OffsetBy(0, CONTROL_HEIGHT / 2),
                Color.White);

            DrawText(spriteBatch, new Rectangle((int) (bounds.Width * 0.2),
                    (int) (bounds.Height * 0.13),
                    (int) (bounds.Width * 0.78),
                    (int) (bounds.Height * 0.5)),
                $"{DrawUtil.WrapText(this._font, this._text, bounds.Width * 0.5f)} [{string.Join(" + ", this.InteractionKeys)}]");
        }
    }
}
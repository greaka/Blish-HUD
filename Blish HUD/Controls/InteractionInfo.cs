using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class InteractionInfo : LabelBase
    {
        private const int CONTROL_WIDTH = 170;
        private const int CONTROL_HEIGHT = 85;

        private const float LEFT_OFFSET = 0.67f;
        private const float TOP_OFFSET = 0.71f;

        private Tween _fadeAnimation;

        protected int _verticalIndex;

        public InteractionInfo()
        {
            this._text = Strings.Controls.InteractionInfo_Info;
            this._verticalAlignment = VerticalAlignment.Middle;
            this._showShadow = true;
            this._strokeText = true;
            this._font = Content.DefaultFont12;
            this.Size = new Point(CONTROL_WIDTH, CONTROL_HEIGHT);
            this.Location = new Point((int) (Graphics.WindowWidth * LEFT_OFFSET),
                (int) (Graphics.WindowHeight * TOP_OFFSET) - CONTROL_HEIGHT * this._verticalIndex);
            this.Opacity = 0f;
            this.Visible = false;
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
        ///     The text this <see cref="InteractionInfo" /> should show.
        /// </summary>
        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value, true);
        }

        public int VerticalIndex
        {
            get => this._verticalIndex;
            set => SetProperty(ref this._verticalIndex, value, true);
        }

        public override void Show()
        {
            this._fadeAnimation?.Cancel();

            this.Visible = true;

            this._fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 0.9f}, (0.9f - this.Opacity) / 2);
        }

        public override void Hide()
        {
            this._fadeAnimation?.Cancel();
            this._fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 0f}, this.Opacity / 2).OnComplete(
                delegate { this.Visible = false; });
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var textRegion = new Rectangle(
                (int) (this._size.X * 0.24),
                (int) (bounds.Height * 0.34),
                (int) (this._size.X * 0.62),
                (int) (this._size.Y * 0.26)
            );

            spriteBatch.DrawOnCtrl(this,
                Content.GetTexture("156775"),
                bounds);

            DrawText(spriteBatch,
                textRegion,
                $"{DrawUtil.WrapText(this._font, this._text, textRegion.Width)}");
        }
    }
}
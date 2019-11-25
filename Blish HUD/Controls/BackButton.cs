using Blish_HUD.Controls.Effects;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class BackButton : Control
    {
        private const int BACKBUTTON_WIDTH = 280;
        private const int BACKBUTTON_HEIGHT = 54;

        private const int BACKBUTTON_ICON_PADDING = 9;
        private const int BACKBUTTON_ICON_SIZE = 36;

        private readonly WindowBase _window;

        /// <summary>
        ///     The secondary path of the back button.  Format is "Text: NavTitle"
        /// </summary>
        protected string _navTitle;

        protected string _text = "button";

        public BackButton(WindowBase window)
        {
            this.Size = new Point(BACKBUTTON_WIDTH, BACKBUTTON_HEIGHT);

            this.EffectBehind = new ScrollingHighlightEffect(this);

            this._window = window;
        }

        /// <summary>
        ///     The primary text of the back button.  Format is "Text: NavTitle"
        /// </summary>
        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value);
        }

        public string NavTitle
        {
            get => this._navTitle;
            set => SetProperty(ref this._navTitle, value);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            this._window.NavigateBack();
        }

        public override void RecalculateLayout()
        {
            this._layoutButtonIconBounds = new Rectangle(BACKBUTTON_ICON_PADDING, BACKBUTTON_ICON_PADDING,
                BACKBUTTON_ICON_SIZE, BACKBUTTON_ICON_SIZE);
            this._layoutTextBounds =
                new Rectangle(BACKBUTTON_HEIGHT, 0, this._size.X - BACKBUTTON_HEIGHT, this._size.Y);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw back button
            spriteBatch.DrawOnCtrl(this, _textureBackButton, this._layoutButtonIconBounds);

            // Draw the full tab path (Tab: Subtab)
            spriteBatch.DrawStringOnCtrl(this, $"{this._text}: {this._navTitle}",
                Content.DefaultFont16, this._layoutTextBounds,
                Color.White * 0.8f);

            // Draw just the tab name
            spriteBatch.DrawStringOnCtrl(this, $"{this._text}:",
                Content.DefaultFont16, this._layoutTextBounds,
                Color.White * 0.8f);
        }

        #region Load Static

        private static readonly Texture2D _textureBackButton;

        static BackButton()
        {
            _textureBackButton = Content.GetTexture("784268");
        }

        #endregion

        #region Calculated Layout

        private Rectangle _layoutButtonIconBounds;
        private Rectangle _layoutTextBounds;

        #endregion
    }
}
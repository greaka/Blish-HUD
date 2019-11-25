using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using static Blish_HUD.ContentService;

namespace Blish_HUD.Controls
{
    public class Page : Control
    {
        private const int SHEET_BORDER = 40;
        private const int FIX_WORDCLIPPING_WIDTH = 30;

        private static readonly BitmapFont PageNumberFont =
            GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size18, FontStyle.Regular);

        private readonly Texture2D SheetSprite;

        private int _pageNumber = 1;
        private string _text = "";

        private BitmapFont _textFont =
            GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size24, FontStyle.Regular);

        /// <summary>
        ///     Creates a control similar to the Tyrian' sheet of paper found on the book UI.
        /// </summary>
        /// <param name="scale">Scale size to keep the sheet's aspect ratio.</param>
        public Page(int scale = 1)
        {
            this.Size = new Point(420 * scale, 560 * scale);
            this.SheetSprite = this.SheetSprite ?? Content.GetTexture("1909316");
        }

        public int PageNumber
        {
            get => this._pageNumber;
            set
            {
                if (value == this._pageNumber) return;
                SetProperty(ref this._pageNumber, value, true);
            }
        }

        /// <summary>
        ///     The text to display on the sheet.
        /// </summary>
        public string Text
        {
            get => this._text;
            set
            {
                if (value.Equals(this._text)) return;
                SetProperty(ref this._text, value, true);
            }
        }

        /// <summary>
        ///     The font that is for the text.
        /// </summary>
        public BitmapFont TextFont
        {
            get => this._textFont;
            set
            {
                if (value.Equals(this._textFont)) return;
                SetProperty(ref this._textFont, value, true);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, this.SheetSprite, bounds, this.SheetSprite.Bounds, Color.White, 0f,
                Vector2.Zero);

            if (this._text.Length > 0)
            {
                var contentArea = new Rectangle(new Point(SHEET_BORDER, SHEET_BORDER),
                    new Point(this.Size.X - SHEET_BORDER * 2 - FIX_WORDCLIPPING_WIDTH, this.Size.Y - SHEET_BORDER * 2));
                spriteBatch.DrawStringOnCtrl(this, this._text, this._textFont, contentArea, Color.Black, true,
                    HorizontalAlignment.Left, VerticalAlignment.Top);
                var pageNumber = this._pageNumber + "";
                var pageNumberSize = (Point) PageNumberFont.MeasureString(pageNumber);
                var pageNumberCenter = new Point((this.Size.X - pageNumberSize.X) / 2,
                    this.Size.Y - pageNumberSize.Y - SHEET_BORDER / 2);
                spriteBatch.DrawStringOnCtrl(this, pageNumber, PageNumberFont,
                    new Rectangle(pageNumberCenter, pageNumberSize), Color.Black, false, HorizontalAlignment.Left,
                    VerticalAlignment.Top);
            }
        }
    }
}
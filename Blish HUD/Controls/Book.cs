using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls
{
    public class Book : Container
    {
        private static readonly int RIGHT_PADDING = 150;
        private static readonly int TOP_PADDING = 100;
        private static readonly int SHEET_OFFSET_Y = 20;

        private readonly BitmapFont TitleFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia,
            ContentService.FontSize.Size32, ContentService.FontStyle.Regular);

        private readonly Texture2D TurnPageSprite;

        private Rectangle _leftButtonBounds;
        private Rectangle _rightButtonBounds;

        private string _title = "No Title";
        private Rectangle _titleBounds;

        private bool MouseOverTurnPageLeft;
        private bool MouseOverTurnPageRight;

        private readonly List<Page> Pages = new List<Page>();

        /// <summary>
        ///     Creates a panel that should act as Parent for Page controls to create a book UI.
        /// </summary>
        public Book()
        {
            this.TurnPageSprite = this.TurnPageSprite ?? GameService.Content.GetTexture("1909317");
        }

        /// <summary>
        ///     Sets the title of this book.
        /// </summary>
        public string Title
        {
            get => this._title;
            set
            {
                if (value.Equals(this._title)) return;
                SetProperty(ref this._title, value, true);
            }
        }

        /// <summary>
        ///     The currently open page of this book.
        /// </summary>
        public Page CurrentPage { get; private set; }

        protected override void OnResized(ResizedEventArgs e)
        {
            this.ContentRegion = new Rectangle(0, 0, e.CurrentSize.X, e.CurrentSize.Y);

            this._leftButtonBounds = new Rectangle(25,
                (this.ContentRegion.Height - this.TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET_Y,
                this.TurnPageSprite.Bounds.Width, this.TurnPageSprite.Bounds.Height);
            this._rightButtonBounds = new Rectangle(this.ContentRegion.Width - this.TurnPageSprite.Bounds.Width - 25,
                (this.ContentRegion.Height - this.TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET_Y,
                this.TurnPageSprite.Bounds.Width, this.TurnPageSprite.Bounds.Height);

            var titleSize = (Point) this.TitleFont.MeasureString(this._title);
            this._titleBounds = new Rectangle((this.ContentRegion.Width - titleSize.X) / 2,
                this.ContentRegion.Top + (TOP_PADDING - titleSize.Y) / 2, titleSize.X, titleSize.Y);

            if ((this.Pages != null) && (this.Pages.Count > 0))
            {
                foreach (var page in this.Pages)
                {
                    if (page == null) continue;
                    page.Size = PointExtensions.ResizeKeepAspect(page.Size, this.ContentRegion.Width - RIGHT_PADDING,
                        this.ContentRegion.Height - TOP_PADDING, true);
                    page.Location = new Point((this.ContentRegion.Width - page.Size.X) / 2,
                        (this.ContentRegion.Height - page.Size.Y) / 2 + SHEET_OFFSET_Y);
                }
            }

            base.OnResized(e);
        }

        protected override void OnHidden(EventArgs e)
        {
            //TODO: Add gw2 book sound: close book.
            base.OnHidden(e);
        }

        protected override void OnShown(EventArgs e)
        {
            //TODO: Add gw2 book sound: open book.
            base.OnShown(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is Page && !this.Pages.Any(x => x.Equals((Page) e.ChangedChild)))
            {
                var page = (Page) e.ChangedChild;
                page.Size = PointExtensions.ResizeKeepAspect(page.Size, this.ContentRegion.Width - RIGHT_PADDING,
                    this.ContentRegion.Height - TOP_PADDING, true);
                page.Location = new Point((this.ContentRegion.Width - page.Size.X) / 2,
                    (this.ContentRegion.Height - page.Size.Y) / 2 + SHEET_OFFSET_Y);
                page.PageNumber = this.Pages.Count + 1;
                this.Pages.Add(page);

                if (this.Pages.Count == 1) this.CurrentPage = page;
                if (page != this.CurrentPage) page.Hide();
            }

            base.OnChildAdded(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = this.RelativeMousePosition;

            this.MouseOverTurnPageLeft = this._leftButtonBounds.Contains(relPos);
            this.MouseOverTurnPageRight = this._rightButtonBounds.Contains(relPos);

            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (this.MouseOverTurnPageLeft)
            {
                TurnPage(this.Pages.IndexOf(this.CurrentPage) - 1);
            }
            else if (this.MouseOverTurnPageRight)
            {
                TurnPage(this.Pages.IndexOf(this.CurrentPage) + 1);
            }

            base.OnLeftMouseButtonPressed(e);
        }

        private void TurnPage(int index)
        {
            if ((index < this.Pages.Count) && (index >= 0))
            {
                // TODO: Add gw2's book sounds: turn page
                this.CurrentPage = this.Pages[index];

                foreach (var other in this.Pages)
                {
                    other.Visible = other == this.CurrentPage;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // TODO: Title background texture from the original.

            // Draw title
            spriteBatch.DrawStringOnCtrl(this, this._title, this.TitleFont, this._titleBounds, Color.White, false,
                HorizontalAlignment.Left, VerticalAlignment.Top);

            // Draw turn page buttons
            if (!this.MouseOverTurnPageLeft)
            {
                spriteBatch.DrawOnCtrl(this, this.TurnPageSprite, this._leftButtonBounds, this.TurnPageSprite.Bounds,
                    new Color(155, 155, 155, 155), 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, this.TurnPageSprite, this._leftButtonBounds, this.TurnPageSprite.Bounds,
                    Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            }

            if (!this.MouseOverTurnPageRight)
            {
                spriteBatch.DrawOnCtrl(this, this.TurnPageSprite, this._rightButtonBounds, this.TurnPageSprite.Bounds,
                    new Color(155, 155, 155, 155));
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, this.TurnPageSprite, this._rightButtonBounds, this.TurnPageSprite.Bounds,
                    Color.White);
            }
        }
    }
}
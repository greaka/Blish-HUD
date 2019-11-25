using System;
using System.Collections.Generic;
using System.Linq;
using Adhesive;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Blish_HUD.Controls
{
    /// <summary>
    ///     Used to group collections of controls. Can have an accented border and title, if enabled.
    /// </summary>
    public class Panel : Container, IAccordion
    {
        public delegate void BuildUIDelegate(Panel buildPanel, object obj);

        // Used when border is enabled
        private const int TOP_PADDING = 7;
        private const int RIGHT_PADDING = 4;
        private const int BOTTOM_PADDING = 7;
        private const int LEFT_PADDING = 4;

        private const int HEADER_HEIGHT = 36;
        private const int ARROW_SIZE = 32;
        private const int MAX_ACCENT_WIDTH = 256;

        public static readonly DesignStandard MenuStandard = new DesignStandard( /*          Size */
            new Point(265, 700),
            /*   PanelOffset */ new Point(9, 28),
            /* ControlOffset */ ControlStandard.ControlOffset);

        private readonly List<Binding> _scrollbarBindings = new List<Binding>();

        protected AsyncTexture2D _backgroundTexture;

        protected bool _canCollapse;

        protected bool _canScroll;

        private Tween _collapseAnim;

        protected bool _collapsed;
        private Rectangle _layoutAccordionArrowBounds;

        private Vector2 _layoutAccordionArrowOrigin;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;

        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;

        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutLeftAccentSrc;

        private Rectangle _layoutTopLeftAccentBounds;
        private Scrollbar _panelScrollbar;

        private int _preCollapseHeight;

        protected bool _showBorder;

        protected bool _showTint;

        protected string _title;

        public bool CanScroll
        {
            get => this._canScroll;
            set
            {
                if (!SetProperty(ref this._canScroll, value, true)) return;

                UpdateScrollbar();
            }
        }

        public string Title
        {
            get => this._title;
            set => SetProperty(ref this._title, value, true);
        }

        /// <summary>
        ///     A texture to be drawn on the <see cref="Panel" /> before children are drawn.
        /// </summary>
        public AsyncTexture2D BackgroundTexture
        {
            get => this._backgroundTexture;
            set => SetProperty(ref this._backgroundTexture, value);
        }

        public bool ShowBorder
        {
            get => this._showBorder;
            set => SetProperty(ref this._showBorder, value, true);
        }

        public bool ShowTint
        {
            get => this._showTint;
            set => SetProperty(ref this._showTint, value);
        }

        public bool CanCollapse
        {
            get => this._canCollapse;
            set => SetProperty(ref this._canCollapse, value, true);
        }

        // Must remain public for Glide to be able to access the property
        [JsonIgnore] public float ArrowRotation { get; set; } = 0f;
        [JsonIgnore] public float AccentOpacity { get; set; } = 1f;

        [JsonIgnore]
        public bool Collapsed
        {
            get => this._collapsed;
            set
            {
                if (value)
                {
                    Collapse();
                }
                else
                {
                    Expand();
                }
            }
        }

        /// <inheritdoc />
        public bool ToggleAccordionState()
        {
            this.Collapsed = !this._collapsed;

            return this._collapsed;
        }

        /// <inheritdoc />
        public void Expand()
        {
            if (!this._collapsed) return;

            this._collapseAnim?.CancelAndComplete();

            SetProperty(ref this._collapsed, false);

            this._collapseAnim = Animation.Tweener
                .Tween(this,
                    new {Height = this._preCollapseHeight, ArrowRotation = 0f, AccentOpacity = 1f},
                    0.15f)
                .Ease(Ease.QuadOut);
        }

        /// <inheritdoc />
        public void Collapse()
        {
            if (this._collapsed) return;

            // Prevent us from setting the _preCollapseHeight midtransition by accident
            if ((this._collapseAnim != null) && (this._collapseAnim.Completion < 1))
            {
                this._collapseAnim.CancelAndComplete();
            }
            else
            {
                this._preCollapseHeight = this._size.Y;
            }

            SetProperty(ref this._canCollapse, true);
            SetProperty(ref this._collapsed, true);

            this._collapseAnim = Animation.Tweener
                .Tween(this,
                    new
                    {
                        Height = this._layoutHeaderBounds.Bottom, ArrowRotation = -MathHelper.PiOver2,
                        AccentOpacity = 0f
                    },
                    0.15f)
                .Ease(Ease.QuadOut);
        }

        public void NavigateToBuiltPanel(BuildUIDelegate buildCall, object obj)
        {
            this.Children.ToList().ForEach(c => c.Dispose());

            var buildPanel = new Panel
            {
                Size = this._size
            };

            buildCall(buildPanel, obj);

            buildPanel.Parent = this;
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e)
        {
            if (this._canCollapse && this._layoutHeaderBounds.Contains(this.RelativeMousePosition))
            {
                ToggleAccordionState();
            }

            base.OnClick(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            e.ChangedChild.Resized += UpdateContentRegionBounds;
            e.ChangedChild.Moved += UpdateContentRegionBounds;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);

            e.ChangedChild.Resized -= UpdateContentRegionBounds;
            e.ChangedChild.Moved -= UpdateContentRegionBounds;
        }

        private void UpdateContentRegionBounds(object sender, EventArgs e)
        {
            //UpdateScrollbar();
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            var showsHeader = !string.IsNullOrEmpty(this._title);

            var topOffset = showsHeader ? HEADER_HEIGHT : 0;
            var rightOffset = 0;
            var bottomOffset = 0;
            var leftOffset = 0;

            if (this.ShowBorder)
            {
                topOffset = Math.Max(TOP_PADDING, topOffset);
                rightOffset = RIGHT_PADDING;
                bottomOffset = BOTTOM_PADDING;
                leftOffset = LEFT_PADDING;

                // Corner accents
                var cornerAccentWidth = Math.Min(this._size.X, MAX_ACCENT_WIDTH);
                this._layoutTopLeftAccentBounds =
                    new Rectangle(-2, topOffset - 12, cornerAccentWidth, _textureCornerAccent.Height);

                this._layoutBottomRightAccentBounds = new Rectangle(this._size.X - cornerAccentWidth + 2,
                    this._size.Y - 59, cornerAccentWidth, _textureCornerAccent.Height);

                this._layoutCornerAccentSrc = new Rectangle(MAX_ACCENT_WIDTH - cornerAccentWidth, 0, cornerAccentWidth,
                    _textureCornerAccent.Height);

                // Left side accent
                this._layoutLeftAccentBounds = new Rectangle(leftOffset - 7, topOffset, _textureLeftSideAccent.Width,
                    Math.Min(this._size.Y - topOffset - bottomOffset, _textureLeftSideAccent.Height));
                this._layoutLeftAccentSrc =
                    new Rectangle(0, 0, _textureLeftSideAccent.Width, this._layoutLeftAccentBounds.Height);
            }

            this.ContentRegion = new Rectangle(leftOffset,
                topOffset, this._size.X - leftOffset - rightOffset, this._size.Y - topOffset - bottomOffset);

            this._layoutHeaderBounds =
                new Rectangle(this.ContentRegion.Left, 0, this.ContentRegion.Width, HEADER_HEIGHT);
            this._layoutHeaderTextBounds = new Rectangle(this._layoutHeaderBounds.Left + 10, 0,
                this._layoutHeaderBounds.Width - 10, HEADER_HEIGHT);

            this._layoutAccordionArrowOrigin = new Vector2((float) ARROW_SIZE / 2, (float) ARROW_SIZE / 2);
            this._layoutAccordionArrowBounds = new Rectangle(this._layoutHeaderBounds.Right - ARROW_SIZE,
                (topOffset - ARROW_SIZE) / 2,
                ARROW_SIZE,
                ARROW_SIZE).OffsetBy(this._layoutAccordionArrowOrigin.ToPoint());
        }

        private void UpdateScrollbar()
        {
            /* TODO: Fix .CanScroll: currently you have to set it after you set other region changing settings for it
               to work correctly */
            if (this.CanScroll)
            {
                if (this._panelScrollbar == null) this._panelScrollbar = new Scrollbar(this);

                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                this._scrollbarBindings.ForEach(bind => bind.Disable());
                this._scrollbarBindings.Clear();

                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.Parent,
                    () => this.Parent, applyLeft: true));

                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.Height,
                    () => this.Height, h => this.ContentRegion.Height - 20, true));

                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.Right,
                    () => this.Right, r => r - this._panelScrollbar.Width / 2, true));

                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.Top, () => this.Top,
                    t => t + this.ContentRegion.Top + 10, true));

                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.Visible,
                    () => this.Visible, applyLeft: true));

                // Ensure scrollbar is visible
                this._scrollbarBindings.Add(Binding.CreateOneWayBinding(() => this._panelScrollbar.ZIndex,
                    () => this.ZIndex, z => z + 2, true));
            }
            else
            {
                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                this._scrollbarBindings.ForEach(bind => bind.Disable());
                this._scrollbarBindings.Clear();

                this._panelScrollbar?.Dispose();
                this._panelScrollbar = null;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._backgroundTexture != null)
            {
                spriteBatch.DrawOnCtrl(this, this._backgroundTexture, bounds);
            }

            if (this._showTint)
            {
                spriteBatch.DrawOnCtrl(this,
                    ContentService.Textures.Pixel,
                    this.ContentRegion,
                    Color.Black * 0.4f);
            }

            if (!string.IsNullOrEmpty(this._title))
            {
                spriteBatch.DrawOnCtrl(this,
                    _texturePanelHeader, this._layoutHeaderBounds);

                // Panel header
                if (this._canCollapse && this._mouseOver && (this.RelativeMousePosition.Y <= HEADER_HEIGHT))
                {
                    spriteBatch.DrawOnCtrl(this,
                        _texturePanelHeaderActive, this._layoutHeaderBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this,
                        _texturePanelHeader, this._layoutHeaderBounds);
                }

                // Panel header text
                spriteBatch.DrawStringOnCtrl(this, this._title,
                    Content.DefaultFont16, this._layoutHeaderTextBounds,
                    Color.White);

                if (this._canCollapse)
                {
                    // Collapse arrow
                    spriteBatch.DrawOnCtrl(this,
                        _textureAccordionArrow, this._layoutAccordionArrowBounds,
                        null,
                        Color.White,
                        this.ArrowRotation, this._layoutAccordionArrowOrigin);
                }
            }

            if (this.ShowBorder)
            {
                // Lightly tint the background of the panel
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.ContentRegion,
                    Color.Black * (0.1f * this.AccentOpacity));

                // Top left accent
                spriteBatch.DrawOnCtrl(this,
                    _textureCornerAccent, this._layoutTopLeftAccentBounds, this._layoutCornerAccentSrc,
                    Color.White * this.AccentOpacity,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally);

                // Bottom right accent
                spriteBatch.DrawOnCtrl(this,
                    _textureCornerAccent, this._layoutBottomRightAccentBounds, this._layoutCornerAccentSrc,
                    Color.White * this.AccentOpacity,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically);

                // Left side accent
                spriteBatch.DrawOnCtrl(this,
                    _textureLeftSideAccent, this._layoutLeftAccentBounds, this._layoutLeftAccentSrc,
                    Color.Black * this.AccentOpacity,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically);
            }
        }

        protected override void DisposeControl()
        {
            this._panelScrollbar?.Dispose();

            base.DisposeControl();
        }

        #region Load Static

        private static readonly Texture2D _texturePanelHeader;
        private static readonly Texture2D _texturePanelHeaderActive;

        private static readonly Texture2D _textureCornerAccent;
        private static readonly Texture2D _textureLeftSideAccent;

        private static readonly Texture2D _textureAccordionArrow;

        static Panel()
        {
            _texturePanelHeader = Content.GetTexture(@"controls\panel\1032325");
            _texturePanelHeaderActive = Content.GetTexture(@"controls\panel\1032324");

            _textureCornerAccent = Content.GetTexture(@"controls\panel\1002144");
            _textureLeftSideAccent = Content.GetTexture("605025");

            _textureAccordionArrow = Content.GetTexture(@"controls\panel\155953");
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public abstract class WindowBase : Container
    {
        private const int COMMON_MARGIN = 16;
        private const int TITLE_OFFSET = 80;
        private const int SUBTITLE_OFFSET = 20;

        private readonly Tween _animFade;

        private Panel _activePanel;

        protected bool _canResize;

        protected Texture2D _emblem;

        protected bool _hoverClose;

        protected string _subtitle = "";

        protected string _title = "No Title";

        protected bool _topMost;

        protected bool Dragging;
        protected Point DragStart = Point.Zero;

        protected bool StandardWindow;

        public WindowBase()
        {
            this.Opacity = 0f;
            this.Visible = false;

            this.ZIndex = Screen.WINDOW_BASEZINDEX;

            Input.LeftMouseButtonReleased += delegate { this.Dragging = false; };

            this._animFade = Animation.Tweener.Tween(this, new {Opacity = 1f}, 0.2f).Repeat().Reflect();
            this._animFade.Pause();

            this._animFade.OnComplete(() =>
            {
                this._animFade.Pause();
                if (this._opacity <= 0) this.Visible = false;
            });
        }

        /// <summary>
        ///     The text shown at the top of the window.
        /// </summary>
        public string Title
        {
            get => this._title;
            set => SetProperty(ref this._title, value, true);
        }

        /// <summary>
        ///     The text shown to the right of the title in the title bar.
        ///     This text is smaller and is normally used to show the current tab name and/or hotkey used to open the window.
        /// </summary>
        public string Subtitle
        {
            get => this._subtitle;
            set => SetProperty(ref this._subtitle, value);
        }

        /// <summary>
        ///     Allows the window to be resized by dragging the bottom right corner.
        /// </summary>
        /// <remarks>This property has not been implemented.</remarks>
        public bool CanResize
        {
            get => this._canResize;
            set => SetProperty(ref this._canResize, value);
        }

        /// <summary>
        ///     The emblem/badge displayed in the top left corner of the window.
        /// </summary>
        public Texture2D Emblem
        {
            get => this._emblem;
            set => SetProperty(ref this._emblem, value);
        }

        /// <summary>
        ///     If this window will show on top of all other windows, regardless of which one had focus last.
        /// </summary>
        /// <remarks>This property has not been implemented.</remarks>
        public bool TopMost
        {
            get => this._topMost;
            set => SetProperty(ref this._topMost, value);
        }

        public Panel ActivePanel
        {
            get => this._activePanel;
            set
            {
                if (this._activePanel != null)
                {
                    this._activePanel.Hide();
                    this._activePanel.Parent = null;
                }

                if (value == null) return;

                this._activePanel = value;

                this._activePanel.Parent = this;
                this._activePanel.Location = Point.Zero;
                this._activePanel.Size = this.ContentRegion.Size;

                this._activePanel.Visible = true;
            }
        }

        protected bool HoverClose
        {
            get => this._hoverClose;
            private set => SetProperty(ref this._hoverClose, value);
        }

        protected virtual void ConstructWindow(Texture2D background, Vector2 backgroundOrigin,
            Rectangle? windowBackgroundBounds = null, Thickness outerPadding = default, int titleBarHeight = 0,
            bool standardWindow = true)
        {
            this.StandardWindow = standardWindow;

            this._windowBackground = background;
            this._windowBackgroundOrigin = backgroundOrigin; //.OffsetBy(0, -titleBarHeight);

            var tempBounds = windowBackgroundBounds ?? background.Bounds;

            this._titleBarBounds = new Rectangle(0, 0, tempBounds.Width, titleBarHeight);

            this.Size = tempBounds.Size;

            this.Padding = outerPadding;

            this._windowBackgroundBounds = new Rectangle(0, titleBarHeight,
                tempBounds.Width + (int) this._padding.Right + (int) this._padding.Left,
                tempBounds.Height + (int) this._padding.Bottom);
        }

        public override void RecalculateLayout()
        {
            // Title bar bounds
            var titleBarDrawOffset = this._titleBarBounds.Y -
                                     (_textureTitleBarLeft.Height / 2 - this._titleBarBounds.Height / 2);
            var titleBarRightWidth = _textureTitleBarRight.Width - COMMON_MARGIN;

            this._layoutLeftTitleBarBounds = new Rectangle(this._titleBarBounds.X, titleBarDrawOffset,
                Math.Min(this._titleBarBounds.Width - titleBarRightWidth,
                    this._windowBackgroundBounds.Width - titleBarRightWidth), _textureTitleBarLeft.Height);
            this._layoutRightTitleBarBounds = new Rectangle(this._titleBarBounds.Right - titleBarRightWidth,
                titleBarDrawOffset, _textureTitleBarRight.Width, _textureTitleBarRight.Height);

            // Title bar text bounds
            if (!string.IsNullOrEmpty(this._title) && !string.IsNullOrEmpty(this._subtitle))
            {
                var titleTextWidth = (int) Content.DefaultFont32.MeasureString(this._title).Width;

                this._layoutSubtitleBounds =
                    this._layoutLeftTitleBarBounds.OffsetBy(TITLE_OFFSET + titleTextWidth + SUBTITLE_OFFSET, 0);
            }


            // Title bar exit button bounds
            this._layoutExitButtonBounds = new Rectangle(
                this._layoutRightTitleBarBounds.Right - COMMON_MARGIN * 2 - _textureExitButton.Width,
                this._layoutRightTitleBarBounds.Y + COMMON_MARGIN,
                _textureExitButton.Width,
                _textureExitButton.Height);

            // Corner edge bounds
            this._layoutWindowCornerBounds = new Rectangle(
                this._layoutRightTitleBarBounds.Right - _textureWindowCorner.Width - COMMON_MARGIN,
                this.ContentRegion.Bottom - _textureWindowCorner.Height + COMMON_MARGIN,
                _textureWindowCorner.Width,
                _textureWindowCorner.Height);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            this.MouseOverTitleBar = false;
            this.MouseOverExitButton = false;
            this.MouseOverCornerResize = false;

            if (this.RelativeMousePosition.Y < this._titleBarBounds.Bottom)
            {
                if (this._layoutExitButtonBounds.Contains(this.RelativeMousePosition))
                {
                    this.MouseOverExitButton = true;
                }
                else
                {
                    this.MouseOverTitleBar = true;
                }
            }
            else if (this._canResize && this._layoutWindowCornerBounds.Contains(this.RelativeMousePosition))
            {
                // TODO: Reduce the size of the corner resize area - compare to in game region for reference
                this.MouseOverCornerResize = true;
            }

            base.OnMouseMoved(e);
        }

        /// <inheritdoc />
        protected override void OnMouseLeft(MouseEventArgs e)
        {
            this.MouseOverTitleBar = false;
            this.MouseOverExitButton = false;
            this.MouseOverCornerResize = false;

            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (this.MouseOverTitleBar)
            {
                this.Dragging = true;
                this.DragStart = Input.MouseState.Position;
            }
            else if (this.MouseOverExitButton)
            {
                Hide();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            this.Dragging = false;

            base.OnLeftMouseButtonReleased(e);
        }

        public void ToggleWindow()
        {
            if (this._visible) Hide();
            else Show();
        }

        public override void Show()
        {
            if (this._visible) return;

            // TODO: Ensure window can't also go off too far to the right or bottom
            this.Location = new Point(
                Math.Max(0, this._location.X),
                Math.Max(0, this._location.Y)
            );

            this.Opacity = 0;
            this.Visible = true;

            this._animFade.Resume();
        }

        public override void Hide()
        {
            if (!this.Visible) return;

            this._animFade.Resume();
            Content.PlaySoundEffectByName(@"audio\window-close");
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            if (this.Dragging)
            {
                var nOffset = Input.MouseState.Position - this.DragStart;
                this.Location += nOffset;

                this.DragStart = Input.MouseState.Position;
            }
        }

        #region Load Static

        private static readonly Texture2D _textureTitleBarLeft;
        private static readonly Texture2D _textureTitleBarRight;
        private static readonly Texture2D _textureTitleBarLeftActive;
        private static readonly Texture2D _textureTitleBarRightActive;

        private static readonly Texture2D _textureExitButton;
        private static readonly Texture2D _textureExitButtonActive;

        private static readonly Texture2D _textureWindowCorner;
        private static readonly Texture2D _textureWindowResizableCorner;
        private static readonly Texture2D _textureWindowResizableCornerActive;

        static WindowBase()
        {
            _textureTitleBarLeft = Content.GetTexture("titlebar-inactive");
            _textureTitleBarRight = Content.GetTexture("window-topright");
            _textureTitleBarLeftActive = Content.GetTexture("titlebar-active");
            _textureTitleBarRightActive = Content.GetTexture("window-topright-active");

            _textureExitButton = Content.GetTexture("button-exit");
            _textureExitButtonActive = Content.GetTexture("button-exit-active");

            _textureWindowCorner = Content.GetTexture(@"controls\window\156008");
            _textureWindowResizableCorner = Content.GetTexture(@"controls\window\156009");
            _textureWindowResizableCornerActive = Content.GetTexture(@"controls\window\156010");
        }

        #endregion

        #region Window Construction

        protected Texture2D _windowBackground;
        protected Vector2 _windowBackgroundOrigin;
        protected Rectangle _windowBackgroundBounds;
        protected Rectangle _titleBarBounds;

        #endregion

        #region Calculated Layout

        private Rectangle _layoutLeftTitleBarBounds;
        private Rectangle _layoutRightTitleBarBounds;

        private Rectangle _layoutSubtitleBounds;

        private Rectangle _layoutExitButtonBounds;

        private Rectangle _layoutWindowCornerBounds;

        #endregion

        #region Region States

        protected bool MouseOverTitleBar;
        protected bool MouseOverExitButton;
        protected bool MouseOverCornerResize;

        #endregion

        #region Window Navigation

        private readonly LinkedList<Panel> _currentNav = new LinkedList<Panel>();

        public void Navigate(Panel newPanel, bool keepHistory = true)
        {
            if (!keepHistory) this._currentNav.Clear();

            this._currentNav.AddLast(newPanel);

            this.ActivePanel = newPanel;
        }

        public void NavigateBack()
        {
            if (this._currentNav.Count > 1) this._currentNav.RemoveLast();

            this.ActivePanel = this._currentNav.Last.Value;
        }

        public void NavigateHome()
        {
            this.ActivePanel = this._currentNav.First.Value;

            this._currentNav.Clear();

            this._currentNav.AddFirst(this.ActivePanel);
        }

        #endregion

        #region Paint Window

        protected virtual void PaintWindowBackground(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, this._windowBackground,
                bounds,
                null,
                Color.White,
                0f, this._windowBackgroundOrigin);
        }

        protected virtual void PaintExitButton(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, this.MouseOverExitButton
                    ? _textureExitButtonActive
                    : _textureExitButton,
                bounds);
        }

        protected virtual void PaintTitleBar(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._mouseOver && this.MouseOverTitleBar)
            {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeftActive, this._layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeftActive, this._layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, this._layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, this._layoutRightTitleBarBounds);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft, this._layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft, this._layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRight, this._layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRight, this._layoutRightTitleBarBounds);
            }

            if (!string.IsNullOrEmpty(this._title))
            {
                spriteBatch.DrawStringOnCtrl(this, this._title,
                    Content.DefaultFont32, this._layoutLeftTitleBarBounds.OffsetBy(80, 0),
                    ContentService.Colors.ColonialWhite);

                if (!string.IsNullOrEmpty(this._subtitle))
                {
                    spriteBatch.DrawStringOnCtrl(this, this._subtitle,
                        Content.DefaultFont16, this._layoutSubtitleBounds,
                        Color.White);
                }
            }
        }

        protected virtual void PaintEmblem(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._emblem != null)
            {
                spriteBatch.DrawOnCtrl(this, this._emblem,
                    this._emblem.Bounds.Subtract(new Rectangle(this._emblem.Width / 8, this._emblem.Height / 4, 0, 0)));
            }
        }

        protected virtual void PaintCorner(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._canResize)
            {
                spriteBatch.DrawOnCtrl(this, this.MouseOverCornerResize
                    ? _textureWindowResizableCornerActive
                    : _textureWindowResizableCorner, this._layoutWindowCornerBounds);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this,
                    _textureWindowCorner, this._layoutWindowCornerBounds);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.StandardWindow)
            {
                PaintWindowBackground(spriteBatch, this._windowBackgroundBounds.Subtract(new Rectangle(0, -4, 0, 0)));
                PaintTitleBar(spriteBatch, bounds);
            }

            PaintExitButton(spriteBatch, this._layoutExitButtonBounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.StandardWindow)
            {
                PaintEmblem(spriteBatch, bounds);

                PaintCorner(spriteBatch, bounds);
            }
        }

        #endregion
    }
}
using System;
using System.Linq;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls
{
    public class Scrollbar : Control
    {
        private const int CONTROL_WIDTH = 12;
        private const int MIN_LENGTH = 32;
        private const int CAP_SLACK = 6;

        private Container _associatedContainer;

        private int _containerLowestContent;

        private int _scrollbarHeight = MIN_LENGTH;

        private float _scrollDistance;

        private float _targetScrollDistance;

        private Tween _targetScrollDistanceAnim;
        private Rectangle BarBounds;
        private Rectangle DownArrowBounds;

        private double ScrollbarPercent = 1.0;

        private bool Scrolling;
        private int ScrollingOffset;
        private Rectangle TrackBounds;

        private Rectangle UpArrowBounds;

        public Scrollbar(Container container)
        {
            this._associatedContainer = container;

            this.UpArrowBounds = Rectangle.Empty;
            this.DownArrowBounds = Rectangle.Empty;
            this.BarBounds = Rectangle.Empty;
            this.TrackBounds = Rectangle.Empty;

            this.Width = CONTROL_WIDTH;

            Input.LeftMouseButtonReleased += delegate
            {
                if (this.Scrolling)
                {
                    this.Scrolling = false; /* Invalidate(); */
                }
            };

            this._associatedContainer.MouseWheelScrolled += HandleWheelScroll;
        }

        private float TargetScrollDistance
        {
            get
            {
                if (this._targetScrollDistanceAnim == null) return this._scrollDistance;

                return this._targetScrollDistance;
            }
            set
            {
                var aVal = MathHelper.Clamp(value, 0f, 1f);
                if ((this._associatedContainer != null) && (this._targetScrollDistance != aVal))
                    this._targetScrollDistance = aVal;

                Invalidate();
            }
        }

        public float ScrollDistance
        {
            get => this._scrollDistance;
            set
            {
                if (!SetProperty(ref this._scrollDistance, MathHelper.Clamp(value, 0f, 1f), true)) return;

                UpdateAssocContainer();
            }
        }

        private int ScrollbarHeight
        {
            get => this._scrollbarHeight;
            set
            {
                if (!SetProperty(ref this._scrollbarHeight, value, true)) return;

                // Reclamps the scrolling content
                RecalculateScrollbarSize();
                UpdateAssocContainer();
            }
        }

        public Container AssociatedContainer
        {
            get => this._associatedContainer;
            set => SetProperty(ref this._associatedContainer, value);
        }

        private int TrackLength => this._size.Y - _textureUpArrow.Height - _textureDownArrow.Height;

        protected override void OnMouseWheelScrolled(MouseEventArgs e)
        {
            HandleWheelScroll(this, e);

            base.OnMouseWheelScrolled(e);
        }

        private void HandleWheelScroll(object sender, MouseEventArgs e)
        {
            // Don't scroll if the scrollbar isn't visible
            if (!this.Visible || (this.ScrollbarPercent > 0.99)) return;

            // Avoid scrolling nested panels
            var ctrl = (Control) sender;
            while ((ctrl != this._associatedContainer) && (ctrl != null))
            {
                if (ctrl is Panel) return;
                ctrl = ctrl.Parent;
            }

            var normalScroll = Input.ClickState.EventDetails.wheelDelta /
                               (float) Math.Abs(Input.ClickState.EventDetails.wheelDelta);

            this._targetScrollDistanceAnim?.Cancel();

            this.TargetScrollDistance += normalScroll * -0.08f;

            this._targetScrollDistanceAnim = Animation.Tweener
                .Tween(this, new {ScrollDistance = this.TargetScrollDistance}, 0.35f)
                .Ease(Ease.QuadOut)
                .OnComplete(() => this._targetScrollDistanceAnim = null);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse | CaptureType.MouseWheel;
        }

        private void UpdateAssocContainer()
        {
            this.AssociatedContainer.VerticalScrollOffset = (int) Math.Floor(
                (this._containerLowestContent - this.AssociatedContainer.ContentRegion.Height) * this.ScrollDistance);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            var relMousePos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (this.BarBounds.Contains(relMousePos))
            {
                this.Scrolling = true;
                this.ScrollingOffset = relMousePos.Y - this.BarBounds.Y;
                this.TargetScrollDistance = this.ScrollDistance;
            }
            else if (this.UpArrowBounds.Contains(relMousePos))
            {
                this.ScrollDistance -= 0.02f;
            }
            else if (this.DownArrowBounds.Contains(relMousePos))
            {
                this.ScrollDistance += 0.02f;
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (this.Scrolling)
            {
                var relMousePos = Input.MouseState.Position - this.AbsoluteBounds.Location -
                                  new Point(0, this.ScrollingOffset) - this.TrackBounds.Location;

                this.ScrollDistance = relMousePos.Y / (float) (this.TrackLength - this.ScrollbarHeight);
                this.TargetScrollDistance = this.ScrollDistance;
            }

            Invalidate();
        }

        public override void Invalidate()
        {
            var _lastVal = this.ScrollbarPercent;
            RecalculateScrollbarSize();

            if ((_lastVal != this.ScrollbarPercent) && (this._associatedContainer != null))
            {
                this.ScrollDistance = 0;
                this.TargetScrollDistance = 0;
            }

            this.UpArrowBounds = new Rectangle(this.Width / 2 - _textureUpArrow.Width / 2, 0, _textureUpArrow.Width,
                _textureUpArrow.Height);
            this.DownArrowBounds = new Rectangle(this.Width / 2 - _textureDownArrow.Width / 2,
                this.Height - _textureDownArrow.Height, _textureDownArrow.Width, _textureDownArrow.Height);
            this.BarBounds = new Rectangle(this.Width / 2 - _textureBar.Width / 2,
                (int) (this.ScrollDistance * (this.TrackLength - this.ScrollbarHeight)) + _textureUpArrow.Height,
                _textureBar.Width, this.ScrollbarHeight);
            this.TrackBounds = new Rectangle(this.Width / 2 - _textureTrack.Width / 2, this.UpArrowBounds.Bottom,
                _textureTrack.Width, this.TrackLength);

            base.Invalidate();
        }

        private void RecalculateScrollbarSize()
        {
            if (this._associatedContainer == null) return;

            this._containerLowestContent = Math.Max(this._associatedContainer.Children.Any(c => c.Visible)
                ? this._associatedContainer.Children.Where(c => c.Visible).Max(c => c.Bottom)
                : 0, this._associatedContainer.ContentRegion.Height);

            this.ScrollbarPercent =
                this._associatedContainer.ContentRegion.Height / (double) this._containerLowestContent;

            this.ScrollbarHeight = (int) Math.Max(Math.Floor(this.TrackLength * this.ScrollbarPercent) - 1, MIN_LENGTH);

            UpdateAssocContainer();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Don't show the scrollbar if there is nothing to scroll
            if (this.ScrollbarPercent > 0.99) return;

            var drawTint = (!this.Scrolling && this.MouseOver) ||
                           ((this._associatedContainer != null) && this._associatedContainer.MouseOver)
                ? Color.White
                : ContentService.Colors.Darkened(0.6f);

            drawTint = this.Scrolling
                ? ContentService.Colors.Darkened(0.9f)
                : drawTint;

            spriteBatch.DrawOnCtrl(this, _textureTrack, this.TrackBounds);

            spriteBatch.DrawOnCtrl(this, _textureUpArrow, this.UpArrowBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, _textureDownArrow, this.DownArrowBounds, drawTint);

            spriteBatch.DrawOnCtrl(this, _textureBar, this.BarBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, _textureTopCap,
                new Rectangle(this.Width / 2 - _textureTopCap.Width / 2, this.BarBounds.Top - CAP_SLACK,
                    _textureTopCap.Width, _textureTopCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureBottomCap,
                new Rectangle(this.Width / 2 - _textureBottomCap.Width / 2,
                    this.BarBounds.Bottom - _textureBottomCap.Height + CAP_SLACK, _textureBottomCap.Width,
                    _textureBottomCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureThumb,
                new Rectangle(this.Width / 2 - _textureThumb.Width / 2,
                    this.BarBounds.Top + (this.ScrollbarHeight / 2 - _textureThumb.Height / 2), _textureThumb.Width,
                    _textureThumb.Height), drawTint);
        }

        #region Load Static

        private static readonly TextureRegion2D _textureTrack;
        private static readonly TextureRegion2D _textureUpArrow;
        private static readonly TextureRegion2D _textureDownArrow;
        private static readonly TextureRegion2D _textureBar;
        private static readonly TextureRegion2D _textureThumb;
        private static readonly TextureRegion2D _textureTopCap;
        private static readonly TextureRegion2D _textureBottomCap;

        static Scrollbar()
        {
            _textureTrack = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-track");
            _textureUpArrow = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-up");
            _textureDownArrow = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-down");
            _textureBar = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-bar-active");
            _textureThumb = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-thumb");
            _textureTopCap = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-top");
            _textureBottomCap = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-bottom");
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class Tooltip : Container
    {
        private const int PADDING = 2;

        private const int MOUSE_VERTICAL_MARGIN = 18;

        private Tween _animFadeLifecycle;

        public Tooltip()
        {
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;

            this.Padding = new Thickness(PADDING);

            ChildAdded += Tooltip_ChildChanged;
            ChildRemoved += Tooltip_ChildChanged;

            _allTooltips.Add(this);
        }

        public Control CurrentControl { get; set; }

        private void Tooltip_ChildChanged(object sender, ChildChangedEventArgs e)
        {
            Invalidate();

            // Ensure we don't miss it if a child control is resized or is moved
            if (e.Added)
            {
                e.ChangedChild.Resized += delegate { Invalidate(); };
                e.ChangedChild.Moved += delegate { Invalidate(); };
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            if ((this.CurrentControl != null) && !this.CurrentControl.Visible)
            {
                this.Visible = false;
                this.CurrentControl = null;
            }
        }

        /// <summary>
        ///     Shows the tooltip at the provided <see cref="x" /> and <see cref="y" /> coordinates.
        /// </summary>
        public void Show(int x, int y)
        {
            Show(new Point(x, y));
        }

        /// <summary>
        ///     Shows the tooltip at the provided <see cref="location" />.
        /// </summary>
        public void Show(Point location)
        {
            this.Location = location;

            Show();
        }

        /// <inheritdoc />
        public override void Show()
        {
            this.Opacity = 0f;

            if (this._animFadeLifecycle == null)
            {
                this._animFadeLifecycle = Animation.Tweener.Tween(this, new {Opacity = 1f}, 0.1f);
            }

            this.Parent = Graphics.SpriteScreen;

            base.Show();
        }

        /// <inheritdoc />
        public override void Hide()
        {
            this._animFadeLifecycle?.Cancel();
            this._animFadeLifecycle = null;

            this.Parent = null;

            base.Hide();
        }

        public override void RecalculateLayout()
        {
            var visibleChildren = this._children.Where(c => c.Visible).ToList();

            var boundsWidth = 0;
            var boundsHeight = 0;

            if (visibleChildren.Count > 0)
            {
                boundsWidth = visibleChildren.Max(c => c.Right);
                boundsHeight = visibleChildren.Max(c => c.Bottom);
            }

            this.Size = new Point((int) (_contentEdgeBuffer.Left + boundsWidth + _contentEdgeBuffer.Right),
                (int) (_contentEdgeBuffer.Top + boundsHeight + _contentEdgeBuffer.Bottom));

            this.ContentRegion = new Rectangle((int) _contentEdgeBuffer.Left,
                (int) _contentEdgeBuffer.Top,
                (int) (this._size.X - _contentEdgeBuffer.Left - _contentEdgeBuffer.Right),
                (int) (this._size.Y - _contentEdgeBuffer.Top - _contentEdgeBuffer.Bottom));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _textureTooltip, bounds, new Rectangle(3, 4, this._size.X, this._size.Y),
                Color.White * 0.98f);

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(1, 0, this._size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(1, 1, this._size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(this._size.X - 3, 1, 3, this._size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2),
                Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(this._size.X - 2, 1, 1, this._size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2),
                Color.Black * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(1, this._size.Y - 3, this._size.X - 2, 3).Add(-PADDING, PADDING, PADDING * 2, 0),
                Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(1, this._size.Y - 2, this._size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0),
                Color.Black * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(0, 1, 3, this._size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                new Rectangle(1, 1, 1, this._size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.6f);
        }

        #region Load Static

        private static readonly Thickness _contentEdgeBuffer;

        private static readonly List<Tooltip> _allTooltips;

        private static readonly Texture2D _textureTooltip;

        static Tooltip()
        {
            _contentEdgeBuffer = new Thickness(4, 4, 3, 6);

            _textureTooltip = Content.GetTexture("tooltip");

            _allTooltips = new List<Tooltip>();

            ActiveControlChanged += ControlOnActiveControlChanged;

            Input.MouseMoved += delegate
            {
                if (ActiveControl?.Tooltip != null)
                {
                    ActiveControl.Tooltip.CurrentControl = ActiveControl;
                    UpdateTooltipPosition(ActiveControl.Tooltip);

                    if (!ActiveControl.Tooltip.Visible)
                        ActiveControl.Tooltip.Show();
                }
            };
        }

        private static Control _prevControl;

        private static void ControlOnActiveControlChanged(object sender, ControlActivatedEventArgs e)
        {
            foreach (var tooltip in _allTooltips)
            {
                tooltip.Hide();
            }

            if (_prevControl != null)
            {
                _prevControl.Hidden -= ActivatedControlOnHidden;
                _prevControl.Disposed -= ActivatedControlOnHidden;
            }

            _prevControl = e.ActivatedControl;

            if (_prevControl != null)
            {
                e.ActivatedControl.Hidden += ActivatedControlOnHidden;
                e.ActivatedControl.Disposed += ActivatedControlOnHidden;
            }
        }

        private static void ActivatedControlOnHidden(object sender, EventArgs e)
        {
            foreach (var tooltip in _allTooltips)
            {
                tooltip.Hide();
            }
        }

        private static void UpdateTooltipPosition(Tooltip tooltip)
        {
            var topPos = Input.MouseState.Position.Y - MOUSE_VERTICAL_MARGIN - tooltip.Height > 0
                ? -MOUSE_VERTICAL_MARGIN - tooltip.Height
                : MOUSE_VERTICAL_MARGIN * 2;

            var leftPos = Input.MouseState.Position.X + tooltip.Width < Graphics.SpriteScreen.Width
                ? 0
                : -tooltip.Width;

            tooltip.Location = Input.MouseState.Position + new Point(leftPos, topPos);
        }

        #endregion
    }
}
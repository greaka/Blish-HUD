using System;
using System.Collections.Generic;
using System.Linq;
using Adhesive;
using Blish_HUD.Content;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Controls.Resources;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;

namespace Blish_HUD.Controls
{
    public class MenuItem : Container, IMenuItem, ICheckable, IAccordion
    {
        private const int DEFAULT_ITEM_HEIGHT = 32;

        private const int ICON_PADDING = 10;
        private const int ICON_SIZE = 32;

        private const int ARROW_SIZE = 16;
        private ScrollingHighlightEffect _scrollEffect;

        private Tween _slideAnim;

        public MenuItem()
        {
            Initialize();
        }

        public MenuItem(string text)
        {
            this._text = text;

            Initialize();
        }

        /// <inheritdoc />
        public bool ToggleAccordionState()
        {
            this.Collapsed = !this._collapsed;

            return this._collapsed;
        }

        public void Expand()
        {
            if (!this._collapsed) return;

            this._slideAnim?.CancelAndComplete();

            SetProperty(ref this._collapsed, false);

            this._slideAnim = Animation.Tweener
                .Tween(this,
                    new {ArrowRotation = 0f},
                    0.3f)
                .Ease(Ease.QuadOut);

            this.Height = this.ContentRegion.Bottom;
        }

        public void Collapse()
        {
            if (this._collapsed) return;

            this._slideAnim?.CancelAndComplete();

            SetProperty(ref this._collapsed, true);

            this._slideAnim = Animation.Tweener
                .Tween(this,
                    new {ArrowRotation = -MathHelper.PiOver2},
                    0.3f)
                .Ease(Ease.QuadOut);

            this.Height = this.MenuItemHeight;
        }

        private void Initialize()
        {
            this._scrollEffect = new ScrollingHighlightEffect(this);

            this.EffectBehind = this._scrollEffect;

            this.Height = this.MenuItemHeight;

            this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, 0);
        }

        public override void RecalculateLayout()
        {
            this._scrollEffect.Size = new Vector2(this._size.X, this._menuItemHeight);

            UpdateContentRegion();
        }

        private void UpdateContentRegion()
        {
            if (this._children.Any())
            {
                this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this._size.X,
                    this._children.Where(c => c.Visible).Max(c => c.Bottom));
            }
            else
            {
                this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this._size.X, 0);
            }

            this.Height = !this._collapsed
                ? this.ContentRegion.Bottom
                : this.MenuItemHeight;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            foreach (var childMenuItem in this._children)
            {
                childMenuItem.Width = e.CurrentSize.X;
            }

            base.OnResized(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (this._canCheck && this.MouseOverIconBox)
            {
                /* Mouse was clicked inside of the checkbox */

                this.Checked = !this.Checked;
            }
            else if (this._overSection && this._children.Any())
            {
                /* Mouse was clicked inside of the mainbody of the MenuItem */

                ToggleAccordionState();
            }
            else if (this._overSection && this._canCheck)
            {
                /* Mouse was clicked inside of the mainbody of the MenuItem,
                                                          but we have no children, so we toggle checkbox */

                this.Checked = !this.Checked;
            }

            if (!this._children.Any())
            {
                Select();
            }

            base.OnClick(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            // Helps us know when the mouse is over the MenuItem itself, or actually over its children
            this.OverSection = this.RelativeMousePosition.Y <= this._menuItemHeight;

            if (this.OverSection)
                this._scrollEffect.Enable();
            else
                this._scrollEffect.Disable();

            // Used if this menu item has its checkbox enabled
            this.MouseOverIconBox = this._canCheck
                                    && this._overSection
                                    && this.FirstItemBoxRegion
                                        .OffsetBy(this.LeftSidePadding, 0)
                                        .Contains(this.RelativeMousePosition);

            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            this.OverSection = false;

            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (!(e.ChangedChild is MenuItem newChild))
            {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;
            newChild.MenuDepth = this.MenuDepth + 1;

            // We'll bind the top of the new control to the bottom of the last control we added
            var lastItem = this._children.LastOrDefault();
            if (lastItem != null)
                Binding.CreateOneWayBinding(() => e.ChangedChild.Top,
                    () => lastItem.Bottom, applyLeft: true);
        }

        private void DrawDropdownArrow(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var arrowOrigin = new Vector2((float) ARROW_SIZE / 2, (float) ARROW_SIZE / 2);

            var arrowDest = new Rectangle(5 + ARROW_SIZE / 2,
                this.MenuItemHeight / 2,
                ARROW_SIZE,
                ARROW_SIZE);

            spriteBatch.DrawOnCtrl(this,
                _textureArrow,
                arrowDest,
                null,
                Color.White,
                this.ArrowRotation,
                arrowOrigin);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var currentLeftSidePadding = this.LeftSidePadding;

            // If MenuItem has children, show dropdown arrow
            if (this._children.Any())
                DrawDropdownArrow(spriteBatch, bounds);

            TextureRegion2D firstItemSprite = null;

            if (this.CanCheck)
            {
                var state = this.Checked ? "-checked" : "-unchecked";

                var extension = "";
                extension = this.MouseOverIconBox ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;

                firstItemSprite =
                    Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}");
            }
            else if ((this.Icon != null) && !this.Children.Any())
            {
                // performance?
                firstItemSprite = new TextureRegion2D(this.Icon);
            }

            // Draw either the checkbox or the icon, if one or the either is available
            if (firstItemSprite != null)
            {
                spriteBatch.DrawOnCtrl(this,
                    firstItemSprite,
                    this.FirstItemBoxRegion.OffsetBy(currentLeftSidePadding, 0));
            }

            if (this._canCheck)
            {
                currentLeftSidePadding += ICON_SIZE + ICON_PADDING;
            }
            else if (this._children.Any())
            {
                currentLeftSidePadding += ICON_PADDING;
            }
            else if (this._icon != null)
            {
                currentLeftSidePadding += ICON_SIZE + ICON_PADDING;
            }

            // TODO: Evaluate menu item text color
            // Technically, this text color should be Color.FromNonPremultiplied(255, 238, 187, 255),
            // but it doesn't look good on the purple background of the main window
            spriteBatch.DrawStringOnCtrl(this, this._text, Content.DefaultFont16,
                new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING),
                    this.MenuItemHeight), Color.White, true, true);
        }

        #region Load Static

        private static readonly Texture2D _textureArrow;

        static MenuItem()
        {
            _textureArrow = Content.GetTexture("156057");
        }

        #endregion

        #region Events

        public event EventHandler<ControlActivatedEventArgs> ItemSelected;

        protected virtual void OnItemSelected(ControlActivatedEventArgs e)
        {
            ItemSelected?.Invoke(this, e);
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        protected virtual void OnCheckedChanged(CheckChangedEvent e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        #endregion

        #region Properties

        protected int _menuItemHeight = DEFAULT_ITEM_HEIGHT;

        public int MenuItemHeight
        {
            get => this._menuItemHeight;
            set
            {
                if (!SetProperty(ref this._menuItemHeight, value, true)) return;

                this.Height = this._menuItemHeight;

                // Update all children to ensure they match in height
                foreach (var childMenuItem in this._children.Cast<IMenuItem>().ToList())
                {
                    childMenuItem.MenuItemHeight = this._menuItemHeight;
                }
            }
        }

        protected bool _shouldShift;

        public bool ShouldShift
        {
            get => this._shouldShift;
            set => SetProperty(ref this._shouldShift, value, true);
        }

        public bool Selected => this._selectedMenuItem == this;

        protected MenuItem _selectedMenuItem;

        public MenuItem SelectedMenuItem => this._selectedMenuItem;

        protected int _menuDepth;

        protected int MenuDepth
        {
            get => this._menuDepth;
            set => SetProperty(ref this._menuDepth, value);
        }

        protected string _text = "";

        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value);
        }

        protected AsyncTexture2D _icon;

        public AsyncTexture2D Icon
        {
            get => this._icon;
            set => SetProperty(ref this._icon, value);
        }

        protected bool _canCheck;

        public bool CanCheck
        {
            get => this._canCheck;
            set => SetProperty(ref this._canCheck, value);
        }

        protected bool _collapsed = true;

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

        protected bool _checked;

        public bool Checked
        {
            get => this._checked;
            set
            {
                if (SetProperty(ref this._checked, value))
                    OnCheckedChanged(new CheckChangedEvent(this._checked));
            }
        }

        #endregion

        #region "Internal" Properties

        protected bool _overSection;

        [JsonIgnore]
        private bool OverSection
        {
            get => this._overSection;
            set
            {
                if (this._overSection == value) return;

                this._overSection = value;
                OnPropertyChanged();
            }
        }

        // Must remain internal for Glide to be able to access the property
        [JsonIgnore] public float ArrowRotation { get; set; } = -MathHelper.PiOver2;

        [JsonIgnore] private bool MouseOverIconBox { get; set; }

        private int LeftSidePadding
        {
            get
            {
                var leftSideBuilder = ICON_PADDING;

                // Add space if we need to render dropdown arrow
                if (this._children.Any())
                    leftSideBuilder += ARROW_SIZE;

                return leftSideBuilder;
            }
        }

        private Rectangle FirstItemBoxRegion =>
            new Rectangle(0,
                this.MenuItemHeight / 2 - ICON_SIZE / 2,
                ICON_SIZE,
                ICON_SIZE);

        #endregion

        #region Menu Item Selection

        public void Select()
        {
            if (this.Selected) return;

            if (this._children.Any())
                throw new InvalidOperationException("MenuItems with sub-MenuItems can not be selected directly.");

            this._scrollEffect.ForceActive = true;

            ((IMenuItem) this).Select(this);
            OnPropertyChanged(nameof(this.Selected));
        }

        void IMenuItem.Select(MenuItem menuItem)
        {
            ((IMenuItem) this).Select(menuItem, new List<IMenuItem> {this});
        }

        void IMenuItem.Select(MenuItem menuItem, List<IMenuItem> itemPath)
        {
            itemPath.Add(this);

            OnItemSelected(new ControlActivatedEventArgs(menuItem));

            // Expand to show the selected MenuItem, if necessary
            if (this._children.Any())
            {
                Expand();
            }

            var parentMenuItem = this.Parent as IMenuItem;
            parentMenuItem?.Select(menuItem, itemPath);
        }

        public void Deselect()
        {
            var isSelected = this.Selected;
            this._selectedMenuItem = null;
            this._scrollEffect.ForceActive = false;

            if (isSelected)
            {
                OnPropertyChanged(nameof(this.Selected));
            }
        }

        #endregion
    }
}
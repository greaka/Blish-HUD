using System;
using System.Collections.Generic;
using System.Linq;
using Adhesive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class Menu : Container, IMenuItem
    {
        private const int DEFAULT_ITEM_HEIGHT = 32;

        private bool _canSelect;

        protected int _menuItemHeight = DEFAULT_ITEM_HEIGHT;

        protected bool _shouldShift;

        public bool CanSelect
        {
            get => this._canSelect;
            set => SetProperty(ref this._canSelect, value);
        }

        public int MenuItemHeight
        {
            get => this._menuItemHeight;
            set
            {
                if (!SetProperty(ref this._menuItemHeight, value)) return;

                foreach (var childMenuItem in this._children.Cast<IMenuItem>())
                {
                    childMenuItem.MenuItemHeight = value;
                }
            }
        }

        public bool ShouldShift
        {
            get => this._shouldShift;
            set => SetProperty(ref this._shouldShift, value, true);
        }

        bool IMenuItem.Selected => false;
        public MenuItem SelectedMenuItem { get; private set; }

        void IMenuItem.Select()
        {
            throw new InvalidOperationException($"The root {nameof(this.Menu)} instance can not be selected.");
        }

        public void Select(MenuItem menuItem, List<IMenuItem> itemPath)
        {
            if (!this._canSelect)
            {
                itemPath.ForEach(i => i.Deselect());
                return;
            }

            foreach (var item in GetDescendants().Cast<IMenuItem>().Except(itemPath))
            {
                item.Deselect();
            }

            this.SelectedMenuItem = menuItem;

            OnItemSelected(new ControlActivatedEventArgs(menuItem));
        }

        public void Select(MenuItem menuItem)
        {
            menuItem.Select();
        }

        void IMenuItem.Deselect()
        {
            Select(null, null);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            foreach (var childMenuItem in this._children)
            {
                childMenuItem.Width = e.CurrentSize.X;
            }

            base.OnResized(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (!(e.ChangedChild is IMenuItem newChild))
            {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;

            e.ChangedChild.Width = this.Width;

            // We'll bind the top of the control to the bottom of the last control we added
            var lastItem = this._children.LastOrDefault();
            if (lastItem != null)
            {
                Binding.CreateOneWayBinding(() => e.ChangedChild.Top,
                    () => lastItem.Bottom, applyLeft: true);
            }

            this.ShouldShift = e.ResultingChildren.Any(mi =>
            {
                var cmi = (MenuItem) mi;

                return cmi.CanCheck || (cmi.Icon != null) || cmi.Children.Any();
            });

            base.OnChildAdded(e);
        }

        public MenuItem AddMenuItem(string text, Texture2D icon = null)
        {
            return new MenuItem(text)
            {
                Icon = icon,
                Parent = this
            };
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            var totalItemHeight = 0;

            for (var i = 0; i < this._children.Count; i++)
            {
                totalItemHeight = Math.Max(this._children[i].Bottom, totalItemHeight);
            }

            this.Height = totalItemHeight;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw items dark every other one
            for (var sec = 0; sec < this._size.Y / this.MenuItemHeight; sec += 2)
            {
                spriteBatch.DrawOnCtrl(this,
                    _textureMenuItemFade,
                    new Rectangle(0, this.MenuItemHeight * sec - this.VerticalScrollOffset, this._size.X,
                        this.MenuItemHeight),
                    Color.Black * 0.7f);
            }
        }

        #region Load Static

        private static readonly Texture2D _textureMenuItemFade;

        static Menu()
        {
            _textureMenuItemFade = Content.GetTexture("156044");
        }

        #endregion

        #region Events

        public event EventHandler<ControlActivatedEventArgs> ItemSelected;

        protected virtual void OnItemSelected(ControlActivatedEventArgs e)
        {
            ItemSelected?.Invoke(this, e);
        }

        #endregion
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls
{
    /// <summary>
    ///     Represents a Guild Wars 2 Dropdown control.
    /// </summary>
    public class Dropdown : Control
    {
        public static readonly DesignStandard Standard = new DesignStandard( /*          Size */ new Point(250, 27),
            /*   PanelOffset */ new Point(5, 2),
            /* ControlOffset */ ControlStandard.ControlOffset);

        private bool _hadPanel;

        private DropdownPanel _lastPanel;

        private string _selectedItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Dropdown" /> class.
        /// </summary>
        public Dropdown()
        {
            this.Items = new ObservableCollection<string>();

            this.Items.CollectionChanged += delegate
            {
                ItemsUpdated();
                Invalidate();
            };

            this.Size = Standard.Size;
        }

        /// <summary>
        ///     The collection of items contained in this <see cref="Dropdown" />.
        /// </summary>
        public ObservableCollection<string> Items { get; }

        /// <summary>
        ///     Gets or sets the currently selected item in the <see cref="Dropdown" />.
        /// </summary>
        public string SelectedItem
        {
            get => this._selectedItem;
            set
            {
                var previousValue = this._selectedItem;

                if (SetProperty(ref this._selectedItem, value))
                {
                    OnValueChanged(new ValueChangedEventArgs(previousValue, this._selectedItem));
                }
            }
        }

        /// <summary>
        ///     If the Dropdown box items are currently being shown, they are hidden.
        /// </summary>
        public void HideDropdownPanel()
        {
            this._hadPanel = this._mouseOver;
            this._lastPanel?.Dispose();
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if ((this._lastPanel == null) && !this._hadPanel)
            {
                this._lastPanel = DropdownPanel.ShowPanel(this);
            }
            else if (this._hadPanel)
            {
                this._hadPanel = false;
            }
        }

        private void ItemsUpdated()
        {
            if (string.IsNullOrEmpty(this.SelectedItem))
            {
                this.SelectedItem = this.Items.FirstOrDefault();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw dropdown
            spriteBatch.DrawOnCtrl(this,
                _textureInputBox,
                new Rectangle(Point.Zero, this._size).Subtract(new Rectangle(0, 0, 5, 0)),
                new Rectangle(0, 0,
                    Math.Min(_textureInputBox.Width - 5, this.Width - 5),
                    _textureInputBox.Height));

            // Draw right side of dropdown
            spriteBatch.DrawOnCtrl(this,
                _textureInputBox,
                new Rectangle(this._size.X - 5, 0, 5, this._size.Y),
                new Rectangle(_textureInputBox.Width - 5, 0,
                    5, _textureInputBox.Height));

            // Draw dropdown arrow
            spriteBatch.DrawOnCtrl(this,
                this.MouseOver ? _textureArrowActive : _textureArrow,
                new Rectangle(this._size.X - _textureArrow.Width - 5, this._size.Y / 2 - _textureArrow.Height / 2,
                    _textureArrow.Width,
                    _textureArrow.Height));

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, this._selectedItem,
                Content.DefaultFont14,
                new Rectangle(5, 0, this._size.X - 10 - _textureArrow.Width, this._size.Y),
                Color.FromNonPremultiplied(239, 240, 239, 255));
        }

        private class DropdownPanel : Control
        {
            private Dropdown _assocDropdown;

            private int _highlightedItem = -1;

            private DropdownPanel(Dropdown assocDropdown)
            {
                this._assocDropdown = assocDropdown;

                this._size = new Point(this._assocDropdown.Width,
                    this._assocDropdown.Height * this._assocDropdown.Items.Count);
                this._location = GetPanelLocation();
                this._zIndex = int.MaxValue;

                this.Parent = Graphics.SpriteScreen;

                Input.LeftMouseButtonPressed += Input_MousedOffDropdownPanel;
                Input.RightMouseButtonPressed += Input_MousedOffDropdownPanel;
            }

            private int HighlightedItem
            {
                get => this._highlightedItem;
                set => SetProperty(ref this._highlightedItem, value);
            }

            private Point GetPanelLocation()
            {
                var dropdownLocation = this._assocDropdown.AbsoluteBounds.Location;

                if (dropdownLocation.Y + this._assocDropdown.Height + this._size.Y < Graphics.SpriteScreen.Bottom)
                {
                    return dropdownLocation + new Point(0, this._assocDropdown.Height - 1);
                }

                return dropdownLocation - new Point(0, this._size.Y + 1);
            }

            public static DropdownPanel ShowPanel(Dropdown assocDropdown)
            {
                return new DropdownPanel(assocDropdown);
            }

            private void Input_MousedOffDropdownPanel(object sender, MouseEventArgs e)
            {
                if (!this.MouseOver)
                {
                    this._assocDropdown.HideDropdownPanel();
                }
            }

            protected override void OnMouseMoved(MouseEventArgs e)
            {
                this.HighlightedItem = this.RelativeMousePosition.Y / this._assocDropdown.Height;

                base.OnMouseMoved(e);
            }

            protected override void OnClick(MouseEventArgs e)
            {
                this._assocDropdown.SelectedItem = this._assocDropdown.Items[this.HighlightedItem];

                base.OnClick(e);

                Dispose();
            }

            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(Point.Zero, this._size),
                    Color.Black);

                var index = 0;
                foreach (var item in this._assocDropdown.Items)
                {
                    if (index == this.HighlightedItem)
                    {
                        spriteBatch.DrawOnCtrl(this,
                            ContentService.Textures.Pixel,
                            new Rectangle(2,
                                2 + this._assocDropdown.Height * index, this._size.X - 12 - _textureArrow.Width,
                                this._assocDropdown.Height - 4),
                            new Color(45, 37, 25, 255));

                        spriteBatch.DrawStringOnCtrl(this,
                            item,
                            Content.DefaultFont14,
                            new Rectangle(8, this._assocDropdown.Height * index,
                                bounds.Width - 13 - _textureArrow.Width, this._assocDropdown.Height),
                            ContentService.Colors.Chardonnay);
                    }
                    else
                    {
                        spriteBatch.DrawStringOnCtrl(this,
                            item,
                            Content.DefaultFont14,
                            new Rectangle(8, this._assocDropdown.Height * index,
                                bounds.Width - 13 - _textureArrow.Width, this._assocDropdown.Height),
                            Color.FromNonPremultiplied(239, 240, 239, 255));
                    }

                    index++;
                }
            }

            protected override void DisposeControl()
            {
                if (this._assocDropdown != null)
                {
                    this._assocDropdown._lastPanel = null;
                    this._assocDropdown = null;
                }

                Input.LeftMouseButtonPressed -= Input_MousedOffDropdownPanel;
                Input.RightMouseButtonPressed -= Input_MousedOffDropdownPanel;

                base.DisposeControl();
            }
        }

        #region Load Static

        private static readonly Texture2D _textureInputBox;

        private static readonly TextureRegion2D _textureArrow;
        private static readonly TextureRegion2D _textureArrowActive;

        static Dropdown()
        {
            _textureInputBox = Content.GetTexture("input-box");

            _textureArrow = Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow");
            _textureArrowActive = Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow-active");
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="SelectedItem" /> property has changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        #endregion
    }
}
using System;
using System.Linq;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Controls.Resources;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class ContextMenuStripItem : Control, ICheckable
    {
        private const int BULLET_SIZE = 18;
        private const int HORIZONTAL_PADDING = 6;

        private const int TEXT_LEFTPADDING = HORIZONTAL_PADDING + BULLET_SIZE + HORIZONTAL_PADDING;

        private bool _canCheck;

        private bool _checked;

        private ContextMenuStrip _submenu;

        private string _text;

        public ContextMenuStripItem()
        {
            this.EffectBehind = new ScrollingHighlightEffect(this);
        }

        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value, true);
        }

        public ContextMenuStrip Submenu
        {
            get => this._submenu;
            set => SetProperty(ref this._submenu, value, true);
        }

        public bool CanCheck
        {
            get => this._canCheck;
            set => SetProperty(ref this._canCheck, value);
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        public bool Checked
        {
            get => this._checked;
            set
            {
                if (SetProperty(ref this._checked, value))
                {
                    OnCheckedChanged(new CheckChangedEvent(this._checked));
                }
            }
        }

        protected virtual void OnCheckedChanged(CheckChangedEvent e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        public override void RecalculateLayout()
        {
            var textSize = GameService.Content.DefaultFont14.MeasureString(this._text);
            var nWidth = (int) textSize.Width + TEXT_LEFTPADDING + TEXT_LEFTPADDING;

            if (this.Parent != null)
            {
                this.Width = Math.Max(this.Parent.Width - 4, nWidth);
            }
            else
            {
                this.Width = nWidth;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (this.CanCheck)
                this.Checked = !this.Checked;
            else
                this.Parent.Hide();

            base.OnClick(e);
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            this.Submenu?.Show(this);

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            if (this.RelativeMousePosition.X < this.Left)
                this.Submenu?.Hide();

            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._canCheck)
            {
                var state = this._checked ? "-checked" : "-unchecked";

                var extension = "";
                extension = this.MouseOver ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;

                spriteBatch.DrawOnCtrl(this,
                    Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}"),
                    new Rectangle(
                        HORIZONTAL_PADDING + BULLET_SIZE / 2 - 16, this._size.Y / 2 - 16,
                        32,
                        32
                    ),
                    StandardColors.Default);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this,
                    _textureBullet,
                    new Rectangle(
                        HORIZONTAL_PADDING, this._size.Y / 2 - BULLET_SIZE / 2,
                        BULLET_SIZE,
                        BULLET_SIZE
                    ),
                    this.MouseOver ? StandardColors.Tinted : StandardColors.Default);
            }

            // Draw shadow
            spriteBatch.DrawStringOnCtrl(this, this._text,
                Content.DefaultFont14,
                new Rectangle(TEXT_LEFTPADDING + 1,
                    0 + 1, this._size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING, this._size.Y),
                StandardColors.Shadow);

            spriteBatch.DrawStringOnCtrl(this, this._text,
                Content.DefaultFont14,
                new Rectangle(TEXT_LEFTPADDING,
                    0, this._size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING, this._size.Y),
                this._enabled ? StandardColors.Default : StandardColors.DisabledText);

            // Indicate submenu, if there is one
            if (this._submenu != null)
            {
                spriteBatch.DrawOnCtrl(this,
                    _textureArrow,
                    new Rectangle(this._size.X - HORIZONTAL_PADDING - _textureArrow.Width,
                        this._size.Y / 2 - _textureArrow.Height / 2,
                        _textureArrow.Width,
                        _textureArrow.Height),
                    this.MouseOver ? StandardColors.Tinted : StandardColors.Default);
            }
        }

        #region Load Static

        private static readonly Texture2D _textureBullet;
        private static readonly Texture2D _textureArrow;

        static ContextMenuStripItem()
        {
            _textureBullet = Content.GetTexture("155038");
            _textureArrow = Content.GetTexture("context-menu-strip-submenu");
        }

        #endregion
    }
}
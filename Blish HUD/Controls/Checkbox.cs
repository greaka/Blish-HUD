using System;
using System.Linq;
using Blish_HUD.Controls.Resources;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    // TODO: Checkbox needs to shrink on mousedown (animation)
    public class Checkbox : LabelBase, ICheckable
    {
        private const int CHECKBOX_SIZE = 32;

        private bool _checked;

        public Checkbox()
        {
            this._size = new Point(64, CHECKBOX_SIZE / 2);

            this._autoSizeWidth = true;
            this._textColor = Color.White;
            this._verticalAlignment = VerticalAlignment.Middle;
        }

        /// <summary>
        ///     The text this <see cref="Checkbox" /> should show.
        /// </summary>
        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value, true);
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
            base.RecalculateLayout();

            this._size = new Point(CHECKBOX_SIZE / 3 * 2 + this.LabelRegion.X, this._size.Y);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (this.Enabled)
                this.Checked = !this.Checked;

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            if (this.Enabled)
                Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnLeftMouseButtonReleased(e);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var state = "-unchecked";
            state = this.Checked ? "-checked" : state;

            var extension = "";
            extension = this.MouseOver ? "-active" : extension;
            extension = !this.Enabled ? "-disabled" : extension;

            var sprite = Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}");

            spriteBatch.DrawOnCtrl(this,
                sprite,
                new Rectangle(-9,
                    this.Height / 2 - CHECKBOX_SIZE / 2,
                    CHECKBOX_SIZE,
                    CHECKBOX_SIZE));

            DrawText(spriteBatch, new Rectangle(CHECKBOX_SIZE / 3 * 2, 0, this.LabelRegion.X, this.LabelRegion.Y));
        }
    }
}
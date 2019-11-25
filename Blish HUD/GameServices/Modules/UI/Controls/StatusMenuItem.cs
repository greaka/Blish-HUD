using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.UI.Controls
{
    public class StatusMenuItem : MenuItem
    {
        private string _statusText;
        private Color _statusTextColor = Color.White;

        public string StatusText
        {
            get => this._statusText;
            set => SetProperty(ref this._statusText, value);
        }

        public Color StatusTextColor
        {
            get => this._statusTextColor;
            set => SetProperty(ref this._statusTextColor, value);
        }

        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, this._statusText, Content.DefaultFont12,
                new Rectangle(bounds.X, bounds.Y, bounds.Width - 20, bounds.Height), this._statusTextColor, false, true,
                1, HorizontalAlignment.Right);
        }
    }
}
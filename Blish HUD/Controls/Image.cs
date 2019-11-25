using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class Image : Control
    {
        private Rectangle? _sourceRectangle;

        private SpriteEffects _spriteEffects;

        protected AsyncTexture2D _texture;

        private Color _tint = Color.White;

        public Image()
        {
            /* NOOP */
        }

        public Image(AsyncTexture2D texture)
        {
            this.Texture = texture;
            this.Size = texture.Texture.Bounds.Size;
        }

        public AsyncTexture2D Texture
        {
            get => this._texture;
            set => SetProperty(ref this._texture, value);
        }

        public SpriteEffects SpriteEffects
        {
            get => this._spriteEffects;
            set => SetProperty(ref this._spriteEffects, value);
        }

        public Rectangle SourceRectangle
        {
            get => this._sourceRectangle ?? this._texture.Texture.Bounds;
            set => SetProperty(ref this._sourceRectangle, value);
        }

        public Color Tint
        {
            get => this._tint;
            set => SetProperty(ref this._tint, value);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._texture == null) return;

            // Draw the texture
            spriteBatch.DrawOnCtrl(this, this._texture,
                bounds,
                this.SourceRectangle, this._tint,
                0f,
                Vector2.Zero, this._spriteEffects);
        }
    }
}
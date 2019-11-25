using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Content
{
    /// <summary>
    ///     A thread-safe wrapper on Texture2D that allows you to swap the <see cref="Texture2D" /> backing for a texture
    ///     already assigned to a property.
    /// </summary>
    public sealed class AsyncTexture2D : IDisposable
    {
        private Texture2D _activeTexture2D;

        private Texture2D _stagedTexture2D;

        /// <summary>
        ///     Create an <see cref="AsyncTexture2D" /> where the current <see cref="Texture" /> is a single transparent pixel.
        /// </summary>
        public AsyncTexture2D()
        {
            this._activeTexture2D = ContentService.Textures.TransparentPixel;
        }

        /// <summary>
        ///     Create an <see cref="AsyncTexture2D" /> where the current <see cref="Texture" /> is the <see cref="Texture2D" />
        ///     passed as
        ///     <param name="defaultTexture" />
        ///     .
        /// </summary>
        public AsyncTexture2D(Texture2D defaultTexture)
        {
            this._activeTexture2D = defaultTexture;
        }

        /// <summary>
        ///     <c>true</c>, if the <see cref="AsyncTexture2D" />'s <see cref="Texture" /> is set.
        /// </summary>
        public bool HasTexture => this._activeTexture2D != null;

        /// <summary>
        ///     The active <see cref="Texture2D" /> of the <see cref="AsyncTexture2D" />.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                if (!this.HasTexture)
                {
                    throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");
                }

                return this._activeTexture2D;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this._stagedTexture2D?.Dispose();
            this._activeTexture2D?.Dispose();
        }

        /// <summary>
        ///     Replaces the <see cref="Texture" /> of the <see cref="AsyncTexture2D" /> with the texture provided in
        ///     <param name="newTexture" />
        ///     on the next game cycle loop.
        /// </summary>
        /// <param name="newTexture">The new texture to assign.</param>
        public void SwapTexture(Texture2D newTexture)
        {
            this._stagedTexture2D = newTexture;

            GameService.Overlay.QueueMainThreadUpdate(ApplyTextureSwap);
        }

        private void ApplyTextureSwap(GameTime gameTime)
        {
            this._activeTexture2D = this._stagedTexture2D;
            this._stagedTexture2D = null;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!this.HasTexture) return obj == null;
            if (obj == null) return false;

            if (obj is Texture2D tobj)
            {
                return this._activeTexture2D.Equals(tobj);
            }

            return this == obj;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this._activeTexture2D?.GetHashCode() ?? 0;
        }

        public static implicit operator Texture2D(AsyncTexture2D asyncTexture2D)
        {
            return asyncTexture2D._activeTexture2D;
        }

        public static implicit operator AsyncTexture2D(Texture2D texture2D)
        {
            if (texture2D == null) return null;

            return new AsyncTexture2D(texture2D);
        }
    }
}
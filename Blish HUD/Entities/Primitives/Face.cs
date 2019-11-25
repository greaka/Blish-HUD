using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives
{
    public class Face : Entity
    {
        private Vector2 _size = Vector2.One;

        protected Texture2D _texture;

        private VertexPositionTexture[] _verts;

        public Face() :
            this(null, Vector3.Zero, Vector2.One)
        {
        }

        public Face(Texture2D image) :
            this(image, Vector3.Zero)
        {
        }

        public Face(Texture2D image, Vector3 position) :
            this(image, position, new Vector2(image.Width / 100f, image.Height / 100f))
        {
        }

        public Face(Texture2D image, Vector3 position, Vector2 size)
        {
            Initialize();

            //this.Texture = image;
            this.Size = size;
            this.Position = position;
        }

        public Vector2 Size
        {
            get => this._size;
            set
            {
                this._size = value;
                UpdateSize();
            }
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            // NOOP
        }

        private void UpdateSize()
        {
            var threeDSize = new Vector3(this.Size.X, this.Size.Y, 0);

            this._verts[0] = new VertexPositionTexture(Vector3.UnitX * threeDSize, Vector2.One);
            this._verts[1] = new VertexPositionTexture(Vector3.Zero, Vector2.UnitY);
            this._verts[2] = new VertexPositionTexture(new Vector3(1f, 1f, 0) * threeDSize, Vector2.UnitX);
            this._verts[3] = new VertexPositionTexture(Vector3.UnitY * threeDSize, Vector2.Zero);
        }

        private void Initialize()
        {
            this._verts = new VertexPositionTexture[4];
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
        }
    }
}
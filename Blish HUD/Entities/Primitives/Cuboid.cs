using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives
{
    public abstract class Cuboid : Entity
    {
        protected readonly BasicEffect _renderEffect;

        protected readonly VertexPositionTexture[] _verts;

        protected VertexBuffer _geometryBuffer;
        protected IndexBuffer _indexBuffer;

        protected Vector3 _size;

        public Cuboid() : this(new Vector3(1f))
        {
            /* NOOP */
        }

        public Cuboid(float size) : this(new Vector3(size))
        {
            /* NOOP */
        }

        public Cuboid(Vector3 size)
        {
            this._verts = new VertexPositionTexture[24];

            this._renderEffect = (BasicEffect) StandardEffect.Clone();
            this._renderEffect.TextureEnabled = true;
            this._renderEffect.VertexColorEnabled = false;

            this.Texture = ContentService.Textures.Error;

            this._size = size;
        }

        public Vector3 Size
        {
            get => this._size;
            set => SetProperty(ref this._size, value, true);
        }

        public Texture2D Texture
        {
            get => this._renderEffect.Texture;
            set => this._renderEffect.Texture = value;
        }

        /// <inheritdoc />
        public override float Opacity
        {
            get => this._renderEffect.Alpha;
            set => this._renderEffect.Alpha = value;
        }

        private void GenerateCuboid(GraphicsDevice device, Vector3 size)
        {
            this._verts[0].Position = new Vector3(-1, 1, -1) * size;
            this._verts[0].TextureCoordinate = new Vector2(0, 0);
            this._verts[1].Position = new Vector3(1, 1, -1) * size;
            this._verts[1].TextureCoordinate = new Vector2(1, 0);
            this._verts[2].Position = new Vector3(-1, 1, 1) * size;
            this._verts[2].TextureCoordinate = new Vector2(0, 1);
            this._verts[3].Position = new Vector3(1, 1, 1) * size;
            this._verts[3].TextureCoordinate = new Vector2(1, 1);

            this._verts[4].Position = new Vector3(-1, -1, 1) * size;
            this._verts[4].TextureCoordinate = new Vector2(0, 0);
            this._verts[5].Position = new Vector3(1, -1, 1) * size;
            this._verts[5].TextureCoordinate = new Vector2(1, 0);
            this._verts[6].Position = new Vector3(-1, -1, -1) * size;
            this._verts[6].TextureCoordinate = new Vector2(0, 1);
            this._verts[7].Position = new Vector3(1, -1, -1) * size;
            this._verts[7].TextureCoordinate = new Vector2(1, 1);

            this._verts[8].Position = new Vector3(-1, 1, -1) * size;
            this._verts[8].TextureCoordinate = new Vector2(0, 0);
            this._verts[9].Position = new Vector3(-1, 1, 1) * size;
            this._verts[9].TextureCoordinate = new Vector2(1, 0);
            this._verts[10].Position = new Vector3(-1, -1, -1) * size;
            this._verts[10].TextureCoordinate = new Vector2(0, 1);
            this._verts[11].Position = new Vector3(-1, -1, 1) * size;
            this._verts[11].TextureCoordinate = new Vector2(1, 1);

            this._verts[12].Position = new Vector3(-1, 1, 1) * size;
            this._verts[12].TextureCoordinate = new Vector2(0, 0);
            this._verts[13].Position = new Vector3(1, 1, 1) * size;
            this._verts[13].TextureCoordinate = new Vector2(1, 0);
            this._verts[14].Position = new Vector3(-1, -1, 1) * size;
            this._verts[14].TextureCoordinate = new Vector2(0, 1);
            this._verts[15].Position = new Vector3(1, -1, 1) * size;
            this._verts[15].TextureCoordinate = new Vector2(1, 1);

            this._verts[16].Position = new Vector3(1, 1, 1) * size;
            this._verts[16].TextureCoordinate = new Vector2(0, 0);
            this._verts[17].Position = new Vector3(1, 1, -1) * size;
            this._verts[17].TextureCoordinate = new Vector2(1, 0);
            this._verts[18].Position = new Vector3(1, -1, 1) * size;
            this._verts[18].TextureCoordinate = new Vector2(0, 1);
            this._verts[19].Position = new Vector3(1, -1, -1) * size;
            this._verts[19].TextureCoordinate = new Vector2(1, 1);

            this._verts[20].Position = new Vector3(1, 1, -1) * size;
            this._verts[20].TextureCoordinate = new Vector2(0, 0);
            this._verts[21].Position = new Vector3(-1, 1, -1) * size;
            this._verts[21].TextureCoordinate = new Vector2(1, 0);
            this._verts[22].Position = new Vector3(1, -1, -1) * size;
            this._verts[22].TextureCoordinate = new Vector2(0, 1);
            this._verts[23].Position = new Vector3(-1, -1, -1) * size;
            this._verts[23].TextureCoordinate = new Vector2(1, 1);

            this._geometryBuffer?.Dispose();
            this._geometryBuffer = new VertexBuffer(GameService.Graphics.GraphicsDevice,
                VertexPositionTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
            this._geometryBuffer.SetData(this._verts);

            //var indices = new int[36];
            //indices[0] = 0; indices[1] = 1; indices[2] = 2;
            //indices[3] = 1; indices[4] = 3; indices[5] = 2;

            //indices[6] = 4; indices[7]  = 5; indices[8]  = 6;
            //indices[9] = 5; indices[10] = 7; indices[11] = 6;

            //indices[12] = 8; indices[13] = 9; indices[14]  = 10;
            //indices[15] = 9; indices[16] = 11; indices[17] = 10;

            //indices[18] = 12; indices[19] = 13; indices[20] = 14;
            //indices[21] = 13; indices[22] = 15; indices[23] = 14;

            //indices[24] = 16; indices[25] = 17; indices[26] = 18;
            //indices[27] = 17; indices[28] = 19; indices[29] = 18;

            //indices[30] = 20; indices[31] = 21; indices[32] = 22;
            //indices[33] = 21; indices[34] = 23; indices[35] = 22;

            //_indexBuffer = new IndexBuffer(device, typeof(int), 36, BufferUsage.WriteOnly);
            //_indexBuffer.SetData(indices);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            GenerateCuboid(graphicsDevice, this._size);
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            if (this._geometryBuffer == null) return;

            this._renderEffect.View = GameService.Camera.View;
            this._renderEffect.Projection = GameService.Camera.Projection;
            this._renderEffect.World = Matrix.CreateTranslation(this._position);

            graphicsDevice.SetVertexBuffer(this._geometryBuffer, 0);

            foreach (var pass in this._renderEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 24);
        }
    }
}
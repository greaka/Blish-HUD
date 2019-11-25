using Blish_HUD.Content;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives
{
    public class Billboard : Entity
    {
        private static BasicEffect _billboardEffect;

        private bool _autoResizeBillboard = true;
        private float _scale = 1f;
        private Vector2 _size = Vector2.One;
        private AsyncTexture2D _texture;
        private BillboardVerticalConstraint _verticalConstraint = BillboardVerticalConstraint.CameraPosition;

        private VertexPositionTexture[] _verts;

        public Billboard() :
            this(null, Vector3.Zero, Vector2.Zero)
        {
        }

        public Billboard(Texture2D image) :
            this(image, Vector3.Zero)
        {
        }

        public Billboard(Texture2D image, Vector3 position) :
            this(image, position, Vector2.Zero)
        {
        }

        public Billboard(Texture2D image, Vector3 position, Vector2 size)
        {
            Initialize();

            this.AutoResizeBillboard = size == Vector2.Zero;
            this.Size = size;
            this.Texture = image;
            this.Position = position;
        }

        /// <summary>
        ///     If set to true, the <see cref="Size" /> will automatically
        ///     update if a new <see cref="Texture" /> is set.
        /// </summary>
        public bool AutoResizeBillboard
        {
            get => this._autoResizeBillboard;
            set => SetProperty(ref this._autoResizeBillboard, value);
        }

        public BillboardVerticalConstraint VerticalConstraint
        {
            get => this._verticalConstraint;
            set => SetProperty(ref this._verticalConstraint, value);
        }

        public Vector2 Size
        {
            get => this._size;
            set
            {
                if (SetProperty(ref this._size, value))
                    RecalculateSize(this._size, this._scale);
            }
        }

        /// <summary>
        ///     Scales the render size of the <see cref="Billboard" />.
        /// </summary>
        public float Scale
        {
            get => this._scale;
            set
            {
                if (SetProperty(ref this._scale, value))
                    RecalculateSize(this._size, this._scale);
            }
        }

        public AsyncTexture2D Texture
        {
            get => this._texture;
            set
            {
                if (SetProperty(ref this._texture, value) && this._autoResizeBillboard && this._texture.HasTexture)
                    this.Size = this._texture.Texture.Bounds.Size.ToVector2().ToWorldCoord();
            }
        }

        private void Initialize()
        {
            this._verts = new VertexPositionTexture[4];

            _billboardEffect = _billboardEffect ?? new BasicEffect(GameService.Graphics.GraphicsDevice);
            _billboardEffect.TextureEnabled = true;
        }

        private void RecalculateSize(Vector2 newSize, float scale)
        {
            this._verts[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1));
            this._verts[1] = new VertexPositionTexture(new Vector3(newSize.X * scale, 0, 0), new Vector2(0, 1));
            this._verts[2] = new VertexPositionTexture(new Vector3(0, newSize.Y * scale, 0), new Vector2(1, 0));
            this._verts[3] =
                new VertexPositionTexture(new Vector3(newSize.X * scale, newSize.Y * scale, 0), new Vector2(0, 0));
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            if (this.Texture == null) return;

            _billboardEffect.View = GameService.Camera.View;
            _billboardEffect.Projection = GameService.Camera.Projection;
            _billboardEffect.World = Matrix.CreateTranslation(new Vector3(this.Size.X / -2, this.Size.Y / -2, 0))
                                     * Matrix.CreateScale(this._scale, this._scale, 1)
                                     * Matrix.CreateBillboard(this.Position + this.RenderOffset,
                                         new Vector3(GameService.Camera.Position.X,
                                             GameService.Camera.Position.Y,
                                             this._verticalConstraint == BillboardVerticalConstraint.CameraPosition
                                                 ? GameService.Camera.Position.Z
                                                 : GameService.Player.Position.Z),
                                         new Vector3(0, 0, 1),
                                         GameService.Camera.Forward);

            _billboardEffect.Alpha = this.Opacity;
            _billboardEffect.Texture = this.Texture.Texture;

            foreach (var pass in _billboardEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, this._verts, 0, 2);
            }
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            RecalculateSize(this._size, this._scale);
        }

        public override void Update(GameTime gameTime)
        {
            // NOOP
        }
    }
}
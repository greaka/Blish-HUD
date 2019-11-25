using System;
using Blish_HUD.Content;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Primitives;
using Blish_HUD.Input;
using Blish_HUD.Pathing.Entities.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities
{
    public enum BillboardVerticalConstraint
    {
        CameraPosition,
        PlayerPosition
    }

    public class Marker : Entity
    {
        private static readonly Logger Logger = Logger.GetLogger<Marker>();

        private bool _autoResize = true;
        private float _fadeFar = -1;
        private float _fadeNear = -1;

        private Effect _markerEffect;

        private bool _mouseOver;
        private float _scale = 1f;
        private Vector2 _size = Vector2.One;
        private AsyncTexture2D _texture;

        private DynamicVertexBuffer _vertexBuffer;
        private BillboardVerticalConstraint _verticalConstraint = BillboardVerticalConstraint.CameraPosition;

        private VertexPositionTexture[] _verts;

        public Marker() : this(null, Vector3.Zero, Vector2.Zero)
        {
            /* NOOP */
        }

        public Marker(Texture2D texture) : this(texture, Vector3.Zero)
        {
            /* NOOP */
        }

        public Marker(Texture2D texture, Vector3 position) : this(texture, position, Vector2.Zero)
        {
            /* NOOP */
        }

        public Marker(Texture2D texture, Vector3 position, Vector2 size)
        {
            Initialize();

            this._autoResize = size == Vector2.Zero;
            this.Position = position;
            this.Size = size;
            this.Texture = texture;

            //GameService.Input.MouseMoved += Input_MouseMoved;
        }

        /// <summary>
        ///     If set to true, the <see cref="Size" /> will automatically
        ///     update if a new <see cref="Texture" /> is set.
        /// </summary>
        public bool AutoResize
        {
            get => this._autoResize;
            set => SetProperty(ref this._autoResize, value);
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

        public float FadeNear
        {
            get => Math.Min(this._fadeNear, this._fadeFar);
            set => SetProperty(ref this._fadeNear, value);
        }

        public float FadeFar
        {
            get => Math.Max(this._fadeNear, this._fadeFar);
            set => SetProperty(ref this._fadeFar, value);
        }

        public AsyncTexture2D Texture
        {
            get => this._texture;
            set
            {
                if (SetProperty(ref this._texture, value) && (this._texture != null))
                {
                    this.VerticalConstraint = this._texture.Texture.Height == this._texture.Texture.Width
                        ? BillboardVerticalConstraint.CameraPosition
                        : BillboardVerticalConstraint.PlayerPosition;

                    if (this._autoResize)
                    {
                        this.Size = new Vector2(WorldUtil.GameToWorldCoord(this._texture.Texture.Width),
                            WorldUtil.GameToWorldCoord(this._texture.Texture.Height));
                    }
                }
            }
        }

        private void Initialize()
        {
            this._verts = new VertexPositionTexture[4];
            this._vertexBuffer = new DynamicVertexBuffer(GameService.Graphics.GraphicsDevice,
                typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        }

        private void RecalculateSize(Vector2 newSize, float scale)
        {
            this._verts[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1));
            this._verts[1] = new VertexPositionTexture(new Vector3(newSize.X * scale, 0, 0), new Vector2(0, 1));
            this._verts[2] = new VertexPositionTexture(new Vector3(0, newSize.Y * scale, 0), new Vector2(1, 0));
            this._verts[3] =
                new VertexPositionTexture(new Vector3(newSize.X * scale, newSize.Y * scale, 0), new Vector2(0, 0));

            this._vertexBuffer.SetData(this._verts);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            RecalculateSize(this._size, this._scale);
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            if (this._texture == null) return;

            var modelMatrix = Matrix.CreateTranslation(this._size.X / -2, this._size.Y / -2, 0)
                              * Matrix.CreateScale(this._scale);

            if (this.Rotation == Vector3.Zero)
            {
                modelMatrix *= Matrix.CreateBillboard(this.Position + this.RenderOffset,
                    new Vector3(GameService.Camera.Position.X,
                        GameService.Camera.Position.Y,
                        this._verticalConstraint == BillboardVerticalConstraint.CameraPosition
                            ? GameService.Camera.Position.Z
                            : GameService.Player.Position.Z),
                    new Vector3(0, 0, 1),
                    GameService.Camera.Forward);
            }
            else
            {
                modelMatrix *= Matrix.CreateRotationX(this.Rotation.X)
                               * Matrix.CreateRotationY(this.Rotation.Y)
                               * Matrix.CreateRotationZ(this.Rotation.Z)
                               * Matrix.CreateTranslation(this.Position + this.RenderOffset);
            }

            _sharedMarkerEffect.SetEntityState(modelMatrix, this._texture, this._opacity, this._fadeNear,
                this._fadeFar);

            graphicsDevice.SetVertexBuffer(this._vertexBuffer);

            foreach (var pass in _sharedMarkerEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        private void Input_MouseMoved(object sender, MouseEventArgs e)
        {
            var screenPosition = GameService.Graphics.GraphicsDevice.Viewport.Project(this.Position,
                GameService.Camera.Projection, GameService.Camera.View, Matrix.Identity);

            var xdist = screenPosition.X - e.MouseState.Position.X;
            var ydist = screenPosition.Y - e.MouseState.Position.Y;

            // Z < 1 means that the point is in front of the camera, not behind it
            this._mouseOver = (screenPosition.Z < 1) && (xdist < 2) && (ydist < 2);
        }

        #region Load Static

        private static readonly MarkerEffect _sharedMarkerEffect;

        static Marker()
        {
            _sharedMarkerEffect = new MarkerEffect(BlishHud.ActiveContentManager.Load<Effect>(@"effects\marker"));
        }

        #endregion
    }
}
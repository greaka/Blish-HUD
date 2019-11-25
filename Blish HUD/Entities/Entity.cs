using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities
{
    public abstract class Entity : INotifyPropertyChanged, IUpdatable, IRenderable3D
    {
        private EntityText _basicTitleTextBillboard;

        private EntityBillboard _billboard;
        protected float _opacity = 1.0f;

        protected Vector3 _position = Vector3.Zero;
        protected Vector3 _renderOffset = Vector3.Zero;
        protected Vector3 _rotation = Vector3.Zero;
        protected bool _visible = true;

        protected static BasicEffect StandardEffect { get; } =
            new BasicEffect(BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice) {TextureEnabled = true};

        public virtual Vector3 Position
        {
            get => this._position;
            set => SetProperty(ref this._position, value);
        }

        public virtual Vector3 Rotation
        {
            get => this._rotation;
            set => SetProperty(ref this._rotation, value);
        }

        public float RotationX
        {
            get => this._rotation.X;
            set
            {
                if (SetProperty(ref this._rotation, new Vector3(value, this._rotation.Y, this._rotation.Z), false,
                    nameof(this.Rotation)))
                    OnPropertyChanged();
            }
        }

        public float RotationY
        {
            get => this._rotation.Y;
            set
            {
                if (SetProperty(ref this._rotation, new Vector3(this._rotation.X, value, this._rotation.Z), false,
                    nameof(this.Rotation)))
                    OnPropertyChanged();
            }
        }

        public float RotationZ
        {
            get => this._rotation.Z;
            set
            {
                if (SetProperty(ref this._rotation, new Vector3(this._rotation.X, this._rotation.Y, value), false,
                    nameof(this.Rotation)))
                    OnPropertyChanged();
            }
        }

        /// <summary>
        ///     If <c>true</c>, the <see cref="Entity" /> will rebuild its <see cref="VertexBuffer" /> during the next update
        ///     cycle.
        /// </summary>
        public bool PendingRebuild { get; private set; } = true;

        /// <summary>
        ///     The offset that this entity is rendered from its origin.
        /// </summary>
        public virtual Vector3 RenderOffset
        {
            get => this._renderOffset;
            set => SetProperty(ref this._renderOffset, value);
        }

        public virtual float VerticalOffset
        {
            get => this._renderOffset.Z;
            set
            {
                if (SetProperty(ref this._renderOffset, new Vector3(this._renderOffset.X, this._renderOffset.Y, value),
                    false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float HorizontalOffset
        {
            get => this._renderOffset.X;
            set
            {
                if (SetProperty(ref this._renderOffset, new Vector3(value, this._renderOffset.Y, this._renderOffset.Z),
                    false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float DepthOffset
        {
            get => this._renderOffset.Y;
            set
            {
                if (SetProperty(ref this._renderOffset, new Vector3(this._renderOffset.X, value, this._renderOffset.Y),
                    false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float Opacity
        {
            get => this._opacity;
            set => SetProperty(ref this._opacity, value);
        }

        public virtual bool Visible
        {
            get => this._visible;
            set => SetProperty(ref this._visible, value);
        }

        public EntityBillboard Billboard
        {
            get => this._billboard ?? this._basicTitleTextBillboard;
            set => SetProperty(ref this._billboard, value);
        }

        public string BasicTitleText
        {
            get => this._basicTitleTextBillboard?.Text ?? string.Empty;
            set
            {
                this._basicTitleTextBillboard = this._basicTitleTextBillboard ?? BuildTitleText();
                this._basicTitleTextBillboard.Text = value;
            }
        }

        public Color BasicTitleTextColor
        {
            get => this._basicTitleTextBillboard?.TextColor ?? Color.White;
            set
            {
                this._basicTitleTextBillboard = this._basicTitleTextBillboard ?? BuildTitleText();
                this._basicTitleTextBillboard.TextColor = value;
            }
        }

        public virtual float DistanceFromPlayer => Vector3.Distance(this.Position, GameService.Player.Position);
        public virtual float DistanceFromCamera => Vector3.Distance(this.Position, GameService.Camera.Position);

        public virtual void Draw(GraphicsDevice graphicsDevice)
        {
            /* NOOP */
        }

        public virtual void Update(GameTime gameTime)
        {
            /* NOOP */
        }

        private EntityText BuildTitleText()
        {
            var entityText = new EntityText(this)
            {
                VerticalOffset = 2f
            };

            return entityText;
        }

        public virtual void DoUpdate(GameTime gameTime)
        {
            if (this.PendingRebuild)
            {
                HandleRebuild(GameService.Graphics.GraphicsDevice);
                this.PendingRebuild = false;
            }

            Update(gameTime);

            this.Billboard?.DoUpdate(gameTime);
        }

        public virtual void DoDraw(GraphicsDevice graphicsDevice)
        {
            Draw(graphicsDevice);

            this.Billboard?.DoDraw(graphicsDevice);
        }

        public abstract void HandleRebuild(GraphicsDevice graphicsDevice);

        #region Property Management and Binding

        protected bool SetProperty<T>(ref T property, T newValue, bool rebuildEntity = false,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(property, newValue) || (propertyName == null)) return false;

            property = newValue;

            this.PendingRebuild = this.PendingRebuild || rebuildEntity;

            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
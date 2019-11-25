using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Effects
{
    public abstract class ControlEffect
    {
        protected bool _enabled = true;

        protected Vector2? _location;


        protected Vector2? _size;

        public ControlEffect(Control assignedControl)
        {
            this.AssignedControl = assignedControl;
        }

        protected Control AssignedControl { get; }

        /// <summary>
        ///     The size within the <see cref="Control" /> it applies to.  If not explicitly set, the size of the assigned control
        ///     will be used.
        /// </summary>
        public Vector2 Size
        {
            get => this._size ?? this.AssignedControl.Size.ToVector2();
            set => this._size = value;
        }

        /// <summary>
        ///     The location relative to the <see cref="Control" /> it applies to.  If not explicitly set,
        ///     <see cref="Vector2.Zero" /> will be used.
        /// </summary>
        public Vector2 Location
        {
            get => this._location ?? Vector2.Zero;
            set => this._location = value;
        }

        /// <summary>
        ///     If the <see cref="ControlEffect" /> should render or not.
        /// </summary>
        public bool Enabled
        {
            get => this._enabled;
            set
            {
                if (this._enabled == value) return;

                this._enabled = value;

                if (this._enabled)
                    OnEnable();
                else
                    OnDisable();
            }
        }

        public abstract SpriteBatchParameters GetSpriteBatchParameters();

        protected virtual void OnEnable()
        {
            /* NOOP */
        }

        protected virtual void OnDisable()
        {
            /* NOOP */
        }

        /// <summary>
        ///     Enables the <see cref="Effect" /> on the <see cref="Control" />.
        /// </summary>
        public void Enable()
        {
            this.Enabled = true;
        }

        /// <summary>
        ///     Disables the <see cref="Effect" /> on the <see cref="Control" />.
        /// </summary>
        public void Disable()
        {
            this.Enabled = false;
        }

        /// <summary>
        ///     Enables or disables the <see cref="ControlEffect" /> depending on the value of
        ///     <param name="enabled"></param>
        ///     .
        /// </summary>
        public void SetEnableState(bool enabled)
        {
            this.Enabled = enabled;
        }

        public virtual void Update(GameTime gameTime)
        {
            /* NOOP */
        }

        public virtual void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds)
        {
            /* NOOP */
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._enabled)
            {
                spriteBatch.Begin(GetSpriteBatchParameters());

                PaintEffect(spriteBatch, new Rectangle(this.Location.ToPoint(), this.Size.ToPoint()));

                spriteBatch.End();
            }
        }
    }
}
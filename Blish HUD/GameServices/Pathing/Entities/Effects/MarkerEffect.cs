using Blish_HUD.Entities.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities.Effects
{
    public class MarkerEffect : EntityEffect
    {
        // Per-effect parameters
        private const string PARAMETER_PLAYERPOSITION = "PlayerPosition";

        // Entity-unique parameters
        private const string PARAMETER_TEXTURE = "Texture";
        private const string PARAMETER_OPACITY = "Opacity";

        private const string PARAMETER_FADENEAR = "FadeNear";
        private const string PARAMETER_FADEFAR = "FadeFar";
        private float _fadeNear, _fadeFar;
        private float _opacity;

        // Per-entity parameter
        private Texture2D _texture;

        public Texture2D Texture
        {
            set
            {
                if (SetProperty(ref this._texture, value))
                {
                    this.Parameters[PARAMETER_TEXTURE].SetValue(this._texture);
                }
            }
        }

        public float Opacity
        {
            set
            {
                if (SetProperty(ref this._opacity, value))
                {
                    this.Parameters[PARAMETER_OPACITY].SetValue(this._opacity);
                }
            }
        }

        public float FadeNear
        {
            set
            {
                if (SetProperty(ref this._fadeNear, value))
                {
                    this.Parameters[PARAMETER_FADENEAR].SetValue(this._fadeNear);
                }
            }
        }

        public float FadeFar
        {
            set
            {
                if (SetProperty(ref this._fadeFar, value))
                {
                    this.Parameters[PARAMETER_FADEFAR].SetValue(this._fadeFar);
                }
            }
        }

        public void SetEntityState(Matrix world, Texture2D texture, float opacity, float fadeNear, float fadeFar)
        {
            this.World = world;
            this.Texture = texture;
            this.Opacity = opacity;
            this.FadeNear = fadeNear;
            this.FadeFar = fadeFar;
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime)
        {
            this.Parameters[PARAMETER_PLAYERPOSITION].SetValue(GameService.Player.Position);
        }

        #region ctors

        /// <inheritdoc />
        public MarkerEffect(Effect baseEffect) : base(baseEffect)
        {
        }

        /// <inheritdoc />
        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        /// <inheritdoc />
        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(
            graphicsDevice, effectCode, index, count)
        {
        }

        #endregion
    }
}
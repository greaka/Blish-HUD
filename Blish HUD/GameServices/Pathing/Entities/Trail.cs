using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities
{
    public class Trail : Entity
    {
        private BasicEffect _renderEffect;

        protected float _trailLength = -1;
        protected List<Vector3> _trailPoints;
        protected Texture2D _trailTexture;

        public Trail() : this(null)
        {
            /* NOOP */
        }

        public Trail(List<Vector3> trailPoints)
        {
            this._trailPoints = trailPoints ?? new List<Vector3>();
            InitTrailPoints();
        }

        private VertexPositionColor[] VertexData { get; set; }

        public float DistanceFromCamera => float.MaxValue;

        public virtual IReadOnlyList<Vector3> TrailPoints => this._trailPoints.AsReadOnly();

        public virtual float TrailLength
        {
            get
            {
                // Lazy load the trail length
                if (this._trailLength < 0)
                {
                    this._trailLength = 0;

                    for (var i = 0; i < this._trailPoints.Count - 1; i++)
                    {
                        this._trailLength += Vector3.Distance(this._trailPoints[i], this._trailPoints[i + 1]);
                    }
                }

                return this._trailLength;
            }
        }

        // TODO: Trail should not own this - only trails that support textures should define this
        public virtual Texture2D TrailTexture
        {
            get => this._trailTexture;
            set => SetProperty(ref this._trailTexture, value);
        }

        protected virtual void InitTrailPoints()
        {
            this._renderEffect = this._renderEffect ?? (BasicEffect) StandardEffect.Clone();
            this._renderEffect.VertexColorEnabled = true;
            this._renderEffect.TextureEnabled = false;

            if (!this._trailPoints.Any()) return;

            this.VertexData = new VertexPositionColor[this._trailPoints.Count - 1];

            for (var i = 0; i < this._trailPoints.Count - 1; i++)
            {
                this.VertexData[i] = new VertexPositionColor(this._trailPoints[i], Color.Blue);
            }
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            /* NOOP */
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            foreach (var basicPass in this._renderEffect.CurrentTechnique.Passes)
            {
                basicPass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip,
                    this.VertexData,
                    0,
                    this.VertexData.Length - 1);
            }
        }
    }
}
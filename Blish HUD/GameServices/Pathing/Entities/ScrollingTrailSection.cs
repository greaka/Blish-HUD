using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities
{
    public class ScrollingTrailSection : Trail, ITrail
    {
        private float _animationSpeed = 1;
        private float _fadeFar = 10000;

        private float _fadeNear = 10000;
        private float _scale = 1;

        private VertexBuffer _vertexBuffer;

        public ScrollingTrailSection() : base(null)
        {
            /* NOOP */
        }

        public ScrollingTrailSection(List<Vector3> trailPoints) : base(trailPoints)
        {
            /* NOOP */
        }

        private VertexPositionColorTexture[] VertexData { get; set; }

        public float AnimationSpeed
        {
            get => this._animationSpeed;
            set => SetProperty(ref this._animationSpeed, value);
        }

        public override Texture2D TrailTexture
        {
            get => this._trailTexture;
            set
            {
                if (SetProperty(ref this._trailTexture, value))
                    InitTrailPoints();
            }
        }

        public float FadeNear
        {
            get => this._fadeNear;
            set => SetProperty(ref this._fadeNear, value);
        }

        public float FadeFar
        {
            get => this._fadeFar;
            set => SetProperty(ref this._fadeFar, value);
        }

        public float Scale
        {
            get => this._scale;
            set => SetProperty(ref this._scale, value);
        }

        private List<Vector3> SetTrailResolution(List<Vector3> trailPoints, float pointResolution)
        {
            var tempTrail = new List<Vector3>();

            var lstPoint = trailPoints[0];

            for (var i = 0; i < trailPoints.Count; i++)
            {
                var dist = Vector3.Distance(lstPoint, trailPoints[i]);

                var s = dist / pointResolution;
                var inc = 1 / s;

                for (var v = inc; v < s - inc; v += inc)
                {
                    var nPoint = Vector3.Lerp(lstPoint, this._trailPoints[i], v / s);

                    tempTrail.Add(nPoint);
                }

                tempTrail.Add(trailPoints[i]);

                lstPoint = trailPoints[i];
            }

            return tempTrail;
        }

        protected override void InitTrailPoints()
        {
            if (!this._trailPoints.Any()) return;

            // TacO has a minimum of 30, so we'll use 30
            this._trailPoints = SetTrailResolution(this._trailPoints, 30f);

            this.VertexData = new VertexPositionColorTexture[this.TrailPoints.Count * 2];

            var imgScale = ScrollingTrail.TRAIL_WIDTH;

            var pastDistance = this.TrailLength;

            var offsetDirection = new Vector3(0, 0, -1);

            var currPoint = this.TrailPoints[0];
            var offset = Vector3.Zero;

            for (var i = 0; i < this.TrailPoints.Count - 1; i++)
            {
                var nextPoint = this.TrailPoints[i + 1];

                var pathDirection = nextPoint - currPoint;

                offset = Vector3.Cross(pathDirection, offsetDirection);

                offset.Normalize();

                var leftPoint = currPoint + offset * imgScale;
                var rightPoint = currPoint + offset * -imgScale;

                this.VertexData[i * 2 + 1] = new VertexPositionColorTexture(leftPoint, Color.White,
                    new Vector2(0f, pastDistance / (imgScale * 2) - 1));
                this.VertexData[i * 2] = new VertexPositionColorTexture(rightPoint, Color.White,
                    new Vector2(1f, pastDistance / (imgScale * 2) - 1));

                pastDistance -= Vector3.Distance(currPoint, nextPoint);

                currPoint = nextPoint;

#if PLOTTRAILS
                GameService.Overlay.QueueMainThreadUpdate((gameTime) => {
                    var leftBoxPoint = new Cube() {
                        Color = Color.Red,
                        Size = new Vector3(0.25f),
                        Position = leftPoint
                    };

                    var rightBoxPoint = new Cube() {
                        Color = Color.Red,
                        Size = new Vector3(0.25f),
                        Position = rightPoint
                    };

                    GameService.Graphics.World.Entities.Add(leftBoxPoint);
                    GameService.Graphics.World.Entities.Add(rightBoxPoint);
                });
#endif
            }

            var fleftPoint = currPoint + offset * imgScale;
            var frightPoint = currPoint + offset * -imgScale;

            this.VertexData[this.TrailPoints.Count * 2 - 1] = new VertexPositionColorTexture(fleftPoint, Color.White,
                new Vector2(0f, pastDistance / (imgScale * 2) - 1));
            this.VertexData[this.TrailPoints.Count * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White,
                new Vector2(1f, pastDistance / (imgScale * 2) - 1));

            this._vertexBuffer = new VertexBuffer(BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice,
                VertexPositionColorTexture.VertexDeclaration, this.VertexData.Length, BufferUsage.WriteOnly);
            this._vertexBuffer.SetData(this.VertexData);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _basicTrailEffect.Parameters["TotalMilliseconds"]
                .SetValue((float) gameTime.TotalGameTime.TotalMilliseconds);
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            if ((this.TrailTexture == null) || (this.VertexData == null) || (this.VertexData.Length < 3)) return;

            _basicTrailEffect.Parameters["WorldViewProjection"].SetValue(GameService.Camera.WorldViewProjection);
            _basicTrailEffect.Parameters["PlayerViewProjection"]
                .SetValue(GameService.Camera.PlayerView * GameService.Camera.Projection);
            _basicTrailEffect.Parameters["Texture"].SetValue(this.TrailTexture);
            _basicTrailEffect.Parameters["FlowSpeed"].SetValue(this.AnimationSpeed);
            _basicTrailEffect.Parameters["PlayerPosition"].SetValue(GameService.Player.Position);
            _basicTrailEffect.Parameters["FadeNear"].SetValue(this.FadeNear);
            _basicTrailEffect.Parameters["FadeFar"].SetValue(this.FadeFar);
            _basicTrailEffect.Parameters["Opacity"].SetValue(this.Opacity);
            _basicTrailEffect.Parameters["TotalLength"].SetValue(20f);

            graphicsDevice.SetVertexBuffer(this._vertexBuffer, 0);

            foreach (var trailPass in _basicTrailEffect.CurrentTechnique.Passes)
            {
                trailPass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, this._vertexBuffer.VertexCount - 2);

                //graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip,
                //                                  this.VertexData,
                //                                  0,
                //                                  this.VertexData.Length - 2);
            }
        }

        #region Load Static

        private static readonly Effect _basicTrailEffect;

        static ScrollingTrailSection()
        {
            _basicTrailEffect = BlishHud.ActiveContentManager.Load<Effect>("effects\\trail");
        }

        #endregion
    }
}
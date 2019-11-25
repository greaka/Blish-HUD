using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class CameraService : GameService
    {
        public float NearPlaneRenderDistance { get; set; } = 0.01f;
        public float FarPlaneRenderDistance { get; set; } = 1000.0f;
        public Vector3 Position { get; private set; }

        public Vector3 Forward { get; private set; }

        public Matrix View { get; private set; }

        public Matrix PlayerView { get; private set; }

        public Matrix Projection { get; private set; }

        public Matrix WorldViewProjection { get; private set; }

        protected override void Update(GameTime gameTime)
        {
            if (Gw2Mumble.Available)
            {
                this.Position = Gw2Mumble.MumbleBacking.CameraPosition.ToXnaVector3();
                this.Forward = Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3();

                this.View = Matrix.CreateLookAt(this.Position, this.Position + this.Forward,
                    VectorUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                this.PlayerView = Matrix.CreateLookAt(this.Position, Player.Position + new Vector3(0, 0, 0.5f),
                    VectorUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                this.Projection = Matrix.CreatePerspectiveFieldOfView(
                    (float) Gw2Mumble.MumbleBacking.Identity.FieldOfView, Graphics.AspectRatio,
                    this.NearPlaneRenderDistance, this.FarPlaneRenderDistance);

                this.WorldViewProjection = this.View * this.Projection;
            }
        }

        protected override void Initialize()
        {
            /* NOOP */
        }

        protected override void Load()
        {
            /* NOOP */
        }

        protected override void Unload()
        {
            /* NOOP */
        }
    }
}
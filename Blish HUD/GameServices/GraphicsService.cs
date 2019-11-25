using System;
using System.Collections.Concurrent;
using System.Windows.Forms;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = Microsoft.Xna.Framework.Point;
using Screen = Blish_HUD.Controls.Screen;

namespace Blish_HUD
{
    public class GraphicsService : GameService
    {
        public enum UiScale
        {
            Small, // 47 x 47
            Normal, // 52 x 52
            Large, // 58 x 58
            Larger // 64 x 64
        }

        private readonly ConcurrentQueue<Action<GraphicsDevice>> _queuedRenders =
            new ConcurrentQueue<Action<GraphicsDevice>>();

        private UiScale _uiScale = UiScale.Normal;

        public UiScale UIScale
        {
            get => this._uiScale;
            set
            {
                if (this._uiScale == value) return;

                this._uiScale = value;

                this.UIScaleMultiplier = GetScaleRatio(value);
                this.SpriteScreen.Size =
                    new Point(
                        (int) (BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth / this.UIScaleMultiplier),
                        (int) (BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight /
                               this.UIScaleMultiplier));

                this.UIScaleTransform = Matrix.CreateScale(this.UIScaleMultiplier);
            }
        }

        public Matrix UIScaleTransform { get; private set; } = Matrix.Identity;

        public float UIScaleMultiplier { get; private set; } = 1f;

        public Screen SpriteScreen => _spriteScreen;

        public World World => _world;

        public GraphicsDeviceManager GraphicsDeviceManager => BlishHud.ActiveGraphicsDeviceManager;

        public GraphicsDevice GraphicsDevice => BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;
        public float AspectRatio { get; private set; }

        public Point Resolution
        {
            get => new Point(BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth,
                BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);
            set
            {
                try
                {
                    BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth = value.X;
                    BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight = value.Y;

                    BlishHud.ActiveGraphicsDeviceManager.ApplyChanges();

                    // Exception would be from the code above, but don't update our
                    // scaling if there is an exception
                    ScreenSizeUpdated(value);
                }
                catch (SharpDXException sdxe)
                {
                    // If device lost, we should hopefully handle in device lost event below
                }
            }
        }

        public float GetScaleRatio(UiScale currScale)
        {
            switch (currScale)
            {
                case UiScale.Small:
                    return 0.810f;
                case UiScale.Normal:
                    return 0.897f;
                case UiScale.Large:
                    return 1f;
                case UiScale.Larger:
                    return 1.103f;
            }

            return 1f;
        }

        /// <summary>
        ///     Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see="GameTime" /> as a parameter.</param>
        public void QueueMainThreadRender(Action<GraphicsDevice> call)
        {
            this._queuedRenders.Enqueue(call);
        }

        private void ScreenSizeUpdated(Point newSize)
        {
            // Update the SpriteScreen
            this.SpriteScreen.Size = new Point((int) (newSize.X / this.UIScaleMultiplier),
                (int) (newSize.Y / this.UIScaleMultiplier));

            // Update the aspect ratio
            this.AspectRatio = Graphics.WindowWidth / (float) Graphics.WindowHeight;
        }

        protected override void Initialize()
        {
            // If for some reason we lose the rendering device, just restart the application
            // Might do better error handling later on
            this.ActiveBlishHud.GraphicsDevice.DeviceLost += delegate { Application.Restart(); };

            this.UIScaleMultiplier = GetScaleRatio(this.UIScale);
            this.UIScaleTransform = Matrix.CreateScale(Graphics.UIScaleMultiplier);
        }

        internal void Render(GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.GraphicsDevice.Clear(Color.Transparent);

            Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game
            if (GameIntegration.IsInGame && (!ArcDps.RenderPresent || ArcDps.HudIsActive))
                this.World.DoDraw(this.GraphicsDevice);
            Debug.StopTimeFunc("3D objects");

            // Slightly better scaling (text is a bit more legible)
            this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            Debug.StartTimeFunc("UI Elements");
            if ((this.SpriteScreen != null) && this.SpriteScreen.Visible)
            {
                this.SpriteScreen.Draw(spriteBatch, this.SpriteScreen.LocalBounds, this.SpriteScreen.LocalBounds);
            }

            Debug.StopTimeFunc("UI Elements");

            Debug.StartTimeFunc("Render Queue");
            if (this._queuedRenders.TryDequeue(out var renderCall))
            {
                renderCall.Invoke(this.GraphicsDevice);
            }

            Debug.StopTimeFunc("Render Queue");
        }

        protected override void Load()
        {
            /* NOOP */
        }

        protected override void Unload()
        {
            /* NOOP */
        }

        protected override void Update(GameTime gameTime)
        {
            this.World.DoUpdate(gameTime);
            EntityEffect.UpdateEffects(gameTime);
            this.SpriteScreen.Update(gameTime);
        }

        #region Load Static

        private static readonly Screen _spriteScreen;
        private static readonly World _world;

        static GraphicsService()
        {
            _spriteScreen = new Screen();
            _world = new World();
        }

        #endregion
    }
}
﻿using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD
{
    public class BlishHud : Game
    {
        // Primarily used to draw debug text
        private SpriteBatch _basicSpriteBatch;

        public BlishHud()
        {
            ActiveGraphicsDeviceManager = new GraphicsDeviceManager(this);
            ActiveGraphicsDeviceManager.PreparingDeviceSettings +=
                delegate(object sender, PreparingDeviceSettingsEventArgs args)
                {
                    args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
                };

            ActiveGraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            ActiveGraphicsDeviceManager.PreferMultiSampling = true;

            ActiveContentManager = this.Content;

            this.Content.RootDirectory = "Content";

            this.IsMouseVisible = true;
        }

        public static IntPtr FormHandle { get; private set; }

        public static Form Form { get; private set; }

        // TODO: Move this into GraphicsService
        public static RasterizerState UiRasterizer { get; private set; }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            FormHandle = this.Window.Handle;
            Form = Control.FromHandle(FormHandle).FindForm();

            this.Window.IsBorderless = true;
            this.Window.AllowAltF4 = false;

#if DEBUG
            ActiveGraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
#endif

            // Initialize all game services
            foreach (var service in GameService.All)
            {
                service.DoInitialize(this);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            UiRasterizer = new RasterizerState
            {
                ScissorTestEnable = true
            };

            // Create a new SpriteBatch, which can be used to draw debug information
            this._basicSpriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Let all of the game services have a chance to unload
            foreach (var service in GameService.All)
            {
                service.DoUnload();
            }
        }

        protected override void BeginRun()
        {
            base.BeginRun();

            // Let all of the game services have a chance to load
            foreach (var service in GameService.All)
            {
                service.DoLoad();
            }
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // If gw2 isn't open - only update the most important things:
            if (!GameService.GameIntegration.Gw2IsRunning)
            {
                GameService.Debug.DoUpdate(gameTime);
                GameService.GameIntegration.DoUpdate(gameTime);

                return;
            }

            // Update all game services
            foreach (var service in GameService.All)
            {
                GameService.Debug.StartTimeFunc($"Service: {service.GetType().Name}");
                service.DoUpdate(gameTime);
                GameService.Debug.StopTimeFunc($"Service: {service.GetType().Name}");
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!GameService.GameIntegration.Gw2IsRunning) return;

            GameService.Graphics.Render(gameTime, this._basicSpriteBatch);

#if DEBUG
            this._basicSpriteBatch.Begin();

            GameService.Debug.DrawDebugOverlay(this._basicSpriteBatch, gameTime);
#endif

            this._basicSpriteBatch.End();

            base.Draw(gameTime);
        }

        #region Internal Static Members

        /// <summary>
        ///     Exposed through the <see cref="GraphicsService" />'s <see cref="GraphicsService.GraphicsDeviceManager" />.
        /// </summary>
        internal static GraphicsDeviceManager ActiveGraphicsDeviceManager { get; private set; }

        /// <summary>
        ///     Exposed through the <see cref="ContentService" />'s <see cref="ContentService.ContentManager" />.
        /// </summary>
        internal static ContentManager ActiveContentManager { get; private set; }

        #endregion
    }
}
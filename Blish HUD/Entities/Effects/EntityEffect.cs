﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Effects
{
    public abstract class EntityEffect : Effect
    {
        private const string PARAMETER_WORLD = "World";
        private const string PARAMETER_VIEW = "View";
        private const string PARAMETER_PROJECTION = "Projection";

        private static readonly Logger Logger = Logger.GetLogger<EntityEffect>();

        private static readonly HashSet<EntityEffect> _loadedEffects = new HashSet<EntityEffect>();

        private Matrix _world, _view, _projection;

        /// <summary>
        ///     The per-entity matrix.
        /// </summary>
        public Matrix World
        {
            set
            {
                if (SetProperty(ref this._world, value))
                {
                    this.Parameters[PARAMETER_WORLD].SetValue(this._world);
                }
            }
        }

        /// <summary>
        ///     Representation of the active camera's look-at matrix.
        /// </summary>
        public Matrix View
        {
            set
            {
                if (SetProperty(ref this._view, value))
                {
                    this.Parameters[PARAMETER_VIEW].SetValue(this._view);
                }
            }
        }

        /// <summary>
        ///     The projection matrix created from the camera based on field of view and aspect ratio.
        /// </summary>
        public Matrix Projection
        {
            set
            {
                if (SetProperty(ref this._projection, value))
                {
                    this.Parameters[PARAMETER_PROJECTION].SetValue(this._projection);
                }
            }
        }

        internal static void UpdateEffects(GameTime gameTime)
        {
            foreach (var loadedEffect in _loadedEffects.ToArray())
            {
                if (loadedEffect.IsDisposed)
                {
                    Logger.Debug("An EntityEffect was disposed of.");
                    _loadedEffects.Remove(loadedEffect);
                    continue;
                }

                loadedEffect.View = GameService.Camera.View;
                loadedEffect.Projection = GameService.Camera.Projection;

                loadedEffect.Update(gameTime);
            }
        }

        private static void RegisterEntityEffect(EntityEffect effectInstance)
        {
            Logger.Debug("EntityEffect {effectName} was registered.", effectInstance.GetType().FullName);
            _loadedEffects.Add(effectInstance);
        }

        protected abstract void Update(GameTime gameTime);

        protected bool SetProperty<T>(ref T property, T newValue)
        {
            if (Equals(property, newValue)) return false;

            property = newValue;

            return true;
        }

        #region ctors

        /// <inheritdoc />
        protected EntityEffect(Effect cloneSource) : base(cloneSource)
        {
            RegisterEntityEffect(this);
        }

        /// <inheritdoc />
        protected EntityEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            RegisterEntityEffect(this);
        }

        /// <inheritdoc />
        protected EntityEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(
            graphicsDevice, effectCode, index, count)
        {
            RegisterEntityEffect(this);
        }

        #endregion
    }
}
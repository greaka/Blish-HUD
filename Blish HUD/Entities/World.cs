using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities
{
    public class World : Entity
    {
        private IOrderedEnumerable<Entity> _sortedEntities;

        public World()
        {
            this.Entities = new SynchronizedCollection<Entity>();
            UpdateEntitySort();
        }

        public SynchronizedCollection<Entity> Entities { get; }

        private void UpdateEntitySort()
        {
            this._sortedEntities = this.Entities.ToList().OrderByDescending(e => e.DistanceFromCamera);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice)
        {
            /* NOOP - world does not need to rebuild */
        }

        public override void DoUpdate(GameTime gameTime)
        {
            UpdateEntitySort();

            foreach (var entity in this._sortedEntities)
            {
                entity.DoUpdate(gameTime);
            }
        }

        public override void DoDraw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var entity in this._sortedEntities.Where(entity => entity.Visible))
            {
                entity.DoDraw(graphicsDevice);
            }
        }
    }
}
using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("mesh")]
    public class Mesh<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        private Mesh _meshEntity;

        private string _meshName = string.Empty;

        /// <inheritdoc />
        public Mesh(TPathable managedPathable) : base(managedPathable)
        {
        }

        /// <inheritdoc />
        public override void Load()
        {
            var loadedMesh = GameService.Content.ContentManager.Load<Model>($@"models\{this._meshName}");

            this._meshEntity = new Mesh(loadedMesh)
            {
                Position = this.ManagedPathable.Position
            };

            this.ManagedPathable.Opacity = 0f;
            GameService.Graphics.World.Entities.Add(this._meshEntity);
        }

        /// <inheritdoc />
        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "mesh":
                        this._meshName = attr.Value.Trim();
                        break;
                }
            }
        }
    }
}
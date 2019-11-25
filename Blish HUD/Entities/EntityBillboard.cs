using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Entities
{
    public class EntityBillboard : Billboard
    {
        private Entity _attachedEntity;

        public EntityBillboard(Entity attachedEntity)
        {
            this.AttachedEntity = attachedEntity;
        }

        public Entity AttachedEntity { get; }

        /// <inheritdoc />
        public override Vector3 Position => this.AttachedEntity.Position + this.AttachedEntity.RenderOffset;
    }
}
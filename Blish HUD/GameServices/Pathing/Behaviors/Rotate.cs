using System.Collections.Generic;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("rotate")]
    internal class Rotate<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public Rotate(TPathable managedPathable) : base(managedPathable)
        {
        }

        public float RotationX
        {
            get => MathHelper.ToDegrees(this.ManagedPathable.ManagedEntity.RotationX);
            set => this.ManagedPathable.ManagedEntity.RotationX = MathHelper.ToRadians(value);
        }

        public float RotationY
        {
            get => MathHelper.ToDegrees(this.ManagedPathable.ManagedEntity.RotationY);
            set => this.ManagedPathable.ManagedEntity.RotationY = MathHelper.ToRadians(value);
        }

        public float RotationZ
        {
            get => MathHelper.ToDegrees(this.ManagedPathable.ManagedEntity.RotationZ);
            set => this.ManagedPathable.ManagedEntity.RotationZ = MathHelper.ToRadians(value);
        }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            var rotateX = 0f;
            var rotateY = 0f;
            var rotateZ = 0f;

            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "rotate-x":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateX);
                        break;
                    case "rotate-y":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateY);
                        break;
                    case "rotate-z":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateZ);
                        break;
                }
            }

            this.ManagedPathable.ManagedEntity.Rotation = new Vector3(MathHelper.ToRadians(rotateX),
                MathHelper.ToRadians(rotateY),
                MathHelper.ToRadians(rotateZ));
        }
    }
}
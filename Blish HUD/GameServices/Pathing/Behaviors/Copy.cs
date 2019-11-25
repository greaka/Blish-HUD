using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("copy")]
    public class Copy<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public Copy(TPathable managedPathable) : base(managedPathable)
        {
            var zoneActivator = new ZoneActivator<TPathable, TEntity>(this)
            {
                ActivationDistance = 5f,
                DistanceFrom = DistanceFrom.Player
            };
        }

        public string CopyValue { get; set; }

        public int CopyRadius { get; set; } = 5;

        public string CopyMessage { get; set; } = "'{0}' copied to clipboard.";

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "copy":
                        this.CopyValue = attr.Value;
                        break;
                    case "copy-radius":
                        this.CopyRadius = int.Parse(attr.Value);
                        break;
                    case "copy-message":
                        this.CopyMessage = attr.Value;
                        break;
                }
            }
        }
    }
}
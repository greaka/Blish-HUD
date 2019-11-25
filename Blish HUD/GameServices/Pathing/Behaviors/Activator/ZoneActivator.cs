using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors.Activator
{
    public enum DistanceFrom
    {
        Player,
        PlayerCamera
    }

    [BehaviorActivator("inzone")]
    /// <summary>
    /// An activator that activates when the player or camera enters within a certain radius.
    /// </summary>
    public class ZoneActivator<TPathable, TEntity> : Activator<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public ZoneActivator(PathingBehavior<TPathable, TEntity> associatedBehavior) : base(associatedBehavior)
        {
            /* NOOP */
        }

        public float ActivationDistance { get; set; } = 3.5f;

        public DistanceFrom DistanceFrom { get; set; } = DistanceFrom.Player;

        public override void Update(GameTime gameTime)
        {
            var farPoint = this.DistanceFrom == DistanceFrom.Player
                ? GameService.Player.Position
                : GameService.Camera.Position;

            if (Vector3.Distance(this.AssociatedBehavior.ManagedPathable.Position, farPoint) <= this.ActivationDistance)
            {
                if (!this.Active)
                    Activate();
            }
            else if (this.Active)
            {
                Deactivate();
            }

            base.Update(gameTime);
        }
    }
}
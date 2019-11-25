using Blish_HUD.Entities;

namespace Blish_HUD.Pathing.Behaviors.Activator
{
    [BehaviorActivator("always")]
    /// <summary>
    /// An activator that is always activated and represents the default activator if no other activator is supplied.
    /// </summary>
    public class Always<TPathable, TEntity> : Activator<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public Always(PathingBehavior<TPathable, TEntity> associatedBehavior) : base(associatedBehavior)
        {
            Activate();
        }
    }
}
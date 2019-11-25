using System;
using Blish_HUD.Entities;
using Microsoft.Scripting.Runtime;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors.Activator
{
    public abstract class Activator<TPathable, TEntity> : IDisposable
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public Activator([NotNull] PathingBehavior<TPathable, TEntity> associatedBehavior)
        {
            this.AssociatedBehavior = associatedBehavior;
        }

        public bool Active { get; private set; }

        protected PathingBehavior<TPathable, TEntity> AssociatedBehavior { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        protected void Activate()
        {
            this.Active = true;
            Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void Deactivate()
        {
            this.Active = false;
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Update(GameTime gameTime)
        {
            /* NOOP */
        }

        protected virtual void OnDispose()
        {
            /* NOOP */
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnDispose();
            }
        }
    }
}
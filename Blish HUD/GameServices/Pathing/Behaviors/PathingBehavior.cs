using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blish_HUD.PersistentStore;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors
{
    public abstract class PathingBehavior
    {
        private const string PATHINGBEHAVIOR_STORENAME = "Behaviors";

        public static List<Type> AllAvailableBehaviors { get; }

        protected Store BehaviorStore => _behaviorStore;

        public virtual void Load()
        {
            /* NOOP */
        }

        public abstract void UpdateBehavior(GameTime gameTime);

        #region Load Static

        private static readonly Store _behaviorStore;

        static PathingBehavior()
        {
            _behaviorStore = GameService.Pathing.PathingStore.GetSubstore(PATHINGBEHAVIOR_STORENAME);

            AllAvailableBehaviors = PathingBehaviorAttribute.GetTypes(Assembly.GetExecutingAssembly()).ToList();
        }

        #endregion
    }
}
using System.Collections.Generic;

namespace Blish_HUD.Pathing.Behaviors
{
    public interface ILoadableBehavior
    {
        void LoadWithAttributes(IEnumerable<PathableAttribute> attributes);

        void Load();
    }
}
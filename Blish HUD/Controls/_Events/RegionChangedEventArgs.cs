using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls
{
    public class RegionChangedEventArgs : EventArgs
    {
        public RegionChangedEventArgs(Rectangle previousRegion, Rectangle currentRegion)
        {
            this.PreviousRegion = previousRegion;
            this.CurrentRegion = currentRegion;
        }

        public Rectangle PreviousRegion { get; }
        public Rectangle CurrentRegion { get; }
    }
}
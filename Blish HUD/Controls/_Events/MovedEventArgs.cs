using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls
{
    public class MovedEventArgs : EventArgs
    {
        public MovedEventArgs(Point previousLocation, Point currentLocation)
        {
            this.PreviousLocation = previousLocation;
            this.CurrentLocation = currentLocation;
        }

        public Point PreviousLocation { get; }
        public Point CurrentLocation { get; }
    }
}
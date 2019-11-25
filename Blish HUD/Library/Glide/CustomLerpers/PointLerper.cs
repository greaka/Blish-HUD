using System;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Library.Glide.CustomLerpers
{
    public class PointLerper : MemberLerper
    {
        private Point _pointFrom;
        private Point _pointRange;
        private Point _pointTo;

        public override void Initialize(object fromValue, object toValue, Behavior behavior)
        {
            this._pointFrom = (Point) fromValue;
            this._pointTo = (Point) toValue;
            this._pointRange = this._pointTo - this._pointFrom;
        }

        public override object Interpolate(float t, object currentValue, Behavior behavior)
        {
            var x = this._pointFrom.X + this._pointRange.X * t;
            var y = this._pointFrom.Y + this._pointRange.Y * t;

            // Only a subtle difference since Point only supports int anyways
            if (behavior.HasFlag(Behavior.Round))
            {
                x = (float) Math.Round(x);
                y = (float) Math.Round(y);
            }

            var current = (Point) currentValue;

            if (this._pointRange.X != 0) current.X = (int) x;
            if (this._pointRange.Y != 0) current.Y = (int) y;

            return current;
        }
    }
}
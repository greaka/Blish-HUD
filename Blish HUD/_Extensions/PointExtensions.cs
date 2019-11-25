using System;
using System.Drawing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Blish_HUD
{
    public static class PointExtensions
    {
        public static Point ToSystemDrawingPoint(this Microsoft.Xna.Framework.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Microsoft.Xna.Framework.Point ScaleToUi(this Microsoft.Xna.Framework.Point point)
        {
            return new Microsoft.Xna.Framework.Point((int) (point.X * GameService.Graphics.UIScaleMultiplier),
                (int) (point.Y * GameService.Graphics.UIScaleMultiplier));
        }

        public static Microsoft.Xna.Framework.Point UiToScale(this Microsoft.Xna.Framework.Point point)
        {
            return new Microsoft.Xna.Framework.Point((int) (point.X / GameService.Graphics.UIScaleMultiplier),
                (int) (point.Y / GameService.Graphics.UIScaleMultiplier));
        }

        public static Rectangle InBounds(this Microsoft.Xna.Framework.Point point, Rectangle bounds)
        {
            return new Rectangle(bounds.Location, point);
        }

        public static Size ToSystemDrawingSize(this Microsoft.Xna.Framework.Point point)
        {
            return new Size(point.X, point.Y);
        }

        public static Microsoft.Xna.Framework.Point ToXnaPoint(this Point point)
        {
            return new Microsoft.Xna.Framework.Point(point.X, point.Y);
        }

        public static Microsoft.Xna.Framework.Point ResizeKeepAspect(Microsoft.Xna.Framework.Point src, int maxWidth,
            int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.X);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Y);

            var rnd = Math.Min(maxWidth / (decimal) src.X, maxHeight / (decimal) src.Y);
            return new Microsoft.Xna.Framework.Point((int) Math.Round(src.X * rnd), (int) Math.Round(src.Y * rnd));
        }
    }
}
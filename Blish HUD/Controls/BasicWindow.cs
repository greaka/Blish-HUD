using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class BasicWindow : WindowBase
    {
        #region Load Static

        #endregion

        public BasicWindow(Texture2D background, Vector2 backgroundOrigin) : this(background, backgroundOrigin, null,
            Thickness.Zero, 0, true)
        {
            /* NOOP */
        }

        public BasicWindow(Texture2D background, Vector2 backgroundOrigin, Rectangle? windowBackgroundBounds) : this(
            background, backgroundOrigin, windowBackgroundBounds, Thickness.Zero, 0, true)
        {
            /* NOOP */
        }

        public BasicWindow(Texture2D background, Vector2 backgroundOrigin, Rectangle? windowBackgroundBounds,
            Thickness outerPadding) : this(background, backgroundOrigin, windowBackgroundBounds, outerPadding, 0, true)
        {
            /* NOOP */
        }

        public BasicWindow(Texture2D background, Vector2 backgroundOrigin, Rectangle? windowBackgroundBounds,
            Thickness outerPadding, int titleBarHeight) : this(background, backgroundOrigin, windowBackgroundBounds,
            outerPadding, titleBarHeight, true)
        {
            /* NOOP */
        }

        public BasicWindow(Texture2D background, Vector2 backgroundOrigin, Rectangle? windowBackgroundBounds,
            Thickness outerPadding, int titleBarHeight, bool standardWindow)
        {
            ConstructWindow(background, backgroundOrigin, windowBackgroundBounds, outerPadding, titleBarHeight,
                standardWindow);
        }
    }
}
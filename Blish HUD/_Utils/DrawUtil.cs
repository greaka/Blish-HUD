using System.Linq;
using System.Text;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD
{
    public static class DrawUtil
    {
        public static void DrawAlignedText(SpriteBatch sb, SpriteFont sf, string text, Rectangle bounds, Color clr,
            HorizontalAlignment ha, VerticalAlignment va)
        {
            // Filter out any characters our font doesn't support
            text = string.Join("", text.ToCharArray().Where(c => sf.Characters.Contains(c)));

            var textSize = sf.MeasureString(text);

            var xPos = bounds.X;
            var yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int) textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int) textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int) textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int) textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        public static void DrawAlignedText(SpriteBatch sb, BitmapFont sf, string text, Rectangle bounds, Color clr,
            HorizontalAlignment ha = HorizontalAlignment.Left, VerticalAlignment va = VerticalAlignment.Middle)
        {
            Vector2 textSize = sf.MeasureString(text);

            var xPos = bounds.X;
            var yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int) textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int) textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int) textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int) textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        /// <remarks> Source: https://stackoverflow.com/a/15987581/595437 </remarks>
        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var words = text.Split(' ');
            var sb = new StringBuilder();
            var lineWidth = 0f;
            var spaceWidth = spriteFont.MeasureString(" ").Width;

            foreach (var word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }
    }
}
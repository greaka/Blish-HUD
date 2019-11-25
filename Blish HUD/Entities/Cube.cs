using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Entities
{
    public class Cube : Cuboid
    {
        public Cube()
        {
            this.Texture = ContentService.Textures.Pixel;
            this.Size = new Vector3(0.25f, 0.25f, 0.25f);
        }

        public Color Color { get; set; }
    }
}
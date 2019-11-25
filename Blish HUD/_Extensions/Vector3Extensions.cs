using GW2NET.Common.Drawing;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public static class Vector3Extensions
    {
        // Note: GW2 uses left handed coordinates and XNA uses right handed coordinates.
        public static Vector3 ToXnaVector3(this Vector3D gw2v3d)
        {
            return new Vector3((float) gw2v3d.X, (float) gw2v3d.Z, (float) gw2v3d.Y);
        }

        public static string ToRoundedString(this Vector3 v3)
        {
            return string.Format("X: {0:0,0} Y: {1:0,0} Z: {2:0,0}",
                v3.X,
                v3.Y,
                v3.Z
            );
        }
    }
}
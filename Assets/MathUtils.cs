using UnityEngine;

namespace Chars.Utils
{
    public static class MathUtils
    {
        public static Vector2Int[] FourDirectionsInt = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        public static Vector2Int[] EightDirectionsInt =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.down + Vector2Int.right,
        };

        public static bool InsideGridLimits(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}

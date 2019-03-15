using BruTile;

namespace Terrain.ExtensionMethods
{
    public static class TileIndexExtension
    {
        public static string ToIndexString(this TileIndex t)
        {
            return $"{t.Level}/{t.Col}/{t.Row}";
        }
    }
}

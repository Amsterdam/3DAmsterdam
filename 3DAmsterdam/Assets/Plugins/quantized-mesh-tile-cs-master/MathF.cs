namespace QuantizedMeshTerrain.Tiles
{
    public static class Mathf
    {
        public static double Lerp(double start, double end, double by)
        {
            return (1 - by) * start + by * end;
        }
    }
}

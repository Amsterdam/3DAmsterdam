namespace QuantizedMeshTerrain.Tiles
{
    public class Coordinate
    {
        public Coordinate(double X, double Y, double Height)
        {
            this.X = X;
            this.Y = Y;
            this.Height = Height;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
    }
}

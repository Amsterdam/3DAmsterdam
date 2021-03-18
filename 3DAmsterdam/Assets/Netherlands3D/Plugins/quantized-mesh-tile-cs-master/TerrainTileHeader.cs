using System.IO;
using System.Runtime.InteropServices;

namespace QuantizedMeshTerrain.Tiles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerrainTileHeader
    {
        // The center of the tile in Earth-centered Fixed coordinates.
        public double CenterX;
        public double CenterY;
        public double CenterZ;

        // The minimum and maximum heights in the area covered by this tile.
        // The minimum may be lower and the maximum may be higher than
        // the height of any vertex in this tile in the case that the min/max vertex
        // was removed during mesh simplification, but these are the appropriate
        // values to use for analysis or visualization.
        public float MinimumHeight;
        public float MaximumHeight;

        // The tile’s bounding sphere.  The X,Y,Z coordinates are again expressed
        // in Earth-centered Fixed coordinates, and the radius is in meters.
        public double BoundingSphereCenterX;
        public double BoundingSphereCenterY;
        public double BoundingSphereCenterZ;
        public double BoundingSphereRadius;

        // The horizon occlusion point, expressed in the ellipsoid-scaled Earth-centered Fixed frame.
        // If this point is below the horizon, the entire tile is below the horizon.
        // See http://cesiumjs.org/2013/04/25/Horizon-culling/ for more information.
        public double HorizonOcclusionPointX;
        public double HorizonOcclusionPointY;
        public double HorizonOcclusionPointZ;

        public TerrainTileHeader(BinaryReader reader)
        {
            CenterX = reader.ReadDouble();
            CenterY = reader.ReadDouble();
            CenterZ = reader.ReadDouble();

            MinimumHeight = reader.ReadSingle();
            MaximumHeight = reader.ReadSingle();

            BoundingSphereCenterX = reader.ReadDouble();
            BoundingSphereCenterY = reader.ReadDouble();
            BoundingSphereCenterZ = reader.ReadDouble();
            BoundingSphereRadius = reader.ReadDouble();

            HorizonOcclusionPointX = reader.ReadDouble();
            HorizonOcclusionPointY = reader.ReadDouble();
            HorizonOcclusionPointZ = reader.ReadDouble();
        }
    }
}

using BruTile;

namespace Terrain
{
    public class TmsGlobalGeodeticTileSchema : TileSchema
    {
        public TmsGlobalGeodeticTileSchema()
        {
            OriginX = -180;
            OriginY = -90;
            YAxis = YAxis.TMS;
            Extent = new Extent(-180, -90, 180, 90);
            var f = 0.70312500000000000000;

            for (var p = 0; p <= 20; p++)
            {
                Resolutions.Add(p.ToString(), new Resolution(p.ToString(), f));
                f = f / 2;
            }

            Srs = "EPSG:4326";
        }
    }
}

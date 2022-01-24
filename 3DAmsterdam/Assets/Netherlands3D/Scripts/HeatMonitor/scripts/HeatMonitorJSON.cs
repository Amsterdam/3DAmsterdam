
using System.Collections.Generic;

namespace HeatMonitor
{

    public class HeatMonitorJSON
    {

        public string caption;
        public List<Layer> layers;

        [System.Serializable]
        public class Tiles
        {
            public int startx;
            public int starty;
            public int endx;
            public int endy;
        }
        [System.Serializable]
        public class Legend
        {
            public int min;
            public int max;
        }

        [System.Serializable]
        public class Layer
        {
            public string id;
            public string location;
            public string title;
            public string description;
            public Tiles tiles;
            public Legend legend;
        }

    }

}
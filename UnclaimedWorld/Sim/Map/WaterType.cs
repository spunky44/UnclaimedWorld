using System;
using System.Collections.Generic;
using System.Text;

namespace UWGame.SimSide.Maps
{
    public class WaterType : SurfaceType
    {
        private static WaterType instance;
        public static WaterType Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new WaterType();
                    return instance;
                }
            }
        }

        private WaterType()
        {
            costs = new byte[3, 5];

            costs[(int)TransportType.Foot, (int)TerrainFeatures.None] = 0;
            costs[(int)TransportType.Foot, (int)TerrainFeatures.FootPath] = 0; 
            costs[(int)TransportType.Foot, (int)TerrainFeatures.WheelPath] = 0; // 2;
            costs[(int)TransportType.Foot, (int)TerrainFeatures.GravelRoad] = 0; // 1;
            costs[(int)TransportType.Foot, (int)TerrainFeatures.PavedRoad] = 0; // 1;

            costs[(int)TransportType.Car, (int)TerrainFeatures.None] = 0;
            costs[(int)TransportType.Car, (int)TerrainFeatures.FootPath] = 0;
            costs[(int)TransportType.Car, (int)TerrainFeatures.WheelPath] = 0;
            costs[(int)TransportType.Car, (int)TerrainFeatures.GravelRoad] = 0;
            costs[(int)TransportType.Car, (int)TerrainFeatures.PavedRoad] = 0;

            costs[(int)TransportType.OffRoad, (int)TerrainFeatures.None] = 0;
            costs[(int)TransportType.OffRoad, (int)TerrainFeatures.FootPath] = 0;
            costs[(int)TransportType.OffRoad, (int)TerrainFeatures.WheelPath] = 0;
            costs[(int)TransportType.OffRoad, (int)TerrainFeatures.GravelRoad] = 0;
            costs[(int)TransportType.OffRoad, (int)TerrainFeatures.PavedRoad] = 0;

        }


        /*   public override byte CostByATV
           {
               get { return 4; }
           }

           public override byte CostByCar
           {
               get { return 10; }
           }

           public override byte CostByFoot
           {
               get { return 4; }
           }*/

        /*   public override byte Cost(TransportType transport, TerrainFeatures feature)
           {
               return costs[(int)transport, (int) feature];
           }*/

        public override string Name
        {
            get { return "Water"; }
        }

    }
}

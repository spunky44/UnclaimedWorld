using System;
using System.Collections.Generic;
using System.Text;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// DELETE THIS???
    /// </summary>
    public class PlainsType: SurfaceType
    {
        private static PlainsType instance;
        public static PlainsType Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new PlainsType();
                    return instance;
                }
            }
        }

        private PlainsType() 
        {
            // this range of costs should be converted into a float speed factor that makes sense!
            // let's use a range of 1-5... 

             costs = new byte[3, 6];

             costs[(int)TransportType.Foot, (int)TerrainFeatures.None] = 3;
             costs[(int)TransportType.Foot, (int)TerrainFeatures.FootPath] = 3; // 2; TODO: converrt to discomfort cost
             costs[(int)TransportType.Foot, (int)TerrainFeatures.WheelPath] = 3; 
             costs[(int)TransportType.Foot, (int)TerrainFeatures.GravelRoad] = 3;
             costs[(int)TransportType.Foot, (int)TerrainFeatures.PavedRoad] = 3;
             costs[(int)TransportType.Foot, (int)TerrainFeatures.Obstacle] = 5;

             costs[(int)TransportType.Car, (int)TerrainFeatures.None] = 5;
             costs[(int)TransportType.Car, (int)TerrainFeatures.FootPath] = 5;
             costs[(int)TransportType.Car, (int)TerrainFeatures.WheelPath] = 4;
             costs[(int)TransportType.Car, (int)TerrainFeatures.GravelRoad] = 2;
             costs[(int)TransportType.Car, (int)TerrainFeatures.PavedRoad] = 1;
             costs[(int)TransportType.Car, (int)TerrainFeatures.Obstacle] = 8;

             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.None] = 4;
             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.FootPath] = 4;
             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.WheelPath] = 3;
             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.GravelRoad] = 2;
             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.PavedRoad] = 2;
             costs[(int)TransportType.OffRoad, (int)TerrainFeatures.Obstacle] = 6;


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
            get { return "Plains"; }
        }

    }
}

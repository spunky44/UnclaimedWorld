using System;
using System.Collections.Generic;
using System.Text;

namespace UWGame.SimSide.Maps
{

    public abstract class SurfaceType
    {        
      /*  public abstract byte CostByFoot { get; }
        public abstract byte CostByCar { get; }
        public abstract byte CostByATV { get; }*/
        protected byte[,] costs;

        public virtual byte Cost(TransportType transport, TerrainFeatures feature)
        {
            if (transport == TransportType.Air)
            {
                return 1; // or 3...?
            }
            else
            {
                return costs[(int)transport, (int)feature];
            }
        }


        /// <summary>
        /// this converts the byte value on the terrain map to a float factor for use in movement code and evaluator travel time comparisons.
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public float MovementFactor(TransportType transport, TerrainFeatures feature) //byte cost)
        {
            // terrain is always in the range 1 - 5...
            //1 : 0.5
            //3 : 0.9
            //5 : 1.3 
            return Convert.ToSingle(Cost(transport, feature) - 1) * 0.2f + 0.5f;

        }

        public float MovementFactor(byte cost)
        {
            // terrain is always in the range 1 - 5...
            //1 : 0.5
            //3 : 0.9
            //5 : 1.3 
            return Convert.ToSingle(cost - 1) * 0.2f + 0.5f;

        }

        public abstract string Name { get; }


        public enum TransportType : ulong
        {
            Foot = 0, Car = 1, OffRoad = 2, Air = 3  
        };

        /// <summary>
        /// Obstacle is for any obstacle that is not big enough to actually block the way completely (small trees for now).
        /// TODO: Move this to data files...
        /// </summary>
        public enum TerrainFeatures: ulong { None = 0, FootPath = 1, WheelPath = 2, GravelRoad = 3, PavedRoad = 4, Obstacle = 5 };
    }
}

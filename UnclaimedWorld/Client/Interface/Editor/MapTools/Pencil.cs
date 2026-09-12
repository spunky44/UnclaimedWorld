using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public class Pencil: MapTool, IHasAlphaOption
    {

        public AlphaSetting AlphaSetting
        {
            get;
            private set;
        }



        public Pencil()
        {          
            AlphaSetting = new AlphaSetting(1f);
        }

        public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
        {
            List<SubTileAndChange> result = new List<SubTileAndChange>();

          //  float changeInValue;

           
            float alpha = AlphaSetting.Value;

            result.Add(new SubTileAndChange(tilePos, alpha));

            /*
            int minX, minY, maxX, maxY;
            GetSubtileBounds(tilePos, (int)(Radius / MapManager.subTileSize), out minX, out minY, out maxX, out maxY);


            for (ushort x = (ushort)minX; x < maxX; x++)
            {
                for (ushort y = (ushort)minY; y < maxY; y++)
                {
                    SubtilePos currentPos = new SubtilePos(x, y);
                    float distance = Common.DistanceOctile(tilePos, currentPos);
                    if (distance <= Radius)
                    {
                        float changeInValue;                        
                            
                        changeInValue = Alpha;                        

                        result.Add(new SubTileAndChange(currentPos, changeInValue));
                    }
                }
            }*/

            return result;

        }
    }
}

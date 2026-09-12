using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public class Eraser : MapTool, IHasRadiusOption, IHasAlphaOption
    {
         /// <summary>
        /// don't share option (settings)
        /// </summary>     
        public RadiusSetting RadiusSetting
        {
            get;
            private set;
        }

        public AlphaSetting AlphaSetting
        {
            get;
            private set;
        }

        public Eraser()
        {
            RadiusSetting = new RadiusSetting(60f);
            AlphaSetting = new AlphaSetting(60f);
        }

        public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
        {
            List<SubTileAndChange> result = new List<SubTileAndChange>();

            float radius = RadiusSetting.Value;
            float alpha = AlphaSetting.Value;
              
            int minX, minY, maxX, maxY;
            GetSubtileBounds(tilePos, (int)(radius / MapManager.subTileSize), out minX, out minY, out maxX, out maxY);
            
            for (ushort x = (ushort)minX; x < maxX; x++)
            {
                for (ushort y = (ushort)minY; y < maxY; y++)
                {
                    SubtilePos currentPos = new SubtilePos(x, y);
                   // float distance = Common.DistanceOctile(tilePos, currentPos);
                    float distance = Common.Distance(MapManager.SubTileToWorldPos3(tilePos), MapManager.SubTileToWorldPos3(currentPos)); // DistanceOctile(tilePos, currentPos);
                 
                    if (distance <= radius)
                    {
                        float changeInValue;

                        changeInValue = -alpha;

                        result.Add(new SubTileAndChange(currentPos, changeInValue));
                    }
                }
            }

            return result;

        }

    }
}

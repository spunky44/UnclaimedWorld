using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public class Paintbrush : MapTool, IHasRadiusOption, IHasAlphaOption
    {
        public float HardCenterRadius;

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

        public Paintbrush()
        {
            RadiusSetting = new RadiusSetting(60f);
            AlphaSetting = new AlphaSetting(1f);
        }

        /// <summary>
        /// the caller can combine these results to tile terrain...
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
        {
            List<SubTileAndChange> result = new List<SubTileAndChange>();

            float radius = RadiusSetting.Value;
            float alpha = AlphaSetting.Value;


            int minX, minY, maxX, maxY;
            GetSubtileBounds(tilePos, (int)(radius / MapManager.subTileSize), out minX, out minY, out maxX, out maxY);

            float softEdgeSize = radius - HardCenterRadius;

            for (ushort x = (ushort)minX; x < maxX; x++)
            {
                for (ushort y = (ushort)minY; y < maxY; y++)
                {
                    SubtilePos currentPos = new SubtilePos(x, y);
                    float distance = Common.Distance(MapManager.SubTileToWorldPos3(tilePos), MapManager.SubTileToWorldPos3(currentPos)); // DistanceOctile(tilePos, currentPos);
                    if (distance <= radius)
                    {
                        float changeInValue;
                        if (distance > HardCenterRadius)
                        {
                            changeInValue = (distance - HardCenterRadius) / softEdgeSize; // MathHelper.lerp
                            changeInValue *= alpha;
                            changeInValue = Common.Clamp(changeInValue, 0f, 1f);
                        }
                        else
                        {
                            changeInValue = alpha;
                        }

                        result.Add(new SubTileAndChange(currentPos, changeInValue));
                    }     
                }                
            }

            return result;

        }

    }
}

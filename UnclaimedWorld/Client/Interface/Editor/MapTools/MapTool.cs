using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    /// <summary>
    /// Soil:
    /// it should be possible to paint on subtiles too! Near the water the resolution is higher in Terrain!
    /// 
    /// The tools should not share option values. So eraser can have different radius than brush.
    /// 
    /// Resources, Entities: Paint on tiles only
    /// </summary>
    public abstract class MapTool
    {
      
        public virtual List<SubTileAndChange> GetAffectedSubtiles(SubtilePos subtilePos) { return null; }

        public virtual List<TileAndChange> GetAffectedTiles(TilePos subtilePos) { return null; }


        public struct SubTileAndChange
        {
            public SubtilePos SubtilePos;

            /// <summary>
            /// -1 - 1
            /// </summary>
            public float Change;

            public SubTileAndChange(SubtilePos pos, float change)
            {
                this.SubtilePos = pos;
                this.Change = change;
            }
        }




        public struct TileAndChange
        {
            public TilePos TilePos;

            /// <summary>
            /// -1 - 1
            /// </summary>
            public float Change;
        }

        protected void GetSubtileBounds(SubtilePos pos, int radius, out int minX, out int minY, out int maxX, out int maxY)
        {
            int mapWidth = The.Map.mapSubtileWidth;
            int mapHeight = The.Map.mapSubtileHeight;


            minX = Math.Max(0, pos.X - radius);
            minY = Math.Max(0, pos.Y - radius);


            maxX = Math.Min(mapWidth - 1, pos.X + radius);
            maxY = Math.Min(mapHeight - 1, pos.Y + radius);
        }

    }
}

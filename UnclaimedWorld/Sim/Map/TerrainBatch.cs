using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Vegetation;
using UWGame.ClientSide.Map;

namespace UWGame.SimSide.Maps //TODO DECOUPLE -- move this to client
{
    public class TerrainBatch : IComparable
    {
        public List<VertexMultitextured> terrainVerticesList;
        VertexMultitextured[] terrainVerticesArray;

        public List<short> terrainIndicesList;
        short[] terrainIndicesArray;

        //public VertexMultitextured[] terrainVertices;
        public List<RenderedTerrainType> terrainInBatch = new List<RenderedTerrainType>();

        public bool RenderAsRocks = false;

        public TerrainBatch()
        {
            terrainVerticesList = new List<VertexMultitextured>();

            terrainIndicesList = new List<short>();

            //terrainVertices = new VertexMultitextured[(noOfVerticesHorizontal) * (noOfVerticesVertical)];

            //terrainVertices = terrainVerticesList.toa
        }

        public VertexMultitextured[] TerrainVerticesArray
        {
            get 
            {
                if (terrainVerticesArray == null)
                {
                    terrainVerticesArray = terrainVerticesList.ToArray();
                }

                return terrainVerticesArray;
            }
        }

        public short[] TerrainIndicesArray
        {
            get 
            {
                if (terrainIndicesArray == null)
                {
                    terrainIndicesArray = terrainIndicesList.ToArray();
                }

                return terrainIndicesArray;
            }
        }

        #region IComparable Members
        public int CompareTo(object obj)
        {
            TerrainBatch comparable = obj as TerrainBatch;
            return (int)(terrainInBatch[0].RenderOrder - comparable.terrainInBatch[0].RenderOrder);
        }
        #endregion

        /*  public TerrainBatch(int noOfVerticesHorizontal, int noOfVerticesVertical)
          {
              terrainVertices = new VertexMultitextured[(noOfVerticesHorizontal) * (noOfVerticesVertical)];

              //terrainVertices = terrainVerticesList.toa
          }
          */
    }
}

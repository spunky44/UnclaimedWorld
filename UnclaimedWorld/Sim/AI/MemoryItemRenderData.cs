using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide;
using UWGame.SimSide;

namespace UWGame.ClientSide 
{
    /*
    public class MemoryItemRenderData
    {
        //protected FeatureQuad quad; // = new FeatureQuad();

        protected List<FeatureQuad> quad;

       // private static Queue<MemoryItemRenderData> freeMemoryItemRenderData = new Queue<MemoryItemRenderData>();
        private static Pool<MemoryItemRenderData> memoryItemRenderDataPool = new Pool<MemoryItemRenderData>(60);

        public static MemoryItemRenderData Get(List<FeatureQuad> quad)
        {
            MemoryItemRenderData memoryItemRenderData = memoryItemRenderDataPool.Get();

            memoryItemRenderData.Init(quad);

            return memoryItemRenderData;
        }

        public void Retire()
        {

            memoryItemRenderDataPool.Retire(this);
        }

        public void Init(List<FeatureQuad> quad)
        {
            this.quad = quad;
        }



        public void CopyQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, ref int index)
        {
            foreach (FeatureQuad q in quad)
            {
                q.CopyQuadToVertexBuffer(featureVertices, index);

                index++;
            }
        }
    }*/
}

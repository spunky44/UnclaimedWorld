using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Vegetation //TODO DECOUPLE -- move part of this class to client namespace
{
    public enum RenderPerlinNoise { None, ChannelRed, ChannelGreen, ChannelBlue}
   // public enum InvertNoise {Yes, No }

    /// <summary>
    /// how does this class manage to serialize correctly..?
    /// </summary>
    public abstract class RenderedTerrainType: IGameData, IComparable
    {
        public float NoiseScaling = 1f;

        public string TextureName;

        private bool invertNoise = false;

        public int RenderOrder;

        public static int HighestOrder = 0;

        public bool IsBaseTerrain = false;

        public string Name
        {
            get;
            set;
        }

        public string KeyName
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }
        /// <summary>
        /// multiplies with sharpness
        /// </summary>
        public bool InvertNoise
        {
            set 
            {
                if (value) // == InvertNoise.Yes)
                {
                    renderPerlinNoiseSharpness = -1f * Math.Abs(renderPerlinNoiseSharpness);
                }
                else
                {
                    renderPerlinNoiseSharpness = Math.Abs(renderPerlinNoiseSharpness);
                }
                invertNoise = value;
            }
            get
            {
                return invertNoise;
            }

        }

        private RenderPerlinNoise renderWithPerlinNoise;
        public RenderPerlinNoise RenderWithPerlinNoise // = RenderPerlinNoise.None;
        {
            set
            {
                switch (value)
                {
                    case RenderPerlinNoise.None:
                        RenderPerlinNoiseSharpness = 0f;
                        break;
                }

                renderWithPerlinNoise = value;
            }
            get { return renderWithPerlinNoise; }
        }

        //private int RenderPerlinNoiseChannel;
        private float renderPerlinNoiseSharpness = 1f;
        public float RenderPerlinNoiseSharpness
        {
            get { return renderPerlinNoiseSharpness; }
            set
            {
                if (value != 0f && renderWithPerlinNoise == RenderPerlinNoise.None)
                {
                    throw new Exception("Sharpness must be 0 when RenderWithPerlinNoise = None is set.");
                }
                renderPerlinNoiseSharpness = value;
            }
        }

        public int GetPerlinNoiseChannel()
        {
            switch (renderWithPerlinNoise)
            {
                case RenderPerlinNoise.ChannelRed:
                    return 0;
                case RenderPerlinNoise.ChannelGreen:
                    return 1;
                case RenderPerlinNoise.ChannelBlue:
                    return 2;               
                default: return 0;
            }
        }

        public void Initialize() { }

        public void PreInitValidate(ref List<string> errors)
        { }
        public void PostInitValidate(ref List<string> errors)
        { }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
        

        #region IComparable Members
        public int CompareTo(object obj)
        {
            RenderedTerrainType comparable = obj as RenderedTerrainType;
            return (int)(RenderOrder - comparable.RenderOrder);
        }
        #endregion

   /*     public int GetPerlinNoiseTexture()
        {
            switch (renderWithPerlinNoise)
            {
                case RenderPerlinNoise.ChannelRed:
                    return 0;
                case RenderPerlinNoise.ChannelGreen:
                    return 0;
                case RenderPerlinNoise.ChannelBlue:
                    return 0;
                default: return 0;
            }
        }*/

    }
}

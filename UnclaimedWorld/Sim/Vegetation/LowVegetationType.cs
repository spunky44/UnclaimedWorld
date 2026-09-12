using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Maps.MapEditor;
using System.Xml.Serialization;
namespace UWGame.SimSide.Vegetation
{
    public class LowVegetationType : RenderedTerrainType //, IPlantType //, IDataType
    {
        /// <summary>
        /// 0: flat and firm
        /// 1: maximum rugged
        /// TODO: create a Sim class for this DECOUPLE
        /// </summary>
        public float MoveFactor = 0f;
        


        public Color DryTint;
        public Color LushTint;

       // public bool IsAquatic = false;
        public bool CanGrowUnderWater = false;

        public string[] Crops;

        [XmlIgnore]
        public List<ResourceType> CropTypes;

        /// <summary>
        /// used to populate low vegetation with default crops in the same way as for trees
        /// not implemented. the terrain subdivision makes it a bit harder to control the resources...
        /// </summary>
      //  public Resource[] DefaultCrops;
        
        public LowVegetationType() { }

        public LowVegetationType(string keyName)
        {
            this.KeyName = keyName;
        }


     /*   public new void Initialize()
        {
            if (CropTypes != null)
            {

                foreach (ResourceType cropType in CropTypes)
                {
                    GameData.Instance.AddToProductionChain(cropType); //, this);
                }
            }

        }*/


        public void PostLoadContentInitialize()
        {
            RenderOrder = RenderedTerrainType.HighestOrder;
            RenderedTerrainType.HighestOrder++;

            if (Crops != null)
            {
                foreach (var item in Crops)
                {
                    CropTypes.Add(GameData.Instance.AllResourceTypes[item]);
                }

            }

           /* if (CropTypes != null)
            {
                foreach (ResourceType cropType in CropTypes)
                {
                    GameData.Instance.AddToProductionChain(cropType); //, this);
                }
            }*/
        }


    }
}

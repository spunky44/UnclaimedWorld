using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Buildings;

namespace UWGame.SimSide.Resources
{
    public class CropType
    {
       
      
      /*  public float CropBulkGrowthPerDayMean;
        public float CropBulkGrowthPerDayStandardDeviation;
        */

        public float DetectionPulsingDuration = 2000f; 
        /// <summary>
        /// by how much can a single crop item grow each day?
        /// </summary>
        public float CropItemGrowthPerDay;

        /// <summary>
        /// how fast do crops ripen? 
        /// </summary>
        public float? RipeSpeed;


        public float MaxSizeShareOfWholePlant; // MaxUnHarvestedCropForMaturePlant;


        public ClientSide.Renderables.StateModifier? TreeSpriteFlag;

        public float BulkLimitToShowFlag = 0f;


        public void Initialize()
        {
        }
    

        /// <summary>
        /// datapoints that give a factor for production from the plant's age
        /// 1 = mature??? and old is 2,3...?
        /// </summary>
        public Vector2[] AgeProduction; 


      
    }
}

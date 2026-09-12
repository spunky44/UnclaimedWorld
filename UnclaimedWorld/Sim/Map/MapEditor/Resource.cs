using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Maps.MapEditor
{
    public class Resource
    {
        public string KeyName;

        /// <summary>
        ///  only filled in editor mode
        /// </summary>
        [XmlIgnore]
        public ResourceType ResourceType;

        /// <summary>
        /// these values are added by the map designer and modify (multiply) the default resource numbers for the type.
        /// default is 100 (%)
        /// </summary>
        public int? Modifier;

        /// <summary>
        /// gets multiplied on the default std dev
        /// default is 1
        /// </summary>
     //   public float? StandardDeviationModifier;


       // public Vector2 MinAndMax;

        /// <summary>
        /// absolute numbers. for Crops, the numbers apply to the full grown entity (tree)
        /// </summary>
        public int? MinResourceItems;
        public int? MaxResourceItems;
     //   public float? Min;
      //  public float? Max;

        [XmlIgnore]
        public float? AbsoluteMeanItems;

        [XmlIgnore]
        public float? AbsoluteStandardDeviation;

        public Resource()
        {

        }

        public Resource(ResourceType resourceType)
        {
            this.ResourceType = resourceType;
            this.KeyName = resourceType.KeyName;

        }
       


        public void PostDataCompleteInitialize()
        {
            if (MinResourceItems.HasValue && MaxResourceItems.HasValue)
            {
                Common.GetNormalDistributionFromMinMaxValues(MinResourceItems.Value, MaxResourceItems.Value, out AbsoluteMeanItems, out AbsoluteStandardDeviation);
            }

            if (The.Sim.Mode == Sim.EngineMode.Edit)
            {
                // only needed in editor mode
                ResourceType = GameData.Instance.AllResourceTypes[KeyName];
            }
        }

        public bool ShouldSerializeModifier() 
        {
            return Modifier != null;
        }

       /* public bool ShouldSerializeStandardDeviationModifier()
        {
            return StandardDeviationModifier != null;
        }*/


        public bool ShouldSerializeMinResourceItems()
        {
            return MinResourceItems != null;
        }

        public bool ShouldSerializeMaxResourceItems()
        {
            return MaxResourceItems != null;
        }


        public override string ToString()
        {
            return KeyName;
        }

       /* public bool ShouldSerializeAbsoluteMean()
        {
            return AbsoluteMean != null;
        }

        public bool ShouldSerializeAbsoluteSpread()
        {
            return AbsoluteStandardDeviation != null;
        }*/
    }

    
}

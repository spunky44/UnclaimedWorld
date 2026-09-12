using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes
{
    /// <summary>
    /// this class describes which tools can be applied to a process, and their effect.
    /// </summary>
    public class Tool
    {
        // designer shorthand: we use string types to make data entry easier...
        // (it is ok to fill in both, even if they overlap)
        public string UsesToolKeyName;
        public string UsesToolTag;
        
       
        /// <summary>
        /// process-specific productivity
        /// </summary>
        public float? ProductivityFactor; // = 1f;

       /// <summary>
       /// process-specific tool degradation
       /// </summary>
        public float? DegradePerSecondOfUse; // = 0f;


        /// <summary>
        /// the list of tool types that can be used
        /// </summary>
        [XmlIgnore]
        public List<EntityType> ToolEntityTypes = new List<EntityType>();

      //  public EntityType EntityType;

        public bool ShouldSerializeProductivityFactor()
        {
            return ProductivityFactor != null;
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (!string.IsNullOrEmpty(UsesToolKeyName))
            {
                EntityType entityType;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, UsesToolKeyName, GameData.Instance.AllEntityTypes, out entityType);                
            }
        }

        public void PostDataCompleteInitialize()
        {
            if (!string.IsNullOrEmpty(UsesToolTag))
            {              
                ToolEntityTypes.AddRange(GameData.Instance.ToolsByTag[UsesToolTag]);
            }

            if (!string.IsNullOrEmpty(UsesToolKeyName))
            {
                EntityType toolType = GameData.Instance.AllEntityTypes[UsesToolKeyName]; 
                if (!ToolEntityTypes.Contains(toolType))
                {
                    ToolEntityTypes.Add(toolType);
                }
            }
           
           
        }


       

        public void PreInitValidate(ref List<string> errors)
        {
           
            EntityType.ValidateRequiredValue(ref errors, "Degrade", DegradePerSecondOfUse.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "Productivity", ProductivityFactor.HasValue);

            /*if (!string.IsNullOrEmpty(Tag) && !string.IsNullOrEmpty(ToolKeyName))
            {
                EntityType.CreateValidationError(ref errors, "ToolKeyName and Tag cannot both be specified as Tool.");
            }*/
        }

        public void PostInitValidate(ref List<string> errors)
        {
            if (ToolEntityTypes != null)
            {
                foreach (var item in ToolEntityTypes)
                {
                    if (item.ToolType == null)
                    {
                        EntityType.CreateValidationError(ref errors, "The entity '" + item.KeyName + "' cannot be assigned as a tool when its ToolType is null.");
                       

                    }
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class ToolContainerType : ContainerType
    {
       
        /// <summary>
        /// A container holding items that are currently replenishing the entity
        /// shared between ReplenishContainerType, VehicleType and AgentStorageType (for robots)
        /// </summary>
        public RequiresReplenishType RequiresReplenishType;

        /// <summary>
        /// holds production outputs. not meant for other storage.
        /// </summary>
        public ItemStorageType ProductionOutputStorageType;

        public override Container CreateContainer(Entity parent)
        {
            return new ToolContainer(parent);
        }

        public override RequiresReplenishType GetRequiresReplenishType()
        {
            return RequiresReplenishType;
        }

        public override Dictionary<EntityType, Processes.ProcessType> GetReplenishProcesses()
        {
            if (RequiresReplenishType != null)
            {
                return RequiresReplenishType.ReplenishProcesses;
            }
            else return null;
        }

        public override float GetOutputStorageCapacity()
        {
            return ProductionOutputStorageType.GetTotalCapacity();          
        }

        public override float? FullStatePercentage
        {
            get
            {
                return ProductionOutputStorageType.FullStatePercentage;
            }
        }

        public override float? HalfFullStatePercentage
        {
            get
            {
                return ProductionOutputStorageType.HalfFullStatePercentage;
            }
        }

        public override bool HasOutputStorage
        {
            get
            {
                return ProductionOutputStorageType != null;
            }
        }

        public ToolContainerType()
        {
            
        }

      /*  public ToolContainerType(string condition1, float capacity1, Storage.Conditions? condition2 = null, 
                                float? capacity2 = null, Storage.Conditions? condition3 = null, float? capacity3 = null)
        {
            
        }*/

        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            if (CanBeEnteredByTags != null)
            {
                EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags should not be specified because this container type has no doors and cannot be entered.");
                
            }
        }
       

        public override void PostLoadContentInitialize(EntityType parent)
        {
            base.PostLoadContentInitialize(parent);

            if (RequiresReplenishType != null)
            {
                RequiresReplenishType.PostLoadContentInitialize(parent);
            }
        }
    }
}

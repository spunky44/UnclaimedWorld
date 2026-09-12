using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class ReplenishContainerType : ContainerType
    {
        /// <summary>
        /// A container holding items that are currently replenishing the entity
        /// the contained class RequiresReplenishType is shared between ReplenishContainerType, VehicleType and AgentStorageType (for robots)
        /// 
        /// the items are stored in ReplenishContainer
        /// </summary>
        public RequiresReplenishType RequiresReplenishType;


        public override Container CreateContainer(Entity parent)
        {
            return new ReplenishContainer(parent);
        }

        public override RequiresReplenishType GetRequiresReplenishType()
        {
            return RequiresReplenishType;
        }

        public override Dictionary<EntityType, Processes.ProcessType> GetReplenishProcesses()
        {
            return RequiresReplenishType.ReplenishProcesses;     
        }

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

            RequiresReplenishType.PostLoadContentInitialize(parent);
        }
    }
}

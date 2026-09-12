using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class WorkshopContainerType : ContainerType, IHasItemStorageType
    {
        public ItemStorageType ItemStorageType { get; set; }
        

        public string DefaultStorageSettings;

        [XmlIgnore]
        public DefaultStorageSettings DefaultStorageSettingsFinal
        {
            get;
            private set;
        }

        public bool AllowStockpiling = true;

        public bool AllowsStockpiling
        {
            get
            {
                return AllowStockpiling;
            }
        }

        public override Container CreateContainer(Entity parent)
        {
            return new WorkshopContainer(parent);
        }

        /// <summary>
        /// optional
        /// A container holding items that are currently replenishing the entity
        /// shared between ReplenishContainerType, VehicleType and AgentStorageType (for robots)
        /// </summary>
        public RequiresReplenishType RequiresReplenishType;

        /// <summary>
        /// holds production outputs. not meant for other storage.
        /// </summary>
        public ItemStorageType ProductionOutputStorageType;

        public override RequiresReplenishType GetRequiresReplenishType()
        {
            return RequiresReplenishType;
        }

        public override float GetOutputStorageCapacity()
        {
            return ProductionOutputStorageType.GetTotalCapacity();
        }

        public override Dictionary<EntityType, Processes.ProcessType> GetReplenishProcesses()
        {
            if (RequiresReplenishType != null)
            {
                return RequiresReplenishType.ReplenishProcesses;
            }
            else return null;
        }

        public override bool HasOutputStorage
        {
            get
            {
                return ProductionOutputStorageType != null;
            }
        }

        public override float? FullStatePercentage
        {
            get
            {
                return ItemStorageType.FullStatePercentage;
            }
        }

        public override float? HalfFullStatePercentage
        {
            get
            {
                return ItemStorageType.HalfFullStatePercentage;
            }
        }

        public WorkshopContainerType()
        {
            
        }

        public WorkshopContainerType(string condition1, float capacity1, string condition2 = null,
                                float? capacity2 = null, string condition3 = null, float? capacity3 = null)
        {
            ItemStorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
        }

        public override DefaultStorageSettings GetDefaultStorageSettings()
        {
            return DefaultStorageSettingsFinal;
        }

        public override void Initialize()
        {
            base.Initialize();

           
            ItemStorageType.Initialize(); // mandatory
        

            if (DefaultStorageSettings != null)
                DefaultStorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];

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

            if (RequiresReplenishType != null)
            {
                RequiresReplenishType.PostLoadContentInitialize(parent);
            }
        }
    }
}

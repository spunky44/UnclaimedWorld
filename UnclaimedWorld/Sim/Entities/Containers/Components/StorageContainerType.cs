using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class StorageContainerType : ContainerType, IHasItemStorageType
    {
        public ItemStorageType ItemStorageType { get; set; }

        public string DefaultStorageSettings;


        public override Container CreateContainer(Entity parent)
        {
            return new StorageContainer(parent);
        }

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

        public StorageContainerType()
        { }

        public StorageContainerType(float isolatedCapacity)
        {
            ItemStorageType = new ItemStorageType(isolatedCapacity);
        }

        public StorageContainerType(string condition1, float capacity1,
            string condition2 = null, float? capacity2 = null,
            string condition3 = null, float? capacity3 = null)
        {

            ItemStorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
                       
        }

        public override DefaultStorageSettings GetDefaultStorageSettings()
        {
            return DefaultStorageSettingsFinal;
        }

        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            if (CanBeEnteredByTags != null)
            {
                EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags should not be specified because this container type has no doors and cannot be entered.");

            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ItemStorageType != null)
            {
                ItemStorageType.Initialize();
            }

            if (DefaultStorageSettings != null)
                DefaultStorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];

        }
    }
}

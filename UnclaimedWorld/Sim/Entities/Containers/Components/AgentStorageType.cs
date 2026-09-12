using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class AgentStorageType : ContainerType
    {
        public ItemStorageType ItemStorageType;

        public ItemStorageType EquipmentStorageType;

        /// <summary>
        /// we want to treat items carried in the stomach differently.
        /// </summary>
        public ItemStorageType StomachStorageType;

      


       // public float? EquipmentStorage = 0f;

        public AgentStorageType()
        { }

      /*  public AgentStorageType(float isolatedCapacity, float? equipmentStorage = null)
        {
            ItemStorageType = new Entities.ItemStorageType(isolatedCapacity);

            
            if (equipmentStorage > 0f)
            {
                EquipmentStorageType = new Entities.ItemStorageType(equipmentStorage.Value);
            }
        }*/

        public override Container CreateContainer(Entity parent)
        {
            return new AgentStorage(parent);
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ItemStorageType != null)
            {
                ItemStorageType.Initialize();
            }

            if (EquipmentStorageType != null)
            {
                EquipmentStorageType.Initialize();
            }
        }

        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            if (CanBeEnteredByTags != null)
            {
                EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags should not be specified because this container type has no doors and cannot be entered.");

            }
        }
    }
}

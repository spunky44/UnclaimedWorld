using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// not meant for dedicated storage, used by fish traps to hold output
    /// </summary>
    public class OtherContainerType : ContainerType
    {       
     
        /// <summary>
        /// holds production outputs. not meant for other storage.
        /// </summary>
        public ItemStorageType StorageType;

        public override Container CreateContainer(Entity parent)
        {
            return new OtherContainer(parent);
        }

       

        public override float? FullStatePercentage
        {
            get
            {
                return StorageType.FullStatePercentage;
            }
        }

        public override float? HalfFullStatePercentage
        {
            get
            {
                return StorageType.HalfFullStatePercentage;
            }
        }


        public OtherContainerType(float isolatedCapacity)
        {
            StorageType = new ItemStorageType(isolatedCapacity);
        }

        public OtherContainerType(string condition1, float capacity1,
            string condition2 = null, float? capacity2 = null,
            string condition3 = null, float? capacity3 = null)
        {

            StorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
                       
        }

        public OtherContainerType()
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

        }
    }
}

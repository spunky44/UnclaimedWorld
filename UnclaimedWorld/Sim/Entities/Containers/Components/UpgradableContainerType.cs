using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// the workshop building can be customized by choosing a tool, but the upgrade is the tool, not this entity.
    /// Cannot be entered by people.  
    /// </summary>
    public class UpgradableContainerType : ContainerType
    {
        public ItemStorageType ItemStorageType;
        
        public string UpgradesProfile;

        [XmlIgnore]
        public UpgradeProfile UpgradesProfileFinal;

       
        public string DefaultStorageSettings;

        [XmlIgnore]
        public DefaultStorageSettings DefaultStorageSettingsFinal
        {
            get;
            private set;
        }

        public override Container CreateContainer(Entity parent)
        {
            return new UpgradableContainer(parent);
        }


        public override bool CanBeUpgraded
        {
            get
            {
                return UpgradesProfileFinal != null;
            }
        }

        public override List<UpgradeCategory> GetUpgradeOptions() 
        {
            if (UpgradesProfileFinal != null)
            {
                return UpgradesProfileFinal.UpgradeCategoriesFinal;
            }

            return null;
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

        public UpgradableContainerType()
        {
            
        }

        public UpgradableContainerType(string condition1, float capacity1, string condition2 = null,
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

        }

        public override void PostDataCompleteInitialize(EntityType parent)
        {
            if (UpgradesProfile != null)
            {
                UpgradesProfileFinal = GameData.Instance.AllUpgradeProfiles[UpgradesProfile];

            }

            base.PostDataCompleteInitialize(parent);
        }
    }
}

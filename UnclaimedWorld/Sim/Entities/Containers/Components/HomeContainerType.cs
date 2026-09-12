using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class HomeContainerType : ContainerType, IHasItemStorageType
    {
        public ItemStorageType ItemStorageType { get; set; }

        public Vector2[] Doors;
        public bool HasRallyPointInCourtyard = false;

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
            return new HomeContainer(parent);
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

        public HomeContainerType() //float isolatedCapacity)
        {
            //ItemStorageType = new Entities.ItemStorageType(isolatedCapacity);
        }

        public HomeContainerType(string condition1, float capacity1, string condition2 = null,
                                float? capacity2 = null, string condition3 = null, float? capacity3 = null)
        {
            ItemStorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
        }


        public override bool CanBeUpgraded
        {
            get
            {
                return UpgradesProfileFinal != null;
            }
        }

        public override List<UpgradeCategory> GetUpgradeOptions() //Dictionary<UpgradeCategory, List<EntityType>> GetUpgradeOptions()
        {
            if (UpgradesProfileFinal != null)
            {
                return UpgradesProfileFinal.UpgradeCategoriesFinal;
            }

            return null;
        }

        public override DefaultStorageSettings GetDefaultStorageSettings()
        {
            return DefaultStorageSettingsFinal;
        }

        public override Vector2[] GetDoors()
        {
            return Doors;
        }

        public override bool GetHasCourtyard()
        {
            return HasRallyPointInCourtyard;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Entities
{
    public class Upgrader
    {
       // public string UpgradeCategory;
        public string[] UpgradeCategories;

        public StateModifier? SpriteModifier;

        [XmlIgnore]
        public List<UpgradeCategory> UpgradeCategoryFinal;
       
       /* [XmlIgnore]
        public UpgradeCategory UpgradeCategoryFinal;
        */

        /// <summary>
        /// these settings will be applied to the host when upgraded. Used by workshops.
        /// </summary>
        public string StorageSettings;

        [XmlIgnore]
        public DefaultStorageSettings StorageSettingsFinal;


        public string[] Effects;

        [XmlIgnore]
        public List<EffectProfileType> EffectsFinal;

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            EntityType.ValidateRequiredValue(ref listOfErrors, "UpgradeCategories", UpgradeCategories != null);

            if (UpgradeCategories != null)
            {
                foreach (var item in UpgradeCategories)
                {
                    UpgradeCategory cat;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item, GameData.Instance.AllUpgradeCategories, out cat);
                }
            }

            if (StorageSettings != null)
            {
                DefaultStorageSettings set;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, StorageSettings, GameData.Instance.AllDefaultStorageSettings, out set);
         
            }
        }

      /*  public void PostDataCompleteValidate(EntityType parent, ref List<string> listOfErrors)
        {
            if (!GameData.Instance.ProcessYieldsThisOutput.ContainsKey(parent))
            {

            }

        }*/

        public void PostDataCompleteInitialize(EntityType parent)
        {

            if (Effects != null)
            {
                EffectsFinal = new List<EffectProfileType>();
                foreach (var item in Effects)
                {
                    EffectsFinal.Add(GameData.Instance.AllEffectProfileTypes[item]);
                }
            }

            UpgradeCategoryFinal = new List<UpgradeCategory>();
            foreach (var item in UpgradeCategories)
            {
                UpgradeCategoryFinal.Add(GameData.Instance.AllUpgradeCategories[item]);
                Common.AddToMultiList(GameData.Instance.UpgraderEntityTypesByUpgradeCategory, GameData.Instance.AllUpgradeCategories[item], parent);

            }

            if (StorageSettings != null)
            {
                StorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[StorageSettings];

            }
            /*
            UpgradeCategoryFinal = GameData.Instance.AllUpgradeCategories[UpgradeCategory];
            Common.AddToMultiList(GameData.Instance.UpgraderEntityTypesByUpgradeCategory, UpgradeCategoryFinal, parent);
             */
        }

    }
}

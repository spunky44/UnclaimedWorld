using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;


namespace UWGame.SimSide.Items
{
    [XmlRoot("Item")]
    public class ItemType : IXmlSerializable /*,IHasCategory<ItemCategory>, IHasIcon *///, IDataType //
    {
        /// <summary>
        /// Must be unique
        /// </summary>        
        public string KeyName { get; set; }

      /*  {
            get { return keyName; }[XmlElement(ElementName = "KeyName", Order = 0)]
            set { keyName = value; }
        }*/

        public string Name {get; set; }

        public string Abbreviation;

        public string[] Tags;

        public string[] RequiredStorageTags;
        public string[] RequiredStorageTypes;

        [XmlIgnore]
        public HashSet<EntityType> RequiredStorageTypesFinal;


      //  public ItemType CircularTest;

        /// <summary>
        /// when true, the item will get assigned a bulk that makes sense for the way it came into being - examples:
        /// junk/organic matter: will get same bulk as degraded item
        /// carcass: will get same bulk as living entity
        /// tree log: will get same bulk as tree
        /// body part (like turtle shell): will get the bulk that is the result of the FractionOfInput and the input bulk defined in the process type (NB meat shouldn't have unique bulk because it is easily divided into seperate items (meat chunks))
        /// (IMPORTANT!!! July 2013 "HACK": Process inputs with no maximum bulk set will NEVER BE MOVED for production no matter what their actual bulk is (the process will always take place at the item's location)! However, agents WILL haul the item if player sets a stock zone or a storage building where it belongs.)
        /// Therefore, limit the number of items that have no maximum bulk, because for example a process with two no maximum bulk items cannot be carried out. Also, don't use on small items because it will seem weird that they can't be carried to another process site.
        /// </summary>
        public bool HasNoMaximumBulk = false;

       
        /// <summary>
        /// if filled, this sets a maximum bulk for the item type.
        /// the bulk of an instance can be lower if it has been converted/extracted (could we also fill this in as a guarantee that the bulk of a new item of this type will never go over this value?)
        /// </summary>
        public float? MaximumBulk;

        public bool ShouldSerializeMaximumBulk()
        {
            return MaximumBulk != null;
        }


        public string[] EffectsWhenEquipped;

        [XmlIgnore]
        public List<EffectProfileType> FinalEffectsWhenEquipped;

      
        /// <summary>       
        /// </summary>
        public enum TaskType
        {
            LongerJourneys, UnspecifiedHunting,
            Scouting, // not in use
            Hauling, // not in use
            PatrolOrAttack, NightActivities,
            Examining
        } 


        /// <summary>
        /// defines how appropriate the equipment/tool/weapon is for the given task 
        /// in code, map these to floats in range 0-1
        /// if not defined, use the normal value as default
        ///      
        /// </summary>
        public enum AppropriateLevel { None , Minor, Normal , Best }


        public bool UseGearAtAnyDistanceFromExpedition = false;

        /// <summary>
        /// Hints to the human AI to tell it what equipment/tool/weapon is more sutiable or appropirate to use for different tasks.
        /// Default is None now!     
        /// </summary>
        public SerializableDictionary<TaskType, AppropriateLevel> TaskAppropriateLevels = new SerializableDictionary<TaskType, AppropriateLevel>();
        public float GetTaskAppropriateLevel(TaskType type)
        {
            AppropriateLevel level;
            if (TaskAppropriateLevels.TryGetValue(type, out level))
            {
                switch (level)
                {
                    case AppropriateLevel.None:
                        return 0f;
                    case AppropriateLevel.Minor:
                        return 0.25f;
                    case AppropriateLevel.Normal:
                        return 0.5f;
                    case AppropriateLevel.Best:
                        return 1f;
                }                
            }

            return 0f; // 0.49f; // ?? // 0.75f;
        }

        public float GetTaskAppropriateLevel(List<ItemType.TaskType> taskTypes)
        {
            float highestLevel = 0f;
            float currentLevel = 0f;
            foreach (var item in taskTypes)
            {
                currentLevel = GetTaskAppropriateLevel(item);

                if (currentLevel > highestLevel)
                {
                    highestLevel = currentLevel;
                }

            }

            return highestLevel;
        }

        public FoodType FoodType;

        // what type of attacks are possible using me
        public WeaponType WeaponType;

        // how shall I move if thrown (TODO extend later to prescribe all forms of movement??? MLo)
        public LocomotorType LocomotorType;

        /// <summary>
        /// items with this property set can not be produced with the slider, they have to be hunted...
        /// </summary>
        public CarcassType CarcassType;

        public FuelType FuelType;

        public AmmunitionType AmmunitionType;

       // public bool MayStoreInHome = false;
       // public bool StoreOutside = false;

        public string AttachedObjectRenderableType;

        public string AttachorTagToMountOn;

        /// <summary>
        /// if filled, no unarmed attacks using this body part will be possible while an item is attached/mounted
        /// </summary>
        public string AttachesToBodyPart;

        public AnimModifier[] AnimStatesWhenAttached;

        public ItemType() { }

        public ItemType(string keyName)
        {
            this.KeyName = keyName;  
            // default categaory:
          //  Parent.Category = GameData.Instance.AllItemCategories["itemcat:Misc"];

        }

        public string GetAbbreviation()
        {
            if (!string.IsNullOrEmpty(Abbreviation))
            {
                return Abbreviation;
            }
            else return Name.Substring(0, Common.Min(3, Name.Length));
        }


       /* public void SetCanBeAPart()
        {

        }*/

        public bool CanBeAPart
        {
            get
            {
                return canBeAPart;
            }
            set
            {
                canBeAPart = true;
            }
        }

        private bool canBeAPart = false;
      /*  public bool CanBeAPart()
        {


        }*/

        public void Initialize()
        {
            // set a default icon sprite:
          /*  if (string.IsNullOrEmpty(IconSpriteName))
            {
                if (it.Renderable.RenderAsBillboardType != null && Parent.Renderable.RenderAsBillboardType.Length > 0)
                {
                    it.ItemType.IconSpriteName = it.Renderable.RenderAsBillboardType[0].SpriteName + "_icon";
                }
            }
            */
           /* if (string.IsNullOrEmpty(IconSpriteName))
            {
                IconSpriteName = SpriteName + "_icon";
            }*/


            if (WeaponType != null
                && !TaskAppropriateLevels.ContainsKey(TaskType.LongerJourneys))
            {
                TaskAppropriateLevels[TaskType.LongerJourneys] = AppropriateLevel.Normal; // set a default for all weapons
            }


            if (FoodType != null)
            {
                FoodType.Initialize();
            }

           
        }

        public void PostDataCompleteInitialize()
        {
            if (FoodType != null)
            {
                FoodType.PostDataCompleteInitialize();
            }

            if (EffectsWhenEquipped != null)
            {
                FinalEffectsWhenEquipped = new List<EffectProfileType>();
                foreach (var item in EffectsWhenEquipped)
                {
                    FinalEffectsWhenEquipped.Add(GameData.Instance.AllEffectProfileTypes[item]);
                }
            }
        }

        public void PostLoadContentInitialize()
        {
            if (WeaponType != null)
            {
                WeaponType.PostLoadContentInitialize();
            }

           

            GameData.ResolveEntityTypeTags(ref RequiredStorageTypesFinal, RequiredStorageTags, RequiredStorageTypes, GameData.Instance.ContainersByTag); // .GeneralTags);

        }


        public void Validate(ref List<string> errors)
        {
            if (HasNoMaximumBulk && MaximumBulk.HasValue)
            {
                EntityType.CreateValidationError(ref errors, "Bulk must not be defined for the item type when using unique instance bulk.");            
            }
        }

       

        public override string ToString()
        {
            return Name;
        }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ItemType)) //typeof(ItemCategory))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
            
            /*new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<EntityCategory, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllItemCategories[s]
                    },
                    new CustomXmlSerializer.XmlTypeMapping<ItemType, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : new ItemType(){ KeyName = s } //GameData.Instance.AllItemTypes[s]  
                    }, 
                    new CustomXmlSerializer.XmlTypeMapping<DegradeType, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllDegradeTypes[s]
                    },
                    new CustomXmlSerializer.XmlTypeMapping<FoodNutrientProfile, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllFoodNutrientProfiles[s]
                    },*/
                    // PARTS:
                /*    new CustomXmlSerializer.XmlTypeMapping<Dictionary<ItemType, int>, KVP<string, int>[]>()  
                    {
                        GetterMethod = t => {
                            if (t == null) return null;
                            var d = new KVP<string, int>[t.Count];
                            int index = 0;
                            foreach (KeyValuePair<ItemType, int> kvp in t)
	                        {
                        	    d[index] = new KVP<string, int>(kvp.Key.KeyName, kvp.Value);	
                                index++;
	                        }
                            return d;                            
                        },
                        SetterMethod = s => { 
                            if (s == null) return null; 
                            var d = new Dictionary<ItemType, int>(); 
                            foreach (var el in s) 
                            { 
                                d[new ItemType(){ KeyName = el.Key }] = el.Value; //  watch out for cycles! (other ItemTypes) - create a special ItemType with empty Name to identify and replace in a second pass
                            } 
                            return d; 
                        }
                    }  */
                
        };

        #endregion

        public enum HauledItemValues
        {
            LowValue,
            Normal,
            MostValuable
        }

        /// <summary>
        /// indicates tthe value of the item. This will make the AI better equipped to prioritze between hauling rifles or stones first.
        /// </summary>
        public HauledItemValues HauledItemValue = HauledItemValues.Normal;
        
        /// <summary>
        /// 
        /// Add it as an component to sligtly change the score when scoring hauling
        /// Placed inside the score function       
        /// </summary>
        /// <returns></returns>
        public float GetHauledItemValueModifier()
        {
            switch( HauledItemValue )
            {
                case HauledItemValues.LowValue:
                    return 0.25f;
                case HauledItemValues.Normal:
                    return 0.5f;
                case HauledItemValues.MostValuable:
                    return 1f;
                default:
                    throw new Exception("No priority set");
            }
        }
    }

    

  /*  public class FoodType
    {
        public bool IsVegetablesForHumans = false;
        public bool IsGrainForHumans = false;
        public bool IsMeatForHumans = false;

        public bool EdibleWhenRaw;
        public bool EdibleWhenCooked;
    }*/
}

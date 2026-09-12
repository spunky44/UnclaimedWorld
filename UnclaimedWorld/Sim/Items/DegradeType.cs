using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using System.Diagnostics;

namespace UWGame.SimSide.Items
{
    [DebuggerDisplay("{KeyName}")]
    public class DegradeType: IGameData
    {
     //   private static Dictionary<StorageCondition, float> ConditionTemperatures;

        public const float WaterBoilingPoint = 373f;

        public const float RoomTemperature = 293f;
        public const float WaterFreezingPoint = 273f;

        /// <summary>
        /// 40 degrees
        /// </summary>
        public const float Hot = 313f;
        
        public const float Refrigeration = 278f;
        public const float DeepFreeze = 255f;


        public string Name { get; set; }
        public string KeyName { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Description;

        /// <summary>
        /// damage per day, cumulative with the other damage types.
        /// currently (2015) outside ambient moisture variation in the game comes from the moisture.png bitmap in the map folder. (It also has effect on graphics, making ground darker). Moisture in containers varies a lot according to container type.
        /// </summary>
        public Vector2[] MoistureDamage;

        /// <summary>
        /// 
        /// currently (2015) no temp variation OUTSIDE in the game. weather temp is hardcoded to 288 Kelvin = 15 C. Temp in containers varies a lot according to container type.
        /// </summary>
        public Vector2[] TemperatureDamage;

        /// <summary>
        /// each vector is a data point: X: 0 is darkness, 1 is bright sunshine.  Y: damage amount per day
        /// currently (2015) outside, ambient light varies between night and day
        /// </summary>
        public Vector2[] LightDamage;

        /// <summary>
        /// optimization - used when storing the item
        /// </summary>
      //  [XmlIgnore]
      //  public Storage.Conditions OptimalStorage;

        /// <summary>
        /// optimization - used when deciding where to store the item.
        /// Sorted by decreasing optimality. Cost of powered refrigeration is factored in too.
        /// </summary>
        [XmlIgnore]
        public List<StorageDamageEstimation> ConditionDamages = new List<StorageDamageEstimation>();

        /// <summary>
        /// used for display on the entity type tooltip
        /// </summary>
        [XmlIgnore]
        public List<StorageDuration> StorageDurations = new List<StorageDuration>(); 
       
      /*  [XmlIgnore]
        public Dictionary<StorageCondition, List<StorageDuration>> StorageDurations = new Dictionary<StorageCondition,List<StorageDuration>>(); //Dictionary<bool, StorageDuration>
        */

        /// <summary>
        /// optimization - used when storing the item
        /// </summary>
        //[XmlIgnore]
        //public SortedDictionary<Storage.Conditions, float> ConditionDamages = new SortedDictionary<Storage.Conditions, float>();

       /* static DegradeType()
        {
            ConditionTemperatures = new Dictionary<Storage.Conditions, float>();

            ConditionTemperatures.Add(Storage.Conditions.Airconditioning, RoomTemperature);
            ConditionTemperatures.Add(Storage.Conditions.Freezer, DeepFreeze);
            ConditionTemperatures.Add(Storage.Conditions.Refrigerator, Refrigeration);
            ConditionTemperatures.Add(Storage.Conditions.EarthCooled, Storage.EarthCooledTemperature);
            
            //ConditionTemperatures.Add(Storage.Conditions., DeepFreeze);

        }*/

        public void Initialize()
        {
           // Dictionary<Storage.Conditions, float> conditionDamages = new Dictionary<Storage.Conditions,float>();
            ConditionDamages.Clear();

            // same damage for all condition types:
            float lightDamage = Common.GetInterpolatedFunctionValue(0f, LightDamage);
            float moistureDamage = Common.GetInterpolatedFunctionValue(0f, MoistureDamage);
            float damage;

           // float minDamage = 100000;
           // Storage.Conditions bestCondition = Storage.Conditions.Freezer;

            foreach (var kvp in GameData.Instance.AllStorageConditions) // ConditionTemperatures)
            {
                if (kvp.Key != "exposed" && kvp.Key != "isolated" && kvp.Value.FixedTemperature.HasValue)
                {
                    float temperatureDamage = Common.GetInterpolatedFunctionValue(kvp.Value.FixedTemperature.Value, TemperatureDamage);
                    damage = (float)(temperatureDamage + lightDamage + moistureDamage);

                    //ConditionDamages.Add(kvp.Key, damage);
                    ConditionDamages.Add(new StorageDamageEstimation() { Condition = kvp.Value, Damage = damage });
                }

               /* if (damage < minDamage)
                {
                    minDamage = damage;
                   // bestCondition = kvp.Key;
                }*/
            }

            float exposedDamage = 
                  Math.Max(Common.GetInterpolatedFunctionValue(WaterFreezingPoint + 36, TemperatureDamage),
                        Common.GetInterpolatedFunctionValue(WaterFreezingPoint - 18, TemperatureDamage))
                        + Common.GetInterpolatedFunctionValue(0.8f, MoistureDamage)
                        + Common.GetInterpolatedFunctionValue(1f, LightDamage);

            // to compensate for cost, only store under refrigeration if damage is 80 % or less:
            exposedDamage *= 0.8f;

           // ConditionDamages.Add(Storage.Conditions.Exposed, exposedDamage);
            ConditionDamages.Add(new StorageDamageEstimation(){ 
                Condition = GameData.Instance.AllStorageConditions["exposed"]/* Storage.Conditions.Exposed*/, Damage = exposedDamage});

            float isolatedDamage =
                  Math.Max(Common.GetInterpolatedFunctionValue(WaterFreezingPoint + 28, TemperatureDamage),
                        Common.GetInterpolatedFunctionValue(WaterFreezingPoint - 2, TemperatureDamage))
                        + Common.GetInterpolatedFunctionValue(0f, MoistureDamage)
                        + Common.GetInterpolatedFunctionValue(0f, LightDamage);

            
            isolatedDamage *= 0.8f;

            //ConditionDamages.Add(Storage.Conditions.Isolated, isolatedDamage);
            ConditionDamages.Add(new StorageDamageEstimation(){ Condition = GameData.Instance.AllStorageConditions["isolated"] /*Storage.Conditions.Isolated*/, Damage = isolatedDamage});

            ConditionDamages.Sort(StorageDamageEstimation.CompareDamage);
                   
        }

        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) { }
        
        public void PostDataCompleteInitialize()
        {
           
            float outsideTemperature = GameData.Instance.Constants.MeanAmbientTemperature;

            // compute values for DegradeTypes * StorageConditions, with different parts flags (if the entity can be a part)
            // select the most important ones
            // store the combos in DegradeType

            foreach (var item in GameData.Instance.GUIConstants.StorageDurationToDisplay)  //.AllStorageConditions)
            {
               // StorageCondition storage = item.Value;
                StorageCondition storage = GameData.Instance.AllStorageConditions[item.StorageCondition];

                // bool isEnclosed;
                //bool isWeatherProof;
                ComputeDuration(outsideTemperature, 1f,  storage, item);

                /*
                ComputeDuration(outsideTemperature, storage, true);
                ComputeDuration(outsideTemperature, storage, false);*/
            }
        }

        private void ComputeDuration(float outsideTemperature, float lightLevel, StorageCondition storage, StorageDurationToDisplay toDisplay) //string name, bool isWeatherProof, bool alwaysDisplay, int priority)
        {
            float damage = NonLivingEntity.ComputeDegradeDamage(this, storage, toDisplay.IsStorageOfWeatherProofPart, /*isEnclosed,*/ outsideTemperature, lightLevel, true, 1); // one day's damage...

            float duration;

            if (damage > 0f)
            {
                duration = 1f / damage;
            }
            else
            {
                duration = 9999f; // infinite?
            }

            StorageDuration storageDuration = new StorageDuration();
            storageDuration.Duration = duration;
            storageDuration.StorageDurationToDisplay = toDisplay;
            storageDuration.DegradeType = this;
            storageDuration.StorageCondition = storage;
            storageDuration.ComputeSortOrder();

            this.StorageDurations.Add(storageDuration);

            //Common.AddToMultiList(this.StorageDurations, storage, storageDuration);
        }


        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

        public class StorageDamageEstimation
        {
            public float Damage;
            public StorageCondition Condition;


            public static int CompareDamage(StorageDamageEstimation x, StorageDamageEstimation y)
            {

                if (x.Damage == y.Damage)
                {
                    return 0;
                }
                else if (y.Damage > x.Damage)
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
                else return 1;

            }
        }


        
    }
}

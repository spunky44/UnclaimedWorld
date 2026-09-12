using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using System.ComponentModel;
using UWGame.SimSide.Items;
namespace UWGame.SimSide.AI.Needs
{
    /// <summary>
    /// these hook into other game systems, AI etc. to fulfill the needs..
    /// can we combine them with personality types?
    /// </summary>
  /*  public enum AINeedClass 
    { 
       // Sleep, // is satisified via EvaluateSleep      
        Companionship, // probably needs an evaluator that can join other agents in a process, like healing
        Activity,  // make a new evaluator for doing a static process...
        Consumable // ??
    }*/

    /*Serialization of Enumerations of Unsigned Long
           The XmlSerializer cannot be instantiated to serialize an enumeration if the following conditions are true: The enumeration is of type unsigned long (ulong in C#) and the enumeration 
        * contains any member with a value larger than 9,223,372,036,854,775,807. */
    public enum NeedTypeID : long
    {
        First = 0L,
        Invalid = long.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// keyname is not unique, so we maintain an ID
    /// </summary>
    public class NeedType: ILookUp<NeedType, NeedTypeID>
    {
        /// <summary>
        /// is not a unique key...
        /// </summary>
        public string KeyName;

       
        /// <summary>
        /// describe how to fulfill the need
        /// only fill in one of the below
        /// </summary>       
        public FoodNeedType FoodNeedType; // satisfied via EvaluateEat
        public SleepNeedType SleepNeedType;
        public ProcessNeedType ProcessNeedType;

       

        // composition - can be null:
        public PhysicalEffects PhysicalEffects;

      
     //   public ComfortEffects ComfortEffects;

      //  [XmlIgnore]
      //  public bool GivesComfortEffects;


        /// <summary>
        /// how 'much' do we need per game-day - can vary between individuals
        /// </summary>
        public NormalDistribution DecreasePerDay;

              

        public float LimitForDecreasedEnergy = 0f;

        /// <summary>
        /// when the value is below the limit, affect energy levels by the difference times this factor
        /// why not in PhyscialNeed too?
        /// </summary>
        public float DecreasedEnergyWeight = 0f;
     
        /// <summary>
        /// default: True
        /// </summary>
        public bool LevelVisibleToOtherAllegiances = true;
      

        [XmlIgnore]
        public float NormalizedDecreasedEnergyWeight;

        public NeedType()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();   
            }
        }


        public override string ToString()
        {
            if (FoodNeedType != null && FoodNeedType.FoodNutrientType != null)
            {
                return FoodNeedType.FoodNutrientType.Name;
            }
            else
            {
                return KeyName.ToString();
            }
        }

       
        public void Initialize()
        {

        }

        public void PostDataCompleteInitialize()
        {          
            if (FoodNeedType != null)
            {
                FoodNeedType.PostDataCompleteInitialize();
            }
       
            /*
            if (PhysicalNeedType != null)
            {
                PhysicalNeedType.PostDataCompleteInitialize();
            }*/
        }

        public void Validate(ref List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay", DecreasePerDay != null);

            if (DecreasePerDay != null)
            {
                EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay.Mean", DecreasePerDay.Mean != null);
                EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay.StandardDeviation", DecreasePerDay.StandardDeviation != null);
            }

        }

        //bool? givesComfortEffects = null;

        public bool GivesComfortEffects()
        {
            if (FoodNeedType != null && FoodNeedType.FoodNutrientType.SatisfiesComfort)
            {
                return true;
            }

            return false;

            /*
            if (givesComfortEffects == null)
            {
                givesComfortEffects = false;
                if (FoodNeedType != null)
                {
                    bool satisfies;
                   if (FoodNeedType.FoodNutrientType.SatisfiesComfort GameData.Instance.SatisfiesComfort.TryGetValue(FoodNeedType.FoodNutrientType, out satisfies) && satisfies == true)
                   {
                       givesComfortEffects = true;
                   }

                }
            }

            return givesComfortEffects.Value;*/
        }

        public float? GetEnergyFactor(float currentLevel)
        {
            if (LimitForDecreasedEnergy > 0f) // see if this need affects energy level
            {
                float energyFactor = 1f;

                if (currentLevel < LimitForDecreasedEnergy)
                {
                    float levelBelowLimit = LimitForDecreasedEnergy - currentLevel;

                    // when we are at the limit, energy factor should be 1. When we are at 0, energy factor should be 0.
                    energyFactor = 1f - (levelBelowLimit / LimitForDecreasedEnergy);


                    energyFactor = Common.Clamp(energyFactor, 0f, 1f);
                }

                return energyFactor * NormalizedDecreasedEnergyWeight;
            }
            else return null;
        }


        #region ILookup

        private NeedTypeID id = NeedTypeID.Invalid;
        static NeedTypeID IDCounter = NeedTypeID.First;

        [XmlIgnore]
        public NeedTypeID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        /// <summary>
        /// workaround, for XmlSerializer to serialize ID values which are not named in the enum type.
        /// </summary>
        [XmlElement("ID")]
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public long IDLong
        {
            get { return (long)ID; }
            set { ID = (NeedTypeID)value; }
        }


        public NeedTypeID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= NeedTypeID.Max)
            {
                throw new Exception("Astounding, NeedTypeID just exceeded 64 bits. Something seriously wrong has happened.");
            }
            return IDCounter;
        }

        public NeedTypeID SnapshotID(Snapshotter sn, NeedTypeID id)
        {
            return (NeedTypeID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != NeedTypeID.Invalid)
                LookUp<NeedType, NeedTypeID>.Add(ID, this);

            LookUp<NeedType, NeedTypeID>.SetPerformSnapshot(false);
        }

        public void SetInvalid()
        {
            id = NeedTypeID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<NeedType, NeedTypeID>.Remove(this);
        }

        void ILookUp<NeedType, NeedTypeID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = NeedTypeID.First;
        }

        void ILookUp<NeedType, NeedTypeID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<NeedType, NeedTypeID>.Create();
        }

        #endregion

       
    }
}

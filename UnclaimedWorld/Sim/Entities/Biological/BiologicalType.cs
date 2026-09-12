using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using GameStateManagement;
using UWGame.SimSide.Items;
using UWGame.SimSide.Resources;
using UWGame.Control;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.AI.Needs;

namespace UWGame.SimSide.Entities.Biological
{
   
    /// <summary>
    /// one instance of this class for each species  
    /// </summary>
    public class BiologicalType : IXmlSerializable
    {
   /*     public float? Size;
        public Vector3 PrimaryColor;
        public Vector3 SecondaryColor;
       */

        /// <summary>
        /// NEW: default if age, race and caste don't specify any
        /// </summary>
        public string ModelBasicTextureName;


        public Vector3? PrimaryColor;
        public Vector3? SecondaryColor;
        public Vector3? TertiaryColor;
        public Vector3? QuaternaryColor;


        public string OrderKey;

        [XmlIgnore]
        public BioOrderType OrderType;

        //public string OrderName;
        //public string OrderDescription;


     //   public SpeciesType[] SpeciesTypes;

        public string SpeciesPlural;

        #region Computed values that describe this type

        [XmlIgnore]
        public float? MinimumBulk = null;

        [XmlIgnore]
        public float MeanBulkOfAdultMember;

        [XmlIgnore]
        public float MaxBulkOfAdultMember;


        [XmlIgnore]
        public float MeanHitpointsOfAdultMember;
        

        [XmlIgnore]
        public CasteType AdultMemberCaste;

        [XmlIgnore]
        public AgeGroupType AdultMemberAgeGroup;

        #endregion

        public List<CasteType> Castes;


        /// <summary>
        /// can be null!
        /// 
        /// Races can breed
        /// 
        /// if the Race is sufficently different that it yields its own products, then it should be an EntityType instead
        /// </summary>
        public RaceType[] RaceTypes;


        //**********************************************
        #region Default BioProperties - All these values can be overridden in Cast, Age, Race and Order types, but here the species defaults are specified
        // we can set the types to nullables and then create a validation that requires it to be filled in
        //**********************************************

        /// <summary>
        /// hitpoints modifier. gets multiplied with Bulk to determine max hitpoints.
        /// </summary>
        public float ResilienceMean = 1f;
        public float ResilienceStandardDeviation = 0f;


        public bool IsNocturnal = false;// ??

        public double TimeOfDayToGoToSleep; //??

        /// <summary>
        /// placeholder - delete this
        /// </summary>
     //   public bool IsPredator = false;

        /// <summary>
        /// if true, will attack non-allegiance entities in range even if they are not predators
        /// </summary>
        public bool IsTerritorial = false; 


        // for later:
      //  public bool IsCannibalistic = false;
     //   public bool IsManEater = false;

       // public bool CanRun = false;

     
        /// <summary>
        /// 0-1
        /// how stealthy is the creature when trying not to be seen (like when hunting)
        /// NOTE that only humans have a 'sneak' skill (athletic) - this gets added
        /// </summary>
        public float? ActiveStealthRating;

        /// <summary>
        /// 0-1
        /// how stealthy is the creature when NOT trying to avoid being seen (like when idling)
        /// mostly used by prey animals?
        /// </summary>
        public float? PassiveStealthRating;

      
        /// <summary>
        /// the rate as percentage per day we replenish the oxygen/muscle energy
        /// </summary>
        public float? OxygenAndMuscleEnergyIncreaseRatePerDay;

        /// <summary>
        /// true if the creature should pick up its food before eating it, false if it should stay on the ground
        /// </summary>
        public bool HoldsFoodWhenEating = false;


        public ChanceToTakeStance[] EatingStances;

        [XmlIgnore]
        public List<ChanceToTakeStance> EatingStanceTypes; 

        // we can make them nullable and validate that they are filled in...

        /// <summary>
        /// how much room does the stomach have
        /// humans: 1/70
        /// snakes: 2/3
        /// </summary>
        public float? StomachSizeFractionOfEntityBulk;


        /// <summary>
        /// the rate as percentage per day that the stomach empties (content is 0 - 1)
        /// </summary>
        public float? StomachContentsDecreaseRatePerDay;

        /// <summary>
        /// how long does it take for an entity to eat its food?
        /// </summary>
        public float? TimeToConsumeFullMealInDays; // = 0.05f;


        /// <summary>
        /// 1: full regen is possible
        /// 0.5: max 50% of total wounds can be regained
        /// 0: no regen
        /// </summary>
        public float? MaxRegainLimit;

        public float? FractionOfMaxHitpointsGainedPerDay;

        public bool IsVermin = false;
       

        //*** tags and strings for data:

        /// <summary>
        /// extraction process types
        /// </summary>
        public string[] ExtractionProcessTypes;

     /*   /// <summary>
        /// food items with these tags can be extracted from before eating
        /// </summary>
        public string[] FoodItemTagsThatCanBeExtracted;  
      */

        /// <summary>
        /// consume process types. Anything that we can consume, we can also extract smaller items from... if they are too big to eat whole
        /// </summary>
        public string[] ConsumeProcessTypes;

        /// <summary>
        /// food items with one of these tags can be consumed by the entity
        /// </summary>
        public string[] FoodItemTagsThatCanBeConsumed;

        //***

        //*** final process types
        // could these be scrapped with optional inputs...?
        [XmlIgnore]
        public Dictionary<EntityType, HashSet<ProcessType>> ExtractionProcesses;
        [XmlIgnore]
        public Dictionary<EntityType, ProcessType> ConsumeProcesses;
        [XmlIgnore]
        public Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable;

        // ***

        #endregion


      

        public PalatableFood[] PalatableFood;

        public string Carcass;

        [XmlIgnore]
        public EntityType CarcassType;

        /*
        /// <summary>
        /// there is no reason to make this a resource. we only need an entity type.
        /// </summary>
        [XmlIgnore]
        public ResourceType CarcassType;
        */

        /// <summary>
        /// defines properties that may be overridden or interpolated in the caste/age/race subtypes of the entity
        /// (made this an array to avoid duplicating the keyname string in game data)
        /// </summary>       
        public BioPropertyType[] BioPropertyTypes;

        /// <summary>
        /// defines properties that may be overridden or interpolated in the caste/age/race subtypes of the entity
        /// </summary>
         [XmlIgnore]
        public Dictionary<string, BioPropertyType> bioPropertyTypes;

        [XmlIgnore]
        public float WorstCaseEnergyFactor;

        public void Validate(ref List<string> errors)
        {
           /* foreach (var item in SpeciesTypes)
            {
                item.Validate(ref errors);
            }*/

            foreach (CasteType caste in Castes)
            {
                caste.Validate(ref errors);
            }

            EntityType.ValidateRequiredValue(ref errors, "Oxygen recharge rate", OxygenAndMuscleEnergyIncreaseRatePerDay.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "Stealth rating", ActiveStealthRating.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "Stomach contents recharge rate", StomachContentsDecreaseRatePerDay.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "Stomach size", StomachSizeFractionOfEntityBulk.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "Time to consume full meal in days", TimeToConsumeFullMealInDays.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "MaxRegainLimit", MaxRegainLimit.HasValue);
            EntityType.ValidateRequiredValue(ref errors, "FractionOfMaxHitpointsGainedPerDay", FractionOfMaxHitpointsGainedPerDay.HasValue);

            if (MinimumBulk.HasValue && MinimumBulk.Value < GameData.Instance.Constants.MinimumAgentBulk)
            {
                EntityType.CreateValidationError(ref errors, string.Format("The minimum bulk has been calculated to a value {0} that is less than the minimum allowed bulk for an agent {1}.", 
                    MinimumBulk.Value, GameData.Instance.Constants.MinimumAgentBulk));
            }

        }

        public void PostDataCompleteInitialize()
        {
            foreach (CasteType caste in Castes)
            {
                caste.PostDataCompleteInitialize();
            }

           
        }

        public float GetMaxBulk()
        {
            return MaxBulkOfAdultMember;
        }

        public void Initialize(EntityType parent)
        {
          
            if (!string.IsNullOrEmpty(OrderKey))
            {
                OrderType = GameData.Instance.AllBioOrderTypes[OrderKey];
            }

            int casteNo = 1;
            foreach (CasteType caste in Castes)
            {
                caste.Initialize(casteNo);

                casteNo++;
            }

            if (RaceTypes != null)
            {
                int raceNo = 1;
                foreach (RaceType race in RaceTypes)
                {
                    race.Initialize(raceNo);

                    raceNo++;
                }
            }

            
            if (BioPropertyTypes != null)
            {
                bioPropertyTypes = new Dictionary<string, BioPropertyType>();
                foreach (var item in BioPropertyTypes)
                {
                    bioPropertyTypes.Add(item.KeyName, item);
                }
            }


            if (EatingStances != null)
            {
                EatingStanceTypes = new List<ChanceToTakeStance>();
                foreach (var item2 in EatingStances)
                {
                    item2.Initialize();
                    EatingStanceTypes.Add(item2);
                }
            }


            GetDefaultAdultMember();
            GetMinimumBulk(out MinimumBulk);
            GetAdultMeanBulk(parent);
           
        }

        public void PostLoadContentInitialize(EntityType parent)
        {
            if (Carcass != null)
            {
                CarcassType = GameData.Instance.AllEntityTypes[Carcass];
            }
            //CarcassType.PostLoadContentInitialize();

           // GameData.Instance.AddToProductionChain(CarcassType);

         
            InitConsumeProcesses(parent);

            InitExtractionProcesses(parent);

            InitExtractionResultsInConsumables(parent);
        }

        private void InitExtractionProcesses(EntityType parent)
        {
            ExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>();

            if (ExtractionProcessTypes != null)
            {                
                foreach (var item in ExtractionProcessTypes)
                {
                    ProcessType processType = GameData.Instance.AllProcessTypes[item];
                    processType.IsInnateExtractionProcess = true;

                    EntityType extractionInput = processType.InputsByType.First().Key; // assume the process only has one input!!

                    // organize the processes by input:
                    Common.AddToMultiList(ExtractionProcesses, 
                        extractionInput, processType);

                }
            }

        }

        private void InitExtractionResultsInConsumables(EntityType parent)
        {
            ComputeExtractionResultsInConsumables(
                ExtractionProcesses, 
                ConsumeProcesses, 
                ref ExtractionResultsInConsumable);
        }


        /// <summary>
        /// for the given collections of extraction processes and consume processes, this method computes whether an extraction from a product results in a consumable
        /// (this will be used when there are animals extracting food for their young (protein) that they cannot eat themselves)
        /// </summary>
        /// <param name="ExtractionProcesses"></param>
        /// <param name="ConsumeProcesses"></param>
        /// <param name="ExtractionResultsInConsumable"></param>
         public static void ComputeExtractionResultsInConsumables(
            Dictionary<EntityType, HashSet<ProcessType>> extractionProcesses,
            Dictionary<EntityType, ProcessType> consumeProcesses,
            ref Dictionary<EntityType, HashSet<ProcessType>> extractionResultsInConsumable)
        {
            if (extractionResultsInConsumable == null)
            {
                extractionResultsInConsumable = new Dictionary<EntityType,HashSet<ProcessType>>();
            }
            extractionResultsInConsumable.Clear();

            foreach (var item in extractionProcesses)
            {
                HashSet<ProcessType> processes = item.Value;
                
                foreach (var process in processes)
	            {
		            foreach (var output in process.Outputs)
	                {
		                if (consumeProcesses.ContainsKey(output.FinalEntityTypeToCreate))
                        {
                            Common.AddToMultiList(extractionResultsInConsumable, item.Key, process);
                        }
                    }

                }
            }
        }

        private void InitConsumeProcesses(EntityType parent)
        {
            // creates a process type for every entity type that we can consume...
            ConsumeProcesses = new Dictionary<EntityType, ProcessType>();

            List<EntityType> foodTypes = new List<EntityType>();

            if (FoodItemTagsThatCanBeConsumed != null) //!string.IsNullOrEmpty(UsesAmmoTag))
            {
                foreach (var tag in FoodItemTagsThatCanBeConsumed)
                {
                    foodTypes.AddRange(GameData.Instance.FoodByTag[tag]);
                }
            }

            foodTypes = foodTypes.Distinct().ToList();

            foreach (var item in foodTypes)
            {
                InitConsumeProcess(parent, 
                    item);
            }

           
            // Perhaps later? allow designer to override the default process with custom process types:
             /* if (ConsumeProcessTypes != null)
              {
                  foreach (var item in ConsumeProcessTypes)
                  {
                      ConsumeProcesses.Add(GameData.Instance.AllProcessTypes[item]);
                  }
              }*/
        }

        

        private void InitConsumeProcess(EntityType parent, EntityType item)
        {
            ProcessType eatingProcess = new ProcessType();
            eatingProcess.KeyName = parent.KeyName + "_defaultConsume_" + item.KeyName;

            eatingProcess.WorkOrTimeNeeded = new WorkOrTime()
            {
                DaysNeeded = TimeToConsumeFullMealInDays
            };

            // set input and output to the same type - the output of consume will be placed in the stomach and then destroyed right after:
            // if we convert a smaller bulk, there will still be some of the input left.
            eatingProcess.Inputs = new Input[] { 
                        new Input(){ Entity = item.KeyName, IsConsumed = true, 
                            Amount = new InputAmount()
                        {
                             NoOfItems = 1
                        }}};

            eatingProcess.Outputs = new Output[] {
                        new Output(){
                             EntityTypeToCreate = item.KeyName,
                              Amount = new OutputAmount()
                              {
                                   Bulk = new Bulk()
                                   {
                                        FractionOfInputBulk = 1f // should be overriden by the stomach capacity!
                                   }
                              }
                        }
                    };

            eatingProcess.AgentActionState = ClientSide.Renderables.AnimAction.Eating;
            eatingProcess.IsConsumeProcess = true;
            eatingProcess.UseWorkerEnergyAsProductionFactor = false;

            
            GameData.Instance.AllProcessTypes.Add(eatingProcess.KeyName, eatingProcess); // ??           
            GameData.InitializeComputerGeneratedData(eatingProcess);

            if (parent.HasStance() && EatingStanceTypes != null)
            {
                eatingProcess.StanceTypes = new Dictionary<StancesType, List<ChanceToTakeStance>>() { { parent.LocomotorType.StancesType, EatingStanceTypes } };
            }
            eatingProcess.PostLoadContentInitialize();// ??

            ConsumeProcesses.Add(item, eatingProcess);
           
        }

        public bool GetBioPropertyType(string propertyKey, out BioPropertyType bioPropertyType)
        {
            bioPropertyType = null;

            if (bioPropertyTypes != null)
            {
                return bioPropertyTypes.TryGetValue(propertyKey, out bioPropertyType);
            }
            else return false;
        }

        void GetMinimumBulk(out float? minimumBulk)
        {
            if (Castes.Count > 0)
            {

                float minBulk = BiologicalEntity.GetBulkFromWeight(Common.GetMinimumValue(Castes[0].WeightMean, Castes[0].WeightStandardDeviation));
                for (int i = 1; i < Castes.Count; i++)//Min bulk is already set to the first element in Castes
                {

                    float curBulk = BiologicalEntity.GetBulkFromWeight(Common.GetMinimumValue(Castes[i].WeightMean, Castes[i].WeightStandardDeviation));
                    if (curBulk < minBulk)
                    {
                        minBulk = curBulk;
                    }


                }
                minimumBulk =  minBulk;
                return;
            }
            minimumBulk = null;
            
        }

        public NeedType[] GetAdultNeedsAndWeight(out float weight)
        {
            weight = -1f;
            CasteType casteType = Castes.FirstOrDefault(c => c.Reproduction == Entities.Biological.Reproduction.Male);

            if (casteType == null)
            {
                casteType = Castes[0];
            }

            if (casteType != null)
            {
                AgeGroupType ageGroup = casteType.AgeGroupTypes.FirstOrDefault(a => a.AIAgeGroup == AIAgeGroup.Adult);

                if (ageGroup != null)
                {
                    weight = casteType.WeightMean;
                    return ageGroup.NeedTypes;
                }
            }

            return null;

        }

        public bool IsEatable(EntityType food)
        {
            return (food.ItemType != null
                && ((food.ItemType.FoodType != null
                && ConsumeProcesses.ContainsKey(food)))
                || ExtractionResultsInConsumable.ContainsKey(food));
        }

        private void GetDefaultAdultMember() //out CasteType casteType, out AgeGroupType ageGroupType)
        {
            AdultMemberCaste = Castes.FirstOrDefault(c => c.Reproduction == Entities.Biological.Reproduction.Male);

            if (AdultMemberCaste == null)
            {
                AdultMemberCaste = Castes[0];
            }


            if (AdultMemberCaste != null)
            {
                AdultMemberAgeGroup = AdultMemberCaste.AgeGroupTypes.FirstOrDefault(a => a.AIAgeGroup == AIAgeGroup.Adult);
            }
            else
            {
                AdultMemberAgeGroup = AdultMemberCaste.AgeGroupTypes[0]; // null;
            }

        }

        void GetAdultMeanBulk(EntityType parent)
        {
            
            MeanBulkOfAdultMember = BiologicalEntity.GetBulkFromWeight(AdultMemberCaste.WeightMean);

            /// three standard deviations account for about 99 percent of the people.
            MaxBulkOfAdultMember = BiologicalEntity.GetBulkFromWeight(AdultMemberCaste.WeightMean + 3.5f * AdultMemberCaste.WeightStandardDeviation);

            float resilience;
         
            // see if overridden:
            BioProperty bioProperty = BiologicalEntity.GetBioProperty("ResilienceMean", parent, AdultMemberCaste, null, AdultMemberAgeGroup.Edge, OrderType,
                ResilienceMean);

            resilience = bioProperty.NumberValue.Value;

            MeanHitpointsOfAdultMember = Body.Body.ComputeHitpoints(parent, MeanBulkOfAdultMember, resilience);
        }

        // TODO: create an enum for the designer to decide how bio properties should be computed (default: priority override / average / min / max)
        // perhaps encapsulate properties in BioProperty class
        // different bio property types have different interpolations available (bool, color, float..)

        #region Bio properties

              
        public string GetModelBasicTexture(CasteType caste, RaceType race, float age)
        {
            string casteTexture;
            int index;

            string ageTexture = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index).ModelBasicTextureName;

            if (!string.IsNullOrEmpty(ageTexture))
            {   // age group model takes precedence? no sex...
                return ageTexture;
            }
            else
            {
                if (race != null 
                    && (!string.IsNullOrEmpty(race.ModelBasicTextureName) || race.ModelBasicTextureNames != null))
                {
                    if (race.ModelBasicTextureNames != null)
                    {
                        List<string> textureNames = race.ModelBasicTextureNames.ToList();
                        if (!string.IsNullOrEmpty(race.ModelBasicTextureName))
                        {
                            textureNames.Add(race.ModelBasicTextureName);
                        }

                        return Common.GetRandomListMember(textureNames, The.Sim.GameplayRandomGenerator);
                    }
                    else 
                    {
                        return race.ModelBasicTextureName;
                    }
                }
                else
                {
                    casteTexture = caste.ModelBasicTextureName;
                    if (!string.IsNullOrEmpty(casteTexture))
                    {
                        return casteTexture;
                    }
                    else
                    {
                        return ModelBasicTextureName; // NEW  //null;      
                    }
                }
            }
        }


        public string GetModelName(CasteType caste, RaceType race, float age)
        {
            string casteModel;
            int index;

            string ageModel = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index).ModelName;

            if (!string.IsNullOrEmpty(ageModel))
            {   // age group model takes precedence? no sex...
                return ageModel;
            }
            else
            {
                
                if (race != null && !string.IsNullOrEmpty(race.ModelName))
                {
                    return race.ModelName;
                }
                else
                {
                    casteModel = caste.ModelName;
                    if (!string.IsNullOrEmpty(casteModel))
                    {
                        return casteModel;
                    }
                    else
                    {
                        return null;
                       /* if (this.PrimaryColor.HasValue)
                        {
                            return this.PrimaryColor.Value;
                        }
                        else return Vector3.One;*/
                    }
                }
            }
        }

        public float GetModelScale(CasteType caste, RaceType race, float age, float defaultScale)
        {
            float? casteScale;
            int index;

            float? ageScaleFraction = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index).ModelScaleFraction;

            float finalModelScaleFraction;
            if (ageScaleFraction.HasValue)
            {
                finalModelScaleFraction = ageScaleFraction.Value;
            }
            else
            {
                finalModelScaleFraction = 1.0f;
            }
            /*if (ageScale.HasValue)
            {   // age group model takes precedence? no sex...
                return ageScale.Value;nefen
            }
            else*/
            {
                if (race != null && race.ModelScale.HasValue)
                {
                    return race.ModelScale.Value * finalModelScaleFraction;
                }
                else
                {
                    casteScale = caste.ModelScale;
                    if (casteScale.HasValue)
                    {
                        return casteScale.Value * finalModelScaleFraction;
                    }
                    else
                    {
                        return defaultScale * finalModelScaleFraction; // null;                       
                    }
                }
            }
        }

        public Vector3 GetPrimaryColor(CasteType caste, RaceType race, float age) //int caste, int race, float age)
        {
            Vector3 color;
            Vector3? casteColor;
            int index;

            Vector3? ageGroupColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index).PrimaryColor;

            if (ageGroupColor.HasValue)
            {   // age group color takes precedence? Gray hair...
                color = ageGroupColor.Value;
            }
            else
            {               
               
                if (race != null && race.PrimaryColor.HasValue)
                {
                    return race.PrimaryColor.Value;
                }
                else
                {
                    casteColor = caste.PrimaryColor;
                    if (casteColor.HasValue)
                    {
                        return casteColor.Value;
                    }
                    else
                    {
                        if (this.PrimaryColor.HasValue)
                        {
                            return this.PrimaryColor.Value;
                        }
                        else return Vector3.One;
                    }
                }

            }

            return color;
        }

        public Vector3 GetSecondaryColor(CasteType caste, RaceType race, float age) //int caste, int race, float age) 
        {
            Vector3 color;
            Vector3 raceColor;  
            Vector3? casteColor;
            int index;
            Vector3? ageGroupColor = Common.GetStairStepIndex(age, /*Castes[caste]*/ caste.AgeGroupTypes, out index).SecondaryColor;
            if (ageGroupColor.HasValue)
            {   // age group color takes precedence? Gray hair...
                color = ageGroupColor.Value;
            }
            else
            {   // deserializes as empty list...
                if (race != null && race.SecondaryColorProbabilityEdges != null && race.SecondaryColorProbabilityEdges.Count > 0)
                {
                    float roll = (float)The.Sim.GameplayRandomGenerator.NextDouble("BiologicalType");
                    raceColor = Common.GetStairStepIndex(roll, /*RaceTypes[race]*/ race.SecondaryColorProbabilityEdges, out index).Color;

                    color = raceColor;

                }
                else
                {
                    casteColor = /*Castes[caste]*/ caste.SecondaryColor;
                    if (casteColor.HasValue)
                    {
                        return casteColor.Value;
                    }
                    else
                    {
                        if (this.SecondaryColor.HasValue)
                        {
                            return this.SecondaryColor.Value;
                        }
                        else return Vector3.One;
                    }
                }              
            }
            
            return color;
            
        }

        public Vector3 GetTertiaryColor(CasteType caste, RaceType race, float age) //int caste, int race, float age)
        {
            Vector3 color;
            Vector3? casteColor;
            int index;

            Vector3? ageGroupColor = Common.GetStairStepIndex(age, /*Castes[caste]*/ caste.AgeGroupTypes, out index).TertiaryColor;

            if (ageGroupColor.HasValue)
            {   // age group color takes precedence? Gray hair...
                color = ageGroupColor.Value;
            }
            else
            {
                
                if (race != null && race.TertiaryColor.HasValue)
                {
                    return race.TertiaryColor.Value;
                }
                else
                {
                    casteColor = /*Castes[caste]*/ caste.TertiaryColor;
                    if (casteColor.HasValue)
                    {
                        return casteColor.Value;
                    }
                    else
                    {
                        if (this.TertiaryColor.HasValue)
                        {
                            return this.TertiaryColor.Value;
                        }
                        else return Vector3.One;
                    }
                }

            }

            return color;
        }

        public Vector3 GetQuaternaryColor(CasteType caste, RaceType race, float age) 
        {
            Vector3 color;
            Vector3? casteColor;
            int index;

            Vector3? ageGroupColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index).QuaternaryColor;

            if (ageGroupColor.HasValue)
            {   // age group color takes precedence? Gray hair...
                color = ageGroupColor.Value;
            }
            else
            {
                if (race != null && race.QuaternaryColor.HasValue)
                {
                    return race.QuaternaryColor.Value;
                }
                else
                {
                    casteColor = /*Castes[caste]*/ caste.QuaternaryColor;
                    if (casteColor.HasValue)
                    {
                        return casteColor.Value;
                    }
                    else
                    {
                        if (this.QuaternaryColor.HasValue)
                        {
                            return this.QuaternaryColor.Value;
                        }
                        else return Vector3.One;
                    }
                }

            }

            return color;
        }

        #endregion

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(BiologicalType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };
        
        #endregion
    }

     
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.Control;
using UWGame.Client.Interface;
using UWGame.SimSide.Processes;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Biological
{
    public enum CauseOfDeath { Starvation, Wounds }
    public enum CauseOfUnconsciousness { Starvation, Wounds }

   // public enum Sex { Male, Female, None }
    public class BiologicalEntity: Component
    {
        public Entity Mate;
        public Entity BiologicalMother;
        public Entity BiologicalFather;
        public List<Entity> BiologicalChildren = new List<Entity>();

        /// <summary>
        /// is transferred from EntityData for use as fixed uniform...
        /// will override race, age and caste textures, if specified
        /// </summary>
        public string ModelTextureName;

        /// <summary>
        /// never null!!!
        /// </summary>
        public CasteType CasteType;

        private string casteKey; // for snapshot

        /// <summary>
        /// can be null!
        /// </summary>
        public RaceType RaceType;
        private string raceKey; // for snapshot
        
        public AgeGroup AgeGroup;

        /// <summary>
        /// physical attractiveness
        /// </summary>
        public float Appearance;

        /// <summary>
        /// physical endurance
        /// </summary>
        public float Endurance;


        #region cached BioProperties
             

        /// <summary>
        /// hitpoints modifier. gets multiplied with Bulk to determine max hitpoints.
        /// </summary>
        public float Resilience = 1f;

        /// <summary>
        /// the stealth or detection factor when the creature is trying to be stealthy
        /// </summary>
        public float ActiveStealthRating;

        /// <summary>
        /// the stealth or detection factor when the creature is not trying to be stealthy
        /// </summary>
        public float PassiveStealthRating;

        /// <summary>
        /// the rate as percentage per day we replenish the oxygen/muscle energy
        /// </summary>
        public float OxygenAndMuscleEnergyIncreaseRatePerDay;


        /// <summary>
        /// how much room does the stomach have
        /// humans: 1/70
        /// snakes: 2/3
        /// </summary>
        public float StomachSizeFractionOfEntityBulk;
        public float TimeToConsumeFullMealInDays;


        public float FractionOfMaxHitpointsGainedPerDay;
        public float MaxRegainLimit;

        /// <summary>
        /// will trigger attacks by entities that hunt vermin...
        /// </summary>
        public bool IsVermin = false;
       
        
        #endregion

        /// <summary>
        /// what can be consumed by the entity? - depends on its caste, age etc.
        /// Any item that can be consumed, can also be converted to a smaller bulk if needed to fit in the stomach
        /// </summary>
        public Dictionary<EntityType, ProcessType> ConsumeProcesses;

        /// <summary>
        /// extraction processes for the individual - depends on its caste, age etc.
        /// keyed by input entity: a carcass will have separate processes for extracting the organs, meat, etc...
        /// 
        /// Note: This collection of processes may not result in a consumable item for the entity!
        /// For instance, a worker may extract food with these processes that it would then give to the infants, but could not consume on its own.
        /// </summary>
        public Dictionary<EntityType, HashSet<ProcessType>> FoodExtractionProcesses; //


        /// <summary>
        /// indicates if we can consume the product that comes from extracting from the key type
        /// 
        /// compute this by running extraction processes on the chain of products, and testing each if it is consumable
        /// </summary>
        public Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable;

        //never used???
       // public FoodExtraction FoodExtraction;



      //  public delegate void FoodProcessesChangedHandler();
       // public event FoodProcessesChangedHandler FoodProcessesChanged;


        public float Height;
        public float Weight;

        public float AdultTargetHeight;
        public float AdultTargetWeight;

       // private float[] ageGroups = new float[] { 1.5f, 11f, 16f, 72f, 200f }; // upper end of range

        /// <summary>
        /// only used for non-human predators
        /// </summary>
       // public bool IsManEater = false;

        //public 

        // public Entity Parent;//moved to base

        // behaviours
        public Vector3? DwellingSpot;


        /// <summary>
        /// non-physical needs only?
        /// </summary>
        public AI.Needs.Needs Needs;

        // wounds and combat:

        /// <summary>
        /// placeholder for wound/blood system
        /// </summary>
      //  public float Hitpoints = 300;

        /// <summary>
        /// TODO: blood left in the body
        /// </summary>
        public float Blood;

        private float oxygenAndMuscleEnergy = 1f; // start at full
        /// <summary>
        /// 0 - 1
        /// oxygen + sugar? Represents oxygen and energy for muscles, as well as acid buildup. 
        /// Will fairly quickly grow back to 1.       
        /// </summary>
        public float OxygenAndMuscleEnergy
        {
            get
            {
                return oxygenAndMuscleEnergy;
            }
            set
            {
                if (value != oxygenAndMuscleEnergy)
                {
                    oxygenAndMuscleEnergy = value;

                    if (oxygenAndMuscleEnergy < GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
                    {
                        Parent.Renderable.SetAnimationStateFlag(ClientSide.Renderables.AnimModifier.Fatigued);
                    }
                    else if (oxygenAndMuscleEnergy > 1.1f * GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
                    {
                        Parent.Renderable.ClearAnimationStateFlag(ClientSide.Renderables.AnimModifier.Fatigued);
                    }
                }
            }

        }


        /// <summary>
        /// 0 - 1
        /// a factor that simulates slowly digesting the stomach contents, even though the food items in it have already been destroyed
        /// </summary>
        public float StomachContents
        {
            get;
            private set;
        }
              
        private float energyLevel;
        private bool energyLevelIsDirty = true;
        /// <summary>
        /// the energy level is a weighted function of all needs that determine work efficiency (concentration, physical ability) and many other things
        /// if a need is below the specified limit, it will reduce energy level by a specified weight.
        /// </summary>
        public float EnergyLevel
        {
            get
            {
                if (energyLevelIsDirty)
                {
                    energyLevel = ComputeEnergyLevel();
                    energyLevelIsDirty = false;
                }

                return energyLevel;
            }
        }


        public BiologicalEntity(Entity parent): base(parent)
        {
            
            // get random caste:
          /*  int caste;
            Common.GetStairStepIndex(Parent.EntityType.BiologicalType.Castes, out caste);
            CasteType = Parent.EntityType.BiologicalType.Castes[caste];

            // get random race:
            int race;
            Common.GetStairStepIndex(Parent.EntityType.BiologicalType.RaceTypes, out race);
            RaceType = Parent.EntityType.BiologicalType.RaceTypes[race];

            Endurance = (float)Common.RandomNormalDistribution(Globals.Instance.RandomPredictable, 0.6f, 0.1f);
            Appearance = (float)Common.RandomNormalDistribution(Globals.Instance.RandomPredictable, 0.5f, 0.1f);

            // determined at birth...
            AdultTargetHeight = (float)Common.RandomNormalDistribution(Globals.Instance.RandomPredictable, CasteType.HeightMean, CasteType.HeightStandardDeviation);
            AdultTargetWeight = (float)Common.RandomNormalDistribution(Globals.Instance.RandomPredictable, CasteType.WeightMean, CasteType.WeightStandardDeviation);
            */

            //CreateRegulators();

            AgeGroup = new AgeGroup(this);

            Needs = new AI.Needs.Needs(this);

            /* OLD: never used??
            EntityGroupID? ownedEntities = null;
            if (Parent.PersonEntity != null)
            {
                ownedEntities = Parent.PersonEntity.OwnedEntities.ID;
            }
            FoodExtraction = new FoodExtraction(Parent, ownedEntities);*/
        }

        public BiologicalEntity()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        /*   void FoodExtraction_FoodProcessesChanged()
           {
               if (Parent.EntityType.Person != null)
               {
                   Parent.PersonEntity.OwnedEntities.SetFoodDirty();
               }

           }*/

  

        /// <summary>
        /// can we consume the item directly, or can we extract food from it?
        /// for instance if the entity is a carcass, and we can convert a meat substance, we need to find out if there is still meat to convert. 
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public bool IsEatable(IKnownEntityData food)
        {
            return (food.EntityType.ItemType != null
                && ((food.EntityType.ItemType.FoodType != null
                && ConsumeProcesses.ContainsKey(food.EntityType)))
                || CanExtractFoodFromItem(food)); // ExtractionResultsInConsumable.ContainsKey(food);
        }

        public bool CanExtractFoodFromItem(IKnownEntityData food)
        {
            // first test the type:
            if (ExtractionResultsInConsumable.ContainsKey(food.EntityType))
            {
                // test the mass of the actual item to see that we would actually get a food item out:
                List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes = null;
                return GetConsumableFoodExtractionResults(food, false, ref listOfFoodTypes);
            }

            return false;
        }

        /// <summary>
        /// can the entity be consumed, or can a consumable item be extracted from it?
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        public bool IsEatable(EntityType food)
        {
            return food.IsEatable(ExtractionResultsInConsumable, 
                                  ConsumeProcesses);
        }


        public bool GetConsumableFoodExtractionResult(IKnownEntityData foodSource, ProcessType process, bool returnListOfFoodTypes, ref List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes)
        {
            List<Tuple<EntityType, float>> listOfProducts;

            // test the process to see if it would result in an item with bulk > 0:
            listOfProducts = process.TestProcess(foodSource);

            //now test that we can consume the outputs:
            if (listOfProducts != null && listOfProducts.Count > 0)
            {
                foreach (var product in listOfProducts)
                {
                    if (ConsumeProcesses.ContainsKey(product.Item1))
                    {
                        if (!returnListOfFoodTypes)
                        {
                            return true;
                        }
                        else
                        {
                            // only compile the list if needed:
                            Common.AddToList(ref listOfFoodTypes, new Tuple<ProcessType, EntityType, float>(process, product.Item1, product.Item2));
                        }
                    }
                }
            }

            if (listOfFoodTypes != null && listOfFoodTypes.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

                // test the process to see if it would result in an item with bulk > 0:
              /*  listOfProducts = process.TestProcess(foodSource);

                //now test the outputs again:
                if (listOfProducts != null && listOfProducts.Count > 0)
                {
                    foreach (var product in listOfProducts)
                    {
                        if (ConsumeProcesses.ContainsKey(product.Item1))
                        {
                            if (!returnListOfFoodTypes)
                            {
                                return true;
                            }
                            else
                            {
                                // only compile the list if needed:
                                Common.AddToList(ref listOfFoodTypes, product);
                            }
                        }
                    }
                }   */            
           

        }



        /// <summary>
        /// returns the types and amounts of consumable items that extracting from this food source would result in
        /// </summary>
        /// <param name="foodSource"></param>
        /// <param name="returnListOfFoodTypes"></param>
        /// <param name="listOfFoodTypes"></param>
        /// <returns></returns>
        public bool GetConsumableFoodExtractionResults(IKnownEntityData foodSource, bool returnListOfFoodTypes, ref List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes)
        {
            // we only test one step of extraction, then test the products for consumability:
            HashSet<ProcessType> processes = ExtractionResultsInConsumable[foodSource.EntityType]; 

            bool processHasConsumableOutput;
            // test all the extraction processes that we can use on this item:
            foreach (var process in processes)
            {
                processHasConsumableOutput = GetConsumableFoodExtractionResult(foodSource, process, returnListOfFoodTypes, ref listOfFoodTypes);

                if (returnListOfFoodTypes == false
                    && processHasConsumableOutput)
                {
                    return true;
                }               
            }

            if (listOfFoodTypes != null && listOfFoodTypes.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


       /* private List<Tuple<EntityType, float>> GetExtractableFoodAmounts(IKnownEntityData foodSource)
        {
            HashSet<ProcessType> processes = ExtractionResultsInConsumable[foodSource.EntityType]; // FoodExtractionProcesses[food.EntityType];

            ProcessType consumeProcess;
            List<Tuple<EntityType, float>> listOfProducts;
            foreach (var process in processes)
            {
                foreach (var output in process.Outputs)
                {
                    // check that we can consume this output type:
                    if (ConsumeProcesses.TryGetValue(output.FinalEntityType, out consumeProcess))
                    {
                        // test the process to see if it would result in an item with bulk > 0:
                        listOfProducts = process.TestProcess(foodSource);

                        // only return food items:
                        return listOfProducts;

                    }
                }
            }

        }*/




      /*  public bool IsExtractable(EntityType foodSource)
        {
            return ExtractionProcesses.ContainsKey(foodSource);               
                
        }*/

        private float GetNormalDistributedBioProperty(string meanPropertyKey, string stdDevPropertyKey, float meanDefaultValue, float stdDevDefaultValue)
        {
            float? mean, deviation;

            // see if overridden:
            BioProperty bioProperty = GetBioProperty(meanPropertyKey, meanDefaultValue);
            mean = bioProperty.NumberValue.Value;

            bioProperty = GetBioProperty(stdDevPropertyKey, stdDevDefaultValue);
            deviation = bioProperty.NumberValue.Value;


            return (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(mean.Value, deviation.Value);


        }

        private bool GetBioPropertyBoolean(string propertyKey, bool defaultValue)
        {
            BioProperty bioProperty = GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, null, defaultValue);

            return bioProperty.BoolValue.Value; // alwasy filled when we have a default

        }

        
        private float GetBioPropertyNumber(string propertyKey, float defaultValue)
        {
            BioProperty bioProperty = GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, defaultValue);

            return bioProperty.NumberValue.Value; // alwasy filled when we have a default

        }

        /// <summary>
        /// we can either cache this value if it will never change over the lifetime of an entity,
        /// or we can look it up each time (when Age changes?)
        /// </summary>
        /// <param name="propertyKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public BioProperty GetBioProperty(string propertyKey, float? defaultValue = null)
        {
            return GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, defaultValue);

        }

        public static BioProperty GetBioProperty(string propertyKey, EntityType entityType, CasteType caste, RaceType race, float age, BioOrderType order, float? defaultFloatValue = null, bool? defaultBoolValue = null)
        {            
            int index;

            BioProperty defaultValueProperty = null;
            if (defaultFloatValue.HasValue)
            {
                defaultValueProperty = new BioProperty() { NumberValue = defaultFloatValue.Value };
            }
            else if (defaultBoolValue.HasValue)
            {
                defaultValueProperty = new BioProperty() { BoolValue = defaultBoolValue.Value };
            }

            BioPropertyType bioPropertyType;
            if (!entityType.BiologicalType.GetBioPropertyType(propertyKey, out bioPropertyType))
            {
                return defaultValueProperty;                
            }

            AgeGroupType ageGroupType = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out index);

            BioProperty ageProperty = null, casteProperty = null, raceProperty = null, orderProperty = null;
            
            if (ageGroupType != null)
            {
                ageGroupType.GetBioPropertyValue(bioPropertyType, out ageProperty);
            }

            if (caste != null)
            {
                caste.GetBioPropertyValue(bioPropertyType, out casteProperty);
            }

            if (race != null)
            {
                race.GetBioPropertyValue(bioPropertyType, out raceProperty);
            }

            if (order != null)
            {
                order.GetBioPropertyValue(bioPropertyType, out orderProperty);
            }
          
            // combine the values as defined in the type:
            switch (bioPropertyType.InterpolateSetting)
            {
                case BioPropertyType.Interpolate.DefaultPriority:

                    return ageProperty ?? raceProperty ?? casteProperty ?? orderProperty ?? defaultValueProperty;

            }

            return null;
        }

        public static bool GetBioPropertyValue(BioPropertyType propertyKey, Dictionary<string, BioProperty> bioProperties, out BioProperty property)
        {
            property = null;

            if (bioProperties != null)
            {
                return (bioProperties.TryGetValue(propertyKey.KeyName, out property));
            }

            return false;
        }

        /// <summary>
        /// random age is weighted by the age spans
        /// </summary>
        /// <param name="age"></param>
        /// <param name="ageGroup"></param>
        public void SetAgePreInit(float? age = null, AIAgeGroup? ageGroup = null)
        {
            if (age.HasValue)
            {            
                AgeGroup.SetAge(age.Value);
            }
            else
            {               
                if (!ageGroup.HasValue)
                {
                    if (CasteType != null)
                    {
                        float maxEdge = CasteType.AgeGroupTypes[CasteType.AgeGroupTypes.Count - 1].Edge;
                        age = (float)The.Sim.GameplayRandomGenerator.NextDouble("BiologicalEntity") * maxEdge;

                        AgeGroup.SetAge(age.Value);

                        /*
                        int index;
                        ageGroup = Common.GetStairStepIndexComputeLastEdge(CasteType.AgeGroupTypes, out index, The.Sim.GameplayRandomGenerator).AIAgeGroup;*/
                    }
                    else
                    {
                        // this skews the result... hopefully never used:
                        ageGroup = Common.GetRandomEnumValue(AIAgeGroup.Adult, The.Sim.GameplayRandomGenerator);
                    }
                }

                AgeGroup.SetRandomAge(ageGroup.Value);
            }
        }

       

        public void SetCasteOnNewEntity(string key)
        {
            CasteType = Parent.EntityType.BiologicalType.Castes.Find(r => r.KeyName == key);
        }

        public void SetRaceOnNewEntity(string key)
        {
            RaceType = Parent.EntityType.BiologicalType.RaceTypes.FirstOrDefault(r => r.KeyName == key);
        }

        /// <summary>
        /// sets random caste and race if not already set.
        /// computes weight and height and bulk from age if not already set (if bulk is zero!)
        /// </summary>
        public void Initialize()
        {
           

            if (CasteType == null)
            {
                SetRandomCaste();
            }

            if (RaceType == null)
            {
                if (Parent.EntityType.BiologicalType.RaceTypes != null)
                {
                    // get random race:
                    int race;
                    Common.GetStairStepIndex(Parent.EntityType.BiologicalType.RaceTypes, out race, The.Sim.GameplayRandomGenerator);
                    RaceType = Parent.EntityType.BiologicalType.RaceTypes[race];
                }
            }

            Endurance = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution( 0.6f, 0.1f);
            Appearance = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution( 0.5f, 0.1f);

            // determined at birth...
            // these are not determined by race... it wouldn't make sense. perhaps introduce a "modifier" system...
            AdultTargetHeight = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution( CasteType.HeightMean, CasteType.HeightStandardDeviation);
            AdultTargetWeight = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution( CasteType.WeightMean, CasteType.WeightStandardDeviation);

            InitializeBioProperties();

            //Resilience = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(Parent.EntityType.BiologicalType.ResilienceMean resilienceMean.Value, resilienceDeviation.Value);

            if (Common.IsZero(Parent.Bulk))
            {
                SetRandomWeight();
            }

            if (Common.IsZero(Height))
            {
                SetRandomHeight();
            }

            // add more random stats here:


            Needs.Initialize();
        }


        /// <summary>
        /// here we compute and cache all bio properties - properties that can be overridden and perhaps interpolated in the caste/age/race/order system
        /// 
        /// this could be triggered when age changes??
        /// </summary>
        private void InitializeBioProperties()
        {
            // use a normal distribution when we want variance within individuals!
            Resilience = GetNormalDistributedBioProperty("ResilienceMean", "ResilienceStandardDeviation", 
                Parent.EntityType.BiologicalType.ResilienceMean, 
                Parent.EntityType.BiologicalType.ResilienceStandardDeviation);


            ActiveStealthRating = GetBioPropertyNumber("ActiveStealthRating", Parent.EntityType.BiologicalType.ActiveStealthRating.Value);
            PassiveStealthRating = GetBioPropertyNumber("PassiveStealthRating", Parent.EntityType.BiologicalType.PassiveStealthRating ?? GameData.Instance.Constants.DefaultPassiveStealthFactor);

            OxygenAndMuscleEnergyIncreaseRatePerDay = 
                GetBioPropertyNumber("OxygenAndMuscleEnergyIncreaseRatePerDay", 
                                     Parent.EntityType.BiologicalType.OxygenAndMuscleEnergyIncreaseRatePerDay.Value);

            StomachSizeFractionOfEntityBulk = GetBioPropertyNumber("StomachSizeFractionOfEntityBulk",
                                     Parent.EntityType.BiologicalType.StomachSizeFractionOfEntityBulk.Value);

            TimeToConsumeFullMealInDays = GetBioPropertyNumber("TimeToConsumeFullMealInDays",
                                     Parent.EntityType.BiologicalType.TimeToConsumeFullMealInDays.Value);

            MaxRegainLimit = GetBioPropertyNumber("MaxRegainLimit",
                                     Parent.EntityType.BiologicalType.MaxRegainLimit.Value);

            FractionOfMaxHitpointsGainedPerDay = GetBioPropertyNumber("FractionOfMaxHitpointsGainedPerDay",
                                     Parent.EntityType.BiologicalType.FractionOfMaxHitpointsGainedPerDay.Value);

            IsVermin = GetBioPropertyBoolean("IsVermin",
                                   Parent.EntityType.BiologicalType.IsVermin);


            if (Parent.AgentStorage != null)
            {
                // added to ensure that stomach capacity is set correctly in case bulk already has a value - otherwise this happens in bulk change event
                Parent.AgentStorage.NotifyStomachFractionChanged();
            }

            InitializeConsumeAndExtractionProcesses();
        }


        private void InitializeConsumeAndExtractionProcesses()
        {
            Dictionary<EntityType, HashSet<ProcessType>> oldExtractionProcesses;
            if (FoodExtractionProcesses != null)
            {
                oldExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>(FoodExtractionProcesses);
            }
            else
            {
                oldExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>();
            }

            Dictionary<EntityType, ProcessType> oldConsumeProcesses;
            if (ConsumeProcesses != null)
            {
                oldConsumeProcesses = new Dictionary<EntityType, ProcessType>(ConsumeProcesses);
            }
            else
            {
                oldConsumeProcesses = new Dictionary<EntityType, ProcessType>();
            }

            // TODO: placeholder code - make it so these collections can differ and be overridden by caste, age group etc.
            ConsumeProcesses = Parent.EntityType.BiologicalType.ConsumeProcesses;
            FoodExtractionProcesses = Parent.EntityType.BiologicalType.ExtractionProcesses;

            // use this when castes/ages have different consumptions: 
            BiologicalType.ComputeExtractionResultsInConsumables(FoodExtractionProcesses, ConsumeProcesses, ref ExtractionResultsInConsumable);

            // after updating the food processes, we have to notify the groups the entity is a part of - Household, Allegiance, Expedition...
            if (FoodExtraction.FoodProcessesHaveChanged(FoodExtractionProcesses, oldExtractionProcesses,
                                           ConsumeProcesses, oldConsumeProcesses))
            {
                // notify other components here. We don't use an event because snapshotting is a bit of a hassle.
                if (Parent.EntityType.Person != null)
                {
                    Parent.PersonEntity.NotifyFoodProcessesChanged();
                }

                if (Parent.EntityType.IntelligenceType != null)
                {
                    Parent.Intelligence.NotifyFoodProcessesChanged();
                }
                
               /* if (FoodProcessesChanged != null)
                    FoodProcessesChanged.Invoke();*/
            }

        }

       // Regulator frequentRegulator, slowRegulator;

       

       /* public override double? GetUpdateInterval()
        {
            if (!Parent.IsDead 
                && Parent.IsCompleted()
                && Parent.IsOnPlaySite())
            {
                return Math.Min(GameData.Instance.Constants.UpdateIntervalForBioEntity, GameData.Instance.Constants.SlowUpdateIntervalForBioEntity);
            }

            return null;
        }*/

        public void SetRandomCaste()
        {
            int caste;
            Common.GetStairStepIndex(Parent.EntityType.BiologicalType.Castes, out caste, The.Sim.GameplayRandomGenerator);
            CasteType = Parent.EntityType.BiologicalType.Castes[caste];
        }

        private void SetRandomWeight()
        {
            // http://graph-plotter.cours-de-math.eu/
            // (1/(1+15^(-x))- 0.5)*2
            float ageFractionOfAdult = AgeGroup.Age / AgeGroup.GetAdultAge(CasteType.AgeGroupTypes);

            // increases swiftly to the adult target weight, then increases more slowly and eventually stops.
            Weight = (float)((1f / (1f + Math.Pow(15, -ageFractionOfAdult) - 0.5f) * 0.55f * AdultTargetWeight));


            Parent.Bulk = GetBulkFromWeight(Weight);
        }


        public static float GetBulkFromWeight(float weight)
        {
            return 0.01f * weight; // ???
        }

       
        public float GetTotalStomachCapacity()
        {
           
            return StomachSizeFractionOfEntityBulk * Parent.Bulk;
        }

      /*  public float GetFreeStomachCapacity()
        {
            return (1f - StomachContents) * GetTotalStomachCapacity();

        }
        */
                

        private void SetRandomHeight()
        {
            float ageFractionOfAdult = AgeGroup.Age / AgeGroup.GetAdultAge(CasteType.AgeGroupTypes);

            // increases swiftly to the adult target height, then stops.
            Height = (float)((1f / (1f + Math.Pow(100, -ageFractionOfAdult) - 0.5f) * 0.5f * AdultTargetHeight));

        }

      


        public void SatisfyNeeds(double elapsedSeconds, float progressDelta, NeedSatisfaction[] needSatisfaction)
        {
            if (Needs != null && needSatisfaction != null)
            {
                Needs.SatisfyNeeds(elapsedSeconds, progressDelta, needSatisfaction);


            }
        }

        /// <summary>
        /// also updates Body to regain hitpoints!!
        /// </summary>
        /// <param name="gameTime"></param>
      /*  public void Update(GameTime gameTime)
        {
            

            double milliSecondsSinceLastReady = 0;
            if (frequentRegulator.IsReady(ref milliSecondsSinceLastReady))
            {
                double deltaTimeInSeconds = 1000d * milliSecondsSinceLastReady;

                // this depends on what the agent is doing, so update more often:
                Needs.UpdateFrequentSimulation(deltaTimeInSeconds);

                energyLevelIsDirty = true; // make sure that we recalculate energy


                // this counter also needs more fluent updates:
                if (OxygenAndMuscleEnergy < 1f)
                {
                    // modify oxygen regain rate by 'energy level':
                    OxygenAndMuscleEnergy =
                        Common.IncreaseValueBetweenZeroAndOne(OxygenAndMuscleEnergy,
                        OxygenAndMuscleEnergyIncreaseRatePerDay * EnergyLevel,
                        deltaTimeInSeconds);
                }
            }


            if (slowRegulator.IsReady(ref milliSecondsSinceLastReady))
            {
                double deltaTimeInSeconds = 1000d * milliSecondsSinceLastReady;

                AgeGroup.UpdateAge(deltaTimeInSeconds);

                if (StomachContents > 0f)
                {
                    StomachContents = Common.DecreaseValueBetweenZeroAndOne(StomachContents, Parent.EntityType.BiologicalType.StomachContentsDecreaseRatePerDay.Value, deltaTimeInSeconds);
                }

                Needs.UpdateSimulation(deltaTimeInSeconds);

                Parent.Body.RegainHitpoints(deltaTimeInSeconds);

                energyLevelIsDirty = true;
            }

        }*/

        public void UpdateSimulation(double deltaTimeInSeconds)
        {
            Needs.Update(deltaTimeInSeconds);

            // OLD: this depends on what the agent is doing, so update more often:
           // Needs.UpdateFrequentSimulation(deltaTimeInSeconds);

           // energyLevelIsDirty = true; // make sure that we recalculate energy


            // this counter also needs more fluent updates:
            if (OxygenAndMuscleEnergy < 1f)
            {
                // modify oxygen regain rate by 'energy level':
                OxygenAndMuscleEnergy =
                    Common.IncreaseValueBetweenZeroAndOne(OxygenAndMuscleEnergy,
                    OxygenAndMuscleEnergyIncreaseRatePerDay * EnergyLevel,
                    deltaTimeInSeconds);
            }

            AgeGroup.UpdateAge(deltaTimeInSeconds);

            if (StomachContents > 0f)
            {
                StomachContents = Common.DecreaseValueBetweenZeroAndOne(StomachContents, Parent.EntityType.BiologicalType.StomachContentsDecreaseRatePerDay.Value, deltaTimeInSeconds);
            }

           // Needs.UpdateSimulation(deltaTimeInSeconds);

          //  Parent.Body.RegainHitpoints(deltaTimeInSeconds); // moved

            energyLevelIsDirty = true;
        }

               

      /*  public void UpdateFrequentSimulationInParallel(double deltaTimeInSeconds)
        {
            // this depends on what the agent is doing, so update more often:
            Needs.UpdateFrequentSimulation(deltaTimeInSeconds);
          
            energyLevelIsDirty = true; // make sure that we recalculate energy


            // this counter also needs more fluent updates:
            if (OxygenAndMuscleEnergy < 1f)
            {
                // modify oxygen regain rate by 'energy level':
                OxygenAndMuscleEnergy = 
                    Common.IncreaseValueBetweenZeroAndOne(OxygenAndMuscleEnergy, 
                    OxygenAndMuscleEnergyIncreaseRatePerDay * EnergyLevel, 
                    deltaTimeInSeconds);
            }            
        }

        public void UpdateSimulation(double deltaTimeInSeconds)
        {

            AgeGroup.UpdateAge(deltaTimeInSeconds);

            if (StomachContents > 0f)
            {
                StomachContents = Common.DecreaseValueBetweenZeroAndOne(StomachContents, Parent.EntityType.BiologicalType.StomachContentsDecreaseRatePerDay.Value, deltaTimeInSeconds);
            }

            Needs.UpdateSimulation(deltaTimeInSeconds);

            Parent.Body.RegainHitpoints(deltaTimeInSeconds);

            energyLevelIsDirty = true;
        }*/



        /// <summary>
        /// 0 - 1
        /// </summary>
        /// <returns></returns>
        private float ComputeEnergyLevel()
        {
            // get weighted total of needs' energy factors
           // float? otherNeedsFactor = null;
            if (Needs.NeedsList.Count > 0)
            {
                float? currentFactor;
                float total = 0f;
              //  int noOfCountedNeeds = 0;
                foreach (var need in Needs.NeedsList)
                {
                    currentFactor = need.Value.GetEnergyFactor();

                    if (currentFactor.HasValue)
                    {
                        total += currentFactor.Value;
                      //  noOfCountedNeeds++;
                    }
                }

                return total;
                //otherNeedsFactor = total / (float)noOfCountedNeeds;
            }

            return 1f;

          //  return 0.5f * FoodLevel + 0.5f * SleepLevel;
        }

      
        public void GetStatus(out bool isDead, out bool isUnconscious, ref CauseOfDeath? causeOfDeath, ref CauseOfUnconsciousness? causeOfUnconsciousness)
        {
            isDead = false;
            isUnconscious = false;

          //  causeOfDeath = null;
          //  causeOfUnconsciousness = null;

            foreach (var need in Needs.NeedsList)
            {
                if (need.Value.PhysicalNeed != null)
                {
                    if (need.Value.PhysicalNeed.IsStarvedToDeath())
                    {
                        isDead = true;
                        causeOfDeath = CauseOfDeath.Starvation;
                    }
                    else if (need.Value.PhysicalNeed.IsCollapsedFromStarvation())
                    {
                        isUnconscious = true;
                        causeOfUnconsciousness = CauseOfUnconsciousness.Starvation;
                    }
                }
            }                       

        }


        public void GenerateBiologicalEntityData(EntityTypeTooltipInstanceData generatedData)
        {
            string speciesDescription = null;
            string orderDescription = null;

            if (Parent.PersonEntity == null)
            {
                orderDescription = "";
                if (Parent.EntityType.BiologicalType.OrderType != null)
                {
                    orderDescription += "ORDER: " + Parent.EntityType.BiologicalType.OrderType.Name + " \n"; 
                }
               

                if (Parent.EntityType.Name != null)
                {
                    speciesDescription = "SPECIES: " + Parent.EntityType.Name + " \n";
                }

              /*  if (RaceType != null && RaceType.Name != null) // skip the race...
                {
                    speciesDescription += "RACE: " + RaceType.Name + " \n";
                }*/
            }

            string sexDescription = "";
            if (CasteType.Reproduction == Reproduction.Female)
            {
                sexDescription = "SEX: Female \n";
            }
            else if (CasteType.Reproduction == Reproduction.Male)
            {
                sexDescription = "SEX: Male \n";
            }

            string ageDescription = "AGE: " + AgeGroup.AgeGroupType.Name;

            generatedData.Description = orderDescription + speciesDescription + sexDescription + ageDescription;

            if (RaceType != null && RaceType.Description != null)
            {
                generatedData.RaceTypeDescription = RaceType.Description;
            }
            
        }

        public void AddToStomachContents(float foodBulk)
        {
            StomachContents += foodBulk / GetTotalStomachCapacity();

            StomachContents = Common.Clamp(StomachContents, 0f, 1f);

        }

        public void SetStomachContents(float contents)
        {
            StomachContents = Common.Clamp(contents, 0f, 1f);
        }

        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);
            ActiveStealthRating = sn.DoFloat(this.ActiveStealthRating);
            AdultTargetHeight = sn.DoFloat(this.AdultTargetHeight);
            AdultTargetWeight = sn.DoFloat(this.AdultTargetWeight);
            AgeGroup = (AgeGroup)sn.DoISnapshot(this.AgeGroup);
            Appearance = sn.DoFloat(this.Appearance);        
            Blood = sn.DoFloat(this.Blood);
          
            casteKey = sn.DoString(CasteType != null ? CasteType.KeyName : null);
            raceKey = sn.DoString(RaceType != null ? RaceType.KeyName : null);
            
            DwellingSpot = sn.DoVector3Nullable(this.DwellingSpot);
            Endurance = sn.DoFloat(this.Endurance);
            energyLevel = sn.DoFloat(this.energyLevel);
            energyLevelIsDirty = sn.DoBool(this.energyLevelIsDirty);
            FractionOfMaxHitpointsGainedPerDay = sn.DoFloat(this.FractionOfMaxHitpointsGainedPerDay);
            Height = sn.DoFloat(this.Height);
        
            MaxRegainLimit = sn.DoFloat(this.MaxRegainLimit);
            Needs = (Needs)sn.DoISnapshot(this.Needs);
            oxygenAndMuscleEnergy = sn.DoFloat(this.oxygenAndMuscleEnergy);
            OxygenAndMuscleEnergyIncreaseRatePerDay = sn.DoFloat(this.OxygenAndMuscleEnergyIncreaseRatePerDay);
            PassiveStealthRating = sn.DoFloat(this.PassiveStealthRating);
            Resilience = sn.DoFloat(this.Resilience);
            StomachContents = sn.DoFloat(this.StomachContents);
            StomachSizeFractionOfEntityBulk = sn.DoFloat(this.StomachSizeFractionOfEntityBulk);
            TimeToConsumeFullMealInDays = sn.DoFloat(this.TimeToConsumeFullMealInDays);
            Weight = sn.DoFloat(this.Weight);
            IsVermin = sn.DoBool(IsVermin);
            ModelTextureName = sn.DoString(ModelTextureName); // #MIGRATE

           // this.FoodExtraction = (FoodExtraction)sn.DoISnapshot(FoodExtraction); // not used?
            this.ConsumeProcesses = sn.DoDictionary(ConsumeProcesses);       
            this.ExtractionResultsInConsumable = sn.DoMultiMapHashSet(ExtractionResultsInConsumable);
            this.FoodExtractionProcesses = sn.DoMultiMapHashSet(FoodExtractionProcesses);

            sn.Ignore(CasteType);
            sn.Ignore(RaceType);
            
            sn.Postpone(BiologicalChildren);
            sn.Postpone(Mate);
            sn.Postpone(BiologicalFather);
            sn.Postpone(BiologicalMother);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            CasteType = Parent.EntityType.BiologicalType.Castes.FirstOrDefault(c => c.KeyName.Equals(casteKey));

            if (raceKey != null)
            {
                RaceType = Parent.EntityType.BiologicalType.RaceTypes.FirstOrDefault(c => c.KeyName.Equals(raceKey));
            }

            AgeGroup.LoadPostProcess(sn);

            Needs.Parent = this;
            Needs.LoadPostProcess(sn);

            //CreateRegulators();
        }


        #endregion
    }
}

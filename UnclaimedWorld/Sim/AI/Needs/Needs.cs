using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.AI.Needs
{
    public class Needs : ISnapshot
    {     
        public Dictionary<string, Need> NeedsList = new Dictionary<string, Need>(); 
   
        public BiologicalEntity Parent;

        public int NoOfEssentialFoodNeeds;
        public int NoOfNonEssentialFoodNeeds;

        private bool needsLengthIsDirty = true;

        private float unsatisfiedEssentialNeedsLength;
        public float UnsatisfiedEssentialNeedsLength
        {
            get
            {
                if (needsLengthIsDirty)
                {
                    UpdateUnsatisfiedNeedsLength();
                }

                return unsatisfiedEssentialNeedsLength;
            }
        }

        private float unsatisfiedNonEssentialNeedsLength;
        public float UnsatisfiedNonEssentialNeedsLength
        {
            get
            {
                if (needsLengthIsDirty)
                {
                    UpdateUnsatisfiedNeedsLength();
                }

                return unsatisfiedNonEssentialNeedsLength;
            }
        }

        public Needs()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Needs(BiologicalEntity parent)
        {
            this.Parent = parent;

            
        }


        public void SetNeedsDirty()
        {
            needsLengthIsDirty = true;

        }

        public void Initialize()
        {
            UpdateSetOfNeeds();
        }

      /*  private float GetEnergy()
        {

        }*/


        public void UpdateNeedsTotalBulk()
        {
            foreach (var item in NeedsList)
            {
                if (item.Value.FoodNeed != null)
                {
                    item.Value.FoodNeed.UpdateBulk();
                }
            }
        }

        public void UpdateSetOfNeeds()
        {
            if (Parent.Parent.ID == (EntityID)4600)
            {
            }

            List<string> listOfNeedsToRemove = null;
            foreach (var kvp in NeedsList)
            {   // remove the needs that no longer exist in the new age group
                if (Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes.First(n => n.KeyName == kvp.Value.NeedType.KeyName) == null) // n.NeedClass == kvp.Value.NeedType.NeedClass) == null)
                {
                    Common.AddToList(ref listOfNeedsToRemove, kvp.Value.NeedType.KeyName);
                }
            }

            if (listOfNeedsToRemove != null)
            {
                foreach (string key in listOfNeedsToRemove)
                {
                    NeedsList.Remove(key);
                }
            }

            if (Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes != null)
            {
                foreach (NeedType needType in Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes)
                {
                    Need need;

                    if (NeedsList.TryGetValue(needType.KeyName, out need))
                    {
                        // update existing need - keeping the realtive deviation intact, but moving to a new distribution:
                        need.DecreasePerDay = (float)Common.MoveValueToNewNormalDistribution(need.DecreasePerDay, need.NeedType.DecreasePerDay.Mean.Value, need.NeedType.DecreasePerDay.StandardDeviation.Value,
                                                needType.DecreasePerDay.Mean.Value, needType.DecreasePerDay.StandardDeviation.Value);

                        need.NeedType = needType;
                    }
                    else
                    {
                        need = new Need(this, needType);
                        need.DecreasePerDay = (float)needType.DecreasePerDay.GetRandomValue(The.Sim.GameplayRandomGenerator); // (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(needType.DecreasePerDayMean, needType.DecreasePerDayStandardDeviation);

                        NeedsList.Add(needType.KeyName, need);
                    }
                }
            }

         


            // count the food needs:
            NoOfEssentialFoodNeeds = 0;
            NoOfNonEssentialFoodNeeds = 0;

            foreach (var need in NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null) // .NeedClass == AINeedClass.Food
                   // && need.Value.NeedType.PhysicalEffects != null)
                {
                    if (need.Value.NeedType.FoodNeedType.IsEssential == true)
                    {
                        NoOfEssentialFoodNeeds++;
                    }
                    else 
                    {
                        NoOfNonEssentialFoodNeeds++;
                    }
                }
            }

            UpdateNeedsTotalBulk();           
        }

        public bool IsEssential(FoodNutrientType nutrient)
        {
            foreach (var item in NeedsList)
            {
                if (item.Value.NeedType.FoodNeedType != null
                    && item.Value.NeedType.FoodNeedType.IsEssential)
                {
                    if (item.Value.NeedType.FoodNeedType.FoodNutrientType == nutrient)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Dictionary<FoodNutrientType, float> GetEssentialNeedBulkAmounts()
        {
            Dictionary<FoodNutrientType, float> needs = new Dictionary<FoodNutrientType, float>();
            foreach (var item in NeedsList)
            {
                if (item.Value.NeedType.FoodNeedType != null 
                    && item.Value.NeedType.FoodNeedType.IsEssential)
                {
                    needs.Add(item.Value.NeedType.FoodNeedType.FoodNutrientType, item.Value.FoodNeed.CurrentNeededNutrientBulk);
                }
            }

            return needs;
        }

        private void UpdateUnsatisfiedNeedsLength() //out float unsatisfiedEssentialNeedsLength, out float unsatisfiedNonEssentialNeedsLength)
        {
            unsatisfiedEssentialNeedsLength = 0f;
            unsatisfiedNonEssentialNeedsLength = 0f;

            foreach (var need in NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null)
                {

                    float unsatisfiedNeed = 1f - need.Value.CurrentLevel;

                    if (need.Value.NeedType.FoodNeedType.IsEssential)
                    {
                        unsatisfiedEssentialNeedsLength += unsatisfiedNeed;
                        //noOfEssentialNeeds++;
                    }
                    else
                    {
                        unsatisfiedNonEssentialNeedsLength += unsatisfiedNeed;
                    }
                }
            }

            needsLengthIsDirty = false;
        }


        /// <summary>
        /// push needs satisfaction from a process
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        /// <param name="needSatisfaction"></param>
        public void SatisfyNeeds(double elapsedSeconds, float progressDelta, NeedSatisfaction[] needSatisfaction)
        {
            double elapsedDays = elapsedSeconds * The.Sim.DateAndTime.DaysPerSecond;
            foreach (var item in needSatisfaction)
            {
                Need need;
                if (NeedsList.TryGetValue(item.NeedType, out need))
                {
                    float gain;
                    if (item.GainPerDay.HasValue)
                    {
                        gain = (float)(item.GainPerDay * elapsedDays);
                    }
                    else
                    {
                        gain = (float)(item.GainPerDay * progressDelta);
                    }

                    need.Satisfy(gain); 
                }
            }
        }

        public void Update(double deltaTimeInSeconds)
        {
            foreach (var kvp in NeedsList)
            {
                kvp.Value.Update(deltaTimeInSeconds);                
            }
        }

        /// <summary>
        /// pull needs satisfication, and update needs
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        public void UpdateSimulation(double deltaTimeInSeconds)
        {
            foreach (var kvp in NeedsList)
            {
                // these needs can be updated less often:
                if (kvp.Value.NeedType.PhysicalEffects == null || kvp.Value.NeedType.PhysicalEffects.UseExertionFactorToDecrease == false) // kvp.Key != NeedClass.Food)
                {
                    kvp.Value.Update(deltaTimeInSeconds);
                }
            }
        }

        public void UpdateFrequentSimulation(double deltaTimeInSeconds)
        {
            foreach (var kvp in NeedsList)
            {
                // these needs must be updated often:
                if (kvp.Value.NeedType.PhysicalEffects != null && kvp.Value.NeedType.PhysicalEffects.UseExertionFactorToDecrease == true) // kvp.Key != NeedClass.Food)
                {
                    kvp.Value.Update(deltaTimeInSeconds);
                }
            }          

        }



        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            NeedsList = sn.DoDictionary(this.NeedsList);
        
            this.NoOfEssentialFoodNeeds = sn.DoInt32(NoOfEssentialFoodNeeds);
            this.NoOfNonEssentialFoodNeeds = sn.DoInt32(NoOfNonEssentialFoodNeeds);


            sn.Ignore(Parent); // is summarily assigned in Post Load
            sn.Ignore(needsLengthIsDirty);
            sn.Ignore(unsatisfiedEssentialNeedsLength);
            sn.Ignore(unsatisfiedNonEssentialNeedsLength);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
            foreach (var item in NeedsList)
            {
                item.Value.Parent = this;

                item.Value.LoadPostProcess(sn);
            }

        }

        #endregion

       
    }
}

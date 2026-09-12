using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    public class Food : ISnapshot
    {
        /// <summary>
        /// the absolute nutrient amounts contained in the item - found by multiplying the item bulk with the nutrient profile data
        /// </summary>
        public Dictionary<FoodNutrientType, float> NutrientBulkAmounts = new Dictionary<FoodNutrientType, float>();

        public Item Parent;
      
      
        public Food()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        public Food(Item parent)
        {
            this.Parent = parent;
            
        }

        public void UpdateNutrientAmounts(Entity parent)
        {
            // clear and recompute the amounts:
            NutrientBulkAmounts.Clear();

            UpdateNutrientAmounts(parent.EntityType, parent.Bulk, NutrientBulkAmounts);
        }

        public static void UpdateNutrientAmounts(EntityType foodType, float bulk, Dictionary<FoodNutrientType, float> nutrientBulkAmounts)  
        {
            
            foreach (var nutrientType in foodType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes)
            {
                nutrientBulkAmounts.Add(nutrientType.Nutrient, bulk * nutrientType.Amount);
            }
        }


        public void ConsumeBy(Entity consumer)
        {
            foreach (var nutrient in NutrientBulkAmounts)
            {
                Need need;

                float uptake;

                // do we have a need for this nutrient?
                if (nutrient.Value > 0f
                    && consumer.BiologicalEntity.Needs.NeedsList.TryGetValue(nutrient.Key.KeyName, out need))
                {
                    uptake = Math.Min(nutrient.Value, need.FoodNeed.CurrentNeededNutrientBulk);

                    LogConsumeStatistics(consumer, nutrient.Key, nutrient.Value, uptake);

                    float normalizedUptake = uptake / need.FoodNeed.TotalNeededNutrientBulk;

                    need.Satisfy(normalizedUptake);

                    //need.CurrentLevel += uptake;
                }
            }


            var effects = Parent.Parent.EntityType.ItemType.FoodType.EffectTypes;
            if (effects != null)
            {
                foreach (var item in effects)
                {
                    //start the effect on the consumer:
                    consumer.SimEffects.Start(item);                   

                }

            }

        }

        private void LogConsumeStatistics(Entity consumer, FoodNutrientType nutrientType, float nutrientValue, float uptake)
        {
            if (consumer.Intelligence.IsIndependent())
            {
                float overflow = nutrientValue - uptake;

                consumer.Intelligence.Allegiance.Statistics.AddNutrientEvent(nutrientType, NutrientStatistics.StatTypes.Consumed, uptake);
                if (Common.IsGreaterThan(overflow, 0f))
                {
                    consumer.Intelligence.Allegiance.Statistics.AddNutrientEvent(nutrientType, NutrientStatistics.StatTypes.Overconsumed, overflow);
                }
            }
        }


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
            this.NutrientBulkAmounts = sn.DoDictionary(NutrientBulkAmounts);


            sn.Ignore(Parent);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            
        }

    }
}

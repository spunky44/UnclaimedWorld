using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using System.Linq;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Buildings
{
    /// <summary>
    /// contians Households - not members
    /// </summary>
    public class Residence : ISnapshot
    {        
        Entity parent;
        EntityID snapshotParent;

        #region "Locks" - can be set on memory fact too
        /// <summary>
        /// Use Add and Remove methods instead!
        /// </summary>
      //  public List<Entities.Household> Households = new List<Entities.Household>();
        public List<HouseholdID> Households = new List<HouseholdID>();
        

        /// <summary>
        /// keep this updated when household members change by calling UpdateResidentsNo!
        /// </summary>
        public int Residents = 0;

        #endregion


        public Residence()
        {
        }

        public Residence(Entity parent)
        {
            this.parent = parent;
        }

       /* public void UpdateResidentsNo()
        {
           
            Residents = 0;

            for (int i = 0; i < Households.Count; i++)
            {
                Residents += Households[i].NoOfMembers;
            }

        }*/


        /// <summary>
        /// compute the value from the comfort level and the structure condition
        /// 
        /// max: GameData.Instance.Constants.MaximumSleepNeedGainFactorForPeople
        /// </summary>
        /// <returns></returns>
        public static float GetSleepNeedGainFactor(double condition, float comfortLevel) // EntityType residenceEntityType)
        {
           // float condition = parent.NonLivingEntity.Condition;

            float product = (float)(condition * comfortLevel); // parent.EntityType.ContainerType.ResidenceType.ComfortLevel; 

            product = Common.ClampBottom(product, 0.4f);

            return MathHelper.Lerp(GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen, GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople, product);

        }

        /// <summary>
        /// can only be called on the Entity. The agent must see the home physically.
        /// </summary>
        /// <param name="household"></param>
     /*   public void AddHousehold(Household household)
        {
            Households.Add(household);
            UpdateResidentsNo();
        }

        /// <summary>
        /// TODO: call on MemoryFact as well
        /// they never move out... what about when they die?
        /// </summary>
        /// <param name="household"></param>
        public void RemoveHousehold(Household household)
        {
            if (Households.Contains(household))
            {
                Households.Remove(household);
            }

            UpdateResidentsNo();
        }*/

        public static bool HasCapacity(Household household, int residents, EntityType residenceType)
        {
            if (residents + household.NoOfMembers > residenceType.ContainerType.ResidenceType.LivingCapacity)
            {
                return false;
            }

            return true;
        }

        public static bool RemoveHousehold(SharedKnowledge sharedKnowledge, Household household) //, EntityID? home) //IKnownEntityData homeData)
        {
            if (household.Home != null)
            {
                IKnownEntityData homeData;
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(household.Home.Value, out homeData)))
                {
                    household.Home = null;

                    return false;
                }
                else
                {
                    homeData.Households.Remove(household.ID);

                    UpdateResidentsNo(homeData);

                    household.Home = null;

                }
            }

            return true;

        }

        public static bool AddHousehold(SharedKnowledge sharedKnowledge, Household household, EntityID? home) //IKnownEntityData homeData)
        {
            if (home != null)
            {
                IKnownEntityData homeData;
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(home.Value, out homeData)))
                {
                    home = null;

                    return false;
                }
                else
                {
                    homeData.Households.Add(household.ID);

                    UpdateResidentsNo(homeData);

                }
            }

            return true;

        }

        public static List<Entity> GetResidents(IKnownEntityData entityData)
        {
            List<Entity> residents = null;
            if (entityData.Households != null)
            {
                foreach (var item in entityData.Households)
                {
                    Household household = LookUp<Household, HouseholdID>.FindByID(item);
                    if (household != null)
                    {
                        household.IterateMembers(e => Common.AddToList(ref residents, e));
                    }
                }
            }

            return residents;
        }

        public static void UpdateResidentsNo(IKnownEntityData homeData)
        {
            homeData.Residents = 0;

            for (int i = homeData.Households.Count - 1; i >= 0; i--)
            {

                Household household = LookUp<Household, HouseholdID>.FindByID(homeData.Households[i]);
                if (household != null)
                {
                    homeData.Residents += household.NoOfMembers;
                }
                else
                {
                    homeData.Households.RemoveAt(i);
                }
            }  

        }

        public static bool UpdateResidentsNo(SharedKnowledge sharedKnowledge, EntityID? home)
        {
            if (home != null)
            {
                IKnownEntityData homeData;
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(home.Value, out homeData)))
                {
                    home = null;

                    return false;
                }
                else
                {
                    UpdateResidentsNo(homeData);                                    
                }
            }

            return true;
        }


        public static double GetComfortRating(Entity homeData) //IKnownEntityData homeData) //double? condition, float comfortLevel)
        {
             
            float comfortLevel = homeData.EntityType.ContainerType.ResidenceType.ComfortLevel;

            comfortLevel = homeData.GetEffect(SimEffects.AffectsNumbers.OfferedComfort, comfortLevel);

            double conditionToUse = homeData.Condition ?? 1d; // condition ?? 1d;

            if (conditionToUse > GameData.Instance.AIConstants.Ratings.Comfort.MinimumHomeConditionToConsiderPerfect) // 
            {
                // let's ignore minor condition issues, so consider values above 0.9 a perfect condition. //mp april 2016 we removed the decay penalty on comfort. is now 0f
                conditionToUse = 1d;
            }
            else
            {
                conditionToUse = (double)MathHelper.Lerp(0f, 1f, (float)conditionToUse / GameData.Instance.AIConstants.Ratings.Comfort.MinimumHomeConditionToConsiderPerfect);
                conditionToUse = Common.Clamp(conditionToUse, 0d, 1d);
            }

            return comfortLevel * conditionToUse; // parent.EntityType.ContainerType.ResidenceType.ComfortLevel * condition;

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
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(parent);

           /* if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotHouseholds = Households.Select(h => h.ID).ToList();
            }
            snapshotHouseholds = sn.DoList(snapshotHouseholds);
            */
            Households = sn.DoList(Households);

            this.Residents = sn.DoInt32(Residents);
           
           // sn.Ignore(Households);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            this.parent = Entity.FindByID(snapshotParent);

           // Households = snapshotHouseholds.Select(h => LookUp<Household, HouseholdID>.FindByID(h)).ToList();
        }



        #endregion
    }
}

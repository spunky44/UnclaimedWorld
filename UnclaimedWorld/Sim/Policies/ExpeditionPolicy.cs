using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Policies
{
    /// <summary>
    /// let's rename to EntityGroupPolicy since there are members and jobs
    /// </summary>
    public class ExpeditionPolicy: ISnapshot
    {
       
        public float? FractionIndependentsAllowedToSleep;

        /// <summary>
        /// fill in one of these
        /// </summary>     
        public int? IndependentsAllowedToSleep = null;


        /// <summary>
        /// survival tiers are always unlocked!
        /// </summary>
        public Dictionary<RatingTypes, TierType> CurrentTiers;

        /// <summary>
        /// default is 'Allow'/True when no entry exists
        /// </summary>
        public Dictionary<EntityType, bool> AllowAmmoForVermin;


        public ExpeditionPolicy()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                CurrentTiers = new Dictionary<RatingTypes, TierType>();
                foreach (var item in Enum.GetValues(typeof(RatingTypes)))
                {
                    CurrentTiers.Add((RatingTypes)item, GameData.Instance.Tiers[0]);
                }

                AllowAmmoForVermin = new Dictionary<EntityType, bool>();
            }

        }

        /// <summary>
        /// checks only weapon attacks that uses ammo against the policy
        /// </summary>
        /// <param name="attackVermin"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public float GetWeaponPolicyScore(bool attackVermin, EntityType weapon, AttackType attackType)
        {
            if (attackVermin == false)
            {
                return 1f;
            }
            else 
            {
                
                if (attackType != null)
                {
                    if (attackType.UsesAmmoType != null)
                    {
                        if (GetAllowAmmoForVermin(attackType.UsesAmmoType))
                        {
                            return 1f;
                        }
                        else
                        {
                            return 0f;
                        }
                    }
                    else
                    {
                        return 1f;
                    }
                }
                else
                {
                    // see if any attack is allowed
                    bool attacksUsingAmmoAreForbidden = false;

                    foreach (var item in weapon.ItemType.WeaponType.AttackTypes)
                    {
                        if (item.UsesAmmoType != null)
                        {
                            if (GetAllowAmmoForVermin(item.UsesAmmoType))
                            {
                                return 1f;
                            }
                            else
                            {
                                attacksUsingAmmoAreForbidden = true;
                            }
                        }
                    }

                    if (attacksUsingAmmoAreForbidden)
                    {
                        return 0f;
                    }

                    return 1f;
                }
            }
        }

        public bool GetAllowAmmoForVermin(EntityType ammo)
        {
            bool oldOrder;
            if (AllowAmmoForVermin.TryGetValue(ammo, out oldOrder))
            {
                return oldOrder;
            }

            return true;
        }

        public void SetAllowAmmoForVermin(EntityType ammo, bool value)
        {
            Common.AddOrUpdateDictionary(ref AllowAmmoForVermin, ammo, value);
        }

        /// <summary>
        /// Creates a Policy from a PolicyData
        /// </summary>
        public static ExpeditionPolicy CreateFromPolicyData(ExpeditionPolicyData policyData)
        {
            ExpeditionPolicy policy = new ExpeditionPolicy();

            if (policyData != null)
            {
               // policy.IndependentsToStayAwake = policyData.IndependentsToStayAwake;
                policy.IndependentsAllowedToSleep = policyData.IndependentsAllowedToSleep;
                policy.FractionIndependentsAllowedToSleep = policyData.FractionIndependentsAllowedToSleep;

                if (policyData.CurrentTiers != null)
                {
                    policy.CurrentTiers = new Dictionary<RatingTypes, TierType>();
                    foreach (var item in policyData.CurrentTiers)
                    {
                        policy.CurrentTiers.Add(item.Key, GameData.Instance.AllTierTypes[item.Value]);
                    }
                }

                if (policyData.AllowAmmoUseAgainstVermin != null)
                {                   
                    foreach (var item in policyData.AllowAmmoUseAgainstVermin)
                    {
                        policy.SetAllowAmmoForVermin(GameData.Instance.AllEntityTypes[item.Key], item.Value);
                    }
                }
            }

            return policy;
        }


        public bool SleepInShiftsIsActive()
        {
            return FractionIndependentsAllowedToSleep.HasValue || IndependentsAllowedToSleep.HasValue;
        }


        public bool TierIsUnlocked(RatingTypes rating, TierType tier)
        {
            TierType currentTier = CurrentTiers[rating];
            if (tier.Index <= currentTier.Index)
            {
                return true;
            }

            return false;
        }

        public bool IsLaterTier(RatingTypes ratingType, TierType tier)
        {
            TierType currentTier = CurrentTiers[ratingType];

            if (tier.Index > currentTier.Index + 1)
            {
                return true;
            }

            return false;
        }

        public bool RatingIsInsideOrAbovePreviousTier(RatingTypes ratingType, float rating, TierType tier, out float requiredRating)
        {
            TierType currentTier = CurrentTiers[ratingType];
            TierType previousTier;
            float previousTierEdge;
            TierType.GetTierBelow(currentTier.Index, out previousTier, out previousTierEdge);

            TierType previousPreviousTier;
            float previousPreviousTierEdge;
            TierType.GetTierBelow(currentTier.Index, out previousPreviousTier, out previousPreviousTierEdge);


            requiredRating = previousPreviousTierEdge;

            if (Common.IsGreaterThanOrEqual(rating, previousPreviousTierEdge)) //tier.Index > currentTier.Index + 1)
            {
                return true;
            }
            else
            {                
                return false;
            }
        }
        
       /* public bool IsUnlockableTier(RatingTypes ratingType, float rating, TierType tier)
        {
            TierType currentTier = CurrentTiers[ratingType];

            if (rating > currentTier.UpperEdge //&& rating < tier.UpperEdge
                && tier.Index == currentTier.Index + 1)
            {
                return true;
            }

            return false;

        }
        */

        public bool CanUseProcess(ProcessType process, out TierOrAreaType policy)
        {
            TierOrAreaType tierArea = process.GetTierArea();

            if (tierArea != null)
            {
                if (!CanProduceOrTrade(tierArea)) // process.TierOrAreaType))
                {
                    policy = tierArea;
                    return false;
                }
            }

            /*
            if (process.TierAreaType != null)
            {
                if (!CanProduceOrTrade(process.TierAreaType))
                {
                    policy = process.TierAreaType;
                    return false;
                }
            }
            else if (process.Outputs != null)
            {
                foreach (var item in process.Outputs)
                {
                    if (!item.IsWasteProduct)
                    {
                        if (!CanProduceOrTradeItem(item.FinalEntityTypeToCreate))
                        {
                            policy = item.FinalEntityTypeToCreate.TierAreaType;
                            return false;
                        }
                    }
                }
            }*/

            policy = null;
            return true;
        }

        public bool CanProduceOrTrade(TierOrAreaType tierOrAreaType) // EntityType item)
        {
            if (tierOrAreaType == null)
            {
                return true;
            }
            else
            {
                if (tierOrAreaType.TierArea != null)
                {
                    return CurrentTiers[tierOrAreaType.TierArea.Area].Index >= tierOrAreaType.TierArea.TierType.Index;
                }
                else
                {
                    return CurrentTiers.Any(t => t.Value.Index >= tierOrAreaType.TierType.Index);
                }

            }

            /*
            if (item.TierAreaType == null)
            {
                return true;
            }
            else
            {
                TierArea tierArea = item.TierAreaType;

                return CanProduceOrTrade(tierArea);
            }*/

        }

        /*
        public bool CanProduceOrTrade(TierArea tierArea)
        {
            if (tierArea == null)
                return true;

            return CurrentTiers[tierArea.Area].Index >= tierArea.TierType.Index;
        }
        */

        public void AdoptTierPolicy(TierType tier, RatingTypes rating)
        {
            CurrentTiers[rating] = tier;            
        }

        public float GetPolicyMinimum(RatingTypes rating)
        {
            int currentIndex = CurrentTiers[rating].Index;
            if (currentIndex > 0)
            {
                return GameData.Instance.Tiers[currentIndex - 1].UpperEdge;
            }
            else return 0f;
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); //sn.DoVersion((Snapshotter.Version)2);  //sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {                      
            IndependentsAllowedToSleep = sn.DoInt32Nullable(IndependentsAllowedToSleep);
            FractionIndependentsAllowedToSleep = sn.DoFloatNullable(FractionIndependentsAllowedToSleep);

            CurrentTiers = sn.DoDictionary(CurrentTiers);

          //  AllowAmmoForVermin = sn.DoDictionary(AllowAmmoForVermin);
            //AllowAmmoForVermin was added in version 2 of Sim
           /* if ((uint)version >= 2) // v. 0.9.3.0
            {*/
                AllowAmmoForVermin = sn.DoDictionary(AllowAmmoForVermin);
          /*  }
            else
            {
                AllowAmmoForVermin = new Dictionary<EntityType, bool>();
            }*/

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           /* if (JobTypePriorities != null)
            {
                foreach (var item in JobTypePriorities)
                {
                    item.Key.loadpo    
                }
            }*/

        }

        #endregion
    }
}

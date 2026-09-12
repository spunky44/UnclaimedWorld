using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Items;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Allegiances.Statistics
{
    /// <summary>
    /// Only the allegiance will compute stockpile stats, member individuals will share this value
    /// </summary>
    public class SecurityStatisticsForAllegiance : SecurityStatistics 
    {
        /// <summary>
        /// for othersite entities, this will probably be the only component in member rating
        /// </summary>
        public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();

        /*
        #region Shared Stats
        // stockpiled weapons and common security that all individuals benefit from.
        //Entities should use this score as part of their own

        List<DataPoint<float>> handWeaponScore = new List<DataPoint<float>>();

        List<DataPoint<float>> defensiveStructureScore = new List<DataPoint<float>>();

        /// <summary>
        /// not sure if we need this, we can do a weighted sum of colony + member ratings instead
        /// </summary>
        List<DataPoint<float>> totalSharedRating = new List<DataPoint<float>>();


        #endregion
        */


        // polled stats:
        // spotted/known threats inside perimeter
        // infiltrated/outside threats?
        
        public SecurityStatisticsForAllegiance()         
        {

        }

        public SecurityStatisticsForAllegiance(GroupStatistics parent)
            : base(parent)
        {
 
        }

        public float GetSharedRatings()
        {
            if (SharedRating.Count > 0)
                return SharedRating.Last().Value;

            return 0f;
        }


        private Dictionary<EntityType, int> GatherAmmoData()
        {
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
         
            Dictionary<EntityType, int> ammoRounds = new Dictionary<EntityType, int>();

            group.IterateOwnedItems(e =>
            {
                GatherAmmoData(e, ref ammoRounds);
            });

            return ammoRounds;

        }


        /// <summary>
        /// robots with intrinsic weapons are scored here... also intrinsic attacks. Each agetn can contributw max 1 attack type.
        /// </summary>
        /// <param name="noOfDefensiveAgents"></param>
        /// <param name="noOfWeapons"></param>
        /// <param name="averageRating"></param>
        /// <param name="totalHandWeaponsScore"></param>
        private void ScoreDefensiveAgents(float assets, Dictionary<EntityType, int> ammoRounds, out int noOfDefensiveAgents, out float averageRating, out float totalScore, out float finalAgentsRating)
        {
            totalScore = 0f;
            noOfDefensiveAgents = 0;
            averageRating = 0f;

            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            Allegiance allegiance = group.GetAllegiance;

            Security security = GameData.Instance.AIConstants.Ratings.Security;

            int agents = 0;
            float score = 0f;
            float defenseRating;
            group.IterateMembers(e =>
            {
                defenseRating = GetAgentDefenseRating(e, security, ammoRounds);
                if (!Common.IsZero(defenseRating)) // defenseRating != DefenseRatings.None)
                {
                    agents++;
                    score += defenseRating;
                }
            });

            totalScore = score;
         
            noOfDefensiveAgents = agents;

            averageRating = score;
            if (noOfDefensiveAgents > 0)
            {
                averageRating /= noOfDefensiveAgents;
            }

            if (!Common.IsZero(assets)) 
            {
                finalAgentsRating = totalScore / assets;
            }
            else
            {
                finalAgentsRating = totalScore;
            }
           
        }

       /* private float GetDefenseRating(Security security, DefenseRatings rating)
        {
            return security.DefenseRatingValue * (int)rating;
        }*/

        public override void AddSharedRating(float ratingValue)
        {
            base.AddSharedRating(ratingValue);

            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            SharedRating.Add(new DataPoint<float>()
            {
                Time = now,
                Value = ratingValue
            });
        }

      //  private void ScoreHandWeapons(int noOfMembers, Dictionary<EntityType, int> ammoRounds, out int noOfWeaponCarriers, out int noOfWeapons, out float averageRating, out float totalHandWeaponsScore)
        private void ScoreHandWeapons(float assets, Dictionary<EntityType, int> ammoRounds, out int noOfWeaponCarriers, out int noOfWeapons, out float averageRating,  out float totalHandWeaponsScore, out float finalHandWeaponsScore)
        {
            totalHandWeaponsScore = 0f;
            noOfWeapons = 0;
            noOfWeaponCarriers = 0;
            averageRating = 0f;
            finalHandWeaponsScore = 0f;

            if (Parent.RepresentativeEntityType.IntelligenceType.CanUseWeapons != true)
            {
                return;
            }

            Security security = GameData.Instance.AIConstants.Ratings.Security;

            int weaponCarriers = 0;
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            Allegiance allegiance = group.GetAllegiance;

            group.IterateMembers(e =>
            {
                if (e.EntityType.IntelligenceType.CanUseWeapons == true)
                {
                    weaponCarriers++;
                }
            });
            
            int handWeaponsCap = (int)(weaponCarriers * security.MaxHandWeaponsToScorePerMember);
           
            List<EntityID> weapons = null;

            group.IterateOwnedItems(e =>
            {
                GatherHandWeaponsData(weaponCarriers, e, ref weapons); // out weaponsData); //, ref totalWeapons);
            });

            noOfWeaponCarriers = weaponCarriers;
                      

          
            // check for status, ammo available
            // every weapon must have XX corresponding ammo item(s)/rounds - search all owned entity groups!         
          //  List<Tuple<IKnownEntityData, DefenseRatings>> weaponsData = new List<Tuple<IKnownEntityData, DefenseRatings>>();
            List<Tuple<IKnownEntityData, float>> weaponsData = new List<Tuple<IKnownEntityData, float>>();
            if (weapons != null)
            {
                foreach (var item in weapons)
                {
                    IKnownEntityData weaponData;
                    if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(item, out weaponData)))
                    {
                        float defenseRating;
                        if (IsUsableWeapon(weaponData, security, ammoRounds, out defenseRating))
                        {
                            weaponsData.Add(new Tuple<IKnownEntityData, float>(weaponData, defenseRating));
                        }
                    }
                }
            }

                      
            noOfWeapons = weaponsData.Count;
            int count = 0;

            // score the best weapons up to the cap, either use ItemType.TaskAppropriateLevels for Patrol or a new field.
            if (weaponsData.Count > handWeaponsCap)
            {
                var ordered = weaponsData.OrderByDescending(t => t.Item2);

                
                foreach (var item in ordered)
                {
                    totalHandWeaponsScore += item.Item2;
                    count++;

                    if (count >= handWeaponsCap)
                    {
                        break;
                    }
                }

            }
            else
            {
                count = weaponsData.Count;
                totalHandWeaponsScore = weaponsData.Sum(t => t.Item2);
            }

            
           // totalHandWeaponsScore *= security.DefenseRatingValue;

            if (count > 0)
            {
                averageRating = totalHandWeaponsScore / count;
            }

           /* if (noOfMembers > 0)
            {
                totalHandWeaponsScore = totalHandWeaponsScore / noOfMembers;
            }*/
            if (!Common.IsZero(assets))
            {
                finalHandWeaponsScore = totalHandWeaponsScore / assets;
            }
            else
            {
                finalHandWeaponsScore = totalHandWeaponsScore;
            }
        }

        private void GatherHandWeaponsData(int noOfMembers, EntityGroup ownedItems, ref List<EntityID> weapons) //out List<IKnownEntityData> weaponsData) //, ref int totalWeapons)
        {
            foreach (var item in ownedItems.Items) //.WeaponsByAttackType)
            {
                if (AffectsHandWeaponsRating(item.Key))
                {                  
                    Common.AddToList(ref weapons, item.Value);

                    //totalWeapons += item.Value.Count;
                }
            }           
        }

        public static bool AffectsHandWeaponsRating(EntityType entityType)
        {
            if (entityType.ItemType != null && entityType.ItemType.WeaponType != null
                && !entityType.IsIntrinsic()
                && entityType.IsMountable()
                && !Common.IsZero(entityType.ItemType.WeaponType.GetHighestDefenseRating()))
            {
                return true;
            }

            return false;
        }

        public static bool AffectsDefenderRating(EntityType entityType)
        {
            if (entityType.IntelligenceType != null)
            {
                
                // add intrinsic weapons
                if (entityType.IntelligenceType.IntrinsicWeaponTypes != null)
                {
                    foreach (var item in entityType.IntelligenceType.IntrinsicWeaponTypes)
                    {
                        foreach (var attackType in item.ItemType.WeaponType.AttackTypes)
                        {
                            if (attackType.DefenseRating.HasValue)
                            {
                                return true;
                            }
                            //GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType);
                        }
                    }
                }

                // now add intrinsic attacks       
                if (entityType.IntelligenceType.AttackTypes != null)
                {
                    foreach (var intrinsicAttackType in entityType.IntelligenceType.AttackTypes)
                    {
                        if (intrinsicAttackType.DefenseRating.HasValue)
                        {
                            return true;
                        }

                      //  GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, intrinsicAttackType);
                    }
                }       
            }


            return false;           
        }

        public static bool AffectsSecurityRating(EntityType entityType)
        {
            return AffectsHandWeaponsRating(entityType)
                || AffectsDefenderRating(entityType);

        }

        private void GatherAmmoData(EntityGroup ownedItems, ref Dictionary<EntityType, int> ammoRounds) 
        {
            foreach (var item in ownedItems.AmmoItems) //.WeaponsByAttackType)
            {
                int ammoRoundsCount = item.Value.TotalRounds;
                int oldAmmoRoundsCount;
                if (ammoRounds.TryGetValue(item.Key, out oldAmmoRoundsCount))
                {
                    ammoRoundsCount += oldAmmoRoundsCount;
                }

                ammoRounds[item.Key] = ammoRoundsCount;
            }
        }

        private bool IsUsableWeapon(IKnownEntityData weapon, Security security, Dictionary<EntityType, int> ammoRounds, out float defenseRating)
        {
            if (!AffectsHandWeaponsRating(weapon.EntityType) /* weapon.EntityType.IsIntrinsic()
                || !weapon.EntityType.IsMountable()*/
                || !weapon.IsCompleted()
                || !Entity.IsFunctional(weapon)) // Common.IsZero(GoalEvaluator.ScoreIsEntityFunctional(weapon)))
            {
                defenseRating = 0f; // DefenseRatings.None;
                return false;
            }
                
            defenseRating = GetWeaponDefenseRating(weapon, security, ammoRounds);

            return !Common.IsZero(defenseRating);
           // return defenseRating != DefenseRatings.None;
        }

        private static float GetAgentDefenseRating(Entity agent, Security security, Dictionary<EntityType, int> ammoRounds)
        {
           // DefenseRatings bestDefenseRating = DefenseRatings.None;
            float bestDefenseRating = 0f;
            AttackType bestAttackType = null;

            // see if we have enough ammo for an attack type:
            Intelligence entityIntelligence = agent.Intelligence;
           
            // add intrinsic weapons
            if (entityIntelligence.IntrinsicWeapons != null)
            {
                foreach (var item in entityIntelligence.IntrinsicWeapons)
                {
                    foreach (var attackType in item.Key.ItemType.WeaponType.AttackTypes)
                    {
                        GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType);
                    }
                }
            }

            // now add intrinsic attacks       
            if (agent.EntityType.IntelligenceType.AttackTypes != null)
            {
                foreach (var intrinsicAttackType in agent.EntityType.IntelligenceType.AttackTypes)
                {
                    GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, intrinsicAttackType);
                }
            }            

            if (bestAttackType != null)
            {
                // 'consume' ammo:
                ConsumeAmmo(security, ammoRounds, bestAttackType);
            }

            return bestDefenseRating;
        }


        private static float GetWeaponDefenseRating(IKnownEntityData weapon, Security security, Dictionary<EntityType, int> ammoRounds)
        {
            float bestDefenseRating = 0f; // DefenseRatings.None;
            AttackType bestAttackType = null;

            // see if we have enough ammo for an attack type:
            foreach (var attackType in weapon.EntityType.ItemType.WeaponType.AttackTypes)
            {
                GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType);
            }
 
            if (bestAttackType != null)
            {
                // 'consume' ammo:
                ConsumeAmmo(security, ammoRounds, bestAttackType);            
            }           

             return bestDefenseRating;
        }

        private static void ConsumeAmmo(Security security, Dictionary<EntityType, int> ammoRounds, AttackType bestAttackType)
        {
            if (bestAttackType.UsesAmmoType != null)
            {
                int availableRounds;
                if (ammoRounds.TryGetValue(bestAttackType.UsesAmmoType, out availableRounds))
                {
                    int costInRounds = bestAttackType.RoundsToSpend ?? 0;
                    int requiredRounds = (int)(security.MinimumNoOfAttacksForWeaponToCount * costInRounds);
                    if (availableRounds >= requiredRounds)
                    {
                        availableRounds -= requiredRounds;
                        ammoRounds[bestAttackType.UsesAmmoType] = availableRounds;
                    }
                }
            }
        }



        private static void GetAttackTypeDefenseRating(Security security, Dictionary<EntityType, int> ammoRounds, ref float /* DefenseRatings*/ bestDefenseRating, ref AttackType bestAttackType, AttackType attackType)
        {
            //if (attackType.DefenseRating.HasValue && (int)attackType.DefenseRating.Value > (int)bestDefenseRating)
            if (attackType.DefenseRating.HasValue && attackType.DefenseRating.Value > bestDefenseRating)
            {
                if (attackType.UsesAmmoType != null)
                {
                    int availableRounds;
                    if (ammoRounds.TryGetValue(attackType.UsesAmmoType, out availableRounds))
                    {
                        int costInRounds = attackType.RoundsToSpend ?? 0;
                        int requiredRounds = (int)(security.MinimumNoOfAttacksForWeaponToCount * costInRounds);
                        if (availableRounds >= requiredRounds)
                        {

                            bestDefenseRating = attackType.DefenseRating.Value;
                            bestAttackType = attackType;
                        }
                    }
                }
                else
                {
                    bestDefenseRating = attackType.DefenseRating.Value;
                    bestAttackType = attackType;
                }
            }
        }

       


        /// <summary>     
        /// To re-score historical data, we would need to store polulation numbers, no of items etc.... so this is not possible.
        /// </summary>
        /// <param name="timepointIndex"></param>
        /// <param name="rating"></param>
        /// <param name="text"></param>
        public void ComposeRatingBreakdown(float rating, float assets, int noOfMembers, int injuries, int deaths, float injuryContribution, float deathContribution, 
            int noOfWeaponCarriers, int noOfWeapons, float totalHandWeaponsScore, float handWeaponsAverageRating, float finalHandWeaponsRating,
            int noOfDefensiveAgents, float totalAgentsRating, float agentsAverageRating, float finalAgentsRating) 
        {          
            StringBuilder text = new StringBuilder();

            Security sec = GameData.Instance.AIConstants.Ratings.Security;
            int maxWeapons = (int)(sec.MaxHandWeaponsToScorePerMember * noOfWeaponCarriers);
            Common.AppendLine(text, "How well the colony provides security:");
            AppendComponent(text, "COLONY SECURITY CONDITIONS", rating, omitIfZero: false, formatAsPercentage: true); //was "COLONY SECURITY RATING"

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            Common.AppendLine(text, "ASSETS (what needs protection)");
            Common.Append(text, "No. of colony members ");
            Common.Append(text, noOfMembers.ToString(), true);
            Common.AppendLine(text);
            Common.Append(text, "Total: ");
            Common.AppendFormat(text, "{0:N2}", true, assets);
            Common.AppendLine(text);
            Common.AppendLine(text);

            if (noOfDefensiveAgents > 0)
            {
                Common.AppendLine(text, "DEFENDERS");               
                Common.Append(text, "No. of defenders ");
                Common.Append(text, noOfDefensiveAgents.ToString(), true);
                Common.Append(text, ", \ncombined Security rating ");//was defense rating
                Common.AppendFormat(text, "{0:N2}", true, totalAgentsRating);
                Common.AppendLine(text);
                AppendAssets(assets, finalAgentsRating, text);
                text.Append("Subscore: +");
                Common.AppendLine(text, Common.PercentageToString(finalAgentsRating, useColoring: true));
                Common.AppendLine(text);
            }

            Common.AppendLine(text, "HAND WEAPONS");
            Common.Append(text, "No. of usable hand weapons ");
            Common.Append(text, noOfWeapons.ToString(), true);
            Common.Append(text, " (max ");
            Common.Append(text, maxWeapons.ToString(), true);
            Common.Append(text, "), \ncombined Security rating ");//was defense rating
            Common.AppendFormat(text, "{0:N2}", true, totalHandWeaponsScore);
            Common.AppendLine(text);
            AppendAssets(assets, finalHandWeaponsRating, text);
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(finalHandWeaponsRating, useColoring: true));

         
            /*

            if (noOfDefensiveAgents > 0)
            {
                Common.AppendLine(text, "DEFENDERS");
             //   Common.AppendLine(text, string.Format("No. of defenders {0}, Avg. quality {1:N2}", noOfDefensiveAgents, agentsAverageRating));
                Common.Append(text,"No. of defenders ");
                Common.Append(text, noOfDefensiveAgents.ToString(), true);
                Common.Append(text, ", Avg. quality ");
                Common.AppendFormat(text, "{0:N2}", true, agentsAverageRating);
                Common.AppendLine(text);
                text.Append("Subscore: +"); //was sub-rating
                Common.AppendLine(text, Common.PercentageToString(totalAgentsRating, useColoring: true));
                Common.AppendLine(text);
            }

            Common.AppendLine(text, "HAND WEAPONS");
            //Common.AppendLine(text, string.Format("No. of usable hand weapons {0} (max {1}), Avg. quality {2:N2}", noOfWeapons, maxWeapons, handWeaponsAverageRating)); //mp was "No. of usable hand weapons {0}, max {1}, Quality {2:N2}"
            Common.Append(text, "No. of usable hand weapons ");
            Common.Append(text, noOfWeapons.ToString(), true);
            Common.Append(text, "(max ");
            Common.Append(text, maxWeapons.ToString(), true); 
            Common.Append(text, "), Avg. quality ");
            Common.AppendFormat(text, "{0:N2}", true, handWeaponsAverageRating);              
            Common.AppendLine(text);
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(totalHandWeaponsScore, useColoring: true));
            */
         
            ComposeDeathsBreakdown(noOfMembers, injuries, deaths, injuryContribution, deathContribution, text, sec);
            
            ratingsBreakdown = text.ToString();
        }

        private static void AppendAssets(float assets, float finalAgentsRating, StringBuilder text)
        {
            Common.Append(text, "Divided by assets (");
            Common.AppendFormat(text, "{0:N2}", true, assets);
            Common.Append(text, "): ");
            Common.AppendFormat(text, "{0:N2}", true, finalAgentsRating);
            Common.AppendLine(text);
        }

       


        /// <summary>
        /// returns 0 - 1      
        /// </summary>
      /*  private void ScoreRating()
        {
            GatherWeaponsData();

          

            ScoreRating(); // out rating, out noOfMembers, out injuries, out deaths);

            
        }*/

     /*   public int GetLatestDataIndex()
        {
            return weaponStocks.Count - 1;
        }*/

     /*   private DateAndTime.TimeDateYear GetTimeDateYear(int timepointIndex)
        {
            return weaponStocks[timepointIndex].Time;
        }*/


        

        /// <summary>
        /// NEW: can only score at the current time. Otherwise we need to store too much data.
        /// </summary>
        /// <param name="timePointIndex"></param>
        /// <param name="rating"></param>
        /// <param name="noOfMembers"></param>
        /// <param name="relevantInjuries"></param>
        /// <param name="relevantDeaths"></param>
        protected override float ScoreRating() ///*int timePointIndex,*/ out float rating, out int noOfMembers, out int relevantInjuries, out int relevantDeaths) //out float injuryContribution, out float deathsContribution)
        {
            int noOfMembers = GetMembers();
            float assets = GetAssets();

            Dictionary<EntityType, int> ammoRounds = GatherAmmoData();
            int noOfDefensiveAgents;
            float agentsAverageRating, totalAgentsRating, finalAgentsRating;
            ScoreDefensiveAgents(assets, ammoRounds, out noOfDefensiveAgents, out agentsAverageRating, out totalAgentsRating, out finalAgentsRating);

            int noOfWeaponCarriers, noOfWeapons; 
            float totalHandWeaponsRating, finalHandWeaponsRating, weaponsAverageRating;
            ScoreHandWeapons(assets, ammoRounds, out noOfWeaponCarriers, out noOfWeapons, out weaponsAverageRating, out totalHandWeaponsRating, out finalHandWeaponsRating);


            float rating;
            int relevantInjuries, relevantDeaths;


            float totalInjuryContribution, finalInjuryContribution;
            float totalDeathsContribution, finalDeathsContribution;
            ComputeInjuriesAndDeaths(assets, out relevantInjuries, out relevantDeaths, out totalInjuryContribution, out totalDeathsContribution, 
                out finalInjuryContribution, out finalDeathsContribution);
            
          
          //  rating = totalHandWeaponsRating + totalAgentsRating - deathsContribution - injuryContribution;
            rating = finalHandWeaponsRating + finalAgentsRating - finalDeathsContribution - finalInjuryContribution;

            rating = Common.Clamp(rating, 0f, 1f);

            // different from food shared rating because the security events of other people affects this
            AddSharedRating(rating);


            if (composeBreakdown)
            {
                ComposeRatingBreakdown(rating, assets, noOfMembers, relevantInjuries, relevantDeaths, totalInjuryContribution, totalDeathsContribution,
                    noOfWeaponCarriers, noOfWeapons, totalHandWeaponsRating, weaponsAverageRating, finalHandWeaponsRating,
                    noOfDefensiveAgents, totalAgentsRating, agentsAverageRating, finalAgentsRating);

            }


            return rating;
                       
        }

       


        #region ISnapshot

        Snapshotter.Version version;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.SharedRating = sn.DoList(SharedRating);



            return this;
        }



        #endregion
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Jobs
{
    public class ThreatJob : AttackJob
    {        
        /// <summary>
        /// a rating of the threat for the colony. takes into account nearness to colony assets.
        /// </summary>
        public double ThreatRating;


        bool isVermin;
        /// <summary>
        /// a catch-all for threats against things other than people. These will often have a threat rating below the limit, but some agents will target them
        /// </summary>
        public bool IsVermin
        {
            get
            {
                return isVermin;
            }

            set
            {
                if (value != isVermin)
                {
                    EntityGroup entityGroup;
                    if (ResolveOwner(out entityGroup))
                    {
                        // switch...
                        if (isVermin)
                        {
                            entityGroup.AssetThreatJobs.Remove(this);     
                            entityGroup.AssetThreatJobsByTarget.Remove(this.Target.Value);  
                            
                            entityGroup.ThreatJobs.Add(this);
                            entityGroup.ThreatJobsByTarget.Add(this.Target.Value, this);                           
                        }
                        else
                        {
                            entityGroup.ThreatJobs.Remove(this);    
                            entityGroup.ThreatJobsByTarget.Remove(this.Target.Value);

                            entityGroup.AssetThreatJobs.Add(this);
                            entityGroup.AssetThreatJobsByTarget.Add(this.Target.Value, this);          
                        }
                    }

                    isVermin = value;
                }
            }
        }

        /// <summary>
        /// the danger of the entity, used in compiling threat maps that tell agents how wide a berth to keep.
        /// factors: aggression, species, current physical state...
        /// </summary>
      //  public double DangerLevel;

        /// <summary>
        /// used to trigger running/slowing of other agents to coordinate attacks, aid each other etc.
        /// </summary>
        public EntityID? ClosestMemberOfAllegiance;

        /// <summary>
        /// 0 - 1
        /// high urgency makes an agent move faster towards the weapon/target
        /// </summary>
       // public float Urgency = 0f;

        public ThreatJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }


        public ThreatJob(Entity entity, EntityGroup entityGroup, bool isAssetThreatOnly)
            : base(entityGroup, false)
        {
            Target = entity.EntityID;

            isVermin = isAssetThreatOnly;

            // NOW we can add to the jobs list:
            entityGroup.AddJob(this);

            if (IsVermin)
            {
                maxTakers = GameData.Instance.AIConstants.Combat.MaxTakersForVerminThreatJob;
            }
            else
            {
                float strengthRatio =
                    entity.StrengthRating.Value / GameData.Instance.Constants.StrengthRatings[entityGroup.GetAllegiance().RepresentativeEntityType.IntelligenceType.StrengthRating];

                float jobTakers = GameData.Instance.AIConstants.Combat.JobTakersPerStrengthRatio * strengthRatio;

                maxTakers = (int)Math.Round(Common.Clamp(jobTakers, 2f, GameData.Instance.AIConstants.Combat.MaxTakersForThreatJob));

                // an entity that is twice as strong should allow 4 takers:
              //  maxTakers =  (int)MathHelper.Lerp(2f, GameData.Instance.AIConstants.Combat.MaxTakersForThreatJob, strengthRatio);
                //maxTakers = GameData.Instance.AIConstants.Combat.MaxTakersForThreatJob; // // 8; 
            }

            ComputeJobType();
            SetDefaultPriority(entityGroup);

        }


        public override double GetWeaponPolicyScore(Entity entity, IKnownEntityData weapon, AttackType attackType)
        {
            double policyScore = 1f;
           
            if (IsVermin)
            {
                // threat jobs belong to an allegiance, not an expedition... so use the attacker's expedition policy instead of the allegiance
                Expedition expedition = entity.Intelligence.CurrentExpedition; // owner.GetExpedition();
                if (expedition != null && expedition.Policy != null)
                {
                    policyScore = expedition.Policy.GetWeaponPolicyScore(true, weapon.EntityType, attackType);
                }
            }           

            return policyScore;
        }


        /// <summary>
        /// 0: low urgency (move slower towards it?)
        /// 0.5: normal
        /// 1: high urgency (move faster)
        /// 
        /// an urgent threat makes the agents move faster to intercept or deal with it.
        /// we would like to see agents running to come to each other's aid, NOT storm towards the enemy on their own
        /// 
        /// urgent when:
        /// target is attacking a member of the allegiance
        /// 
        /// target is near allegiance's assets (camp, food stores etc.)
        /// 
        /// not urgent when:
        /// we are being attacked
        /// we are the nearest allegiance member to the threat - don't storm towards the enemy
        /// </summary>
        /// <returns></returns>
        public float GetUrgency(Entity entity)
        {
            if (ClosestMemberOfAllegiance.HasValue)
            {
                if (ClosestMemberOfAllegiance == entity.EntityID)
                {
                    return 0.4f; // slow down..?
                }
                else
                {
                    return 1f;
                    //TODO: scale urgency depending on how far we are from the action
                    //float urgency = MathHelper.Lerp(0.5f, 1f, Common.DistanceOctile(Target)
                }
            }

            return 0.5f;
        }

        private int maxTakers;

        /// <summary>
        /// should relate to threat rating, but capped at 8...
        /// </summary>
        public override int MaxJobPositions
        {
            get
            {
                return maxTakers;
            }
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.ClosestMemberOfAllegiance = sn.DoEnumNullable(ClosestMemberOfAllegiance);
            this.ThreatRating = sn.DoDouble(ThreatRating);

            this.IsVermin = sn.DoBool(IsVermin);
            this.maxTakers = sn.DoInt32(maxTakers);

            return this;

        }

    }
}

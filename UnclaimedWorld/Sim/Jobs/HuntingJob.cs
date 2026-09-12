using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    public class HuntingJob : AttackJob
    {
        
        /// <summary>
        /// the sum of the approximate strength of the current job takers
        /// </summary>
        public double StrengthOfCurrentTakers;

        /// <summary>
        /// only start the hunt when we exceed this level!
        /// </summary>
        public double StrengthOfTarget;

        // needed...?
        //public int MinimumTakers;

        public bool IsSpecificJob = false;

        public ZoneID? HuntZone;
        public EntityType TargetCreatureType;

        /// <summary>
        /// the danger of the entity, used in compiling threat maps that tell agents how wide a berth to keep.
        /// factors: aggression, species, current physical state...
        /// </summary>
      //  public double DangerLevel;

        public HuntingJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public HuntingJob(EntityID? target, EntityType targetCreatureType, EntityGroup entityGroup, ZoneID? huntZone = null)
            : base(entityGroup, false)
        {
            Target = target;
           // EntityTypeToHunt = entityTypeToHunt;          
            this.HuntZone = huntZone;
            this.TargetCreatureType = targetCreatureType;

            IsSpecificJob = true;

            // add to group now (this depends on EntityTypeToHunt):
            entityGroup.AddJob(this);

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

     /*   public HuntingJob(EntityType entityTypeToHunt, List<Job> belongsTo)
            : base(belongsTo)
        {
            EntityTypeToHunt = entityTypeToHunt;
            Entity = null;

            IsSpecificJob = false;
        }*/


        public override void GetLocation(out Point? tilePos, out EntityID? targetEntity, out ZoneID? zoneID)
        {
            tilePos = null;
            zoneID = null;

            targetEntity = Target;
        }

        public GoalEvaluator.CalculateResult ScoreThisJob(ref double rating)
        {
            rating = 1f;
            return GoalEvaluator.CalculateResult.Done;
        }

        public override void Abandon(Entity entity, bool isDestroyingJob = false)
        {
            base.Abandon(entity, isDestroyingJob);

            if (!IsSpecificJob)
            {
                Target = null;
            }
        }
        public override string GetName()
        {
            return "Hunting";
        }
        

#region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

           // this.EntityTypeOutput = sn.DoGameData(EntityTypeOutput);          
            this.IsSpecificJob = sn.DoBool(IsSpecificJob);          
            this.StrengthOfCurrentTakers = sn.DoDouble(StrengthOfCurrentTakers);
            this.StrengthOfTarget = sn.DoDouble(StrengthOfTarget);

            this.HuntZone = sn.DoEnumNullable(HuntZone);
            this.TargetCreatureType = sn.DoGameData(TargetCreatureType);

            return this;
        }


#endregion

    }
}

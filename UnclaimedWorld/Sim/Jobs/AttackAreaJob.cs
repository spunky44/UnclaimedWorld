using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// similar to a patrol job, but is destroyed after a while. The clock starts when first reached by an agent.
    /// </summary>
    public class AttackAreaJob : CombatAreaJob
    {
        public bool AttackVermin;
        public bool AttackThreats;

        private double? /* TimeSpan?*/ areaReachedTimePoint;

        CollideShape2D area;
        List<Pair<Entity, Vector2>> threatsInArea = new List<Pair<Entity,Vector2>>();

        public override string GetName()
        {
            return "Attacking";
        }


        public AttackAreaJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

         public AttackAreaJob(Zone zone, EntityGroup entityGroup, bool attackVermin, bool attackThreats, int noOfPatrollers)
             : base(zone, entityGroup, noOfPatrollers)
         {
             this.AttackVermin = attackVermin;
             this.AttackThreats = attackThreats;

             CreateArea();
         }

         private void CreateArea()
         {
             Vector3 topLeft = MapManager.TileToWorldPos(ThreatArea.Location);
             Vector3 lowerRight = MapManager.TileToWorldPos(new Point(ThreatArea.Right, ThreatArea.Bottom));
             area = new Collisions.CollideShape2D(topLeft.Y, topLeft.X, lowerRight.Y, lowerRight.X);
         }

       //  const float durationInSeconds = 10;

         public bool IsDurationReached()
         {
             if (areaReachedTimePoint != null)
             {
                 return The.Sim.TimepointReached(areaReachedTimePoint + GameData.Instance.AIConstants.MinimumSearchTimeInAttackZone);
             }
             else
             {
                 return false;
             }

         }

        public bool AreaContainsThreats()
        {
            EntityGroup owner;
            if (ResolveOwner(out owner))
            {
                The.AgentQuadTree.GetObjectsIntersectingBounds(area,
                    e => owner.ThreatJobsByTarget.ContainsKey(e.ID) || (AttackVermin && owner.AssetThreatJobsByTarget.ContainsKey(e.ID)),
                    ref threatsInArea);

                bool containsThreats = threatsInArea.Count > 0;

                threatsInArea.Clear();

                return containsThreats;
            }

            return false;
        }


         public void SetAreaReached()
         {
             if (areaReachedTimePoint == null)
             {
                 areaReachedTimePoint = The.Sim.TotalUnPausedGameTimeInSeconds;
             }
         }

         public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
         {
             //remove the zone order
             if (Zone != null) 
             {
                 if (Zone.AttackAreaJob != null)
                 {
                     Zone.AttackAreaJob = null;
                 }
             }

             base.Destroy(removeTakers, entityToExcludeFromCancel);

         }

         public override bool CanAttackVermin
         {
             get { return AttackVermin; }
         }

         public override bool CanAttackTargetsOutsideZone
         {
             get { return true; } // is more aggressive?
         }

        #region ISnapshot

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

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.AttackThreats = sn.DoBool(AttackThreats);
            this.AttackVermin = sn.DoBool(AttackVermin);

           // this.areaReachedTimePoint = sn.DoTimeSpanNullable(areaReachedTimePoint);
            this.areaReachedTimePoint = sn.DoDoubleNullable(areaReachedTimePoint);

            sn.Ignore(area);
            sn.Ignore(threatsInArea);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            CreateArea();

            base.LoadPostProcess(sn);

        }


        #endregion
    }
}

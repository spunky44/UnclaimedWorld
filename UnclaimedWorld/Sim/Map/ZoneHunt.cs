using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    public class ZoneHunt: ISnapshot
    {
        public bool RemoveAfterFirstSuccessfulHunt;

        /// <summary>          
        ///        
        ///
        /// </summary>
        public Dictionary<EntityType, int> CreaturesToHunt = new Dictionary<EntityType, int>();
        //  Dictionary<EntityType, int> snapshotFindPreyJobs;

        /// <summary>
        /// was: FindPreyJob
        /// 
        /// Managed by OtherJobManager
        /// 
        /// The find prey job will check the zone to see the creature types it is permitted to hunt.
        /// 
        /// Each job has one taker as usual.
        /// </summary>
        public List<FindPreyJob> FindPreyJobs = new List<FindPreyJob>();
        List<JobID> snapshotFindPreyJobs;


       // public Dictionary<EntityType, double?> TimePointForLastUnsuccessfulHunt = new Dictionary<EntityType, double?>();

       /// <summary>
       /// the time when we can create the next find prey job. The prurpose is to create delays between hunting trips. Gets reset immediately on a succesful hunt..?
       /// </summary>
        public Dictionary<EntityType, double> TimePointForNextHunt = new Dictionary<EntityType, double>();
       // public Dictionary<EntityType, double?> TimePointForNextHunt = new Dictionary<EntityType, double?>();

        /// <summary>
        /// we store the interval too, so we can increase it for each unsuccesful hunt
        /// </summary>
        public Dictionary<EntityType, double> NextHuntInterval = new Dictionary<EntityType, double>();
       // public Dictionary<EntityType, double?> NextHuntInterval = new Dictionary<EntityType, double?>();


        /* public Dictionary<EntityType, List<FindPreyJob>> FindPreyJobs = new Dictionary<EntityType, List<FindPreyJob>>();
         Dictionary<EntityType, List<JobID>> snapshotFindPreyJobs;
         */

        /// <summary>
        /// TODO: allow a slider to change this
        /// there should be a max no. of hunt jobs in the zone. This number can be set with a slider in the GUI. Do the same in Patrol zones.
        /// max hunters. Max value could scale with zone size.
        /// 
        /// Each job has one taker as usual.
        /// </summary>
        public int MaxHuntJobs = 1;

        /// <summary>
        /// Critter type!!
        /// signals to the JobManager if it can create hunt jobs here. Also, the zone may not be auto-destroyed if it contains these
        /// 
        /// NOTE: may not actually mean that we are currently hunting this creature. There has to be an ordered amount in Orders too
        /// </summary>
        public HashSet<EntityType> AllowStandingOrderHunt = new HashSet<EntityType>();

        public bool HasFindPreyJobs()
        {
            if (FindPreyJobs != null)
            {
                if (FindPreyJobs.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasDirectOrders()
        {
            return CreaturesToHunt.Any(kvp => kvp.Value > 0);
        }

        public bool AllowsStandingOrders()
        {
            return AllowStandingOrderHunt.Count > 0;
        }


        public bool HasHuntOrders()
        {
            return HasDirectOrders() || AllowsStandingOrders();
        }

        /// <summary>
        /// does not check inventory for standing orders.
        /// </summary>
        /// <param name="creatureType"></param>
        /// <returns></returns>
        public bool HasAnyHuntOrders(EntityType creatureType)
        {
            int value;
            if (CreaturesToHunt.TryGetValue(creatureType, out value))
            {
                if (value > 0)
                    return true;
            }
          
            if (AllowStandingOrderHunt.Contains(creatureType))
            {
                return true;
            }           

            return false;
        }

        /// <summary>
        /// checks if the standing order is active - there has to be an inventory shortfall
        /// </summary>
        /// <param name="creatureType"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool HasUnfulfilledHuntOrders(EntityType creatureType, EntityGroup owner)
        {
            int value;
            if (CreaturesToHunt.TryGetValue(creatureType, out value))
            {
                if (value > 0)
                    return true;
            }

            if (owner.ProductionOrders != null) // creature allegiances don't use standing orders...
            {
                if (AllowStandingOrderHunt.Contains(creatureType)
                    && owner.StandingOrderJobIsNeeded(creatureType.BiologicalType.CarcassType))
                {
                    return true;
                }
            }

            return false;
        }

        public void CancelJobs()
        {
            for (int i = FindPreyJobs.Count - 1; i >= 0; i--)
            {
                FindPreyJob job = FindPreyJobs[i];

                job.Destroy(true);              

            }
        }


        public void RegisterUnsuccessfulHunt(EntityGroup owner)
        {
            HashSet<EntityType> creaturesWithHuntOrders = GetCreaturesWithHuntOrders(owner);

            double lastInterval;
           
           // double timeForNextHunt = The.Sim.TotalUnPausedGameTimeInSeconds + GameData.Instance.AIConstants.CooldownTimeAfterUnsuccessfulHunt;

            if (creaturesWithHuntOrders != null)
            {
                foreach (var item in creaturesWithHuntOrders)
                {
                    double interval;
                    if (NextHuntInterval.TryGetValue(item, out lastInterval))
                    {
                        double newInterval = GameData.Instance.AIConstants.IncreaseCooldownTimeFactorAfterUnsuccessfulHunt * lastInterval;
                        newInterval = Common.ClampTop(newInterval, GameData.Instance.AIConstants.MaxCooldownTimeAfterUnsuccessfulHunt);
                        interval = newInterval;                     
                    }
                    else                         
                    {
                        interval = GameData.Instance.AIConstants.CooldownTimeAfterUnsuccessfulHunt;
                    }

                    SetTimePointForNextHunt(item, interval);

                    NextHuntInterval[item] = interval;
                    
                }
            }
        }

        private void SetTimePointForNextHunt(EntityType item, double interval)
        {
            double timeForNextHunt = The.Sim.TotalUnPausedGameTimeInSeconds + interval;
            TimePointForNextHunt[item] = timeForNextHunt;
        }

        public void NotifyHasFoundPrey(EntityType creature)
        {
            // put the zone on cooldown so we don't create a new find prey job while GoalHunt is executing:
            SetTimePointForNextHunt(creature, GameData.Instance.AIConstants.CooldownTimeAfterSpottingPrey);

        }

        public void NotifySuccessfulHunt(EntityType creature, Zone zone)
        {
            NextHuntInterval.Remove(creature);
            ResetTimepoint(creature); // OK to start a new find prey job

            Common.RemoveFromDictWithSums(CreaturesToHunt, creature); 


            if (this.RemoveAfterFirstSuccessfulHunt)
            {
                zone.Destroy();
            }
            else
            {
                zone.RemoveZoneOrFireOrdersChangedEvent(); 
            }
        }

        private HashSet<EntityType> GetCreaturesWithHuntOrders(EntityGroup owner)
        {
            HashSet<EntityType> creatures = null;
            EntityType representativeType = owner.GetAllegiance().RepresentativeEntityType;
            foreach (var item in representativeType.IntelligenceType.PreyTypes)
	        {
                if (HasUnfulfilledHuntOrders(item, owner))
                {
                    Common.AddToSet(ref creatures, item);
                }	 	
            }

            return creatures;
        }

        public bool HuntIsOnCooldown(EntityType creatureType)
        {
            return TimePointForNextHunt.ContainsKey(creatureType);

        }

        public bool HasOrdersNotOnCooldown()
        {
            if (HasHuntOrders())
            {
                foreach (var item in CreaturesToHunt)
                {
                    if (item.Value > 0 && !HuntIsOnCooldown(item.Key))
                    {
                        return true;
                    }
                }

                foreach (var item in AllowStandingOrderHunt)
                {
                    if (!HuntIsOnCooldown(item))
                    {
                        return true;
                    }
                }

                return false;
            }
            else 
            {
                return false;
            }
        }

       /* public bool AllowsHunting(EntityType creatureType)
        {
            double? timepoint;
            TimePointForLastUnsuccessfulHunt.TryGetValue(creatureType, out timepoint);
            if (timepoint.HasValue)
            {

            }


        }*/


        public void ResetTimepoint(EntityType creatureType)
        {
            //TimePointForNextHunt[creatureType] = null;
            TimePointForNextHunt.Remove(creatureType);
           // NextHuntInterval.Remove(creatureType);
        }

        public void Destroy()
        {
            if (FindPreyJobs != null)
            {
                for (int i = FindPreyJobs.Count - 1; i >= 0; i--)
                {
                    Job job = FindPreyJobs[i];
                    job.Destroy(true);
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
            this.AllowStandingOrderHunt = sn.DoHashSet(AllowStandingOrderHunt);
            this.CreaturesToHunt = sn.DoDictionary(CreaturesToHunt);
            this.MaxHuntJobs = sn.DoInt32(MaxHuntJobs);
            this.RemoveAfterFirstSuccessfulHunt = sn.DoBool(RemoveAfterFirstSuccessfulHunt);

            this.TimePointForNextHunt = sn.DoDictionary(TimePointForNextHunt);
            this.NextHuntInterval = sn.DoDictionary(NextHuntInterval);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (FindPreyJobs != null)
                {
                    snapshotFindPreyJobs = new List<JobID>();
                    foreach (var item in FindPreyJobs)
                    {
                        snapshotFindPreyJobs.Add(item.ID);
                    }
                }
            }

            snapshotFindPreyJobs = sn.DoList(snapshotFindPreyJobs);


            sn.Ignore(FindPreyJobs);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (snapshotFindPreyJobs != null)
            {
                FindPreyJobs = new List<FindPreyJob>();
                foreach (var item in snapshotFindPreyJobs)
                {
                    FindPreyJobs.Add((FindPreyJob)LookUp<Job, JobID>.FindByID(item));
                }
            }
          
        }

        #endregion


    }
}

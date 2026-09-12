using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Jobs.JobTypes
{
    /// <summary>
    /// could these be overridden by process types, so selecting them does not affect a process type with a more specific job type
    /// </summary>
    public enum StaticJobTypes
    {
        Patrol, Hunt, Examine, Scout, HaulToStorage, /*Gathering,*/ Repairing, Salvaging, Upgrading,
        AttackArea
    }

    public class StaticJobType: JobType
    {
        public StaticJobTypes StaticJobTypeSetting;
        

        public override void IterateJobs(Entities.EntityGroup entityGroup, Action<Job> iterateFunction)
        {
            switch (StaticJobTypeSetting)
            {
                case StaticJobTypes.Patrol:
                    {
                        foreach (var item in entityGroup.PatrolJobs)
                        {
                            iterateFunction(item);
                        }

                        return;
                    }
                case StaticJobTypes.AttackArea:
                    {
                        foreach (var item in entityGroup.AttackAreaJobs)
                        {
                            iterateFunction(item);
                        }

                        return;
                    }
                case StaticJobTypes.Scout:
                case StaticJobTypes.Examine:
                    {
                        foreach (var item in entityGroup.ScoutingJobs)
                        {
                            if (IsType(item))
                            {
                                iterateFunction(item);
                            }
                        }
                        return;
                    }
                case StaticJobTypes.Hunt:
                    {
                        foreach (var item in entityGroup.FindPreyJobs)
                        {
                            iterateFunction(item);
                        }

                        foreach (var item in entityGroup.OtherJobs) // hunting jobs...
                        {
                            if (IsType(item))
                            {
                                iterateFunction(item);
                            }
                        }

                        return;
                    }
                case StaticJobTypes.Upgrading:
                    {                      
                        foreach (var item in entityGroup.OtherJobs) 
                        {
                            if (IsType(item))
                            {
                                iterateFunction(item);
                            }
                        }

                        return;
                    }
                case StaticJobTypes.Repairing:
                    {
                        foreach (var item in entityGroup.RepairJobs) // .OtherJobs) 
                        {
                            foreach (var job in item.Value)
                            {
                                if (IsType(job))
                                {
                                    iterateFunction(job);
                                }
                            }
                        }

                        return;
                    }
                case StaticJobTypes.HaulToStorage:
                    {
                        foreach (var item in entityGroup.HaulingJobs)
                        {
                            if (IsType(item))
                            {
                                iterateFunction(item);
                            }
                        }
                        return;                        
                    }

             /*   case StaticJobTypes.Gathering:               
                    {
                        foreach (var item in entityGroup.ProductionJobs)
                        {
                            foreach (var job in item.Value)
                            {
                                if (IsType(job))
                                {
                                    iterateFunction(job);
                                }
                            }                          
                            
                        }

                        return;
                    }*/
                case StaticJobTypes.Salvaging:
                    {
                        foreach (var job in entityGroup.OtherJobs)
                        {
                            if (IsType(job))
                            {
                                iterateFunction(job);
                            }
                        }

                        return;

                    }
            }
        }


        public override bool IsType(Job job)
        {
            switch (StaticJobTypeSetting)
            {
                case StaticJobTypes.Scout:
                    {
                        ScoutingJob sJob = job as ScoutingJob;
                        return sJob != null && sJob.Examine == false;
                    }
                case StaticJobTypes.Examine:
                    {
                        ScoutingJob sJob = job as ScoutingJob;
                        return sJob != null && sJob.Examine == true;
                    }
                case StaticJobTypes.Patrol:
                    {
                        return job is PatrolJob;
                    }
                case StaticJobTypes.AttackArea:
                    {
                        return job is AttackAreaJob;
                    }
                case StaticJobTypes.HaulToStorage:
                    {
                        HaulingJobSpecificItem haul = job as HaulingJobSpecificItem;
                        if (haul != null && haul.IsHaulJobToStorage)
                        {
                            return true;
                        }
                        break;
                    }
                case StaticJobTypes.Hunt:
                    {
                        return job is FindPreyJob || job is HuntingJob;
                    }
              //  case StaticJobTypes.Gathering:
                case StaticJobTypes.Salvaging:
                case StaticJobTypes.Repairing:
                case StaticJobTypes.Upgrading:
                    {
                        ProcessJob pJob = job as ProcessJob;
                        if (pJob != null)
                        {
                           /* if (StaticJobTypeSetting == StaticJobTypes.Gathering && pJob.HarvestJob != null)
                            {
                                return true;
                            }*/

                            if (StaticJobTypeSetting == StaticJobTypes.Salvaging && pJob.SalvageJob != null)
                            {
                                return true;
                            }

                            if (StaticJobTypeSetting == StaticJobTypes.Repairing && pJob.RepairJob != null)
                            {
                                return true;
                            }

                            if (StaticJobTypeSetting == StaticJobTypes.Upgrading && pJob.GetIsUpgradeJob() == true) // .UpgradeJob != null)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
            }

            return false;           
        }


        /// <summary>
        /// should correspond to ingame captions
        /// </summary>
        /// <returns></returns>
        public override string GetDefaultDisplayName()
        {
            switch (StaticJobTypeSetting)
            {
                case StaticJobTypes.Patrol: return "Patrolling";
                case StaticJobTypes.AttackArea: return "Attacking";
                case StaticJobTypes.Hunt: return "Hunting";
                case StaticJobTypes.Examine: return "Examining";
                case StaticJobTypes.Scout: return "Scouting";
                case StaticJobTypes.HaulToStorage: return "Hauling to storage";
              //  case StaticJobTypes.Gathering: return "Gathering";
                case StaticJobTypes.Repairing: return "Doing maintenance";  //mp was: "Repairing" which was confusing because it doesn't work on broken structures
                case StaticJobTypes.Salvaging: return "Salvaging";
                case StaticJobTypes.Upgrading: return "Upgrading";

                default: return "No display string for:" + StaticJobTypeSetting.ToString();
            }
        }

    }
}

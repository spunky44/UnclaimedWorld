using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// changes the standing production order for some item
    /// 
    /// used by: production manager, gather, hunt
    /// </summary>
    public class SetStandingOrder : Control.Commands.Command
    {
        /// <summary>
        /// the owner of the job
        /// </summary>
        public long ExpeditionID;
       // public long EntityGroupID;


        public string EntityTypeKey; // store string for serialization...
        public int NewCount;

        public bool GiveClientFeedback;

        public SetStandingOrder()
        {

        }

        public SetStandingOrder(ExpeditionID expeditionID, /* EntityGroupID entityGroupID,*/ string entityTypeKey, int newCount, bool giveClientFeedback)
        {
            this.EntityTypeKey = entityTypeKey;
            this.NewCount = newCount;
            this.ExpeditionID = (long)expeditionID;

            this.GiveClientFeedback = giveClientFeedback;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool setProductionSuccesful = DoSetStandingOrder();

            if (giveClientFeedback && GiveClientFeedback 
                && setProductionSuccesful)
            {
                The.Client.SetProduction(EntityTypeKey);
            }
        }


        private bool DoSetStandingOrder()
        {
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
            
            EntityType entityType = GameData.Instance.AllEntityTypes[EntityTypeKey];

            expedition.OwnedEntities.ProductionOrders.SetStandingOrder(entityType, NewCount);

          /*  ProductionOrder order = expedition.OwnedEntities.ProductionOrders.Orders[entityType];

            order.AmountToKeepInStore = NewCount;
            order.ProductionJobsToComplete = null; // mutex
            */
            // this seems to set the same target on all outputs...
          /*  List<ProcessType> processesYieldingOutput;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out processesYieldingOutput)) // this retrieves all processes...
            {
                foreach (var process in processesYieldingOutput)
                {
                    if (process.Outputs != null)
                    {
                        foreach (var output in process.Outputs)
                        {
                            if (!output.IsWasteProduct) // never order waste products
                            {
                                expedition.OwnedEntities.ProductionOrders.Orders[output.FinalEntityTypeToCreate].ProductionJobsToComplete = NewCount;
                            }

                        }
                    }
                }              
            }*/

           // expedition.JobManager.UpdateProductionJobs(entityType);

            return true;
        }
    }
}

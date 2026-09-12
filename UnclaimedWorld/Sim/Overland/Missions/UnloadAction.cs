using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Overland.Missions
{
    public class UnloadAction: MissionAction
    {
        UnloadActionTemplate unloadActionType;
        private MissionActionTemplateID snapshotActionTemplateID;

        public UnloadAction()
        {

        }

        public UnloadAction(Mission /*MissionActions*/ parent, UnloadActionTemplate template)
             : base(parent) 
        {
            this.unloadActionType = template;    
          
        }

        

        public override bool Update(GameTime elapsed)
        {
            base.Update(elapsed);

            Unload();

            return true;
        }


        private bool LoadAsTradeGood(Entity entity) // EntityType entityType)
        {
            if (!Garrison.EntityBelongs(entity) 
                || entity.EntityType.IntelligenceType.ServantForEntityTypeTag != null) // dogs + robots //  entity.OwnedBy.HasValue)
            {
                return true;
            }

            return false;
        }

        private void Unload()
        {
            Allegiance destinationAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.CurrentLocation.Value.AllegianceID);  //The.Sim.World.GetAllegianceWithID(DestinationAllegiance);
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)parent.CurrentLocation.Value.ExpeditionID);

            Entity vehicleToUnload = parent.GetVehicleToLoad();


            // only unload goods, not passengers...
            // NEW: dogs also
            if (vehicleToUnload != null)
            {
                vehicleToUnload.Contains.IterateContained(e =>
                    {
                        if (LoadAsTradeGood(e)) // !Garrison.EntityBelongs(e))
                        {
                            vehicleToUnload.Contains.Uncontain(e); // this happens instantly and not in a goal, unlike GoalExit

                            if (e.AssignedToJob == parent.MissionJob)
                            {
                                e.AssignedToJob = null; // unassign (assign was done in LoadItems())
                            }
                        }
                    });
            }

           

            // fire events                
            if (destinationAllegiance.AllegianceType == AllegianceType.Player)
            {
                List<ActionSets> defaultActionSets;
                GameData.Instance.AllegianceEvents.TryGetValue(AllegianceEvents.CargoDeliveredToPlayer, out defaultActionSets);
                Goal.FireEventActions(vehicleToUnload, null, defaultActionSets, null);
            }



        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(unloadActionType);

            sn.Ignore(unloadActionType);

            return base.DoSnapshot(sn);


        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            unloadActionType = (UnloadActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);

            base.LoadPostProcess(sn);
        }
      

    }
}

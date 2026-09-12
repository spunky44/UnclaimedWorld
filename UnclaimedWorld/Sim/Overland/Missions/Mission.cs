using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Overland.Missions
{
    public enum MissionID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// describes an off-map mission. Is the instance part of a MissionType
    /// 
    /// shares function and data responsibility with MissionJob.
    /// </summary>
    public class Mission : ISnapshot, ILookUp<Mission, MissionID>, ICommunicates
    {
       // public bool IsActive;

        /// <summary>
        /// The Job/Mission can be owned by an NPC allegiance if the player has hired the vehicles!
        /// There is also MissionType.Allegiance as the owner of the mission...
        /// 
        /// Can the player sell items and lock them with this ID???
        /// 
        /// contains "Crew" in TakenBy..?
        /// 
        /// on missions that start on the play site, evaluators will determine who becomes the crew as normal. Off site, they will be assigned.
        /// </summary>
        public JobID MissionJob;

        #region defining properties

        public MissionTemplate MissionTemplate;
        private MissionTemplateID snapshotMissionTemplate;

        public MissionStop StartMissionStop;
        private MissionStopID snapshotStartMissionStop;
       
        public Transportation Transportation;

        #endregion


        #region progress fields

        /// <summary>
        /// move to MissionStop?
        /// </summary>
      //  public List<EntityID> Immigrants;

        /// <summary>
        /// contracts to buy/sell goods, being fulfilled by this mission
        /// </summary>
        public List<Contract> Contracts = new List<Contract>();
               
      
       
        /// <summary>
        /// contains the actions that are currently being executed. Also has the Travel action that may currently be executed
        /// 
        /// Is never null - even between sites.
        /// </summary>
        public MissionStop CurrentMissionStop;
        private MissionStopID snapshotCurrentMissionStop;
       
        /// <summary>
        /// is null when between sites!
        /// Gets set when we arrive at a site.
        /// </summary>
        public TravelLocation? CurrentLocation
        {
            get
            {
                if (CurrentMissionStop.TravelAction != null &&
                    CurrentMissionStop.TravelAction.Progress == TravelAction.OverlandProgress.BetweenSites)
                {
                    return null;
                }
                else
                {
                    return CurrentMissionStop.MissionStopTemplate.TravelLocation;
                }
            }
        }

        #endregion

      

        public Site Site
        {
            get
            {
                if (CurrentLocation != null)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance);
                    SharedKnowledge sharedKnowledge = thisAllegiance.SharedKnowledge;


                    Allegiance allegiance;
                    Expedition exp;
                    IKnownEntityData termin;
                    Site site;
                    if (CurrentLocation.Value.ResolveLocation(sharedKnowledge, out site, out allegiance, out exp, out termin))
                    {
                        return site; // allegiance.Site;
                    }
                }

                return null;
            }
        }

        GeodeticCoordinate? tempCoords;
        public Locations.GeodeticCoordinate? Coords
        {
            get 
            {
                Site currentSite = this.Site;
                if (currentSite != null)
                {
                    return currentSite.Coords;
                }

                MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
                tempCoords = null;

                if (job.Vehicles != null)
                {
                    job.IterateVehicles(e => GetCoords(e));                                        
                }

                if (tempCoords == null)
                {
                    job.TakenBy.Iterate(e => GetCoords(e));
                }

                return tempCoords;
            }
        }

        void CreateRegulators()
        {
           
        }


        public string GetMissionMarker()
        {
            string spriteName = "hiker_map_icon"; 

            if (MissionTemplate.TransportationType.Vehicles != null)
            {
                if (MissionTemplate.TransportationType.Vehicles != null &&
                    MissionTemplate.TransportationType.Vehicles.Count > 0)
                {
                    EntityType vehicle = GameData.Instance.AllEntityTypes[MissionTemplate.TransportationType.Vehicles[0].First];

                    spriteName = vehicle.WorldMapIcon ?? spriteName;
                }
            }

            return spriteName;

        }

        private void GetCoords(Entity entity)
        {
            GeodeticCoordinate? coords = entity.Coords;
            if (coords != null)
            {
                tempCoords = coords;               
            }
        }

        public string GetTooltip()
        {
            StringBuilder text = new StringBuilder();
            EntityType transport = GetMainTransportation();

            text.Append("Transportation: ");

            if (transport != null)
            {                
                Common.Append(text, transport.Name);
            }
            else
            {
                Common.Append(text, "On foot");
            }

            return text.ToString();

        }

        public bool CanCommunicate(CommunicationMethod method, double distance)
        {
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.MissionTemplate.Allegiance);

            if (thisAllegiance != null)
            {
                Site site;
                Allegiance allegiance;
                Expedition exp;
                IKnownEntityData terminal;
                if (CurrentLocation != null && CurrentLocation.Value.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out exp, out terminal))
                {
                    // at a site. we can use the base's equipment (test for friendly..?)
                    if (allegiance != null && allegiance.CanCommunicate(method, distance))
                    {
                        return true;
                    }
                }
            }

            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);

            tempCanCommunicate = false;
            job.IterateVehicles(e => CanCommunicate(e, method, distance));

            if (tempCanCommunicate)
            {
                return true;
            }

            // check the job takers:
            for (int i = 0; i < job.TakenBy.Count; i++)
            {
                Entity taker = job.TakenBy.Get(i);
                if (taker.CanCommunicate(method, distance))
                {
                    return true;
                }
            }

            return false;
        }

        private bool tempCanCommunicate = false;
        private void CanCommunicate(Entity entity, CommunicationMethod method, double distance)
        {
            if (entity.CanCommunicate(method, distance))
                tempCanCommunicate = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityGroup">The owner of the Job. This can be an NPC allegiance if the player has hired the vehicles</param>
        /// <param name="missionType">The owner of the Mission is an Allegiance, most often the player</param>
        /// <param name="vehicles"></param>
        public Mission(EntityGroup entityGroup, MissionTemplate missionType, Dictionary<EntityType, List<EntityID>> vehicles) 
        {
            this.MissionTemplate = missionType;

            AddToLookup();

            StartMissionStop = new MissionStop(this, missionType.StartMissionStopTemplate);

            Transportation = new Missions.Transportation(this); //, vehicles);

            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)missionType.Allegiance);

            // this assigns the vehicles immediately:
            MissionJob job = new Jobs.MissionJob(entityGroup, vehicles);
            this.MissionJob = job.ID;

            CurrentMissionStop = StartMissionStop;
           // CurrentLocation = StartMissionLocation.MissionLocationTemplate.TravelLocation;
            

            allegiance.AddMission(this);  
        }


        public Mission()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        /// <summary>
        /// perform transactions with terminals in range. Also pay for the mission itself.
        /// </summary>
        public void StartMission()
        {
            Transportation.StartMission();

            // invokes on all the actions:
            StartMissionStop.StartMission();

            
           
        }


        public bool IsOnPlaySite()
        {
            Entity vehicle = GetVehicleOrAgentToLoad();
           /* if (vehicle != null)
            {*/
                return vehicle.IsOnPlaySite();
           // }
           
        }

        public void Destroy()
        {
            RemoveIDEntry();

            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance);

            allegiance.RemoveMission(this);

            Job job = LookUp<Job, JobID>.FindByID(MissionJob);
            job.Destroy(true);
        }

        /// <summary>
        /// this method should be called when there has been a problem that means the mission cannot complete. 
        /// It should put the Return action at the top of the stack.
        /// </summary>
        public void Abort()
        {
            // show dialog
            // refund - penalty
            // turn back with goods

            // see if there are transactions that have not been done, but have been paid for:
            RefundOrders();

            // delayed ownership changes... change ownership back if needed

            //move back the money, add a penalty if needed


            FireAbortEvents();


            //skip all other stops and actions except the return Travel action
            RemoveAllStopsAndActionsBeforeReturnAction();

        }

        private void RefundOrders()
        {
            //1: we are transporting goods that have been paid for, but now we are unable to deliver them.
            //2. we have paid for goods, but now we won't be able to pick them up

            // the job owner is the same allegiance that sent the transport.
            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
            EntityGroup jobOwner;

            if (!job.ResolveOwner(out jobOwner))
                return;

            Entity cargoContainer = GetVehicleOrAgentToLoad();
            List<Entity> carriedCargo = cargoContainer.Contains.GetContainedItemsList(e => e.AssignedToJob == job.ID);

            foreach (var contract in Contracts)
            {
                // Buy case:
                IOwner buyer = LookUpOwners.FindByID((OwnerID)contract.ContractTemplate.BuyerID);

                // if we are not the buyer, a refund may be needed:
                if (buyer != jobOwner.Parent)
                {                  
                   
                    // see if all the goods are still in the inventory:
                    if (contract.TransferredEntities.All(e => carriedCargo.Contains(Entity.FindByID(e))))
                    {
                        contract.Revert();
                    }

                }
                else 
                {
                    // we are the owner - the goods must be in the inventory:
                }

                // Sell case (TODO):


            }

        }


       

        private void FireAbortEvents()
        {
            // fire events    
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance);

            if (allegiance.AllegianceType == AllegianceType.Player)
            {
                List<ActionSets> defaultActionSets;
                GameData.Instance.AllegianceEvents.TryGetValue(AllegianceEvents.TransportToPlayerAborted, out defaultActionSets);

                Goal.FireEventActions(null, null, defaultActionSets, null);
            }
        }

        private void RemoveAllStopsAndActionsBeforeReturnAction()
        {
            // if we are aborting at a site:
            // keep the current mission stop. Add a Travel Action to return home, and an unload action when we get there.
            // clear all other stops and actions.
            if (CurrentMissionStop.Actions.Count > 0)
            {
                CurrentMissionStop.Actions.Clear();
                CurrentMissionStop.MissionStopTemplate.Actions.Clear();
            }
   
            // get our original return destination.  or - perhaps get a differnet destination to abort to...
            MissionStop end = CurrentMissionStop.GetEnd();
                        

            if (end != CurrentMissionStop)
            {
                // re-route:

                // make sure end has an unload action.
                end.MissionStopTemplate.Actions.Clear();
                end.MissionStopTemplate.Actions.Enqueue(new UnloadActionTemplate(end.MissionStopTemplate, false));
                end.MissionStopTemplate.Actions.Enqueue(new DisembarkActionTemplate(end.MissionStopTemplate, false));

                CurrentMissionStop.MissionStopTemplate.TravelAction = new TravelActionTemplate(CurrentMissionStop.MissionStopTemplate, end.MissionStopTemplate);
                CurrentMissionStop.TravelAction = new TravelAction(this, CurrentMissionStop.MissionStopTemplate.TravelAction); // new TravelAction(this, end);

                // TODO: currently cannot select a route when from site and to site is the same - as when turning around mid way.     
                // should use the same route it was on.
                // Now, travel action will have distance = 0 and finish immediately. (exploit?)            
                //MissionTemplate.SelectRoutes(); 

                CurrentMissionStop.MissionStopTemplate.RecalculateNumbers();

                CurrentMissionStop.TravelAction.Start();
               

            }
            else
            {
                // already home...
            }

          /*  if (CurrentMissionStop.TravelAction != null)
            {*/
             //   CurrentMissionStop.RemoveAllStopsAndActionsBeforeReturnAction();

                // if aborting between sites (CurrentMissionStop is null):
                //Destroy the current travel action

          //  }           
           
            
        }

        private void RecalculateNumbers()
        {
            StartMissionStop.MissionStopTemplate.SetNumber(0);            
        }


       /* public bool IsInCommunicationRange(ICommunicates communicates) // allegiance)
        {
            CommunicationMethod? method;
            if (CurrentLocation != null)
            {
                Allegiance allegiance;
                Expedition exp;
                Entity terminal;
                if (CurrentLocation.ResolveLocation(out allegiance, out exp, out terminal))
                {                   
                    if (communicates.IsInCommunicationRange(allegiance, out method))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // if not at a base, we check the transport's comm equipment (no relays...)
                MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(this.MissionJob);
                if (job != null && job.Vehicles != null)
                {
                    foreach (var item in job.Vehicles)
                    {
                        foreach (var vehicle in item.Value)
                        {
                            Entity vehicleEntity = Entity.FindByID(vehicle);
                            if (communicates.IsInCommunicationRange(vehicleEntity, out method))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        */

        public EntityType GetMainTransportation()
        {
            if (MissionTemplate.TransportationType != null)
            {
                return MissionTemplate.TransportationType.GetMainTransportation();                
            }

            return null;
        }

        public bool GetNextStopAndETA(out TravelLocation? nextStop, out DateAndTime.TimeDateYear? eta) // out string eta) 
        {
            nextStop = null;
            eta = null;

            if (CurrentMissionStop.TravelAction != null)
            {
                nextStop = CurrentMissionStop.TravelAction.ToMissionStop.MissionStopTemplate.TravelLocation;
                eta = CurrentMissionStop.TravelAction.GetETA();

                return true;
            }

            return false;

        }

        public Entity GetVehicleToLoad()
        {
            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);

            if (job.Vehicles != null && job.Vehicles.Count > 0)
            {
                return Entity.FindByID(job.Vehicles.First().Value[0]);
            }

            return null;

        }

        /// <summary>
        /// TODO: handle multiple vehicles...
        /// </summary>
        /// <returns></returns>
        public Entity GetVehicleOrAgentToLoad()
        {
            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);

            if (job.Vehicles.Count > 0)
            {
                return Entity.FindByID(job.Vehicles.First().Value[0]);
            }
            else
            {
                return job.TakenBy.Get(0);
            }
        }

        public void Update(GameTime gameTime)
        {
            CurrentMissionStop.Update(gameTime); 

            Transportation.Update(gameTime); // TODO: consume food & fuel here...

          
            if (CurrentMissionStop.Actions.Count == 0 && CurrentMissionStop.TravelAction == null)
            {
                Destroy(); // the mission is over.
            }

        }



        #region ILookup

        private MissionID id = MissionID.Invalid;
        static MissionID IDCounter = MissionID.First;

        public MissionID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public MissionID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MissionID.Max)
            {
                throw new Exception("Astounding, MissionID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != MissionID.Invalid)
                LookUp<Mission, MissionID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MissionID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Mission, MissionID>.Remove(this);
        }

        void ILookUp<Mission, MissionID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MissionID.First;
        }

        void ILookUp<Mission, MissionID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Mission, MissionID>.Create();
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

            this.MissionJob = sn.DoEnum(MissionJob);

            //this.StartMissionStop = (MissionStop)sn.DoISnapshot(StartMissionStop);

            snapshotStartMissionStop = (MissionStopID)sn.SnapshotID<MissionStop, MissionStopID>(StartMissionStop);
            snapshotCurrentMissionStop = (MissionStopID)sn.SnapshotID<MissionStop, MissionStopID>(CurrentMissionStop);

            snapshotMissionTemplate = (MissionTemplateID)sn.SnapshotID<MissionTemplate, MissionTemplateID>(MissionTemplate);

            this.Contracts = sn.DoList(Contracts);

            this.Transportation = (Transportation)sn.DoISnapshot(Transportation);


            /*if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotAllegiance = Manager.Parent.ID;
                
            }*/

           
           // sn.Ignore(Manager);

            sn.Ignore(this.tempCoords);
            sn.Ignore(this.tempCanCommunicate);

            return this;

        }

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            StartMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotStartMissionStop);
            CurrentMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotCurrentMissionStop);

            MissionTemplate = LookUp<MissionTemplate, MissionTemplateID>.FindByID(snapshotMissionTemplate);

            //StartMissionStop.mission = this;

            // Warning - MissionLocation.LoadPostProcess is called after Mission.LoadPostProcess...
            // TravelActions are deleted when done - this breaks the call chain to the later stops!
            StartMissionStop.SetParentPostLoad(this);

            // this call will compensate:
            CurrentMissionStop.SetParentPostLoad(this);

            if (Transportation != null)
            {
                Transportation.LoadPostProcess(sn);
                Transportation.SetParentPostLoad(this);
            }

           // StartMissionStop.LoadPostProcess(sn);

            foreach (var item in Contracts)
            {
                item.LoadPostProcess(sn);
            }
        }

        #endregion



        
    }
}

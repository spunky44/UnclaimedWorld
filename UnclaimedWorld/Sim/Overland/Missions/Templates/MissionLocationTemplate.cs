using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;


namespace UWGame.SimSide.Overland.Missions.Templates
{
    public enum MissionStopTemplateID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// contains a reference to a location (TravelLocation) and a list of actions to undertake there
    /// </summary>
    public class MissionStopTemplate : ISnapshot, ILookUp<MissionStopTemplate, MissionStopTemplateID>
    {
        /// <summary>
        /// Start is 0
        /// </summary>
        public int Number = 0;


        /// <summary>
        /// if true, the player is not allowed to change the actions...
        /// </summary>
        public bool IsLocked = false;

        public TravelLocation TravelLocation;

        /// <summary>
        /// actions to undertake at this stop
        /// </summary>
        public SerializableQueue<MissionActionTemplate> Actions = new SerializableQueue<MissionActionTemplate>();

        [XmlIgnore]
        private Queue<MissionActionTemplateID> snapshotActions;

        /// <summary>
        /// this object contains the next destination. Can be null
        /// </summary>
        public TravelActionTemplate TravelAction;

        [XmlIgnore]
        private MissionActionTemplateID? snapshotTravelAction;


        public MissionStopTemplate(bool createID)
        {
            if (createID)
            {
                AddToLookup();
            }          
           
        }

        public MissionStopTemplate()
        {

        }

        public bool IsStart()
        {
            return Number == 0;
        }

        public void AssignIDs()
        {
            AddToLookup();

            if (Actions != null)
            {
                foreach (var item in Actions)
                {
                    item.SetParentID(this); // push the parent at Execute()

                    item.AssignIDs();
                }
            }

            if (TravelAction != null)
            {
                TravelAction.AssignIDs();

               // TravelAction.ToMissionStop.AssignIDs();
            }
            
        }


        public void Destroy()
        {
            RemoveIDEntry();

            foreach (var item in Actions)
            {
                item.Destroy();
            }

            if (TravelAction != null)
            {
                TravelAction.Destroy();
            }

            Actions.Clear();
        }

        /// <summary>
        /// recalcs the numbering of the following stops
        /// </summary>
        public void RecalculateNumbers()
        {
            SetNumber(Number);
        }

        public Queue<MissionActionTemplate> GetActionsAtLocation(MissionStopTemplate location)
        {
            if (this == location)
            {
                return Actions;
            }

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.GetActionsAtLocation(location);
            }

            return null;

            //return locationActions[location];

        }

        public double GetTotalDistance(double distance)
        {
            if (TravelAction != null)
            {
                distance += TravelAction.Distance;

                return TravelAction.ToMissionStop.GetTotalDistance(distance);
            }
            else
            {
                return distance;
            }
        }

        public bool GetOwner(SharedKnowledge sharedKnowledge, Predicate<EntityGroup> matchesPredicate, //Allegiance allegiance, 
            out EntityGroup otherOwner) 
        {
          /*  if (this.TravelLocation.AllegianceID.HasValue && ((AllegianceID)TravelLocation.AllegianceID) != allegiance.ID)            
            {*/
                Site site;
                Allegiance thisAllegiance;
                IKnownEntityData terminal;
                Expedition thisExpedition;
                if (TravelLocation.ResolveLocation(sharedKnowledge, out site, out thisAllegiance, out thisExpedition, out terminal))
                {
                   // if (thisAllegiance != allegiance && thisExpedition != null)
                    if (thisExpedition != null && matchesPredicate(thisExpedition.OwnedEntities))
                    {
                        otherOwner = thisExpedition.OwnedEntities;
                        return true;
                    }                   
                }
                else 
                {
                    otherOwner = null;
                    return false;
                }
           // }
           

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.GetOwner(sharedKnowledge, matchesPredicate, out otherOwner);
            }
            else
            {
                otherOwner = null;
                return true;
            }
        }

        public void RemoveAction(MissionActionTemplate action)
        {
           /* Actions = new SerializableQueue<MissionActionTemplate>(
                Actions.Where(p => p != action));
            */
            List<MissionActionTemplate> actions = Actions.ToList();
            actions.Remove(action);
            Actions = new SerializableQueue<MissionActionTemplate>();

            foreach (var item in actions)
            {
                Actions.Enqueue(item);
            }
        }


        public void AddActionAtLocation(MissionActionTemplate action, MissionStopTemplate location)
        {
            if (this == location)
            {
                Actions.Enqueue(action);
                return;
            }

            if (TravelAction != null)
            {
                TravelAction.ToMissionStop.AddActionAtLocation(action, location);
            }

        }

       

        public bool ContainsLocation(MissionStopTemplate m)
        {
            if (m == this)
                return true;

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.ContainsLocation(m);
            }

            return false;
        }

        public void AddMissionLocation(MissionStopTemplate destination, int number)
        {
            if (TravelAction == null)
            {
                destination.Number = number;
                TravelAction = new TravelActionTemplate(this, destination);
            }
            else
            {
                TravelAction.ToMissionStop.AddMissionLocation(destination, number + 1);
            }

        }

        /// <summary>
        /// call this after re-routing...
        /// </summary>
        /// <param name="number"></param>
        public void SetNumber(int number)
        {
            Number = number;   

            if (TravelAction != null)
            {           
                TravelAction.ToMissionStop.SetNumber(number + 1);
            }
        }


        public float ComputeTotalCargoBulk()
        {
            float total = 0;
            foreach (var item in Actions)
            {
                total += item.ComputeTotalCargoBulk();
            }

            if (TravelAction != null)
            {
                total += TravelAction.ComputeTotalCargoBulk();
            }

            return total;

        }

        public decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
        {
            decimal total = 0;
            boughtItemsCost = 0;
            soldItemsCost = 0;

            decimal actionBoughtItemsCost, actionSoldItemsCost;
            foreach (var item in Actions)
            {               
                total += item.ComputeTotalCost(parent, out actionBoughtItemsCost, out actionSoldItemsCost); 
                
                boughtItemsCost += actionBoughtItemsCost;
                soldItemsCost += actionSoldItemsCost;
            }

            if (TravelAction != null)
            {
                total += TravelAction.ComputeTotalCost(parent, out actionBoughtItemsCost, out actionSoldItemsCost);

                boughtItemsCost += actionBoughtItemsCost;
                soldItemsCost += actionSoldItemsCost;
            }

            return total;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasMeaning">returns true if any meaningful action is present (Buy, Load etc)</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool ValidateMissionActions(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
        {
            if (Actions != null && Actions.Count > 0)
            {
                foreach (var item in Actions)
                {
                    item.Validate(parent, ref hasMeaning, ref errors);
                }
            }

            if (TravelAction != null)
            {
                TravelAction.Validate(parent, ref hasMeaning, ref errors);

                return TravelAction.ToMissionStop.ValidateMissionActions(parent, ref hasMeaning, ref errors);
            }

            return hasMeaning && (errors == null || errors.Count == 0); //  true;
        }

        public bool SelectRoute(MissionTemplate parent, EntityType transport)
        {
            if (TravelAction != null)
            {
                // clear old route:
                TravelAction.SetRoute(null, false);

                VehicleContainerType vehicle = transport.ContainerType as VehicleContainerType;
                if (vehicle.Aircraft != null)
                {
                    TravelAction.SetRoute(null, true);
                }
                else
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
                    
                    Allegiance fromAllegiance;
                    Expedition fromExpedition;
                    IKnownEntityData fromTerminal;
                    Site thisSite;
                   
                    if (!this.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out thisSite, out fromAllegiance, out fromExpedition, out fromTerminal))
                    {
                        return false;
                    }

                    Allegiance toAllegiance;
                    Expedition toExpedition;
                    IKnownEntityData toTerminal;
                    Site toSite;

                    if (!TravelAction.ToMissionStop.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out toSite, out toAllegiance, out toExpedition, out toTerminal))
                    {
                        return false;
                    }

                    var routes = The.Sim.World.GetRoutesAndDistances(thisSite, toSite);

                    if (routes != null)
                    {
                        foreach (var route in routes)
                        {
                            double distance = route.Item2;
                            if (route.Item1 == null)
                            {
                                if (vehicle.CanUseRoute(null, true, distance))
                                {
                                    TravelAction.SetRoute(null, true);
                                    break;
                                }
                            }
                            else if (vehicle.CanUseRoute(route.Item1.RouteType, false, distance))
                            {
                                TravelAction.SetRoute(route.Item1, false);
                                break;
                            }
                        }
                    }
                }

                return TravelAction.ToMissionStop.SelectRoute(parent, transport);
            }

            return true;
        }

        public Expedition GetAllegianceExpeditionOnRoute(Allegiance missionAllegiance)
        {
            Site site;
            Expedition expedition;
            IKnownEntityData terminal;
            Allegiance allegiance;
            if (TravelLocation.ResolveLocation(missionAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                if (expedition.Allegiance == missionAllegiance)
                {
                    return expedition;
                }
            }

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.GetAllegianceExpeditionOnRoute(missionAllegiance);
            }

            return null;
        }

        public Expedition GetExpeditionOnRoute(SharedKnowledge sharedKnowledge, Predicate<Expedition> matches)
        {
            Site site;
            Expedition expedition;
            IKnownEntityData terminal;
            Allegiance allegiance;
            if (TravelLocation.ResolveLocation(sharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                if (matches(expedition))
                {
                    return expedition;
                }
            }

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.GetExpeditionOnRoute(sharedKnowledge, matches);
            }

            return null;
        }

        public PassengerListTemplate GetPassengerListTemplate()
        {
            if (Actions != null)
            {
                EmbarkActionTemplate embarkAction;
                foreach (var item in Actions)
                {
                    embarkAction = item as EmbarkActionTemplate;
                    if (embarkAction != null)
                    {
                        return embarkAction.PassengerListTemplate;
                    }
                }
            }

            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.GetPassengerListTemplate();
            }

            return null;
        }



        public bool ValidateMissionStops(MissionTemplate parent)
        {
             Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            fromSite = null;
            fromAllegiance = null;
            fromExpedition = null;
            fromTerminal = null;

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
            
            if (thisAllegiance == null
                || !TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                return false;
            }

            
            if (TravelAction != null)
            {
                return TravelAction.ToMissionStop.ValidateMissionStops(parent);
            }

            return true;
        }

        #region ILookup

        private MissionStopTemplateID id = MissionStopTemplateID.Invalid;
        static MissionStopTemplateID IDCounter = MissionStopTemplateID.First;

        /// <summary>
        /// make sure we don't attempt to xmlserialize this. It should not exist before the Command has executed. That way, we can cancel out of the dialog without affecting the Sim
        /// 
        /// The ID will get created manually in the CreateMissionTemplate command when executing.
        /// </summary>
        [XmlIgnore]
        public MissionStopTemplateID ID
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

        public MissionStopTemplateID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MissionStopTemplateID.Max)
            {
                throw new Exception("Astounding, MissionStopTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
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
            if (ID != MissionStopTemplateID.Invalid)
                LookUp<MissionStopTemplate, MissionStopTemplateID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MissionStopTemplateID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<MissionStopTemplate, MissionStopTemplateID>.Remove(this);
        }

        void ILookUp<MissionStopTemplate, MissionStopTemplateID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MissionStopTemplateID.First;
        }

        void ILookUp<MissionStopTemplate, MissionStopTemplateID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<MissionStopTemplate, MissionStopTemplateID>.Create();
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);
            
           /* this.Actions = sn.DoSerializableQueue(Actions);   
            this.TravelAction = (TravelActionTemplate)sn.DoISnapshot(TravelAction);
            */

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotActions = new Queue<MissionActionTemplateID>(Actions.Select(a => a.ID));                
            }

            snapshotActions = sn.DoQueue(snapshotActions);
            snapshotTravelAction = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(TravelAction);
               

            this.IsLocked = sn.DoBool(IsLocked);
            this.TravelLocation = sn.DoTravelLocation(TravelLocation);
          
            this.Number = sn.DoInt32(Number);

            sn.Ignore(TravelAction);
            sn.Ignore(Actions);

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

        [XmlIgnore]
        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (snapshotActions != null)
            {
                Actions = new SerializableQueue<MissionActionTemplate>();
                while(snapshotActions.Count > 0)
                {
                    MissionActionTemplateID id = snapshotActions.Dequeue();
                    Actions.Enqueue(LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(id));
                }
            }

            TravelAction = (TravelActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotTravelAction);

            /*
            foreach (var item in Actions)
            {
                item.LoadPostProcess(sn);
            }*/

        }


        #endregion



        
    }
}

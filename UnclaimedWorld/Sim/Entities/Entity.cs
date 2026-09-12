#region Using Statements
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AI.Goals;
using System.Collections;
using Xclna.Xna.Animation;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using System.Text;
using UWGame.SimSide.Vehicles;
using WindowSystem;
using GameStateManagement;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Collisions;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Map;
using UWGame.ClientSide;

using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Log;

using UWGame.SimSide.Entities;
using System.Diagnostics;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.AI.Needs;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.Client.Interface;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Combat;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Containers.Components;
#endregion

namespace UWGame.SimSide.Entities
{
    public delegate void GetChildren(IHasExposedProperties presentedObject, PropertyCondition filter, ref List<IHasExposedProperties> result);


    public struct Message
    {
        public enum CancelJobKeepVehicle { KeepVehicle, LeaveVehicle }
        public enum MessageTypes
        {
            PathFound, PathNotFound, DistanceFound, BestCropFound, DistanceFoundNoAccess, CancelJobOrItemInUse, CancelJobForAIReset, OtherAgentRequestsDropItem, PreyIsNear, EntityDied, DrivenVehicleIsNear, GunshotIsNear, Die,
            WakeUpCombatAlert, StopGoalSleep, AlertToPresence, DinnerIsReady, PickMeUp, GoToRendezvousPoint, HopOnBoard, OKImOn, GetOff, Disembark, StartGroupMovement, EndGroupMovement, WaitForMeGroup, SlowdownGroup, Hit, HitAndCollapse, Collapse, WaitAndMakeRoom,
            StartConversation, ListenToConversation, Interest,
            SpeakLine
        };
        public MessageTypes MessageType;
        public object OtherInfo;
        public Entity Sender;

        public Message(Entity sender, MessageTypes type, object otherInfo)
        {
            MessageType = type;
            OtherInfo = otherInfo;
            Sender = sender;
        }
        public Message(MessageTypes type)
        {
            MessageType = type;
            OtherInfo = null;
            Sender = null;
        }
    }


    public enum FollowerStatus { IsFollowingPathToWaypoint, IsCatchingUp, Normal }

    public struct GoalAndScore
    {
        public double Score;
        public string Goal;
    }


    /// <summary>
    /// Used to ensure that no dangling Entity pointers get referenced
    /// NEVER cache an Entity by pointer/reference, cache its EntityID
    /// instead, and use Entity.FindByID( EntityID ) to retrieve pointer.
    /// Returns null if the Entity has been removed from the simulation.
    /// </summary>
    public enum EntityID : long // now long, for XmlSerializer to work //ulong
    {
        First = 0,
        Invalid = long.MaxValue, // ulong.MaxValue,
        Max = Invalid
    }



    [DebuggerDisplay("{Name}{ID}{EntityType.Name}{MapPosition}")]
    public class Entity : GameObject, IAddon, IComposite, ICanIterateEntities, IKnownEntityData, IDetectable, IHasExposedProperties, ISnapshot, ILookUp<Entity, EntityID>, ICommunicates, ISleepingUpdatable
    {


        #region IKnownEntityData

        public EntityID EntityID
        {
            get
            {
                return id;
            }
        }

        #endregion

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();


        private static Dictionary<string, GetChildrenDelegate> getChildrenProperties = new Dictionary<string, GetChildrenDelegate>();

        private delegate void GetChildrenDelegate(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren);


        Vector3 IDetectable.Location
        {
            get
            {
                return Location.Value;
            }
        }

        public DetectableID DetectableID
        {
            get
            {
                return detectableID;
            }
        }

        /// <summary>
        /// will be a copy of the one in Renderable... merge them???
        /// </summary>
        private BitMask64 simStateFlags;       

        /// <summary>
        /// can be null for now.
        /// 
        /// NOT snapshotted - reselcted after load.
        ///        
        /// </summary>
        public SimStateInfo CurrentSimState;


        #region ILookup

        private EntityID id = EntityID.Invalid;
        static EntityID IDCounter = EntityID.First;


        public EntityID ID
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

        public EntityID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= EntityID.Max)
            {
                throw new Exception("Astounding, EntityID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public EntityID SnapshotID(Snapshotter sn, EntityID id)
        {
            return (EntityID)sn.DoEnum(id);
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
            AddToLookup(false);
        }
        public void AddToLookup(bool hasID)
        {
            if (!hasID)
            {
                ID = GetUniqueID();
            }
            else
            {
                IDCounter++;
            }

            if (ID == (EntityID)9)
            {

            }

            if (ID != EntityID.Invalid)
                LookUp<Entity, EntityID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = EntityID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Entity, EntityID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<Entity, EntityID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = EntityID.First;
        }

        void ILookUp<Entity, EntityID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Entity, EntityID>.Create();
        }


        /// <summary>
        /// This method can now be merged with the ctor and the ctor made public...
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /*  public static Entity Produce(EntityType type, bool structureBeingPlaced = false) //, EntityID? entityID = null)
          {          
              Entity entity = new Entity(type, structureBeingPlaced);        

              return entity;
          }*/

        public static Entity FindByID(EntityID id)
        {
            if (id == EntityID.Invalid)
                return null;

            return LookUp<Entity, EntityID>.FindByID(id);
        }

        public static Entity FindByID(EntityID? id)
        {
            if (id == null
                || id == EntityID.Invalid)
            {
                return null;
            }

            return LookUp<Entity, EntityID>.FindByID(id);
        }


        #endregion

        public DebugLog DebugLog = new DebugLog();

        public OwnerID? SpawnedByOwner;

        public float GetEffect(AffectsNumbers affects, float baseValue, string typeKey = null, string typeTag = null)
        {

            SimEffectsComponent simEffects = SimEffects;
            if (simEffects != null)
            {
                return simEffects.GetEffect(affects, baseValue, typeKey, typeTag);
            }

            return 1f; // return base value??
        }

        public bool GetEffect(AffectsFlags affects, bool baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, bool>> effectComponents = null)
        {

            SimEffectsComponent simEffects = SimEffects;
            if (simEffects != null)
            {
                return simEffects.GetEffect(affects, baseValue, typeKey, typeTag, effectComponents);
            }

            return baseValue;
        }

        public static EntityID LastUsedID
        {
            get
            {
                return IDCounter;
            }
        }


        #region "Client" fields

        Renderable.SnapshotRenderable snapshotRenderable;


        #endregion

        private bool isInitialized = false;

        public ResourceType ResourceType
        {
            get { return null; }
        }

        public Renderable Renderable
        {
            get;
            set;
        }

        /// <summary>
        /// returns null when off site...
        /// </summary>
        public Vector3? AccessPoint
        {
            //TODO : make sure that access point gets refreshed when the environment changes
            get
            {
                // NEW! PartOf and Contained handling. Test this...
                if (PartOf != null)
                {
                    Entity root = GetRootAsEntity();
                    return root.AccessPoint;
                }
                else
                {
                    Entity container;
                    GetContainedBy(out container);
                    if (container != null)
                    {
                        return container.AccessPoint;
                    }
                }

                // TODO: fish trap locations (no geo layout) should have an accesspoint on shore, so comment out this case. 
                //But items then start behaving strangely!
                if ((CurrentSimState == null || CurrentSimState.GeometryLayoutType == null) //EntityType.GeometryLayoutType == null
                    && EntityType.PointLayoutType == null)
                {
                    return Location;
                }
                else
                {
                    if (!accessPoint.HasValue)
                    {
                        if (Location == null)
                        {
                            return null; // othersite
                        }

                        Vector3 locationToScanFrom = location.Value;

                        // containers:
                        if (this.Contains != null)
                        {
                            // the access point(s) assigned by an IExit always take precedence over "natural" access points
                            IExit exit = this.Contains as IExit;
                            if (exit != null)
                            {
                                // is really the rally point... this means access can fail in the goal if only one of rally point or door is accessible...
                             //   accessPoint = exit.ComputeAccessPoint();
                                locationToScanFrom = exit.ComputeAccessPoint(); // can be blocked..
                                                              
                               // return accessPoint.Value;
                            }
                        }

                        // other objects:
                        // trees should not respect reserved tiles, but structures should.
                        bool avoidReservedTiles = false;
                        if (EntityType.StructureType != null)
                        {
                            avoidReservedTiles = true;
                        }

                        accessPoint = MapManager.FindUnblockedLocation(locationToScanFrom, 200f, MapManager.ScanMethod.Fan, EntityType.AccessPointDirection, avoidReservedTiles, radiusIncrements: 40f);
                    }

                    return accessPoint.Value;
                }
            }
        }

        public CasteType CasteType
        {
            get
            {
                if (EntityType.BiologicalType != null)
                {
                    BiologicalEntity bio;
                    if (Find(out bio))
                    {
                        return bio.CasteType;
                    }
                }

                return null;
            }
        }



        public void SetInterestInCollidedEntity(Entity otherEntity)
        {
            if (EntityType.RenderableTypeMode.AnimatedHeadType != null) //Renderable.AnimatedHead != null)
            {
                //look at the other guy:
                float interestLevel = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(
                    GameData.Instance.Constants.InterestLevelForCollidedEntityMean,
                    GameData.Instance.Constants.InterestLevelForCollidedEntityStdDeviation);

                //MathHelper.Lerp(GameData.Instance.Constants.MinimumInterestLevelForCollidedEntity, 
                //            GameData.Instance.Constants.MaximumInterestLevelForCollidedEntity, Globals.Instance.Random.NextDouble()) //  50f;//how interesting is he?

                if (interestLevel > 0f)
                {
                    Intelligence.SetNewCenterOfAttention(otherEntity.EntityID, null, interestLevel);
                }
            }
        }


        private Vector3? accessPoint = null;

        /// <summary>
        /// must be done when the area around the entity changes - will trigger a recompute if needed
        /// </summary>
        public void SetAccessPointDirty()
        {
            accessPoint = null;
        }

        /// <summary>
        /// Defines which of the EntityType.SpecialActionTypes are available. Since they apply to memory facts too, I want to place it in SharedKnowledge to avoid syncing issues
        /// 
        /// only gets modified from script..?
        /// </summary>
      //  public List<ProcessType> AvailableSpecialActions { get; set; }


        /// <summary>
        /// NEW: only physical actions here! No locks
        /// </summary>
        public List<ProcessType> AvailableSharedSpecialActions { get; set; }

        private Regulator showStatusRegulator;

        private bool tooltipEntityDataIsDirty = true;
        private EntityTypeTooltipInstanceData tooltipEntityData;// this should be decoupled from the entity
        public EntityTypeTooltipInstanceData TooltipEntityData
        {
            get
            {
                if (BiologicalEntity != null)
                {
                    if (tooltipEntityDataIsDirty == true)
                    {
                        if (tooltipEntityData == null)
                        {
                            tooltipEntityData = new EntityTypeTooltipInstanceData();
                        }
                        BiologicalEntity.GenerateBiologicalEntityData(tooltipEntityData);
                        tooltipEntityDataIsDirty = false;
                    }
                    return tooltipEntityData;
                }
                else
                {
                    return null;
                }
            }
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


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            //here we snapshot everything that determines the state of this entity
            //any fields that do not need to be snapshotted here should be moved to
            //renderable anyway, or else ignored with sn.Ignore(memberName) 
           

            #region IDs

            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);
            compositeID = sn.DoEnum(compositeID);
            detectableID = sn.DoEnum(detectableID);
            CanIterateEntitiesID = sn.DoEnum(CanIterateEntitiesID);

            #endregion

            EntityType = sn.DoGameData(EntityType);
            accessPoint = sn.DoVector3Nullable(accessPoint);
            avoidDetectionFactor = sn.DoFloat(avoidDetectionFactor);
            tooltipEntityDataIsDirty = sn.DoBool(tooltipEntityDataIsDirty);
            bulk = sn.DoFloat(bulk);
            collidingTimeout = sn.DoInt32(collidingTimeout);
            Components = sn.DoDictionary(Components);

            DebugLog = (DebugLog)sn.DoISnapshot(DebugLog);

            SpawnedByOwner = sn.DoEnumNullable(SpawnedByOwner);

            if (ID == (Entities.EntityID)4528) // containedBy == (EntityID)4648)
            {

            }

            containedBy = sn.DoEntityIDNullable(containedBy);
            CustomFields = sn.DoDictionary(CustomFields);
            DrivingVehicle = sn.DoEntityIDNullable(DrivingVehicle);

            isInitialized = sn.DoBool(isInitialized);

            facingNormal = sn.DoVector3(facingNormal);
            flipHorizontally = sn.DoBool(flipHorizontally);
            footprintIsDirty = sn.DoBool(footprintIsDirty);
            GeometryLayout = sn.DoISnapshot(GeometryLayout) as GeometryLayout;
            PartIsBroken = sn.DoBool(PartIsBroken);
            IsDead = sn.DoBool(IsDead);
            location = sn.DoVector3Nullable(location);
            mapPosition = sn.DoPointNullable(mapPosition);
            name = sn.DoString(name);
            PointLayout = sn.DoISnapshot(PointLayout) as PointLayout;
            rotation = sn.DoFloat(rotation);
            // staticFlagsAreDirty = sn.DoBool(staticFlagsAreDirty);

            if (sn.mode != Snapshotter.Mode.Load
                && The.Client != null
                && Renderable != null)
            {
                snapshotRenderable = Renderable.GetFieldsToSnapshot(); // ugly, but necessary..
            }
            snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);

            //SpriteConditions = (BitMask64)sn.DoISnapshot(SpriteConditions); 
            DirectionalLayout = (DirectionalLayout)sn.DoISnapshot(DirectionalLayout);//TODO Directional Layout should extend ISnapshot
            BoundingRadius3D = sn.DoFloat(BoundingRadius3D);
            BulkChangedEvent = (IDActionEvent<float>)sn.DoISnapshot(BulkChangedEvent);

            tooltipEntityData = (EntityTypeTooltipInstanceData)sn.DoISnapshot(tooltipEntityData);

            AvailableSharedSpecialActions = sn.DoList(AvailableSharedSpecialActions);

            OwnedBy = sn.DoEnumNullable(OwnedBy);
            Contains = (Container)sn.DoISnapshot(Contains);
            containedEntities = sn.DoMultiMap(containedEntities);

            processes = sn.DoList(processes);

            updateInterval = sn.DoDoubleNullable(updateInterval);
            timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);

            coords = sn.DoGeodeticCoordinateNullable(coords); // (GeodeticCoordinate)sn.DoISnapshot(coords);
          //  AvailableSpecialActions = sn.DoList(AvailableSpecialActions);
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);
            this.bioSystemsRegulator = (Regulator)sn.DoISnapshot(bioSystemsRegulator);

            simStateFlags = (BitMask64)sn.DoISnapshot(simStateFlags);

            #region References/IDs
            /*
            if (partOf != null)
            {
                partOfID = partOf.ID;
            }
            else
            {
                partOfID = null;
            }
            partOfID = sn.DoEnumNullable(partOfID);

            
            if (Parts != null)
            {
                snapshotParts = Parts.Select(p => p.id).ToList();
            }
            snapshotParts = sn.DoList(snapshotParts);
            */

            assignedToJob = sn.DoEnumNullable(assignedToJob);
            // snapshotAssignedToJobID = sn.SnapshotID<Job, JobID>(assignedToJob);
            snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(GatheringSite);

            snapshotSiteID = sn.SnapshotID<Site, SiteID>(site);

            inUseBy = sn.DoDictionary(inUseBy);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotTriggers = attachedTriggers.Select(t => t.ID).ToList();

            }
            snapshotTriggers = sn.DoList(snapshotTriggers);



            #endregion

            sn.Ignore(CurrentSimState);
            sn.Ignore(attachedTriggers);
            // sn.Ignore(Site); // crashes when loading!
            sn.Ignore(exposedPropertyValueFunctions); // handled in static ctor
            sn.Ignore(getChildrenProperties);
            sn.Ignore(Parts);
            sn.Ignore(Collidable); // recreate post load
            sn.Ignore(SelectionShape); // recreate post load
            sn.Ignore(DebugGoalPlan);
           
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            /*
            if (partOfID.HasValue)
            {
                partOf = LookUpIComposites.FindByID(partOfID.Value); // uses special class!
            }

            if (snapshotParts != null)
            {
                Parts = snapshotParts.Select(p => Entity.FindByID(p)).ToList();
                snapshotParts = null;
            }*/

           
            // call LoadPostProcess on members here:
            foreach (var item in Components)
            {
                item.Value.LoadPostProcess(sn);
            }

            if (Contains != null)
            {
                Contains.LoadPostProcess(sn);
            }

            if (GeometryLayout != null)
            {
                GeometryLayout.LoadPostProcess(sn);
            }

            if (PointLayout != null)
            {
                PointLayout.LoadPostProcess(sn);
            }

            /* Item itemComponent;
             if (Find(out itemComponent))
             {
                 itemComponent.LoadPostProcess(sn);
             }*/


            attachedTriggers = snapshotTriggers.Select(t => LookUp<Trigger, TriggerID>.FindByID(t)).ToList();

            site = LookUp<Site, SiteID>.FindByID(snapshotSiteID);

            if (snapshotGatheringSite != null && snapshotGatheringSite.HasValue)
            {
                GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
            }

            #region RE-INIT - recreate all the stuff that was not snapshotted:


            showStatusRegulator = CreateShowStatusIconRegulator("Entity", EntityType);


            if (IsOnPlaySite()
                && EntityType.RenderableTypeMode != null)
            {
                ConstructRenderableIfNull(true);

                Renderable.SetToParentLocation(); // set the renderable to the entity's location

                Renderable.UpdateAnimationConditionState(); // make sure the correct animation tracks are running         

                // make sure that matrices, locations etc have proper values:
                Renderable.ComputeMatricesForDrawing();
            }

            // CreateRegulators();

            if (bioSystemsRegulator != null)
            {
                bioSystemsRegulator.LoadPostProcess(sn);
            }
 


            bool footprintWasDirty = footprintIsDirty;

            ReplaceSimState(); // this will create a collidable if needed
           
            CreateCollidable(); // don't recompute the footprint. All terrain maps were snapshotted. Also, Sim will add the Collidable to CollisionManager if it was previously present

            footprintIsDirty = footprintWasDirty;

            #endregion


        }

        void RemoveFromAgentQuadTree()
        {
            if (EntityType.IntelligenceType != null)
            {
                if (The.AgentQuadTree != null)
                {
                    The.AgentQuadTree.RemoveObject(this);// (this); 
                }
            }
        }

        public bool CanDefendItself()
        {
            if (EntityType.IntelligenceType.AttackTypes != null)
            {
                if (EntityType.IntelligenceType.AttackTypes.Count != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public bool CanCommunicate(CommunicationMethod method, double distance)
        {
            // will a skimmer have two comm methods, satellite and shorter range radio..?

            // check innate comm equipment. Will agents have carried/contained equipment too..?
            if (EntityType.CommunicatorType != null
                && EntityType.CommunicatorType.Method == method)
            {
                Communicator communicatorComponent;
                Find(out communicatorComponent);

                if (communicatorComponent.IsCommunicatorWorkingAndInRange(distance))
                    return true;
            }

            return false;
        }


        public DebugGoalPlan DebugGoalPlan
        {
            get;
            set;
        }



        /// <summary>
        /// only has a value for agents!
        /// </summary>
        public AllegianceID? AllegianceID
        {
            get
            {
                if (EntityType.IntelligenceType != null)
                {
                    Intelligence intelligence = Intelligence;
                    if (intelligence.Allegiance != null)
                    {
                        return intelligence.Allegiance.ID;
                    }
                }
                /* else if (EntityType.ThreatType != null)
                 {
                     Threat threat;
                     if (Find(out threat))
                     {
                         return threat.Allegiance.ID;
                     }
                 }*/

                return null;
            }
        }

        public ThreatGroup ThreatGroup
        {
            get
            {
                if (EntityType.IntelligenceType != null)
                {
                    Intelligence intelligence = Intelligence;
                    if (intelligence != null)
                    {
                        return intelligence.Allegiance.ThreatGroup;
                    }
                }
                else if (EntityType.ThreatType != null)
                {
                    Threat threat;
                    if (Find(out threat))
                    {
                        return threat.ThreatGroup;
                    }
                }

                return null;
            }
        }


        private string name;
        public string Name
        {
            get { return name; }
            set
            {
               

                // only on playsite!!

                if (!string.IsNullOrEmpty(name))
                {
                    // remove old name...
                    The.Sim.PlaySite.EntitiesByName.Remove(name);
                }

                if (!string.IsNullOrEmpty(value))
                {
                    if (!The.Sim.PlaySite.EntitiesByName.ContainsKey(value))
                    {
                        The.Sim.PlaySite.EntitiesByName.Add(value, EntityID);
                    }

                }

                name = value;
            }
        }

        // public bool IsEditorPlaced = false;

        public EntityType EntityType { get; private set; }


        public Dictionary<Type, Component> Components;
        //public Dictionary<int, Component> Components; // int is quickest as key...


        //public OwnerID? OwnedBy { get; set; }
        public OwnerID? OwnedBy { get; set; }

        /// <summary>
        /// used to see/unsee processes in FOW
        /// </summary>
        public List<SimProcessID> Processes
        {
            get
            {
                return processes;
            }
            set
            {
                processes = value;
            }
        }

        private List<SimProcessID> processes;


        public void AddProcess(SimProcess process)
        {
            if (!HasProcess(process))
            {
                Common.AddToList(ref processes, process.ID);
            }
        }

        public void RemoveProcess(SimProcessID id)
        {
            if (processes != null)
            {
                processes.Remove(id);
            }
        }

        public bool HasProcess(SimProcess process)
        {
            if (Processes != null)
            {
                return Processes.Contains(process.ID);
            }

            return false;
        }


        SiteID? IKnownEntityData.Site
        {
            get
            {
                if (this.Site != null)
                {
                    return Site.ID;
                }

                return null;
            }
        }

        SiteID? snapshotSiteID;
        private Site site;

        /// <summary>
        /// only null when between sites, has a value when contained or a part
        ///       
        /// entities between sites receive an Update from World
        /// </summary>
        public Site Site
        {
            get
            {
                return site;
                /* Entity root = GetRootAndContainer();

                 return root.site;*/
            }

            set
            {

                if (value != site)
                {
                    if (site != null)
                    {
                        site.RemoveEntity(this);
                    }
                    else
                    {
                        The.Sim.World.RemoveEntityFromBetweenSites(this);
                    }

                    site = value;

                    if (site != null)
                    {
                        site.AddEntity(this);
                    }
                    else
                    {
                        The.Sim.World.AddEntityBetweenSites(this);
                    }
                }

                if (Parts != null)
                {
                    foreach (var part in Parts)
                    {
                        part.Site = value;
                    }
                }

                if (Contains != null)
                {
                    Contains.IterateContained(e => e.Site = value);
                }
            }
        }

        /*  public Site Site
          {
              get
              {
                  Entity root = GetRootAndContainer();

                  return root.site;
              }

              set
              {
                  if (value != site)
                  {
                      if (site != null)
                      {
                          site.RemoveEntity(this);
                      }

                      site = value;

                      if (site != null)
                      {
                          site.AddEntity(this);
                      }

                      if (PartOf == null && ContainedBy == null)
                      {
                          site = value;
                      }

                  }
              }
          }*/

        public Vector3 PlaySiteLocation
        {
            get
            {
                return Location.Value;
            }

        }


        public Point PlaySiteMapPosition
        {
            get
            {
                return MapPosition.Value;
            }

        }

        /// <summary>
        /// NEW: also updates MapPosition which adds to Tile Entity list
        /// for entities being placed, it is necessary to also add them to the tile entity lists in order to render them correctly.
        /// 
        /// make this nullable for when off-playsite. also set to null when added as a part or contained entity.
        /// </summary>      
        public override Vector3? Location
        {
            get
            {
                Entity root = GetRootAndContainer();
                return root.location;
            }

            set
            {
                // TODO
                // why not set on root entity only..?
                if (value != null)
                {
                    // NEW: let's clamp the location. there was a crash near the map edge...
                    Vector3 clampedLocation = new Vector3(The.Map.ClampWorldPosition(value.Value.ToVector2()), value.Value.Z);

                    base.Location = clampedLocation; // value;

                    Vector2 location2D = clampedLocation.ToVector2();

                    UpdateShapeLocation(Collidable, location2D);
                    UpdateShapeLocation(SelectionShape, location2D);

                    Point newMapPosition = MapManager.WorldPosToTile(clampedLocation);

                    MapPosition = newMapPosition; // NEW

                }
                else
                {
                    base.Location = null;

                    MapPosition = null;                  

                }
            }
        }


        /// <summary>
        /// set this to null when a part or contained item, also when off playsite
        /// </summary>
        private Point? mapPosition = null;

        /// <summary>    
        /// make this derived from Location. Only let Location set its value.
        /// 
        /// Also updates fog of war.
        /// 
        /// NOTE: Items in the prcess of being moved between containers will have an illegal position...
        /// 
        ///        
        /// There is one crash where Mapposition is null and location is valid, containedBy is null.
        /// </summary>
        public Point? MapPosition
        {
            get
            {
                Entity root = GetRootAndContainer();
                return root.mapPosition;
            }

            private set // only to be called from Location! 
            {
                if (value != mapPosition)
                {

                    Point? oldMapPosition = mapPosition;
                    mapPosition = value;

                    // NEW
                    if (oldMapPosition != null)
                    {
                        The.Map.GetTile(oldMapPosition.Value).RemoveEntity(this);
                    }

                    if (mapPosition.HasValue)
                    {
                        The.Map.GetTile(mapPosition.Value).AddEntity(this);
                    }


                    Intelligence intelligence;
                    Find(out intelligence);

                  
                    // update others watching us:                      
                    The.Map.UpdateWhoCanSeeEntityMovingBetweenTiles(this, intelligence, oldMapPosition, value);


                    Sensor sensor;
                    if (intelligence != null && IsCompleted() && intelligence.Allegiance != null
                        && Find(out sensor) && sensor.IsActive && mapPosition != null)
                    {

                        sensor.UpdateTilesSeenBySensor(mapPosition.Value);

                        //set nearby remembered entities to deprecated. Since the corresponding entity has not been spotted nearby, this means that the memory fact is stale and the agent should disregard it.

                        DeprecateMemoryFactsInRadius();
                    }                   
                }
            }
        }


        /// <summary>
        /// Global coordinates. This should get updated on outermost entities when moving between sites.
        /// </summary>
        private GeodeticCoordinate? coords;
        public GeodeticCoordinate? Coords
        {
            get
            {
                Entity root = GetRootAndContainer();

                //return root.site;

                if (root.Site != null)
                {
                    return Site.Coords;
                }
                else
                {
                    return root.coords;
                }
            }

            set
            {
                coords = value;
            }
        }

        public Container Contains
        {
            get;
            set;
        }


        #region Statistics dispatcher



        /// <summary>
        /// Logs the injury in Security Statistics. Affects the security rating
        /// </summary>
        /// <param name="attacker"></param>
        public void LogInjuryStatistics(Entity attacker)
        {
            string description = "Injured by " + attacker.ToString();

            LogInjuryDescription(description);
        }

        public void LogInjuryDescription(string description)
        {
            if (Intelligence.Statistics != null)
            {
                Intelligence.Statistics.AddViolentEvent(this, description, ViolentEventType.Injury);
            }

            Intelligence.Allegiance.Statistics.AddViolentEvent(this, description, ViolentEventType.Injury);
        }


        #endregion


        #region IKnownEntityData members


        /*  public float? Rotation
        {
            get
            {
                Locomotor locomotor;
                if (Find(out locomotor))
                {
                    return locomotor.Rotation;
                }
                else return null;
            }

        }*/

        public float? CurrentMaximumSpeed
        {
            get
            {
                Locomotor locomotor;
                if (Find(out locomotor))
                {
                    return locomotor.CurrentMaximumSpeed;
                }
                else return null;
            }
        }

        public Dictionary<FoodNutrientType, float> NutrientBulkAmounts
        {
            get
            {
                Item item;
                if (Find(out item))
                {
                    return item.Food.NutrientBulkAmounts;
                }

                return null;
            }
        }

        public double? Condition
        {
            get
            {
                NonLivingEntity nonLiving;
                if (Find(out nonLiving))
                {
                    return nonLiving.Condition;
                }

                return null;
            }
        }

        public float? ConditionChangeSpeed
        {
            get
            {
                NonLivingEntity nonLiving;
                if (Find(out nonLiving))
                {
                    return nonLiving.ConditionChangeSpeed;
                }

                return null;
            }
        }


        public float? Integrity
        {
            get
            {
                NonLivingEntity nonLiving;
                if (Find(out nonLiving))
                {
                    return nonLiving.Integrity;
                }

                return null;
            }
        }

        public float? Progress
        {
            get
            {
                NonLivingEntity nonLiving;
                if (Find(out nonLiving))
                {
                    return nonLiving.Progress;
                }

                return null;
            }
        }


        public float GetRepairProgress(RepairAction repairAction) //, EntityID? partToFix)
        {
            return NonLivingEntity.GetRepairProgress(repairAction); //, partToFix);

        }

        /* public Dictionary<Entity, Vector3> MeleeAttackers 
        // List<Tuple<Entity, Vector3>> MeleeAttackers
         {
             get
             {
                 Intelligence intelligence = Intelligence;
                 if (intelligence != null)
                 {
                     return intelligence.CombatInfo.MeleeAttackers;
                 }

                 return null;
             }

         }*/

        public StanceType Stance
        {
            get
            {
                if (HasStance())
                {
                    return Locomotor.Stance.CurrentStance;
                }

                return null;
            }
        }

        /*
        public bool AreaIsCleared
        {
            get
            {
                if (GeometryLayout != null)
                {
                    return !GeometryLayout.ShapesContainEntities();

                }
                else return true;
            }
        }

       */


        /// <summary>
        /// a number from 0 to 1 that tells how dangerous the threat is (how many people should attack it, for instance)
        /// </summary>
        public float? StrengthRating
        {
            get
            {
                float strengthRating;

                // scale, so that the same strength rating gives danger = 0.5.
                //   float strengthRatio = 0.5f * ((float)EntityType.IntelligenceType.StrengthRating) / ((float)ourStrengthRating);
                if (EntityType.ThreatType != null)
                {
                    // non-agents, like nests...
                    strengthRating = GameData.Instance.Constants.StrengthRatings[EntityType.ThreatType.StrengthRating]; // (float)EntityType.ThreatType.StrengthRating;                    
                }
                else if (EntityType.IntelligenceType != null)
                {
                    strengthRating = GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating]; //(float)EntityType.IntelligenceType.StrengthRating;
                }
                else return null;

                if (BiologicalEntity != null)
                {
                    switch (BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)
                    {
                        case UWGame.SimSide.Entities.Biological.AIAgeGroup.Baby:
                            strengthRating = 0f;
                            break;
                        case AIAgeGroup.Child:
                            strengthRating *= 0.5f;
                            break;
                        case AIAgeGroup.YoungAdult:
                            strengthRating *= 0.7f;
                            break;
                        case AIAgeGroup.Old:
                            strengthRating *= 0.8f;
                            break;
                    }
                }

                BodyComponent body;
                if (Find(out body))
                {
                    strengthRating *= (float)body.Body.FunctionalScore;
                }

                //  strengthRating = Common.Clamp(strengthRating, 0f, 1f);

                return strengthRating;
            }
        }


        /// <param name="entity"></param>
        /// <returns></returns>
        /*  public static double GetStrengthLevel(Entity entity, StrengthRating ourStrengthRating)
          {
              float strengthRatio = 0.6f * ((float)entity.EntityType.IntelligenceType.StrengthRating + 1) / ((float)ourStrengthRating + 1); 

              // scale, so that the same strength rating gives danger = 0.5.
              double strengthLevel = strengthRatio - 0.1;

              if (entity.BiologicalEntity != null)
              {
                  switch (entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)
                  {
                      case UWGame.SimSide.Entities.Biological.AIAgeGroup.Baby:
                          strengthLevel = 0;
                          break;
                      case AIAgeGroup.Child:
                          strengthLevel *= 0.5;
                          break;
                      case AIAgeGroup.YoungAdult:
                          strengthLevel *= 0.7;
                          break;
                      case AIAgeGroup.Old:
                          strengthLevel *= 0.8;
                          break;
                  }
              }

              Body.Body body;
              if (entity.Find(out body))
              {
                  strengthLevel *= body.FunctionalScore;
              }

              strengthLevel = Common.Clamp(strengthLevel, 0, 1);

              return strengthLevel;
          }*/

        public Body.Body Body
        {
            get
            {
                BodyComponent body;
                if (Find(out body))
                {
                    return body.Body;
                }

                return null;
            }
        }


        public Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts
        {
            get
            {
                if (EntityType.SubstancesType != null)
                {
                    SubstanceComponent substances;
                    if (Find(out substances))
                    {
                        return substances.BulkAmounts;
                    }
                }

                return null;
            }
        }

        public bool? IsMoving
        {
            get
            {
                if (Locomotor != null)
                {
                    return Locomotor.IsMoving();
                }
                else return null;
            }
        }

        /// <summary>
        /// TODO: IExit / IDock !!!
        /// </summary>
        /* public Vector3 AccessPoint1 
         {
             get
             {
                 if (TileLayout != null && TileLayout.Building != null)
                 {
                     return TileLayout.Building.FrontDoorLocation;
                 }
                 else return Location;
             }
         }

         public Vector3? AccessPoint2
         {
             get
             {
                 if (TileLayout != null && TileLayout.Building != null)
                 {                    
                     return TileLayout.Building.BackDoorLocation;
                 }
                 else return null;
             }
         }*/

        /// <summary>
        /// DELETE THIS??? NEVER ASSIGNED!
        /// 
        /// use for 'equipment items': weapons and gadgets not used for a particular job but still carried.
        /// </summary>
        /*  public Entity EquippedBy
          {
              get
              {
                  Item item;
                  if (Find(out item))
                  {
                      return item.EquippedBy;
                  }

                  return null;

              }
              set
              {
                  Item item;
                  if (Find(out item))
                  {
                      item.EquippedBy = value;
                  }
              }
          }*/


        public float CalculateSpeed(float bulk)
        {
            return Locomotor.CalculateSpeed(bulk);
        }

        public void ImpairMovement(float aMovementPartToRemove)
        {
            Locomotor.ImpairMovement(aMovementPartToRemove);
        }

        public bool IsCarrying(EntityID itemToCheckID)
        {
            return Contains.Contains(itemToCheckID);
        }


        /// <summary>
        /// TODO DECOUPLE
        /// //TODO this should be set from EntityType. Right now, it is set from RenderAsModel.Initialize()!
        /// </summary>
        public float BoundingRadius3D { get; set; }

        /*  private Dictionary<EntityType, int> containedEntities;
          public Dictionary<EntityType, int> ContainedEntityTotals
          {
              get
              {
                  if (Contains != null)
                  {
                      if (containedEntities == null)
                      {
                          containedEntities = new Dictionary<EntityType,int>();
                      }
                      containedEntities.Clear();

                  //    Contains.IterateContained(e => Container.ComputeSumsOfItems(e, containedEntities));

                      Contains.IterateContained(e => Container.ComputeSumsOfItems(e, containedEntities));

                      return containedEntities;
                  }

                  return null;
              }
          }*/


        private Dictionary<EntityType, List<EntityID>> containedEntities;
        public Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType
        {
            get
            {
                if (Contains != null)
                {
                    if (containedEntities == null)
                    {
                        containedEntities = new Dictionary<EntityType, List<EntityID>>();
                    }
                    containedEntities.Clear();

                    Contains.IterateContained(e => Common.AddToMultiList(containedEntities, e.EntityType, e.ID));

                    return containedEntities;
                }

                return null;
            }
        }

        public List<EntityID> ContainedEntities
        {
            get
            {
                if (Contains != null)
                {
                    return Contains.GetContainedItemsList(null).Select(e => e.EntityID).ToList();
                }

                return null;
            }
        }

        public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades 
        { 
            get
            {
                if (EntityType.ContainerType != null && EntityType.ContainerType.CanBeUpgraded)
                {
                    IUpgrades upgrades = Contains as IUpgrades;
                    if (upgrades != null)
                    {
                        return upgrades.ContainedUpgrades;
                    }
                }

                return null;
            }
        }


        public Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType
        {
            get
            {
                if (Contains != null)
                {
                    TerminalContainer terminal = Contains as TerminalContainer;
                    if (terminal != null)
                    {
                        return terminal.GetOfferedItems();
                    }
                }

                return null;
            }
        }


        public Dictionary<EntityType, EntityID> IntrinsicWeapons
        {
            get
            {
                if (EntityType.IntelligenceType != null)
                {
                    return Intelligence.IntrinsicWeapons;
                }

                return null;
            }
        }


        public PassengerOrCargoSlot GetFreeDriversSlot()
        {
            Vehicle vehicle;
            if (Find(out vehicle))
            {
                return vehicle.GetFreeDriversSlot();
            }

            return null;
        }

        public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
        {
            Vehicle vehicle;
            if (Find(out vehicle))
            {
                return vehicle.GetCargoSlotsForLoading(bulkToLoad);
            }

            return null;
        }

        public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad)
        {
            Vehicle vehicle;
            if (Find(out vehicle))
            {
                return vehicle.GetCargoSlotsForUnloading(bulkToLoad);
            }

            return null;
        }

        public bool IsUnassigned(SharedKnowledge sharedKnowledge)
        {
            return MemoryFact.IsUnassigned(this, sharedKnowledge);
        }


        public bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge)
        {
            return MemoryFact.IsUnassignedToAnythingButThisJob(this, job, sharedKnowledge);

        }

        /// <summary>
        /// seems like a mixed bag...
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static bool CanBeHauled(IKnownEntityData entityData) // Entity haulingEntity, Intelligence entityIntelligence, HaulingJob job)
        {
           
            if (entityData.PartOfID == null // don't haul components!
               && GoalEvaluator.IsOnPlaySite(entityData)
               && entityData.IsCompleted() // allow hauling of incomplete items??? doesn't really make sense for stuff like meat...            
               && !Items.Item.IsImmovable(entityData.Bulk) // don't haul big objects. rework this once we get vehicles...             
               && entityData.Replenishes == null
               && entityData.UpgradeFor == null)
            {
                return true;
            }

            return false;
        }

        public bool CanBeHauled()
        {
            return CanBeHauled(this);
        }

        /// <summary>
        /// see IsReplenishItemOk
        /// </summary>
        /// <param name="haulingAgent"></param>
        /// <param name="entityIntelligence"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool IsItemValidForHauling(Entity haulingAgent, Intelligence entityIntelligence, HaulingJob job)
        {
            return IsItemValidForHauling(haulingAgent, entityIntelligence, job, this);
        }

        public static bool IsItemValidForHauling(Entity haulingAgent, Intelligence entityIntelligence, HaulingJob job, IKnownEntityData itemData)
        {
            Entity itemEntity = itemData as Entity;
            bool canTakeFromAgent = true;
            if (itemEntity != null)
            {
                Item itemComponent = itemEntity.Item;
                canTakeFromAgent = (itemComponent.OKToTakeThisItemFromCarrier != false || haulingAgent.AgentStorage.Contains(itemEntity)); // only consider items that we are carrying or which are OK to take from others carrying them.

            }

            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(itemData);

            if (itemData.NotOnboardDrivenVehicle
                && canTakeFromAgent
                && itemData.CanBeHauled()
                && itemData.OwnedBy != null
                && Entity.StorageHasRoomForItem(entityIntelligence.Allegiance.SharedKnowledge, job, itemData)
                && (assignedToJob == null || assignedToJob is HaulingJob)
                && (haulingAgent.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(itemData.Bulk))
                && !job.IsStoredInTarget(itemData))
            {

                return true;
            }

            return false;
        }



        public static bool StorageHasRoomForItem(SharedKnowledge sharedKnowledge, HaulingJob job, IKnownEntityData anEntityToBeHauled)
        {

            EntityID? toStorageEntityID = job.GetToStorageEntity;
            if (toStorageEntityID != null)
            {
                IKnownEntityData toStorageEntity;
                if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(toStorageEntityID.Value, out toStorageEntity)))
                {
                    Storage toStorage = toStorageEntity.FindStorage(job.GetToStorageID.Value);

                    if (toStorage != null)
                    {
                        if (!toStorage.HasCapacityForItem(anEntityToBeHauled.Bulk))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;

        }

        public bool HasItemStorage
        {
            get
            {
                return StorageContainer != null;
            }
        }

        public bool HasStance()
        {
            return EntityType.HasStance();
            //  return EntityType.LocomotorType != null && EntityType.LocomotorType.StancesType != null;
        }

        public bool ContainsEntity(EntityID entity)
        {
            return Contains != null && Contains.Contains(entity);
        }

        public IStorage StorageContainer
        {
            get
            {
                Container container = Contains;
                if (container != null)
                {
                    IStorage storage = container as IStorage;
                    if (storage != null)
                    {
                        return storage;
                    }
                }

                return null;

            }

        }

        public Storage FindStorage(StorageID storageID)
        {
            if (Contains != null)
            {
                IStorage iStorage = Contains as IStorage;
                if (iStorage != null)
                {
                    return iStorage.FindStorage(storageID);
                }
            }

            return null;
        }

        public bool IsTradeOfferStorage(StorageID storageID)
        {
            if (FindCompartment(storageID) == StorageCompartment.OfferedForTrade)
            {
                return true;
            }
            else return false;
        }


        public StorageCompartment? FindCompartment(StorageID storageID)
        {
            if (Contains != null)
            {
                IStorage iStorage = Contains as IStorage;
                if (iStorage != null)
                {
                    return iStorage.GetCompartment(storageID);
                }
            }

            return null;
        }

        public Dictionary<StorageCondition, Storage> StorageSpaces
        {
            get
            {
                IStorage storage = StorageContainer;
                if (storage != null)
                {
                    return storage.GetStorageSpaces();
                }

                return null;
            }
        }

        public Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces
        {
            get
            {
                if (Contains != null)
                {
                    TerminalContainer terminal = Contains as TerminalContainer;
                    if (terminal != null)
                    {
                        return terminal.GetTradeOffersStorageSpaces();
                    }

                }

                return null;
            }
        }



        public float? TotalItemStorageCapacity
        {
            get
            {
                IStorage storage = StorageContainer;
                if (storage != null)
                {
                    return storage.TotalItemStorageCapacity;
                }


                return null;
            }
        }

        public float? TotalStored
        {
            get
            {
                IStorage storage = StorageContainer;
                if (storage != null)
                {
                    return storage.TotalStored;
                }

                return null;
            }
        }

        public bool? IsPrepared
        {
            get
            {
                UWGame.SimSide.Items.Tool tool;
                if (Find(out tool))
                {
                    return tool.IsPrepared;
                }

                return null;
            }
        }

        #endregion




        public BiologicalEntity BiologicalEntity
        {
            get
            {
                BiologicalEntity b;
                if (Find(out b))
                {
                    return b;
                }
                else return null;
            }
        }

        public Person PersonEntity
        {
            get
            {
                Person v;
                if (Find(out v))
                {
                    return v;
                }
                else return null;
            }
        }

        public Item Item
        {
            get
            {
                Item i;
                if (Find(out i))
                {
                    return i;
                }
                else return null;
            }
        }

        public Locomotor Locomotor
        {
            get
            {
                Locomotor l;
                if (Find(out l))
                {
                    return l;
                }
                else
                {
                    //throw new Exception("you may have expected a real locomotor here");
                    return null;
                }
            }

        }

        public SimEffectsComponent SimEffects
        {
            get
            {
                SimEffectsComponent l;
                if (Find(out l))
                {
                    return l;
                }
                else
                {
                    return null;
                }
            }

        }


        public Intelligence Intelligence
        {
            get
            {
                Intelligence i;
                if (Find(out i))
                {
                    return i;
                }
                else return null;
            }
        }


        public Vehicle Vehicle
        {
            get
            {
                Vehicle v;
                if (Find(out v))
                {
                    return v;
                }
                else return null;
            }
        }

        // public Structure Structure;
        public Structure Structure
        {
            get
            {
                Structure v;
                if (Find(out v))
                {
                    return v;
                }
                else return null;
            }
        }

        public GatheringSite GatheringSite { get; set; }
        private GatheringSiteID? snapshotGatheringSite;

        /// <summary>
        /// this object has 3 purposes:
        /// 1. produce a footprint on the terrain
        /// 2. detect collisions, by registering with CollisionManager
        /// 3. intercept mouse clicks
        /// 
        /// for terrain entities, we want to do 1, but not 2 (to save CPU)
        /// for movable entities, we only do 2.
        /// structures, 1 + 2 + 3 (2 may be unnecessary?)
        /// </summary>
        public Collidable<Entity> Collidable
        {
            get;
            set;
        }

        public Collidable<Entity> SelectionShape;


        public DirectionalLayout DirectionalLayout;

        /// <summary>
        /// TODO: deprecate this
        /// </summary>
        public PointLayout PointLayout;
        public GeometryLayout GeometryLayout;



        public TerrainPath TerrainPath
        {
            get
            {
                TerrainPath v;
                if (Find(out v))
                {
                    return v;
                }
                else return null;
            }
        }





        private List<Trigger> attachedTriggers = new List<Trigger>();
        List<TriggerID> snapshotTriggers;




        /// <summary>
        /// remember composites!
        /// </summary>
        /*  public Point MapPosition
          {
              get
              {
                  IComposite root = GetRoot();
                  Item item = root as Item;
                  if (item != null)
                  {
                      return item.mapPosition;
                  }
                  else return ((Entities.Entity)root).MapPosition;
              }
              set { mapPosition = value; }
          }
          */

        private Entity GetRootAsEntity()
        {
            IComposite root = GetRoot();
            Entity entity = root as Entity;
            return entity;
        }

        /// <summary>
        /// finds the outermost containing entity or parent of this part.
        /// </summary>
        /// <returns></returns>
        public Entity GetRootAndContainer()
        {
            Entity root = GetRootAsEntity();

            Entity container;
            GetContainedBy(out container);
            if (container != null)
            {
                return container.GetRootAndContainer();
            }
            else return root;
        }




        /// <summary>
        /// only called when map position changes. perhaps call it periodically?
        /// </summary>
        private void DeprecateMemoryFactsInRadius()
        {
            Point? mapPos = MapPosition;
            if (!mapPos.HasValue)
                return;

            // scan other tiles for memory facts within radius

            int radius = GameData.Instance.AIConstants.DeprecateMemoryFactsWithinTileRadius; // 2;

            int minX = The.Map.ClampTileMapXPosition(mapPos.Value.X - radius);
            int maxX = The.Map.ClampTileMapXPosition(mapPos.Value.X + radius);
            int minY = The.Map.ClampTileMapXPosition(mapPos.Value.Y - radius);
            int maxY = The.Map.ClampTileMapXPosition(mapPos.Value.Y + radius);

            TerrainTile newTile;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    newTile = The.Map.TileMap[x][y];

                    newTile.DeprecateMemoryFactsOnTile(this, TerrainTile.DeprecateDistance.Near);
                }

            }
        }

        #region Basic positioning

        private Vector3 facingNormal = new Vector3(1f, 0f, 0f);
        /// <summary>
        /// Must be set in sync with Rotation.
        /// The right vector corresponds to 0 radians rotation, the down vector to Pi/2
        /// </summary>
        public Vector3 FacingNormal// = new Vector3(1f, 0f, 0f); //TODO DECOUPLE
        {
            get { return facingNormal; }
            set { facingNormal = value; }
        }

        public bool HasLocation
        {
            get
            {
                if (containedBy == null && mapPosition == null)
                {
                    return false;
                }
                else return true;
            }
        }







        /// <summary>
        /// upper left tile for structures! Keep this for now.
        /// 
        /// TODO: remove this. use geolayout to get the touched tiles if needed
        /// 
        /// TODO: change this to accomodate: 1. non-structures 2. non-tile-aligned entities.
        /// move to tilelayout?
        /// </summary>
        public Point? TopLeftMapPosition
        {
            get
            {
                return MapPosition;

                //return topLeftMapPosition;
            }
            /*   set
               {
                   topLeftMapPosition = value;     
               }*/
        }

        private void UpdateShapeLocation(Collidable<Entity> shapes, Vector2 value)
        {
            if (shapes != null)
            {
                shapes.Center = value;
            }

        }

        protected float rotation = 0f;
        /// <summary>
        /// Remember to also set the NormalizedMoveDir vector
        /// Rotation starts at 0 radians looking right (1, 0) and goes back towards 2pi clockwise.
        /// (a range between -pi and pi alos seems to work, but let's not complicate things.)
        /// </summary>
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public void SetRotationAndDir(float rotation)
        {
            Rotation = rotation;
            FacingNormal = new Vector3((float)Math.Cos(rotation), (float)Math.Sin(rotation), 0f);
        }


        /// <summary>
        /// also handles Rotator (sentry)
        /// </summary>
        public float FacingAngleWithRotator
        {
            get
            {
                if (Locomotor == null || Locomotor.Rotator == null)
                {
                    return Rotation;
                }
                else
                {
                    return Locomotor.Rotator.AbsoluteRotation;
                }
            }
        }


        #endregion





        public bool Find<T>(out T c) where T : Component//IEntityComponent
        {
            Type type = typeof(T);
            Component co;
            if (Components.TryGetValue(type, out co))
            {
                c = (T)co;
                return true;
            }

            c = null;
            return false;


        }

        /*
          public T Find<T>() where T : IEntityComponent
        {
            Type type = typeof(T);

            if (components.ContainsKey(type)) {
                return (T)components[type];
            }

            // todo: exception
            //throw new Exception("Well, the component wasn't found.");
            return default(T);
        }
         */

        //public AI.Goals.GoalFollowPath.Waypoint? GroupMoveDestinationWaypoint;
        //  public bool? GroupMoveIsCatchingUp;
        /// <summary>
        /// will be true when the follower has requested and is following a path to get to his assigned waypoint, while the group waits.
        /// </summary>
        //  public bool? GroupMoveIsFollowingPathToWaypoint;

        public bool IsDead = false;

       
        public static string GetExceptionInformation(IKnownEntityData data)
        {
            string exceptionString = "\n Whoops - fatal error. Press Ctrl-C to copy the contents of this dialog and paste the text into the forums: \n";

            if (data != null)
            {
                string itemToString = data.ToString();
                bool isInsideContainer = data.ContainedBy != null;
                if (data is MemoryFact)
                {
                    exceptionString += "Data is MemoryFact \n";
                }
                else
                {
                    exceptionString += "Data is Entity \n";
                }

                if (data.MapPosition.HasValue)
                {
                    exceptionString += "Map Position X:" + data.MapPosition.Value.X + " Y:" + data.MapPosition.Value.Y + "\n";
                }
                else
                {
                    exceptionString += "Map Position is null.";
                }

                if (data.Location.HasValue)
                {
                    exceptionString += "Location X:" + data.Location.Value.X + " Y:" + data.Location.Value.Y + "\n";
                }
                else
                {
                    exceptionString += "Location is null.";
                }

                exceptionString += "Is inside container: " + isInsideContainer.ToString() + "\n";
                exceptionString += itemToString + "\n";
                exceptionString += "ID: " + data.EntityID.ToString() + "\n";

                Entity entity = Entity.FindByID(data.EntityID);
                if (entity == null)
                {
                    exceptionString += "Entity has been removed from entity factory. Entity is null \n";
                }
                else
                {
                    exceptionString += "Entity still exists in entity factory. Entity is not null \n";
                }
            }
            else
            {
                exceptionString = "Entity/memoryFact is null \n";
            }

            return exceptionString;
        }

       


        public NonLivingEntity NonLivingEntity
        {
            get
            {
                NonLivingEntity nonLivingEntity;
                Find(out nonLivingEntity);
                return nonLivingEntity;
            }

        }

        public CompositeID? PartOfID
        {
            get
            {
                NonLivingEntity nonLivingEntity = NonLivingEntity;
                if (nonLivingEntity != null)
                {
                    return nonLivingEntity.PartOfID;                  
                }

                /*
                if (partOf != null)
                    return partOf.ID;
                */

                return null;
            }
        }

        public void CreateParts()
        {
            NonLivingEntity nonLivingEntity = NonLivingEntity;
            if (nonLivingEntity != null)
            {
                nonLivingEntity.CreateParts();
            }
        }

     
        public List<EntityID> PartIDs
        {
            get
            {
                List<Entity> parts = Parts;
                if (parts != null)
                {
                    return parts.Select(e => e.ID).ToList();
                }
                
                return null;
            }
        }

    
        public IComposite PartOf
        {
            get
            {
                NonLivingEntity nonLiving = NonLivingEntity;
                if (nonLiving != null)
                {
                    return nonLiving.PartOf;
                }

                return null;

                //return partOf;
            }
            set
            {
                 NonLivingEntity nonLiving = NonLivingEntity;
                 if (nonLiving != null)
                 {
                     nonLiving.PartOf = value;
                 }
                
            }
        }

        /// <summary>
        /// to be called on leaf entities
        /// </summary>
        public void SetLocationPropertiesForLeaf()
        {
            //site = null;
            Location = null; // triggers memory fact generation

        }

        /// <summary>
        /// not the root!
        /// </summary>
        public EntityID? ParentEntityID
        {
            get
            {
                NonLivingEntity nonLivingEntity = NonLivingEntity;
                if (nonLivingEntity != null)
                {
                    return nonLivingEntity.ParentEntityID;
                }

                /*
                if (partOf != null)
                {
                    Entity parentAsEntity = partOf as Entity;
                    if (parentAsEntity != null)
                    {
                        return parentAsEntity.ID;
                    }
                }*/

                return null;
            }
        }

       
        public List<Entity> Parts 
        {
            get
            {
                NonLivingEntity nonLivingEntity = NonLivingEntity;
                if (nonLivingEntity != null)
                {
                    return nonLivingEntity.Parts;
                }

                return null;
            }
            set
            {
                NonLivingEntity nonLivingEntity = NonLivingEntity;
                if (nonLivingEntity != null)
                {
                    nonLivingEntity.Parts = value;
                }
            }
        }

      //  List<EntityID> snapshotParts;

        // TODO: after repair/replacement - reset this flag
        public bool PartIsBroken
        {
            get;
            private set;
        }


        public void SetPart(Entity newPart)
        {
            NonLivingEntity nonLiving = NonLivingEntity;
            if (nonLiving != null)
            {
                nonLiving.SetPart(newPart);
            }
            
            /*
            Parts.Add(newPart);
            newPart.PartOf = this;
            newPart.Site = this.Site; // new

            if (this.EntityType.IntelligenceType != null)
            {
                // recursively scan the parts to get references to intrinsic tools or weapons
                if (this.EntityType.IntelligenceType.IntrinsicToolTypes != null)
                {
                    AddIntrinsicTool(newPart);
                }

                if (this.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
                {
                    AddIntrinsicWeapon(newPart);
                }
            }*/
        }

       

        public void RemovePart(Entity part, bool setPartOfToNull = true)
        {
            NonLivingEntity nonLiving = NonLivingEntity;
            if (nonLiving != null)
            {
                nonLiving.RemovePart(part, setPartOfToNull);
            }

            /*
            if (part.PartOf == this)
            {
                Parts.Remove(part);

                if (setPartOfToNull)
                {
                    part.PartOf = null;
                }

                if (this.EntityType.IntelligenceType != null)
                {
                    // recursively scan the parts to get references to intrinsic tools or weapons
                    if (this.EntityType.IntelligenceType.IntrinsicToolTypes != null)
                    {
                        RemoveIntrinsicTool(part);
                    }

                    if (this.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
                    {
                        RemoveIntrinsicWeapon(part);
                    }
                }
            }*/
        }

        public EntityAndRoot GetAsEntityAndRoot()
        {
            Entity root = GetRootAsEntity();
           
            EntityAndRoot partAndRoot = new EntityAndRoot(this.ID, root.ID);
            return partAndRoot;
        }

        public Entity GetRootEntity()
        {
            if (PartOf == null)
            {
                return this;
            }
            else
            {
                return (Entity)PartOf.GetRoot();
            }
        }

        public EntityID RootEntityID
        {
            get
            {
                return GetRootEntity().ID;
            }
        }

      /// <summary>
      /// gets the root of the parts tree
      /// </summary>
      /// <returns></returns>
        public IComposite GetRoot()
        {
            if (PartOf == null)
            {
                return this;
            }
            else
            {
                return PartOf.GetRoot();
            }
        }

        /// <summary>
        /// true if it is a root with parts
        /// </summary>
        public bool IsCompositeRoot
        {
            get
            {
                return IsRoot && !IsLeaf;
            }
        }

        public bool IsRoot
        {
            get
            {
                return PartOf == null;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return Parts == null;
            }
        }

        /// <summary>
        /// only call this on the root
        /// </summary>
        public void SetConditionDirty()
        {
            NonLivingEntity nonLiving;
            if (Find(out nonLiving))
            {
                nonLiving.SetConditionDirty();
            }
        }

        public void SetBrokenPart()
        {
            bool wasBroken = PartIsBroken;

            PartIsBroken = true;

            // notify other components - we won't use an event because snapshotting is a bit of a hassle
            if (EntityType.SensorType != null)
            {
                Sensor sensor;
                Find(out sensor);
                sensor.NotifyPartIsBroken();
            }

            /* if (PartIsBrokenEvent != null)
             {
                 // notify listeners:
                 PartIsBrokenEvent.Invoke(this, null);
             }*/

            // signal that a part broke down completely
            if (PartOf != null)
            {
                PartOf.SetBrokenPart();
            }
            else
            {

                if (!wasBroken)
                {
                    // log breakdown of entity owned and seen by player:
                    The.Client.LogIsBroken(this);

                }
            }


            UpdateFunctionality();
        }


        public IDActionEvent<float> BulkChangedEvent;

        //public delegate void BulkChangedHandler(float oldValue);
        //public event BulkChangedHandler BulkChanged; // TODO: push the change to components instead to avoid the hassle with Snapshot


        private float bulk;

        /// <summary>
        /// Bulk size of entity
        /// 1: average person size
        /// 'Bulk'? or mass?
        /// </summary>
        public float Bulk
        {
            get { return bulk; }
            set
            {
                // here, we could perhaps use an event instead of calling the components directly...
                if (!Common.IsEqual(bulk, value))
                {
                    float oldBulk = bulk;

                    bulk = value;

                    BulkChangedEvent.Invoke(oldBulk);
                    /*
                    if (BulkChanged != null)
                    {
                        BulkChanged.Invoke(oldBulk);
                    }*/

                    if (EntityType.TreeType != null)
                    {
                        Tree tree;
                        Find(out tree);
                        tree.UpdateBulk();
                    }

                    if (EntityType.BodyType != null)
                    {
                        BodyComponent body;
                        Find(out body);
                        body.Body.UpdateBulk(oldBulk);
                    }

                    if (EntityType.BiologicalType != null)
                    {

                        // removed because it didn't give a correct warning...the size of diamond birds were all above 0.1 but it still gave a warning..MP dec
                        //#if !RELEASE
                        //                    if (value < GameData.Instance.Constants.MinimumAgentBulk)
                        //                    {
                        //                        throw (new Exception("An entity had a too low bulk"));
                        //                    }
                        //#endif
                        BiologicalEntity bioEntity;
                        Find(out bioEntity);
                        if (bioEntity.Needs != null)
                        {
                            if (ID == (EntityID)4600)
                            {
                            }

                            bioEntity.Needs.UpdateNeedsTotalBulk();
                        }

                    }

                    /* if (EntityType.SubstancesType != null)
                     {
                         SubstanceComponent substances;
                         Find(out substances);
                         substances.UpdateBulk(oldBulk);

                     }*/
                }
            }
        }


        public AgentStorage AgentStorage
        {
            get
            {
                if (Contains != null)
                {
                    AgentStorage agentStorage = Contains as AgentStorage;
                    if (agentStorage != null)
                    {
                        return agentStorage;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// probalby put this stuff in SharedKnowledge instead
        /// </summary>
        /// <param name="allegiance"></param>
        /// <param name="memoryFact"></param>
        public void SyncWithMemoryFact(Allegiance allegiance, MemoryFact memoryFact)
        {
            // For now, only the player allegiance will use these fields! If later on more tool using/ownership allegiances are introduced, create dictionaries for them.                        
            if (allegiance.AllegianceType == AllegianceType.Player)
            {
                AssignedToJob = memoryFact.AssignedToJob;
                Residents = memoryFact.Residents;

                List<HouseholdID> households = Households;
                if (households != null)
                {
                    households.Clear();
                    households.AddRange(memoryFact.Households);
                }
               
                OwnedBy = memoryFact.OwnedBy; // the memory fact is allowed to be discarded, but cannot be claimed
            }


            // merge the locks
        /*    Dictionary<EntityGroupID, EntityID> allegianceLocks = memoryFact.InUseBy;
            if (allegianceLocks != null)
            {
                if (inUseBy == null)
                {
                    inUseBy = new Dictionary<EntityGroupID, EntityID>();
                }

                foreach (var item in allegianceLocks)
                {
                    inUseBy[item.Key] = item.Value;
                }

                // need delete too!!!
                // remove EntityGroupIDs that are invalid or belong to this allegiance, but are not in the MF inUseBy dict

            }*/

          //  inUseBy = memoryFact.GetInUseByListForSync();

            DebugLog.Add("Synced with memory fact.");

            // keep the memoryfact log entries:
            DebugLog.Entries.AddRange(memoryFact.DebugLog.Entries);
            
        }

     
        private JobID? assignedToJob;
     

        /// <summary>
        /// both structures and items (both tools) can be assigned to a Process job. Only Items can be assigned to a HaulingJob
        /// 
        /// This should get assigned in Goal.Activate, and de-assigned in Terminate
        ///   1. if cancelling: 
        /// remove InUseBy locks on ALL tools
        /// remove AssignedToJob on HAND tools only
        /// 
        /// 2. if completing the process:
        /// remove InUseBy locks on ALL tools
        /// remove AssignedToJob locks on ALL tools (is handled by Job.Destroy())
        /// 
        /// Only agents (evaluators) should assign the job, and de-assign when cancelling/completing. 
        /// Not job managers. Otherwise the entity can be locked forever
        ///    
        /// 
        /// (Future TODO: to support more allegiances, this would have to become a dictionary in the same way as InUseBy)
        /// </summary>
        public JobID? AssignedToJob
        {
            get
            {
                Entity root = (Entity)GetRoot();
                if (root != null)
                {
                    // Debug.Assert(root.assignedToJob == null || root.assignedToJob.ID != JobID.Invalid, "Destroyed job??");

                    return root.assignedToJob;
                }
                else return null;
            }
            set
            {
                Entity root = (Entity)GetRoot();
                if (root != null)
                {
                  
                    JobID? previousAssigned = root.assignedToJob;

                    if (root.assignedToJob != value)
                    {
                        if (value.HasValue)
                        {
                           
                            DebugLog.Add(string.Format("AssignedToJob set: JobID {0}", value.Value.ToString()));
                        }
                        else
                        {                           
                            DebugLog.Add(string.Format("AssignedToJob cleared. Old JobID: {0}", previousAssigned.Value.ToString()));
                        }
                    }

                    root.assignedToJob = value;



                    // if haulingjob status changed, mark any carrier of the item so the anim states can be updated
                    if (previousAssigned != null)
                    {
                        Job previousAssignedJob = LookUp<Job, JobID>.FindByID(previousAssigned);
                        Job newAssignedJob = LookUp<Job, JobID>.FindByID(value);

                        // will be false if the value is null
                        if (previousAssignedJob is HaulingJob || newAssignedJob is HaulingJob) // value is HaulingJob)
                        {
                            if (root.ContainedBy.HasValue)
                            {
                                Container container;
                                if (root.GetContainedBy(out container))
                                {
                                    AgentStorage agentStorage = container as AgentStorage;
                                    if (agentStorage != null)
                                    {
                                        agentStorage.isHaulingIsDirty = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //private EntityID? inUseBy;
        // //<summary>
        // //NEW: used when claiming items and vehicles not for a specific job - for example for eating
        // //</summary>
        //public EntityID? InUseBy
        //{
        //    get
        //    {
        //        Entity root = (Entity)GetRoot();
        //        if (root != null)
        //        {
        //            return root.inUseBy;
        //        }
        //        else return null;
        //    }
        //    set
        //    {
        //        Entity root = (Entity)GetRoot();
        //        if (root != null)
        //        {
        //            root.inUseBy = value;
        //        }
        //    }
        //}


        /// <summary>
        /// No class should directly interact with inuseby. They should use the 2 methods  SetInUseBy and GetInUseBy
        /// </summary>
        private Dictionary<EntityGroupID, EntityID> inUseBy;



        /// <summary>
        /// the purpose of InUseBy is to enable agents to place locks on items not used in jobs. Such as GoalEat, or weapons/equipment being used in GoalEat and other non-job activities.
        /// 
        /// InuseBy will be set when the agent targets the item for pickup, and cleared when he is done with it (but he may still carry it)
        /// 
        /// 
        /// For simplicity reasons, the lock is also set together with AssignedToJob. 
        /// 
        /// For process tools in unattended processes, the lock is cleared when the agent leaves the process. Then AssignedToJob is the only lock left.
        /// </summary>
      /*  private Dictionary<EntityGroupID, EntityID> InUseBy
        {
            get
            {
                if (inUseBy == null)
                {
                    inUseBy = new Dictionary<EntityGroupID, EntityID>();
                }

                return inUseBy;
            }
        }

        */
        /*
        public Dictionary<EntityGroupID, EntityID> GetInUseByListForSync()
        {
           // return inUseBy;
            // NEW: copy this dict:
            return new Dictionary<EntityGroupID, EntityID>(inUseBy);
        }

       

        public void ClearInUseBy(EntityGroup entityGroup, EntityID userID)
        {
            ClearInUseBy(InUseBy, entityGroup, userID);

            DebugLog.Add(string.Format("ClearInUseBy: EntityGroup {0}, Entity {1}", entityGroup.ID, userID));
        }

        public static void ClearInUseBy(Dictionary<EntityGroupID, EntityID> inUseBy, EntityGroup entityGroup, EntityID userID)
        {
            EntityID currentUser;

            if (inUseBy.TryGetValue(entityGroup.ID, out currentUser)
                && currentUser == userID)
            {
                inUseBy.Remove(entityGroup.ID);


            }
        }

        public void SetInUseBy(EntityGroup entityGroup, EntityID userID)
        {
            if (this.EntityID == (EntityID)4546)
            {

            }

            //UsedInProcess[entityGroup.ID] = new SimProcess(id);

            EntityID idToSet;
            if (InUseBy.TryGetValue(entityGroup.ID, out idToSet))
            {
                InUseBy.Remove(entityGroup.ID);
            }

            InUseBy.Add(entityGroup.ID, userID);


            DebugLog.Add(string.Format("SetInUseBy: EntityGroup {0}, Entity {1}", entityGroup.ID, userID));
        }

        public EntityID? GetInUseBy(EntityGroup entityGroup)
        {
            EntityID userID;

            if (!InUseBy.TryGetValue(entityGroup.ID, out userID))
                return null;

            return userID;
        }

        */

        private bool flipHorizontally = false;
        public bool FlipHorizontally
        {
            set
            {
                flipHorizontally = value;

                if (Renderable != null)
                {
                    Renderable.FlipHorizontally = value;
                   // Renderable.FlipLightingHorizontally(value);
                }
            }

            get
            {
                return flipHorizontally;
            }
        }

        //   public Dictionary<Items.Item, Items.Item> CarriedItems = new Dictionary<Items.Item, Items.Item>();


        #region Containment and helpers

        private EntityID? containedBy;

        /// <summary>
        /// use this for all buildings, storage, vehicles etc. Not for parts, though.
        /// 
        /// for parts, we only update this on the parent/root
        /// 
        /// the helper method for getting the container entity is called GetContainedBy
        /// </summary>
        public EntityID? ContainedBy
        {
            get
            {
                Entity root = GetRootAsEntity();

                return root.containedBy;
            }

            set
            {
                Entity root = GetRootAsEntity(); // get the root if part
                if (this == root)
                {
                    if (value != containedBy)
                    {
                      
                        // NEW: order changed to create MFs at the current position                       
                        if (value != null)
                        {
                            The.Map.UpdateWhoCanSeeEntityBeingContained(this, Intelligence, value); //, oldTilePos); // NEW - critters that cannot see inside the container will forget the item here
                        }

                        containedBy = value;

                        if (value != null)
                        {
                            SetLocationPropertiesForLeaf(); // Sets location to null. UnSee is done in Location -> MapPosition. This creates MemoryFacts when moving between tiles, but not in this case.
                        }
                    }
                }
                else
                {
                    root.ContainedBy = value;
                }
          
            }
        }

      
        /// <summary>
        /// only called when becoming a part, hmm
        /// </summary>
        public void ClearContainedBy()
        {
            Entity container;

            // DON't get the root! // #CONTAINFIX 1.0.1.4
            if (containedBy.HasValue) //  GetContainedBy(out container) && container != null)
            {
                container = Entity.FindByID(containedBy.Value);
                if (container != null)
                {
                    // #KNIFEHACK: Could the error have been that only one reference was set to null?
                    // The agent still had the knife part in its storage...

                    container.Contains.Remove(this);
                }
            }
        }

        // if stored on a Vehicle...?!?!
        public Entity OnBoard
        {
            get
            {
                if (containedBy.HasValue)
                {
                    Entity containedByEntity = Entity.FindByID(containedBy.Value);
                    if (containedByEntity.EntityType.ContainerType is VehicleContainerType)
                    {
                        return containedByEntity;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// perhaps rework this...
        /// </summary>
        public bool NotOnboardDrivenVehicle
        {
            get
            {
                Item itemComponent = Item;
                if (itemComponent != null)
                {
                    return OnBoard == null; // || OnBoard.Vehicle.DrivenBy == null;
                }

                return true;
            }
        }

        /* public EntityID? ContainingBuilding
         {
             get
             {
                 Entity container = GetContainingBuilding();
                 return (container != null ? (EntityID?)container.ID : null);
             }
         }*/


        /// <summary>
        /// TODO: Delete this and use containers
        /// </summary>
        //  public Entity InsideBuilding;



        /// <summary>
        /// TODO: 
        /// make this into a more general get outermost ContainedBy..??
        /// </summary>
        /// <returns></returns>
        /*  public Entity GetContainingBuilding()
          {
            
              Entity container;
              if (!GetContainedBy(out container))
              {
                  return null;
              }

              if (container != null)
              {
                  if (container.TileLayout != null && container.TileLayout.Building != null)
                  {
                      return container;
                  }
              }
            

              return null;
          }*/


        /// <summary>
        /// TODO: DELETE THIS - make vehicles Containers and call GetDrivenVehicle instead.
        /// </summary>
        public EntityID? DrivingVehicle;

        /// <summary>
        /// TODO: DELETE THIS - make vehicles Containers and call GetDrivenVehicle instead.
        /// </summary>
        public EntityID? PassengerInVehicle;


        public EntityID? InsideVehicle //TODO should return if contained and containertype is vehicle
        {
            get
            {
                if (PassengerInVehicle != null)
                {
                    return PassengerInVehicle;
                }
                else if (DrivingVehicle != null)
                {
                    return DrivingVehicle;
                }

                return null;
            }
        }

        /// <summary>
        /// if false is returned, the entity is destroyed...
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public bool GetVehicle(out Entity vehicle)
        {
            if (!GetContainedBy(out vehicle))
            {
                return false; // something bad happened
            }

            if (vehicle != null)
            {
                Container vehicleContains = vehicle.Contains;

                if (vehicleContains != null)
                {
                    if (!vehicleContains.IsDriver(this))
                    {
                        vehicle = null;
                    }
                }
            }

            return true;

        }


        /// <summary>
        /// gives the driven vehicle or null.
        /// if false is returned, the entity is destroyed...
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public bool GetDrivenVehicle(out Entity vehicle)
        {
            if (!GetContainedBy(out vehicle))
            {
                return false; // something bad happened
            }

            if (vehicle != null)
            {
                Container vehicleContains = vehicle.Contains;

                if (vehicleContains != null)
                {
                    if (!vehicleContains.IsDriver(this))
                    {
                        vehicle = null;
                    }
                }
            }

            return true;

        }

        public bool IsInAVehicle()
        {
            Entity vehicle;
            if (!GetContainedBy(out vehicle))
            {
                return false; // something bad happened
            }

            if (vehicle != null)
            {
                Container vehicleContains = vehicle.Contains;

                if (vehicleContains != null)
                {
                    if (vehicleContains.IsDriverOrPassenger(this))
                    {
                        return true;
                    }
                }
            }

            return false;
        }



        /*  public Entity IsCarriedByAgentInAllegiance(Allegiance.Allegiance allegiance)
          {
            
              Entity container;
              if (GetContainedBy(out container))
              {
                  IStorage storage = container.Contains as IStorage;
                  if (storage != null)
                  {
                      if (container.EntityType.IntelligenceType != null)
                      {
                          if (container.Intelligence.Allegiance == allegiance)
                          {
                              return container;
                          }
                      }

                  }
              }

              return null;
          }*/

        public StorageTarget? StoredPermanentlyIn
        {
            get
            {
                Entity container;
                if (GetContainedBy(out container))
                {
                    if (container != null)
                    {
                        IStorage iStorage = container.Contains as IStorage;
                        if (iStorage != null)
                        {
                            if (IsPermanentStorage(container.EntityType))
                            {
                                Storage storage = iStorage.GetStoredIn(this);

                                if (storage != null)
                                    return new StorageTarget(container.ID, storage.ID);

                                // return  container.EntityID; 
                            }

                        }
                    }
                }

                return null;
            }
        }

        // public bool IsOfferedForTrade { get; set; }

        /*  public StorageTarget? OfferedForTradeIn
          {
              get
              {
                  Entity container;
                  if (GetContainedBy(out container))
                  {
                      if (container != null)
                      {
                          TerminalContainer terminal = container.Contains as TerminalContainer;
                          if (terminal != null)
                          {
                              Storage storage = terminal.GetStoredAsTradeOffer(this);
                              if (storage != null)
                              {
                                  return new StorageTarget(container.ID, storage.ID);
                              }
                          }

                      }
                  }

                  return null;
              }
          }*/

        /*
        public EntityID? StoredPermanentlyIn
        {
            get
            {
                Entity container;
                if (GetContainedBy(out container))
                {
                    if (container != null)
                    {
                        IStorage storage = container.Contains as IStorage;
                        if (storage != null)
                        {
                            if (IsPermanentStorage(container.EntityType))
                            {
                                return container.EntityID; // storage.GetStoredIn(this);
                            }

                        }
                    }
                }

                return null;

            }
        }

        public StorageID? StoredPermanentlyStorageID
        {
            get
            {
                Entity container;
                if (GetContainedBy(out container))
                {
                    if (container != null)
                    {
                        IStorage iStorage = container.Contains as IStorage;
                        if (iStorage != null)
                        {
                            if (IsPermanentStorage(container.EntityType))
                            {
                                Storage storage = iStorage.GetStoredIn(this);

                                if (storage != null)
                                    return storage.ID;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public StorageCondition StoredPermanentlyCondition
        {
            get
            {
                Entity container;
                if (GetContainedBy(out container))
                {
                    if (container != null)
                    {
                        IStorage iStorage = container.Contains as IStorage;
                        if (iStorage != null)
                        {
                            if (IsPermanentStorage(container.EntityType))
                            {
                                Storage storage = iStorage.GetStoredIn(this);

                                if (storage != null)
                                    return storage.StorageConditions;
                            }

                        }
                    }
                }

                return null;

            }
        }
        
       */

        public static bool IsPermanentStorage(EntityType entityType)
        {
            return (entityType.IntelligenceType == null // terminals have intelligence too...
                    && entityType.LocomotorType == null)
                    || (entityType.ContainerType != null && entityType.ContainerType is TerminalContainerType);
        }

        /// <summary>
        /// returns false if the container and entity has been destroyed..
        /// </summary>
        /// <param name="carrier"></param>
        /// <returns></returns>
        public bool CarriedByAgent(out Entity carrier)
        {
            carrier = null;

            Entity container;
            if (GetContainedBy(out container))
            {
                if (container != null)
                {
                    AgentStorage containerAsAgentStorage = container.Contains as AgentStorage;
                    if (containerAsAgentStorage != null)
                    {
                        carrier = container;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// returns false if the container and entity has been destroyed..
        /// gets the first Storage container in the containment chain
        /// </summary>
        public bool StoredIn(out Storage storage)
        {
            storage = null;

            Entity container;
            if (GetContainedBy(out container))
            {
                if (container != null)
                {
                    IStorage containerAsStorage = container.Contains as IStorage;
                    if (containerAsStorage != null)
                    {
                        storage = containerAsStorage.GetStoredIn(this);
                    }
                }

                return true;
            }

            return false;
        }


        /* public bool GetStoredIn(out Entity storage)
         {
           
             storage = null;
             Entity container;
             if (GetContainedBy(out container))
             {
                 IStorage storageContainer = container.Contains as IStorage;
                 if (storageContainer != null)
                 {
                     storage = container;

                 }
             }
             else 
             { // KABOOM
                 return false;
             }

                
             return true;
           
         }*/

        /// <summary>
        /// get the entity we are serving as replenishment item for
        /// </summary>
        /// <param name="replenishes"></param>
        /// <returns></returns>
        public bool GetReplenishes(out Entity replenishes)
        {
            replenishes = null;
            Entity container;
            if (GetContainedBy(out container))
            {
                if (container != null)
                {
                    IReplenishes storageContainer = container.Contains as IReplenishes;
                    if (storageContainer != null
                        && storageContainer.IsReplenishing(this.ID))
                    {
                        replenishes = container;

                    }
                }
            }
            else
            { // KABOOM
                return false;
            }

            return true;
        }

        public EntityID? Replenishes
        {
            get
            {
                Entity replenishes;

                if (GetReplenishes(out replenishes))
                {
                    if (replenishes != null)
                    {
                        return replenishes.EntityID;
                    }
                }

                return null;
            }

        }

        public bool GetUpgradesFor(out Entity upgrades)
        {
            upgrades = null;
            Entity container;
            if (GetContainedBy(out container))
            {
                if (container != null)
                {
                    IUpgrades storageContainer = container.Contains as IUpgrades;
                    if (storageContainer != null
                        && storageContainer.IsUpgrade(this.ID))
                    {
                        upgrades = container;

                    }
                }
            }
            else
            { // KABOOM
                return false;
            }

            return true;
        }

        public EntityID? UpgradeFor
        {
            get
            {
                Entity upgraded;

                if (GetUpgradesFor(out upgraded))
                {
                    if (upgraded != null)
                    {
                        return upgraded.EntityID;
                    }
                }

                return null;
            }

        }


        /// <summary>
        /// NOTE! returns false if the container is invalid (destroyed). This triggers destruction of the entity too...
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public bool GetContainedBy(out Entity container)
        {
            if (ContainedBy != null)
            {
                container = Entity.FindByID(ContainedBy.Value);

                if (container == null)
                {
                    // clean up:
                    HandleInvalidContainerEntityBug();

                    // signal the client that something wrong has happened:
                    return false;
                }

            }
            else
            {
                container = null;
            }

            return true;
        }

        public bool GetContainedBy(out Container container)
        {
            container = null;
            Entity containingEntity;

            if (GetContainedBy(out containingEntity))
            {
                if (containingEntity != null)
                {
                    container = containingEntity.Contains;
                }
            }
            else
            {
                return false;
            }


            return true;
        }



        /// <summary>
        /// call this to clean up after the ContainedBy ID has resolved to null. => a bug.
        /// </summary>
        private void HandleInvalidContainerEntityBug()
        {
            ContainedBy = null;

            // destroy the entity (we don't have any coordinates, so we can't place it anywhere)
            this.Destroy();
        }

        #endregion


        #region structures


        public List<HouseholdID> Households
        {
            get
            {
                Container container = Contains;

                if (container != null)
                {
                    IResidence residence = container as IResidence;
                    if (residence != null)
                    {
                        if (residence.Residence != null)
                            return residence.Residence.Households;
                    }
                }

                return null;

            }

        }

        /// <summary>
        /// TODO: move to EntityLock
        /// </summary>
        public int? Residents
        {
            get
            {
                //Residence residenceComponent;
                Container container = Contains;

                if (container != null)
                {
                    IResidence residence = container as IResidence;
                    if (residence != null)
                    {
                        if (residence.Residence != null)
                            return residence.Residence.Residents;
                    }
                }

                return null;
            }

            set
            {
                Container container = Contains;

                if (container != null && value.HasValue)
                {
                    IResidence residence = container as IResidence;
                    if (residence != null)
                    {
                        residence.Residence.Residents = value.Value;
                    }
                }
            }
        }

        #endregion


        /// <summary>
        /// for Ammo type items
        /// </summary>
        public int? NoOfRounds
        {
            get
            {
                Item item;
                if (Find(out item))
                {
                    if (item.Ammunition != null)
                    {
                        return item.Ammunition.NoOfRounds;
                    }
                }

                return null;
            }
        }


        #region Replenish

        public bool HasEnoughFuel(float neededFuel)
        {
            if (Contains != null)
            {
                IHasReplenishItems iReplenishes = Contains as IHasReplenishItems;
                if (iReplenishes != null
                    && iReplenishes.ReplenishItems != null
                    && iReplenishes.ReplenishItems.RequiresFuel != null)
                {
                    return iReplenishes.ReplenishItems.RequiresFuel.HasEnoughFuel(neededFuel);
                }
            }

            /* RequiresEnergy energy;
             if (Find(out energy))
             {
                 if (energy.RequiresFuel != null)
                 {
                     return energy.RequiresFuel.HasEnoughFuel(neededFuel);
                 }
             }*/

            return true;
        }

        public bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload)
        {
            return NeedsReload(sharedKnowledge, this, out itemToReload);
        }

        public static bool NeedsReload(SharedKnowledge sharedKnowledge, IKnownEntityData entityData, out IKnownEntityData itemToReload)
        {
            itemToReload = null;

            if (entityData.EntityType.ContainerType != null && entityData.EntityType.ContainerType is MagazineContainerType)
            {
                int maxCapacity = ((MagazineContainerType)entityData.EntityType.ContainerType).MaxCapacity;

                int limit = (int)(0.3f * maxCapacity);

                int? totalAmmo = entityData.GetTotalAmmo();
                if (totalAmmo.HasValue && totalAmmo < limit)
                {
                    itemToReload = entityData;
                    return true;
                }

                return false;
            }

            if (entityData.EntityType.IntelligenceType != null
                && entityData.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                IKnownEntityData weaponData;
                foreach (var item in entityData.IntrinsicWeapons) // .IntelligenceType.IntrinsicWeaponTypes)
                {
                    if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item.Value, out weaponData)))
                    {
                        if (NeedsReload(sharedKnowledge, weaponData, out itemToReload))
                        {
                            return true;
                        }
                    }
                }

            }

            return false;
        }

        public bool NeedsRepair() //SharedKnowledge sharedKnowledge) //, out RepairAction? action, out EntityID? partToFix)
        {
            return NonLivingEntity.NeedsRepair(); //sharedKnowledge); //, this, out action, out partToFix);
        }

        public RepairPackage ComputeBestRepairPackage() //SharedKnowledge sharedKnowledge, out RepairAction? action, out EntityID? partToFix)
        {
            return NonLivingEntity.ComputeBestRepairPackage();
        }

        public bool HasEnoughAmmo(EntityType ammoType, int noOfRounds)
        {
            if (Contains != null)
            {
                MagazineContainer mag = Contains as MagazineContainer;
                if (mag != null)
                {
                    return mag.HasAmmo(ammoType, noOfRounds);
                }
            }

            return false;
        }

        public int? GetTotalAmmo()
        {
            if (Contains != null)
            {
                MagazineContainer mag = Contains as MagazineContainer;
                if (mag != null)
                {
                    return mag.GetTotalAmmo();
                }
            }

            return null;
        }

        public bool HasEnergyForDuration(float durationInDays)
        {
            if (Contains != null)
            {
                IHasReplenishItems iReplenishes = Contains as IHasReplenishItems;
                if (iReplenishes != null
                    && iReplenishes.ReplenishItems != null)
                {
                    return iReplenishes.ReplenishItems.HasEnergyForDuration(durationInDays);
                }
            }

            /* RequiresEnergy energy;
             if (Find(out energy))
             {
                 return energy.HasEnergyForDuration(durationInDays);
             }*/

            return true;
        }


        #endregion

        public Entity()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Entity(EntityType entityType, bool isStructureBeingPlaced = false, bool isItemBeingProduced = false)
        {
           
            this.EntityType = entityType;

            if (EntityType.KeyName == "entity:dog")
            {

            }


            #region IDs

            // create all the Ids:
            AddToLookup(); //entityID.HasValue);          
            ((ILookUp<IComposite, CompositeID>)this).AddToLookup();
            ((ILookUp<IDetectable, DetectableID>)this).AddToLookup();
            if (EntityType.IntelligenceType != null)
            {
                ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
            }

            #endregion

            simStateFlags = new BitMask64(typeof(StateModifier));

            showStatusRegulator = CreateShowStatusIconRegulator("Entity", EntityType);

            Components = new Dictionary<Type, Component>();

            BulkChangedEvent = new IDActionEvent<float>();

            if (EntityType.BiologicalType != null)
            {
                Add(new BiologicalEntity(this));
            }


            if (EntityType.IntelligenceType != null)
            {
                Add(new Intelligence(this)); // depends on BiologicalEntity! PUT THIS AFTER BIOLOGICAL

                if (EntityType.IntelligenceType.Skills != null)
                {
                    foreach (var kvp in EntityType.IntelligenceType.Skills)
                    {
                        Intelligence.SetSkill(kvp.Key, kvp.Value);
                    }
                }
            }

            if (EntityType.Person != null)
            {
                // depends on BioEntity and on Intelligence:                
                Add(new Person(this));
            }


            if (EntityType.ContainerType != null)
            {
                // handle component/type key issue in components dictionary;
                Container container = null;

                container = EntityType.ContainerType.CreateContainer(this);

              /*  if (EntityType.ContainerType is StorageContainerType)
                {
                    container = new Entities.Containers.Components.StorageContainer(this);
                }
                else if (EntityType.ContainerType is MagazineContainerType)
                {
                    container = new MagazineContainer(this);
                }
                else if (EntityType.ContainerType is AgentStorageType)
                {
                    container = new AgentStorage(this);
                }
                else if (EntityType.ContainerType is VehicleContainerType)
                {
                    container = new VehicleContainer(this); // Add(new Vehicle(this));
                }
                else if (EntityType.ContainerType is HomeContainerType)
                {
                    container = new HomeContainer(this);
                }
                else if (EntityType.ContainerType is ReplenishContainerType)
                {
                    container = new ReplenishContainer(this);
                }
                else if (EntityType.ContainerType is ToolContainerType)
                {
                    container = new ToolContainer(this);
                }
                else if (EntityType.ContainerType is WorkshopContainerType)
                {
                    container = new WorkshopContainer(this);
                }
                else if (EntityType.ContainerType is TerminalContainerType)
                {
                    container = new TerminalContainer(this);
                }
                else if (EntityType.ContainerType is UpgradableContainerType)
                {
                    container = new UpgradableContainer(this);
                }
                else if (EntityType.ContainerType is UpgradableContainerType)
                {
                    container = new UpgradableContainer(this);
                }*/

                if (container != null)
                {
                    // cache this...
                    Contains = container;
                }
            }

            if (EntityType.HeatingType != null)
            {
                Add(new Heating());
            }

            /* moved to Initialize..
             * 
             if (EntityType.RenderableType != null)
             {               

                 ConstructRenderableIfNull();                
             }*/

            if (EntityType.ThreatType != null)
            {
                Add(new Threat(this));
            }


            if (EntityType.ToolType != null)
            {
                /*if (EntityType.ToolType.PrepareAction != null)
                {*/
                UWGame.SimSide.Items.Tool tool = new UWGame.SimSide.Items.Tool(this);
                Add(tool);
                // }
            }

            if (EntityType.LocomotorType != null)
            {
                Locomotor locomotor = new Locomotor(this);

                Add(locomotor);
            }


            if (EntityType.StructureType != null)
            {
                Add(new Structure(this));
                if (isStructureBeingPlaced)
                {
                    Structure.State = Buildings.StructureStates.BeingPlaced;
                }
            }

            if (EntityType.TreeType != null)
            {
                //Tree = new Trees.Tree(this);

                if (EntityType.GatheringSiteType != null)
                {
                    // create gathering site automatically 
                    //If there was a gatheringsitetype defined in the treeloader 
                    GatheringSite = new GatheringSite(this);

                }

                Tree tree = new Trees.Tree(this);
                Add(tree);

                // without a type??

                // !!! we don't have to specify this!
                // EdgeLayout = new EdgeLayout(this);                
            }

            if (EntityType.ItemType != null)
            {
                Add(new Item(this));
            }

            if (EntityType.TerrainType != null)
            {
                if (EntityType.TerrainType.PathType != null)
                {
                    Add(new TerrainPath(this));
                }

            }

            if (EntityType.DirectionalLayoutType != null)
            {
                DirectionalLayout = new DirectionalLayout(this);
            }

            if (EntityType.PointLayoutType != null)
            {
                PointLayout = new PointLayout(this);
            }

            /*
            if (EntityType.GeometryLayoutType != null)
            {
                GeometryLayout = new GeometryLayout(this);                
            }*/

            AdoptSimStateInfo(EntityType.DefaultSimState); // this does not stamp the geolayout, that happens in Update. Creates collidable

            if (Collidable == null) // needed for non-geolayouts
            {
                CreateCollidable(); // needs CurrentSimState
            }

            // #AVAILABLEFIX
            AvailableSharedSpecialActions = new List<ProcessType>(); 
            
            if (EntityType.SharedSpecialActionTypes != null) 
            {
                foreach (var item in EntityType.SharedSpecialActionTypes) 
                {
                    if (item.SpecialActionEnabledAtStart == true) 
                    {
                        AvailableSharedSpecialActions.Add(item);                         
                    }
                }
            }


            /* if (EntityType.RequiresEnergyType != null)
             {
                 Add(new RequiresEnergy(this));
             }*/

            if (EntityType.GatheringSiteType != null)
            {
                GatheringSite = new GatheringSite(this);
            }


            if (EntityType.NonLivingType != null || EntityType.StructureType != null) // ?!?!? // EntityType.ItemType != null || EntityType.StructureType != null)
            {
                Add(new NonLivingEntity(this));
            }
            /*  else
              {
                  IsNowCompleted();
              }*/

            if (EntityType.RockType == null && EntityType.TreeType == null)
            {
                Add(new SimEffectsComponent(this));

            }


            if (EntityType.SensorType != null)
            {
                Add(new Sensor(this));
            }

            if (EntityType.SubstancesType != null)
            {
                Add(new SubstanceComponent(this));
            }

            if (EntityType.CommunicatorType != null)
            {
                Add(new Communicator(this));
            }

            if (EntityType.BodyType != null)
            {
                BodyComponent body = new BodyComponent(this);
                Add(body);


                /* if (Intelligence != null)
                 {
                     // set up a listener for hitpoints
                     body.HitpointsChanged += Intelligence.body_HitpointsChanged;
                 }*/
            }

            if ((Structure == null || isStructureBeingPlaced == false) // Structure.State != StructureStates.BeingPlaced)
                && isItemBeingProduced == false) // an item being produced will be given a parts list 
            {
                // structures get their parts when construction begins
                // we cannot initialize it before Site has been set??
                CreateParts();
            }

            CreateBioSystemsRegulator();

            if (EntityType.CustomFields != null)
            {
                foreach (var item in EntityType.CustomFields)
                {
                    Entity.SetPropertyValue(ref CustomFields, item.Key, item.Value);    
                }        
            }

            // OLD:
            // Bulk = 0.5f + (float)Globals.Instance.RandomPredictable.NextDouble();

            //Initialize();

            // new: set random rotation (can be overridden later):
            SetRotationAndDir((float)(The.Sim.GameplayRandomGenerator.NextDouble("Entity") * MathHelper.TwoPi));

            //TODO HERE AT THE BOTTOM OF ENTITY CTOR, lets construct a stub renderable if Renderable is still null.
            ConstructDumbRenderableIfNull();
            // If the entity is living then it should be added to the living quad tree

            //  CreateRegulators();


           // RecomputeUpdateInterval();

            The.Sim.AddEntity(this);


            List<ActionSets> defaultActions;
            EntityType.EventActions.TryGetValue(EntityEventHooks.Created, out defaultActions);
            Goal.FireEventActions(this, null, defaultActions);

        }

        public void CreateBioSystemsRegulator()
        {
            if (EntityType.BiologicalType != null)
            {
                bioSystemsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.Constants.UpdateIntervalForBioEntity, "BiologicalEntity");
            }
        }

        public static List<ProcessType> GetSharedSpecialActionsForDisplay(IKnownEntityData entityData)
        {
            if (entityData.EntityType.SharedSpecialActionTypes != null && entityData.EntityType.SharedSpecialActionTypes.Count > 0)
            {
                List<ProcessType> list = new List<ProcessType>();
                foreach (var item in entityData.EntityType.SharedSpecialActionTypes)
                {
                    if (!item.ShowDisabledSpecialAction)
                    {
                        // filter disabled actions, if desired:
                        if (entityData.AvailableSharedSpecialActions.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        list.Add(item);
                    }
                }

                return list;
            }

            return null;
        }

        public static Regulator CreateShowStatusIconRegulator(string source, EntityType entityType)
        {
            if (entityType.GetShowMarkerWindowMode() == Entities.EntityType.ShowMarkerWindowMode.ByStatus)
            {
                Regulator showStatusRegulator = new Regulator(The.Client.ClientRandomGenerator, 1d, source, Regulator.Modes.Client); // we want update while paused, too

                return showStatusRegulator;
            }

            return null;
        }


        public bool IsTimeToShowStatusMarkerWindow()
        {
            return showStatusRegulator.IsReady();
        }

        /// <summary>
        /// during post-load, we don't want to overwrite snapshotted renderable values.
        /// </summary>
        /// <param name="initialize"></param>
        private void ConstructRenderableIfNull(bool initialize = false)
        {
            if (Renderable == null) 
            {
                if (The.Client == null)
                {
                    //if we are running headless, then construct the special dumb Renderable
                    // TODO DECOUPLE make dumb Renderable extremely lightweight and safe
                    ConstructDumbRenderableIfNull();
                    return;
                }

                Renderable = RenderableFactory.Produce(this, EntityType.RenderableTypeMode, snapshotRenderable);
                //Renderable = RenderableFactory.Produce(this, EntityType.RenderableTypeMode, snapshotRenderable);
                

                bool setModelProperties = true;

                if (snapshotRenderable != null)
                    setModelProperties = false; // don't overwrite texture, scale and model

                if (initialize) // NEW: always initialize. but exclude values that overwrite snapshotrenderable settings.
                    Renderable.Initialize(setModelProperties);

            }
        }


        private void ConstructDumbRenderableIfNull()
        {

        }




        public bool ContentCanBeAccessedBy(Entity accessingEntity)
        {
            return accessingEntity == this // can always access itself..
                || EntityType.ContainerType.CanTransactWithContainer(accessingEntity.EntityType);
        }




        public bool HasBeenPlaced()
        {
            return mapPosition != null;//mapPosition in Entity now nullable 2014-07-04
        }

        public void Add(Component c)
        {
            Components.Add(c.GetType(), c);
        }



        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (PersonEntity != null)
            {
                text.Append(Name);
                text.Append(" ");
            }
            else
            {
                text.Append(EntityType.Name); // + "(" + Name + ")");
            }

            // text.Append(MapPosition);

            return text.ToString();
        }




        /// <summary>
        /// for client only..?
        /// </summary>
        /// <param name="coneWidth"></param>
        /// <param name="coneLength"></param>
        /// <returns></returns>
        public float GetAttackRange(out float? coneWidth, out float? coneLength)
        {
            coneWidth = null;
            coneLength = null;
            Container container = Contains;
            if (container != null && container is AgentStorage)
            {
                return ((AgentStorage)container).GetMountedWeaponRange(out coneWidth, out coneLength);
            }

            if (EntityType.IntelligenceType != null && Intelligence.IntrinsicWeapons != null)
            {
                foreach (var item in Intelligence.IntrinsicWeapons)
                {
                    Entity weapon = Entity.FindByID(item.Value);

                    if (weapon != null)
                    {
                        return weapon.GetWeaponRange(ref coneWidth, ref coneLength);
                    }
                }
            }

            return 0f;
        }

        public float GetWeaponRange(ref float? coneWidth, ref float? coneLength)
        {
            float maxRange = 0f;
            if (EntityType.ItemType.WeaponType != null)
            {
                foreach (var item in EntityType.ItemType.WeaponType.AttackTypes)
                {
                    if (item.MaxRange.HasValue && item.MaxRange > maxRange)
                    {
                        maxRange = item.MaxRange.Value;

                        if (item.AreaAttack != null)
                        {
                            ConeAttack coneAttack = item.AreaAttack as ConeAttack;
                            if (coneAttack != null)
                            {
                                coneWidth = coneAttack.WidthInDegrees;
                                coneLength = coneAttack.Length;
                            }
                        }
                    }
                }
            }
            return maxRange;
        }

        public string ToLink(bool useUpperCase = false)
        {
            string displayName = GetDisplayName();
            if (useUpperCase)
            {
                displayName = displayName.ToUpper(Config.Culture);
            }

            return Hyperlink.ToLink(displayName, (long)id);
        }

     

        /// <summary>
        /// why is this needed...? I imagine it is for activating in-game features. So there can be entities that don't appear on the game field, but exist off-map...
        /// 
        /// TODO: make this the method that creates Renderable
        /// Maybe move this to Site property???
        /// </summary>
        /// <param name="game"></param>
        public void InitializeModelAndOnScreenFunctionality()
        {
            if (IsOnPlaySite()) // wrap the whole call in this..?
            {
                if (EntityType.RenderableTypeMode != null)
                {
                    ConstructRenderableIfNull(true);
                }
            }


            if (Renderable != null && Renderable.RenderAsModel != null)
            {
                //  Renderable.RenderAsModel.Initialize(game);

                // if (EntityType.Renderable.RenderAsModelType.ModelData.HasSteerableFrontWheels)
                if (Renderable.RenderAsModel.ModelData.HasSteerableFrontWheels)
                {   // couldn't do it before the animator was created:
                    Add(new SteerableFrontWheels(this));
                }

                if (EntityType.KeyName == "entity:forestGuardian") // should this be data driven...?
                {
                    AttachPoint attachor = Renderable.RenderAsModel.ModelData.BackAttachor; // GameData.Instance.AllModels["forestguardian"].BackAttachor;
                    //  AttachPoint attachee = GameData.Instance.AllModels["bush"].BackAttachee;

                    AttachPoint attachee;
                    Vector3 translationToUse, rotationToUse;

                    Renderable renderableToAttach = The.Client.Renderer.GetFreeAttachableRenderable("bush");
                    RenderAsModel.GetAttachTransformations(renderableToAttach, attachor, AttacheePoint.Back, null, out attachee, out translationToUse, out rotationToUse);

                    Renderable.AttachObject(renderableToAttach.RenderAsModel, attachor, attachee, AttacheePoint.Back, translationToUse, rotationToUse);
                }
            }


            // NEW: init parts in proper sequence:
            if (Parts != null)
            {
                foreach (var item in Parts)
                {
                    item.InitializeModelAndOnScreenFunctionality();
                }
            }

        }

        /// <summary>
        /// This is separated from construction to allow paramters to be set inbetween.
        /// </summary>
        /// <param name="ageGroup"></param>
        public void Initialize(Site site, Allegiances.Allegiance allegiance = null, Expedition expedition = null, ThreatGroup threatGroup = null)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                this.Site = site;


                if (Body != null)
                {
                    Body.Initialize(); // NEW: sets bulk for robots
                }

                // Init Biological first, since Household and Allegiance depend on it:
                if (BiologicalEntity != null)
                {
                    BiologicalEntity.Initialize();
                }

                if (EntityType.IntelligenceType != null)
                {
                    Intelligence.Initialize(allegiance);
                }

                if (EntityType.Person != null)
                {

                    PersonEntity.Household = new Household(expedition, this); //expedition); //Site.GetFirstPlayerExpedition()); // null); //Site);
                    //  PersonEntity.Household.AddMember(this);


                    //   PersonEntity.SkinColor = EntityType.Person.GetRandomSkinColor();
                    //   PersonEntity.HairColor = EntityType.Person.GetHairColor(PersonEntity.SkinColor, false, BiologicalEntity.Age);

                    //  UpdateRenderableRandomColors();   
                    //    SecondaryColor = EntityType.Person.HairColorsForRendering[PersonEntity.HairColor];
                    //    PrimaryColor = EntityType.Person.SkinColorsForRendering[PersonEntity.SkinColor]; // skin
                }


                /*  if (EntityType.IntelligenceType != null)
                  {
                      Intelligence.Initialize(allegiance);
                  }*/

                if (EntityType.ThreatType != null)
                {
                    Threat threat;
                    if (Find(out threat))
                    {
                        if (threatGroup == null && allegiance != null)
                        {
                            // use the allegaince threat group if the threat group was not provided (PlaceGameEntities):
                            threatGroup = allegiance.ThreatGroup;
                        }

                        threat.ThreatGroup = threatGroup;
                    }
                }


                Tree tree;
                if (Find(out tree))
                {
                    tree.Initialize();
                }

                Item item;
                if (Find(out item))
                {
                    item.Initialize();
                }

                if (EntityType.Person != null)
                {
                    PersonEntity.Initialize();

                }
            }

            // NEW: init parts in proper sequence:
            if (Parts != null)
            {
                foreach (var part in Parts)
                {
                    part.Initialize(site, allegiance, expedition, threatGroup);
                }
            }
        }

        public static Entity CreateAndInitEntity(EntityType entityType, Site site, List<Entity> parts = null, float? bulk = null, Allegiance allegiance = null)
        {
            Entity item = new Entity(entityType, isItemBeingProduced: true);

            if (item.NonLivingEntity != null)
            {
                item.NonLivingEntity.SetPartsOrCreateNew(parts);
            }

            if (bulk.HasValue)
            {
                item.Bulk = bulk.Value;
            }


            item.Initialize(site, allegiance); // invokes the parts too
            item.InitializeModelAndOnScreenFunctionality();

            return item;
        }

        public void SetPosition(Vector3 location) //Vector3 location)  //Point pos, Common.Direction dir)
        {
            Point pos = MapManager.WorldPosToTile(location);
            Common.Direction dir = The.Map.WorldLocationToDirectionWithinTile(location);
            SetPosition(pos, dir, location);
        }

        public void SetPosition(Point pos, Common.Direction dir) //Vector3 location)  //Point pos, Common.Direction dir)
        {
            SetPosition(pos, dir, MapManager.TileAndDirectionToWorldPos(pos, dir)); // MapManager.TileToWorldPos(pos)); 
        }

        /// <summary>
        /// Also for interface rendering purposes.
        /// </summary>
        /// <param name="location"></param>
        public void SetPosition(Point pos, Common.Direction? dir, Vector3 location) //Point pos, Common.Direction dir)
        {
            // Point pos = MapManager.WorldPosToTile(location); // UWGame.SimSide.Instance.Map.world
            // MapManager.WorldPosToTilePosAndPositionWithinTile


            /*  if (Structure != null)
              {
                  // Error...? same as mapposition?
                  Structure.TopLeftMapPosition = pos;
              }*/

            if (DirectionalLayout != null)
            {
                if (EntityType.DirectionalLayoutType.Fixed8DirPlacement == true)
                {
                    //Common.Direction dir = UWGame.SimSide.Instance.Map.WorldLocationToDirectionWithinTile(location);
                    Location = MapManager.GetWorldCoordsFromDirection(pos, dir.Value);
                }
                else
                {
                    Location = location;
                }

                //MapPosition = MapManager.WorldPosToTile(Location);

                DirectionalLayout.EdgePosition = dir.Value;

                //NEW: use base of building as reference
                // Point tilePos = MapManager.WorldPosToTile(parent.Location);
                /*BaseCenterTile*/
                //
            }
            else
            {
                if ((EntityType.PointLayoutType != null && EntityType.PointLayoutType.GridAlignedPlacement == true)
                 || (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null && CurrentSimState.GeometryLayoutType.GridAlignedPlacement == true)) //(EntityType.GeometryLayoutType != null && EntityType.GeometryLayoutType.GridAlignedPlacement == true))
                {
                    location = MapManager.SubTileToWorldPos3(MapManager.WorldPosToSubtile(location));
                }

                Location = location;
            }


            Renderable.LocationChanged(); // this is necessary to compute MapPosition, otherwise the billboards won't get drawn!

        }



        /// <summary>
        /// assigns the parts and/or creates new ones if they were not supplied
        /// </summary>
        /// <param name="suppliedParts"></param>
      /*  public void SetPartsOrCreateNew(List<Entity> suppliedParts)
        {
            if (EntityType.Parts != null)
            {
                // NEW: we don't create parts in the ctor.
                Parts = new List<Entity>();

                Entity suppliedPart;

                foreach (var part in EntityType.Parts)
                {
                    for (int i = 0; i < part.Value; i++)
                    {
                        if (suppliedParts != null)
                        {
                            suppliedPart = suppliedParts.Find(p => p.EntityType == part.Key);
                        }
                        else
                        {
                            suppliedPart = null;
                        }

                        if (suppliedPart != null)
                        {
                            SetPart(suppliedPart);

                            suppliedParts.Remove(suppliedPart);
                        }
                        else
                        {
                            // else create a new part:
                            CreatePart(part.Key);
                        }
                    }

                }

                //  Parts.AddRange(parts);

            }
        }*/



        public bool DoDamage(float damage) //, bool rollForChanceToDestroy)
        {
            bool partWasDestroyed = false;

            if (IsCompositeRoot)
            {
                // damage integrity at the root level, and only for composites..
                partWasDestroyed = NonLivingEntity.DoIntegrityDamage(damage); //, rollForChanceToDestroy);
            }


            if (Parts != null)
            {
                // do parts
                Entity part;

                for (int i = Parts.Count - 1; i >= 0; i--) // iterate backwards - parts may dissappear
                {
                    part = Parts[i];
                    partWasDestroyed = part.DoDamage(damage /*, rollForChanceToDestroy*/) | partWasDestroyed;
                }

            }
            else
            {
                partWasDestroyed = NonLivingEntity.DoConditionDamage(damage, null/*, rollForChanceToDestroy*/) | partWasDestroyed;
            }

            NonLivingEntity.ComputeConditionOfComposite();

            return partWasDestroyed;
        }




        private bool footprintIsDirty = false;
        public bool FootprintIsDirty
        {
            get
            {
                return footprintIsDirty;
            }

            set
            {
                if (footprintIsDirty != true)
                {
                    footprintIsDirty = true; //always sets to true, cannot be unset externally
                    RecomputeUpdateInterval();
                }
            }
        }

        /// <summary>
        /// called when moving between sites -> new PlaySite
        /// </summary>
        /// <param name="location"></param>
        /// <param name="container"></param>
        /// <param name="structureState"></param>
        /// <param name="expedition"></param>
        public void TransferToPlaySite(Vector3? location, Entity container, Entity.StructureState? structureState, Expedition expedition)
        {
            // keep the previous owner - passing null removes ownership...
            //IOwner owner = LookUpOwners.FindByID(OwnedBy);

            PlaceEntityOnPlaySite(location, container, structureState, null, expedition);


            ComeOnline(); // start the AI we need

        }

        /// <summary>
        /// called when moving between sites -> new OtherSite
        /// 
        /// PlaceOffSite is called when moving PlaySite -> between sites
        /// </summary>
        /// <param name="container"></param>
        /// <param name="structureState"></param>
        /// <param name="expedition"></param>
        public void TransferToOtherSite(Site site, Entity container, Expedition expedition)
        {
            PlaceEntityOnOtherSite(site, container, false, null, expedition);

        }

        /// <summary>
        /// to be called when moving off-playsite. Handle this special case that differs from an entity becoming a part or contained.
        /// 
        /// This case is actually similar to an entity being contained in a container that we cannot see into.
        /// 
        /// Call this on the root entity and leaf entities will be handled also.
        /// </summary>
        public void PlaceEntityOffSite(GeodeticCoordinate newCoords) //Point oldMapPosition)
        {
            if (MapPosition.HasValue)
            {
                
                TerrainTile thisTile = The.Map.GetTile(MapPosition.Value);

                foreach (Allegiances.Allegiance allegiance in thisTile.AllegiancesThatSeeThisTile)
                {
                    // we keep transports/people in memory if we are in contact
                    // other allegiances should forget them, critters and so on, as well as any food items onboard...    

                    // all transports that can communicate are also agents ( = have Intelligence)!

                   
                    if (this.Intelligence != null && Intelligence.Allegiance == allegiance)
                    {
                        // agent from same allegiance is leaving
                        // don't leave a memory fact.
                        
                        CommunicationMethod? method;
                        bool canCom = Communicates.IsInCommunicationRange(allegiance, this, out method, null, newCoords);

                        // if we can still communicate, we: 
                        // 1. remove the entity from play site collections such as knowledge quad trees
                        // if not: 
                        // do 1. and
                        // 2. do the same as when off-site vehicles or entities are removed from terminals
                       
                        if (canCom)
                        {
                            DeletePlaySiteKnowledgeOfEntity(allegiance); // delete the information we don't need without changing status seen/unseen

                           // SeeEntityMovingOffSite(allegiance);                           
                        }
                        else
                        {
                            DeletePlaySiteKnowledgeOfEntity(allegiance);
                                                       
                            HandleEntityMovingOutOfCommunicationRange(allegiance);
                        }
                    }
                    else
                    {
                        // non-agents(?), or agents from other allegiances - unsee as usual, create a memoryfact on the playsite
                        IKnownEntityData entityData;
                        
                        if (allegiance.SharedKnowledge.GetKnownData(this.ID, out entityData) == EntityResult.SeenDirectly)
                        {
                            allegiance.SharedKnowledge.UnSeeEntity(this); // create an mf - since the entity may return?
                        }
                    }
                }

                Sensor sensor;
                if (Find(out sensor))
                {
                    sensor.UnseeTilesInRange();
                }


                DisableCollisions(); // TODO: test to see if collisions are enabled again when the harpy arrives

                RemoveFromAgentQuadTree(); // transport is maybe not an agent... remove passengers too..
                if (Contains != null)
                {
                    Contains.IterateContained(e =>
                        {
                            e.RemoveFromAgentQuadTree();
                            e.DeleteAttachedTriggers();
                        });
                }

                DeleteAttachedTriggers(); // no reason to have triggers off-site either          


                Site = null;
                Coords = newCoords;
                Location = null;

                // call this after setting the location/site - other allegiances will be notified and have their Known Entity collections updated:
                // this is here to make the barge rejoin its owning expedition after leaving... because foreign allegiances are currently not allowed at a site, it had to join the player allegiance temporarily
                if (EntityType.IntelligenceType != null
                    && Intelligence.Allegiance.Site.IsPlaySite
                    && OwnedBy.HasValue)
                {
                    IOwner owner = LookUpOwners.FindByID(OwnedBy);
                    if (owner != null)
                    {
                        Expedition owningExpedition = owner as Expedition;
                        if (owningExpedition != null && owner.Allegiance != Intelligence.Allegiance)
                        {
                            ChangeExpedition(owningExpedition, true);

                        }
                    }
                }

                // NEW - #RenderableCrash: put renderable to sleep:
                bool intervalChanged;
                Renderable.RecomputeUpdateInterval(out intervalChanged);

                 if (EntityType.IntelligenceType != null)
                 {
                     Intelligence.Brain.RemoveAllSubgoals(); // NEW
                 }

                 // reset regulators too!!
                 ResetPlaySiteRegulators();

                 if (Contains != null)
                 {
                     Contains.ResetPlaySiteRegulators();
                 }

                 foreach (var item in Components)
                 {
                     item.Value.ResetPlaySiteRegulators();
                 }

            }

            RecomputeUpdateIntervalOnLeafs();        // RecomputeUpdateInterval();
        }


        private void ResetPlaySiteRegulators()
        {
            CreateBioSystemsRegulator();

        }

       /* private void SeeEntityMovingOffSite(Allegiances.Allegiance allegiance)
        {
            DeletePlaySiteKnowledgeOfEntity(allegiance);

        }*/

        private void DeletePlaySiteKnowledgeOfEntity(Allegiances.Allegiance allegiance)
        {
            allegiance.SharedKnowledge.DeletePlaySiteKnowledgeOfEntity(this.ID);

            IterateLeafs((leafEntity) => allegiance.SharedKnowledge.DeletePlaySiteKnowledgeOfEntity(leafEntity.ID), allegiance);
        }

        private void IterateLeafs(Action<Entity> action, Allegiance detectingAllegiance)
        {
            //*** Handle leafs. ****
            // see inside containers:
            if (EntityType.ContainerType != null)
            {
                // only see inside containers that the entity can interact through/enter.
                // entities should be able to see inside containers that are owned by/belong to the same allegiance                         
                if (detectingAllegiance == null || detectingAllegiance.SharedKnowledge.CanSeeInsideContainer(this))
                {
                    //Container contains = Contains;

                    // see inside recursively     
                    Contains.IterateContained(
                        action);
                    //Contains.IterateContained((containedEntity) => allegiance.SharedKnowledge.DeleteMemoryOfEntity(containedEntity.ID, containedEntity.DetectableID, true));
                }
            }

            //handle parts here:
            if (Parts != null)
            {
                foreach (var part in Parts)
                {
                    action(part);
                    //allegiance.SharedKnowledge.DeleteMemoryOfEntity(part.ID, part.DetectableID, true);
                }
            }

        }

        /// <summary>
        /// called when 
        /// 1. entity moves out of terminal, for instance by being loaded
        /// 2. when agents leave the playsite
        /// perhaps split these two cases...
        /// 
        /// what about when moving out of comm range in between sites? is that handled?
        /// </summary>
        /// <param name="allegiance"></param>
        public void HandleEntityMovingOutOfCommunicationRange(Allegiances.Allegiance allegiance)
        {            
            // this seems wrong. If a member is out of range we will need a memory fact to represent him. 
            // But this is probably a case that hasn't been done before (members are never memory facts) so it would need testing.
           
            DeleteMemoryOfEntityAndLeafs(allegiance);
        }

        private void DeleteMemoryOfEntityAndLeafs(Allegiances.Allegiance allegiance)
        {
            // does not delete contained entities:
            allegiance.SharedKnowledge.DeleteMemoryOfEntity(this.ID, this.DetectableID, true);

            // this does:
            IterateLeafs((leafEntity) => allegiance.SharedKnowledge.DeleteMemoryOfEntity(leafEntity.ID, leafEntity.DetectableID, true), allegiance);
        }



        /// <summary>
        /// TODO: make this symmetric with PlaceEntityOnPlaySite!
        /// </summary>
        /// <param name="container"></param>
        /// <param name="isFinishedStructure"></param>
        /// <param name="newOwner"></param>
        /// <param name="newExpedition"></param>
        public bool PlaceEntityOnOtherSite(Site otherSite, Entity container, bool? isFinishedStructure, SetOwnerInfo? setOwnerInfo, //IOwner newOwner, 
            Expedition newExpedition, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, bool simulateJoinedExpeditionNow = true) // bool offerForSale = false)
        {

            Site = otherSite; // NEW!

            SetOwnerAndExpedition(setOwnerInfo, newExpedition, simulateJoinedExpeditionNow);


            if (container != null)
            {
                if (!container.Contains.AddToContain(this, compartment: compartment, placeInStorage: placeInStorage)) // isOfferedForSale: offerForSale))
                {
                    // the item is now in an illegal state and will CTD if not handled properly.
                    return false;
                }
            }


            RecomputeUpdateIntervalOnLeafs();
            

            return true;
        }


        public void PlaceEntityOnPlaySite(Vector3 location, AddRandomOffset addRandomOffset, Entity creatorOfItem, StructureState? structureState,
            SetOwnerInfo? setOwnerInfo, Expedition newExpedition = null, bool isProductionOutput = false, bool assertContainment = true, UpgradeCategory upgradeCategory = null) // bool isUpgrade = false)
        {
            location = MapManager.FindFreeLocation(location, EntityType.ItemType != null, addRandomOffset, creatorOfItem);

            PlaceEntityOnPlaySite(location, null, structureState, setOwnerInfo, newExpedition, isProductionOutput: isProductionOutput, assertContainment: assertContainment, upgradeCategory: upgradeCategory); // isUpgrade: isUpgrade);
        }

        public enum StructureState { Ordered, Unfinished, Finished }


        public struct SetOwnerInfo
        {
            public IOwner NewOwner;
            public GiveNewOwnerKnowledge GiveNewOwnerKnowledge;

            public SetOwnerInfo(IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge)
            {
                NewOwner = newOwner;
                GiveNewOwnerKnowledge = giveNewOwnerKnowledge;
            }

            public SetOwnerInfo(IOwner newOwner)
            {
                NewOwner = newOwner;
                GiveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes;
            }
        }

        /// <summary>
        /// Places the new entity in the PlaySite world, either in the open or inside a container.
        /// This is intended to be the only Place method in existence!
        /// Idempotence - should have no effect when called twice...
        ///         
        /// For structures, if the parameter isFinishedStructure is true, the structure will be placed fully formed. If false, it will be placed as an ordered structure without parts.
        /// 
        /// Call right after Initialize!
        /// Make sure that placement is valid BEFORE calling!
        /// 
        /// newOwner can be null for objects who does not have an owner
        /// </summary>
        /// <param name="pos"></param>
        public bool PlaceEntityOnPlaySite(Vector3? location, Entity container, StructureState? structureState,
            SetOwnerInfo? setOwnerInfo,
            Expedition newExpedition = null, StorageCompartment? placeProductsInCompartment = null, StorageCondition placeInStorage = null, bool isProductionOutput = false, EntityID? anchorID = null,
            bool assertContainment = true, bool simulateJoinedExpeditionNow = true, UpgradeCategory upgradeCategory = null) // bool isUpgrade = false)
        {
            ConstructRenderableIfNull(true); // NEW!

            accessPoint = null;


            if (location.HasValue) // is null for arriving members becaue they are still contained until told to disembark...
            {
                Site = The.Sim.PlaySite; // NEW

                Point pos = MapManager.WorldPosToTile(location.Value);
                Common.Direction dir = The.Map.WorldLocationToDirectionWithinTile(location.Value);

                SetPosition(pos, dir, location.Value); // <- allegiances will see the new entity here

                // create footprint:
                if (DirectionalLayout != null)
                {
                    DirectionalLayout.Place(dir);
                }
                else if (PointLayout != null)
                {
                    PointLayout.Place();
                }

                // every GeometryLayout gets collisionProxy, too.
                if (GeometryLayout != null)
                {
                    Debug.Assert(PointLayout == null);//GeometryLayout does not play nice with PointLayout

                    if (Collidable == null)
                    {
                        CreateCollidable();
                    }

                    GeometryLayout.Place(GeoPlaceMode.NewFeature);
                }

                EnableCollisions();

                /*  if (The.Sim.Mode == Sim.EngineMode.Game
                      && Collidable != null
                      && EntityType.CausesCollisions())
                  {
                      // start creating collisions
                      The.CollisionManager.AddCollidable(Collidable);
                  }*/

            }
            else if (container != null)
            {
                if (!container.Contains.AddToContain(this, placeProductsInCompartment, placeInStorage: placeInStorage, isProductionOutput: isProductionOutput, assertContainment: assertContainment, upgradeCategory: upgradeCategory)) // isUpgrade: isUpgrade)) //, isOfferedForSale: offerForSale))
                {
                    // the item is now in an illegal state and will CTD if not handled properly.
                    return false;
                }
            }



            if (GeometryLayout != null && Structure == null
                && !The.Sim.AllTerrainEntities.Contains(EntityID))
            {
                The.Sim.AllTerrainEntities.Add(EntityID);
            }

            if (EntityType.BiologicalType != null)
            {
                // NEW: all bios are potential prey. also humans...
                Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes["prey"]);
                AttachTrigger(trigger);
            }


            if (IsNonHumanAnimal())
            {
                /* Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes["prey"]); 
                 AttachTrigger(trigger);*/

                Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes["creature"]);
                AttachTrigger(trigger);
            }

            /*
            if (EntityType.IntelligenceType != null)
            {
                if (The.AgentQuadTree != null)
                {
                    if (!The.AgentQuadTree.Contains(this))
                    {
                        The.AgentQuadTree.AddObject(this, PlaySiteLocation.ToVector2());
                    }
                    else
                    {
                        The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
                    }

                }
            }
            */

            if (Structure != null)
            {
                Structure.PlaceBuilding();

                if (structureState == StructureState.Ordered) // isFinishedStructure != true) 
                {
                    // start the flicker animation to acknowledge building placement to user:
                    Renderable.SetOverlayFlashing(1000f);

                }
                else if (structureState == StructureState.Finished)
                {
                    // placing a finished structure. Parts will already have been created, in the ctor...
                    // for unfinished structures, this call will be made later, at the end of construction:
                    Structure.ConstructionFinished(anchorID, true);
                }
                else if (structureState == StructureState.Unfinished)
                {


                }
            }

            Tree tree;
            if (Find(out tree))
            {
                tree.Place();
            }


            if (Renderable != null)
            {
                Renderable.SetToParentLocation();
            }


            Locomotor locomotor;
            if (Find(out locomotor))
            {
                locomotor.CurrentMoveTarget = this.PlaySiteLocation;
            }

            if (EntityType.GatheringSiteType != null)
            {
                GatheringSite.SetLocation(this.PlaySiteLocation);
            }

            // change ownership if needed - also set knowledge of item according to the 8 cases
            /*
            * no owner:
               in FOW    - unknown - needs RollToDetect to be detected |Done|

               not in FOW - unknown - needs RollToDetect to be detected |Done|

               in container with Remembered status  - unknown - needs RollToDetect to be detected |Done|

               in container with DirectlySeen status  -  SeenDirectly 
            * 
                * owner:
               in FOW  - with Remembered status|Done|

               not in FOW - Directly Seen|Done|

               in container with Remembered status - Remembered |Done|

               in container with DirectlySeen status - Directly Seen |Done|
            */

            SetOwnerAndExpedition(setOwnerInfo, newExpedition, simulateJoinedExpeditionNow);



            // add knowledge of entity to all allegiances that see its tile (if they can see the container it is in, they can also see the contents)
            SeeEntityPlacedInContainerByAllegiances();

            // agents (robots) should always see their own parts:
            SeeOwnPartsForRobots();
            
            //RecomputeUpdateInterval();
            RecomputeUpdateIntervalOnLeafs();

            return true;
        }

        /// <summary>
        /// contained and parts won't know when the root moves to/from playsite - but this influences their update frequency.
        /// </summary>
        void RecomputeUpdateIntervalOnLeafs()
        {
            RecomputeUpdateInterval();

            if (Parts != null)
            {
                foreach (var item in Parts)
                {
                    item.RecomputeUpdateIntervalOnLeafs();
                }
            }

            if (Contains != null)
            {
                Contains.IterateContained(e => e.RecomputeUpdateIntervalOnLeafs());
            }
        }

        private void SetOwnerAndExpedition(SetOwnerInfo? setOwnerInfo, Expedition newExpedition, bool simulateJoinedExpeditionNow)
        {
          
            if (setOwnerInfo.HasValue)
            {
                ChangeOwnership(setOwnerInfo.Value.NewOwner, setOwnerInfo.Value.GiveNewOwnerKnowledge);

                if (setOwnerInfo.Value.NewOwner != null && EntityType.IntelligenceType != null)
                {
                    // NEW: created robots and dogs also get assigned to the owner expedition - MOVE THIS?
                    newExpedition = setOwnerInfo.Value.NewOwner as Expedition;
                }
            }

            // migrants, robots etc. change allegiance here. Crew should not!! (how?)
            // Also don't switch owned vehicles like skimmer or barge - but we have to, otherwise their Update call will fail.
            if (newExpedition != null && EntityType.IntelligenceType != null)
            {
                ChangeExpedition(newExpedition, simulateJoinedExpeditionNow);
            }

        }



        /// <summary>
        /// goes up one level
        /// </summary>
        /// <param name="container"></param>
        /// <param name="location"></param>
        public bool GetContainerOrLocation(ref Entity container, ref Vector3? location)
        {
            if (ContainedBy != null)
            {
                return GetContainedBy(out container);

            }
            else
            {
                location = this.AccessPoint;
                return true;
            }

        }

        private void SeeOwnPartsForRobots()
        {
            if (IsOnPlaySite() && EntityType.IntelligenceType != null && Parts != null)
            {
                foreach (Entity part in Parts)
                {
                    Intelligence.Allegiance.SharedKnowledge.SeeDetectable(part, doAssert: false);// AddKnowledgeOfItemToNewOwner(newOwnerToUse, this);
                }
            }
        }

        /// <summary>
        /// checks if the item is owned by any of the ownership classes that an entity can be involved in
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        public bool IsOwnedByUs(IKnownEntityData food)
        {
            if (food.OwnedBy == null || EntityType.Person == null)
            {
                return false;
            }
            else
            {
                Person person = PersonEntity;
                if (((IOwner)person).ID == food.OwnedBy)
                {
                    return true;
                }

                if (((IOwner)Intelligence.CurrentExpedition).ID == food.OwnedBy)
                {
                    return true;
                }

                if (((IOwner)person.Household).ID == food.OwnedBy)
                {
                    return true;
                }

                return false;
            }

        }

        /// <summary>
        /// To be called when a robot is finished, or an agent is created.
        /// 
        /// Re-called when moving to the PlaySite
        ///      
        /// do branching so only the modules that are needed for othersite/playsite are created
        /// 
        /// make sure the method can be called twice and do nothing with modules are already existing/running - Idempotency!
        /// </summary>
        public void ComeOnline(bool isSpawning = false)
        {

            Intelligence intelligenceComponent;
            if (Find(out intelligenceComponent))
            {
                intelligenceComponent.ComeOnline(); // this handles both play site and other site cases
            }

            if (EntityType.CommunicatorType != null)
            {
                Communicator communicator;
                if (Find(out communicator))
                {
                    communicator.GainContact();
                }
            }

            if (EntityType.ContainerType != null && EntityType.ContainerType is TerminalContainerType)
            {
                TerminalContainer terminal = Contains as TerminalContainer;

                terminal.ComeOnline();

            }

            Entity container;
            if (GetContainedBy(out container) && container != null)
            {
                container.Contains.NotifyFunctionalContainedEntity(this);
            }           

            if (IsOnPlaySite())
            {
                AttachTriggers(EntityType.Triggers);

                Sensor sensor;
                if (Find(out sensor))
                {
                    // depends on Intelligence
                    sensor.IsActive = true;

                    // detect the surroundings now that we are online
                    sensor.SeeTilesInRange(MapPosition.Value);
                }


                if (EntityType.PolledEvents != null)
                {
                    List<PolledEventType> list;
                    if (EntityType.PolledEvents.TryGetValue(Scope.Entity, out list))
                    {
                        foreach (var item in list)
                        {
                            if (IsOnPlaySite() || item.PlaySiteOnly == false)
                            {
                                PolledEvent polledEvent = Site.EventManager.AddPolledEvent(item.KeyName, this.EntityID);
                            }
                        }
                    }

                }

                if (EntityType.IntelligenceType != null)
                {
                    if (The.AgentQuadTree != null)
                    {
                        if (!The.AgentQuadTree.Contains(this))
                        {
                            The.AgentQuadTree.AddObject(this, PlaySiteLocation.ToVector2());
                        }
                        else
                        {
                            The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
                        }

                    }
                }
            }

            List<ActionSets> defaultActions;
            EntityType.EventActions.TryGetValue(EntityEventHooks.ComeOnline, out defaultActions);
            Goal.FireEventActions(this, null, defaultActions, isSpawning: isSpawning);

            RecomputeUpdateInterval();
        }



        /// <summary>
        /// Should only be called from PlaceNewEntityInWorld as that is the unified place were we add newly created objects to the world.
        /// </summary>
        private void SeeEntityPlacedInContainerByAllegiances()
        {

            if (ContainedBy != null)
            {
                Entity container = Entity.FindByID(containedBy.Value);
                //Go through all allegiances that can see the tile the container is placed on

                if (container.MapPosition.HasValue)
                {
                    TerrainTile tile = The.Map.GetTile(container.MapPosition.Value);
                    foreach (var allegiance in tile.AllegiancesThatSeeThisTile)
                    {
                        //IS the container seen by the current allegiance
                        IKnownEntityData data;
                        if (allegiance.SharedKnowledge.GetKnownData(ContainedBy.Value, out data) == EntityResult.SeenDirectly)
                        {
                            if (allegiance.SharedKnowledge.CanSeeInsideContainer(container))
                            {
                               // allegiance.SharedKnowledge.SeeDetectable(this);
                                allegiance.SharedKnowledge.SeeDetectableIfRelevant(this);
                            }
                        }
                    }
                }
            }

        }

        public bool IsNonHumanAnimal()
        {
            if (BiologicalEntity == null)
            {
                return false;
            }
            if (Intelligence == null)
            {
                return false;
            }
            if (PersonEntity != null)
            {
                return false;
            }
            return true;
        }


        private void CreateCollidable()
        {
            Vector2? location = null;
            if (Location.HasValue) // will still be null when constructing. It will be set later...
            {
                location = Location.Value.ToVector2();
            }

            if (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null) // use selectedGeoLayout. The Type property should be default.
            {
                if (CurrentSimState.GeometryLayoutType.Shapes != null)
                {
                    Collidable = CreateCollidableFromShapes(this, CurrentSimState.GeometryLayoutType.Shapes, location);
                }

                if (CurrentSimState.GeometryLayoutType.SelectionShapes != null)
                {
                    SelectionShape = CreateCollidableFromShapes(this, CurrentSimState.GeometryLayoutType.SelectionShapes, location);
                }
            }
          /*  if (EntityType.GeometryLayoutType != null) // TODO: use selectedGeoLayout instead. The Type property should be default.
            {
                if (EntityType.GeometryLayoutType.Shapes != null)
                {
                    Collidable = CreateCollidableFromShapes(this, EntityType.GeometryLayoutType.Shapes, location);
                }

                if (EntityType.GeometryLayoutType.SelectionShapes != null)
                {
                    SelectionShape = CreateCollidableFromShapes(this, EntityType.GeometryLayoutType.SelectionShapes, location);
                }
            }*/
            else if (EntityType.CollidableType != null)
            {

                if (EntityType.CollidableType.CircleRadius.HasValue)
                {
                    float size = EntityType.CollidableType.CircleRadius.Value;

                    // create & register:                 
                    Collidable = new Collidable<Entity>(this, location, new Vector2(size));
                    Collidable.BeCircle();
                }
                else if (EntityType.CollidableType.Shapes != null)
                {
                    Collidable = CreateCollidableFromShapes(this, EntityType.CollidableType.Shapes, location);

                    /*
                    float size = 2 * EntityType.CollidableType.Shapes[0].Radius;  //double the radius

                    Collidable = new Collidable<Entity>(this, Location.ToVector2(), new Vector2(size));

                    foreach (CollideShape2D shape in EntityType.CollidableType.Shapes)
                    {
                        Collidable.AddChildShape(shape);
                    }*/
                }
            }
            /* else if (EntityType.ItemType != null) // TODO: in time, we may want to make items on the ground collide (but not move!) to push agents away. But take care when producing - placement!
             {
                 float size = 3f;

                 CollisionProxy = The.CollisionManager.AddCollidable(this, Location.ToVector2(), new Vector2(size));
                 CollisionProxy.BeCircle();
             }*/
            else if (EntityType.IntelligenceType != null)
            {
                float size = 8f;

                Collidable = new Collidable<Entity>(this, location, new Vector2(size));
                Collidable.BeCircle();
            }

            if (Collidable != null)
            {
                Collidable.FlipHorizontally = FlipHorizontally;
            }

            if (SelectionShape != null)
            {
                SelectionShape.FlipHorizontally = FlipHorizontally;
            }
        }

        private static Collidable<Entity> CreateCollidableFromShapes(Entity entity, CollideShape2D[] shapes, Vector2? location)
        {
            float size = 2 * shapes[0].Radius;  //double the radius

            Collidable<Entity> Collidable = new Collidable<Entity>(entity, location, new Vector2(size));

            foreach (CollideShape2D shape in shapes)
            {
                Collidable.AddChildShape(shape);
            }

            return Collidable;
        }




        /*   public void UpdateAge(float addYears)
           {
               if (BiologicalEntity != null)
               {
                   BiologicalEntity.AgeGroup.UpdateAge(addYears);
               }

           }*/




        public SurfaceType.TransportType GetTransportType()
        {
            if (DrivingVehicle != null)
            {
                Entity vehicle = Entity.FindByID(DrivingVehicle.Value); // never null

                return ((VehicleContainerType)vehicle.EntityType.ContainerType).Transport;
            }
            else return SurfaceType.TransportType.Foot;

        }

        public static float GetSelectionRadius(IKnownEntityData entityData)
        {
            if (entityData.BoundingRadius3D < MapManager.tileSizeOver2)
            {
                return MapManager.tileSizeOver2;
            }

            return entityData.BoundingRadius3D;//value set currently by RenderAsModel.Initialize() TODO DECOUPLE

        }

        public static bool ContainsIntelligence(IKnownEntityData entityData, SharedKnowledge sharedKnowledge, ref List<IKnownEntityData> agentsInside)
        {
            if (entityData.ContainedEntities != null)
            {
                for (int i = entityData.ContainedEntities.Count - 1; i >= 0; i--)
                {
                    EntityID contained = entityData.ContainedEntities[i];
                    IKnownEntityData containedData;
                    if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(contained, out containedData)))
                    {
                        if (containedData.EntityType.IntelligenceType != null)
                        {
                            Common.AddToList(ref agentsInside, containedData);
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        /* public bool ContainsIntelligence()
         {
             if (Intelligence != null)
                 return true;

            
             if (Structure != null && Structure.HasIntelligenceInside())
             {
                 return true;
             }

             if (Vehicle != null && Vehicle.HasIntelligenceOnboard())
             {
                 return true;
             }

             return false;
         }*/

        /* public double GetMaximumTransportationSpeed()
         {
             if (DrivingVehicle != null)
             {
                 return DrivingVehicle.Locomotor.CurrentMaximumSpeed;
             }
             else return this.Locomotor.CurrentMaximumSpeed;

         }*/

        /*    public double GetMoveSpeed()
            {
                if (DrivingVehicle != null)
                {
                    return DrivingVehicle.Locomotor.MoveSpeed;
                }
                else return this.Locomotor.MoveSpeed;

            }*/

        /*
        public Vector3? FacingNormal
        {
            get
            {
                
                if (Locomotor != null)
                {
                    return Locomotor.FacingNormal;
                }

                return null;

            }
        }*/


        /*  public Vector3 GetLocationWhenInVehicle()
          {
            
              return PlaySiteLocation; // Location;
          }*/

        public bool CanBeHunted(Allegiances.Allegiance byAllegiance) // IKnownEntityData entity)
        {
            return CanBeHunted(this, byAllegiance);
        }

        public static bool CanBeHunted(IKnownEntityData entity, Allegiances.Allegiance byAllegiance) // IKnownEntityData entity)
        {
            if (entity != null
                // && entity.EntityType.Person == null
                && entity.EntityType.BiologicalType != null
                && entity.EntityType.IntelligenceType != null
                && entity.AllegianceID != byAllegiance.ID) // Allegiance.AllegianceType.Player)
            {
                return true;
            }

            return false;
        }

        public bool CanSetStockpileSettings(EntityGroupID byOwner)
        {
            return CanSetStockpileSettings(this, byOwner);
        }



        public static bool CanSetStockpileSettings(IKnownEntityData entityData, EntityGroupID owner)
        {
            if (entityData.HasItemStorage                
                && entityData.EntityType.StructureType != null)
            {
                IHasItemStorageType hasItemStorage = entityData.EntityType.ContainerType as IHasItemStorageType;

                if (hasItemStorage != null && hasItemStorage.AllowsStockpiling)
                {
                    EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
                    if (entityGroup != null)
                    {
                        return entityGroup.Contains(entityData); // does this test owner..?
                    }
                }
            }

            return false;
        }

        public bool CanSetTradeOfferSettings(EntityGroupID byOwner)
        {
            return CanSetTradeOfferSettings(this, byOwner);
        }

        public static bool CanSetTradeOfferSettings(IKnownEntityData entityData, EntityGroupID owner)
        {
            if (entityData.EntityType.ContainerType != null &&
                entityData.EntityType.ContainerType is TerminalContainerType)
            {
                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
                if (entityGroup != null)
                {
                    return entityGroup.Contains(entityData);
                }
            }

            return false;
        }


        public bool CanBeUpgraded(EntityGroupID byOwner)
        {
            return CanBeUpgraded(this, byOwner);
        }

        public static bool CanBeUpgraded(IKnownEntityData entityData, EntityGroupID owner)
        {
            if (entityData.EntityType.ContainerType != null &&
                entityData.EntityType.ContainerType.CanBeUpgraded)
            {
                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
                if (entityGroup != null)
                {
                    return entityGroup.Contains(entityData);
                }
            }

            return false;
        }

        /// <summary>
        /// can only have one trigger of each type.
        /// </summary>
        /// <param name="trigger"></param>
        public void AttachTrigger(Trigger trigger)
        {
            // NEW: First make sure that we only have one trigger of each type attached:
            DeleteTriggerOfType(trigger.TriggerType);

            /*
            if (trigger.MoveTrigger == Trigger.TriggerMovement.Attached)
            {
                triggerCooldownTimers.Add(trigger,new Regulator(trigger.maxNumberOfChecksPerSecond));
            }*/
            attachedTriggers.Add(trigger);
            The.Sim.TriggerSystem.RegisterTrigger(trigger);
        }



        public void DeleteTrigger(Trigger trigger)
        {

            attachedTriggers.Remove(trigger);
            if (The.Sim != null)
            {
                The.Sim.TriggerSystem.DeleteTrigger(trigger);
            }
        }

        public void DeleteTriggerOfType(TriggerType type)
        {
            Trigger trigger;
            // start from the back:
            for (int index = attachedTriggers.Count - 1; index >= 0; index--)
            {
                trigger = attachedTriggers[index];
                if (trigger.TriggerType == type)
                {
                    DeleteTrigger(trigger);
                }
            }
        }

        public void DeleteTriggers(TriggerType[] triggerTypes)
        {
            if (triggerTypes != null)
            {
                foreach (var triggerType in triggerTypes)
                {
                    DeleteTriggerOfType(triggerType);
                }
            }
        }



        /*  public void ProductionFinished(bool createParts = false)
          {            
              if (Structure != null)
              {
                  Structure.ConstructionFinished(createParts); // structure parts are created when construction begins, not when the structure is placed
              }

              // start systems running (AI, sensor, power...)
              ComeOnline();

          }*/



        public string HisHerOrIts()
        {
            if (EntityType.BiologicalType != null)
            {
                if (EntityType.Person != null)
                {
                    if (BiologicalEntity.CasteType.Reproduction == Reproduction.Male)
                    {
                        return "his";
                    }
                    else if (BiologicalEntity.CasteType.Reproduction == Reproduction.Female)
                    {
                        return "her";
                    }
                }
                else return "its";
            }

            return "its";
        }

        /// <summary>
        /// kills a living entity - produces a carcass item
        /// </summary>
        public Entity Kill(OwnerID? ownerOfCarcassID, CauseOfDeath? causeOfDeathToLog = null, EntityID? killer = null)
        {

            if (EntityType.BiologicalType == null)
                return null;


            if (causeOfDeathToLog.HasValue)
            {
                string causeOfDeath = LogDeathMessage(causeOfDeathToLog);

                LogDeathStatistics(causeOfDeathToLog, causeOfDeath);
            }


            Entity carcass = null;



            carcass = Entity.CreateAndInitEntity(EntityType.BiologicalType.CarcassType, Site, null, Bulk);

            if (EntityType.Person != null)
            {
                // ###HACK! the human dead anim is 180 degrees rotated???
                carcass.SetRotationAndDir(Common.WrapAngleBetweenZeroAndTwoPi(Rotation + MathHelper.Pi));
            }

            if (carcass.EntityType.RenderableTypeMode != null)
            {
                //First, snap the renderable to the entity's position (no more lerping is possible):
                Renderable.SetToParentLocation();

                // set the anim before copying the renderable to the carcass (it has no anim tracks):
                /*   Renderable.SetAnimationActionStateFlag(AnimAction.Dying);
                   Renderable.SetAnimationStateFlag(AnimModifier.Post);
                   */
                Renderable.SetAnimationActionStateFlag(AnimAction.Dying); // works??
                Renderable.SetAnimationStateFlag(AnimModifier.Post);

                Renderable.UpdateAnimationConditionState();


                // copy the living entity's renderable to get matching texture, scale etc.                
                carcass.Renderable = RenderableFactory.Produce(Renderable, carcass); // new ClientSide.Renderables.Renderable(Renderable, carcass); 
                // carcass.Renderable.SetToParentLocation();

            }


            Entity killerEntity = null;

            IOwner ownerOfCarcass = null;



            if (Intelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player
                && Intelligence.IsIndependent())
            {
                The.Sim.PlaySite.EventManager.PlayerEntityHasDied(this, carcass, causeOfDeathToLog);
            }


            if (killer != null)
            {
                killerEntity = Entity.FindByID(killer.Value);
            }

            ownerOfCarcass = LookUpOwners.FindByID(ownerOfCarcassID);



            Container container;

            try
            {

                if (killerEntity != null)                    
                {
                    if (killerEntity.Intelligence != null) // why is this necessary?
                    {
                        if (ownerOfCarcass != null)
                        {
                            if (killerEntity.PersonEntity != null)
                            {
                                killerEntity.Intelligence.Memory.SetRecentlyHuntedCarcass(carcass.EntityID);
                            }
                        }

                        killerEntity.Intelligence.Allegiance.Statistics.AddKillEvent(this.EntityType);
                    }
                    else
                    {
#if !RELEASE
                        throw new Exception("Killer??");
#endif
                    }
                }

                IsDead = true;

                string triggerKey;
                triggerKey = "entityDied";

                // don't attach the trigger
                // make it short lived and include some extra info
                The.Sim.TriggerSystem.RegisterTrigger(
                    new Trigger(null, // parent is gone! 
                               Location,
                               GameData.Instance.AllTriggerTypes[triggerKey],
                               new Tuple<EntityID, EntityType>(EntityID, EntityType)));


                GetContainedBy(out container);

                Vector3? location = Location;

                if (Contains != null)
                {
                    Contains.IterateContained(e => Contains.Uncontain(e)); // eject all
                }

            }
            catch (NullReferenceException e) // https://steamcommunity.com/app/284100/discussions/4/208684375414789158/
            {
                string info = "";

                try
                {                    
                    info += "EntityType: " + EntityType + "\n";
                    
                    if (killerEntity != null)
                    {
                        info += "Killer: " + killerEntity.GetDisplayName();
                    }
                    else
                    {
                        info += "Killer: null";
                    }

                    if (carcass != null)
                    {
                        info += "Carcass: " + carcass.GetDisplayName();
                    }
                    else
                    {
                        info += "Carcass: null";
                    }
                }
                catch(Exception){}

                throw new Exception("Kill error #4, " + info, e);
            }

            /* OLD: buggy
            Destroy(); // can't use location or Site after this

            //Lets place the body after we have destoyed the entity as then the agents items will be dropped inside his location before it gets
            //swapped with the body????
           */


            if (container != null)
            {
                // set owner?
                //PlaceEntity does a lot more than this...
                container.SwitchEntities(this, carcass);
            }
            else
            {
                carcass.PlaceEntityOnPlaySite(location.Value, AddRandomOffset.No, null, null, new Entity.SetOwnerInfo(ownerOfCarcass, GiveNewOwnerKnowledge.No));

                carcass.Renderable.SetToParentLocation();

                if (ownerOfCarcass != null)
                {
                    ownerOfCarcass.Allegiance.LogProductionStatistics(carcass, null);
                }
            }

            Destroy(); // can't use location or Site after this

            //We had a crash that happened sometimes due to the carcass being placed after the haulingjobs where created
            //This sometimes crashed the game due to the location of the entity had not been set yet.
            //So be carefull when changing things here and make sure that the hauling jobs are created after the carcass has been placed.

            // create a haul job immediately if we own it:
            if (ownerOfCarcass != null)
            {
                HaulingJobManager.CreateHaulingJobsForItemOutOfBand(carcass, ownerOfCarcass.OwnedEntities);
            }

            //  RecomputeUpdateInterval(); // why..? // #starvefix

            return carcass;


        }

        private string LogDeathMessage(CauseOfDeath? causeOfDeathToLog)
        {
            string causeOfDeath;
            switch (causeOfDeathToLog.Value)
            {
                case CauseOfDeath.Starvation:
                    causeOfDeath = "starvation";
                    break;
                case CauseOfDeath.Wounds:
                    causeOfDeath = HisHerOrIts() + " wounds";
                    break;
                default:
                    causeOfDeath = "unknown causes";
                    break;
            }

            The.Client.AddLogEvent(Intelligence.Allegiance, The.Client.Log.GeneralEvent, this, "has died from " + causeOfDeath + ".");
            return causeOfDeath;
        }

        private void LogDeathStatistics(CauseOfDeath? causeOfDeathToLog, string causeOfDeath)
        {
            if (Intelligence.Allegiance.Statistics != null
                && GroupStatistics.GatherStatisticsForEntity(this))
            {
                foreach (var stat in Intelligence.Allegiance.Statistics.Ratings)
                {
                    if (stat.Key == RatingTypes.Security
                        && causeOfDeathToLog.Value == CauseOfDeath.Wounds)
                    {
                        SecurityStatisticsForAllegiance securityStat = (SecurityStatisticsForAllegiance)stat.Value;
                        securityStat.AddViolentEvent(this, "Died from " + causeOfDeath + ".", ViolentEventType.Death);
                    }

                    if (stat.Key == RatingTypes.Food
                        && causeOfDeathToLog.Value == CauseOfDeath.Starvation)
                    {
                        FoodStatistics foodStat = (FoodStatistics)stat.Value;

                        DataPoint<EntityID> starvationDeathDataPoint = new DataPoint<EntityID>()
                        {
                            Value = ID,
                            Time = The.Sim.DateAndTime.CurrentTimeDateYear
                        };

                        Common.AddToList(ref foodStat.starvingDeaths, starvationDeathDataPoint);
                    }
                }
            }
        }



        public enum AddRandomOffset { Yes, No }

        /// <summary>
        /// call PlaceInWorld instead of this
        /// </summary>
        /// <param name="location"></param>
        /// <param name="newOwner"></param>
        /// <param name="addRandomOffset"></param>
        /*   public void PlaceNewEntityInTheOpen(Vector3 location, IOwner newOwner, AddRandomOffset addRandomOffset = AddRandomOffset.Yes) 
           {
               PlaceOnGroundWithSmallRandomOffset(location, addRandomOffset);

               ChangeOwnership(newOwner); 
               The.Map.GetTile(MapPosition.Value).AddEntity(this);
           }*/



        /// <summary>    
        /// add a small offset to avoid 'Point' piles or stacks, but stay within this tile.
        /// </summary>
        /// <param name="location"></param>
        /*   public void SetGroundLocationWithSmallRandomOffset(Vector3 location, AddRandomOffset addRandomOffset = AddRandomOffset.Yes, Entity creatorOfItem = null) 
           {
               location = MapManager.FindFreeLocation(location, EntityType.ItemType != null, addRandomOffset, creatorOfItem);

               SetPosition(location);
           }*/



        /*   private Vector3? ScanInDirectionFromCenter(
               Vector3 directionFromBaseCenter, 
               UWGame.SimSide.Maps.MapManager.TerrainValue[][] terrain, 
               out float? distance, 
               ref Vector3 locationToScanFrom)
           {
               float dotGap = MapManager.subTileSizeOver2; // 0.1f;
               Point subtile;
               distance = null;
               for (float incr = 0; incr < 200f; incr += dotGap)
               {
                   Vector3 dot = locationToScanFrom + directionFromBaseCenter * incr;

                   if (The.Map.ClampWorldPosition(dot) != dot)
                   {
                       // we are off map... try another direction
                       return null;
                   }

                   subtile = MapManager.WorldPosToSubtile(dot); // The.Map.world
                   if (terrain[subtile.X][subtile.Y] != 0)
                   {
                       Vector3 foundPoint = MapManager.SubTileToWorldPos(subtile).ToVector3();
                       distance = (foundPoint - location).Length();
                       return foundPoint;
                   }
               }
               return null;
           }*/

        
       
        /// <summary>
        /// remove Attraction effects here
        /// </summary>
        /// <param name="oldAllegiance"></param>
        private void ResetAfterEmigrating(Allegiance oldAllegiance)
        {
            if (PersonEntity != null)
            {
                PersonEntity.Personality.Attraction.Remove(oldAllegiance.ID);
            }

        }

        /// <summary>
        /// Changes the expedition of the entity. Also checks wether the entity belong to the same allegiance as the expedition.
        /// If it doesn't, it calls ChangeAllegiance too.
        /// 
        /// Other allegiances/expeditions are not allowed on other sites!
        /// </summary>
        /// <param name="newExpedition"></param>
        public void ChangeExpedition(Expedition newExpedition, bool simulateJoinedNow)
        {
            if (newExpedition == null)
                return;

            if (EntityType.IntelligenceType == null)
                return;

            bool isInPlayerAllegiance = false;
            if (Intelligence.Allegiance != null &&
                Intelligence.Allegiance.AllegianceType == AllegianceType.Player)
            {
                isInPlayerAllegiance = true;
            }


            if (newExpedition.Allegiance.ID != AllegianceID)
            {
                ChangeAllegiance(newExpedition.Allegiance);
                                

                if (isInPlayerAllegiance == false && Intelligence.Allegiance.AllegianceType == AllegianceType.Player)
                {
                    // fire triggers, events etc. after entity is fully init'ed:  

                    List<ActionSets> defaultActionSets;
                    EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.SwitchedToPlayerAllegiance, out defaultActionSets);

                    Goal.FireEventActions(this, null, defaultActionSets, null);
                }

               
            }

            Expedition oldExpedition = Intelligence.CurrentExpedition;

            if (oldExpedition != newExpedition)
            {
                if (oldExpedition != null)
                {
                    oldExpedition.RemoveMember(this);
                }

                // Intelligence.CurrentExpedition = newExpedition;
                newExpedition.AddMember(this);

                if (PersonEntity != null)
                {
                    // TODO: how do we handle multi-member households... they need to be moved together instead of broken up...
                    if (oldExpedition != null)
                    {
                        oldExpedition.RemoveHousehold(PersonEntity.Household);
                    }

                    Debug.Assert(PersonEntity.Household != null, "household cannot be null.");

                    newExpedition.AddHousehold(PersonEntity.Household);
                }

                if (simulateJoinedNow)
                {
                    Intelligence.Memory.SetTimepointForJoiningExpedition();
                }
                
            }

        }

        /// <summary>
        /// Changes the allegiance of the entity. Also checks wether the entity is already on the same site as the new allegiance. If it isn't it calls ChangeSite.
        /// </summary>
        /// <param name="newAllegiance"></param>
        public void ChangeAllegiance(Allegiance newAllegiance)
        {
            /*  if (newAllegiance.Site != Site)
              {
                  ChangeSite(newAllegiance.Site);
              }*/

            Allegiance oldAllegiance = Intelligence.Allegiance;
            Intelligence.Allegiance.RemoveMember(this, false);
            Intelligence.Allegiance = newAllegiance;
            Intelligence.Allegiance.AddMember(this);

            Intelligence.Statistics.ChangeAllegiance(newAllegiance);

            ResetAfterEmigrating(oldAllegiance);
                
        }



        public enum GiveNewOwnerKnowledge { Yes, No }

        /// <summary>
        /// Call this to switch the owner of this entity. USe a null parameter to give up ownership.
        /// The method can be called safely - if the owner stays the same, nothing will happen
        /// 
        /// we can decide whether the item gets detected or not by the new owner.
        /// 
        /// we can change ownership of non-existing (remembered) entities, but only if the knowledge about their status is the same among the owners..
        /// 
        /// </summary>
        /// <param name="newOwner"></param>
        public void ChangeOwnership(IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge = GiveNewOwnerKnowledge.Yes) 
        {
            ChangeOwnership(this, newOwner, giveNewOwnerKnowledge);

          /*
            if (PartOf == null) //  we are at the root
            {
                // call this on the root, it will make recursive calls on the parts too:
                ChangeOwnershipOnParts(this, newOwner);

                if (giveNewOwnerKnowledge == GiveNewOwnerKnowledge.Yes)
                {
                    // make sure the new owner gets knowledge about his new item:
                    if (newOwner != null)
                    {
                        newOwner.Allegiance.SharedKnowledge.AddKnowledgeOfItemToNewOwner(this);

                    }
                }
            }
            else // we are a part - get the root:
            {
                Entity root = GetRootAsEntity();
                root.ChangeOwnership(newOwner, giveNewOwnerKnowledge); // call this method again to get to the root
            }*/

        }

        public static void ChangeOwnership(IKnownEntityData entityData, IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge = GiveNewOwnerKnowledge.Yes)
        {
            if (entityData.PartOfID == null) // TEST THIS - was: .PartOf == null) //  we are at the root
            {
                IOwner oldOwner;
                LookUpOwners.ResolveEntityOwner(entityData, out oldOwner);


                // call this on the root, it will make recursive calls on the parts too:
                ChangeOwnershipOnParts(entityData, newOwner);

                if (giveNewOwnerKnowledge == GiveNewOwnerKnowledge.Yes)
                {
                    // make sure the new owner gets knowledge about his new item:
                    if (newOwner != null)
                    {
                        newOwner.Allegiance.SharedKnowledge.AddKnowledgeOfItemToNewOwner((Entity)entityData);
                    }
                }

                // NEW: upgrades are included:
                if (entityData.ContainedUpgrades != null)
                {                   
                    foreach (var item in entityData.ContainedUpgrades)
                    {
                        IKnownEntityData upgrade = GetOwnedEntityData(item.Value, oldOwner, newOwner);
                        ChangeOwnership(upgrade, newOwner, giveNewOwnerKnowledge);
                    }
                }


                if (entityData.Households != null)
                {
                    if (newOwner == null)
                    {                       
                        for (int i = entityData.Households.Count - 1; i >= 0; i--)
                        {
                            Household household = LookUp<Household, HouseholdID>.FindByID(entityData.Households[i]);
                            if (household != null)
                            {
                                if (household.Home == entityData.EntityID)
                                {
                                    Residence.RemoveHousehold(oldOwner.Allegiance.SharedKnowledge, household);

                                    //household.Home = null;
                                }
                            }
                        }

                        entityData.Households.Clear();
                    }
                }  

            }
            else // we are a part - get the root:
            {
                //Entity root = GetRootAsEntity();
                IOwner oldOwner;
                LookUpOwners.ResolveEntityOwner(entityData, out oldOwner);

                IKnownEntityData root = GetOwnedEntityData(entityData.RootEntityID, oldOwner, newOwner);
                ChangeOwnership(root, newOwner, giveNewOwnerKnowledge); // call this method again to get to the root
            }
        }

        /// <summary>
        /// don't call this directly, call the ChangeOwnership method instead.
        /// 
        /// ChangeOwnership should call this method on its root
        /// to set a new owner, the entity must be seen. so it can be cast to Entity
        /// 
        /// NEW: Abandoning should be possible on MemoryFacts too! Even parts!
        /// 
        /// NEW: changes ownership on upgrades too (contained entities)
        /// </summary>
        /// <param name="newOwner"></param>
        public static void ChangeOwnershipOnParts(IKnownEntityData entityData, IOwner newOwner)
        {
            if (entityData == null)
            {
                return;
            }

            IOwner oldOwner;
            LookUpOwners.ResolveEntityOwner(entityData, out oldOwner);

            if (oldOwner != null) // the entity has previously been owned.
            {
                if (newOwner != oldOwner) // the new owner is different from the old owner - we are changing ownership for this entity.
                {
                    // delete from old collection:   
                    oldOwner.OwnedEntities.DeleteEntity(entityData.EntityID, entityData.EntityType);
                    entityData.OwnedBy = null;

                    // to set a new owner, the entity must be seen. so it can be cast to Entity here.
                    if (newOwner != null)
                    {
                        Entity entity = entityData as Entity;
                        newOwner.OwnedEntities.AddEntity(entity);
                        entity.OwnedBy = newOwner.ID;
                    }
                }
            }
            else
            {   // the entity has no previous owner.
                // add to new collection:
                if (newOwner != null)
                {
                    Entity entity = entityData as Entity;

                    newOwner.OwnedEntities.AddEntity(entity);
                    entity.OwnedBy = newOwner.ID;
                }
            }
           
            /* // we don't want this feature anyway... makes the game too fiddly.
            BodyComponent body;
            if (Find(out body))
            {
                foreach (BodyPart bodyPart in body.Body.BodyParts)
                {
                    bodyPart.ChangeOwnershipOnParts(newOwner);
                }
            }*/

            if (entityData.PartIDs != null)
            {
                foreach (EntityID partID in entityData.PartIDs)
                {
                    if (oldOwner != null || newOwner != null)
                    {
                        IKnownEntityData part = GetOwnedEntityData(partID, oldOwner, newOwner);

                        ChangeOwnershipOnParts(part, newOwner);
                    }
                }
            }

        } 


        private static IKnownEntityData GetOwnedEntityData(EntityID entityID, IOwner oldOwner, IOwner newOwner)
        {
            SharedKnowledge sharedKnowledge;
            if (oldOwner != null)
            {
                sharedKnowledge = oldOwner.Allegiance.SharedKnowledge;
            }
            else
            {
                sharedKnowledge = newOwner.Allegiance.SharedKnowledge;
            }

            IKnownEntityData entityData;
            sharedKnowledge.GetKnownData(entityID, out entityData);
            return entityData;
        }

      /*  public void ChangeOwnershipOnParts(IOwner newOwner)
        {
            IOwner oldOwner;
            LookUpOwners.ResolveEntityOwner(this, out oldOwner);

            if (oldOwner != null) // the entity has previously been owned.
            {
                if (newOwner != oldOwner) // the new owner is different from the old owner - we are changing ownership for this entity.
                {
                    // delete from old collection:   
                    oldOwner.OwnedEntities.DeleteEntity(this);
                    OwnedBy = null;

                    // to set a new owner, the entity must be seen. so it can be cast to Entity here.
                    if (newOwner != null)
                    {
                        newOwner.OwnedEntities.AddEntity(this);
                        OwnedBy = newOwner.ID;
                    }
                }
            }
            else
            {   // the entity has no previous owner.
                // add to new collection:
                if (newOwner != null)
                {
                    newOwner.OwnedEntities.AddEntity(this);
                    OwnedBy = newOwner.ID;
                }
            }
                      

            if (Parts != null)
            {
                foreach (Entity part in Parts)
                {
                    part.ChangeOwnershipOnParts(newOwner);
                }
            }

            if (ContainedUpgrades != null)
            {
                foreach (var item in ContainedUpgrades)
                {
                  
                }
            }

        }*/

        public void ClearSpriteStateFlag(StateModifier state)
        {
            Renderable.ClearSpriteStateFlag(state);

            // sim class must select a new state immediately:
            if (Renderable.ClearSpriteStateFlag(simStateFlags, state))
            {
                ReplaceSimState();
            }
        }

        public void SetSpriteStateFlag(StateModifier state)
        {
            // renderable will pull a new state when needed by the Client
            Renderable.SetSpriteStateFlag(state);
            
            // sim class must select a new state immediately:
            if (Renderable.SetSpriteStateFlag(simStateFlags, state))
            {
                ReplaceSimState();
            }
            

            /*SetSpriteStateFlag(spriteConditions, state);
            spriteFlagsAreDirty = true;*/
        }
        
     

        private void ReplaceSimState()
        {
            //do the magical best match trick, on the current bits
            //this retrieves a reference to the read-only data defined in EntityType

            IStateInfo info = null;
            RenderableType.FindBestStaticInfo(simStateFlags, EntityType.SimStateConditions, EntityType.DefaultSimState, out info);


            if (info != null)
            {
                AdoptSimStateInfo((SimStateInfo)info);
            }
        }

        private void AdoptSimStateInfo(SimStateInfo newInfo)
        {
            if (newInfo == null)
            {
                newInfo = EntityType.DefaultSimState; // default is always good
            }

            SimStateInfo oldInfo = CurrentSimState;
            CurrentSimState = newInfo;

            if (oldInfo != CurrentSimState)
            {
                AdoptGeoLayout(CurrentSimState, oldInfo);
            }

        }

        /// <summary>
        /// shifting this would need to create new collidable, stamp shapes
        /// switch the GeoLayoutType reference? Recreate GeoLayout instance?
        /// 
        /// Then invoke the new Collidable to re-stamp with Entity.UpdateShapeLocation?
        /// </summary>
        /// <param name="newInfo"></param>
        private void AdoptGeoLayout(SimStateInfo newInfo, SimStateInfo oldInfo)
        {
            if (newInfo.GeometryLayoutType != null)
            {
                if (Collidable != null)
                {
                    Collidable.Delete();
                    Collidable = null;
                }
                
                CreateCollidable();

                if (GeometryLayout == null)
                {
                    GeometryLayout = new GeometryLayout(this);         
                }

                FootprintIsDirty = true; // will restamp in Update...

                SetAccessPointDirty();

                /*
                if (RenderAsGroundSprite == null)
                {
                    RenderAsGroundSprite = new RenderAsGroundSprite(Entity, newInfo.RenderAsGroundSpriteType, this);
                }

                if (RenderAsGroundSprite != null)
                {
                    Rectangle? rectangle;
                    string spriteName;

                    spriteName = newInfo.RenderAsGroundSpriteType.AssetName;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        rectangle = The.Client.FlatSpriteSheet.GetSourceRectangle(spriteName);
                    }
                    else
                    {
                        // clear the ground sprite:
                        rectangle = null;
                    }

                    RenderAsGroundSprite.Redraw(rectangle);
                }*/
            }
            else
            {
                // remove existing geo layout
                if (this.GeometryLayout != null)
                {
                    //GeometryLayout.Place(GeoPlaceMode.FeatureRemoved);
                    GeometryLayout.Destroy();
                    GeometryLayout = null;
                }

                if (Collidable != null)
                {
                    Collidable.Delete();
                    Collidable = null;
                }

                if (oldInfo == null || oldInfo.GeometryLayoutType != null)
                {
                    SetAccessPointDirty();
                }
            }


            /*
            if (newInfo.RenderAsGroundSpriteType != null)
            {
                if (RenderAsGroundSprite == null)
                {
                    RenderAsGroundSprite = new RenderAsGroundSprite(Entity, newInfo.RenderAsGroundSpriteType, this);
                }

                if (RenderAsGroundSprite != null)
                {
                    Rectangle? rectangle;
                    string spriteName;

                    spriteName = newInfo.RenderAsGroundSpriteType.AssetName;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        rectangle = The.Client.FlatSpriteSheet.GetSourceRectangle(spriteName);
                    }
                    else
                    {
                        // clear the ground sprite:
                        rectangle = null;
                    }

                    RenderAsGroundSprite.Redraw(rectangle);
                }
            }
            else
            {
                // remove existing ground sprite:
                if (RenderAsGroundSprite != null)
                {
                    RenderAsGroundSprite = null;
                    //RenderAsGroundSprite.Redraw(null);
                }
            }*/
        }

        public static bool IsOnPlaySite(IKnownEntityData entityData)
        {
            return entityData.Site != null && entityData.Site == The.Sim.PlaySite.ID; // .IsPlaySite;
        }

        public bool IsOnPlaySite()
        {
            return IsOnPlaySite(this);
            //return Site != null && Site.IsPlaySite;
        }

        /// <summary>
        /// returns true if we are in the hauling state, but not holding any item in the hand...
        /// </summary>
        /// <returns></returns>
        public bool IsHaulingNothingMounted()
        {
            /* if (PersonEntity != null && Name.Contains("Augustine Yeboah"))
             {

             }*/

            bool isHauling = false;
            if (AgentStorage != null)
            {
                isHauling = AgentStorage.MountedToolOrWeapon == null && AgentStorage.IsHauling;
            }

            return isHauling;
        }




        // for the evaluators
        public bool IsAttacking()
        {
            return Intelligence.CombatInfo.Target != null;
        }

        /// <summary>
        /// TODO: make this work for non humans also
        /// </summary>
        /// <returns></returns>
        public bool IsFleeing()
        {
            // hmm.. only humans run...
            return Locomotor != null
                && Locomotor.LeggedLocomotor != null
                && Locomotor.LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run;
        }

        public bool IsAwakeAndActive()
        {
            return Intelligence.IsAwakeAndActive;
        }

        /// <summary>
        /// update stuff that does not need to be done every frame, and doesn't benefit from common calculations
        /// OLD: ! Run in parallel! Watch out for operations on collections other than read... such as foreach! 
        /// </summary>
        /// <param name="time"></param>
        /*    public void UpdateSimulation(double deltaTimeInSeconds)
            {
                if (EntityType.RockType != null)
                {
                    return;
                }

                BiologicalEntity bioEntity;
                if (Find(out bioEntity))
                {
                    bioEntity.UpdateSimulation(deltaTimeInSeconds);
                }

                if (!IsDead && IsCompleted())
                {
                    Intelligence intelligence;
                    if (Find(out intelligence))
                    {
                        intelligence.UpdateSimulation(deltaTimeInSeconds);
                    }               
                }


                if (EntityType.ContainerType != null
                    && EntityType.ContainerType.GetRequiresReplenishType() != null)
                {
                    ((IHasReplenishItems)Contains).ReplenishItems.UpdateSimulation(deltaTimeInSeconds);

                }
            }*/


        public bool CanSeeEntity(Entity entity)
        {
            Entity seenEntity;
            return Intelligence.GetEntitySeenDirectly(entity.EntityID, out seenEntity);
        }

        public void SetSneaking(bool value)
        {
            if (value == true)
            {
                Locomotor.LeggedLocomotor.TargetSpeed = UWGame.SimSide.AI.Goals.Goal.MovementSpeeds.WalkSlowly;
                Intelligence.IsStealthy = true;
            }
            else
            {
                Locomotor.LeggedLocomotor.TargetSpeed = UWGame.SimSide.AI.Goals.Goal.MovementSpeeds.Normal;
                Intelligence.IsStealthy = false;
            }

        }


        /// <summary>
        /// the purpose is to check all life systems to determine if we should be dead or collapsed
        /// </summary>
        /// <param name="isDead"></param>
        /// <param name="isUnconscious"></param>
        public void GetStatus(out bool isDead, out bool isUnconscious,
            out CauseOfDeath? causeOfDeath, out CauseOfUnconsciousness? causeOfUnconsciousness)
        {
            bool bodyIsDead = false;
            bool bodyIsUnconscious = false;

            causeOfDeath = null;
            causeOfUnconsciousness = null;

            BodyComponent body;
            if (Find(out body))
            {
                body.Body.GetStatus(out bodyIsDead, out bodyIsUnconscious, ref causeOfDeath, ref causeOfUnconsciousness);
            }

            bool bioIsDead = false;
            bool bioIsUnconscious = false;


            BiologicalEntity bioEntity;
            if (Find(out bioEntity))
            {
                bioEntity.GetStatus(out bioIsDead, out bioIsUnconscious, ref causeOfDeath, ref causeOfUnconsciousness);

            }

            isDead = bodyIsDead || bioIsDead;
            isUnconscious = bodyIsUnconscious || bioIsUnconscious;
        }

        /// <summary>
        /// we need to update needs of active entities more often since they sample activities/goals
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        /*  public void UpdateFrequentSimulationInParallel(double deltaTimeInSeconds) //double deltaTimeInMilliseconds)
          {
              if (!IsDead)
              {
                  if (EntityType.BiologicalType != null)
                  {
                      BiologicalEntity bioEntity;
                      if (Find(out bioEntity))
                      {

                          bool isDeadNow, isUnconsciousNow;
                          CauseOfDeath? causeOfDeath;
                          CauseOfUnconsciousness? causeOfUnconsciousness;

                          GetStatus(out isDeadNow, out isUnconsciousNow, out causeOfDeath, out causeOfUnconsciousness);
                        

                          bioEntity.UpdateFrequentSimulationInParallel(deltaTimeInSeconds);

                          // see if we have died/gone unconscious...
                          bool isDead, isUnconscious;

                          GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

                          bool collapseAndDie = false;

                          if (isDead || isUnconscious)
                          {


                              //makes sure that we don't die twice...
                              if (!isDeadNow && !isUnconsciousNow) // old. we were not dead before the update, but now we are. Start the death anims.
                              {
                                  collapseAndDie = true;
                              }
                              else
                              {
                                  // this situation can occur if the hitpoints went below critical before this call... 
                                  if (Intelligence.Brain.Subgoals.Count == 0)
                                  {
                                      collapseAndDie = true;
                                  }
                                  else
                                  {
                                      Goal goal = Intelligence.Brain.Subgoals.Peek();
                                      //makes sure that we don't die twice...
                                      if (!(goal is GoalCollapse)
                                          && !(goal is GoalIsDying))
                                      {
                                          collapseAndDie = true;
                                      }
                                  }
                              }
                          }



                          if (collapseAndDie) //(!isDeadNow && !isUnconsciousNow) && (isDead || isUnconscious))
                          {
                              Intelligence.Brain.RemoveAllSubgoals();

                              // adding these goals makes death occur outside this loop.

                              Intelligence.Brain.AddSubgoal(new GoalCollapse(this, OwnedBy));
                              if (causeOfDeath == CauseOfDeath.Wounds || causeOfUnconsciousness == CauseOfUnconsciousness.Wounds)
                              {

                                  Intelligence.Brain.AddSubgoal(new GoalIsDying(this, OwnedBy, true)); // this may never execute if the Collapse goal determines that we're dead.
                              }
                              else
                              {

                                  Intelligence.Brain.AddSubgoal(new GoalIsDying(this, OwnedBy, false)); // this may never execute if the Collapse goal determines that we're dead.
                              }
                          }
                      }
                  }
              }
          }*/


        public void EnableCollisions()
        {
            if (The.Sim.Mode == Sim.EngineMode.Game
                && Collidable != null
                && Collidable.Enabled == false
                && CausesCollisions())
            {
                The.CollisionManager.AddCollidable(Collidable);
            }
        }
        
        private bool CausesCollisions()
        {

            if (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null && CurrentSimState.GeometryLayoutType.CausesCollisions == true) //  GeometryLayoutType != null && GeometryLayoutType.CausesCollisions == true) 
            {
                // can be specified on the type level
                return true;
            }
            else return EntityType.TerrainType == null && EntityType.TreeType == null && EntityType.StructureType == null;  // the default case
        }


        public void DisableCollisions()
        {
            if (Collidable != null
                && Collidable.Enabled == true)
            {
                The.CollisionManager.RemoveCollidable(Collidable);
            }
        }

        private int collidingTimeout = 3;


        /// <summary>
        /// NEW: ISleepingUpdatable.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="wasDestroyed"></param>
        public void Update(GameTime time, out bool wasDestroyed)
        {
           
            bool isCompleted = IsCompleted();

            if (name != null && name.Contains("onlan"))
            {

            }

            if (!IsDead && isCompleted && EntityType.IntelligenceType != null)
            {
                Intelligence intelligenceComponent;
                if (Find(out intelligenceComponent))
                {
                    intelligenceComponent.Update(time); // we may die here...
                }
            }


            if (EntityID != Entities.EntityID.Invalid
                && !IsDead
                && IsOnPlaySite())
            {
                UpdatePlaySite(time, isCompleted);
            }

            if (EntityID == Entities.EntityID.Invalid)
            {
                wasDestroyed = true;
            }
            else
            {
                wasDestroyed = false;
            }

        }

        private void UpdatePlaySite(GameTime time, bool isCompleted)
        {
            if (EntityType.IntelligenceType != null)
            {
                if (The.AgentQuadTree != null)
                {
                    The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
                }              
            }

            if (Locomotor != null)
            {
                if (collidingTimeout-- <= 0)
                {
                    Locomotor.SetIsColliding(false);// isColliding = false;
                    collidingTimeout = 3;//hack to make the color flash in overlay

                }

#if DEBUG || PROFILE

                if (Locomotor.RotateSlowly == true
                    && (Intelligence == null || Intelligence.DisableAI == true))
                {
                    SetRotationAndDir((float)(Rotation + time.ElapsedGameTime.TotalSeconds));
                }
#endif

                if (!IsDead && ContainedBy == null && Locomotor != null && Collidable != null && Collidable.Enabled)
                {
                    Locomotor.ReactToCollisions();
                }
            }

            PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged);

            if (!IsDead)
            {
                if (isCompleted)
                {
                    // remember that these calls are coming from SleepyUpdater and may already be regulated

                    if (EntityType.LocomotorType != null)
                    {
                        Locomotor locomotor;
                        if (Find(out locomotor))
                        {
                            locomotor.UpdatePlaySite(time); // Update(time);
                        }
                    }

                    if (EntityType.SensorType != null)
                    {
                        Sensor sensor;
                        if (Find(out sensor))
                        {
                            sensor.UpdatePlaySite(time); // Update(time);
                        }
                    }

                    if (EntityType.TreeType != null)
                    {
                        Tree tree;
                        if (Find(out tree))
                        {
                            tree.UpdatePlaySite(time); // Update(time);
                        }
                    }

                    if (EntityType.Person != null)
                    {
                        Person person;
                        if (Find(out person))
                        {
                            person.UpdatePlaySite(time); //Update(time);
                        }
                    }

                    SimEffectsComponent effects = SimEffects;
                    if (effects != null)
                    {
                        effects.UpdatePlaySite(time); // Update(time);
                    }
                }

                // degrade incomplete items too:
                if (EntityType.NonLivingType != null)
                {
                    NonLivingEntity nonLiving;
                    if (Find(out nonLiving))
                    {
                        nonLiving.Update(time);
                    }
                }
            }
            

           
            // UpdateSimulation  // NEW - moved from UpdateSimulation
            if (EntityType.ContainerType != null
                && EntityType.ContainerType.GetRequiresReplenishType() != null)
            {
                IHasReplenishItems hasReplenishItems = Contains as IHasReplenishItems;
                if (hasReplenishItems != null && hasReplenishItems.ReplenishItems != null)
                {
                    hasReplenishItems.ReplenishItems.Update();
                }
            }


            if (!IsDead)
            {
                UpdateSystemsThatCanCauseDeath();
            }
        }

        Regulator bioSystemsRegulator;

        /// <summary>
        /// we need regulators to control the Update calls...
        /// </summary>
        /* private void CreateRegulators()
         {
             // regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.Constants.UpdateIntervalForEntityComponents, "BiologicalEntity");

             // won't this restart after save...?
             frequentRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.Constants.UpdateIntervalForBioEntity, "BiologicalEntity");
        
         }*/





        /// <summary>
        /// check bio state before and after to determine death/collapse change...
        /// 
        /// this is placed in Entity because of the interplay between 2 components.
        /// </summary>
        private void UpdateSystemsThatCanCauseDeath()
        {

            if (EntityType.BiologicalType != null)
            {
                double deltaTimeInSeconds;
                if (bioSystemsRegulator.IsReadyGetTimeElapsedInSeconds(out deltaTimeInSeconds))
                {

                    BiologicalEntity bioEntity;
                    if (Find(out bioEntity))
                    {

                        bool isDeadNow, isUnconsciousNow;
                        CauseOfDeath? causeOfDeath;
                        CauseOfUnconsciousness? causeOfUnconsciousness;

                        GetStatus(out isDeadNow, out isUnconsciousNow, out causeOfDeath, out causeOfUnconsciousness);


                        bioEntity.UpdateSimulation(deltaTimeInSeconds);
                        //bioEntity.UpdateFrequentSimulationInParallel(deltaTimeInSeconds);

                        Body.RegainHitpoints(deltaTimeInSeconds); // cannot cause death...


                        // see if we have died/gone unconscious...
                        bool isDead, isUnconscious;

                        GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

                        bool collapseAndDie = false;

                        if (isDead || isUnconscious)
                        {

                            //makes sure that we don't die twice...
                            if (!isDeadNow && !isUnconsciousNow) // old. we were not dead before the update, but now we are. Start the death anims.
                            {
                                collapseAndDie = true;
                            }
                            else
                            {
                                // this situation can occur if the hitpoints went below critical before this call... 
                                if (Intelligence.Brain.Subgoals.Count == 0)
                                {
                                    collapseAndDie = true;
                                }
                                else
                                {
                                    Goal goal = Intelligence.Brain.Subgoals.Peek();
                                    //makes sure that we don't die twice...
                                    if (!(goal is GoalCollapse)
                                        && !(goal is GoalIsDying))
                                    {
                                        collapseAndDie = true;
                                    }
                                }
                            }
                        }

                        if (collapseAndDie)
                        {
                            Intelligence.Brain.RemoveAllSubgoals();

                            // adding these goals makes death occur outside this loop.

                          
                            bool wasKilledByWounds = causeOfDeath == CauseOfDeath.Wounds || causeOfUnconsciousness == CauseOfUnconsciousness.Wounds;

                            EntityID? lastAttacker = null;
                            if (wasKilledByWounds)
                            {
                                // we need to get the 'killer' from memory... otherwise the palyer might get cheated from some kills...
                                lastAttacker = Intelligence.Memory.GetLastAttacker();
                            }

                            Intelligence.Brain.AddSubgoal(new GoalCollapse(this, OwnedBy, lastAttacker));


                            Intelligence.Brain.AddSubgoal(new GoalIsDying(this, OwnedBy, wasKilledByWounds)); // this may never execute if the Collapse goal determines that we're dead.
                           
                        }
                    }
                }
            }
        }


        /*  public void Update(GameTime time)
          {
              bool isCompleted = IsCompleted();
              if (!IsDead && isCompleted && EntityType.IntelligenceType != null)
              {
                  Intelligence intelligenceComponent;
                  if (Find(out intelligenceComponent))
                  {
                      intelligenceComponent.Update(time); // we may die here...
                  }
              }


              if (EntityType.TreeType == null && EntityType.RockType == null) // optimization...
              {
                  if (!IsOnPlaySite()) // the profiler says that this call adds up to a significant amount for 1000s of entities...
                      return;
              }



              if (
                  EntityType.IntelligenceType != null)
              {
                  if (The.AgentQuadTree != null)
                  {
                      The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
                  }

                  if (Intelligence.Allegiance != null && IsOnPlaySite())
                  {                    
                      Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(EntityID, PlaySiteLocation.ToVector2());
                  }
              }

           

              if (collidingTimeout-- <= 0)
              {
                  if (Locomotor != null)
                  {
                      Locomotor.SetIsColliding(false);// isColliding = false;
                      collidingTimeout = 3;//hack to make the color flash in overlay
                  }
              }



              if (Locomotor != null)
              {

  #if DEBUG || PROFILE

                  if (Locomotor.RotateSlowly == true
                      && (Intelligence == null || Intelligence.DisableAI == true))
                  {
                      SetRotationAndDir((float)(Rotation + time.ElapsedGameTime.TotalSeconds));
                  }
  #endif

                  if (!IsDead && ContainedBy == null && Locomotor != null && Collidable != null && Collidable.Enabled)
                  {
                      Locomotor.ReactToCollisions();
                  }
              }


              PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged);


              if (EntityType.RockType != null && !EntityType.RockType.Animates)// looks like client stuff.
              {
                  return;
              }



              if (!IsDead && isCompleted)
              {
                  Locomotor locomotor;
                  if (Find(out locomotor))
                  {
                      Locomotor.State state = locomotor.Update(time);
                  }


                  Sensor sensor;
                  if (Find(out sensor))
                  {
                      sensor.Update(time);
                  }
              }


              //  UpdateStaticConditionState(); 

              UpdateTriggerPosition();
            

          }*/

        public void SetNotDirty()
        {
            footprintIsDirty = false;
        }

        public void PlaceGeometryLayoutIfDirty(GeoPlaceMode mode)
        {
            if (footprintIsDirty && GeometryLayout != null
                && (Structure == null || Structure.ConstructionHasStarted()))
            {
                // #OPTIMIZE
                GeometryLayout.Place(mode); //GeoPlaceMode.ShapeChanged);
            }

            footprintIsDirty = false;//set to false every time, regardless of ground feature
        }


        /*  public void UpdateTriggerPosition()
          {*/
        /*  for (int index = 0; index < attachedTriggers.Count; index++)
          {
              attachedTriggers[index].Location = location;
          }*/
        // }



        public Vector3 ModifyNewLocationToStayOnFreeTerrain(Vector3 newLocation, Vector2 pushVector)
        {
            // test the new position:
            SubtilePos nextSubtile = MapManager.WorldPosToSubtilePos(newLocation);
            SubtilePos currentSubtile = MapManager.WorldPosToSubtilePos(PlaySiteLocation);
            if (nextSubtile != currentSubtile)
            {
                SubtileLayers terrain = The.Map.TerrainCosts[GetTransportType()];

                // we have crossed into a new subtile -               
                // test if the next subtile is passable!                
                if (MapManager.IsBlocked(terrain.GetValue(nextSubtile)))
                {
                    if (MapManager.IsBlocked(terrain.GetValue(currentSubtile)))
                    {
                        // stay where we are - we cannot move.
                        return PlaySiteLocation;
                    }

                    // Slide along the obstacle by modifying the push(move) vector:
                    Common.Direction dir = Common.GetDirection(currentSubtile.ToPoint(), nextSubtile.ToPoint());

                    switch (dir)
                    {
                        case Common.Direction.North:
                        case Common.Direction.South:
                            if (pushVector.X == 0f)
                            { // never run straight into an obstacle:
                                return PlaySiteLocation;
                            }
                            else
                            {
                                pushVector.Y = 0f;

                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                        case Common.Direction.West:
                        case Common.Direction.East:
                            if (pushVector.Y == 0f)
                            {
                                return PlaySiteLocation;
                            }
                            else
                            {
                                pushVector.X = 0f;

                                return PlaySiteLocation + pushVector.ToVector3();

                            }
                        case Common.Direction.NorthEast:
                            // slide either North or East if possible:
                            if (!IsDirectionBlocked(terrain, Common.Direction.North, currentSubtile))
                            {
                                pushVector.X = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else if (!IsDirectionBlocked(terrain, Common.Direction.East, currentSubtile))
                            {
                                pushVector.Y = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else return PlaySiteLocation;
                        case Common.Direction.NorthWest:
                            // slide if possible:
                            if (!IsDirectionBlocked(terrain, Common.Direction.North, currentSubtile))
                            {
                                pushVector.X = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else if (!IsDirectionBlocked(terrain, Common.Direction.West, currentSubtile))
                            {
                                pushVector.Y = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else return PlaySiteLocation;
                        case Common.Direction.SouthEast:
                            // slide if possible:
                            if (!IsDirectionBlocked(terrain, Common.Direction.South, currentSubtile))
                            {
                                pushVector.X = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else if (!IsDirectionBlocked(terrain, Common.Direction.East, currentSubtile))
                            {
                                pushVector.Y = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else return PlaySiteLocation;
                        case Common.Direction.SouthWest:
                            // slide if possible:
                            if (!IsDirectionBlocked(terrain, Common.Direction.South, currentSubtile))
                            {
                                pushVector.X = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else if (!IsDirectionBlocked(terrain, Common.Direction.West, currentSubtile))
                            {
                                pushVector.Y = 0f;
                                return PlaySiteLocation + pushVector.ToVector3();
                            }
                            else return PlaySiteLocation;
                        default:
                            return PlaySiteLocation;

                    }
                }
            }

            return newLocation;
        }


        /// <summary>
        /// does not check map bounds!!!
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="dir"></param>
        /// <param name="currentSubtile"></param>
        /// <returns></returns>
        private bool IsDirectionBlocked(SubtileLayers terrain, Common.Direction dir, SubtilePos currentSubtile)
        {
            switch (dir)
            {
                case Common.Direction.East:
                    currentSubtile.X += 1;
                    break;
                case Common.Direction.North:
                    currentSubtile.Y -= 1;
                    break;
                case Common.Direction.South:
                    currentSubtile.Y += 1;
                    break;
                case Common.Direction.West:
                    currentSubtile.X -= 1;
                    break;
                default: return true;
            }

            return MapManager.IsBlocked(terrain.GetValue(currentSubtile));

        }

        /* public void IsNowCompleted()
         {
             // attach triggers:
             AttachTriggers(EntityType.Triggers);
         }*/

        public void AttachTriggers(TriggerType[] triggers)
        {
            if (triggers != null)
            {
                foreach (var triggerType in triggers)
                {
                    Trigger trigger = new Trigger(this, null, triggerType);
                    AttachTrigger(trigger);
                }
            }
        }

        public void Destroy(bool destroyParts = true, bool parentIsDestroyed = false)
        {
            if (PersonEntity != null)
            {
                // find out why ResourceMapForAgent is not removed when starving

            }

            // fire events:

            List<ActionSets> defaultActions;

            EntityType.EventActions.TryGetValue(EntityEventHooks.ToBeDestroyed, out defaultActions);
            Goal.FireEventActions(this, null, defaultActions);


            Site site = Site; // cache this before removing...
            Point? mapPos = MapPosition;

            AssignedToJob = null;

            inUseBy = null;

            if (DirectionalLayout != null)
            {
                DirectionalLayout.Destroy();
            }

            if (GeometryLayout != null)
            {
                GeometryLayout.Destroy();
            }

            if (Structure != null)
            {
                Structure.Destroy();
            }

            Tree tree;
            if (Find(out tree))
            {
                tree.Destroy();
            }

            // order matters here, sensors should see themselves being destroyed
            RemoveOwnedItemIfOwnerSeesItDestroyed(mapPos);

            Sensor sensor;
            if (Find(out sensor))
            {
                // remove old detected tiles:

                // (if we are the last allegiance member, don't store memory facts)
                sensor.UnseeTilesInRange();
            }

            RemoveFromMap();

            Item item;
            if (Find(out item))
            {
                item.Destroy();
            }



            if (PartOf != null)
            {
                if (!parentIsDestroyed)
                {
                    // don't signal the parent that one of its parts broke if the parent is already destroyed.
                    PartOf.SetBrokenPart();
                }

                // also very important:
                PartOf.RemovePart(this, false); // the part needs info from its parent such as map position, so don't reset the PartOf reference

            }

            if (Parts != null) // !!! destroy the parts as well.
            {
                for (int i = Parts.Count - 1; i >= 0; i--)
                {
                    Entity part = Parts[i];

                    if (destroyParts)
                    {
                        part.Destroy(parentIsDestroyed: true);
                    }
                    else
                    {
                        // Salvaging. Parts are coming out - set their location since they no longer have a parent
                        part.PartOf = null;

                        // we will "Eject" parts later if needed in order to set their location...

                        // don't leave the parts without a location. The may need it to eject contained entities...
                      /*  if (part.Location == null)
                        {
                            part.location = this.Location;
                        }*/

                        // site is always set on parts as well as root
                        // part.site = this.Site; 
                    }
                }
            }


            if (Contains != null && IsCompleted())
            {
                Contains.Destroy();
            }

            if (PartOf == null)
            {
                Container container;
                GetContainedBy(out container);
                if (container != null)
                {
                    container.Remove(this);
                }
            }


            if (mapPos.HasValue)
            {
                The.Map.GetTile(mapPos.Value).DeleteMemoryOfDestroyedEntity(this);
            }

            // depends on Intelligence:
            if (EntityType.Person != null)
            {
                PersonEntity.Destroy();
            }

            if (Intelligence != null)
            {
                Intelligence.Destroy();
            }

            /*  RequiresEnergy energy;
              if (Find(out energy))
              {
                  if (energy.RequiresFuel != null)
                  {
                      energy.RequiresFuel.Extinguish();
                  }
              }*/



            if (Renderable != null)
                Renderable.Destroy();

            if (Collidable != null)
            {
                Collidable.Delete();
                Collidable = null;
            }

            if (SelectionShape != null)
            {
                SelectionShape.Delete();
                SelectionShape = null;
            }

            /*
            if (EntityType.Person != null)
            {
                PersonEntity.Destroy();
            }
            */

            Vehicle vehicle;
            if (Find(out vehicle))
            {
                vehicle.Destroy();
            }


            if (EntityType.CommunicatorType != null)
            {
                Communicator communicator;
                if (Find(out communicator))
                {
                    communicator.Destroy();
                }
            }

            //Site removes us from all its auxiliary collections:
            site.RemoveEntity(this);


            //if the entity is destroyed, remove accessibility data from Client:
            The.Client.HandleDestroyedEntity(EntityID);

            #region Remove ID entries:

            RemoveIDEntry();
            ((ILookUp<IComposite, CompositeID>)this).RemoveIDEntry();
            ((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();

            if (EntityType.IntelligenceType != null)
            {
                ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
            }

            #endregion

            DeleteAttachedTriggers();
            RemoveFromAgentQuadTree();


            // fire events:
            EntityType.EventActions.TryGetValue(EntityEventHooks.Destroyed, out defaultActions);
            Goal.FireEventActions(this, null, defaultActions);

            if (EntityType.IntelligenceType != null && EntityType.BiologicalType != null)
            {
                if (site != null && site.IsPlaySite)  //IsOnPlaySite()) // can't use .Site..?
                {
                    EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DiedOnPlaySite, out defaultActions);
                    Goal.FireEventActions(this, null, defaultActions);
                }
            }

            if (EntityType.PolledEvents != null)
            {
                List<PolledEventType> list;
                if (EntityType.PolledEvents.TryGetValue(Scope.Entity, out list))
                {
                    foreach (var eventType in list)
                    {
                        Site.EventManager.RemovePolledEvent(eventType, this.EntityID);
                    }
                }

            }


            PartOf = null; // cannot use Site or Location after this line

            The.Sim.RemoveEntity(this);
            

        }

        /// <summary>
        /// only logs owned items
        /// </summary>
        /// <param name="item"></param>
        public static void LogProductionEvent(IKnownEntityData item, ProductionStatistics.StatTypes statType, bool testIfSeen)
        {
            IOwner owner;
            LookUpOwners.ResolveEntityOwner(item, out owner);

            if (owner != null)
            {
                IKnownEntityData entityData;
                if (testIfSeen == false || owner.Allegiance.SharedKnowledge.GetKnownData(item.EntityID, out entityData) == EntityResult.SeenDirectly)
                {
                    owner.Allegiance.Statistics.AddProductionEvent(item.EntityType, statType /*Allegiances.Statistics.ProductionStatistics.StatTypes.UsedAsInput*/, 1);
                }
            }
        }


        private void RemoveOwnedItemIfOwnerSeesItDestroyed(Point? mapPos)
        {
            // remove from ownership now instead waiting for an AI evaluator or similar.
            // entities that have sensors should always see themselves being destroyed
            Allegiance owningAllegiance = null;
            EntityGroup owner = null;
            if (PersonEntity == null && OwnedBy.HasValue)
            {
                IOwner iowner = LookUpOwners.FindByID(OwnedBy);
                if (iowner != null)
                {
                    owningAllegiance = iowner.Allegiance;
                    owner = iowner.OwnedEntities;
                }
            }


            if (owningAllegiance != null && owner != null
                && mapPos.HasValue
                && The.Map.GetTile(mapPos.Value).AllegiancesThatSeeThisTile.Contains(owningAllegiance))
            {

                owner.DeleteEntity(this);

                /* don't un-own the parts. if this is a salvage job, we will keep those.
                if (Parts != null) // un-own the parts as well.
                {
                    for (int i = Parts.Count - 1; i >= 0; i--)
                    {
                        Entity part = Parts[i];
                        owner.DeleteEntity(part);
                    }
                }*/
            }

        }

        public void RemoveFromMap()
        {
            Point? mapPos = MapPosition;

            if (mapPos.HasValue
                && The.Map.TileIsOnMap(mapPos.Value.X, mapPos.Value.Y))
            {
                TerrainTile tile = The.Map.GetTile(mapPos.Value);
                if (tile.ContainsEntity(this))
                {
                    tile.RemoveEntity(this);
                }
            }
        }

        /*
        public ThreatJobManager.ThreatEvaluationStatus AllegianceIsAggravatedByMe(Allegiance.Allegiance allegianceToCheckAggrevationAgainst, ref bool isInAggrevationRange)
        {
            if(EntityType.IntelligenceType.AggroRange == null)
            {
                isInAggrevationRange = true;
                return ThreatJobManager.ThreatEvaluationStatus.Done;
            }
            else
            {
                return allegianceToCheckAggrevationAgainst.IsInAggrevationRange(EntityID, ref isInAggrevationRange, EntityType.IntelligenceType.AggroRange);
            }
        }
        */

        /*   public ThreatJobManager.ThreatEvaluationStatus IsAggrevatedByEntity(IKnownEntityData anEntityToCheckAggrevationAgainst, float aggroRangeToCheck, ref bool isInAggrevationRange)
           {
               RegionMap footRegionMap = Intelligence.Allegiance.SharedKnowledge.GetMovementMap(
                   ProtectionLevel.Exposed, 
                   Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory, 
                   ThreatStance.Bold)
                   .RegionMap[SurfaceType.TransportType.Foot];
            
               float distanceToEntity = 0.0f;
               IKnownEntityData anEntityToCheckAggrevationFrom = this;
               //distanceToEntity = (Location - Intelligence.Allegiance.SharedKnowledge.GetLocation(anEntityToCheckAggrevationAgainst)).Length();
               RegionMap.NotifyWhenFinished callback = this.Intelligence.Allegiance.ThreatAndCombatJobManager.SetToNotWaiting; // WTF??
               RegionMap.Result result = footRegionMap.GetDistanceToEntity(this, anEntityToCheckAggrevationFrom, anEntityToCheckAggrevationAgainst, ref distanceToEntity, null, null, false, callback);
               if (result == RegionMap.Result.Wait)
               {
                   return ThreatJobManager.ThreatEvaluationStatus.Processing;
               }
               //float flightDistanceToEntity = (anEntityToCheckAggrevationAgainst.Location - location).Length();//TODO: Use the region map to get the real distance as well as the quad tree
               if (distanceToEntity > aggroRangeToCheck)
               {
                   isInAggrevationRange = false;
                   return ThreatJobManager.ThreatEvaluationStatus.Done;
               }
               else
               {
                   isInAggrevationRange = true;
                   return ThreatJobManager.ThreatEvaluationStatus.Done;
               }
           }*/


        void DeleteAttachedTriggers()
        {
            while (attachedTriggers.Count != 0)
            {
                DeleteTrigger(attachedTriggers[attachedTriggers.Count - 1]);
            }
        }

        public Entity FindPartOfType(EntityType type)
        {
            if (Parts != null)
            {
                foreach (var item in Parts)
                {
                    if (item.EntityType == type)
                    {
                        return item;
                    }
                    else
                    {
                        Entity result = item.FindPartOfType(type);
                        if (result != null)
                        {
                            return result; // found!
                        }
                        // continue looking...
                    }
                }

            }

            return null;
        }


        //   public bool Stealth;

        public bool IsCompleted()
        {
            NonLivingEntity nonLivingEntity;
            if (Find(out nonLivingEntity))
            {
                return nonLivingEntity.IsCompleted();
            }
            else return true;
        }

       /* public bool IsEnclosed()
        {
            Item itemComponent = Item;
            if (itemComponent != null)
            {
                return itemComponent.IsEnclosed();
            }

            return false;
        }*/

        /// <summary>
        /// items and structures are always spotted at once
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool RequiresRollToDetect()
        {
            if (EntityType.RequiresRollToDetect.HasValue)
            {
                return EntityType.RequiresRollToDetect.Value;
            }
            else
            {
                return EntityType.ItemType == null && EntityType.StructureType == null && EntityType.TreeType == null; // && !EntityType.KeyName.Contains("shore"); //
            }
        }

        /*
        public void SeeByAllegiance(Allegiances.Allegiance allegiance)
        {
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;
            
            // store food items for quick iteration:
            if (allegiance.RepresentativeEntityType.Person != null // people don't use this list, they use ownership lists instead
                && EntityType.ItemType != null
                && EntityType.ItemType.Food != null
                && EntityType.ItemType.Food.IsEatable(allegiance.RepresentativeEntityType)
               )
            {
                if (!sharedKnowledge.AllKnownFoodItems.ContainsKey(this.ID))
                {
                    sharedKnowledge.AllKnownFoodItems.Add(this.ID, this.ID);
                }
            }
            else if (sharedKnowledge.IsOutsiderAgent(this))
            {
                if (!sharedKnowledge.AllKnownOutsideAgents.ContainsKey(this.ID))
                {
                    sharedKnowledge.AllKnownOutsideAgents.Add(this.ID, this.ID);
                }
            }
            else if (sharedKnowledge.IsOutsideThreat(this))
            {
                if (!sharedKnowledge.AllKnownThreatSources.ContainsKey(this.ID))
                {
                    sharedKnowledge.AllKnownThreatSources.Add(this.ID, this.ID);
                }
            }

            if (!sharedKnowledge.AllKnownEntities.ContainsKey(this.ID))
            {
                sharedKnowledge.AllKnownEntities.Add(this.ID, this.ID);
            }

            if (sharedKnowledge.KnownEntityDataTree.Contains(this.EntityID) == false)
            {
                sharedKnowledge.KnownEntityDataTree.AddObject(this.EntityID, location.ToVector2());
            }
            else
            {
                sharedKnowledge.KnownEntityDataTree.UpdateObject(this.EntityID, location.ToVector2());
            }

            // now see inside containers:

            if (EntityType.ContainerType != null) // contains != null)
            {
                // only see inside containers that the entity can interact through/enter.
                // entities should be able to see inside containers that are owned by/belong to the same allegiance
                if (EntityType.ContainerType.CanTransactWithContainer(allegiance.RepresentativeEntityType)) //.IntelligenceType.CanEnterBuilding(entity.EntityType))
                {
                    //Container contains = Contains;

                    // see inside recursively
                    Contains.IterateContained(sharedKnowledge.SeeEntity);

                }
            }
        }*/


        public bool IsWeatherProof()
        {
            Item itemComponent = Item;
            if (itemComponent != null)
            {
                return itemComponent.IsWeatherProof();
            }

            return false;

        }

        public bool UsesMemory(SharedKnowledge sharedKnowledge)
        {
            return 
                EntityType.GetUsesMemory() // exclude trees and rocks
                && IsStarted() != false // exclude entities not being started
                && HasInterestInEntity(sharedKnowledge.Allegiance); // can interact with the entity
        }

        /// <summary>
        /// many critters do not need to track other items than food...
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasInterestInEntity(Allegiances.Allegiance allegiance) // Entity entity)
        {
            if (EntityType.IntelligenceType != null)
            {
                if (Intelligence.Allegiance == allegiance)
                {
                    return false; // don't track agents in the same allegiance
                }
            }

            EntityType representativeType = allegiance.RepresentativeEntityType;

            // TODO: this could be made more accurate by checking the intelligenceType behaviour flags
            // people and robots that use tools:
            if (representativeType.Person != null)
            {
                // track everything:
                return true;
            }
            else // other critters are interested in food and other agents
            {
                if (EntityType.IntelligenceType != null)
                {
                    return true;
                }

                if (allegiance.IsEatable(EntityType))
                {
                    return true;
                }

                if (EntityType.ThreatType != null)
                {
                    return true; // NEW: critters track threat emitters
                }

                if (representativeType.IntelligenceType.ContainerTransactValue.HasValue
                    && EntityType.ContainerType != null) // NEW: some critters are interested in containers too
                {
                    return true;
                }

                if (representativeType.IntelligenceType.CanMountToolsOrWeapons()
                    && EntityType.ItemType != null)
                {
                    return true; // later, perhaps
                }

                return false;
            }
        }


        public bool? IsStarted()
        {
            NonLivingEntity nonLivingEntity;
            if (EntityType.NonLivingType != null
                && Find(out nonLivingEntity))
            {
                return nonLivingEntity.IsStarted();
            }
            else return null; //false;
        }

        public bool IsInsideVehicle()
        {
            return PassengerInVehicle != null || DrivingVehicle != null;
        }



        //TODO clarify difference between non-moving and not-moving
        // not moving should mean currently stationery
        // non moving should mean incapable of moving
        public bool IsIntelligentAndNonMoving()
        {
            return Intelligence != null
                        && Locomotor != null
                        && !Locomotor.IsMoving();
        }



        public double? FunctionalScore
        {
            get
            {
                BodyComponent body;
                if (Find(out body))
                {
                    return body.Body.FunctionalScore;

                }
                else return null;
            }
        }


        /// <summary>
        /// does not test if Completed!!!
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsFunctional(IKnownEntityData data)
        {
            return !data.PartIsBroken
                && (data.Condition == null || data.Condition.Value > 0f)
                && (data.FunctionalScore == null || data.FunctionalScore > 0d);
        }


        public bool IsVehicleValidForHauling(Entity entity, IKnownEntityData item)
        {
            Vehicle vehicleComponent;
            if (Find(out vehicleComponent)
                && ((vehicleComponent.DrivenBy == null) || vehicleComponent.DrivenBy == entity)
                && AgentStorage != null
                && AgentStorage.ItemStorage.TotalCapacity >= item.Bulk
                && GoalEvaluator.IsOnPlaySite(this)
                && IsCompleted()
                && IsFunctional(this)) // GoalEvaluator.ScoreIsEntityFunctional(this) > 0.0) // omit broken down vehicles
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// remove effects and abilities from carrier or improved entity when the entity has a broken part or condition = 0
        /// </summary>
        public void UpdateFunctionality() // SetNotFunctional()
        {
            if (!IsFunctional(this))
            {
                Entity container;                
                if (GetContainedBy(out container) && container != null)
                {
                    container.Contains.NotifyBrokenContainedEntity(this);
                }

             /*   if (Contains != null) // should be contained by???
                {
                    Contains.NotifyBrokenContainedEntity(this);
                }*/

                /*
                Entity carrier;
                if (CarriedByAgent(out carrier) && carrier != null)
                {
                    carrier.AgentStorage.EndEffectsFromEquippedItem(this);
                    return;
                }

                // Here, add a similar call to ImprovementContainer
                Entity upgraded;
                if (GetUpgradesFor(out upgraded))
                {
                    upgraded.Contains.GetUpgrader();

                }*/
            }
        }



        public bool SendMessage(Message msg)
        {
            Intelligence intelligenceComponent;
            if (Find(out intelligenceComponent))
            {
                if (intelligenceComponent.Brain.SendMessage(msg))
                {
                    return true;
                }

            }

            return false;
        }

        private float avoidDetectionFactor = 0f;

        /// <summary>
        /// 0 - 1: 0 means always detected, 1 means impossible to detect without prior knowledge
        /// </summary>
        public float GetAvoidDetectionFactor()
        {
            Intelligence intelligence = Intelligence;
            if (intelligence != null && intelligence.Brain != null) // Brain is null before ComeOnline()
            {
                float stealthFactor = intelligence.Brain.GetStealthFactorOfActivity();

                if (intelligence.IsStealthy) // will only be able to be stealthy when doing some activities... controlled in Goal
                {
                    float maxSneaking = Math.Max(intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["sneaking"]), intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["hunting"]));

                    stealthFactor =
                        stealthFactor
                        + BiologicalEntity.ActiveStealthRating
                        + 0.8f * maxSneaking; // intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["sneaking"]);

                    //(we can go beyond 1, making detection impossible.)
                    // stealthFactor = Common.Clamp(stealthFactor, 0f, 1f);
                }

                stealthFactor = GetEffect(AffectsNumbers.Stealth, stealthFactor);

                stealthFactor = Common.Clamp(stealthFactor, 0f, 1f);

                return stealthFactor;
            }
            else
            {
                // TODO: resources
                return avoidDetectionFactor;
            }


        }

        public float? GetChanceToIdleWalkShortDistanceAway()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("ChanceToIdleWalkShortDistanceAway");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.IntelligenceType.ChanceToIdleWalkShortDistanceAway;
        }

        public float? GetShortIdleWalkMaxDistance()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("ShortIdleWalkMaxDistance");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.IntelligenceType.ShortIdleWalkMaxDistance;
        }

        public float? GetShortIdleWalkMinDistance()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("ShortIdleWalkMinDistance");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.IntelligenceType.ShortIdleWalkMinDistance;
        }

        public float? GetAggroRange()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("AggroRange");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.IntelligenceType.AggroRange;
        }

        public float? GetAssistanceRange()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("AssistanceRange");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.IntelligenceType.AssistanceRange;
        }

        /// <summary>
        /// in pixels
        /// </summary>
        /// <returns></returns>
        public float GetDaySensorRange()
        {
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("SensorRange");

                if (bioProperty != null)
                {
                    return bioProperty.NumberValue.Value;
                }
            }

            // default:
            return EntityType.SensorType.Range;
        }

        public float GetNightSensorRange()
        {
            float? value = null;
            if (EntityType.BiologicalType != null)
            {
                // see if overridden:
                BioProperty bioProperty = BiologicalEntity.GetBioProperty("SensorRangeAtNight");

                if (bioProperty != null)
                {
                    value = bioProperty.NumberValue.Value;
                }
            }

            // default:
            value = value ?? EntityType.SensorType.RangeAtNight;

            value = GetEffect(AffectsNumbers.NightSensorRange, value.Value);

            return value.Value;
        }

        public int GetVisionRangeInTiles(float lightLevels)
        {
            return (int)(MapManager.oneOverTileSize * (GetVisionRange(lightLevels)));
        }

        public float GetVisionRange(float lightLevels)
        {
            // float heightFactor = MathHelper.Lerp(1f, 1.5f, Location.Z / 200f);

            return /*heightFactor **/ MathHelper.Lerp(GetNightSensorRange(), GetDaySensorRange(), lightLevels);

        }

        private const float constantDetectionFactor = 0.2f;

        /// <summary>
        /// 0 - 1
        /// </summary>
        public float GetDetectionFactor(IDetectable detectable, bool requiresExamineAction)
        {
            Intelligence intelligence = Intelligence;
            if (intelligence != null && intelligence.Brain != null) // Brain is null before ComeOnline()
            {
                float detectionActivityFactor = intelligence.Brain.GetDetectionFactorOfActivity(detectable, requiresExamineAction);

                // this depends on the type of resource and the specific abilities that the entity has
                float detectionSkillFactor = 1f;

                return detectionActivityFactor * detectionSkillFactor;
            }
            else
            {
                return constantDetectionFactor;
            }

        }

        /*   public bool IsEatable(EntityType inquiringEntityType)
           {
               if (this.Item != null && this.Item.Food != null)
                   return true;

               return false;
           }*/

        public bool IsIntelligent
        {
            get { return EntityType.IntelligenceType != null; }
        }



        static Entity()
        {
            //validate presentations and events against this dictionary on startup?
            exposedPropertyValueFunctions.Add("hungerStatus", GetHungerStatus);

            exposedPropertyValueFunctions.Add("foodEnergyLevel", GetFoodEnergyLevel);
            exposedPropertyValueFunctions.Add("foodEnergyStatus", GetFoodEnergyStatus);

            exposedPropertyValueFunctions.Add("proteinLevel", GetProteinLevel);
            exposedPropertyValueFunctions.Add("micronutrientsLevel", GetMicronutrientsLevel);
            exposedPropertyValueFunctions.Add("stimulantsLevel", GetStimulantsLevel);

            exposedPropertyValueFunctions.Add("sleepynessLevel", GetSleepLevel);
            exposedPropertyValueFunctions.Add("sleepStatus", GetSleepStatus);

            exposedPropertyValueFunctions.Add("itemBulkForPresentation", GetItemBulkForPresentation);
            exposedPropertyValueFunctions.Add("energyLevel", GetEnergyLevel);
            exposedPropertyValueFunctions.Add("moraleLevel", GetMoraleLevel);
            exposedPropertyValueFunctions.Add("threatStance", GetThreatStance);

            exposedPropertyValueFunctions.Add("replenishStatus", GetReplenishStatus);
            exposedPropertyValueFunctions.Add("replenishStatusTooltip", GetReplenishStatusTooltip);
            exposedPropertyValueFunctions.Add("ammoStatus", GetAmmoStatus);
            exposedPropertyValueFunctions.Add("inAccessible", GetInaccessible); // also in MemoryFact!
            exposedPropertyValueFunctions.Add("hasThreatJob", GetHasThreatJob); // also in MemoryFact!

            exposedPropertyValueFunctions.Add("ConstructionProgress", GetConstructionProgress);
            exposedPropertyValueFunctions.Add("name", GetNameAsPropertyResult);
            exposedPropertyValueFunctions.Add("type", GetTypeAsPropertyResult);
            exposedPropertyValueFunctions.Add("hitpointLevel", GetHitpointsFraction);
            exposedPropertyValueFunctions.Add("ammoLevel", GetAmmoRoundsLeft);
            exposedPropertyValueFunctions.Add("itemPartCondition", GetCondition);
            exposedPropertyValueFunctions.Add("itemPartConditionTooltip", GetConditionTooltip);
            exposedPropertyValueFunctions.Add("entityIsFunctional", GetEntityIsFunctional);
            exposedPropertyValueFunctions.Add("integrity", GetIntegrity);
            exposedPropertyValueFunctions.Add("daysUntilBreakDown", GetDaysLeftUntilBreakdown);

            exposedPropertyValueFunctions.Add("tradeStorageFraction", GetTradeStorageFraction);
            exposedPropertyValueFunctions.Add("tradeStorageFractionTooltip", GetTradeStorageFractionTooltip);
            exposedPropertyValueFunctions.Add("itemStorageFraction", GetItemStorageFraction);
            exposedPropertyValueFunctions.Add("storageFractionTooltip", GetItemStorageFractionTooltip);
          
            exposedPropertyValueFunctions.Add("totalTradeCapacity", GetTotalTradeCapacity);
            exposedPropertyValueFunctions.Add("totalStorageCapacity", GetTotalStorageCapacity);


            exposedPropertyValueFunctions.Add("totalProductivity", GetTotalProductivity);
            exposedPropertyValueFunctions.Add("skillProductivity", GetCurrentSkillProductivity);
            exposedPropertyValueFunctions.Add("toolProductivity", GetCurrentToolProductivity);
            exposedPropertyValueFunctions.Add("energyLevelProductivity", GetEnergyLevelProductivity);

            exposedPropertyValueFunctions.Add("isAgent", IsAgent);
            exposedPropertyValueFunctions.Add("location", GetLocation);
            exposedPropertyValueFunctions.Add("allegiance", GetAllegiance);
            exposedPropertyValueFunctions.Add("expedition", GetExpedition);

            exposedPropertyValueFunctions.Add("owningExpedition", GetOwningExpedition);
            exposedPropertyValueFunctions.Add("owningAllegiance", GetOwningAllegiance);

            //Caption functions:
            exposedPropertyValueFunctions.Add("skillInUseName", GetSkillInUseName);
            exposedPropertyValueFunctions.Add("toolInUseName", GetToolInUseName);
            exposedPropertyValueFunctions.Add("replenishTypeName", GetReplenishTypeName);

            // ratings
            exposedPropertyValueFunctions.Add("overallRating", GetOverallRating);
            exposedPropertyValueFunctions.Add("comfortRating", GetComfortRating);
            exposedPropertyValueFunctions.Add("foodRating", GetFoodRating);
            exposedPropertyValueFunctions.Add("securityRating", GetSecurityRating);

            exposedPropertyValueFunctions.Add("comfortPrinciplesAndRating", GetComfortPrinciplesAndRating);
            exposedPropertyValueFunctions.Add("foodPrinciplesAndRating", GetFoodPrinciplesAndRating);
            exposedPropertyValueFunctions.Add("securityPrinciplesAndRating", GetSecurityPrinciplesAndRating);


            exposedPropertyValueFunctions.Add("comfortRatingTooltip", GetComfortRatingTooltip);
            exposedPropertyValueFunctions.Add("foodRatingTooltip", GetFoodRatingTooltip);
            exposedPropertyValueFunctions.Add("securityRatingTooltip", GetSecurityRatingTooltip);

            exposedPropertyValueFunctions.Add("foodHappinessTooltip", GetFoodHappinessTooltip);
            exposedPropertyValueFunctions.Add("comfortHappinessTooltip", GetComfortHappinessTooltip);
            exposedPropertyValueFunctions.Add("securityHappinessTooltip", GetSecurityHappinessTooltip);


            exposedPropertyValueFunctions.Add("happiness", GetHappiness);
            exposedPropertyValueFunctions.Add("emigrateRisk", GetEmigrateRisk);
            exposedPropertyValueFunctions.Add("emigrateRiskForPlaySite", GetPlaySiteEmigrateRisk);
            exposedPropertyValueFunctions.Add("emigrateRiskTooltip", GetEmigrateRiskTooltip);

            exposedPropertyValueFunctions.Add("highestUnhappiness", GetHighestUnhappinessType);

            exposedPropertyValueFunctions.Add("emigrationTarget", GetEmigrationTarget);


            /*  exposedPropertyValueFunctions.Add("toggleComfortRatingTooltip", ToggleComfortRatingTooltip);
              exposedPropertyValueFunctions.Add("toggleFoodRatingTooltip", ToggleFoodRatingTooltip);
              exposedPropertyValueFunctions.Add("toggleSecurityRatingTooltip", ToggleSecurityRatingTooltip);
              */

           
            exposedPropertyValueFunctions.Add("freeStorage", GetFreeStorageSpaceAsPropertyResult);
            exposedPropertyValueFunctions.Add("EntityID", GetEntityIDasPropertyResult);
            exposedPropertyValueFunctions.Add("progress", GetProgress);
            exposedPropertyValueFunctions.Add("consumeProgress", GetConsumeProgress);
            exposedPropertyValueFunctions.Add("professionIcon", GetProfessionIcon);
            exposedPropertyValueFunctions.Add("professionDescription", GetProfessionDescription);
            exposedPropertyValueFunctions.Add("homeComfortLevel", GetHomeComfortLevel);
            exposedPropertyValueFunctions.Add("vehicleType", GetVehicleType);
           

            getChildrenProperties.Add("SubstanceTypes", GetSubstances);
            getChildrenProperties.Add("bodyParts", GetBodyParts);
            getChildrenProperties.Add("triggers", GetTriggers);
            getChildrenProperties.Add("itemParts", GetParts);
            getChildrenProperties.Add("home", GetHome);
            getChildrenProperties.Add("anchor", GetAnchor);
            getChildrenProperties.Add("skills", GetSkills);
            getChildrenProperties.Add("nutrition", GetNutrition);
            getChildrenProperties.Add("contained", GetContained);
            getChildrenProperties.Add("effectProfiles", GetEffectProfiles);
            getChildrenProperties.Add("residents", GetResidents);

            /*
                case "bodyParts":
                    {
                        if (Body != null)
                        {
                            Body.GetBodyParts<IHasExposedProperties>(ref listOfChildren);
                        }
                        break;
                    }
                case "triggers":
                    {
                        GetTriggers(ref listOfChildren);
                        break;
                    }
                case "itemParts":
                    {
                        GetParts<IHasExposedProperties>(ref listOfChildren); //, filterKey);
                        break;
                    }
                case "home":
                    {
                        GetHome(getterKnowledge, ref listOfChildren); //, filterKey);
                        break;
                    }
                case "skills":
                    {
                        GetSkills(getterKnowledge, ref listOfChildren);
                        break;
                    }
                case "nutrition":
                    {
                        GetNutrition(ref listOfChildren);
                        break;
                    }
                case "contained":
                    {                        
                        GetContained(ref listOfChildren); 
                        break;
                    }
                case "effectProfiles":
                    {
                        GetEffectProfiles(getterKnowledge, ref listOfChildren);
                        break;
                    }
            */
        }


        public void PrintScriptVariables(System.Text.StringBuilder description)
        {
            if (CustomFields != null)
            {
                foreach (var item in CustomFields)
                {
                    description.AppendLine(item.Key + " = " + item.Value.ToString());
                }
            }

        }



        //   private static Dictionary<string, GetCaption> exposedCaptionFunctions = new Dictionary<string, GetCaption>();

        public string GetCaption(string captionKey)
        {
            PropertyResult? result = GetPropertyValue(captionKey, null);

            if (result.HasValue)
            {
                return result.Value.StringResult;
            }
            else return null;
        }

        private static void GetAnchor(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
        {
            ((Entity)hasProperties).GetAnchor(getterKnowledge, ref listToBeFilledWithParts);
        }

        private void GetAnchor(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            if (EntityType.StructureType != null
                && Structure.AnchorID.HasValue) // if (Structure != null)
            {               
              //  IKnownEntityData anchorData;
               // getterKnowledge.GetKnownData(Structure.AnchorID.Value, out anchorData);

                Entity anchor = Entity.FindByID(Structure.AnchorID.Value);
                if (anchor != null)
                {
                    listOfChildren.Add(anchor);
                }                
            }
        }

       

        private static PropertyResult? GetToolInUseName(IHasExposedProperties hasExposedProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposedProperties).GetToolInUseName();
        }

        private PropertyResult? GetToolInUseName()
        {
            if (Intelligence != null && Intelligence.Brain != null) // Brain is null before ComeOnline()
            {
                PropertyResult result = new PropertyResult();
                result.StringResult = Intelligence.Brain.GetToolInUseName();
                return result;
            }
            else
            {
                return null;
            }
        }

        private static PropertyResult? GetSkillInUseName(IHasExposedProperties hasExposedProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposedProperties).GetSkillInUseName();
        }

        private PropertyResult? GetSkillInUseName()
        {
            if (Intelligence != null && Intelligence.Brain != null) // Brain is null before ComeOnline()
            {
                PropertyResult result = new PropertyResult();
                result.StringResult = Intelligence.Brain.GetSkillInUseName();
                return result;
            }
            else
            {
                return null;
            }
        }

        #region IHasExposedProperties
        //All IHasExposedProperties are casted to be able to call the class specific function for a certain property, 
        //this is safe because GetPropertyValue uses the global functions defined for the class in which it is used
        // However if we by mistake added a global function from another class to our dictionary then it might cause problems, but that should never happen
        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            PropertyResult? result = null;
            PropertyResult customResult;

            if (propertyKey == "harvestDate")
            {

            }


            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else if (CustomFields != null && CustomFields.TryGetValue(propertyKey, out customResult))
            {
                result = customResult;
            }

            return result;
        }



        /// <summary>
        /// the filter key is a property and a value encoded in a string...
        /// perhaps they should be separated..?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="listOfChildren"></param>
        /// <param name="filterKey"></param>
        public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            SharedKnowledge getterKnowledge = null)
        {
            // first get the list of items:
            // see in Site how to optimized lookups

            GetChildrenDelegate getChildren;
            if (getChildrenProperties.TryGetValue(key, out getChildren))
            {
                getChildren(getterKnowledge, this, ref listOfChildren);
            }



            //maybe put these keys in a dictionary, it would make validation possible
            /* switch (key)
             {
               
             }*/

            // then apply filter:
            if (filter != null)
            {
                Site.FilterChildren(listOfChildren, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }

        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            if (value.HasValue)
            {
                switch (propertyKey)
                {
                    // you need to clear the spriteflag before setting a new one
                    case "spriteFlag":
                        SetSpriteFlag(value.Value);
                        return;

                    case "clearFlag":
                        ClearSpriteFlag(value.Value);
                        return;

                    case "addTrigger":
                        AddTrigger(value.Value);
                        return;

                    case "removeTrigger":
                        RemoveTrigger(value.Value);
                        return;

                    case "enableSpecialAction":
                        EnableSpecialAction(value.Value);
                        return;

                    case "disableSpecialAction":
                        DisableSpecialAction(value.Value);
                        return;

                    case "toggleSecurityRatingTooltip":
                        ToggleRatingTooltip(RatingTypes.Security, value.Value.BoolResult.Value);
                        return;

                    case "toggleComfortRatingTooltip":
                        ToggleRatingTooltip(RatingTypes.Comfort, value.Value.BoolResult.Value);
                        return;

                    case "toggleFoodRatingTooltip":
                        ToggleRatingTooltip(RatingTypes.Food, value.Value.BoolResult.Value);
                        return;
                }
            }

            Entity.SetPropertyValue(ref CustomFields, propertyKey, value);

        }

        public static void SetPropertyValue(ref Dictionary<string, PropertyResult> customFields, string propertyKey, PropertyResult? value)
        {
            if (customFields == null)
            {
                customFields = new Dictionary<string, PropertyResult>();
            }

            if (value == null)
            {
                customFields.Remove(propertyKey);
            }
            else
            {
                customFields[propertyKey] = value.Value;
            }
        }


        #endregion


        private void AddTrigger(PropertyResult value)
        {
            Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes[value.StringResult]);
            AttachTrigger(trigger);
        }

        private void RemoveTrigger(PropertyResult value)
        {
            DeleteTriggerOfType(GameData.Instance.AllTriggerTypes[value.StringResult]);
        }




        private void SetSpriteFlag(PropertyResult value)
        {
            StateModifier stateFlag;

            // try to parse the string as an enum...
            if (Enum.TryParse(value.StringResult, out stateFlag))
            {
                SetSpriteStateFlag(stateFlag);
            }

        }

        private void ClearSpriteFlag(PropertyResult value)
        {
            StateModifier stateFlag;

            // try to parse the string as an enum...
            if (Enum.TryParse(value.StringResult, out stateFlag))
            {
                Renderable.ClearSpriteStateFlag(stateFlag);
            }

        }

        /// <summary>   
        /// </summary>
        /// <param name="value"></param>
        private void EnableSpecialAction(PropertyResult value)
        {
            ProcessType action;
            if (GameData.Instance.AllProcessTypes.TryGetValue(value.StringResult, out action))
            {
                EnableSharedSpecialAction(action); // action);
            }
        }

        /// <summary>
        /// Shared by all allegiances
        /// 
        /// entities now have unique processes!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="processKey"></param>
        public void EnableSharedSpecialAction(ProcessType sharedProcessType)
        {
            if (EntityType.SharedSpecialActionTypes != null) // only the actions in the set can be user started 
            {
                // look up the custom process with the same original:
                ProcessType uniqueProcessType = EntityType.SharedSpecialActionTypes.FirstOrDefault(p => p.OriginalProcess == sharedProcessType);
                
                if (uniqueProcessType != null && !AvailableSharedSpecialActions.Any(p => p.OriginalProcess == sharedProcessType))
                {
                    AvailableSharedSpecialActions.Add(uniqueProcessType);
                }
            }

        }

        /*
        /// <summary>
        /// entities now have unique processes!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="processKey"></param>
        public static void EnableSpecialAction(IKnownEntityData entity, ProcessType sharedProcessType) 
        {
            if (entity.EntityType.SpecialActionTypes != null) // only the actions in the set can be user started 
            {
                // look up the custom process with the same original:
                ProcessType uniqueProcessType = entity.EntityType.SpecialActionTypes.FirstOrDefault(p => p.OriginalProcess == sharedProcessType);


                if (uniqueProcessType != null && !entity.AvailableSpecialActions.Any(p => p.OriginalProcess == sharedProcessType))
                {
                    entity.AvailableSpecialActions.Add(uniqueProcessType);
                }
            }
           
        }
        */


        private void DisableSpecialAction(PropertyResult value)
        {
            ProcessType action;
            if (GameData.Instance.AllProcessTypes.TryGetValue(value.StringResult, out action))
            {
                DisableSharedSpecialAction(action.OriginalProcess);
            }
        }

        /// <summary>
        /// Mutex other special actions.
        /// 
        /// Note: If the process has a structure output, then it is easier to use Anchor instead! It also handles salvaged structures!
        /// But Place Fish trap etc. don't have structure outputs.
        ///        
        /// separate availability of actions into PHYSICAL (for all, all the time) and LOCKS (per allegiance)
        /// PHYSICAL availability can be toggled automatically with Anchor.
        /// 
        /// Grow crop: this is a lock (not physical. does not affect other allegiances.) - Is invoked by a process - EnablesSpecialActionProcessTypes in ProcessProductionFinished
        /// Use fertilizer: also a lock.
        /// 
        /// Locks: stored in SharedKnowledge for the allegiance, no syncing required. Are set on IKnownEntityData. REALLY NEEDED???
        /// 
        /// Physical: are only set on the entity. Require syncing:
        /// Use smoke bomb. Enable attack vermin. Build farm plot. Build fish trap. Build turnip hut.
        /// They all have a corresponding job, so the UI will disable the button from the job, NOT a disabled action!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sharedProcessType"></param>
        public void DisableSharedSpecialAction(ProcessType sharedProcessType)
        {
            if (EntityType.SharedSpecialActionTypes != null) // only the actions in the set can be user started 
            {
                 // look up the custom process with the same original:
                ProcessType uniqueProcessType = EntityType.SharedSpecialActionTypes.FirstOrDefault(p => p.OriginalProcess == sharedProcessType);

                if (uniqueProcessType != null)
                {
                    AvailableSharedSpecialActions.Remove(uniqueProcessType);
                }
            }

            /*
            if (entity.AvailableSpecialActions.Contains(sharedProcessType.KeyName)) //!EntityType.SpecialActionTypes.Contains(action))
            {
                entity.AvailableSpecialActions.Remove(sharedProcessType.KeyName);
            }*/
        }


        /// <summary>
        /// enabling a special action by anchor means the action is now possible by ALL allegiances. Therefore it should be called from Structure.Destroy only
        /// </summary>
        /// <param name="entityData"></param>
        public void EnableSpecialActionsUsingAnchor()
        {
            if (EntityType.SharedSpecialActionTypes != null)
            {
                foreach (var processType in EntityType.SharedSpecialActionTypes)
                {
                    if (processType.UsesAnchor())
                    {
                        EnableSharedSpecialAction(processType.OriginalProcess);
                    }
                }
            }
        }

     /*   public static void EnableSpecialActionsUsingAnchor(IKnownEntityData entityData)
        {
            if (entityData.EntityType.SpecialActionTypes != null)
            {
                foreach (var processType in entityData.EntityType.SpecialActionTypes)
                {
                    if (processType.UsesAnchor())
                    {
                        EnableSpecialAction(entityData, processType);
                    }
                }
            }
        }*/

        /// <summary>
        /// disabling a special action by anchor means the action is now physically impossible by ALL allegiances
        /// </summary>
        /// <param name="entityData"></param>
        public void DisableSpecialActionsUsingAnchor() //IKnownEntityData entityData)
        {
            if (EntityType.SharedSpecialActionTypes != null)
            {
                foreach (var processType in EntityType.SharedSpecialActionTypes)
                {
                    if (processType.UsesAnchor())
                    {
                        DisableSharedSpecialAction(processType.OriginalProcess);
                    }
                }
            }
        }

        /* public void DisableSpecialActionsUsingAnchor()
         {
             if (EntityType.SpecialActionTypes != null) // only the actions in the set can be user started 
             {
                 foreach (var processType in EntityType.SpecialActionTypes)
                 {
                     if (processType.UsesAnchor())
                     {
                         DisableSpecialAction(this, processType);
                     }
                 }
             }
         }*/

       

        private static void GetNutrition(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
        {
            ((Entity)hasProperties).GetNutrition(ref listOfChildren);
        }

        private void GetNutrition(ref List<IHasExposedProperties> listOfChildren)
        {
            if (EntityType.ItemType != null && EntityType.ItemType.FoodType != null)
            {
                //Skill currentSkill;
                foreach (var nutrition in EntityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes)
                {

                    listOfChildren.Add(nutrition);
                }
            }
        }

        private static void GetSkills(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
        {
            ((Entity)hasProperties).GetSkills(getterKnowledge, ref listOfChildren);
        }

        private void GetSkills(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            if (getterKnowledge != null)
            {
                if (AllegianceID.HasValue == true)
                {
                    if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                    {
                        return;
                    }

                    /*   if (getterKnowledge.Allegiance.ID != AllegianceID)
                       {
                           return;
                       }*/
                }
            }

            if (EntityType.IntelligenceType != null && Intelligence.Skills != null)
            {
                foreach (var skill in Intelligence.Skills)
                {
                    if (skill.Key.SuppressDisplayForBiologicals && EntityType.BiologicalType != null)
                    {
                        continue;
                    }

                    if (skill.Key.SuppressDisplayForPersons && EntityType.Person != null)
                    {
                        continue;
                    }

                    listOfChildren.Add(skill.Value);

                }

                // sort for display:
                listOfChildren = listOfChildren.OrderBy(c => ((Skill)c).SkillType.SortOrder).ToList();

            }

        }

        private static void GetEffectProfiles(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
        {
            ((Entity)hasProperties).GetEffectProfiles(getterKnowledge, ref listOfChildren);
        }

        private void GetEffectProfiles(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            /* if (getterKnowledge != null)
             {
                 if (AllegianceID.HasValue == true)
                 {
                     if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                     {
                         return;
                     }
                 }
             }*/

            SimEffectsComponent effects = SimEffects;
            if (effects != null)
            {
                foreach (var item in effects.EffectProfiles)
                {
                    listOfChildren.Add(item);
                }

                // sort for display:
                // listOfChildren = listOfChildren.OrderBy(c => ((Skill)c).SkillType.SortOrder).ToList();

            }

        }

        private static void GetTriggers(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
        {
            ((Entity)hasProperties).GetTriggers(ref listOfChildren);
        }

        private void GetTriggers(ref List<IHasExposedProperties> listOfChildren)
        {
            if (attachedTriggers != null)
            {
                listOfChildren.AddRange(attachedTriggers);
            }
        }

        /*  static char[] separator = new char[] { ':' };

          public static void FilterEntityList(string filterKey, 
              List<Entity> listOfEntities,  
              Dictionary<string, EntityID> EntitiesByName,
              Dictionary<string, List<EntityID>> EntitiesByType,
              ref List<Entity> listToBeFilledWithParts)
          {
              if (filterKey.StartsWith("name:")) // "name:Ward Conlan"
              {
                  string[] parts = filterKey.Split(separator);
                  string name = parts[1];
                  name = name.Trim();

                  EntityID entityID;
                  if (EntitiesByName != null)
                  {
                      if (EntitiesByName.TryGetValue(name, out entityID))
                      {
                          Entity entity = Entity.FindByID(entityID);
                          if (entity != null)
                          {
                              listToBeFilledWithParts.Add(entity);
                          }

                      }
                  }
                  else
                  {

                  }
              }
              else if (filterKey.StartsWith("type:")) // "type:structure:skimmerHull"
              {
                  //  string[] parts = filterKey.Split(separator);

                  int firstIndex = filterKey.IndexOf(':');

                  string type = filterKey.Substring(firstIndex + 1);


                  //string type = parts[1];
                  type = type.Trim();

                  List<EntityID> list;
                  if (EntitiesByType.TryGetValue(type, out list))
                  {
                      foreach (var item in list)
                      {
                          Entity entity = Entity.FindByID(item);
                          if (entity != null)
                          {
                              listToBeFilledWithParts.Add(entity);
                          }
                      }

                  }
              }

          }*/

        private static void GetBodyParts(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> list)
        {
            ((Entity)hasProperties).GetBodyParts(ref list);
        }

        private void GetBodyParts(ref List<IHasExposedProperties> list)
        {
            if (Body != null)
            {
                Body.GetBodyParts<IHasExposedProperties>(ref list);
            }
        }

        private static void GetSubstances(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfSubstances)
        {
            ((Entity)hasProperties).GetSubstances(ref listOfSubstances);
        }

        private void GetSubstances(ref List<IHasExposedProperties> listOfSubstances)
        {
            if (SubstanceBulkAmounts != null)
            {
                foreach (var substance in SubstanceBulkAmounts)
                {
                    listOfSubstances.Add(substance.Value);
                }
            }
        }

        private static void GetParts<T>(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<T> listToBeFilledWithParts) where T : IHasExposedProperties
        {
            ((Entity)hasProperties).GetParts(ref listToBeFilledWithParts);
        }

        private void GetParts<T>(ref List<T> listToBeFilledWithParts) where T : IHasExposedProperties
        {
            if (Parts != null)
            {
                foreach (var part in Parts)
                {
                    listToBeFilledWithParts.Add(part.ConvertToDesiredType<T>());
                }
            }
        }

        private void GetInhabitants(ref List<IHasExposedProperties> listOfChildren)
        {
            if (EntityType.StructureType != null)
            {
                if (Contains != null)
                {
                    List<IHasExposedProperties> listOfInhabitants = new List<IHasExposedProperties>();
                    Contains.IterateContained(entity =>
                    {
                        if (entity.EntityType.BiologicalType != null)
                        {
                            listOfInhabitants.Add(entity.ConvertToDesiredType<IHasExposedProperties>());
                        }
                    });
                    listOfChildren = listOfInhabitants;
                }
            }
        }

        private static void GetContained(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
        {
            ((Entity)hasProperties).GetContained(ref listOfChildren);
        }

        private void GetContained(ref List<IHasExposedProperties> listOfChildren)
        {
            if (Contains != null)
            {
                List<IHasExposedProperties> containedEntities = new List<IHasExposedProperties>();
                Contains.IterateContained(entity =>
                {
                    
                    containedEntities.Add(entity.ConvertToDesiredType<IHasExposedProperties>());
                    
                });

                listOfChildren = containedEntities;
            }
        }

        public static PropertyResult? GetConditionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetConditionTooltip((Entity)hasExposed);
        }

      

        public static PropertyResult? GetTradeStorageFractionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTradeStorageFractionTooltip();
        }

        private PropertyResult? GetTradeStorageFractionTooltip()
        {
            if (Contains != null)
            {
                TerminalContainer terminal = Contains as TerminalContainer;
                if (terminal != null)
                {
                    float stored = terminal.TotalTradeItemsStored;
                    float capacity = terminal.TotalTradeItemStorageCapacity;

                    var conditions = terminal.GetStorageSpaces();
                    PropertyResult result = FormatStorageTooltip("Trade storage", conditions, stored, capacity);
                    return result;
                }
            }

            return null;
        }

        public static PropertyResult? GetItemStorageFractionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetItemStorageFractionTooltip();
        }

        private PropertyResult? GetItemStorageFractionTooltip()
        {
            if (Contains != null)
            {
                IStorage storage = Contains as IStorage;
                if (storage != null)
                {
                    var conditions = storage.GetStorageSpaces();
                    PropertyResult result = FormatStorageTooltip( "Item storage", conditions, storage.TotalStored, storage.TotalItemStorageCapacity);
                    return result;
                }
            }

            return null;
        }

        private static PropertyResult FormatStorageTooltip(string header, Dictionary<StorageCondition, Storage> /*List<string>*/ condition, float stored, float capacity) //IStorage storage)
        {
            PropertyResult result = new PropertyResult();
            StringBuilder text = new StringBuilder();
            Common.AppendHeaderOnLightBG(text, header);

            foreach (var item in condition)
            {
                if (Common.IsGreaterThan(item.Value.TotalCapacity, 0d))
                {
                    Common.AppendLine(text, item.Key.Name);
                }
            }

            Common.AppendDivider(text);
            Common.Append(text, "Used percentage: ");
            Common.ValueTint valueTint;
            double percentage = stored / capacity;
            if (percentage > 0.98f)
            {
                valueTint = Common.ValueTint.Negative;
            }
            else
            {
                valueTint = Common.ValueTint.Positive;
            }

            Common.AppendPercentage(text,
                percentage, true, valueTint);

            Common.AppendLine(text);
            Common.Append(text, "Total: ");
            Common.Append(text, Common.ValueToDecimalString(stored, true, valueTint));
            Common.Append(text, " / ");
            Common.Append(text, Common.ValueToDecimalString(capacity, true, valueTint));
            Common.Append(text, " BLK");

            result.StringResult = text.ToString();
            return result;
        }

        public static PropertyResult? GetItemStorageFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetItemStorageFraction();
        }

        private PropertyResult? GetItemStorageFraction()
        {
            if (Contains != null)
            {
                IStorage storage = Contains as IStorage;
                if (storage != null)
                {
                    PropertyResult result = new PropertyResult();

                    result.NumberResult = storage.TotalStored / storage.TotalItemStorageCapacity;
                    return result;
                }
            }

            return null;
        }

        public static PropertyResult? GetTotalTradeCapacity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTotalTradeCapacity();
        }

        private PropertyResult? GetTotalTradeCapacity()
        {
            if (Contains != null)
            {
                TerminalContainer terminal = Contains as TerminalContainer;
                if (terminal != null)
                {
                    PropertyResult result = new PropertyResult();

                    result.NumberResult = terminal.TotalTradeItemStorageCapacity;
                    return result;
                }
            }

            return null;
        }

        public static PropertyResult? GetTotalStorageCapacity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTotalStorageCapacity();
        }

        private PropertyResult? GetTotalStorageCapacity()
        {
            if (Contains != null)
            {
                IStorage storage = Contains as IStorage;
                if (storage != null)
                {
                    PropertyResult result = new PropertyResult();

                    result.NumberResult = storage.TotalItemStorageCapacity;
                    return result;
                }
            }

            return null;
        }

        public static PropertyResult? GetTradeStorageFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTradeStorageFraction();
        }

        private PropertyResult? GetTradeStorageFraction()
        {
            if (Contains != null)
            {
                TerminalContainer terminal = Contains as TerminalContainer;
                if (terminal != null)
                {
                    PropertyResult result = new PropertyResult();

                    result.NumberResult = terminal.TotalTradeItemsStored / terminal.TotalTradeItemStorageCapacity;
                    return result;
                }
            }

            return null;
        }


        // TB 19.03.14: property getter to get free space in the storage
        public static PropertyResult? GetFreeStorageSpaceAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetFreeStorageSpaceAsPropertyResult();
        }

        private PropertyResult GetFreeStorageSpaceAsPropertyResult()
        {
            PropertyResult freeStorageResult = new PropertyResult();
            freeStorageResult.NumberResult = (float)(TotalItemStorageCapacity - TotalStored);
            return freeStorageResult;
        }

        public static PropertyResult? GetEntityIDasPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEntityIDasPropertyResult();
        }


        private PropertyResult GetEntityIDasPropertyResult()
        {
            PropertyResult EntityIDPropertyResult = new PropertyResult();
            EntityIDPropertyResult.StringResult = GetEntityID().ToString();
            return EntityIDPropertyResult;
        }


        public Type ConvertToDesiredType<Type>() where Type : IHasExposedProperties
        {
            IHasExposedProperties item = this;
            return (Type)item;
        }
        public string GetDefaultCaption(string propertyKey)
        {
            return EntityType.Name;
        }
        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = "" + EntityID;
        }
        public string KeyName
        {
            get { return EntityType.KeyName; }
        }
        public EntityID? GetEntityID()
        {
            return EntityID;
        }

        public bool GetIsSeenDirectly()
        {
            return true;
        }

        /// <summary>
        /// use GetDisplayName for UI output!
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Name ?? EntityType.Name;           
        }

        public string GetDisplayName()
        {
            return GetDisplayName(this);
        }

        public static string GetDisplayName(IKnownEntityData entityData)
        {
            if (entityData.EntityType.UseTypeNameForDisplay)
            {
                return entityData.EntityType.Name;
            }
            else
            {
                return entityData.Name ?? entityData.EntityType.Name;
            }
        }

        /*  public static string GetDisplayName(IKnownEntityData entity)
          {
              if (!string.IsNullOrEmpty(entity.Name)) // EntityType.PersonType != null)
              {
                  return entity.Name;
              }
              else
              {
                  return entity.EntityType.Name;
              }
          }*/

        public Vector3 RenderedLocation
        {
            get
            {
                if (Renderable.RenderAsModel != null)
                {
                    return Renderable.RenderAsModel.Location;
                }
                else
                {
                    return Renderable.Location.Value;
                }
            }
        }
        #region Exposed properties

        public static PropertyResult? GetNameAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetNameAsPropertyResult();
        }

        private PropertyResult GetNameAsPropertyResult()
        {
            PropertyResult nameResult = new PropertyResult();
            nameResult.StringResult = GetName();
            return nameResult;
        }

        public static PropertyResult? GetTypeAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTypeAsPropertyResult();
        }

        private PropertyResult GetTypeAsPropertyResult()
        {
            PropertyResult nameResult = new PropertyResult();
            nameResult.StringResult = EntityType.KeyName;
            return nameResult;
        }

        public static PropertyResult? GetConstructionProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetConstructionProgress(getterKnowledge);
        }
        private PropertyResult? GetConstructionProgress(SharedKnowledge getterKnowledge)
        {
            if (NonLivingEntity != null && Structure != null)
            {
                PropertyResult progressResult = new PropertyResult();
                progressResult.NumberResult = NonLivingEntity.Progress;
                return progressResult;
            }
            else
            {
                return null;
            }
        }


        public static PropertyResult? GetReplenishTypeName(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetReplenishTypeName(getterKnowledge);
        }

        private PropertyResult? GetReplenishTypeName(SharedKnowledge getterKnowledge)
        {
            if (EntityType.ContainerType != null
                && EntityType.ContainerType.GetRequiresReplenishType() != null) // .RequiresEnergyType != null) 
            {
                if (EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType != null)
                {
                    PropertyResult result = new PropertyResult();
                    result.StringResult = EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.FuelClientString;

                    return result;
                }
            }
            return null;
        }


        public static PropertyResult? GetReplenishStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetReplenishStatus(getterKnowledge, (IKnownEntityData)hasExposed);
            // return ((Entity)hasExposed).GetReplenishStatus(getterKnowledge);
        }

        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetReplenishStatus(SharedKnowledge getterKnowledge, IKnownEntityData entityData)
        {
            float? requiredAmount;
            bool? canBeReplenished;

            if (GetReplenishStatus(getterKnowledge, entityData, out canBeReplenished, out requiredAmount))
            {
                PropertyResult replenishResult = new PropertyResult();

                // convert to float to match thresholds in presentation type:
                replenishResult.NumberResult = canBeReplenished.Value ? 1f : 0f;

                return replenishResult;
            }
            else
            {
                return null;
            }


           /* if (entityData.EntityType.NonLivingType != null
               && entityData.OwnedBy != null) // only if owned... hmmm...
            {
                IOwner owner;
                LookUpOwners.ResolveEntityOwner(entityData, out owner);

                if (owner == null)
                    return null;

                if (getterKnowledge != null)
                {
                    // only show status on entities owned by the allegiance:
                    if (owner == null || owner.Allegiance != getterKnowledge.Allegiance)
                    {
                        return null;
                    }
                }

                Expedition owningExpedition = owner as Expedition;
                if (owningExpedition != null)
                {
                    PropertyResult replenishResult = new PropertyResult();

                    float? minimumRequiredAmount;
                    bool canBeReplenished = The.Client.GetToolReplenishStatus(owningExpedition, entityData.EntityType, out minimumRequiredAmount); 

                    // convert to float to match thresholds in presentation type:
                    replenishResult.NumberResult = canBeReplenished ? 1f : 0f;

                    return replenishResult;
                }
                else return null;
            }

            return null;*/

        }


        private static bool GetReplenishStatus(SharedKnowledge getterKnowledge, IKnownEntityData entityData, out bool? canBeReplenished, out float? requiredAmount)
        {
            canBeReplenished = null;
            requiredAmount = null;

            if (entityData.EntityType.NonLivingType != null
              && entityData.OwnedBy != null) // only if owned... hmmm...
            {
                IOwner owner;
                LookUpOwners.ResolveEntityOwner(entityData, out owner);

                if (owner == null)
                    return false;

                if (getterKnowledge != null)
                {
                    // only show status on entities owned by the allegiance:
                    if (owner == null || owner.Allegiance != getterKnowledge.Allegiance)
                    {
                        return false;
                    }
                }

                Expedition owningExpedition = owner as Expedition;
                if (owningExpedition != null)
                {                   
                   // float? minimumRequiredAmount;
                    canBeReplenished = The.Client.GetToolReplenishStatus(owningExpedition, entityData.EntityType, out requiredAmount);

                    // convert to float to match thresholds in presentation type:
                   // replenishResult.NumberResult = canBeReplenished ? 1f : 0f;

                    //return replenishResult;

                    return true;
                }
                else return false;
            }

            return false;
        }


        public static PropertyResult? GetReplenishStatusTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetReplenishStatusTooltip(getterKnowledge, (IKnownEntityData)hasExposed);
        }

        public static PropertyResult? GetReplenishStatusTooltip(SharedKnowledge getterKnowledge, IKnownEntityData entityData)
        {
            float? requiredAmount;
            bool? canBeReplenished;

            if (GetReplenishStatus(getterKnowledge, entityData, out canBeReplenished, out requiredAmount))
            {
                PropertyResult replenishResult = new PropertyResult();

                if (canBeReplenished == false)
                {
                    StringBuilder text = new StringBuilder();
                    Common.Append(text, "Not enough suitable fuel is available within a certain range to complete any of the tasks.");
                    if (requiredAmount.HasValue)
                    {
                        Common.AppendLine(text);
                        Common.Append(text, " The minimum required fuel is: ");
                        Common.AppendFormat(text, "{0:N2} BLK", true, requiredAmount.Value);
                    }

                    replenishResult.StringResult = text.ToString();
                }

                return replenishResult;
            }
            else
            {
                return null;
            }

        }

      

        public static PropertyResult? GetInaccessible(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return MemoryFact.GetInaccessibleStatus(((Entity)hasExposed).ID);
            
           // return ((Entity)hasExposed).GetInaccessible();
        }

       /* public PropertyResult? GetInaccessible()
        {
            PropertyResult result = MemoryFact.GetInaccessibleStatus(EntityID);
            
            return result;
        }*/

        public static PropertyResult? GetHasThreatJob(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetHasThreatJob(((Entity)hasExposed).ID);
        }

        public static PropertyResult? GetHasThreatJob(EntityID entityID)
        {
            PropertyResult result = new PropertyResult();

            Job job;
            if (The.InGameUI.UIAllegiance.SharedKnowledge.AllKnownEntities.ThreatJobsByTarget.TryGetValue(entityID, out job))
            {
                result.NumberResult = 1f;
                //result.BoolResult = true;               
            }
            else
            {
                result.NumberResult = 0f;
            }

            return result;
        }


        public static PropertyResult? GetItemBulkForPresentation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetItemBulkForPresentation((Entity)hasExposed);
        }

        public static PropertyResult? GetThreatStance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {

            return ((Entity)hasExposed).GetThreatStance(getterKnowledge);
        }

        public static PropertyResult? GetMoraleLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {

            return ((Entity)hasExposed).GetMoraleLevel(getterKnowledge);
        }

        public static PropertyResult? GetEnergyLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEnergyLevel(getterKnowledge);
        }


        // Ratings
        public static PropertyResult? GetOverallRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetOverallRating(getterKnowledge);
        }

        public static PropertyResult? GetComfortRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Comfort);
        }

        public static PropertyResult? GetSecurityRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Security);
        }

        public static PropertyResult? GetFoodRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Food);
        }

        public static PropertyResult? GetComfortPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Comfort);
        }
        public static PropertyResult? GetSecurityPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Security);
        }
        public static PropertyResult? GetFoodPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Food);
        }

        public static PropertyResult? GetSecurityRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Security);
        }

        public static PropertyResult? GetComfortRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Comfort);
        }

        public static PropertyResult? GetFoodRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Food);
        }

        public static PropertyResult? GetFoodHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Food);
        }
        public static PropertyResult? GetComfortHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Comfort);
        }
        public static PropertyResult? GetSecurityHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Security);
        }

        public static PropertyResult? GetEmigrateRiskTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEmigrateRiskTooltip(getterKnowledge);
        }

        public static PropertyResult? GetHighestUnhappinessType(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHighestUnhappinessType();
        }

        public static PropertyResult? GetEmigrationTarget(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEmigrationTarget();
        }


        /* public static PropertyResult? ToggleSecurityRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
         {
             return ((Entity)hasExposed).ToggleRatingTooltip(getterKnowledge, StatTypes.Security);
         }*/


        /* public static PropertyResult? GetHome(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge)
         {
             return ((Entity)hasExposed).GetHome(getterKnowledge);
         }*/

        public static PropertyResult? GetHappiness(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHappiness(getterKnowledge);
        }

        public static PropertyResult? GetEmigrateRisk(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEmigrateRisk(getterKnowledge);
        }

        public static PropertyResult? GetPlaySiteEmigrateRisk(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetPlaySiteEmigrateRisk(getterKnowledge);
        }


        /// <summary>
        /// shared with MemoryFact
        /// Returns the entity bulk multiplied by 100.0. 
        /// Only for items!
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetItemBulkForPresentation(IKnownEntityData entityData)
        {
            if (entityData.EntityType.ItemType != null)
            {
                PropertyResult bulkResult = new PropertyResult();
                bulkResult.NumberResult = GetBulkAsNumber(entityData.Bulk);

                return bulkResult;
            }
            else
            {
                return null;
            }
        }

        private static float GetBulkAsNumber(float bulk)
        {
            return 100f * bulk;
        }

        public static string GetBulkAsString(float bulk)
        {
            // formatting is done similar to:
            // property: itemBulkForPresentation
            // presentation: itemBulkPresentation

            return GetBulkAsNumber(bulk).ToString("F0"); // no decimals

        }

        private PropertyResult? GetThreatStance(SharedKnowledge getterKnowledge)
        {
            if (Intelligence != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (getterKnowledge.Allegiance.ID != AllegianceID)
                        {
                            return null;
                        }
                    }
                }
                PropertyResult threatResult = new PropertyResult();
                threatResult.NumberResult = (int)Intelligence.ThreatStance;

                return threatResult;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetMoraleLevel(SharedKnowledge getterKnowledge)
        {
            if (Intelligence != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (getterKnowledge.Allegiance.ID != AllegianceID)
                        {
                            return null;
                        }
                    }
                }
                PropertyResult threatResult = new PropertyResult();
                threatResult.NumberResult = (int)Intelligence.Morale * 100;

                return threatResult;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetOverallRating(SharedKnowledge getterKnowledge)
        {
            if (EntityType.IntelligenceType != null && Intelligence.IsIndependent()) // EntityType.IntelligenceType.IsIndependent == true)
            {
                if (getterKnowledge != null)
                {
                    if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                    {
                        return null;
                    }
                }
                PropertyResult result = new PropertyResult();
                result.NumberResult = (float)Intelligence.Statistics.GetOverallRating(); // .Comfort;


                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetHappiness(SharedKnowledge getterKnowledge)
        {
            if (EntityType.IntelligenceType != null && Intelligence.HasHappiness())
            {
                if (getterKnowledge != null)
                {
                    if (!RatingsAreVisible(getterKnowledge))
                    {
                        return null;
                    }
                }
                PropertyResult result = new PropertyResult();

                result.NumberResult = (float)PersonEntity.Personality.Happiness;


                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetEmigrateRisk(SharedKnowledge getterKnowledge)
        {
            if (EntityType.IntelligenceType != null
                // && Intelligence.IsIndependent() 
                && Intelligence.EmigrateDecider != null)
            {
                PropertyResult result;
                if (Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
                {
                    if (getterKnowledge != null)
                    {
                        if (!RatingsAreVisible(getterKnowledge))
                        {
                            return null;
                        }
                    }
                    result = new PropertyResult();
                    result.NumberResult = (float)Intelligence.EmigrateDecider.MigrationRisk;
                }
                else
                {
                    result = new PropertyResult();
                    result.NumberResult = -1f;
                }

                return result;
            }
            else
            {
                return null;
            }
        }


        private PropertyResult? GetPlaySiteEmigrateRisk(SharedKnowledge getterKnowledge)
        {
            if (Site == null || !Site.IsPlaySite)
            {
                return null;
            }
            else
            {
                return GetEmigrateRisk(getterKnowledge);
            }

        }

        private bool RatingsAreVisible(SharedKnowledge getterKnowledge)
        {
            if (getterKnowledge.Allegiance.ID == AllegianceID
                || (EntityType.Person != null && this.Site != The.Sim.PlaySite)) // show ratings for possible immigrants...
            {
                return true;
            }

            return false;
        }

        private PropertyResult? GetRating(SharedKnowledge getterKnowledge, RatingTypes statType)
        {
            if (EntityType.IntelligenceType != null && Intelligence.IsIndependent()) // EntityType.IntelligenceType.IsIndependent == true)
            {
                if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                {
                    return null;
                }

                PropertyResult result = new PropertyResult();

                result.NumberResult = (float)Intelligence.Statistics.GetRating(statType);

                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetPrinciplesAndRating(SharedKnowledge getterKnowledge, RatingTypes statType)
        {
            if (EntityType.IntelligenceType != null && Intelligence.IsIndependent() && PersonEntity != null) // EntityType.IntelligenceType.IsIndependent == true)
            {
                if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                {
                    return null;
                }

                PropertyResult result = new PropertyResult();

                Pair<float, float> pair = new Pair<float, float>();
                pair.First = (float)Intelligence.Statistics.GetRating(statType);
                pair.Second = (float)PersonEntity.Personality.Principles[statType];

                result.NumberPairResult = pair;

                return result;
            }
            else
            {
                return null;
            }
        }


        private PropertyResult? GetHappinessTooltip(SharedKnowledge getterKnowledge, RatingTypes statType)
        {
            if (EntityType.IntelligenceType != null && Intelligence.IsIndependent() && PersonEntity != null)
            {
                if (!RatingsAreVisible(getterKnowledge))
                {
                    return null;
                }

                PropertyResult result = new PropertyResult();

                Rating stat = Intelligence.Statistics.GetStatisticByKey(statType);

                // return condition + principle
                StringBuilder text = new StringBuilder();

                Common.AppendLine(text, "The bar shows the relation between the person's");
                text.Append("PRINCIPLES (Vertical marker): ");
                text.Append(Common.PercentageToString(PersonEntity.Personality.Principles[statType]));

                Common.AppendLine(text);
                text.Append(stat.GetRatingsBreakdown());


                result.StringResult = text.ToString();

                return result;
            }
            else
            {
                return null;
            }
        }


        private PropertyResult? GetRatingTooltip(SharedKnowledge getterKnowledge, RatingTypes statType)
        {
            if (EntityType.IntelligenceType != null && Intelligence.IsIndependent())
            {
                if (!RatingsAreVisible(getterKnowledge))
                {
                    return null;
                }

                PropertyResult result = new PropertyResult();

                Rating stat = Intelligence.Statistics.GetStatisticByKey(statType);

                result.StringResult = stat.GetRatingsBreakdown();

                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetEmigrateRiskTooltip(SharedKnowledge getterKnowledge)
        {
            if (EntityType.IntelligenceType != null
                && Intelligence.IsIndependent()
                && Intelligence.EmigrateDecider != null)
            {
                if (!RatingsAreVisible(getterKnowledge))
                {
                    return null;
                }

                PropertyResult result = new PropertyResult();

                if (Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
                {
                    result.StringResult = Intelligence.EmigrateDecider.GetMigrateRiskTooltip();
                }
                else
                {
                    result.StringResult = Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(null); // true);
                }

                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetHighestUnhappinessType()
        {
            if (EntityType.IntelligenceType != null
                && Intelligence.IsIndependent())
            {
                PropertyResult result = new PropertyResult();

                result.StringResult = Statistic.RatingsTypeToKey(PersonEntity.Personality.GetHighestUnhappiness());

                return result;
            }
            else
            {
                return null;
            }
        }

        private PropertyResult? GetEmigrationTarget()
        {
            if (EntityType.IntelligenceType != null
                && Intelligence.CanEmigrate())
            {
                PropertyResult result = new PropertyResult();

                string emigrateTo = null;
                if (Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue
                    /*&& Intelligence.EmigrateDecider.MigrationRisk > 0f*/)
                {
                    Allegiance target = LookUp<Allegiance, AllegianceID>.FindByID(Intelligence.EmigrateDecider.PreferredMigrationTarget.Value);
                    if (target != null)
                    {
                        emigrateTo = target.Site.Name;
                        result.StringResult = emigrateTo;

                        return result;
                    }
                }
            }

            return null;
        }


        private void ToggleRatingTooltip(RatingTypes statType, bool composeTooltip)
        {
            Rating stat = Intelligence.Statistics.GetStatisticByKey(statType);

            stat.ToggleComposeBreakdown(composeTooltip);
        }

        private static void GetResidents(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
        {
            GetResidents((Entity)hasProperties, getterKnowledge, ref listToBeFilledWithParts);
        }

        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="getterKnowledge"></param>
        /// <param name="listOfChildren"></param>
        public static void GetResidents(IKnownEntityData entityData, SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            /*if (getterKnowledge != null)
            {
                if (AllegianceID.HasValue == true)
                {
                    if (getterKnowledge.Allegiance.ID != AllegianceID)
                    {
                        return; // null;
                    }
                }
            }*/

            List<Entity> list = Residence.GetResidents(entityData);
            if (list != null)
            {
                listOfChildren.AddRange(list);
            }
        }

        /*
        private void GetResidents(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            if (entityData.EntityType.ContainerType != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (getterKnowledge.Allegiance.ID != AllegianceID)
                        {
                            return; // null;
                        }
                    }
                }

                HomeContainer home = Contains as HomeContainer;
                if (home != null)
                {
                    home.Residence.Residents


                }
            }

        }*/



        private static void GetHome(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
        {
            ((Entity)hasProperties).GetHome(getterKnowledge, ref listToBeFilledWithParts);
        }

        private void GetHome(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
        {
            if (EntityType.IntelligenceType != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (getterKnowledge.Allegiance.ID != AllegianceID)
                        {
                            return; // null;
                        }
                    }
                }

                if (PersonEntity != null && PersonEntity.Household != null
                    && PersonEntity.Household.Home.HasValue && getterKnowledge != null)
                {
                    IKnownEntityData homeData;
                    getterKnowledge.GetKnownData(PersonEntity.Household.Home.Value, out homeData);

                    if (homeData != null)
                    {
                        listOfChildren.Add(homeData);
                    }
                }

            }

        }


        private PropertyResult? GetEnergyLevel(SharedKnowledge getterKnowledge)
        {
            if (BiologicalEntity != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (getterKnowledge.Allegiance.ID != AllegianceID)
                        {
                            return null;
                        }
                    }
                }
                PropertyResult energyResult = new PropertyResult();
                energyResult.NumberResult = BiologicalEntity.EnergyLevel;

                return energyResult;
            }
            else
            {
                return null;
            }
        }

        public static PropertyResult? GetEnergyLevelProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetEnergyLevelProductivity(getterKnowledge);
        }

        private PropertyResult? GetEnergyLevelProductivity(SharedKnowledge getterKnowledge)
        {
            Intelligence intelligenceComponent;
            if (Find(out intelligenceComponent))
            {
                if (intelligenceComponent.Brain.GetCurrentTotalProductivity() != null)
                {
                    return GetEnergyLevel(getterKnowledge);
                }
            }

            return null;
        }


        public static PropertyResult? GetEntityIsFunctional(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            // return ((Entity)hasExposed).GetCondition();
            return GetEntityIsFunctional((Entity)hasExposed);
        }


        public static PropertyResult? GetIntegrity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetIntegrity((Entity)hasExposed);
        }

        public static PropertyResult? GetCondition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {           
            return GetCondition((Entity)hasExposed);
        }


        public static PropertyResult? GetDaysLeftUntilBreakdown(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetDaysLeftUntilBreakdown((Entity)hasExposed);
        }


        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetEntityIsFunctional(IKnownEntityData entityData)
        {
            if (entityData.EntityType.NonLivingType != null)
            {

                PropertyResult conditionResult = new PropertyResult();

                bool isFunctional = Entity.IsFunctional(entityData);
                if (isFunctional)
                {
                    conditionResult.NumberResult = 1;
                }
                else
                {
                    conditionResult.NumberResult = 0;
                }
                return conditionResult;
            }
            else
            {
                return null;
            }
        }



        private static bool ShowCondition(IKnownEntityData entityData) 
        {
            EntityType entityType = entityData.EntityType;

            return entityType.NonLivingType != null
                && (entityType.NonLivingType.FinalDegradeType != null || entityType.Parts != null)//;
                && entityData.IsStarted() == true;
        }

        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetIntegrity(IKnownEntityData entityData)
        {
            if (ShowCondition(entityData)/* entityData.EntityType.NonLivingType.FinalDegradeType != null*/
                && entityData.Integrity.HasValue)
            {
                PropertyResult result = new PropertyResult();

                // use this??
                //We don´t want to have to list all item keynames in order to make them show data, 
                //instead all items which share a degrade type will use the same state descriptions
                if (entityData.EntityType.NonLivingType.FinalDegradeType != null)
                {
                    result.PropertyKeyName = entityData.EntityType.NonLivingType.FinalDegradeType.KeyName;
                }

                result.NumberResult = entityData.Integrity; // (float)(entityData.Integrity ?? 1d);


                return result;
            }
            else
            {
                return null;
            }
        }


        public static PropertyResult? GetDaysLeftUntilBreakdown(IKnownEntityData entityData)
        {
            if (entityData.EntityType.NonLivingType != null
                && entityData.ConditionChangeSpeed.HasValue)
            {
                PropertyResult result = new PropertyResult();

                double timeInDaysUntilSpoiling = NonLivingEntity.GetDaysLeftUntilBreakdown(entityData.Condition.Value, entityData.ConditionChangeSpeed.Value);

                if (timeInDaysUntilSpoiling > 12) // suppress...
                {
                    return null;
                }
                else
                {
                    result.NumberResult = (float)timeInDaysUntilSpoiling;

                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetConditionTooltip(IKnownEntityData entityData)
        {
            if (entityData.EntityType.NonLivingType != null
                && entityData.EntityType.NonLivingType.FinalDegradeType != null
                && entityData.IsStarted() == true)
            {
              
                PropertyResult result = new PropertyResult();
                StringBuilder text = new StringBuilder();
                Common.AppendHeaderOnLightBG(text, "Condition");
                Common.AppendDivider(text);

                Entity entity = entityData as Entity;
                if (entity != null)
                {
                   
                    Storage storedIn = null;
                    if (!entity.StoredIn(out storedIn))
                    {
                        return null;
                    }

                    StorageCondition storageCondition;
                
                    if (storedIn != null)
                    {
                        storageCondition = storedIn.StorageConditions;
                    }
                    else
                    {
                        /*if (entity.PartOf != null)
                        {

                        }
                        else
                        {*/
                            storageCondition = GameData.Instance.AllStorageConditions["exposed"];
                       // }
                    }

                    Common.Append(text, "Storage: ");
                    Common.Append(text, storageCondition.Name, true);
                    Common.AppendLine(text);
                }
             
                Common.Append(text, "Durability profile: ");
                Common.Append(text, entityData.EntityType.NonLivingType.FinalDegradeType.Name, true);
                Common.AppendLine(text);

                Common.Append(text, "Current condition: ");
                Common.ValueTint valueTint;
                double percentage = entityData.Condition ?? 1d;
               /* if (percentage > 0.98f)
                {
                    valueTint = Common.ValueTint.Negative;
                }
                else
                {
                    valueTint = Common.ValueTint.Positive;
                }*/

                Common.AppendPercentage(text,
                    percentage, true, Common.ValueTint.Neutral);

                if (entityData.ConditionChangeSpeed.HasValue && !Common.IsZero(entityData.ConditionChangeSpeed.Value))
                {
                    double timeInDaysUntilSpoiling = NonLivingEntity.GetDaysLeftUntilBreakdown(entityData.Condition.Value, entityData.ConditionChangeSpeed.Value);

                    Common.AppendLine(text);
                    Common.Append(text, "Days left: ");
                    Common.Append(text, Common.ValueToDecimalString((float)timeInDaysUntilSpoiling, true, Common.ValueTint.Neutral));                   
                }

                result.StringResult = text.ToString();


                return result;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetCondition(IKnownEntityData entityData)
        {
            if (ShowCondition(entityData)) //(entityData.EntityType.NonLivingType.FinalDegradeType != null || entityData.EntityType.Parts != null)
            {
                PropertyResult conditionResult = new PropertyResult();
                //We don´t want to have to list all item keynames in order to make them show data, 
                //instead all items which share a degrade type will use the same state descriptions
                if (entityData.EntityType.NonLivingType.FinalDegradeType != null)
                {
                    conditionResult.PropertyKeyName = entityData.EntityType.NonLivingType.FinalDegradeType.KeyName;
                }

                if (entityData.EntityID == (EntityID)6604)
                {

                }

                if (entityData.PartIsBroken)
                {
                    conditionResult.NumberResult = 0f;
                }
                else
                {
                    conditionResult.NumberResult = (float)(entityData.Condition ?? 1d);
                }

                // conditionResult.NumberResult = 0f; // TEST!!


                return conditionResult;
            }
            else
            {
                return null;
            }
        }

        public static PropertyResult? GetLocation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetLocation();
        }

        private PropertyResult? GetLocation() // use memory fact location..?
        {
            if (Location.HasValue)
            {
                return new PropertyResult() { LocationResult = Location.Value.ToVector2() };
            }
            else
            {
                return null;
            }
        }

        public Allegiance GetAllegianceOrOwner()
        {
            Allegiance thisAllegiance = null;
            if (AllegianceID.HasValue)
            {
                thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(AllegianceID.Value);
            }
            else
            {
                if (PersonEntity == null && OwnedBy.HasValue)
                {
                    IOwner owner = LookUpOwners.FindByID(OwnedBy);
                    if (owner != null)
                    {
                        thisAllegiance = owner.Allegiance;
                    }
                }
            }

            return thisAllegiance;
        }

        public static PropertyResult? GetAllegiance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetAllegiance();
        }

        private PropertyResult? GetAllegiance()
        {
            if (Intelligence != null && Intelligence.Allegiance != null)
            {
                return new PropertyResult() { StringResult = Intelligence.Allegiance.KeyName };
            }
            return null;
        }

        public static PropertyResult? GetExpedition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetExpedition();
        }

        private PropertyResult? GetExpedition()
        {
            if (Intelligence != null && Intelligence.CurrentExpedition != null)
            {
                return new PropertyResult() { StringResult = Intelligence.CurrentExpedition.KeyName };
            }
            return null;
        }

        public static PropertyResult? GetOwningExpedition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetOwningExpedition();
        }

        private PropertyResult? GetOwningExpedition()
        {
            Expedition expedition = GetOwner();
            if (expedition != null)
            {
                return new PropertyResult() { StringResult = expedition.KeyName };
            }
            /*
            if (PersonEntity == null && OwnedBy.HasValue)
            {
                IOwner owner = LookUpOwners.FindByID(OwnedBy);
                if (owner != null)
                {
                    Expedition expedition = owner as Expedition;
                    if (expedition != null)
                    {
                        return new PropertyResult() { StringResult = expedition.KeyName };
                    }
                }
            }*/

            return null;
        }


        public Expedition GetOwner()
        {
            if (PersonEntity == null && OwnedBy.HasValue)
            {
                IOwner owner = LookUpOwners.FindByID(OwnedBy);
                if (owner != null)
                {
                    return owner as Expedition;                    
                }
            }

            return null;
        }

        public static PropertyResult? GetOwningAllegiance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetOwningAllegiance();
        }

        private PropertyResult? GetOwningAllegiance()
        {
            if (PersonEntity == null && OwnedBy.HasValue)
            {
                IOwner owner = LookUpOwners.FindByID(OwnedBy);
                if (owner != null)
                {
                    return new PropertyResult() { StringResult = owner.Allegiance.KeyName };
                }
            }
            return null;
        }


        public static PropertyResult? GetProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetProgress();
        }

        private PropertyResult? GetProgress()
        {
            if (EntityType.NonLivingType != null)
            {
                if (!IsInStomach())
                {
                    NonLivingEntity nonLiving;
                    Find(out nonLiving);

                    return new PropertyResult() { NumberResult = nonLiving.Progress };
                }
                else return null;
            }
            else return null;
        }

        public static PropertyResult? GetConsumeProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetConsumeProgress();
        }

        private bool IsInStomach()
        {
            Container container;
            if (GetContainedBy(out container))
            {
                 IStorage iStorage = container as IStorage;
                 if (iStorage != null)
                 {
                     Storage storage = iStorage.GetStoredIn(this);

                     if (storage != null 
                         && iStorage.GetCompartment(storage.ID) == StorageCompartment.Stomach)
                     {
                         return true;
                     }                     
                 }
            }

            return false;
        }

        /// <summary>
        /// Only return progress if the produced item is contained in a stomach - this means it is being produced by a Consume Process
        /// </summary>
        /// <returns></returns>
        private PropertyResult? GetConsumeProgress()
        {
            if (EntityType.NonLivingType != null)
            {               
                if (IsInStomach())
                {
                    NonLivingEntity nonLiving;
                    Find(out nonLiving);

                    return new PropertyResult() { NumberResult = nonLiving.Progress };
                }
                else return null;
            }
            else return null;
        }

        public static PropertyResult? GetProfessionIcon(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetProfessionIcon();
        }

        private PropertyResult? GetProfessionIcon()
        {
            if (EntityType.IntelligenceType != null)
            {
                Intelligence intelligence;
                Find(out intelligence);

                if (intelligence.Profession != null)
                {
                    return new PropertyResult()
                    {
                        StringResult = intelligence.Profession.Icon
                        /*
                        MultiResults = new List<string>() 
                        { 
                            intelligence.Profession.Icon, 
                            "Profession: " + intelligence.Profession .Name + " \nThe profession is derived from the best skill the character has."
                        } */
                    };
                }
            }

            return null;
        }


        public float? ComfortLevel
        {
            get
            {
                if (EntityType.ContainerType != null && EntityType.ContainerType.ResidenceType != null)
                {
                    return (float)Residence.GetComfortRating(this);
                }

                return null;
            }
        }

        public static PropertyResult? GetHomeComfortLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            IKnownEntityData entityData = hasExposed as IKnownEntityData;

            return GetHomeComfortLevel(entityData);
        }

        /// <summary>
        /// shared with MemoryFact
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns></returns>
        public static PropertyResult? GetHomeComfortLevel(IKnownEntityData entityData)
        {
            if (entityData.ComfortLevel.HasValue)
            {
                PropertyResult result = new PropertyResult();

                result.NumberResult = entityData.ComfortLevel.Value;

                return result;
            }
            else
            {
                return null;
            }
        }

        public static PropertyResult? GetVehicleType(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            IKnownEntityData entityData = hasExposed as IKnownEntityData;

            return GetVehicleType(entityData);
        }

       
        public static PropertyResult? GetVehicleType(IKnownEntityData entityData)
        {
            if (entityData.EntityType.ContainerType != null)
            {
                PropertyResult result = new PropertyResult();

                VehicleContainerType vehicleContainer = entityData.EntityType.ContainerType as VehicleContainerType;
                if (vehicleContainer != null)
                {
                    result.StringResult = vehicleContainer.VehicleType.ToString();
                }

                return result;
            }
            else
            {
                return null;
            }

        }
        

        public static PropertyResult? GetProfessionDescription(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetProfessionDescription();
        }

        private PropertyResult? GetProfessionDescription()
        {
            if (EntityType.IntelligenceType != null)
            {
                Intelligence intelligence;
                Find(out intelligence);

                if (intelligence.Profession != null)
                {
                    return new PropertyResult()
                    {
                        StringResult = /*"Profession: " +*/ intelligence.Profession.Name /*+ " \nThe profession is derived from the best skill the character has."*/
                    };
                }
            }

            return null;
        }

        public static PropertyResult? GetHungerStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetHungerStatus(getterKnowledge);
        }

        /// <summary>
        /// a data-driven weighted function of all food needs, taking starvation and needs visibility into account.
        /// 0.5: starvation starts - the bar can jump to this value...
        /// The bar should reach 0 when starvation death occurs
        /// </summary>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        private PropertyResult? GetHungerStatus(SharedKnowledge getterKnowledge)
        {
            float? total = null;
            if (EntityType.BiologicalType != null)
            {
                bool knowsAllNeeds = true;
                if (getterKnowledge != null)
                {

                    if (getterKnowledge != Intelligence.Allegiance.SharedKnowledge)
                    {
                        knowsAllNeeds = false;
                    }
                }

                int foodNeeds = 0;
                float needsSum = 0f;
                foreach (var need in BiologicalEntity.Needs.NeedsList)
                {
                    if (!knowsAllNeeds)
                    {
                        // exclude the levels which are not visible...
                        if (!need.Value.NeedType.LevelVisibleToOtherAllegiances)
                            continue;
                    }

                    if (need.Value.NeedType.FoodNeedType != null
                        && need.Value.NeedType.FoodNeedType.IsEssential == true // excludes stimulants...
                        )
                    {
                        if (need.Value.PhysicalNeed != null && need.Value.PhysicalNeed.DaysAtZero > 0f
                            && need.Value.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
                        {
                            // if starving for at least one need, set to gravest starvation level
                            float currentTotal = total ?? 1f;
                            float starvationFraction = need.Value.PhysicalNeed.GetStarvedToDeathFraction();
                            total = Math.Min(currentTotal, 0.5f * starvationFraction); // let starvation start at 0.5
                        }
                        else
                        {
                            // else compute an average.
                            needsSum += need.Value.CurrentLevel; //.GetWeightedStatus();
                        }

                        foodNeeds++;
                    }
                }

                if (total == null)
                {
                    if (foodNeeds > 0)
                    {
                        total = needsSum / foodNeeds;
                    }
                    else
                    {
                        total = needsSum;
                    }

                    total = 0.5f * total + 0.5f; // scale and shift
                }

                PropertyResult hungerResult = new PropertyResult()
                {
                    NumberResult = total.Value
                };

                return hungerResult;

            }
            return null;
        }

        /// <summary>
        /// do we really have to hardcode these methods???
        /// </summary>
        /// <param name="hasExposed"></param>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        public static PropertyResult? GetProteinLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetProteinLevel(getterKnowledge);
        }

        public static PropertyResult? GetMicronutrientsLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetMicronutrientsLevel(getterKnowledge);
        }

        public static PropertyResult? GetFoodEnergyLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetFoodEnergyLevel(getterKnowledge);
        }

        public static PropertyResult? GetStimulantsLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetStimulantsLevel(getterKnowledge);
        }

        private PropertyResult? GetFoodEnergyLevel(SharedKnowledge getterKnowledge)
        {
            return GetNeedLevelResult("foodEnergy", getterKnowledge);

            /* PropertyResult? foodEnergyResult = GetNeedLevelResult("foodEnergy", getterKnowledge);
             if (foodEnergyResult.HasValue)
             {
                 Need foodEnergyNeed = BiologicalEntity.Needs.NeedsList["foodEnergy"];
                 float foodEnergyStarvedToDeathFraction = foodEnergyNeed.PhysicalNeed.GetCollapsedFraction();

                 float result = foodEnergyResult.Value.NumberResult.Value * 0.5f + foodEnergyStarvedToDeathFraction * 0.5f;
                 PropertyResult newResult = new PropertyResult();
                 newResult.NumberResult = result;
                 foodEnergyResult = newResult;
                 return foodEnergyResult;
             }
             else
             {
                 return null;
             }*/
        }

        public static PropertyResult? GetFoodEnergyStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetFoodEnergyStatus(getterKnowledge);
        }
        private PropertyResult? GetFoodEnergyStatus(SharedKnowledge getterKnowledge)
        {
            return GetNeedStatusResult("foodEnergy", getterKnowledge);
        }

        private PropertyResult? GetProteinLevel(SharedKnowledge getterKnowledge)
        {
            return GetNeedLevelResult("protein", getterKnowledge);
        }
        private PropertyResult? GetMicronutrientsLevel(SharedKnowledge getterKnowledge)
        {
            return GetNeedLevelResult("micronutrients", getterKnowledge);
        }
        private PropertyResult? GetStimulantsLevel(SharedKnowledge getterKnowledge)
        {
            return GetNeedLevelResult("stimulants", getterKnowledge);
        }

        public static PropertyResult? GetSleepLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetSleepLevel(getterKnowledge);
        }
        private PropertyResult? GetSleepLevel(SharedKnowledge getterKnowledge)
        {
            return GetNeedLevelResult("sleep", getterKnowledge);
        }

        public static PropertyResult? GetSleepStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetSleepStatus(getterKnowledge);
        }
        private PropertyResult? GetSleepStatus(SharedKnowledge getterKnowledge)
        {
            return GetNeedStatusResult("sleep", getterKnowledge);
        }


        /// <summary>
        /// only returns the current level, unaffected by starvation
        /// </summary>
        /// <param name="needToGetResultFrom"></param>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        private PropertyResult? GetNeedLevelResult(string needToGetResultFrom, SharedKnowledge getterKnowledge)
        {
            Need need = GetNeed(needToGetResultFrom);
            if (need == null)
            {
                return null;
            }
            if (getterKnowledge != null)
            {
                if (getterKnowledge.Allegiance != Intelligence.Allegiance)
                {
                    if (need.NeedType.LevelVisibleToOtherAllegiances == false)
                    {
                        return null;
                    }
                }
            }

            PropertyResult needLevel = new PropertyResult();
            needLevel.NumberResult = need.CurrentLevel;

            return needLevel;
        }


        /// <summary>
        /// returns a number from 0 - 1 by combining current level and starvation, scaled in the following way:
        /// 1: full
        /// 0.5: empty, but 0 starvation
        /// 0: starved near death
        /// </summary>
        /// <param name="needToGetResultFrom"></param>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        private PropertyResult? GetNeedStatusResult(string needToGetResultFrom, SharedKnowledge getterKnowledge)
        {
            Need need = GetNeed(needToGetResultFrom);
            if (need == null)
            {
                return null;
            }
            if (getterKnowledge != null)
            {
                if (getterKnowledge.Allegiance != Intelligence.Allegiance)
                {
                    if (need.NeedType.LevelVisibleToOtherAllegiances == false)
                    {
                        return null;
                    }
                }
            }

            PropertyResult needStatus = new PropertyResult();
            needStatus.NumberResult = need.GetWeightedStatus();

            return needStatus;
        }



        private Need GetNeed(string needKey)
        {
            if (EntityType.BiologicalType != null)
            {
                Need need;
                if (BiologicalEntity.Needs.NeedsList.TryGetValue(needKey, out need))
                {
                    return need;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static PropertyResult? GetHitpointsFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return GetHitpointsFractionValue(((Entity)hasExposed).Body);
        }

        public static PropertyResult? GetHitpointsFractionValue(Body.Body body)
        {
            if (body != null /*BiologicalEntity != null*/)
            {
                PropertyResult hitpointsResult = new PropertyResult();
                hitpointsResult.NumberResult = body.GetModifiedHitpointsForPresentation(); //Body.GlobalHitpoints / Body.MaxHitpoints;


                return hitpointsResult;
            }
            else
            {
                return null;
            }
        }


        public static PropertyResult? GetAmmoStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            // Test
            /*  PropertyResult ammoResult = new PropertyResult();
              ammoResult.NumberResult = 0.2f;
              return ammoResult;*/

            return ((Entity)hasExposed).GetAmmoStatus(getterKnowledge);
        }

        private PropertyResult? GetAmmoStatus(SharedKnowledge getterKnowledge)
        {

            int? noOfRounds = null;

            if (!GetAmmoRoundsLeft(getterKnowledge, ref noOfRounds, false))
                return null;

            if (noOfRounds.HasValue)
            {
                // uses ammo
                PropertyResult ammoResult = new PropertyResult();

                if (EntityType.ItemType != null && EntityType.ItemType.WeaponType != null
                    && EntityType.ItemType.WeaponType.IsIntrinsic == true)
                {
                    // for loose weapon parts, don't display out of ammo:
                    ammoResult.NumberResult = 1f;
                }
                else if (noOfRounds.Value == 0)
                {
                    ammoResult.NumberResult = 0.2f;
                    // TODO: check ammo inventory status also... use code from JobsPanel..? set a flag from evaluators?
                }
                else
                {
                    ammoResult.NumberResult = 1f;
                }

                return ammoResult;
            }

            return null; // doesn't use ammo
        }

        public static PropertyResult? GetAmmoRoundsLeft(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetAmmoRoundsLeft(getterKnowledge);
        }

        private PropertyResult? GetAmmoRoundsLeft(SharedKnowledge getterKnowledge)
        {
            int? noOfRounds = null;

            if (!GetAmmoRoundsLeft(getterKnowledge, ref noOfRounds))
                return null;


            if (noOfRounds.HasValue)
            {
                PropertyResult ammoResult = new PropertyResult();
                ammoResult.StringResult = noOfRounds.Value.ToString();

                return ammoResult;
            }

            return null;
        }

        private bool GetAmmoRoundsLeft(SharedKnowledge getterKnowledge, ref int? noOfRounds, bool checkAmmoItems = true)
        {
            if (Item != null)
            {
                if (getterKnowledge != null)
                {
                    if (AllegianceID.HasValue == true)
                    {
                        if (AllegianceID != getterKnowledge.Allegiance.ID)
                        {
                            return false;
                            // return null;
                        }
                    }
                }

                if (Item.Ammunition != null && checkAmmoItems)
                {
                    // ammo clips:            
                    noOfRounds = Item.Ammunition.NoOfRounds;
                }
                else //if (EntityType.ContainerType != null && EntityType.ContainerType.MagazineContainerType != null)
                {
                    // weapons:
                    noOfRounds = GetWeaponRounds(this);
                }

            }
            else if (EntityType.IntelligenceType != null && EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                // robots (sentry):
                int maxRounds = -1;

                foreach (var item in IntrinsicWeapons)
                {
                    Entity weapon = Entity.FindByID(item.Value);
                    if (weapon != null)
                    {
                        noOfRounds = GetWeaponRounds(weapon);

                        if (noOfRounds.HasValue)
                        {
                            maxRounds = Common.Max(maxRounds, noOfRounds.Value);
                        }
                    }
                }

                if (maxRounds > -1)
                {
                    noOfRounds = maxRounds;
                }
            }

            return true;
        }

        private static int? GetWeaponRounds(Entity weapon)
        {
            int? noOfRounds = null;
            // weapons:
            if (weapon.EntityType.ContainerType != null && weapon.EntityType.ContainerType is MagazineContainerType)
            {
                MagazineContainer magazine = weapon.Contains as MagazineContainer;
                noOfRounds = magazine.GetTotalAmmo();
            }

            return noOfRounds;
        }

        public static PropertyResult? IsAgent(IHasExposedProperties hasProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {

            PropertyResult IsAgentResult = new PropertyResult();
            IsAgentResult.BoolResult = ((Entity)hasProperties).EntityType.IntelligenceType != null;
            return IsAgentResult;

        }

        // Productivity

        public static PropertyResult? GetTotalProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetTotalProductivity(getterKnowledge);
        }

        private PropertyResult? GetTotalProductivity(SharedKnowledge getterKnowledge)
        {
            Intelligence intelligence;
            if (Find(out intelligence)
                && intelligence.Brain != null)
            {
                if (getterKnowledge != null)
                {
                    if (getterKnowledge.Allegiance != intelligence.Allegiance)
                    {
                        return null;
                    }
                }

                float? currentTotalProductivity = intelligence.Brain.GetCurrentTotalProductivity();
                if (currentTotalProductivity.HasValue)
                {
                    PropertyResult result = new PropertyResult() { NumberResult = currentTotalProductivity };

                    return result;
                }
            }
            return null;
        }

        private static PropertyResult? GetCurrentSkillProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Entity)hasExposed).GetCurrentSkillProductivity(getterKnowledge);
        }

        private PropertyResult? GetCurrentSkillProductivity(SharedKnowledge getterKnowledge)
        {
            Intelligence intelligence;
            if (Find(out intelligence)
                && intelligence.Brain != null)
            {
                if (getterKnowledge != null)
                {
                    if (!RatingsAreVisible(getterKnowledge)) //AllegianceID.HasValue == true)
                    {
                        return null;
                    }/*

                    if (getterKnowledge.Allegiance != intelligence.Allegiance)
                    {
                        return null;
                    }*/
                }
                float? currentSkillProductivity = intelligence.Brain.GetSkillProductivity();
                if (currentSkillProductivity.HasValue)
                {
                    PropertyResult result = new PropertyResult() { NumberResult = currentSkillProductivity };

                    return result;
                }
            }
            return null;
        }

        private static PropertyResult? GetCurrentToolProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {

            return ((Entity)hasExposed).GetCurrentToolProductivity(getterKnowledge);
        }

        private PropertyResult? GetCurrentToolProductivity(SharedKnowledge getterKnowledge)
        {
            Intelligence intelligence;
            if (Find(out intelligence)
                && intelligence.Brain != null)
            {
                if (getterKnowledge != null)
                {
                    if (getterKnowledge.Allegiance != intelligence.Allegiance)
                    {
                        return null;
                    }
                }

                float? currentToolProductivity = intelligence.Brain.GetToolProductivity();
                if (currentToolProductivity.HasValue)
                {
                    PropertyResult result = new PropertyResult() { NumberResult = currentToolProductivity };

                    return result;
                }
            }
            return null;
        }

        #endregion

        public Dictionary<string, PropertyResult> CustomFields;


        #region CompositeID ILookup

        // this is the second ID an Entity has - for when it is referenced as an IComposite.

        CompositeID compositeID;
        CompositeID ILookUp<IComposite, CompositeID>.ID
        {
            get
            {
                return compositeID;
            }
        }

        CompositeID ILookUp<IComposite, CompositeID>.GetUniqueID()
        {
            return Composite.GetUniqueID();
        }


        void ILookUp<IComposite, CompositeID>.AddToLookup()
        {
            compositeID = ((ILookUp<IComposite, CompositeID>)this).GetUniqueID();

            if (compositeID != CompositeID.Invalid)
            {
                LookUpIComposites.Add(compositeID, this); // uses special class!
            }
        }

        void ILookUp<IComposite, CompositeID>.RemoveIDEntry()
        {
            LookUpIComposites.Remove(this);  // uses special class!
        }

        void ILookUp<IComposite, CompositeID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IComposite, CompositeID>.SetInvalid()
        {
            compositeID = CompositeID.Invalid;
        }

        void ILookUp<IComposite, CompositeID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        #endregion


        #region DetectableID ILookup

        // this is the third ID an Entity has - for when it is referenced as an IDetectable.

        DetectableID detectableID;
        DetectableID ILookUp<IDetectable, DetectableID>.ID
        {
            get
            {
                return detectableID;
            }
        }

        DetectableID ILookUp<IDetectable, DetectableID>.GetUniqueID()
        {
            return Detectable.GetUniqueID();
        }


        void ILookUp<IDetectable, DetectableID>.AddToLookup()
        {
            detectableID = ((ILookUp<IDetectable, DetectableID>)this).GetUniqueID();

            if (detectableID != DetectableID.Invalid)
            {
                LookUpIDetectables.Add(detectableID, this); // uses special class!
            }
        }

        void ILookUp<IDetectable, DetectableID>.RemoveIDEntry()
        {
            LookUpIDetectables.Remove(this);  // uses special class!
        }

        void ILookUp<IDetectable, DetectableID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IDetectable, DetectableID>.SetInvalid()
        {
            detectableID = DetectableID.Invalid;
        }

        void ILookUp<IDetectable, DetectableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }
              

        #endregion

        public void IterateMembers(Action<Entity> iterateFunction)
        {
            iterateFunction(this);
        }

        public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
        {
            if (PersonEntity != null)
            {
                iterateFunction(PersonEntity.OwnedEntities);
            }
        }

        Allegiance ICanIterateEntities.GetAllegiance
        {
            get
            {
                return Intelligence.Allegiance;
            }
        }

        public void RecomputeUpdateInterval()
        {
            bool intervalChanged;
            RecomputeUpdateInterval(out intervalChanged);
        }

        /// <summary>
        /// Remember to keep this up-to-date when more functionality is added to the Update method!
        /// Otherwise performance will suffer because of unnecessary updates, or the object may not receive any Update calls when it needs it.
        /// 
        /// Call RecomputeUpdateInterval explicitly!
        /// </summary>
        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
            intervalChanged = false;

            double? tempInterval = null, currentInterval = null;

            if (!IsDead
                && ID != EntityID.Invalid)
            {
                // loop over the components, ask each for their update interval
                foreach (var item in Components)
                {
                    tempInterval = item.Value.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                }

                if (IsOnPlaySite())
                {
                    if (!IsDead
                       && IsCompleted())
                    {

                        if (EntityType.ContainerType != null)
                        {
                            // not a component
                            tempInterval = Contains.GetUpdateInterval();
                            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                        }

                        if (EntityType.BiologicalType != null)
                        {
                            // Entity manages this update because of dependencies:
                            tempInterval = GameData.Instance.Constants.UpdateIntervalForBioEntity;
                            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                        }
                    }


                    if (footprintIsDirty && GeometryLayout != null
                        && (Structure == null || Structure.ConstructionHasStarted()))
                    {
                        // ugh..
                        // should only fire once...?
                        tempInterval = 0;
                        UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                    }
                }

            }

            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }
        }

        public virtual void TurnOnDesiredShareOfLights(float fractionToTurnOn)
        {
            SetSpriteStateFlag(StateModifier.LightIsOn);

           /* if (Lighting != null)
                Lighting.TurnOnDesiredShareOfLights(fractionToTurnOn);
                */
        }

        public virtual void TurnOffTheLight()
        {
            ClearSpriteStateFlag(StateModifier.LightIsOn);
           /* if (Lighting != null)
                Lighting.TurnOffTheLight();*/
        }

        #region ISleepingUpdatable

        private double? timePointInSeconds;
        public double? TimePointInSeconds
        {
            get
            {
                return timePointInSeconds;
            }
        }

        public void SetNextTimepoint(double? timepoint)
        {
            // gets called every frame on agents, much more rarely on trees.

            this.timePointInSeconds = timepoint;
        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<Entity>.Create();
        }

        private SleepyUpdaterID sleepyUpdater;
        public SleepyUpdaterID SleepyUpdater
        {
            get
            {
                return sleepyUpdater;
            }
            set
            {
                sleepyUpdater = value;
            }
        }

        private double? updateInterval;
        public double? UpdateInterval
        {
            get
            {
                return updateInterval;
            }

            private set
            {
                if (!Common.IsEqual(updateInterval, value))
                {
                    updateInterval = value;

                    SleepyUpdater<Entity> updater = LookUpSleepyUpdater<Entity>.FindByID(SleepyUpdater);
                    if (updater != null) // is null after Load, but not after Save+Load???
                    {
                        updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
                    }

                }
            }

        }

        #endregion

        #region CanIterateEntitiesID ILookup

        public CanIterateEntitiesID CanIterateEntitiesID;
        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID
        {
            get
            {
                return CanIterateEntitiesID;
            }
        }

        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
        {
            return HasMembers.GetUniqueID();
        }


        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
        {
            CanIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();

            if (CanIterateEntitiesID != CanIterateEntitiesID.Invalid)
            {
                LookUpICanIterateEntities.Add(CanIterateEntitiesID, this); // uses special class!
            }
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
        {
            LookUpICanIterateEntities.Remove(this);  // uses special class!
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
        {
            CanIterateEntitiesID = CanIterateEntitiesID.Invalid;
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        #endregion


    }

   
}

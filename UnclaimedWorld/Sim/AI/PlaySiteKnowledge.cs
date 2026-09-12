using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI
{
    /// <summary>
    /// Only playsite allegiances should use this class!
    /// 
    /// Should only contain Playsite entities!
    /// </summary>
    public class PlaySiteKnowledge : ISnapshot, IIDEventSubscriber
    {

        public SharedKnowledge Parent;


        /// <summary>
        /// needed??? for explosions, gun shots? scare moves?
        /// 
        /// not currently snapshotted!!
        /// </summary>
        public List<ThreatSource> TransientThreats = new List<ThreatSource>();

        public Dictionary<EntityType, ResourceMap> ResourceMaps = new Dictionary<EntityType, ResourceMap>();
        Dictionary<EntityType, CyclableID> snapshotResourceMaps;

        //   public Dictionary<EntityType, List<Tuple<EntityID, ResourceContainer>>> ResourceHarvesters = new Dictionary<EntityType, List<Tuple<EntityID, ResourceContainer>>>();
        public Dictionary<EntityType, List<Tuple<EntityID, ResourceID>>> ResourceHarvesters = new Dictionary<EntityType, List<Tuple<EntityID, ResourceID>>>();

        public Dictionary<SimProcessID, ProcessMemory> ProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemory>();
        private Dictionary<SimProcessID, ProcessMemoryID> snapshotProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemoryID>();


        public Dictionary<ResourceType, HashSet<ResourceID>> AllKnownResourceContainers = new Dictionary<ResourceType, HashSet<ResourceID>>();

        /// <summary>      
        /// TODO: make it a hashset
        /// 
        /// here we track (only intelligent, for now) entities belonging to different allegiances that we know about, which are on the playsite.
        /// 
        /// Agents moving to the playsite will require special handling if we have already detected them off-map!! Also agents that change allegiance!
        /// 
        /// Note: some structures like terminals and sensors are also considered agents...
        /// </summary>
        public Dictionary<EntityID, EntityID> AllKnownOutsideAgentsOnPlaySite = new Dictionary<EntityID, EntityID>();


        /// <summary>
        ///  TODO: make it a hashset
        ///  
        /// Twinkler nests are in this list - as well as agents and all other threatening entities which are not agents...
        /// </summary>
        public Dictionary<EntityID, EntityID> AllKnownThreatSources = new Dictionary<EntityID, EntityID>();

        private const int maxKnownEntityDatasPerNode = 10;

        /// <summary>
        /// A quad tree keeping track of all known entity data that the allegiance can see. 
        /// Only contains entities filtered by UseMemory, that means no rocks or trees! To get those, use the shared quad tree instead.
        /// 
        /// (should it contain contained entities? what about parts?)
        /// </summary>
        public PointQuadTree<EntityID> KnownEntityDataTree
        {
            get
            {              
                return knownEntityDataTree;
            }
        }

        private PointQuadTree<EntityID> knownEntityDataTree;
        private List<Pair<EntityID, Vector2>> snapshotKnownEntityDataTree;

        /// <summary>
        /// prey that has ever been spotted
        /// </summary>
        public HashSet<EntityType> SpottedPrey = new HashSet<EntityType>();

        /// <summary>
        /// animals that have ever been spotted
        /// </summary>
        public HashSet<EntityType> SpottedAnimals = new HashSet<EntityType>();


        Regulator assertRegulator;

        #region Events


        // our clients/subscribers:
        /// <summary>
        /// is invoked once it is known that the process has been destroyed
        /// is invoked right after ID is set invalid and the process removed
        /// </summary>
        Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessDestroyedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

        /// <summary>
        /// is invoked once it is known that the process has completed - before the process is destroyed
        /// </summary>
        Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessCompletedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

        Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessStartedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();
        Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessProducingEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

       // Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessInputDestroyedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();


        MethodID processCompletedMethodID, processDestroyedMethodID, processStartedMethodID, processProducingMethodID; //, processInputDestroyedMethodID;

        #endregion


        #region Auxiliary Maps

        /// <summary>
        /// it would be better to offer different move maps for the different entity types that can belong to the same allegiance. Such as dog, livestock, robot, human.
        /// Instead of only having one for the representative type...
        /// The entity types will have different strength levels and threats too... but there is always one set of threat(jobs) that can threaten at least one in the allegiance
        /// (similar to the food type sets)
        /// </summary>
        public Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>> ThreatMaps;
        private Dictionary<EntityType, Dictionary<ThreatStance, IMapID>> snapshotThreatMaps;

        public Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>> DiscomfortMaps;
        private Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>> snapshotAllDiscomfortMaps;

        public Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> AllMovementMaps;
        private Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>> snapshotAllMovementMaps;


        #endregion

        public PlaySiteKnowledge()
        {

        }

        public PlaySiteKnowledge(SharedKnowledge parent)
        {
            this.Parent = parent;

            processDestroyedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessDestroyed);
            processCompletedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessCompleted);
            processStartedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessStarted);
            processProducingMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessProducing);
           // processInputDestroyedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessInputDestroyed);

            
            knownEntityDataTree = new PointQuadTree<EntityID>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxKnownEntityDatasPerNode, 7);
            
            // ??
            // let's skip this step while we are placing entities in the editor, or when we are testing animations:
            // TODO: init these maps when an entity switches site from Othersite to Playsite...
            /* if (computeAuxiliaryMaps) // !allegiance.MembersHaveAIDisabled())
             {
                 if (The.Map != null && The.Map.mapTileWidth > 0)
                 {
                     InitAuxiliaryMaps();
                 }
             }*/

            CreateRegulators();


        }


        public void AddOrUpdateKnownEntityLocation(Entity entity, Vector2 location)
        {
            if (entity.EntityType.TreeType != null)
            {

            }

            EntityID entityID = entity.ID;

            if (IsOutsiderAgentOnPlaySite(entity))
            {
                if (!AllKnownOutsideAgentsOnPlaySite.ContainsKey(entityID))
                {
                    AllKnownOutsideAgentsOnPlaySite.Add(entityID, entityID);
                }
            }
            else if (IsOutsideThreat(entity))
            {
                if (!AllKnownThreatSources.ContainsKey(entityID))
                {
                    AllKnownThreatSources.Add(entityID, entityID);
                }
            }

            if (KnownEntityDataTree.Contains(entityID))
            {
                KnownEntityDataTree.UpdateObject(entityID, location);
            }
            else
            {
                if (entity.ToString().Contains("Mudbrick"))
                {

                }

                KnownEntityDataTree.AddObject(entityID, location);
            }

        }



        /// <summary>
        /// potential threat?
        /// </summary>
        /// <returns></returns>
        private bool IsOutsiderAgentOnPlaySite(Entity entity)
        {
            if (entity.EntityType.IntelligenceType != null
                && entity.Location.HasValue)
            {
                AllegianceID? entitysAllegianceId = entity.AllegianceID;

                if (entitysAllegianceId != null)
                {
                    if (entitysAllegianceId != Parent.Allegiance.ID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CreateRegulators()
        {
            assertRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "PlaySiteKnowledge");

        }


        private bool IsOutsideThreat(Entity entity)
        {
            if (entity.EntityType.ThreatType != null
                && entity.EntityType.IntelligenceType == null)
            {
                ThreatGroup entitysThreatGroup = entity.ThreatGroup;

                if (entitysThreatGroup != null)
                {
                    if (entitysThreatGroup != Parent.Allegiance.ThreatGroup)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool EntityTypeRequiresAuxiliaryMaps(EntityType entityType)
        {
            return entityType.IntelligenceType != null && entityType.IntelligenceType.IsMobile; // also mobile
        }

        /// <summary>
        /// Init discomfort maps and so on... stuff that needs the dimensions of the map.
        /// </summary>
        public void InitAuxiliaryMaps()
        {
            if (AllMovementMaps != null)
                return; // makes sure we don't init more than once...


            ThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>>();
            DiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>>();

            // is it overkill to have 3 transport types also...?
            AllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>>();


            // build the map hierarchies
            AddAuxiliaryMaps(Parent.Allegiance.RepresentativeEntityType);


            foreach (var item in Parent.Allegiance.MembersList)
            {
                if (EntityTypeRequiresAuxiliaryMaps(item.EntityType)
                    && !ThreatMaps.ContainsKey(item.EntityType))
                {
                    AddAuxiliaryMaps(item.EntityType);
                }
            }

            //HighResolutionTime.Start();

            // we need to initialize all the maps to start with:
            // this takes 1 second         
            /*    foreach (KeyValuePair<ProtectionLevel, Dictionary<ThreatCategory, Dictionary<ThreatStance, MovementMap>>> kvp1 in AllMovementMaps)
                {
                    foreach (KeyValuePair<ThreatCategory, Dictionary<ThreatStance, MovementMap>> kvp2 in kvp1.Value)
                    {
                        foreach (KeyValuePair<ThreatStance, MovementMap> kvp3 in kvp2.Value)
                        {
                            kvp3.Value.ComputeAll = true;
                            while (kvp3.Value.CycleOnce() == false) ;
                        }
                    }
                }*/

            // double timeTaken = HighResolutionTime.GetTime();

            /*    // now let's do all region maps:
              // on my pc (lars) it takes 4 s per region map...
              foreach (KeyValuePair<ProtectionLevel, Dictionary<ThreatCategory, Dictionary<EntityApproach, MovementMap>>> kvp1 in AllMovementMaps)
              {
                  foreach (KeyValuePair<ThreatCategory, Dictionary<EntityApproach, MovementMap>> kvp2 in kvp1.Value)
                  {
                      foreach (KeyValuePair<EntityApproach, MovementMap> kvp3 in kvp2.Value)
                      {
                          foreach (KeyValuePair<TerrainType.TransportType, byte[][]> kvp3t in kvp3.Value.Map)
                          {                            
                              while (kvp3.Value.RegionMap[kvp3t.Key].CycleOnce() == false);
                          }                      
                      }
                  }
              }

              timeTaken = HighResolutionTime.GetTime();
             */

        }

        private void RemoveAuxiliaryMaps(EntityType entityType)
        {
            Dictionary<ThreatStance, ThreatMap> threatMaps;
            if (!ThreatMaps.TryGetValue(entityType, out threatMaps))
            {
                return;
            }

          //  var threatMaps = ThreatMaps[entityType];
            foreach (var item in threatMaps)
            {
                item.Value.Destroy();
            }

            ThreatMaps.Remove(entityType);

            // var dMaps = DiscomfortMaps[ProtectionLevel.Exposed];
            foreach (var item in DiscomfortMaps)
            {
                var dMaps = item.Value[entityType];

                foreach (var dmap in dMaps)
                {
                    dmap.Value.Destroy();
                }

                item.Value.Remove(entityType);
            }

            foreach (var item in AllMovementMaps)
            {
                var moveMaps = item.Value[entityType];

                foreach (var moveMap in moveMaps)
                {
                    moveMap.Value.Destroy();
                }

                item.Value.Remove(entityType);
            }


        }

        private void AddAuxiliaryMaps(EntityType entityType)
        {
            int mapWidth, mapHeight;
            MapManager map = The.Map;

            mapWidth = map.mapTileWidth;
            mapHeight = map.mapTileHeight;

            ThreatMap normalThreatMap = new ThreatMap(Parent, mapWidth, mapHeight, entityType, ThreatStance.Normal) { IDName = entityType.KeyName + "Normal" };
            ThreatMap boldThreatMap = new ThreatMap(Parent, mapWidth, mapHeight, entityType, ThreatStance.Bold) { IDName = entityType.KeyName + "Bold" };
            ThreatMap cautiousThreatMap = new ThreatMap(Parent, mapWidth, mapHeight, entityType, ThreatStance.Cautious) { IDName = entityType.KeyName + "Cautious" };

            AddThreatMap(normalThreatMap);
            AddThreatMap(boldThreatMap);
            AddThreatMap(cautiousThreatMap);

            // for now, we don't use protected/xposed state maps...         
            DiscomfortMap exposedNormalApproachDiscomfortMap = new DiscomfortMap(ProtectionLevel.Exposed, normalThreatMap) { IDName = "Exposed" + normalThreatMap.IDName };
            AddDiscomfortMap(exposedNormalApproachDiscomfortMap, normalThreatMap);
            DiscomfortMap exposedBoldApproachDiscomfortMap = new DiscomfortMap(ProtectionLevel.Exposed, boldThreatMap) { IDName = "Exposed" + boldThreatMap.IDName };
            AddDiscomfortMap(exposedBoldApproachDiscomfortMap, boldThreatMap);
            DiscomfortMap exposedCautiousApproachDiscomfortMap = new DiscomfortMap(ProtectionLevel.Exposed, cautiousThreatMap) { IDName = "Exposed" + cautiousThreatMap.IDName };
            AddDiscomfortMap(exposedCautiousApproachDiscomfortMap, cautiousThreatMap);


            if (Parent.Allegiance.AllegianceType == AllegianceType.Player)
            {
                MovementMap moveNormalMap = new MovementMap(0.2f, exposedNormalApproachDiscomfortMap, normalThreatMap, exposedNormalApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
                AddMovementMap(moveNormalMap, exposedNormalApproachDiscomfortMap, normalThreatMap);

                MovementMap moveBoldMap = new MovementMap(0.2f, exposedBoldApproachDiscomfortMap, boldThreatMap, exposedBoldApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
                AddMovementMap(moveBoldMap, exposedBoldApproachDiscomfortMap, boldThreatMap);

                MovementMap moveCautiousMap = new MovementMap(0.2f, exposedCautiousApproachDiscomfortMap, cautiousThreatMap, exposedCautiousApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
                AddMovementMap(moveCautiousMap, exposedCautiousApproachDiscomfortMap, cautiousThreatMap);

            }
            else
            {
                MovementMap moveNormalMap = new MovementMap(0.2f, exposedNormalApproachDiscomfortMap, normalThreatMap, exposedNormalApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, SurfaceType.TransportType.Foot);
                AddMovementMap(moveNormalMap, exposedNormalApproachDiscomfortMap, normalThreatMap);

                MovementMap moveBoldMap = new MovementMap(0.2f, exposedBoldApproachDiscomfortMap, boldThreatMap, exposedBoldApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, SurfaceType.TransportType.Foot);
                AddMovementMap(moveBoldMap, exposedBoldApproachDiscomfortMap, boldThreatMap);

                MovementMap moveCautiousMap = new MovementMap(0.2f, exposedCautiousApproachDiscomfortMap, cautiousThreatMap, exposedCautiousApproachDiscomfortMap.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, SurfaceType.TransportType.Foot);
                AddMovementMap(moveCautiousMap, exposedCautiousApproachDiscomfortMap, cautiousThreatMap);

            }
        }

        public MovementMap GetMovementMap(Entity entity, ThreatStance? threatStance = null) // ProtectionLevel p, ThreatCategory c, EntityApproach a)
        {           
            // for the DEMO, we only use the Exposed protection level (it has performance implications to add more maps):
            ProtectionLevel p = ProtectionLevel.Exposed;
            // ProtectionLevel p = entity.Intelligence.ProtectionLevel;

            if (!threatStance.HasValue)
            {
                threatStance = entity.Intelligence.ThreatStance;
            }

            return (MovementMap)AllMovementMaps[p][entity.EntityType][threatStance.Value].GetCurrent();
        }

        public ThreatMap GetThreatMap(EntityType c, ThreatStance a)
        {
            return (ThreatMap)ThreatMaps[c][a].GetCurrent();
        }

        public DiscomfortMap GetDiscomfortMap(ProtectionLevel p, EntityType c, ThreatStance a)
        {
            // for the DEMO, we only use the Exposed protection level (it has performance implications to add more maps):
            p = ProtectionLevel.Exposed;

            return (DiscomfortMap)DiscomfortMaps[p][c][a].GetCurrent();
        }

        public MovementMap GetMovementMap(ProtectionLevel p, EntityType c, ThreatStance a)
        {           
            // for the DEMO, we only use the Exposed protection level (it has performance implications to add more maps):
            p = ProtectionLevel.Exposed;

            return (MovementMap)AllMovementMaps[p][c][a].GetCurrent();
        }

        private void AddMovementMap(MovementMap map, DiscomfortMap childDMap, ThreatMap childThreatMap)
        {
            if (!AllMovementMaps.ContainsKey(childDMap.ProtectionLevel))
            {
                AllMovementMaps.Add(childDMap.ProtectionLevel, new Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>());
            }
            if (!AllMovementMaps[childDMap.ProtectionLevel].ContainsKey(childThreatMap.EntityType))
            {
                AllMovementMaps[childDMap.ProtectionLevel].Add(childThreatMap.EntityType, new Dictionary<ThreatStance, MovementMap>());
            }
            AllMovementMaps[childDMap.ProtectionLevel][childThreatMap.EntityType].Add(childThreatMap.Approach, map);

        }

        private void AddThreatMap(ThreatMap threatMap)
        {
            if (!ThreatMaps.ContainsKey(threatMap.EntityType))
            {
                ThreatMaps.Add(threatMap.EntityType, new Dictionary<ThreatStance, ThreatMap>());
            }

            ThreatMaps[threatMap.EntityType].Add(threatMap.Approach, threatMap);
        }

        private void AddDiscomfortMap(DiscomfortMap dMap, ThreatMap childThreatMap)
        {
            if (!DiscomfortMaps.ContainsKey(dMap.ProtectionLevel))
            {
                DiscomfortMaps.Add(dMap.ProtectionLevel, new Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>());
            }
            if (!DiscomfortMaps[dMap.ProtectionLevel].ContainsKey(childThreatMap.EntityType))
            {
                DiscomfortMaps[dMap.ProtectionLevel].Add(childThreatMap.EntityType, new Dictionary<ThreatStance, DiscomfortMap>());
            }
            DiscomfortMaps[dMap.ProtectionLevel][childThreatMap.EntityType].Add(childThreatMap.Approach, dMap);

        }

        /// <summary>
        /// crate a new set of maps if needed for the new member type -only for movers?
        /// </summary>
        /// <param name="newMember"></param>
        public void AddMember(Entity newMember)
        {
            if (ThreatMaps != null // is null during game init...
                && EntityTypeRequiresAuxiliaryMaps(newMember.EntityType)
                && !ThreatMaps.ContainsKey(newMember.EntityType))
            {
                AddAuxiliaryMaps(newMember.EntityType);
            }
        }

        public void RemoveMember(Entity memberToRemove)
        {
            if (memberToRemove.EntityType != Parent.Allegiance.RepresentativeEntityType)
            {
                if (!Parent.Allegiance.MembersList.Exists(m => m.EntityType == memberToRemove.EntityType))
                {
                    RemoveAuxiliaryMaps(memberToRemove.EntityType);
                }

            }
        }


        public void AssertSeenEntitiesOnPlaySiteNotInFOW()
        {
            return;
         
            var objects = KnownEntityDataTree.GetAllObjectsAndPositions();
            foreach (var item in objects)
            {
                ValidateSeenEntityNotInFOW(item.First);         
            }
        }

        /// <summary>
        /// a person ID stored in AllKnownOutsideAgentsOnPlaySite but without a corresponding memory fact even though the person was in FOW for the critter allegiance,
        /// caused a crash after emigrating when the entity.Location was accessed in ThreatJobManager.
        /// </summary>
        /// <param name="item"></param>
        private void ValidateSeenEntityNotInFOW(EntityID item)
        {

            return;

#if DEBUG || PROFILE
            IKnownEntityData entityData;


            if (item == (EntityID)4303)
            {

            }

            if (Parent.GetKnownData(item, out entityData) == EntityResult.SeenDirectly)
            {
               
                // assert that we can also see the tile:
                TerrainTile tile = The.Map.GetTile(entityData.MapPosition.Value);

                if (!tile.AllegiancesThatSeeThisTile.Contains(Parent.Allegiance)
                    && !Parent.Allegiance.Members.Contains(entityData)) // ignore own members, during spawn
                {
                    //int minDistance = (int)Parent.Allegiance.Members.Min(e => Common.DistanceOctile(e.MapPosition.Value, entityData.MapPosition.Value));
                    float minDistance = Parent.Allegiance.Members.Min(e => Common.DistanceOctile(e.Location.Value, entityData.Location.Value));

                    float sensorRange = Parent.Allegiance.RepresentativeEntityType.SensorType.Range;
                    
                    Console.WriteLine("{0} can see {1} in FOW on {2}? Min distance: {3} / {4} Missing memoryfact?", Parent.Allegiance.KeyName, entityData.ToString(), entityData.MapPosition.Value, minDistance, sensorRange);
                    System.Diagnostics.Debug.Assert(false, "Can see entity in FOW? Missing memoryfact?");

                   //throw new Exception("Can see entity in FOW? Missing memoryfact?");
                }
            }           
#endif
        }


        /// <summary>
        /// To be called when we no longer have knowledge of an entity
        /// </summary>
        public void RemoveFromCollectionsOfKnownEntities(EntityID entityID)
        {
            KnownEntityDataTree.RemoveObject(entityID);

            if (!AllKnownOutsideAgentsOnPlaySite.Remove(entityID))
            {
                AllKnownThreatSources.Remove(entityID);  // mutex'ed
            }

           // AllKnownOutsideAgentsOnPlaySite.Remove(entityIDToRemove);
           
        }



        public void RemoveCropHarvester(EntityType cropItem, Entity harvester) //, ResourceContainer crop)
        {
            List<Tuple<EntityID, ResourceID>> list;
            if (ResourceHarvesters.TryGetValue(cropItem, out list))
            {
                list.RemoveAll(c => c.Item1 == harvester.EntityID);
            }
        }

        public void AddCropHarvester(EntityType cropItem, Entity harvester, ResourceContainer crop)
        {
            List<Tuple<EntityID, ResourceID>> cropHarvesters;
            if (!ResourceHarvesters.TryGetValue(cropItem, out cropHarvesters))
            {
                cropHarvesters = new List<Tuple<EntityID, ResourceID>>();
                ResourceHarvesters.Add(cropItem, cropHarvesters);
            }

            cropHarvesters.Add(new Tuple<EntityID, ResourceID>(harvester.EntityID, crop.ID));
        }

        public ResourceMap GetCropsMap(ResourceType resourceType)
        {
            ResourceMap cropsmap;
            if (!ResourceMaps.TryGetValue(resourceType.ResourceItemType, out cropsmap))
            {
                cropsmap = new ResourceMap(resourceType);
                ResourceMaps.Add(resourceType.ResourceItemType, cropsmap);
            }

            return cropsmap;
        }


       // bool hasValidated = false;

        public void Update(GameTime gameTime)
        {

            foreach (ThreatSource threat in TransientThreats)
            {
                threat.Update(gameTime);
            }

            // clean up
            for (int i = TransientThreats.Count - 1; i >= 0; i--)
            {
                if (TransientThreats[i].IsExpired())
                {
                    TransientThreats.Remove(TransientThreats[i]);
                }
            }


            foreach (KeyValuePair<EntityType, ResourceMap> kvp in ResourceMaps)
            {
                kvp.Value.Update(gameTime);
            }

            foreach (var kvp1 in AllMovementMaps)
            {
                foreach (var kvp2 in kvp1.Value)
                {
                    foreach (var kvp3 in kvp2.Value)
                    {
                        kvp3.Value.Update(gameTime);
                    }
                }
            }

         /*   if (!hasValidated)
            {
                AssertSeenEntitiesOnPlaySiteNotInFOW();

                hasValidated = true;
            }


            if (assertRegulator.IsReady())
            {
                AssertSeenEntitiesOnPlaySiteNotInFOW();
            }*/
        }


        public void Destroy()
        {
            if (AllMovementMaps != null)
            {
                foreach (var kvp1 in AllMovementMaps)
                {
                    foreach (var kvp2 in kvp1.Value)
                    {
                        foreach (var kvp3 in kvp2.Value)
                        {
                            kvp3.Value.Destroy(); // safe in foreach..? yes, if we don't touch the collection.
                        }
                    }
                }
            }

            foreach (KeyValuePair<EntityType, ResourceMap> kvp in ResourceMaps)
            {
                kvp.Value.Destroy();
            }

          
            if (DiscomfortMaps != null)
            {
                foreach (var item in DiscomfortMaps)
                {

                    foreach (var innerItem in item.Value)
                    {
                        foreach (var innerInnerItem in innerItem.Value)
                        {
                            innerInnerItem.Value.Destroy();
                        }
                    }
                }
            }

            if (ThreatMaps != null)
            {
                foreach (var item in ThreatMaps)
                {
                    foreach (var innerItem in item.Value)
                    {
                        innerItem.Value.Destroy();
                    }
                }
            }
        }

        /// <summary>
        /// used to filter process events by FOW/knowledge
        /// </summary>
        /// <param name="action"></param>
        /// <param name="process"></param>
        /// <param name="subscriber"></param>
        /// <param name="methodID"></param>
        public void RegisterProcessDestroyedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            // place ourselves as middleman for the event:
            process.ProcessDestroyedEvent.Add(processDestroyedMethodID, this);
            RegisterProcessEvent(ProcessDestroyedEvents, action, process, subscriber, out methodID);
        }

        public void RegisterProcessCompletedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            // place ourselves as middleman for the event:
            process.ProcessCompletedEvent.Add(processCompletedMethodID, this);
            RegisterProcessEvent(ProcessCompletedEvents, action, process, subscriber, out methodID);
        }

        public void RegisterProcessProducingEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            // place ourselves as middleman for the event:
            process.ProcessProducingEvent.Add(processProducingMethodID, this);
            RegisterProcessEvent(ProcessProducingEvents, action, process, subscriber, out methodID);
        }

        public void RegisterProcessStartedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            // place ourselves as middleman for the event:
            process.ProcessStartedEvent.Add(processStartedMethodID, this);
            RegisterProcessEvent(ProcessStartedEvents, action, process, subscriber, out methodID);
        }

      /*  public void RegisterInputDestroyedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            // place ourselves as middleman for the event:
            process.InputDestroyedEvent.Add(processInputDestroyedMethodID, this);
            RegisterProcessEvent(ProcessInputDestroyedEvents, action, process, subscriber, out methodID);
        }*/

        /// <summary>
        /// creates an event for every process. the event can have many subscribers, but most likely just one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="events"></param>
        /// <param name="action"></param>
        /// <param name="process"></param>
        /// <param name="subscriber"></param>
        /// <param name="methodID"></param>
        private static void RegisterProcessEvent<T>(Dictionary<SimProcessID, IDActionEvent<T>> events, Action<T> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
        {
            IDActionEvent<T> newEvent;

            if (!events.TryGetValue(process.ID, out newEvent))
            {
                newEvent = new IDActionEvent<T>();
                events.Add(process.ID, newEvent);
            }

            newEvent.AddAndRegister(action, subscriber, out methodID);

        }

        public void DeleteMemoryOfProcesses(MemoryFact memoryFact)
        {
            if (memoryFact.Processes != null)
            {
                foreach (var item in memoryFact.Processes)
                {
                    DeleteMemoryOfProcess(item);
                }

                memoryFact.Processes = null;
            }
        }

        /// <summary>
        /// similar to DeleteMemoryOfEntity
        /// call this when the process has been detected, or when a memory of a process needs to be removed
        /// </summary>
        /// <param name="gameObject"></param>
        public bool DeleteMemoryOfProcess(SimProcessID id) //, SimProcess process) 
        {
           
            ProcessMemory memoryFact;
            if (ProcessMemoryFacts.TryGetValue(id, out memoryFact))
            {
                ProcessMemoryFacts.Remove(id);

                SimProcess process = SimProcess.FindById(id);

                // Either sync with process now or notify listeners that the process is gone.
                // Sync process with the properties that were set on the memory fact:
                if (process != null)
                {
                    process.ImmovableInput = memoryFact.ImmovableInput;
                    process.ImmovableTool = memoryFact.ImmovableTool;
                    process.GroundLocation = memoryFact.GroundLocation;
                }
                else
                {
                    if (memoryFact.RealProcessIsCompleted)
                    {
                        InvokeProcessEvent(ProcessCompletedEvents, id, memoryFact);
                    }

                    InvokeProcessEvent(ProcessDestroyedEvents, id, memoryFact);

                    // NOW we can deregister:
                    DeregisterProcessEvents(id);       
                }

              
                if (memoryFact.MapPosition.HasValue)
                {
                    The.Map.TileMap[memoryFact.MapPosition.Value.X][memoryFact.MapPosition.Value.Y].RemoveRememberedProcess(Parent, memoryFact);
                }

                memoryFact.Destroy();

                return true;
            }

            return false;
        }

        /// <summary>
        /// should be in symmetry with StoreMemoryOfProcess...
        /// </summary>
        /// <param name="process"></param>
        /// <param name="testForUsesMemory"></param>
        /// <param name="suppressClientFeedback"></param>
        /// <param name="resourceDetectionFactor"></param>
        /// <param name="detectingEntity"></param>
        public void SeeProcess(SimProcess process)
        {           
            if (UsesMemory(process))
            {
                DeleteMemoryOfProcess(process.ID);
            }
        }


        /// <summary>
        /// some critters cannot see the process outputs inside containers. They should not store memories of the process either as the inconsistency leads to the process being removed immediately...
        /// 
        /// check everything that ProcessMemory requires!
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private bool CanSeeProcessItems(SimProcess process) //, out bool processIsInvalid)
        {
            if (process.ContainerToPlaceOutputsIn != null)
            {
                IKnownEntityData siteData;
                EntityResult result;
                result = this.Parent.GetKnownData(process.ContainerToPlaceOutputsIn.Value, out siteData);
               
                if (GoalEvaluator.EntityDataResultCausesSkip(result))  // also NewUnknownStatus
                {                    
                    return false;
                }                
            }
            

            //processIsInvalid = false;

            if (process.OutputEntities != null)
            {
                foreach (var item in process.OutputEntities)
                {
                    IKnownEntityData data;
                    EntityResult result;
                    result = Parent.GetKnownData(item, out data);

                    if (GoalEvaluator.EntityDataResultCausesSkip(result))
                    {
                        return false;
                    }   

                    /*
                    if (!CanSeeEntity(item, out processIsInvalid))
                    {
                        return false;
                    }*/
                }
            }

            if (process.StationaryTools != null)
            {
                foreach (var item in process.StationaryTools)
                {
                    IKnownEntityData data;
                    EntityResult result;
                    result = Parent.GetKnownData(item, out data);

                    if (GoalEvaluator.EntityDataResultCausesSkip(result))
                    {
                        return false;
                    }  
                    /*
                    if (!CanSeeEntity(item, out processIsInvalid))
                    {
                        return false;
                    }*/
                }
            }

           /* if (process.ContainerToPlaceOutputsIn.HasValue)
            {
                if (!CanSeeEntity(process.ContainerToPlaceOutputsIn.Value, out processIsInvalid))
                {
                    return false;
                }
            }*/

            return true;
        }


        public void StoreMemoryOfProcess(SimProcess process)
        {
            if (!UsesMemory(process))
            {
                return;
            }

            // we have to know all of the process items before we can init the memory correctly:
            if (!CanSeeProcessItems(process))
            {             
                // clean up the process memory if it exists
                DeleteMemoryOfProcess(process.ID);
                return;
            }

            ProcessMemory processMemory;
            if (ProcessMemoryFacts.TryGetValue(process.ID, out processMemory))
            {
                // if we already have it in memory, just update...    
                if (!processMemory.Init(process, Parent))
                {
                    // this means that the process is invalid. abort the operation.                  
                    DeleteMemoryOfProcess(process.ID);
                }
            }
            else
            {
                // create a knowledge unit for this object:                  
                processMemory = new ProcessMemory(process);

                if (processMemory.Init(process, Parent))
                {
                    // place on the map so we can re-detect it:
                    if (StoreProcessOnTerrainTile(processMemory))
                    {
                        if (processMemory.MapPosition.HasValue)
                        {
                            The.Map.TileMap[processMemory.MapPosition.Value.X][processMemory.MapPosition.Value.Y].AddRememberedProcess(Parent, processMemory);
                        }
                    }

                    ProcessMemoryFacts.Add(process.ID, processMemory);
                }
            }
        }


        private bool StoreProcessOnTerrainTile(IKnownProcess processData)
        {
            if (processData.GroundLocation.HasValue) // ?
            {
                return true;
            }
            else return false;

        }

        /// <summary>
        /// similar to GetKnownEntityData
        /// 
        /// returns known data about this process.
        /// the current data if we can see the entity, else the stored, remembered data from last we saw it.
        /// if the memory has gone stale, null is returned.
        /// if the seen entity does not exist anymore, null is returned.
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="processID"></param>
        /// <returns>SeenDirectly: it is safe to cast the data to Entity</returns>
        public ProcessResult GetKnownProcessData(SimProcessID processID, out IKnownProcess data)
        {
            ProcessResult? result;
            if (TryGetMemoryProcess(processID, out data, out result))
            {
                return result.Value;
            }
            else
            {
                data = LookUp<SimProcess, SimProcessID>.FindByID(processID);
                if (data == null)
                {
                    return ProcessResult.Destroyed;
                }
                else
                {
                    /* if (data.AllegianceID == Allegiance.ID || AllDetectedEntities.ContainsKey(data as Entity))
                     {*/
                    // it is now safe to cast the data to Entity
                    return ProcessResult.SeenDirectly;
                    /*  }
                      else
                      {
                          return EntityResult.IsUnknownEntity;
                      }*/
                }
            }
        }


        private bool TryGetMemoryProcess(SimProcessID processID, out IKnownProcess data, out ProcessResult? returnValue)
        {
            ProcessMemory status;
            if (ProcessMemoryFacts.TryGetValue(processID, out status))
            {
                data = status;
                returnValue = ProcessResult.Remembered;

                return true;
            }

            returnValue = null; // EntityResult.EntityStatusIsNowUnknown;
            data = null;
            return false;
        }

        public void UnSeeProcess(SimProcess process)
        {
            if (UsesMemory(process))
            {
                StoreMemoryOfProcess(process);
            }
        }

        private bool UsesMemory(SimProcess process)
        {
            return true;
        }


        public void ProcessDestroyed(SimProcess process)
        {
            IKnownProcess processData;
            if (GetKnownProcessData(process.ID, out processData) == ProcessResult.SeenDirectly)
            { 
                // only if seen:
   
                // also invoked when memory is seen as destroyed.
                InvokeProcessEvent(ProcessDestroyedEvents, process.ID, processData);
                
                // not if a process memory exists:             
                DeregisterProcessEvents(process.ID);               
            }

           
         /*   ProcessStartedEvents.Remove(process.ID);
            ProcessProducingEvents.Remove(process.ID);
            ProcessCompletedEvents.Remove(process.ID);
            ProcessDestroyedEvents.Remove(process.ID);
            */
        }

        private void DeregisterProcessEvents(SimProcessID processID)
        { 
            // de-register to avoid filling up the memory
            ProcessStartedEvents.Remove(processID);
            ProcessProducingEvents.Remove(processID);
            ProcessCompletedEvents.Remove(processID);
            ProcessDestroyedEvents.Remove(processID);
        }


        public void ProcessCompleted(SimProcess process)
        {
            IKnownProcess processData;
            ProcessResult result = GetKnownProcessData(process.ID, out processData);
            if (result == ProcessResult.SeenDirectly)
            {                 
                // only if seen:   
                // also invoked when memory is seen as completed.
                InvokeProcessEvent(ProcessCompletedEvents, process.ID, processData);
             

                // these won't be needed anymore 
                // process destroyed event fires after this.
                // (processDestroyed may not have been registered, if this is a non-job process):
                ProcessProducingEvents.Remove(process.ID);
                ProcessCompletedEvents.Remove(process.ID);
            }
            else if (result == ProcessResult.Remembered)
            {
                ((ProcessMemory)processData).SetCompletedProcess(process);
              
            }
        }

        public void ProcessStarted(SimProcess process)
        {
            IKnownProcess processData;
            if (GetKnownProcessData(process.ID, out processData) == ProcessResult.SeenDirectly)
            { // only if seen:

                InvokeProcessEvent(ProcessStartedEvents, process.ID, processData);
            }

            // Whether it fired or not, this event will not be used anymore:
            ProcessStartedEvents.Remove(process.ID);
          
        }

        /// <summary>
        /// gets called regularly during production
        /// </summary>
        /// <param name="process"></param>
        public void ProcessProducing(SimProcess process)
        {
            IKnownProcess processData;
            if (GetKnownProcessData(process.ID, out processData) == ProcessResult.SeenDirectly)
            { // only if seen:

                InvokeProcessEvent(ProcessProducingEvents, process.ID, processData);
                // also invoked when memory is seen as destroyed.

              //  ProcessProducingEvents.Remove(process.ID);
            }
        }


       /* public void ProcessInputDestroyed(SimProcess process, Entity input)
        {
            IKnownProcess processData;
            if (GetKnownProcessData(process.ID, out processData) == ProcessResult.SeenDirectly)
            { // only if seen:

                InvokeProcessEvent(ProessInputDestroyedEvents, process.ID, processData);
            }
        }*/


        private static void InvokeProcessEvent<T>(Dictionary<SimProcessID, IDActionEvent<T>> events, SimProcessID processID, T eventArgs)
        {
            IDActionEvent<T> subscriberEvents;
            if (events.TryGetValue(processID, out subscriberEvents))
            {
                subscriberEvents.Invoke(eventArgs);
            }
        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup<SimProcess>.Add(processCompletedMethodID,
                    ProcessCompleted);

            ActionLookup<SimProcess>.Add(processDestroyedMethodID,
                  ProcessDestroyed);

            ActionLookup<SimProcess>.Add(processStartedMethodID,
                ProcessStarted);

            ActionLookup<SimProcess>.Add(processProducingMethodID,
              ProcessProducing);

           /* ActionLookup<SimProcess>.Add(processInputDestroyedMethodID,
               ProcessInputDestroyed);*/

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

        /// <summary>
        /// debug method..
        /// </summary>
        /// <returns></returns>
        private DetectableID GetID(KeyValuePair<IDetectable, bool> kvp)
        {
            System.Diagnostics.Debug.Assert(kvp.Key.ID != DetectableID.Invalid, "Invalid ID!!");
            return kvp.Key.ID;
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
           

            this.snapshotAllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>>();

            if (sn.mode != Snapshotter.Mode.Load)
            {
                // gather the data to save:

                foreach (var item in AllMovementMaps)
                {
                    Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>> dict2 = new Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>();
                    snapshotAllMovementMaps.Add(item.Key, dict2);

                    foreach (var item2 in item.Value)
                    {
                        Dictionary<ThreatStance, CyclableID> dict3 = new Dictionary<ThreatStance, CyclableID>();
                        dict2.Add(item2.Key, dict3);

                        foreach (var item3 in item2.Value)
                        {
                            dict3.Add(item3.Key, item3.Value.ID);
                        }
                    }
                }

                snapshotAllDiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>>();
                foreach (var item in DiscomfortMaps)
                {
                    Dictionary<EntityType, Dictionary<ThreatStance, IMapID>> dict2 = new Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>();
                    snapshotAllDiscomfortMaps.Add(item.Key, dict2);

                    foreach (var item2 in item.Value)
                    {
                        Dictionary<ThreatStance, IMapID> dict3 = new Dictionary<ThreatStance, IMapID>();
                        dict2.Add(item2.Key, dict3);

                        foreach (var item3 in item2.Value)
                        {
                            dict3.Add(item3.Key,
                                item3.Value.ID);
                        }
                    }
                }


                snapshotThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>();
                foreach (var item in ThreatMaps)
                {
                    Dictionary<ThreatStance, IMapID> dict3 = new Dictionary<ThreatStance, IMapID>();
                    snapshotThreatMaps.Add(item.Key, dict3);

                    foreach (var item3 in item.Value)
                    {
                        dict3.Add(item3.Key,
                            item3.Value.ID);
                    }
                }

                snapshotProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemoryID>();
                foreach (var item in ProcessMemoryFacts)
                {
                    snapshotProcessMemoryFacts.Add(item.Key, item.Value.ID);
                }

               // this.snapshotKnownEntityDataTree = knownEntityDataTree != null ? knownEntityDataTree.GetAllObjectsAndPositions() : null;
                //this.snapshotMemoryFacts = MemoryFacts.ToDictionary(m => m.Key, m => m.Value.ID);

                this.snapshotResourceMaps = ResourceMaps.ToDictionary(r => r.Key, r => r.Value.ID);

                this.snapshotKnownEntityDataTree = knownEntityDataTree != null ? knownEntityDataTree.GetAllObjectsAndPositions() : null;
            
            }

            this.snapshotAllMovementMaps = sn.DoDoubleNestedDictionary(snapshotAllMovementMaps);
            this.snapshotAllDiscomfortMaps = sn.DoDoubleNestedDictionary(snapshotAllDiscomfortMaps);
            this.snapshotThreatMaps = sn.DoNestedDictionary(snapshotThreatMaps);
            this.snapshotProcessMemoryFacts = sn.DoDictionary(snapshotProcessMemoryFacts);
            
            this.snapshotKnownEntityDataTree = sn.DoList(snapshotKnownEntityDataTree); // save tree members for recreation post-load
            this.knownEntityDataTree = (PointQuadTree<EntityID>)sn.DoISnapshot(knownEntityDataTree); // only snapshots some data, not the whole quad tree and nodes.
            
            this.AllKnownOutsideAgentsOnPlaySite = sn.DoDictionary(AllKnownOutsideAgentsOnPlaySite);
            this.AllKnownThreatSources = sn.DoDictionary(AllKnownThreatSources);

         //   this.memoryFactLeafs = sn.DoMultiMap(memoryFactLeafs);
            this.ResourceHarvesters = sn.DoMultiMap(ResourceHarvesters);

            this.processCompletedMethodID = sn.DoEnum(processCompletedMethodID);
            this.processDestroyedMethodID = sn.DoEnum(processDestroyedMethodID);
            this.processStartedMethodID = sn.DoEnum(processStartedMethodID);
            this.processProducingMethodID = sn.DoEnum(processProducingMethodID);

            this.ProcessStartedEvents = sn.DoDictionary(ProcessStartedEvents);
            this.ProcessCompletedEvents = sn.DoDictionary(ProcessCompletedEvents);
            this.ProcessDestroyedEvents = sn.DoDictionary(ProcessDestroyedEvents);
            this.ProcessProducingEvents = sn.DoDictionary(ProcessProducingEvents);

            this.AllKnownResourceContainers = sn.DoMultiMapHashSet(AllKnownResourceContainers);

            this.snapshotResourceMaps = sn.DoDictionary(snapshotResourceMaps);

            this.SpottedPrey = sn.DoHashSet(SpottedPrey);
            this.SpottedAnimals = sn.DoHashSet(SpottedAnimals);

            // this.TransientThreats not currently used... maybe added later?

            sn.Ignore(ProcessMemoryFacts);
            sn.Ignore(ThreatMaps);          
            sn.Ignore(AllMovementMaps);
            sn.Ignore(DiscomfortMaps);         
            sn.Ignore(ResourceMaps);
           
            sn.Ignore(Parent);
            sn.Ignore(TransientThreats); // TODO if ever put in use


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           
          /*  if (knownEntityDataTree != null)
            {
                knownEntityDataTree.SetPreLoadPostProcess(snapshotKnownEntityDataTree);
                knownEntityDataTree.LoadPostProcess(sn); // this will fix the quad tree

                snapshotKnownEntityDataTree = null;
            }*/

            if (knownEntityDataTree != null)
            {
                knownEntityDataTree.SetPreLoadPostProcess(snapshotKnownEntityDataTree);
                knownEntityDataTree.LoadPostProcess(sn); // this will fix the quad tree

                snapshotKnownEntityDataTree = null;
            }


            ResourceMaps = snapshotResourceMaps.ToDictionary(r => r.Key, r => (ResourceMap)LookUp<ICyclable, CyclableID>.FindByID(r.Value));

            AllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>>();
            DiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>>();
            ThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>>();

            foreach (var item in snapshotAllMovementMaps)
            {
                Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>> dict2 = new Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>();
                AllMovementMaps.Add(item.Key, dict2);

                foreach (var item2 in item.Value)
                {
                    Dictionary<ThreatStance, MovementMap> dict3 = new Dictionary<ThreatStance, MovementMap>();
                    dict2.Add(item2.Key, dict3);

                    foreach (var item3 in item2.Value)
                    {
                        dict3.Add(item3.Key, (MovementMap)LookUp<ICyclable, CyclableID>.FindByID(item3.Value));
                    }
                }
            }
            snapshotAllMovementMaps = null; // set to null for next save

            foreach (var item in snapshotAllDiscomfortMaps)
            {
                Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>> dict2 = new Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>();
                DiscomfortMaps.Add(item.Key, dict2);

                foreach (var item2 in item.Value)
                {
                    Dictionary<ThreatStance, DiscomfortMap> dict3 = new Dictionary<ThreatStance, DiscomfortMap>();
                    dict2.Add(item2.Key, dict3);

                    foreach (var item3 in item2.Value)
                    {
                        dict3.Add(item3.Key,
                            (DiscomfortMap)LookUp<IMap, IMapID>.FindByID(item3.Value));
                    }
                }
            }
           
            snapshotAllDiscomfortMaps = null; // set to null for next save

            foreach (var item in snapshotThreatMaps)
            {
                Dictionary<ThreatStance, ThreatMap> dict3 = new Dictionary<ThreatStance, ThreatMap>();
                ThreatMaps.Add(item.Key, dict3);

                foreach (var item3 in item.Value)
                {
                    dict3.Add(item3.Key,
                        (ThreatMap)LookUp<IMap, IMapID>.FindByID(item3.Value));
                }
            }
            snapshotThreatMaps = null; // set to null for next save



            ProcessMemoryFacts = snapshotProcessMemoryFacts.ToDictionary(m => m.Key, m => (ProcessMemory)LookUp<ProcessMemory, ProcessMemoryID>.FindByID(m.Value));

            LoadPostProcessRegisterMethodIDs();

            CreateRegulators();

        }

        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using System.Timers;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Log;
using GameStateManagement;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Resources;
using UWGame.Control;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public class Sensor : Component
    {
       
        public bool IsActive = false; // true;

     
        int? oldSensorRadiusInTiles;

        public enum TileStatus { Seen, Unseen }

        public HashSet<TerrainTile> tilesCurrentlySeen = new HashSet<TerrainTile>();
        List<TerrainTileID> snapshotCurrentlySeen;


        List<TerrainTile> tilesToUnsee = new List<TerrainTile>();
        List<TerrainTile> tilesToSee = new List<TerrainTile>();
       

        public void NotifyPartIsBroken()
        {
            IsActive = false;
        }

        public override double? GetUpdateInterval()
        {
            if (!Parent.IsDead
                && Parent.IsCompleted()
                && Parent.IsOnPlaySite())
            {
                return 1d / GameData.Instance.Constants.DetectionUpdatesPerSecond; // 0;
            }

            return null;
        }

        public Sensor()
        {
        }

        public Sensor(Entity parent)
            : base(parent, 1d / GameData.Instance.Constants.DetectionUpdatesPerSecond)
        {

            // CreateRegulators();
        }

       
        public void UnseeTilesInRange()
        {            
            foreach (var tile in tilesCurrentlySeen)
            {
                tile.ChangeTileSeenBy(Parent, TileStatus.Unseen);
            }           
        }

       

        public List<TerrainTile> UpdateTilesSeenBySensor(Point newCenterTilePos)
        {
            TerrainTile[][] map = The.Map.TileMap;
            int newRadiusInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
            

            foreach (var item in tilesCurrentlySeen)
            {
                if (Common.DistanceOctile(newCenterTilePos, new Point(item.X, item.Y)) > newRadiusInTiles)
                {
                    tilesToUnsee.Add(item);
                }
            }
           
            int width = Common.GetJaggedArrayWidth(map);
            int height = Common.GetJaggedArrayHeight(map);

            int minX, minY, maxX, maxY;

            TerrainTile[] column;
            
           
            minX = Math.Max(0, newCenterTilePos.X - newRadiusInTiles);
            minY = Math.Max(0, newCenterTilePos.Y - newRadiusInTiles);


            maxX = Math.Min(width - 1, newCenterTilePos.X + newRadiusInTiles);
            maxY = Math.Min(height - 1, newCenterTilePos.Y + newRadiusInTiles);

            
            TerrainTile currentTile;
            for (int x = minX; x <= maxX; x++)
            {
                column = map[x];
                for (int y = minY; y <= maxY; y++)
                {
                    currentTile = column[y];
                    if (!tilesCurrentlySeen.Contains(currentTile)
                        && Common.DistanceOctile(newCenterTilePos, new Point(x, y)) <= newRadiusInTiles)
                    {
                        tilesToSee.Add(column[y]);
                    }
                }
            }

         
            // set new status of the tiles that have changed
            foreach (TerrainTile tile in tilesToSee)
            {
                SeeTile(tile);
            }

            foreach (TerrainTile tile in tilesToUnsee)
            {
                UnseeTile(tile);
            }

            tilesToSee.Clear();
            tilesToUnsee.Clear();
           
            return null;
        }


        private void SeeTile(TerrainTile tile)
        {
            tile.ChangeTileSeenBy(Parent, TileStatus.Seen);
            tilesCurrentlySeen.Add(tile);
        }

        private void UnseeTile(TerrainTile tile)
        {
            tile.ChangeTileSeenBy(Parent, TileStatus.Unseen);
            tilesCurrentlySeen.Remove(tile);
        }

        public void SeeTilesInRange(Point centerTilePos)
        {
          
            if (The.Map.TileIsOnMap(centerTilePos))
            {
                int newRadiusInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);

                TerrainTile[][] map = The.Map.TileMap;


                int minX = Math.Max(0, centerTilePos.X - newRadiusInTiles);
                int minY = Math.Max(0, centerTilePos.Y - newRadiusInTiles);

                int width = Common.GetJaggedArrayWidth(map);
                int height = Common.GetJaggedArrayHeight(map);

                int maxX = Math.Min(width - 1, centerTilePos.X + newRadiusInTiles);
                int maxY = Math.Min(height - 1, centerTilePos.Y + newRadiusInTiles);


                TerrainTile[] mapRow;
                for (int x = minX; x <= maxX; x++)
                {
                    mapRow = map[x];
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (Common.DistanceOctile(centerTilePos, new Point(x, y)) <= newRadiusInTiles)
                        {
                            SeeTile(mapRow[y]);
                        }
                    }
                }

                oldSensorRadiusInTiles = newRadiusInTiles;
            }
        }


        protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
        {
            // update seen tiles if our sensor range has changed (with light level etc.)
            int newRadiusInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
            if (newRadiusInTiles != oldSensorRadiusInTiles)
            {
                UpdateTilesSeenBySensor(Parent.MapPosition.Value);

                oldSensorRadiusInTiles = newRadiusInTiles;
            }

            RollToDetect(); // should the chance to detect be made relative to elapsed game time? probably... but how?       

        }

        /* OLD:
        protected override void UpdateRegulated(double? timeSinceLastUpdate)
        {
            // update seen tiles if our sensor range has changed (with light level etc.)
            int newRadiusInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
            if (newRadiusInTiles != oldSensorRadiusInTiles)
            {
                UpdateTilesSeenBySensor(Parent.MapPosition.Value);

                oldSensorRadiusInTiles = newRadiusInTiles;
            }

            RollToDetect(); // should the chance to detect be made relative to elapsed game time? probably... but how?       

        }*/


        /*
        public void Update(GameTime gameTime)
        {           

            if (visionRangeRegulator.IsReady())
            {
                // update seen tiles if our sensor range has changed (with light level etc.)
                int newRadiusInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
                if (newRadiusInTiles != oldSensorRadiusInTiles)
                {
                    UpdateTilesSeenBySensor(Parent.MapPosition.Value);
                    
                    oldSensorRadiusInTiles = newRadiusInTiles;
                }
            }

            if (detectionRegulator.IsReady())
            {
                RollToDetect(); // should the chance to detect be made relative to elapsed game time? probably... but how?
            }

        }*/

        List<IDetectable> allDetectables = new List<IDetectable>();                     
        
        /// <summary>
        ///  Update function that periodically can detect hidden things in range depending on agent action, skill, stance, environment etc.
        /// </summary>
        /// <param name="centerTilePos"></param>
        public void RollToDetect() 
        {
           
            Point centerTilePos = Parent.MapPosition.Value;

            int radius = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
            TerrainTile[][] map = The.Map.TileMap;

            int minX = Math.Max(0, centerTilePos.X - radius);
            int minY = Math.Max(0, centerTilePos.Y - radius);

            int width = Common.GetJaggedArrayWidth(map);
            int height = Common.GetJaggedArrayHeight(map);

            int maxX = Math.Min(width - 1, centerTilePos.X + radius);
            int maxY = Math.Min(height - 1, centerTilePos.Y + radius);
            
           
            allDetectables.Clear();
            
            // get all detectables in range
            SharedKnowledge sharedKnowledge = Parent.Intelligence.Allegiance.SharedKnowledge;

            // NEW: iterate tiles seen instead, there was an inconsistency:
            foreach (var thisTile in tilesCurrentlySeen)
            {
                thisTile.GetDetectablesOnTile(allDetectables);
            }

            /* 
             * TerrainTile[] mapRow;
            TerrainTile thisTile;
            for (int x = minX; x <= maxX; x++)
            {
                mapRow = map[x];
                for (int y = minY; y <= maxY; y++)
                {
                    if (Common.DistanceOctile(centerTilePos, new Point(x, y)) <= radius)
                    {
                        if (Common.DistanceOctile(centerTilePos, new Point(x, y)) == radius)
                        {

                        }

                        thisTile = mapRow[y];

                      //  System.Diagnostics.Debug.Assert(thisTile.AllegiancesThatSeeThisTile.Contains(Parent.Intelligence.Allegiance), "tile is in FOW?");

                        if (tilesCurrentlySeen.Contains(thisTile)) // extra safeguard, because assertion failed
                        {
                            thisTile.GetDetectablesOnTile(allDetectables);
                        }
                        else
                        {
                            // it seems FOW  ChangeTileSeenBy code and detection are out of sync...
                        }
                      
                    }
                }
            }*/

            RollToDetect(Parent, sharedKnowledge, allDetectables);
        }

        


        public void RollToDetect(Entity detectingEntity, SharedKnowledge sharedKnowledge, List<IDetectable> allDetectables, bool suppressClientFeedbackAndEvents = false, bool detectAllWhichHasMinimumRange = false, bool unseeAfterDetecting = false, bool rollToDetectHiddenEntities = true, bool doAssert = true)
        {
            if (allDetectables != null)
            {

                bool seeEntity;
                foreach (IDetectable detectable in allDetectables)
                {
                    seeEntity = false;
                                       

                    if (detectable != detectingEntity)
                    {
                        DetectionFactor resourceDetectionFactor = null;

                        if (sharedKnowledge != null)
                        {
                            //TODO: critters shouldn't detect resources etc.
                            if (sharedKnowledge.UsesMemory(detectable)) // exclude rocks and trees
                            {
                                if (!sharedKnowledge.AllDetectedEntities.Contains(detectable.ID)) // only see non-detected entities. 
                                {
                                    if (detectable.RequiresRollToDetect()) // exclude items and structures from the roll - just see them directly
                                    {
                                        if (rollToDetectHiddenEntities 
                                            && RollToDetect(detectable, sharedKnowledge, out resourceDetectionFactor, detectAllWhichHasMinimumRange))
                                        {
                                            seeEntity = true;
                                        }

                                    }
                                    else
                                    {
                                        seeEntity = true;
                                    }
                                }
                            }

                            if (seeEntity)
                            {
                                sharedKnowledge.SeeDetectable(detectable, suppressClientFeedbackAndEvents, resourceDetectionFactor, detectingEntity, doAssert: doAssert);

                               
                                if (unseeAfterDetecting)  // ExploreAction uses this
                                {                                  
                                    Entity detectableAsEntity = detectable as Entity;
                                    if (detectableAsEntity != null)
                                    {
                                        sharedKnowledge.UnSeeEntity(detectableAsEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        

      /*  public void RollToDetect(Entity detectingEntity, SharedKnowledge sharedKnowledge, List<IDetectable> allDetectables, bool suppressClientFeedbackAndEvents = false, bool detectAllWhichHasMinimumRange = false) 
        {
            if (allDetectables != null)
            {
              
                bool seeEntity;
                foreach (IDetectable detectable in allDetectables)
                {
                    seeEntity = false;

                    if (detectable != detectingEntity)
                    {

                        //TODO: critters shouldn't detect resources etc.
                        if (sharedKnowledge.UsesMemory(detectable)) // exclude rocks and trees
                        {
                            if (detectable.RequiresDetection()) // exclude items and structures from the roll - just see them directly
                            {
                                
                                if (!sharedKnowledge.AllDetectedEntities.ContainsKey(detectable)) // only call RollToDetect() on non-detected entities. 
                                {
                                    bool addResourceLogMessage;
                                    ResourceDetectionFactor resourceDetectionFactor;

                                    
                                    if (RollToDetect(detectable, sharedKnowledge, out addResourceLogMessage, out resourceDetectionFactor, detectAllWhichHasMinimumRange))
                                    {
                                        HandleNewDetection(detectingEntity, sharedKnowledge, detectable, addResourceLogMessage, resourceDetectionFactor, suppressClientFeedbackAndEvents);

                                        if (!suppressClientFeedbackAndEvents)
                                        {
                                            FirePlayerDetectionEvents(detectingEntity, sharedKnowledge, detectable);
                                        }

                                        seeEntity = true;
                                    }
                                }
                            }
                            else
                            {
                                // items in range will continuously pass through this code... perhaps we can optimize it?

                                // NEWDETECT:
                                if (!sharedKnowledge.AllDetectedEntities.ContainsKey(detectable)) 
                                {

                                    if (!suppressClientFeedbackAndEvents)
                                    {
                                        FirePlayerDetectionEvents(detectingEntity, sharedKnowledge, detectable);
                                    }

                                    seeEntity = true;

                                }
                            }

                            if (seeEntity)
                            {
                                sharedKnowledge.SeeDetectable(detectable, false);
                            }
                        }
                    }                    
                }
            }
        }
        */

        /// <summary>
        /// TODO: fix the issue where SeeEntity are called on seen detectables, then merge the two methods
        /// </summary>
        /// <param name="detectingEntity"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="detectable"></param>
        /// <param name="addResourceLogMessage"></param>
    /*    private static void HandleNewDetection(Entity detectingEntity, SharedKnowledge sharedKnowledge, IDetectable detectable, 
            ResourceDetectionFactor resourceDetectionFactor, bool suppressClientFeedback = false)
        {
           
            // we only set this flag so we don't need to roll to detect the entity again.
          //  sharedKnowledge.AllDetectedEntities.Add(detectable, true); // NEWDETECT
           
            
            Entity asEntity = detectable as Entity;
                        
      
            HandleInterestOfDetectable(detectingEntity, detectable, asEntity, resourceDetectionFactor);
        
           
            // is trigger message enough?
            if (asEntity != null && asEntity.CanBeHunted(sharedKnowledge.allegiance)) // The.Map.EntityCanBeMarkedAsHuntTarget(asEntity))
            {
                detectingEntity.HandleMessage(new Message(asEntity, Message.MessageTypes.PreyIsNear, null));//the message needs to be sent in the sammer manner as the corresponding trigger message
            }


            if (!suppressClientFeedback)
            {
                if (sharedKnowledge.allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                {
                    SetFlashing(detectable, asEntity);


                    if (asEntity != null ||  addResourceLogMessage)
                    {
                        The.Client.Log.AddLogEvent(The.Client.Log.GeneralEvent, detectingEntity, string.Format("has spotted {0}", detectable.ToLink()));
                    }
                }
                else
                {
                    if (asEntity != null)
                    {
                        The.Client.Log.AddLogEvent(The.Client.Log.DebugEvent, detectingEntity, string.Format("has spotted {0} (DEBUG)", detectable));

                    }
                }
            }
          
        }*/

        

       

      


        /// <summary>
        /// fire any events that show tutorial dialogs etc. to the player
        /// </summary>
        /// <param name="detectedEntity"></param>
        public static void HandleEntityTypeDetectionEvents(Entity detectingEntity, Entity detectedEntity)
        {
            var detectionEvents = detectingEntity.EntityType.IntelligenceType.DetectEntityTypeEvents;

            if (detectionEvents != null 
                && detectionEvents.Count > 0)
            {
                //List<GlobalConditionalEvent> events;
                List<ActionSets> actionSets;
                if (detectionEvents.TryGetValue(detectedEntity.EntityType, out actionSets))
                {
                 
                    for (int i = actionSets.Count - 1; i >= 0; i--)
                    {
                        ActionSets actionSet = actionSets[i];
                        bool isExpired;
                        actionSet.Fire(detectingEntity, detectedEntity.EntityID, null, out isExpired);

                        if (isExpired)
                        {   
                            // no longer needed:
                            actionSets.Remove(actionSet);
                            if (actionSets.Count == 0)
                            {
                                detectionEvents.Remove(detectedEntity.EntityType);
                            }
                        }
                    }
                   
                }
            }
        }

        public static void HandleResourceDetectionEvents(Entity detectingEntity, ResourceType resourceType)
        {
            var detectionEvents = detectingEntity.EntityType.IntelligenceType.DetectResourceTypeEvents;

            if (detectionEvents != null
               && detectionEvents.Count > 0)
            {
                List<ActionSets> actionSets;
                if (detectionEvents.TryGetValue(resourceType, out actionSets))
                {
                    for (int i = actionSets.Count - 1; i >= 0; i--)
                    {
                        ActionSets actionSet = actionSets[i];
                        bool isExpired;
                        actionSet.Fire(detectingEntity, null, null, out isExpired);

                        if (isExpired)
                        {
                            // no longer needed:
                            actionSets.Remove(actionSet);
                            if (actionSets.Count == 0)
                            {
                                detectionEvents.Remove(resourceType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// fire any events that show tutorial dialogs etc. to the player
        /// </summary>
        /// <param name="entity"></param>
      /*  private static void HandleDetectionEvents(Entity entity)
        {
            if (The.Sim.DetectEntityTypeEvents != null && The.Sim.DetectEntityTypeEvents.Count > 0)
            {
                List<ConditionalEvent> events;
                if (The.Sim.DetectEntityTypeEvents.TryGetValue(entity.EntityType, out events))
                {
                    ConditionalEvent e;
                    for (int i = events.Count - 1; i >= 0; i--)
                    {
                        e = events[i];
                        e.Fire();
                        events.RemoveAt(i);
                    }

                    if (events.Count == 0)
                    {
                        The.Sim.DetectEntityTypeEvents.Remove(entity.EntityType);
                    }
                }
            }
        }*/



       /* private float GetDetectionFactor(IDetectable detectable, bool onlyActiveDetection)        
        {
            return parent.GetDetectionFactor(detectable);

        }*/


        

        /// <summary>
        /// returns true if the entity/resource has been succesfully detected by the sensor
        /// </summary>
        /// <param name="detectable"></param>
        /// <returns></returns>
        public bool RollToDetect(IDetectable detectable, SharedKnowledge sharedKnowledge, out DetectionFactor resourceDetectionFactor, bool detectAllWhichHasMinimumRange)
        {
            resourceDetectionFactor = null;
            //addResourceLogMessage = false;

          

#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Dev.Detect all") == true)
            {
                return true;
            }
#endif

            float distance = Common.DistanceOctile(Parent.PlaySiteLocation, detectable.Location);


         //   Entity entity = detectable as Entity;


            float distanceToAlwaysDetect;


            DetectionType detection = Parent.EntityType.SensorType.DetectionType;
          
            float detectableFactor;
            float stealthFactor;

            float skillFactor = 1f;

            bool requiresExamineAction = false;

            float priorKnowledgeBonus = 0f; 

            if (detectable.ResourceType != null)
            {               
                stealthFactor = 0f;
                
                // use defined settings or default
                if (detection.DetectFactors.TryGetValue(detectable.ResourceType, out resourceDetectionFactor))
                {
                    detectableFactor = resourceDetectionFactor.Value;

                    distanceToAlwaysDetect = resourceDetectionFactor.DistanceToAlwaysDetect ?? GameData.Instance.Constants.DefaultDistanceToAlwaysDetectResources;

                    requiresExamineAction = resourceDetectionFactor.RequiresExamineAction;

                   // addResourceLogMessage = resourceDetectionFactor.AddLogMessageWhenDetected;

                    if (resourceDetectionFactor.SkillToUse != null)
                    {
                        skillFactor = Parent.Intelligence.GetSkillValue(resourceDetectionFactor.SkillToUse);
                    }

                    detectableFactor = Parent.GetEffect(SimEffects.AffectsNumbers.Detection, detectableFactor, resourceDetectionFactor.TypeKey, resourceDetectionFactor.TypeTag); // ??

                }
                else if (detection.DetectionDisabled)
                {
                    distanceToAlwaysDetect = 0f;
                    detectableFactor = 0f;
                }
                else
                {
                    distanceToAlwaysDetect = GameData.Instance.Constants.DefaultDistanceToAlwaysDetectResources;
                    detectableFactor = 1f;                    
                }
            }
            else
            {
                Entity detectableAsEntity = detectable as Entity;

                IKnownEntityData entityData;
                EntityResult result;

                if (sharedKnowledge.TryGetMemoryFacts(detectableAsEntity.EntityID, out entityData, out result))
                {
                    if (entityData != null)
                    {
                        // peat deposits etc. also need to be redetected after coming out of FOW!
                        if (entityData.EntityType.LocomotorType != null)
                        {
                            if (entityData.Location == detectableAsEntity.Location)
                            {
                                priorKnowledgeBonus = 1f; // if we remember this entity, and it is still in the same spot, give a big bonus to detection
                            }
                            else
                            {
                                priorKnowledgeBonus = 0.2f; // give a lesser bonus if the entity moved
                            }
                        }
                        else
                        {
                            return true;
                            //priorKnowledgeBonus = 100f; // NEW: always see (redetect) terrain and unowned static entities right away. 
                        }
                    }
                    else
                    {
                        priorKnowledgeBonus = 0.2f; // give a lesser bonus if the entity moved
                    }
                }

                // entities can sneak or cause detection because of their actions:
                stealthFactor = detectableAsEntity.GetAvoidDetectionFactor();

                DetectionFactor entityTypeDetectionFactor;

                // use defined settings or default
                if (detection.DetectFactors.TryGetValue(detectable.EntityType, out entityTypeDetectionFactor))
                {                    
                    detectableFactor = entityTypeDetectionFactor.Value;
                    
                    distanceToAlwaysDetect = entityTypeDetectionFactor.DistanceToAlwaysDetect ?? GameData.Instance.Constants.DefaultDistanceToAlwaysDetectHiddenEntities;

                    requiresExamineAction = entityTypeDetectionFactor.RequiresExamineAction;

                    if (entityTypeDetectionFactor.SkillToUse != null)
                    {
                        skillFactor = Parent.Intelligence.GetSkillValue(entityTypeDetectionFactor.SkillToUse);
                    }
                }
                else if (detection.DetectionDisabled)
                {
                    distanceToAlwaysDetect = 0f;
                    detectableFactor = 0f;
                }
                else
                {
                    distanceToAlwaysDetect = GameData.Instance.Constants.DefaultDistanceToAlwaysDetectHiddenEntities;
                    detectableFactor = 1f;
                }              
            }


            if (distanceToAlwaysDetect > 0f)
            {
                if (detectAllWhichHasMinimumRange
                    || distance < distanceToAlwaysDetect)
                {
                    return true;
                }
                
            }

            float sensorRange = Parent.GetDaySensorRange();

            float maxDistance = sensorRange - distanceToAlwaysDetect;
            float distanceBeyondMinDistance = distance - distanceToAlwaysDetect;

              //normalized to 1 at min distance, 0 at max distance:
            float distanceFactor;

            // use different fall-off methods for the distance factor... resources should be harder to spot when at the max distance
            if (detectable.ResourceType != null)
            {
                distanceFactor = MathHelper.Lerp(1f, 0f, distanceBeyondMinDistance / maxDistance);

                distanceFactor = distanceFactor * distanceFactor; // for resources, use a square function to get faster falloff = even lower chances further away.           
            }
            else 
            {
                 // for entities, use an inverted square function to get slow falloff from 1 at start, then more rapid falloff to 0 further away.
                distanceFactor = MathHelper.Lerp(0f, 1f, distanceBeyondMinDistance / maxDistance);
                distanceFactor = 1f - (distanceFactor * distanceFactor);
            }

            distanceFactor = Common.Clamp(distanceFactor, 0f, 1f);

            float agentDetectionFactor = Parent.GetDetectionFactor(detectable, requiresExamineAction);
         
            if (agentDetectionFactor > 0f)
            {

            }

            float chanceToDetect = distanceFactor * (1f - stealthFactor) * agentDetectionFactor * skillFactor * detectableFactor 
                + GameData.Instance.AIConstants.DetectionBonusForRememberedEntitiesInSameSpot * priorKnowledgeBonus;

            
            // if this is 'too random', we can get a higher chance for a middle result if we instead make 2 random throws and add them with half weight.
            return The.Sim.GameplayRandomGenerator.NextDouble("Sensor") <= chanceToDetect;
        }


        #region ISnapshot

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            tilesCurrentlySeen = new HashSet<TerrainTile>();
            foreach (var item in snapshotCurrentlySeen)
            {
                tilesCurrentlySeen.Add(LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(item));
            }

            //CreateRegulators();
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.IsActive = sn.DoBool(this.IsActive);
            this.oldSensorRadiusInTiles = sn.DoInt32Nullable(this.oldSensorRadiusInTiles);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotCurrentlySeen = tilesCurrentlySeen.Select(t => t.ID).ToList();
            }
            snapshotCurrentlySeen = sn.DoList(snapshotCurrentlySeen);

           
            sn.Ignore(allDetectables);
          
            sn.Ignore(tilesToSee);
            sn.Ignore(tilesToUnsee);
            sn.Ignore(tilesCurrentlySeen);

           /* sn.Ignore(newTiles);
            sn.Ignore(oldTiles);
            sn.Ignore(oldOldTiles);
            */
            return this;
        }

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

        #endregion
    }
}

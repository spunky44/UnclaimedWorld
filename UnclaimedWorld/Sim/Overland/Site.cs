using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using WindowSystem;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Trees;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.Control;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Templates;
namespace UWGame.SimSide.Overland
{

    public enum SiteID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// one Site is the local game map. It can contain both the player's faction (allegiance) and others as well...
    /// </summary>
    public class Site : IHasExposedProperties, ISnapshot, ILookUp<Site, SiteID>
    {
        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();


        public string Name;
        private string keyName;

        public string Description;

        private bool isPlaySite = false;
        public bool IsPlaySite
        {
            get
            {
                return isPlaySite;
            }
            set
            {
                isPlaySite = value;
                if (isPlaySite)
                {
                    The.Sim.PlaySite = this;
                }
            }
        }

        // 
        #region client stuff
        public bool ShowLabel = true;
        public bool ShowTallPin = true;

        public int SiteMarkerOrder = 0;
        #endregion

        public GeodeticCoordinate Coords;

      
        // these are the allegiances present on the Site
       // public Dictionary<string, Allegiance.Allegiance> Allegiances = new Dictionary<string, Allegiance.Allegiance>();
        public List<Allegiances.Allegiance> Allegiances = new List<Allegiances.Allegiance>();
        List<AllegianceID> snapshotAllegiances;

        /// <summary>
        /// is null on all other sites than PlaySite
        /// TODO: Delete this
        /// </summary>
        public Allegiances.Allegiance PlayerAllegiance;
        AllegianceID? snapshotPlayerAllegiance;

        public List<ThreatGroup> ThreatGroups = new List<ThreatGroup>();
        List<ThreatGroupID> snapshotThreatGroups;
               
        /// <summary>
        /// I don't see a valid use for this collection. It seems to be misused. Allegiances should be referenced
        /// </summary>
      /*  public List<Entity> Persons = new List<Entity>();
        List<EntityID> snapshotPersons;
        */

        public HashSet<EntityID> AllEntities = new HashSet<EntityID>();
     
        public ObservableList<Entity> Entities = new ObservableList<Entity>();
        List<EntityID> snapshotEntities;

      /*  public ObservableList<Entity> DegradableEntities = new ObservableList<Entity>();
        List<EntityID> snapshotDegradableEntities;
        */


        /// <summary>
        /// perhaps move this to World instead..?
        /// </summary>
        public Dictionary<string, EntityID> EntitiesByName = new Dictionary<string, EntityID>();
        public Dictionary<string, List<EntityID>> EntitiesByType = new Dictionary<string, List<EntityID>>();

        /// <summary>
        /// used in drawing influence maps for crop harvesters... maybe overkill
        /// 
        /// can be used by map editor too
        /// 
        /// we can see all trees by satellite!!!    
        /// we know all trees, but not their state, or current crops...
        /// </summary>
        public Dictionary<ResourceType, ObservableList<ResourceContainer>> Resources = new Dictionary<ResourceType, ObservableList<ResourceContainer>>();
        Dictionary<ResourceType, List<ResourceID>> snapshotResources;

        /// <summary>
        /// editor only
        /// 
        /// should contain crops and terrain resources that can have their probabilities set
        /// </summary>
        public HashSet<ResourceType> EditorResources = new HashSet<ResourceType>();

       // public Dictionary<ResourceType, List<>> DesignerPlacedResources 

        /// <summary>
        /// TODO: use this for data that only a playsite needs
        /// </summary>
        public PlaySite PlaySite;


        public EventManager EventManager;
        private CyclableID eventManagerID;



        public Site(string keyName, bool isPlaySite = false)
        {
            this.keyName = keyName;

            AddToLookup();

            // TODO: make it possible to switch playsite true/false
            // use same pattern as Allegiance

            //This needs to be done before iterating the allegiances as the CreateFromAllegianceData method depends on it.
            The.Sim.World.AllSites.Add(keyName, this);

            this.IsPlaySite = isPlaySite;

            EventManager = new EventManager(this); // NEW: allow events on othersites too... only a subset of event actions can be used though.
            eventManagerID = EventManager.ID;

            if (isPlaySite == true) 
            {
                PlaySite = new Overland.PlaySite();

                Allegiances = The.Sim.PlaySite.Allegiances;
               
              
            }
                
        }

       

        public Site()
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        static Site()
        {
            exposedPropertyValueFunctions.Add("getJournalHeaderNames", GetJournalHeaderNames);
          //  exposedPropertyValueFunctions.Add("getRandomName", GetRandomName);
            exposedPropertyValueFunctions.Add("getJournalDate", GetJournalDate);  // DATE: 06-10 2238 
            exposedPropertyValueFunctions.Add("getDate", GetDate);            
            exposedPropertyValueFunctions.Add("getLocation", GetLocation);
            exposedPropertyValueFunctions.Add("getTime", GetElapsedTime);
            exposedPropertyValueFunctions.Add("getRandomSimNumber", GetRandomSimNumber);
            exposedPropertyValueFunctions.Add("getRandomClientNumber", GetRandomClientNumber);
            

        }

        /// <summary>
        /// Creates a Site from a SiteData
        /// </summary>
        public static Site CreateFromSiteData(SiteData siteData, GeodeticCoordinate coords)
        {
            Site site = new Site(siteData.KeyName, siteData.IsPlaySite)
            { 
                Name = siteData.Name,
                keyName = siteData.KeyName,
                Description = siteData.Description,
                Coords = coords, // siteData.Coords,
                ShowLabel = siteData.ShowLabel,
                ShowTallPin = siteData.ShowTallPin,
                SiteMarkerOrder = siteData.SiteMarkerOrder
            };


           
            if (siteData.SiteTemplates != null)
            {
                // NEW: fill from template, 
                int index;
                StringChance trait = Common.GetStairStepIndex(siteData.SiteTemplates, out index, The.Sim.GameplayRandomGenerator);

                SiteTemplate template = GameData.Instance.AllSiteTemplates[trait.String];

                template.FillSite(site, siteData.AllegianceKeyName, siteData.ExpeditionKeyName);

            }

           /* if (siteData.Allegiances != null)
            {
                // NEW, spawn allegiances
                foreach (var item in siteData.Allegiances)
                {
                    int index;
                    StringChance trait = Common.GetStairStepIndex(item.Chances, out index, The.Sim.GameplayRandomGenerator);

                    AllegianceData allegianceData = GameData.Instance.AllAllegianceData[trait.String];
                    //allegianceData.Site = siteData.Key;
                    Allegiance allegiance = Allegiance.CreateFromAllegianceData(allegianceData, site); // this can create exeditions in turn                    
                }
            }*/


            return site;

           /*
            // OLD:
            if (siteData.Allegiances == null)
            {
                siteData.Allegiances = new AllegianceData[] { }; //???
            }
            else
            {
                foreach (AllegianceData allegianceData in siteData.Allegiances)
                {
                    allegianceData.Site = siteData.Key;
                    Allegiance.CreateFromAllegianceData(allegianceData);
                }
            }

            return site;*/
        }


        
       

        /// <summary>
        /// HACK - should be picked by closeness, or other criteria...
        /// </summary>
        /// <returns></returns>
        public Expedition GetFirstPlayerExpedition()
        {

            if (PlayerAllegiance != null)
            {
                if (PlayerAllegiance.Expeditions.Count > 0)
                {
                    return PlayerAllegiance.Expeditions[0];
                }
                else
                {
                    //return ExpeditionFactory.Produce(MapManager.TileToWorldPos(new Point(10, 10)), "Camp", PlayerAllegiance);
                    return null;
                }
            }
            else return null;
        }

       /* public Expedition GetExpeditionByName(string name)
        {
            foreach (Allegiance allegiance in Allegiances)
            {
                foreach (Expedition expedition in allegiance.Expeditions)
                {
                    if (expedition.Name == name)
                    {
                        return expedition;
                    }
                }
            }
            return null; 
        }*/

        /// <summary>
        /// loops over all entities, calling Geometry.Place (moved from 1st Update frame)
        ///  if run on all terrain during the first frame, it makes the screen go black for 7 secs. 47 secs on map 4...
        ///  
        /// needed because footprint is dirty, neighbours need to be asked to restamp...
        /// </summary>
        public bool PlaceAllGeometry(ref int geoLayoutEntityProgress, int entitiesPerCycle)
        {
            if (isPlaySite)
            {
                int max = Math.Min(geoLayoutEntityProgress + entitiesPerCycle, Entities.Count); // 24k entities

                Entity entity;
                for (int i = geoLayoutEntityProgress; i < max; i++)
                {
                    entity = Entities[i];

                    // entity.SetNotDirty();
                    // only needed because some geos can stamp a passable cost over a blocked subtile during Place(GeoPlaceMode.New)
                    // can we optimize this, perhaps by checking 
                    entity.PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged); 
                }

                if (max == Entities.Count)
                {
                    geoLayoutEntityProgress = 0;
                    return true;
                }
                else
                {
                    geoLayoutEntityProgress = max;
                    return false;
                }
            }

            return true;
        }

        /*
        public bool PlaceAllGeometry()
        {
            if (isPlaySite)
            {
                Entity entity;

                for (int i = Entities.Count - 1; i >= 0; i--)
                {
                    entity = Entities[i];

                    // entity.SetNotDirty();
                    // only needed because some geos can stamp a passable cost over a blocked subtile during Place(GeoPlaceMode.New)
                    // can we optimize this, perhaps by checking 
                    entity.PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged);
                }
            }

            return true;
        }*/

        public void InitAuxiliaryMaps()
        {
            if (isPlaySite)
            {
                foreach (var item in Allegiances)
                {
                    item.SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();  //InitPlaySite(true);
                }
            }
        }

        public void Update(GameTime gameTime)
        {                      
           
            foreach (Allegiances.Allegiance allegiance in Allegiances)
            {
                allegiance.Update(gameTime); // TODO: put in sleepyUpdater
            }

            EventManager.Update(gameTime);

            if (PlaySite != null)// editor mode??
            {
                PlaySite.Update(gameTime);
            }          
        }

        /*
        public ActionSetData GetActionSetData(ActionSetType actionSet)
        {
            ActionSetData data;
            if (!ActionSetData.TryGetValue(actionSet, out data))
            {
                data = new ActionSetData(actionSet);
                ActionSetData.Add(actionSet, data);
            }

            return data;
        }
        */

        public void RemoveEntity(Entity entity)
        {
            AllEntities.Remove(entity.ID);
            Entities.Remove(entity);

            List<EntityID> entitiesOfType;
            if (EntitiesByType.TryGetValue(entity.EntityType.KeyName, out entitiesOfType))
            {
                entitiesOfType.Remove(entity.EntityID);
            }

           /* if (entity.EntityType.Person != null)
            {
                Persons.Remove(entity);
            }*/

            if (!string.IsNullOrEmpty(entity.Name))
            {
                EntitiesByName.Remove(entity.Name);
            }
                     

        }

        /// <summary>
        /// parts will also be added
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(Entity entity)
        {
            if (AllEntities.Contains(entity.ID))
                return;

            AllEntities.Add(entity.ID);
            Entities.Add(entity);

            if (!string.IsNullOrEmpty(entity.Name))
            {
                if (!EntitiesByName.ContainsKey(entity.Name))
                {
                    EntitiesByName.Add(entity.Name, entity.EntityID);
                }
            }

            Common.AddToMultiList(EntitiesByType, entity.EntityType.KeyName, entity.EntityID);


          /*  if (entity.EntityType.Person != null)
            {
                Persons.Add(entity);
            }*/

          
        }


        /// <summary>
        /// registers a resource container for iterating convenience
        /// </summary>
        /// <param name="resourceContainer"></param>
        public void AddResourceContainer(ResourceContainer resourceContainer)
        {
            ObservableList<ResourceContainer> listOfContainers;
            if (!Resources.TryGetValue(resourceContainer.ResourceType, out listOfContainers))
            {           
            
                listOfContainers = new ObservableList<ResourceContainer>();
                Resources.Add(resourceContainer.ResourceType, listOfContainers);
            }
            
            listOfContainers.Add(resourceContainer);
        }

        public string GetCaption(string captionKey)
        {
            return null;
        }

        


        #region Property methods


        public static PropertyResult? GetJournalHeaderNames(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetJournalHeaderNames();
        }

        /// <summary>
        /// should be in Expedition instead
        /// </summary>
        /// <returns></returns>
        public PropertyResult? GetJournalHeaderNames()
        {
            // RECORDED BY: Glen Tarkov\n PRESENT: Ward Conlan, Josie Kane and Kurt Mansell\n
            List<Entity> persons = new List<Entity>(PlayerAllegiance.MembersList);

            AgentCondition canParticipate = UnhappinessGroupMeetingEvent.GetMeetingParticipantConditions();


            persons = persons.FindAll(e => e.PersonEntity != null && e.Site == The.Sim.PlaySite
                && canParticipate.IsFulfilled(e));

            PropertyResult result = new PropertyResult();
           

            if (persons.Count > 0)
            {
                Entity randomPerson = Common.GetRandomListMember(persons, The.Sim.GameplayRandomGenerator);


                List<Entity> rest = persons.FindAll(e => e != randomPerson);

                string namesOfRest = Common.ListToCommaSeparatedString(rest,
                    e => SubstituteValue.FormatEntity(e)); //e.Name);

                string stringResult = string.Format("RECORDED BY: {0}", SubstituteValue.FormatEntity(randomPerson));

                if (!string.IsNullOrEmpty(namesOfRest))
                {
                    stringResult += string.Format(" \nPRESENT: {0}", namesOfRest);
                }

                result.StringResult = stringResult;

            }

            return result;
        }

       /* public static PropertyResult? GetRandomName(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetRandomName();
        }

        public PropertyResult? GetRandomName()
        {
            Entity randomPerson = Common.GetRandomListMember(Persons, The.Sim.GameplayRandomGenerator);

            PropertyResult result = new PropertyResult()
            {
                StringResult = randomPerson.Name
            };

            return result;
        }*/

        static char[] separator = new char[] { ':' };

        public static void GetEntities(IHasExposedProperties source, FilterCondition /* PropertyCondition*/ filter, bool onlyFinished, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            Site site = (Site)source;

            PropertyCondition propertyCondition = filter as PropertyCondition;

            if (propertyCondition != null)
            {
                if (propertyCondition.PropertyKey == "name")
                {
                    EntityID entityID;
                    if (site.EntitiesByName.TryGetValue(propertyCondition.ConstantStringEqual, out entityID))
                    {
                        Entity entity = Entity.FindByID(entityID);
                        if (entity != null)
                        {
                            listToFillWithProperties.Add(entity);
                        }

                    }

                    wasFiltered = true;
                }
                else if (propertyCondition.PropertyKey == "ID")
                {
                    EntityID entityID;

                    // evaluate filter expression to get the ID
                    // we use string instead of float...
                    string entityIDString = propertyCondition.GetStringCompareValue(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                    if (entityIDString != null)
                    {
                        entityID = (EntityID)long.Parse(entityIDString);

                        Entity entity = Entity.FindByID(entityID);

                        if (entity != null)
                        {
                            listToFillWithProperties.Add(entity);
                        }

                    }

                    wasFiltered = true;
                }
                else if (propertyCondition.PropertyKey == "type")
                {
                    List<EntityID> list;
                    if (site.EntitiesByType.TryGetValue(propertyCondition.ConstantStringEqual, out list))
                    {
                        foreach (var item in list)
                        {
                            Entity entity = Entity.FindByID(item);
                            if (entity != null)
                            {
                                listToFillWithProperties.Add(entity);
                            }
                        }

                    }

                    wasFiltered = true;
                }
                else
                {
                    // return the full list:            
                    listToFillWithProperties.AddRange(site.Entities.GetAsList());

                    wasFiltered = false;
                }
            }
            else
            {
                // return the full list:            
                listToFillWithProperties.AddRange(site.Entities.GetAsList());

                wasFiltered = false;
            }


            if (onlyFinished)
            {
                listToFillWithProperties.RemoveAll(i => !((Entity)i).IsCompleted());
            }
        }

        public static void GetAllegiances(IHasExposedProperties source, /*string filterKey,*/ ref List<IHasExposedProperties> listToFillWithProperties)
        {
            Site site = (Site)source;
         /*   if (filterKey != null && filterKey.StartsWith("name:")) // "name:playerAllegiance"
            {
                string[] parts = filterKey.Split(separator);
                string name = parts[1];
                name = name.Trim();

                Allegiance allegiance = site.Allegiances.FirstOrDefault(a => a.Name == name);

                if (allegiance != null)
                {
                    listToFillWithProperties.Add(allegiance);

                }

                //wasFiltered = true;
            }
            else
            {*/
                listToFillWithProperties.AddRange(site.Allegiances);

               // wasFiltered = false;
           // }
        }

        #endregion

        #region IHasExposedProperties

        public string KeyName
        {
            get { return keyName; }
        }

        public void GetChildren(string keyToList, ref List<IHasExposedProperties> resultList, FilterCondition /* PropertyCondition*/ filter, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            SharedKnowledge getterKnowledge = null) //string filterPropertyKey, string filterPropertyValue)
        {
            bool wasFiltered = false;   
            switch (keyToList)
            {
                case "entities":
                    GetEntities(this, filter, false, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                    break;
                case "finishedEntities": // for convenience, instead of a filter...
                    GetEntities(this, filter, true, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                    break;
                case "allegiances":
                    GetAllegiances(this, ref resultList);

                    break;
            }

         
         //   getListFunction.Invoke(this, filterKey, ref resultList);
            

            // then apply filter:
            if (wasFiltered == false 
                && filter != null)
            {
                FilterChildren(resultList, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }     

        }

        /// <summary>
        /// warning - this will probably cause delays if run often on the entity collection. Make optimized collections instead...
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="filter"></param>
        public static void FilterChildren(List<IHasExposedProperties> resultList, FilterCondition filter, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            resultList.RemoveAll(h => !filter.IsFulfilled(h, triggeringEntity, targetEntity, polledEventSource, dynamicTarget));
        }

        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            PropertyResult? result = null;
            PropertyResult customResult;

            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
            {
                result = customResult;
            }

            return result;
        }

       
        public string GetDefaultCaption(string propertyKey)
        {
            return Name;
        }

        public void GetDefaultKey(out string propertyKey)
        {
            propertyKey = keyName;
        }

        public EntityID? GetEntityID()
        {
            return null;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return true; 
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);

        }

        private Dictionary<string, PropertyResult> customFields;

        #endregion

        public static PropertyResult? GetLocation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetLocation();
        }

        private PropertyResult? GetLocation()
        {
            return new PropertyResult() { StringResult = Coords.ToString() };
        }

        public static PropertyResult? GetJournalDate(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetJournalDate();
        }

        private PropertyResult? GetJournalDate()
        {
            // DATE: 06-10 2238        
            return new PropertyResult() { StringResult = The.Sim.DateAndTime.CurrentTimeDateYear.GetDateForJournal() };
        }

        public static PropertyResult? GetDate(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetDate();
        }

        private PropertyResult? GetDate()
        {           
            return new PropertyResult() { DateResult = The.Sim.DateAndTime.CurrentTimeDateYear };
        }


       

        public static PropertyResult? GetElapsedTime(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetElapsedTime();
        }

        private PropertyResult? GetElapsedTime()
        {
            return new PropertyResult() { NumberResult = (float)The.Sim.TotalUnPausedGameTimeInSeconds };
        }

        public static PropertyResult? GetRandomSimNumber(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetRandomSimNumber();
        }

        private PropertyResult? GetRandomSimNumber()
        {
            return new PropertyResult() { NumberResult = (float)The.Sim.GameplayRandomGenerator.NextDouble("getRandomSimNumber") };
        }

        public static PropertyResult? GetRandomClientNumber(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Site)hasExposed).GetRandomClientNumber();
        }

        private PropertyResult? GetRandomClientNumber()
        {
            if (The.Client != null)
            {
                return new PropertyResult() { NumberResult = (float)The.Client.ClientRandomGenerator.NextDouble("getRandomClientNumber") };
            }
            else
            {
                return new PropertyResult() { NumberResult = 0f };
            }
        }
        /// <summary>
        /// returns a String
        /// </summary>
        /// <returns></returns>
        
        //promoted to special variable
      /*  private PropertyResult? GetLastSpawnedEntityID()
        {
            
            if (LastSpawnedEntity.HasValue)
            {
                return new PropertyResult() { StringResult = ((long)this.LastSpawnedEntity.Value).ToString() };
            }
            else 
            {
                return null;
            }
        }*/

        
        
        public void PrintGlobalScriptVariables(System.Text.StringBuilder description)
        {
            if (customFields != null)
            {
                foreach (var item in customFields)
                {
                    description.AppendLine(item.Key + " = " + item.Value.ToString());
                }
            }

        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = (SiteID)sn.DoEnum(IDCounter);

            this.Name = sn.DoString(Name);
            this.keyName = sn.DoString(keyName);

            this.Description = sn.DoString(Description);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotAllegiances = Allegiances.Select(a => a.ID).ToList();

             //   this.snapshotPersons = Persons.Select(e => e.ID).ToList();

                this.snapshotEntities = Entities.GetAsList().Select(e => e.EntityID).ToList();
              
                snapshotResources = new Dictionary<ResourceType,List<ResourceID>>();
                foreach (var item in Resources)
                {
                    snapshotResources.Add(item.Key, item.Value.GetAsList().Select(r => r.ID).ToList()); // save the entry ids
                    item.Value.Clear(); // remove references before snapshotting
                }

                snapshotThreatGroups = ThreatGroups.Select(t => t.ID).ToList();
                              
            }

            this.PlaySite = (PlaySite)sn.DoISnapshot(PlaySite);
            this.snapshotAllegiances = sn.DoList(snapshotAllegiances);
            this.customFields = sn.DoDictionary(customFields);
            this.snapshotPlayerAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(PlayerAllegiance);
         //   this.snapshotPersons = sn.DoList(snapshotPersons);
            this.Entities.Clear(); // remove entity references before snapshotting
            this.Entities = (ObservableList<Entity>)sn.DoISnapshot(Entities);
            this.AllEntities = sn.DoHashSet(AllEntities);
           /* this.DegradableEntities.Clear(); // remove entity references before snapshotting
            this.DegradableEntities = (ObservableList<Entity>)sn.DoISnapshot(DegradableEntities); 
            this.snapshotDegradableEntities = sn.DoList(snapshotDegradableEntities);*/
            this.EntitiesByName = sn.DoDictionary(EntitiesByName);
            this.EntitiesByType = sn.DoMultiMap(EntitiesByType);
            this.snapshotEntities = sn.DoList(snapshotEntities);
           
            this.snapshotThreatGroups = sn.DoList(snapshotThreatGroups);
            this.Resources = sn.DoDictionary(Resources);
            this.snapshotResources = sn.DoMultiMap(snapshotResources);
            
            this.isPlaySite = sn.DoBool(isPlaySite);
            this.Coords = sn.DoGeodeticCoordinate(Coords); // (GeodeticCoordinate)sn.DoISnapshot(Coords);
            this.ShowLabel = sn.DoBool(ShowLabel);
            this.ShowTallPin = sn.DoBool(ShowTallPin);
            this.SiteMarkerOrder = sn.DoInt32(SiteMarkerOrder);

            #region ICyclables - only Ids

            eventManagerID = (CyclableID)sn.DoEnum(eventManagerID);
          
            #endregion


            sn.Ignore(separator);          
            sn.Ignore(EventManager);        
            sn.Ignore(PlayerAllegiance);
            sn.Ignore(Allegiances);           
            sn.Ignore(ThreatGroups);
            sn.Ignore(exposedPropertyValueFunctions); // handled in static ctor
            sn.Ignore(EditorResources);


            return this;
        }


       /* public override void Init()
        {
            Map.Init();
        }*/

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Entities.AddRange(snapshotEntities.Select(e => Entity.FindByID(e)));
            Entities.LoadPostProcess(sn);

            /*
            DegradableEntities.AddRange(snapshotDegradableEntities.Select(e => Entity.FindByID(e)));
            DegradableEntities.LoadPostProcess(sn);
            */

            if (snapshotResources != null)
            {
                foreach (var item in snapshotResources)
                {                   
                    Resources[item.Key].AddRange(item.Value.Select(r => LookUp<ResourceContainer, ResourceID>.FindByID(r)).ToList());                  
                }
                
                snapshotResources = null;
            }

            foreach (var item in Resources)
            {
                item.Value.LoadPostProcess(sn);
            }


            PlayerAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotPlayerAllegiance);
            Allegiances = snapshotAllegiances.Select(a => LookUp<Allegiance, AllegianceID>.FindByID(a)).ToList();

          /*  if (snapshotPersons != null)
            {
                Persons = snapshotPersons.Select(p => Entity.FindByID(p)).ToList();
            }*/

            if (snapshotThreatGroups != null)
            {
                ThreatGroups = snapshotThreatGroups.Select(t => LookUp<ThreatGroup, ThreatGroupID>.FindByID(t)).ToList();
            }

            if (PlaySite != null)
            {
                PlaySite.LoadPostProcess(sn);
            }
           
            #region ICyclables

            EventManager = (EventManager)LookUp<ICyclable, CyclableID>.FindByID(eventManagerID);
          
            #endregion

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


        #region ILookup

        private SiteID id = SiteID.Invalid;
        static SiteID IDCounter = SiteID.First;
       
        public SiteID ID
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

        public SiteID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SiteID.Max)
            {
                throw new Exception("Astounding, SiteID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public SiteID SnapshotID(Snapshotter sn, SiteID id)
        {
            return (SiteID)sn.DoEnum(id);
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
            if (ID != SiteID.Invalid)
                LookUp<Site, SiteID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = SiteID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Site, SiteID>.Remove(this);
        }

        void ILookUp<Site, SiteID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SiteID.First;
        }

        void ILookUp<Site, SiteID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Site, SiteID>.Create();
           // LookUp<Site, SiteID>.SetLoadPostProcessOrder(15);  
        
        }


        #endregion
    }
}

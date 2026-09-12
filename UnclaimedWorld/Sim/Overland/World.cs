using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Overland
{
    public enum RouteType { Land, CalmWater, Sea }

    public class World : ISnapshot, IHasExposedProperties
    {

        public Dictionary<string, Site> AllSites = new Dictionary<string, Site>();
        List<SiteID> snapshotSites;


        /// <summary>
        /// a way for off-site entities to receive an Update...
        /// </summary>
        HashSet<EntityID> OffSiteEntities = new HashSet<EntityID>();
       // List<Entity> OffSiteEntities;


        /// <summary>
        /// also keep a reference to named entities here.
        /// </summary>
      //  public Dictionary<string, EntityID> EntitiesByName = new Dictionary<string, EntityID>();
      

        /// <summary>
        /// in km
        /// </summary>
        public double WorldRadius;

        /// <summary>
        /// these are used to limit the view of the world in the interface:
        /// </summary>
        public float ViewLatitudeStart, ViewLatitudeEnd, ViewLongitudeStart, ViewLongitudeEnd;

        /// <summary>
        ///  each relation is stored twice!
        /// </summary>
        public Dictionary<AllegianceID, List<AllegianceRelation>> Relations = new Dictionary<AllegianceID, List<AllegianceRelation>>();
        Dictionary<AllegianceID, List<AllegianceRelationID>> snapshotRelations = new Dictionary<AllegianceID, List<AllegianceRelationID>>();


       
        /// <summary>
        /// there can be more than one route between two sites, but each should have a different type
        /// we don't store air routes, it's better to just calculate them.       
        /// </summary>
       // public Dictionary<SiteID, Dictionary<SiteID, RouteID>> AllRoutes = new Dictionary<SiteID, Dictionary<SiteID, RouteID>>();
        public Dictionary<RouteType, Dictionary<SiteID, Dictionary<SiteID, RouteID>>> AllRoutes = new Dictionary<RouteType, Dictionary<SiteID, Dictionary<SiteID, RouteID>>>();
       // public Dictionary<RouteType, Dictionary<Site, Dictionary<Site, Route>>> AllRoutes = new Dictionary<RouteType, Dictionary<Site, Dictionary<Site, Route>>>();

        /// <summary>
        /// for scripting...
        /// </summary>
        public EntityID? LastSpawnedEntity;


        /// <summary>
        /// Creates a World from a WorldData
        /// </summary>
        public static World CreateFromWorldData(WorldData worldData)
        {
            World world = new World();

           /* foreach (SiteData siteData in worldData.Sites)
            {
                world.AllSites.Add(siteData.Key, Site.CreateFromSiteData(siteData));
            }*/

            world.WorldRadius = worldData.WorldRadius;

            world.ViewLatitudeStart = worldData.ViewLatitudeStart;
            world.ViewLatitudeEnd = worldData.ViewLatitudeEnd;
            world.ViewLongitudeStart = worldData.ViewLongitudeStart;
            world.ViewLongitudeEnd = worldData.ViewLongitudeEnd;

            return world;
        }


        public Allegiance GetAllegianceFromKey(string key)
        {
            foreach (Site site in AllSites.Values)
            {
                foreach (Allegiance allegiance in site.Allegiances)
                {
                    if (allegiance.KeyName == key)
                    {
                        return allegiance;
                    }
                }
            }

            return null;
        }


       
        public AllegianceRelation GetRelationBetweenAllegianceIDs(AllegianceID id1, AllegianceID id2)
        {
            List<AllegianceRelation> list;

            if (Relations.TryGetValue(id1, out list))
            {
                AllegianceRelation relation = list.FirstOrDefault(
                    a => a.AllegianceA.ID == id2 // test both ends...
                    || a.AllegianceB.ID == id2); 

                return relation;
            }

            return null;

          /*  foreach (KeyValuePair<int, AllegianceRelation> relationEntry in Relations)
            {
                if (relationEntry.Key == id1 && relationEntry.Value.AllegianceB.ID == id2)
                {
                    return relationEntry.Value;
                }
                if (relationEntry.Key == id2 && relationEntry.Value.AllegianceB.ID == id1)
                {
                    return relationEntry.Value;
                }
            }*/

           // return null;
        }


        public void RemoveRelation(AllegianceID allegianceID)
        {
            List<AllegianceRelation> relations;
            if (The.Sim.World.Relations.TryGetValue(allegianceID, out relations))
            {
                AllegianceRelation relation;
                for (int i = relations.Count - 1; i >= 0; i--)
                {
                    relation = relations[i];
                    relations.RemoveAt(i);

                    // remove the other end also:
                    List<AllegianceRelation> otherRelations;
                    if (The.Sim.World.Relations.TryGetValue(relation.AllegianceB.ID, out otherRelations))
                    {
                        otherRelations.Remove(relation);
                    }

                    relation.Destroy();
                }
               
            }
        }

        public void AddEntityBetweenSites(Entity entity)
        {
            this.OffSiteEntities.Add(entity.ID);
        }

        public void RemoveEntityFromBetweenSites(Entity entity)
        {
            this.OffSiteEntities.Remove(entity.ID);
        }

        /// <summary>
        /// Returns a list of all contracts related to an allegiance.
        /// </summary>
     /*   public List<ContractOLD> GetContractsFromAllegiance(AllegianceID allegianceID)
        {
            List<ContractOLD> contractList = new List<ContractOLD>();
            List<AllegianceRelation> allegianceList = new List<AllegianceRelation>();

            Relations.TryGetValue(allegianceID, out allegianceList);

            foreach (AllegianceRelation relation in allegianceList)
            {
                contractList.AddRange(relation.Contracts);
            }

            return contractList;
        }*/


        public Site GetPlaySite()
        {
            foreach (Site site in AllSites.Values)
            {
                if (site.IsPlaySite)
                {
                    return site;
                }
            }

            return null;
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

            foreach (var site in AllSites)
            {
                site.Value.Update(gameTime);
            }

        }

        public double GetAirDistance(GeodeticCoordinate coords1, GeodeticCoordinate coords2)
        {
            // get the surface distance:
            return DistanceCalculator.Haversine(coords1, coords2, WorldRadius);

        }

        /// <summary>
        ///  Also returns an entry for 'Air' even though it does not have a route...
        /// </summary>
        /// <param name="fromSite"></param>
        /// <param name="toSite"></param>
        /// <returns></returns>
        public List<Tuple<Route, double>> GetRoutesAndDistances(Site fromSite, Site toSite) //, 
        // /* TravelLocation from, TravelLocation to,*/ out double airDistance)
        {
           // List<Tuple<RouteType?, double>> allRoutes = new List<Tuple<RouteType?, double>>();
            List<Tuple<Route, double>> allRoutes = new List<Tuple<Route, double>>();

            double airDistance = The.Sim.World.GetAirDistance(fromSite.Coords, toSite.Coords);

          //  allRoutes.Add(new Tuple<RouteType?, double>(null, airDistance));
            allRoutes.Add(new Tuple<Route, double>(null, airDistance));

            foreach (var item in Enum.GetValues(typeof(RouteType)))
            {
                RouteType routeType = (RouteType)item;
                Dictionary<SiteID, Dictionary<SiteID, RouteID>> routesOfType;

                if (The.Sim.World.AllRoutes.TryGetValue(routeType,
                    out routesOfType))
                {
                    Dictionary<SiteID, RouteID> routesFromSite;
                    if (routesOfType.TryGetValue(fromSite.ID, out routesFromSite))
                    {
                        RouteID routeID;
                        if (routesFromSite.TryGetValue(toSite.ID, out routeID))
                        {
                            Route route = LookUp<Route, RouteID>.FindByID(routeID);
                            allRoutes.Add(new Tuple<Route, double>(route, route.Length));
                           // allRoutes.Add(new Tuple<RouteType?, double>(routeType, route.Length));
                        }
                    }
                }
            }

            return allRoutes;

            //return null;

        }

        #region IHasExposedProperties

        const string keyName = "world";

        public string KeyName
        {
            get { return keyName; }
        }

        public void GetChildren(string keyToList, ref List<IHasExposedProperties> resultList, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            SharedKnowledge getterKnowledge = null) //string filterPropertyKey, string filterPropertyValue)
        {
            bool wasFiltered = false;
            switch (keyToList)
            {
                case "sites":
                    GetSites(this, filter, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                    break;   
            
               /* case "entities":
                    GetEntities(this, filter, false, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    break;*/
            }


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

        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge = null, IHasExposedProperties parent = null)
        {
            PropertyResult? result = null;
            
            /*
            switch (propertyKey)
            {
                case "getJournalHeaderNames":
                    return GetJournalHeaderNames();

                case "getRandomName":
                    return GetRandomName();
            }


            PropertyResult customResult;
            if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
            {
                result = customResult;
            }*/


            return result;

        }

        public string GetDefaultCaption(string propertyKey)
        {
            return "World";
        }

        public void GetDefaultKey(out string propertyKey)
        {
            propertyKey = keyName;
        }

        public string GetCaption(string captionKey)
        {
            return null;
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

        public static void GetSites(IHasExposedProperties source, FilterCondition filter, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            World world = (World)source;

            PropertyCondition propertyCondition = filter as PropertyCondition;
            if (propertyCondition != null)
            {
                if (propertyCondition.PropertyKey == "name")
                {
                    Site site;
                    if (world.AllSites.TryGetValue(propertyCondition.ConstantStringEqual, out site))
                    {
                        listToFillWithProperties.Add(site);
                    }

                    wasFiltered = true;

                    return;
                }               
            }

            // return the full list:            
            listToFillWithProperties.AddRange(world.AllSites.Values.ToList());

            wasFiltered = false;
        }

       /* public static void GetEntities(IHasExposedProperties source, PropertyCondition filter, bool onlyFinished, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            World world = (World)source;

            if (filter.PropertyKey == "name")
            {

            }
            else if (filter.PropertyKey == "ID")
            {


            }
            else if (filter.PropertyKey == "type")
            {

            }
            else
            {
                // return the full list:            
                listToFillWithProperties.AddRange(site.Entities.GetAsList());

                wasFiltered = false;
            }

        }*/


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotSites = new List<SiteID>();
                foreach (var item in AllSites)
                {
                    snapshotSites.Add(item.Value.ID);
                }


                snapshotRelations = new Dictionary<AllegianceID, List<AllegianceRelationID>>();
                foreach (var item in Relations)
                {
                    snapshotRelations.Add(item.Key, item.Value.Select(r => r.ID).ToList());
                }
            }

            this.snapshotRelations = sn.DoMultiMap(snapshotRelations);
            snapshotSites = sn.DoList(snapshotSites);
            this.WorldRadius = sn.DoDouble(WorldRadius);
            this.LastSpawnedEntity = sn.DoEnumNullable(LastSpawnedEntity);
            this.OffSiteEntities = sn.DoHashSet(OffSiteEntities);
            this.ViewLatitudeStart = sn.DoFloat(ViewLatitudeStart);
            this.ViewLatitudeEnd = sn.DoFloat(ViewLatitudeEnd);
            this.ViewLongitudeStart = sn.DoFloat(ViewLongitudeStart);
            this.ViewLongitudeEnd = sn.DoFloat(ViewLongitudeEnd);
            this.customFields = sn.DoDictionary(customFields);
            this.AllRoutes = sn.DoDoubleNestedDictionary(AllRoutes); // sn.DoNestedDictionary(AllRoutes);

            sn.Ignore(Relations);
            sn.Ignore(AllSites);

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


        public bool IsSnapshotted
        {
            get; set;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            AllSites.Clear();
            foreach (var item in snapshotSites)
            {
                Site site = LookUp<Site, SiteID>.FindByID(item);
                AllSites.Add(site.KeyName, site);
            }

            foreach (var item in snapshotRelations)
            {
                Relations.Add(item.Key, item.Value.Select(r => LookUp<AllegianceRelation, AllegianceRelationID>.FindByID(r)).ToList());
            }
            snapshotRelations = null;
        }


        #endregion
    }



}

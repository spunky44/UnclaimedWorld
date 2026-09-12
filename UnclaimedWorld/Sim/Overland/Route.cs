using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland
{
    public enum RouteID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// a shared instance like a region edge. do not save progress in this.
    /// </summary>
    public class Route: ISnapshot, ILookUp<Route, RouteID>
    {
        public Site Site1;
        SiteID snapshotSite1;

        public Site Site2;
        SiteID snapshotSite2;

        public float Length;

        public RouteType RouteType;

        public Route(Site fromSite, Site toSite, RouteType type, float length)
        {
            AddToLookup();

            Site1 = fromSite;
            Site2 = toSite;

            this.RouteType = type;
            this.Length = length;

            Dictionary<SiteID, Dictionary<SiteID, RouteID>> routes;
            if (!The.Sim.World.AllRoutes.TryGetValue(RouteType, out routes))
            {
                routes = new Dictionary<SiteID, Dictionary<SiteID, RouteID>>();
                The.Sim.World.AllRoutes.Add(RouteType, routes);
            }

            Common.AddToNestedDictionary(routes, Site1.ID, Site2.ID, this.ID);

        }



        public Route()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();
            }
        }

        /// <summary>
        /// Creates a 2-way route from a RouteData
        /// </summary>
        public static void CreateFromRouteData(RouteData routeData)
        {
            Site site1, site2;

            if (The.Sim.World.AllSites.TryGetValue(routeData.FromSite, out site1)
                && The.Sim.World.AllSites.TryGetValue(routeData.ToSite, out site2))
            {

                Route route1 = new Route(site1, site2, routeData.RouteType, routeData.Length);
                Route route2 = new Route(site2, site1, routeData.RouteType, routeData.Length);

            }
        }



        #region ILookup

        private RouteID id = RouteID.Invalid;
        static RouteID IDCounter = RouteID.First;

        public RouteID ID
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

        public RouteID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= RouteID.Max)
            {
                throw new Exception("Astounding, RouteID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public RouteID SnapshotID(Snapshotter sn, RouteID id)
        {
            return (RouteID)sn.DoEnum(id);
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
            if (ID != RouteID.Invalid)
                LookUp<Route, RouteID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = RouteID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Route, RouteID>.Remove(this);
        }

        void ILookUp<Route, RouteID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = RouteID.First;
        }

        void ILookUp<Route, RouteID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Route, RouteID>.Create();
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);
            snapshotSite1 = (SiteID)sn.SnapshotID<Site, SiteID>(Site1);
            snapshotSite2 = (SiteID)sn.SnapshotID<Site, SiteID>(Site2);
            Length = sn.DoFloat(Length);
            RouteType = sn.DoEnum(RouteType);

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
            
            Site1 = LookUp<Site, SiteID>.FindByID(snapshotSite1);
            Site2 = LookUp<Site, SiteID>.FindByID(snapshotSite2);
        }

        #endregion
    }
}

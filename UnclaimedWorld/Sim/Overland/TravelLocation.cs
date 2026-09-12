using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Overland
{
   /* public enum TravelLocationID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }*/

    /// <summary>
    /// made this a struct to make snapshotting simpler, also it seems logical with the way we are using the type
    /// </summary>
    public struct TravelLocation 
    {
        public long? AllegianceID
        {
            get
            {
                return allegianceID;
            }
        }
     
        private readonly long? allegianceID; 
     
        public long SiteID
        {
            get
            {
                return siteID;
            }
        }

        private readonly long siteID;

        public long? ExpeditionID
        {
            get
            {
                return expeditionID;
            }
        }

        private readonly long? expeditionID;


        private readonly long? terminalEntityID; 
     
        /// <summary>
        /// helipad, pier...
        /// </summary>
        public long? TerminalEntityID
        {
            get
            {
                return terminalEntityID;
            }
        }

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="original"></param>
        public TravelLocation(TravelLocation original)
        {
            this.allegianceID = original.AllegianceID;
            this.siteID = original.SiteID;
            this.expeditionID = original.ExpeditionID;
            this.terminalEntityID = original.TerminalEntityID;
        }

        public TravelLocation(long SiteID, long? AllegianceID, long? ExpeditionID, long? TerminalEntityID)
        {
            this.allegianceID = AllegianceID;
            this.siteID = SiteID;
            this.expeditionID = ExpeditionID;
            this.terminalEntityID = TerminalEntityID;
        }

        public TravelLocation(Allegiance allegiance, long? expeditionID, long? terminalEntityID)
        {
            this.expeditionID = expeditionID;
            this.terminalEntityID = terminalEntityID;
            this.allegianceID = (long)allegiance.ID;
            this.siteID = (long)allegiance.Site.ID;
          
        }

      
        
        /// <summary>
        /// until we develop MemoryFact classes for Expeditions and Allegiances, a failed lookup has to abort the mission immediately...
        /// </summary>
        /// <param name="location"></param>
        /// <param name="allegiance"></param>
        /// <param name="expedition"></param>
        /// <param name="terminal"></param>
        public bool ResolveLocation(SharedKnowledge sharedKnowledge, out Site site, out Allegiance allegiance, out Expedition expedition, out IKnownEntityData terminalData)
        {
            site = null;
            allegiance = null;
            expedition = null;
            terminalData = null;

            bool allItemsResolved = true;

            if (AllegianceID.HasValue)
            {
                allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);
                if (allegiance == null)
                {
                    allItemsResolved = false;
                }
            }

            site = LookUp<Site, SiteID>.FindByID((SiteID)SiteID);
            if (site == null)
            {
                allItemsResolved = false;                
            }

            if (ExpeditionID.HasValue)
            {
                expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
                if (expedition == null)
                {
                    allItemsResolved = false;
                }
            }

            if (TerminalEntityID.HasValue)
            {
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData((EntityID)TerminalEntityID, out terminalData)))
                {
                    allItemsResolved = false;
                }
               /* terminal = Entity.FindByID((EntityID)TerminalEntityID);
                if (terminal == null)
                {
                    allItemsResolved = false;    
                }*/
            }

            return allItemsResolved;
        }

      /*  public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                TravelLocation t = (TravelLocation)obj;
                return AllegianceID == t.AllegianceID
                    && ExpeditionID == t.ExpeditionID
                    && SiteID == t.SiteID
                    && TerminalEntityID == t.TerminalEntityID;
            }
        }*/

        public override bool Equals(Object obj)
        {
            return obj is TravelLocation && this == (TravelLocation)obj;
        }

        public override int GetHashCode()
        {
            // ??
            return AllegianceID.GetHashCode() ^ SiteID.GetHashCode() ^ expeditionID.GetHashCode() ^ terminalEntityID.GetHashCode();
        }

        public static bool operator ==(TravelLocation x, TravelLocation y)
        {
            return x.SiteID == y.SiteID 
                && x.AllegianceID == y.AllegianceID 
                && x.ExpeditionID == y.ExpeditionID 
                && x.TerminalEntityID == y.TerminalEntityID;
        }

        public static bool operator !=(TravelLocation x, TravelLocation y)
        {
            return !(x == y);
        }

        public static string GetKey(SiteID siteID, AllegianceID? allegianceID,  ExpeditionID? expeditionID, EntityID? terminal)
        {
            StringBuilder text = new StringBuilder();

            text.Append("Site:");
            text.Append(siteID.ToString());
            text.Append("All:");
            if (allegianceID.HasValue)
            {
                text.Append(((long)allegianceID.Value).ToString());
            }
            text.Append("Exp:");
            if (expeditionID.HasValue)
            {
                text.Append(((long)expeditionID.Value).ToString());
            } 
            text.Append("Term:");
            if (terminal.HasValue)
            {
                text.Append(((long)terminal.Value).ToString());
            }

            return text.ToString();
        }

        /*

    /// <summary>
    /// defines a stop on a mission
    /// 
    /// This is a "Value" class. Don't compare references, use Equals that will compare the members.
    /// 
    /// The intance itself has no meaning, only the IDs.
    /// 
    /// </summary>
    public class TravelLocation: ISnapshot 
    {
        public long? AllegianceID; // { get; private set; } 
     
        public long SiteID;

        public long? ExpeditionID;

        /// <summary>
        /// helipad, pier...
        /// </summary>
        public long? TerminalEntityID;

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="original"></param>
        public TravelLocation(TravelLocation original)
        {
            this.AllegianceID = original.AllegianceID;
            this.SiteID = original.SiteID;
            this.ExpeditionID = original.ExpeditionID;
            this.TerminalEntityID = original.TerminalEntityID;
        }

        
        public TravelLocation()
        {

        }
        
       

        public void SetAllegiance(Allegiance allegiance)
        {
            this.AllegianceID = (long)allegiance.ID;
            this.SiteID = (long)allegiance.Site.ID;
        }



        /// <summary>
        /// until we develop MemoryFact classes for Expeditions and Allegiances, a failed lookup has to abort the mission immediately...
        /// </summary>
        /// <param name="location"></param>
        /// <param name="allegiance"></param>
        /// <param name="expedition"></param>
        /// <param name="terminal"></param>
        public bool ResolveLocation(out Site site, out Allegiance allegiance, out Expedition expedition, out Entity terminal)
        {
            site = null;
            allegiance = null;
            expedition = null;
            terminal = null;

            bool allItemsResolved = true;

            if (AllegianceID.HasValue)
            {
                allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);
                if (allegiance == null)
                {
                    allItemsResolved = false;
                }
            }

            site = LookUp<Site, SiteID>.FindByID((SiteID)SiteID);
            if (site == null)
            {
                allItemsResolved = false;                
            }

            if (ExpeditionID.HasValue)
            {
                expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
                if (expedition == null)
                {
                    allItemsResolved = false;
                }
            }

            if (TerminalEntityID.HasValue)
            {
                terminal = Entity.FindByID((EntityID)TerminalEntityID);
                if (terminal == null)
                {
                    allItemsResolved = false; // TODO: use MemoryFacts and allow missions to destroyed terminals.                    
                }
            }

            return allItemsResolved;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                TravelLocation t = (TravelLocation)obj;
                return AllegianceID == t.AllegianceID
                    && ExpeditionID == t.ExpeditionID
                    && SiteID == t.SiteID
                    && TerminalEntityID == t.TerminalEntityID;
            }
        }


        public static string GetKey(SiteID siteID, AllegianceID? allegianceID,  ExpeditionID? expeditionID, EntityID? terminal)
        {
            StringBuilder text = new StringBuilder();

            text.Append("Site:");
            text.Append(siteID.ToString());
            text.Append("All:");
            if (allegianceID.HasValue)
            {
                text.Append(((long)allegianceID.Value).ToString());
            }
            text.Append("Exp:");
            if (expeditionID.HasValue)
            {
                text.Append(((long)expeditionID.Value).ToString());
            } 
            text.Append("Term:");
            if (terminal.HasValue)
            {
                text.Append(((long)terminal.Value).ToString());
            }

            return text.ToString();
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            AllegianceID = sn.DoInt64Nullable(AllegianceID);
            SiteID = sn.DoInt64(SiteID);
            ExpeditionID = sn.DoInt64Nullable(ExpeditionID);
            TerminalEntityID = sn.DoInt64Nullable(TerminalEntityID);



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


        }

        #endregion

        */
    }
}

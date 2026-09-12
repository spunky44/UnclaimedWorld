using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps
{
    public enum RegionSearchRequestID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// class that is meant to persist over region map redraws. so it does not contain region data, only coordinates.
    /// </summary>
    public class RegionSearchRequest : ISnapshot, ILookUp<RegionSearchRequest, RegionSearchRequestID>
    {
        public EntityID? Entity;
        public Point FromSubtile;
        public Point ToSubtile;


        // LOG: FOR DEBUG ONLY
        public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

        /// <summary>
        /// used to find a distance when FromSubtile is a blocked area.
        /// </summary>
        public bool UseClosestRegionToFromSubtile;

        //public RegionMap.NotifyWhenFinished NotifyWhenFinished;
        /*Action*/ 
        private MethodID? notifyWhenFinishedMethodID; // ID for callback method

        public bool SendMessageToEntityWhenDone = true;

        public RegionSearchRequest()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public RegionSearchRequest(EntityID? entityID, Point fromSubtile, Point toSubtile, bool sendMessageToEntity, MethodID? notifyWhenFinished)
        {
            AddToLookup();

            if (//IDName == "ExposedIndigHerbivoreCautiousFoot" && ID == (CyclableID)170
               //&& 
                The.Sim.TotalUnPausedGameTimeInSeconds > 35 //) // 35.6104778)
                && fromSubtile.X == 118 && fromSubtile.Y == 61 )
            {

            }

            Entity = entityID;
            FromSubtile = fromSubtile;
            ToSubtile = toSubtile;
            SendMessageToEntityWhenDone = sendMessageToEntity;
            notifyWhenFinishedMethodID = notifyWhenFinished;

            AddLog("Created");

        }

        public void AddLog(string text)
        {
#if DEBUG || PROFILE

            Log.Add(new Tuple<double, string>(The.Sim.TotalUnPausedGameTimeInSeconds, text));
#endif
        }

        public void Notify(Entity entity, RegionMap.Result result, float distance)
        {
            if (entity != null && SendMessageToEntityWhenDone) // sometimes a subsystem has requested the distances...
            {
                if (result == RegionMap.Result.OK)
                {
                    entity.SendMessage(new Message(Message.MessageTypes.DistanceFound) { OtherInfo = distance });
                }
                else if (result == RegionMap.Result.NoAccess)
                {
                    entity.SendMessage(new Message(Message.MessageTypes.DistanceFoundNoAccess) { OtherInfo = distance });
                }
            }

            if (notifyWhenFinishedMethodID.HasValue)
            {
                Action callback = ActionLookup.FindByID(notifyWhenFinishedMethodID.Value);
                if (callback != null)
                {
                    callback();
                }
            }

        }


        public void Destroy()
        {
            RemoveIDEntry();
        }

        #region ILookup

        private RegionSearchRequestID id = RegionSearchRequestID.Invalid;
        static RegionSearchRequestID IDCounter = RegionSearchRequestID.First;

        //=================== ILookup Methods =====================
        public RegionSearchRequestID ID
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

        public RegionSearchRequestID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= RegionSearchRequestID.Max)
            {
                throw new Exception("Astounding, RegionSearchRequestID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public RegionSearchRequestID SnapshotID(Snapshotter sn, RegionSearchRequestID id)
        {
            return (RegionSearchRequestID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != RegionSearchRequestID.Invalid)
                LookUp<RegionSearchRequest, RegionSearchRequestID>.Add(ID, this);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void SetInvalid()
        {
            id = RegionSearchRequestID.Invalid;
        }

        public void RemoveIDEntry()
        {
            // LookUp<Entity, EntityID>.Remove(ID);
            LookUp<RegionSearchRequest, RegionSearchRequestID>.Remove(this);
        }

        void ILookUp<RegionSearchRequest, RegionSearchRequestID>.ResetIDCounter() // does nothing.
        { }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = RegionSearchRequestID.First;
        }

        void ILookUp<RegionSearchRequest, RegionSearchRequestID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<RegionSearchRequest, RegionSearchRequestID>.Create();
        }

        #endregion

        #region ISnapshot
        //MethodID? notifyMethodID;

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);

            this.Entity = sn.DoEnumNullable(Entity);
            this.FromSubtile = sn.DoPoint(FromSubtile);
            this.ToSubtile = sn.DoPoint(ToSubtile);
            this.UseClosestRegionToFromSubtile = sn.DoBool(UseClosestRegionToFromSubtile);
            this.SendMessageToEntityWhenDone = sn.DoBool(SendMessageToEntityWhenDone);

            notifyWhenFinishedMethodID = sn.DoEnumNullable(notifyWhenFinishedMethodID);


            sn.Ignore(Log);

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

            //sn.FinalLoadProcess += new Action<Snapshotter>(sn_FinalLoadProcess);


        }

        void sn_FinalLoadProcess(Snapshotter sn)
        {
            // look up the delegate:
        /*    if (notifyMethodID.HasValue)
            {
                notifyWhenFinishedMethodID = (RegionMap.NotifyWhenFinished)DelegateLookup.FindByID(notifyMethodID.Value);
            }

            sn.FinalLoadProcess -= sn_FinalLoadProcess; // de-register..*/
        }

        #endregion
    }
}

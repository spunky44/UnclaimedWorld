using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;


namespace UWGame.SimSide.Resources
{

    public enum ResourceID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    public abstract class ResourceContainer : ILookUp<ResourceContainer, ResourceID>, ISnapshot, IDetectable
    {
        protected ResourceType resourceType;

        /// <summary>
        /// used for replenishing...
        /// </summary>
        private ResourceReplenish resourceReplenish;

       

        public ResourceType ResourceType
        {
            get
            {
                return resourceType;
            }
        }

        public ResourceContainer()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public ResourceContainer(ResourceType resourceType)
        {
            AddToLookup();
            
            this.resourceType = resourceType;

            if (resourceType.CanReplenish())
            {
                resourceReplenish = new ResourceReplenish();
            }

            SetNextReplenishTimepoint();
        }

        private void SetNextReplenishTimepoint()
        {
            if (resourceReplenish != null)
            {
                resourceReplenish.SetNextReplenishTimepoint(this);
                /*
                //DateAndTime.TimeDateYear date = resourceType.ComputeReplenishDate();
                double daysFromNow = resourceType.ComputeReplenishDaysFromNow();

                replenishTimePointInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds + daysFromNow * DateAndTime.secondsPerDay;

                // replenishTimePointInSeconds = date.ToSeconds();*/
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
        /// get replenish amount per year, from 0 (maximum) and from current state
        /// </summary>
        /// <returns></returns>
        public float GetMaximumRegrowth()
        {
            if (resourceReplenish != null)
            {
                return resourceReplenish.GetMaximumReplenishRate(this);
            }

            return 0f;
        }

        public float GetCurrentRegrowth(out bool maximumReached)
        {
            if (resourceReplenish != null)
            {
                return resourceReplenish.GetCurrentReplenishRate(this, out maximumReached);
            }

            maximumReached = true;
            return 0f;
        }

        public abstract Renderable Renderable { get; }

        public abstract float TotalHarvestableBulk { get; }

        public abstract Point MapPosition { get; }

        public abstract Vector3 Location { get; }

        public abstract Vector3 AccessPoint { get; }

        public abstract bool RequiresRollToDetect();
        public abstract EntityType EntityType
        {
            get;
        }

        public abstract bool IsIntelligent
        {
            get;
        }

        public abstract bool UsesMemory(SharedKnowledge sharedKnowledge);


        public abstract Entity ParentEntity { get; }

        //  Entity TargetedForHarvestingBy { get; set; }

        // Jobs.Job TargetedForHarvestingBy { get; set; }

        //List<IHarvestItem> CropItems { get; }

        public abstract int NoOfHarvestableItems { get; }


        //bool IsDestroyed { get; }
        public abstract bool IsDestroyed(SharedKnowledge knowledge);

        public abstract bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation);

        //  bool GatherResource();

        public abstract bool GatherResource(IResourceItem resourceItem);

        public abstract List<IResourceItem> ResourceItems { get; }

     //   protected abstract void UpdateBulkAndSprites();

        protected virtual void UpdateBulkAndSprites(int noOfItems)
        {
            if (resourceReplenish != null)
            {
                resourceReplenish.UpdateMaxItemsEverSet(noOfItems);
                
            }
        }

        //bool HasResourceItem(IResourceItem item);

       // protected 

        public abstract void SetResourceItems(int noOfItems);
       

        public virtual double? GetUpdateInterval()
        {
            
            double? tempInterval = null, currentInterval = null;

            if (resourceReplenish != null)
            {

                tempInterval = resourceReplenish.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }

            return currentInterval;
        }


        /// <summary>
        /// what about jobs/goals - is it safe?
        /// </summary>
        /// <param name="amounttoRemove"></param>
        public virtual void RemoveResourceItems(int amounttoRemove)
        {           
        }

        public virtual void AddResourceItems(int noOfItemsToAdd)
        {           
        }

      


        public virtual void Update(GameTime gameTime)
        {
            if (resourceReplenish != null)
            {
                resourceReplenish.Update(gameTime, this);
            }

           
        }

       


        public abstract IResourceItem FindHarvestableItem();

        /// <summary>
        /// this gets used to set the text on hyperlinks.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return resourceType.Name; // "ID: " + ID + " Type: " + resourceType.KeyName;
        }

        public string ToLink(bool useUpperCase = false)
        {
            // §C123¤Text§

            StringBuilder text = new StringBuilder();
            //text.Append("§P");
            //text.Append(MapPosition.X + "," + MapPosition.Y); // this.ToString());
            //text.Append("¤");
            text.Append("§C");
           
            text.Append(id.ToString());
            text.Append("¤");

            if (useUpperCase)
            {
                text.Append(this.ToString().ToUpper(Config.Culture));
            }
            else
            {
                text.Append(this.ToString());
            }


            text.Append("§");

            return text.ToString();
        }

        /*
        public virtual void FlashAsDetected()
        {         
        }
        */

        

        public void FlashAsDetected()
        {
            if (Renderable != null)
            {
                float duration = ResourceType.DetectionFlashDuration ?? GameData.Instance.Constants.FlashDuration;

                Renderable.SetResourceContainerColorFlashing(GameData.Instance.Constants.FlashingColorWhenDetected, duration);

                /*
                Renderable.SetAdditionalTintForDurationOfTime(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, GameData.Instance.Constants.FlashingColorWhenDetected, duration);
                Renderable.SetFlashing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, duration);
                */
            }
        }

        public void FlashWhenClicked()
        {
            if (Renderable != null)
            {
                float duration = GameData.Instance.Constants.FlashDuration;

                Renderable.SetResourceContainerColorFlashing(GameData.Instance.Constants.FlashingColorWhenClicked, duration);

                /*
                Renderable.SetAdditionalTintForDurationOfTime(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, GameData.Instance.Constants.FlashingColorWhenClicked, duration);
                //Renderable.SetOverlayFlashing(duration);
                Renderable.SetFlashing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, duration);*/
            }
        }

        public virtual void Destroy()
        {
            RemoveIDEntry();
            
        }



        #region DetectableID ILookup

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

       /* public static void CreateLookupCollection()
        {
            LookUpIDetectables.Create();
        }*/

        #endregion

        #region ILookup

        private ResourceID id = ResourceID.Invalid;
        static ResourceID IDCounter = ResourceID.First;

        public ResourceID ID
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

        public ResourceID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ResourceID.Max)
            {
                throw new Exception("Astounding, ResourceID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ResourceID SnapshotID(Snapshotter sn, ResourceID id)
        {
            return (ResourceID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != ResourceID.Invalid)
                LookUp<ResourceContainer, ResourceID>.Add(ID, this);
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
            id = ResourceID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<ResourceContainer, ResourceID>.Remove(this);
        }

        void ILookUp<ResourceContainer, ResourceID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ResourceID.First;
        }

        void ILookUp<ResourceContainer, ResourceID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ResourceContainer, ResourceID>.Create();
        }


        #endregion

        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = SnapshotID(sn, id);
            IDCounter = (ResourceID)sn.DoEnum(IDCounter);

            this.detectableID = sn.DoEnum(detectableID);      
            resourceType = sn.DoGameData(resourceType);

            resourceReplenish = (ResourceReplenish)sn.DoISnapshot(resourceReplenish);

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (resourceReplenish != null)
            {
                resourceReplenish.LoadPostProcess(sn);
            }

        }

        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion
       

    }
}

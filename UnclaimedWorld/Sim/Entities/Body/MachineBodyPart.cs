using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body
{
    public class MachineBodyPart: BodyPart, IComposite
    {
        private bool partIsBroken;

        public MachineBodyPart()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public MachineBodyPart(BodyPartType bodyPartType, Body body)
            : base(bodyPartType, body)
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();
            }
        }

        public MachineBodyPart(BodyPart original)
            : base(original)
        {

        }

        public void SetBrokenPart()
        {            
            partIsBroken = true;

        }


        public void SetConditionDirty()
        {
            GetRoot().SetConditionDirty();
        }

        /// <summary>
        /// not used... should we scrap this?
        /// </summary>
        private List<Entity> parts = new List<Entity>();
        public List<Entity> Parts
        {
            get { return parts; }
        }
        List<EntityID> snapshotParts;


        public void SetPart(Entity newPart)
        {
            Body.Parent.SetPart(newPart);
        }

        public void RemovePart(Entity part, bool setPartOfToNull = true)
        {
            Body.Parent.RemovePart(part, setPartOfToNull);
        }


        public IComposite GetRoot()
        {
            return Body.Parent;
            /*
            if (Root != null)
            {
                return Root;
            }
            else
            {
                return ((MachineBodyPart)Parent).GetRoot();
            }*/
        }

       /* public bool PartBroken = false;

        public void SetBrokenPart()
        {
            PartBroken = true;
        }*/


        public override void ChangeOwnershipOnParts(IOwner newOwner)
        {
            foreach (Entity item in Parts)
            {
                Entity.ChangeOwnershipOnParts(item, newOwner);
            }

            base.ChangeOwnershipOnParts(newOwner);

        }

        public void Destroy()
        {
            RemoveIDEntry();
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            id = sn.DoEnum(id);
            partIsBroken = sn.DoBool(partIsBroken);

            if (Parts != null)
            {
                snapshotParts = Parts.Select(p => p.ID).ToList();
            }
            snapshotParts = sn.DoList(snapshotParts);

            sn.Ignore(parts);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (snapshotParts != null)
            {
                parts = snapshotParts.Select(p => Entity.FindByID(p)).ToList();
            }

        }

        #region ILookup

        private CompositeID id = CompositeID.Invalid;

        //=================== ILookup Methods =====================
        public CompositeID ID
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

        public CompositeID GetUniqueID()
        {
            return Composite.GetUniqueID();
        }

        public CompositeID SnapshotID(Snapshotter sn, CompositeID id)
        {
            return (CompositeID)sn.DoEnum(id);
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

            if (ID != CompositeID.Invalid)
            {
                LookUpIComposites.Add(ID, this);    // use the special class!            
            }
        }

        public void RemoveIDEntry()
        {
            LookUpIComposites.Remove(this);  // use the special class!         
        }

        public void SetInvalid()
        {
            id = CompositeID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing since the counter is in a nother class... Sim will call ResetIDCounter.
        {
        }

        void ILookUp<IComposite, CompositeID>.CreateLookupCollection() // interface method - does nothing...
        {

        }

        public static void CreateLookupCollection()
        {
            LookUpIComposites.Create();
        }

        #endregion
    }
}

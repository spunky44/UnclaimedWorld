using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body
{
    /// <summary>
    /// a wrapper for the Body class, to be used as an Entity component
    /// Body can be copied and referenced from MemoryFact, but BodyComponent can't
    /// </summary>
    public class BodyComponent : Component //, IHasBodyParts 
    {
        public Body Body;

        public BodyComponent(Entity parent): base(parent)
        {
            Body = new Body(parent);

            foreach (BodyPartType bodyPartType in parent.EntityType.BodyType.BodyPartTypes)
            {
                AddBodyParts(Body, bodyPartType);
            }
        }

        public BodyComponent()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        private void AddBodyParts(IHasBodyParts addTo, BodyPartType bodyPartType)
        {
            // The original idea for machines was that body parts should be added as it gets constructed...

            BodyPart addedBodyPart;
            if (bodyPartType is BiologicalBodyPartType) // .BiologicalBodyPartTypeComponent != null) 
            {
                //bodyPartType.
                addedBodyPart = new BiologicalBodyPart(bodyPartType, Body);
            }
            else
            {
                addedBodyPart = new MachineBodyPart(bodyPartType, Body);

                /* if (((MachineBodyPartType)bodyPartType).MadeOf != null)
                 {   // creates items out of thin air - what about construction bit by bit?
                     ((MachineBodyPart)addedBodyPart).Parts.Add(new Item(((MachineBodyPartType)bodyPartType).MadeOf));

                 }*/
            }

            if (addTo.BodyParts == null)
            {
                addTo.BodyParts = new List<BodyPart>();
            }

            addTo.BodyParts.Add(addedBodyPart);

            if (bodyPartType.BodyPartTypes != null)
            {
                foreach (BodyPartType childBodyPartType in bodyPartType.BodyPartTypes)
                {
                    AddBodyParts(addedBodyPart, childBodyPartType);
                }
            }
        }


        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Body = (Body)sn.DoISnapshot(this.Body);
           

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


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            Body.LoadPostProcess(sn);
        }

        #endregion

    }
}

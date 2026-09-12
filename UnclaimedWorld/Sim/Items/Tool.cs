using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    public class Tool: Component
    {
        
        private bool? isPrepared;

        /// <summary>
        /// some tools, like the oven or campfire require a preparation step (light fire) before they can be used
        /// </summary>
        public bool? IsPrepared
        {
            get
            {
                return isPrepared;
            }
            set
            {
                if (value != isPrepared)
                {
                   
                    if (value == true)
                    {
                        // notify fuel component...
                        if (Parent.EntityType.ContainerType != null
                            && Parent.EntityType.ContainerType.GetRequiresReplenishType() != null)
                          //  Parent.EntityType.RequiresEnergyType != null)
                        {
                            if (!((IHasReplenishItems)Parent.Contains).ReplenishItems.Start())
                            {
                                return; // could not complete action, no fuel..
                            }
                            /*
                            RequiresEnergy energy;
                            Parent.Find(out energy);
                            if (!energy.Start())
                            {
                                return; // could not complete action, no fuel..
                            }*/
                        }
                    }

                    isPrepared = value;

                    if (Parent.EntityType.ToolType.PrepareProcess != null)
                    {
                        StateModifier modifier = Parent.EntityType.ToolType.PrepareProcessType.PreparedToolModifier ?? StateModifier.PreparedTool;
                        if (isPrepared == true)
                        {
                            Parent.SetSpriteStateFlag(modifier);

                            // set the flag on the container too (generalize this?):                           
                            Entity container;
                            if (Parent.GetContainedBy(out container) && container != null)
                            {
                                container.SetSpriteStateFlag(modifier);
                            }                            
                        }
                        else if (isPrepared == false)
                        {
                            Parent.ClearSpriteStateFlag(modifier);

                            // clear the flag on the container too (generalize this?):                           
                            Entity container;
                            if (Parent.GetContainedBy(out container) && container != null)
                            {
                                container.ClearSpriteStateFlag(modifier);
                            }

                        }
                    }
                }
            }
        }


        public Tool(Entity parent): base(parent)
        {
            
            if (Parent.EntityType.ToolType.PrepareProcess != null)
            {
                // init...   
                isPrepared = false;
            }
            else
            {// if no prepare action is defined, the value should stay null..
                isPrepared = null;
            }

        }

        public Tool()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
      

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.isPrepared = sn.DoBoolNullable(this.isPrepared);

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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Expeditions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances
{
    /// <summary>
    /// delete this..? does not seem entity-component compatible...
    /// </summary>
    public class HumanActivities: ISnapshot
    {
        //public PlayerActivities PlayerActivities;

      //  public decimal TradeCredits;

        /// <summary>
        /// "Research": for realism, unknown critters are chased on foot before the AI will know if this is a succesful tactic
        /// </summary>
        public Dictionary<EntityType, bool> HasChasedHuntedCritter = new Dictionary<EntityType, bool>();



        public void Update(GameTime gameTime)
        {
            

           /* if (PlayerActivities != null)
            {
                PlayerActivities.Update();
            }*/
        }


       

        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            HasChasedHuntedCritter = sn.DoDictionary(HasChasedHuntedCritter);
            //TradeCredits = sn.DoDecimal(TradeCredits);
          
            //sn.Ignore(PlayerActivities); 

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            

        }

        #endregion
    }
}

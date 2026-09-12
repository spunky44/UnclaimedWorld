using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions
{
    /// <summary>
    /// move to EntityGroup? SharedKnowledge...
    /// 
    /// Allegiance has all TradeCredits, but Expeditions can offer different prices... logical?
    /// </summary>
    public class HumanExpeditionActivities : ISnapshot
    {
        #region Othersite fields

        /// <summary>
        /// move to EntityGroup?
        /// </summary>
      //  public TradeManager TradeManager;

        #endregion


        public void Update(GameTime gameTime)
        {
           /* if (TradeManager != null)
            {
                TradeManager.Update(gameTime);
            }*/

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {

          //  this.TradeManager = (TradeManager)sn.DoISnapshot(TradeManager);



            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

         /*   if (TradeManager != null)
                TradeManager.LoadPostProcess(sn);*/

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

        #endregion
    }
}

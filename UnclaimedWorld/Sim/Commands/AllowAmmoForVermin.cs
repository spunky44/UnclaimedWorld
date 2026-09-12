using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.Commands
{
    public class AllowAmmoForVermin : Control.Commands.Command
    {
        public long ExpeditionID;

        public string AmmoType;

        public bool Allow;
      

        public AllowAmmoForVermin()
        {
        }


        public AllowAmmoForVermin(ExpeditionID expeditionID, EntityType ammo, bool value, bool giveClientFeedback) 
        {
            this.ExpeditionID = (long)expeditionID;
            this.AmmoType = ammo.KeyName;
            this.Allow = value;
           
        }


        public override void Execute(bool giveClientFeedback)
        {
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);

            EntityType type;
            GameData.Instance.AllEntityTypes.TryGetValue(AmmoType, out type);

            if (expedition != null && type != null)
            {
                expedition.Policy.SetAllowAmmoForVermin(type, Allow);
           
            }
        }


    }
}

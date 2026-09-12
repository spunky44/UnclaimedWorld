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
    public class AdoptTierPolicy : Control.Commands.Command
    {
        public long ExpeditionID;

        public string Tier;

        public RatingTypes Rating;
      

        public AdoptTierPolicy()
        {
        }

      
        public AdoptTierPolicy(ExpeditionID expeditionID, TierType tier, RatingTypes rating, bool giveClientFeedback) 
        {
            this.ExpeditionID = (long)expeditionID;
            this.Tier = tier.KeyName;
            this.Rating = rating;
           
        }


        public override void Execute(bool giveClientFeedback)
        {
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);

            TierType tierType;
            GameData.Instance.AllTierTypes.TryGetValue(Tier, out tierType);

            if (expedition != null && tierType != null)
            {
                expedition.AdoptTierPolicy(tierType, Rating);
           
            }
        }


    }
}

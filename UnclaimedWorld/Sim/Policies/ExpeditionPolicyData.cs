using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Policies
{
    public class ExpeditionPolicyData
    {
        /// <summary>
        /// will be clamped to min. 50% in game
        /// should not be less than 0.5, in my opinion, to avoid disturbing behaviours
        /// </summary>
        public float? FractionIndependentsAllowedToSleep;

        /// <summary>
        /// will be clamped to min. 50% in game
        /// </summary>
        public int? IndependentsAllowedToSleep = null;       
      
        public SerializableDictionary<RatingTypes, string> CurrentTiers;

        /// <summary>
        /// The default is True when no entry exists.
        /// </summary>
        public SerializableDictionary<string, bool> AllowAmmoUseAgainstVermin;


        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (CurrentTiers != null)
            {
                foreach (var item in CurrentTiers)
                {
                    TierType tier;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.Value, GameData.Instance.AllTierTypes, out tier);
                }

                foreach (var item in Enum.GetValues(typeof(RatingTypes)))
                {
                    string tier;
                    if (!CurrentTiers.TryGetValue((RatingTypes)item, out tier))
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "Missing tier");
                    }
                }

            }

            if (AllowAmmoUseAgainstVermin != null)
            {
                foreach (var item in AllowAmmoUseAgainstVermin)
                {
                    EntityType tier;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.Key, GameData.Instance.AllEntityTypes, out tier);
                }

            }

        } 

    }
}

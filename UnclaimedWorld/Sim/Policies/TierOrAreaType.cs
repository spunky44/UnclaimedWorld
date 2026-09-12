using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.Policies
{
    public class TierOrAreaType
    {
        /// <summary>
        /// only one will be filled
        /// </summary>
        public TierArea TierArea;

        public TierType TierType;


        public string Icon
        {
            get
            {
                if (TierType != null)
                {
                    return TierType.Icon;
                }
                else
                {
                    return TierArea.Icon;
                }
            }
        }

        public string Description
        {
            get
            {
                if (TierType != null)
                {
                    return TierType.Description;
                }
                else
                {
                    return TierArea.Description;
                }
            }
        }

        public TierOrAreaType(TierOrArea tierArea)
        {
            if (tierArea.Area != null)
            {
                TierArea = GameData.Instance.AllTierAreas.Values.FirstOrDefault(t => t.Tier == tierArea.Tier && t.Area == tierArea.Area); //GameData.Instance.AllTierAreas[tierArea.Area];
            }
            else if (tierArea.Tier != null)
            {
                TierType = GameData.Instance.AllTierTypes[tierArea.Tier];
            }
        }

        public TierType GetTier()
        {
            if (TierType != null)
            {
                return TierType;
            }
            else
            {
                return TierArea.TierType;
            }
        }

        public string GetNotAvailableTooltip()
        {
            return "The following policy needs to be enacted first: " + ToString();
        }

        public override string ToString()
        {
            if (TierArea != null)
            {
                return TierArea.ToString();
            }
            else
            {
                return TierType.Name + " tier (any area)";
            }
        }
    }
}

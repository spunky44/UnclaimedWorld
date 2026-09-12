using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Tiers
{
    public class TierType: IGameData, IEdge
    {
        public string KeyName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }

        public string Description;

        public string Icon;

        public float UpperEdge;

        public float Edge
        {
            get
            {
                return UpperEdge;
            }
            set
            {

            }
        }
       // public string PolicyButton;

        
        [XmlIgnore]
        public int Index;

        public bool DeleteRecord
        {
            get;
            set;
        }

        public static void GetTierBelow(int tierIndex, out TierType previousTier, out float lowerTierEdge)
        {
            previousTier = null;

            if (tierIndex > 0)
            {
                previousTier = GameData.Instance.Tiers[tierIndex - 1];
                lowerTierEdge = previousTier.UpperEdge;
            }
            else
            {
                lowerTierEdge = 0f;
            }
        }

        public float GetTierEdgeBelow()
        {
            TierType previousTier;
            float lowerEdge;
            GetTierBelow(Index, out previousTier, out lowerEdge);

            return lowerEdge;
        }

        public void PreInitValidate(ref List<string> listOfErrors)
        {
           /* EntityType.ValidateRequiredValue(ref listOfErrors, "Icons", Icons != null);

            if (Icons != null)
            {
                foreach (var item in Enum.GetValues(typeof(RatingTypes)))
                {
                    if (!Icons.ContainsKey((RatingTypes)item))
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "Missing icon for: " + ((RatingTypes)item).ToString());
                    }
                }
            }*/
        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {          
        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {

           
        }


    }
}

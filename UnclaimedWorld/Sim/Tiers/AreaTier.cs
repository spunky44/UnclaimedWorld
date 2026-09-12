using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Tiers
{
    public class TierArea : IGameData
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


        public string Tier;
        public RatingTypes Area;


        public string Icon;

        public string Description;


        [XmlIgnore]
        public TierType TierType;

        public bool DeleteRecord
        {
            get;
            set;
        }

        public void PreInitValidate(ref List<string> listOfErrors)
        {
            EntityType.ValidateRequiredValue(ref listOfErrors, "Icon", Icon != null);
        }


        public void PostDataCompleteInitialize()
        {
            TierType = GameData.Instance.AllTierTypes[Tier];
        }

        public override string ToString()
        {
            return Name ?? TierType.Name + " " + Statistic.RatingsTypeToString(Area).ToLower(Config.Culture);
        }


        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {
        }

     
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {


        }
    }
}

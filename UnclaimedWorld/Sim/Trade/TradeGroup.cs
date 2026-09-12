using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// a group of items that can be referred to by expeditions, together describing a trade profile/economy
    ///   
    /// could also be used to mark economic events
    /// 
    /// perhaps later, there will be substitution effects...? 
    /// </summary>
    public class TradeGroup: IGameData
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

        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Comments
        {
            get;
            set;
        }

       

        public string OfferDemandProfile;

       // public SerializableDictionary<string, TradeAmountType> AvailableForTrade;
        public TradeAmountType[] AvailableForTrade;

        /*
        /// <summary>
        /// high priority will override items in lower priority trade groups.
        /// </summary>
        public int Priority;
        */

        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {

        }

        public void PostDataCompleteInitialize()
        {

        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            foreach (var item in AvailableForTrade)
            {
                EntityType p;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.GetEntityTypeKey(), GameData.Instance.AllEntityTypes, out p);                
            }
           
            if (OfferDemandProfile != null)
            {
                OfferDemandProfile p;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, OfferDemandProfile, GameData.Instance.AllOfferDemandProfiles, out p);
            }
        }
    }
    /*
    public struct TradeEntityType
    {
        public string EntityType;
        public string EntityDataKey;

        public TradeAmountType TradeAmountType;

    }*/
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// this class makes it easier to set up randomized NPC expeditions. It refers to trade groups in a similar way as TraitTemplate refers to sets of skills
    /// </summary>
    public class TradeProfile: IGameData
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
        
        public string Comments;
        

        public string[] HighTrade;
        public string[] MediumTrade;
        public string[] LowTrade;

        public RandomTrade RandomHighTrade;
        public RandomTrade RandomMediumTrade;
        public RandomTrade RandomLowTrade;


        
        /// <summary>
        /// all of the trade group items here will be produced in a surplus for export
        /// </summary>
        public string[] HighExport;


       
        public RandomTrade RandomHighExport;

        public string[] MediumExport;

       
        public RandomTrade RandomMediumExport;

        /// <summary>
        /// these will be produced in lesser numbers
        /// </summary>
        public string[] LowExport;

        public RandomTrade RandomLowExport;

       
        public string[] HighImport;

        public RandomTrade RandomHighImport;

        public string[] MediumImport;

        public RandomTrade RandomMediumImport;
        

        /// <summary>
        /// these will be imported in lesser numbers
        /// </summary>
        public string[] LowImport;

        public RandomTrade RandomLowImport;

        /// <summary>
        /// optional - can set some trade groups as higher priority so their items will override lower priority ones.
        /// Default priority is 0
        /// </summary>
        public SerializableDictionary<string, int> TradeGroupPriority;


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

        private void ValidateRandomTradeGroups(ref List<string> listOfErrors, RandomTrade randomTrade)
        {
            if (randomTrade != null)
            {
                foreach (var item in randomTrade.Options)
                {
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item, GameData.Instance.AllTradeGroups);
                }
            }
        }

        private void ValidateTradeGroups(ref List<string> listOfErrors, string[] groups)
        {
            if (groups != null)
            {
                foreach (var item in groups)
                {
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item, GameData.Instance.AllTradeGroups);
                }
            }

        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            ValidateTradeGroups(ref listOfErrors, LowTrade);
            ValidateTradeGroups(ref listOfErrors, MediumTrade);
            ValidateTradeGroups(ref listOfErrors, HighTrade);
            ValidateTradeGroups(ref listOfErrors, LowImport);
            ValidateTradeGroups(ref listOfErrors, MediumImport);
            ValidateTradeGroups(ref listOfErrors, HighImport);
            ValidateTradeGroups(ref listOfErrors, LowExport);
            ValidateTradeGroups(ref listOfErrors, MediumExport);
            ValidateTradeGroups(ref listOfErrors, HighExport);

            ValidateRandomTradeGroups(ref listOfErrors, RandomHighTrade);
            ValidateRandomTradeGroups(ref listOfErrors, RandomMediumTrade);
            ValidateRandomTradeGroups(ref listOfErrors, RandomLowTrade);

            ValidateRandomTradeGroups(ref listOfErrors, RandomHighExport);
            ValidateRandomTradeGroups(ref listOfErrors, RandomMediumExport);
            ValidateRandomTradeGroups(ref listOfErrors, RandomLowExport);

            ValidateRandomTradeGroups(ref listOfErrors, RandomHighImport);
            ValidateRandomTradeGroups(ref listOfErrors, RandomMediumImport);
            ValidateRandomTradeGroups(ref listOfErrors, RandomLowImport);

        }



    }

    public class RandomTrade
    {
        public int NoOfGroups;
        public string[] Options;
    }


}

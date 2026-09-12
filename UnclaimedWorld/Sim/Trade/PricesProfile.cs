using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// can be used for a global price list to avoid having to specify them in more than one place. 
    /// Can also be local.
    /// The prices can have site specific modifers too...
    /// 
    /// The buy/sell prices can be derived from this one price I think. Bigger traders should have narrower price gap perhaps.
    /// </summary>
    public class PricesProfile: IGameData
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


        public SerializableDictionary<string, float> Prices;



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

        }
    }

}

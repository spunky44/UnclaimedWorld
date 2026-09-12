using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade
{
    public class VehiclesProfile: IGameData
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

        public SerializableDictionary<string, VehiclesForHireType> VehiclesForHire;

      //  public SerializableDictionary<string, TradeAmountType> VehiclesForHire;
        

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

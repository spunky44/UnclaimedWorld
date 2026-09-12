using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// defines non-linear changes in the offer and demand of a good
    /// </summary>
    public class OfferDemandProfile: IGameData
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


        /// <summary>
        /// to make this a markov chain, we need keys for each state, and a transition matrix. Not really needed now.
        /// </summary>
        public OfferDemandState[] States;


        /// <summary>
        /// for non-linear production or consumption (Markov chain)
        /// </summary>
      //  public StringChance[] TransitionsPerDay;

        /// <summary>
        /// simulates a fluctuating amount for sale
        /// </summary>
        public NoiseParams NonlinearAmountForSale;


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

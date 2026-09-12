using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Trade;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Expeditions
{
    /// <summary>
    /// defines the state of a non-player allegiance on startup - after that, the class is not used anymore.
    /// 
    /// </summary>
    public class ExpeditionData: IGameData
    {
        public string KeyName { get; set; }
        public string Name { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }

        /// <summary>
        /// This variable only needs to be set for the CreateExpeditionAction
        /// 
        /// If ExpeditionData is used within the action that creates allegiances that allegiance will be used.
        /// </summary>
        public string AllegianceKey;
      
        /// <summary>
        /// only required for playsite expeditions
        /// </summary>
        public EvalNode Location;

        /// <summary>
        /// this can override the size factor set at the Site level
        /// </summary>
        public float? SizeFactor;

        /// <summary>
        /// profile instead of inline TradeAmounts
        /// </summary>
        public string TradeProfile;

        /// <summary>
        /// the default prices to use
        /// </summary>
        public string PricesProfile;


        /// <summary>
        /// profile defines terminals and communication
        /// </summary>
        public string StructuresProfile;

        /// <summary>
        /// profile defines vehicles for hire and their price
        /// </summary>
        public string VehiclesProfile;


        //public TradeAmountTypes TradeAmounts;

        /// <summary>
        /// for inline definition of trade, overrides any trade profile that has been set, on an entity type basis.
        /// </summary>
        public SerializableDictionary<string, TradeAmountType> AvailableForTrade;

        public SerializableDictionary<string, VehiclesForHireType> VehiclesForHire;


        public ExpeditionPolicyData PolicyData;


        public PopulationData PopulationData;

        /*
        /// <summary>
        /// ???
        /// </summary>
        public StringChance[] ExpeditionTemplates;
        */

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (AvailableForTrade != null)
            {
                foreach (var item in AvailableForTrade)
                {
                    EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item.Key);
                }
            }

             if (VehiclesForHire != null)
             {
                 foreach (var item in VehiclesForHire)
                 {
                     EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item.Key);
                 }
             }

            if (PricesProfile != null)
            {
                PricesProfile profile;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, PricesProfile, GameData.Instance.AllPricesProfiles, out profile);
            }

            if (StructuresProfile != null)
            {
                StructuresProfile profile;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, StructuresProfile, GameData.Instance.AllStructuresProfiles, out profile);
            }

            if (VehiclesProfile != null)
            {
                VehiclesProfile profile;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, VehiclesProfile, GameData.Instance.AllVehiclesProfiles, out profile);

                if (VehiclesForHire != null)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "VehiclesForHire and VehiclesProfile cannot both be specified");
                }
            }

            /*
            if (TradeAmounts != null)
            {
                TradeAmounts.PostDataCompleteValidate(ref listOfErrors);
            }*/

            if (PopulationData != null)
            {
                PopulationData.PostDataCompleteValidate(ref listOfErrors);
            }

            if (PolicyData != null)
            {
                PolicyData.PostDataCompleteValidate(ref listOfErrors);
            }
        }



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

    }
}

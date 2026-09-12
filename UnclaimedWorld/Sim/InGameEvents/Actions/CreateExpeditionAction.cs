using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Trade;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Policies;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class CreateExpeditionAction : EventActionType
    {
        public ExpeditionData ExpeditionData;

        public string ExpeditionDataKey;

        /// <summary>
        /// needs to be filled if ExpeditionData does not have the key
        /// </summary>
        public string AllegianceKey;
      

        public StatsData StatsData;

         public CreateExpeditionAction(string keyName): base(keyName)
        {

        }

         public CreateExpeditionAction()           
        {

        }


        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            ExpeditionData expeditionData;
            if (ExpeditionDataKey != null)
            {
                expeditionData = GameData.Instance.AllExpeditionData[ExpeditionDataKey];
            }
            else
            {
                expeditionData = ExpeditionData;
            }


            Allegiances.Allegiance allegiance;

            string allegianceKeyToUse = AllegianceKey ?? expeditionData.AllegianceKey;

            allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKeyToUse);

            if (allegiance == null)
            {
                return false;
            }

            return Expedition.CreateFromExpeditionData(expeditionData, allegiance, action, null, null, out failReason);
         
        }


        public override void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (ExpeditionData != null)
            {
                ExpeditionData.PostDataCompleteValidate(ref listOfErrors);
            }
        }


        public override void PreInitValidate(ref List<string> listOfErrors)
        {
            base.PreInitValidate(ref listOfErrors);

            if (ExpeditionData != null)
            {
                EntityType.ValidateRequiredValue(ref listOfErrors, "Allegiance key", !string.IsNullOrEmpty(ExpeditionData.AllegianceKey));
            }
            else
            {
                ExpeditionData data;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, ExpeditionDataKey, GameData.Instance.AllExpeditionData, out data);
            }
        }


        public override string ToString()
        {
            if (ExpeditionData != null)
            {
                return "Spawn expedition " + ExpeditionData.KeyName;
            }
            else return "Spawn expedition " + ExpeditionDataKey;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Allegiances;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.ClientSide.Log;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents
{
   
    public class LogAction : EventActionType, IGameData
    {
       
        public Priority Priority = Priority.Normal;

        //public EventType EventType;

        public string Text;

        public bool ReferToTriggeringEntity = true;

          public LogAction(string keyName): base(keyName)
        {

        }

          public LogAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute(EntityID? triggeringEntity)
        {
            Entity concernedEntity = null;

            if (ReferToTriggeringEntity && action.TriggeringEntity != null)
            {                
                concernedEntity = Entity.FindByID(action.TriggeringEntity.Value);
            }

            The.Client.Log.AddLogEvent(The.Client.Log.GeneralEvent, concernedEntity, Text, Priority);
           
            return true;
        }


        public override void PostInitValidate(ref List<string> listOfErrors)
        {
            base.PostInitValidate(ref listOfErrors);

            if (Text == null)
            {
                EntityType.CreateValidationError(ref listOfErrors,
                                string.Format("Text was not filled out!", KeyName));
            }
           
        }
       
    }
}

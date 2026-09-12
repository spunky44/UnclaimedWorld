using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions
{

    public class DestroyEntityAction : EventActionType
    {
        
        public TargetObject TargetObject;
                        
        public string EntityName;

        public DestroyEntityAction(string keyName): base(keyName)
        {

        }

        public DestroyEntityAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            Entity entity = null;
            if (!EventActionType.GetEntity(EntityName, TargetObject, action, out entity, ref failReason))
            {
                return false;
            }

         
            if (entity != null)
            {
                entity.Destroy();
                return true;
            }

            return false;
        }

       

     /*   public static Entity GetEntity(TargetEntityOfAction targetEntityOfAction, string entityName, EntityID? triggeringEntity, EntityID? targetEntity)
        {
            Entity entity = null;

            switch (targetEntityOfAction)
            {
                case TargetEntityOfAction.SpecifiedEntity:
                    if (!string.IsNullOrEmpty(entityName))
                    {
                        entity = TalkAction.GetEntityByName(entityName);
                    }
                    break;
                case TargetEntityOfAction.TriggeringEntity:
                    if (triggeringEntity.HasValue)
                    {
                        entity = Entity.FindByID(triggeringEntity.Value);
                    }
                    break;
                case TargetEntityOfAction.TargetEntity:
                    if (targetEntity.HasValue)
                    {
                        entity = Entity.FindByID(targetEntity.Value);
                    }
                    break;
            }
            return entity;
        }*/

       

        public override string ToString()
        {
            return "Destroy entity " + EntityName ?? "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    ///  for non-persons, OwnerOfTarget will resolve to the owning (expedition). 
    ///  For persons, the other values will return that particular ownership
    /// </summary>
    public enum Ownership { Expedition, Private, Household, OwnerOfTarget }
    public enum ActionType { Claim, Discard }
    public class ClaimEntityAction : EventActionType
    {
        /// <summary>
        /// Discard is not implemented...
        /// </summary>
        public ActionType Action;
    
        /// <summary>
        /// Claim target.
        /// This action can also handle lists of items
        /// </summary>
        public TargetObject TargetObject;

                
        /// <summary>
        /// shortcut for claim target
        /// </summary>
        public string EntityName;




        public Ownership OwnershipType;
        public TargetObject NewOwner;

         public ClaimEntityAction(string keyName): base(keyName)
        {

        }

         public ClaimEntityAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            //Entity entity = null;

            List<IHasExposedProperties> entities = null;

            if (EntityName != null)
            {
                Entity entity = TalkAction.GetEntityByName(EntityName);

                if (entity == null)
                {
                    failReason = "No entity with name '" + EntityName + "' exists.";
                    return false;
                }
                else
                {
                    Common.AddToList(ref entities, entity);
                }
            }
            else
            {

                entities = TargetObject.GetResult(action);

                if (entities.Count == 0)
                {
                    failReason = "Lookup did not give any results.";
                    return false;
                }
               
            }

            if (entities != null)
            {
                IOwner newOwner = null;

                if (this.Action == ActionType.Claim)
                {
                    newOwner = ResolveNewOwner(OwnershipType, NewOwner, action);
                    if (newOwner == null)
                    {
                        failReason = "Owner lookup did not give any results.";
                        return false;
                    }
                }

                // we can claim a list of items - such as all contained items
                foreach (var item in entities)
                {
                    Entity entity = item as Entity;
                    if (entity != null)
                    {
                        if (this.Action == ActionType.Claim)
                        {
                            DoClaim(entity, newOwner);
                        }
                        else
                        {
                            //DoDiscard(entity);
                            Discard.DoDiscard(entity); // TODO: there may be a problem with discarding the parts/parent item.

                        }
                    }
                }

                return true;
            }

            return false;
        }


        public static IOwner ResolveNewOwner(Ownership OwnershipType, TargetObject NewOwner, EventAction action)
        {
            if (NewOwner == null)
                return null;

            var result = NewOwner.GetResult(action);

            if (result.Count > 0)
            {
                // can only handle Entity results now. IHasExposed could later be implemented by Expedition and Household too
                IHasExposedProperties resultObject = result[0];

              
                Entity entity = resultObject as Entity;
                if (entity != null /*
                    && entity.EntityType.Person != null*/) 
                {                   

                    switch (OwnershipType)
                    {
                        case Ownership.Expedition:
                            if (entity.EntityType.IntelligenceType != null)
                            {
                                return entity.Intelligence.CurrentExpedition;
                            }
                            /*else if (entity.OwnedBy != null)
                            {
                                IOwner owningExpedition = LookUp<IOwner, OwnerID>.FindByID(entity.OwnedBy.Value);
                                return owningExpedition as Expedition;
                            }*/

                            return null;

                        case Ownership.Household: // only people can own things
                            if (entity.EntityType.Person != null)
                            {
                                return entity.PersonEntity.Household;
                            }
                            /*else if (entity.OwnedBy != null)
                            {
                                IOwner owningHousehold = LookUp<IOwner, OwnerID>.FindByID(entity.OwnedBy.Value);
                                return owningHousehold as Household;
                            }*/

                            return null;

                        case Ownership.Private:
                            if (entity.EntityType.Person != null)
                            {
                                return entity.PersonEntity;
                            }
                           /* else if (entity.OwnedBy != null)
                            {
                                IOwner owningPerson = LookUp<IOwner, OwnerID>.FindByID(entity.OwnedBy.Value);
                                return owningPerson as PersonEntity;
                            }*/

                            return null;

                        case Ownership.OwnerOfTarget:
                            // for non-agents, OwnerOfTarget will resolve to the owning (expedition)
                            if (entity.EntityType.Person == null && entity.OwnedBy != null)
                            {
                                IOwner owningHousehold = LookUpOwners.FindByID(entity.OwnedBy.Value); //LookUp<IOwner, OwnerID>.FindByID(entity.OwnedBy.Value);
                                return owningHousehold;
                            }
                            return null;

                    }
                }

            }

            return null;
        }

        private void DoClaim(Entity entity, IOwner newOwner)
        {
          /*  Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);

            IKnownEntityData entityData;
            allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out entityData);
            Entity itemEntity = entityData as Entity;

            IOwner newOwner = LookUpOwners.FindByID((OwnerID)NewOwner);
            */

            // can only claim seen items!
          /*  if (itemEntity != null)
            {*/

            entity.ChangeOwnership(newOwner);  
             /*   return true;
            }*/

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

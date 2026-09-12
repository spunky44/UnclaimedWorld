using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// a valid owner of credits can be: Allegiance, Household, Person
    /// </summary>
    public class ChangeCreditsAction : EventActionType
    {       
       
        public EvalNode Amount;

        /// <summary>
        /// the target that should have the change applied to its credits.
        /// </summary>
        public TargetObject TargetObject;

        /// <summary>
        /// shortcut for selecting a named allegiance
        /// </summary>
        public string AllegianceKey; 
       


        public enum Operation { Set, Add }

        /// <summary>
        /// specifies how the value should be applied to the current credits
        /// /// </summary>
        public Operation OperationToUse;

        public ChangeCreditsAction(string keyName): base(keyName)
        {

        }

        public ChangeCreditsAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            // find the owner:
            IHasExposedProperties ownerOfCredits;
            if (AllegianceKey != null)
            {
                ownerOfCredits = The.Sim.PlaySite.Allegiances.First(a => a.KeyName == AllegianceKey);
            }
            else
            {
                var result = TargetObject.GetResult(action);

                if (result.Count == 0)
                {
                    ownerOfCredits = null;
                }
                else
                {
                    ownerOfCredits = result[0];
                }

            }

            EntityGroup entityGroupOwner = null;
            if (ownerOfCredits != null)
            {
                Allegiance ownerAsAllegiance = ownerOfCredits as Allegiance;
                if (ownerAsAllegiance != null)
                {
                    entityGroupOwner = ownerAsAllegiance.SharedKnowledge.AllKnownEntities;
                }
                else 
                {
                    Entity ownerAsEntity = ownerOfCredits as Entity;
                    if (ownerAsEntity != null && ownerAsEntity.PersonEntity != null)
                    {
                        entityGroupOwner = ownerAsEntity.PersonEntity.OwnedEntities;
                    }
                    else
                    {
                        // TODO: Household

                    }
                }
            }

            if (entityGroupOwner == null)
            {
                failReason = "Lookup of target owner did not give any valid results.";
                return false;
            }

            // find the value to set:
            float numberResult;

            if (Amount != null)
            {
                PropertyResult? value = Amount.Evaluate(action);

                numberResult = value.Value.NumberResult.Value;
            }
            else
            {
                failReason = "Amount has not been set.";
                return false;
            }

            

            decimal currentAmount = entityGroupOwner.Parent.TradeCredits ?? 0;

            /*if (!currentAmount.HasValue)
            {
                failReason = "The targeted owner did not support trade credits.";
                return false;
            }*/

            decimal newAmount;

            switch (OperationToUse)
            {               
                case Operation.Add:

                    newAmount = (decimal)numberResult + currentAmount;
                    break;

                case Operation.Set:
                default:

                    newAmount = (decimal)numberResult;
                    break;
            }

            entityGroupOwner.Parent.TradeCredits = newAmount;

            return true;               
            
        }

      

        public override string ToString()
        {
            return "Change trade credits for: " + (AllegianceKey ?? "");
        }
    }
}

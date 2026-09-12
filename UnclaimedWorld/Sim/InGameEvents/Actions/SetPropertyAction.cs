using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Actions
{

    public class SetPropertyAction : EventActionType
    {
       
        /// <summary>
        /// this can be left out - in which case the Site will be the target object
        /// </summary>
        public TargetObject TargetObject;
        
      
        /// <summary>
        /// the name of the property to set
        /// </summary>
        public string PropertyKey;


        public EvalNode Value;
        private string logMessage;

        public bool SetValueToNull = false;

        public SetPropertyAction()
        {

        }

        public SetPropertyAction(string keyName): base(keyName)
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<IHasExposedProperties> resultList = null;

            if (this.PropertyKey == "disableSpecialAction")
            {

            }

            if (this.PropertyKey == "addTrigger")
            {

            }

            if (TargetObject != null)
            {
                resultList = TargetObject.GetResult(action);
            }
            else
            {
                resultList = TargetObject.GetRootElementAsList();
            }

            // we will only set values on single entities, not on lists - for now.
            IHasExposedProperties targetObject = null;

            if (resultList != null && resultList.Count > 0)
            {
                targetObject = resultList[0];
                // set the value:
                SetValue(targetObject, action);
            }

            return true;
        }



        private void SetValue(IHasExposedProperties firstResult, EventAction action) // EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (firstResult != null)
            {
                // compute and set the value:
                if (Value != null)
                {
                    PropertyResult? valueToSet = Value.Evaluate(action); // triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    if (valueToSet.HasValue)
                    {
                        logMessage = "set property: " + PropertyKey + " to " + valueToSet.Value.ToString() + " at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
                        firstResult.SetPropertyValue(PropertyKey, valueToSet.Value);
                    }
                    else
                    {
                        logMessage = "Failed to set property " + PropertyKey + " at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
                    }
                }
                else if (SetValueToNull)
                {
                    logMessage = "set property: " + PropertyKey + " to null" + " at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
                     
                    firstResult.SetPropertyValue(PropertyKey, null);
                }
            }
            
        }

       

        public void PreInitValidate(List<string> errors)
        {
            /* we now allow these properties to not be filled, it is interpreted as 'Root'
            if (this.TargetEntity == TargetEntityOfAction.SpecifiedEntity && 
                (GetList == null && GetObject == null)) 
            {
              
                EntityType.CreateValidationError(ref errors,
                                string.Format("Target expression filter was not filled out!", KeyName));
            }*/


        }


        public override string ToString()
        {
            return logMessage; // +" = " + (NumberValue ?? BoolValue ?? StringValue ?? "").ToString();
        }
    }
}

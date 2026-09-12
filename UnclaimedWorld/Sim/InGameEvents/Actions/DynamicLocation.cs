using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.InGameEvents.Actions
{
   
    /// <summary>
    /// we can deprecate this - i have made a general class instead (ValueNode)
    /// </summary>
    public class DynamicLocation
    {
        public TargetObject TargetObject;

        // gets a location or a container by invoking this property on the target object:
        public string PropertyKey;



        public Vector2? GetLocation(EventAction action) // EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<IHasExposedProperties> resultList = null;
            
            if (TargetObject != null)
            {
                resultList = TargetObject.GetResult(action); // triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }
            else
            {
                resultList = TargetObject.GetRootElementAsList();
            }

            if (resultList != null && resultList.Count > 0)
            {
                IHasExposedProperties targetObject = resultList[0];

                PropertyResult? result = null;
                if (targetObject != null)
                {
                    if (PropertyKey != null)
                    {
                        result = targetObject.GetPropertyValue(PropertyKey);

                        if (result != null)
                        {
                            return result.Value.LocationResult;
                        }
                    }
                }
            }

            return null;
        }
    }

    public class ContainerLocation
    {
        public TargetObject TargetObject;

        public string StorageCondition;
       
        public bool OfferForSale = false;
        public bool IsProductionOutput = false;

        public string UpgradeCategory;

        public Entity GetContainer(EventAction action) // EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<IHasExposedProperties> resultList = null;

            if (TargetObject != null)
            {
                resultList = TargetObject.GetResult(action); // triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }
          /*  else if (NameOfContainer != null)
            {
                
            }*/
            else
            {
                resultList = TargetObject.GetRootElementAsList();
            }

            if (resultList != null && resultList.Count > 0)
            {
                IHasExposedProperties targetObject = resultList[0];
                              
                if (targetObject != null)
                {
                    Entity container = targetObject as Entity;
                    if (container != null && container.Contains != null)
                    {
                        return container;
                    }
                }
            }

            return null;
        }
    }
}

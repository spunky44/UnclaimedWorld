using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.Entities
{
    public delegate PropertyResult? GetPropertyValue(IHasExposedProperties presentedObject, SharedKnowledge getterKnowledge, IHasExposedProperties parent);
  

    public delegate string GetCaption(IHasExposedProperties presentedObject);

    public interface IHasExposedProperties
    {
        string KeyName { get; }

        /// <summary>
        /// is used for returning any IHasExposedProperties objects, single or multiple
        /// </summary>
        /// <param name="keyToList"></param>
        /// <param name="listToFillWithProperties"></param>
        /// <param name="filter"></param>
        /// <param name="getterKnowledge"></param>
        void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition /* PropertyCondition*/ filter, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            SharedKnowledge getterKnowledge = null); 

        /// <summary>
        /// it would be nice to get a reference to the parent when invoking this on children...
        /// </summary>
        /// <param name="propertyKey"></param>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge = null, IHasExposedProperties parent = null);
        void SetPropertyValue(string propertyKey, PropertyResult? value);
        
        string GetCaption(string propertyKey);
        string GetDefaultCaption(string propertyKey);
        void GetDefaultKey(out string PropertyKey);

        EntityID? GetEntityID();
        bool GetIsSeenDirectly(); //SharedKnowledge getterKnowledge);
    }
}

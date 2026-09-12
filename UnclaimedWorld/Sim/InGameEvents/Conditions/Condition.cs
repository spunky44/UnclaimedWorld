using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    [XmlInclude(typeof(ConditionFunction))]
    [XmlInclude(typeof(ConditionValue))]
    public abstract class Condition
    {
        public abstract void Initialize();

        public abstract bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);

        public abstract void PreInitValidate(ref List<string> errors);
    }
}

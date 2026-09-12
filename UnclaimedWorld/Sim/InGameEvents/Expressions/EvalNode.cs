using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.Expressions
{
    /// <summary>
    /// meant for returning property values (variables) and constants - not complex objects.
    /// Use TargetObject for those.
    /// </summary>
    [XmlInclude(typeof(ValueNode))]
    [XmlInclude(typeof(FunctionNode))]
    [XmlInclude(typeof(UnaryFunctionNode))]    
    public abstract class EvalNode
    {

        public PropertyResult? Evaluate(EventAction eventAction)
        {
            return Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
        }


        public abstract PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);

        /// <summary>
        /// for validation only...
        /// </summary>
        /// <returns></returns>
        public virtual string EvaluateConstant()
        {
            return null;
        }

    }
}

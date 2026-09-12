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
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnTriggerAction : EventActionType 
    {
        public string TriggerType;

        /// <summary>
        /// either a location for the trigger
        /// </summary>
        public EvalNode Location;

        /// <summary>
        /// or an entity to attach to
        /// </summary>
        public TargetObject TargetObject;

        // override TriggerType defaults:

        /// <summary>
        /// either circular area
        /// </summary>
        public EvalNode Range;
 
        /// <summary>
        /// or rectangle area
        /// </summary>
        public Vector2? AreaDimensions;

        public SpawnTriggerAction(string keyName): base(keyName)
        {

        }

        public SpawnTriggerAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            
            float? range = null;
            if (Range != null)
            {
                PropertyResult? result = Range.Evaluate(action);

                if (result != null && result.Value.NumberResult.HasValue)
                {
                    range = result.Value.NumberResult.Value;
                }
            }

            Vector3? location = null;
            Entity entity = null;
            if (Location != null)
            {
                PropertyResult? result = Location.Evaluate(action);

                if (result != null && result.Value.LocationResult.HasValue)
                {
                    location = result.Value.LocationResult.Value.ToVector3();
                    
                }
                else
                {
                    failReason = "Location did not give a result.";
                    return false;

                }
            }
            else
            {
                var result = TargetObject.GetResult(action);
                if (result.Count == 0)
                {
                    failReason = "Lookup did not give any results.";
                    return false;
                }
                entity = (Entity)result[0];
            }          

            if (entity != null)
            {
                entity.AttachTrigger(new Trigger(entity,
                        null,
                        GameData.Instance.AllTriggerTypes[TriggerType], null, range, AreaDimensions));
            }
            else
            {

                The.Sim.TriggerSystem.RegisterTrigger(
                    new Trigger(null, 
                    location,
                    GameData.Instance.AllTriggerTypes[TriggerType], null, range, AreaDimensions));
            }
            
            
            return true; 
        }

      

        public override string ToString()
        {
            return "Spawn trigger " + TriggerType;
        }
    }
}

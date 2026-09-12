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
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.InGameEvents.Actions
{

    public enum DetectMode { DetectAlwaysSeenEntities, RollToDetectHiddenEntities, NoEntityDetection }

    public class ExploreAction : EventActionType
    {
        public bool ExploreWholeMap;

        //public bool PerformDetection;
        public DetectMode DetectMode = DetectMode.DetectAlwaysSeenEntities;

        public float RadiusStart;
        public float? RadiusEnd;

        public Vector2 OffsetLocationStart;
        public Vector2? OffsetLocationEnd;

        public EvalNode DynamicLocationStart;
        public EvalNode DynamicLocationEnd;

        public TargetObject EntityToExploreWith;

      //  public TargetEntityOfAction EntityToExploreWith;
      //  public string EntityName;

         public ExploreAction(string keyName): base(keyName)
        {

        }

         public ExploreAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {

            Entity entity = null;
            if (EntityToExploreWith != null)
            {
                var result = EntityToExploreWith.GetResult(action); // DestroyEntityAction.GetEntity(EntityToExploreWith, EntityName, triggeringEntity, targetEntity);

                if (result.Count == 0)
                    return false;

                entity = (Entity)result[0];
            }

            if (ExploreWholeMap)
            {
                The.Sim.ExploreShroud(entity, this.DetectMode);
            }
            else
            {

                Vector2 exploreLocationStart = OffsetLocationStart;

                if (AddDynamicOffset(action, DynamicLocationStart, ref failReason, ref exploreLocationStart) == false)
                    return false;


                if (DynamicLocationEnd != null || OffsetLocationEnd.HasValue)
                {
                    Vector2 exploreLocationEnd = OffsetLocationEnd ?? Vector2.Zero;

                    if (AddDynamicOffset(action, DynamicLocationEnd, ref failReason, ref exploreLocationEnd) == false)
                        return false;

                    The.Sim.ExploreShroud(new WorldLocation(exploreLocationStart.ToVector3()), new WorldLocation(exploreLocationEnd.ToVector3()),
                        RadiusStart, RadiusEnd ?? RadiusStart,
                        entity, DetectMode);
                }
                else
                {

                    The.Sim.ExploreCircularSpot(entity, new WorldLocation(exploreLocationStart.ToVector3()), DetectMode, RadiusStart);
                }
            }

            return true;
        }

        public override void PreInitValidate(ref List<string> errors)
        {
            base.PreInitValidate(ref errors);

            if (DetectMode != Actions.DetectMode.NoEntityDetection)
            {
                EntityType.ValidateRequiredValue(ref errors, "EntityToExploreWith when DetectMode is not None", EntityToExploreWith != null);
            }
        }

        public override string ToString()
        {
            return "ExploreAction " + OffsetLocationStart.ToString() + ", whole map: " + ExploreWholeMap.ToString();
     
        }

        private static bool AddDynamicOffset(EventAction action, //EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            /*DynamicLocation*/ EvalNode dynamicLocation, ref string failReason, ref Vector2 exploreLocationStart)
        {
            if (dynamicLocation != null)
            {
                Vector2? offsetLocation;

                // add a dynamic offset, such as a starting location, the location of a triggering entity etc.
              //  Vector2? offsetLocation = dynamicLocation.GetLocation(triggeringEntity, targetEntity);

                // get the location:
                PropertyResult? result = dynamicLocation.Evaluate(action);

                if (result != null
                    && result.Value.LocationResult.HasValue)
                {
                    offsetLocation = result.Value.LocationResult.Value;
                }
                else
                {
                    failReason = "Location did not evaluate to a result";
                    return false;
                }


                if (offsetLocation.HasValue)
                {
                    exploreLocationStart += offsetLocation.Value;
                }
             /*   else
                {
                    // cancel the spawn if the offset was not available...
                    failReason = dynamicLocation.PropertyKey + " dynamic offset location was null.";
                    return false;
                }*/
            }

            return true;
        }

       

    }
}

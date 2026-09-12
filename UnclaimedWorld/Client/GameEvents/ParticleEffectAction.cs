using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.ClientSide.GameEvents
{
    /// <summary>
    /// sets off some particle emitters at either a location or as an attachment to entities
    /// </summary>
    public class ParticleEffectAction : EventActionType
    {
     //   public enum TargetOfAction { TriggeringEntity, TargetEntity, Location }

       
      //  public TargetOfAction Target;

        /// <summary>
        /// particles associated with the anim
        /// </summary>
        public ParticleEmitterEffect[] ParticleEmitters;


        /// <summary>
        /// will attach the emitter to the target entity.
        /// can also set the emitter at an entity's current location without attaching it. To do this, set Attach = false in ParticleEmitterEffect
        /// </summary>        
       // public DynamicLocation DynamicLocation; //??

        public EvalNode Location; // NEW?

        /// <summary>
        /// define this to get an entity location, or to get an entity to attach the emitter to. (Attachment is defined in ParticleEmitter)
        /// </summary>
        public TargetObject UseLocationOfEntity;


        public float? Scale;

        public float? TimeBetweenEmissions;

        public double? DurationInSeconds;

        public ParticleEffectAction(string keyName): base(keyName)
        {

        }

        public ParticleEffectAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetedEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            Entity concernedEntity = null;

           
            Vector2 spawnLocation = Vector2.Zero; //LocationOffset ?? Vector2.Zero;


            if (Location != null)
            { 
                // get the location:
                PropertyResult? result = Location.Evaluate(action);

                if (result != null
                    && result.Value.LocationResult.HasValue)
                {
                    spawnLocation = result.Value.LocationResult.Value;
                }
                else
                {
                    failReason = "Location did not evaluate to a result";
                    return false;
                }
            }
            else if (UseLocationOfEntity != null)
            {
                bool failed = false;
                // get the entity and its location
                // attaching is optional
                List<IHasExposedProperties> result = UseLocationOfEntity.GetResult(action);
                if (result != null && result.Count > 0)
                {
                    concernedEntity = result[0] as Entity;

                    if (concernedEntity != null)
                    {
                        spawnLocation = concernedEntity.PlaySiteLocation.ToVector2();
                    }
                    else
                    {
                        failed = true;
                    }
                }
                else
                {
                    failed = true;
                }

                if (failed)
                {
                    failReason = "EntityToAttachTo did not evaluate to an Entity result";
                    return false;
                }
            }
            else
            {
                failReason = "Neither EntityToAttachTo or Location was specified.";
                return false;
            }

            //OLD
          /*  if (DynamicLocation != null)
            {
                // add a dynamic offset, such as a starting location, the location of a triggering entity etc.
                Vector2? dynamicLocation = DynamicLocation.GetLocation(triggeringEntity, targetedEntity);
                if (dynamicLocation.HasValue)
                {
                    spawnLocation += dynamicLocation.Value;
                }
                else
                {
                    // cancel the spawn if the offset was not available...
                    failReason = DynamicLocation.PropertyKey + " dynamic offset location was null.";
                    return false;
                }

                // get the entity to attach to:
                if (DynamicLocation.TargetObject.TargetElement == TargetObjectType.TargetEntity)
                {
                    if (targetedEntity.HasValue)
                    {
                        concernedEntity = Entity.FindByID(targetedEntity.Value);
                    }
                }
                else if (DynamicLocation.TargetObject.TargetElement == TargetObjectType.TriggeringEntity)
                {
                    if (triggeringEntity.HasValue)
                    {
                        concernedEntity = Entity.FindByID(triggeringEntity.Value);
                    }
                }
            }*/

                    
            foreach (var item in ParticleEmitters)
            {
                if (concernedEntity == null || item.AttachToEntity == false) 
                {
                    The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, spawnLocation, null, null, DurationInSeconds, item.Offset); 
                }
                else 
                {
                    if (item.AttachToEntity)
                    {
                        // attach it:
                        The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, concernedEntity.Renderable, Scale, TimeBetweenEmissions, item.EmitParticlesInParentDirection, DurationInSeconds, item.Offset);
                    }
                    else
                    {
                        // use the location, but don't attach:
                        The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, spawnLocation, Scale, TimeBetweenEmissions, DurationInSeconds, item.Offset);
                    }
                }
            }

            return true;

        }

        public override void PreInitValidate(ref List<string> errors)
        {
            base.PreInitValidate(ref errors);

            if (Location == null && UseLocationOfEntity == null)
            {
                EntityType.CreateValidationError(ref errors, "Either Location or UseLocationOfEntity must be specified to place the emitter on the map");
            }

           /* if (Target == TargetOfAction.Location)
            {
                EntityType.ValidateRequiredValue(ref errors, "Location", LocationOffset.HasValue);
            }*/
        }


    }
}

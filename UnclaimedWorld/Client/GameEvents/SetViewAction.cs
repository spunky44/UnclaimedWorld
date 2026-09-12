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

namespace UWGame.ClientSide.GameEvents
{
   /// <summary>
   /// sets the client view port to the specified world location
   /// </summary>
    public class SetViewAction : EventActionType
    {
      
        /// <summary>
        /// MP this is an adjustment to the default camera location defined by the start coordinates. Turned out, it was too far to the right on bouth North and South cast locations, so I had this adjustment of -5 tiles put in.
        /// </summary>
        public Vector2 CenterOnLocation;

        /// <summary>
        /// forklaring 2
        /// </summary>
        public DynamicLocation OffsetToLocation;

        public SetViewAction(string keyName): base(keyName)
        {

        }

           public SetViewAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            
            Vector2 viewLocation = CenterOnLocation;

            if (OffsetToLocation != null)
            {
                // add a dynamic offset, such as a starting location, the location of a triggering entity etc.
                Vector2? offsetLocation = OffsetToLocation.GetLocation(action);
                if (offsetLocation.HasValue)
                {
                    viewLocation += offsetLocation.Value;
                }
                else
                {
                    // cancel the spawn if the offset was not available...
                    failReason = OffsetToLocation.PropertyKey + " dynamic offset location was null.";
                    return false;
                }
            }
                       
            The.MapUI.ZoomToMapPosition(viewLocation.ToVector3());

            return true;
           
        }


        public override string ToString()
        {
            return "SetView " + CenterOnLocation.ToString();
        }

       
    }
}

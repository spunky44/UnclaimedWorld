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

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnRouteAction : EventActionType
    {
        public RouteData RouteData;

         public SpawnRouteAction(string keyName): base(keyName)
        {

        }

         public SpawnRouteAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {
           
            if (!The.Sim.World.AllSites.ContainsKey(RouteData.FromSite))
            {
                failReason = "Site: " + RouteData.FromSite + " not found.";
                return false;
            }

            if (!The.Sim.World.AllSites.ContainsKey(RouteData.ToSite))
            {
                failReason = "Site: " + RouteData.ToSite + " not found.";
                return false;
            }


            if (RouteData != null)
            {
                Route.CreateFromRouteData(RouteData);
            }
            else
            {
                return false;
            }

            return true;               
            
        }

      

        public void PreInitValidate(List<string> listOfErrors)
        { 
        
        }

        public void Initialize()
        { 
        
        }

        public void PostInitValidate(List<string> listOfErrors)
        {
                      
        }

        public void PostLoadContentValidate(List<string> listOfErrors)
        {
        }


        public override string ToString()
        {
            return "Spawn route: " + this.RouteData.Name ?? RouteData.RouteType.ToString();
        }
    }
}

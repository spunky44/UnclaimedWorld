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
    public class SpawnWorldAction : EventActionType
    {
        public WorldData WorldData;

         public SpawnWorldAction(string keyName): base(keyName)
        {

        }

         public SpawnWorldAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {

            if (WorldData != null)
            {
                The.Sim.World = World.CreateFromWorldData(WorldData);
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
            if (WorldData == null)
            {
            }

        }

        public void PostLoadContentValidate(List<string> listOfErrors)
        {
        }


        public override string ToString()
        {
            return "Spawned World";
        }
    }
}



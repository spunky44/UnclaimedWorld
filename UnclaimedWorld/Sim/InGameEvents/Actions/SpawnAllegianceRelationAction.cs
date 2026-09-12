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

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnAllegianceRelationAction : EventActionType
    {
        public AllegianceRelationData AllegianceRelationData;

        public SpawnAllegianceRelationAction(string keyName): base(keyName)
        {

        }

        public SpawnAllegianceRelationAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {

            if (AllegianceRelationData != null)
            {
                ///Currently relation data doesnt contain ID, making it impossible to lookup in the dictionary if using id's
                ///this procedure is done in the CreateRelationFromData method for now.
                if (AllegianceRelation.CreateRelationFromData(AllegianceRelationData) == null)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;               
            
        }

      

        public override string ToString()
        {
            return "Spawn relation between " + this.AllegianceRelationData.Allegiance1 + " and " + this.AllegianceRelationData.Allegiance2;
        }
    }
}

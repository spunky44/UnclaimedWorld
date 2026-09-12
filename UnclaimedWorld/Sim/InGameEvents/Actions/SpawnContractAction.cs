using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.InGameEvents.Actions
{
  /*  public class SpawnContractAction : EventActionType
    {
        public ContractData ContractData;

        public SpawnContractAction(string keyName): base(keyName)
        {

        }

        public SpawnContractAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {
            if (ContractData != null)
            {
                if(ContractOLD.CreateFromContractData(ContractData) == null)
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
            return "Spawn contract between " + ContractData.AllegianceA + " and " + ContractData.AllegianceB;
        }
    }*/
}

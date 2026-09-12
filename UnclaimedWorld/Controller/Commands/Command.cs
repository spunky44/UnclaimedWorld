using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Commands;
using System.Xml.Serialization;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.Control.Commands
{
    // we cannot serialize enum IDs with XmlSerializer, it complains about values not in the definition... so just use long instead and cast. :(
    // remember to make all members public for XmlSerializer.

    // add more hints here...
    [XmlInclude(typeof(HuntArea))]
    [XmlInclude(typeof(Build))]
    [XmlInclude(typeof(SetProduction))]
    [XmlInclude(typeof(Hunt))]
    [XmlInclude(typeof(Salvage))]
    [XmlInclude(typeof(PlaceExpedition))]
    [XmlInclude(typeof(Discard))]
    [XmlInclude(typeof(Claim))]
    [XmlInclude(typeof(Scout))]
    [XmlInclude(typeof(CancelJob))]
    [XmlInclude(typeof(Examine))]
    [XmlInclude(typeof(PatrolArea))]
    [XmlInclude(typeof(SetTaskPriority))]
    [XmlInclude(typeof(SetJobTypePriority))]
    [XmlInclude(typeof(CreateMission))]
    [XmlInclude(typeof(CreateMissionTemplate))]
    [XmlInclude(typeof(DeleteZone))]
    [XmlInclude(typeof(Gather))]
    [XmlInclude(typeof(CreateStockpile))]
    [XmlInclude(typeof(SpecialAction))]
    [XmlInclude(typeof(Pause))]
    [XmlInclude(typeof(Resume))]
    [XmlInclude(typeof(SetGameSpeed))]
    [XmlInclude(typeof(AdoptTierPolicy))]
    [XmlInclude(typeof(SetUpgrade))]
    [XmlInclude(typeof(SetStandingOrder))]
    [XmlInclude(typeof(SetStandingOrderGatherInZone))]
    [XmlInclude(typeof(SetStandingOrderHuntInZone))]
    [XmlInclude(typeof(AttackArea))]
    [XmlInclude(typeof(AttackAreaUpdateJob))]
    [XmlInclude(typeof(PatrolAreaUpdateJob))]
    [XmlInclude(typeof(AllowAmmoForVermin))]    
    public abstract class Command
    {
        
        public int frameCalled;

        /// <summary>
        /// don't call directly - use StoreAndExecuteCommand
        /// </summary>
        /// <param name="giveClientFeedback"></param>
        public abstract void Execute(bool giveClientFeedback = true);


       
    }
}

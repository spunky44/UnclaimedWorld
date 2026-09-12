using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions.Templates;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// store the data needed to recreate a mission... we cannot XML-serialize the actual objects.
    /// </summary>
    public class CreateMissionTemplate : Control.Commands.Command
    {       
        //AllegianceID allegianceID;

        public MissionTemplate MissionTemplate;


        public CreateMissionTemplate(MissionTemplate missionTemplate)
        {
            this.MissionTemplate = missionTemplate;
        }

        public CreateMissionTemplate()
        {
        }

        public override void Execute(bool giveClientFeedback)
        {
           // Allegiance allegiance = (Allegiance)LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);

            // assign the IDs now:
           // MissionTemplate.AddToLookup();
            MissionTemplate.AssignIDs();


        }
    }
}

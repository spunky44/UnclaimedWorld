using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions
{
    /// <summary>
    /// while loading, immigrants should be able to get onboard...
    /// </summary>
    public class LoadAction : MissionAction
    {
        const double timeInDaysToLoad = 0.1;

        double elapsedTime = 0;

        public LoadActionTemplate LoadActionType;
        private MissionActionTemplateID snapshotActionTemplateID;

        public LoadAction()
        { }

        public LoadAction(Mission parent, LoadActionTemplate template) // Dictionary<EntityType, int> orders)
            : base(parent)
        {
            LoadActionType = template;
        }

       
        public override bool Update(Microsoft.Xna.Framework.GameTime elapsed)
        {
            base.Update(elapsed);

            elapsedTime += The.Sim.DateAndTime.DaysPerSecond * elapsed.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > timeInDaysToLoad)
            {

                return true;
            }

            return false;

        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(LoadActionType);

            return base.DoSnapshot(sn);


        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            LoadActionType = (LoadActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);

            base.LoadPostProcess(sn);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.Steam;

namespace UWGame.SimSide.Allegiances.Statistics
{
    /// <summary>
    /// used for achievements!
    /// </summary>
    public class KillStatistics : ISnapshot
    {

        public Dictionary<EntityType, int> Kills = new Dictionary<EntityType,int>();


        public void AddKillEvent(EntityType victim)
        {
            Common.AddToDictWithSums(Kills, victim);

            CheckAchievements();
        }


        private void CheckAchievements()
        {
            StartScenarioParams parms = The.Sim.StartGameParams.StartScenarioParams;
            if (The.Sim.StartGameParams.GetRGScenario() == StartGameParams.RGScenario.MuckrootMiningCamp)
            {
                if (!The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.swarmerKills))
                {
                    int swarmerKills;
                    if (Kills.TryGetValue(GameData.Instance.AllEntityTypes["entity:swarmer"], out swarmerKills)
                        && swarmerKills >= 300)
                    {
                        The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.swarmerKills);
                    }
                }
            }
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Kills = sn.DoDictionary(Kills);


            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }


        #endregion
    }
}

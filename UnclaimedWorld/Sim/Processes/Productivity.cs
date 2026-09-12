using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes
{
    /// <summary>
    /// only processes wtih tools or workers should track their productivity
    /// </summary>
    public class Productivity: ISnapshot
    {
        public float SkillProductivity { get; private set; }
        public float ToolProductivity { get; private set; }
        public float EnergyProductivity { get; private set; }
        public float TotalProductivity { get; private set; }


        public Productivity()
        {

        }


        public Productivity(Productivity original)
        {
            this.SkillProductivity = original.SkillProductivity;
            this.ToolProductivity = original.ToolProductivity;
            this.EnergyProductivity = original.EnergyProductivity;
            this.TotalProductivity = original.TotalProductivity;
        }

        private float GetMean(float val1, float val2)
        {
            return (val1 + val2) / 2f;
        }

        public void Merge(Productivity mergeWith)
        {
            TotalProductivity = GetMean(TotalProductivity, mergeWith.TotalProductivity);
            SkillProductivity = GetMean(SkillProductivity, mergeWith.SkillProductivity);
            ToolProductivity = GetMean(ToolProductivity, mergeWith.ToolProductivity);
            EnergyProductivity = GetMean(EnergyProductivity, mergeWith.EnergyProductivity);
        }

        public void SaveProductivityStats(float progressDelta, float toolProductivity, float skillProductivity, float energyProductivity, float updateInterval) //, float totalProductivity)
        {
            this.ToolProductivity += progressDelta * toolProductivity;
            this.SkillProductivity += progressDelta * skillProductivity;
            this.EnergyProductivity += progressDelta * energyProductivity;

            // make include other factors:
            this.TotalProductivity += progressDelta / updateInterval; // GetUpdateInterval();
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
            this.ToolProductivity = sn.DoFloat(ToolProductivity);
            this.EnergyProductivity = sn.DoFloat(EnergyProductivity);
            this.SkillProductivity = sn.DoFloat(SkillProductivity);
            this.TotalProductivity = sn.DoFloat(TotalProductivity);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}

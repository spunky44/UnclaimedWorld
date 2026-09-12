using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors.Stances
{
    public class Stance: ISnapshot
    {
        public Locomotor Parent;

        private StanceType currentStance;
        public StanceType CurrentStance
        {
            get
            {
                return currentStance;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value != null, "Stance must not be null");
                currentStance = value;
            }
        }

        public Stance() { }

        public Stance(Locomotor parent)
        {
            this.Parent = parent;

            this.currentStance = parent.Parent.EntityType.LocomotorType.StancesType.DefaultStanceType; // let's set a default stance instead of null.
        }

        public StanceType PickRandomStance(IList<ChanceToTakeStance> stancesToSelectFrom, StanceType defaultStance)
        {
            StanceType bestStance = null;
            if (stancesToSelectFrom != null)
            {
                double bestScore = -1;
                double currentScore = 0;
                foreach (var item in stancesToSelectFrom)
                {
                    currentScore = ScoreStance(/*currentStance,*/ item,
                        stancesToSelectFrom.Count);

                    if (currentScore >= bestScore)
                    {
                        bestScore = currentScore;
                        bestStance = item.StanceType;
                    }
                }

            }
            else
            {
                bestStance = defaultStance; // DefaultStanceTypeWhenWorking;
            }

            return bestStance;
        }


       // 


        public StanceType PickRandomProcessStance(Dictionary<StancesType, List<ChanceToTakeStance>> stanceTypes)
        {
            StanceType defaultStance = Parent.Parent.EntityType.LocomotorType.StancesType.DefaultStanceTypeWhenWorking;
            List<ChanceToTakeStance> stancesToSelectFrom = null;

            if (stanceTypes != null)
            {
                stanceTypes.TryGetValue(Parent.Parent.EntityType.LocomotorType.StancesType,
                    out stancesToSelectFrom);
            }

            return PickRandomStance(stancesToSelectFrom, defaultStance);

        }


        public double ScoreStance(/*StanceType evaluatedStance, StanceType currentStance,*/ ChanceToTakeStance chanceToTakeStance, int totalStances) // float chanceToUseStanceFactor, float remainInCurrentStanceAddend)
        {
            double random;
            float chanceToUseStanceFactor = chanceToTakeStance.Chance ?? 1f / (float)totalStances;
            random = chanceToUseStanceFactor * The.Sim.GameplayRandomGenerator.NextDouble("Goal");

            double stanceAddend = 0;
            if (currentStance == chanceToTakeStance.StanceType) // evaluatedStance)
            {
                stanceAddend = chanceToTakeStance.AddedChanceToRemainInStance ?? 0f;
            }

            return random + stanceAddend;
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
            this.currentStance = sn.DoGameData(currentStance); 
          

            sn.Ignore(Parent); // is re-assigned from Locomotor

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }


        #endregion
    }
}

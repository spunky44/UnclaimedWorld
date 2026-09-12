using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants
{
    public class EvaluatorWeights
    {
        public float HarvestJobScoreWeight = 0.75f;
        public float HarvestTravelScoreWeight = 0.15f;
        public float HarvestToolScoreWeight = 0.10f;

        public float ProcessJobScoreWeight = 0.75f;
        public float ProcessTravelScoreWeight = 0.15f;
        public float ProcessToolScoreWeight = 0.10f;

        public float ProcessImportanceWeight = 0.1f;
        public float ProcessSkillWeight = 0.25f;
        public float ProcessProgressWeight = 0.15f;

        public float HarvestImportanceWeight = 0.1f;
        public float HarvestSkillWeight = 0.25f;
        public float HarvestProgressWeight = 0.15f;


        public float MundaneJobSpecialistPenalty = 0.1f;

        /// <summary>
        /// sub-weights
        /// </summary>
        public float UniqueSkillWeight = 0.15f;

        public double HaulJobTravelWeight = 0.3; // 0.35;
        public double HaulJobUrgencyWeight = 0.2;
        public double HaulJobMemoryWeight = 0.2;
        public double HaulJobStarvationWeight = 0.1;

        public double HaulJobAddend = 0.15;


        public float FindPreyImportanceWeight = 0.1f;
        public float FindPreySkillWeight = 0.25f;
        public float FindPreyTravelTimeWeight = 0.45f;

    }
}

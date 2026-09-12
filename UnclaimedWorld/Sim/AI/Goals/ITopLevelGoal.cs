using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// to be implemented by the goals that can be created and started from an Evaluator
    /// </summary>
    public interface ITopLevelGoal
    {
        double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// should be the final score, including Priority, but not Inertia!
        /// </summary>
        /// <returns></returns>
        double ScoreGoal();

    }
}

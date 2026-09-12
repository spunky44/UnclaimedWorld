using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// can be a house, camp, mobile home...
    /// </summary>
    public class ResidenceType
    {
        /// <summary>
        /// 0 - 1
        /// combined with Condition, it determines sleep need regain rate
        /// </summary>
        public float ComfortLevel;


        /// <summary>
        /// Max number of residents that can comfortably live here (beds?). Max people capacity is something else...
        /// </summary>
        public int LivingCapacity = 0;

        /// <summary>
        /// max no of people that can stay inside - by defualt, this will be the same as LivingCapacity
        /// </summary>
        public int PeopleCapacity;


        public void Initialize()
        {
            PeopleCapacity = Math.Max(PeopleCapacity, LivingCapacity);

        }
    }
}

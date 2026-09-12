using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants.Rating
{
    public class Ratings
    {
        
        public float PrinciplesAdaptationSpeedPerSecond = 0.00005f; //.........was: 0.0005f: 20 secs to move one point

        /// <summary>
        /// the amount above colony ratings that principles will converge to
        /// </summary>
        public float PrinciplesTargetDelta = .05f;

        public Security Security;
        public Comfort Comfort;
        public Food Food;

        /// <summary>
        /// happiness below this limit may trigger the dreaded GROUP MEETING
        /// </summary>
        public float HappinessLimitForGroupMeeting = -0.05f;

        /// <summary>
        /// the amount of people that must be unhappy to trigger the meeting
        /// </summary>
        public float UnhappyExpeditionMembersPercentageForGroupMeeting = 0.25f;


        public Ratings()
        {
            Security = new Security();
            Comfort = new Comfort();
            Food = new Food();
        }
    }
}

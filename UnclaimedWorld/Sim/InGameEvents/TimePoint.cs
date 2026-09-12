using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents
{
    /// <summary>
    /// we don't need these date options anymore with the UnaryFunction conversions
    /// </summary>
    public class TimePoint
    {
        /// <summary>
        /// fill in one of the below
        /// 
        /// set an ABSOLUTE time and day in the game's calendar
        /// </summary>
        public DateAndTime.TimeDateYear? Date;


        /// <summary>
        /// set here the no. of in-game days AFTER the start of the game
        /// </summary>
        public double? RelativeNoOfDays;

        /// <summary>
        /// the no. of in-game days AFTER the start of the game.
        /// the same as the above but can use an expression
        /// </summary>
        public EvalNode DynamicRelativeNoOfDays;

        /// <summary>
        /// set the no. of seconds after the start of the game
        /// </summary>
        public EvalNode RelativeTimeInSeconds;

        /// <summary>
        /// TODO: keep this, delete the others. Use UnaryFunction to convert.
        /// </summary>
        public EvalNode AbsoluteDate;


        [XmlIgnore]
        private double? timeInSeconds;

        /// <summary>
        /// do a lazy init of this value in realtime seconds
        /// </summary>
        [XmlIgnore]
        public double TimeInSeconds
        {
            get
            {
                if (!timeInSeconds.HasValue)
                {
                    timeInSeconds = GetTimepointInSecondsOfElapsedGameTime();
                }

                return timeInSeconds.Value;
            }
        }


      
        public double? GetUpdateInterval()
        {
            return UpdateTimePoints.ComputeIntervalFromTimepoint(TimeInSeconds).Value;
        }



        /// <summary>
        /// returns the time in seconds that must pass after the start of the game
        /// </summary>
        /// <returns></returns>
        private double? GetTimepointInSecondsOfElapsedGameTime()
        {
            if (AbsoluteDate != null)
            {
                PropertyResult? result = AbsoluteDate.Evaluate(null, null, null, null);

                if (result != null && result.Value.DateResult.HasValue)
                {
                    DateAndTime.TimeDateYear startDate = The.Sim.DateAndTime.StartTimeDateYear;

                    return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(result.Value.DateResult.Value) - The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(startDate); 
                }
            }
            else if (DynamicRelativeNoOfDays != null)
            {
                PropertyResult? result = DynamicRelativeNoOfDays.Evaluate(null, null, null, null);

                if (result != null && result.Value.NumberResult.HasValue)
                {
                    double totalDays = result.Value.NumberResult.Value;
                    DateAndTime.TimeDateYear date = new DateAndTime.TimeDateYear(totalDays);

                    // we don't have to subtract the start date.
                    return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(date);

                }

                //  return result.Value.NumberResult.Value;
            }
            else if (RelativeNoOfDays.HasValue)
            {

                DateAndTime.TimeDateYear date = new DateAndTime.TimeDateYear(RelativeNoOfDays.Value);

                // we don't have to subtract the start date.
                return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(date);

            }
            else if (RelativeTimeInSeconds != null)
            {
                PropertyResult? result = RelativeTimeInSeconds.Evaluate(null, null, null, null);

                if (result != null)
                {
                    return result.Value.NumberResult;
                }
            }
            else
            {
                // an absolute game date was supplied. compute the difference to the scenario start date and return the value in real time seconds:

                DateAndTime.TimeDateYear startDate = The.Sim.DateAndTime.StartTimeDateYear;

                DateAndTime.TimeDateYear? dateToFire = null;

                if (Date.HasValue)
                {
                    dateToFire = Date.Value;
                }


                if (dateToFire.HasValue)
                {
                    // convert to real time seconds:
                    return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(dateToFire.Value) - The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(startDate);
                }
                else return null;

            }

            return null;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using System.Globalization;

namespace UWGame.SimSide
{
    public class DateAndTime: ISnapshot
    {
       
        /// <summary>
        /// we save the start point for the scenario here. its value is used in timed conditions
        /// </summary>
        public TimeDateYear StartTimeDateYear;

        /// <summary>
        /// returns a new instance that is safe to add to
        /// </summary>
        public TimeDateYear CurrentTimeDateYear
        {
            get
            {
                return new TimeDateYear(this.TimeOfDay, this.Day, this.Year);
                
            }
        }

        /// <summary>
        /// the sun position will be computed from this.
        /// 0 and 1.0 = midnight, 0.5 = midday. Time of day is set in top of the mapdata xml file for the loaded map.
        /// 
        /// </summary>
        public double TimeOfDay = 0.4; // 0.78; 
               

        public int Year = 15;

        /// <summary>
        /// TODO: replace with TimeDateYear struct. But first figure out if Day is 0- or 1-based...
        /// should only be used for display, internally, use a decimal format: Year.TimeOfYear
        /// </summary>
        public int Day = 1;


        /// <summary>
        /// the sun position will be computed from this.
        /// 0 - 1. 0 is 'January 1st, or New year'
        /// </summary>
        public double TimeOfYear = 0.53; //0;

        private Vector3? sunPosition = new Vector3(0f, 0f, -1f);

        public bool SunIsUp;

        /// <summary>
        /// (on norhtern hemisphere?) 0f = sun is to the south, shadows point north. Pi/2 = Sun is to the east. Pi = Sun is to the North, Pi + Pi/2 = sun is to the west.
        /// </summary>
        public float SunAzimuth = 0f;

        public float UnshiftedAzimuth = 0f;

        /// <summary>
        /// sun is up: between 0 and pi. down: between pi and 2*pi
        /// </summary>
        public float SunElevation;

        /// <summary>
        /// tilt of the planet
        /// </summary>
        private const float axialTilt = MathHelper.Pi / 8f;

        /// <summary>
        /// TODO: hook up with Geodetic coordinates in Site
        /// longitude is assumed 0...
        /// 
        /// equator = 0f, north pole = Pi/2
        /// </summary>
        private const float latitude = MathHelper.PiOver4;
        private float cosLatitude; 
        private float sinLatitude; 

        private bool isOnNorthernHemisphere;

       

        private static string[] timeOfDayStrings = new string[] { "Night", "Morning", "Noon", "Afternoon", "Evening", "Night" };
        private static float[] timesOfDay = new float[] { 0.15f, 0.3f, 0.6f, 0.75f, 0.9f, 1.0f }; // upper end of range

        private static string[] seasonStrings = new string[] { "Late winter", "Start of spring", "Spring", "Late spring", "Start of summer", "Mid summer", "Late summer", 
                                                        "Start of autumn", "Mid autumn", "Late autumn", "Start of winter", "Mid winter"};
       
        /// <summary>
        /// the speed the planet rotates around itself.
        /// </summary>
        //private const float rotationSpeed = (13f / 12f) * MathHelper.TwoPi;


      

        // -12 degrees - nautical dawn on earth
        public const float dawnSunElevation = -0.209f;
        
        // 6 degrees - my own choice.
        public const float dawnSunElevationEnd = 0.104f;
              

        // 6 degrees - my own choice.
        public const float sunsetElevationStart = 0.104f;

        // -18 degrees - Dusk on earth.
        public const float sunsetElevationEnd = -0.314f;

        

        /// <summary>
        /// perhaps read from Constants...
        /// </summary>
        public static double secondsPerDay = 1600; // This is the time we are designing the game for 11/8/2013//; 80; // 1800; // 1600; // 1800; // 800; //80; // 180; //1800; //3000; //720;//260; 180; // good for needs testing  ..for timelapse, SPEED OF SUN , SECONDS PER DAY
        public double SecondsPerDay
        {
            get
            {
                return secondsPerDay;
            }
            set
            {
                secondsPerDay = value;

                ComputeSpeed();               
            }
        }

      //  public const double SecondsPerDay = 380; 
        public const double DaysPerSeason = 3;
        public const double DaysPerYear = 12;
      
        public double YearsPerSecond {get; private set;} // = 1.0 / (SecondsPerDay * 4.0 * DaysPerSeason);
        public double DaysPerSecond {get; private set;} // = 1.0 / secondsPerDay;

        Regulator displayDateRegulator;

        public Sim.DayPhases CurrentPhase;

        private float? lightLevel;
        public float LightLevel
        {
            get
            {
                if (!lightLevel.HasValue)
                {
                    lightLevel = GetLightLevel();
                }

                return lightLevel.Value;
            }
        }

         NumberFormatInfo numberFormat;


        public DateAndTime()
        {
            cosLatitude = (float)Math.Cos(latitude);
            sinLatitude = (float)Math.Sin(latitude);

            isOnNorthernHemisphere = (latitude > 0f && latitude < MathHelper.PiOver2);

            ComputeSpeed();

            if (!Snapshotter.IsSnapshotting)
            {
                CreateRegulators();

                CreateNumberFormat();

                UpdateAfterAdvancing();
                //Update(null);
            }
        }

        void CreateNumberFormat()
        {
            numberFormat = new NumberFormatInfo();
            numberFormat.NumberDecimalSeparator = ".";
        }

        void CreateRegulators()
        {
            displayDateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 4, "DataAndTimeDisplayData");
        }

        private void ComputeSpeed()
        {
            YearsPerSecond = 1.0 / (secondsPerDay * 4.0 * DaysPerSeason);
            DaysPerSecond = 1.0 / secondsPerDay;
        }


       
        public void ResetTimeOfYearAndTimeOfDay(TimeDateYear timeDateYear) // int dayNo, float timeOfDay)
        {
            if (timeDateYear.Day > DaysPerYear)
            {
                throw new Exception("Day no. too high.");
            }

            StartTimeDateYear = timeDateYear;

            TimeOfDay = timeDateYear.TimeOfDay;
            TimeOfYear = timeDateYear.Day / DaysPerYear;
            
            Year = timeDateYear.Year;
            Day = timeDateYear.Day;

            ComputeSunAndLight();
        }

        

       

        public Vector3 SunPosition
        {
            get
            {
                return sunPosition.Value;                
            }
        }

        /// <summary>
        /// ???? not the timestamp format we want
        /// </summary>
        /// <returns></returns>
        public DateTime GetTime()
        {
            float hourLength = 1f / 24f;
            double hourFraction = TimeOfDay % hourLength;
            // float hoursTime = 2400f * timeOfDay;
            DateTime time = new DateTime(1, 1, 1, Common.Clamp((int)(24f * TimeOfDay), 0, 23), Common.Clamp((int)(hourFraction * 60f), 0, 59), 0);

            return time;
        }

      /*  public string TimeToString(float timeOfDay)
        {
            DateTime time = GetTime(); // new DateTime(1, 1, 1, Common.Clamp((int)(24f * timeOfDay), 0, 23), Common.Clamp((int)(hourFraction * 60f), 0, 59), 0);
            
            //if (UserSettings.Culture.)

            return time.ToShortTimeString(); //UserSettings.Culture);
        }*/

       /* public void UpdateDayAndYear(GameTime elapsed)
        {
            DateAndTime.UpdateDayAndYear(elapsed);

            if (DateAndTime.TimeOfDay >= 0 && DateAndTime.TimeOfDay < WorkPhaseStarts)
            {
                DateAndTime.CurrentPhase = DayPhases.Sleep;
            }
            else if (DateAndTime.TimeOfDay >= WorkPhaseStarts && DateAndTime.TimeOfDay < LeisurePhaseStarts)
            {
               
                DateAndTime.CurrentPhase = DayPhases.Work;
            }
            else if (DateAndTime.TimeOfDay >= LeisurePhaseStarts && DateAndTime.TimeOfDay < SleepPhaseStarts)
            {

                DateAndTime.CurrentPhase = DayPhases.Leisure;
            }
            else
            {
                DateAndTime.CurrentPhase = DayPhases.Sleep;
            }

            DateAndTime.UpdateDisplayDate(elapsed);
        }*/

        public void AdvanceTime(double seconds)
        {
            AddTime(seconds * DaysPerSecond);

            UpdateAfterAdvancing();
        }
       
        public void Update(GameTime gameTime)
        {
            // I think adding small intervals will cause drifting. It would be better to derive the date from the Sim time instead
            AddTime(gameTime.ElapsedGameTime.TotalSeconds * DaysPerSecond);

            UpdateAfterAdvancing();
        }

        private void UpdateAfterAdvancing()
        {
            if (TimeOfDay >= 0 && TimeOfDay < Sim.WorkPhaseStarts)
            {
                CurrentPhase = Sim.DayPhases.Sleep;
            }
            else if (TimeOfDay >= Sim.WorkPhaseStarts && TimeOfDay < Sim.LeisurePhaseStarts)
            {
                CurrentPhase = Sim.DayPhases.Work;
            }
            else if (TimeOfDay >= Sim.LeisurePhaseStarts && TimeOfDay < Sim.SleepPhaseStarts)
            {
                CurrentPhase = Sim.DayPhases.Leisure;
            }
            else
            {
                CurrentPhase = Sim.DayPhases.Sleep;
            }

            UpdateDisplayDate();

            ComputeSunAndLight();
        }

        private void ComputeSunAndLight()
        {
            
            //  SunAzimuth += 0.3f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //  SunAzimuth = SunAzimuth % MathHelper.TwoPi;

            //the hour angle, h, is negative (morning) and the angle west of south when the hour angle, h, is positive (afternoon).
            // 0 at noon!
            float hourAngle = (float)((TimeOfDay - 0.5) * MathHelper.TwoPi);
            float cosHourAngle = (float)(Math.Cos(hourAngle));

            // goes from -axialTilt in the winter to +axialTilt in summer.
            float declination = (float)(axialTilt * (-1d + 2d * TimeOfYear));
            float cosDeclination = (float)(Math.Cos(declination));

            /*
             * The solar elevation angle is the angle between the direction of the sun and the ground. 
             * */
            double sinSunElevation = cosHourAngle * cosDeclination * cosLatitude + Math.Sin(declination) * sinLatitude;

            // Is this correct???
            SunElevation = (float)(Math.Asin(sinSunElevation));
            /*     if (SunElevation < 0f)
                 {
                     SunElevation += MathHelper.TwoPi;
                 }*/

            bool sunWasUp = SunIsUp;
            SunIsUp = SunElevation > 0f && SunElevation < MathHelper.Pi;

            if (SunIsUp != sunWasUp)
            {
                SetTreeStateChanges(!SunIsUp, ClientSide.Renderables.StateModifier.Night);
            }

            if (SunIsUp)
            {
                /* For example: x = sin-1(y) has more than one solution, therefore at a northern latitude in summer, 
                 * when the azimuth at sunrise should be a positive number greater than 90 degrees, 
                 * inverse of sine will incorrectly yield an angle between 0 and 90 degrees that has the same sine.
                 */
                double sinSunAzimuth = (-Math.Sin(hourAngle) * cosDeclination) / Math.Cos(SunElevation);
                if (sinSunAzimuth < -1.0 || sinSunAzimuth > 1.0)
                {
                    // Gives ERROR!
                    sinSunAzimuth = Common.Clamp(sinSunAzimuth, -1.0, 1.0);
                }

                SunAzimuth = (float)(Math.Asin(sinSunAzimuth));
                UnshiftedAzimuth = SunAzimuth;

                // not sure if it is important to use the unshifted Azimuth...
               // ShadowXAlignment = MathHelper.SmoothStep(0f, 1f, Math.Abs(UnshiftedAzimuth) / MathHelper.PiOver2);

                if (isOnNorthernHemisphere) // && SunAzimuth > 0 && SunAzimuth < MathHelper.PiOver2)
                {
                    SunAzimuth = MathHelper.Pi - SunAzimuth;
                }


                sunPosition = new Vector3((float)sinSunAzimuth, -(float)Math.Cos(SunAzimuth), -(float)sinSunElevation);
                sunPosition = Vector3.Normalize(sunPosition.Value);

                /*
                SunShadowRotationMatrix = Matrix.CreateRotationZ(SunAzimuth - MathHelper.Pi) * shadowWarping; // warp the shadows so they are aligned with the model shadows
                
                ShadowLength = MathHelper.Clamp((float)(1.0 / Math.Tan(SunElevation)), 0.1f, maxShadowLengthScaling);

                ShadowLength = MathHelper.SmoothStep(0.1f, maxShadowLengthScaling, ShadowLength / maxShadowLengthScaling);

                // make long shadows slimmer:
                ShadowScaling = Matrix.CreateScale(1f - 0.5f * (ShadowLength / maxShadowLengthScaling), ShadowLength, 1f);
                */
            }

           
            lightLevel = GetLightLevel();
        }


        private void SetTreeStateChanges(bool value, StateModifier modifier)
        {
            Site site = The.Sim.PlaySite;

            if (site == null)
                return;


            Entity entity;
            for (int i = 0; i < site.Entities.Count; i++) // will this cause stutter...?
            {
                entity = site.Entities[i];

                if (entity.EntityType.TreeType != null)
                {                    
                    /*if (entity.EntityType.TreeType.HasDayNightCycle)
                    {*/
                    entity.Renderable.SetOrClearSpriteStateFlag(value, modifier);
                       
                           // season = "_day";                      
                           // season = "_night";                                         
                                      
                }
            }         

        }

        public void UpdateDisplayDate()
        {
            if (displayDateRegulator.IsReady())
            {               

                if (The.InGameUI != null)
                {
                    string timeOfDayString = GetTimeOfDayAsString(TimeOfDay);

                   // int dayNo = GetDayNo();
                
                    string season = seasonStrings[(int)(TimeOfYear * seasonStrings.Length)];
                   
                    The.InGameUI.SetTimeAndDate(timeOfDayString, season, CurrentTimeDateYear.ToString());
                       // (float)Year + TimeOfYear); // Year);
                }

                /*
                System.Text.StringBuilder date = new System.Text.StringBuilder();
                date.Append(timeOfDayString);
                date.Append(" (");
                date.Append(CurrentPhase.ToString());
                date.Append(")");
                date.Append(", ");
                date.Append(season);
                date.Append(", ");
                date.Append(Year.ToString());

                DateDisplayString = date.ToString();*/

            }
            
        }

        public static string GetTimeOfDayAsString(double timeOfDay)
        {
            int timeIndex = 0;
            timeIndex = Common.GetStairStepIndex((float)timeOfDay, timesOfDay);

            string timeOfDayString = timeOfDayStrings[timeIndex];
            return timeOfDayString;
        }

       /* public int GetDayNo()
        {
            return Day;
          
        }*/

        /// <summary>
        /// This is a Sim value.
        /// not taking clouds, moon etc. into account.
        /// </summary>
        /// <returns></returns>
        public float GetLightLevel()
        {
            if (TimeOfDay < 0.5) // after midnight, before noon A.M
            {
                return 1f - MathHelper.Clamp((SunElevation) / (dawnSunElevation), 0f, 1f); 

            }
            else // P.M
            {
                return 1f - MathHelper.Clamp((SunElevation) / (sunsetElevationEnd), 0f, 1f);   

            }

        }

       

     
   /*     public float GetMorningBeforeSunriseAnimationProgress()
        {
            return MathHelper.Distance(SunElevation, dawnSunElevation) / Math.Abs(dawnSunElevation);
            //return Math.Abs(SunElevation / Math.Abs(dawnSunElevation) //* oneOverMorningDuration; 
        }

        public float GetMorningAfterSunriseAnimationProgress()
        {
            return MathHelper.Distance(SunElevation, dawnSunElevationEnd) / dawnSunElevationEnd;           
        }*/

        public float GetProgress(float elevationPoint)
        {
            return MathHelper.Distance(SunElevation, elevationPoint) / Math.Abs(elevationPoint);
        }

    

        /// <summary>
        /// advances the date and time in the game
        /// </summary>
        /// <param name="timeInDays"></param>
        private void AddTime(double timeInDays)
        {
            TimeOfDay += timeInDays;

            double yearsToAdd = (timeInDays / DaysPerSecond) * YearsPerSecond;

            TimeOfYear += yearsToAdd;

            if (TimeOfYear > 1)
            {
                TimeOfYear = TimeOfYear - 1; 
                Year++;

                Day = 1;
            }

            if (TimeOfDay > 1)
            {
                TimeOfDay = TimeOfDay - 1;

                Day++;
            }
        }

       
     
        public static bool DateIsAfter(TimeDateYear date, TimeDateYear compareToDate) // double compareToTimeOfDay, int compareToDate)
        {
            if (date.Year > compareToDate.Year)
            {
                return true;
            }
            else if (date.Year == compareToDate.Year)
            {
                if (date.Day > compareToDate.Day)
                {
                    return true;
                }
                else if (date.Day == compareToDate.Day)
                {
                    return date.TimeOfDay > compareToDate.TimeOfDay;
                }
                else return false;
            }
            else return false;
        }

      /*  public void AddTime(double time)
        {
            double newTime = TimeOfDay + time;

            if (newTime > 1d)
            {
                double newTimeNextDay = newTime - 1d;

                TimeOfDay = newTimeNextDay;

                DayHasPassed();
            }
        }*/

        public static double GetTimeDifferenceInDays(DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
        {
            double toInDays = to.TotalDays; // to.TimeOfDay + to.Day + (to.Year * DaysPerYear);
            double fromInDays = from.TotalDays; // from.TimeOfDay + from.Day + (from.Year * DaysPerYear);

            return toInDays - fromInDays;
        }



       /* public static int CompareDates(UWGame.SimSide.Maps.MapEditor.EntityData s1, UWGame.SimSide.Maps.MapEditor.EntityData s2) // double compareToTimeOfDay, int compareToDate)
        {
            return CompareDates(s1.SpawnDate.Value, s2.SpawnDate.Value);
        }*/

        /// <summary>
        /// 2 is greater : returns -1
        /// equal: return 0
        /// 1 is greater: return 1
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int CompareDates(TimeDateYear date1, TimeDateYear date2) // double compareToTimeOfDay, int compareToDate)
        {
            if (date1.Year > date2.Year)
            {
                return 1;
            }
            else if (date1.Year == date2.Year)
            {
                if (date1.Day > date2.Day)
                {
                    return 1;
                }
                else if (date1.Day == date2.Day)
                {
                    if (date1.TimeOfDay > date2.TimeOfDay)
                    {
                        return 1;
                    }
                    else if (date1.TimeOfDay == date2.TimeOfDay)
                    {
                        return 0;
                    }
                    else return -1;
                }
                else return -1;
            }
            else return -1;
        }

        public static TimeDateYear GetSecondsToIngameDays(float seconds)
        {
            float timeInDays = (float)The.Sim.DateAndTime.DaysPerSecond * seconds;
            DateAndTime.TimeDateYear timeStruct = new DateAndTime.TimeDateYear(timeInDays);

            return timeStruct;
        }

        public static bool DateIsAfter(double timeOfDay, int date, double compareToTimeOfDay, int compareToDate)
        {
            if (date > compareToDate)
            {
                return true;
            }
            else if (date == compareToDate)
            {
                return timeOfDay > compareToTimeOfDay;
            }
            else return false;
        }

        public double ConvertDateToRealTimeSeconds(TimeDateYear date)
        {
            double seconds = (date.TimeOfDay + date.Day) * SecondsPerDay + (double)date.Year / YearsPerSecond;

            return seconds;
        }


        public double MillisecondsToDays(double milliSecondsSinceLastReady)
        {
            return (milliSecondsSinceLastReady / 1000d) * DaysPerSecond;
        }


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.TimeOfDay = sn.DoDouble(TimeOfDay);
            this.Day = sn.DoInt32(Day);
            this.Year = sn.DoInt32(Year);
            this.TimeOfYear = sn.DoDouble(TimeOfYear);
            secondsPerDay = sn.DoDouble(secondsPerDay);
            this.CurrentPhase = sn.DoEnum(CurrentPhase);
            this.StartTimeDateYear = sn.DoTimeDateYear(StartTimeDateYear);

            // ignore derived fields:
            sn.Ignore(this.cosLatitude);
            sn.Ignore(this.CurrentPhase);
            sn.Ignore(this.sunPosition);
            sn.Ignore(SunIsUp);
            sn.Ignore(SunAzimuth);
            sn.Ignore(SunElevation);
            sn.Ignore(sinLatitude);
            sn.Ignore(cosLatitude);
            sn.Ignore(isOnNorthernHemisphere);
            sn.Ignore(YearsPerSecond);
            sn.Ignore(DaysPerSecond);
  
            sn.Ignore(timeOfDayStrings);
            sn.Ignore(timesOfDay);
            sn.Ignore(seasonStrings);
            sn.Ignore(lightLevel);

            sn.Ignore(numberFormat);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public bool IsSnapshotted
        {
            get;
            set;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            ComputeSpeed();

            ComputeSunAndLight();

            CreateRegulators();

            CreateNumberFormat();
        }


        #endregion

      

        public struct TimeDateYear: IComparable
        {
            // TODO: this struct should have a single decimal value and public getters for display etc.

            public double TimeOfDay; // { get; private set; }

            /// <summary>
            /// 0 based???
            /// </summary>
            public int Day; // { get; private set; }
            public int Year; // { get; private set; }


            public double TotalDays
            {
                get
                {
                    return TimeOfDay + Day + (Year * DaysPerYear);
                   
                }
            }

            /// <summary>
            /// returns y.tt
            /// </summary>
            /// <returns></returns>
            public double ToYearAndTimeOfYear()
            {
                return (double)Year + (double)Day / DaysPerYear + TimeOfDay;

            }

            public TimeDateYear(double totalDays)
            {
                TimeOfDay = 0;
                Day = 0;
                Year = 0;

                SetDateFromTotalDays(totalDays);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="timeOfDay">0 - 1</param>
            /// <param name="day"></param>
            /// <param name="year"></param>
            public TimeDateYear(double timeOfDay, int day, int year)
            {
                TimeOfDay = timeOfDay;
                Day = day;
                Year = year;

                // totalDays = 0;
                //  totalDaysAreDirty = true;
            }


           

            public void SetDateFromTotalDays(double totalDays)
            {
                double year = totalDays / DateAndTime.DaysPerYear;
                Year = (int)Math.Truncate(year);

                //double yearRemainder = Math.IEEERemainder(totalDays, DateAndTime.DaysPerYear);
                double yearRemainder = totalDays - Year * DateAndTime.DaysPerYear;
                Day = (int)Math.Truncate(yearRemainder);

                double dayFraction = totalDays - Math.Floor(totalDays);              
                TimeOfDay = dayFraction;

              //  int quotient = (int)Math.Truncate(totalDays);


            }

            public double ToRelativeDays()
            {
                double totalDays = TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays;

                return totalDays;
            }

            public double ToRelativeSeconds()
            {
                double totalDays = TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays;

                return DateAndTime.secondsPerDay * totalDays;
            }

            public double ToSeconds()
            {
                double totalDays = TotalDays;

                return DateAndTime.secondsPerDay * totalDays;
            }


            /// <summary>
            /// TOO CONFUSING? DELETE THIS?
            /// [18:37:54] Morten Pedersen: ska hellere være dato
            ///[18:38:09] Morten Pedersen: vi skal vænne spilleren til at kigge på uret øverst
            ///[18:38:32] Morten Pedersen: og alle deaths og andre events skal stampes med en dato
            ///[18:39:19] Morten Pedersen: også Harvest in: xx days på fields
            ///[18:39:24] Morten Pedersen: ska være dato
            ///[18:40:15] Morten Pedersen: så ka vi senere lave en alert kalender med upcoming events.
            ///
            /// displays the data as an amount of time rather than an absolute date, for instance for showing ETA
            /// </summary>
            /// <returns></returns>
            public string ToIntervalString()
            {
                string dayString = (Day + TimeOfDay).ToString("F2"); // "F1");
                if (Year > 0)
                {
                    return string.Format("{0} years, {1} days", Year, dayString); 
                }
                else
                {
                    return string.Format("{0} days", dayString); 
                }
            }

            /// <summary>
            /// returns DATE: yyyy.d.t
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {               
                //string day =  ((double)Day / DaysPerYear).ToString("F1", The.Sim.DateAndTime.numberFormat).Replace("0.", ""); // TrimStart('0', '.');
                string timeOfDay = (TimeOfDay).ToString("F1", The.Sim.DateAndTime.numberFormat).Replace("0.", ""); // TrimStart('0', '.');
               
                return string.Format("DATE: {0}.{1}.{2}", Year, Day, timeOfDay); 
                
                //string timeOfDayString = DateAndTime.GetTimeOfDayAsString(TimeOfDay);
                //return string.Format("Year: {0} {1}", ToYearAndTimeOfYear(), timeOfDayString);
              //  return string.Format("Year: {0} Day: {1} {2}", Year, Day, timeOfDayString );
            }

            /// <summary>
            /// adds current time
            /// </summary>
            public void ConvertToAbsoluteTime()
            {
                AddTime(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays);
            }


            /// <summary>
            /// will adjust day, time and year.
            /// use a negative parameter to subtract.
            /// </summary>
            /// <param name="timeInDays"></param>
            public void AddTime(double timeInDays)
            {
                // convert to total days, reset and recompute
                double newTotalDays = TotalDays + timeInDays;

                TimeOfDay = 0;
                Day = 0;
                Year = 0;

                SetDateFromTotalDays(newTotalDays);

            }

            /// <summary>
            ///  // DATE: Year 0, Month 4, Noon (EARTH DATE 06-10 2238) 
            /// </summary>
            /// <returns></returns>
            public string GetDateForJournal()
            {
                return string.Format("{0} ({1})", this.ToString(), GetEarthDate());                

            }


           


            /// <summary>
            /// for flavour only...
            /// </summary>
            /// <returns></returns>
            public string GetEarthDate()
            {
                // DATE: 06-10 2238 
                int year = Year + GameData.Instance.Constants.StartingYear;
                double yearProgress = Common.Clamp((this.Day + this.TimeOfDay) / DateAndTime.DaysPerYear, 0d, 1d);

                int month = (int)(yearProgress * 12);
                int daysPerMonth = DateTime.DaysInMonth(year, month);

                double monthProgress = yearProgress % 12;

                int day = Common.Clamp((int)(daysPerMonth * monthProgress), 1, daysPerMonth);

                DateTime earthDate = new DateTime(year, month, day);

                //if (Config.Culture.DateTimeFormat.ShortDatePattern )
              
                return "EARTH DATE: " + earthDate.ToShortDateString();

                //return string.Format("{0}-{1} {2}", day, month, year);
            }
            


            /// <summary>
            /// parameter is greater : returns -1,
            /// equal: return 0,
            /// this is greater: return 1
            /// </summary>
            /// <param name="date1"></param>
            /// <param name="date2"></param>
            /// <returns></returns>
            public int CompareTo(object obj)
            {
                return DateAndTime.CompareDates(this, (TimeDateYear)obj);
            }
        }

    }
}

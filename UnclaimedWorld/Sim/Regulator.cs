using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.Control;
using UWGame.Control.Replays;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{
    /// <summary>
    /// need one of two modes, Sim or Client - Sim uses TotalUnpausedGameTime so it won't ever be ready when paused!
    /// 
    /// enable snapshotting to preserve progress???
    /// </summary>
    public class Regulator: ISnapshot
    {
        public enum Modes { Sim, Client }

        public Modes Mode;

        //the time period between updates 
        private long updatePeriod;

       /// <summary>
        ///  this is where we keep the time.
       /// </summary>
        private TimeSpan elapsedTime = new TimeSpan(0);

        /// <summary>
        /// not sure why we need 2 timepoints...
        /// </summary>
        private TimeSpan? lastUpdate = null;

      
        RandomGenerator randomGenerator;

     /*   bool showDebugMessages = false;
        string regulatorBelongsTo;*/

        public Regulator()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       
        }

        /// <summary>
        /// uses random to stagger updates
        /// </summary>
        /// <param name="random"></param>
        /// <param name="numUpdatesPerSecond"></param>
        /// <param name="belongsTo"></param>
        /// <param name="mode"></param>
        public Regulator(RandomGenerator random, double numUpdatesPerSecond, string belongsTo, Modes mode = Modes.Sim)
        {
            this.Mode = mode;            
           
          /*  regulatorBelongsTo = belongsTo + "Regulator";

            if (The.Sim != null)
            {
                if (random == The.Sim.GameplayRandomGenerator)
                {
                    showDebugMessages = true;
                }
            }*/

            randomGenerator = random;
            if (numUpdatesPerSecond > 0)
            {
                updatePeriod = (long)(1000.0 / numUpdatesPerSecond);

                if (randomGenerator != null)
                {
                    if (updatePeriod < Int32.MaxValue)
                    {
                        elapsedTime = new TimeSpan(0, 0, 0, 0, (int)(updatePeriod - randomGenerator.Next(10, "Regulator", false))); // regulatorBelongsTo, showDebugMessages))); //random.Next((int)updatePeriod));
                    }
                    else
                    {
                        //elapsedTime = new TimeSpan(0, 0, 0, 0, Globals.Instance.Random.Next(10));
                        // ??? what to do?
                        elapsedTime = new TimeSpan(0, 0, 0, 0, (int)(updatePeriod - randomGenerator.Next(10, "Regulator", false))); // regulatorBelongsTo, showDebugMessages))); //random.Next((int)updatePeriod));
                    }
                }
            }
            else if (Common.IsEqual(0.0, numUpdatesPerSecond))
            {
                updatePeriod = 0;
            }
            else if (numUpdatesPerSecond < 0)
            {
                updatePeriod = -1;
            }

        }

        public bool IsReadyGetTimeElapsedInSeconds(out double secondsSinceLastReady)
        {
            double millisecondsSinceLastReady = 0d;
            bool isReady = IsReady(ref millisecondsSinceLastReady);

            secondsSinceLastReady = millisecondsSinceLastReady / 1000d;

            return isReady;
        }

        /// <summary>
        /// call this when not in game
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="millisecondsSinceLastReady"></param>
        /// <returns></returns>
        public bool IsReady(GameTime gameTime, ref double millisecondsSinceLastReady)
        {
            TimeSpan timeToUse = gameTime.TotalGameTime;

            return IsReady(timeToUse, ref millisecondsSinceLastReady);

        }

        /// <summary>
        /// Sim uses unpaused game time
        /// Client uses TotalGameTime
        /// </summary>
        /// <param name="millisecondsSinceLastReady"></param>
        /// <returns></returns>
        public bool IsReady(ref double millisecondsSinceLastReady)
        {
            TimeSpan timeToUse = GetTimeToUse();

            return IsReady(timeToUse, ref millisecondsSinceLastReady);

        }

       
        /// <summary>
        /// Sim uses unpaused game time
        /// Client uses TotalGameTime
        /// </summary>
        /// <param name="millisecondsSinceLastReady"></param>
        /// <returns></returns>
        private bool IsReady(TimeSpan timeToUse, ref double millisecondsSinceLastReady)
        {
            //If this is the first update of this regulator and the TotalUnpausedGameTime is large then we need to adjust the 
            //lastUpdate time else it will think that there has been a big step of time that does not exist. 
                        
            if (lastUpdate == null)
            {
                lastUpdate = timeToUse; 
            }

            // we may not call IsReady every tick. So we are using the total game time instead of the elapsed time since last tick - otherwise we may wait forever!


            TimeSpan timeSinceLastUpdate = timeToUse.Subtract(lastUpdate.Value);

            /*
#if !RELEASE
            if (timeSinceLastUpdate.Seconds > 100)
            {
                throw new Exception();
            }
#endif
            */

            lastUpdate = timeToUse; 

          
            elapsedTime = elapsedTime.Add(timeSinceLastUpdate);


            if (elapsedTime.TotalMilliseconds >= updatePeriod)
            {
                millisecondsSinceLastReady = elapsedTime.TotalMilliseconds;

                //the number of milliseconds the update period can vary per required
                //update-step. This is here to make sure any multiple clients of this class
                //have their updates spread evenly
                if (randomGenerator != null)
                {
                    elapsedTime = new TimeSpan(0, 0, 0, 0, randomGenerator.Next(10, "Regulator", false)); //  regulatorBelongsTo, showDebugMessages));
                }
                else
                {
                    elapsedTime = new TimeSpan(0, 0, 0, 0, 0);
                }

                return true;
            }

            return false;

        }

        private TimeSpan GetTimeToUse()
        {
            if (Mode == Modes.Sim)
            {
                return The.Sim.TotalUnPausedGameTime;
            }
            else //if (Mode == Modes.Client)
            {
                return The.Sim.GameTime.TotalGameTime;
            }
            
        }


        public bool IsReady()
        {
            double millisecondsSinceLastReady = 0;
            return IsReady(ref millisecondsSinceLastReady);

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Mode = sn.DoEnum(Mode);

            System.Diagnostics.Debug.Assert(Mode == Modes.Sim, "Don't snapshot client regulators, recreate them instead");

            this.lastUpdate = sn.DoTimeSpanNullable(lastUpdate);
            this.elapsedTime = sn.DoTimeSpan(elapsedTime);
            this.updatePeriod = sn.DoInt64(updatePeriod);
            

            sn.Ignore(randomGenerator);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            randomGenerator = The.Sim.GameplayRandomGenerator;

        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion
    }
}

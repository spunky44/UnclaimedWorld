using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{
    public class WeatherManager : ICyclable, ISnapshot
    {
       
        public Vector2 WindDirection;
        private Vector2 WindDirectionTarget; // The direction will gradually change until it achieves this direction

        public float windSpeed = 1.5f; //3f

        public float SunIntensity = 1f;
        private float globalTemperature;
        private float temperatureBeingAssigned;

        /// <summary>
        /// In Kelvin???
        /// </summary>
        public float Temperature;

        private float cloudCover = 0.2f; // 0.6f; //0.5f; set cloud cover here
        private float cloudCoverTarget;
        private float cloudCoverSpeedOfChange = 1f; // how fast do the cloud conditions change?

        public bool IsPaused { get; set; }

        public double StartedOnTimeInSeconds { get; set; }

        static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }
        public double ComputationTimeSpentInSeconds { get; set; }

        public double? UpdateInterval
        {
            get
            {
                return 2d;
            }
        }

        public float CloudCover
        {
            get { return cloudCover; }
            set
            {
                if (cloudCover != value)
                {
                    cloudCover = value;
                    UpdateWeatherDisplay();
                }
            }
        }

        /// <summary>
        /// meters per second???
        /// </summary>
        public float WindSpeed
        {
            get { return windSpeed; }
            set
            {
                if (windSpeed != value)
                {
                    windSpeed = value;
                    UpdateWeatherDisplay();
                }
            }

        }

        private float[] cloudEdges;
        private string[] cloudCoverTerms;

       // private float[] windEdges = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
        private float[] windEdges;
        private string[] windTerms;
        // http://en.wikipedia.org/wiki/Beaufort_scale

        /// <summary>
        /// Wraps around, so it always stays within 0 - 1
        /// </summary>
        public Vector2 CloudPosition = Vector2.Zero;
        private Vector2 cloudDrift;

        private enum Phase { Temperature, Moisture }
        private Phase phase = Phase.Temperature;
        private int cycleTileY = 0;

        private Regulator regulator;

        //private double deltaTime;


        public WeatherManager()
        {
            InitConstants();

          

            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();

                WindDirection = new Vector2(5f, -1f);//new Vector2(-5f, -1f);
                WindDirection.Normalize();

                WindDirectionTarget = WindDirection;

                cloudCoverTarget = cloudCover;

                cloudDrift = WindDirection * WindSpeed / 600f;
                cloudDrift.X *= -1f;

                CreateRegulators();
            }
        }

     
        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1 / UpdateInterval.Value, "WeatherManager");

        }

        private void UpdateWeatherDisplay()
        {

            int index = Common.GetStairStepIndex(cloudCover, cloudEdges);
            string clouds = cloudCoverTerms[index];
            index = Common.GetStairStepIndex(WindSpeed, windEdges);
            string wind = windTerms[index];
            The.InGameUI.SetWeatherNow(clouds, wind);
        }

       /* public static void RecreateInstance()
        {
            instance = new WeatherManager();
        }*/

    /*    public static WeatherManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new WeatherManager();
                    return instance;
                }
            }
        }
        */

        public void Update(GameTime gameTime)
        {
            if (The.Sim.DateAndTime.SunIsUp)
            {
                //SunIntensity = 1f;
                SunIntensity = Common.Clamp((1f - 0.5f * CloudCover) * (float) Math.Sin(The.Sim.DateAndTime.SunElevation), 0f, 1f);
            }
            else
            {
                SunIntensity = 0f;
            }

         //   SunIntensity = 0f; // TESTING HACK!!! comment in for testing Degradation of food items

            CloudPosition += cloudDrift * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // wrap 0-1:
            CloudPosition.X = CloudPosition.X % 1f;
            CloudPosition.Y = CloudPosition.Y % 1f;

            // replace this... temp goes from 5 to 20 deg. C.
            // integrate received sunlight, minus radiation to space???
            globalTemperature = 273f + 5f + 20f * SunIntensity; // globalTemperature + (float)gameTime.ElapsedGameTime.TotalSeconds * 



            //gradually lerp, ease-in to new direction target
            WindDirection = WindDirection * 0.99f + WindDirectionTarget * 0.01f;
            WindDirection.Normalize();

            if (!Common.IsEqual(cloudCover, cloudCoverSpeedOfChange))
            {
                CloudCover = CloudCover * (1f - cloudCoverSpeedOfChange) + cloudCoverTarget * cloudCoverSpeedOfChange;
            }


            //update drift rate
            cloudDrift = WindDirection * WindSpeed / 600f;
            cloudDrift.X *= -1f;//flip speed X




            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                    //TODO:  should be moved to CycleOnce
                    temperatureBeingAssigned = globalTemperature;

                    //deltaTime = milliSecondsSinceLastReady; // get the precise time since we were last here! 

                    phase = Phase.Temperature;
                    cycleTileY = 0;
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);

                    //changes in wind direction are seldom
                    if (The.Sim.GameplayRandomGenerator.Next(50,"Weather") == 2)
                    {
                        WindDirectionTarget = new Vector2((float)(The.Sim.GameplayRandomGenerator.NextDouble("Weather") - .5d),
                                                                (float)(The.Sim.GameplayRandomGenerator.NextDouble("Weather") - .5d));
                        WindDirectionTarget.Normalize();
                    }


                    // random changes in cloud cover
                    if (The.Sim.GameplayRandomGenerator.Next(80, "Weather") == 1)
                    {
                        // all cloud covers are equally likely:
                        cloudCoverTarget = (float)(The.Sim.GameplayRandomGenerator.NextDouble("Weather"));

                        // set the rate of change to the new situation:
                        cloudCoverSpeedOfChange = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(1f, 0.2f);
                        cloudCoverSpeedOfChange = 0.01f * cloudCoverSpeedOfChange;

                    }
                }
            }
        }

        #region ILookup

        private CyclableID id = CyclableID.Invalid;

        //=================== ILookup Methods =====================
        public CyclableID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion

        #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("Weather"));

        }

        public bool CycleOnce()
        {
            switch (phase)
            {
                case Phase.Temperature:
                                      
                    int mapWidth = The.Map.mapTileWidth;

                    TerrainTile[][] map = The.Map.TileMap;

                    // disabled for testing temperature override on tile levels
                  /*  for (int x = 0; x < mapWidth; x++)
                    {
                        // do some cellular automation stuff here?
                        // make woods and water areas cooler than deserts
                        map[x][cycleTileY].Temperature = temperatureBeingAssigned;

                    }
                                   
                    */

                    cycleTileY++;

                    if (cycleTileY == The.Map.mapTileHeight)
                    {
                        cycleTileY = 0;


                        // finished!!!
                        // reset:
                        // or go to next phase
                       // return true;
                        phase = Phase.Moisture;
                    }

                    return false;


                case Phase.Moisture:

                    // todo


                    return true;

                default:
                    return true;

            }


        }


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false;   // saves the current cyclable progress
            }
        }


        #endregion

        #region ISnapshot
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);

            // save the current cyclable progress:
            this.cycleTileY = sn.DoInt32(cycleTileY);
            this.phase = (Phase)sn.DoEnum(phase);
            

            this.cloudCover = sn.DoFloat(cloudCover);
            this.cloudCoverSpeedOfChange = sn.DoFloat(cloudCoverSpeedOfChange);
            this.cloudCoverTarget = sn.DoFloat(cloudCoverTarget);            
            this.cloudDrift = sn.DoVector2(cloudDrift);         
            this.CloudPosition = sn.DoVector2(CloudPosition);

            this.globalTemperature = sn.DoFloat(globalTemperature);
            this.SunIntensity = sn.DoFloat(SunIntensity);
            this.Temperature = sn.DoFloat(Temperature);

            this.WindDirection = sn.DoVector2(WindDirection);
            this.WindDirectionTarget = sn.DoVector2(WindDirectionTarget);
            this.windSpeed = sn.DoFloat(windSpeed);
            this.IsPaused = sn.DoBool(IsPaused);

            sn.Ignore(windTerms);
            sn.Ignore(cloudCoverTerms);
            sn.Ignore(windEdges);
            sn.Ignore(cloudEdges);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            return this;
        }

        private void InitConstants()
        {
            cloudEdges = new float[] { 0.1f, 0.4f, 0.96f, 1f };
            cloudCoverTerms = new string[] { "Clear", "Partly cloudy", "Cloudy", "Overcast" };

            // private float[] windEdges = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
            windEdges = new float[] { 0.5f, 3f, 5f, 11f, 14f, 17f, 20f, 24f, 28f, 100f };
            windTerms = new string[] { "Calm", "Light breeze", "Gentle breeze", "Fresh breeze", "Strong breeze", "High wind", "Gale", "Strong gale", "Storm", "Hurricane" };


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

        public void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

            CreateRegulators();
        }

        public bool IsSnapshotted{ get; set; }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.Control.Replays
{
    public class RandomGenerator: ISnapshot
    {
        public enum GeneratorType { Client, Sim }

        public GeneratorType Type;

        private static GeneratorType latestTypeInUse;
        private static string lastMessage;

        private int numberOfCalls = 0;

        public int? RandomSeed
        {
            get;
            private set;
        }

        private Random randomGenerator;       
        public Random Random
        {
            get
            {
                return randomGenerator;
            }
        }

        public RandomGenerator()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public RandomGenerator(int randomSeed, GeneratorType type)
        {
            RandomSeed = randomSeed;
            randomGenerator = new Random(RandomSeed.Value);

            this.Type = type;
        }

        public RandomGenerator(GeneratorType type)
        {
            randomGenerator = new Random();

            this.Type = type;
        }

        public int Next(string getterMessage,bool saveMessage = true)
        {

            if (saveMessage)
            {
                The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
            }

            lastMessage = getterMessage;
            latestTypeInUse = Type;
            numberOfCalls++;
            
            return randomGenerator.Next();
            //return 5; 
        }

        public int Next(int maximumValue, string getterMessage, bool saveMessage = true)
        {            

            if (saveMessage)
            {
                The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
            }

            lastMessage = getterMessage;
            latestTypeInUse = Type;
            numberOfCalls++;
           
            return randomGenerator.Next(maximumValue);
        }

        /// <summary>
        /// max is exclusive!
        /// </summary>
        /// <param name="minimumValue"></param>
        /// <param name="maximumValue"></param>
        /// <param name="getterMessage"></param>
        /// <param name="saveMessage"></param>
        /// <returns></returns>
        public int Next(int minimumValue, int maximumValue, string getterMessage, bool saveMessage = true)
        {

            if (saveMessage)
            {
                The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
            }
            
            lastMessage = getterMessage;
            latestTypeInUse = Type;
            numberOfCalls++;
            
            return randomGenerator.Next(minimumValue, maximumValue);
        }

        /// <summary>
        ///  Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="getterMessage"></param>
        /// <param name="saveMessage"></param>
        public void NextBytes(byte[] values, string getterMessage, bool saveMessage = true)
        {

            if (saveMessage && The.Sim != null)
            {
                The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
            }

            lastMessage = getterMessage;
            latestTypeInUse = Type;
            numberOfCalls++;

            randomGenerator.NextBytes(values);
        }


        public double NextDouble(string getterMessage, bool saveMessage = true)
        {

            if (saveMessage)
            {
                The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
            }
            lastMessage = getterMessage;
            latestTypeInUse = Type;
           
            double randomValue = randomGenerator.NextDouble();
            numberOfCalls++;
            
            return randomValue;
        }

        public float RandomBetween(float min, float max)
        {
            bool showMessage;
            if (Type == GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return min + (float)this.NextDouble("Common - RandomBetween", showMessage) * (max - min);
        }

        public int RandomBetween(int min, int max)
        {
            bool showMessage;
            if (Type == GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return this.Next(min, max, "Common - RandomBetween", showMessage);
        }

        public int RandomSign()
        {
            bool showMessage;
            if (Type == GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return (2 * this.Next(0, 1, "Common - RandomSign", showMessage) - 1);
        }


        /// <summary>
        /// standard deviation is the deviation from the mean to each side - a max. spread divided by 3 gives the deviation value
        /// e.g. we want a max 'spread' of 0.10 to each side - divide by 3 and pass 0.03.
        /// 
        /// One standard deviation away from the mean in either direction on the horizontal axis accounts for somewhere around 68 percent of the people in this group. 
        /// Two standard deviations away from the mean account for roughly 95 percent of the people. 
        /// And three standard deviations account for about 99 percent of the people.
        /// 
        /// For example, the average height for adult men in the United States is about 178 cm, with a standard deviation 
        /// of around 8 cm. This means that most men (about 68 percent, assuming a normal distribution) 
        /// have a height within 8 cm of the mean (170–185 cm), while almost all men (about 95%) have a height 
        /// within 15 cm of the mean (163–193 cm). If the standard deviation were zero, then all men would be 
        /// exactly 178 cm high. If the standard deviation were 51 cm, then men would have much more variable heights, 
        /// with a typical range of about 127 to 229 cm.
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public double RandomNormalDistribution(double mean, double stdDev)
        {
            bool showMessage;
            if (Type == GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            double r1 = NextDouble("Common - RandomNormalDistribution", showMessage);
            double r2 = NextDouble("Common - RandomNormalDistribution", showMessage);

            return mean + (stdDev * (Math.Sqrt(-2 * Math.Log(r1)) * Math.Cos(6.28 * r2)));
        }


        #region ISnapshot

        public bool IsSnapshotted { get; set; }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.RandomSeed = sn.DoInt32Nullable(RandomSeed);
            this.Type = sn.DoEnum(Type);

            if (sn.mode == Snapshotter.Mode.Load)
            {
                // create the generator now, not in LoadPost. it is needed for ctoring the regulators in the other loaded objects:
                if (RandomSeed.HasValue)
                {
                    // recreate the generator from the seed:
                    randomGenerator = new Random(RandomSeed.Value);
                }
                else
                {
                    randomGenerator = new Random();
                }
            }


            sn.Ignore(randomGenerator);
            sn.Ignore(lastMessage);
            sn.Ignore(latestTypeInUse);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    [DebuggerDisplay("{KeyName}")]
    public class StorageCondition: IGameData
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Description;


        public bool RequiresPower;

        /// <summary>
        /// maintains a constant moisture level
        /// </summary>
        public float? FixedMoisture;

        /// <summary>
        /// maintains a constant temperature
        /// </summary>
        public float? FixedTemperature;

        /// <summary>
        /// maintains a fixed light level: 0 is darkness, 1 is maximum
        /// </summary>
        public float? FixedLightLevel;
        
        /// <summary>
        /// if true, follows the ambient temperature but skewed towards 21C
        /// </summary>
        public bool IsolatedTemperature;


        public float GetTemperature(bool isPowered, float ambientTemperature)
        {
            if (RequiresPower && !isPowered)
            {
                return ambientTemperature;
            }

            if (IsolatedTemperature == true)
            {
                return ComputeIsolatedTemperature(ambientTemperature);
            }

            return FixedTemperature ?? ambientTemperature;
        }


        /// <summary>
        /// skews the temperature towards 21C... not very realistic...
        /// </summary>
        /// <param name="ambientTemperature"></param>
        /// <returns></returns>
        public static float ComputeIsolatedTemperature(float ambientTemperature)
        {
            // f(x) = x - 0.4 (x - 21)
            // 0C -> 8C
            // 30C -> 26C
            float roomTemperature = 293f;

            return ambientTemperature - 0.4f * (ambientTemperature - roomTemperature);
        }

        public void Initialize()
        { }

        public void PreInitValidate(ref List<string> listOfErrors) 
        {
            if (IsolatedTemperature == true && FixedTemperature.HasValue)
            {
                EntityType.CreateValidationError(ref listOfErrors, "Cannot specify both IsolatedTemperature and FixedTemperature.");
            }
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}

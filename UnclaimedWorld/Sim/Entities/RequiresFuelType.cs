using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// TODO: this does not contain entities. so handle this with substances.
    /// </summary>
    public class RequiresFuelType
    {
        public float MaxFuel = 1f;

        public bool HasFlames;

        public bool ProducesSmoke;

        public float BurnRatePerDay;

        
        public string FuelTypeTag;        
        public string FuelTypeKeyName;



        [XmlIgnore]
        public List<EntityType> FuelEntityTypes;
     //   public EntityType[] FuelEntityTypes;

        [XmlIgnore]
        public string FuelClientString
        {
            get;
            private set;
        }


        public float GetNeededFuel(float durationInDays)
        {
            float neededFuel = (float)(BurnRatePerDay * durationInDays);
            return neededFuel;
        }


        public void PostLoadContentInitialize()
        {

            if (!string.IsNullOrEmpty(FuelTypeTag))
            {
                FuelEntityTypes = GameData.Instance.FuelByTag[FuelTypeTag];
            }

            if (!string.IsNullOrEmpty(FuelTypeKeyName))
            {
                EntityType fuelType = GameData.Instance.AllEntityTypes[FuelTypeKeyName];

                if (FuelEntityTypes == null)
                {
                    FuelEntityTypes = new List<EntityType>();
                }

                if (!FuelEntityTypes.Contains(fuelType))
                {
                    FuelEntityTypes.Add(fuelType);
                }
            }

            if (FuelEntityTypes != null)
            {
                string concatenatedString = "";
               
                string delim = "";
                // make a comma separated list of the first few types of fuel that can be used, for player feedback:
                int i = 0;
                while(concatenatedString.Length < 20 && i < FuelEntityTypes.Count)
                {
                    concatenatedString += delim;
                    concatenatedString += FuelEntityTypes[i].Name;

                    delim = ", ";
                    i++;
                }

                if (i < FuelEntityTypes.Count)
                {
                    concatenatedString += "...";
                }

                FuelClientString = concatenatedString;
            }

        }
    }
}

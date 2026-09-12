using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Substances
{
    public class SubstancesType
    {
        /// <summary>
        /// These will be normalized so the sum is always less than or equal to 1
        /// </summary>
        public SerializableDictionary<string, float> SubstanceFractions;

        //public SubstanceType[] Substances;

        [XmlIgnore]
        public Dictionary<SubstanceType, float> FinalSubstanceFractions;

        public void Initialize()
        {
            FinalSubstanceFractions = new Dictionary<SubstanceType, float>();
            float total = SubstanceFractions.Sum(s => s.Value);

            float normalizeFactor = 1f;
            // normalize so the total stays under 1
            if (total > 1f)
            {
                normalizeFactor = 1f / total;             
            }

            foreach (var item in SubstanceFractions)
            {
                float value = normalizeFactor * item.Value;
                FinalSubstanceFractions.Add(GameData.Instance.AllSubstanceTypes[item.Key], value);

            }

        }
    }
}

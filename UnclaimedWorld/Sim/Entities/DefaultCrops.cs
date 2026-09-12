using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// specifies a random number of crops // resources (crops, tiles).
    /// this has nothing to do with simulation and should probably be deleted after we expand the ecology.
    /// </summary>
    public class DefaultCrops
    {
        public string KeyName;

        public int? MinItemsForFullGrownPlant;
        public int? MaxItemsForFullGrownPlant;


        [XmlIgnore]
        public float? AbsoluteMeanItems;

        [XmlIgnore]
        public float? AbsoluteStandardDeviationItems;


        public void Initialize() //float bulkOfFullGrownTree)
        {
            if (MinItemsForFullGrownPlant.HasValue && MaxItemsForFullGrownPlant.HasValue)
            {
                // convert to per-bulk numbers:
              //  float minAmountPerPlantBulk = MinItemsForFullGrownPlant.Value / bulkOfFullGrownTree;
             //   float maxAmountPerPlantBulk = MaxItemsForFullGrownPlant.Value / bulkOfFullGrownTree;

                Common.GetNormalDistributionFromMinMaxValues(MinItemsForFullGrownPlant.Value, MaxItemsForFullGrownPlant.Value, out AbsoluteMeanItems, out AbsoluteStandardDeviationItems);
            }
        }
    }
}

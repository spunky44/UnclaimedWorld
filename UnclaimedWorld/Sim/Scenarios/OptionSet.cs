using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Scenarios
{
   
    /// <summary>
    /// a set of options in a logical group, like "Starting location", "Duration", "Equipment"
    /// </summary>
    public class OptionSet
    {
        public string KeyName;

        /// <summary>
        /// gets displayed in the left hand side on a colored banner
        /// </summary>
        public string Name;

        public string Description;

        /// <summary>
        /// a color will be shown accordingly...
        /// </summary>
        public int DisplayGroup;
      

        public Option[] Options;

        [XmlIgnore]
        public List<IGrouping<string, Option>> OptionsGroupedByDifficulty;

      

        public void Initialize()
        {
            var optionsGroupedByDifficulty = Options.GroupBy(o => o.Difficulty.KeyName);

            OptionsGroupedByDifficulty = optionsGroupedByDifficulty.ToList();


            foreach (var item in Options)
            {
                item.Initialize();
            }

        }


        


    }
}

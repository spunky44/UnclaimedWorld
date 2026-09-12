using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Scenarios
{
   
    public class Difficulty
    {
        public string KeyName;

        public string Name;

        public string Description;
        
        public bool IsDefault = false;

        /// <summary>
        /// each difficulty setting must specify the options that will be used (or selected from randomly).
        /// if null, the radio button will not appear
        /// 
        /// Key: Same as KeyName in the OptionSet
        /// Value: A list of Option keynames
        /// </summary>
        public SerializableDictionary<string, string[]> OptionsToUse;



        public void PostInitValidate(ScenarioData parent, List<string> listOfErrors)
        {
            if (OptionsToUse != null)
            {
                foreach (var item in OptionsToUse)
                {
                    OptionSet optionSet = parent.OptionSets.FirstOrDefault(o => o.KeyName == item.Key);
                    if (optionSet == null)
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "OptionsToUse: The option set key: " + item.Key + " was not found in OptionSets.");
                 
                    }
                    else 
                    {
                        foreach (var key in item.Value)
                        {
                            Option option = optionSet.Options.FirstOrDefault(o => o.KeyName == key);

                            if (option == null)
                            {
                                EntityType.CreateValidationError(ref listOfErrors, "The option key: " + key + " was not found in the Options list for OptionSet: " + optionSet.KeyName);
                            }               

                        }
                    }
                }
            }

        }
    }
}

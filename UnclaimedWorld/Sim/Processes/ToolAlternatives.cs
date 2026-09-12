using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;

namespace UWGame.SimSide.Processes
{
    public class ToolAlternatives
    {
        /// <summary>
        /// we only need one of these tools!
        /// each Tool object is a collection of tool items with the same production parameters!
        /// </summary>
        public Tool[] Tools;

        /// <summary>
        /// helper list for tool tips - entity type, productivity factor (also degrade info?)
        /// </summary>
        [XmlIgnore]
        public List<Tuple<EntityType, float>> ToolsAndProductivity;


        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            
            foreach (var item in Tools)
            {
                item.PreDataCompleteValidate(ref listOfErrors);
            }
            
        }

        public void PostDataCompleteInitialize()
        {
            ToolsAndProductivity = new List<Tuple<EntityType, float>>();

            foreach (var item in Tools)
            {
                item.PostDataCompleteInitialize();

                ToolsAndProductivity.AddRange(item.ToolEntityTypes.Select(t => new Tuple<EntityType, float>(t, item.ProductivityFactor.Value)));
            }

            
        }


        public void PreInitValidate(ref List<string> errors)
        {
            foreach (var item in Tools)
            {
                item.PreInitValidate(ref errors);
            }
                        
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            foreach (var item in Tools)
            {
                item.PostInitValidate(ref listOfErrors);
            }

            if (ToolsAndProductivity.Count > 1)
            {
                List<EntityType> duplicates = new List<EntityType>();


                // check for duplicates (overlapping tags?)
                for (int i = 0; i < ToolsAndProductivity.Count; i++)
			    {
                    var tool1 = ToolsAndProductivity[i];

                    for (int j = 0; j < ToolsAndProductivity.Count; j++)
			        {
                        if (i != j)
                        {
                            var tool2 = ToolsAndProductivity[j];

                            if (tool1.Item1 == tool2.Item1
                                && !duplicates.Contains(tool1.Item1))
                            {
                                duplicates.Add(tool1.Item1);
                                EntityType.CreateValidationError(ref listOfErrors, tool1.Item1.KeyName + " is duplicated in the tool options set. Overlapping tags?");
                            }
                        }
                    }			 
                }

                
            }
        }

        public bool NeedsImmobileTool()
        {
            foreach (var tool in Tools)
            {
                foreach (var toolEntityType in tool.ToolEntityTypes)
                {
                    if (!ToolType.IsImmovable(toolEntityType))
                    {
                        // immobileTools.Add(tool.EntityType);
                        //toolEntity = tool.EntityType;

                        return false;
                    }
                }
                
            }

            return true;
        }
    }
}

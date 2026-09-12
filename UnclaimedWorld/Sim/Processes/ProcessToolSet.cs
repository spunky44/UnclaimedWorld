using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes
{
    [DebuggerDisplay("{KeyName}")]
    public class ProcessToolSet: IGameData
    {
        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        /// <summary>
        /// we need one tool from each of the ToolAlternatives!
        /// </summary>
        public ToolAlternatives[] Tools;

        public string Comments;

        // derived collections:
              

        [XmlIgnore]
        public List<ToolTypeCombination> ToolTypeCombinations;

        public void Initialize()
        {
            /* moved to PostLoadComplete
            if (Tools != null)
            {                
                foreach (var item in Tools)
                {
                    item.Initialize();
                }
           

                // we avoid deferred execution of LINQ statement by converting IEnumerable to List             
                IEnumerable<IEnumerable<ToolData>> allToolsCartesian = GetAllToolCombos(); 


                ToolTypeCombinations = new List<ToolTypeCombination>();

                ToolTypeCombination combo;
                foreach (var listOfTools in allToolsCartesian)
                {
                    
                    combo = new ToolTypeCombination()
                    {
                        Tools = listOfTools.Select(t => new Tuple<EntityType, float>(t.ToolEntityType, t.DegradePerSecond)).ToList(),
                        Productivity = listOfTools.Average(t => t.Productivity) // productivity is computed as average of all tools in set
                    }; 
                  
                    ToolTypeCombinations.Add(combo);
                }
              
            }*/

        }


        private class ToolData
        {
            public EntityType ToolEntityType;
            public float Productivity;
            public float DegradePerSecond;
        }

        /// <summary>
        /// compute the cartesian product of the valid tool combinations in a form that the evaluator can consume
        /// </summary>
        /// <param name="job"></param>
        /// <param name="itemType"></param>
       // private IEnumerable<IEnumerable<Tuple<EntityType, float>>> GetAllToolCombosForProcessJob(/*ProcessTypeToolProfile toolProfile*/ ) // ProcessJob job) 
        private IEnumerable<IEnumerable<ToolData>> GetAllToolCombos() 
        {               
            List<List<ToolData>> allTools = new List<List<ToolData>>();           
          
            List<ToolData> toolsInThisAlternative;

            if (Tools.Length > 0)
            {
                foreach (var item in Tools)  
                {
                    toolsInThisAlternative = new List<ToolData>(); 
                    foreach (var t in item.Tools)
                    {   // include the productivity factor with the tool entity type:
                        toolsInThisAlternative.AddRange(t.ToolEntityTypes.Select(te => new ToolData(){ ToolEntityType = te, Productivity = t.ProductivityFactor.Value, DegradePerSecond = t.DegradePerSecondOfUse.Value }));
                    }

                    allTools.Add(toolsInThisAlternative);
                }
            }
          
            IEnumerable<IEnumerable<ToolData>> allToolsCartesian = Common.CartesianProduct(allTools);                   
            
            return allToolsCartesian;
        }

        public void PreInitValidate(ref List<string> listOfErrors) 
        {
            if (Tools != null)
            {
                foreach (var item in Tools)
                {
                    item.PreInitValidate(ref listOfErrors);
                }
            }

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {
           
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) 
        {
            if (Tools != null)
            {
                foreach (var item in Tools)
                {
                    item.PreDataCompleteValidate(ref listOfErrors);
                }
            }
        }
       
        public void PostDataCompleteInitialize()
        {
            if (Tools != null)
            {
                foreach (var item in Tools)
                {
                    item.PostDataCompleteInitialize();
                }


                // we avoid deferred execution of LINQ statement by converting IEnumerable to List             
                IEnumerable<IEnumerable<ToolData>> allToolsCartesian = GetAllToolCombos();


                ToolTypeCombinations = new List<ToolTypeCombination>();

                ToolTypeCombination combo;
                foreach (var listOfTools in allToolsCartesian)
                {

                    combo = new ToolTypeCombination()
                    {
                        Tools = listOfTools.Select(t => new Tuple<EntityType, float>(t.ToolEntityType, t.DegradePerSecond)).ToList(),
                        Productivity = listOfTools.Average(t => t.Productivity) // productivity is computed as average of all tools in set
                    };

                    ToolTypeCombinations.Add(combo);
                }

            }
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (Tools != null)
            {
                foreach (var item in Tools)
                {
                    item.PostDataCompleteValidate(ref listOfErrors);
                }

                bool needsImmobileTool = false;
                foreach (var item in Tools)
                {
                    //item.PreInitValidate(ref listOfErrors);

                    if (item.NeedsImmobileTool())
                    {
                        if (needsImmobileTool)
                        {
                            // this could(?) be removed when workshops are implemented
                            // is it best to require many processes instead of one?
                            EntityType.CreateValidationError(ref listOfErrors, "Only one immobile tool may be required");
                        }
                        else
                        {
                            needsImmobileTool = true;
                        }
                    }
                }

                foreach (var toolTypeCombo in ToolTypeCombinations)
                {
                    if (!CanCarryAllTools(toolTypeCombo.Tools))
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "All mobile tools must be able to be carried at once (total bulk <= 1)" + System.String.Join(",", toolTypeCombo.Tools.Select(p => p.Item1.KeyName.ToString()) /*.ToArray()*/));
                    }
                }

            }

        }


        private bool CanCarryAllTools(List<Tuple<EntityType, float>> tools) //List<EntityType> tools) 
        {
            float totalBulk = 0f;
            
            foreach (var tool in tools)
            {
                if (!ToolType.IsImmovable(tool.Item1))
                {
                    totalBulk += tool.Item1.ItemType.MaximumBulk.Value;
                }
            }

            return totalBulk <= 1f; // entity.ItemStorage.HasCapacityForItemWhenEmpty(totalBulk);
        }

        public List<EntityType> GetImmobileToolChoices() //out EntityType toolEntity)
        {
            // toolEntity = null;
            List<EntityType> immobileTools = new List<EntityType>();

            if (Tools != null)
            {

                foreach (var toolAlternatives in Tools)
                {
                    foreach (var tool in toolAlternatives.Tools)
                    {
                        foreach (var toolEntityType in tool.ToolEntityTypes)
                        {
                            if (ToolType.IsImmovable(toolEntityType))
                            {
                                immobileTools.Add(toolEntityType);
                                break;

                                //toolEntity = tool.EntityType;

                                //return true;
                            }
                        }

                        /*
                        if (tool.EntityType != null)
                        {
                            if (tool.EntityType.ToolType.IsImmobile())
                            {
                                immobileTools.Add(tool.EntityType);
                                break;

                                //toolEntity = tool.EntityType;

                                //return true;
                            }
                        }
                        else
                        {
                            List<EntityType> listOfToolTypes;
                            if (GameData.Instance.ToolsByTag.TryGetValue(tool.Tag, out listOfToolTypes))
                            {
                                foreach (var item in listOfToolTypes)
                                {
                                    if (item.ToolType.IsImmobile())
                                    {
                                        immobileTools.Add(item);
                                        break;

                                        //toolEntity = tool.EntityType;

                                        //return true;
                                    }
                                }
                            }
                        }*/
                    }
                }
            }

            return immobileTools;
        }
    }


   

    
}

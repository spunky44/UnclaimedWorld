using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.RepairTypes;

namespace UWGame.SimSide.AllGameData
{
    public class RepairProfileLoader
    {
        public static List<RepairProfile> Init()
        {
            List<RepairProfile> list = new List<RepairProfile>();

            list.Add(new RepairProfile()
                {
                    Comments = "not used yet",
                     KeyName = "improvisedEquipment",
                     Integrity = new RepairType()
                     {
                          Repair = RepairProcess.Production
                     },
                     DefaultPartsReplacement = new RepairType()
                     {
                          Repair = RepairProcess.Production
                     },
                     DefaultPartsCondition = new RepairType()
                     {
                         Repair = RepairProcess.Production
                     }
                });

            list.Add(new RepairProfile()
            {
                Comments = "Survival tier building parts can be reconditioned without tools",
                KeyName = "buildingRepair",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                PartsCondition = new SerializableDictionary<string, RepairType>()
                {
                    { "item:spoakBranchesTrimmed", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpokBranchesTrimmedPart"} }, // example of using specific process for one part type
                    { "item:spoakLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpoakLeaves"} }, // example of using specific process for one part type      
                    { "item:daysheenLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionDaysheenLeaves"} },
                    { "item:sticks", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSticks"} }                   
                   // { "item:solidMudBrick", new RepairType() { Repair = RepairProcess.Production, ProductionTimeFactor = 0.2f } } // example of using the production process and a time factor
                }
            });

            list.Add(new RepairProfile()
            {
                Comments = "For mining pits and the like (NOT peat bank). Used for entities that have no production process. Uses digging tools to repair integrity. Survival tier building parts can be reconditioned without tools",
                KeyName = "diggingRepairCustomProcess",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveIntegrityWithDigging" //"establishClayPit" 
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.None
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveCondition" // "establishClayPit" 
                },
                PartsCondition = new SerializableDictionary<string, RepairType>()
                {
                    { "item:spoakBranchesTrimmed", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpokBranchesTrimmedPart"} }, // example of using specific process for one part type
                    { "item:spoakLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpoakLeaves"} }, // example of using specific process for one part type      
                    { "item:daysheenLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionDaysheenLeaves"} },
                    { "item:sticks", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSticks"} }               
                }
            });

            list.Add(new RepairProfile()
            {
                Comments = "For peat bank. Needs only plowing tools (like farm plot) Used for entities that have no production process. Uses digging tools to repair integrity. Survival tier building parts can be reconditioned without tools",
                KeyName = "plowingRepairCustomProcess",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveIntegrityWithPlowing" //"" 
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.None
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveCondition" 
                },
                PartsCondition = new SerializableDictionary<string, RepairType>()
                {
                    { "item:spoakBranchesTrimmed", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpokBranchesTrimmedPart"} }, // example of using specific process for one part type
                    { "item:spoakLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpoakLeaves"} }, // example of using specific process for one part type      
                    { "item:daysheenLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionDaysheenLeaves"} },
                    { "item:sticks", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSticks"} }               
                }
            });
          


            list.Add(new RepairProfile()
            {
                Comments = "Used for the entities that have no production process. Survival tier building parts can be reconditioned without tools",
                KeyName = "buildingRepairCustomProcess",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveIntegrity"
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.None
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Custom,
                    ProcessType = "repairPrimitiveCondition" 
                },
                PartsCondition = new SerializableDictionary<string, RepairType>()
                {
                    { "item:spoakBranchesTrimmed", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpokBranchesTrimmedPart"} }, // example of using specific process for one part type
                    { "item:spoakLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpoakLeaves"} }, // example of using specific process for one part type      
                    { "item:daysheenLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionDaysheenLeaves"} },
                    { "item:sticks", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSticks"} }                   
                   // { "item:solidMudBrick", new RepairType() { Repair = RepairProcess.Production, ProductionTimeFactor = 0.2f } } // example of using the production process and a time factor
                }
            });

         


            list.Add(new RepairProfile()
            {
                Comments = "for repairing poup tents",
                KeyName = "tentRepair",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Production, 
                    ProductionTimeFactor = 1f
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Production,
                    ProductionTimeFactor = 1.5f // takes longer to repair than to put up
                }
            });

          /*  list.Add(new RepairProfile()
            {
                Comments = "Survival tier building parts can be reconditioned without tools",
                KeyName = "buildingRepairExample",
                Integrity = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                DefaultPartsReplacement = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                DefaultPartsCondition = new RepairType()
                {
                    Repair = RepairProcess.Production
                },
                PartsCondition = new SerializableDictionary<string, RepairType>()
                {
                    { "item:spoakBranchesTrimmed", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpokBranchesTrimmedPart"} }, // example of using specific process for one part type
                    { "item:spoakLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionSpoakLeaves"} }, // example of using specific process for one part type      
                    { "item:daysheenLeaves", new RepairType() { Repair = RepairProcess.Custom, ProcessType = "reconditionDaysheenLeaves"} },
                   // { "item:solidMudBrick", new RepairType() { Repair = RepairProcess.Production, ProductionTimeFactor = 0.2f } } // example of using the production process and a time factor
                }
            });*/

            return list;


        }



    }
}

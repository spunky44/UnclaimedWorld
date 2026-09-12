using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.RepairTypes
{
    /// <summary>
    /// an instance is kept for every nonlivingtype. The process types can be generated from the entity type so they are unique.
    /// </summary>
    public class EntityRepairProfile
    {
        public ProcessType Integrity;

        /// <summary>
        /// The process for repairing a part of this type.
        /// used for RemovePart, AddPart and ReplacePart
        /// 
        /// how many levels down?? 1???
        /// </summary>
        public Dictionary<EntityType, ProcessType> PartsReplacement;

        /// <summary>
        /// reconditioning a part in place, not very realistic
        /// </summary>
        public Dictionary<EntityType, ProcessType> PartsCondition;

        /// <summary>
        /// for reconditioning the entity type itself, not its parts!
        /// </summary>
        public ProcessType Condition;

        /// <summary>
        /// retrieves or creates entity type specific repair process types from more general ones
        /// </summary>
        /// <param name="parent"></param>
        public void GenerateProcesses(EntityType parent, List<string> errors)
        {
            RepairProfile profile = parent.NonLivingType.RepairProfile;

          
            // either creates a process or gets a reference. returns null if repair is not possible
            if (profile.Integrity != null)
            {
                Integrity = profile.Integrity.GetProcessType(RepairAction.Integrity, parent, null);

                if (profile.Integrity.Repair == RepairProcess.Production && Integrity == null)
                {
                     EntityType.CreateValidationError(ref errors, "Integrity repair is set to use the production process, but no production process exists.");
                }
            }

            if (profile.Condition != null)
            {
                Condition = profile.Condition.GetProcessType(RepairAction.Condition, parent, null);

                if (profile.Condition.Repair == RepairProcess.Production && Condition == null)
                {
                    EntityType.CreateValidationError(ref errors, "Condition repair is set to use the production process, but no production process exists.");
                }
            }

            if (parent.Parts != null)
            {
                PartsReplacement = new Dictionary<EntityType, ProcessType>();              
                PartsCondition = new Dictionary<EntityType, ProcessType>();

                foreach (var item in parent.Parts)
                {
                    //Replace: see if the parts type is specified in the profile, else use the default.
                    RepairType repairType;
                    if (profile.PartsReplacementFinal != null
                        && profile.PartsReplacementFinal.TryGetValue(item.Key, out repairType))
                    {
                        PartsCondition.Add(item.Key, repairType.GetProcessType(RepairAction.PartsCondition, parent, item.Key));
                    }
                    else if (profile.DefaultPartsReplacement != null)
                    {
                        PartsReplacement.Add(item.Key, profile.DefaultPartsReplacement.GetProcessType(RepairAction.ReplacePart, parent, item.Key));
                    }


                    //Recondition: see if the parts type is specified in the profile, else use the default.                  
                    if (profile.PartsConditionFinal != null
                        && profile.PartsConditionFinal.TryGetValue(item.Key, out repairType))
                    {
                        PartsCondition.Add(item.Key, repairType.GetProcessType(RepairAction.PartsCondition, parent, item.Key));
                    }
                    else if (profile.DefaultPartsCondition != null)
                    {
                        PartsCondition.Add(item.Key, profile.DefaultPartsCondition.GetProcessType(RepairAction.PartsCondition, parent, item.Key));
                    }
                }

            }  
        }


    }
}

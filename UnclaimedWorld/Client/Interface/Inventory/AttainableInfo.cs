using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Tiers;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Inventory
{
    public enum IconKeys { NoProcess, NoResource, NoSkill, NoInput, NoTool, NoSpecialEntity, Salvage, MultipleProcesses, NoPolicy, Upgrade, Pseudo }

    public class AttainableInfo
    {
       
        /// <summary>
        /// if less than 0 then not attainable...
        /// </summary>
        public int DistanceToRoot;

        /// <summary>
        /// both Owned items and Producable items are considered attainable in the algo!
        /// </summary>
        public bool IsOwned;

        /// <summary>
        /// both Owned items and Producable items are considered attainable in the algo!
        /// </summary>
        public bool IsProducable;

        /// <summary>
        /// For feedback if not attainable.
        /// only store the first blocked/missing item in the chain.
        /// </summary>
        public List<EntityType> UnavailableInputs;
        public List<EntityType> UnavailableTools;       
        public ResourceType UnavailableResource;
        public TierOrAreaType UnavailablePolicy;
        public EntityType UnavailableSpecialSite;
        public SkillType UnavailableSkill;
      //  public bool NoProcess = false;
     //   public int ProcessCount = 0;

      //  public bool IncludesSalvageProcesses;

        public AttainableInfo(int distanceToRoot)
        {
            this.DistanceToRoot = distanceToRoot;
           // this.IncludesSalvageProcesses = The.InGameUI.InventorySettings.IncludeSalvageProcesses;
        }

        public bool IsAttainable
        {
            get
            {
                return DistanceToRoot > -1;
            }
        }

        /*
        public string GetText()
        {          
            if (IsAttainable)
            {
                return "ATTAINABLE";
            }
            else
            {
                return "NOT ATTAINABLE";
            }
        }*/

        const string tooltipHeader = "Missing/unattainable: \n \n";

        
       
/*
        public string GetTooltip()
        {
            string text = "";
            string delim = "";

            if (NoProcess == true)
            {
                if (IncludesSalvageProcesses)
                {
                    text = "We have no way of producing this.";
                }
                else
                {
                    text = "We have no way of producing this (HOWEVER: There may/may not be salvage options available!)"; //mp: i added (There may/may not be salvage options available) because it depends on the Salvage filter button.
                }
            }
            else if (UnavailableInputs != null || UnavailableResource != null || UnavailableSkill != null || UnavailableTools != null)
            {
                
                if (UnavailableSkill == null)
                {
                    text = "Currently, there is no way of getting the materials needed to produce this. Exploration may help. \n \n";
                }
                else
                {
                    text = "No one has the SKILLS needed to produce this. \n \n";              
                }

                text += tooltipHeader;
                
                if (UnavailableSkill != null)
                {
                    text = "Skill: " + UnavailableSkill.Name;
                    delim = " \n \n";
                }

                if (UnavailableResource != null)
                {
                    text += delim + "Resource: " + UnavailableResource.Name;
                    delim = " \n \n";
                }

                if (UnavailableInputs != null)
                {
                    text += delim + "Inputs: \n";

                    delim = "";
                    foreach (var item in UnavailableInputs)
                    {
                        text += delim + item.PluralName;
                        delim = " \n";
                    }

                    delim = " \n \n";

                }

                if (UnavailableTools != null)
                {
                    int toolCounter = 0;
                    text += delim + "One of these tools: \n";
                    string toolDelim = "";
                    foreach (var item in UnavailableTools)
                    {
                        if (toolCounter == 3)
                        {
                            text += "...";
                            break;
                        }

                        text += toolDelim + item.Name;
                        toolDelim = ", ";

                        toolCounter++;                        
                    }
                }
            }
            else
            {
                text = "If we can source the required materials in sufficient amounts, we may be able to produce this. Examine the tooltip to determine what we need.";
            }

            return text;
        }*/
    }
}

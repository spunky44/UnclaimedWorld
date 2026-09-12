using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Client.Interface;

using InputEventSystem;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// a type of datatooltip that displays info about a process type
    /// For salvage processes, we show the outputs from the process
    /// </summary>
    public class ProcessTypeDataSheet : DataSheet
    {

        ProcessType processType;

        const float outputsIndex = 200f;

        /// <summary>
        /// not to be confused with "USED IN" on the EntityTypeTooltip
        /// </summary>
        Grid grdOutputs;
        UIComponent outputsHeader;
        Label lblOutputs;


              
        /// <summary>
        /// creates a panel with the relevant fields for this entity type
        /// </summary>
        /// <param name="entityType"></param>
        public ProcessTypeDataSheet() 
        {
          
        }


        public void Fill(ProcessType processType)
        {
            this.processType = processType;
            
            Fill();
        }


        protected override string GetDescription()
        {
            return processType.Description;
        }

        protected override void CreateGeneralPanelContents()
        {
        }

        protected override void CreateProductionPanelContents()
        {
             CreateGridAndHeader(grdProductionOuter, out outputsHeader, "OUTPUT:", "Creates these output types and amounts", 
                outputsIndex, out grdOutputs, out lblOutputs);
         

        }

        protected override void Retire()
        {
            The.InGameUI.poolOfProcessTypeTooltips.Retire(this);        
        }

        protected override void PopulateCollapsedFieldsContents()
        {
            if (processTypeToShowProductionFor != null)
            {
                lblName.Text = processTypeToShowProductionFor.Name;
                summaryDescription.Text = processTypeToShowProductionFor.SummaryDescription;
            }     
        }


        protected override void PopulateGeneralDataContent()
        {
          
        }
        
       

        protected override ProcessType GetProcessToShow()
        {           
            return processType;
        }

      
                

        protected override void PopulateProductionContentRefresh()
        {
            EntityGroup resolvedOwner;
            // resolve the owner - if this fails, availabilitiy will show unavailable...
            ResolveOwner(out resolvedOwner);


            PopulateOutputsList(resolvedOwner);
        }

        protected override void SetProductionHeading(Label lbl)
        {
            lbl.Text = "REQUIREMENTS";
            PadHeader(lbl);
            
        }

      /*  protected override string GetProductionHeading()
        {
            return "REQUIREMENTS";
        }*/

        private UIComponent AddOutputItemRow(EntityType outputEntityType, bool ownsItem, int amount)
        {
            UIComponent itemRow = new UIComponent(gui);
            grdOutputs.AddEntry(outputEntityType, itemRow);

            DataTypeButton tbCaption;
            CreateItemGridRow(outputEntityType, itemRow, out tbCaption);
            

            return itemRow;
        }


        /// <summary>
        /// this list shows the process outputs. Not to be confused with USED IN on the entity type tooltip
        /// </summary>
        /// <param name="resolvedOwner"></param>
        private void PopulateOutputsList(EntityGroup resolvedOwner)
        {
            grdOutputs.BeginAddingEntries();

            // Owner owner = expedition.ExpeditionOwner;


            // update the list to show availability

          
            if (processType.Outputs != null) // GameData.Instance.ProcessesUsingThisInput.TryGetValue(entityType, out processesUsingThisInput))
            {
                UIComponent itemRow;
                EntityType outputEntityType;

                bool hasInputs, hasTools, ownsItem;
               
                foreach (var output in processType.Outputs) 
                {
                    outputEntityType = output.FinalEntityTypeToCreate;

                    if (!output.IsWasteProduct)// omit waste products
                    {
                       // score = ScoreItem(resolvedOwner, outputEntityType, 0f, out hasInputs, out hasTools, out ownsItem, false);

                        bool ownsItemIncludingIntrinsicParts;
                       
                        //InventoryPanel.OwnsProductOrHasProcessInputsAndTools(item, resolvedOwner, out isAvailable, out ownsItemIncludingIntrinsicParts, out hasInputs, out hasTools, null, false); 
                        InventoryPanel.OwnsProductOrHasProcessInputsAndTools(output.FinalEntityTypeToCreate, resolvedOwner, out ownsItem, out ownsItemIncludingIntrinsicParts, out hasInputs, out hasTools, null, true); 


                        if (!grdOutputs.TryGetEntry(outputEntityType, out itemRow))
                        {
                            itemRow = AddEntityAmountRow(grdOutputs, outputEntityType, ownsItem, "The amount that will be produced");
                        }

                        UpdateEntityAmountRow(itemRow, outputEntityType, ownsItem, output.Amount.NoOfItems ?? 0);
                    }
                }                    
                

                grdOutputs.DeleteEntries<EntityType>(e => processType.Outputs.Any(o => o.FinalEntityTypeToCreate == e)); //  processesUsingThisInput.Exists(p =>  ProcessHasEntityTypeAsOutput(p, e)));

                // sort the grid, first by score, then by name:              
               // grdOutputs.Sort(i => (float)i.OrderByTag1, Grid.Sorting.Descending, i => i.OrderByTag2, Grid.Sorting.Ascending);

            }
            else
            {
                grdOutputs.Clear();
            }



            grdOutputs.EndAddingEntries();


            // show/hide the heading:
            grdProductionOuter.TryRemoveEntry(outputsHeader);

            if (grdOutputs.Entries.Count > 0)
            {               
                grdProductionOuter.AddEntry(outputsHeader, outputsHeader); //, usedInHeaderIndex);
                               
                lblOutputs.Text = "OUTPUT:";
                lblOutputs.ToolTip = "Shows the outputs for the process";
                
            }
        }


        protected override string GetInputHeading()
        {
            return "MATERIALS";
        }
    }
}

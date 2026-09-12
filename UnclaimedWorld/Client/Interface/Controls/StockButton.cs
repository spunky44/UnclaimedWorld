using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls
{
    /// <summary>
    /// has a label with no of available items of a type, opens the entity list window when clicked
    /// </summary>
    public class StockButton: TextButton
    {
        public EntityType EntityType;
        public List<EntityID> EntityList = new List<EntityID>();


        public StockButton(GUIManager gui, EntityType entityType): base(gui)
        {            
            Init(TextButton.TextButtonType.LCDAmount); //LCDToolTipBlack); //TextButton.TextButtonType.LCD);
            ToolTip = "Click to see the list of items";
            EntityType = entityType;
            //EventArgs = eventArgs; // new ItemTypeButtonEventArgs(entityType);
           
           // ID = UIComponent.DataControlID.Stock;

        }

        const string clickToSeeListTooltip = "\n \nClick to see the list";

        public void UpdateStockButton(int? noOfAvailableItems, int? noOfIncompleteItems, int? noOfItemsUsedAsParts, int? noOfItemsOffSite, int? noOfItemsOwnedByOthers,  
           // int? intrinsicToolsThatAreParts,
            List<EntityID> listOfEntities, bool hideIfZero)
        {
            EntityList.Clear();
            if (listOfEntities != null)
            {
                EntityList.AddRange(listOfEntities); // preserve a copy.
            }

            int? unavailable = null;
            if (noOfIncompleteItems != null || noOfItemsUsedAsParts != null || noOfItemsOffSite != null || noOfItemsOwnedByOthers != null)
            {
                unavailable = (noOfIncompleteItems ?? 0) + (noOfItemsUsedAsParts ?? 0) + (noOfItemsOffSite ?? 0) + (noOfItemsOwnedByOthers ?? 0);
            }

            bool inStock = noOfAvailableItems > 0;

            Color color = DataTypeButton.GetStockStatusColor(inStock, GetNormalColor(), Type); 

            if (noOfIncompleteItems != 0 && noOfAvailableItems == 0 || noOfItemsUsedAsParts != 0 && noOfAvailableItems == 0) // ???
            {
                color = Common.ColorFromHex("#FFFFFF"); // ??? 
            }

            LabelColor = color;

            Tag1 = inStock; // hack???


            if (noOfAvailableItems.HasValue && unavailable.HasValue)
            {
                // not used?!?!??!?!

                Text = noOfAvailableItems + "|" + unavailable;

                ToolTip = noOfAvailableItems.Value + " items are available.";

                string unavailableBreakdown = GetUnavailableItemsBreakdown(noOfIncompleteItems, noOfItemsUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers);

                if (!string.IsNullOrEmpty(unavailableBreakdown))
                {
                    ToolTip += " \n" + unavailableBreakdown;
                }

                ToolTip = ToolTip + clickToSeeListTooltip;
            }
            else if (noOfAvailableItems.HasValue)
            {
                Text = noOfAvailableItems.Value.ToString();

                ToolTip = "We have " + noOfAvailableItems.Value + " items in inventory.";
               // ToolTip = noOfAvailableItems.Value + " items are available.";

                if (noOfAvailableItems > 0)
                {
                    Visible = true;
                    ToolTip += clickToSeeListTooltip;
                }
                else if (hideIfZero)
                {
                    Visible = false;
                }
            }
            else
            {
                Text = unavailable.Value.ToString();

                ToolTip = GetUnavailableItemsBreakdown(noOfIncompleteItems, noOfItemsUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers);

                if (unavailable > 0)
                {
                    Visible = true;
                    ToolTip += clickToSeeListTooltip;
                }
                else if (hideIfZero)
                {
                    Visible = false;
                }
            }
            
            if (noOfAvailableItems > 0 || unavailable > 0) 
            {
                Tag1 = true; // hack???
                Enabled = true;
            }
            else
            {
                Enabled = false;
            }

            
            ScaleWidthToFitText();
        }

        private string GetUnavailableItemsBreakdown(int? noOfIncompleteItems, int? noOfItemsUsedAsParts, int? noOfItemsOffSite, int? noOfItemsOwnedByOthers)
        {
            StringBuilder tooltip = new StringBuilder();

            bool addLinebreak = false;
            if (noOfIncompleteItems.HasValue && noOfIncompleteItems.Value > 0)
            {
                tooltip.Append(noOfIncompleteItems.Value);
                tooltip.Append(" items are being produced");

                addLinebreak = true;
            }

            if (noOfItemsUsedAsParts.HasValue && noOfItemsUsedAsParts.Value > 0)
            {
                if (addLinebreak)
                {
                    tooltip.Append(" \n");
                }

                tooltip.Append(noOfItemsUsedAsParts.Value);
                tooltip.Append(" items are parts");

                addLinebreak = true;
            }


            if (noOfItemsOffSite.HasValue&& noOfItemsOffSite.Value > 0)
            {
                if (addLinebreak)
                {
                    tooltip.Append(" \n");
                }

                tooltip.Append(noOfItemsOffSite.Value);
                tooltip.Append(" items are off-site");

                addLinebreak = true;
            }

            if (noOfItemsOwnedByOthers.HasValue && noOfItemsOwnedByOthers.Value > 0)
            {
                if (addLinebreak)
                {
                    tooltip.Append(" \n");
                }

                tooltip.Append(noOfItemsOwnedByOthers.Value);
                tooltip.Append(" items are owned by others");

                addLinebreak = true;
            }

            return tooltip.ToString();

        }

        /*
        public void UpdateStockButton(int? noOfAvailableItems, int? noOfIncompleteItems, int? noOfItemsUsedAsParts, int? noOfItemsOffSite, int? noOfItemsOwnedByOthers,
           bool displayParts,
           List<EntityID> listOfEntities)
        {
            EntityList.Clear();
            if (listOfEntities != null)
            {
                EntityList.AddRange(listOfEntities); // preserve a copy.
            }

            int? unavailable = null;
            if (noOfIncompleteItems != null || noOfItemsUsedAsParts != null || noOfItemsOffSite != null || noOfItemsOwnedByOthers != null)
            {
                unavailable = (noOfIncompleteItems ?? 0) + (noOfItemsUsedAsParts ?? 0) + (noOfItemsOffSite ?? 0) + (noOfItemsOwnedByOthers ?? 0);
            }

            bool inStock = noOfAvailableItems > 0;

            Color color = DataTypeButton.GetStockStatusColor(inStock, GetNormalColor(), Type);

            if (noOfIncompleteItems != 0 && noOfAvailableItems == 0 || noOfItemsUsedAsParts != 0 && noOfAvailableItems == 0) // ???
            {
                color = Common.ColorFromHex("#FFFFFF"); // ??? 
            }

            Color = color;

            Tag1 = inStock;

            //Order of Numbers on the StockButton -> In Stock / Part of / Being produced

            //In Stock
            Text = noOfAvailableItems.ToString();
            ToolTip = noOfAvailableItems + " finished items";

            if (displayParts)
            {
                // Part of
                if (listOfEntities != null && listOfEntities.Count > 0)
                {
                    Entity entity = Entity.FindByID(listOfEntities[0]);


                    if (entity != null && entity.EntityType.StructureType == null)
                    {
                        Text += "|" + noOfItemsUsedAsParts;
                    }
                    else
                    {
                        Text += "|" + " - ";
                    }

                }
                ToolTip = ToolTip + " \n" + noOfItemsUsedAsParts + " items are parts";
            }

            // Being produced            
            Text += "|" + noOfIncompleteItems;
            ToolTip = ToolTip + " \n" + noOfIncompleteItems + " items being produced";

            ToolTip = ToolTip + "\n \n Click to see the list";



            if (noOfItemsUsedAsParts == 0 && noOfIncompleteItems == 0 && noOfAvailableItems == 0)
            {
                Text = "0";
                ToolTip = "No items in stock";
            }


            if (noOfAvailableItems > 0 || noOfIncompleteItems > 0 || noOfItemsUsedAsParts > 0)
            {
                Tag1 = true;
                Enabled = true;
            }
            else
            {
                Enabled = false;
            }


            ScaleWidthToFitText();
        }*/

    }
}

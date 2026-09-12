using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface.EntityPanel
{
    public class CombatHistory : EntityPanelTabPage
    {
        
        FullLCDPanel.TypeIsRepresented EntityHasSkillTypeDelegate;
        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

        // attributes
        protected UIComponent pnSummary;
        protected Label lblKills, lblEndurance, lblAppearance, lblAccuracy, lblPerception;

        // skills 
        protected UIComponent pnKills;
        protected Grid grdKills; //, grdSkillsRight;

        protected Regulator displayRegulator;


        public CombatHistory(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
            : base(gui, entityPanel, true, fullLCD, halfLCD)
        //CommonInterface intf, Point pos, Vector2 dimensions):base (intf, pos, dimensions)
        {
            Title = "COMBAT";

            InitAttributes();

            //InitKills(); //Morten says it takes up too much space...


            //lcdContent.Height
        }

        private void InitAttributes()
        {            
            pnSummary = CreatePanelWithMargins(gui, leftMargin, lcdContent); //new UIComponent(Interface.Instance.gui);
            //pnAttributes.Width = lcdContent.Width;
            //pnAttributes.Y = 15; // won't work when added to grid.

            pnSummary.RenderType = RenderType.CRTAndLCD;

            Label lblHeader = new Label(gui);
            pnSummary.Add(lblHeader);
            lblHeader.Y = 5;
            lblHeader.Text = "COMBAT STATISTICS";
            lblHeader.Init(Label.LabelType.LCDNormal);
          //  lblHeader.X = leftMargin;

            FullLCDPanel.AddLCDLineThin(gui, new Point(0, 23), pnSummary.Width, pnSummary);

            int attributesTop = 35;
            int yPos = attributesTop; //10;

            AddCaptionAndLabel(pnSummary, "Kills:", ref lblKills, 0, ref yPos);
            AddCaptionAndLabel(pnSummary, "Accuracy:", ref lblAccuracy, 0, ref yPos);

         /*   int column2Left = 200;
            yPos = attributesTop;

            AddCaptionAndLabel(pnAttributes, "Endurance:", ref lblEndurance, column2Left, ref yPos);
            AddCaptionAndLabel(pnAttributes, "Appearance:", ref lblAppearance, column2Left, ref yPos);
            */

            pnSummary.Height = yPos + 10;
            
        }


       
        private void InitKills()
        {
            
            pnKills = CreatePanelWithMargins(gui, leftMargin, lcdContent);

            
            int columnSpacing = 15;
            int gridWidth = (pnKills.Width - columnSpacing) / 2; // 340; //360;
            // 318

            Label lblHeader = new Label(gui);
            pnKills.Add(lblHeader);
            lblHeader.Text = "KILLS";
            lblHeader.Init(Label.LabelType.LCDNormal);
           // lblHeader.X = leftMargin;

            Bar line = FullLCDPanel.AddLCDLineThin(gui, new Point(0, 18), pnKills.Width /*- 2 * leftMargin - 80*/ /* gridWidth*/, pnKills);
            line.DebugTag = "SkillsLine";

            int gridTop = 22;

            grdKills = FullLCDPanel.AddGridWithFixedItemHeights(gui, lcdContent, 30);
            grdKills.Width = 220;
                
            pnKills.Add(grdKills);
            grdKills.Y = gridTop;
            grdKills.HMargin = 0;
         
            // make sure that the panel can expand around the grid!
            grdKills.HeightResize += new ResizeHandler(grdSkills_HeightResize);
            
        
        }

        void grdSkills_HeightResize(UIComponent sender)
        {
            pnKills.Height = grdKills.Y + grdKills.Height + 10; // (int)MathHelper.Max(grdSkillsLeft.Y + grdSkillsLeft.Height + 10, grdSkillsRight.Y + grdSkillsRight.Height + 10);
        }


       

        private void PopulateKills()
        {
            // lcdContentGrid.AddEntry(pnSkills, pnSkills);

          /*  FullLCDPanel.PopulateCategoryGrid<SkillType, Skill, SkillCategory>(gui, grdSkillsLeft, grdSkillsRight, grdSkillsLeft.Width - 30,
                EntityHasSkillTypeDelegate, SetSummaryDelegate,
                entityPanel.SelectedEntity.PersonEntity.Skills); */
            
            grdKills.BeginAddingEntries();
            grdKills.Clear();

            grdKills.AddEntryRightJustifyValue(null, null, null, null, "Night piper", 0, "2");
            grdKills.AddEntryRightJustifyValue(null, null, null, null, "Tree dragon", 0, "1");
            grdKills.AddEntryRightJustifyValue(null, null, null, null, "Hecatonth", 0, "1");

            grdKills.EndAddingEntries();               
        }

        private string AttributeToString(float attribute)
        {
            return ((int)(100 * attribute)).ToString();
        }

        private void PopulateSummary()
        {
            lblAccuracy.Text = "89 %"; // AttributeToString(entityPanel.SelectedEntity.MobileEntity.Agility);
            lblKills.Text = "4      Night piper (2), Tree dragon (1), Hecatonth (1)"; //"3"; //AttributeToString(entityPanel.SelectedEntity.MobileEntity.Strength);

           // lblEndurance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Endurance);
           // lblAppearance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Appearance);

           
        }

        

        public override void Refresh()
        {            
            // lcdSurface.Add(pnAttributes);

            // when we remove the panels, CleanUp is called, and the currently focused control loses focus
            // this means that a click doesn't work becuase mouse down sets focus, and mouse up checks focus before firing Click!
            // therefore, we must preserve the controls if at all possible!
            lcdContentGrid.BeginAddingEntries();
            //   lcdContentGrid.Clear();

            if (entityPanel.SelectedEntity.Intelligence != null)
            {
                if (!lcdContentGrid.EntriesByKey.ContainsKey(pnSummary))
                {
                    lcdContentGrid.AddEntry(pnSummary, pnSummary);
                }
                PopulateSummary();

               /* if (!lcdContentGrid.EntriesByKey.ContainsKey(pnKills))
                {
                    lcdContentGrid.AddEntry(pnKills, pnKills);
                }
                PopulateKills(); not wanted...
                * */
            }
            else
            {
                lcdContentGrid.TryRemoveEntry(pnSummary);
                lcdContentGrid.TryRemoveEntry(pnKills);
            }

           
            lcdContentGrid.EndAddingEntries();           
        }

    }
}


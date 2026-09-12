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
    public class SkillsAndAttributes : EntityPanelTabPage
    {
        
        FullLCDPanel.TypeIsRepresented EntityHasSkillTypeDelegate;
        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

        // attributes
        protected UIComponent pnAttributes;
        protected Label lblStrength, lblEndurance, lblAppearance, lblAgility, lblPerception;

        // skills 
        protected UIComponent pnSkills;
        protected Grid grdSkillsLeft, grdSkillsRight;

        protected Regulator displayRegulator;


        public SkillsAndAttributes(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
            : base(gui, entityPanel, true, fullLCD, halfLCD)
        //CommonInterface intf, Point pos, Vector2 dimensions):base (intf, pos, dimensions)
        {
            Title = "MAIN";

            InitAttributes();
            InitSkills();
        }

        private void InitAttributes()
        {            
            pnAttributes = CreatePanelWithMargins(gui, leftMargin, lcdContent); //new UIComponent(Interface.Instance.gui);
            //pnAttributes.Width = lcdContent.Width;
            //pnAttributes.Y = 15; // won't work when added to grid.

            pnAttributes.RenderType = RenderType.CRTAndLCD;

            Label lblHeader = new Label(gui);
            pnAttributes.Add(lblHeader);
            lblHeader.Y = 5;
            lblHeader.Text = "ATTRIBUTES";
            lblHeader.Init(Label.LabelType.LCDNormal);
          //  lblHeader.X = leftMargin;

            FullLCDPanel.AddLCDLineThin(gui, new Point(0, 23), pnAttributes.Width, pnAttributes);

            int attributesTop = 35;
            int yPos = attributesTop; //10;

            AddCaptionAndLabel(pnAttributes, "Strength:", ref lblStrength, 0, ref yPos);
            AddCaptionAndLabel(pnAttributes, "Agility:", ref lblAgility, 0, ref yPos);

            int column2Left = 200;
            yPos = attributesTop;

            AddCaptionAndLabel(pnAttributes, "Endurance:", ref lblEndurance, column2Left, ref yPos);
            AddCaptionAndLabel(pnAttributes, "Appearance:", ref lblAppearance, column2Left, ref yPos);

            pnAttributes.Height = yPos + 10;

        }


        

        private void InitSkills()
        {
            EntityHasSkillTypeDelegate = new FullLCDPanel.TypeIsRepresented(EntityHasSkillType);
            SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SetSummary);

            pnSkills = CreatePanelWithMargins(gui, leftMargin, lcdContent);
                       
            int columnSpacing = 15;
            int gridWidth = (pnSkills.Width - columnSpacing) / 2; // 340; //360;
            // 318

            Label lblHeader = new Label(gui);
            pnSkills.Add(lblHeader);
            lblHeader.Text = "SKILLS";
            lblHeader.Init(Label.LabelType.LCDNormal);
           // lblHeader.X = leftMargin;

            Bar line = FullLCDPanel.AddLCDLineThin(gui, new Point(0, 18), pnSkills.Width /*- 2 * leftMargin - 80*/ /* gridWidth*/, pnSkills);
            line.DebugTag = "SkillsLine";

            int gridTop = 22;

            grdSkillsLeft = FullLCDPanel.AddTreeGrid(gui, CollapsablePanel.PanelType.DropDownBig);
            grdSkillsLeft.IsOuterGrid = false;
            grdSkillsLeft.Width = gridWidth; // lcdContentGrid.Width;
            pnSkills.Add(grdSkillsLeft);
            grdSkillsLeft.Y = gridTop;
            grdSkillsLeft.HMargin = 0;
           // grdSkillsLeft.X = leftMargin;
            // make sure that the panel can expand around the grid!
            grdSkillsLeft.HeightResize += new ResizeHandler(grdSkills_HeightResize);


            grdSkillsRight = FullLCDPanel.AddTreeGrid(gui, CollapsablePanel.PanelType.DropDownBig);
            grdSkillsRight.Width = gridWidth; // lcdContentGrid.Width;
            grdSkillsRight.IsOuterGrid = false;
            pnSkills.Add(grdSkillsRight);
            grdSkillsRight.Y = gridTop;
            grdSkillsRight.HMargin = 0;
            grdSkillsRight.X = grdSkillsLeft.X + grdSkillsLeft.Width + columnSpacing; // 333;
            // make sure that the panel can expand around the grid!
            grdSkillsRight.HeightResize += new ResizeHandler(grdSkills_HeightResize);
        }

        void grdSkills_HeightResize(UIComponent sender)
        {
            pnSkills.Height = (int)MathHelper.Max(grdSkillsLeft.Y + grdSkillsLeft.Height + 10, grdSkillsRight.Y + grdSkillsRight.Height + 10);
        }


        

       

        public void SetSummary(CollapsablePanel cpCategory, object o)
        {
            // find the best skill in each category and display it in the bar:
            string skillValue = o.ToString();

            string maxSkillValueInCategory = cpCategory.Summary;
            if (maxSkillValueInCategory == "" || int.Parse(maxSkillValueInCategory) < int.Parse(skillValue))
            {
                cpCategory.Summary = skillValue;
            }
        }

        public bool EntityHasSkillType(object type)
        {
            return entityPanel.SelectedEntity.Intelligence.Skills.ContainsKey((SkillType)type);
        }

        private void PopulateSkills()
        {
            // lcdContentGrid.AddEntry(pnSkills, pnSkills);

            FullLCDPanel.PopulateCategoryGrid<SkillType, Skill, SkillCategory>(gui, grdSkillsLeft, grdSkillsRight, grdSkillsLeft.Width - 30,
                EntityHasSkillTypeDelegate, SetSummaryDelegate,
                entityPanel.SelectedEntity.Intelligence.Skills); 
        
                
        }

        private string AttributeToString(float attribute)
        {
            return ((int)(100 * attribute)).ToString();
        }

        private void PopulateAttributes()
        {
            lblAgility.Text = AttributeToString(entityPanel.SelectedEntity.Locomotor.LeggedLocomotor.Agility);
            lblStrength.Text = AttributeToString(entityPanel.SelectedEntity.Locomotor.LeggedLocomotor.Strength);

            if (entityPanel.SelectedEntity.BiologicalEntity != null)
            {
                lblEndurance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Endurance);
                lblAppearance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Appearance);
            }
            else
            {
                lblEndurance.Text = "n/a";
                lblAppearance.Text = "n/a";
            }
            /*
            lblAgility.SetText(AttributeToString(entityPanel.SelectedEntity.MobileEntity.Agility));            
            lblStrength.SetText(AttributeToString(entityPanel.SelectedEntity.MobileEntity.Strength));

            lblEndurance.SetText(AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Endurance));
            lblAppearance.SetText(AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Appearance));*/
        }

        

        public override void Refresh()
        {            
            // lcdSurface.Add(pnAttributes);

            // when we remove the panels, CleanUp is called, and the currently focused control loses focus
            // this means that a click doesn't work becuase mouse down sets focus, and mouse up checks focus before firing Click!
            // therefore, we must preserve the controls if at all possible!
            lcdContentGrid.BeginAddingEntries();
            //   lcdContentGrid.Clear();

            if (entityPanel.SelectedEntity.Locomotor.LeggedLocomotor != null)
            {
                if (!lcdContentGrid.EntriesByKey.ContainsKey(pnAttributes))
                {
                    lcdContentGrid.AddEntry(pnAttributes, pnAttributes);
                }
                PopulateAttributes();
            }
            else
            {
                lcdContentGrid.TryRemoveEntry(pnAttributes);
            }

            if (entityPanel.SelectedEntity.PersonEntity != null)
            {
                //lcdContentGrid.AddEntry(grdSkills, grdSkills);
                if (!lcdContentGrid.EntriesByKey.ContainsKey(pnSkills))
                {
                    lcdContentGrid.AddEntry(pnSkills, pnSkills);
                }
                PopulateSkills();
            }
            else
            {
                lcdContentGrid.TryRemoveEntry(pnSkills);
            }

            lcdContentGrid.EndAddingEntries();

            /*if (!hasBeenDrawn)
            {
                TextButton expand;
                Label lblDescription, lblLevel;

                int descriptionColumnX = Interface.Instance.leftMargin; //sender.position.X + 100; 
                int levelColumnX = descriptionColumnX + 220;
                int yPos = 100;

                // setup skill labels:
                foreach (KeyValuePair<SkillType, Skill> kvp in Interface.Instance.SelectedEntity.PersonEntity.Skills)
                {
                    lblDescription = new Label(intf.gui);
                    Form.Add(lblDescription);
                    lblDescription.Text = kvp.Key.Name;
                    lblDescription.Position = new Point(descriptionColumnX, yPos); // "lblDescr" + kvp.Key.Name, new Vector2(descriptionColumnX, yPos), kvp.Key.Name, Label.Alignment.Left, null, Color.Black, intf.InterfaceFont);

                    lblLevel = new Label(intf.gui);
                    Form.Add(lblLevel);
                    lblLevel.Text = kvp.Value.ToString();
                    lblLevel.Position = new Point(levelColumnX, yPos); //"lblLvl" + kvp.Key.Name, new Vector2(levelColumnX, yPos), kvp.Value.ToString(), Label.Alignment.Left, null, Color.Black, intf.InterfaceFont);
                    skillLabels.Add(kvp.Key, lblLevel);
                    yPos += 20;
                }

                hasBeenDrawn = true;

            }
            else if (displayRegulator.IsReady(UWGame.SimSide.Instance.GameTime))
            {
                UpdateSkills();
            }
            */

            /*    if (intf.SelectedEntity is Entities.PersonEntity)
                {
                    System.Text.StringBuilder description = new System.Text.StringBuilder();
                    System.Text.StringBuilder levels = new System.Text.StringBuilder();
                    foreach (KeyValuePair<SkillType, Skill> kvp in ((PersonEntity)intf.SelectedEntity).Skills)
                    {
                        Skill skill = kvp.Value;
                        description.Append(skill.SkillType.Name);
                        description.Append(": ");
                        description.Append("\n");
                        levels.Append(skill.Value);
                        levels.Append("\n");
                    }

                    formSpriteBatch.DrawString(intf.InterfaceFont, description.ToString(), new Vector2(descriptionColumnX, intf.topMargin), Color.Black);
                    formSpriteBatch.DrawString(intf.InterfaceFont, levels.ToString(), new Vector2(levelColumnX, intf.topMargin), Color.Black);

                }*/
        }

    }
}


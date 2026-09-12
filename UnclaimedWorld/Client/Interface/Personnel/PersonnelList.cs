using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.AI.StrategicDecisions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.ClientSide.Interface.Personnel
{
    /// <summary>
    /// this list of personnel can be hosted on an lcd surface in various places   
    /// </summary>
    public class PersonnelList : UIComponent
    {
        Grid outerGrid;

        bool isRoster;

        const int itemHeight = 25; // 25 is needed to accomodate the text button when FixedItemHeight is false  23; // 20;
        const int horizPadding = 6;
        const int vertPadding = 2;

      //  const int ratingColumnX = 50, professionColumnX = 91, nameColumnX = 113, missionStatusColumnX = 252, ageColumnX = 282, sexColumnX = 310, statusColumn = 410;
        const int ratingColumnX = 65, professionColumnX = 106, nameColumnX = 128, missionStatusColumnX = 267, ageColumnX = 297, sexColumnX = 325, statusColumn = 425;

        //   List<EntityID> entities;
        /// <summary>
        /// decoupled from SharedKnowledge!
        /// </summary>
        List<IKnownEntityData> entities;

        /// <summary>
        /// to get people, decouple from SharedKnowledge for main menu use
        /// </summary>
        Func<List<IKnownEntityData>> getEntities;


        bool showSelectors = false;
        bool showMigrationRisk = false;

        Dictionary<EntityID, List<CategoryPanelAndData>> customPanels = new Dictionary<EntityID, List<CategoryPanelAndData>>();


        public PersonnelList(CommonInterface intf, UIComponent lcdSurface, bool showSelectors, int bottomMargin, bool isRoster)
            : base(intf.gui)
        {
            this.showSelectors = showSelectors;
            this.isRoster = isRoster;

            //  RosterPanel.CreateRosterStyleLCDPanel(intf, window, out display, out lcdSurface, ref lcdScreen);

            if (isRoster)
            {
                Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, itemHeight, out outerGrid, bottomMargin,
                     new Tuple<string, int, int>("OPINION", 0, 98),
                     new Tuple<string, int, int>("PERSON DATA", 106, 312),
                     new Tuple<string, int, int>("STATUS", 410, 123)
                     );

               /* Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, itemHeight, out outerGrid, bottomMargin,
                     new Tuple<string, int, int>("OPINION", 0, 83),
                     new Tuple<string, int, int>("PERSON DATA", 91, 312),
                     new Tuple<string, int, int>("STATUS", 410, 123)
                     );*/
            }
            else
            {
                Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, itemHeight, out outerGrid, bottomMargin,
                    new Tuple<string, int, int>("OPINION", 0, 98),
                    new Tuple<string, int, int>("PERSON DATA", 106, 312)
                    );
                /*
                Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, itemHeight, out outerGrid, bottomMargin,
                     new Tuple<string, int, int>("OPINION", 0, 83),
                     new Tuple<string, int, int>("PERSON DATA", 91, 312)
                     );*/
            }

            outerGrid.FixedItemHeights = false;
        }


        public void Fill(Func<List<IKnownEntityData>> getEntities, bool showSelectors, bool showMigrationRisk)
        {
            this.getEntities = getEntities;

            // this.showSelectors = showSelectors;

            this.showSelectors = showSelectors;
            this.showMigrationRisk = showMigrationRisk;

        }

        private UIComponent AddRow(IKnownEntityData entityData)
        {

            UIComponent item;
            item = new UIComponent(guiManager);
            item.Height = itemHeight;
            item.Tag1 = entityData.EntityID;

            AddControlsToCollapsedPart(item, entityData);

            outerGrid.AddEntry(entityData.EntityID, item);

            return item;
        }

        private void AddExpandedPart(UIComponent item, EntityID entityID)
        {
            Box panelBox = new Box(guiManager); // lcdSurfaceOptions.Width);
            //item.Add(rowPanel.Panel);           
            panelBox.ID = DataControlID.PanelBox;
            panelBox.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), Color.LightYellow, Color.LightYellow);
            panelBox.Height = 0;
            panelBox.Width = outerGrid.Width - 20;
            panelBox.Y = item.FindChildById(UIComponent.DataControlID.Name).Bottom + 2;  //item.Bottom + 2; // FindChildById(UIComponent.DataControlID.Selector).Bottom + 2;
            panelBox.ID = DataControlID.PanelBox;
            panelBox.Visible = true;
            panelBox.DebugTag = "panelBox";

            panelBox.Tag1 = entityID;

            int gridTopMargin = 4;

            Grid grid = new Grid(guiManager, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
            grid.FixedItemHeights = false;
            grid.RenderType = RenderType.CRTAndLCD;
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Width = 250;
            grid.ID = DataControlID.GridInItemRow;
            grid.Height = 50;
            grid.ItemHeight = 26;//22; // why?
            grid.Position = new Point(0, gridTopMargin);
            grid.CanGrowInHeight = true;
            grid.ScrollBarEnabled = false;
            grid.HeightResize += grid_HeightResize;
            grid.DebugTag = "innerGrid";

            panelBox.Add(grid);
            panelBox.CenterChildHorizontally(grid);

            item.Add(panelBox);


            panelBox.Parent.Height = itemHeight;

            AddCustomPresentationPanel(entityID, grid);
            //return rowPanel;
        }

        void grid_HeightResize(UIComponent sender)
        {
            Grid innerGrid = (Grid)sender;

            SetExpandedItemHeight(innerGrid);
        }

        private static void SetExpandedItemHeight(Grid innerGrid)
        {

            UIComponent panelBox = innerGrid.Parent;
            panelBox.Height = innerGrid.Height + 8; // sets expanded panel height

            UIComponent itemRow = panelBox.Parent;
            itemRow.Height = panelBox.Height + itemHeight; // +7; // sets item row height
        }

        private void AddCustomPresentationPanel(EntityID entityID, Grid grid)
        {
            List<CategoryPanelAndData> customPanel = new List<CategoryPanelAndData>();

            Entity entity = LookUp<Entity, EntityID>.FindByID(entityID);

            if (entity.IsOnPlaySite())
            {
                SidePanelEntity.AddPresentationPanel(grid, customPanel, GameData.Instance.CustomSidePanelData);
            }
            else
            {
                SidePanelEntity.AddPresentationPanel(grid, customPanel, GameData.Instance.CustomOtherSiteSidePanelData);
            }
            grid.Tag1 = Entity.FindByID(entityID).AllegianceID;
           

            customPanels.Add(entityID, customPanel);
        }

        private void AddControlsToCollapsedPart(UIComponent item, IKnownEntityData entityData)
        {

            CheckBox cbSelect = new CheckBox(guiManager);
            cbSelect.Init(CheckBoxType.LCDNoLabel);
            //rowPanel.AddContent(cbSelect, 0);
            item.Add(cbSelect);
            cbSelect.X = 0;
            cbSelect.Width = 28;
            cbSelect.ID = DataControlID.Selector;
            item.CenterChildVertically(cbSelect);


            Icon imRating = new Icon(guiManager);
            imRating.ScaleImageToSizeOfControl = false;
            // rowPanel.AddContent(imProfession, Panel.GetItemColumnFromHeader(professionColumnX));   
            item.Add(imRating);
            imRating.X = 28;
            imRating.ID = UIComponent.DataControlID.RatingIcon;
            item.CenterChildVertically(imRating);
            imRating.TooltipWidth = 280;
            imRating.TooltipExpires = false;

            Icon imBiggestConcern = new Icon(guiManager);
            imBiggestConcern.ScaleImageToSizeOfControl = false;
            item.Add(imBiggestConcern);
            imBiggestConcern.X = 44;
            imBiggestConcern.ID = UIComponent.DataControlID.BiggestConcernIcon;
            item.CenterChildVertically(imBiggestConcern);
            imBiggestConcern.TooltipWidth = 280;
            imBiggestConcern.TooltipExpires = false;
            imBiggestConcern.DebugTag = "imBiggestConcern";


            Icon imCannotEmigrate = new Icon(guiManager);
            imCannotEmigrate.ScaleImageToSizeOfControl = false;
            // rowPanel.AddContent(imProfession, Panel.GetItemColumnFromHeader(professionColumnX));   
            item.Add(imCannotEmigrate);
            imCannotEmigrate.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_noEntry"), GameData.Instance.GUIConstants.NegativeTint, Hyperlink.HoverColor);
            imCannotEmigrate.ResizeControlToFitImage();
            imCannotEmigrate.X = ratingColumnX + 10;
            imCannotEmigrate.ID = UIComponent.DataControlID.CannotEmigrate;
            item.CenterChildVertically(imCannotEmigrate);
            imCannotEmigrate.TooltipWidth = 280;
            imCannotEmigrate.TooltipExpires = false;
            imCannotEmigrate.Visible = false;

            Label lblRating = new Label(guiManager);
            //rowPanel.AddContent(lblRating, Panel.GetItemColumnFromHeader(ratingColumnX));
            item.Add(lblRating);
            lblRating.X = ratingColumnX;
            lblRating.Width = 120;
            lblRating.Init(Label.LabelType.LCDNormal);
            lblRating.ID = UIComponent.DataControlID.Rating;
            item.CenterChildVertically(lblRating);
            lblRating.TooltipWidth = 280;
            lblRating.TooltipExpires = false;

            Icon imProfession = new Icon(guiManager);
            // imProfession.Color = Label.LCDNormal; 
            imProfession.ScaleImageToSizeOfControl = false;
            // rowPanel.AddContent(imProfession, Panel.GetItemColumnFromHeader(professionColumnX));   
            item.Add(imProfession);
            imProfession.X = professionColumnX;
            imProfession.Y = 4;
            imProfession.ID = UIComponent.DataControlID.Profession;
            //  item.CenterChildVertically(imProfession);

            Hyperlink hlName = new Hyperlink(guiManager);
            // rowPanel.AddContent(hlName, Panel.GetItemColumnFromHeader(nameColumnX));
            item.Add(hlName);
            hlName.X = nameColumnX;
            hlName.Width = 120;
            hlName.NormalColor = Label.LCDNormal; // new Color(0x1E, 0x52, 0x5C); 
            hlName.ID = UIComponent.DataControlID.Name;
            hlName.TargetEntityID = (uint)entityData.EntityID;
            hlName.RenderType = base.RenderType;
            item.CenterChildVertically(hlName);

            Icon imStatus = new Icon(guiManager);
            // imStatus.Color = Label.LCDNormal; 
            imStatus.ScaleImageToSizeOfControl = false;
            item.Add(imStatus);
            imStatus.X = missionStatusColumnX;
            imStatus.Y = 0;
            imStatus.ID = UIComponent.DataControlID.TravelStatus;


            Label lblAge = new Label(guiManager);
            //rowPanel.AddContent(lblAge, Panel.GetItemColumnFromHeader(ageColumnX));
            item.Add(lblAge);
            lblAge.Width = 40;
            lblAge.X = ageColumnX;
            lblAge.Y = 1;
            lblAge.Init(Label.LabelType.LCDNormal);
            lblAge.ID = UIComponent.DataControlID.Age;
            lblAge.ToolTip = "The age of the person";
            item.CenterChildVertically(lblAge);

            Icon imSex = new Icon(guiManager);
            //  imSex.Color = Label.LCDNormal; // hlName.NormalColor;
            imSex.ScaleImageToSizeOfControl = false;
            item.Add(imSex);
            imSex.X = sexColumnX;
            imSex.Y = 2;
            imSex.ID = UIComponent.DataControlID.Sex;


            TextButton tb = new TextButton(guiManager);
            item.Add(tb);
            tb.Init(TextButton.TextButtonType.LCD);
            tb.Text = "MORE";
            tb.ID = DataControlID.Expand;
            tb.X = 345;
            tb.Click += tbExpand_Click;
            tb.ScaleToFitText();
            tb.MinHeight = 25;
            tb.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;

            if (isRoster)
            {
                Label lblStatus = new Label(guiManager);
                //rowPanel.AddContent(lblRating, Panel.GetItemColumnFromHeader(ratingColumnX));
                item.Add(lblStatus);
                lblStatus.X = statusColumn;
                lblStatus.Width = 120;
                lblStatus.Init(Label.LabelType.LCDNormal);
                lblStatus.ID = UIComponent.DataControlID.Status;
                item.CenterChildVertically(lblStatus);               
                lblStatus.Width = 122;
               // lblStatus.TooltipExpires = false;
            }
        }

        void tbExpand_Click(UIComponent sender, EventArgs e)
        {
            TextButton btExpand = sender as TextButton;
            // Job job = (Job)sender.Tag1;

            EntityID entityID = (EntityID)sender.Parent.Tag1;

            UIComponent itemRow;
            outerGrid.TryGetEntry(entityID, out itemRow);

            if (btExpand.IsChecked)
            {
                // only add the expanded part if it does not exist.
                List<CategoryPanelAndData> entityPanels;
                if (!customPanels.TryGetValue(entityID, out entityPanels))
                {
                    AddExpandedPart(itemRow, entityID);
                }

                // decouple from sharedknowledge...
                IKnownEntityData entityData = entities.FirstOrDefault(n => n.EntityID == entityID);

                if (entityData != null)
                {
                    UpdateExpandedPart(entityData, itemRow);
                }
            }

            Box panel = (Box)itemRow.FindChildById(UIComponent.DataControlID.PanelBox);
            if (btExpand.IsChecked)
            {
                panel.Visible = true;

                btExpand.Text = "LESS";

                Grid innerGrid = (Grid)panel.FindChildById(DataControlID.GridInItemRow);
                SetExpandedItemHeight(innerGrid);
            }
            else
            {

                panel.Visible = false;

                btExpand.Text = "MORE";
                itemRow.Height = itemHeight;
            }

        }

        private void UpdateRow(IKnownEntityData entity, UIComponent itemRow)
        {
            TextButton btExpand = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);

            UpdateCollapsedPart(itemRow, entity);

            if (btExpand.IsChecked)
            {
                UpdateExpandedPart(entity, itemRow);
            }
        }

        private void UpdateExpandedPart(IKnownEntityData entityData, UIComponent itemRow)
        {
            Grid innerGrid = (Grid)itemRow.FindChildById(UIComponent.DataControlID.GridInItemRow);

            SidePanelEntity.PopulateCustomPanels(entityData, customPanels[entityData.EntityID], innerGrid); // Needs, parts, skills etc.      

        }

        private void CancelDialog()
        {
            /* Window.Hide();

             if (CancelClick != null)
                 CancelClick.Invoke(this, null);*/
        }

        private void UpdateCollapsedPart(UIComponent itemRow, IKnownEntityData entityData)
        {
            // we don't support everything for FOW... perhaps later.
            Entity entity = entityData as Entity;


            UIComponent selector = itemRow.FindChildById(DataControlID.Selector);
            if (showSelectors)
            {
                selector.Visible = true;
            }
            else
            {
                selector.Visible = false;
            }

            Icon imProfession = itemRow.FindChildById(UIComponent.DataControlID.Profession) as Icon;

            ProfessionType profession = null;

            if (entity != null)
            {
                profession = entity.Intelligence.Profession;
            }

            if (profession != null && !string.IsNullOrEmpty(profession.Icon))
            {
                imProfession.DebugTag = "profession";
                imProfession.Visible = true;
                imProfession.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(profession.Icon), UIComponent.LCDNormal, Hyperlink.HoverColor);
                imProfession.ResizeControlToFitImage();
                imProfession.ToolTip = "Field: " + profession.Name; //mp I don't want 'profession'

                // itemRow.CenterChildVertically(imProfession);
                itemRow.OrderByTag1 = profession.Name;
            }
            else
            {
                imProfession.Visible = false;
                imProfession.ToolTip = null;
                itemRow.OrderByTag1 = "";
            }


            Hyperlink hlName = itemRow.FindChildById(UIComponent.DataControlID.Name) as Hyperlink;
            hlName.Text = entityData.Name ?? entityData.EntityType.Name;
            itemRow.OrderByTag2 = hlName.Text;

            if (entityData.Location.HasValue)
            {
                hlName.Enabled = true;
            }
            else
            {
                hlName.Enabled = false;
            }



            Reproduction? sex = null;
            if (entityData.CasteType != null)
            {
                sex = entityData.CasteType.Reproduction;
            }

            string sexIcon = null;
            string sexTooltip = null;
            if (sex == Reproduction.Male)
            {
                sexIcon = "lcd_icon_male";
                sexTooltip = "Male";
            }
            else if (sex == Reproduction.Female)
            {
                sexIcon = "lcd_icon_female";
                sexTooltip = "Female";
            }

            if (sexIcon != null)
            {
                Icon imSex = itemRow.FindChildById(UIComponent.DataControlID.Sex) as Icon;
                imSex.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(sexIcon), Label.LCDNormal, Hyperlink.HoverColor);
                imSex.ToolTip = sexTooltip;
                imSex.ResizeControlToFitImage();
                // itemRow.CenterChildVertically(imSex);
            }

            string age = "";


            if (entity != null && entity.EntityType.BiologicalType != null)
            {
                BiologicalEntity bioComponent;
                entity.Find(out bioComponent);
                age = bioComponent.AgeGroup.Age.ToString("N0");
            }

            Label lblAge = itemRow.FindChildById(UIComponent.DataControlID.Age) as Label;
            lblAge.Text = age;
            //  itemRow.CenterChildVertically(lblAge);

            // shows either thumbs up/down/stop sign or a smiley (for playsite)
            Icon imRating = itemRow.FindChildById(UIComponent.DataControlID.RatingIcon) as Icon;

            Icon imBiggestConcern = itemRow.FindChildById(UIComponent.DataControlID.BiggestConcernIcon) as Icon;

            // shows the stop sign (playsite only)
            Icon imCanEmigrate = itemRow.FindChildById(UIComponent.DataControlID.CannotEmigrate) as Icon;

            // shows emigrate risk (playsite) or rating % (othersite)
            Label lblRating = itemRow.FindChildById(UIComponent.DataControlID.Rating) as Label;

            if (entity != null && entity.EntityType.Person != null)
            {
                imRating.Visible = true;

                if (showMigrationRisk == false) // is other allegiance??
                {
                    UpdateOtherSiteEntity(itemRow, entity, imRating, lblRating);
                }
                else
                {
                    UpdatePlaySiteEntity(itemRow, entity, imRating, imCanEmigrate, lblRating, imBiggestConcern);
                }
            }
            else
            {
                imRating.Visible = false;
                lblRating.Visible = false;
                imBiggestConcern.Visible = false;
            }
            

            //aligns the label to the right
            lblRating.FitToText();
            lblRating.AlignRight(ratingColumnX + 32);

            /*
            if (lblRating.Right != 81)
            {
                lblRating.X = 81 - lblRating.Width;
            }*/

            //  imRating.Color = lblRating.NormalColor;
            // Brain is null on othersite...
            /*  string status = entity.Intelligence.Brain.GetStatus().ToUpper(Config.Culture);               
              Label lblStatus = itemRow.FindChildById(UIComponent.DataControlID.Status) as Label;
              lblStatus.Text = status;*/

            Icon imMissionStatus = itemRow.FindChildById(UIComponent.DataControlID.TravelStatus) as Icon;
            string missionTooltip = "";
            string missionIcon = "";
            if (entityData.Site != null)
            {
                missionTooltip = "On site";
                missionIcon = "HUD_icon_structure";
            }
            else
            {
                //missionIcon = mission.GetMissionMarker();
                missionTooltip = "Travelling";
                missionIcon = "hiker_map_icon";

                // imMissionStatus.X = 
            }

            if (isRoster)
            {
                Label lblStatus = itemRow.FindChildById(UIComponent.DataControlID.Status) as Label;

                if (entity != null)
                {
                    string task = entity.Intelligence.Brain.GetStatus().ToUpper(Config.Culture);
                    
                    if (string.IsNullOrEmpty(task))
                    {
                        // happens regularly when Brain has no subgoals, and is Inactive...
                        task = GoalTakeFive.IdlingText.ToUpper(Config.Culture); ;
                    }

                    lblStatus.Text = task;                
                    lblStatus.ToolTip = task;
                }
                else 
                {
                    lblStatus.Text = "";
                    lblStatus.ToolTip = null;
                }
            }


            imMissionStatus.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(missionIcon), Label.LCDNormal, Hyperlink.HoverColor);
            imMissionStatus.ToolTip = missionTooltip;
            imMissionStatus.ResizeControlToFitImage();
            itemRow.CenterChildVertically(imMissionStatus);
            imMissionStatus.CenterThisHorizontally(missionStatusColumnX);

        }

        private void UpdatePlaySiteEntity(UIComponent itemRow, Entity entity, Icon imRating, Icon cannotEmigrate, Label lblEmigrateRisk, Icon imBiggestConcern)
        {
            // For player personnel: show risk and highest desirability target
            Color? ratingColor;

          /*  if (entity.Name.Contains("Ortiz"))
            {
                imRating.DebugTag = "ortiz";
            }
            else
            {
                imRating.DebugTag = "otherperson";
            }*/

            // show happiness?
            float happiness = entity.PersonEntity.Personality.Happiness;

            string sprite;
            // show happiness icon - number as well???
            if (Common.IsGreaterThan(happiness, 0f)) // happiness > 0.1)
            {
                sprite = "lcd_icon_smiley_happy";
                ratingColor = null;

                imBiggestConcern.Visible = false;
            }
            else if (Common.IsZero(happiness)) // happiness > 0)
            {
                sprite = "lcd_icon_smiley_content";
                ratingColor = null;

                imBiggestConcern.Visible = false;
            }
            else
            {
                sprite = "lcd_icon_smiley_unhappy";
                ratingColor = GameData.Instance.GUIConstants.NegativeTint;

               
                //float rating = entity.Intelligence //Statistics.GetRating(item.Key);

                
                if (entity.PersonEntity.Personality.MostUnhappyRating.HasValue)
                {
                    RatingTypes biggestRating = entity.PersonEntity.Personality.MostUnhappyRating.Value;
                    imBiggestConcern.Visible = true;

                    imBiggestConcern.SetSkinLocations(
                        guiManager.GUISpriteSheet.GetSourceRectangle(Rating.RatingsTypeToIcon(biggestRating)),
                        GameData.Instance.GUIConstants.NegativeTint, Hyperlink.HoverColor);

                    imBiggestConcern.ResizeControlToFitImage();
                    itemRow.CenterChildVertically(imBiggestConcern);

                    StringBuilder biggestIssue = new StringBuilder();
                    Common.Append(biggestIssue, "The area causing the most unhappiness for the character: ");
                    Statistic.AppendRatingsTypeToStringAndIcon(biggestIssue, biggestRating);
                    
                    imBiggestConcern.ToolTip = biggestIssue.ToString();
                }
                else
                {
                    imBiggestConcern.Visible = false;
                }

            }

            imRating.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(sprite), ratingColor ?? Label.LCDNormal, Hyperlink.HoverColor);
            // imRating.Color = ratingColor;
            imRating.ResizeControlToFitImage();
            imRating.Y = 3; //itemRow.CenterChildVertically(imRating); <- breaks when expanding the row
            imRating.ToolTip = entity.PersonEntity.Personality.HappinessBreakdown; // ratingTooltip;

           
            // if the person can emigrate, show the risk %. otherwise show the stop icon (same as for other site entities)
            if (entity.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
            {
                cannotEmigrate.Visible = false;
                lblEmigrateRisk.Visible = true;

                // show emigration risk % 
                string migrateTooltip = null;
                lblEmigrateRisk.Text = Common.PercentageToString(entity.Intelligence.EmigrateDecider.MigrationRisk);

                // tooltip should include the same breakdown as for other site entities showing desirability, then converted to emigration risk.
                migrateTooltip = entity.Intelligence.EmigrateDecider.GetMigrateRiskTooltip();
                lblEmigrateRisk.ToolTip = migrateTooltip;
            }
            else
            {
                cannotEmigrate.Visible = true;
                lblEmigrateRisk.Visible = false;

                cannotEmigrate.ToolTip = entity.Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(null); // true); // popCapReached);
            }
        }

        private void SetCannotEmigrateIcon()
        {

        }


        private void UpdateOtherSiteEntity(UIComponent itemRow, Entity entity, Icon imRating, Label lblRating)
        {
            // For non-player personnel: show desirability and thumbs up/down. example: [thumbsup] 2%
            string ratingTooltip = "The person's rating of our colony.";
            string ratingString = "";

            Color? ratingColor = null;

            AllegianceRatings rating;


          //  bool isWithinPopCap = The.InGameUI.UIAllegiance.IsWithinPopulationCap(1); // .IsOverPopulationCap(); 

            string sprite = null;
            if (!entity.Intelligence.EmigrateDecider.CanEmigrateToTarget(The.InGameUI.UIAllegiance)) //CanEmigrateToAnyTarget())
               // || !isWithinPopCap)
            {
                sprite = "lcd_icon_noEntry";
               // imRating.ToolTip = "The person cannot move right now.";
                ratingColor = GameData.Instance.GUIConstants.NegativeTint;
                imRating.ToolTip = entity.Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(The.InGameUI.UIAllegiance); //isWithinPopCap);

                lblRating.Visible = false;
            }
            else
            {
                lblRating.Visible = true;

                rating = entity.Intelligence.GetRatingsForAllegiance(The.InGameUI.UIAllegiance.ID);

                if (rating != null)
                {
                    ratingTooltip = "The person's willingness to join our colony. \n";
                    ratingTooltip += rating.Breakdown;

                    ratingString = Common.PercentageToString(rating.Desirability); // stil show desira as percentage??

                    /*  if (rating.Desirability < 0)
                      {
                          //lcderror color.
                          ratingColor = new Color(0x95, 0x35, 0x40);
                      }*/


                    if (entity.Intelligence.IsReadyForEmbark(The.InGameUI.UIAllegiance))
                    {
                        sprite = "lcd_icon_thumbsUp";
                        ratingColor = GameData.Instance.GUIConstants.PositiveTint; // Common.ColorFromHex("#389953"); // green
                        imRating.ToolTip = "The person is willing to join our colony right now.";
                    }
                    else
                    {
                        sprite = "lcd_icon_thumbsDown";
                        ratingColor = GameData.Instance.GUIConstants.NegativeTint; // new Color(0x95, 0x35, 0x40);
                        imRating.ToolTip = "The person is not willing to join our colony at this time.";
                    }


                    lblRating.Text = ratingString;
                    lblRating.ToolTip = ratingTooltip;
                    // lblRating.NormalColor = ratingColor ?? lblRating.GetNormalColor();

                }
            }

            if (sprite != null)
            {
                imRating.Visible = true;
                imRating.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(sprite), ratingColor ?? Label.LCDNormal, Hyperlink.HoverColor);
                imRating.Y = itemRow.FindChildById(UIComponent.DataControlID.Sex).Y;
                imRating.ResizeControlToFitImage();
            }
            else
            {
                imRating.Visible = false;
            }
        }

        public List<EntityID> GetSelectedEntities()
        {
            List<EntityID> result = null;

            CheckBox cbSelect;
            UIComponent selector;
            foreach (var item in outerGrid.EntriesByKey)
            {
                selector = item.Value.FindChildById(DataControlID.Selector);
                if (((CheckBox)selector).IsChecked)
                {
                    Common.AddToList(ref result, (EntityID)item.Key);
                }
            }

            return result;

        }

        private void HandleDestroyedEntityGroupOrDataSource()
        {
            // TODO: show a popup message to the user.

            outerGrid.Clear();// clear all...

            CancelDialog();

            // Hide();
        }

        private bool GetData()
        {
            entities = this.getEntities.Invoke();

            if (entities == null)
            {
                HandleDestroyedEntityGroupOrDataSource();
                //HandleDestroyedDataSource();

                return false;
            }

            return true;
        }


        public void Populate()
        {
            if (!GetData())
            {
                return;
            }

            outerGrid.BeginAddingEntries();

            EntityID entityID;
            // Entity entity;
            IKnownEntityData entityData;
            UIComponent row;

            if (entities != null)
            {
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    entityData = entities[i];

                    /* if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData)))
                     {*/
                    if (entityData.EntityType.Person != null)
                    {
                        if (!outerGrid.TryGetEntry(entityData.EntityID, out row))
                        {
                            row = AddRow(entityData);
                        }

                        UpdateRow(entityData, row);
                    }
                    // }

                    /*
                    entity = Entity.FindByID(entityID);
                    if (entity != null && entity.EntityType.Person != null)
                    {
                        if (!outerGrid.TryGetEntry(entityID, out row))
                        {
                            row = AddRow(entity);
                        }

                        UpdateRow(entity, row);
                    }*/
                }
            }
            //grid.

            // remove unused rows           
            DeleteRows();

            outerGrid.Sort(r => r.OrderByTag1, Grid.Sorting.Descending, r => r.OrderByTag2, Grid.Sorting.Descending);


            outerGrid.EndAddingEntries();


            /* if (grid.Entries.Count == 0)
             {
                 lblNoWaresNote.Visible = true;
             }
             else
             {
                 lblNoWaresNote.Visible = false;
             }

             UpdateTotals(expeditionOwner);*/

        }

        private void DeleteRows()
        {
            UIComponent item;

            for (int i = outerGrid.Entries.Count - 1; i >= 0; i--)
            {
                item = outerGrid.Entries[i];
                EntityID key = (EntityID)item.Tag1;

                if (!entities.Exists(e => e.EntityID == key)) // .Contains(key))
                {
                    DeleteRow(key);
                }
            }

            //  outerGrid.DeleteEntries<EntityID>(j => entities != null && entities.Contains(j));
        }

        private void DeleteRow(EntityID key)
        {
            outerGrid.RemoveEntry(key);

            customPanels.Remove(key);
        }

    }
}

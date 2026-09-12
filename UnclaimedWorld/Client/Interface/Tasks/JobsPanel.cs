using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Interface.LCD;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs.JobTypes;
namespace UWGame.ClientSide.Interface.Tasks
{
    /// <summary>
    /// one panel per expedition
    /// </summary>
    public class JobsPanel : RosterPanel
    {
        Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
              
        Grid outerGrid;

        LCDInnerPanel topPanel;

       
        const int collapsedItemHeight = 35; // 90;
        const int collapsedContentHeight = 31; // 90;

        const int hyperLinkWidth = 15;
        const int EntityTypeButtonEventArgsSize = 100;
        const int XMoveRightLeftBar = 150;

        const int priorityXpos = 418; // 452;

       // static Color errorColor = Color.Red;
        SortingButtons<TaskSettings.SortColumns> sortingButtons;
        UIComponent sortingButtonsContainer;
        ImageButton ibExpand, ibCompress;

        List<Tuple<string, Job>> allJobsToShow = new List<Tuple<string, Job>>();
        List<Job> tempJobsList = new List<Job>();

        const int titleWidth = 150;
        RadioGroup rgPriority;
        RadioButton rbJobTypeNormal, rbJobTypeHigh, rbJobTypeLow;
       // ComboBox cbHaul;

        ComboBox cbTaskType;
      //  ComboBox cbTaskTypePriority;
        const string selectTaskTypePromptKey = "SELECTTASKTYPE";

        TextArea taMessages;

        public JobsPanel()
            : base("TASKS", 622, false)
        {
         
            CreateTopPanel();

            CreateGridHeaderButtons();

            int yPos = sortingButtonsContainer.Bottom + 2; // rgPriority.Bottom + 4; // 60; // 0;
            outerGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(outerGrid);
            outerGrid.HMargin = 0; // 5; 
            outerGrid.VMargin = 0; // 5; // 
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = lcdSurface.Width;
            outerGrid.Height = lcdSurface.Height - yPos; // -gridTopMargin;
            outerGrid.ItemHeight = collapsedItemHeight;//22;          
            outerGrid.Position = new Point(0, yPos);
            outerGrid.RowSpacing = 2;

            LoadUserSettings();

            // receive economic alerts:
            The.Client.Log.NewEventAlert += Log_NewEventAlert;

        }

        Dictionary<string, JobsMessage> messages = new Dictionary<string, JobsMessage>();
        //List<JobsMessage> messages = new List<JobsMessage>();

        void Log_NewEventAlert(Log.Event newEvent)
        {
            if (newEvent.EventType == The.Client.Log.EconomicEvent)
            {
                string plainText = Label.GetPlainText(newEvent.Text);

                if (!messages.ContainsKey(plainText))
                {
                    messages.Add(plainText, new JobsMessage() { Text = plainText });
                }
                //taMessages.Text += newEvent.Text + " \n";

            }
        }

      
        private void CreateTopPanel()
        {
            topPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, false); // lcdSurfaceOptions.Width);
            //item.Add(rowPanel.Panel);
            topPanel.HorizontalContentPadding = horizPadding;
            topPanel.VerticalContentPadding = vertPadding;
            lcdSurface.Add(topPanel.Panel);

            Box pnMessages = new Box(Interface.gui);
            topPanel.AddContent(pnMessages, -6, -8);
            pnMessages.CornerSize = 20;
            pnMessages.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"));
            pnMessages.Width = topPanel.Panel.Width; // .ContentWidth;
            pnMessages.Height = 60;

            taMessages = new TextArea(Interface.gui, ListBoxType.LCD);
            taMessages.VMargin = 6;
            taMessages.HMargin = 6;
           // topPanel.AddContent(taMessages, 0, 0);
            pnMessages.Add(taMessages);
            //taMessages.Y = 2;
           // taMessages.X = 0;
            taMessages.RenderType = RenderType.CRTAndLCD;
            taMessages.Init(Label.LabelType.LCDNormal);
            taMessages.CanGrowInHeight = false;
            taMessages.ScrollBarEnabled = true;         
            taMessages.Height = 60;
            taMessages.Width = pnMessages.Width; // -12; // topPanel.ContentWidth;
            taMessages.Color = UIComponent.errorColor;

                    
           // rgPriority = CreatePriorityButtons(out rbJobTypeNormal, out rbJobTypeHigh, out rbJobTypeLow);
            
            int taskTypeYPos = 64;
  
            Label lblJobType = new Label(Interface.gui);
            topPanel.AddContent(lblJobType, 0, taskTypeYPos);
            Label.LabelType labelType = Label.LabelType.LCDSmallHeadingBanner; // .LCDHeadingGrey; 
            lblJobType.Init(labelType);
            lblJobType.Text = "SET PRIORITY:"; //was "TASK TYPE:"
            lblJobType.Width = 100; // titleWidth;

            cbTaskType = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            topPanel.AddContent(cbTaskType, 104, taskTypeYPos - 6);
            cbTaskType.Init(ComboBoxTypes.LCD);
            cbTaskType.Width = 145;  
            PopulateTaskTypeCombo();   // invokes selectionChanged which isn't ready yet     
            cbTaskType.SelectionChanged += cbTaskType_SelectionChanged;           
            cbTaskType.ToolTip = "Select a task type to view or set its priority";

            rgPriority = new RadioGroup(Interface.gui);
            topPanel.AddContent(rgPriority, 270, taskTypeYPos - 1);
          
            //  panel.AddContent(rgPriority);
            rgPriority.Width = 280;
            rgPriority.Height = 40;
            // rgPriority.X = priorityXpos; //255;
                        
        
            rbJobTypeLow = new RadioButton(Interface.gui);
            rgPriority.Add(rbJobTypeLow);
            rbJobTypeLow.Init(CheckBoxType.LCDRadioBanner);   
            rbJobTypeLow.Text = Job.GetPriorityAsString(Priority.Low);
            rbJobTypeLow.ToolTip = "Set to low priority. This will affect all current and future tasks of this type.";
            rbJobTypeLow.Tag1 = Priority.Low;  // key;
            rbJobTypeLow.Width = 90;
          //  rbJobTypeLow.ID = UIComponent.DataControlID.PriorityLow;
    
            rbJobTypeNormal = new RadioButton(Interface.gui);
            rgPriority.Add(rbJobTypeNormal);
            rbJobTypeNormal.Init(CheckBoxType.LCDRadioBanner);
            rbJobTypeNormal.X = rbJobTypeLow.Right + SingleSpacing; // .X; // 0; //310;
            rbJobTypeNormal.Text = Job.GetPriorityAsString(Priority.Normal);
            rbJobTypeNormal.ToolTip = "Set to normal priority. This will affect all current and future tasks of this type.";
            rbJobTypeNormal.IsChecked = true;
            rbJobTypeNormal.Tag1 = Priority.Normal; // key;
            rbJobTypeNormal.Width = rbJobTypeLow.Width;
          //  rbJobTypeNormal.ID = UIComponent.DataControlID.PriorityNormal;

            rbJobTypeHigh = new RadioButton(Interface.gui);
            rgPriority.Add(rbJobTypeHigh);
            rbJobTypeHigh.Init(CheckBoxType.LCDRadioBanner);
            rbJobTypeHigh.X = rbJobTypeNormal.Right + SingleSpacing;
            rbJobTypeHigh.Text = Job.GetPriorityAsString(Priority.High);
            rbJobTypeHigh.ToolTip = "Set to high priority. This will affect all current and future tasks of this type.";
            rbJobTypeHigh.Tag1 = Priority.High; // key;
            rbJobTypeHigh.Width = rbJobTypeLow.Width;
        //    rbJobTypeHigh.ID = UIComponent.DataControlID.PriorityHigh;
            
            rbJobTypeNormal.Click += new ClickHandler(btJobTypeNormal_Click);
            rbJobTypeHigh.Click += new ClickHandler(btJobTypeHigh_Click);
            rbJobTypeLow.Click += new ClickHandler(btJobTypeLow_Click);

            UpdateTaskTypeControls();
           
        }

        private void PopulateTaskTypeCombo()
        {
            cbTaskType.AddEntry(selectTaskTypePromptKey, "Select task type:");

            foreach (var item in GameData.Instance.AllJobTypes)
            {
                cbTaskType.BeginAddingEntries();
                cbTaskType.AddEntry(item.Value, item.Value.GetDefaultDisplayName());
                cbTaskType.EndAddingEntries();
            }

            cbTaskType.SelectedKey = selectTaskTypePromptKey;

        }

        private void FillTaskTypeControls(JobType taskType) //, Priority priority)
        {
            Priority priority;
            if (!expedition.OwnedEntities.Policy.JobTypePriorities.TryGetValue(taskType, out priority))
            {
                priority = Priority.Normal;
            }

            switch(priority)
            {
                case Priority.Low:
                    rgPriority.SelectMember(rbJobTypeLow);
                    break;
                case Priority.Normal:
                    rgPriority.SelectMember(rbJobTypeNormal);
                    break;
                case Priority.High:
                    rgPriority.SelectMember(rbJobTypeHigh);
                    break;
            }

           // cbTaskTypePriority.SelectedKey = priority;

        }

      

        private void cbTaskType_SelectionChanged(UIComponent sender)
        {
            UpdateTaskTypeControls();
        }

        private void UpdateTaskTypeControls()
        {
            if (!cbTaskType.SelectedKey.Equals(selectTaskTypePromptKey))
            {
                rgPriority.Visible = true;
                JobType selectedItem = (JobType)cbTaskType.SelectedKey;

                FillTaskTypeControls(selectedItem);
            }
            else
            {
                rgPriority.Visible = false;
            }
        }

     

        public override void Refresh()
        {
            UpdateTopPanel();

            UpdateJobs(null);

            UpdateMessages();

            base.Refresh();
        }

        public override void Update(GameTime gameTime)
        {
            List<JobsMessage> outdatedMessages = null;

            foreach (var item in messages)
            {
                item.Value.Update(gameTime);

                if (item.Value.IsExpired())
                {
                    Common.AddToList(ref outdatedMessages, item.Value);
                }
            }

            if (outdatedMessages != null)
            {
                foreach (var item in outdatedMessages)
                {
                    messages.Remove(item.Text);
                }
            }

            base.Update(gameTime);
        }

        private void LoadUserSettings()
        {
            TaskSettings settings = The.InGameUI.TaskSettings;

            sortingButtons.Fill(settings.SortingSettings);

          /*  SetView(settings.viewType);

            ExpandOrCollapseTopPanel(settings.IsExpanded);
            */
        }

       
        /// <summary>
        /// get representative jobs sorted by creation time
        /// don't show hauling jobs... they still use the global priority setting
        /// </summary>
        private void GetAllJobsToShow()
        {
            allJobsToShow.Clear();
            tempJobsList.Clear();

            tempJobsList.AddRange(expedition.OwnedEntities.OtherJobs);

            ProcessJob processJob;
            foreach (var list in expedition.OwnedEntities.ProductionJobs)
            {
                foreach (var job in list.Value)
                {
                    processJob = job as ProcessJob;
                    if (processJob == null || processJob.HarvestJob == null)
                    {
                        tempJobsList.Add(job);
                    }
                }
                //allJobsToShow.AddRange(job.Value);
            }

            //get harvest jobs from zones
            // harvest jobs are grouped by zone, and by resource type. One row in the jobspanel for each group
            foreach (var zone in expedition.OwnedEntities.Zones)
            {
                if (zone.HarvestJobs != null) 
                {
                    foreach (var listOfJobs in zone.HarvestJobs) // resource type list of jobs
                    {
                        if (listOfJobs.Value.Count > 0)
                        {
                            tempJobsList.Add(listOfJobs.Value[0]); // add one representative job?                         
                        }
                    }
                }
            }

            // more jobs?

            tempJobsList.AddRange(expedition.OwnedEntities.ScoutingJobs);
            tempJobsList.AddRange(expedition.OwnedEntities.FindPreyJobs);
            tempJobsList.AddRange(expedition.OwnedEntities.PatrolJobs);
            tempJobsList.AddRange(expedition.OwnedEntities.AttackAreaJobs);
            tempJobsList.AddRange(expedition.OwnedEntities.CheckProcessJobs);

            foreach (var item in expedition.OwnedEntities.RepairJobs)
            {
                tempJobsList.AddRange(item.Value);
            }
           
            foreach (var item in tempJobsList)
            {
                allJobsToShow.Add(new Tuple<string, Job>(GetKey(item), item));
            }

            // sort the jobs:
            allJobsToShow = allJobsToShow.OrderBy(j => j.Item2.Timestamp).ToList();
        }

        private bool GetProgressOfGroupedJobs(ProcessJob job, out float progress)
        {
            progress = 0f;
            List<ProcessJob> groupedJobs;
            if (job.HarvestJob.Zone.HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out groupedJobs))
            {
                float totalProgress = 0f;
                int countedJobs = 0;
                foreach (var item in groupedJobs)
                {
                    float thisProgress;
                    if (item.GetKnownProgress(out thisProgress))
                    {
                        totalProgress += thisProgress;
                        countedJobs++;
                    }
                }

                if (countedJobs > 0)
                {
                    progress = totalProgress / countedJobs;

                    return true;
                }
                else
                {
                    progress = 0f;
                }
            }

            return true;
        }

        private bool GetProgress(ProcessJob job, out float thisProgress, out bool isMemory)
        {
            if (job.HarvestJob != null)
            {
                isMemory = false;
                return GetProgressOfGroupedJobs(job, out thisProgress);
            }
            else
            {
                IKnownProcess processData;
                
                bool result = job.GetKnownProgress(out thisProgress, out processData);

                if (processData is ProcessMemory)
                {
                    isMemory = true;
                }
                else
                {
                    isMemory = false;
                }

                return result;
            }
        }


    
       
        private void UpdateTopPanel()
        {
            if (!cbTaskType.SelectedKey.Equals(selectTaskTypePromptKey))
            {
                JobType jobType = (JobType)cbTaskType.SelectedKey;

                FillTaskTypeControls(jobType);

                /*
                RadioButton selected = rgPriority.GetSelected();
                rgPriority.se = expedition.Policy.GetPriority((JobType)cbTaskType.SelectedKey);*/
               // cbTaskTypePriority.SelectedKey = expedition.Policy.GetPriority((JobType)cbTaskType.SelectedKey);
                //UpdatePriorityRadios(rbHaulLow, rbHaulNormal, rbHaulHigh, expedition.Policy.HaulToStoragePriority);
            }         

        }


        private void UpdateJobs(ExpandCollapse? expandOrCollapse)
        {
            // grid.Clear();

            UIComponent itemRow;

            GetAllJobsToShow();

            outerGrid.BeginAddingEntries();

           // bool canCancel;

            Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
            EntityGroup owner = expedition.OwnedEntities;

            Job job;
            string key;
            foreach (var item in allJobsToShow)
            {
                job = item.Item2;
                key = item.Item1;

               // canCancel = job.UserCanCancel();
                Box rowPanel = null;
                
                if (!outerGrid.TryGetEntry(key, out itemRow))
                {
                    rowPanel = AddItemRow(owner, key, job); //, canCancel);
                }
                else
                {
                    rowPanel = itemRow as Box;
                }

                UpdateRow(rowPanel, job, owner, expandOrCollapse);
            }

            // remove unused rows                   
            outerGrid.DeleteEntries<string>(j => allJobsToShow.Exists(t => t.Item1 == j));

            outerGrid.Sort(i => i.OrderByTag1, The.InGameUI.TaskSettings.SortingSettings.SortOrder);

            outerGrid.EndAddingEntries();

        }

        /// <summary>
        /// string type, because we need value comparisons
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private string GetKey(Job job)
        {
            ProcessJob pJob = job as ProcessJob;
            if (pJob != null)
            {
                if (pJob.HarvestJob != null)
                {
                    // harvestjobs are grouped, so they must use a special key:
                    return pJob.HarvestJob.Zone.ID.ToString() + pJob.HarvestJob.ResourceType.KeyName;
                }

            }

            return job.ID.ToString();
        }

      /*  private static bool CanExpand(Job job)
        {
            return job is ProcessJob;
        }*/

        private const int orderedX = 200;


      
        private void SetDataTypeButtonWidth(DataTypeButton button)
        {
            button.ScaleWidthToFitText();
            if (button.Width > 140)
            {
                button.Width = 140;
            }
        }

        private void AddControlsToCommonSection(LCDInnerPanel panel, EntityGroup owner, string key, Job job) //, bool canCancel)
        {
            EntityType entityType = null;
            ProcessType processType = null;

            ProcessJob processJob = job as ProcessJob;
            if (processJob != null)
            {
                if (processJob.ProcessType.IsSalvageProcess)
                {
                    //The item that we will display as an EntityTypeButton in the title after the process name      
                    //Example Salvaging <EntityType>
                    if (processJob.ProcessType.Inputs != null && processJob.ProcessType.Inputs.Length > 0)
                    {
                        entityType = processJob.ProcessType.Inputs[0].EntityType;
                    }
                }
                else if (processJob.ReplenishJob != null)
                {
                    Entity entity = Entity.FindByID(processJob.ReplenishJob.EntityToReplenish);
                    if (entity != null)
                    {
                        if (entity.ParentEntityID != null)
                        {
                            entityType = Entity.FindByID(entity.ParentEntityID).EntityType;
                        }
                        else
                        {
                            entityType = entity.EntityType;
                        }
                    }
                }
                else
                {
                    if (processJob.ProcessType.HasOutput) // processJob.ProcessType.Outputs != null && processJob.ProcessType.Outputs.Count() > 0)
                    {
                        //The item that we will display as an EntityTypeButton in the title after the process name                   
                        entityType = processJob.ProcessType.Outputs[0].FinalEntityTypeToCreate;
                    }
                    else
                    {
                        processType = processJob.ProcessType;
                    }
                }
            }

            JobType jobType = job.GetJobType();

            // sequence: left to right, top to bottom
            Label lblJobType = new Label(Interface.gui);
            panel.AddContent(lblJobType, 0, -1);
            Label.LabelType labelType = GetLabelType(job);
            lblJobType.Init(labelType);
            SetTitle(job, jobType, lblJobType);
            lblJobType.ID = UIComponent.DataControlID.JobType;
            lblJobType.Width = titleWidth;

            if (entityType != null)
            {
                DataTypeButton dtOutput = new DataTypeButton(Interface.gui, HUD_Windows.DataSheet.InfoToShow.Data, entityType, owner.ID, false);
                dtOutput.Init(TextButton.TextButtonType.LCDToolTipBlack);
                dtOutput.ID = UIComponent.DataControlID.Caption;
                dtOutput.IsRoot = true;
             //   dtOutput.Text = entityType.PluralName;
                panel.AddContent(dtOutput);
                dtOutput.TextAlignment = TextButton.TextAlign.Left;
                //tbCaption.Width = // (quantityX - captionX);
                SetDataTypeButtonWidth(dtOutput);
                dtOutput.X = lblJobType.Right + 6;
                dtOutput.CenterThisVertically(lblJobType.MiddleVertical);
                //  tbCaption.Y = lblJobType.Y - 3;
                dtOutput.Tag1 = key;
                dtOutput.LabelColor = dtOutput.GetNormalColor();
            }
            else if (processType != null)
            {
                DataTypeButton dtProcessType = new DataTypeButton(Interface.gui, HUD_Windows.DataSheet.InfoToShow.Data, processType, owner.ID, false);
                dtProcessType.Init(TextButton.TextButtonType.LCDToolTipBlack);
                dtProcessType.ID = UIComponent.DataControlID.Caption;
                dtProcessType.IsRoot = true;
              //  dtProcessType.Text = entityType.PluralName;
                panel.AddContent(dtProcessType);
                dtProcessType.TextAlignment = TextButton.TextAlign.Left;              
                SetDataTypeButtonWidth(dtProcessType);
                dtProcessType.X = lblJobType.Right + 6;
                dtProcessType.CenterThisVertically(lblJobType.MiddleVertical);             
                dtProcessType.Tag1 = key;
                dtProcessType.LabelColor = dtProcessType.GetNormalColor();
            }

            ImageButton btExpand = new ImageButton(Interface.gui);
            panel.AddContent(btExpand, 300, -4);
            btExpand.Init(ImageButtonType.LCDExpandWithUpAndDownArrows);
            btExpand.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            btExpand.ToolTip = "Show more details";
            btExpand.Click += new ClickHandler(btnExpand_Click);
            btExpand.Tag1 = key;
            btExpand.Width = 28;          
            btExpand.ID = UIComponent.DataControlID.Expand;

            FillableBar progressBar = new FillableBar(Interface.gui, FillableBar.FillableBarType.ProgressBar);
            progressBar.ShowMaxValueLabelAtEnd = false;
            panel.AddContent(progressBar, btExpand.Right + 4, 4);
            progressBar.ID = UIComponent.DataControlID.Progress;
            progressBar.Width = 70; // 20; // 30;         
            progressBar.MaxValue = 100;          
            progressBar.Height = 12; // 16; // <- this makes it 12 pixels tall? // 12;
            progressBar.BarColor = BaseDataLoader.ProgressColor;

            ComboBox cbPriority = CreatePriorityComboBox();
            cbPriority.ID = UIComponent.DataControlID.Priority;
            cbPriority.Tag1 = GetKey(job);
            cbPriority.SelectionChanged += cbPriority_SelectionChanged;
            panel.AddContent(cbPriority, priorityXpos, -4);
            cbPriority.ToolTip = "Set the priority for this task";

            ImageButton btApplyPriorityToAll = new ImageButton(Interface.gui);
            panel.AddContent(btApplyPriorityToAll, 516, -2);
            btApplyPriorityToAll.InitWithIcon(ImageButtonType.LCD, "lcd_icon_asterisk", false, UIComponent.LCDNormal);
            //btApplyPriorityToAll.CheckedMode = CheckedModes.CannotBeChecked; // CheckedModes.SwitchCheckedStateOnClick;
            btApplyPriorityToAll.Click += btApplyPriorityToAll_Click;
            btApplyPriorityToAll.Tag1 = key;
            btApplyPriorityToAll.Width = 28;
            btApplyPriorityToAll.ID = UIComponent.DataControlID.ApplyPriority;

           
            if (jobType == null)
            {
                btApplyPriorityToAll.Enabled = false;
                btApplyPriorityToAll.ToolTip = "Not possible to set default priority for this task type"; //mp was Unavailable for this task   mp was Unavailable. The task does not belong to any type.
            }
            else
            {
                btApplyPriorityToAll.ToolTip = string.Format("Click to set this priority as the default for all current and future tasks of this type ({0})", jobType.GetDefaultDisplayName()); //"Click to set this task's priority to all current and future tasks of this type ({0})"         
            }

            /*  Label lblLocation = new Label(collapsablePanel.guiManager);
              collapsablePanel.Add(lblLocation);
              lblLocation.Init(Label.LabelType.LCDNormal);
              lblLocation.X = 278;
              lblLocation.Y = lblName.Y;
              lblLocation.Name = "Location";
              lblLocation.Text = "LOCATION: " + locationText ?? "";
              lblLocation.FitToText();*/

            Icon skillIcon = new Icon(Interface.gui);
            panel.AddContent(skillIcon, -5);
            skillIcon.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_icon_person"), UIComponent.LCDNormal, UIComponent.LCDNormal);
            skillIcon.ResizeControlToFitImage();
            skillIcon.Y = 40;
            skillIcon.ID = UIComponent.DataControlID.Skill;
           
            LinkOrLabel lolAssignedWorker = new LinkOrLabel(Interface.gui, Label.LabelType.LCDNormal);
            panel.AddContent(lolAssignedWorker, skillIcon.Right - 7);
            lolAssignedWorker.Hyperlink.NormalColor = Label.LCDNormal;
            lolAssignedWorker.Width = 120;
            lolAssignedWorker.Y = 42;
            lolAssignedWorker.ID = UIComponent.DataControlID.JobsPanelAssignedWorker;
            lolAssignedWorker.Text = "H"; // to set the height...
            lolAssignedWorker.DebugTag = "lolAssignedWorker";


            Label lblLocationCaption = new Label(Interface.gui);
            lblLocationCaption.Init(Label.LabelType.LCDNormal);
            panel.AddContent(lblLocationCaption);
            lblLocationCaption.Text = "LOCATION:";
            lblLocationCaption.FitToText();
            lblLocationCaption.Y = lolAssignedWorker.Y;
            lblLocationCaption.X = 180;

            //Location will contain the current job location if there is one
            Hyperlink hlLocation = new Hyperlink(Interface.gui); //, RenderType.Normal);
            panel.AddContent(hlLocation);
            hlLocation.Initialize();
            hlLocation.NormalColor = Label.LCDNormal;
            hlLocation.ID = UIComponent.DataControlID.Location;
            hlLocation.Y = lblLocationCaption.Y; //lblJobType.Y + 1;
            hlLocation.X = lblLocationCaption.Right + 2; // 223; 


            // merge this with lblStatus below...
          /*  Label lblWeapon = new Label(Interface.gui);
            lblWeapon.Init(Label.LabelType.LCDNormal);
            panel.AddContent(lblWeapon);
            lblWeapon.Text = "NO WEAPON AVAILABLE";
            lblWeapon.FitToText();
            lblWeapon.Y = lolAssignedWorker.Y; // SingleSpacing;
            lblWeapon.ID = UIComponent.DataControlID.WeaponWarning;
            lblWeapon.NormalColor = Label.LCDErrorColor;
            // right adjust:
            lblWeapon.X = priorityXpos - lblWeapon.Width - DoubleSpacing;
            */

            Label lblStatus = new Label(Interface.gui);
            lblStatus.Init(Label.LabelType.LCDNormal);
            panel.AddContent(lblStatus);
            lblStatus.Text = "STATUS:";
            lblStatus.FitToText();
            lblStatus.Y = lolAssignedWorker.Bottom + 8;
            lblStatus.ID = UIComponent.DataControlID.JobStatus;



            /*
            RadioButton btNormal, btHigh, btUrgent;

            RadioGroup rgPriority = CreatePriorityButtons(key, out btNormal, out btHigh, out btUrgent); // new ClickHandler(btNormal_Click), new ClickHandler(btHigh_Click), new ClickHandler(btUrgent_Click));          
            panel.AddContent(rgPriority);
            rgPriority.X = priorityXpos;

            btNormal.Click += new ClickHandler(btNormal_Click);
            btHigh.Click += new ClickHandler(btHigh_Click);
            btUrgent.Click += new ClickHandler(btLow_Click);
            */

            string reason;
            bool canCancel = job.UserCanCancel(out reason);

            ImageButton ibDelete = new ImageButton(Interface.gui);
            panel.AddContent(ibDelete);
            ibDelete.InitWithIcon(ImageButtonType.LCD, "lcd_icon_trash", false);
            ibDelete.ToolTip = canCancel ? "Cancel this task" : reason; // job.GetNoCancelOptionText();     
            ibDelete.X = 370; // rgPriority.X - 75;
            ibDelete.Y = lolAssignedWorker.Y - 4;          
            ibDelete.Click += new ClickHandler(btCancel_Click);
            ibDelete.Tag1 = key;
            ibDelete.Enabled = canCancel;
            ibDelete.ID = UIComponent.DataControlID.Cancel;
            ibDelete.Width = 24;

            /*
            TextButton btnCancel = new TextButton(Interface.gui);
            panel.AddContent(btnCancel);
            btnCancel.Init(TextButton.TextButtonType.LCD);
            btnCancel.X = 356; // rgPriority.X - 75;
            btnCancel.Y = lolAssignedWorker.Y - 4; // rgPriority.Bottom + SingleSpacing;
            btnCancel.Text = "CANCEL";
            // btnCancel.Width = 70;
            btnCancel.ScaleWidthToFitText();
            btnCancel.ToolTip = canCancel ? "Cancel this task" : job.GetNoCancelOptionText();
            btnCancel.Click += new ClickHandler(btCancel_Click);
            btnCancel.Tag1 = key;
            btnCancel.ID = UIComponent.DataControlID.Cancel;
            btnCancel.Enabled = canCancel;
            */

        }

        private static void SetTitle(Job job, JobType jobType, Label lblJobType)
        {
            if (jobType != null)
            {
                lblJobType.Text = jobType.GetDefaultDisplayName().ToUpper(Config.Culture);
            }
            else
            {
                lblJobType.Text = job.GetName().ToUpper(Config.Culture);
            }
        }

        void btApplyPriorityToAll_Click(UIComponent sender, EventArgs e)
        {
            Job job = GetJobFromKey((string)sender.Tag1); // (Job)sender.Tag1;            

            JobType jobType = (job).GetJobType();

            Command priorityJobCommand = new SetJobTypePriority(expedition.OwnedEntities, jobType.KeyName, job.Priority, true);

            The.Client.Controller.StoreAndExecuteCommand(priorityJobCommand);

            UpdateJobs(null);

            UpdateTopPanel();
        }

        const string messagePrefix = "- ";
        private void UpdateMessages()
        {

            //return; 

            taMessages.BeginAddingEntries();
            taMessages.Text = null;

            if (ProductionOrderControl.TotalJobsRequireWarning(expedition.OwnedEntities))
            {
                foreach (var item in expedition.OwnedEntities.ProductionJobs)
                {
                    // count the number of jobs for each output type.
                    int noOfJobs = item.Value.Count;
                    if (noOfJobs > GameData.Instance.GUIConstants.OrderedJobsWithSameOutputToTriggerWarning)
                    {
                        taMessages.Text += messagePrefix + item.Key.PluralName + ": " + ProductionOrderControl.orderSpamWarning + " \n";
                        //taMessages.AddEntry(item.Key.PluralName + ": " + InventoryPanel.orderSpamWarning);
                    }
                }
            }

            foreach (var item in messages)
            {
                taMessages.Text += messagePrefix + item.Value.Text + " \n";
            }
            
            taMessages.EndAddingEntries();
        }

        private ComboBox CreatePriorityComboBox()
        {
            ComboBox cbPriority = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            cbPriority.Init(ComboBoxTypes.LCD);         
            cbPriority.Width = 100;
            cbPriority.BeginAddingEntries();
            cbPriority.AddEntry(Priority.Low, Job.GetPriorityAsString(Priority.Low));
            cbPriority.AddEntry(Priority.Normal, Job.GetPriorityAsString(Priority.Normal));
            cbPriority.AddEntry(Priority.High, Job.GetPriorityAsString(Priority.High));
            cbPriority.EndAddingEntries();
            return cbPriority;
        }

        void cbPriority_SelectionChanged(UIComponent sender)
        {
            // this does not set the priority on all harvest jobs?

            Job job = GetJobFromKey((string)sender.Tag1);
            if (job.ID != JobID.Invalid) // NEW: job in the list can be invalid it seems...
            {
                ProcessJob pJob = job as ProcessJob;
                List<Job> listOfJobs = null;
                if (pJob != null && pJob.HarvestJob != null)
                {
                    List<ProcessJob> allHarvestJobsInGroup;
                    if (pJob.HarvestJob.Zone.HarvestJobs.TryGetValue(pJob.HarvestJob.ResourceType, out allHarvestJobsInGroup))
                    {
                        listOfJobs = new List<Job>();
                        listOfJobs.AddRange(allHarvestJobsInGroup);
                    }
                }
                else
                {
                    Common.AddToList(ref listOfJobs, job);
                }

                Priority priority = (Priority)((ComboBox)sender).SelectedKey;
                foreach (var j in listOfJobs)
                {
                    Command priorityJobCommand = new SetTaskPriority(j.ID, priority);
                    The.Client.Controller.StoreAndExecuteCommand(priorityJobCommand);
                }          
            }
        }

        private void CreateGridHeaderButtons()
        {
            sortingButtonsContainer = new UIComponent(Interface.gui);
            lcdSurface.Add(sortingButtonsContainer);
            sortingButtonsContainer.Width = lcdSurface.Width;
            sortingButtonsContainer.Height = 28;
            sortingButtonsContainer.Position = new Point(0, topPanel.Panel.Bottom);

            sortingButtons = new SortingButtons<TaskSettings.SortColumns>(Interface.gui);
            sortingButtons.Width = lcdSurface.Width;
            sortingButtons.Height = 50;
            sortingButtons.Position = new Point(60, 0);
            sortingButtonsContainer.Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;

            ibCompress = new ImageButton(Interface.gui);
            sortingButtonsContainer.Add(ibCompress);
            ibCompress.InitWithIcon(ImageButtonType.LCD, "lcd_icon_allCollapse", true);
            ibCompress.CheckedMode = CheckedModes.CannotBeChecked;  //CanBeChecked;
            ibCompress.Click += ibCompress_Click;
            ibCompress.Y = 0;
            ibCompress.X = 0;
            ibCompress.ToolTip = "Collapse all";
            ibCompress.Width = 30;
            ibCompress.Height = 30;
            ibCompress.RecalculateIconPosition();
            //   ibList.MouseOut += ibList_MouseOut;


            ibExpand = new ImageButton(Interface.gui);
            sortingButtonsContainer.Add(ibExpand);
            ibExpand.InitWithIcon(ImageButtonType.LCD, "lcd_icon_allExpand", true);
            ibExpand.CheckedMode = CheckedModes.CannotBeChecked; //   CanBeChecked;
            ibExpand.Click += ibExpand_Click;
            ibExpand.ToolTip = "Expand all";
            ibExpand.Width = 30;
            ibExpand.Y = 0;
            ibExpand.X = 30;        
            // ibCategory.Pressed = true;        
            ibExpand.Height = 30;
            ibExpand.RecalculateIconPosition();
            // ibCategory.MouseOut += ibList_MouseOut;

            // tbListView_Click(ibList, null);

            sortingButtons.CreateTextButton(0, 248, "TASK TYPE", TaskSettings.SortColumns.TaskType);
            sortingButtons.CreateTextButton(244, 124, "COMPLETION", TaskSettings.SortColumns.Completion);
            sortingButtons.CreateTextButton(364 /*priorityXpos*/, 136, "PRIORITY", TaskSettings.SortColumns.Priority);            
        }

        enum ExpandCollapse { Expand, Collapse }
        void ibExpand_Click(UIComponent sender, EventArgs e)
        {
            UpdateJobs(ExpandCollapse.Expand);

        }

        void ibCompress_Click(UIComponent sender, EventArgs e)
        {
            UpdateJobs(ExpandCollapse.Collapse);
        }

        private void tbSort_Click()
        {
            UpdateJobs(null);

        }

        
        private RadioGroup CreatePriorityButtons(/*string key,*/ out RadioButton btNormal, out RadioButton btHigh, out RadioButton btLow) // ClickHandler normal, ClickHandler high, ClickHandler urgent)
        {
            RadioGroup rgPriority;
            rgPriority = new RadioGroup(Interface.gui);
          //  panel.AddContent(rgPriority);
            rgPriority.Width = 100;
            rgPriority.Height = 80;
           // rgPriority.X = priorityXpos; //255;

            //3 radio buttons that will change the priority of the job
            btNormal = new RadioButton(Interface.gui);
            btLow = new RadioButton(Interface.gui);
            btHigh = new RadioButton(Interface.gui);


            rgPriority.Add(btLow);
            btLow.Init(CheckBoxType.LCDRadioBanner);
            btLow.X = 0; // btHigh.Right + SingleSpacing;
            btLow.Y = 5;
            btLow.Text = "LOW";
            btLow.ToolTip = "Set to low priority";
            btLow.Tag1 = Priority.Low; // key;
            btLow.Width = 90;
            btLow.ID = UIComponent.DataControlID.PriorityLow;

            rgPriority.Add(btNormal);
            btNormal.Init(CheckBoxType.LCDRadioBanner);
            btNormal.X = btLow.X; // 0; //310;
            btNormal.Y = btLow.Bottom + 4;
            btNormal.Text = "NORMAL";
            btNormal.ToolTip = "Set to normal priority";
            btNormal.IsChecked = true;
            btNormal.Tag1 = Priority.Normal; // key;
            btNormal.Width = btLow.Width;
            btNormal.ID = UIComponent.DataControlID.PriorityNormal;

            rgPriority.Add(btHigh);
            btHigh.Init(CheckBoxType.LCDRadioBanner);
            btHigh.X = btLow.X; // btNormal.Right + SingleSpacing;
            btHigh.Y = btNormal.Bottom + 4;
            btHigh.Text = "HIGH";
            btHigh.ToolTip = "Set to high priority";
            btHigh.Tag1 = Priority.High; // key;
            btHigh.Width = btLow.Width;
            btHigh.ID = UIComponent.DataControlID.PriorityHigh;

            return rgPriority;
        }

        void btnExpand_Click(UIComponent sender, EventArgs e)
        {
            ImageButton btExpand = sender as ImageButton;
           // Job job = (Job)sender.Tag1;

            UIComponent rowPanel;
            outerGrid.TryGetEntry(sender.Tag1, out rowPanel);

            ExpandOrCollapseRow(btExpand.IsChecked, rowPanel);


        }

        private void ExpandOrCollapseRow(bool expand, UIComponent rowPanel)
        {
            ImageButton btExpand = (ImageButton)rowPanel.FindChildById(UIComponent.DataControlID.Expand);
            if (expand)
            {
               // btExpand.Text = "LESS";
                ResizeExpandedPanel(rowPanel);
                btExpand.SetIconSkinState(1);
            }
            else
            {
               // btExpand.Text = "MORE";
                rowPanel.Height = collapsedItemHeight;
                btExpand.SetIconSkinState(0);
            }
        }


        Label.LabelType GetLabelType(Job job)
        {
            JobLabelTypes jobLabelType = JobLabelTypes.SteelGrey;

            JobType jobType = job.GetJobType();
            if (jobType != null && jobType.LabelType.HasValue)
            {
                jobLabelType = jobType.LabelType.Value;
                //return jobType.LabelType.Value;
            }
            else
            {
                ProcessJob processJob = job as ProcessJob;
                if (processJob != null)
                {                   
                    if (processJob.SalvageJob != null)
                    {
                        jobLabelType = JobLabelTypes.Red;
                    }
                    else if (processJob.HarvestJob != null)
                    {
                        jobLabelType = JobLabelTypes.Green;
                    }
                    else if (processJob.BuildingJob != null)
                    {
                        jobLabelType = JobLabelTypes.Blue;
                    }
                    else
                    {
                        jobLabelType = JobLabelTypes.Grey;
                    }                  
                }
            }

            switch (jobLabelType)
            {
                case JobLabelTypes.Blue:
                    return Label.LabelType.LCDHeadingBlue;
                case JobLabelTypes.Brown:
                    return Label.LabelType.LCDHeadingBrown;
                case JobLabelTypes.Green:
                    return Label.LabelType.LCDHeadingGreen;
                case JobLabelTypes.Grey:
                    return Label.LabelType.LCDHeadingGrey;
                case JobLabelTypes.Red:
                    return Label.LabelType.LCDHeadingRed;
                case JobLabelTypes.SteelGrey:
                    return Label.LabelType.LCDHeadingSteelGrey;
                default:
                    return Label.LabelType.LCDHeadingBrown;
            }    

        }



        /// <summary>
        /// test if the job has problems like: not accessible, no materials...
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
      /*  private bool JobHasProblems(Job job)
        {
            if (The.Client.GetIsInaccessible(job.ID) == true)
            {
                return true;
            }


            return false;
        }*/


        private bool JobHasProblems(Job job, EntityGroup owner,
            out bool isInaccessible, out bool blockedByThreat, out bool isBlockedDueToBoldStanceRequired,
            out bool? hasTools, out bool? hasSkill, out bool? hasWeapon, out bool tooFarAwayFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
        {
            bool hasProblems = false;

           
            The.Client.GetFeedback(job.ID, out isInaccessible, out blockedByThreat, 
                out isBlockedDueToBoldStanceRequired, out tooFarAwayFromExpedition, out huntingNotFeasible, out areaNotCleared);
            
            hasTools = null;
            hasSkill = null;
            hasWeapon = null;

            if (isInaccessible == true || tooFarAwayFromExpedition == true || huntingNotFeasible == true || areaNotCleared) 
            {
                hasProblems = true;
            }

            ProcessJob pJob = job as ProcessJob;
            if (pJob != null)
            {
                
                // distinguish between tools in use or available..?
                if (!pJob.ToolsAreAvailable //AllToolsInUse == true 
                    || !InventoryPanel.HasToolsForProcess(pJob.ProcessType, owner)) // it would be better to set a flag from EvaluateJobs instead of iterating
                {
                    hasTools = false;
                    hasProblems = true;
                }
                else
                {
                    hasTools = true;
                }

                if (owner.GetExpedition().HasSkill(pJob.ProcessType.RequiredSkillType))
                {
                    hasSkill = true;
                }
                else
                {
                    hasSkill = false;
                    hasProblems = true;
                }

            }

            IRequiresWeapon requiresWeapon = job as IRequiresWeapon;
            if (requiresWeapon != null)
            {
                if (!requiresWeapon.WeaponsAreAvailable)
                {
                    hasWeapon = false;
                    hasProblems = true;
                }
                else
                {
                    hasWeapon = true;
                }
            }

            return hasProblems;

        }

        


        private void UpdateCollapsedPart(Box cpJob, Job job, EntityGroup owner, out bool hasProblems, out bool? hasTools, out float? progress)
        {
            progress = null;

            Label lblJobType = cpJob.FindChildById(UIComponent.DataControlID.JobType) as Label;
            

            ProcessJob pJob = job as ProcessJob;

#if !RELEASE
            if (Kensei.Dev.Options.GetOption("Dev.Show task importance") && pJob != null)
            {
                lblJobType.Text = string.Format("Imp. {0:N3}, Sc. {1:N3}", pJob.GetImportance(owner), pJob.DebugScore);
                  
            }
            else
            {
                JobType jobType = job.GetJobType();
                SetTitle(job, jobType, lblJobType);
            }
#endif


            bool isInaccessible, blockedByThreat, isBlockedDueToBoldStanceRequired, tooFarAwayFromExpedition, huntingNotFeasible, areaNotCleared;
            bool? hasSkill;
            bool? hasWeapon;
            hasProblems = JobHasProblems(job, owner, out isInaccessible, out blockedByThreat, out isBlockedDueToBoldStanceRequired, out hasTools, out hasSkill, 
                out hasWeapon, out tooFarAwayFromExpedition, out huntingNotFeasible, out areaNotCleared);
                                  
            UpdateWorkerAndSkill(cpJob, job, hasSkill);
           // UpdateWeapon(cpJob, job, hasWeapon);            

            ComboBox cbPriority = (ComboBox)cpJob.FindChildById(UIComponent.DataControlID.Priority);
            cbPriority.SelectedKey = job.Priority;
            /*
            RadioButton rbLow = (RadioButton)cpJob.FindChildById(UIComponent.DataControlID.PriorityLow);
            RadioButton rbNormal = (RadioButton)cpJob.FindChildById(UIComponent.DataControlID.PriorityNormal);
            RadioButton rbHigh = (RadioButton)cpJob.FindChildById(UIComponent.DataControlID.PriorityHigh);

            Priority priority = job.Priority;
            UpdatePriorityRadios(rbLow, rbNormal, rbHigh, priority);
            */
          
            Label lblJobStatus = cpJob.FindChildById(UIComponent.DataControlID.JobStatus) as Label;
           
            string statusText = null;
            string statusTooltip = null;
            Color statusColor = lblJobStatus.GetNormalColorForType();

            if (hasWeapon == false)
            {
                statusText = "No weapon available";
                statusTooltip = "No suitable weapon is available for this task.";
            }
            else if (isInaccessible == true)
            {                                               
                if (blockedByThreat == true)
                {
                    statusText = "Dangerous area";
                    statusTooltip = "The task is located in a dangerous spot. Suggestion: First secure the area with PATROL/ATTACK zones. Use the THREAT overlay (next to the minimap) to highlight dangerous areas."; //was: via security patrols
                }
                else if (isBlockedDueToBoldStanceRequired == true)
                {
                    statusText = "Noone dares go near"; //Noone is willing to go near
                    statusTooltip = "No workers are currently willing to move near danger, perhaps because of injuries and/or low morale";
                    //lblJobStatus.Text += " No characters are currently willing to move near danger";//" Characters will stay away from any dangerous areas due to their current mood";
                }
                else
                {
                    // geographically inaccessible area                   
                    statusText = "Area not accessible";
                    statusTooltip = "The area is geographically inaccessible because of terrain or structures blocking the way"; //mp: I added this: because of terrain blocking the way
                }

                statusColor = Label.LCDErrorColor;                     

            }
            else if (tooFarAwayFromExpedition)
            {
                statusText = "Too far away from camp"; //mp sep 2015_:this presented strange behavior where they were chilling and didnt want to hunt an animal which was a few tiles away.
                statusTooltip = "The target is too far away from the camp.";
                statusColor = Label.LCDErrorColor;                     
            }
            else if (huntingNotFeasible)
            {
                statusText = "Ranged weapon needed";
                statusTooltip = "No workers have the speed to hunt down this animal after it has spotted us.";
                statusColor = Label.LCDErrorColor;                     

            }
            else if (areaNotCleared)
            {
                statusText = "Area not cleared";
                statusTooltip = "The construction area contains items that must be removed first.";
                statusColor = Label.LCDErrorColor;
            }
            else
            {
                lblJobStatus.NormalColor = lblJobStatus.GetNormalColorForType();
            }


            FillableBar progressbar = cpJob.FindChildById(UIComponent.DataControlID.Progress) as FillableBar;
            progressbar.Visible = false;

            if (pJob != null)
            {
                if (statusText == null) // !isInaccessible) // show progress as well as error message?
                {
                    float thisProgress;
                    bool isMemory;
                    if (GetProgress(pJob, out thisProgress, out isMemory))
                    { 
                 
                        progress = thisProgress;
                        progressbar.Visible = true; // sequence matters - valuebar should stay invisible when width = 1

                        progressbar.Value = (int)(100 * progress.Value);
                       
                        PresentationType presentation = GameData.Instance.AllPresentationTypes["ProcessProgress"];

                        if (isMemory)
                        {
                            progressbar.BarColor = Color.Gray;
                            progressbar.ToolTip = presentation.GetValueTerm(progress.Value, null, null) + " (last known status)";
                        }
                        else
                        {
                            progressbar.BarColor = BaseDataLoader.ProgressColor; 
                            progressbar.ToolTip = presentation.GetValueTerm(progress.Value, null, null);
                        }

                        
                        bool isActive;
                        progressbar.UnderBarColor = GetProgressTint(job, out isActive); // ColorAllControls /* .BarColor*/ = GetProgressTint(job);

                        statusText = ""; 

                    }
                }

                TextButton button = cpJob.FindChildById(UIComponent.DataControlID.Cancel) as TextButton;
                if (button != null)
                {                   
                    if (pJob.CumulativelyEstimatedProgress > 0 && button.Enabled == true)
                    {
                        button.Enabled = false;
                        button.ToolTip = "Cannot cancel this ongoing task.";
                    }
                }
            }

            if (statusText != null)
            {
                lblJobStatus.Text = "STATUS: " + statusText;
              
                if (statusTooltip != null)
                {
                    lblJobStatus.ToolTip = statusTooltip;
                }
            }
            else
            {
                lblJobStatus.Text = "";
                lblJobStatus.ToolTip = "";
            }

            lblJobStatus.NormalColor = statusColor;
             

            Hyperlink hlLocation = cpJob.FindChildById(UIComponent.DataControlID.Location) as Hyperlink;
            Point? tilePos;
            EntityID? targetEntity;
            ZoneID? zoneID;
            string locationText = null;

            job.GetLocation(out tilePos, out targetEntity, out zoneID);

            hlLocation.TargetMapPosition = null;
            hlLocation.TargetEntityID = null;
            hlLocation.TargetZoneID = null;

            if (tilePos.HasValue)
            {
                hlLocation.TargetMapPosition = tilePos.Value;
                locationText = MapManager.TilePosToString(tilePos.Value);
            }
            else if (targetEntity.HasValue)
            {                
                hlLocation.TargetEntityID = (uint)targetEntity.Value;
                IKnownEntityData entityData;
                if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(targetEntity.Value, out entityData)))
                {
                    locationText = entityData.GetDisplayName();
                }
            }
            else if (zoneID.HasValue)
            {
                hlLocation.TargetZoneID = (uint)zoneID.Value;
                Zone zone = LookUp<Zone, ZoneID>.FindByID(zoneID);
                if (zone != null)
                {
                    locationText = zone.GetDisplayName();
                }
            }

            if (locationText != null)
            {
                hlLocation.Text = locationText;
                hlLocation.Enabled = true; // resets tooltip also
            }
            else
            {
                hlLocation.Text = "NONE";
                hlLocation.ToolTip = "The task has no location yet.";
                hlLocation.Enabled = false; 
            }
                                  
            hlLocation.ScaleWidthToFitText();

            int maxWidth = 115;           
            if (hlLocation.Width > maxWidth)
            {                   
                hlLocation.Width = Math.Min(hlLocation.Width, maxWidth);
            }

            // right adjust:
           // hlLocation.X = priorityXpos - hlLocation.Width - DoubleSpacing;           
        }

        static Color notActiveColor = Color.Gray;
        static Color activeColor = Common.ColorFromHex("EDFAFF"); 

        public static Color GetProgressTint(Job job, out bool isActive)
        {
            // unstarted: grey
            // started w. worker: white
            // started and no worker required: white:
            //started and worker requird but no worker: grey

            bool isTaken = job.TakenBy.Count > 0;

            ProcessJob pJob = job as ProcessJob;
            if (pJob != null)
            {

                bool isStarted;
                if (pJob.IsStarted(out isStarted) && isStarted)
                {                
                    if (pJob.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded && !isTaken)
                    {
                        // stalled
                        isActive = false;
                        return notActiveColor;
                    }
                    else
                    {
                        isActive = true;
                        return activeColor;
                    }
                }
                else
                {
                    if (!isTaken)
                    {
                        isActive = false;
                        return notActiveColor;
                    }
                    else
                    {
                        isActive = true;
                        return activeColor;
                    }
                }
            }
            else
            {
                if (!isTaken)
                {
                    isActive = false;
                    return notActiveColor;
                }
                else
                {
                    isActive = true;
                    return activeColor;
                }
            }
        }
       
      /*  private static void UpdatePriorityRadios(RadioButton rbLow, RadioButton rbNormal, RadioButton rbHigh, Priority priority)
        {
            switch (priority)
            {
                case Priority.Low:
                    rbLow.IsChecked = true;
                    rbNormal.IsChecked = false;
                    rbHigh.IsChecked = false;
                    break;
                case Priority.Normal:
                    rbLow.IsChecked = false;
                    rbNormal.IsChecked = true;
                    rbHigh.IsChecked = false;
                    break;
                case Priority.High:
                    rbLow.IsChecked = false;
                    rbNormal.IsChecked = false;
                    rbHigh.IsChecked = true;
                    break;
            }
        }*/

        /// <summary>
        /// How do we make room for a list here?
        /// </summary>
        /// <param name="cpJob"></param>
        /// <param name="job"></param>
        /// <param name="hasSkill"></param>
        private static void UpdateWorkerAndSkill(Box cpJob, Job job, bool? hasSkill)
        {
            Icon skillIcon = cpJob.FindChildById(UIComponent.DataControlID.Skill) as Icon;

            LinkOrLabel lolAssignedTo = cpJob.FindChildById(UIComponent.DataControlID.JobsPanelAssignedWorker) as LinkOrLabel;

            lolAssignedTo.Label.NormalColor = lolAssignedTo.Label.GetNormalColorForType();
            lolAssignedTo.ToolTip = null;

            Entity assignedWorker = job.GetAssignedWorker();

            string skillName = null;
            string skillDescription = null;
            ProcessJob pJob = job as ProcessJob;
            if (pJob != null && pJob.ProcessType.RequiredSkillType != null)
            {
                skillName = pJob.ProcessType.RequiredSkillType.Name;
                skillDescription = pJob.ProcessType.RequiredSkillType.Description;
            }

            if (assignedWorker != null)
            {
                lolAssignedTo.Mode = LinkOrLabel.Modes.Link;

                // Does not make sense for grouped gather jobs...
                lolAssignedTo.Text = /*"ASSIGNED TO: " +*/ assignedWorker.Name ?? assignedWorker.EntityType.Name; // The GOPHER's assignedWorker.Name is null so we have to use the assignedWorker.EntityType.Name  AO
                lolAssignedTo.Hyperlink.TargetEntityID = (uint)assignedWorker.ID;

                if (skillName != null)
                {
                    skillIcon.ToolTip = string.Format("This worker supplies the needed '{0}' skill", skillName);
                }
                else
                {
                    skillIcon.ToolTip = null;
                }

            }
            else
            {
                lolAssignedTo.Mode = LinkOrLabel.Modes.Label;
                lolAssignedTo.Hyperlink.TargetEntityID = null;

                if (skillName != null)
                { 
                   // lolAssignedTo.Text = "REQUIRED SKILL: " + pJob.ProcessType.RequiredSkillType.Name;                
                    lolAssignedTo.Text = skillName;
                    skillIcon.ToolTip = string.Format("A worker with '{0}' skill is needed", skillName);

                    if (hasSkill == false)
                    {
                        lolAssignedTo.Label.NormalColor = Label.LCDErrorColor;
                        lolAssignedTo.ToolTip = string.Format("There are no colony members with this skill (Requirement: Skill level above {0:N1})", GameData.Instance.Constants.MinimumSkillValueToUse);
                    }
                    else
                    {
                        lolAssignedTo.ToolTip = skillDescription;
                    }
                }
                else 
                {
                    lolAssignedTo.Text = "NO ONE ASSIGNED";
                }
            }

           // lblAssignedTo.FitToText();
        }


        private UIComponent AddAssignedItem(Grid grid, IKnownEntityData itemData)
        {
            UIComponent row = new UIComponent(The.InGameUI.gui);

            Hyperlink hlName = AddHyperLink(The.InGameUI.gui, (uint)itemData.EntityID, itemData.EntityType.Name, 0); // 20);
            row.Add(hlName);
            // truncate:
            hlName.MaxWidth = 147;

          /*  expandedGridItem.Width = categoryGrid.Width;
            expandedGridItem.Height = lblInputInfo.Height;*/
           
            Label lblInputInfo = new Label(The.InGameUI.gui);
            lblInputInfo.Init(Label.LabelType.LCDNormal);
            row.Add(lblInputInfo);
            /*lblInputInfo.Text = lblText;
            lblInputInfo.FitToText();*/
            lblInputInfo.X = XMoveRightLeftBar;
            lblInputInfo.ID = UIComponent.DataControlID.Status;


            grid.AddEntry(itemData.EntityID, row);

            return row;
        }

        
        

        private void UpdateAllAssignedItems(Grid grid, ProcessJob pJob, EntityGroup owner)
        {
           
            EntityID itemID;
            Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs;
        
            if (!pJob.GetAssignedInputs(out inputs))
            {
                return;
            }
         
            if (inputs != null && inputs.Count > 0) //pJob.InputsAssignedAndOnSite.Count > 0)
            {
                foreach (var item1 in inputs) // pJob.InputsAssignedAndOnSite) // also consumed items!?!?
                {
                    for (int i = item1.Value.Count - 1; i >= 0; i--)
                    {
                        var assignedItem = item1.Value[i];
                        itemID = assignedItem.Item1;
                        IKnownEntityData entityData;
                        if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge,
                            itemID, owner, out entityData))
                        {
                            UpdateAssignedInputs(grid, entityData, "On site");
                        }
                        else
                        {
                            item1.Value.RemoveAt(i);
                        }
                    }

                  /*  foreach (var item2 in item1.Value)
                    {
                        itemID = item2.Item1;
                        IKnownEntityData entityData;
                        if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge,
                            itemID, owner, out entityData))
                        {

                            UpdateAssignedInputs(grid, entityData, "On site");
                        }
                                                
                    }*/
                }
            }
            

            if (pJob.InputsBeingHauled.Count > 0)
            {
                foreach (var item1 in pJob.InputsBeingHauled)
                {
                    for (int i = item1.Value.Count - 1; i >= 0; i--)
                    {
                        itemID = item1.Value[i];
                        IKnownEntityData entityData;
                        if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge,
                            itemID, owner, out entityData))
                        {
                            UpdateAssignedInputs(grid, entityData, "In transit");
                        }
                        else
                        {
                            item1.Value.RemoveAt(i);
                        }                       
                    }

                    /*
                    foreach (var item2 in item1.Value)
                    {
                        itemID = item2;
                        IKnownEntityData entityData;
                        if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge,
                            itemID, owner, out entityData))
                        {

                            UpdateAssignedInputs(grid, entityData, "In transit");
                        }
                    }*/
                }
            }

            // remove unused rows           
            grid.DeleteEntries<EntityID>(j => IsAssignedAndOnSite(j, inputs) // pJob.IsAssignedAndOnSite(j)
                                         || Common.MultiListContains(pJob.InputsBeingHauled, null, j));


        }

        private bool IsAssignedAndOnSite(EntityID entityID, Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs)
        {
            foreach (var item in inputs)
            {
                if (item.Value.Exists(t => t.Item1 == entityID))
                    return true;
            }

            return false;
        }

        private void GetInputStatus(ProcessJob pJob, Input input, out int unassigned, out int inTransit, out int onSite)
        {           

            List<Tuple<EntityID, WorldLocation>> onSiteItems;
            Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs;
            if (!pJob.GetAssignedInputs(out inputs))
            {
                unassigned = 0;
                inTransit = 0;
                onSite = 0;
                return;
            }

            if (inputs.TryGetValue(input.EntityType, out onSiteItems))
            {
                onSite = onSiteItems.Count; // also consumed items
            }
            else
            {
                onSite = 0;
            }


            List<EntityID> inTransitItems;
            if (pJob.InputsBeingHauled.TryGetValue(input.EntityType, out inTransitItems))
            {
                inTransit = inTransitItems.Count;
            }
            else
            {
                inTransit = 0;
            }


            bool isStarted;
            if (pJob.IsStarted(out isStarted))
            {
                if (isStarted)
                {
                    unassigned = 0; // no more is needed now that production has started!
                    return;
                }
            }

            unassigned = input.Amount.NoOfItems.Value - inTransit - onSite;

        }

        private bool UpdateHarvestJobResources(EntityGroup owner, Grid grid, ProcessJob pJob, ref bool setTitleColorToRed)
        {
           
            int inaccessible = 0;
            int accessible = 0;

            Job otherJob;

            //For each harvest job in this zone
            //
            // but a harvester will claim several harvest jobs in the same tile/container...
            // harvest jobs are grouped by zone, and by resource type.
            Zone zone = pJob.HarvestJob.Zone;
            if (zone != null && zone.HasHarvestJobs()) // needed?
            {
                foreach (var otherProcessJob in pJob.HarvestJob.Zone.HarvestJobs[pJob.HarvestJob.ResourceType])
                {
                    if (The.Client.GetIsInaccessible(otherProcessJob.ID) == true)
                    {
                        inaccessible++;
                    }
                    else
                    {
                        accessible++;
                    }                   
                }
            }

            
            if (accessible <= 0 && inaccessible > 0)
            {
                setTitleColorToRed = true;
            }
            
           
            string accessibleKey = "accessible"; 
            string inAccessibleKey = "inaccessible";

            // there will be max 2 items in this grid.
            UIComponent row;           
            if (accessible > 0)
            {
                if (!grid.TryGetEntry(accessibleKey, out row))
                {
                    row = AddEntityTypeRow(pJob.HarvestJob.ResourceType.ResourceItemType, accessibleKey, owner, grid);
                    
                }

                UpdateEntityTypeRow(row, /*pJob.HarvestJob.ResourceType.Name,*/ accessible, "Accessible", false);

            }
            else
            {
                grid.TryRemoveEntry(accessibleKey);
            }

            if (inaccessible > 0)
            {
                if (!grid.TryGetEntry(inAccessibleKey, out row))
                {
                    row = AddEntityTypeRow(pJob.HarvestJob.ResourceType.ResourceItemType, inAccessibleKey, owner, grid);

                }

                UpdateEntityTypeRow(row, /*pJob.HarvestJob.ResourceType.Name,*/ inaccessible, "Inaccessible", true);

            }
            else
            {
                grid.TryRemoveEntry(inAccessibleKey);
            }
            
            return true;
            
        }


        private void UpdateProcessJobInputs(Box cPanel, ProcessJob pJob, EntityGroup owner, Grid grid, ref bool setTitlebarColorToRed)
        {            
            
            // 2 nested grids:
            UIComponent unassignedInputsControl;
            grid.TryGetEntry(unassignedInputsGridKey, out unassignedInputsControl);

            Grid grdUnassignedInputs = unassignedInputsControl as Grid;
            grdUnassignedInputs.BeginAddingEntries();


            UIComponent assignedInputsControl;
            grid.TryGetEntry(assignedInputsGridKey, out assignedInputsControl);

            Grid grdAssignedInputs = assignedInputsControl as Grid;
            grdAssignedInputs.BeginAddingEntries();

            // update:
            UpdateAllUnassignedInputs(pJob, owner, ref setTitlebarColorToRed, grdUnassignedInputs);
            UpdateAllAssignedItems(grdAssignedInputs, pJob, owner);
                   

            grdUnassignedInputs.EndAddingEntries();
            grdAssignedInputs.EndAddingEntries();


        }

        private void UpdateAllUnassignedInputs(ProcessJob pJob, EntityGroup owner, ref bool setTitlebarColorToRed, Grid grdUnassignedInputs)
        {
            foreach (var input in pJob.ProcessType.Inputs)
            {               
                UpdateUnassignedInputs(grdUnassignedInputs, input, pJob, owner, ref setTitlebarColorToRed);
            }
          
        }

        private string GetMaterialStatus(int takenBy, bool hasInputs)
        {
            if (takenBy > 0) // hack?
            {
                return "On site";
            }
            else if (hasInputs)
            {
                return "In inventory"; 
            }
            else
            {
                return "Not in inventory";  //MP: was "not existing"
            }
        }

        /// <summary>
        /// Will update the status of input materials for a job
        /// So for example it will update what is currently happening to an stone that is going to be used for an construction job
        /// 
        /// It also updates the status of sub-jobs like if one harvest job has 10 harvest jobs inside (harvest 10 sticks)
        /// 
        /// it will group theese up and display them so for expample 3 harvest jobs are inaccessible due to terrain and 7 are accessible.
        /// </summary>
        /// <param name="itemRow"></param>
        /// <param name="job"></param>
        /// <param name="owner"></param>
        /// <param name="hasItemsToUpdate"></param>
        private void UpdateLeftColumn(Box cPanel, ProcessJob pJob, EntityGroup owner, ref bool hasProblems)
        {           
            bool leftColumnHasProblems = false;

            Label lblInputHeading = (Label)cPanel.FindChildById(UIComponent.DataControlID.InputHeading);

            if (pJob.HarvestJob != null)
            {
                lblInputHeading.Text = "RESOURCES";
                lblInputHeading.ToolTip = "This column shows a summary of the resources being gathered";

                Grid grid = (Grid)cPanel.FindChildById(UIComponent.DataControlID.LeftGrid); 
                grid.BeginAddingEntries();

                UpdateHarvestJobResources(owner, grid, pJob, ref leftColumnHasProblems);

                grid.EndAddingEntries();
            }
            else if (pJob != null && pJob.ProcessType.Inputs != null)
            {
                lblInputHeading.Text = "MATERIALS";
                lblInputHeading.ToolTip = "This column shows the status of the needed input materials";

                Grid grid = (Grid)cPanel.FindChildById(UIComponent.DataControlID.LeftGrid); 
                grid.BeginAddingEntries();

                UpdateProcessJobInputs(cPanel, pJob, owner, grid, ref leftColumnHasProblems);

                grid.EndAddingEntries();
            }

            lblInputHeading.Width = firstColumnWidth;

            if (leftColumnHasProblems)
            {
                hasProblems = true;
            }          
        }

        private void ResizeExpandedPanel(UIComponent itemRow) //  Box itemRow)
        {
            int bottom = 0;
           
            foreach (var item in itemRow.Controls)
            {
                if (item.Visible)
                {
                    bottom = Common.Max(bottom, item.Bottom); //Get the tallest column panel 
                }
            }

            itemRow.Height = bottom;
        }

       

       // string errorLabelKey = "errorLabelKey";
        string noTools = "Tools not in inventory"; //or "Tools not owned"  
        string allToolsInUse = "Tools owned but currently in use"; //"Tools available but currently in use"
        string allToolsBroken = "Tools are broken and unusable";

        private void UpdateToolsColumn(UIComponent itemRow, ProcessJob pJob, EntityGroup owner, bool? hasTools)
        {
            EntityID toolID = EntityID.Invalid;
            IKnownEntityData entityData;
            UIComponent toolEntry;

            Grid grdTools = (Grid)itemRow.FindChildById(UIComponent.DataControlID.Tools); // .Controls[1]; // second control is the tools grid
            Label lblToolsError = (Label)itemRow.FindChildById(UIComponent.DataControlID.ToolsError);

            grdTools.BeginAddingEntries();

            //bool hasTools = hasToolsForProcessJob;

            // cases currently covered:
            //We got the tools to do the job
            //We own tools to do the job but they are in use
            //We do not own any tools that can do the job.

            //Case1: If we own tools to do the job we should display what tools are currently assigned.
            //Case2: If we own tools to do the job but they are in use we will display a text about this to the player.
            //Case3: If we do not own tools to do the job will display a text about this to the player.


            List<EntityID> AssignedTools = new List<EntityID>();
            if (pJob.HarvestJob != null)
            {
                foreach (var hJob in pJob.HarvestJob.Zone.HarvestJobs)
                {
                    if (hJob.Key == pJob.HarvestJob.ResourceType) //Go through all harvest jobs of this type in the area
                    {
                        foreach (var innerHJob in hJob.Value)
                        {
                            foreach (var tool in innerHJob.AssignedTools)
                            {
                                AssignedTools.Add(tool);
                            }
                        }
                    }
                }
            }
            else
            {
                AssignedTools = pJob.AssignedTools;
            }

            if (hasTools == true && pJob.ToolsAreAvailable) // AllToolsInUse == false)
            {
                lblToolsError.Visible = false;
                grdTools.Visible = true; 

                //First we want to clean up the case when we did not have any tools available
            /*    if (grdTools.TryGetEntry(errorLabelKey, out toolEntry))
                {
                    //Are we displaying an label saying that we got no availible tools?
                    //Remove this label.

                    grdTools.RemoveEntry(errorLabelKey);
                }*/

                //Then we add a tool entry for each assigned tool we have if we have not already done that

                for (int i = AssignedTools.Count - 1; i >= 0; i--)
                {
                    toolID = AssignedTools[i];

                    if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge, toolID, owner, out entityData))
                    {
                        // add/update/delete tools entries from the grid:
                        if (grdTools.TryGetEntry(toolID, out toolEntry))
                        {
                            // update
                            // UpdateToolEntry(toolEntry, entityData);
                        }
                        else
                        {
                            // add
                            AddToolEntry(grdTools, entityData);
                        }
                    }                
                }
            }
            else
            { 
                //If there are no available tools we should display an label telling the player this.
                lblToolsError.Visible = true;
                grdTools.Visible = false; // this can still contain assigned tool entries (immovable tools only?), but they are in use...

                if (pJob.AllToolsInUse)
                {
                    lblToolsError.Text = allToolsInUse;
                }
                else if (pJob.AllToolsAreBroken)
                {
                    lblToolsError.Text = allToolsBroken;
                }
                else
                {
                    lblToolsError.Text = noTools;
                }

                lblToolsError.Height = paddedErrorHeight; // give it some padding

            }

            //And lastly we cleanup any tools that are displayed but are no longer assigned.
            grdTools.DeleteEntries<EntityID>(e => AssignedTools.Contains(e));

            grdTools.EndAddingEntries();

        }

        const int paddedErrorHeight = 23;

        private void AddToolEntry(Grid toolGrid, IKnownEntityData tool)
        {
            UIComponent toolEntry = new UIComponent(Interface.gui);
            Hyperlink hlName = AddHyperLink(Interface.gui, (uint)tool.EntityID, tool.EntityType.Name, 0); // 10);
            toolEntry.Add(hlName);

            toolGrid.AddEntry(tool.EntityID, toolEntry);


        }

        private void AddLabelEntry(Grid grid, string labelText, Color color, string key)
        {
            UIComponent labelEntry = new UIComponent(Interface.gui);
            // Hyperlink hlName = AddHyperLink(Interface.gui, (uint)tool.EntityID, tool.EntityType.Name, 0); // 10);
            // toolEntry.Add(hlName);
            Label label = new Label(Interface.gui);
            label.Init(Label.LabelType.LCDNormal);
            label.NormalColor = color;
            label.Text = labelText;
            label.ID = UIComponent.DataControlID.Status;
            labelEntry.Add(label);

            grid.AddEntry(key, labelEntry);


        }


        private Hyperlink AddHyperLink(GUIManager guiManager, uint targetID, string text, int xOffSet)
        {
            Hyperlink hlName = new Hyperlink(guiManager, RenderType.Normal);
            hlName.Initialize();
            hlName.NormalColor = Label.LCDNormal;
            hlName.Text = text;
            hlName.X += xOffSet;
            hlName.ID = UIComponent.DataControlID.Caption;
            hlName.TargetEntityID = targetID;
            return hlName;
        }

       
        /// <summary>
        /// process job tools and inputs
        /// </summary>
        /// <param name="typeToShow"></param>
        /// <param name="key"></param>
        /// <param name="owner"></param>
        /// <param name="materialsGrid"></param>
        /// <returns></returns>
        private UIComponent AddEntityTypeRow(EntityType typeToShow, object key, EntityGroup owner, Grid materialsGrid)
        {
            UIComponent row = new UIComponent(The.InGameUI.gui);

            Label lblNumber = new Label(row.guiManager);
            lblNumber.Init(Label.LabelType.LCDNormal);
            row.Add(lblNumber);
          //  lblNumber.Text = numberOfItems.ToString();
            lblNumber.FitToText();
            lblNumber.X = 0;
            lblNumber.ID = UIComponent.DataControlID.Amount;

            DataTypeButton tbProcessOutput = new DataTypeButton(row.guiManager, HUD_Windows.DataSheet.InfoToShow.Production, typeToShow, owner.ID, false);
            tbProcessOutput.Init(TextButton.TextButtonType.LCDToolTipBlack);
           // tbProcessOutput.ID = UIComponent.DataControlID.Caption; // never updates
            tbProcessOutput.IsRoot = true;
         //   tbProcessOutput.Text = typeToShow.PluralName;
            row.Add(tbProcessOutput);
            tbProcessOutput.TextAlignment = TextButton.TextAlign.Left;
            tbProcessOutput.Width = EntityTypeButtonEventArgsSize;// (quantityX - captionX);
            tbProcessOutput.X = 20;
            tbProcessOutput.LabelColor = tbProcessOutput.GetNormalColor();
            tbProcessOutput.Tag1 = typeToShow;
            tbProcessOutput.Width = 128;
         
            Label lblMoreInfo = new Label(row.guiManager);
            lblMoreInfo.Init(Label.LabelType.LCDNormal);
            row.Add(lblMoreInfo);
         //   lblMoreInfo.Text = labelText;
            lblMoreInfo.FitToText();
            lblMoreInfo.X = XMoveRightLeftBar;
         //   lblMoreInfo.NormalColor = color;
            lblMoreInfo.ID = UIComponent.DataControlID.Status;

            materialsGrid.AddEntry(key, row);

            return row;

        }

        /// <summary>
        /// we can reuse this for both materials and harvest jobs
        /// </summary>
        /// <param name="row"></param>
        /// <param name="caption"></param>
        /// <param name="amount"></param>
        /// <param name="moreInfo"></param>
        /// <param name="highlightMoreInfo"></param>
        private void UpdateEntityTypeRow(UIComponent row, /*string caption,*/ int amount, string moreInfo, bool highlightMoreInfo)
        {
            Label label = (Label)row.FindChildById(UIComponent.DataControlID.Amount);
            label.Text = amount.ToString();

          /*  DataTypeButton button = (DataTypeButton)row.FindChildById(UIComponent.DataControlID.Caption);
            button.Text = caption;
            */

            Label lblInfo = (Label)row.FindChildById(UIComponent.DataControlID.Status);
            lblInfo.Text = moreInfo;

            if (highlightMoreInfo)
            {
                lblInfo.NormalColor = Label.LCDErrorColor;                
            }
            else
            {

                lblInfo.NormalColor = lblInfo.GetNormalColorForType();
            }
        }

        /// <summary>
        /// add, update, delete
        /// </summary>
        /// <param name="grdUnassigned"></param>
        /// <param name="input"></param>
        /// <param name="pJob"></param>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        private void UpdateUnassignedInputs(Grid grdUnassigned, Input input, ProcessJob pJob, EntityGroup owner, ref bool highlightParent)
        {
            // items, summed and grouped by type
            int unassigned, inTransit, onSite;
            GetInputStatus(pJob, input, out unassigned, out inTransit, out onSite);
            
            if (unassigned > 0)
            {
                UIComponent row;
                if (!grdUnassigned.TryGetEntry(input.EntityType, out row))
                {                  
                    // add, then update:
                    row = AddEntityTypeRow(input.EntityType,
                        input.EntityType, owner, grdUnassigned);
                }

                UpdateUnassignedInputRow(row, input, pJob, owner, unassigned, ref highlightParent);
            }
            else //remove it:
            {
                grdUnassigned.TryRemoveEntry(input.EntityType);
            }
            

        }


        private void UpdateUnassignedInputRow(UIComponent row, Input input, ProcessJob pJob, EntityGroup owner, int amount, ref bool highlightParent)
        {
            bool hasInputs = false;
         //   bool hasTools = false;
            int productionLimit;
            int? noOfMissingInputTypes, noOfAvailableInputTypes, noOfAvailableItems;

            InventoryPanel.HasInputForProcess(pJob.ProcessType, owner, out hasInputs, 
                out productionLimit, out noOfMissingInputTypes, out noOfAvailableInputTypes, out noOfAvailableItems, input.EntityType);
           
           /* string moreInfo = GetMaterialStatus(pJob.TakenBy.Count, 
                                                hasInputs == false && pJob.IsInputBeginDelivered == false);
            */
            string moreInfo = GetMaterialStatus(pJob.TakenBy.Count, hasInputs); // ???
           

            bool highlight = hasInputs == false;

            UpdateEntityTypeRow(row, /*input.EntityType.PluralName,*/ amount, moreInfo, highlight);

            if (highlight) // mark with red...
            {
                highlightParent = true;
            }
        }


        private void UpdateAssignedInputs(Grid grid, IKnownEntityData itemData, string statusText)
        {
            // items shown as links

            UIComponent row;
            if (!grid.TryGetEntry(itemData.EntityID, out row))
            {
                row = AddAssignedItem(grid, itemData);
            }

            Label label = (Label)row.FindChildById(UIComponent.DataControlID.Status);
            label.Text = statusText;

        }

   
        /// <summary>
        /// only adds the containers, not the items
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="jobKey"></param>
        /// <param name="job"></param>
        /// <param name="canCancel"></param>
        private Box AddItemRow(EntityGroup owner, string jobKey, Job job) //, bool canCancel)
        {
            LCDInnerPanel collapsablePanel = null;

            AddRowPanel(ref collapsablePanel, jobKey);

            // common to all jobs:
            AddControlsToCommonSection(collapsablePanel, owner, jobKey, job); //, canCancel);

            // only some process jobs:
            AddToolsAndMaterialsControls(job, collapsablePanel);

            return collapsablePanel.Panel;
        }

        private void AddToolsAndMaterialsControls(Job job, LCDInnerPanel collapsablePanel)
        {
            if (job is ProcessJob) //CanExpand(job))
            {
                int headerHeight = 20; // 28;

               // header:
                UIComponent headerItem = new UIComponent(Interface.gui);
                headerItem.Height = headerHeight;
                headerItem.Width = collapsablePanel.Panel.Width;
                collapsablePanel.AddContent(headerItem, 0, 82);
               // headerItem.Y = 90; // 64; // collapsedContentHeight + DoubleSpacing;

                Label lblMaterials = new Label(headerItem.guiManager);
                lblMaterials.Init(Label.LabelType.LCDSmallHeadingBanner);
                headerItem.Add(lblMaterials);
              //  lblMaterials.Text = "MATERIALS";
                lblMaterials.Width = firstColumnWidth;
                lblMaterials.ID = UIComponent.DataControlID.InputHeading;

                Label lblTools = new Label(headerItem.guiManager);
                lblTools.Init(Label.LabelType.LCDSmallHeadingBanner);
                headerItem.Add(lblTools);
                lblTools.Text = "TOOLS";
                lblTools.Width = 297;
                lblTools.X = lblMaterials.Right + columnGap; 


                // grids:
                Grid grdLeft = null;
                Grid grdTools = null;

                int yPos = headerItem.Bottom - 6;// collapsedContentHeight + headerHeight + +2; // 10;
                             

                // left column is for inputs:
                ProcessJob pJob = job as ProcessJob;
                if (pJob.HarvestJob != null)
                {
                    grdLeft = AddColumnGrid(collapsablePanel, 0, yPos, 0, firstColumnWidth, true);
                    grdLeft.ID = UIComponent.DataControlID.LeftGrid;

                }
                else if (pJob.ProcessType.Inputs != null)
                {
                    grdLeft = AddColumnGrid(collapsablePanel, 0, yPos, 0, firstColumnWidth, false);
                    grdLeft.ID = UIComponent.DataControlID.LeftGrid;

                    grdLeft.BeginAddingEntries();

                    // add 2 nested grids below eachother:
                    Grid grid = CreateFixedItemHeightGrid(grdLeft.Width); 
                    grdLeft.AddEntry(unassignedInputsGridKey, grid); // creates an empty row?

                    grid = CreateFixedItemHeightGrid(grdLeft.Width);
                    grdLeft.AddEntry(assignedInputsGridKey, grid);

                    grdLeft.EndAddingEntries();
                }

                // right column is for tools:
                int rightColumnXPos = firstColumnWidth + columnGap;
                grdTools = AddColumnGrid(collapsablePanel, rightColumnXPos, yPos, 1, secondColumnWidth, true);
                grdTools.ID = UIComponent.DataControlID.Tools;

                Label lblToolsError = new Label(Interface.gui);
                collapsablePanel.AddContent(lblToolsError, rightColumnXPos, yPos);
                lblToolsError.Init(Label.LabelType.LCDNormal);
                lblToolsError.NormalColor = Label.LCDErrorColor;
                lblToolsError.ID = UIComponent.DataControlID.ToolsError;
                lblToolsError.Visible = false;
                lblToolsError.Height = 23;
            }
        }

        const int firstColumnWidth = 230;
        const int secondColumnWidth = 273;
        //const int secondColumnXPos = 245;
        const int columnGap = 10;

        private void AddProcessJobHeader(LCDInnerPanel cPanel, int headerHeight)
        {
           

        }

        const int horizPadding = 6;
        const int vertPadding = 8; // 4;


        private void AddRowPanel(ref LCDInnerPanel rowPanel, object key)
        {
            rowPanel = new LCDInnerPanel(Interface.gui, outerGrid.Width, false); // lcdSurfaceOptions.Width);
            //item.Add(rowPanel.Panel);
            rowPanel.HorizontalContentPadding = horizPadding;
            rowPanel.VerticalContentPadding = vertPadding;
            outerGrid.AddEntry(key, rowPanel.Panel);
            
        }
        
        const int inputItemHeight = 18; // SidePanel.SlimGridItemHeight; 

        private Grid AddColumnGrid(LCDInnerPanel cPanel, int xPos, int yPos, int i, int width, bool fixedItemHeight)
        {
            Grid grid;
            if (fixedItemHeight)
            {
                grid = CreateFixedItemHeightGrid(width);               
            }
            else
            {
                grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);              
                grid.Width = width; // cPanel.Width / 2; // make grid fill half the collapsable panel   
                grid.ScrollBarEnabled = false;        
                grid.CanGrowInHeight = true;
                grid.Font = GUIManager.LCDandHUDBodyFontPath;
                grid.IsOuterGrid = false;
                grid.Tag1 = i;
            }

            grid.FixedItemHeights = fixedItemHeight;           
            grid.BottomMargin = 4;

            cPanel.AddContent(grid, xPos, yPos); 

            return grid;
        }

        const string unassignedInputsGridKey = "unassigned";
        const string assignedInputsGridKey = "assigned";

       
        private Grid CreateFixedItemHeightGrid(int width)
        {
            Grid grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grid.FixedItemHeights = true;
            grid.Width = width; // grdLeft.Width;
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = inputItemHeight;
            grid.RowSpacing = 4;
            grid.FixedItemHeights = true;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.IsOuterGrid = false;
            grid.Tag1 = 0;
            return grid;
        }



        private void UpdateRow(Box itemRow, Job job, EntityGroup owner, ExpandCollapse? expandOrCollapse)
        {
            bool hasProblems;
            bool? hasTools;
            float? progress;
            UpdateCollapsedPart(itemRow, job, owner, out hasProblems, out hasTools, out progress);

            ProcessJob pJob = job as ProcessJob;

            if (pJob != null)
            {
                UpdateLeftColumn(itemRow, pJob, owner, ref hasProblems);
                UpdateToolsColumn(itemRow, pJob, owner, hasTools);
            }
            
            UpdateTitleColor(itemRow, hasProblems);

            bool expand;
            ImageButton btExpand = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);

            if (expandOrCollapse.HasValue)
            {
                if (expandOrCollapse.Value == ExpandCollapse.Expand)
                {
                    btExpand.IsChecked = true;
                    expand = true;
                }
                else
                {
                    btExpand.IsChecked = false;
                    expand = false;
                }

                //ExpandOrCollapseRow(expandOrCollapse.Value == ExpandCollapse.Expand, itemRow);
            }
            else
            {
                // resize the expanded panel if needed:
               
                expand = btExpand.IsChecked;
                /*
                if (btExpand.IsChecked)
                {
                  //  tbJobs.Text = "LESS";
                    ResizeExpandedPanel(itemRow); // (Box)rowPanel);
                }*/
            }

            ExpandOrCollapseRow(expand, itemRow);

            /*
            TextButton tbJobs = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);
            if (tbJobs != null)
            {               
                UIComponent rowPanel;
                outerGrid.TryGetEntry(tbJobs.Tag1, out rowPanel);

                if (tbJobs.IsChecked)
                {
                    tbJobs.Text = "LESS";
                    ResizeExpandedPanel((Box)rowPanel);
                }
                else
                {
                    tbJobs.Text = "MORE";
                    rowPanel.Height = collapsedItemHeight;
                }
            }*/


             //Set OrderByTag values:
            switch (The.InGameUI.TaskSettings.SortingSettings.SortedBy)
            {
                case TaskSettings.SortColumns.TaskType:
                    itemRow.OrderByTag1 = job.GetName(); 
                    break;

                case TaskSettings.SortColumns.Completion:
                    itemRow.OrderByTag1 = progress ?? -1f;
                    break;

                case TaskSettings.SortColumns.Priority:
                    int prio = 0;
                    switch(job.Priority)
                    {
                        case Priority.Low:
                            prio = 0;
                            break;

                        case Priority.Normal:
                            prio = 1;
                            break;

                        case Priority.High:
                            prio = 2;
                            break;
                    }
                    itemRow.OrderByTag1 = prio;
                    break;
            }
        }

        private static void UpdateTitleColor(Box itemRow, bool hasProblems)
        {
            Label titleBarLabel = itemRow.FindChildById(UIComponent.DataControlID.JobType) as Label;
            if (hasProblems)
            {
                titleBarLabel.NormalColor = Label.LCDErrorColor; 
            }
            else
            {
                titleBarLabel.NormalColor = titleBarLabel.GetNormalColorForType();
            }
        }

        
        void btJobTypeHigh_Click(UIComponent sender, EventArgs e)
        {
            jobTypePriorityButtonClick(Priority.High);
        }

        void btJobTypeNormal_Click(UIComponent sender, EventArgs e)
        {
            jobTypePriorityButtonClick(Priority.Normal);
        }

        void btJobTypeLow_Click(UIComponent sender, EventArgs e)
        {
            jobTypePriorityButtonClick(Priority.Low);
        }

        void jobTypePriorityButtonClick(Priority priority)
        {
            if (!cbTaskType.SelectedKey.Equals(selectTaskTypePromptKey)) //if (cbTaskType.SelectedKey != null)
            {
                JobType jobType = (JobType)cbTaskType.SelectedKey;

                Command priorityJobCommand = new SetJobTypePriority(expedition.OwnedEntities, jobType.KeyName, priority, true);

                The.Client.Controller.StoreAndExecuteCommand(priorityJobCommand);

                UpdateJobs(null);
            }            
        }


        /// <summary>
        /// All prioritybutton clicks uses this function
        /// </summary>
        /// <param name="job"></param>
        /// <param name="priority"></param>
      /*  static void priorityButton_Click(Job job, Priority priority)
        {
            if (job.ID != JobID.Invalid) // NEW: job in the list can be invalid it seems...
            {
                Command priorityJobCommand = new SetTaskPriority(job.ID, priority);

                The.Client.Controller.StoreAndExecuteCommand(priorityJobCommand);
            }
        }*/

      /*  void btHigh_Click(UIComponent sender, EventArgs e)
        {
            Job job = GetJobFromKey((string)sender.Tag1);
            priorityButton_Click(job, Priority.High);
        }

        void btNormal_Click(UIComponent sender, EventArgs e)
        {

            Job job = GetJobFromKey((string)sender.Tag1);
            priorityButton_Click(job, Priority.Normal);
        }

        void btLow_Click(UIComponent sender, EventArgs e)
        {

            Job job = GetJobFromKey((string)sender.Tag1);
            priorityButton_Click(job, Priority.Low);
        }*/

        private Job GetJobFromKey(string key)
        {
            return allJobsToShow.Find(t => t.Item1 == key).Item2;
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Job job = GetJobFromKey((string)sender.Tag1);

            if (job.ID != JobID.Invalid) // NEW: job in the list can be invalid it seems...
            {
                //// cancel the job:
                //job.Destroy(true);

                Command cancelJobCommand = new CancelJob(job.ID);

                The.Client.Controller.StoreAndExecuteCommand(cancelJobCommand);
            }

            UpdateJobs(null);

        }

        public override void Hide()
        {
            base.Hide();


        }




    }
}

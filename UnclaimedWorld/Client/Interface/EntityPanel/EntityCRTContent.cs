using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities.Biological;
using System.Globalization;

namespace UWGame.ClientSide.Interface.EntityPanel
{
    public class EntityCRTContent
    {
        private Dictionary<EntityType, Entity> modelsToPlay = new Dictionary<EntityType, Entity>();

        private UIComponent crtContent;

        private UIComponent modelRenderer;

        private GUIManager gui;

        private EntityPanel entityPanel;
        private FramedCRT framedCRT;

        Dictionary<EntityType, Entity> screenModels = new Dictionary<EntityType, Entity>();
        private Entity currentScreenModel;

        float rotation = 0;
        Vector3 modelLocation, realLocation;
        Matrix perspectiveView;

        const int leftMargin = 34; 
        // int valueLeftMargin = 140;
        const int captionWidth = 100;
        const int lineHeight = 18;

        protected UIComponent pnHeading;
        protected Label lblHeading, lblSubHeading, lblProducer, lblFlavourLine1, lblFlavourLine2;
        protected Bar underline;

        protected Image imCRT;
        protected UIComponent pnBillboards;

        // use this as a person template
        protected UIComponent pnPerson;
        //protected Image imPerson;
        protected Box boxPersonImageBorder;
        protected Label lblPersonHeading, lblPersonSex, lblPersonAge, lblPersonOccupation, lblPersonFamily, lblPersonHealth, lblPersonHome, lblPersonHeight, lblPersonWeight;

        // could these panels/lines be a grid instead?
        // suggestion: populate a grid with max. 8 lines of data, with nice context grouping.
        // keep the text description blob at the bottom (outside the grid).

        // use this for structures
        protected UIComponent pnStructure;
        //protected Image imStructure;
        protected Box boxStructureImageBorder;
        protected Label lblStructureHeading, lblStructureWorkRequired, lblStructureMaterialsRequired, /*lblStructureCapacity,*/ lblStructureCondition, lblStructureEnergy; //, lblStructureDescription;
        protected TextArea taStructureDescription;

        // vehicles...
        protected UIComponent pnVehicle;
        protected Label lblVehicleHeading, lblVehicleSubHeading, lblVehicleProducer, lblVehicleWorkRequired, lblVehicleMaterialsRequired, lblVehicleCapacity
            , lblVehicleRange, lblVehicleSpeed, lblVehicleCondition, lblVehicleEnergy; //, lblStructureDescription;
        protected TextArea taVehicleDescription;

        private int lineNo = 0;

        const int emptyLine = 6;

        public EntityCRTContent(GUIManager gui, EntityPanel entityPanel, FramedCRT framedCRT)
        {
            this.gui = gui;
            this.entityPanel = entityPanel;
            this.framedCRT = framedCRT;

            crtContent = framedCRT.GetNewSurfaceContent();

            InitModelRenderer();

            pnHeading = new UIComponent(gui);
            pnHeading.Width = crtContent.Width;
            pnHeading.Height = crtContent.Height;
            pnHeading.RenderType = RenderType.CRTAndLCD;
            InitHeading(ref lblHeading, ref lblSubHeading, ref lblProducer, pnHeading);

            imCRT = new Image(gui);
            InitImageFrame(ref boxPersonImageBorder, imCRT);

            // TODO: put the panels in a grid please. hide the unused ones.
            InitCRTPersonTemplate();
            InitCRTStructureTemplate();
            InitCRTVehicleTemplate();
        }

        public void AddCRTCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, ref int yPos) //, int lineNo)
        {
            AddCRTCaptionAndLabel(pnPanel, caption, ref lblValue, leftMargin, captionWidth, ref yPos); //, lineNo);
        }

        public void AddCRTCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, int xPos, int captionWidth, ref int yPos) //, int lineNo)
        {
            Label lblCaption = new Label(gui);
            pnPanel.Add(lblCaption);
            lblCaption.Text = caption;
            lblCaption.Init(Label.LabelType.CRTSmall);
            lblCaption.Position = new Point(xPos, yPos);
            lblCaption.AnimateOnCRTScreen = Label.AnimationMode.Line;
            lblCaption.AnimateOnCRTScreenLineNo = lineNo;
            
            lblValue = new Label(gui);
            pnPanel.Add(lblValue);
            //  lblValue.Text = caption;
            lblValue.Init(Label.LabelType.CRTSmall);
            lblValue.Position = new Point(xPos + Common.Max(captionWidth, lblCaption.Width), yPos);
            lblValue.Width = 280;
            lblValue.AnimateOnCRTScreen = Label.AnimationMode.Line;
            lblValue.AnimateOnCRTScreenLineNo = lineNo;
            
            lineNo++;

            yPos += lineHeight;
        }

        private void InitImageFrame(ref Box box, /*UIComponent pnTemplate,*/ Image image)
        {
            box = new Box(gui);
            //  pnTemplate.Add(box);
            // box.Position = new Point(image.X - 6, image.Y - 6);
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutBox");
            box.SetSkinLocation(SkinState.Normal,rect);
            box.RenderType = RenderType.CRTAndLCD;
            box.CornerSize = 11;
        }

        private void InitCRTPersonTemplate()
        {
            pnPerson = new UIComponent(gui);
            pnPerson.Width = crtContent.Width;
            pnPerson.Height = crtContent.Height;
            pnPerson.RenderType = RenderType.CRTAndLCD;

            //   InitHeading(ref lblPersonHeading, pnPerson);

            
            int yPos = 10;
            AddCRTCaptionAndLabel(pnPerson, "AGE:", ref lblPersonAge, ref yPos);            
            AddCRTCaptionAndLabel(pnPerson, "SEX:", ref lblPersonSex, ref yPos);
            AddCRTCaptionAndLabel(pnPerson, "HEIGHT:", ref lblPersonHeight, ref yPos);
            AddCRTCaptionAndLabel(pnPerson, "WEIGHT:", ref lblPersonWeight, ref yPos);
           
            yPos += emptyLine;
            AddCRTCaptionAndLabel(pnPerson, "FAMILY:", ref lblPersonFamily, ref yPos);
            AddCRTCaptionAndLabel(pnPerson, "HOME:", ref lblPersonHome, ref yPos);
            AddCRTCaptionAndLabel(pnPerson, "HEALTH:", ref lblPersonHealth, ref yPos);            

            yPos += emptyLine;
            // TEASER HACK - removed this:
            AddCRTCaptionAndLabel(pnPerson, "CURRENT JOB:", ref lblPersonOccupation, ref yPos);

            yPos += emptyLine;
            lblFlavourLine1 = new Label(gui);
            pnPerson.Add(lblFlavourLine1);
            lblFlavourLine1.Text = "";
            lblFlavourLine1.Init(Label.LabelType.CRTSmall);
            lblFlavourLine1.Position = new Point(leftMargin, yPos);
            lblFlavourLine1.AnimateOnCRTScreen = Label.AnimationMode.Line;
            lblFlavourLine1.AnimateOnCRTScreenLineNo = lineNo;
            yPos += lineHeight;
            yPos += emptyLine;
            lineNo++;
            lblFlavourLine2 = new Label(gui);
            pnPerson.Add(lblFlavourLine2);
            lblFlavourLine2.Text = "";
            lblFlavourLine2.Init(Label.LabelType.CRTSmall);
            lblFlavourLine2.Position = new Point(leftMargin, yPos);
            lblFlavourLine2.AnimateOnCRTScreen = Label.AnimationMode.Line;
            lblFlavourLine2.AnimateOnCRTScreenLineNo = lineNo;
            yPos += lineHeight;

            /*
                        imPerson = new Image(gui);
                        pnPerson.Add(imPerson);

                     //   Rectangle rect = intf.SelectedEntity.PersonEntity.PortraitRect; 
                        imPerson.Texture = gui.GUI_CRT_SpriteSheet.Texture;
                       // imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
                        imPerson.Position = new Point(290, 14);
                        imPerson.Scale = false;
                      //  imStatusBackground.ResizeToFit();
                        //imStatusBackground.Alpha = 1f; // 0.35f;// use for background for text!
                        */

            //InitImageFrame(ref boxPersonImageBorder, pnPerson, imPerson);
        }


        private void InitCRTStructureTemplate()
        {
            pnStructure = new UIComponent(gui);
            pnStructure.Width = crtContent.Width;
            pnStructure.Height = crtContent.Height;
            pnStructure.RenderType = RenderType.CRTAndLCD;

            pnBillboards = new UIComponent(gui);
            pnBillboards.Width = crtContent.Width;
            pnBillboards.Height = crtContent.Height;

            //   InitHeading(ref lblStructureHeading, pnStructure);

            int yPos = 10;

          //  AddCRTCaptionAndLabel(pnStructure, "WORK REQD.:", ref lblStructureWorkRequired, ref yPos);
            AddCRTCaptionAndLabel(pnStructure, "MATERIALS:", ref lblStructureMaterialsRequired, ref yPos);
           // AddCRTCaptionAndLabel(pnStructure, "CAPACITY:", ref lblStructureCapacity, ref yPos);

            yPos += emptyLine;

            AddCRTCaptionAndLabel(pnStructure, "CONDITION:", ref lblStructureCondition, ref yPos);
            AddCRTCaptionAndLabel(pnStructure, "ENERGY:", ref lblStructureEnergy, ref yPos);

            yPos += emptyLine;
            yPos += emptyLine;
            yPos += emptyLine;
            yPos += emptyLine;

            taStructureDescription = new TextArea(gui, ListBoxType.Main);
            pnStructure.Add(taStructureDescription);
            taStructureDescription.Position = new Point(leftMargin, yPos);
            taStructureDescription.Width = pnStructure.Width - leftMargin;
            taStructureDescription.Height = pnStructure.Height - taStructureDescription.Position.Y;
            taStructureDescription.HMargin = 0;
          
            taStructureDescription.Init(Label.LabelType.CRTSmall);
       /*     taStructureDescription.Font = GUIManager.CRTSmallFontPath;
            taStructureDescription.Color = Label.CRTLightBlue;*/

            /*   imStructure = new Image(gui);
               pnStructure.Add(imStructure);

               //   Rectangle rect = intf.SelectedEntity.PersonEntity.PortraitRect; 
               imStructure.Texture = gui.GUI_CRT_SpriteSheet.Texture;
               // imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
               imStructure.Position = new Point(240, 12);
               imStructure.Scale = false;
               //  imStatusBackground.ResizeToFit();
               //imStatusBackground.Alpha = 1f; // 0.35f;// use for background for text!
               */
            //  InitImageFrame(ref boxStructureImageBorder, pnStructure, imStructure);

            // !!!
            //  pnStructure.Add(modelRenderer);

        }

        private void InitCRTVehicleTemplate()
        {
            pnVehicle = new UIComponent(gui);
            pnVehicle.Width = crtContent.Width;
            pnVehicle.Height = crtContent.Height;
            pnVehicle.RenderType = RenderType.CRTAndLCD;

            //   InitHeading(ref lblVehicleHeading, pnVehicle);

            int yPos = 10;

            //    AddCompanyLabel(pnVehicle, ref lblVehicleProducer, yPos);

            //   yPos = 90;

            // AddCaptionAndLabel(pnVehicle, ":", ref lblVehicleWorkRequired, ref yPos);
            AddCRTCaptionAndLabel(pnVehicle, "WORK REQD.:", ref lblVehicleWorkRequired, ref yPos);
          //  AddCRTCaptionAndLabel(pnVehicle, "MATERIALS:", ref lblVehicleMaterialsRequired, ref yPos);

            yPos += emptyLine;
            AddCRTCaptionAndLabel(pnVehicle, "CAPACITY:", ref lblVehicleCapacity, ref yPos);

            yPos += emptyLine;
            AddCRTCaptionAndLabel(pnVehicle, "SPEED:", ref lblVehicleSpeed, ref yPos);
            AddCRTCaptionAndLabel(pnVehicle, "RANGE:", ref lblVehicleRange, ref yPos);
            
            yPos += emptyLine;
            AddCRTCaptionAndLabel(pnVehicle, "CONDITION:", ref lblVehicleCondition, ref yPos);
            AddCRTCaptionAndLabel(pnVehicle, "ENERGY:", ref lblVehicleEnergy, ref yPos);
            
            yPos += emptyLine;
            yPos += emptyLine;
            yPos += emptyLine;
            yPos += emptyLine;

            taVehicleDescription = new TextArea(gui, ListBoxType.Main);
            pnVehicle.Add(taVehicleDescription);
            taVehicleDescription.Position = new Point(leftMargin, yPos);
            taVehicleDescription.Width = pnVehicle.Width - leftMargin;
            taVehicleDescription.Height = pnVehicle.Height - taVehicleDescription.Position.Y;
            taVehicleDescription.HMargin = 0;
            taVehicleDescription.Init(Label.LabelType.CRTSmall);
          //  taVehicleDescription.Font = GUIManager.CRTSmallFontPath;
           // taVehicleDescription.Color = Label.CRTLightBlue;


            //   pnVehicle.Add(modelRenderer);

        }


        private void InitModelRenderer()
        {
            // map from selected entity to Model entity by type
          /*  foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllEntityTypes)
            {
                if (kvp.Value.RenderableType != null && kvp.Value.RenderableType.RenderAsModelType != null && kvp.Value.Person == null && kvp.Value.ItemType == null)
                {
                    Entity newEntity = Entity.Produce(kvp.Value, true);// TODO Refactor, this should use Renderables only, not entities
                                                                   // Entities are for Simulation only
                    screenModels.Add(kvp.Value, newEntity);

                    newEntity.Initialize(null);
                    newEntity.InitializeModelAndOnScreenFunctionality(entityPanel.intf.Game); // //UWGame.SimSide.Instance.ScreenManager.Game);
                }              

            }*/

            // 195 to the right of center (width/2) // w = 1840, w/2 = 920   | w = 1280, w/2 = 640  
            // x = 100:                                     x = 1110                x = 832
            // x = 0:                                       x = 925 (920)           x = 649 (640)


            float modelX = 0f; //100f; // 180f; // 0f; 
            float modelY = -180f; //-280f; 

            int xPointToPlaceModelAt = framedCRT.DisplayWindow.AbsolutePosition.X + 390;
            //int xPointToPlaceModelAt = crtContent.AbsolutePosition.X + crtContent.Width - 95;
            int distanceFromCenter = xPointToPlaceModelAt - gui.ScreenWidth / 2; // (entityPanel.Interface.Game.GetScreenResolution().X / 2); 

            modelX = distanceFromCenter / 2; // !!! this screen-resolution dependent coordinate was found by trial and error...


            modelLocation = new Vector3(modelX, modelY, 0f);

            perspectiveView = Matrix.CreateLookAt(new Vector3(modelX, modelY - 420, -420f), new Vector3(modelX, modelY, 0f), Vector3.UnitY);



            modelRenderer = new UIComponent(gui);
            //crtContent.Add(modelRenderer);

            modelRenderer.DrawContentEvent += new WindowSystem.DrawContentHandler(ModelRenderer_DrawContentEvent);
            modelRenderer.UpdateEvent += new WindowSystem.UpdateHandler(ModelRenderer_UpdateEvent);
            modelRenderer.Width = crtContent.Width;
            modelRenderer.Height = crtContent.Height;


            /*    TextButton bt = Panel.AddTextButton(Form, new Point(60, 80), TextButton.TextButtonType.Brown, "SWITCH", "");
                bt.Click += new ClickHandler(bt_Click);

                TextButton btN = Panel.AddTextButton(Form, new Point(100, 80), TextButton.TextButtonType.Brown, "NOISE", "");
                btN.Click += new ClickHandler(btN_Click);

                TextButton btNo = Panel.AddTextButton(Form, new Point(60, 110), TextButton.TextButtonType.Brown, "NO RCPT", "");
                btNo.Click += new ClickHandler(btNo_Click);
              */
            /*  heading.Width = 200;
              heading.Height = 24;*/
        }


        private void InitHeading(ref Label lblHeading, ref Label lblSubHeading, ref Label lblProducer, UIComponent pnTemplate)
        {
            lblHeading = new Label(gui);
            //crtHeading.DebugTag = "FindThis";
            pnTemplate.Add(lblHeading);
            lblHeading.Init(Label.LabelType.CRTBigGlow);
            lblHeading.Position = new Point(leftMargin, 35);
            //lblHeading.ID = "CRTHeading"; // mark for animating...
            lblHeading.AnimateOnCRTScreen = Label.AnimationMode.Character;
            lblHeading.Width = 400;

            lblSubHeading = new Label(gui);
            //crtHeading.DebugTag = "FindThis";
            pnTemplate.Add(lblSubHeading);
            lblSubHeading.Init(Label.LabelType.CRTSmall); // CRTNormal);
            lblSubHeading.Position = new Point(leftMargin, 64);
            // lblSubHeading.ID = "CRTHeading"; // mark for animating...
            lblSubHeading.AnimateOnCRTScreen = Label.AnimationMode.Character;
            lblSubHeading.Width = 400;

            underline = new Bar(gui);
            pnTemplate.Add(underline);
            underline.Position = new Point(leftMargin, 88);
            underline.EdgeSize = 6;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutLine");
            underline.SetSkinLocation(SkinState.Normal,rect);
            underline.Width = 290; // crtContent.Width - 2 * leftMargin;
            underline.Height = rect.Height;
            underline.RenderType = RenderType.CRTAndLCD;
            underline.DebugTag = "underline";


            lblProducer = new Label(gui);
            pnTemplate.Add(lblProducer);
            //  lblValue.Text = caption;
            lblProducer.Init(Label.LabelType.CRTSmall);
            lblProducer.Position = new Point(leftMargin, 106);
            lblProducer.Width = 400;
        }

        //private void SetHeading

        private void DisplayHeading(string heading, string summary, string producer)
        {
            lblHeading.Text = heading;

            if (string.IsNullOrEmpty(summary))
            {
                lblSubHeading.Text = "";
                underline.Y = lblHeading.Y + lblHeading.Height + 2;

                underline.Width = lblHeading.Width;
            }
            else
            {
                lblSubHeading.Text = summary;
                underline.Y = lblSubHeading.Y + lblSubHeading.Height + 2;

                underline.Width = lblSubHeading.Width;
            }

            lblProducer.Y = underline.Y + underline.Height; // +4;

            if (string.IsNullOrEmpty(producer))
            {
                lblProducer.Text = "";
                pnHeading.Height = underline.Y + underline.Height + 4;
            }
            else
            {
                lblProducer.Text = producer;
                pnHeading.Height = lblProducer.Y + lblProducer.Height + 4;
            }
        }

        /*
        private void InitCRTHeading(ref Label lblHeading, UIComponent pnTemplate)
        {
            lblHeading = new Label(gui);
            //crtHeading.DebugTag = "FindThis";
            pnTemplate.Add(lblHeading);
            //framedCRT.SurfacePanel.Add(heading);
            lblHeading.Text = "SKIMMER";
            lblHeading.Init(Label.LabelType.CRTBigGlow);
            lblHeading.Position = new Point(leftMargin, 35);
            //  lblHeading.ID = "CRTHeading"; // mark for animating...
            lblHeading.AnimateOnCRTScreen = Label.AnimationMode.Character; // true;
            lblHeading.Width = 400;

            Bar underline = new Bar(gui);
            pnTemplate.Add(underline);
            underline.Position = new Point(leftMargin, 58);
            underline.EdgeSize = 6;
            Rectangle rect = gui.GUISpriteSheet.SourceRectangle("CRT_LayoutLine");
            underline.SetSkinLocation(SkinState.Normal,rect);
            underline.Width = 220; // crtContent.Width - 2 * leftMargin;
            underline.Height = rect.Height;
            underline.RenderType = RenderType.CRTAndLCD;
            underline.DebugTag = "underline";

        }

        private void AddCRTCompanyLabel(UIComponent pnPanel, ref Label lblValue, int yPos) //, ref int yPos)
        {
            lblValue = new Label(gui);
            pnPanel.Add(lblValue);
            //  lblValue.Text = caption;
            lblValue.Init(Label.LabelType.CRTSmall); //CRTNormal);
            lblValue.Position = new Point(leftMargin, yPos);
            lblValue.Width = 400;

            //  yPos += lineHeight;
        }
        */

        void ModelRenderer_UpdateEvent(GameTime gameTime)
        {
            // is this still called when window is hidden????

          /*  if (Interface.Instance.IsCurrentlyDisplayed(this))
            {*/
                if (currentScreenModel != null)
                {
                    FramedCRT.RotateModel(gameTime, currentScreenModel, ref rotation, modelLocation);
                }
           // }
        }


        void ModelRenderer_DrawContentEvent()
        {
          /*  if (Interface.Instance.IsCurrentlyDisplayed(this))
            {*/
                if (currentScreenModel != null)
                {
                    gui.EndSpriteBatch();

                    FramedCRT.DrawModel(currentScreenModel, /*UWGame.SimSide.Instance.AllModels,*/ perspectiveView);

                    gui.BeginSpriteBatch();
                }
           // }
        }

        /// <summary>
        /// Setup the display depending on what is being shown...
        /// </summary>
        public void Refresh(Entity entity)
        {
            if (entity == null)
                return; // on CreateGameScreen, we can be blank...


            //Entity entity = Interface.Instance.SelectedEntity;
            if (framedCRT.crtTextAnimatorCharacter.IsStarted ||
                framedCRT.crtTextAnimatorLine.IsStarted)
            {
                // don't fill in labels while we're animating them!!!
                return;
            }

            // UpdateSkills();
            FramedCRT.ClearContent(crtContent);

            if (entity.Renderable./*TODO DECOUPLE*/RenderAsModel != null && entity.PersonEntity == null)
            {
                screenModels.TryGetValue(entity.EntityType, out currentScreenModel);
                crtContent.Add(modelRenderer);
            }
            else
            {
                currentScreenModel = null;
            }

            crtContent.Add(pnHeading);

            string heading;
            if (entity.PersonEntity != null)
            {
                heading = entity.Name;
            }
            else
            {
                heading = entity.EntityType.FormalName ?? entity.EntityType.Name;
            }

            DisplayHeading(heading.ToUpper(Config.Culture), GetUpperCase(entity.EntityType.SummaryDescription), ""); // GetUpperCase(entity.EntityType.Producer));

            // person
            if (entity.PersonEntity != null)
            {
                
                crtContent.Add(pnPerson);
                pnPerson.Y = pnHeading.Y + pnHeading.Height;

                crtContent.Add(imCRT);
                Rectangle rect = entity.PersonEntity.GetPortraitForEntityPanel(gui); // .portraitRectEntityPanel; //"Construction");
                imCRT.Position = new Point(290, 14);
                imCRT.Texture = gui.GUI_CRT_SpriteSheet.Texture;
                imCRT.SetSkinLocation(SkinState.Normal,rect);
                imCRT.ScaleImageToSizeOfControl = false;
                imCRT.ResizeControlToFitImage();

                crtContent.Add(boxPersonImageBorder);
                boxPersonImageBorder.Position = new Point(imCRT.X - 6, imCRT.Y - 6);
                boxPersonImageBorder.Width = imCRT.Width + 12;
                boxPersonImageBorder.Height = imCRT.Height + 12;
                //imPerson.Alpha = 1f; // 0.35f;// use for background for text!


                //   lblPersonHeading.Text = entity.PersonEntity.Name.ToUpper();
                lblPersonSex.Text = entity.BiologicalEntity.CasteType.Reproduction.ToString().ToUpper(Config.Culture);
                if (entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Adult
                    || entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Old)
                {
                    // TEASER HACK
                   /* if (entityPanel.intf.Game.ScreenManager.Random.Next(2) == 0)
                    {
                        lblPersonFamily.Text = "MARRIED";
                    }
                    else
                    {
                        lblPersonFamily.Text = "SINGLE";
                    }*/
                    // TEASER HACK - removed this:
                    
                    if (entity.BiologicalEntity.Mate != null)
                    {
                        lblPersonFamily.Text = "MARRIED";
                    }
                    else
                    {
                        lblPersonFamily.Text = "SINGLE";
                    }
                }
                else
                {
                    lblPersonFamily.Text = "-";
                }

                lblPersonAge.Text = ((int)entity.BiologicalEntity.AgeGroup.Age).ToString();

                //todo: also display these for other species.
                lblPersonHeight.Text = ((int) (100 * entity.BiologicalEntity.AdultTargetHeight)).ToString() /*ToString("F2")*/ + " CM";
                lblPersonWeight.Text = ((int)entity.BiologicalEntity.AdultTargetWeight).ToString() + " KG";

              
               /* if (entityPanel.intf.Game.ScreenManager.Random.Next(2) == 0)
                {*/
                    lblPersonHealth.Text = "EXCELLENT";
              /*  }
                else
                {
                    lblPersonHealth.Text = "GOOD";
                }*/
               
              //  lblPersonHome.Text = (entity.PersonEntity.Household.Home != null ? entity.PersonEntity.Household.Home.EntityType.Name : "NONE");

               
                // TEASER HACK - remove this:

                SkillType bestSkill;

                //entity.PersonEntity.Skills.OrderByDescending<Skill, SkillType>(

                StringBuilder bestSkills = new StringBuilder("SKILLS: ");
                string delim = "";
                int count = 0;
                foreach (KeyValuePair<SkillType, Skill> kvp in entity.Intelligence.Skills)
                {
                    bestSkills.Append(delim);
                    bestSkills.Append(kvp.Key.Name.ToUpper(Config.Culture));                    

                    delim = ", ";
                    count++;
                    if (count > 1)
                    {
                        break;
                    }
                }
                lblFlavourLine1.Text = bestSkills.ToString();
                lblFlavourLine1.Width = pnPerson.Width - 2 * leftMargin;

                lblFlavourLine2.Text = "WEALTH: 12000c"; // "WEALTH: " + (10000 + entityPanel.intf.Game.ScreenManager.Random.Next(20000)).ToString() + "c";
                lblFlavourLine2.Text += ", CHILDREN: 0"; //", CHILDREN: " + (entityPanel.intf.Game.ScreenManager.Random.Next(3)).ToString();
                lblFlavourLine2.Text += ", CRIMES: NONE"; //, REPUTATION: " + (entityPanel.intf.Game.ScreenManager.Random.Next(1) == 0? "UNKNOWN" : "FAIR");
            }
            else if (entity.Structure != null) //intf.SelectedEntity.EntityType.Renderable.RenderAsBillboardType != null)
            {
                crtContent.Add(pnStructure);
                pnStructure.Y = pnHeading.Y + pnHeading.Height;

                if (entity.Renderable.RenderAsBillboard != null)
                {
                   // crtContent.Add(imCRT);
                 //   SidePanel.DisplayScaledBillboardImage(pnStructure, imCRT, entity.EntityType, 180, true);
                  //  imCRT.Position = new Point(240, 12);

                    // NEW: billboards:
                    crtContent.Add(pnBillboards);
                  
                   /* SidePanel.CreateAndPlaceBillboards(gui, pnBillboards, entity.EntityType, new Vector2(300, 185),
                        220, true);*/

                }
                else
                {
                    // hide the image control, we have a model instead...
                    // crtContent.Remove(imCRT);
                }

                lblStructureCondition.Text = "GOOD";
                //   lblStructureHeading.Text = entity.EntityType.FormalName ?? entity.EntityType.Name.ToUpper();

              
               /* if (entity.EntityType.StructureType.ConstructionInputs.Count > 0)
                {
                    StringBuilder materials = new StringBuilder();
                    string delim = "";
                    foreach (KeyValuePair<EntityType, Buildings.MaterialInput> kvp in entity.EntityType.StructureType.ConstructionInputs)
                    {
                        materials.Append(delim);
                        materials.Append(kvp.Value.Amount);
                        materials.Append(" ");
                        materials.Append(kvp.Key.Name.ToUpper());
                        delim = ", ";
                    }
                    lblStructureMaterialsRequired.Text = materials.ToString(); 
                }
                else
                {
                    lblStructureMaterialsRequired.Text = "N/A";
                }*/

                taStructureDescription.Text = entity.EntityType.Description.ToUpper(Config.Culture);

            }
            else if (entity.Vehicle != null)
            {   // vehicle...

                crtContent.Add(pnVehicle);

                pnVehicle.Y = pnHeading.Y + pnHeading.Height;

                /*   lblVehicleHeading.Text = entity.EntityType.FormalName ?? intf.SelectedEntity.EntityType.Name.ToUpper();

                   lblVehicleSubHeading.Text = entity.EntityType.SummaryDescription ?? intf.SelectedEntity.EntityType.SummaryDescription.ToUpper();

                   lblVehicleProducer.Text = entity.EntityType.Producer.ToUpper();*/

                taVehicleDescription.Text = entity.EntityType.Description.ToUpper(Config.Culture);

                lblVehicleWorkRequired.Text = "2000 MAN HRS";
                lblVehicleCapacity.Text = "3 SEATS / MAX 500 KG";

               
                lblVehicleCondition.Text = "GOOD";

                lblVehicleRange.Text = "600 KM";
                lblVehicleSpeed.Text = "280 KM/H";
                
            }


           // base.Refresh();
        }

        public void Show(bool showFrame)
        {
            if (showFrame)
            {
                framedCRT.TurnOn();
                framedCRT.Show();
            }

            //Now display the fresh data on the CRT screen:
            framedCRT.ChangeContent(crtContent);
        }

        public void Hide()
        {
            framedCRT.TurnOff();
            framedCRT.Hide();
        }

        private string GetUpperCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            else return text.ToUpper(Config.Culture);
        }

    }
}

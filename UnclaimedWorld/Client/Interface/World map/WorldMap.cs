using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Jobs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RoundLineCode;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.SimSide.Overland.Locations;
namespace UWGame.ClientSide.Interface.World_map
{
    /// <summary>
    /// can be hosted inside the WorldMapRoster or the WorldMapDialog
    /// </summary>
    public class WorldMap : UIComponent
    {
        // Image map; // use a static image for now

        Box border;

        Box ruler;

        /// <summary>
        /// only if filled with a different world do we need to redraw between showings of the panel
        /// </summary>
        World world;

        /// <summary>
        /// gets passed to SiteWindow to show goods to buy
        /// </summary>
        EntityGroupID? otherPartyID;
      //  bool isMissionAction;

        double longitudeStart, longitudeEnd, latitudeStart, latitudeEnd;


        private SiteWindow siteWindow;

        public event Action<TravelLocation> TerminalSelected
        {
            // pass through to siteWindow:
            add
            {
                siteWindow.TerminalSelected += value;
            }
            remove
            {
                siteWindow.TerminalSelected -= value;
            }
        }

        private bool gridHasChanged = true;


        #region computed axis
        float yAxisSpacing;
        float xAxisSpacing;

        float minY, minX, maxY, maxX;
        float tickStartY, tickStartX, tickEndY, tickEndX;

        float xInterval, yInterval;

        /// <summary>
        /// the area within the axis
        /// </summary>
        // float chartWidth, chartHeight;


        public Color AxisColor = Color.SlateGray;

        #endregion

        Dictionary<SiteID, SiteMarker> sitesOnMap = new Dictionary<SiteID, SiteMarker>();

        Dictionary<JobID, JobID> missionsOnMap = new Dictionary<JobID, JobID>();

        int canvasWidth, canvasHeight;

        // const int padding = 10;


        /* Label lblYAxisMin;      
         Label lblXAxisMin;*/

        /// <summary>
        /// area outside the canvas, includes the border
        /// </summary>
        const int canvasMarginLeft = 3;
        const int canvasMarginRight = 7;
        const int canvasMarginTop = 11;
        const int canvasMarginBottom = 12;

        public event Action ChildDialogDisplayed;
        public event Action ChildDialogClosed;


        private enum WorldMapLayers { Map, Grid, SiteMarkers, MissionMarkers };
        Dictionary<WorldMapLayers, UIComponent> layersToDraw = new Dictionary<WorldMapLayers, UIComponent>();

        /// <summary>
        /// assign a render target to this. also add controls, to clip them
        /// </summary>
        Image imCanvas;

        RenderTarget2D renderTarget;

        PrimitiveBatch primitiveBatch;

        /// <summary>
        /// for drawing fat grid lines
        /// </summary>
        RoundLineManager RoundLineManager;

        Matrix viewProj;

        List<RoundLine> roundLines = new List<RoundLine>();

        Label.LabelType axisType = WindowSystem.Label.LabelType.LCDNormalDark;

        public WorldMap(UIComponent surface, int canvasWidth, int canvasHeight)
            : base(surface.guiManager)
        {

            surface.Add(this);

            int clampedCanvasWidth = Common.Min(canvasWidth, surface.Width - canvasMarginLeft - canvasMarginRight);
            int clampedCanvasHeight = Common.Min(canvasHeight, surface.Height - canvasMarginTop - canvasMarginBottom);

            this.DebugTag = "worldmap";
            this.RenderType = WindowSystem.RenderType.CRTAndLCD;

            this.canvasWidth = clampedCanvasWidth;
            this.canvasHeight = clampedCanvasHeight;

            this.Width = canvasWidth + canvasMarginLeft + canvasMarginRight; // 2 * padding;
            this.Height = canvasHeight + canvasMarginTop + canvasMarginBottom; // 2 * padding;


            // for drawing lines?
            renderTarget = new RenderTarget2D(guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight); //, false, format, DepthFormat.None);

            primitiveBatch = new PrimitiveBatch(guiManager.ScreenWidth, guiManager.ScreenHeight, guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
            viewProj = primitiveBatch.Projection;

            /*   RoundLineManager = new RoundLineManager();
               RoundLineManager.LoadContent(guiManager.Game.GraphicsDevice, guiManager.Game.Content);
               */



            border = new Box(guiManager);
            Add(border);
            border.CornerSize = 20;
            border.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background")); // is filled
            border.X = canvasMarginLeft - 2;
            border.Y = canvasMarginTop - 2;

            ruler = new Box(guiManager);
           // Add(ruler);
            ruler.CornerSize = 4;
            ruler.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background")); // is filled
            ruler.X = canvasMarginLeft - 2;
            ruler.Y = canvasMarginTop - 2;

            imCanvas = new WindowSystem.Image(guiManager);
            Add(imCanvas);
            imCanvas.Width = canvasWidth;
            imCanvas.Height = canvasHeight;
            imCanvas.X = canvasMarginLeft;
            imCanvas.Y = canvasMarginTop;
            imCanvas.DebugTag = "canvas";

            siteWindow = new World_map.SiteWindow(guiManager, 410, 150);
            siteWindow.ChildDialogDisplayed += siteWindow_ChildWindowDisplayed;
            siteWindow.ChildDialogClosed += siteWindow_ChildWindowClosed;

           // lcdSurface.Add(siteWindow);
           // imCanvas.Add(siteWindow);
            Add(siteWindow);
            siteWindow.Hide();

            /*
            lblYAxisMin = new Label(guiManager);
            Add(lblYAxisMin);
            lblYAxisMin.Init(axisType);
            lblYAxisMin.Text = "0";
            lblYAxisMin.FitToText();

            lblXAxisMin = new Label(guiManager);
            Add(lblXAxisMin);
            lblXAxisMin.Init(axisType);
            lblXAxisMin.Text = "0";
            lblXAxisMin.FitToText();*/
        }

        public override void Destroy()
        {
            base.Destroy();

            renderTarget.Dispose();
        }

     

        void siteWindow_ChildWindowClosed()
        {
            if (ChildDialogClosed != null)
            {
                ChildDialogClosed.Invoke();
            }
        }

        void siteWindow_ChildWindowDisplayed()
        {
            if (ChildDialogDisplayed != null)
            {
                ChildDialogDisplayed.Invoke();
            }
            
        }

        public void Fill(World world, EntityGroupID? otherPartyID, bool isMissionAction)
        {
            this.latitudeStart = world.ViewLatitudeStart;
            this.latitudeEnd = world.ViewLatitudeEnd; // latitudeEnd;
            this.longitudeStart = world.ViewLongitudeStart; // longitudeStart;
            this.longitudeEnd = world.ViewLongitudeEnd; // longitudeEnd;

          
            this.otherPartyID = otherPartyID;
          //  this.isMissionAction = isMissionAction;

            if (this.world != world)
            {
                this.world = world;
            }

            siteWindow.Hide();

            SetRanges();
            
            DrawMap();

            DrawGridAndAxis();
                      

            DrawBorder();

            UpdateChangingData();


            UpdateCanvas();

            /*
            BeginDraw();

          
            primitiveBatch.AddVertex(new Vector2(GetVertexXPosFromValue(5), GetVertexYPosFromValue(75)), Color.Green);
            primitiveBatch.AddVertex(new Vector2(GetVertexXPosFromValue(15),  GetVertexYPosFromValue(75)), Color.Green);

            primitiveBatch.AddVertex(new Vector2(GetVertexXPosFromValueAsInt(5),  GetVertexYPosFromValueAsInt(65)), Color.Green);
            primitiveBatch.AddVertex(new Vector2(GetVertexXPosFromValueAsInt(15),  GetVertexYPosFromValueAsInt(65)), Color.Green);
           

            EndDraw();*/

        }


        public void HideOpenDialogs()
        {
            siteWindow.ResetAfterBuySellDialog();
            siteWindow.ResetAfterPersonnelDialog();

            The.InGameUI.BuySellDialog.Hide();
            The.InGameUI.PersonnelDialog.Hide();
        }

        private void UpdateCanvas()
        {
            foreach (var layer in layersToDraw)
            {
                int indexOfLayer = getLayerIndex(layer.Key);

                if (indexOfLayer > imCanvas.Controls.Count - 1)
                {
                    imCanvas.Add(layer.Value); 
                }
                else
                {                   
                    if (imCanvas.Controls[indexOfLayer] != layer.Value)
                    {
                        imCanvas.Controls[indexOfLayer] = layer.Value;
                    }
                }
            }
        }

        private int getLayerIndex(WorldMapLayers drawingLayers)
        {
            switch (drawingLayers)
            {
                case WorldMapLayers.Map: return 0;
                case WorldMapLayers.Grid: return 1;
                case WorldMapLayers.SiteMarkers: return 2;
                case WorldMapLayers.MissionMarkers: return 3;

                default: return 0;
            }
        }

        private void UpdateChangingData()
        {
            UpdateSites();

            UpdateMissions();
        }

        public void Update()
        {
            UpdateChangingData();

            UpdateCanvas();


            if (siteWindow.Visible == true)
            {
                siteWindow.Refresh();
            }

            if (The.InGameUI.PersonnelDialog.Window.IsVisibleAndActive)
            {
                The.InGameUI.PersonnelDialog.Refresh();
            }

            if (The.InGameUI.BuySellDialog.Window.IsVisibleAndActive)
            {
                The.InGameUI.BuySellDialog.Refresh();
            }
        }

        private Dictionary<MissionID, Image> missionMarkers = new Dictionary<MissionID, WindowSystem.Image>();

        private void UpdateMissions()
        {
            foreach (var site in The.Sim.World.AllSites)
            {
                foreach (var allegiance in site.Value.Allegiances)
                {
                    if (allegiance.Missions != null)
                    {
                        foreach (var mission in allegiance.Missions)
                        {

                            Image missionMarker;
                            if (!missionMarkers.TryGetValue(mission.ID, out missionMarker))
                            {
                                missionMarker = AddMissionMarker(mission);
                            }

                            UpdateMissionMarker(mission, missionMarker);
                        }                       
                    }
                }                
            }

            DeleteMissionMarkers();           
        }

        private void DeleteMissionMarkers()
        {
           /* for (int i = missionMarkers.Count - 1; i >= 0; i--)
            {
                MissionID id = missionMarkers[i].
            }*/

            List<MissionID> missionMarkersToRemove = null;
            foreach (var item in missionMarkers)
            {
                if (LookUp<Mission, MissionID>.FindByID(item.Key) == null)
                {
                    Common.AddToList(ref missionMarkersToRemove, item.Key);
                }
            }

            if (missionMarkersToRemove != null)
            {
                foreach (var item in missionMarkersToRemove)
                {
                    RemoveMissionMarker(item);
                }                
            }
        }

        Color missionColor = Color.Orange;
        private void UpdateMissionMarker(Mission mission, Image missionMarker)
        {
            Allegiance fromAllegiance = The.InGameUI.UIAllegiance;
            CommunicationMethod? method;
            bool canCom = Communicates.IsInCommunicationRange(fromAllegiance, mission, out method);

           int indexOfMissionMarker =  layersToDraw[WorldMapLayers.MissionMarkers].Controls.IndexOf(missionMarker);

            if (canCom)
            {
                missionMarker.Y = GetVertexYPosFromValueAsInt((float)mission.Coords.Value.Latitude) - missionMarker.Height / 2;
                missionMarker.X = GetVertexXPosFromValueAsInt((float)mission.Coords.Value.Longitude) - missionMarker.Width / 2;

                missionMarker.SetSkinLocation(SkinState.Normal,null, missionColor, missionColor);

                missionMarker.ToolTip = mission.GetTooltip();
            }
            else
            {
                missionMarker.SetSkinLocation(SkinState.Normal,null, Color.Gray, Color.Gray);

                missionMarker.ToolTip = "No communication";
            }

           /* layersToDraw[WorldMapLayers.MissionMarker].X = missionMarker.X;
            layersToDraw[WorldMapLayers.MissionMarker].Y = missionMarker.Y;

            missionMarker.X = 0;
             missionMarker.Y = 0;*/

           // layersToDraw[WorldMapLayers.MissionMarker].Controls[indexOfMissionMarker] = missionMarker;
      
        }


        private Image AddMissionMarker(Mission mission)
        {
            string spriteName = mission.GetMissionMarker();


            Image missionMarker = new Image(guiManager);
            missionMarker.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle(spriteName), Color.Orange, Color.Orange);  //  //mp wanted to change color of transport icon on map  but didnt work Color.Cyan, Color.Cyan
            missionMarker.ResizeControlToFitImage();
            missionMarker.Visible = true;
           // AddForegroundSprite(missionMarker);          
            missionMarkers.Add(mission.ID, missionMarker);
            missionMarker.Visible = true;

            missionMarker.ToolTip = mission.GetTooltip();

            if (!layersToDraw.ContainsKey(WorldMapLayers.MissionMarkers))
            {
                UIComponent layer = new UIComponent(guiManager) { Height = this.Height, Width = this.Width };
                layer.CanHaveFocus = false;
                layersToDraw.Add(WorldMapLayers.MissionMarkers, layer);
               // layersToDraw.Add(WorldMapLayers.MissionMarker, new UIComponent(guiManager) { Height = missionMarker.Height+5, Width = missionMarker.Width+5 });
            }
            layersToDraw[WorldMapLayers.MissionMarkers].Add(missionMarker);
           

            return missionMarker;
        }

        private void AddForegroundSprite(UIComponent component)
        {
         //   imCanvas.Add(component);           
        }

        private void RemoveForegroundSprite(UIComponent component)
        {
           // imCanvas.Remove(component);
        }

        private void RemoveMissionMarker(MissionID id)
        {
            Image image = missionMarkers[id];            
            RemoveForegroundSprite(image);
            layersToDraw[WorldMapLayers.MissionMarkers].Controls.Remove(image);
            missionMarkers.Remove(id);
        }

        //  Texture2D worldMapTexture;
        /*  public void LoadContent()
          {
              SimSide.Scenarios.Scenario scenario = The.Sim.StartGameParams.StartScenarioParams.Scenario;
           
              worldMapTexture = guiManager.ContentManager.Load<Texture2D>(scenario.ScenarioData.WorldMapImage);
           

          }*/

        private void DrawMap()
        {
            if (imCanvas.FindChildById(UIComponent.DataControlID.MapTexture) == null)
            {
                Image mapImage = new Image(guiManager);
                if (The.Sim.StartGameParams.StartScenarioParams != null)
                {
                    SimSide.Scenarios.Scenario scenario = The.Sim.StartGameParams.StartScenarioParams.Scenario;

                    if (scenario.ScenarioData.WorldMapTexture != null)
                    {
                        mapImage.Texture = scenario.ScenarioData.WorldMapTexture; // worldMapTexture; // guiManager.ContentManager.Load<Texture2D>(scenario.ScenarioData.WorldMapImage);
                    }
                }

                mapImage.ID = DataControlID.MapTexture;
              //  imCanvas.Add(mapImage);
                mapImage.Visible = true;
                mapImage.ResizeControlToFitImage();
                mapImage.X = 0;
                mapImage.Y = 0;


                layersToDraw.Add(WorldMapLayers.Map, mapImage);
            }           

        }

        private void DrawBorder()
        {
            border.Width = canvasWidth + 4;
            border.Height = canvasHeight + 4;

            /*
            primitiveBatch.Begin(PrimitiveType.LineStrip);

            // draw a box around the chart area:
            primitiveBatch.AddVertex(new Vector2(canvasMarginLeft, canvasMarginTop), AxisColor);
            primitiveBatch.AddVertex(new Vector2(canvasMarginLeft, canvasMarginTop + chartHeight), AxisColor);
            primitiveBatch.AddVertex(new Vector2(Width - canvasMarginRight, canvasMarginTop + chartHeight), AxisColor);
            primitiveBatch.AddVertex(new Vector2(Width - canvasMarginRight, canvasMarginTop), AxisColor);
            primitiveBatch.AddVertex(new Vector2(canvasMarginLeft, canvasMarginTop), AxisColor);


            primitiveBatch.End();
            */


            /*
            if (roundLines.Count > 0)
            {

                RoundLineManager.BlurThreshold = RoundLineManager.ComputeBlurThreshold(lineRadius, viewProj, // viewProjMatrix,
                    imCanvas.Width); //The.Client.GraphicsDevice.PresentationParameters.BackBufferWidth);

                float time = 0f; // (float)The.Sim.GameTime.TotalGameTime.TotalSeconds;

                RoundLineManager.Draw(roundLines, lineRadius, plotColor, viewProj, time, roundLineTechniqueName);
            }
            else if (previousPlotPoint.HasValue)
            {
                Disc disc = new Disc(previousPlotPoint.Value);

                // draw a single point:
                RoundLineManager.Draw(disc, lineRadius, plotColor, viewProj, 0f, roundLineTechniqueName);
            }*/

        }

        private void DrawRuler(UIComponent gridLayer)
        {
            // Warning: Distances on the map are distorted, this is impossible to prevent...

            // choose a length that fits less than 50% of the width and has a nice round number
            // make a vertical ruler as well?
            float mapWidthInKilometers = (float)The.Sim.World.GetAirDistance(new GeodeticCoordinate(minX, minY), new GeodeticCoordinate(maxX, minY));

            double tickSpacing;
            float min, max;
            NiceScale numScale = new NiceScale(0f, 0.5f * mapWidthInKilometers);

            tickSpacing = (float)numScale.tickSpacing;
            min = (float)numScale.niceMin;
            max = (float)numScale.niceMax;

            int rulerWidth = (int)((max / mapWidthInKilometers) * canvasWidth);

            ruler.Width = rulerWidth;
            ruler.Height = 9;

            ruler.X = canvasWidth - ruler.Width + 2;
            ruler.Y = -2;

            gridLayer.Add(ruler);

            int rulerLabelYPos = ruler.Bottom;

            // add label:
            Label lblRuler = new Label(guiManager);
            gridLayer.Add(lblRuler);
            gridLineLabels.Add(lblRuler);
            lblRuler.Init(axisType);
            lblRuler.Text = GetRulerLabelText(0);
            lblRuler.FitToText();
            lblRuler.Y = rulerLabelYPos;
            lblRuler.X = ruler.X;

            lblRuler = new Label(guiManager);
            gridLayer.Add(lblRuler);
            gridLineLabels.Add(lblRuler);
            lblRuler.Init(axisType);
            lblRuler.Text = GetRulerLabelText(max);
            lblRuler.FitToText();
            lblRuler.Y = rulerLabelYPos;
            lblRuler.X = ruler.Right - lblRuler.Width;
        }

        /// <summary>
        /// selects an even number for the maximum and for the division lines
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
      /*  private void ChooseRulerDivision(float max, float min, out double tickSpacing, out float min, out float max) 
        {
            NiceScale numScale = new NiceScale(min, max);

            tickSpacing = (float)numScale.tickSpacing;
            min = (float)numScale.niceMin;
            max = (float)numScale.niceMax;
        }*/

        private void DrawGridAndAxis()
        {
            if (gridHasChanged)
            {               
                UIComponent gridLayer;

                if (layersToDraw.TryGetValue(WorldMapLayers.Grid, out gridLayer))
                {
                    gridLayer.Controls.Clear();
                }
                else
                {
                    gridLayer = new UIComponent(guiManager) { Height = this.Height, Width = this.Width };
                    layersToDraw.Add(WorldMapLayers.Grid, gridLayer);
                }

                gridLines.Clear();
                gridLineLabels.Clear();

                // y- axis:
                float tickPos = tickStartY;

                int yAxisLblXPos = canvasMarginLeft + 2;

                int? lastLabelPos = null; 

                int minDistanceBetweenLabels = 166;

                int linePos;
                while (tickPos < tickEndY)
                {
                    Bar horizGridline = new WindowSystem.Bar(guiManager);
                    horizGridline.EdgeSize = 1;
                    horizGridline.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("gridline_horiz")); //, new Color(0.2f, 0.2f, 0.2f, 0.2f));
                    gridLines.Add(horizGridline);
                 //   imCanvas.Add(horizGridline); 
                    horizGridline.Height = 3;
                    horizGridline.Width = canvasWidth;
                    horizGridline.X = 0;
                    linePos = GetVertexYPosFromValueAsInt(tickPos);
                    horizGridline.CenterThisVertically(linePos);

                    gridLayer.Add(horizGridline);

                    if (canvasHeight - linePos > 20
                        && (lastLabelPos == null || Math.Abs(linePos - lastLabelPos.Value) > minDistanceBetweenLabels))
                    {
                        // add label:
                        Label lblTick = new Label(guiManager);
                        Add(lblTick);
                        gridLineLabels.Add(lblTick);
                        lblTick.Init(axisType);
                        lblTick.Text = GetTickLabelText(tickPos);
                        lblTick.FitToText();
                        lastLabelPos = GetVertexYPosFromValueAsInt(tickPos, true);
                        lblTick.CenterThisVertically(lastLabelPos.Value);
                        lblTick.X = yAxisLblXPos;
                                             
                    }

                    tickPos += yAxisSpacing;
                }

                // x-axis:
                int xAxisLabelYPos = Height - 27;

                tickPos = tickStartX;
                
                lastLabelPos = null;
                while (tickPos < tickEndX)
                {
                    Bar vertGridline = new WindowSystem.Bar(guiManager);
                    vertGridline.EdgeSize = 1;
                    vertGridline.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("gridline_vert")); //, new Color(0.2f, 0.2f, 0.2f, 0.2f));
                    gridLines.Add(vertGridline);
                  //  imCanvas.Add(vertGridline); //?
                    vertGridline.IsVertical = true;
                    vertGridline.Width = 3;
                    vertGridline.Height = canvasHeight;
                    vertGridline.Y = 0;
                    linePos = GetVertexXPosFromValueAsInt(tickPos);
                    vertGridline.CenterThisHorizontally(linePos);

                    gridLayer.Add(vertGridline);

                    if (linePos > 20
                        && (lastLabelPos == null //((lastLabelPos == null && System.Math.Round(tickPos) == tickPos)
                        || Math.Abs(linePos - lastLabelPos.Value) > minDistanceBetweenLabels))
                    {
                        // add label:
                        Label lblTick = new Label(guiManager);
                        Add(lblTick);
                        gridLineLabels.Add(lblTick);
                        lblTick.Init(axisType);
                        lblTick.Text = GetTickLabelText(tickPos);
                        lblTick.FitToText();
                        lastLabelPos = GetVertexXPosFromValueAsInt(tickPos, true);
                        lblTick.CenterThisHorizontally(lastLabelPos.Value);
                        lblTick.Y = xAxisLabelYPos;
                        lblTick.X = vertGridline.X + vertGridline.Width + 2;

                      
                    }

                    tickPos += xAxisSpacing;
                }

                DrawRuler(gridLayer);

                gridHasChanged = false;
            }
        }

        private SiteMarker AddSite(UIComponent layer, Site site)
        {
            SiteMarker.MarkerType markerType = SiteMarker.MarkerType.Short;
            if (site.ShowTallPin)
            {
                markerType = SiteMarker.MarkerType.Tall;
            }
            SiteMarker siteMarker = new SiteMarker(guiManager, site, The.Sim.PlaySite, btSite_Click, markerType, site.SiteMarkerOrder);


            siteMarker.X = GetVertexXPosFromValueAsInt((float)site.Coords.Longitude) - 29; // 40; //

            // OLD, round marker:
            //siteMarker.Y = GetVertexYPosFromValueAsInt((float)site.Coords.Latitude) - 19;

            int yOffset;
            if (markerType == SiteMarker.MarkerType.Tall)
            {
                yOffset = -42;                
            }
            else
            {
                yOffset = -31;
            }

            siteMarker.Y = GetVertexYPosFromValueAsInt((float)site.Coords.Latitude) + yOffset;

           // AddForegroundSprite(siteMarker);
            sitesOnMap[site.ID] = siteMarker;


            layer.Add(siteMarker);

            return siteMarker;
        }
        
        private void UpdateSite(SiteMarker siteMarker, Site site)
        {                    
            
            CommunicationMethod? method;

            Allegiance fromAllegiance = The.InGameUI.UIAllegiance;
            bool canCommunicate = false;

        //    int indexOfSiteMarker = layersToDraw[WorldMapLayers.SiteMarkers].Controls.IndexOf(siteMarker);

            foreach (var allegiance in site.Allegiances)
            {
                if (allegiance.RepresentativeEntityType.Person != null) // only list human allegiances...
                {
                    if (Communicates.IsInCommunicationRange(fromAllegiance, allegiance, out method))
                    {
                        canCommunicate = true;
                        break;
                    }                    
                }
            }          
          
            bool isStart = The.InGameUI.CreateMissionPanel.start != null && site.ID == (SiteID)The.InGameUI.CreateMissionPanel.start.Value.SiteID;
            bool isDestination = The.InGameUI.CreateMissionPanel.destination != null && site.ID == (SiteID)The.InGameUI.CreateMissionPanel.destination.Value.SiteID;
          
            siteMarker.UpdateMarker(canCommunicate, isStart, isDestination, site.Name, site.ShowLabel);

          //  layersToDraw[WorldMapLayers.SiteMarkers].Controls[indexOfSiteMarker] = siteMarker;
        }

        public void UpdateSites()
        {
            UIComponent layer;
            if (!layersToDraw.TryGetValue(WorldMapLayers.SiteMarkers, out layer)) //. ContainsKey(WorldMapLayers.SiteMarker))
            {
                layer = new UIComponent(guiManager) { Height = this.Height, Width = this.Width };
                layersToDraw.Add(WorldMapLayers.SiteMarkers, layer);
            }

            bool sitesWereAddedOrRemoved = false;
            SiteMarker siteMarker;
            foreach (var item in world.AllSites)
            {
                if (!sitesOnMap.TryGetValue(item.Value.ID, out siteMarker))
                {
                    siteMarker = AddSite(layer, item.Value);
                    sitesWereAddedOrRemoved = true;
                }

                UpdateSite(siteMarker, item.Value);
            }                       

            // TODO: cleanup sites

            if (sitesWereAddedOrRemoved)
            {
                // sort the site markers to better manage overlap
                layer.SortControls(c => (int)c.OrderByTag1);
            }
        }


        #region vertices

        public string GetSiteName(SiteID siteID)
        {
            return sitesOnMap[siteID].lblName.Text;
        }

        /// <summary>
        /// selects an even number for the maximum and for the division lines
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        private void ChooseAxisDivision(float min, float max, out float tickSpacing, out float niceMin, out float niceMax) //, out float niceMin, out float niceMax, out float tickSpacing)
        {
            NiceScale numScale = new NiceScale(min, max);

            tickSpacing = (float)numScale.tickSpacing;

            niceMin = (float)numScale.niceMin;
            niceMax = (float)numScale.niceMax;
        }

        public void DrawFatLine(float x, float y, float toX, float toY) //, Color color)
        {
            /*
            x = Math.Max(x, 0f); // clamp bottom...
            y = Math.Max(y, 0f);
            */

            float xPos;
            float yPos;
            GetVertexPosFromValues(toX, toY, out xPos, out yPos);
            Vector2 toPoint = new Vector2(xPos, yPos);

            GetVertexPosFromValues(x, y, out xPos, out yPos);
            Vector2 fromPoint = new Vector2(xPos, yPos);

            // old: Draws thin lines:
            //  AddVertex((float)xPos, (float)yPos, color);

            roundLines.Add(new RoundLine(fromPoint, toPoint)); // xPos, yPos, xPos, yPos));     
        }

        /// <summary>
        /// clears the render target
        /// </summary>
        public void BeginDraw()
        {
            guiManager.Game.GraphicsDevice.SetRenderTarget(renderTarget);
            guiManager.Game.GraphicsDevice.Clear(Color.Transparent);

            primitiveBatch.Begin(PrimitiveType.LineStrip);

        }

        public void EndDraw(RenderTarget2D previousRenderTarget = null)
        {
            primitiveBatch.End();

            roundLines.Clear();

            guiManager.Game.GraphicsDevice.SetRenderTarget(previousRenderTarget);

            imCanvas.Texture = renderTarget; //??
        }

        private void GetVertexPosFromValues(float x, float y, out float xPos, out float yPos)
        {
            xPos = GetVertexXPosFromValue(x);
            yPos = GetVertexYPosFromValue(y);
        }

        /// <summary>
        /// converts longitude to an x position on the canvas (inside the border)
        /// </summary>
        /// <param name="longitudeXpos"></param>
        /// <returns></returns>
        private float GetVertexXPosFromValue(float longitudeXpos)
        {
            float xPos;
            xPos = ((longitudeXpos - minX) / xInterval) * canvasWidth; // chartWidth;
            return xPos;
        }

        private int GetVertexXPosFromValueAsInt(float longitudeXpos, bool addMargin = false)
        {
            int value = (int)Math.Round(GetVertexXPosFromValue(longitudeXpos), MidpointRounding.AwayFromZero);

            if (addMargin)
            {
                value += canvasMarginTop;
            }

            return value;
        }

        private int GetVertexYPosFromValueAsInt(float latitudeYPos, bool addMargin = false)
        {
            int value = (int)Math.Round(GetVertexYPosFromValue(latitudeYPos), MidpointRounding.AwayFromZero);

            if (addMargin)
            {
                value += canvasMarginLeft;
            }

            return value;
        }

        /// <summary>
        /// converts latitude to a y position on the canvas (inside the border)
        /// </summary>
        /// <param name="longitudeXpos"></param>
        /// <returns></returns>
        private float GetVertexYPosFromValue(float latitudeYPos)
        {
            float yPos;
            yPos = ((latitudeYPos - minY) / yInterval) * canvasHeight; //* chartHeight;
            yPos = canvasHeight - yPos; // invert and shift
            return yPos;
        }

        private void SetRanges() 
        {
            
            // select a nice division for the scales:
           // float tempYAxisSpacing, tempXAxisSpacing;
            ChooseAxisDivision((float)latitudeStart, (float)latitudeEnd, out yAxisSpacing, out tickStartY, out tickEndY);

            ChooseAxisDivision((float)longitudeStart, (float)longitudeEnd, out xAxisSpacing, out tickStartX, out tickEndX);


            // keep the endpoints:
            minY = (float)latitudeStart;
            maxY = (float)latitudeEnd;

            minX = (float)longitudeStart;
            maxX = (float)longitudeEnd;



            xInterval = this.maxX - this.minX;
            yInterval = this.maxY - this.minY;

            /* lblYAxisMin.Text = "0";         
             lblYAxisMin.FitToText();
             PositionYAxisMinLabel();
             */
            // SetXAxisLabels(xMinLabel, xMaxLabel);

        }

        List<Bar> gridLines = new List<WindowSystem.Bar>();
        List<Label> gridLineLabels = new List<Label>();

        private string GetRulerLabelText(float tickPos)
        {
            return tickPos.ToString("N0") + "KM";
        }

        private string GetTickLabelText(float tickPos)
        {
            // this removes trailing zeroes, but allows decimals:
            return tickPos.ToString("G6") + "°"; // return tickPos.ToString("G29") + "°"; //tickPos.ToString("N1") + "°";
        }
        #endregion


        public void UnCheckSiteMarkerButtons()
        {
            foreach (var sitemarker in sitesOnMap)
            {
                sitemarker.Value.btSite.IsChecked = false;
            }
        }

        private void SiteWindow_MouseOut(InputEventSystem.MouseEventArgs args)
        {
            siteWindow.Visible = false;
        }

        void btSite_Click(WindowSystem.UIComponent sender, EventArgs e)
        {
            siteWindow.Fill((Site)sender.Tag1, otherPartyID); //, isMissionAction);      
            PlaceSiteWindow(sender);
            siteWindow.Show();
        }

        void PlaceSiteWindow(UIComponent itemButton)
        {
            siteWindow.Position = itemButton.Parent.Position;
            siteWindow.X = siteWindow.X - siteWindow.Width;

            //Correct if its outside of the window
            if (siteWindow.X < 0)
            {
                siteWindow.X = 0;
            }

            if (siteWindow.X + siteWindow.Width > Width)
            {
                siteWindow.X = Width - siteWindow.Width;
            }

            if (siteWindow.Y < 0)
            {
                siteWindow.Y = 0;
            }

            if (siteWindow.Y + siteWindow.Height > Height)
            {
                siteWindow.Y = Height - siteWindow.Height - 60;
            }
        }
    }

    class SiteMarker : UIComponent
    {
        public ImageButton btSite;
        //public Image imSite;

        public Label lblName;

        public Image imRadio;

        public enum MarkerType { Tall, Short }

        public SiteMarker(GUIManager gui, Site site, Site playSite, ClickHandler siteClickAction, MarkerType markerType, int order)
            : base(gui)
        {
            this.RenderType = WindowSystem.RenderType.CRTAndLCD;
            this.Width = 190;
            //this.Height = 45; // 30; // 38;

            this.OrderByTag1 = order;

            ImageButtonType buttonType; 
            int yPos = 0;
            if (markerType == MarkerType.Short)
            {
              //  yPos = 18;
                this.Height = 33; // 30; // 38;
                buttonType = ImageButtonType.SiteMarkerShortPin;
            }
            else
            {
              //  yPos = 0;
                this.Height = 45;

                buttonType = ImageButtonType.SiteMarkerTallPin;
            }

            double distance;
            if (site != playSite)
            {
                distance = The.Sim.World.GetAirDistance(playSite.Coords, site.Coords);
            }
            else
            {
                distance = 0;
            }

            StringBuilder tooltip = new StringBuilder();
            Common.AppendLine(tooltip, site.Name);
            Common.AppendDivider(tooltip);
            tooltip.Append("Distance: ");
            Common.AppendLine(tooltip, string.Format("{0:N1} km", distance));

            btSite = new ImageButton(guiManager);
            if (site.Allegiances.Contains(The.InGameUI.UIAllegiance))
            {
                Common.AppendLine(tooltip, "This is where we are.");
                Common.AppendLine(tooltip, "Click to view options and details.");

                btSite.InitWithIcon(buttonType, "HUD_icon_structure", false);
                btSite.SetIconTooltip(tooltip.ToString()); 
                btSite.SetIconTint(Color.LightGreen);
                btSite.Tag1 = site;
                btSite.SetIconClick(siteClickAction);
            }
            else
            {
                Common.AppendLine(tooltip, "Click to view options and details.");

                btSite.Init(buttonType);
                btSite.Tag1 = site;
            }

            

            btSite.ToolTip = tooltip.ToString(); 
            Add(btSite);
            btSite.X = 17; // 25;
            btSite.Y = yPos;
            btSite.Click += siteClickAction;
            // btSite.TintButtonSkin(Common.ColorFromHex("#000000"));


            imRadio = new Image(guiManager);
            imRadio.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("antenna_icon"), Color.Yellow, Color.Yellow);
            imRadio.ResizeControlToFitImage();
            Add(imRadio);
            imRadio.X = 0;
            imRadio.Y = yPos;

            lblName = new Label(guiManager);
            Add(lblName);
            lblName.Init(Label.LabelType.LCDRadioBannerTinted);
            lblName.TintLabelBackground(Common.ColorFromHex("#6E8284"));
            lblName.X = btSite.Right - 2; // 46;
            lblName.Y = yPos +4;

        }

        public void UpdateMarker(bool canCommunicate, bool isStart, bool isDestination, string siteName, bool showLabel)
        {
            btSite.Enabled = true;
           
            string toolTip;
            Color color;

            if (canCommunicate)
            {
                toolTip = "Communication is established with this location."; //mp was: allegiance    too technical
                color = Color.Yellow;
            }
            else
            {
                toolTip = "No communication with this location. Both locations need a functioning radio or satellite station.";
                color = Color.Red;
            }

            imRadio.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("antenna_icon"), color, color);
            imRadio.ResizeControlToFitImage();
            imRadio.ToolTip = toolTip;
            
            if (isStart)
            {
                lblName.TintLabelBackground(Common.ColorFromHex("#3B6C75"));
            }
            else if (isDestination)
            {
                lblName.TintLabelBackground(Common.ColorFromHex("#735538"));
            }
            else
            {
                lblName.TintLabelBackground(Common.ColorFromHex("#6E8284"));
            }

            lblName.Text = siteName;
            lblName.FitToText();

            lblName.Visible = showLabel;

           // OrderByTag1 = 2;

        }
    }

   
}

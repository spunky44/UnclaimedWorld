using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using WindowSystem;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Collisions;
using GameStateManagement;
using UWGame.SimSide.Entities.Containers;
using UWGame.Control;
using UWGame.SimSide.Entities.Locomotors;
using InputEventSystem;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Commands;
using UWGame.Control.Commands;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide
{
    /// <summary>
    /// this class contains code that has to do with the user's view of the map and interaction with it (scrolling, clicking, hovering etc). The GUI elements are in InGameUI
    /// </summary>
    public class MapClient
    {
        private ButtonState leftMouseButtonStateInMap = ButtonState.Released;
        private ButtonState rightMouseButtonStateInMap = ButtonState.Released;

        public enum ButtonState
        {
            MouseOver,
            MouseOut,
            Pressed,
            Released
        }

        InputData inputData;

        private Point previousMouseScreenPosition;
        private Vector3? previousMouseWorldLocation = null;
        private TilePos? previousMouseTilePosition = null; // new Point(-1, -1);
        private SubtilePos? previousMouseSubtilePosition = null;
        

        private Point? startLeftDragMouseScreenPosition = null;
        private SubtilePos? startLeftDragMouseSubtilePosition = null;
        private TilePos? startLeftDragMouseTilePosition = null;
       
        // private Point? startRightDragMouseTilePosition = null;
        private bool hasRightDragged = false;
        private Point? lastRightDragMousePosition = null;


        Point mapWindowTilePosClamped;
        /// <summary>
        /// Map coordinates for upper left corner
        /// recalculate when MapWindowWorldPos is calculated
        /// </summary>
        public int mapWindowTileX
        {
            get { return mapWindowTilePosClamped.X; }
        }

        public int mapWindowTileY
        {
            get { return mapWindowTilePosClamped.Y; }
        }

        public event Action<Vector3> LeftMouseDownInMap;
        public event Action<Vector3> LeftMouseReleasedInMap; //MouseClickedInMap

        public event Action<Vector3> LeftMouseWorldPosDragInMap;

        /// <summary>
        /// fires when a new subtile is dragged over
        /// </summary>
        public event Action<SubtilePos> LeftMouseSubtileDragInMap;
        public event Action<TilePos> LeftMouseTileDragInMap;
       

        public int mapWindowWidth;
        public int mapWindowHeight;



        // How many tiles should we display at a time
        public int noOfTilesToDisplayHorizontally = 22; // 32; //
        public int noOfTilesToDisplayVertically = 19; // 22; //

        public int noOfTilesLeftOfDetailsInterface = 10;
        public int widthOfWindowLeftOfDetailsInterface;

        // How far from the Upper Left corner of the display do we want our map to start
        public int iMapDisplayOffsetX = 0;
        public int iMapDisplayOffsetY = 0;

        private Vector2 mapWindowWorldPosition = new Vector2();

        public Vector2 MapWindowWorldPosition
        {
            get
            {
                return mapWindowWorldPosition;
            }
            set
            {
                if (mapWindowWorldPosition != value)
                {
                    mapWindowWorldPosition = value;
                    
                    mapWindowTilePosClamped = MapManager.WorldPosToTile(mapWindowWorldPosition);

                    //Clamp TileMap so that we can't actually exit the actual map
                    ClampTileMapPosition();

                    if (The.InGameUI.Minimap != null)
                    {
                        The.InGameUI.Minimap.SetFrameDirty();
                    }
                }
            }
        }

        /// <summary>
        /// the screen rect in world coords
        /// </summary>
        public System.Drawing.RectangleF ScreenRect
        {
            get
            {
                System.Drawing.RectangleF screenRect = new System.Drawing.RectangleF(MapWindowWorldPosition.X, MapWindowWorldPosition.Y, mapWindowWidth, mapWindowHeight);
                return screenRect;
            }
        }

        Vector2 oldMapWindowWorldPosition;

        private bool isScrolling = false;
        public bool IsScrolling
        {
            get { return isScrolling; }
        }

        #region Debug rendering

        public List<object> Overlays = new List<object>();

        public List<ResourceType> ResourceOverlays = new List<ResourceType>();

        #endregion


        public bool RenderedTerrainIsDirty = true;

        private static Common.Direction[,] mouseToDirection = new Common.Direction[4, 4] { {Common.Direction.NorthWest, Common.Direction.West, Common.Direction.West, Common.Direction.SouthWest }, 
                                                                                   /*x=1*/  {Common.Direction.North, Common.Direction.North, Common.Direction.South, Common.Direction.South}, 
                                                                                   /*x=2*/  {Common.Direction.North, Common.Direction.North, Common.Direction.South, Common.Direction.South}, 
                                                                                            {Common.Direction.NorthEast, Common.Direction.East, Common.Direction.East, Common.Direction.SouthEast} };

        /// <summary>
        /// Tile position of mouse.
        /// </summary>
        public TilePos MouseTilePosition = new TilePos(-1, -1);
        public SubtilePos MouseSubtilePosition;
        public Vector3 MouseWorldLocation;

        public Common.Direction MouseMapDirection = Common.Direction.North;


        public MapClient()
        {

            // build map and interface based on screen resolution:
            //  mapWindowWidth = game.ScreenManager.Game.graphics.PreferredBackBufferWidth - Interface.InGameInterface.InterfaceWidth;
            mapWindowWidth = The.Sim.Controller.DrawArea.Width; // .GraphicsDeviceManager.PreferredBackBufferWidth;
            mapWindowHeight = The.Sim.Controller.DrawArea.Height; // .Game.GraphicsDeviceManager.PreferredBackBufferHeight;

            inputData = The.Sim.Controller.InputData;

            //   noOfTilesToDisplayHorizontally = (int)Math.Ceiling((decimal)mapWindowWidth / (decimal)tileSize) + 1; // add one to draw under the right side interface
            noOfTilesToDisplayHorizontally = (int)Math.Ceiling((decimal)mapWindowWidth / (decimal)MapManager.tileSize);
            noOfTilesToDisplayVertically = (int)Math.Ceiling((decimal)mapWindowHeight / (decimal)MapManager.tileSize);

           /* noOfTilesToDisplayHorizontally = Common.ClampTop(noOfTilesToDisplayHorizontally, The.Map.mapTileWidth);
            noOfTilesToDisplayVertically = Common.ClampTop(noOfTilesToDisplayVertically, The.Map.mapTileHeight);
            */

            bullets = new List<Bullet>();
            VisitorMarkers = new Dictionary<string, Marker>();
        }

         /// <summary>
        /// Init stuff that needs the dimensions of the map.
        /// </summary>
        public void SetSize()
        {
            noOfTilesToDisplayHorizontally = Common.ClampTop(noOfTilesToDisplayHorizontally, The.Map.mapTileWidth);
            noOfTilesToDisplayVertically = Common.ClampTop(noOfTilesToDisplayVertically, The.Map.mapTileHeight);

        }

        public void Destroy()
        {
            inputData = null;
        }

        /// <summary>
        /// Sets current map tile position of mouse. Also checks for mouse click in map on models etc.
        /// </summary>
        /// 
        //MouseState mouseState2;
        public void TryMapScrolling()
        {
            if (isRightScrolling == true && inputData.RightButtonDown)
            {

                TryMouseDragForMapScrolling(inputData);

            }
            else
            {
                isRightScrolling = false;
            }

            previousMouseScreenPosition.X = inputData.mouseX;
            previousMouseScreenPosition.Y = inputData.mouseY;
        }

        private bool isRightScrolling = false;
        private void TryMouseDragForMapScrolling(InputData inputData)
        {
            if (rightMouseButtonStateInMap != ButtonState.Pressed) // the mouse was just pressed down
            {
                rightMouseButtonStateInMap = ButtonState.Pressed;

                //   HandleMousePressInTile(MouseTilePosition);

                lastRightDragMousePosition = new Point(inputData.mouseX, inputData.mouseY);// store this pos if the user starts dragging

            }
            else
            {
                // button was down already. handle dragging 

                if (inputData.mouseX != previousMouseScreenPosition.X || inputData.mouseY != previousMouseScreenPosition.Y)
                {
                    isRightScrolling = true;
                    HandleRightMouseDrag();
                }
            }
        }

        bool allowLeftDraggingToStart = true;
        public void UpdateMouseInMap()
        {
            // perform mouse interaction with the map.
            // map selection/dragging is allowed to pass over the GUI, but it can never start there.

            if (The.Client.IsModal)
                return;

            if (inputData.LeftButtonDown == false)
            {
                allowLeftDraggingToStart = true; // reset after mouse is released
            }

            if (The.InGameUI.gui.IsMouseInInterface(inputData.mouseX, inputData.mouseY))
            {
                // if the mouse is pressed down over the gui/windows, don't allow dragging to start on the map when it is moved out of the window (this is annoying when resizing a window):
                if (inputData.LeftButtonDown == true)
                {
                    allowLeftDraggingToStart = false;
                }

                if (startLeftDragMouseScreenPosition == null) // if not currently dragging, end the mouse-in-map handling here.
                    return;
            }
           
            // Let's check to see if we are inside the map area.  
            if (IsMouseInsideMap()) 
            {
                isRightScrolling = false;

                MouseWorldLocation = new Vector3(inputData.mouseX + MapWindowWorldPosition.X, inputData.mouseY + MapWindowWorldPosition.Y, 0f);
                MouseWorldLocation = The.Map.ClampWorldPosition(MouseWorldLocation);

                MouseTilePosition = MapManager.WorldPosToTilePos(MouseWorldLocation);
                MouseSubtilePosition = MapManager.WorldPosToSubtilePos(MouseWorldLocation);

                // TODO DECOUPLE... make a way for Client to query picking changes
                if (The.Client != null)
                    The.Client.Renderer.UpdatePicking();

                if (inputData.LeftButtonDown == true
                    && The.Map.TileIsOnMap(MouseTilePosition.ToPoint()))
                {
                   
                    if (leftMouseButtonStateInMap != ButtonState.Pressed) // the mouse was just pressed down
                    {
                        leftMouseButtonStateInMap = ButtonState.Pressed;

                        EvaluateMouseDirection();

                        //   HandleMousePressInTile(MouseTilePosition);

                        if ((The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.None
                            || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorTool)
                            && allowLeftDraggingToStart)
                        {
                            // store this pos if the user starts dragging
                            startLeftDragMouseSubtilePosition = MouseSubtilePosition;
                            startLeftDragMouseTilePosition = MouseTilePosition; 
                            startLeftDragMouseScreenPosition = new Point(inputData.mouseX, inputData.mouseY);
                        }

                        if (LeftMouseDownInMap != null)
                        {
                            LeftMouseDownInMap.Invoke(MouseWorldLocation);
                        }
                    }
                    else if (startLeftDragMouseScreenPosition.HasValue)
                    {
                        // button was down already. handle dragging - see if the cursor is in a new tile:
                        if (inputData.mouseX != previousMouseScreenPosition.X || inputData.mouseY != previousMouseScreenPosition.Y)
                        {
                            if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.None)
                            {                               
                                HandleMouseDragCaptureRectangle();

                                The.InGameUI.ShowOverlaysAndMarkerWindows = true;                               
                            }

                            // fire events if position has changed in the various coordinate systems:
                            if (MouseTilePosition != previousMouseTilePosition)
                            {
                                if (LeftMouseTileDragInMap != null)
                                {
                                    LeftMouseTileDragInMap.Invoke(MouseTilePosition);
                                }
                            }

                            if (MouseSubtilePosition != previousMouseSubtilePosition)
                            {
                                if (LeftMouseSubtileDragInMap != null)
                                {
                                    LeftMouseSubtileDragInMap.Invoke(MouseSubtilePosition);
                                }
                            }

                            if (MouseWorldLocation != previousMouseWorldLocation)
                            {
                                if (LeftMouseWorldPosDragInMap != null)
                                {
                                    LeftMouseWorldPosDragInMap.Invoke(MouseWorldLocation);
                                }
                            }
                        }
                    }

                }
                else if (inputData.RightButtonDown)
                {
                    TryMouseDragForMapScrolling(inputData);
                }
                else
                {
                    HandleMouseHoverInMap();

                    if (leftMouseButtonStateInMap == ButtonState.Pressed)
                    { 
                        // user stopped dragging / released the button:
                        HandleLeftMouseRelease(MouseTilePosition);
                    }

                    if (rightMouseButtonStateInMap == ButtonState.Pressed)
                    {
                        // right button was just released
                        HandleRightMouseRelease(inputData);
                    }
                }
            }
            else
            {
                // TODO: should set the other pos'es to null also..
                MouseTilePosition.X = MouseTilePosition.Y = -1;
               // MouseSubtilePosition = null;

            }

            // store the mouse positions to handle dragging:
            previousMouseWorldLocation = MouseWorldLocation;
            previousMouseTilePosition = MouseTilePosition;
            previousMouseSubtilePosition = MouseSubtilePosition;
            previousMouseScreenPosition = new Point(inputData.mouseX, inputData.mouseY);
            
        }

        private void HandleRightMouseRelease(InputData inputData)
        {
            The.Sim.Controller.Game.IsMouseVisible = true;

            // new: right click in map cancels interface ops
            if (!hasRightDragged
                && inputData.mouseX == lastRightDragMousePosition.Value.X
                && inputData.mouseY == lastRightDragMousePosition.Value.Y)
            {
                The.InGameUI.HideMapInterface();
            }

            rightMouseButtonStateInMap = ButtonState.Released;

            hasRightDragged = false;
        }



        private void UpdateScrolling(GameTime time)
        {           
            float elapsed = (float)time.ElapsedGameTime.TotalSeconds;
           // fTotalElapsedTime += elapsed;

          //  bool ismouseInsideMap = IsMouseInsideMap();


            float yScrollSpeedPerSecond = 0f, xScrollSpeedPerSecond = 0f;

            // See if enough time has elapsed since we last moved on the map.
            //  if (fTotalElapsedTime >= fKeyPressCheckDelay)
            //    {

            Options options = The.Client.Controller.Options;
            float maxScrollSpeedPerSecond = options.KeyScrollSpeedPerSecond;

            // scroll with arrow keys...
            bool keyIsPressed = false;

            if (The.Client.EnableKeyboardShortcuts)
            {
                if (inputData.IsKeyDown(options.KeyScrollUp))
                {
                    yScrollSpeedPerSecond = -maxScrollSpeedPerSecond;
                    keyIsPressed = true;
                }
                if (inputData.IsKeyDown(options.KeyScrollDown))
                {
                    yScrollSpeedPerSecond = maxScrollSpeedPerSecond;
                    keyIsPressed = true;
                }
                if (inputData.IsKeyDown(options.KeyScrollLeft))
                {
                    xScrollSpeedPerSecond = -maxScrollSpeedPerSecond;
                    keyIsPressed = true;
                }
                if (inputData.IsKeyDown(options.KeyScrollRight))
                {
                    xScrollSpeedPerSecond = maxScrollSpeedPerSecond;
                    keyIsPressed = true;
                }
            }

            if (keyIsPressed)
            {
              //  fTotalElapsedTime = 0.0f;

                float deltaX = (float)(xScrollSpeedPerSecond * time.ElapsedGameTime.TotalSeconds);
                float deltaY = (float)(yScrollSpeedPerSecond * time.ElapsedGameTime.TotalSeconds);

                ChangeMapWindowWorldPosition(deltaX, deltaY);
            }           

            


          //  mouseIsScrolling = false;


            /*  if (keyIsPressed || mouseIsScrolling)
              {
                  dx += (int)(elapsed * xScrollSpeedPerSecond);
                  dy += (int)(elapsed * yScrollSpeedPerSecond);
                  fTotalElapsedTime = 0.0f;

                  if (The.InGameUI.TrackSelectedEntity)
                  {
                      // stop tracking?

                  }

              }
              else
              {*/
            if (The.InGameUI.TrackSelectedEntity)
            {
                if (The.InGameUI.SelectedEntity != null)
                {
                    IKnownEntityData data;
                    The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
                    if (data != null)
                    {
                        The.InGameUI.ZoomToEntity(data);
                    }
                }
            }


            /*   if (keyIsPressed || mouseIsScrolling)
               {

                   while (dx > tileSize)
                   {
                       dx = dx - tileSize;
                       mapX++;
                   }

                   while (dx < -tileSize)
                   {
                       dx = dx + tileSize;
                       mapX--;
                   }

                   while (dy > tileSize)
                   {
                       dy = dy - tileSize;
                       mapY++;
                   }

                   while (dy < -tileSize)
                   {
                       dy = dy + tileSize;
                       mapY--;
                   }

                   ClampMapPosition();

               }*/

            //RefreshMapWindowWorldPosition();

            /*     if (!Common.IsEqual(MapWindowWorldPosition.X, oldMapWindowWorldPosition.X) 
                     || !Common.IsEqual(MapWindowWorldPosition.Y, oldMapWindowWorldPosition.Y))  //MapWindowWorldPosition != oldMapWindowWorldPosition)
                 {
                     isScrolling = true;

                     extraTerrainRenderFrames = 1;

                    // hasScrolled = true;
                 }
                 else
                 {
                     if (extraTerrainRenderFrames > 0)
                     {
                         isScrolling = true;
                         extraTerrainRenderFrames--;
                     }
                     else
                     {
                         isScrolling = false;
                     }
                 }*/
        }

        /*   private void RefreshMapWindowWorldPosition()
           {
               MapWindowWorldPosition.X = mapWindowTileX * MapManager.tileSize + dx;
               MapWindowWorldPosition.Y = mapWindowTileY * MapManager.tileSize + dy;

               ClampMapPosition();
           }*/

        private void ClampTileMapPosition()
        {
            if (mapWindowTilePosClamped.X < 0)
            {
                mapWindowTilePosClamped.X = 0;
            }

            int maxX = The.Map.mapTileWidth - noOfTilesToDisplayHorizontally;
            if (maxX > 0 && mapWindowTilePosClamped.X > maxX)
            {
                mapWindowTilePosClamped.X = maxX;
            }

            if (mapWindowTilePosClamped.Y < 0)
            {
                mapWindowTilePosClamped.Y = 0;
            }

            int maxY = The.Map.mapTileHeight - noOfTilesToDisplayVertically;
            if (maxY > 0 && mapWindowTilePosClamped.Y > maxY)
            {
                mapWindowTilePosClamped.Y = maxY;
            }
        }

       

        public void ZoomToMapPosition(Point pos) //, Centering centering)
        {
            ZoomToMapPosition(pos.X, pos.Y); //, centering);
        }

        public void ZoomToMapPosition(int x, int y) //, Centering centering)
        {
            ZoomToMapPosition(new Vector3(x * MapManager.tileSize, y * MapManager.tileSize, 0f));

        }

      
        public void ZoomToMapPosition(Vector3 worldLocation) //, Centering centering)
        {
            float mapWindowX, mapWindowY;

            float widthToUse = Math.Max(this.mapWindowWidth, noOfTilesToDisplayHorizontally * MapManager.tileSize);
            float heightToUse = Math.Max(this.mapWindowHeight, noOfTilesToDisplayVertically * MapManager.tileSize);

            mapWindowX = worldLocation.X - widthToUse / 2f;
            mapWindowY = worldLocation.Y - heightToUse / 2f;
            MapWindowWorldPosition = new Vector2(mapWindowX, mapWindowY);

            return;



            // TODO: add zooming animation - ease in?
            //int x = (int)(worldLocation.X / (float)tileSize);
            //int y = (int)(worldLocation.Y / (float)tileSize);

            //  mapWindowTileX = (int)(x / (float)MapManager.tileSize);
            //   mapWindowTileY = (int)(y / (float)MapManager.tileSize);

            /*dx = x % MapManager.tileSize; //(int)(x % tileSize);
            dy = y % MapManager.tileSize; // (int)(y % tileSize);*/

            /*
            if (centering == Centering.Middle)
            {
                x -= noOfTilesToDisplayHorizontally / 2;
                y -= noOfTilesToDisplayVertically / 2;
            }
            else if (centering == Centering.LeftOfCenter)
            {   // make room for interface panel
                x -= noOfTilesLeftOfDetailsInterface / 2;
                y -= noOfTilesToDisplayVertically / 2;
            }

            UWGame.SimSide.Instance.Map.mapX = x;
            UWGame.SimSide.Instance.Map.mapY = y;*/

            //    ClampTileMapPosition();
        }

        public void HandleLeftMouseRelease(TilePos tile)
        {
            leftMouseButtonStateInMap = ButtonState.Released;

            //TerrainTile clickedTile;
            InGameInterface intf = The.InGameUI;

            switch (The.InGameUI.InterfaceMode)
            {
                /* case Interface.InGameInterface.InterfaceState.TileInfo:
                     clickedTile = TileMap[tile.X][tile.Y];
                     intf.SelectTile(clickedTile);
                     intf.InterfaceMode = Interface.InGameInterface.InterfaceState.None;
                     break;
                 */
                /*   case Interface.InGameInterface.InterfaceState.None:
                       {
                           Entity selectedEntity = GetPickedEntity();

                           if (selectedEntity != null)
                           {
                               intf.SelectEntity(selectedEntity);
                               break;
                           }

                           // look at the tile contents now:

                           clickedTile = TileMap[tile.X][tile.Y];

                           if (clickedTile.TiledEntityOnTile != null)
                           {
                               for (int i = 0; i < clickedTile.TiledEntityOnTile.Count; i++)
                               {
                                   if (CanBeSelected(clickedTile.TiledEntityOnTile[i]))
                                   {   // don't select rocks...
                                       selectedEntity = clickedTile.TiledEntityOnTile[i];
                                       intf.SelectEntity(selectedEntity);
                                       return;
                                   }
                               }
                           }

                          // intf.SelectTile(clickedTile);

                           break;

                       }*/
                case InGameInterface.InterfaceState.Launch:
                    {
                        if (The.InGameUI.SelectedEntity != null)
                        {
                            Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                            if (entity != null && entity.Intelligence == null)
                            {
                                entity.Locomotor.StartMoving(Locomotor.Mode.Ballistic, MouseWorldLocation, 680f, 0f, null, null, GameData.Instance.AllAttackTypes["throwSpear"], null, null);
                            }
                        }

                        break;
                    }
                case InGameInterface.InterfaceState.Build:
                    {

                        Common.Direction? dir = null;

                        Entity buildEntity = intf.EntitiesBeingPlaced[0].Entity;

                        bool placementIsValid;

                        
                        if (buildEntity.DirectionalLayout != null)
                        {
                            dir = buildEntity.DirectionalLayout.EdgePosition;

                            // not implemented!!
                            placementIsValid = buildEntity.Structure.IsPlacementValid(buildEntity.TopLeftMapPosition.Value, dir);

                        }
                        else
                        {
                            placementIsValid = buildEntity.Structure.IsPlacementValid(buildEntity.PlaySiteLocation);
                        }


                        if (placementIsValid)
                        {
                            //NON command build code
                            //Expedition expedition = The.Map.GetClosestExpedition(buildEntity.TopLeftMapPosition);

                            //buildEntity.Structure.PrepareAndStartBuildingJob(expedition.ExpeditionOwner, expedition.PlayerSetWorkPriority, buildEntity.Location);


                            //intf.EntitiesBeingPlaced.Clear();
                            ////  intf.EntityBeingPlaced = null;
                            //intf.InterfaceMode = InGameInterface.InterfaceState.None;

                            //// play a sound:
                            //The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
                            //

                            //Command test
                            Expedition expedition = The.Map.GetClosestExpedition(buildEntity.TopLeftMapPosition.Value);

                            Command buildCommand = new Build(buildEntity.EntityType, buildEntity.PlaySiteLocation, true, expedition.OwnedEntities.ID);
                            The.Client.Controller.StoreAndExecuteCommand(buildCommand);
                            buildEntity.Destroy();

                        }

                        break;
                    }
                case InGameInterface.InterfaceState.PlaceExpeditionCenter:
                    {
                        Vector3 location = MouseWorldLocation;

                        // TODO: store the expedition ID when the  PLACE/MOVE button gets clicked
                        Expedition expedition = The.InGameUI.GetExpedition(); // The.Sim.PlaySite.GetFirstPlayerExpedition()
                        if (expedition != null)
                        {
                            Command placeExpeditionCommand = new PlaceExpedition(expedition.ID, location, true);
                            The.Client.Controller.StoreAndExecuteCommand(placeExpeditionCommand);
                        }

                        /*if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(location)))
                        {
                            The.Sim.Site.GetMainExpedition().Center = location;
                            The.Sim.Site.GetMainExpedition().RemoveGatheringSite();
                            intf.SetExpeditionMarkerPositions();
                            
                            // play a sound:
                            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);

                            intf.InterfaceMode = InGameInterface.InterfaceState.None;

                        }*/
                        break;
                    }             

                case InGameInterface.InterfaceState.EditorPlaceEntity:

                    foreach (InGameInterface.EntityPosition ep in intf.EntitiesBeingPlaced)
                    {
                        Vector3 location = MouseWorldLocation + ep.Position.ToVector3();

                        location = The.Map.ClampWorldPosition(location);

                        ep.Entity.PlaceEntityOnPlaySite(/*MapManager.WorldPosToTile(location), MouseMapDirection,*/ location, null, null, null);
                        // ep.Entity.Place(MouseTilePosition, MouseMapDirection, MouseWorldLocation);
                    }

                    Entity representative = intf.EntitiesBeingPlaced[0].Entity;
                    intf.EntitiesBeingPlaced.Clear();

                    // start a new entity:
                    intf.SidePanelEditorEntity.CreateEntityForPlacement(representative.EntityType);
                    The.InGameUI.UpdateEntitiesBeingPlacedPositions();

                    // play a sound:
                    // GUIManager.PlaySound(GUIManager.PlaceBuildingBeep);
                    break;
               
                case InGameInterface.InterfaceState.EditorDeleteEntities:
                    // TODO: move this code to the panel class
                    if (The.InGameUI.IsShowingMapEditor() && The.InGameUI.SidePanelEditorEntity.SelectedEntityType != null)
                    {
                        List<Entity> entitiesOnTile = The.Map.GetTile(MouseTilePosition).EntitiesOnTile;
                        if (entitiesOnTile != null)
                        {
                            string prefix = SidePanelEditorEntity.GetPrefix(The.InGameUI.SidePanelEditorEntity.SelectedEntityType.KeyName);

                            for (int i = entitiesOnTile.Count - 1; i >= 0; i--)
                            {
                                Entity entity = entitiesOnTile[i];
                                if (entity.EntityType == The.InGameUI.SidePanelEditorEntity.SelectedEntityType
                                    || SidePanelEditorEntity.MatchesPrefix(entity.EntityType, prefix))
                                {
                                    entity.Destroy();
                                }
                            }
                        }
                    }
                    break;
                case InGameInterface.InterfaceState.EditorClearTile:
                    if (The.Map.TileIsOnMap(MouseTilePosition))
                    {
                        The.Map.ClearTile(The.Map.TileMap[MouseTilePosition.X][MouseTilePosition.Y]);
                    }
                    break;
                case InGameInterface.InterfaceState.Threat:
                    //AddThreatSource(MouseTilePosition); // moved to SharedKnowledge
                    intf.InterfaceMode = InGameInterface.InterfaceState.None;
                    break;

                case InGameInterface.InterfaceState.BlockSubtile:
                    The.Map.SetSubtileTerrainCost(MouseWorldLocation, 0);
                    break;
                case InGameInterface.InterfaceState.UnblockSubtile:
                    The.Map.SetSubtileTerrainCost(MouseWorldLocation, 3);
                    break;

                case InGameInterface.InterfaceState.None:
                    HandleLeftClickOrStopDragging();
                    break;

              /*  case InGameInterface.InterfaceState.EditorTool:
                    HandleLeftClickOrStopDragging();
                    break;*/
            }

            if (LeftMouseReleasedInMap != null)
            {
                LeftMouseReleasedInMap.Invoke(MouseWorldLocation);
            }

        }

       
        public void OnPlaceExpedition()
        {
            The.InGameUI.SetExpeditionMarkerPositions();
            // play a sound:
            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);

            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
        }

        private EntityID? GetPickedEntity()
        {
            EntityID? selectedEntity = null;
            if (The.Client != null && The.Client.Renderer.PickedModel != null)
            {
                IKnownEntityData entityData;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.Client.Renderer.PickedModel.Value, out entityData);

                if (entityData != null)
                {
                    if (!CanBeSelected(entityData))
                    {
                        // don't select rocks or items...

                    }
                    else
                    {
                        selectedEntity = The.Client.Renderer.PickedModel;
                    }
                }
                else
                {
                    The.Client.Renderer.PickedModel = null;
                }
            }

            return selectedEntity;
        }

        private static bool CanBeSelected(IKnownEntityData entity)
        {
#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Overlays.CollisionGeometries") == true)
            {
                return entity.EntityType.ItemType == null;
            }
#endif
            if (entity.EntityType.IsSelectable.HasValue)
            {
                return entity.EntityType.IsSelectable.Value;
            }
            else
            {
                return
                     (entity.EntityType.RockType == null
                    && entity.EntityType.ItemType == null
                    && entity.EntityType.TreeType == null
                    && entity.EntityType.TerrainType == null);
            }
        }

        public void EvaluateMouseDirection()
        {
            /*int mouseTileX = ((mouseState.X - iMapDisplayOffsetX + (int)dx) % MapManager.tileSize);
            int mouseTileY = ((mouseState.Y - iMapDisplayOffsetY + (int)dy) % MapManager.tileSize);*/

            int mouseTileX = MouseTilePosition.X;
            int mouseTileY = MouseTilePosition.Y;

            int xPart = mouseTileX / 12;
            int yPart = mouseTileY / 12;
            MouseMapDirection = mouseToDirection[Common.Clamp(xPart, 0, 3), Common.Clamp(yPart, 0, 3)];
        }

        /// <summary>
        /// when the user releases the left mouse button
        /// </summary>
        private void HandleLeftClickOrStopDragging()
        {
#if DEBUG


            /*
            if (The.InGameUI.SelectedEntity != The.Sim.PlaySite.Persons[0].ID)
            {
                The.InGameUI.SelectEntity(The.Sim.PlaySite.Persons[0]);  // #TURNTEST
            }
             


            Message message = Trigger.CreateInterestMessage(null, null, null, MouseWorldLocation, 40); //100f);
            The.Sim.PlaySite.Persons[0].Intelligence.Brain.HandleMessage(message);

            Console.WriteLine("{0} Interest message sent", The.Sim.TotalUnPausedGameTimeInSeconds);

            return; // #TURNTEST
            * */
#endif

            if (startLeftDragMouseTilePosition.HasValue
                && startLeftDragMouseTilePosition.Value != MouseTilePosition) //The.InGameUI.SelectedTiles.Coverage.Count > 0)
            {
                // he dragged over multiple tiles:

                The.MapUI.SelectBoundedRectangleTiles(startLeftDragMouseTilePosition.Value, MouseTilePosition, The.InGameUI.SelectedTiles);

                if (inputData.IsKeyDown(limitSelectionKey))
                {
                    The.MapUI.CropTilesToConnectedArea(The.InGameUI.SelectedTiles);
                }


                The.InGameUI.SelectedZone = null;


                Point pos = GetContextMenuOpenerPosFromMouse();
                ShowContextMenuOpener(pos.X, pos.Y);

                The.InGameUI.ShowSelectedMapAreaPanel();

            }
            else
            {
                // he clicked inside the same tile - handle it differently depending on whether an entity or a tile was hit:
                HandleMouseClickInTile();
            }

            // turn off the rectangle
            The.InGameUI.SelectRectangle.Visible = false;

            // remove preview tiles:
            The.InGameUI.SelectedTilesPreview.Clear();

            // no longer dragging:
            ResetDragging();

        }


        public void ResetDragging()
        {
            startLeftDragMouseTilePosition = null;
            startLeftDragMouseScreenPosition = null;
            startLeftDragMouseSubtilePosition = null;
        }


        public void CropTilesToConnectedArea(MapArea mapArea)
        {
            List<TerrainTile> newList = null;

            mapArea.CheckConnectivity(true, ref newList);

            mapArea.Clear();
            foreach (var tile in newList)
            {
                mapArea.Add(tile);
            }
        }




        public void SelectBoundedRectangleTiles(TilePos from, TilePos to, MapArea mapArea) //, out Rectangle boundingRectangle)
        {
            mapArea.Clear();


            int minX = Common.Min(from.X, to.X);
            int maxX = Common.Max(from.X, to.X);
            int minY = Common.Min(from.Y, to.Y);
            int maxY = Common.Max(from.Y, to.Y);

            //  Rectangle boundingRectangle = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

            // drag from lower right to top left gives start drag tile outside the area...??
            mapArea.StartDragTile = from.ToPoint();
            //   mapArea.BoundingRectangle = boundingRectangle;


            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
            Point tilePos;


            Point minTile = The.Map.ClampTileMapPosition(new Point(minX, minY));
            Point maxTile = The.Map.ClampTileMapPosition(new Point(maxX, maxY));



            for (int x = minTile.X; x <= maxTile.X; x++)
            {
                for (int y = minTile.Y; y <= maxTile.Y; y++)
                {
                    tilePos = new Point(x, y);

                    if (!The.Map.TileIsCompletelyBlocked(terrainCosts, tilePos)) // exclude blocked tiles...
                    {
                        mapArea.Add(The.Map.GetTile(tilePos));

                        if (mapArea.Count > 1600)
                        {
                            // avoid humongous selections... put in CycleManager?
                            mapArea.RecomputeBoundingRectangleAndEdges();

                            return;
                        }
                    }

                }
            }

            mapArea.RecomputeBoundingRectangleAndEdges();
        }


        /// <summary>
        /// perhaps we will implement point commands again..?
        /// </summary>
        private void HandleMouseHoverInMap()
        {
            switch (The.InGameUI.InterfaceMode)
            {

                case InGameInterface.InterfaceState.None:
                    HandleHoverOverEntity();

                    break;

                /*  case InGameInterface.InterfaceState.Hunt:
                      {
                          Color? tintColor = null;
                          if (The.Client != null && The.Client.Renderer.PickedModel != null) // is Entity)
                          {
                              if (The.Map.EntityCanBeMarkedAsHuntTarget(The.Client.Renderer.PickedModel))
                              {
                                  tintColor = Color.LightGreen;
                              }
                              else
                              {
                                  tintColor = Color.OrangeRed;
                              }
                          }


                          HandleHoverOverEntity(tintColor);

                          break;
                      }*/
                /*  case InGameInterface.InterfaceState.Salvage:
                      {
                          Color? tintColor = null;
                          if (The.Client != null && The.Client.Renderer.PickedModel is Entity)
                          {
                              if (EntityCanBeSalvaged((Entity)The.Client.Renderer.PickedModel))
                              {
                                  tintColor = Color.LightGreen;
                              }
                              else
                              {
                                  tintColor = Color.OrangeRed;
                              }
                          }


                          HandleHoverOverEntity(tintColor);

                          break;
                      }*/

                /* case InGameInterface.InterfaceState.Gather:
                     {
                         Color? tintColor = null;
                         TerrainTile newHoverTile;

                         if (IsMouseInsideMap())
                         {
                             newHoverTile = The.Map.TileMap[MouseTilePosition.X][MouseTilePosition.Y];

                             if (newHoverTile.HasGatherableResources())
                             {
                                 tintColor = Color.LightGreen;
                             }
                             else
                             {
                                 tintColor = Color.OrangeRed;
                             }
                         }


                         HandleHoverOverTile(tintColor);

                         break;
                     }*/
            }
        }

        /// <summary>
        /// never gets called while the mouse is over a marker window (or any other window)
        /// </summary>
        /// <param name="hoverTintingColor"></param>
        private void HandleHoverOverEntity(Color? hoverTintingColor = null)
        {
            The.InGameUI.HoverTile = null;

            EntityID? newHoverEntity = null;

            /*if (The.InGameUI.MarkerWindowHoverEntity != null)
            {
                newHoverEntity = The.InGameUI.MarkerWindowHoverEntity;
                //The.InGameUI.HoverEntity = The.InGameUI.MarkerWindowHoverEntity;

            }
            else
            {*/
            newHoverEntity = GetPickedEntity(); // can be null

            The.InGameUI.SetHoverEntity(newHoverEntity, hoverTintingColor);

            // }

            /* if (The.InGameUI.HoverEntity != newHoverEntity
                     || The.InGameUI.Selection.HoverTintingColor != hoverTintingColor) // also restart hover anim if the hover color has changed.
             {
                 The.InGameUI.HoverEntity = newHoverEntity;

                 The.InGameUI.Selection.StartHoverOverEntity(hoverTintingColor);
             }*/

        }

        public bool IsMouseInsideMap()
        {
            if (The.InGameUI == null)
                return false; //perfectly sensible

            return inputData.mouseX >= 0 && inputData.mouseX < mapWindowWidth && inputData.mouseY >= 0 && inputData.mouseY < mapWindowHeight;
        }

        /// <summary>
        /// not currently used, but preserved for later if we ever need it.
        /// </summary>
        /// <param name="hoverTintingColor"></param>
        private void HandleHoverOverTile(Color? hoverTintingColor = null)
        {
            The.InGameUI.SetHoverEntity(null); //) HoverEntity = null;

            if (IsMouseInsideMap())
            {
                TerrainTile newHoverTile = The.Map.GetTile(MouseTilePosition);

                if (The.InGameUI.HoverTile != newHoverTile
                    || The.InGameUI.Selection.HoverTintingColor != hoverTintingColor) // also restart hover anim if the hover color has changed.
                {
                    The.InGameUI.HoverTile = newHoverTile;

                    The.InGameUI.Selection.StartHoverOverTile(hoverTintingColor);
                }
            }
            else
            {
                The.InGameUI.HoverTile = null;
            }

        }


        private void HandleMouseClickInTile()
        {
            // The.Sim.Site.Persons[0].Renderable.SetNewCenterOfAttention(MouseWorldLocation, 2);


            // click in the same tile, see if we hit a model:                
            EntityID? selectedEntity = GetPickedEntity();

           

            bool hasSelected = false;

            if (selectedEntity != null)
            {
                The.InGameUI.SelectEntity(selectedEntity.Value);
                hasSelected = true;

                // why not show panel???
            }

            // look at the tile contents now:
            if (MouseTilePosition.X < 0 || MouseTilePosition.X >= The.Map.mapTileWidth
                || MouseTilePosition.Y < 0 || MouseTilePosition.Y >= The.Map.mapTileHeight)
            {
                return;
            }

            TerrainTile clickedTile = The.Map.GetTile(MouseTilePosition);


            // first see if we can select an entity:

            if (!hasSelected && clickedTile.GeoLayoutEntitiesOnTile != null)
            {
                Entity entityOnTile;
                for (int i = 0; i < clickedTile.GeoLayoutEntitiesOnTile.Count; i++)
                {
                    entityOnTile = Entity.FindByID(clickedTile.GeoLayoutEntitiesOnTile[i]);
                    if (entityOnTile != null && CanBeSelected(entityOnTile)) // don't select rocks...
                    {

                        if (!MouseIsInEntitySelectionArea(entityOnTile)) // entityOnTile.Collidable != null && !entityOnTile.Collidable.ContainsPoint(MouseWorldLocation.ToVector2()))
                        {
                            continue;
                        }

                        selectedEntity = entityOnTile.ID;
                        The.InGameUI.SelectEntity(selectedEntity.Value);

                        hasSelected = true;

                        The.InGameUI.ShowSelectedEntityPanel();

                        return;
                    }
                }
            }

            if (!hasSelected && clickedTile.EntitiesOnTile != null)
            {
                foreach (var entityOnTile in clickedTile.EntitiesOnTile)
                {
                    if (CanBeSelected(entityOnTile)) // don't select rocks...
                    {
                        // select collidables:
                        if ((entityOnTile.Renderable == null || entityOnTile.Renderable.RenderAsModel == null) // models are selected via picking
                            && MouseIsInEntitySelectionArea(entityOnTile))
                        {
                            The.InGameUI.SelectEntity(entityOnTile);
                            The.InGameUI.ShowSelectedEntityPanel();

                            return;
                        }
                    }
                }
            }

            if (!hasSelected)
            {
                // else select the tile itself:
                The.InGameUI.SelectTile(clickedTile);

                Point pos = GetContextMenuOpenerPosFromMouse();
                ShowContextMenuOpener(pos.X, pos.Y); // new Point(clickedTile.X, clickedTile.Y));      

                The.InGameUI.ShowSelectedMapAreaPanel();

                The.InGameUI.ShowOverlaysAndMarkerWindows = true;
            }


        }

        private bool MouseIsInEntitySelectionArea(Entity entity)
        {
            Vector2 mouseLocation = MouseWorldLocation.ToVector2();
            if (entity.SelectionShape != null && entity.SelectionShape.ContainsPoint(mouseLocation))
            {
                return true;
            }

            if (entity.Collidable != null && entity.Collidable.ContainsPoint(mouseLocation))
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// tries to follow the mouse move direction to place the opener icon where it is convenient for the user
        /// </summary>
        /// <returns></returns>
        private Point GetContextMenuOpenerPosFromMouse()
        {
            Vector2 currentMousePos = new Point(inputData.mouseX, inputData.mouseY).ToVector2();

            Vector2 moveDir = currentMousePos - previousMouseScreenPosition.ToVector2();

            Vector2 mousePos = currentMousePos;

            mousePos.Y -= 16;
            mousePos.X -= 2;

            // displace in the move direction if the mouse was moving significantly:
            if (moveDir.LengthSquared() > 25)
            {
                // fast movement??!!
                moveDir.Normalize();

                mousePos += moveDir * 8f; // WorldPosToScreen(MouseWorldLocation).ToPoint();
            }
            else if (startLeftDragMouseScreenPosition.HasValue
                && currentMousePos != startLeftDragMouseScreenPosition.Value.ToVector2())
            {
                // displace in the drag direction:
                moveDir = currentMousePos - startLeftDragMouseScreenPosition.Value.ToVector2();
                moveDir.Normalize();

                mousePos += moveDir * 32f; // WorldPosToScreen(MouseWorldLocation).ToPoint();

            }
            else
            {
                // click without movement:
                mousePos.X += 6;
            }


            return mousePos.ToPoint();
        }

        /* private void HandleSelectSingleTile()
         {
             if (The.InGameUI.SelectedTiles.Coverage.Count == 0) 
             {
                 The.InGameUI.SelectedTiles.Coverage.Add(TileMap[MouseTilePosition.X][MouseTilePosition.Y]);
                
                 ShowSelectedTilesInfo();
             }
         }*/

        private void HandleMouseDragCaptureRectangle()
        {
            // show rectangular capture box
            // Vector2 start = TilePosToScreen(startDragMouseTilePosition.Value);

            //Vector2 end =  TilePosToScreen(MouseTilePosition);

            int mouseX = inputData.mouseX;
            int mouseY = inputData.mouseY;

            int width = Math.Abs((int)(mouseX - startLeftDragMouseScreenPosition.Value.X /*+ tileSize*/));
            int height = Math.Abs((int)(mouseY - startLeftDragMouseScreenPosition.Value.Y /*+ tileSize*/));


            The.InGameUI.SelectRectangle.SetSize(Math.Min((int)startLeftDragMouseScreenPosition.Value.X, mouseX), Math.Min((int)startLeftDragMouseScreenPosition.Value.Y, mouseY), width, height);

            if (width > 12 || height > 12)
            {
                The.InGameUI.SelectRectangle.Visible = true;
            }
            else
            {
                The.InGameUI.SelectRectangle.Visible = false;
            }

            if (MouseTilePosition != previousMouseTilePosition)
            {
                UpdateTileSelectionPreview();
            }
        }

        private Keys limitSelectionKey = Keys.LeftAlt;

        private void UpdateTileSelectionPreview()
        {
            // update the selection preview:
            The.InGameUI.SelectedTilesPreview.Clear();


            SelectBoundedRectangleTiles(startLeftDragMouseTilePosition.Value, MouseTilePosition, The.InGameUI.SelectedTilesPreview); //, out boundingRectangle);

            if (inputData.IsKeyDown(limitSelectionKey))
            {
                CropTilesToConnectedArea(The.InGameUI.SelectedTilesPreview);
            }
        }

        private void HandleRightMouseDrag()
        {
            // TODO: improve this somehow!!!
            hasRightDragged = true; // don't deselect when we just dragged

            int mouseX = inputData.mouseX;
            int mouseY = inputData.mouseY;
                       
            float deltaX = (lastRightDragMousePosition.Value.X - mouseX);
            float deltaY = (lastRightDragMousePosition.Value.Y - mouseY);


            ChangeMapWindowWorldPosition(deltaX, deltaY);

            /*
            MapWindowWorldPosition += new Vector2(deltaX, deltaY);
            int halfWidth = The.Sim.Controller.Game.GraphicsDeviceManager.PreferredBackBufferWidth / 2;
            int halfHeight = The.Sim.Controller.Game.GraphicsDeviceManager.PreferredBackBufferHeight / 2;
            MapWindowWorldPosition = new Vector2(Common.Clamp(MapWindowWorldPosition.X, 0 - halfWidth, The.Map.MapWorldWidth - halfWidth),
                                                Common.Clamp(MapWindowWorldPosition.Y, 0 - halfHeight, The.Map.MapWorldHeight - halfHeight));
            */

            //MapWindowWorldPosition.X += deltaX;
            //MapWindowWorldPosition.Y += deltaY;


            /*int deltaX = (int)(lastRightDragMousePosition.Value.X - mouseX);
            int deltaY = (int)(lastRightDragMousePosition.Value.Y - mouseY);*/

            //     int tileDeltaX = (int) (deltaX / MapManager.tileSize);
            //    int tileDeltaY = (int) (deltaY / MapManager.tileSize);

            /*int deltaDx = deltaX % MapManager.tileSize;
            int deltaDy = deltaY % MapManager.tileSize;

            mapWindowTileX += tileDeltaX;
            mapWindowTileY += tileDeltaY;*/

            /*dx += deltaDx;
            dy += deltaDy;*/

            //     dx += deltaX % MapManager.tileSize;
            //    dy += deltaY % MapManager.tileSize;

            //   RefreshMapWindowWorldPosition();

            //ClampMapPosition();
            

            // change mouse cursor location???
            // Mouse.SetPosition(lastRightDragMousePosition.Value.X, lastRightDragMousePosition.Value.Y);
            lastRightDragMousePosition = new Point(mouseX, mouseY);

        }

        private void ChangeMapWindowWorldPosition(float deltaX, float deltaY)
        {
            Vector2 mapWindowWorldPosition = MapWindowWorldPosition + new Vector2(deltaX, deltaY);
            int halfWidth = The.Sim.Controller.DrawArea.Width / 2; //Game.GraphicsDeviceManager.PreferredBackBufferWidth / 2;
            int halfHeight = The.Sim.Controller.DrawArea.Height / 2; //Game.GraphicsDeviceManager.PreferredBackBufferHeight / 2;
            MapWindowWorldPosition = new Vector2(Common.Clamp(mapWindowWorldPosition.X, 0 - halfWidth, The.Map.MapWorldWidth - halfWidth),
                                                Common.Clamp(mapWindowWorldPosition.Y, 0 - halfHeight, The.Map.MapWorldHeight - halfHeight));
        }

        public void ShowContextMenuOpener(int screenPosX, int screenPosY) //Point tilePos)
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                The.InGameUI.ContextMenuOpener.Hide(); // this hides all child windows

                //  Vector2 screenPos = TilePosToScreen(tilePos);// (int)screenPos.X + MapManager.tileSizeOver2, (int)screenPos.Y - MapManager.tileSizeOver2
                The.InGameUI.ContextMenuOpener.ShowOnPlayfield(screenPosX, screenPosY); //  The.InGameUI.input.MouseState.X, The.InGameUI.input.MouseState.Y);

                The.InGameUI.ContextMenuOpener.Populate();
            }
            else
            {
                // in edit mode we open the dialog right away:
                SetTileResourcesWindow setTileResources = The.InGameUI.SetTileResources;

                // closes the popup:
                setTileResources.Hide();
                setTileResources.DisplayWindow.Show();


                setTileResources.WorldPosition = MouseWorldLocation.ToVector2();

                setTileResources.DisplayWindow.X = inputData.mouseX;
                setTileResources.DisplayWindow.Y = inputData.mouseY;

                setTileResources.Populate();
            }
        }


        public void DrawNoiseMap(Color color, Tuple<NoiseParams, byte[]> noise)
        {


            /*   int maxY = mapWindowTileY + noOfTilesToDisplayVertically;
               int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally;

               int startY = mapWindowTileY;
               int startX = mapWindowTileX;

          
               SimplexNoise.perm = noise.Item2;

               for (int y = startY; y < maxY; y++)
               {
                   for (int x = startX; x < maxX; x++)
                   {
                       DrawResourceNoiseOnTile(ref color, noise, ref tileColor, y, x, out from);
                   }
               }*/
        }

        private void PrintResourceAmountOnTile(TerrainTile terrainTile, ResourceType resourceType, Color color)
        {
            
            string printString = null;
            int noOfItems = 0;
            Vector2 screenPosition;

            if (resourceType.TileResourceType != null && terrainTile.TileResources != null)
            {
                TileResourceContainer container;
                if (terrainTile.TileResources.TryGetValue(resourceType, out container))
                {
                    noOfItems = container.NoOfHarvestableItems;
                }
            }
            else if (resourceType.CropType != null && terrainTile.TreesOnTile != null)
            {
                Entity treeWithCropsOfType = terrainTile.TreesOnTile.FirstOrDefault(
                    t => t.EntityType.TreeType.CropTypes != null
                    && t.EntityType.TreeType.CropTypes.Contains(resourceType));

                if (treeWithCropsOfType != null)
                {
                    UWGame.SimSide.Trees.Tree tree;
                    treeWithCropsOfType.Find(out tree);
                    Crop crop;

                    if (tree.Crops.TryGetValue(resourceType, out crop))
                    {
                        noOfItems = crop.NoOfHarvestableItems;
                    }
                }
            }

            if (noOfItems > 0)
            {
                screenPosition = The.MapUI.TileEdgeToScreen(terrainTile.X, terrainTile.Y);

                printString = noOfItems.ToString();
                Kensei.Dev.DevText.Print(screenPosition, printString, color);
            }

        }

        private void DrawResourceNoiseOnTile(Point tilePos, Color color, Tuple<NoiseParams, SimplexNoise> noise)
        {
           
            //SimplexNoiseGenerator.SeedNumbers = noise.Item2;
            float value = ChangeResourcesAction.GetResourceNoiseValue(noise.Item2, noise.Item1, tilePos); // SimplexNoise.Generate((float)x * NoiseFrequency.Value, (float)y * NoiseFrequency.Value);

            DrawTileOverlay(tilePos, color, value);
        }

        private void DrawTileOverlay(Point tilePos, Color color, float value)
        {

            int tileSize = MapManager.tileSize;
            Vector2 from;
            Color tileColor;

            float alpha;
            from = TileEdgeToScreen(tilePos.X, tilePos.Y);

            if (value > 0)
            {
                alpha = value / 3f;
                alpha = Math.Min(1f, alpha);
                tileColor = color * alpha;
                Kensei.Dev.Shape.Box(from, new Vector2(from.X + tileSize, from.Y + tileSize), tileColor, true);
            }
        }



        private void IterateOnScreenTiles(Action<TerrainTile> tileFunction)
        {
            int maxY = mapWindowTileY + noOfTilesToDisplayVertically;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally;

            int startY = mapWindowTileY;
            int startX = mapWindowTileX;

            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    tileFunction(The.Map.GetTile(x, y));
                }
            }

        }

      /*  private void IterateVisibleTiles(Action<TerrainTile> action)
        {
            int maxY = mapWindowTileY + noOfTilesToDisplayVertically + 1;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally + 1;

            maxY = The.Map.ClampTileMapYPosition(maxY);
            maxX = The.Map.ClampTileMapXPosition(maxX);

            int startY = mapWindowTileY - 1;
            int startX = mapWindowTileX - 1;

            startX = The.Map.ClampTileMapXPosition(startX);
            startY = The.Map.ClampTileMapYPosition(startY);

            //Color color = Color.Black * 0.25f;
            TerrainTile tile;
            int noOfHarvestJobsOnTile;
            for (int y = startY; y <= maxY; y++)
            {
                for (int x = startX; x <= maxX; x++)
                {
                    tile = The.Map.GetTile(x, y);

                    action(tile);
                }
            }
        }*/

        public void DrawCrops(Allegiance allegiance, ResourceType resourcetype)
        {

            Color color = Color.Black * 0.25f;

            ResourceMap crops = allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(resourcetype); //.CropsMaps[harvestJob.CropType.CropItem];
            byte[][] cropsMap = null;
            ResourceMap.Result result = crops.GetMap(ref cropsMap); // store it...
            if (result == ResourceMap.Result.OK)
            {
                DrawByteMap(ref color, cropsMap);
            }
        }

        private void DrawByteMap(ref Color color, byte[][] byteMap)
        {
            int maxY = mapWindowTileY + noOfTilesToDisplayVertically;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally;

            int startY = mapWindowTileY;
            int startX = mapWindowTileX;

            int tileSize = MapManager.tileSize;

            Vector2 from;
            byte value;
            float alpha;

            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    from = TileEdgeToScreen(x, y);

                    value = byteMap[x][y];

                    if (value > 0)
                    {
                        alpha = value / 4f;
                        alpha = Math.Min(1f, alpha);
                        color = Color.Crimson * alpha;
                        Kensei.Dev.Shape.Box(from, new Vector2(from.X + tileSize, from.Y + tileSize), color, true);
                    }
                }
            }

        }

      /// <summary>
      /// doesn't draw, updates HUD positions...
      /// </summary>
        public void Draw()
        {
            // do this check in Draw instead of Update because if Draw calls are dropped because of low framerate, we may not redraw everything...
            if (oldMapWindowWorldPosition != MapWindowWorldPosition)
            {
                isScrolling = true;

                The.Client.MapHasMoved = true;
            }
            else
            {
                isScrolling = false;
            }


            oldMapWindowWorldPosition = MapWindowWorldPosition;

            // Lars: it was necessary to move this update function to Draw from Update to avoid a lagging effect
            UpdateHUD();


        }

        public void DrawPathSearch() 
        {
            
            Entity entity = null;
            if (The.InGameUI.SelectedEntity != null)
            {
                entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
            }
            else if (The.InGameUI.UIAllegiance != null && The.InGameUI.UIAllegiance.MembersList.Count > 0)
            {
                entity = The.InGameUI.UIAllegiance.MembersList[0];
            }

            if (entity != null)
            {

                if (entity.Intelligence != null && entity.Intelligence.PathPlanner != null)
                {
                    UWGame.SimSide.AI.Pathfinding.AStarSearch search = entity.Intelligence.PathPlanner.search;
                    if (search != null && !search.Stopped)
                    {
                        search.Draw();


                    }
                }

            }
        }

       


        public void Update(GameTime time)
        {
            if (The.Client.IsModal)
                return;

            UpdateScrolling(time);

            //  UpdateHUD();

        }


        /// <summary>
        /// move the HUD if the user is scrolling?
        /// </summary>
        private void UpdateHUD()
        {
            //  return;

            if (isScrolling)
            {
                The.InGameUI.MoveHUDWindows();

                The.InGameUI.MoveFogMap();
            }

        }

        public void DrawCollisionGeometries()
        {
            if (The.CollisionManager == null)
                return;

            List<Collidable<Entity>> collidables = new List<Collidable<Entity>>();
            CollideShape2D windowBounds = new CollideShape2D(mapWindowWorldPosition, mapWindowWorldPosition + new Vector2(mapWindowWidth, mapWindowHeight));
            The.CollisionManager.GetCollidablesIntersectingBounds(windowBounds, ref collidables);

            Entity entity;
            foreach (var item in collidables)
            {
                entity = item.Parent;
                Color col = Color.Orange;

                if (entity.EntityType.Person != null)
                {
                    col = Color.DarkCyan;//cyan is people, so is Soylent Green
                }
                else if (entity.EntityType.ItemType != null)
                {
                    col = Color.Cornsilk;
                }

                if (entity.EntityType.LocomotorType != null)
                {
                    if (entity.Locomotor.IsColliding)
                        col = Color.Yellow;

                }

                DrawCollisionShape(item.Parent.Collidable, col, true);

            }

            /*
            foreach (Entity entity in The.Sim.AllStructures)
            {
                Color col = Color.Orange;

                if (entity.Locomotor != null)
                {
                    if (entity.Locomotor.IsColliding)//crashes when collision geometries is enabled
                        col = Color.Yellow;//yellow is for structures
                   
                }

                DrawCollisionShape(entity, col);
                
            }

            foreach (Entity entity in The.Sim.Site.Persons)
            {
                Color col = Color.DarkCyan;//cyan is people, so is Soylent Green

                if (entity.Locomotor.IsColliding)
                    col = Color.LightCyan;

                DrawCollisionShape(entity, col);
            }*/
        }

        public void DrawSelectionShapes()
        {
            // TODO: make this better
            foreach (EntityID id in The.Sim.AllStructures)
            {
                Entity entity = Entity.FindByID(id);
                if (entity == null)
                    continue;

                DrawCollisionShape(entity.SelectionShape, Color.LightYellow, false);
            }

            foreach (EntityID id in The.Sim.AllTerrainEntities)
            {
                Entity entity = Entity.FindByID(id);
                if (entity == null)
                    continue;

                DrawCollisionShape(entity.SelectionShape, Color.LightGoldenrodYellow, false);
            }
        }

        public void DrawTerrainGeometries()
        {
            foreach (EntityID id in The.Sim.AllStructures)
            {
                Entity entity = Entity.FindByID(id);
                if (entity == null)
                    continue;

                DrawCollisionShape(entity.Collidable, Color.LightPink, false);
            }

            foreach (EntityID id in The.Sim.AllTerrainEntities)
            {
                Entity entity = Entity.FindByID(id);
                if (entity == null)
                    continue;

                DrawCollisionShape(entity.Collidable, Color.LightPink, false);//magenta is for terrain (not really collidable)
            }
        }


        private byte strobe = 0;
        private void DrawCollisionShape(Collidable<Entity> shape, Color color, bool onlyEnabled = true)
        {
            if (shape != null)
            {
                if (onlyEnabled == true && !shape.Enabled)
                    return;

                if (shape.IsComposite)
                {

                    bool isSelected = The.InGameUI.SelectedEntity == shape.Parent.EntityID;
                    int selectedShapeIndex = 0;

                    if (isSelected)
                    {
                        if ((strobe++ & 8) != 0)
                            color = Color.Black;

                        //re-assign child shape index?
                        
                        if (inputData.IsKeyDown(Keys.F1))
                            selectedShapeIndex = 1;
                        if (inputData.IsKeyDown(Keys.F2))
                            selectedShapeIndex = 2;
                        if (inputData.IsKeyDown(Keys.F3))
                            selectedShapeIndex = 3;
                        if (inputData.IsKeyDown(Keys.F4))
                            selectedShapeIndex = 4;

                        /* // number keys are used for speed control
                        if (inputData.IsKeyDown(Keys.D1))
                            g = 1;
                        if (inputData.IsKeyDown(Keys.D2))
                            g = 2;
                        if (inputData.IsKeyDown(Keys.D3))
                            g = 3;
                        if (inputData.IsKeyDown(Keys.D4))
                            g = 4;*/

                        //diagonal keys
                        if (inputData.IsKeyDown(Keys.NumPad7))
                            shape.NudgeChildShape(selectedShapeIndex, -1, -1, 0);
                        else if (inputData.IsKeyDown(Keys.NumPad9))
                            shape.NudgeChildShape(selectedShapeIndex, 1, -1, 0);
                        else if (inputData.IsKeyDown(Keys.NumPad1))
                            shape.NudgeChildShape(selectedShapeIndex, -1, 1, 0);
                        else if (inputData.IsKeyDown(Keys.NumPad3))
                            shape.NudgeChildShape(selectedShapeIndex, 1, 1, 0);
                        else
                        {
                            //up or down
                            if (inputData.IsKeyDown(Keys.NumPad8))
                                shape.NudgeChildShape(selectedShapeIndex, 0, -1, 0);
                            else if (inputData.IsKeyDown(Keys.NumPad2)
                                || inputData.IsKeyDown(Keys.NumPad5))
                                shape.NudgeChildShape(selectedShapeIndex, 0, 1, 0);

                            //left or right
                            if (inputData.IsKeyDown(Keys.NumPad4))
                                shape.NudgeChildShape(selectedShapeIndex, -1, 0, 0);
                            else if (inputData.IsKeyDown(Keys.NumPad6))
                                shape.NudgeChildShape(selectedShapeIndex, 1, 0, 0);
                        }

                        //scale radius
                        if (inputData.IsKeyDown(Keys.Subtract))
                            shape.NudgeChildShape(selectedShapeIndex, 0, 0, -1);
                        else if (inputData.IsKeyDown(Keys.Add))
                            shape.NudgeChildShape(selectedShapeIndex, 0, 0, 1);

                        // scale rectangle width
                        if (inputData.IsKeyDown(Keys.Divide))
                            shape.NudgeChildShape(selectedShapeIndex, 0, 0, 0, -2);
                        else if (inputData.IsKeyDown(Keys.Multiply))
                            shape.NudgeChildShape(selectedShapeIndex, 0, 0, 0, 2);
                    }
                    

                    //draw all of the child shapes
                   // shape.IterateChildShapes(DrawShapeDelegate, color, isSelected);

                    shape.IterateChildShapes((s, idx) => DrawShapeDelegate(s, color, isSelected, drawCoords: idx == selectedShapeIndex));
                    
                    //draw the bounding box

                    if (shape.Parent.EntityType.TerrainType == null)//don't draw bounds on terrains
                    {
                        Color c = Color.White;
                        if (isSelected && shape.FlipHorizontally)
                        {
                            c = Color.Chartreuse;
                            Kensei.Dev.DevText.Print(WorldPosToScreen(shape.Bounds.BoundsLowerLeft),
                                                                    "FLIPPED HORIZ.", c);

                        }


                        Kensei.Dev.Shape.Box(WorldPosToScreen(shape.Bounds.BoundsUpperLeft),
                            WorldPosToScreen(shape.Bounds.BoundsLowerRight), c, false);
                    }
                }
                else
                    DrawShapeDelegate(shape.Bounds, color, drawCoords: true);
            }

        }

        static bool DrawShapeDelegate(CollideShape2D shape, Color c, bool isSelected = false, bool drawCoords = true)
        {
            Color color = c;
            if (shape.HFlipped && isSelected && color != Color.Black)
                color = Color.Chartreuse;

            Vector2 parentCenter = Vector2.Zero;
            if (shape.Parent != null) //this is a child shape
            {
                parentCenter = shape.Parent.Center;

            }


            /* if (shape.PrimitiveType == CollidePrim.Circle)
             {*/
            float radius = shape.Radius;

            Vector2 center = shape.Center + shape.Offset;
            center += parentCenter;

            center = The.MapUI.WorldPosToScreen(center);

            if (shape.PrimitiveType == CollidePrim.Circle)
            {
                Kensei.Dev.Shape.Circle(center, radius, color);
            }

            if (isSelected && drawCoords)
            {
                if (color == Color.Black)
                    color = Color.LightGray;

                string displayString = "X: " + shape.Offset.X + ", Y: " + shape.Offset.Y;

                if (shape.PrimitiveType == CollidePrim.Circle)
                {
                    displayString += ", R: " + radius;
                }

                if (shape.PrimitiveType == CollidePrim.Rectangle)
                {
                    displayString += ", W: " + shape.GetWidth() + ", H: " + shape.GetHeight();
                }

                Kensei.Dev.DevText.Print(center - Vector2.One, displayString, Color.Black);
                Kensei.Dev.DevText.Print(center + Vector2.One, displayString, Color.Black);
                Kensei.Dev.DevText.Print(center, displayString, color);
                Kensei.Dev.DevText.Print(center, displayString, color);
            }


            //   }

            //draw the bounding box

            parentCenter = The.MapUI.WorldPosToScreen(parentCenter);
            Kensei.Dev.Shape.Box(shape.BoundsUpperLeft + shape.Offset + parentCenter,
                shape.BoundsLowerRight + shape.Offset + parentCenter, Color.Brown, false);

            return true;
        }

        /*
        public void DrawAccessPointsAndExits()
        {
            foreach (var structure in The.Sim.AllStructures) 
            {
                Vector2 center;

                if (structure.Contains != null)
                {
                    // the access point(s) assigned by an IExit always take precedence over "natural" access points
                    IExit exit = structure.Contains as IExit;
                    if (exit != null)
                    {
                        center = WorldPosToScreen(exit.AccessPoint());
                        Kensei.Dev.Shape.Circle(center, 2, Color.Azure);
               
                    }
                }

                center = WorldPosToScreen(structure.AccessPoint);
                Kensei.Dev.Shape.Circle(center, 2, Color.Yellow);
               

            }


        }*/

        /*  private void DrawSpokenLines()
          {
              Entity entity;

              if (entity.Intelligence.SpokenLine != null)
              {
                  Kensei.Dev.DevText.Print(entity.Location from, y.ToString(), Color.White);
              }

             // The.InGameUI.TalkPanel.HUDTalkPanel.
            //  Kensei.Dev.DevText.Print(from, y.ToString(), Color.White);

          }*/
        public void DrawExpeditionScoutingRadius()
        {
            foreach (var allegiance in The.Sim.PlaySite.Allegiances)
            {
                foreach (var expedition in allegiance.Expeditions)
                {
                    Vector2 center = WorldPosToScreen(expedition.Location.Value);
                    float radius = allegiance.GetForageAndHuntingRadius();
                    if (allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction == 0)
                    {
                        continue;
                    }

                    if (The.InGameUI.SelectedEntity != null)
                    {

                        Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);

                        if (entity != null && entity.AllegianceID.HasValue && (entity.AllegianceID == expedition.Allegiance.ID))
                        {
                            Kensei.Dev.Shape.Circle(center, radius, Color.Chartreuse, false);//dashed //Red
                            continue;

                        }
                    }
                    Kensei.Dev.Shape.Circle(center, radius, Color.Green, false);//dashed //White

                    //  DrawMarker(new Marker(expedition.Location, Color.Blue));
                }
            }
        }
        public void DrawRanges()
        {

            List<Pair<Entity, Vector2>> agentsOnScreen = null;
            The.AgentQuadTree.GetEntitiesInRange(GetCenterOfScreenWorldLocation(), (2f / 3f) * mapWindowWidth, null, ref agentsOnScreen);
            if (agentsOnScreen != null)
            {
                foreach (var agent in agentsOnScreen)
                {
                    Vector2 center = WorldPosToScreen(agent.First.PlaySiteLocation);

                    // attack range (dashed)
                    float? coneWidth, coneLength;
                    float attackRange = agent.First.GetAttackRange(out coneWidth, out coneLength);

                    if (attackRange > 0f)
                    {
                        Kensei.Dev.Shape.Circle(center, attackRange, Color.Yellow, true);//dashed
                    }

                    if (coneWidth.HasValue && coneLength.HasValue)
                    {
                         
                        Vector2 leftConeEdge = Common.AngleToVector(agent.First.Rotation - 0.5f * MathHelper.ToRadians(coneWidth.Value)); // coneWidth.Value);
                        leftConeEdge.Normalize();
                        Vector2 rightConeEdge = Common.AngleToVector(agent.First.Rotation + 0.5f * MathHelper.ToRadians(coneWidth.Value)); //coneWidth.Value);
                        rightConeEdge.Normalize();

                        Kensei.Dev.Shape.Line(center, center + coneLength.Value * leftConeEdge, Color.Orange);
                        Kensei.Dev.Shape.Line(center, center + coneLength.Value * rightConeEdge, Color.Orange);
                    }

                    // aggro range (red):
                    float? aggroRadius = agent.First.GetAggroRange();
                    if (aggroRadius.HasValue && aggroRadius.Value > 0)
                    {
                        Kensei.Dev.Shape.Circle(center, aggroRadius.Value, Color.Red, false);
                    }

                    // assistance range (green):
                    float? assistanceRadius = agent.First.GetAssistanceRange();
                    if (assistanceRadius.HasValue && assistanceRadius.Value > 0)
                    {
                        Kensei.Dev.Shape.Circle(center, assistanceRadius.Value, Color.Green, false);
                    }

                    // sensor range (white):
                    float visionRange = agent.First.GetVisionRange(The.Sim.DateAndTime.LightLevel);
                    Kensei.Dev.Shape.Circle(center, visionRange, Color.White, false);

                }

            }

        }

        public void DrawInterest()
        {
            foreach (var person in The.InGameUI.UIAllegiance.Persons) // The.Sim.AllOwners) 
            {
                Vector2? target = null;
                if (person.Intelligence.EntityIDToLookAt != EntityID.Invalid)
                {
                    Entity entity = Entity.FindByID(person.Intelligence.EntityIDToLookAt);
                    if (entity != null)
                    {
                        target = entity.PlaySiteLocation.ToVector2();
                    }
                }

                if (person.Intelligence.LocationToLookAt.HasValue)
                {
                    target = person.Intelligence.LocationToLookAt.Value.ToVector2();
                }

                if (target.HasValue)
                {

                    Kensei.Dev.Shape.Line(WorldPosToScreen(person.PlaySiteLocation), WorldPosToScreen(target.Value), Color.Aqua);
                }
            }
        }

        /// <summary>
        /// move bullet stuff to other class??? particles...?
        /// </summary>
        class Bullet
        {
            public Vector3 Muzzle;
            public Vector3 Impact;
            public Color Color1;
            public Color Color2;
            public Bullet(Vector3 muzzleLocation, Vector3 impactLocation, Color startColor, Color endColor)
            {
                this.Muzzle = muzzleLocation;
                this.Impact = impactLocation;
                this.Color1 = startColor;
                this.Color2 = endColor;
            }

            public void Fade()
            {
                Color1.R -= 10;
                Color1.G -= 10;
                Color1.B -= 10;
                if (Color1.R < 10 || Color1.G < 10 || Color1.B < 10)
                {
                    bullets.Remove(this);
                }
            }
        };

        static List<Bullet> bullets;

        public void DrawBullets()
        {
            for (int b = 0; b < bullets.Count; b++)
            {
                Vector2 m = WorldPosToScreen(bullets[b].Muzzle);
                m.Y -= 20;
                Vector2 i = WorldPosToScreen(bullets[b].Impact);
                Kensei.Dev.Shape.Line(m, i, bullets[b].Color1, bullets[b].Color2);

                bullets[b].Fade();
            }
        }

        public static void AddBullet(Vector3 muzzleLocation, Vector3 impactLocation, Color startColor, Color endColor)
        {
            bullets.Add(new Bullet(muzzleLocation, impactLocation, startColor, endColor));
        }

        public static void StartBulletEffect(BulletEffect bulletEffect, float? maxRange, Entity targetAsEntity, Entity attacker, bool hitTarget)
        {
            if (bulletEffect != null)
            {
                Vector3 bulletStart, bulletEnd;

                if (hitTarget)
                {
                    Vector3 dir = targetAsEntity.PlaySiteLocation - attacker.PlaySiteLocation;
                    dir.Normalize();

                    bulletStart = attacker.PlaySiteLocation + bulletEffect.MuzzleDistance * dir;
                    bulletEnd = targetAsEntity.PlaySiteLocation;
                }
                else
                {
                    // go past the target:

                    //TODO it would make sense that many MISSED attacks would end up  PAST the target, few would be in front or alongside
                    //should calculate a broad elliptical footprint with target position at the near focus
                    // http://www.coolmath.com/algebra/25-conic-sections/ellipse-foci1.gif 

                    // TODO:
                    //  ShowClientEffectsAtActionPoint(attackType, targetAsEntity, attacker);

                    float maxRangeToUse = maxRange ?? 200f;

                    Vector3 directionToTarget = targetAsEntity.PlaySiteLocation - attacker.PlaySiteLocation;
                    directionToTarget.Normalize();

                    // add a spread to the direction to simulate a missed bullet:
                    directionToTarget.X += (float)The.Client.ClientRandomGenerator.RandomNormalDistribution(0, 0.04);
                    directionToTarget.Y += (float)The.Client.ClientRandomGenerator.RandomNormalDistribution(0, 0.04);


                    float missLength = The.Client.ClientRandomGenerator.RandomBetween(0.5f * maxRangeToUse, 2f * maxRangeToUse);


                    bulletStart = attacker.PlaySiteLocation + bulletEffect.MuzzleDistance * directionToTarget;
                    bulletEnd = bulletStart + directionToTarget * missLength;

                }

                MapClient.AddBullet(bulletStart, bulletEnd,
                        bulletEffect.StartColor,
                        bulletEffect.EndColor);

            }
        }

        class Marker
        {
            public Vector3 loc;
            public Color color;
            public float size;
            public Marker(Vector3 l, Color c, float s = 5)
            {
                this.loc = l;
                this.size = s;
                this.color = c;
            }

        };

        static Dictionary<string, Marker> VisitorMarkers;

        /// <summary>
        /// used in debug info - cleared during draw
        /// </summary>
        Dictionary<object, List<Marker>> RenderedObjectMarkers = new Dictionary<object, List<Marker>>();

        public void DrawMarkers()
        {

            if (inputData.IsKeyDown(Keys.S))
                return; // hold S key to suspend drawing


            foreach (KeyValuePair<string, Marker> kvp in VisitorMarkers)
            {
                Marker m = kvp.Value;
                DrawMarker(m);

                //Markers[b].fade();
            }

            foreach (var marker in RenderedObjectMarkers)
            {
                foreach (var m in marker.Value)
                {

                    DrawMarker(m);
                }

                //Markers[b].fade();
            }

            ClearAllDebugMarkers();
        }

        private void DrawMarker(Marker m)
        {
            Vector2 ul = WorldPosToScreen(m.loc);
            Vector2 lr = ul;
            float size = m.size / 2;
            ul.X -= size;
            ul.Y -= size;
            lr.X += size;
            lr.Y += size;

            Kensei.Dev.Shape.Box(ul, lr, m.color, true);
        }

        static ulong hash = 0;
        public static void AddVisitorMarker(Vector3 l, Color c, Object key, float size = 5)
        {
            string newKey = key.GetHashCode().ToString() + (hash++);
            VisitorMarkers.Add(newKey, new Marker(l, c, size));
        }

        public void AddDebugMarker(Vector3 l, Color c, Object key, float size = 5)
        {
            List<Marker> list;
            if (!RenderedObjectMarkers.TryGetValue(key, out list))
            {
                list = new List<Marker>();
                RenderedObjectMarkers.Add(key, list);
            }

            list.Add(new Marker(l, c, size));
        }

        public static void ClearAllVisitorMarkers(Object key)
        {

            var toRemove = new List<string>();

            foreach (var item in VisitorMarkers)
            {
                if (item.Key.StartsWith(key.GetHashCode().ToString()))
                    toRemove.Add(item.Key);
            }
            foreach (var k in toRemove)
            {
                VisitorMarkers.Remove(k);
            }

            //Markers.Clear();
        }

        public void ClearAllDebugMarkers()
        {
            RenderedObjectMarkers.Clear();
        }


        public void DrawJobs() //TerrainType.TransportType transport)
        {
            Vector2 from, to;
            Color startColor = Color.White;
            Color endColor;
            /*   color.A = alpha;


               byte lowAlpha = 70;
               byte hiAlpha = 100;

               // draw subtiles???
               int maxY = mapY * 3 + noOfTilesToDisplayVertically * 3;
               int maxX = mapX * 3 + noOfTilesToDisplayHorizontally * 3;

               // byte[,] map = TerrainCosts[transport];

               int startY = mapY;
               int startX = mapX;

               if (startY % 2 == 0)
               {
                   alpha = hiAlpha;
               }
               else
               {
                   alpha = lowAlpha;
               }
           
               */

            // Dictionary<Item, Item> drawnItems = new Dictionary<Item, Item>();

            HaulingJob haulingJob;
            // foreach (var owner in The.Sim.PlaySite.AllOwners)
            // {
            LookUp<EntityGroup, EntityGroupID>.IterateMembers(
              owner =>
              {
                  if (!owner.Parent.Allegiance.Site.IsPlaySite
                      || owner.Parent.Location == null)
                  {
                      return;
                  }

                  if (owner.Parent is Person)
                  {
                      startColor = new Color(255, 0, The.Client.ClientRandomGenerator.Next(100, "MapClient", false));

                      if (((Person)owner.Parent).Parent.Location == null)
                      {
                          return;
                      }
                  }
                  else if (owner.Parent is Household)
                  {
                      startColor = new Color(0, 255, The.Client.ClientRandomGenerator.Next(100, "MapClient", false));
                  }
                  else if (owner.Parent is Expedition)
                  {
                      startColor = new Color(0, The.Client.ClientRandomGenerator.Next(50, "MapClient", false), 255);
                  }

                  Kensei.Dev.Shape.Box(WorldPosToScreen(owner.Parent.Location.Value) - new Vector2(5f, 5f), WorldPosToScreen(owner.Parent.Location.Value) + new Vector2(5f, 5f), startColor, true);

                  endColor = startColor;
                  endColor *= 0.6f;

                  foreach (Job job in owner.HaulingJobs)
                  {
                      haulingJob = job as HaulingJobSpecificItem;

                      if (haulingJob != null)
                      {
                          Entity item = Entity.FindByID(haulingJob.Item.Value);

                          if (item != null)
                          {
                              from = WorldPosToScreen(item.PlaySiteLocation);
                              Vector3 toLocation;
                              if (haulingJob.ToLocation.HasValue)
                              {
                                  toLocation = haulingJob.ToLocation.Value;
                              }
                              else
                              {
                                  Entity toStorage = Entity.FindByID(haulingJob.ToStorage.Value.StorageEntity);
                                  if (toStorage != null)
                                  {
                                      toLocation = toStorage.AccessPoint.Value;
                                  }
                                  else
                                  {
                                      toLocation = item.PlaySiteLocation;
                                  }
                              }

                              to = WorldPosToScreen(toLocation);

                              Kensei.Dev.Shape.Line(from, to, startColor, endColor);

                              if (haulingJob.TakenBy.Count > 0)
                              {
                                  to = WorldPosToScreen(haulingJob.TakenBy.Get(0).PlaySiteLocation);

                                  Kensei.Dev.Shape.Line(from, to, Color.White);
                              }
                          }
                      }
                  }

                  foreach (Job job in owner.ScoutingJobs)
                  {
                      ScoutingJob bJob = job as ScoutingJob;

                      if (bJob != null)
                      {
                          if (bJob.Location.HasValue)
                              from = WorldPosToScreen(bJob.Location.Value);
                          else
                          {
                              TerrainTile bottomLeftTile = bJob.Zone.MapArea.BottomLeftTile;
                              from = WorldPosToScreen(new Vector3(bottomLeftTile.X * 48, bottomLeftTile.Y * 48, 0));
                          }

                          DrawJob(ref from, job, Color.Aqua);
                      }
                  }

                  foreach (var job in owner.OtherJobs)
                  {
                      Vector3? circaLocation = job.GetCircaLocation();
                      if (circaLocation.HasValue)
                      {
                          Vector2 pos = circaLocation.Value.ToVector2();
                          DrawJob(ref pos, job, Color.Orange);
                      }
                  }
                 

                  foreach (var kvp in owner.ProductionJobs)
                  {
                      foreach (var job in kvp.Value)
                      {
                          ProcessJob pJob = job as ProcessJob;

                          if (pJob != null)
                          {
                              Vector3? location;
                              // IKnownEntityData siteData = null, toolData = null; 
                              if (pJob.GetCurrentJobLocation(out location) //, ref siteData, ref toolData)
                               && location.HasValue)
                              {
                                  from = WorldPosToScreen(location.Value);

                                  Kensei.Dev.Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), Color.Yellow, true);

                                  for (int i = 0; i < job.TakenBy.Count; i++)
                                  {

                                      to = WorldPosToScreen(job.TakenBy.Get(i).PlaySiteLocation);

                                      Kensei.Dev.Shape.Line(from, to, Color.White);
                                  }
                              }

                              if (pJob.HarvestJob != null)
                              {
                                  from = WorldPosToScreen(pJob.HarvestJob.Item.Container.AccessPoint);

                                  Kensei.Dev.Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), Color.Yellow, true);

                                  for (int i = 0; i < job.TakenBy.Count; i++)
                                  {

                                      to = WorldPosToScreen(job.TakenBy.Get(i).PlaySiteLocation);

                                      Kensei.Dev.Shape.Line(from, to, Color.White);
                                  }
                              }
                          }

                          /*
                          HuntingJob hJob = job as HuntingJob;

                          if (hJob != null)
                          {
                              DrawAttackJob(ref startColor, owner.GetAllegiance(), hJob);
                          }*/
                          
                      }
                  }

              });

            //foreach (KeyValuePair<string, Allegiance.Allegiance> allegiance in UWGame.SimSide.Instance.Site.Allegiances)
            foreach (Allegiance allegiance in The.Sim.PlaySite.Allegiances)
            {
                if (allegiance.AllegianceType == AllegianceType.Player)
                {
                    startColor = new Color(255, 255, 0);
                }
                else
                {
                    startColor = new Color(155, 155, 0); //new Color(0, 255, Globals.Instance.RandomPredictable.Next(100));
                }

                //   Kensei.Dev.Shape.Box(WorldPosToScreen(owner.Location) - new Vector2(5f, 5f), WorldPosToScreen(owner.Location) + new Vector2(5f, 5f), startColor, true);

                ThreatJob threatJob;
                foreach (Job job in allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs)
                {
                    threatJob = job as ThreatJob;

                    DrawAttackJob(ref startColor,
                        allegiance, threatJob);
                }

                foreach (Job job in allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobs)
                {
                    threatJob = job as ThreatJob;

                    DrawAttackJob(ref startColor,
                        allegiance, threatJob);
                }
            }


            //Vector2 from;

            int maxY = mapWindowTileY + noOfTilesToDisplayVertically + 1;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally + 1;

            maxY = The.Map.ClampTileMapYPosition(maxY);
            maxX = The.Map.ClampTileMapXPosition(maxX);

            int startY = mapWindowTileY - 1;
            int startX = mapWindowTileX - 1;

            startX = The.Map.ClampTileMapXPosition(startX);
            startY = The.Map.ClampTileMapYPosition(startY);

            //Color color = Color.Black * 0.25f;
            TerrainTile tile;
            int noOfHarvestJobsOnTile;
            for (int y = startY; y <= maxY; y++)
            {
                for (int x = startX; x <= maxX; x++)
                {
                    tile = The.Map.GetTile(x, y);

                    if (tile.HarvestJobs != null)
                    {
                        noOfHarvestJobsOnTile = tile.HarvestJobs.Sum(o => o.Value.Sum(r => r.Value.Count));

                        if (noOfHarvestJobsOnTile > 0)
                        {
                            Kensei.Dev.DevText.Print(TileEdgeToScreen(x, y), noOfHarvestJobsOnTile.ToString(), Color.White);
                        }
                    }
                }
            }

        }


       

        private void DrawAttackJob(ref Color startColor, Allegiance allegiance, AttackJob attackJob)
        {
            Vector2 from;
            if (attackJob.Target.HasValue)
            {
                IKnownEntityData targetData;
                allegiance.SharedKnowledge.GetKnownData(attackJob.Target.Value, out targetData);

                if (targetData != null)
                {
                    from = WorldPosToScreen(targetData.PlaySiteLocation);

                    DrawJob(ref from, attackJob, startColor);
                }
            }

        }

        private void DrawJob(ref Vector2 from, Job job, Color color)
        {
            Vector2 to;
            Kensei.Dev.Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), color, true);

            for (int i = 0; i < job.TakenBy.Count; i++)
            {
                to = WorldPosToScreen(job.TakenBy.Get(i).PlaySiteLocation);

                Kensei.Dev.Shape.Line(from, to, Color.White);
            }
        }



        public void DrawSectorLines()
        {
            int maxXInTiles = mapWindowTileX + noOfTilesToDisplayHorizontally;
            int maxYInTiles = mapWindowTileY + noOfTilesToDisplayVertically;

            Point startSubtilePos = MapManager.TileEdgeToSubtile(new Point(mapWindowTileX, mapWindowTileY));
            Point maxSubtilesPos = MapManager.TileEdgeToSubtile(new Point(maxXInTiles, maxYInTiles));

            System.Drawing.RectangleF screenRect = ScreenRect;
            int sectorSize = MapManager.subTileSize * MapManager.SectorSizeInSubtiles;
            for (int y = 0; y < The.Map.MapWorldHeight; y += sectorSize)
            {
                for (int x = 0; x < The.Map.MapWorldWidth; x += sectorSize)
                {
                    System.Drawing.RectangleF sectorRect = new System.Drawing.RectangleF(x, y, sectorSize, sectorSize);
                    if (sectorRect.IntersectsWith(screenRect))
                    {
                        Kensei.Dev.Shape.Box(WorldPosToScreen(new Vector2(sectorRect.X, sectorRect.Y)),
                                             WorldPosToScreen(new Vector2(sectorRect.Right, sectorRect.Bottom)), Color.White, false);
                        Kensei.Dev.Shape.Box(WorldPosToScreen(new Vector2(sectorRect.X - 1, sectorRect.Y - 1)),
                                             WorldPosToScreen(new Vector2(sectorRect.Right - 1, sectorRect.Bottom - 1)), Color.White, false);
                    }
                }
            }

        }

        public void DrawRegionMapOverlay(RegionMap regionMap)
        {
            bool drawInProgress = false;
            if (Kensei.Dev.Options.GetOption("Overlays.Render region maps in progress"))
            {
                drawInProgress = true;
            }

            int subTileSize = MapManager.subTileSize;

            byte alpha = 100;

            Vector2 from, to;
            Color color = Color.Black;
            color.A = alpha;


            byte lowAlpha = 70;
            byte hiAlpha = 100;


            int maxXInTiles = mapWindowTileX + noOfTilesToDisplayHorizontally;
            int maxYInTiles = mapWindowTileY + noOfTilesToDisplayVertically;

            Point startSubtilePos = MapManager.TileEdgeToSubtile(new Point(mapWindowTileX, mapWindowTileY));
            Point maxSubtilesPos = MapManager.TileEdgeToSubtile(new Point(maxXInTiles, maxYInTiles));


            if (startSubtilePos.Y % 2 == 0)
            {
                alpha = hiAlpha;
            }
            else
            {
                alpha = lowAlpha;
            }

            Region region;

            ushort regionColor;

            HashSet<Region> encounteredRegions = new HashSet<Region>();


            for (int y = startSubtilePos.Y; y < maxSubtilesPos.Y; y++)
            {

                for (int x = startSubtilePos.X; x < maxSubtilesPos.X; x++)
                {
                    from = SubtileEdgeToScreen(x, y);

                    if (!drawInProgress)
                    {
                        regionColor = regionMap.GetRegionColor(x, y); // subtiles!
                    }
                    else
                    {
                        regionColor = regionMap.GetRegionColorInProgress(x, y); //).newAllSubtiles[x][y]; // subtiles!
                    }

                    if (regionColor == 63)
                    {

                    }

                    if (regionColor > 0) //region != null)
                    {
                        if (!drawInProgress)
                        {
                            region = regionMap.GetRegion(regionColor);
                        }
                        else
                        {
                            region = regionMap.GetRegionInProgress(regionColor);

                        }

                        encounteredRegions.Add(region);

                        color = region.DebugColor;
                        if (regionColor < RegionMap.BaseLayerRegionColors && regionMap is DependentRegionMap)
                        {
                            color = GetFadedColor(color);
                        }


                        color.A = alpha;
                    }
                    else
                    {
                        color = Color.Transparent;
                    }

                    Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);


                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
                }

                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

            // draw region info:

            Dictionary<ushort, RegionEdge> edges;
            Region toRegion;
            foreach (Region newRegion in encounteredRegions)
            {
                if (newRegion.Color == 63)
                {

                }

                from = WorldPosToScreen(newRegion.CenterLocation);
                // print id ('color')
                Kensei.Dev.DevText.Print(from, newRegion.Color.ToString());

                // draw connections
                if (!drawInProgress)
                {
                    regionMap.RegionGraph.TryGetValue(newRegion.Color, out edges);
                }
                else
                {
                    regionMap.newRegionGraph.TryGetValue(newRegion.Color, out edges);
                }

                if (edges != null)
                {

                    // foreach (ushort edge in edges)
                    foreach (KeyValuePair<ushort, RegionEdge> kvp in edges)
                    {
                        if (!drawInProgress)
                        {
                            toRegion = regionMap.GetRegion(kvp.Key); //edge);
                        }
                        else
                        {
                            toRegion = regionMap.GetRegionInProgress(kvp.Key); //edge);
                        }

                        to = WorldPosToScreen(toRegion.CenterLocation);
                        Kensei.Dev.Shape.Line(from, to, Color.Blue);

                    }
                }
            }


        }

        private static Color GetFadedColor(Color color)
        {
            color *= 0.20f;
            return color;
        }



        public void DrawTerrainCosts(SurfaceType.TransportType transport)
        {
            int subTileSize = MapManager.subTileSize;

            byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;

            Vector2 from, to;
            Color color = Color.Black;
            color.A = alpha;

            byte cost;

            int maxY = 3 * (mapWindowTileY + noOfTilesToDisplayVertically);
            int maxX = 3 * (mapWindowTileX + noOfTilesToDisplayHorizontally);

            SubtileLayers map = The.Map.TerrainCosts[transport];

            int startY = mapWindowTileY * 3;
            int startX = mapWindowTileX * 3;

            if (startY % 2 == 0)
            {
                alpha = hiAlpha;
            }
            else
            {
                alpha = lowAlpha;
            }

            MapManager.SubtileValue value;
            bool isInPad, isReserved;
            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    from = SubtileEdgeToScreen(x, y);

                    value = map.GetValue(x, y);
                    cost = MapManager.GetCost(value);


                    switch (cost)
                    {
                        case 0://blocked
                            {
                                color = Color.Maroon;
                                color.A = alpha;
                                break;
                            }
                        case 1://car-paved
                            {
                                color = Color.LightBlue;
                                color.A = alpha;
                                break;
                            }
                        case 2://car-gravel
                            {
                                color = Color.Purple;
                                color.A = alpha;
                                break;
                            }
                        case 3://foot-normal - draw only flags
                            {
                                isInPad = MapManager.TestForFlag(value, MapManager.SubtileValue.Pad);
                                isReserved = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);

                                if (isInPad && isReserved)
                                {
                                    color = Color.LightYellow;
                                    color.A = alpha;
                                }
                                else if (isInPad)
                                {
                                    color = Color.Yellow;
                                    color.A = alpha;
                                }
                                else
                                {
                                   
                                    if (isReserved)
                                    {
                                        color = Color.White;
                                        color.A = alpha;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                break;
                            }
                        case 4://car-wheel path
                            {
                                color = Color.Magenta;
                                color.A = 200;
                                break;
                            }
                        case 5://Foot-Obstacle
                        case 6://Offroad-Obstacle
                        case 8://car-Obstacle
                            {
                                color = Color.DarkBlue;
                                color.A = 11;
                                break;
                            }
                        default:
                            {
                                color = Color.Black;
                                color.A = alpha;
                                break;
                            }

                    }


                    //Kensei.Dev.Shape.Line(from, to, color);
                    Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);

                }
                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }


            for (int y = mapWindowTileY; y < mapWindowTileY + noOfTilesToDisplayVertically; y++)
            {
                from = TileEdgeToScreen(mapWindowTileX, y);
                to = TileEdgeToScreen(mapWindowTileX + noOfTilesToDisplayHorizontally, y);

                Kensei.Dev.Shape.Line(from, to, Color.Black, Color.Black);

                Kensei.Dev.DevText.Print(from, y.ToString(), Color.White);
            }

            for (int x = mapWindowTileX; x < mapWindowTileX + noOfTilesToDisplayHorizontally; x++)
            {
                from = TileEdgeToScreen(x, mapWindowTileY);
                to = TileEdgeToScreen(x, mapWindowTileY + noOfTilesToDisplayVertically);

                Kensei.Dev.Shape.Line(from, to, Color.Black, Color.Black);

                Kensei.Dev.DevText.Print(from, x.ToString(), Color.White);
            }

            /*
            for (int y = mapY; y < mapY + noOfTilesToDisplayVertically; y++)
            {
                //yPos = y * MapManager.tileSize + MapManager.tileSizeOver2;
                for (int x = mapX; x < mapX + noOfTilesToDisplayHorizontally; x++)
                {
                    from = AbsoluteTileToScreen(x, y);
                    for (int i = 0; i < 8; i++)
                    {
                        to = from + eightDirsAsVectors[i];
                        cost = TerrainCosts[(int)transport, x, y, i];
                        if (cost == 0)
                        {
                            color = Color.Red;
                        }
                        else
                        {
                            color.R = 0;
                            color.B = (byte)Common.Clamp(cost * 50, 0, 255);
                        }
                        //Kensei.Dev.Shape.Line(from, to, color);
                        Kensei.Dev.Shape.Box( , , color, true);
                    }        
                }
            }*/
        }



        private float fogOfWarFadeRate = 0.03f;//range of 0.01 (one hundredth) to 0.1 (one tenth)
        public float FogOfWarFadeRate
        {
            get
            {
                return fogOfWarFadeRate;
            }
        }
        public void SetFogOfWarFadeRate(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            fogOfWarFadeRate = newFloat.Value;
        }


        private float fogOfWarTint = 0.7f;
        public float FogOfWarTint
        {
            get
            {
                return fogOfWarTint;
            }
        }
        public void SetFogOfWarTint(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            fogOfWarTint = newFloat.Value;
        }


        private float cloudSharpness = 5;
        public float CloudSharpness
        {
            get
            {
                return cloudSharpness;
            }
        }
        public void SetCloudEdgeHardness(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            cloudSharpness = newFloat.Value;
        }

        private float cloudOpacity = .7f;
        public float CloudOpacity
        {
            get
            {
                return cloudOpacity;
            }
        }
        public void SetCloudOpacity(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            cloudOpacity = newFloat.Value;
        }

        public void ShowFootRegionMap_OnPress(string option, bool? newBool, float? newFloat)
        {
            /* if (newBool == true)
             {
                 Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", false);
                 Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", false);
             }*/
        }

        public void ShowFoot_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", false);
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", false);
            }
        }
        public void ShowATV_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Foot)", false);
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", false);
            }
        }
        public void ShowCar_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", false);
                Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Foot)", false);
            }
        }

        public void RegionsFoot_OnPress(string option, bool? newBool, float? newFloat)
        {

        }


        #region DebugMethods


        /*  public void DrawFogOfWar(Allegiance allegiance)
        {
            int tileSize = MapManager.tileSize;

            Vector2 from;

            int maxY = mapWindowTileY + noOfTilesToDisplayVertically + 1;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally + 1;

            maxY = The.Map.ClampTileMapYPosition(maxY);
            maxX = The.Map.ClampTileMapXPosition(maxX);

            int startTileY = mapWindowTileY - 1;
            int startTileX = mapWindowTileX - 1;

            startTileX = The.Map.ClampTileMapXPosition(startTileX);
            startTileY = The.Map.ClampTileMapYPosition(startTileY);


            bool solid = (!The.Client.keyboard.IsKeyDown(Keys.F));
            Color color = solid ? (Color.Black * 0.33f) : Color.HotPink;

            int numRows = maxY - startTileY + 1;
            bool[] rowsWithFullRuns = new bool[numRows];

            //float tileStartWorldX = MapWindowWorldPosition.X

            float xStart = (float)(((startTileX - mapWindowTileX) * tileSize) - dx);

            for (int y = startTileY; y <= maxY; y++)
            {

                from = TileEdgeToScreen(startTileX, y);
                Vector2 to = new Vector2(from.X + tileSize, from.Y + tileSize);

                //begin testing for run length
                bool prevFogged = The.Map.GetTile(startTileX, y).AllegiancesThatSeeThisTile.Contains(allegiance);

               
                for (int x = startTileX; x <= maxX; x++)
                {
                    bool fogged = !The.Map.GetTile(x, y).AllegiancesThatSeeThisTile.Contains(allegiance);
                    if (fogged)//time to draw darkness here
                    {
                        if (x == maxX) // fogged on last tile  
                        {
                            to = TileEdgeToScreen(x + 1, y + 1);

                            if (from.X == xStart)
                                rowsWithFullRuns[y - startTileY] = true;
                            else
                                Kensei.Dev.Shape.Box(from, to, color, solid);
                        }
                        else if (prevFogged)//just continuing an existing run
                        {
                            to.X += tileSize;
                        }
                        else // must be we're starting a new run
                        {
                            from = TileEdgeToScreen(x, y);
                        }

                    }
                    else // this is a gap tile
                    {
                        if (prevFogged && x != startTileX) // first gap tile
                        {
                            // are these methods optimized...?
                            Kensei.Dev.Shape.Box(from, to, color, solid);
                        }


                        from = TileEdgeToScreen(x, y);
                        to = TileEdgeToScreen(x + 1, y + 1);
                    }

                    prevFogged = fogged;

                }//next x
            }//next y

            bool prevFull = false;
            bool draw = false;
            bool drawLast = false;
            Vector2 f = TileEdgeToScreen(startTileX, startTileY);
            Vector2 t = TileEdgeToScreen(maxX + 1, startTileY + 1);
            for (int b = 0; b < numRows; ++b)
            {

                if (rowsWithFullRuns[b])
                {
                    //we anchor the from corner, only if the previous row was not full
                    if (!prevFull)
                        f = TileEdgeToScreen(startTileX, startTileY + b);

                    draw = false;
                    drawLast = (b == numRows - 1);
                    prevFull = true;
                }
                else
                {
                    draw = prevFull;
                    prevFull = false;
                }

                if (draw || drawLast)
                {
                    //we anchor the to corner every time we draw
                    t = TileEdgeToScreen(maxX + 1, startTileY + b + (drawLast ? 1 : 0));
                    Kensei.Dev.Shape.Box(f, t, color, solid);
                }
            }

        }
        */

        /*  public void DrawTerrainDivisions()
          {
              TerrainTile tileToDraw;

              for (int y = firstYToDraw; y < lastYToDraw; y++)
              {
                  for (int x = firstXToDraw; x < lastXToDraw; x++)
                  {

                      Kensei.Dev.Shape.Triangle(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color);

                  }
              }

          }*/

        public void DrawResourceOverlays()
        {
            if (The.Sim.ResourceNoiseSeeds != null)
            {
                foreach (var item in ResourceOverlays)
                {
                    //DrawNoiseMap(item.Color ?? Color.Red, The.Sim.ResourceNoiseSeeds[item]);

                    Color color = item.Color ?? Color.Red;

                    // drwa the color overlay:
                    // here I add some extra parameters to the delegate. this trick is called "currying a method": http://stackoverflow.com/questions/14324803/passing-delegate-function-with-extra-parameters         

                    Tuple<NoiseParams, SimplexNoise> noise;
                    if (The.Sim.ResourceNoiseSeeds.TryGetValue(item, out noise))
                    {
                        IterateOnScreenTiles((tile) => DrawResourceNoiseOnTile(new Point(tile.X, tile.Y), color, noise));
                    }

                    // print the resource amount:
                    IterateOnScreenTiles((tile) => PrintResourceAmountOnTile(tile, item, color));

                }
            }

        }

        public void DrawDebugInfo()
        {
            bool drawAllSectorLines = false;
            if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (Foot)"))
            {
                DrawTerrainCosts(SurfaceType.TransportType.Foot);
                drawAllSectorLines = true;
            }
            else if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (ATV)"))
            {
                DrawTerrainCosts(SurfaceType.TransportType.OffRoad);
                drawAllSectorLines = true;
            }
            else if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (Car)"))
            {
                DrawTerrainCosts(SurfaceType.TransportType.Car);
                drawAllSectorLines = true;
            }
            if (Kensei.Dev.Options.GetOption("Overlays.Region map (Terrain/Foot)"))
            {
                DrawRegionMapOverlay(The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap); // The.Map.TerrainCosts[SurfaceType.TransportType.Foot][] regionMap);
                drawAllSectorLines = true;
            }
            /*
            if (game.Mode == Sim.EngineMode.Game && Kensei.Dev.Options.GetOption("Rendering.OLD Fog of war") && !Kensei.Dev.Options.GetOption("Dev.God mode"))
            {
                The.MapUI.DrawFogOfWar(The.InGameUI.UIAllegiance);
            }*/


            if (Kensei.Dev.Options.GetOption("Overlays.Crops"))
            {

                string blackpulp = "item:blackpulp";

                EntityType type;

                if (GameData.Instance.AllItemTypes.TryGetValue(blackpulp, out type))
                {
                    ResourceType resource;
                    if (GameData.Instance.ItemHarvestSource.TryGetValue(type, out resource))
                    {
                        DrawCrops(The.InGameUI.UIAllegiance, resource);
                    }
                }
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Allegiances"))
            {
                DrawAllegiances();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Jobs"))
            {

                DrawJobs();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Path search"))
            {
                DrawPathSearch();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Ranges"))
            {
                DrawRanges();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Expedition Scouting Radius"))
            {
                DrawExpeditionScoutingRadius();
            }
            if (Kensei.Dev.Options.GetOption("Overlays.Interest"))
            {
                DrawInterest();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Markers"))
            {
                DrawMarkers();
            }


            if (Kensei.Dev.Options.GetOption("Overlays.CollisionGeometries"))
            {
                DrawCollisionGeometries();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.TerrainGeometries"))
            {
                DrawTerrainGeometries();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.SelectionShapes"))
            {
                DrawSelectionShapes();
            }

            drawAllSectorLines = The.MapUI.DrawCostOverlays() || drawAllSectorLines;

            if (drawAllSectorLines)
            {
                DrawSectorLines();
            }

            if (Kensei.Dev.Options.GetOption("Overlays.Sensor tiles"))
            {
                DrawSensorTiles();
            }

            DrawResourceOverlays();

          //  DrawBullets();
        }


        private void DrawSensorTiles()
        {
            if (The.InGameUI.SelectedEntity.HasValue)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity);

                Color color = Color.Cyan;
                if (entity != null)
                {
                    Sensor sensor;
                    if (entity.Find(out sensor))
                    {
                        foreach (var item in sensor.tilesCurrentlySeen)
                        {
                            DrawTileOverlay(new Point(item.X, item.Y), color, 1f);
                        }    
                    }
                }
            }           

        }

        private List<OverlayLimit> threatMapLimits = new List<OverlayLimit>();

        public bool DrawCostOverlays()
        {
            Entity entity = null;
            if (The.InGameUI.SelectedEntity != null)
            {
                entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;               
            }

            bool drawAllSectors = false;

            foreach (object map in Overlays)
            {
                // map.GetCurrent();
                RegionMap regionMap = map as RegionMap;
                if (regionMap != null)
                {
                    DrawRegionMapOverlay(regionMap);
                    drawAllSectors = true;
                }


                InfluenceMap influenceMap = map as InfluenceMap;
                if (influenceMap != null)
                {
                    ThreatMap threatMap = map as ThreatMap;
                    if (threatMap != null)
                    {
                        threatMapLimits.Clear();
                        if (threatMap.Approach != ThreatStance.Bold
                            && entity != null)
                        {
                            GetThreatLimits(threatMapLimits, entity.Intelligence.PanicLevel);
                        }

                        DrawDiscomfortOrInfluenceMapOverlay(threatMap.Map, threatMapLimits);
                    }
                    else
                    {
                        DrawDiscomfortOrInfluenceMapOverlay(influenceMap.Map);
                    }

                    drawAllSectors = false;
                }

                DiscomfortMap discomfortMap = map as DiscomfortMap;
                if (discomfortMap != null)
                {
                    DrawDiscomfortOrInfluenceMapOverlay(discomfortMap.Map);
                    drawAllSectors = false;
                }

                MovementMap moveMap = map as MovementMap;
                if (moveMap != null)
                {
                    DrawMoveMapOverlay(moveMap);
                    drawAllSectors = true;
                }
            }

            return drawAllSectors;

        }

        public static void GetThreatLimits(List<OverlayLimit> threatMapLimits, int panicLevel) // Entity entity)
        {
            // blue: we can idle here
            threatMapLimits.Add(
                new OverlayLimit()
                {
                    Color = Color.Blue,
                    Edge = (float)GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart // 4
                });

            // violet: we will move away when idling
            threatMapLimits.Add(
               new OverlayLimit()
               {
                   Color = Color.Violet,
                   Edge = (float)GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue //16: tolerance is higher when working
               });

            // orange: we will move away when working
            threatMapLimits.Add(
                new OverlayLimit()
                {
                    Color = Color.Orange,
                    Edge = (float)panicLevel // 100
                });

            // yellow: we will flee in panic
            threatMapLimits.Add(
                new OverlayLimit()
                {
                    Color = Color.Yellow,
                    Edge = 1000f
                });
        }

        private void DrawMoveMapOverlay(MovementMap moveMap)
        {
            int subTileSize = MapManager.subTileSize;

            Vector2 from;

            byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;

            // Vector2 from, to;
            Color color = Color.Black;
            color.A = alpha;

            byte cost;

            int maxY = 3 * (mapWindowTileY + noOfTilesToDisplayVertically);
            int maxX = 3 * (mapWindowTileX + noOfTilesToDisplayHorizontally);

            SubtileLayers mapCosts = moveMap.Layers[SurfaceType.TransportType.Foot];

            int startY = mapWindowTileY * 3;
            int startX = mapWindowTileX * 3;

            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    from = SubtileEdgeToScreen(x, y);

                    int layerIndex;
                    cost = MapManager.GetCost(mapCosts.GetValue(x, y, out layerIndex));
                    if (cost == 0)
                    {
                        color = Color.Red;
                    }
                    else
                    {
                        color.R = 0;
                        color.B = (byte)Common.Clamp(cost * 50, 0, 255);
                    }

                    if (layerIndex == 0)
                    {
                        color = GetFadedColor(color);
                    }

                    color.A = alpha;

                    //Kensei.Dev.Shape.Line(from, to, color);
                    Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
                }
                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

            TileSector[][] sectors = moveMap.Children[0].Child.Map.Sectors;
            TileSector sector;
            Vector2 f, t;
            for (int x = 0; x < Common.GetJaggedArrayWidth(sectors); x++)
            {
                for (int y = 0; y < Common.GetJaggedArrayHeight(sectors); y++)
                {
                    sector = sectors[x][y];

                    if (sector != null)
                    {
                        f = TileEdgeToScreen(sector.TileArea.Left, sector.TileArea.Top);
                        t = TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Top);

                        Kensei.Dev.Shape.Line(f, t, Color.White, Color.White);

                        f = TileEdgeToScreen(sector.TileArea.Left, sector.TileArea.Bottom);
                        t = TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Bottom);

                        Kensei.Dev.Shape.Line(f, t, Color.White, Color.White);

                        f = TileEdgeToScreen(sector.TileArea.Left, sector.TileArea.Top);
                        t = TileEdgeToScreen(sector.TileArea.Left, sector.TileArea.Bottom);

                        Kensei.Dev.Shape.Line(f, t, Color.White, Color.White);

                        f = TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Top);
                        t = TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Bottom);

                        Kensei.Dev.Shape.Line(f, t, Color.White, Color.White);
                    }
                }

            }


        }



        public class OverlayLimit : IEdge
        {
            public float Edge { get; set; }
            public Color Color;
        }

        private void DrawDiscomfortOrInfluenceMapOverlay(TileLayer map, List<OverlayLimit> limits = null) // InfluenceMap influenceMap)
        {
            int tileSize = MapManager.tileSize;

            Vector2 from;
                     

            byte value;
            OverlayLimit limit = null;
            Color baseColor;

            for (int sectorY = 0; sectorY < map.SectorsAcrossHeight; sectorY++)
            {
                for (int sectorX = 0; sectorX < map.SectorsAcrossWidth; sectorX++)
                {
                    TileSector sector = map.Sectors[sectorX][sectorY];
                    if (sector != null && TileAreaIsOnScreen(sector.TileArea))
                    {
                        for (int y = 0; y < sector.TileArea.Height; y++)
                        {
                            for (int x = 0; x < sector.TileArea.Width; x++)
                            {
                                value = sector.GetValue((ushort)x, (ushort)y);

                                int index;
                                if (limits != null && limits.Count > 0)
                                {
                                    limit = Common.GetStairStepIndex((float)value, limits, out index);
                                }

                                if (limit != null)
                                {
                                    baseColor = limit.Color;
                                }
                                else
                                {
                                    baseColor = Color.Blue;
                                }

                                Color color = baseColor * Common.Clamp((value / 50f), 0f, 1f);

                                from = TileEdgeToScreen(x + sector.TileArea.Left, y + sector.TileArea.Top);

                                Kensei.Dev.Shape.Box(from, new Vector2(from.X + tileSize, from.Y + tileSize), color, true);
                            }

                        }

                        Kensei.Dev.Shape.Box(TileEdgeToScreen(sector.TileArea.Location), TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Bottom), Color.White, false);

                    }
                }
            }
        }

        public void DrawAllegiances() //Allegiance.Allegiance allegiance, ResourceType resourcetype)
        {
            int tileSize = MapManager.tileSize;

            int maxY = mapWindowTileY + noOfTilesToDisplayVertically;
            int maxX = mapWindowTileX + noOfTilesToDisplayHorizontally;

            int startY = mapWindowTileY;
            int startX = mapWindowTileX;

            //   Color color = Color.Black * 0.25f;
            Color color;

            byte value;
            float alpha;
            alpha = 0.25f;


            TerrainTile tile;
            Vector2 from;

            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    tile = The.Map.GetTile(x, y);

                    from = TileEdgeToScreen(x, y);
                    // from = SubtileEdgeToScreen(x, y);

                    // CropsMap crops = UWGame.SimSide.Instance.SharedKnowledge[entity.Intelligence.Allegiance].GetCropsMap(harvestJob.CropType); // CropsMaps[harvestJob.CropType.CropItem];

                    if (tile.AllegiancesThatSeeThisTile != null)
                    {
                        foreach (var item in tile.AllegiancesThatSeeThisTile)
                        {
                            value = (byte)((ulong)item.ID * 30); // tile.AllegiancesThatSeeThisTile cropsMap[x][y];


                            //alpha = Math.Min(1f, alpha);
                            color = item.DebugColor * alpha; // new Color((item.ID * 30), (item.ID * 20), (item.ID * 70), alpha); // Color Color.Crimson * alpha;
                            Kensei.Dev.Shape.Box(from, new Vector2(from.X + tileSize, from.Y + tileSize), color, true);

                        }
                    }
                }

            }

        }



        #endregion


        /// <summary>
        /// returns the starting edge of a subtile in screen coordinates
        /// TODO: change to use mapWindowWorldPos.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector2 SubtileEdgeToScreen(int x, int y)
        {
            return SubtileEdgeToScreen(new Point(x, y));
        }

        public Vector2 SubtileEdgeToScreen(Point subtile)
        {
            Vector3 worldPos = MapManager.SubTileEdgeToWorldPos3(subtile);

            return WorldPosToScreen(worldPos);
        }

        public Vector2 TileEdgeToScreen(Point tilePos)
        {
            return TileEdgeToScreen(tilePos.X, tilePos.Y);
        }

        public Vector2 TileEdgeToScreen(int x, int y)
        {
            Vector3 worldEdgePos = MapManager.TileEdgeToWorldPos(new Point(x, y));

            return WorldPosToScreen(worldEdgePos);

            //return new Vector2((float)(((x - mapWindowTileX) * tileSize) - dx), (float)((y - mapWindowTileY) * tileSize - dy));
        }

        public Vector2 ScreenToWorldPos(int x, int y)
        {
            return new Vector2(x + MapWindowWorldPosition.X, y + MapWindowWorldPosition.Y);
        }

        public Vector2 WorldPosToScreen(Vector3 worldPos)
        {
            return new Vector2(worldPos.X - MapWindowWorldPosition.X, worldPos.Y - MapWindowWorldPosition.Y);
            /*
            return new Vector2( this.mapX
            xScreen =  - dx + relativePosToTileCenter.X;
            yScreen = relativeTileY * tileHeight - dy + relativePosToTileCenter.Y;          */
        }

        public Vector2 WorldPosToScreen(Vector2 worldPos)
        {
            return worldPos - MapWindowWorldPosition;
            /*
            return new Vector2( this.mapX
            xScreen =  - dx + relativePosToTileCenter.X;
            yScreen = relativeTileY * tileHeight - dy + relativePosToTileCenter.Y;          */
        }

        public Point WorldPosToScreenPoint(Vector2 worldPos)
        {
            return new Point((int)(worldPos.X - MapWindowWorldPosition.X), (int)(worldPos.Y - MapWindowWorldPosition.Y));
            /*
            return new Vector2( this.mapX
            xScreen =  - dx + relativePosToTileCenter.X;
            yScreen = relativeTileY * tileHeight - dy + relativePosToTileCenter.Y;          */
        }

        public void TileCenterToScreen(int x, int y, out int xScreen, out int yScreen)
        {
            //Vector3 worldEdgePos = MapManager.TileEdgeToWorldPos(new Point(x, y));
            Vector3 worldPos = MapManager.TileToWorldPos(new Point(x, y));

            Point screenPos = WorldPosToScreenPoint(worldPos.ToVector2());

            xScreen = screenPos.X;
            yScreen = screenPos.Y;

            //   xScreen = ((x - mapWindowTileX) * MapManager.tileSize) - (int)dx + MapManager.tileSizeOver2;
            //   yScreen = (y - mapWindowTileY) * MapManager.tileSize - (int)dy + MapManager.tileSizeOver2;
        }

        /// <summary>
        /// Converts with tileX = 0 the left edge of map window
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xScreen"></param>
        /// <param name="yScreen"></param>
        public void TileToScreen(int relativeTileX, int relativeTileY, out int xScreen, out int yScreen)
        {
            Vector3 worldPos = MapManager.TileToWorldPos(new Point(relativeTileX, relativeTileY));

            Vector2 screenPos = WorldPosToScreen(new Vector2(worldPos.X, worldPos.Y));

            xScreen = (int)screenPos.X;
            yScreen = (int)screenPos.Y;

            /*int xScreen2 = (relativeTileX * MapManager.tileSize) - (int)dx;
            int yScreen2 = relativeTileY * MapManager.tileSize - (int)dy;*/
        }

        public Vector2 TilePosToScreen(Point tilePos)
        {
            return MapManager.TileToWorldPosVector2(tilePos) - MapWindowWorldPosition;
        }

        /// <summary>
        /// returns the center of the screen, clamped to world coords.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenterOfScreenWorldLocation()
        {
            Vector2 centerOfScreen = new Vector2(MapWindowWorldPosition.X + mapWindowWidth / 2,
                                                 MapWindowWorldPosition.Y + mapWindowHeight / 2);

            centerOfScreen = The.Map.ClampWorldPosition(centerOfScreen);

            return centerOfScreen;
        }


        public bool WorldPositionIsOnScreen(Vector2 pos, float border)
        {
            return pos.X >= MapWindowWorldPosition.X - border && pos.X <= MapWindowWorldPosition.X + mapWindowWidth + border
                    && pos.Y >= MapWindowWorldPosition.Y - border && pos.Y <= MapWindowWorldPosition.Y + mapWindowHeight + border;
        }

        public bool WorldPositionIsOnScreen(Vector3 pos, float border)
        {
            return pos.X >= MapWindowWorldPosition.X - border && pos.X <= MapWindowWorldPosition.X + mapWindowWidth + border
                    && pos.Y >= MapWindowWorldPosition.Y - border && pos.Y <= MapWindowWorldPosition.Y + mapWindowHeight + border;
        }

        public bool ScreenPositionIsVisible(Vector2 screenPos, int border)
        {
            if (screenPos.X >= -border && screenPos.X < mapWindowWidth + border
                && screenPos.Y >= -border && screenPos.Y < mapWindowHeight + border)
            {
                return true;
            }
            return false;

        }

        public bool TileIsOnScreen(int x, int y, int gutterSize = 0)
        {
            return x >= mapWindowTileX - gutterSize
                && y >= mapWindowTileY - gutterSize
                && x <= mapWindowTileX + noOfTilesToDisplayHorizontally + gutterSize
                && y <= mapWindowTileY + noOfTilesToDisplayVertically + gutterSize;
        }

        public bool TileAreaIsOnScreen(Rectangle area) //Point upperLeft, Point lowerRight) //int upperLeftX, int upperLeftY)
        {
            Rectangle map = new Rectangle(mapWindowTileX, mapWindowTileY, noOfTilesToDisplayHorizontally, noOfTilesToDisplayVertically);

            return map.Intersects(area);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Trees;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Map;
using WindowSystem;
using UWGame.ClientSide;

namespace UWGame.Client.MapRender
{
    /// <summary>
    /// draws the resource icons, and the flashing outline of the resource sprites. However, not the resource sprites themselves
    /// </summary>
    class MapResourceRenderer
    {
        private Rectangle iconCatchingSpriteRectangle;
        private Rectangle iconFishingSpriteRectangle;
        private Rectangle iconGatheringSpriteRectangle;

        private Rectangle iconFrameSingle;
        private Rectangle iconFrameDouble;
        private Rectangle iconFrameTriple;

        private int iconWidth;
        private int iconHalfHeight;

     //   private ResourceCategory onlyResourceCategoryTypeToRender = null;//If null we render all types

        List<RenderAsGroundSprite> groundOutlineSprites = new List<RenderAsGroundSprite>();//Has color saved in vertexstructure. Uses slow pulsing speed
      //  List<IDrawnAsGroundSprite> groundOutlineSprites = new List<IDrawnAsGroundSprite>();//Has color saved in vertexstructure. Uses slow pulsing speed
       

      //  private bool onlyRenderFlashingResources = false;

        VertexFeatureQuad[] outlineVertices;


        public void Init()
        {
            iconCatchingSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_catching");
            iconFishingSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_fishing");
            iconGatheringSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_gathering");

            iconFrameSingle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_single");
            iconFrameDouble = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_double");
            iconFrameTriple  = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_triple");


            iconWidth = iconCatchingSpriteRectangle.Width;
            iconHalfHeight = iconCatchingSpriteRectangle.Height / 2;
        }

        public void PostLoadContent()
        {
            outlineVertices = new VertexFeatureQuad[GameWorldRenderer.noOfFeatureQuads * 4];

        }

        
        public void Render(MapManager map, GameWorldRenderer renderer)
        {           
            //onlyRenderFlashingResources = false;// !The.InGameUI.ShowOverlaysAndMarkerWindows; set this to false so we dont have to select a tile to draw the resource outlines AO
            //onlyResourceCategoryTypeToRender = The.InGameUI.ResourceOutlinesToRender;

            RenderBillboardOverlays(renderer, map);
            RenderTileResourceContainerOverlays(renderer);
        }

        private void RenderBillboardOverlays(GameWorldRenderer renderer, MapManager map)
        {
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            CollectBillboardOverlays(map, renderer);
            DrawBillboardOverlays(renderer.OverlayBillboards, renderer);

            renderer.OverlayBillboards.Clear();
        }


      /*  private bool SkipThisResource(ResourceContainer tileResourceContainer, bool isInGodMode)
        {
            if (onlyResourceCategoryTypeToRender != null)
            {
                if (onlyResourceCategoryTypeToRender != tileResourceContainer.ResourceType.Category)
                {
                    return true;
                }
            }

            if (onlyRenderFlashingResources)
            {
                if (!tileResourceContainer.Renderable.IsResourceContainerFlashing())
                {
                    return true;
                }
            }

            if (tileResourceContainer.TotalHarvestableBulk <= 0) //  tileResourceContainer.Renderable.IsAllowedToRenderOverlay == false) // TODO: replace this test with seeing if the sprite is not currently rendered, its SourceRectangle will be null
            {
                return true;
            }

            

            SharedKnowledge sharedKnowledge = The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge;

            IDetectable detectable = tileResourceContainer as IDetectable;

            if (isInGodMode == false && !sharedKnowledge.AllDetectedEntities.ContainsKey(detectable))
            {
                return true;
            }

          

            return false;
        }*/

        
        private void DrawGroundOutlines()
        {
            GameWorldRenderer renderer = The.Client.Renderer;
            int groundFeatureQuadIndex = 0;
            
            groundFeatureQuadIndex = 0;
            foreach (var groundSprite in groundOutlineSprites)
            {
                // set the pulsing/flashing/tinting effect manually.
             /*   Vector4 outlineEffect = groundSprite.Renderable.GetCombinedEffects(Renderable.AdditionalEffect.Outline);

                // override the current Tint, remembering to reset afterwards, since this quad is shared with other rendering:
                groundSprite.quad.SetProperties(outlineEffect);
                */
             //   groundSprite.quad.Tint = outlineEffect;

                groundSprite.CopyQuadToVertexBuffer(renderer.groundFeatureVertices, ref groundFeatureQuadIndex, Renderable.AdditionalEffect.Outline);                
            }

            if (groundFeatureQuadIndex > 0)
            {
                renderer.DrawGroundOutlineUserVertices(groundFeatureQuadIndex);
            }
            
        }


        private void CollectBillboardOverlays(MapManager map, GameWorldRenderer renderer)
        {
            bool isInGodMode = GameWorldRenderer.GetIsInGodMode();

            foreach (var sortedList in renderer.sortedObjectsToDraw)//We go trough the objects that are going to be rendered
            {
                foreach (var drawObject in sortedList)
                {                    

                    RenderAsBillboard renderAsBillboard = drawObject as RenderAsBillboard;

                    bool renderThisTree = false;

                    if (renderAsBillboard != null
                        && renderAsBillboard.Parent != null
                        && renderAsBillboard.Parent.Entity != null)
                    {
   
                        Renderable renderable = renderAsBillboard.Parent.AsRenderable;

                        Tree tree;
                        if (renderAsBillboard.Parent.Entity.Find(out tree))
                        {
                                                        
                            if (tree != null && tree.Crops != null)
                            {
                                foreach (var crop in tree.Crops)
                                {
                                    if (The.InGameUI.OverlaySettings.DrawResource(crop.Value, isInGodMode))
                                    {
                                        renderThisTree = true;
                                        break;
                                    }

                                   /*if (SkipThisResource(crop.Value, isInGodMode))
                                    {
                                      continue;
                                   }*/

                                    /*
                                      if (crop.Value.ResourceItems.Count <= 0)
                                      {
                                          continue;
                                      }

                                      if (The.InGameUI.ResourceOutlinesToRender != null &&
                                          The.InGameUI.ResourceOutlinesToRender != crop.Value.ResourceType.Category)
                                      {
                                          continue;
                                      }


                                      if (sharedKnowledge.AllDetectedEntities.ContainsKey(crop.Value) == false)//Do we know this crop exists?
                                      {
                                          continue;
                                      }*/

                                    /*
                                    bool display = false;
                                    if (The.InGameUI.OverlaySettings.ResourceTypesToDisplay.TryGetValue(crop.Key, out display) && display)
                                    {
                                        renderThisTree = true;
                                        break;
                                    }*/
                                }
                                
                            }
                        }

                        if (renderThisTree)
                        {

                           /* if (onlyRenderFlashingResources)
                            {
                                if (!renderable.IsResourceContainerFlashing())
                                {
                                    continue;
                                }
                            }   */                        
                            

                          /*  Vector4 tint = renderable.GetCombinedOverlayEffects();
                            
                            if (renderAsBillboard.Tint != tint)
                            {
                                renderAsBillboard.Tint = tint;
                                renderAsBillboard.SetIsDirty();
                            }*/

                            renderer.OverlayBillboards.Add(renderAsBillboard);
                        }
                        
                    }
                }
            }
        }



        private void DrawBillboardOverlays(List<RenderAsBillboard> listOfBillBoards, GameWorldRenderer renderer, bool doubleSpeed = false)
        {
            int featureQuadIndex = 0;
            for (int i = 0; i < listOfBillBoards.Count; i++)
            {               
               // listOfBillBoards[i].CopyQuadToVertexBuffer(renderer.outlineVertices, ref featureQuadIndex);

                listOfBillBoards[i].CopyQuadToVertexBuffer(outlineVertices, ref featureQuadIndex, Renderable.AdditionalEffect.Outline);
                
            }

            if (featureQuadIndex > 0)
            {
                DrawOutlineBillboards(featureQuadIndex, renderer, doubleSpeed);
            }
        }

        private void DrawOutlineBillboards(int featureQuadIndex, GameWorldRenderer renderer, bool doubleSpeed = false)
        {

            Effect billboardEffect = renderer.billboardEffect;
            billboardEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);

            // draw the quads now:

            // Important - don't write to the depth buffer. Only the 3d models use it for sorting their meshes. Everything else is sorted 'manually'.
            The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;



            // render in daylight
#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Rendering.Show light amount"))
            {
                billboardEffect.CurrentTechnique = billboardEffect.Techniques["StandardDebugLighting"];
            }
#endif
            billboardEffect.CurrentTechnique = billboardEffect.Techniques["Outline"];

            
            // for normal mapping light effect:

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            billboardEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
            billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);


            foreach (EffectPass pass in billboardEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                // IMPORTANT: No state changes here without CommitChanges!
                The.Client.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, outlineVertices, 0, featureQuadIndex * 4, renderer.featureIndices, 0, featureQuadIndex * 2);

            }
        }


        // Render Tile Icons on Map
        private void RenderTileResourceContainerOverlays(GameWorldRenderer renderer)
        {           
            groundOutlineSprites.Clear();
            //groundOutlineSpritesQuickPulsing.Clear();

            bool isInGodMode = GameWorldRenderer.GetIsInGodMode();


            TerrainTile tile;
            TerrainTile[] tileColumn;
            TileIcons iconsOnTile;
            SharedKnowledge knowledgeToShow = The.InGameUI.UIAllegiance.SharedKnowledge;

            The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

          /*  for (int x = 0; x < The.MapUI.noOfTilesToDisplayHorizontally; x++)
            {
                tileColumn = map.TileMap[x + The.MapUI.mapWindowTileX];

                for (int y = 0; y < The.MapUI.noOfTilesToDisplayVertically; y++)
                {
                    tile = tileColumn[y + The.MapUI.mapWindowTileY];
            */
            for (int x = renderer.TileStartX; x <= renderer.TileEndX; x++)
            {
                tileColumn = The.Map.TileMap[x];

                for (int y = renderer.TileStartY; y <= renderer.TileEndY; y++)
                {
                    tile = tileColumn[y];

                    if (tile.TileResources != null || tile.TreesOnTile != null)//If we got resources on this tile
                    {
                        iconsOnTile = new TileIcons(true);
                        Vector2 renderPosition = new Vector2(0, 0);

                       // Animation2DPlayer playerToUse = null;
                       // Vector4 batchTint;
                        
                        foreach (var resource in tile.TileResources)
                        {
                            foreach (var item in resource.Value.ResourceItems)
                            {
                                if (item.Container.ResourceType.Name.Equals("Clamwich"))
                                { }

                                bool display = false;
                                if (The.InGameUI.OverlaySettings.ResourceTypesToDisplay.TryGetValue(item.Container.ResourceType,out display) && display)
                                {
                                    TileResourceContainer container = resource.Value;

                                    CollectGroundOutline(container, isInGodMode);

                                    CollectIconToDraw(container, isInGodMode, ref iconsOnTile, ref renderPosition); //, playerToUse, out playerToUse);
                                }
                            }
                        }

                        List<Zone> zones = tile.GetListOfZones(The.InGameUI.UIAllegiance);
                        bool isInZone = zones != null && zones.Count > 0;

                        DrawTileIcons(iconsOnTile, renderPosition, isInZone); //, batchTint); // playerToUse);//Renders the current tile and all the content on this tile that is tileResourceItems
                        
                    }
                }
            }

            The.Client.spriteBatch.End();

            DrawGroundOutlines();
        }

        /// <summary>
        /// we need to batch icons drawn using the same tint
        /// </summary>
        /// <param name="tileResourceContainer"></param>
        /// <param name="tileStruct"></param>
        /// <param name="renderPosition"></param>
        /// <param name="tint"></param>
        /// <returns></returns>
        private bool CollectIconToDraw(TileResourceContainer tileResourceContainer, bool isInGodMode,
            ref TileIcons tileStruct, 
            ref Vector2 renderPosition)
            //Animation2DPlayer currentAnimationPlayer, out Animation2DPlayer newAnimationPlayer)
        {
           // newAnimationPlayer = currentAnimationPlayer;

            
            if (tileResourceContainer.Renderable == null ||
                tileResourceContainer.Renderable.RenderAsIcon == null ||
                !The.InGameUI.OverlaySettings.DrawResource(tileResourceContainer, isInGodMode))
                //SkipThisResource(tileResourceContainer, isInGodMode))
            {
                return false;
            }
                       

            // LArs: not sure what this does..
          /*  if (tileResourceContainer.Renderable.IsUsingDefaultAnimation2DPlayer())
            {
                if (currentAnimationPlayer == null)
                {
                    newAnimationPlayer = tileResourceContainer.Renderable.GetCurrentPulsingAnimationPlayer();
                }
            }
            else
            {
                newAnimationPlayer = tileResourceContainer.Renderable.GetCurrentPulsingAnimationPlayer();
            }*/
            
            renderPosition = tileResourceContainer.AccessPoint.ToVector2();//TODO: See if we want to store the render position somewhere else than accessing it from container


            Color color = tileResourceContainer.Renderable.GetCombinedEffectsAsColor(); //GetCombinedOverlayEffectsAsColor();
            tileStruct.SetIconToRender(tileResourceContainer.Renderable.RenderAsIcon.GetIconToRender(),
                                       color); //tileResourceContainer.Renderable.GetOverlayTintColor());

            tileResourceContainer.Renderable.IsOnScreen = true; // Wake up the updatable Renderable. I know... not pretty. But it saves code...

            return true;
        }

        private void CollectGroundOutline(TileResourceContainer tileResourceContainer, bool isInGodMode)
        {

            if (tileResourceContainer.Renderable.RenderAsGroundSprite == null
                || !The.InGameUI.OverlaySettings.DrawResource(tileResourceContainer, isInGodMode))
                //|| SkipThisResource(tileResourceContainer, isInGodMode))
            {
                return; // false;
            }

        
            groundOutlineSprites.Add(tileResourceContainer.Renderable.RenderAsGroundSprite); 

           /* if (tileResourceContainer != null) //??
            {
             
                Vector4 tint = tileResourceContainer.Renderable.GetCombinedOverlayEffects();

                tileResourceContainer.Renderable.RenderAsGroundSprite.Tint = tint;  
                               
            }*/

        }
  

        
        
        /// <summary>
        /// kind of sad that this could not have been part of Renderable..
        /// </summary>
        /// <param name="tileIcons"></param>
        /// <param name="renderPosition"></param>
        /// <param name="isInZone"></param>
        /// <param name="player"></param>
        private void DrawTileIcons(TileIcons tileIcons, Vector2 renderPosition, bool isInZone) //, Vector4 tint) // Animation2DPlayer player)
        {
           /* if (player == null)
            {
                return;
            }*/

          //  The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Color colorToRenderWith = Color.White;
            Rectangle rect = new Rectangle();
            Point screenPos = The.MapUI.WorldPosToScreenPoint(renderPosition);
            tileIcons.SortIcons();

            for (int renderIndex = 0; renderIndex < tileIcons.iconsToRender.Count; renderIndex++) //This one will max loop 3 times
            {

                Rectangle sourcerect;

                switch (tileIcons.iconsToRender[renderIndex])
                {
                    case IconToRender.Bug:
                        sourcerect = iconCatchingSpriteRectangle;
                        colorToRenderWith = tileIcons.bugColor;
                        break;
                    case IconToRender.Hook:
                        sourcerect = iconFishingSpriteRectangle;
                        colorToRenderWith = tileIcons.hookColor;
                        break;
                    case IconToRender.Ore:
                        sourcerect = iconGatheringSpriteRectangle;
                        colorToRenderWith = tileIcons.oreColor;
                        break;
                    default:
                        return;
                }

                rect.Height = sourcerect.Height;
                rect.Width = sourcerect.Width;

                int renderOffset = 0;
                switch (renderIndex)
                {
                    case 0:
                        renderOffset = 0;
                        break;
                    case 1:
                        renderOffset = iconWidth;
                        break;
                    case 2:
                        renderOffset = -iconWidth;
                        break;
                }
                rect.X = screenPos.X - renderOffset - (iconWidth/2);
                rect.Y = screenPos.Y - iconHalfHeight;

                if (tileIcons.iconsToRender.Count == 2)
                {
                    rect.X += (iconWidth / 2); //We want not to render any of the objects in the middle if 2
                }

               /* Color color;
                
                color = player.GetCurrentColor(colorToRenderWith);
                */

               // Color color = new Color(colorToRenderWith.ToVector4() * tint);

                The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, rect, sourcerect, colorToRenderWith); // color);

            }

            // draws the frame around the image icons, but only when no zone is present:
            if (tileIcons.iconsToRender.Count > 0 && isInZone == false)
            {
                Rectangle frameRectangle = new Rectangle();
                Rectangle sourceRect = new Rectangle();

                switch (tileIcons.iconsToRender.Count)
                {
                    case 1:
                        sourceRect = iconFrameSingle;
                        break;
                    case 2:
                        sourceRect = iconFrameDouble;
                        break;
                    case 3:
                        sourceRect = iconFrameTriple;
                        break;
                }

                frameRectangle.Width = sourceRect.Width;
                frameRectangle.Height = sourceRect.Height;
                frameRectangle.X = screenPos.X - (frameRectangle.Width / 2);
                frameRectangle.Y = screenPos.Y - (frameRectangle.Height / 2);

                The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, frameRectangle, sourceRect, The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color.White));

            }

          //  The.Client.spriteBatch.End();
        }


        private struct TileIcons
        {
            public List<IconToRender> iconsToRender;

            public Color oreColor;
            public Color hookColor;
            public Color bugColor;


            public TileIcons(bool doInit)
            {
                if (doInit == true)
                {
                    iconsToRender = new List<IconToRender>();
                }
                else
                {
                    iconsToRender = null;
                }

                oreColor = Color.White;
                hookColor = Color.White;
                bugColor = Color.White;
            }

            public void SetIconToRender(IconToRender aIcon, Color aColor)
            {
                //Logic is currently designed for 1 to 3 icons for each tile. 
                //This could be done datadriven but then we would need new assets that works to use in a data driven way.

                for (int i = 0; i < iconsToRender.Count; i++)
                {
                    if (aIcon == iconsToRender[i])
                    {
                        return;
                    }

                }

                switch (aIcon)
                {
                    case IconToRender.Bug:
                        bugColor = aColor;
                        break;
                    case IconToRender.Hook:
                        hookColor = aColor;
                        break;
                    case IconToRender.Ore:
                        oreColor = aColor;
                        break;
                }

                if (aIcon != IconToRender.None)
                {
                    iconsToRender.Add(aIcon);
                }
            }

            public void SortIcons() //List<IconToRender> iconsToRender) //This will sort up to 3 items in an icontorender list
            {
                if (iconsToRender.Count > 1)
                {
                    IconToRender bufferIcon;
                    if (iconsToRender.Count == 3)
                    {
                        if (iconsToRender[0] > iconsToRender[1])
                        {
                            bufferIcon = iconsToRender[0];
                            iconsToRender[0] = iconsToRender[1];
                            iconsToRender[1] = bufferIcon;
                        }
                        if (iconsToRender[1] > iconsToRender[2])
                        {
                            bufferIcon = iconsToRender[1];
                            iconsToRender[1] = iconsToRender[2];
                            iconsToRender[2] = bufferIcon;
                        }
                        if (iconsToRender[0] > iconsToRender[1])
                        {
                            bufferIcon = iconsToRender[0];
                            iconsToRender[0] = iconsToRender[1];
                            iconsToRender[1] = bufferIcon;
                        }
                    }
                    else
                    {
                        if (iconsToRender[0] > iconsToRender[1])
                        {
                            bufferIcon = iconsToRender[0];
                            iconsToRender[0] = iconsToRender[1];
                            iconsToRender[1] = bufferIcon;
                        }
                    }
                }
            }

        }

    }
}
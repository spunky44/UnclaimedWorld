using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.Control;

namespace UWGame.ClientSide.Interface.MapGUI
{
    /// <summary>
    /// This represents the animated selection circle
    /// </summary>
    public class Selection
    {
        Animation2DPlayer selectedCirclePlayer;
        Animation2DPlayer hoverCirclePlayer;

        Animation2D mediumSelection;
        Animation2D mediumSelectionLoop;

        Animation2D smallSelection;
        Animation2D smallSelectionLoop;
               
        private InGameInterface intf = The.InGameUI;

        private Regulator selectionUpdateRegulator = new Regulator(The.Client.ClientRandomGenerator, 4, "SelectionSelection");
        private Regulator hoverUpdateRegulator = new Regulator(The.Client.ClientRandomGenerator, 4, "SelectionHover");

        Color selectionTintingColor = Color.White;

        /// <summary>
        /// the animated color
        /// </summary>
        Color selectionCurrentColor;


        /// <summary>
        /// the color that we use for hover
        /// </summary>
        public Color HoverTintingColor = Color.White;

        /// <summary>
        /// the animated hover color
        /// </summary>
        Color hoverCurrentColor;

       
        Rectangle selectionSourceRect, hoverSourceRect;
        SelectionSprite selectionSprite;
        SelectionSprite hoverSprite;

        OverlayGroundSpriteQuad quad;

        // #5186B7
        private Color hoverCircleFullTintColor = new Color(81, 134, 183);
        private Color hoverCircleEmptyTintColor = new Color(137, 168, 196);

        private enum SelectionSprite { Small, Medium }

        public Selection()
        {
 
            quad = new OverlayGroundSpriteQuad();            
        }

        public void PostLoadContent()
        {
            // sprite sheets are loaded by now.
            mediumSelection = new Animation2D(The.Client.FlatSpriteSheet.Texture, 0.08f, false);
            mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame2")){ Color = Animation2D.transp});
            mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame1")) { Color = Animation2D.halfTransp });
            mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame3")));
            mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame2"))); // repeat!
            mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));

            mediumSelectionLoop = new Animation2D(The.Client.FlatSpriteSheet.Texture, 1f, true);
            mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));
            mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")) { Color = Animation2D.thirdTransp });
            mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));

            smallSelection = new Animation2D(The.Client.FlatSpriteSheet.Texture, 0.08f, false);
            smallSelection.Cells.Add(new Cell() { Frame = The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1"), Color = Animation2D.transp });
            smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame2")) { Color = Animation2D.halfTransp });
            smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame3")));
            smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1"))); // repeat!
            smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));

            smallSelectionLoop = new Animation2D(The.Client.FlatSpriteSheet.Texture, 1f, true);
            smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));
            smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")) { Color = Animation2D.thirdTransp });
            smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));
            
            selectedCirclePlayer = new Animation2DPlayer();

            hoverCirclePlayer = new Animation2DPlayer();
            
        }

        public void SelectEntity(IKnownEntityData selectedEntity)
        {
            SetupSelectionDimensionsAndColor(selectedEntity, out selectionSourceRect, out selectionSprite, /*out selectionTintingColor,*/ false);

          
            if (selectionSprite == SelectionSprite.Small)
            {
                selectedCirclePlayer.StartAnimation(smallSelection);
                selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_smallSelectionAnimationEndedEvent);
     
            }
            else if (selectionSprite == SelectionSprite.Medium)
            {
                selectedCirclePlayer.StartAnimation(mediumSelection);
                selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_mediumSelectionAnimationEndedEvent);
     
            }
        }

        public void StartHoverOverEntity(Color? hoverTintingColor = null)
        {           
            if (intf.HoverEntity.HasValue)
            { 
                IKnownEntityData data = null;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out data);
                if (data != null)
                {
                    SetupSelectionDimensionsAndColor(data, out hoverSourceRect, out hoverSprite, true, hoverTintingColor);

                    if (hoverSprite == SelectionSprite.Small)
                    {
                        hoverCirclePlayer.StartAnimation(smallSelectionLoop);
                        // selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_smallSelectionAnimationEndedEvent);

                    }
                    else if (hoverSprite == SelectionSprite.Medium)
                    {
                        hoverCirclePlayer.StartAnimation(mediumSelectionLoop);
                        // selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_mediumSelectionAnimationEndedEvent);

                    }
                }
            }
        }

        public void StartHoverOverTile(Color? hoverTintingColor = null)
        {
            SetupSelectionDimensionsAndColor(null, out hoverSourceRect, out hoverSprite, true, hoverTintingColor);

            hoverCirclePlayer.StartAnimation(smallSelectionLoop);
            /*
            if (hoverSprite == SelectionSprite.Small)
            {
                hoverCirclePlayer.StartAnimation(smallSelectionLoop);
                // selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_smallSelectionAnimationEndedEvent);

            }
            else if (hoverSprite == SelectionSprite.Medium)
            {
                hoverCirclePlayer.StartAnimation(mediumSelectionLoop);
                // selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_mediumSelectionAnimationEndedEvent);

            }*/
        }

       /* public void SelectTile()
        {
            selectedCirclePlayer.StartAnimation(smallSelection);
            selectedCirclePlayer.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(player_smallSelectionAnimationEndedEvent);
        }*/

        void player_smallSelectionAnimationEndedEvent()
        {
            selectedCirclePlayer.StartAnimation(smallSelectionLoop);
            selectedCirclePlayer.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(player_smallSelectionAnimationEndedEvent);
        }

        void player_mediumSelectionAnimationEndedEvent()
        {
            selectedCirclePlayer.StartAnimation(mediumSelectionLoop);
            selectedCirclePlayer.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(player_mediumSelectionAnimationEndedEvent);
        }

        public void Update(GameTime gameTime)
        {
            if (intf.SelectedEntity != null)
            {
                if (selectionUpdateRegulator.IsReady())
                {
                    IKnownEntityData data;
                    The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.SelectedEntity.Value, out data);
                    if (data != null)
                    {
                        SetupSelectionDimensionsAndColor(data, out selectionSourceRect, out selectionSprite, /*out selectionTintingColor,*/ false);
                    }
                }
            }

            if (intf.HoverEntity != null)
            {
                if (hoverUpdateRegulator.IsReady())
                {
                    IKnownEntityData data;
                    The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out data);
                    if (data != null)
                    {
                        SetupSelectionDimensionsAndColor(data, out hoverSourceRect, out hoverSprite, /*out hoverTintingColor,*/ true);
                    }
                }
            }

            selectedCirclePlayer.Update(gameTime);
            hoverCirclePlayer.Update(gameTime);
        }

        /// <summary>
        /// entity can be null if we want to hover/select tiles...
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="sourceRect"></param>
        /// <param name="sprite"></param>
        /// <param name="isHover"></param>
        /// <param name="hoverTintingColor"></param>
        private void SetupSelectionDimensionsAndColor(IKnownEntityData entityData, out Rectangle sourceRect, out SelectionSprite sprite, bool isHover, Color? hoverTintingColor = null)
        {
            Color tintingColor;

            float radius = MapManager.tileSizeOver2;

            if (entityData != null)
            {
                radius = Entity.GetSelectionRadius(entityData);
            }

            bool containsIntelligence = false;
            Entity entity = entityData as Entity;
            if (entity != null)
            {
                List<IKnownEntityData> agentsInside = null;
                   containsIntelligence = Entity.ContainsIntelligence(entityData, 
                       The.InGameUI.UIAllegiance.SharedKnowledge, ref agentsInside);
             
            }

            if (isHover)
            {
                if (hoverTintingColor.HasValue)
                {
                    tintingColor = hoverTintingColor.Value;
                }
                else
                {
                    if (entityData != null)
                    {
                        tintingColor = (containsIntelligence ? hoverCircleFullTintColor : hoverCircleEmptyTintColor);
                    }
                    else
                    {
                        tintingColor = hoverCircleEmptyTintColor;
                    }
                }
            }
            else
            {
                tintingColor = (containsIntelligence ? Color.Gold : Color.White);
            }

            if (isHover)
            {
                this.HoverTintingColor = tintingColor;
            }
            else
            {
                this.selectionTintingColor = tintingColor;
            }

            if (radius < MapManager.tileSize)
            {
                sourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1");
                sprite = SelectionSprite.Small;
            }
            else
            {
                sourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame1");
                sprite = SelectionSprite.Medium;
            }
        }



        public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index) //SpriteBatch spriteBatch) 
        {
            int xScreen, yScreen;
            Rectangle? destination = null;
           // Color color = Color.White;

            if (selectedCirclePlayer.Animation != null)
            {
               /* if (intf.SelectedTiles.Count == 1) // intf.SelectedTiles != null)
                {
                    TerrainTile selTile = intf.SelectedTiles.GetFirst();

                    if (The.MapUI.TileIsOnScreen(selTile.X, selTile.Y))
                    {
                        Rectangle source = The.Client.FlatSpriteSheet.SourceRectangle("smallcircle_frame1");
                        The.MapUI.AbsoluteTileCenterToScreen(selTile.X, selTile.Y, out xScreen, out yScreen);

                        destination = new Rectangle(xScreen - source.Width / 2, yScreen - source.Height / 2, source.Width, source.Height);

                        //player.Draw(spriteBatch, destination.Value, color);

                        selectionCurrentColor = selectedCirclePlayer.GetCurrentColor(selectionTintingColor);

                        quad.SetupQuadVertices(destination.Value.Left, destination.Value.Top, destination.Value.Right, destination.Value.Bottom,
                            selectedCirclePlayer.GetFrame(), The.Client.FlatSpriteSheet.Texture, true, selectionCurrentColor.ToVector4());


                        quad.CopyQuadToVertexBuffer(overlayVertices, index);
                        index++;
                    }
                }*/


                if (intf.SelectedEntity != null)
                {
                    IKnownEntityData data;
                    The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.SelectedEntity.Value, out data);

                    if (!InGameInterface.CanSelectEntity(data))
                    {
                        // deselect:
                        The.InGameUI.SelectEntity(null);                      
                        return;
                    }

                    Vector2 screenPos = The.MapUI.WorldPosToScreen(data.PlaySiteLocation); //Location);

                    Entity entity = data as Entity;
                   
                    if (entity != null)
                    {
                        Entity root = entity.GetRootAndContainer();

                        if (root.Renderable != null && root.Renderable.RenderAsModel != null)
                        {
                            screenPos = The.MapUI.WorldPosToScreen(root.Renderable.RenderAsModel.Location);
                        }
                    }


                    destination = new Rectangle(
                        (int)(screenPos.X - selectionSourceRect.Width / 2),
                        (int)(screenPos.Y - selectionSourceRect.Height / 2),
                        selectionSourceRect.Width, selectionSourceRect.Height);

                    selectionCurrentColor = selectedCirclePlayer.GetCurrentColor(selectionTintingColor);

                    quad.SetupQuadVertices(destination.Value.Left, destination.Value.Top, destination.Value.Right, destination.Value.Bottom,
                            selectedCirclePlayer.GetFrame(), The.Client.FlatSpriteSheet.Texture, true, selectionCurrentColor.ToVector4());


                    quad.CopyQuadToVertexBuffer(overlayVertices, index);
                    index++;


                }
            }

            if (hoverCirclePlayer.Animation != null)
            {
                Vector2? screenPos = null;
                if (intf.HoverEntity != null && intf.HoverEntity != intf.SelectedEntity)
                {
                    IKnownEntityData entityData;
                    The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out entityData);

                    if (entityData == null
                       || entityData.Location == null)
                    {
                        // deselect:
                        intf.SetHoverEntity(null);
                       // The.InGameUI.SelectedEntity = null;
                        return;
                    }

                    if (entityData != null)
                    {
                        screenPos = The.MapUI.WorldPosToScreen(entityData.PlaySiteLocation); //GetLocationWhenInVehicle()); //Location);

                        Entity entity = entityData as Entity;
                        if (entity != null && entity.Renderable != null && entity.Renderable.RenderAsModel != null)
                        {
                            screenPos = The.MapUI.WorldPosToScreen(entity.Renderable.RenderAsModel.Location);
                        }
                    }       
                
                }
                else if (intf.HoverTile != null)
                {
                    screenPos = The.MapUI.TilePosToScreen(new Point(intf.HoverTile.X, intf.HoverTile.Y));                    
                }

                if (screenPos.HasValue)
                {
                    destination = new Rectangle(
                            (int)(screenPos.Value.X - hoverSourceRect.Width * 0.5f),
                            (int)(screenPos.Value.Y - hoverSourceRect.Height * 0.5f),
                            hoverSourceRect.Width, hoverSourceRect.Height);

                    hoverCurrentColor = hoverCirclePlayer.GetCurrentColor(HoverTintingColor);

                    quad.SetupQuadVertices(destination.Value.Left, destination.Value.Top, destination.Value.Right, destination.Value.Bottom,
                            hoverCirclePlayer.GetFrame(), The.Client.FlatSpriteSheet.Texture, true, hoverCurrentColor.ToVector4());


                    quad.CopyQuadToVertexBuffer(overlayVertices, index);
                    index++;
                }
            }
                     
        }

    }
}

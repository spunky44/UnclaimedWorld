using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Interface;
using WindowSystem;
using UWGame.SimSide.Trees;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame;

namespace UWGame.ClientSide.Renderables 
{                                           
    // The use of this class as a generic billboard asset solution is wrong
    // this class is an entity component. Some other class should be the GameObject solution. -MLo
    public class RenderAsBillboard: ILocatable, IUpdatable // GameObject //
    {
        /// <summary>
        /// cached tile position for quick culling when rendering.
        /// </summary>
        public Point? MapPosition;

        /// <summary>
        /// is never null
        /// </summary>
        protected FeatureQuad quad = new FeatureQuad();

        /// <summary>
        /// The shadow quad is separate, becuase it is cut off from the base center and down.
        /// </summary>
        protected FeatureQuad shadowQuad = new FeatureQuad();

        protected OverlayQuad overlayQuad = new OverlayQuad();

        private bool spriteIsDirty = true;
        private bool overlaySpriteIsDirty = true;

        /// <summary>
        /// tint, bendyness...
        /// </summary>
        private bool propertiesAreDirty = true;

        private bool overlayPropertiesAreDirty = true;


        public RenderAsBillboardType renderAsBillboardType;
     
        public Renderable Parent;

        public Renderable AsRenderable
        {
            get 
            {
                return null;
            }
        }

        public RenderAsBillboard AsRenderAsBillboard
        {
            get
            {
                return this;
            }
        }

        public Vector3 Location
        {
            get; set;
        }


        //ctor
        public RenderAsBillboard(Renderable parent, RenderAsBillboardType renderAsBillboardType)
        {
            this.Parent = parent;
            this.renderAsBillboardType = renderAsBillboardType;

            
        }

     /*   public RenderAsBillboard(RenderAsBillboard original)
        {
            MapPosition = original.MapPosition;

            Location = original.Location; // New - why not earlier..?

            // make sure we are up to date:
            original.Parent.UpdateStaticConditionState();

            if (original.spriteIsDirty && original.quad != null)
            {
                original.SetupQuadVertices();
            }
            if (original.overlaySpriteIsDirty && original.overlayQuad != null)
            {
                original.SetupOverlayQuadVertices();
            }

            spriteIsDirty = false;
            overlaySpriteIsDirty = false;

            quad = new FeatureQuad(original.quad);
            shadowQuad = new FeatureQuad(original.shadowQuad);

            // only copy overlay?
            overlayQuad = new OverlayQuad(original.overlayQuad);

           
            renderAsBillboardType = original.renderAsBillboardType;

        }*/

       


       /* public bool FlipNormalMapAndLights
        {
            set 
            {
                quad.FlipNormalMap = value;
                shadowQuad.FlipNormalMap = value; // unnecessary?
            }
        }*/

      /*  private bool FlipSprite
        {
            set
            {
                quad.FlipSprite = value;
                shadowQuad.FlipSprite = value; 
            }
        }*/

       /* public Vector4 Tint
        {
            set
            {
                quad.Tint = value;
                shadowQuad.Tint = value;

            }
            get
            {
                return quad.Tint;
            }
        }*/


        /// <summary>
        /// either be in Renderable, or be pulled from Tree(Type)...
        /// </summary>
       /* private float Bendyness 
        {
            get
            {
                if (Parent.Entity != null
                    && Parent.Entity.EntityType.TreeType != null)
                {
                    // perhaps some structures must also sway..?
                     return Parent.Entity.EntityType.TreeType.Bendyness;
                }

                return 0f;
            }
        }*/
/*
        /// <summary>
        /// used in Bendyness animation
        /// </summary>
        public float RandomValue 
        {
            set
            {
                quad.RandomValue = value;
                shadowQuad.RandomValue = value;
            }
        }*/

        /// <summary>
        /// to be called when position or image changes
        /// </summary>
        public void SetIsDirty()
        {
            spriteIsDirty = true;

            overlaySpriteIsDirty = true;
        }


        public void SetPropertiesAreDirty()
        {
            propertiesAreDirty = true;
        }

        public void SetOverlayPropertiesAreDirty()
        {
            overlayPropertiesAreDirty = true;
        }

       /* public void Redraw()
        {
            isDirty = true;
            overlayQuadIsDirty = true;
        }*/

       

       /* public void Redraw(Rectangle? spriteRect, RenderAsBillboardType billboardType)
        {
            this.renderAsBillboardType = billboardType;

            Texture2D texture = GameData.Instance.BillboardSpriteSheet.Texture;

            quad.SetStaticFrame(spriteRect, texture);
            shadowQuad.SetStaticFrame(spriteRect, texture);

            spriteIsDirty = true;
            overlaySpriteIsDirty = true;
        }*/

        /// <summary>
        /// never used..??
        /// </summary>
        /// <param name="spritesheet"></param>
        public void Redraw(SpriteSheetRuntime.SpriteSheet spritesheet, RenderAsBillboardType billboardType, bool drawAsOverlay) 
        {
            this.renderAsBillboardType = billboardType;

            if (spritesheet != null
                && !string.IsNullOrEmpty(renderAsBillboardType.AssetName))
            {

                Texture2D texture = spritesheet.Texture;

                Rectangle sourceRect = spritesheet.GetSourceRectangle(renderAsBillboardType.AssetName);

                if (renderAsBillboardType.AssetName == "clayGranary_construct")
                {

                }

                quad.SetStaticFrame(sourceRect, texture);
                shadowQuad.SetStaticFrame(sourceRect, texture);

                if (drawAsOverlay)
                {
                    RedrawOverlay(sourceRect);

                    overlayPropertiesAreDirty = true; // this will pull gradient colors etc.
                }

            }
            else if (!string.IsNullOrEmpty(renderAsBillboardType.AnimationAssetName))
            {
                // start a billobard animation
                Animation2D anim = GameData.Instance.Animation2Ds[renderAsBillboardType.AnimationAssetName];

                quad.SetAnimationFrames(anim);
                shadowQuad.SetAnimationFrames(anim);

                quad.Player.FrameChangedEvent += new WindowSystem.Animation2DPlayer.FrameChanged(Player_FrameChangedEvent);

            }
            else
            {
                // don't render anything:
                Texture2D texture = spritesheet.Texture;

                quad.SetStaticFrame(null, texture);
                shadowQuad.SetStaticFrame(null, texture);

                if (drawAsOverlay)
                {
                    RedrawOverlay(null);
                }
            }
            

          /*  string spriteName = billboardType.AssetName;

            if (!string.IsNullOrEmpty(spriteName))
            {
                rectangle = spritesheet.GetSourceRectangle(spriteName);
            }
            else if (!string.IsNullOrEmpty(billboardType.AnimationAssetName))
            {
                billboard.Redraw(rectangle, billboardType); // should also set the type.

            }
            else
            {
                // don't draw anything
                rectangle = null;
            }

            billboard.Redraw(spriteSheet, billboardType);

            billboard.Redraw(rectangle, billboardType); // should also set the type.
            */


            spriteIsDirty = true;
            overlaySpriteIsDirty = true;
        }


        public void RedrawOverlay(Rectangle? spriteRect)
        {
            
            Texture2D texture = The.Client.Renderer.GhostedStructuresSpriteSheet.Texture;

            if (overlayQuad == null)
            {                
                overlayQuad = new OverlayQuad();                          
                //overlayQuad.FlipSprite = quad.FlipSprite;
            }

            overlayQuad.SetStaticFrame(spriteRect, texture);

          
            spriteIsDirty = true;
            overlaySpriteIsDirty = true;
        }

      /*  public void RedrawOverlayCycleAnimation(Rectangle spriteRect)
        {
            Texture2D texture = The.Client.Renderer.GhostedStructuresSpriteSheet.Texture;

            Animation2D animation = new WindowSystem.Animation2D(texture, 0.6f, true)
            {
                DoColorInterpolation = true, // false,
                Cells = new List<WindowSystem.Cell>() 
                { 
                    new Cell(spriteRect),
                    new Cell(spriteRect) { Color = Animation2D.halfTransp },
                    new Cell(spriteRect) { Color = Color.White
                    }                
                }
            };

            StartOverlayAnimation(animation);
        }

        public void RedrawOverlayPlaceAnimation(Rectangle spriteRect)
        {
            Texture2D texture = The.Client.Renderer.GhostedStructuresSpriteSheet.Texture;

            Animation2D animation = new WindowSystem.Animation2D(texture, 1f / 12f, false)
            {
                DoColorInterpolation = false, 
                Cells = new List<WindowSystem.Cell>() { new Cell(spriteRect),
                new Cell(spriteRect) { Color = Animation2D.transp },
                new Cell(spriteRect) { Color = Color.White},
                new Cell(spriteRect) { Color = Animation2D.transp },
                new Cell(spriteRect) { Color = Color.White}
                }
            };

            StartOverlayAnimation(animation);
        }*/



        public void SetOverlayGradientColors(Color gradient1, Color gradient2, Color gradient3)
        {
            overlayQuad.GradientColor1 = gradient1.ToVector4();          
            overlayQuad.GradientColor2 = gradient2.ToVector4();           
            overlayQuad.GradientColor3 = gradient3.ToVector4();

            overlaySpriteIsDirty = true;
        }
        

        /// <summary>
        /// deleted, replaced with effect on Renderable
        /// </summary>
        /// <param name="animation"></param>
      /*  private void StartOverlayAnimation(Animation2D animation)
        {
            
            if (overlayQuad == null)
            {
                overlayQuad = new OverlayQuad();
                overlayQuad.FlipSprite = quad.FlipSprite;
            }

            overlayQuad.Player.StartAnimation(animation);

            // not currently used... trigger redraw for animated images.
            overlayQuad.Player.FrameChangedEvent += new WindowSystem.Animation2DPlayer.FrameChanged(Player_FrameChangedEvent);
            
            // make sure that we get the ending color when finished even if frames are skipped:
            overlayQuad.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(overlayQuad.UpdateVerticeColorFromPlayer);

            spriteIsDirty = true;
            overlaySpriteIsDirty = true;
           
        }*/



        public void CopyShadowQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, ref int index)
        {
            if (spriteIsDirty)
            {
                SetupQuadVertices();
            }

            if (shadowQuad.CopyQuadToVertexBuffer(featureVertices, index))
            {
                index++;
            }
        }
        

        /// <summary>
        /// the 'Draw' method
        /// </summary>
        /// <param name="featureVertices"></param>
        /// <param name="index"></param>
        public void CopyQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null)
        {
            if (spriteIsDirty)
            {
                // recompute quad coordinates:
                SetupQuadVertices();
            }
            else if (propertiesAreDirty)
            {
                // set properties on the quad:
                SetQuadProperties();
            }

            if (overridingEffectID.HasValue)
            {             
                Vector4 overridingEffect = Parent.GetCombinedEffects(overridingEffectID.Value);

                quad.SetTint(overridingEffect);
            }

            if (quad.CopyQuadToVertexBuffer(featureVertices, index))
            {
                index++;
            }

            // reset the old tint:
            if (overridingEffectID.HasValue)
            {
                SetQuadProperties();
            }
            
        }

        /// <summary>
        /// the 'Draw' method for the overlay quad
        /// </summary>
        /// <param name="featureVertices"></param>
        /// <param name="index"></param>
        public void CopyOverlayQuadToVertexBuffer(VertexOverlayQuad[] overlayVertices, ref int index)
        {
            if (overlaySpriteIsDirty)
            {
                SetupOverlayQuadVertices();
            }
            else if (overlayPropertiesAreDirty)
            {
                // set properties on the quad:
                SetOverlayQuadProperties();
            }

            overlayQuad.CopyQuadToVertexBuffer(overlayVertices, index);
            
            index++;
        }


        private void SetupOverlayQuadVertices()
        {
            float posLeft;
            float posRight;
            float posTop;
            float posBottom;
            Vector3 baseCenterWorld;
            Vector2 baseCenterOffset;

            SetupDimensions(out baseCenterOffset, out posLeft, out posRight, out posTop, out posBottom, out baseCenterWorld);

          
            overlayQuad.SetupQuadVertices(baseCenterWorld, posLeft, posTop, posRight, posBottom, /*sourceRect, texture,*/ true, Parent.FlipHorizontally);
           
            overlaySpriteIsDirty = false;
        }

        void Player_FrameChangedEvent()
        {
            // change the texture coordinates on the quad vertices:
            spriteIsDirty = true;
            overlaySpriteIsDirty = true;

        }

        /// <summary>
        /// Billboard position changes have to be applied immendiately. GameWorldRenderer uses this position in culling and sorting.
        /// Corner vertices can wait, however.
        /// </summary>
        public void ComputeMapPosition()
        {
            Vector2 baseCenterOffset;

            float posLeft;
            float posRight;
            float posTop;
            float posBottom;
            Vector3 baseCenterWorld;
            
            SetupDimensions(out baseCenterOffset, out posLeft, out posRight, out posTop, out posBottom, out baseCenterWorld);
        }
      

        private void SetupQuadVertices()       
        {         
            Vector2 baseCenterOffset;

            float posLeft;
            float posRight;
            float posTop;
            float posBottom;
            Vector3 baseCenterWorld;

            // pull any changes from Renderable here

            SetupDimensions(out baseCenterOffset, out posLeft, out posRight, out posTop, out posBottom, out baseCenterWorld);


            quad.SetupQuadVertices(baseCenterWorld, posLeft, posTop, posRight, posBottom, Parent.FlipHorizontally); //, 0f, 0f);



            // this quad goes from the base center and up, and is used for drawing drop shadows only.
            posTop = -baseCenterOffset.Y;
            posBottom = 0f;
            shadowQuad.WidthHeightRatio = renderAsBillboardType.AspectRatio;
            shadowQuad.SetupQuadVerticesFromBaseCenter(baseCenterWorld, posLeft, posTop, posRight, posBottom, /*0f, 0f,*/ baseCenterOffset, Parent.FlipHorizontally);


            SetQuadProperties();


            spriteIsDirty = false;
        }

        private void SetQuadProperties()
        {          
            Vector4 tint = Parent.GetCombinedEffects();// get combined effects??

            
            quad.SetProperties(renderAsBillboardType.Bendyness, Parent.RandomConstant, tint);
            shadowQuad.SetProperties(renderAsBillboardType.Bendyness, Parent.RandomConstant, tint);

            propertiesAreDirty = false;
        }

        private void SetOverlayQuadProperties()
        {
           
            Vector4 tint = Parent.GetCombinedOverlayEffects(); // get combined effects??

            Vector4 gradient1, gradient2, gradient3;

            Parent.GetOverlayGradientColors(out gradient1, out gradient2, out gradient3);

            overlayQuad.SetProperties(tint, gradient1, gradient2, gradient3); 

            overlayPropertiesAreDirty = false;
        }

        /// <summary>
        /// Compute the dimensions and position of the vertices based on either the current sprite rectangle or the utility map and the world position.
        /// </summary>
        /// <param name="baseCenterOffset"></param>
        /// <param name="posLeft"></param>
        /// <param name="posRight"></param>
        /// <param name="posTop"></param>
        /// <param name="posBottom"></param>
        /// <param name="baseCenterWorld"></param>
        private void SetupDimensions(out Vector2 baseCenterOffset, out float posLeft, out float posRight, out float posTop, 
                                                                        out float posBottom, out Vector3 baseCenterWorld)
        {            
            float width = quad.StaticSourceRectangle.Width;
            float height = quad.StaticSourceRectangle.Height;

            Tree tree; //TODO MLo: should this be data-driven?
            Entity parentEntity = null;
            if (Parent != null && Parent.Parent != null)
            {
                parentEntity = Parent.Parent as Entity;
            }

            if (parentEntity != null && parentEntity.Find(out tree)) // Parent != null && Parent.EntityType.TreeType != null) 
            {
                 //.Find(out tree))
                tree.GetSizeScaling(ref width, ref height);

                // for trees, we don't mark the base center. It is always at the bottom center:
               // baseCenterOffset = new Vector2(width / 2f, height);
                baseCenterOffset = new Vector2(width / 2f, height - 14f);
             
                // make room for a thick stem (15 pixels) on the quad as the tree grows up:
              //  posTop = -baseCenterOffset.Y; // +15f; 
              //  posBottom = posTop + height;

            }
            else
            { // non-trees:
                
                if (renderAsBillboardType.BaseCenter == Vector2.Zero)
                {   // if no base center has been entered, we use the center of the sprite:
                    baseCenterOffset = new Vector2(width / 2f, height / 2f);

                    // new: round base center to whole numbers, this prevents blurry rendering:
                    baseCenterOffset.X = (float)Math.Floor(baseCenterOffset.X);
                    baseCenterOffset.Y = (float)Math.Floor(baseCenterOffset.Y);
                    
                 

                }
                else
                {
                    baseCenterOffset = renderAsBillboardType.BaseCenter;
                }

                baseCenterOffset.X = Common.FlipOffset(baseCenterOffset.X, width, Parent.FlipHorizontally);

            }

            posLeft = -baseCenterOffset.X;
            posRight = posLeft + width;

            posTop = -baseCenterOffset.Y;
            posBottom = posTop + height;

            baseCenterWorld = Parent.Location.Value;
            
            // flip the offset
            Vector2 offsetFromParent = renderAsBillboardType.Offset;
            if (Parent.FlipHorizontally && offsetFromParent != Vector2.Zero)
            {
                offsetFromParent.X = -offsetFromParent.X;               
            }

            baseCenterWorld.X = baseCenterWorld.X + offsetFromParent.X;
            baseCenterWorld.Y = baseCenterWorld.Y + offsetFromParent.Y;
            
            // Each RenderBillboard has a Location and MapPosition:
            this.Location = baseCenterWorld;
            this.MapPosition =  MapManager.WorldPosToTile(this.Location);
        }


        public void Update(GameTime gameTime) //, out double? timeBeforeNextUpdate)
        {
           // bool requiresUpdate = false;
           // bool currentRequiresUpdate = false;

            if (quad != null)
            {
                quad.Update(gameTime); // updates 2d animation

             //   currentRequiresUpdate = currentRequiresUpdate || requiresUpdate;
            }

            if (shadowQuad != null)
            {
                shadowQuad.Update(gameTime);

             //   currentRequiresUpdate = currentRequiresUpdate || requiresUpdate;
            }

            if (overlayQuad != null)
            {
                overlayQuad.Update(gameTime);

             //   currentRequiresUpdate = currentRequiresUpdate || requiresUpdate;
            }
           
        }

        public double? GetUpdateInterval()
        {
            bool requiresUpdate = false;

            if (quad != null)
            {
                requiresUpdate = requiresUpdate || quad.RequiresUpdate;
            }

            if (overlayQuad != null)
            {
                requiresUpdate = requiresUpdate || overlayQuad.RequiresUpdate;
            }

            if (shadowQuad != null)
            {
                requiresUpdate = requiresUpdate || shadowQuad.RequiresUpdate;
            }


            if (requiresUpdate)
            {
                return 0;  
            }
            else
            {
                return null; // no updates needed, we can sleep 
            }

        }


       public int CompareTo(object obj)
       {
            ILocatable comparable = obj as ILocatable;
            return (int)(Location.Y - comparable.Location.Y);
        }

      
    }
}

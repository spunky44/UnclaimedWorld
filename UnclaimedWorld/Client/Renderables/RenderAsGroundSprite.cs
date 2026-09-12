using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// does not currently support "overlay (additive+scanlines)" rendering effect
    /// </summary>
    public class RenderAsGroundSprite : RenderAsBase, IDrawnAsGroundSprite
    {
        /// <summary>
        /// set this to null to not draw anything
        /// </summary>
        public Rectangle? SourceRect;

        private bool spriteOrLocationIsDirty = true;

        /// <summary>
        /// tint...
        /// </summary>
        private bool propertiesAreDirty = true;

       
        private bool overlayPropertiesAreDirty = true;


        public RoadAndPathQuad quad = new RoadAndPathQuad();

        /// <summary>
        /// used for the glowing outlines on tile resources. The only thing that is different is the tint...
        /// </summary>
       // public RoadAndPathQuad overlayQuad = new RoadAndPathQuad();


        //ctor
        public RenderAsGroundSprite(Entity parent, RenderAsGroundSpriteType type, Renderable renderable)
            : base(parent, renderable) 
        {
                       
            if (type.AssetName != null) // allow for late binding of sprites, as well as invisible sprites.
            {
                SourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle(type.AssetName);
            }
        }

       /* public RenderAsGroundSprite(RenderAsGroundSprite original, Renderable renderable)
            : base(null, renderable)
        {
            SourceRect = original.SourceRect;

            if (original.spriteOrLocationIsDirty)
            {
                original.SetupQuadVertices(); // refresh if dirty...
            }

            quad = new RoadAndPathQuad(original.quad);

        }*/


    
        public void Redraw(Rectangle? spriteRect)
        {
            this.SourceRect = spriteRect;
            spriteOrLocationIsDirty = true;
        }

        public void SetIsDirty()
        {
            spriteOrLocationIsDirty = true;
        }

        public void SetPropertiesAreDirty()
        {
            propertiesAreDirty = true;
        }

        public void SetOverlayPropertiesAreDirty()
        {
            overlayPropertiesAreDirty = true;
        }

        /// <summary>
        /// "drawing"
        /// </summary>
        /// <param name="featureVertices"></param>
        /// <param name="index"></param>
        public void CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null)
        {
            if (SourceRect.HasValue)
            {
                if (spriteOrLocationIsDirty)
                {
                    SetupQuadVertices();
                }
                
                if (propertiesAreDirty)
                {
                    // set properties on the quad - these effects are hardwired to the quad, and pulled automatically
                    SetQuadProperties();
                }

                // we can now reuse a quad with different effects. 
                // apply an additional effect if needed:
               // Vector4? oldTint = null;
                if (overridingEffectID.HasValue)
                {
                   // oldTint = quad.Tint; // save the old effect
                    Vector4 additionalEffect = Renderable.GetCombinedEffects(overridingEffectID.Value);

                    quad.SetProperties(additionalEffect);
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

               // index++;
            }
        }

        /// <summary>
        /// the 'Draw' method for the overlay quad
        /// </summary>
        /// <param name="featureVertices"></param>
        /// <param name="index"></param>
    /*    public void CopyOverlayQuadToVertexBuffer(VertexGroundFeature[] overlayVertices, ref int index)
        {
            if (spriteOrLocationIsDirty)
            {
                SetupQuadVertices(); // this method sets both quads (they are copies)
            }
            
            if (overlayPropertiesAreDirty)
            {
                // set properties on the quad:
                SetOverlayQuadProperties();
            }

            overlayQuad.CopyQuadToVertexBuffer(overlayVertices, index);

            index++;
        }*/

        public void SetupQuadVertices()
        {
            Vector2 baseCenterOffset;
                       
            // use the center of the sprite
            baseCenterOffset = new Vector2(SourceRect.Value.Width / 2f, SourceRect.Value.Height / 2f);


            quad.SetupQuadVertices(Renderable.Location.Value, // Parent.Location;
                                        baseCenterOffset,
                                        SourceRect.Value,
                                        The.Client.FlatSpriteSheet.Texture,
                                        Renderable.FlipHorizontally);

            // make the overlay quad a copy...
            /*overlayQuad.SetupQuadVertices(Renderable.Location, 
                                       baseCenterOffset,
                                       SourceRect.Value,
                                       The.Client.FlatSpriteSheet.Texture,
                                       Renderable.FlipHorizontally);
            */

            SetQuadProperties();
           // SetOverlayQuadProperties();

            spriteOrLocationIsDirty = false;
        }

        /// <summary>
        /// pull what we need from Renderable..
        /// </summary>
        private void SetQuadProperties()
        {
         
            Vector4 tint = Renderable.GetCombinedEffects();// get combined effects??

            quad.SetProperties(tint);
           
            propertiesAreDirty = false;
        }

       /* private void SetOverlayQuadProperties()
        {
          
            Vector4 tint = Renderable.GetCombinedOverlayEffects();

            overlayQuad.SetProperties(tint); // NOTE: sets properties on the same quad - for now we do not use a separate overlay quad...

            overlayPropertiesAreDirty = false;
        }*/
       
    }

}

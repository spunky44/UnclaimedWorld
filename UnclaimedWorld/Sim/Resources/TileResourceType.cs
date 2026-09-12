using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Resources // TODO: DECOUPLE
{ 
    public enum IconToRender { None = 0, Ore = 1, Hook = 2, Bug = 3, NewlyDiscovered = 4 };

    /// <summary>
    /// Similar to CropType... but is made to handle display of sprites on the ground
    /// </summary>
    public class TileResourceType //: IXmlSerializable
    {

       
        // Firewood should be harvested from each tile. 
        // It should fall to the ground. Then go into the nutrient cycle, or create fire hazards. 
        // Dead trees fall to the ground, and start to rot and/or dry. Drying should be modelled...
        // Standing dead trees are more likely to dry than rot. They can be felled for firewood.

      
        public int MaxFlavours;

     
      //  public string[] TileResourceSpritesLess;
      //  public string[] TileResourceSpritesMore;

        public float MoreSpriteLimit;

        
        public RenderableType RenderableType; // = new RenderableType();
        public RenderableType EditorRenderableType;

        public RenderableType RenderableTypeMode
        {
            get
            {
                if (The.Sim.Mode == Sim.EngineMode.Game)
                {
                    return RenderableType;
                }
                else
                {
                    return EditorRenderableType ?? RenderableType;
                }
            }
        }


        public TileResourceType() { }

        [XmlIgnore]
        public bool IsRenderedWithSprites
        {
            get;
            private set;
        }

        public void Initialize()
        {
            IsRenderedWithSprites = IsRenderedWithSprite();

        }

        public bool IsRenderedWithSprite()
        {
            RenderableType renderableToUse = RenderableTypeMode;

            if (renderableToUse == null)
                return false;

            return
                (renderableToUse.DefaultClientState != null
                && (renderableToUse.DefaultClientState.RenderAsGroundSpriteType != null || renderableToUse.DefaultClientState.RenderAsBillboardType != null))
                ||
                (renderableToUse.ClientStateConditions != null
                && renderableToUse.ClientStateConditions.Any(s => s.RenderAsGroundSpriteType != null || s.RenderAsBillboardType != null));

            //return resourceType.TileResourceType.TileResourceSpritesLess != null;
        }
    }
}

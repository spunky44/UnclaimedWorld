using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Collisions
{
    public class GeometryLayoutType
    {
        /// <summary>
        /// set this to true if the structure should repel entities
        /// </summary>
        public bool CausesCollisions = false;

        public bool GridAlignedPlacement = false;

        /// <summary>
        /// for footprints, mouse selection and collisions.
        /// </summary>
        public CollideShape2D[] Shapes;


        /// <summary>
        /// used for mouse selection only. Will override Shapes if both are defined.
        /// </summary>
        public CollideShape2D[] SelectionShapes;


        /// <summary>
        /// This replaces TiledLayout
        /// Compatible with TiledLayout, though, they get along nicely
        /// Not compatible with PointLayout, will assert if both are present
        /// You can edit geometry shapes on the fly while the game is running
        /// Enable Geometries in Debug Menu, also Enable TerrainCosts(foot)
        /// Use Number pad 8,4,6,2 to nudge the child shape
        /// Use numeral keys 1,2,3,4 to assign the child shape index (default is zeroth child)
        /// Use numeric Add and Subtract keys to scale radius of child shape
        /// The collision tiles will update on the fly
        /// Be warned: flipped Entities will display their offsets in world (non-flipped) coordinates
        /// I color the geo lines green and add a warning in these cases -- pay attention, negate the X coords
        /// Have fun
        /// </summary>
        public GeometryLayoutType()
        {
            Shapes = new CollideShape2D[] 
            {
                new CollideShape2D(Vector2.Zero, 0)
            };
        }

        private float pad = 0f;

        /// <summary>
        /// this is an additional radius that blocks construction on the area it covers. shows up yellow on the terrain overlay.
        /// </summary>
        public float Pad
        {
            get
            {
                return pad;
            }
            set
            {
                pad = value;
            }
        }

        private CollidePrim padShape = CollidePrim.Rectangle;
        public CollidePrim PadShape
        {
            get
            {
                return padShape;
            }
            set
            {
                padShape = value;
            }
        }


        public void Initialize(EntityType parent)
        {
            //nothing to do
        }

        public void PostLoadContentInitialize()
        {
            //nothing to do
        }
    }
}


 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Renderables
{
    
    /// <summary>
    /// a common parent for RenderAsBillboard, RenderAsGroundSprite, RenderAsModel classes
    /// </summary>
    public class RenderAsBase 
    {
        /// <summary>
        /// there's also ParentEntity!
        /// </summary>
        public IKnownEntityData Parent
        {
            get;
            set;
        }

        /// <summary>
        /// can be null!!!
        /// </summary>
        public Entity ParentEntity
        {
            get;
            set;
        }

        public Renderable Renderable;

        //ctor
        public RenderAsBase(Entity parent,Renderable renderable)
        {
            this.Parent = parent;
            this.ParentEntity = parent;

            this.Renderable = renderable;
         }


    }
}
 
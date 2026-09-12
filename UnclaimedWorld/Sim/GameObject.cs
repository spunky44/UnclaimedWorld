using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;

namespace UWGame 
{
    
   
    /// <summary>
    /// Not sure why we need this other than for ILocatable...
    /// can now also exist off-site...
    /// </summary>
    public class GameObject : ILocatable 
    {
       
        public virtual Renderable AsRenderable
        {
            get
            {
                return null;
            }
        }

        public virtual RenderAsBillboard AsRenderAsBillboard
        {
            get
            {
                return null;
            }
        }

        Vector3 ILocatable.Location
        {
            get
            {
                return Location.Value; // location.Value;
            }
        }

       /* Vector3 ILocatable.Location
        {
            get
            {
                return location.Value;
            }
        }*/


        protected Vector3? location;

        /// <summary>
        /// These are world coordinates for game objects. 
        /// 
        /// set this null for off-site entities.
        /// </summary>
        public virtual Vector3? Location
        {
            get { return location; }
            set 
            {
                if (value.HasValue)
                {
                    location = The.Map.ClampWorldPosition(value.Value);
                }
                else
                {
                    location = null;
                }
            }
        }

        public int CompareTo(object obj)
        {
            /*
             By definition, any object compares greater than null, and two null references compare equal to each other.
             */

            ILocatable comparable = obj as ILocatable;

            if (Location.HasValue)
            {
                return (int)(Location.Value.Y - comparable.Location.Y);
            }
            else
            {
                return -1;
            }
        }

       

    }
}

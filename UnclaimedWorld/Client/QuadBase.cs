using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide
{
    public abstract class QuadBase
    {
        /// <summary>
        /// We use the Player for static images also.
        /// </summary>
        public Animation2DPlayer Player = new Animation2DPlayer();


        public QuadBase()
        {

        }

        public QuadBase(QuadBase original)
        {
            Player = new Animation2DPlayer(original.Player);
        }

        public Rectangle StaticSourceRectangle
        {
            get
            {
                return Player.GetFrame();
            }
        }

        public Texture2D Texture
        {
            get
            {
                if (Player != null && Player.Animation != null)
                {
                    return Player.Animation.Texture;
                }
                else return null;
            }
        }


        public bool RequiresUpdate
        {
            get
            {
                if (Player != null)
                {
                    return Player.RequiresUpdate;
                }
                else return false;
            }
        }

        public void Update(GameTime gameTime) //, out bool requiresUpdate)
        {
           // requiresUpdate = false;

            if (Player != null)
            {
                if (Player.RequiresUpdate) 
                {
                    Player.Update(gameTime);

                   // requiresUpdate = true;
                }
            }
        }

        public void SetStaticFrame(Rectangle? rectangle, Texture2D texture)
        {
            // perhaps better if a shared 'animation' was used here...
            if (rectangle.HasValue)
            {
                Player.StartAnimation(new Animation2D(texture, 0.1f, false) { Cells = new List<Cell>() { new Cell(rectangle.Value) } });
            }
            else
            {
                Player.StartAnimation(new Animation2D(texture, 0.1f, false) { Cells = new List<Cell>() { new Cell(new Rectangle()) { Color = Color.Transparent } } });
            }
        }


        public void SetAnimationFrames(Animation2D animation) // Rectangle? rectangle, Texture2D texture)
        {
            Player.StartAnimation(animation);
                       
        }

    }
}

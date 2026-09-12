using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WindowSystem
{
   
    /// <summary>
    /// texture and recctangles are optional. pure color cycles are possible
    /// </summary>
    public class Animation2D
    {
        // xna 3
      /*  public static Color transp = new Color(255, 255, 255, 0);
        public static Color halfTransp = new Color(255, 255, 255, 128);
        public static Color thirdTransp = new Color(255, 255, 255, 85);
        public static Color black = new Color(0f, 0f, 0f, 1f);
        */

        /// <summary>
        /// can be used to mark the 'center' of the anim, like the middle
        /// </summary>
        public Vector2 Origin;

        public static Color transp = new Color(0, 0, 0, 0);
        public static Color halfTransp = new Color(128, 128, 128, 128);
        public static Color thirdTransp = new Color(85, 85, 85, 85);
        public static Color black = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f));

        public bool DoColorInterpolation = true;


        /// <summary>
        /// All frames in the animation arranged horizontally.
        /// 
        /// TODO: serialize as string - make LoadContent()
        /// </summary>
        [XmlIgnore]
        public Texture2D Texture
        {
            get { return texture; }
        }
        Texture2D texture;

        /// <summary>
        /// Duration of time to show each frame.
        /// </summary>
        public float FrameTime
        {
            get { return frameTime; }
            set { frameTime = value; }
        }
        float frameTime;

     /*   private float totalRunningTime;

        public float TotalRunningTime
        {
            get { return totalRunningTime; }
        }*/

        /// <summary>
        /// When the end of the animation is reached, should it
        /// continue playing from the beginning?
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
        }
        bool isLooping;

        public List<Cell> Cells = new List<Cell>();

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public int FrameCount
        {
            get { return Cells.Count;} 
        }


        public Animation2D() { }

        /// <summary>
        /// Constructors a new animation.
        /// </summary>        
        public Animation2D(Texture2D texture, float frameTime, bool isLooping)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }

    public struct Cell
    {
        
        public Rectangle? Frame;
        public Color Color;
      //  public float? FrameTime;

      /*  public Cell()
        {
            Frame = null;
            Color = Color.White;
        }*/

        public Cell(Rectangle frame)
        {
            Frame = frame;
            Color = Color.White;
         //   FrameTime = null;
        }
    }
}

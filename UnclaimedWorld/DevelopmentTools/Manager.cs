using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kensei
{
    namespace Dev
    {
        /// <summary>
        /// Manager class to simplify debug stuff.
        /// </summary>
        public static class Manager
		{
			#region Class Behaviour

			/// <summary>
            /// Initialises the various debug systems.
            /// </summary>
            /// <param name="content">The content manager to load shaders and fonts from.</param>
            /// <param name="device">The device to make content compatible with.</param>
            static public void Initialise(ContentManager content, GraphicsDevice device, int drawableAreaX, int drawableAreaY, int drawableAreaWidth, int drawableAreaHeight)
            {
				// XXX this path should NOT be hardcoded at this level. But I leave it to the user to change
				// it if they need to. Ideally you would embed the font as a resource in your project; see
				// http://blogs.msdn.com/shawnhar/archive/2007/06/12/embedding-content-as-resources.aspx.
                s_font = content.Load<SpriteFont>("Arial");
				s_spriteBatch = new SpriteBatch( device );

              //  Dev.Command.Initialise();
               
				Dev.Options.Initialise();
                Dev.DevText.Initialise(device, drawableAreaX, drawableAreaY, drawableAreaWidth, drawableAreaHeight);
                Dev.Shape.Initialise( content, device );
            }

            /// <summary>
            /// Per-frame update for all debug stuff.
            /// </summary>
            static public void Update()
			{
				Dev.Command.Update( Keyboard.GetState() );
            }

            /// <summary>
            /// Renders the various debug systems.
            /// </summary>
            /// <param name="device">The graphics device to render with.</param>
            /// <param name="renderMatrix">The transformation for 3D rendering.</param>
            /// <param name="width">The width of the screen.</param>
            /// <param name="height">The height of the screen.</param>
            static public void Draw( GraphicsDevice device, Matrix renderMatrix, float width, float height )
            {
				// Order matters! Don't change it unless you're sure you won't break things
				Dev.Command.PreDraw( width, height );
                Dev.Shape.Draw( device, renderMatrix, width, height );
                Dev.Command.Draw( width, height );
                Dev.DevText.Draw( renderMatrix, width, height );
            }

			/// <summary>
			/// Shut down the debug systems.
			/// </summary>
			static public void Shutdown()
			{

			}

			#endregion

			#region Variables

			private static SpriteFont s_font;
			private static SpriteBatch s_spriteBatch;

			internal static SpriteFont SpriteFont
			{
				get { return s_font; }
			}

			internal static SpriteBatch SpriteBatch
			{
				get { return s_spriteBatch; }
			}

			#endregion
		}
    }
}

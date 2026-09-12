using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kensei
{
    namespace Dev
    {
        /// <summary>
        /// Wrapper class for debug text. Offers a limited wrapper to SpriteBatch font drawing,
        /// but makes up for being limited by being more convenient.
        /// </summary>
        public static class DevText
        {
            #region Class Behaviour

            internal static void Initialise(GraphicsDevice device, int drawableAreaX, int drawableAreaY, int drawableAreaWidth, int drawableAreaHeight)
            {

                // This sucks. Why can't I use XELibrary.Utility.GetTitleSafeArea? What's wrong with circular dependencies?
                s_safeArea = new Rectangle(drawableAreaX, drawableAreaY, drawableAreaWidth, drawableAreaHeight);
              //  s_safeArea = new Rectangle(device.Viewport.X, device.Viewport.Y, device.Viewport.Width, device.Viewport.Height);

#if XBOX360
					// Find Title Safe area of Xbox 360
					float border = ( 1 - Xbox360SafeArea ) / 2;
					m_safeArea.X = (int)( border * m_safeArea.Width );
					m_safeArea.Y = (int)( border * m_safeArea.Height );
					m_safeArea.Width = (int)( Xbox360SafeArea * m_safeArea.Width );
					m_safeArea.Height = (int)( Xbox360SafeArea * m_safeArea.Height );
#endif
            }

            /// <summary>
            /// Draws all debug text requested this frame, restores draw state, and prepares for next frame.
            /// Drawing can be disabled by the game via the "DebugDrawText" dev option.
            /// </summary>
            internal static void Draw(Matrix renderMatrix, float width, float height)
            {
                if (s_printList.Count > 0 || s_printStackList.Count > 0 || s_print3DList.Count > 0)
                {
                    // NOTE we ask for state to be saved and restored. Normally such an operation is expensive,
                    // and should be avoided - each draw call should set up the state it needs. As we have cached
                    // all the debug text calls, that's probably not an issue for us and we can do what's easy. See
                    // https://blogs.msdn.com/shawnhar/archive/2006/11/13/spritebatch-and-renderstates.aspx

                    //Dev.Manager.SpriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState ); // XNA 3
                    Dev.Manager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    try
                    {
                        // Render all the 2D text...
                        Render2D();

                        // ...then all the stack text...
                        RenderStack();

                        // ... followed by all the 3D text (TODO support split screen)
                        Render3D(ref renderMatrix, width, height);
                    }
                    catch (ArgumentException)
                    {
                        // TODO at least log or print out the error, including which character was not in the font
                    }

                    Dev.Manager.SpriteBatch.End();

                    // Clear the lists. Do this immediately after rendering them, not in Update, to be certain nothing is missed.
                    // This breaks the usual rule of not changing anything during Draw, but it's the most convenient way.
                    s_printList.Clear();
                    s_print3DList.Clear();
                    s_printStackList.Clear();
                }
            }

            #endregion

            #region Printing Text

            /// <summary>
            /// Requests a string be printed as debug text.
            /// </summary>
            /// <param name="position">2D screen space position to print text.</param>
            /// <param name="text">The text to print.</param>
            public static void Print(Vector2 position, string text)
            {
                Print(position, text, DefaultColour);
            }

            /// <summary>
            /// Requests a string be printed as debug text.
            /// </summary>
            /// <param name="position">2D screen space position to print text.</param>
            /// <param name="text">The text to print.</param>
            /// <param name="colour">The colour in which to print it.</param>
            public static void Print(Vector2 position, string text, Color colour)
            {
                Print(position, text, colour, Alignment.Left);
            }

            /// <summary>
            /// Requests a string be printed as debug text.
            /// </summary>
            /// <param name="position">2D screen space position to print text.</param>
            /// <param name="text">The text to print.</param>
            /// <param name="align">How to align the text.</param>
            public static void Print(Vector2 position, string text, Alignment align)
            {
                Print(position, text, align);
            }

            /// <summary>
            /// Requests a string be printed as debug text.
            /// </summary>
            /// <param name="position">2D screen space position to print text.</param>
            /// <param name="text">The text to print.</param>
            /// <param name="colour">The colour in which to print it.</param>
            /// <param name="align">How to align the text.</param>
            public static void Print(Vector2 position, string text, Color colour, Alignment align)
            {

                TextInfo2D printInfo;
                printInfo.m_position = position;
                printInfo.m_text = text;
                printInfo.m_colour = colour;

                switch (align)
                {
                    case Alignment.Left:
                        break;

                    case Alignment.Centre:
                        printInfo.m_position.X -= GetDims(printInfo.m_text).X / 2;
                        break;

                    case Alignment.Right:
                        printInfo.m_position.X -= GetDims(printInfo.m_text).X;
                        break;
                }

                s_printList.Add(printInfo);

            }

            /// <summary>
            /// Prints a string as debug test, in a 3D position.
            /// </summary>
            /// <param name="position">3D point at which to print text.</param>
            /// <param name="text">The text to print.</param>
            public static void Print(Vector3 position, string text)
            {
                Print(position, text, DefaultColour);
            }

            /// <summary>
            /// Prints a string as debug test, in a 3D position.
            /// </summary>
            /// <param name="position">3D point at which to print text.</param>
            /// <param name="text">The text to print.</param>
            /// <param name="colour">The colour in which to print it.</param>
            public static void Print(Vector3 position, string text, Color colour)
            {

                TextInfo3D printInfo;
                printInfo.m_position = position;
                printInfo.m_text = text;
                printInfo.m_colour = colour;
                s_print3DList.Add(printInfo);

            }

            /// <summary>
            /// Prints text on a stack, to avoid the inconvenience of specifying a position.
            /// </summary>
            /// <param name="text">What to print.</param>
            public static void Print(string text)
            {
                Print(text, DefaultColour);
            }

            /// <summary>
            /// Prints text on a stack, to avoid the inconvenience of specifying a position.
            /// </summary>
            /// <param name="text">What to print.</param>
            /// <param name="colour">What colour to print it.</param>
            public static void Print(string text, Color colour)
            {

                TextInfoStack printInfo;
                printInfo.m_colour = colour;
                printInfo.m_text = text;
                s_printStackList.Add(printInfo);

            }

            /// <summary>
            /// Provides the dimensions of the string when rendered as debug text.
            /// </summary>
            /// <param name="text">The string in question.</param>
            /// <returns>The screen space it will consume.</returns>
            public static Vector2 GetDims(string text)
            {
                return Dev.Manager.SpriteFont.MeasureString(text);
            }

            /// <summary>
            /// Sets the colour to use when no Color is specified.
            /// </summary>
            public static Color DefaultColour
            {
                get { return s_defaultColour; }
                set { s_defaultColour = value; }
            }

            #endregion

            #region Types

            // Alignment is only available in 2D - it's confusing in 3D and meaningless on the stack
            public enum Alignment
            {
                Left,
                Centre,
                Right,
            }

            private struct TextInfo2D
            {
                public Vector2 m_position;
                public string m_text;
                public Color m_colour;
            }

            private struct TextInfo3D
            {
                public Vector3 m_position;
                public string m_text;
                public Color m_colour;
            }

            private struct TextInfoStack
            {
                public string m_text;
                public Color m_colour;
            }

            #endregion

            #region Variables

            private static List<TextInfo2D> s_printList = new List<TextInfo2D>();
            private static List<TextInfo3D> s_print3DList = new List<TextInfo3D>();
            private static List<TextInfoStack> s_printStackList = new List<TextInfoStack>();
            private static Rectangle s_safeArea;
            private static Color s_defaultColour = Color.LimeGreen;

#if XBOX360
			private static readonly float Xbox360SafeArea = 0.85f;
#endif

            #endregion

            #region Private Functions

            private static void Render3D(ref Matrix renderMatrix, float width, float height)
            {
                foreach (TextInfo3D print in s_print3DList)
                {
                    // TODO this world-to-screen maths really ought to be separated into a function and made available to other stuff
                    Vector4 position = new Vector4(print.m_position, 1);
                    Vector4.Transform(ref position, ref renderMatrix, out position);

                    position.X = position.X * 0.5f / position.W + 0.5f;
                    position.Y = 1.0f - (position.Y * 0.5f / position.W + 0.5f);
                    position.X *= width;
                    position.Y *= height;

                    if (position.X >= 0 && position.X < width
                        && position.Y >= 0 && position.Y < height
                        && position.Z > 0)
                    {
                        Dev.Manager.SpriteBatch.DrawString(Dev.Manager.SpriteFont, print.m_text, new Vector2(position.X, position.Y), print.m_colour);
                    }
                }
            }

            private static void RenderStack()
            {
                // TODO allow the option of also printing stuff from the bottom of the screen (literally a stack).
                // Might also be nice to be able to give this stuff a lifetime - ie. "print this for the next five
                // seconds", rather than printing it every frame for the next five seconds.

                Vector2 stackPos = new Vector2(s_safeArea.Left, s_safeArea.Top);

                for (int i = 0; i < s_printStackList.Count; ++i)
                {
                    Dev.Manager.SpriteBatch.DrawString(Dev.Manager.SpriteFont, s_printStackList[i].m_text, stackPos, s_printStackList[i].m_colour);
                    stackPos.Y += Dev.Manager.SpriteFont.MeasureString(s_printStackList[i].m_text).Y;
                }
            }

            private static void Render2D()
            {
                for (int i = 0; i < s_printList.Count; ++i)
                {
                    Dev.Manager.SpriteBatch.DrawString(Dev.Manager.SpriteFont, s_printList[i].m_text, s_printList[i].m_position, s_printList[i].m_colour);
                }
            }

            #endregion
        }
    }
}
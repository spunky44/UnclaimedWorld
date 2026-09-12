using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if !XBOX360
using System.Drawing;
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// XXX clash between Microsoft.Xna.Framework.Input.Keys and System.Windows.Forms.Keys makes parts of this code very ugly

namespace Kensei
{
    namespace Dev
    {
		/// <summary>
		/// A Quake-style command prompt, extensible to support just about any kind of text command you can think of.
		/// On Windows, it's most convenient to use the modeless dialog box method of interaction, rather than interfere
		/// with the game rendering - though you must then only accept keyboard input for the game when the dialog box
		/// is not in focus.
		/// </summary>
        public static class Command
        {
            #region Class behaviour

			internal static void Initialise()
            {
				AddCommand( "help", HelpDelegate, "\"help\" to see a list of commands, or \"help <command>\" to see help for that command." );
				AddCommand( "cls", ClsDelegate, "Clears the console output." );
				AddCommand( "clearHistory", ClearDoskeyDelegate, "Clears the command history." );
				AddCommand( "exit", ExitDelegate, "Closes the command console and returns to game (can also press Esc)." );
				AddCommand( "collectGarbage", CollectGarbageDelegate, "Forcibly collects garbage." );
#if !XBOX
				AddCommand( "command", DialogBoxDelegate, "Brings up a dialog box offering more powerful access to the command prompt system." );
#endif
			}
			 
            internal static void Update( KeyboardState keyboard )
            {
                if (keyboard.IsKeyDown(s_activateKey) && !s_lastKeyboard.IsKeyDown(s_activateKey))
                {
                    DoModelessDialog();
                }
                
                /* Quake style console:
                if ( keyboard.IsKeyDown( s_activateKey ) && !s_lastKeyboard.IsKeyDown( s_activateKey ) )
				{
					Active = !Active;
				}
                
				if ( Active )
                {
                    ProcessInput( keyboard );
				}

				s_lastKeyboard = keyboard;*/
            }

			/// <summary>
			/// A little bit nasty; this is required so that the shape can be drawn first, then the text on top of it
			/// </summary>
			internal static void PreDraw( float screenWidth, float screenHeight )
			{
				if ( Active )
				{
                    // Draw the background for the command console
                    // WISHLIST allow this to optionally be a texture or something
					// WISHLIST smoothly fade this on/off over a second or so when the console is enabled / disabled
					float height = screenHeight * ScreenProportion;
					Kensei.Dev.Shape.Box( Vector2.Zero, new Vector2( screenWidth, height ), BackgroundColour, true );
				}
			}

            internal static void Draw( float screenWidth, float screenHeight )
            {
				// If you're running under Windows, the dialog box will almost always be preferable
                if ( Active )
                {
					// Consider Xbox 360 safe area
					Microsoft.Xna.Framework.Rectangle safeArea = 
						new Microsoft.Xna.Framework.Rectangle( 0, 0, (int)screenWidth, (int)( screenHeight * ScreenProportion ) );
					#if XBOX360
						// Find Title Safe area of Xbox 360
						float border = ( 1 - Xbox360SafeArea ) / 2;
						safeArea.X = (int)( border * safeArea.Width );
						safeArea.Y = (int)( border * safeArea.Height );
						safeArea.Width = (int)( Xbox360SafeArea * screenWidth );
						safeArea.Height -= safeArea.Y;
					#endif

					// In order to fit on screen, we'll need to find how far back up the stack we can print
					float totalTextHeight = Dev.Manager.SpriteFont.MeasureString( "> " + s_currentCommandLine ).Y;
					int index;
					
					// Print as much of the rest as will fit on screen - so find the first thing to print.
					// XXX sometimes this seems to get confused by \n characters (eg. try typing help and letting it scroll off).
					// We could maybe fix that by splitting the string on \n characters before adding to s_output?
					// TODO implement automatic word wrap for anything and everything that is printed (again using GetDims),
					// though it's pretty rare for text to be printed wide enough to need it.
					for ( index = s_output.Count - 1; index >= 0; --index )
					{
						totalTextHeight += Dev.Manager.SpriteFont.MeasureString( s_output[index] ).Y;

						if ( totalTextHeight >= safeArea.Height )
						{
							++index; // We've gone too far!
							break;
						}
					}

					Vector2 pos = new Vector2( safeArea.Left, safeArea.Top );

                  
                    //Dev.Manager.SpriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState ); // XNA 3
                    Dev.Manager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
          
					
					for ( index = (int)MathHelper.Max( index, 0 ); index >= 0 && index < s_output.Count; ++index )
					{
						Dev.Manager.SpriteBatch.DrawString( Dev.Manager.SpriteFont, s_output[index], pos, InactiveTextColour );
						pos.Y += Dev.Manager.SpriteFont.MeasureString( s_output[index] ).Y;
					}

					// Now actually print the current command line
					Dev.Manager.SpriteBatch.DrawString( Dev.Manager.SpriteFont, "> " + s_currentCommandLine, pos, ActiveTextColour );
					
					// Restore dev text
					Dev.Manager.SpriteBatch.End();
                }
            }

            #endregion

			#region Command Prompt with Command History

			/// <summary>
			/// Adds a string to the output from the command prompt. Can be used for feedback / results for example.
			/// </summary>
			/// <param name="printString">The string to add.</param>
			static public void Print( string message )
			{
				s_output.Add( message );

				#if !XBOX
					if ( s_form != null )
					{
						s_form.Print( message );
					}
				#endif
			}

			/// <summary>
			/// Registers a command with the command console. Note that name-checking is case insensitive.
			/// The string array passed to the delegate will contain the arguments given on the command line,
			/// as separated by whitespace, in the order they were given. The [0]th argument will always be
			/// the name of the command itself.
			/// </summary>
			/// <param name="commandName">The name for the command (case insensitive).</param>
			/// <param name="commandFunction">The delegate to call for this command.</param>
			/// <param name="helpText">Explanatory text for this command.</param>
			/// <returns>True for success, false if the command could not be added.</returns>
			static public bool AddCommand( string commandName, CommandFunction commandFunction, string helpText )
			{
				if ( !commandName.Contains( " " ) )
				{
					if ( !s_commands.ContainsKey( commandName ) )
					{
						try
						{
							s_commands.Add( commandName, new CommandEntry( commandFunction, helpText ) );
							return true;
						}
						catch ( ArgumentException )
						{
							Print( "Error: couldn't register command \"" + commandName + "\"." );
						}
					}
					else
					{
						Print( "Error: command \"" + commandName + "\" already registered." );
					}
				}
				else
				{
					Print( "Error: command \"" + commandName + "\" contains spaces." );
				}

				return false;
			}

			#endregion

            #region Types

			public delegate void CommandFunction( string[] arguments );

			private struct CommandEntry
			{
				internal readonly CommandFunction m_function;
				internal readonly string m_help;

				internal CommandEntry( CommandFunction commandFunction, string help )
				{
					m_function = commandFunction;
					m_help = help;
				}
			}

            #endregion
            
            #region Properties - mostly relevant only when not using the modeless dialog box

            /// <summary>
            /// The console itself controls when it is activated and deactivated (see ActivationKey).
            /// However, the game will probably want to disable input and/or pause when it is active.
            /// </summary>
            public static bool Active
            {
                private set { s_active = value; }
                get { return s_active; }
            }

			/// <summary>
			/// Sets which key activates the debug console.
			/// </summary>
			public static Microsoft.Xna.Framework.Input.Keys ActivationKey
			{
				get { return s_activateKey; }
				set { s_activateKey = value; }
			}

			/// <summary>
			/// The colour of the background to the console.
			/// </summary>
			public static Microsoft.Xna.Framework.Color BackgroundColour
			{
				set { s_backgroundColour = value; }
				get { return s_backgroundColour; }
			}

			/// <summary>
			/// The colour of active text (ie. the current command line).
			/// </summary>
			public static Microsoft.Xna.Framework.Color ActiveTextColour
			{
				set { s_activeTextColour = value; }
				get { return s_activeTextColour; }
			}

			/// <summary>
			/// The colour of inactive text (ie. previously entered commands and printouts).
			/// </summary>
			public static Microsoft.Xna.Framework.Color InactiveTextColour
			{
				set { s_inactiveTextColour = value; }
				get { return s_inactiveTextColour; }
			}

			/// <summary>
			/// The amount of screen space the console takes up when it is active.
			/// </summary>
			public static float ScreenProportion
			{
				set { s_screenProportion = MathHelper.Clamp( value, 0.0f, 1.0f ); }
				get { return s_screenProportion; }
			}

            #endregion

            #region Variables

			// Activation/deactivation and core behaviour
			private static Dictionary<string, CommandEntry> s_commands = new Dictionary<string,CommandEntry>( new InsensitiveComparer() );
            private static bool s_active;
			#if XBOX360
				private static Keys s_activateKey = Keys.F2;	// ` doesn't seem to be recognised on 360? Is that just my keyboard or...?
			#else
				private static Microsoft.Xna.Framework.Input.Keys s_activateKey = Microsoft.Xna.Framework.Input.Keys.OemPipe;// Microsoft.Xna.Framework.Input.Keys.Oem8;	// ` (ie. the key below Esc)
				private static CommandForm s_form;
			#endif
			
            // Input and output
			private static KeyboardState s_lastKeyboard = new KeyboardState();
			private static string s_currentCommandLine;
            private static List<string> s_output = new List<string>();

			// DOSkey (command history)
            private static List<string> s_commandHistory = new List<string>();	// For use with the DOSkey feature
			private static int s_doskey = -1;
			private static bool s_doskeyActive;

			// Cosmetic stuff
			private static Microsoft.Xna.Framework.Color s_backgroundColour = Microsoft.Xna.Framework.Color.Crimson;
			private static Microsoft.Xna.Framework.Color s_activeTextColour = Microsoft.Xna.Framework.Color.White;
			private static Microsoft.Xna.Framework.Color s_inactiveTextColour = Microsoft.Xna.Framework.Color.Yellow;
			private static float s_screenProportion = 0.5f;
			#if XBOX360
				private static readonly float Xbox360SafeArea = 0.85f;
			#endif

            #endregion

            #region Private Functions

            static private void ProcessInput( KeyboardState keyboard )
            {
				Microsoft.Xna.Framework.Input.Keys[] keys = keyboard.GetPressedKeys();

				foreach ( Microsoft.Xna.Framework.Input.Keys key in keys )
				{
					if ( !s_lastKeyboard.IsKeyDown( key ) )	// TODO implement repeat delay and repeat speed
					{
						if ( IsSpecialKey( key ) )
						{
							if ( key == Microsoft.Xna.Framework.Input.Keys.Escape )
							{
								s_currentCommandLine = "";
							}
							else if ( key == Microsoft.Xna.Framework.Input.Keys.Enter )
							{
								ProcessCurrentCommand();
							}
							else if ( key == Microsoft.Xna.Framework.Input.Keys.Up )
							{
								DoskeyDecrement();
							}
							else if ( key == Microsoft.Xna.Framework.Input.Keys.Down )
							{
								DoskeyIncrement();
							}
							else if ( key == Microsoft.Xna.Framework.Input.Keys.Back )
							{
								if ( s_currentCommandLine.Length > 0 )
								{
									// XBOX360 - must use two-argument version of Remove
									s_currentCommandLine = s_currentCommandLine.Remove( s_currentCommandLine.Length - 1, 1 );
								}
							}
							else if ( (int)key >= (int)Microsoft.Xna.Framework.Input.Keys.NumPad0 && (int)key <= (int)Microsoft.Xna.Framework.Input.Keys.NumPad9 )
							{
								s_currentCommandLine += KeyToChar( key, 
									keyboard.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.LeftShift ) || keyboard.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.RightShift ) );
							}
							// TODO caret, using left and right (will also change how backspace and KeyToChar are dealt with)
						}
						else
						{
							s_currentCommandLine += KeyToChar( key, 
								keyboard.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.LeftShift ) || keyboard.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.RightShift ) );
						}
					}
				}
            }

            static private void ProcessCurrentCommand()
            {
				ProcessCommand( s_currentCommandLine );
				s_currentCommandLine = "";
            }

			static private void ProcessCommand( string command )
			{
				command = command.Trim();
				string[] commandArguments = command.Split();

				if ( ( commandArguments.Length > 0 )	// Always seems to be true unfortunately?
					&& ( command.Length > 0 ) )			// The actual test that therefore matters (but shouldn't be needed)
				{
					CommandEntry currentCommand;

					// Add command to s_commandHistory, unless it is equal to the most recent command (compare
					// with the behaviour of the Windows Command Prompt) - this supports the DOSkey feature.
					if ( s_commandHistory.Count == 0 ||
						( command != s_commandHistory[s_commandHistory.Count - 1] ) )
					{
						s_commandHistory.Add( command );
					}

					// If there has been a command and doskey has not been used this time, forget what we know of doskey (like Windows does)
					if ( !s_doskeyActive )
					{
						s_doskey = -1;
					}

					// Show the results of the command
					Print( "> " + command );

					if ( s_commands.TryGetValue( commandArguments[0], out currentCommand ) )
					{
						currentCommand.m_function( commandArguments );
					}
					else
					{
						Print( "Error: unknown command \"" + commandArguments[0] + "\"." );
					}

					// Tidy up
					s_doskeyActive = false;
				}
			}

			static private void DoskeyIncrement()
			{
				if ( s_doskey != -1 )
				{
					++s_doskey;
					s_doskey = (int)MathHelper.Min( s_commandHistory.Count - 1, s_doskey );
				}

				if ( s_doskey >= 0 && s_doskey < s_commandHistory.Count )
				{
					s_currentCommandLine = s_commandHistory[s_doskey];

					#if !XBOX
						if ( s_form != null )
						{
							s_form.SetInput( s_currentCommandLine );
						}
					#endif
				}

				s_doskeyActive = true;
			}

			static private void DoskeyDecrement()
			{
				if ( ( s_doskey == -1 ) )
				{
					s_doskey = s_commandHistory.Count - 1;
				}
				else
				{
					// This is how Windows Command Prompt does it; means that up/enter/up/enter/up/enter
					// does the same command three times, it doesn't move up through the list three times.
					// Does not work the same way if you press down though.
					if ( s_doskeyActive )
					{
						s_doskey = (int)MathHelper.Max( s_doskey - 1, 0 );
					}
				}

				if ( s_doskey >= 0 && s_doskey < s_commandHistory.Count )
				{
					s_currentCommandLine = s_commandHistory[s_doskey];
					
					#if !XBOX
						if ( s_form != null )
						{
							s_form.SetInput( s_currentCommandLine );
						}
					#endif
				}

				s_doskeyActive = true;
			}

			public static char KeyToChar( Microsoft.Xna.Framework.Input.Keys key, bool shiftPressed )
			{
                // Sorry guys - the complete code that I use for this internally is from the book
                // Professional XNA Game Programming (For Xbox 360 and Windows), by Benjamin Nitschke.
                // Unfortunately as I don't own the copyright on that code, I can't distribute it!
                // I've edited this to comprise a "crippled" equivalent version but you might
                // encounter some funny results if you press unusual key combinations, in which case
                // I'm afraid I'll have to leave you to figure it out for yourself, and this is really
                // very ugly and inelegant. Again, sorry.

                switch (key)
                {
                    case Microsoft.Xna.Framework.Input.Keys.A: return (shiftPressed ? 'A' : 'a');
                    case Microsoft.Xna.Framework.Input.Keys.B: return (shiftPressed ? 'B' : 'b');
                    case Microsoft.Xna.Framework.Input.Keys.C: return (shiftPressed ? 'C' : 'c');
                    case Microsoft.Xna.Framework.Input.Keys.D: return (shiftPressed ? 'D' : 'd');
                    case Microsoft.Xna.Framework.Input.Keys.E: return (shiftPressed ? 'E' : 'e');
                    case Microsoft.Xna.Framework.Input.Keys.F: return (shiftPressed ? 'F' : 'f');
                    case Microsoft.Xna.Framework.Input.Keys.G: return (shiftPressed ? 'G' : 'g');
                    case Microsoft.Xna.Framework.Input.Keys.H: return (shiftPressed ? 'H' : 'h');
                    case Microsoft.Xna.Framework.Input.Keys.I: return (shiftPressed ? 'I' : 'i');
                    case Microsoft.Xna.Framework.Input.Keys.J: return (shiftPressed ? 'J' : 'j');
                    case Microsoft.Xna.Framework.Input.Keys.K: return (shiftPressed ? 'K' : 'k');
                    case Microsoft.Xna.Framework.Input.Keys.L: return (shiftPressed ? 'L' : 'l');
                    case Microsoft.Xna.Framework.Input.Keys.M: return (shiftPressed ? 'M' : 'm');
                    case Microsoft.Xna.Framework.Input.Keys.N: return (shiftPressed ? 'N' : 'n');
                    case Microsoft.Xna.Framework.Input.Keys.O: return (shiftPressed ? 'O' : 'o');
                    case Microsoft.Xna.Framework.Input.Keys.P: return (shiftPressed ? 'P' : 'p');
                    case Microsoft.Xna.Framework.Input.Keys.Q: return (shiftPressed ? 'Q' : 'q');
                    case Microsoft.Xna.Framework.Input.Keys.R: return (shiftPressed ? 'R' : 'r');
                    case Microsoft.Xna.Framework.Input.Keys.S: return (shiftPressed ? 'S' : 's');
                    case Microsoft.Xna.Framework.Input.Keys.T: return (shiftPressed ? 'T' : 't');
                    case Microsoft.Xna.Framework.Input.Keys.U: return (shiftPressed ? 'U' : 'u');
                    case Microsoft.Xna.Framework.Input.Keys.V: return (shiftPressed ? 'V' : 'v');
                    case Microsoft.Xna.Framework.Input.Keys.W: return (shiftPressed ? 'W' : 'w');
                    case Microsoft.Xna.Framework.Input.Keys.X: return (shiftPressed ? 'X' : 'x');
                    case Microsoft.Xna.Framework.Input.Keys.Y: return (shiftPressed ? 'Y' : 'y');
                    case Microsoft.Xna.Framework.Input.Keys.Z: return (shiftPressed ? 'Z' : 'z');
                    case Microsoft.Xna.Framework.Input.Keys.D0: return '0';
                    case Microsoft.Xna.Framework.Input.Keys.D1: return '1';
                    case Microsoft.Xna.Framework.Input.Keys.D2: return '2';
                    case Microsoft.Xna.Framework.Input.Keys.D3: return '3';
                    case Microsoft.Xna.Framework.Input.Keys.D4: return '4';
                    case Microsoft.Xna.Framework.Input.Keys.D5: return '5';
                    case Microsoft.Xna.Framework.Input.Keys.D6: return '6';
                    case Microsoft.Xna.Framework.Input.Keys.D7: return '7';
                    case Microsoft.Xna.Framework.Input.Keys.D8: return '8';
                    case Microsoft.Xna.Framework.Input.Keys.D9: return '9';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad0: return '0';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad1: return '1';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad2: return '2';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad3: return '3';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad4: return '4';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad5: return '5';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad6: return '6';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad7: return '7';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad8: return '8';
                    case Microsoft.Xna.Framework.Input.Keys.NumPad9: return '9';
                }

                return ' ';     // Yeah... this sucks. Sorry... again!
			}

			
			public static bool IsSpecialKey( Microsoft.Xna.Framework.Input.Keys key )
			{
                // This is another function fully implemented in the book mentioned above.
                // So again, you may encounter some strange behaviour. Sorry.

                if ( key == Microsoft.Xna.Framework.Input.Keys.Escape
                    || key == Microsoft.Xna.Framework.Input.Keys.Enter
                    || key == Microsoft.Xna.Framework.Input.Keys.Up
                    || key == Microsoft.Xna.Framework.Input.Keys.Down
                    || key == Microsoft.Xna.Framework.Input.Keys.Back )
                {
                    return true;
                }

                return false;
            }

			#endregion

			#region Command Delegates

			static private void HelpDelegate( string[] commandArguments )
			{
				// The [0]th element in the array will always be the name of the command itself, in this case, "help"
				if ( commandArguments.Length > 1 )
				{
					CommandEntry currentCommand;

					if ( s_commands.TryGetValue( commandArguments[1], out currentCommand ) )
					{
						Print( commandArguments[1] + ": " + currentCommand.m_help );
					}
					else
					{
						Print( "Error: unknown command \"" + commandArguments[1] + "\"." );
					}
				}
				else
				{
					Print( "Type a command, then press Enter, or press Esc to exit.\nUse \"help <command>\" to get help for a specific command.\nCommands available:" );
					List<string> commands = new List<string>();

					foreach ( string command in s_commands.Keys )
					{
						commands.Add( command );
					}

					commands.Sort();

					foreach ( string command in commands )
					{
						Print( "    " + command );
					}
				}
			}

			static private void ClsDelegate( string[] commandArguments )
			{
				s_output.Clear();

#if !XBOX
				if ( s_form != null )
				{
					s_form.Cls();
				}
#endif
			}

			static private void ClearDoskeyDelegate( string[] commandArguments )
			{
				s_commandHistory.Clear();
				s_doskeyActive = false;
				s_doskey = -1;
			}

			static private void ExitDelegate( string[] commandArguments )
			{
				s_active = false;

#if !XBOX
				if ( s_form != null )
				{
					s_form.Dispose();
					s_form = null;
				}
#endif			
			}

			static private void CollectGarbageDelegate( string[] commandArguments )
			{
				GC.Collect();
			}

#if !XBOX
			static private void DialogBoxDelegate( string[] commandArguments )
			{
				DoModelessDialog();
			}
#endif

            #endregion

			#region Modeless Dialog Box

			/// <summary>
			/// Displays a modeless dialog box, containing the command line and all previous
			/// command output. Honestly this is far superior to the manual draw code and you
			/// should use it whenever possible, ie. on Windows. Must still support the old
			/// method for use on Xbox (so it might just as well therefore work on Windows too).
			/// </summary>
			public static void DoModelessDialog()
			{
#if !XBOX
				// XXX for this to work, the Main method of the running program must be
				// marked with [STAThread], and it is not supported at all on Xbox 360.

				if ( s_form != null )
				{
					s_form.Dispose();
				}

				s_form = new CommandForm();

				if ( s_output.Count == 0 )
				{
					// Initialise the dialog box output with something useful
					HelpDelegate( new string[0] );
				}

				// Modeless dialog box
				s_form.Show();
#endif
			}

#if !XBOX
			private class CommandForm : Form
			{
				#region Constants

				static readonly int InitialWidth = 320;
				static readonly int InitialHeight = 450;

				#endregion

				#region Construction and Destruction

				internal CommandForm()
				{
					Width = InitialWidth;
					Height = InitialHeight;

					SuspendLayout();

					// OK button, not really needed as you can just hit Enter but does no harm
					m_button.SuspendLayout();
					m_button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					m_button.Dock = DockStyle.None;
					m_button.Text = "OK";
					m_button.Size = m_button.PreferredSize;
					m_button.Location = new System.Drawing.Point( ClientSize.Width - m_button.Width, ClientSize.Height - m_button.Height );
					m_button.Click += Form_CommandEntered;
					m_button.ResumeLayout( false );
					Controls.Add( m_button );

					// Where the user enters text
					m_commandLine.SuspendLayout();
					m_commandLine.AcceptsReturn = false;
					m_commandLine.AcceptsTab = false;
					m_commandLine.AllowDrop = true;
					m_commandLine.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
					m_commandLine.Dock = DockStyle.None;
					m_commandLine.Location = new System.Drawing.Point( 0, ClientSize.Height - m_commandLine.Height );
					m_commandLine.Multiline = false;
					m_commandLine.ReadOnly = false;
					m_commandLine.ScrollBars = ScrollBars.None;
					m_commandLine.WordWrap = false;
					m_commandLine.Width = ClientSize.Width - m_button.Width;
					m_commandLine.KeyDown += Input_KeyDown;
					m_commandLine.ResumeLayout( false );
					Controls.Add( m_commandLine );

					// Where the results of the user's input are displayed
					m_commandOutput.SuspendLayout();
					m_commandOutput.AcceptsReturn = false;
					m_commandOutput.AcceptsTab = false;
					m_commandOutput.AllowDrop = false;
					m_commandOutput.Anchor = AnchorStyles.Top;
					m_commandOutput.Dock = DockStyle.Top;
					m_commandOutput.Multiline = true;
					m_commandOutput.ReadOnly = true;
					m_commandOutput.ScrollBars = ScrollBars.Vertical;
					m_commandOutput.WordWrap = true;
					m_commandOutput.ResumeLayout( false );
					m_commandOutput.Height = ClientSize.Height - m_button.Height;
					m_commandOutput.Width = ClientSize.Width;
					Controls.Add( m_commandOutput );

					foreach ( string command in Command.s_output )
					{
						Print( command );
					}
					
					// The form itself
					Name = "DevCommand";
					Text = "Dev Command";
					ClientSizeChanged += Form_ClientSizeChanged;

					ResumeLayout( false );
				}

				/// <summary>
				/// Clean up any resources being used.
				/// </summary>
				/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
				protected override void Dispose( bool disposing )
				{
					if ( disposing )
					{
						
					}
					base.Dispose( disposing );
				}

				#endregion

				#region Variables

				private Button m_button = new Button();
				private TextBox m_commandLine = new TextBox();
				private TextBox m_commandOutput = new TextBox();
				
				#endregion

				#region Class Behaviour

				internal void Print( string text )
				{
					// TODO don't replace \n if it's already \r\n! But it won't be... right?
					m_commandOutput.Text += text.Replace( "\n", "\r\n" );
					
					if ( !m_commandOutput.Text.EndsWith( "\r\n" ) )
					{
						m_commandOutput.Text += "\r\n";
					}
					
					m_commandOutput.SelectionStart = m_commandOutput.Text.Length;
					m_commandOutput.SelectionLength = 0;
					m_commandOutput.ScrollToCaret();
				}

				internal void Cls()
				{
					m_commandOutput.Clear();
					m_commandOutput.ScrollToCaret();
				}

				internal void SetInput( string input )
				{
					m_commandLine.Text = input;
				}

				#endregion

				#region Event Delegates

				private void Form_ClientSizeChanged( object sender, EventArgs e )
				{
					CommandForm form = (CommandForm)sender;

					form.m_commandLine.Width = form.ClientSize.Width - m_button.Width;
					form.m_commandOutput.Height = form.ClientSize.Height - form.m_commandLine.Height;
					form.m_commandOutput.Width = form.ClientSize.Width;
					m_commandOutput.ScrollToCaret();
				}

				private void Form_CommandEntered( object sender, EventArgs e )
				{
					Button button = (Button)sender;
					CommandForm form = (CommandForm)button.Parent;

					Command.ProcessCommand( form.m_commandLine.Text );
					form.m_commandLine.Clear();
				}

				private void Input_KeyDown( object sender, EventArgs e )
				{
					TextBox textbox = (TextBox)sender;
					CommandForm form = (CommandForm)textbox.Parent;

					KeyEventArgs args = (KeyEventArgs)e;

					if ( args.KeyCode == System.Windows.Forms.Keys.Return )
					{
						Command.ProcessCommand( form.m_commandLine.Text );
						textbox.Clear();
						args.Handled = true;
					}
					else if ( args.KeyCode == System.Windows.Forms.Keys.Down )
					{
						Command.DoskeyIncrement();
						args.Handled = true;
					}
					else if ( args.KeyCode == System.Windows.Forms.Keys.Up )
					{
						Command.DoskeyDecrement();
						args.Handled = true;
					}
				}

				#endregion
			}
#endif
			#endregion

			#region Case insensitive string comparer, you might like to use this in other stuff too

			private class InsensitiveComparer : IEqualityComparer<string>
			{
				CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();

				public int GetHashCode( string str )
				{
					 return str.ToLowerInvariant().GetHashCode();
				}

				public bool Equals( string lhs, string rhs )
				{
					return ( comparer.Compare( lhs, rhs ) == 0 );
				}
			}

			#endregion
        }
    }
}
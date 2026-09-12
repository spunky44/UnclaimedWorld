#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using UWGame.SimSide;
using InputEventSystem;
using UWGame.Control;
using System.Globalization;
using UWGame;
using System.Reflection;
using Steamworks;
using System.Diagnostics;
using System.Runtime;
using UWGame.ClientSide.Screens;
using System.IO;

#endregion


namespace GameStateManagement //TODO DECOUPLE -- I think this belongs outside either the sim or the client MLo
{
   
    public class UnclaimedWorld : Microsoft.Xna.Framework.Game
    {
        #region Fields

        public GraphicsDeviceManager GraphicsDeviceManager;
        public Controller Controller;

       // public MultiSampleType MultiSampleTypeToUse;

        /// <summary>
        /// the user is able to turn this on an off:
        /// </summary>
      //  public bool AntiAliasingEnabled = true; //true;

        
        #endregion

        #region Initialization


        public Point GetScreenResolution()
        {
            return new Point(GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight);

        }

        /// <summary>
        /// The main game constructor.
        /// </summary>
        public UnclaimedWorld(/*TraceSource traceSource*/)
        {
            // setting the thread culture as a precaution - this will affect many formatting calls...
            System.Threading.Thread.CurrentThread.CurrentCulture = Config.Culture;
            
            GraphicsDeviceManager = new GraphicsDeviceManager(this);

            // GraphicsDeviceManager.HardwareModeSwitch = false; // has no effect...

            GraphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;

            //graphics.PreferredDepthStencilFormat = SelectStencilMode(); // XNA 3
            GraphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            GraphicsDeviceManager.PreferMultiSampling = true; // NEW XNA 4

#if PROFILE
            // SHAWN's PROFILE SETTINGS:
             this.IsFixedTimeStep = false; // DEFAULT: TRUE
           //  GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true; // false; // VSYNC

            // discussion of these settings and their relation to jerkiness/stutter once per second:
         // http://forums.xna.com/forums/p/30500/173566.aspx#173566
         //   this.IsFixedTimeStep = false;
         //   graphics.SynchronizeWithVerticalRetrace = true; // false;
#else
            this.IsFixedTimeStep = false; // DEFAULT: TRUE    LARS: I get atrocious framerates without these settings!
            // GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false; // VSYNC - this limits framerates/cpu usage when true

#endif




          //  Debug.Assert(false);

#if RELEASE
            try
            {
#endif

                // Create the screen manager component.
                Controller = new Controller(this);
                Controller.ScreenTraceEnabled = false; // true;
                Components.Add(Controller);



                // NEW: VSync is an option:
                GraphicsDeviceManager.SynchronizeWithVerticalRetrace = Controller.Options.SynchronizeWithVerticalRetrace;


                // Activate the first screens.
                Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.Normal));
                MainMenuScreen mainMenuScreen = new MainMenuScreen(this);
                Controller.AddScreen(mainMenuScreen);


                IngameLoadGameScreen ingameLoadGameScreen = new IngameLoadGameScreen(this); // Controller, "");
                The.IngameLoadScreen = ingameLoadGameScreen;

                // DEBUG: starts game directly: 
                //  LoadingScreen.Load(ScreenManager, mainMenuScreen.LoadGameplayScreen, true);

                InitTracing(false);

                // Lars: Steamworks.NET needs the two dlls CSteamworks.dll and steam_api.dll in the correct version for the bitness platform
              /*  if (Controller.Options.EnableSteam) // has no effect on the Steam overlay...
                {
                    if (!SteamAPI.Init()) // tries to load CSteamworks.dll
                    {

                        Console.WriteLine("SteamAPI.Init() failed!");
                        //  return;
                    }

                    if (!Packsize.Test())
                    {
                        Console.WriteLine("You're using the wrong Steamworks.NET Assembly for this platform!");
                        //	return;
                    }

                    if (!DllCheck.Test()) // tests platform version of steam_api.dll
                    {
                        Console.WriteLine("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
                    }

                    if (SteamAPI.IsSteamRunning()) // SteamManager.Initialized)
                    {
                        string name = SteamFriends.GetPersonaName();
                        Console.WriteLine(name);

                  
                    }
                }*/

#if RELEASE
            }
            catch (Exception e)
            {
                // catches errors from both Update and Draw
                HandleExceptionInReleaseMode(e, true);
            }
#endif

        }


       /* public class MyTraceListener : TraceListener
        {
            public override void Write(string msg)
            {
                throw new Exception(msg);
            }
            public override void WriteLine(string msg)
            {
                throw new Exception(msg);
            }
        }*/


        


        /// <summary>
        /// tracing requires the TRACE symbol to have been defined (Properties)
        /// </summary>
        private void InitTracing(bool enableTracing)
        {
            if (enableTracing)
            {
                TextWriterTraceListener textListener = new TextWriterTraceListener("trace.log");

                Trace.Listeners.Add(textListener);
                Trace.AutoFlush = true;

                textListener.Filter = new EventTypeFilter(SourceLevels.All);


                System.Diagnostics.Trace.WriteLine("InitTracing");
            }
            else
            {
                // this disables asserts too:
               // Trace.Listeners.Clear(); // removes the default listener that writes to Output
            }
                       
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // prevent clearing of back buffer when calling SetRenderTarget:
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            // is this the way to do it? seems to work.
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;

            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;


            
        }


        const int maxFSRetries = 5;

        /// <summary>
        /// BEFORE LoadContent!
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Content.RootDirectory = "Content";

            DisplayModeCollection modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;

            
            //set the graphics device to the same dimentions (resolution) as the current desktop setting

        
            // To get a youtube HD / Steam screenie resolution of 1920X1080, you must set it to 1950X1130  because of some adjustments that are made.
            // resolution guide:
            // used on pc1 and pc2 for test etc: w1440 h900 
            // the best resolution for experiencing the game windowed on a smaller screen: w1680 h1050
 
            
            // catch device not ready error here?
            // bring window to front and give keyboard focus? http://www.sivachandran.in/2014/06/enabling-direct3d-fullscreen-mode-from.html

            bool retry = false;
            int retries = 0;
            do
            {
                try
                {
                    int width;
                    int height;
                    if (Controller.Options.FullScreen == false || Controller.Options.HardwareModeSwitch)
                    {
                        width = Controller.Options.ResolutionWidth;
                        height = Controller.Options.ResolutionHeight;
                    }
                    else
                    {
                        width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    }

                    SetScreenResolution(Controller.Options.FullScreen, Controller.Options.Borderless, Controller.Options.HardwareModeSwitch, width, height);
           
                    GraphicsDeviceManager.ApplyChanges();


                    retry = false;
                }
                catch (Exception e) 
                {
                    /*
                     Type: SharpDX.SharpDXException
                        Message: HRESULT: [0x887A0022], Module: [SharpDX.DXGI], ApiCode: [DXGI_ERROR_NOT_CURRENTLY_AVAILABLE/NotCurrentlyAvailable], Message: A resource is not available at the time of the call, but may become available later.

                        Source: SharpDX
                        Stack Trace:
                           at SharpDX.Result.CheckError()
                           at SharpDX.DXGI.SwapChain.SetFullscreenState(Bool fullscreen, Output targetRef)
                           at Microsoft.Xna.Framework.Graphics.GraphicsDevice.CreateSizeDependentResources(Boolean useFullscreenParameter)
                           at MonoGame.Framework.WinFormsGamePlatform.EnterFullScreen()                     
                     */


                    if (e.Message.Contains("DXGI_ERROR_NOT_CURRENTLY_AVAILABLE")) // e.HResult == 0x887A0022) 
                    {
                        
                        System.Windows.Forms.Form windowForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);

                        string status = string.Format("IsActive: {0}, TopMost: {1}, Focused: {2}, ContainsFocus: {3}" + Environment.NewLine, IsActive, windowForm.TopMost, windowForm.Focused, windowForm.ContainsFocus);
                        

                       
                        if (retries == maxFSRetries)
                        {
                            // try without mode switch:

                            GraphicsDeviceManager.HardwareModeSwitch = false;

                            string title = "The game failed to start in fullscreen mode (DXGI_ERROR_NOT_CURRENTLY_AVAILABLE).";

                            string message = status;

                            message += "Attempting to start without resolution mode switch.";
                                /*+ Environment.NewLine + "You can try again, or you can switch to windowed mode instead, by changing the FullScreen property to False in Options.xml (this file can be found in the Documents/Unclaimed World folder.)"
                                + Environment.NewLine + Environment.NewLine + "If you need more help, press Ctrl-C to copy this text and then paste it into the Steam forum.";
                                */
                            LogError(message, title);


                            /*
                            throw new UWException(message, e)
                            {
                                IncludePasteInstructions = false
                            };*/

                        }
                        else if (retries < maxFSRetries)
                        {
                            // log each retry:
                            string title = "DXGI_ERROR_NOT_CURRENTLY_AVAILABLE, retry: " + retries.ToString() + ", v. " + UnclaimedWorld.GetVersionAsString();

                            string message = status;
                            message += GetErrorMessage(e);


                            LogError(message, title); // log, don't throw
                            
                            System.Threading.Thread.Sleep(100);

                            // attempt to fix focus issue:
                            windowForm.TopMost = true;
                            windowForm.BringToFront();
                        }
                        else
                        {
                           
                            string message = string.Format("IsActive: {0}, TopMost: {1}" + Environment.NewLine, IsActive, windowForm.TopMost);

                            message += "The game failed to start in fullscreen mode (DXGI_ERROR_NOT_CURRENTLY_AVAILABLE). This may be because the game window is out of focus."
                                + Environment.NewLine + "You can try again, or you can switch to windowed mode instead, by changing the FullScreen property to False in Options.xml (this file can be found in the Documents/Unclaimed World folder.)"
                                + Environment.NewLine + Environment.NewLine + "If you need more help, press Ctrl-C to copy this text and then paste it into the Steam forum.";

                            throw new UWException(message, e)
                            {
                                IncludePasteInstructions = false
                            };

                            /*
                            throw;*/
                        }

                        retries++;

                        retry = true;
                    }
                }
            }
            while(retry == true);

            this.IsMouseVisible = true;

            base.Initialize();


        }
        #endregion

        public static readonly Point MaxScreenDimensions = new Point(4096, 4096);
        
        private void SetWindowSize(int width, int height)
        {
            int clampedWidth = Common.ClampTop(width, MaxScreenDimensions.X); // XNA will give an exception if larger resolution is attempted...
            int clampedHeight = Common.ClampTop(height, MaxScreenDimensions.Y);

            if (GraphicsDeviceManager.PreferredBackBufferWidth != clampedWidth || GraphicsDeviceManager.PreferredBackBufferHeight != clampedHeight)
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = clampedWidth;
                GraphicsDeviceManager.PreferredBackBufferHeight = clampedHeight;
            }
        }

        public void SetScreenResolution(bool useFullscreen, bool borderLess, bool hardwareModeSwitch, int width, int height)
        {
        
            if (useFullscreen)
            {
                SetWindowSize(width, height);

                GraphicsDeviceManager.HardwareModeSwitch = hardwareModeSwitch;

                //if the game is not currently full screen, make it so  
                if (GraphicsDeviceManager.IsFullScreen == false)
                {
                    GraphicsDeviceManager.ToggleFullScreen();

                   // GraphicsDeviceManager.ApplyChanges();
                }
            }
            else
            {
                // reduce the size when in windowed mode (to account for window borders?) otherwise the display is scrunched and the mouse becomes offset
                SetWindowSize(width - 30, height - 50);


                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                var form = control.FindForm();
                if (borderLess)
                {
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                }
                else
                {
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
                }


                if (GraphicsDeviceManager.IsFullScreen == true)
                {
                    GraphicsDeviceManager.ToggleFullScreen();

                  //  graphics.ApplyChanges();
                }

            }
        }


        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion

        public void HandleExceptionInReleaseMode(Exception e, bool isMainThread)
        {
            if (isMainThread)
            {
                // threads cannot do this:
                SetScreenResolution(false, false, false, 1024, 768);
            }
  
            string title = GetTitle(e);
            // Get stack trace for the exception with source file information
            string message = GetErrorMessage(e);
          
            // show modal dialog just before we die:
            if (isMainThread)
            {
                try
                {
                    string dialogInstructions = GetPasteInstructions(e); // "Whoops - fatal error. Press Ctrl-C to copy the contents of this dialog and paste the text into the forums: " + Environment.NewLine + Environment.NewLine;

                    //  throw new Exception("blaaaa");

                    System.Windows.Forms.MessageBox.Show(new WindowHandle(Window.Handle),
                       dialogInstructions + message, title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                catch (Exception e2)
                {
                    message += Environment.NewLine + "----------------";
                    message += Environment.NewLine;
                    message += "MessageBox failed: " + e2.Message;
                    message += Environment.NewLine;
                    message += e2.StackTrace;
                }
            }

            // NEW: Always write a text dump also. sometimes the dialog doesn't appear?
            LogError(message, title);

            // didn't work:
            //  CreateSteamMiniDump("Fatal error " + UnclaimedWorld.GetVersionAsString() + "\n" + message, e);


            throw (e); // now die...
        }

        static readonly string dialogInstructions = "Whoops - fatal error. Press Ctrl-C to copy the contents of this dialog and paste the text into the forums: " + Environment.NewLine + Environment.NewLine;

        private static string GetPasteInstructions(Exception e)
        {
            UWException uwException = e as UWException;
            if (uwException != null)
            {
                if (uwException.IncludePasteInstructions)
                {
                    return dialogInstructions;
                }
                else return "";
            }
            else
            {
                return dialogInstructions;
            }
        }


        private static string GetTitle(Exception e)
        {
            string title;
            string defaultTitle = "Fatal error " + UnclaimedWorld.GetVersionAsString();
            UWException uwException = e as UWException;
            if (uwException != null)
            {
                title = uwException.Title ?? defaultTitle;
            }
            else
            {
                title = defaultTitle;
            }
           

            return title;
        }

        private static string GetErrorMessage(Exception e)
        {
            int line = -1;

            var st = new StackTrace(e, true);
            if (st != null)
            {
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                if (frame != null)
                {
                    // is null in non-main thread
                    line = frame.GetFileLineNumber();
                }
            }

            string message =
                e.Message
                + Environment.NewLine
                + "Line: " + line
                + Environment.NewLine
                + e.StackTrace;
            ;

            if (e.InnerException != null) // is filled for wrapped exceptions
            {
                message += Environment.NewLine;
                message += e.InnerException.Message;
                message += Environment.NewLine;
                message += e.InnerException.StackTrace;
            }
            return message;
        }

        /// <summary>
        /// appends a message in the Error.txt file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public static void LogError(string message, string title)
        {
            string filePath = @"Errors.txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("---------------------------------------");
                    writer.WriteLine(title);
                    writer.WriteLine("Date :" + DateTime.Now.ToString());
                    writer.Write(message);
                    writer.Write(Environment.NewLine);
                    writer.Write(Environment.NewLine);
                }
            }
            catch (Exception e2)
            { }
        }

        public class WindowHandle : System.Windows.Forms.IWin32Window
        {
            public WindowHandle(IntPtr handle)
            {
                _hwnd = handle;
            }

            public IntPtr Handle
            {
                get { return _hwnd; }
            }

            private IntPtr _hwnd;
        }


        public static System.Version GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return AssemblyName.GetAssemblyName(assembly.Location).Version;
        }

        public static string GetVersionAsString()
        {
            return GetVersion().ToString();
        }
    }


    public class UWException: Exception
    {
        public string Title;

        public bool IncludePasteInstructions = true;


        public UWException(string message, Exception innerException): base(message, innerException)
        {

        }
    }

    #region Entry Point
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
       // private static TraceSource traceSource = new TraceSource("UnclaimedWorld");

        [STAThread]
        static void Main()
        {

            /* for (int i = 0; i < 3; i++)
             {
                 try
                 {
                     throw new Exception("TestException");
                 }
                 catch (Exception e)
                 {
                     MiniDumpFunction("test", e);
                 }
             }*/

            // from https://github.com/MonoGame/MonoGame/issues/4287
            // detect Media Feature Pack presence
            object legacyWMPCheck = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Active Setup\Installed Components\{22d6f312-b0f6-11d0-94ab-0080c74c7e95}", "IsInstalled", null);
            if (legacyWMPCheck == null || legacyWMPCheck.ToString() != "1")
            {
                System.Windows.Forms.MessageBox.Show("It appears that you don't have Windows Media Player installed. This game needs system features bound to Windows Media Player. Please install the Media Feature Pack corresponding to your Windows version to run this game:"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Windows 7: http://www.microsoft.com/en-US/download/details.aspx?id=16546"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Windows 8: http://www.microsoft.com/en-US/download/details.aspx?id=30685"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Windows 8.1: http://www.microsoft.com/en-US/download/details.aspx?id=40744"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Windows 10: https://www.microsoft.com/en-US/download/details.aspx?id=48231"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Windows Vista & XP: http://www.microsoft.com/en-US/download/windows-media-player-details.aspx?id=8163",
                    "Missing Windows Media Player", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }

            using (UnclaimedWorld game = new UnclaimedWorld(/*traceSource*/))
            {
                game.Exiting += game_Exiting;

#if RELEASE
                try
                {
#endif

                    game.Run();
#if RELEASE
                }
                catch (Exception e)
                {
                    // catches errors from both Update and Draw
                    game.HandleExceptionInReleaseMode(e, true);
                }
#endif
            }


        }


       


        static void game_Exiting(object sender, EventArgs e)
        {
            UnclaimedWorld game = sender as UnclaimedWorld;

            game.Controller.Destroy();


          /*  if (SteamAPI.IsSteamRunning())
            {
                SteamAPI.Shutdown();
            }*/


           // traceSource.Close();

        }

        static void OnProcessExit(object sender, EventArgs e)
        {
           
        }

        
    }

    #endregion


    
}

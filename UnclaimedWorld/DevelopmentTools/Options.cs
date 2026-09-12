using System;
using System.Collections.Generic;
using System.Text;
#if !XBOX360
using System.Drawing;
using System.Windows.Forms;
using UWGame.SimSide.Entities;
using UWGame;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide;
#endif

namespace Kensei
{
    namespace Dev
    {
        /// <summary>
        /// A simple class consisting entirely of static members, to set and unset options
        /// for use during development and debugging.
        /// </summary>
        public static class Options
        {
            #region Class Behaviour

            internal static void Initialise()
            {
                s_optionsBool = new Dictionary<string, bool>();
                s_callbacks = new Dictionary<string, OptionChangedFunction>();

                /*
                Kensei.Dev.Command.AddCommand("SetOption", SetOptionDelegate, "Sets the value of a dev option. SetOption <option> [<true|1|false|0>]");
                Kensei.Dev.Command.AddCommand("GetOption", GetOptionDelegate, "Gets the value of a dev option. GetOption <option>");
                Kensei.Dev.Command.AddCommand("ListOptions", ListOptionsDelegate, "Lists all available dev options. ListOptions [<prefix>]");
                Kensei.Dev.Command.AddCommand("Options", OptionsDelegate, "Brings up a combo box offering tickboxes for each dev option.");
                */

            }

            #endregion

            #region Dev Options Accessors


            public enum DebugButton
            {
                MakeAButton
            }

            /// <summary>
            /// Sets a button dev option, inserting it into the map if not already present.
            /// </summary>
            /// <param name="optionName">The name of the option.</param>
            /// <param name="boolToSet">The new value for the option.</param>
            static public void SetOption(string optionName, DebugButton butt, bool doCallback = false)
            {
#if !XBOX
                if (s_form != null)
                {
                    if (!s_optionsButtons.ContainsKey(optionName))
                    {
                        s_form.AddOption(optionName);
                    }
                }
#endif
                s_optionsButtons[optionName] = butt;

                if (doCallback)
                {
                    if (s_callbacks.ContainsKey(optionName))
                    {
                        s_callbacks[optionName](optionName, null, null);
                    }
                    else if (optionName.StartsWith("ItemTypes") || optionName.StartsWith("StructureTypes") || optionName.StartsWith("EntityTypes") || optionName.StartsWith("TerrainTypes"))
                    {
                        //select the next entity of this type
                        int index = optionName.IndexOf(".", StringComparison.Ordinal);
                        string typeKey = optionName.Substring(index + 1, optionName.Length - (index + 1));
                        The.InGameUI.SelectNextEntityOfType(typeKey);

                    }
                }
            }

            static public void SetOption(string optionName, EntityType type, bool doCallback = false)
            {

                if (s_form != null)
                {
                    if (!s_optionsEntityTypes.ContainsKey(optionName))
                    {
                        s_form.AddButton(optionName, type);
                    }
                }

                s_optionsButtons[optionName] = DebugButton.MakeAButton;//cheese

                if (doCallback)
                {
                    if (s_callbacks.ContainsKey(optionName))
                    {
                        s_callbacks[optionName](optionName, null, null);
                    }
                }

            }



            /// <summary>
            /// Sets the bool of a dev option, inserting it into the map if not already present.
            /// </summary>
            /// <param name="optionName">The name of the option.</param>
            /// <param name="boolToSet">The new value for the option.</param>
            static public void SetOption(string optionName, bool boolToSet, bool doCallback = false)
            {
#if !XBOX
                if (s_form != null)
                {
                    if (!s_optionsBool.ContainsKey(optionName))
                    {
                        s_form.AddOption(optionName, boolToSet);
                    }
                    else
                    {
                        s_form.UpdateOption(optionName, boolToSet);
                    }
                }
#endif
                s_optionsBool[optionName] = boolToSet;

                if (doCallback)
                {
                    if (s_callbacks.ContainsKey(optionName))
                    {
                        s_callbacks[optionName](optionName, boolToSet, null);
                    }
                }
            }


            /// <summary>
            /// Sets the float value of a dev option, inserting it into the map if not already present.
            /// </summary>
            /// <param name="optionName">The name of the option.</param>
            /// <param name="defaultVal">The new value for the option.</param>
            static public void SetOption(string optionName, float val, float min, float max, bool doCallback = false)
            {
#if !XBOX
                if (s_form != null)
                {
                    if (!s_optionsFloat.ContainsKey(optionName))
                    {
                        s_form.AddOption(optionName, val, min, max);
                    }
                    else
                    {
                        s_form.UpdateOption(optionName, val);
                    }
                }
#endif
                s_optionsFloat[optionName] = val;

                if (doCallback)
                {
                    if (s_callbacks.ContainsKey(optionName))
                    {
                        s_callbacks[optionName](optionName, null, val);
                    }
                }
            }

           /* static public void SetThreatLabelText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetThreatLabelText(text);
                }
            }*/
            static public void SetEntityInfoText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetEntityInfotext(text);
                }
            }

            static public void SetEntityGoalsText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetEntityGoalsText(text);
                }

            }

            static public void SetEventsText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetEventsText(text);
                }

            }

            static public void SetJobsText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetJobsText(text);
                }

            }

            static public void SetPerformanceText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetPerformanceText(text);
                }
            }

            static public void SetEmigrateRollText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetEmigrateRollText(text);
                }
            }
            

            static public void SetAllegiancesText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetAllegiancesText(text);
                }

            }

            static public void PopulateTestEvents(List<ActionSets> actions)
            {
                if (s_form != null)
                {
                    s_form.PopulateTestEvents(actions);
                }

            }

            static public void PopulatePolledTestEvents(List<PolledEventType> actions)
            {
                if (s_form != null)
                {
                    s_form.PopulatePolledTestEvents(actions);
                }

            }

            static public bool AppendEventsLogText(string text)
            {
                if (s_form != null)
                {
                    s_form.AppendEventsLogText(text);

                    return true;
                }

                return false;
            }

            static public bool AppendPopSpawnText(string text)
            {
                if (s_form != null)
                {
                    s_form.AppendPopSpawnText(text);

                    return true;
                }

                return false;
            }
            

            static public void SetSoundsText(string text)
            {
                if (s_form != null)
                {
                    s_form.SetSoundsText(text);
                }

            }

            /// <summary>
            /// An option can have up to one function which is called whenever it is changed. This can be
            /// useful to, for example, update something in the code when an option is changed, that is
            /// not able to catch the change via the usual GetOption method (eg. it's not something that is
            /// checked every frame).
            /// </summary>
            /// <param name="optionName">The option to attach the callback to.</param>
            /// <param name="callback">The callback to attach - each option may have up to one.</param>
            static public void SetOptionCallback(string optionName, OptionChangedFunction callback)
            {
                s_callbacks[optionName] = callback;
            }

            public static string ShownTab()
            {
                if (s_form != null)
                {
                    return s_form.ShownTab();
                }
                return null;
            }

            static public void RemoveOption(string name)
            {
                if (s_optionsBool.ContainsKey(name))
                {
                    s_optionsBool.Remove(name);
                    s_form.RemoveOption(name);
                }
                if (s_optionsButtons.ContainsKey(name))
                {
                    s_optionsButtons.Remove(name);
                    s_form.RemoveOption(name);
                }
                if (s_optionsEntityTypes.ContainsKey(name))
                {
                    s_optionsEntityTypes.Remove(name);
                    s_form.RemoveOption(name);
                }
                if (s_optionsFloat.ContainsKey(name))
                {
                    s_optionsFloat.Remove(name);
                    s_form.RemoveOption(name);
                }
            }

            static public void RemoveOptionsStartingWith(string startsWith)
            {

                List<string> optionsToRemove = new List<string>();
                foreach (var item in s_optionsBool)
                {
                    if (item.Key.StartsWith(startsWith))
                    {
                        optionsToRemove.Add(item.Key);

                    }
                }
                foreach (var item in s_optionsButtons)
                {
                    if (item.Key.StartsWith(startsWith))
                    {
                        optionsToRemove.Add(item.Key);

                    }
                }

                foreach (var item in s_optionsEntityTypes)
                {
                    if (item.Key.StartsWith(startsWith))
                    {
                        optionsToRemove.Add(item.Key);

                    }
                }

                foreach (var item in s_optionsFloat)
                {
                    if (item.Key.StartsWith(startsWith))
                    {
                        optionsToRemove.Add(item.Key);

                    }
                }

                foreach (var item in optionsToRemove)
                {
                    RemoveOption(item);
                }

            }


            static public void SetOptionsStartingWith(string startsWith, bool value, bool doCallback)
            {
                List<string> optionsToRemove = new List<string>();
                foreach (var item in s_optionsBool)
                {
                    if (item.Key.StartsWith(startsWith))
                    {
                        SetOption(item.Key, value, doCallback);

                    }
                }


            }


            public static void RemoveAllOptions()
            {
                //  s_optionsBool = null;
                s_callbacks = null;
            }

            /// <summary>
            /// Gets the value of a dev option.
            /// </summary>
            /// <param name="optionName">The name of the option.</param>
            /// <param name="ifNotPresent">What to do if the option has not been previously registered.</param>
            /// <returns>The value of the named option.</returns>
            static public bool GetOption(string name, BehaviourIfNotPresent ifNotPresent)
            {
                bool value = false;

                if (s_optionsBool.TryGetValue(name, out value))
                {
                    return value;
                }
                else
                {
                    bool result = false;

                    switch (ifNotPresent)
                    {
                        case BehaviourIfNotPresent.Throw:
                            throw new OptionNotFoundException();

                        case BehaviourIfNotPresent.ReturnTrue:
                            result = true;
                            break;

                        case BehaviourIfNotPresent.ReturnFalse:
                        default:
                            result = false;
                            break;
                    }

                    // Note we also add the option to the list, so the system knows that it is being 
                    // asked for by something. This allows us to include it in the dialog box for example.
                    SetOption(name, result);
                    return result;
                }
            }

            /// <summary>
            /// Gets the value of a dev option.
            /// </summary>
            /// <param name="optionName">The name of the option.</param>
            /// <returns>The value of the named option.</returns>
            static public bool GetOption(string name)
            {
                return GetOption(name, BehaviourIfNotPresent.ReturnFalse);
            }

            #endregion

            #region Variables

            // NOTE these Dictionaries are case sensitive
            static private Dictionary<string, EntityType> s_optionsEntityTypes = new Dictionary<string, EntityType>();
            static private Dictionary<string, DebugButton> s_optionsButtons = new Dictionary<string, DebugButton>();
            static private Dictionary<string, bool> s_optionsBool = new Dictionary<string, bool>();
            static private Dictionary<string, float> s_optionsFloat = new Dictionary<string, float>();

            static private Dictionary<string, OptionChangedFunction> s_callbacks = new Dictionary<string, OptionChangedFunction>();

            static private OptionsForm s_form;
            static public Form Form
            {
                get
                {
                    return s_form as Form;
                }
            }


            #endregion

            #region Types

            [Serializable]
            public class OptionNotFoundException : Exception
            {
                public OptionNotFoundException() : base() { }
                public OptionNotFoundException(string s) : base(s) { }
                public OptionNotFoundException(string s, Exception e) : base(s, e) { }
#if !XBOX360
                protected OptionNotFoundException(System.Runtime.Serialization.SerializationInfo info,
                    System.Runtime.Serialization.StreamingContext cxt)
                    : base(info, cxt) { }
#endif
            };

            /// <summary>
            /// What to do if GetOption tries to get an option that the system doesn't yet know about
            /// </summary>
            public enum BehaviourIfNotPresent
            {
                ReturnFalse,
                ReturnTrue,
                Throw
            };

            public delegate void OptionChangedFunction(string option, bool? newBool, float? newFloat);

            #endregion

            /*
            #region Command Prompt Delegates

            static private void SetOptionDelegate(string[] arguments)
            {
                switch (arguments.Length)
                {
                    case 3:
                        {
                            bool newValue = false;

                            // Case insensitive comparison
                            if ((string.Compare(arguments[2], "true", true) == 0) || (arguments[2] == "1"))
                            {
                                newValue = true;
                            }

                            SetOption(arguments[1], newValue);
                            Kensei.Dev.Command.Print("dev option \"" + arguments[1] + "\" is now " + GetOption(arguments[1]).ToString() + ".");
                        }
                        break;

                    case 2:
                        {
                            // Toggle the option
                            SetOption(arguments[1], !GetOption(arguments[1]));
                            Kensei.Dev.Command.Print("dev option \"" + arguments[1] + "\" is now " + GetOption(arguments[1]).ToString() + ".");
                        }
                        break;

                    default:
                        {
                            Kensei.Dev.Command.Print("Invalid number of arguments. Expected: SetOption <option> [<value>]");
                        }
                        break;
                }
            }

            static private void GetOptionDelegate(string[] arguments)
            {
                if (arguments.Length == 2)
                {
                    try
                    {
                        bool optionValue = GetOption(arguments[1], BehaviourIfNotPresent.Throw);
                        Kensei.Dev.Command.Print("dev option \"" + arguments[1] + "\" is currently " + optionValue.ToString() + ".");
                    }
                    catch (OptionNotFoundException)
                    {
                        Kensei.Dev.Command.Print("dev option \"" + arguments[1] + "\" not found, will typically default to False.");
                    }
                }
                else
                {
                    Kensei.Dev.Command.Print("Invalid number of arguments. Expected: SetOption <option> <value>");
                }
            }

            static private void ListOptionsDelegate(string[] arguments)
            {
                List<string> options = new List<string>();

                foreach (string option in s_optionsBool.Keys)
                {
                    options.Add(option);
                }

                options.Sort();

                foreach (string option in options)
                {
                    if ((arguments.Length == 1)
                        || (option.StartsWith(arguments[1])))
                    {
                        Kensei.Dev.Command.Print(option + " (" + GetOption(option).ToString() + ")");
                    }
                }
            }

#if !XBOX360
            static private void OptionsDelegate(string[] arguments)
            {
                CreateDialog();
            }
#endif

            #endregion
            */

            #region Modeless Dialog Box

            /// <summary>
            /// Displays a modeless dialog box, with tab pages, containing all options, so
            /// that they may be modified at runtime quickly and easily. Tab pages are
            /// separated by "." characters.
            /// </summary>
            static public void CreateDialog()
            {

                // XXX for this to work, the Main method of the running program must be
                // marked with [STAThread], and it is not supported at all on Xbox 360.

                if (s_form != null)
                {
                    Destroy();
                }

                s_form = new OptionsForm();

                foreach (KeyValuePair<string, bool> pair in s_optionsBool)
                {
                    s_form.AddOption(pair.Key, pair.Value);
                }

                // Modeless dialog box
              
                s_form.Show(); // initdebug dialog - show/hide dialog window
               

                int screenWidth = Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                s_form.Location = new Point(screenWidth - (s_form.ClientSize.Width + 32), 0); // MLo HACK: puts debug to the right of game window

            }

            public static void Destroy()
            {
                // destroy the form when the session ends...
                if (s_form != null)
                {
                    s_form.Dispose();
                    s_form = null;

                    s_optionsEntityTypes.Clear();
                    s_optionsButtons.Clear();
                    s_optionsBool.Clear();
                    s_optionsFloat.Clear();

                    if (s_callbacks != null) // Sim can clear this too to clean up memory...
                    {
                        s_callbacks.Clear();
                    }
                }
            }

#if !XBOX
            class OptionsForm : Form
            {
                #region Constants

                static readonly int FormSize = 440;
                static readonly int CheckBoxPosition = 8;
                static readonly int CheckBoxYHeight = 23;
                static readonly int ButtonHeight = 16;
                static readonly int CheckBoxPadding = 3;

                #endregion
               // Label lblThreat;
                Label lblEntityInfo;
                Label lblEntityGoals;
                Label lblEvents, lblTestEvents, lblEventsLog, lblSounds, lblAllegiances, lblPerformance, lblEmigrateRoll, lblJobs;
                ListBox lbEventsLog;

                ComboBox ddlTestActionSets, ddlTestPolledEvents;
                TextBox tbTestEventsResult;

                ListBox lbPopulationSpawn;

                #region Construction and Destruction


                public OptionsForm()
                {
                    SuspendLayout();
                    m_tabControl.SuspendLayout();

                    m_tabControl.Location = new Point(0, 0);
                    m_tabControl.Name = "TabControl";
                    m_tabControl.SelectedIndex = 0;
                    m_tabControl.Size = new Size(FormSize, 8000);
                    m_tabControl.TabIndex = 0;
                    m_tabControl.Multiline = true;

                    AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    Controls.Add(m_tabControl);
                    ClientSize = new System.Drawing.Size(FormSize, FormSize * 2);
                    ClientSizeChanged += new EventHandler(Form_ClientSizeChanged);


                    this.Location = new Point(1000, 0);

                    this.ResizeEnd += OptionsForm_ResizeEnd;

                    this.Resize += OptionsForm_Resize;

                    Name = "Dev Options";
                    Text = "Dev Options";

                    //*******

                    TabPage page = GetTabPage("Output");
                   // TabPage page2 = GetTabPage("ThreatMap");

                   /* lblThreat = new Label();
                    lblThreat.AutoSize = true;
                    lblThreat.Text = "Current Threat Stance";
                    lblThreat.Location = new System.Drawing.Point(10, 10);
                    page2.Controls.Add(lblThreat);*/

                    lblEntityInfo = new Label();

                    lblEntityInfo.AutoSize = true;
                    lblEntityInfo.Location = new System.Drawing.Point(10, 10);

                    lblEntityInfo.Text = "ENTITY INFO";
                    page.Controls.Add(lblEntityInfo);

                    lblEntityGoals = new Label();

                    lblEntityGoals.AutoSize = true;
                    lblEntityGoals.Location = new System.Drawing.Point(10, 600); // 500);

                    lblEntityGoals.Text = "ENTITY GOALS";
                    page.Controls.Add(lblEntityGoals);

                    // sounds
                    page = GetTabPage("Sounds");
                    lblSounds = new Label();

                    lblSounds.AutoSize = true;
                    lblSounds.Location = new System.Drawing.Point(10, 10);

                    lblSounds.Text = "SOUNDS";
                    page.Controls.Add(lblSounds);

                    /****************
                     * */
                    page = GetTabPage("Allegiances");
                    lblAllegiances = new Label();
                    lblAllegiances.AutoSize = true;
                    lblAllegiances.Location = new System.Drawing.Point(10, 10);
                    lblAllegiances.Text = "EVENTS";
                    page.Controls.Add(lblAllegiances);

                    //perf
                    page = GetTabPage("Performance");
                    lblPerformance = new Label();
                    lblPerformance.AutoSize = true;
                    lblPerformance.Location = new System.Drawing.Point(10, 90);
                    lblPerformance.Text = "PERFORMANCE";
                    page.Controls.Add(lblPerformance);

                    // rolls                    
                    page = GetTabPage("Emigrate rolls");
                    lblEmigrateRoll = new Label();
                    lblEmigrateRoll.AutoSize = true;
                    lblEmigrateRoll.Location = new System.Drawing.Point(10, 90);                   
                    page.Controls.Add(lblEmigrateRoll);

                    // events
                    page = GetTabPage("Events");
                    lblEvents = new Label();
                    lblEvents.AutoSize = true;
                    lblEvents.Location = new System.Drawing.Point(10, 10);
                    lblEvents.Text = "EVENTS";
                    page.Controls.Add(lblEvents);

                    // Jobs
                    page = GetTabPage("Jobs");
                    lblJobs = new Label();
                    lblJobs.AutoSize = true;
                    lblJobs.Location = new System.Drawing.Point(0, 10);
                    lblJobs.Text = "JOBS";
                    Font monoFont = new Font("Consolas", 9, FontStyle.Regular, GraphicsUnit.Point);
                    lblJobs.Font = monoFont;
                    page.Controls.Add(lblJobs);

                    // test events
                    page = GetTabPage("Test events");
                    lblTestEvents = new Label();
                    lblTestEvents.AutoSize = true;
                    lblTestEvents.Location = new System.Drawing.Point(10, 10);
                    lblTestEvents.Text = "TEST EVENTS";
                    page.Controls.Add(lblTestEvents);

                    ddlTestActionSets = new ComboBox();
                    ddlTestActionSets.DropDownStyle = ComboBoxStyle.DropDownList;
                    ddlTestActionSets.Width = page.Width - 40;
                    ddlTestActionSets.DropDownHeight = 600;
                    ddlTestActionSets.Location = new Point(10, 40);
                    ddlTestActionSets.SelectionChangeCommitted += ddlTestEvents_SelectionChangeCommitted;
                    page.Controls.Add(ddlTestActionSets);

                    ddlTestPolledEvents = new ComboBox();
                    ddlTestPolledEvents.DropDownStyle = ComboBoxStyle.DropDownList;
                    ddlTestPolledEvents.Width = page.Width - 40;
                    ddlTestPolledEvents.DropDownHeight = 600;
                    ddlTestPolledEvents.Location = new Point(10, 70);
                    ddlTestPolledEvents.SelectionChangeCommitted += ddlTestPolledEvents_SelectionChangeCommitted;
                    page.Controls.Add(ddlTestPolledEvents);
                    

                    tbTestEventsResult = new TextBox();
                    tbTestEventsResult.Multiline = true;
                    tbTestEventsResult.WordWrap = true;
                    tbTestEventsResult.ScrollBars = ScrollBars.Vertical;
                    tbTestEventsResult.Width = ddlTestActionSets.Width;
                    tbTestEventsResult.Height = 600; // page.Height - 60; // 120;
                  //  lbTestEventsResult.AutoSize = true;
                    tbTestEventsResult.Location = new System.Drawing.Point(10, 90);                 
                    page.Controls.Add(tbTestEventsResult);


                    // events log
                    page = GetTabPage("Events log");

                    lblEventsLog = new Label();
                    lblEventsLog.AutoSize = true;
                    lblEventsLog.Location = new System.Drawing.Point(0, 0);
                    lblEventsLog.Text = "EVENTS \n";
                    page.Controls.Add(lblEventsLog);

                    lbEventsLog = new ListBox();
                    lbEventsLog.Location = new System.Drawing.Point(0, 20);
                    //lbEventsLog.SelectionMode = SelectionMode.One;
                    lbEventsLog.Height = 600; // page.Height - 60; // 120;
                    lbEventsLog.Width = page.Width - 40; // 240;
                    lbEventsLog.KeyUp += lbEventsLog_KeyUp;
                    lbEventsLog.DrawMode = DrawMode.OwnerDrawFixed; // for drawing w. red background
                    lbEventsLog.DrawItem += lbEventsLog_DrawItem;
                    // autoresize not working
                    // we resize manually now.
                   /// lbEventsLog.Dock = DockStyle.Fill; // not working
                   // lbEventsLog.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left); // not working
                    page.Controls.Add(lbEventsLog);


                    // population spawns                    
                    page = GetTabPage("Population spawns");

                    Label lblPop = new Label();
                    lblPop.AutoSize = true;
                    lblPop.Location = new System.Drawing.Point(0, 5);
                    lblPop.Text = "POPULATION SPAWN \n";
                    page.Controls.Add(lblPop);

                    lbPopulationSpawn = new ListBox();
                    lbPopulationSpawn.Width = page.Width - 50;
                    lbPopulationSpawn.Height = 600; // page.Height - 60; // 120;
                    lbPopulationSpawn.KeyUp += lbEventsLog_KeyUp;
                    lbPopulationSpawn.Location = new System.Drawing.Point(10, 20);
                    page.Controls.Add(lbPopulationSpawn);
                   

                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);
                }


               

                void ddlTestEvents_SelectionChangeCommitted(object sender, EventArgs e)
                {
                    ActionSets action = (ActionSets)ddlTestActionSets.SelectedValue;
                    try
                    {
                        action.TestFire(null, null, null, null); // possibly allow params to be selected from other Dropdownlists
                    }
                    catch(AggregateException ex)
                    {
                        string output = "";

                        foreach (var item in ex.InnerExceptions)
                        {
                            OutputException(item, ref output);
                        }

                        
                        tbTestEventsResult.Text = output;                       
                    }
                }

                void ddlTestPolledEvents_SelectionChangeCommitted(object sender, EventArgs e)
                {
                    PolledEventType polledEventType = (PolledEventType)ddlTestPolledEvents.SelectedValue;
                    try
                    {
                       // PolledEvent polled = new PolledEvent(polledEventType);
                        bool wasDestroyed;
                        polledEventType.Fire(null, null, out wasDestroyed);  // possibly allow params to be selected from other Dropdownlists
                                    
                    }
                    catch (AggregateException ex)
                    {
                        string output = "";

                        foreach (var item in ex.InnerExceptions)
                        {
                            OutputException(item, ref output);
                        }


                        tbTestEventsResult.Text = output;
                    }
                }

                private static void OutputException(Exception ex, ref string output)
                {
                    if (ex != null)
                    {
                        output += ex.Message + " " + ex.StackTrace + Environment.NewLine;

                        OutputException(ex.InnerException, ref output);
                    }

                }

                void lbEventsLog_DrawItem(object sender, DrawItemEventArgs e)
                {
                    ListBox lb = (ListBox)sender;

                    string itemText = lb.Items[e.Index].ToString();

                    Graphics g = e.Graphics;

                    e.DrawBackground();
                       
                    if (itemText.Contains("NO EXEC"))
                    {
                         
                        // draw the background color you want                      
                        g.FillRectangle(new SolidBrush(Color.LightSalmon), e.Bounds);

                    }

                    g.DrawString(itemText, e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));

                    e.DrawFocusRectangle();

                }

                void lbEventsLog_KeyUp(object sender, KeyEventArgs e)
                {
                    if (e.Control && e.KeyCode == Keys.C)
                    {
                        CopyListBoxItemsToClipboard((ListBox)sender);
                    }
                }

                /// <summary>
                /// Handles the DrawItem event of the listBox1 control.
                /// </summary>
                /// <param name="sender">The source of the event.</param>
                /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
                private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
                {
                    
                }

                void OptionsForm_Resize(object sender, EventArgs e)
                {
                    ResizeTabPages();

                    ResizeListBox(lbEventsLog);
                    ResizeListBox(lbPopulationSpawn);
                }

                private void ResizeListBox(ListBox listbox)
                {        
                    listbox.Width = listbox.Parent.Width - 40;
                    listbox.Height = listbox.Parent.Height - 160; // this.Height - 100;
                }

                private void ResizeTabPages()
                {
                    foreach (var item in m_tabPages)
                    {
                        item.Width = item.Parent.Width;
                    }
                }

                void OptionsForm_ResizeEnd(object sender, EventArgs e)
                {
                    ResizeTabPages();

                    ResizeListBox(lbEventsLog);
                    ResizeListBox(lbPopulationSpawn);
                }

                /// <summary>
                /// Clean up any resources being used.
                /// </summary>
                /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
               /* protected override void Dispose(bool disposing)
                {
                    if (disposing)
                    {

                    }
                    base.Dispose(disposing);
                }*/


                private void CopyListBoxItemsToClipboard(ListBox listBox)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();

                        if (listBox.Items.Count > 0) // lbEventsLog.Items.Count > 0)
                        {
                            foreach (object row in listBox.Items) // lbEventsLog.Items) //SelectedItems)
                            {
                                sb.Append(row.ToString());
                                sb.AppendLine();
                            }
                            sb.Remove(sb.Length - 1, 1); // Just to avoid copying last empty row
                        }
                        Clipboard.SetData(System.Windows.Forms.DataFormats.Text, sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }


                #endregion

                #region Class Behaviour

                public void SetEntityInfotext(string text)
                {
                    //TabPage page = GetTabPage("Output");
                    lblEntityInfo.Text = text;
                    // TODO: add label - set text.
                }
               /* public void SetThreatLabelText(string text)
                {
                    lblThreat.Text = text;
                }*/
                public void SetEntityGoalsText(string text)
                {
                    lblEntityGoals.Text = text;
                }

                public void SetEventsText(string text)
                {
                    lblEvents.Text = text;
                }

                public void SetJobsText(string text)
                {
                    lblJobs.Text = text;
                }
                

                public void SetPerformanceText(string text)
                {
                    lblPerformance.Text = text;
                }

                public void SetEmigrateRollText(string text)
                {
                    lblEmigrateRoll.Text = text;
                }
                

                public void SetAllegiancesText(string text)
                {
                    lblAllegiances.Text = text;
                }

                public void AppendEventsLogText(string text)
                {
                    //lblEventsLog.Text += text + "\n";
                    lbEventsLog.Items.Add(text);

                }

                public void AppendPopSpawnText(string text)
                {
                    lbPopulationSpawn.Items.Add(text);

                    lbPopulationSpawn.Refresh();

                 /*   lbPopulationSpawn.Hide();
                    lbPopulationSpawn.Show();*/
                }

                public void PopulateTestEvents(List<ActionSets> actions)
                {
                    ddlTestActionSets.DisplayMember = "KeyName";                   
                    actions.Sort(CompareDataByName);
                    ddlTestActionSets.DataSource = actions;
                }

                public void PopulatePolledTestEvents(List<PolledEventType> actions)
                {
                    ddlTestPolledEvents.DisplayMember = "KeyName";                  
                    actions.Sort(CompareDataByName);
                    ddlTestPolledEvents.DataSource = actions;
                }

                public static int CompareDataByName(IGameData a1, IGameData a2)
                {
                    return System.String.Compare(a1.KeyName, a2.KeyName);
                }

               

                public void SetSoundsText(string text)
                {
                    lblSounds.Text = text;
                }


                //for setting buttons
                public void AddOption(string option)
                {
                    string pageName = "Misc";

                    int index = option.IndexOf(".", StringComparison.Ordinal);

                    if (index > 0 && index < option.Length - 1)
                    {
                        pageName = option.Substring(0, index);
                    }

                    TabPage page = GetTabPage(pageName);

                    int pageNewHeight = AddButton(option, page);

                    //// Resize the form to fit additional tab pages and options as they are added
                    //ClientSize = new Size(
                    //    Math.Max(ClientSize.Width, m_tabControl.Width),	// TODO detect when there's enough pages to scroll, and increase width
                    //    Math.Max(ClientSize.Height, pageNewHeight));
                }

                //for setting check boxes
                public void AddOption(string option, bool value)
                {
                    string pageName = "Misc";

                    int index = option.IndexOf(".", StringComparison.Ordinal);

                    if (index > 0 && index < option.Length - 1)
                    {
                        pageName = option.Substring(0, index);
                    }

                    TabPage page = GetTabPage(pageName);

                    int pageNewHeight = AddCheckBox(option, page, value);

                    //// Resize the form to fit additional tab pages and options as they are added
                    //ClientSize = new Size(
                    //    Math.Max(ClientSize.Width, m_tabControl.Width),	// TODO detect when there's enough pages to scroll, and increase width
                    //    Math.Max(ClientSize.Height, pageNewHeight));
                }

                //for setting track bars
                public void AddOption(string option, float value, float min, float max)
                {
                    string pageName = "Misc";

                    int index = option.IndexOf(".", StringComparison.Ordinal);

                    if (index > 0 && index < option.Length - 1)
                    {
                        pageName = option.Substring(0, index);
                    }

                    TabPage page = GetTabPage(pageName);

                    int pageNewHeight = AddTrackBar(option, page, value, min, max);

                    //// Resize the form to fit additional tab pages and options as they are added
                    //ClientSize = new Size(
                    //    Math.Max(ClientSize.Width, m_tabControl.Width),	// TODO detect when there's enough pages to scroll, and increase width
                    //    Math.Max(ClientSize.Height, pageNewHeight));
                }

                //for setting entity types
                public void AddButton(string option, EntityType type)
                {

                    string pageName;

                    int index = option.IndexOf(".", StringComparison.Ordinal);

                    if (index > 0 && index < option.Length - 1)
                    {
                        pageName = option.Substring(0, index);
                    }
                    else
                    {
                        pageName = "Misc";
                    }

                    TabPage page = GetTabPage(pageName);

                    int pageNewHeight = AddButton(option, page);
                    
                    //// Resize the form to fit additional tab pages and options as they are added
                    //ClientSize = new Size(
                    //    Math.Max(ClientSize.Width, m_tabControl.Width),	// TODO detect when there's enough pages to scroll, and increase width
                    //    Math.Max(ClientSize.Height, pageNewHeight));
                }


                public void RemoveOption(string option)
                {
                    string pageName = "Misc";

                    int index = option.IndexOf(".", StringComparison.Ordinal);

                    if (index > 0 && index < option.Length - 1)
                    {
                        pageName = option.Substring(0, index);
                    }

                    TabPage page = GetTabPage(pageName);

                    RemoveCheckBox(option, page);
                    RemoveTrackBar(option, page);

                    //// Resize the form to fit additional tab pages and options as they are added
                    //ClientSize = new Size(
                    //    Math.Max(ClientSize.Width, m_tabControl.Width),	// TODO detect when there's enough pages to scroll, and increase width
                    //    Math.Max(ClientSize.Height, pageNewHeight));
                }

                public void UpdateOption(string option, bool value)
                {
                    CheckBox checkbox;
                    if (checkBoxes.TryGetValue(option, out checkbox))
                    {
                    /*for (int i = 0; i < m_checkBoxes.Count; ++i)
                    {
                        checkbox = m_checkBoxes[i];
                        if (checkbox.Name == option)
                        {*/
                            checkbox.CheckedChanged -= new System.EventHandler(Option_CheckedChanged); // LPE: no event please!

                            checkbox.CheckState = value ? CheckState.Checked : CheckState.Unchecked;

                            checkbox.CheckedChanged += new System.EventHandler(Option_CheckedChanged);

                            return;
                       // }
                    }
                }

                public void UpdateOption(string option, float value)
                {
                    TrackBar trackBar;
                    for (int i = 0; i < m_trackBars.Count; ++i)
                    {
                        trackBar = m_trackBars[i];
                        if (trackBar.Name == option)
                        {
                            trackBar.ValueChanged -= new System.EventHandler(Option_ValueChanged);

                            trackBar.Value = (int)(value * 1000); // (int)value; 

                            trackBar.ValueChanged += new System.EventHandler(Option_ValueChanged);



                            //see whether we have already saved the default value into a label

                            System.Windows.Forms.Control[] defaultLabel = trackBar.Controls.Find("default", true);
                            if (defaultLabel.Length == 0)
                            {
                                Label label = new Label();
                                label.Name = "default";
                                label.Tag = value.ToString();
                                label.Location = new System.Drawing.Point(trackBar.Width - 70, 0);
                                label.ForeColor = Color.White;
                                label.Font = new Font(label.Font, FontStyle.Bold);
                                label.AutoSize = false;
                                label.Size = new System.Drawing.Size(70, 18);
                                trackBar.Controls.Add(label);

                            }

                            defaultLabel = trackBar.Controls.Find("default", true);
                            if (defaultLabel.Length > 0)
                            {
                                Label label = defaultLabel[0] as Label;
                                if (label.Tag.ToString() != value.ToString())
                                {
                                    if (option.Contains("Rotate"))
                                        label.BackColor = Color.Purple;
                                    else if (option.Contains("Translate"))
                                        label.BackColor = Color.DarkOrange;
                                    else
                                        label.BackColor = Color.Red;


                                    label.Text = " " + value;
                                    label.BorderStyle = BorderStyle.Fixed3D;
                                    label.Size = new System.Drawing.Size(70, 18);
                                }
                                else
                                {
                                    label.BorderStyle = BorderStyle.None;
                                    label.Size = new System.Drawing.Size(00, 00);
                                    label.BackColor = Color.Transparent;
                                    label.Text = "";
                                }
                            }


                            //Control[] controls = trackBar.Controls.Find(option, true);
                            //controls[0].Text = option + " = " + value*0.001f;
                            //controls[0].
                            //trackBar.BackColor = Color.HotPink;


                            return;
                        }




                    }
                }

                private TabPage GetTabPage(string pageName)
                {

                    // Yuck
                    for (int i = 0; i < m_tabPages.Count; ++i)
                    {
                        if (m_tabPages[i].Name == pageName)
                        {
                            return m_tabPages[i];
                        }
                    }

                    SuspendLayout();
                    TabPage page = new TabPage();
                    page.SuspendLayout();

                    page.Name = pageName;
                    page.Text = pageName;
                    page.Padding = new Padding(CheckBoxPadding);
                    page.Size = new System.Drawing.Size(FormSize - CheckBoxPosition, CheckBoxPosition); //start short, and autosize as controls are added
                    page.UseVisualStyleBackColor = true;
                    page.AutoScroll = true;
                    AutoScroll = true;
                    page.AutoSize = false;
                    page.Size = new System.Drawing.Size(FormSize - CheckBoxPosition, 2000); //start short, and 
                    
                    m_tabPages.Add(page);
                    m_tabControl.SuspendLayout();
                    m_tabControl.Controls.Add(page);
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    return page;
                }

               

                private int RemoveCheckBox(string option, TabPage page)
                {
                    CheckBox checkbox;
                    if (!checkBoxes.TryGetValue(option, out checkbox))
                    {
                        return 0;
                    }
                    /*
                    CheckBox checkbox = m_checkBoxes.FindLast(c => c.Name == option);
                    if (checkbox == null)
                        return 0; //nevermind
                    */
                    SuspendLayout();
                    m_tabControl.SuspendLayout();
                    page.SuspendLayout();

                  //  m_checkBoxes.Remove(checkbox); //.RemoveAll(c => c.Name == option);
                    checkBoxes.Remove(option);
                    page.Controls.Remove(checkbox);

                    page.ResumeLayout(false);
                    page.PerformLayout();
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    return checkbox.Location.Y + checkbox.Size.Height + 2 * CheckBoxPadding + page.Bounds.Top;
                }

                private int RemoveTrackBar(string option, TabPage page)
                {
                    TrackBar trackBar = m_trackBars.FindLast(c => c.Name == option);
                    if (trackBar == null)
                        return 0; //nevermind

                    SuspendLayout();
                    m_tabControl.SuspendLayout();
                    page.SuspendLayout();

                    m_trackBars.Remove(trackBar); //.RemoveAll(c => c.Name == option);
                    page.Controls.Remove(trackBar);

                    page.ResumeLayout(false);
                    page.PerformLayout();
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    return trackBar.Location.Y + trackBar.Size.Height + 2 * CheckBoxPadding + page.Bounds.Top;
                }



                private int AddTrackBar(string option, TabPage page, float value, float min, float max)
                {
                    // Not very pretty
                    for (int i = 0; i < m_trackBars.Count; ++i)
                    {
                        if (m_trackBars[i].Name == option)
                        {
                            m_trackBars[i].Value = (int)value * 1000;
                            m_trackBars[i].Minimum = (int)min;
                            m_trackBars[i].Maximum = (int)max;
                            return 0;
                        }
                    }

                    SuspendLayout();
                    m_tabControl.SuspendLayout();
                    page.SuspendLayout();


                    TrackBar trackBar = new TrackBar();

                    int yPos = CheckBoxPosition;
                    foreach (System.Windows.Forms.Control c in page.Controls)
                    {
                        yPos = Math.Max(yPos, c.Bottom);
                    }

                    int idx = option.LastIndexOf('.');
                    string text = option;
                    if (idx >= 0)
                        text = text.Substring(idx + 1);

                    bool addExtraLine = false;
                    if (text.Length > 42)
                    {
                        // make room for an extra line of text inside the trackbar.
                        addExtraLine = true;
                    }


                    /*  Padding padding = trackBar.Padding; // Margin;
                      padding.Top = padding.Top + 25;
                      trackBar.Padding = padding;*/
                    trackBar.Location = new System.Drawing.Point(CheckBoxPosition, yPos + CheckBoxPadding * 4);
                    if (addExtraLine)
                    {
                        trackBar.AutoSize = false; // true;
                        trackBar.Size = new System.Drawing.Size(FormSize - 4 * CheckBoxPosition, 62); //20);
                    }
                    else
                    {
                        trackBar.AutoSize = true; // true;
                        trackBar.Size = new System.Drawing.Size(FormSize - 4 * CheckBoxPosition, 20);
                    }
                    // trackBar.AutoSize = false; // true;
                    trackBar.TabIndex = page.Controls.Count;
                    trackBar.Minimum = (int)(min * 1000);
                    trackBar.Maximum = (int)(max * 1000);
                    trackBar.Value = (int)(value * 1000);
                    trackBar.Scroll += new System.EventHandler(Option_ValueChanged);
                    trackBar.Name = option;
                    trackBar.TickStyle = TickStyle.Both;
                    trackBar.BackColor = ChooseColorForPageControl(page);
                    if (trackBar.Maximum - trackBar.Minimum > 5000000)
                        trackBar.TickFrequency = 1000000;
                    else if (trackBar.Maximum - trackBar.Minimum > 500000)
                        trackBar.TickFrequency = 100000;
                    else if (trackBar.Maximum - trackBar.Minimum > 50000)
                        trackBar.TickFrequency = 10000;
                    else if (trackBar.Maximum - trackBar.Minimum > 5000)
                        trackBar.TickFrequency = 1000;
                    else if (trackBar.Maximum - trackBar.Minimum > 500)
                        trackBar.TickFrequency = 100;
                    else if (trackBar.Maximum - trackBar.Minimum > 50)
                        trackBar.TickFrequency = 10;

                    if (option.Contains("Rotate"))
                        trackBar.BackColor = Color.Thistle;
                    else if (option.Contains("Translate"))
                        trackBar.BackColor = Color.Wheat;



                    Label label = new Label();
                    label.Text = text;
                    label.Name = option;
                    label.Location = new System.Drawing.Point(CheckBoxPadding, CheckBoxPadding - 1);
                    label.Width = 330;
                    label.AutoEllipsis = true;
                    label.Height = 14;
                    // label.
                    //label.AutoSize = true;
                    label.BackColor = Color.Transparent;
                    trackBar.Controls.Add(label);

                    Label lblCodedTo = new Label();
                    lblCodedTo.Text = "Coded to: " + value;
                    lblCodedTo.Name = option;
                    lblCodedTo.AutoSize = true;
                    lblCodedTo.BackColor = Color.LightBlue;
                    trackBar.Controls.Add(lblCodedTo);
                    if (addExtraLine)
                    {
                        lblCodedTo.Location = new System.Drawing.Point(CheckBoxPadding, 49);
                    }
                    else
                    {
                        lblCodedTo.Location = new System.Drawing.Point(224, label.Location.Y);
                    }


                    int trackLabelYpos = 33; // (trackBar.Height - CheckBoxPosition * 2) + 3;

                    Label minLabel = new Label();
                    minLabel.Text = min.ToString();
                    minLabel.Name = "min";
                    minLabel.Location = new System.Drawing.Point(CheckBoxPosition, trackLabelYpos);
                    minLabel.AutoSize = true;
                    trackBar.Controls.Add(minLabel);

                    Label maxLabel = new Label();
                    maxLabel.Text = max.ToString();
                    maxLabel.Name = "min";
                    maxLabel.Location = new System.Drawing.Point(trackBar.Size.Width - CheckBoxPosition * 3, trackLabelYpos);
                    maxLabel.AutoSize = true;
                    trackBar.Controls.Add(maxLabel);

                    Label midLabel = new Label();
                    midLabel.Text = ((max + min) * 0.5f).ToString();
                    midLabel.Name = "mid";
                    midLabel.Location = new System.Drawing.Point((maxLabel.Location.X + minLabel.Location.X) >> 1, trackLabelYpos);
                    midLabel.AutoSize = true;
                    trackBar.Controls.Add(midLabel);



                    m_trackBars.Add(trackBar);
                    page.Controls.Add(trackBar);
                    page.ResumeLayout(false);
                    page.PerformLayout();
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    return trackBar.Bottom;

                }

                private Color ChooseColorForPageControl(TabPage page)
                {
                    return page.Controls.Count % 2 == 0 ? Color.Gainsboro : page.BackColor;
                }


                private int AddCheckBox(string option, TabPage page, bool value)
                {
                     CheckBox checkbox;
                     if (checkBoxes.TryGetValue(option, out checkbox))
                     {
                         checkbox.Checked = value;
                         return 0;
                     }
                    
                   /* for (int i = 0; i < m_checkBoxes.Count; ++i)
                    {
                        if (m_checkBoxes[i].Name == option)
                        {
                            m_checkBoxes[i].Checked = value;
                            return 0;
                        }
                    }*/

                    SuspendLayout();
                    m_tabControl.SuspendLayout();
                    page.SuspendLayout();

                    checkbox = new CheckBox();
                    checkbox.AutoSize = true;

                    int yPos = CheckBoxPosition;
                    foreach (System.Windows.Forms.Control c in page.Controls)
                    {
                        yPos = Math.Max(yPos, c.Bottom);
                    }

                    checkbox.Location = new System.Drawing.Point(CheckBoxPosition, yPos + 2 * CheckBoxPadding);

                    checkbox.Name = option;
                    int idx = option.LastIndexOf('.');
                    string text = option;
                    if (idx >= 0)
                        text = text.Substring(idx + 1);

                    checkbox.Text = text;
                    checkbox.Size = new System.Drawing.Size(FormSize - 4 * CheckBoxPosition, CheckBoxYHeight - 2 * CheckBoxPadding);
                    checkbox.TabIndex = page.Controls.Count;
                    checkbox.UseVisualStyleBackColor = true;
                    checkbox.Checked = value;
                    checkbox.CheckedChanged += new System.EventHandler(Option_CheckedChanged);
                    checkbox.BackColor = ChooseColorForPageControl(page);

                    checkBoxes.Add(option, checkbox);
                    page.Controls.Add(checkbox);
                    page.ResumeLayout(false);
                    page.PerformLayout();
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    return checkbox.Bottom;
                }

                public string ShownTab()
                {
                    return m_tabControl.SelectedTab.Name;

                }

                private int AddButton(string option, TabPage page)
                {
                    SuspendLayout();
                    m_tabControl.SuspendLayout();
                    page.SuspendLayout();

                    Button button = new Button();
                    button.AutoSize = true;

                    int yPos = CheckBoxPosition;
                    foreach (System.Windows.Forms.Control c in page.Controls)
                    {
                        yPos = Math.Max(yPos, c.Bottom);
                    }

                    button.Location = new System.Drawing.Point(CheckBoxPosition, yPos + 2 * CheckBoxPadding);

                    button.Name = option;
                    int idx = option.LastIndexOf('.');
                    string text = option;
                    if (idx >= 0)
                        text = text.Substring(idx + 1);

                    button.Text = text;
                    button.Size = new System.Drawing.Size(FormSize / 2, ButtonHeight + CheckBoxPadding);
                    button.TabIndex = page.Controls.Count;
                    button.UseVisualStyleBackColor = true;
                    button.Anchor = AnchorStyles.Top;

                    button.Click += new System.EventHandler(Option_ButtonClicked);
                    button.BackColor = ChooseColorForPageControl(page);

                    page.Controls.Add(button);
                    page.ResumeLayout(false);
                    page.PerformLayout();
                    m_tabControl.ResumeLayout(false);
                    ResumeLayout(false);

                    if (text.EndsWith("person"))
                    {
                        int bottomToReturn = button.Bottom;
                        button.Font = new Font(button.Font, FontStyle.Bold);
                        button.BackColor = Color.LightGreen;
                        button.Location = new System.Drawing.Point(FormSize / 2 + 4 * CheckBoxPadding, CheckBoxPosition + 2 * CheckBoxPadding);
                        button.Size = new System.Drawing.Size(FormSize / 4, ButtonHeight + CheckBoxPadding);
                        return bottomToReturn;
                    }

                    return button.Bottom;
                }


                #endregion

                #region Event Delegates

                private void Option_CheckedChanged(object sender, EventArgs e)
                {
                    CheckBox checkbox = sender as CheckBox;
                    if (checkbox != null)
                        SetOption(checkbox.Name, checkbox.Checked, true);
                }

                private void Option_ButtonClicked(object sender, EventArgs e)
                {
                    Button button = sender as Button;
                    if (button != null)
                        SetOption(button.Name, DebugButton.MakeAButton, true);
                }



                private void Option_ValueChanged(object sender, EventArgs e)
                {
                    TrackBar trackBar = sender as TrackBar;
                    if (trackBar != null)
                    {
                        SetOption(trackBar.Name, 0.001f * (float)trackBar.Value, (float)trackBar.Minimum, (float)trackBar.Maximum, true);
                        string optionName = trackBar.Name;

                        //if (s_callbacks.ContainsKey(optionName))
                        //{
                        //    int val = trackBar.Value;
                        //    s_callbacks[optionName](optionName, null, val * .001f);
                        //}
                    }

                }

                private void Form_ClientSizeChanged(object sender, EventArgs e)
                {
                    OptionsForm form = (OptionsForm)sender;

                    // Force the tab control to completely fill the client window, however large it gets
                    form.m_tabControl.ClientSize = new Size(form.ClientSize.Width, 8000);
                }




                #endregion

                #region Variables

                private TabControl m_tabControl = new TabControl();
                private List<TabPage> m_tabPages = new List<TabPage>();
               // private List<CheckBox> m_checkBoxes = new List<CheckBox>();
                private Dictionary<string, CheckBox> checkBoxes = new Dictionary<string,CheckBox>();
                private List<TrackBar> m_trackBars = new List<TrackBar>();

                #endregion
            }
#endif
            #endregion
        }
    }
}

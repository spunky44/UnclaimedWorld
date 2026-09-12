#region File Description
//-----------------------------------------------------------------------------
// File:      Label.cs
// Namespace: WindowSystem
// Author:    Aaron MacDougall
//-----------------------------------------------------------------------------
#endregion

#region License
//-----------------------------------------------------------------------------
// Copyright (c) 2007, Aaron MacDougall
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Aaron MacDougall nor the names of its contributors may
//   be used to endorse or promote products derived from this software without
//   specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using InputEventSystem;
using System.Collections.Generic;
#endregion

namespace WindowSystem
{

    public class Label : UIComponent, IHasText
    {
        #region Default Properties
        private static string defaultFont = "Content/Fonts/DefaultFont";
        private static Color defaultColor = Color.Black;

       

        public enum LabelType
        {
            None,
            PlainPanelNormal, PlainPanelMedium, CRTBigGlow, CRTNormal, CRTSmall, LCDError, /*LCDHeader,*/
            LCDBigHeaderBanner, // big dark banner, yellow text
            LCDNormal, LCDNormalLight, LCDNormalDark, LCDWhite, LCDDate, LCDWeather, LCDCheckbox, LCDCheckboxWhite, LCDRadioBanner, LCDRadio, LCDComboBoxItem,
            LCDSmallHeadingBanner, // dark banner
            LCDSmallHeadingBannerLight, // light banner
            RosterTitle,

            HUDWindow, HUDWindowHeader,
            EntityTypeTooltip, EntityTypeTooltipHeader, EntityTypeTooltipSubHeading, 
            // colored banner
            LCDHeadingBlue, LCDHeadingGreen, LCDHeadingRed, LCDHeadingBrown, LCDHeadingGrey, LCDHeadingSteelGrey,
            LCDBarfillBlue,
            LCDRadioBannerTinted
        }

        LabelType type = LabelType.None;

        private Bar background;

        private Bar hoverBackground;


        /// <summary>
        /// use this to set the link color in its normal state! Do not set Color!
        /// 
        /// same as in HyperLink.
        /// 
        /// ModulateColor is needed for imagebutton/icon hover colors to work?
        /// </summary>
        public Color NormalColor
        {
            get
            {
                return normalColor ?? Color;
            }
            set
            {
               
                if (normalColor != value)
                {
                    // only set the label color now if it is in 'normal' state:                   
                    if (Color == normalColor || Color == defaultColor)
                    {
                        Color? oldNormalColor = normalColor;
                        normalColor = value; // set to prevent infinite recursion..

                        this.color = value;
                        Redraw();
 
                        // a label can have children! set their color too:
                        // we iterate the children here instead of in Color, because we need to check a condition!                       
                        foreach (var childControl in Controls)
                        {
                            Label childLabel = childControl as Label;
                            if (childLabel != null)
                            {
                                if (childLabel.normalColor == oldNormalColor) // NEW: don't set color on child labels which differ in their NormalColor!
                                {
                                    childLabel.Color = color;

                                    // set normalcolor too???
                                }
                            }
                            else
                            {
                                Hyperlink hl = childControl as Hyperlink;
                                if (hl != null)
                                {
                                    /* if (hl.Color == hl.NormalColor)
                                     {
                                         hl.Color = color;
                                     }*/

                                    hl.NormalColor = color;
                                }
                            }
                        }

                    }

                    normalColor = value;
                }
            }
        }

        public bool EnableCursor = false;

       
        private Color? normalColor;
        private Color labelHoverColor = new Color(125, 125, 125, 255); //Default color used when hovering over labels with a tooltip


        // copied in ImageButton...
        private Color? DisabledColor;
        private Color? enabledColor = null;


        /// <summary>
        /// these are used to position the text in relation to the background bar
        /// </summary>
        private int leftPadding = 0;
        private int rightPadding = 0;
        private int topPadding = 0;

        /// <summary>
        /// Sets the default font.
        /// </summary>
        /// <value>Must be a non-empty string.</value>
        public static string DefaultFont
        {
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Length > 0);
                defaultFont = value;
            }
        }

        /// <summary>
        /// Sets the default text colour.
        /// </summary>
        public static Color DefaultColor
        {
            set { defaultColor = value; }
        }
        #endregion

        #region Fields
        private string text;

     //   private string plainText;
        private string /*char*/ cursor;

        /// <summary>
        /// cycles true/false to animate a blinking cursor
        /// </summary>
        private bool isCursorShown;
        private SpriteFont font;
        private Color color;     
       // private Texture2D renderedTexture;
        private bool isRedrawRequired;
        private Viewport viewPort;

        private static RasterizerState scissorTestRasterizerState;
      //  private static RasterizerState defaultRasterizerState;

        public enum AnimationMode {None, Character, Line}
        private AnimationMode animationMode = AnimationMode.None;
        public AnimationMode AnimateOnCRTScreen
        {
            get {return animationMode;}
            set 
            {
                if (animationMode == AnimationMode.Line)
                {
                    throw new Exception("Must set line no as well!");
                }
                else animationMode = value;
               
            }
        }

        public int? AnimateOnCRTScreenLineNo = null; // false;
        
        //62FBBB
        private static Color digitalGreen = new Color(0x62, 0xFB, 0xBB);
        // BCD8F2
        public static Color CRTLightBlue = new Color(188, 216, 242); //new Color(224, 240, 255); hex: BCD8F2

        #endregion

        public void SetLineAnimationMode(int lineNo)
        {
            animationMode = AnimationMode.Line;
            AnimateOnCRTScreenLineNo = lineNo;
        }

        /// <summary>
        /// color can be a custom constant too
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToLabel(string labelText, string color)
        {
            // §C#00FF00¤True§
            StringBuilder text = new StringBuilder();
            text.Append("§L"); // means 'Label'
            text.Append(color); // .ToString());
            text.Append("¤");

            text.Append(labelText);

            text.Append("§");

            return text.ToString();
        }

        public static string ToLabel(string labelText, Color color)
        {
            return ToLabel(labelText, color.ToHex());           
        }

        #region Properties

        /// <summary>
        /// Get/Set the label text.
        /// </summary>
        /// <value>Must not be null.</value>
        public string Text
        {
            get { return text; }
            set
            {
                            
                // make sure that no child labels or hyperlinks exist:
                if (background == null && hoverBackground == null)
                {
                    Controls.Clear();
                }
                else
                {
                    // don't remove the background graphics
                    Controls.RemoveAll(c => !(c is Bar));
                }

                Debug.Assert(value != null);
                
                // extract 'links' from text, build labels and hyperlinks, add as children
                // "|E0¤Joe| hits |E12¤Jack Houlahan| with a board."

                // Font must have been set before the controls can be placed correctly...
                if (value.Contains("§"))
                {
                    StringBuilder plainTextBuilder = new StringBuilder();
                    Label label;
                 
                   
                    int currentXPos = 0;

                   
                    string[] tokens = value.Split('§');

                    for (int i = 0; i < tokens.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            if (tokens[i] != "")
                            {
                                label = new Label(guiManager);
                                // NEW:
                                label.Init(type);

                                label.RenderType = base.RenderType; //VERY important!
                                this.Add(label);
                                if (normalColor.HasValue)
                                {
                                    label.NormalColor = normalColor.Value;
                                }
                                //label.normalColor = normalColor; // NormalColor; // Color;
                                label.Text = tokens[i];
                                label.Width = label.TextWidth;
                                label.Height = label.TextHeight;
                              
                                label.X = currentXPos;
                              //  label.DebugTag = "timeLabel";

                                // we want the contained controls to resize with the parent!
                              //  label.HeightResize += new ResizeHandler(ContainedControlHeightResize);

                                currentXPos += label.TextWidth;

                                plainTextBuilder.Append(tokens[i]);
                                //text += tokens[i];
                            }
                        }
                        else
                        {
                            string token = tokens[i];

                            if (token.StartsWith("E") || token.StartsWith("C") || token.StartsWith("P"))
                            {
                                ParseHyperlink(plainTextBuilder, ref currentXPos, token);
                            }
                            else if (token.StartsWith("L"))
                            {
                                ParseLabel(plainTextBuilder, ref currentXPos, token);
                            }
                            else if (token.StartsWith("I"))
                            {
                                ParseIcon(plainTextBuilder, ref currentXPos, token);
                            }
                        }
                    }
                    text = plainTextBuilder.ToString();
                }
                else
                {   // no links...
                    this.text = value;  
                }

                // NEW!
                FitToText();

                Redraw();
                this.isRedrawRequired = true;
            }
        }

        private void ParseLabel(StringBuilder plainTextBuilder, ref int currentXPos, string token)
        {
            string[] labelTokens;
            labelTokens = token.Split('¤');

            string text = labelTokens[1];
            CreateNestedLabel(ref currentXPos, text, labelTokens[0], null);
            plainTextBuilder.Append(text);           
        }

        private void ParseHyperlink(StringBuilder plainTextBuilder, ref int currentXPos, string token)
        {
            Hyperlink hyperlink;
            string[] linkTokens;

            linkTokens = token.Split('¤');

            uint? entityID;
            Point? tilePos;
            uint? containerID;

            if (LinkIsValid(linkTokens[0], out entityID, out tilePos, out containerID))
            {
                hyperlink = new Hyperlink(guiManager) { Text = linkTokens[1], X = currentXPos };
                hyperlink.RenderType = base.RenderType; // VERY important!
                hyperlink.DebugTag = "hyperlink";
                hyperlink.NormalColor = Color; // hm. we have no way to set hover/pressed color...

                if (containerID.HasValue)
                {
                    hyperlink.TargetResourceContainerID = containerID.Value;
                }
                else if (entityID.HasValue)
                {
                    hyperlink.TargetEntityID = entityID.Value;
                }
                else
                {
                    hyperlink.TargetMapPosition = tilePos.Value;
                }

                this.Add(hyperlink);
                // we want the contained controls to resize with the parent!
                //   hyperlink.HeightResize += new ResizeHandler(ContainedControlHeightResize); 
                //hyperlink.Init(TextButton.TextButtonType.Hyperlink);
                currentXPos += hyperlink.Width;
            }
            else
            {
                // link is dead... create a label:
                CreateNestedLabel(ref currentXPos, linkTokens[1], null, normalColor);
            }
            //text += linkTokens[1];
            plainTextBuilder.Append(linkTokens[1]); //tokens[1]);
            
        }

        private void ParseIcon(StringBuilder plainTextBuilder, ref int currentXPos, string token)
        {
            string[] iconTokens;
            iconTokens = token.Split('¤');

            string colorToken = iconTokens[0];
            string sprite = iconTokens[1];
            
            Icon icon;
            icon = new Icon(guiManager);
            icon.ScaleImageToSizeOfControl = false;
            icon.X = currentXPos;          
            icon.RenderType = base.RenderType; //needed?

            Color? normalColor = null;
            if (colorToken != null)
            {
                string colorString = colorToken.Substring(1, colorToken.Length - 1);
                normalColor = GetColorFromToken(colorString);

               // icon.NormalColor = normalColorFromString;
                // label.DebugTag = "GreenLabel";
            }

            icon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(sprite), normalColor, normalColor);
            icon.ResizeControlToFitImage();

            this.Add(icon);

            // we want the contained controls to resize with the parent!
            //  this.HeightResize += new ResizeHandler(ContainedControlHeightResize);
            currentXPos += icon.Width;
           
           // plainTextBuilder.Append(text);
        }

        private void CreateNestedLabel(ref int currentXPos, string text, string colorToken, Color? normalColor) // //string[] linkTokens, Color? color = null)
        {
            Label label;
            label = new Label(guiManager);
            label.Text = text;
            label.X = currentXPos;
            label.Init(type);
            label.RenderType = base.RenderType; //VERY important!
          
            if (colorToken != null)
            {
                string colorString = colorToken.Substring(1, colorToken.Length - 1);
                Color normalColorFromString = GetColorFromToken(colorString);

                label.NormalColor = normalColorFromString;
               // label.DebugTag = "GreenLabel";
            }
            else
            {
                label.normalColor = normalColor;
            }

            this.Add(label);

            // we want the contained controls to resize with the parent!
            //  this.HeightResize += new ResizeHandler(ContainedControlHeightResize);
            currentXPos += label.TextWidth;
        }

        private Color GetColorFromToken(string colorToken)
        {
            Color customColor;
            if (guiManager.CustomColors.TryGetValue(colorToken, out customColor))
            {
                return customColor;
            }
            else
            {
                Color normalColorFromString = Util.ColorFromHex(colorToken) ?? UIComponent.LCDNormal;

                return normalColorFromString;
            }
        }

        void ContainedControlHeightResize(UIComponent sender)
        {
            // we want the contained controls to resize with the parent!
            if (Controls.Count > 0)
            {
                foreach (UIComponent control in Controls)
                {
                    control.Height = sender.Height;
                }
            }
            
        }


        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;

                    if (base.Enabled)
                    {
                        if (enabledColor.HasValue)
                        {
                            Color = enabledColor.Value;
                        }


                        if (background != null)
                        {
                            background.CurrentSkinState = SkinState.Normal;
                           
                        }

                    }
                    else
                    {
                        if (DisabledColor.HasValue)
                        {
                            Color = DisabledColor.Value;
                        }

                        if (background != null)
                        {
                            background.CurrentSkinState = SkinState.Disabled;
                          
                        }
                    }
                }
            }
        }

        /// <summary>
        /// does a resize as well.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            Text = text;
            FitToText();
        }

        public static int GetParsedLineWidth(string textWithMetaLinks, GUIManager gui, SpriteFont font)
        {
            int widthOfSprites = 0;

            List<string> iconSprites;
            string plainText = GetPlainText(textWithMetaLinks, true, out iconSprites);
            int textWidth = GetTextWidth(plainText, font);

            if (iconSprites != null)
            {
                foreach (var item in iconSprites)
                {
                    Rectangle? spriteRect;
                    if (gui.GUISpriteSheet.TryGetSourceRectangle(item, out spriteRect))
                    {
                        widthOfSprites += spriteRect.Value.Width;
                    }
                }
            }

            return widthOfSprites + textWidth;

        }

     

        public static string GetPlainText(string textWithMetaLinks)
        {
            List<string> iconSprites;
            return GetPlainText(textWithMetaLinks, false, out iconSprites);
        }

        public static string GetPlainText(string textWithMetaLinks, bool collectIconSprites, out List<string> iconSprites)
        {
            iconSprites = null;
            if (textWithMetaLinks.Contains("§"))
            {
                string[] linkTokens;
                StringBuilder plainTextBuilder = new StringBuilder();

                string[] tokens = textWithMetaLinks.Split('§');

                for (int i = 0; i < tokens.Length; i++)
                {
                    string token = tokens[i];
                    if (i % 2 == 0)
                    {
                        plainTextBuilder.Append(token);
                    }
                    else if (token.StartsWith("I")) // don't extract the icon sprite, only label and hyperlink text
                    {
                        if (collectIconSprites)
                        {
                            linkTokens = token.Split('¤');
                            Util.AddToList(ref iconSprites, linkTokens[1]); // output the sprite so it can be measured if needed
                        }
                    }
                    else //if (!token.StartsWith("I")) 
                    {
                        linkTokens = token.Split('¤');
                        plainTextBuilder.Append(linkTokens[1]);
                    }
                }

                return plainTextBuilder.ToString();

            }
            else return textWithMetaLinks;
        }

        /// <summary>
        /// can return one of two possible link targets - and entity or a map position
        /// </summary>
        /// <param name="link"></param>
        /// <param name="entityID"></param>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        private bool LinkIsValid(string link, out uint? entityID, out Point? tilePos, out uint? containerID) //out IHyperlinkTarget linkEntity)
        {
           // linkEntity = null;
            entityID = null;
            containerID = null;
            tilePos = null;
          
            if (link.StartsWith("C"))
            {
                uint id;
                if (uint.TryParse(link.Substring(1, link.Length - 1), out id)) // id can be Invalid
                {
                    containerID = id;
                    return true;

                    //return guiManager.EntitiesByID.TryGetValue(id, out linkEntity); // .ContainsKey(id);
                }
            }
            else if (link.StartsWith("E"))
            {
                uint id;
                if (uint.TryParse(link.Substring(1, link.Length - 1), out id)) // id can be Invalid
                {
                    entityID = id;
                    return true;

                    //return guiManager.EntitiesByID.TryGetValue(id, out linkEntity); // .ContainsKey(id);
                }
                else return false;
            }
            else if (link.StartsWith("P"))
            {
                link = link.Replace("P", "");
                // P3,14 (=tile pos 3,14 )
                string[] tileCoords = link.Split(',');

                tilePos = new Point(int.Parse(tileCoords[0]), int.Parse(tileCoords[1]));

            }

            return true;
        }

        /// <summary>
        /// Get/Set whether the cursor should be shown.
        /// </summary>
        public bool IsCursorShown
        {
            get { return isCursorShown; }
            set
            {
                if (value != this.isCursorShown)
                {
                    this.isCursorShown = value;
                    Redraw();
                    this.isRedrawRequired = true;
                }
            }
        }

        /// <summary>
        /// Sets the text font.
        /// </summary>      
        public SpriteFont Font
        {
            set
            {
                this.font = value; 

                // check for embedded links:
                if (Controls.Count > 0)
                {
                    int currentXPos = 0;
                    int totalWidth = 0; // NEW!

                    Hyperlink hyperLink;
                    Label label;

                    bool hasTextChildren = false;

                    foreach (UIComponent child in Controls)
                    {
                        label = child as Label;
                        if (label != null)
                        {
                            label.Font = value;
                            label.X = currentXPos;
                            label.Width = label.TextWidth; 
                            currentXPos += label.TextWidth;
                            totalWidth += label.Width; // NEW

                            hasTextChildren = true;
                        }
                        else
                        {
                            hyperLink = child as Hyperlink;
                            if (hyperLink != null)
                            {
                                hyperLink.Font = value;
                                hyperLink.X = currentXPos;
                                currentXPos += hyperLink.Width; // hyperLink.TextWidth;
                                totalWidth += hyperLink.Width; // NEW

                                hasTextChildren = true;
                            }
                            else
                            {
                                Icon icon = child as Icon;
                                if (icon != null)
                                {
                                    icon.X = currentXPos;
                                    currentXPos += icon.Width; 
                                    totalWidth += icon.Width;

                                    hasTextChildren = true;
                                }
                            }
                        }

                    }

                    if (hasTextChildren)
                    {
                        this.Width = totalWidth; // NEW
                    }

                    ResetHeight();
                }
                else
                {
                    // NEW!
                    this.Width = TextWidth;

                    cursorWidth = GetTextWidth(cursor);

                    ResetHeight();
                }

                Redraw();
                this.isRedrawRequired = true;
            }
        }


        private void ResetHeight()
        {
            if (background == null)
            {
                this.Height = TextHeight;
            }
            else
            {
                this.Height = background.Height;
            }
        }


        public Color BackColor
        {           
            set
            {
                if (background != null)
                {
                    background.SetSkinLocation(SkinState.Normal,null, value, value);                  
                }
            }
        }

        /// <summary>
        /// Get/Set the text colour.
        /// </summary>
        public Color Color
        {
            get 
            { 

                return this.color; 
            }

            set
            {
               
                this.color = value;
                Redraw();
                this.isRedrawRequired = true;

                // a label can have children! set their color too:
                foreach (var childControl in Controls)
                {
                    Label childLabel = childControl as Label;
                    if (childLabel != null)
                    {
                        if (childLabel.normalColor == this.normalColor) // NEW: don't set color on child labels which differ in their NormalColor!
                        {
                            childLabel.Color = color;
                        }
                    }
                    else
                    {
                        Hyperlink hl = childControl as Hyperlink;
                        if (hl != null)
                        {
                           /* if (hl.Color == hl.NormalColor)
                            {
                                hl.Color = color;
                            }*/

                            hl.NormalColor = color;
                        }
                    }
                }
            
            }
        }

        /// <summary>
        /// Gets the font height.
        /// </summary>
        public int TextHeight
        {
            get
            {
                int result = 0;
                if (this.font != null)
                    result = this.font.LineSpacing;
                return result;
            }
        }

        /// <summary>
        /// Gets the width of current text.
        /// </summary>
        public int TextWidth
        {
            get
            {
                return GetTextWidth(Text);
                /*
                int result = 0;
                if (this.font != null)
                    result = (int)(this.font.MeasureString(Text).X + 1.0f);
                return result;*/
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public Label(GUIManager guiManager)
            : base(guiManager)
        {
            this.text = string.Empty;
            this.cursor = "|"; // '_';
            this.isCursorShown = false;

            #region Properties
            CanHaveFocus = false;
            #endregion

            #region Set Default Properties
            Color = defaultColor;
            #endregion

            // we want the contained controls to resize with the parent!
            this.HeightResize += new ResizeHandler(ContainedControlHeightResize);

            this.viewPort = new Viewport();
        }

        static Label()
        {
            scissorTestRasterizerState = new RasterizerState();
            scissorTestRasterizerState.ScissorTestEnable = true;

            
        }

        #endregion


        public int GetTextWidth(string text)
        {
            int result = 0;
            if (this.font != null)
            {
                result = GetTextWidth(text, font);
               // result = (int)(this.font.MeasureString(text).X + 1.0f);
            }

            return result;
        }

        /*  public int GetTextWidth(string text)
       {
           return GetTextWidth(text, font);
       }*/

        /// <summary>
        /// Gets the width of a piece of text.
        /// </summary>
        /// <param name="txt">The text to be measured.</param>
        /// <returns>The number of pixels the text will occupy on screen.</returns>
        private static int GetTextWidth(string txt, SpriteFont font)
        {
            int a = (int)font.MeasureString(txt).X;
            return a;
        }

        public void TintLabelBackground(Color color)
        {          
           background.SetSkinLocation(SkinState.Normal,null, color, color);          
        }


        const int lcdCornerHeadingPadding = 6;
        const int lcdMediumHeadingTopPadding = 1;

        public static Microsoft.Xna.Framework.Color LCDErrorColor = Color.Red;
       

        /// <summary>
        /// Set Text before calling this, then it will be sized correctly.
        /// </summary>
        /// <param name="type"></param>
        public void Init(LabelType type)
        {
            if (this.type == type)
            {
                return;
            }

            this.type = type;
                       

            switch (type)
            {
                case LabelType.LCDCheckbox:
                    {
                        InitNormalBackground("basic_header_small_square", 8);
                        ApplyTextFormat(this, type);

                        Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_header_small_square_disabled");
                        background.SetSkinLocation(SkinState.Disabled, rect);

                        InitDisabledColor(TextButton.lcdDisabledColor, Color);

                        leftPadding = 2;
                        rightPadding = 6;

                        break;
                    }
                case LabelType.LCDCheckboxWhite:
                    {
                        InitNormalBackground("basic_header_small_square_white", 8);
                        ApplyTextFormat(this, type);

                        Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_header_small_square_disabled");
                        background.SetSkinLocation(SkinState.Disabled, rect);

                        InitDisabledColor(TextButton.lcdDisabledColor, Color);

                        leftPadding = 2;
                        rightPadding = 6;

                        break;
                    }
                case LabelType.LCDRadioBanner:
                    InitNormalBackground("basic_header_small_round", 8);
                    ApplyTextFormat(this, type);

                    InitDisabledColor(TextButton.lcdDisabledColor, Color);

                    leftPadding = 8;
                    rightPadding = 6;

                    break;
                case LabelType.LCDRadioBannerTinted:
                    InitNormalBackground("basic_header_small_round_white", 8);
                    ApplyTextFormat(this, type);

                    InitDisabledColor(TextButton.lcdDisabledColor, Color);

                    leftPadding = 8;
                    rightPadding = 6;
                   // Height = background.Height;

                    break;
                case LabelType.LCDRadio:
                    ApplyTextFormat(this, type);

                   /* leftPadding = 8;
                    rightPadding = 6;*/

                    break;
                case LabelType.LCDSmallHeadingBannerLight:
                    InitNormalBackground("basic_header_small_lightgrey", 8);
                    ApplyTextFormat(this, type);

                    leftPadding = 6;
                    rightPadding = 6;

                    break;

                case LabelType.LCDSmallHeadingBanner:
                    InitNormalBackground("basic_header_small", 8);
                    ApplyTextFormat(this, type);

                    leftPadding = 6;
                    rightPadding = 6;

                    break;
                case LabelType.RosterTitle:
                    InitNormalBackground("rosterpanel_titleBlack", 46);
                    ApplyTextFormat(this, type);

                    topPadding = 17;
                    leftPadding = 32;
                    rightPadding = 38;

                   /* InitNormalBackground("rosterpanel_title", 23);
                    ApplyTextFormat(this, type);

                    topPadding = 6;
                    leftPadding = 25;
                    rightPadding = 13;*/

                    break;

                case LabelType.LCDComboBoxItem:
                  //  InitHoverBackground("basic_dropdown_highlight", 8);
                    ApplyTextFormat(this, type);

                 //   leftPadding = 8;
                 //   rightPadding = 6;

                //    labelHoverColor = GetNormalColor(type);

                 //   EnableHover();

                    break;
                case LabelType.LCDBarfillBlue:
                    InitNormalBackground("lcd_barfill_blue", 10);

                    leftPadding = 10;
                    topPadding = 3;
                    ApplyTextFormat(this, type);


                    break;
                case LabelType.LCDBigHeaderBanner:
                    InitNormalBackground("basic_header_big", 10);
                    
                    leftPadding = 10;
                    topPadding = 3;
                    ApplyTextFormat(this, type);


                    break;
                case LabelType.LCDHeadingBlue:
                    InitMediumColoredBanner("basic_header_medium_blue", type);

                    break;
                case LabelType.LCDHeadingGreen:
                    InitMediumColoredBanner("basic_header_medium_green", type);

                    break;
                case LabelType.LCDHeadingRed:
                    InitMediumColoredBanner("basic_header_medium_red", type);

                    break;
                case LabelType.LCDHeadingBrown:
                    InitMediumColoredBanner("basic_header_medium_brown", type);

                    break;
                case LabelType.LCDHeadingGrey:
                    InitMediumColoredBanner("basic_header_medium_grey", type);

                    break;
                case LabelType.LCDHeadingSteelGrey:
                     InitMediumColoredBanner("basic_header_medium", type);

                    break;
                default:
                    ApplyTextFormat(this, type);

                    break;
            }

            FitToText();

        }

        private void InitMediumColoredBanner(string sprite, LabelType type)
        {
            InitNormalBackground(sprite, 20);

            leftPadding = lcdCornerHeadingPadding;
            topPadding = lcdMediumHeadingTopPadding;
            rightPadding = lcdCornerHeadingPadding;

            ApplyTextFormat(this, type);
        }

        /// <summary>
        /// if the label should appear in a different color when disabled, call this.
        /// </summary>
        /// <param name="disabledColor"></param>
        /// <param name="enabledColor"></param>
        public void InitDisabledColor(Color disabledColor, Color enabledColor)
        {
            this.DisabledColor = disabledColor;
            this.enabledColor = enabledColor;
        }


        private void InitNormalBackground(string backgroundSprite, int edgeSize)
        {
            InitBackground(backgroundSprite, edgeSize, ref background, true);
        }

        private void InitHoverBackground(string backgroundSprite, int edgeSize)
        {
            InitBackground(backgroundSprite, edgeSize, ref hoverBackground, false);
        }

        private void InitBackground(string backgroundSprite, int edgeSize, ref Bar backgroundToUse, bool addNow)
        {
            backgroundToUse = new Bar(guiManager);            
            if (addNow)
            {
                Add(backgroundToUse);
            }

            backgroundToUse.EdgeSize = edgeSize;
            Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
            backgroundToUse.SetSkinLocation(SkinState.Normal, rect);
            backgroundToUse.Height = rect.Height;

            drawChildrenFirst = true;
        }

        public static void ApplyTextFormat(IHasText component, LabelType type)
        {
            switch (type)
            {
                case LabelType.CRTBigGlow:
                    /*component.Font = GUIManager.CRTBigGlowFont;
                    component.Color = CRTLightBlue;*/

                    component.NormalColor = Color.White;

                    component.Font = GUIManager.LCDandHUDSubheadingFont;
                    component.RenderType = RenderType.CRTAndLCD;
                    break;
                case LabelType.CRTSmall:
                   /* component.Font = GUIManager.CRTSmallFont;
                    component.Color = CRTLightBlue;
                    */

                    component.NormalColor = Color.White; 
                    component.Font = GUIManager.LCDandHUDFont;

                    component.RenderType = RenderType.CRTAndLCD;
                    break;
                case LabelType.CRTNormal:
                  /*  component.Font = GUIManager.CRTBasicFont;
                    component.Color = CRTLightBlue;*/

                    component.NormalColor = Color.White; 
                    component.Font = GUIManager.LCDandHUDFont;
                    component.RenderType = RenderType.CRTAndLCD;
                    break;
                       
                case LabelType.PlainPanelNormal:
                    component.Font = GUIManager.LCDandHUDFont; 
                    component.NormalColor = GetNormalColorForType(type); // Color.Black;
                    break;
              
             /*   case LabelType.LCDHeader:
                    component.Font = GUIManager.LCDInterfaceBoldFont; // make bold
                    component.NormalColor = GetNormalColor(type); // Color.Black;
                    component.RenderType = RenderType.CRTAndLCD;
                    break;*/

                case LabelType.LCDCheckbox:
                case LabelType.LCDCheckboxWhite:
                case LabelType.LCDNormal:
                case LabelType.LCDNormalLight:
                case LabelType.LCDNormalDark:
                case LabelType.LCDWhite:
                case LabelType.LCDBarfillBlue:
                case LabelType.LCDBigHeaderBanner:
                case LabelType.LCDRadioBanner:
                case LabelType.LCDRadioBannerTinted:
                case LabelType.LCDRadio:
              //  case LabelType.LCDComboBoxItem:
                case LabelType.LCDHeadingRed:
                case LabelType.LCDHeadingGreen:
                case LabelType.LCDHeadingBlue:
                case LabelType.LCDHeadingBrown:
                case LabelType.LCDHeadingGrey:
                case LabelType.LCDHeadingSteelGrey:
                case LabelType.LCDSmallHeadingBanner:      
                case LabelType.LCDSmallHeadingBannerLight:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type); 
                    component.RenderType = RenderType.CRTAndLCD;

                    Label label = component as Label;
                    if (label != null)
                    {
                        label.labelHoverColor = lcdYellow;
                    }

                    break;

                case LabelType.LCDComboBoxItem:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type);
                    component.RenderType = RenderType.Normal; // make it float
                    break;

                case LabelType.RosterTitle:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type);
                    component.RenderType = RenderType.Normal;
                    break;

              

                case LabelType.LCDDate:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type);
                    component.RenderType = RenderType.CRTAndLCD;
                    break;

                case LabelType.LCDError:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type);
                    component.RenderType = RenderType.CRTAndLCD;
                    break;

                case LabelType.LCDWeather:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type);
                    component.RenderType = RenderType.CRTAndLCD;
                    break;

                case LabelType.EntityTypeTooltip:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type); //Color.White;
                    component.RenderType = RenderType.Normal;
                    break;
                case LabelType.EntityTypeTooltipHeader:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type); //Color.Yellow;
                    component.RenderType = RenderType.Normal;
                    break;
                case LabelType.EntityTypeTooltipSubHeading:
                    component.Font = GUIManager.LCDandHUDSubheadingFont;
                    component.NormalColor = GetNormalColorForType(type); //Color.Yellow;
                    component.RenderType = RenderType.Normal;
                    break;
                case LabelType.HUDWindow:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type); // Color.White;
                    component.RenderType = RenderType.Normal;
                    break;
                case LabelType.HUDWindowHeader:
                    component.Font = GUIManager.LCDandHUDFont;
                    component.NormalColor = GetNormalColorForType(type); // Color.Yellow;
                    component.RenderType = RenderType.Normal;
                    break;              
            }
        }
       
        public override string ToolTip
        {
            
            get { return base.ToolTip; }
            set
            {
                if (base.toolTip != value)
                {
                    base.ToolTip = value;

                    if (!string.IsNullOrEmpty(value))
                    {
                        EnableHover();
                    }
                    else
                    {
                        DisableHover();
                    }
                }
            }
        }

        private bool hoverEnabled = false;
        
        private void EnableHover()
        {
            if (hoverEnabled == false)
            {
               // normalColor = color;
                MouseOver += SetHover;
                MouseOut += ResetHover;

                hoverEnabled = true;

                CanHaveFocus = true;
            }
        }

        private void DisableHover()
        {
            if (hoverEnabled == true)
            {
              //  color = normalColor;

                MouseOver -= SetHover;
                MouseOut -= ResetHover;

                hoverEnabled = false;

                CanHaveFocus = false;
            }
        }

        protected void SetHover(UIComponent sender, MouseEventArgs args)
        {
            if (normalColor == null)
            {
                normalColor = color;
            }

            color = labelHoverColor;

            if (hoverBackground != null)
            {
                if (background != null)
                {
                    Remove(background);
                }

                Add(hoverBackground);
            }
        }

        protected void ResetHover(UIComponent sender, MouseEventArgs args)
        {
            // revert to normal color:
            color = normalColor.Value;

            if (hoverBackground != null)
            {
                Remove(hoverBackground);

                if (background != null)
                {
                    Add(background);
                }
            }

        }

        /// <summary>
        /// this is for special marker colors to revert to the original
        /// </summary>
        /// <returns></returns>
        public Color GetNormalColorForType()
        {
            return GetNormalColorForType(type);
        }

       
        /// <summary>
        /// this is for special marker colors to revert to the original
        /// </summary>
        /// <returns></returns>
        private static Color GetNormalColorForType(LabelType type)
        {
            switch (type)
            {
                case LabelType.LCDNormalDark:
                case LabelType.RosterTitle:
                    return LCDDark;

               
              //  case LabelType.LCDHeader:
                case LabelType.LCDComboBoxItem:
                case LabelType.LCDNormal:
                case LabelType.LCDBarfillBlue:
                case LabelType.LCDSmallHeadingBannerLight:
                    return LCDNormal; // Color.Black;

                case LabelType.LCDNormalLight:
                case LabelType.LCDCheckbox:
                case LabelType.LCDCheckboxWhite:
                case LabelType.LCDRadioBanner:
                case LabelType.LCDRadio:
                case LabelType.LCDHeadingRed:
                case LabelType.LCDHeadingGreen:
                case LabelType.LCDHeadingBlue:
                case LabelType.LCDHeadingBrown:
                case LabelType.LCDHeadingGrey:
                case LabelType.LCDHeadingSteelGrey:
                case LabelType.LCDSmallHeadingBanner:
                    return lcdLight;

                case LabelType.LCDWhite:
                    return Color.White;

                case LabelType.LCDDate:
                    return Color.LightCyan;
                case LabelType.LCDError:
                    return errorColor;
                case LabelType.LCDWeather:
                    return Color.Gold;
                case LabelType.LCDBigHeaderBanner:
                    return lcdYellow;
                case LabelType.HUDWindow:
                    return Color.White;
                case LabelType.HUDWindowHeader:
                    return Color.Yellow;
                case LabelType.EntityTypeTooltip:
                    return Color.White;
                case LabelType.EntityTypeTooltipHeader:
                    return Color.Yellow; 
                case LabelType.PlainPanelNormal:
                    return Color.Black;

                default: return Color.White;
            }
        }

        int cursorWidth = 15;

        public void FitToText()
        {
            int width = TextWidth + leftPadding + rightPadding;

            if (background != null)
            {
                // prevent overlapping edges:
                width = (int)MathHelper.Max(background.EdgeSize * 2, width);
            }

            if (EnableCursor)
            {
                width += cursorWidth;
            }

            Width = width;

            ResetHeight();          
        }

        /// <summary>
        /// Load default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            /* OBSDEFAULT*/
        /*    if (loadAllContent)
                Font = GUIManager.ContentManager.Load<SpriteFont>(GUIManager.LCDInterfaceFontPath); // defaultFont;
            */
            base.LoadGraphicsContent(loadAllContent);
        }

        /// <summary>
        /// Invalidate the control so that the texture is drawn on the first
        /// frame.
        /// </summary>
        public override void Initialize()
        {
            this.isRedrawRequired = true;

            base.Initialize();
        }

        public override void CleanUp()
        {
            if (IsInitialized)
            {
               
               // this.renderedTexture = null;
            }

            base.CleanUp();
        }

        /// <summary>
        /// This method is called when the graphics device has been reset, so a
        /// redraw is required.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        public void UnloadGraphicsContent(bool unloadAllContent)
        {
            // Make changes to handle the new device
          /*  if (this.renderTarget != null)
            {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }*/

            // Control must be redrawn after device changes
            this.isRedrawRequired = true;

            base.UnloadGraphicsContent(unloadAllContent);
        }

        /// <summary>
        /// Draws the text and performs clipping. Clipping is implemented by
        /// rendering to a texture, which can be kept to draw each frame, until
        /// the control is invalidated, such as a text change, a resize, colour
        /// change, or if the graphics device becomes invalidated.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        /// <param name="parentScissor">The scissor region of the parent control.</param>
        protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
        {

            // if the label has children, the text is only for measuring. We don't want to draw it.
            if ((background != null || hoverBackground != null || this.Controls.Count == 0) 
                && this.font != null)
            {
                bool draw = true;
                Rectangle source;
                Rectangle destination;
                int dif;

                int widthToUse = Width;
              

                source = new Rectangle(0, 0, widthToUse, Height);
                destination = new Rectangle(AbsolutePosition.X + leftPadding, AbsolutePosition.Y + topPadding, widthToUse, Height);

                if (!parentScissor.Contains(destination))
                {
                    // Perform culling
                    if (parentScissor.Intersects(destination))
                    {
                        // Perform clipping

                        if (destination.X < parentScissor.X)
                        {
                            dif = parentScissor.X - destination.X;

                            if (destination.Width == source.Width)
                            {
                                source.Width -= dif;
                                source.X += dif;
                                destination.Width -= dif;
                                destination.X += dif;
                            }
                            else
                            {
                                destination.Width -= dif;
                                destination.X += dif;
                            }
                        }
                        else if (destination.Right > parentScissor.Right)
                        {
                            dif = destination.Right - parentScissor.Right;

                            if (destination.Width == source.Width)
                            {
                                source.Width -= dif;
                                destination.Width -= dif;
                            }
                            else
                                destination.Width -= dif;
                        }

                        if (destination.Y < parentScissor.Y)
                        {
                            dif = parentScissor.Y - destination.Y;

                            if (destination.Height == source.Height)
                            {
                                source.Height -= dif; //error in this code??
                                source.Y += dif;
                                destination.Height -= dif;
                                destination.Y += dif;
                            }
                            else
                            {
                                destination.Height -= dif;
                                destination.Y += dif;
                            }
                        }

                        /*else*/ // NEW - clip bottom too
                        if (destination.Bottom > parentScissor.Bottom)
                        {
                            dif = destination.Bottom - parentScissor.Bottom;

                            if (destination.Height == source.Height)
                            {
                                source.Height -= dif;
                                destination.Height -= dif;
                            }
                            else
                                destination.Height -= dif;
                        }
                    }
                    else
                        draw = false;
                }

                // Only draw if necessary, because scissor rectangles are expensive
                if (draw)
                {
                    spriteBatch.End();
                    //  spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None); // XNA 3
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                   
                    // spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = true; // XNA 3
                    spriteBatch.GraphicsDevice.RasterizerState = scissorTestRasterizerState;

                    spriteBatch.GraphicsDevice.ScissorRectangle = destination;

                    Color currentColor = color;
                    if (alpha != 1)
                    {
                        Vector4 col = currentColor.ToVector4();
                        col.W *= alpha;
                        currentColor = new Color(col); 
                    }

                    string textToDraw = text;
                    if (isCursorShown)
                    {
                        textToDraw += cursor; //"" + cursor; // += cursor;
                    }

                    spriteBatch.DrawString(this.font, textToDraw, new Vector2(AbsolutePosition.X + leftPadding, AbsolutePosition.Y + topPadding), this.color);
                
                    spriteBatch.End();

                    // Start the original drawing mode again, so that other
                    // controls are not affected.
                    //  spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None); // XNA 3
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    // Reset original viewport
                    //spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = false; XNA 3
                    spriteBatch.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                }
            }
        }

        protected override void OnResize(UIComponent sender)
        {
            viewPort.X = X;
            viewPort.Y = Y;
            viewPort.Width = Width;
            viewPort.Height = Height;

            if (this.background != null)
            {
                this.background.Width = Width;
                this.background.Height = Height;
            }

            if (this.hoverBackground != null)
            {
                this.hoverBackground.Width = Width;
                this.hoverBackground.Height = Height;
            }

            base.OnResize(sender);
        }
    }
}
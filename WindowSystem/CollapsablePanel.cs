using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowSystem
{
    public class CollapsablePanel: UIComponent
    {
        public enum PanelType { Node, DropDownBig, HUD, StockpileHUD, Panel, DropDownSmall, HUDSmall }

        public const int HUD_textbox_CornerSize = 8;
        public const int HUD_button_CornerSize = 9;

        public bool isExpanded = false;
        protected PanelType type;

        // these are contained in the header:
        ImageButton expandButton; // only used on set resource panel
        Icon expandIcon;

        Label lblTitle;
        public Label lblTitleSummary;

       
        TextButton headerbox;

        /// <summary>
        /// This is where stuff should be added to.
        /// it is a Box in order to accomodate a background image
        /// </summary>
        public Box ExpandedPanel;

        /// <summary>
        /// optional icon, used with Node style
        /// </summary>
        Image icon;

        private const int iconX = 12;

        
        public delegate void ExpandHandler();        
        public event ExpandHandler ExpandEvent;

        /// <summary>
        /// this gets overwritten...
        /// </summary>
        public int CollapsedHeight = 14; // 18;

        /// <summary>
        /// where the content panel is placed from the top
        /// </summary>
        public int ExpandedPanelYPos = 20;

        /// <summary>
        /// the vertical space from where the expanded panel starts to where the first row appears
        /// </summary>
        private int expandedPanelTopPadding = 0;

        /// <summary>
        /// the width of the empty area between the panel/header and scrollbar
        /// </summary>
        public int CollapsablePanelRightPadding = 0;

        /// <summary>
        /// empty space to the right and left of the expanded panel - not the headerbox.
        /// </summary>
        private int expandedPanelHorizMargin = 0;

        private const int headingXPosDropDown = 30; // 40;
        private const int headingXPosNode = 36;

        /// <summary>
        /// distance from right edge to end of summary text
        /// </summary>
        private const int headingSummaryRightPaddingDropDown = 30; // 9; make place for arrow?
        private const int headingSummaryRightPaddingNode = 14;

        const string hudCollapsedArrowSprite = "HUD_rightarrow";
        const string lcdCollapsedArrowSprite = "lcd_rightarrow";
        const string lcdExpandedArrowSprite = "lcd_downarrow";


        public int HeadingYPos = 3;

        public int? TitleSummaryRightAlignXPos;

        //public CollapsablePanel(Game game, GUIManager guiManager)

        public CollapsablePanel(GUIManager guiManager, PanelType type)
            : base(guiManager)
        {
            this.type = type;

            CanHaveFocus = false;

            #region Create Child Controls
            if (!(type == PanelType.Panel))
            {
                if (type == PanelType.DropDownBig || type == PanelType.StockpileHUD || type == PanelType.DropDownSmall || type == PanelType.HUDSmall)
                {
                    this.expandIcon = new Icon(guiManager);
                    expandIcon.ScaleImageToSizeOfControl = false;
                    expandIcon.CanHaveFocus = false;
                }
                else
                {

                    this.expandButton = new ImageButton(guiManager);
                    this.expandButton.DebugTag = "expandButton";
                    this.expandButton.ZOrder = 1f;
                    expandButton.Click += new ClickHandler(expandButton_Click);
                }                
            }

            this.ExpandedPanel = new Box(guiManager); //new UIComponent(guiManager);
           

            this.lblTitle = new Label(guiManager);
            this.lblTitle.DebugTag = "collapseTitle";

            #endregion

            #region Add Child Controls
            if (type == PanelType.DropDownBig || type == PanelType.StockpileHUD || type == PanelType.DropDownSmall || type == PanelType.HUDSmall)
            {               
                headerbox = new TextButton(guiManager);
                Add(headerbox);
            }

            if (!(type == PanelType.Panel)) // ?? 
            {
                if (type == PanelType.DropDownBig || type == PanelType.StockpileHUD || type == PanelType.DropDownSmall || type == PanelType.HUDSmall)
                {
                    Add(this.expandIcon);
                }
                else
                {
                    Add(this.expandButton);
                }
            }

           /* if (!(type == PanelType.Panel))
            {
                Add(this.expandButton);
            }*/
            
            Add(this.lblTitle);
            //Add(this.expandedPanel);            // starts collapsed
            #endregion

            ExpandedPanel.CanHaveFocus = false;

          /*  if (!(type == PanelType.Panel))
            {
                expandButton.Click += new ClickHandler(expandButton_Click);
            }*/
        }


        public string TitleTooltip
        {
            set
            {
                lblTitle.ToolTip = value;
            }
        }
       
        public bool IsExpanded
        {
            get { return isExpanded; }
            set 
            {
                if (isExpanded != value)
                {
                    if (value == true)
                    {
                        Expand();
                    }
                    else
                    {
                        Collapse();
                    }
                    isExpanded = value;
                }
            }

        }

        void expandButton_Click(UIComponent sender, EventArgs e)
        {
            if (!isExpanded)
            {
                IsExpanded = true;
                if (ExpandEvent != null)
                {
                    ExpandEvent.Invoke();
                }
            }
            else
            {
                IsExpanded = false;

            }
        }

        public void AddContent(UIComponent content, bool hookToResize = true)
        {           
            ExpandedPanel.Add(content);

            // NEW:
            content.Y = expandedPanelTopPadding;

            if (hookToResize == true)
            {
                content.Resize += new ResizeHandler(content_Resize);
            }
        }

        void content_Resize(UIComponent sender)
        {
            ExpandedPanel.Height = sender.Bottom; // sender.Height + sender.Y;

            if (isExpanded)
            {   
                Height = ExpandedPanelYPos + ExpandedPanel.Height; 
            }
            else
            {
                Height = CollapsedHeight;
            }
        }

        private void Collapse()
        {
            Height = CollapsedHeight;
            Remove(ExpandedPanel); // we don't want the gui code to loop through these unnecessarily, they can be quite complex.

            if (type == PanelType.Node)
            {
                expandButton.Init(ImageButtonType.LCDExpand);
            }
            else if (type == PanelType.DropDownSmall)
            {
                //expandButton.Init(ImageButtonType.LCDArrowRight);

                SetIconCollapsed(lcdCollapsedArrowSprite); // "lcd_rightarrow");
            }
            else if (type == PanelType.DropDownBig)
            {
               // expandButton.Init(ImageButtonType.LCDArrowRight);

                SetIconCollapsed(lcdCollapsedArrowSprite);
            }
            else if (type == PanelType.HUD)
            {
                expandButton.Init(ImageButtonType.HUDArrowRight);
            }
           /* else if (type == PanelType.StockpileHUD)
            {
                expandButton.Init(ImageButtonType.HUDExpandArrowRight);
            }*/
            else if (type == PanelType.HUDSmall)
            {
                SetIconCollapsed(hudCollapsedArrowSprite);          
            }
        }

        private void SetIconCollapsed(string sprite)
        {
            expandIcon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(sprite));
            expandIcon.ResizeControlToFitImage();

            if (type == PanelType.HUDSmall)
            {
                expandIcon.Y = 4;
            }
        }

        private void SetIconExpanded(string sprite)
        {
            expandIcon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(sprite));
            expandIcon.ResizeControlToFitImage();

            if (type == PanelType.HUDSmall)
            {
                expandIcon.Y = 6;
            }
        }

        private void Expand()
        {
            Height = ExpandedPanelYPos + ExpandedPanel.Height; 

            Add(ExpandedPanel);

            ExpandedPanel.Y = ExpandedPanelYPos; 

            if (type == PanelType.Node)
            {
                expandButton.Init(ImageButtonType.LCDCollapse);
            }
            else if (type == PanelType.DropDownSmall)
            {
                //expandButton.Init(ImageButtonType.LCDArrowDownNew);

                SetIconExpanded(lcdExpandedArrowSprite);
            }
            else if (type == PanelType.DropDownBig)
            {
              //  expandButton.Init(ImageButtonType.LCDArrowDown);

                SetIconExpanded(lcdExpandedArrowSprite);    
            }
            else if (type == PanelType.HUD)
            {
                expandButton.Init(ImageButtonType.HUDArrowDown);
            }
           /* else if (type == PanelType.StockpileHUD)
            {
                expandButton.Init(ImageButtonType.HUDExpandArrowDown);
            }*/
            else if (type == PanelType.HUDSmall)
            {
                SetIconExpanded("HUD_downarrow");    
                //expandButton.Init(ImageButtonType.HUDExpandArrowDown);
            }
        }

      

       // public Point TitlePosition
        public int TitlePositionX
        {
            set
            {
                lblTitle.X = value;
            }
        }

        public string Title
        {
            set 
            { 
                lblTitle.Text = value;
                lblTitle.Width = lblTitle.TextWidth;
                lblTitle.Height = lblTitle.TextHeight;
            }

            get { return lblTitle.Text; }
        }

        public string Summary
        {
            set
            {
                lblTitleSummary.Text = value;
                lblTitleSummary.Width = lblTitleSummary.TextWidth;
                lblTitleSummary.Height = lblTitleSummary.TextHeight;

                RightAlignTitleSummary();
               
            }

            get
            {
                return lblTitleSummary.Text;
            }
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
               
                base.Width = value;

                ExpandedPanel.Width = value - CollapsablePanelRightPadding - 2 * expandedPanelHorizMargin;
                
                if (type == PanelType.DropDownBig
                    || type == PanelType.DropDownSmall
                    || type == PanelType.HUDSmall)
                {
                    // right justify:
                    RightAlignTitleSummary();

                    SetExpandButtonPosition();

                }

                if (headerbox != null)
                {                    
                    headerbox.Width = value - CollapsablePanelRightPadding;
                   
                }                
            }
        }

        private void SetExpandButtonPosition()
        {
            UIComponent componentToUse = expandButton ?? expandIcon;

            if (componentToUse != null)
            {
                // right justify:
                if (type == PanelType.HUDSmall)
                {
                    componentToUse.X = Width - 70; 
                }
                else
                {
                    componentToUse.X = Width - 24; // 20;
                }
            }
           
        }

        public void SetIcon(string spriteName)
        {
            if (icon == null)
            {
                icon = new Image(guiManager);                
                Add(icon);
                icon.X = iconX;
                
               // icon.DebugTag = "categoryIcon";
            }

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(spriteName);                
            icon.SetSkinLocation(SkinState.Normal,rect);
            icon.ResizeControlToFitImage();
            headerbox.CenterChildVertically(icon);

            //??
          //  lblTitle.Position = new Point(headingXPosNode, HeadingYPos);
          /*  lblTitle.X = headingXPosNode;
            headerbox.CenterChildVertically(lblTitle);*/
        }

        private void RightAlignTitleSummary()
        {
            if (TitleSummaryRightAlignXPos.HasValue)
            {
                lblTitleSummary.AlignRight(TitleSummaryRightAlignXPos.Value);
            }
            else
            {
                if (type == PanelType.DropDownBig
                    || type == PanelType.DropDownSmall
                    || type == PanelType.Node)
                {
                    lblTitleSummary.X = Width - headingSummaryRightPaddingDropDown - lblTitleSummary.TextWidth;
                }
               /* else if (type == PanelType.StockpileHUD)
                {
                    lblTitleSummary.X = Width - 172 - lblTitleSummary.TextWidth;
                }*/
            }
        }

        public int GetPaddingRight()
        {
            if (type == WindowSystem.CollapsablePanel.PanelType.DropDownBig)
            {
                return CollapsablePanel.headingSummaryRightPaddingDropDown;
            }
            else
            {
                return CollapsablePanel.headingSummaryRightPaddingNode;
            }
        }

        public void RightJustifyLabel(Label lblTitleSummary)
        {
            if (type == WindowSystem.CollapsablePanel.PanelType.DropDownBig
             || type == WindowSystem.CollapsablePanel.PanelType.DropDownSmall)
            {
                lblTitleSummary.X = Width - CollapsablePanel.headingSummaryRightPaddingDropDown - lblTitleSummary.TextWidth;
            }
            else
            {
                lblTitleSummary.X = Width - CollapsablePanel.headingSummaryRightPaddingNode - lblTitleSummary.TextWidth;
            }
        }

        public void CenterOnHeader(UIComponent control)
        {
            headerbox.CenterChildVertically(control);
        }


        public enum SubType { Normal, Blue, Green, Red }
        public void Init(SubType subtype = SubType.Normal) //PanelType type)
        {
            lblTitleSummary = new Label(guiManager);
            Add(lblTitleSummary);

           
           // this.type = type;
            switch (type)
            {
                case PanelType.HUD:
                    expandButton.Init(ImageButtonType.HUDArrowRight);
                    expandButton.Position = new Point(0, HeadingYPos);

                    lblTitle.Init(Label.LabelType.HUDWindow);
                    lblTitle.Position = new Point(headingXPosDropDown /*headingXPosNode*/, HeadingYPos); //16, 0);

                    lblTitleSummary.Init(Label.LabelType.HUDWindow);

                    break;
                case PanelType.Node: // not used
                    expandButton.Init(ImageButtonType.LCDExpand);
                    expandButton.Position = new Point(0, HeadingYPos);

                    lblTitle.Init(Label.LabelType.LCDNormal);
                    lblTitle.Position = new Point(headingXPosDropDown, HeadingYPos); //16, 0);

                    lblTitleSummary.Init(Label.LabelType.LCDNormal);

                    break;
                case PanelType.DropDownSmall:
                    {
                        //*******************
                        // side panel
                        //*******************

                        Label.LabelType labelType = Label.LabelType.LCDNormalLight;

                        HeadingYPos = 5;

                        lblTitle.Init(labelType); // LCDNormal);
                        lblTitle.Position = new Point(13, HeadingYPos); //16, 0);

                        headerbox.Init(TextButton.TextButtonType.LCDCollapsableHeaderSmall);

                        SetIconCollapsed(lcdCollapsedArrowSprite);
                        headerbox.CenterChildVertically(expandIcon);
                        expandIcon.Y += 1;
                        SetExpandButtonPosition();     

                      /*  expandButton.Init(ImageButtonType.LCDArrowRight);                   
                        headerbox.CenterChildVertically(expandButton);
                        expandButton.Y += 2;
                        SetExpandButtonPosition();                       
                        */
                    
                        headerbox.Click += new ClickHandler(expandButton_Click);

                        lblTitleSummary.Init(labelType);

                        // headerbar.X = 3; // !!!!

                        ExpandedPanelYPos = headerbox.Height - 2;

                        
                        break;
                    }
                case PanelType.DropDownBig:
                    {
                        //********
                        // Inventory panel and Buy/Sell panel. 
                        //*********

                        Label.LabelType labelType = Label.LabelType.LCDNormalLight;

                        HeadingYPos = 8; //6; 

                        lblTitle.Init(labelType); // LCDNormal);
                        lblTitle.Position = new Point(headingXPosDropDown, HeadingYPos); //16, 0);

                        TextButton.TextButtonType textButtonType;
                        switch(subtype)
                        {
                            case SubType.Normal:
                                textButtonType = TextButton.TextButtonType.LCDCollapsableHeaderBig;
                                break;
                            case SubType.Green:
                                textButtonType = TextButton.TextButtonType.LCDCollapsableHeaderBigGreen;
                                break;
                            case SubType.Blue:
                                textButtonType = TextButton.TextButtonType.LCDCollapsableHeaderBigBlue;
                                break;
                            case SubType.Red:
                                textButtonType = TextButton.TextButtonType.LCDCollapsableHeaderBigRed;
                                break;
                            default: textButtonType = TextButton.TextButtonType.LCDCollapsableHeaderBig;
                                break;
                        }
                        headerbox.Init(textButtonType);

                        SetIconCollapsed(lcdCollapsedArrowSprite);
                        headerbox.CenterChildVertically(expandIcon);
                        expandIcon.Y += 1;
                        SetExpandButtonPosition(); 
                        
                        headerbox.Click += new ClickHandler(expandButton_Click);
                        
                        lblTitleSummary.Init(labelType);


                        ExpandedPanelYPos = headerbox.Bottom - 5; // 4; // headerbox.Height - 2;

                        expandedPanelTopPadding = 6;

                        float alpha = 0.5f;
                        Color color = new Color(alpha, alpha, alpha, alpha);
                        Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background");
                        ExpandedPanel.SetSkinLocation(SkinState.Normal,rect, color, color);

                        ExpandedPanel.CornerSize = 6;
                        expandedPanelHorizMargin = 6;

                        break;
                    }
                case PanelType.StockpileHUD:
                    {
                        Remove(expandButton);

                        headerbox.Init(TextButton.TextButtonType.HUD);
                        headerbox.Height = 31;

                        Rectangle rect;
                      /* rect = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_textbox");
                        headerbox.Skin = rect;
                      
                        //headerbox.SetSkinLocation(SkinState.Normal,rect);
                        headerbox.CornerSize = HUD_textbox_CornerSize; 
                        headerbox.Width = Width;
                        headerbox.Height = 31; // 24; // CollapsedHeight; // rect.Height;
*/
                        headerbox.Click += new ClickHandler(expandButton_Click);
                        
                        ExpandedPanelYPos = headerbox.Height - 3;

                        CollapsablePanelRightPadding = 6;

                        lblTitle.Init(Label.LabelType.HUDWindow);
                        lblTitle.Position = new Point(20, HeadingYPos); //16, 0);
                        headerbox.CenterChildVertically(lblTitle);


                      
                        //headerbar.CenterVertically(lblTitle);

                      //  expandButton.Init(ImageButtonType.HUDExpandArrowRight);
                     //   expandButton.X = 192;                      
                     //   headerbox.CenterVertically(expandButton);

                        lblTitleSummary.Init(Label.LabelType.HUDWindow);
                        headerbox.CenterChildVertically(lblTitleSummary);

                        HeadingYPos = lblTitle.Y;

                        // set a background gradient image:
                        rect = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_gradient");
                        ExpandedPanel.SetSkinLocation(SkinState.Normal,rect);
                        ExpandedPanel.CornerSize = 6;
                        expandedPanelHorizMargin = 3;

                        // ExpandedPanel.ScaleImageToSizeOfControl = true;

                        //lblTitleSummary.X = 

                        // headerbar.X = 3; // !!!!
                        break;
                    }
                case PanelType.HUDSmall: // overlay menu panel
                    {
                      //  Remove(expandButton);
                       // SetExpandButtonPosition(); 

                        headerbox.Init(TextButton.TextButtonType.HUD);
                        headerbox.Height = 27;

                        Rectangle rect;
                        /* rect = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_textbox");
                          headerbox.Skin = rect;
                      
                          //headerbox.SetSkinLocation(SkinState.Normal,rect);
                          headerbox.CornerSize = HUD_textbox_CornerSize; 
                          headerbox.Width = Width;
                          headerbox.Height = 31; // 24; // CollapsedHeight; // rect.Height;
  */
                        headerbox.Click += new ClickHandler(expandButton_Click);

                        ExpandedPanelYPos = headerbox.Height - 3;

                        CollapsablePanelRightPadding = 6;

                        lblTitle.Init(Label.LabelType.HUDWindow);
                        lblTitle.Position = new Point(20, HeadingYPos); //16, 0);
                        headerbox.CenterChildVertically(lblTitle);



                        //headerbar.CenterVertically(lblTitle);

                        //  expandButton.Init(ImageButtonType.HUDExpandArrowRight);
                        //   expandButton.X = 192;                      
                        //   headerbox.CenterVertically(expandButton);

                        lblTitleSummary.Init(Label.LabelType.HUDWindow);
                        headerbox.CenterChildVertically(lblTitleSummary);

                        HeadingYPos = lblTitle.Y;

                        // set a background gradient image:
                        rect = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_gradient");
                        ExpandedPanel.SetSkinLocation(SkinState.Normal,rect);
                        ExpandedPanel.CornerSize = 6;
                        expandedPanelHorizMargin = 3;


                        SetIconCollapsed(hudCollapsedArrowSprite); // NEW
                        headerbox.CenterChildVertically(expandIcon);
                       // expandIcon.Y += 1;
                        SetExpandButtonPosition();     

                        break;
                    }
                case PanelType.Panel:
                    lblTitle.Init(Label.LabelType.LCDNormal);
                    lblTitle.Position = new Point(headingXPosDropDown /*headingXPosNode*/, HeadingYPos); //16, 0);

                    break;
            }

            lblTitleSummary.Y = HeadingYPos;

            ExpandedPanel.Width = Width;
            ExpandedPanel.X = expandedPanelHorizMargin;
            ExpandedPanel.Y = ExpandedPanelYPos; // CollapsedHeight;

            // start off collapsed:
            Collapse();

        }

        void headerbox_Click(UIComponent sender, EventArgs e)
        {

        }

    }
}

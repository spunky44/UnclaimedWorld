using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowSystem
{
    public class ActionButtonPanel: UIComponent
    {
        public enum ActionType { Cancel, Ok, Scrap }

       
        Box brown;
        TextButton bt1, bt2, bt3;
        Image scratch1, scratch2;

        ActionType action1, action2, action3;


        public delegate void ActionButtonHandler(ActionType action);
        public event ActionButtonHandler ActionButtonEvent;

       /*       
        public int HeadingXPos = 30; // 40;

        public int HeadingSummaryRightPadding = 6; //30;

        public int HeadingYPos = 3;
        */

        //public int MarginX = 14;

        private const int buttonX = 2;

        protected const int defaultHeight = 40;


        //public CollapsablePanel(Game game, GUIManager guiManager)

        public ActionButtonPanel(GUIManager guiManager)
            : base(guiManager)
        {
            CanHaveFocus = false;

            #region Create Child Controls
          
            // brown blot:
            brown = new Box(guiManager);
            bt1 = new TextButton(guiManager);
            bt2 = new TextButton(guiManager);
            bt3 = new TextButton(guiManager);
            scratch1 = new Image(guiManager);
            scratch2 = new Image(guiManager);
                      
            #endregion

            #region Add Child Controls           
            Add(this.brown);
            Add(scratch1);
            Add(scratch2);
            Add(bt1);
            Add(bt2);
            Add(bt3);
            #endregion

            

        }


        

      /*  void content_Resize(UIComponent sender)
        {
            ExpandedPanel.Height = sender.Height;            
        }*/

       

      

     

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {                
                brown.Width = value;
                
                base.Width = value;

            }
        }

        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                brown.Height = value;

                base.Height = value;                
            }
        }
       

        public void Init() //PanelType type)
        {
            Rectangle rect;

            this.Height = defaultHeight;

          /*  switch (type)
            {
                case PanelType.Node:*/
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
                    brown.SetSkinLocation(SkinState.Normal,rect);
                    brown.CornerSize = 3;
                 //   brown.Position = new Point(MarginX, Form.Height - 60); 
                 //   brown.Width = GetContentWidth();
                    brown.Height = defaultHeight;

                  //  Image scratch1 = AddImage(intf.gui, Form, "basic_scratch1", new Point(brown.X - 4, brown.Y - 9));
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scratch1");
                    scratch1.SetSkinLocation(SkinState.Normal,rect);
                    //window.Add(image);
                    scratch1.Position = new Point(brown.X - 4, brown.Y - 9);
                    scratch1.ResizeControlToFitImage();
                    scratch1.CanHaveFocus = false; // don't destroy buttons!

                   // Image scratch2 = AddImage(intf.gui, Form, "basic_scratch2", new Point(brown.X - 10, brown.Y + 22));
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scratch2");
                    scratch1.SetSkinLocation(SkinState.Normal,rect);
                    //window.Add(image);
                    scratch1.Position = new Point(brown.X - 10, brown.Y + 22);
                    scratch1.ResizeControlToFitImage();
                    scratch1.CanHaveFocus = false; // don't destroy buttons!
                                

                    //Form.Add(button1); // add first!!! sets defaults!
                    int btDistance = -2;

                    bt1.Init(TextButton.TextButtonType.White);
                    bt1.Position = new Point(buttonX, brown.Y);
                    bt1.Click  += new ClickHandler(bt1_Click);

                    bt2.Init(TextButton.TextButtonType.White);
                    bt2.Position = new Point(bt1.X + bt1.Width + btDistance, brown.Y);
                    bt2.Click += new ClickHandler(bt2_Click); 

                    bt3.Init(TextButton.TextButtonType.White);
                    bt3.Position = new Point(bt2.X + bt2.Width + btDistance, brown.Y);
                    bt3.Click += new ClickHandler(bt3_Click); 

                 //   bt1.Text = button1Text;
                 //   bt1.ToolTip = button1Tooltip;

            /*        break;
               
            }**/
           

        }

        public void SpreadButtons()
        {
            if (!Contains(bt3))
            {
                bt2.X = Width - bt2.Width - buttonX;
            }
        }

        public void ClearCommands()
        {
            Remove(bt1);
            Remove(bt2);
            Remove(bt3);
        }

    /*    public void AddCommand(string text, string tooltip)
        {


        }*/

        public void SetCommands(string text1, string tooltip1)
        {
            Remove(bt2);
            Remove(bt3);

            SetCommand1(text1, tooltip1);           
        }

        public void SetCommands(string text1, string tooltip1, string text2, string tooltip2)
        {            
            Remove(bt3);

            SetCommand1(text1, tooltip1);
            SetCommand2(text2, tooltip2);
        }

        public void SetCommands(string text1, string tooltip1, string text2, string tooltip2, string text3, string tooltip3)
        {
            
            SetCommand1(text1, tooltip1);
            SetCommand2(text2, tooltip2);
            SetCommand3(text3, tooltip3);
        }

        private void SetCommand1(string text1, string tooltip1)
        {
            Add(bt1);
            bt1.Text = text1;
            bt1.ToolTip = tooltip1;
        }

        private void SetCommand2(string text1, string tooltip1)
        {
            Add(bt2);
            bt2.Text = text1;
            bt2.ToolTip = tooltip1;
        }

        private void SetCommand3(string text1, string tooltip1)
        {
            Add(bt3);
            bt3.Text = text1;
            bt3.ToolTip = tooltip1;
        }

        void bt3_Click(UIComponent sender, EventArgs e)
        {
            if (ActionButtonEvent != null)
            {
                ActionButtonEvent.Invoke(action3);
            }
        }

        void bt2_Click(UIComponent sender, EventArgs e)
        {
            if (ActionButtonEvent != null)
            {
                ActionButtonEvent.Invoke(action2);
            }
        }

        void bt1_Click(UIComponent sender, EventArgs e)
        {
            if (ActionButtonEvent != null)
            {
                ActionButtonEvent.Invoke(action1);
            }
        }
    }
}

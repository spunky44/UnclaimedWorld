using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.ClientSide.Interface.Layout;

namespace UWGame.ClientSide.HelpTopics
{
    public class HelpTopic: IGameData
    {
        
        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public LayoutElement[] FlowElements;

       /* public string Image;

        public string Text;*/

        /*
        public const string HeaderColor = "#FF0000";
        public const string TextColor = "#FFFF00";
        public const string HintColor = "#00FFFF";
        */

        public const string ColorHeader = "#COLORHEADER";
        public const string ColorHeaderOnLightBG = "#COLORHEADERDARK";
        public const string ColorMember = "#COLORMEMBER";
        public const string ColorDate = "#COLORDATE";
        public const string ColorItem = "#COLORTYPE";



        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        public void Initialize()
        {
          
        }

        public void PostInitValidate(ref List<string> errors)
        {
           
        }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}

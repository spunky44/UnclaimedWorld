using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface
{
    public class IconInfo: IGameData
    {

        public int? CenterYPos;
       // public int? CenterXPos;

        /// <summary>
        /// use this for the sprite name...
        /// </summary>
        public string KeyName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }


        public int GetYPosAdjustment(int spriteHeight)
        {
            if (CenterYPos.HasValue)
            {
                return spriteHeight / 2 - CenterYPos.Value;
            }

            return 0;
        }



        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {           
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}

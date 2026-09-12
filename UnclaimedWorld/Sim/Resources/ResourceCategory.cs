using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace UWGame.SimSide.Resources
{
    [DebuggerDisplay("{KeyName}")]
    public class ResourceCategory: ICategoryType/*, IHasIcon*/, IGameData
    {

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        /// <summary>
        /// print info in this color on the editor map
        /// </summary>
        /// 
        #region Client properties

       // public enum OverlayOptions { NotInMenu, DefaultInMenu, AppearsInMenu }

      //  public OverlayOptions OverlayOption = OverlayOptions.AppearsInMenu;

      //  public bool AppearsInOverlayMenu = true;
        public Color? Color; // = Color.White;
        #endregion


        public bool ShouldSerializeColor()
        {
            return Color != null;
        }

      /*  public string SpriteName;

        [XmlIgnore]
        public string IconSpriteName { get; set; }
        */
        public void Initialize()
        {
         //   IconSpriteName = SpriteName + "_icon";
        }
        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
        public string KeyName
        {
            get;
            set;
        }
    }
}

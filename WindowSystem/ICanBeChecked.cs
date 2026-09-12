using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowSystem
{
   
    /// <summary>
    /// SwitchCheckedStateOnClick - this setting determines if the button can go from checked to not checked by clicking on it.
    /// for radiobuttons, we typically don't want this.
    /// </summary>
    public enum CheckedModes { CannotBeChecked, SwitchCheckedStateOnClick, CanBeChecked }

    public interface ICanBeChecked
    {
        
        bool IsChecked { get; set; }
        event ClickHandler Click;


        CheckedModes CheckedMode
        {
            get; set;           
        }
    }
}

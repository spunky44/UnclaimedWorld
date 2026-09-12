using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger
{
    public abstract class LedgerSheet: UIComponent
    {
        public abstract string DisplayName { get; }
        public abstract string Tooltip { get; }


        public abstract bool ShowRangeSelector { get; }

        public DateAndTime.TimeDateYear FromDate { set; get; }


        protected string CreateTooltip(string header, string blob)
        {
            return Common.ComposeHeadingAndBlobText(header, blob, true);

        }


        public LedgerSheet(GUIManager gui, int width, int height): base(gui)
        {
            base.Width = width;
            base.Height = height;
        }


        /// <summary>
        /// refresh the panel info
        /// </summary>
        public virtual void RefreshData() { }


      /*  public virtual void LoadUserSettings<T>(SheetSettings<T> settings) where T : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
        {
            


        }*/

    }
}

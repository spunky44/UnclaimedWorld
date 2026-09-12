using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    /// <summary>
    /// each tool has its own instance, may also be serialized?
    /// </summary>
    public class AlphaSetting : Setting
    {
        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Value
        {
            get;
            set;
        }


        public AlphaSetting(float defaultValue)
        {
            this.Value = defaultValue;
        }

    }
}

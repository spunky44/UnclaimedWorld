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
    public class RadiusSetting : Setting
    {
        /// <summary>
        /// radius in world coords
        /// </summary>
        public float Value
        {
            get;
            set;
        }


        public RadiusSetting(float defaultValue)
        {
            this.Value = defaultValue;
        }

    }
}

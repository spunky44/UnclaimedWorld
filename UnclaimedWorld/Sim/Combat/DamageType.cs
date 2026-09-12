using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Combat
{
    /// <summary>
    /// an empty class... only the key matters.
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class DamageType: IGameData
    {
        
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get; set;
        }

        public bool DeleteRecord
        {
            get; set;
        }

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

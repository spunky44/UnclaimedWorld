using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide;

namespace UWGame.ClientSide.Hints
{
    /// <summary>
    /// these are needed before loading starts. So we cannot add/update/delete in scenarios and mods. So it does not make sense to let them be IGameData.
    /// </summary>
    public class Hint //: IGameData
    {
        public string KeyName { get; set; }
       // public string Name { get; set; }

        public string Text;

        /// <summary>
        /// higher numbers = higher priority
        /// </summary>
        public int Priority;

      /*  public bool DeleteRecord { get; set; }


        public void PreInitValidate(ref List<string> errors) { }



        public void Initialize() { }



        public void PostInitValidate(ref List<string> errors) { }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize() { }
        public void PostDataCompleteValidate(ref List<string> listOfErrors) { }*/
    }
}

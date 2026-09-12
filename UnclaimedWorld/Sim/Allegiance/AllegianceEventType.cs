using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.Allegiances
{

    /// <summary>
    /// represents the event actions that will be executed for a given AllegianceEvent
    /// </summary>
    public class AllegianceEventType: IGameData
    {
        public AllegianceEvents Event;
        public ActionSets ActionSets;
        


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
            get;
            set;
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

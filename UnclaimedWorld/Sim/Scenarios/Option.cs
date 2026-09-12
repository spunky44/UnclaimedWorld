using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Scenarios
{
    public class Option //: IGameData
    {
       
        /// <summary>
        /// referenced from MainDifficulty
        /// 
        /// </summary>
        public string KeyName { get; set; }
             

        /// <summary>
        /// the name that the player sees in the drop down when he does not want random, like "South coast", "Scientists", "Soldiers"
        /// Only gets displayed if there is more than one option
        /// </summary>
        public string Name { get; set; }

       /* public bool DeleteRecord
        {
            get;
            set;
        }*/


        public string ShortDescription;

        /// <summary>
        /// this describes the difficulty and score modifier of this option.
        /// </summary>
        public CustomDifficulty Difficulty;


        public enum LoadingDialogTextModes { Override, Append }

        /// <summary>
        /// the option can change the loading dialog text in two ways - override or append
        /// </summary>
        public LoadingDialogTextModes LoadingDialogTextMode;
        public string LoadingDialogText;
        public int? LoadingDialogTextOrder;

        /// <summary>
        /// actions to perform immediately - can be combined with keys
        /// </summary>
      //  public EventActionType[] Actions;

        /// <summary>
        /// actions to perform immediately 
        /// keys are for shared actions...
        /// </summary>
        public string[] ActionKeys;

        
        
        //    public string ActionSetsKey;


        /// <summary>
        /// global events to register
        /// </summary>
        public string[] ConditionalEvents;


        public void Initialize()
        {
            

        }


        public List<EventActionType> GetStartActions()
        {
            List<EventActionType> actions = new List<EventActionType>();
            
            if (ActionKeys != null)
            {
                // look up keys and append to list of actions:   
                foreach (var item in ActionKeys)
                {
                    EventActionType action = GameData.Instance.AllEventActionTypes[item];

                    actions.Add(action);
                }
            }

            return actions;
        }

        public List<PolledEventType> GetEvents()
        {
            List<PolledEventType> events = new List<PolledEventType>();

            if (this.ConditionalEvents != null)
            {
                // look up keys and append to list of actions:   
                foreach (var item in ConditionalEvents)
                {
                    PolledEventType evt = GameData.Instance.AllPolledEvents[item];


                    events.Add(evt);
                }
            }

            return events;
        }

      /*  public void PreInitValidate(List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));

        }

        public void PostInitValidate(List<string> errors)
        {
        }*/
    }
}

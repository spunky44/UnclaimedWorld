using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using System.Diagnostics;

namespace UWGame.SimSide.InGameEvents.Actions
{
  

    /// <summary>
    /// has a corresponding runtime class called ActionSetData that holds status variables
    /// 
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class ActionSetType: IGameData 
    {
        /// <summary>
        /// place the description here, then it will be preserved for player modding when serialized to xml.
        /// </summary>
        public string Comments;


        /// <summary>
        /// if filled, it sets the chance that this event will be valid for firing.
        /// 0 - 1
        /// 
        /// </summary>
        public float? ChanceToFire;

        /// <summary>
        /// if filled, it sets a max no. of times this event will be allowed to fire in a game.
        /// </summary>
        public int? MaxFirings;

        /// <summary>
        /// if filled, gives the edge for this action to be selected among the others by a random throw 0-1 
        /// </summary>
      //  public float? RandomChanceEdge;

        /// <summary>
        /// 
        /// </summary>
        public Condition Condition;
    
        /// <summary>
        /// the events will only fire if the conditions are true
        /// </summary>
        public EventActionType[] Actions;


       

        /// <summary>
        /// used to determine when the conversation has ended
        /// </summary>
        [XmlIgnore]
        public int NoOfTalkActions
        {
            get;
            private set;
        }


        /// <summary>
        /// this will be auto-generated and written to xml since all the instances are nested...
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

        public ActionSetType(string keyName)
        {
            this.KeyName = keyName;
        }

        public ActionSetType()
        {
           // auto-generate a keyname for use in snapshotting.
           // KeyName = GameData.CreateKeyName();

          //  if (!Snapshotter.IsSnapshotting) // done both when loading from xml and when instantiating directly in GameDataLoaders!
         //   {
                /*
                 * 1. load from xml/instantiate in GameDataLoader - creates IDs
                 * 2. load objects from save file with referencing IDs
                 * 
                 * Same???
                 * This will only work if the sequence is exactly the same, so the IDs will get assigned the same values each time the game data types are loaded. 
                // And of course it will break if there are changes to the data types.
                 * 
                 * // make sure we don't snapshot this collection - it is already stored as a gamedatatype
                 * 
                 */
                             

            //    AddToLookup(); // this also sets a flag not to snapshot the collection.
           //     
          //  }
        }

        public void Initialize()
        {
           

            if (Actions != null)
            {
                for (int i = 0; i < Actions.Length; i++)
			    {
                    var item = Actions[i];
			 
                    item.Initialize();

                    TalkAction talkAction = item as TalkAction;

                    if (talkAction != null)
                    {
                        talkAction.SetLineNo(NoOfTalkActions);
                        NoOfTalkActions++;
                    }
                }
            }

        }

        public void PostInitValidate(ref List<string> errors)
        {
            if (Actions != null)
            {
                TalkAction.TalkActionPriority? priority = null;
                foreach (var item in Actions)
                {
                    item.PostInitValidate(ref errors);

                    TalkAction talkAction = item as TalkAction;

                    if (talkAction != null)
                    {
                        if (priority.HasValue)
                        {
                            if (priority != talkAction.TalkPriority)
                            {
                                EntityType.CreateValidationError(ref errors, "All talk actions in an ActionSet must have the same priority.");
                            }
                        }
                        else
                        {
                            priority = talkAction.TalkPriority;
                        }
                    }
                }
            }
        }

        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (Actions != null)
            {
                foreach (var item in Actions)
                {
                    item.PostDataCompleteValidate(ref listOfErrors);
                }
            }
        }


        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
        {
            if (GameData.Instance.AllActionSetTypes.ContainsKey(KeyName)) 
            {
                Common.AddToList(ref duplicateKeyErrors, "Duplicate key: " + KeyName);
            }
            else //if (!GameData.Instance.AllActionSetTypes.ContainsKey(KeyName))
            {
                GameData.Instance.AllActionSetTypes.Add(KeyName, this); // add to dict so we can look it up when snapshotting
            }

            foreach (var item3 in Actions)
            {
                // WinGame actions can define nested actions:
                item3.ExtractNestedActionTypes(ref duplicateKeyErrors);
            }

        }

        public void LoadContent(ContentManager content)
        {
            if (Actions != null)
            {
                foreach (var item in Actions)
                {
                    item.LoadContent(content);
                }
            }
        }

        public void PostLoadContentValidate(ref List<string> listOfErrors)
        {
            foreach (var item in Actions)
            {
                item.PostLoadContentValidate(ref listOfErrors);
            }

        }

        public bool ConditionsAreFulfilled(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (ChanceToFire.HasValue)
            {
                if (The.Sim.GameplayRandomGenerator.NextDouble("") > ChanceToFire.Value)
                    return false;
            }
                       

            if (Condition != null)
            {

                return Condition.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);               
            }

            return true;
        }




        public void PreInitValidate(ref List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));

            if (Condition != null)
            {
                Condition.PreInitValidate(ref errors);
            }

            foreach (var item in Actions)
            {
               
                item.PreInitValidate(ref errors);
            }
        }

      
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.SimEffects
{
   

    /// <summary>
    /// contained in: FoodType, ProcessType, AttackType... the creator of the effect can scale it by consumed bulk etc.
    /// 
    /// A named group of effects, for symptoms or states, like Drunk (affects Comfort and Productivity)...  
    /// to be displayd in a side panel.
    /// After the last effect expires, the profile is removed
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class EffectProfileType: IGameData
    {
        public string KeyName { get; set; }
        public string Name { get; set; }

        public string Description;

        public string Comments;

        public AIDesirability AIDesirability;

        /// <summary>
        /// allow the effects to have different durations.
        /// </summary>
        public string[] Effects;


        public int SortOrder;

        [XmlIgnore]
        public List<EffectType> EffectTypes;



        [XmlIgnore]
        public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();
       


        public bool DeleteRecord
        {
            get;
            set;
        }

        public bool Affects(SimEffects.AffectsNumbers affects)
        {
            if (EffectTypes != null)
            {
                if (EffectTypes.Any(e => e is NumberEffectType && ((NumberEffectType)e).Affects == affects))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Affects(SimEffects.AffectsFlags affects)
        {
            if (EffectTypes != null)
            {
                if (EffectTypes.Any(e => e is FlagEffectType && ((FlagEffectType)e).Affects == affects))
                {
                    return true;
                }
            }

            return false;
        }

       /* public bool SatisfiesComfort()
        {       
            if (EffectTypes != null)
            {                
                if (EffectTypes.Any(e => e.Affects == SimEffects.Affects.AgentComfort || e.Affects == SimEffects.Affects.OfferedComfort))
                {
                    return true;
                }     
            }

            return false;
        }*/


        public void Initialize()
        {
            EffectTypes = new List<EffectType>();
            foreach (var item in Effects)
            {
                EffectTypes.Add(GameData.Instance.AllEffectTypes[item]);
            }

        }


        public void PostLoadContentInitialize()
        {

            List<EffectTypeActionHook> eventActions;
            if (GameData.Instance.EventHooksByEffectType.TryGetValue(this, out eventActions))
            {
                // group actions by hook type:
                foreach (var item in eventActions)
                {
                    List<ActionSets> actions = null;

                    if (!this.EventActions.TryGetValue(item.Hook, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.EventActions.Add(item.Hook, actions);
                    }

                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);

                }
            }
        }

        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors)
        { }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

    }
}

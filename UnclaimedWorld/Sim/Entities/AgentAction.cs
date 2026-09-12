using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.Client.Particles;
using UWGame.SimSide.Systems.Triggers;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// some action that an agent can perform...
    /// should probably be replaced by ProcessType.
    /// </summary>
  /*  public class AgentAction : IGameData
    {
        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public string StatusDescription;

        /// <summary>
        /// fill in only one of these two
        /// </summary>
        public float? TimeInDaysRequired;
        public float? TimeInRealSeconds;

        /// <summary>
        /// Scale the anims so they play the whole thing by modifiying their speed(!).
        /// default is True!
        /// 
        /// WARNING:
        /// if True - ALL matching anims must have runtime near the wait time, at least for character anims. otherwise, a sped up horror movie effect can occur.
        /// 
        /// Cannot be combined with looping either.
        /// </summary>
        public bool ScaleAnimDuration = true;

        public AnimAction AgentActionState = AnimAction.Mending;

        /// <summary>
        /// don't put stances here!! Use Stances instead
        /// </summary>
        public AnimModifier[] AgentAnimationStates;

        [XmlIgnore]
        public List<AnimModifier> AgentAnimationStatesList;



        /// <summary>
        /// StancesType key  => list of StanceTypes
        /// The possible stances. null means only 'default' = Standing is possible
        /// </summary>
        public SerializableDictionary<string, ChanceToTakeStance[]> Stances;
        // public LeggedLocomotor.Stance[] Stances;

        [XmlIgnore]
        public Dictionary<StancesType, List<ChanceToTakeStance>> StanceTypes; 


        public ParticleEmitterEffect[] ParticleEmittersWhenCompleted;

        /// <summary>
        /// triggers that are active during the action
        /// </summary>
        public TriggerType[] Triggers;


        public AgentAction() { }

        public AgentAction(string key)
        {
            KeyName = key;
        }

        public void PreInitValidate(ref List<string> listOfErrors)
        { }

        public void PostInitValidate(ref List<string> listOfErrors)
        { }

        public void Initialize()
        {
            if (AgentAnimationStates != null)
            {
                AgentAnimationStatesList = AgentAnimationStates.ToList();
            }

            if (ParticleEmittersWhenCompleted != null)
            {
                foreach (var item in ParticleEmittersWhenCompleted)
                {
                    item.Initialize();

                }
            }

            ProcessType.InitializeStances(Stances, out StanceTypes);
          
        }

        public float GetTimeInRealSeconds()
        {
            if (TimeInRealSeconds.HasValue)
            {
                return TimeInRealSeconds.Value;
            }
            else
            {
                return (float)(TimeInDaysRequired.Value / The.Sim.DateAndTime.DaysPerSecond);   
            }
        }

        public float GetTimeInDays()
        {
            if (TimeInDaysRequired.HasValue)
            {
                return TimeInDaysRequired.Value;
            }
            else
            {
                return (float)(TimeInRealSeconds.Value * The.Sim.DateAndTime.DaysPerSecond);
            }
        }
    }*/
}

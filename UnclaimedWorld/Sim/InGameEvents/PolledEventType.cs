using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.IngameEvents;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.InGameEvents
{
    /// <summary>
    /// type class for polled conditional events
    /// These can be global, or can be attached to an Entity, Expedition or Allegiance
    /// </summary>
    public class PolledEventType: IGameData, IXmlSerializable
    {
        public string Comment;

        public string KeyName { get; set; }
        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public bool PlaySiteOnly = true;
     
       
        public Condition Condition;


        public const double DefaultPollInterval = 4d;


        public bool UseDefaultPollInterval = false;


        /// <summary>       
        /// if not specified, the PolledEvent should destroy after the first update
        /// </summary>
        public EvalNode PollInterval;

        /// <summary>
        /// If true, adds a random time interval to PollInterval to stagger updates to prevent load spikes.
        /// this is set false by default.
        /// </summary>
        public bool AllowRandomTimeOffset = false;


       

        /*public class FirstPoll
        {*/
            
        /// <summary>
        /// the time point for when the polled event should update for the first time.
        /// 
        /// If not specified, the event will poll at time = 0
        /// </summary>
        public TimePoint StartTimePoint;


        public bool StartAfterInterval = false;

       // }


       /// <summary>
       /// fill in at most one of the below
       /// </summary>
        public ActionSets ActionSets;

        public string ActionSetsKey;



        public void PreInitValidate(ref List<string> errors) 
        {
            if (Condition != null)
            {
                Condition.PreInitValidate(ref errors);
            }

            if (ActionSets != null)
            {
                ActionSets.PreInitValidate(ref errors);
            }        
        }

        public void PostDataCompleteInitialize()
        {
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

        public void Initialize() 
        {
            if (Condition != null)
            {
                Condition.Initialize();
            }
       
            if (ActionSets != null)
            {
                ActionSets.Initialize();               
            }

            if (ActionSetsKey != null)
            {
                ActionSets = GameData.Instance.AllActionSets[ActionSetsKey];
            }
        }

        public void PostInitValidate(ref List<string> errors) 
        {
            if (ActionSets != null)
            {
                ActionSets.PostInitValidate(ref errors);
            }
        }

        /*
        public double? GetUpdateInterval()
        {

            double? tempInterval = null, currentInterval = null;

            if (Function != null)
            {
                tempInterval = Function.Left.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = Function.Right.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }
            else if (Value != null)
            {
                tempInterval = Value.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }
            else
            {
                // farm plot..
                tempInterval = GetCurrentInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }

            if (Common.IsZero(currentInterval) || Common.IsEqual(currentInterval, DefaultPollInterval)) // don't update each frame, use the default poll interval as minimum
            {
                currentInterval = GetCurrentInterval();
            }


            return currentInterval;
        }*/


        public double? GetPollIntervalUpdateInterval()
        {
            if (PollInterval != null)
            {
                PropertyResult? result = PollInterval.Evaluate(null, null, null, null);
                if (result != null)
                {
                    if (result.Value.NumberResult.HasValue)
                    {
                        return result.Value.NumberResult.Value;
                    }

                }
            }

            return null;

           // return DefaultPollInterval;
        }

        /// <summary>
        /// triggering entity is an agent but can be null!
        /// </summary>
        /// <param name="triggeringEntity"></param>
        /// <param name="laterFiringWasAdded"></param>
        public void Fire(Entity triggeringEntity, IHasExposedProperties polledEventSource, out bool isExpired)
        {
          
            ActionSets.Fire(triggeringEntity, null, polledEventSource, out isExpired); 
        }


        public void LoadContent(ContentManager content)
        {
            if (ActionSets != null)
            {
                ActionSets.LoadContent(content);                               
            }
        }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(PolledEventType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };



        #endregion
    }
}

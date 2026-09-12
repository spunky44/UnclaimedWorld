using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Systems.Triggers
{
    [DebuggerDisplay("{KeyName}")]
    public class TriggerType : IGameData, IXmlSerializable
    {        
        public string KeyName { get; set; }
        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        
        /// <summary>
        /// either circular area
        /// </summary>
        public float? Range;

        /// <summary>
        /// or rectangle area
        /// </summary>
        public Vector2? AreaDimensions;


        public float? LifetimeInSeconds;

        public string ActionSetsKey;

        public double DurationBetweenTriggerUpdatesInSeconds;

     //   [XmlIgnore]
       // public long DurationInTicksBetweenTriggerUpdates;


        public TriggerPriority Priority;

        public bool IsPrey;

     
        public bool IsDrivenVehicle;

        public bool IsEntityDied;

        public int? MaxTimesToTriggerBeforeExpiring;

        public float? CooldownInSeconds;

        [XmlIgnore]
        public long? CooldownInTicks;

      /*  public string DamageToTriggeringEntitiesKey;

        [XmlIgnore]
        public AttackType DamageToTriggeringEntities;
        */

        public Interest Interest;

        /// <summary>
        /// default is false.
        /// set this to true if this trigger should fire even if the trigger entity is unseen by the entity that gets within range
        /// </summary>
        public bool CanTriggerWhenUndetected = false;

        public void PreInitValidate(ref List<string> errors)
        {
            
        }

        public void Initialize()
        {          
            if (CooldownInSeconds.HasValue)
            {
                CooldownInTicks = TimeSpan.FromSeconds(CooldownInSeconds.Value).Ticks;
            }

           

            if (Interest != null)
                Interest.Initialize();
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


        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(TriggerType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()            
        };


        #endregion
    }
}

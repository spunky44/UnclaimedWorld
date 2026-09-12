using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    public enum Scope
    {
        Entity, Expedition, Allegiance
    }

    /// <summary>
    /// Same as for event hooks, I made the connection into a class to make it easier for mods to update/add/delete them without interfering with the rest of the EntityType
    /// </summary>
    public class EntityTypePolledEvent : IGameData, IHook
    {
        
        /// <summary>
        /// EntityType key
        /// </summary>
        [XmlElement(ElementName = "EntityTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        /// <summary>
        /// should the polled event run on the entity, expedition or allegiance level (for the last two, only Representative entity type events will be used)
        /// </summary>
        public Scope Scope;

        /// <summary>
        /// refers to PolledEvents
        /// </summary>
        public string PolledEventKey;



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

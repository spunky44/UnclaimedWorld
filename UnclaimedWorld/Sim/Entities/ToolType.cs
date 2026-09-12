using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Renderables;
using System.Xml.Serialization;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// NOTE: Immobile tools are also stationary!
    /// </summary>
    public enum ToolHandlingType { Intrinsic, HandTool, Stationary }


    public class ToolType // : IXmlSerializable
    {
        public string[] ToolTag;

       // private EntityType parent;

       
        /// <summary>
        /// Value between 0 and 1
        /// 1: indestructible
        /// 0: breaks quickly when used
        /// </summary>
        public float? Durability; // = 1f;

        public ToolHandlingType? ToolHandling;

       /* public bool? IsHandTool;

        public bool? IsIntrinsic;*/

        /// <summary>
        /// an optional process that will end with setting the Prepared property on the tool. Tools and inputs are not supported.
        /// </summary>
        public string PrepareProcess;

        [XmlIgnore]
        public ProcessType PrepareProcessType;

        /// <summary>
        /// move to Sprite state. add a flag Prepared(Active?)
        /// </summary>
      //  public ParticleEmitterEffect[] ParticleEmittersWhenActive;

        /// <summary>
        /// true, if a tool for feedback only
        /// </summary>
        public bool IsPseudoTool;

        public ToolType()
        { }

        

        public static bool IsImmovable(EntityType entityType)
        {
            if (entityType.StructureType != null || entityType.Upgrader != null) 
            {
                return true;
            }

            return false;
        }


        public void PostLoadContentInitialize()
        {
            if (PrepareProcess != null)
            {
                PrepareProcessType = GameData.Instance.AllProcessTypes[PrepareProcess];
                PrepareProcessType.SetPreparedProperty = true;
            }
        }


        public void PreInitValidate(ref List<string> listOfErrors)
        {
            if (IsPseudoTool == false)
            {
                EntityType.ValidateRequiredValue(ref listOfErrors, "Durability", Durability.HasValue);
                EntityType.ValidateRequiredValue(ref listOfErrors, "ToolHandling", ToolHandling.HasValue);
            }
        }

        public override string ToString()
        {
            return System.String.Join(",", ToolTag); //.Joi. Name + "(" + KeyName + ")";
        }

/*
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ToolType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };

        #endregion*/

    }


   
}

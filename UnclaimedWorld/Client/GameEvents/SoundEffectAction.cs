using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents
{
    public class SoundEffectAction: EventActionType, IXmlSerializable, IGameData
    {      
        public string Sound;
        
        private SoundData soundEffect;

       /* [XmlIgnore]
        public SoundEffect SoundEffect
        {
            get
            {
                return soundEffect;
            }
        }*/

        public WorldLocation? Location;

         public SoundEffectAction(string keyName): base(keyName)
        {

        }

         public SoundEffectAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute()
        {
            if (Location.HasValue)
            {
                The.Client.AudioManager.PlayWorldSound(soundEffect, Location.Value);
            }
            else
            {
                The.Client.AudioManager.PlaySound(soundEffect /*soundEffect.SoundEffect, soundEffect.Volume*/, 1f);
            }

            return true;
        }

       

        public override void Initialize()
        {
            base.Initialize();

            soundEffect = GameData.Instance.AllSoundData[Sound];
        }

        public override void PostInitValidate(ref List<string> listOfErrors)
        {
            base.PostInitValidate(ref listOfErrors);

            if (soundEffect == null)
            {
                EntityType.CreateValidationError(ref listOfErrors,
                                string.Format("soundEffect was not filled out!", KeyName));
            }
        }
      /*  public void LoadContent(ContentManager content)
        {
            this.soundEffect = content.Load<SoundEffect>(Sound);
            
        }*/


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


        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SoundEffectAction))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true) 

        };


        #endregion
    }
}

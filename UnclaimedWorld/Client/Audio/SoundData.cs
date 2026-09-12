using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace UWGame.Client.Audio
{
    [DebuggerDisplay("{KeyName}")]
    public class SoundData: IGameData
    {
        public string Sound;
        public float Volume;

        /// <summary>
        /// Pitch should have the value range from -1.0f (down one octave) to 1.0f (up one octave)
        /// to make a random pitch change of -0.1 - 0.1, use Mean: 0, StandardDeviation: 0.03
        /// </summary>
        public NormalDistribution RandomPitchChange;

        /// <summary>
        /// most ambient sounds should have this set to True to avoid a dizzying 'phasing' effect when multiple instances are played at the same time
        /// </summary>
        public bool PlayMaxOneInstance = false;


        public string KeyName
        {
            get;
            set;
        }

        /// <summary>
        /// never used...
        /// </summary>
        public string Name
        {
            get; set;
        }

        public bool DeleteRecord
        {
            get;
            set;
        }


        [XmlIgnore]
        public SoundEffect SoundEffect;

        public void LoadContent(ContentManager content)
        {
            SoundEffect = content.Load<SoundEffect>("Sounds\\" + Sound);
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

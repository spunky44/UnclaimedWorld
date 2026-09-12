using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using UWGame.SimSide;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents
{
    public class MusicAction : EventActionType //, IGameData
    {      
        public string Song;
        
        private Song song;

        public bool? StopMusic;

        public bool Loop = false;

          public MusicAction(string keyName): base(keyName)
        {

        }

          public MusicAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public override void Execute(EventAction action, ref string failReason)
        {
            if (StopMusic == true)
            {
                The.Client.Controller.AudioManager.StopSong();
            }
            else
            {
                The.Client.Controller.AudioManager.PlaySong(song, Loop);
            }

            return true;
        }


        public override void LoadContent(ContentManager content)
        {
            if (Song != null)
            {
                this.song = content.Load<Song>("Music\\" + Song);
            }
            
        }
    

        public override void PostInitValidate(ref List<string> listOfErrors)
        {
            if (Song == null && StopMusic != true)
            {
                EntityType.CreateValidationError(ref listOfErrors,
                                string.Format("Song was not filled out!", KeyName));
            }
        }

      
    }
}

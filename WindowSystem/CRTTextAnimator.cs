using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace WindowSystem
{
    public abstract class CRTTextAnimator : UIComponent
    {
        
        
        public float TimeBetweenUpdates = 0.02f; //0.03f; //0.1f;// 0.10f;
        protected double timePassed = 0f;

        public bool IsStarted
        {
            get { return isStarted; }
        }

        protected bool isStarted = false;

        protected int currentIndex = 0;


        protected static SoundEffectInstance sound;
        protected static int noOfSoundPlays = 0;


        public CRTTextAnimator(GUIManager gui): base(gui)
        {


        }

        public abstract void Add(Label label);


        public abstract void Clear();
        

        public override int Add(UIComponent control)
        {
            throw (new Exception("Can only add labels..."));
        }

        public abstract void StartAnimating();


        protected void Stop()
        {
            // all finished.
            isStarted = false;

            // these static variables are shared among multiple controls!
            noOfSoundPlays--;
            if (noOfSoundPlays <= 0)
            {
                if (sound != null)
                {
                    sound.Stop(true);
                }

                noOfSoundPlays = 0;
            }
        }

        protected void StartIt()
        {
            isStarted = true;
            timePassed = 0;
            currentIndex = 0;

            //GUIManager.Typing.Play()

            noOfSoundPlays++;

            /*
            if (sound == null || sound.State == SoundState.Stopped)
            {
                sound = GUIManager.Typing.CreateInstance();
                sound.IsLooped = true;
               // sound.Volume = GUIManager.MasterSFXVolume;
                sound.Play();
                
            }*/
        }

        

    }
}

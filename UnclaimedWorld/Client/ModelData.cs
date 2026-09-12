using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using GameStateManagement;
using Xclna.Xna.Animation;
using UWGame.Client.Audio;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide
{
    public enum ModelType { Skinned, Stiff }
    public class ModelData
    {
        // this data gets copied over into RenderAsModelType...

        public Model Model;
        public float CRTDisplayScale = 1;
        public float CRTDisplayLightIntensity = 1f;

        /// <summary>
        /// Don't use this! Use the scaled version in RenderAsModel instead!
        /// </summary>
        public float BoundingSphereRadius;

        public ModelType ModelType;
        public Vector3 ModelOffset = new Vector3(0f, 0f, 0f);


        public AttachPoint LeftHandAttachor;
        public AttachPoint RightHandAttachor;
        public AttachPoint BackAttachor;
        public AttachPoint HelmetAttachor;

        // for vehicles
        public AttachPoint DriverAttachor;

        public List<AttachPoint> PassengerAttachors;

        public AttachPoint CargoAttachor;

        // for items:
        public AttachPoint BackAttachee;
        public AttachPoint RightHandAttachee;
        public AttachPoint LeftHandAttachee;
        public AttachPoint BottomAttachee; // for boxes. 

        //******
       
      /*  public Vector3? DriversSeatTranslation; 
        public string DriverAttachBoneName;

        public Vector3? Passenger1SeatTranslation; 
        public string Passenger1AttachBoneName;

        public Vector3? Passenger2SeatTranslation; 
        public string Passenger2AttachBoneName;

        public Vector3? Passenger3SeatTranslation; 
        public string Passenger3AttachBoneName;*/

        public bool HasAircraftDucts = false;

        public bool HasSteerableFrontWheels = false;

        public bool HasEmittingParts = false;

      //  public bool HasRunAnimation = false;

    //    public bool HasIdleLookAnim = false;

     //   public AnimationSet WalkAnimations;

    //    public AnimationSet RunAnimations;

    //    public RandomAnimationSet IdleAnimations;

   //     public RandomAnimationSet Eat;


      /*  public string Passenger1Animation;
        public string Passenger2Animation;
        public string Passenger3Animation;
        public string Passenger4Animation;*/

        public AttachPoint GetAttachPointFromKeyName(string keyName)
        {
            if (CargoAttachor != null && keyName == CargoAttachor.KeyName)
            {
                return CargoAttachor;
            }

            if (DriverAttachor != null && keyName == DriverAttachor.KeyName)
            {
                return DriverAttachor;
            }

            foreach (AttachPoint attachPoint in PassengerAttachors)
            {
                if (attachPoint.KeyName == keyName)
                {
                    return attachPoint;
                }
            }

            return null;
        }

        public AttachPoint GetAttachPointFromTag(string tag)
        {
            if (RightHandAttachor != null && tag == RightHandAttachor.Tag)
            {
                return RightHandAttachor;
            }

            if (LeftHandAttachor != null && tag == LeftHandAttachor.Tag)
            {
                return LeftHandAttachor;
            }

            if (BackAttachor != null && tag == BackAttachor.Tag)
            {
                return BackAttachor;
            }

            if (HelmetAttachor != null && tag == HelmetAttachor.Tag)
            {
                return HelmetAttachor;
            }

            if (CargoAttachor != null && tag == CargoAttachor.Tag)
            {
                return CargoAttachor;
            }

            if (CargoAttachor != null && tag == CargoAttachor.Tag)
            {
                return CargoAttachor;
            }

            if (DriverAttachor != null && tag == DriverAttachor.Tag)
            {
                return DriverAttachor;
            }

            foreach (AttachPoint attachPoint in PassengerAttachors)
            {
                if (attachPoint.Tag == tag)
                {
                    return attachPoint;
                }
            }

            return null;
        }


        public AttachPoint GetAttacheePoint(AttacheePoint? attacheePointName)
        {
            if (attacheePointName == AttacheePoint.Back)
            {
                return BackAttachee;
            }
            else if (attacheePointName == AttacheePoint.RightHand)
            {
                return RightHandAttachee;
            }
            else if (attacheePointName == AttacheePoint.Bottom)
            {
                return BottomAttachee;
            }
            else if (attacheePointName == AttacheePoint.LeftHand)
            {
                return LeftHandAttachee;
            }
            else
            {
                return RightHandAttachee; // not used
            }       

        }
       /* public AttachPoint GetPassengerAttachPoint(Place place)
        {
            int? index = null;
            switch (place)
            {
                case Place.Passenger1:
                    index = 0;
                    break;
                case Place.Passenger2:
                    index = 1;
                    break;
                case Place.Passenger3:
                    index = 2;
                    break;
                case Place.Passenger4:
                    index = 3;
                    break;

            }

            if (index.HasValue && index < PassengerAttachPoints.Count)
            {
                return PassengerAttachPoints[index.Value];
            }
            else return null;
        }*/

        /*
        public string GetPassengerAnimation(Place place)
        {
            switch (place)
            {
                case Place.Passenger1:
                    return Passenger1Animation;
                case Place.Passenger2:
                    return Passenger2Animation;
                case Place.Passenger3:
                    return Passenger3Animation;
                case Place.Passenger4:
                    return Passenger4Animation;

            }

            return null;
        }*/

       // public string CargoAttachBone1;
    }

    /// <summary>
    /// enables synchronized playback of random sounds and animations
    /// </summary>
    public class RandomSoundAndAnimationSet
    {
        /// <summary>
        /// a random one of these will be played.
        /// </summary>
        public string[] BaseAnimations;
    
        public string[] AdditionalAnimations1;
        public string[] AdditionalAnimations2;
                
               
        /// <summary>
        /// fill these with an equal number of entries in order to start sounds and animations in sync
        /// </summary>
        public string[] Sounds;


        [XmlIgnore]
        public SoundData[] SoundData
        {
            get;
            private set;
        }


        public void Initialize()
        {
            if (Sounds != null)
            {
                SoundData = new SoundData[Sounds.Length];

                for (int i = 0; i < Sounds.Length; i++)
                {
                    string sound = Sounds[i];
                    if (!string.IsNullOrEmpty(sound))
                    {
                        SoundData[i] = GameData.Instance.AllSoundData[sound];
                    }
                    else
                    {
                        SoundData[i] = (SoundData)null;
                    }
                }

            }
        }

        public void PostLoadContentValidate(ref List<string> listOfErrors) // RenderableType parent)
        {
            if (BaseAnimations != null && SoundData != null
                && BaseAnimations.Length != SoundData.Length)
            {
                EntityType.CreateValidationError(ref listOfErrors, "There must be the same number of animation and sound entries.");
            }
        }
    }

        
}

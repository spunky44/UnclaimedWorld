using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Xclna.Xna.Animation;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Renderables
{
    [DebuggerDisplay("Conditions:{ConditionSet} Forbiddens:{Forbiddens.StateNames}")]
    public class AnimConditionInfo 
    {
       
        /// <summary>
        /// TODO: XML serialize these as an array/sequence of bits (binary?)
        /// </summary>
        public AnimConditions ConditionSet;
     
        /// <summary>
        /// the anim will never be chosen when the entity has one of these states
        /// </summary>
        public BitMask64 Forbiddens;

     
        public RandomSoundAndAnimationSet SoundAndAnimationSet;         


        // perhaps move these to each anim key, to give even higher fidelity...
        public Playback Playback;
        public StartingPoint StartingPoint;
        public float? StartingPointInSeconds;
        public BlendMode BlendMode;

        /// <summary>
        /// controls looping of sounds as well as anims - Default is Yes!!!
        /// </summary>
        public Looping Looping;
        public float SpeedFactor;


        /// <summary>
        /// the gait to use in this state - it is possible to combine this with additional anims, but not main/base anims.
        /// </summary>
        public string GaitSetKey;



        /// <summary>
        /// attachee point and item can be null
        /// </summary>
        public AttachPointData[] AttachPoints;

        

        /// <summary>
        /// particles associated with the anim
        /// </summary>
        public ParticleEmitterEffect[] ParticleEmitters;


        /// <summary>
        /// these renderables will disappear after the anim ends
        /// </summary>
        public TemporaryAttachable[] TemporaryRenderablesToAttach;
        
        /*
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public SoundData SoundData
        {
            get;
            private set;
        }


        /// <summary>
        /// TODO: deprecate this - move to RandomSoundAndAnimationSet
        /// </summary>
        public string Sound;
        */
       


        public AnimConditionInfo()
        {           
          //  AnimationSet = new RandomAnimationSet() { BaseAnimations = new string[] { "idle" } }; //default to just idle as safeguard

            // default values...

            Playback = Playback.Forwards;
            StartingPoint = StartingPoint.FromBeginning; 
            BlendMode = BlendMode.Normal;
            Looping = Looping.Yes;
            SpeedFactor = 1f;

        }

        public bool Equals(AnimConditionInfo other) 
        {           
            if (other != null)
            {
                bool conditionsAreEqual = false;

                if (ConditionSet != null && other.ConditionSet != null)
                {
                    conditionsAreEqual = ConditionSet.Equals(other.ConditionSet); // == other.Action && Modifiers.Equals(other.Modifiers);
                }
                else if (ConditionSet == null && other.ConditionSet == null)
                {
                    conditionsAreEqual = true;
                }

                bool forbiddensAreEqual = false;

                if (Forbiddens != null && other.Forbiddens != null)
                {
                    forbiddensAreEqual = Forbiddens.Equals(other.Forbiddens);
                }
                else if (Forbiddens == null && other.Forbiddens == null)
                {
                    forbiddensAreEqual = true;
                }

                return conditionsAreEqual && forbiddensAreEqual;
            }

            return false;
        }

        public AnimAction? GetAction()
        {
            if (ConditionSet != null)
                return ConditionSet.Action;

            return null;
        }

        public BitMask64 GetModifiers()
        {
            if (ConditionSet != null)
                return ConditionSet.Modifiers;

            return null;
        }

     
        /*
        public class ScriptedEventInfo
        {
            public int frame;
            public string data;
            TriggerType triggerType;

            public enum TriggerType
            {
                OnFrame,		// trigger on frame specified by 'Frame'
                OnStateEnter,	// trigger when entering state
                OnStateExit 	// trigger when leaving state
            };

            public ScriptedEventInfo()
            {
                frame = 0;
                triggerType = TriggerType.OnFrame;
            }
        };*/

    /*    public void ProcessScriptEvents(ref Entity entity, ScriptEventCategory category, int frame = 0)
        {
            //TODO IMPL
            //This will be like the Lua events in RTS
            //A library of scripts for all puposes will be available to modder
            //constructed like RTS ScriptEngine,
            //Semantics: IF [conditions] THEN [actions]
            //Three types of Conditions: booleanConstant(t/f), is,isnot(AnimState), is,isnot(kindof)
            //An assortment of actions, all of which modify the RenderAsModel instance's curAnimInfo,
            //which should be a mutable copy of the readonly member of this class, returned by findBestAnimInfo()

            //these are important points ^
        }*/


        internal void Initialize()
        {
            if (ParticleEmitters != null)
            {
                foreach (var item in ParticleEmitters)
                {
                    item.Initialize();
                }
            }

            
            if (SoundAndAnimationSet != null)
            {
                SoundAndAnimationSet.Initialize();
            }
        }

        public class TemporaryAttachable: IEquatable<TemporaryAttachable>
        {
            /// <summary>
            /// the renderable type (NOT entity type) that this data should be used for
            /// </summary>
            public string RenderableTypeKey;

            public AttacheePoint? AttacheePoint;

            /// <summary>
            /// Defines the body part that the attached object is attached to        
            /// </summary>
            public string AttachorTag;



            public bool Equals(TemporaryAttachable other)
            {
                if (RenderableTypeKey == other.RenderableTypeKey
                    && AttacheePoint == other.AttacheePoint
                    && AttachorTag == other.AttachorTag)
                {
                    return true;
                }

                return false;
            }
        }

        public class AttachPointData
        {
            /// <summary>
            /// only use this transform data when the object is attached using this attachee point! (can be left as null)
            /// for instance, some transforms should only be used when the oject is attached to a hand, not to a back...
            /// </summary>
            public AttacheePoint? AttacheePoint;

           

            /// <summary>
            /// the renderable type (NOT entity type) that this data should be used for
            /// </summary>
            public string RenderableTypeKey;


            /// <summary>
            /// the transforms that should be applied
            /// </summary>
            public Vector3 Translation;

            public Vector3 Rotation;

        }

        public void PostLoadContentValidate(List<string> listOfErrors, string modelName, AnimationInfoCollection animations)
        {
            if (SoundAndAnimationSet != null)
            {
                if (SoundAndAnimationSet.BaseAnimations != null)
                {
                    foreach (var anim in SoundAndAnimationSet.BaseAnimations)
                    {
                        listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, anim, modelName, animations);
                    }

                }

                if (SoundAndAnimationSet.AdditionalAnimations1 != null)
                {
                    foreach (var anim in SoundAndAnimationSet.AdditionalAnimations1)
                    {
                        listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, anim, modelName, animations);
                    }

                }
                if (SoundAndAnimationSet.AdditionalAnimations2 != null)
                {
                    foreach (var anim in SoundAndAnimationSet.AdditionalAnimations2)
                    {
                        listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, anim, modelName, animations);
                    }

                }

                if (SoundAndAnimationSet != null)
                {
                    SoundAndAnimationSet.PostLoadContentValidate(ref listOfErrors);
                }
            }
        }
    }

    public class AnimConditions: ISnapshot
    {
        /// <summary>
        /// Action takes priority when selecting anims. 
        /// if this is null, no matching occurs...
        /// </summary>
        public AnimAction? Action;

        /// <summary>
        /// modifier flags to the Action anim
        /// </summary>
        public BitMask64 Modifiers;


        public AnimConditions()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                Modifiers = new BitMask64(typeof(AnimModifier));
            }
        }

        /// <summary>
        /// copy ctor for memoryfact
        /// </summary>
        /// <param name="original"></param>
        public AnimConditions(AnimConditions original)
        {
            Action = original.Action;
            Modifiers = new BitMask64(original.Modifiers);
        }

        public bool Equals(AnimConditions other) // object obj)
        {
            //ConditionSet other = obj as ConditionSet;
            if (other != null)
            {
                return Action == other.Action && Modifiers.Equals(other.Modifiers);
            }

            return false;
        }

        public override string ToString()
        {
            StringBuilder states = new StringBuilder();
            if (Action.HasValue)
            {
                states.Append(Enum.GetName(typeof(AnimAction), Action.Value));
            }

            states.Append(" - ");

            if (Modifiers != null)
            {
                states.Append(Modifiers.StateNames);
            }

            return states.ToString();
        }

     



        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Action = sn.DoEnumNullable(Action);
            this.Modifiers = (BitMask64)sn.DoISnapshot(Modifiers);
            
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Modifiers.LoadPostProcess(sn);
        }

        #endregion
    }
}

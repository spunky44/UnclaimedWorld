using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using System.Xml.Serialization;
using UWGame.ClientSide.Particles;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using UWGame.Client.Audio;
using UWGame.SimSide.AllGameData;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables
{
   
    public class RenderableType : /*GameObjectType,*/ IXmlSerializable, IGameData // IHasCategory<EntityCategory>
    {

        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public RenderKindOfType RenderKindOfType { get; set; }
      
        public RenderAsIconType RenderAsIconType = null;
      //  public static BitMask64 ActionConditionStates;
       
        /// <summary>
        /// could these be the Default sprites???
        /// </summary>
     /*   public RenderAsBillboardType[] RenderAsBillboardType;
        public RenderAsGroundSpriteType RenderAsGroundSpriteType;
        */

        public RenderAsModelType RenderAsModelType;
        public RenderAsConnectedGroundSpriteType RenderAsConnectedGroundSpriteType;


        /// <summary>
        /// used for selecting sprites, emitters and sounds for non-animating entities
        /// </summary>
        public ClientStateInfo[] ClientStateConditions;

        public ClientStateInfo DefaultClientState;


        /// <summary>
        /// TODO: delete (use Default StaticInfo instead)
        /// </summary>
        public ParticleEmitterType[] ParticleEmitterTypes;

       
     //   public LightingType LightingType;

        public AnimatedHeadType AnimatedHeadType;

        /// <summary>
        ///  since the box attachment runs in the game logic instead of using anim conditions/temporary attachbles, these properties are needed.
        /// </summary>
        public BoxHandlingWhenHauling BoxHandlingWhenHauling;

        



        /// <summary>
        /// if set true, independent renderables will fade out before being removed (important, and default setting for particles)
        /// </summary>
        public bool FadeOutWhenDestroyed = false;

        /* moved to StaticInfo Default
        public string Sound;

        public SoundData AmbientSound
        {
            get; private set;            
        }*/

        // static constructor
        static RenderableType()
        {
         
        }

        public RenderableType()
        {

        }

    
        public RenderableType(string keyName = null) //, string modelName)
        {
            this.KeyName = keyName;

            //kindOfs = new BitMask64(typeof(AnimState)); // for GameObjectType

          //  return RenderableType();
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(RenderableType))
        {
            //TODO RenderableType probably needs to serialize, too
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };



        #endregion



        public void PreInitValidate(ref List<string> listOfErrors)
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {
            if (RenderAsModelType != null && DefaultClientState != null && DefaultClientState.RenderAsBillboardType != null)
            {
                CreateValidationError(ref listOfErrors, "Both the RenderAsModel and RenderAsBillboard types are defined. This is not allowed.");
            }
        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

        public void LoadContent(ContentManager content)
        {
           
            if (RenderAsModelType != null)
            {
                RenderAsModelType.LoadContent(content);
            }

        }

        public void PostLoadContentValidate(ref List<string> listOfErrors, EntityType parent)
        {
            if (RenderAsModelType != null)
            {
                RenderAsModelType.PostLoadContentValidate(ref listOfErrors, parent);

            }

            // let's validate that all sprites are present (in debug only..?):
//#if DEBUG || PROFILE

            if (DefaultClientState != null)
            {
                DefaultClientState.PostLoadContentValidate(parent, ref listOfErrors);
            }

            if (ClientStateConditions != null)
            {
                foreach (var item in ClientStateConditions)
                {
                    item.PostLoadContentValidate(parent, ref listOfErrors);
                }
            }

//#endif


        }

        public static void CreateValidationError(ref List<string> listOfErrors, string errorMessage)
        {
            if (listOfErrors == null)
            {
                listOfErrors = new List<string>();
            }
            listOfErrors.Add(errorMessage);
        }

     /*   public static void ValidateRequiredValue(ref List<string> listOfErrors, string fieldName, bool hasValue)
        {
            if (hasValue == false)
            {
                if (listOfErrors == null)
                {
                    listOfErrors = new List<string>();
                }
                listOfErrors.Add(fieldName + " is a required value.");
            }
        }*/

        public void Initialize() 
        {
            //here we let each type decide which of its conditions is best as default
            if (DefaultClientState == null) // NEW: only unless specified.
            {
                BitMask64 defaultMask = new BitMask64(typeof(StateModifier)); 
                //SpriteConditionInfo defaultInfo = new SpriteConditionInfo();
                IStateInfo defaultInfo;
                FindBestStaticInfo(defaultMask, 
                    ClientStateConditions, DefaultClientState, out defaultInfo);

                DefaultClientState = (ClientStateInfo)defaultInfo;
            }

            if (DefaultClientState != null)
            {
                DefaultClientState.Initialize();
            }

            if (ClientStateConditions != null)
            {
                foreach (var item in ClientStateConditions)
                {
                    item.Initialize();
                }
            }

          /*  if (Sound != null)
            {
                AmbientSound = GameData.Instance.AllSoundData[Sound]; 
            }*/

            if (RenderAsModelType != null)
            {
                RenderAsModelType.Initialize(); 
            } 
        }


        /// <summary>
        /// we only fade sound and particles...
        /// </summary>
        /// <returns></returns>
        public bool CanFade()
        {
            return RenderAsModelType != null || ParticleEmitterTypes != null;

        }

        //-------------------------------------------------------------------------------------------------
        public static void FindBestStaticInfo(BitMask64 condition, IStateInfo[] SpriteConditions, IStateInfo Default, out IStateInfo bestMatch)
        {

            // Search through priority best match first

            bestMatch = Default; //if we don't find a good match below, then use the defaultiest one we got (could be null)

            if (SpriteConditions == null)
                return;



            int bestScore = 0;// can't be bested by any state with zero matches, so this is default score

            foreach (var matchCandidate in SpriteConditions)
            {
                if (matchCandidate.Conditions == null || !(matchCandidate.Conditions.Any()))
                    continue; // empty set doesn't match anything.

                if (condition.Equals(matchCandidate.Conditions))
                {
                    bestMatch = matchCandidate;
                    return;// perfect match always wins
                }

                //score how many bits this info got right 

                int modifiersScore = 0;

                if (condition.Bits != 0) // != null)
                {
                    if (matchCandidate.Conditions != null)
                    {
                        modifiersScore = (int)condition.CountIntersection(matchCandidate.Conditions);

                    }


                    int negativesScore = (matchCandidate.Forbiddens == null) ? 0 : (int)condition.CountIntersection(matchCandidate.Forbiddens);

                    if (negativesScore > 0)
                        continue; //forbiddens are absolutely forbidden
                }



                //-- Condition states choose the match with the closest match among the "Condition" bits in the 
                //info record, based on satisfying the most of the "required" conditions and none of the forbidden conditions

                int scoreThisInfo = modifiersScore;

                if (scoreThisInfo > bestScore)
                {
                    bestScore = scoreThisInfo;
                    bestMatch = matchCandidate;
                }

            }
        }

        public static StateModifier GetRandomFlavour(int maxFlavour)
        {
            int flavourNo = The.Client.ClientRandomGenerator.RandomBetween(1, maxFlavour);

            switch (flavourNo)
            {
                case 1: return StateModifier.Flavour1;
                case 2: return StateModifier.Flavour2;
                case 3: return StateModifier.Flavour3;
                case 4: return StateModifier.Flavour4;
                case 5: return StateModifier.Flavour5;
                case 6: return StateModifier.Flavour6;
                case 7: return StateModifier.Flavour7;
                default: return StateModifier.Flavour1;
            }
        }

        //end class RenderableType
    }


    /// <summary>
    /// TODO: delete this... make Temporary attachables work instead.
    /// </summary>
    public class BoxHandlingWhenHauling
    {
        public bool UseHeavyBackpack;

        public AttacheePoint? ShowBoxInHand;
        public string AttachorWhenBoxIsInHand;

        public enum BoxHandlingType { AlwaysInHand, AlwaysOnBack, OnlyOnBackWhenHeavyAndHaulingFar }
                

        public BoxHandlingType BoxHandling;

        /*  public string AttachRenderableKey; // "box"

          public string  AttachorKey;

          public AttacheePoint AttacheePoint;*/

    }
}

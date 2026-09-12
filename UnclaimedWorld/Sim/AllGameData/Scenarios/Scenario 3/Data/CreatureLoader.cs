using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Combat;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    class CreatureLoader
    {

        public const int sensorRangeHuman = 350; // 7 tiles
        public const int sensorRangeHumanNight = 200; // 4;


        public const float humanMaxRegainLimit = 0.5f;
        public const float humanFractionOfMaxHitpointsGainedPerDay = 0.3f;


        public static void Init(List<EntityType> listOfEntityTypes)
        {
            #region Human Needs

            NeedType babySleepNeed;
            NeedType childSleepNeed;
            NeedType youngAdultSleepNeed;
            NeedType adultSleepNeed;
            NeedType oldSleepNeed;
            NeedType humanFoodNeed;
            NeedType humanProteinNeed;
            NeedType humanMicronutrientsNeed;
            NeedType humanStimulantsNeed;
            CreateHumanNeeds(out humanFoodNeed, out humanProteinNeed, out humanMicronutrientsNeed, out humanStimulantsNeed);

            #endregion

            #region PersonType

            //****** PERSON
            const float pickupHeavyActionPointDuration = 0.32f;
            const float pickupLightActionPointDuration = 0.56f;

            BodyType bodyType = GameData.Instance.AllBodyTypes["humanoid"];
            EntityType personType = new EntityType("entity:human")
            {
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                //AlwaysShowStatus = true,
                Name = "Human",  // TODO: show allegiance/expedition/scenario info some other way. //"PRECOL explorer",
                SummaryDescription = "", 
                Description = "", 
                UseTypeNameForDisplay = false,
                RenderableType = new RenderableType()               
                {                   
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "man", // will be overridden, only used in validation
                        GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                           /* WalkSlowSpeed = 25f, //10f, // 8f, 
                            HaulSpeed = 32f,
                            WalkNormalSpeed = 45f, // , (NORMAL man walk speed=1.0, basespeed=45f) ;  (SLOW man walk speed=0.7, basespeed=25f) ;  (FAST man walk speed=1.3, basespeed=60f)                
                            //   (NORMAL man run speed=1.0, basespeed=140f) ;   (SLOW man run speed=0.9, basespeed=120) ; (FAST man run speed=1.2, basespeed=160f)
                            WalkFastSpeed = 60f, // 24f, // 18f,//30f, // 63f,
                            RunSpeed = 120f, // 60f // ???
                            */

                         // MP: The gaitbrackets here must correspond to the base speeds defined in  LeggedLocomotorType  waaay further down (search for word: briskly)!! base speeds define the speed they use for different purposes (?)

                           { "normal", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 37f, StrideLength = 18f, StrideDuration = 0.96f, AnimationKey = "gaitStroll"}, //stride length here is just estimated, not very precise (MP 3 sep 2013)
                                              new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 67f, StrideLength = 26f, StrideDuration = 0.96f, AnimationKey = "gaitWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                           { "haulHeavy", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 40f, StrideLength = 16f, StrideDuration = 0.96f, AnimationKey = "haulHeavy"},  //mytest
                                              new GaitAnimationBracket(){ MinimumSpeed = 38f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },                                              

                        
                            { "haulHeavyFar", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 40f, StrideLength = 22f, StrideDuration = 0.96f, AnimationKey = "haulHeavyBack"},  //mp feb 20, 2015: since it uses same base speed as haulHeavy (HaulSpeed = 40f) I just adjust the stridelength to make it fit with the anim.
                                              new GaitAnimationBracket(){ MinimumSpeed = 38f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },                            


                           { "sneak",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 24f, StrideDuration = 0.96f, AnimationKey = "gaitSneak"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                           { "wounded",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 18f, StrideDuration = 0.96f, AnimationKey = "gaitLimpWalk"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                             { "fatigued",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 20f, StrideDuration = 0.96f, AnimationKey = "gaitFatiguedWalk"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                       },
                        DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                        },
                        DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "kneel" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "sittingIdle" } }           
                            }
                        },
                        AnimConditions = new AnimConditionInfo[] 
                        {

                          new AnimConditionInfo()
                            {                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },                             
                                GaitSetKey = "normal"
                            },
                            new AnimConditionInfo() // haul light - - clutching arm
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ AdditionalAnimations1 = new string[]{"haulLight"}},
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },     
                            new AnimConditionInfo() // haul light long distance - same as above, added to prevent match with HaulHeavy
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ AdditionalAnimations1 = new string[]{"haulLight"}},
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Far )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },                  
                            new AnimConditionInfo() // haul heavy - box anim
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy )},
                                GaitSetKey = "haulHeavy" //
                            },
                            new AnimConditionInfo() // haul heavy long distance
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy, (int)AnimModifier.Far )},
                                GaitSetKey = "haulHeavyFar" //
                            },
                            new AnimConditionInfo() // fatigued walk
                            {
                                GaitSetKey = "fatigued",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Fatigued )}                               
                            },
                             new AnimConditionInfo() // walking wounded
                            {
                                GaitSetKey = "wounded",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Damaged )}                              
                            },
                            new AnimConditionInfo() // 
                            {
                                GaitSetKey = "sneak",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Stealthy )}
                            },


                            // whole pickup anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },                          
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },                         
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupMounted" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupMountedEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount, (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "pickupHeavy"/*  "pickupLightSame"*/ }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            // 2nd half of pickup:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy, (int)AnimModifier.Post )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupHeavyActionPointDuration
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy),
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupLightActionPointDuration
                            },   
                            //***** drop anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping},
                                                                 
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropLightSame" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },              
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropMounted" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },  
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropMountedSame" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount, (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },  
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            }, 
                           
#region Stance changes / breakdowns

                            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
                             //default direction as "get up". so, sitting down needs "reverse flag".
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelToIdle" }}, // stand up from kneeling
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)}, 
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting),
                                Looping = Looping.No
                            },  

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sittingToIdle" }}, // stand up from sitting
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,   
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling),
                                Looping = Looping.No
                            },  

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToKneel" }}, // kneeling from standing
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Reverse)}, 
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting),
                                Looping = Looping.No
                            },   
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToSleep" }}, // standing to lying (sleeping)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying, (int)AnimModifier.Reverse)
                                },
                                Looping = Looping.No
                            },   
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToSitting" }}, // sitting from standing
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting, (int)AnimModifier.Reverse)},  //default direction is "get up" so, sitting down needs "reverse flag"
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling),
                                Looping = Looping.No
                            },   

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelToSitting" }}, // sitting from kneeling
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting, (int)AnimModifier.Kneeling, (int)AnimModifier.Reverse)}, //default direction is "get up" so, sitting down needs "reverse flag"
                                                            

                                Looping = Looping.No
                            },   


#endregion

#region IDLE 
                          

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel", "kneelCollectSoil", "kneelExamineSoil", "kneelTablet", "kneelWaterDevice" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes
                            },  


                            //MP: something wrong with position of tablet, it floats in the air
             /*                new AnimConditionInfo()
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "sittingTablet" }},  //  use of a renderable tablet (right hand) which is not an item (similar to the haul heavy box)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},                                
                                Looping = Looping.Yes,
                                TemporaryRenderablesToAttach = new[]{ new AnimConditionInfo.TemporaryAttachable(){ RenderableTypeKey = "tablet", AttachorTag = "rightHand" }},  //attachorTag necessary to type in because the tablet is not an item but just a renderable. (in items this is defined)
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.Hand,
                                         RenderableTypeKey = "tablet", Translation = new Vector3(-29.764f, 15.591f, 8.031f), Rotation = new Vector3(1.601f, 0.709f, 0.026f) }},  //MP: something wrong with position of tablet, it floats in the air
                            },  
             */

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelTablet" }},  //  use of a renderable tablet  (left hand) which is not an item (similar to the haul heavy box)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},                                
                                Looping = Looping.Yes,
                                TemporaryRenderablesToAttach = new[]{ new AnimConditionInfo.TemporaryAttachable(){ RenderableTypeKey = "tablet", AttachorTag = "leftHand" }},  //attachorTag necessary to type in because the tablet is not an item but just a renderable. (in items this is defined)
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "tablet", Translation = new Vector3(1.601f, 0.026f, 0.184f), Rotation = new Vector3(-67.559f, 167.717f, -33.543f) }},  
                            },  

                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idle", "idleLong", "idleHandsOnHips", "idleStretchesNeck",  "idleWipesNose" }},  // MP: "idleStretchesBody" should only be played very rarely
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle},                                 
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low) //forbidden low animations while standing
                               
                            },
                            
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idleTalkShort" }}, // take pauses?
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,                                 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Talk)},
                               
                            },               
                      
                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "exult" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,                                 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Happy)},
                                Looping = Looping.No
                               
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "sittingIdle", "sittingAttentive", "sittingMend" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                Looping = Looping.Yes
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel" }}, // for idling in dangerous areas
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Trouble)},
                                Looping = Looping.Yes
                            },

#endregion // END IDLE

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel", "kneelShieldEyes", "kneelExamineSoil", "kneelCollectSoil", "kneelTablet" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)}
                            },     
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "scout" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting }
                            },    
                           
                            //new AnimConditionInfo()
                            //{
                            //    animControllerKeys = new string[] { "smoking" },
                            //    Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Idle, (int)AnimState.Smoking)
                            //},
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sleep" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Sleeping,
                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)},
                                Looping = Looping.Yes  
                            },
                         
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                                                  
                                Playback = Playback.Manual
                            },
                             new AnimConditionInfo() // lighting fires, cooking...
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend" }},
                                //Sound = "activities/cooking/cookingBoil",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Mending,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes,
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun),                           
                            },

                             new AnimConditionInfo() // Crafting
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "mend" },
                                Sounds = new string [] { "activities/crafting/craftingGeneral1", "activities/crafting/craftingGeneral2" }}, //MS: I set animmodifier to Improvised on all crafting jobs in the ProcessLoader. That should make this animation and sound play when crafting.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Mending,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Improvised)},
                                Looping = Looping.Yes,
                                //Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun),    //mp don't use this anymore, it's a patchy hacky thing                 
                            },

#region Building

    #region Build while standing
                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                },
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH METAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/salvage/salvagePullMetal", "", "", "activities/gather/gatherChopLow", "" }}, //PLACEHOLDER
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Metal )},
                              
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH ELECTRONIC COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingElectronic1", "", "", "activities/building/buildingElectronic2", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Electronic )},
                              
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingImprovisedKneelWaterDevice", "", "", "activities/gather/gatherChopLow", "" }}, //GATHERCHOPLOW IS PLACEHOLDER
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised )},
                              
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH TARP
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingTarpKneelWaterDevice", "", "", "activities/building/buildingTarpKneelDig", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp )},
                              
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH TARP AND IMPROVISED
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingTarpImprovisedKneelWaterDevice", "", "", "activities/building/buildingTarpImprovisedMend", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp, (int)AnimModifier.Improvised )},
                              
                                Looping = Looping.Yes
                            },
    #endregion


    #region Build in kneeling stance   
                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingImprovisedMend", "activities/building/buildingImprovisedKneelDig", "activities/building/buildingImprovisedKneelWaterDevice" }}, //FINAL
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised, (int)AnimModifier.Kneeling )}, //Kneeling
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILDING KNEELED WITH TARP AND NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingTarpImprovisedMend", "activities/building/buildingTarpImprovisedKneelDig", "activities/building/buildingTarpImprovisedKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised, (int)AnimModifier.Tarp, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILDING KNEELED WITH TARP
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingTarpMend", "activities/building/buildingTarpKneelDig", "activities/building/buildingTarpKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH ELECTRONIC COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" }, 
                                    Sounds = new string[]{ "activities/building/buildingElectronic1", "activities/building/buildingElectronic2", "activities/building/buildingElectronic1" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Electronic, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH METAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" }, 
                                    Sounds = new string[]{ "activities/building/buildingMetalMend", "activities/building/buildingMetalKneelDig", "activities/building/buildingMetalKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Metal, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
#endregion


    #region Build with tools
                            //MS: I have not had the animationsets below occur yet. I will create the build sounds above here, and after that maybe look at creating tool building sounds.
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{ "constructPull", "cutLow", "idleSweat", "idleStretchesNeck", "idleWipesNose" },
                                    Sounds = new string[] { "activities/building/buildingSteel", "activities/building/buildingSteel", "", "", "" } 
                                },
                               // Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife )},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{ "mend", "kneelDig" }, 
                                    Sounds = new string[] { "activities/building/buildingSteel", "" } },                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                { 
                                    BaseAnimations = new string[]{  "constructPull", "chopLow", "idleSweat", "idleStretchesNeck" },
                                    Sounds = new string[] { "", "activities/building/buildingHammer", "", "" }
                                },                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer )},
                                Looping = Looping.Yes
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{ "mend" },
                                    Sounds = new string[] { "activities/building/buildingHammer" }},                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
#endregion

#endregion
                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "chopLow", "idleWipesNose", "gather" },
                                 Sounds = new string[]{ "activities/butcher/butcherConstructPull", "activities/butcher/butcherChopLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Butchering,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete )},
                                Looping = Looping.Yes
                            },

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "cutLow", "idleWipesNose", "gather" },
                                 Sounds = new string[]{ "activities/butcher/butcherConstructPull", "activities/butcher/butcherCutLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Butchering,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife )},
                                Looping = Looping.Yes
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "cutLow", "idleSweat", "attackStompRight", "constructPull", "cutLow"},
                                    Sounds = new string [] { "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow", "", "activities/salvage/salvageBreakMetal2", "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow"}},
                                //Sound = "activities/salvage/salvageAll",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Salvaging},
                                                               
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo() //MP how does it know not to attach the hoe when he plays the "gather" anim??
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "useHoe", "idleWipesNose", "gather", "useHoe" }, //mp hmm it never plays 2 in a row..so the hoeing cycle is kinda short.
                                    Sounds = new string[] { "activities/farming/farmingHoe1A", "", "activities/gather/gatherHandGather", "activities/farming/farmingHoe1B"  }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Tilling,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hoe )},
                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "farmingHoe",  Translation = new Vector3(1.916f, 4.331f, -0.026f), Rotation = new Vector3(92.12601f, -168.661f, 167.717f) }},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating},
                                Looping = Looping.Yes  
                            },                        
                           /* new AnimConditionInfo()
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                Looping = Looping.Yes  
                            },*/
                         
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gather", "kneelDig", "kneelExamineSoil" },
                                    Sounds = new string[] { "activities/gather/gatherHandGather", "activities/gather/gatherHandKneelDig", "" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes,
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun), 
                            },
                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "chopLow" },
                                    Sounds = new string[]{ "activities/gather/gatherChopLow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Low)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "machete", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) }},  
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "fishingSpearIdle", "fishingSpearThrustMiss" }},

                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear) /*, (int)AnimModifier.Low)*/}, // fishing wihtout a spear falls back on Gather standing

                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.499f, -1.234f, 0.079f), Rotation = new Vector3(86.457f, 1.417f, -4.252f) }},  
                                Looping = Looping.Yes
                            },
 

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "cutLow" },
                                    Sounds = new string[]{ "activities/gather/gatherCutLow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Low)},
                                Looping = Looping.Yes
                                //same knife coordinates as idle (defined in GameData.cs)
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gatherStand" },
                                    Sounds = new string[]{ "activities/gather/gatherHandGatherStand" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gatherStand" },
                                    Sounds = new string[]{ "activities/gather/gatherHandGatherStand" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting, 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low)}, // to fall back on when no tool is used
                                Looping = Looping.Yes
                            },



#region combat


                            ////// Unarmed

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleUnarmed" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatHit" }},
                                Playback = Playback.Manual,
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)},
                                Looping = Looping.No
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatHit" }},
                                Playback = Playback.Manual,
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)},
                                Looping = Looping.No
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Damaged)},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Bold)}  ,
                                Looping = Looping.No
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Bold, (int)AnimModifier.Damaged)},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying/*, ** lets wait with stances for death... uses special collapse anim anyway...
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)*/},
                                Looping = Looping.No
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Damaged)
                                    /*, ** lets wait with stances for death... uses special collapse anim anyway...
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)*/},
                                Looping = Looping.No
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post, (int)AnimModifier.Damaged)}  
                            },
                              
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchRightMiss" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Fail)},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Bow, (int)AnimModifier.Watergun), // don't play a miss anim when shooting
                                Looping = Looping.No
                            },
                           
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackStompRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low,  (int)AnimModifier.Right, (int)AnimModifier.Near, (int)AnimModifier.Extreme)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackSnapkickRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No

                            },
                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchLowRight" }}, 
                                // sound on  KeyName = "personPunchLowRight" in GameDataLoader
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Right, (int)AnimModifier.Near)},  
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No

                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchHighRight" }},                                 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No
                            },
                                          
                
                            ////Knife
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "attackClubHigh" },
                                 Sounds = new string[]{ "melee/STAB2_24 - 4 Stabs With Blood" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                               
                                Looping = Looping.No,
                             /*   AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.Hand,
                                         RenderableTypeKey = "knife", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) }}*/
                            },


                            ///////////Rifle

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleRifle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle)}
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAim" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},

                         //       ParticleEmitters = new[]{ new ParticleEmitterEffect(){ ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true }  }
                                
                            },      
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAim" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Far, (int)AnimModifier.Fail)}, 
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},

                         //       ParticleEmitters = new[]{ new ParticleEmitterEffect(){ ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true }  }
                                
                            },   
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "combatRifleReload" },  
                                    Sounds = new[]{ "activities/weapons/coilrifleReload" }
                                }, // keycount 48
                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle )},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving ),
                                Playback = Playback.Manual,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                                Looping = Looping.No
                            },


                            ///////Watergun         //MP: I want them to wear the watergunTank on their back as well. is that TODO?

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleRifle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun)}
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAimHip" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun, (int)AnimModifier.Far, (int)AnimModifier.Low)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatWatergunReload" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun )},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving ),
                                Playback = Playback.Manual,
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }}
                            },


                            ////////Bow

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleBow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow)}
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatBowAim" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },

                            
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatBowReload" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow )},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving ),
                                Playback = Playback.Manual,
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },


                            /////Spear

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleSpear" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold, (int)AnimModifier.Spear)}
                            },
      
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear)}
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackSpearMid" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.814f, 6.85f, 1.391f), Rotation = new Vector3(-77.008f, 36.378f, 4.252f) }}
                            },
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatSpearThrow" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.499f, -1.234f, 0.079f), Rotation = new Vector3(86.457f, 1.417f, -4.252f) }}
                            },


                            ////Club

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "hammer", Translation = new Vector3(-1.076f, -0.079f, -0.814f), Rotation = new Vector3(92.126f, 60.945f, -94.96f) }}
                            },


                            ////Machete

                                    new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" },
                                Sounds = new string[]{ "melee/STAB2_24 - 4 Stabs With Blood" } }, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),                               
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "machete", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) }}
                            },

                         
#endregion


                            /*

                            new AnimConditionInfo()//FOR AIRCRAFT, OTHER VEHICLES?
                            {
                                AnimControllerKeys = new string[] { "open_for_loading" },
                                playback = Playback.Manual,
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Reverse) //uncontaining!  
                            },


                

                            new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "propeller_right_start" },
                                playback = Playback.Manual, //forwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Pre, (int)AnimState.Right, (int)AnimState.Slow)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "propeller_left_start" },
                                playback = Playback.Manual, //forwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Pre, (int)AnimState.Left, (int)AnimState.Slow)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "driver_entry" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Crew)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "passenger_entry" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Passenger)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "open_for_loading" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Open)   
                            },
             */
                        }

                    },
                    AnimatedHeadType = new AnimatedHeadType()
                    {
                        SpineBones = new[] { "SpineB", "SpineC", "SpineD", "Neck", "Head" },
                        TurnToLookLerpFactor = 0.06f,
                        PitchForward = 0.12f
                    },
                    BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                    {
                        BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar,
                        UseHeavyBackpack = true,
                        ShowBoxInHand = AttacheePoint.RightHand,
                        AttachorWhenBoxIsInHand = "rightHand"
                    }
                }
              ,
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = 2.5f * MathHelper.Pi, //MP was: 1.5f * MathHelper.Pi   ...increased it, to 2.5f to make them turn  a bit quicker. (there's also TurnSpeedWhenTurningInPlace which controls rotation speed (turn speed) when turning in place)                
                    MeleeRadius = 12f,
                    CanRun = true,
                    Stances = "humanoid",
                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                       /* Stances = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Lying, LeggedLocomotor.Stance.Kneeling, LeggedLocomotor.Stance.Sitting },
                        IdleStancesAwayFromHome = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling },
                        IdleStancesNearHome = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling, LeggedLocomotor.Stance.Sitting },
                        
                        IdleChanceToSitFactor = 0.74f, //MP: was 0.42, they never sat at all. 0.8f
                        IdleChanceToKneelFactor = 0.8f,
                        IdleChanceToStandFactor = 1f,
                        IdleRemainInCurrentSitOrStandStanceAddend = 0.5f,
                       */

                        TerrainNegateFactor = 0.1f,
                        WalkSlowSpeed = 25f, //10f, // 8f, 
                        HaulSpeed = 40f,     //MP nov21 code change requires overhaul. was 37f                    ......    MP oct 22: was 32f.   I increased this to make them move more briskly 
                        WalkNormalSpeed = 66f, //MP nov21 code change requires overhaul. was 60f                     ...... MP oct 22: was 45f.   I increased this to make them move more briskly              /*proper speed*/,                        (NORMAL man walk speed=1.0, basespeed=45f) ;  (SLOW man walk speed=0.7, basespeed=25f) ;  (FAST man walk speed=1.3, basespeed=60f)                
                        //   (NORMAL man run speed=1.0, basespeed=140f) ;   (SLOW man run speed=0.9, basespeed=120) ; (FAST man run speed=1.2, basespeed=160f)
                        WalkFastSpeed = 70f, //MP nov21 code change requires overhaul. was  64f   ........        MP oct 22: was 60f      I set this to fit with MinimumSpeed (64f) for jog gaitbracket, so they dont break into a 1 second jog after walking for a while.          // 24f, // 18f,//30f, // 63f,
                        RunSpeed = 88f // 100f // 110f, //95f //  120f, // 60f // ???                         
                    },
                    CollisionResponderType = new CollisionResponderType()
                    {
                        AgentCollisionResponderType = new AgentCollisionResponderType()
                    }
                },
                SensorType = new SensorType()
                {
                    Range = sensorRangeHuman, // 9, // 8, // TEST ONLY //  12, //6
                    RangeAtNight = sensorRangeHumanNight, // 5, // 24 // 6
                    DetectionTypeKey = "human"
                    //DetectionType = GameData.Instance.AllDetectionTypes["human"]
                },
                ContainerType = new AgentStorageType()
                {
                    ItemStorageType = new ItemStorageType(1f), // fixed capacity - never changes
                    EquipmentStorageType = new ItemStorageType(0.5f), // fixed capacity - never changes
                    StomachStorageType = new ItemStorageType(0.07f) // stomach capacity is defined in BiologicalType - it changes with size of entity
                    
                },

                // ToolOrWeaponSlotType = new ToolOrWeaponSlotType(),
                BodyType = bodyType
            };

            personType.IntelligenceType = new IntelligenceType()
            {
                IsMobile = true,
                RespectsOwnership = true,
                AllowEscapeFromTinyAreas = true,
                CanAttack = true,
                CanSpeak = true,
                CanTradeAndCommunicate = true,
                CanProduce = true,
                CanDoJobs = true,
                CanUseWeapons = true,
                CanCheckProgress = true,
                CanUseGadgets = true,
                CanEmigrate = true,
                CanMountTools = true,
                CanReplenish = true,
                CanHaul = true,
                CanPatrol = true,
                CanHunt = true,
                CanScout = true,
                CanExamine = true,      
                IsPredator = true,
                OtherAgentsNearExpeditionCenterAreConsideredThreats = true,
                IdleChanceToTalk = 0.2f,
                ContainerTransactTag = "humanTransact",
                HasServantsTags = new[] { "servesHumans" },
                Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:bushDragon" },
                
                Attacks = new[]
                     { 
                         "personPunchHighRight",
                         "personPunchLowRight",
                         "personSnapkickRight",
                         "personStompRight"                       
                     },
                Skills = new SerializableDictionary<string, float>()
                     {
                         { "fruitPicking", 1f },
                         { "grasping", 1f }
                     },
                InterestInTriggerTypes = new[] { /*"drivenVehicle",*/ "entityDied", "creature" },
                AggroRange = 160.0f,
                ChanceToRestAfterMeleeAttack = 0.6,
                ChanceToRestAfterRangedAttack = 0.6,
                MinRestTimeAfterAttackingInSeconds = 0.5f,
                MaxRestTimeAfterAttackingInSeconds = 1.2f,

                
                #region Pickup Anim durations
                // from file AnimDurations.txt

                /*
                man_pickupHeavy 18 (17*0,04)   =0,68    ......actionpoint(with bindpose=0): 9 (8 = 0,32)
                man_pickupLight 31 = 1,2		......actionpoint(with bindpose=0): 15 (14 = 0,56)
                man_pickupLightSame 32 = 1,24		......actionpoint(with bindpose=0): 15 (0,56)
                man_pickupEquipped 44 1,72		......actionpoint(with bindpose=0): 13 (12 = 0,48)
                man_pickupMounted 25 = 0,96		...actionpoint(with bindpose=0): 13 (12 = 0,48)


                man_dropHeavy 24 (23*0,04)= 0,92     	...actionpoint(with bindpose=0): 12 (11 = 0,44)
                man_dropEquipped 34 = 1,32		...actionpoint(with bindpose=0): 21 (20 = 0,8)
                man_dropLight 31 = 1,2			...actionpoint(with bindpose=0): 17 (16 = 0,64)
                man_dropMounted 25  (24*0,04) = 0,96	...actionpoint(with bindpose=0): 10 (9  = 0,36)
                */

                DropLightDuration = 1.2f,
                DropLightActionPointDuration = 0.64f,

                DropHeavyDuration = 0.92f,
                DropHeavyActionPointDuration = 0.44f,

                pickupMountedEquippedDuration = 1.72f,
                pickupMountedEquippedActionPointDuration = 0.48f,

                pickupEquipDuration = 1.72f,
                pickupEquipActionPointDuration = 0.48f,


                PickupLightDuration = 1.2f,
                PickupLightActionPointDuration = pickupLightActionPointDuration,

                PickupHeavyDuration = 0.68f,
                PickupHeavyActionPointDuration = pickupHeavyActionPointDuration,

                pickupMountDuration = 0.96f,
                pickupMountActionPointDuration = 0.48f,

                SwitchLightToLightDuration = 2.4f, // 1.24f;
                SwitchLightToLightActionPointDuration = 1.2f, // 0.56f;

                #endregion

                #region Drop Anim durations

                dropEquippedDuration = 1.32f,
                dropEquippedActionPointDuration = 0.8f,

                dropMountedDuration = 0.96f,
                dropMountedActionPointDuration = 0.36f,

                DropLightToLightActionPointDuration = 0.6f,
                DropLightToLightDuration = 1.24f, // 

                dropMountToMountActionPointDuration = 0.5f, // TODO
                dropMountToMountDuration = 1f, // TODO

                #endregion
            };


            personType.BiologicalType = new BiologicalType()
            {
                SpeciesPlural = "humans",
                HoldsFoodWhenEating = true,
                EatingStances = new[] { new ChanceToTakeStance() { Stance = "sitting" } },
                OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,  //ONLY influences the maximum time an agent can run between breaks. (no effect on tasks such as hauling as of yet (july 2013)) We aim for top speed running across 1,5 screen before rest is needed.
                FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "edibleVegi", "coffee", "alcoholicBeverage" }, // TODO: add more tags
                TimeToConsumeFullMealInDays = 0.0065f,
                StomachSizeFractionOfEntityBulk = 0.45f,  //corresponds to recommended daily food intake of 0.25f
                StomachContentsDecreaseRatePerDay = 2,
                ActiveStealthRating = 0f, // use a skill instead
               
                MaxRegainLimit = humanMaxRegainLimit,
                FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                TimeOfDayToGoToSleep = 0.35, //was 0.95 ...for tut. so that they sleep after a looong night...
            
                Carcass = "item:body",
                RaceTypes = new[] //primary color=skin color, secondary color= hair color
                                {                                
                                    new RaceType()
                                    { KeyName = "white1", Name="White", PortraitSkinType="Celtic",                                         
                                        PrimaryColor = new Vector3(0.9803922f, 0.9764706f, 0.9686275f),  Edge = 0.2f, // Celtic                                      
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.9333333f, 0.9019608f, 0.8588235f)),
                                            new ColorProbability(0.5f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
                                            new ColorProbability(0.9f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
                                            new ColorProbability(1f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f))} 
                                    },
                                    new RaceType()                                      // LightEuropean
                                    { KeyName = "white2", Name="White", PortraitSkinType="Light European", PrimaryColor = new Vector3(0.9529412f, 0.9176471f, 0.8980392f), Edge = 0.5f, // 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f 
                                    SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.9333333f, 0.9019608f, 0.8588235f)),
                                            new ColorProbability(0.2f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
                                            new ColorProbability(0.3f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
                                            new ColorProbability(0.5f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f)),
                                            new ColorProbability(0.6f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
                                            new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
                                            new ColorProbability(0.8f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.9f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // AverageCaucasian
                                    { KeyName = "white3",  Name="White", PortraitSkinType="Average Caucasian", PrimaryColor = new Vector3(0.9960784f, 0.9647059f, 0.8823529f),  Edge = 0.7f, // 0f, 0f, 0f, 0.1f, 0.2f, 0.7f, 0.8f, 0.9f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f)),
                                            new ColorProbability(0.2f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
                                            new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
                                            new ColorProbability(0.8f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.9f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1.0f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // OliveSkin
                                    { KeyName = "hispanic", Name="Hispanic", PortraitSkinType="Olive skin", PrimaryColor = new Vector3(0.9215686f, 0.8392157f, 0.6235294f),  Edge = 0.8f, //  0f, 0f, 0f, 0f, 0f, 0f, 0.1f, 0.7f, 0.9f, 1.0f
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.7f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1.0f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // Dark
                                    { KeyName = "black1", Name="Black", PortraitSkinType="Dark", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 0.85f, // 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0.7f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.2f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.7f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // Black
                                    { KeyName = "black2", Name="Black", PortraitSkinType = "Black", PrimaryColor = new Vector3(0.3411765f, 0.1960784f, 0.1607843f),  Edge = 0.9f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.4f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "asian", Name="Asian", PortraitSkinType="East Asian", PrimaryColor = new Vector3(0.9529412f, 0.9176471f, 0.8980392f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // //blue stripes. white hair
                                    { KeyName = "blue1", Name="ManBlue1", PortraitSkinType="White", ModelBasicTextureName="ManBlue1Texture", PrimaryColor = new Vector3(0.9960784f, 0.9647059f, 0.8823529f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 
                                    },
                                    new RaceType()                              // // blue stripes. black skin black hair
                                    { KeyName = "blue2", Name="ManBlue2", PortraitSkinType="Dark", ModelBasicTextureName="ManBlue2Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 

                                    },
                                    new RaceType()                              // red stripes. black hair light skin
                                    { KeyName = "red1", Name="ManRed1", PortraitSkinType="East Asian", ModelBasicTextureName="ManRed1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 

                                    },
                                    new RaceType()                              // red stripes. blond hair.
                                    { KeyName = "red2", Name="ManRed2", PortraitSkinType="White", ModelBasicTextureName="ManRed2Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 

                                    },
                                    new RaceType()                              // // blue stripes. YES blue, even tho it says ManGreen1Texture. black hair light skin   MP: why did skintype say black then. Answer: it's overridden by the texture file, but in any case, I've now changed it to SkinType="White"
                                    { KeyName = "green1", Name="ManGreen1", PortraitSkinType="White", ModelBasicTextureName="ManGreen1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 

                                    },
                                    new RaceType()                              // //green stripes. brown hair
                                    { KeyName = "green2", Name="ManGreen2", PortraitSkinType="White", ModelBasicTextureName="ManGreen2Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} 

                                    },
                                    new RaceType()                              // // black stripes. blond hair.
                                    { KeyName = "grey1", Name="ManGrey1", PortraitSkinType="White", ModelBasicTextureName="ManGrey1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}

                                    },
                                    new RaceType()                              // //black stripes. brown hair
                                    { KeyName = "grey2", Name="ManGrey2", PortraitSkinType="White", ModelBasicTextureName="ManGrey2Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}

                                    },
                                    new RaceType()                              //  //blue stripes. brown hair.
                                    { KeyName = "grey3", Name="ManGrey3", PortraitSkinType="White", ModelBasicTextureName="ManGrey3Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}


                                    },
                                    new RaceType()                            
                                    { KeyName = "greySolid1", Name="ManGreySolid", PortraitSkinType="White", ModelBasicTextureName="ManGreySolid1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f, 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}

                                    } ,
                                    new RaceType()                             
                                    { KeyName = "greenSolid1", Name="ManGreenSolid", PortraitSkinType="White", ModelBasicTextureName="ManGreenSolid1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    },
                                    new RaceType()                             
                                    { KeyName = "blueSolid1", Name="ManBlueSolid", PortraitSkinType="White", ModelBasicTextureName="ManBlueSolid1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    },
                                    new RaceType()   /////                          
                                    { KeyName = "blueBrownClothes1", Name="ManBlueBrownClothes1", PortraitSkinType="White", ModelBasicTextureName="ManBlueBrownClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    },  
                                    new RaceType()                             
                                    { KeyName = "greenGreyClothes1", Name="ManGreenGreyClothes1", PortraitSkinType="Black", ModelBasicTextureName="ManGreenGreyClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    }, 
                                    new RaceType()                             
                                    { KeyName = "greyClothes1", Name="ManGreyClothes1", PortraitSkinType="White", ModelBasicTextureName="ManGreyClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    },
                                    new RaceType()                             
                                    { KeyName = "whitePantsClothes1", Name="ManWhitePantsClothes1", PortraitSkinType="East Asian", ModelBasicTextureName="ManWhitePantsClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    }, 
                                    new RaceType()                             
                                    { KeyName = "greenBlueClothes1", Name="ManGreenBlueClothes1", PortraitSkinType="White", ModelBasicTextureName="ManGreenBlueClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    },
                                     new RaceType()                             
                                    { KeyName = "whiteBlueClothes1", Name="ManWhiteBlueClothes1", PortraitSkinType="White", ModelBasicTextureName="ManWhiteBlueClothes1Texture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    }                                    
                                   
                                },
                Castes = new List<CasteType>() 
                { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 1.8f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 80f, WeightStandardDeviation = 0.15f,
                         
                        ModelName = "man", // "man",
                        ModelScale = 2f, 
                  
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, 
                                NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                                , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                               , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                               , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                               , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 1.68f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 68f, WeightStandardDeviation = 0.10f,

                        ModelName = "woman",                       
                        ModelScale = 2f, //1.95f, 
                  
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, 
                                NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                              , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                               , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                              , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                              , NeedTypes = new NeedType[]{  humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                        } 
                    } 
                }

            };

            personType.Person = new PersonType()
            {

                ShirtColors = new List<Vector3>() { new Vector3(0.4980392f, 0.4352941f, 0.3411765f), new Vector3(0.6196079f, 0.4705882f, 0.2509804f), 
                    new Vector3(0.3568628f, 0.6196079f, 0.3843137f), new Vector3(0.509804f, 0.6784314f, 0.4941176f), new Vector3(0.2862745f, 0.6196079f, 1f), 
                    new Vector3(0.3176471f, 0.5529412f, 0.6784314f), new Vector3(0.7372549f, 0.2588235f, 0.2509804f), new Vector3(0.7764706f, 0.7764706f, 0.7764706f), 
                    new Vector3(0.4784314f, 0.4784314f, 0.4784314f), new Vector3(0.3372549f, 0.3372549f, 0.3372549f), new Vector3(0.3882353f, 0.3882353f, 0.3882353f)},

                PantsColors = new List<Vector3>(){ new Vector3(0.4392157f, 0.3647059f, 0.2705882f), new Vector3(0.627451f, 0.5372549f, 0.427451f), new Vector3(0.3803922f, 0.509804f, 0.3843137f), 
                        new Vector3(0.2588235f, 0.4352941f, 0.6470588f), new Vector3(0.4901961f, 0.7215686f, 0.8784314f), new Vector3(0.4980392f, 0.4980392f, 0.4980392f), new Vector3(0.627451f, 0.627451f, 0.627451f)}


            };

            listOfEntityTypes.Add(personType);

            #endregion



            #region BushDragon Food needs
            NeedType bushDragonFoodNeed = new NeedType()
            {
                KeyName = "foodEnergy",
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "foodEnergy",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.1f
                },
                DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.02f },  //MP...july 24.. was 0.6f   ..buffed it to make rats hungrier quicker (because they spawn with full nutrition, 1f, which I can't seem to change anywhere?)
                         
                LimitForDecreasedEnergy = 0.10f,
                DecreasedEnergyWeight = 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    DaysAtZeroCausingCollapse = 4f, // never die??
                    DaysAtZeroCausingDeath = 4f,
                    DaysAtZeroDecreaseFactor = 1f, //0.5f,
                    UseExertionFactorToDecrease = true                    
                },
            };

            #endregion


            #region bushdragontype
            EntityType bushdragonType = new EntityType("entity:bushDragon")
            {
                Name = "Northern bush dragon", //mp i changed its texture to grey because its attack looks different (squirt instead of spray). under race... MP june 2015
                ThumbnailSmall = "HUD_thumbnail_bushDragon",
                SummaryDescription = "Omnivorous herd animal",
                Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats low vegetation, small animals, carrion.\n \n HEIGHT: Up to 1.5 m (wings excluded)\n \n ANATOMY\n Waddling, 3-legged animal which has developed wings, not for flight but for display purposes. Likely used to dissuade predators and possibly in mating behaviour. The creature has a defensive weapon in the form of a chemical spray.\n \n BEHAVIOR\n If approached, bush dragons will defend themselves much in the manner of the terran skunk. Against the quadites the spray seems to be particularly effective, causing incapacitation and even death.\n \n SURVIVAL GUIDE NOTES\n The slow-moving creatures protect their herd, but if distance is observed they do not attack. The toxicity to humans of their defensive chemical is yet to be determined, but caution is advised.",
                DetectionTag = "huge",
                RenderableType = new RenderableType()
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        ModelScale = 0.8f, //0.8f
                        AssetName = "bushdragon",
                        GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 11f /* measured? 21.8f*/, StrideDuration = 1.1f, AnimationKey = "gaitWalk"},        //MP: iterated strideduration 17th dec 2013.   ... was:    MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 11f /* measured? 21.8f*/, StrideDuration = 0.6f, AnimationKey = "gaitWalk"                                 
                                              } }
                       },
                        //new
                        DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                        },
                        DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   //bush dragon
                            }
                        },
                        //new end



                        AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                    /*         new AnimConditionInfo() //mp jan 2015. how come it works when this one is not in use. this anim is the unfolding wings anim. that we see in game.
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "scare" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Trouble) }, //??
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                            },
                     */


                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackNormal" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Near)}
                              
  
                            },

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/bushdragonHit" }},
                                Playback = Playback.Manual,                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }
                        }
                    }
                },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                    FourSidedSymmetry = false,
                    MeleeRadius = 16, //16f (pixels)

                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                        TerrainNegateFactor = 0.6f,
                        WalkSlowSpeed = 8f, // 8f, 
                        WalkNormalSpeed = 9f /*debug only*/, //18f /*proper speed*/, patrician:26f, turnip:11f
                        WalkFastSpeed = 11f, // 18f,//30f, // 63f,
                    },
                    CollisionResponderType = new CollisionResponderType()
                    {
                        AgentCollisionResponderType = new AgentCollisionResponderType()
                    }
                },
                SensorType = new SensorType()
                {
                    Range = sensorRangeHuman,
                    RangeAtNight = sensorRangeHumanNight,
                    DetectionTypeKey = "defaultDetection" // this is how the bush dragon detects. see BaseDataLoader under listOfDetectionTypes for definition
                },

                BodyType = GameData.Instance.AllBodyTypes["bushdragon"],
                
                ContainerType = new AgentStorageType() //0.5f)
                {
                    ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                    StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                }                
            };

            bushdragonType.IntelligenceType = new IntelligenceType()
            {
                ForageAndHuntingRadius = 290,
                MembersScoutingFraction = 1f,
                IsMobile = true,
                CanAttack = true,
                CanUseWeapons = false,
                CanHunt = false,
                CanScout = true,
                CanExamine = true,      
                CanPatrol = true,
                CanHaul = false,

                StrengthRating = StrengthRating.LikeHumans, 
                Courage = 0.5f,
                MemoryInDays = 3f,
                Boldness = 0.25f,//0.5f,
                AggroRange = 200.0f, // higher than normal to force combat 
                Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.6f
                        }
                    },
                Attacks = new[]
                { 
                 "bushDragonSpray"            
                }
            };


            bushdragonType.BiologicalType = new BiologicalType()
            {
                OxygenAndMuscleEnergyIncreaseRatePerDay = 8f,
                TimeToConsumeFullMealInDays = 0.05f,
                FoodItemTagsThatCanBeConsumed = new[] { "inedibleVegi", "edibleVegi", "spoiledMeal" },
                StomachSizeFractionOfEntityBulk = 0.2f,
                StomachContentsDecreaseRatePerDay = 3,
                ActiveStealthRating = 0.1f,
                OrderKey = "bushDragonOrder",
                IsTerritorial = true,
                MaxRegainLimit = humanMaxRegainLimit,
                FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                Carcass = "item:bushDragonCarcass",
                RaceTypes = new[] // mp I'm using a different texture because their spray is different...ray instead of spray.
                    {            
                        new RaceType()   //note that this name does not overwrite the species name, currently. bug. (june 2015)                               // Beige
                        { Name="Grey Bush Dragon", PortraitSkinType="Dark", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "BushdragonDarkTexture", Edge = 0.3f, ModelScale = 0.8f
                        },   
                    },
    
                Castes = new List<CasteType>() 
                { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 140f, WeightStandardDeviation = 0.10f,  //*---mp increased weight to avoid them hauling carcasses after hunt in tutorial scenario. was:  WeightMean = 100f WeightStandardDeviation = 0.15f

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                               , NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                             //   , NeedTypes = new NeedType[]{ childSleepNeed }
                               , NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f 
                            //   , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                               , NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f 
                            //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                                ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                            //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                                ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 140f, WeightStandardDeviation = 0.10f, //was WeightMean = 90f

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                                    ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                                 ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                            //   , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                                ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f   
                          //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                                ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                           //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                                ,NeedTypes = new NeedType[]{ bushDragonFoodNeed }
                            }
                        } 
                    } 
                }

            };

            listOfEntityTypes.Add(bushdragonType);

            #endregion

          
        }


        /// <summary>
        /// deprecate this - find a better way to show entity type descriptions
        /// </summary>
        /// <param name="babySleepNeed"></param>
        /// <param name="childSleepNeed"></param>
        /// <param name="youngAdultSleepNeed"></param>
        /// <param name="adultSleepNeed"></param>
        /// <param name="oldSleepNeed"></param>
        /// <param name="humanFoodNeed"></param>
        /// <param name="humanProteinNeed"></param>
        /// <param name="humanMicronutrientsNeed"></param>
        public static void CreateHumanNeeds(out NeedType humanFoodNeed, out NeedType humanProteinNeed, out NeedType humanMicronutrientsNeed, out NeedType humanStimulantsNeed)
        {
           
            float dailyMealBulk = 0.25f;   //....Lars suggests 0,25f bulk food pr game day

            humanFoodNeed = new NeedType()
            {
                KeyName = "foodEnergy",
                FoodNeedType = new FoodNeedType() { FoodNutrient = "foodEnergy",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.6f * dailyMealBulk / 1f  //  = 0,15....Lars suggests 0,25f bulk food pr game day (dailyMealBulk defined 12 lines up). with 60% energy from an ideal meal, for a man weighing 1f bulk, the RequiredNutrientsAsFractionOfEntityBulk is 0,6*0,25/1f
                },
                DecreasePerDay = new NormalDistribution() { Mean = 0f, StandardDeviation = 0f },              
             
                LimitForDecreasedEnergy = 0.15f,
                DecreasedEnergyWeight = 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.05f,
                    DaysAtZeroDecreaseFactor = 1f, //0.5f,
                    UseExertionFactorToDecrease = false                            
                },

            };
            humanProteinNeed = new NeedType()
            {
                KeyName = "protein",
                FoodNeedType = new FoodNeedType() 
                { 
                    FoodNutrient = "protein",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.012f   // answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
         
                },
                DecreasePerDay = new NormalDistribution() { Mean = 0f, StandardDeviation = 0f },              
                LimitForDecreasedEnergy = 0.05f,
                DecreasedEnergyWeight = 0.08f, // 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    /*DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.1f,
                    DaysAtZeroDecreaseFactor = 0.5f,*/
                    LimitForReducedGrowth = 0.1f,
                    LimitForIncreasedSickness = 0.05f,
                    UseExertionFactorToDecrease = false                      
                }
            };
            humanMicronutrientsNeed = new NeedType()
            {
                KeyName = "micronutrients",
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "micronutrients",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.0006f   //recommended daily intake of min+vit is 6g. 0,006kg*8/80kg/bulk = 0,0006f bulk vitamins/gameday... (calculated here: https://docs.google.com/a/unclaimedworld-game.com/spreadsheet/ccc?key=0Asy7trIq4ukddFZ1V0Zhd1ptOVJId0JfQS1TQ2FoZ2c#gid=0 and also look in the drive doc where needs are calculated )
                
                },
                DecreasePerDay = new NormalDistribution() { Mean = 0f, StandardDeviation = 0f },              
             
                LimitForDecreasedEnergy = 0.05f,
                DecreasedEnergyWeight = 0.08f, // 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    /*DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.1f,
                    DaysAtZeroDecreaseFactor = 0.5f,*/
                    LimitForReducedGrowth = 0.1f,
                    LimitForIncreasedSickness = 0.05f,
                    UseExertionFactorToDecrease = false                    
                }
            };

            humanStimulantsNeed = new NeedType()
            {
                KeyName = "stimulants", // we use plural as with micronutrients...             
                FoodNeedType = new FoodNeedType()
                { 
                    FoodNutrient = "stimulants", // the idea is that coffee, drugs, cigarettes etc. can substitute for eachother
                    IsEssential = false,
                    RequiredNutrientsAsFractionOfEntityBulk = 0.0006f // same amount as micronutrients..
                },
                DecreasePerDay = new NormalDistribution() { Mean = 0f, StandardDeviation = 0f },      
                LimitForDecreasedEnergy = 0f,
                DecreasedEnergyWeight = 0f, // has no adverse effects, the need is only intended to limit the consumption
               /* ComfortEffects = new ComfortEffects()
                {
                    ComfortWeight = 0.05f                   
                }*/
            };
        }
        //public static void CreateHumanNeeds(out NeedType babySleepNeed, out NeedType childSleepNeed, out NeedType youngAdultSleepNeed, out NeedType adultSleepNeed, out NeedType oldSleepNeed, out NeedType humanFoodNeed, out NeedType humanProteinNeed, out NeedType humanMicronutrientsNeed, out NeedType humanStimulantsNeed)
        //{
        //    babySleepNeed = new NeedType() { KeyName = "sleep", NeedClass = NeedClass.Sleep, DecreasePerDayMean = 0.7f, DecreasePerDayStandardDeviation = 0.03f, HappinessWeightMean = 0.8f, HappinessWeightDeviation = 0.03f, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
        //    childSleepNeed = new NeedType() { KeyName = "sleep", NeedClass = NeedClass.Sleep, DecreasePerDayMean = 0.4f, DecreasePerDayStandardDeviation = 0.02f, HappinessWeightMean = 0.7f, HappinessWeightDeviation = 0.02f, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
        //    youngAdultSleepNeed = new NeedType() { KeyName = "sleep", NeedClass = NeedClass.Sleep, DecreasePerDayMean = 0.38f, DecreasePerDayStandardDeviation = 0.015f, HappinessWeightMean = 0.5f, HappinessWeightDeviation = 0.02f, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
        //    adultSleepNeed = new NeedType()
        //    {
        //        KeyName = "sleep",
        //        NeedClass = NeedClass.Sleep,
        //        DecreasePerDayMean = 1,
        //        DecreasePerDayStandardDeviation = 1,
        //        HappinessWeightMean = 0.5f,
        //        HappinessWeightDeviation = 0.015f,
        //        LimitForDecreasedEnergy = 0.4f,   //MP: used to be 0.2f
        //        DecreasedEnergyWeight = 0.4f,
        //        PhysicalNeedType = new PhysicalNeedType()
        //        {
        //            DaysAtZeroCausingDeath = 2f,
        //            DaysAtZeroDecreaseFactor = 2f // decrease 'starvation' quickly
        //        }
        //    };
        //    oldSleepNeed = new NeedType() { KeyName = "sleep", NeedClass = NeedClass.Sleep, DecreasePerDayMean = 0.4f, DecreasePerDayStandardDeviation = 0.02f, HappinessWeightMean = 0.6f, HappinessWeightDeviation = 0.015f, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };

        //    float dailyMealBulk = 0.25f;   //....Lars suggests 0,25f bulk food pr game day

        //    humanFoodNeed = new NeedType()
        //    {
        //        KeyName = "foodEnergy",
        //        NeedClass = NeedClass.Food,
        //        DecreasePerDayMean = 0f,  //0.7f
        //        DecreasePerDayStandardDeviation = 0f,
        //        LimitForDecreasedEnergy = 0.15f,
        //        DecreasedEnergyWeight = 0.6f,
        //        PhysicalNeedType = new PhysicalNeedType()
        //        {
        //            DaysAtZeroCausingCollapse = 2f,
        //            DaysAtZeroCausingDeath = 2.05f,
        //            DaysAtZeroDecreaseFactor = 0.5f,
        //            UseExertionFactorToDecrease = true,
        //            FoodNutrient = GameData.Instance.AllFoodNutrientTypes["foodEnergy"],
        //            RequiredNutrientsAsFractionOfEntityBulk = 0.6f * dailyMealBulk / 1f  //  = 0,15....Lars suggests 0,25f bulk food pr game day (dailyMealBulk defined 12 lines up). with 60% energy from an ideal meal, for a man weighing 1f bulk, the RequiredNutrientsAsFractionOfEntityBulk is 0,6*0,25/1f
        //        },

        //    };
        //    humanProteinNeed = new NeedType()
        //    {
        //        KeyName = "protein",
        //        NeedClass = NeedClass.Food,
        //        DecreasePerDayMean = 0f,  //0.4f
        //        DecreasePerDayStandardDeviation = 0f,

        //        LimitForDecreasedEnergy = 0.05f,
        //        DecreasedEnergyWeight = 0.08f, // 0.6f,
        //        PhysicalNeedType = new PhysicalNeedType()
        //        {
        //            /*DaysAtZeroCausingCollapse = 2f,
        //            DaysAtZeroCausingDeath = 2.1f,
        //            DaysAtZeroDecreaseFactor = 0.5f,*/
        //            LimitForReducedGrowth = 0.1f,
        //            LimitForIncreasedSickness = 0.05f,
        //            UseExertionFactorToDecrease = false,
        //            FoodNutrient = GameData.Instance.AllFoodNutrientTypes["protein"],
        //            RequiredNutrientsAsFractionOfEntityBulk = 0.012f   // answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
        //        }
        //    };
        //    humanMicronutrientsNeed = new NeedType()
        //    {
        //        KeyName = "micronutrients",
        //        NeedClass = NeedClass.Food,
        //        DecreasePerDayMean = 0f,  //1f
        //        DecreasePerDayStandardDeviation = 0f,

        //        LimitForDecreasedEnergy = 0.05f,
        //        DecreasedEnergyWeight = 0.08f, // 0.6f,
        //        PhysicalNeedType = new PhysicalNeedType()
        //        {
        //            /*DaysAtZeroCausingCollapse = 2f,
        //            DaysAtZeroCausingDeath = 2.1f,
        //            DaysAtZeroDecreaseFactor = 0.5f,*/
        //            LimitForReducedGrowth = 0.1f,
        //            LimitForIncreasedSickness = 0.05f,
        //            UseExertionFactorToDecrease = false,
        //            FoodNutrient = GameData.Instance.AllFoodNutrientTypes["micronutrients"],
        //            RequiredNutrientsAsFractionOfEntityBulk = 0.0006f   //recommended daily intake of min+vit is 6g. 0,006kg*8/80kg/bulk = 0,0006f bulk vitamins/gameday... (calculated here: https://docs.google.com/a/unclaimedworld-game.com/spreadsheet/ccc?key=0Asy7trIq4ukddFZ1V0Zhd1ptOVJId0JfQS1TQ2FoZ2c#gid=0 and also look in the drive doc where needs are calculated )
        //        }
        //    };

        //    humanStimulantsNeed = new NeedType()
        //    {
        //        KeyName = "stimulants", // we use plural as with micronutrients...
        //        NeedClass = NeedClass.Food,
        //        DecreasePerDayMean = 0f,  //0.4f
        //        DecreasePerDayStandardDeviation = 0f,

        //        LimitForDecreasedEnergy = 0f,
        //        DecreasedEnergyWeight = 0f, // has no adverse effects, the need is only intended to limit the consumption

        //        PhysicalNeedType = new PhysicalNeedType()
        //        {
        //            LimitForReducedGrowth = 0f,
        //            LimitForIncreasedSickness = 0f,
        //            UseExertionFactorToDecrease = false,
        //            FoodNutrient = GameData.Instance.AllFoodNutrientTypes["stimulants"], // the idea is that coffee, drugs, cigarettes etc. can substitute for eachother
        //            RequiredNutrientsAsFractionOfEntityBulk = 0.0006f // same amount as micronutrients..
        //        }
        //    };
        //}

    }
}

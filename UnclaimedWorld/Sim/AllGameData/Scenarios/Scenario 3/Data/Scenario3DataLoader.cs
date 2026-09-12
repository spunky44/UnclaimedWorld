using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Combat;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.HelpTopics;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    /// <summary>
    /// loads/defines the game data that this scenario depends on
    /// </summary>
    public class Scenario3DataLoader : DataLoader
    {

        public Scenario3DataLoader(): base(Config.DataType.RGScenario, 0.1f)
        {

        }

      

        protected override List<InGameEvents.Actions.EventActionType> InitEventActionTypes()
        {
            return EventActionLoader.Init();
        }

        protected override List<InGameEvents.PolledEventType> InitGlobalConditionalEvents()
        {
            return PolledEventsLoader.Init();
        }




        protected override List<AgentActionHook> InitAgentActionHooks()
        {
            return EventHooksLoader.InitAgentActionHooks();
        }

        protected override List<AttackTypeActionHook> InitAttackTypeEventHooks()
        {
            return EventHooksLoader.InitAttackTypeHooks();
        }

        protected override List<ProcessTypeActionHook> InitProcessTypeEventHooks()
        {
            return EventHooksLoader.InitProcessTypeHooks();
        }

        protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
        }

        protected override List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectResourceTypeHooks();
        }

        protected override ClientSide.Interface.GUIConstants InitGUIConstants()
        {
            GUIConstants constants = new GUIConstants();

            constants.EnableFilters = false;
            constants.EnableStandingOrders = false;

            return constants;  
        }

        protected override List<InGameEvents.Actions.ActionSets> InitActionSets()
        {
            return ActionSetsLoader.Init();
        }

        protected override List<EntityType> InitEntityTypes()
        {
            List<EntityType> listOfEntityTypes = new List<EntityType>();

         // CreatureLoader.Init(listOfEntityTypes);
            StructureLoader.Init(listOfEntityTypes);
            ItemsLoader.Init(listOfEntityTypes);
            TerrainFeatureLoader.Init(listOfEntityTypes);
            CreatureLoader.Init(listOfEntityTypes);
            return listOfEntityTypes;
        }


        protected override List<HelpTopic> InitTutorialTopics()
        {
            return TutorialLoader.Init();
        }

        protected override List<Processes.ProcessType> InitProcessTypes()
        {
            return ProcessLoader.Init();
        }

        protected override List<EntityTypePolledEvent> InitEntityPolledEvents()
        {
            return EntityPolledEventsLoader.Init();
        }

        protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
        {
            return EntityTypeDescriptionLoader.Init();
        }

        protected override List<BodyLayerType> InitBodyLayerTypes()
        {
           
            List<BodyLayerType> list = new List<BodyLayerType>();

            list.Add( new BodyLayerType()
            {
                KeyName = "clothesLayerBuffed",
                Name = "Survival suit",
                DamageReductionConstant = new Dictionary<string, float>{
                     { "bite", 4f },
                     {"sharp", 4f },
                     {"blunt", 2f },                   
                     {"fire", 3f },
                     {"piercing", 5f },
                     {"smallAnimalGrapple", 5f },
                     {"antiTwinkler", 5f }, //1f buffed
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.2f },
                     {"sharp", 0.2f },
                     {"blunt", 0.2f },                 
                     {"fire", 0.3f },
                     {"piercing", 0.3f },
                     {"smallAnimalGrapple", 0.3f },
                     {"antiTwinkler", 0.3f }, // 0.0f buffed 
                     }
            });

            return list;
        }

        protected override List<Entities.Body.BodyType> InitBodyTypes()
        {
            // overrides human body with buffed version for tut
            List<BodyType> listOfBodyTypes = new List<BodyType>();
            
            string bodyKeyName = "humanoid";
            listOfBodyTypes.Add(new BodyType(bodyKeyName)
            {
                BodyPartTypes = new BodyPartType[]{
                            new BiologicalBodyPartType()
                            { 
                                BodyKeyName = bodyKeyName, Name = "Torso", 
                                ArmorLayer = "clothesLayerBuffed", 
                                    OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}, 
                                ToHitProfileBack = 0.35f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]
                            {
                             new BiologicalBodyPartType()
                             { 
                                 Name = "Head", BodyKeyName = bodyKeyName, 
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}   
                                 , 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.1f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f, HitpointsFraction = 0.2f,
                             },
                            new BiologicalBodyPartType()
                            {   Name = "Left arm", BodyKeyName = bodyKeyName, 
                                ArmorLayer = "clothesLayerBuffed", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.3f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Manipulation, Weight = 0.5f }},
                            },
                            new BiologicalBodyPartType() { Name = "Right arm", BodyKeyName = bodyKeyName, 
                               ArmorLayer = "clothesLayerBuffed",
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.3f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Manipulation, Weight = 0.5f } },
                            },
                            new BiologicalBodyPartType() { Name = "Left leg", BodyKeyName = bodyKeyName, 
                                 ArmorLayer = "clothesLayerBuffed",
                                ToHitProfileBack = 0.25f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0.3f, ToHitProfileRight = 0f, HitpointsFraction = 0.35f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.35f } }, // don't immobilize fully... we need more features
                            },
                            new BiologicalBodyPartType() { Name = "Right leg", BodyKeyName = bodyKeyName, 
                                 ArmorLayer = "clothesLayerBuffed",
                                ToHitProfileBack = 0.25f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.3f, HitpointsFraction = 0.35f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.35f } },
                            }
                            } 
                                
                            } 
                        }

            });

            return listOfBodyTypes;

        }

        protected override List<AttackType> InitAttackTypes()
        {
            List<AttackType> list = new List<AttackType>();

            // overrides bush dragon attack with nerfed version for tut.
            list.Add(new AttackType()
            {
                KeyName = "bushDragonSpray",
                Damage = "antiTwinkler",
                DamageMean = 0.1f, // 0.5f //nerfed for tut..............MP feb 2015: tried to put in the new bush dragon particle effect and cone, but it needs some  code migration from the BaseDataLoader.cs I think.
                DamageStandardDeviation = 0f, // 0.5f, nerfed for tut
                ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonPoisonShot"],
                ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
                SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonAttack"],
                DurationInSeconds = 1f,
                ActionPointInSeconds = 0.4f,
                AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Near },
                RequiredSkill = "unarmedFighting",/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/
                MaxRange = 100f,
                RangeType = AttackType.RangeTypes.Ray,
                BulletEffect = new BulletEffect()
                {
                    MuzzleDistance = 20f,
                    StartColor = Color.Yellow,
                    EndColor = Color.LightGreen
                },
            });

            return list;
        }

    }
}

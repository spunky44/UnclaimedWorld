

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.AI;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UWGame.ClientSide.Particles;
using UWGame.Client.Particles;
using UWGame.SimSide.Systems.Triggers;
using Microsoft.Xna.Framework.Audio;
using UWGame.Client.Audio;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Trees;
using WindowSystem;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.AI.Goals;
using SpriteSheetRuntime;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;


namespace UWGame.ClientSide.Renderables
{
    // Be mindful when adding AnimationStates. Most ideas you can come up with are probably just aggregations of existing AnimationStates
    // try your best to build your AnimationState with carefully chosen IS and IS-NOT cases of other AnimationStates
    // many examples are embedded in the enum declaration, as comments, please read
    // These constants represent the bit order in a bitmask, so do not assign values 0-63 to anything not to be used, like 'invalid' or 'none'
    // // CRITICAL: try to arrange states so that the most NORMAL state has the most CLEAR bits. The flags described in these enums
    // should characterize the abnormal exceptions.

    public enum AnimAction
    {
        Moving,         // default is stationery
        Turning,        // initiated change in orientation 
        Shuffling,      // minor position adjustments, sidestepping, getting out of someone's way, etc.
        Attacking,      // with near, it means melee, with far it means ranged attacks
        Containing,     // taking on contained entity(s)
        Harvesting,     // gathering unseen items, modifiers high and low
        Recoiling,      // getting hit/pushed by outside force
        Building,       // hammer and wrench gestures
        Mending,        // producing/repairing smaller stuff. (this is currently the default in process loader if nothing is set.mp July 2015)
        Digging,        //standing/kneeling while using: shovel/pickaxe/hands/trowel etc for digging in the earth
        Hauling,        // lugging something heavy around
        Smoking,        // Enjoying a fine tobacco product
        Eating,         // hand to mouth 
        Sleeping,
        Dying,          // the cessation of life functions, not "disappearing"
        Scouting,
        Reloading,
        PickingUp,
        Dropping,
        Idle,           // lounging, idling
        Butchering,
        Salvaging,
        Tilling, //moving soil on a farmplot http://en.wikipedia.org/wiki/Tillage
        Fishing, //mp added this to use for fishing with hook and line and spearfishing
        ChangingStance
        /*,
        Unloading */      // from a container like a structure or a vehicle
    }

    public enum AnimModifier
    {

        // ---Modifiers   NO LIMIT but  mutex means "mutually exclusive"
        //WHEN YOU ADD A MUTEX or a WEAPON you must add it to MutexOtherItemStates further down this doc //
        //the new item also needs to be added to the list: case AnimModifier.Spear


        Left,           // modifier for other state flags (default is in facing direction)
        Right,          // mutex with left
        Slow,           // modifier for other state flags
        Fast,           // mutex with slow
        Happy,          // mutex with trouble
        Trouble,        // sad, afraid, (different from fail)
        Bold,        // default is normal - modifier for other states -- invokes the exaggerated version of other, if exists
        //  Danger,         // being near danger makes us more cautious
        Damaged,        // to indicate a dysfunction in movement
        Fatigued,
        Reverse,        // NOT playback mode for animation! doing some act physically backwards, backing-up a car. default is forward
        Passenger,      // in transport seats
        Crew,           // in the driver seats
        Pre,            // precursor to doing, etc...   mutex with Post. Useful for breakdowns...
        Post,           // post doing, etc...    mutex with Pre. Useful for breakdowns...
        Talk,           // saying something

        Kneeling,       // mutex with Sitting, (default is standing)
        Sitting,        // mutex with Kneeling, (default is standing)
        Lying,          // mutext with Sitting and Kneeling (default is standing)

        Near,           // modifier for other flags (default is mid-range, or unspecificed)
        Far,            // mutex with close
        High,           // NOT a stance!! (default is middle or unspecified)
        Low,            // NOT a stance!! mutex with high
        Night,          // default is day - some times we should shine headlamps, flashlights, torches... or maybe stumble around in the dark?
        Open,           // default is closed
        Fail,           // default is success (missed attack, botched job, etc.)
        Extreme,        // doing something in an extreme manner

        Heavy,          // lifting/dropping heavy items
        Mount,          // lifting/dropping items to mount
        Equip,          // lifting/dropping equipped items
        Same,         // the persons state is the same before and after the action...

        HaulHeavy,      // reserved flag only to be used together with Hauling!

        Spear,          // mutex with other item flags
        Rifle,
        Watergun,
        Knife,
        Hammer,
        Machete,
        Axe,
        Pickaxe,
        Bow,
        Hoe,
        Shovel,
        //32
        Stealthy,
        Tarp,			//Using thermal tarp
        Improvised,		//Using leaves, sticks, branches, firewood
        Metal,			//Using metal components
        Electronic,		//Using electronic components    

        Inactive,       // inactive robots mostly
        //        Prone,        Low  
        //        Strafing,     Moving | slow | left, ! turning
        //        Ruined,       Damaged | Extreme
        //        EnemyNear,    Near| Afraid
        //        SuperFast,    Fast | Extreme
        //        Firing,       attacking | doing
        //        Reloading,    attacking | preparing - hmm, you can reoload without attacking
        //        Stalking,     hunting | Slow | Moving - it seems that the act of trying not get seen should have its own flag, I have added Stealthy
        //        Cheering      joyful | extreme


        Count,
        Invalid = -1,
      


    }

    /// <summary>
    /// flags for depicting non-animating entities (trees, structures, items)
    /// will select a sprite, groundsprite, geolayout, base center, emitters, lighting in a similar fashion as for animated entities
    /// </summary>
    public enum StateModifier // was: SpriteModifier
    {
        Ordered, // structure orderec by the player but not begun yet, drawn as a blurry, ghosted sprite with additive effect
        BeingBuilt,
        BeingSalvaged,
        Burning,
        Burnt,
        Destroyed,
        Damaged,

        Inactive, // default is Active
        Overgrown, // perhaps use this as the stage after Dead for trees?

        HasResources, // for trees
        HasBranches, // for trees
        HasCrops, // for plots
        Ripe, // gatherable resources/crops are ripe and ready to be harvested
        Winter, // default is 'Summer'
        Night, // default is 'Day'
        Young, // default is 'Grown'

        Full, // for containers. default is empty
        HalfFull, // mutex with Full

        Dead, //for trees..

        Less, // for tile resources. default is 'None'
        More, // for tile resources. default is 'None'

        Flavour1, // mutex with other flavours
        Flavour2,
        Flavour3,
        Flavour4,
        Flavour5,
        Flavour6,
        Flavour7,

        Upgrade1,
        Upgrade2,
        Upgrade3, // textileWorkshop
        Upgrade4, // metalLatheShopHumanPoweredUpgrade
        Upgrade5, // carpenterWorkshopUpgrade
        Upgrade6, // polymerWorkshopUpgrade

        UpgradeStove,     //StoveUpgrade 

        UpgradeCookhouseDryingShed,
        UpgradeCookhouseSmokeOven,
        UpgradeCookhouseCommunityHall,
        
        LightIsOn,
        PreparedTool,
        BurningFuel // specific flag, for the general purpose workshop (each tool upgrade can have different effects)
    }


   


    // Decoupling guidelines: - Never read a property in Renderable from Sim
    // - Never use a return value when calling from Sim
    // - Never access properties in Renderable such as RenderAsModel when calling from Sim - instead, create methods in Renderable that access these in turn.

    // Polymorphism warning: the renderAs components of Entity are going to be replaced by
    // a list of IRenderable instances, generically
    // stores all the state variables common to all RenderAs__ classes
    // the only way for an entity to have a visible presence for the player is through this class
    // if this class is missing, or if the gameclient is missing or disabled, the simulation
    // will operate unaltered. This is called playing "headless"
    // Renderable gets updated in a separate block in the game main loop, after the simulation tick
    // also, the refresh rate of the client (Renderables...) may be different from the simulation
    // Entities and their components are considered SIMULATION side, and Renderable types are CLIENT side
    // These should always stay decoupled. Simulation is read-only to the client, and client is write-only to simulation
    // MLo

    //This class or a subclass needs to substitute in headless mode, where all calls from the Sim will be swallowed without action
    // but not requiring that the simulation ever checks for the presence of a renderable, or whether headless
    [DebuggerDisplay("{Entity}{MemoryFact}")]
    public class Renderable : GameObject, ISleepingUpdatable // IHasOptionalExpiryTimePoint
    {

        // it is possible to have Renderables without an Entity or MemoryFact!
        
        /// <summary>
        /// both Entity and MemoryFact can be null
        /// </summary>
        public Entity Entity // accessor for the Entity bound to this Renderable instance... sometimes null
        {
            get;
            set;
        }

        /// <summary>
        /// can be null
        /// </summary>
        public MemoryFact MemoryFact
        {
            get;
            set;
        }

        /// <summary>
        /// always filled.
        /// </summary>
        public RenderableType RenderableType
        {
            get;
            set;
        }

     

        public override Renderable AsRenderable
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// can be null!!!
        /// </summary>
        public IKnownEntityData Parent
        {
            get
            {
                if (Entity != null)
                {
                    return Entity;
                }
                else if (MemoryFact != null)
                {
                    return MemoryFact;
                }

                return null;
            }
        }


        //public AnimConditions AnimConditions { get; set; }

      

        /// <summary>   
        /// 
        /// current model state flags.      
        /// </summary>
        public AnimConditions AnimConditions { get; set; }

        /// <summary>
        /// current state flags for structures, trees and other non-animated entities        
        /// </summary>
        //  private BitMask64 SpriteConditions;

        private double? nextSoundTimepoint;

        /// <summary>
        /// if the renderable needs to expire (be destroyed) after a set time, set this property
        /// </summary>
        public double? ExpiryTimePointInSeconds; // { get; set; }

        private double? timePointInSeconds;
        public double? TimePointInSeconds {
            get
            {
                return timePointInSeconds;
            }
        }

        public void SetNextTimepoint(double? timepoint)
        {
            this.timePointInSeconds = timepoint;
        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<Renderable>.Create();
        }


        private SleepyUpdaterID sleepyUpdater;
        public SleepyUpdaterID SleepyUpdater
        {
            get
            {
                return sleepyUpdater;
            }
            set
            {
                sleepyUpdater = value;
            }
        }

        private double? updateInterval;
        public double? UpdateInterval 
        {
            get
            {
                return updateInterval;
            }

            private set
            {
                if (!Common.IsEqual(updateInterval, value))
                {
                    updateInterval = value;

                    SleepyUpdater<Renderable> updater = LookUpSleepyUpdater<Renderable>.FindByID(SleepyUpdater);
                    if (updater != null) // is null after Load, but not after Save+Load???
                    {
                        updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
                    }

                }
            }
        
        }

        bool isOnScreen;
        public bool IsOnScreen
        {
            get
            {
                return isOnScreen;
            }
            set
            {
                if (value != isOnScreen)
                {
                    isOnScreen = value;

                    if (isOnScreen == true)
                    {
                        // wake up any sleeping renderables:
                        RecomputeUpdateInterval();
                    }
                }                
            }
        }
       
        /// <summary>
        /// not for rendering!
        /// </summary>
      /*  public Color DebugColor = new Color(The.Client.ClientRandomGenerator.Next(255, "Renderable", false),
                                        The.Client.ClientRandomGenerator.Next(255, "Renderable", false),
                                        The.Client.ClientRandomGenerator.Next(255, "Renderable", false));
        */

        #region Effects

        // are applied directly to the quad.Tint before drawing
        private Effect<Color> tintEffect; 
        private Effect<Animation2DPlayer> pulsingEffect; 
        #endregion

        // can be optionally applied during Draw
        public enum AdditionalEffect { Outline };
        private Dictionary<AdditionalEffect, Effect<Animation2DPlayer>> AdditionalEffects;
        private Dictionary<AdditionalEffect, Effect<Color>> additionalTintEffects;

        #region "Overlay" effects
        // are applied directly to the overlayquad.Tint before drawing

        // any effects that do not aplly to the renderable (billboard/model etc.) but rather in a separate draw pass can be added here
        // perhaps later we need a whole list instead of just one, since there could theoretically be many (status) effects applied to a renderable? Or perhaps those should be independent particle effects..
        private Effect<Color> overlayTintEffect;
        private Effect<Animation2DPlayer> overlayPulsingEffect;


        #endregion


        public AnimatedHead AnimatedHead;

        public List<RenderAsBillboard> RenderAsBillboard;

        public RenderAsModel RenderAsModel;

        public RenderAsIcon RenderAsIcon = null;

        public RenderAsGroundSprite RenderAsGroundSprite;

        public RenderAsConnectedGroundSprite RenderAsConnectedGroundSprite;


        public List<ParticleEmitter> ParticleEmitters;
   

        public List<LightSource> LightSources; 
       // public Lighting Lighting;


        public bool LerpableWasRenderedLastFrame = false;

        //ctor
        public Renderable(Entity entity, RenderableType type) 
        {
            RenderableType = type;
            Entity = entity;

            spriteConditions = new BitMask64(typeof(StateModifier));

            if (RenderableType.RenderAsIconType != null)
            {
                RenderAsIcon = new RenderAsIcon();
                RenderAsIcon.IconToRender = RenderableType.RenderAsIconType.IconToRender;
            }

            if (Parent != null && RenderableType.AnimatedHeadType != null) // Parent.EntityType.Person != null)
            {
                AnimatedHead = new AnimatedHead(this);
            }

            // RenderAsBillboard creation is now in Adopt()...


            if (RenderableType.RenderAsModelType != null)
            {              
                RenderAsModel = new RenderAsModel(Entity, this); // this);

                AnimConditions = new AnimConditions();  //?? only models need this...
                
            }

            if (RenderableType.RenderAsConnectedGroundSpriteType != null)
            {
                
                RenderAsConnectedGroundSprite = new RenderAsConnectedGroundSprite(Entity, this);
                //Renderable.Add(new RenderAsConnectedGroundSprite(this));
            }

            if (RenderableType.ParticleEmitterTypes != null)
            {
                foreach (var item in RenderableType.ParticleEmitterTypes)
                {
                    The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, this);
                }
            }

           
        }

        /// <summary>
        /// copy constructor for MemoryFact
        /// </summary>
        /// <param name="original"></param>
        public Renderable(Renderable original, MemoryFact memoryFact)
        {
            MemoryFact = memoryFact;

            InitCopy(original, memoryFact);

            if (MemoryFact.EntityType.RenderWithOverlayWhenMemoryFact())
            {
                renderEffect = RenderEffect.Memory;               
            }

        }

        /// <summary>
        /// copy constructor for Carcass
        /// </summary>
        /// <param name="original"></param>
        /// <param name="entity"></param>
        public Renderable(Renderable original, Entity entity)
        {
            Entity = entity;

            InitCopy(original, entity);


           // RecomputeUpdateInterval();
        }


        /// <summary>
        /// for loaded memoryfacts
        /// </summary>
        /// <param name="snapshotRenderable"></param>
        /// <param name="memoryFact"></param>
        public Renderable(MemoryFact memoryFact)
        {
            MemoryFact = memoryFact;

            RenderableType = memoryFact.EntityType.RenderableTypeMode;

           // InitCopy(original, memoryFact);

            if (MemoryFact.EntityType.RenderWithOverlayWhenMemoryFact())
            {
                renderEffect = RenderEffect.Memory;
            }
        }



        /// <summary>
        /// this breaks Sim/Client decoupling -  so only to be called before snapshotting!
        /// </summary>
        /// <returns></returns>
        public virtual SnapshotRenderable GetFieldsToSnapshot()
        {
            return new SnapshotRenderable(this);
            
        }

     /*   private void InitLoadedMemoryFact(SnapshotRenderable snapshotRenderable, IKnownEntityData entityData)
        {
            if (snapshotRenderable.RenderAsModel != null)
            {
               // this.AnimConditions = new Renderables.AnimConditions(original.AnimConditions);

                bool setAnimationFlagsDirty;
                if (entityData is MemoryFact)
                {
                    setAnimationFlagsDirty = false; // we can display the frozen pose of the original
                }
                else
                {
                    setAnimationFlagsDirty = true; // for carcass items, run the animation once to show the dead pose -also for loaded MemoryFacts??
                }

                RenderAsModel = new RenderAsModel(//original.RenderAsModel, 
                    original.RenderAsModel.CustomColor0, original.RenderAsModel.CustomColor1, original.RenderAsModel.CustomColor2, original.RenderAsModel.CustomColor3,
                    original.RenderAsModel.LocalTransform,
                    original.RenderAsModel.Location,
                    original.RenderAsModel.FacingNormal,
                    original.RenderAsModel.FinalModelName,
                    original.RenderAsModel.FinalModelBasicTextureName,
                    original.RenderAsModel.FinalModelScale,
                    original.RenderAsModel.AnimatedModel,
                    this, setAnimationFlagsDirty, entityData);

                // RenderAsModel.Parent = entityData;

            }

        }*/

        private void InitCopy(Renderable original, IKnownEntityData entityData)
        {                        
            location = original.location;
            
            RenderableType = original.RenderableType;

            spriteConditions = new BitMask64(original.spriteConditions);

            if (original.RenderAsModel != null)
            {
                this.AnimConditions = new Renderables.AnimConditions(original.AnimConditions);

                bool setAnimationFlagsDirty;
                if (entityData is MemoryFact)
                {
                    setAnimationFlagsDirty = false; // we can display the frozen pose of the original
                }
                else
                {
                    setAnimationFlagsDirty = true; // for carcass items, run the animation once to show the dead pose -also for loaded MemoryFacts??
                }

                RenderAsModel = new RenderAsModel(//original.RenderAsModel, 
                    original.RenderAsModel.CustomColor0, original.RenderAsModel.CustomColor1, original.RenderAsModel.CustomColor2, original.RenderAsModel.CustomColor3,
                    original.RenderAsModel.LocalTransform,
                    original.RenderAsModel.Location,
                    original.RenderAsModel.FacingNormal,
                    original.RenderAsModel.FinalModelName,
                    original.RenderAsModel.FinalModelBasicTextureName,
                    original.RenderAsModel.FinalModelScale,
                    original.RenderAsModel.AnimatedModel,
                    this, setAnimationFlagsDirty, entityData);

               // RenderAsModel.Parent = entityData;
                
            }

            /* create the sprites when drawn...
            if (original.RenderAsGroundSprite != null)
            {
                RenderAsGroundSprite = new RenderAsGroundSprite(original.RenderAsGroundSprite, this);
                RenderAsGroundSprite.Parent = entityData;
            }

            if (original.RenderAsBillboard != null && original.RenderAsBillboard.Count > 0)
            {
                RenderAsBillboard = new List<RenderAsBillboard>();

                RenderAsBillboard billboard;
                foreach (var item in original.RenderAsBillboard)
                {
                    billboard = new RenderAsBillboard(item);
                    billboard.Parent = this;
                    RenderAsBillboard.Add(billboard);

                }
            }*/

            if (original.RenderAsConnectedGroundSprite != null)
            {
                RenderAsConnectedGroundSprite = new RenderAsConnectedGroundSprite(original.RenderAsConnectedGroundSprite, this);
                RenderAsConnectedGroundSprite.Parent = entityData;
            }

            IsDrawn = original.IsDrawn;
        }


        private void UpdateSettingsFromEntity()
        {
            if (Entity != null)
            {
                // with the new late creation of Renderable,
                // pull all the settings we need for a new renderable:

                // the Update methods need to check for a renderable, we don't want their calls to be swallowed by a stub, at least not until the refactor is complete...
                // later, we could add a renderable stub at the same time as the normal renderable would have been created. Then replace it when moving on-site
                FlipHorizontally = Entity.FlipHorizontally;

                if (Entity.EntityType.TreeType != null)
                {
                    Tree tree;
                    Entity.Find(out tree);
                    tree.UpdateRenderable();
                }
                
                if (Entity.EntityType.StructureType != null)
                {
                    Entity.Structure.UpdateRenderable();
                }

                if (Entity.EntityType.BodyType != null)
                {
                    Entity.Body.UpdateRenderable();
                }

                if (Entity.EntityType.NonLivingType != null)
                {
                    Entity.NonLivingEntity.UpdateRenderable();
                }

                if (Entity.EntityType.StructureType != null)
                {
                    Entity.Structure.UpdateRenderable();
                }
                
            }
        }


        public void Initialize(bool updatePropertiesFromEntity)
        {
            if (updatePropertiesFromEntity) // retrieves (default) model properties from the entity. don't do this when loading.
            {
                UpdateSettingsFromEntity();

                if (RenderAsModel != null)
                {
                    UpdateModelSettingsFromEntity();
                }
            }

            if (RenderAsModel != null)
            {               

                RenderAsModel.Initialize();

                if (snapshotAttachables != null)
                {
                    foreach (var item in snapshotAttachables)
                    {
                        AttachPoint attachor = RenderAsModel.ModelData.GetAttachPointFromTag(item.Item2);
                        AttachPooledObjectIfPossible(item.Item1, attachor, item.Item3, 
                            false);   // don't save again.. 
                    }                   
                }

            }
          

            RecomputeUpdateInterval();
        }

        private void UpdateModelSettingsFromEntity()
        {
            // carcasses that have never lived (for instance event spawned items) can have textures and scales defined too...
            RenderAsModel.FinalModelName = RenderableType.RenderAsModelType.AssetName;
            RenderAsModel.FinalModelScale = RenderableType.RenderAsModelType.ModelScale;
            RenderAsModel.FinalModelBasicTextureName = RenderableType.RenderAsModelType.ModelBasicTextureName;


            if (Parent != null)
            {
                if (Parent.EntityType.Person != null)
                {
                    // possibly enable randomness again in the future...
                    Parent.EntityType.Person.UpdateRenderableRandomColors(this);
                }

                // bio type can override the above:
                if (Parent.EntityType.BiologicalType != null)
                {
                    BiologicalEntity bioEntity = Entity.BiologicalEntity;

                    RenderAsModel.CustomColor3 = Parent.EntityType.BiologicalType.GetPrimaryColor(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);
                    RenderAsModel.CustomColor2 = Parent.EntityType.BiologicalType.GetSecondaryColor(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);

                    //No randomness for now...
                    RenderAsModel.CustomColor1 = Parent.EntityType.BiologicalType.GetTertiaryColor(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);
                    RenderAsModel.CustomColor0 = Parent.EntityType.BiologicalType.GetQuaternaryColor(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);


                    string modelName = Parent.EntityType.BiologicalType.GetModelName(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        RenderAsModel.FinalModelName = modelName;
                    }

                    // TODO: caste or race scale should not be required for age.scalefraction to have an effect.
                    RenderAsModel.FinalModelScale = Parent.EntityType.BiologicalType.GetModelScale(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age, RenderAsModel.FinalModelScale);
                   
                    string modelTextureName;

                    if (!string.IsNullOrEmpty(bioEntity.ModelTextureName))
                    {
                        modelTextureName = bioEntity.ModelTextureName;
                    }
                    else
                    {
                        modelTextureName = Parent.EntityType.BiologicalType.GetModelBasicTexture(bioEntity.CasteType, bioEntity.RaceType, bioEntity.AgeGroup.Age);
                    }

                    if (!string.IsNullOrEmpty(modelTextureName))
                    {
                        RenderAsModel.FinalModelBasicTextureName = modelTextureName;
                    }
                    else
                    {

                    }
                }
            }
        }


        #region Anim state flag matching



        public void SetAnimationActionStateFlag(AnimAction state)
        {
            if (AnimConditions == null)
                return;

            if (AnimConditions.Action == state)
                return;


            AnimConditions.Action = state;

            SetAnimFlagsDirty();
            //  animationFlagsAreDirty = true;
        }

        public void SetAnimationStateFlags(AnimModifier[] states)
        {
            if (AnimConditions == null)
                return;

            if (states != null)
            {
                foreach (var item in states)
                {
                    SetAnimationStateFlag(item);
                }
            }
        }

        public void ClearAnimationStateFlags(AnimModifier[] states)
        {
            if (AnimConditions == null)
                return;

            if (states != null)
            {
                foreach (var item in states)
                {
                    ClearAnimationStateFlag(item);
                }
            }
        }

        public void SetAnimationStateFlag(AnimModifier state)
        {
            if (AnimConditions == null)
                return;

            if (AnimConditions.Modifiers.Test(state))
            {
                return; // no point setting a true bit to true
            }

            AnimConditions.Modifiers.Set(state);


            //apply radio toggles
            switch (state)
            {

                //many of the modifier states are each other's opposites
                case AnimModifier.High:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Low); break; }
                case AnimModifier.Low:
                    { AnimConditions.Modifiers.Clear(AnimModifier.High); break; }


                case AnimModifier.Fast:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Slow); break; }
                case AnimModifier.Slow:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Fast); break; }


                case AnimModifier.Left:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Right); break; }
                case AnimModifier.Right:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Left); break; }


                case AnimModifier.Near:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Far); break; }
                case AnimModifier.Far:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Near); break; }


                case AnimModifier.Happy:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Trouble); break; }
                case AnimModifier.Trouble:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Happy); break; }


                case AnimModifier.Pre:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Post); break; }
                case AnimModifier.Post:
                    { AnimConditions.Modifiers.Clear(AnimModifier.Pre); break; }

                case AnimModifier.Sitting:
                    {
                        AnimConditions.Modifiers.Clear(AnimModifier.Kneeling);
                        AnimConditions.Modifiers.Clear(AnimModifier.Lying);
                        break;
                    }
                case AnimModifier.Kneeling:
                    {
                        AnimConditions.Modifiers.Clear(AnimModifier.Sitting);
                        AnimConditions.Modifiers.Clear(AnimModifier.Lying);
                        break;
                    }
                case AnimModifier.Lying:
                    {
                        AnimConditions.Modifiers.Clear(AnimModifier.Sitting);
                        AnimConditions.Modifiers.Clear(AnimModifier.Kneeling);
                        break;
                    }

                case AnimModifier.Spear:
                case AnimModifier.Rifle:
                case AnimModifier.Watergun:
                case AnimModifier.Hammer:
                case AnimModifier.Knife:
                case AnimModifier.Machete:
                case AnimModifier.Axe:
                case AnimModifier.Pickaxe:
                case AnimModifier.Bow:
                case AnimModifier.Hoe:
                case AnimModifier.Shovel:
                    {
                        MutexOtherItemStates(state);
                        break;
                    }

            }

            SetAnimFlagsDirty();
            // animationFlagsAreDirty = true;

        }



        public void ClearAnimationActionStateFlag(AnimAction state)
        {
            if (AnimConditions == null)
                return;

            if (AnimConditions.Action != state)
                return; // no point clearing a false bit to false


            AnimConditions.Action = null; // ??? set to idle? or prevent anim matching until it has been set...?


            SetAnimFlagsDirty();
            //animationFlagsAreDirty = true;

        }


        public void ClearAnimationStateFlag(AnimModifier state)
        {
            if (AnimConditions == null)
                return;

            if (AnimConditions.Modifiers == null || !AnimConditions.Modifiers.Test(state))
            {
                return; // no point clearing a false bit to false
            }


            AnimConditions.Modifiers.Clear(state);

            SetAnimFlagsDirty();

            //  animationFlagsAreDirty = true;

        }

        /*protected void ConstructConditionFlags() //Type t)
        {
            AnimConditions = new AnimConditions(); // { Modifiers = new BitMask64(t) };
        }*/

        private void MutexOtherItemStates(AnimModifier state)
        {
            if (state != AnimModifier.Spear)
                AnimConditions.Modifiers.Clear(AnimModifier.Spear);

            if (state != AnimModifier.Rifle)
                AnimConditions.Modifiers.Clear(AnimModifier.Rifle);

            if (state != AnimModifier.Knife)
                AnimConditions.Modifiers.Clear(AnimModifier.Knife);

            if (state != AnimModifier.Machete)
                AnimConditions.Modifiers.Clear(AnimModifier.Machete);

            if (state != AnimModifier.Axe)
                AnimConditions.Modifiers.Clear(AnimModifier.Axe);

            if (state != AnimModifier.Pickaxe)
                AnimConditions.Modifiers.Clear(AnimModifier.Pickaxe);

            if (state != AnimModifier.Watergun)
                AnimConditions.Modifiers.Clear(AnimModifier.Watergun);

            if (state != AnimModifier.Bow)
                AnimConditions.Modifiers.Clear(AnimModifier.Bow);

            if (state != AnimModifier.Hammer)
                AnimConditions.Modifiers.Clear(AnimModifier.Hammer);

            if (state != AnimModifier.Hoe)
                AnimConditions.Modifiers.Clear(AnimModifier.Hoe);

            if (state != AnimModifier.Shovel)
                AnimConditions.Modifiers.Clear(AnimModifier.Shovel);

        }


        #endregion



        #region Static state flag matching



        private bool spriteFlagsAreDirty = true;

        /// <summary>
        /// current state flags for structures, trees and other non-animated entities        
        /// </summary>
        private BitMask64 spriteConditions;
        
        private ClientStateInfo selectedSpriteInfo;


        /// <summary>
        /// the static info that was selected as the best match to the entity's state flags.
        /// 
        /// This will select and assign the best match if the state is dirty.
        /// 
        /// In addition, billboards and ground sprites must be recalculated (set as dirty?)
        /// if position or flipped status changes...
        /// </summary>
        public ClientStateInfo SelectedSpriteInfo
        {
            get
            {
               

                if (spriteFlagsAreDirty || selectedSpriteInfo == null)
                {
                   
                    ReplaceSpriteConditionState();
                    spriteFlagsAreDirty = false;
                }

                return selectedSpriteInfo;
            }

            private set
            {
                selectedSpriteInfo = value;
            }
        }

       
       /*  public virtual void UpdateStaticConditionState()
         {
            
             if (spriteFlagsAreDirty)
             {
                 ReplaceSpriteConditionState();
                 spriteFlagsAreDirty = false;
             }
         }*/
        

        private void ReplaceSpriteConditionState()
        {           
            //do the magical best match trick, on the current bits
            //this retrieves a reference to the read-only data defined in RenderableType



            IStateInfo info = null;
            RenderableType.FindBestStaticInfo(spriteConditions, RenderableType.ClientStateConditions, RenderableType.DefaultClientState, out info);


            if (info != null)
            {
                AdoptSpriteInfo((ClientStateInfo)info);
            }
        }

       
        private void AdoptSpriteInfo(ClientStateInfo newInfo)
        {

            if (newInfo == null)
            {
                newInfo = RenderableType.DefaultClientState; //default is always good
            }

            ClientStateInfo oldInfo = selectedSpriteInfo;
            SelectedSpriteInfo = newInfo;

            if (oldInfo != selectedSpriteInfo)
            {
                AdoptSpriteInfo(selectedSpriteInfo, oldInfo);
            }

        }

        public void SetOrClearSpriteStateFlag(bool setFlag, StateModifier state)
        {
            if (setFlag)
            {
                SetSpriteStateFlag(state);
            }
            else
            {
                ClearSpriteStateFlag(state);
            }
        }

        /// <summary>
        /// call via Entity
        /// </summary>
        /// <param name="state"></param>
        public void SetSpriteStateFlag(StateModifier state)
        {
         
            bool flagsWereChanged = SetSpriteStateFlag(spriteConditions, state);

            spriteFlagsAreDirty = spriteFlagsAreDirty || flagsWereChanged;

           

            //spriteFlagsAreDirty = true;
        }

        public static bool SetSpriteStateFlag(BitMask64 spriteConditions, StateModifier state)
        {

            if (spriteConditions.Test(state))
            {
                return false; // no point setting a true bit to true
            }

            spriteConditions.Set(state);


            //apply radio toggles
            switch (state)
            {
                //many of the modifier states are each other's opposites
                case StateModifier.BeingBuilt:
                    spriteConditions.Clear(StateModifier.Ordered);
                    break;

                case StateModifier.Less:
                    spriteConditions.Clear(StateModifier.More);
                    break;

                case StateModifier.More:
                    spriteConditions.Clear(StateModifier.Less);
                    break;

                case StateModifier.HalfFull:
                    spriteConditions.Clear(StateModifier.Full);
                    break;

                case StateModifier.Full:
                    spriteConditions.Clear(StateModifier.HalfFull);
                    break;

                case StateModifier.Flavour1:
                case StateModifier.Flavour2:
                case StateModifier.Flavour3:
                case StateModifier.Flavour4:
                case StateModifier.Flavour5:
                case StateModifier.Flavour6:
                case StateModifier.Flavour7:
                    {
                        MutexOtherFlavourStates(spriteConditions, state);
                        break;
                    }

            }

            return true;

           // spriteFlagsAreDirty = true;

        }

        /// <summary>
        /// call via Entity if possible
        /// </summary>
        /// <param name="state"></param>
        public void ClearSpriteStateFlag(StateModifier state)
        {
            if (ClearSpriteStateFlag(spriteConditions, state))
            {
                spriteFlagsAreDirty = true;               
            }
        }

        public static bool ClearSpriteStateFlag(BitMask64 spriteConditions, StateModifier state)
        {

            if (!spriteConditions.Test(state))
            {
                return false; // no point clearing a false bit to false
            }


            spriteConditions.Clear(state);

           // spriteFlagsAreDirty = true;
            return true;

        }

        private static void MutexOtherFlavourStates(BitMask64 spriteConditions, StateModifier state)
        {
            if (state != StateModifier.Flavour1)
                spriteConditions.Clear(StateModifier.Flavour1);

            if (state != StateModifier.Flavour2)
                spriteConditions.Clear(StateModifier.Flavour2);

            if (state != StateModifier.Flavour3)
                spriteConditions.Clear(StateModifier.Flavour3);

            if (state != StateModifier.Flavour4)
                spriteConditions.Clear(StateModifier.Flavour4);

            if (state != StateModifier.Flavour5)
                spriteConditions.Clear(StateModifier.Flavour5);

            if (state != StateModifier.Flavour6)
                spriteConditions.Clear(StateModifier.Flavour6);

            if (state != StateModifier.Flavour7)
                spriteConditions.Clear(StateModifier.Flavour7);

        }

        #endregion


        /// <summary>
        /// only call this from ParticleManager - never from Sim
        /// </summary>
        /// <param name="emitter"></param>
        public void AddParticleEmitter(ParticleEmitter emitter)
        {
            if (ParticleEmitters == null)
            {
                ParticleEmitters = new List<ParticleEmitter>();
            }

          //  emitter.Parent = this;

            ParticleEmitters.Add(emitter);

           
        }


        /// <summary>
        /// only call this from ParticleManager - never from Sim
        /// </summary>
        /// <param name="emitter"></param>
        public void RemoveParticleEmitters(string systemKey) // ParticleEmitter emitter)
        {
            if (ParticleEmitters != null)
            {
                ParticleEmitters.RemoveAll(p => p.System.ParticleSystemType.KeyName == systemKey);

            }
        }

        /// <summary>
        /// only call this from ParticleManager - never from Sim
        /// </summary>
        /// <param name="emitter"></param>
        public void RemoveParticleEmitter(ParticleEmitter emitter) // string systemKey) // ParticleEmitter emitter)
        {
            if (ParticleEmitters != null)
            {
                ParticleEmitters.Remove(emitter);
            }
        }

        public void AdoptParticleEffects(ParticleEmitterEffect[] newEffects, ParticleEmitterEffect[] oldEffects) //AnimConditionInfo oldInfo)
        {
            //setup any particle effects associated with animationstates. Also stop the old ones, if different
            //if the current particle systems are of same type, then reassign them to current info

            if (oldEffects != null) //oldInfo != null && oldInfo.ParticleEmitters != null)
            {
                foreach (var item in oldEffects) // oldInfo.ParticleEmitters)
                {
                    // remove old effects:
                    RemoveParticleEmitters(item.ParticleSystemKey);

                }
            }

            if (newEffects != null) // SelectedAnimInfo.ParticleEmitters != null)
            {
                // start new ones:
                foreach (var item in newEffects) // SelectedAnimInfo.ParticleEmitters)
                {
                    The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, this, null, null, item.EmitParticlesInParentDirection, offset: item.Offset);
                }
            }

        }

        private void AdoptSpriteInfo(ClientStateInfo newInfo, ClientStateInfo oldInfo)
        {
           
            AdoptParticleEffects(newInfo.ParticleEmitters, oldInfo != null ? oldInfo.ParticleEmitters : null);

            AdoptBillboards(newInfo);

            AdoptGroundSprite(newInfo);

            AdoptLighting(newInfo);

            AdoptSound(newInfo);

        }

        private void AdoptBillboards(ClientStateInfo newInfo)
        {
           
            // see if we need to construct additional billboards 
            if (newInfo.RenderAsBillboardType != null)
            {
                if (RenderAsBillboard == null)
                {
                    RenderAsBillboard = new List<RenderAsBillboard>();
                }
   
                // create instances on the fly if needed by newInfo..
                while (RenderAsBillboard.Count < newInfo.RenderAsBillboardType.Length)
                {
                    RenderAsBillboard billboard = new RenderAsBillboard(this, null);                                       

                    RenderAsBillboard.Add(billboard);
                }

                // other option: The.Client.Renderer.GhostedStructuresSpriteSheet
                SpriteSheet spriteSheet = GameData.Instance.BillboardSpriteSheet;

                bool drawAsOverlay = newInfo.Test(StateModifier.Ordered) // HACK!!! but must match up with the sprite sheet selection done by GameWorldRenderer...
                     || DrawAsOverlay;

                if (drawAsOverlay)  
                {
                    spriteSheet = The.Client.Renderer.GhostedStructuresSpriteSheet;
                }

                //assign the sprites and other data:
                RenderAsBillboardType billboardType;
                for (int i = 0; i < newInfo.RenderAsBillboardType.Length; i++)
                {
                    RenderAsBillboard billboard = RenderAsBillboard[i];
                    billboardType = newInfo.RenderAsBillboardType[i];

                    billboard.Redraw(spriteSheet, billboardType, drawAsOverlay);
                                       

                    billboard.ComputeMapPosition(); // GameWorldRenderer requires the correct MapPosition that contains the billboard, as well as the location for sorting.
                    
                }

                // hide any billboards which are not in the new state:
                int noOfExistingBillboards = RenderAsBillboard.Count;
                for (int i = newInfo.RenderAsBillboardType.Length; i < noOfExistingBillboards; i++)
                {
                    RenderAsBillboard.RemoveAt(i); // NEW: remove them..                    
                }

            }
            else
            {
                // new state has no billboards: hide any existing quads:
                if (RenderAsBillboard != null)
                {
                    RenderAsBillboard.Clear(); // NEW: remove them..
                    RenderAsBillboard = null;
                    
                }
            }

            // in case billboard animations were added or removed, we may require more/fewer updates:
            RecomputeUpdateInterval();

        }

        public void PrintStaticStates(System.Text.StringBuilder states)
        {
            if (spriteConditions != null)
            {
                states.Append("\nActual SpriteStateFlags:\n");
                states.Append(spriteConditions.StateNames);

                if (selectedSpriteInfo != null)
                {
                    states.Append("\nBest Match Conditions: ");
                    if (spriteConditions.Equals(selectedSpriteInfo.Conditions))
                    {
                        states.Append("PERFECT");
                    }
                    states.Append("\n");

                    if (selectedSpriteInfo.Conditions != null)
                    {
                        states.Append(selectedSpriteInfo.Conditions.StateNames);
                    }

                    if (selectedSpriteInfo.Forbiddens != null)
                    {
                        if (selectedSpriteInfo.Forbiddens.Any())
                        {
                            states.Append("\nBest Match Forbiddens:\n     ");
                            states.Append(selectedSpriteInfo.Forbiddens.StateNames);
                        }
                    }

                    states.Append("\n");

                    //   renderable.RenderAsModel.AppendAnimDebugInfo(/*keyname,*/ states);

                }
                else
                    states.Append("\n\n   MATCH FAILED -- selectedSpriteInfo is null -- this is bad.\n\n");
            }
        }

        public virtual void LocationChanged()
        {
            SetSpriteQuadsDirty();
                       
            if (RenderAsBillboard != null)
            {
                for (int i = 0; i < RenderAsBillboard.Count; i++)
                {
                    // redraw with the new position:
                    RenderAsBillboard[i].ComputeMapPosition(); // GameWorldRenderer requires the correct MapPosition that contains the billboard, as well as the location for sorting.                

                }
            }

        }

        private void AdoptSound(ClientStateInfo newInfo)
        {
            
            // to play a new random sound instead of looping continuously, we have to turn off looping and catch the event after the sound has stopped playing.     
            // then, add a random delay
            // then play a new random sound

            Looping looping = Looping.No;

            SoundData newSound = GetRandomSound();

            // this method is also called from RenderAsModel to play animation state sounds. But we should never be using sounds from RenderAsModel and StaticInfo at the same time...
            AdoptStateSound(newSound, null /* oldInfo != null ? oldInfo.SoundData : null*/, looping, 
                PickNewRandomSound); 

        }

        public void PickNewRandomSound(/*SoundData*/ PlayedSound soundThatEnded) // int soundIndex) // object sender, EventArgs e)
        {
            // don't restart if looping!! the MO for ambient renderables seem to be that looping is achieved by calling this forever.
            // does this happen when the renderable is off-screen?

            soundThatEnded.SoundEndedEvent -= PickNewRandomSound; 

           
            // check that we are still in the same set:
            if (selectedSpriteInfo.SoundDatas != null && selectedSpriteInfo.SoundDatas.Any(s => s.KeyName == soundThatEnded.SoundData.KeyName))
            {
                Looping looping = Looping.No;

                if (selectedSpriteInfo.DelayBetweenSounds != null)
                {
                    // insert a delay
                    nextSoundTimepoint = UpdateTimePoints.ComputeTimePointFromInterval(
                        selectedSpriteInfo.DelayBetweenSounds.GetRandomValue(The.Client.ClientRandomGenerator));

                    RecomputeUpdateInterval();
                }
                else 
                {
                    nextSoundTimepoint = null;

                    SoundData newSound = GetRandomSound();

                    AdoptStateSound(newSound /*newInfo.SoundData*/, null /* oldInfo != null ? oldInfo.SoundData : null*/, looping,
                            PickNewRandomSound); 
                }
            }
        }

        private SoundData GetRandomSound()
        {
            if (selectedSpriteInfo.SoundDatas != null)
            {
                return Common.GetRandomListMember(selectedSpriteInfo.SoundDatas, The.Client.ClientRandomGenerator);
            }
            else return null;
                  
        }

        private void AdoptLighting(ClientStateInfo newInfo)
        {
            if (newInfo.LightingTypes != null)
            {
                if (LightSources == null)
                {
                    LightSources = new List<LightSource>();
                }

                // create instances on the fly if needed by newInfo..
               /* while (LightSources.Count < newInfo.LightingTypes.Length)
                {
                    LightSource billboard = new LightSource(this, null);

                    LightSources.Add(billboard);
                }*/

                foreach (var item in newInfo.LightingTypes)
                {
                    if (!LightSources.Exists(l => l.LightingType == item))
                    {
                        var l = new LightSource(item, this);
                        LightSources.Add(l);
                        
                    }
                }

                LightSources.RemoveAll(p => !newInfo.LightingTypes.Any(l => l == p.LightingType));

                //assign the sprites and other data:
               /* RenderAsBillboardType billboardType;
                for (int i = 0; i < newInfo.RenderAsBillboardType.Length; i++)
                {
                    RenderAsBillboard billboard = RenderAsBillboard[i];
                    billboardType = newInfo.RenderAsBillboardType[i];

                    billboard.Redraw(spriteSheet, billboardType, drawAsOverlay);


                    billboard.ComputeMapPosition(); // GameWorldRenderer requires the correct MapPosition that contains the billboard, as well as the location for sorting.

                }*/

            }
            else
            {
                // remove existing lighting:
                if (LightSources != null)
                {
                    LightSources = null;
                    //RenderAsGroundSprite.Redraw(null);
                }
            }
        }

        private void AdoptGroundSprite(ClientStateInfo newInfo)
        {
            if (newInfo.RenderAsGroundSpriteType != null)
            {
                if (RenderAsGroundSprite == null)
                {
                    RenderAsGroundSprite = new RenderAsGroundSprite(Entity, newInfo.RenderAsGroundSpriteType, this);
                }
                
                if (RenderAsGroundSprite != null)
                {
                    Rectangle? rectangle;
                    string spriteName;

                    spriteName = newInfo.RenderAsGroundSpriteType.AssetName;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        rectangle = The.Client.FlatSpriteSheet.GetSourceRectangle(spriteName);                  
                    }
                    else
                    {
                        // clear the ground sprite:
                        rectangle = null;
                    }

                    RenderAsGroundSprite.Redraw(rectangle);
                }
            }
            else
            {
                // remove existing ground sprite:
                if (RenderAsGroundSprite != null)
                {
                    RenderAsGroundSprite = null;
                    //RenderAsGroundSprite.Redraw(null);
                }
            }
        }
        
       
        /// <summary>
        /// null will stop any state sound playing
        /// </summary>
        /// <param name="newSound"></param>
        /// <param name="oldSound"></param>
        /// <param name="loopSound"></param>
        public void AdoptStateSound(SoundData newSound, SoundData oldSound, Looping loopSound, Action<PlayedSound> soundEndedCallback = null) 
        {
            //setup any sounds associated with animationstates. Also stop the old ones, if different

            if (newSound != null && newSound == oldSound)
                return;

            if (newSound != null)
            {

            }

            if (StateSoundPlaying != null 
                && StateSoundPlaying.Item2 != null 
                && (newSound == null || newSound.PlayMaxOneInstance == false || StateSoundPlaying.Item1 != newSound) // don't stop the sound if we are going to start it now... (for grouped sounds, another renderable may just have been here and started playing it)
                && StateSoundPlaying.Item2.State == SoundState.Playing) 
            {
                The.Client.AudioManager.StopSound(StateSoundPlaying.Item1, StateSoundPlaying.Item2);
               
                StateSoundPlaying = null;
            }

            if (newSound != null) 
            {
                SoundEffectInstance instance = The.Client.AudioManager.PlayWorldSound(newSound, new WorldLocation(Location.Value), fadeProgress, loopSound == Looping.Yes, soundEndedCallback);
                if (instance != null)
                {
                    StateSoundPlaying = new Tuple<SoundData, SoundEffectInstance>(newSound, instance);
                }
                else
                {
                    StateSoundPlaying = null;
                }
              
            }
        }


       
        /*
        public virtual void PlaceLightSources(Vector2 baseCenter)
        {
            if (Lighting != null)
                Lighting.PlaceLightSources(baseCenter);
        }

        public virtual void FlipLightingHorizontally(bool value)
        {
            if (Lighting != null)
                Lighting.FlipHorizontally = value;
            
        }
        */

        /// <summary>
        /// since these sounds are typically short, I think we should optimize a bit and skip starting them entirely if the renderable is not seen. (although the renderable could get in view before the sound had stopped playing..
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="oldSound"></param>
        public virtual void PlayActionSound(SoundData sound)
        {
            IKnownEntityData entityData;
            if (Parent != null 
                && The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Parent.EntityID, out entityData) != EntityResult.SeenDirectly)
            {
                return; // state sound is only started when the renderable is drawn... this code achieves the same
            }


            if (ActionSoundsPlaying == null)
                ActionSoundsPlaying = new List<Tuple<SoundData, SoundEffectInstance>>();

           // clean up any stopped sounds here:
            for (int i = ActionSoundsPlaying.Count - 1; i >= 0; i--)
            {
                if (ActionSoundsPlaying[i].Item2 != null && ActionSoundsPlaying[i].Item2.State == SoundState.Stopped)
                {
                    ActionSoundsPlaying.RemoveAt(i);
                }
            }

            SoundEffectInstance newSoundInstance = The.Client.AudioManager.PlayWorldSound(sound, new WorldLocation(Location.Value), fadeProgress, false);
            if (newSoundInstance != null)
            {
                ActionSoundsPlaying.Add(new Tuple<SoundData, SoundEffectInstance>(sound, newSoundInstance));
            } 
        }


        public bool CanLerpLocation()
        {
            if (this.Entity != null
                && Entity.EntityType.LocomotorType != null // only lerp movers
                && Entity.IsStarted() != false) // don't lerp structures being placed
            {
                return true;
            }

            return false;
        }
       

        #region Fading
        public bool CanFade()
        {
            // TODO make a method that tests each sound, and also tests for IsDisposed
            return RenderableType.CanFade()
                || (this.StateSoundPlaying != null && this.StateSoundPlaying.Item2.State == SoundState.Playing)
               // || (this.AmbientSoundPlaying != null && this.AmbientSoundPlaying.Item2.State == SoundState.Playing)
                || (this.ActionSoundsPlaying != null && this.ActionSoundsPlaying.Count > 0 && this.ActionSoundsPlaying.Exists(s => s.Item2.State == SoundState.Playing))
                || this.ParticleEmitters != null;
        }

        public virtual void FadeIn()
        {
           
            if (fadeStatus == FadeStatus.None && fadeProgress >= 1f)
                return; // already fully faded in. is also the case on startup...

            if (destroyAfterFadeOut) // we won't go back now
                return;

            if (fadeStatus != FadeStatus.FadeIn)
            {
                fadeStatus = FadeStatus.FadeIn;

                RecomputeUpdateInterval();
            }
        }


        /// <summary>
        /// we use fading:
        /// 1. when an object dissappears in the fog of war (particle emitters and sounds)
        /// 2. When an object is destroyed (particle emitters mostly)
        /// </summary>
        /// <param name="destroyAfterFadeOut"></param>
        public virtual void FadeOut(bool destroyAfterFadeOut = false)
        {
          
            if (this.fadeStatus != FadeStatus.FadeOut || this.destroyAfterFadeOut != destroyAfterFadeOut)
            {
                this.fadeStatus = FadeStatus.FadeOut;

                this.destroyAfterFadeOut = destroyAfterFadeOut;

                RecomputeUpdateInterval();
            }
        }

        #endregion


        #region Pulsing


        public bool IsFlashing(AdditionalEffect effectID)
        {
            Effect<Animation2DPlayer> effect;
           
            if (AdditionalEffects.TryGetValue(effectID, out effect))
            {
                return effect.GetNumberOfEnvelopes() > 0;
            }

            return false;
        }


        public bool IsResourceContainerFlashing()
        {
          
            if (RenderAsIcon != null)
            {
                // tile resource icons
                return pulsingEffect != null &&
                    pulsingEffect.GetValue() == The.InGameUI.SelectedCyclePlayerFlashing;
            }
            else
            {
                // outlines on ground sprites and billboards:
                Effect<Animation2DPlayer> effect;
                if (AdditionalEffects != null 
                    && AdditionalEffects.TryGetValue(AdditionalEffect.Outline, out effect))
                {
                    return effect.GetValue() == The.InGameUI.SelectedCyclePlayerFlashing;
                }
            }

            return false;
        }

       

        public virtual void SetPulsing()
        {
            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayer;
            if (pulsingEffect == null)
            {
                pulsingEffect = new Effect<Animation2DPlayer>(this, animation2DPlayer);
            }
            else
            {
                pulsingEffect.SetDefaultValue(animation2DPlayer);
            }

            RecomputeUpdateInterval();
        }


        public virtual void SetPulsing(AdditionalEffect effectID)
        {
            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayer;

            if (AdditionalEffects == null)
            {
                AdditionalEffects = new Dictionary<AdditionalEffect,Effect<Animation2DPlayer>>();
            }
            Effect<Animation2DPlayer> effect;

            if (!AdditionalEffects.TryGetValue(effectID, out effect))
            {
                effect = new Effect<Animation2DPlayer>(this, animation2DPlayer);
                AdditionalEffects.Add(effectID, effect);
            }
            else 
            {
                effect.SetDefaultValue(animation2DPlayer);
            }


            RecomputeUpdateInterval();
        }

        public virtual void SetFlashing(AdditionalEffect effectID, float duration)
        {            
            if (AdditionalEffects == null)
            {
                AdditionalEffects = new Dictionary<AdditionalEffect, Effect<Animation2DPlayer>>();
            }

            Effect<Animation2DPlayer> effect;
            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayerFlashing;

            if (!AdditionalEffects.TryGetValue(effectID, out effect))
            {
                effect = new Effect<Animation2DPlayer>(this, animation2DPlayer);
                AdditionalEffects.Add(effectID, effect);
            }
      /*      else
            {
                effect.SetDefaultValue(animation2DPlayer);
            }*/

            effect.AddNewEnvelope(duration, animation2DPlayer);
           

            RecomputeUpdateInterval();
        }

        public virtual void SetOverlayPulsing()
        {
            
            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayer;
            if (overlayPulsingEffect == null)
            {
                overlayPulsingEffect = new Effect<Animation2DPlayer>(this, animation2DPlayer);
            }
            else
            {
                overlayPulsingEffect.SetDefaultValue(animation2DPlayer);
            }

            RecomputeUpdateInterval();
        }

        /// <summary>
        /// flashing is achieved by overriding the default pulsing anim with one that goes quicker for a short while
        /// </summary>
        /// <param name="duration"></param>
        public virtual void SetFlashing(float duration)
        {
            if (pulsingEffect == null)
            {
                pulsingEffect = new Effect<Animation2DPlayer>(this, The.InGameUI.SelectedCyclePlayer); // default..?
            }

            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayerFlashing; 
            pulsingEffect.AddNewEnvelope(duration, animation2DPlayer);
            

            RecomputeUpdateInterval();
        }

        public virtual void SetOverlayFlashing(float duration)
        {
            if (overlayPulsingEffect == null)
            {
                overlayPulsingEffect = new Effect<Animation2DPlayer>(this, The.InGameUI.SelectedCyclePlayer); // default..?
              //  overlayPulsingEffect = new Effect<Animation2DPlayer>(The.InGameUI.SelectedCyclePlayerQuick); 
            }

            Animation2DPlayer animation2DPlayer = The.InGameUI.SelectedCyclePlayerFlashing;
            overlayPulsingEffect.AddNewEnvelope(duration, animation2DPlayer);
            

            RecomputeUpdateInterval();
        }

        public virtual void SetResourceContainerTintColor(Color color)
        {
            if (RenderAsIcon != null)
            {
                SetTintColor(color);                
            }
            else
            {
                SetAdditionalTintColor(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, color);
                
            }
        }


        public virtual void SetResourceContainerColorFlashing(Color color, float duration)
        {
            if (RenderAsIcon != null)
            {
                SetTintForDurationOfTime(color, duration);
                SetFlashing(duration);
            }
            else
            {
                SetAdditionalTintForDurationOfTime(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, color, duration);
                SetFlashing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline, duration);
            }
        }


        #endregion

        #region Tinting

        public Color GetOverlayTintColor()
        {
            if (overlayTintEffect != null)
            {
                return overlayTintEffect.GetValue();
            }
            else return Color.White;
        }

        public Color GetTintColor()
        {
            if (tintEffect != null)
            {
                return tintEffect.GetValue();
            }
            else return Color.White;
        }

        public void SetOverlayTintColor(Color color)
        {
            if (overlayTintEffect == null)
            {
                overlayTintEffect = new Effect<Color>(this, color); 
            }
            else
            {
                overlayTintEffect.SetDefaultValue(color);
            }

            RecomputeUpdateInterval();
        }

        public void SetTintColor(Color color)
        {
            if (tintEffect == null)
            {
                tintEffect = new Effect<Color>(this, color); 
            }
            else
            {
                tintEffect.SetDefaultValue(color);
            }

            RecomputeUpdateInterval();
        }

        public void SetAdditionalTintColor(AdditionalEffect effectID, Color color)
        {
            if (additionalTintEffects == null)
            {
                additionalTintEffects = new Dictionary<AdditionalEffect, Effect<Color>>();
            }

            Effect<Color> effect;

            if (!additionalTintEffects.TryGetValue(effectID, out effect))
            {
                effect = new Effect<Color>(this, color);
                additionalTintEffects.Add(effectID, effect);
            }
            else
            {
                effect.SetDefaultValue(color);
            }
           
            RecomputeUpdateInterval();
        }

        public void SetTintForDurationOfTime(Color color, float time)
        {
            if (tintEffect == null)
            {
                tintEffect = new Effect<Color>(this, Color.White);
            }

            tintEffect.AddNewEnvelope(time, color);

            RecomputeUpdateInterval();
        }

        public void SetOverlayTintForDurationOfTime(Color color, float time)
        {
            if (overlayTintEffect == null)
            {
                overlayTintEffect = new Effect<Color>(this, Color.White);
            }

            overlayTintEffect.AddNewEnvelope(time, color);

            RecomputeUpdateInterval();
        }

        public void SetAdditionalTintForDurationOfTime(AdditionalEffect effectID, Color color, float time)
        {
            if (additionalTintEffects == null)
            {
                additionalTintEffects = new Dictionary<AdditionalEffect, Effect<Color>>();
            }

            Effect<Color> effect;

            if (!additionalTintEffects.TryGetValue(effectID, out effect))
            {
                effect = new Effect<Color>(this, Color.White);
                additionalTintEffects.Add(effectID, effect);
            }
            else
            {
                effect.SetDefaultValue(Color.White);
            }

            effect.AddNewEnvelope(time, color);

            RecomputeUpdateInterval();
        }



        #endregion

        public Vector4 GetCombinedEffects(AdditionalEffect effectID)
        {
            Vector4 pulsing = Vector4.One;
            if (AdditionalEffects != null)
            {
                pulsing = AdditionalEffects[effectID].GetValue().GetCurrentColor(null).ToVector4();
            }

            Vector4 tinting = Vector4.One;
            if (additionalTintEffects != null)
            {
                tinting = additionalTintEffects[effectID].GetValue().ToVector4();
            }

            return pulsing * tinting; // *fadingAlpha;

        }


        /// <summary>
        /// adds all current effects together and returns a color for drawing for drawing the renderable
        /// </summary>
        /// <returns></returns>
        public Vector4 GetCombinedEffects()
        {
            return GetCommonEffects(pulsingEffect, tintEffect, fadeProgress);           

        }

        public Color GetCombinedEffectsAsColor()
        {
            return new Color(GetCommonEffects(pulsingEffect, tintEffect, fadeProgress));
        }

        private static Vector4 GetCommonEffects(Effect<Animation2DPlayer> pulsingEffect, Effect<Color> tintingEffect, float fadeProgress)
        {
            Color pulsing;
            if (pulsingEffect != null)
            {
                pulsing = pulsingEffect.GetValue().GetCurrentColor(null);
            }
            else
            {
                pulsing = Color.White;
            }

            Color tinting;
            if (tintingEffect != null)
            {
                tinting = tintingEffect.GetValue();
            }
            else
            {
                tinting = Color.White;
            }

            float fadingAlpha = fadeProgress;


            return pulsing.ToVector4() * tinting.ToVector4() * fadingAlpha;

        }

        public Color GetCombinedOverlayEffectsAsColor()
        {
            Vector4 commonEffects = GetCommonEffects(overlayPulsingEffect, overlayTintEffect, fadeProgress);
            return new Color(commonEffects);

        }

        public Vector4 GetCombinedOverlayEffects()
        {
            return GetCommonEffects(overlayPulsingEffect, overlayTintEffect, fadeProgress);
            
        }

        public void GetOverlayGradientColors(out Vector4 gradient1, out Vector4 gradient2, out Vector4 gradient3)
        {
            if (renderEffect == RenderEffect.Memory)
            {
                gradient1 = memoryFactGradient1;
                gradient2 = memoryFactGradient2;
                gradient3 = memoryFactGradient3;
            }
            else //if (renderEffect == RenderEffect.Normal)
            {
                gradient1 = overlayGradient1;
                gradient2 = overlayGradient2;
                gradient3 = overlayGradient3;
            }
        }

        public void Destroy()
        {
            //clear-up resources and unregister from systems and junk

            if (ParticleEmitters != null)
            {
                for (int i = ParticleEmitters.Count - 1; i >= 0; i--)
                {
                    ParticleEmitter emitter = ParticleEmitters[i];

                    The.Client.ParticleManager.RemoveEmitter(emitter);
                }
            }

            RenderableFactory.Remove(this);

        }


        #region Sprite properties

        /// <summary>
        /// copied from Entity and MemoryFact because some renderables which do not have a parent Entity/MemoryFact need this property (like TileResources)
        /// </summary>
        private bool flipHorizontally;
        public bool FlipHorizontally
        {
            get
            {
                return flipHorizontally;
            }

            set
            {
                if (flipHorizontally != value)
                {
                    flipHorizontally = value;

                    // requires recomputed sprite coords:
                    SetSpriteQuadsDirty();
                }
            }
        }


        #endregion


        //private bool hideHandAttachments;
        public bool HideHandAttachments = false;


        private float? randomConstant;

        /// <summary>
        /// this is needed by trees to make the phase of each swaying tree different
        /// </summary>
        public float RandomConstant
        {
            get
            {
                if (!randomConstant.HasValue)
                {
                    randomConstant = (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree") * 2 - 1;
                }

                return randomConstant.Value;
            }

        }



        /// <summary>
        /// will return the position of entity or memoryfact if available - I don't want to copy the data
        /// </summary>
        public Point MapPosition
        {
            get
            {
                if (Entity != null)
                {
                    return Entity.MapPosition.Value;
                }
                else if (MemoryFact != null)
                {
                    return MemoryFact.MapPosition.Value;
                }
                else return MapManager.WorldPosToTile(Location.Value);
            }
        }

        /// <summary>
        /// will return the position of entity or memoryfact if available - I don't want to copy the data
        /// 
        /// should never be null since the renderable is only present on playsite!
        /// </summary>
        public override Vector3? Location
        {
            get
            {
                if (Entity != null)
                {
                    return Entity.Location;
                }
                else if (MemoryFact != null)
                {
                    return MemoryFact.Location;
                }
                else return base.Location;
            }
            set
            {
                base.Location = value;
            }
        }

      /*  public Vector3 PlaySiteLocation
        {
            get
            {
                return Location.Value;
            }
        }*/

        private void BindToEntityID(EntityID id) { }//TODO IMPL

        public void spawnAttachableRenderable(string asset, bool randomlyRotate, int expireTimer) { }//TODO IMPL


        [Flags]
        public enum TintStatus : uint
        {
            None = 0,///< Um, none.
            Disabled = 1 << 0,///< renderable tint color is deathly dark grey
            Experience = 1 << 1, ///< renderable tint color for level up
            Selected = 1 << 2  ///< indicates a selected entity
        };

        private TintStatus tintStatus;
        public void setTintStatus(TintStatus statusBits) { tintStatus |= statusBits; }
        public void clearTintStatus(TintStatus statusBits) { tintStatus = TintStatus.None; }
        public bool testTintStatus(TintStatus statusBits) { return (tintStatus & statusBits) != 0; }


        public TintEnvelope selectionFlashEnvelope; ///< used	for	selection flash, works WITH	m_colorTintEnvelope
        public TintEnvelope colorTintEnvelope;	  ///< panic color flashing, etc...	works WITH m_selectionFlashEnvelope

        public void setTintColor(Color tintColor, uint preColorTime, uint postColorTime, uint sustainedColorTime, float tintFrequency, float tintAmplitude)
        {
            //TODO IMPL
        }
        public TintEnvelope getColorTintEnvelope() { return colorTintEnvelope; }
        public void setColorTintEnvelope(ref TintEnvelope source) { if (colorTintEnvelope != null) colorTintEnvelope = source; }

        [Flags]
        public enum LightAttenuationStatus : uint
        {
            None = 0,	///< not active
            Normal = 1 << 0,	///< in full light
            Dark = 1 << 1,	///< in full shadow
            Darkening = 1 << 2,	///< light to shadow
            Lightening = 1 << 3	///< shadow to light
        };

        private LightAttenuationStatus lightAttenuationStatus;
        private TintEnvelope environmentLightAttenuation;

        // Light attenuation of renderable based on environment effects
        public void setLightAttenuationStatus(LightAttenuationStatus statusBits) { lightAttenuationStatus |= statusBits; }
        public void clearLightAttenuationStatus(LightAttenuationStatus statusBits) { lightAttenuationStatus = LightAttenuationStatus.None; }
        public bool testLightAttenuationStatus(LightAttenuationStatus statusBits) { return (lightAttenuationStatus & statusBits) != 0; }
        public void setEnvironmentLightAttenuation(ref TintEnvelope source) { if (environmentLightAttenuation != null) environmentLightAttenuation = source; }
        public TintEnvelope getEnvironmentLightAttenuation() { return environmentLightAttenuation; }

        //public _Image_ getPortraitImage()
        //{
        //    //TODO IMPL
        //    return null;
        //}
        //public _Image_ getButtonImage()
        //{
        //    //TODO IMPL
        //    return null;
        //}

        public class RadiusDecalType
        {
            //TODO IMPL
        }
        RadiusDecalType selectionDecalType;
        public void createSelectionDecal(uint numSelectedUnits, Color color)
        {
            //TODO IMPL
        }
        public RadiusDecalType getSelectionDecalTemplate() { return selectionDecalType; }
        public void assignSelectionDecalType(ref RadiusDecalType decalType)
        {
            //TODO IMPL
        }
        public void setSelectionDecalPosition(ref Vector3 position, ref Vector3 normal)
        {
            //TODO IMPL
        }
        public void setSelectionDecalColor(Color color)
        {
            //TODO IMPL
        }
        public void removeSelectionDecal()
        {
            //TODO IMPL
        }


        /// <summary>
        /// hide or unhide renderable when contained status changes
        /// </summary>
        public bool IsDrawn = true;

        /// <summary>
        /// draw without casting shadows, having outlines or blocking light
        /// </summary>
        public bool DrawAsNonPhysical
        {
            get
            {
               // return false;
                return renderEffect != RenderEffect.Normal;
            }
        }

        /// <summary>
        /// drawwn with the overlay shader - currently not implmented for skinned models
        /// </summary>
        public bool DrawAsOverlay
        {
            get
            {
                return renderEffect == RenderEffect.Overlay || renderEffect == RenderEffect.Memory;
            }
        }

        
        /// <summary>
        /// set the modes we need for rendering something that represents a memory fact
        /// </summary>
        public void SetMemoryRendering(bool value)
        {
            if (value == true && renderEffect == RenderEffect.Normal)// overlay trumps memory!? implement best match on bit flags?
            {
                renderEffect = RenderEffect.Memory;
            }
            else renderEffect = RenderEffect.Normal;
        }

        public void SetOverlayRendering(bool value)
        {
            if (value == true)
            {
                renderEffect = RenderEffect.Overlay;
            }
            else renderEffect = RenderEffect.Normal;
        }

    

        /// <summary>
        /// call this on Load..?
        /// </summary>
        public void UpdateIsDrawnStatus()
        {

            bool isAttached = false;
            if (RenderAsModel != null && RenderAsModel.AttachedTo != null) // .AttachorBone != null)
            {   // we draw attached entities along with the entity they are attached to.
                isAttached = true;
            }

            Container container;
            if (!Entity.GetContainedBy(out container))
            {
                IsDrawn = false;
                return;
            }


            IsDrawn = Entity.PartOf == null
                && (container == null || container.IsOpenContainer() || container.IsVisible(Entity)) && !isAttached
                && Entity.DrivingVehicle == null && Entity.PassengerInVehicle == null;
        }

        public void SetNormalRendering()
        {
            renderEffect = RenderEffect.Normal;
        }

       
        public void setSelectable(bool selectable)///< Changes the renderable's selectability	
        {
            //TODO IMPL
        }
        public bool isSelectable()
        {
            //TODO IMPL
            return true;
        }
        /// <summary>
        /// If my bound Entity's footprint or collision shapes are different, then I have to refresh lots of client side stuff
        /// </summary>
        public void reactToGeometryChange()
        {
            //TODO IMPL
        }

       
        public float getScale()
        {
            return 1f;
            //TODO IMPL
        }

        uint fogOfWarClearFrame;
        void setFogOfWarClearFrame(uint frame) { fogOfWarClearFrame = frame; }
        uint getFogOfWarClearFrame() { return fogOfWarClearFrame; }

        #region Effects
        
        // Fading
        private enum FadeStatus { None, FadeIn, FadeOut }

        FadeStatus fadeStatus = FadeStatus.None;

        /// <summary>
        /// very handy for particle emitters and renderables without an entity...
        /// </summary>
        bool destroyAfterFadeOut = false;

        /// <summary>
        /// 0 - 1
        /// 0 : invisible, 1: fully visible
        /// it is OK for this progress value to change direction part way through.
        /// </summary>
        private float fadeProgress = 1;


        // add 'flashing overlay' effect:


        // add 'flashing' effect (the renderable itself is flashing, like structures being placed):


        #endregion

        public void setFullyObscuredByShroud(bool fullyObscured)
        {
            //TODO IMPL
        }

        public void colorFlash(Color color, uint decayFrames = 16, uint attackFrames = 0, uint sustainAtPeak = 0)  ///< flash a Renderable in the color specified for a short time
        {
            //TODO IMPL
        }
        public void colorTint(Color color)	 ///< tint this renderable the color specified
        {
            //TODO IMPL
        }
        public void setTintEnvelope(Color color, float attack, float decay)	 ///< how to transition color
        {
            //TODO IMPL
        }
        public void flashAsSelected(Color? color = null) ///< renderable takes care of the details if you spec no color
        {
            //TODO IMPL
        }
        public void updateLightAttenuation()
        {
            //TODO IMPL
        }

        private bool selected;
        /// Return true if renderable has been marked as "selected"
        public bool isSelected() { return selected; }
        public void onSelected()														///< Work unrelated to selection that must happen at time of selection
        {
            //TODO IMPL
        }
        public void onUnselected()													///< Work unrelated to selection that must happen at time of unselection
        {
            //TODO IMPL
        }

        public void recalcCollisionType()
        {
            //TODO IMPL
        }

        #region Sound

        /// <summary>
        /// when the sound is playing, these properties have a value:
        /// we only allow one state sound to play at a time, and it will most often be looping.
        /// they are set from AnimCondition
        /// </summary>
        public Tuple<SoundData, SoundEffectInstance> StateSoundPlaying; // List<SoundEffectInstance> ActionSounds;

        /// <summary>
        /// these sounds are not looping and there can be multiple playing at the same time.
        /// for instance impact sounds when fighting
        /// </summary>
        public List<Tuple<SoundData, SoundEffectInstance>> ActionSoundsPlaying; 


        /// <summary>
        /// State sound holds ambient sounds.
        /// 
        /// not sure if this is the best way to make an ambient sound emitter.
        /// state sound can only be played by RenderAsModel...
        /// </summary>
       // public Tuple<SoundData, SoundEffectInstance> AmbientSoundPlaying;
       

      //  public bool Audible { get; set; }
      //  public void setInaudible() { Audible = false; }
        //bool hasSoundAmbient()   { return hasAudio(soundAmbient); }

        //SoundInfo getSoundMoveStart() 				{ return getAudio(soundMoveStart); }
        //SoundInfo getSoundMoveStartDamaged() 		{ return getAudio(soundMoveStartDamaged); }
        //SoundInfo getSoundMoveLoop() 				{ return getAudio(soundMoveLoop); }
        //SoundInfo getSoundMoveLoopDamaged() 		{ return getAudio(soundMoveLoopDamaged); }
        //SoundInfo getSoundAmbient() 				{ return getAudio(soundAmbient); }
        //SoundInfo getSoundAmbientDamaged() 			{ return getAudio(soundAmbientDamaged); }
        //SoundInfo getSoundAmbientReallyDamaged() 	{ return getAudio(soundAmbientReallyDamaged); }
        //SoundInfo getSoundAmbientRubble() 			{ return getAudio(soundAmbientRubble); }
        //SoundInfo getSoundAmbientBattle() 			{ return getAudio(soundAmbientBattle); }
        //SoundInfo getSoundCreated() 				{ return getAudio(soundCreated); }
        //SoundInfo getSoundOnDamaged() 				{ return getAudio(soundOnDamaged); }
        //SoundInfo getSoundOnReallyDamaged() 		{ return getAudio(soundOnReallyDamaged); }
        //SoundInfo getSoundEnter() 					{ return getAudio(soundEnter); }
        //SoundInfo getSoundExit() 					{ return getAudio(soundExit); }
        //SoundInfo getSoundFalling() 				{ return getAudio(soundFalling); }
        //SoundInfo getSoundImpact() 		  			{ return getAudio(soundImpact); }


        #endregion


        public void setTimeOfDay(GameTime tod)
        {
            //TODO IMPL
        }


       


        /// <summary>
        /// override this with an empty method in RenderableStub
        /// 
        /// 
        /// </summary>
        /// <param name="attachRenderableKey"></param>
        /// <param name="attachor"></param>
        /// <param name="attacheePoint"></param>
        public virtual void AttachPooledObjectIfPossible(string attachRenderableKey, AttachPoint attachor, AttacheePoint attacheePoint, bool saveToSnapshot = false)
        {
            if (RenderAsModel != null)
            {                 
                if (!RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachor.BoneName))
                {
                    if (saveToSnapshot)
                    {
                        // save this...
                        Common.AddToList(ref snapshotAttachables,
                            new Tuple<string, string, AttacheePoint>(attachRenderableKey, attachor.Tag, attacheePoint));
                    }


                    Renderable attachable = The.Client.Renderer.GetFreeAttachableRenderable(attachRenderableKey);
                    AttachPoint attachee;

                    // see if the current anim specifies a transform/attach point:
                    Vector3 translationToUse, rotationToUse;
                    RenderAsModel.GetAttachTransformations(attachable, attachor, attacheePoint, RenderAsModel.SelectedAnimInfo, out attachee, out translationToUse, out rotationToUse ); 

                    
                    if (!AttachObject(attachable.RenderAsModel, attachor, attachee, attacheePoint, translationToUse, rotationToUse, false))
                    {
                        The.Client.Renderer.RetireAttachableRenderable(attachable);
                    }
                }
            }

        }

        public virtual void RemoveAttachedMountedRenderables(string renderableKey)
        {
            if (RenderAsModel != null)
            {
                RemoveAndRetireAllAttachedModels(RenderAsModel.ModelData.RightHandAttachor, renderableKey);
                RemoveAndRetireAllAttachedModels(RenderAsModel.ModelData.LeftHandAttachor, renderableKey);
            }
        }


        public virtual void AttachPooledObjectIfPossible(string attachRenderableKey, string attachorTag, AttacheePoint attacheePoint, bool saveToSnapshot = false)
        {
            if (RenderAsModel != null)
            {
                AttachPoint attachor = RenderAsModel.ModelData.GetAttachPointFromTag(attachorTag);

                if (attachor != null)
                {
                    AttachPooledObjectIfPossible(attachRenderableKey, attachor, attacheePoint, saveToSnapshot);
                   
                }
            }

        }

        public virtual void DeattachAndRetireObject(IAttachable attachedObject, string attachorBoneName)
        {
            RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(attachedObject, attachorBoneName);

            // place it back in the pool:
            The.Client.Renderer.RetireAttachableRenderable(((RenderAsModel)attachedObject).Renderable);

            // The.Client.Renderer.RetireAttachableEntity((Entity)(((RenderAsModel)attachedObject).Parent));

        }

        public virtual void RemoveAndRetireAllAttachedModels(AttachPoint attachor, string renderableKey) // string attachorBoneName)
        {
            List<IAttachable> attachedObjectList;
            if (RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachor.BoneName, out attachedObjectList))
            {
                IAttachable attachedObject;
                
                // cannot modify list in foreach!
                for (int i = 0; i < attachedObjectList.Count; i++)
                {
                    attachedObject = attachedObjectList[i];

                    ClearSnapshotAttachables(renderableKey, /* attachedObject,*/ attachor);

                    DeattachAndRetireObject(attachedObject, attachor.BoneName);

                    i--;
                }
            }
        }

        public virtual void RemoveAttachables(string renderableKey, AttachPoint attachor)
        {
            List<IAttachable> attachedObjectList;
            if (RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachor.BoneName, out attachedObjectList))
            {
                IAttachable attachedObject;

                // not called from AgentStorage??

                for (int i = attachedObjectList.Count - 1; i >= 0; i--)
                {
                    attachedObject = attachedObjectList[i];

                    ClearSnapshotAttachables(renderableKey, /* attachedObject,*/ attachor);

                    if (((RenderAsModel)attachedObject).Renderable.RenderableType.KeyName == renderableKey)
                    {
                        DeattachAndRetireObject(attachedObject, attachor.BoneName);
                    }
                }
            }
        }

        private void ClearSnapshotAttachables(string renderableKey, /*IAttachable attachable,*/ AttachPoint attachor)
        {
            if (snapshotAttachables != null)
            {
                snapshotAttachables.RemoveAll(
                    t => (renderableKey == null || t.Item1 == renderableKey) 
                         && t.Item2 == attachor.Tag);
            }
             
        }



        public virtual void UpdateAttachBoxModelToHand(/*bool addBox, BoxHandlingWhenHauling boxHandling*/)
        {           
          //  AgentStorage.BurdenState currentBurdenState = Entity.AgentStorage.GetCurrentBurdenState();

            float burdenPercentage = Entity.AgentStorage.GetHaulingPercentageOfCapacity();
            AnimModifier? burdenStateFlag = GetBurdenAnimStateFlag(burdenPercentage); // storedPercentage);


            AttachBoxModelToHand(burdenStateFlag); //currentBurdenState);
        }

        public virtual void AttachBoxModelToHand(AnimModifier? burdenFlag) //  AgentStorage.BurdenState currentBurdenState)
        {
            BoxHandlingWhenHauling boxHandling = RenderableType.BoxHandlingWhenHauling;
            if (boxHandling != null)
            {
                if (boxHandling.ShowBoxInHand != null
                    && (burdenFlag == AnimModifier.Heavy // currentBurdenState == AgentStorage.BurdenState.HaulHeavy // addBox 
                        || boxHandling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack)) // AlwaysInHand))
                {
                    AttachPooledObjectIfPossible("box", RenderAsModel.ModelData.GetAttachPointFromTag(boxHandling.AttachorWhenBoxIsInHand), boxHandling.ShowBoxInHand.Value, true);
                }

                // mutex with heavy backpack:
                if (boxHandling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar)
                {
                    RemoveAttachedHauledObjectsFromBack("backpackHeavy");
                }
            }
        }

        public virtual void AttachModelToBack(string boxModelKey, AttacheePoint attacheePoint)
        {
            AttachPooledObjectIfPossible(boxModelKey, RenderAsModel.ModelData.BackAttachor, attacheePoint, true);

            // mutex with carry box:
            RemoveAttachedBoxModelsFromHand();
        }

        public virtual void RemoveHauledObjects()
        {
            RemoveAttachedBoxModelsFromHand();
            RemoveAttachedHauledObjectsFromBack("backpackHeavy");
            RemoveAttachedHauledObjectsFromBack("box");// robot..
            
        }

        public virtual void UpdateBurdenState(AgentStorage.BurdenState burdenState)
        {
            BoxHandlingWhenHauling boxHandling = Parent.EntityType.RenderableTypeMode.BoxHandlingWhenHauling;

            if (boxHandling != null)
            {
                if (boxHandling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack)
                {
                    if (burdenState == AgentStorage.BurdenState.None)
                    {
                        RemoveHauledObjects();
                    }
                }
                else
                {
                    if (burdenState != AgentStorage.BurdenState.HaulHeavy) // human carry
                    {
                        RemoveHauledObjects();
                    }
                }
            }
        }

        public virtual void RemoveAttachedHauledObjectsFromBack(string objectKey)
        {
            if (RenderAsModel != null)
            {
                AttachPoint attachor = RenderAsModel.ModelData.BackAttachor;

                if (attachor != null)
                {
                    RemoveAttachables(objectKey, attachor);
                }
            }
        }


        public virtual void RemoveAttachedBoxModelsFromHand()
        {
            if (RenderAsModel != null)
            {                
                AttachPoint attachor = RenderAsModel.ModelData.RightHandAttachor;
                if (attachor != null)
                {
                    RemoveAttachables("box", attachor);
                }

                attachor = RenderAsModel.ModelData.LeftHandAttachor;
                if (attachor != null)
                {
                    RemoveAttachables("box", attachor);
                }
            }
        }

        


       

        public virtual void RemoveAttachables(string renderableKey, string attachorTag)
        {
            if (RenderAsModel != null)
            {
                AttachPoint attachor = RenderAsModel.ModelData.GetAttachPointFromTag(attachorTag); //  .RightHandAttachor;

                if (attachor != null)
                {
                    RemoveAttachables(renderableKey, attachor);
                }
            }
        }

        

    /*    public virtual void AttachPooledObjectIfPossible(string attachableKey, AttachPoint attachor, AttacheePoint attacheePoint)
        {
            if (RenderAsModel != null)
            {
                if (!RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachor.BoneName))
                {
                    Entity attachable = The.Client.Renderer.GetFreeAttachableEntity(attachableKey);
                    AttachPoint attachee;

                    // see if the current anim specifies a transfom/attach point:
                    attachee = RenderAsModel.GetCurrentAttachPointData(attachable, attacheePoint); //, attachableKey);



                    if (!AttachObject(attachable.Renderable.RenderAsModel, attachor, attachee, attacheePoint, false))
                    {
                        The.Client.Renderer.RetireAttachableEntity(attachable);
                    }
                }
            }

        }*/

        /// <summary>
        /// attachee data overrides attachor data
        /// </summary>
        public virtual bool AttachObject(IAttachable objectToAttach, AttachPoint attachor, AttachPoint attachee, AttacheePoint? attacheePoint, Vector3 translation, Vector3 rotation, bool testIfAlreadyAttached = true)
        {
            if (RenderAsModel == null)
            {
                return false;
            }

            if (testIfAlreadyAttached)
            {
                if (RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachor.BoneName))
                {
                    return false;
                }
            }


           // Entity entityToAttach = ((RenderAsModel)objectToAttach).Parent as Entity;

            Renderable renderable = ((RenderAsModel)objectToAttach).Renderable;

            AttachEntityAndCreateLocalTransform(
                renderable, //entityToAttach,
                // entityToAttach.EntityType.Renderable.RenderAsModelType.ModelScale,
                renderable.RenderAsModel.FinalModelScale,
                attachor,
                attachee,
                attacheePoint,
                translation,
                rotation);

            return true;

        }


        /// <summary>
        /// attachee data overrides attachor data
        /// </summary>
        /// <param name="renderableToAttach"></param>
        /// <param name="scaling"></param>
        /// <param name="attachorPoint"></param>
        /// <param name="attachee"></param>
        /// <param name="attacheeBoneName"></param>
        public virtual void AttachEntityAndCreateLocalTransform(Renderable renderableToAttach, float scaling, AttachPoint attachorPoint, AttachPoint attachee, AttacheePoint? attacheePoint, Vector3 translation, Vector3 rotation) //, string attacheeBoneName = null)
        {
            Matrix localTransform = CreateTransformForAttachedEntity(scaling, attachorPoint, attachee, translation, rotation);

            renderableToAttach.RenderAsModel.LocalTransform = localTransform;

            if (!string.IsNullOrEmpty(attachorPoint.BoneName))
            {
                BonePose attacheeBone = renderableToAttach.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[attachee.BoneName];

                RenderAsModel.AnimatedModel.ModelAnimator.AttachObject(renderableToAttach.RenderAsModel, attachorPoint, attachee, attacheePoint, attacheeBone, Parent); //attachorPoint.AttachBoneName, attacheeBone, attacheePoint);
            }

        }

        public static Matrix CreateTransformForAttachedEntity(float scaling, AttachPoint attachorPoint, AttachPoint attachee, Vector3 translation, Vector3 rotation)
        {
            Matrix scale;
            scale = Matrix.CreateScale(scaling);

            Matrix rotateMatrix = Matrix.Identity;

            Matrix translate = Matrix.Identity;

            translate = Matrix.CreateTranslation(translation);

            if (rotation != Vector3.Zero)
            {
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(rotation.X));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(rotation.Y));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(rotation.Z));
            }

            // attachee transformations should override attachor!
          /*  if (attachee != null && attachee.Translation.HasValue)
            {
                translate = Matrix.CreateTranslation(attachee.Translation.Value);
            }
            else if (attachorPoint.Translation.HasValue)
            {
                translate = Matrix.CreateTranslation(attachorPoint.Translation.Value);
            }
            
            if (attachee != null && attachee.Rotation != new Vector3(0f, 0f, 0f))
            {
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(attachee.Rotation.X));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(attachee.Rotation.Y));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(attachee.Rotation.Z));
            }
            else if (attachorPoint.Rotation != new Vector3(0f, 0f, 0f))
            {
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(attachorPoint.Rotation.X));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(attachorPoint.Rotation.Y));
                rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(attachorPoint.Rotation.Z));
            }*/

            Matrix localTransform = scale * rotateMatrix * translate;
            return localTransform;
        }

        #region Animation

       


        /// <summary>
        /// because of the lagging of the renderable behind the sim entity, 
        /// we may be running other anims at the moment than we expect, such as a gait anim. In that case, the animation is not progressed.
        ///       
        /// NEW: added code to ensure we only update the 'latest' anim. We don't want to restart the previous anim with a scalar factor 0f 0.01...
        /// </summary>
        /// <param name="scalar"></param>
        public virtual void UpdateMainAnimationTimeScalar(double scalar)
        {
            if (RenderAsModel != null)
                RenderAsModel.UpdateAnimationManuallyByTimeScalar(scalar);
        }
       

     /*   public virtual void ClearAnimationStateFlags(AnimModifier[] states)
        {
            if (states != null)
            {
                foreach (var item in states)
                {
                    ClearAnimationStateFlag(item);
                }
            }
        }*/

        public virtual void SetAnimFlagsDirty()
        {
            if (Entity != null && Entity.Name != null && Entity.Name.Contains("Pezal"))
            {

            }

            if (RenderAsModel != null)
                RenderAsModel.SetAnimFlagsDirty();
        }

        public virtual void SetStaticFlagsDirty()
        {


            /*if (RenderAsModel != null)
                RenderAsModel.SetAnimFlagsDirty();*/
        }

       /* public virtual void ClearAnimationStateFlag(AnimModifier state)
        {
            if (RenderAsModel != null)
                RenderAsModel.ClearAnimationStateFlag(state);
        }

        public virtual void ClearAnimationActionStateFlag(AnimAction state)
        {
            if (RenderAsModel != null)
                RenderAsModel.ClearAnimationActionStateFlag(state);
        }

        public virtual void SetAnimationActionStateFlag(AnimAction state)
        {
            if (RenderAsModel != null)
                RenderAsModel.SetAnimationActionStateFlag(state);
        }

        public virtual void SetAnimationStateFlag(AnimModifier state)
        {
            if (RenderAsModel != null)
                RenderAsModel.SetAnimationStateFlag(state);
        }*/

       /* public virtual void SetAnimationStateFlags(AnimModifier[] states)
        {
            if (states != null)
            {
                foreach (var item in states)
                {
                    SetAnimationStateFlag(item);
                }
            }
        }*/

      

        #endregion




        public void UpdateAnimationConditionState()
        {
            if (RenderAsModel != null)
                RenderAsModel.UpdateAnimationConditionState();
        }

       

    /*    private bool renderOverlay = false;

        /// <summary>
        /// TODO: delete this esoteric property, replace with seeing if the sprite is not currently rendered, its SourceRectangle will be null
        /// </summary>
        public bool IsAllowedToRenderOverlay
        {                        
            set
            {
                renderOverlay = value;   
            }
            get
            {
                return renderOverlay;
            }
        }*/



       


        private void RecomputeUpdateInterval()
        {
            bool intervalChanged;
            RecomputeUpdateInterval(out intervalChanged);
        }

        /// <summary>
        /// Remember to keep this up-to-date when more functionality is added to the Update method!
        /// Otherwise performance will suffer because of unnecessary updates, or the object may not receive any Update calls when it needs it.
        /// 
        /// Call RecomputeUpdateInterval explicitly!
        /// </summary>
        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
            intervalChanged = false;

            double? tempInterval = null, currentInterval = null;

            if (Location == null && Parent != null && Parent is Entity)// NEW - #RenderableCrash
            {
                tempInterval = null; // off-site entity, go to sleep
            }
            else
            {
                tempInterval = GetOverlayEnvelopeEffectsUpdateInterval();
                currentInterval = tempInterval;


                tempInterval = GetAdditionalEffectsUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = GetFadingUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = GetIntervalForExpiry();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = GetRenderAsModelInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = GetRenderAsBillboardInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                tempInterval = GetNextSoundInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }

            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }
        }
        
        

      /*  private void PlayAmbientSound()
        {
            if (RenderableType.AmbientSound != null)
            {
                if (AmbientSoundPlaying == null)
                {
                    SoundEffectInstance instance = The.Client.AudioManager.PlayWorldSound(RenderableType.AmbientSound, new WorldLocation(Location), fadeProgress, true); //  RenderableType.AmbientSound.SoundEffect.CreateInstance();
                    if (instance != null)
                    {
                        AmbientSoundPlaying = new Tuple<SoundData, SoundEffectInstance>(RenderableType.AmbientSound, instance);
                    }
                  
                }
                else
                {
                    if (AmbientSoundPlaying.Item2.State != SoundState.Playing)
                    {
                        AmbientSoundPlaying.Item2.Play();
                    }
                }
            }
        }*/

        /// <summary>
        /// snaps the renderable to the parent's location and direction
        /// </summary>
        public virtual void SetToParentLocation()
        {
            if (RenderAsModel != null)
                RenderAsModel.SetToParentLocation();
        }

        public virtual void UpdateLerpingBackSpine(GameTime gameTime)
        {
            if (AnimatedHead != null)
            {
                AnimatedHead.UpdateLerpingBackSpine(gameTime);
            }
        }

        public virtual void StartLerpingBackSpine()
        {
            if (AnimatedHead != null)
            {
                AnimatedHead.StartLerpingBackSpine();
            }
        }

        public virtual void TurnToLook(Vector2 lookTarget, float lookAngle, float lookTargetAngle) //, out bool isLookingAtTarget, out bool isOutOfView)
        {
            if (AnimatedHead != null)
            {
                AnimatedHead.TurnToLook(lookTarget, lookAngle, lookTargetAngle);
            }

        }

    

        private enum RenderEffect { Normal, Memory, Overlay, Stealth }

        static readonly Vector4 memoryFactGradient1 = Color.Cyan.ToVector4();
        static readonly Vector4 memoryFactGradient2 = Color.LightSeaGreen.ToVector4();
        static readonly Vector4 memoryFactGradient3 = Color.White.ToVector4();


        private Vector4 overlayGradient1 = Color.DarkGray.ToVector4();
        private Vector4 overlayGradient2 = Color.LightGray.ToVector4();
        private Vector4 overlayGradient3 = Color.White.ToVector4();


        /// <summary>
        /// the effect that applies to standard rendering
        /// </summary>
        private RenderEffect renderEffect = RenderEffect.Normal;

     

        private float strobe = 0;

     

        /// <summary>
        /// called from Client!
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime, /*out double? timeBeforeNextUpdate,*/ out bool wasDestroyed)
        {
            // ask each component when it is time to update again. 

            // also recompute the time if the effects/animation settings are changed...

            // this will trigger a resorting of the list of renderables just before the next Update().

            // null means Sleep/no update needed. 0 means next frame

         //   timeBeforeNextUpdate = null;
          //  double? tempTimeBeforeNextUpdate = null;

            wasDestroyed = false;

            if (RenderAsModel != null)
            {
                RenderAsModel.Update(gameTime);
             //   GetNextInterval(tempTimeBeforeNextUpdate, ref timeBeforeNextUpdate);
            }


            if (RenderAsBillboard != null)
            {
                foreach (RenderAsBillboard billboard in RenderAsBillboard)
                {
                    billboard.Update(gameTime);
                  //  GetNextInterval(tempTimeBeforeNextUpdate, ref timeBeforeNextUpdate);
                }
            }


            bool effectsWereUpdated = false;
            UpdateFading(gameTime, ref effectsWereUpdated); // also fade sounds that are not visible!
            UpdateEnvelopeEffects(gameTime, ref effectsWereUpdated); 
           
            if (effectsWereUpdated)
            {
                SetSpritePropertiesDirty();
            }

            bool overlayEffectsWereUpdated = false;
            UpdateOverlayEnvelopeEffects(gameTime, ref overlayEffectsWereUpdated);


            if (overlayEffectsWereUpdated)
            {
                SetOverlaySpritePropertiesDirty();
            }

            UpdateAdditionalEffects(gameTime);

            UpdateSoundDelay();

            UpdateExpiry(out wasDestroyed);

            

         /*   if (!wasDestroyed)
            {
               
                RecomputeUpdateInterval();
            }*/

        }

        private void SetSpritePropertiesDirty()
        {
            // notify sprites of tint changes:
            if (RenderAsGroundSprite != null)
            {
                RenderAsGroundSprite.SetPropertiesAreDirty();
            }

            if (RenderAsBillboard != null)
            {
                foreach (var item in RenderAsBillboard)
                {
                    item.SetPropertiesAreDirty();
                }
            }
        }


        private void SetOverlaySpritePropertiesDirty()
        {
            if (RenderAsBillboard != null)
            {
                foreach (var item in RenderAsBillboard)
                {
                    item.SetOverlayPropertiesAreDirty();
                }
            }

            if (RenderAsGroundSprite != null)
            {
                RenderAsGroundSprite.SetOverlayPropertiesAreDirty(); 
            }
        }

        private void SetSpriteQuadsDirty()
        {
            // notify sprites of coordinate changes:
            if (RenderAsGroundSprite != null)
            {
                RenderAsGroundSprite.SetIsDirty();
            }

            if (RenderAsBillboard != null)
            {
                foreach (var item in RenderAsBillboard)
                {
                    item.SetIsDirty();
                }
            }

            if (LightSources != null)
            {
                LightSources.ForEach(l => l.SetIsDirty());
            }
        }


        private double? GetRenderAsModelInterval()
        {
            if (RenderAsModel != null)
            {
                return RenderAsModel.GetUpdateInterval();
            }
            else return null;
        }

        private double? GetRenderAsBillboardInterval()
        {
            if (RenderAsBillboard != null)
            {
                double? tempInterval = null, currentInterval = null;
                foreach (var item in RenderAsBillboard)
                {
                    tempInterval = item.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

                }

                return currentInterval;

            }
            else return null;
        }

        /*private double? GetRenderAsModelInterval()
        {
            if (RenderAsModel != null)
            {
                return RenderAsModel.GetTimeBeforeNextUpdate();
            }
            else return null;
        }*/


        private void UpdateSoundDelay()
        {
           
            if (this.nextSoundTimepoint.HasValue)
            {
                if (The.Sim.TimepointReached(nextSoundTimepoint.Value))
                {
                    // the delay has passed, start a new sound (here we should check if we are still in the same state..?):
                    AdoptSound(selectedSpriteInfo);

                    nextSoundTimepoint = null; 
                }
            }
        }
       

        private void UpdateExpiry(/*out double? timeBeforeNextUpdate,*/ out bool wasDestroyed)
        {
           // timeBeforeNextUpdate = null;
            wasDestroyed = false;

            if (this.ExpiryTimePointInSeconds.HasValue)
            {
                if (The.Sim.TimepointReached(ExpiryTimePointInSeconds.Value))
                {
                    if (RenderableType.FadeOutWhenDestroyed == true)
                    {
                        FadeOut(true); // destroy after fading
                    }
                    else
                    {
                        Destroy(); // destroy directly
                        wasDestroyed = true;
                    }

                    ExpiryTimePointInSeconds = null; // don't expire again!
                }              
            }
        }

        private double? GetIntervalForExpiry()
        {
            return UpdateTimePoints.ComputeIntervalFromTimepoint(this.ExpiryTimePointInSeconds);

        }


        private double? GetOverlayEnvelopeEffectsUpdateInterval()
        {
            double? tempInterval = null, currentInterval = null;

            if (overlayTintEffect != null)
            {
                tempInterval = overlayTintEffect.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }


            if (overlayPulsingEffect != null)
            {
                tempInterval = overlayPulsingEffect.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }


            return currentInterval;
        }

        private double? GetEnvelopeEffectsUpdateInterval()
        {
            double? tempInterval = null, currentInterval = null;

            if (tintEffect != null)
            {
                tempInterval = tintEffect.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }
            
            
            if (pulsingEffect != null)
            {
                tempInterval = pulsingEffect.GetUpdateInterval();
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            }

        
            return currentInterval;
        }

        private double? GetAdditionalEffectsUpdateInterval()
        {
            double? tempInterval = null, currentInterval = null;

            if (AdditionalEffects != null)
            {
                foreach (var effect in AdditionalEffects)
                {
                    tempInterval = effect.Value.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                }                
            }

            if (additionalTintEffects != null)
            {
                foreach (var effect in additionalTintEffects)
                {
                    tempInterval = effect.Value.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                }
            }
            
            return currentInterval;
        }

        private void UpdateOverlayEnvelopeEffects(GameTime gameTime, ref bool effectsWereUpdated)
        {

            if (overlayTintEffect != null)
            {
                overlayTintEffect.Update(gameTime);

                effectsWereUpdated = true;
            }

            if (overlayPulsingEffect != null)
            {
                overlayPulsingEffect.Update(gameTime);

                effectsWereUpdated = true;
            }

        }


        private void UpdateAdditionalEffects(GameTime gameTime)
        {
            if (AdditionalEffects != null)
            {
                foreach (var item in AdditionalEffects)
                {
                    item.Value.Update(gameTime);
                }
            }

            if (additionalTintEffects != null)
            {
                foreach (var item in additionalTintEffects)
                {
                    item.Value.Update(gameTime);
                }
            }
        }

        private void UpdateEnvelopeEffects(GameTime gameTime, ref bool effectsWereUpdated) 
        {
         
            if (tintEffect != null)
            {
                tintEffect.Update(gameTime);

                effectsWereUpdated = true;
            }            
      
            if (pulsingEffect != null)
            {
                pulsingEffect.Update(gameTime);

                effectsWereUpdated = true;
            }
     
        }

        private void UpdateFading(GameTime gameTime, ref bool effectsWereUpdated) 
        {
          
            if (fadeStatus != FadeStatus.None)
            {
              //  updateIsRequired = true;
                effectsWereUpdated = true;

                float fadePeriod = 2f;
               
                float fadeAmount = (float)(/*The.Sim.GameTime. previously Sim time was used??*/ gameTime.ElapsedGameTime.TotalSeconds / fadePeriod);

                if (fadeStatus == FadeStatus.FadeIn)
                {
                    fadeProgress += fadeAmount;
                }
                else
                {
                    fadeProgress -= fadeAmount;
                }

                fadeProgress = Common.Clamp(fadeProgress, 0f, 1f);

                // fade sounds (set their volume) (this may get called twice while fading, if the fading happens while "visible"):
                UpdateSoundDirectionAndDistance();
                //FadeSounds();

                if (fadeProgress >= 1d)
                {
                    fadeStatus = FadeStatus.None;

                    fadeProgress = 1f;
                }
                else if (fadeProgress <= 0f)
                {
                    The.Client.Renderer.renderablesFadingOut.Remove(this);

                    fadeStatus = FadeStatus.None;

                    fadeProgress = 0f;

                    // stop any sound playing:
                   // StopSounds();


                    if (destroyAfterFadeOut)
                    {
                        Destroy();
                    }
                }                
            }

         /*   if (updateIsRequired)
            {
                timeBeforeNextUpdate = 0;
            }
            else
            {
                timeBeforeNextUpdate = null;
            }*/
        }

        private double? GetNextSoundInterval()
        {
            if (nextSoundTimepoint.HasValue)
            {
                return UpdateTimePoints.ComputeIntervalFromTimepoint(nextSoundTimepoint.Value); // this.ExpiryTimePointInSeconds);
            }
            else return null;
        }

        private double? GetFadingUpdateInterval()
        {
            if (fadeStatus != FadeStatus.None)
            {
                return 0;
            }
            else
            {
                return null;
            }
        }

        
        private void UpdateSoundDirectionAndDistance()
        {
           // UpdateSoundDirectionAndDistance(AmbientSoundPlaying);
            UpdateSoundDirectionAndDistance(StateSoundPlaying);

            if (ActionSoundsPlaying != null)
            {
                foreach (var item in ActionSoundsPlaying)
                {
                    UpdateSoundDirectionAndDistance(item);
                }
            }
        }



        private void UpdateSoundDirectionAndDistance(Tuple<SoundData, SoundEffectInstance> sound)
        {
            if (sound != null && sound.Item2.State == SoundState.Playing)
            {
                /*
                float pan, distanceFactor;
                AudioManager.GetSoundPanAndDistanceFactor(Location, out pan, out distanceFactor);
                */

                The.Client.AudioManager.SetSoundLocation(sound.Item2, sound.Item1, fadeProgress, Location.Value);
              
               // AudioManager.SetSoundVolumeAndPan(sound.Item2, sound.Item1, fadeProgress, distanceFactor, pan);               

            }
        }

        private void StopSounds()
        {
            StopSound(StateSoundPlaying);

            if (ActionSoundsPlaying != null)
            {
                foreach (var item in ActionSoundsPlaying)
                {
                    StopSound(item);
                }
            }
        }


        private void StopSound(Tuple<SoundData, SoundEffectInstance> sound)
        {
            if (sound != null && sound.Item2.State == SoundState.Playing)
            {
                The.Client.AudioManager.StopSound(sound.Item1, sound.Item2);
            }

        }

        /// <summary>
        /// this draw call doesn't draw billboards - they require sorting and are drawn elsewhere
        /// 
        /// also draws any particle emitters - the only way that particles are drawn
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="drawWithAlpha"></param>
        /// <param name="lightIntensity"></param>
        /// <param name="tintColor"></param>
        public void Draw(GameWorldRenderer.RenderTechnique technique, ref Matrix view, ref Matrix projection, float drawWithAlpha = 1f, float lightIntensity = 1f, Color? tintColor = null)
        {
            if (IsDrawn) 
            {
                Color combinedEffects = GetCombinedEffectsAsColor();

#if DEBUG || PROFILE
                if (technique == GameWorldRenderer.RenderTechnique.Standard && Kensei.Dev.Options.GetOption("Overlays.Show entity waypoints"))
                {
                    DrawEntityWaypoints(technique);
                }
#endif

                //PlayAmbientSound();

               

                DrawModel(technique, ref view, ref projection, drawWithAlpha, lightIntensity, ref tintColor);

                if (technique == GameWorldRenderer.RenderTechnique.Standard) // particles should not cast shadows, have outlines etc.
                {
                    DrawParticleEmitters(combinedEffects);

                    UpdateSoundDirectionAndDistance();
                }
               
            }
        }

        private void DrawModel(GameWorldRenderer.RenderTechnique technique, ref Matrix view, ref Matrix projection, float drawWithAlpha, float lightIntensity, ref Color? tintColor)
        {
           
            if (RenderAsModel != null)
            {
                RenderAsModel.UpdateAnimationConditionState();
                

                float dirtLevel = 0f;
                // MachineEntity machineEntity;
                if (Parent != null && Parent.Condition.HasValue) // Find(out machineEntity))
                {
                    dirtLevel = 1f - (float)Parent.Condition.Value; // machineEntity.Condition;
                }

                RenderAsModel ram = RenderAsModel;


                if (renderEffect == RenderEffect.Memory && technique == GameWorldRenderer.RenderTechnique.StandardOverlay) //renderEffect != RenderEffect.Normal)
                {

                    /*  BlendState restore = new BlendState();
                      restore = device.BlendState;
                      device.BlendState = BlendState.Additive;//     
                     * */
                    // float stealthOpacity = Math.Abs((float)Math.Sin((float)ID + (strobe += 0.01f))) * 0.5f +.2f;
                    //  float cyclingOpacity = Math.Abs((float)Math.Sin((float)ID + (strobe += 0.01f))) * 0.5f;

                    //  Draw(technique, ref view, ref projection, cyclingOpacity, 1f, Color.Cyan);
                    // Draw(GameWorldRenderer.RenderTechnique.StandardOverlay, ref view, ref projection, cyclingOpacity, 1f, Color.Cyan);

                    //   device.BlendState = restore;//           

                    tintColor = Color.Cyan;
                }


                ram.AnimatedModel.DrawStandard(technique, view, projection,
                    Color.Cyan.ToVector3(),// ram.CustomColor0, 
                    Color.Magenta.ToVector3(),// ram.CustomColor1, 
                    Color.Yellow.ToVector3(),// ram.CustomColor2, 
                    Color.Red.ToVector3(),// ram.CustomColor3, 
                    drawWithAlpha, lightIntensity, dirtLevel, ram.FinalModelBasicTexture, tintColor);


                //foreach (IAttachable attached in RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects)
                //{
                foreach (KeyValuePair<string, List<IAttachable>> kvp in RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects)
                {
                    foreach (IAttachable attachedObject in kvp.Value)
                    {
                        if (HideHandAttachments && attachedObject.AttachorPoint.IsHand)
                            continue;


                        RenderAsModel attachedEntity = attachedObject as RenderAsModel;


                        attachedEntity.AnimatedModel.StandardDrawingWorldTransformation = attachedObject.CombinedTransform;
                        attachedEntity.AnimatedModel.CreateBoneTransformMatrixArray();

                        dirtLevel = 0f;
                        if (attachedEntity.Parent != null && attachedEntity.Parent.Condition.HasValue) // .Find(out machineEntity))
                        {
                            dirtLevel = 1f - (float)attachedEntity.Parent.Condition.Value;
                        }

                        attachedEntity.AnimatedModel.DrawStandard(technique, view, projection,
                            attachedEntity.CustomColor0, attachedEntity.CustomColor1, attachedEntity.CustomColor2, attachedEntity.CustomColor3,
                            drawWithAlpha, lightIntensity, dirtLevel, attachedEntity.FinalModelBasicTexture, tintColor);
                    }

                }

            }
        }

        private void DrawParticleEmitters(Color combinedEffects)
        {
            if (ParticleEmitters != null)
            {

                foreach (var emitter in ParticleEmitters)
                {
                    emitter.Draw(combinedEffects);
                }

                The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
        }

        /// <summary>
        /// this changes the rainbow of colors that is used in the overlay effect. Yellow/red for ordered structures, blue for memoryfacts. These colors should be more permanent... 
        /// 
        /// For fading, tinting, flashing, pulsing, start an effect instead.
        /// </summary>
        /// <param name="gradient1"></param>
        /// <param name="gradient2"></param>
        /// <param name="gradient3"></param>
        public void SetOverlayGradientColors(Color gradient1, Color gradient2, Color gradient3)
        {
            overlayGradient1 = gradient1.ToVector4();
            overlayGradient2 = gradient2.ToVector4();
            overlayGradient3 = gradient3.ToVector4();
            
            SetOverlaySpritePropertiesDirty();

           /* if (RenderAsBillboard != null)
            {
                foreach (RenderAsBillboard billboard in RenderAsBillboard)
                {
                    billboard.SetOverlayGradientColors(gradient1, gradient2, gradient3);

                }
            }*/

        }

        /// <summary>
        /// Delete this??
        /// start a (2d?) anim that fades/flashes a newly detected entity/resource
        /// </summary>
        public virtual void FlashAsDetected()
        {

        }

        /// <summary>
        /// used for repairing after load
        /// </summary>
        public virtual void ComputeMatricesForDrawing()
        {
            if (RenderAsModel != null)
            {
                RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, RenderAsModel.FinalModelScale);

                //RenderAsModel.Update();

                // NEW: some values need to be computed/copied before the model will render... Normally this is done in Renderable.Update(). 
                // only needed because we are paused when loading...      
                RenderAsModel.CopyAbsoluteTransforms();
                 
            }
        }

        public static AnimModifier? GetBurdenAnimStateFlag(float storedPercentage)
        {
            AnimModifier? burdenStateFlag;

            if (storedPercentage > GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
            {               
                burdenStateFlag = AnimModifier.Heavy;               
            }
            else
            {
                // normal... 
                burdenStateFlag = null;
            }
         
         /*   if (storedPercentage < GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
            {
                // normal...
                burdenStateFlag = null;                 
            }
            else
            {
                burdenStateFlag = AnimModifier.Heavy;
            }*/

            return burdenStateFlag;
        }


        public virtual void StartAdditionalAnimation(string animKey, Playback playback, StartingPoint startingPoint,
                                    BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
        {
            RenderAsModel.StartAdditionalAnimation(animKey, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd);

        }

    /*    public virtual void StartMainAnimation(string animKey, Playback playback, StartingPoint startingPoint,
                                    BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
        {
            RenderAsModel.StartMainAnimation(animKey, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd);
        }*/

        public virtual void SetModelBoneRotation(string boneName, Matrix rotation) //, bool keepCurrentTranslation)
        {            

            BonePose bone = RenderAsModel.ModelAnimator.BonePoses[boneName];

            bone.DefaultTransform = rotation * Matrix.CreateTranslation(bone.DefaultTransform.Translation);
            
            // this makes us override animation transforms from animations running at the same time:
            bone.UseSpecialTransform = true; 

        }

        public virtual void StopOverridingAnimTransforms(string boneName)
        {
            BonePose bone = RenderAsModel.ModelAnimator.BonePoses[boneName];

            bone.UseSpecialTransform = false; 
        }

    /*    public virtual Matrix GetModelBoneDefaultTransformation(string boneName)
        {
            return RenderAsModel.ModelAnimator.BonePoses[boneName].DefaultTransform;

        }
        */
        private void DrawEntityWaypoints(GameWorldRenderer.RenderTechnique technique)
        {
            Color color = Color.Pink;

            // draw debugging info:
            if (technique == GameWorldRenderer.RenderTechnique.Standard)
            {
                Intelligence intelligenceComponent;
                if (Entity != null && Entity.Find(out intelligenceComponent))
                {
               
                    // draw self:
                    The.Client.DrawPoint(The.MapUI.WorldPosToScreen(Location.Value), Color.White);

                    if (intelligenceComponent.GroupMoveAssignedWaypoint != null)
                    {
                        The.Client.DrawPoint(The.MapUI.WorldPosToScreen(intelligenceComponent.GroupMoveAssignedWaypoint.Location), color);
                    }


                    //draw waypoints as debugging help:
                    List<Waypoint> waypoints = intelligenceComponent.Brain.GetWaypointPath();
                    if (waypoints != null)
                    {
                        Vector2? last = The.MapUI.WorldPosToScreen(Location.Value);

                        foreach (Waypoint waypoint in waypoints)
                        {
                            if (last.HasValue)
                            {
                                Kensei.Dev.Shape.Line(last.Value, The.MapUI.WorldPosToScreen(waypoint.Location), color, color);
                            }

                            last = The.MapUI.WorldPosToScreen(waypoint.Location);
                            Kensei.Dev.Shape.Box(new Vector2(last.Value.X - 2, last.Value.Y - 2), new Vector2(last.Value.X + 2, last.Value.Y + 2), Color.Black, true);
                         
                            //The.Client.DrawPoint(last.Value, Color.Black); // DebugColor);
                        }
                    }

                    // draw current move target:
                    Locomotor locomotor;
                    if (Entity.Find(out locomotor))
                    {
                        The.Client.DrawPoint(The.MapUI.WorldPosToScreen(locomotor.CurrentMoveTarget), Color.Red);
                    }
                }
            }
        }

        /// <summary>
        /// returns the flags that are mutex'ed by this flag - where null means Standing!
        /// </summary>
        /// <param name="stanceFlag"></param>
        /// <param name="excludedFlagsResult"></param>
        public static void GetExcludedStanceFlags(AnimModifier? stanceFlag, List<AnimModifier> excludedFlagsResult)
        {
            if (stanceFlag.HasValue)
            {
                switch (stanceFlag.Value)
                {
                    case AnimModifier.Lying:
                        excludedFlagsResult.Add(AnimModifier.Kneeling);
                        excludedFlagsResult.Add(AnimModifier.Sitting);
                        break;
                    case AnimModifier.Sitting:
                        excludedFlagsResult.Add(AnimModifier.Kneeling);
                        excludedFlagsResult.Add(AnimModifier.Lying);
                        break;
                    case AnimModifier.Kneeling:
                        excludedFlagsResult.Add(AnimModifier.Lying);
                        excludedFlagsResult.Add(AnimModifier.Sitting);
                        break;

                }
            }
            else
            {
                excludedFlagsResult.Add(AnimModifier.Lying);
                excludedFlagsResult.Add(AnimModifier.Sitting);
                excludedFlagsResult.Add(AnimModifier.Kneeling);
            }
        }


        /// <summary>
        /// list of renderable key, attachor point tag, attachee point that describe an attached renderable.
        /// </summary>
        private List<Tuple<string, string, AttacheePoint>> snapshotAttachables;
       
        /*private string snapshotAttachRenderableKey;
        private string snapshotAttachorTag; 
        private AttacheePoint? snapshotAttacheePoint;*/


        /// <summary>
        /// Here, we will add and encapsulate fields from the Renderable that we want to Snapshot. Should be few... and may NOT be read by Sim!
        /// </summary>
        public class SnapshotRenderable : ISnapshot
        {
            private AnimConditions animConditions;
            private BitMask64 spriteConditions;
            private bool flipHorizontally;
            private RenderEffect renderEffect;

            /// <summary>
            /// renderable key, attachor point tag, attachee point.
            /// </summary>
            private List<Tuple<string, string, AttacheePoint>> attachables;
       /*
            private string attachRenderableKey;
            private string attachorTag; // AttachPoint attachor;
            private AttacheePoint? attacheePoint;
            */

            /// <summary>
            /// RenderAsModel fields, needed to recreate MemoryFact Renderables since the original Entity is gone           
            /// </summary>
            SnapshotRenderasModel SnapshotRenderAsModel;
           

            public SnapshotRenderable()
            {
                System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

            }

            public SnapshotRenderable(Renderable renderable)
            {
                this.spriteConditions = renderable.spriteConditions;
                this.animConditions = renderable.AnimConditions;
                this.flipHorizontally = renderable.flipHorizontally;
                this.renderEffect = renderable.renderEffect;
                this.attachables = renderable.snapshotAttachables;

                if (/*renderable.Parent is MemoryFact // MemoryFacts only???
                     &&*/ renderable.RenderAsModel != null)
                {
                    this.SnapshotRenderAsModel = new SnapshotRenderasModel(renderable.RenderAsModel);                    

                }
            }


            /// <summary>
            /// Transfers snapshotted data to the new Renderable instance.
            /// because this class is nested, it is able to set the private fields of the outer class (Renderable). This ensures encapsulation.
            /// </summary>
            /// <param name="renderable"></param>
             public void LoadRenderableWithSnapshotData(Renderable renderable)
             {
                 renderable.spriteConditions = spriteConditions;
                 renderable.AnimConditions = animConditions;
                 renderable.flipHorizontally = flipHorizontally;
                 renderable.renderEffect = renderEffect;
                 renderable.snapshotAttachables = attachables;
                 /*
                 renderable.snapshotAttacheePoint = attacheePoint;
                 renderable.snapshotAttachorTag = attachorTag;
                 renderable.snapshotAttachRenderableKey = attachRenderableKey;*/

                 if (SnapshotRenderAsModel != null)
                 {
                     // TODO: this could be made cleaner. Separate RenderAsModel creation and property assignment so the code paths are similar in the different cases

                     if (renderable.Parent is MemoryFact) // MemoryFacts only???
                     {

                         bool setAnimationFlagsDirty;
                         /* if (entityData is MemoryFact)
                          {
                              setAnimationFlagsDirty = false; // we can display the frozen pose of the original
                          }
                          else
                          {*/
                         setAnimationFlagsDirty = true; // for carcass items, run the animation once to show the dead pose -also for loaded MemoryFacts??
                         // }

                         renderable.RenderAsModel = new RenderAsModel(
                             SnapshotRenderAsModel.CustomColor0, SnapshotRenderAsModel.CustomColor1, SnapshotRenderAsModel.CustomColor2, SnapshotRenderAsModel.CustomColor3,
                             SnapshotRenderAsModel.LocalTransform,
                             SnapshotRenderAsModel.Location,
                             SnapshotRenderAsModel.FacingNormal,
                             SnapshotRenderAsModel.FinalModelName,
                             SnapshotRenderAsModel.FinalModelBasicTextureName,
                             SnapshotRenderAsModel.FinalModelScale,
                             null, // create an AnimatedModel from scratch
                             renderable, setAnimationFlagsDirty, renderable.Parent);
                     }
                     else
                     {
                         // ???
                         renderable.RenderAsModel.CustomColor0 = SnapshotRenderAsModel.CustomColor0;
                         renderable.RenderAsModel.CustomColor1 = SnapshotRenderAsModel.CustomColor1;
                         renderable.RenderAsModel.CustomColor2 = SnapshotRenderAsModel.CustomColor2;
                         renderable.RenderAsModel.CustomColor3 = SnapshotRenderAsModel.CustomColor3;

                         renderable.RenderAsModel.Location = SnapshotRenderAsModel.Location;
                         renderable.RenderAsModel.LocalTransform = SnapshotRenderAsModel.LocalTransform;
                         renderable.RenderAsModel.FacingNormal = SnapshotRenderAsModel.FacingNormal;

                         renderable.RenderAsModel.FinalModelScale = SnapshotRenderAsModel.FinalModelScale;

                         renderable.RenderAsModel.FinalModelName = SnapshotRenderAsModel.FinalModelName;
                         renderable.RenderAsModel.ModelData = GameData.Instance.AllModels[renderable.RenderAsModel.FinalModelName];

                         renderable.RenderAsModel.FinalModelBasicTextureName = SnapshotRenderAsModel.FinalModelBasicTextureName;
                         if (!string.IsNullOrEmpty(renderable.RenderAsModel.FinalModelBasicTextureName))
                         {
                             renderable.RenderAsModel.FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[renderable.RenderAsModel.FinalModelBasicTextureName];
                         }
                     }
                 }
             }
            

            #region ISnapshot

            public ISnapshot DoSnapshot(Snapshotter sn)
            {
                animConditions = (AnimConditions)sn.DoISnapshot(animConditions);
                spriteConditions = (BitMask64)sn.DoISnapshot(spriteConditions);
                flipHorizontally = sn.DoBool(flipHorizontally);
                renderEffect = sn.DoEnum(renderEffect);
                attachables = sn.DoList(attachables);

                SnapshotRenderAsModel = (SnapshotRenderasModel)sn.DoISnapshot(SnapshotRenderAsModel);


                return this;

            }

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

            public void LoadPostProcess(Snapshotter sn)
            {
                sn.RegisterLoadPostProcessCall(this);

                if (spriteConditions != null)
                {
                    spriteConditions.LoadPostProcess(sn);
                }

                if (animConditions != null)
                {
                    animConditions.LoadPostProcess(sn);
                }

                if (SnapshotRenderAsModel != null)
                {
                    SnapshotRenderAsModel.LoadPostProcess(sn);
                }
            }

            #endregion
        }


        public class SnapshotRenderasModel : ISnapshot
        {
            /* RenderAsModel fields, needed to recreate MemoryFact Renderables
           */
           public Vector3 CustomColor0;
           public Vector3 CustomColor1;
           public Vector3 CustomColor2;
           public Vector3 CustomColor3;


           public Vector3 FacingNormal = Vector3.UnitX;
           public float FinalModelScale = 1f;
           public string FinalModelName;
           public string FinalModelBasicTextureName;

           public Matrix LocalTransform = Matrix.Identity;
           public Vector3 Location = Vector3.Zero;

           public SnapshotRenderasModel(RenderAsModel renderAsModel)
           {
               this.CustomColor0 = renderAsModel.CustomColor0;
               this.CustomColor1 = renderAsModel.CustomColor1;
               this.CustomColor2 = renderAsModel.CustomColor2;
               this.CustomColor3 = renderAsModel.CustomColor3;

               this.FacingNormal = renderAsModel.FacingNormal;
               this.FinalModelScale = renderAsModel.FinalModelScale;
               this.FinalModelName = renderAsModel.FinalModelName;
               this.FinalModelBasicTextureName = renderAsModel.FinalModelBasicTextureName;
               this.LocalTransform = renderAsModel.LocalTransform;
               this.Location = renderAsModel.Location;

           }

           public SnapshotRenderasModel()
           {
               System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
           }

           /* 
          ModelData = original.ModelData;

          FinalModelScale = original.FinalModelScale;
          FinalModelBasicTexture = original.FinalModelBasicTexture;
          FinalModelName = original.FinalModelName;

          facingNormal = original.facingNormal;
          location = original.location;

          localTransform = original.localTransform;

          CustomColor0 = original.CustomColor0;
          CustomColor1 = original.CustomColor1;
          CustomColor2 = original.CustomColor2;
          CustomColor3 = original.CustomColor3;

          */

            #region ISnapshot

            public ISnapshot DoSnapshot(Snapshotter sn)
            {

                CustomColor0 = sn.DoVector3(CustomColor0);
                CustomColor1 = sn.DoVector3(CustomColor1);
                CustomColor2 = sn.DoVector3(CustomColor2);
                CustomColor3 = sn.DoVector3(CustomColor3);
                FacingNormal = sn.DoVector3(FacingNormal);
                Location = sn.DoVector3(Location);
                FinalModelScale = sn.DoFloat(FinalModelScale);
                FinalModelName = sn.DoString(FinalModelName);
                FinalModelBasicTextureName = sn.DoString(FinalModelBasicTextureName);
                LocalTransform = sn.DoMatrix(LocalTransform);

                return this;

            }

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

            public void LoadPostProcess(Snapshotter sn)
            {
                sn.RegisterLoadPostProcessCall(this);

              
            }

            #endregion
        }
        
    }


    //-----------------------------------------------------------------------------
    //* TintEnvelope handles the fading of the tint color up, down stable etc...
    //* assumes that 0,0,0, is the color for the AT REST state, used as decay target
    //* works like an ADSR envelope, 
    //* except that SUSTAIN and RELEASE are randomly (or never) triggered, externally 
    public class TintEnvelope
    {

        private Vector3 m_attackRate;		 	///< step amount to make tint turn on slow or fast 
        private Vector3 m_decayRate;			///< step amount to make tint turn off slow or fast
        private Color m_peakColor;			///< um, the peak color, what color we are headed toward during attack
        private Vector3 m_currentColor;		    ///< um, the current color, how we are colored, now
        private uint m_sustainCounter;
        private State m_envState;				///< a randomly switchable SUSTAIN state, release is compliment
        private bool m_affect;         ///< set TRUE if this has any effect (has a non 0,0,0 color).

        private float m_vibratoAmplitude;
        private float m_vibratoFrequency;
        private Vector3 m_vibratoColor;

        public enum State
        {
            Rest,
            Attack,
            Decay,
            Sustain ///< RELEASE IS THE LOGICAL COMPLIMENT TO SUSTAIN								
        };

        public TintEnvelope()
        {
            //TODO IMPL
        }
        public void update()
        {
            //TODO IMPL
        }
        public void play(Color peak, uint atackFrames = 8, uint decayFrames = 8, uint sustainAtPeak = 8)
        {
            //TODO IMPL
        }
        public void sustain() { m_envState = State.Sustain; }
        public void release() { m_envState = State.Decay; }
        public void rest() { m_envState = State.Rest; }
        public bool isEffective() { return m_affect; }
        public void saturate(Color color)
        {
            //TODO IMPL
        }

        public void setVibrato(float amplitude, float frequency)
        {
            //TODO IMPL
        }

        public Color GetColor()
        {
            return Color.White;
        }

        public void setSustain(uint x) { m_sustainCounter = x; }

        private void setAttackFrames(uint frames)
        {
            //TODO IMPL
        }
        private void setDecayFrames(uint frames)
        {
            //TODO IMPL
        }
        private void setPeakColor(Color peak) { m_peakColor = new Color(peak.R, peak.G, peak.B); }
        private void setPeakColor(float r, float g, float b) { m_peakColor = new Color(r, g, b); }



        

    };

}

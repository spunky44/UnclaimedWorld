using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Containers;
using System.Collections;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Entities.Locomotors
{
    /// <summary>
    /// Mark's idea was to make subclasses for thrown spear etc.
    /// I have decided to use has-a pattern instead
    /// 
    /// Perhaps move movement code from AI goals into this class too... difficult, since they are so intwined
    /// </summary>
    public class Locomotor : Component //: LocomotorBase
    {
        /// <summary>
        /// used for parking, obstacle avoidance etc.
        /// </summary>      
        public float BoundingRadius; //TODO DECOUPLE ??

        public enum Mode { None, Legged, Ballistic }

        private Mode currentMoveMode;
        public Mode CurrentMoveMode
        {
            get { return currentMoveMode; }
            set
            {
                currentMoveMode = value;
                if (CollisionResponder != null)
                {
                    CollisionResponder.SetCollisionResponse(value);
                }
            }
        }

        /// <summary>
        /// my idea is that only one of these locomotors should be active at a time - they have a corresponding enum value
        /// </summary>
        public LeggedLocomotor LeggedLocomotor;
        public BallisticLocomotor BallisticLocomotor;
        public CollisionResponder CollisionResponder;

        public Stance Stance;

        public Rotator Rotator;

        private float? previousRotation = null; //TODO DECOUPLE

        /// <summary>
        /// the speed we are currently turning with
        /// </summary>

        /// <summary>
        /// the realized speed
        /// </summary>
        public float MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
            set
            {

                moveSpeed = value;
            }
        } //TODO DECOUPLE
        private float moveSpeed = 0f;
        /// <summary>
        /// It is not enough to compare with the previous frame to detect motion. We need one more frame..
        /// </summary>
        private Vector3? previousLocation; //TODO DECOUPLE
        // these are used in AI evaluators:      
        // NEW! pixels per second
        public const float CommonLowestHaulingSpeed = 8f; //0.2f //TODO DECOUPLE
        public const float CommonCarryLimit = 1f; //TODO DECOUPLE

        /// <summary>
        /// used to stop lerping (position + move anim) when the entity is close to stopping.
        /// 
        /// If this is not up to date, the symptoms will be: slow drifting/not playing the idle anim/not playing combat anim
        /// </summary>
        public Vector3 CurrentMoveTarget;


        //   public Vector3 oldCurrentMoveTarget;
        float moveAbility = 1.0f;

        public float MoveAbility
        {
            get
            {
                return moveAbility;
            }

        }

        public override double? GetUpdateInterval()
        {
            if (!Parent.IsDead
                && Parent.IsCompleted()
                && Parent.IsOnPlaySite())
            {
                return 0;
            }

            return null;
        }


        /// <summary>
        /// after flipping, this property stores the 'real' rotation - used when left/right body parts matter.
        /// </summary>
        public float SymmetricCreatureRealRotation = 0f;

        /// <summary>
        /// for debug only
        /// </summary>
        public bool RotateSlowly = false;
        // public Entity Parent;//moved to base
        public EntityID targetEntity;
        public Vector3 direction = new Vector3(0);
        public float speed = 0f;
        public Vector3? targetLocation = null;
        public float optimumSpeed;

        /*   public delegate void DestinationReachedHandler(object sender, EventArgs e);
           public event DestinationReachedHandler DestinationReached;
           */
        public enum State
        {
            Idle, Moving, Stationery, Arrived
        }

        /// <summary>
        /// not currently used...
        /// </summary>
        public State state = State.Idle;

        private bool isColliding = false;
        public bool IsColliding
        {
            get
            {
                return isColliding;
            }
        }

        public void SetIsColliding(bool aValue)
        {
            isColliding = aValue;
        }

        //ctor
        /*  public LocomotorBase(Entity parent)
          {
              this.Parent = parent;
              //can this.Parent.EntityType.locomotorType be assumed non-null ????? MLo

              if (Parent != null)
                  SetState(State.Idle);


              //TODO MLo this is a hack to make locmotor owner start moving autonomously
              SetState(State.Moving);
              targetLocation = new Vector3(0);

          }*/

        public void SetState(State newState)
        {
            state = newState;
        }

        //CTOR
        public Locomotor(Entity parent)
            : base(parent)
        {

            //can this.Parent.EntityType.locomotorType be assumed non-null ????? MLo

            if (Parent != null)
                SetState(State.Idle);

            if (parent.EntityType.LocomotorType.LeggedLocomotorType != null)
            {
                LeggedLocomotor = new LeggedLocomotor(this);
            }

            if (parent.EntityType.LocomotorType.BallisticLocomotorType != null)
            {
                BallisticLocomotor = new BallisticLocomotor(this);
            }

            if (parent.EntityType.LocomotorType.CollisionResponderType != null)
            {
                CollisionResponder = new CollisionResponder(this);
            }

            if (parent.EntityType.LocomotorType.RotatorType != null)
            {
                Rotator = new Rotator(this);
            }

            if (parent.EntityType.LocomotorType.StancesType != null)
            {
                Stance = new Stance(this);
            }

            //TODO MLo this is a hack to make locmotor owner start moving autonomously
            SetState(State.Moving);
            targetLocation = new Vector3(0);

            if (LeggedLocomotor != null)
            {
                CurrentMoveMode = Mode.Legged;
            }
            else
            {
                CurrentMoveMode = Locomotor.Mode.None; // not moving yet...
            }



            // new: set random rotation (can be overridden later):
            //  SetRotationAndDir((float)(Globals.Instance.RandomPredictable.NextDouble() * MathHelper.TwoPi));
        }

        public Locomotor()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.BallisticLocomotor = (BallisticLocomotor)sn.DoISnapshot(BallisticLocomotor);
            this.BoundingRadius = sn.DoFloat(BoundingRadius);
            this.CollisionResponder = (CollisionResponder)sn.DoISnapshot(CollisionResponder);
            this.currentMaximumSpeed = sn.DoFloat(currentMaximumSpeed);
            this.currentMaximumSpeedNotAffectedByTerrain = sn.DoFloat(currentMaximumSpeedNotAffectedByTerrain);
            this.currentMaximumSpeedForEvaluator = sn.DoFloat(currentMaximumSpeedForEvaluator);
            this.CurrentMaximumSpeedIsDirty = sn.DoBool(CurrentMaximumSpeedIsDirty);
            this.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = sn.DoBool(CurrentMaximumSpeedNotAffectedByTerrainIsDirty);
            this.CurrentMaximumSpeedForEvaluatorIsDirty = sn.DoBool(CurrentMaximumSpeedForEvaluatorIsDirty);          
            this.currentMoveMode = sn.DoEnum(currentMoveMode);
            this.CurrentMoveTarget = sn.DoVector3(CurrentMoveTarget);
            this.direction = sn.DoVector3(direction);
            this.isColliding = sn.DoBool(isColliding);
            // this.IsMoving = sn.DoBool(IsMoving);
            this.IsMovingOrRotating = sn.DoBool(IsMovingOrRotating);
            this.LeggedLocomotor = (LeggedLocomotor)sn.DoISnapshot(LeggedLocomotor);
            this.Stance = (Stance)sn.DoISnapshot(Stance);
            this.MaximumSpeedDebugOnly = sn.DoFloatNullable(MaximumSpeedDebugOnly);
            this.moveAbility = sn.DoFloat(moveAbility);
            this.MoveSpeed = sn.DoFloat(MoveSpeed);
            this.optimumSpeed = sn.DoFloat(optimumSpeed);
            this.previousLocation = sn.DoVector3Nullable(previousLocation);
            this.previousRotation = sn.DoFloatNullable(previousRotation);
            this.RotateSlowly = sn.DoBool(RotateSlowly);
            this.speed = sn.DoFloat(speed);
            this.state = sn.DoEnum(state);
            this.SymmetricCreatureRealRotation = sn.DoFloat(SymmetricCreatureRealRotation);
            this.targetEntity = sn.DoEntityID(targetEntity);
            this.targetLocation = sn.DoVector3Nullable(targetLocation);
            this.terrainModifier = sn.DoFloat(terrainModifier);
            this.Rotator = (Rotator)sn.DoISnapshot(Rotator);




            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (LeggedLocomotor != null)
            {
                LeggedLocomotor.Parent = this;
                LeggedLocomotor.LoadPostProcess(sn);
            }

            if (CollisionResponder != null)
            {
                CollisionResponder.Parent = this;
                CollisionResponder.LoadPostProcess(sn);
            }

            if (BallisticLocomotor != null)
            {
                BallisticLocomotor.Parent = this;
                BallisticLocomotor.LoadPostProcess(sn);
            }

            if (Rotator != null)
            {
                Rotator.Parent = this;
                Rotator.LoadPostProcess(sn);
            }

            if (Stance != null)
            {
                Stance.Parent = this;
                Stance.LoadPostProcess(sn);
            }

        }

        /// <summary>
        /// the copy of locomotor only needs the data needed to render the memoryfact
        /// </summary>
        /// <param name="original"></param>
        /*   public Locomotor(Locomotor original): base(null)
           {
               FacingNormal = original.FacingNormal;
               Rotation = original.Rotation;
            
           }*/

        public void Initialize(Game game)
        {
        }


        public void OnDestinationReached()
        {
            /* if (DestinationReached != null)
             {
                 DestinationReached.Invoke(this, null);
             }*/
        }

        public void FlipFourSidedSymmetryCreature(float rotation)
        {

            if (Parent.Locomotor == null)
            {
                string name = Parent.Name;
                throw new Exception("no locomotor in " + name);
            }


            // make sure we store the real rotation before doing the flip...
            SymmetricCreatureRealRotation = Parent.Rotation;

            Parent.SetRotationAndDir(rotation);
        }

        /// <summary>
        /// for the evaluators, don't include burden state in the scoring, because this can cause job switching when the score is high before starting, but low after picking up the needed tools...
        /// </summary>
        /// <param name="totalBulkForCalculation"></param>
        /// <param name="calculateWithTerrain"></param>
        /// <returns></returns>
        public float CalculateSpeed(float? totalBulkForCalculation = null, bool calculateWithTerrain = true, bool calculateWithBurden = true)
        {
            // get the speed the AI wants to move by:
            float baseSpeed;
            if (!calculateWithBurden)
            {
                baseSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed;
            }
            else 
            {
                baseSpeed = GetTargetSpeed();
            }

            float speed = baseSpeed;


            // apply all modifiers - most likely this will slow the speed down:
            // if needed, we can add more complex logic here, such as clamping, overriding modifiers etc.
            ApplyDisabilityModifier(ref speed);

            if (calculateWithBurden == true)
            {
                ApplyBurdenModifier(ref speed, totalBulkForCalculation);
            }

            if (calculateWithTerrain == true)
            {
                ApplyTerrainModifier(ref speed);
            }

            // add more here:


            // clamping??? no, disability can give 0 speed.
            // speed = Common.ClampBottom(speed, 0.5f * baseSpeed); 

            return speed;
        }

        private void RecomputeCurrentMaximumSpeedForEvaluator()
        {
            currentMaximumSpeedForEvaluator = CalculateSpeed(null, false, false);
            CurrentMaximumSpeedForEvaluatorIsDirty = false;
        }

        private void RecomputeCurrentMaximumSpeedNoTerrain()
        {
            currentMaximumSpeedNotAffectedByTerrain = CalculateSpeed(null, false);
            CurrentMaximumSpeedNotAffectedByTerrainIsDirty = false;
        }


        private void RecomputeCurrentMaximumSpeed()
        {
            currentMaximumSpeed = CalculateSpeed();

            CurrentMaximumSpeedIsDirty = false;
        }

        public void ImpairMovement(float aMovementPartToRemove)
        {
            moveAbility -= aMovementPartToRemove;
            if (moveAbility < 0)
            {
                moveAbility = 0.0f;
            }
            CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
            CurrentMaximumSpeedForEvaluatorIsDirty = true;
            CurrentMaximumSpeedIsDirty = true;
        }

        public void ToggleImmobilize()
        {
            if (!Common.IsZero(moveAbility))
            {
                moveAbility = 0f;
            }
            else
            {
                moveAbility = 1f;
            }

            CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
            CurrentMaximumSpeedForEvaluatorIsDirty = true;
            CurrentMaximumSpeedIsDirty = true;
        }


        /// <summary>
        /// returns the speed the entity AI is currently targeting (as a trade off of energy expenditure, urgency...)
        /// </summary>
        /// <returns></returns>
        public float GetTargetSpeed()
        {

            float targetSpeed = 0.0f;
            if (LeggedLocomotor == null)
                return 1;//TODO HACK MLo, get rid of MobileENtity class, encorporate into locomotor

            // get the speed the AI wants to move by:
            switch (LeggedLocomotor.TargetSpeed)
            {
                case Goal.MovementSpeeds.WalkSlowly:
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkSlowSpeed;
                    break;
                case Goal.MovementSpeeds.Normal:
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed;
                    break;
                case Goal.MovementSpeeds.WalkFast:
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed;
                    break;
                case Goal.MovementSpeeds.Run:
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.RunSpeed;
                    break;
                case Goal.MovementSpeeds.Haul:                   
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.HaulSpeed;
                    break;
                default:
                    targetSpeed = Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed;
                    break;
            }


            return targetSpeed;
        }

        #region Speed modifiers

        private void ApplyDisabilityModifier(ref float speed)
        {
            speed *= moveAbility;
        }

        private void ApplyBurdenModifier(ref float speed, float? totalBulkForCalculation = null)
        {
            ItemStorage storage = Parent.AgentStorage != null ? Parent.AgentStorage.ItemStorage : null;

            float totalBulk = 0f;
            if (totalBulkForCalculation != null)
            {
                totalBulk = totalBulkForCalculation.Value;
            }
            else if (storage != null)
            {
                totalBulk = storage.TotalStored;
            }


            if (Parent.Vehicle == null)
            {
                // people...

                if (storage != null
                    && Parent.AgentStorage.GetCurrentBurdenState() == AgentStorage.BurdenState.HaulHeavy)
                {
                    // only move slower if the box hauling anim is playing...        // ItemStorage.TotalStored / ItemStorage.TotalCapacity;
                    float burdenModifier = MathHelper.Lerp(1f, 0.6f, totalBulk / storage.TotalCapacity);
                    speed = burdenModifier * speed;
                }
            }
            else
            {
                // vehicles... TODO: no agent storage

                if (storage != null)
                {
                    speed = speed * ((2f / 3f) + ((storage.TotalCapacity - totalBulk) / storage.TotalCapacity) / 3f);
                }              
            }

        }

        /// <summary>
        /// 0 - 1
        /// </summary>
        private float terrainModifier = 1f;
        public void SetTerrainModifier(Point sampleSubtile)
        {
            float roughness = The.Map.GetRoughness(sampleSubtile);

            float negatedRoughness = roughness * (1f - Parent.EntityType.LocomotorType.LeggedLocomotorType.TerrainNegateFactor);

            // TODO: set anim flag when moving through difficult terrain

            terrainModifier = (1f - negatedRoughness);

            CurrentMaximumSpeedIsDirty = true;
        }

        private void ApplyTerrainModifier(ref float speed)
        {
            if (speed > GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect)
            {
                // TODO: set anim flag when moving through difficult terrain

                float newSpeed = terrainModifier * speed;

                if (newSpeed < speed) // terrain won't increase speed
                {
                    speed = Common.ClampBottom(newSpeed,
                        GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect);
                }
            }

        }

        #endregion

        /// <summary>
        /// perhaps make this 
        /// </summary>
        /// <returns></returns>
        public float GetAcceleration()
        {
            if (LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run)
            {
                return 100f;
            }
            else
            {
                return 60f; // 6000; // 160; // 60f;
            }
        }


        public float? MaximumSpeedDebugOnly;


        private float currentMaximumSpeed = 0.5f; 
        public bool CurrentMaximumSpeedIsDirty = true; // hmmmmmm public... 
        /// <summary>
        /// The speed that the agent will accelerate towards.
        /// In pixels per second
        /// </summary>
        public float CurrentMaximumSpeed
        {
            get
            {
               /* if (Parent.PersonEntity != null && Parent.Name.Contains("Augustine Yeboah"))
                {

                }*/

                if (CurrentMaximumSpeedIsDirty)
                {
                    RecomputeCurrentMaximumSpeed();
                }

#if DEBUG || PROFILE
                if (MaximumSpeedDebugOnly.HasValue)
                {
                    return MaximumSpeedDebugOnly.Value;
                }
#endif

                return currentMaximumSpeed;
            }

        }

        private float currentMaximumSpeedNotAffectedByTerrain = 0.5f;
        public bool CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true; // hmmmmmm public... 
        public float CurrentMaximumSpeedNoTerrain
        {
            get
            {
                if (CurrentMaximumSpeedNotAffectedByTerrainIsDirty)
                {
                    RecomputeCurrentMaximumSpeedNoTerrain();
                }

                return currentMaximumSpeedNotAffectedByTerrain;
            }

        }

        private float currentMaximumSpeedForEvaluator = 0.5f;
        public bool CurrentMaximumSpeedForEvaluatorIsDirty = true; 
        /// <summary>
        /// safe to use by evaluators and will not cause job switching. 
        /// </summary>
        public float CurrentMaximumSpeedForEvaluator
        {
            get
            {
                if (CurrentMaximumSpeedForEvaluatorIsDirty)
                {
                    RecomputeCurrentMaximumSpeedForEvaluator();
                }

                return currentMaximumSpeedForEvaluator;
            }

        }

        

        public bool IsMovingOrRotating = false; //TODO DECOUPLE
        public bool IsMoving()
        {
            if (MoveSpeed > 0)
            {
                return true;
            }
            return false;
        }




      //  public override void Update(GameTime gameTime)
        public override void UpdatePlaySite(GameTime gameTime)
        {

            //LOCOMOTORY UPDATE STUFF ///////////////////////////////////////////////////
            // let's keep track of the objects movements here:

            if (previousLocation != Parent.Location && MoveSpeed > 0f)
            // sometimes the two locations will be equal even though we should be moving??? use MoveSpeed to correct this... 
            //it seems to work. otherwise, add more variables to store more frames
            {
                IsMovingOrRotating = true;
                //IsMoving = true;

            }
            else if (previousRotation != Parent.Rotation)
            {
                IsMovingOrRotating = true;
                //IsMoving = false;
            }
            else
            {
                IsMovingOrRotating = false;
                //IsMoving = false;

            }

            previousLocation = Parent.Location;
            previousRotation = Parent.Rotation;



            Entity target = Entity.FindByID(targetEntity);
            if (target != null) // we are trying to reach a taret entity
            {
                MoveTowardsEntity(target, speed);
            }
            else if (targetLocation != null)// or a goal position instead of an entity
            {
                MoveTowardsLocation(gameTime);
            }
           
        }



        public void ReactToCollisions()
        {

            /* if (Parent.CollisionProxy != null)
             {*/
            if (CollisionResponder != null) // not all entities will react to collisions
                CollisionResponder.ReactToCollisions();

            /* switch (CurrentMoveMode)
             {
                 case Mode.Legged:
                     LeggedLocomotor.ReactToCollisions();
                     break;
             }*/
        }

        public static float resistance = 1.3f; // new Vector2(1.3f);
        /*  public Vector2 GetResistance()
          {
              return resistance;
          }*/




        public void ApplyPushVector(Entity other, Vector2 pushVector)
        {
            //normalize and add the pushVector to our Location
            pushVector.Normalize();

            float resistanceToUse;

            if (other.EntityType.LocomotorType == null || other.EntityType.LocomotorType.CollisionResponderType == null)
            {
                resistanceToUse = GameData.Instance.Constants.CollisionResistanceFromNonMovers; //makes building/terrain perimeters more forgiving than movers              
            }
            else
            {
                resistanceToUse = GameData.Instance.Constants.CollisionResistanceFromMovers;  //resistance; // / forgivenessOfOtherEntity;//Get owner entity strengh and apply to normalized pushvector
            }

            pushVector *= resistanceToUse;

            // move the entity... and clamp if we cross into blocked areas!
            Vector3 newLocation = Parent.PlaySiteLocation + pushVector.ToVector3();
            newLocation = The.Map.ClampWorldPosition(newLocation);
            Parent.Location = Parent.ModifyNewLocationToStayOnFreeTerrain(newLocation, pushVector);
        }



        private bool IsEntityIdle(Entity entity)
        {
            return entity.Intelligence.IsIdle();
        }



        /*
         public virtual State Update(GameTime gameTime)
        {

            return State.Idle;//TODO HACK TO CRIPPLE THIS BASE CLASS DURING DECOUPLING

            //a simple way to stop locomotor without de-initializing its state
            if (state == State.Idle)
                return state;



            if (targetEntity != null) // we are trying to reach a taret entity
            {
                return MoveTowardsEntity(targetEntity, speed);
            }
            else if (targetLocation != null)// or a goal position instead of an entity
            {
                return MoveTowardsLocation(targetLocation, speed);
            }

            return state;

        }*/



        /// <summary>
        /// start moving in a ballistic trajectory
        /// </summary>
        /// <param name="from"></param>
        /// <param name="velocity"></param>
        public void Launch(Vector3 from, Vector3 velocity)
        {
            if (BallisticLocomotor != null)
            {
                CurrentMoveMode = Mode.Ballistic;

                BallisticLocomotor.Launch(from, velocity);
            }
        }

        public void StartMoving(Mode mode, Vector3 target, float speed, float upwardsSpeed, EntityID? launchedByEntity, Allegiances.Allegiance launchedByAllegiance, AttackType attackType, OwnerID? ownerOfCarcass, AttackJob job)
        {
            CurrentMoveMode = mode;
            switch (mode)
            {
                case Mode.None:
                case Mode.Legged:
                    break;

                case Mode.Ballistic:
                    BallisticLocomotor.StartMoving(target, speed, upwardsSpeed, launchedByEntity, launchedByAllegiance, attackType, ownerOfCarcass, job);
                    break;
            }
        }

        public State MoveTowardsLocation(GameTime elapsed)
        {

            switch (this.CurrentMoveMode)
            {
                case Mode.None:
                case Mode.Legged:
                    break;

                case Mode.Ballistic:
                    BallisticLocomotor.MoveTowardsLocation(elapsed);
                    break;

            }



            return (state = State.Idle);
        }

        /// <summary>
        /// Lars: not sure how this method is supposed to work... perhaps to ensure that the thrown entity always hits its target?
        /// </summary>
        /// <param name="target"></param>
        /// <param name="optimumSpeed"></param>
        /// <returns></returns>
        public State MoveTowardsEntity(Entity target, float optimumSpeed)
        {
            switch (Parent.EntityType.LocomotorType.appearance)
            {
                case LocomotorType.Appearance.Thrown:
                    {
                        //TODO make this not just a curly shuffle
                        //     Parent.Renderable.RenderAsModel.SetRotationAndDir(0.1f); //TODO DECOUPLE -- move fn to this class
                        //Parent.Locomotor.NormalizedMoveDir = XXX;
                        //     Parent.Locomotor.MoveSpeed = 1.0f; //TODO DECOUPLE -- move fn to this class
                        //TODO these movement properties of RenderAsModel should be migrated to this class instead
                        //so that Locmotor updates the Location of RenderAsModel only, and RenderAsModel does not track movement states

                        return (state = State.Moving);
                    }


            }

            return (state = State.Idle);
        }
    }




    /*

    public class LocomotorBase : Component // TODO DECOUPLE scour out all these spear-implementations into a projectile locomotor class
    {
        public Entity Parent;
        public Entity targetEntity;
        public Vector3 direction = new Vector3(0);
        public float speed = 0f;
        public Vector3? targetLocation = null;
        public float optimumSpeed;

        public enum State
        {
            Idle, Moving, Stationery, Arrived
        }

        public State state = State.Idle;

        private bool isColliding = false;
        public bool IsColliding
        {
            get
            {
                return isColliding;
            }
        }

        public void SetIsColliding(bool aValue)
        {
            isColliding = aValue;
        }

        //ctor
        public LocomotorBase(Entity parent)
        {
            this.Parent = parent;
            //can this.Parent.EntityType.locomotorType be assumed non-null ????? MLo

            if (Parent != null)
                SetState(State.Idle);


            //TODO MLo this is a hack to make locmotor owner start moving autonomously
            SetState(State.Moving);
            targetLocation = new Vector3(0);

        }

        public void SetState(State newState)
        {
            state = newState;
        }


        public virtual void Initialize(Game game)
        {
        }





        public virtual State Update(GameTime gameTime)
        {

            return State.Idle;//TODO HACK TO CRIPPLE THIS BASE CLASS DURING DECOUPLING

            //a simple way to stop locomotor without de-initializing its state
            if (state == State.Idle)
                return state;



            if (targetEntity != null) // we are trying to reach a taret entity
            {
                return MoveTowardsEntity(targetEntity, speed);
            }
            else if (targetLocation != null)// or a goal position instead of an entity
            {
                return MoveTowardsLocation(targetLocation, speed);
            }

            return state;

        }

        public double aa = 0;


        public State MoveTowardsLocation(Vector3? goal, float optimumSpeed)
        {
            switch (Parent.EntityType.LocomotorType.appearance)
            {
                // TODO: this should control the movement of the spear and arrow
                case LocomotorType.Appearance.Thrown:
                    {
                        //TODO make this not just a curly shuffle
                        //          Rotation += 0.01f;
                        //          SetRotationAndDir(Rotation); //TODO DECOUPLE -- move to this class

                        Vector3 pos = Parent.Location;

                        pos.Z = (float)(30.0 + (Math.Sin(aa += 0.01) * 30.0));

                        Parent.SetPosition(pos);


                        return (state = State.Moving);
                    }


            }

            return (state = State.Idle);
        }

        /// <summary>
        /// Lars: not sure how this method is supposed to work... perhaps to ensure that the thrown entity always hits its target?
        /// </summary>
        /// <param name="target"></param>
        /// <param name="optimumSpeed"></param>
        /// <returns></returns>
        public State MoveTowardsEntity(Entity target, float optimumSpeed)
        {
            switch (Parent.EntityType.LocomotorType.appearance)
            {
                case LocomotorType.Appearance.Thrown:
                    {
                        //TODO make this not just a curly shuffle
                        //     Parent.Renderable.RenderAsModel.SetRotationAndDir(0.1f); //TODO DECOUPLE -- move fn to this class
                        //Parent.Locomotor.NormalizedMoveDir = XXX;
                        //     Parent.Locomotor.MoveSpeed = 1.0f; //TODO DECOUPLE -- move fn to this class
                        //TODO these movement properties of RenderAsModel should be migrated to this class instead
                        //so that Locmotor updates the Location of RenderAsModel only, and RenderAsModel does not track movement states

                        return (state = State.Moving);
                    }


            }

            return (state = State.Idle);
        }

    }*/
}






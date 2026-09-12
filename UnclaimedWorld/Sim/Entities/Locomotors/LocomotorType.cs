using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Locomotors.Stances;

namespace UWGame.SimSide.Entities.Locomotors
{
    
    public class LocomotorType //: IGameData
    {
        //many of these are non-generic, and should be shelled out to subclasses
     /*   public float maxSpeed = 1f;
        public float minSpeed = 0f;
        public float lift = 0f;
        public float gravity = 0.1f;
        public Vector2? tumble;
        public bool canMoveBackwards = false;
        */

        public bool CanRun = false;

        /// <summary>
        /// don't use this - may be overridden in BiologicalEntityType
        /// </summary>
        public bool FourSidedSymmetry = false;// TODO MLo: is this a client property? research!

        /// <summary>
        /// can be different than bounding radius...
        /// it is like a circular base in a tabletop/Warhammer game. If two units want to fight, their bases must touch.
        /// </summary>
        public float MeleeRadius;

        public LeggedLocomotorType LeggedLocomotorType;
        public BallisticLocomotorType BallisticLocomotorType;


        public CollisionResponderType CollisionResponderType;

        public float MaxAngularSpeed = 1.5f * MathHelper.Pi;

        /// <summary>
        /// if present, allows a part of the entity to rotate during GoalTurnToFace instead of the whole body.
        /// </summary>
        public RotatorType RotatorType;

        public string Stances;

        [XmlIgnore]
        public StancesType StancesType;
        

        /// <summary>
        /// these enums may become classes...
        /// </summary>
        public enum Surface
        {
            Ground, WaterFloat, WaterFord, Air, Obstacle 
        };
        public Surface surface = Surface.Ground;
        
        /// <summary>
        /// should these enums trigger creation of a subclass of Locomotor?
        /// </summary>
        public enum Appearance
        {
            Biped, Quadruped, Car, Bike, Treads, Hover, Wings, Boat, Thrown
        };
        public Appearance appearance = Appearance.Biped;

        /// <summary>
        /// perhaps make the subclass a contained class instead?
        /// </summary>
        public enum ZBehavior
        {
            Ground, SeaLevel, SurfaceRelative, Ballistic, Bounce
        }
        public ZBehavior zBehavior = ZBehavior.Ground;

        public enum AxialBehavior
        {
            Plumb, Tumble, Roll, Bank, Fletch, Corkscrew
        }
        public AxialBehavior axialBehavior = AxialBehavior.Plumb;

        //TODO later on, perhaps move the thrust, braking, turning, skidding, coasting,
        //rudder, stiffness, suspension, traction, cornering, sideslip, drift, climb
        //banking, hovering, pitching yawing, damping and damage effect logic to here
        //instead of GoalTraverseEdgeBetweenWaypointsAtomic::Move, and Entity::Update


       /* public string KeyName
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }

        public bool DeleteRecord
        {
            get;
            set;
        }*/

        public void Initialize()
        {
            if (Stances != null)
            {
                StancesType = GameData.Instance.AllStancesTypes[Stances];
            }
        }


        public void PreInitValidate(List<string> listOfErrors)
        {


        }

        public void PostInitValidate(List<string> listOfErrors)
        {

        }


       
                
    }

    
}

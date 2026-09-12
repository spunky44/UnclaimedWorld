using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors
{
    /// <summary>
    /// the entity can move on legs when this locomotor is active
    /// 
    /// for robots on wheels, some behaviour will be shared I think..
    /// </summary>
    public class LeggedLocomotor : ISnapshot
    {
        //TODO: move these stats to other class
        public float Strength;
        public float Agility;

       // public enum Stance { Lying, Sitting, Kneeling, Standing };
      //  private Stance currentStance = Stance.Standing;
     /*   private StanceType currentStance;
        public Stance CurrentStance
        {
            get
            {
                return currentStance;
            }
            set
            {
                currentStance = value;
            }
        }
        */
        public Locomotor Parent;  

        private Goal.MovementSpeeds currentMovementSpeedType = Goal.MovementSpeeds.Normal;

        /// <summary>
        /// the speed the AI wants to move by - modifiers (terain, disability etc.) will be applied to this.
        /// </summary>
        public Goal.MovementSpeeds TargetSpeed
        {
            get
            {
                return currentMovementSpeedType;
            }
            set 
            {
                if (currentMovementSpeedType != value)
                {
                    currentMovementSpeedType = value;
                    Parent.CurrentMaximumSpeedIsDirty = true;
                    Parent.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                    // does not affect evaluator speed.
                }
            }
        }

        public bool TestWander = false;

        public LeggedLocomotor()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public LeggedLocomotor(Locomotor parent)
        {
            this.Parent = parent;

            Strength = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.5f, 0.1f);
            Agility = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.5f, 0.1f);

        }


        /// <summary>
        /// null is for standing
        /// </summary>
        /// <param name="stance"></param>
        /// <returns></returns>
      /*  public static AnimModifier? GetCorrespondingAnimFlag(Stance stance)
        {
            switch (stance)
            {
                case Stance.Kneeling:
                    return AnimModifier.Kneeling;

                case Stance.Sitting:
                    return AnimModifier.Sitting;

                case Stance.Lying:
                    return AnimModifier.Lying;

                case Stance.Standing:
                    return null;

                default:
                    return null;
            }
        }*/


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
            this.Agility = sn.DoFloat(Agility);
            this.currentMovementSpeedType = (Goal.MovementSpeeds)sn.DoEnum(currentMovementSpeedType);            
            this.Strength = sn.DoFloat(Strength);
            this.TestWander = sn.DoBool(TestWander);

            sn.Ignore(Parent); // is re-assigned from Locomotor

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }


        #endregion


    }
}

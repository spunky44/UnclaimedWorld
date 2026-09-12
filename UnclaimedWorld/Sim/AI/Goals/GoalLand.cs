using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using Xclna.Xna.Animation;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    public class GoalLand: Goal
    {
        public Entity Aircraft;
        private Vehicle vehicleComponent;

        private Vector3 destinationPoint;

        public GoalLand(Entity owner)
            : base(owner)
        {
            
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



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            sn.DoUnknownObject(Aircraft);//TODO use lookup
            sn.DoUnknownObject(vehicleComponent);//TODO use lookup
            this.destinationPoint = sn.DoVector3(destinationPoint);

            return this;
        }

        public GoalLand()
        {
        }


        protected override void Activate()
        {
            if (!entity.GetDrivenVehicle(out Aircraft))
            {
                Status = Goals.Status.Failed;
                return;
            }
            Aircraft.Find(out vehicleComponent);



            Status = Status.Active;
            destinationPoint = new Vector3(MapManager.tileSize * (0.5f + entity.MapPosition.Value.X),
                   MapManager.tileSize * (0.5f + entity.MapPosition.Value.Y), 0f);

     /*       if (UWGame.SimSide.Instance.map.TileMap[entity.MapPosition.X, entity.MapPosition.Y].ContainsItem(meal))
            {
                Status = Status.Active;
                secondsToEat = Common.RandomSpread(Globals.Instance.Random, baseSecondsToEat, secondsToEatSpread);
                
            }
            else
            {
                // cannot get the item. Fail.
                Status = Status.Failed;
            }*/

        }
        public override bool IsSame(Job job)
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
        /*    ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/

                float desiredMoveSpeed = - ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxVerticalMoveSpeed;

                if (GoalTakeoff.UpdateAircraftUpDown(desiredMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
                {
                    Aircraft.Locomotor.MoveSpeed = 0f;
                    //  Aircraft.Renderable.RenderAsModel.RotationSpeed = 0f;
                    vehicleComponent.Aircraft.VerticalSpeed = 0f;

                    // put us on the ground:
                    entity.Location = new Vector3(entity.PlaySiteLocation.X, entity.PlaySiteLocation.Y, 0f);

                    // stop engines. don't wait for them to stop before getting out...
                    Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", true);
                    Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", true);

                    Aircraft.Renderable.StartAdditionalAnimation("propeller_right_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal); //TODO DECOUPLE
                    Aircraft.Renderable.StartAdditionalAnimation("propeller_left_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal); //TODO DECOUPLE

                    Status = Status.Completed;
                }
                else
                {
                    // still in the air, kick up some dust:
                    GoalTakeoff.AddSkimmerDust(Aircraft);

                    /* float dustIntensity = (GoalTakeoff.TargetAltitude - Aircraft.Location.Z) / GoalTakeoff.TargetAltitude;
                     dustIntensity *= UWGame.SimSide.Instance.Map.TileMap[Aircraft.MapPosition.X, Aircraft.MapPosition.Y].GetDustFactor();
                     UWGame.SimSide.Instance.particleManager.AddDust(Aircraft, Aircraft.Location, dustIntensity, GameConstants.SkimmerDustScale);*/
                }
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/

        }
       
    }
}

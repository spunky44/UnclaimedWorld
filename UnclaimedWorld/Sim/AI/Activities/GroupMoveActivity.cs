using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.Control;
namespace UWGame.SimSide.AI.Activities
{
   

    public class GroupMoveActivity: Activity
    {
      //  public enum Formation { LooseGroup, Pair, Column, Wedge }
        public Entity Leader;

        private Matrix? leadersRotationMatrix;
        public Matrix LeadersRotationMatrix
        {
            get
            {
                if (leadersRotationMatrix != null)
                {
                    return leadersRotationMatrix.Value;
                }
                else
                {
                    ComputeLeadersRotationMatrix();                    
                    return leadersRotationMatrix.Value;
                }                
            }
        }

        public void ComputeLeadersRotationMatrix()
        {
            leadersRotationMatrix = Matrix.CreateRotationZ(Leader.Rotation);//TODO DECOUPLE
        }

        public Formation MoveFormation = new VFormation(); //new LooseGroupFormation(); //  

        public Vector3 Destination;

        private const float fullSpeedModifier = 2.2f;
        private const float minimumSpeedModifier = 0.5f;

        //public Dictionary<
       // public Dictionary<Entity, AI.Goals.GoalFollowPath.Waypoint> MemberWaypoints = new Dictionary<Entity, global::UWGame.SimSide.AI.Goals.GoalFollowPath.Waypoint>();


        public GroupMoveActivity(List<Activity> addToList, Vector3 destination)
            : base(addToList)
        {
            Destination = destination;
        }


        public bool IsLeader(Entity entity)
        {
            return Leader == entity;
        }

        /// <summary>
        /// is entity a follower, NOT a leader
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsFollower(Entity entity)
        {
            if (Leader != entity)
            {
                return Members.Contains(entity);
            }
            else return false;
        }

        public override void AddMember(Entity member)
        {
            base.AddMember(member);

            MoveFormation.FormationPositions.Add(member, MoveFormation.ComputePosition());
            MoveFormation.FormationPositionDrifts.Add(member, Vector2.Zero);
        }

        public override void LeaveActivity(Entity entity)
        {
            base.LeaveActivity(entity);

            Intelligence intelligenceComponent = entity.Intelligence;

            intelligenceComponent.GroupMoveAssignedWaypoint = null;
            intelligenceComponent.IsAtGroupMoveDestination = null;
            //entity.GroupMoveIsCatchingUp = null;
            intelligenceComponent.FollowerStatus = FollowerStatus.Normal;

            MoveFormation.LeaveFormation(entity);

            // one-man activities allowed? others may join...
            if (Members.Count > 0)
            {
                if (Leader == entity)
                {   // select new leader
                    Leader = Members[0];
                }
            }
           
        }

        public bool AllAreReady()
        {
            foreach (Entity member in Members)
            {
                if (member.Intelligence.IsAtGroupMoveDestination != true)
                {
                    return false;
                }
            }
            return true;
        }

        public bool EveryoneElseIsReady(Entity me)
        {
            foreach (Entity member in Members)
            {
                if (member != me && member.Intelligence.IsAtGroupMoveDestination != true)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3 ComputePositionFromLeader(Entity member, Vector3 leadersPosition, Matrix rotMatrix)
        {
            Vector2 offset = MoveFormation.FormationPositions[member];
            // drift up to 2 pixels from previous drift:
            Vector2 newDrift = MoveFormation.FormationPositionDrifts[member] +
                new Vector2(The.Sim.GameplayRandomGenerator.RandomBetween(-2f, 2f),
                    The.Sim.GameplayRandomGenerator.RandomBetween(-2f, 2f));                

            // clamp the new drift:
          //  Vector2 upperLeftDriftBorder = new Vector2(-8f, -8f); // new Vector2(offset.X - 8f, offset.Y - 8f);
          //  Vector2 lowerRightDriftBorder = //new Vector2(offset.X + 8f, offset.Y + 8f);

            newDrift.X = MathHelper.Clamp(newDrift.X, -8f, 8f); //upperLeftDriftBorder.X, lowerRightDriftBorder.X);
            newDrift.Y = MathHelper.Clamp(newDrift.Y, -8f, 8f);//upperLeftDriftBorder.Y, lowerRightDriftBorder.Y);

            MoveFormation.FormationPositionDrifts[member] = newDrift;

            // rotate the offset by leader heading
            offset = Vector2.TransformNormal(offset + newDrift, rotMatrix);

            // add to leaders waypoint and clamp to map:
            return The.Map.ClampWorldPosition(new Vector3(leadersPosition.X + offset.X, leadersPosition.Y + offset.Y, 0f));
           
        }

        public float ComputeSpeedModifier(Entity member) //, Vector2 leadersCurrentPosition, Matrix leadersRotationMatrix)
        {
            Intelligence intelComponent = member.Intelligence;

            if (intelComponent.FollowerStatus == FollowerStatus.IsFollowingPathToWaypoint
                || intelComponent.FollowerStatus == FollowerStatus.IsCatchingUp)
            {
                // go max speed
                return fullSpeedModifier;
            }
            else 
            {
                Vector2 leadersCurrentPosition = Leader.Location.Value.ToVector2();

                Vector2 offset = MoveFormation.FormationPositions[member];
                Vector2 drift = MoveFormation.FormationPositionDrifts[member];

                // rotate the offset by current leader heading
                offset = Vector2.TransformNormal(offset + drift, LeadersRotationMatrix);

                Vector2 vectorToPosition = (offset + leadersCurrentPosition) - member.Location.Value.ToVector2();

                float distanceToformationPosition = vectorToPosition.Length();// Vector2.Distance(member.GetGroundLocation(), offset + leadersCurrentPosition);
                vectorToPosition = vectorToPosition / distanceToformationPosition;

                float memberMovementDotLeaderHeading = Vector2.Dot(vectorToPosition, new Vector2(Leader.FacingNormal.X, Leader.FacingNormal.Y));

                if (Math.Abs(memberMovementDotLeaderHeading) < 0.5f)
                { // moving 'sideways' - full speed?
                    return fullSpeedModifier; //1f; 
                }
                else
                {
                    int sign = Math.Sign(memberMovementDotLeaderHeading);
                    float modifier;

                    if (sign < 0)
                    {
                        modifier = MathHelper.Clamp(distanceToformationPosition / 40f, 0f, minimumSpeedModifier);
                        // if we are ahead, make modifier < 1:
                        return 1f - modifier;
                    }
                    else
                    {
                        // we are behind:
                        modifier = MathHelper.Clamp(distanceToformationPosition / 40f, 0f, fullSpeedModifier); 

                        return 1f + modifier;
                    }
                }
            }
       }
    }
}

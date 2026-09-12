using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
namespace UWGame.SimSide.AI.Activities
{
    public abstract class Formation
    {
        //public List<Vector2> FormationPositions = new List<Vector2>();

        // Relative positions in the formation!
        public Dictionary<Entity, Vector2> FormationPositions = new Dictionary<Entity, Vector2>();
        public Dictionary<Entity, Vector2> FormationPositionDrifts = new Dictionary<Entity, Vector2>();

        //public abstract List<Vector2> FormationPositions { get; }
        public abstract int MaxMembers { get; }

        public abstract Vector2 ComputePosition();

        public virtual void LeaveFormation(Entity entity)
        {
            if (FormationPositions.ContainsKey(entity))
            {
                FormationPositions.Remove(entity);
            }
            if (FormationPositionDrifts.ContainsKey(entity))
            {
                FormationPositionDrifts.Remove(entity);
            }

        }

    }
}

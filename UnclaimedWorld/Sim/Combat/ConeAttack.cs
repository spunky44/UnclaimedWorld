using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Combat
{
    public class ConeAttack : AreaAttack
    {
        public float Length;

        public float WidthInDegrees;

        [XmlIgnore]
        private float lengthSquared;

        [XmlIgnore]
        private float angleDistanceFromCenterLineInRadians;


        public override void Initialize()
        {
            lengthSquared = Length * Length;
            angleDistanceFromCenterLineInRadians = MathHelper.ToRadians(WidthInDegrees) / 2f;
        }


        public override List<Entity> GetEntitiesInArea(Vector3 location, Vector2 direction)
        {

            List<Pair<Entity, Vector2>> results = null;
            The.AgentQuadTree.GetEntitiesInRange(location.ToVector2(), Length, e => PointIsWithinCone(location, direction, e.Location.Value), ref results);


            if (results != null)
            {
                return results.Select(t => t.First).ToList();
            }
            else return null;

        }

        private bool PointIsWithinCone(Vector3 coneLocation, Vector2 coneDirection, Vector3 point)
        {
            Vector2 targetDirection = point.ToVector2() - coneLocation.ToVector2();
            float distanceToTarget = targetDirection.Length();

            if (distanceToTarget > Length)
                return false;

            double angleFromConeCenterLine = Common.GetAngleBetweenVectors(coneDirection, targetDirection);

            return angleFromConeCenterLine < angleDistanceFromCenterLineInRadians;

        }
    }
}

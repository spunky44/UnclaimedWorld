using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Combat
{
    public class SplashAttack : AreaAttack
    {
        float DamageRadius;

        public enum DamageFalloffType { None, Linear, Quadratic }

        public DamageFalloffType DamageFalloff;

        public override List<Entities.Entity> GetEntitiesInArea(Microsoft.Xna.Framework.Vector3 location, Microsoft.Xna.Framework.Vector2 direction)
        {
            List<Pair<Entity, Vector2>> results = null;
            The.AgentQuadTree.GetEntitiesInRange(location.ToVector2(), DamageRadius, null, ref results);


            if (results != null)
            {
                return results.Select(t => t.First).ToList();
            }
            else return null;
        }

        public override void Initialize()
        {
        }
    }
}

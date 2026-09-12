using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Combat
{
    [XmlInclude(typeof(ConeAttack))]
    [XmlInclude(typeof(AreaAttack))]
    public abstract class AreaAttack
    {

        public bool DamageOtherAllegianceMembers = false;

        public abstract void Initialize();

        /// <summary>
        /// returns null or empty list if no entities in area.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public abstract List<Entity> GetEntitiesInArea(Vector3 location, Vector2 direction);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// a shorthand for a trigger event that only affects the player characters
    /// </summary>
    public class AreaCondition : ConditionValue
    {
        public Rectangle Area;

        [XmlIgnore]
        Collisions.CollideShape2D area;

        [XmlIgnore]
        List<Pair<Entity, Vector2>> membersInArea;

        

        public override void Initialize()
        {
            area = new Collisions.CollideShape2D(Area.Top, Area.Left, Area.Bottom, Area.Right);
            membersInArea = new List<Pair<Entity,Vector2>>();
        }

        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
            {
                return IsFulfilled(ref triggeringEntity);
            }
            else return false;
        }

        public bool IsFulfilled(ref Entity triggeringEntity)
        {          
             
             The.AgentQuadTree.GetObjectsIntersectingBounds(area, e => The.Sim.PlaySite.PlayerAllegiance.Members.Contains(e), ref membersInArea);

             if (membersInArea.Count > 0)
             {
                 triggeringEntity = membersInArea[0].First;

                 membersInArea.Clear();

                 return true;
             }

             return false;          

        }

       
    }
}

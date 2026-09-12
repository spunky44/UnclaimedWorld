using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.InGameEvents.Actions
{

    public class AttackEntityAction : EventActionType
    {
        /// <summary>
        /// optional "attacker"
        /// </summary>
        public TargetObject Attacker;
        
        
        public TargetObject TargetToAttack;
        
       // public string AttackTypeKey;
        public EvalNode AttackTypeKey = null;


        public Ownership? OwnershipType;
        public TargetObject OwnerOfCarcass;

         public AttackEntityAction(string keyName): base(keyName)
        {

        }

         public AttackEntityAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            Entity targetEntityToUse = null;
            Entity attacker = null;

            var result = TargetToAttack.GetResult(action);
            if (result.Count == 0)
            {
                failReason = "Target lookup did not give any results.";
                return false;
            }
            targetEntityToUse = (Entity)result[0];

            if (Attacker != null)
            {
                result = Attacker.GetResult(action);
                if (result.Count == 0)
                {
                    failReason = "Attacker lookup did not give any results.";
                    return false;
                }
                attacker = (Entity)result[0];
            }


            AttackType attackType;
            PropertyResult? attackTypeKey = AttackTypeKey.Evaluate(action);
            if (attackTypeKey.HasValue)
            {
                GameData.Instance.AllAttackTypes.TryGetValue(attackTypeKey.Value.StringResult, out attackType);
            }
            else
            {
                failReason = "Failed to find attack type key";
                return false;
            }

            IOwner ownerOfCarcass = null;
            // for traps, the owner is the owner of the trap. for agents, the owner is the agent...
            if (OwnershipType.HasValue && OwnerOfCarcass != null)
            {
                ownerOfCarcass = ClaimEntityAction.ResolveNewOwner(OwnershipType.Value, OwnerOfCarcass, action);
                if (ownerOfCarcass == null)
                {
                    failReason = "Owner lookup did not give any results.";
                    return false;
                }
            }

         
            if (targetEntityToUse != null && attackType != null)
            {
                Attack(attacker, targetEntityToUse, attackType, ownerOfCarcass);
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// delays are not implemented...
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="targetEntity"></param>
        /// <param name="attackType"></param>
        /// <param name="ownerOfCarcass"></param>
        private void Attack(Entity attacker, Entity targetEntity, AttackType attackType, IOwner ownerOfCarcass)
        {
            // see if it's dead
            bool isDead;
            bool isUnconscious;

            CauseOfDeath? causeOfDeath;
            CauseOfUnconsciousness? causeOfUnconsciousness;

            targetEntity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);
            
            if (isDead)
                return; // playing its death anim


            attackType.StartStartEffects(targetEntity, attacker);

            // find body part to damage          
            BodyPart.AttackDirection attackDirection;
            if (attacker != null)
            {
                attackDirection = GoalDoAttack.GetAttackDirection(attacker, targetEntity.Location.Value, targetEntity.Rotation);
            }
            else
            {
                attackDirection = BodyPart.AttackDirection.Front;
            }

            BodyPart targetBodyPart = targetEntity.Body.GetRandomBodyPartToHit(attackDirection);
            BodyPart hitBodyPart;

            bool willHitTarget = GoalDoAttack.RollToHit(attacker, targetEntity, attackType, targetBodyPart, attackDirection, out hitBodyPart);
            if (willHitTarget)
            {               
                OwnerID? ownerOfCarcassID = ownerOfCarcass != null ? ownerOfCarcass.ID : (OwnerID?)null;
                //  EntityID? attackerID = attacker != null ? attacker.ID : (EntityID?)null;

                attackType.HitTargets(attacker, null, ownerOfCarcassID, hitBodyPart.BodyPartID, targetEntity);               

            }

            attackType.StartActionPointEffects(targetEntity, attacker, willHitTarget);
        }

       

        public override string ToString()
        {
            return "Attack entity";
        }
    }
}

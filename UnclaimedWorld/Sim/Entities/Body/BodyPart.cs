using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body
{

    /// <summary>
    /// NOTE! Not for global lookup - only use it when looking up body parts in a MemoryFact or Entity Body!!!
    /// </summary>
    public enum BodyPartID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    public abstract class BodyPart : ISnapshot, IHasBodyParts, IHasExposedProperties
    {
     
        public string KeyName
        {
            get; private set;
        }

        public BodyPartType BodyPartType;
        string snapshotBodyPartName;


        /// <summary>
        /// the body can have either an Entity or MemoryFact parent.
        /// </summary>
        public Body Body;

        /// <summary>
        /// save these IDs to retrieve the Body reference post-load
        /// </summary>
        EntityID? snapshotParentEntity;
        MemoryFactID? snapshotParentMemoryFact;

        /// <summary>
        /// will be false when the part is missing!
        /// </summary>
        public bool Exists = true;

        public float Hitpoints;

        public float MaxHitpoints;

        /// <summary>
        /// NOT for global lookup - use only from Body.FindBodyPart
        /// </summary>
        public BodyPartID BodyPartID;

        /// <summary>
        /// why static??? confusing...
        /// </summary>
        static BodyPartID idCounter = BodyPartID.First;


        public List<BodyPart> BodyParts
        {
            get;
            set;           
        }

        

        public BodyPart(BodyPartType bodyPartType, Body body)
        {    
            BodyPartID = idCounter;
            idCounter++;

            this.BodyPartType = bodyPartType;
            this.Body = body;
           
            KeyName = Body.Parent.EntityType.KeyName + ":" + BodyPartType.Name;
        }

        /// <summary>
        /// copies a BodyPart to store in a MemoryFact
        /// </summary>
        /// <param name="original"></param>
        protected BodyPart(BodyPart original)
        {
            // MemoryFact will create a copy of the Entity's Body and all its BodyParts.
            // the evaluator stores an EntityID and a BodyPartID in each combo.
            // The evaluator will look up an IKnownEntityData object using an EntityID, then
            // it will use a BodyPartID to look up in the Body (which could be a copy) to find the specified body part.
            // therefore, the two IDs must be copies.
                       
            BodyPartID = original.BodyPartID; // copy

            this.BodyPartType = original.BodyPartType;
            Hitpoints = original.Hitpoints;
            Exists = original.Exists;
            MaxHitpoints = original.MaxHitpoints;
          
            KeyName = original.KeyName;
        }

        public BodyPart()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       
        }


        public static void ResetBodyPartCounter()
        {
            idCounter = BodyPartID.First;
        }

        public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, 
            SharedKnowledge getterKnowledge = null)
        {

        }

        public void GatherBodyParts(List<Body.BodyPartChance> bodyParts, AttackDirection attackDirection)
        {

            bodyParts.Add(new Body.BodyPartChance(){ BodyPart = this, Score = GetToHitProfile(attackDirection) });

            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.GatherBodyParts(bodyParts, attackDirection);
                }
            }
        }

        public virtual void ChangeOwnershipOnParts(IOwner newOwner)
        {            
            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.ChangeOwnershipOnParts(newOwner);

                }
            }

        }

        public float DoDamage(float damage)
        {

            if (The.Sim.TotalUnPausedGameTimeInSeconds > 23 &&
                this.Body.Parent.ID == (Entities.EntityID)19)
            {

            }

            float damageDone = GetDamageDone(damage, Hitpoints);

            Hitpoints -= damageDone;
            Hitpoints = Common.ClampBottom(Hitpoints, 0);

            Body.GlobalHitpoints -= damageDone;
            Body.GlobalHitpoints = Common.ClampBottom(Body.GlobalHitpoints, 0);
            if(IsFunctional() == false)
            {
                AffectParentOnLossOfBodyPart();
            }
            return damageDone;
        }

        public static float GetDamageDone(float damage, float Hitpoints)
        {
            float maxDamage = Hitpoints;
        /*    Hitpoints -= damage;
            Hitpoints = Common.ClampBottom(Hitpoints, 0);
            */
            float damageDone = Math.Min(damage, maxDamage);
            return damageDone;
        }

        private void AffectParentOnLossOfBodyPart()
        {
            if (BodyPartType.Functions != null)
            {
                foreach(BodyPartFunction bodyPartFunction in BodyPartType.Functions)
                {
                    switch (bodyPartFunction.Function)
                    {
                        case BodyPartFunction.FunctionType.Locomotion:
                        {
                            Body.Parent.ImpairMovement(bodyPartFunction.Weight);
                            break;
                        }
                    }
                }
            }
            if (BodyParts != null)
            {
                foreach(BodyPart bodyPart in BodyParts)
                {
                    bodyPart.AffectParentOnLossOfBodyPart();
                }
            }
        }

        public bool IsBodyPartDamageFatal()
        {
            if (Hitpoints <= 0 && IsVital())
                return true;

            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    if (bodyPart.IsBodyPartDamageFatal())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsBodyPartDamageCausingCollapse()
        {
            if (IsVital() && Hitpoints <= GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * MaxHitpoints)
                return true;

            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    if (bodyPart.IsBodyPartDamageCausingCollapse())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void InitializeHitpoints()
        {
            Hitpoints = BodyPartType.HitpointsFraction * Body.GlobalHitpoints;
            MaxHitpoints = Hitpoints;
                      
            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.InitializeHitpoints();
                }
            }
        }

        public void UpdateHitpointsWithNewBulk(float percentageIncrease) //float bulkIncrease)
        {
            if (Hitpoints > 0)
            {
                // make changes in relation to bulk change:
               // float percentageIncrease = (Parent.Bulk - oldBulk) / Parent.Bulk;

                Hitpoints = (1f + percentageIncrease) * Hitpoints;

                MaxHitpoints = BodyPartType.HitpointsFraction * Body.MaxHitpoints;
            }

            foreach (BodyPart bodyPart in BodyParts)
            {
                bodyPart.UpdateHitpointsWithNewBulk(percentageIncrease);
            }           
        }

        public enum AttackDirection { Front, Back, Left, Right }
        /// <summary>
        /// modifier based on sizes
        /// 0.5 - 3
        /// </summary>
        /// <returns></returns>
        public float GetToHitModifier(float attackerSize, AttackDirection direction)
        {
            float tohitProfile = 1f;
            tohitProfile = GetToHitProfile(direction);

           // float sizeRelation = Common.Clamp(Body.Parent.Size / attackerSize, 0.1f, 3f);
            float sizeRelation;
            float bulk;
            if (Body.Parent != null)
            {
                bulk = Body.Parent.Bulk;
            }
            else 
            {
                bulk = Body.ParentMemoryFact.Bulk;
            }

            sizeRelation = bulk / attackerSize;

            return Common.Clamp(sizeRelation * tohitProfile, 0.5f, 3f); 

        }

        private float GetToHitProfile(AttackDirection direction)
        {
            float tohitProfile = 0f;
            switch (direction)
            {
                case AttackDirection.Front:
                    tohitProfile = BodyPartType.ToHitProfileFront;
                    break;
                case AttackDirection.Back:
                    tohitProfile = BodyPartType.ToHitProfileBack;
                    break;
                case AttackDirection.Left:
                    tohitProfile = BodyPartType.ToHitProfileLeft;
                    break;
                case AttackDirection.Right:
                    tohitProfile = BodyPartType.ToHitProfileRight;
                    break;
            }
            return tohitProfile;
        }

        public bool IsVital()
        {
            return BodyPartType.IsVital();
        }

        public bool IsFunctional()
        {
            return Hitpoints > 0;
        }

        public override string ToString()
        {
            return BodyPartType.Name;
        }

        /// <summary>
        /// placeholder for proper healing/wounds system...
        /// </summary>
        /// <param name="totalRegainAmount"></param>
        public void Regain(float totalRegainAmount)        
        {
            if (IsFunctional()) // don't increase if not functional...
            {
                if (Hitpoints < MaxHitpoints)
                {
                    Hitpoints += this.BodyPartType.HitpointsFraction * totalRegainAmount;

                    Hitpoints = Common.ClampTop(Hitpoints, MaxHitpoints);
                }

                if (BodyParts != null)
                {
                    foreach (var item in BodyParts)
                    {
                        item.Regain(totalRegainAmount);
                    }
                }
            }
        }

        public BodyPart FindFirstMatchingBodyPart(Predicate<BodyPart> predicate)
        {
            if (predicate(this)) // this.BodyPartType == bodyPartType)
            {
                return this;
            }
            else
            {
                if (BodyParts != null)
                {
                    BodyPart foundBodyPart;
                    foreach (BodyPart bodyPart in BodyParts)
                    {
                        foundBodyPart = bodyPart.FindFirstMatchingBodyPart(predicate);
                        if (foundBodyPart != null)
                        {
                            return foundBodyPart;
                        }
                    }
                }
            }

            return null;
        }
        public void GetHurtVitalBodyPartFactor(ref float currentFactor)
        {
            if(IsVital())
            {
                float myFactor = 1.0f - GetMoraleDecreaseFactor();
                if (myFactor < currentFactor)
                {
                    currentFactor = myFactor;
                }
            }
            if (BodyParts != null)
            {
                foreach(BodyPart bodyPart in BodyParts )
                {
                    bodyPart.GetHurtVitalBodyPartFactor(ref currentFactor);
                }
            }
        }

        /// <summary>
        /// applies a scaling factor to the hitpoints if the bodypart is vital.
        /// </summary>
        /// <param name="totalHitpoints"></param>
        public void GetModifiedHitpointsForPresentation(ref float totalHitpoints, ref float totalMaxHitpoints)
        {
            totalMaxHitpoints += MaxHitpoints; // compute the max too.

            float hitpoints = Hitpoints; 
            
            if (IsVital() && hitpoints < MaxHitpoints)
            {
                float level = hitpoints / MaxHitpoints;

                float levelFactor = level * level; // square it..

                // make it seem like hitpoints trail off faster for vital bodyparts
                hitpoints = MaxHitpoints * levelFactor;               
            }

            totalHitpoints += hitpoints;

            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.GetModifiedHitpointsForPresentation(ref totalHitpoints, ref totalMaxHitpoints);
                }
            }
        }

        private float GetMoraleDecreaseFactor()
        { 
            float damageFromOneAttack = 1.0f / GameData.Instance.OneOverMeanDamageFromHumanPunch;
            return GetMoraleDecreaseFactor(damageFromOneAttack);
        }

        public float GetMoraleDecreaseFactor(float damageToAffectMorale)
        {
            if(Hitpoints == MaxHitpoints)
            {
                return 0.0f;//Our morale should not decrease if we have not been damaged by an attack
            }
            if (damageToAffectMorale == 0.0f)
            {
                return 0.0f;
            }
            float currentSustainableNumberOfAttacks = (int)(GetHitpointsUntilUnconsciousness() / damageToAffectMorale);
            if (currentSustainableNumberOfAttacks < 0)
            {
                currentSustainableNumberOfAttacks = 0;
            }
            float maxNumberOfSustainableAttacks = (int)(GetMaxHitpointsUntilUnconsciousness() / damageToAffectMorale);
            if (maxNumberOfSustainableAttacks <= 0.0f)
            {
                return 1.0f;//This body part cannot sustain any attacks at all, decrease morale as much as possible
            }
            float partOfMaxSustainableAttacks =  (currentSustainableNumberOfAttacks/maxNumberOfSustainableAttacks); //1 - 0
            return 1.0f - partOfMaxSustainableAttacks;
        }

        float GetMaxHitpointsUntilUnconsciousness()
        {
            return MaxHitpoints * (1 - GameData.Instance.Constants.FractionOfHitpointsCausingCollapse);
        }

        float GetHitpointsUntilUnconsciousness()
        { 
            float differenceToMaxHitpoints = MaxHitpoints - Hitpoints;
            float hitpointsLeftUntilUnconsciousness = GetMaxHitpointsUntilUnconsciousness() - differenceToMaxHitpoints;
            return hitpointsLeftUntilUnconsciousness;
        }

        static BodyPart()
        {
            exposedPropertyValueFunctions.Add("Injuries", GetHealthFraction);
            exposedPropertyValueFunctions.Add("MovementImpairments", GetMovementValue);
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string,GetPropertyValue>();

        #region IHasExposedProperties

        //All IHasExposedProperties are casted to be able to call the class specific function for a certain property, 
        //this is safe because GetPropertyValue uses the global functions defined for the class in which it is used
        // However if we by mistake added a global function from another class to our dictionary then it might cause problems, but that should never happen
        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            PropertyResult? result = null;
            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else
            {
                PropertyResult customResult;
                if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
                {
                    result = customResult;
                }
            }

            return result;
        }

        public string GetDefaultCaption(string propertyKey)
        {
            return BodyPartType.Name;
        }

        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }

        public void GetList<Type>(ref List<Type> listToFillWithProperties) where Type : IHasExposedProperties
        {
            if (listToFillWithProperties == null)
                listToFillWithProperties = new List<Type>();
            IHasExposedProperties item = this;
            Type convertedItem = (Type)item;
            listToFillWithProperties.Add(convertedItem);
            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.GetList<Type>(ref listToFillWithProperties);
                }
            }
        }

        public EntityID? GetEntityID()
        {
            return null;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            if (this.Body.Parent != null)
            {
                return true;
            }
            else return false;
        }

        public string GetCaption(string captionKey)
        {
            return null;
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);           

        }

        private Dictionary<string, PropertyResult> customFields;

        #endregion

        #region Exposed properties

        public static PropertyResult? GetHealthFraction(IHasExposedProperties bodyPartToGetFractionFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((BodyPart)bodyPartToGetFractionFrom).GetHealthFraction();
        }

        public PropertyResult GetHealthFraction()
        {
            PropertyResult healthResult = new PropertyResult();
            healthResult.NumberResult = Hitpoints / MaxHitpoints;
            return healthResult;
        }

        public static PropertyResult? GetMovementValue(IHasExposedProperties bodyPartToGetFractionFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((BodyPart)bodyPartToGetFractionFrom).GetMovementValue();
        }
        public PropertyResult? GetMovementValue()
        {
            if (BodyPartType.Functions == null)
            {
                return null;
            }


            foreach(BodyPartFunction bodyPartFunction in BodyPartType.Functions)
            {
                if(bodyPartFunction.Function == BodyPartFunction.FunctionType.Locomotion)
                {
                    PropertyResult movementResult = new PropertyResult();
                    if (IsFunctional() == true)
                    {
                        movementResult.NumberResult = 1.0f;
                    }
                    else
                    {
                        movementResult.NumberResult = 1.0f - bodyPartFunction.Weight;
                    }
                    return movementResult;
                }
            }
            return null;
        }

        #endregion



        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.BodyPartID = sn.DoEnum(BodyPartID);
            idCounter = sn.DoEnum(idCounter);

            this.BodyParts = sn.DoList(BodyParts);            
            this.snapshotBodyPartName = sn.DoString(BodyPartType != null? BodyPartType.Name : null);
            this.KeyName = sn.DoString(KeyName);
            this.MaxHitpoints = sn.DoFloat(this.MaxHitpoints);
            this.Hitpoints = sn.DoFloat(Hitpoints);
            this.Exists = sn.DoBool(Exists);
            this.customFields = sn.DoDictionary(customFields);


          /* */

            if (sn.mode != Snapshotter.Mode.Load)
            {
                // handle the Body parent object
                 snapshotParentEntity = null;
                if (Body.Parent != null)
                {
                    this.snapshotParentEntity = Body.Parent.EntityID;
                }

                snapshotParentMemoryFact = null;
                if (Body.ParentMemoryFact != null)
                {
                    this.snapshotParentMemoryFact = Body.ParentMemoryFact.ID;
                }
            }

            this.snapshotParentEntity = sn.DoEntityIDNullable(snapshotParentEntity);                        
            this.snapshotParentMemoryFact = sn.DoEnumNullable(snapshotParentMemoryFact);

            sn.Ignore(exposedPropertyValueFunctions);
            sn.Ignore(Body);
            sn.Ignore(BodyPartType);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (snapshotParentEntity.HasValue)
                Body = Entity.FindByID(snapshotParentEntity.Value).Body;

            snapshotParentEntity = null; // cleared for next save

            if (snapshotParentMemoryFact.HasValue)
                Body = LookUp<MemoryFact, MemoryFactID>.FindByID(snapshotParentMemoryFact.Value).Body;

            snapshotParentMemoryFact = null; // cleared for next save

            EntityType entityType;
            if (Body.Parent != null)
            {
                entityType = Body.Parent.EntityType;
            }
            else 
            {
                entityType = Body.ParentMemoryFact.EntityType;
            }

            this.BodyPartType = entityType.BodyType.FindBodyPart(snapshotBodyPartName);


            if (BodyParts != null)
            {
                foreach (var item in BodyParts)
                {
                    item.LoadPostProcess(sn);
                }
            }
        }

        #endregion
    }
}

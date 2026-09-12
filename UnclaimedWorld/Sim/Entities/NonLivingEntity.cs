using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// non-living things: structures, items and vehicles all have condition
    /// </summary>
    public class NonLivingEntity: Component
    {

        private float? integrity = null;

        /// <summary>
        /// this is the condition that exists outside the parts - that holds the parts together... 
        /// I added this because improvised structures made of branches and stones should degrade quicker than the raw materials do.    
        /// 
        /// null for leaf items.
        /// is filled for 'middle' parts also, but we won't use the value except for the root
        /// </summary>
        public float? Integrity
        {
            get
            {
                return integrity;
            }

            private set
            {
                integrity = value;

                compositeConditionIsDirty = true;
            }
        }

        private float condition = 1f;

        /// <summary>
        /// only for IsCompositeRoot
        /// </summary>
        private bool compositeConditionIsDirty = true;

        /// <summary>
        /// 0 - 1
        /// Watch out: Only modify condition on leaf parts!
        /// the condition is only modified on the leaf parts. all the upper levels have a computed (average of their immediate children) value.
        /// 
        /// TODO: replace with a series of Substances (water, cracks, stress, rot...) each substance can trigger conversion at a certain level
        /// degrade should be a number of processes that can occur simultaneously (drying, contorting, decomposition, rotting...)
        /// </summary>        
        public float Condition
        {
            get
            {
                // composites may need to recompute:
                if (compositeConditionIsDirty)
                {
                   /* if (Parent.IsCompositeRoot) 
                    {*/
                        // set their condition to average of their parts...                      
                        ComputeConditionOfComposite();
                       
                   // }

                    compositeConditionIsDirty = false;

                    if (Parent.IsCompositeRoot)
                    {
                        Parent.UpdateFunctionality();
                    }
                }

                return condition;
            }

            set
            {
                if (condition != value)
                {
                    condition = value;

                    if (Parent.IsLeaf && !Parent.IsRoot)
                    {
                        // if this is a leaf, make sure the root knows that it needs to recalculate its condition:
                        Parent.GetRoot().SetConditionDirty();
                    }
                }
            }
        }

        public float? ConditionChangeSpeed;

        /// <summary>
        /// !!! 
        /// </summary>
        public float MaxCondition = 1f;

        float progress = 1f;


        /// <summary>
        /// for produced items/entities...
        /// progress = 0: the entity does not exist in the physical world
        /// progress > 0: the entity exists, but cannot be used
        /// progress = 1: the entity is fully functional
        /// </summary>
        public float Progress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    bool wasCompleted = IsCompleted(progress);

                    progress = value;

                    bool isCompleted = IsCompleted(value);

                    UpdateRenderable();
                                       

                    // use ComeOnline() instead to do actions when completed.
                  /*  if (!wasCompleted && isCompleted)
                    {
                        // notify the parent of our new status:
                        Parent.IsNowCompleted();
                    }*/

                }
            }
        }
            


        public NonLivingEntity(Entity parent)
            : base(parent, GameData.Instance.Constants.UpdateIntervalForNonLivingTypes)
        {

            if (parent.EntityType.Parts != null && parent.EntityType.Parts.Count > 0)
            {
                Integrity = 1f;
            }

           // CreateRegulators();
        }

        public NonLivingEntity()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public void UpdateRenderable()
        {
            if (progress > 0f && Parent.Renderable != null)
            {
                Parent.Renderable.SetNormalRendering();
            }
        }


        protected override void UpdateRegulated(double? timeSinceLastUpdate)
        {          
            if (Parent.IsStarted() == true
                && (Parent.IsLeaf // only degrade the leaf parts...
                || Parent.IsCompositeRoot)) // degrade the root also (integrity)
            {

                double daysPassed = timeSinceLastUpdate.Value * The.Sim.DateAndTime.DaysPerSecond;

                // do degrade damage corresponding to environment, and have a chance for total brokedown
                Degrade(daysPassed);
            }
        }

      
        public override double? GetUpdateInterval()
        {
            if (Parent.IsOnPlaySite()
                && !Parent.IsDead
                /*&& IsCompleted()*/) // allow shred items etc. to degrade and be destroyed.
            {
                return GameData.Instance.Constants.UpdateIntervalForNonLivingTypes;
            }


            return null;
        }

      
        /// <summary>
        /// parts should call this
        /// </summary>
        public void SetConditionDirty()
        {
            compositeConditionIsDirty = true;
        }


        /// <summary>
        /// do environmental damage to the entity
        /// </summary>
        /// <param name="daysElapsed"></param>
        public void Degrade(double daysElapsed)
        {
            try   // crash here: http://steamcommunity.com/app/284100/discussions/0/405692758726557395/
            {

                Storage storedIn = null;

                if (!Parent.StoredIn(out storedIn))
                {
                    return;
                }


                float damage;

                Entity storedInEntity = null;
                if (storedIn != null)
                {
                    storedInEntity = Entity.FindByID(storedIn.Parent);
                }

             

                if (Parent.EntityType.NonLivingType.FinalDegradeType != null)
                {
                    if (storedInEntity != null && Entity.IsFunctional(storedInEntity))
                    {
                        damage = ComputeDegradeDamage(Parent, storedIn.StorageConditions, storedIn.IsPowered, Parent.PlaySiteMapPosition, daysElapsed);
                    }
                    else
                    {
                        damage = ComputeDegradeDamage(Parent, null, null, Parent.PlaySiteMapPosition, daysElapsed);
                    }


                    if (Parent.IsLeaf)
                    {
                        // for leaf parts
                        DoConditionDamage(damage, daysElapsed); //, true);
                    }
                    else if (Parent.IsCompositeRoot)
                    {
                        // for root part - only if it actually has parts.
                        DoIntegrityDamage(damage); //, true);

                        ComputeConditionOfComposite(); // this is an attempt to disable Effects immediately if the gear is broken rather than waiting for Condition to be polled...
                        Parent.UpdateFunctionality();
                    }
                }

            }
            catch (Exception e)
            {
                string exceptionString = e.Message;
                if (Parent == null)
                {
                    exceptionString += "PARENT NULL\n";
                }
                else
                {
                    exceptionString += Entity.GetExceptionInformation(Parent);
                }

                throw new Exception(exceptionString);
            }
        }

        public static float ComputeDegradeDamage(IKnownEntityData item, StorageCondition storedIn, bool? storageIsPowered, Point mapPosition, double daysElapsed)
        {
            TerrainTile tile = The.Map.GetTile(mapPosition);
            float temperature = tile.Temperature;
            return ComputeDegradeDamage(item.EntityType.NonLivingType.FinalDegradeType, storedIn, item.IsWeatherProof(),/* item.IsEnclosed(),*/ 
                temperature, The.Sim.PlaySite.PlaySite.Weather.SunIntensity, 
                storageIsPowered, daysElapsed);
        }

        public static float ComputeDegradeDamage(DegradeType degradeType, StorageCondition storedIn, bool isWeatherProof, 
            float temperature, float lightLevel, bool? storageIsPowered, double daysElapsed)
        {
            //float temperature = 0f, 
            float light = 0f, moisture = 0f;

            if (degradeType == null)
            {
                // some upgrades didn't have this
                return 0f;
            }

            // crash here? http://steamcommunity.com/app/284100/discussions/2/341537388318335309/
            try
            {

                if (storedIn != null) // carried / stored / onboard
                {
                    temperature = storedIn.GetTemperature(storageIsPowered.Value, temperature);

                    moisture = storedIn.FixedMoisture ?? 0f; // Storage.GetMoisture(storedIn.Value); // 0f; // completely dry

                    light = storedIn.FixedLightLevel ?? lightLevel; // The.Sim.PlaySite.PlaySite.Weather.SunIntensity;

                }
                else // exposed to the elements. 
                {
                    GetEnvironment(isWeatherProof, ref temperature, ref light, ref moisture);
                }


                float temperatureDamage = Common.GetInterpolatedFunctionValue(temperature, degradeType.TemperatureDamage);
                float lightDamage = Common.GetInterpolatedFunctionValue(light, degradeType.LightDamage);
                float moistureDamage = Common.GetInterpolatedFunctionValue(moisture, degradeType.MoistureDamage);

                float damage = (float)(daysElapsed * (temperatureDamage + lightDamage + moistureDamage));
                return damage;
            }
            catch(Exception e)
            {
                throw new Exception("DegradeType: " + degradeType.KeyName, e);
            }
        }

        private static void GetEnvironment(bool isWeatherProof, ref float temperature, ref float light, ref float moisture)
        {        
            if (isWeatherProof) 
            { 
                // the item is part of a composite that is weather proof, or it is weather proof by itself:

                // NEW: use "isolated" temperature:
                temperature = StorageCondition.ComputeIsolatedTemperature(temperature);


                // reduce moisture impact due to weather proof coating!
                moisture = 0f; // disabled, for lack of UI feedback. This meant that port structures degraded more quickly. was: 0.2f * tile.Moisture;

                // reduce light impact too:
                light = 0.2f * The.Sim.PlaySite.PlaySite.Weather.SunIntensity;

            }
            else
            {
               
                moisture = 0f; // disabled, for lack of UI feedback. This meant that port structures degraded more quickly. was:  tile.Moisture;

                light = The.Sim.PlaySite.PlaySite.Weather.SunIntensity;
            }
        }

        public bool DoIntegrityDamage(float damage) //, bool rollForChanceToDestroy)
        {
            if (Integrity == null)
                Integrity = 1f;  // may be necessary if the item has been removed/added as part?

            if (Common.IsZero(Parent.EntityType.NonLivingType.IntegrityWeightInCondition))
            {
                return false; // no integrity damage gets applied.
            }

            Integrity -= damage;

            Integrity = Common.ClampBottom(integrity.Value, 0f);

            if (Integrity <= 0f)
            {               
                return true;
            }
           /* else if (rollForChanceToDestroy)
            {
                double chanceOfBreakdown = Math.Pow(1.0 - Integrity.Value, GameData.Instance.Constants.PowerCoefficientForRandomChanceOfBreakdown); // to increase the Fun, have a chance of breakdown before we reach zero:

                if (The.Sim.GameplayRandomGenerator.NextDouble("NonLivingEntity") < chanceOfBreakdown)
                {                   
                    return true;
                }
            }*/

            return false;
        }

        public void Repair(RepairAction repairAction, float progress)
        {
            switch (repairAction)
            {
                case RepairAction.Integrity:
                    if (Parent.IsCompositeRoot)
                    {
                        //Integrity += amount;
                        Integrity = progress;

                        Integrity = Common.ClampTop(integrity.Value, 1f);
                    }

                    break;

                case RepairAction.Condition:
                case RepairAction.PartsCondition:
                    if (Parent.IsLeaf)
                    {
                        Condition = progress; // will notify root
                        /* float newCondition = condition + amount;
                         newCondition = Common.ClampTop(newCondition, 1f);
                         Condition = newCondition; // will notify root*/
                    }
                    break;
            }

        }
      

        public bool DoConditionDamage(float damage, double? daysElapsed) //, bool rollForChanceToDestroy)
        {           
            Condition -= damage;

           /* if (Parent.IsLeaf && !Parent.IsRoot)
            {
                // if this is a leaf, make sure the root knows that it needs to recalculate its condition:
                Parent.GetRoot().SetConditionDirty();
            }*/

            if (daysElapsed.HasValue && !Common.IsZero(daysElapsed.Value))
            {
                ConditionChangeSpeed = (float)(damage / daysElapsed);
            }

            MaxCondition -= (0.3f * Parent.EntityType.NonLivingType.Repairability) * damage;
            
            if (Common.IsLessThanOrEqual(Condition, 0f)) 
            {
                DestroyOrTurnToJunk();

                return true;
            }
           /* else if (rollForChanceToDestroy)
            {
               
                double chanceOfBreakdown = Math.Pow(1.0 - Condition, GameData.Instance.Constants.PowerCoefficientForRandomChanceOfBreakdown); // to increase the Fun, have a chance of breakdown before we reach zero:

                if (The.Sim.GameplayRandomGenerator.NextDouble("NonLivingEntity") < chanceOfBreakdown)
                {
                    DestroyOrTurnToJunk();

                    return true;
                }

            }*/

            return false;
        }

        public float GetRepairProgress(RepairAction repairAction) //, EntityID? partToFix)
        {
            switch (repairAction)
            {
                case RepairAction.Integrity:
                    return Integrity.Value;                  
                case RepairAction.Condition:
                    return Condition; 
                case RepairAction.PartsCondition:
                    return Condition;

                // replace progress is tracked in SimProcess
            }

            return 1f;

        }

        public static double GetDaysLeftUntilBreakdown(double condition, float conditionChangeSpeed)
        {
            return condition / conditionChangeSpeed;                    
        }

        private void DestroyOrTurnToJunk()
        {
            IOwner oldOwner;
            LookUpOwners.ResolveEntityOwner(Parent, out oldOwner);

            // NEW: gather stats also (only for seen items):
            if (oldOwner != null)
            {
                IKnownEntityData entityData;
                if (oldOwner.Allegiance.SharedKnowledge.GetKnownData(Parent.ID, out entityData) == EntityResult.SeenDirectly)
                {
                    oldOwner.Allegiance.Statistics.AddProductionEvent(Parent.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.Degraded, 1);
                }
            }         
                       


            if (Parent.EntityType.NonLivingType.DegradesTo != null)
            {
                TurnIntoJunk(oldOwner);
            }
            else
            {
                Parent.Destroy();
            }
        }


        private void TurnIntoJunk(IOwner oldOwner)
        {

            Entity degradedInto = new Entity(Parent.EntityType.NonLivingType.DegradesToType);
            degradedInto.Initialize(The.Sim.PlaySite);
            degradedInto.InitializeModelAndOnScreenFunctionality();

            degradedInto.ComeOnline();

            if (degradedInto.EntityType.ItemType.HasNoMaximumBulk)
            {
                degradedInto.Bulk = Parent.Bulk;
            }
           
            // degradedInto.CarriedBy = CarriedBy;
            //  degradedIntoItemComponent.EquippedBy = EquippedBy;
            //  degradedIntoItemComponent.OKToTakeThisItemFromCarrier = OKToTakeThisItemFromCarrier;

            // we don't assign the new entity to any jobs etc. Any jobs would not have a reference to it and be able to clear it.
            // the preconditions on the goal should trigger a fail when a needed item turns to junk.
          

            if (Parent.PartOf == null)
            {
                // only switch container entities if the item is not a part
                Container container;
                if (Parent.GetContainedBy(out container))
                {
                    Entity carriedBy;
                    Parent.CarriedByAgent(out carriedBy);
                   
                    if (container != null)
                    {
                        // NEW: eject any contained items before switching:
                        if (Parent.Contains != null)
                        {                            
                             Parent.Contains.IterateContained(e => Parent.Contains.Uncontain(e)); // eject all
                        }

                        container.SwitchEntities(Parent, degradedInto);
                    }

                    if (carriedBy != null)
                    {
                       // carriedBy.Intelligence.Allegiance.SharedKnowledge.SeeDetectable(degradedInto, true, false, null, carriedBy); 
                        carriedBy.Intelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(degradedInto, true, false, null, carriedBy); 
                    }
                    
                }
            }
           
            // switch the parts:
            degradedInto.PartOf = Parent.PartOf;
            if (degradedInto.PartOf != null)
            {
                degradedInto.PartOf.SetPart(degradedInto);
                //degradedInto.PartOf.Parts.Add(degradedInto);
            }

            if (degradedInto.ContainedBy == null && degradedInto.PartOf == null)
            {
                degradedInto.SetPosition(Parent.PlaySiteLocation);
            }

          /*  IOwner oldOwner;            
            LookUpOwners.ResolveEntityOwner(Parent, out oldOwner);*/

            degradedInto.ChangeOwnership(oldOwner);

            // remove the old...
            this.Parent.Destroy();
        }

       

        public void ComputeConditionOfComposite()
        {
            if (Parent.IsCompositeRoot)
            {
                float integrityWeight = Parent.EntityType.NonLivingType.IntegrityWeightInCondition;

                ComputeConditionOfPart();

                if (Parent.IsCompositeRoot && integrity.HasValue)
                {
                    // if integrity is 0, condition should be 0 as well. This is a parallel to a part where Condition is 0 => broken part.
                    if (Common.IsLessThanOrEqual(integrity.Value, 0d))
                    {
                        condition = 0f;
                    }
                    else
                    {
                        condition = integrityWeight * integrity.Value + (1f - integrityWeight) * condition;
                    }
                }

               // Parent.UpdateFunctionality();
            }

            compositeConditionIsDirty = false;
        }

        public float ComputeConditionOfPart()
        {
            if (Parent.Parts != null)
            {
                float conditionSum = 0f;
                foreach (Entity part in Parent.Parts)
                {
                    conditionSum += part.NonLivingEntity.ComputeConditionOfPart();
                }

                condition = conditionSum / Parent.Parts.Count;
                return condition;
            }
            else return condition;
        }


        

        public bool IsCompleted()
        {
            return Common.IsGreaterThanOrEqual(Progress, 1f);
        }

        public static bool IsCompleted(float? progress)
        {
            return progress == null || Common.IsGreaterThanOrEqual(progress.Value, 1f);
        }

        // test IsStarted flag instead. there can be a delay before progress is made...
       /* public static bool IsStarted(float? progress)
        {
            return progress.HasValue && Common.IsGreaterThan(progress.Value, 0f);
        }*/


        public bool IsStarted()
        {
            return Common.IsGreaterThan(Progress, 0f);
        }


        #region Parts & repair

        List<EntityID> snapshotParts;
        public List<Entity> Parts
        {
            get;
            set;
        }

        public EntityID? ParentEntityID
        {
            get
            {
                if (partOf != null)
                {
                    Entity parentAsEntity = partOf as Entity;
                    if (parentAsEntity != null)
                    {
                        return parentAsEntity.ID;
                    }
                }

                return null;
            }
        }

        public CompositeID? PartOfID
        {
            get
            {
                if (partOf != null)
                    return partOf.ID;

                return null;
            }
        }




        public void CreateParts()
        {
            if (Parent.EntityType.Parts != null && Parent.EntityType.Parts.Count > 0)
            {
                if (Parts == null) // there are too many calls to this method...
                {
                    Parts = new List<Entity>();

                    foreach (KeyValuePair<EntityType, int> kvp in Parent.EntityType.Parts)
                    {
                        for (int i = 0; i < kvp.Value; i++)
                        {
                            CreatePart(kvp.Key);
                        }
                    }
                }
            }
        }

        private void CreatePart(EntityType typeOfPart) // ref KeyValuePair<EntityType, int> kvp)
        {
            Entity part;
            part = new Entity(typeOfPart);

            // NEW: will be done from Parent functions instead
            /*part.Initialize(Site); 
            part.InitializeModelAndOnScreenFunctionality();
            */

            SetPart(part);


            if (part.EntityType.Parts != null)
            {
                part.CreateParts();
            }
        }

        private CompositeID? partOfID; // only used during snapshot
        private IComposite partOf;
        public IComposite PartOf
        {
            get
            {
                return partOf;
            }
            set
            {
                if (value != partOf)
                {

                    partOf = value;

                    if (Parent.Renderable != null)
                    {
                        Parent.Renderable.UpdateIsDrawnStatus();
                    }

                    if (partOf != null)
                    {
                        // NEW: reset these:
                        Parent.SetLocationPropertiesForLeaf(); // this will set location and mapposition, removing from map


                        Parent.ClearContainedBy(); // part of and contained by cannot both be set.                       
                    }                   
                }
            }
        }

        /// <summary>
        /// assigns the parts and/or creates new ones if they were not supplied
        /// </summary>
        /// <param name="suppliedParts"></param>
        public void SetPartsOrCreateNew(List<Entity> suppliedParts)
        {
            if (Parent.EntityType.Parts != null)
            {
                // NEW: we don't create parts in the ctor.
                Parts = new List<Entity>();

                Entity suppliedPart;

                foreach (var part in Parent.EntityType.Parts)
                {
                    for (int i = 0; i < part.Value; i++)
                    {
                        if (suppliedParts != null)
                        {
                            suppliedPart = suppliedParts.Find(p => p.EntityType == part.Key);
                        }
                        else
                        {
                            suppliedPart = null;
                        }

                        if (suppliedPart != null)
                        {
                            SetPart(suppliedPart);

                            suppliedParts.Remove(suppliedPart);
                        }
                        else
                        {
                            // else create a new part:
                            CreatePart(part.Key);
                        }
                    }

                }

                //  Parts.AddRange(parts);

            }
        }

        public void SetPart(Entity newPart)
        {           
            Parts.Add(newPart);
            newPart.PartOf = Parent;
            newPart.Site = Parent.Site; // new

            if (Parent.EntityType.IntelligenceType != null)
            {
                // recursively scan the parts to get references to intrinsic tools or weapons
                if (Parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
                {
                    AddIntrinsicTool(newPart);
                }

                if (Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
                {
                    AddIntrinsicWeapon(newPart);
                }
            }
        }

        public void RemovePart(Entity part, bool setPartOfToNull = true)
        {          
            if (part.PartOf == Parent)
            {
                Parts.Remove(part);

                if (setPartOfToNull)
                {
                    part.PartOf = null;
                }

                if (Parent.EntityType.IntelligenceType != null)
                {
                    // recursively scan the parts to get references to intrinsic tools or weapons
                    if (Parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
                    {
                        RemoveIntrinsicTool(part);
                    }

                    if (Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
                    {
                        RemoveIntrinsicWeapon(part);
                    }
                }
            }
        }

        private void AddIntrinsicTool(Entity newPart)
        {
            if (MatchesIntrinsicToolType(newPart))
            {
                EntityID existingTool;
                if (!Parent.Intelligence.IntrinsicTools.TryGetValue(newPart.EntityType, out existingTool))
                {
                    Parent.Intelligence.IntrinsicTools.Add(newPart.EntityType, newPart.ID);

                    return;
                }
            }

            if (newPart.Parts != null)
            {
                // recursion - scan the parts:
                foreach (var item in newPart.Parts)
                {
                    AddIntrinsicTool(item);
                }
            }
        }

        private void AddIntrinsicWeapon(Entity newPart)
        {
            if (MatchesIntrinsicWeaponType(newPart))
            {
                EntityID existingTool;
                if (!Parent.Intelligence.IntrinsicWeapons.TryGetValue(newPart.EntityType, out existingTool))
                {
                    Parent.Intelligence.IntrinsicWeapons.Add(newPart.EntityType, newPart.ID);

                    return;
                }
            }

            if (newPart.Parts != null)
            {
                // recursion - scan the parts:
                foreach (var item in newPart.Parts)
                {
                    AddIntrinsicWeapon(item);
                }
            }
        }

        private void RemoveIntrinsicTool(Entity part)
        {
            if (MatchesIntrinsicToolType(part))
            {
                // remove intrinsic tool if needed:
                EntityID existingTool;
                if (Parent.Intelligence.IntrinsicTools.TryGetValue(part.EntityType, out existingTool)
                    && existingTool == part.ID)
                {
                    Parent.Intelligence.IntrinsicTools.Remove(part.EntityType);
                }
            }

            if (part.Parts != null)
            {
                // recursion - scan the parts:
                foreach (var item in part.Parts)
                {
                    RemoveIntrinsicTool(item);
                }
            }
        }

        private void RemoveIntrinsicWeapon(Entity part)
        {
            if (MatchesIntrinsicWeaponType(part))
            {
                // remove intrinsic weapon if needed:
                EntityID existingTool;
                if (Parent.Intelligence.IntrinsicWeapons.TryGetValue(part.EntityType, out existingTool)
                    && existingTool == part.ID)
                {
                    Parent.Intelligence.IntrinsicWeapons.Remove(part.EntityType);
                }
            }

            if (part.Parts != null)
            {
                // recursion - scan the parts:
                foreach (var item in part.Parts)
                {
                    RemoveIntrinsicWeapon(item);
                }
            }
        }
        // public event EventHandler PartIsBrokenEvent;



        private bool MatchesIntrinsicWeaponType(Entity newPart)
        {
            if (newPart.EntityType.ItemType != null
               && newPart.EntityType.ItemType.WeaponType != null
               && newPart.EntityType.ItemType.WeaponType.IsIntrinsic == true
               && Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes.Contains(newPart.EntityType))
            {
                return true;
            }

            return false;
        }

        private bool MatchesIntrinsicToolType(Entity newPart)
        {
            if (newPart.EntityType.ToolType != null
               && newPart.EntityType.ToolType.ToolHandling == ToolHandlingType.Intrinsic // .IsIntrinsic == true
               && Parent.EntityType.IntelligenceType.IntrinsicToolTypes.Contains(newPart.EntityType))
            {
                return true;
            }

            return false;
        }



       // const float conditionLimit = 0.2f; // perhaps use a higher limit on buildings since comfort is affected

        public bool NeedsRepair() //SharedKnowledge sharedKnowledge) //, IKnownEntityData entityData, out RepairAction? action, out EntityID? partToFix)
        {            

            // gather data for multiple repair jobs...
            if (Parent.IsRoot)
            {
                float conditionLimit;
                conditionLimit = GetConditionLimit();

               // NonLivingEntity nonLiving = this.NonLivingEntity;

                // only check the root for integrity
                if (NeedsIntegrityRepair(conditionLimit))
                {
                    return true;
                }

                List<EntityOrType> partsNeedingRepair = null;

                GatherPartsWithProblems(ref partsNeedingRepair, conditionLimit);

                if (partsNeedingRepair != null)
                {
                    return true;
                }

            }

            return false;
        }

        private float GetConditionLimit()
        {
            float conditionLimit;
            if (Parent.EntityType.ContainerType != null && Parent.EntityType.ContainerType.ResidenceType != null)
            {
                conditionLimit = GameData.Instance.AIConstants.ResidenceConditionToStartRepair;
            }
            else
            {
                conditionLimit = GameData.Instance.AIConstants.OtherStructureConditionToStartRepair;
            }
            return conditionLimit;
        }


        private bool NeedsIntegrityRepair(float conditionLimit)
        {
            if (Integrity < conditionLimit)
            {
                return true;
            }

            return false;
        }

        public struct EntityOrType
        {
            public EntityType EntityType;
            public Entity Entity; // can be null
            public int Level; // 0 = root
            public bool IsJunkPart;

            public EntityOrType(Entity part, EntityType entityType, int level, bool isJunkPart = false)
            {
                this.Entity = part;
                this.EntityType = entityType;
                this.IsJunkPart = isJunkPart;
                this.Level = level;
            }
        }

        public RepairPackage ComputeBestRepairPackage() //SharedKnowledge sharedKnowledge, out RepairAction? action, out EntityID? partToFix)
        {
            // only check the leaves for condition.
            /*
             if a leaf needs repair, there are 3 options:
             * 1. recondition it
             * 2. replace the part
             * 3. replace a parent part
             * 
             * if no replacements are available, we can also remove the part. After reconditioning, it can be placed back
             * 
             * if there is a junk part:
             * remove the part
             * 
             * if a leaf is missing:
             * 1. replace the part
             * 2. replace a parent part
             * 
             * if a composite part is missing:
             * 1. replace the part
             * 2. replace a parent part
             * 
             * if multiple parts have problems, it may be easier to replace the parent, even if the parts can be repaired in place
             * 
             */

            float conditionLimit;
            conditionLimit = GetConditionLimit();

           
            List<EntityOrType> partsNeedingRepair = null;
            GatherPartsWithProblems(ref partsNeedingRepair, conditionLimit);

            List<RepairPackage> packages = CreateRepairOptions(partsNeedingRepair, conditionLimit);
            
            return SelectBestRepairPackage(packages);

        }

        /// <summary>
        /// score available items, tools, skills and so on?
        /// </summary>
        /// <returns></returns>
        private RepairPackage SelectBestRepairPackage(List<RepairPackage> packages)
        {
            // TODO: select the procedure with the lowest time cost! Prefer replacement. The parts can later be repaired at a workshop
            if (packages.Count > 0)
            {
                return packages[packages.Count - 1];
            }

            return null;
        }

        private List<RepairPackage> CreateRepairOptions(List<EntityOrType> partsNeedingRepair, float conditionLimit)
        {
            /*
                if a leaf needs repair, there are 3 options:
                * 1. recondition it
                * 2. replace the part
                * 3. replace a parent part
                * 
                * if no replacements are available, we can also remove the part. After reconditioning, it can be placed back
                * 
                * if there is a junk part:
                * 1. remove the part 
                * 2. remove a parent part
                * 
                * if a leaf is missing:
                * 1. replace the part
                * 2. replace a parent part
                * 
                * if multiple parts have problems, it may be easier to replace the parent, even if the parts can be repaired in place
                * 
                */

            // create repair packages, each package fixes all problems if possible
            // a repair package contains one or more repair actions

            List<RepairPackage> repairPackages = new List<RepairPackage>();

            // add integrity repair action if needed:

            CreateIntegrityRepair(repairPackages, conditionLimit);

            if (partsNeedingRepair != null)
            {
                foreach (var leafPart in partsNeedingRepair)
                {
                    HandleRepairOfPart(repairPackages, leafPart); // each call grows the list of repair packages                              
                }
            }

            return repairPackages;
        }

        private void CreateIntegrityRepair(List<RepairPackage> repairPackages, float conditionLimit)
        {
            if (NeedsIntegrityRepair(conditionLimit))
            {
                ProcessType process;
                if (CanRepairIntegrity(out process))
                {
                    CreateOrAddToExistingPackage(repairPackages, null,
                        new RepairPackageAction()
                        {
                             RepairAction = RepairAction.Integrity,
                             RepairProcess = process
                        });               
                }
            }
        }

        private void CreateOrAddToExistingPackage(List<RepairPackage> list, Entity part, RepairPackageAction partRepairAction)
        {
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (!item.IncludesPartRepair(part))
                    {
                        item.Actions.Add(partRepairAction);
                    }
                }
            }
            else
            {
                list.Add(new RepairPackage() { Actions = new List<RepairPackageAction>() { partRepairAction } });
            }
        }

        private void HandleRepairOfPart(List<RepairPackage> repairPackages, EntityOrType leafPart)
        {
            // add repair actions to all repair packages, if not already included 
            // create new packages by cloning existing packages and appending more actions
            List<RepairPackage> copy1 = null, copy2 = null, copy3 = null;

            ProcessType process;
            if (leafPart.Entity != null
                && CanReconditionPart(leafPart.Entity, out process)) // only reconditioning of parts in-place, for now...
            {
                copy1 = CopyPackages(repairPackages); // copy packages
                RepairPackageAction partRepairAction = new RepairPackageAction() { RepairAction = RepairAction.PartsCondition, Part = leafPart.Entity, RepairProcess = process };
                CreateOrAddToExistingPackage(copy1, leafPart.Entity, partRepairAction);
                /*
                if (copy1.Count > 0)
                {
                    foreach (var item in copy1)
                    {
                        if (!item.IncludesPartRepair(leafPart.Entity))
                        {
                            item.RepairOptions.Add(new PartRepairAction() { RepairAction = RepairAction.PartsCondition, Part = leafPart.Entity });
                        }
                    }
                }
                else
                {
                    copy1.Add(new RepairPackage() { RepairOptions = new List<PartRepairAction>() { new PartRepairAction() { RepairAction = RepairAction.PartsCondition, Part = leafPart.Entity } }})
                }*/
            }

          /*  
            if (CanReplace(leafPart, out process))
            {
                copy2 = CopyPackages(repairPackages); // copy packages

                //TODO: if the input is not available, add a RemovePart action instead
                foreach (var item in copy2)
                {
                    if (!item.IncludesPartRepair(leafPart))
                    {
                        item.RepairOptions.Add(new PartRepairAction() { RepairAction = RepairAction.ReplacePart, Part = leafPart, RepairProcess = process });
                    }
                }
            }

            // replace parent parts
            List<Entity> parentParts = null;
            if (CanReplaceParentPart(leafPart, ref parentParts))
            {
                copy3 = CopyPackages(repairPackages); // copy packages

                //TODO: if the input is not available, add a RemovePart action instead
                foreach (var item in copy3)
                {
                    foreach (var parentPart in parentParts)
                    {
                        if (!item.IncludesPartRepair(leafPart)
                            && !item.IncludesPartRepair(parentPart))
                        {
                            item.RepairOptions.Add(new PartRepairAction() { RepairAction = RepairAction.ReplacePart, Part = parentPart });
                        }
                    }
                }
            }*/


            if (copy1 != null)
            {
                repairPackages.AddRange(copy1);
            }

            if (copy2 != null)
            {
                repairPackages.AddRange(copy2);
            }

            if (copy3 != null)
            {
                repairPackages.AddRange(copy3);
            }

        }

        private static List<RepairPackage> CopyPackages(List<RepairPackage> repairPackages)
        {
            List<RepairPackage> copy = new List<RepairPackage>(); //repairPackages);
            foreach (var item in repairPackages)
            {
                RepairPackage packageCopy = new RepairPackage(item);
                copy.Add(packageCopy);
            }

            return copy;
        }

        /// <summary>
        /// reconditioning a part inplace!
        /// </summary>
        /// <param name="part"></param>
        /// <param name="processType"></param>
        /// <returns></returns>
        private bool CanReconditionPart(Entity part, out ProcessType processType)
        {
            if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsCondition != null)
            {
                if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsCondition.TryGetValue(part.EntityType, out processType))
                {                   
                    return true;
                }
            }

            processType = null;
            return false;
            //return true;
        }

        private bool CanRepairIntegrity(out ProcessType processType)
        {
            if (Parent.EntityType.NonLivingType.EntityRepairProfile.Integrity != null)
            {
                processType = Parent.EntityType.NonLivingType.EntityRepairProfile.Integrity;
                return true;
            }

            processType = null;
            return false;
        }

        private bool CanRecondition(Entity part, out ProcessType processType)
        {
            if (part.EntityType.NonLivingType.EntityRepairProfile.Condition != null)
            {
                processType = part.EntityType.NonLivingType.EntityRepairProfile.Condition;
                return true;
            }

            processType = null;
            return false;
        }

        private bool CanReplace(Entity part, out ProcessType processType)
        {
            if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement != null)
            {
                if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement.TryGetValue(part.EntityType, out processType))
                {
                    return true;
                }
            }

            processType = null;
            return false;
        }

        private bool CanReplaceParentPart(Entity part, ref List<Entity> parentParts)
        {
            Entity parentOfPart = part.PartOf as Entity;
            while(parentOfPart != null
                && Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement.ContainsKey(parentOfPart.EntityType))
            {
                Common.AddToList(ref parentParts, parentOfPart);
                parentOfPart = parentOfPart.PartOf as Entity;
            }          

            return true;
        }

        private void GatherMissingParts(ref List<EntityOrType> partsNeedingRepair, int level)
        {
            // detect missing parts at every level:
            // TODO: give each part type slot an ID to quickly match parts / part types
            foreach (var item in Parent.EntityType.Parts)
            {
                int partsDelta = item.Value - Parts.Count;
                if (partsDelta > 0)
                {
                    // missing parts detected:
                    for (int i = 0; i < partsDelta; i++)
                    {
                        Common.AddToList(ref partsNeedingRepair, new EntityOrType(null, item.Key, level));
                    }
                }

                if (item.Key.Parts != null)
                {
                    GatherMissingParts(ref partsNeedingRepair, level + 1);
                }
            }
        }

        private void GatherPartsWithProblems(ref List<EntityOrType> partsNeedingRepair, float conditionLimit)
        {
           // GatherMissingParts(ref partsNeedingRepair, 0);

            GatherPartsNeedingRepair(ref partsNeedingRepair, conditionLimit, 0);

        }
      
        private void GatherPartsNeedingRepair(ref List<EntityOrType> partsNeedingRepair, float conditionLimit, int level)
        {

            if (Parts != null)
            {               
               
                // iterate the parts, look for condition < limit;
                foreach (var part in Parts)
                {
                   /* if (!Parent.EntityType.IsInOriginalBlueprint(part.EntityType))
                    {
                        // junk part detected
                        Common.AddToList(ref partsNeedingRepair, new EntityOrType(part, part.EntityType, level, true));    
                    }
                    else*/ if (part.IsLeaf)
                    {
                        if (part.Condition < conditionLimit)
                        {
                            Common.AddToList(ref partsNeedingRepair, new EntityOrType(part, part.EntityType, level));
                        }
                    }
                    else
                    {
                        part.NonLivingEntity.GatherPartsNeedingRepair(ref partsNeedingRepair, conditionLimit, level + 1);
                    }
                }                
            }
        }

        #endregion



        #region ISnapshot


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.condition = sn.DoFloat(condition);
            this.compositeConditionIsDirty = sn.DoBool(compositeConditionIsDirty);
            //this.Condition = sn.DoFloat(this.Condition);
            this.ConditionChangeSpeed = sn.DoFloatNullable(ConditionChangeSpeed);
            this.MaxCondition = sn.DoFloat(this.MaxCondition);
            this.progress = sn.DoFloat(this.progress);
            this.integrity = sn.DoFloatNullable(integrity);

            if (partOf != null)
            {
                partOfID = partOf.ID;
            }
            else
            {
                partOfID = null;
            }
            partOfID = sn.DoEnumNullable(partOfID);

            if (Parts != null)
            {
                snapshotParts = Parts.Select(p => p.EntityID).ToList();
            }
            snapshotParts = sn.DoList(snapshotParts);

            sn.Ignore(partOf);
            sn.Ignore(Parts);


            return this;
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


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (partOfID.HasValue)
            {
                partOf = LookUpIComposites.FindByID(partOfID.Value); // uses special class!
            }

            if (snapshotParts != null)
            {
                Parts = snapshotParts.Select(p => Entity.FindByID(p)).ToList();
                snapshotParts = null;
            }

            //CreateRegulators();
        }


        #endregion
    }
}

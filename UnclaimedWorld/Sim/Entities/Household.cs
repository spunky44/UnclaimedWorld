using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Activities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
namespace UWGame.SimSide.Entities
{
    public enum HouseholdID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    public class Household: IHasEntityGroup, IOwner, ICanIterateEntities, ILookUp<Household, HouseholdID>, ISnapshot
    {
       
       
      //  public Expeditions.Expedition Expedition;
      //  ExpeditionID? snapshotExpedition;

        public Expeditions.Expedition Expedition
        {
            get
            {
                if (HeadOfHousehold1 != null)
                {
                    return HeadOfHousehold1.Intelligence.CurrentExpedition;
                }

                return null;
            }
        }

        private List<Entity> members = new List<Entity>();
        private List<EntityID> snapshotMembers = new List<EntityID>();
      
        public Entity HeadOfHousehold1;       
 
        /// <summary>
        /// eh..? Delete this... We need 1 leader to make decisions
        /// </summary>
        public Entity HeadOfHousehold2;
        EntityID? snapshotHead1, snapshotHead2;

        public FoodExtraction FoodExtraction;


        public int TotalAssignedFood = 0;
        public Dictionary<EntityType, int> FoodAssignedToday = new Dictionary<EntityType, int>();

        private EntityID? home;
        /// <summary>
        /// keep Home residents updated when household members change!
        /// </summary>
        public EntityID? Home
        {
            get
            {
                return home;
            }
            set
            {
                if (this.home != value)
                {
                    this.home = value;

                    if (Allegiance != null)
                    {
                        if (!Residence.UpdateResidentsNo(Allegiance.SharedKnowledge, home))
                        {
                            home = null;
                        }
                    }
                }
               
            }
        }
     
        private EntityGroup ownedEntities;
        EntityGroupID snapshotOwnedEntities;

        public EntityGroup OwnedEntities
        {
            get
            {
                return ownedEntities;
            }
        }

        private decimal? tradeCredits = 0;
        public decimal? TradeCredits
        {
            get { return tradeCredits; }
            set { tradeCredits = value; }
        }


        public OwnerID? GetOwnerID()
        {
            OwnerID? ownerID = null;
            IOwner iOwner = this as IOwner;
            if (iOwner != null)
            {
                ownerID = iOwner.ID;
            }

            return ownerID;
        }

        public int NoOfMembers
        {
            get
            {
                return members.Count;
            }
        }

        public int NoOfWorkers
        {
            get
            {
                return members.Count;
            }
        }
     
       

       /* public Entity Home
        {
            get 
            {
                return Entity.FindByID(homeEntityID);
            }
            set 
            {
                if (homeEntityID == null)
                {
                    homeEntityID = value.ID;

                    FillNewHome();
                }
                else
                {
                    homeEntityID = value.ID;
                }     
            }
        }*/

        public Allegiance GetAllegiance
        {
            get
            {
                return this.Allegiance;
            }
        }

        public void IterateMembers(Action<Entity> iterateFunction)
        {
            foreach (var item in members)
            {
                iterateFunction(item);
            }
        }

        public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
        {
            iterateFunction(ownedEntities);
        }


        public bool IsEatable(EntityType entityType)
        {
            return FoodExtraction.IsEatable(entityType); 
        }

        public Allegiances.Allegiance Allegiance 
        {
            get
            {
                if (Expedition != null)
                {
                    return Expedition.Allegiance;
                }

                return null;

                /*

                if (HeadOfHousehold1 == null)
                {
                    return null;
                }
                else
                {
                    return HeadOfHousehold1.Intelligence.Allegiance;
                }*/
            }          
        }


        Vector3? IHasEntityGroup.Location
        {
            get
            {
                return Location;
            }
        }

        /// <summary>
        /// off map households..?
        /// </summary>
        public Vector3? Location
        {
            get
            {
                if (Home != null)
                {
                    IKnownEntityData homeData = ResolveHome();
                    if (homeData != null)
                    {
                        return homeData.Location;
                    }

                    /*
                    Entity home = Entity.FindByID(Home);
                    if (home != null)
                    {
                        return Home.Location;
                    }
                    else
                    {
                        Home = null; 
                    }*/
                }

                return HeadOfHousehold1.Location;
            }
        }


        public IKnownEntityData ResolveHome()
        {
            if (Home != null)
            {
                IKnownEntityData homeData;
                if (GoalEvaluator.EntityDataResultCausesSkip(Allegiance.SharedKnowledge.GetKnownData(Home.Value, out homeData)))
                {
                    Home = null;// clear invalid ID
                }
                else
                {
                    return homeData;
                }
            }

            return null;

        }

        /// <summary>
        /// households can never split in two homes
        /// </summary>
        /// <param name="home"></param>
        /// <returns></returns>
        public bool SetHome(EntityID home)
        {     
            if (Residence.AddHousehold(Allegiance.SharedKnowledge, this, home))
            {
                // move out of old home:
                if (Home.HasValue)
                {
                   /* IKnownEntityData oldHomeData;
                    if (GoalEvaluator.EntityDataResultCausesSkip(Allegiance.SharedKnowledge.GetKnownData(Home.Value, out oldHomeData)))
                    {*/
                    Residence.RemoveHousehold(Allegiance.SharedKnowledge, this);
                   // }
                }

                Home = home;

                return true;
            }

            return false;
        }
             

        private void FillNewHome()
        {
            // fill stocks:
          /*  ClearHaulingJobs();

            // ??? move to Manager?
            foreach (ItemType itemType in foodItemTypesForHauling)
            {
                AddHaulingJob(itemType, Expedition.ExpeditionOwner, Ownership);
            }

            foodItemTypesForHauling.Clear();
            */


            // enable cooking:
           /* if (CookingJob.Count == 0)
            {
                CookingJob cJob = new CookingJob(Home, ownerContent.ID, CookingJob);               
            }*/

            // haul the old household stuff:
           // HaulStuffToNewHome(Items); // not with the new manager...

            // haul the personal stuff:
          /*  foreach (Entity member in Members)
            {
                HaulStuffToNewHome(member.PersonEntity.Items);
            }*/

        }

      /*  private void HaulStuffToNewHome(Dictionary<ItemType, List<Item>> items)
        {
            foreach (KeyValuePair<ItemType, List<Item>> kvp in items)
            {
                Item itemToMove;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    itemToMove = kvp.Value[i];

                    if (itemToMove.IsUnassigned() && itemToMove.MapPosition != home.MapPosition)
                    {
                        AddHaulingJob(itemToMove, Ownership);
                    }
                }
            }
        }*/

        public int NoOfChildren()
        {
            int noOfChildren = 0;
            foreach (Entity member in members)
            {
                if (!member.PersonEntity.IsHeadOfHousehold() && member.PersonEntity.IsChild())
                {
                    noOfChildren++;
                }
            }
            return noOfChildren;
        }

        public Household()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Household(Expeditions.Expedition expedition, Entity member) //Expeditions.Expedition expedition) //Site site)
        {
            AddToLookup();
            ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
            ((ILookUp<IOwner, OwnerID>)this).AddToLookup();

            //this.Expedition = expedition;

         //   Members.ListMemberRemoved += new ObservableList<Entity>.ListMemberRemovedHandler(Members_ListItemRemoved);
         //   Members.ListMemberAdded += new ObservableList<Entity>.ListMemberAddedHandler(Members_ListItemAdded);


            //Ownership = new Owner(this);//, new Dictionary<ItemType, List<Item>>(), new Dictionary<ItemType, List<Item>>(), new List<global::UWGame.SimSide.Vehicles.Vehicle>());


            ownedEntities = new EntityGroup(this, true, true); //, FoodExtraction); 

            ownedEntities.HaulingJobManager = new HaulingJobManager(ownedEntities);

            ownedEntities.OtherJobManager = new OtherJobManager(ownedEntities);

            FoodExtraction = new FoodExtraction(this, ownedEntities.ID);
            //FoodExtraction.FoodProcessesChanged += new Allegiances.FoodExtraction.FoodProcessesChangedHandler(FoodExtraction_FoodProcessesChanged);

            AddMember(member);


           /* ResetRations();

            items.Add(GameData.Instance.MeatType, new List<Entity>());
            items.Add(GameData.Instance.StaplesType, new List<Entity>());
            items.Add(GameData.Instance.VegetablesType, new List<Entity>());
            */

         //   FoodstocksOwnership = new Owner(this, FoodStocks);
        //    OtherItemsOwnership = new Owner(this, HouseholdItems);


            if (expedition != null)
            {
                expedition.AddHousehold(this);                
            }
        }

        public void Destroy(Entity lastMember)
        {
            Allegiance allegiance = lastMember.Intelligence.Allegiance; // properties are now invalid since the last member has been removed
            Expedition expedition = lastMember.Intelligence.CurrentExpedition;

            //TransferAllOwnedEntities(ownerContent);
            
            // belongings go to the state...     
            List<IOwner> owners = new List<IOwner>();
            if (expedition != null)
            {
                owners.Add(expedition);
            }

            Person.DivideItems(OwnedEntities, owners);

          //  DivideBelongingsWhenDestroyed(owners);
           
            Residence.RemoveHousehold(allegiance.SharedKnowledge, this);


            // delete this household:
            if (expedition != null)
            {
                expedition.RemoveHousehold(this);
            }

           
            ownedEntities.Destroy();

            RemoveIDEntry();
            ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
            ((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();

        }


       /* public static void TransferAllOwnedEntities(Owner ownerContent)
        {
            // move all household items to the expedition?

            ownerContent.IterateEntities(e =>
                {
                    if (e.OwnedBy == ownerContent.ID)
                    {
                        e.OwnedBy = null; // set to unowned
                    }
                });
        }*/

        void FoodExtraction_FoodProcessesChanged()
        {
            ownedEntities.SetFoodDirty();
            
        }

        public void AddMember(Entity entity)
        {
            members.Add(entity);

            UpdateWhenMembersChanged();

           
        }

       
        void Members_ListItemAdded(object sender)
        {
            AssignHeadsOfHousehold();
        }

        void Members_ListItemRemoved(object sender, int indexOfRemovedItem)
        {
            AssignHeadsOfHousehold();
        }

        public bool IsEntitled()
        {
            // TODO: claculate based on work for colony
            return true;
        }

       /* private void AddHaulingJobsForMembers(int noOfMembers)
        {
            for (int i = 0; i < noOfMembers; i++)
            {
                for (int j = 0; j < UWGame.SimSide.FoodStockSize; j++)
                {
                    AddHaulingJob(UWGame.SimSide.Instance.MeatType);
                    AddHaulingJob(UWGame.SimSide.Instance.GrainType);
                    AddHaulingJob(UWGame.SimSide.Instance.VegetablesType);
                }
            }
        }*/

        /// <summary>
        /// Remove the person from the previous household and add it to this.
        /// Delete the household if it is empty now. 
        /// </summary>
        /// <param name="member"></param>
    /*    public void AddMember(PersonEntity member)
        {
            
        }*/

        /// <summary>
        /// do this every time the household members change.
        /// </summary>
        private void AssignHeadsOfHousehold()
        {
            Entity currentBest = null, currentSecondBest = null;
            float bestScore = 0f;
            float secondBestScore = 0f;

            float score;

            foreach (Entity member in members)
            {
                score = ScoreMemberAsHeadOfHousehold(member);

                if (score > bestScore)
                {
                    currentSecondBest = currentBest;
                    secondBestScore = bestScore;

                    currentBest = member;
                    bestScore = score;                    
                }
                else if (score > secondBestScore)
                {
                    currentSecondBest = member;
                    secondBestScore = score;
                }
            }

            HeadOfHousehold1 = currentBest; // can be null
            HeadOfHousehold2 = currentSecondBest; // can also be null

        }

        private float ScoreMemberAsHeadOfHousehold(Entity member)
        {
            float sexScore;

            if (member.BiologicalEntity.CasteType.Reproduction == Reproduction.Male)
            {
                sexScore = 1f;
            }
            else
            {
                sexScore = 0.7f;
            }

          /*  if (member.BiologicalEntity.BiologicalChildren.Count > 0)
            {

            }*/

            float ageScore;
            float ageFactor = 1f;
            if (member.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Adult)
            {
                ageFactor = 3f;
            }

            // add more?

            ageScore = ageFactor * Common.ClampTop((member.BiologicalEntity.AgeGroup.Age / member.BiologicalEntity.CasteType.MaxAge), 1f);

            return 0.3f * sexScore + 0.7f * ageScore;
        }

        public void MergeHouseholds(Household householdToDissappear)
        {      
            // LARS: All of the ownership related code needs to be reworked, and most code moved to OwnerContent...

            /*
            //member.Household.RemoveMember(member);

           // move the members:
            for (int i = 0; i < householdToDissappear.Members.Count; i++)
            {
                householdToDissappear.Members[i].PersonEntity.Household = this;
            }
            Members.AddRange(householdToDissappear.Members);

            // move the items:
            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in householdToDissappear.ownerContent.Items)
            {
                Entity itemToMove;
                EntityID id;
                while (kvp.Value.Count > 0)
                {
                    itemToMove = kvp.Value[kvp.Value.Count - 1];
                    
                    if (itemToMove.PartOf == null)
                    {                        
                        itemToMove.ChangeOwnership(Ownership);

                        if (Home != null && itemToMove.IsUnassigned() && itemToMove.MapPosition != home.MapPosition)
                        {
                            AddHaulingJob(itemToMove, Ownership);
                        }
                    }
                }            
            }
            
            // move the vehicles:            
            for (int i = 0; i < householdToDissappear.ownerContent.Vehicles.Count; i++)
            {
                householdToDissappear.ownerContent.Vehicles[i].ChangeOwnership(Ownership);
            }
            // move the buildings:   
            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in householdToDissappear.ownerContent.Buildings)
            {
                EntityID buildingToMove;
                while (kvp.Value.Count > 0)
                {
                    buildingToMove = kvp.Value[kvp.Value.Count - 1];
                    buildingToMove.ChangeOwnership(Ownership);

                }
            }

            // delete old household:
            if (householdToDissappear.Expedition != null)
            {
                householdToDissappear.Expedition.Households.Remove(householdToDissappear);
            }
           // UWGame.SimSide.Instance.Households.Remove(householdToDissappear);

            if (householdToDissappear.Home != null)
            {   // remove it from its home:
                Residence residenceComponent;
                householdToDissappear.Home.Find(out residenceComponent);

                residenceComponent.RemoveHousehold(householdToDissappear);
            }                        

            // merge jobs:
            // just update references - used by goalevaluator object. if there are any jobs in the old lists, they are lost!
            householdToDissappear.ownerContent.HaulingJobs = ownerContent.HaulingJobs;

            // merge activities:
            // just update references - used by goalevaluator object. if there are any activities in the old lists, they are lost!
            householdToDissappear.OwnerContent.Activities = ownerContent.Activities;

            // join the two cooking jobs if both are yet to start:
            if (householdToDissappear.CookingJob != null 
                 && householdToDissappear.CookingJob.Count > 0
                 && !((CookingJob)CookingJob[0]).IsInProgress())
            {
                CookingJob oldCookingJob = (CookingJob)householdToDissappear.CookingJob[0];
                // if cooking has not yet started, invite the new members to eat at the new place: 
                if (!oldCookingJob.IsInProgress())
                {
                    ((CookingJob)CookingJob[0]).WillBeEating.AddRange(oldCookingJob.WillBeEating);
                    oldCookingJob.WillBeEating.Clear();
                }
            }

            householdToDissappear.CookingJob = CookingJob;

            The.Sim.CycleManager.UnRegister(householdToDissappear.ownerContent.HaulingJobManager);

               
            if (Home != null)
            {
                Residence residenceComponent;
                Home.Find(out residenceComponent);

                residenceComponent.UpdateResidentsNo();
            }

            householdToDissappear.RemoveOwnerFromCollection();

            // this takes care of re-referencing all the old goals and jobs to the new items and vehicles collections:
            Ownership.MergeOwners(householdToDissappear.Ownership);

*/


            // reset the evaluators to reference the new household:
            // UNECESSARY???
       /*     for (int i = 0; i < Members.Count; i++)
            {
                Members[i].Brain.ResetLeisureEvaluators();
            }*/
        }

        /// <summary>
        /// TODO: Rework this!!! only one household in Demo.
        /// Remove members and create a new household for them. Split the household's possessions evenly.
        /// When we split, we have to update the leaving persons' evaluators to point at the new Household Owner object.
        /// Also, any active goals and jobs will stay associated to the 'old' household. 
        /// So acitve hauling jobs may move split items from the new household back to the old...
        /// Wouldn't it be best to cancel all household jobs, to be on the safe side?
        /// </summary>
        public void SplitHouseholds(List<Entity> membersOfNewHousehold, bool splitItems, bool splitVehicles, bool splitBuildings)
        {
            if (membersOfNewHousehold.Count >= members.Count)
            {
                throw new Exception("Illegal split. Household would be destroyed...");
            }
/*
            // make it belong to the same Site:
            Household newHousehold = new Household(Expedition);

            Entity member;
            for (int i = 0; i < membersOfNewHousehold.Count; i++)
            {
                member = membersOfNewHousehold[i];
                RemoveMember(member);

                member.PersonEntity.Household = newHousehold;
                newHousehold.Members.Add(member);
            }

            // split possessions, items first:
            float splitDivision = (float)(newHousehold.Members.Count) / (float)(Members.Count + newHousehold.Members.Count);
            int itemsToAdd, alreadyAdded = 0, topLevelItems;
            if (splitItems)
            {               
                // CHANGE OWNERSHIP
                foreach (KeyValuePair<EntityType, List<EntityID>> kvp in ownerContent.Items)
                {
                    topLevelItems = 0;

                    // only count top level items
                    foreach (Entity item in kvp.Value)
                    {
                        if (item.PartOf == null)
                        {
                            topLevelItems++;
                        }
                    }

                    itemsToAdd = (int)(topLevelItems * splitDivision);

                    alreadyAdded = 0;
                    int i = kvp.Value.Count - 1;
                    // start removing from the end of the list:
                    while (alreadyAdded < itemsToAdd && i >= 0)
                    {
                        if (kvp.Value[i].PartOf == null)
                        {
                            kvp.Value[i].ChangeOwnership(newHousehold.Ownership);
                            alreadyAdded++;
                        }
                        i--;
                    }
                                        
                   
                }                   
                  
            }

            if (splitVehicles)
            {
                // vehicles:
                if (ownerContent.Vehicles.Count > 0)
                {
                    List<Entity> vehicles = ownerContent.Vehicles;
                    // divide vehicles based on seat capacity...
                    int totalCapacity = 0;
                    for (int i = 0; i < vehicles.Count; i++)
                    {   // compute the total
                        totalCapacity += vehicles[i].EntityType.VehicleType.MaxPassengers;
                    }

                    int capacityToAdd = (int)(totalCapacity * splitDivision);
                    int capacityAdded = 0;
                    int j = vehicles.Count - 1;
                    while (capacityAdded < capacityToAdd && j > 0)
                    {
                        vehicles[j].ChangeOwnership(newHousehold.Ownership);
                        capacityAdded += vehicles[j].EntityType.VehicleType.MaxPassengers;
                        j--;
                    }
                }
                
            }

            if (splitBuildings)
            {
                foreach (KeyValuePair<EntityType, List<Entity>> kvp in ownerContent.Structures)
                {
                    itemsToAdd = (int)(kvp.Value.Count * splitDivision);
                    alreadyAdded = 0;
                    int i = kvp.Value.Count - 1;
                    // start removing from the end of the list:
                    while (alreadyAdded < itemsToAdd && i >= 0)
                    {
                        kvp.Value[i].ChangeOwnership(newHousehold.Ownership);
                        alreadyAdded++;
                        i--;
                    }
                }
            }

            if (CookingJob != null
                && CookingJob.Count > 0
                && !((CookingJob)CookingJob[0]).IsInProgress())
            {
                for (int i = 0; i < newHousehold.Members.Count; i++)
                {   // won't be eating here anymore...
                    ((CookingJob)CookingJob[0]).WillBeEating.Remove(newHousehold.Members[i]);
                
                }

            }

            // reset the evaluators to reference the new household:
            //Entity member;
            foreach (Entity member1 in newHousehold.Members) // int i = 0; i < newHousehold.Members.Count; i++)
            {
                // also CANCEL jobs!?!?! what if... the person is flying. He must then land...
                                
                member1.HandleMessage(new Message(member1, Message.MessageTypes.CancelJobForAIReset, Message.CancelJobKeepVehicle.LeaveVehicle));

                member1.Intelligence.Brain.ResetEvaluators(); //ResetHouseholdAndAgeGroupDependentEvaluators();
            }
            */
        }

        /*
        private void AddHaulingJob(EntityType itemType, Owner ownerOfItemsToHaul, Owner newOwner)
        {
            HaulingJobAnyItemOfType haulingJob = new HaulingJobAnyItemOfType(
                               Home.Location, ownerContent.HaulingJobs, itemType, ownerOfItemsToHaul, newOwner);
            
        }

        private void AddHaulingJob(Entity item, Owner newOwner)
        {
            HaulingJobSpecificItem haulingJob = new HaulingJobSpecificItem(
                                home.Location, null, null, ownerContent.HaulingJobs, item, newOwner, true);            
            
        }
        */
    /*    private void AddItemHaulingJob(Item item)
        {
            HaulingJob haulingJob = new HaulingJob(Home.EntranceAbsolute, otherHaulingJobs);
            haulingJob.Item = item;
        }*/

     /*   private void ClearHaulingJobs()
        {
            // TODO: IF the job is being handled, find some way to cancel it!
            haulingJobs.Clear();
        }*/

        /// <summary>
        /// call daily
        /// </summary>
    /*    public void ResetEntitledFoodHaulingJobs()
        {           
            FoodAssignedToday.Clear();
            FoodAssignedToday.Add(GameData.Instance.MeatType, 0);
            FoodAssignedToday.Add(GameData.Instance.StaplesType, 0);
            FoodAssignedToday.Add(GameData.Instance.VegetablesType, 0);
           ClearHaulingJobs();

            if (Home == null)
            {
                foodItemTypesForHauling.Clear();
                
            }

            int totalTakenAllMembers = 0;
            int maxStockSize = 3 * Members.Count * UWGame.SimSide.FoodStockSize;

            // fill up to the normal limit
            foreach (KeyValuePair<ItemType, int> kvp in Expedition.FoodEntitlementPerPerson) 
            {
                int jobsToAdd = Math.Min(Members.Count * kvp.Value, Members.Count * UWGame.SimSide.FoodStockSize) - Items[kvp.Key].Count;
                jobsToAdd = Math.Max(0, jobsToAdd);

                FoodAssignedToday[kvp.Key] = jobsToAdd;
                totalTakenAllMembers = totalTakenAllMembers + jobsToAdd;
                for (int i = 0; i < jobsToAdd; i++)
                {
                    if (Home != null)
                    {
                        AddHaulingJob(kvp.Key, Expedition.ExpeditionOwner, Ownership);
                    }
                    else
                    {
                        foodItemTypesForHauling.Add(kvp.Key);
                    }
                }
            }

            int totalStocksAndAddedJobs = TotalFoodStocks + totalTakenAllMembers;

            // if there is a shortage, go over the limit for some food items if we can:
            int totalEntitled = Members.Count * Expedition.TotalEntitledPerPerson; 
            if (totalTakenAllMembers < totalEntitled && totalStocksAndAddedJobs < maxStockSize)
            {
                foreach (KeyValuePair<ItemType, int> kvp in Expedition.FoodEntitlementPerPerson) 
                {
                    int jobsToAdd = Math.Max(0, Members.Count * kvp.Value - FoodAssignedToday[kvp.Key]); 
                     
                    int i = 0;
                    while (i < jobsToAdd && totalTakenAllMembers < totalEntitled && totalStocksAndAddedJobs < maxStockSize)
                    {
                        i++;
                        totalTakenAllMembers++;
                        totalStocksAndAddedJobs++;
                        FoodAssignedToday[kvp.Key] = FoodAssignedToday[kvp.Key] + 1;

                        if (Home != null)
                        {
                            AddHaulingJob(kvp.Key, Expedition.ExpeditionOwner, Ownership);
                        }
                        else
                        {
                            foodItemTypesForHauling.Add(kvp.Key);
                        }
                    }
                }
            }

            if (Home != null)
            {
                if (CookingJob.Count == 0)
                {
                    CookingJob cJob = new CookingJob(Home, Ownership, CookingJob);
                }
            }

        }*/

     /*   public void GetRationsStillNeeded(out int meat, out int staples, out int veg)
        {            
            if ((Members.Count * Expedition.RationLevel - TotalAssignedFood) > 0)
            {
                FoodAssignedToday[]

            }
        }*/

      


    /*    public static void DivideBuildings(Dictionary<StructureType, List<Structure>> itemsToDivide, List<Owner> newOwners)
        {
            int noOfPortions = newOwners.Count;

            float splitDivision = (float)(1 / noOfPortions);

            // use round-robin method to divide items
            int currentTurn = 0;

            // CHANGE OWNERSHIP
            int i;
            foreach (KeyValuePair<StructureType, List<Structure>> kvp in itemsToDivide)
            {
                i = kvp.Value.Count - 1;
                // start removing from the end of the list:
                while (i >= 0)
                {
                 //   kvp.Value[i].ChangeOwnership(newOwners[currentTurn]);
                    currentTurn++;
                    currentTurn = currentTurn % noOfPortions;
                    
                    i--;
                }
            }

        }*/


        public void RemoveMember(Entity member)
        {
            if (members.Contains(member))
            {
                members.Remove(member);

                UpdateWhenMembersChanged();


                if (members.Count == 0)
                {  
                    // distribute the belongings to the heirs of the last household member:
                    List<Entity> heirs;
                    List<IOwner> owners;

                    member.PersonEntity.GetHeirsAsOwners(out heirs, out owners);
                    Person.DivideItems(ownedEntities, owners);            
                                       
 
                    // delete this household:
                    Destroy(member);
                }


            }
        }

    /*    public void UpdateResidentsNo()
        {
            if (Home != null)
            {
                IKnownEntityData homeData;
                if (GoalEvaluator.EntityDataResultCausesSkip(Allegiance.SharedKnowledge.GetKnownData(Home.Value, out homeData)))
                {
                    Home = null;
                }
                else
                {


                    homeData.Residents = 0;

                    for (int i = homeData.Households.Count - 1; i >= 0; i--)
                    {
                        
                        Household household = LookUp<Household, HouseholdID>.FindByID(homeData.Households[i]);
                        if (household != null)
                        {
                            homeData.Residents += household.NoOfMembers;
                        }
                        else
                        {
                            homeData.Households.RemoveAt(i);
                        }
                    }

       

                    IResidence residence = Home.Contains as IResidence;

                    if (residence != null)
                        residence.Residence.UpdateResidentsNo();
                }
            }

            Residents = 0;

            for (int i = 0; i < Households.Count; i++)
            {
                Residents += Households[i].NoOfMembers;
            }

        }*/

       
        private void UpdateWhenMembersChanged()
        {
            AssignHeadsOfHousehold();

            if (Home != null && Allegiance != null)
            {
                if (!Residence.UpdateResidentsNo(Allegiance.SharedKnowledge, Home))
                {
                    Home = null;
                }
            }
                       
            Allegiance.HandleGroupMembersChanged(FoodExtraction);

            OwnedEntities.RecomputeFoodConsumeRate();
       
           // Statistics.NotifyPopulationChanged(Members.Count); // TODO

        }



        #region ILookup

        private HouseholdID id = HouseholdID.Invalid;
        static HouseholdID IDCounter = HouseholdID.First;

        public HouseholdID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public HouseholdID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= HouseholdID.Max)
            {
                throw new Exception("Astounding, HouseholdID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public HouseholdID SnapshotID(Snapshotter sn, HouseholdID id)
        {
            return (HouseholdID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != HouseholdID.Invalid)
                LookUp<Household, HouseholdID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = HouseholdID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Household, HouseholdID>.Remove(this);
        }

        void ILookUp<Household, HouseholdID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = HouseholdID.First;
        }

        void ILookUp<Household, HouseholdID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Household, HouseholdID>.Create();
        }

        #endregion


        #region CanIterateEntitiesID ILookup

        // this is the second ID a household has - for when it is referenced as an IHasMembers.

        CanIterateEntitiesID canIterateEntitiesID;
        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID
        {
            get
            {
                return canIterateEntitiesID;
            }
        }

        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
        {
            return HasMembers.GetUniqueID();
        }

      
        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
        {
            canIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();

            if (canIterateEntitiesID != CanIterateEntitiesID.Invalid)
            {
                LookUpICanIterateEntities.Add(canIterateEntitiesID, this); // uses special class!
            }
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
        {
            LookUpICanIterateEntities.Remove(this);  // uses special class!
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
        {
            canIterateEntitiesID = CanIterateEntitiesID.Invalid;
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        /*public static void CreateLookupCollection()
        {
            LookUpICanIterateEntities.Create();
        }*/

        #endregion




        #region HasEntityGroupID ILookup

        HasEntityGroupID hasEntityGroupID;
        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID
        {
            get
            {
                return hasEntityGroupID;
            }
        }

        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
        {
            return HasEntityGroup.GetUniqueID();
        }

    
        void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
        {
            hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();

            if (hasEntityGroupID != HasEntityGroupID.Invalid)
            {
                LookUpHasEntityGroup.Add(hasEntityGroupID, this); // uses special class!
            }
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
        {
            LookUpHasEntityGroup.Remove(this);  // uses special class!
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
        {
            hasEntityGroupID = HasEntityGroupID.Invalid;
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection() // interface method - does nothing...
        {
        }


        #endregion

        #region OwnerID ILookup

        OwnerID ownerID;
        OwnerID ILookUp<IOwner, OwnerID>.ID
        {
            get
            {
                return ownerID;
            }
        }

        OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
        {
            return Owner.GetUniqueID();
        }

      
        void ILookUp<IOwner, OwnerID>.AddToLookup()
        {
            ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();

            if (ownerID != OwnerID.Invalid)
            {
                LookUpOwners.Add(ownerID, this); // uses special class!
            }
        }

        void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
        {
            LookUpOwners.Remove(this);  // uses special class!
        }

        void ILookUp<IOwner, OwnerID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IOwner, OwnerID>.SetInvalid()
        {
            ownerID = OwnerID.Invalid;
        }

        void ILookUp<IOwner, OwnerID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        #endregion

        #region ISnapshot
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            #region IDs

            id = this.SnapshotID(sn, id);
            IDCounter = sn.DoEnum(IDCounter);

            hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
            canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);
            ownerID = sn.DoEnum(ownerID);

            #endregion

            //  this.FoodAssignedToday = sn.DoDictionary(FoodAssignedToday);
            this.FoodExtraction = (FoodExtraction)sn.DoISnapshot(FoodExtraction);
            this.snapshotHead1 = sn.SnapshotID<Entity, EntityID>(HeadOfHousehold1);
            this.snapshotHead2 = sn.SnapshotID<Entity, EntityID>(HeadOfHousehold2);

            this.snapshotOwnedEntities = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(ownedEntities);


           // this.snapshotExpedition = sn.SnapshotID<Expedition, ExpeditionID>(Expedition);
            this.Home = sn.DoEntityIDNullable(Home);
            this.tradeCredits = sn.DoDecimalNullable(tradeCredits);


            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotMembers = members.Select(e => e.ID).ToList();
            }
            this.snapshotMembers = sn.DoList(snapshotMembers);


            sn.Ignore(FoodAssignedToday); // not currently used
            sn.Ignore(TotalAssignedFood);
            sn.Ignore(members);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
            members = snapshotMembers.Select(e => Entity.FindByID(e)).ToList();
          //  Expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);

            HeadOfHousehold1 = Entity.FindByID(snapshotHead1);
            HeadOfHousehold2 = Entity.FindByID(snapshotHead2);

            ownedEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnedEntities);

            FoodExtraction.LoadPostProcess(sn);
        }

        #endregion
    }
}

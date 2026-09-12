using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.Client.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.GatheringSites;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities.RepairTypes;

namespace UWGame.SimSide.AI
{

    public interface IKnownEntityData : IHasExposedProperties
    {
        EntityID EntityID { get;  }

        /// <summary>
        /// make this nullable..
        /// </summary>
        Vector3? Location { get; }

        /// <summary>
        /// helper, that will give more accurate stack traces in any null reference exceptions.
        /// </summary>
        Vector3 PlaySiteLocation { get; }


        Vector3 RenderedLocation { get; }

        /// <summary>     
        /// we don't need to check null status in on-site code... like AI
        /// </summary>
        Point? MapPosition { get; }

        SiteID? Site { get; }

        /// <summary>
        /// for structures ..
        /// </summary>
        Point? TopLeftMapPosition { get; }
       // Point TopLeftMapPosition { get; }
            
        bool PartIsBroken { get; }

        float? Progress { get; }

        float Rotation { get; }

        Vector3 FacingNormal { get; }

        bool? IsMoving { get; }

        string Name { get; }
       
        EntityType EntityType { get; }

        CasteType CasteType { get; }

        EntityAndRoot GetAsEntityAndRoot();

        AllegianceID? AllegianceID { get; }

        ThreatGroup ThreatGroup { get; }

        bool IsTimeToShowStatusMarkerWindow();

        /// <summary>
        /// not really needed in MemoryFact since all robots are agents. 
        /// For recharging, we will need their power modules because the robot can go dead...
        /// </summary>
        Dictionary<EntityType, EntityID> IntrinsicWeapons { get; }

        /// <summary>
        /// Defines which of the physical EntityType.SpecialActionTypes are available - only anchor processes?
        /// </summary>
        List<ProcessType> AvailableSharedSpecialActions { get; }

      //  bool AreaIsCleared { get; }

        List<SimProcessID> Processes { get; }

        EntityTypeTooltipInstanceData TooltipEntityData { get; }

        /// <summary>        
        /// We should only be able to claim an entity that we can see directly!
        /// However, it is ok to give up ownership (set Owner to null) of a remembered entity.
        /// </summary>
        OwnerID? OwnedBy { get; set; }

        #region Containers

        /// <summary>
        /// most likely a building or a vehicle
        /// </summary>
     //   EntityID? ContainingBuilding { get; }


        EntityID? ContainedBy { get; }

        /// <summary>
        /// storage or magazine for items
        /// </summary>
      //  EntityID? InnerContainer { get; }

        /// <summary>
        /// is null if stored on a person or similar.
        /// Also null when stored for trading!
        /// Non-null if in an IStorage structure
        /// </summary>
        StorageTarget? StoredPermanentlyIn { get; }

       // bool IsOfferedForTrade { get; }

      //  StorageTarget? OfferedForTradeIn { get; }

        /*
        EntityID? StoredPermanentlyIn  { get; }
        StorageID? StoredPermanentlyStorageID { get; }
        StorageCondition StoredPermanentlyCondition { get; }

        EntityID? OfferedForTradeIn { get; } // NEW
        StorageID? OfferedForTradeStorageID { get; }
        StorageCondition OfferedForTradeCondition { get; } // NEW
        */

        EntityID? Replenishes { get; }

        EntityID? UpgradeFor { get; }

        Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; }

        /// <summary>
        /// we don't keep a reference to the container component itself since the entity may be destroyed...
        /// </summary>
        bool HasItemStorage { get; }

        
        //   bool ItemStorageContains(EntityID entity);

        bool ContainsEntity(EntityID entity);

        /// <summary>
        /// used for populating side panel - not needed??
        /// </summary>
       // Dictionary<EntityType, int> ContainedEntityTotals { get; } //= new Dictionary<EntityType, int>();
        Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType { get; } 

        /// <summary>
        /// used for populating entity list window from side panel
        /// </summary>
        List<EntityID> ContainedEntities { get;  }

        /// <summary>
        /// simply reports the contained items... they may not be fit for sale, so also test ownership, parts, completeness...
        /// </summary>
        Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType { get; } 

        Dictionary<StorageCondition, Storage> StorageSpaces { get; }
        Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces { get; }

        Storage FindStorage(StorageID storageID);
       // StorageCompartment FindCompartment(StorageID storageID);

        bool IsTradeOfferStorage(StorageID storageID);

        float? TotalItemStorageCapacity { get; }
        float? TotalStored { get; }

        #endregion


        float? CurrentMaximumSpeed { get; }


        //float? LoadedVehicleSpeed { get; }

        float CalculateSpeed(float bulk);

        double? Condition { get; }

        float? ConditionChangeSpeed { get; }

        float? Integrity { get; }

        double? FunctionalScore { get; }

        bool FlipHorizontally { get; }

        float Bulk { get; }

        Body Body { get; }

        Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts { get; }



        float? StrengthRating { get;  }

        StanceType Stance { get; }

        bool NotOnboardDrivenVehicle { get; }


        /// <summary>
        /// even destroyed structures/entities must accept visitors!
        /// for IKnownEntityData, this will point to the Entity object... which must survive the entity's destruction.
        /// </summary>
        GatheringSite GatheringSite { get; }


     //   AgentStorage AgentStorage { get; }

        #region Locks

        /// <summary>
        /// TODO: move to SharedKnowledge EntityLocks to allow rivals / enemies to both use tools
        /// 
        /// we must be able to assign destroyed entities to preserve the illusion that the items still exist.
        /// 
        /// </summary>
        JobID? AssignedToJob 
        { 
            get; 
            set; 
        }


           
        #endregion

        /// <summary>
        /// for buildings, this will be the front entrance location.
        /// for other entities, it will probably be their Location   
        /// 
        /// Improve/generalize this?? with IExit...
        /// 
        /// yes, adding an AccessPoint property to IExit, and a convenience method to Entity to query it
        /// 
        /// Agents should always give their location here
        /// </summary>
        Vector3? AccessPoint { get; }

        PassengerOrCargoSlot GetFreeDriversSlot();

        List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad);

        List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad);

        bool HasEnoughFuel(float neededFuel);

        bool HasEnergyForDuration(float durationInDays);

        /// <summary>
        /// for camp fire etc. (IsBurning)
        /// </summary>
        bool? IsPrepared { get; }

        bool HasEnoughAmmo(EntityType ammoType, int amount);

        /// <summary>
        /// iterates its parts too, to find intrinsic weapons
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <returns></returns>
        bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload);

        /// <summary>
        /// only call on root entities.
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="action"></param>
        /// <param name="partToFix"></param>
        /// <returns></returns>
        bool NeedsRepair(); //SharedKnowledge sharedKnowledge); //, out List<EntityID> partsToFix ); //, out RepairAction? action, out EntityID? partToFix);

        RepairPackage ComputeBestRepairPackage();

        int? GetTotalAmmo();

        float GetRepairProgress(RepairAction repairAction); //, EntityID? partToFix);

        /// <summary>
        /// for Ammo type items
        /// </summary>
        int? NoOfRounds { get; }

      //  string GetName();

        string GetDisplayName();

        float BoundingRadius3D { get; }

        /// <summary>
        /// Warning: composite parent may no longer exist!
        /// </summary>
        CompositeID? PartOfID { get; }

        List<EntityID> PartIDs { get; }

        /// <summary>
        /// the closest entity that we are a part of. seems more useful than PartOfID
        /// Not the root!
        /// </summary>
        EntityID? ParentEntityID { get; }

        /// <summary>
        /// the root of the parts tree (or the entity itself)
        /// </summary>
        EntityID RootEntityID { get; }


        bool IsCompleted();

        bool? IsStarted();

       // bool IsEnclosed();

        bool IsWeatherProof();

      //  bool GetReplenishes(out Entity replenishes);

        Dictionary<FoodNutrientType, float> NutrientBulkAmounts { get;  } // = new Dictionary<FoodNutrientType, float>();


        bool CanBeHunted(Allegiances.Allegiance byAllegiance);

        /// <summary>
        /// for the player, only owned structures can have stockpile settings set - but for critters ownership does not matter.
        /// </summary>
        /// <param name="byOwner"></param>
        /// <returns></returns>
        bool CanSetStockpileSettings(EntityGroupID byOwner);

        bool CanSetTradeOfferSettings(EntityGroupID byOwner);

        bool CanBeUpgraded(EntityGroupID byOwner);

        void ChangeOwnership(IOwner newOwner, Entity.GiveNewOwnerKnowledge giveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes); //GiveNewOwnerKnowledge? giveNewOwnerKnowledge = null) 
        
        #region structures

        int? Residents { get; set; }

        float? ComfortLevel { get; }

        List<HouseholdID> Households { get; }
       
        #endregion

        bool IsUnassigned(SharedKnowledge sharedKnowledge); //  EntityGroup groupToCheck);     

        bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge); // EntityGroup groupToCheck);
    

        bool CanBeHauled(); 


        bool IsItemValidForHauling(Entity entity, Intelligence entityIntelligence, HaulingJob job); //, Entity item)

        /*
        /// <summary>
        /// will be null if the entity is represented by a MemoryFact
        /// </summary>
        Entity IsCarriedByAgentInAllegiance(Allegiance.Allegiance allegiance);
        */

      //  IKnownEntityData CarriedByAgent();

        bool IsVehicleValidForHauling(Entity entity, /*IKnownEntityData vehicle,*/ IKnownEntityData item);

       /* {
            Vehicle vehicleComponent;
            vehicle.Find(out vehicleComponent);

            if (((vehicleComponent.DrivenBy == null) || vehicleComponent.DrivenBy == entity)
                && vehicle.Storage != null
                && vehicle.Storage.ItemStorage.TotalCapacity >= item.Bulk
                && vehicle.IsCompleted()
                && ScoreIsEntityFunctional(vehicle) > 0.0) // omit broken down vehicles
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/


      /*  {
            Vector3 knownItemLocation = The.Sim.GetKnownLocation(entityIntelligence.Allegiance, item);

            Item itemComponent = item.Item;

            if (itemComponent.IsAccessible()
                && !The.Sim.IsKnownToBeDestroyed(entity, item)
                && (itemComponent.OKToTakeThisItemFromCarrier != false || entity.Storage.Contains(item)) // only consider items that we are carrying or which are OK to take from others carrying them.
                && (itemComponent.OnBoard == null || itemComponent.OnBoard.Vehicle.DrivenBy == null)
                && (item.AssignedToJob == null || item.AssignedToJob is HaulingJob)
                && (entity.Storage.ItemStorage.HasCapacityForItemWhenEmpty(item)) // NEW!!(?)
                && (job.ToStorage == null || itemComponent.StoredIn != job.ToStorage)
                && (job.ToLocation != item.Location)) //Map.MapManager.WorldPosToTile(knownItemLocation) != job.ToTilePos) // ADDED AGAIN - entrance to buildings should not be used for dumping, piling etc. if an item is stored inside a building, and the base tile of the building is next to the destination, we don't want to disregard it.
            {

                if (EstimateWorkSiteDiscomfort(entity, knownItemLocation) > 0) // item.MapPosition) > 0)
                {
                    return true;
                }
            }

            return false;

        }*/
    }
}

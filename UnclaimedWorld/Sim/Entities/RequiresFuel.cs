using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Particles;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// used in tools like ovens, furnaces, fireplaces...
    /// </summary>
    public class RequiresFuel: ISnapshot
    {

        public ReplenishItems Parent;

        /// <summary>
        /// unspent fuel - prevent other ágents from taking them again by setting the property Replenishes on the Item!
        /// burnt in sequence... progress for the item is tracked with burnProgress
        /// </summary>
      //  public List<EntityID> FuelItems = new List<EntityID>();

        Regulator fuelBurningRegulator;

       
        private bool fuelIsDirty = true;

        private float fuel;
        /// <summary>
        /// Total bulk (mass?) of remaining fuel... computed as the total bulk of fuel items minus burnprogress
        /// </summary>
        public float Fuel
        {
            get
            {
                if (fuelIsDirty)
                {
                    RecomputeFuel();
                }
                return fuel;
            }

        }

        /// <summary>
        /// for testing...
        /// </summary>
        public void SetBulkLeftOfBurningItem(float value)
        {
            bulkLeftOfCurrentlyBurningItem = value;

        }

        /// <summary>
        /// how much of the currently burning fuel item is left. when this reaches zero a fuel item will be destroyed and burnprogress will be reset to the bulk of the next fuel item
        /// we don't change the bulk of the item itself as it 'burns'...
        /// - so this will be inaccurate if the fuel items have different bulk...
        /// </summary>
        private float bulkLeftOfCurrentlyBurningItem = 0f;

        /// <summary>
        /// 0 - 1. we want the fire to grow slowly, and die down as well...
        /// </summary>
      //  public float FireProgress;


     //   FireAndSmoke FireAndSmoke;

        public RequiresFuel()
        {

        }

        public RequiresFuel(ReplenishItems parent)
        {
            this.Parent = parent;

            CreateRegulators();
        }

    /*     public int RequiredNoOfFuelItemsForJob(float duration) //ProcessType processType)
        {


           float? processTime = processType.WorkOrTimeNeeded.TimeNeeded ?? processType.WorkOrTimeNeeded.ManSecondsOfWorkNeeded ?? null;
            
            if (processTime.HasValue)
            {
                // we must have enough fuel to keep it going for the entire time of the process.

            }
            
        }*/


        public void ResetPlaySiteRegulators()
        {
            CreateRegulators();        
        }

        public bool HasFuelForDuration(float durationInDays)
        {
            return HasFuelForDuration(durationInDays, Parent.Parent.EntityType, Fuel);

         /*   if (Fuel > 0f)
            {
                float neededFuel = GetNeededFuel(parent.Parent.EntityType, duration);

                return HasEnoughFuel(neededFuel);
            }
            else return false;
            */
        }

        public static bool HasFuelForDuration(float durationInDays, EntityType entityType, float currentFuel)
        {
            if (currentFuel > 0f)
            {
                float neededFuel = entityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.GetNeededFuel(durationInDays);

                return HasEnoughFuel(neededFuel, currentFuel);
            }
            else return false;

        }

      /*  public static float GetNeededFuel(EntityType entityType, float durationInDays)
        {
            float neededFuel = (float)(entityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.BurnRatePerDay * durationInDays);
            return neededFuel;
        }*/

        public bool HasEnoughFuel(float neededFuel)
        {
            return neededFuel <= Fuel;
        }

        private static bool HasEnoughFuel(float neededFuel, float currentFuel)
        {
            return neededFuel <= currentFuel;
        }

        public void Refuel(Entity fuel)
        {
            //FuelItems.Add(fuel.ID);

            fuelIsDirty = true;

            fuel.Item.Replenishes = Parent.Parent.EntityID;

            
           // fuel.Destroy();
        }

        private void CreateRegulators()
        {
            fuelBurningRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.Constants.UpdateIntervalForEntityComponents, "RequiresFueld");

        }

        private void RecomputeFuel()
        {
            fuelIsDirty = false;

            
            //parent.IterateContained(r => bulkOfFuelItems += r.Bulk);

            List<Entity> fuelItems = new List<Entity>();
            Parent.GetContainedItemsList(null, fuelItems);

            if (fuelItems.Count > 0)
            {
                // consider one of the items 'burning' (perhaps we should make this explicit by storing its ID..?)
                Entity burningFuelItemEntity = fuelItems[0];
                float bulkOfBurningItem = burningFuelItemEntity.Bulk; // get its real bulk

                float totalBulkOfFuelItems = 0f;

                foreach (var item in fuelItems)
                {
                    totalBulkOfFuelItems += item.Bulk;
                }

                if (Common.IsGreaterThan(bulkLeftOfCurrentlyBurningItem, 0f))
                {
                    // subtract the real bulk and substitute with the simulated bulk:
                    fuel = totalBulkOfFuelItems - bulkOfBurningItem + bulkLeftOfCurrentlyBurningItem;
                }
                else
                {
                    fuel = totalBulkOfFuelItems;
                }
            }
            else
            {
                fuel = 0f;
            }
            
        }

        private bool IsBurning()
        {
            Items.Tool tool;
            if (Parent.Parent.Find(out tool) && tool.IsPrepared == true) // TODO: not just tools...
            {
                return true;
            }

            return false;
        }

        public void Update()
        {
            double deltaTimeInSeconds;
            if (fuelBurningRegulator.IsReadyGetTimeElapsedInSeconds(out deltaTimeInSeconds))
            {
                if (IsBurning()) // parent.Parent.Find(out tool) && tool.IsPrepared == true) // TODO: not just tools...
                {
                    if (Fuel > 0f)
                    {
                        //   FireAndSmoke.Size = FireProgress;

                        bulkLeftOfCurrentlyBurningItem -= (float)(deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond * Parent.Parent.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.BurnRatePerDay);

                        if (bulkLeftOfCurrentlyBurningItem <= 0f)
                        {

                            Entity fuelItemEntity = null;
                            fuelItemEntity = GetValidFuelItem();

                            if (fuelItemEntity != null)
                            {
                                
                                // only log if seen and owned:
                                Entity.LogProductionEvent(fuelItemEntity, ProductionStatistics.StatTypes.UsedAsInput, true);
           
                                fuelItemEntity.Destroy(); // burn the item
                            }

                            if (Parent.NoOfItems() > 0)
                            {
                                // start on the next item..
                                StartBurningNextFuelItem();
                            }
                        }

                        fuelIsDirty = true;
                    }
                    //   Fuel -= (float)(deltaTimeInSeconds * DateAndTime.DaysPerSeconds * parent.Parent.EntityType.RequiresEnergyType.RequiresFuelType.BurnRatePerDay);



                    if (Parent.NoOfItems() == 0)
                    {
                        Extinguish();

                        bulkLeftOfCurrentlyBurningItem = 0f;

                        //IsBurning = false;
                    }

                }
            }
        }

        /// <summary>
        /// returns a valid fuel item and cleans up the list of any destroyed items
        /// </summary>
        /// <returns></returns>
        private Entity GetValidFuelItem()
        {
            Entity fuelItemEntity = null;
           // EntityID fuelID;

            List<Entity> fuelItems = new List<Entity>();
            Parent.GetContainedItemsList(null, fuelItems);

            if (fuelItems.Count > 0)
            {
                fuelItemEntity = fuelItems[0];
            }

          /*  while (fuelItemEntity == null && FuelItems.Count > 0) // remove any degraded/destroyed fuel items
            {
                fuelID = FuelItems[0];
               
                fuelItemEntity = Entity.FindByID(fuelID);

                if (fuelItemEntity == null)
                {
                    FuelItems.RemoveAt(0);
                }
            }*/

            return fuelItemEntity;
        }

        private bool StartBurningNextFuelItem()
        {
            Entity fuelItemEntity = GetValidFuelItem();
            if (fuelItemEntity != null)
            {
                bulkLeftOfCurrentlyBurningItem = fuelItemEntity.Bulk;
                return true;
            }
            else return false;
        }

        public bool LightFire()
        {
            if (bulkLeftOfCurrentlyBurningItem <= 0f)
            {
                return StartBurningNextFuelItem();               
            }

            return true;
        }

      /*  public void LightFire()
        {
           // parent.IsStarted = true;
          //  FireProgress = 0.1f;
            
            //FireAndSmoke = The.Client.particleManager.AddFireAndSmoke(parent.Parent, FireProgress, 1f); 
            
          //  ParticleEmitter smoke = The.Client.ParticleManager.AddEmitter("smoke", parent.Parent.Renderable); //, scale, null);

           // The.Client.ParticleManager.AddEmitter("smallSmoke", parent.Parent.Renderable); 
            //parent.Parent.Renderable.AddParticleEmitter(smoke);

        }*/

        public void Extinguish()
        {
           
            Items.Tool tool;
            if (Parent.Parent.Find(out tool))
            {
                tool.IsPrepared = false;
            }

        }


        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.bulkLeftOfCurrentlyBurningItem = sn.DoFloat(bulkLeftOfCurrentlyBurningItem);
            this.fuel = sn.DoFloat(fuel);
            this.fuelIsDirty = sn.DoBool(fuelIsDirty);

            sn.Ignore(fuelBurningRegulator);
            sn.Ignore(Parent);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            CreateRegulators();
        }


        #endregion
    }
}

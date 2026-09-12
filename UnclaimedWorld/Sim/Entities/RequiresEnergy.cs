using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// generalize this into a Replenishable component
    /// with options: continuous consumption (campfire) / manual consumption (bait for fishing rod) / consumption while using (fuel cells for power tools)
    /// and either discrete / bulk consumption of replenish items, or use of some internal property (fuel cell charge/ammo)
    /// 
    /// move this to Container
    /// </summary>
    public class RequiresEnergy //: Component
    {
        public RequiresFuel RequiresFuel;

        public RequiresPower RequiresPower;


        // public Entity Parent;//moved to base


        public bool Start()
        {
            if (RequiresFuel != null)
            {
                return RequiresFuel.LightFire();
            }

            return true;
        }

      /*  public RequiresEnergy(Entity parent)
        {
            this.Parent = parent;

            if (parent.EntityType.RequiresEnergyType.RequiresFuelType != null)
            {
                RequiresFuel = new RequiresFuel(this);
            }

            if (parent.EntityType.RequiresEnergyType.RequiresPowerType != null)
            {
                RequiresPower = new RequiresPower();
            }

        }*/

     /*   public override ISnapshot DoSnapshot(Snapshot sn)
        {
            base.DoSnapshot(sn);
            sn.Do(this.RequiresFuel);
            sn.Do(this.RequiresPower);
        }

        public override Snapshot.Version DoVersion(Snapshot sn)
        {
            sn.Do(Snapshot.Version.one);
        }*/

        public bool HasEnergyForDuration(float durationInDays)
        {
            bool hasFuel = true;
            if (RequiresFuel != null)
            {
                hasFuel = RequiresFuel.HasFuelForDuration(durationInDays);
            }

            bool hasPower = true;
            if (RequiresPower != null)
            {
                hasPower = RequiresPower.HasPowerForDuration(durationInDays);
            }

            return hasFuel && hasPower;           


        }

      /*  public void UpdateSimulationInParallel(double deltaTimeInSeconds)
        {
            if (RequiresFuel != null)
            {
                RequiresFuel.Update(deltaTimeInSeconds);
            }

           
            if (RequiresPower != null)
            {
                RequiresPower.UpdateSimulationInParallel(deltaTimeInSeconds);
            }


        }*/
       
    }
}

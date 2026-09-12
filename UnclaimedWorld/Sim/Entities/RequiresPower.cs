using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// for power cells or cable power
    /// </summary>
    public class RequiresPower
    {
        public bool IsOn;

        /// <summary>
        /// Total bulk (mass?) of remaining fuel...
        /// </summary>
       // public float Energy;

        /// <summary>
        /// the rechargable power cells currently inserted
        /// </summary>
        public List<Entity> PowerCells;



        /// <summary>
        /// TODO!
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public bool HasPowerForDuration(float duration)
        {
            return true;

        }


        public void UpdateSimulationInParallel(double deltaTimeInSeconds)
        {

        }

     /*   public void Recharge(Entity fuel)
        {
            FuelItems.Add(fuel);

            Fuel += fuel.Bulk;

            // we destroy the fuel item now even though it has not been burned yet. If we didn't, we would have to make a new lock variable on the item to prevent other agents from using it.
            fuel.Destroy();
        }*/


      /*  public void Refuel(Entity fuel)
        {
            Fuel += fuel.Bulk;

            fuel.Destroy();
        }*/
    }
}

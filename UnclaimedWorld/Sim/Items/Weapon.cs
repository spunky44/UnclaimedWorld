using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.Items
{
   /* public class Weapon : ISnapshot
    {        
        double reloadProgress = 0;
        bool canShoot = true;
        private TimeSpan lastFired = new TimeSpan(0, 0, 0);


        public Weapon(WeaponType weaponType )
        {
            //setup weapon before first use 
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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.canShoot = sn.DoBool(canShoot);
            this.lastFired = sn.DoTimeSpan(lastFired); TimeSpan ts;
            this.reloadProgress = sn.DoDouble(reloadProgress);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            //lookups and other fix-ups
        }

        public Weapon()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


       

        public enum WeaponResult { TargetMiss, TargetHit, TargetDead }
        public virtual WeaponResult UseWeapon(Entity user, Entity victim, TimeSpan totalGameTime)
        {
            lastFired = totalGameTime;
            return WeaponResult.TargetMiss;  
        }

        
    }*/
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class MagazineContainerType : ContainerType
    {
      
    
        public string UsesAmmoTag;
        public string UsesAmmoTypeKeyName;

        /// <summary>
        /// MP: this number must be the SAME as MaxNoOfRounds for the ammunition item. else they will not load the weapon. (from my experience, using a sentry) MP Edit: for rifles, no such problem. they will split up the ammo item.
        /// //// LP: TODO: make a validation that no attacks spend more ammo than this number. agents go into an infinite reload loop if that is the case...
        /// </summary>
        public int MaxCapacity;

      
        /// <summary>
        /// should not include inputs. these will be generated per auto
        /// </summary>
        public string ReplenishProcess;

        [XmlIgnore]
        private ProcessType ReplenishProcessType;

        /// <summary>
        /// Same as eating/consume... could these be scrapped with optional inputs...? 
        /// </summary>
        [XmlIgnore]
        public Dictionary<EntityType, ProcessType> ReplenishProcesses;
        

        [XmlIgnore]
        public List<EntityType> AmmoEntityTypes;

        public override Container CreateContainer(Entity parent)
        {
            return new MagazineContainer(parent);
        }

        public override Dictionary<EntityType, ProcessType> GetReplenishProcesses()
        {
            return ReplenishProcesses;
        }

        public override void PostLoadContentInitialize(EntityType parent)
        {
            base.PostLoadContentInitialize(parent);

            if (!string.IsNullOrEmpty(UsesAmmoTag))
            {
                AmmoEntityTypes = GameData.Instance.AmmoByTag[UsesAmmoTag];
            }

            if (!string.IsNullOrEmpty(UsesAmmoTypeKeyName))
            {
                EntityType ammoType = GameData.Instance.AllEntityTypes[UsesAmmoTypeKeyName];

                if (AmmoEntityTypes == null)
                {
                    AmmoEntityTypes = new List<EntityType>();
                }

                if (!AmmoEntityTypes.Contains(ammoType))
                {
                    AmmoEntityTypes.Add(ammoType);
                }
            }

            if (!string.IsNullOrEmpty(ReplenishProcess))
            {
                ReplenishProcessType = GameData.Instance.AllProcessTypes[ReplenishProcess];
                ReplenishProcessType.IsReplenishProcess = true;
                ReplenishProcessType.ReplenishAction = AI.Goals.GoalReplenish.ReplenishAction.Reload; // important to show where the outputs shpould be placed

                // iterate over possible items, create a process for each
                foreach (var item in AmmoEntityTypes)
                {
                    RequiresReplenishType.InitReplenishProcess(parent, ReplenishProcessType, item, ref ReplenishProcesses);
                }
            }

        }

        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            if (CanBeEnteredByTags != null)
            {
                EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags should not be specified because this container type has no doors and cannot be entered.");

            }
        }
      

    }
}

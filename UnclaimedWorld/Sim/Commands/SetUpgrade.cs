using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Commands
{
    public class SetUpgrade : Control.Commands.Command
    {
        /// <summary>
        /// the owner of the Job
        /// </summary>
        public long EntityGroup;

        /// <summary>
        /// the entity to perform the action on
        /// </summary>
        public long EntityID;

        /// <summary>
        /// for SharedKnowledge lookup of entity
        /// </summary>
        public long AllegianceID;

        public string UpgradeCategory;

        /// <summary>
        /// set this to null to clear the upgrade order
        /// </summary>
        public string UpgradeEntityType;

       // public bool Value;

        public bool GiveClientFeedback;

        public Priority? Priority;

        public SetUpgrade()
        { 
        }

        public SetUpgrade(EntityID entityID, AllegianceID allegianceID, EntityGroupID entityGroupID, bool giveClientFeedback, UpgradeCategory category, string upgradeTypeKey) // EntityType upgradeType) //,   string processTypeKey) //, Priority priority = Priority.Normal)
        {
            this.EntityID = (long)entityID;
            this.EntityGroup = (long)entityGroupID;
            this.AllegianceID = (long)allegianceID;
            this.GiveClientFeedback = giveClientFeedback;
            this.UpgradeCategory = category.KeyName;
            this.UpgradeEntityType = upgradeTypeKey; // upgradeType.KeyName;
          
           // this.ProcessType = processTypeKey;
          
        }

        public override void Execute(bool giveClientFeedback)
        {
            
            bool success = DoSetUpgrade();
            

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (success)
                {
                    The.Client.OnSpecialAction();
                }
            }   
        }

        private bool DoSetUpgrade()
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);

            EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup);

            EntityType upgradeType = null; // null means 'No upgrade'
            if (UpgradeEntityType != null)
            {
                upgradeType = GameData.Instance.AllEntityTypes[UpgradeEntityType];
            }


            entityGroup.SetUpgrade((EntityID)EntityID, GameData.Instance.AllUpgradeCategories[UpgradeCategory], upgradeType);
                   

            return true;
        }

       
        
    }
}

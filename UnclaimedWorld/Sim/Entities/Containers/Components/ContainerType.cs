using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using UWGame.SimSide.Processes;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers.Components
{
    [XmlInclude(typeof(TerminalContainerType))]
    [XmlInclude(typeof(VehicleContainerType))]
    [XmlInclude(typeof(AgentStorageType))]
    [XmlInclude(typeof(HomeContainerType))]
    [XmlInclude(typeof(ReplenishContainerType))]
    [XmlInclude(typeof(WorkshopContainerType))]
    [XmlInclude(typeof(ToolContainerType))]
    [XmlInclude(typeof(StorageContainerType))]
    public abstract class ContainerType
    {
        /// <summary>
        /// who can retrieve or deposit contents - is assumed if the agent can enter 
        /// </summary>
        public string[] CanTransactWithTags;
        /// <summary>
        /// A bitarray from the string tags to quicker match CanTransactWithDesignerTags
        /// </summary>
        [XmlIgnore]
        private BitArray CanTransactWith;


        /// <summary>
        /// who can enter - only relevant for agents
        /// </summary>
        public string[] CanBeEnteredByTags;
        /// <summary>
        /// A bitarray from the string tags to quicker match CanBeContainedByDesignerTags
        /// </summary>
        [XmlIgnore]
        private BitArray CanBeEnteredBy;


        /// <summary>
        /// referenced by Item to determine what can be stored here.
        /// </summary>
        public string[] StorageTags;

       


        /// <summary>
        /// i decided to make this separate from HomeContainer because I want to combine it with VehicleContainer as a mobile home too...
        /// then it would be better to make an interface 
        /// </summary>
        public ResidenceType ResidenceType;


        [XmlIgnore]
        public bool VerminCanAccess;

        /// <summary>
        /// this object can be located in different places - let's make this the single access point!
        ///      
        /// </summary>
        public virtual RequiresReplenishType GetRequiresReplenishType()
        {
            return null;
        }


        public abstract Container CreateContainer(Entity parent);

        public virtual void Initialize()
        {            
            if (ResidenceType != null)
            {
                ResidenceType.Initialize();
            }
            
         /*   if (MagazineContainerType != null)
            {
                MagazineContainerType.Initialize();
            }*/
         }


        public virtual void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            if (HasOutputStorage)
            {
                // validate that parent is a tool!
                if (parent.ToolType == null)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Only tools can have production output. Either make the type a tool by defining EntityType.ToolType, or change the container type.");
                }
            }
            
        }

        public virtual void PostDataCompleteInitialize(EntityType parent)
        {
            if (GetUpgradeOptions() != null)
            {                
                foreach (var item in GetUpgradeOptions()) // UpgradesProfileFinal.UpgradeCategoriesFinal)
	            {
                    Common.AddToMultiList(GameData.Instance.EntityTypesToUpgradeByUpgradeCategory, item, parent);
	            }
            } 


        }

        public virtual void PostLoadContentInitialize(EntityType parent)
        {
            
            string[] combinedArray = CanTransactWithTags;

            //If we only have one array we do not need to combine them. As we want to use the combined set of tags.
            if (CanTransactWithTags == null)
            {
                combinedArray = CanBeEnteredByTags;
            }
            else if (CanBeEnteredByTags != null) // why not put a comment here that explains this code..? //Added a few comments that explains what this code does.
            {
                //Here we copy both the arrays to the same combined array so we use the flags from both.
                int array1OriginalLength = combinedArray.Length;
                Array.Resize<string>(ref combinedArray, array1OriginalLength + CanBeEnteredByTags.Length);
                Array.Copy(CanBeEnteredByTags, 0, combinedArray, array1OriginalLength, CanBeEnteredByTags.Length);
                
                //Here we make sure so that only one of each element is saved in the array.
                combinedArray = combinedArray.Distinct().ToArray();
            }

            CanTransactWith = GameData.CreateBitArrayFromTags(GameData.Instance.ContainerTags, combinedArray);
            CanBeEnteredBy = GameData.CreateBitArrayFromTags(GameData.Instance.ContainerTags, CanBeEnteredByTags);

           /* if (parent.KeyName == "structure:wigwamSpoakShingles")
            {

            }

            if (parent.KeyName == "structure:clayGranary")
            {

            }*/
            
           // SetVerminCanAccess();

        }

        public void SetVerminCanAccess()
        {           
            foreach (var verminType in GameData.Instance.AllVerminTypes) // see if ANY vermin can access
            {
                if (CanTransactWithContainer(verminType.Value))
                {
                    VerminCanAccess = true;
                    return;
                }
            }
        }



        /// <summary>
        /// can the agent add/remove things from the container?
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public bool CanTransactWithContainer(EntityType agentType)  
        {
            if (agentType.IntelligenceType != null
                && agentType.IntelligenceType.ContainerTransactValue.HasValue)
            {
                
                return CanTransactWith[agentType.IntelligenceType.ContainerTransactValue.Value];

            }
            else
                return false;
        }

        /// <summary>
        /// can the agent enter this container? (if yes, then he can also transact with items in it)
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public bool AllowedInContainer(EntityType agentType)  
        {
            if (agentType.IntelligenceType != null
                && agentType.IntelligenceType.ContainerTransactValue.HasValue)
            {

                return CanBeEnteredBy[agentType.IntelligenceType.ContainerTransactValue.Value];

            }
            else
                return false;
        }

        public virtual DefaultStorageSettings GetDefaultStorageSettings()
        {           
            return null;
        }

        public virtual float? FullStatePercentage
        {
            get
            {           
                return null;
            }
        }

        public virtual float? HalfFullStatePercentage
        {
            get
            {               
                return null;
            }
        }

        public virtual bool HasOutputStorage
        {
            get
            {
               return false;
            }
        }

        public virtual bool CanBeUpgraded
        {
            get
            {
                return false;
            }
        }

        public virtual List<UpgradeCategory> /*Dictionary<UpgradeCategory, List<EntityType>>*/ GetUpgradeOptions()
        {
            return null;
        }

        public virtual Dictionary<EntityType, ProcessType> GetReplenishProcesses()
        {          
            return null;
        }
        
        public virtual float GetOutputStorageCapacity()
        {           
            return 0f;
        }

        /// <summary>
        /// IExit
        /// </summary>
        /// <returns></returns>
        public virtual Vector2[] GetDoors()
        {
            return null;
        }

        /// <summary>
        /// IExit
        /// </summary>
        /// <returns></returns>
        public virtual bool GetHasCourtyard()
        {
            return false;
        }


        public int GetCapacityForIdlingPeople()
        {
            if (ResidenceType != null) //HomeContainerType != null)
            {
                return ResidenceType.PeopleCapacity;
            }
            /*else if (VehicleContainerType != null)
            {
                return VehicleContainerType.PeopleCapacity;
            }*/

            return 0;
        }
    }
}

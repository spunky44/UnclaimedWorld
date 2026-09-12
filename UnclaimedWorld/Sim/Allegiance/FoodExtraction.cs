using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances
{
    /*public enum FoodExtractionID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }*/


    /// <summary>
    /// manages the food abilities of a group's members. the result determines which items are categorized as 'Food' by the group.
    /// </summary>
    public class FoodExtraction: ISnapshot
    {
        /// <summary>
        /// the superset of extraction processes by the allegiance's members - this is how we manage members with different needs, like infants, adults etc. Or even different species.
        /// </summary>
        private Dictionary<EntityType, HashSet<ProcessType>> FoodExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>();   //  public Dictionary<EntityType, List<ProcessType>> FoodExtractionProcesses;          
        private Dictionary<EntityType, ProcessType> ConsumeProcesses = new Dictionary<EntityType, ProcessType>();

        private Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable = new Dictionary<EntityType, HashSet<ProcessType>>();


        bool extractionProcessesAreDirty = true;


        /// <summary>
        /// we need this reference to gather information from all members about their food abilities
        /// </summary>
        private ICanIterateEntities parent;
        private CanIterateEntitiesID parentID;

     //   public delegate void FoodProcessesChangedHandler();
       // public event FoodProcessesChangedHandler FoodProcessesChanged;

        /// <summary>
        /// we will use this pointer to notify our parent about changes in food classification
        /// </summary>
        private EntityGroupID? foodItemsGroup;

        public FoodExtraction()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }


        public FoodExtraction(ICanIterateEntities parent, EntityGroupID? foodItemsGroup, bool includeNonIndependentMembers = true)
        {
            this.parent = parent;
            this.parentID = parent.ID;
            this.IncludeNonIndependentMembers = includeNonIndependentMembers;

            this.foodItemsGroup = foodItemsGroup;

        }

        /// <summary>
        /// calling this will trigger recomputing of the processes superset.
        /// In turn it will notify the entity group
        /// </summary>
        public void SetIsDirty()
        {
            extractionProcessesAreDirty = true;
        }

        public bool IncludeNonIndependentMembers = true;
       
      //  private bool? extractionProcessesChanged = null;

        /// <summary>
        /// makes sure that the superset of the members' eating processes is up to date.
        /// 
        /// returns true if there are changes
        /// </summary>
        private void UpdateEatingProcessesByMembers()
        {            
            // store/clone the old set. then clear them and start adding from members. After that, compare the two sets to see if they were changed...
            // this is costly...
            extractionProcessesAreDirty = false;

            // clone the old values:
            Dictionary<EntityType, ProcessType> oldConsumeProcesses = new Dictionary<EntityType, ProcessType>(ConsumeProcesses);
            Dictionary<EntityType, HashSet<ProcessType>> oldFoodExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>(); //FoodExtractionProcesses);
            foreach (var item in FoodExtractionProcesses)
            {
                foreach (var processType in item.Value)
	            {
                    Common.AddToMultiList(oldFoodExtractionProcesses, item.Key, processType);
	            }                
            }

           // Dictionary<EntityType, HashSet<ProcessType>> extractionResultsInConsumable = new Dictionary<EntityType, HashSet<ProcessType>>(ExtractionResultsInConsumable);

            FoodExtractionProcesses.Clear();
            ConsumeProcesses.Clear();
            ExtractionResultsInConsumable.Clear();
            
            parent.IterateMembers(GetFoodProcesses);

            bool hasChanged = FoodProcessesHaveChanged(FoodExtractionProcesses, oldFoodExtractionProcesses, 
                                            ConsumeProcesses, oldConsumeProcesses);

            if (hasChanged)
            {
                if (this.foodItemsGroup.HasValue)
                {
                    EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(foodItemsGroup.Value);
                    if (entityGroup != null)
                    {
                        entityGroup.SetFoodDirty();
                    }
                }

               /* if (FoodProcessesChanged != null)
                    FoodProcessesChanged.Invoke();*/
            }
        }

        /// <summary>
        /// compare old and new collections to see if they changed. This is probably a bit costly...
        /// </summary>
        /// <returns></returns>
        public static bool FoodProcessesHaveChanged(Dictionary<EntityType, HashSet<ProcessType>> newFoodExtractionProcesses,
                                                    Dictionary<EntityType, HashSet<ProcessType>> oldFoodExtractionProcesses,
                                                    Dictionary<EntityType, ProcessType> newConsumeProcesses,
                                                    Dictionary<EntityType, ProcessType> oldConsumeProcesses)
        {           
            // no need to compare ExtractionResultsInConsumable since it is derived from the other two collections.

            HashSet<ProcessType> oldProcessTypes;
            HashSet<ProcessType> newProcessTypes;
            int noOfOldProcesses, noOfNewProcesses;
            foreach (var item in oldFoodExtractionProcesses)
            {
                oldProcessTypes = item.Value;
                newFoodExtractionProcesses.TryGetValue(item.Key, out newProcessTypes);
               
                // first compare ordinals:
                if (oldProcessTypes != null)
                {
                    noOfOldProcesses = oldProcessTypes.Count;
                }
                else
                {
                    noOfOldProcesses = 0;
                }

                if (newProcessTypes != null)
                {
                    noOfNewProcesses = newProcessTypes.Count;
                }
                else
                {
                    noOfNewProcesses = 0;
                }

                if (noOfOldProcesses != noOfNewProcesses)
                    return true;

                // next, compare each member:
                foreach (var processType in oldProcessTypes)
                {
                    if (!newProcessTypes.Contains(processType))
                    {
                        return true;
                    }
                }               

            }

            // now compare the other collections...
            if (oldConsumeProcesses.Count != newConsumeProcesses.Count)
                return true;

            foreach (var item in oldConsumeProcesses)
            {
                ProcessType newProcessType;
                if (!newConsumeProcesses.TryGetValue(item.Key, out newProcessType))
                {
                    return true;
                }

                if (newProcessType != item.Value)
                    return true;

            }

            return false;

        }

        private void GetFoodProcesses(Entity member)
        {
            if (member.EntityType.BiologicalType != null
                && (IncludeNonIndependentMembers == true || member.Intelligence.IsIndependent()))
            {
                BiologicalEntity bioEntity;
                member.Find(out bioEntity);
                if (bioEntity.FoodExtractionProcesses != null)
                {
                    foreach (var item in bioEntity.FoodExtractionProcesses)
                    {
                        Common.AddToMultiList(FoodExtractionProcesses,
                            item.Key, item.Value);
                    }
                }

                if (bioEntity.ConsumeProcesses != null)
                {
                    foreach (var item in bioEntity.ConsumeProcesses)
                    {
                        Common.AddToDictionary(ref ConsumeProcesses,
                            item.Key, item.Value);
                        
                    }
                }

                if (bioEntity.ExtractionResultsInConsumable != null)
                {
                    foreach (var item in bioEntity.ExtractionResultsInConsumable)
                    {
                        Common.AddToDictionary(ref ExtractionResultsInConsumable,
                            item.Key, item.Value);
                       
                    }
                }
            }
        }


        public bool IsEatable(EntityType food)
        {
            if (extractionProcessesAreDirty)
            {
                UpdateEatingProcessesByMembers();
            }

            return food.IsEatable(ExtractionResultsInConsumable, ConsumeProcesses);
        }


        public void Destroy()
        {
            //RemoveIDEntry();
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
            //IDCounter = (FoodExtractionID)sn.DoEnum(IDCounter);
            //id = SnapshotID(sn, id);

            this.ConsumeProcesses = sn.DoDictionary(ConsumeProcesses);
            this.extractionProcessesAreDirty = sn.DoBool(extractionProcessesAreDirty);
            this.ExtractionResultsInConsumable = sn.DoMultiMapHashSet(ExtractionResultsInConsumable);
            this.FoodExtractionProcesses = sn.DoMultiMapHashSet(FoodExtractionProcesses); 
            this.parentID = (CanIterateEntitiesID)sn.DoEnum(parentID);
            this.foodItemsGroup = sn.DoEnumNullable(foodItemsGroup);
            this.IncludeNonIndependentMembers = sn.DoBool(IncludeNonIndependentMembers);

            sn.Ignore(parent);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            parent = LookUpICanIterateEntities.FindByID(parentID); // uses special class!
            

        }

        #endregion

     /*   #region ILookup

        private static FoodExtractionID IDCounter;

        private FoodExtractionID id = FoodExtractionID.Invalid;

        public FoodExtractionID ID
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

        public FoodExtractionID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= FoodExtractionID.Max)
            {
                throw new Exception("Astounding, FoodExtractionID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public FoodExtractionID SnapshotID(Snapshotter sn, FoodExtractionID id)
        {
            return (FoodExtractionID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != FoodExtractionID.Invalid)
                LookUp<FoodExtraction, FoodExtractionID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = FoodExtractionID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<FoodExtraction, FoodExtractionID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<FoodExtraction, FoodExtractionID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = FoodExtractionID.First;
        }

        #endregion*/
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Items
{
    /// <summary>
    /// compute values for DegradeTypes * StorageConditions, with different parts flags (if the entity can be a part)
    /// select the most important ones
    /// store the combos in DegradeType
    /// </summary>
    public class StorageDuration
    {
      //  public string DisplayName;

        public StorageCondition StorageCondition;

        public DegradeType DegradeType;

        /*
        /// <summary>
        /// is true if a part of an entity that gives this property to its parts...
        /// </summary>
        public bool IsWeatherProof;

        
        public bool DisplayAlways;

        public float Priority;
        */

        public StorageDurationToDisplay StorageDurationToDisplay;

        public float Duration;


        public float SortOrder;


        public void ComputeSortOrder()
        {
            if (StorageDurationToDisplay.DisplayAlways)
            {
                SortOrder = float.MaxValue;
            }
            else
            {
                if (StorageDurationToDisplay.SortAtTop)
                {
                    SortOrder = 10000f * Duration;
                }
                else
                {
                    SortOrder = Duration;
                }
            }
        }
    }

    public class StorageDurationToDisplay
    {
        public string DisplayName;
        public string Tooltip;

        public string StorageCondition;

        public bool IsStorageOfWeatherProofPart;
        
       // public bool FoodItemsOnly;

        public bool DisplayAlways;
        public bool DisplayForStructure;
        public bool DisplayForFoodOnly;

        public bool SortAtTop;

      //  public float Priority;

        public bool DisplayThis(EntityType entityType)
        {
            if (DisplayAlways)
            {
                return true;
            }

            if (entityType.StructureType != null && DisplayForStructure == false)
            {
                return false;
            }

            if ((entityType.ItemType == null || entityType.ItemType.FoodType == null) && DisplayForFoodOnly == true)
            {
                return false;
            }

            if (entityType.NonLivingType.CanBeAPart == false && IsStorageOfWeatherProofPart)
            {
                return false;
            }

            return true;
        }

    }

}

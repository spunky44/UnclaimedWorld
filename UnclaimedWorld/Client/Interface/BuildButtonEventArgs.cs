using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface
{
    public class BuildButtonEventArgs : EventArgs
    {
        public BuildButtonEventArgs(StructureType buildingType)
        {
            this.StructureType = buildingType;
        }


        public StructureType StructureType;

      
    }

    public class ItemButtonEventArgs : EventArgs
    {
        public ItemButtonEventArgs(object item)
        {
            this.Item = item;
        }


        public object Item;


    }

    public class ItemTypeButtonEventArgs : EventArgs
    {
        public ItemTypeButtonEventArgs(EntityType item)
        {
            this.Item = item;
        }

        public EntityType Item;
    }

    public class ProductionTargetEventArgs : EventArgs
    {
        public EntityType Item;
        public int? OutputBatchAmount;

        public ProductionTargetEventArgs(EntityType item, int? outputBatchAmount)
        {
            this.Item = item;
            this.OutputBatchAmount = outputBatchAmount;
        }

       
    }

    public class ResourceTypeButtonEventArgs : EventArgs
    {
        public ResourceTypeButtonEventArgs(ResourceType item)
        {
            this.Item = item;
        }

        public ResourceType Item;
    }

    public class ActionButtonEventArgs : EventArgs
    {
        public ActionButtonEventArgs(ProcessType item)
        {
            this.ProcessType = item;
        }


        public ProcessType ProcessType;


    }

    public class DataTypeButtonEventArgs : EventArgs
    {
        public DataTypeButtonEventArgs(EntityGroupID? owner, bool useUIOwner)
        {
           // this.Item = item;
            this.Owner = owner;
            this.UseUIOwner = useUIOwner;
        }


       // public EntityType Item;
        public EntityGroupID? Owner;
        public bool UseUIOwner;

    }

    public class ItemCategoryButtonEventArgs : EventArgs
    {
        public ItemCategoryButtonEventArgs(EntityCategory item)
        {
            this.Category = item;
        }
        
        public EntityCategory Category;
    }

   /* public class StructureCategoryButtonEventArgs : EventArgs
    {
        public StructureCategoryButtonEventArgs(StructureCategory item)
        {
            this.Category = item;
        }

        public StructureCategory Category;
    }*/

    public class EntityButtonEventArgs : EventArgs
    {
        public EntityButtonEventArgs(EntityID entityID)
        {
            this.Entity = entityID;
        }

        public EntityID Entity;
    }

    public class IGameDataButtonEventArgs : EventArgs
    {
        public IGameDataButtonEventArgs(/*IGameData*/ object item)
        {
            this.Item = item;
        }


       // public IGameData Item;
        public object Item;


    }
}

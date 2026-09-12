using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Entities.Containers
{

    /// <summary>
    /// implementing this REQUIRES storage! Does not have to be "permanent" storage.
    /// </summary>
    public interface IStorage
    {

        Storage GetStoredIn(Entity entity);


        float TotalItemStorageCapacity { get; }
        float TotalStored { get; }


        Dictionary<StorageCondition, Storage> GetStorageSpaces();

        StorageCompartment GetCompartment(StorageID storageID);

        Storage FindStorage(StorageID storageID);

       
    }
}

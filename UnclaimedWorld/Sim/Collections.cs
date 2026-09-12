using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{
    /// <summary>
    /// we subclass this dictionary to handle the elements individually
    /// </summary>
    public class Collections : Dictionary<Type, ILookUpCollectible>, ISnapshot
    {
       
        /// <summary>
        /// we snapshot all elements in order to get to the static instances by their type...
        /// the indiviudal ILookUpCollectible classes can still decide if they snapshot their members or not.
        /// </summary>
        Dictionary<Type, ILookUpCollectible> snapshotCollection = new Dictionary<Type,ILookUpCollectible>();

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            if (sn.mode != Snapshotter.Mode.Load)
            {           
                foreach (var item in this)
	            {
                   /* if (item.Value.PerformSnapshot)
                    {*/
                        snapshotCollection.Add(item.Key, item.Value);
                    //} 
                }               
            }

            snapshotCollection = sn.DoDictionary(snapshotCollection);

         
            return this;

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);



            foreach (var item in snapshotCollection)
            {
                this.Add(item.Key, item.Value);                
            }
            snapshotCollection.Clear();
          

            // if a class does LookUp FindByID on a collection which has not been used previously, the static generic type instance will be created and added to this collection,
            // while we are iterating, this will throw an exception.
            // But in that case the collection will not contain any items so it will be OK to skip over it, so let's do that by copying the pointers first.
            List<ILookUpCollectible> listCopy = Values.ToList();

            // sort:
            // collections should probably snapshot their sort order
            var sortedList = listCopy.OrderBy(l => l.LoadPostProcessOrder);

            int i = 0;
            foreach (var item in sortedList) // Values.ToList())
            {
                /*if (item.PerformSnapshot)
                {*/
                    item.LoadPostProcess(sn);

                    i++;
               // }
            }

        }


        #endregion
    }
}

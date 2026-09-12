using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{
    /// <summary>
    /// this class is just a list that will notify listeners when items are added or removed from it.
    /// can we get rid of this and replace it with SleepyUpdater?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableList<T> : ISnapshot
    {       
       /* public delegate void ListMemberRemovedHandler(object sender, int indexOfRemovedMember);
        public event ListMemberRemovedHandler ListMemberRemoved;

        public delegate void ListMemberAddedHandler(object sender);
        public event ListMemberAddedHandler ListMemberAdded;
        */

        private List<T> list = new List<T>();

        public IDActionEvent<int> ListMemberRemoved = new IDActionEvent<int>();
        public IDActionEvent ListMemberAdded = new IDActionEvent();


        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public T this[int key]
        {
            get
            {
                return list[key];
            }
            set
            {
                list[key] = value;
            }
        }

        public List<T> GetAsList()
        {
            return list;
        }



        public bool Remove(T objectToRemove)
        {            

            if (ListMemberRemoved != null)
            {
                int indexOfRemovedItem = list.IndexOf(objectToRemove);

                Debug.Assert(indexOfRemovedItem >= 0);
               
                if (indexOfRemovedItem >= 0)
                {
                    list.RemoveAt(indexOfRemovedItem);

                    ListMemberRemoved.Invoke(indexOfRemovedItem);
                }
               

                return true;
            }
            else
            {
                return list.Remove(objectToRemove);
            }

        }


        public void Add(T objectToRemove)
        {
            list.Add(objectToRemove);

            if (ListMemberAdded != null)
            {
                ListMemberAdded.Invoke(); //this);               
            }         
        }

        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            list.AddRange(itemsToAdd);

            if (ListMemberAdded != null)
            {
                ListMemberAdded.Invoke(); // (this);

            }
        }

        public static void UpdateCounterWhenItemIsRemoved(ref int itemCounter, int indexOfRemovedItem)
        {
            if (itemCounter > 0 && indexOfRemovedItem <= itemCounter)
            {   // make sure we don't skip any items!
                itemCounter--;
            }
        }


        public void Clear()
        {
            list.Clear();
        }

        #region ISnapshot
        
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            list = sn.DoList(list);
            this.ListMemberAdded = (IDActionEvent)sn.DoISnapshot(ListMemberAdded);
            this.ListMemberRemoved = (IDActionEvent<int>)sn.DoISnapshot(ListMemberRemoved);
                       

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            ListMemberAdded.LoadPostProcess(sn);
            ListMemberRemoved.LoadPostProcess(sn);

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

#endregion
    }
}

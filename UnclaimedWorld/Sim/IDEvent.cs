using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{

    public interface IIDEventSubscriber
    {
        /// <summary>
        /// this method is just a reminder that the event subscriber must register the method post-load, using the same MethodID
        /// </summary>
        void LoadPostProcessRegisterMethodIDs();

    }

    /// <summary>
    /// a replacement for the C# event that can be snapshotted (saved/loaded). 
    /// To be used in Sim. Client can still use the ordinary event.
    /// 
    /// snapshot procedure:
    /// The subscriber must snapshot the MethodID that it registrs with.
    /// The subscriber must, in LoadPostProcess, re-register this method under the same MethodID
    /// 
    /// </summary>
    /// <typeparam name="Id"></typeparam>
    public class IDActionEvent<T> : ISnapshot 
    {
      
        public Dictionary<MethodID, MethodID> Subscribers = new Dictionary<MethodID,MethodID>();

        private List<MethodID> subscriberList = new List<MethodID>(); // iteration list


        /// <summary>
        /// Registers the method and assigns it an ID. Also subscribes to this event.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public void AddAndRegister(Action<T> method, IIDEventSubscriber client, out MethodID methodID)
        {
            methodID = ActionLookup<T>.AddWithNewID(method);

            Add(methodID, client);
        }

        public void AddAndRegister(Action<T> method, IIDEventSubscriber client, out MethodID? outMethodID)
        {
            MethodID methodID;
            
            AddAndRegister(method, client, out methodID);

            outMethodID = methodID;
        }


        public void Add(MethodID eventHandlerId, IIDEventSubscriber client) 
        {
            if (!Subscribers.ContainsKey(eventHandlerId))
            {
                Subscribers.Add(eventHandlerId, eventHandlerId);
                subscriberList.Add(eventHandlerId);
            }
           
        }

        public void Remove(MethodID eventHandlerId)
        {
            if (Subscribers.ContainsKey(eventHandlerId))
            {
                Subscribers.Remove(eventHandlerId);
                subscriberList.Remove(eventHandlerId);
            }
        }


        public void Invoke(T invokeArgument)
        {
            // it should be possible for subscribers to de-register inside the loop!
            for (int i = subscriberList.Count - 1; i >= 0; i--) // go in reverse
            {
                MethodID subscriber = subscriberList[i];

                Action<T> subscriberHandler = ActionLookup<T>.FindByID(subscriber);

                if (subscriberHandler != null)
                {
                    subscriberHandler.Invoke(invokeArgument);
                }
                else
                {
                    // cleanup invalid handler
                    Remove(subscriber);

                   /* Subscribers.Remove(subscriber);
                    subscriberList.RemoveAt(i);*/
                }
            }
            
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
#if !RELEASE
            if ((Subscribers != null && Subscribers.Count > 100)
                || (subscriberList != null && subscriberList.Count > 100))
            {
                throw new Exception("mem leak?");
            }
#endif

            Subscribers = sn.DoDictionary(Subscribers);
            subscriberList = sn.DoList(subscriberList);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // now the subscribers should be told to re-register their method ids...
            // however, there is no way to do that...
            // with the ISubscriber interface, we can only force an implementation of HookUpMethodIDs...

        }

        #endregion

    }


    /// <summary>
    /// a similar event class, but without invocation arguments
    /// </summary>
    public class IDActionEvent : ISnapshot
    {       
        public Dictionary<MethodID, MethodID> Subscribers = new Dictionary<MethodID, MethodID>();

        private List<MethodID> subscriberList = new List<MethodID>(); // iteration list
               

        /// <summary>
        /// Registers the method and assigns it an ID. Also subscribes to this event.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public void AddAndRegister(Action method, IIDEventSubscriber client, out MethodID methodID)
        {
            methodID = ActionLookup.AddWithNewID(method);

            Add(methodID, client);
        }

        public void AddAndRegister(Action method, IIDEventSubscriber client, out MethodID? outMethodID)
        {
            MethodID methodID;

            AddAndRegister(method, client, out methodID);

            outMethodID = methodID;
        }


        public void Add(MethodID eventHandlerId, IIDEventSubscriber client) // Delegate eventHandler)
        {
            if (!Subscribers.ContainsKey(eventHandlerId))
            {
                Subscribers.Add(eventHandlerId, eventHandlerId);
                subscriberList.Add(eventHandlerId);
            }            
        }

        public void Remove(MethodID eventHandlerId)
        {
            if (Subscribers.ContainsKey(eventHandlerId))
            {
                Subscribers.Remove(eventHandlerId);
                subscriberList.Remove(eventHandlerId);
            }
        }


        public void Invoke()
        {
            // it should be possible for subscribers to de-register inside the loop!
            for (int i = subscriberList.Count - 1; i >= 0; i--) // go in reverse
            {
                MethodID subscriber = subscriberList[i];

                Action subscriberHandler = ActionLookup.FindByID(subscriber);
                
                if (subscriberHandler != null)
                {
                    subscriberHandler.Invoke();
                }
                else
                {
                    // cleanup invalid handler
                    Remove(subscriber);
                }
            }

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
            Subscribers = sn.DoDictionary(Subscribers);
            subscriberList = sn.DoList(subscriberList);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // now the subscribers should be told to re-register their method ids...
            // however, there is no way to do that...
            // with the ISubscriber interface, we can only force an implementation of HookUpMethodIDs...

        }

        #endregion

    }



    /// <summary>
    /// Static class that contains the MethodID counter
    /// </summary>
    public class MethodCounter
    {
        static MethodID IDCounter = MethodID.First;

        public static MethodID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MethodID.Max)
            {
                throw new Exception("Astounding, MethodID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (MethodID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = MethodID.First;
        }

    }
}

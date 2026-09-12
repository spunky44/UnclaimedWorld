using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Snapshots
{
    //   ___ ____                        _           _   
    //  |_ _/ ___| _ __   __ _ _ __  ___| |__   ___ | |_ 
    //   | |\___ \| '_ \ / _` | '_ \/ __| '_ \ / _ \| __|
    //   | | ___) | | | | (_| | |_) \__ \ | | | (_) | |_ 
    //  |___|____/|_| |_|\__,_| .__/|___/_| |_|\___/ \__|
    //                        |_|                        
    public interface ISnapshot
    {
        /// <summary>
        /// Never call this directly, only Snapshotter may do that.
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        ISnapshot DoSnapshot(Snapshotter sn);

        /// <summary>
        /// the version is a separate method, to ensure that it is implemented by every ISnapshot class.
        /// all implementations should be the same.
        /// 
        /// implementors nust not call this!! Snapshotter will handle it.
        /// 
        /// For class hierarchies/inheritance, make sure that every class calls DoVersion and keeps track of its own version number in a private field.
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        Snapshotter.Version DoVersion(Snapshotter sn);

        /// <summary>
        /// here, reconnection of IDs and objects and other repair work after load should take place.
        /// Holders of objects should call this method directly in their own LoadPostProcess
        /// </summary>
        /// <param name="sn"></param>
        void LoadPostProcess(Snapshotter sn);

        /// <summary>
        /// used by Snapshot class to check that the same instance is not saved more than once.
        /// </summary>
        bool IsSnapshotted { get; set; }

    }
}

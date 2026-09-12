using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Interface;
using GameStateManagement;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Entities;
using UWGame.ClientSide;
using UWGame.ClientSide.Screens;
using UWGame.Control.Replays;
using UWGame.SimSide.Expeditions;



namespace UWGame//.Sim //TODO Refactor,move this into Sim root namespace 
{
    /// <summary>
    /// 'The' is a repository for all Singleton system classes used throughout the game
    ///  Design of "The" class is for the members of "The" to be reliably non-null. INterfaces of these global classes should always be accessible.
    ///   Exception is during startup, while "The" classes are being constructed.
    ///   
    /// when running headless, there is still The.Client, but it is an instance of another class with client interfaces... but implemented to just swallow messages and such... designed to make the Sim not feel the difference.
    ///   Also, there might be  a special versions of CLient for deeper analysis, recording, playback.... a special version that has no display, or minimal display. etc... The.Client would need to be a parent or an interface.
    ///    the alternative is undesirable --- to have If(The.Client != null) checks peppered throughout SimSide code... yuck.  
    ///   
    /// </summary>
    public static class The
    {
        //NOTE: please don't spam this class with globals... bad programming practice... data hiding, encapsulation and all that.

        #region Sim globals - these are all managed from Sim
    
        public static Sim Sim;

        public static MapManager Map;
        public static CollisionManager<Entity> CollisionManager;

       
        /// <summary>
        /// as the name says, should only contain Agents!
        /// </summary>
        public static PointQuadTree<Entity> AgentQuadTree;

        #endregion

        #region Client globals - these objects are all managed from The.Client

        /// <summary>
        /// DECOUPLE!!
        /// in headless mode, this will be a stub class... never get a property from Sim!
        /// </summary>
        public static UWGame.ClientSide.Client Client;
      
        /// <summary>
        /// DECOUPLE!!
        /// in headless mode, this will be a stub class... never get a property from Sim!
        /// </summary>
        public static InGameInterface InGameUI;
        public static MapClient MapUI;

        #endregion

        // other globals

        /// <summary>
        /// shows a bg image and a progress bar
        /// </summary>
        public static LoadingScreen LoadScreen;

        /// <summary>
        /// shows a messagebox.
        /// this screen stays in memory always, does not get added to Screens collection either...
        /// </summary>
        public static IngameLoadGameScreen IngameLoadScreen;      

        public static UWGame.SimSide.Snapshots.Snapshotter Snapshotter;
    }

}

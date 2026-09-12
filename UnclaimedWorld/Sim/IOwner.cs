using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide
{
    public enum HasEntityGroupID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }



    /// <summary>      
    /// let's try not to bloat this interface.
    /// </summary>
    public interface IHasEntityGroup : ILookUp<IHasEntityGroup, HasEntityGroupID>
    {      
        /// <summary>
        /// will be null for persons, expeditions, allegiances or households off map
        /// </summary>
        Vector3? Location { get; } // used by HaulingJobManager for center hauling location - could we push the location instead?
       
       
        int NoOfWorkers { get; }

        Allegiances.Allegiance Allegiance { get; } // give a reference to Allegiance instead?

        /// <summary>
        /// is this entity type eatable to anyone that owns it? If yes, it will be places in a separate collection for quick iteration.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool IsEatable(EntityType entityType);


        /// <summary>
        /// only placed here because Expedition does not define this proerty - it should use Allegiance's.
        /// (If we ever decide that Expeditions should manage their own credits, then move this property to EntityGroup.)
        /// 
        /// </summary>
        decimal? TradeCredits { get; set; }
    }
}

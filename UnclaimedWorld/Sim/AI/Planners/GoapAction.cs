using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Allegiances;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.AI
{

    /// <summary>
    /// http://gamedevelopment.tutsplus.com/tutorials/goal-oriented-action-planning-for-a-smarter-ai--cms-20793
    /// </summary>
    abstract class GoapAction: ISnapshot
    {
        // reference Planner instead?
        protected Allegiance allegiance;
        AllegianceID snapshotAllegiance;

        protected Expedition expedition;
        ExpeditionID snapshotExpedition;


        public GoapAction(Allegiance allegiance, Expedition expedition)
        {
            this.allegiance = allegiance;
            this.expedition = expedition;
        }

        public GoapAction()
        {
        }

        /// <summary>
        /// The cost of performing the action
        /// </summary>
      //  public float Cost = 1f;

        /// <summary>
        /// This will get run once when the action is created and added to a plan. 
        /// It should do all changes that is needed for this action to run properly.
        /// </summary>
        public abstract bool Init();


        //TODO: Add method exectue


        /// <summary>
        /// If this action returns false it will get deactivated an removed.
        /// </summary>
        /// <returns></returns>
        public abstract bool MonitorAction();

        /// <summary>
        /// This should handle all removal of actions this has done to the program.
        /// It should remove created jobs etc. 
        /// 
        /// is called on unexecuted as well as executed actions.
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        /// Temporary function returning an random point inside an area using 
        /// PlannerLocation
        /// and Planner radius defined in allegiance.
        /// 
        /// This function does not currently care about if the location is inside an unaccessible area 
        /// (only checks if the tile is completly blocked or not)
        /// </summary>
        /// <param name="allegiance"></param>
        /// <returns></returns>
        public static MapArea GetRandomMapArea(Expeditions.Expedition expedition, Allegiance allegiance)
        {
            
            //We need to have an map area, this will get reworked .

           // MapManager.SubtileValue[][] map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
            MapArea coveredArea = new MapArea();
            Point location = expedition.Location.Value.ToPoint();//.DwellingLocation;
            int radius = allegiance.GetForageAndHuntingRadius();

            int breakLoop = 0;
            Point tilePos;

            while(true)
            {
                //This gets an random area inside an square area.

                //TODO: Get an random point inside an circle instead.
                int randomX = The.Sim.GameplayRandomGenerator.RandomBetween(location.X - radius, location.X + radius);
                int randomY = The.Sim.GameplayRandomGenerator.RandomBetween(location.Y - radius, location.Y + radius);
                Vector3 randomPosition = new Vector3(randomX, randomY, 0);
                randomPosition = The.Map.ClampWorldPosition(randomPosition);
                tilePos = MapManager.WorldPosToTilePos(randomPosition).ToPoint();
                
                //Add escape loop if all random positions picked is blocked.
                breakLoop++;

                
               // Vector3 loc = new Vector3(location.X, location.Y, 0);
                Point sourceSubtile = MapManager.WorldPosToSubtile(location.ToVector2());
                Point destinationSubtile = MapManager.TileToCenterSubtile(tilePos);
                RegionMap.Result result = IsPathAccessible(sourceSubtile, destinationSubtile, allegiance);
                if (result == RegionMap.Result.OK)
                {
                    break;
                }
                else if (result == RegionMap.Result.Wait)
                {
                    return null;
                }


                if (breakLoop > 1000)
                {

                    //1000 tries still no valid position, bail with blocked pos...
                    //TODO: prevent this from happening? use regions?
                    return null;
                }

            } 

            coveredArea.Add(The.Map.GetTile(tilePos));

            coveredArea.StartDragTile = The.Map.GetTile(31, 40).TilePos.ToPoint(); // what an ugly hack


            return coveredArea;
        }

        private static RegionMap.Result IsPathAccessible(Point sourceSubtile,Point destinationSubtile, Allegiance allegiance)
        {
            //Currently uses ThreatStance.Cautious
            //TODO: Look at the threatstance design. How should this work.
            ThreatStance stanceToUse = ThreatStance.Cautious;

            //Need to see if the allegiance can fight. Usually this could be done in a case by case for each entity but as we only know about
            //The allegiance we will have to guess what stance they will use depending on their attack types.
            if (allegiance.RepresentativeEntityType.IntelligenceType.AttackTypes != null)
            {
                stanceToUse = ThreatStance.Normal;
            }
            RegionMap footRegionMap = allegiance.SharedKnowledge.GetMovementMap(
                ProtectionLevel.Exposed,
                allegiance.RepresentativeEntityType, //.ThreatCategory,
                stanceToUse).Layers[SurfaceType.TransportType.Foot].RegionMap;
            
            float distance = 0;
            RegionMap.Result result = footRegionMap.GetDistance(null, sourceSubtile, destinationSubtile, ref distance, false); // callback);
            return result;

        }


        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
            this.snapshotExpedition = (ExpeditionID)sn.SnapshotID<Expedition, ExpeditionID>(expedition);


            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
            expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);

        }

        #endregion

    }
}

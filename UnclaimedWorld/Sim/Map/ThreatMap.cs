using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
   
    /// <summary>
    /// use cautious for weak or vulnerable entities and bold for aggressive entities
    /// the values are the limits used for designating blocked tiles and causing repathing
    /// </summary>
    public enum ThreatStance: ulong { Cautious = 4, Normal = 12, Bold = 100 } // Bold never blocks.
    public class ThreatMap: InfluenceMap
    {
      
       /* const float cautiousWeight = 1.5f;
        const float normalWeight = 1f;
        const float boldWeight = 0.5f;*/

        public ThreatStance Approach;
        private float threatStanceFactor = 1f;
        private int? maxRadius = null;


        public EntityType EntityType
        {
            get;
            private set;
        }
      
      //  private List<ThreatCategory> UsersThreatenedBy;     
     //   private Dictionary<ThreatCategory, byte> UsersThreatLevels;

        
        private Allegiance allegiance;
        AllegianceID snapshotAllegiance;

        private float boldnessFactor;


        public ThreatMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
   
        }
       

        public ThreatMap(SharedKnowledge shared, int mapWidth, int mapHeight, EntityType entityType, ThreatStance approach)
            : base(mapWidth, mapHeight)
        {
            //Color = Color.Red;
            allegiance = shared.Allegiance;

            this.EntityType = entityType;

            Approach = approach;

            BlockingLimit = (byte)Approach;

            Map = new TileLayer((ushort)mapWidth, (ushort)mapHeight);

            if (Approach != ThreatStance.Bold)
            {   
                // block tiles:
                Map.BlockingLimit = (byte)Approach;
            }

            switch (Approach)
            {
                case ThreatStance.Bold:
                    threatStanceFactor = 1f; 
                    maxRadius = 2; // clamp the threat radius because we want less running around in combat situations, but still need the full value to determine panicking when facing an enemy.
                    break;
                case ThreatStance.Normal:
                    threatStanceFactor = 1f;
                    maxRadius = null;
                    break;
                case ThreatStance.Cautious:
                    threatStanceFactor = 2f;
                    maxRadius = null;
                    break;
            }


           // UsersCategory = allegiance.RepresentativeEntityType.ThreatCategory;


            /*UsersThreatenedBy = GameData.Instance.GlobalThreatenedBy[UsersCategory];
            UsersThreatLevels = GameData.Instance.GlobalThreatLevels[UsersCategory];
            */

            //invert it, and make 0.5 into 1 (scale: 0.5 - 1.5)
            this.boldnessFactor = (1f - EntityType.IntelligenceType.Boldness) + 0.5f; // (boldness * 0.4f) + 0.1f;
          
           // shared.AddThreatMap(this);

        }


        /// <summary>
        /// returns the number to paint in the center of the threat influence circle
        /// </summary>
        /// <param name="dangerLevel"></param>
        /// <returns></returns>
        private int GetThreatValue(double dangerLevel)
        {
            if (dangerLevel > 0)
            {
                int value = (int)(threatStanceFactor * boldnessFactor * 60f * dangerLevel) + 8;
               
                value = Common.ClampTop(value, 100);

                return value;

            }
            else return 0;
        }

        /// <summary>
        /// returns the negative number to paint in the center of the threat influence circle
        /// </summary>
        /// <param name="dangerLevel"></param>
        /// <returns></returns>
        private int GetAllyStrengthValue(double allyStrengthLevel)
        {
            return -1 * ((int)(boldnessFactor * 15f * allyStrengthLevel) + 8);
        }


        /// <summary>
        /// draw allies with a negative value relative to their strength
        /// </summary>
        private void DrawAllies()
        {
            if (Approach == ThreatStance.Bold)
            {
                return;
            }
        
            // collect the sectors that we touched, add to those found when doing threats.
          
           
            double allyStrengthLevel;
            int strengthValue;

            foreach (var entity in allegiance.Members)
            {   // TODO: IsSpotted, IsSleeping, InsideBuilding...

                
                if (!entity.IsOnPlaySite()
                    || entity.StrengthRating == null
                    || (entity.EntityType.NonLivingType != null && !entity.NonLivingEntity.IsCompleted())
                    || (entity.EntityType.IntelligenceType != null && !entity.Intelligence.IsAwakeAndActive))
                {
                    continue;
                }
                
             
                allyStrengthLevel = GetStrengthRatio(entity.StrengthRating.Value, 
                    GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating]); // allegiance.RepresentativeEntityType.IntelligenceType.StrengthRating]);

                if (allyStrengthLevel > 0f)
                {
                    strengthValue = GetAllyStrengthValue(allyStrengthLevel);

                    Map.DrawLinearInfluenceCircle(entity.MapPosition.Value,
                            strengthValue,
                            Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, -fallOffValueEachTile);

                    /*
                    InfluenceMap.DrawLinearInfluenceCircle(map,
                        entity.MapPosition,
                            strengthValue,
                            Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, -fallOffValueEachTile,
                            affectedSectors, MapManager.SectorSizeInTiles);*/
                }
              
            }
                                  
        }

        private const int fallOffValueEachTile = 8;

        /// <summary>
        ///  scales so that the same strength rating gives danger = 0.5.
        /// </summary>
        /// <param name="enemyRating"></param>
        /// <param name="ourStrengthRating"></param>
        /// <returns></returns>
        private static float GetStrengthRatio(float enemyRating, float ourStrengthRating)
        {
            float ourStrengthRatingToUse = Common.ClampBottom(ourStrengthRating, 0.05f);

            //
           /* if (Common.IsZero(ourStrengthRating))
            {

            }*/

            float strengthRatio = 0.5f * (enemyRating / ourStrengthRatingToUse);

            return strengthRatio;
        }

       

        private void DrawThreats()
        {
            
            if (Approach == ThreatStance.Bold) // we don't want threats on the bold map. it creates a disturbing movement pattern
            {
                return;
            }
            
           
            List<EntityID> invalidEntityIDs = null;

            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;

            foreach (var kvp in sharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite) 
            {
                DrawThreat(ref invalidEntityIDs, kvp.Key);                
            }


            foreach (var kvp in sharedKnowledge.PlaySiteKnowledge.AllKnownThreatSources)
            {
                DrawThreat(ref invalidEntityIDs, kvp.Key);
            }

            sharedKnowledge.RemoveInvalidEntityIDs(invalidEntityIDs);

        }

       

        private void DrawThreat(ref List<EntityID> invalidEntityIDs, EntityID entityID)
        {
            IKnownEntityData entityData;
            if (GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(entityID, out entityData)))
            {
                Common.AddToList(ref invalidEntityIDs, entityID);
                // later: remove invalid entityIDs from all collections...
                // continue;
            }
            else
            {

                if (entityData.StrengthRating != null) // TODO: IsSleeping, InsideBuilding...
                {
                    double dangerLevel;
                    int threatValue;
                   
                    dangerLevel = GetStrengthRatio(entityData.StrengthRating.Value, 
                        GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating]); 

                    threatValue = GetThreatValue(dangerLevel);

                    if (threatValue > 0f)
                    {
                        // displace the threat footprint by the entity's move vector to compensate for low update frequency (only if seen directly)
                        Vector2 position = entityData.PlaySiteLocation.ToVector2();

                        Entity entity = entityData as Entity;
                        if (entity != null)
                        {
                            Locomotor locomotor = entity.Locomotor;
                            if (locomotor != null
                                && locomotor.IsMoving() == true)
                            {
                                position += entity.FacingNormal.ToVector2() * locomotor.GetTargetSpeed(); // use realized speed instead???
                            }
                        }

                        Point tilePos = MapManager.WorldPosToTile(position);

                        Map.DrawLinearInfluenceCircle(tilePos,
                                threatValue,
                                Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, fallOffValueEachTile,
                                maxRadius);

                       /* InfluenceMap.DrawLinearInfluenceCircle(map,
                                tilePos,
                                threatValue,
                                Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, fallOffValueEachTile,
                                affectedSectors, MapManager.SectorSizeInTiles, maxRadius);*/
                    }
                }
            }
        }

        /* // moved to TileSector
        private void SetBlockedStatus()
        {
            if (Approach != ThreatStance.Bold)
            {   // block tiles:

                int width = Common.GetJaggedArrayWidth(map);
                int height = Common.GetJaggedArrayHeight(map);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[x][y] > BlockingLimit)
                        {
                            IsBlocked[x][y] = true;
                        }
                        else
                        {
                            IsBlocked[x][y] = false;
                        }
                    }
                }
            }
        }*/

       // HashSet<Point> affectedNowAndPreviously = new HashSet<Point>();
        

       /* private bool SectorHasChanged(Sector sector)
        {
            for (int x = sector.TileArea.Left; x < sector.TileArea.Right; x++)
            {
                for (int y = sector.TileArea.Top; y < sector.TileArea.Bottom; y++)
                {
                    // compare old and new values:
                    if (backBufferMap1[x][y] != backBufferMap2[x][y])
                    {
                        return true;
                    }
                }
            }

            return false;
        }*/


        /*
        private Sector GetSectorFromCoords(Point coords)
        {
            return Sectors[coords.X][coords.Y];
        }*/

     /*   public static float GetThreatApproachFactor(ThreatStance approach)
        {
            switch (approach)
            {
                case ThreatStance.Cautious: return cautiousWeight;
                case ThreatStance.Normal: return normalWeight;
                case ThreatStance.Bold: return boldWeight;
                default: return normalWeight;
            }
        }*/

        public override string ToString()
        {
            return IDName;
        }

       
      /*  public override bool IsDirty()
        {
            //bool returnValue = isDirty;
           // isDirty = false;

         //   return returnValue;

            return false;
        }*/

        private enum Phase { ClearMap, DrawThreats, DrawAllies, /*SetBlockedStatus,*/ CompareSectors }
        private Phase phase = Phase.ClearMap;

        /*
        private HashSet<TileSector> affectedSectors = new HashSet<TileSector>();
        private HashSet<TileSector> previouslyAffectedSectors = new HashSet<TileSector>();
       */

        /// <summary>
        /// during drawing and after clear, all sectors that get a value are gathered in this set.
        /// sectors that are not drawn into should be removed at the end.
        /// </summary>
       /* private HashSet<Point> affectedSectors = new HashSet<Point>();

        /// <summary>
        /// these are the sectors that were drawn into last time. We compare the two sets for any changes in order to signal the dependent maps via the IsDirty flag.
        /// </summary>
        private HashSet<Point> previouslyAffectedSectors = new HashSet<Point>();
        */


        /// <summary>
        /// called from MovementMap, via DiscomfortMap!
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoCycle()
        {
            switch (phase)
            {
                case Phase.ClearMap:
                    IsReady = false;

                    Map.BeginDrawing();
                  
                    Map.ClearMaps();                    

                    phase = Phase.DrawThreats;

                    return false;

                case Phase.DrawThreats:
                    DrawThreats();

                    phase = Phase.DrawAllies;
                                       

                    return false;
                case Phase.DrawAllies:
                    DrawAllies();

                   // Map.SwitchBuffers();
                    IsReady = true; // done

                    phase = Phase.CompareSectors;  
                   
                    return false;
              
                case Phase.CompareSectors:
                    RemoveEmptySectors();
                    
                    Map.CompareOldAndNewSectors();
                    phase = Phase.ClearMap;

                    return true;

                default:
                    return true;
            }           

        }

        void RemoveEmptySectors()
        {
            HashSet<Point> sectorsToRemove = new HashSet<Point>();
            sectorsToRemove.UnionWith(Map.GetAllSectors());
            sectorsToRemove.ExceptWith(Map.AffectedSectors);

            Map.RemoveSectors(sectorsToRemove);
        }
        

#region ISnapshot

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            // save current progress

            this.Approach = (ThreatStance)sn.DoEnum(Approach);
            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
          
            this.boldnessFactor = sn.DoFloat(boldnessFactor);
            this.maxRadius = sn.DoInt32Nullable(maxRadius);
            this.phase = (Phase)sn.DoEnum(phase);
            this.threatStanceFactor = sn.DoFloat(threatStanceFactor);
          //  this.UsersCategory = (ThreatCategory)sn.DoEnum(UsersCategory);
            this.EntityType = sn.DoGameData(EntityType);

          /*  sn.Ignore(UsersThreatenedBy); // not used..
            sn.Ignore(UsersThreatLevels);
            */

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
        }

#endregion


    }
}

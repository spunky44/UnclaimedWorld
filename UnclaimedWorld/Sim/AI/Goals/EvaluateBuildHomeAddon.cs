using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using SpriteSheetRuntime;
using UWGame.SimSide.Entities.Biological;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// start new addons to current home. Should be active in leisure period.
    /// </summary>
    class EvaluateBuildHomeAddon: GoalEvaluator
    {
        EntityType addonToStart;
        Point mostDesirableLocation;

        bool buildNewHome = false;

        /// <summary>
        /// start new addons to current home. Should be active in leisure period.
        /// </summary>
        public EvaluateBuildHomeAddon(Entity entity): base(entity)
        {
            
        }


        private double ScoreNeedForAddon(Person personEntity)
        {
           /* if (personEntity.Household.Home.EntityType.TileLayoutType != null)
            {
                int noOfExistingAddons = 0;
                            

                return 1.0 - //(double)noOfExistingAddons / //
                    (double)personEntity.Household.Home.Structure.AddOns.Count /
                    (double)personEntity.Household.Home.EntityType.TileLayoutType.BuildingType.TotalAddonSlots;
            }
            else*/ return 0;
        }

             
        private static bool IsAddon(Entity entity)
        {
            return entity.EntityType.StructureType != null && entity.EntityType.StructureType.IsAddon;
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            //return 0;  
            Person personEntity = entity.PersonEntity;

          /*  Entity home = personEntity.Household.Home;

            if (home != null
                && false
               )
            {
                BiologicalEntity bioEntity = entity.BiologicalEntity;
                double ageContribution = ScoreAge(bioEntity);

                if (ageContribution > 0)
                {

                    double timeOfDayContribution = ScoreTimeOfDay();

                    // placeholder... replace with real evaluation of all addon types, matched with household needs and wants as well as cost (in ground space, work and materials)
                    double needForAddon = ScoreNeedForAddon(personEntity);

                    addonToStart = null;
                    if (needForAddon > 0.75)
                    {
                        addonToStart = FindAddonToStart(personEntity);
                    }

                    if (addonToStart != null)
                    {
                        result = 0.7 * needForAddon + 0.2 * timeOfDayContribution + 0.1 * ageContribution;

                        result *= Priority; // NEW!

                        return CalculateResult.Done;
                    }
                }
            }*/

            result = 0;
            return CalculateResult.Done;            
        }

        private bool WeHaveAddonAlready(Person personEntity, EntityType proposedAddon)
        {
           /*   Entity home = personEntity.Household.Home;
            // test to see if we already have one of these addons built:
            foreach (IAddon addon in home.Structure.AddOns)
            {
                Entity addonAsEntity = addon as Entity;

                if (addonAsEntity != null && addonAsEntity.EntityType == proposedAddon)
                {
                    return true;
                }
            }
              if (home.Contains != null)
            {
             
                //foreach (Entity contained in home.Contains)
                {          
                    if (IsAddon(contained) && contained.EntityType == proposedAddon)
                    {
                        return true;
                    }
                }
            }*/
            return false;
        }

        private EntityType FindAddonToStart(Person personEntity)
        {
            
          /*  TerrainTile[][] tileMap = The.Map.TileMap;
            foreach (KeyValuePair<AddonSize, List<Point>> kvp in
                personEntity.Household.Home.EntityType.TileLayoutType.BuildingType.GetAddonSlots(personEntity.Household.Home.FlipHorizontally))
            {
                foreach (Point slot in kvp.Value)
                {
                    if (tileMap[slot.X][slot.Y].GeoLayoutEntitiesOnTile == null)
                    {
                        // free slot... find an addon:

                        foreach (KeyValuePair<string, EntityType> structureType in GameData.Instance.AllStructureTypes)
                        {
                            if (structureType.Value.StructureType.IsAddon && structureType.Value.StructureType.AddonSize == kvp.Key)
                            {
                                if (!WeHaveAddonAlready(personEntity, structureType.Value))
                                {
                                    mostDesirableLocation = slot;
                                    return structureType.Value;
                                }
                            }
                        }
                    }
                }
            }*/
            return null;
        }

        public override bool CancelCurrentTakers()
        {
            throw new NotImplementedException();
        }
        public override bool CanTakeGoal()
        {
            throw new NotImplementedException();
        }
        public override bool SetGoal()
        {
            base.SetGoal();

            return false;
            // TODO...
         /*   PersonEntity personEntity = entity.PersonEntity;

            Entity home = personEntity.Household.Home;
        
            Entity newAddon = new Entity(addonToStart);
                        
            // start the addon for the whole household to participate in:
            if (newAddon.Structure.PrepareAndStartBuildingJob(
                personEntity.Household.Ownership, Jobs.Priority.Normal, new Point(mostDesirableLocation.X + home.TopLeftMapPosition.X, mostDesirableLocation.Y + home.TopLeftMapPosition.Y)))
            {
                newAddon.Structure.AddonTo = home;
                home.Structure.AddOns.Add(newAddon);

            }
            else
            {
                //TODO: mark the slot so it doesn't get used again for some time...

            }*/
            
        }
    }
}

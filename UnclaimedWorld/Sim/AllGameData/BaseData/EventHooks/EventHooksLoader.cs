using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.EventHooks
{
    /// <summary>
    /// all the connections from game events to actions are loaded here  //MP? no there's also a doc called DetectionEventHooksLoader
    /// </summary>
    public class EventHooksLoader
    {
        public static List<AgentActionHook> InitAgentActionHooks()
        {
            List<AgentActionHook> list = new List<AgentActionHook>();


            #region migration

            list.Add(new AgentActionHook()
            {
                KeyName = "humanDecidedToLeave",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.DecidedToLeaveAllegiance,

                ActionSetsKey = "humanDecidedToLeaveDialog"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanDecidedToLeaveRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.StartsToLeaveAllegiance, // DecidedToLeaveAllegiance,

                ActionSetsKey = "humanDecidedToLeaveRemark"
            });

            /*   list.Add(new AgentActionHook()
               {
                   KeyName = "humanLeavesSiteRemark",
                   TypeKey = "entity:human",
                   Hook = AgentActionHooks.LeavesSiteForNewAllegiance,

                   ActionSetsKey = "humanLeavesSiteRemark"
               });*/

            #endregion        


            #region Agent messaging

            list.Add(new AgentActionHook()
            {
                KeyName = "otherAgentRequestsCarriedItem",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.ForceDropsItem,

                ActionSetsKey = "otherAgentRequestsCarriedItemRemark"
            });

            #endregion


            return list;

        }

        public static List<AttackTypeActionHook> InitAttackTypeHooks()
        {
            List<AttackTypeActionHook> list = new List<AttackTypeActionHook>();

          /*  #region human barehanded fight

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithLowPunch",
                TypeKey = "personPunchLowRight",
                Hook = AgentActionHooks.HitEnemy,

                //ActionSetsKey = "humanHitEnemyWithPunchRemark" //same remark as the other kind of punch
            });
            #endregion*/

            return list;

        }

        public static List<EntityEventHook> InitEntityEventHooks()
        {
            List<EntityEventHook> list = new List<EntityEventHook>();
            // TB Farming 19.03.2015
            // hooks in when the small plot is to be destroyed
            #region on smallPlot ToBeDestroyed
            list.Add(new EntityEventHook()
            {
                KeyName = "smallPlotToBeDestroyedHook",
                TypeKey = "structure:smallPlot",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "smallPlotToBeDestroyed"
            });
            list.Add(new EntityEventHook()
            {
                KeyName = "smallPlotToBeDestroyedHook2",
                TypeKey = "structure:smallPlot",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "transcripePlot"
            });
            #endregion
            // hooks in when the large plot is to be destroyed
            #region on largePlot ToBeDestroyed
            list.Add(new EntityEventHook()
            {
                KeyName = "largePlotToBeDestroyedHook",
                TypeKey = "structure:largePlot",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "largePlotToBeDestroyed"
            });
            list.Add(new EntityEventHook()
            {
                KeyName = "largePlotToBeDestroyedHook2",
                TypeKey = "structure:largePlot",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "transcripePlot"
            });
            #endregion

          
            /*  #region on fishTrap ToBeDestroyed
          #region CreekSticks
            list.Add(new EntityEventHook()
            {
                KeyName = "fishTrapCreekSticksSalvaged", 
                TypeKey = "structure:fishTrapCreekSticks",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "fishTrapSalvaged"
            });
                #endregion
            #region CreekNet
            list.Add(new EntityEventHook()
            {
                KeyName = "fishTrapCreekNetSalvaged",
                TypeKey = "structure:fishTrapCreekNet",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "fishTrapSalvaged"
            });
            #endregion
                #region Coast
            list.Add(new EntityEventHook()
            {
                KeyName = "fishTrapCoastSalvaged",
                TypeKey = "structure:fishTrapCoast",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "fishTrapSalvaged"
            });
                #endregion
            #region ShoreBasket
            list.Add(new EntityEventHook()
            {
                KeyName = "fishTrapShoreBasketSalvaged",
                TypeKey = "structure:fishTrapShoreBasket",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "fishTrapSalvaged"
            });
                #endregion
            #region ShoreHoopNet
            list.Add(new EntityEventHook()
            {
                KeyName = "fishTrapShoreHoopNetSalvaged",
                TypeKey = "structure:fishTrapShoreHoopNet",
                Hook = EntityEventHooks.ToBeDestroyed,

                ActionSetsKey = "fishTrapSalvaged"
            });
            #endregion
            #endregion
            */

           list.Add(new EntityEventHook()
           {
               KeyName = "CompleteFishTrapCreekSticks",
               TypeKey = "structure:fishTrapCreekSticks",
               Hook = EntityEventHooks.ComeOnline, // should also work on script spawns
               ActionSetsKey = "fishTrapFinished"
           });

           list.Add(new EntityEventHook()
           {
               KeyName = "CompleteFishTrapCreekNet",
               TypeKey = "structure:fishTrapCreekNet",
               Hook = EntityEventHooks.ComeOnline, // should also work on script spawns
               ActionSetsKey = "fishTrapFinished"
           });

           list.Add(new EntityEventHook()
           {
               KeyName = "CompleteFishTrapCoast",
               TypeKey = "structure:fishTrapCoast",
               Hook = EntityEventHooks.ComeOnline, // should also work on script spawns
               ActionSetsKey = "fishTrapFinished"
           });

           list.Add(new EntityEventHook()
           {
               KeyName = "CompleteFishTrapShoreBasket",
               TypeKey = "structure:fishTrapShoreBasket",
               Hook = EntityEventHooks.ComeOnline, // should also work on script spawns
               ActionSetsKey = "fishTrapFinished"
           });

           list.Add(new EntityEventHook()
           {
               KeyName = "CompleteFishTrapShoreHoopNet",
               TypeKey = "structure:fishTrapShoreHoopNet",
               Hook = EntityEventHooks.ComeOnline, // should also work on script spawns
               ActionSetsKey = "fishTrapFinished"
           });

            return list;
        }


        public static List<EffectTypeActionHook> InitEffectTypeHooks()
        {
            List<EffectTypeActionHook> list = new List<EffectTypeActionHook>();

            list.Add(new EffectTypeActionHook()
                {
                    KeyName = "usesStimulantHook",
                    Hook = AgentActionHooks.StartedEffect,
                    TypeKey = "caffeine", 
                    ActionSetsKey = "usesStimulantRemark"
                });

            list.Add(new EffectTypeActionHook()
            {
                KeyName = "endedStimulantHook",
                Hook = AgentActionHooks.EndedEffect,
                TypeKey = "caffeine",
                ActionSetsKey = "endedStimulantRemark"
            });


            return list;
        }


        public static List<ProcessTypeActionHook> InitProcessTypeHooks()
        {
            List<ProcessTypeActionHook> list = new List<ProcessTypeActionHook>();

            #region animal traps 
            #region collapse trap
            /*
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "collapseTrapCompleted1",
                TypeKey = "constructCollapseTrap",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "noAutoCheckTrapFinished"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "collapseTrapCompleted2",
                TypeKey = "constructCollapseTrap",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });*/
            #endregion
            #region Spring snare
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "snareCompleted1",
                TypeKey = "constructSpringSnare",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "snareFinished"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "snareCompleted2",
                TypeKey = "constructSpringSnare",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });
            #endregion
            #region Deadfall trap
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructDeadfallTrapCompleted1", // "deadfallTrapFinished",
                TypeKey = "constructDeadfallTrap",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "snareFinished"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructDeadfallTrapCompleted2",
                TypeKey = "constructDeadfallTrap",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });
            #endregion
            #region spike trap
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructRatTrapCompleted1",
                TypeKey = "constructSpikeTrap",
                Hook = AgentActionHooks.CompletedProducing,
                ExecutionOrder = 0,
                ActionSetsKey = "snareFinished"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructRatTrapCompleted3",
                TypeKey = "constructSpikeTrap",
                Hook = AgentActionHooks.CompletedProducing,
                ExecutionOrder = 1,
                ActionSetsKey = "customSpikeTrapProperties" // overrides some snareFinished properties
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructRatTrapCompleted2",
                TypeKey = "constructSpikeTrap",
                Hook = AgentActionHooks.CompletedProducing,
                ExecutionOrder = 2,
                ActionSetsKey = "activateSnare"
            });
            
            #endregion

            #region change bait
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "changeBaitToBlackpulpCompleted",
                TypeKey = "changeBaitToBlackpulp",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "changeBaitToBlackpulp"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "changeBaitToGlassyCreeperCompleted",
                TypeKey = "changeBaitToGlassyCreeper",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "changeBaitToGlassyCreeper"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "changeBaitToRatMeatCompleted",
                TypeKey = "changeBaitToRatMeat",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "changeBaitToRatMeat"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "changeBaitToNoBaitCompleted", 
                TypeKey = "changeBaitToNoBait",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "changeBaitToNoBait"
            });
            #endregion

            #region no bait activation
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateSnareCompleted1",
                TypeKey = "activateSnare",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateSnareCompleted2",
                TypeKey = "activateSnare",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "animalTrapCheckTalk"
            });
            #endregion
            #region blackpulp bait activation
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithBlackpulpCompleted1",
                TypeKey = "activateTrapWithBlackpulp",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithBlackpulpCompleted2",
                TypeKey = "activateTrapWithBlackpulp",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "animalTrapCheckTalk"
            });
            #endregion
            #region Glassy Creeper bait activation
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithGlassyCreeperCompleted1",
                TypeKey = "activateTrapWithGlassyCreeper",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithGlassyCreeperCompleted2",
                TypeKey = "activateTrapWithGlassyCreeper",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "animalTrapCheckTalk"
            });
            #endregion
            #region rat meat bait activation 
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithRatMeatCompleted1",
                TypeKey = "activateTrapWithRatMeat",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "activateSnare"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "activateTrapWithRatMeatCompleted2",
                TypeKey = "activateTrapWithRatMeat",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "animalTrapCheckTalk"
            });
            #endregion
            // kill animal inside trap
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "killTrappedAnimalCompleted",
                TypeKey = "killTrappedAnimal",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "killContainingEntities"
            });
            #endregion


            // TB Farming 19.03.2015
            #region Farming
                // hooks in when a small plot has been established (build)
                #region establishSmallPlot - CompletedProducing
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "establishSmallPlotCompleted1",
                TypeKey = "establishSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "smallPlotFinished"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "establishSmallPlotCompleted2",
                TypeKey = "establishSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plotEstablished"
            });
                #endregion
                // hooks in when a plot has been established (build)
                #region establishLargePlot - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "establishLargePlotCompleted1",
                    TypeKey = "establishLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "largePlotFinished"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "establishLargePlotCompleted2",
                    TypeKey = "establishLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "plotEstablished"
                });
                #endregion
                // hooks in when a greenhouse has been established (build)
                #region constructImprovisedGreenhouse - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "establishGreenhouseCompleted1",
                    TypeKey = "constructImprovisedGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing, 

                    ActionSetsKey = "greenhouseFinished"
                });
                #endregion
                #region constructGreenhouse - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "establishGreenhouseCompleted2",
                    TypeKey = "constructGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "greenhouseFinished" // for now, uses same properties as the primitive gh
                });
                #endregion
                // hooks in when seeds are planted
                #region hooks for the different seeds - CompletedProducing
                    #region glassyCreeperPods
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantGlassyCreeperPodsInSmallPlotHook",
                TypeKey = "plantGlassyCreeperPodsInSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeGlassyCreeperPodsInSmallPlotHook",
                TypeKey = "plantGlassyCreeperPodsInSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeGlassyCreeperPodsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantGlassyCreeperPodsInLargePlotHook",
                TypeKey = "plantGlassyCreeperPodsInLargePlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeGlassyCreeperPodsInLargePlotHook",
                TypeKey = "plantGlassyCreeperPodsInLargePlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeGlassyCreeperPodsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantGlassyCreeperPodsInGreenhouseHook",
                TypeKey = "plantGlassyCreeperPodsInGreenhouse",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeGlassyCreeperPodsInGreenhouseHook",
                TypeKey = "plantGlassyCreeperPodsInGreenhouse",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeGlassyCreeperPodsAction"
            });
                    #endregion
                #region cotton
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "plantCottonInSmallPlotHook",
                    TypeKey = "plantCottonInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "plantSeedsAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "initializeCottonInSmallPlotHook",
                    TypeKey = "plantCottonInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "initializeCottonAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "plantCottonInLargePlotHook",
                    TypeKey = "plantCottonInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "plantSeedsAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "initializeCottonInLargePlotHook",
                    TypeKey = "plantCottonInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "initializeCottonAction"
                });           
                #endregion
            #region crystalBerries
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantCrystalBerriesInSmallPlotHook",
                TypeKey = "plantCrystalBerriesInSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeCrystalBerriesInSmallPlotHook",
                TypeKey = "plantCrystalBerriesInSmallPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeCrystalBerriesAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantCrystalBerriesInLargePlotHook",
                TypeKey = "plantCrystalBerriesInLargePlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeCrystalBerriesInLargePlotHook",
                TypeKey = "plantCrystalBerriesInLargePlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeCrystalBerriesAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "plantCrystalBerriesInGreenhouseHook",
                TypeKey = "plantCrystalBerriesInGreenhouse",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "plantSeedsAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "initializeCrystalBerriesInGreenhouseHook",
                TypeKey = "plantCrystalBerriesInGreenhouse",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "initializeCrystalBerriesAction"
            });
                #endregion
                #region fingerfruit
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "plantFingerFruitInGreenhouseHook",
                    TypeKey = "plantFingerFruitInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "plantSeedsAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "initializeFingerFruitInGreenhousePlotHook",
                    TypeKey = "plantFingerFruitInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "initializeFingerFruitsAction"
                });
                #endregion
                #endregion
            // hooks in when a plot gets weeded
                #region weedPlot - CompletedProducing
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "weedPlotHook1",
                TypeKey = "weedPlot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "weedPlotAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "weedPlotHook2",
                TypeKey = "weedLargePlot",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "weedPlotAction"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "weedPlotHook3",
                TypeKey = "weedGreenhouse",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "weedPlotAction"
            });
                #endregion
            // hooks in when a plot gets weeded
                #region fertilize - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "organicFertilizePlotHook1",
                    TypeKey = "organicFertilizePlot",
                    Hook = AgentActionHooks.CompletedProducing,
                    ActionSetsKey = "organicFertilizePlotAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "organicFertilizePlotHook2",
                    TypeKey = "organicFertilizeLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,
                    ActionSetsKey = "organicFertilizePlotAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "guanoFertilizePlotHook1",
                    TypeKey = "guanoFertilizePlot",
                    Hook = AgentActionHooks.CompletedProducing,
                    ActionSetsKey = "guanoFertilizePlotAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "guanoFertilizePlotHook2",
                    TypeKey = "guanoFertilizeLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,
                    ActionSetsKey = "guanoFertilizePlotAction"
                });
                #endregion
                // hooks in when the plot gets harvested
                #region harvestCrops - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "harvestCropsHook",
                    TypeKey = "harvestCrops",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "harvestCropsAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "harvestCropsHook2",
                    TypeKey = "harvestLargeCrops",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "harvestCropsAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "harvestCropsHook3",
                    TypeKey = "harvestGreenhouseCrops",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "harvestCropsAction"
                });
                #endregion

                #region useFertilize - CompletedProducing
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "useOrganicFertilizerHook",
                    TypeKey = "useOrganicFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "useOrganicFertilizerAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "useLargeOrganicFertilizerHook",
                    TypeKey = "useLargeOrganicFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "useLargeOrganicFertilizerAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "useGuanoFertilizerHook",
                    TypeKey = "useGuanoFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "useGuanoFertilizerAction"
                });
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "useLargeGuanoFertilizerHook",
                    TypeKey = "useLargeGuanoFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "useLargeGuanoFertilizerAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopUsingLargeGuanoFertilizerHook",
                    TypeKey = "stopUsingLargeGuanoFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopUsingFertilizerAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopUsingGuanoFertilizerHook",
                    TypeKey = "stopUsingGuanoFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopUsingFertilizerAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopUsingLargeOrganicFertilizerHook",
                    TypeKey = "stopUsingLargeOrganicFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopUsingFertilizerAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopUsingOrganicFertilizerHook",
                    TypeKey = "stopUsingOrganicFertilizer",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopUsingFertilizerAction"
                });
                #endregion

                #region NEW: automatic farming - set crop
                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growCrystalBerriesInLargePlotHook",
                    TypeKey = "growCrystalBerriesInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growCrystalBerriesInLargePlotAction" 
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingCrystalBerriesInLargePlotHook",
                    TypeKey = "stopGrowingCrystalBerriesInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growGlassyCreeperPodsInLargePlotHook",
                    TypeKey = "growGlassyCreeperPodsInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growGlassyCreeperPodsInLargePlotAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingGlassyCreeperPodsInLargePlotHook",
                    TypeKey = "stopGrowingGlassyCreeperPodsInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growCottonInLargePlotHook",
                    TypeKey = "growCottonInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growCottonInLargePlotAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingCottonInLargePlotHook",
                    TypeKey = "stopGrowingCottonInLargePlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

            //** small plot

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growCrystalBerriesInSmallPlotHook",
                    TypeKey = "growCrystalBerriesInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growCrystalBerriesInSmallPlotAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingCrystalBerriesInSmallPlotHook",
                    TypeKey = "stopGrowingCrystalBerriesInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growGlassyCreeperPodsInSmallPlotHook",
                    TypeKey = "growGlassyCreeperPodsInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growGlassyCreeperPodsInSmallPlotAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingGlassyCreeperPodsInSmallPlotHook",
                    TypeKey = "stopGrowingGlassyCreeperPodsInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growCottonInSmallPlotHook",
                    TypeKey = "growCottonInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growCottonInSmallPlotAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingCottonInSmallPlotHook",
                    TypeKey = "stopGrowingCottonInSmallPlot",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                //** greenhouse

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growCrystalBerriesInGreenhouseHook",
                    TypeKey = "growCrystalBerriesInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growCrystalBerriesInGreenhouseAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingCrystalBerriesInGreenhouseHook",
                    TypeKey = "stopGrowingCrystalBerriesInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growGlassyCreeperPodsInGreenhouseHook",
                    TypeKey = "growGlassyCreeperPodsInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growGlassyCreeperPodsInGreenhouseAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingGlassyCreeperPodsInGreenhouseHook",
                    TypeKey = "stopGrowingGlassyCreeperPodsInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "growFingerFruitInGreenhouseHook",
                    TypeKey = "growFingerFruitInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "growFingerFruitInGreenhouseAction"
                });

                list.Add(new ProcessTypeActionHook()
                {
                    KeyName = "stopGrowingFingerFruitInGreenhouseHook",
                    TypeKey = "stopGrowingFingerFruitInGreenhouse",
                    Hook = AgentActionHooks.CompletedProducing,

                    ActionSetsKey = "stopGrowingAction"
                });


                #endregion

            #endregion

            #region Pier
            // functionality replaced by code
           /* list.Add(new ProcessTypeActionHook()
            {
                KeyName = "buildImprovisedLandingFinishedHook",
                TypeKey = "buildImprovisedLanding",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "placePierFinished"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "buildSimplePortFinishedHook",
                TypeKey = "buildSimplePort",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "placePierFinished"
            });
            */
            #endregion

                // TB FishTrap 19.03.2015 
            #region fishTrap
                // hooks in when a fishTrap gets placed
                // this sets properties on the fish trap spot - NOT on the fish trap - and on completion, the values are copied to the fish trap - WTF?
                // -fishtrapCompleted now runs when the fish trap comes online!
            #region fishTrapFinished - StartProducing
            #region Creek //sticks weir
           /* list.Add(new ProcessTypeActionHook()
            {
                KeyName = "beginPlaceFishTrapCreekSticksHook",
                TypeKey = "placeFishTrapCreekSticks",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "initializeFishTrapCreekSticks"
            });*/
            // replaced with Entity Completed hook event
           /* list.Add(new ProcessTypeActionHook()
            {
                KeyName = "PlaceFishTrapCreekSticksHook",
                TypeKey = "placeFishTrapCreekSticks",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "fishTrapFinished"
            });*/
            #endregion

            #region Creek //net weir
          /*  list.Add(new ProcessTypeActionHook()
            {
                KeyName = "beginPlaceFishTrapCreekNetHook",
                TypeKey = "placeFishTrapCreekNet",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "initializeFishTrapCreekNet"
            });*/
           /* list.Add(new ProcessTypeActionHook()
            {
                KeyName = "PlaceFishTrapCreekNetHook",
                TypeKey = "placeFishTrapCreekNet",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "fishTrapFinished"
            });*/
            #endregion

            #region coast //saltwater fyke
          /*  list.Add(new ProcessTypeActionHook()
            {
                KeyName = "beginPlaceFishTrapCoastHook",
                TypeKey = "placeFishTrapCoast",
                Hook = AgentActionHooks.StartProducing,
                ActionSetsKey = "initializeFishTrapCoast"
            });*/
           /* list.Add(new ProcessTypeActionHook()
            {
                KeyName = "PlaceFishTrapCoastHook",
                TypeKey = "placeFishTrapCoast",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "fishTrapFinished"
            });*/
            #endregion

            #region shore // freshwater basket
          /*  list.Add(new ProcessTypeActionHook()
            {
                KeyName = "beginPlaceFishTrapShoreBasketHook",
                TypeKey = "placeFishTrapShoreBasket",
                Hook = AgentActionHooks.StartProducing,
                ActionSetsKey = "initializeFishTrapShoreBasket"
            });*/
          /*  list.Add(new ProcessTypeActionHook()
            {
                KeyName = "PlaceFishTrapShoreBasketHook",
                TypeKey = "placeFishTrapShoreBasket",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "fishTrapFinished"
            });*/
            #endregion

            #region shore // freshwater hoop net
          /*  list.Add(new ProcessTypeActionHook()
            {
                KeyName = "beginPlaceFishTrapShoreHoopNetHook",
                TypeKey = "placeFishTrapShoreHoopNet",
                Hook = AgentActionHooks.StartProducing,
                ActionSetsKey = "initializeFishTrapShoreHoopNet"
            });*/
         /*   list.Add(new ProcessTypeActionHook()
            {
                KeyName = "PlaceFishTrapShoreHoopNetHook",
                TypeKey = "placeFishTrapShoreHoopNet",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "fishTrapFinished"
            });*/
            #endregion

            #endregion
                // hooks in when a trap gets checked - CompletedProducing
                #region checkFishTrap
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "checkFishTrapHook",
                TypeKey = "checkFishTrap",
                Hook = AgentActionHooks.StartProducing,
                ActionSetsKey = "checkFishTrap"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "checkFishTrapHook2",
                TypeKey = "checkFishTrap",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "fishCheckTalk"
            });
                #endregion
            #endregion

          
            return list;
            
        }
    }
}

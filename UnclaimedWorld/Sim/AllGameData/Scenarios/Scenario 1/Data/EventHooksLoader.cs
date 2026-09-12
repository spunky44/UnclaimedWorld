using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    public class EventHooksLoader
    {
        public static List<AgentActionHook> InitAgentActionHooks()
        {
            List<AgentActionHook> list = new List<AgentActionHook>();



            #region production
            list.Add(new AgentActionHook()
            {
                KeyName = "startHarvestRemark", 
                TypeKey = "entity:human",
                Hook = AgentActionHooks.StartHarvesting,

                ActionSetsKey = "startHarvestRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "endHarvestRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.CompletedHarvesting,

                ActionSetsKey = "endHarvestRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "startConstructionRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.StartConstructing,

                ActionSetsKey = "startConstructionRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "endConstructionRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.CompletedConstructing,  //CompletedConstructing,

                ActionSetsKey = "endConstructionRemark"
            });

            #endregion

            #region combat
            list.Add(new AgentActionHook()
            {
                KeyName = "humanKilledEnemy",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledEnemyRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanMissedEnemy",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.MissedAnAttackOnAnEnemy,

                ActionSetsKey = "humanMissedEnemyRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanHitEnemy",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitEnemyRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanTakingAHit",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.TakingAHit,

                ActionSetsKey = "humanHitByEnemyRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanKilledInCombat",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.KilledInCombat,

                ActionSetsKey = "humanKilledInCombatRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanDied",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.DiedOnPlaySite,

                ActionSetsKey = "increaseDeathCount"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanFleeing",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.Fleeing,

                ActionSetsKey = "humanFleeingRemark"
            });

            #endregion

            #region other
            /*
                  EventActionsWhenEating = GameData.Instance.AllActionSets["humanEatingRemark"],
                     EventActionsWhenGoingToSleep = GameData.Instance.AllActionSets["humanGoingToSleepRemark"],
                     EventActionsOnStartConstructing = GameData.Instance.AllActionSets["startConstructionRemark"],
                     EventActionsOnCompletedConstruction = GameData.Instance.AllActionSets["endConstructionRemark"],
                     EventActionsOnStartHarvesting = GameData.Instance.AllActionSets["startHarvestRemark"],
                     EventActionsOnCompletedHarvesting = GameData.Instance.AllActionSets["endHarvestRemark"],
                     */
            list.Add(new AgentActionHook()
            {
                KeyName = "humanEatingRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.Eating,

                ActionSetsKey = "humanEatingRemark"
            });

            list.Add(new AgentActionHook()
            {
                KeyName = "humanGoingToSleepRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.GoingToSleep,

                ActionSetsKey = "humanGoingToSleepRemark"
            });

            #endregion

           

            return list;

        }

        /// <summary>
        /// attack type specific - these will override the agent combat hooks
        /// </summary>
        public static List<AttackTypeActionHook> InitAttackTypeHooks()
        {
            List<AttackTypeActionHook> list = new List<AttackTypeActionHook>();

            /*
                EventActionsWhenHittingAnEnemy = GameData.Instance.AllActionSets["humanHitEnemyWithRifleRemark"], //will override speaks defined in creatureloader under attacks
                    EventActionsWhenKilledAnEnemy = GameData.Instance.AllActionSets["humanKilledEnemyWithRifleRemark"],
                    EventActionsWhenMissingAnAttackOnAnEnemy = GameData.Instance.AllActionSets["humanMissedEnemyWithRifleRemark"],
                    EventActionsWhenHittingPrey = GameData.Instance.AllActionSets["humanHitPreyWithRifleRemark"],
                    EventActionsWhenKilledPrey = GameData.Instance.AllActionSets["humanKilledPreyWithRifleRemark"],
                    EventActionsWhenMissingAnAttackOnPrey = GameData.Instance.AllActionSets["humanMissedPreyWithRifleRemark"], 
             */
            //personPunchLowRight
            #region human barehanded fight

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithLowPunch",
                TypeKey = "personPunchLowRight",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitEnemyWithPunchRemark" //same remark as the other kind of punch
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithHighPunch",
                TypeKey = "personPunchHighRight",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitEnemyWithPunchRemark"
            });

            //

            #endregion

            //mp: the shoot weapon talk does not fire...
            #region human shoot bow


            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledEnemyWithImprovisedBasicArrow",
                TypeKey = "shootImprovisedBasicArrow",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledPreyWithImprovisedBasicArrow",
                TypeKey = "shootImprovisedBasicArrow",
                Hook = AgentActionHooks.KilledPrey,

                ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledEnemyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"  //same talk as basic.
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledPreyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.KilledPrey,

                ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"  //same talk as basic.
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledEnemyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledWithImprovisedMetalArrowRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledPreyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.KilledPrey,

                ActionSetsKey = "humanKilledWithImprovisedMetalArrowRemark"
            });


            //
            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithImprovisedBasicArrow",
                TypeKey = "shootImprovisedBasicArrow",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark"  //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitPreyWithImprovisedBasicArrow",
                TypeKey = "shootImprovisedBasicArrow",
                Hook = AgentActionHooks.HitPrey,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark"  //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark" //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitPreyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.HitPrey,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark" //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark"  //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitPreyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.HitPrey,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark"  //same  remarks for all arrows when hitting
            });

            #endregion
            #region human shoot bushdragon poison

           
            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithImprovedFireExtinguisherBushDragonPoison",
                TypeKey = "shootImprovedFireExtinguisherBushDragonPoison",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitEnemyWithBushDragonPoisonRemark"  //
            });
           
            #endregion

            #region human shoot rifle

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledEnemyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledEnemyWithRifleRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitEnemyWithRifleRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanMissedEnemyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.MissedAnAttackOnAnEnemy,

                ActionSetsKey = "humanMissedEnemyWithRifleRemark"
            });


            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledPreyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.KilledPrey,

                ActionSetsKey = "humanKilledPreyWithRifleRemark"
            });


            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitPreyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.HitPrey,

                ActionSetsKey = "humanHitPreyWithRifleRemark"
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanMissedPreyWithRifle",
                TypeKey = "shootCoilRifle",
                Hook = AgentActionHooks.MissedAnAttackOnPrey,

                ActionSetsKey = "humanMissedPreyWithRifleRemark"
            });
            #endregion

            return list;

            // BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllAttackTypeEventHooks, "", "attackTypeEventHooks.xml", Config.DataType.BaseData);

        }

        /// <summary>
        /// Process specific - these will override the agent hooks:
        /// </summary>
        public static List<ProcessTypeActionHook> InitProcessTypeHooks()
        {
            List<ProcessTypeActionHook> list = new List<ProcessTypeActionHook>();

            // many of these belong to the scenario, not in the vanilla data

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "salvageTopProducePropellerDome",
                TypeKey = "salvageSkimmerEngineTop",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_producePropellerDome"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "salvageSideProducePropellerDome",
                TypeKey = "salvageSkimmerEngineSide",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_producePropellerDome"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endCannibalizeFieldLab", //sets a bool property
                TypeKey = "salvageFieldLabPacked",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "cannibalizeFieldLabSetProperty"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endCannibalizeFieldLabRemark",
                TypeKey = "salvageFieldLabPacked",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "cannibalizeFieldLabRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DEMOISLANDMAP_makeImprovisedCookingPot",
                TypeKey = "makeImprovisedCookingPot",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_makeImprovisedCookingPot"
            });




            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DEMOISLANDMAP_produceBushdragonPoisonGlands",
                TypeKey = "butcherBushDragon",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_produceBushdragonPoisonGlands"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DEMOISLANDMAP_makeBushDragonCartridge",
                TypeKey = "makeBushDragonCartridge",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_makeBushDragonCartridge"
            });


            //


/* mp no longer used
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DEMOISLANDMAP_produceBushdragonPoison",
                TypeKey = "makeBushDragonPoison",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_produceBushdragonPoison"
            });
*/

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DEMOISLANDMAP_produceSpoakBranches",
                TypeKey = "harvestSpoakBranches",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "DEMOISLANDMAP_produceSpoakBranches"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "produceCampfire",
                TypeKey = "constructCampfire",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "produceCampfire"

            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "produceAbatis",
                TypeKey = "constructAbatis1",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "produceAbatis"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startUseSulfurSmokeBombRemark",
                TypeKey = "useSulfurSmokeBomb",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startUseSulfurSmokeBombRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "sulfurBombActivated",
                TypeKey = "useSulfurSmokeBomb",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "sulfurBombActivated"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "sulfurBombActivatedRemark",
                TypeKey = "useSulfurSmokeBomb",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "sulfurBombActivatedRemark"
            });
            #region rat nest Hooks
            /*
            list.Add(new ProcessTypeActionHook() //bso
            {
                KeyName = "startDigUpRatNest",
                TypeKey = "digUpRatNest",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "destroyRatNestTalk"
            });*/
            #endregion
            #region dig up rat nest
          /*list.Add(new ProcessTypeActionHook() //bso
            {
                KeyName = "detectFishTrapCoastRemark",
                TypeKey = "digUpRatNest",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "detectFishTrapCoastRemark"
            });*/
            #endregion

            #region Farm hooks //mp nov 2015 put this in again only if you test it properly.
       /*     list.Add(new ProcessTypeActionHook()
            {
                KeyName = "harvestCropsRemark",
                TypeKey = "harvestCrops",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "harvestCropsRemark"
            });*/
            #endregion
            //////////
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startRetrieveCratesRemark",
                TypeKey = "retrieveCrates",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startRetrieveCratesRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endRetrieveCrates",
                TypeKey = "retrieveCrates",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endRetrieveCrates"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endRetrieveCratesRemark",
                TypeKey = "retrieveCrates",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endRetrieveCratesRemark"
            });
            //////////

            list.Add(new ProcessTypeActionHook() // the talk cooldown needs to end before endrescue, otherwise a) extend process b) remove startRescue
            {
                KeyName = "startRescueColleagueRemark",
                TypeKey = "rescueColleague",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startRescueColleagueRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endRescueColleague",
                TypeKey = "rescueColleague",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endRescueColleague"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endRescueColleagueRemark",
                TypeKey = "rescueColleague",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endRescueColleagueRemark"
            });

            #region rope bridge construct //not used
/*
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startBuildRopeBridgeRemark",
                TypeKey = "buildRopeBridgeVine",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startBuildRopeBridgeRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "ropeBridgeFinished",
                TypeKey = "buildRopeBridgeVine",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "ropeBridgeFinished"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "ropeBridgeFinishedRemark",
                TypeKey = "buildRopeBridgeVine",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "ropeBridgeFinishedRemark"
            });
*/
            #endregion



            /*   KeyName = "constructA-frameTarp",
                RequiredSkill = "bushcraft",
                PhysicalWorkFactor = constructionExertion,
                Stances = constructionStances,
                 
                EventActionsOnCompletion = GameData.Instance.AllActionSets["produceShelter"],*/

            #region produce exotic/strange meal

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedPhantomWeaver",
                TypeKey = "makeRoastedPhantomWeaver",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedWebWing",
                TypeKey = "makeRoastedWebWing",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedCrestedFoiler",
                TypeKey = "makeRoastedCrestedFoiler",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedGoldenCenobite",
                TypeKey = "makeRoastedGoldenCenobite",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedMuckGrinder",
                TypeKey = "makeRoastedMuckGrinder",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedCrazyDweller",
                TypeKey = "makeRoastedCrazyDweller",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedDaggermouth",
                TypeKey = "makeRoastedDaggermouth",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startMakeRoastedImpEel",
                TypeKey = "makeRoastedImpEel",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startMakeStrangeAnimalMeal"
            });



            #endregion

            #region shelters
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "AFrameTarpProduceShelter",
                TypeKey = "constructA-frameTarp",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });

            /*   KeyName = "constructA-frameSpoakLeaves",
                RequiredSkill = "bushcraft",
                PhysicalWorkFactor = constructionExertion,
                Stances = constructionStances,
                              
                EventActionsOnCompletion = GameData.Instance.AllActionSets["produceShelter"],*/
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "AFrameSpoakLeavesProduceShelter",
                TypeKey = "constructA-frameSpoakLeaves",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "AFrameScrapsProduceShelter",
                TypeKey = "constructA-frameScraps",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DaysheenTipiProduceShelter",
                TypeKey = "constructDaysheenTipi",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "Lean-toTarpProduceShelter",
                TypeKey = "constructLean-toTarp",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "Lean-toSpoakLeavesProduceShelter",
                TypeKey = "constructLean-toSpoakLeaves",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "Lean-toScrapsProduceShelter",
                TypeKey = "constructLean-toScraps",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "DomeShelterTarpProduceShelter",
                TypeKey = "constructDomeShelterTarp",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });

            list.Add(new ProcessTypeActionHook()
            {                
                KeyName = "DomeShelterSpoakShinglesProduceShelter",
                TypeKey = "constructDomeShelterSpoakShingles",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endConstructDomeShelterSpoakShingles"
            });

/* //don't have wigwamtarp billboard asset
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "WigwamTarpProduceShelter",
                TypeKey = "constructWigwamTarp",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });
*/
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startConstructWigwamSpoakShingles", // "WigwamSpoakShinglesProduceShelter"
                TypeKey = "constructWigwamSpoakShingles",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructWigwamSpoakShingles"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endConstructWigwamSpoakShingles",
                TypeKey = "constructWigwamSpoakShingles",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endConstructWigwamSpoakShingles"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endConstructSensor",
                TypeKey = "constructSensor",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endConstructSensor"
            });


            #endregion

            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllProcessTypeEventHooks, "", "processTypeEventHooks.xml", Config.DataType.BaseData);

        }


    }
}

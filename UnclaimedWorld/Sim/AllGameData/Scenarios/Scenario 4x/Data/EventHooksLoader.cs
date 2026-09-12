using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data
{
    public class EventHooksLoader
    {
        public static List<EntityEventHook> InitEntityEventHooks()
        {
            List<EntityEventHook> list = new List<EntityEventHook>();


            return list;
        }


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


            list.Add(new AgentActionHook()
            {
                KeyName = "newMemberRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.DisembarkedNewPlayerAllegianceMember,

                ActionSetsKey = "SANDBOXMAP_newPlayerAllegianceMemberRemark"
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
                KeyName = "humanKilledEnemyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.KilledEnemy,

                ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"  //same talk as basic.
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanKilledEnemyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.KilledEnemy,

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
                KeyName = "humanHitEnemyWithImprovisedChitinousArrow",
                TypeKey = "shootImprovisedChitinousArrow",
                Hook = AgentActionHooks.HitEnemy,

                ActionSetsKey = "humanHitWithImprovisedArrowRemark" //same  remarks for all arrows when hitting
            });

            list.Add(new AttackTypeActionHook()
            {
                KeyName = "humanHitEnemyWithImprovisedMetalArrow",
                TypeKey = "shootImprovisedMetalArrow",
                Hook = AgentActionHooks.HitEnemy,

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

            #region special action

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "bigBombActivatedHook1",
                TypeKey = "useVarmintBomb",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "bigBombActivated"
            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "bigBombActivatedHook2",
                TypeKey = "useVarmintBomb",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "bigBombTalk"
            });

            #endregion

 
            
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endConstructWigwamSpoakShinglesHook",
                TypeKey = "constructWigwamSpoakShingles",
                Hook = AgentActionHooks.CompletedProducing,
                ActionSetsKey = "endConstructWigwamSpoakShingles"

            });
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "produceCampfire",
                TypeKey = "constructCampfire",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "produceCampfire"

            });

  
       

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

/* //don't have wigwamtarp billboard asset
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "WigwamTarpProduceShelter",
                TypeKey = "constructWigwamTarp",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startConstructShelter"
            });
*/
 


            #endregion

            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllProcessTypeEventHooks, "", "processTypeEventHooks.xml", Config.DataType.BaseData);

        }


    }
}

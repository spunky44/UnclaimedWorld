using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
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
                KeyName = "humanFleeing",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.Fleeing,

                ActionSetsKey = "humanFleeingRemark"
            });

            #endregion

            #region sleep

            list.Add(new AgentActionHook()
            {
                KeyName = "humanGoingToSleepRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.GoingToSleep,

                ActionSetsKey = "humanGoingToSleepRemark"
            });

            #endregion


            //used in tutorial, eating commonOilTubers:
            #region eating
            list.Add(new AgentActionHook()
            {
                KeyName = "humanEatingRemark",
                TypeKey = "entity:human",
                Hook = AgentActionHooks.Eating,

                ActionSetsKey = "humanEatingRemark"
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





            #region salvage boat

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startSalvageBoatWreckRemark",
                TypeKey = "salvageBoatWreck",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startSalvageBoatWreckRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "endSalvageBoatWreckRemark",
                TypeKey = "salvageBoatWreck",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "endSalvageBoatWreckRemark"
            });




            #endregion   

            #region rope bridge construct

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startBuildRopeBridgeRemark",
                TypeKey = "buildRopeBridge",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startBuildRopeBridgeRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "ropeBridgeFinished",
                TypeKey = "buildRopeBridge",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "ropeBridgeFinished"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "ropeBridgeFinishedRemark",
                TypeKey = "buildRopeBridge",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "ropeBridgeFinishedRemark"
            });

            #endregion
/*
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "gatherFirewoodFinished",
                TypeKey = "gatherFirewood",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "gatherFirewoodFinished"
            });
*/


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructCampfireFinished",
                TypeKey = "constructCampfire",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "constructCampfireFinished"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "makeMashedCommonOilTubersFinished",
                TypeKey = "makeMashedCommonOilTubers",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "makeMashedCommonOilTubersFinished"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "makeImprovisedFlintSpearFinished",
                TypeKey = "makeImprovisedFlintSpear",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "makeImprovisedFlintSpearFinished"
            });


         /*   list.Add(new ProcessTypeActionHook()
            {
                KeyName = "makeFlintSpearheadFinished",
                TypeKey = "makeFlintSpearhead",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "makeFlintSpearheadFinished"
            });*/


            //
            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "constructSignalPyreFinished",
                TypeKey = "constructSignalPyre",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "constructSignalPyreFinished"
            });



            #region light signal pyre

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "startLightSignalPyreRemark",
                TypeKey = "lightSignalPyre",
                Hook = AgentActionHooks.StartProducing,

                ActionSetsKey = "startLightSignalPyreRemark"
            });


            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "lightSignalPyreFinished",
                TypeKey = "lightSignalPyre",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "lightSignalPyreFinished"
            });

            list.Add(new ProcessTypeActionHook()
            {
                KeyName = "lightSignalPyreRemark",
                TypeKey = "lightSignalPyre",
                Hook = AgentActionHooks.CompletedProducing,

                ActionSetsKey = "lightSignalPyreRemark"
            });

            #endregion



 





            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllProcessTypeEventHooks, "", "processTypeEventHooks.xml", Config.DataType.BaseData);

        }


    }
}

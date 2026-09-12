using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.ClientSide.Interface.World_map;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide
{
    public delegate void GoalPlanner(Entity entity, DebugJobEvaluator eval);


    public class PlaceGameEntities
    {
        /// //The top of this list will be used!////////
        /// 
        private static DebugScenarios GetDefaultScenario()
        {
            return DebugScenarios.TwinklerEatTest;

            return DebugScenarios.UpgradeTest;

            return DebugScenarios.ShelterTest;

          //  return DebugScenarios.FightTest;

            return DebugScenarios.HuntTest;

            return DebugScenarios.LightingTest;

            //  return DebugScenarios.MudBrick;

            //return DebugScenarios.HuntTest;


            return DebugScenarios.EatTest;

            // return DebugScenarios.HuntTest;

            return DebugScenarios.ThreatTest;

            //return DebugScenarios.AllItemsTest;

            //return DebugScenarios.MudBrick;


            // return DebugScenarios.ThreatTest;
            return DebugScenarios.PeatTest;


            return DebugScenarios.UpgradeTest;

            return DebugScenarios.StationaryToolTest;


            return DebugScenarios.SleepInShiftsTest;



            return DebugScenarios.CookingTest;

            return DebugScenarios.ShelterTest;

            return DebugScenarios.RepairTest;


            return DebugScenarios.PierTest;

            return DebugScenarios.MudBrick;


            //  return DebugScenarios.ThreatTest;


            return DebugScenarios.NanomapTest;

            // return DebugScenarios.FarmTest;


            // return DebugScenarios.HarvestTest;

            // 
            return DebugScenarios.StructureGeoMap;

            return DebugScenarios.MudBrick;




            return DebugScenarios.FarmTest;

            return DebugScenarios.EatTest;


            return DebugScenarios.NanomapTest;
            return DebugScenarios.ThreatTest;



            // return DebugScenarios.FarmTest;
            return DebugScenarios.NanomapTest;

            return DebugScenarios.ThreatTest;

            return DebugScenarios.UpgradeTest;

        }


        private static Dictionary<DebugScenarios, Tuple<StartDebugScenarioParams, LoadDebugScenario>> loadScenarioFunctions = new Dictionary<DebugScenarios, Tuple<StartDebugScenarioParams, LoadDebugScenario>>();

        public delegate void LoadDebugScenario();

        public enum DebugScenarios
        {
            BushDragon,
            NeedsTest,
            WeaponsTest,
            EmptyMap,
            PlaceGameEntitiesMilestoneBuild,
            PlaceGameEntitiesTestMap,
            CollisionTest,
            TestMicroMap,
            MapApril2013,
            QuarterSizeMap,
            DemoIslandMap,
            QuarterSizeMap_Alt_Test_v1,
            BuildingSoundTest,
            BuildingTest,
            HarvestTest2,
            HarvestPickupTest,
            FindPreyTest,
            AnimTweak,
            ModelTest,
            WildernessCampScreenshot,
            HarvestTest,
            GeometryTest,
            CookingTest,
            MLoAnimationTest,
            FightTest,
            RobotTest,
            AssetGeometryMap,
            StructureGeoMap,
            ProductionTest,
            StorageTest,
            StorageTest2,
            ShelterTest,
            DecompositionTest,
            QuarterSizeMapBuildingTest,
            GaitTest,
            None,
            SoundTest,
            UnloadTest,
            AttackNestTest,
            ThreatTest,
            SkimmerSalvageTest,
            TwinklerEatTest,
            CampFireTest,
            RatEatTest,
            ChickenEatTest,
            AllStructuresBeingBuilt,
            DemonTreeTest,
            ButcherTest,
            SpikePlantTest,
            FarmTest,
            BushDragonParticleTest,
            FishTrapTest,
            ManyAgentsTest,
            LongTermFarmingTest,
            ScareCrowTest,
            ShaiHuludTest,
            PlotAssetTest,
            AmbientSoundTest,
            AllItemsTest,
            HuntTest,
            ItemsNotCarriedBugHunt,
            CarcassNoMeat,
            TrapTest,
            CommTest,
            mineShowcase,
            WorldMapTest,
            MudBrick,
            FuelTest,
            Recording,
            QuaditeTest,
            RemoveAttachablesBug,
            ReloadTest,
            MemoryTest,
            MapEdgeTest,
            SleepInShiftsTest,
            TurnipHutTest,
            BreedingTest,
            AnimTest,
            Compost,
            PathingTest,
            EatTest,
            StationaryToolTest,
            PierTest,
            MusicTest,
            DogEatTest,
            MidsizeTest,
            VerminTest,
            SkillTest,
            DetectionTest,
            RepairTest,
            ClayPitTest,
            UpgradeTest,
            NanomapTest,
            PeatTest,
            LightingTest
        }


        static PlaceGameEntities()
        {
            AddScenario(DebugScenarios.BushDragon, "d Mezzomap MLo", BushDragon);
            AddScenario(DebugScenarios.NeedsTest, "d Mezzomap MLo", NeedsTest);
            AddScenario(DebugScenarios.WeaponsTest, "d Mezzomap MLo", WeaponsTest);
            AddScenario(DebugScenarios.LightingTest, "d Mezzomap MLo", LightingTest);

            AddScenario(DebugScenarios.EmptyMap, "d Mezzomap MLo", EmptyMap);
            AddScenario(DebugScenarios.PlaceGameEntitiesMilestoneBuild, "d Mezzomap MLo", PlaceGameEntitiesMilestoneBuild);
            AddScenario(DebugScenarios.PlaceGameEntitiesTestMap, "d Mezzomap MLo", PlaceGameEntitiesTestMap);
            AddScenario(DebugScenarios.CollisionTest, "d Mezzomap MLo", CollisionTest);
            AddScenario(DebugScenarios.TestMicroMap, "d Mezzomap MLo", TestMicroMap);
            AddScenario(DebugScenarios.MapApril2013, "d Mezzomap MLo", MapApril2013);
            AddScenario(DebugScenarios.QuarterSizeMap, "d Mezzomap MLo", QuarterSizeMap);
            AddScenario(DebugScenarios.DemoIslandMap, "d Mezzomap MLo", DemoIslandMap);
            AddScenario(DebugScenarios.QuarterSizeMap_Alt_Test_v1, "d Mezzomap MLo", QuarterSizeMap_Alt_Test_v1);
            AddScenario(DebugScenarios.BuildingSoundTest, "d Mezzomap MLo", BuildingSoundTest);
            AddScenario(DebugScenarios.BuildingTest, "d Mezzomap MLo", BuildingTest);
            AddScenario(DebugScenarios.HarvestTest2, "d Mezzomap MLo", HarvestTest2);
            AddScenario(DebugScenarios.HarvestPickupTest, "d Mezzomap MLo", HarvestPickupTest);
            AddScenario(DebugScenarios.FindPreyTest, "d Mezzomap MLo", FindPreyTest);
            AddScenario(DebugScenarios.AnimTweak, "d Mezzomap MLo", AnimTweak);
            AddScenario(DebugScenarios.ModelTest, "d Mezzomap MLo", ModelTest);
            AddScenario(DebugScenarios.WildernessCampScreenshot, "d Mezzomap MLo", WildernessCampScreenshot);
            AddScenario(DebugScenarios.HarvestTest, "d Mezzomap MLo", HarvestTest);
            AddScenario(DebugScenarios.GeometryTest, "d Mezzomap MLo", GeometryTest);
            AddScenario(DebugScenarios.CookingTest, "d Mezzomap MLo", CookingTest);
            AddScenario(DebugScenarios.VerminTest, "d Mezzomap MLo", VerminTest);
            AddScenario(DebugScenarios.PathingTest, "d Mezzomap MLo", PathingTest);
            AddScenario(DebugScenarios.MLoAnimationTest, "d Mezzomap MLo", MLoAnimationTest);
            AddScenario(DebugScenarios.FightTest, "d Mezzomap MLo", FightTest);
            AddScenario(DebugScenarios.AssetGeometryMap, "d Mezzomap MLo", AssetGeometryMap);
            AddScenario(DebugScenarios.StructureGeoMap, "d Mezzomap MLo", StructureGeoMap);
            AddScenario(DebugScenarios.ProductionTest, "d Mezzomap MLo", ProductionTest);
            AddScenario(DebugScenarios.AnimTest, "d Mezzomap MLo", AnimTest);
            AddScenario(DebugScenarios.CampFireTest, "d Mezzomap MLo", CampfireTest);
            AddScenario(DebugScenarios.StorageTest, "d Mezzomap MLo", StorageTest);
            AddScenario(DebugScenarios.StorageTest2, "d Mezzomap MLo", StorageTest2);
            AddScenario(DebugScenarios.ShelterTest, "d Mezzomap MLo", ShelterTest);
            AddScenario(DebugScenarios.DecompositionTest, "d Mezzomap MLo", DecompositionTest);
            AddScenario(DebugScenarios.GaitTest, "d Mezzomap MLo", GaitTest);
            AddScenario(DebugScenarios.UnloadTest, "d Mezzomap MLo", UnloadTest);
            AddScenario(DebugScenarios.QuarterSizeMapBuildingTest, "d Mezzomap MLo", QuarterSizeMapBuildingTest);
            AddScenario(DebugScenarios.AttackNestTest, "d Mezzomap MLo", AttackNestTest);
            AddScenario(DebugScenarios.SkimmerSalvageTest, "d Mezzomap MLo", SkimmerSalvageTest);
            AddScenario(DebugScenarios.TwinklerEatTest, "d Mezzomap MLo", TwinklerEatTest);
            AddScenario(DebugScenarios.RatEatTest, "d Mezzomap MLo", RatEatTest);
            AddScenario(DebugScenarios.ChickenEatTest, "d Mezzomap MLo", ChickenEatTest);
            AddScenario(DebugScenarios.AllStructuresBeingBuilt, "d Mezzomap MLo", AllStructuresBeingBuilt);
            AddScenario(DebugScenarios.DemonTreeTest, "d Mezzomap MLo", DemonTreeTest);
            AddScenario(DebugScenarios.ButcherTest, "d Mezzomap MLo", ButcherTest);
            AddScenario(DebugScenarios.SpikePlantTest, "d Mezzomap MLo", SpikePlantTest);
            AddScenario(DebugScenarios.FarmTest, "d Mezzomap MLo", FarmTest);
            AddScenario(DebugScenarios.NanomapTest, "Nanomap", NanomapTest);
            AddScenario(DebugScenarios.BushDragonParticleTest, "d Mezzomap MLo", BushDragonParticleTest);
            AddScenario(DebugScenarios.FishTrapTest, "d Mezzomap MLo", FishTrapTest); //The.Map.AllMaps[1]
            AddScenario(DebugScenarios.ManyAgentsTest, "d Mezzomap MLo", ManyAgentsTest);
            AddScenario(DebugScenarios.MemoryTest, "d Mezzomap MLo", MemoryTest);
            AddScenario(DebugScenarios.ScareCrowTest, "d Mezzomap MLo", ScareCrowTest);
            AddScenario(DebugScenarios.LongTermFarmingTest, "d Mezzomap MLo", LongTermFarmingTest);
            AddScenario(DebugScenarios.SoundTest, "d Mezzomap MLo", SoundTest);
            AddScenario(DebugScenarios.ShaiHuludTest, "d Mezzomap MLo", ShaiHuludTest);
            AddScenario(DebugScenarios.PlotAssetTest, "d Mezzomap MLo", PlotAssetTest);
            AddScenario(DebugScenarios.AmbientSoundTest, "d Mezzomap MLo", AmbientSoundTest);
            AddScenario(DebugScenarios.AllItemsTest, "d Mezzomap MLo", AllItemsTest);
            AddScenario(DebugScenarios.HuntTest, "d Mezzomap MLo", HuntTest);
            AddScenario(DebugScenarios.ItemsNotCarriedBugHunt, "d Mezzomap MLo", ItemsNotCarriedBugHunt);
            AddScenario(DebugScenarios.RobotTest, "d Mezzomap MLo", RobotTest);
            AddScenario(DebugScenarios.ReloadTest, "d Mezzomap MLo", ReloadTest);
            AddScenario(DebugScenarios.CarcassNoMeat, "d Mezzomap MLo", CarcassNoMeat);
            AddScenario(DebugScenarios.TrapTest, "d Mezzomap MLo", TrapTest);
            AddScenario(DebugScenarios.mineShowcase, "k Map Oasis Valley 2016", mineShowcase);
            AddScenario(DebugScenarios.WorldMapTest, "d Mezzomap MLo", WorldMapTest);
            AddScenario(DebugScenarios.MudBrick, "d Mezzomap MLo", MudBrickTest);
            AddScenario(DebugScenarios.MusicTest, "d Mezzomap MLo", MusicTest);
            AddScenario(DebugScenarios.FuelTest, "d Mezzomap MLo", FuelTest);
            AddScenario(DebugScenarios.Recording, "d Mezzomap MLo", Recording);
            AddScenario(DebugScenarios.QuaditeTest, "d Mezzomap MLo", QuaditeTest);
            AddScenario(DebugScenarios.RemoveAttachablesBug, "d Mezzomap MLo", RemoveAttachablesBug);
            AddScenario(DebugScenarios.DetectionTest, "d Mezzomap MLo", DetectionTest);
            AddScenario(DebugScenarios.MapEdgeTest, "d Mezzomap MLo", MapEdgeTest);
            AddScenario(DebugScenarios.SleepInShiftsTest, "d Mezzomap MLo", SleepInShiftsTest);
            AddScenario(DebugScenarios.TurnipHutTest, "d Mezzomap MLo", TurnipHutTest);
            AddScenario(DebugScenarios.BreedingTest, "d Mezzomap MLo", BreedingTest);
            AddScenario(DebugScenarios.Compost, "d Mezzomap MLo", Compost);
            AddScenario(DebugScenarios.ThreatTest, "d Mezzomap MLo", ThreatTest);
            AddScenario(DebugScenarios.EatTest, "d Mezzomap MLo", EatTest);
            AddScenario(DebugScenarios.SkillTest, "d Mezzomap MLo", SkillTest);
            AddScenario(DebugScenarios.DogEatTest, "d Mezzomap MLo", DogEatTest);
            AddScenario(DebugScenarios.StationaryToolTest, "d Mezzomap MLo", StationaryToolTest);
            AddScenario(DebugScenarios.CommTest, "d Mezzomap MLo", CommTest);
            AddScenario(DebugScenarios.PierTest, "d Mezzomap MLo", PierTest);
            AddScenario(DebugScenarios.MidsizeTest, "jx Map Hills Rivers Small", MidsizeTest); //"k Map Oasis Valley 2016"
            AddScenario(DebugScenarios.RepairTest, "d Mezzomap MLo", RepairTest);
            AddScenario(DebugScenarios.ClayPitTest, "d Mezzomap MLo", ClayPitTest);
            AddScenario(DebugScenarios.UpgradeTest, "d Mezzomap MLo", UpgradeTest);
            AddScenario(DebugScenarios.PeatTest, "Nanomap", PeatTest);


        }

        static void AddScenario(DebugScenarios key, string map, LoadDebugScenario placeMethod)
        {
            StartDebugScenarioParams parms = new StartDebugScenarioParams()
            {
                ScenarioKey = key,
                MapKey = map
            };

            loadScenarioFunctions.Add(key, new Tuple<StartDebugScenarioParams, LoadDebugScenario>(parms, placeMethod));

        }

        public static void BeginRun(DebugScenarios scenarioToRun)
        {
            loadScenarioFunctions[scenarioToRun].Item2.Invoke();
        }


        public static StartDebugScenarioParams GetNewScenarioParams()
        {
            // make a copy since we want to snapshot it:
            StartDebugScenarioParams parms = loadScenarioFunctions[GetDefaultScenario()].Item1;

            StartDebugScenarioParams newParms = new StartDebugScenarioParams() { MapKey = parms.MapKey, ScenarioKey = parms.ScenarioKey };
            return newParms;

            //return loadScenarioFunctions[GetDefaultScenario()].Item1;

        }



        public static void BreedingTest()
        {
            /*  if (!The.Sim.LoadMap("j Map SandboxNomads March 2015"))//("d Mezzomap MLo")) //QuaditeType
                  return;*/

            Point spot = new Point(6, 6);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(spot.X, spot.Y);
            for (int i = 0; i < 100; i++)
            {
                Entity human = PlacePerson("Charles", "Jacobi" + i, Reproduction.Male, spot, Color.White, 40f, false, expedition);
            }
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), spot);

            //AddFinishedStructure("structure:chickenFarm", new Point(spot.X - 0, spot.Y + 3), expedition, false);
        }

        public static void DetectionTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                  return;*/

            Point spot = new Point(6, 6);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(spot.X, spot.Y);


            UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
            Expedition exp1 = new Expedition(all1, "Start2", "Start2", MapManager.TileToWorldPos(spot));

            Point treeSpot = new Point(spot.X - 0, spot.Y - 1);
            // AddTree("tree:marshcotflower", treeSpot, "crop:marshcotSap", 4);
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), treeSpot);
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), treeSpot);

            // AddFinishedStructure("structure:clayHut", new Point(11, 6), expedition, false);


            for (int i = 0; i < 5; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);

            }

            for (int i = 0; i < 1; i++)
            {
                //  Entity e1 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(spot.X - 3, spot.Y + 0), 20, all1, null, exp1);
                //  e1.Locomotor.ToggleImmobilize();
            }

            GetBob(spot, expedition);

            //AddFinishedStructure("structure:sensor", new Point(spot.X - 0, spot.Y + 1), expedition, false);

            // AddFinishedStructure("structure:simpleSmithy", new Point(spot.X - 0, spot.Y - 1), expedition, false);
            //  AddTree("tree:marshcotflower", new Point(spot.X - 0, spot.Y - 1), "crop:marshcotSap", 4);


        }

        public static void RemoveAttachablesBug()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                  return;*/

            Point spot = new Point(6, 6);
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(spot.X, spot.Y);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), spot);

            UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
            Expedition exp1 = new Expedition(all1, "Start2", "Start2", MapManager.TileToWorldPos(spot));

            for (int i = 0; i < 1; i++)
            {
                Entity e1 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(spot.X - 1, spot.Y + 0), 20, all1, null, exp1);
                e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
            }
            AddFinishedStructure("structure:sensor", new Point(spot.X - 0, spot.Y + 1), expedition, false);
        }

        public static void QuaditeTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                 return;*/

            Point spot = new Point(6, 6);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            AddFinishedStructure("structure:sensor", new Point(spot.X - 0, spot.Y + 1), expedition, false);

            The.MapUI.ZoomToMapPosition(6, 6);
            //Entity human = PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);

            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), spot);

            Allegiance enemyAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
            Allegiance enemyAllegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);
            Expedition exp1 = new Expedition(enemyAllegiance, "Start2", "Start2", MapManager.TileToWorldPos(spot));

            //Entity e2 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Female, spot, 20, enemyAllegiance2);

            for (int i = 0; i < 5; i++)
            {
                Entity e1 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Female, new Point(spot.X - 4, spot.Y + 0), 20, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, exp1);
                e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
                Entity e2 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Female, new Point(spot.X - 4, spot.Y + 0), 20, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, exp1);
                Entity e3 = PlaceAnimal("entity:studdedThunderChicken", Reproduction.Female, new Point(spot.X - 4, spot.Y + 0), 20, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, exp1);
            }

            /*
            Entity e2 = PlaceAnimal("entity:fieldQuadite", Reproduction.Female, new Point(spot.X + 2, spot.Y), 20, enemyAllegiance, null, exp1);
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
            e2.ImpairMovement(1f);*/
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:musketoon"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), spot);

            //Entity hole = AddFinishedStructure("structure:storageHole", new Point(6, 7), expedition, false);

            //  AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]),e1);
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), spot);
        }

        public static void MapEdgeTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                  return;*/

            Point campSpot = new Point(The.Map.mapTileWidth - 1, The.Map.mapTileHeight - 1);
            The.MapUI.ZoomToMapPosition(campSpot.X, campSpot.Y);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campSpot));
            Entity human = PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(campSpot.X - 4, campSpot.Y - 4), Color.White, 40f, false, expedition);

            UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
            Expedition exp1 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(campSpot));

            int t = 4;
            for (int i = 0; i < t; i++) // use this to spawn multiple creatures, t = amount of creatures
            {
                Entity e2 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(campSpot.X - 0, campSpot.Y), 18, all2, null, exp1);
                Entity e3 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(campSpot.X, campSpot.Y), 18, all2, null, exp1);
            }
        }


        static void FuelTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                 return;*/

            Point campSpot = new Point(5, 5);
            The.MapUI.ZoomToMapPosition(campSpot.X, campSpot.Y);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campSpot));
            Entity human = PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(campSpot.X + 1, campSpot.Y + 1), Color.White, 40f, false, expedition);
            /*   human.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
               human.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f
               human.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f
               human.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;  //0.2f
               */
            human = PlacePerson("2", "2", Reproduction.Male, new Point(campSpot.X + 1, campSpot.Y + 1), Color.White, 40f, false, expedition);

            AddFinishedStructure("structure:kilnImprovisedSmall", new Point(4, 5), expedition, false);
            AddFinishedStructure("structure:kilnImprovisedSmall", new Point(3, 5), expedition, false);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:unfiredClayPot"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:unfiredClayPot"]), campSpot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);

        }


        public static void Compost()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                 return;*/

            Point spot = new Point(6, 6);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(6, 6);

            Entity human = PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetFirewood"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:rottenVegetables"]), spot);


            AddFinishedStructure("structure:firewoodStack", new Point(7, 7), expedition, false);
            AddFinishedStructure("structure:compostBin", new Point(4, 5), expedition, false);

        }


        public static void UpgradeTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                  return;*/
            The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear() { Year = 206, Day = 0, TimeOfDay = 0.46 });

            Point spot = new Point(9, 6);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(10, 6);

            //            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);


            Entity hut = AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(4, 6)));

            UpgradeCategory cat;

            cat = GameData.Instance.AllUpgradeCategories["furniture4People"];
            PlaceGameEntities.AddColonyItemUpgrade("item:furniture4People", hut, cat);
            expedition.OwnedEntities.SetUpgrade(hut.EntityID, cat, GameData.Instance.AllEntityTypes["item:furniture4People"]);

            cat = GameData.Instance.AllUpgradeCategories["bedsOrMats4People"];
            PlaceGameEntities.AddColonyItemUpgrade("item:beds4People", hut, cat);
            expedition.OwnedEntities.SetUpgrade(hut.EntityID, cat, GameData.Instance.AllEntityTypes["item:beds4People"]);


            AddFinishedStructure("structure:octagonalTent", null, expedition, false, MapManager.TileToWorldPos(new Point(8, 6)));

            hut = AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(18, 6)));
            /* hut.DoDamage(0.5f);
             */
            GetBob(null, expedition, hut);

            hut = AddFinishedStructure("structure:caneHut", null, expedition, false, MapManager.TileToWorldPos(new Point(7, 6)));

            hut = AddFinishedStructure("structure:domeShelterTarp", null, expedition, false, MapManager.TileToWorldPos(new Point(7, 4)));
            hut = AddFinishedStructure("structure:campfire", null, expedition, false, MapManager.TileToWorldPos(new Point(9, 4)));

            hut = AddFinishedStructure("structure:turnipHut", null, expedition, false, MapManager.TileToWorldPos(new Point(8, 8)));

            Entity workshop = AddFinishedStructure("structure:workshopBuilding", null, expedition, false, MapManager.TileToWorldPos(new Point(8, 10)));
            //workshop.DoDamage(1f);

            AddFinishedStructure("structure:workshopBuilding", null, expedition, false, MapManager.TileToWorldPos(new Point(11, 10)));
            AddFinishedStructure("structure:workshopBuilding", null, expedition, false, MapManager.TileToWorldPos(new Point(14, 10)));
            AddFinishedStructure("structure:workshopBuilding", null, expedition, false, MapManager.TileToWorldPos(new Point(17, 10)));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:extrusionMachineComponents"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:loomComponents"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"]), spot);

            /*new Input() { Entity = "item:metalLatheComponents", IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1 } }, // the major components
                                     new Input() { Entity = "item:humanPowerUnit", IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1 } },
                                     new Input() { Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, // for toolheads
                                    new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, // for various bits and pieces                                
                                    new Input(){ Entity = "item:thunderChickenTannedHide",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }} //for the belt drive       
              */

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalLatheComponents"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:humanPowerUnit"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:loomComponents"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:furniture"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurPowder"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vinegar"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWorkersToolbox"]), spot);

            // GetBob(spot, expedition);
            /* GetBob(spot, expedition);
             GetBob(spot, expedition);
             GetBob(spot, expedition);
             GetBob(spot, expedition);*/
            //PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stove"]), spot);

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
              */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), spot);


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), spot);

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:groundScanner"]), spot);


            //The.Map.GetTile(spot).AddResource("salt", 4);

            //AddTree("tree:marshcotflower", new Point(1, 8), "crop:marshcotSap", 4);


        }

        public static void StationaryToolTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                  return;*/
            The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear() { Year = 206, Day = 0, TimeOfDay = 0.46 });

            Point spot = new Point(4, 6);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(10, 6);

            Entity hut = AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(10, 6)));
            /* hut.DoDamage(0.5f);
             */
            // GetBob(null, expedition, hut);
            GetBob(spot, expedition);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);


            // GetBob(spot, expedition);
            /* GetBob(spot, expedition);
             GetBob(spot, expedition);
             GetBob(spot, expedition);
             GetBob(spot, expedition);*/
            //PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);

            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), spot);
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);
            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);
             */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(14, 14));

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:groundScanner"]), spot);


            //The.Map.GetTile(spot).AddResource("salt", 4);

            AddTree("tree:marshcotflower", new Point(1, 8), "crop:marshcotSap", 1);
            AddTree("tree:marshcotflower", new Point(1, 6), "crop:marshcotSap", 1);
            AddTree("tree:marshcotflower", new Point(1, 4), "crop:marshcotSap", 1);
            AddTree("tree:marshcotflower", new Point(2, 4), "crop:marshcotSap", 1);


        }

        static void AddTree(string key, Point spot, string resourceKey = null, int? resourceAmount = null)
        {
            Entity entity = new Entity(GameData.Instance.AllEntityTypes[key]);
            AddTerrainItem(entity, spot);

            if (resourceKey != null)
            {
                Trees.Tree treeComponent;
                entity.Find(out treeComponent);

                treeComponent.Crops[GameData.Instance.AllResourceTypes[resourceKey]].SetResourceItems(resourceAmount.Value);
            }
        }

        public static void MusicTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                 return;*/

            Point spot = new Point(6, 6);
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(6, 6);

            /*   GameData.Instance.AllPolledEvents.Add("SANDBOXNOMADMAP_song1", 
                   new PolledEventType()
                      {
                          KeyName = "SANDBOXNOMADMAP_song1", // scenario starts at TimeOfDay = 0.50   nighttime starts at 1.0, morning around 1.7?
                          ConditionSet = new ConditionSet()
                          {
                              Value = new TimeCondition()
                                  {
                                      RelativeNoOfDays = 0.0
                                    //  Date = new DateAndTime.TimeDateYear() { Year = 0, Day = 1, TimeOfDay = 0.5 }
                                  }
                          }
                          ,
                          ActionSets = new ActionSets()
                          {
                              SetsOfActions = new []{ new ActionSetType("09535dtyud56udt6udtryutdytyrwye28d")
                              {                       
                                  Actions = new EventActionType[]{
          
                                      new EventActionType("faf18teyudtyyyyyyyteyuteyutes3bab")
                                      {
                                              MusicAction = new ClientSide.GameEvents.MusicAction()
                                              {
                                                  Song = "Jesper Lundager - Building a Home_320" //duration number of days = 0,189
                                              }
                                      }}}}
                          }
                      });

               GameData.Instance.AllPolledEvents.Add("SANDBOXNOMADMAP_song2",
                   new PolledEventType()
              {
                  KeyName = "SANDBOXNOMADMAP_song2",
                  ConditionSet = new ConditionSet()
                  {
                      Value = new TimeCondition()
                          {
                              RelativeTimeInSeconds = new ValueNode() { Decimal = 20 }

                          }
                  }
                  ,
                  ActionSets = new ActionSets()
                  {
                      SetsOfActions = new []{ new ActionSetType("5dtyu6578u6yduduydt6udjfbe5")
                      {                       
                          Actions = new EventActionType[]{
          
                              new EventActionType("32edghjdddyjud6678tyufjdtyudtud65j458")
                              {
                                      MusicAction = new ClientSide.GameEvents.MusicAction()
                                      {
                                          Song = "Jesper Lundager - Prosperous Frontier_320" //duration 0,162
                                      }
                              }}}}
                  }
              });
              */
            foreach (var item in GameData.Instance.AllPolledEvents)
            {
                item.Value.LoadContent(The.Client.Content);
            }

            The.Sim.PlaySite.EventManager.AddPolledEvent("SANDBOXNOMADMAP_song1");
            The.Sim.PlaySite.EventManager.AddPolledEvent("SANDBOXNOMADMAP_song2");


        }

        public static void MudBrickTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //QuaditeType
                 return;*/

            Point spot = new Point(6, 2);
            //  The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));
            The.MapUI.ZoomToMapPosition(6, 6);

            /*   expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
               expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
               expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
               */

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);


            //  PlaceRobot("entity:haulingRobot", spot, The.Sim.PlaySite.PlayerAllegiance, expedition);

            spot = new Point(5, 2);
            // spot = new Point(16, 8);
            /*  for (int i = 0; i < 2; i++)
              {*/
            /* Entity human = PlacePerson("BB", "8", Reproduction.Male, spot, Color.White, 40f, false, expedition);
             human.PersonEntity.Personality.Principles[SimSide.Allegiances.Statistics.RatingTypes.Security] = 0.1f;
             */

            Entity bob = GetBob(spot, expedition);
            bob.Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
            bob = GetBob(spot, expedition);
            bob.Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
            bob = GetBob(spot, expedition);
            bob.Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
            bob = GetBob(spot, expedition);
            // bob.Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f; 



            /*  human = PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);
              human.PersonEntity.Personality.Principles[SimSide.Allegiances.Statistics.RatingTypes.Security] = 0.15f;
              */
            //  human.EntityType.SensorType.Range = 1000;
            //  }

            //   human = PlacePerson("Andrew", "Zimmern", Reproduction.Male, spot, Color.White, 40f, false, expedition);


            AddResource(new Point(2, 2), "firewood", 6);


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stoneHammer"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stoneHammer"]), spot);

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), spot);*/
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedTrowel"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);


            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), spot);
            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);
           */
            // "item:improvisedGreenHouseCover" //, IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 5 }},
            //         "item:shadeleafCanes" //, IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 3 }},

            //"item:steelSpade"
            /*   Entity hut = AddFinishedStructure("structure:clayHut", new Point(12, 10), expedition, false);
               hut.DoDamage(0.7f);
               */
            // Entity port = AddFinishedStructure("structure:simplePort", new Point(8, 10), expedition, false);

            /* for (int i = 0; i < 10; i++)
             {
                 AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), port, StorageCompartment.OfferedForTrade);
        
             }*/

            /*
            for (int i = 0; i < 5; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGreenHouseCover"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), spot);

            }
            */

            /*   spot = new Point(40, 2);
               The.Map.GetTile(spot).AddResource("salt", 8);
               The.Map.GetTile(spot).AddResource("clay", 4);
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), spot);
               */
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunBarrelSmoothShort"]), spot);
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), spot);
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryWeaponMount"]), spot);

               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), spot);
               */

            //human.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;

            /*Entity dog = PlaceAnimal("entity:dog", Reproduction.Male, spot, 5f, The.Sim.PlaySite.PlayerAllegiance);
            dog.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;*/

            The.Map.GetTile(6, 6).AddResource("salt", 200);

            for (int i = 0; i < 202; i++)
            {
                // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(8, 3));

                //                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowder"]), spot);

                // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), spot);
                /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:driedBeef"]), spot);
                    */
                /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetFirewood"]), spot);
                  //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:rottenVegetables"]), spot);
                  //
                  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), spot);*/
                //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), spot);
                //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), spot);
                /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), spot);
                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:rottenVegetables"]), spot);
                  
                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);
               
                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bogOre"]), spot);*/
                /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), spot);
                  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hammer"]), spot);*/
            }

            /*  AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(5, 9));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapBasket"]), new Point(16, 3));
          
              Entity trap = new Entity(GameData.Instance.AllEntityTypes["item:fishTrapBasket"]);
              trap.NonLivingEntity.Progress = 0.5f;
              AddColonyItem(trap, new Point(6, 8));
              */
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(8, 3));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
            /*    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
                */

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));          
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), new Point(6, 8));
             */

            /*Entity firewood = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            //firewood.NonLivingEntity.Progress = 0.5f;
            AddColonyItem(firewood, new Point(6, 8));*/


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(6, 8));


            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(8, 3));
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(8, 3));
             */

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(4, 3));
               */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
              */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:furniture"]), spot);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), spot);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:minnowsLive"]), spot);
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:minnowsLive"]), spot);

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), spot);

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), spot);

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);


              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hammer"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:goldOre"]), spot);
              //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), spot);
              //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), spot);
              //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), spot);
              //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), spot);
              //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), spot);
           /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCracker"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vat"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGreenHide"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenBrain"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenRawhide"]), spot);
              Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
              carcass.Bulk = 2.1f;
              AddColonyItem(carcass, spot);*/

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blacksmithsToolbox"]), spot);
           
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), spot);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vinegar"]), spot);
  */
            AddFinishedStructure("structure:kiln", new Point(18, 6), expedition, false);
            AddFinishedStructure("structure:kiln", new Point(18, 2), expedition, false);
            AddFinishedStructure("structure:kiln", new Point(18, 4), expedition, false);


            Entity smithy = AddFinishedStructure("structure:simpleSmithy", new Point(8, 5), expedition, false);
            /*  Entity part = smithy.Parts[0]; //.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
              part.DoDamage(0.95f);
              part = smithy.Parts[1]; //.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
              part.DoDamage(0.52f);
              */
            // AddFinishedStructure("structure:simpleSmithy", new Point(9, 5), expedition, false);
            // AddFinishedStructure("structure:simpleSmithy", new Point(10, 5), expedition, false);
            //  AddFinishedStructure("structure:campfire", new Point(6, 3), expedition, false);
            AddFinishedStructure("structure:hideRack", new Point(4, 7), expedition, false);
            AddFinishedStructure("structure:firewoodStack", new Point(7, 7), expedition, false);
            // AddFinishedStructure("structure:compostBin", new Point(4, 5), expedition, false);
            AddFinishedStructure("structure:mudBrickKitchen", new Point(2, 8), expedition, false);

            // AddFinishedStructure("structure:storageHole", new Point(10, 7), expedition, false);

            AddFinishedStructure("structure:compostPit", new Point(8, 8), expedition, false);

            /* Entity firewood = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
             AddColonyItem(firewood, new Point(6, 8));
          
             Entity oven = AddFinishedStructure("structure:smokeOven", new Point(11, 3), expedition, false); 
             ToolContainer ovenAsTool = oven.Contains as ToolContainer;         
             ovenAsTool.AddToContain(firewood, replenish: true);
            
           
             ovenAsTool.ReplenishItems.RequiresFuel.SetBulkLeftOfBurningItem(0.35f);
             */

            Entity e1 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Female, new Point(spot.X - 4, spot.Y + 0), 20,
                new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]), null);

        }

        public static void Recording()
        {
            /*  if (!The.Sim.LoadMap("j Map SandboxNomads March 2015"))
                  return;*/
            The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear() { Year = 206, Day = 7, TimeOfDay = 0.36 });

            Point campSpot = new Point(40, 25); //  <--- spawn point for camp

            /*****************************
             *      HUMAN SPAWNING
             *****************************/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(campSpot));
            The.MapUI.ZoomToMapPosition(campSpot.X, campSpot.Y);

            Entity human = PlacePerson("Andon", "Green", Reproduction.Male, campSpot, Color.White, 40f, false, "blueBrownClothes1", expedition);
            Entity human2 = PlacePerson("Castor", "Hernes", Reproduction.Male, campSpot, Color.White, 40f, false, "greenGreyClothes1", expedition);
            Entity human3 = PlacePerson("Linsey", "Cattier", Reproduction.Female, campSpot, Color.White, 40f, false, "greyClothes1", expedition);
            Entity human4 = PlacePerson("Manny", "Pezal", Reproduction.Male, campSpot, Color.White, 40f, false, "whitePantsClothes1", expedition);
            Entity human5 = PlacePerson("Nisa", "Manekki", Reproduction.Female, campSpot, Color.White, 40f, false, "greenBlueClothes1", expedition);
            Entity human6 = PlacePerson("Illen", "Rence", Reproduction.Female, campSpot, Color.White, 40f, false, "whiteBlueClothes1", expedition);
            PlaceAnimal("entity:dog", Reproduction.Male, campSpot, 5f, The.Sim.PlaySite.PlayerAllegiance);
            PlaceRobot("entity:haulingRobot", campSpot, The.Sim.PlaySite.PlayerAllegiance, expedition);

            /*****************************
             *      ITEM SPAWNING
             *****************************/

            int x = 3;
            for (int i = 0; i < x; i++) // use this to make multiple items, x = amount of items 
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), campSpot);//
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneSeeds"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), campSpot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), campSpot);
            }


            //1 of each:
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:pigFliesLive"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalHooks"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hammer"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bogOre"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:roughBloomIron"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), campSpot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), campSpot);
            /* carcass spawning
            Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
            carcass.Bulk = 4.9f;
            AddColonyItem(carcass, campSpot);*/

            /*****************************
             *      CREATURE SPAWNING
             *****************************/

            UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
            UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
            UWGame.SimSide.Allegiances.Allegiance all3 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:turnip"]);
            UWGame.SimSide.Allegiances.Allegiance all4 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);
            Expedition exp1 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(campSpot));
            Expedition exp2 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:turnip"]), "Start3", "Start3", MapManager.TileToWorldPos(campSpot));
            Expedition exp3 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]), "Start4", "Start4", MapManager.TileToWorldPos(campSpot));

            int t = 4;
            for (int i = 0; i < t; i++) // use this to spawn multiple creatures, t = amount of creatures
            {
                Entity e2 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(campSpot.X - 0, campSpot.Y + 1), 18, all2, null, exp1);
                Entity e3 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(campSpot.X + 6, campSpot.Y - 4), 18, all2, null, exp1);
            }
            /*
                        for (int i = 0; i < 2; i++)
                        {
                            Entity e1 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(campSpot.X + 6, campSpot.Y - 7), 18, all1, null, exp3);
                        }
            */

            Entity e4 = PlaceAnimal("entity:bird", Reproduction.Female, new Point(46, 37), 18, all4);
            Entity e5 = PlaceAnimal("entity:bird", Reproduction.Female, new Point(47, 39), 8, all4);

            Entity e6 = PlaceAnimal("entity:turnip", Reproduction.Female, new Point(46, 22), 18, all3, null, exp2);
            Entity e7 = PlaceAnimal("entity:turnip", Reproduction.Female, new Point(39, 21), 18, all3, null, exp2);
            Entity e8 = PlaceAnimal("entity:turnip", Reproduction.Female, new Point(39, 17), 18, all3, null, exp2);

            Entity e9 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(campSpot.X + 11, campSpot.Y - 7), 18, all1, null, exp3);
            Entity e10 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(58, 21), 18, all1, null, exp3);
            Entity e11 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(75, 18), 18, all1, null, exp3);
            /*****************************
             *      STRUCTURE SPAWNING
             *****************************/

            AddFinishedStructure("structure:caneHut", new Point(campSpot.X - 3, campSpot.Y + 5), expedition, false);
            AddFinishedStructure("structure:clayHut", new Point(campSpot.X - 3, campSpot.Y + 0), expedition, false);
            AddFinishedStructure("structure:turnipHut", new Point(campSpot.X + 2, campSpot.Y + 2), expedition, false);//(campSpot.X + 2, campSpot.Y + 2)
            AddFinishedStructure("structure:kiln", new Point(campSpot.X - 2, campSpot.Y + 2), expedition, false);
            AddFinishedStructure("structure:improvisedSmithy", new Point(campSpot.X + 0, campSpot.Y + 2), expedition, false);
            //          AddFinishedStructure("structure:sentry", new Point(campSpot.X + 0, campSpot.Y - 3), expedition, false); //crashes.........
            AddFinishedStructure("structure:improvisedKitchen", new Point(campSpot.X - 5, campSpot.Y + 1), expedition, false);
            AddFinishedStructure("structure:improvisedWorkbench", new Point(campSpot.X + 1, campSpot.Y - 1), expedition, false);
            //

            //For decoration: These farmplots are spawned in tilled condition, however, cannot be used for gameplay because they were born this way. only for decoration.
            Vector3[] largeFarmPos = new Vector3[] { new Vector3(1894f, 884f, 0) }; //, new Vector3(1882f, 1042f, 0) , new Vector3(2030f, 536f, 0), new Vector3(1209f, 825f, 0)
            foreach (Vector3 v in largeFarmPos)
            {
                AddFinishedStructure("structure:largePlot", null, expedition, false, v);
            }

            Vector3[] smallFarmPos = new Vector3[] { new Vector3(1680f, 912f, 0) };
            /// new Vector3(2072f, 1256f, 0), new Vector3(3168f, 3072f, 0), new Vector3(2260f, 2020f, 0), new Vector3(1700f, 1844f, 0), new Vector3(1498f, 1882f, 0), new Vector3(1550f, 1464f, 0), new Vector3(3498f, 1364f, 0) , new Vector3(1688f, 624f, 0), new Vector3(2119f, 952f, 0), new Vector3(2297f, 960f, 0), new Vector3(2450f, 682f, 0), new Vector3(3024f, 390f, 0), new Vector3(3130f, 272f, 0),
            foreach (Vector3 v in smallFarmPos)
            {
                AddFinishedStructure("structure:smallPlot", null, expedition, false, v);
            }
            /****************************
             *      TERRAIN SPAWNING
             ****************************/

            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");

            //For gameplay: these farmspots have to be activated by tilling from the beginning.
            largeFarmPos = new Vector3[] { new Vector3(1882f, 1042f, 0), new Vector3(2030f, 536f, 0) }; //new Vector3(1209f, 825f, 0), , new Vector3(1894f, 884f, 0)
            foreach (Vector3 v in largeFarmPos)
            {
                AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), v);
            }

            smallFarmPos = new Vector3[] { new Vector3(3168f, 3072f, 0), new Vector3(2260f, 2020f, 0), new Vector3(1700f, 1844f, 0), new Vector3(1498f, 1882f, 0), new Vector3(1550f, 1464f, 0), new Vector3(2072f, 1256f, 0), new Vector3(1680f, 912f, 0), new Vector3(1688f, 624f, 0), new Vector3(2119f, 952f, 0), new Vector3(2297f, 960f, 0), new Vector3(2450f, 682f, 0), new Vector3(3024f, 390f, 0), new Vector3(3130f, 272f, 0), new Vector3(3498f, 1364f, 0) };
            foreach (Vector3 v in smallFarmPos)
            {
                AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), v);
            }


            //fish trap locations////////////

            Vector3[] coastPos = new Vector3[] { new Vector3(4254f, 233f, 0), new Vector3(5303f, 252f, 0) };
            foreach (Vector3 v in coastPos)
            {
                AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), v);
            }
            Vector3[] shorePos = new Vector3[] { new Vector3(3085f, 1066f, 0), new Vector3(1940f, 1431f, 0), new Vector3(2568f, 1738f, 0), new Vector3(2708f, 2071f, 0), new Vector3(3172f, 2292f, 0), new Vector3(1840f, 2036f, 0), new Vector3(1278f, 2420f, 0), new Vector3(2089f, 2457f, 0), new Vector3(3580f, 3130f, 0), new Vector3(4000f, 2520f, 0), new Vector3(4972f, 2150f, 0), new Vector3(4618f, 2900f, 0), new Vector3(2824f, 3784f, 0), new Vector3(3178f, 4210f, 0), new Vector3(2342f, 4362f, 0), new Vector3(748f, 4282f, 0) };
            foreach (Vector3 v in shorePos)
            {
                AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), v);
            }
        }

        public static void mineShowcase()
        {
            /* if (!The.Sim.LoadMap("j Map SandboxNomads March 2015"))
                 return;*/
            Point campSpot = new Point(50, 40); //73, 27
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(campSpot));
            UWGame.SimSide.Allegiances.Allegiance twinklerAli = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);
            UWGame.SimSide.Allegiances.Allegiance nestAli = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
            Expedition exp1 = new Expedition(twinklerAli, "start2", "Start2", MapManager.TileToWorldPos(new Point(campSpot.X - 17, campSpot.Y - 17)));//was (campSpot.X -17, campSpot.Y - 17, 0),   //   MapManager.TileToWorldPos(campSpot) //33,23
            AddFinishedStructure("structure:sentry", null, expedition, false, new Vector3(campSpot.X * 48 + (40), campSpot.Y * 48 + (1), 0)); //was: campSpot, expedition, false);
            AddFinishedStructure("structure:sentry", null, expedition, false, new Vector3(campSpot.X * 48 + (28), campSpot.Y * 48 + (2 * 48), 0));
            //AddFinishedStructure("structure:sensor", new Point(25, 44), expedition, false);
            The.MapUI.ZoomToMapPosition(campSpot.X, campSpot.Y);
            for (int i = 0; i < 50; i++) //i < 10
            { //was "entity:swarmer"
                Entity t1 = PlaceAnimal("entity:swarmer", Reproduction.Male, new Point(campSpot.X - 17, campSpot.Y - 17), 17, twinklerAli, null, exp1); //(campSpot.X - 3, campSpot.Y + 5)
                t1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
            }
            Entity p1 = PlacePerson("Andon", "Green", Reproduction.Male, new Point(campSpot.X - 3, campSpot.Y - 1), Color.MediumSeaGreen, 52, false, expedition, "blueBrownClothes1", null);
            Entity p2 = PlacePerson("Linsey", "Cattier", Reproduction.Female, new Point(campSpot.X - 1, campSpot.Y - 0), Color.Bisque, 39, false, expedition, "greenGreyClothes1", null);
            Entity p3 = PlacePerson("Larsen", "Cattier", Reproduction.Female, new Point(campSpot.X - 1, campSpot.Y - 0), Color.Bisque, 39, false, expedition, "greenGreyClothes1", null);
            Entity p4 = PlacePerson("Andon", "Green", Reproduction.Male, new Point(campSpot.X - 3, campSpot.Y - 1), Color.MediumSeaGreen, 52, false, expedition, "blueBrownClothes1", null);
            Entity p5 = PlacePerson("Linsey", "Cattier", Reproduction.Female, new Point(campSpot.X - 1, campSpot.Y - 0), Color.Bisque, 39, false, expedition, "greenGreyClothes1", null);
            Entity p6 = PlacePerson("Larsen", "Cattier", Reproduction.Female, new Point(campSpot.X - 1, campSpot.Y - 0), Color.Bisque, 39, false, expedition, "greenGreyClothes1", null);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(campSpot.X - 3, campSpot.Y - 1));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(campSpot.X - 1, campSpot.Y - 0)); //
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(campSpot.X - 3, campSpot.Y - 1));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:varmintBomb"]), new Point(campSpot.X - 1, campSpot.Y - 0)); //
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(campSpot.X - 1, campSpot.Y - 0));
            for (int i = 0; i < 2; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), new Point(campSpot.X - 2, campSpot.Y - 0));
            }
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
            AddFinishedStructure("structure:smallPlot", new Point(campSpot.X - 2, campSpot.Y - 0), expedition, false);
            Entity te1 = new Entity(GameData.Instance.AllEntityTypes["terrain:fieldQuaditeNest"]);
            AddTerrainItem(te1, new Vector3(3404, 1400, 0));
        }

        public static void CommTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point spot = new Point(6, 6);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));

            The.MapUI.ZoomToMapPosition(spot.X, spot.Y);

            GetBob(spot, expedition);

            // AddFinishedStructure("structure:satelliteGroundStation", new Point(6, 16), expedition, false);

            // AddFinishedStructure("structure:helipad", new Point(8, 8), expedition, false);
            AddFinishedStructure("structure:simplePort", new Point(8, 8), expedition, false);

            for (int i = 0; i < 10; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
            }
        }

        public static void TrapTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point spot = new Point(3, 6);
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 9)));
            UWGame.SimSide.Allegiances.Allegiance ChickenAllegiance1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bajingan"]);
            UWGame.SimSide.Allegiances.Allegiance ratal = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);

            for (int i = 0; i < 1; i++)
            {
                Entity e1 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(14, 6), 20, ratal);//pygmyThunderChicken
                e1.Bulk = 0.2f;
                e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;

                ImmobilizeEntity(e1);

                /*   Entity e3 = PlaceAnimal("entity:bajingan", Reproduction.Male, new Point(6, 6), 1, ChickenAllegiance1);
                   e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
                   Entity e4 = PlaceAnimal("entity:bajingan", Reproduction.Male, new Point(6, 6), 1, ChickenAllegiance1);
                   e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;*/
            }
            The.MapUI.ZoomToMapPosition(6, 6);
            Entity human = PlacePerson("Charles", "Jacobi", Reproduction.Male, spot, Color.White, 40f, false, expedition);
            //Entity human2 = PlacePerson("Charles", "Jacobi2", Reproduction.Male, new Point(0, 0), Color.White, 40f, false, expedition);
            // Bob01.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
            AddFinishedStructure("structure:spikeTrap", new Point(14, 6), expedition, false, null, "constructSpikeTrap");
            // AddFinishedStructure("structure:springSnare", new Point(8, 6), expedition, false, null, "constructSpringSnare");

            for (int i = 0; i < 1; i++)
            {
                //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), spot);
                //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), spot);
                //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), spot);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), spot);
                //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), spot);
            }
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(7, 7));

            //  AddFinishedStructure("structure:spikeTrap", spot, expedition, false);

            //AddFinishedStructure("structure:sensor", spot, expedition, false);
            //AddFinishedStructure("structure:collapseTrap", spot, expedition, false);
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), spot);
            /*Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thinThunderChickenCarcass"]);
            carcass.Bulk = 3.8f;
            AddColonyItem(carcass, spot);*/
            /*Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:binalRatChunk"]);
            carcass.Bulk = 0.1f;
            AddColonyItem(carcass, spot);*/
        }


        public static void CarcassNoMeat()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(0, 15)));

            The.MapUI.ZoomToMapPosition(10, 5);
            UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");

            for (int i = 0; i < 1; i++)
            {
                Entity e1 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(10, 6), 8, all2);
                e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
            }

            //  Entity Bob01 = GetBob(new Point(0, 15), expedition);
            /*for(int i=0; i<5; i++)
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(9, 7));*/
            for (int i = 0; i < 1; i++)
            {
                //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(4, 10));
                Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
                carcass.Bulk = 0.35f;
                AddColonyItem(carcass, new Point(6, 6));
            }
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(4, 10));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(4, 10));

              AddFinishedStructure("structure:campfire", new Point(3, 10), expedition, false);*/
            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);
        }

        public static void ShaiHuludTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            UWGame.SimSide.Allegiances.Allegiance ShaiHuludAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
            UWGame.SimSide.Allegiances.Allegiance ChickenAllegiance1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);
            UWGame.SimSide.Allegiances.Allegiance ChickenAllegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
            UWGame.SimSide.Allegiances.Allegiance WhipJawAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
            //Entity e1 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(15, 15), 1, ShaiHuludAllegiance);
            //Entity e2 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(15, 17), 20, ShaiHuludAllegiance);
            //Entity e3 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Male, new Point(17, 15), 1, ShaiHuludAllegiance);
            //Entity e4 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Male, new Point(17, 17), 20, ShaiHuludAllegiance);
            Entity e5 = PlaceAnimal("entity:whipjaw", Reproduction.Male, new Point(19, 15), 20, WhipJawAllegiance);
            Entity e6 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(19, 17), 20, WhipJawAllegiance);
        }


        public static void ScareCrowTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
            The.MapUI.ZoomToMapPosition(45, 45);


            AddFinishedStructure("structure:scarecrow", new Point(46, 44), expedition, false);
            // Entity e01 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(48, 44), 5);
            //e01.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.01f;

            /*
            Entity Bob = PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(50, 45), Color.White, 52f, false, expedition);
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
            */
        }

        public static void ReloadTest()
        {
            /* if (!The.Sim.LoadMap("c Micromap Test")) //"fx Map DemoIsland Dec 2014")) // "d Mezzomap MLo"))
                 return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
            The.MapUI.ZoomToMapPosition(5, 5);

            Entity p1 = PlacePerson("Jamy", "Hassert", Reproduction.Male, new Point(5, 5), Color.MediumSeaGreen, 35, false, expedition, null, null);

            Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]);
            AddColonyItem(ammo, new Point(5, 5));
            ammo.Item.Ammunition.NoOfRounds = 1;

            Entity rifle = new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]);
            AddColonyItem(rifle, new Point(5, 5));

            /*  Container magazine = rifle.Contains;
              Entity surplusAmmo;
              magazine.AddToContain(ammo, out surplusAmmo);
              */

            Entity ammo2 = new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]);
            AddColonyItem(ammo2, new Point(5, 5));
            //  ammo2.Item.Ammunition.NoOfRounds = 5;

            //   magazine.AddToContain(ammo2, out surplusAmmo);


            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);

            int y = 6;
            Entity twinkler = PlaceAnimal("entity:patrician",
                                            Reproduction.Male,
                                            new Point(8, 8), //x + 5),
                                            20,
                                            twinklerAllegiance);

        }

        public static void RobotTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //"fx Map DemoIsland Dec 2014")) // "d Mezzomap MLo"))
                  return;*/
            Point campPos = new Point(7, 5);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));
            The.MapUI.ZoomToMapPosition(5, 5);

            Entity p2 = PlacePerson("Jamy", "Hassert2", Reproduction.Male, new Point(7, 6), Color.MediumSeaGreen, 35, false, expedition, null, null);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Food);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);

            /* for (int i = 20 - 1; i >= 0; i--)
             {*/
            /* Entity p1 = PlacePerson("Jamy", "Hassert1", Reproduction.Male, new Point(6, 6), Color.MediumSeaGreen, 35, false, expedition, null, null);
             // }

          /*  Entity robot =  PlaceRobot("entity:guardRobot", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);
              Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
             AddColonyItem(ammo, new Point(5, 2));
             Container magazine = robot.Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentryGun").Contains;
             Entity surplusAmmo;
             magazine.AddToContain(ammo, out surplusAmmo);
             */

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(5, 5));

            //   PlaceAnimal("entity:dog", Reproduction.Male, new Point(9, 9), 5f, The.Sim.PlaySite.PlayerAllegiance);
            //   PlaceRobot("entity:haulingRobot", new Point(3, 5), The.Sim.PlaySite.PlayerAllegiance, expedition);
            //  PlaceRobot("entity:robotSmall", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);
            //  PlaceRobot("entity:haulingRobot", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);
            PlaceRobot("entity:diggingRobot", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);

            //  PlaceRobot("entity:patrolRobot", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);

            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");


            AddFinishedStructure("structure:rareMetalOrePit3", new Point(10, 12), expedition);

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 7)) + new Vector3(0f, 24f, 0f));

        }

        public static void ItemsNotCarriedBugHunt()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
            The.MapUI.ZoomToMapPosition(5, 5);

            for (int i = 0; i < 4; i++)
            {
                Entity Bob01 = GetBob(new Point(6, 6 + i), expedition);
            }


            UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);

            // Structures //
            AddFinishedStructure("structure:campfire", new Point(7, 7), expedition, false);
            // Items //
            for (int i = 0; i < 10; i++)
            {
                //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(6, 6));
                //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
                Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
                carcass.Bulk = 1f;
                AddColonyItem(carcass, new Point(6, 6));
            }
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
        }
        public static void BenjaminsTest()
        {
            /* if (!The.Sim.LoadMap("fx Map DemoIsland Dec 2014"))
                 return;*/

            // Spawn Points //
            Point humanSpawn = new Point(13, 48);
            Point alienSpawn1 = new Point(15, 44);
            Point alienSpawn2 = new Point(13, 44);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(humanSpawn)); // old 5,5
            The.MapUI.ZoomToMapPosition(humanSpawn.X, humanSpawn.Y);

            UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);



            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(6, 6));
            Entity arrow = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicArrow"]);
            AddColonyItem(arrow, new Point(6, 6));
            arrow.Item.Ammunition.NoOfRounds = 1;

            // Items //
            for (int i = 0; i < 8; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), humanSpawn);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), humanSpawn);
            }
            for (int i = 0; i < 20; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), humanSpawn);
            }

            // Structures //
            for (int i = 0; i < (7 * 3); i++)
            {
                if (i == (6 * 3) / 2 || i == (6 * 3) / 2 + 1 || i == (6 * 3) - 1)
                    continue;
                AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(528 + (i * 16), 2304, 0));
            }
            AddFinishedStructure("structure:satelliteGroundStation", null, expedition, false, new Vector3(624, 2448, 0));
            //  AddFinishedStructure("structure:weatherStationSensors", null, expedition, false, new Vector3(624+16, 2448+3, 0));
            //  AddFinishedStructure("structure:weatherStationMast", null, expedition, false, new Vector3(624+3, 2448+16, 0));

            // Agents //
            Entity Bob01 = GetMinion(humanSpawn, expedition, "Bob", " ");
            Entity Bob02 = GetMinion(humanSpawn, expedition, "Peter", " ");
            Entity Bob03 = GetMinion(humanSpawn, expedition, "Luke", " ");
            Entity Bob04 = GetMinion(humanSpawn, expedition, "Kurt", " ");
            Entity Bob05 = GetMinion(humanSpawn, expedition, "Picard", " ");
            //  Entity Bob06 = getBobTheMachine(humanSpawn, expedition);

            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            Allegiances.Allegiance patricianAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);

            for (int i = 0; i < 3; i++)
            {
                Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, alienSpawn1, 20, twinklerAllegiance);
                Entity twinkler02 = PlaceAnimal("entity:twinkler", Reproduction.Male, alienSpawn1, 20, twinklerAllegiance);
                Entity patrician02 = PlaceAnimal("entity:patrician", Reproduction.Male, alienSpawn2, 20, patricianAllegiance);
            }
        }

        public static Entity GetMinion(Point pos, Expedition exp, string firstName, string sirName)
        {
            Entity Minion = PlacePerson(firstName, sirName, Reproduction.Male, pos, Color.Purple, 30f, false, exp);
            Minion.PersonEntity.UpdatePortrait(The.InGameUI.gui, "skimmerDark");
            foreach (SkillType skillType in GameData.Instance.AllSkillTypes.Values)
            {
                Minion.Intelligence.Skills.Add(skillType, new Skill(0.5f, skillType));
            }
            return Minion;
        }



        public static void HuntTest()
        {

            // The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear() { Year = 206, Day = 0, TimeOfDay = 0.8 });

            Point campPos = new Point(10, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));
            The.MapUI.ZoomToMapPosition(5, 10);

            Entity Bob01 = GetBob(campPos, expedition);
           // GetBob(campPos, expedition);

            UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);

          //  UWGame.SimSide.Allegiances.Allegiance turinpAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:turnip"]);
          
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), new Point(6, 10));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), new Point(6, 10));

              Entity e11 = PlaceAnimal("entity:dog", Reproduction.Male, new Point(8, 8), 20, expedition.Allegiance);
           */

            for (int i = 0; i < 8; i++)
            {
                Entity e11 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(5 + i * 3, 15), 20, chickenAllegiance);
                ImmobilizeEntity(e11);
            }

            // Entity e11 = PlaceAnimal("entity:turnip", Reproduction.Male, new Point(8, 15), 20, turinpAllegiance);
            //  ImmobilizeEntity(e11);
            /*

             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cloak"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:nightVisionGoggles"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:groundScanner"]), campPos);

             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), campPos);
             Entity arrow = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicArrow"]);
             AddColonyItem(arrow, campPos);*/
            //  arrow.Item.Ammunition.NoOfRounds = 1;

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), campPos);


            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), campPos);
              */

            //  The.Client.LogOutOfFuel(GameData.Instance.AllEntityTypes["item:cloak"], 1f);

            /* for (int i = 0; i < 5; i++)
             {
                 The.Client.LogOutOfFuel(GameData.Instance.AllEntityTypes["item:gunpowderRifle"], 1f);
                 The.Client.LogOutOfFuel(GameData.Instance.AllEntityTypes["item:gunpowderRifle"], null);

             }*/


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:musket"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderShotAmmo"]), campPos);
            */

            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), campPos);          
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), campPos);
             */

        }

        public static void MemoryTest()
        {
            /* if (!The.Sim.LoadMap( "j Map SandboxNomads March 2015")) //"d Mezzomap MLo"))
                 return;*/

            Point pos = new Point(30, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(pos));
            The.MapUI.ZoomToMapPosition(pos.X, pos.Y);

            for (int i = 0; i < 20; i++)
            {
                UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);

                Entity e11 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(pos.X + i, pos.Y), 20, chickenAllegiance);

                UWGame.SimSide.Allegiances.Allegiance twinklerAli = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
                PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(pos.X + i, pos.Y), 20, twinklerAli);


            }
        }


        public static void ManyAgentsTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
            The.MapUI.ZoomToMapPosition(5, 5);

            UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);

            //Creatures



            Entity e11 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e12 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e13 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e14 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e15 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e16 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e17 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e18 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e19 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e20 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e21 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e22 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e23 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e24 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e25 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e26 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e27 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e28 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e29 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e30 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e31 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e32 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e33 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e34 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e35 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e36 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e37 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e38 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e39 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e40 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e41 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e42 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e43 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e44 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e45 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e48 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e49 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);
            Entity e50 = PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20, chickenAllegiance);


            //colony
            Entity Bob01 = GetBob(new Point(8, 9), expedition);
            Entity Bob02 = GetBob(new Point(8, 9), expedition);
            Entity Bob03 = GetBob(new Point(8, 9), expedition);
            Entity Bob04 = GetBob(new Point(8, 9), expedition);
            Entity Bob05 = GetBob(new Point(8, 9), expedition);
            Entity Bob06 = GetBob(new Point(8, 9), expedition);
            Entity Bob07 = GetBob(new Point(8, 9), expedition);
            Entity Bob08 = GetBob(new Point(8, 9), expedition);
            Entity Bob09 = GetBob(new Point(8, 9), expedition);
            Entity Bob10 = GetBob(new Point(8, 9), expedition);
            Entity Bob11 = GetBob(new Point(8, 9), expedition);
            Entity Bob12 = GetBob(new Point(8, 9), expedition);
            Entity Bob13 = GetBob(new Point(8, 9), expedition);
            Entity Bob14 = GetBob(new Point(8, 9), expedition);
            Entity Bob15 = GetBob(new Point(8, 9), expedition);
            Entity Bob16 = GetBob(new Point(8, 9), expedition);


            //items
            for (int i = 0; i < 40; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(6, 6));
            }
            for (int i = 0; i < 20; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(6, 6));
            }
            for (int i = 0; i < 50; i++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:mashedCommonOilTubers"]), new Point(6, 6));
            }

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedFlintSpear"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));


            //right side wall
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 484f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 500f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 564f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 580f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 516f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 532f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 548f, 0f));

            //left side wall
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 484f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 500f, 0f));
            //AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 516f, 0f));
            //AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 532f, 0f));
            //AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 548f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 564f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 580f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 596f, 0f));

            //top hori wall
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1134f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1118f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1102f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1086f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1070f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1054f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1038f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1022f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1006f, 468f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 468f, 0f));

            //bottom hori wall
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1150f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1134f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1118f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1102f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1086f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1070f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1054f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1038f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1022f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(1006f, 596f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, new Vector3(990f, 596f, 0f));

            AddFinishedStructure("structure:octagonalTent", new Point(4, 5), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(6, 5), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(8, 5), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(4, 3), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(6, 3), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(8, 3), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(10, 3), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(10, 5), expedition, false);
            AddFinishedStructure("structure:cooledFoodCache", new Point(12, 5), expedition, false);
            AddFinishedStructure("structure:cooledFoodCache", new Point(14, 5), expedition, false);
            AddFinishedStructure("structure:campfire", new Point(7, 7), expedition, false);


        }


        public static void AmbientSoundTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(20, 10)));
            The.MapUI.ZoomToMapPosition(20, 20);

            Entity Bob = PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(20, 10), Color.White, 52f, false, expedition);

            PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(20, 11), Color.White, 52f, false, expedition);

            /*     AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:shoreWavesLongA"]), MapManager.TileToWorldPos(new Point(25, 20)));

                 AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:shoreWavesLongA"]), MapManager.TileToWorldPos(new Point(1, 20)));

                 AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:shoreWavesLongA"]), MapManager.TileToWorldPos(new Point(20, 30)));
              */

        }

        public static void AllItemsTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
            The.MapUI.ZoomToMapPosition(15, 5);

            Entity Bob = PlacePerson("Byggemand", "Bob", Reproduction.Male, new Point(15, 5), Color.White, 52f, false, expedition);
            /*Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(1f, GameData.Instance.AllSkillTypes["butchering"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(1f, GameData.Instance.AllSkillTypes["cooking"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(1f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(1f, GameData.Instance.AllSkillTypes["medicine"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(1f, GameData.Instance.AllSkillTypes["psychology"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));
            */

            int noOfItemsOfEachType = 4;

            foreach (var item in GameData.Instance.AllItemTypes)
            {
                try
                {
                    for (int i = 0; i < noOfItemsOfEachType; i++)
                    {
                        if (item.Value.ItemType.HasNoMaximumBulk)
                        {
                            Entity carcass = new Entity(item.Value);
                            carcass.Bulk = 1f;
                            AddColonyItem(carcass, new Point(15, 5));
                        }
                        else
                        {
                            AddColonyItem(new Entity(item.Value), new Point(15, 5));
                        }
                    }
                }
                catch (Exception) { }// skip placeholders/unfinished types
            }


        }

        public static void SoundTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
            The.MapUI.ZoomToMapPosition(45, 45);

            Entities.Body.BodyComponent body;


            /*  Entity ee2 = PlaceAnimal("entity:whipjaw", Reproduction.Male, new Point(49, 35), 20);
              ee2.Find(out body);
              body.Body.ChangeMaxHitpoints(450.0f);*/
            //Entity ee4 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(47, 40), 20);
            //Entity ee5 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(46, 40), 20, twinklerAllegiance);
            //Entity ee6 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(45, 40), 20, twinklerAllegiance);
            //Entity ee7 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(44, 39), 20, twinklerAllegiance);
            //Entity ee8 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(43, 39), 20, twinklerAllegiance);
            //Entity e2 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(47, 43), 20);
            //  Entity e3 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(45, 43), 20);
            //Entity e4 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(46, 43), 20);
            //Entity e5 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(44, 44), 20);
            //Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            // AddFinishedStructure("structure:sensor", new Point(45, 43), expedition, false);
            //item.Bulk = 1f;
            //AddColonyItem(item, new Point(47, 43));
            //MP: this takes a couple seconds before it shows up ! about 2 sec before the model appears.
            ///*
            Entity Bob = PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(49, 41), Color.White, 52f, false, expedition);
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(49, 40));
            Bob.Find(out body);
            Bob.Body.ChangeMaxHitpoints(450.0f);

            //*/
        }



        // test for the demontree
        public static void DemonTreeTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point campPos = new Point(15, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));
            The.MapUI.ZoomToMapPosition(campPos);

            Entities.Body.BodyComponent body;

            UWGame.SimSide.Allegiances.Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:spoakDendront"]);
            Entity e1 = PlaceAnimal("entity:spoakDendront", Reproduction.Male, new Point(15, 8), 20, allegiance);
            e1.Find(out body);
            e1.Body.ChangeMaxHitpoints(450.0f);


            ////Entity ee1 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(47, 49), 20, twinklerAllegiance);
            //Entity ee2 = PlaceAnimal("entity:whipjaw", Reproduction.Male, new Point(49, 42), 20);
            //Entity ee3 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(48, 40), 20, twinklerAllegiance);
            //Entity ee4 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(47, 40), 20);
            //Entity ee5 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(46, 40), 20, twinklerAllegiance);
            //Entity ee6 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(45, 40), 20, twinklerAllegiance);
            //Entity ee7 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(44, 39), 20, twinklerAllegiance);
            //Entity ee8 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(43, 39), 20, twinklerAllegiance);
            //Entity e2 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(47, 43), 20);
            //Entity e3 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(45, 43), 20);
            //Entity e4 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(46, 43), 20);
            //Entity e5 = PlaceAnimal("entity:megapod", Reproduction.Male, new Point(44, 44), 20);
            //Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            // AddFinishedStructure("structure:sensor", new Point(45, 43), expedition, false);
            //item.Bulk = 1f;
            //AddColonyItem(item, new Point(47, 43));
            //MP: this takes a couple seconds before it shows up ! about 2 sec before the model appears.
            ///*
            Entity Bob = PlacePerson("Bobby", "Bob", Reproduction.Male, campPos, Color.White, 52f, false, expedition);
            PlacePerson("Bobby", "Bob", Reproduction.Male, campPos, Color.White, 52f, false, expedition);
            PlacePerson("Bobby", "Bob", Reproduction.Male, campPos, Color.White, 52f, false, expedition);
            PlacePerson("Bobby", "Bob", Reproduction.Male, campPos, Color.White, 52f, false, expedition);


            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(49, 38));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), campPos);


            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(49, 38));
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvedFireExtinguisher"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(49, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(49, 40));
            ^*/
            //*/
        }
        public static void ButcherTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));
            The.MapUI.ZoomToMapPosition(15, 15);

            Entity Bob = GetBob(new Point(10, 15), expedition);

            Bob = GetBob(new Point(1, 2), expedition);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(10, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(10, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCracker"]), new Point(10, 15));

            Entity carcass;

            int spawnCarcass = 1;
            for (int i = 0; i < spawnCarcass; i++)
            {
                carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
                carcass.Bulk = 4f;
                AddColonyItem(carcass, new Point(15, 15));
            }

        }

        // test for spikePlantTest
        public static void SpikePlantTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
            The.MapUI.ZoomToMapPosition(45, 45);

            /*  Entity e1 = PlaceAnimal("entity:spikePlant", Reproduction.Male, new Point(46, 44), 20);
              Entity e2 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(47, 45), 20);
              Entity e3 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(45, 43), 20);
              Entity e4 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(46, 43), 20);
              Entity e5 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(44, 44), 20);*/
            //MP: this takes a couple seconds before it shows up ! about 2 sec before the model appears.
            //Entity Bob = PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(49, 39), Color.White, 52f, false, expedition);
            //Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            //Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(49, 40));
        }

        public static void WorldMapTest()
        {
            /*  if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                  return;*/

            Point campPos = new Point(8, 22);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            GetBob(campPos, expedition);

            WorldMapDialog dialog = The.InGameUI.WorldMapDialog;

            /*  dialog.OKClick += new EventHandler(WorldMapDialog_OKClick);
              dialog.CancelClick += new EventHandler(WorldMapDialog_CancelClick);
              */

            // dialog.Fill();


            dialog.ShowInScreenSpace(400, 100); // absolutePosition.X - dialog.Window.Width / 2, absolutePosition.Y - dialog.Window.Height / 2, false);

        }

        private static void AddResource(Point pos, string resource, int amount = 4)
        {
            The.Map.GetTile(pos).AddResource(resource, amount);
        }

        public static void ClayPitTest()
        {

            Point campPos = new Point(8, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            //  AddFinishedStructure("structure:sensor", campPos, expedition, false);

            //The.Map.GetTile(campPos).AddResource("salt", 8);
            The.Map.GetTile(campPos).AddResource("clay", 4);

            The.Map.GetTile(new Point(5, 10)).AddResource("clay", 4);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);

            int ppl = 1;
            for (int i = 0; i < ppl; i++)
            {
                GetBob(campPos, expedition);
            }


            //      <item, quantity>
            Dictionary<string, int> items = new Dictionary<string, int>();

            items.Add("advancedString", 2);
            items.Add("steelSpade", 2);
            items.Add("sticks", 4);
            items.Add("crystalBerries", 2);
            items.Add("clayJar", 1);
            // items.Add("vine", 1);
            items.Add("steelKnife", 1);

            foreach (KeyValuePair<string, int> entry in items)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + entry.Key]), campPos);
                }
            }

            The.Map.GetTile(campPos).AddResource("vine", 8);

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:clayDeposit"]), MapManager.TileToWorldPos(new Point(11, 9)) + new Vector3(0f, 24f, 0f));

            AddFinishedStructure("structure:improvisedKitchen", new Point(campPos.X - 2, campPos.Y + 1), expedition, false);


            /*  Allegiances.Allegiance birdAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);
              PlaceAnimal("entity:bird", Reproduction.Male, new Point(12, 12), 20, birdAllegiance, "Dark race");
              PlaceAnimal("entity:bird", Reproduction.Male, new Point(13, 12), 20, birdAllegiance, "Dark race");
            */
        }


        public static void NanomapTest()
        {
            Point campPos = new Point(7, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            GetBob(new Point(6, 5), expedition);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), campPos);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);

            /* The.Map.GetTile(new Point(8, 8)).AddResource("clay", 4);
             The.Map.GetTile(new Point(8, 7)).AddResource("clay", 4);
             The.Map.GetTile(new Point(8, 7)).AddResource("clay", 4);
             The.Map.GetTile(new Point(8, 7)).AddResource("clay", 4);
             */



            // The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(5, 5)), null, 0.5f);

            //  The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(7, 8)));

            // The.Client.LogOutOfFuel(GameData.Instance.AllEntityTypes["item:cloak"]);

            /*  for (int i = 0; i < 250; i++)
              {
                  The.Client.Log.AddLogEvent(The.Client.Log.EconomicEvent, null, i + " sdfsdfsfsdfs"); //string.Format("No fuel in inventory. Production orders using {0} cannot be completed before we have a supply of fuel", GameData.Instance.AllEntityTypes["item:gunpowderRifle"].Name), UWGame.ClientSide.Log.Priority.High);
         
                 // The.Client.LogOutOfFuel();

              }*/
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), campPos);
              */
            // Entity item = AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:streakFin"]), campPos);
            //  item.NonLivingEntity.DoConditionDamage(0.98f, null);
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);

            //AddFinishedStructure("structure:canopyHouse", new Point(4, 10), expedition, false);
            /*  Entity item = new Entity(GameData.Instance.AllEntityTypes["item:organicMatter"]);
              item.Bulk = 1f;
              AddColonyItem(item, campPos);*/

            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vine"]), new Point(7, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vine"]), new Point(7, 8));
            */
        }


        // test enviroment for the farming feature
        public static void FarmTest()
        {
            /* if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                 return;*/


            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");

            Point campPos = new Point(8, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            AddFinishedStructure("structure:sensor", campPos, expedition, false);

            AddFinishedStructure("structure:greenhouse", new Point(4, 10), expedition, false, constructionProcessToUse: "constructGreenhouse");

            AddFinishedStructure("structure:improvisedGreenhouse", new Point(4, 8), expedition, false, constructionProcessToUse: "constructImprovisedGreenhouse");

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);


            The.Map.GetTile(new Point(8, 7)).AddResource("glassyCreeperPods", 4);
            // The.Map.GetTile(new Point(8, 7)).AddResource("clay", 4);


            int ppl = 1;
            for (int i = 0; i < ppl; i++)
            {
                GetBob(campPos, expedition);
            }


            //      <item, quantity>
            Dictionary<string, int> items = new Dictionary<string, int>();
            items.Add("item:farmingHoe", ppl);
            //  items.Add("ironSpade", 1);//
            items.Add("item:glassyCreeperPods", 12);
            items.Add("item:crystalBerries", 12);
            items.Add("item:advancedMachete", 2);
            items.Add("item:guano", 2);
            items.Add("item:fingerFruit", 8);
            items.Add("item:advancedString", 3);
            items.Add("item:cotton", 3);
            items.Add("item:steelKnife", 3);
            items.Add("item:improvisedGreenHouseCover", 15);
            items.Add("item:shadeleafCanes", 9);
            items.Add("item:improvisedSpade", 3);
            items.Add("item:astroRation", 4);

            foreach (KeyValuePair<string, int> entry in items)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes[entry.Key]), campPos);
                    // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + entry.Key]), campPos);
                }
            }

            Entity i1 = new Entity(GameData.Instance.AllEntityTypes["item:organicMatter"]);
            i1.Bulk = 2.5f;
            AddColonyItem(i1, campPos);
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(10, 7)));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 5)) + new Vector3(0f, 24f, 0f));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)) + new Vector3(0f, 24f, 0f));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 9)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 9)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 5)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 7)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 9)) + new Vector3(0f, 24f, 0f));
        }

        public static void LongTermFarmingTest()
        {
            /*  if (!The.Sim.LoadMap(The.Map.AllMaps[13]))
                  return;*/


            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");
            //The.Sim.PlaySite.EventManager.AddPolledEvent("fishTrapCounter"); //bso outcommented because the old system have been replaced
            //The.Sim.PlaySite.EventManager.AddPolledEvent("fishTrapSpawningLoop"); //bso outcommented because the old system have been replaced
            //The.Sim.PlaySite.EventManager.AddPolledEvent("fishTrapCheckingLoop"); //bso outcommented because the old system have been replaced

            Point campPos = new Point(45, 26);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Camp", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);


            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(39, 19)));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(45, 19)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(44, 29));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), new Point(49, 28));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCreek"]), new Point(48, 18));

            //      <item, quantity>
            Dictionary<string, int> items = new Dictionary<string, int>();
            items.Add("farmingHoe", 2);
            items.Add("sticks", 36);
            items.Add("waterCaneStem", 6);
            items.Add("paracord", 6);
            items.Add("glassyCreeperPods", 9);
            items.Add("cotton", 9);
            items.Add("crystalBerries", 9);
            items.Add("machete", 2);
            items.Add("organicMatter", 2);

            foreach (KeyValuePair<string, int> entry in items)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + entry.Key]), campPos);
                }
            }
        }

        public static void PlotAssetTest()
        {
            /*  if (!The.Sim.LoadMap(The.Map.AllMaps[13]))
                  return;*/

            The.Sim.PlaySite.EventManager.AddPolledEvent("initDebugStuff");
            The.Sim.PlaySite.EventManager.AddPolledEvent("debugCounter");

            Point campPos = new Point(45, 26);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Camp", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            GetBob(campPos, expedition);


            AddFinishedStructure("structure:smallPlot", null, expedition, false, MapManager.TileToWorldPos(new Point(43, 26)) + new Vector3(0f, 24f, 0f));
        }

        // test for playing around with some particle effects
        public static void BushDragonParticleTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
            The.MapUI.ZoomToMapPosition(10, 10);

            /* Entity e1 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(10, 10), 20);
             Entity e2 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(20, 20), 20);
             Entity e3 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(12, 12), 20);
             Entity e4 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(22, 22), 20);*/
        }

        // test the fishtrap
        public static void FishTrapTest()
        {
            /* if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                 return;*/

            The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");

            Point campPos = new Point(18, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));
            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            Entity Bob = GetBob(campPos, expedition);

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCreek"]), new Point(14, 10));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), new Point(14, 14));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(8, 10));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapHoopNet"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapBasket"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingNet"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingNet"]), campPos);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);

            /*
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(135, 113)));
            The.MapUI.ZoomToMapPosition(135, 115);

               
            Entity Bob = GetBob(new Point(135, 115), expedition);

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCreek"]), new Point(136, 111));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), new Point(136, 117));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(132, 117));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapHoopNet"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapBasket"]), new Point(136, 115));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(135, 115));*/
        }


        public static void PierTest()
        {
            /*  if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                  return;*/
            Point campPos = new Point(15, 12);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));
            The.MapUI.ZoomToMapPosition(campPos);


            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);


            Entity Bob = GetBob(campPos, expedition);

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:pierSpot"]), new Point(campPos.X + 2, campPos.Y));
            /*
                   Inputs = new[] { new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },  //mp IsConsumed = true - compare with building fishTrapCoast ???                               
                                          new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                          new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = true },
                                          new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },
                 */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), campPos);


            /* new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },  //mp IsConsumed = true - compare with building fishTrapCoast ???                               
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = true },
                                     new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },*/

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakShingles"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), campPos);

        }

        /// <summary>
        /// validate any code placed entities 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// 
        public static void CollisionTest()
        {

            /*  if (!The.Sim.LoadMap("c Micromap Test")) //The.Map.AllMaps[12]))
                  return;*/

            The.MapUI.ZoomToMapPosition(10, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 9)));


            //  The.Sim.Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);


            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;



            int i = 0;


            Entities.Body.Body body;



            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]);
            AddColonyItem(item, new Point(15, 9));

            for (int astroRationIndex = 0; astroRationIndex < 100; astroRationIndex++)
            {
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(15, 9));  //shortest way of placing stuuf
            }


            for (int playerIndex = 0; playerIndex < 4; playerIndex++)
            {
                Entity e1 = PlacePerson("Collisioneer", " " + playerIndex, Reproduction.Male, new Point(15 + playerIndex, 9), Color.White, 52f, false, "blue1", expedition);

                e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                //e1.BiologicalEntity.StomachContents = 1f;

            }

            AddFinishedStructure("structure:abatis", new Point(15, 7), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(10, 10), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(11, 10), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(12, 10), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 10)) + new Vector3(16f, 0f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 10)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(13, 10), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(13, 10)) + new Vector3(16f, 0f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(13, 10)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(14, 10), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(14, 10)) + new Vector3(16f, 0f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(14, 10)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(15, 10), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(15, 10)) + new Vector3(16f, 0f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(15, 10)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:campfire", new Point(35, 11), expedition, false);
            AddFinishedStructure("structure:campfirePotCrane", new Point(39, 13), expedition, false);
            AddFinishedStructure("structure:lean-toTarp", new Point(30, 12), expedition, false);
            AddFinishedStructure("structure:lean-toSpoakLeaves", new Point(30, 15), expedition, false);
            AddFinishedStructure("structure:A-frameSpoakLeaves", new Point(36, 15), expedition, false);
            AddFinishedStructure("structure:A-frameScraps", new Point(39, 15), expedition, false);
            AddFinishedStructure("structure:A-frameTarp", new Point(32, 17), expedition, false);
            AddFinishedStructure("structure:lean-toScraps", new Point(31, 19), expedition, false);
            AddFinishedStructure("structure:domeShelterTarp", new Point(34, 19), expedition, false);
            AddFinishedStructure("structure:domeShelterSpoakShingles", new Point(38, 19), expedition, false);
            AddFinishedStructure("structure:wigwamSpoakShingles", new Point(30, 23), expedition, false);
            AddFinishedStructure("structure:daysheenTipi", new Point(28, 23), expedition, false);
            AddFinishedStructure("structure:skimmerHull", new Point(33, 22), expedition, false);
            AddFinishedStructure("structure:skimmerTail", new Point(34, 24), expedition, false);
            AddFinishedStructure("structure:skimmerEngineSide", new Point(28, 25), expedition, false);
            AddFinishedStructure("structure:storageHole", new Point(30, 25), expedition, false);
            AddFinishedStructure("structure:smokeOven", new Point(32, 25), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(34, 26), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(35, 26), expedition, false);
            AddFinishedStructure("structure:skimmerHull", new Point(8, 2), expedition, false);
            AddFinishedStructure("structure:skimmerTail", new Point(6, 4), expedition, false);
            AddFinishedStructure("structure:skimmerEngineSide", new Point(9, 2), expedition, false);
            AddFinishedStructure("structure:storageHole", new Point(8, 4), expedition, false);
            AddFinishedStructure("structure:smokeOven", new Point(9, 3), expedition, false);
            AddFinishedStructure("structure:lean-toTarp", new Point(10, 6), expedition, false);


            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(6, 7));


            Allegiances.Allegiance chickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

        }


        private static bool CheckValidEntity(Entity entity)
        {
            if (entity.Bulk == 0f)
            {
                throw new Exception("Bulk has not been set on entity");
            }

            return true;
            /*if (entity.EntityType.ItemType != null)
            {
                if (entity)
            }*/
        }

        public static Entity AddColonyItemUpgrade(string type /*Entity item*/, Entity container, UpgradeCategory upgradeCategory, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
        {

            Entity item = new Entity(GameData.Instance.AllEntityTypes[type]);
            item.Initialize(The.Sim.PlaySite);
            item.InitializeModelAndOnScreenFunctionality();

            CheckValidEntity(item);

            item.PlaceEntityOnPlaySite(null, container, null, new Entity.SetOwnerInfo(The.Sim.PlaySite.GetFirstPlayerExpedition(), giveNewOwnerInfo), upgradeCategory: upgradeCategory);

            return item;
        }

        public static Entity AddColonyItem(Entity item, Vector3 pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
        {

            item.Initialize(The.Sim.PlaySite);
            item.InitializeModelAndOnScreenFunctionality();

            CheckValidEntity(item);

            item.PlaceEntityOnPlaySite(pos, null, null, new Entity.SetOwnerInfo(The.Sim.PlaySite.GetFirstPlayerExpedition(), giveNewOwnerInfo));

            return item;
        }

        private static Entity AddColonyItem(string type, Point pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
        {
            return AddColonyItem(new Entity(GameData.Instance.AllEntityTypes[type]), MapManager.TileToWorldPos(pos), giveNewOwnerInfo);
        }

        private static Entity AddColonyItem(Entity item, Point pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
        {
            return AddColonyItem(item, MapManager.TileToWorldPos(pos), giveNewOwnerInfo);
        }

        private static void AddNoOwnerItem(Entity item, Point pos)
        {

            item.Initialize(The.Sim.PlaySite);
            item.InitializeModelAndOnScreenFunctionality();

            CheckValidEntity(item);

            item.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos), null, null, null);

            // item.PlaceNewEntityInTheOpen(MapManager.TileToWorldPos(pos), null, Entity.AddRandomOffset.No);
        }


        private static void AddTerrainItem(Entity item, Point pos)
        {
            AddTerrainItem(item, MapManager.TileToWorldPos(pos));
        }


        private static void AddTerrainItem(Entity item, Vector3 pos)
        {


            item.Initialize(The.Sim.PlaySite);
            item.InitializeModelAndOnScreenFunctionality();

            //  CheckValidEntity(item);

            item.PlaceEntityOnPlaySite(pos, null, null, null);

        }



        /*
        private static void AddColonyItem(Entity item, Point tilePos, Vector2 pos)
        {
            Owner ColonyOwner = The.Sim.Site.GetMainExpedition().ExpeditionOwner;

            item.Initialize(The.Sim.Site);
            item.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

            item.PlaceNewEntityInTheOpen(MapManager.TileToWorldPos(tilePos) + new Vector3(pos.X, pos.Y, 0f), ColonyOwner, Entity.AddRandomOffset.No);
        }
        */

        private static void AddColonyItemToStorage(Entity item, Entity container, StorageCompartment? compartment = null)
        {

            item.Initialize(The.Sim.PlaySite);
            item.InitializeModelAndOnScreenFunctionality();

            CheckValidEntity(item);

            //   container.Contains.
            if (!container.Contains.AddToContain(item, compartment))
            {
                // no room...
                throw new Exception();
            }
            else
            {

                IOwner ColonyOwner = The.Sim.PlaySite.GetFirstPlayerExpedition();
                item.ChangeOwnership(ColonyOwner);
            }
        }

        /*  public Entity PlaceWeapon(string entityKey, Point position, Owner newOwner)
          {
              Entity entity = new Entity(GameData.Instance.AllEntityTypes[entityKey]);
              return PlaceVehicle(entity, position, newOwner);//TODO, don't place vehicles as weapons, 
          }*/



        /*  public static Entity PlaceVehicle(Entity entity, Point pos, Owner newOwner)
          {

              // entity.PlaceEntityOnTile(position.X, position.Y);
              //Entities.Add(entity);

              //UWGame.SimSide.Instance.map.TileMap[MapPosition.X, MapPosition.Y].AddVehicle(this);

              //  entity.PlaceGroundFeature(pos, null, MapManager.TileToWorldPos(pos));

              entity.Initialize(The.Sim.Site);
              entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

              //TODO DECOUPLE
              entity.Renderable.RenderAsModel.CustomColor0 = new Vector3(239f, 237f, 196f) / 255f; // add color scheme
              entity.Renderable.RenderAsModel.CustomColor1 = new Vector3(42f, 119f, 40f) / 255f;
              entity.Renderable.RenderAsModel.CustomColor2 = new Vector3(0f, 0f, 0f) / 255f;
              entity.Renderable.RenderAsModel.CustomColor3 = new Vector3(0f, 0f, 0f) / 255f;

              entity.PlaceInWorld(pos, null, MapManager.TileToWorldPos(pos));


              if (newOwner != null)
              {
                  entity.ChangeOwnership(newOwner); //belongingCollection, ownership);
              }

              return entity;
          }*/



        public static Entity PlaceRobot(string entityKey, Point pos,
                            Allegiances.Allegiance allegiance, Expedition expedition)
        {
            AllegianceAndExpedition owner = null;
            AllegianceAndExpedition memberOf = null;


            /* if (expedition != null)
             {*/
            owner = new AllegianceAndExpedition() { AllegianceKey = allegiance.KeyName, ExpeditionKey = expedition.KeyName };
            memberOf = new AllegianceAndExpedition() { AllegianceKey = allegiance.KeyName, ExpeditionKey = expedition.KeyName };
            /* }
             else
             { //??
                 memberOf = new AllegianceAndExpedition();
                 if (allegiance != null)
                 {
                     memberOf.AllegianceKey = allegiance.KeyName;
                 }
             }*/


            EntityData entityData = new EntityData()
            {
                EntityKey = entityKey,
                Location = MapManager.TileToWorldPos(pos),
                MemberOf = memberOf,
                OwnedBy = owner
            };

            bool placeFailed;
            Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placeFailed);

            if (entity != null)
            {
                TestEntityNotPlacedOnblockedTerrain(entity);

            }



            /*
            Entity entity = new Entity(GameData.Instance.AllEntityTypes[entityKey]);
            entity.Initialize(The.Sim.PlaySite, allegiance);
            entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);
                      

            entity.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos), null, null, expedition, expedition);


            TestEntityNotPlacedOnblockedTerrain(entity);

            entity.ComeOnline();
            */

            return entity;
        }



        public static Entity PlaceAnimal(string entityKey, Reproduction? /*Sim.PersonSex?*/ sex, Point pos, float age,
                            Allegiances.Allegiance allegiance, string raceKey = null, Expedition ownerExpedition = null)
        {

            AllegianceAndExpedition owner = null;
            AllegianceAndExpedition memberOf = null;
            if (ownerExpedition != null)
            {
                owner = new AllegianceAndExpedition() { AllegianceKey = allegiance.KeyName, ExpeditionKey = ownerExpedition.KeyName };
                memberOf = new AllegianceAndExpedition() { AllegianceKey = allegiance.KeyName, ExpeditionKey = ownerExpedition.KeyName };
            }
            else
            {
                memberOf = new AllegianceAndExpedition();
                if (allegiance != null)
                {
                    memberOf.AllegianceKey = allegiance.KeyName;
                }
            }

            string casteKey = null;
            if (sex.HasValue)
            {
                EntityType entityType = GameData.Instance.AllEntityTypes[entityKey];
                CasteType casteType = entityType.BiologicalType.Castes.FirstOrDefault(c => c.Reproduction == sex.Value);
                if (casteType != null)
                {
                    casteKey = casteType.KeyName;
                }
            }

            EntityData entityData = new EntityData()
            {
                EntityKey = entityKey,
                Location = MapManager.TileToWorldPos(pos),
                BioEntity = new Maps.MapEditor.BiologicalEntity()
                {
                    AgeInYears = new NormalDistribution() { Mean = age },
                    RaceKey = raceKey,
                    CasteKey = casteKey
                },
                MemberOf = memberOf,
                OwnedBy = owner
            };

            bool placeFailed;
            Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placeFailed);

            if (entity != null)
            {
                TestEntityNotPlacedOnblockedTerrain(entity);

            }

            /*  Entity entity = new Entity(GameData.Instance.AllEntityTypes[entityKey]);
              InitializeBioEntityToPlace(sex, age, null, allegiance, raceKey, entity);

              entity.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos), null, null, ownerExpedition);

              TestEntityNotPlacedOnblockedTerrain(entity);

              entity.ComeOnline();
              */

            return entity;
        }


        /* public static Entity PlaceAnimal(string entityKey, Sim.PersonSex sex, Vector2 location, float age, 
                 Allegiances.Allegiance allegiance = null, string raceName = null, bool testLocation = true)  
         {
             Entity entity = new Entity(GameData.Instance.AllEntityTypes[entityKey]);

             InitializeBioEntityToPlace(sex, age, null, allegiance, raceName, entity);

             entity.PlaceInWorld(MapManager.WorldPosToTile(location), null, location.ToVector3());

             if (testLocation)
             {
                 TestEntityNotPlacedOnblockedTerrain(entity);
             }

             return entity;
         }*/

        public static void PlaceGameEntitiesMilestoneBuild()
        {
            /*  if (!The.Sim.LoadMap("a Map Demo March2013")) //The.Map.AllMaps[10]))
                  return;*/

            The.MapUI.ZoomToMapPosition(215, 95);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(216, 95)));



            GameData.Instance.AllEntityTypes["entity:human"].SensorType.Range = 1200;

            //     Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            //     Entity twinkler; //202,83

            //   Entity ward = PlacePerson("Ward", "Conlan", PersonSex.Male, new Point(10, 10), Color.White, 52f, false);
            //    ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            /*
            ward.Intelligence.Morale = 0.3f;*/

            //   int j = 0;
            /*      for (int x = 10; x <= 18; x+=2)
                  {
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);  
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);

                     for (int y = 9; y < 16; y+=2)
                      {
                          twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, y), 20, twinklerAllegiance);
                          //twinkler.MobileEntity.TestWander = true;
                      }               
                  }
                  */
            //     twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(18, 20), 20, twinklerAllegiance);
            //       twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(16, 10), 20, twinklerAllegiance);
            //     twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(16, 10), 20, twinklerAllegiance);

            int i = 0;

            /*    man = PlacePerson("Karol Nikolaev " + i, PersonSex.Male, new Point(20, 15), Color.White, 40f, false);
                man.PersonEntity.HasEatenToday = true;
           //     man.Intelligence.Morale = 0.5f;
                i++;*/
            Entities.Body.Body body;


            /*      TerrainTile tile = The.Map.TileMap[216][100];
                  tile.Temperature = 273; // 0 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[217][100];
                  tile.Temperature = 283; // 10 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[218][100];
                  tile.Temperature = 294; // 21 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[219][100];
                  tile.Temperature = 303; // 30 degrees
                  tile.Moisture = 0f;

           

                  The.Map.TileMap[216][101].Temperature = Storage.RefrigeratorTemperature;
                  The.Map.TileMap[217][101].Temperature = Storage.AirConTemperature;
                  The.Map.TileMap[218][101].Temperature = Storage.EarthCooledTemperature;
                  The.Map.TileMap[219][101].Temperature = Storage.FreezerTemperature;

                  */

            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(216, 100));
            /*      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(217, 100));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(218, 100));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(219, 100));
          
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(216, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(217, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(218, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(219, 101));

                  */



            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(215, 95), Color.White, 52f, false, "blue1", expedition);

            /*    e1.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.05f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.05f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.15f;  //0.2f
                e1.BiologicalEntity.StomachContents = 0f;*/


            Entity e2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(214, 96), Color.White, 52f, false, "blue2", expedition);



            /*  e2.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.02f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.1f;  //0.2f
              e2.BiologicalEntity.StomachContents = 0f;*/


            /* Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
             campfire.FlipHorizontally = true;
             campfire.Structure.AddBuildingToWorld(new Point(215, 96), Common.Direction.North, expedition.ExpeditionOwner);
             campfire.Initialize(The.Sim.Site);
             campfire.Structure.ConstructionFinished(true);
             */


            //    AddFinishedStructure("structure:storageHole", new Point(217, 96), expedition, false);

            AddFinishedStructure("structure:abatis", new Point(218, 97), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(218, 98), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(213, 100), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(214, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(214, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(215, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(215, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(216, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(216, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(217, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(217, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(218, 100), expedition, false);

            //    AddFinishedStructure("structure:improvisedTent1", new Point(218, 100), expedition, false);
            AddFinishedStructure("structure:A-frameTarp", new Point(217, 95), expedition, false);
            AddFinishedStructure("structure:skimmerHull", new Point(215, 98), expedition, false);
            AddFinishedStructure("structure:skimmerTail", new Point(213, 98), expedition, false);
            //  AddFinishedStructure("structure:skimmerEngineSide", new Point(215, 100), expedition, false);
            //    AddFinishedStructure("structure:storageHole", new Point(213, 100), expedition, false);
            //          AddFinishedStructure("structure:smokeOven", new Point(217, 96), expedition, false);



            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(216, 97));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
              carcass.Bulk = 10f;
              AddColonyItem(carcass, new Point(10, 5));
              */



            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(213, 95));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(215, 96));

            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(215, 96));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(215, 96));

            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                   AddColonyItem(item, new Point(215, 96));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                   AddColonyItem(item, new Point(215, 96));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                   AddColonyItem(item, new Point(215, 96));*/

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(215, 94));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(213, 95));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(215, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(215, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(214, 97));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(216, 97));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(216, 97));



            /*     Entity skimmer = PlaceVehicle("entity:skimmer", new Point(215, 93), expedition.ExpeditionOwner);
                 skimmer.Vehicle.Pitch = 0.3f;
                 skimmer.Vehicle.Roll = 0.22f;
                 skimmer.RenderAsModel.SetRotationAndDir(MathHelper.PiOver4);*/

            //   Entity rotor = skimmer.FindPartOfType(GameData.Instance.AllEntityTypes["item:skimmerrotor"]);
            //   rotor.Item.TurnIntoJunk(); // break it...

            /*Body skimmerBody;
            skimmer.Find(out skimmerBody);
            skimmerBody.FunctionalScore = 0f; // make sure it doesn't fly...
            */

            //skimmer.Condition = 0.09f;


            /*   for (int x = 8; x < 12; x += 3)
               {
                   for (int y = 5; y < 7; y += 2)
                   {
                       man = PlacePerson("Karol Nikolaev " + i, PersonSex.Male, new Point(x, y), Color.White, 40f, false);
                       man.PersonEntity.HasEatenToday = true;
                       i++;
                   }

               }*/

            //    PlaceAnimal("entity:patrician", PersonSex.Male, new Point(13, 8), 20);

            /*     Entity animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10576, 4353), 20, null, "Dark race");
                 animal.Intelligence.DisableAI = false; */

            Allegiances.Allegiance chickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


            Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(223, 88), 20, chickenAllegiance);


            chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(218, 84), 20, chickenAllegiance);



            //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(215, 95));


            //  PlaceAnimal("entity:forestGuardian", PersonSex.Male, new Point(18, 8), 20);

            /*  Entity entity = PlaceAnimal("entity:patrician", PersonSex.Male, new Point(14, 15), 20);
              entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
              */





            /*         Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);         

      
                       Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(214, 102), 20, twinklerAllegiance);  //twinklerAllegiance
                       twinkler.Find(out body);
                       body.ChangeMaxHitpoints(1.0f);0;

                       twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(215, 102), 20, twinklerAllegiance);
                       twinkler.Find(out body);
                       body.GlobalHitpoints = 8;

                       twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(216, 101), 20, twinklerAllegiance);
                       twinkler.Find(out body);
                       body.GlobalHitpoints = 5;      
          }*/

        }


        public static void PlaceGameEntitiesTestMap()
        {
            /* if (!The.Sim.LoadMap(The.Map.AllMaps[11]))
                 return;*/

            The.MapUI.ZoomToMapPosition(215, 95);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));



            GameData.Instance.AllEntityTypes["entity:human"].SensorType.Range = 1200;

            //     Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            //     Entity twinkler; //202,83

            //   Entity ward = PlacePerson("Ward", "Conlan", PersonSex.Male, new Point(10, 10), Color.White, 52f, false);
            //    ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            /*Body body;
            ward.Find(out body);
            body.GlobalHitpoints = 50;

            ward.Intelligence.Morale = 0.3f;*/

            //   int j = 0;
            /*      for (int x = 10; x <= 18; x+=2)
                  {
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);  
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);
                      twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, 10), 20, twinklerAllegiance);

                     for (int y = 9; y < 16; y+=2)
                      {
                          twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(x, y), 20, twinklerAllegiance);
                          //twinkler.MobileEntity.TestWander = true;
                      }               
                  }
                  */
            //     twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(18, 20), 20, twinklerAllegiance);
            //       twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(16, 10), 20, twinklerAllegiance);
            //     twinkler = PlaceAnimal("entity:twinkler", PersonSex.Male, new Point(16, 10), 20, twinklerAllegiance);

            int i = 0;

            /*    man = PlacePerson("Karol Nikolaev " + i, PersonSex.Male, new Point(20, 15), Color.White, 40f, false);
                man.PersonEntity.HasEatenToday = true;
           //     man.Intelligence.Morale = 0.5f;
                i++;*/
            Entities.Body.Body body;


            /*      TerrainTile tile = The.Map.TileMap[216][100];
                  tile.Temperature = 273; // 0 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[217][100];
                  tile.Temperature = 283; // 10 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[218][100];
                  tile.Temperature = 294; // 21 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[219][100];
                  tile.Temperature = 303; // 30 degrees
                  tile.Moisture = 0f;

           

                  The.Map.TileMap[216][101].Temperature = Storage.RefrigeratorTemperature;
                  The.Map.TileMap[217][101].Temperature = Storage.AirConTemperature;
                  The.Map.TileMap[218][101].Temperature = Storage.EarthCooledTemperature;
                  The.Map.TileMap[219][101].Temperature = Storage.FreezerTemperature;

                  */

            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(216, 100));
            /*      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(217, 100));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(218, 100));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(219, 100));
          
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(216, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(217, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(218, 101));
                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(219, 101));

                  */



            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(215, 95), Color.White, 52f, false, "blue1", expedition);

            /*    e1.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.05f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.05f; //0.2f
                e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.15f;  //0.2f
                e1.BiologicalEntity.StomachContents = 0f;*/


            Entity e2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(214, 95), Color.White, 52f, false, "blue2", expedition);


            Entity e3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(216, 94), Color.White, 52f, false, "blue2", expedition);


            /*  e2.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.02f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.1f;  //0.2f
              e2.BiologicalEntity.StomachContents = 0f;*/


            /*   Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
               campfire.FlipHorizontally = true;
               campfire.Structure.AddBuildingToWorld(new Point(215, 96), Common.Direction.North, expedition.ExpeditionOwner);
               campfire.Initialize(The.Sim.Site);
               campfire.Structure.ConstructionFinished(true);
               */


            //    AddFinishedStructure("structure:storageHole", new Point(217, 96), expedition, false);

            AddFinishedStructure("structure:abatis", new Point(218, 97), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(218, 98), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(213, 100), expedition, false);
            AddFinishedStructure("structure:abatis", new Point(214, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(214, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(215, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(215, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(216, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(216, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(217, 100), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(217, 100)) + new Vector3(24f, 0f, 0f));
            AddFinishedStructure("structure:abatis", new Point(218, 100), expedition, false);

            //    AddFinishedStructure("structure:improvisedTent1", new Point(218, 100), expedition, false);
            AddFinishedStructure("structure:A-frameTarp", new Point(217, 95), expedition, false);
            AddFinishedStructure("structure:skimmerHull", new Point(211, 98), expedition, false);
            AddFinishedStructure("structure:skimmerTail", new Point(210, 99), expedition, false);
            //  AddFinishedStructure("structure:skimmerEngineSide", new Point(215, 100), expedition, false);
            AddFinishedStructure("structure:storageHole", new Point(214, 96), expedition, false);
            AddFinishedStructure("structure:smokeOven", new Point(217, 96), expedition, false);



            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(216, 97));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
              carcass.Bulk = 10f;
              AddColonyItem(carcass, new Point(10, 5));
              */



            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(213, 95));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(215, 96));

            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(215, 96));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(215, 96));

            /*         item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                     AddColonyItem(item, new Point(215, 96));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                     AddColonyItem(item, new Point(215, 96));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                     AddColonyItem(item, new Point(215, 96));*/

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(215, 94));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(213, 95));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(215, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(215, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 96));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(214, 97));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(216, 97));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(216, 97));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(216, 97));



            /*     Entity skimmer = PlaceVehicle("entity:skimmer", new Point(215, 93), expedition.ExpeditionOwner);
                 skimmer.Vehicle.Pitch = 0.3f;
                 skimmer.Vehicle.Roll = 0.22f;
                 skimmer.RenderAsModel.SetRotationAndDir(MathHelper.PiOver4);*/

            //   Entity rotor = skimmer.FindPartOfType(GameData.Instance.AllEntityTypes["item:skimmerrotor"]);
            //   rotor.Item.TurnIntoJunk(); // break it...

            /*Body skimmerBody;
            skimmer.Find(out skimmerBody);
            skimmerBody.FunctionalScore = 0f; // make sure it doesn't fly...
            */

            //skimmer.Condition = 0.09f;


            /*   for (int x = 8; x < 12; x += 3)
               {
                   for (int y = 5; y < 7; y += 2)
                   {
                       man = PlacePerson("Karol Nikolaev " + i, PersonSex.Male, new Point(x, y), Color.White, 40f, false);
                       man.PersonEntity.HasEatenToday = true;
                       i++;
                   }

               }*/

            //    PlaceAnimal("entity:patrician", PersonSex.Male, new Point(13, 8), 20);

            /*     Entity animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10576, 4353), 20, null, "Dark race");
                 animal.Intelligence.DisableAI = false; */

            Allegiances.Allegiance chickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


            Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(221, 93), 20, chickenAllegiance);


            chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(218, 84), 20, chickenAllegiance);

            //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(215, 95));


            //  PlaceAnimal("entity:forestGuardian", PersonSex.Male, new Point(18, 8), 20);

            /*  Entity entity = PlaceAnimal("entity:patrician", PersonSex.Male, new Point(14, 15), 20);
              entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
              */





            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);


            Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(214, 102), 20, twinklerAllegiance);  //twinklerAllegiance

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(215, 102), 20, twinklerAllegiance);

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(216, 101), 20, twinklerAllegiance);



        }


        public static void TestMicroMap()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //The.Map.AllMaps[12]))
                  return;*/

            The.MapUI.ZoomToMapPosition(10, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            //Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(15, 9))) { Name = "Start", Allegiance = The.Sim.Site.PlayerAllegiance };
            //   The.Sim.Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);


            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;



            /*     int i = 0;
                 Entities.Body.Body body;
                 Entity item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                 AddColonyItem(item, new Point(15, 8));

                 Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);
                 e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                 e1.Find(out body);
                 body.ChangeMaxHitpoints(150.0f);
                 e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f; //0.2f
                 e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f; //0.2f
                 e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f; //0.2f
                 e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                 e1.BiologicalEntity.StomachContents = 8f;


                 Entity e2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(15, 10), Color.White, 52f, false, "blue2", expedition);
                 e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                 e2.Find(out body);
                 body.ChangeMaxHitpoints(150.0f);
                 e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
                 e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
                 e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
                 e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                 e2.BiologicalEntity.StomachContents = 1f;



                 Entity e3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(15, 11), Color.White, 52f, false, "grey1", expedition);
                 e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                 e3.Find(out body);
                 body.ChangeMaxHitpoints(150.0f);
                 e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
                 e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
                 e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
                 e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                 e3.BiologicalEntity.AddToStomachContents(1f);


                 Entity e4 = PlacePerson("Glen", "Tarkov", Reproduction.Male, new Point(15, 12), Color.White, 52f, false, "red1", expedition);
                 e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                 e4.Find(out body);
                 body.ChangeMaxHitpoints(150.0f);
                 e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
                 e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
                 e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
                 e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                 e4.BiologicalEntity.StomachContents = 1f;


               /*  Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
                 campfire.FlipHorizontally = true;
                 campfire.Structure.AddBuildingToWorld(new Point(16, 7), Common.Direction.North, expedition.ExpeditionOwner);
                 campfire.Initialize(The.Sim.Site);
                 campfire.Structure.ConstructionFinished(true);
                 */




            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
            AddColonyItem(item, new Point(15, 9));

            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
            AddColonyItem(item, new Point(15, 10));


            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 10));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 11));

            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(14, 9));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(14, 10));

            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(14, 11));
            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(15, 13));
            /*        item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
                    AddColonyItem(item, new Point(15, 8));*/
            item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
            AddColonyItem(item, new Point(15, 15));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(15, 8));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 9));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(14, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(14, 9));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(14, 10));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(16, 9));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(16, 10));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(17, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(15, 5));
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(15, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(16, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(15, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(15, 8));



            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(16, 9));
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(16, 8));

            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(16, 10));




            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 9));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 10));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 11));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(14, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(14, 9));



        }

        public static void QuarterSizeMap()
        {
            /*  if (!The.Sim.LoadMap("e Map Quartersize Oct 2013"))
                  return;*/

            The.MapUI.ZoomToMapPosition(35, 25);


            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));



            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;




            Entities.Body.Body body;

            bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
            //  e1.BiologicalEntity.StomachContents = 0.8f;

            e1.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");

            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();

            The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, e1);


            Entity e2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
            //e2.BiologicalEntity.StomachContents = 1f;

            e2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");

            e2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));




            Entity e3 = PlacePerson("Joaquin", "Lehner" /*"Kurt Mansell"*/, Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);

            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
            //e3.BiologicalEntity.StomachContents = 0.7f;

            e3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");

            e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));



            Entity e4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);

            e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //e4.BiologicalEntity.StomachContents = 0.9f;

            e4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");

            e4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));





            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP



            //----------------------------------------------------------
            // Nordic Game Final Milestone Demo Loadout:

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));


            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);



            //   ---------------------------------------------------------



            /*            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(40, 27), Common.Direction.North, expedition.ExpeditionOwner);
            campfire.Initialize(The.Sim.Site);
            campfire.Structure.ConstructionFinished(true);


                        Entity wigwam = new Entity(GameData.Instance.AllStructureTypes["structure:wigwamSpoakShingles"]);
                        wigwam.FlipHorizontally = true;
                        wigwam.Structure.AddBuildingToWorld(new Point(52, 27), Common.Direction.North, expedition.ExpeditionOwner);
                        wigwam.Initialize(The.Sim.Site);
                        wigwam.Structure.ConstructionFinished(true);


                        Entity smokeOven = new Entity(GameData.Instance.AllStructureTypes["structure:smokeOven"]);
                        smokeOven.FlipHorizontally = true;
                        smokeOven.Structure.AddBuildingToWorld(new Point(50, 28), Common.Direction.North, expedition.ExpeditionOwner);
                        smokeOven.Initialize(The.Sim.Site);
                        smokeOven.Structure.ConstructionFinished(true);

                        


/*
            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            item.Bulk = 1f;
            AddColonyItem(item, new Point(34, 23));

*/





            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(55, 41));

                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                   AddColonyItem(item, new Point(55, 41));
       */
            /*
            //testing stuff:
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
             * 
 
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(50, 26));
                        //

                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMachete"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
         

         
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(57, 39));

                          // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(57, 39));
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(34, 25));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(57, 39));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(57, 39));
       
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));



                    /*    item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                        item.Bulk = 1f;  // see comment
                        AddColonyItem(item, new Point(39, 28));
            */

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(57, 39));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(57, 39));
   */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));



            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(57, 39));
            */



            /* PARTICLES-----------------------------


             // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
             // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
             if (The.Client != null)
             {

                 //campfire
                 //The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(58, 41)), 0.2f, 2f);


                 //    The.Client.particleManager.AddDustStormPlume(MapManager.TileToWorldPosVector2(new Point(63, 33)), 2.2f, 0.3f);  //classic duststorm, not migrated to the new emittersystem setup yet.
             }

             //fire burning with sparks
             /*    The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(55, 44)));
                 The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(55, 44)), 0.5f, 1f);
        */


            /* moved to events
            The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(35, 24))+ new Vector2(-5, 5), 0.5f, null);  //the last 2 numbers are size and time between emitting...I think MP oct  2013
            */

            //  For testing the new, smaller campfire: the results are put in: new PrepareAction("lightFire")  in GameDataLoader..
            /*    The.Client.ParticleManager.AddEmitter("smallerSmoke", MapManager.TileToWorldPosVector2(new Point(40, 27))); 
                  The.Client.ParticleManager.AddEmitter("smallFire", MapManager.TileToWorldPosVector2(new Point(40, 27)));
           */


            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));



            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)), null, null);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f, null);


            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f, null);

            //swamp fog

            /*       Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
                   AddTerrainItem(fogInstance, new Point(32, 39));
                   //fogInstance.Initialize(The.Sim.Site);
       */
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(14, 9)), 8f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(11, 11)), 7f, 2f);



            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);

            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */



            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
            /*        The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

          */

        }


        public static void DemoIslandMap()
        {
            /*  if (!The.Sim.LoadMap("f Map DemoIsland Jan 2014"))
                  return;*/

            /* <StartDate>
            <TimeOfDay>0.36</TimeOfDay>
            <Day>1</Day>
            <Year>0</Year>
          </StartDate>*/

            // NEW: set the time in scenario, not from map - must be done BEFORE the events are registered - TimeCondition needs the date to init correctly
            The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear() { Year = 0, Day = 1, TimeOfDay = 0.36 });



            The.MapUI.ZoomToMapPosition(16, 51);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));



            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;




            Entities.Body.Body body;

            /*      bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
                  Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(16, 51), Color.White, 52f, noSkills, "grey1", expedition);
                  e1.PersonEntity.PersonalityType = GameData.Instance.AllPersonalityTypes["Conlan"];         
                  e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                  e1.Find(out body);
                  body.ChangeMaxHitpoints(150.0f);
                  e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
                  e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
                  e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
                  e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
                  e1.BiologicalEntity.StomachContents = 0.8f;

                  e1.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");

                  e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
                  e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

                  The.Sim.ExploreShroud(new TilePos(13, 51), new TilePos(0, 70), 9, 16, e1);


                  Entity e2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(15, 50), Color.White, 39f, noSkills, "blue2", expedition);
                  e2.PersonEntity.PersonalityType = GameData.Instance.AllPersonalityTypes["Yeboah"];
                  e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                  e2.Find(out body);
                  body.ChangeMaxHitpoints(150.0f);
                  e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
                  e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
                  e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
                  e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
                  e2.BiologicalEntity.StomachContents = 1f;

                  e2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");

                  e2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
                  e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));




                  Entity e3 = PlacePerson("Joaquin", "Lehner" , Reproduction.Male, new Point(15, 52), Color.White, 44f, noSkills, "green2", expedition);
                  e3.PersonEntity.PersonalityType = GameData.Instance.AllPersonalityTypes["Lehner"];
                  e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                  e3.Find(out body);
                  body.ChangeMaxHitpoints(60.0f);
                  e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
                  e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
                  e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f; //0.2f
                  e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
                  e3.BiologicalEntity.StomachContents = 0.7f;

                  e3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");

                  e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
                  e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));



                  Entity e4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(13, 49), Color.White, 38f, noSkills, "red1", expedition);
                  e4.PersonEntity.PersonalityType = GameData.Instance.AllPersonalityTypes["Khan"];                    
                  e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating
                  e4.Find(out body);
                  body.ChangeMaxHitpoints(150.0f);
                  e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f; //0.2f
                  e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
                  e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
                  e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
                  e4.BiologicalEntity.StomachContents = 0.9f;

                  e4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");

                  e4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
                  e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));


                  */


            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(13, 51), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f); //, false);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 50)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(14, 52)) + new Vector3(-16f, -16f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(11, 51), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP

            //----------------------------------------------------------
            // Nordic Game Final Milestone Demo Loadout:

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(16, 51));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(16, 51));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(16, 51));  //shortest way of placing stuuf

            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);





            // PARTICLES-----------------------------


            // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
            // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)



            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(20, 49)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(18, 43)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(19, 46)));



            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(44, 28)), null, 0.5f);
            //     The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
            //     The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
            //     The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)), null, null);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f, null);
            //        The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(33, 29)), 5f, null);
            //        The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 26)), 10f, null);


            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 16)), 8f, null);

            //swamp fog

            /*       Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
                   AddTerrainItem(fogInstance, new Point(32, 39));
                   //fogInstance.Initialize(The.Sim.Site);
       
      //      The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
      //      The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(14, 9)), 8f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(11, 11)), 7f, 2f);



            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);
*/
            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */

            /* PlaceAnimal("entity:bird", Reproduction.Male, new Point(10, 53), 20);
             PlaceAnimal("entity:bird", Reproduction.Male, new Point(10, 52), 20);
             PlaceAnimal("entity:bird", Reproduction.Male, new Point(9, 51), 20);
             PlaceAnimal("entity:bird", Reproduction.Male, new Point(9, 50), 20);*/

            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(11, 43)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(15, 37)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(10, 33)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(9, 29)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(14, 27)), 1f, 4f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(55, 17)), 1f, 3f);
            /*        The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

          */

        }


        public static void QuarterSizeMapBuildingTest()
        {
            /*  if (!The.Sim.LoadMap("e Map Quartersize Oct 2013"))
                  return;*/

            The.MapUI.ZoomToMapPosition(35, 25);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;



            Entities.Body.Body body;

            bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
            // e1.BiologicalEntity.StomachContents = 0.8f;

            e1.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");

            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, e1);



            Entity e2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
            //  e2.BiologicalEntity.StomachContents = 1f;

            e2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");

            e2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));




            Entity e3 = PlacePerson("Joaquin", "Lehner" /*"Kurt Mansell"*/, Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);

            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
            //e3.BiologicalEntity.StomachContents = 0.7f;

            e3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");

            e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));



            Entity e4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);

            e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //e4.BiologicalEntity.StomachContents = 0.9f;

            e4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");

            e4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));





            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP

            AddFinishedStructure("structure:lean-toTarp", null, expedition, false, new Vector3(1913f, 1167f, 0f)); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP


            //----------------------------------------------------------
            // Nordic Game Final Milestone Demo Loadout:

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));


            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);



            //   ---------------------------------------------------------



            /*            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(40, 27), Common.Direction.North, expedition.ExpeditionOwner);
            campfire.Initialize(The.Sim.Site);
            campfire.Structure.ConstructionFinished(true);


                        Entity wigwam = new Entity(GameData.Instance.AllStructureTypes["structure:wigwamSpoakShingles"]);
                        wigwam.FlipHorizontally = true;
                        wigwam.Structure.AddBuildingToWorld(new Point(52, 27), Common.Direction.North, expedition.ExpeditionOwner);
                        wigwam.Initialize(The.Sim.Site);
                        wigwam.Structure.ConstructionFinished(true);


                        Entity smokeOven = new Entity(GameData.Instance.AllStructureTypes["structure:smokeOven"]);
                        smokeOven.FlipHorizontally = true;
                        smokeOven.Structure.AddBuildingToWorld(new Point(50, 28), Common.Direction.North, expedition.ExpeditionOwner);
                        smokeOven.Initialize(The.Sim.Site);
                        smokeOven.Structure.ConstructionFinished(true);

                        


/*
            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            item.Bulk = 1f;
            AddColonyItem(item, new Point(34, 23));

*/





            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(55, 41));

                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                   AddColonyItem(item, new Point(55, 41));
       */

            //testing stuff:
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));

            /*
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(51, 28));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(51, 28));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(51, 28));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(50, 26));
                                   //

                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(34, 25));
                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(34, 25));
                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMachete"]), new Point(34, 25));
                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(34, 25));
                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
                            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
         

         
                             //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
                                   // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(57, 39));

                                     // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(57, 39));
                             //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(34, 25));
                                   // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(57, 39));
                                   // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(57, 39));
       
                             //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));
                             //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));



                               /*    item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                                   item.Bulk = 1f;  // see comment
                                   AddColonyItem(item, new Point(39, 28));
                       */

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(57, 39));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(57, 39));
   */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));



            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(57, 39));
            */



            /* PARTICLES-----------------------------


             // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
             // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
             if (The.Client != null)
             {

                 //campfire
                 //The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(58, 41)), 0.2f, 2f);


                 //    The.Client.particleManager.AddDustStormPlume(MapManager.TileToWorldPosVector2(new Point(63, 33)), 2.2f, 0.3f);  //classic duststorm, not migrated to the new emittersystem setup yet.
             }

             //fire burning with sparks
             /*    The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(55, 44)));
                 The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(55, 44)), 0.5f, 1f);
        */


            /* moved to events
            The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(35, 24))+ new Vector2(-5, 5), 0.5f, null);  //the last 2 numbers are size and time between emitting...I think MP oct  2013
            */

            //  For testing the new, smaller campfire: the results are put in: new PrepareAction("lightFire")  in GameDataLoader..
            /*    The.Client.ParticleManager.AddEmitter("smallerSmoke", MapManager.TileToWorldPosVector2(new Point(40, 27))); 
                  The.Client.ParticleManager.AddEmitter("smallFire", MapManager.TileToWorldPosVector2(new Point(40, 27)));
           */


            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));



            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)), null, null);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f, null);


            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f, null);

            //swamp fog

            /*       Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
                   AddTerrainItem(fogInstance, new Point(32, 39));
                   //fogInstance.Initialize(The.Sim.Site);
       */
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(14, 9)), 8f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(11, 11)), 7f, 2f);



            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);

            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */



            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
            /*        The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

          */

        }

        public static void QuarterSizeMap_Alt_Test_v1()
        {
            /*  if (!The.Sim.LoadMap("e Map Quartersize Oct 2013"))
                  return;*/

            The.MapUI.ZoomToMapPosition(35, 25);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));



            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;




            Entities.Body.Body body;

            bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
            //e1.BiologicalEntity.StomachContents = 0.8f;

            e1.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");

            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, e1);

            Entity e2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
            //e2.BiologicalEntity.StomachContents = 1f;

            e2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");

            e2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
            e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));




            Entity e3 = PlacePerson("Joaquin", "Lehner" /*"Kurt Mansell"*/, Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);

            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
            //e3.BiologicalEntity.StomachContents = 0.7f;

            e3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");

            e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));



            Entity e4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);

            e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //e4.BiologicalEntity.StomachContents = 0.9f;

            e4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");

            e4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
            e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));





            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f); //, false);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP



            //----------------------------------------------------------
            // Nordic Game Final Milestone Demo Loadout:
            /*
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));  //shortest way of placing stuuf
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));

            */
            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);



            //   ---------------------------------------------------------

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:streakFin"]), new Point(38, 25));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(38, 25));

            //

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedChitinousArrow"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvedFireExtinguisher"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]), new Point(38, 25));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));

            /*
                                    Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
                        campfire.FlipHorizontally = false;
                        campfire.Structure.AddBuildingToWorld(new Point(40, 27), Common.Direction.North, expedition.ExpeditionOwner);
                        campfire.Initialize(The.Sim.Site);
                        campfire.Structure.ConstructionFinished(true);


                                    Entity wigwam = new Entity(GameData.Instance.AllStructureTypes["structure:wigwamSpoakShingles"]);
                                    wigwam.FlipHorizontally = false;
                                    wigwam.Structure.AddBuildingToWorld(new Point(52, 27), Common.Direction.North, expedition.ExpeditionOwner);
                                    wigwam.Initialize(The.Sim.Site);
                                    wigwam.Structure.ConstructionFinished(true);


                                    Entity smokeOven = new Entity(GameData.Instance.AllStructureTypes["structure:smokeOven"]);
                                    smokeOven.FlipHorizontally = false;
                                    smokeOven.Structure.AddBuildingToWorld(new Point(50, 28), Common.Direction.North, expedition.ExpeditionOwner);
                                    smokeOven.Initialize(The.Sim.Site);
                                    smokeOven.Structure.ConstructionFinished(true);

                        


            /*
                        item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
                        item.Bulk = 1f;
                        AddColonyItem(item, new Point(34, 23));

            */





            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(55, 41));

                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                   AddColonyItem(item, new Point(55, 41));
       */
            /*
            //testing stuff:
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));  //shortest way of placing stuuf
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
             * 
 
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(51, 28));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(50, 26));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(50, 26));
                        //

                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMachete"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
                 //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
         

         
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(57, 39));

                          // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(57, 39));
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(34, 25));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(57, 39));
                        // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(57, 39));
       
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));
                  //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));



                    /*    item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                        item.Bulk = 1f;  // see comment
                        AddColonyItem(item, new Point(39, 28));
            */

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(57, 39));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(57, 39));
   */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));



            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(57, 39));
            */



            /* PARTICLES-----------------------------


             // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
             // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
             if (The.Client != null)
             {

                 //campfire
                 //The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(58, 41)), 0.2f, 2f);


                 //    The.Client.particleManager.AddDustStormPlume(MapManager.TileToWorldPosVector2(new Point(63, 33)), 2.2f, 0.3f);  //classic duststorm, not migrated to the new emittersystem setup yet.
             }

             //fire burning with sparks
             /*    The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(55, 44)));
                 The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(55, 44)), 0.5f, 1f);
        */


            /* moved to events
            The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(35, 24))+ new Vector2(-5, 5), 0.5f, null);  //the last 2 numbers are size and time between emitting...I think MP oct  2013
            */

            //  For testing the new, smaller campfire: the results are put in: new PrepareAction("lightFire")  in GameDataLoader..
            /*    The.Client.ParticleManager.AddEmitter("smallerSmoke", MapManager.TileToWorldPosVector2(new Point(40, 27))); 
                  The.Client.ParticleManager.AddEmitter("smallFire", MapManager.TileToWorldPosVector2(new Point(40, 27)));
           */


            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));



            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)), null, null);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f, null);


            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f, null);

            //swamp fog

            /*       Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
                   AddTerrainItem(fogInstance, new Point(32, 39));
                   //fogInstance.Initialize(The.Sim.Site);
       */
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(14, 9)), 8f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(11, 11)), 7f, 2f);



            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);

            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */



            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
            /*        The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

          */

        }


        public static void AnimTweak()
        {
            /* if (!The.Sim.LoadMap("e Map Quartersize Oct 2013"))
                 return;*/

            The.MapUI.ZoomToMapPosition(35, 25);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));



            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;



            Entities.Body.Body body;

            bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "blue1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
            //e1.BiologicalEntity.StomachContents = 0.8f;

            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, e1);



            /*
            Entity e3 = PlacePerson("Stefan Zima", Reproduction.Male, new Point(37, 26), Color.White, 52f, noSkills, "grey1", expedition);
            e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating
            e3.Find(out body);
            body.ChangeMaxHitpoints(60.0f);
            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
            e3.BiologicalEntity.StomachContents = 0.7f;

            e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
            e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));

            */


            /*            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
                        campfire.FlipHorizontally = true;
                        campfire.Structure.AddBuildingToWorld(new Point(40, 27), Common.Direction.North, expedition.ExpeditionOwner);
                        campfire.Initialize(The.Sim.Site);
                        campfire.Structure.ConstructionFinished(true);
*/


            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP



            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:inactivatedFoodCoolerUnit"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);











            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(55, 41));

                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                   AddColonyItem(item, new Point(55, 41));
       */
            /*
            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
            AddColonyItem(item, new Point(57, 39));
            */

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(34, 25));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(34, 25));
            //       AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMachete"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(34, 25));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(57, 39));

            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(34, 25));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(57, 39));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(34, 25));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(34, 25));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(34, 25));



            /*    item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                item.Bulk = 1f;  // see comment
                AddColonyItem(item, new Point(39, 28));
    */

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(57, 39));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(57, 39));
   */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));



            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(57, 39));
            */



            /* PARTICLES-----------------------------


             // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
             // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
             if (The.Client != null)
             {

                 //campfire
                 //The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(58, 41)), 0.2f, 2f);


                 //    The.Client.particleManager.AddDustStormPlume(MapManager.TileToWorldPosVector2(new Point(63, 33)), 2.2f, 0.3f);  //classic duststorm, not migrated to the new emittersystem setup yet.
             }

             //fire burning with sparks
             /*    The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(55, 44)));
                 The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(55, 44)), 0.5f, 1f);
        */

            The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(35, 24)) + new Vector2(-5, 5), 0.5f, null);  //the last 2 numbers are size and time between emitting...I think MP oct  2013


            //  For testing the new, smaller campfire: the results are put in: new PrepareAction("lightFire")  in GameDataLoader..
            /*    The.Client.ParticleManager.AddEmitter("smallerSmoke", MapManager.TileToWorldPosVector2(new Point(40, 27))); 
                  The.Client.ParticleManager.AddEmitter("smallFire", MapManager.TileToWorldPosVector2(new Point(40, 27)));
           */


            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));



            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)), null, null);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f, null);


            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f, null);

            //swamp fog

            /*       Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
                   AddTerrainItem(fogInstance, new Point(32, 39));
                   //fogInstance.Initialize(The.Sim.Site);
       */
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(14, 9)), 8f, 2f);
            //        The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(11, 11)), 7f, 2f);



            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);

            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */



            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
            /*        The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
                      The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

          */

        }


        public static void ModelTest()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            The.MapUI.ZoomToMapPosition(10, 5);

            Allegiance a = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:skinnedtest"]);

            Entity e1 = PlaceAnimal("entity:skinnedtest", Reproduction.Male, new Point(10, 5), 20, a); //, expedition.Allegiance, null, expedition);
            //MP: this takes a couple seconds before it shows up ! about 2 sec before the model appears.


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:meshTest"]), new Point(9, 9));


            Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            carcass.Bulk = 1f;
            AddColonyItem(carcass, new Point(9, 7));

            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);


        }




        public static void SleepInShiftsTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //The.Map.AllMaps[12]))
                  return;*/

            The.MapUI.ZoomToMapPosition(15, 10);

            The.Sim.DateAndTime.TimeOfDay = 0.3;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            //expedition.Policy.IndependentsToStayAwake = 3;
            expedition.Policy.IndependentsAllowedToSleep = 5;

            /*    Entity dog = PlaceAnimal("entity:dog", Reproduction.Male, new Point(15, 9), 5f, The.Sim.PlaySite.PlayerAllegiance);
                dog.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
                dog.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;
                */

            Entity e1 = PlacePerson("Sleepiest", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;  //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;

            /*  Entity e2 = PlacePerson("2nd Sleepiest", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.05f;  //0.2f

              e2 = PlacePerson("3rd Sleepiest", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.06f;  //0.2f

              e2 = PlacePerson("4th Sleepiest", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
            */

        }

        public static void FindPreyTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //The.Map.AllMaps[12]))
                  return;*/

            The.MapUI.ZoomToMapPosition(10, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;

            int i = 0;
            Entities.Body.Body body;
            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(15, 8));

            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            // e1.BiologicalEntity.StomachContents = 8f;

            Entity e2 = PlacePerson("The Hillismo", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, false, "blue1", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //e2.BiologicalEntity.StomachContents = 8f;

            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
            AddColonyItem(item, new Point(15, 9));

            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
            AddColonyItem(item, new Point(15, 10));
        }


        public static void MapApril2013()
        {
            /* if (!The.Sim.LoadMap("b Map Halfsize April 2013"))
                 return;*/

            The.MapUI.ZoomToMapPosition(55, 40);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;




            int i = 0;


            Entities.Body.Body body;


            // For degradation/storage testing:

            /*      TerrainTile tile = The.Map.TileMap[216][100];
                  tile.Temperature = 273; // 0 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[217][100];
                  tile.Temperature = 283; // 10 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[218][100];
                  tile.Temperature = 294; // 21 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[219][100];
                  tile.Temperature = 303; // 30 degrees
                  tile.Moisture = 0f;

           

                  The.Map.TileMap[216][101].Temperature = Storage.RefrigeratorTemperature;
                  The.Map.TileMap[217][101].Temperature = Storage.AirConTemperature;
                  The.Map.TileMap[218][101].Temperature = Storage.EarthCooledTemperature;
                  The.Map.TileMap[219][101].Temperature = Storage.FreezerTemperature;

                  */

            /*    Entity item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
             AddColonyItem(item, new Point(55, 40));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(217, 100));
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(218, 100));
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(219, 100));
          
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(216, 101));
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(217, 101));
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(218, 101));
                      item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                      AddColonyItem(item, new Point(219, 101));

                      */



            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(57, 40), Color.White, 52f, false, "blue1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.6f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.7f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.6f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
            //e1.BiologicalEntity.StomachContents = 0.5f;


            Entity e2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(58, 40), Color.White, 52f, false, "blue2", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.5f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.4f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.3f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
            //  e2.BiologicalEntity.StomachContents = 0.6f;



            Entity e3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(59, 41), Color.White, 52f, false, "grey1", expedition);

            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.4f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.5f;  //0.2f
            //e3.BiologicalEntity.StomachContents = 0.7f;


            Entity e4 = PlacePerson("Glen", "Tarkov", Reproduction.Male, new Point(60, 41), Color.White, 52f, false, "red1", expedition);

            e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.5f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.4f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.5f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.3f;  //0.2f
            //  e4.BiologicalEntity.StomachContents = 0.4f;


            /*  e2.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.02f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.1f;  //0.2f
              e2.BiologicalEntity.StomachContents = 0f;*/


            /*  Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
              campfire.FlipHorizontally = true;
              campfire.Structure.AddBuildingToWorld(new Point(58, 41), Common.Direction.North, expedition.ExpeditionOwner);
              campfire.Initialize(The.Sim.Site);
              campfire.Structure.ConstructionFinished(true);


              */


            //    AddFinishedStructure("structure:storageHole", new Point(217, 96), expedition, false);

            /*      AddFinishedStructure("structure:abatis", new Point(15, 7), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 8), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 9), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 10), expedition, false);
                  AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 10)) + new Vector3(24f, 0f, 0f));
                  AddFinishedStructure("structure:abatis", new Point(10, 10), expedition, false);
                  AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(13, 10)) + new Vector3(24f, 0f, 0f));
                  AddFinishedStructure("structure:abatis", new Point(11, 10), expedition, false);
                  AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(14, 10)) + new Vector3(24f, 0f, 0f));
                  AddFinishedStructure("structure:abatis", new Point(12, 10), expedition, false);
                  AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(15, 10)) + new Vector3(24f, 0f, 0f));
                  AddFinishedStructure("structure:abatis", new Point(13, 10), expedition, false);*/

            //    AddFinishedStructure("structure:improvisedTent1", new Point(218, 100), expedition, false);
            //  AddFinishedStructure("structure:A-frameTarp", new Point(57, 39), expedition, false);
            //   AddFinishedStructure("structure:domeShelterTarp", new Point(57, 39), expedition, false);
            //   AddFinishedStructure("structure:lean-toSpoakLeaves", new Point(60, 40), expedition, false);

            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(55, 41), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f);

            //hull.NonLivingEntity.Condition = 0f;

            //hull.SetBrokenPart();

            //    AddFinishedStructure("structure:skimmerTail", new Point(35, 40), The.Sim.NoOwner, false); // expedition, false);   ..on other side of river

            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, MapManager.TileToWorldPos(new Point(54, 40)) + new Vector3(32f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(56, 42)) + new Vector3(-16f, -20f, 0f));
            AddFinishedStructure("structure:skimmerTail", new Point(53, 41), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP


            //  AddFinishedStructure("structure:storageHole", new Point(8, 4), expedition, false);
            //   AddFinishedStructure("structure:smokeOven", new Point(9, 3), expedition, false);

            // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
            // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
            if (The.Client != null)
            {

                //campfire
                //The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(58, 41)), 0.2f, 2f);


                //    The.Client.particleManager.AddDustStormPlume(MapManager.TileToWorldPosVector2(new Point(63, 33)), 2.2f, 0.3f);  //classic duststorm, not migrated to the new emittersystem setup yet.
            }

            //fire burning with sparks
            /*    The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(55, 44)));
                The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(55, 44)), 0.5f, 1f);
            

                The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(54, 39))+ new Vector2(-5, -5), 0.5f, null);
    */

            //Little ""steam" rising from pond
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(61, 42)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(63, 43)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(64, 42)));
            The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(65, 41)));


            //sulphurous lakes
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(29, 121)), null, null);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 122)), 2f, 0.5f);
            The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 122)), 3f, 0.5f);

            //desert haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(10, 111)), 4f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(13, 115)), 5f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(20, 120)), 10f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(30, 118)), 8f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(30, 105)), 10f, null);
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 122)), 8f, null);

            //north steppe haze
            The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(79, 17)), 10f, null);

            //swamp fog

            Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
            AddTerrainItem(fogInstance, new Point(32, 39));
            //fogInstance.Initialize(The.Sim.Site);

            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(48, 9)), 5f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(15, 20)), 10f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(30, 18)), 8f, 2f);
            The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 7f, 2f);

            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 2f, null);
            //      The.Client.ParticleManager.AddEmitter("groundFog", MapManager.TileToWorldPosVector2(new Point(28, 39)), 2f, null);

            /*  //marsh fog
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(75, 62)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(99, 85)), 5f, 1f);
              The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(90, 94)), 5f, 1f);
  */
            //spoak pollen
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(67, 37)), 1f, 3f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(95, 24)), 1f, 7f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 15)), 1f, 11f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(109, 27)), 1f, 15f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 33)), 1f, 4f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
            The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);

            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:inactivatedFoodCoolerUnit"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);









            /*       item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(55, 41));

                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                   AddColonyItem(item, new Point(55, 41));
       */
            /*
            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(57, 39));

            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
            AddColonyItem(item, new Point(57, 39));
            item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
            AddColonyItem(item, new Point(57, 39));
            */

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(54, 40));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(54, 40));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));

            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(57, 39));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(57, 39));*/
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(55, 40));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(57, 39));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(54, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(55, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(55, 39));


            item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
            item.Bulk = 1f;  // see comment
            AddColonyItem(item, new Point(57, 41));


            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(57, 39));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(57, 39));
   */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));



            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(57, 39));
            */

            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(57, 39));
            */
            /*        

                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
        
        */


            /*     Entity skimmer = PlaceVehicle("entity:skimmer", new Point(215, 93), expedition.ExpeditionOwner);
                 skimmer.Vehicle.Pitch = 0.3f;
                 skimmer.Vehicle.Roll = 0.22f;
                 skimmer.RenderAsModel.SetRotationAndDir(MathHelper.PiOver4);*/

            //   Entity rotor = skimmer.FindPartOfType(GameData.Instance.AllEntityTypes["item:skimmerrotor"]);
            //   rotor.Item.TurnIntoJunk(); // break it...

            /*Body skimmerBody;
            skimmer.Find(out skimmerBody);
            skimmerBody.FunctionalScore = 0f; // make sure it doesn't fly...
            */

            //skimmer.Condition = 0.09f;


            /*   for (int x = 8; x < 12; x += 3)
               {
                   for (int y = 5; y < 7; y += 2)
                   {
                       man = PlacePerson("Karol Nikolaev " + i, PersonSex.Male, new Point(x, y), Color.White, 40f, false);
                       man.PersonEntity.HasEatenToday = true;
                       i++;
                   }

               }*/

            //    PlaceAnimal("entity:patrician", PersonSex.Male, new Point(13, 8), 20);

            /*     Entity animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10576, 4353), 20, null, "Dark race");
                 animal.Intelligence.DisableAI = false; */






            /*        Allegiance.Allegiance chickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


                    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(65, 38), 20, chickenAllegiance);

                    chicken.Find(out body);
                    body.ChangeMaxHitpoints(1.0f);;

                    chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(67, 36), 20, chickenAllegiance);
                    chicken.Find(out body);
                    body.ChangeMaxHitpoints(1.0f);;
        */




            //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(215, 95));


            //  PlaceAnimal("entity:forestGuardian", PersonSex.Male, new Point(18, 8), 20);

            /*  Entity entity = PlaceAnimal("entity:patrician", PersonSex.Male, new Point(14, 15), 20);
              entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
              */



            /*

                        Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);


                        Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(85, 20), 20, twinklerAllegiance);  //twinklerAllegiance
                        twinkler.Find(out body);
                        body.ChangeMaxHitpoints(1.0f);0;

                        twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(81, 19), 20, twinklerAllegiance);
                        twinkler.Find(out body);
                        body.GlobalHitpoints = 8;

                        twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(82, 21), 20, twinklerAllegiance);
                        twinkler.Find(out body);
                        body.GlobalHitpoints = 5;

            */
        }


        public static void WildernessCampScreenshot()
        {
            /*   if (!The.Sim.LoadMap("b Map Halfsize April 2013"))
                   return;*/

            The.MapUI.ZoomToMapPosition(105, 59);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            //   GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;



            int i = 0;


            Entities.Body.Body body;


            // For degradation/storage testing:

            /*      TerrainTile tile = The.Map.TileMap[216][100];
                  tile.Temperature = 273; // 0 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[217][100];
                  tile.Temperature = 283; // 10 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[218][100];
                  tile.Temperature = 294; // 21 degrees
                  tile.Moisture = 0f;

                  tile = The.Map.TileMap[219][100];
                  tile.Temperature = 303; // 30 degrees
                  tile.Moisture = 0f;

           

                  The.Map.TileMap[216][101].Temperature = Storage.RefrigeratorTemperature;
                  The.Map.TileMap[217][101].Temperature = Storage.AirConTemperature;
                  The.Map.TileMap[218][101].Temperature = Storage.EarthCooledTemperature;
                  The.Map.TileMap[219][101].Temperature = Storage.FreezerTemperature;

                  */

            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
            AddColonyItem(item, new Point(104, 60));
            /*         item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(217, 100));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(218, 100));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(219, 100));
          
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(216, 101));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(217, 101));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(218, 101));
                     item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(219, 101));

                     */



            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(104, 59), Color.White, 52f, false, "blue1", expedition);

            e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f; //0.2f
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //   e1.BiologicalEntity.StomachContents = 8f;


            Entity e2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(103, 61), Color.White, 52f, false, "blue2", expedition);

            e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //  e2.BiologicalEntity.StomachContents = 1f;



            Entity e3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(105, 60), Color.White, 52f, false, "grey1", expedition);

            e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //e3.BiologicalEntity.StomachContents = 1f;


            Entity e4 = PlacePerson("Glen", "Tarkov", Reproduction.Male, new Point(106, 59), Color.White, 52f, false, "red1", expedition);

            e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
            e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
            //  e4.BiologicalEntity.StomachContents = 1f;


            /*  e2.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.02f;  //0.2f
              e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.1f;  //0.2f
              e2.BiologicalEntity.StomachContents = 0f;*/


            /*     Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfirePotCrane"]);
                 campfire.FlipHorizontally = false;
                 campfire.Structure.AddBuildingToWorld(new Point(105, 59), Common.Direction.North, expedition.ExpeditionOwner);
                 campfire.Initialize(The.Sim.Site);
                 campfire.Structure.ConstructionFinished(true);

                 */

            //    AddFinishedStructure("structure:storageHole", new Point(217, 96), expedition, false);

            /*      AddFinishedStructure("structure:abatis", new Point(15, 7), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 8), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 9), expedition, false);
                  AddFinishedStructure("structure:abatis", new Point(15, 10), expedition, false);
              
             */


            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(101, 58)) + new Vector3(-18f, 18f, 0f));  //place it in the center and translate these pixels from the center. (3rd coordinate is not used because it's height)
            AddFinishedStructure("structure:abatis", new Point(101, 58), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(101, 58)) + new Vector3(18f, -18f, 0f));  //place it in the center and translate these pixels from the center. (3rd coordinate is not used because it's height)

            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(102, 57)) + new Vector3(0f, -18f, 0f));  //place it in the center and translate these pixels from the center. (3rd coordinate is not used because it's height)
            AddFinishedStructure("structure:abatis", new Point(102, 57), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(102, 57)) + new Vector3(-18f, 18f, 0f));  //place it in 

            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(107, 61)) + new Vector3(-18f, 18f, 0f));  //place it in the center and translate these pixels from the center. (3rd coordinate is not used because it's height)
            AddFinishedStructure("structure:abatis", new Point(107, 61), expedition, false);
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(107, 61)) + new Vector3(18f, -18f, 0f));  //place it in the center and translate these pixels from the center. (3rd coordinate is not used because it's height)

            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(107, 62)) + new Vector3(-18f, -18f, 0f));


            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(109, 59)) + new Vector3(18f, -18f, 0f));
            AddFinishedStructure("structure:abatis", null, expedition, false, MapManager.TileToWorldPos(new Point(109, 59)) + new Vector3(18f, 0f, 0f));



            //    AddFinishedStructure("structure:improvisedTent1", new Point(218, 100), expedition, false);

            AddFinishedStructure("structure:lean-toScraps", new Point(105, 58), expedition, false);
            AddFinishedStructure("structure:lean-toTarp", new Point(107, 59), expedition, false);

            /*   Entity tipi = new Entity(GameData.Instance.AllStructureTypes["structure:lean-toTarp"]);    //could not make this work (wanted to flip the building) ends up rendering wrong sprite...
               tipi.FlipHorizontally = false;
               tipi.Structure.AddBuildingToWorld(new Point(107, 59), Common.Direction.North, expedition.ExpeditionOwner);
   */
            AddFinishedStructure("structure:wigwamSpoakShingles", new Point(103, 58), expedition, false);
            AddFinishedStructure("structure:smokeOven", new Point(103, 60), expedition, false);








            /*         item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                     AddColonyItem(item, new Point(55, 40));

                     Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
                     carcass.Bulk = 10f;
                     AddColonyItem(carcass, new Point(10, 5));




         */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(104, 60));  //shortest way of placing stuuf
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(104, 60));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(104, 60));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(104, 60));


            // for tools test
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(55, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(55, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(55, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(55, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(55, 40));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), new Point(55, 40));
            */

            /*
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(56, 40));
            */
            /*        

                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
                    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(56, 40));
        
        */




        }


        /*    private static Entity AddFinishedStructure(string entityType, Point? pos, Expedition expedition, bool flipHorizontally, Vector3? location = null)
            {
                return AddFinishedStructure(entityType, pos, expedition, flipHorizontally, location);                       
            }*/

        public static Entity AddFinishedStructure(string entityType, Point? pos, IOwner owner, bool flipHorizontally = false, Vector3? location = null, string constructionProcessToUse = null)
        {
            Entity structure = new Entity(GameData.Instance.AllStructureTypes[entityType]);
            structure.FlipHorizontally = flipHorizontally;

            structure.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
            structure.InitializeModelAndOnScreenFunctionality();

            if (pos.HasValue)
            {
                structure.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos.Value), null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(owner));
            }
            else
            {
                structure.PlaceEntityOnPlaySite(location.Value, null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(owner));
            }

            structure.ComeOnline();

            // for testing traps etc:
            if (constructionProcessToUse != null)
            {
                ProcessType process = GameData.Instance.AllProcessTypes[constructionProcessToUse];
                Goal.FireEventActions(null, structure.ID, AgentActionHooks.CompletedProducing, // .CompletedConstructing,
                     process.EventActions,
                     AgentActionHooks.CompletedProducing, null);
            }

            return structure;
        }

        /* private static Entity AddFinishedStructure(Entity structure, Point? pos, IOwner owner, bool flipHorizontally, Vector3? location = null)
         {
             //Entity structure = new Entity(GameData.Instance.AllStructureTypes[entityType]);
             structure.FlipHorizontally = flipHorizontally;

             structure.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
             structure.InitializeModelAndOnScreenFunctionality();

             Vector3 locationToUse;
             if (pos.HasValue)
             {
                 locationToUse = MapManager.TileToWorldPos(pos.Value);
              
             }
             else
             {
                 locationToUse = location.Value;
             }
            
             structure.PlaceEntityOnPlaySite(locationToUse, null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(owner));
             //structure.ProductionFinished(true);
             structure.ComeOnline();

             return structure;
         }*/

        private static void AddRandomSkills(Entity entity, bool noSkills)
        {

            if (!noSkills)
            {

                foreach (var item in GameData.Instance.AllSkillTypes)
                {
                    if (!entity.Intelligence.Skills.ContainsKey(item.Value))
                    {
                        Skill skill = new Skill(Common.ClampTop((float)The.Sim.GameplayRandomGenerator.NextDouble("PlaceGameEntities") + 0.1f, 1f), item.Value);

                        entity.Intelligence.Skills.Add(item.Value, skill);
                    }
                }

                entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["armedMelee"]].Value = 0.7f;
                entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["shooting"]].Value = 0.7f;
                entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["unarmedFighting"]].Value = 0.7f;

            }
        }


        /* public static void InitializeBioEntityToPlace(Sim.PersonSex sex, float? age, AIAgeGroup? ageGroup, Allegiances.Allegiance allegiance, string raceKey, Entity entity)
         {
             entity.BiologicalEntity.CasteType = entity.EntityType.BiologicalType.Castes[sex == Reproduction.Female ? 1 : 0];

           
             if (!string.IsNullOrEmpty(raceKey))
             {
                 SetRaceOnNewEntity(raceKey, entity);
             }

             // must be set before Initialize:
             // entity.BiologicalEntity.SetRandomEntityStats(age);

             if (allegiance == null && The.Sim.Mode == Sim.EngineMode.Game) // don't set allegiances in edit mode.
             {
                 allegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, entity.EntityType);
             }

             // must be set before Initialize:          
             entity.BiologicalEntity.SetAgePreInit(age, ageGroup);

             entity.Initialize(The.Sim.PlaySite, allegiance);
             entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

             //    entity.BiologicalEntity.SetRandomEntityStats();

         }*/

        /*   public static void SetRaceOnNewEntity(string key, Entity entity)
           {
               entity.BiologicalEntity.SetRaceOnNewEntity(key);
           }

           public static void SetCasteOnNewEntity(string key, Entity entity)
           {
               entity.BiologicalEntity.SetCasteOnNewEntity(key);
           }*/




        private static void TestEntityNotPlacedOnblockedTerrain(Entity entity)
        {
            if (entity.Locomotor == null)
                return; //we don't care about immobile entities

            //if ( ! entity.isKindOf(KindOfType.Mobile) )
            //    return; //we don't care about immobile entities


            if (The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(entity.PlaySiteLocation)))
            {
                if (entity.EntityType.KeyName != "entity:bird") // Movie hack
                {
                    //throw new Exception("It seems like you placed the entity on a blocked subtile.");
                }
            }
        }

        public static Entity GetBob(Point? pos, Expedition exp, Entity container = null, bool useRandomValues = false, float skillValue = 1f)
        {
            Entity bob = PlacePerson("Bob", "Bobson", Reproduction.Male, pos, Color.Purple, 30f, false, exp, container: container);
            // bob.PersonEntity.UpdatePortrait(The.InGameUI.gui, "skimmerDark");
            foreach (SkillType skillType in GameData.Instance.AllSkillTypes.Values)
            {
                float value;
                if (useRandomValues)
                {
                    value = (float)The.Sim.GameplayRandomGenerator.NextDouble(null);
                }
                else value = skillValue;

                bob.Intelligence.Skills[skillType] = new Skill(value, skillType);
                //  bob.Intelligence.Skills.Add(skillType, new Skill(1.0f, skillType));
            }
            return bob;
        }

        private static Entity PlacePerson(string firstName, string lastName, Reproduction? sex, Point? pos, Color color, float age, bool noSkills,
            string raceKey, Expedition expedition, Entity container = null)
        {
            return PlacePerson(firstName, lastName, sex, pos, color, age, noSkills, expedition, raceKey, container);
        }

        private static Entity PlacePerson(string firstName, string lastName, Reproduction? sex, Point? pos, Color color, float age, bool noSkills,
            Expedition expedition, string raceKey = null, Entity container = null)
        {
            string entityKey = "entity:human";

            AllegianceAndExpedition memberOf = null;

            memberOf = new AllegianceAndExpedition();
            /* if (allegiance != null)
             {*/
            memberOf.AllegianceKey = The.Sim.PlaySite.PlayerAllegiance.KeyName; // gets set on startup...  //allegiance.KeyName;
            // }
            if (expedition != null)
            {
                memberOf.ExpeditionKey = expedition.KeyName;
            }



            string casteKey = null;
            if (sex.HasValue)
            {
                EntityType entityType = GameData.Instance.AllEntityTypes[entityKey];
                CasteType casteType = entityType.BiologicalType.Castes.FirstOrDefault(c => c.Reproduction == sex.Value);
                if (casteType != null)
                {
                    casteKey = casteType.KeyName;
                }
            }

            Vector3? location = null;
            if (pos != null)
            {
                location = MapManager.TileToWorldPos(pos.Value);
            }

            EntityData entityData = new EntityData()
            {
                EntityKey = entityKey,
                Location = location,
                BioEntity = new Maps.MapEditor.BiologicalEntity()
                {
                    AgeInYears = new NormalDistribution() { Mean = age },
                    RaceKey = raceKey,
                    CasteKey = casteKey
                },
                Person = new SimSide.Maps.MapEditor.Person()
                {
                    FirstName = firstName,
                    LastName = lastName
                },
                MemberOf = memberOf
            };

            bool placeFailed;
            Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placeFailed, container,
                                                memberOfAllegianceKey: memberOf.AllegianceKey,
                                                memberOfExpeditionKey: memberOf.ExpeditionKey);
            if (entity != null)
            {
                TestEntityNotPlacedOnblockedTerrain(entity);

                AddRandomSkills(entity, noSkills);

            }


            //The.Sim.PlaySite.GetFirstPlayerExpedition()
            /* OLD


            Entity entity = new Entity(GameData.Instance.AllEntityTypes["entity:human"]);
            PersonEntity person = entity.PersonEntity; //new PersonEntity(entity);
            person.SetName(firstName, lastName);
            entity.OwnedBy = (OwnerID)expedition.ID; // ??


            if (debugGoals != null)
            {
                entity.DebugGoalPlan = new AI.Goals.DebugGoalPlan(debugGoals);
            }

            //  entity.PlaceEntityOnTile(pos.X, pos.Y);
            //entity.Renderable.DebugColor = color;

            entity.BiologicalEntity.CasteType = entity.EntityType.BiologicalType.Castes[sex == Reproduction.Female ? 1 : 0];
            // Thor did this... and it was just for fun...I regret nothing! :D
            if (firstName.Equals("Bob") && lastName.Equals("The Machine"))
            {
                entity.BiologicalEntity.RaceType = entity.EntityType.BiologicalType.RaceTypes[new Random().Next(entity.EntityType.BiologicalType.RaceTypes.Length)];
            }

            // SetRaceOnNewEntity("TestColorRace", entity);

            //person.


            expedition.AddMember(entity);
            entity.Intelligence.CurrentExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();

            // must be set before Initialize:          
            entity.BiologicalEntity.SetAgePreInit(age, null);

            entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
            entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

            //   entity.BiologicalEntity.SetRandomEntityStats();

            //entity.PlaceEntityOnTile(pos.X, pos.Y);
            Vector3? location = null;
            if (pos.HasValue)
            {
                location = MapManager.TileToWorldPos(pos.Value);
            }

            entity.PlaceEntityOnPlaySite(location, container, null, null);
            // entity.Renderable.RenderAsModel.SetRotationAndDir((float)(MathHelper.TwoPi * Globals.Instance.RandomPredictable.NextDouble()));

            TestEntityNotPlacedOnblockedTerrain(entity);
            entity.ComeOnline();
            */

            return entity;

        }

        /*
        /// <summary>
        /// why 2 copies of this method???
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="sex"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        /// <param name="age"></param>
        /// <param name="noSkills"></param>
        /// <param name="race"></param>
        /// <param name="expedition"></param>
        /// <param name="debugGoals"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        private static Entity PlacePerson(string firstName, string lastName, Reproduction? sex, Point pos, Color color, float age, 
            bool noSkills, string race, Expedition expedition, GoalPlanner debugGoals = null, Entity container = null)
        {
            Entity entity = new Entity(GameData.Instance.AllEntityTypes["entity:human"]);
            PersonEntity person = entity.PersonEntity; //new PersonEntity(entity);
            person.SetName(firstName, lastName);


            expedition.AddMember(entity);

            if (debugGoals != null)
            {
                entity.DebugGoalPlan = new AI.Goals.DebugGoalPlan(debugGoals);
            }

            //  entity.PlaceEntityOnTile(pos.X, pos.Y);

            AddRandomSkills(entity, noSkills);

            entity.Renderable.DebugColor = color;

            SetRaceOnNewEntity(race, entity);

            entity.BiologicalEntity.CasteType = entity.EntityType.BiologicalType.Castes[sex == Reproduction.Female ? 1 : 0];


            // entity.BiologicalEntity.SetRandomEntityStats(age);


            entity.Intelligence.CurrentExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();

            // must be set before Initialize:          
            entity.BiologicalEntity.SetAgePreInit(age, null);

            entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
            entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

            //     entity.BiologicalEntity.SetRandomEntityStats();

            //entity.PlaceEntityOnTile(pos.X, pos.Y);
            entity.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos), container, null, null);
            entity.SetRotationAndDir((float)(MathHelper.TwoPi * The.Sim.GameplayRandomGenerator.NextDouble("PlaceGameEntities")));

            TestEntityNotPlacedOnblockedTerrain(entity);

            entity.ComeOnline();

            return entity;
        }*/











        /*private void Prototype()
        {

            // 0 is the top map in the directory... replace with 1 to load the second and so on.
            // 0: Post Settlement
            // 1: Pre Settlement
            if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                return;


            The.MapUI.ZoomToMapPosition(194, 66, MapUI.Centering.Middle);

            Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(194, 68)))
            {
                Name = "Start"
            };
            Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


            Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


            Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
            house1.FlipHorizontally = true;
            house1.Structure.AddBuildingToWorld(new Point(192, 64), Common.Direction.North, expeditionOwner);
            house1.Initialize(Site);
            house1.Structure.ConstructionFinished(true);


            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(193, 65), Common.Direction.North, expeditionOwner);
            campfire.Initialize(Site);
            campfire.Structure.ConstructionFinished(true);

            campfire.Structure.AddonTo = house1;
            house1.Structure.AddOns.Add(campfire);

            // starts cooking immediately:
            StoreItems(4, house1, "item:tomatoes");
            StoreItems(5, house1, "item:rice");
            StoreItems(3, house1, "item:chickenmeat");



            //  Item item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
            //  AddColonyItem(item1, new Point(45, 168), new Vector2(-5f, 5f));
            //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
            //  AddColonyItem(item1, new Point(45, 168), new Vector2(-8f, 15f));
            //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
            //  AddColonyItem(item1, new Point(45, 168), new Vector2(0f, 9f));
               
            //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

            Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(198, 68), Color.White, 52f, false);
            // ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(199, 67), Color.White, 40f, false);
            //   man.PersonEntity.HasEatenToday = true;


            Expedition e = Site.GetMainExpedition();
            e.Households[0].MergeHouseholds(e.Households[1]);

            //  Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
            //  PlaceVehicle(car, new Point(33, 165), expeditionOwner); 

        }*/


        /* private void NGPScreenshots1()
         {

             // 0 is the top map in the directory... replace with 1 to load the second and so on.
             // 0: Post Settlement
             // 1: Pre Settlement
             if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                 return;

             The.MapUI.ZoomToMapPosition(194, 66, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(194, 68)))
             {
                 Name = "Start"
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             // starts harvesting:
             //   expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 9;


             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:campLarge"]);
             house1.FlipHorizontally = true;
             house1.Structure.AddBuildingToWorld(new Point(193, 65), Common.Direction.North, expeditionOwner);
             house1.Initialize(Site);
             house1.Structure.ConstructionFinished(true);

             StoreItems(4, house1, "item:tomatoes");
             StoreItems(5, house1, "item:rice");
             StoreItems(3, house1, "item:chickenMeat");


             //   Item item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //   AddColonyItem(item1, new Point(192, 65), new Vector2(-5f, 5f));
             //   item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //   AddColonyItem(item1, new Point(192, 65), new Vector2(-8f, 15f));
             //   item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //   AddColonyItem(item1, new Point(192, 65), new Vector2(0f, 9f));
             
             //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(198, 68), Color.White, 52f, false);
             man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(199, 67), Color.White, 40f, false);

             man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(196, 67), Color.White, 40f, false);
             man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(196, 68), Color.White, 40f, false);




             Expedition e = Site.GetMainExpedition();
             e.Households[0].MergeHouseholds(e.Households[1]);

             Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityVehicle"]);
             PlaceVehicle(car, new Point(191, 67), expeditionOwner);
             car.RenderAsModel.SetRotationAndDir(2.3f);

         }*/

        public static void TwinklerEatTest()
        {

            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/
            The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            The.MapUI.ZoomToMapPosition(10, 5);

            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            
            Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(15, 8), 20, twinklerAllegiance);

                /*
                 Entity e1 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(10, 5), 20);
                 e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f; //0.2f       
                 e1.BiologicalEntity.AddToStomachContents(-1f);
                 */
                /* Entity man;
                 man = PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(8, 8), Color.White, 40f, false, expedition);
                 man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f       
                 man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f       
                 man.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f       
                 */
                //  AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
                //    AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(10, 7));
                //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(9, 7));

            Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            carcass.Bulk = 1f;
            AddColonyItem(carcass, new Point(9, 7));

            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);




            /*     Entity food = new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]);       
                 AddNoOwnerItem(food, new Point(10, 7));
                 food.Bulk = 0.04f;*/

        }

        public static void DogEatTest()
        {
            Point dogPos = new Point(10, 10);
            Point campPos = new Point(10, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(dogPos.X, dogPos.Y);

            Entity dog = PlaceAnimal("entity:dog", Reproduction.Male, dogPos, 5f, The.Sim.PlaySite.PlayerAllegiance);
            dog.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f               
            //  dog.BiologicalEntity.AddToStomachContents(-1f);
            //   dog.BiologicalEntity.Needs.NeedsList["foodEnergy"].PhysicalNeed.DaysAtZero = 2f; // 1.999f;

            GetBob(dogPos, expedition);

            Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            carcass.Bulk = 3.8f;
            AddColonyItem(carcass, campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelKnife"]), dogPos);

            UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]);

            /*  for (int i = 0; i < 5; i++)
              {
                  Entity vermin = PlaceAnimal("entity:fieldQuadite", Reproduction.Male, new Point(10, i * 4 + 10), 5f, chickenAllegiance);
                  ImmobilizeEntity(vermin);
                  BodyComponent body;
                  vermin.Find(out body);
                  body.Body.GlobalHitpoints = 40;

              }*/



            //   UWGame.SimSide.Allegiances.Allegiance chickenAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:studdedThunderChicken"]);

            //    PlaceAnimal("entity:studdedThunderChicken", Reproduction.Male, campPos, 5f, chickenAllegiance);
            //     PlaceAnimal("entity:bajingan", Reproduction.Male, campPos, 5f, chickenAllegiance);
            /*
                       // these two use same model/anims:
                       PlaceAnimal("entity:whiteThunderChicken", Reproduction.Male, campPos, 5f, chickenAllegiance);
                       PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, campPos, 5f, chickenAllegiance);
                       */

        }


        public static void SkillTest()
        {
            Point campPos = new Point(24, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(15, 10);


            Entity bob = PlacePerson("Ib", "bi", Reproduction.Male, campPos, Color.White, 33, true, expedition, "grey1");

            bob = GetBob(campPos, expedition, skillValue: 1f);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);

        }

        public static void EatTest()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point campPos = new Point(10, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(campPos);

            /*  Entity port = AddFinishedStructure("structure:simplePort", new Point(18, 10), expedition, false);

              AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), port, StorageCompartment.OfferedForTrade);

              PlaceRobot("entity:haulingRobot", campPos, The.Sim.PlaySite.PlayerAllegiance, expedition);
              */

            Entity bob;
            //  GetBob(campPos, expedition);

            bob = GetBob(campPos, expedition, useRandomValues: true);

            Entity alice = GetBob(campPos, expedition, useRandomValues: true);

          /*  bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.01f;
            bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.95f; //0.2f  
            bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.1f;
            */

              bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
              bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.3f; //0.2f  
              bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.1f;
              


            // bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].PhysicalNeed.DaysAtZero = 1.9f;
            /*      bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f    */

            /*bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.6f;
            bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.6f; //0.2f    
            bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.6f; //0.2f    */
            //  bob.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0.5f; //0.2f    

            bob.BiologicalEntity.AddToStomachContents(-1f);
          //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campPos + new Point(3, 3));

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hardtack"]), campPos + new Point(3, 3));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]), campPos);

            Entity food = new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]);
            AddColonyItem(food, campPos + new Point(-3, 0));
            // food.Bulk *= 0.3f;

           /* food = new Entity(GameData.Instance.AllEntityTypes["item:turnipSalami"]);
            AddColonyItem(food, campPos + new Point(-3, 0));
            food.Bulk *= 0.3f;
            alice.Contains.AddToContain(food, StorageCompartment.Haul);
            */

            //  food = new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]);
            //  AddColonyItem(food, campPos + new Point(-3, 0));
            //food.Bulk *= 0.3f;

            /*  food = new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]);
              AddColonyItem(food, campPos + new Point(-3, 0));*/
            //   food.Bulk *= 0.3f;
            /*    bob.PersonEntity.Personality.Principles[RatingTypes.Food] = 1f;

                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalWine"]), campPos);

                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
                 */
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hardtack"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]), campPos);
          */

            /* bob = GetBob(campPos, expedition, useRandomValues: true);
             bob.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0f; //0.2f           
             bob.BiologicalEntity.AddToStomachContents(-1f);
             */
            /*  bob = GetBob(campPos, expedition, useRandomValues: true);
              bob.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0f; //0.2f           
              bob.BiologicalEntity.AddToStomachContents(-1f);

              bob = GetBob(campPos, expedition, useRandomValues: true);
              bob.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0f; //0.2f           
              bob.BiologicalEntity.AddToStomachContents(-1f);

              bob = GetBob(campPos, expedition, useRandomValues: true);
              bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f 
              bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f       
              bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f       
              bob.BiologicalEntity.AddToStomachContents(-1f);


              bob = GetBob(campPos, expedition, useRandomValues: true);
              BodyComponent body;
              bob.Find(out body);
              body.Body.GlobalHitpoints = 30;

         
      */
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);           
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
            /*          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hardtack"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipSalami"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smokedTurnip"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenStew"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hexapineSalad"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:simCoffee"]), campPos);
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:simCoffee"]), campPos);           
            */

            /*    
               // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);

                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), campPos);          
            
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);

                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), campPos);

                AddFinishedStructure("structure:smokeOven", new Point(24, 12), expedition, false);
                */

            /* for (int i = 0; i < 1; i++)
             {
                 Entity bob = GetBob(campPos, expedition);
                 bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f       
                 bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f       
                 bob.BiologicalEntity.AddToStomachContents(-1f);

                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(18, 12));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(18, 12));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(18, 12));
        
             }*/

          //  AddFinishedStructure("structure:campfire", new Point(10, 12), expedition, false, null);
            /*   AddFinishedStructure("structure:campfire", new Point(24, 8), expedition, false, null);
              */
            //  AddFinishedStructure("structure:smokeOven", new Point(24, 8), expedition, false, null);

          //  AddFinishedStructure("structure:improvisedWorkbench", new Point(13, 10), expedition, false, null);


        }

        public static void MidsizeTest()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point campPos = new Point(15, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(15, 10);

            GetBob(new Point(6, 11), expedition);


        }



        /// <summary>
        /// for testing security stats also
        /// </summary>
        public static void ThreatTest()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Point campPos = new Point(15, 10);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            The.MapUI.ZoomToMapPosition(15, 10);


            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos); 
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);*/

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), campPos);
            /*    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), campPos);
          

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), campPos);
   */
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), campPos);

          //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), campPos);
             */
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), campPos);
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
              */

            /*    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:musketoon"]), campPos);
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderShotAmmo"]), campPos);

                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:varmintBomb"]), campPos);
                */
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), campPos);
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campPos);*/

            //  AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(11, 12));

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), campPos);

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGun"]), campPos);
           */
            /*   
                Entity sentry = AddFinishedStructure("structure:shotgunSentry", new Point(10, 12), expedition, false, null);
              */


            //  Entity sentry = AddFinishedStructure("structure:sprayGunSentry", new Point(8, 8), expedition, false, null);
            //  Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
            /*   AddColonyItem(ammo, new Point(8, 8));        
               Container magazine = sentry.Parts[0].Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentrySprayGun").Contains;
               Entity surplusAmmo;
               magazine.AddToContain(ammo, out surplusAmmo);
               */

            // Entity sentry = AddFinishedStructure("structure:sentry", new Point(7, 10), expedition, false, null);           

            /*   Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
                 AddColonyItem(ammo, new Point(10, 12));        
                  Container magazine = sentry.Parts[0].Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentryGun").Contains;
                 Entity surplusAmmo;
                 magazine.AddToContain(ammo, out surplusAmmo);
                 */

            //  AddFinishedStructure("structure:shotgunSentry", new Point(7, 12), expedition, false, null);
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgunAmmo"]), campPos);
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:varmintBomb"]), campPos);

            GetBob(campPos, expedition);
            // GetBob(campPos, expedition);
            /*  GetBob(campPos, expedition);
              GetBob(campPos, expedition);
              GetBob(campPos, expedition);
              GetBob(campPos, expedition);*/

            //  PlaceRobot("entity:guardRobot", campPos, The.Sim.PlaySite.PlayerAllegiance, expedition);

            /*   UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
          
               Entity e11 = PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(10, 10), 18, all1, null);
            */
            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            //  Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]);
            Entity nest = CreateNest("terrain:quaditeNest", "North quadite nest", twinklerAllegiance);

            //  nest.PlaceEntityOnPlaySite(new Vector3(30, 30, 0f), null, Entity.StructureState.Finished, null);
            nest.PlaceEntityOnPlaySite(new Vector3(50, 100, 0f), null, Entity.StructureState.Finished, null);

            /*  nest = new Entity(GameData.Instance.AllEntityTypes["terrain:quaditeNest"]);
              nest.Initialize(The.Sim.PlaySite, twinklerAllegiance);
              nest.Name = "North quadite nest";
              //  nest.PlaceEntityOnPlaySite(new Vector3(30, 30, 0f), null, Entity.StructureState.Finished, null);
              nest.PlaceEntityOnPlaySite(new Vector3(100, 50, 0f), null, Entity.StructureState.Finished, null);
           */
            Allegiances.Allegiance fieldQuaditeAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]);
            Allegiances.Allegiance swarmerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);

            nest = CreateNest("terrain:fieldQuaditeNest", "Field quadite nest", fieldQuaditeAllegiance);
            nest.PlaceEntityOnPlaySite(new Vector3(250, 250, 0f), null, Entity.StructureState.Finished, null);

            //   GetBob(new Point(10, 10), expedition);

            Allegiances.Allegiance bushdragonAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);

            Allegiances.Allegiance whipjawAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);

            //   PlaceAnimal("entity:whipjaw", Reproduction.Male, new Point(5, 10), 20, whipjawAllegiance);

            Allegiances.Allegiance chickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);

            for (int i = 0; i < 5; i++)
            {
                Entity e2 = PlaceAnimal("entity:whiteThunderChicken", Reproduction.Male, new Point(5, 5), 20, chickenAllegiance);
                ImmobilizeEntity(e2);
            }


            for (int x = 0; x < 10; x += 2)
            {
                for (int y = 10; y < 15; y += 2)
                {
                    /* Entity e1 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(x, y), 20, bushdragonAllegiance);
           
                   //  Entity e1 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                     // Entity e1 = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                     //e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f; //0.2f       
                     //e1.BiologicalEntity.AddToStomachContents(-1f);
                     ImmobilizeEntity(e1);
                     */

                    //Entity e1 = PlaceAnimal("entity:fieldQuadite", Reproduction.Male, new Point(x + 1, y), 20, fieldQuaditeAllegiance);
                    Entity e1 = PlaceAnimal("entity:swarmer", Reproduction.Male, new Point(x + 1, y), 20, swarmerAllegiance);

                    // ImmobilizeEntity(e1);
                }
            }


            //Body body;
            /* Entities.Body.BodyComponent body;
             e1.Find(out body);
             body.Body.ChangeMaxHitpoints(1500.0f);
            */

            /* GetBob(new Point(6, 11), expedition);
             GetBob(new Point(6, 11), expedition);
             */
            /*
            e1 = PlaceAnimal("entity:fieldQuadite", Reproduction.Male, new Point(10, 10), 20, twinklerAllegiance);
            ImmobilizeEntity(e1);
            e1 = PlaceAnimal("entity:bajingan", Reproduction.Male, new Point(12, 10), 20, twinklerAllegiance);
            ImmobilizeEntity(e1);
            e1 = PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(12, 12), 20, twinklerAllegiance);
            ImmobilizeEntity(e1);
            e1 = PlaceAnimal("entity:fieldQuadite", Reproduction.Male, new Point(8, 12), 20, twinklerAllegiance);
            ImmobilizeEntity(e1);
            */
            // AddFinishedStructure("structure:sensor", new Point(4, 4), expedition, false);

            // PlaceAnimal("entity:dog", Reproduction.Male, campPos, 5f, The.Sim.PlaySite.PlayerAllegiance);

            //  PlaceRobot("entity:haulingRobot", campPos, The.Sim.PlaySite.PlayerAllegiance, expedition);

            /*    for (int x = 10; x < 20; x+=2)
                {
                    for (int y = 10; y < 20; y += 2)
                    {
                        e1 = GetBob(new Point(x, y), expedition);
                    }
                }*/


            /*   e1 = GetBob(new Point(13, 9), expedition);
              e1 = GetBob(new Point(13, 9), expedition);*/
            //   e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;  //0.2f
            //  e1.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.5f;
            //   GetBob(campPos, expedition);
            //  GetBob(campPos, expedition);
            //   GetBob(campPos, expedition);

            //  Entity tent = AddFinishedStructure("structure:octagonalTent", new Point(12, 9), expedition, false);

            //  AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), tent);
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(13, 9));

            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);*/
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), campPos);

        }

        private static Entity CreateNest(string entityType, string name, Allegiances.Allegiance twinklerAllegiance)
        {
            Entity nest = new Entity(GameData.Instance.AllEntityTypes[entityType]); // "terrain:quaditeNest"]);
            nest.Initialize(The.Sim.PlaySite, twinklerAllegiance);
            nest.Name = name; // "North quadite nest";
            return nest;
        }

        public static void AllStructuresBeingBuilt()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 7)));

            The.MapUI.ZoomToMapPosition(10, 5);


            /*   for (int i = 0; i < 2; i++)
               {
                   Entity man;
                   man = PlacePerson("Sebastian", "Zyp " +i+11, Reproduction.Male, new Point(5, 8), Color.White, 40f, false, expedition);
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
                   man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
               }*/




            //  AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
            //    AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(10, 7));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:"]), new Point(9, 7));
            /*    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
                AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(9, 7));*/
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(9, 7));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
              carcass.Bulk = 1f;
              AddColonyItem(carcass, new Point(9, 7));*/

            Entity structure;

            int x = 2; int y = 2;
            foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllStructureTypes)
            {
                EntityType type = kvp.Value;
                if (type == null)
                    continue;
                if (type.KeyName.EndsWith("sentry"))
                    continue;
                if (type.KeyName.Contains("sensor"))
                    continue;
                if (type.StructureType.IsAddon)
                    continue;
                if (type.StructureType.IsRoad)
                    continue;
                //if (type.ContainerType != null && type.ContainerType.HomeContainerType != null)
                //    continue;


                structure = new Entity(type);
                structure.PlaceEntityOnPlaySite(MapManager.TilePosToWorldPos(new TilePos(x, y)), null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(expedition)); //The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.AllKnownEntities as IOwner);

                structure.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);

                // USEFUL BUG: if no agents are added to the scene, then in God Mode the finished and the under construction billboards will be rendered at the same time. 
                // This makes it easy to see if they are aligned.
                structure.Structure.State = Buildings.StructureStates.UnderConstruction;



                x += type.StructureType.WidthInTiles + 1;
                if (!The.Map.TileIsOnMap(new Point(x + 2, y)) || x > 30)
                {
                    x = 1;
                    y += 4;
                }



            }

            //  AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);

            /*     Entity food = new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]);       
                 AddNoOwnerItem(food, new Point(10, 7));
                 food.Bulk = 0.04f;*/

        }

        /// <summary>
        /// //////
        public static void BuildStructuresTest()
        {

            /*   if (!The.Sim.LoadMap("d Mezzomap MLo"))
                   return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 7)));

            The.MapUI.ZoomToMapPosition(10, 5);


            int numberOfAgents = 10; //change number of agents here

            for (int i = 0; i < numberOfAgents; i++)
            {

                Entity man;
                man = PlacePerson("Agent", "NR: " + i, Reproduction.Male, new Point(5, 8 + i), Color.White, 40f, false, expedition);
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
                man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            }

            //  AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
            //    AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(10, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(9, 7));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
              carcass.Bulk = 1f;
              AddColonyItem(carcass, new Point(9, 7));*/

            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);

            /*     Entity food = new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]);       
                 AddNoOwnerItem(food, new Point(10, 7));
                 food.Bulk = 0.04f;*/

        }
        /// </summary>




        public static void ChickenEatTest()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            The.MapUI.ZoomToMapPosition(10, 5);

            Allegiance a = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);

            Entity e1 = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(10, 5), 20, a);
            e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f; //0.2f       
            //e1.BiologicalEntity.AddToStomachContents(-1f);

            Entity man;
            man = PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(5, 8), Color.White, 40f, false, expedition);

            //  AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
            //    AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(10, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(9, 7));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
              carcass.Bulk = 1f;
              AddColonyItem(carcass, new Point(9, 7));*/

            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);

            /*     Entity food = new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]);       
                 AddNoOwnerItem(food, new Point(10, 7));
                 food.Bulk = 0.04f;*/

        }

        public static void RatEatTest()
        {

            /*   if (!The.Sim.LoadMap("d Mezzomap MLo"))
                   return;*/

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            The.MapUI.ZoomToMapPosition(10, 5);

            UWGame.SimSide.Allegiances.Allegiance ratAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
            Expedition ratExpedition = new Expedition(ratAllegiance, "Rat", "Rat", MapManager.TileToWorldPos(new Point(22, 5)));

            for (int i = 0; i < 1; i++)
            {
                Entity e1 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(10, 5), 8, ratAllegiance, ownerExpedition: ratExpedition);
                //  e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f; //0.2f       
                //e1.BiologicalEntity.AddToStomachContents(-1f);
            }
            //  Entity man;
            //  man = PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(2, 10), Color.White, 40f, false, expedition);

            AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
            //    AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(10, 7));
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]), new Point(9, 7));

            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
              carcass.Bulk = 1f;
              AddColonyItem(carcass, new Point(9, 7));
              */

            AddFinishedStructure("structure:sensor", new Point(8, 7), expedition, false);

            /*     Entity food = new Entity(GameData.Instance.AllEntityTypes["item:alabasterStew"]);       
                 AddNoOwnerItem(food, new Point(10, 7));
                 food.Bulk = 0.04f;*/

        }

        public static void NeedsTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            The.MapUI.ZoomToMapPosition(5, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));

            The.Sim.DateAndTime.TimeOfDay = 0.7;

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(5, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(5, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), new Point(5, 7));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(5, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(5, 7));
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(5, 3));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(5, 3));
          */

            /* The.Map.GetTile(new Point(0, 0)).AddResource("stones", 4);

             The.Map.GetTile(new Point(0, 0)).AddResource("treeScuttler", 4);

           
            
             The.Map.GetTile(new Point(3, 5)).AddResource("firewood", 4);

             The.Map.GetTile(new Point(3, 5)).AddResource("clamwich", 4);
             The.Map.GetTile(new Point(7, 5)).AddResource("clamwich", 4);
             The.Map.GetTile(new Point(12, 5)).AddResource("clamwich", 4);
             */


            /* new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }                                ,
                                new Input() { Entity = "item:firegrassSod", Amount = new Input*/

            /*   Entity bees = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:bees"]);
               bees.Initialize(The.Sim.PlaySite);
               bees.PlaceNewEntityInWorld(new Vector3(100f, 100f, 0f), null, null, null);
               */
            //  AddFinishedStructure("structure:skimmerTail", new Point(2, 2), expedition, false);

            /*  Entity bees = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:shoreWaves"]);
              bees.Initialize(The.Sim.PlaySite);
              bees.PlaceNewEntityInWorld(new Vector3(100f, 100f, 0f), null, null, null);
             */

            /*
            Entity house2b = new Entity(GameData.Instance.AllStructureTypes["structure:house2b"]);  
            house2b.FlipHorizontally = true;
            house2b.Structure.AddBuildingToWorld(new Point(10, 3), Common.Direction.North, expeditionOwner);
            house2b.Initialize(The.Sim.Site);
            house2b.Structure.ConstructionFinished(true);
            */

            //  AddFinishedStructure("structure:helipad", new Point(5, 7), expedition, false);

            //  AddFinishedStructure("structure:sensor", new Point(5, 7), expedition, false);

            /*
            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(10, 5), Common.Direction.North, expeditionOwner);
            campfire.Initialize(The.Sim.Site);
            campfire.Structure.ConstructionFinished(true);
            */


            /*  campfire.Structure.AddonTo = house2b;
              house2b.Structure.AddOns.Add(campfire);
              */



            /*
            Entity house2 = new Entity(GameData.Instance.AllStructureTypes["structure:house2"]);
            //not flipped horizontally
            house2.Structure.AddBuildingToWorld(new Point(8, 12), Common.Direction.North, expeditionOwner);
            house2.Initialize(The.Sim.Site);
            house2.Structure.ConstructionFinished(true);

            house2 = new Entity(GameData.Instance.AllStructureTypes["structure:house2"]);
            house2.FlipHorizontally = true;
            house2.Structure.AddBuildingToWorld(new Point(16, 11), Common.Direction.North, expeditionOwner);
            house2.Initialize(The.Sim.Site);
            house2.Structure.ConstructionFinished(true);

            house2 = new Entity(GameData.Instance.AllStructureTypes["structure:house2"]);
            house2.FlipHorizontally = true;
            house2.Structure.AddBuildingToWorld(new Point(17, 4), Common.Direction.North, expeditionOwner);
            house2.Initialize(The.Sim.Site);
            house2.Structure.ConstructionFinished(true);
            */


            // Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
            //  carcass.Bulk = 10f;
            //  AddColonyItem(carcass, new Point(10, 5));
            //   


            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(5, 7));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(5, 7));

               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(5, 7));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(5, 7));

               */
            /* Entity item = new Entity(GameData.Instance.AllEntityTypes["item:sensor"]);
             AddColonyItem(item, new Point(5, 6));*/

            //item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            //AddColonyItem(item, new Point(11, 5));

            //    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(2, 2), 20, chickenAllegiance);

            //    Body body;
            //    chicken.Find(out body);
            //    body.ChangeMaxHitpoints(1.0f);;

            //    chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(2, 4), 20, chickenAllegiance);
            //    chicken.Find(out body);
            //    body.ChangeMaxHitpoints(1.0f);;

            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(2, 5));
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(2, 6));

            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(5, 5));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(5, 5));*/

            //  PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(18, 8), 20);

            //  Entity entity = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(14, 15), 20);
            //  entity.Renderable./*TODO DECOUPLE*/RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
            //

            //  twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(16, 10), 20, twinklerAllegiance);

            //   Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            //   AddColonyItem(item, new Point(14, 7));

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(6, 3)); //187, 80



            //  The.Sim.DateAndTime.TimeOfDay = 1; // night

            int i = 0;

            //    man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(20, 15), Color.White, 40f, false);
            //    man.PersonEntity.HasEatenToday = true;
            //     man.Intelligence.Morale = 0.5f;
            i++;
            Entity man;

            man = PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(4, 3), Color.White, 40f, false, expedition);

            man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f; // 0f;
            man.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.5f;
            man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = .0f;
            man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = .0f;
            man.BiologicalEntity.Needs.NeedsList["foodEnergy"].PhysicalNeed.DaysAtZero = 0f; // 1.999f;
            man.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0f;


            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.8f, GameData.Instance.AllSkillTypes["menial"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.8f, GameData.Instance.AllSkillTypes["bushcraft"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));


            //    man = PlacePerson("Sebastian", "Zyp 2", Reproduction.Male, new Point(4, 3), Color.White, 40f, false, expedition);


            //   man = PlacePerson("Sebastian", "Zyp 2", Reproduction.Male, new Point(6, 3), Color.White, 40f, false, expedition);

            //  man.BiologicalEntity. = 0f;

            //Entity food = new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]);
            // food.Bulk = 0.03f;

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:simCoffee"]), new Point(5, 7));

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(5, 7));


            /*  Entity food = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);
              food.Bulk = 1f;
              AddColonyItem(food, new Point(5, 7));*/

            /*   Entity item;
               item = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]);

               AddColonyItem(item, new Point(20, 20));
               man.Intelligence.Allegiance.SharedKnowledge.SeeDetectable(item, true, false, null, man);
               man.Contains.AddToContain(item);*/


            /*    man = PlacePerson("Sebastian", "Zyp 2", Reproduction.Male, new Point(7, 8), Color.White, 40f, false, expedition);
                man.PersonEntity.HasEatenToday = true;
                //  man.PersonEntity.Household.Home = house2b;
                man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
                man.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.5f;


                man = PlacePerson("Gustav", "Nielsen", Reproduction.Male, new Point(7, 9), Color.White, 40f, false, expedition);
              */

            /*  int numMen = 27;
              for (int t = 4; t < 4+numMen; t++)
              {
                  string name = "Sebastian Zyp ";
                  Sim.PersonSex sex = Reproduction.Male;
                  if (t % 2 == 1)
                  {
                      name = "Eloise Dijkstra ";
                      sex = Reproduction.Female;
                  }
           


                  man = PlacePerson(name + (i++), sex, new Point(t, 6 + (t % 4)), Color.White, 40f, false, expedition);
                  man.PersonEntity.HasEatenToday = true;
                  //MLo      man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
                  man.PersonEntity.Household.Home = house2b;
                     man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;

                  //  man.Intelligence.Skills[GameData.Instance.AllSkillTypes["butchering"]].Value = 0.5f;

                  //MLo       man.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;
                 // man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
                  //MLo      man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.2f;
                  //MLo       man.BiologicalEntity.StomachContents = 1f;
                  //MLo forcing a hungry Karol Nikolaev


                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(t, 8 + (t%3)));//lots of food


              }*/

        }



        public static void GeometryTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;*/

            //The.Recorder.SaveMap((int)Scenarios.GeometryTest);

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;

            Entity structure;

            int x = 1;
            int y = 2;
            int maxMen = 180;
            int manCount = 0;
            foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllStructureTypes)
            {
                EntityType type = kvp.Value;
                if (type == null)
                    continue;
                if (type.KeyName.EndsWith("sentry"))
                    continue;
                if (type.StructureType.IsAddon)
                    continue;

                for (bool flipped = false; ; flipped = true)
                {

                    /*  structure = new Entity(type);
                      structure.FlipHorizontally = flipped;
                      structure.Structure.AddBuildingToWorld(new Point(x, y), Common.Direction.North, expeditionOwner);
                      structure.Initialize(The.Sim.Site);
                      structure.Structure.ConstructionFinished(true);
                      */

                    /*  for (int q = 0; q < 9; ++q)
                      {
                          if (maxMen-- < 1)
                          {
                              maxMen = 0;
                              break;
                          }
                          Entity man = PlacePerson("Gabor", "Czupo" + x + y + q, Reproduction.Male,
                              new Point(x , y + 3), Color.Orange, 23f, false, expedition
                              , 
                              //enterHome
                              //+
                              randomScouting 
                              //+   
                              //doSomething
                              );
                          man.PersonEntity.HasEatenToday = true;
                    //      man.PersonEntity.Household.Home = structure;
                          manCount++;

                      }*/

                    x += type.StructureType.WidthInTiles + 1;
                    if (!The.Map.TileIsOnMap(new Point(x + 12, y)) || x > 30)
                    {
                        x = 1;
                        y += 4;
                    }

                    if (flipped)
                        break;
                }


            }

            Entity rock = new Entity(GameData.Instance.AllEntityTypes["terrain:ovalrocks4"]);
            AddTerrainItem(rock, new Point(15, 4));





        }

        /*  static GoalPlanner enterHome = EnterHome;
          static void EnterHome(Entity entity, DebugJobEvaluator eval)
          {
              Intelligence intel = entity.Intelligence;

              Entity home = entity.PersonEntity.Household.Home;
              Debug.Assert(home != null);

              intel.SetTopLevelGoal(new GoalEnter(entity, home.EntityID)
              {
                  GoalEvaluator = eval
              }, double.MaxValue);
            

          }*/

        public static void CampfireTest()
        {
            /*  if (!The.Sim.LoadMap("4 Micromap"))
                  return;  // micro map*/

            Point campPos = new Point(10, 5);

            The.MapUI.ZoomToMapPosition(campPos);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, false, expedition);

            PlacePerson("Kasdfsdfrol", "rtyrtyNikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, false, expedition);

            /*   Entity sentry = AddFinishedStructure("structure:sentry", new Point(10, 12), expedition, false, null);
               Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
               AddColonyItem(ammo, new Point(10, 12));
               */

            //  Entity firewood = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            //   firewood.Initialize(The.Sim.PlaySite);
            // firewood.InitializeModelAndOnScreenFunctionality();

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(6, 8));

            /*  Entity firewood = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              firewood.Initialize(The.Sim.PlaySite);
              firewood.InitializeModelAndOnScreenFunctionality();
              IOwner ColonyOwner = The.Sim.PlaySite.GetFirstPlayerExpedition();
              */

            Entity fire = AddFinishedStructure("structure:campfire", new Point(16, 7), expedition, false);
            /*  ReplenishContainer fireAsTool = fire.Contains as ReplenishContainer;
              fireAsTool.AddToContain(firewood, replenish: true);
              firewood.ChangeOwnership(ColonyOwner);*/

            //  fire = AddFinishedStructure("structure:campfire", new Point(18, 7), expedition, false);

            // The.Map.GetTile(new Point(6, 8)).AddResource("blackpulp", 8);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(16, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
        }

        public static void PeatTest()
        {
            /*  if (!The.Sim.LoadMap("4 Micromap"))
                  return;  // micro map*/

            The.MapUI.ZoomToMapPosition(5, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));

            GetBob(new Point(5, 5), expedition);

            /*   for (int i = 0; i < 10; i++)
               {
                   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetPeat"]), new Point(5, 5));
          
               }*/

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelSpade"]), new Point(7, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedTrowel"]), new Point(7, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), new Point(7, 6));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(7, 6));

            AddTree("tree:spoak", new Point(7, 4), "crop:spoakBranches", 4);

            /*  Entity bugnet = AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(9, 6));
              bugnet.DoDamage(0.98f);
              Entity rifle = AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(9, 6));
              rifle.DoDamage(0.98f);*/

            // AddFinishedStructure("structure:peatStack", new Point(7, 5), expedition, false);*/
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:clayDeposit"]), MapManager.TileToWorldPos(new Point(6, 8)));

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:peatDeposit"]), MapManager.TileToWorldPos(new Point(9, 8)));

        }


        public static void AnimTest()
        {
            /*  if (!The.Sim.LoadMap("4 Micromap"))
                  return;  // micro map*/

            The.MapUI.ZoomToMapPosition(15, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            Entity e1 = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, false, expedition);

        }

        public static void ProductionTest()
        {
            /*  if (!The.Sim.LoadMap("4 Micromap"))
                  return;  // micro map*/

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            Entity e1 = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, false, expedition);
            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["smithing"], new Skill(0.1f, GameData.Instance.AllSkillTypes["smithing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["mechanics"], new Skill(0.1f, GameData.Instance.AllSkillTypes["mechanics"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["electronics"], new Skill(0.1f, GameData.Instance.AllSkillTypes["electronics"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["chemistry"], new Skill(0.1f, GameData.Instance.AllSkillTypes["chemistry"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["farming"], new Skill(0.2f, GameData.Instance.AllSkillTypes["farming"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["weeding"], new Skill(0.2f, GameData.Instance.AllSkillTypes["weeding"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["grasping"], new Skill(0.2f, GameData.Instance.AllSkillTypes["grasping"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fruitPicking"], new Skill(0.2f, GameData.Instance.AllSkillTypes["fruitPicking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["construction"], new Skill(0.2f, GameData.Instance.AllSkillTypes["construction"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["archery"], new Skill(0.2f, GameData.Instance.AllSkillTypes["archery"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["sneaking"], new Skill(0.2f, GameData.Instance.AllSkillTypes["sneaking"]));

            /*
            e1 = PlacePerson("Karol2", "Nikolaev", Reproduction.Male, new Point(15, 5), Color.White, 40f, false, expedition);
            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
            */
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:testToolbox"]), new Point(17, 8));

            /*
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(17, 8));
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), new Point(17, 8));
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(17, 8));

          
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes[ "item:thermalTarp"]), new Point(17, 8));
             */


            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;



            AddFinishedStructure("structure:campfire", new Point(18, 5), expedition, false);
            AddFinishedStructure("structure:simpleSmithy", new Point(22, 5), expedition, false);

            AddFinishedStructure("structure:goldFurnace", new Point(13, 5), expedition, false);

            Entity item;

            item = new Entity(GameData.Instance.AllEntityTypes["item:sensor"]);
            AddColonyItem(item, new Point(16, 5));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 5));
            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 5));
            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 5));
            item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
            AddColonyItem(item, new Point(16, 5));

            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]);
            AddColonyItem(item, new Point(16, 5));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 7));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWorkersToolbox"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gold"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gold"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunBarrelUnbored"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sandMold"]), new Point(15, 7));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:goldOre"]), new Point(15, 7));

            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(16, 5));




            /*  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
              carcass.Bulk = 10f;
              AddColonyItem(carcass, new Point(12, 8));
              //item:streakFin
  /*
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(15, 7));
          
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:streakFin"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(15, 7));  //shortest way of placing stuuf
           //  
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(15, 7));
          //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 7));         
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 7));
         

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(15, 7));

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(15, 7));
           //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 7));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(15, 7));
           //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRodNeonHornet"]), new Point(15, 7));

              //

                  item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                  item.Bulk = 1f;  // see comment
                  AddColonyItem(item, new Point(39, 28));
    

                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(17, 8));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(17, 8));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(17, 8));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(17, 8));
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(17, 8));
            

              //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(57, 39));

                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(15, 8));

              // for tools test
            
          
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(17, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothBasaltstone"]), new Point(17, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalFile"]), new Point(17, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(17, 8));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalCutter"]), new Point(17, 8));
            
              */
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 7));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 7));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(17, 8));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(17, 8));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(17, 8));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(17, 8));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 7));  //shortest way of placing stuuf
       


              /* item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
               AddColonyItem(item, new Point(17, 8));

               item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
               AddColonyItem(item, new Point(14, 7));
               */
            /*   for (int i = 0; i < 5; i++)
               {
                   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(15, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                   AddColonyItem(item, new Point(16, 7));
               }
          
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
               AddColonyItem(item, new Point(16, 7));
           
               item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
               AddColonyItem(item, new Point(14, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
               AddColonyItem(item, new Point(14, 8));
               item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
               AddColonyItem(item, new Point(15, 8));
               item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
               AddColonyItem(item, new Point(16, 7));*/
        }

        public static void GaitTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            The.MapUI.ZoomToMapPosition(15, 5);

            //  The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(2, 2), Color.White, 40f, false, expedition);
            man.SetRotationAndDir(MathHelper.PiOver2);
            man.Renderable.RenderAsModel.FinalModelScale = 3f;
        }

        public static void UnloadTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            The.MapUI.ZoomToMapPosition(11, 5);

            //  The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(11, 8)));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(10, 6), Color.White, 40f, false, expedition);
            man.SetRotationAndDir(MathHelper.PiOver2);


            Entity hull = AddFinishedStructure("structure:skimmerHull", new Point(8, 6), expedition, false);
            Entity part = hull.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            part.DoDamage(1f); //, false);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(15, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(15, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));

            /*
            Entity item;
            item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]);  /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);   /////////place items inside skimmerHull
            AddColonyItemToStorage(item, hull);
           */
        }

        public static void MacheteTest()
        {
            /* if (!The.Sim.LoadMap("4 Micromap")) //"c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                 return;  // micro map*/

            The.MapUI.ZoomToMapPosition(15, 5);

            //  The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(2, 2)));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(2, 2), Color.White, 40f, false, expedition);
            man.SetRotationAndDir(MathHelper.PiOver2);

            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]); //"item:advancedMachete"]);
            AddColonyItem(item, new Point(2, 2));
        }


        public static void StorageTest()
        {
            /* if (!The.Sim.LoadMap("c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                 return;  // micro map*/
            //  The.Sim.ExploreShroud();

            The.MapUI.ZoomToMapPosition(15, 12);

            //  The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(13, 4)));

            Entity shelter = AddFinishedStructure("structure:A-frameTarp", new Point(13, 4), expedition, false);

            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(4, 2), Color.White, 40f, false, expedition);

            // Entity bob = GetBob(new Point(13, 2), expedition);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(4, 4));

            //   Entity shelter = AddFinishedStructure("structure:A-frameScraps", new Point(14, 7), expedition, false);
            //  AddColonyItemToStorage(item, shelter);


            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(15, 4));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(15, 4));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(15, 4));
               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), new Point(15, 4));

               AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:smoothSandstone"]), shelter);*/
            for (int i = 0; i < 80; i++)
            {
                AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), shelter);
            }
            //   AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), shelter);
            //  AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), shelter);

            /*   Entity storage = AddFinishedStructure("structure:cooledFoodCache", new Point(14, 8), expedition, false);
               AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), storage);
               */

            //   The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
            /*  The.Sim.PlaySite.EventManager.AddPolledEvent("farmingCounter");
              The.Sim.PlaySite.EventManager.AddPolledEvent("plotSpawningLoop");*/

            /*  Dictionary<string, int> items = new Dictionary<string, int>();
              items.Add("farmingHoe", 2);
              items.Add("glassyCreeperPods", 12);
              items.Add("crystalBerries", 12);
              items.Add("machete", 2);

              foreach (KeyValuePair<string, int> entry in items)
              {
                  for (int i = 0; i < entry.Value; i++)
                  {
                      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + entry.Key]), new Point(15, 4));
                  }
              }*/

            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(10, 7)));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 5)) + new Vector3(0f, 24f, 0f));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)) + new Vector3(0f, 24f, 0f));
            //AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(5, 9)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
            AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 9)) + new Vector3(0f, 24f, 0f));



            /* Entity e1 = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(12, 10), Color.White, 40f, false, expedition);
             e1.SetRotationAndDir(MathHelper.PiOver2);
             e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
             */


            // e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
            //  e1.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.5f;


            //    Entity man = PlacePerson("Nikola", Reproduction.Male, new Point(17, 9), Color.White, 40f, false, expedition);
            //    man.SetRotationAndDir(MathHelper.PiOver2);

            //PlacePerson("Inez Rafael", Reproduction.Female, new Point(3, 2), Color.White, 40f, false, expedition);

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:smokedTurnip"]].ProductionTarget = 1;

            //    AddFinishedStructure("structure:skimmerTail", new Point(15, 11), expedition, false); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP

            // AddFinishedStructure("structure:abatis", new Point(0, 0), expeditionOwner, false); // expedition, false);   ..on other side of river

            //   AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 8)) + new Vector3(-16f, -20f, 0f));

            /*
            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(14, 4), Common.Direction.North, expedition.ExpeditionOwner);
            campfire.Initialize(The.Sim.Site);
            campfire.Structure.ConstructionFinished(true);

            The.Client.ParticleManager.AddEmitter("smallFire", MapManager.TileToWorldPosVector2(new Point(14, 7)));
            */
            //   
            //  The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(14, 7)));
            //   The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(15, 7)), 0.5f, 1f);

            //  The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(16, 7)), 0.5f, null);

            //   The.Client.ParticleManager.AddFlamePlume(MapManager.TileToWorldPosVector2(new Point(18, 7)));

            // Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            //    Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(14, 12), 20, twinklerAllegiance);  //twinklerAllegiance


            /* Entity hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
              hole.FlipHorizontally = true;
              hole.Structure.AddBuildingToWorld(new Point(10, 7), Common.Direction.North, expedition.ExpeditionOwner);
              hole.Initialize(The.Sim.Site);
              hole.Structure.ConstructionFinished(true);
              */

            //   Allegiance.Allegiance allegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

            //    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 4), 20, allegiance);


            //    hole.Parts[1].DoDamage(0.999f);
            //   hole.Parts[0].DoDamage(0.999f);

            /*   AddFinishedStructure("structure:A-frameTarp", new Point(8, 4), expedition, false);
          
             //  AddFinishedStructure("structure:lean-toTarp", new Point(8, 4), expedition, false);

               //AddFinishedStructure("structure:abatis", new Point(8, 4), expedition, false);
               */

            //   AddFinishedStructure("structure:smokeOven", new Point(12, 3), expedition, false);

            //   Entity hole = AddFinishedStructure("structure:cooledFoodCache", new Point(12, 10), expedition, false);
            //   Entity part = hole.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:spoakLeaves"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
            //   part.DoDamage(0.98f);

            Entity item;
            /*  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
            item.Bulk = 0.5f;
            AddColonyItem(item, new Point(12, 4));
           */
            /*    item = EntityFactory.Produce(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
                item.Bulk = 0.5f;
                AddColonyItem(item, new Point(12, 4));*/

            /*  AddColonyItem(EntityFactory.Produce(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 4));
              AddColonyItem(EntityFactory.Produce(GameData.Instance.AllEntityTypes["item:thunderChickenSkewers"]), new Point(12, 4));
             */


            /*  item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(12, 4));
              item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(12, 4));*/

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(15, 4));
        
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(15, 4));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(15, 4));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(15, 4));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(15, 4));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 4));

              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 4));
              */

            /*
              "item:sticks", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = true }, //AF 30/5-14 noOfItems change from 2
                                     new Input() { Entity = "item:wingweedLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:thermalTarp", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },*/

            /*  for (int i = 0; i < 3; i+=2)
              {
                  item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                  AddColonyItem(item, new Point(8 + i, 8));
                  item.Bulk = 0.5f;

                  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                  AddColonyItem(item, new Point(8 + i, 10));
                  item.Bulk = 0.2f;
              }
          */
            /*
           for (int i = 0; i < 5; i ++)
           {
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
               AddColonyItem(item, new Point(15, 12));
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]);
               AddColonyItem(item, new Point(15, 12));
               item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
               AddColonyItem(item, new Point(15, 12));
           }*/

            /* item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
             AddColonyItem(item, new Point(15, 2));*/

            /* item = new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]);
              AddColonyItem(item, new Point(15, 2));
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(15, 2));*/
            /*    item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
                item.Bulk = 0.5f;
                AddColonyItem(item, new Point(15, 2));
                item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);
                item.Bulk = 0.5f;
                AddColonyItem(item, new Point(15, 2)); 
               */
            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:alabasterRay"]);
               AddColonyItem(item, new Point(15, 2));*/


            //AddColonyItemToStorage(item, hole);

            /*   hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
               hole.FlipHorizontally = true;
               hole.Structure.AddBuildingToWorld(new Point(13, 5), Common.Direction.North, expedition.ExpeditionOwner);
               hole.Initialize(The.Sim.Site);
               hole.Structure.ConstructionFinished(true);
               */

            //  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            //   AddColonyItem(item, new Point(20, 8));


            //  AddColonyItem(item, new Point(15, 18));

            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
               AddColonyItemToStorage(item, hole);
               //AddColonyItem(item, new Point(15, 7));
               */
            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:rottenMeat"]);
               item.Bulk = 0.5f;
               AddColonyItem(item, new Point(16, 7));*/
            /*
             for (int i = 0; i < 5; i++)
               {
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                //   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(15, 7));
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                //   AddColonyItem(item, new Point(16, 7));
               }*/

            /*  item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
              AddColonyItem(item, new Point(16, 7));
           
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(14, 7));
              item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(14, 8));
              item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(15, 8));
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(16, 7));
              item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
              AddColonyItem(item, new Point(16, 7));*/
        }

        public static void HarvestPickupTest()//Made for testing how an agent with a full inventory handless trying to pick up a tool for a harvest job
        {

            /*  if (!The.Sim.LoadMap("e Map Quartersize Oct 2013")) //"c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                  return;*/

            The.MapUI.ZoomToMapPosition(38, 25);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(38, 25)));


            //Entity man = PlacePerson("Karol Nikolaev", Reproduction.Male, new Point(20, 20), Color.White, 40f, false, expedition);
            Entity man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, true, "grey1", expedition);
            man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));

        }

        public static void HarvestTest2()
        {

            /* if (!The.Sim.LoadMap(The.Map.AllMaps[4])) //"c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                 return;*/

            The.MapUI.ZoomToMapPosition(15, 15);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(20, 22)));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(20, 20), Color.White, 40f, false, expedition);
            /* man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
             man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
             */
            /*
                        Entity item;
                        item = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]);

                        AddColonyItem(item, new Point(20, 20));

                        man.Contains.AddToContain(item);

                        man.AgentStorage.MountedToolOrWeapon = item.EntityID;*/

        }
        public static void BuildingSoundTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;  // micro map
             */
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));
            The.MapUI.ZoomToMapPosition(15, 15);

            Entity Bob = PlacePerson("Builder", "Bob", Reproduction.Male, new Point(13, 15), Color.White, 52f, false, expedition);
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            Bob.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            float itemBulk = 0.2f;
            Entity item;
            for (int index = 0; index < 4; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]);
                item.Bulk = 1f;
                AddColonyItem(item, new Point(15, 15));
            }
            /*
            for (int index = 0; index < 16; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
                        
            for (int index = 0; index < 9; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }

            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakShingles"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            for (int index = 0; index < 12; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]);
                item.Bulk = itemBulk;
                AddColonyItem(item, new Point(15, 15));
            }
            */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:domeTent"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:octagonalTent"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smallTent"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:domeTent"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:octagonalTent"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smallTent"]), new Point(15, 15));
        }

        public static void BuildingTest()
        {
            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;  // micro map
              */
            The.MapUI.ZoomToMapPosition(15, 15);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(1, 1)));


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(15, 15), Color.White, 40f, false, expedition);
            man.SetRotationAndDir(MathHelper.PiOver2);

            man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            man = PlacePerson("Dude", "Dudeson", Reproduction.Male, new Point(18, 15), Color.White, 40f, false, expedition);


            Entity item;
            for (int index = 0; index < 16; index++)
            {
                item = new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]);
                item.Bulk = 0.5f;
                AddColonyItem(item, new Point(15, 12));
            }
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 15));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
        }

        public static void StorageTest2()
        {

            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //"c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                 return;  // micro map
             */
            The.MapUI.ZoomToMapPosition(15, 15);

            //  The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));

            //    The.Sim.Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(15, 15), Color.White, 40f, false, expedition);
            man.SetRotationAndDir(MathHelper.PiOver2);


            //   Entity fogInstance = new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]);
            //   AddTerrainItem(fogInstance, new Point(5, 5));

            //    PlacePerson("Inez Rafael", Reproduction.Female, new Point(3, 2), Color.White, 40f, false, expedition);

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;

            // AddFinishedStructure("structure:abatis", new Point(0, 0), expeditionOwner, false); // expedition, false);   ..on other side of river

            //   AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 8)) + new Vector3(-16f, -20f, 0f));



            //   
            //  The.Client.ParticleManager.AddEmitter("fireSparks", MapManager.TileToWorldPosVector2(new Point(14, 7)));
            //   The.Client.ParticleManager.AddEmitter("smallSmoke", MapManager.TileToWorldPosVector2(new Point(15, 7)), 0.5f, 1f);

            //  The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(16, 7)), 0.5f, null);

            //   The.Client.ParticleManager.AddFlamePlume(MapManager.TileToWorldPosVector2(new Point(18, 7)));

            // Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            //    Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(14, 12), 20, twinklerAllegiance);  //twinklerAllegiance


            /*  Entity hole = new Entity(GameData.Instance.AllStructureTypes["structure:cooledFoodCache"]);
              hole.FlipHorizontally = true;
              hole.Structure.AddBuildingToWorld(new Point(15, 9), Common.Direction.North, expedition.ExpeditionOwner);
              hole.Initialize(The.Sim.Site);
              hole.Structure.ConstructionFinished(true);

              for (int i = 0; i < 16; i++)
              {
                  AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), hole);    
              }
              */

            //   Allegiance.Allegiance allegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

            //    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 4), 20, allegiance);


            //    hole.Parts[1].DoDamage(0.999f);
            //   hole.Parts[0].DoDamage(0.999f);

            /*   AddFinishedStructure("structure:A-frameTarp", new Point(8, 4), expedition, false);
          
             //  AddFinishedStructure("structure:lean-toTarp", new Point(8, 4), expedition, false);

               //AddFinishedStructure("structure:abatis", new Point(8, 4), expedition, false);
               */




            Entity item;

            /*    for (int i = 0; i < 3; i+=2)
                {
                    item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                    AddColonyItem(item, new Point(8 + i, 8));
                    item.Bulk = 0.5f;

                    item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                    AddColonyItem(item, new Point(8 + i, 10));
                    item.Bulk = 0.2f;
                }
            */

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 12));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 12));

            /* item = new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]);
              AddColonyItem(item, new Point(15, 2));
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(15, 2));*/
            /*  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
              item.Bulk = 0.5f;
              AddColonyItem(item, new Point(15, 12));
              item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
              item.Bulk = 0.5f;
              AddColonyItem(item, new Point(15, 12));*/
            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            item.Bulk = 0.5f;
            //   AddColonyItem(item, new Point(15, 12));
            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:alabasterRay"]);
               AddColonyItem(item, new Point(15, 2));*/

            Entity hole = AddFinishedStructure("structure:storageHole", new Point(13, 15), expedition, false);

            AddColonyItemToStorage(item, hole);



            /*   hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
               hole.FlipHorizontally = true;
               hole.Structure.AddBuildingToWorld(new Point(13, 5), Common.Direction.North, expedition.ExpeditionOwner);
               hole.Initialize(The.Sim.Site);
               hole.Structure.ConstructionFinished(true);
               */

            //  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            //   AddColonyItem(item, new Point(20, 8));


            //  AddColonyItem(item, new Point(15, 18));

            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
               AddColonyItemToStorage(item, hole);
               //AddColonyItem(item, new Point(15, 7));
               */

            /*
             for (int i = 0; i < 5; i++)
               {
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                //   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(15, 7));
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                //   AddColonyItem(item, new Point(16, 7));
               }*/
            /*
              item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
              AddColonyItem(item, new Point(16, 7));
           
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(14, 7));
              item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(14, 8));
              item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
              AddColonyItem(item, new Point(15, 8));
              item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
              AddColonyItem(item, new Point(16, 7));
              item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
              AddColonyItem(item, new Point(16, 7));*/
        }

        public static void ShelterTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //"c Micromap Test")) // "4 Micromap")) //The.Map.AllMaps[4]))
                 return;  // micro map
             */
            The.MapUI.ZoomToMapPosition(5, 5);

            // The.Sim.DateAndTime.TimeOfDay = 0;

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(3, 3)));

            //    ExploreShroud(new TilePos(10, 10), new TilePos(30, 30), 4, 9);

            Entity shelter = AddFinishedStructure("structure:lean-toTarp", null, expedition, false, new Vector3(150, 150, 0f)); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP


            bool noSkills = false;  //'false' means no skills, 'true' means skills are defined under the character
            PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(4, 4), Color.White, 52f, noSkills, "blue1",
                expedition, shelter);

         /*   PlacePerson("A", "2", Reproduction.Male, null, Color.White, 52f, noSkills,
               expedition, null, shelter);
            PlacePerson("Ward", "3", Reproduction.Male, null, Color.White, 52f, noSkills,
                  expedition, null, shelter);
            PlacePerson("Ward", "4", Reproduction.Male, null, Color.White, 52f, noSkills,
                  expedition, null, shelter);
            PlacePerson("Ward", "5", Reproduction.Male, null, Color.White, 52f, noSkills,
                  expedition, null, shelter);
            PlacePerson("Ward", "6", Reproduction.Male, null, Color.White, 52f, noSkills,
                    expedition, null, shelter);
            PlacePerson("Ward", "7", Reproduction.Male, null, Color.White, 52f, noSkills,
                  expedition, null, shelter);*/

         //   AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(6, 6)));
         //   AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(8, 9)));
            //    AddFinishedStructure("structure:clayHut", null, expedition, false, MapManager.TileToWorldPos(new Point(4, 6)));


            /*   Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(14, 9), Color.White, 52f, noSkills, "blue2", expedition);
               e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating
               e2.Find(out body);
               body.ChangeMaxHitpoints(150.0f);
               e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
               e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
               e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
               e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;  //0.2f
               e2.BiologicalEntity.StomachContents = 1f;

               e2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
               e2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));

               Entity e3 = PlacePerson("Kurt Mansell", Reproduction.Male, new Point(12, 11), Color.White, 52f, noSkills, "grey1", expedition);
               e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating
               e3.Find(out body);
               body.ChangeMaxHitpoints(60.0f);
               e3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.4f; //0.2f
               e3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.5f; //0.2f
               e3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.6f; //0.2f
               e3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;  //0.2f
               e3.BiologicalEntity.StomachContents = 0.7f;

               e3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
               e3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));


               Entity e4 = PlacePerson("Glen Tarkov", Reproduction.Male, new Point(12, 12), Color.White, 52f, noSkills, "red1", expedition);
               e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating
               e4.Find(out body);
               body.ChangeMaxHitpoints(150.0f);
               e4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f; //0.2f
               e4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f; //0.2f
               e4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f; //0.2f
               e4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;  //0.2f
               e4.BiologicalEntity.StomachContents = 1f;

               e4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
               e4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));

             */

            //    PlacePerson("Inez Rafael", Reproduction.Female, new Point(3, 2), Color.White, 40f, false, expedition);

            // AddFinishedStructure("structure:abatis", new Point(0, 0), expeditionOwner, false); // expedition, false);   ..on other side of river

            //   AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, MapManager.TileToWorldPos(new Point(12, 8)) + new Vector3(-16f, -20f, 0f));

            /*
            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(14, 4), Common.Direction.North, expedition.ExpeditionOwner);
            campfire.Initialize(The.Sim.Site);
            campfire.Structure.ConstructionFinished(true);
            */





            /*   Entity hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
               hole.FlipHorizontally = true;
               hole.Structure.AddBuildingToWorld(new Point(4, 4), Common.Direction.North, expedition.ExpeditionOwner);
               hole.Initialize(The.Sim.Site);
               hole.Structure.ConstructionFinished(true);
            
               Entity part = hole.Parts.Find(p => p.EntityType == GameData.Instance.AllEntityTypes["item:spoakLeaves"]);   //here we make skimmerHull broken (condition=0) by doing damage to one of its parts (scrapMetal)
               part.DoDamage(0.94f);
               */





            Entity item;

            /*    for (int i = 0; i < 3; i+=2)
                {
                    item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                    AddColonyItem(item, new Point(8 + i, 8));
                    item.Bulk = 0.5f;

                    item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                    AddColonyItem(item, new Point(8 + i, 10));
                    item.Bulk = 0.2f;
                }
            */



            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:alabasterRay"]);
               AddColonyItem(item, new Point(15, 2));*/


            //AddColonyItemToStorage(item, hole);

            /*   hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
               hole.FlipHorizontally = true;
               hole.Structure.AddBuildingToWorld(new Point(13, 5), Common.Direction.North, expedition.ExpeditionOwner);
               hole.Initialize(The.Sim.Site);
               hole.Structure.ConstructionFinished(true);
               */

            //  item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            //   AddColonyItem(item, new Point(20, 8));


            //  AddColonyItem(item, new Point(15, 18));

            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
               AddColonyItemToStorage(item, hole);
               //AddColonyItem(item, new Point(15, 7));
               */

            /*
             for (int i = 0; i < 5; i++)
               {
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                //   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(14, 7));
                   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
                   AddColonyItem(item, new Point(15, 7));
                //   item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                //   AddColonyItem(item, new Point(16, 7));
               }*/

            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
               AddColonyItem(item, new Point(16, 7));
            */

            /*    item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                AddColonyItem(item, new Point(14, 7));
              */
            /*    item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:sticks"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]);
                AddColonyItem(item, new Point(1, 3));
                 item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]);
                AddColonyItem(item, new Point(14, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]);
                AddColonyItem(item, new Point(14, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]);
                AddColonyItem(item, new Point(14, 7));*/
            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
               AddColonyItem(item, new Point(14, 8));
               item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
               AddColonyItem(item, new Point(15, 8));

               item = new Entity(GameData.Instance.AllEntityTypes["item:stones"]);
               AddColonyItem(item, new Point(1, 3));
           
             item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:vine"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:clamwich"]);
               AddColonyItem(item, new Point(16, 7));
               item = new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]);
               AddColonyItem(item, new Point(16, 7));*/
            /*   item = new Entity(GameData.Instance.AllEntityTypes["item:inactivatedFoodCoolerUnit"]);
              AddColonyItem(item, new Point(16, 7));*/
            /*  item = new Entity(GameData.Instance.AllEntityTypes["item:alabasterRay"]);
               AddColonyItem(item, new Point(4, 5));
               item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenStew"]);
               AddColonyItem(item, new Point(4, 5));*/
            /*    item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
                AddColonyItem(item, new Point(16, 7));
                item = new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]);
                AddColonyItem(item, new Point(16, 7));
              */



            // problematic, because crash when you click it ..MP
            /*         item = new Entity(GameData.Instance.AllEntityTypes["item:quaditeCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
                     item.Bulk = 0.3f;  // see comment
                     AddColonyItem(item, new Point(17, 8));
       */

            // 
        }




        public static void MartinAITest()
        {
            /* if (!The.Sim.LoadMap("c Micromap Test"))
                 return;
             */
            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            //expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;

            Allegiances.Allegiance thunderChickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
            Entity thunderChicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 9), 20, thunderChickenAllegiance);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 5));

            Entity man;
            man = PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(14, 5), Color.White, 40f, false, expedition);

            /*Entity item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(16, 9));*/
        }



        public static void MLoAnimationTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo"))
                 return;*/

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            Allegiances.Allegiance thunderChickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
            Entity thunderChicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 9), 20, thunderChickenAllegiance);

            Entity John;
            John = PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(14, 9), Color.White, 40f, false, expedition);

            /*    Entity Fidel;
                Fidel = PlacePerson("Fidel", "Castro ", Reproduction.Male, new Point(13, 7), Color.Beige, 80f, false, expedition, randomScouting);
                Fidel.PersonEntity.HasEatenToday = true;

                Entity Martha;
                Martha = PlacePerson("Martha", "Plympton ", Reproduction.Female, new Point(15, 4), Color.Tan, 50f, false, expedition);
                Martha.PersonEntity.HasEatenToday = true;

                Entity Emily;
                Emily = PlacePerson("Emily", "Blunt ", Reproduction.Female, new Point(10, 6), Color.Brown, 30f, false, expedition, randomScouting + doSomething);
                Emily.PersonEntity.HasEatenToday = true;
                */

            /*  Owner expeditionOwner = The.Sim.Site.GetMainExpedition().ExpeditionOwner;
              Entity skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
              PlaceVehicle(skimmer, new Point(6, 5), expeditionOwner); //185, 81
              skimmer.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);
              skimmer.Name += "Skimmer";
              */

        }

        /*  static GoalPlanner randomScouting = RandomScouting;
          private static byte cycle = 0;
          static void RandomScouting(Entity entity, DebugJobEvaluator eval)
          {
              Intelligence intel = entity.Intelligence;

              cycle++;
              cycle %= 4;

              if (cycle == 0)
              {
                  int xGoal = 100 + (The.Sim.GameplayRandomGenerator.Next(900,"PlaceGameEntities"));
                  int yGoal = 100 + (The.Sim.GameplayRandomGenerator.Next(600, "PlaceGameEntities"));


                  while (The.Map.SubtileIsOrAdjacentToBlockedSubtile(The.Map.TerrainCosts[SurfaceType.TransportType.Foot],
                                         new Point(xGoal / MapManager.subTileSize, yGoal / MapManager.subTileSize)))
                  {
                      xGoal = 100 + (The.Sim.GameplayRandomGenerator.Next(900, "PlaceGameEntities"));
                      yGoal = 100 + (The.Sim.GameplayRandomGenerator.Next(600, "PlaceGameEntities"));
                  }

 
                  //xGoal -= xGoal % 100;//this makes everyone cluster on the same coarse grid points 
                  //yGoal -= yGoal % 100; 


                  ScoutingJob mostDesirableJob = new ScoutingJob(new Vector3(xGoal, yGoal, 0), new EntityGroup(), Priority.Normal);
                  List<EntityGroupID> vehicleOwnerList = new List<EntityGroupID>();
                  if (intel.SetTopLevelGoal( new GoalScouting(entity, mostDesirableJob, vehicleOwnerList, null)
                      {
                          GoalEvaluator = eval
                      }, double.MaxValue))
                  {
                      mostDesirableJob.TakeJob(entity);                  
                  }
              }
              else
              {
                  intel.SetTopLevelGoal(new GoalTakeFive(entity)
                      {
                          GoalEvaluator = eval
                      }, double.MaxValue);
              
              }

          }*/

        static GoalPlanner doSomething = DoSomething;
        static void DoSomething(Entity entity, DebugJobEvaluator eval)
        {
        }



        public static void DecompositionTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //The.Map.AllMaps[12]))
                  return;*/

            The.Sim.DateAndTime.SecondsPerDay = 80;

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            Entity man = PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(8, 8), Color.White, 40f, false, expedition);
            /*   man.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;
               man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
               man.BiologicalEntity.StomachContents = 0.0f;
               */

            //  man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.2f;

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;

            //Entity house = new Entity(GameData.Instance.AllStructureTypes["structure:smokeOven"]);
            // house.FlipHorizontally = true;
            // house.Structure.AddBuildingToWorld(new Point(10, 5), Common.Direction.North, expedition.ExpeditionOwner);
            // house.Initialize(The.Sim.Site);
            // house.Structure.ConstructionFinished(true);


            TerrainTile tile = The.Map.TileMap[17][11];
            tile.Temperature = 255; // -18 degrees
            tile.Moisture = 0f;  //this row of 4 has zero moisture

            tile = The.Map.TileMap[18][11];
            tile.Temperature = 278; // 5 degrees
            tile.Moisture = 0f;

            tile = The.Map.TileMap[19][11];
            tile.Temperature = 284; // 11 degrees
            tile.Moisture = 0f;

            tile = The.Map.TileMap[20][11];
            tile.Temperature = 294; // 21 degrees
            tile.Moisture = 0f;


            The.Map.TileMap[17][12].Temperature = Storage.FreezerTemperature;       //this row of 4 has natural moisture
            The.Map.TileMap[18][12].Temperature = Storage.RefrigeratorTemperature;
            The.Map.TileMap[19][12].Temperature = Storage.EarthCooledTemperature;
            The.Map.TileMap[20][12].Temperature = Storage.AirConTemperature;




            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]);
            AddColonyItem(item, new Point(17, 11));
            item = new Entity(GameData.Instance.AllEntityTypes["item:fishingRodPigFly"]);
            AddColonyItem(item, new Point(2, 2));

            item = new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]);
            AddColonyItem(item, new Point(19, 11));
            item = new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]);
            AddColonyItem(item, new Point(20, 11));

            item = new Entity(GameData.Instance.AllEntityTypes["item:blackzpacho"]);
            AddColonyItem(item, new Point(17, 12));
            item = new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]);

            AddColonyItem(item, new Point(18, 12));
            item = new Entity(GameData.Instance.AllEntityTypes["item:blackzpacho"]);
            AddColonyItem(item, new Point(19, 12));
            item = new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]);
            item.NonLivingEntity.DoConditionDamage(0.95f, null);
            AddColonyItem(item, new Point(20, 12));



            /*    Entity hole = new Entity(GameData.Instance.AllStructureTypes["structure:storageHole"]);
                hole.FlipHorizontally = true;
                hole.Structure.AddBuildingToWorld(new Point(10, 5), Common.Direction.North, expedition.ExpeditionOwner);
                hole.Initialize(The.Sim.Site);
                hole.Structure.ConstructionFinished(true);
                */


        }


        public static void EmptyMap()
        {
            /* if (!The.Sim.LoadMap("4 Micromap"))
                 return;  // micro map
             */
            Kensei.Dev.Options.SetOption("Dev.God mode", true);

            The.MapUI.ZoomToMapPosition(5, 10);


        }




        public static void StructureGeoMap()
        {

            /*  if (!The.Sim.LoadMap("d Mezzomap MLo"))
                  return;
              */
            Kensei.Dev.Options.SetOption("Dev.God mode", true);

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));


            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


            int x = 100;
            int y = 100;


            int xDistance = 200, yDistance = 200;
            int minX = 100, minY = 100;
            int maxX = 1200;
            Vector3 location = new Vector3(minX, minY, 0);

            AddFinishedStructure("structure:campfire", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            /*        AddFinishedStructure("structure:campfirePotCrane", null, expedition, false, location); //mp not implemented
                    UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);
        */
            AddFinishedStructure("structure:lean-toTarp", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:lean-toSpoakLeaves", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:A-frameSpoakLeaves", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:A-frameScraps", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:A-frameTarp", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:lean-toScraps", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:domeShelterTarp", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:domeShelterSpoakShingles", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:wigwamSpoakShingles", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:daysheenTipi", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);
            //moved to scenario folder so cannot be shown in testmap:
            /*       AddFinishedStructure("structure:skimmerHull", null, expedition, false, location);
                   UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

                   AddFinishedStructure("structure:skimmerTail", null, expedition, false, location);
                   UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

                   AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, location);
                   UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);*/

            AddFinishedStructure("structure:storageHole", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:smokeOven", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:abatis", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:abatis", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);


            AddFinishedStructure("structure:storageHole", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:cooledFoodCache", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:smokeOven", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:lean-toTarp", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:improvisedKitchen", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:improvisedWorkbench", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fieldLab", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:cookhouse", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            //muckroot station:
            AddFinishedStructure("structure:domeTent", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:octagonalTent", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:smallTent", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:weatherStation", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fieldKitchen", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:molecularAssembler", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:satelliteGroundStation", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:helipad", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:helipadBig", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:rareMetalRefinery", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            //tau ceti:
            AddFinishedStructure("structure:improvisedGreenhouse", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:kiln", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:improvisedSmithy", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:simpleSmithy", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:firewoodStack", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:compostBin", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:hideRack", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:caneHut", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:clayHut", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:compostPit", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:meatDryingRack", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:dryingShed", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:kilnImprovisedSmall", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:goldFurnace", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:simplePort", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:radioHutImprovised", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:landingImprovised", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:helipadBig", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);


            AddFinishedStructure("structure:fishTrapCreekSticks", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fishTrapCreekNet", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fishTrapCoast", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fishTrapShoreBasket", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            AddFinishedStructure("structure:fishTrapShoreHoopNet", null, expedition, false, location);
            UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);

            //

            /*        AddFinishedStructure("structure:smallPlot", null, expedition, false, location);
                    UpdateXYPos(ref location, xDistance, yDistance, minX, maxX);*/
        }



        private static void UpdateXYPos(ref Vector3 location, int xDistance, int yDistance, int minX, int maxX)
        {
            location.X += xDistance;

            if (location.X > maxX)
            {
                location.X = minX;
                location.Y += yDistance;
            }
        }


        public static void AssetGeometryMap()
        {
            /* if (!The.Sim.LoadMap("d Asset Geometry"))
                 return;  // micro map
             */
            Kensei.Dev.Options.SetOption("Dev.God mode", true);

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


        }

        public static void SkimmerSalvageTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //if (!The.Sim.LoadMap("c Micromap Test")) //if (!The.Sim.LoadMap("d Mezzomap MLo")) // if (!The.Sim.LoadMap("c Micromap Test")) //if (!The.Sim.LoadMap("c Micromap Test")) //if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                 return;
             */

            The.MapUI.ZoomToMapPosition(35, 25);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(35, 20)));



            Vector3 startingLocation = new Microsoft.Xna.Framework.Vector3(576f, 2400f, 0f);

            Entity hull = AddFinishedStructure("structure:skimmerHull", null, expedition, false, startingLocation + new Vector3(96f, 48f, 0f));
            AddFinishedStructure("structure:skimmerEngineTop", null, expedition, false, startingLocation + new Vector3(80f, 0f, 0f));
            AddFinishedStructure("structure:skimmerEngineSide", null, expedition, false, startingLocation + new Vector3(128f, 80f, 0f));
            AddFinishedStructure("structure:skimmerTail", null, expedition, false, startingLocation + new Vector3(0f, 48f, 0f)); // The.Sim.NoOwner, false); // expedition, false); HACK to avoid container. Couldn't get the hull method to work. -MP


            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(35, 20), Color.White, 40f, false, expedition);
            man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));


        }

        public static void HarvestTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test"))
                  return;
              */

            The.MapUI.ZoomToMapPosition(5, 10);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 10)));
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:woodenCookingPot"]), new Point(8, 8));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(5, 10));

            Entity man = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(5, 10), Color.White, 40f, false, expedition);
            /*  man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f       
              man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f       
              man.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f       
              man.BiologicalEntity.AddToStomachContents(-1f);
              */

            Point spot = new Point(5, 13);


            AddResource(spot, "firewood");

            for (int i = 0; i < 12; i++)
            {
                AddColonyItem("item:wetFirewood", spot);
            }

            AddFinishedStructure("structure:firewoodStack", new Point(7, 13), expedition);


            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), spot);

               AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:favorbread"]), new Point(8, 8));

               The.Map.GetTile(spot).AddResource("favorbread", 4);
               */
            /* The.Map.GetTile(spot).AddResource("salt", 8);
             The.Map.GetTile(spot).AddResource("clay", 4);
             */

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(8, 5));

            /*   Entity hull = AddFinishedStructure("structure:storageHole", new Point(14, 7), expedition, false);

               Entity item;
               item = new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]);
               AddColonyItem(item, new Point(15, 5));        */

            //   AddColonyItemToStorage(item, hull);


            //   The.Sim.Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

            // starts harvesting:
            //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;

            //  Owner expeditionOwner = The.Sim.Site.GetMainExpedition().ExpeditionOwner;
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(15, 5)); 

            //      AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackzpacho"]), new Point(15, 5));

            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hexapineSalad"]), new Point(15, 5));

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 5));

            /*Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);
            carcass.Bulk = 1f;
            AddColonyItem(carcass, new Point(5, 8));*/
            //  AddNoOwnerItem(carcass, new Point(22, 5));


            // Entity tent = AddFinishedStructure(new Entity(GameData.Instance.AllEntityTypes["structure:domeTent"]), new Point(10, 5), expedition, false);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(8, 5));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(8, 5));
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(8, 5));
  */

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 5));
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 5));
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(14, 5));
             */

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(14, 5));

            //   AddFinishedStructure("structure:sensor", new Point(15, 7), expedition, false);

            //  PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(15, 9), 20, null, null);

            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 5));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 5));
             */
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(14, 5));


            /* man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
           man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));*/
            //  man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.01f;

            /*   man.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.1f; //0.2f
               man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f
               man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f; //0.2f
               */
            /*  man = PlacePerson("Ipsum", "Dolores", Reproduction.Male, new Point(14, 5), Color.White, 40f, false, expedition);
              man.Intelligence.Skills = new Dictionary<SkillType, Skill>();
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
              man.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
              */
        }

        public static void TurnipHutTest()
        {
            /*   if (!The.Sim.LoadMap("d Mezzomap MLo"))
                   return;
               */

            The.MapUI.ZoomToMapPosition(3, 10);

            Point campPos = new Point(8, 3);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            Entity man = GetBob(new Point(9, 10), expedition);

            Entity shell = new Entity(GameData.Instance.AllEntityTypes["item:turnipShell"]);
            shell.Bulk = 3f;
            AddColonyItem(shell, new Point(9, 12));

            shell = new Entity(GameData.Instance.AllEntityTypes["item:turnipShell"]);
            shell.Bulk = 3f;
            AddColonyItem(shell, new Point(9, 15));

            Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
            carcass.Bulk = 4.9f;
            AddColonyItem(carcass, new Point(12, 5));

            carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
            carcass.Bulk = 4.9f;
            AddColonyItem(carcass, new Point(15, 5));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCracker"]), campPos);



            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), campPos);


        }

        public static void PathingTest()
        {
            /* if (!The.Sim.LoadMap("c Micromap Test")) //"d Mezzomap MLo" //"c Micromap Test")) //The.Map.AllMaps[4]))
                 return;  // micro map
             */
            Point campPos = new Point(24, 25);

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));


            BodyComponent body;

            Entity e1 = GetBob(campPos, expedition);
            e1.Intelligence.SetName("Bob", "1");
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);
            GetBob(campPos, expedition);


            Allegiances.Allegiance bushdragonAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);

            PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(30, 30), 20, bushdragonAllegiance, "Pale race");
            PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(25, 30), 20, bushdragonAllegiance, "Pale race");

        }

        public static void VerminTest()
        {
            Point campPos = new Point(16, 5);
            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);
            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));


            Entity bob = GetBob(campPos - new Point(3, 4), expedition);
            bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0;
            bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0; //0.2f      
            bob.BiologicalEntity.AddToStomachContents(-1f);


            // AddFinishedStructure("structure:octagonalTent", new Point(campPos.X - 4, 5), expedition, false);

            // AddFinishedStructure("structure:sensor", new Point(campPos.X - 0, campPos.Y - 4), expedition, false);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), campPos);

            UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
            Expedition exp1 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(campPos));

            // Entity dog = PlaceAnimal("entity:dog", Reproduction.Male, new Point(15, 9), 5f, The.Sim.PlaySite.PlayerAllegiance);
            /* dog.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
             dog.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;*/
            //  dog.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), campPos);
            /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), campPos);
             /*   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), campPos);
           */

            foreach (var item in GameData.Instance.AllEntityTypes)
            {
                The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(item.Value, ProductionStatistics.StatTypes.Produced, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
                The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(item.Value, ProductionStatistics.StatTypes.ConsumedFood, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
                The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(item.Value, ProductionStatistics.StatTypes.Degraded, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
                The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(item.Value, ProductionStatistics.StatTypes.EatenByCreatures, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
                The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(item.Value, ProductionStatistics.StatTypes.Disappeared, The.Sim.GameplayRandomGenerator.Next(20, "fd"));

            }



            for (int i = 0; i < 10; i++)
            {
                //  Entity e2 = PlaceAnimal("entity:binalRat", Reproduction.Female, campPos, 18, all2, null, exp1);
                /* e2.Intelligence.DisableAI = true;
                 BodyComponent body;
                 e2.Find(out body);
                 body.Body.GlobalHitpoints = 350;
                 */
                //  e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
            }

        }

        public static void CookingTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //"d Mezzomap MLo" //"c Micromap Test")) //The.Map.AllMaps[4]))
                  return;  // micro map
              */
            Point campPos = new Point(16, 5);

            The.MapUI.ZoomToMapPosition(campPos.X, campPos.Y);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(campPos));

            /*
             Map Position X:41 Y:14
            Location X:2008 Y:697             
             */

            The.Sim.DateAndTime.TimeOfDay = 0.4;
            BodyComponent body;

            Entity e1 = GetBob(campPos, expedition);
            e1.Intelligence.SetName("Bob", "1");

            Entity oven = AddFinishedStructure("structure:smokeOven", new Point(16, 3), expedition, false);

            // ToolContainer ovenAsTool = oven.Contains as ToolContainer;           

            /*   for (int i = 0; i < 10; i++)
               {
                   Entity carbonTail = new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]);
                   AddColonyItem(carbonTail, new Point(6, 8));

                   ovenAsTool.AddToContain(carbonTail, replenish: true);

                
               }*/

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), campPos);


            UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
            Expedition exp1 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(campPos));
            Entity e2 = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(12, 5), 18, all2, null, exp1);
            e2.Intelligence.DisableAI = true;


            /*   e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.9f;  //0.2f
               */
            /* Entity e2 = GetBob(campPos, expedition);
            e2.PersonEntity.SetName("Bob", "2");
           e2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f
            e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.9f;  //0.2f
           

            Entity e3 = GetBob(campPos, expedition);
            e3.PersonEntity.SetName("Bob", "3");
           /* bool noSkills = true;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan1", Reproduction.Male, campPos, Color.White, 52f, noSkills, "blue1", expedition);
            e1 = PlacePerson("Ward", "Conlan2", Reproduction.Male, campPos, Color.White, 52f, noSkills, "blue1", expedition);
            e1 = PlacePerson("Ward", "Conlan3", Reproduction.Male, campPos, Color.White, 52f, noSkills, "blue1", expedition);
            */


            //e1.BiologicalEntity.StomachContents = 0.8f;


            //  Entity storage = AddFinishedStructure("structure:storageHole", new Point(17, 3), expedition, false);
            // AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), storage);

            return;

            //  Entity storage = AddFinishedStructure("structure:storageHole", new Point(17, 3), expedition, false);
            // AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), storage);


            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenRawhide"]), campPos);
            AddFinishedStructure("structure:kilnImprovisedSmall", new Point(15, 1), expedition, false);
            AddFinishedStructure("structure:campfire", new Point(19, 3), expedition, false);
            AddFinishedStructure("structure:smokeOven", new Point(14, 3), expedition, false);
            AddFinishedStructure("structure:kiln", new Point(16, 3), expedition, false);
            AddFinishedStructure("structure:hideRack", new Point(18, 5), expedition, false);
            AddFinishedStructure("structure:meatDryingRack", new Point(20, 5), expedition, false);
            AddFinishedStructure("structure:compostPit", new Point(22, 3), expedition, false);
            AddFinishedStructure("structure:dryingShed", new Point(15, 5), expedition, false);
            AddFinishedStructure("structure:mudBrickKitchen", new Point(13, 5), expedition, false);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:guano"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vat"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:unfiredClayPot"]), campPos);

            //           AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenRawhide"]), new Point(16, 5));
            //         AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), campPos);
            //           AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGreenHide"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenBrain"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            /*
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipGuts"]), new Point(12, 5));
                              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipBrain"]), new Point(12, 5));

                        AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos); //
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), campPos);
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
                          AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(12, 5));
            */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), campPos);
            /*     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelKnife"]), campPos);
                 AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), campPos);
            */
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fingerFruit"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fingerFruit"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vinegar"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:salt"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:salt"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:salt"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(16, 5));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), new Point(16, 5));



            //    Entity kitchen = AddFinishedStructure("structure:improvisedKitchen", new Point(14, 5), expedition, false);
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), campPos);
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);

            /*  Entity rotor = kitchen.FindPartOfType(GameData.Instance.AllEntityTypes["item:spoakLeaves"]);
              rotor.NonLivingEntity.DoConditionDamage(100f, true); // break it...
             */
            /*
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), campPos);
             
           // return;

            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
            item.Bulk = 0.5f;  // see comment
            AddColonyItem(item, campPos);
            item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"]);   
            item.Bulk = 0.5f; 
            AddColonyItem(item, campPos);

            item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   
            item.Bulk = 1f; 
            AddColonyItem(item, new Point(16, 5));
            item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   
            item.Bulk = 1f; 
            AddColonyItem(item, new Point(16, 5));

            item = new Entity(GameData.Instance.AllEntityTypes["item:quaditeCarcass"]);   
            item.Bulk = 0.4f;  
            AddColonyItem(item, new Point(16, 5));


            Entity food = new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]);
            AddColonyItem(food, new Point(16, 5));
          

            food = new Entity(GameData.Instance.AllEntityTypes["item:roastedMuckGrinder"]);
            AddColonyItem(food, new Point(16, 5));
            */
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hydraulicsComponents"]), new Point(16, 5));


            //  AddFinishedStructure("structure:improvisedKitchen", null/*new Point(12, 3)*/, expedition, false, new Vector3(2008f, 697f, 0f));

            /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), campPos);
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vine"]), campPos);
             */
            /*
             "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:panelScraps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false }, //
                                     new Input() { Entity = "item:spoakLeaves"*/


        }

        public static void ImmobilizeEntity(Entity e1)
        {
            e1.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed = 0.01f;
            e1.EntityType.LocomotorType.LeggedLocomotorType.WalkSlowSpeed = 0.01f;
            e1.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed = 0.01f;
            e1.EntityType.LocomotorType.LeggedLocomotorType.RunSpeed = 0.01f;

        }

        public static void RepairTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //d Mezzomap MLo")) //c Micromap Test")) //The.Map.AllMaps[4]))
                  return;
              */

            The.MapUI.ZoomToMapPosition(5, 2);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(8, 6)));

            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(8, 8), Color.White, 52f, false, "blue1", expedition);


            Entity hut = AddFinishedStructure("structure:clayHut", new Point(9, 8), expedition, false);
            Entity part = hut.NonLivingEntity.Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:spoakBranchesTrimmed");
            /*  part.NonLivingEntity.DoConditionDamage(0.9f, null);
              hut.NonLivingEntity.DoIntegrityDamage(0.9f);
                */



            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), new Point(8, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(8, 6));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWire"]), new Point(8, 6));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedSpade"]), new Point(8, 6));

            /*  AddFinishedStructure("structure:sensor", new Point(8, 5), expedition, false, null);

              Entity sentry = AddFinishedStructure("structure:sentry", new Point(10, 12), expedition, false, null);
              Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
              AddColonyItem(ammo, new Point(10, 12));
              */

        }


        public static void AttackNestTest()
        {
            /*  if (!The.Sim.LoadMap("c Micromap Test")) //d Mezzomap MLo")) //c Micromap Test")) //The.Map.AllMaps[4]))
                  return;
              */
            The.MapUI.ZoomToMapPosition(5, 2);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(8, 6)));


            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(8, 8), Color.White, 52f, false, "blue1", expedition);
            e1.SetRotationAndDir(-MathHelper.PiOver2);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(8, 6));  //shortest way of placing stuuf

            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
            Entity nest = new Entity(GameData.Instance.AllEntityTypes["terrain:quaditeNest"]);
            nest.Initialize(The.Sim.PlaySite, twinklerAllegiance);
            nest.Name = "North quadite nest";
            nest.PlaceEntityOnPlaySite(new Vector3(100f, 600f, 0f), null, Entity.StructureState.Finished, null);


            // Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(5, 5), 20, twinklerAllegiance);  //twinklerAllegiance


            /*  list.Add(new ActionSets()
              {
                  KeyName = "sulfurBombActivated",

                  SetsOfActions = new []{

                      new ActionSetType(){   Actions = new EventActionType[]{ 
                      new EventActionType()
                      {  // kills the nest after 5 s
                          DelayInSeconds = 5f,
                          DestroyEntity = new DestroyEntityAction(){
                               TargetObject = new TargetObject()
                               {
                                    TargetElement = TargetObjectType.TargetEntity
                               }
                         // EntityToDestroy = TargetEntityOfAction.TargetEntity
                      }
                      },
                      new EventActionType()
                      {  // kills the nest after 5 s
                          DelayInSeconds = 5f,
                          SetPropertyAction = new SetPropertyAction()
                          {
                              PropertyKey = "nestDestroyed",// to avoid exposition about how to kill a nest once the player has learned this
                              BoolValue = true
                          }                                   
                                  
                      },
                                
                      new EventActionType()
                      {
                          ParticleEffectAction = new ParticleEffectAction(){
                              DynamicLocation = new DynamicLocation(){ TargetObject = new TargetObject(){ TargetElement = TargetObjectType.TargetEntity } },
                           //   Target = ParticleEffectAction.TargetOfAction.TargetEntity, // use entity location, don't attach emitter
                              DurationInSeconds = 12d,
                              ParticleEmitters = new[]{ new ParticleEmitterEffect()
                              {
                                  AttachToEntity = false, // keep emitting after nest destroyed
                                  ParticleSystemKey = "sulfurBomb",
                              }}
                          }
                             
                      }
                     
                    
                               }} 
                           }
              });*/
        }

        public static void LightingTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //c Micromap Test")) //The.Map.AllMaps[4]))
                 return;  // micro map
             */
            The.MapUI.ZoomToMapPosition(5, 2);
            The.Sim.DateAndTime.TimeOfDay = 0.8;

            Point spot = new Point(5, 5);
          Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(spot));

            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
            expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);


            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(5, 2), Color.White, 52f, false, "blue1", expedition);
            e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
          //  e1.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.2f;

            AddFinishedStructure("structure:domeTent", new Point(9, 2), expedition, false);
            AddFinishedStructure("structure:octagonalTent", new Point(12, 2), expedition, false);
            AddFinishedStructure("structure:smallTent", new Point(15, 2), expedition, false);

          
            AddColonyItem("item:firewood", spot);
            AddColonyItem("item:firewood", spot);
            AddColonyItem("item:firewood", spot);
            AddFinishedStructure("structure:campfire", new Point(7, 7), expedition, false);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), spot);

            AddFinishedStructure("structure:kiln", new Point(4, 7), expedition, false);
            
        }

        public static void WeaponsTest()
        {
            /* if (!The.Sim.LoadMap("d Mezzomap MLo")) //c Micromap Test")) //The.Map.AllMaps[4]))
                 return;  // micro map
             */
            The.MapUI.ZoomToMapPosition(5, 2);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));

          //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(5, 14));
        //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(5, 14));
        //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(5, 14));

          //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(7, 10));

           /* AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(5, 10));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(5, 10));
            */
           /* Entity item = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);   //when placing an item through code, check if it has unique bulk. if yes, then you need to set the bulk value manually like here
            item.Bulk = 1f;  // see comment
            AddColonyItem(item, new Point(10, 10));
            */
            //   AddFinishedStructure("structure:domeShelterTarp", new Point(9, 12), expedition, false);

            The.Sim.DateAndTime.TimeOfDay = 0.8;
            Entities.Body.Body body;

            bool noSkills = false;  //'false' means no skills, 'true' means skills are defined under the character
            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills, "blue1", expedition);
            e1.SetRotationAndDir(-MathHelper.PiOver2);

            PlacePerson("Ward2", "Conlan2", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills, "blue1", expedition);
          //  PlacePerson("Ward3", "Conlan2", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills, "blue1", expedition);

            Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]);
            AddColonyItem(ammo, new Point(5, 5));
            ammo.Item.Ammunition.NoOfRounds = 1;

            Entity rifle = new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]);
            AddColonyItem(rifle, new Point(5, 5));

            Container magazine = rifle.Contains;
            Entity surplusAmmo;
            magazine.AddToContain(ammo, out surplusAmmo);


            ammo = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
            AddColonyItem(ammo, new Point(6, 5));
          //  ammo.Item.Ammunition.NoOfRounds = 1;

            Entity e = new Entity(GameData.Instance.AllEntityTypes["item:sentrySprayGun"]);
            AddColonyItem(e, new Point(6, 5));
            /* magazine = rifle.Contains;
             magazine.AddToContain(ammo, out surplusAmmo);*/

            AddColonyItem("item:sentryWeaponMount", new Point(6, 5));

            AddColonyItem("item:spraySentry", new Point(6, 5));

            

            /*
               e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.5f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.5f; //0.2f
               e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;

               e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
               e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
               */

            // e1 = PlacePerson("1", "2", Reproduction.Male, new Point(5, 3), Color.White, 52f, false, "blue1", expedition);

            /* e1 = PlacePerson("2", "2", Reproduction.Male, new Point(9, 2), Color.White, 52f, noSkills, "blue1", expedition);
             e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating
             e1.SetRotationAndDir(-MathHelper.PiOver2);
             e1.Find(out body);
             body.ChangeMaxHitpoints(150.0f);

             e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
             e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
            
             // immobilize:
             ImmobilizeEntity(e1);
          */



            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(8, 3));
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(8, 3));


            /*    Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:quaditeCarcass"]);
                //Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
                carcass.Bulk = 10f;
                AddColonyItem(carcass, new Point(5, 5));
                */


            //    Entity e2 = PlacePerson("2", Reproduction.Male, new Point(15, 5), Color.White, 52f, false, "blue1", expedition);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(15, 5));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(14, 5));


            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedWaterGun"]), new Point(14, 5));
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(14, 5));


            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(15, 5));
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicArrow"]), new Point(15, 5));

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(16, 5));

            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(8, 5));

            //   Entity e2 = PlacePerson("2", Reproduction.Male, new Point(16, 5), Color.White, 52f, noSkills, "blue1", expedition);


            //OPPONENT:  
            //  PlaceAnimal("entity:binalRat", Reproduction.Male, new Point(10, 14), 20);

            //    Entity a = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(10, 15), 20);
            //   Entity a = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(10, 15), 20);
            //   ImmobilizeEntity(a);

            //  PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(14, 15), 20);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(5, 2));
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(5, 2));

            /*  Entity sentry = AddFinishedStructure("structure:sentry", new Point(5, 2), expedition, false, null);
              Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
              AddColonyItem(ammo, new Point(5, 2));*/

            /*  Container magazine = sentry.Parts[0].Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentryGun").Contains;
              Entity surplusAmmo;
              magazine.AddToContain(ammo, out surplusAmmo);*/

            /*   string opponentType = "entity:twinkler"; //"entity:patrician"; // //"entity:bushDragon"; // "entity:thunderChicken"; //"entity:patrician"; //

               Allegiances.Allegiance opponentAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes[opponentType]);

               for (int i = 0; i < 4; i++)
               {
                   Entity chicken = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(3 + i * 1, 3), 20, opponentAllegiance);
                   // chicken.Intelligence.DisableAI = true;
               }
               */

            /*
            Entity opponent = PlaceAnimal(opponentType, Reproduction.Male, new Point(1, 3), 20, opponentAllegiance, null);
            opponent.Find(out body);
            

            //  body.ChangeMaxHitpoints(1.0f);
           // body.ChangeMaxHitpoints(10000.0f);

            // immobilize:
            opponent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed = 0.1f;
            opponent.EntityType.LocomotorType.LeggedLocomotorType.WalkSlowSpeed = 0.1f;
            opponent.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed = 0.1f;
            opponent.EntityType.LocomotorType.LeggedLocomotorType.RunSpeed = 0.1f;
            */
            //AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(8, 5)); 


            //Entity nest = new Entity(GameData.Instance.AllEntityTypes["terrain:quaditeNest"]);
            //nest.Initialize(The.Sim.Site, opponentAllegiance);
            //nest.Name = "North quadite nest";
            //nest.PlaceInWorld(new Vector3(100f, 600f, 0f));

            //Allegiances.Allegiance chickenAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


            //Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(8, 88), 20, chickenAllegiance);

            //chicken.Find(out body);
            //body.ChangeMaxHitpoints(1.0f); ;


            /*
                        Allegiance.Allegiance chickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

                        for (int i = 0; i < 1; i++)
                        {
                            Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(10 + i * 1, 1 + i * 2), 20, chickenAllegiance);
                            chicken.Intelligence.DisableAI = true;
                        }*/


            //    Body body;
            //    chicken.Find(out body);
            //    body.ChangeMaxHitpoints(1.0f);;

            //    chicken = PlaceAnimal("entity:thunderChicken", Sim.Reproduction.Male, new Point(2, 4), 20, chickenAllegiance);
            //    chicken.Find(out body);
            //    body.ChangeMaxHitpoints(1.0f);;


            //  PlaceAnimal("entity:forestGuardian", Sim.Reproduction.Male, new Point(18, 8), 20);

            //  Entity entity = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(6, 4), 20, );
            //  entity.Renderable./*TODO DECOUPLE*/RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
            //   

            //  PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(4, 4), 20, twinklerAllegiance);

            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(5, 5));


        }

        /*
        private void Creaturetest()
        {
            if (!The.Sim.LoadMap(The.The.Map.AllMaps[4]))
                return;  // micro map

            The.MapUI.ZoomToMapPosition(5, 10, MapUI.Centering.Middle);

            Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(15, 5)))
            {
                Name = "Start",
                Allegiance = Site.PlayerAllegiance
            };
            Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

            man = PlacePerson("Karol Nikolaev", Reproduction.Male, new Point(12, 12), Color.White, 40f, false);
            man.PersonEntity.HasEatenToday = true;

            man.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
        }*/


        /* private void Regiontest()
         {
             if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                 return;   //micro map

             The.MapUI.ZoomToMapPosition(5, 10, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(15, 5)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             // starts harvesting:
             //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;



             Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
             carcass.Bulk = 10f;
             AddColonyItem(carcass, new Point(10, 5));

             carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
             carcass.Bulk = 10f;
             AddColonyItem(carcass, new Point(20, 5));

             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(15, 5));
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 5));
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(15, 5));

             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["structure:campfire"]), new Point(16, 9));

             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(18, 7));

        
             Allegiance.Allegiance chickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


             //    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(2, 2), 20, chickenAllegiance);

             //    Body body;
             //    chicken.Find(out body);
             //    body.ChangeMaxHitpoints(1.0f);;

             //    chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(2, 4), 20, chickenAllegiance);
             //    chicken.Find(out body);
             //    body.ChangeMaxHitpoints(1.0f);;
                 
             AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(15, 5));

             //  PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(18, 8), 20);

             //  Entity entity = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(14, 15), 20);
             //  entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
               

             Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

             //     Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(5, 10), 20, twinklerAllegiance);  //twinklerAllegiance
             //     twinkler.Find(out body);
             //     body.ChangeMaxHitpoints(1.0f);0;

             //     twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(7, 11), 20, twinklerAllegiance);
             //     twinkler.Find(out body);
             //     body.GlobalHitpoints = 8;

             //     twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(8, 9), 20, twinklerAllegiance);
             //     twinkler.Find(out body);
             //     body.GlobalHitpoints = 5;
              




             // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(16, 7));

             // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipmeat"]), new Point(16, 5));
             //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipmeat"]), new Point(16, 5));

             //   Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
             //   campfire.Structure.AddBuildingToWorld(new Point(15, 10), Common.Direction.North, expedition.ExpeditionOwner);
             //   campfire.Initialize(Site);
             //   campfire.Structure.ConstructionFinished(true);
                


             //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipcarcass"]), new Point(10, 5));


             //item = Item.CreateItem(GameData.Instance.AllEntityTypes["item:turnipcarcass"]);
             //AddColonyItem(item, new Point(10, 5));

             
             //   item = Item.CreateItem(GameData.Instance.AllEntityTypes["item:meat"]);
             //   AddColonyItem(item, new Point(4, 5));
             
             //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(208, 89), Color.White, 52f, false);
             //  ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //  ward.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2); 
             // ward.Intelligence.DisableAI = true;

             //    man = PlacePerson("Sarah Connor", Reproduction.Female, new Point(10, 9), Color.White, 40f, false);
             //    man.PersonEntity.HasEatenToday = true;

             //    man = PlacePerson("Karol Nikolaev", Reproduction.Female, new Point(5, 5), Color.White, 40f, false);
             //    man.PersonEntity.HasEatenToday = true; 

             //   Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

             //Entity twinkler; //202,83

             //   Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(10, 10), Color.White, 52f, false);
             //    ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //Body body;
             //ward.Find(out body);
             //body.GlobalHitpoints = 50;

            // ward.Intelligence.Morale = 0.3f; 

             //   int j = 0;
             //      for (int x = 10; x <= 18; x+=2)
             //      {
             //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);  
             //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);
             //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);

             //         for (int y = 9; y < 16; y+=2)
             //          {
             //              twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                           //twinkler.MobileEntity.TestWander = true;
             //          }               
             //      }
                   
             //   twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(18, 20), 20, twinklerAllegiance);
             //    twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(16, 10), 20, twinklerAllegiance);
             //  twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(16, 10), 20, twinklerAllegiance);

             //int i = 0;

             //    man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(20, 15), Color.White, 40f, false);
             //    man.PersonEntity.HasEatenToday = true;
            //     man.Intelligence.Morale = 0.5f;
             //    i++; 

            // man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(0, 5), Color.White, 40f, false);
            // man.PersonEntity.HasEatenToday = true;
            // man.Intelligence.Skills[GameData.Instance.AllSkillTypes["butchering"]].Value = 0.5f;

            // i++;
             //  man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.05f;
             //   man.BiologicalEntity.Needs.NeedsList["food"].CurrentLevel = 0.05f;


            // man = PlacePerson("Adam West", Reproduction.Male, new Point(12, 5), Color.White, 40f, false);
            // man.PersonEntity.HasEatenToday = true;
            // man.Intelligence.Skills[GameData.Instance.AllSkillTypes["butchering"]].Value = 0.5f;


             
            // Entity skimmer = PlaceVehicle("entity:skimmer", new Point(16, 6), expedition.ExpeditionOwner);

            // Entity rotor = skimmer.FindPartOfType(GameData.Instance.AllEntityTypes["item:skimmerrotor"]);
            // rotor.Item.TurnIntoJunk(); // break it...
             

             //   Allegiance.Allegiance guardianAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:forestGuardian"]);

              //  Entity animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(5, 5), 20, guardianAllegiance, "Dark race"); 
             //    animal.Intelligence.DisableAI = false;

             //skimmer.Condition = 0.09f;


            //    for (int x = 8; x < 12; x += 3)
             //   {
             //       for (int y = 5; y < 7; y += 2)
             //       {
             //           man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(x, y), Color.White, 40f, false);
             //           man.PersonEntity.HasEatenToday = true;
            //            i++;
            //        }

            //    } 

             //    PlaceAnimal("entity:patrician", Reproduction.Male, new Point(13, 8), 20);

             //  Allegiance.Allegiance chickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

           //    Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(8, 8), 20, chickenAllegiance);
           //    Entities.Body.Body body;
            //   chicken.Find(out body);
            //   body.ChangeMaxHitpoints(1.0f);0;
            //   PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(8, 9), 20, chickenAllegiance);
            //   PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(8, 10), 20, chickenAllegiance);
            //   PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(7, 8), 20, chickenAllegiance);
            //   PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(9, 9), 20, chickenAllegiance);
            //   PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(10, 9), 20, chickenAllegiance);
               

             //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(15, 5));


             //  PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(18, 8), 20);

             //  Entity entity = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(14, 15), 20);
            //   entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
               
             //  twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(5, 10), 20, twinklerAllegiance);
             //  twinkler.MobileEntity.TestWander = true;

             //   twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(17, 10), 20, twinklerAllegiance);
             // twinkler.MobileEntity.TestWander = true;
         }*/


        private static void FightTest()
        {
            /* if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                 return;  // micro map
             */
            The.MapUI.ZoomToMapPosition(15, 5);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));

            //     Entity sentry = AddFinishedStructure("structure:sentry", new Point(9, 6), expedition, false);

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(15, 5));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 5));


            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(15, 8));
            //  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(15, 8));
            // AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(15, 8));
            //   AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(15, 8));
            //    AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(11, 3));
            /*  AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(11, 3));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(11, 3));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(11, 3));
              AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(11, 3));
              */

            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvedFireExtinguisher"]), new Point(11, 3));
            //     AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]), new Point(11, 3));

            GetBob(new Point(15, 5), expedition);
            GetBob(new Point(15, 5), expedition);
            GetBob(new Point(15, 5), expedition);

            Entity e1;
            e1 = PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(15, 5), Color.White, 40f, false, expedition);

            e1.Intelligence.Skills = new Dictionary<SkillType, Skill>();
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.75f, GameData.Instance.AllSkillTypes["menial"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
            e1.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));

            /*  e1.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f
              e1.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f
              e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f; //0.2f
              e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;  //0.2f
              */
            // PlaceAnimal("entity:dog", Reproduction.Male, new Point(6, 6), 5f, The.Sim.PlaySite.PlayerAllegiance, null, expedition); // expedition is owner and at the same time the dog is member


            //   man = PlacePerson("Dude", "Dudeson", Reproduction.Male, new Point(14, 5), Color.White, 40f, false, expedition);

            // Entities.Body.Body body = man.Body;

            /*  BodyPart part;
              part = body.FindBodyPartOfType(man.EntityType.BodyType.FindBodyPart("Head"));
              part.DoDamage(10f);

              part = body.FindBodyPartOfType(man.EntityType.BodyType.FindBodyPart("Left leg"));
              part.DoDamage(10f);
              */



            Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);
            //   Allegiances.Allegiance twinklerAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            int y = 6;


            Entity twinkler = PlaceAnimal("entity:patrician",
                                            Reproduction.Male,
                                            new Point(12, 5), //x + 5),
                                            20,
                                            twinklerAllegiance);

            BodyComponent body;
            twinkler.Find(out body);
            body.Body.ChangeMaxHitpoints(1000f);

            return;

            // ImmobilizeEntity(twinkler);

            // }

            /*  twinkler = PlaceAnimal("entity:twinkler",
                                                  Reproduction.Male,
                                                  new Point(15, 5), //x + 5),
                                                  20,
                                                  twinklerAllegiance);
              ImmobilizeEntity(twinkler);

              twinkler = PlaceAnimal("entity:twinkler",
                                                 Reproduction.Male,
                                                 new Point(5, 8), //x + 5),
                                                 20,
                                                 twinklerAllegiance);
              ImmobilizeEntity(twinkler);

              twinkler = PlaceAnimal("entity:twinkler",
                                               Reproduction.Male,
                                               new Point(10, 8), //x + 5),
                                               20,
                                               twinklerAllegiance);
              ImmobilizeEntity(twinkler);

              twinkler = PlaceAnimal("entity:twinkler",
                                                 Reproduction.Male,
                                                 new Point(15, 8), //x + 5),
                                                 20,
                                                 twinklerAllegiance);
              ImmobilizeEntity(twinkler);*/
        }

        /*private void MilestoneBuild()
        {
            if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                return;

            The.MapUI.ZoomToMapPosition(215, 95, MapUI.Centering.Middle);

            Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(216, 95)))
            {
                Name = "Start",
                Allegiance = Site.PlayerAllegiance
            };
            Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);


            GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 32;

            // Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            //Entity twinkler; //202,83

            //   Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(10, 10), Color.White, 52f, false);
            //    ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            //Body body;
            //ward.Find(out body);
            //body.GlobalHitpoints = 50;

            //ward.Intelligence.Morale = 0.3f; 

            //   int j = 0;
            //      for (int x = 10; x <= 18; x+=2)
            //      {
            //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);  
            //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);
            //          twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, 10), 20, twinklerAllegiance);

            //         for (int y = 9; y < 16; y+=2)
            //          {
            //              twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                          //twinkler.MobileEntity.TestWander = true;
            //          }               
            //      }
                   
            //   twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(18, 20), 20, twinklerAllegiance);
            //    twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(16, 10), 20, twinklerAllegiance);
            //  twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(16, 10), 20, twinklerAllegiance);

            int i = 0;

            //    man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(20, 15), Color.White, 40f, false);
            //    man.PersonEntity.HasEatenToday = true;
           //     man.Intelligence.Morale = 0.5f;
             //   i++;
 
            Entities.Body.Body body;


            Entity e1 = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(215, 95), Color.White, 52f, false, "blue1");
            e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating
            e1.Find(out body);
            body.ChangeMaxHitpoints(150.0f);
            //    e1.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.05f; //0.2f
            //    e1.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.05f; //0.2f
            //    e1.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.15f;  //0.2f
            //    e1.BiologicalEntity.StomachContents = 0f; 


            Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(214, 96), Color.White, 52f, false, "blue2");
            e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating
            e2.Find(out body);
            body.ChangeMaxHitpoints(150.0f);
            //  e2.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel = 0.2f;  //0.2f
            //  e2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.02f;  //0.2f
            //  e2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.1f;  //0.2f
            //  e2.BiologicalEntity.StomachContents = 0f; 


            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(215, 96), Common.Direction.North, expedition.ExpeditionOwner);
            campfire.Initialize(Site);
            campfire.Structure.ConstructionFinished(true);

            // campfire.Structure.AddonTo = house1;
            //   house1.Structure.AddOns.Add(campfire);



            Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]);
            AddColonyItem(item, new Point(216, 97));

            //  Entity carcass = new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"]);
            //  carcass.Bulk = 10f;
            //  AddColonyItem(carcass, new Point(10, 5));
              
            item = new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]);
            AddColonyItem(item, new Point(215, 94));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(213, 95));

            item = new Entity(GameData.Instance.AllEntityTypes["item:firewood"]);
            AddColonyItem(item, new Point(215, 96));

            item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
            AddColonyItem(item, new Point(215, 96));
            item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
            AddColonyItem(item, new Point(215, 96));
            item = new Entity(GameData.Instance.AllEntityTypes["item:branches"]);
            AddColonyItem(item, new Point(215, 96));

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(215, 94));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(213, 95));
            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(215, 96));


            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(216, 97));



            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(216, 97));



            //     Entity skimmer = PlaceVehicle("entity:skimmer", new Point(215, 93), expedition.ExpeditionOwner);
            //     skimmer.Vehicle.Pitch = 0.3f;
            //     skimmer.Vehicle.Roll = 0.22f;
            //     skimmer.RenderAsModel.SetRotationAndDir(MathHelper.PiOver4); 

            //   Entity rotor = skimmer.FindPartOfType(GameData.Instance.AllEntityTypes["item:skimmerrotor"]);
            //   rotor.Item.TurnIntoJunk(); // break it...

            //Body skimmerBody;
            //skimmer.Find(out skimmerBody);
            //skimmerBody.FunctionalScore = 0f; // make sure it doesn't fly...
             

            //skimmer.Condition = 0.09f;


            //   for (int x = 8; x < 12; x += 3)
            //   {
            //       for (int y = 5; y < 7; y += 2)
            //       {
            //           man = PlacePerson("Karol Nikolaev " + i, Reproduction.Male, new Point(x, y), Color.White, 40f, false);
            //           man.PersonEntity.HasEatenToday = true;
            //           i++;
            //       }
            //   } 

            //    PlaceAnimal("entity:patrician", Reproduction.Male, new Point(13, 8), 20);

            Entity animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10576, 4353), 20, null, "Dark race");
            animal.Intelligence.DisableAI = false;

            Allegiance.Allegiance chickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);


            Entity chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(223, 88), 20, chickenAllegiance);

            chicken.Find(out body);
            body.ChangeMaxHitpoints(1.0f);;

            chicken = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(218, 84), 20, chickenAllegiance);
            chicken.Find(out body);
            body.ChangeMaxHitpoints(1.0f);;

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(215, 95));

            //  PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(18, 8), 20);

            //  Entity entity = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(14, 15), 20);
            //  entity.Renderable.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
               



            
            //Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);         

            //Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(213, 101), 20, twinklerAllegiance);  //twinklerAllegiance
            //twinkler.Find(out body);
            //body.ChangeMaxHitpoints(1.0f);0;

            //twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(210, 101), 20, twinklerAllegiance);
            //twinkler.Find(out body);
            //body.GlobalHitpoints = 8;

            //twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(211, 99), 20, twinklerAllegiance);
            //twinkler.Find(out body);
            //body.GlobalHitpoints = 5;       

        }*/









        /*   private void AIdemo()
           {
               if (!The.Sim.LoadMap(The.The.Map.AllMaps[0]))
                   return;

               The.MapUI.ZoomToMapPosition(218, 95, MapUI.Centering.Middle);

               Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(210, 90)))
               {
                   Name = "Start",
                   Allegiance = Site.PlayerAllegiance
               };
               Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

               // starts harvesting:
               //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


               Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


               Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(208, 89), Color.White, 52f, false);
               ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

               ward.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
               // ward.Intelligence.DisableAI = true;

               //    man = PlacePerson("Sarah Connor", Reproduction.Female, new Point(10, 9), Color.White, 40f, false);
               //    man.PersonEntity.HasEatenToday = true;

               //    man = PlacePerson("Karol Nikolaev", Reproduction.Female, new Point(5, 5), Color.White, 40f, false);
               //    man.PersonEntity.HasEatenToday = true; 

               Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

               Entity twinkler; //202,83

               for (int x = 202; x < 212; x += 3)
               {
                   for (int y = 97; y < 101; y += 2)
                   {

                       twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                   }

                   // for (int y = 9; y < 16; y+=2)
                    //{
                    //    twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(x, y), 20, twinklerAllegiance);
                        //twinkler.MobileEntity.TestWander = true;
                    //}    
               }

               for (int x = 202; x < 212; x += 3)
               {
                   for (int y = 87; y < 91; y += 2)
                   {
                       man = PlacePerson("Karol Nikolaev", Reproduction.Male, new Point(x, y), Color.White, 40f, false);
                       man.PersonEntity.HasEatenToday = true;

                   }

               }

               PlaceAnimal("entity:patrician", Reproduction.Male, new Point(202, 96), 20);

               PlaceAnimal("entity:patrician", Reproduction.Male, new Point(200, 96), 20);

               PlaceAnimal("entity:patrician", Reproduction.Male, new Point(198, 97), 20);




               //  twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(5, 10), 20, twinklerAllegiance);
               //  twinkler.MobileEntity.TestWander = true;

               //   twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(17, 10), 20, twinklerAllegiance);
               // twinkler.MobileEntity.TestWander = true;
           }*/

        private void MapEditorTest()
        {

            /*   if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                   return;*/

            The.MapUI.ZoomToMapPosition(16, 16);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(10, 10)));

        }

        /*  private void FiregrassTurnipCamp()
          {
              if (!The.Sim.LoadMap(The.The.Map.AllMaps[8]))
                  return;

              The.MapUI.ZoomToMapPosition(16, 16, MapUI.Centering.Middle);

              Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(10, 10)))
              {
                  Name = "Start",
                  Allegiance = Site.PlayerAllegiance
              };
              Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

              Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


              Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(214, 97), Color.White, 52f, false, "green2");
              ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              Body body;
              ward.Find(out body);
              body.GlobalHitpoints = 50;

              ward.Intelligence.Morale = 0.3f;

              Entity e1 = PlacePerson("Stefan Zima", Reproduction.Male, new Point(215, 95), Color.White, 52f, false, "blue1");
              e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(214, 96), Color.White, 52f, false, "blue2");
              e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              //      Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(214, 93), Color.White, 52f, false, "red1");
              //      e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              //   Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
              //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating


              Entity animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10576, 4353), 20, null, "Dark race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Vector2(10583, 4859), 20, null, "Pale race");
              animal.Intelligence.DisableAI = true;



              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(9677, 4592), 20, null, "Dark race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(9729, 4541), 20, null, "Pale race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(9776, 4568), 20, null, "Pale race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(9825, 4579), 20, null, "Pale race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(10390, 5217), 20, null, "Dark race");
              animal.Intelligence.DisableAI = true;

              animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Vector2(10803, 4862), 20, null, "Dark race");
              animal.Intelligence.DisableAI = true;


              Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:largeCamp"]);
              house1.FlipHorizontally = true;
              house1.Structure.AddBuildingToWorld(new Point(213, 95), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);


              Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
              campfire.FlipHorizontally = true;
              campfire.Structure.AddBuildingToWorld(new Point(214, 96), Common.Direction.North, expeditionOwner);
              campfire.Initialize(Site);
              campfire.Structure.ConstructionFinished(true);

              campfire.Structure.AddonTo = house1;
              house1.Structure.AddOns.Add(campfire);

              Entity item1 = new Entity(GameData.Instance.AllItemTypes["item:cement"]);
              AddColonyItem(item1, new Point(217, 96), new Vector2(-8f, 5f));

              item1 = new Entity(GameData.Instance.AllItemTypes["item:coilRifle"]);
              AddColonyItem(item1, new Point(217, 97), new Vector2(-8f, 5f));

              item1 = new Entity(GameData.Instance.AllItemTypes["item:chemicals"]);
              AddColonyItem(item1, new Point(217, 97), new Vector2(2f, -5f));


              Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
              house4.Structure.AddBuildingToWorld(new Point(237, 88), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
              house4.Structure.AddBuildingToWorld(new Point(238, 85), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
              house4.Structure.AddBuildingToWorld(new Point(235, 89), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
              house4.Structure.AddBuildingToWorld(new Point(233, 87), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
              house4.Structure.AddBuildingToWorld(new Point(237, 86), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
              house4.Structure.AddBuildingToWorld(new Point(234, 88), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
              house4.Structure.AddBuildingToWorld(new Point(228, 102), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
              house4.Structure.AddBuildingToWorld(new Point(231, 101), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

              house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
              house4.Structure.AddBuildingToWorld(new Point(226, 103), Common.Direction.North, expeditionOwner);
              house4.Initialize(Site);
              house4.Structure.ConstructionFinished(true);

          }*/



        /* private void MuckrootPatricianBattle()
         {
             if (!The.Sim.LoadMap(The.The.Map.AllMaps[8]))
                 return;

             The.MapUI.ZoomToMapPosition(126, 120, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(127, 119)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(117, 114), Color.White, 52f, false, "grey1");
             ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Body body;
             ward.Find(out body);
             body.GlobalHitpoints = 50;

             ward.Intelligence.Morale = 0.3f;

             //     Entity e1 = PlacePerson("William Shtilitchev", Reproduction.Male, new Point(119, 114), Color.White, 52f, false, "blue1");
             //     e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating  

             Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(118, 115), Color.White, 52f, false, "blue2");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(119, 115), Color.White, 52f, false, "grey2");
             e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(120, 114), Color.White, 52f, false, "ManGrey3");
             e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating


             Entity animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(126, 119), 20, null, "Dark race");
             animal.Intelligence.DisableAI = false;

             animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(127, 119), 20, null, "Pale race");
             animal.Intelligence.DisableAI = false;


             //   Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
             //   PlaceVehicle(car, new Point(126, 121), expeditionOwner);
             //    car.RenderAsModel.SetRotationAndDir(2.3f);




             //  int x = 6; // bottom left
             //  int y = 245;
               

             int x = 111;
             int y = 113;
             man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(x, y - 1), Color.White, 40f, false);

             skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
             PlaceVehicle(skimmer, new Point(x, y), expeditionOwner);
             skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);

             //set a target only the skimmer can reach: does not work with vehicles
             ScoutingJob job = new ScoutingJob(MapManager.TileToWorldPos(new Point(120, 5)), The.Sim.Site.PlayerAllegiance.ScoutingJobs);

             
             //Entity entityBeingPlaced;
             //entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:largecamp"]);          
             //entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
             //entityBeingPlaced.Initialize(Site);
             //entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
             //entityBeingPlaced.Structure.PrepareAndStartBuildingJob(expeditionOwner.OwningBody.Jobs, expeditionOwner, new Point(120, 5));
        

           
         }*/


        /*  private void Swamp()
          {
              if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                  return;

              The.MapUI.ZoomToMapPosition(191, 30, MapUI.Centering.Middle);


              Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(239, 19)))
              {
                  Name = "Start",
                  Allegiance = Site.PlayerAllegiance
              };
              Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

              Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


              Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(180, 57), Color.White, 52f, false, "green2");  //183, 81
              ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              Body body;
              ward.Find(out body);
              body.GlobalHitpoints = 50;

              skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
              PlaceVehicle(skimmer, new Point(179, 56), expeditionOwner); //185, 81
              skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);

            



              Entity e1 = PlacePerson("Armand Kinlaw", Reproduction.Male, new Point(191, 30), Color.White, 52f, false, "grey2");
              e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              Entity e2 = PlacePerson("John Kane", Reproduction.Male, new Point(192, 30), Color.White, 52f, false, "green2");
              e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating     

              //      Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(214, 93), Color.White, 52f, false, "red1");
              //      e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              //   Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
              //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating

              Allegiance.Allegiance guardianAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:forestGuardian"]);

              Entity animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(187, 29), 20, guardianAllegiance, "Dark race");
              animal.Intelligence.DisableAI = false;

              animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(197, 26), 20, guardianAllegiance, "Dark race");
              animal.Intelligence.DisableAI = false;

              //animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(198, 22), 20, null, "Dark race");
              animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(188, 31), 20, guardianAllegiance, "Dark race");
              animal.Intelligence.DisableAI = false;

          }*/

        /* private void Thunderchickens()
         {
             if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                 return;

             The.MapUI.ZoomToMapPosition(221, 160, MapUI.Centering.Middle);

             GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTiles = 12;
             GameData.Instance.AllEntityTypes["entity:human"].SensorType.RangeInTilesAtNight = 12;

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(221, 160)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(219, 160), Color.White, 52f, false, "green2");
             ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating
             ward.Intelligence.DisableAI = true;

             Body body;
             ward.Find(out body);
             body.GlobalHitpoints = 50;

             ward.Intelligence.Morale = 0.3f;

             Entity e1 = PlacePerson("William Shtilitchev", Reproduction.Male, new Point(221, 160), Color.White, 52f, false, "blue1");
             e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating
             e1.Intelligence.DisableAI = true;

             Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(222, 159), Color.White, 52f, false, "blue2");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating
             e2.Intelligence.DisableAI = true;

             //      Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(214, 93), Color.White, 52f, false, "red1");
             //      e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //   Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
             //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating


             Allegiance.Allegiance thunderchickenAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);

             Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);



             Entity animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(217, 156), 20, thunderchickenAllegiance, "Pale race");
             animal.Intelligence.DisableAI = false;

             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(219, 155), 20, thunderchickenAllegiance, "Dark race");
             animal.Intelligence.DisableAI = false;

             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(221, 154), 20, thunderchickenAllegiance, "Pale race");
             animal.Intelligence.DisableAI = false;

             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(222, 154), 20, thunderchickenAllegiance, "Pale race");
             animal.Intelligence.DisableAI = false;

             //    animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(224, 153), 20, thunderchickenAllegiance, "Pale race");
             //    animal.Intelligence.DisableAI = false;

             //   animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(222, 152), 20, thunderchickenAllegiance, "Dark race");
             //   animal.Intelligence.DisableAI = false;

             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(220, 155), 20, thunderchickenAllegiance, "Dark race");
             animal.Intelligence.DisableAI = false;


             // men as twinklers:
             e2 = PlacePerson("Twinkler 1", Reproduction.Female, new Point(222, 151), Color.White, 52f, false, "Twinkler");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             e2 = PlacePerson("Twinkler 2", Reproduction.Female, new Point(223, 150), Color.White, 52f, false, "Twinkler");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             e2 = PlacePerson("Twinkler 2", Reproduction.Female, new Point(224, 151), Color.White, 52f, false, "Twinkler");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating


            
             //Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(230, 151), 20, twinklerAllegiance, "Black race");  //twinklerAllegiance
             //twinkler.Find(out body);
             //  body.ChangeMaxHitpoints(1.0f);0;

             //animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(227, 152), 20, twinklerAllegiance, "Black race");
             //animal.Intelligence.DisableAI = false;

             //animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(229, 151), 20, twinklerAllegiance, "Black race");
             //animal.Intelligence.DisableAI = false;

             //animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(229, 150), 20, twinklerAllegiance, "Black race");
             //animal.Intelligence.DisableAI = false;

             //animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(231, 150), 20, twinklerAllegiance, "Black race");
             //animal.Intelligence.DisableAI = false;
             


             //      animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(113, 113), 20, null, "Pale race");
             //      animal.Intelligence.DisableAI = false;

         }*/


        private static void BushDragon()
        {
            /* if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                 return;
             */

            The.MapUI.ZoomToMapPosition(157, 56);

            Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(161, 60)));


            Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(183, 81), Color.White, 52f, false, "green2", expedition);  //183, 81

            Entities.Body.Body body;

            /* Entity skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
             PlaceVehicle(skimmer, new Point(185, 81), expeditionOwner); //185, 81
             skimmer.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);
             */

            AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(187, 80)); //187, 80

            //set a target only the skimmer can reach: does not work with vehicles
            //    ScoutingJob job = new ScoutingJob(MapManager.TileToWorldPos(new Point(120, 5)), UWGame.SimSide.Instance.Site.PlayerAllegiance.ScoutingJobs);



            //       Entity e1 = PlacePerson("William Shtilitchev", Reproduction.Male, new Point(159, 60), Color.White, 52f, false, "blue1");
            //       e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            //       Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(158, 61), Color.White, 52f, false, "blue2");
            //       e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating


            //      Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(214, 93), Color.White, 52f, false, "red1");
            //      e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            //   Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
            //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            Allegiances.Allegiance bushdragonAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);

            Entity animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(158, 58), 20, bushdragonAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 59), 20, bushdragonAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(160, 54), 20, bushdragonAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(161, 51), 20, bushdragonAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(157, 56), 20, bushdragonAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(151, 55), 20, bushdragonAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(149, 56), 20, bushdragonAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(150, 58), 20, bushdragonAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(150, 62), 20, bushdragonAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 55), 20, bushdragonAllegiance, "Small race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(155, 56), 20, bushdragonAllegiance, "Small race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(152, 56), 20, bushdragonAllegiance, "Small race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 57), 20, bushdragonAllegiance, "Small race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(153, 57), 20, bushdragonAllegiance, "Small race");
            animal.Intelligence.DisableAI = true;


            animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Point(172, 54), 20, null, "Dark race");
            animal.Intelligence.DisableAI = true;




            Allegiances.Allegiance birdAllegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);

            animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(167, 68), 20, birdAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(169, 69), 20, birdAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(174, 62), 20, birdAllegiance, "Dark race");
            animal.Intelligence.DisableAI = true;

            animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(145, 64), 20, birdAllegiance, "Pale race");
            animal.Intelligence.DisableAI = true;

            //      animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(113, 113), 20, null, "Pale race");
            //      animal.Intelligence.DisableAI = false;


            //   Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
            //  PlaceVehicle(car, new Point(126, 121), expeditionOwner);
            //  car.RenderAsModel.SetRotationAndDir(2.3f);

            //         Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            //      Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(224, 98), 20, twinklerAllegiance);  //twinklerAllegiance


            //     Body body;
            //      twinkler.Find(out body);
            //      body.GlobalHitpoints = 50;

            //      animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(158, 61), 20, twinklerAllegiance, "Dark race");
            //            animal.Intelligence.DisableAI = true;
            //            twinkler.Find(out body);

            //            animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(159, 60), 20, twinklerAllegiance, "Dark race");
            //            animal.Intelligence.DisableAI = true;
            //            twinkler.Find(out body);





        }

        /*private void FiregrassTwinklerNightCamp()
        {
            if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                return;

            The.MapUI.ZoomToMapPosition(214, 97, MapUI.Centering.Middle);

            Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(214, 95)))
            {
                Name = "Start",
                Allegiance = Site.PlayerAllegiance
            };
            Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

            Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


            Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(214, 97), Color.White, 52f, false, "blue1");
            ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            Body body;
            ward.Find(out body);
            body.GlobalHitpoints = 50;

            ward.Intelligence.Morale = 0.3f;

            Entity e1 = PlacePerson("Stefan Zima", Reproduction.Male, new Point(215, 95), Color.White, 52f, false, "green1");
            e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(214, 96), Color.White, 52f, false, "blue2");
            e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            //      Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(214, 93), Color.White, 52f, false, "red1");
            //      e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

            //   Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
            //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating


            Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            Entity twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(224, 98), 20, twinklerAllegiance);  //twinklerAllegiance
            twinkler.Find(out body);
            //  body.ChangeMaxHitpoints(1.0f);0;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(226, 97), 20, twinklerAllegiance, "Pale race");
            twinkler.Find(out body);
            //   body.GlobalHitpoints = 8; 

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(224, 96), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //   body.GlobalHitpoints = 5; 

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(223, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //   body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(227, 99), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //   body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(229, 99), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //   body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(227, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(222, 94), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(223, 94), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(224, 94), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(221, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(222, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(225, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(228, 95), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(228, 97), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;

            twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(229, 97), 20, twinklerAllegiance);
            twinkler.Find(out body);
            //  body.GlobalHitpoints = 8;


            Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:largeCamp"]);
            house1.FlipHorizontally = true;
            house1.Structure.AddBuildingToWorld(new Point(213, 95), Common.Direction.North, expeditionOwner);
            house1.Initialize(Site);
            house1.Structure.ConstructionFinished(true);


            Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
            campfire.FlipHorizontally = true;
            campfire.Structure.AddBuildingToWorld(new Point(214, 96), Common.Direction.North, expeditionOwner);
            campfire.Initialize(Site);
            campfire.Structure.ConstructionFinished(true);

            campfire.Structure.AddonTo = house1;
            house1.Structure.AddOns.Add(campfire);

            Entity item1 = new Entity(GameData.Instance.AllItemTypes["item:cement"]);
            AddColonyItem(item1, new Point(217, 96), new Vector2(-8f, 5f));

            item1 = new Entity(GameData.Instance.AllItemTypes["item:coilRifle"]);
            AddColonyItem(item1, new Point(217, 97), new Vector2(-8f, 5f));

            item1 = new Entity(GameData.Instance.AllItemTypes["item:chemicals"]);
            AddColonyItem(item1, new Point(217, 97), new Vector2(2f, -5f));


            Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
            house4.Structure.AddBuildingToWorld(new Point(237, 88), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
            house4.Structure.AddBuildingToWorld(new Point(238, 85), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
            house4.Structure.AddBuildingToWorld(new Point(235, 89), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
            house4.Structure.AddBuildingToWorld(new Point(233, 87), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
            house4.Structure.AddBuildingToWorld(new Point(237, 86), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
            house4.Structure.AddBuildingToWorld(new Point(234, 88), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
            house4.Structure.AddBuildingToWorld(new Point(228, 102), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
            house4.Structure.AddBuildingToWorld(new Point(231, 101), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
            house4.Structure.AddBuildingToWorld(new Point(226, 103), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

        }*/


        /* private void BirdMarsh()
         {
             if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                 return;

             The.MapUI.ZoomToMapPosition(16, 16, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(213, 95)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(241, 73), Color.White, 52f, false, "green2");  // nederst t.v. ved lejr: (214, 97)(215, 95)(214, 96)(214, 93)
             ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Body body;
             ward.Find(out body);
             body.GlobalHitpoints = 50;

             ward.Intelligence.Morale = 0.3f;

             Entity e1 = PlacePerson("Stefan Zima", Reproduction.Male, new Point(240, 75), Color.White, 52f, false, "blue1");
             e1.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Entity e2 = PlacePerson("Josie Kane", Reproduction.Female, new Point(237, 77), Color.White, 52f, false, "blue2");
             e2.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Entity e3 = PlacePerson("Renee Van Huygens", Reproduction.Female, new Point(236, 78), Color.White, 52f, false, "red1");
             e3.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //  Entity e4 = PlacePerson("Sam Eno", Reproduction.Male, new Point(222, 89), Color.White, 52f, false, "red2");
             //   e4.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             Allegiance.Allegiance birdAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);

             Entity animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10395, 3879), 20, birdAllegiance, "Pale race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10356, 3766), 20, birdAllegiance, "Dark race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10228, 3938), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10438, 3826), 20, birdAllegiance, "Black race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10489, 3919), 20, birdAllegiance, "Yellow race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10475, 3923), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10475, 3981), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10554, 3832), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10537, 3872), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10523, 3905), 20, birdAllegiance, "Dark race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10533, 3950), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10558, 3974), 20, birdAllegiance, "Black race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10592, 3794), 20, birdAllegiance, "Yellow race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10575, 3898), 20, birdAllegiance, "Dark race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10642, 3831), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10644, 3898), 20, birdAllegiance, "Black race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10702, 3923), 20, birdAllegiance, "Yellow race", false);  // 
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10736, 3711), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10717, 3815), 20, birdAllegiance, "Red race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10745, 3866), 20, birdAllegiance, "Dark race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10728, 4372), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10790, 3917), 20, birdAllegiance, "Black race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10770, 3997), 20, birdAllegiance, "Yellow race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10811, 3836), 20, birdAllegiance, "Dark race", false);
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10861, 3707), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10882, 3917), 20, birdAllegiance, "Black race", false);
             animal.Intelligence.DisableAI = true;






             Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:largecamp"]);
             house1.FlipHorizontally = true;
             house1.Structure.AddBuildingToWorld(new Point(213, 95), Common.Direction.North, expeditionOwner);
             house1.Initialize(Site);
             house1.Structure.ConstructionFinished(true);


             Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
             campfire.FlipHorizontally = true;
             campfire.Structure.AddBuildingToWorld(new Point(214, 96), Common.Direction.North, expeditionOwner);
             campfire.Initialize(Site);
             campfire.Structure.ConstructionFinished(true);

             campfire.Structure.AddonTo = house1;
             house1.Structure.AddOns.Add(campfire);

             Entity item1 = new Entity(GameData.Instance.AllItemTypes["item:cement"]);
             AddColonyItem(item1, new Point(217, 96), new Vector2(-8f, 5f));

             item1 = new Entity(GameData.Instance.AllItemTypes["item:coilRifle"]);
             AddColonyItem(item1, new Point(217, 97), new Vector2(-8f, 5f));

             item1 = new Entity(GameData.Instance.AllItemTypes["item:chemicals"]);
             AddColonyItem(item1, new Point(217, 97), new Vector2(2f, -5f));


             Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(237, 88), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(238, 85), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
             house4.Structure.AddBuildingToWorld(new Point(235, 89), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(233, 87), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(237, 86), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
             house4.Structure.AddBuildingToWorld(new Point(234, 88), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(228, 102), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(231, 101), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
             house4.Structure.AddBuildingToWorld(new Point(226, 103), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

         }*/


        /* private void MarshSunrise()
         {
             if (!The.Sim.LoadMap(The.Map.AllMaps[8]))
                 return;

             The.MapUI.ZoomToMapPosition(219, 81, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(213, 95)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

            
             //            Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(214, 97), Color.White, 52f, false, "green2");  // nederst t.v. ved lejr: (214, 97)(215, 95)(214, 96)(214, 93)
             //            ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //            Body body;
             //            ward.Find(out body);
             //            body.GlobalHitpoints = 50;

             //            ward.Intelligence.Morale = 0.3f;

              

             //   Allegiance.Allegiance birdAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);

              //  Entity animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10395, 3879), 20, birdAllegiance, "Pale race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10356, 3766), 20, birdAllegiance, "Dark race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10228, 3938), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10438, 3826), 20, birdAllegiance, "Black race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10489, 3919), 20, birdAllegiance, "Yellow race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10475, 3923), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10475, 3981), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10554, 3832), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10537, 3872), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10523, 3905), 20, birdAllegiance, "Dark race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10533, 3950), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10558, 3974), 20, birdAllegiance, "Black race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10592, 3794), 20, birdAllegiance, "Yellow race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10575, 3898), 20, birdAllegiance, "Dark race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10642, 3831), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10644, 3898), 20, birdAllegiance, "Black race", false);
              //  animal.Intelligence.DisableAI = true;

               // animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10702, 3923), 20, birdAllegiance, "Yellow race", false);  // 
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10736, 3711), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10717, 3815), 20, birdAllegiance, "Red race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10745, 3866), 20, birdAllegiance, "Dark race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10728, 4372), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10790, 3917), 20, birdAllegiance, "Black race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10770, 3997), 20, birdAllegiance, "Yellow race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10811, 3836), 20, birdAllegiance, "Dark race", false);
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10861, 3707), 20, birdAllegiance, "Dark race", false);   //uses  world location coordinates
              //  animal.Intelligence.DisableAI = true;

              //  animal = PlaceAnimal("entity:bird", Reproduction.Male, new Vector2(10882, 3917), 20, birdAllegiance, "Black race", false);
              //  animal.Intelligence.DisableAI = true;


              



             Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:largecamp"]);
             house1.FlipHorizontally = true;
             house1.Structure.AddBuildingToWorld(new Point(213, 95), Common.Direction.North, expeditionOwner);
             house1.Initialize(Site);
             house1.Structure.ConstructionFinished(true);


             Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
             campfire.FlipHorizontally = true;
             campfire.Structure.AddBuildingToWorld(new Point(214, 96), Common.Direction.North, expeditionOwner);
             campfire.Initialize(Site);
             campfire.Structure.ConstructionFinished(true);

             campfire.Structure.AddonTo = house1;
             house1.Structure.AddOns.Add(campfire);

             Entity item1 = new Entity(GameData.Instance.AllItemTypes["item:cement"]);
             AddColonyItem(item1, new Point(217, 96), new Vector2(-8f, 5f));

             item1 = new Entity(GameData.Instance.AllItemTypes["item:coilRifle"]);
             AddColonyItem(item1, new Point(217, 97), new Vector2(-8f, 5f));

             item1 = new Entity(GameData.Instance.AllItemTypes["item:chemicals"]);
             AddColonyItem(item1, new Point(217, 97), new Vector2(2f, -5f));


             Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(237, 88), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(238, 85), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
             house4.Structure.AddBuildingToWorld(new Point(235, 89), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(233, 87), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(237, 86), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
             house4.Structure.AddBuildingToWorld(new Point(234, 88), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(228, 102), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(231, 101), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
             house4.Structure.AddBuildingToWorld(new Point(226, 103), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

         }*/

        /* private void AnimIteration()
         {
             if (!The.Sim.LoadMap(The.Map.AllMaps[4]))
                 return;

             The.MapUI.ZoomToMapPosition(16, 16, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(21, 10)))
             {
                 Name = "Start",
                 Allegiance = Site.PlayerAllegiance
             };
             Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             // starts harvesting:
             //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(7, 16), Color.White, 52f, false);
             ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             //   ward.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
             // ward.Intelligence.DisableAI = true;

             //    man = PlacePerson("Sarah Connor", Reproduction.Female, new Point(10, 9), Color.White, 40f, false);
             //    man.PersonEntity.HasEatenToday = true;

             //    man = PlacePerson("Karol Nikolaev", Reproduction.Female, new Point(5, 5), Color.White, 40f, false);
             //    man.PersonEntity.HasEatenToday = true;

             Allegiance.Allegiance twinklerAllegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);





             PlaceAnimal("entity:patrician", Reproduction.Male, new Point(4, 12), 20);






             PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(15, 17), 20, twinklerAllegiance);
             //  twinkler.MobileEntity.TestWander = true;

             //   twinkler = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(17, 10), 20, twinklerAllegiance);
             // twinkler.MobileEntity.TestWander = true;
         }*/



        /* private void Animtest()
         {
             Kensei.Dev.Options.SetOption("Overlays.Jobs", false);

             // 0 is the top map in the directory... replace with 1 to load the second and so on.
             // 0: Post Settlement
             // 1: Pre Settlement
             if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                 return;

             The.MapUI.ZoomToMapPosition(194, 66, MapUI.Centering.Middle);

             Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(194, 68)))
             {
                 Name = "Start",
                 Allegiance = The.Sim.Site.PlayerAllegiance
             };
             The.Sim.Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

             // starts harvesting:
             //  expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 6;


             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


             Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
             house1.FlipHorizontally = true;
             house1.Structure.AddBuildingToWorld(new Point(193, 64), Common.Direction.North, expeditionOwner);
             house1.Initialize(Site);
             house1.Structure.ConstructionFinished(true);


             Entity campfire = new Entity(GameData.Instance.AllStructureTypes["structure:campfire"]);
             campfire.FlipHorizontally = true;
             campfire.Structure.AddBuildingToWorld(new Point(194, 65), Common.Direction.North, expeditionOwner);
             campfire.Initialize(Site);
             campfire.Structure.ConstructionFinished(true);

             campfire.Structure.AddonTo = house1;
             house1.Structure.AddOns.Add(campfire);

             // starts cooking immediately:




             //  Item item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //  AddColonyItem(item1, new Point(45, 168), new Vector2(-5f, 5f));
             //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //  AddColonyItem(item1, new Point(45, 168), new Vector2(-8f, 15f));
             //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
             //  AddColonyItem(item1, new Point(45, 168), new Vector2(0f, 9f));
               
             //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

             Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(198, 68), Color.White, 52f, false);
             ward.PersonEntity.HasEatenToday = true; // turns off cooking and eating

             ward.RenderAsModel.SetRotationAndDir(MathHelper.PiOver2);
             ward.Intelligence.DisableAI = true;

             man = PlacePerson("Sarah Connor", Reproduction.Female, new Point(199, 67), Color.White, 40f, false);
             man.PersonEntity.HasEatenToday = true;
             man.Intelligence.DisableAI = true;

             //    PlaceAnimal("entity:turnip", Reproduction.Male, new Point(199, 70), 20);

             //    PlaceAnimal("entity:patrician", Reproduction.Male, new Point(189, 68), 20);

             //    PlaceAnimal("entity:bushback", Reproduction.Male, new Point(192, 69), 5);

             //     PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(194, 68), 20);

             Entity animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(194, 70), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(193, 70), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(192, 71), 20, null, "Black race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(191, 70), 20, null, "Red race");
             animal.Intelligence.DisableAI = true;

             //    Entity boxtest = PlaceVehicle("boxtest", new Point(192, 68), null);
             //   boxtest.Renderable.RenderAsModel.SetRotationAndDir(2.3f);


             //  PlaceVehicle("meshtest", new Point(192, 68), null);


             animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Point(200, 68), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:turnip", Reproduction.Male, new Point(195, 68), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;


             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(194, 68), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(196, 71), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;



             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(190, 67), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(186, 69), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(186, 72), 20, null, "Black race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(185, 71), 20, null, "Yellow race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bird", Reproduction.Male, new Point(184, 70), 20, null, "Red race");
             animal.Intelligence.DisableAI = true;


             animal = PlaceAnimal("entity:bushdragon", Reproduction.Male, new Point(192, 69), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:bushdragon", Reproduction.Male, new Point(188, 69), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(189, 68), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(191, 67), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(193, 69), 20, null, "Black race");
             animal.Intelligence.DisableAI = true;

             //   animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(181, 69), 20, null, "Yellow race");
             //   animal.Intelligence.DisableAI = true;

             //    animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(190, 75), 20, null, "Red race");
             //    animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(190, 73), 20, null, "White race");
             animal.Intelligence.DisableAI = true;

             //    animal = PlaceAnimal("entity:patrician", Reproduction.Male, new Point(183, 74), 20, null, "Purple race");
             //   animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(200, 70), 20, null, "Pale race");
             animal.Intelligence.DisableAI = true;

             animal = PlaceAnimal("entity:forestGuardian", Reproduction.Male, new Point(198, 71), 20, null, "Dark race");
             animal.Intelligence.DisableAI = true;

             //    PlaceVehicle("skinnedtest", new Point(190, 66), null);

             //  Expedition e = Site.Expeditions[0];
             //   e.Households[0].MergeHouseholds(e.Households[1]);

             //  Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
             //  PlaceVehicle(car, new Point(33, 165), expeditionOwner); 

         }*/




        /*  private void NGPScreenshots2()
          {

              // 0 is the top map in the directory... replace with 1 to load the second and so on.
              // 0: Post Settlement
              // 1: Pre Settlement
              if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                  return;

              // -143, 171
              The.MapUI.ZoomToMapPosition(51, 236, MapUI.Centering.Middle);

              Expedition expedition = new Expedition(MapManager.TileToWorldPos(new Point(51, 236)))
              {
                  Name = "Start"
              };
              Site.PlayerAllegiance.HumanActivities.Expeditions.Add(expedition);

              // starts harvesting:
              //   expedition.Stocks.Targets[GameData.Instance.AllItemTypes["item:blackpulp"]].ProductionTarget = 9;


              Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


              Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              house1.FlipHorizontally = true;
              house1.Structure.AddBuildingToWorld(new Point(51, 236), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);

              StoreItems(4, house1, "item:tomatoes");
              StoreItems(5, house1, "item:rice");
              StoreItems(3, house1, "item:chickenmeat");


              // Item item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
              // AddColonyItem(item1, new Point(52, 235), new Vector2(-5f, 5f));
              // item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
              // AddColonyItem(item1, new Point(52, 235), new Vector2(-8f, 15f));
              // item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
              // AddColonyItem(item1, new Point(52, 235), new Vector2(0f, 9f));
              
              //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

              Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(52, 239), Color.White, 52f, false);
              man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(48, 240), Color.White, 40f, false);

              man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(50, 236), Color.White, 40f, false);
              man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(47, 241), Color.White, 40f, false);




              Expedition e = Site.GetMainExpedition();
              e.Households[0].MergeHouseholds(e.Households[1]);

              Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
              PlaceVehicle(car, new Point(50, 235), expeditionOwner);
              car.RenderAsModel.SetRotationAndDir(2.3f);

          }*/

        /*   private void Harvest()
           {
               // 0 is the top map in the directory... replace with 1 to load the second and so on.
               // 0: Post Settlement
               // 1: Pre Settlement
               if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                   return;

               The.MapUI.ZoomToMapPosition(32, 170, MapUI.Centering.Middle);

               Site.PlayerAllegiance.HumanActivities.Expeditions.Add(new Expedition(MapManager.TileToWorldPos(new Point(40, 170)))
               {
                   Name = "Start"
               });

               Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


               Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
               house1.FlipHorizontally = true;
               house1.Structure.AddBuildingToWorld(new Point(43, 168), Common.Direction.North, expeditionOwner);
               house1.Initialize(Site);
               house1.Structure.ConstructionFinished(true);

               //  Item item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //  AddColonyItem(item1, new Point(45, 168), new Vector2(-5f, 5f));
               //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //  AddColonyItem(item1, new Point(45, 168), new Vector2(-8f, 15f));
               //  item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //  AddColonyItem(item1, new Point(45, 168), new Vector2(0f, 9f));
               
               //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

               Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(34, 168), Color.White, 52f, false);
               man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(35, 168), Color.White, 40f, false);

               //  Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
               //  PlaceVehicle(car, new Point(33, 165), expeditionOwner); 

           }*/




        /*  private void Hitchhiking()
          {


              // 0 is the top map in the directory... replace with 1 to load the second and so on.
              // 0: Post Settlement
              // 1: Pre Settlement
              if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                  return;

              The.MapUI.ZoomToMapPosition(32, 170, MapUI.Centering.Middle);

              Site.PlayerAllegiance.HumanActivities.Expeditions.Add(new Expedition(MapManager.TileToWorldPos(new Point(40, 170)))
              {
                  Name = "Start"
              });

              Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


              Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              house1.FlipHorizontally = true;
              house1.Structure.AddBuildingToWorld(new Point(55, 162), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);


              house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              house1.FlipHorizontally = false;
              house1.Structure.AddBuildingToWorld(new Point(60, 168), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);

              house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              house1.FlipHorizontally = false;
              house1.Structure.AddBuildingToWorld(new Point(66, 168), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);

              house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              house1.FlipHorizontally = false;
              house1.Structure.AddBuildingToWorld(new Point(66, 174), Common.Direction.North, expeditionOwner);
              house1.Initialize(Site);
              house1.Structure.ConstructionFinished(true);


              //  Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(2, 16), Color.White, 52f, false);

              Entity ward = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(34, 168), Color.White, 52f, false);
              man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(35, 168), Color.White, 40f, false);

              man = PlacePerson("Hiroto Takahashi", Reproduction.Male, new Point(38, 164), Color.White, 40f, false);
              man = PlacePerson("Louise Girard", Reproduction.Female, new Point(37, 164), Color.White, 38f, false);


              Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
              PlaceVehicle(car, new Point(36, 167), expeditionOwner);
              car.RenderAsModel.SetRotationAndDir(2.3f);



              Point destination = new Point(50, 120);

              Entity entityBeingPlaced;
              entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
              //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
              entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
              entityBeingPlaced.Initialize(Site);
              entityBeingPlaced.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);
              entityBeingPlaced.Structure.PrepareAndStartBuildingJob( expeditionOwner, destination);



              //   Item item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
              //   AddColonyItem(item, new Point(55, 155));        
        


              //item = new Item(GameData.Instance.AllItemTypes["item:cement"]);          
              //AddColonyItem(item, new Point(5, 16));
              //item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
              //AddColonyItem(item, new Point(5, 10));
              //item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
              //AddColonyItem(item, new Point(8, 12));
             


              // return;
              //PlacePerson(new Point(10, 12), Color.White, BiologicalEntity.AgeGroup.Adult);
              //   man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(12, 12), Color.White, 40f, false);
              //  man = PlacePerson("Hannah White", Reproduction.Female, new Point(8, 13), Color.White, 45f, false);
              //  Entity ying = PlacePerson("Ying Zhao", Reproduction.Female, new Point(10, 8), Color.White, 30f, true);
              //  man = PlacePerson("Misaki Shimizu", Reproduction.Female, new Point(8, 8), Color.White, 38f, false);
              //   

             
               // skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
               //PlaceVehicle(skimmer, new Point(207, 37), ColonyOwner);
               //skimmer.RenderAsModel.SetRotationAndDir(0.8f);

               //// mAke it fly:
               //Point destination = new Point(192, 50);

               //Entity entityBeingPlaced;
               //entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
               ////   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
               //entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
               //entityBeingPlaced.Initialize(Site);
               //entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
               //entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, ColonyOwner.OwningBody.Jobs, ColonyOwner);
            
              


              // man = PlacePersonNoSkills(new Point(12, 12), Color.White, BiologicalEntity.AgeGroup.Adult);
                 // int x, y;
                 //for (int i = 0; i < 0; i++)
                 //{
                 //    item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //    x = 207; // random.Next(5, 10);
                 //    y = 37; // random.Next(5, 10);
                 //    AddColonyItem(item, new Point(x, y));
                 //}
           

                 //x = 18;
                 //for (int i = 0; i < 0; i++)
                 //{
                 //    item = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                 //    AddColonyItem(item, new Point(14, 7));
                 //} 

             
              //StoreItems(5, house1, "item:rice");           
              //StoreItems(3, house1, "item:tomatoes");
              //StoreItems(3, house1, "item:whitebread");
              //StoreItems(3, house1, "item:cowmilk");
              //StoreItems(2, house1, "item:chickenmeat");
              //StoreItems(1, house1, "item:stew");
              //StoreItems(5, house1, "item:fingerfruit");
              //StoreItems(1, house1, "item:hexapineleaves");
              //StoreItems(10, house1, "item:watercaneseeds");

              //StoreItems(2, house1, "item:advancedKnife");
              //StoreItems(1, house1, "item:coilRifle");
              //StoreItems(1, house1, "item:pistol");
              //StoreItems(1, house1, "item:basictools");
              //StoreItems(8, house1, "item:powercell");
              //StoreItems(1, house1, "item:firstaidkit");
              //StoreItems(1, house1, "item:basictools");
             


             
                         //foreach (KeyValuePair<string, AnimationController> kvp in man.RenderAsModel.AnimationControllers)
                         //{
                         //    string option = "Dev.Test anim " + kvp.Key;
                         //    animationOptions.Add(option, kvp.Key);
                         //    Kensei.Dev.Options.SetOption(option, false);
                         //    Kensei.Dev.Options.SetOptionCallback(option, TestAnims);
                         //}

          

                         //Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:woodpile"]);
                         //house4.Structure.AddBuildingToWorld(new Point(12, 16), Common.Direction.North, ColonyOwner);
                         //house4.Initialize(Site);
                         //house4.Structure.ConstructionFinished(true);

                         //Map.TileMap[5, 8].AddToFootPath(Common.Direction.East, 0.5f);
                         //Map.TileMap[6, 8].AddToFootPath(Common.Direction.West, 0.3f);
                         //Map.TileMap[6, 8].AddToFootPath(Common.Direction.East, 0.3f);
                         //Map.TileMap[7, 8].AddToFootPath(Common.Direction.West, 0.5f);
                         //Map.TileMap[7, 8].AddToFootPath(Common.Direction.East, 0.6f); 
                         //Map.TileMap[8, 8].AddToFootPath(Common.Direction.West, 0.9f);
                         //Map.TileMap[8, 8].AddToFootPath(Common.Direction.East, 1f);
            

              Entity road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(4, 16), Common.Direction.West, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.8f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(4, 16), Common.Direction.East, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.8f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(5, 16), Common.Direction.West, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.7f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(5, 16), Common.Direction.NorthEast, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.8f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(6, 15), Common.Direction.SouthWest, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.8f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(6, 15), Common.Direction.East, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.8f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(7, 15), Common.Direction.West, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.9f;

              road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
              road.Structure.AddBuildingToWorld(new Point(7, 15), Common.Direction.East, expeditionOwner);
              road.Structure.ConstructionFinished(true);
              road.TerrainPath.Value = 0.9f;
          }*/

        /* private void StoreItems(int noOfItems, Entity structure, string itemType)
         {
             Owner expeditionOwner = The.Sim.Site.GetMainExpedition().ExpeditionOwner;
             for (int i = 0; i < noOfItems; i++)
             {
                 Entity item = Item.CreateItem(GameData.Instance.AllEntityTypes[itemType]);

                 structure.Structure.StoreItem(item, expeditionOwner);
             }
         }*/


        /*private void LoadItems(int noOfItems, Entity vehicle, string itemType)
        {
            Owner ColonyOwner = Site.Expeditions[0].ExpeditionOwner;

            for (int i = 0; i < noOfItems; i++)
            {
                Item item = new Item(GameData.Instance.AllItemTypes[itemType]);
                item.ChangeOwnership(ColonyOwner); 
                vehicle.ItemStorage.LoadItem(item);               
            }
        }*/

        /* private void LandingSite()
        {
            Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

            // 0 is the top map in the directory... replace with 1 to load the second and so on.
            // 0: Post Settlement
            // 1: Pre Settlement
            if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                return;   


            //sæt time of day 0.71/0.72           
            UWGame.SimSide.Instance.Map.ZoomToMapPosition(214, 37, MapUI.Centering.Middle);

            Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
            house1.FlipHorizontally = true;
            house1.Structure.AddBuildingToWorld(new Point(210, 34), Common.Direction.North, expeditionOwner);
            house1.Initialize(Site);
            house1.Structure.ConstructionFinished(true);

            StoreItems(5, house1, "item:rice");
            StoreItems(3, house1, "item:tomatoes");
            StoreItems(3, house1, "item:whitebread");
            StoreItems(3, house1, "item:cowmilk");
            StoreItems(2, house1, "item:chickenmeat");
            StoreItems(1, house1, "item:stew");
            StoreItems(5, house1, "item:fingerfruit");
            StoreItems(1, house1, "item:hexapineleaves");
            StoreItems(10, house1, "item:watercaneseeds");

            StoreItems(2, house1, "item:advancedKnife");
            StoreItems(1, house1, "item:coilRifle");
            StoreItems(1, house1, "item:pistol");
            StoreItems(1, house1, "item:basictools");
            StoreItems(8, house1, "item:powercell");
            StoreItems(1, house1, "item:firstaidkit");
            StoreItems(1, house1, "item:basictools");





            skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
            PlaceVehicle(skimmer, new Point(207, 37), expeditionOwner);
            skimmer.RenderAsModel.SetRotationAndDir(1.8f);


            // mAke it fly:
            Point destination = new Point(192, 50); //top right: (240, 10);

            Entity entityBeingPlaced;
            entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
            //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
            entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
            entityBeingPlaced.Initialize(Site);
            entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
            entityBeingPlaced.Structure.PrepareAndStartBuildingJob(expeditionOwner.OwningBody.Jobs, expeditionOwner, null, destination);


            // 185, 71

            // Entity ward = PlacePerson("Jules Moreau", Reproduction.Male, new Point(185, 70), Color.White, 52f, false);

            man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(211, 38), Color.White, 40f, false);
            man.Intelligence.Skills[GameData.Instance.AllSkillTypes["skill:construction"]].Value = 0.4f; // set to half max

            man = PlacePerson("Camille Bernard", Reproduction.Female, new Point(210, 35), Color.White, 45f, false);
            man.Intelligence.Skills[GameData.Instance.AllSkillTypes["skill:construction"]].Value = 0.4f; // set to half max

            //  man = PlacePerson("Louise Girard", Reproduction.Female, new Point(8, 8), Color.White, 38f, false);
            man = PlacePerson("Hiroto Takahashi", Reproduction.Male, new Point(207, 37), Color.White, 40f, false); // pilot
            man.Intelligence.Skills[GameData.Instance.AllSkillTypes["skill:construction"]].Value = 1f; // set to max

            // no construction skills:
            man = PlacePerson("Santiago Ortiz", Reproduction.Male, new Point(213, 35), Color.White, 40f, true);
            man = PlacePerson("Gabrielle Carter", Reproduction.Female, new Point(208, 34), Color.White, 30f, true);



            Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:woodpile"]);
            house4.Structure.AddBuildingToWorld(new Point(190, 52), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

           
            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
            house4.Structure.AddBuildingToWorld(new Point(210, 30), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
            house4.Structure.AddBuildingToWorld(new Point(207, 32), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
            house4.Structure.AddBuildingToWorld(new Point(208, 31), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
            house4.Structure.AddBuildingToWorld(new Point(201, 33), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

            house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
            house4.Structure.AddBuildingToWorld(new Point(206, 27), Common.Direction.North, expeditionOwner);
            house4.Initialize(Site);
            house4.Structure.ConstructionFinished(true);

        }*/

        /* private void LandingSiteByNight()
         {
             Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

             // 0 is the top map in the directory... replace with 1 to load the second and so on.
             // 0: Post Settlement
             // 1: Pre Settlement
             if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                 return;

             //sæt time of day 1.0
             // sæt startskærmens koordinater.
             Map.mapX = 214 - Map.noOfTilesToDisplayHorizontally / 2;
             Map.mapY = 37 - Map.noOfTilesToDisplayVertically / 2;

             Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
             house1.FlipHorizontally = true;
             house1.Structure.AddBuildingToWorld(new Point(210, 34), Common.Direction.North, expeditionOwner);
             house1.Initialize(Site);
             house1.Structure.ConstructionFinished(true);

             StoreItems(5, house1, "item:rice");
             StoreItems(3, house1, "item:tomatoes");
             StoreItems(3, house1, "item:whitebread");
             StoreItems(3, house1, "item:cowmilk");
             StoreItems(2, house1, "item:chickenmeat");
             StoreItems(1, house1, "item:stew");
             StoreItems(5, house1, "item:fingerfruit");
             StoreItems(1, house1, "item:hexapineleaves");
             StoreItems(10, house1, "item:watercaneseeds");

             StoreItems(2, house1, "item:advancedKnife");
             StoreItems(1, house1, "item:coilRifle");
             StoreItems(1, house1, "item:weaponpistol");
             StoreItems(1, house1, "item:basictools");
             StoreItems(8, house1, "item:powercell");
             StoreItems(1, house1, "item:firstaidkit");
             StoreItems(1, house1, "item:basictools");




               // Item item1 = new Item(GameData.Instance.AllItemTypes["item:coilRifle"]);
               //AddColonyItem(item1, new Point(214, 36), new Vector2(6f, 2f));
               

             // 185, 71

             // Entity ward = PlacePerson("Jules Moreau", Reproduction.Male, new Point(185, 70), Color.White, 52f, false);

             man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(180, 38), Color.White, 40f, false);
             man = PlacePerson("Camille Bernard", Reproduction.Female, new Point(180, 35), Color.White, 45f, false);
             Entity ying = PlacePerson("Gabrielle Carter", Reproduction.Female, new Point(170, 34), Color.White, 30f, true);
             //  man = PlacePerson("Louise Girard", Reproduction.Female, new Point(8, 8), Color.White, 38f, false);
             man = PlacePerson("Hiroto Takahashi", Reproduction.Male, new Point(180, 35), Color.White, 40f, false);

             man = PlacePerson("Santiago Ortiz", Reproduction.Male, new Point(179, 35), Color.White, 40f, false);




             Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:woodpile"]);
             house4.Structure.AddBuildingToWorld(new Point(190, 52), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

                // item1 = new Item(GameData.Instance.AllItemTypes["item:sentry"]);
                //AddColonyItem(item1, new Point(180, 35), new Vector2(6f, -2f));
                //item1 = new Item(GameData.Instance.AllItemTypes["item:sentry"]);
                //AddColonyItem(item1, new Point(180, 35), new Vector2(-6f, 2f));

                //item1 = new Item(GameData.Instance.AllItemTypes["item:sentry"]);
                //AddColonyItem(item1, new Point(180, 33), new Vector2(0f, 2f));

                //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                //AddColonyItem(item1, new Point(210, 37), new Vector2(6f, -2f));
                //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                //AddColonyItem(item1, new Point(210, 37), new Vector2(-4f, 7f));
                
             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
             house4.Structure.AddBuildingToWorld(new Point(210, 30), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
             house4.Structure.AddBuildingToWorld(new Point(207, 32), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(208, 31), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
             house4.Structure.AddBuildingToWorld(new Point(201, 33), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

             house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
             house4.Structure.AddBuildingToWorld(new Point(206, 27), Common.Direction.North, expeditionOwner);
             house4.Initialize(Site);
             house4.Structure.ConstructionFinished(true);

         }*/

        /*    private void DustStormScene()
            {
                Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

                int x = 179;
                int y = 31;


                UWGame.SimSide.Instance.Map.ZoomToMapPosition(x, y, MapUI.Centering.Middle);


                man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(x, y), Color.White, 40f, false);

                skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
                PlaceVehicle(skimmer, new Point(x + 5, y), expeditionOwner);
                skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);


                //   UWGame.SimSide.Instance.ParticleManager.AddDust(Aircraft, Aircraft.Location, dustIntensity, GameConstants.SkimmerDustScale);

                ParticleManager.AddDustStormPlume(new Point(x + 1, y - 8), new Vector2(0, 0), 2.2f, 0.3f);
                ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 1.5f, 0.2f);
                ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 1.5f, 0.2f);
                ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 1.7f, 0.3f);
                ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 2f, 0.2f);
                ParticleManager.AddDustStormPlume(new Point(x - 2, y + 4), new Vector2(0, 0), 1.5f, 0.2f);
                ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 1.5f, 0.5f);
                ParticleManager.AddDustStormPlume(new Point(x - 3, y + 8), new Vector2(0, 0), 1.8f, 0.2f);



                // mAke it fly:
                Point destination = new Point(80, 177); //top right: (240, 10);

                Entity entityBeingPlaced;
                entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
                //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
                entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
                entityBeingPlaced.Initialize(Site);
                entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
                entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, expeditionOwner.OwningBody.Jobs, expeditionOwner);

            }*/


        /*    private void FlyScene1()
            {
                Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

                // 0 is the top map in the directory... replace with 1 to load the second and so on.
                // 0: Post Settlement
                // 1: Pre Settlement
                if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                    return;   

                // teaser hack: make the skimmer fly higher over the plateaus to avoid "clipping":
                GameData.Instance.AllEntityTypes["entity:skimmer"].VehicleType.Aircraft.CruiseAltitude = 300f;

                int x = 6; // bottom left
                int y = 245;

                man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(x, y - 1), Color.White, 40f, false);

                skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
                PlaceVehicle(skimmer, new Point(x, y), expeditionOwner);
                skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);

            

                // mAke it fly:
                Point destination = new Point(87, 185); //(80, 177) top right: (240, 10);

                Entity entityBeingPlaced;
                entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
                //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
                entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
                entityBeingPlaced.Initialize(Site);
                entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
                entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, expeditionOwner.OwningBody.Jobs, expeditionOwner);

            }*/

        /*   private void FlyScene2()
           {
               Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;
               if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                   return;   

               //Weather.Instance.CloudCover = 0f;


               Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:woodpile"]);
               house4.Structure.AddBuildingToWorld(new Point(190, 52), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
               house4.Structure.AddBuildingToWorld(new Point(118, 126), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(127, 126), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(125, 120), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(111, 128), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
               house4.Structure.AddBuildingToWorld(new Point(113, 132), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(115, 128), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(116, 127), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
               house4.Structure.AddBuildingToWorld(new Point(113, 122), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(112, 123), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);


               // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
               // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
               ParticleManager.AddSmokePlume(new Point(99, 150), new Vector2(0, 0), null, null);
               ParticleManager.AddSmokePlume(new Point(99, 150), new Vector2(0, -30), 0.5f, 0.5f);
               ParticleManager.AddSmokePlume(new Point(94, 140), new Vector2(0, 0), 0.7f, 0.2f);
               ParticleManager.AddSmokePlume(new Point(94, 141), new Vector2(0, 0), 0.2f, 0.1f);
               ParticleManager.AddSmokePlume(new Point(95, 141), new Vector2(0, 0), 0.2f, 0.8f);
               ParticleManager.AddSmokePlume(new Point(99, 137), new Vector2(0, 0), 0.6f, 0.8f);


               int x = 72;
               int y = 170;

               man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(x, y - 1), Color.White, 40f, false);

               skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
               PlaceVehicle(skimmer, new Point(x, y), expeditionOwner);
               skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);



               // mAke it fly:
               Point destination = new Point(134, 113);

               Entity entityBeingPlaced;
               entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
               //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
               entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
               entityBeingPlaced.Initialize(Site);
               entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
               entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, expeditionOwner.OwningBody.Jobs, expeditionOwner);



           }*/

        /*     private void Precolony()
             {
                 Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;

                 // 0 is the top map in the directory... replace with 1 to load the second and so on.
                 // 0: Post Settlement
                 // 1: Pre Settlement
            if (!The.Sim.LoadMap(The.Map.AllMaps[1]))
                return;   

                 int x = 16; // bottom left
                 int y = 240;

                 man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(x, 240 - 1), Color.White, 40f, false);

                 skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
                 PlaceVehicle(skimmer, new Point(x, y), expeditionOwner);
                 skimmer.RenderAsModel.SetRotationAndDir(-0.8f); //(-0.8f);(0.8f);(4.8f);



                 // mAke it fly:
                 Point destination = new Point(80, 177); //top right: (240, 10);

                 Entity entityBeingPlaced;
                 entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);
                 //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 
                 entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
                 entityBeingPlaced.Initialize(Site);
                 entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
                 entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, expeditionOwner.OwningBody.Jobs, expeditionOwner);

             }*/



        /*   private void Postcolony()
           {
               // 0 is the top map in the directory... replace with 1 to load the second and so on.
               // 0: Post Settlement
               // 1: Pre Settlement
               if (!The.Sim.LoadMap(The.Map.AllMaps[0]))
                   return;

               // must be done after map is loaded...
               Site.PlayerAllegiance.HumanActivities.Expeditions.Add(new Expedition(MapManager.TileToWorldPos(new Point(215, 33)))
               {
                   Name = "Start"
               });

               Owner expeditionOwner = Site.GetMainExpedition().ExpeditionOwner;


               //  PrePlaceRoad();

               // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
               // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
                 // ParticleManager.AddSmokePlume(new Point(98, 149), new Vector2(0, 0), null, null);
                 //ParticleManager.AddSmokePlume(new Point(98, 149), new Vector2(0, -30), 0.5f, 0.5f);
                 //ParticleManager.AddSmokePlume(new Point(94, 140), new Vector2(0, 0), 0.7f, 0.2f);
                 //ParticleManager.AddSmokePlume(new Point(92, 138), new Vector2(0, 0), 0.2f, 0.1f);
                 //ParticleManager.AddSmokePlume(new Point(97, 139), new Vector2(0, 0), 0.2f, 0.8f); //these don´t show up..? only play once I think
                 //ParticleManager.AddSmokePlume(new Point(96, 138), new Vector2(0, 0), 0.6f, 0.8f); //these don´t show up..? only play once I think
               

               The.MapUI.ZoomToMapPosition(214, 37, MapUI.Centering.Middle);


               // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile.
               // 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
               ParticleManager.AddSmokePlume(new Point(99, 150), new Vector2(0, 0), null, null);
               ParticleManager.AddSmokePlume(new Point(99, 150), new Vector2(0, -30), 0.5f, 0.5f);
               ParticleManager.AddSmokePlume(new Point(94, 140), new Vector2(0, 0), 0.7f, 0.2f);
               ParticleManager.AddSmokePlume(new Point(94, 141), new Vector2(0, 0), 0.2f, 0.1f);
               ParticleManager.AddSmokePlume(new Point(95, 141), new Vector2(0, 0), 0.2f, 0.8f);
               ParticleManager.AddSmokePlume(new Point(99, 137), new Vector2(0, 0), 0.6f, 0.8f);


               float timeBetweenPuffsFactor = 2f;

               int x = 175;
               int y = 30;

             
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 8), new Vector2(0, 0), 3.9f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 3.2f, 0.05f * timeBetweenPuffsFactor);
           ////    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 3.7f, 0.03f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 4f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 4), new Vector2(0, 0), 3f, 0.1f * timeBetweenPuffsFactor);
           ////    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 4.5f, 0.05f);


           //    x += 4;

           //    ParticleManager.AddDustStormPlume(new Point(x + 1, y - 10), new Vector2(0, 0), 4f, 0.1f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 8), new Vector2(0, 0), 3.5f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 3), new Vector2(0, 0), 3f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 3f, 0.03f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 4f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 3), new Vector2(0, 0), 3.5f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 3.5f, 0.05f * timeBetweenPuffsFactor);
           ////    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 9), new Vector2(0, 0), 3.8f, 0.02f);

            
           //    x += 3;

           //    ParticleManager.AddDustStormPlume(new Point(x + 1, y - 8), new Vector2(0, 0), 3.2f, 0.03f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 4.5f, 0.05f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 4.5f, 0.1f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 3.7f, 0.03f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 4f, 0.2f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 4), new Vector2(0, 0), 3.5f, 0.1f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 4.5f, 0.05f * timeBetweenPuffsFactor);
           // //   ParticleManager.AddDustStormPlume(new Point(x - 3, y + 8), new Vector2(0, 0), 3.8f, 0.02f);

           //    x += 4;
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 4.9f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 3.2f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 3.7f, 0.03f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 3f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 4), new Vector2(0, 0), 3f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 3.5f, 0.05f * timeBetweenPuffsFactor);

           //    x += 2;

           //    ParticleManager.AddDustStormPlume(new Point(x, y - 5), new Vector2(0, 0), 3f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 3.3f, 0.03f * timeBetweenPuffsFactor);

           //    x += 2;
           // //   ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 4.8f, 0.02f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 4.2f, 0.02f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 3f, 0.03f * timeBetweenPuffsFactor);
           //    ParticleManager.AddDustStormPlume(new Point(x, y + 3), new Vector2(0, 0), 4.5f, 0.02f * timeBetweenPuffsFactor);
 
           //    x += 0;
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 1.9f, 0.2f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 3.2f, 0.02f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 2.7f, 0.03f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 4f, 0.02f);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 4), new Vector2(0, 0), 3f, 0.2f);
           //    ParticleManager.AddDustStormPlume(new Point(x - 2, y + 7), new Vector2(0, 0), 4.5f, 0.05f);

           //    x -= 2;
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 6), new Vector2(0, 0), 4.8f, 0.02f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 2.2f, 0.2f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y), new Vector2(0, 0), 2f, 0.03f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y + 3), new Vector2(0, 0), 3.5f, 0.02f);
           
           //    x -= 12;
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 8), new Vector2(0, 0), 3.8f, 0.05f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 4), new Vector2(0, 0), 4.2f, 0.08f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 2), new Vector2(0, 0), 5f, 0.1f);
           //    ParticleManager.AddDustStormPlume(new Point(x, y - 7), new Vector2(0, 0), 1.5f, 0.07f);
 



                   // man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(235, 43), Color.White, 40f, false);
                   //man = PlacePerson("Chloe Williams", Reproduction.Female, new Point(208, 28), Color.White, 45f, true);
                   //Entity ying = PlacePerson("Alyssa Green", Reproduction.Female, new Point(206, 42), Color.White, 30f, true);
                   //man = PlacePerson("Imani Harris", Reproduction.Female, new Point(212, 30), Color.White, 38f, true);
                   //man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(213, 30), Color.White, 40f, true);
                   //man = PlacePerson("Dejan Radovic", Reproduction.Male, new Point(215, 33), Color.White, 40f, true);
                   //man = PlacePerson("Antonio Rodriguez", Reproduction.Male, new Point(215, 30), Color.White, 40f, true);
                   //man.RenderAsModel.SetRotationAndDir(0.8f);

                   //man = PlacePerson("Gabrielle Carter", Reproduction.Female, new Point(220, 28), Color.White, 38f, true);
                   //man = PlacePerson("Santiago Ortiz", Reproduction.Male, new Point(213, 28), Color.White, 40f, true);
                   ////man = PlacePerson("Diego Sanchez", Reproduction.Male, new Point(213, 27), Color.White, 40f, true);
                   ////man = PlacePerson("Antonio Rodriguez", Reproduction.Male, new Point(214, 31), Color.White, 40f, true);
                   //man = PlacePerson("Camille Bernard", Reproduction.Female, new Point(216, 28), Color.White, 38f, true);
                   ////man = PlacePerson("Wei Guo", Reproduction.Male, new Point(215, 20), Color.White, 40f, true);
                   ////man = PlacePerson("Ming Zhao", Reproduction.Male, new Point(214, 20), Color.White, 40f, true);
                   //man = PlacePerson("Hiroto Takahashi", Reproduction.Male, new Point(205, 32), Color.White, 40f, true);
                   //man = PlacePerson("Louise Girard", Reproduction.Female, new Point(214, 29), Color.White, 38f, true);
                   ////man = PlacePerson("Yuto Yoshida", Reproduction.Male, new Point(201, 20), Color.White, 40f, true);
                   ////man = PlacePerson("Peng Wu", Reproduction.Male, new Point(205, 10), Color.White, 40f, true);
                   //man = PlacePerson("Antonio Rodriguez", Reproduction.Male, new Point(218, 31), Color.White, 40f, true);

                 

               man = PlacePerson("Jules Moreau", Reproduction.Male, new Point(210, 35), Color.White, 40f, false);
               man = PlacePerson("Chloe Williams", Reproduction.Female, new Point(208, 28), Color.White, 45f, false);
               Entity ying = PlacePerson("Alyssa Green", Reproduction.Female, new Point(206, 42), Color.White, 30f, false);
               man = PlacePerson("Imani Harris", Reproduction.Female, new Point(212, 30), Color.White, 38f, false);
               man = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(213, 30), Color.White, 40f, false);

               man = PlacePerson("Antonio Rodriguez", Reproduction.Male, new Point(217, 32), Color.White, 40f, false);
               man.RenderAsModel.SetRotationAndDir(0.8f);


               man = PlacePerson("Santiago Ortiz", Reproduction.Male, new Point(213, 28), Color.White, 40f, false);

               man = PlacePerson("Camille Bernard", Reproduction.Female, new Point(216, 29), Color.White, 38f, false);
               //man = PlacePerson("Wei Guo", Reproduction.Male, new Point(215, 20), Color.White, 40f, true);
               //man = PlacePerson("Ming Zhao", Reproduction.Male, new Point(214, 20), Color.White, 40f, true);
               man = PlacePerson("Hiroto Takahashi", Reproduction.Male, new Point(216, 36), Color.White, 40f, false);
               man = PlacePerson("Louise Girard", Reproduction.Female, new Point(207, 38), Color.White, 38f, false);
               //man = PlacePerson("Yuto Yoshida", Reproduction.Male, new Point(201, 20), Color.White, 40f, true);
               //man = PlacePerson("Peng Wu", Reproduction.Male, new Point(205, 10), Color.White, 40f, true);

               man = PlacePerson("Dejan Radovic", Reproduction.Male, new Point(200, 35), Color.White, 40f, false);

               man = PlacePerson("Gabrielle Carter", Reproduction.Female, new Point(220, 28), Color.White, 38f, false);

               Random random = new Random();
               //int skill = 70;


                // Entity bike = new Entity(GameData.Instance.AllEntityTypes["entity:quad"]);
                //PlaceVehicle(bike, new Point(15, 6), ColonyOwner);
              

               Entity car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
               PlaceVehicle(car, new Point(217, 36), expeditionOwner);
               car.RenderAsModel.SetRotationAndDir(4f);


                 //car = new Entity(GameData.Instance.AllEntityTypes["entity:utilityvehicle"]);
                 //PlaceVehicle(car, new Point(207, 35), expeditionOwner);
                 //car.RenderAsModel.SetRotationAndDir(1.5f);
                 //car.RenderAsModel.CustomColor0 = new Vector3(239f, 237f, 196f) / 255f; // cream
                 //car.RenderAsModel.CustomColor1 = new Vector3(42f, 119f, 40f) / 255f; // green
                 //car.RenderAsModel.CustomColor2 = new Vector3(0f, 0f, 0f) / 255f;
                 //car.RenderAsModel.CustomColor3 = new Vector3(0f, 0f, 0f) / 255f;
               


                 // skimmer = new Entity(GameData.Instance.AllEntityTypes["entity:skimmer"]);
                 //PlaceVehicle(skimmer, new Point(206, 30), expeditionOwner);
                 //skimmer.RenderAsModel.SetRotationAndDir(3.6f); 


               // mAke it fly:
                // Point destination = new Point(160, 24); //top right: (240, 10);

                //Entity entityBeingPlaced;
                //entityBeingPlaced = new Entity(GameData.Instance.AllStructureTypes["structure:camplarge"]);          
                //entityBeingPlaced.Structure.State = global::UWGame.SimSide.Buildings.States.BeingPlaced;
                //entityBeingPlaced.Initialize(Site);
                //entityBeingPlaced.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);
                //entityBeingPlaced.Structure.PrepareAndStartBuildingJob(destination, Common.Direction.North, expeditionOwner.OwningBody.Jobs, expeditionOwner);
              


               // return; // GOOD


                    //  Color color = Color.White;
                    //PersonEntity p1 = PlacePerson(new Point(18, 6), Color.Red, BiologicalEntity.AgeGroup.Adult);
     


               //   p1.Location = p1.Location + new Vector3(15f, 0f, 0f);


                    //bike.DrivenBy = p1;
                    //p1.DrivingVehicle = bike;
            
                    //Camp camp = new Camp();
                    //camp.PrepareAndStartBuildingJob(new Point(6, 6), Common.Direction.North, ColonyOwner.OwningBody.Jobs, ColonyOwner);

                    //camp = new Camp();
                    //camp.PrepareAndStartBuildingJob(new Point(10, 2), Common.Direction.North, ColonyOwner.OwningBody.Jobs, ColonyOwner);
            
                //   PersonEntity p3 = PlacePerson(new Point(6, 2), Color.White, PersonEntity.AgeGroup.Adult);
                //PersonEntity p4 = PlacePerson(new Point(6, 2), Color.White, PersonEntity.AgeGroup.Adult);
                //     Households[0].MergeHouseholds(Households[Households.Count - 1]);
  
                //       p1.Name = "Owner";

                //      p1.Mate = p3;
                //      p3.Mate = p1;
            
 
                //     PersonEntity p4 = PlacePerson(new Point(6, 2), Color.White, PersonEntity.AgeGroup.Adult);

                //      PersonEntity p2 = PlacePerson(new Point(5, 3), Color.White, PersonEntity.AgeGroup.Adult);

                //     PersonEntity p5 = PlacePerson(new Point(8, 7), Color.White, PersonEntity.AgeGroup.Adult);
                //      PersonEntity p6 = PlacePerson(new Point(14, 8), Color.White, PersonEntity.AgeGroup.Adult);
                //     PersonEntity p7 = PlacePerson(new Point(10, 10), Color.White, PersonEntity.AgeGroup.Adult);
                //     PersonEntity p8 = PlacePerson(new Point(9, 2), Color.White, PersonEntity.AgeGroup.Adult);
                //      PersonEntity p9 = PlacePerson(new Point(7, 1), Color.White, PersonEntity.AgeGroup.Adult);
            
                //      PersonEntity p10 = PlacePerson(new Point(12, 4), Color.White, PersonEntity.AgeGroup.Adult);
                //      PersonEntity p11 = PlacePerson(new Point(14, 3), Color.White, PersonEntity.AgeGroup.Adult);    
                //      PersonEntity p12 = PlacePerson(new Point(14, 4), Color.White, PersonEntity.AgeGroup.Adult);
                //      PersonEntity p13 = PlacePerson(new Point(20, 5), Color.White, PersonEntity.AgeGroup.Adult); 




                //      skimmer = new Skimmer();
                //   skimmer.AddVehicleToWorld(new Point(12, 6), ColonyOwner); //12, 6), ColonyOwner);
                //   skimmer.Rotation = 0.6f; //0.6f; //2.2f;
                //   skimmer.Direction = new Vector3(
                //       (float)Math.Cos(skimmer.Rotation),
                //       (float)Math.Sin(skimmer.Rotation), 0);

                //   skimmer.DrivenBy = p1;
                //   p1.DrivingVehicle = skimmer;

                // skimmer.Direction.X = 0f;
                //   skimmer.Direction.Y = 1f;
                //   skimmer.Direction.Z = 1f;
                //   skimmer.Direction.Normalize();
               




                       //PersonEntity p3 = PlacePerson(new Point(18, 4), Color.White, PersonEntity.AgeGroup.YoungAdult);
                       //Households[0].MergeHouseholds(Households[Households.Count - 1]);
                       //Households[0].MergeHouseholds(Households[Households.Count - 1]);
            
          
          
                       //p1.BiologicalChildren.Add(p2);
                       //p1.BiologicalChildren.Add(p3);
         
                  // huntingrifle rifle = new HuntingRifle();
                  //rifle.AddItemToWorld(new Point(4,4), p1.PrivateOwnership);
                  //rifle = new HuntingRifle();
                  //rifle.AddItemToWorld(new Point(4, 4), p1.PrivateOwnership);
                  //rifle = new HuntingRifle();
                  //rifle.AddItemToWorld(new Point(4, 4), p1.PrivateOwnership);
                

               //p1.Household.Ownership); //p1.PrivateOwnership);
                 //  bike.Location  = bike.Location + new Vector3(-8f, 0f, 0f);
                   


               //    skimmer.Location.Z = 200f;

                 // skimmer.Direction.X = 0f;
                 //skimmer.Direction.Y = 1f;
                 //skimmer.Direction.Z = 1f;
                 //skimmer.Direction.Normalize();


                 //skimmer = new Skimmer();
                 //skimmer.AddVehicleToWorld(new Point(4, 4), ColonyOwner);
             


               //        Truck truck = new Truck();
               //        truck.AddVehicleToWorld(new Point(6, 0), ColonyOwner); 

               //ColonyOwner.OwningBody.Vehicles.Add(truck);

               //     AI.Goals.GoalEvaluator.AddGoalIfNotPresent(Entities[1], new AI.Goals.GoalTakeFive(Entities[1], Globals.Instance.Random.Next(1, 4)));           

               //    PlacePerson(new Point(2, 2), Color.White);
                  // PlacePerson(new Point(4, 2), Color.White);

                  //PlacePerson(new Point(4, 7), Color.White);
               



                     // truck = new Truck(this);
                     //truck.MapPosition = new Point(8, 7);
                     //ColonyVehicles.Add(truck);
                     //map.TileMap[8, 7].AddVehicle(truck);
                 
               //     color = new Color((byte)random.Next(150, 255), (byte)random.Next(150, 255), (byte)random.Next(150, 255));

               for (int i = 0; i < 3; i++)
               {
                   // skill = 5;
                   x = random.Next(5, 7);
                   y = random.Next(5, 7);
               }

                   // Structure smelter = new Structure(AllStructureTypes["structure:smelter"]);
                   //smelter.AddBuildingToWorld(new Point(6, 5), Common.Direction.North, ColonyOwner);
                   //smelter.ConstructionFinished();
                   //smelter = new Structure(AllStructureTypes["structure:smelter"]);
                   //smelter.AddBuildingToWorld(new Point(6, 10), Common.Direction.North, ColonyOwner);
                   //smelter.ConstructionFinished();
                

                   //Structure house2 = new Structure(AllStructureTypes["structure:house2"]);
                   //house2.AddBuildingToWorld(new Point(20, 4), Common.Direction.North, p1.Household.Ownership); 
                   //house2.ConstructionFinished(); 

               Entity house1 = new Entity(GameData.Instance.AllStructureTypes["structure:house1"]);
               house1.FlipHorizontally = true;
               house1.Structure.AddBuildingToWorld(new Point(15, 41), Common.Direction.North, man.PersonEntity.Household.Ownership);
               house1.Initialize(Site);
               house1.Structure.ConstructionFinished(true);

             
               //StoreItems(5, house1, "item:rice");
               //StoreItems(3, house1, "item:tomatoes");
               //StoreItems(3, house1, "item:whitebread");
               //StoreItems(3, house1, "item:cowmilk");
               //StoreItems(2, house1, "item:chickenmeat");
               //StoreItems(1, house1, "item:stew");
               //StoreItems(5, house1, "item:fingerfruit");
               //StoreItems(1, house1, "item:hexapineleaves");
               //StoreItems(10, house1, "item:watercaneseeds");

               //StoreItems(2, house1, "item:advancedKnife");
               //StoreItems(1, house1, "item:coilRifle");
               //StoreItems(1, house1, "item:pistol");
               //StoreItems(1, house1, "item:basictools");
               //StoreItems(8, house1, "item:powercell");
               //StoreItems(1, house1, "item:firstaidkit");
               //StoreItems(1, house1, "item:basictools");
            

               Entity house2 = new Entity(GameData.Instance.AllStructureTypes["structure:house2"]);
               house2.FlipHorizontally = true;
               house2.Structure.AddBuildingToWorld(new Point(14, 42), Common.Direction.North, expeditionOwner);
               house2.Initialize(Site);
               house2.Structure.ConstructionFinished(true);

               Entity house2b = new Entity(GameData.Instance.AllStructureTypes["structure:house2"]); // house2b
               house2b.Structure.AddBuildingToWorld(new Point(15, 44), Common.Direction.North, expeditionOwner);
               house2b.Initialize(Site);
               house2b.Structure.ConstructionFinished(true);


               Entity house3 = new Entity(GameData.Instance.AllStructureTypes["structure:house3"]);
               house3.FlipHorizontally = true;
               house3.Structure.AddBuildingToWorld(new Point(20, 44), Common.Direction.North, expeditionOwner);
               house3.Initialize(Site);
               house3.Structure.ConstructionFinished(true);

               Entity house4 = new Entity(GameData.Instance.AllStructureTypes["structure:house3"]);
               house4.Structure.AddBuildingToWorld(new Point(20, 8), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:house1c"]);
               house4.Structure.AddBuildingToWorld(new Point(212, 12), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               Entity house1b = new Entity(GameData.Instance.AllStructureTypes["structure:house1b"]);
               house1b.Structure.AddBuildingToWorld(new Point(213, 30), Common.Direction.North, expeditionOwner);
               house1b.Initialize(Site);
               house1b.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:house2c"]);
               house4.Structure.AddBuildingToWorld(new Point(20, 23), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:house3c"]);
               house4.Structure.AddBuildingToWorld(new Point(210, 26), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

                  // house4 = new Entity(GameData.Instance.AllStructureTypes["structure:house2construction"]);
                  //house4.Structure.AddBuildingToWorld(new Point(207, 29), Common.Direction.North, expeditionOwner);
                  //house4.Initialize(Site);
                  //house4.Structure.ConstructionFinished(true); 

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:housebunker"]);
               house4.Structure.AddBuildingToWorld(new Point(207, 38), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:houseveranda"]);
               house4.Structure.AddBuildingToWorld(new Point(210, 32), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:houseexplorer"]);
               house4.Structure.AddBuildingToWorld(new Point(200, 34), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:doghouse"]);
               house4.Structure.AddBuildingToWorld(new Point(210, 34), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:greenhouse"]);
               house4.Structure.AddBuildingToWorld(new Point(211, 34), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:rabbitcages"]);
               house4.Structure.AddBuildingToWorld(new Point(214, 34), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               // house4 = new Entity(GameData.Instance.AllStructureTypes["structure:shed"]);
               //house4.Structure.AddBuildingToWorld(new Point(200, 17), Common.Direction.North, ColonyOwner);
               //house4.Initialize(Site);
               //house4.Structure.ConstructionFinished(true); 

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:shed b"]);
               house4.Structure.AddBuildingToWorld(new Point(209, 27), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);


               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:japanesegarden"]);
               house4.Structure.AddBuildingToWorld(new Point(205, 37), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:woodpile"]);
               house4.Structure.AddBuildingToWorld(new Point(216, 31), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:scrapheap"]);
               house4.Structure.AddBuildingToWorld(new Point(214, 35), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:roboworkshop"]);
               house4.Structure.AddBuildingToWorld(new Point(211, 35), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:greenhouse2"]);
               house4.Structure.AddBuildingToWorld(new Point(204, 34), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);




               house1 = new Entity(GameData.Instance.AllStructureTypes["structure:house3c"]);
               house1.Structure.AddBuildingToWorld(new Point(23, 229), Common.Direction.North, expeditionOwner);
               house1.Initialize(Site);
               house1.Structure.ConstructionFinished(true);

             
               //StoreItems(5, house1, "item:rice");
               //StoreItems(3, house1, "item:tomatoes");
               //StoreItems(3, house1, "item:whitebread");
               //StoreItems(3, house1, "item:cowmilk");
               //StoreItems(2, house1, "item:chickenmeat");
               //StoreItems(1, house1, "item:stew");
               //StoreItems(5, house1, "item:fingerfruit");
               //StoreItems(1, house1, "item:hexapineleaves");
               //StoreItems(10, house1, "item:watercaneseeds");

               //StoreItems(2, house1, "item:advancedKnife");
               //StoreItems(1, house1, "item:coilRifle");
               //StoreItems(1, house1, "item:weaponpistol");
               //StoreItems(1, house1, "item:basictools");
               //StoreItems(8, house1, "item:powercell");
               //StoreItems(1, house1, "item:firstaidkit");
               //StoreItems(1, house1, "item:basictools");
           

               //Item item33 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //AddColonyItem(item33, new Point(28, 229), new Vector2(-5f, 5f));
               //item33 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //AddColonyItem(item33, new Point(28, 229), new Vector2(-8f, 15f));
               //item33 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
               //AddColonyItem(item33, new Point(28, 229), new Vector2(0f, 9f));

               //item33 = new Item(GameData.Instance.AllItemTypes["item:sentry"]);
               //AddColonyItem(item33, new Point(29, 229), new Vector2(-6f, 2f));
 





               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
               house4.Structure.AddBuildingToWorld(new Point(127, 126), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(135, 129), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(123, 135), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
               house4.Structure.AddBuildingToWorld(new Point(102, 130), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(104, 123), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(113, 134), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
               house4.Structure.AddBuildingToWorld(new Point(110, 127), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
               house4.Structure.AddBuildingToWorld(new Point(115, 131), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(118, 121), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungDark"]);
               house4.Structure.AddBuildingToWorld(new Point(121, 119), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownDark"]);
               house4.Structure.AddBuildingToWorld(new Point(122, 117), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(124, 108), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareGrownLit"]);
               house4.Structure.AddBuildingToWorld(new Point(123, 106), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);

               house4 = new Entity(GameData.Instance.AllStructureTypes["structure:starsnareYoungLit"]);
               house4.Structure.AddBuildingToWorld(new Point(117, 120), Common.Direction.North, expeditionOwner);
               house4.Initialize(Site);
               house4.Structure.ConstructionFinished(true);








       // for (int yy = 0; yy < 34; yy++)
       //{ 
               int yy = 7;

                 
              

               Entity rocks1 = new Entity(GameData.Instance.AllEntityTypes["terrain:diagonalrocks5"]);
               // rocks1.PlaceEntityOnTile(30, yy);
               Point tilePos = new Point(30, yy);
               rocks1.PlaceGroundFeature(tilePos, Common.Direction.SouthEast, MapManager.TileToWorldPos(tilePos));
       //  }

               Entity road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(209, 35), Common.Direction.NorthWest, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(208, 32), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(208, 33), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(208, 33), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(208, 34), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(208, 34), Common.Direction.SouthEast, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(209, 35), Common.Direction.SouthEast, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(210, 36), Common.Direction.NorthWest, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(210, 36), Common.Direction.SouthEast, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(211, 37), Common.Direction.NorthWest, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(211, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(212, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(212, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(213, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(213, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(214, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(214, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(215, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(215, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(216, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(216, 37), Common.Direction.East, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 37), Common.Direction.West, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(216, 37), Common.Direction.NorthEast, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 36), Common.Direction.SouthWest, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 36), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 35), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 35), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 34), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 34), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 33), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 33), Common.Direction.North, expeditionOwner);
               road.Structure.ConstructionFinished(true);
               road = new Entity(GameData.Instance.AllStructureTypes["structure:gravelroad"]);
               road.Structure.AddBuildingToWorld(new Point(217, 32), Common.Direction.South, expeditionOwner);
               road.Structure.ConstructionFinished(true);



               Map.TileMap[215][27].AddToWheelPath(Common.Direction.SouthWest, 0.1f);
               Map.TileMap[214][28].AddToWheelPath(Common.Direction.NorthEast, 0.3f);
               Map.TileMap[214][28].AddToWheelPath(Common.Direction.West, 0.5f);
               Map.TileMap[213][28].AddToWheelPath(Common.Direction.East, 0.9f);
               Map.TileMap[213][28].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[212][28].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[212][28].AddToWheelPath(Common.Direction.SouthWest, 1f);
               Map.TileMap[211][29].AddToWheelPath(Common.Direction.South, 1f);
               Map.TileMap[211][30].AddToWheelPath(Common.Direction.SouthEast, 1f);
               Map.TileMap[211][30].AddToWheelPath(Common.Direction.North, 1f);
               Map.TileMap[212][31].AddToWheelPath(Common.Direction.NorthWest, 1f);
               Map.TileMap[209][31].AddToWheelPath(Common.Direction.SouthWest, 1f);
               Map.TileMap[208][32].AddToWheelPath(Common.Direction.NorthEast, 1f);
               Map.TileMap[211][29].AddToWheelPath(Common.Direction.NorthEast, 0.8f);
               Map.TileMap[211][29].AddToWheelPath(Common.Direction.West, 0.8f);
               Map.TileMap[210][29].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[210][29].AddToWheelPath(Common.Direction.South, 1f);
               Map.TileMap[210][30].AddToWheelPath(Common.Direction.North, 0.9f);
               Map.TileMap[210][30].AddToWheelPath(Common.Direction.SouthWest, 0.7f);
               Map.TileMap[209][31].AddToWheelPath(Common.Direction.NorthEast, 0.6f);
               Map.TileMap[209][31].AddToWheelPath(Common.Direction.West, 0.6f);
               Map.TileMap[208][31].AddToWheelPath(Common.Direction.East, 0.2f);
               Map.TileMap[208][31].AddToWheelPath(Common.Direction.West, 0.2f);
               Map.TileMap[207][31].AddToWheelPath(Common.Direction.East, 0.6f);
               Map.TileMap[207][31].AddToWheelPath(Common.Direction.South, 0.6f);
               Map.TileMap[207][32].AddToWheelPath(Common.Direction.North, 0.3f);
               Map.TileMap[207][32].AddToWheelPath(Common.Direction.South, 0.1f);
               Map.TileMap[217][32].AddToWheelPath(Common.Direction.NorthEast, 1f);
               Map.TileMap[218][31].AddToWheelPath(Common.Direction.SouthWest, 1f);
               Map.TileMap[218][31].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[219][31].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[219][31].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[221][30].AddToWheelPath(Common.Direction.SouthWest, 1f);
               Map.TileMap[221][30].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[222][30].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[222][30].AddToWheelPath(Common.Direction.East, 1f);

               Map.TileMap[217][36].AddToWheelPath(Common.Direction.East, 0.3f);
               Map.TileMap[218][36].AddToWheelPath(Common.Direction.West, 0.8f);
               Map.TileMap[218][36].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[219][36].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[219][36].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[220][36].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[220][36].AddToWheelPath(Common.Direction.SouthEast, 1f);
               Map.TileMap[221][37].AddToWheelPath(Common.Direction.NorthWest, 1f);
               Map.TileMap[221][37].AddToWheelPath(Common.Direction.SouthEast, 1f);
               Map.TileMap[222][38].AddToWheelPath(Common.Direction.NorthWest, 1f);
               Map.TileMap[222][38].AddToWheelPath(Common.Direction.South, 1f);

               Map.TileMap[217][33].AddToWheelPath(Common.Direction.NorthWest, 0.2f);
               Map.TileMap[216][32].AddToWheelPath(Common.Direction.SouthEast, 0.6f);
               Map.TileMap[217][32].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[216][32].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[216][32].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[215][32].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[215][32].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[214][32].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[214][32].AddToWheelPath(Common.Direction.West, 1f);
               Map.TileMap[213][32].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[213][32].AddToWheelPath(Common.Direction.NorthWest, 1f);
               Map.TileMap[212][31].AddToWheelPath(Common.Direction.SouthEast, 1f);
               Map.TileMap[212][31].AddToWheelPath(Common.Direction.West, 1f);

               Map.TileMap[213][36].AddToFootPath(Common.Direction.West, 0.8f);
               Map.TileMap[213][36].AddToFootPath(Common.Direction.East, 0.8f);
               Map.TileMap[214][36].AddToFootPath(Common.Direction.West, 0.8f);
               Map.TileMap[214][36].AddToFootPath(Common.Direction.East, 0.8f);
               Map.TileMap[215][36].AddToFootPath(Common.Direction.West, 0.8f);

               Map.TileMap[212][36].AddToWheelPath(Common.Direction.East, 1f);
               Map.TileMap[212][36].AddToWheelPath(Common.Direction.SouthWest, 0.7f);
               Map.TileMap[211][37].AddToWheelPath(Common.Direction.NorthEast, 0.3f);



               Map.TileMap[217][35].AddToWheelPath(Common.Direction.East, 0.4f);
               Map.TileMap[218][35].AddToWheelPath(Common.Direction.West, 0.7f);
               Map.TileMap[218][35].AddToFootPath(Common.Direction.East, 0.7f);
               Map.TileMap[219][35].AddToFootPath(Common.Direction.West, 0.7f);
               Map.TileMap[219][35].AddToFootPath(Common.Direction.East, 0.7f);

               Map.TileMap[205][35].AddToFootPath(Common.Direction.NorthWest);
               Map.TileMap[205][35].AddToFootPath(Common.Direction.South);
               Map.TileMap[205][36].AddToFootPath(Common.Direction.North);
               Map.TileMap[205][36].AddToFootPath(Common.Direction.South);
               Map.TileMap[205][37].AddToFootPath(Common.Direction.North);
               Map.TileMap[205][37].AddToFootPath(Common.Direction.SouthEast);

               Map.TileMap[210][30].AddToFootPath(Common.Direction.SouthEast);
               Map.TileMap[211][31].AddToFootPath(Common.Direction.NorthWest);
                  //  Map.TileMap[209][29].AddToFootPath(Common.Direction.SouthEast);
                  //Map.TileMap[209][29].AddToFootPath(Common.Direction.NorthWest)  
               Map.TileMap[211][31].AddToFootPath(Common.Direction.East);


               Entity rocks = new Entity(GameData.Instance.AllEntityTypes["terrain:rockformation1"]);
               Point pos = new Point(4, 8);
               rocks.PlaceGroundFeature(pos, null, MapManager.TileToWorldPos(pos));

               // rocks.PlaceEntityOnTile(4, 8);

              //   Road road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(17, 5), Common.Direction.NorthEast, ColonyOwner);
              // road.ConstructionFinished();
              //road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(17, 5), Common.Direction.SouthWest, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(16, 6), Common.Direction.NorthEast, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(16, 6), Common.Direction.SouthWest, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(15, 7), Common.Direction.NorthEast, ColonyOwner);
              // road.ConstructionFinished();
              //road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(15, 7), Common.Direction.SouthWest, ColonyOwner);
              // road.ConstructionFinished();
              //  road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(14, 8), Common.Direction.NorthEast, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(14, 8), Common.Direction.West, ColonyOwner);
              // road.ConstructionFinished();           
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(13, 8), Common.Direction.East, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(13, 8), Common.Direction.West, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(12, 8), Common.Direction.East, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(12, 8), Common.Direction.West, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(11, 8), Common.Direction.East, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(11, 8), Common.Direction.SouthWest, ColonyOwner);
              // road.ConstructionFinished();

              // // crossroad:
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(13, 8), Common.Direction.North, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(13, 8), Common.Direction.South, ColonyOwner);
              // road.ConstructionFinished();


              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(10, 4), Common.Direction.South, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(10, 4), Common.Direction.West, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(9, 4), Common.Direction.East, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(9, 4), Common.Direction.South, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(9, 5), Common.Direction.North, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(9, 5), Common.Direction.East, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(10, 5), Common.Direction.West, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(10, 5), Common.Direction.SouthEast, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(11, 6), Common.Direction.NorthWest, ColonyOwner);
              // road.ConstructionFinished();
              // road = new Road(AllStructureTypes["structure:road"]);
              // road.AddBuildingToWorld(new Point(11, 6), Common.Direction.South, ColonyOwner);
              // road.ConstructionFinished(); 



                 // Item item1 = new Item(GameData.Instance.AllItemTypes["item:mixedstonespilemedium"]);
                 //AddColonyItem(item1, new Point(200, 39));

                       

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:sandpilesmall"]);
                 //AddColonyItem(item1, new Point(218, 38));

           

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:mixedstonespilesmall"]);
                 //AddColonyItem(item1, new Point(220, 34));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:mixedstonespilemedium"]);
                 //AddColonyItem(item1, new Point(210, 31));
           
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:mixedstonespilelarge"]);
                 //AddColonyItem(item1, new Point(219, 34));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:orepilesmall"]);
                 //AddColonyItem(item1, new Point(203, 34));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:orepilemedium"]);
                 //AddColonyItem(item1, new Point(219, 37));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:orepilelarge"]);
                 //AddColonyItem(item1, new Point(218, 37));
           
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:sandpilemedium"]);
                 //AddColonyItem(item1, new Point(217, 37));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:sandpilelarge"]);
                 //AddColonyItem(item1, new Point(211, 31));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:soilpilesmall"]);
                 //AddColonyItem(item1, new Point(218, 34));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:soilpilemedium"]);
                 //AddColonyItem(item1, new Point(218, 33));
                 // item1 = new Item(GameData.Instance.AllItemTypes["item:soilpilelarge"]); // has outline?
                 //AddColonyItem(item1, new Point(208, 29)); 

                 // item1 = new Entity(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(208, 31), new Vector2(-8f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(208, 31), new Vector2(-8f, 12f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                 //AddColonyItem(item1, new Point(208, 31), new Vector2(28f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(207, 30), new Vector2(-8f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(207, 31), new Vector2(18f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:coilRifle"]);
                 //AddColonyItem(item1, new Point(209, 32), new Vector2(6, -2f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:coilRifle"]);
                 //AddColonyItem(item1, new Point(203, 35), new Vector2(6, -20f));

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(216, 33), new Vector2(-8f, 7f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(216, 33), new Vector2(12f, 7f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(216, 33), new Vector2(-9f, -5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(215, 33), new Vector2(8f, 6f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:planks"]);
                 //AddColonyItem(item1, new Point(215, 33), new Vector2(9f, -5f));

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                 //AddColonyItem(item1, new Point(214, 36), new Vector2(-5f, -5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                 //AddColonyItem(item1, new Point(214, 36), new Vector2(5f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                 //AddColonyItem(item1, new Point(215, 36), new Vector2(-9f, 3f));

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(218, 35), new Vector2(8f, 3f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(218, 35), new Vector2(8f, -3f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(219, 35), new Vector2(-8f, -9f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(219, 35), new Vector2(-5f, -3f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                 //AddColonyItem(item1, new Point(219, 35), new Vector2(-8f, 5f));

                 //item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
                 //AddColonyItem(item1, new Point(218, 35), new Vector2(-5f, 5f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
                 //AddColonyItem(item1, new Point(218, 35), new Vector2(-8f, 15f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:tent"]);
                 //AddColonyItem(item1, new Point(218, 35), new Vector2(0f, 9f));



                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(-13f, -6f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(1f, -6f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(13f, -6f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(-15f, 7f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(-1f, 7f));
                 //item1 = new Item(GameData.Instance.AllItemTypes["item:chemicals"]);
                 //AddColonyItem(item1, new Point(220, 37), new Vector2(13f, 9f));
               



               return;

                  // for (int i = 0; i < 20; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                  //    house3.Structure.StoreItem(item, expeditionOwner);
                  //    x = 15; // random.Next(5, 10);
                  //    y = 5; //random.Next(5, 10);
                  //    //   AddColonyItem(item, new Point(x, y));
                  //}
                  //for (int i = 0; i < 2; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                  //    x = 17; // random.Next(5, 10);
                  //    y = 7; // random.Next(5, 10);
                  //    AddColonyItem(item, new Point(17, 7));
                  //}
                  //for (int i = 0; i < 2; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:cement"]);
                  //    x = 14; // random.Next(5, 10);
                  //    y = 7; // random.Next(5, 10);
                  //    AddColonyItem(item, new Point(x, y));
                  //}
                  //for (int i = 0; i < 2; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:ironore"]);
                  //    house3.Structure.StoreItem(item, expeditionOwner);
                  //}

                  //x = 18;
                  //for (int i = 0; i < 2; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
                  //    // Features[0].StoreItem(item, ColonyOwner);
                  //    //random.Next(5, 5);
                  //    //   y = 7; // random.Next(5, 6);

                  //    AddColonyItem(item, new Point(18, 5));
                  //    //x = 7;
                  //}
               



                  // for (int i = 0; i < 2; i++)
                  //{
                  //    Item item = new Item(GameData.Instance.AllItemTypes["item:metalparts"]);
               
                  //    //  Features[0].StoreItem(item, p14.Household.Ownership);
                  //    //  item.AddItemToWorld(MapManager.TileToWorldPos(new Point(16, 3)), p14.Household.Ownership);
                  //    //  item.InsideBuilding = Features[0]; 

                  //    //  item.AddItemToWorld(MapManager.TileToWorldPos(new Point(18, 5)), p14.Household.Ownership);
                  //} 

                 // for (int i = 0; i < 5; i++)
                 //{
                 //    Item item = new Item(GameData.Instance.AllItemTypes["item:stones"]);
                 //    x = random.Next(12, 16);
                 //    y = random.Next(4, 7);
                 //    AddColonyItem(item, new Point(x, y));
                 //}
                 //Item stones = new Item(GameData.Instance.AllItemTypes["item:stones"]);
                 //AddColonyItem(stones, new Point(15, 4));
                 //stones = new Item(GameData.Instance.AllItemTypes["item:stones"]);
                 //AddColonyItem(stones, new Point(14, 4));
              


                 // for (int i = 0; i < 0; i++)
                 //{
                 //    Item item = new Item(GameData.Instance.AllItemTypes["item:meat"]);
                 //    x = 12;// random.Next(10, 16);
                 //    y = 6;// random.Next(4, 7);
                 //    AddColonyItem(item, new Point(x, y));
                 //}
                 //for (int i = 0; i < 0; i++)
                 //{
                 //    Item item = new Item(GameData.Instance.AllItemTypes["item:grain"]);
                 //    x = 12;// random.Next(10, 16);
                 //    y = 6;
                 //    AddColonyItem(item, new Point(x, y));
                 //}
                 //for (int i = 0; i < 0; i++)
                 //{
                 //    Item item = new Item(GameData.Instance.AllItemTypes["item:vegetables"]);
                 //    x = 12;// random.Next(10, 16);
                 //    y = 6;
                 //    AddColonyItem(item, new Point(x, y));
                 //} 


            
                                        for (int i = 0; i < 1; i++)
                                          {
                                              Item item = new huntingrifle();
                                              x = random.Next(6, 8);
                                              y = random.Next(4, 7);
                                              AddColonyItem(item, new Point(x, y));
                                          }
            
                                          ////// FIRE ///////

               //    PlaceFire();


               //     Fire.Fire2DBase fire2D = new Fire.Fire2DBase(this, 15);
               //     Components.Add(fire2D);

               //     Quaternion rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(-MathHelper.PiOver2));
               //     fire.myRotation = rotation;
              

                           //PowerBike bike = new PowerBike(this);
                           //bike.MapPosition = new Point(10, 7);
                           //Vehicles.Add(bike);
                           //map.TileMap[10, 7].AddVehicle(bike);

                           //bike = new PowerBike(this);
                           //bike.MapPosition = new Point(10, 12);
                           //Vehicles.Add(bike);
                           //map.TileMap[10, 12].AddVehicle(bike);
                         
           }*/











    }
}

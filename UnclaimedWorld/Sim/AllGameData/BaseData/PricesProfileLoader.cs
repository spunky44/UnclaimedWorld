using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData
{
    public class PricesProfileLoader
    {
        public static List<PricesProfile> Init()
        {
            List<PricesProfile> list = new List<PricesProfile>();

            list.Add(new PricesProfile()
            {
                KeyName = "descentEraPrices",
                
                // Sell prices (what the player can buy at) are 1.5X these prices
                Prices = new SerializableDictionary<string, float>()
                {
                        #region FOOD
                             { "item:fingerFruit", 1.5f }, 
                             { "item:fermentedFingerFruit", 3f }, 
                             { "item:driedBeef", 3.2f }, 
                             { "item:waterCaneSeeds", 1f }, 
                             { "item:smokedTurnip", 2.2f }, 
                             { "item:turnipSalami", 3f }, 
                             { "item:smokedThunderChicken", 2f }, 
                             { "item:driedThunderChicken", 2f }, 
                             { "item:pickledAlabasterRay", 2f }, 
                             { "item:smokedAlabasterRay", 2f }, 
                             { "item:smokedStreakFin", 1.8f }, 
                             { "item:driedSaltedStreakFin", 2f }, 
                             { "item:smokedCarbonTail", 1.8f }, 
                             { "item:pickledCarbonTail", 2f }, 
                             { "item:commonOilTubers", 1.2f }, 
                             { "item:glassyCreeperPods", 1f }, 
                             { "item:hardtack", 2f }, 
                             { "item:crystalBerries", 1.5f }, 
                             { "item:powderedCrystalBerries", 1.7f }, 
                             { "item:simCoffeeBeans", 40f }, 
                        #endregion       
                             #region RAW MATERIALS
                                { "item:blackPowder", 6f }, 
                                { "item:saltpeter", 2f }, 
                                { "item:gunBarrelUnbored", 15f }, 
                                { "item:gunBarrelSmoothLong", 18f }, 
                                { "item:gunBarrelSmoothShort", 18f }, 
                                { "item:gunBarrelRifled", 26f }, 
                                { "item:gunStock", 5f }, 
                                { "item:anvil", 20f }, 
                                { "item:barClamps", 7f },                               
                                { "item:bogOre", 3f }, 
                                { "item:goldOre", 3f }, 
                                { "item:gold", 5f }, 
                                { "item:roughBloomIron", 4f }, 
                                { "item:wroughtIron", 5f }, 
                                { "item:blisterSteel", 6f }, 
                                { "item:gaskets", 10f }, 
                                { "item:guano", 2f }, 
                                { "item:marshcotSap", 3f },
                                { "item:sulfurPowder", 3f }, 
                                { "item:clay", 1f }, 
                                { "item:solidMudBrick", 2f }, 
                                { "item:firebricks", 6f }, 
                                { "item:salt", 3f }, 
                                { "item:organicFertilizer", 2f }, 
                                { "item:guanoFertilizer", 3f }, 
                                { "item:sulfurBlocks", 1f }, 
                                { "item:scrapMetal", 3f }, 
                                { "item:panelScraps", 5f }, 
                                { "item:inactivatedFoodCoolerUnit", 70f }, 
                                { "item:textile", 4f }, 
                                { "item:cotton", 2f }, 
                                { "item:smallTent", 60f }, 
                                { "item:octagonalTent", 75f }, 
                                { "item:domeTent", 70f }, 
                                { "item:thermalTarp", 80f }, 
                                { "item:fishingNet", 10f },
                                { "item:fishTrapHoopNet", 13f }, 
                                { "item:fishTrapBasket", 6f }, 
                                { "item:daysheenLeaves", 1f }, 
                                { "item:charcoal", 2f }, 
                                { "item:firewood", 1f }, 
                                { "item:dryPeat", 1f }, 
                                { "item:waterCaneStem", 1f }, 
                                { "item:waterCaneLeaves", 1f }, 
                                { "item:shadeleafCanes", 1f }, 
                                { "item:shadeleafBowStave", 1f }, 
                                { "item:wingweedLeaves", 1f }, 
                                { "item:wingweedMat", 4f }, 
                                { "item:spoakLeaves", 1f }, 
                                { "item:spoakBranches", 1f }, 
                                { "item:spoakBranchesTrimmed", 1f }, 
                                { "item:spoakShingles", 4f }, 
                                { "item:stones", 1f }, 
                                { "item:soil", 1f }, 
                                { "item:podlacUnrefined", 12f }, 
                                { "item:twinklerPlating", 3f }, 
                                { "item:improvisedGreenHouseCover", 14f }, 
                                { "item:megapodRawhide", 9f }, 
                                { "item:megapodTannedHide", 11f }, 
                                { "item:whipjawRawhide", 8f }, 
                                { "item:whipjawTannedHide", 10f }, 
                                { "item:thunderChickenRawhide", 7f }, //was:4. trapping and hunting is difficult and  needs to be profitable
                                { "item:thunderChickenTannedHide", 8f }, 
                                { "item:improvisedBowLimb", 3f }, 
                                { "item:improvisedArrowShaft", 2f }, 
                                { "item:improvisedMetalArrowHead", 5f }, 
                                { "item:ironArrowHead", 5f }, 
                                { "item:flintlockMechanism", 25f }, 
                                { "item:boltActionMechanism", 31f }, 
                                { "item:landMine", 40f }, 
                                { "item:improvisedMetalHoeBlade", 4f }, 
                                { "item:ironSpearhead", 3f }, 
                                { "item:improvisedMetalSpearhead", 3f },        
                            #endregion                               
                            #region INDUSTRIAL COMPONENTS
                                { "item:loomComponents", 50 }, 
                                { "item:stillComponents", 40 }, 
                                { "item:extrusionMachineComponents", 70 },
                                { "item:metalLatheComponents", 80 },
                                { "item:humanPowerUnit", 50 },
                                { "item:humanPowerUnitComponents", 35 },
                            #endregion
                            #region GADGETS
                                { "item:groundScanner", 90 }, //mp has no use, so i've removed it from trade profile.
                                { "item:cloak", 80 }, //mp limited if any use
                                { "item:nightVisionGoggles", 70 }, //mp limited if any use
                            #endregion
                            #region WEAPONS
                                { "item:improvisedBow", 5f },  
                                { "item:musket", 40f },  
                                { "item:musketoon", 40f },
                                { "item:gunpowderRifle", 50f },
                                { "item:boltActionRifle", 90f }, 
                                { "item:shotgun", 100f }, //mp reduced prices sep 2016. price for advanced items should not be THAT high because it's plenty difficult to reach Advanced tier
                                { "item:coilRifle", 115f }, 
                                { "item:sentry", 150f },
 

                            #endregion
                            #region AMMUNITION
                                { "item:ironArrow", 7f }, 
                                { "item:blackPowderShotAmmo", 10f }, 
                                { "item:blackPowderRifleAmmo", 12f }, 
                                { "item:corditeAmmo", 21f }, 
                                { "item:shotgunAmmo", 26f },
                                { "item:coilRifleAmmo", 35f },     
                                { "item:sentryGunAmmo", 50f },                          
                               
                             #endregion
                             #region       BODIES 
                                { "item:binalRatCarcass", 1f }, 
                                { "item:thunderChickenCarcass", 1f }, 
                                { "item:quaditeCarcass", 1f }, 
                         //       { "item:leafcutterCarcass", 3f }, 
                             
                                //note, some are too heavy to carry and therefore not used for sale:
                                //"item:turnipCarcass"
                                //"item:bushDragonCarcass"
                                //"item:forestGuardianCarcass"
                                //"item:megapodCarcass"
                                //"item:swampDemonTreeCarcass"
                                //"item:whipjawCarcass"
                                //"item:demontreeCarcass"
                                //"item:patricianCarcass"
                            #endregion
                                 #region TOOLS  
                                { "item:bellows", 6f }, 
                                { "item:turnipCracker", 7f }, 
                                { "item:advancedKnife", 50f }, 
                                { "item:improvisedKnife", 6f }, 
                                { "item:steelKnife", 10f }, 
                                { "item:advancedMachete", 65f }, 
                                { "item:steelMachete", 12f }, 
                                { "item:steelHandAxe", 11f }, 
                                { "item:steelPickaxe", 12f }, 
                                { "item:hammer", 6f }, 
                                { "item:file", 6f }, 
                                { "item:tongs", 6f }, 
                                { "item:handDrill", 10f }, 
                                { "item:hacksaw", 10f }, 
                                { "item:blacksmithsToolbox", 22f }, 
                                { "item:metalWorkersToolbox", 46f }, 
                                { "item:steelHoe", 12f }, 
                                { "item:steelSpade", 12f }, 
                                { "item:ironSpear", 7f }, 
                                { "item:vinegar", 2f }, 
                                { "item:shadeleafResin", 2f },  
                                { "item:advancedString", 45f }, 
                                { "item:metalWire", 6f }, 
                                { "item:rawhideString", 3f }, 
                                { "item:cottonString", 3f }, 
                                { "item:bulletMold", 6f }, 
                                { "item:sandMold", 6f }, 
                                { "item:tinnerSnips", 10f }, 
                                { "item:advancedSnips", 65f }, 
                                { "item:advancedCookingPot", 60f }, 
                                { "item:goldPot", 7f }, 
                                { "item:clayPotUnglazed", 5f }, 
                                { "item:vat", 6f }, 
                                { "item:tappingBucket", 5f }, 
                                { "item:plasticTappingBucket", 6f }, 
                                { "item:clayJar", 6f }, 
                                { "item:improvisedPlasticJar", 6f }, 
                                { "item:strongBugNet", 4f }, 
                                { "item:ironHooks", 5f },          
                             #endregion
                             #region STRUCTURE ITEMS and PARTS
                                
                                { "item:fieldLabPacked", 250f }, 
                                { "item:spikeTrap", 8f }, 
                                { "item:sensor", 60f }, 
                                { "item:structurePanels", 3f }, 
                                { "item:radio", 50f }, 
                                { "item:radioAntenna", 40f }, 
                            #endregion
                            #region COMFORT ITEMS
                                { "item:crystalWine", 3f },
                                { "item:crystalBrandy", 5f },
                    #endregion
                    #region ANIMALS
                    { "entity:dog", 35f },
                    #endregion
                    #region ROBOTS
                     { "entity:haulingRobot", 200f }
                    #endregion                   
                }

                       
            });

            list.Add(new PricesProfile()
            {
                KeyName = "planetFallEraPrices",
                Comments = "kept the descent era prices, but lowered all advanced items",
                // Sell prices (what the player can buy at) are 1.5X these prices
                Prices = new SerializableDictionary<string, float>()
                {
                        #region FOOD
                             { "item:fingerFruit", 1.5f }, 
                             { "item:fermentedFingerFruit", 3f }, 
                             { "item:driedBeef", 3.2f }, 
                             { "item:waterCaneSeeds", 1f }, 
                             { "item:smokedTurnip", 2.2f }, 
                             { "item:turnipSalami", 3f }, 
                             { "item:smokedThunderChicken", 2f }, 
                             { "item:driedThunderChicken", 2f }, 
                             { "item:pickledAlabasterRay", 2f }, 
                             { "item:smokedAlabasterRay", 2f }, 
                             { "item:smokedStreakFin", 1.8f }, 
                             { "item:driedSaltedStreakFin", 2f }, 
                             { "item:smokedCarbonTail", 1.8f }, 
                             { "item:pickledCarbonTail", 2f }, 
                             { "item:commonOilTubers", 1.2f }, 
                             { "item:glassyCreeperPods", 1f }, 
                             { "item:hardtack", 2f }, 
                             { "item:crystalBerries", 1.5f }, 
                             { "item:powderedCrystalBerries", 1.7f }, 
                             { "item:simCoffeeBeans", 5f },
                             { "item:astroRation", 4f },
                        #endregion       
                             #region RAW MATERIALS
                                { "item:blackPowder", 6f }, 
                                { "item:saltpeter", 2f }, 
                                { "item:gunBarrelUnbored", 15f }, 
                                { "item:gunBarrelSmoothLong", 18f }, 
                                { "item:gunBarrelSmoothShort", 18f }, 
                                { "item:gunBarrelRifled", 26f }, 
                                { "item:gunStock", 5f }, 
                                { "item:anvil", 20f }, 
                                { "item:barClamps", 7f },                               
                                { "item:bogOre", 3f }, 
                                { "item:goldOre", 3f }, 
                                { "item:gold", 5f }, 
                                { "item:roughBloomIron", 4f }, 
                                { "item:wroughtIron", 5f }, 
                                { "item:blisterSteel", 6f }, 
                                { "item:gaskets", 10f }, 
                                { "item:guano", 2f }, 
                                { "item:marshcotSap", 3f },
                                { "item:sulfurPowder", 3f }, 
                                { "item:clay", 1f }, 
                                { "item:solidMudBrick", 2f }, 
                                { "item:firebricks", 6f }, 
                                { "item:salt", 3f }, 
                                { "item:organicFertilizer", 2f }, 
                                { "item:guanoFertilizer", 3f }, 
                                { "item:sulfurBlocks", 1f }, 
                                { "item:scrapMetal", 3f }, 
                                { "item:panelScraps", 5f }, 
                                { "item:inactivatedFoodCoolerUnit", 20f }, 
                                { "item:textile", 4f }, 
                                { "item:cotton", 2f }, 
                                { "item:smallTent", 20f }, 
                                { "item:octagonalTent", 35f }, 
                                { "item:domeTent", 40f }, 
                                { "item:thermalTarp", 10f }, 
                                { "item:fishingNet", 10f },
                                { "item:fishTrapHoopNet", 13f }, 
                                { "item:fishTrapBasket", 6f }, 
                                { "item:daysheenLeaves", 1f }, 
                                { "item:charcoal", 2f }, 
                                { "item:firewood", 1f }, 
                                { "item:dryPeat", 1f }, 
                                { "item:waterCaneStem", 1f }, 
                                { "item:waterCaneLeaves", 1f }, 
                                { "item:shadeleafCanes", 1f }, 
                                { "item:shadeleafBowStave", 1f }, 
                                { "item:wingweedLeaves", 1f }, 
                                { "item:wingweedMat", 4f }, 
                                { "item:spoakLeaves", 1f }, 
                                { "item:spoakBranches", 1f }, 
                                { "item:spoakBranchesTrimmed", 1f }, 
                                { "item:spoakShingles", 4f }, 
                                { "item:stones", 1f }, 
                                { "item:soil", 1f }, 
                                { "item:podlacUnrefined", 12f }, 
                                { "item:twinklerPlating", 3f }, 
                                { "item:improvisedGreenHouseCover", 14f }, 
                                { "item:megapodRawhide", 9f }, 
                                { "item:megapodTannedHide", 11f }, 
                                { "item:whipjawRawhide", 8f }, 
                                { "item:whipjawTannedHide", 10f }, 
                                { "item:thunderChickenRawhide", 7f }, //was:4. trapping and hunting is difficult and  needs to be profitable
                                { "item:thunderChickenTannedHide", 8f }, 
                                { "item:improvisedBowLimb", 3f }, 
                                { "item:improvisedArrowShaft", 2f }, 
                                { "item:improvisedMetalArrowHead", 5f }, 
                                { "item:ironArrowHead", 5f }, 
                                { "item:flintlockMechanism", 25f }, 
                                { "item:boltActionMechanism", 50f }, 
                                { "item:landMine", 40f }, 
                                { "item:improvisedMetalHoeBlade", 4f }, 
                                { "item:ironSpearhead", 3f }, 
                                { "item:improvisedMetalSpearhead", 3f },
                                { "item:liquidGas", 2f },
                                //NA MINING CAMP MATERIAL
                                { "item:scandium", 60f },
                                { "item:terbium", 150f },
                                 
                            #endregion                               
                            #region INDUSTRIAL COMPONENTS
                                { "item:loomComponents", 50 }, 
                                { "item:stillComponents", 40 }, 
                                { "item:extrusionMachineComponents", 70 },
                                { "item:metalLatheComponents", 80 },
                                { "item:humanPowerUnit", 50 },
                                { "item:humanPowerUnitComponents", 35 },
                            #endregion
                            #region GADGETS
                                { "item:groundScanner", 26 }, 
                                { "item:cloak", 25 }, 
                                { "item:nightVisionGoggles", 22 },
                            #endregion
                            #region WEAPONS
                                { "item:boltActionRifle", 52f }, 
                                { "item:gunpowderRifle", 50f }, 
                                { "item:musket", 40f },  
                                { "item:musketoon", 40f }, 
                                { "item:shotgun", 45f }, 
                                { "item:coilRifle", 55f }, 
                                { "item:sentry", 90f }, 
                                { "item:improvisedBow", 5f },
                                { "item:sentrySprayGun", 40f},
                            #endregion
                            #region AMMUNITION
                                { "item:sentryGunAmmo", 10f }, 
                                { "item:corditeAmmo", 15f }, 
                                { "item:blackPowderRifleAmmo", 12f }, 
                                { "item:coilRifleAmmo", 10f }, 
                                { "item:shotgunAmmo", 11f }, 
                                { "item:blackPowderShotAmmo", 10f }, 
                                { "item:ironArrow", 7f },                             
                               
                             #endregion
                             #region       BODIES 
                                { "item:binalRatCarcass", 1f }, 
                                { "item:thunderChickenCarcass", 1f }, 
                                { "item:quaditeCarcass", 1f }, 
                            #endregion
                                 #region TOOLS                                
                                { "item:advancedMachete", 10f }, 
                                { "item:advancedKnife", 8f }, 
                                { "item:advancedString", 6f }, 
                                { "item:advancedSnips", 6f }, 
                                { "item:advancedCookingPot", 5f }, 
                                { "item:improvisedKnife", 6f }, 
                                { "item:steelKnife", 10f }, 
                                { "item:bellows", 6f }, 
                                { "item:turnipCracker", 7f }, 
                                { "item:steelMachete", 12f }, 
                                { "item:steelHandAxe", 11f }, 
                                { "item:steelPickaxe", 12f }, 
                                { "item:hammer", 6f }, 
                                { "item:file", 6f }, 
                                { "item:tongs", 6f }, 
                                { "item:handDrill", 10f }, 
                                { "item:hacksaw", 10f }, 
                                { "item:blacksmithsToolbox", 22f }, 
                                { "item:metalWorkersToolbox", 46f }, 
                                { "item:steelHoe", 12f }, 
                                { "item:steelSpade", 12f }, 
                                { "item:ironSpear", 7f }, 
                                { "item:vinegar", 2f }, 
                                { "item:shadeleafResin", 2f },                                 
                                { "item:metalWire", 6f }, 
                                { "item:rawhideString", 3f }, 
                                { "item:cottonString", 3f }, 
                                { "item:bulletMold", 6f }, 
                                { "item:sandMold", 6f }, 
                                { "item:tinnerSnips", 10f },                               
                                { "item:goldPot", 7f }, 
                                { "item:clayPotUnglazed", 5f }, 
                                { "item:vat", 6f }, 
                                { "item:tappingBucket", 5f }, 
                                { "item:plasticTappingBucket", 6f }, 
                                { "item:clayJar", 6f }, 
                                { "item:improvisedPlasticJar", 6f }, 
                                { "item:strongBugNet", 4f }, 
                                { "item:ironHooks", 5f },          
                             #endregion
                             #region STRUCTURE ITEMS and PARTS                                
                                { "item:fieldLabPacked", 20f }, 
                                { "item:fieldKitchenStove", 18f },
                                { "item:fieldKitchenEquipment", 20f },                             
                                { "item:spikeTrap", 8f }, 
                                { "item:sensor", 14f }, 
                                { "item:structurePanels", 3f }, 
                                { "item:diamondGlass", 8f }, 
                                { "item:radio", 50f }, 
                                { "item:radioAntenna", 40f },
                                { "item:labComponents", 30f },
                                { "item:satelliteGroundStation", 20f },
                            #endregion
                            #region COMFORT ITEMS
                                { "item:crystalWine", 6f },
                                { "item:crystalBrandy", 7f },
                    #endregion
                    #region ANIMALS
                    { "entity:dog", 55f },
                    #endregion
                    #region ROBOTS
                     { "entity:haulingRobot", 100f }
                    #endregion   
                }


            });
           

            return list;

        }

    }
}

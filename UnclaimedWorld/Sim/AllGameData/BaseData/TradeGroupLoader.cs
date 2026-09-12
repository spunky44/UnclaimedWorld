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
    public class TradeGroupLoader
    {
        public static List<TradeGroup> Init()
        {
            List<TradeGroup> list = new List<TradeGroup>();
            

            /* TODO: Destiny river delta trade goods
             * 
            #region otherSite1Expedition1////GOODS available to BUY                                                 
            #region FOOD                      
  "otherSite1Expedition1_fermentedFingerFruit",   "otherSite1Expedition1_fermentedFingerFruit",  "otherSite1Expedition1_fermentedFingerFruit",  "otherSite1Expedition1_fermentedFingerFruit",  "otherSite1Expedition1_fermentedFingerFruit",  "otherSite1Expedition1_fermentedFingerFruit",
"otherSite1Expedition1_driedBeef", "otherSite1Expedition1_driedBeef","otherSite1Expedition1_driedBeef","otherSite1Expedition1_driedBeef","otherSite1Expedition1_driedBeef","otherSite1Expedition1_driedBeef",
"otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds","otherSite1Expedition1_waterCaneSeeds",
//"otherSite1Expedition1_smokedTurnip",
//"otherSite1Expedition1_turnipSalami",
"otherSite1Expedition1_smokedThunderChicken",
"otherSite1Expedition1_driedThunderChicken","otherSite1Expedition1_driedThunderChicken","otherSite1Expedition1_driedThunderChicken","otherSite1Expedition1_driedThunderChicken","otherSite1Expedition1_driedThunderChicken","otherSite1Expedition1_driedThunderChicken",
//"otherSite1Expedition1_pickledAlabasterRay",
//"otherSite1Expedition1_smokedAlabasterRay",
"otherSite1Expedition1_smokedStreakFin",
"otherSite1Expedition1_driedSaltedStreakFin","otherSite1Expedition1_driedSaltedStreakFin","otherSite1Expedition1_driedSaltedStreakFin","otherSite1Expedition1_driedSaltedStreakFin","otherSite1Expedition1_driedSaltedStreakFin",
"otherSite1Expedition1_smokedCarbonTail",
"otherSite1Expedition1_pickledCarbonTail", "otherSite1Expedition1_pickledCarbonTail","otherSite1Expedition1_pickledCarbonTail","otherSite1Expedition1_pickledCarbonTail",
"otherSite1Expedition1_commonOilTubers",
"otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods","otherSite1Expedition1_glassyCreeperPods",
"otherSite1Expedition1_hardtack", "otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack","otherSite1Expedition1_hardtack",
"otherSite1Expedition1_crystalBerries","otherSite1Expedition1_crystalBerries","otherSite1Expedition1_crystalBerries","otherSite1Expedition1_crystalBerries","otherSite1Expedition1_crystalBerries",
"otherSite1Expedition1_powderedCrystalBerries","otherSite1Expedition1_powderedCrystalBerries","otherSite1Expedition1_powderedCrystalBerries","otherSite1Expedition1_powderedCrystalBerries",
"otherSite1Expedition1_simCoffeeBeans",
  #endregion 
            #region WEAPONS  
"otherSite1Expedition1_boltActionRifle", "otherSite1Expedition1_boltActionRifle", "otherSite1Expedition1_boltActionRifle","otherSite1Expedition1_boltActionRifle","otherSite1Expedition1_boltActionRifle","otherSite1Expedition1_boltActionRifle",
"otherSite1Expedition1_gunpowderRifle","otherSite1Expedition1_gunpowderRifle","otherSite1Expedition1_gunpowderRifle","otherSite1Expedition1_gunpowderRifle",
"otherSite1Expedition1_musket",
"otherSite1Expedition1_musketoon","otherSite1Expedition1_musketoon","otherSite1Expedition1_musketoon",
"otherSite1Expedition1_shotgun",
"otherSite1Expedition1_sentry", //mp crashed the game when i placed the sentry structure.
"otherSite1Expedition1_improvisedBow","otherSite1Expedition1_improvisedBow",
  #endregion 
            #region RAW MATERIALS 
"otherSite1Expedition1_blackPowder", "otherSite1Expedition1_blackPowder","otherSite1Expedition1_blackPowder",
"otherSite1Expedition1_saltpeter", 
"otherSite1Expedition1_gunBarrelUnbored","otherSite1Expedition1_gunBarrelUnbored","otherSite1Expedition1_gunBarrelUnbored",
"otherSite1Expedition1_gunBarrelSmoothLong",
"otherSite1Expedition1_gunBarrelSmoothShort", 
"otherSite1Expedition1_gunBarrelRifled",
"otherSite1Expedition1_gunStock","otherSite1Expedition1_gunStock","otherSite1Expedition1_gunStock",
"otherSite1Expedition1_anvil", "otherSite1Expedition1_anvil", "otherSite1Expedition1_anvil",
"otherSite1Expedition1_barClamps","otherSite1Expedition1_barClamps",
//"otherSite1Expedition1_metalLathe",
//"otherSite1Expedition1_metalLatheComponents", 
//"otherSite1Expedition1_humanPowerUnitComponents",
"otherSite1Expedition1_humanPowerUnit", 

"otherSite1Expedition1_cotton","otherSite1Expedition1_cotton","otherSite1Expedition1_cotton","otherSite1Expedition1_cotton","otherSite1Expedition1_cotton", //mp cotton
//"otherSite1Expedition1_bogOre",
"otherSite1Expedition1_goldOre", 
"otherSite1Expedition1_gold","otherSite1Expedition1_gold","otherSite1Expedition1_gold",
//"otherSite1Expedition1_roughBloomIron",
"otherSite1Expedition1_wroughtIron","otherSite1Expedition1_wroughtIron","otherSite1Expedition1_wroughtIron","otherSite1Expedition1_wroughtIron","otherSite1Expedition1_wroughtIron","otherSite1Expedition1_wroughtIron",
"otherSite1Expedition1_blisterSteel","otherSite1Expedition1_blisterSteel","otherSite1Expedition1_blisterSteel",
"otherSite1Expedition1_guano",
"otherSite1Expedition1_sulfurPowder","otherSite1Expedition1_sulfurPowder",
"otherSite1Expedition1_clay","otherSite1Expedition1_clay","otherSite1Expedition1_clay","otherSite1Expedition1_clay","otherSite1Expedition1_clay","otherSite1Expedition1_clay",
"otherSite1Expedition1_solidMudBrick","otherSite1Expedition1_solidMudBrick","otherSite1Expedition1_solidMudBrick",

"otherSite1Expedition1_firebricks","otherSite1Expedition1_firebricks",
"otherSite1Expedition1_salt","otherSite1Expedition1_salt","otherSite1Expedition1_salt","otherSite1Expedition1_salt","otherSite1Expedition1_salt","otherSite1Expedition1_salt","otherSite1Expedition1_salt",

"otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer","otherSite1Expedition1_organicFertilizer",
"otherSite1Expedition1_guanoFertilizer",
"otherSite1Expedition1_sulfurBlocks", 

"otherSite1Expedition1_scrapMetal",
"otherSite1Expedition1_panelScraps",
//"otherSite1Expedition1_inactivatedFoodCoolerUnit",
"otherSite1Expedition1_textile", "otherSite1Expedition1_textile","otherSite1Expedition1_textile","otherSite1Expedition1_textile", "otherSite1Expedition1_textile", //enough for a cookhouse
"otherSite1Expedition1_smallTent",
//"otherSite1Expedition1_octagonalTent",
//"otherSite1Expedition1_domeTent",
"otherSite1Expedition1_thermalTarp",

"otherSite1Expedition1_fishTrapCoast",
"otherSite1Expedition1_fishTrapShore",
"otherSite1Expedition1_daysheenLeaves",
"otherSite1Expedition1_charcoal","otherSite1Expedition1_charcoal","otherSite1Expedition1_charcoal","otherSite1Expedition1_charcoal","otherSite1Expedition1_charcoal",

"otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood","otherSite1Expedition1_firewood",
"otherSite1Expedition1_waterCaneStem",
"otherSite1Expedition1_waterCaneLeaves",
"otherSite1Expedition1_shadeleafCanes","otherSite1Expedition1_shadeleafCanes","otherSite1Expedition1_shadeleafCanes","otherSite1Expedition1_shadeleafCanes","otherSite1Expedition1_shadeleafCanes","otherSite1Expedition1_shadeleafCanes",
//"otherSite1Expedition1_shadeleafBowStave",
//"otherSite1Expedition1_wingweedLeaves",
"otherSite1Expedition1_wingweedMats","otherSite1Expedition1_wingweedMats",
//"otherSite1Expedition1_spoakLeaves",
//"otherSite1Expedition1_spoakBranches",
"otherSite1Expedition1_spoakBranchesTrimmed","otherSite1Expedition1_spoakBranchesTrimmed",

"otherSite1Expedition1_spoakShingles","otherSite1Expedition1_spoakShingles",
"otherSite1Expedition1_stones","otherSite1Expedition1_stones","otherSite1Expedition1_stones","otherSite1Expedition1_stones",
"otherSite1Expedition1_soil",
"otherSite1Expedition1_podlacUnrefined",
//"otherSite1Expedition1_twinklerPlating",
"otherSite1Expedition1_improvisedGreenHouseCover",
//"otherSite1Expedition1_megapodRawhide",
//"otherSite1Expedition1_megapodTannedHide",
//"otherSite1Expedition1_whipjawRawhide",
//"otherSite1Expedition1_whipjawTannedHide",

"otherSite1Expedition1_thunderChickenRawhide",
"otherSite1Expedition1_thunderChickenTannedHide",
"otherSite1Expedition1_improvisedBowLimb",  
//"otherSite1Expedition1_improvisedArrowShaft", 

//"otherSite1Expedition1_improvisedMetalArrowHead",
//"otherSite1Expedition1_ironArrowHead", "otherSite1Expedition1_ironArrowHead", 
"otherSite1Expedition1_flintlockMechanism","otherSite1Expedition1_flintlockMechanism","otherSite1Expedition1_flintlockMechanism",
"otherSite1Expedition1_boltActionMechanism", 
"otherSite1Expedition1_landMine","otherSite1Expedition1_landMine","otherSite1Expedition1_landMine",

"otherSite1Expedition1_steelHoe",
//"otherSite1Expedition1_improvisedMetalHoe",
//"otherSite1Expedition1_ironSpearhead",
//"otherSite1Expedition1_improvisedMetalSpearhead",


   #endregion 
            #region AMMUNITION
"otherSite1Expedition1_sentryGunAmmo",
"otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo","otherSite1Expedition1_corditeAmmo",
"otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo","otherSite1Expedition1_blackPowderRifleAmmo",
"otherSite1Expedition1_shotgunAmmo","otherSite1Expedition1_shotgunAmmo","otherSite1Expedition1_shotgunAmmo",
"otherSite1Expedition1_blackPowderShotAmmo","otherSite1Expedition1_blackPowderShotAmmo","otherSite1Expedition1_blackPowderShotAmmo","otherSite1Expedition1_blackPowderShotAmmo","otherSite1Expedition1_blackPowderShotAmmo",
"otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow","otherSite1Expedition1_ironArrow",
   #endregion
            #region       BODIES
//"otherSite1Expedition1_binalRatCarcass",
//"otherSite1Expedition1_thunderChickenCarcass",
//"otherSite1Expedition1_quaditeCarcass",
   #endregion 
            #region TOOLS
"otherSite1Expedition1_bellows",
"otherSite1Expedition1_turnipCracker",
"otherSite1Expedition1_knife",
"otherSite1Expedition1_improvisedGoodKnife","otherSite1Expedition1_improvisedGoodKnife",
"otherSite1Expedition1_steelKnife","otherSite1Expedition1_steelKnife","otherSite1Expedition1_steelKnife",
"otherSite1Expedition1_machete",
"otherSite1Expedition1_steelMachete","otherSite1Expedition1_steelMachete","otherSite1Expedition1_steelMachete","otherSite1Expedition1_steelMachete",
"otherSite1Expedition1_steelHandAxe", "otherSite1Expedition1_steelHandAxe","otherSite1Expedition1_steelHandAxe",
"otherSite1Expedition1_steelPickaxe","otherSite1Expedition1_steelPickaxe","otherSite1Expedition1_steelPickaxe",
"otherSite1Expedition1_hammer","otherSite1Expedition1_hammer",
"otherSite1Expedition1_file","otherSite1Expedition1_file",
"otherSite1Expedition1_tongs","otherSite1Expedition1_tongs","otherSite1Expedition1_tongs",
"otherSite1Expedition1_handDrill","otherSite1Expedition1_handDrill",
"otherSite1Expedition1_hacksaw",
"otherSite1Expedition1_blacksmithsToolbox",
"otherSite1Expedition1_metalworkersToolbox",
"otherSite1Expedition1_steelHoe","otherSite1Expedition1_steelHoe","otherSite1Expedition1_steelHoe","otherSite1Expedition1_steelHoe","otherSite1Expedition1_steelHoe",
"otherSite1Expedition1_steelSpade","otherSite1Expedition1_steelSpade","otherSite1Expedition1_steelSpade",
"otherSite1Expedition1_ironSpear",

"otherSite1Expedition1_vinegar","otherSite1Expedition1_vinegar","otherSite1Expedition1_vinegar","otherSite1Expedition1_vinegar","otherSite1Expedition1_vinegar",
"otherSite1Expedition1_shadeleafResin","otherSite1Expedition1_shadeleafResin",
"otherSite1Expedition1_marshcotSap","otherSite1Expedition1_marshcotSap","otherSite1Expedition1_marshcotSap",
"otherSite1Expedition1_string",
"otherSite1Expedition1_metalWire","otherSite1Expedition1_metalWire","otherSite1Expedition1_metalWire",
"otherSite1Expedition1_rawhideString","otherSite1Expedition1_rawhideString","otherSite1Expedition1_rawhideString","otherSite1Expedition1_rawhideString","otherSite1Expedition1_rawhideString",
"otherSite1Expedition1_bulletMold", "otherSite1Expedition1_bulletMold",
"otherSite1Expedition1_sandMold",
"otherSite1Expedition1_tinnerSnips",
"otherSite1Expedition1_snips",
"otherSite1Expedition1_cookingPot",
"otherSite1Expedition1_goldPot","otherSite1Expedition1_goldPot","otherSite1Expedition1_goldPot",
"otherSite1Expedition1_clayPotUnglazed",
//"otherSite1Expedition1_vat",
"otherSite1Expedition1_tappingBucket",
"otherSite1Expedition1_plasticTappingBucket",
"otherSite1Expedition1_clayJar","otherSite1Expedition1_clayJar","otherSite1Expedition1_clayJar",
"otherSite1Expedition1_improvisedPlasticJar",
"otherSite1Expedition1_strongBugNet",
"otherSite1Expedition1_ironHooks","otherSite1Expedition1_ironHooks","otherSite1Expedition1_ironHooks","otherSite1Expedition1_ironHooks","otherSite1Expedition1_ironHooks",
   #endregion
            #region STRUCTURE ITEMS and PARTS
"otherSite1Expedition1_spikeTrap","otherSite1Expedition1_spikeTrap","otherSite1Expedition1_spikeTrap","otherSite1Expedition1_spikeTrap","otherSite1Expedition1_spikeTrap",
"otherSite1Expedition1_sensor",
"otherSite1Expedition1_structurePanels",
"otherSite1Expedition1_radio",
"otherSite1Expedition1_radioAntenna","otherSite1Expedition1_radioAntenna",
   #endregion 

   #endregion 

            */

            list.Add(new TradeGroup
            {
                KeyName = "foodCropsTrade", //see also "cropstrade"
                AvailableForTrade = new[] //new SerializableDictionary<string,TradeAmountType>()
                {
                       new TradeAmountType(){ EntityType = "item:glassyCreeperPods", LinearIncreasePerDay = 5, MaxAmountForSale = 30, LinearConsumptionPerDay = 5, MaxAmountToBuy = 30 },                               
                       new TradeAmountType(){ EntityType = "item:crystalBerries", LinearIncreasePerDay = 5, MaxAmountForSale = 30, LinearConsumptionPerDay = 5, MaxAmountToBuy = 30 },
                       new TradeAmountType(){ EntityType = "item:fingerFruit", LinearIncreasePerDay = 4f, MaxAmountForSale = 18, LinearConsumptionPerDay = 4, MaxAmountToBuy = 18 }                      

                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "cottonTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:cotton", LinearIncreasePerDay = 5, MaxAmountForSale = 25, LinearConsumptionPerDay = 5, MaxAmountToBuy = 25 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "firewoodTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:firewood", LinearIncreasePerDay = 5, MaxAmountForSale = 20, LinearConsumptionPerDay = 5, MaxAmountToBuy = 20 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "peatTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:dryPeat", LinearIncreasePerDay = 5, MaxAmountForSale = 20, LinearConsumptionPerDay = 5, MaxAmountToBuy = 20 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "bakedFoodTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:hardtack", LinearIncreasePerDay = 5, MaxAmountForSale = 25, LinearConsumptionPerDay = 5, MaxAmountToBuy = 25 }
                }
            });
            
                   

            list.Add(new TradeGroup
            {
                KeyName = "farmToolsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:steelHoe", LinearIncreasePerDay = 1, MaxAmountForSale = 5, LinearConsumptionPerDay = 1, MaxAmountToBuy = 5  },
                     new TradeAmountType(){ EntityType = "item:steelSpade", LinearIncreasePerDay = 1f, MaxAmountForSale = 3, LinearConsumptionPerDay = 0.5f, MaxAmountToBuy = 3  }                   
                }
            });
                                

            list.Add(new TradeGroup
            {
                KeyName = "toolsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                    //these 2 are not tools. but related:
                    new TradeAmountType(){ EntityType = "item:anvil", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  },
                    new TradeAmountType(){ EntityType = "item:barClamps", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  },

                    new TradeAmountType(){ EntityType = "item:steelHoe", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:steelSpade", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:steelPickaxe", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }, 
                    new TradeAmountType(){ EntityType = "item:steelMachete", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }, 
                    new TradeAmountType(){ EntityType = "item:steelHandAxe", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:hammer", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:file", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:handDrill", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:tongs", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:hacksaw", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:blacksmithsToolbox", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:metalWorkersToolbox", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:turnipCracker", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:steelKnife", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },               
                    new TradeAmountType(){ EntityType = "item:metalWire", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },               
                    new TradeAmountType(){ EntityType = "item:rawhideString", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }, 
                    new TradeAmountType(){ EntityType = "item:cottonString", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },               
                    new TradeAmountType(){ EntityType = "item:bulletMold", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },           
                    new TradeAmountType(){ EntityType = "item:shadeleafResin", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 4, MaxAmountToBuy = 4  },     
                }               
            });
           
            list.Add(new TradeGroup
            {
                KeyName = "weaponsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                      new TradeAmountType(){ EntityType = "item:boltActionRifle", LinearIncreasePerDay = 0.4f, LinearConsumptionPerDay = 0.3f, MaxAmountForSale = 5, MaxAmountToBuy = 2  },
                      new TradeAmountType(){ EntityType = "item:gunpowderRifle", LinearIncreasePerDay = 0.7f, LinearConsumptionPerDay = 0.5f, MaxAmountForSale = 5, MaxAmountToBuy = 3  },
                      new TradeAmountType(){ EntityType = "item:musket", LinearIncreasePerDay = 0.7f, LinearConsumptionPerDay = 0.5f, MaxAmountForSale = 5, MaxAmountToBuy = 3  },
                      new TradeAmountType(){ EntityType = "item:musketoon", LinearIncreasePerDay = 0.7f, LinearConsumptionPerDay = 0.5f, MaxAmountForSale = 5, MaxAmountToBuy = 3  },

                      new TradeAmountType(){ EntityType = "item:corditeAmmo", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  },
                      new TradeAmountType(){ EntityType = "item:blackPowderRifleAmmo", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:blackPowderShotAmmo", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:ironArrow", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 6, MaxAmountToBuy = 6  }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "mediumWeaponsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                { 
                       new TradeAmountType(){ EntityType = "item:boltActionRifle", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5f, MaxAmountForSale = 10, MaxAmountToBuy = 10  },
                       new TradeAmountType(){ EntityType = "item:corditeAmmo", LinearIncreasePerDay = 8f, LinearConsumptionPerDay = 8f, MaxAmountForSale = 20, MaxAmountToBuy = 20  },
                  
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "advancedHandWeaponsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {                     
                      new TradeAmountType(){ EntityType = "item:coilRifle", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:shotgun", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                      new TradeAmountType(){ EntityType = "item:coilRifleAmmo", LinearIncreasePerDay = 10f, LinearConsumptionPerDay = 10f, MaxAmountForSale = 35, MaxAmountToBuy = 35  },                      
                      new TradeAmountType(){ EntityType = "item:shotgunAmmo", LinearIncreasePerDay = 7f, LinearConsumptionPerDay = 7f, MaxAmountForSale = 25, MaxAmountToBuy = 25  }                    
                }
            });

            //NA MINING CAMP MATERIAL
            list.Add(new TradeGroup
            {
                KeyName = "rareMetalsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                      // SHOULD BE CHANGED TO THE NEW MATERIAL ITEMS
                      new TradeAmountType(){ EntityType = "item:scandium", LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, MaxAmountForSale = 40, MaxAmountToBuy = 40  },
                      new TradeAmountType(){ EntityType = "item:terbium", LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, MaxAmountForSale = 20, MaxAmountToBuy = 20  },
                      
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "advancedFoodTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                { 
                      new TradeAmountType(){ EntityType = "item:astroRation", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5f, MaxAmountForSale = 50, MaxAmountToBuy = 50  },
                      new TradeAmountType(){ EntityType = "item:simCoffeeBeans", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5f, MaxAmountForSale = 50, MaxAmountToBuy = 50  },
                     /* new TradeAmountType(){ EntityType = "item:crystalWine", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 } },
                      new TradeAmountType(){ EntityType = "item:crystalBrandy", LinearIncreasePerDay = 3, LinearConsumptionPerDay = 3, MaxAmountForSale = 15, MaxAmountToBuy = 15 } },   */   
                }
            });

       
            list.Add(new TradeGroup
            {
                KeyName = "advancedToolsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {                    
                      new TradeAmountType(){ EntityType = "item:advancedKnife", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8 },   
                      new TradeAmountType(){ EntityType = "item:advancedString", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8 },
                      new TradeAmountType(){ EntityType = "item:advancedMachete", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 6, MaxAmountToBuy = 6  },
                      new TradeAmountType(){ EntityType = "item:advancedSnips", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 6, MaxAmountToBuy = 6  },
                      new TradeAmountType(){ EntityType = "item:advancedCookingPot", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 6, MaxAmountToBuy = 6  }                     
                   
                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "advancedEquipmentTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                { 
                      new TradeAmountType(){ EntityType = "item:fieldLabPacked", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 4, MaxAmountToBuy = 4  },   
                      new TradeAmountType(){ EntityType = "item:groundScanner", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 4, MaxAmountToBuy = 4 }, // mp fjernet fordi den ikke har nogen gameplay betydning, mangler testing...
                      new TradeAmountType(){ EntityType = "item:cloak", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 5, MaxAmountToBuy = 5 }, 
                      new TradeAmountType(){ EntityType = "item:nightVisionGoggles", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                      new TradeAmountType(){ EntityType = "item:sensor", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 6, MaxAmountToBuy = 6 },
                      new TradeAmountType(){ EntityType = "item:satelliteGroundStation", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 2, MaxAmountToBuy = 2  },   
                      new TradeAmountType(){ EntityType = "item:octagonalTent", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:smallTent", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:domeTent", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 8, MaxAmountToBuy = 8  },
                      new TradeAmountType(){ EntityType = "item:thermalTarp", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 10, MaxAmountToBuy = 10  },
                      new TradeAmountType(){ EntityType = "item:diamondGlass", LinearIncreasePerDay = 3f, LinearConsumptionPerDay = 3f, MaxAmountForSale = 12, MaxAmountToBuy = 12  },
                      new TradeAmountType(){ EntityType = "item:fieldKitchenStove", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  },
                      new TradeAmountType(){ EntityType = "item:fieldKitchenEquipment", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  },
                      new TradeAmountType(){ EntityType = "entity:haulingRobot", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 3, MaxAmountToBuy = 3  }
                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "advancedMaterialsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:liquidGas", LinearIncreasePerDay = 8f, LinearConsumptionPerDay = 8f, MaxAmountForSale = 20, MaxAmountToBuy = 20  }        
                }
            });

            /*
               #region STRUCTURE ITEMS and PARTS
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_spikeTrap", "item:spikeTrap", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "wharf", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sensor", "item:sensor", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "wharf", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_structurePanels", "item:structurePanels", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "wharf", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_radio", "item:radio", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "wharf", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_radioAntenna", "item:radioAntenna", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "wharf", offerForSale: true));

            #endregion
             */

            list.Add(new TradeGroup
            {
                KeyName = "electronicsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                    new TradeAmountType(){ EntityType = "item:radio", LinearIncreasePerDay = 1f, LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:radioAntenna", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }                         
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "trapsTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                    new TradeAmountType(){ EntityType = "item:spikeTrap", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2f, MaxAmountForSale = 8, MaxAmountToBuy = 8  }                              
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "fishingEquipmentTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {         
                    new TradeAmountType(){ EntityType = "item:strongBugNet", LinearIncreasePerDay = 2f,  LinearConsumptionPerDay = 2f, MaxAmountForSale = 9, MaxAmountToBuy = 9  },
                    new TradeAmountType(){ EntityType = "item:fishTrapHoopNet", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  },
                    new TradeAmountType(){ EntityType = "item:fishTrapBasket", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }, 
                    new TradeAmountType(){ EntityType = "item:fishingNet", LinearIncreasePerDay = 1f,  LinearConsumptionPerDay = 1f, MaxAmountForSale = 5, MaxAmountToBuy = 5  }, 
                    new TradeAmountType(){ EntityType = "item:ironHooks", LinearIncreasePerDay = 5f,  LinearConsumptionPerDay = 5f, MaxAmountForSale = 15, MaxAmountToBuy = 15  },                                  
                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "foodTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType = "item:hardtack", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 20, MaxAmountToBuy = 20  },
                     new TradeAmountType(){ EntityType = "item:fermentedFingerFruit", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:driedBeef", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 20, MaxAmountToBuy = 20  }, 
                     new TradeAmountType(){ EntityType = "item:waterCaneSeeds", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:smokedTurnip", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:turnipSalami", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 15, MaxAmountToBuy = 15  }, 
                     new TradeAmountType(){ EntityType = "item:smokedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:driedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:pickledAlabasterRay", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 25, MaxAmountToBuy = 15  }, 
                     new TradeAmountType(){ EntityType = "item:smokedAlabasterRay", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:smokedStreakFin", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:driedSaltedStreakFin", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType = "item:smokedCarbonTail", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType = "item:pickledCarbonTail", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 15, MaxAmountToBuy = 15  },
                     new TradeAmountType(){ EntityType = "item:commonOilTubers", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType = "item:glassyCreeperPods", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType = "item:crystalBerries", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 18, MaxAmountToBuy = 18  },
                     new TradeAmountType(){ EntityType = "item:powderedCrystalBerries", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 18, MaxAmountToBuy = 18  }, 

                }
            });

           /* list.Add(new TradeGroup
            {
                KeyName = "preparedLandFoodTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:hardtack", LinearIncreasePerDay = 5, MaxAmountForSale = 25  },                   
                     new TradeAmountType(){ EntityType = "item:smokedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:driedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:smokedTurnip", LinearIncreasePerDay = 4, MaxAmountForSale = 15  }}                  
                     new TradeAmountType(){ EntityType = "item:fermentedFingerFruit", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType = "item:smokedTurnip", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:turnipSalami", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 15, MaxAmountToBuy = 15  }}
                }
            });*/

            list.Add(new TradeGroup
            {
                KeyName = "bakedTrade",
                AvailableForTrade = new[] //new SerializableDictionary<string, TradeAmountType>()
                {
                      new TradeAmountType(){ EntityType = "item:hardtack", LinearIncreasePerDay = 5, MaxAmountForSale = 25  }     
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "thunderChickenTrade",
                AvailableForTrade = new[] // new SerializableDictionary<string, TradeAmountType>()
                {
                     new TradeAmountType(){ EntityType= "item:smokedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                     new TradeAmountType(){ EntityType= "item:driedThunderChicken", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },                   
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "turnipTrade",
                AvailableForTrade = new[] // new SerializableDictionary<string, TradeAmountType>()
                {                    
                    new TradeAmountType(){ EntityType= "item:smokedTurnip",  LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  },
                    new TradeAmountType() { EntityType = "item:turnipSalami", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 15, MaxAmountToBuy = 15  }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "textileTrade",
                AvailableForTrade = new []
                {                    
                     new TradeAmountType(){ EntityType = "item:textile", LinearIncreasePerDay = 2f, LinearConsumptionPerDay = 2,  MaxAmountForSale = 10, MaxAmountToBuy = 10  },
                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "hidesTrade",
                AvailableForTrade = new[]
                {
                     new TradeAmountType(){ EntityType = "item:megapodRawhide", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:megapodTannedHide", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:thunderChickenRawhide", LinearIncreasePerDay = 2, LinearConsumptionPerDay = 2, MaxAmountForSale = 10, MaxAmountToBuy = 10 },
                     new TradeAmountType(){ EntityType = "item:thunderChickenTannedHide", LinearIncreasePerDay = 2, LinearConsumptionPerDay = 2, MaxAmountForSale = 10, MaxAmountToBuy = 10 },
                     new TradeAmountType(){ EntityType = "item:whipjawRawhide", LinearIncreasePerDay = 2, LinearConsumptionPerDay = 2, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:whipjawTannedHide", LinearIncreasePerDay = 2, LinearConsumptionPerDay = 2, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:improvisedGreenHouseCover", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 2, MaxAmountToBuy = 5 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "cropsTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:cotton", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:glassyCreeperPods", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:crystalBerries", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 18, MaxAmountToBuy = 18  },                    
                     new TradeAmountType(){ EntityType = "item:fingerFruit", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 18, MaxAmountToBuy = 18  }                    
                }
            });

            list.Add(new TradeGroup
            {                
                KeyName = "fishTrade", // RAW is not realistic without refrigeration, which is not common on descent era maps
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:pickledAlabasterRay", LinearIncreasePerDay = 4f, LinearConsumptionPerDay = 4,  MaxAmountForSale = 25, MaxAmountToBuy = 15  }, 
                     new TradeAmountType(){ EntityType = "item:smokedAlabasterRay", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:smokedStreakFin", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:driedSaltedStreakFin", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:smokedCarbonTail", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 25, MaxAmountToBuy = 25  }, 
                     new TradeAmountType(){ EntityType = "item:pickledCarbonTail", LinearIncreasePerDay = 5f, LinearConsumptionPerDay = 5,  MaxAmountForSale = 15, MaxAmountToBuy = 15  },

                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "mineralsTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:bogOre", LinearIncreasePerDay = 10, LinearConsumptionPerDay = 10, MaxAmountForSale = 40, MaxAmountToBuy = 40 },
                     new TradeAmountType(){ EntityType = "item:goldOre", LinearIncreasePerDay = 10, LinearConsumptionPerDay = 10, MaxAmountForSale = 40, MaxAmountToBuy = 40 },
                     new TradeAmountType(){ EntityType = "item:sulfurPowder", LinearIncreasePerDay = 6, LinearConsumptionPerDay = 6, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:salt", LinearIncreasePerDay = 6, LinearConsumptionPerDay = 6, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:saltpeter", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 12, MaxAmountToBuy = 12 },

                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "metalsTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:roughBloomIron", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:wroughtIron", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:blisterSteel", LinearIncreasePerDay = 4, LinearConsumptionPerDay = 4, MaxAmountForSale = 14, MaxAmountToBuy = 14 },
                     new TradeAmountType(){ EntityType = "item:gold", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 }

                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "fuelTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:firewood", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:dryPeat", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:charcoal", LinearIncreasePerDay = 4, LinearConsumptionPerDay = 4, MaxAmountForSale = 20, MaxAmountToBuy = 20 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "plasticsTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:marshcotSap", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:gaskets", LinearIncreasePerDay = 4, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },                    
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "animalsTrade",
                AvailableForTrade = new[]
              {
                     new TradeAmountType(){ EntityDataKey = "dog", LinearIncreasePerDay = 2, LinearConsumptionPerDay = 2, MaxAmountForSale = 8, MaxAmountToBuy = 8 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "industrialComponentsTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:loomComponents", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:stillComponents", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:extrusionMachineComponents", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:humanPowerUnit", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },                
                     new TradeAmountType(){ EntityType = "item:humanPowerUnitComponents", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 },
                     new TradeAmountType(){ EntityType = "item:metalLatheComponents", LinearIncreasePerDay = 1, LinearConsumptionPerDay = 1, MaxAmountForSale = 5, MaxAmountToBuy = 5 }               
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "containersTrade",
                AvailableForTrade = new []
                {
                     new TradeAmountType(){ EntityType = "item:clayJar", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:plasticTappingBucket", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                     new TradeAmountType(){ EntityType = "item:tappingBucket", LinearIncreasePerDay = 4, LinearConsumptionPerDay = 4, MaxAmountForSale = 20, MaxAmountToBuy = 20 },                   
                     new TradeAmountType(){ EntityType = "item:goldPot", LinearIncreasePerDay = 4, LinearConsumptionPerDay = 4, MaxAmountForSale = 20, MaxAmountToBuy = 20 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "comfortTrade",
                AvailableForTrade = new []
                {         
                    new TradeAmountType(){ EntityType = "item:crystalWine", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 20, MaxAmountToBuy = 20 },
                    new TradeAmountType(){ EntityType = "item:crystalBrandy", LinearIncreasePerDay = 5, LinearConsumptionPerDay = 5, MaxAmountForSale = 15, MaxAmountToBuy = 15 },                  
                }
            });

           

            #region Occasional items - appear and disappear

            list.Add(new TradeGroup
            {
                KeyName = "occasionalAdvancedTradeItems",
                OfferDemandProfile = "occasionallyOfferedGood", // instead of linear increase/decrease
                AvailableForTrade = new []
                { 
                      new TradeAmountType(){ EntityType = "item:fieldLabPacked", MaxAmountForSale = 2, MaxAmountToBuy = 2  },
                      new TradeAmountType(){ EntityType = "item:coilRifle", MaxAmountForSale = 2, MaxAmountToBuy = 2  },
                      new TradeAmountType(){ EntityType = "item:sentry", MaxAmountForSale = 2, MaxAmountToBuy = 2 },
                      new TradeAmountType(){ EntityType = "entity:haulingRobot", MaxAmountForSale = 2, MaxAmountToBuy = 2 },
                      new TradeAmountType(){ EntityType = "item:shotgun", MaxAmountForSale = 2, MaxAmountToBuy = 2 },    
               //       new TradeAmountType(){ EntityType = "item:groundScanner", new TradeAmountType(){ MaxAmountForSale = 2, MaxAmountToBuy = 2 }}, mp fjernet fordi den ikke har nogen gameplay betydning, mangler testing...
                      new TradeAmountType(){ EntityType = "item:cloak", MaxAmountForSale = 2, MaxAmountToBuy = 2 }, 
                      new TradeAmountType(){ EntityType = "item:nightVisionGoggles", MaxAmountForSale = 2, MaxAmountToBuy = 2 },
                      new TradeAmountType() { EntityType = "item:sensor", MaxAmountForSale = 3, MaxAmountToBuy = 3 },
                      new TradeAmountType() { EntityType = "item:advancedKnife", MaxAmountForSale = 3, MaxAmountToBuy = 3 },   
                      new TradeAmountType() { EntityType = "item:advancedString", MaxAmountForSale = 3, MaxAmountToBuy = 3 }
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "occasionalAdvancedAmmoTradeItems",
                OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity", // instead of linear increase/decrease
                AvailableForTrade = new []
                {                     
                      new TradeAmountType(){ EntityType = "item:coilRifleAmmo", MaxAmountForSale = 8, MaxAmountToBuy = 6  },
                      new TradeAmountType(){ EntityType = "item:sentryGunAmmo", MaxAmountForSale = 5, MaxAmountToBuy = 5  },    
                      new TradeAmountType(){ EntityType = "item:shotgunAmmo", MaxAmountForSale = 5, MaxAmountToBuy = 5  },    
                }
            });


            list.Add(new TradeGroup
            {
                KeyName = "occasionalEquipmentTradeItems",
                Comments = "in planet fall era, medium/basic tier industrial items should be rare (this is opposite from descent era)",
                OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity", // instead of linear increase/decrease
                
                AvailableForTrade = new []
                {         
                    new TradeAmountType(){ EntityType = "item:loomComponents", MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:stillComponents", MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:extrusionMachineComponents", MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:humanPowerUnit", MaxAmountForSale = 4, MaxAmountToBuy = 4 },                
                     new TradeAmountType(){ EntityType = "item:humanPowerUnitComponents", MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:metalLatheComponents", MaxAmountForSale = 4, MaxAmountToBuy = 4 },
                     new TradeAmountType(){ EntityType = "item:bellows", MaxAmountForSale = 4, MaxAmountToBuy = 4 }, 
                     new TradeAmountType(){ EntityType = "item:turnipCracker", MaxAmountForSale = 4, MaxAmountToBuy = 4 }                   
                }
            });

            list.Add(new TradeGroup
            {
                KeyName = "occasionallyOfferedSeeds",             
                OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity", // instead of linear increase/decrease

                AvailableForTrade = new []
                {      
                     new TradeAmountType(){ EntityType = "item:cotton", MaxAmountForSale = 8, MaxAmountToBuy = 8 },  // make sure cotton is available sometimes..
                     new TradeAmountType(){ EntityType = "item:glassyCreeperPods", MaxAmountForSale = 8, MaxAmountToBuy = 8 }, 
                     new TradeAmountType(){ EntityType = "item:crystalBerries", MaxAmountForSale = 8, MaxAmountToBuy = 8 }, 
                     new TradeAmountType(){ EntityType = "item:fingerFruit", MaxAmountForSale = 8, MaxAmountToBuy = 8 }, 
                }
            });

            #endregion

            return list;

        }

    }
}

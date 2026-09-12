using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.AllGameData
{
    public class ResourceLoader
    {
        public static List<ResourceType> Init()
        {
            List<ResourceType> listOfResourceTypes = new List<ResourceType>();


            float commonResourcesFlashDuration = 500f;


            ResourceType tileResourceType = new ResourceType("firewood")
            {
                ResourceItem = "item:firewood",
                Name = "Firewood",
                Color = Color.PaleVioletRed,

                FractionOfMaximumToReplenishEachTime = 0.5f, 
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.45f, StandardDeviation = 0.03f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.03f } }, 
             
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //"smallAboveGround", 
                TileResourceType = new TileResourceType()  //it places only one png on each tile. either small or large quantity
                {
                    MaxFlavours = 4,
                    MoreSpriteLimit = 3f,
                    RenderableType = new RenderableType()
                    {
                        // default is None:
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                      // NONE
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)   
                                     },
                                      // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_more_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_more_3" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.More)   
                                     },

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_less_4" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_less_5" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_less_6" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName =  "tileresource_firewood_less_7" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Less)   
                                     }
                                    
                                 }

                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            //DaysOfYearToReplenish      replenish several times a year by 50%  (FractionOfMaximumToReplenishEachTime = 0.5f)
            NormalDistribution[] shellfishReplenish = new[] { 
                new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.02f }, 
                new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.02f }, 
                new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.02f }, 
                new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.02f },
                new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.02f }};


            NormalDistribution[] fishReplenish = new[] { 
                new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.01f }, 
                new NormalDistribution() { Mean = 0.1f, StandardDeviation = 0.01f }, 
                new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.01f }, 
                new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.01f }, 
                new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.01f },
                new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.01f },
                new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.01f },
                new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.01f },  
                new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.01f },  
                new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.01f }}; 
 

            tileResourceType = new ResourceType("clamwich")
            {
                ResourceItem = "item:clamwich",
                Name = "Clamwich",
                Color = Color.LimeGreen,
                Category = GameData.Instance.AllResourceCategories["food"],

                FractionOfMaximumToReplenishEachTime = 0.5f, 
                DaysOfYearToReplenish = shellfishReplenish,      //new[] { new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f } }, 
             
                DetectionTag = "smallAboveGround",                        //  defined further up in gamedataloader
                TileResourceType = new TileResourceType()  //it places only one png on each tile. eather small or large quantity
                {
                    MaxFlavours = 3,
                    MoreSpriteLimit = 2f,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                      // NONE
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },  
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)   
                                     }, 
                                     // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "tileresource_clamwich_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "tileresource_clamwich_more_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },  
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_clamwich_more_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.More)   
                                     }, 

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_clamwich_less_3" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_clamwich_less_4" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_clamwich_less_5" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Less)   
                                     }                                  
                                 }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("sulfurDeposit")
            {
                ResourceItem = "item:sulfurBlocks",
                Name = "Sulfur",
                Color = Color.Yellow,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround",                        //  defined further up in gamedataloader
                TileResourceType = new TileResourceType()  //it places only one png on each tile. eather small or large quantity
                {
                    MaxFlavours = 3,
                    MoreSpriteLimit = 2f,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                      // NONE
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },  
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)   
                                     }, 
                                     // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "tileresource_sulfur_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "tileresource_sulfur_more_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },  
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_sulfur_more_3" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.More)   
                                     }, 

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_sulfur_less_4" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_sulfur_less_5" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_sulfur_less_6" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Less)   
                                     }                                  
                                 }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("torux")
            {
                ResourceItem = "item:torux",
                Name = "Torux",
                Color = Color.LightSteelBlue,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAboveGround",                        //AvoidDetectionFactor = 0.3f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = shellfishReplenish, 
             
                TileResourceType = new TileResourceType()  //it places only one png on each tile. eather small or large quantity
                {
                    MaxFlavours = 2,
                    MoreSpriteLimit = 2f,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                     // NONE
                                     new ClientStateInfo() 
                                     {
                                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },  
                                     // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_torux_more_3" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_torux_more_4" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },                                   

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_torux_less_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_torux_less_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     }                                  
                                 }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            #region "blackpulp"
            tileResourceType = new ResourceType("blackpulp")
            {
                ResourceItem = "item:blackpulp",
                Name = "Blackpulp",
                Color = Color.LightSeaGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAboveGround",     // where's this defined?? MP                   //AvoidDetectionFactor = 0.7f,
                FractionOfMaximumToReplenishEachTime = 1.0f,
                DaysOfYearToReplenish = fishReplenish,   //mp oct 2016 i boosted this so that the player can have a standing gather order on blackpulp when farming favorbread.          
                TileResourceType = new TileResourceType()  //it places only one png on each tile. eather small or large quantity
                {
                    MaxFlavours = 2,
                    MoreSpriteLimit = 2f,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                     // NONE
                                     new ClientStateInfo() 
                                     {                                                                    
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {                                                       
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },   
                                     // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_blackpulp_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_blackpulp_more_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },                                   

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_blackpulp_less_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_blackpulp_less_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     }                                  
                                 }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);
            #endregion

            #region favorbread
            tileResourceType = new ResourceType("favorbread")
            {
                ResourceItem = "item:favorbread",
                Name = "Favorbread",
                Color = Color.BlueViolet,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAboveGround",     // 
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.55f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f } },
                TileResourceType = new TileResourceType()  //it places only one png on each tile. eather small or large quantity
                {
                    MaxFlavours = 2, //bug: doesnt randomize between less 1 and less 2
                    MoreSpriteLimit = 1.9f, //bug: doesnt show the 'more' sprite
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { },
                        ClientStateConditions = new[]
                                 {
                                     // NONE
                                     new ClientStateInfo() 
                                     {                                                                    
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)   
                                     },
                                     new ClientStateInfo() 
                                     {                                                       
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)   
                                     },   
                                     // MORE
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_favorbread_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.More)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_favorbread_more_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.More)   
                                     },                                   

                                     // LESS
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_favorbread_less_1" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Less)   
                                     },
                                     new ClientStateInfo() 
                                     {
                                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){  AssetName = "tileresource_favorbread_less_2" },                                 
                                          Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Less)   
                                     }                                  
                                 }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);
            #endregion

            tileResourceType = new ResourceType("podlac") //maybe use the above system, with broken apart podlac structures? have to choose between ground sprite and billboards (=trees)though.
            {
                ResourceItem = "item:podlacUnrefined",
                Name = "Podlac",
                Color = Color.BurlyWood,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("podlac")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "charcoal", // needs asset?
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("bogOre") //bso. mp :   was called "ore:bogOre" for some reason.
            {
                ResourceItem = "item:bogOre",
                Name = "Bog ore",
                Color = Color.HotPink,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //maybe create a new tag
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("bogOre")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "ore_s", // needs asset?
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("goldOre") //
            {
                ResourceItem = "item:goldOre",
                Name = "Gold ore",
                Color = Color.Gold,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //maybe create a new tag
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("bogOre") //placeholder asset
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "ore_s", // needs asset?
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };

            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("clay") //bso
            {
                ResourceItem = "item:clay",
                Name = "Clay",
                Color = Color.Blue,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //maybe create a new tag
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("clay")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "soil_s", // needs asset?
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("salt") //
            {
                ResourceItem = "item:salt",
                Name = "Salt",
                Color = Color.White,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround", //
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("salt")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "saltpeterPowder", // mp i dont think this is used anywhere, correct?
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("stones")
            {
                ResourceItem = "item:stones",
                Name = "Stones",
                Color = Color.LemonChiffon,
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround",                        //AvoidDetectionFactor = 0.1f,
                TileResourceType = new TileResourceType()
                {

                    RenderableType = new RenderableType("stones")
                    {

                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "stones",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }

            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("smoothSandstone")
            {
                ResourceItem = "item:smoothSandstone",
                Name = "Sharpening stones",
                Color = Color.Black,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAboveGround",                        //AvoidDetectionFactor = 0.1f,
                TileResourceType = new TileResourceType()
                {

                    RenderableType = new RenderableType("smoothSandstone")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "smoothSandstone",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("flint")
            {
                ResourceItem = "item:flintRough",
                Name = "Flint",
                Color = Color.White,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAboveGround",                        //AvoidDetectionFactor = 0.1f,
                TileResourceType = new TileResourceType()
                {

                    RenderableType = new RenderableType("smoothSandstone") //mp: what is this? no asset named thus.
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "smoothSandstone",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);



            tileResourceType = new ResourceType("firegrassSod")
            {
                ResourceItem = "item:firegrassSod",
                Name = "Firegrass sod",
                Color = Color.Indigo,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f } }, 
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("firegrassSod")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "firegrassSod",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("vine")
            {
                ResourceItem = "item:vine",
                Name = "Vine",
                Color = Color.Indigo,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeOnGround",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 0.7f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("vine")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "vine",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


/*
            tileResourceType = new ResourceType("sulfurDeposit")
            {
                ResourceItem = "item:sulfurBlocks",
                Name = "Sulfur deposit",
                Color = Color.Yellow,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAboveGround",                        //AvoidDetectionFactor = 0.1f,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("sulfurDeposit")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "sulfurDeposit",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);
*/
            tileResourceType = new ResourceType("guanoDeposit")
            {
                ResourceItem = "item:guano",
                Name = "Guano deposit",
                Color = Color.Blue,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                FractionOfMaximumToReplenishEachTime = 0.5f, //around 50% replenished per year.
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.25f, StandardDeviation = 0.1f } }, 
                DetectionTag = "fruitOnGroundHardToFind",                        //AvoidDetectionFactor = 0.1f,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("guanoDeposit")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "guanoDeposit",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);





          
            
 


            listOfResourceTypes.Add(new ResourceType("minnowsLive")
            {
                ResourceItem = "item:minnowsLive",
                Name = "Minnows",
                Color = Color.Azure,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "inShallowWater",                        //AvoidDetectionFactor = 0.3f,
                FractionOfMaximumToReplenishEachTime = 1f, 
                DaysOfYearToReplenish = fishReplenish,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("minnowsLive")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "minnowsLive",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            });
           
            listOfResourceTypes.Add(new ResourceType("alabasterRay")
            {
                ResourceItem = "item:alabasterRay",
                Name = "Alabaster ray",
                Color = Color.Fuchsia,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "inDeeperWater",                        //AvoidDetectionFactor = 0.7f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = fishReplenish,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("alabasterRay")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "alabasterRay",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            });
           
            listOfResourceTypes.Add(new ResourceType("streakFin")
            {
                ResourceItem = "item:streakFin",
                Name = "Streak fin",
                Color = Color.Gold,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "inDeeperWater",                        //AvoidDetectionFactor = 0.8f,
                FractionOfMaximumToReplenishEachTime = 1f, 
                DaysOfYearToReplenish = fishReplenish,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("streakFin")
                    {

                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "streakFin",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            });
          
            listOfResourceTypes.Add(new ResourceType("carbonTail")
            {
                ResourceItem = "item:carbonTail",
                Name = "Carbon tail",
                Color = Color.DarkSeaGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "inDeeperWater",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = fishReplenish,
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("carbonTail")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "carbonTail",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            });

            tileResourceType = new ResourceType("neonHornets")
            {
                ResourceItem = "item:neonHornetsLive",
                Name = "Neon hornets",
                Color = Color.DeepSkyBlue,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.3f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("neonHornets")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "neonHornets",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            /*          tileResourceType = new ResourceType("neonHornets")  //     ...... made into crop on copperfern
                      {
                          ResourceItem = "item:neonHornetsLive"),
                          Name = "Pig flies",
                          Color = Color.DeepPink,
                          Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                          DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.4f,
                          TileResourceType = new TileResourceType()
                          {
                          }
                      };
                      listOfResourceTypes.Add(tileResourceType);
  */
            tileResourceType = new ResourceType("stinkpup")  //live in deep caves, can be angled with neon hornet bait
            {
                ResourceItem = "item:stinkpup",
                Name = "Stinkpup",
                Color = Color.DarkTurquoise,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGroundHardToSee",
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("stinkpup")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "stinkpup",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            
          
            tileResourceType = new ResourceType("phantomWeaver")
            {
                ResourceItem = "item:phantomWeaver",
                Name = "Phantom weaver",
                Color = Color.DarkSalmon,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGroundHardToSee",                        //AvoidDetectionFactor = 0.95f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("phantomWeaver")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "phantomWeaver",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("ursinix")
            {
                ResourceItem = "item:ursinix",
                Name = "Ursinix",
                Color = Color.DarkSalmon,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitOnGroundHardToFind",    //    "smallAnimalBelowGround"       
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("ursinix")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "ursinix",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("webWing")
            {
                ResourceItem = "item:webWing",
                Name = "Web wing",
                Color = Color.DarkOrange,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("webWing")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "webWing",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("crestedFoiler")
            {
                ResourceItem = "item:crestedFoiler",
                Name = "Crested foiler",
                Color = Color.DarkOliveGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("crestedFoiler")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "crestedFoiler",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("goldenCenobite")
            {
                ResourceItem = "item:goldenCenobite",
                Name = "Golden cenobite",
                Color = Color.DarkGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("goldenCenobite")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "goldenCenobite",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);



            tileResourceType = new ResourceType("treeScuttler")
            {
                ResourceItem = "item:treeScuttler",
                Name = "Tree scuttler",
                Color = Color.Cornsilk,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("treeScuttler")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "treeScuttler",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);



            tileResourceType = new ResourceType("muckGrinder")
            {
                ResourceItem = "item:muckGrinder",
                Name = "Muck grinder",
                Color = Color.BurlyWood,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalOnGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("muckGrinder")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "muckGrinder",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("spriteSlug")
            {
                ResourceItem = "item:spriteSlug",
                Name = "Sprite slug",
                Color = Color.Chocolate,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallHidden",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("spriteSlug")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "spriteSlug",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("crazyDweller")
            {
                ResourceItem = "item:crazyDweller",
                Name = "Crazy dweller",
                Color = Color.Coral,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalOnGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("crazyDweller")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "crazyDweller",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("daggermouth")
            {
                ResourceItem = "item:daggermouth",
                Name = "Daggermouth",
                Color = Color.Crimson,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalOnGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("daggermouth")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "daggermouth",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("impEel")
            {
                ResourceItem = "item:impEel",
                Name = "Imp eel",
                Color = Color.Crimson,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalBelowGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("impEel")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "impEel",
                            IconToRender = IconToRender.Hook
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            //

            tileResourceType = new ResourceType("scampBeetle")
            {
                ResourceItem = "item:scampBeetle",
                Name = "Scamp beetle",
                Color = Color.DarkBlue,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalOnGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("scampBeetle")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "scampBeetle",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("scampGrub")
            {
                ResourceItem = "item:scampGrub",
                Name = "Scamp grub",
                Color = Color.AliceBlue,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "smallAnimalBelowGround",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("scampGrub")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "scampGrub",
                            IconToRender = IconToRender.Bug
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

             

            //update maps
            tileResourceType = new ResourceType("spottedOilTubers")
            {
                ResourceItem = "item:spottedOilTubers",
                Name = "Spotted oil tubers",
                Color = Color.Azure,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitOnGroundHardToFind",                        //AvoidDetectionFactor = 0.8f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("spottedOilTubers")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "spottedOilTubers",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("commonOilTubers")
            {
                ResourceItem = "item:commonOilTubers",
                Name = "Common oil tubers",
                Color = Color.Aquamarine,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitOnGroundHardToFind",                        //AvoidDetectionFactor = 0.8f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("commonOilTubers")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "commonOilTubers",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("hexapineLeaves")
            {
                ResourceItem = "item:hexapineLeaves",
                Name = "Hexapine leaves",
                Color = Color.LawnGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitOnGroundHardToFind",                        //AvoidDetectionFactor = 0.8f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("hexapineLeaves")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "hexapineLeaves",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);

            tileResourceType = new ResourceType("fingerFruit")
            {
                ResourceItem = "item:fingerFruit",
                Name = "Finger fruit",
                Color = Color.CadetBlue,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitOnGroundHardToFind",                        //AvoidDetectionFactor = 0.8f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("fingerFruit")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "fingerFruit",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("glassyCreeperPods")
            {
                ResourceItem = "item:glassyCreeperPods",
                Name = "Glassy creeper pods",
                Color = Color.LavenderBlush,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } }, 
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("glassyCreeperPods")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "glassyCreeperPods",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);


            tileResourceType = new ResourceType("crystalBerries")
            {
                ResourceItem = "item:crystalBerries",
                Name = "Crystal berries",
                Color = Color.LawnGreen,
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "fruitAboveGround",                        //AvoidDetectionFactor = 0.5f,
                FractionOfMaximumToReplenishEachTime = 1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
              
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("crystalBerries")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "crystalBerries",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }
            };
            listOfResourceTypes.Add(tileResourceType);






            /* //mp feb 2015. not used, made into ground resource instead 
                                resourceType = new ResourceType("crop:blackpulp")


// mp april 2015. made into ground resource. we have to choose, else there's duplication on the gather window.
            resourceType = new ResourceType("crop:vine")
*/


            ResourceType resourceType = new ResourceType("crop:sticks")
            {
                Name = "Sticks",
                ResourceItem = "item:sticks",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionFlashDuration = commonResourcesFlashDuration,
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 0.5f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.75f, StandardDeviation = 0.05f } }, 
                CropType =
                new CropType()
                {
                  /*  CropBulkGrowthPerDayStandardDeviation = 0.01f,
                    CropBulkGrowthPerDayMean = 0.08f,*/
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                 //   RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("crop:spoakBranches")
            {
                Name = "Spoak branches",
                ResourceItem = "item:spoakBranches",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionFlashDuration = commonResourcesFlashDuration,
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.1f,
                FractionOfMaximumToReplenishEachTime = 0.5f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.05f } },
                CropType =
                new CropType()
                {                 
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    BulkLimitToShowFlag = 0f,
                    TreeSpriteFlag = StateModifier.HasBranches, //TreeSpriteFlag is used so that we can have multiple resources and flags
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);



            resourceType = new ResourceType("crop:waterCaneLeaves")
            {
                Name = "Water cane leaves",
                ResourceItem = "item:waterCaneLeaves",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.2f,
                DetectionFlashDuration = commonResourcesFlashDuration,
                FractionOfMaximumToReplenishEachTime = 1f, //default
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.05f } },
                CropType =
                new CropType()
                {                   
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                //    RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:waterCaneStem")
            {
                Name = "Water cane stem",
                ResourceItem = "item:waterCaneStem",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime = 1f, //boosted growth because used a lot.
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.45f, StandardDeviation = 0.03f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.03f } }, //was: once per year ,mp replenishes in the summer 0.5f
                CropType =
                new CropType()
                {                 
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                //    RipeSpeed = 0.6f,
                    BulkLimitToShowFlag = 0f,
                    TreeSpriteFlag = StateModifier.HasBranches, //makes the sprite state change
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:waterCaneSeeds")
            {
                Name = "Water cane seeds",
                ResourceItem = "item:waterCaneSeeds",
                Category = GameData.Instance.AllResourceCategories["food"],
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.2f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.0f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.8f, StandardDeviation = 0.05f } },
                CropType =
                new CropType()
                {                
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
              //      RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:pigFlies")
            {
                Name = "Pig flies",
                ResourceItem = "item:pigFliesLive",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAnimalAboveGround",
                DaysOfYearToReplenish =  new[]{ new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.05f }, new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.05f }}, 
                CropType =
                new CropType()
                {                  
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
               //     RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }/*
                TileResourceType = new TileResourceType()
                {
                    RenderableType = new RenderableType("crop:pigFlies")
                    {
                        RenderAsIconType = new RenderAsIconType()
                        {
                            AssetName = "crystalBerries",
                            IconToRender = IconToRender.Ore
                        }
                    }
                }*/
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:shadeleafBowStave")
            {
                Name = "Shadeleaf bow stave",
                ResourceItem = "item:shadeleafBowStave",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "aboveGroundHardToSee",     //                   // 
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.02f } }, //mp replenishes in summer and autumn
                CropType =
                new CropType()
                {                 
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
              //      RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("crop:shadeleafCanes")
            {
                Name = "Shadeleaf canes",
                ResourceItem = "item:shadeleafCanes",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.1f,
                DetectionFlashDuration = commonResourcesFlashDuration,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.02f } }, //mp replenishes in summer and autumn
                CropType =
                new CropType()
                {                  
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                //    RipeSpeed = 0.6f,
                    BulkLimitToShowFlag = 0f,
                    TreeSpriteFlag = StateModifier.HasBranches, //makes the sprite state change
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:shadeleafResin") //mp currently only found on dead shadeleaf trees, make sure to always have a few dead trees together with the rest.
            {
                Name = "Shadeleaf resin",
                ResourceItem = "item:shadeleafResin",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "smallAboveGround",                        //AvoidDetectionFactor = 0.8f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.02f } }, //mp replenishes in summer and autumn
                CropType =
                new CropType()
                {                 
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:giantHollowBud")
            {
                Name = "Bud from giant hollow",
                ResourceItem = "item:giantHollowBud",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                        //AvoidDetectionFactor = 0.1f,
                DetectionFlashDuration = commonResourcesFlashDuration,
                DaysOfYearToReplenish =  new[]{ new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.05f }}, //mp replenishes in the summer 0.5f
                CropType =
                new CropType()
                {                  
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:daysheenLeaves")
            {
                Name = "Daysheen leaves",
                ResourceItem = "item:daysheenLeaves",
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                         //AvoidDetectionFactor = 0.1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.02f } },
                CropType =
                new CropType()
                {                   
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    BulkLimitToShowFlag = 0f,
                    TreeSpriteFlag = StateModifier.HasBranches, //makes the sprite state change
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) } //not used currently may 2015
                }
            };
            listOfResourceTypes.Add(resourceType);
/*NOT USED
            resourceType = new ResourceType("crop:thorns")
            {
                Name = "Thorns",
                ResourceItem = "item:thorns",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                         //AvoidDetectionFactor = 0.1f,
                DaysOfYearToReplenish =  new[]{ new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.05f }}, //mp replenishes in the summer 0.5f
                CropType =
                new CropType()
                {                 
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);
*/

            resourceType = new ResourceType("crop:marshcotSap")
            {
                Name = "Marshcot sap",
                ResourceItem = "item:marshcotSap",
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "aboveGroundHardToSee",                        //AvoidDetectionFactor = 0.2f,
                FractionOfMaximumToReplenishEachTime =0.7f,
                DaysOfYearToReplenish = fishReplenish,

                CropType =
                new CropType()
                {                   
                    MaxSizeShareOfWholePlant = 0.05f,
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);


            resourceType = new ResourceType("crop:wingweedLeaves")
            {
                Name = "Wingweed leaves",
                ResourceItem = "item:wingweedLeaves",
                DetectionFlashDuration = commonResourcesFlashDuration,
                Category = GameData.Instance.AllResourceCategories["rawMaterials"],
                DetectionTag = "largeAboveGround",                      //  AvoidDetectionFactor = 0.1f,
                DaysOfYearToReplenish = new[] { new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f }, new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.02f } }, //mp replenishes in summer and autumn
                CropType =
                new CropType()
                {
                    DetectionPulsingDuration = 500f,                 
                    MaxSizeShareOfWholePlant = 0.7f,  //not used right now april 2013
                    CropItemGrowthPerDay = 0.3f,
                    //RipeSpeed = 0.6f,
                    BulkLimitToShowFlag = 0f,
                    TreeSpriteFlag = StateModifier.HasBranches, //makes the sprite state change
                    AgeProduction = new Vector2[] { new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(8f, 1f), new Vector2(10f, 0.5f) }
                }
            };
            listOfResourceTypes.Add(resourceType);



            /* no longer needed.
            #region Carcasses

            resourceType = new ResourceType("patriciancarcass") { Name = "Patrician carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:patricianCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("demontreecarcass") { Name = "Tree demon carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:demontreeCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("swampDemonTreeCarcass") { Name = "Swamp Tree demon carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:swampDemonTreeCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("spikePlantCarcass") { Name = "Ursinix carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:spikePlantCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("wormCarcass") { Name = "Worm carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:wormCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("whipjawCarcass") { Name = "Whipjaw carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:whipjawCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("turnipcarcass") { Name = "Turnip carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:turnipCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("birdcarcass") { Name = "Bird carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:birdCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("twinklercarcass") { Name = "Twinkler carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:quaditeCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("leafcutterCarcass") { Name = "Leafcutter carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:leafcutterCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("forestguardiancarcass") { Name = "Forest guardian carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:forestGuardianCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("bushdragoncarcass") { Name = "Bush dragon carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:bushDragonCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("thunderchickencarcass") { Name = "Thunder chicken carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:thunderChickenCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("thinThunderChickenCarcass") { Name = "Bajingan Carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:thinThunderChickenCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("pygmyThunderChickenCarcass") { Name = "Pygmy Thunder Chicken Carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:pygmyThunderChickenCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("bulkyThunderChickenCarcass") { Name = "Bulky Thunder Chicken Carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:bulkyThunderChickenCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("binalratcarcass") { Name = "Binal rat carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:binalRatCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("dogcarcass") { Name = "Dog carcass", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:dogCarcass" };
            listOfResourceTypes.Add(resourceType);

            resourceType = new ResourceType("body") { Name = "Body", Category = GameData.Instance.AllResourceCategories["carcasses"], ResourceItem = "item:body" };
            listOfResourceTypes.Add(resourceType);

            #endregion
            */

            /*      
                    ResourceCategory[] categories = { GameData.Instance.AllResourceCategories["rawMaterials"], GameData.Instance.AllResourceCategories["food"], GameData.Instance.AllResourceCategories["carcasses"] };
                    foreach (ResourceCategory cat in categories)
                    {
                        System.Diagnostics.Debug.WriteLine("Category: " + cat.Name);
                        foreach (ResourceType rt in listOfResourceTypes)
                        {
                            if (rt.Category == cat)
                            {
                                System.Diagnostics.Debug.WriteLine("name: " + rt.Name);
                            }
                        }
                    }
            */

            return listOfResourceTypes;
        }


    }
}

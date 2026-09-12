using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Combat;
using UWGame.ClientSide;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.AllGameData
{
    public class StructureLoader
    {
        public const float padRadiusShelterAndStorage = 10f; //mp feb 2016 was 20f   I would like to be able to pack structures tighter.


        public static void Init(List<EntityType> listOfEntityTypes)
        {
           
            EntityType structure;

            float housingComfortSurvivalLow = 0.07f;//0.08f     //cannot be ugraded
            float housingComfortSurvivalMedium = 0.09f;//0.12f  //can be ugraded +5
            float housingComfortSurvivalMediumHigh = 0.11f;      //can be ugraded +5
                                      //MediumHigh is close to the wigwam (high) because I need a reason for the player to build the smaller dome shelter and not just skip to wigwam.  
            float housingComfortSurvivalHigh = 0.13f;//0.16f    //can be ugraded +5

            float housingComfortBasicLow = 0.20f;//0.30f    //can be ugraded +5 
            float housingComfortBasicHigh = 0.30f;//was 0.40f  //can be ugraded +5 +8

            float housingComfortMedium = 0.40f; //not used currently



            /*was:
            float crappyImprovisedShelterComfort = 0.1f;
            float decentImprovisedShelterComfort = 0.15f;
            float goodImprovisedShelterComfort = 0.24f;
            float smallTentComfort = 0.22f;
            float mediumTentComfort = 0.28f;
            float barracksComfort = 0.32f;
            float hutComfort = 0.35f;
            float houseComfort = 0.5f;
             */

            #region daysheenTipi
            listOfEntityTypes.Add(new EntityType("structure:daysheenTipi")
            {
                Name = "Daysheen Tipi",
                SummaryDescription = "A temporary 1-person shelter, quick to construct.",
                Description = "We came up with this design inspired by the shape and properties of the daysheen leaves. Has room for at least one person",
                ThumbnailSmall = "HUD_thumbnail_daysheenTipi",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 1, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome1People",
                    Doors = new Vector2[]
                            {
                                new Vector2(8,7),
                            }

                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "daysheenTipi",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "daysheenTipi_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo() //MP if this is commented out, then the finished billboard will appear immediately when construction begins.
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "daysheenTipi_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {              	
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 9) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!
                            {
                                Offset= new Vector2(4, -9)//offset the circle
                            },
                              new CollideShape2D(new Vector2(0, 0), 12) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!
                            {
                                Offset= new Vector2(-12, 0)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "ricketyConstruction", //DegradeType for entire structure
                    SalvageProcess = "salvageDaysheenTipi",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:daysheenLeaves", 2 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region lean-toTarp
            listOfEntityTypes.Add(new EntityType("structure:lean-toTarp")
            {
                Name = "Lean-to (Tarp)",
                SummaryDescription = "Simple 2-person shelter with a sloping roof, covered with a thermal tarp.",// A simple 2-person shelter using the thermal tarp as covering.
                Description = "This design requires a number of long, straight sticks and poles. The thermal tarp is used as covering to keep out rain and wind and ensures stable temperature.",
                // shortened, cut this text:  \n \nMade by first placing two upright poles in the ground. A crosspiece is laid horizontally on top of them and other poles are rested against it, forming the slanting roof. Over these poles other straight sticks are laid horizontally and the two walls are formed by placing sticks vertically on the sides. \nA thermal tarp is draped across the frame.
                ThumbnailSmall = "HUD_thumbnail_leanToBigTarp",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {

                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort }, //note that the thermal tarp item is advanced tier
                ContainerType = new HomeContainerType() 
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome2People",
                    Doors = new Vector2[]
                        {
                            new Vector2(-28,-6),  // (-30,2),
                        }
                    
                },

   /*             RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "leanToBigTarp",
                            BaseCenter = new Vector2(36,29)
                        }                    
                    
                    }}
                },
*/
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "leanToBigTarp",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
             /*           RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = ""
                        }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "leanToBigTarp" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "leanToBigTarp_construct" }},                                    
                             //   RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    CausesCollisions = true,
                    Pad = padRadiusShelterAndStorage,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 28) // 0,0,18 //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!
                            {
                                Offset= new Vector2(-4,6)//use this entry to offset the circle 
                            }
                        }
                    }
                },                 
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", //DegradeType for entire structure now used (feb 2015)
                    SalvageProcess = "salvageLean-toTarp",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 6 }, { "item:thermalTarp", 1 } },
                    Repair = "buildingRepair"
                } 

            });
            #endregion

            #region lean-toSpoakLeaves
            listOfEntityTypes.Add(new EntityType("structure:lean-toSpoakLeaves")
            {
                Name = "Lean-to (Spoak leaves)",
                SummaryDescription = "Simple 2- person shelter with a sloping roof, covered with spoak leaves",//A rudimentary 2-person shelter covered by spoak leaves.
                Description = "This design requires a number of long, straight sticks. The structure uses the waterproof spoak leaf to provide basic protection from rain and wind and wingweed leaves for insolation. However, the leaves are prone to infestation by the scuttler bug that can end up consuming them.",
                ThumbnailSmall = "HUD_thumbnail_leanToBigSpoak",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome2People",
                    Doors = new Vector2[]
                            {
                               new Vector2(-25,2),
                            }

                },

                /*                RenderableType = new RenderableType()
                                {
                                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                                    {                      
                                        new RenderAsBillboardType()
                                        { 
                                            AssetName = "leanToBigSpoak",
                                            BaseCenter = new Vector2(36,29)
                                        }                    
                    
                                    }}
                                },*/
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "leanToBigSpoak",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
                        /*         RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                 {
                                     AssetName = ""
                                 }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "leanToBigSpoak" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "leanToBigSpoak_construct" }},                                    
                           //     RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = padRadiusShelterAndStorage,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 18) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!
                            {
                                Offset= new Vector2(-4,6)//use this entry to offset the circle 
                            }
                        }
                    }
                },



              
                NonLivingType = new NonLivingType() { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageLean-toSpoakLeaves",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 4 }, { "item:wingweedLeaves", 2 }, { "item:spoakLeaves", 1 } },
                    Repair = "buildingRepair"
                }

            });
            #endregion

            #region lean-toScraps
            listOfEntityTypes.Add(new EntityType("structure:lean-toScraps")  //replace with structure:lean-toScrap. made from plastics  interior of aircraft.
            {
                Name = "Lean-to (Scraps)",
                SummaryDescription = "An improvised 2-person shelter made from scrap thermoplastics panels.",
                Description = "This building will probably not last for very long, but offers temporary shelter for 2 people.",
                ThumbnailSmall = "HUD_thumbnail_leanToSmallScrap",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome2People",
                    Doors = new Vector2[]
                        {
                            new Vector2(21, 4),
                        }
                    
                },     
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "leanToSmallScrap",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
                                 RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                 {
                                     AssetName = "leanToSmallScrap_g"
                                 }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "leanToSmallScrap" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                           //     RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "leanToSmallScrap_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },
             

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = padRadiusShelterAndStorage,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 18) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(0,-6)//offset the circle
                            }
                        }
                    }
                },

               
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageLean-toScraps",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 6 }, { "item:panelScraps", 1 } },
                    Repair = "buildingRepair"
                } 

            });
            #endregion

            #region A-frameTarp
            listOfEntityTypes.Add(new EntityType("structure:A-frameTarp")
            {
                Name = "A-frame (tarp)",
                SummaryDescription = "Simple 1-person shelter formed from a long backbone stick, covered with a thermal tarp.", //A hastily made 1-person shelter covered with a thermal tarp.
                Description = "Offers a small space for one person lying down. \n \nThe frame is made from a long pole which is rested against a couple of shorter sticks so that the entrance resembles an 'A'. Wingweed leaves are put in as bedding and the thermal tarp is draped on top.",
                ThumbnailSmall = "HUD_thumbnail_aFrameTarp",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 1, ComfortLevel = housingComfortSurvivalLow },

                    CanTransactWithTags = new[] { "humanTransact" },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)                    
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome1People",
                    Doors = new Vector2[]
                        {
                            new Vector2(10,3),
                        }
                    
                },

  /*              RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "aFrameTarp",
                            BaseCenter = new Vector2(36,29)
                        }     }}               
                    
                    },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "aFrameTarp"
                            }                   
                    
                        }             
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "aFrameTarp" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "aFrameTarp_construct" }},                                    
                      
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = padRadiusShelterAndStorage,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-2,3)//offset the circle
                            },
                             new CollideShape2D(new Vector2(0, 0), 8) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-18,-9)//offset the circle
                            }
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageA-frameTarp",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 2 }, { "item:wingweedLeaves", 2 }, { "item:thermalTarp", 1 } },
                    Repair = "buildingRepair"
                } 
            });
            #endregion

            #region A-frameSpoakLeaves
            listOfEntityTypes.Add(new EntityType("structure:A-frameSpoakLeaves")
            {
                Name = "A-frame (Spoak leaves)",
                SummaryDescription = "Simple 1-person shelter formed from a spoak branch, covered with leaves.", //"Simple 1-person shelter formed from a long backbone stick, covered with spoak leaves."
                Description = "Offers a small space for one person lying down. \n \nThe frame is made from a branch which makes the entrance resemble an 'A'. Spoak leaves are placed to serve as a waterproof covering.", //"Offers a small space for one person lying down. \n \nThe frame is made from a long pole which is rested against a couple of shorter sticks so that the entrance resembles an 'A'. Spoak leaves are placed to serve as a waterproof covering."
                ThumbnailSmall = "HUD_thumbnail_aFrameSpoak",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 1, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome1People",
                    Doors = new Vector2[]
                        {
                            new Vector2(10,3),
                        }
                    
                },

  /*              RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "aFrameSpoak",
                            BaseCenter = new Vector2(36,29)
                        }  }}                  
                    
                    },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "aFrameSpoak"
                            }    
                        }                       
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "aFrameSpoak" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "aFrameSpoak_construct" }},  
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-2,3)//offset the circle
                            },
                             new CollideShape2D(new Vector2(0, 0), 8) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-18,-9)//offset the circle
                            }
                        }
                    }
                },

                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageA-frameSpoakLeaves",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranchesTrimmed", 1 }, { "item:spoakLeaves", 1 } }, //super easy because only requires 1 branch. cannot use an untreated spoak branch because it does not have infestation . because of abatis in CLay Pit scenario etc.
                    Repair = "buildingRepair"
                } 

            });
            #endregion

            #region A-frameScraps
            listOfEntityTypes.Add(new EntityType("structure:A-frameScraps")
            {
                Name = "A-frame (Scraps)",
                SummaryDescription = "A crude 1-person shelter made from scrap plastic and cushions",
                Description = "Various scraps make this a somewhat comfortable place to sleep.",
                ThumbnailSmall = "HUD_thumbnail_imptent2",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 1, ComfortLevel = housingComfortSurvivalLow },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome1People",
                    Doors = new Vector2[]
                        {
                            new Vector2(25,-15),
                        }                    
                },

  /*              RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "imptent2",
                            BaseCenter = new Vector2(36,29)
                       }
                    },
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "imptent2_g"
                    }}
                },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "imptent2",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "imptent2_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "imptent2" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                        //        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "imptent2_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 18) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(0, 0)//offset the circle
                            }
                        }
                    }
                },

                
                NonLivingType = new NonLivingType() { 
                    PartsAreWeatherProof = true,
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageA-frameScraps",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 4 }, { "item:seatCushions", 1 }, { "item:panelScraps", 1 } },
                    Repair = "buildingRepair"
                } 

            });
            #endregion

            #region smallTent
            listOfEntityTypes.Add(new EntityType("structure:smallTent")
            {
                Name = "Small tent",
                SummaryDescription = "A small tent for 1 person.",
                Description = "The tent will offer good protection from rain and wind and is quite durable.",
                ThumbnailSmall = "HUD_thumbnail_smallTent",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 1, ComfortLevel = housingComfortSurvivalMedium },

                    CanTransactWithTags = new[] { "humanTransact" },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)                   
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome1People",
                    Doors = new Vector2[]
                        {
                            new Vector2(-20,3),
                        }                    
                },                
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "tent3",
                                BaseCenter = new Vector2(36,29)
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "tent3_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tent3" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                        //        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent3_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                              RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType(){ AssetName = "tent3" } },
                              RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent3_g" },
                              LightingTypes = new[] { new LightingType() { SpriteName = "common_25_tent3", Offset = new Point(-41, -30) } },
                              Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.LightIsOn)
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 13) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(10,-3)//offset the circle
                            },
                            new CollideShape2D(new Vector2(0, 0), 13) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-7,-1)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageSmallTent", 
                    PartKeys = new SerializableDictionary<string, int>() { { "item:smallTent", 1 }},
                    Repair = "tentRepair"
                }

            });
            #endregion

            #region octagonalTent
            listOfEntityTypes.Add(new EntityType("structure:octagonalTent")
            {
                Name = "Octagonal tent",
                SummaryDescription = "A comfortable tent for 2 people.",
                Description = "The tent will offer good protection from rain and wind and is quite durable.",
                ThumbnailSmall = "HUD_thumbnail_octagonalTent",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalMedium },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    UpgradesProfile = "survivalHome2People",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    Doors = new Vector2[]
                        {
                            new Vector2(-19,10),
                        }                    
                },      
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "tent2",
                    //           BaseCenter = new Vector2(0,0) //mp: please test!
                            }                                        
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "tent2_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tent2" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                        //        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent2_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                              RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType(){ AssetName = "tent2" } },
                              RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent2_g" },
                              LightingTypes = new[] { new LightingType() { SpriteName = "common_24_tent2", Offset = new Point(-34, -31) } },
                              Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.LightIsOn)
                         }
                         
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 21) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(1,-3)//offset the circle
                            }
                        }
                    }
                },               
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", //DegradeType for entire structure
                    SalvageProcess = "salvageOctagonalTent",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:octagonalTent", 1 }, },
                    Repair = "tentRepair"
                }  

            });
            #endregion

            #region domeTent
            listOfEntityTypes.Add(new EntityType("structure:domeTent")
            {
                Name = "Dome tent",
                SummaryDescription = "A comfortable tent for 2 people.",
                Description = "The tent will offer good protection from rain and wind and is quite durable.",
                ThumbnailSmall = "HUD_thumbnail_domeTent",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                 //   Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalMedium },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    UpgradesProfile = "survivalHome2People",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    Doors = new Vector2[]
                        {
                            new Vector2(17,6),
                        }                    
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType(){ AssetName = "tent1", BaseCenter = new Vector2(48,55) } },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent1_g" }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tent1" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                        //        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent1_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                              RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType(){ AssetName = "tent1", BaseCenter = new Vector2(48,55) } },
                              RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "tent1_g" },
                              LightingTypes = new[] { new LightingType() { SpriteName = "common_23_tent1", Offset = new Point(-47, -52) } },
                              Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.LightIsOn)
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 26) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-0,-3)//offset the circle
                            },
                            new CollideShape2D(new Vector2(0, 0), 12) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-22,15)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", //DegradeType for entire structure
                    SalvageProcess = "salvageDomeTent",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:domeTent", 1 }, },
                    Repair = "tentRepair"
                } 
            });
            #endregion

            #region domeShelterTarp
            listOfEntityTypes.Add(new EntityType("structure:domeShelterTarp")
            {
                Name = "Dome shelter (Tarp)",
                SummaryDescription = "A quite comfortable shelter for 2 people.",
                Description = "Based on a sturdy frame made from shadeleaf canes bent in arches and covered with a thermal tarp. The dome will offer good protection from rain and wind and should be reasonably durable.",
                ThumbnailSmall = "HUD_thumbnail_domeTarp",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalMedium },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome2People",
                    Doors = new Vector2[]
                        {
                            new Vector2(15,5),
                        }                    
                },
  /*              RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "domeTarp",
                            BaseCenter = new Vector2(60,55)//80,75)//36,29
                        }                    
                    
                    },
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "domeTarp_g"
                    }}
                },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "domeTarp",
                                BaseCenter = new Vector2(53,56) 
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "domeTarp_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "domeTarp",  BaseCenter = new Vector2(53,56)  }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "domeTarp_construct",  BaseCenter = new Vector2(53,56)  }},                                    
                      //          RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 25) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-1,-8)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageDomeShelterTarp",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:shadeleafCanes", 4 }, { "item:thermalTarp", 1 } },
                    Repair = "buildingRepair"
                } //DegradeType for entire structure

            });
            #endregion

            #region domeShelterSpoakShingles
            listOfEntityTypes.Add(new EntityType("structure:domeShelterSpoakShingles")
            {
                Name = "Dome shelter (Spoak shingles)",
                SummaryDescription = "A quite comfortable 2-person shelter, covered with durable shingles",
                Description = "Has a sufficiently sturdy frame made from shadeleaf canes bent in arches. The dome is covered with shingles made from spoak leaves, giving protection from the elements. The shingles are treated with a preservative which greatly reduces the risk of bug infestation.",
                ThumbnailSmall = "HUD_thumbnail_domeShingles",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 2, ComfortLevel = housingComfortSurvivalMediumHigh },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                    UpgradesProfile = "survivalHome2People",
                    Doors = new Vector2[]
                        {
                            new Vector2(15,5),
                        }                    
                },
  /*              RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType()
                        { 
                            AssetName = "domeShingles",
                            BaseCenter = new Vector2(60,55)
                        }                    
                    
                    },
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "domeShingles_g"
                    }}
                },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "domeShingles",
                                BaseCenter = new Vector2(53,56) 
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "domeShingles_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "domeShingles",  BaseCenter = new Vector2(53,56) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "domeShingles_construct",  BaseCenter = new Vector2(53,56) }},                                    
                      //          RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 25) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-1,-8)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageDomeShelterSpoakShingles",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:shadeleafCanes", 4 }, { "item:spoakShingles", 1 } }, //needs to be easier to build than wigwam.
                    Repair = "buildingRepair"
                } //DegradeType for entire structure

            });
            #endregion


            #region wigwamSpoakShingles
            listOfEntityTypes.Add(new EntityType("structure:wigwamSpoakShingles")
            {
                Name = "Wigwam (Spoak shingles)",
                SummaryDescription = "A roomy and sturdy dwelling for 4 people",
                Description = "Not a simple building task in the wilderness, but the result will last for a long time. The design takes advantage of the curving shape of the spoak branches which are placed in an inter-locking pattern to form a large domed frame. Shingles from the tree's leaves make a durable cover. The leaves are treated with an anti-infestation emulsion, making the wigwam free from scuttler bugs.",
                ThumbnailSmall = "HUD_thumbnail_wigwamShingles",
                CategoryKey = "shelter",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort }, // ?
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 4, ComfortLevel = housingComfortSurvivalHigh},

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,//true=rally point is between door and entity center 
                    UpgradesProfile ="survivalHome4People",
                    Doors = new Vector2[]
                        {
                            new Vector2(21, 19),
                        }                    
                },

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "wigwamShingles",
                           //     BaseCenter = new Vector2(36,29) //commented this out because it ruined the relative position of billboard / groundsprite.  juli 2013 MP
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "wigwamShingles_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "wigwamShingles" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "wigwamShingles_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "wigwamShingles_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage, 
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 29) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset= new Vector2(-6, 0)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", //DegradeType for entire structure
                    SalvageProcess = "salvageWigwamSpoakShingles",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranchesTrimmed", 3 }, { "item:spoakShingles", 3 }, { "item:firegrassSod", 2 }, { "item:stones", 1 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

           
  

            #region storageHole
            listOfEntityTypes.Add(new EntityType("structure:storageHole")
            {
               Name = "Storage hole",  // http://books.google.dk/books?id=deRKF5kv5wwC&pg=PA453&lpg=PA453&dq=storing+food+in+the+wilderness&source=bl&ots=EupkGs6c_I&sig=cOy19RAmiOOhwIv1YwzvznMWlXE&hl=da&sa=X&ei=T-w0UeL3DefY0QWBqoD4CA&ved=0CFMQ6AEwBQ#v=onepage&q=storing%20food%20in%20the%20wilderness&f=false
               // http://www.waysofthewildinstitute.com/2010/03/wilderness-food-preservation/
               SummaryDescription = "For storing and protecting food and ingredients",
               Description = "A hole lined with large stones. Covered with spoak leaves and some heavy rocks that will keep most animals out.",
               ThumbnailSmall = "HUD_thumbnail_storageholeYellowleaves",
               CategoryKey = "production",
               StructureType = new StructureType()
               {
                   BuildByPlayer = true,
                 //  Category = GameData.Instance.AllStructureCategories["production"]
               },
               TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
               ContainerType = new StorageContainerType("earthCooled", 8f)
               {
                   StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                   
                   CanTransactWithTags = new[] { "humanTransact", "leafcutterTransact" },
                  
                   DefaultStorageSettings = "darkFoodStorage" // find this in DefaultStorageSettings                   
               },


               RenderableType = new RenderableType()
               {
                   DefaultClientState = new ClientStateInfo()
                   {
                       RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "storageholeYellowleaves",
                         //       BaseCenter = new Vector2(0,0) //mp had no coords
                            }                    
                    
                        },
                       RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                       {
                           AssetName = "storageholeYellowleaves_g"
                       }
                   },
                   ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storageholeYellowleaves" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                          //      RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "storageholeYellowleaves_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
               },

               DefaultSimState = new SimStateInfo()
               {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                       Pad = padRadiusShelterAndStorage,
                       PadShape = CollidePrim.Circle,
                       Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16)
                            {
                                Offset= new Vector2(0, 3)//offset the circle
                            }
                        }
                    }
               },               
               NonLivingType = new NonLivingType()
               {
                   PartsAreWeatherProof = true,
                   DegradeType = "adequateConstruction",
                   SalvageProcess = "salvageStorageHole",
                   PartKeys = new SerializableDictionary<string, int>() { { "item:spoakLeaves", 1 }, { "item:stones", 1 } },
                   Repair = "buildingRepair"
               }

           });
            #endregion


            #region toolshed
            listOfEntityTypes.Add(new EntityType("structure:toolshed")
            {
                Name = "Toolshed",  // 
                SummaryDescription = "A sturdy shed for storing farm equipment",
                Description = "Built from mudbricks covered in a basic white plaster. Will keep equipment in good condition when they are not in use.", //
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ThumbnailSmall = "HUD_thumbnail_placeholder", //todo
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true, 
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },

                ContainerType = new StorageContainerType("isolated", 8f)
                {
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    CanTransactWithTags = new[] { "humanTransact" },

                    DefaultStorageSettings = "farmToolshed" // find this in DefaultStorageSettings                   
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "shedskin1",
                        
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "shed_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "shedskin1" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                          //      RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "shed_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //TODO
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16)
                            {
                                Offset= new Vector2(0, 3)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:waterCaneStem", 2 }, { "item:solidMudBrick", 3 } }, //was: { "item:structurePanels", 1 }, because of the sprite...
                    Repair = "buildingRepair",
                    SalvageProcess = "salvageToolshed"
                }

            });
            #endregion


            #region clayGranary
            listOfEntityTypes.Add(new EntityType("structure:clayGranary")
            {
                Name = "Clay granary",  // http://books.google.dk/books?id=deRKF5kv5wwC&pg=PA453&lpg=PA453&dq=storing+food+in+the+wilderness&source=bl&ots=EupkGs6c_I&sig=cOy19RAmiOOhwIv1YwzvznMWlXE&hl=da&sa=X&ei=T-w0UeL3DefY0QWBqoD4CA&ved=0CFMQ6AEwBQ#v=onepage&q=storing%20food%20in%20the%20wilderness&f=false
                // http://www.waysofthewildinstitute.com/2010/03/wilderness-food-preservation/
                SummaryDescription = "Safe storage of food and ingredients", 
                Description = "To protect its contents from field quadites (and other scavengers) the granary is raised from the ground, built on pillars. The design and choice of building materials will prohibit field quadites (the most problematic pest animal) from entering.",//
                ThumbnailSmall = "HUD_thumbnail_clayGranary",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ContainerType = new StorageContainerType("isolated", 8f)
                {
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    CanTransactWithTags = new[] { "humanTransact" },
                    DefaultStorageSettings = "darkFoodStorage" // find this in DefaultStorageSettings                   
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "clayGranary",
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "clayGranary_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "clayGranary" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "clayGranary_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "clayGranary_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 3), 16)
                        },
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, -20), 31)
                        }
                    }
                },                
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageClayGranary",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakShingles", 1 }, { "item:solidMudBrick", 5 } },
                    Repair = "buildingRepair"
                }

            });
            #endregion

            #region cooledFoodCache
            listOfEntityTypes.Add(new EntityType("structure:cooledFoodCache")
            {
                Name = "Cooled food cache",
                SummaryDescription = "Cooled with the aircraft airconditioning unit", //description overwritten in scenario
                Description = "It should be possible to refrigerate our food by storing it in a hole together with the airconditioning unit. The hole must be lined with large stones and covered with spoak leaves and rocks to keep animals out.",
                ThumbnailSmall = "HUD_thumbnail_storageholeYellowleaves",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ContainerType = new StorageContainerType("refrigerator" /* Storage.Conditions.Refrigerator*/, 8f)
                {
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    CanTransactWithTags = new[] { "humanTransact", "leafcutterTransact" },
                    DefaultStorageSettings = "cooledStorage" // find this in DefaultStorageSettings                   
                },

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "storageholeYellowleaves",
                            //    BaseCenter = new Vector2(36,29) mp had no coords
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "storageholeYellowleaves_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storageholeYellowleaves" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                          //      RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "storageholeYellowleaves_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16)
                            {
                                Offset= new Vector2(0, 3)//offset the circle
                            }
                        }
                    }
                },

                //replace with a battery fuel system (like campfire) when it becomes more stable                
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageCooledFoodCache",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:activatedFoodCoolerUnit", 1 }, { "item:spoakLeaves", 1 }, { "item:stones", 1 } },
                    Repair = "buildingRepair"
                }

            });
            #endregion

            #region abatis
            listOfEntityTypes.Add(new EntityType("structure:abatis")
            {
                Name = "Abatis",
                SummaryDescription = "Fence made from tangled, curving branches",
                Description = "Obstacles like these might be able to block passage of some predators since spoak branches can form a dense barrier.",
                ThumbnailSmall = "HUD_thumbnail_abatis",
                CategoryKey = "defense",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
     /*           RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType(){ AssetName = "abatis1" }                    
                    
                    }}
                },*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "abatis1",
                    //            BaseCenter = new Vector2(0,0) // no coord before. 
                            }                    
                    
                        },
               /*         RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "_g"
                        }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "abatis1" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                     /*    new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "_construct_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }*/
                     }
                },



                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        GridAlignedPlacement = true,

                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 7)
                            {
                                Offset= new Vector2(0,0)//offset the circle up and to the left of entity center
                            }
                        }
                    }
                },                
                //   TileLayoutType = new TileLayoutType() { LayoutName = "abatis1"}                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageAbatis1",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranches", 1 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region abatis2 (not yet implemented)
            /*
            listOfEntityTypes.Add(new EntityType("structure:abatis2") // // commented out abatis2 till we get a way to vary the abatis billboard
            {
                Name = "Abatis 2",
                ThumbnailSmall = "HUD_thumbnail_abatis",
                StructureType = new StructureType()
                {

                    BuildByPlayer = true,
                    Category = GameData.Instance.AllStructureCategories["defense"]
                },

                RenderableType = new RenderableType()
                {
                    Default = new StaticConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType(){ AssetName = "abatis2" }                    
                    
                    }
                },


                //PointLayoutType = new PointLayoutType()
                //{
                //    GridAlignedPlacement = true
                //},

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    GridAlignedPlacement = true,

                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 8)
                            {
                                Offset = new Vector2(0,0)//offset the circle up and to the left of entity center
                            }
                        }
                },


                PartKeys = new SerializableDictionary<string, int>() { { CreatePlaceholder("item:spoakBranches"), 1 } },
                NonLivingType = new NonLivingType() { PartsAreWeatherProof = true, DegradeType = "adequateConstruction", SalvageProcess = "salvageAbatis2" }
                //     TileLayoutType = new TileLayoutType() { LayoutName = "abatis2" }

            });

*/
            #endregion



            #region campfire
            listOfEntityTypes.Add(new EntityType("structure:campfire")
            {
                Name = "Campfire",
                SummaryDescription = "The campfire needs an available supply of firewood",
                Description = "Most food requires preparation on a campfire. Some bushcraft production also requires heat from a fireplace. It is important to always have a supply of firewood in stock, else the campfire is useless.",
                TierOrArea = new TierOrArea() { Tier = "survival" },
                ThumbnailSmall = "HUD_thumbnail_fireplace",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    IsAddon = true,
                    //Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },               
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 10f, // 50f, NA was 20f 
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 22f),

                            new CollideShape2D(Vector2.Zero, 8f) { Offset = new Vector2(-32f, -9f)},
                            new CollideShape2D(Vector2.Zero, 9f) { Offset = new Vector2(24f, -17f)}
                        }
                    }
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "fireplace",
                         //       BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fireplace_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "fireplace" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                             //   RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "fireplace_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "fireplace" }},     
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "fireplace_g" },                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                LightingTypes = new[]{ new LightingType() { SpriteName = "common_21_circularbig", Offset = new Point(-48, -48) } },
                                ParticleEmitters = new[]{ 
                                                new ParticleEmitterEffect(){  ParticleSystemKey = "smallerSmoke",},//todo add offset (add a space and choose from options) and alter geometry of groundsprite for improvised kitchen 
                                                new ParticleEmitterEffect(){  ParticleSystemKey = "smallFire" }} //todo add offset and geometry of groundsprite for improvised kitchen                            
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    ToolTag = new[] { "fireplace" },
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "lightFire"                  
                 //       new ParticleEmitterEffect(){  ParticleSystemKey = "fireSparks" ...Only for bonfire
             
                },
                ContainerType = new ReplenishContainerType()
                 {
                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                      
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelCampfire",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 7f //MP feb 2016: reduced by 33%...was 10f         MP 11 sep 2014. 20f made it burn out really quickly I think.       //  4f
                        },
                        /*
                        ReplenishActionType = new ReplenishActionType()
                        {
                            ReplenishAction = AI.Goals.GoalReplenish.ReplenishAction.Refuel,
                            AgentAction = new AgentAction()
                            {
                                Stances = AllGameData.ProcessLoader.kneelingProduction, // new[]{ LeggedLocomotor.Stance.Kneeling },
                                AgentActionState = AnimAction.Mending,
                                ScaleAnimDuration = false,
                                TimeInRealSeconds = 3 // mend: 156 = 6,24
                            }
                        }*/
                    }
                      
                 },        
                GatheringSiteType = new GatheringSiteType()
                {
                    arc = new Arc()
                    {
                        Radius = 40f,
                        MinAngle = -180,
                        MaxAngle = 180
                    },
                    MaxVisitors = 30

                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageCampfire",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 1 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region fieldKitchen
            listOfEntityTypes.Add(new EntityType("structure:fieldKitchen")
            {
                Name = "Kitchen (movable)", //was: "Field kitchen"
                SummaryDescription = "Advanced field kitchen which uses liquid gas for cooking.",
                Description = "Specially designed for the Tau Ceti mission. Provides essential functions for food preparation in the field.",
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food }, //why not advanced. is it because the parts only are advanced, and the putting it up doesnt require much debating once you have the parts?
                ThumbnailSmall = "HUD_thumbnail_kitchenPremade",
                CategoryKey = "production",
                StructureType = new StructureType()
                {                 
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {                  
                        Pad = 5f, // small pad...
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 26f),
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(-37f, 8f)},
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(24f, 17f)}
                        }
                    }
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "kitchenPremade",
                       //         BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "kitchenPremade_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenPremade" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                             //   RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenPremade_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenPremade" }},                                   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenPremade_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new ParticleEmitterEffect[]
                                 {
                                     new ParticleEmitterEffect(){  ParticleSystemKey = "foodSteam"}
                                 }
                          
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //   ToolTag = new[] { "kitchen" }, //mp feb 2015. was: "fireplace"  MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "cookAtStove", // "lightFireWithoutFlames",
                },
      /*          ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, 

                    ItemStorageType = new ItemStorageType("refrigerator", 1.5f, "isolated", 2.5f), // NEW: has fridge and storage for tools and ingredients
                    DefaultStorageSettings = "kitchenStorage",
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeKeyName = "item:liquidGas",
                            //FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////MP feb 2016: reduced by 33%...was 7f       mp set to same as improvised kitchen 7f. was 20f
                        },
                        ReplenishProcess ="refuelKitchen"                           
                    }                    
                },*/


                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                           // FuelTypeKeyName = "item:liquidGas",
                            FuelTypeTag = "fuelForFieldKitchen",
                            BurnRatePerDay = 5f ////MP feb 2016: reduced by 33%...was 7f       mp set to same as improvised kitchen 7f. was 20f
                        },
                        ReplenishProcess ="refuelKitchen"                           
                    }    ,
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("refrigerator", 1.5f, "isolated", 2.5f), // NEW: has fridge and storage for tools and ingredients
                    DefaultStorageSettings = "kitchenStorage",

                },


               /* GatheringSiteType = new GatheringSiteType()
                {   // use this?? no..
                    arc = new GatheringSiteType.Arc()
                    {
                        Radius = 40f,
                        MinAngle = -180,
                        MaxAngle = 180
                    },
                    MaxVisitors = 30

                },*/
                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageFieldKitchen",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:fieldKitchenStove", 1 }, 
                                                            { "item:fieldKitchenEquipment", 1 }},
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region improvisedKitchen
            listOfEntityTypes.Add(new EntityType("structure:improvisedKitchen")
            {
                Name = "Kitchen (scraps)",
                SummaryDescription = "A work area for preparing ingredients and cooking food).",
                Description = "This workspace gives us the ability to quickly prepare and cook large amounts of food.",
                ThumbnailSmall = "HUD_thumbnail_kitchenImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                 //   Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food }, //requires scraps
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = -10f, // mp I made it small because the ground sprite is so big 
                        PadShape = CollidePrim.Circle, //,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(21, -3f)},
                            new CollideShape2D(Vector2.Zero, 25f) { Offset = new Vector2(-42f, 13f)},

                        },
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(21, -3), 22f)
                        }
                    }
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "kitchenImprovised",
                                Offset = new Vector2(-35f, 7f) // this structure is asymmetric.
                            }                    
                    
                        },


                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "kitchenImprovised_g",
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised", Offset = new Vector2(-35f, 7f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised_construct", Offset = new Vector2(-35f, 7f) }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised", Offset = new Vector2(-35f, 7f) }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallerSmoke",Offset = new Vector2(21,-3)},
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallFire",Offset = new Vector2(21,-3) }
                                }
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //    ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kitchenImprovisedLightFire"
                },

                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelCampfire",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////MP feb 2016: reduced by 33%...was 7f      mp: more efficient fuel economy than fireplace which has 7f
                        }
                    },
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // has  storage for tools 
                    DefaultStorageSettings = "uncooledKitchenStorage",

                },


                GatheringSiteType = new GatheringSiteType()
                {
                    arc = new Arc()
                    {
                        Radius = 27f,
                        MinAngle = -180,
                        MaxAngle = 180,
                    },
                    MaxVisitors = 30

                },
                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageImprovisedKitchen",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:panelScraps", 1 }, 
                                                            { "item:sticks", 2 }, { "item:spoakShingles", 1 }},
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region mudBrickKitchen
            listOfEntityTypes.Add(new EntityType("structure:mudBrickKitchen") //does not require mubricks. only clay. copy pasted from the other kitchen, only diff is clay instead of panels scraps.
            {
                Name = "Kitchen (simple)",
                SummaryDescription = "A work area for preparing ingredients and cooking food.",
                Description = "This workspace gives the ability to efficiently prepare and cook sizable amounts of food.",
                ThumbnailSmall = "HUD_thumbnail_kitchenImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food }, //requires clay, basic
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = -10f, // mp I made it small because the ground sprite is so big                    
                        PadShape = CollidePrim.Circle, //,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(21, -3f)},
                            new CollideShape2D(Vector2.Zero, 25f) { Offset = new Vector2(-42f, 13f)},

                        },
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(21, -3), 22f)
                        }
                    }
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "kitchenImprovised",
                                Offset = new Vector2(-35f, 7f) // this structure is asymmetric. 
                            }                    
                    
                        },


                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "kitchenImprovised_g",
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised", Offset = new Vector2(-35f, 7f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised_construct", Offset = new Vector2(-35f, 7f) }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                          new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kitchenImprovised", Offset = new Vector2(-35f, 7f) }},                                     
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "kitchenImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]{ 
                                        new ParticleEmitterEffect(){ ParticleSystemKey = "smallerSmoke", Offset = new Vector2(21,-3)},
                                        new ParticleEmitterEffect(){ ParticleSystemKey = "smallFire", Offset = new Vector2(21,-3) },
                                    }
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //    ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kitchenImprovisedLightFire"                  
                },
                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelCampfire",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////MP feb 2016: reduced by 33%...was 7f      mp: more efficient fuel economy than fireplace which has 7f
                        }
                    },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // has  storage for tools 
                    DefaultStorageSettings = "uncooledKitchenStorage",

                },
////////////////////////////
                GatheringSiteType = new GatheringSiteType()
                {
                    arc = new Arc()
                    {
                        Radius = 27f,
                        MinAngle = -180,
                        MaxAngle = 180,
                    },
                    MaxVisitors = 30
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageMudBrickKitchen",
                    PartKeys = new SerializableDictionary<string, int>() { 
                                                            { "item:sticks", 2 }, { "item:spoakShingles", 1 }},
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region WorkshopBuilding - NEW: upgradable building!
            listOfEntityTypes.Add(new EntityType("structure:workshopBuilding") //
            {
                Name = "Workshop building", // "Polymer workshop",
                SummaryDescription = "A building that can house different upgrades", //"A workshop for producing polymer products such as rubber and plastic items",
                Description = "The building starts empty but can be equipped with various tools to form an efficient workplace.", //"The building is equipped with different tools such as an extrusion machine which can shape rubber and plastic materials into a diverse range of products. The workshop has a fireplace which provides heat for the processes.",//todo
                ThumbnailSmall = "HUD_thumbnail_plasticWorkshop",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 0f, // 
                        PadShape = CollidePrim.Circle, //,
                        Shapes = new CollideShape2D[] 
                        {   new CollideShape2D(Vector2.Zero, 24f) { Offset = new Vector2(-15f, -7f)},
                            new CollideShape2D(Vector2.Zero, 13f) { Offset = new Vector2(16f, -9f)},
                            new CollideShape2D(Vector2.Zero, 13f) { Offset = new Vector2(39, -8f)}     
                        }
                    }
                },
                SimStateConditions = new[]
                {
                    new SimStateInfo()
                    {
                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade3), // textileWorkshop
                         GeometryLayoutType = new GeometryLayoutType()
                         {
                            Pad = 0f, // 
                            PadShape = CollidePrim.Circle, //,
                            Shapes = new CollideShape2D[] 
                            {   new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(-58f, -2f)},
                                new CollideShape2D(Vector2.Zero, 23f) { Offset = new Vector2(-27f, -5f)},
                                new CollideShape2D(Vector2.Zero, 33f) { Offset = new Vector2(8, 1f)},
                                new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(45f, 12f)},

                            }
                         }
                    },
                    new SimStateInfo()
                    {
                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade4), // metalLatheShopHumanPoweredUpgrade
                         GeometryLayoutType = new GeometryLayoutType()
                         {
                            Pad = 0f, // 
                            PadShape = CollidePrim.Circle, //,
                            Shapes = new CollideShape2D[] 
                            {   new CollideShape2D(Vector2.Zero, 23f) { Offset = new Vector2(-16f, -6f)},
                                new CollideShape2D(Vector2.Zero, 28f) { Offset = new Vector2(6f, -3f)},
                                new CollideShape2D(Vector2.Zero, 22f) { Offset = new Vector2(35, 4f)}                              
                            }
                         }
                    },
                    new SimStateInfo()
                    {
                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade5), // carpenterWorkshopUpgrade
                         GeometryLayoutType = new GeometryLayoutType()
                         {
                            Pad = 0f, // 
                            PadShape = CollidePrim.Circle, //,
                            Shapes = new CollideShape2D[] 
                            {   new CollideShape2D(Vector2.Zero, 21f) { Offset = new Vector2(-24f, -2f)},
                                new CollideShape2D(Vector2.Zero, 25f) { Offset = new Vector2(2f, -5f)},
                                new CollideShape2D(Vector2.Zero, 27f) { Offset = new Vector2(42, 15f)},
                                new CollideShape2D(Vector2.Zero, 14f) { Offset = new Vector2(-15f, 25f)},

                            }
                         }
                    },
                    new SimStateInfo()
                    {
                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade6), // polymerWorkshopUpgrade
                         GeometryLayoutType = new GeometryLayoutType()
                         {
                            Pad = 0f, // 
                            PadShape = CollidePrim.Circle, //,
                            Shapes = new CollideShape2D[] 
                            {   new CollideShape2D(Vector2.Zero, 18f) { Offset = new Vector2(-62f, 2f)},
                                new CollideShape2D(Vector2.Zero, 37f) { Offset = new Vector2(-8f, 7f)},
                                new CollideShape2D(Vector2.Zero, 33f) { Offset = new Vector2(8, 1f)},
                                new CollideShape2D(Vector2.Zero, 27f) { Offset = new Vector2(25f, 1f)},

                            }
                         }
                    }
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "workshopEmpty"                              
                            }           
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "workshopEmpty_g",
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopEmpty" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "plasticWorkshop_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "plasticWorkshop_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },                         
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopTextile" }},  
                                 RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workshopTextile_g" },                                  
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade3)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopMachinist" }},  
                                 RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workshopMachinist_g" },                                  
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade4)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopCarpenter" }},  
                                 RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workshopCarpenter_g" },                                  
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade5)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopPolymer" }},  
                                 RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workshopPolymer_g" },                                  
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Upgrade6)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workshopPolymer" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workshopPolymer_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BurningFuel, (int)StateModifier.Upgrade6),
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke",Offset = new Vector2(0,-30)}, //chimney smoke
                                    new ParticleEmitterEffect() {  ParticleSystemKey = "tinyFire", Offset = new Vector2(4,13)   }, //furnace
                                }
                         }
                     }
                },               
                ContainerType = new UpgradableBuildingContainerType() // UpgradableWorkshopContainerType() // note that the upgrade is a tool, and it should also have room for outputs and replenish items. This container is for general storage, like materials or extra tools
                {
                    CanBeEnteredByTags = new[] { "humanTransact" }, // NEW: they can now enter so they can salvage workshops inside, otherwise they would have to be unloaded/carried outside                                
                    UpgradesProfile = "workshopProfile",                                    
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 4f), // has  storage for tools and materials
                    DefaultStorageSettings = "noStorage", //no storage without upgrade
                    Doors = new Vector2[] { new Vector2(0f, 20f) /*new Vector2(-26f, 9f) <- at entrance */} 
                },

                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageWorkshopBuilding",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 5 }, { "item:waterCaneStem", 3 }, { "item:stones", 1 }},
                    Repair = "buildingRepair"
                }
            });

            /*
            listOfEntityTypes.Add(new EntityType("structure:plasticWorkshop") //
            {
                Name = "Polymer workshop",
                SummaryDescription = "A workshop for producing polymer products such as rubber and plastic items",
                Description = "The building is equipped with different tools such as an extrusion machine which can shape rubber and plastic materials into a diverse range of products. The workshop has a fireplace which provides heat for the processes.",//todo
                ThumbnailSmall = "HUD_thumbnail_plasticWorkshop",//
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.ByStatus,
                StructureType = new StructureType()
                {
                    Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = 0f, // 


                    PadShape = CollidePrim.Circle, //,
                    Shapes = new CollideShape2D[] 
                        {   new CollideShape2D(Vector2.Zero, 13f) { Offset = new Vector2(5f, 18f)},
                            new CollideShape2D(Vector2.Zero, 13f) { Offset = new Vector2(5f, 32f)},
                            new CollideShape2D(Vector2.Zero, 22f) { Offset = new Vector2(22, -1f)},
                            new CollideShape2D(Vector2.Zero, 29f) { Offset = new Vector2(-18f, 3f)},

                        }               
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "plasticWorkshop"                              
                            }           
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "plasticWorkshop_g",
                        }
                    },
                    SpriteConditions = new[]
                     {
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "plasticWorkshop" }},                                                                    
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ordered)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "plasticWorkshop_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "plasticWorkshop_construct_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         },
                          new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "plasticWorkshop" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "plasticWorkshop_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.PreparedTool),
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke",Offset = new Vector2(0,-30)}, //chimney smoke
                                    new ParticleEmitterEffect() {  ParticleSystemKey = "tinyFire", Offset = new Vector2(4,13)   }, //furnace
                                }
                         }
                     }
                },
                ToolType = new ToolType()
                {
                    
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kitchenImprovisedLightFire", //REQUIRED for the particle effects to appear. ...or use "lightFireWithoutFlames" or  "kilnSmoke"  ..mp maybe doesnt matter                   
                },
                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelCampfire",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////same as simple kitchen
                        }
                    },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // has  storage for tools 
                    DefaultStorageSettings = "workbenchStorage", //TODO
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvagePlasticWorkshop",
                    PartKeys = new SerializableDictionary<string, int>() { 
                                                            { "item:extrusionMachine", 1 }, { "item:solidMudBrick", 5 }, { "item:waterCaneStem", 3 }, { "item:stones", 1 }},
                    Repair = "buildingRepair"
                } 
            });*/
            #endregion

            #region cookhouse

            var cookhouseSmoke = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke", Offset = new Vector2(5,-54)}, //chimney smoke . we do not use smoke from smoke oven, since this would be very complicated.                                    
                                };
            var cookhouse = new RenderAsBillboardType(){ AssetName = "cookhouse" };
            var cookhouseCommunityHall = new RenderAsBillboardType(){ AssetName = "cookhouseAddition", Offset = new Vector2(37f, -25f) };   
            var cookhouseDryingShed = new RenderAsBillboardType(){ AssetName = "cookhouseDryingShed", Offset = new Vector2(-41f, 4f) };
            var cookhouseSmokeOven = new RenderAsBillboardType(){ AssetName = "cookhouseSmokeOven", Offset = new Vector2(-67f, -10f) };
          //  var cookhouse_g = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" }; mp i couldn't make it work with a variable...

            listOfEntityTypes.Add(new EntityType("structure:cookhouse") //
            {
                Name = "Cookhouse", // 
                SummaryDescription = "A kitchen building that can be outfitted with different cooking installations", //
                Description = "The building starts empty but can have various tools installed to form an efficient workplace for making food.", //
                ThumbnailSmall = "HUD_thumbnail_cookhouse",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Food },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 0f, // 
                        PadShape = CollidePrim.Circle, //,
                        Shapes = new CollideShape2D[] //carefully placed so that accesspoint is behind billboard:
                        { //  new CollideShape2D(Vector2.Zero, 27f) { Offset = new Vector2(-32f, 0f)},//cookhouse
                            new CollideShape2D(Vector2.Zero, 19f) { Offset = new Vector2(-42f, -17f)},//cookhouse
                            new CollideShape2D(Vector2.Zero, 21f) { Offset = new Vector2(-23f, 2f)},//cookhouse
                            new CollideShape2D(Vector2.Zero, 21f) { Offset = new Vector2(9f, 5f)},//cookhouse chimney
                            new CollideShape2D(Vector2.Zero, 38f) { Offset = new Vector2(36f, -21f)},//cookhouse canopy

                            new CollideShape2D(Vector2.Zero, 12f) { Offset = new Vector2(-64f, -11f)}  //smoke oven  
                        },
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 35)
                        }
                    }
                },
               /* SimStateConditions = new[] // upgrades geo. not finished...
                {
                    new SimStateInfo()
                    {
                         Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven), //
                         GeometryLayoutType = new GeometryLayoutType()
                         {
                            Pad = 0f, // 
                            PadShape = CollidePrim.Circle, //,
                            Shapes = new CollideShape2D[] 
                            {   
                                new CollideShape2D(Vector2.Zero, 12f) { Offset = new Vector2(-64f, -11f)}  //smoke oven
                            }
                         }
                    },

                },*/
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "cookhouse"                              
                            }    
                            // when testing the structure geo, list all of the billboards here, and you can see all the geo when running the StructureGeoMap:                            
                    //        new RenderAsBillboardType(){ AssetName = "cookhouse", Offset = new Vector2(0f, 0f) },
                    //        new RenderAsBillboardType(){ AssetName = "cookhouseAddition", Offset = new Vector2(27f, -16f) },
                    //        new RenderAsBillboardType(){ AssetName = "cookhouseDryingShed", Offset = new Vector2(-41f, 4f) }, 
                    //        new RenderAsBillboardType(){ AssetName = "cookhouseSmokeOven", Offset = new Vector2(-67f, -10f) },                                 
                                  
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "cookhouse_g",
                        },

                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{ cookhouse },                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "cookhouse_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_construct_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },  
                   /*      new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{ cookhouse },                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" }, //is this sprite necessary?
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool, (int)StateModifier.UpgradeStove), 
                                ParticleEmitters = cookhouseSmoke
                         },  */
    
                         //stove should not cause graphics change, it is invisible from outside
                        #region 1 upgrade
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse                                                                  
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                                                   
                                },                   
                                ParticleEmitters = cookhouseSmoke , 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove,  (int)StateModifier.PreparedTool )                            
                         },
///////////////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseCommunityHall                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                                
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseCommunityHall                               
                                },                   
                                ParticleEmitters = cookhouseSmoke ,
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall,  (int)StateModifier.PreparedTool )                            
                         },
/////////////////////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven
                                },          
                                ParticleEmitters = cookhouseSmoke , 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.PreparedTool)                            
                         },
                         ////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseDryingShed
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseDryingShed
                                },                
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseDryingShed,  (int)StateModifier.PreparedTool)                            
                         },
                         ////////




                        #endregion
                        #region 2 upgrades
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,                                                                   
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeStove)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,                                                                   
                                },   
                                ParticleEmitters = cookhouseSmoke,  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeStove,  (int)StateModifier.PreparedTool)                          
                         },
//////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseCommunityHall                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeStove)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseCommunityHall                               
                                }, 
                                ParticleEmitters = cookhouseSmoke,   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeStove,  (int)StateModifier.PreparedTool)                            
                         },
//////////

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseDryingShed                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.UpgradeStove)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseDryingShed                               
                                },    
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.UpgradeStove,  (int)StateModifier.PreparedTool)                           
                         },

////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseCommunityHall                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                                
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeCookhouseCommunityHall)                            
                         },
                          new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseCommunityHall                               
                                },                  
                                ParticleEmitters = cookhouseSmoke,  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeCookhouseCommunityHall,  (int)StateModifier.PreparedTool)                            
                         },
////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                                
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },
                          new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed                               
                                },                  
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseSmokeOven, (int)StateModifier.UpgradeCookhouseDryingShed,  (int)StateModifier.PreparedTool)                            
                         },
//////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseCommunityHall,
                                    cookhouseDryingShed                               
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },
                          new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseCommunityHall,
                                    cookhouseDryingShed                               
                                },                  
                                ParticleEmitters = cookhouseSmoke,
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseDryingShed,  (int)StateModifier.PreparedTool)                            
                         },


                        #endregion
                        #region 3 upgrades
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                                                   
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                                                   
                                },   
                                ParticleEmitters = cookhouseSmoke,
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                             
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.PreparedTool)                            
                         },
//////////////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,                                    
                                    cookhouseCommunityHall                               
                                },   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeStove)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,                                    
                                    cookhouseCommunityHall                               
                                },   
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeStove, (int)StateModifier.PreparedTool)                            
                         },
//////////////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                },  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeStove,  (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,                                    
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                },   
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeStove,  (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.PreparedTool)                            
                         },
//////////////////////
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                },     
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                },   
                                ParticleEmitters = cookhouseSmoke, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.PreparedTool)                            
                         },
//////////////////////

                        #endregion
                        #region 4 upgrades
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                }, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                                
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove, (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]
                                {
                                    cookhouse,
                                    cookhouseSmokeOven,
                                    cookhouseDryingShed,
                                    cookhouseCommunityHall                               
                                },   
                                ParticleEmitters = cookhouseSmoke,  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "cookhouse_g" },                                                           
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.UpgradeStove, (int)StateModifier.UpgradeCookhouseCommunityHall, (int)StateModifier.UpgradeCookhouseSmokeOven,  (int)StateModifier.UpgradeCookhouseDryingShed, (int)StateModifier.PreparedTool)                            
                         },
                        #endregion


                     }
                },
                ContainerType = new UpgradableBuildingContainerType() // UpgradableWorkshopContainerType() // note that the upgrade is a tool, and it should also have room for outputs and replenish items. This container is for general storage, like materials or extra tools
                {
                    CanBeEnteredByTags = new[] { "humanTransact" }, // NEW: they can now enter so they can salvage workshops inside, otherwise they would have to be unloaded/carried outside                                
                    UpgradesProfile = "cookhouseProfile",
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 4f), // has  storage for tools and materials
                    DefaultStorageSettings = "homeStorage", //because it may be expanded with community hall. Also, I need a place to put lots of starting stuff on Headway until we get a store house?
                    Doors = new Vector2[] { new Vector2(-12f, -11f) } //place to the north so that worker is hidden by billboard
                },
                ////////////////////////////
                GatheringSiteType = new GatheringSiteType()
                {
                    arc = new Arc()
                    {
                        Radius = 27f,
                        MinAngle = -180,
                        MaxAngle = 180,
                    },
                    MaxVisitors = 30
                },  
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageCookhouse",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 5 }, { "item:waterCaneStem", 3 }, { "item:spoakBranchesTrimmed", 2 }, { "item:spoakShingles", 2 }, { "item:textile", 3 }, { "item:stones", 1 } },
                    Repair = "buildingRepair"
                }
            });

            #endregion

            #region still
            listOfEntityTypes.Add(new EntityType("structure:still") //
            {
                Name = "Still",
                SummaryDescription = "Equipment for distilling liquid mixtures",//todo: reflux or pot still?
                Description = "Primarily used for producing ethanol for beverages such as brandy. The still boils liquids and condenses the vapor and this way it could be used for separating and purifying many types of chemicals.",//
                ThumbnailSmall = "HUD_thumbnail_still",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },//todo: not only for food?
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 0f, // 
                        PadShape = CollidePrim.Circle, //,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(15, -3f)},
                            new CollideShape2D(Vector2.Zero, 17f) { Offset = new Vector2(-15f, -3f)},

                        },
               /*     SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(21, -3), 22f)
                        }*/
                    }
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "still",
                                 
                            }                    
                    
                        },


                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "still_g",
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "still" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "still_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "still_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "still" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "still_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){ ParticleSystemKey = "smallerSmoke", Offset = new Vector2(-21,7)},
                                    new ParticleEmitterEffect(){ ParticleSystemKey = "smallFire", Offset = new Vector2(-21,7) },
                                }
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //    ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kitchenImprovisedLightFire"
                },

                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelCampfire",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////MP same as simple kitchen
                        }
                    },

                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // has  storage for tools 
                    DefaultStorageSettings = "uncooledKitchenStorage", //todo

                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageStill",
                    PartKeys = new SerializableDictionary<string, int>() { 
                                                            { "item:stillComponents", 1 }, { "item:stones", 1 }, { "item:clayJar", 1 }, { "item:sticks", 1 }},
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region improvisedWorkbench
            listOfEntityTypes.Add(new EntityType("structure:improvisedWorkbench")
            {
                Name = "Workbench (scraps)",
                SummaryDescription = "A work area for preparing materials and making items",
                Description = "Has a table for doing simple carpentry which improves efficiency and speeds up crafting of items.",
                TierOrArea = new TierOrArea() { Tier = "survival" },
                ThumbnailSmall = "HUD_thumbnail_workbenchImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 7f, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 26f),
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(-37f, -17f)},
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(24f, 17f)}
                        }
                    }
                },

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "workbenchImprovised",
                       //         BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "workbenchImprovised_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workbenchImprovised" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workbenchImprovised_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workbenchImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                ToolType = new ToolType()
                {
                  //  ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary
                },

                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // NEW: has  storage for tools 
                    DefaultStorageSettings = "workbenchStorage"
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageImprovisedWorkbench",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:panelScraps", 1 }, { "item:sticks", 3 }}, //check out breakdown time compared to averageConstruction..
                    Repair = "buildingRepair"
                } 
            });
            #endregion

            #region mudBrickWorkbench
            listOfEntityTypes.Add(new EntityType("structure:mudBrickWorkbench") //copy pasted from the other workbench, only diff is clay instead of panels scraps. no longer requires mudbricks
            {
                Name = "Workbench (simple)",
                SummaryDescription = "A work area for preparing materials and making items",
                Description = "Has a table for doing simple carpentry which improves efficiency and speeds up crafting of items.",
                TierOrArea = new TierOrArea() { Tier = "survival" },
                ThumbnailSmall = "HUD_thumbnail_workbenchImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                  //  Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 7f, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 26f),
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(-37f, -17f)},
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(24f, 17f)}
                        }
                    }
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "workbenchImprovised",
                       //         BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "workbenchImprovised_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workbenchImprovised" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workbenchImprovised_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "workbenchImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //  ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary

                    //IsHandTool = false
                },

                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 1f), // NEW: has  storage for tools 
                    DefaultStorageSettings = "workbenchStorage",

                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageMudBrickWorkbench",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 3 } }, //check out breakdown time compared to averageConstruction..}
                    Repair = "buildingRepair"
                }
            });
            #endregion

            /*
            #region metalLatheShopHumanPowered
            listOfEntityTypes.Add(new EntityType("structure:metalLatheShopHumanPowered") //copy pasted from the  workbench
            {
                Name = "Metal shop (human powered)",
                SummaryDescription = "A work area for turning and making cylindrical metal shapes",
                Description = "The workshop is built around a lathe: An essential tool for making precision-shaped metal items. The tool is powered by the operator's muscles through a simple gearbox.", //
                ThumbnailSmall = "HUD_thumbnail_latheWorkshop",
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.ByStatus,
                StructureType = new StructureType()
                {
                    Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = 7f, // 50f,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
     
                            new CollideShape2D(Vector2.Zero, 13f) { Offset = new Vector2(-37f, 0f)},
                            new CollideShape2D(Vector2.Zero, 30f) { Offset = new Vector2(0f, 0f)}
                        }
                },


                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "latheShopHumanPowered", 
                       //         BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "latheShopHumanPowered_g" 
                        }
                    },
                    SpriteConditions = new[]
                     {
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "latheShopHumanPowered" }},                                                                   
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ordered)                            
                         },
                         new SpriteConditionInfo()
                         {                                
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "latheShopHumanPowered_construct_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }
                     }
                },

                ToolType = new ToolType()
                {
                    //  ToolTag = new[] { "kitchen" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary

                    //IsHandTool = false
                },
                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("isolated", 3f), // NEW: has  storage for tools 
                    DefaultStorageSettings = "metalShopStorage",

                },               
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageMetalLatheShopHumanPowered",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:metalLathe", 1 }, { "item:humanPowerUnit", 1 }, { "item:sticks", 3 }, { "item:spoakShingles", 2 } }, //
                    Repair = "buildingRepair"
                }
            });
            #endregion 
            */

            #region smokeOven
            listOfEntityTypes.Add(new EntityType("structure:smokeOven")
            {
                Name = "Smoke oven",  // http://books.google.dk/books?id=deRKF5kv5wwC&pg=PA453&lpg=PA453&dq=storing+food+in+the+wilderness&source=bl&ots=EupkGs6c_I&sig=cOy19RAmiOOhwIv1YwzvznMWlXE&hl=da&sa=X&ei=T-w0UeL3DefY0QWBqoD4CA&ved=0CFMQ6AEwBQ#v=onepage&q=storing%20food%20in%20the%20wilderness&f=false
                SummaryDescription = "Structure for smoking food",
                Description = "Smoking is an ancient way to preserve meat and fish. This structure will cold smoke the food since the fire is placed away from the food. Consists of small teepee made from sticks and sod wherein the meat is placed. A small tunnel covered with stones is dug out and a fire is lit at the end of the tunnel.",
                ThumbnailSmall = "HUD_thumbnail_smokeOven",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    IsAddon = true,
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 8f, // NA changed it from 16f to 8f
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 17)
                            {
                                Offset= new Vector2(-12, 2) // this structure is asymmetric.  displace the geo to the left. 
                            }
                        }
                    }
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "smokeOven",
                                Offset = new Vector2(-12, 0f) // this structure is asymmetric.  displace the sprite to the left relative to the ground sprite

                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "smokeOven_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "smokeOven", Offset = new Vector2(-12, 0f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "smokeOven_construct", Offset = new Vector2(-12, 0f) }},    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "smokeOven", Offset = new Vector2(-12, 0f) }},   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "smokeOven_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke"}
                                }
                         }
                     }
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "smokeOven" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "lightFireWithoutFlames"
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, //mp animals cannot access.

                    ProductionOutputStorageType = new ItemStorageType(1f) //todo
                    {
                        FullStatePercentage = 0.1f,//mp todo?
                        HalfFullStatePercentage = 0.05f
                    },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelSmokeOven",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 5f ////MP feb 2016: reduced by 33%...was 7f     mp set it lower than fireplace 7f 
                        }                          
                    }                   
               },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageSmokeOven",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 }, { "item:stones", 1 }, { "item:firegrassSod", 1 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

     

            #region sensor
            listOfEntityTypes.Add(new EntityType("structure:sensor")
            {
                Name = "Motion sensor (deployed)",
                SummaryDescription = "Detects animals that enter its area",
                Description = "The sensor will monitor its immediate surroundings. To move the sensor, select SALVAGE",
                ThumbnailSmall = "HUD_thumbnail_sensorStructure",
                CategoryKey = "defense",
                StructureType = new StructureType()
                {

                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },

     /*           RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType(){ AssetName = "sensorStructure" }                    
                    
                    }
                }},*/
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //setting up advanced item
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "sensorStructure",
                       //         BaseCenter = new Vector2(0,0) //MP no coord
                            }                    
                    
                        },
                 /*       RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "_g"
                        }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "sensorStructure" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                   /*      new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "smokeOven_construct" }},                                    
                          //      RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "smokeOven_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }*/
                     }
                },


                SensorType = new SensorType()
                {
                    Range = 420,      //380   make sensor range longer than humans to give a nice effect when it comes online        
                    RangeAtNight = 420, // can see at night too
                    DetectionTypeKey = "motionSensor"
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 0f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 6)
                            {
                                Offset = new Vector2(0, 8)
                            }
                        }
                    }
                },
// MP may 29 '14: haven't used powercell part because there's no way to avoid the power cell degrading even when it's not in use, and this kind of ruins the idea of "saving power" by turning off equipment.
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageSensor",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sensor", 1 }, },   //mp necessary for making the salvage process work.
                    Repair = "buildingRepair",
                }
            });
            #endregion

            #region Port (simple)
            listOfEntityTypes.Add(new EntityType("structure:simplePort")
            { //What is the difference between port, pier, and harbor? https://answers.yahoo.com/question/index?qid=20130624152848AA41XSI 'port is a facility for receiving ships and transferring cargo to and from them'       https://answers.yahoo.com/question/index?qid=20100530145659AAHL8k4
                Name = "Port (simple)", //has a pier structure and a clay storehouse
                //COPY PASTE THE DEscriptions to the process description which is shown on the port location also!
                SummaryDescription = "Tiny port which allows us to receive boats and sell goods to other settlements. Small capacity. Suited for selling food.",
                Description = "Has a pier where small boats and barges can moor and a storehouse where goods intended for sale can be placed. The clay storehouse is raised on pillars to keep a small amount of goods safe from vermin. \nThe storehouse also has a space for storage of items that are not intended for sale.", // \nThe pier is made from heavy spoak branches whose open shapes allow water to pass through.
                ThumbnailSmall = "HUD_thumbnail_simplePier", //
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Pier //  
                },
              //  AccessPointDirection = new Vector3(0f, -1f, 0f), // scan North to set the accesspoint, since south is most likely blocked by water.
                ContainerType = new TerminalContainerType()
                {                        
                    //Doors = new[]{ new Vector2() },
                   // CanBeEnteredByTags = new[] { "humanTransact" },
                    CanTransactWithTags = new[]{ "humanTransact" },
                    OfferedForTradeStorageType = new ItemStorageType(8f), //mp making this the same as the barge capacity. makes it more simple I guess?
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType(2f),
                    DefaultStorageSettings = "simplePortLocalStorage", //default settings for the local area of the storage building. (Not for the sale area)
                    
                },
                SensorType = new SensorType() // required to buy/sell
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            {  
                                AssetName = "storeHouseClay",
                                Offset = new Vector2(18, -36f) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite    
  
                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        }, 
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        { 
                            AssetName = "pierSimple_g"
                        }
                    }, 
                    ClientStateConditions = new[]
                     { 
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay", Offset = new Vector2(18, -36f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay_construct", Offset = new Vector2(18, -36f) }},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "pierSimple_construct_g" },    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {  
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 46) { Offset = new Vector2(0, 0)},

                        },
                    
                        Shapes = new CollideShape2D[] //shape for blocking
                        {  
                            //new CollideShape2D(new Vector2(0, 0), 20) { Offset = new Vector2(25, -40) }, //NA after removing this with testing it does not seems like it does anything important.. was new Vector2(0, 0), 20) (new Vector2(25, -40))
                            new CollideShape2D(new Vector2(0, 0), 20) { Offset = new Vector2(30, -30) },  //NA was (new Vector2(0, 0), 10) { Offset = new Vector2(-4, -40)
                        }
                    }
                },
               
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageSimplePort",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranchesTrimmed", 3 }, { "item:spoakShingles", 1 }, { "item:solidMudBrick", 5 }, { "item:waterCaneStem", 3 } },
                    Repair = "buildingRepairCustomProcess"
                } 
            });
            #endregion

            #region Port (canopy)
            listOfEntityTypes.Add(new EntityType("structure:canopyPort")
            { //What is the difference between port, pier, and harbor? https://answers.yahoo.com/question/index?qid=20130624152848AA41XSI 'port is a facility for receiving ships and transferring cargo to and from them'       https://answers.yahoo.com/question/index?qid=20100530145659AAHL8k4
                Name = "Port (canopy)", //has a pier structure and an open storage area
                //COPY PASTE THE DEscriptions to the process description which is shown on the port location also!
                SummaryDescription = "Big port which allows us to receive boats and sell goods to other settlements. No vermin protection.",
                Description = "Has a pier where small boats and barges can moor and a large storage canopy where goods intended for sale can be placed. The goods are not protected from vermin, so this structure is NOT suited for trading food. \nThe canopy also has a space for storage of items that are not intended for sale.", // \nThe pier, where the boats will dock, is made of heavy spoak branches whose open shapes allow water to pass through.
                ThumbnailSmall = "HUD_thumbnail_storageCanopy",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Pier //  
                },
                //  AccessPointDirection = new Vector3(0f, -1f, 0f), // scan North to set the accesspoint, since south is most likely blocked by water.
                ContainerType = new TerminalContainerType()
                {
                    //Doors = new[]{ new Vector2() },
                    // CanBeEnteredByTags = new[] { "humanTransact" },
                    CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
                    OfferedForTradeStorageType = new ItemStorageType(80f), //
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType(20f),
                    DefaultStorageSettings = "simplePortLocalStorage", //default settings for the local area of the storage building. (Not for the sale area)

                },
                SensorType = new SensorType() // required to buy/sell
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            {  
                                AssetName = "storageCanopy",
                                Offset = new Vector2(18, -36f) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite    
  
                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "pierCanopy_g"
                        }
                    },
                    ClientStateConditions = new[]
                     { 
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay", Offset = new Vector2(18, -36f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                            //    RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay_construct", Offset = new Vector2(18, -36f) }}, //todo construction billboard
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "pierSimple_construct_g" },    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 46) { Offset = new Vector2(0, 0)},

                        },
                        Shapes = new CollideShape2D[] //shape for blocking
                        {  
                            new CollideShape2D(new Vector2(0, 0), 32) { Offset = new Vector2(20, -40) },
                       //     new CollideShape2D(new Vector2(0, 0), 10) { Offset = new Vector2(-4, -40) },
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageCanopyPort",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranchesTrimmed", 3 }, { "item:waterCaneStem", 3 }, { "item:daysheenLeaves", 2 } },
                    Repair = "buildingRepairCustomProcess" // use this because no production process
                }                
            });
            #endregion

            /////////////
            #region Port (large) //for OtherSite!!! VERY large capacity!
            listOfEntityTypes.Add(new EntityType("structure:largePier")
            {
                Name = "Port (large)", //for OtherSite. large capacity
                SummaryDescription = "Large port. Allows us to sell goods to other settlements",
                Description = "Has a pier where small boats and barges can moor and a storehouse with large capacity where goods intended for sale can be placed.",
                ThumbnailSmall = "HUD_thumbnail_simplePier", //
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Pier //  
                },
                ContainerType = new TerminalContainerType()
                {
                    //Doors = new[]{ new Vector2() },
                    // CanBeEnteredByTags = new[] { "humanTransact" },
                    CanTransactWithTags = new[] { "humanTransact" },
                    OfferedForTradeStorageType = new ItemStorageType(1000f), //very large capacity. large enough?
                    ItemStorageType = new ItemStorageType(2f)
                },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "storeHouseClay",
                                Offset = new Vector2(18, -36f) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite    

                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "pierSimple_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay", Offset = new Vector2(18, -36f) }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "storeHouseClay_construct", Offset = new Vector2(18, -36f) }},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "pierSimple_construct_g" },    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 46) { Offset = new Vector2(0, 0)},

                        },
                        Shapes = new CollideShape2D[] //shape for blocking
                        {
                            new CollideShape2D(new Vector2(0, 0), 20) { Offset = new Vector2(25, -40) },
                            new CollideShape2D(new Vector2(0, 0), 10) { Offset = new Vector2(-4, -40) },
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageSimplePort",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranchesTrimmed", 3 }, { "item:spoakShingles", 1 }, { "item:solidMudBrick", 5 }, { "item:waterCaneStem", 3 } },
                    //no repair - othersite building only. 
                }
            });
            #endregion


            #region improvised landing
            listOfEntityTypes.Add(new EntityType("structure:landingImprovised")
            {
                Name = "Landing (improvised)", //has a flag, ropes and some poles for rolling a boat on
                //COPY PASTE THE DEscriptions to the process description which is shown on the port location also!
                SummaryDescription = "Place for boats to moor transfer passengers. Goods can ONLY be received, not sold",
                Description = "This simple structure does not allow us to sell goods because it lacks a storehouse. It is only suited for receiving goods and embarking and disembarking passengers.",
                ThumbnailSmall = "HUD_thumbnail_landingImprovised", //
                CategoryKey = "miscellaneous",
                TierOrArea = new TierOrArea() { Tier = "basic" }, //or survival?
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },
              //  AccessPointDirection = new Vector3(0f, -1f, 0f), // scan North to set the accesspoint, since south is most likely blocked by water.              
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Pier  
                },
                // no sensor needed because no trading
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "landingImprovised",
                                Offset = new Vector2(-20,-28) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite   (-20, -16f)  

                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "landingImprovised_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "landingImprovised", Offset = new Vector2(-20,-28) }},   //todo                                                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "landingImprovised_g" },    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 32) { Offset = new Vector2(-6, -6)},

                        },
                        Shapes = new CollideShape2D[] //shape for blocking
                        {
                            new CollideShape2D(new Vector2(0, 0), 10) { Offset = new Vector2(-20, -30) },
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageLandingImprovised",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:waterCaneStem", 1 } },
                    Repair = "buildingRepairCustomProcess" // "buildingRepair"
                }
            });
            #endregion


            #region helipad
            listOfEntityTypes.Add(new EntityType("structure:helipad")
            {
                Name = "Helipad",
                SummaryDescription = "Indicates an area for a VTOL aircraft to land",
                Description = "Any vertically landing aircraft will be looking for the big 'H' when selecting a spot to land on. This structure allows aircraft to deliver items and personnel to us, but does not make us able to sell items.",
                ThumbnailSmall = "HUD_thumbnail_helipad", //
                TierOrArea = new TierOrArea() { Tier = "basic" }, //not sure.. not really used
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {                   
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"],                    
                },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
                },
               
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "helipad",
                        //        BaseCenter = new Vector2(0,0) //MP no coord
                            }                    
                    
                        },
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                               {
                                   AssetName = "helipad_g"
                               }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "helipad" }},     
                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "helipad_g" },
                               Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 30f,///////////////////////////////////////////////////////////////
                        SelectionShapes = new CollideShape2D[] //no blocking, this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 42)
                            {
                                Offset = new Vector2(0, 0)
                            }
                        }
                        /*
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 2)
                            {
                                Offset = new Vector2(20, 20)
                            }
                        }*/
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", //very sturdy. 
                    SalvageProcess = "salvageHelipad",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 1 } }, // MP jan 2016: without parts it will dissappear when degraded! this confuses the player.               
                    Repair = "buildingRepair"
                } 
            });
            #endregion

            #region big helipad
            listOfEntityTypes.Add(new EntityType("structure:helipadBig")
            {
                Name = "Helipad (big)",
                SummaryDescription = "Storehouse and helipad for trading with VTOL aircraft",
                Description = "This landing spot has storage space for items intended for sale. This allows us to trade with aircraft from other colonies.", //
                ThumbnailSmall = "HUD_thumbnail_helipadBig", //
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType() //mp below copy+pasted from the other helipad apart from the sprite info
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },

                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
                },
                ContainerType = new TerminalContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    OfferedForTradeStorageType = new ItemStorageType(8f), //same as simple port
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType(2f), // offer a little space for general storage as well.
                    DefaultStorageSettings = "simplePortLocalStorage", //default settings for the local area of the storage building. (Not for the sale area)
                },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "helipadBigStorehouse",
                                Offset = new Vector2(49, -28f) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite    

                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "helipadBig_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "helipadBigStorehouse", Offset = new Vector2(49, -28f) }}, 
                             //   RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "helipadBig_g" }, //DOES NOT WORK WITH A GHOSTED GROUND SPRITE. must be billboard.                                
                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "helipadBig_construct_g" },
                               Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] //no blocking, this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 42)  { Offset = new Vector2(-31, 3) }
                        },                    
                        Shapes = new CollideShape2D[] //shape for blocking
                        {
                            new CollideShape2D(new Vector2(0, 0), 26) { Offset = new Vector2(48, -27) },
                            new CollideShape2D(new Vector2(0, 0), 10) { Offset = new Vector2(46, -1) },
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageHelipadBig",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:structurePanels", 2 } }, //
                    Repair = "buildingRepair"
                } //very sturdy.
            });
            #endregion

            #region heliport VERY LARGE for othersites
            listOfEntityTypes.Add(new EntityType("structure:heliportLarge")
            {
                Name = "Heliport", //for OtherSite. large capacity
                SummaryDescription = "Storehouse and helipad for trading with VTOL aircraft",
                Description = "This landing spot has storage space for items intended for sale. This allows us to trade with aircraft from other colonies.", //
                ThumbnailSmall = "HUD_thumbnail_helipad", //
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType() //mp below copy+pasted from the other helipad apart from the sprite info
                {
                    BuildByPlayer = true,
                    //Category = GameData.Instance.AllStructureCategories["miscellaneous"],
                },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
                },
                ContainerType = new TerminalContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },
                    OfferedForTradeStorageType = new ItemStorageType(1000f), //A LOT of room.
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType(2f), // offer a little space for general storage as well.
                    DefaultStorageSettings = "simplePortLocalStorage", //default settings for the local area of the storage building. (Not for the sale area)
                },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24f,
                    RangeAtNight = 24f
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "helipadBigStorehouse",
                                Offset = new Vector2(49, -28f) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite    

                                //BaseCenter property not set - using default because base center should be at the center of the billboard
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "helipadBig_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                             //   RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "helipadBigStorehouse" Offset = new Vector2(49, -28f) }}, 
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "helipadBig_g" }, //trying this..might not render good as ghosted..
                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "helipadBig_g" },
                               Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f,
                        SelectionShapes = new CollideShape2D[] //no blocking, this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 42)  { Offset = new Vector2(-31, 3) }
                        },
                        Shapes = new CollideShape2D[] //shape for blocking
                        {
                            new CollideShape2D(new Vector2(0, 0), 26) { Offset = new Vector2(48, -27) },
                            new CollideShape2D(new Vector2(0, 0), 10) { Offset = new Vector2(46, -1) },
                        }
                    }
                },
                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:structurePanels", 2 } },
                    //no repair - othersite building only.
                } 
            });
            #endregion

            #region fieldLab
            listOfEntityTypes.Add(new EntityType("structure:fieldLab")
            {
                Name = "Field lab (deployed)",
                SummaryDescription = "Lab set up and ready to use for analyzing specimens and make new chemical substances",
                Description = "With the lab, we can make new enzymes to treat ingredients which would otherwise be inedible to us.\n The deployed field lab can be packed down and carried by ordering SALVAGE.", //mp: the following is confusing, should be mentioned on the packed down item only, right? : \n It has been suggested in our group to salvage the field lab for component parts that can be used for building a weapon.
                ThumbnailSmall = "HUD_thumbnail_fieldLab",
                TierOrArea = new TierOrArea() { Tier = "survival" }, //the item part is advanced
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                   // Category = GameData.Instance.AllStructureCategories["production"],
                    BuildByPlayer = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 1f, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {     
                     
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(-17f, -12f)},
                            new CollideShape2D(Vector2.Zero, 11f) { Offset = new Vector2(17f, -8f)}
                        },
                        SelectionShapes = new CollideShape2D[] // this is a shape for selecting.
                        {
                            new CollideShape2D(new Vector2(0, 0), 20f)  { Offset = new Vector2(0, 0) }
                        }
                    }
                },

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "fieldLab",
                       //         BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fieldLab_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "fieldLab" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                             //   RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "workbenchImprovised_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "fieldLab_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                ToolType = new ToolType()
                {
                  //  ToolTag = new[] { "fieldLab" }, //todo. MP is a tooltag here always needed? isnt it enough to define it in toolsloader?
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageFieldLab",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:fieldLabPacked", 1 } }, //necessary with a part here - to make the salvage process work.
                    Repair = "tentRepair"
                } //using moisture resistant structure degrade profile because player doesnt know about moisture levels on map.
            });




            listOfEntityTypes.Add(new EntityType("structure:molecularAssembler")
            {
                Name = "Molecular assembler",
                SummaryDescription = "Creates complex products with molecular precision",
                Description = "The molecular assembler works by putting together molecules of matter in a bottom-up fashion. It can create the most advanced products in a short time, with minimal waste.",
                ThumbnailSmall = "HUD_thumbnail_sensorStructure",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },

      /*          RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType(){ AssetName = "molecularAssembler" }                    
                    
                    }
                }},*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "molecularAssembler",
                     //           BaseCenter = new Vector2(0,0) //mp no coord
                            }                    
                    
                        },
               /*         RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "daysheenTipi_g"
                        }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "molecularAssembler" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                 /*        new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "daysheenTipi_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }*/
                     }
                },

                ToolType = new ToolType()
                {                   
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary                    
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 8f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 11)
                            {
                                Offset = new Vector2(-5, 6)
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageMolecularAssembler",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:vacuumChamber", 1 }, { "item:assemblerCabinet", 1 }, { "item:assemblerCooling", 1 } },
                    Repair = "tentRepair"
                }
            });
            #endregion


            #region radioHutImprovised //todo make one from shingles also, which lasts longer.
            listOfEntityTypes.Add(new EntityType("structure:radioHutImprovised")
            {
                Name = "Radio hut (improvised)",
                SummaryDescription = "An improvised radio station. Used for communication with other settlements",//
                Description = "A shelter for the radio, quickly constructed for urgent communication. \nMost settlements maintain communication through radio when satellite communication is no longer available. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
                ThumbnailSmall = "HUD_thumbnail_radioHut",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CommunicatorType = new CommunicatorType()
                {
                    
                    Method = CommunicationMethod.Radio, //
                    Range = null
                },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24,
                    RangeAtNight = 24
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "radioHut",                   
                            }     
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "radioHut_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "radioHut" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "radioHut_construct" }}, 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 5f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 20)
                            {
                                Offset = new Vector2(0, 0)
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction",  //mp  compare structure:improvisedKitchen DegradeType = "adequateConstruction"
                    SalvageProcess = "salvageRadioHutImprovised",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:radio", 1 }, { "item:radioAntenna", 1 }, { "item:shadeleafCanes", 3 }, { "item:spoakLeaves", 2 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion

            #region radioHut // from shingles, which last longer.
            listOfEntityTypes.Add(new EntityType("structure:radioHut")
            {
                Name = "Radio hut",
                SummaryDescription = "A simple radio station. Used for communication with other settlements",//
                Description = "A durable structure which houses the radio. \nMost settlements maintain communication through radio when satellite communication is no longer available. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
                ThumbnailSmall = "HUD_thumbnail_radioHut",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CommunicatorType = new CommunicatorType()
                {

                    Method = CommunicationMethod.Radio, //
                    Range = null
                },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor",
                    Range = 24,
                    RangeAtNight = 24
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "radioHut",                   
                            }     
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "radioHut_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "radioHut" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "radioHut_construct" }}, 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         }
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 5f, 
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 20)
                            {
                                Offset = new Vector2(0, 0)
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "adequateConstruction",  //mp  compare structure:improvisedKitchen DegradeType = "adequateConstruction"
                    SalvageProcess = "salvageRadioHut",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:radio", 1 }, { "item:radioAntenna", 1 }, { "item:shadeleafCanes", 3 }, { "item:spoakShingles", 2 } },
                    Repair = "buildingRepair"
                }
            });
            #endregion


            #region satelliteGroundStation
            listOfEntityTypes.Add(new EntityType("structure:satelliteGroundStation")
            {
                Name = "Satellite ground station (deployed)",
                SummaryDescription = "Communicates with a satellite",
                Description = "This equipment can be used to communicate with other sites on the planet via the network of satellites in orbit. When deployed, we will be able to receive messages from all other sites.",
                ThumbnailSmall = "HUD_thumbnail_satelliteDish",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["miscellaneous"]                  
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                 CommunicatorType = new CommunicatorType()
                 {
                      // TODO: satellite ground station should be a Tool, inside a Comm shed
                      Method = CommunicationMethod.Satellite,
                      Range = null
                 },
                SensorType = new SensorType()
                {
                    DetectionTypeKey = "communicationSensor", 
                    Range = 24,
                    RangeAtNight = 24
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.None,
                    IsMobile = false,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "satelliteDish",
                   //             BaseCenter = new Vector2(0,0) //mp no coord
                            }                   
                    
                        },                       
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "satelliteDish" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                 /*        new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "daysheenTipi_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }*/
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 8f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 11)
                            {
                                Offset = new Vector2(2, 8)
                            }
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageSatelliteGroundStation",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:satelliteGroundStation", 1 } },
                    Repair = "buildingRepair",
                }
            });
            #endregion

            #region weatherStation
            listOfEntityTypes.Add(new EntityType("structure:weatherStation")
            {
                Name = "Weather station (deployed)",
                SummaryDescription = "Gathers data about the current weather situation",
                Description = "",
                ThumbnailSmall = "HUD_thumbnail_weatherAntenna", //
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },

  /*              RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]
                    {                      
                        new RenderAsBillboardType(){ AssetName = "weatherAntenna" }                    
                    
                    }
                }},*/

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "weatherAntenna",
                          //      BaseCenter = new Vector2(35,0) //mp no coord
                            }                    
                    
                        },
                        /*         RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                 {
                                     AssetName = "daysheenTipi_g"
                                 }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "weatherAntenna" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                 /*        new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "daysheenTipi_construct" }},                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "daysheenTipi_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         }*/
                     }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 4f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 11)
                            {
                                Offset = new Vector2(2, 27)
                            }
                        }
                    }
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageWeatherStation",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:weatherStationMast", 1 }, { "item:weatherStationSensors", 1 } },
                    Repair = "buildingRepair",
                }
            });
            #endregion


            #region turnip hut
            listOfEntityTypes.Add(new EntityType("structure:turnipHut")
            {
                Name = "Turnip hut",
                SummaryDescription = "Modest hut made from a hollow turnip shell", //
                Description = "The shell of a grown turnip can be used as roof and walls in a small building. It needs to be thoroughly cleaned and placed on a foundation of firegrass sod.", 
                ThumbnailSmall = "HUD_thumbnail_turnipHut",
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                CategoryKey = "shelter",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = housingComfortSurvivalHigh },

                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,
                    UpgradesProfile = "survivalHome3People",
                    Doors = new Vector2[]
                        {
                            new Vector2(12,19),
                        }                    
                },

                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "turnipHut",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "turnipHut_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "turnipHut"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                          new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "turnipHut"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         }
                       /*  new SpriteConditionInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "turnipHut_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)
                         }*/
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                           new CollideShape2D(new Vector2(-14,3), 22),
                           new CollideShape2D(new Vector2(0,0), 23),
                           new CollideShape2D(new Vector2(18,1), 16),
                        }
                    }
                },               
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageTurnipHut",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:firegrassSod", 1 }, { "item:sticks", 2 }, { "item:turnipShell", 1 } },
                    Repair = "buildingRepairCustomProcess" // use this because there is no proper production process 
                }
            });
            #endregion

            #region clay hut
            listOfEntityTypes.Add(new EntityType("structure:clayHut")
            {
                Name = "Clay hut",
                SummaryDescription = "Small, comfortable building for 4 occupants",
                Description = "A simple, permanent building made from mudbricks that are carefully stacked and supported by a wooden framework. After constructing the walls and the roof, a clay plaster is applied to make the building waterproof.", //
                ThumbnailSmall = "HUD_thumbnail_clayPolyhedron",
                CategoryKey = "shelter",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 4, ComfortLevel = housingComfortBasicHigh },

                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,
                    UpgradesProfile = "basicHome4People",
                    Doors = new Vector2[]
                    {
                            new Vector2(-25,15),
                    }                    
                },                
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Comfort },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "clayPolyhedron", 
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "clayPolyhedron_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "clayPolyhedron"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{ new RenderAsBillboardType(){ AssetName = "clayPolyhedron_construct"},},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "clayPolyhedron_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt) 
                         },
                         new ClientStateInfo()//no stove, at the moment not used:
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{ new RenderAsBillboardType(){ AssetName = "clayPolyhedron"},},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "clayPolyhedron_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BurningFuel), 
                                ParticleEmitters = new[]{ 
                                    new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke",Offset = new Vector2(0,-30)} //chimney smoke                                  
                                }
                         }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 5f, // NA was padRadiusShelterAndStorage
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(19,7), 22),
                            new CollideShape2D(new Vector2(-14,2), 22),
                            new CollideShape2D(new Vector2(4,0), 22),
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction", //
                    SalvageProcess = "salvageClayHut",
                    Repair = "buildingRepair",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 5 }, { "item:spoakBranchesTrimmed", 1 }, { "item:spoakShingles", 1 }, { "item:stones", 1 } }
                }
            });
            #endregion

            #region cane hut
            listOfEntityTypes.Add(new EntityType("structure:caneHut")
            {
                Name = "Cane hut",
                SummaryDescription = "Handsome dwelling made from water cane. Houses 3 people.", //
                Description = "In warmer, wetter areas where water cane is abundant, this building design is a good option. The hut is raised from the ground on pillars, making it suitable for wetlands. Some clay plaster keeps the wind out but the hut will not be very comfortable in cold regions.", //
                ThumbnailSmall = "HUD_thumbnail_caneHut",
                CategoryKey = "shelter",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact", "leafcutterTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = housingComfortBasicLow },

                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,
                    UpgradesProfile = "basicHome3People",                
                    Doors = new Vector2[]
                    {
                            new Vector2(-27,8),
                    }                    
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Comfort },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "caneHut",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "caneHut_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "caneHut"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{ new RenderAsBillboardType(){ AssetName = "caneHut_construct"},},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt) 
                         }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(19,7), 22),
                            new CollideShape2D(new Vector2(-17,6), 25),
                            new CollideShape2D(new Vector2(4,5), 25),
                        }
                    }
                },
               
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageCaneHut",
                    Repair = "buildingRepair",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 1 }, { "item:waterCaneStem", 7 } }
                }
            });
            #endregion



            #region Sentry (deployed)

            listOfEntityTypes.Add(new EntityType("structure:sentry")
            {
                Name = "Sentry (deployed)",
                FormalName = "TRIAAD",
                SummaryDescription = "Autonomous gun turret for area defence",
                Description = "The TRIAAD is a stationary machine gun for area defence. It has sensors and a degree of AI for operating in all conditions, and has been optimized for an alien environment containing unknown threats. Its low power consumption and deep magazine makes it able to operate unsupervised for extended periods of time.",
                ThumbnailSmall = "HUD_thumbnail_sentry",                
                ThumbnailBig = "sentry",
              //  SharedSpecialActions = new[] { new Pair<string, bool>("enableAttackVermin", true ), new Pair<string, bool>("disableAttackVermin", false ) },
                ContainerType = new ReplenishContainerType()
              {
                  CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside
                  
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                       
                    }                  
              },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = 0.6f * MathHelper.Pi,
                    RotatorType = new RotatorType() { BoneKeyName = "tower_joint" /* "ammoAndpivot1"*/ /*"barrel"  "barrel_joint" */}
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //note that the sentry structure is survival (once they have it they might as well put it up) but the item part is advanced
                CategoryKey = "defense",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },
                RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "sentry",
                       ModelScale = 2.2f,

                       DefaultInfo = new AnimConditionInfo()
                       {
                           // use this neutral anim when all flags are cleared:
                           SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } } //"combatIdle" pointing straight out
                       },
                       AnimConditions = new AnimConditionInfo[] 
                        {        
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idle" }}, //"idle" scanning
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                           new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } },
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )}   
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "shoot" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier))},
                            }                        
                        }
                   }
                   
               },

                DefaultSimState = new SimStateInfo()
                {   
                    GeometryLayoutType = new GeometryLayoutType()//mp why is its center so high up? the orange marker dot used for 3d models...
                    {
                        Pad = 0f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 6)
                            {
                                Offset = new Vector2(0, 9)
                            }
                        }
                    }
                },
                BodyType = GameData.Instance.AllBodyTypes["sentry"],
                SensorType = new SensorType()
                {
                    Range = 420,      //380   make sensor range longer than humans to give a nice effect when it comes online        
                    RangeAtNight = 420, // can see at night too
                    DetectionTypeKey = "motionSensor"
                },                
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.LikeHumans,
                    AggroRange = 300f, // less than the shooting range...
                    IsMobile = false,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,     
                    CanDoJobs = false,
                    HuntsVermin = true,
                    ServantForEntityTypeTag = "servesHumans",
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "shooting" , 0.9f
                        }
                    },
                    IntrinsicWeapons = new[]{ "item:sentryGun" }                 
                },                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageSentry",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sentry", 1 }, },
                   // Repair = "buildingRepair", // no repair
                }

            });
            #endregion


            #region Spray gun sentry (deployed)

            listOfEntityTypes.Add(new EntityType("structure:sprayGunSentry")
            {
                Name = "Spray gun sentry (deployed)",
                SummaryDescription = "Autonomous turret, modified with an improvised spray gun",
                Description = "We have dismounted the machine gun and jerry-rigged a fire extinguisher gun onto the turret, making it able to shoot a poisonous liquid at approaching twinklers. For ammunition, it uses cartridges filled with bush dragon poison.",
                ThumbnailSmall = "HUD_thumbnail_sentry",
                ThumbnailBig = "sentry",


                ContainerType = new ReplenishContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                    RequiresReplenishType = new RequiresReplenishType()
                    {

                    }

                },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = 0.6f * MathHelper.Pi,
                    RotatorType = new RotatorType() { BoneKeyName = "tower_joint" /* "ammoAndpivot1"*/ /*"barrel"  "barrel_joint" */}
                },
                CategoryKey = "defense",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    //Category = GameData.Instance.AllStructureCategories["defense"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "sentry",
                        ModelScale = 2.2f,

                        DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } } //"combatIdle" pointing straight out
                        },
                        AnimConditions = new AnimConditionInfo[] 
                        {              
                            
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idle" }}, //"idle" scanning
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },

                           new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } },
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
  
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "shoot" }}, //todo spray SFX
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier))},
                            },
                        
                        }
                    }

                },

                DefaultSimState = new SimStateInfo()
                {   
                    GeometryLayoutType = new GeometryLayoutType()//mp why is its center so high up? the orange marker dot used for 3d models...
                    {
                        Pad = 0f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 6)
                            {
                                Offset = new Vector2(0, 9)
                            }
                        }
                    }
                },
                BodyType = GameData.Instance.AllBodyTypes["sentry"],
                SensorType = new SensorType()
                {
                    Range = 420,      //380   make sensor range longer than humans to give a nice effect when it comes online        
                    RangeAtNight = 420, // can see at night too
                    DetectionTypeKey = "motionSensor"
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.LikeHumans,
                    AggroRange = 300f, // less than the shooting range...
                    IsMobile = false,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "shooting" , 0.9f
                        }
                    },
                    IntrinsicWeapons = new[] { "item:sentrySprayGun" },
                    /* Attacks = new[]
                      {
                          "sentryShootGun" // moved to weapon type
                      }*/
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    SalvageProcess = "salvageSprayGunSentry",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spraySentry", 1 }, } ,
                    Repair = "buildingRepair",
                }

            });
            #endregion


            #region Shotgun sentry (deployed)

            listOfEntityTypes.Add(new EntityType("structure:shotgunSentry")
            {
                Name = "Shotgun sentry (deployed)",
                SummaryDescription = "Autonomous turret outfitted with a shotgun",
                Description = "We have dismounted the machine gun and attached a shotgun onto the turret.",
                ThumbnailSmall = "HUD_thumbnail_sentry",
                ThumbnailBig = "sentry",
               // SharedSpecialActions = new[] { new Pair<string, bool>("enableAttackVermin", true ), new Pair<string, bool>("disableAttackVermin", false) },
                ContainerType = new ReplenishContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                    RequiresReplenishType = new RequiresReplenishType()
                    {

                        /*RequiresPowerType = new RequiresPowerType()
                        {
                            PowerCellCapacity = 2
                        }*/
                    }

                },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = 0.6f * MathHelper.Pi,
                    RotatorType = new RotatorType() { BoneKeyName = "tower_joint" /* "ammoAndpivot1"*/ /*"barrel"  "barrel_joint" */}
                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                CategoryKey = "defense",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    //Category = GameData.Instance.AllStructureCategories["defense"]
                },
                RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "sentry",
                        ModelScale = 2.2f,

                        DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } } //"combatIdle" pointing straight out
                        },
                        AnimConditions = new AnimConditionInfo[] 
                        {              
                            
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idle" }}, //"idle" scanning
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },

                           new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } },
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
  
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new ClientSide.RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "shoot" }}, //todo spray SFX
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier))},
                            },
                        
                        }
                    }

                },

                DefaultSimState = new SimStateInfo()
                { 
                    GeometryLayoutType = new GeometryLayoutType()//mp why is its center so high up? the orange marker dot used for 3d models...
                    {
                        Pad = 0f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 6)
                            {
                                Offset = new Vector2(0, 9)
                            }
                        }
                    }
                },
                BodyType = GameData.Instance.AllBodyTypes["sentry"],
                SensorType = new SensorType()
                {
                    Range = 420,      //380   make sensor range longer than humans to give a nice effect when it comes online        
                    RangeAtNight = 420, // can see at night too
                    DetectionTypeKey = "motionSensor"
                },
                IntelligenceType = new IntelligenceType()
                {
                    StrengthRating = Entities.StrengthRating.LikeHumans,
                    AggroRange = 300f, // less than the shooting range...
                    IsMobile = false,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "shooting" , 0.9f
                        }
                    },
                    IntrinsicWeapons = new[] { "item:sentryShotgun" },
                    /* Attacks = new[]
                      {
                          "sentryShootGun" // moved to weapon type
                      }*/
                },
                
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction", 
                    /*SalvageProcess = "salvageSprayGunSentry"*/ //todo
                    PartKeys = new SerializableDictionary<string, int>() { { "item:shotgunSentry", 1 }, },
                    Repair = "buildingRepair",
                } 

            });
            #endregion


            #region Animal Traps


            #region spike trap
            listOfEntityTypes.Add(new EntityType("structure:spikeTrap")
            {
                Name = "Spring trap (deployed)",
                SummaryDescription = "4 clenching iron spikes set off by a trigger plate", //
                Description = "Bait can be put in the center of the trap or the trap can be placed without bait in an area frequented by the target animal. The trap is strong enough to kill most small animals and injure larger ones.", //
                ThumbnailSmall = "HUD_thumbnail_spikeTrap",
                CategoryKey = "defense",
                Triggers = new TriggerType[] 
                { 
                    GameData.Instance.AllTriggerTypes["spikeTrapTrigger"]//strength and type of trap attack..creatures with this tag can trigger the trap (set it in creatureloader) not necessarily the same that can eat the bait, defined below.
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },
                ContainerType = new StorageContainerType()
                {
                    CanTransactWithTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, //creatures that are able to access (and eat) the bait. I think they should only be animals that also trigger it.
                    //Question: does this tag define which animals are attracted to the trap, meaning: will an animal who is not able to access, get attracted anyaway (probably not) (..big animals that trigger the trap will suffer injury but not get killed..
                    ItemStorageType = new ItemStorageType("exposed", 0.15f), //max bulk room available. make sure it allows for the bait type options.
                    DefaultStorageSettings = "trapBaitStorage", //bso don't use a storage that allows carcass, this is to prevent the caught carcass to be placed as new bait.
                    //mp I set it so it correspond to the bait options below.                    
                },
                SharedSpecialActions = new[] { 
                    new Pair<string, bool>("changeBaitToBlackpulp", true ), 
                    new Pair<string, bool>("changeBaitToGlassyCreeper", true ), 
                    new Pair<string, bool>("changeBaitToRatMeat", true ), 
                    new Pair<string, bool>("changeBaitToNoBait", true ) },
                //these are the bait options that the player is presented with. once set, the agents will carry ONE of that item TO the trap when resetting it. If the stockpile is open, they will then stockpile it.  MP maybe give the player more options.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "metalSpikeTrapSet_g" },
                    },

                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[] {  new RenderAsBillboardType() { AssetName = "metalSpikeTrapSprung"  }},  //mp  can only use billboard (not groundsprite) for .Ordered state rendering.                                                               
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "metalSpikeTrapSprung" }},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[] {  new RenderAsBillboardType() { AssetName = "metalSpikeTrapSprung"  } },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Inactive)                  
                         },
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f, // small pad. does not seem to prevent agents them from resting there, sadly.
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 14)//
                            {
                                Offset= new Vector2(0,0)//
                                
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageSpikeTrap",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spikeTrap", 1 } } ,
                    Repair = "buildingRepair",
                }
            });
            #endregion
            #region deadfall trap
            listOfEntityTypes.Add(new EntityType("structure:deadfallTrap")
            {
                Name = "Deadfall trap",
                SummaryDescription = "A heavy rock supported by a stick. Triggered when the bait is taken.",
                Description = "This simple trap has an in-built area where the bait has to be placed. When an animal tugs at the bait, the rock falls down on top of it, killing it.",
                ThumbnailSmall = "HUD_thumbnail_deadfall",
                TierOrArea = new TierOrArea() { Tier = "survival" },
                CategoryKey = "defense",
                Triggers = new TriggerType[] 
                { 
                    GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"]
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },
                ContainerType = new StorageContainerType()
                {
                    CanTransactWithTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact" }, //creatures that are able to access (and eat) the bait. I think they should only be animals that also trigger it. (..matter of game design if we want big animals to come and eat without getting killed by trap.
                    ItemStorageType = new ItemStorageType("exposed", 0.15f), //max bulk room available. make sure it allows for the bait type options.
                    DefaultStorageSettings = "trapBaitStorage", //bso don't use a storage with carcass, this is to prevent the caught carcass to be placed as new bait.                    
                },               
                SharedSpecialActions = new[] // these work differently from farm plots
                { 
                    new Pair<string, bool>("changeBaitToBlackpulp", true ), 
                    new Pair<string, bool>("changeBaitToGlassyCreeper", true ), 
                    new Pair<string, bool>("changeBaitToRatMeat", true ), 
                    new Pair<string, bool>("changeBaitToNoBait", true) 
                },
                //these are the bait options that the player is presented with. once set, the agents will carry ONE of that item TO the trap when resetting it. If the stockpile is open, they will then stockpile it.  MP maybe give the player more options.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "deadfallSet",
                           //     BaseCenter = new Vector2(0,0) //
                            }                   
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "deadfallSet_g" }, 
                    },

                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "deadfallSet" }},                                                                
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "deadfallSprung" }},
                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "deadfallSprung_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },

                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[] {  new RenderAsBillboardType() { AssetName = "deadfallSprung"  } },  
                          RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "deadfallSprung_g" },                                                            
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Inactive)                  
                         },
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f, // small pad. does not seem to prevent agents them from resting there, sadly.
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 14)//
                            {
                                Offset= new Vector2(0,0)//
                                
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageDeadfallTrap",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 1 } }, //salvageDeadfallTrap   
                    Repair = "buildingRepair",
                } //add salvage process
            });
            #endregion
            #region spring snare
            listOfEntityTypes.Add(new EntityType("structure:springSnare")
            {
                Name = "Snare", //was: spring snare
                SummaryDescription = "Simple trap that uses a noose connected to an elastic pole, triggered by touch",
                Description = "This trap type should be set up in areas often frequented by the target animal, such as game trails or feeding grounds. Because the trap does not have an in-built bait area, any bait must be placed separate from the trap.",
                ThumbnailSmall = "HUD_thumbnail_springSnare",
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                CategoryKey = "defense",
                Triggers = new TriggerType[] 
                { 
                    GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"] //trigger type defines size of kill zone, and what animals can be killed by it, and some more, see basedataloader.
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "springSnareSet",
                           //     BaseCenter = new Vector2(0,0) //
                            }                   
                    
                        }, 
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "springSnareSet" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {
                               RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "springSnareSprung" }},   //                              

                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[] {  new RenderAsBillboardType() { AssetName = "springSnareSprung"  } },                                                              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Inactive) //bso using the Flavour2 flag to change sprite to triggered                       
                         }
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f, // small pad. does not seem to prevent agents them from resting there, sadly.
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 14)//
                            {
                                Offset= new Vector2(0,0)//
                                
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageSpringSnare",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:shadeleafCanes", 1 } } ,
                    Repair = "buildingRepair",
                } 
            });
            #endregion
            #region landmine
            listOfEntityTypes.Add(new EntityType("structure:landMine")
            {
                Name = "Land mine (deployed)",
                SummaryDescription = "Explosive device triggered by pressure", //
                Description = "When a sufficiently heavy target steps on the trigger, it will set off a flintlock mechanism which ignites the black powder charge. The explosion will be strong enough to kill most animals in the vicinity.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "defense",
                Triggers = new TriggerType[] 
                { 
                    GameData.Instance.AllTriggerTypes["mineTrigger"]
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["defense"]
                },

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "wire",
                            }                    
                    
                        },
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "wire" }},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         }
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f, // small pad. does not seem to prevent agents them from resting there, sadly.
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 7)//bso TO DO: disable the collider in terrain cost calculation
                            {
                                Offset= new Vector2(0,0)//offset the circle up and to the left of entity center
                                
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageLandMine",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:landMine", 1 }} ,
                    Repair = "buildingRepair",
                }
            });
            #endregion
            #endregion

/////////////Fish Traps
            #region fishTrap
                #region fishTrapContainerType
            ContainerType fishTrapContainerType = new StorageContainerType("underWater", 0.76f) //OtherContainerType("underWater", 0.76f) // new StorageContainerType("earthCooled", 0.76f)
                {
                    CanTransactWithTags = new[] { "humanTransact"},
                    DefaultStorageSettings = "fishTrapCage",
                    AllowStockpiling = false
                };
                #endregion
 
                #region fishTrapGeometryLayoutType
                GeometryLayoutType fishTrapGeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = 22f,
                    PadShape = CollidePrim.Circle,
                    SelectionShapes = new CollideShape2D[] //mp not collision. this is  used to define the size and location of the selectable area
                    {
                        new CollideShape2D(new Vector2(0, 0), 13) 
                        {
                            Offset= new Vector2(0, 0)
                        },
                        new CollideShape2D(new Vector2(0, 0), 19)
                        {
                            Offset= new Vector2(4, 21)
                        }
                    }
                };
                #endregion
                #region structureType
                StructureType structureType = new StructureType()
                {
                    BuildByPlayer = true,
                    UsesAnchor = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                };
                #endregion
                #region toolType
                ToolType toolType = new ToolType()
                {
                    ToolTag = new[] { "fishTrap" },

                    // pseudo tool?
                    IsPseudoTool = true
                    /*
                    Durability = ItemLoader.toolDurabilityUnbreakable,
                    ToolHandling = ToolHandlingType.Stationary*/
                };
                #endregion
                #region fishTrapCreekSticks - weir, sticks
                listOfEntityTypes.Add(new EntityType("structure:fishTrapCreekSticks")
            {
                Name = "Fish weir (sticks)",
                SummaryDescription = "Simple, fence-like structure that can catch a large number of migrating fish.",
                Description = "This trap is designed to catch schools of fish as they move through a narrow stream. \nWe will inspect the weir periodically and bring any fish we find back to camp.",
                ThumbnailSmall = "HUD_thumbnail_fishWeir",
                StructureType = structureType,
                CategoryKey = "production",
                ContainerType = fishTrapContainerType,
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "fishTrapWeirStraight",
                                Offset = new Vector2(0f, -40f)
                            }                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapWeirStraight_g"
                        }
                    }
                },
                ToolType = toolType,
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f,
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] //not for collision
                        {
                            new CollideShape2D(new Vector2(0, 0), 24)
                            {
                                Offset= new Vector2(6, -48)
                            },
                            new CollideShape2D(new Vector2(0, 0), 20)
                            {
                                Offset= new Vector2(-6, -25)
                            },
                            new CollideShape2D(new Vector2(0, 0), 20)
                            {
                                Offset= new Vector2(-6, -65)
                            }

                        }
                    }
                },                
                CustomFields = new SerializableDictionary<string, PropertyResult>()
                {
                    { "isFishTrap", new PropertyResult() { BoolResult = true }},
                    { "fishType", new PropertyResult() { StringResult = "item:carbonTail" }},
                    { "fishTypeBulk", new PropertyResult() { NumberResult = 0.07f }}, //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!       
                    { "spawnChance", new PropertyResult() { NumberResult = 0.04f }},                   
                    { "maxNoOfFishToSpawnAtATime", new PropertyResult() { NumberResult = 5 }}
                },
                /*
                 *     #region set custom property fishType to: "item:carbonTail"
                            new SetPropertyAction("3saf32523a-e464-4faf-99sda02-12ed532573fd4a0e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishType",
                                    Value = new ValueNode(){ String = "item:carbonTail" }
                                
                            },
                            #endregion

                            #region  set custom property fishTypeBulk to: 0.07f
                            new SetPropertyAction("153c5926-a126-4408-9377-f233fc40a9bb")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishTypeBulk",
                                    Value = new ValueNode(){ Decimal = 0.07f }  //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!
                                
                            },
                            #endregion

                            #region  set custom property spawnChance to: xf
                            new SetPropertyAction("e0d177d6-695f-4093-a735-e18a6c3e610e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "spawnChance",
                                    Value = new ValueNode(){ Decimal = 0.04f } 
                                
                            },
                            #endregion

                            #region  set custom property trapType to: "CreekSticks"
                            new SetPropertyAction("aad3f761-3c58-4267-b219-2657f5519aa8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "trapType",
                                    Value = new ValueNode(){ String = "CreekSticks" }
                                
                            },
                            #endregion
                
                            #region set custom property maxNoOfFishToSpawnAtATime to: xxf
                            new SetPropertyAction("74bsada3525jkytg0-1458-4160-a7a9-84c65c4d2dc8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "maxNoOfFishToSpawnAtATime",
                                    Value = new ValueNode(){ Decimal = 5 } 
                                
                            },
                            #endregion
                 */
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction",
                    SalvageProcess = "salvageFishTrapCreekSticks",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 5 } } ,
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from
                }
            });
                #endregion

            #region fishTrapCreekNet - weir, netting
            listOfEntityTypes.Add(new EntityType("structure:fishTrapCreekNet")
            {
                Name = "Fish weir (netting)",
                SummaryDescription = "Two-way weir that can catch fish migrating in both directions",
                Description = "This trap has an ingenious design which catches schools of fish travelling both up and down the stream. \nWe will inspect the weir periodically and bring any fish we find back to camp.",
                ThumbnailSmall = "HUD_thumbnail_fishWeirNet", 
                StructureType = structureType,
                CategoryKey = "production",
                ContainerType = fishTrapContainerType,
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "fishTrapWeirNet",
                                Offset = new Vector2(0f, -40f)
                            }                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapWeirStraight_g" //same ground sprite from the other weir
                        }
                    }
                },
                ToolType = toolType,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f,
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] //not for collision
                        {
                            new CollideShape2D(new Vector2(0, 0), 31)
                            {
                                Offset= new Vector2(-6, -48)
                            },
                            new CollideShape2D(new Vector2(0, 0), 23)
                            {
                                Offset= new Vector2(-6, -25)
                            },
                            new CollideShape2D(new Vector2(0, 0), 25)
                            {
                                Offset= new Vector2(-6, -65)
                            }

                        }
                    }
                },
                CustomFields = new SerializableDictionary<string, PropertyResult>()
                {
                    { "isFishTrap", new PropertyResult() { BoolResult = true }},
                    { "fishType", new PropertyResult() { StringResult = "item:carbonTail" }},
                    { "fishTypeBulk", new PropertyResult() { NumberResult = 0.07f }}, //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!       
                    { "spawnChance", new PropertyResult() { NumberResult = 0.04f }},                 
                    { "maxNoOfFishToSpawnAtATime", new PropertyResult() { NumberResult = 7 }}
                },
                /*
                    #region set custom property fishType to: "item:carbonTail"
                            new SetPropertyAction("3saf3dtyjt756rejueije2-12ed532573fd4a0e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishType",
                                    Value = new ValueNode(){ String = "item:carbonTail" }
                                
                            },
                            #endregion

                            #region  set custom property fishTypeBulk to: 0.07f
                            new SetPropertyAction("153cetue575eu7eyjdteyua9bb")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishTypeBulk",
                                    Value = new ValueNode(){ Decimal = 0.07f }  //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!
                                
                            },
                            #endregion

                            #region  set custom property spawnChance to: xf
                            new SetPropertyAction("e0d1dfhrtgfshjry6swudtyudrtgy-e18a6c3e610e")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "spawnChance",
                                Value = new ValueNode(){ Decimal = 0.04f }                                 
                            },
                            #endregion

                            #region  set custom property trapType to: "CreekNet" //
                            new SetPropertyAction("aadsrty56rtyrshsrtfyhsrtyhaa8")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "trapType",
                                Value = new ValueNode(){ String = "CreekNet" }                                
                            },
                            #endregion
                            #region set custom property maxNoOfFishToSpawnAtATime to: xxf
                            new SetPropertyAction("74bsada35adfgdfahsr56thrsthsrtfjhrzy-84c65c4d2dc8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "maxNoOfFishToSpawnAtATime",
                                    Value = new ValueNode(){ Decimal = 7 } 
                                
                            },
                            #endregion*/

                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageFishTrapCreekNet",
                    PartKeys = new SerializableDictionary<string, int>() {{ "item:fishingNet", 2 }, { "item:sticks", 3 } },
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from
                }
            });
            #endregion

            #region fishTrapCoast -Fyke
            listOfEntityTypes.Add(new EntityType("structure:fishTrapCoast")
            {
                Name = "Fish trap - Fyke", 
                SummaryDescription = "Fish trap designed for catching the 'streak fin' in its coastal habitat",
                Description = "The trap consists of a cylindrical net with wings which guide the fish toward the entrance. \nWe will check the trap periodically to collect any captures.",
                ThumbnailSmall = "HUD_thumbnail_fishTrapFyke",//
                StructureType = structureType,
                ContainerType = fishTrapContainerType,
                CategoryKey = "production",
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapFyke_g"
                        }
                    }
                },
                ToolType = toolType,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 15f,
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] //not for collision
                        {
                        new CollideShape2D(new Vector2(0, 0), 13) 
                        {
                            Offset= new Vector2(0, 0)
                        },
                        new CollideShape2D(new Vector2(0, 0), 19)
                        {
                            Offset= new Vector2(-14, 31)
                        },
                        new CollideShape2D(new Vector2(0, 0), 13)
                        {
                            Offset= new Vector2(14, 15)
                        }

                        }
                    }
                }, 
                CustomFields = new SerializableDictionary<string, PropertyResult>()
                {
                    { "isFishTrap", new PropertyResult() { BoolResult = true }},
                    { "fishType", new PropertyResult() { StringResult = "item:streakFin" }},
                    { "fishTypeBulk", new PropertyResult() { NumberResult = 0.07f }}, //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!       
                    { "spawnChance", new PropertyResult() { NumberResult = 0.05f }},                   
                    { "maxNoOfFishToSpawnAtATime", new PropertyResult() { NumberResult = 5 }}
                },
                /* #region  set custom property fishType to: "item:streakFin"
                            new SetPropertyAction("6597ee35-6007-4f81-ad55-26a541a13b63")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "fishType",
                                Value = new ValueNode(){ String = "item:streakFin" }                                
                            },
                            #endregion

                            #region  set custom property fishTypeBulk to: 0.07f
                            new SetPropertyAction("4b578af7-3a36-4049-aa3b-2f5ff52742ec")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "fishTypeBulk",
                                Value = new ValueNode(){ Decimal = 0.07f } //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!                                
                            },
                            #endregion

                            #region  set custom property spawnChance to: xf
                            new SetPropertyAction("5d2840ed-d806-4741-87c7-6c14396a067a")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "spawnChance",
                                Value = new ValueNode(){ Decimal = 0.05f }                                 
                            },
                            #endregion

                            #region  set custom property trapType to: "Coast"
                            new SetPropertyAction("64516802-f3e9-4c17-9d62-77839b607aac")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "trapType",
                                Value = new ValueNode(){ String = "Coast" }                                
                            },
                            #endregion

                            #region set custom property maxNoOfFishToSpawnAtATime to: xxf
                            new SetPropertyAction("bccb2ac0-155e-4454-8e7d-de10a05212f2")
                            {                               
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyKey = "maxNoOfFishToSpawnAtATime",
                                Value = new ValueNode(){ Decimal = 5  }                                
                            },
                            #endregion*/
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction", 
                    SalvageProcess = "salvageFishTrapCoast",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:fishTrapHoopNet", 1 }, { "item:fishingNet", 1 } },
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from
                }
            });
                #endregion
                #region fishTrapShore //basket trap
            listOfEntityTypes.Add(new EntityType("structure:fishTrapShoreBasket")
            {
                Name = "Fish trap - Basket",
                SummaryDescription = "Simple fish trap designed for catching the 'carbon tail'",
                Description = "A wicker basket which allows fish to enter through a funnel and hampers their escape. \nWe will check the trap periodically to collect any captures.",
                ThumbnailSmall = "HUD_thumbnail_fishTrapCylinder",
                StructureType = structureType,
                CategoryKey = "production",
                ContainerType = fishTrapContainerType,
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapCylinderSmall_g"
                        }
                    }
                },
                ToolType = toolType,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = fishTrapGeometryLayoutType
                },
                CustomFields = new SerializableDictionary<string, PropertyResult>()
                {
                    { "isFishTrap", new PropertyResult() { BoolResult = true }},
                    { "fishType", new PropertyResult() { StringResult = "item:carbonTail" }},
                    { "fishTypeBulk", new PropertyResult() { NumberResult = 0.07f }}, //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!       
                    { "spawnChance", new PropertyResult() { NumberResult = 0.05f }},                  
                    { "maxNoOfFishToSpawnAtATime", new PropertyResult() { NumberResult = 3 }}
                },
                /*  #region  set custom property fishType to: item:carbonTail
                            new SetPropertyAction("520f8d58-d2d3-4559-84ed-610399c3f4a8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishType",
                                    Value = new ValueNode(){ String = "item:carbonTail" }
                                
                            },
                            #endregion

                            #region  set custom property fishTypeBulk to: 0.07f
                            new SetPropertyAction("8632e2f5-49bd-4642-8e22-5ba83810d552")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishTypeBulk",
                                    Value = new ValueNode(){ Decimal = 0.07f } //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!
                                
                            },
                            #endregion

                            #region set custom property spawnChance to: xf
                            new SetPropertyAction("cdb80fd8-8e6c-4c55-81d4-0e66f1521642")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "spawnChance",
                                    Value = new ValueNode(){ Decimal = 0.05f } 
                                
                            },
                            #endregion

                            #region set custom property trapType to: "ShoreBasket"
                            new SetPropertyAction("5071dbcc-38e1-4c8e-860b-69a23038e8e7")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "trapType",
                                    Value = new ValueNode(){ String = "ShoreBasket" }
                                
                            },
                            #endregion

                            #region set custom property maxNoOfFishToSpawnAtATime to: xf
                            new SetPropertyAction("74b46320-1458-4160-a7a9-8sadaferhyiopgdv2362dc8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "maxNoOfFishToSpawnAtATime",
                                    Value = new ValueNode(){ Decimal = 3f  } 
                                
                            },
                            #endregion
                 */
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction", 
                    SalvageProcess = "salvageFishTrapShoreBasket",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:fishTrapBasket", 1 } },
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from
                }
            });
                #endregion
            #region fishTrapShoreHoopNet //hoop net
            listOfEntityTypes.Add(new EntityType("structure:fishTrapShoreHoopNet")
            {
                Name = "Fish trap - Hoop net",
                SummaryDescription = "Good quality fish trap specially designed for catching the 'carbon tail'",
                Description = "A cylindrical trap that allows fish to enter from both ends but restricts their escape. Made from cotton netting and wooden hoops. \nWe will check the trap periodically to collect any captures.",
                ThumbnailSmall = "HUD_thumbnail_fishTrapHoopNet",
                StructureType = structureType,
                CategoryKey = "production",
                ContainerType = fishTrapContainerType,
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapHoopNet_g"
                        }
                    }
                },
                ToolType = toolType,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = fishTrapGeometryLayoutType
                },

                /*
                 * fishType -> what kind of fish to spawn
                 * fishTypeBulk -> the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!  
                 * spawnChance -> the chance a fish will spawn each iteration 0..1 (multiply by 100 to get the percentage)                  
                 * maxNoOfFishToSpawnAtATime -> the maximum no of fish that will be spawned each time. The number is rounded down after being multiplied by a random 0-1 number
                 * 
                 */
                CustomFields = new SerializableDictionary<string, PropertyResult>()
                {
                    { "isFishTrap", new PropertyResult() { BoolResult = true }},
                    { "fishType", new PropertyResult() { StringResult = "item:carbonTail" }},
                    { "fishTypeBulk", new PropertyResult() { NumberResult = 0.07f }}, //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!       
                    { "spawnChance", new PropertyResult() { NumberResult = 0.05f }},         
                    { "maxNoOfFishToSpawnAtATime", new PropertyResult() { NumberResult = 4 }}
                },
                /*      #region  set custom property fishType to: item:carbonTail
                            new SetPropertyAction("52syhjsghfjstyjsrtjghfsjrty399c3f4a8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishType",
                                    Value = new ValueNode(){ String = "item:carbonTail" }
                                
                            },
                            #endregion

                            #region  set custom property fishTypeBulk to: 0.07f
                            new SetPropertyAction("8632e2f5dgjdgtjydtjdtyjdgj10d552")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "fishTypeBulk",
                                    Value = new ValueNode(){ Decimal = 0.07f } //the bulk defined for the fishType in the itemLoader needs to correspond to the number defined here!
                                
                            },
                            #endregion

                            #region set custom property spawnChance to: xf
                            new SetPropertyAction("cdb80fddghjdgkdthkjsgthksghk21642")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "spawnChance",
                                    Value = new ValueNode(){ Decimal = 0.05f } 
                                
                            },
                            #endregion

                            #region set custom property trapType to: "ShoreHoopNet" 
                            new SetPropertyAction("5071dgkdgjkdjdgjkdkjdj038e8e7")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "trapType",
                                    Value = new ValueNode(){ String = "ShoreHoopNet" }
                                
                            },
                            #endregion

                            #region set custom property maxNoOfFishToSpawnAtATime to: xf
                            new SetPropertyAction("74b46fsghjgfjjjjjjjyxgfhjsgfhsrftyhdc8")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyKey = "maxNoOfFishToSpawnAtATime",
                                    Value = new ValueNode(){ Decimal = 4f  } 
                                
                            },
                            #endregion*/
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageFishTrapShoreHoopNet",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:fishTrapHoopNet", 1 } },
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from
                }
            });
            #endregion

            #endregion

//////////// Farming
            #region smallPlot
            // http://www.almanac.com/plant/beans
            listOfEntityTypes.Add(new EntityType("structure:smallPlot")
            {
                Name = "Small plot",
                SummaryDescription = "A small plot for farming.",
                Description = "After sowing/planting, this area can provide crops depending on the seeds used. The crops will need to be weeded periodically, so we must ensure we have tools and workforce available to tend to the plot during the growing cycle. Once the crops are ripe, we have to harvest them in time to avoid losing them.",
                ThumbnailSmall = "HUD_thumbnail_plotYellowShrubs",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    IsPseudoTool = true
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "plotPlowed_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },                                                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier))                            
                         },

                        #region crops
                            #region glassy creeper pods
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubYoung_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubGrown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubYoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                            #endregion
                            #region crystal berries
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrub2Young_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrub2Grown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrub2YoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrub2Weeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotShrubGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                            #endregion
                          #region cotton
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonYoung_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonGrown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotPlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonYoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonGrownWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotCottonGrownDecayed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                            #endregion
                        #endregion
                     }          
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {//START HERE!!!
                        Pad = 50f,
                        //PadShape = CollidePrim.Rectangle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(-72f, -48f), new Vector2(72f, 48f))
                        }
                    }
                },
                //SpecialActions 
                SpecialActionLocks = new[] {                  
                    new Pair<string, bool>("useOrganicFertilizer", false), //is default
                    new Pair<string, bool>("useGuanoFertilizer", true),
                    new Pair<string, bool>("stopUsingGuanoFertilizer", false),
                    new Pair<string, bool>("stopUsingOrganicFertilizer", true),
                    new Pair<string, bool>("growGlassyCreeperPodsInSmallPlot", true),
                    new Pair<string, bool>("growCrystalBerriesInSmallPlot", true),
                    new Pair<string, bool>("growCottonInSmallPlot", true),
                    new Pair<string, bool>("stopGrowingCrystalBerriesInSmallPlot", false),
                    new Pair<string, bool>("stopGrowingGlassyCreeperPodsInSmallPlot", false),
                    new Pair<string, bool>("stopGrowingCottonInSmallPlot", false)},  
                                    
               NonLivingType = new NonLivingType() { PartsAreWeatherProof = true, DegradeType = "dirt", SalvageProcess = "salvageSmallPlot" }, // mp DegradeType was "equipment". but if they do maintenance on a field it shouldn't degrade. 
                IsSelectable = true
            });
            #endregion
            
            #region largePlot
            // http://www.almanac.com/plant/beans
            listOfEntityTypes.Add(new EntityType("structure:largePlot")
            {
                Name = "Large plot",
                SummaryDescription = "A large plot for farming.",
                Description = "After sowing/planting, this area can provide crops depending on the seeds used. The crops will need to be weeded periodically, so we must ensure we have tools and workforce available to tend to the plot during the growing cycle. Once the crops are ripe, we have to harvest them in time to avoid losing them.",
                ThumbnailSmall = "HUD_thumbnail_plotYellowShrubs",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    IsPseudoTool = true
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "plotLargePlowed_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowed_g"
                                },                                                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },

                        #region crops
                            #region glassy creeper pods
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargeShrubYoung_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargeShrubGrown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargeShrubYoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargeShrubWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                            #endregion
                            #region crystal berries
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Young_g"
                                    AssetName = "plotLargeShrub2Young_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Grown_g"
                                    AssetName = "plotLargeShrub2Grown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Weeds_g"
                                    AssetName = "plotLargePlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Weeds_g"
                                    AssetName = "plotLargeShrub2YoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Weeds_g"
                                    AssetName = "plotLargeShrub2YoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                            #endregion
                           #region cotton
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargePlowed_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    AssetName = "plotLargeCottonYoung_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {                                 
                                    AssetName = "plotLargeCottonGrown_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {
                                    //AssetName = "plotLargeShrub2Weeds_g"
                                    AssetName = "plotLargePlowedWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {                                  
                                    AssetName = "plotLargeCottonYoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                                {                                  
                                    AssetName = "plotLargeCottonYoungWeeds_g"
                                },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour3)                            
                         },
                            #endregion
                        #endregion                        
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {//START HERE!!!
                        Pad = 78f,
                        //PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(-120f, -72f), new Vector2(120f, 72f))
                        }
                    }
                },
                SpecialActionLocks = new[] { 
                    new Pair<string, bool>("useLargeOrganicFertilizer", false), //is default
                    new Pair<string, bool>("useLargeGuanoFertilizer", true),
                    new Pair<string, bool>("stopUsingLargeGuanoFertilizer", false),
                    new Pair<string, bool>("stopUsingLargeOrganicFertilizer", true),
                    new Pair<string, bool>("growGlassyCreeperPodsInLargePlot", true),
                    new Pair<string, bool>("growCrystalBerriesInLargePlot", true),
                    new Pair<string, bool>("growCottonInLargePlot", true),
                    new Pair<string, bool>("stopGrowingCrystalBerriesInLargePlot", false),
                    new Pair<string, bool>("stopGrowingGlassyCreeperPodsInLargePlot", false),
                    new Pair<string, bool>("stopGrowingCottonInLargePlot", false)},                    

                NonLivingType = new NonLivingType() { PartsAreWeatherProof = true, DegradeType = "dirt", SalvageProcess = "salvageLargePlot" },
                IsSelectable = true
            });
            #endregion

            #region improvisedGreenhouse
            // http://www.almanac.com/plant/beans
            listOfEntityTypes.Add(new EntityType("structure:improvisedGreenhouse")
            {
                Name = "Greenhouse (primitive)",
                SummaryDescription = "Greenhouse made with primitive materials",//placeholder txt
                Description = "The covering is made from sheets of turnip entrails which have enough translucency to trap the sun's heat. This makes us able to grow the finger fruit. We can also get a quicker harvest of ordinary crops which benefit from the higher temperature.",
                ThumbnailSmall = "HUD_thumbnail_greenhouseImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    IsPseudoTool = true
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "greenhouseDomeImprovised",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "greenhouseDomeImprovisedNoCrops_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised"}},                                                              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){AssetName = "greenhouseDomeImprovisedNoCrops_g"},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },

                        #region crops
                            #region glassy creeper pods
                         new ClientStateInfo()
                         {                   
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised"}},                   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){AssetName = "greenhouseDomeImprovisedNoCrops_g"},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour1)                            
                         },
                            #endregion
                            #region crystal berries
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedNoCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         { 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HasCrops, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ripe, (int)StateModifier.Overgrown, (int)StateModifier.Flavour2)                            
                         },
                            #endregion
                        #endregion                        
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 10f,
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0,-2), 25),
                        },
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-10,-2), 20),
                            new CollideShape2D(new Vector2(10,-2), 20),
                        }
                    }
                },
                SpecialActionLocks = new[] {                  
                    new Pair<string, bool>("useOrganicFertilizer", false), //is default
                    new Pair<string, bool>("useGuanoFertilizer", true),
                    new Pair<string, bool>("stopUsingGuanoFertilizer", false),
                    new Pair<string, bool>("stopUsingOrganicFertilizer", true),
                    new Pair<string, bool>("growFingerFruitInGreenhouse", true),
                    new Pair<string, bool>("growCrystalBerriesInGreenhouse", true),
                    new Pair<string, bool>("growGlassyCreeperPodsInGreenhouse", true),
                    new Pair<string, bool>("stopGrowingCrystalBerriesInGreenhouse", false),
                    new Pair<string, bool>("stopGrowingGlassyCreeperPodsInGreenhouse", false),
                    new Pair<string, bool>("stopGrowingFingerFruitInGreenhouse", false)},  

                    /*
                SpecialActions = new[] { new Pair<string, bool>("plantFingerFruitsInGreenhouse", true), 
                    new Pair<string, bool>("plantGlassyCreeperPodsInGreenhouse", true), new Pair<string, bool>("plantCrystalBerriesInGreenhouse", true), 
                    new Pair<string, bool>("harvestGreenhouseCrops", true), 
                    new Pair<string, bool>("useOrganicFertilizer", true),
                    new Pair<string, bool>("useGuanoFertilizer", true), },*/

                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "sturdyConstruction", 
                    SalvageProcess = "salvageImprovisedGreenhouse",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedGreenHouseCover", 2 }, { "item:shadeleafCanes", 3 } } ,
                    Repair = "buildingRepair",
                },
                IsSelectable = true
            });
            #endregion
///////////////////


            #region Greenhouse
            // 
            listOfEntityTypes.Add(new EntityType("structure:greenhouse")
            {
                Name = "Greenhouse",
                SummaryDescription = "Greenhouse made with advanced materials",//
                Description = "The greenhouse uses sheets of diamond glass left by the Ancestors. Makes us able to grow the finger fruit. Also speeds up the growth cycle of other crops which benefit from the higher temperature and protection.",
                ThumbnailSmall = "HUD_thumbnail_greenhouse",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = false, //!!!!!! mp prebuilt structure
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    IsPseudoTool = true
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "greenhouse2",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "greenhouse2_g"
                        }
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouse2"}},                                                              
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo()
                         {                                    
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){AssetName = "greenhouse2_g"},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)                            
                         },

                        #region crops sprites//not used
                         
                            #region glassy creeper pods
                         /*
                         new SpriteConditionInfo()
                         {                   
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised"}},                   
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){AssetName = "greenhouseDomeImprovisedNoCrops_g"},
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour1)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.HasCrops, (int)SpriteModifier.Flavour1)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ripe, (int)SpriteModifier.Flavour1)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour1)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.HasCrops, (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour1)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ripe, (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour1)                            
                         },
                          * */
                            #endregion
                            #region crystal berries
                           /*
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedNoCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour2)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.HasCrops, (int)SpriteModifier.Flavour2)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ripe, (int)SpriteModifier.Flavour2)                            
                         },
                         new SpriteConditionInfo()
                         { 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour2)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.HasCrops, (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour2)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "greenhouseDomeImprovised" }},  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "greenhouseDomeImprovisedCrops_g" },
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ripe, (int)SpriteModifier.Overgrown, (int)SpriteModifier.Flavour2)                            
                         },
                          * */
                            #endregion
                           
                        #endregion                        
                     }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //TODO
                    {
                        Pad = 10f,
                        PadShape = CollidePrim.Circle,
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0,-2), 25),
                        },
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-10,-2), 20),
                            new CollideShape2D(new Vector2(10,-2), 20),
                        }
                    }
                },

                SpecialActionLocks = new[] {                  
                    new Pair<string, bool>("useOrganicFertilizer", false), //is default
                    new Pair<string, bool>("useGuanoFertilizer", true),
                    new Pair<string, bool>("stopUsingGuanoFertilizer", false),
                    new Pair<string, bool>("stopUsingOrganicFertilizer", true),
                    new Pair<string, bool>("growFingerFruitInGreenhouse", true),
                    new Pair<string, bool>("growCrystalBerriesInGreenhouse", true),
                    new Pair<string, bool>("growGlassyCreeperPodsInGreenhouse", true),
                    new Pair<string, bool>("stopGrowingCrystalBerriesInGreenhouse", false),
                    new Pair<string, bool>("stopGrowingGlassyCreeperPodsInGreenhouse", false),
                    new Pair<string, bool>("stopGrowingFingerFruitInGreenhouse", false)},  

                    /*
                SpecialActions = new[] { new Pair<string, bool>("plantFingerFruitsInGreenhouse", true), 
                    new Pair<string, bool>("plantGlassyCreeperPodsInGreenhouse", true), new Pair<string, bool>("plantCrystalBerriesInGreenhouse", true), 
                    new Pair<string, bool>("harvestGreenhouseCrops", true), new Pair<string, bool>("useOrganicFertilizer", true), 
                    new Pair<string, bool>("useGuanoFertilizer", true), },*/

                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "advancedConstruction",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:diamondGlass", 5 }, { "item:shadeleafCanes", 3 } }  ,
                    Repair = "buildingRepair",
                },
                IsSelectable = true
            });
            #endregion


            #region scarecrow 
            listOfEntityTypes.Add(new EntityType("structure:scarecrow")
            {
                Name = "Pest repellent (twinkler scent)",
                SummaryDescription = "Small rig that uses twinkler scent to ward off binal rats",
                Description = "Can be placed in areas that we want free from binal rats such as foodstores. The repellent effect wears off after some time at which point the repellent device has to be rebuilt using fresh twinkler pheromones.",
                ThumbnailSmall = "HUD_thumbnail_pestRepellentTwinkler",
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                CategoryKey = "defense",
                StructureType = new StructureType()
                {

                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["defense"]
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "pestRepellentStructure", 
                            }                    
                    
                        },
                        
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "pestRepellentStructure" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                    
                     }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        GridAlignedPlacement = false,
                        Pad = 0f,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 7)
                            {
                                Offset= new Vector2(0,0)
                            }
                        }
                    }
                },
                ThreatType = new ThreatType()
                {
                    StrengthRating = StrengthRating.LikeHumans //based on binalrats, but binal rats are super brave, needs rebalance, levels are VeryWeak = 0, WeakerThanHumans = 1, LikeHumans = 2, StrongerThanHumans = 3, VeryStrong = 4 
                },
                NonLivingType = new NonLivingType() 
                { 
                    PartsAreWeatherProof = true, 
                    DegradeType = "ricketyConstruction",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:twinklerPlating", 1 }, { "item:twinklerPheromone", 1 } },
                    Repair = "buildingRepair",
                }
            });
            #endregion

            #region kiln
            listOfEntityTypes.Add(new EntityType("structure:kiln")
            {
                Name = "Kiln", //http://en.wikipedia.org/wiki/Kiln
                SummaryDescription = "A large oven for making charcoal, bricks or other products", 
                Description = "This simple kiln is made from clay. Fueled with ordinary firewood it can carbonize other batches of firewood, making charcoal. The kiln can also be used for producing mudbricks (at a faster rate than sun drying) and for firing pottery.",
                ThumbnailSmall = "HUD_thumbnail_kilnImprovised",
                CategoryKey = "production",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "kiln" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kilnSmoke"                 
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "kilnImprovised",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "kilnImprovised_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kilnImprovised"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "kilnImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kilnImprovised"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "kilnImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool), 
                               // LightingTypes = new[]{ new LightingType() { SpriteName = "common_22_circularsmall", Offset = new Point(-17, -18) } },                            
                                ParticleEmitters = new[] 
                                    { 
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "smallestSmoke",
                                            Offset = new Vector2(0,-30)
                                        },
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "tinyFire",
                                            Offset = new Vector2(10,10)
                                        } 
                                    }
                         }
                    }
                },
   
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, //mp animals cannot access.

                    ProductionOutputStorageType = new ItemStorageType(2.5f) // has room for 3 mudbricks now...   1.5f) //todo
                    {
                        FullStatePercentage = 0.1f,//mp todo?
                        HalfFullStatePercentage = 0.05f
                    },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelKiln",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 3f ////MP feb 2016: reduced by 33%...was 5f      mp I set it low because the long crafting times (in processloader) mean that a lot of fuel is needed for kiln processes compared to campfire processes.
                            //if an agent is unable to carry all that fuel bulk in one go, a behavior bug will occur (sep 2015)
                        }
                    }   
                },

 
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-7), 24)
                        }
                    }
                },                
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageKiln",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 1 } },
                    Repair = "buildingRepair",
                }
            });
            #endregion

            #region Oven (improvised) - kilnImprovisedSmall. made from stone
            listOfEntityTypes.Add(new EntityType("structure:kilnImprovisedSmall")
            {
                Name = "Oven (improvised)", //http://en.wikipedia.org/wiki/Kiln
                SummaryDescription = "A small oven for firing pottery or baking",
                Description = "This small kiln is made from stones. It is not very efficient but useful in a survival situation.",
                ThumbnailSmall = "HUD_thumbnail_kilnImprovisedSmall",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "kiln" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kilnSmoke"
                },
                TierOrArea = new TierOrArea() { Tier = "survival" },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "kilnImprovisedSmall",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "kilnImprovisedSmall_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kilnImprovisedSmall"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "kilnImprovisedSmall_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "kilnImprovisedSmall"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "kilnImprovisedSmall_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]
                                    { 
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "smallestSmoke",
                                            Offset = new Vector2(-5,-2)
                                        },
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "tinyFire",
                                            Offset = new Vector2(-5,3)
                                        },
                                    }
                         }
                    }
                },

                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, //mp animals cannot access.

                    ProductionOutputStorageType = new ItemStorageType(1f) //todo
                    {
                        FullStatePercentage = 0.1f,//mp todo?
                        HalfFullStatePercentage = 0.05f
                    },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelKiln",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f, //todo
                            FuelTypeTag = "fuelForCampfire",
                            BurnRatePerDay = 2f ////MP feb 2016: reduced by 33%...was 3f     lower than the big kiln.//mp I set it low because the long crafting times (in processloader) mean that a lot of fuel is needed for kiln processes compared to campfire processes.
                            //if an agent is unable to carry all that fuel bulk in one go, a behavior bug will occur (sep 2015)
                        }
                    }
                },


                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-7), 17)
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageKilnImprovisedSmall",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 3 } } ,
                    Repair = "buildingRepair",
                }

            });
            #endregion


            #region gold furnace
            listOfEntityTypes.Add(new EntityType("structure:goldFurnace")
            {
                Name = "Gold furnace", // 
                SummaryDescription = "Can melt metals such as gold which have melting points up to 1100 C / 2012 F",
                Description = "A flat structure with a chimney; the technical name is 'reverberatory furnace'. Made from tiles of clay which can withstand the intense heat required to melt gold. The metal is placed in a hearth which lays next to the firebox. Firewood is used as fuel, and a natural draft carries the flames from the firebox over to the metal. The melted metal comes out from a tap in the bottom.",
                ThumbnailSmall = "HUD_thumbnail_goldFurnace", //
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {
                  //  ToolTag = new[] { "" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "kilnSmoke",
                    
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "goldFurnace", //
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "goldFurnace_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "goldFurnace"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "goldFurnace_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "goldFurnace"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "goldFurnace_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]
                                    { 
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "smallestSmoke",
                                            Offset = new Vector2(16,-30)
                                        },
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "tinyFire",
                                            Offset = new Vector2(-10,10)
                                        },
                                    }
                         }
                    }
                },

                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, //mp animals cannot access.

                    ProductionOutputStorageType = new ItemStorageType(1.5f) //todo
                    {
                        FullStatePercentage = 0.1f,//mp todo?
                        HalfFullStatePercentage = 0.05f
                    },
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelKiln",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeTag = "fuelForCampfire", //requires firewood, cannot use charcoal.
                            BurnRatePerDay = 3f ////MP feb 2016: reduced by 33%...was 5f        mp I set it low because the long crafting times (in processloader) mean that a lot of fuel is needed for kiln processes compared to campfire processes.
                            //if an agent is unable to carry all that fuel bulk in one go, a behavior bug will occur (sep 2015)
                        }
                    }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,7), 12),
                            new CollideShape2D(new Vector2(16,-9), 14)
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageGoldFurnace",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:firebricks", 3 } } ,
                    Repair = "buildingRepair",
                }

            });
            #endregion

            #region improvisedSmithy
            listOfEntityTypes.Add(new EntityType("structure:improvisedSmithy")
            {
                Name = "Smithy (improvised)", //http://en.wikipedia.org/wiki/Bloomery it's a combo of a furnace and a forge.. http://www.engr.psu.edu/mtah/essays/forge_furnace.htm
                SummaryDescription = "Simple furnace for smelting / heating iron and a rock anvil. Fuel:charcoal", //   for shaping the metal
                Description = "The primitive furnace is built from clay. To reach a sufficient temperature, it requires charcoal as fuel and an air supply tool such as a bellows. After smelting the ore in the furnace, the metal is shaped on the anvil with a hammer.",
                ThumbnailSmall = "HUD_thumbnail_forgeImprovised",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "furnace" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "forgeSmoke"
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "forgeImprovised",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "forgeImprovised_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "forgeImprovised"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "forgeImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "forgeImprovised"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "forgeImprovised_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]
                                    { 
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "smallestSmoke",
                                            Offset = new Vector2(-9,-26)
                                        },
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "tinyFire",
                                            Offset = new Vector2(-15,0)
                                        }
                                    }
                         }
                    }
                },



                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },

                    ItemStorageType = new ItemStorageType("isolated", 1f), // NEW: has  storage for tools 
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "forgeStorage",
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelSmithy",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeKeyName = "item:charcoal",
                            BurnRatePerDay = 2f ////MP feb 2016: reduced by 33%...was 3f
                        }
                    }  
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-8), 14),
                            new CollideShape2D(new Vector2(17,2), 14),
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageImprovisedSmithy",
                    Repair = "buildingRepair",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 3 }, { "item:stones", 1 } }                
                }

            });
            #endregion

            #region simpleSmithy
            listOfEntityTypes.Add(new EntityType("structure:simpleSmithy")//
            {
                Name = "Smithy (simple)", //http://en.wikipedia.org/wiki/Bloomery it's a combo of a furnace and a forge.. http://www.engr.psu.edu/mtah/essays/forge_furnace.htm
                SummaryDescription = "Simple furnace for smelting / heating iron and an iron anvil. Fuel: charcoal", // for shaping the metal
                Description = "A step up from the improvised smithy with its rock anvil, this smithy equipped with an iron anvil can make more sophisticated iron objects. \n \nThe primitive bloomery furnace is built from clay. It requires charcoal as fuel and an air supply tool such as a bellows. After smelting iron ore in the furnace, a solid iron bloom is worked on the anvil with a hammer, removing slag until low-carbon 'wrought iron' is produced. This can be further worked into iron tools.",//
                ThumbnailSmall = "HUD_thumbnail_forgeSimple",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "furnace" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary,
                    PrepareProcess = "forgeSmoke"
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "forgeSimple",//
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "forgeSimple_g"//
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "forgeSimple"}},//
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "forgeSimple_g" },//
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "forgeSimple"}},//
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "forgeSimple_g" },//
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.PreparedTool),
                                ParticleEmitters = new[]
                                    { 
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "smallestSmoke",
                                            Offset = new Vector2(-11,-22)
                                        },
                                        new ParticleEmitterEffect()
                                        { 
                                            ParticleSystemKey = "tinyFire",
                                            Offset = new Vector2(-19,4)
                                        }
                                    }
                         }
                    }
                },
                ContainerType = new WorkshopContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" },

                    ItemStorageType = new ItemStorageType("isolated", 1f), // NEW: has  storage for tools 
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "forgeStorage",
                    RequiresReplenishType = new RequiresReplenishType()
                    {
                        ReplenishProcess = "refuelSmithy",
                        RequiresFuelType = new RequiresFuelType()
                        {
                            MaxFuel = 1f,
                            FuelTypeKeyName = "item:charcoal",
                            BurnRatePerDay = 2f ////MP feb 2016: reduced by 33%...was 3f
                        }
                    }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 8f, //NA changed from padRadiusShelterAndStorage to 8f
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageSimpleSmithy",
                    Repair = "buildingRepair",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 3 }, { "item:anvil", 1 }, { "item:barClamps", 1 } }               
                }

            });
            #endregion


            #region favorbread farm
            listOfEntityTypes.Add(new EntityType("structure:favorbreadFarm")//
            {
                Name = "Favorbread farm",
                SummaryDescription = "A pit for cultivating the favorbread vegetable (which must be ordered from the Production Panel)",
                Description = "By excavating a small garden directly underneath the dead sanctuary tree we can revive the favorbread if we provide it with the carbohydrates that the tree no longer supplies it with. We have found that the blackpulp is well suited as a substrate that will make the favorbread grow vigorously.\n Note: We have found that this method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms causes a dangerous, defensive response from them.",
                ThumbnailSmall = "HUD_thumbnail_favorbreadFarm",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "favorbreadFarm" }, //used for placing the output in the tool container
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "favorbreadFarm_g",
                            //mp needs to be situated underneath the sanctuary tree but  Offset dosnt work on ground sprite which is why i saved the png with a large empty area

                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "favorbreadFarm_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {                                                            
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "favorbreadFarm_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                               RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "favorbreadFarmFull_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }

                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // is safe from scavengers

                    ProductionOutputStorageType = new ItemStorageType(4f) //each piece is around 0.05f bulk
                    {
                        FullStatePercentage = 0.1f, // only one extra sprite exists.
                   //     HalfFullStatePercentage = 0.15f 
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        { //mp I make a hole of unblocked terrain in the middle edge where the output, favorbread, is supposed to appear once they empty it. note that the output is hidden in the container until it is collected.
                            new CollideShape2D(new Vector2(-17,-4), 11),//left 'prong'
                            new CollideShape2D(new Vector2(19,2), 11),//right 'prong'
                            new CollideShape2D(new Vector2(2,-17), 13),// center north of the 2 'prongs'
                        },
                        SelectionShapes = new CollideShape2D[] //no blocking, this is a shape for selecting. else it can be difficult to avoid selecting the sanctuary tree
                        {
                            new CollideShape2D(new Vector2(0, 0), 24)
                            {
                                Offset = new Vector2(0, 10) //south
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                { //
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageFavorbreadFarm",//
                    Repair = "diggingRepairCustomProcess", // 
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }
                }

            });

            #endregion

            #region clay pit
            listOfEntityTypes.Add(new EntityType("structure:clayPit")//
            {
                Name = "Clay pit",
                SummaryDescription = "A site where we can extract a large supply of clay (which must be ordered from the Production Panel)",
                Description = "The pit gives access to the rich deposits of clay beneath the surface which are otherwise hard to reach.",
                ThumbnailSmall = "HUD_thumbnail_clayPit",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {                   
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary                   
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "clayPit_g"
                        }
                    }                    
                },               
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {//
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageClayPit",
                    Repair = "diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }                    
                }

            });

            #endregion

            #region salt mine
            listOfEntityTypes.Add(new EntityType("structure:saltMine")//
            {
                Name = "Salt mine",
                SummaryDescription = "A site where we can extract a large supply of salt (which must be ordered from the Production Panel)",
                Description = "The mine gives access to the rich deposits of rock salt beneath the surface which are otherwise hard to reach.",
                ThumbnailSmall = "HUD_thumbnail_saltMine",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "saltMine_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 19),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                { //
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageSaltMine",
                    Repair = "diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }                 
                }

            });

            #endregion

            #region bogOrePit
            listOfEntityTypes.Add(new EntityType("structure:bogOrePit")//
            {
                Name = "Bog ore pit",
                SummaryDescription = "A site where we can extract a large supply of bog ore (which must be ordered from the Production Panel)",
                Description = "The pit gives access to the rich deposits of bog ore beneath the surface which are otherwise hard to reach.",
                ThumbnailSmall = "HUD_thumbnail_bogOrePit",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "bogOrePit_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                { //
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageBogOrePit",
                    Repair = "diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }                 
                }

            });

            #endregion

            #region Scandium mine   rareMetalOrePit1
          
            listOfEntityTypes.Add(new EntityType("structure:rareMetalOrePit1")//
            {
                Name = "Scandium mine",
                SummaryDescription = "A site where we can extract scandium ore (which must be ordered from the Production Panel)",
                Description = "The mine gives access to the rich deposits of scandium ore beneath the surface.",
                ThumbnailSmall = "HUD_thumbnail_saltMine",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                ToolType = new ToolType()
                {
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "saltMine_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {//
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageRareMetalorePit1",
                    Repair = "diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }
                }

            });
            #endregion
            #region Terbium mine   rareMetalOrePit1
           
            listOfEntityTypes.Add(new EntityType("structure:rareMetalOrePit2")//
            {
                Name = "Terbium mine",
                SummaryDescription = "A site where we can extract terbium ore (which must be ordered from the Production Panel)",
                Description = "The mine gives access to the rich deposits of terbium ore beneath the surface.",
                ThumbnailSmall = "HUD_thumbnail_saltMine",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                ToolType = new ToolType()
                {
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "saltMine_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {//
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvageRareMetalorePit2",
                    Repair = "diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }
                }

            });
            #endregion
            
     
            #region peatBank
            listOfEntityTypes.Add(new EntityType("structure:peatBank")//
            {
                Name = "Peat bank",
                SummaryDescription = "A site where peat can be cut (which must be ordered from the Production Panel)",
                Description = "The peat bank is a place in a bog or marsh where slabs of partially decomposed vegetation can be cut and used as fuel.",
                ThumbnailSmall = "HUD_thumbnail_peatBank",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {
                    Durability = 1,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "peatBank_g" //
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 4f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-9,-4), 16),//
                            new CollideShape2D(new Vector2(17,2), 14),//placeholder
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "adequateConstruction", // 
                    SalvageProcess = "salvagePeatBank",
                    Repair = "plowingRepairCustomProcess", //"diggingRepairCustomProcess", // use this because there is no proper production process
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 1 } }//todo
                }

            });

            #endregion



            //woodpile that works similar to drying shed:
            #region firewoodStack
            listOfEntityTypes.Add(new EntityType("structure:firewoodStack")
            {
                Name = "Woodpile",
                SummaryDescription = "A pile where wet firewood is stacked and dried so that it can be used as fuel",//
                Description = "",//todo
                ThumbnailSmall = "HUD_thumbnail_woodPile",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "survival" },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "woodpileTool" }, //used for placing the output in the tool container
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                      
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "woodpile_g"
                        }

                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "woodpile"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "woodpile_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "woodpileHalfFull"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "woodpile_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HalfFull)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "woodpile"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "woodpile_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }
                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // is safe from scavengers

                    ProductionOutputStorageType = new ItemStorageType(6f) // room enough for one output batch and one in production. currently produces 10*0.3 bulk
                    {
                        FullStatePercentage = 0.8f, // seen when it holds a batch of 10 in storage and has another batch being made  = 3f+3f
                        HalfFullStatePercentage = 0.15f //seen when processing a batch
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-7), 24)
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    //no parts. to compensate, I set degradeprofile to dirt, as described below.
                    DegradeType = "dirt",// mp nov 2015 . I set it to last indefinetely because I cannot come up with a meaningful part for it. and a part is required for all structures if we want to avoid them simply dissappearing when integrity degrades.
                    SalvageProcess = "salvageFirewoodStack",
                    Repair = "buildingRepair",
                }

            });
            #endregion

           
            #region compost pit  ..a bigger area for compost than the small container.
            listOfEntityTypes.Add(new EntityType("structure:compostPit")
            {
                Name = "Compost pit",
                SummaryDescription = "A simple pit that accelerates decomposition into organic matter",//
                Description = "Branches and other plant matter such as rotten plant food will, when stored here, quickly decompose into organic matter which can then be made into compost fertilizer.",//
                ThumbnailSmall = "HUD_thumbnail_compostPit",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "medium" }, //not necessarily for food, could also affect cotton crops
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {

                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "compostPitEmpty_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostPitFull"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostPitEmpty_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostPitEmpty_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostPitHalfFull_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HalfFull)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostPitFull"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostPitEmpty_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }
                    }
                },
                ContainerType = new StorageContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },

                    DefaultStorageSettings = "compostBinSettings",
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("moist", 5f)
                    {
                        FullStatePercentage = 0.7f,
                        HalfFullStatePercentage = 0.1f,
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-5,-2), 17), //
                            new CollideShape2D(new Vector2(5,-2), 17)
                        }
                    }
                },
                
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "sturdyConstruction",// shouldn't have to rebuild often, since it's just a pit, so it doesn't matter if it's not in its prime
                    SalvageProcess = "salvageCompostPit",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:stones", 1 } },//every structure must have parts, else the structure  just dissappears when integrity degrades.
                    Repair = "buildingRepair",
                }
            });
            #endregion



            #region compostbin heap
            listOfEntityTypes.Add(new EntityType("structure:compostBin")
            {
                Name = "Compost heap",
                SummaryDescription = "A simple container that accelerates decomposition into organic matter",//
                Description = "Branches and other plant matter such as rotten plant food will, when stored here, quickly decompose into organic matter which can then be made into compost fertilizer.",//
                ThumbnailSmall = "HUD_thumbnail_compostHeap",
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "medium" }, //not necessarily for food, could also affect cotton crops
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "compostHeapEmpty",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "compostHeap_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostHeapFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostHeapEmpty"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostHeap_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostHeapHalfFull"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostHeap_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HalfFull)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "compostHeapFull"}},
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "compostHeap_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }
                    }
                },
                ContainerType = new StorageContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, 
                    
                    DefaultStorageSettings = "compostBinSettings",
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    ItemStorageType = new ItemStorageType("moist", 5f)
                    {
                        FullStatePercentage = 1,
                        HalfFullStatePercentage = 0.15f,
                    }                   
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-2), 20)
                        }
                    }
                },               
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "sturdyConstruction",//bso shouldn't have to rebuild often, since it's just a container for trash, so it doesn't matter if it's not in its prime
                    SalvageProcess = "salvageCompostBin",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 2 } },
                    Repair = "buildingRepair",
                }
            });
            #endregion

            #region meatDryingRack
            listOfEntityTypes.Add(new EntityType("structure:meatDryingRack")
            {
                Name = "Meat drying rack",
                SummaryDescription = "A simple frame for drying meat", //
                Description = "A simple solution for drying thinly cut strips of meat. Must be protected from scavengers, however. ",//
                ThumbnailSmall = "HUD_thumbnail_meatDryingRack",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "meatDryingRack" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "meatDryingRackEmpty",
                            }
                        },

                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "meatDryingRackFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "meatDryingRackEmpty"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "meatDryingRackHalfFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.HalfFull)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "meatDryingRackFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }
                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, //

                    ProductionOutputStorageType = new ItemStorageType(1f) //make room for  the amount of meat for the processes, currently 10 X 0.05f, maybe some extra space for previous batch??
                    {
                        FullStatePercentage = 0.6f, //
                        HalfFullStatePercentage = 0.05f //each piece is around 0.05f bulk. As soon as one piece is put up, I want visual indication.
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(-7,-1), 14), //todo
                            new CollideShape2D(new Vector2(9,-6), 14),
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageMeatDryingRack",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 3 } } ,
                    Repair = "buildingRepair",
                }

            });
            #endregion


            #region DryingShed
            listOfEntityTypes.Add(new EntityType("structure:dryingShed")
            {
                Name = "Drying shed",
                SummaryDescription = "A structure for drying meat, raised from the ground", //
                Description = "This shed will protect the meat from rain while letting the wind blow through. It is raised on a pillar to protect the food from scavengers.",//
                ThumbnailSmall = "HUD_thumbnail_towerDryingShed",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "meatDryingRack" }, //used for placing the output in the tool container
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "towerDryingShed",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "towerDryingShed_g"
                        }

                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "towerDryingShed"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "clayGranary_construct" }},  //reused sprite                                  
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "towerDryingShed_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },

                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // is safe from scavengers

                    ProductionOutputStorageType = new ItemStorageType(4f) //
                    {
                        FullStatePercentage = 0.8f, //? //is it necessary to define this? if there's no sprite change.
                        HalfFullStatePercentage = 0.15f //each piece is around 0.05f bulk
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 3), 16)
                        },
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, -20), 31)
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "sturdyConstruction",
                    SalvageProcess = "salvageDryingShed",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakShingles", 1 }, { "item:solidMudBrick", 2 }, { "item:shadeleafCanes", 3 } },
                    Repair = "buildingRepair",
                }

            });
            #endregion

            #region peatStack
            listOfEntityTypes.Add(new EntityType("structure:peatStack")
            {
                Name = "Peat stack",
                SummaryDescription = "Wet peat slabs are placed here to dry so that they can be used as fuel", // "A simple stack for drying wet peat so that it can be used as fuel"
                Description = "",// todo
                ThumbnailSmall = "HUD_thumbnail_peatStack",//
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                  //  Category = GameData.Instance.AllStructureCategories["production"]
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "peatStackTool" },//used for placing the output in the tool container
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "peatStack_g"
                        }

                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "peatStack"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                                            
                                RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "peatStack_g" },
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "peatStack"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         }

                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // is safe from scavengers

                    ProductionOutputStorageType = new ItemStorageType(4f) // room enough. currently produces 10*0.3 bulk
                    {
                        FullStatePercentage = 0.05f, //
                    //    HalfFullStatePercentage = 0.15f //never seen because can only process one batch at a time
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 3), 16)
                        },
                        SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, -20), 31)
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    //no parts. to compensate, I set degradeprofile to dirt, as described below.
                    DegradeType = "dirt",// mp nov 2015 . I set it to last indefinetely because I cannot come up with a meaningful part for it. and a part is required for all structures if we want to avoid them simply dissappearing when integrity degrades.
                    SalvageProcess = "salvagePeatStack",     
                    Repair = "buildingRepair",
                }

            });
            #endregion


            #region hideRack
            listOfEntityTypes.Add(new EntityType("structure:hideRack")
            {
                Name = "Hide rack",
                SummaryDescription = "A simple frame for stretching and cleaning a hide", //
                Description = "An important tool for making hide products the primitive way.",//
                ThumbnailSmall = "HUD_thumbnail_hideRack",//
                TierOrArea = new TierOrArea() { Tier = "survival" },
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                   // Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "hideRack" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "hideRackEmpty",
                            }
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "hideRack"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "hideRackEmpty"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "hideRack"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                /*         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "hideRack"}},
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.HalfFull)
                         }*/
                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, //

                    ProductionOutputStorageType = new ItemStorageType(0.08f) //match with the size of hide and turnip guts
                    {
                        FullStatePercentage = 0.1f, //mp was 0.1f when emptied it was shown as full
                         //   HalfFullStatePercentage = 0.2f //mp was 0.05f. when emptied it was shown as full
                    }                    
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-1), 14),
                        }
                    }
                },               
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "adequateConstruction",
                    SalvageProcess = "salvageHideRack",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:sticks", 2 } },
                    Repair = "buildingRepair",
                }

            });
            #endregion
            #region RefineryRareMetal
            //NA MINING CAMP MATERIAL
            listOfEntityTypes.Add(new EntityType("structure:rareMetalRefinery")
            {
                Name = "Refinery: Rare-earth",
                SummaryDescription = "An advanced machine for separating rare-earth metals from their ores", //
                Description = "The refinery is so compact that it can be dismantled, moved and set up near the mineral deposits. It uses a series of chemical and mechanical processes such as solvent-extraction and flotation to refine rare-earth metals.",//
                ThumbnailSmall = "HUD_thumbnail_refiner",//
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "production",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    //Category = GameData.Instance.AllStructureCategories["production"]
                },
                ToolType = new ToolType()
                {
                    ToolTag = new[] { "rareMetalRefinery" },
                    Durability = ItemLoader.toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.Stationary
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "refiner",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "refiner_g"//
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "refiner"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "refiner_construct"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt)
                         },

             
                    }
                },
                ContainerType = new ToolContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // only human can interact

                    ProductionOutputStorageType = new ItemStorageType(0.35f) //mp should have room for just the output of one batch , see the processes "makeRareMetal"
                    {
                        FullStatePercentage = 0.1f, //not used because no sprite change
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {
                            new CollideShape2D(new Vector2(0,-1), 20),
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "advancedConstruction",
                    SalvageProcess = "salvageRareMetalRefinery",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:metalRefineryPart1", 1 }, { "item:metalRefineryEquipment", 1 } },
                    Repair = "buildingRepair",
                }

            });
            #endregion

            #region ///////Structures - NOT USED in any scenario



            #region collapse trap ! WIP ! not used
            /*
            listOfEntityTypes.Add(new EntityType("structure:collapseTrap")
            {
                Name = "Collapse trap",
                SummaryDescription = "A humane trap that keeps the animal alive, by luring it into a cage.",
                Description = "N/A",
                ThumbnailSmall = "HUD_thumbnail_abatis",
                //Triggers = new TriggerType[]  { GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"]   },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    Category = GameData.Instance.AllStructureCategories["defense"]
                },
                ContainerType = new ContainerType()
                {
                    CanBeEnteredByDesignerTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact" },
                    HomeContainerType = new HomeContainerType() //10f)  //items
                    {
                        ItemStorageType = new ItemStorageType("isolated", 8f),
                        DefaultStorageSettings = "homeStorage",
                        HasRallyPointInCourtyard = false,//true=rally point is between door and entity center  
                        Doors = new Vector2[]
                            {
                                new Vector2(2,3),
                            }
                    }
                },
                SpecialActions = new[] { "changeBaitToBlackpulp", "changeBaitToRatMeat", "activateSnare", "killTrappedAnimal" },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "springSnareSet",
                            }
                        },
                    },
                    SpriteConditions = new[]
                     {
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "springSnareSet" }},                                                                    
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Ordered)                            
                         },
                         new SpriteConditionInfo()
                         {
                               RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "springSnareSprung" }},   //mp: these are necessary but ruined the flavour anim...now there's no anim when sprung.                              

                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.BeingBuilt)                            
                         },
                         new SpriteConditionInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[] {  new RenderAsBillboardType() { AssetName = "springSnareSprung"  } },                                                              
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Inactive)    
                         }
                     }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    GridAlignedPlacement = true,
                    SelectionShapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 14)//
                            {
                                Offset= new Vector2(0,0)//
                                
                            }
                        }
                },
                PartKeys = new SerializableDictionary<string, int>() { { "item:shadeleafCanes", 1 } },
                NonLivingType = new NonLivingType() { PartsAreWeatherProof = true, DegradeType = GameData.Instance.AllDegradeTypes["improvisedEquipment"], SalvageProcess = "salvageCollapseTrap" } //add salvage process
            });*/
            #endregion


            #region campfirePotCrane //not used   
            #endregion

            #region fishTank (not implemented)
            /*
            listOfEntityTypes.Add(new EntityType("structure:fishTank")  // fish tank..live fish...called a livewell. no. takes an oxygen system and pump to get rid of waste..could be made from skimmer but too much crafting for them I think, when you think of how little they gain
            {
                Name = "Fish tank",
                StructureType = new StructureType()
                {

                    BuildByPlayer = true,
                    Category = GameData.Instance.AllStructureCategories["production"]
                },

                ContainerType = new ContainerType()
                {
                    CanTransactWithDesignerTags = new[] { "human" },
                    StorageContainerType = new StorageContainerType(Storage.Conditions.Aquarium, 8f)
                },

                RenderableType = new RenderableType()   //MP if you use, needs the new renderabletype migrated here
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "storageholeYellowleaves_g"
                        }
                    }
                },

                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Pad = padRadiusShelterAndStorage,
                    PadShape = CollidePrim.Circle,
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0, 0), 16)
                            {
                                Offset= new Vector2(0, 3)//offset the circle
                            }
                        }
                },


                PartKeys = new SerializableDictionary<string, int>() { { "item:thermalTarp", 1 } },
                NonLivingType = new NonLivingType() { PartsAreWeatherProof = true, DegradeType = "ricketyConstruction", SalvageProcess = "salvageFishTank" }

            });
            */
            #endregion


            #region Canopy house //currently not used
            /*      listOfEntityTypes.Add(new EntityType("structure:canopyHouse")
            {
                Name = "Canopy house",
                SummaryDescription = "Comfortable building which can house 4 people. Has space enough for holding bigger meetings.",
                Description = "", //TODO
                ThumbnailSmall = "HUD_thumbnail_clayPolyhedron", //todo
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact" },

                    ResidenceType = new ResidenceType() { LivingCapacity = 4, ComfortLevel = housingComfortBasicHigh }, //influences balancing of goals on scenario 5

                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                    DefaultStorageSettings = "homeStorage",
                    HasRallyPointInCourtyard = false,
                    UpgradesProfile = "basicHome4People",
                    Doors = new Vector2[]
                    {
                            new Vector2(-32,15), //was: new Vector2(-25,15)
                    }
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    Category = GameData.Instance.AllStructureCategories["shelter"]
                },
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "house3skin3",
                            }
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "house3_g"
                        }
                    },
                    ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "house3skin2"}},
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)
                         },
                         new ClientStateInfo()
                         {                                
                            RenderAsGroundSpriteType = new RenderAsGroundSpriteType(){ AssetName = "house3_g" },
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.BeingBuilt) 
                         }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Pad = padRadiusShelterAndStorage,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[]
                        {

                            new CollideShape2D(new Vector2(-48,-13), 22),
                            new CollideShape2D(new Vector2(-32,-11), 28),
                            new CollideShape2D(new Vector2(4,-20), 35),
                            new CollideShape2D(new Vector2(40,-13), 30),
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "advancedConstruction",
                    Repair = "buildingRepairCustomProcess", // needed because there is no production process to derive from. Can be changed when/if this becomes constructable. Was:  Repair = "buildingRepair"                
                    PartKeys = new SerializableDictionary<string, int>() { { "item:solidMudBrick", 2 }, { "item:structurePanels", 3 }, { "item:stones", 1 } }               
                }
            });*/
            #endregion
       

            #region //unused Pre-2011 HOUSES - NOT USED
            /*
            #region house1
            listOfEntityTypes.Add(new EntityType("structure:house1")
            {
                Name = "House 1",
                Description = "This building is well-planned with good amenities. It is a safe and comfortable home for a single family.",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 6, ComfortLevel = housingComfortMedium },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"    
                },
                StructureType = new StructureType()
                {

                }
                ,
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(0,-10)
                            },

                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house1"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house1_g"
                    }
                }}
            });
           

            #region house1b
            structure = new EntityType("structure:house1b")
            {
                Name = "House 1B",
                Description = "A modest residence.",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = housingComfortMedium },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"                       
                },
                StructureType = new StructureType()
                {

                },              
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {

                            new CollideShape2D(Vector2.Zero, 50)
                            {
                                Offset = new Vector2(0,-10)
                            },

                        }
                },


                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Stove },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house1skin1"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house1_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion

            #region house1c
            structure = new EntityType("structure:house1c")
            {
                Name = "House 1C",
                Description = "A modest residence.",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = housingComfortMedium },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"                     
                },
                StructureType = new StructureType()
                {


                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(0,-10)
                            },

                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Stove },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house1skin2"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house1_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
            #endregion

            #region house2
            structure = new EntityType("structure:house2")
            {
                Name = "House 2",
                Description = "This building is well-planned with good amenities. It is a safe and comfortable home for a single family.",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact" },
                    ResidenceType = new ResidenceType()
                    {
                        LivingCapacity = 6,
                        ComfortLevel = housingComfortMedium
                    },  //people
                   
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",                       
                    HasRallyPointInCourtyard = false,
                    Doors = new Vector2[]
                        {
                            new Vector2(-15,22),
                            new Vector2(21,25),
                        }                    
                },

                StructureType = new StructureType()
                {


                },               
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {

                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(20,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,0)
                            } 
                        }
                },

                HeatingType = new HeatingType()
                {
                    TypeOfHeating = HeatingType.HeatingTypes.Electrical
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house2"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house2_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);


            #region house2b
            structure = new EntityType("structure:house2b")
            {
                Name = "House 2B",
                Description = "This building is well-planned with good amenities. It is a safe and comfortable home for a single family.",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact" },
                    ResidenceType = new ResidenceType()
                    {
                        LivingCapacity = 6,
                        ComfortLevel = housingComfortMedium
                    },  //people
                   
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",                       
                    HasRallyPointInCourtyard = false,
                    Doors = new Vector2[]
                        {
                            new Vector2(-15,22),
                            new Vector2(21,25),
                        }
                    
                },
                StructureType = new StructureType()
                {

                },               
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(20,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,0)
                            } 
                        }
                },

                HeatingType = new HeatingType()
                {
                    TypeOfHeating = HeatingType.HeatingTypes.Electrical
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house2skin1"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house2_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
            #region house2c
            structure = new EntityType("structure:house2c")
            {
                Name = "House 2C",
                Description = "This building is well-planned with good amenities. It is a safe and comfortable home for a single family.",
                ContainerType = new HomeContainerType()
                {
                    CanBeEnteredByTags = new[] { "humanTransact" },
                    ResidenceType = new ResidenceType()
                    {
                        LivingCapacity = 6,
                        ComfortLevel = housingComfortMedium
                    },  //people
                  
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage",                       
                    HasRallyPointInCourtyard = false,
                    Doors = new Vector2[]
                        {
                            new Vector2(-15,22),
                            new Vector2(21,25),
                        }
                    
                },
                StructureType = new StructureType()
                {

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(20,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,0)
                            } 
                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house2skin2"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house2_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
            #endregion

            #region house3
            structure = new EntityType("structure:house3")
            {
                Name = "House 3",

                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"                       
                    
                },
                StructureType = new StructureType()
                {

                },
               DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(30,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,-15)
                            } 
                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house3"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house3_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #region house3b
            structure = new EntityType("structure:house3b")
            {
                Name = "House 3B",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                   
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"     
                },
                StructureType = new StructureType()
                {

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(30,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,-15)
                            } 
                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house3skin1"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house3_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
            #region house3c
            structure = new EntityType("structure:house3c")
            {
                Name = "House 3C",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"    
                },
                StructureType = new StructureType()
                {

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 50f)
                            {
                                Offset = new Vector2(30,-10)
                            },
                            new CollideShape2D(Vector2.Zero, 30f)
                            {
                                Offset = new Vector2(-45,-15)
                            } 
                        }
                },


                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "house3skin2"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "house3_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
            #endregion

            #region houseBunker
            structure = new EntityType("structure:houseBunker")
            {
                Name = "Bunker House",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"   
                },
                StructureType = new StructureType()
                {

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 40f)
                            {
                                Offset = new Vector2(40,15)
                            },
                            new CollideShape2D(Vector2.Zero, 40f)
                            {
                                Offset = new Vector2(0,0)
                            },
                            new CollideShape2D(Vector2.Zero, 40f)
                            {
                                Offset = new Vector2(-50,-10)
                            } 
                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "housebunker"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "housebunker_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion

            #region houseExplorer
            structure = new EntityType("structure:houseExplorer")
            {
                Name = "Laboratory",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                   ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"    
                },
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 60)
                            {
                                Offset = new Vector2(-10,-10)
                            } 
                        }
                },

                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "houseexplorer"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "houseexplorer_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion

            #region houseVeranda
            structure = new EntityType("structure:houseVeranda")
            {
                Name = "Veranda House",
                ContainerType = new HomeContainerType()
                {
                    ResidenceType = new ResidenceType() { LivingCapacity = 3, ComfortLevel = houseComfort },
                    ItemStorageType = new ItemStorageType("isolated", 8f),
                    DefaultStorageSettings = "homeStorage"       
                },
                StructureType = new StructureType()
                {


                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 60)
                            {
                                Offset = new Vector2(0,-10)
                            } 
                        }
                },


                HeatingType = new HeatingType() { TypeOfHeating = HeatingType.HeatingTypes.Electrical },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "houseveranda"
                }}
                ,
                    RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                    {
                        AssetName = "houseveranda_g"
                    }
                }}
            };
            listOfEntityTypes.Add(structure);
            #endregion
*/
            #endregion


            #region doghouse
            /*         structure = new EntityType("structure:doghouse")
            {
                Name = "Doghouse",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false,
                    // SpriteName = "doghouse",
                    IsAddon = true
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 20f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "doghouse"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "doghouse_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region greenhouse
    /*        structure = new EntityType("structure:greenhouse")
            {
                Name = "Greenhouse",
                StructureType = new StructureType()
                {
                    IsAddon = true//, WidthInTiles = 2, HeightInTiles = 1

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 25f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "greenhouse"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "greenhouse_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region rabbitCage
   /*         structure = new EntityType("structure:rabbitCages")
            {
                Name = "Rabbit cages",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false, //SpriteName = "rabbitcages",
                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 25f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "rabbitcages"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "rabbitcages_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region shed
     /*       structure = new EntityType("structure:shed")
            {
                Name = "Shed",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false, SpriteName = "shed",
                    IsAddon = true, WidthInTiles = 1, HeightInTiles = 1
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 25f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "shed"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "shed_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
#endregion
            #region shed b
     /*       structure = new EntityType("structure:shed b")
            {
                Name = "Shed B",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false, //SpriteName = "shed",
                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 25f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "shedskin1"
                }}
                        ,

                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "shed_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion
            

            #region japaneseGarden //in terrainfeatureloader instead
    /*        structure = new EntityType("structure:japaneseGarden")
            {
                Name = "Japanese garden",
                StructureType = new StructureType()
                {

                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 35f)
                            {
                                Offset = new Vector2(-15,0)
                            },
                            new CollideShape2D(Vector2.Zero, 35f)
                            {
                                Offset = new Vector2(15,0)
                            }
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "japanesegarden"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "japanesegarden_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region woodpile
  /*          structure = new EntityType("structure:woodpile")
            {
                Name = "Wood pile",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false, //SpriteName = "woodpile",
                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1
                },
                 DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 25f)
                            {
                                Offset = new Vector2(-24,0)
                            },
                            new CollideShape2D(Vector2.Zero, 25f)
                            {
                                Offset = new Vector2(10,-15)
                            }
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "woodpile"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "woodpile_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region scrapheap
  /*          structure = new EntityType("structure:scrapheap")
            {
                Name = "Scrapheap",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = false, //SpriteName = "scrapheap",
                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1
                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 15f)
                            {
                                Offset = new Vector2(-60,0)
                            },
                            new CollideShape2D(Vector2.Zero, 15f)
                            {
                                Offset = new Vector2(15,15)
                            },
                            new CollideShape2D(Vector2.Zero, 15f)
                            {
                                Offset = new Vector2(0,-15)
                            }
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "scrapheap"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "scrapheap_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region roboWorkshop
    /*        structure = new EntityType("structure:roboWorkshop")
            {
                Name = "Workshop",
                StructureType = new StructureType()
                {
                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 45f),
                            new CollideShape2D(Vector2.Zero, 25f)
                            {
                                Offset = new Vector2(-50,0)
                            }

                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "roboworkshop"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "roboworkshop_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region greenhouse2
       /*     structure = new EntityType("structure:greenhouse2")
            {
                Name = "Greenhouse",
                StructureType = new StructureType()
                {

                    IsAddon = true//, WidthInTiles = 1, HeightInTiles = 1

                },
                DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                    Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 30f)
                        }
                },
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]//new List<RenderAsBillboardType>()
                {new RenderAsBillboardType()
                {
                    AssetName = "greenhouse2"
                }}
                        ,
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "greenhouse2_g"
                        }
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion

            #region gravelroad
   /*         structure = new EntityType("structure:gravelroad")
            {
                Name = "Gravel road",
                StructureType = new StructureType()
                {
                    //IsEdgeFeature = true,
                    IsRoad = true,
                    BuildByPlayer = true,

                    Category = GameData.Instance.AllStructureCategories["miscellaneous"],

                },
                RenderableType = new RenderableType()
                {
                    RenderAsConnectedGroundSpriteType = new RenderAsConnectedGroundSpriteType()
                    {
                        AssetName = "gravelroad"
                    }
                },
                DirectionalLayoutType = new DirectionalLayoutType()
                {

                },
                TerrainType = new global::UWGame.SimSide.Entities.TerrainFeatureType()
                {
                    PathType = new PathType()
                    {
                        TransportCosts = new byte[] { 3, 2, 2, 1 }, // TEST ONLY! Use this: { 3, 2, 2, 1 },
                        // FootCost = 3,
                     //    ATVCost = 2,
                     //    CarCost = 2,
                        Rank = 1
                    }
                }
            };
            listOfEntityTypes.Add(structure);*/
            #endregion


            #endregion





        }

       
    }
}

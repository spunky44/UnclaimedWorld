using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    class StructureLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {


            //MP cannot move skimmer  to scenario 1 structureloader, get error, even though i also move salvage process.
            #region skimmerHull
            listOfEntityTypes.Add(new EntityType("structure:skimmerHull")
            {
                Name = "Aircraft wreck (hull)",  //was: Wrecked Skimmer (hull)     changed to:....Aircraft wreck (hull)   because after salvage: Stripped aircraft hull  ..the description is copyed to the other 3 aircraft parts.
                ThumbnailSmall = "HUD_thumbnail_skimmerHull",
                SummaryDescription = "Heavily damaged fuselage of a Skimmer aircraft",  //  http://www.pandorapedia.com/sa_2_samson  Range: 450 kilometers fully loaded   http://en.wikipedia.org/wiki/Tiltrotor#List_of_tiltrotor_aircraft   http://en.wikipedia.org/wiki/Ducted_fan  engine numbering from  left to right from the view of the pilot http://en.wikipedia.org/wiki/Aircraft_engine_position_number
                Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = false,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
                ContainerType = new StorageContainerType("isolated", 8f)
                {
                    CanTransactWithTags = new[] { "humanTransact" }
                },


                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "skimmerHull",
                                BaseCenter = new Vector2(0, 0), Offset = new Vector2(0, 0)
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "skimmerHull_g"
                        }
                    },
                    //         SpriteConditions = new[] //MP cannot be built
                    //        {
                    //   }
                },

                DefaultSimState = new SimStateInfo()
               {
                   GeometryLayoutType = new GeometryLayoutType()
                   {
                       Pad = AllGameData.StructureLoader.padRadiusShelterAndStorage, // 50f,
                       PadShape = CollidePrim.Circle,
                       Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0,0), 25) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset = new Vector2(4,-6)//Here you offset the circle with the amounts from in-game
                            },
                             new CollideShape2D(new Vector2(0,0), 26) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset = new Vector2(44 ,-22)//Here you offset the circle 
                            }
                        }
                   }
               },
                NonLivingType = new NonLivingType()
                 {
                     PartsAreWeatherProof = true,
                     DegradeType = "equipment",
                     SalvageProcess = "salvageSkimmerHull",
                     PartKeys = new SerializableDictionary<string, int>() { { "item:scrapMetal", 3 }, { "item:seatCushions", 3 }, { "item:inactivatedFoodCoolerUnit", 1 }, { "item:textile", 1 } } //"item:scrapMetal" part is destroyed in start event

                 }

            });
            #endregion

            #region skimmerTail
            listOfEntityTypes.Add(new EntityType("structure:skimmerTail")
            {
                Name = "Aircraft wreck (tail)",
                SummaryDescription = "Broken off tail section of a Skimmer aircraft",
                Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
                ThumbnailSmall = "HUD_thumbnail_skimmerTail",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {

                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },

                //  ContainerType = new ContainerType()
                //  {
                //       CanTransactWithDesignerTags = new[] { "human" },
                //       StorageContainerType = new StorageContainerType("earthCooled", 8f)
                //   },



                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "skimmerTail",
                           //     BaseCenter = new Vector2(0, 0), Offset = new Vector2(0, 0) //MP this did not have any coords before..
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "skimmerTail_g"
                        }
                    },
                    //        SpriteConditions = new[] //MP cannot be built
                    //         {
                    //     }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 12f, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(2,-1), 25)
                            {
                                Offset= new Vector2(-4,8)//offset the circle
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "equipment",
                    SalvageProcess = "salvageSkimmerTail",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:scrapMetal", 2 }, { "item:panelScraps", 1 } }, //"item:scrapMetal" part is destroyed in start event               
                }

            });
            #endregion

            #region skimmerEngineSide
            listOfEntityTypes.Add(new EntityType("structure:skimmerEngineSide")
            {
                Name = "Aircraft wreck (rotor #2)",
                SummaryDescription = "Damaged rotor of the Skimmer aircraft",
                Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
                ThumbnailSmall = "HUD_thumbnail_skimmerEngine",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = false,
                   // Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "skimmerEngineSideBuried",
                           //     BaseCenter = new Vector2(0, 0), Offset = new Vector2(0, 0) //MP this did not have any coords before..
                            }                    
                    
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "skimmerEngineSideBuried_g"
                        }
                    },
                    //   SpriteConditions = new[] //MP cannot be built
                    //       {
                    //       }
                },

                DefaultSimState = new SimStateInfo()
               {
                   GeometryLayoutType = new GeometryLayoutType()
                   {
                       Pad = 14f,
                       PadShape = CollidePrim.Circle,
                       Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(2,-1), 20)
                            {
                                Offset= new Vector2(0,-6)
                            }
                        }
                   }
               },

                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "equipment",
                    SalvageProcess = "salvageSkimmerEngineSide",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:scrapMetal", 1 }, { "item:propellerDome", 1 }, { "item:superconductingWire", 2 } } //"item:scrapMetal" part is destroyed in start event

                }

            });
            #endregion

            #region skimmerEngineTop
            listOfEntityTypes.Add(new EntityType("structure:skimmerEngineTop")
            {
                Name = "Aircraft wreck (rotor #1)",  //was: Wrecked Skimmer (engine 2)     changed to:....Aircraft wreck (engine 1)   ... after salvage: Stripped aircraft engine
                SummaryDescription = "Damaged rotor of the Skimmer aircraft",
                Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
                ThumbnailSmall = "HUD_thumbnail_skimmerEngine",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = false,
                  //  Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "skimmerEngineTop",
                           //     BaseCenter = new Vector2(0, 0), Offset = new Vector2(0, 0) //MP this did not have any coords before..
                            }                    
                    
                        },
                        //        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        //        {
                        //            AssetName = "_g"
                        //        }

                    },
                    //         SpriteConditions = new[] //MP cannot be built
                    //          {
                    //          }
                },

                DefaultSimState = new SimStateInfo()
               {
                   GeometryLayoutType = new GeometryLayoutType()
                   {
                       Pad = 20f,
                       PadShape = CollidePrim.Circle,
                       Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(2,-1), 20)
                            {
                                Offset= new Vector2(0,-6)
                            }
                        }
                   }
               },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "equipment",
                    SalvageProcess = "salvageSkimmerEngineTop",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:scrapMetal", 1 }, { "item:propellerDome", 1 }, { "item:superconductingWire", 2 } }, //"item:scrapMetal" part is destroyed in start event

                }
            });
            #endregion

           
            listOfEntityTypes.Add(new EntityType("structure:signalPyre")
            {
                DeleteRecord = true
            });

          /*   
           * listOfEntityTypes.Add(new EntityType("structure:helipad")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("structure:molecularAssembler")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("structure:satelliteGroundStation")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("structure:weatherStation")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("structure:fieldKitchen")
            {
                DeleteRecord = true
            });

 */
     
        }
    }
}

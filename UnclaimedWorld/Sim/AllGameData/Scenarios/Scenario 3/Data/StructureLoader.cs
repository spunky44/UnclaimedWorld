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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    class StructureLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {

            float padRadiusShelterAndStorage = 20f;



            listOfEntityTypes.Add(new EntityType("structure:signalPyre")
            {
                Name = "Signal pyre",  //http://www.artofmanliness.com/2011/05/23/wilderness-survival-know-your-distress-signals/
                SummaryDescription = "A fire that will produce a great deal of smoke, visible for miles",
                Description = "Built from a large amount of firewood covered with fresh spoak leaves. We should keep it burning as often as possible to increase chances of being found.",
                ThumbnailSmall = "HUD_thumbnail_signalPyre",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = true,
                    //Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },
            //    TierOrArea = new TierOrArea() { Tier = "survival",  },   not used in tut, not needed              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "signalPyre",
                           //     BaseCenter = new Vector2(36,29) //MP commented out because I use center of sprite (default) as base center
                            }                    
                    
                        },
                        /*     RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                             {
                                 AssetName = "daysheenTipi_g"
                             }*/
                    },
                    ClientStateConditions = new[]
                     {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "signalPyre" }},                                                                    
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered)                            
                         },
                         new ClientStateInfo() //MP if this is commented out, then the finished billboard will appear immediately when construction begins.
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "signalPyre_construct" }},                                    
                            //  RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "daysheenTipi_g" },
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
                            new CollideShape2D(new Vector2(0, 0), 22) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!
                            {
                                Offset= new Vector2(0, -2)//offset the circle
                            }
                        }
                    }
                },
                SharedSpecialActions = new[] { new Pair<string, bool>("lightSignalPyre", true) },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "ricketyConstruction",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:spoakBranches", 1 } }
                }
            });



            listOfEntityTypes.Add(new EntityType("structure:boatWreck")
            {
                Name = "Catamaran wreck", //
                ThumbnailSmall = "HUD_thumbnail_catamaran",
                SummaryDescription = "Our catamaran is capsized and damaged. Not going to sail again",
                Description = "\n NAME: Welcome Winds\n \n HOMEPORT: Noame\n \n TYPE:Catamaran\n \n BUILD YEAR:2422\n \n LENGTH: 22 m",
                CategoryKey = "miscellaneous",
                StructureType = new StructureType()
                {
                    BuildByPlayer = false,
                    //Category = GameData.Instance.AllStructureCategories["miscellaneous"]
                },

                //do not store stuff in it.
                /*            ContainerType = new ContainerType()
                            {
                                CanTransactWithDesignerTags = new[] { "human" },
                                StorageContainerType = new StorageContainerType(Storage.Conditions.Isolated, 8f)
                               {

                                }
                            },*/
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                                              
                             new RenderAsBillboardType(){ AssetName = "catamaran", Offset = new Vector2(-5, -12) } // this structure is asymmetric.  displace the sprite to the right relative to the ground sprite
                        },                                                                                       //BaseCenter property not set - using default because base center should be at the center of the billboard
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "catamaran_g"
                        }
                    }
                },
                //PointLayoutType = new PointLayoutType()
                //{
                //    GridAlignedPlacement = true,
                //},

                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = padRadiusShelterAndStorage, // 50f,
                        PadShape = CollidePrim.Circle,
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(0,0), 25) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset = new Vector2(-5, -12)//Here you offset the circle with the amounts from in-game
                            },
                             new CollideShape2D(new Vector2(0,0), 46) //Set radius of circle (3rd coord). ALWAYS set x,y to (0,0)!!
                            {
                                Offset = new Vector2(-5, -12)//Here you offset the circle 
                            }
                        }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    PartsAreWeatherProof = true,
                    DegradeType = "ricketyConstruction",
                    SalvageProcess = "salvageBoatWreck",
                    PartKeys = new SerializableDictionary<string, int>() { { "item:lines", 1 }, { "item:scrapMetal", 1 }, { "item:metalWire", 1 } } //"item:scrapMetal" used in damaging the structure with the start event "destroyBoatPart" 
                }

            });





//not necessary to remove structures because production manager display options is disabled and player can only see Attainalbe




    

     
        }

       /* private static EntityType CreatePlaceholder(string p)
        {
            throw new NotImplementedException();
        }*/
    }
}

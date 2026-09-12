using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.ClientSide.Particles;

namespace UWGame.SimSide.AllGameData
{
    public class TerrainFeatureLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {


            EntityType terrain = new EntityType("tracks")
            {
                Name = "Wheel path",
                /* StructureType = new StructureType()
                 {
                     //IsEdgeFeature = true,
                     IsRoad = true,
                     BuildByPlayer = false,

                     ConstructionPositions = 2,
                     ManSecondsOfWorkNeeded = 3,
                     Category = AllStructureCategories["structcat:Road"],
                     EdgeLayoutType = new EdgeLayoutType()
                     {

                     }

                 },*/
                RenderableType = new RenderableType()
                {
                    RenderAsConnectedGroundSpriteType = new RenderAsConnectedGroundSpriteType()
                    {
                        AssetName = "Tracks"
                    }
                },
                DirectionalLayoutType = new DirectionalLayoutType()
                {
                    // ConnectsToNeighbours = true
                },
                TerrainType = new global::UWGame.SimSide.Entities.TerrainFeatureType()
                {
                    PathType = new PathType()
                    {
                        TransportCosts = new byte[] { 3, 4, 3, 1 },
                        /*FootCost = 3,
                        ATVCost = 3,
                        CarCost = 4,*/
                        Rank = 2
                    }
                }
            };
            listOfEntityTypes.Add(terrain);
            terrain = new EntityType("footpath")
            {
                Name = "Foot path",
                RenderableType = new RenderableType()
                {
                    RenderAsConnectedGroundSpriteType = new RenderAsConnectedGroundSpriteType()
                    {
                        AssetName = "footpath"
                    }
                },
                DirectionalLayoutType = new DirectionalLayoutType()
                {
                    // ConnectsToNeighbours = true
                },
                TerrainType = new global::UWGame.SimSide.Entities.TerrainFeatureType()
                {
                    PathType = new PathType()
                    {
                        TransportCosts = new byte[] { 3, 5, 4, 1 },
                        /*
                        FootCost = 3,
                        ATVCost = 4,
                        CarCost = 5,*/
                        Rank = 3
                    }
                }
            };
            listOfEntityTypes.Add(terrain);
            terrain = new EntityType("terrain:plains")
            {
                Name = "Plains",

                TerrainType = new global::UWGame.SimSide.Entities.TerrainFeatureType()
                {
                    PathType = new PathType()
                    {
                        TransportCosts = new byte[] { 3, 5, 4, 1 },

                        Rank = 3
                    }
                }
            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:rockformation1")
            {
                Name = "Rocks",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            /*baseCenter.X = 120f;
                              baseCenter.Y = 72f;
                             */
                            new RenderAsBillboardType(){ AssetName = "rockwall1", Offset = new Vector2(204, 39) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall2", Offset = new Vector2(68, 103) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall3", Offset = new Vector2(40, 83) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall4", Offset = new Vector2(136, 103) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall5", Offset = new Vector2(189, 73) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall6", Offset = new Vector2(226, 56) - new Vector2(120f, 72f)},
                            new RenderAsBillboardType(){ AssetName = "rockwall7", Offset = new Vector2(12, 70) - new Vector2(120f, 72f)},
                        },

                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType() { AssetName = "rockwall_g" }
                    }
                },
                TerrainType = new TerrainFeatureType()

            };
            listOfEntityTypes.Add(terrain);



            #region natural terminals
            listOfEntityTypes.Add(new EntityType("terrain:naturalLandTerminal")
            {
                RequiresRollToDetect = false,
                IsNeverInFogOfWar = false,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                UsesMemory = true,
                IsSelectable = true,
                Name = "Land passage",
                SummaryDescription = "A passage to a neighbor area which can be reached by foot. (Leads to another site)", //was Leads to or from the site
                Description = "The neighbor site is close enough that we can see it from here. Communication could be made with simple means such as signs and sound.",//todo
                ThumbnailSmall = "HUD_thumbnail_path",
                UseTypeNameForDisplay = false, // use geographical names
               
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "shedskin1"                        
                            }      
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "shed_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 48f,
                        PadShape = CollidePrim.Circle
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Land
                },
                // no sensor needed because no trading      
            });

            listOfEntityTypes.Add(new EntityType("terrain:naturalPseudoWaterTerminal")
            {
                RequiresRollToDetect = false,
                IsNeverInFogOfWar = false,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                UsesMemory = true,
                IsSelectable = true,
                Name = "Water passage",
                SummaryDescription = "Water that can be crossed by a determined (or desperate) swimmer. (Leads to another site)",  //was: Leads to or from the site 
                Description = "The neighbor site is close enough that we can see it from here. Communication could be made with simple means such as signs and sound.",//todo
                ThumbnailSmall = "HUD_thumbnail_waterPassage", //mp put this back in , im missing the asset right now: "HUD_thumbnail_waterPassage" 
                UseTypeNameForDisplay = false, // use geographical names
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "shedskin1"                        
                            }      
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "shed_g"
                        }
                    }
                },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Pad = 48f,
                        PadShape = CollidePrim.Circle
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                TerminalType = new TerminalType()
                {
                    TypeOfTerminal = TerminalType.TypesOfTerminal.Land
                },
                // no sensor needed because no trading      
            });
            #endregion



          /*  terrain = new EntityType("rockwall1")
            {
                Name = "rockwall1",
                RenderAsBillboardType = new RenderAsBillboardType[]
                {                   
                    new RenderAsBillboardType(){ AssetName = sprite, AspectRatio = widthHeightRatio }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },
                TileLayoutType = new TileLayoutType() { LayoutName = sprite, GridAlignedPlacement = false },

                RockType = new RockType() { Bulk = bulk }
            };
            listOfEntityTypes.Add(terrain);
                                   
            terrain.GeometryLayoutType = new GeometryLayoutType()
            {
                shapes = new CollideShape2D[] 
                    {
                        new CollideShape2D(new Vector2(-size.Value, 0f), size.Value),
                        new CollideShape2D(-(0.5f * rectangleWidthHeightRatio * size.Value), -size.Value, (0.5f * rectangleWidthHeightRatio * size.Value), size.Value), 
                        new CollideShape2D(new Vector2(size.Value, 0f), size.Value),

                    }
            };*/

            //MS: Ambience starts here
            terrain = new EntityType("terrain:shoreWavesLongA")
            {
                Name = "S: Water, Shore waves, long A",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/shoreWavesCalmLonger3A" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalmLonger3A" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };

            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesLongB")
            {
                Name = "S: Water, Shore waves, long B",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/shoreWavesCalmLonger3B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalmLonger3B" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesLongLowerA")
            {
                Name = "S: Water, Shore waves, long & lower A",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/shoreWavesCalmLongerLower3A" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalmLongerLower3A" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };

            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesLongLowerB")
            {
                Name = "S: Water, Shore waves, long & lower B",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/shoreWavesCalmLongerLower3B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalmLongerLower3B" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesA")
            {
                Name = "S: Water, Shore waves A",
                IsNeverInFogOfWar = true, 
                RequiresRollToDetect = false, 
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient/water/shoreWavesCalm2A" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalm2A" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };

            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesB")
            {
                Name = "S: Water, Shore waves B",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient/water/shoreWavesCalm2B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalm2B" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };

            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:shoreWavesC")
            {
                Name = "S: Water, Shore waves",
                IsNeverInFogOfWar = true, //if false, this makes this terrain type disappear (fade out) in FOW  .....MP: I've set this ambient to be the only one that is always playing in FOW, it's a nice background sound when you scroll around the map. The other ambient sounds will require an agent to move into the area, so it feels more like exploration when those ambients are heard.
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient/water/shoreWavesCalm2C" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/shoreWavesCalm2C" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:brook")
            {
                Name = "S: Water, Brook",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient/water/waterBrook" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterBrook" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:riverStreamA")
            {
                Name = "S: Water, River, medium stream A",
                IsNeverInFogOfWar = true, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamA" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamA" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:riverStreamB")
            {
                Name = "S: Water, River, medium stream B",
                IsNeverInFogOfWar = true, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamB" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamB" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:riverStreamC")
            {
                Name = "S: Water, River, medium stream C",
                IsNeverInFogOfWar = true, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamC" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterRiverMediumStreamC" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:brookSmallA")
            {
                Name = "S: Water, Brook, Small A",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterBrookSmallA" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterBrookSmallA" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:brookSmallB")
            {
                Name = "S: Water, Brook, Small B",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterBrookSmallB" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterBrookSmallB" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:brookSmallC")
            {
                Name = "S: Water, Brook, Small C",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/waterBrookSmallC" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/waterBrookSmallC" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:mudflatsA")
            {
                Name = "S: Water, Mudswamp, A",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/mudflats1" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/mudflats1" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:mudflatsB")
            {
                Name = "S: Water, Mudswamp, B",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/water/mudflats2" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/water/mudflats2" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:windMid")
            {
                Name = "S: Wind stable, mid freq",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/wind/windMidFreq" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/wind/windMidFreq" }               
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:windHighFreqA")
            {
                Name = "S: Wind gusty, hi-freq",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.      
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/wind/windHighFreq" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                {
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/wind/windHighFreq" }
                  
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:windHighFreqB")
            {
                Name = "S: Wind gusty, hi-freq (cycle 3 var)",
                IsNeverInFogOfWar = true,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.      
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[] { "ambient/wind/windHighFreqGustyLong1A", "ambient/wind/windHighFreqGustyLong1B", "ambient/wind/windHighFreqGustyLong1C" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/wind/windHighFreqGustyLong1A", "ambient/wind/windHighFreqGustyLong1B", "ambient/wind/windHighFreqGustyLong1C" }                 
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:insectsMedium")
            {
                Name = "S: Insects, medium group",
                IsNeverInFogOfWar = false, 
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 20f, Max = 22f },
                        Sounds = new[] { "ambient/aliens/insectsMediumGroup" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/insectsMediumGroup" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:insectsLongConstant")
            {
                Name = "S: insect, long w break",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 10f, Max = 15f },
                        Sounds = new[]{ "ambient/aliens/insectsLongConstantwBreak" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/insectsLongConstantwBreak" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:insectFlying")
            {
                Name = "S: Insect, flying (cycle 2 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 22f, Max = 36f },
                        Sounds = new[] { "ambient/insects/insectFlying1A", "ambient/insects/insectFlying1B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                {
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/insects/insectFlying1A", "ambient/insects/insectFlying1B" }              
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:insectGroup1")
            {
                Name = "S: Insects, (cycle 3 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 2f, Max = 5f },
                        Sounds = new[] { "ambient/insects/insectGroup1A", "ambient/insects/insectGroup1B", "ambient/insects/insectGroup1C" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/insects/insectGroup1A", "ambient/insects/insectGroup1B", "ambient/insects/insectGroup1C" }             
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:insectsLongSingle")
            {
                Name = "S: insect, single, long",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 12f, Max = 22f },
                        Sounds = new[]{ "ambient/aliens/insectsSingleB" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/insectsSingleB" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:birdsSingle1")
            {
                Name = "S: Bird 1, single, (cycle 3 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 10f, Max = 15f },
                        Sounds = new[] { "ambient/birds/birdSingle1A", "ambient/birds/birdSingle1B", "ambient/birds/birdSingle1C" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/birds/birdSingle1A", "ambient/birds/birdSingle1B", "ambient/birds/birdSingle1C" }               
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:birdsSingle2")
            {
                Name = "S: Bird 2, single, (cycle 3 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 6f, Max = 8f },
                        Sounds = new[] { "ambient/birds/birdSingle2A", "ambient/birds/birdSingle2B", "ambient/birds/birdSingle2C" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/birds/birdSingle2A", "ambient/birds/birdSingle2B", "ambient/birds/birdSingle2C" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:birdsSingle3")
            {
                Name = "S: Bird 3, single, (cycle 2 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 12f, Max = 18f },
                        Sounds = new[] { "ambient/birds/birdSingle3A", "ambient/birds/birdSingle3B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/birds/birdSingle3A", "ambient/birds/birdSingle3B" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:birdsSingle4")
            {
                Name = "S: Bird 4, single, (cycle 4 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 8f, Max = 21f },
                        Sounds = new[] { "ambient/birds/birdSingle4A", "ambient/birds/birdSingle4B", "ambient/birds/birdSingle4C", "ambient/birds/birdSingle4B" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/birds/birdSingle4A", "ambient/birds/birdSingle4B", "ambient/birds/birdSingle4C", "ambient/birds/birdSingle4B" }               
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:birdsSingle5")
            {
                Name = "S: Bird 5, single, (cycle 5 var)",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 14f, Max = 24f },
                        Sounds = new[] { "ambient/birds/birdSingle5A", "ambient/birds/birdSingle5B", "ambient/birds/birdSingle5C", "ambient/birds/birdSingle5D", "ambient/birds/birdSingle5E" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/birds/birdSingle5A", "ambient/birds/birdSingle5B", "ambient/birds/birdSingle5C", "ambient/birds/birdSingle5D", "ambient/birds/birdSingle5E" }                
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:swampFrogsA")
            {
                Name = "S: frogs A, swamp",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 20f, Max = 38f },
                        Sounds = new[]{ "ambient/aliens/swampFrogs" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/swampFrogs" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:swampFrogsB")
            {
                Name = "S: frogs B, swamp",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 22f, Max = 31f },
                        Sounds = new[]{ "ambient/aliens/swampFrogs2" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/swampFrogs2" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:swampFrogsC")
            {
                Name = "S: frogs C, swamp",
                IsNeverInFogOfWar = false,
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        DelayBetweenSounds = new NormalDistribution() { Min = 12f, Max = 20f },
                        Sounds = new[]{ "ambient/aliens/swampFrogs3" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/swampFrogs3" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:wormSingle")
            {
                Name = "S: Worm, single worm calls",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.             

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient/aliens/wormAmbienceSingle" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient/aliens/wormAmbienceSingle" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);

            terrain = new EntityType("terrain:rattleWooden")
            {
                Name = "S: Rattle, wooden",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.             
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                      
                        Sounds = new[]{ "ambient\\aliens\\rattleWoodenAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\rattleWoodenAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:toothCrickets")
            {
                Name = "S: Tooth crickets",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient\\aliens\\toothCricketsAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\toothCricketsAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:angryToyAmbient")
            {
                Name = "S: Angry toy",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        Sounds = new[]{ "ambient\\aliens\\angryToyAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\angryToyAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:croakerAmbient")
            {
                Name = "S: Croaker",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                       
                        Sounds = new[]{ "ambient\\aliens\\croakerAmbient" }
                    }
                },                
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\croakerAmbient" }
                    }
                },                                         
                  
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:hummingClickClackingAmbient")
            {
                Name = "S: Humming, ClickClacking Ambient",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                      
                        Sounds = new[]{ "ambient\\aliens\\hummingClickClackingAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\hummingClickClackingAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:rattleBuzzAmbient")
            {
                Name = "S: Rattle, Buzz Ambient",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                      
                        Sounds = new[]{ "ambient\\aliens\\rattleBuzzAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\rattleBuzzAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:spacechickenAmbient")
            {
                Name = "S: Spacechicken Ambient",
                IsNeverInFogOfWar = false, // this makes this terrain type disappear (fade out) in FOW
                RequiresRollToDetect = false, // this makes this terrain type fade in when not in FOW, without having to be detected first.              
              
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {                      
                        Sounds = new[]{ "ambient\\aliens\\spacechickenAmbient" }
                    }
                },
                EditorRenderableType = new RenderableType() 
                { 
                    DefaultClientState = new ClientStateInfo() 
                    { 
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } },
                        Sounds = new[] { "ambient\\aliens\\spacechickenAmbient" }
                    } 
                },                                         
               
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            };
            listOfEntityTypes.Add(terrain);


            terrain = new EntityType("terrain:groundFog")
            {
                Name = "Ground fog",
                RenderableType = new RenderableType()
                {
                    ParticleEmitterTypes = new[] { new ParticleEmitterType() { ParticleSystemKey = "groundFog" } }
                  
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false }

            };
            listOfEntityTypes.Add(terrain);


/////Crates:

            EntityType crates = new EntityType("terrain:crates")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true, // NEW
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Lost supplies",
                SummaryDescription = "Items that fell from the skimmer during the crash",
                Description = "We have spotted some of our items lying at the bottom of this sandstone canyon. It should be possible to retrieve them by climbing down, using suitable rope.",
                DetectionTag = "hardToSpot",

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "moss1_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false },

                IsSelectable = true,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = AllGameData.TerrainFeatureLoader.MakeRectangle(0, 2, 3, 3)
                },
                SharedSpecialActions = new [] { new Pair<string, bool>("retrieveCrates", true )}
            };
            
            crates.DefaultSimState.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(crates);


//rescue:

            EntityType unconscious = new EntityType("terrain:unconscious")
            {
                RequiresRollToDetect = false, // false: cannot fail to detect him when approaching him in the FOW. This prevents jobs from being cancelled.
                IsNeverInFogOfWar = false,
                UsesMemory = true, //was true, but should be false because I want the marker to dissappear when they know he's dead from afar. he wears a life-sign sensor and tracker. 
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Lost team member", // this is the type name. not the instance name. when destroying it with an event, we use the instance name which is set in the spawn event in the scenario file
                SummaryDescription = "Our lost mission member is alive but motionless at the bottom of this crevice", //text duplicated in the process rescueColleague
                Description = "He seems to be unconscious. We need to climb down and perform first aid as soon as possible.",
                DetectionTag = "kindaHardToSpot",

                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "moss1_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },

                IsSelectable = true,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = AllGameData.TerrainFeatureLoader.MakeRectangle(0, 2, 3, 3), //
                },
                SharedSpecialActions = new [] { new Pair<string, bool>("rescueColleague", true ) }
            };


            unconscious.DefaultSimState.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(unconscious);

/////Binal Rat Nests:
            //bso  NOT USED ATM

            /*
            EntityType ratNest = new EntityType("terrain:ratNest")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true, // NEW
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Rat Nest",
                SummaryDescription = "Entrance to an underground nest of Binal Rats",
                Description = "WIP",
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "moss1_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false },
                DetectionTag = "kindaHardToSpot",
                ThreatType = new ThreatType()
                {
                    StrengthRating = StrengthRating.WeakerThanHumans //maybe set to veryweak if it blocks hunt zones too much. VeryWeak = 0, WeakerThanHumans = 1, LikeHumans = 2, StrongerThanHumans = 3, VeryStrong = 4 
                },
                IsSelectable = true,

                GeometryLayoutType = MakeRectangle(0, 2, 3, 3), //
                SpecialActions = new [] { new Pair<string, bool>("digUpRatNest", true ) }
            };
            ratNest.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(ratNest);
            */
/////Quadite Nests:



            EntityType nest = new EntityType("terrain:quaditeNest")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true, // NEW
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Quadite Nest",
                SummaryDescription = "Entrance to an underground nest of dangerous quadites",
                Description = "The nest lies in a cave which is likely part of a bigger underground system of so-called lava caves which have been formed from ancient volcanic activity. The cave network could hold countless individuals - this number would depend on the amount of prey available in the area.\n We would expect to see other entrances to the underground network which could hold other quadite nests.\n Approach with extreme caution.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "moss1_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },
                DetectionTag = "kindaHardToSpot",
                ThreatType = new ThreatType()
                {
                    StrengthRating = StrengthRating.WeakerThanHumans //maybe set to veryweak if it blocks hunt zones too much. VeryWeak = 0, WeakerThanHumans = 1, LikeHumans = 2, StrongerThanHumans = 3, VeryStrong = 4 
                },
                IsSelectable = true,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = MakeRectangle(0, 2, 3, 3) //
                },
                SharedSpecialActions = new [] { new Pair<string, bool>("useSulfurSmokeBomb", true) }
            };
            nest.DefaultSimState.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(nest);


            EntityType leafcutterNest = new EntityType("terrain:fieldQuaditeNest")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Field quadite nest",
                SummaryDescription = "Entrance to an underground nest network",
                Description = "Underneath firegrass, the field quadites build a multitude of nests interconnected by underground passages.", //
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "holeSoilSmall_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },
                DetectionTag = "kindaHardToSpot",
                ThreatType = new ThreatType()
                {
                    StrengthRating = StrengthRating.VeryWeak
                },
                IsSelectable = false,
                SharedSpecialActions = new[] { new Pair<string, bool>("useVarmintBomb", true) },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = MakeRectangle(0, 2, 3, 3)
                }
            };
            leafcutterNest.DefaultSimState.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(leafcutterNest);

            EntityType swarmerNest = new EntityType("terrain:swarmerNest")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Swarmer nest",
                SummaryDescription = "Entrance to an underground nest network",
                Description = "Underneath firegrass, the field quadites build a multitude of nests interconnected by underground passages.", //
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "holeSoilSmall_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },
                DetectionTag = "kindaHardToSpot",
                ThreatType = new ThreatType()
                {
                    StrengthRating = StrengthRating.VeryWeak
                },
                IsSelectable = false,
                SharedSpecialActions = new[] { new Pair<string, bool>("useVarmintBomb", true) },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = MakeRectangle(0, 2, 3, 3)
                }
            };
            leafcutterNest.DefaultSimState.GeometryLayoutType.Pad = 20f;
            listOfEntityTypes.Add(swarmerNest);
            //todo: put in access point
 
     

////////////////tutorial


            EntityType crevice = new EntityType("terrain:crevice")// for crossing this one, you need  metal wire (catamaran)
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Gorge",
                SummaryDescription = "A narrow gorge.",
                Description = "We need to find a way across.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "terrainBlockerWide_g" //invisible blocker 
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },
                DetectionTag = "largeOnGround",
                IsSelectable = true,
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = AllGameData.TerrainFeatureLoader.MakeRectangle(0, 0, 150, 44)
                },
                SharedSpecialActions = new[] { new Pair<string, bool>("buildRopeBridge", true) } //mp not used:  , new Pair<string, bool>("buildRopeBridgeVine", true),
            };
            crevice.DefaultSimState.GeometryLayoutType.Pad = 20f;//update name  when copy+pasting!!
            listOfEntityTypes.Add(crevice);//update name  when copy+pasting!!

/*
            EntityType crevice2 = new EntityType("terrain:crevice2") // for crossing this one, you need a vine
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Gorge",
                SummaryDescription = "A narrow gorge.",
                Description = "We need to find a way across.",
                RenderableType = new RenderableType()
                {
                    Default = new SpriteConditionInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "terrainBlockerWide_g" //invisible blocker
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },
                DetectionTag = "largeOnGround",
                IsSelectable = true,

                GeometryLayoutType = AllGameData.TerrainFeatureLoader.MakeRectangle(0, 0, 150, 44), //
                SpecialActions = new [] { "buildRopeBridgeVine", "retrieveCrates" } //
            };
            crevice2.GeometryLayoutType.Pad = 20f;//update name  when copy+pasting!!
            listOfEntityTypes.Add(crevice2);//update name  when copy+pasting!!

*/

            EntityType terrainBlockerWide = new EntityType("terrain:terrainBlockerWide") // ordinary terrain feature, invisible to player.
            {
                Name = "Terrain Blocker Wide", // should be given an instance name via a spawn event/EntityData for scripted access
                SummaryDescription = "for blocking passage",
               /* RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "terrainBlockerWide_g" //invisible blocker
                        }
                    }
                },*/
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false },

                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = AllGameData.TerrainFeatureLoader.MakeRectangle(0, 0, 150, 44)
                }

            };
            terrainBlockerWide.DefaultSimState.GeometryLayoutType.Pad = 20f; //update name  when copy+pasting!!
            listOfEntityTypes.Add(terrainBlockerWide);  //update name when copy+pasting!!


           

            EntityType ropeBridge = new EntityType("terrain:ropeBridge")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true, // NEW
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Rope bridge",
                SummaryDescription = "We built it. It's safe enough to cross.",
                Description = "",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "ropeBridge_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false },
                DetectionTag = "kindaHardToSpot",
                IsSelectable = true,
            };
            //       ropeBridge.GeometryLayoutType.Pad = 20f;

            listOfEntityTypes.Add(ropeBridge); //when copy+pasting, remember to rename!!



            EntityType catamaranWreck = new EntityType("terrain:catamaranWreck") //unsalvagable, therefore terrain
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true, // NEW
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.OnlyWhenSelected,
                Name = "Catamaran wreck",
                SummaryDescription = "Our catamaran is capsized and damaged. Not going to sail again",
                Description = "\n NAME: Welcome Winds\n \n HOMEPORT: Spoakdale\n \n TYPE:Catamaran\n \n BUILD YEAR:2422\n \n LENGTH: 22 m",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
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
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {

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
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false },
                DetectionTag = "kindaHardToSpot",
                IsSelectable = true,
            };

            listOfEntityTypes.Add(catamaranWreck); //when copy+pasting, remember to rename!!

            // TB FishTrap 19.03.2015
            #region fishTrap
                #region fishTrapSpotCreek
            EntityType fishTrapSpotCreek = new EntityType("terrain:fishTrapSpotCreek") //fish weir placed across creek. catches freshwater fish (carbon tail. see initializeFishTrap
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Fish weir location", //"Fish (Creek)"
                SummaryDescription = "This spot is suitable for setting up a fish weir.",
                Description = "A fish trap made of wooden fences could catch a large number of fish when they migrate through this body of water.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
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
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "inDeeperWaterFishingSpot", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("placeFishTrapCreekSticks", true), new Pair<string, bool>("placeFishTrapCreekNet", true) } //
                
            };
            listOfEntityTypes.Add(fishTrapSpotCreek);
                #endregion
                #region fishTrapSpotCoast
            EntityType fishTrapSpotCoast = new EntityType("terrain:fishTrapSpotCoast") //catches saltwater fish (streak fin. see initializeFishTrapCoast)
             
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Fish trap location (saltwater)", //"Fish (Coast)"
                SummaryDescription = "This spot is suitable for setting up a fyke to catch the 'streak fin'",
                Description = "N/A",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapFyke_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "inDeeperWaterFishingSpot", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new [] { new Pair<string, bool>("placeFishTrapCoast", true ) } //
            };
            listOfEntityTypes.Add(fishTrapSpotCoast);
                #endregion

                #region fishTrapSpotShore
            EntityType fishTrapSpotShore = new EntityType("terrain:fishTrapSpotShore") //catches freshwater fish (carbon tail. see initializeFishTrap
            
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Fish trap location (freshwater)", //"Fish (Shore)"
                SummaryDescription = "This spot is suitable for setting up a fish trap to catch the 'carbon tail'",
                Description = "N/A",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "fishTrapCylinderSmall_g"
                        }
                    }
                }, 
              //  AccessPointDirection = new Vector3(0f, -1f, 0f), //mp no use for this currently July 2016 because the the accesspoint must never be blocked anyway.
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "inDeeperWaterFishingSpot", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("placeFishTrapShoreBasket", true), new Pair<string, bool>("placeFishTrapShoreHoopNet", true) } //
            };
            listOfEntityTypes.Add(fishTrapSpotShore);
                #endregion
            #endregion

            #region Piers
            #region pierSpot
            EntityType pierSpot = new EntityType("terrain:pierSpot") //
            {
                RequiresRollToDetect = false, //mp copied from "terrain:unconscious". so that the spot is visible in the explored area.
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Port location", //
                SummaryDescription = "Location suitable for setting up a boat landing or small port",
                Description = "This location is navigable for boats and barges that come in from the sea. They can moor here if we build a landing or a small port.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                      
                            new RenderAsBillboardType()
                            { 
                                AssetName = "landingImprovised",
                                Offset = new Vector2(-20,-28) // this structure has billboard and groundsprite of different sizes and positions.   displace the sprite  relative to the ground sprite   (-20, -16f)  

                            }                   
                        },
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "landingImprovised_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                //      DetectionTag = "inDeeperWaterFishingSpot", //
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("buildSimplePort", true), new Pair<string, bool>("buildCanopyPort", true), new Pair<string, bool>("buildImprovisedLanding", true) } //

            };
            listOfEntityTypes.Add(pierSpot);
            #endregion
            #endregion


            #region clay deposit
            listOfEntityTypes.Add(new EntityType("terrain:clayDeposit")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Clay deposit",
                SummaryDescription = "A suitable place to dig a clay pit",
                Description = "This area has a large clay deposit below the surface that can be extracted if we establish a clay pit.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "clayPit_g"
                        }
                    }   
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action 
                IsSelectable = true,
                SharedSpecialActions = new [] { new Pair<string, bool>("establishClayPit", true) }
            });

            #endregion

            #region salt deposit
            listOfEntityTypes.Add(new EntityType("terrain:saltDeposit")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Salt deposit",
                SummaryDescription = "A suitable place to dig a salt mine",
                Description = "This area has a large rock salt deposit below the surface that can be extracted if we establish a salt mine.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "saltMine_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("establishSaltMine", true) }
            });

            #endregion

            #region bog ore deposit
            listOfEntityTypes.Add(new EntityType("terrain:bogOreDeposit")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Bog ore deposit",
                SummaryDescription = "A suitable place to establish a bog ore pit",
                Description = "This area has a large bog ore deposit below the surface that can be extracted if we establish a pit.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "bogOrePit_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("establishBogOrePit", true) }
            });

            #endregion

            #region Scandium deposit    raremetal ore deposit
            //NA MINING CAMP MATERIAL
            listOfEntityTypes.Add(new EntityType("terrain:rareMetalOreDeposit1")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Scandium deposit",
                SummaryDescription = "A site with a high concentration of scandium",
                Description = "The rare-earth metal scandium is usually difficult to mine but this area has highly concentrated scandium ores below the surface that can be extracted if we establish a simple pit.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "terrainBlockerWide_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("establishRareMetalOrePit", true) } // TODO
            });
            #endregion
            #region Terbium deposit    raremetal ore deposit2
            //NA MINING CAMP MATERIAL
            listOfEntityTypes.Add(new EntityType("terrain:rareMetalOreDeposit2")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Terbium deposit",
                SummaryDescription = "A site with a high concentration of terbium",
                Description = "The rare-earth metal terbium is usually difficult to mine but this area has highly concentrated terbium ores below the surface that can be extracted if we establish a simple pit.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "terrainBlockerWide_g"
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = false, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("establishRareMetalOrePit2", true) } // TODO
            });
            #endregion
            

            #region peat deposit
            listOfEntityTypes.Add(new EntityType("terrain:peatDeposit")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Peat deposit",
                SummaryDescription = "A suitable place to establish a peat bank where peat fuel can be cut",
                Description = "This grassy area of bog or marsh has a large deposit of peat just below the surface that can be extracted if we dig down through the firegrass sod.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "peatBank_g" //
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new[] { new Pair<string, bool>("establishPeatBank", true) }
            });

            #endregion

            // TB Farming 19.03.2015
            #region Farming

            //http://en.wikipedia.org/wiki/Arable_land

                #region smallPlotSpot
            EntityType smallPlotSpot = new EntityType("terrain:smallPlotSpot")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Small arable plot",
                SummaryDescription = "A small farm plot could be made here.",
                Description = "This area of firegrass soil is free from rocks and has the right conditions for establishing a small farm plot. First, the land would need to be tilled.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "plotPlowed_g"
                        }
                    },
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new [] { new Pair<string, bool>("establishSmallPlot", true) }
            };
            listOfEntityTypes.Add(smallPlotSpot);
                #endregion
                #region largePlotSpot
            EntityType largePlotSpot = new EntityType("terrain:largePlotSpot")
            {
                IsNeverInFogOfWar = false,
                UsesMemory = true,
                ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                Name = "Large arable plot",
                SummaryDescription = "A large farm plot could be made here.",
                Description = "This area of firegrass soil is free from rocks and has the right conditions for establishing a large farm plot. First, the land would need to be tilled.",
                ThumbnailSmall = "HUD_thumbnail_placeholder",
                EditorRenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = "plotLargePlowed_g"
                        }
                    },
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true, IsSpecialInterestFeature = true },
                DetectionTag = "farmSpotAndResourceDeposit", //needs Examine action
                IsSelectable = true,
                SharedSpecialActions = new [] { new Pair<string, bool>("establishLargePlot", true) }
            };
            listOfEntityTypes.Add(largePlotSpot);
                #endregion
            #endregion

            ////////////////
            /*
            listOfEntityTypes.Add(new EntityType("terrain:blackSmoke")
            {
                Name = "Black smoke",
                RenderableType = new RenderableType()
                {
                    ParticleEmitterTypes = new[] { new ParticleEmitterType() { ParticleSystemKey = "signalSmoke" } }

                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }

            });*/


            //marts 7, MP: there's a bug where flipped billboards don't have their geo flipped. (this goes for strucutres too, which is why flipping has been disabled on structures.)
            //for the terrain, I've tried to make the geo sorta symmetric so that it will still work when the billboard is flipped..
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall1", 2f, 12f, MakeRectangle(2, 2, 24, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall2", 2f, 30f, MakeRectangle(0, 2, 53, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall3", 1f, 12f, MakeRectangle(1, 4, 17, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall4", 2f, 30f, MakeRectangle(-1, 2, 35, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall5", 1f, 60f, MakeRectangle(-3, 5, 65, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall6", 1f, 5f, MakeRectangle(1, 2, 8, 4));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall7", 1f, 2f, MakeRectangle(-2, -3, 8, 4));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall8", 1.5f, 8f, MakeRectangle(-5, 2, 25, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall9", 2f, 30f, MakeRectangle(-3, 2, 27, 18));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks1", 2f, 18f, MakeRectangle(-1, 2, 19, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks2", 2f, 30f, MakeRectangle(3, -3, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks3", 2f, 50f, MakeRectangle(3, 2, 39, 26));
       //     AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks4", 3f, 100f, RockShape.Rectangle, null, 1.5f);

            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks4", 2f, 100f, MakeRectangle(-6, -1, 57, 24));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks5", 1.5f, 20f, MakeRectangle(-1, 1, 25, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks6", 2f, 12f, MakeRectangle(-1, 2, 23, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks7", 3f, 100f, MakeRectangle(-7, -2, 79, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks8", 2f, 30f, MakeRectangle(-3, 2, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks9", 2.5f, 120f, MakeRectangle(-1, 0, 91, 20));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks10", 1f, 10f, MakeRectangle(-1, 2, 19, 4));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae1", 2f, 18f, MakeRectangle(-1, 2, 19, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae2", 2f, 30f, MakeRectangle(3, -3, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae3", 2f, 50f, MakeRectangle(3, 2, 39, 26));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae4", 2f, 100f, MakeRectangle(-6, -1, 57, 24));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae5", 1.5f, 20f, MakeRectangle(-1, 1, 25, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae6", 2f, 12f, MakeRectangle(-1, 2, 23, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae7", 3f, 100f, MakeRectangle(-7, -2, 79, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae8", 2f, 30f, MakeRectangle(-3, 2, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae9", 2.5f, 120f, MakeRectangle(-1, 0, 91, 20));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae10", 1f, 10f, MakeRectangle(-1, 2, 19, 4));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks1", 1f, 50f, MakeRectangle(0, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks2", 2.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks3", 2f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks4", 3f, 30f, MakeRectangle(0, 2, 65, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks5", 1f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks6", 1f, 26f, MakeRectangle(0, 2, 31, 22));
            
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks1", 2f, 18f, MakeRectangle(-1, 2, 19, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks2", 2f, 30f, MakeRectangle(3, -3, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks3", 2f, 50f, MakeRectangle(3, 2, 39, 26));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks5", 1.5f, 20f, MakeRectangle(-1, 1, 25, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks6", 2f, 12f, MakeRectangle(-1, 2, 23, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks7", 3f, 100f, MakeRectangle(-7, -2, 79, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks8", 2f, 30f, MakeRectangle(-3, 2, 39, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks9", 2.5f, 120f, MakeRectangle(-1, 0, 91, 20));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks2", 2.5f, 20f, MakeRectangle(0, 2, 49, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks3", 2.5f, 28f, MakeRectangle(0, 2, 65, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks4", 2f, 14f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks5", 1f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks6", 1.5f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks7", 3f, 100f, MakeRectangle(7, 2, 79, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks8", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks9", 1.5f, 6f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks10", 2.5f, 18f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeMossrocks2", 2f, 50f, MakeRectangle(0, 2, 63, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeMossrocks4", 2f, 50f, MakeRectangle(-7, 2, 69, 20));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks1", 1f, 50f, MakeRectangle(0, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks2", 2.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks3", 2f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks4", 3f, 30f, MakeRectangle(0, 2, 65, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks5", 1f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks6", 1f, 26f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks1", 1.5f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks2", 2.5f, 20f, MakeRectangle(0, 2, 49, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks3", 2.5f, 28f, MakeRectangle(0, 2, 65, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks4", 2f, 14f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks5", 1f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks6", 1.5f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks7", 3f, 100f, MakeRectangle(7, 2, 79, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks8", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks9", 1.5f, 6f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks10", 2.5f, 18f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks11", 2f, 6f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks12", 1.5f, 20f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks1", 1f, 8f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks2", 1f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks3", 1.5f, 36f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks1", 3f, 26f, MakeRectangle(3, 5, 65, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks2", 2f, 50f, MakeRectangle(0, 2, 63, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks3", 2f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks4", 2f, 50f, MakeRectangle(-7, 2, 69, 20));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks5", 2f, 14f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks6", 2f, 22f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks7", 1f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks8", 2f, 160f, MakeRectangle(0, 4, 105, 30));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks1", 1f, 16f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks2", 1.5f, 28f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks3", 1f, 4f, MakeRectangle(0, 0, 17, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks4", 1.5f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks5", 1.5f, 9f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks1", 1f, 14f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks2", 1f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks3", 1.5f, 8f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks4", 1f, 12f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks1", 1.5f, 9f, MakeRectangle(0, 0, 8, 4));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks2", 3f, 70f, MakeRectangle(0, -6, 71, 4));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks3", 1f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks4", 1f, 42f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks5", 2f, 28f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks6", 1.5f, 28f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks7", 1f, 5f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks1", 2f, 20f, MakeRectangle(2, -5, 43, 20));
            AddRockType(listOfEntityTypes, "squarerocks2", 1f, 5f);
            AddRockType(listOfEntityTypes, "squarerocks3", 1f, 4f);
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks4", 2f, 40f, MakeRectangle(0, 2, 51, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks5", 2f, 30f, MakeRectangle(0, 4, 43, 20));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks6", 2f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks7", 2f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks8", 2f, 10f, MakeRectangle(0, 2, 17, 16));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks9", 1f, 42f, MakeRectangle(0, 3, 59, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks10", 3f, 40f, MakeRectangle(0, 2, 67, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks11", 2f, 44f, MakeRectangle(0, 2, 67, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks12", 2f, 22f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks13", 1.5f, 50f, MakeRectangle(5, 2, 63, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock1", 1f, 50f, MakeRectangle(0, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock2", 3f, 30f, MakeRectangle(0, 2, 65, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock3", 2f, 30f, MakeRectangle(0, 2, 53, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock4", 1f, 12f, MakeRectangle(1, 4, 17, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock5", 3f, 40f, MakeRectangle(0, 2, 67, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock6", 1f, 60f, MakeRectangle(-3, 5, 65, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock7", 1.5f, 8f, MakeRectangle(-5, 2, 25, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock8", 2f, 30f, MakeRectangle(-3, 2, 27, 18));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock9", 1f, 42f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock10", 2f, 28f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock11", 2f, 40f, MakeRectangle(0, 2, 51, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock12", 2f, 30f, MakeRectangle(0, 4, 43, 20));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock13", 2f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock14", 1f, 42f, MakeRectangle(0, 3, 59, 22));
            

            //marts 7, MP: there's a bug where flipped billboards don't have their geo flipped. (this goes for strucutres too, which is why flipping has been disabled on structures.)
            //for the terrain, I've tried to make the geo sorta symmetric so that it will still work when the billboard is flipped..

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutland", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-48, 99, 266, 36), MakeRectangleShape(119, 109, 83, 34), MakeRectangleShape(-19, 54, 200, 184), MakeRectangleShape(57, 45, 147, 164) } }); //once the flipgeo bug has been fixed, these are the coordinates to use: MakeRectangleShape(-48, 99, 266, 74), MakeRectangleShape(119, 109, 83, 34), MakeRectangleShape(-48, 53, 162, 202), MakeRectangleShape(57, 45, 147, 164)
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutlandEarth", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-48, 99, 266, 36), MakeRectangleShape(119, 109, 83, 34), MakeRectangleShape(-19, 54, 200, 184), MakeRectangleShape(57, 45, 147, 164) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutlandMuckroot", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-48, 99, 266, 36), MakeRectangleShape(119, 109, 83, 34), MakeRectangleShape(-19, 54, 200, 184), MakeRectangleShape(57, 45, 147, 164) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroe", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-14, 26, 210, 74), MakeRectangleShape(87, 34, 59, 38) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroeEarth", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-14, 26, 210, 74), MakeRectangleShape(87, 34, 59, 38) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroeMuckroot", 2f, 1000f, 
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-14, 26, 210, 74), MakeRectangleShape(87, 34, 59, 38) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillfaroe", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-14, 26, 210, 74), MakeRectangleShape(87, 34, 59, 38) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkney", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-60, 12, 165, 84), MakeRectangleShape(-18, 45, 119, 60), MakeRectangleShape(57, 2, 190, 30), MakeRectangleShape(73, 19, 81, 84) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkneyEarth", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-60, 12, 165, 84), MakeRectangleShape(-18, 45, 119, 60), MakeRectangleShape(57, 2, 190, 30), MakeRectangleShape(73, 19, 81, 84) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkneyMuckroot", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-60, 12, 165, 84), MakeRectangleShape(-18, 45, 119, 60), MakeRectangleShape(57, 2, 190, 30), MakeRectangleShape(73, 19, 81, 84) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillorkney", 2f, 1000f,
           new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-60, 12, 165, 84), MakeRectangleShape(-18, 45, 119, 60), MakeRectangleShape(57, 2, 190, 30), MakeRectangleShape(73, 19, 81, 84) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreece", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-122, 32, 163, 110), MakeRectangleShape(-12, 19, 211, 198), MakeRectangleShape(29, 5, 225, 174), MakeRectangleShape(150, 15, 163, 86) } });  //once the flipgeo bug has been fixed, these are the coordinates to use: MakeRectangleShape(-152, 32, 163, 110), MakeRectangleShape(149, -10, 163, 86), MakeRectangleShape(29, 2, 225, 174), MakeRectangleShape(-64, 21, 211, 198)
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreeceEarth", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-122, 32, 163, 110), MakeRectangleShape(-12, 19, 211, 198), MakeRectangleShape(29, 5, 225, 174), MakeRectangleShape(150, 15, 163, 86) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreeceMuckroot", 2f, 10000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-122, 32, 163, 110), MakeRectangleShape(-12, 19, 211, 198), MakeRectangleShape(29, 5, 225, 174), MakeRectangleShape(150, 15, 163, 86) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholm", 2f, 600f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-9, 6, 135, 60), MakeRectangleShape(11, 30, 160, 30) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholmEarth", 2f, 600f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-9, 6, 135, 60), MakeRectangleShape(11, 30, 160, 30) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholmMuckroot", 2f, 600f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-9, 6, 135, 60), MakeRectangleShape(11, 30, 160, 30) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillsaltholm", 2f, 600f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-9, 6, 135, 60), MakeRectangleShape(11, 30, 160, 30) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetland", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-11, 41, 213, 94), MakeRectangleShape(-1, -15, 163, 60), MakeCircleShape(6, -23, 70) } });  //MP marts 7: why are the circle coordinates not updated correctly in-game
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetlandEarth", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-11, 41, 213, 94), MakeRectangleShape(-1, -15, 163, 60), MakeCircleShape(6, -23, 70) } }); 
            AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetlandMuckroot", 2f, 1000f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[]{MakeRectangleShape(-11, 41, 213, 94),  MakeRectangleShape(-1, -15, 163, 60),  MakeCircleShape(6, -23, 70)}});
        

            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockLair", 2f, 100f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[]{MakeRectangleShape(-25, -18, 135, 38),  MakeRectangleShape(20, -3, 163, 34)}});
            AddRockGroundSpriteType(listOfEntityTypes, "rockLair_g"); //mp:  needs migration to AddRockTypeWithGeoLayout
            AddRockGroundSpriteType(listOfEntityTypes, "rockLairBones_g");//mp:  needs migration to AddRockTypeWithGeoLayout
            AddRockGroundSpriteType(listOfEntityTypes, "bones_g");//mp:  needs migration to AddRockTypeWithGeoLayout

            AddRockGroundSpriteType(listOfEntityTypes, "rockCreviceForRopeBridge_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "ropeBridge_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "rockCreviceMidSection_g", false);//mp:  needs migration to AddRockTypeWithGeoLayout?

            AddRockTypeWithGeoLayout(listOfEntityTypes, "rockCrevasse1", 2f, 100f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[]{MakeRectangleShape(-9, 0, 139, 48),  MakeRectangleShape(38, -24, 113, 20)}});

            AddRockGroundSpriteType(listOfEntityTypes, "candystalk_g", false);


            AddBillboardAnimTerrain(listOfEntityTypes, "Campfire", "campfireFast", "campfireFast"); //mp this was probably used as a quick test

            AddBillboardAnimTerrain(listOfEntityTypes, "Small campfire", "campfireSmall", "campfireSmall"); //mp this was probably used as a quick test

            AddBillboardAnimTerrain(listOfEntityTypes, "Fish circling", "fishCircling", "fishCircling");

            AddBillboardAnimTerrain(listOfEntityTypes, "Fish swarm", "fishSwarm", "fishSwarming");

            AddBillboardAnimTerrain(listOfEntityTypes, "Butterflies", "butterflies", "butterfliesSwarm");

            AddBillboardAnimTerrain(listOfEntityTypes, "Mosquito swarm", "mosquitoSwarm", "mosquitoSwarming");

            AddBillboardAnimTerrain(listOfEntityTypes, "Dragonfly", "dragonflies", "dragonflies");

            AddBillboardAnimTerrain(listOfEntityTypes, "Ground bugs", "groundBugs", "groundBugsSwarm");
           
          



//ret dem til en type med geolayout. den her har hardcoded terrain block:
            AddRockType(listOfEntityTypes, "holdenstreeFallenGrownDead1", 2f, 22f);
            AddRockType(listOfEntityTypes, "holdenstreeFallenGrownFresh1", 2f, 22f);
            AddRockType(listOfEntityTypes, "holdenstreeFallenGrownVines1", 2f, 22f);
            AddRockType(listOfEntityTypes, "holdenstreeFallenGrownNaked1", 2f, 22f);


            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerBig", 1f, 280f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerTall", 1f, 180f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall1", 1f, 18f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall2", 2f, 18f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall3", 1f, 8f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall4", 2f, 30f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall5", 1.5f, 26f, MakeRectangle(0, 2, 31, 22));

            AddRockGroundSpriteType(listOfEntityTypes, "utgardstower1_g");
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerHole_g", MakeRectangle(0, 2, 31, 22));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerHoleWeb_g", MakeRectangle(0, 2, 31, 22));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerPondWeb_g", MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "japanesegarden", 1f, 30f, MakeRectangle(15, 2, 47, 34));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "japanesegarden_g", MakeRectangle(-35, 2, 22, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "doghouse", 1.5f, 12f, MakeRectangle(0, 2, 22, 22));
            AddRockGroundSpriteType(listOfEntityTypes, "doghouse_g", false);

            AddRockTypeWithGeoLayout(listOfEntityTypes, "clothesLine", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "rabbitcages", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "orchard", 2f, 12f, MakeRectangle(0, -5, 31, 22));

            AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceDiagonal", 2f, 10f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-18, 14, 20, 20), MakeRectangleShape(-5, 2, 20, 20), MakeRectangleShape(8, -13, 20, 20), MakeRectangleShape(18, -19, 20, 18) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceDiagonalFlipped", 2f, 10f, //required asset because flipping terrainbillboard does not flip collidershapes (old bug)
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(-20, -20, 20, 20), MakeRectangleShape(-6, -4, 20, 20), MakeRectangleShape(9, 5, 20, 20) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceVertical", 2f, 10f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(0, 0, 20, 58) } });
            AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceHorizontal", 2f, 10f,
            new GeometryLayoutType() { Shapes = new CollideShape2D[] { MakeRectangleShape(0, 0, 108, 20) } });

            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlat", 2f, 40f, MakeRectangle(0, -1, 73, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlatFiregrass", 2f, 40f, MakeRectangle(0, -1, 73, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlatCave", 2f, 40f, MakeRectangle(0, -1, 73, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretower", 1.5f, 50f, MakeRectangle(0, -7, 81, 60));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerFiregrass", 1.5f, 50f, MakeRectangle(0, -7, 81, 60));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerVines", 1.5f, 50f, MakeRectangle(0, -7, 81, 60));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerGuano", 1.5f, 50f, MakeRectangle(0, -7, 81, 60));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneChubbytower", 1f, 36f, MakeRectangle(0, -4, 55, 44));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneChubbytowerFiregrass", 1f, 36f, MakeRectangle(0, -4, 55, 44));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneCube", 1f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneCubeFiregrass", 1f, 20f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouse", 1f, 30f, MakeRectangle(0, 2, 47, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseFiregrass", 1f, 30f, MakeRectangle(0, 2, 47, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseCave", 1f, 30f, MakeRectangle(0, 2, 47, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseGuano", 1f, 30f, MakeRectangle(0, 2, 47, 34));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNugget", 1.5f, 28f, MakeRectangle(-2, -3, 45, 26));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNuggetFiregrass", 1.5f, 28f, MakeRectangle(-2, -3, 45, 26));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNuggetGuano", 1.5f, 28f, MakeRectangle(-2, -3, 45, 26));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtower", 1f, 50f, MakeRectangle(0, -6, 47, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtowerFiregrass", 1f, 50f, MakeRectangle(0, -6, 47, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtowerCave", 1f, 50f, MakeRectangle(0, -6, 47, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall1", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall1Firegrass", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneGuano1", 1.5f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall2", 2f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall2Firegrass", 2f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall3", 2f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall3Firegrass", 2f, 12f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall4", 1f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall4Firegrass", 1f, 10f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall5", 1f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall5Firegrass", 1f, 5f, MakeRectangle(0, 2, 31, 22));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittower", 2f, 110f, MakeRectangle(0, -8, 115, 50));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittowerFiregrass", 2f, 110f, MakeRectangle(0, -8, 115, 50));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittowerGuano", 2f, 110f, MakeRectangle(0, -8, 115, 50));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerrace", 2f, 30f, MakeRectangle(4, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceFiregrass", 2f, 30f, MakeRectangle(4, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceVines", 2f, 30f, MakeRectangle(4, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceGuano", 2f, 30f, MakeRectangle(4, 2, 51, 32));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneWalltower", 2f, 80f, MakeRectangle(-3, -9, 109, 46));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneWalltowerFiregrass", 2f, 80f, MakeRectangle(-3, -9, 109, 46));



            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauOmelette", 1f, 800f, MakeRectangle(10, 9, 269, 202));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauPizza", 1f, 1000f, MakeRectangle(-10, 2, 329, 188));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauTortilla", 1.5f, 600f, MakeRectangle(-8, 2, 227, 138));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCake", 1f, 700f, MakeRectangle(-16, 17, 255, 124));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCaveVines", 1f, 700f, MakeRectangle(11, 10, 243, 134));
            AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCakeBare", 1f, 700f, MakeRectangle(-16, 17, 255, 124));

            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "sandstoneCrevasseZigzag_g", MakeRectangle(0, 0, 150, 44));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "sandstoneCrevasseStraight_g", MakeRectangle(0, -2, 108, 20));



            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks6_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks7_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks8_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks9_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone6_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone7_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone8_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone9_g", false);



            AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "brambleRootsSmall_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "vines1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "vines1Bare_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "vines2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "vines2Bare_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "vines3Bare_g", false);

            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "holeSoil_g", MakeRectangle(0, 2, 31, 22));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "holeSoilSmall_g", MakeRectangle(0, 2, 31, 22));


            AddRockGroundSpriteType(listOfEntityTypes, "earthpatchLong_g");
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "demonring_g", MakeRectangle(-3, -1, 107, 68));


            AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub5_g", false);

            //MP: the below have been converted to similar looking resource sprites. the old ones are still present in older unused maps, because i didn't know how to efficiently remove so much data from the map xml's
            #region goodie sprites not used
            /* 
        * AddRockGroundSpriteType(listOfEntityTypes, "goodiedonut1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiedonut2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiedonut3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiedonut4_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "goodietriangle1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodietriangle2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodietriangle3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodietriangle4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodietriangle5_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches6_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "goodiebranches7_g", false);
*/
            #endregion

            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead6_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale6_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "moss1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "moss2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "moss3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "moss4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "moss5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "moss6_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "mossDesert6_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "sealilies1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "sealiliesDead1_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "daffodils1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "daffodils2_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "thicketbare1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbare2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbare3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbare4_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "thicketflower1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketflower2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketflower3_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert4_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert3_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms3_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "waterlettuce1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "waterlettuce2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "waterlettuceDead1_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "pondfoil1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "pondfoil2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "pondfoilDead1_g", false);

            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "marshcotStem1_g", MakeRectangle(0, 2, 31, 22));
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "marshcotStemDead1_g", MakeRectangle(0, 2, 31, 22));

            AddRockGroundSpriteType(listOfEntityTypes, "ditchHorizontal1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "ditchVertical1_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "footpathfork_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathcenter_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathsouth_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "footpathDiagonal_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathHorizontalLong_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathHorizontalShort_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathVerticalLong_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathVerticalShort_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy2_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy3_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy4_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy5_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvyThin1_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "tilesPatch1_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "tilesStrip1_g", false);


            AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_diagonal_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_horizontal_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_vertical_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelES_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNE_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNSE_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNWE_g", false);

            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelSNE_g", false);
            AddRockGroundSpriteType(listOfEntityTypes, "roadGravelSWE_g", false);

            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerSmallAlmostInvisible_g", MakeRectangle(0, 0, 16, 16)); //pixelsize barely visible groundsprite  to block 1 subtile terrain in leveldesign
            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerSmall_g", MakeRectangle(0, 0, 16, 16)); //somewhat visible groundsprite to block 1 subtile terrain in leveldesign    

            AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerWide_g", MakeRectangle(0, 0, 150, 44)); //MP: invisible but the size of "sandstoneCrevasseZigzag_g"
            
        }

        private static EntityType AddBillboardAnimTerrain(List<EntityType> listOfEntityTypes, string name, string keyPostfix, string animationAsset = null)
        {
            EntityType animGroundBugs = new EntityType("terrain:" + keyPostfix /*groundBugs"*/)
            {
                Name = name,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                                    {                   
                                        new RenderAsBillboardType(){ AnimationAssetName = animationAsset /* "groundBugsSwarm"*/}
                                    }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },

            };
            listOfEntityTypes.Add(animGroundBugs);

            return animGroundBugs;
        }

        private static EntityType AddRockType(List<EntityType> listOfEntityTypes, string spriteName, float widthHeightRatio, float bulk, string animationAsset = null)
        {
            return AddRockType(listOfEntityTypes, "terrain:" + spriteName, spriteName, spriteName, widthHeightRatio, bulk, animationAsset);
        }

        /*
        private static EntityType AddRockType(List<EntityType> listOfEntityTypes, string spriteName, float bulk)
        {
            return AddRockType(listOfEntityTypes, "terrain:" + spriteName, spriteName, spriteName, 1f, bulk);
        }
        */

        private static EntityType AddRockType(List<EntityType> listOfEntityTypes, string keyName, string sprite, string name, float widthHeightRatio, float bulk, string animationAsset = null)
        {
            EntityType terrain = new EntityType(keyName)
            {
                Name = name,

                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = new GeometryLayoutType()
                    {
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 15)
                        }
                    }
                },
                RockType = new RockType() { Bulk = bulk }
            };

            if (animationAsset != null)
            {
                terrain.RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                   
                            new RenderAsBillboardType(){ AnimationAssetName = animationAsset, AspectRatio = widthHeightRatio }
                        }
                    }
                    };

                terrain.RockType.Animates = true;
            }
            else
            {
                terrain.RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                   
                            new RenderAsBillboardType(){ AssetName = sprite, AspectRatio = widthHeightRatio }
                        }
                    }
                };
            }

            listOfEntityTypes.Add(terrain);

            return terrain;
        }

        /*   private static EntityType AddRockTypeWithTileLayout(List<EntityType> listOfEntityTypes, string spriteName)
           {
               return AddRockTypeWithGeoLayout(listOfEntityTypes, "terrain:" + spriteName, spriteName, spriteName, 1f);
           }*/

        public static GeometryLayoutType MakeRectangle(float x, float y, float width, float height)
        {
            //note x and y are the coords of the center of the shape, not the upper left corner
            return new GeometryLayoutType()
                {
                    Shapes = new CollideShape2D[] 
                        {
                            //note, this ctor take top, left, bottom, right,
                            new CollideShape2D(-height*.5f, -width*.5f, height*.5f, width*.5f)
                            {
                                Offset = new Vector2(x,y)
                            }

                        }
                };

        }


        private static CollideShape2D MakeRectangleShape(float x, float y, float width, float height)
        {
            //note x and y are the coords of the center of the shape, not the upper left corner
            return new CollideShape2D(-height * .5f, -width * .5f, height * .5f, width * .5f)
                {
                    Offset = new Vector2(x, y)
                };


        }

        private static CollideShape2D MakeCircleShape(float x, float y, float radius)
        {
            return new CollideShape2D(new Vector2(x, y), radius); 
                                 
            

        }

        private static GeometryLayoutType MakeCircle(float x, float y, float radius)
        {
            return new GeometryLayoutType()
            {
                Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(new Vector2(x, y), radius) 

                        }
            };

        }

        private static void AddRockWithGeoLayout()
        {

        }


        private static EntityType AddRockTypeWithGeoLayout(List<EntityType> listOfEntityTypes, string keyName,float widthHeightRatio, float bulk, GeometryLayoutType geoType)
        {
            string sprite = keyName;
            string name = keyName;
 
            EntityType terrain = AddRockTypeWithoutLayout(listOfEntityTypes, "terrain:"+keyName, sprite, name, widthHeightRatio, bulk);

            terrain.DefaultSimState = new SimStateInfo()
            {
                GeometryLayoutType = geoType
            };

            return terrain;

        }

       

        private static EntityType AddRockTypeWithoutLayout(List<EntityType> listOfEntityTypes, string keyName, string sprite, string name, float widthHeightRatio, float bulk)
        {
            EntityType terrain = new EntityType(keyName)
            {
                Name = name,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                        {                   
                            new RenderAsBillboardType(){ AssetName = sprite, AspectRatio = widthHeightRatio }
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },

                RockType = new RockType() { Bulk = bulk }
            };
            listOfEntityTypes.Add(terrain);
            return terrain;
        }

        private static EntityType AddRockGroundSpriteType(List<EntityType> listOfEntityTypes, string sprite, bool hasPointLayout = true)
        {
            EntityType terrain = new EntityType("terrain:" + sprite)
            {
                Name = sprite,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = sprite
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true }/*,
                PointLayoutType = new PointLayoutType() { }*/
            };

            if (hasPointLayout)
            {
                terrain.PointLayoutType = new PointLayoutType();
            }

            listOfEntityTypes.Add(terrain);

            return terrain;
        }

        private static EntityType AddRockGroundSpriteTypeWithGeoLayout(List<EntityType> listOfEntityTypes, string sprite, GeometryLayoutType geoType)
        {
            EntityType terrain = new EntityType("terrain:" + sprite)
            {
                Name = sprite,
                RenderableType = new RenderableType()
                {
                    DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsGroundSpriteType = new RenderAsGroundSpriteType()
                        {
                            AssetName = sprite
                        }
                    }
                },
                TerrainType = new TerrainFeatureType() { CanBeMapEditorPlaced = true },
                DefaultSimState = new SimStateInfo()
                {
                    GeometryLayoutType = geoType
                }
            };
            listOfEntityTypes.Add(terrain);

            return terrain;
        }




    }
}

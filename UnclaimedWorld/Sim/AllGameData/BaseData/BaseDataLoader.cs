using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Items;
using System.IO;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Soil;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using System.Reflection;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Resources;
using System.Collections;
using UWGame.SimSide.Processes;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide.Renderables;
using UWGame;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.IngameEvents;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide;

using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.Client.Audio;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.AllGameData.EventHooks;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.Layout;
using UWGame.SimSide.Combat;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Jobs.JobTypes;
using WindowSystem;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trade;
using UWGame.ClientSide.Hints;
/*
 Serialization - IMPORTANT NOTES:
 
 * - don't serialize interface lists or classes which use inheritance. It makes the serializer throw up.
 *  rework the designs to use composition instead (like the ResourceType or BodyPart classes)
 *  
 * - Since XmlSerializer will deserialize a List that is null as an empty list, we have to use an array instead if we want to detect behaviours with " != null"!
    See RenderAsBillboardType.
 * 
 * - when there are arrays of IGameDataType (like BodyPartType[]), it is not enough to create a serializer for IGameDataTypes. specific methods are needed to handle arrays of these types.
 * see EntityType serializer methods.
 * 
 * 
 *  Other tips:
 * - How to suppress nullable types when they are null (makes the xml look cleaner):
 * public bool ShouldSerializeAmount() 
 * {
        return Amount != null;
   }
 * 
 * 
 */

namespace UWGame.SimSide.AllGameData
{
    /// <summary>
    /// loads all vanilla game data
    /// also has utility routines for loading game data
    /// </summary>
    public class BaseDataLoader: DataLoader
    {



        public BaseDataLoader()
            : base(Config.DataType.BaseData, 1f)
        {
        }

        protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
        }

        #region renderable items
        protected override List<RenderableType> InitAttachableRenderableTypes() 
        {


            List<RenderableType> listOfRenderableTypes = new List<RenderableType>();

           
                RenderableType renderableType = new RenderableType("box")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "box",
                        ModelScale = 1f,
                    }
                };
                listOfRenderableTypes.Add(renderableType);


                renderableType = new RenderableType("hammer")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "hammer",
                        ModelScale = 1f //3f //1.5f,
                    }
                };

                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("backpack")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "backpack",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);


                renderableType = new RenderableType("backpackHeavy")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "backpackHeavy",
                        ModelScale = 1f //3f //1.5f,
                    }
                };

                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("farmingHoe")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "farmingHoe",
                        ModelScale = 1f //3f //1.5f,
                    }

                };


                listOfRenderableTypes.Add(renderableType);


                renderableType = new RenderableType("rifle")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "rifle",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("spear")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "spear",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("machete")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "machete",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("pickaxe")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "pickaxe",
                        ModelScale = 1f 
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("axe")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "axe",
                        ModelScale = 1f
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("shovel")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "shovel",
                        ModelScale = 1f
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("knife")
                {

                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "knife",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("armsling")
                {
                    RenderAsModelType = new RenderAsModelType()
                     {
                         AssetName = "armsling",
                         ModelScale = 1f //3f //1.5f,
                     }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("watergun")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "watergun",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("watergunTank")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "watergunTank",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("bow")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "bow",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("tablet")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "tablet",
                        ModelScale = 1f //3f //1.5f,
                    }

                };
                listOfRenderableTypes.Add(renderableType);

                renderableType = new RenderableType("bush")
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "bush",
                        ModelScale = 1f,
                    }
                };
                listOfRenderableTypes.Add(renderableType);

                return listOfRenderableTypes;

          //  SerializeAndDeserializeTypeList(listOfRenderableTypes, GameData.Instance.AttachableRenderableTypes, "", "attachableObjects.xml", Config.DataType.BaseData);
   
        }
#endregion


        #region vehicles
        private static void InitMiscEntityTypes(List<EntityType> listOfEntityTypes)
        {       

            
            EntityType vehicleType;
            Entrance driverEntrance = new Entrance()
                {
                     Offset = new Vector2(2f, -20f),
                     ExitDoor = ExitDoor.Door1
                };
            Entrance passengerEntrance = new Entrance()
                {
                     Offset = new Vector2(2f, 20f),
                     ExitDoor = ExitDoor.Door2
                };
             Entrance cargoEntrance = new Entrance()
                {
                    Offset = new Vector2(-24f, 0f),
                    ExitDoor = ExitDoor.Door3
                };  
                

            vehicleType = new EntityType("entity:utilityvehicle")
            {
                Name = "Mule",
                FormalName = "",
                SummaryDescription = "All-terrain utility vehicle",              
                ThumbnailBig = "skimmerDark",
                              
                RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "utilityvehicle",
                       ModelScale = 2f,
                   }
               },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = MathHelper.Pi / 3f,

                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                        WalkSlowSpeed = 55f, //30f, //21f,
                        WalkNormalSpeed = 55f, //30f, // 260f,
                        WalkFastSpeed = 70f, //40f, // 63f,
                    }
                },
                            
                ContainerType = new VehicleContainerType(4f)
                    {
                        UnladenWeight = 40f,
                        LoadingRadius = 60f,
                        // MaxPassengers = 3,
                        Transport = SurfaceType.TransportType.OffRoad,
                        MainFunction = VehicleContainerType.Function.Hauling,
                        MaxAcceleration = 30f,
                        Deceleration = 0.9f,
                        AverageOverlandTravelSpeed = 600f,
                        RequiresReplenishType = new RequiresReplenishType()
                        {
                            /*RequiresPowerType = new RequiresPowerType()
                            {
                                PowerCellCapacity = 4
                            }*/
                        },

                        PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(4f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="FrontSeat", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoLeft", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, -12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoRight", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }}
                    
                        }

                   
                },
                BodyType = GameData.Instance.AllBodyTypes["mulevehicle"]

            };
            listOfEntityTypes.Add(vehicleType);

            #region Barge
            
            vehicleType = new EntityType("entity:barge")
            {
                Name = "Barge",
                SummaryDescription = "Slow-moving river boat",
                ThumbnailBig = "skimmerDark",
                WorldMapIcon = "barge_map_icon", //was "sailboat_map_icon"
                // mp the below is important feedback so the player can see on the map when the boat will arrive.
                CommunicatorType = new Communication.CommunicatorType()
                {
                    Method = Communication.CommunicationMethod.Radio
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
                //
                RenderableType = new RenderableType()
                {
                   /* RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "skimmer",
                        ModelScale = 1.3f,
                    }*/
                },
              /*  LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = MathHelper.Pi / 6f,

                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                        // for use in estimation???
                        WalkSlowSpeed = 21f,
                        WalkNormalSpeed = 20f, //280f,
                        WalkFastSpeed = 25f, //63f,
                    }
                },*/

                ContainerType = new VehicleContainerType(60f)
                {
                    VehicleType = VehicleContainerType.VehicleTypes.Boat,
                    CanUseTerminal = TerminalType.TypesOfTerminal.Pier,
                    
                    CanNavigateRoutes = new RouteType[]{ RouteType.CalmWater },

                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee cargo

                    // not used:///////////////////////////////////////////////
                    UnladenWeight = 40f,
                    LoadingRadius = 240f,
                    Transport = SurfaceType.TransportType.Foot,
                    MaxAcceleration = 50f, //30f,
                    MainFunction = VehicleContainerType.Function.Hauling,
                    AverageOverlandTravelSpeed = 270f, //mp sep 2016 was 200f
                    
                    PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(4f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot1", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot2", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot3", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot4", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot5", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot6", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot7", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},                      
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot8", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoLeft", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, -12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoRight", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo1", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo2", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 15f }}
                    
                        },
                    
                    Deceleration = 1.3f, //0.9f,  
                }               

            };
            listOfEntityTypes.Add(vehicleType);
            #endregion

            #region Advanced Barge

            vehicleType = new EntityType("entity:advancedBarge")
            {
                Name = "Barge",
                SummaryDescription = "Slow-moving river boat",
                ThumbnailBig = "skimmerDark",
                WorldMapIcon = "barge_map_icon", //was "sailboat_map_icon"
                // mp the below is important feedback so the player can see on the map when the boat will arrive.
                CommunicatorType = new Communication.CommunicatorType()
                {
                    Method = Communication.CommunicationMethod.Satellite
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
                    CanExamine = false, 
                    CanScout = false,
                    CanPatrol = false,
                    CanHaul = false,
                    CanDoJobs = false,
                    ServantForEntityTypeTag = "servesHumans",
                },
                //
                RenderableType = new RenderableType()
                {
                    /* RenderAsModelType = new RenderAsModelType()
                     {
                         AssetName = "skimmer",
                         ModelScale = 1.3f,
                     }*/
                },
                /*  LocomotorType = new LocomotorType()
                  {
                      MaxAngularSpeed = MathHelper.Pi / 6f,

                      LeggedLocomotorType = new LeggedLocomotorType()
                      {
                          // for use in estimation???
                          WalkSlowSpeed = 21f,
                          WalkNormalSpeed = 20f, //280f,
                          WalkFastSpeed = 25f, //63f,
                      }
                  },*/

                ContainerType = new VehicleContainerType(60f)
                {
                    VehicleType = VehicleContainerType.VehicleTypes.Boat,
                    CanUseTerminal = TerminalType.TypesOfTerminal.Pier,

                    CanNavigateRoutes = new RouteType[] { RouteType.CalmWater },

                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee cargo

                    // not used:///////////////////////////////////////////////
                    UnladenWeight = 40f,
                    LoadingRadius = 240f,
                    Transport = SurfaceType.TransportType.Foot,
                    MaxAcceleration = 50f, //30f,
                    MainFunction = VehicleContainerType.Function.Hauling,
                    AverageOverlandTravelSpeed = 270f, //mp sep 2016 was 200f

                    PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(4f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot1", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot2", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot3", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot4", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot5", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot6", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot7", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},                      
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot8", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoLeft", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, -12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoRight", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo1", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 15f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo2", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 15f }}
                    
                        },

                    Deceleration = 1.3f, //0.9f,  
                }

            };
            listOfEntityTypes.Add(vehicleType);
            #endregion

            #region Small Barge

            vehicleType = new EntityType("entity:smallBarge")
            {
                Name = "Small barge", //for the clay pit scenario. can only carry 40 mudbricks. 40 X 0.75f = 30f bulk
                SummaryDescription = "Slow-moving river boat",
                ThumbnailBig = "skimmerDark",
                WorldMapIcon = "barge_map_icon", //was "sailboat_map_icon"
                // mp the below is important feedback so the player can see on the map when the boat will arrive.
                CommunicatorType = new Communication.CommunicatorType()
                {
                    Method = Communication.CommunicationMethod.Radio
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
                //
                RenderableType = new RenderableType()
                {
                    /* RenderAsModelType = new RenderAsModelType()
                     {
                         AssetName = "skimmer",
                         ModelScale = 1.3f,
                     }*/
                },
                /*  LocomotorType = new LocomotorType()
                  {
                      MaxAngularSpeed = MathHelper.Pi / 6f,

                      LeggedLocomotorType = new LeggedLocomotorType()
                      {
                          // for use in estimation???
                          WalkSlowSpeed = 21f,
                          WalkNormalSpeed = 20f, //280f,
                          WalkFastSpeed = 25f, //63f,
                      }
                  },*/

                ContainerType = new VehicleContainerType(40f) //for the clay pit scenario. mp increased it again because this calculation doesnt take into account that the items yo BUY come on top of the items you sell. so it needs extra room. didnt work:   can only carry 40 mudbricks. 40 X 0.75f = 30f bulk. Everything else is the same as Barge
                {
                    VehicleType = VehicleContainerType.VehicleTypes.Boat,
                    CanUseTerminal = TerminalType.TypesOfTerminal.Pier,

                    CanNavigateRoutes = new RouteType[] { RouteType.CalmWater },

                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee cargo

                    // not used:///////////////////////////////////////////////
                    UnladenWeight = 40f,
                    LoadingRadius = 240f,
                    Transport = SurfaceType.TransportType.Foot,
                    MaxAcceleration = 50f, //30f,
                    MainFunction = VehicleContainerType.Function.Hauling,
                    AverageOverlandTravelSpeed = 200f,//mp sep 2016 didnt reduce this because only used on clay pit scenario

                    PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(4f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot1", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot2", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot3", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot4", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot5", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot6", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot7", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},                      
                        new PassengerOrCargoSlotType(){ AttachPointName="PassengerSpot8", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(4f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoLeft", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, -12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 10f }}, //mp changed so that it adds up to 30f above. ContainerType = new VehicleContainerType(30f) 
                        new PassengerOrCargoSlotType(){ AttachPointName="CargoRight", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ Capacity = 10f }},//mp changed so that it adds up to 30f above. ContainerType = new VehicleContainerType(30f)
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo1", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 10f }},//mp changed so that it adds up to 30f above. ContainerType = new VehicleContainerType(30f)
                        new PassengerOrCargoSlotType(){ AttachPointName="Cargo2", Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-12f, 12f), 
                             CargoSlotType = new CargoSlotType(){ Capacity = 10f }}//mp changed so that it adds up to 30f above. ContainerType = new VehicleContainerType(30f)
                    
                        },

                    Deceleration = 1.3f, //0.9f,  
                }

            };
            listOfEntityTypes.Add(vehicleType);
            #endregion

            #region Skimmer
            driverEntrance = new Entrance()
            {
                Offset = new Vector2(12f, -24f),
                ExitDoor = ExitDoor.Door1
            };
            passengerEntrance = new Entrance()
            {
                Offset = new Vector2(12f, 24f),
                ExitDoor = ExitDoor.Door2
            };
            cargoEntrance = new Entrance()
            {
                Offset = new Vector2(-40f, 0f),
                ExitDoor = ExitDoor.Door3
            }; 
            vehicleType = new EntityType("entity:skimmer")
            {
                Name = "Skimmer",
                FormalName = "SK-40 'Skimmer'",
                SummaryDescription = "VTOL utility aircraft",               
                ThumbnailBig = "skimmerDark",
                WorldMapIcon = "skimmer_map_icon",
                 CommunicatorType = new Communication.CommunicatorType()
                 {
                     Method = Communication.CommunicationMethod.Satellite
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
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "skimmer",
                       ModelScale = 1.3f
                   }
               },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = MathHelper.Pi / 6f,

                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                        // for use in estimation???
                        WalkSlowSpeed = 21f,
                        WalkNormalSpeed = 20f, //280f,
                        WalkFastSpeed = 25f, //63f,
                    }

                },
                ContainerType = new VehicleContainerType(8f)
                    {
                        VehicleType = VehicleContainerType.VehicleTypes.Aircraft,

                        UnladenWeight = 40f,
                        LoadingRadius = 240f,
                        Transport = SurfaceType.TransportType.Air,
                        MaxAcceleration = 50f, //30f,
                        MainFunction = VehicleContainerType.Function.Hauling,
                        AverageOverlandTravelSpeed =  3000f, //30000f, //
                        Aircraft = new AircraftType()
                        {
                            MaxAirSpeed = 180f,
                            MaxRollDegreeWhenTurning = 0.7f,
                            MaxVerticalAcceleration = 30f,
                            MaxVerticalMoveSpeed = 90f,
                            MaxPitchInRadians = MathHelper.Pi / 12f,
                            PitchChangeSpeed = MathHelper.Pi / 6f,
                            DuctChangeAngleSpeed = MathHelper.PiOver2,
                            MaxPropellerSpeed = 30f,
                            PropellerAcceleration = 4f
                        },
                        CanUseTerminal = TerminalType.TypesOfTerminal.Helipad,
                         RequiresReplenishType = new RequiresReplenishType()
                         {
                             /*RequiresPowerType = new RequiresPowerType()
                             {
                                 PowerCellCapacity = 12
                             }*/
                         },
                        Deceleration = 1.3f, //0.9f,

                        PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(12f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="FrontSeat", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(12f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                       
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }},
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }},
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            CargoSlotType = new CargoSlotType(){ Capacity = 4f }  }                     
                    
                    }


                    
                },
                NonLivingType = new NonLivingType()
                {
                    PartKeys = new SerializableDictionary<string, int>() { { "item:skimmerHull", 1}, { "item:skimmerLandingGear", 1},
                        { "item:skimmerWing", 2 }, { "item:skimmerCanopy", 1 }, { "item:skimmerMotor", 2 }, { "item:skimmerRotor", 2 }}
                }               

            };
            listOfEntityTypes.Add(vehicleType);
            #endregion

            #region Harpy
            driverEntrance = new Entrance()
            {
                Offset = new Vector2(12f, -24f),
                ExitDoor = ExitDoor.Door1
            };
            passengerEntrance = new Entrance()
            {
                Offset = new Vector2(12f, 24f),
                ExitDoor = ExitDoor.Door2
            };
            cargoEntrance = new Entrance()
            {
                Offset = new Vector2(-40f, 0f),
                ExitDoor = ExitDoor.Door3
            }; 
            vehicleType = new EntityType("entity:harpy")
            {
                Name = "Harpy",
                FormalName = "ML-40 'Harpy'",
                SummaryDescription = "Medium-lift VTOL aircraft",               
                ThumbnailBig = "skimmerDark",
                WorldMapIcon = "skimmer_map_icon",
                
                CommunicatorType = new Communication.CommunicatorType()
                {
                    Method = Communication.CommunicationMethod.Satellite
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
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "skimmer",
                        ModelScale = 1.3f,

                    }
                },
                LocomotorType = new LocomotorType()
                {
                    MaxAngularSpeed = MathHelper.Pi / 6f,

                    LeggedLocomotorType = new LeggedLocomotorType()
                    {
                        // for use in estimation???
                        WalkSlowSpeed = 21f,
                        WalkNormalSpeed = 20f, //280f,
                        WalkFastSpeed = 25f, //63f,
                    }

                },


                ContainerType = new VehicleContainerType(14f)
                    {
                        VehicleType = VehicleContainerType.VehicleTypes.Aircraft,
                        UnladenWeight = 40f,
                        LoadingRadius = 240f,
                        Transport = SurfaceType.TransportType.Air,
                        MaxAcceleration = 50f, //30f,
                        MainFunction = VehicleContainerType.Function.Hauling,
                        AverageOverlandTravelSpeed = 2400f, // 30000f, // 3000f,
                        Aircraft = new AircraftType()
                        {
                            MaxAirSpeed = 180f,
                            MaxRollDegreeWhenTurning = 0.7f,
                            MaxVerticalAcceleration = 30f,
                            MaxVerticalMoveSpeed = 90f,
                            MaxPitchInRadians = MathHelper.Pi / 12f,
                            PitchChangeSpeed = MathHelper.Pi / 6f,
                            DuctChangeAngleSpeed = MathHelper.PiOver2,
                            MaxPropellerSpeed = 30f,
                            PropellerAcceleration = 4f
                        },
                        CanUseTerminal = TerminalType.TypesOfTerminal.Helipad,
                      
                        Deceleration = 1.3f, //0.9f,
                        
                        PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[]{ 
                        new PassengerOrCargoSlotType(){ AttachPointName="DriversSeat", Entrance = driverEntrance, PointToFaceAtEntrance = new Vector2(12f, -12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = true, Comfort = 1f }},
                        new PassengerOrCargoSlotType(){ AttachPointName="FrontSeat", Entrance = passengerEntrance, PointToFaceAtEntrance = new Vector2(12f, 12f), PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 1f }},
                       
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }},
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            PassengerSlotType = new PassengerSlotType(){ IsDriversSeat = false, Comfort = 0.5f }
                            , CargoSlotType = new CargoSlotType(){ }},
                        new PassengerOrCargoSlotType(){  Entrance = cargoEntrance, PointToFaceAtEntrance = new Vector2(-15f, 0f), 
                            CargoSlotType = new CargoSlotType(){ Capacity = 10f }  }                     
                    
                    }


                    
                }
                /*,
                PartKeys = new SerializableDictionary<string, int>() { { "item:skimmerHull", 1}, { "item:skimmerLandingGear", 1},
                        { "item:skimmerWing", 2 }, { "item:skimmerCanopy", 1 }, { "item:skimmerMotor", 2 }, { "item:skimmerRotor", 2 }}*/

            };
            listOfEntityTypes.Add(vehicleType);
            #endregion

        }
         #endregion

        // None = 0, VeryLow = 1, Low = 3, Middle = 5, High = 7, Highest = 10
        public const float NoDefenseRating = 0.0f;
        public const float VeryLowDefenseRating = 0.05f;
        public const float UnderLowDefenseRating = 0.10f;//new
        public const float LowDefenseRating = 0.15f;
    //    public const float UnderMiddleDefenseRating = 0.20f;//new
        public const float MiddleDefenseRating = 0.25f;
        public const float AboveMiddleDefenseRating = 0.30f;//new
        public const float HighDefenseRating = 0.55f; //was 0.40f   was 0.35
        public const float HighestDefenseRating = 0.80f; //was 0.55f  was 0.50

        protected override UWGame.SimSide.Constants InitGameConstants()
        {
            return new UWGame.SimSide.Constants();          

        }

        protected override AI.Constants.AIConstants InitAIConstants()
        {
            return new AI.Constants.AIConstants();            

        }

        protected override GUIConstants InitGUIConstants()
        {
            return new GUIConstants();

        }

       /*  #region Agent Actions              

       protected override List<AgentAction> InitAgentActions()
        {
            List<AgentAction> list = new List<AgentAction>();
           
                list.Add(new AgentAction("lightFire") //mp todo: make one specially for improvised kitchen where the particlesystem is displaced to the right, so that we do not need to use the default position center of ground sprite. that way we can make the fire position asymmetrical and avoid a huge ground sprite with dead space to the right.
                {
                    Name = "Light fire",
                    StatusDescription = "Lighting fire",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending, //make one for field kitchen where agent is standing up.
                    Stances = AllGameData.ProcessLoader.kneelingProduction, // new[]{ LeggedLocomotor.Stance.Kneeling },

                    ParticleEmittersWhenCompleted = new[]{ 
                        new ParticleEmitterEffect(){  ParticleSystemKey = "smallerSmoke",},//todo add offset (add a space and choose from options) and alter geometry of groundsprite for improvised kitchen 
                        new ParticleEmitterEffect(){  ParticleSystemKey = "smallFire" }, //todo add offset and geometry of groundsprite for improvised kitchen 
                 //       new ParticleEmitterEffect(){  ParticleSystemKey = "fireSparks" } ...Only for bonfire
                    }
      
                });

                list.Add(new AgentAction("kitchenImprovisedLightFire") //mp todo: make one specially for improvised kitchen where the particlesystem is displaced to the right, so that we do not need to use the default position center of ground sprite. that way we can make the fire position asymmetrical and avoid a huge ground sprite with dead space to the right.
                {
                    Name = "Light fire",
                    StatusDescription = "Lighting fire",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,

                    ParticleEmittersWhenCompleted = new[]{ 
                        new ParticleEmitterEffect(){  ParticleSystemKey = "smallerSmoke",Offset = new Vector2(21,-3)},
                        new ParticleEmitterEffect(){  ParticleSystemKey = "smallFire",Offset = new Vector2(21,-3) },
                    }

                });

                list.Add(new AgentAction("lightFireWithoutFlames")
                {
                    Name = "Light fire",
                    StatusDescription = "Lighting fire",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending,
                    Stances = AllGameData.ProcessLoader.kneelingProduction, //new[] { LeggedLocomotor.Stance.Kneeling },

                    ParticleEmittersWhenCompleted = new[]{ 
                        new ParticleEmitterEffect(){  ParticleSystemKey = "smallestSmoke"}
                    }

                });

                list.Add(new AgentAction("forgeSmoke") //mp ordinary grey smoke because it's charcoal 
                {
                    Name = "Light fire",
                    StatusDescription = "Lighting fire",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,

                    ParticleEmittersWhenCompleted = new[]
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
                        },
                    }

                });
                list.Add(new AgentAction("kilnSmoke") //mp ordinary grey smoke because it's charcoal 
                {
                    Name = "Light fire",
                    StatusDescription = "Lighting fire",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,

                    ParticleEmittersWhenCompleted = new[]
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
                        },
                    }

                });

                list.Add(new AgentAction("cookAtStove") //for portable field kitchen. no big smoke or flames, just steam.
                {
                    Name = "Cook at stove",
                    StatusDescription = "Cooking at stove",
                    TimeInDaysRequired = 0.002f,

                    AgentActionState = AnimAction.Mending, //mp feb 2015 we need a standing mend anim
                    Stances = AllGameData.ProcessLoader.kneelingProduction, //new[] { LeggedLocomotor.Stance.Kneeling },

                    ParticleEmittersWhenCompleted = new[]{ 
                        new ParticleEmitterEffect(){  ParticleSystemKey = "foodSteam"}
                    }

                });

                return list;
        }
        #endregion*/

        #region Soil Types
        protected override List<SoilComponentType> InitSoilComponentTypes()
        {
           
                List<SoilComponentType> listOfSoilComponentTypes = new List<SoilComponentType>();
               
                    SoilComponentType soil = new SoilComponentType("soil:groundrock")
                    {
                        Name = "Ground rock",
                        MoveFactor = 0f,
                        TextureName = "earth",
                        WetTint = new Color(240, 240, 240),
                        RenderWithPerlinNoise = RenderPerlinNoise.None,
                        IsBaseTerrain = true
                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:muckroot")
                    {
                        Name = "Muckroot",
                        TextureName = "muckroot",
                        MoveFactor = 0.2f,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed,
                        ScaleDisplayAmountAsWithRocks = true,
                        RenderAsRocksType = new RenderAsRocksType()
                        {
                            DepthMapTextureName = "linear gradient normal map"
                        }
                        // RenderPerlinNoiseSharpness = 1f // 8f                

                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:clay")
                    {
                        Name = "Clay",
                        MoveFactor = 0f,
                        TextureName = "earth",
                        DryTint = new Color(255, 240, 240),
                        WetTint = new Color(178, 175, 165), //new Color(178, 175, 165),(240, 240, 240),
                        RenderWithPerlinNoise = RenderPerlinNoise.None
                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:sand")
                    {
                        Name = "Sand",
                        MoveFactor = 0.1f,
                        TextureName = "sand",
                        WetTint = new Color(180, 180, 180), //new Color(160, 160, 160), new Color(240, 240, 240)
                        RenderWithPerlinNoise = RenderPerlinNoise.None /* RenderPerlinNoise.ChannelBlue // 
                    , RenderPerlinNoiseSharpness = 3, 
                    NoiseScaling = 2*/
                    };

                listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:vulcanic")
                    {
                        Name = "Vulcanic",
                        MoveFactor = 0.1f,
                        TextureName = "vulcanic",
                        WetTint = new Color(180, 180, 180), 
                        RenderWithPerlinNoise = RenderPerlinNoise.None 
                    };

                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:seabed")
                    {
                        Name = "Seabed",
                        TextureName = "seabed",
                        WetTint = new Color(180, 180, 180), //new Color(160, 160, 160), new Color(240, 240, 240)
                        RenderWithPerlinNoise = RenderPerlinNoise.None
                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:deepseabed")
                    {
                        Name = "Deep Seabed",
                        TextureName = "deepseabed",
                        WetTint = new Color(180, 180, 180), //new Color(160, 160, 160), new Color(240, 240, 240)
                        RenderWithPerlinNoise = RenderPerlinNoise.None
                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:humus")
                    {
                        // completely broken down organic matter that is better able to hold water and nutrients, and benefits aeration of the soil
                        Name = "Humus",
                        MoveFactor = 0f,
                        TextureName = "humus", // add this...
                        // DryTint = new Color(0, 0, 0),
                        //TextureName = "humus", // add this...
                        RenderWithPerlinNoise = RenderPerlinNoise.None
                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:rocks")
                    {
                        Name = "Rocks",
                        TextureName = "rocks",
                        MoveFactor = 0.5f,
                        WetTint = new Color(180, 180, 180),

                        //InvertNoise = true,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen, //Small, //
                        RenderPerlinNoiseSharpness = 8f, //20f,// //6f
                        NoiseScaling = 1f, //0.8f, // NoiseScaling < 1 + NoiseSharpness > 1 gives ugly artifacts!
                        ScaleDisplayAmountAsWithRocks = true,
                        RenderAsRocksType = new RenderAsRocksType()
                        {
                            DepthMapTextureName = "linear gradient normal map"
                        }

                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:rockssandstone")
                    {
                        Name = "SandstoneRocks",
                        TextureName = "rocks", //"rockssandstone",
                        MoveFactor = 0.35f,
                        DryTint = new Color(213, 178, 154),
                        WetTint = new Color(213, 178, 154),
                        //HasTransparency = true,
                        InvertNoise = true,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelBlue, //Small, //
                        RenderPerlinNoiseSharpness = 8f, //20f,// //6f
                        NoiseScaling = 1f, //0.8f,
                        ScaleDisplayAmountAsWithRocks = true,
                        RenderAsRocksType = new RenderAsRocksType()
                        {
                            DepthMapTextureName = "linear gradient normal map"

                        }

                    };
                    listOfSoilComponentTypes.Add(soil);
                    soil = new SoilComponentType("soil:limestone")
                    {
                        Name = "LimestoneRocks",
                        TextureName = "limestone",
                        MoveFactor = 0.35f,
                        WetTint = new Color(198, 198, 198),
                        //HasTransparency = true,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed, //Small, //
                        RenderPerlinNoiseSharpness = 8f, //20f,// //6f
                        NoiseScaling = 1f, //0.8f,
                        ScaleDisplayAmountAsWithRocks = true,
                        RenderAsRocksType = new RenderAsRocksType()
                        {
                            DepthMapTextureName = "linear gradient normal map"

                        }

                    };
                    listOfSoilComponentTypes.Add(soil);
              
            

            // assign rendering order:
            //RenderedTerrainType.HighestOrder = Math.Max(RenderedTerrainType.HighestOrder, GameData.Instance.AllLowVegetationTypes.Count);
        /*    foreach (KeyValuePair<string, SoilComponentType> kvp in GameData.Instance.AllSoilComponentTypes)
            {
                if (kvp.Value.RenderAsRocksType == null)
                {
                    kvp.Value.RenderOrder = RenderedTerrainType.HighestOrder;
                    RenderedTerrainType.HighestOrder++;
                }
                else
                {
                    // kvp.Value.RenderOrder = 0; // don't care
                    kvp.Value.RenderOrder = RenderedTerrainType.HighestOrder + 100;
                    RenderedTerrainType.HighestOrder++;
                }
            }*/

            return listOfSoilComponentTypes;

        }
        #endregion


        #region Low Vegetation Types
        protected override List<LowVegetationType> InitLowVegetationTypes()
        {
           
                List<LowVegetationType> listOfVegTypes = new List<LowVegetationType>();
                
                    LowVegetationType veg = new LowVegetationType("veg:muckrootthin")
                    {
                        Name = "Thin muckroot", // thick muckroot is hacked in under "soil" 
                        TextureName = "muckrootthin",
                        MoveFactor = 0.1f,
                        InvertNoise = true,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen
                        //RenderPerlinNoiseSharpness = 8f                
                    };
                    listOfVegTypes.Add(veg);
                    veg = new LowVegetationType("veg:firegrass")
                    {
                        Name = "Firegrass",
                        TextureName = "firegrass",
                        MoveFactor = 0.07f,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen
                    };
                    listOfVegTypes.Add(veg);
                    veg = new LowVegetationType("veg:greengrass") //
                    {
                        Name = "Grass",
                        TextureName = "greengrass", //
                        MoveFactor = 0f,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed //
                    };
                    listOfVegTypes.Add(veg);
                 /*   veg = new LowVegetationType("veg:blueroot")
                    {
                        Name = "Blueroot",
                        TextureName = "blueroot",
                        MoveFactor = 0.1f,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed
                    };
                    listOfVegTypes.Add(veg);*/
                    veg = new LowVegetationType("veg:billowgrass")
                    {
                        Name = "Billowgrass",
                        CanGrowUnderWater = true,
                        TextureName = "billowgrass",
                        MoveFactor = 0.1f,
                        RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed
                    };
                    listOfVegTypes.Add(veg);
                

             //   SerializeAndDeserializeTypeList(listOfVegTypes, GameData.Instance.AllLowVegetationTypes, "", "vegetation.xml", Config.DataType.BaseData);

          

            // assign rendering order:      
         
            foreach (KeyValuePair<string, LowVegetationType> kvp in GameData.Instance.AllLowVegetationTypes)
            {
                kvp.Value.RenderOrder = RenderedTerrainType.HighestOrder;
                RenderedTerrainType.HighestOrder++;

                kvp.Value.Initialize();

            }

            return listOfVegTypes;
        }
        #endregion

        protected override List<Entities.Substances.SubstanceType> InitSubstanceTypes()
        {
            return SubstanceLoader.Init();
        }

        protected override List<IconInfo> InitIconInfo()
        {
            List<IconInfo> icons = new List<IconInfo>();

            icons.Add(new IconInfo() { KeyName = "canister", CenterYPos = 8 });
            icons.Add(new IconInfo() { KeyName = "charcoal", CenterYPos = 11 });
            icons.Add(new IconInfo() { KeyName = "blackBox", CenterYPos = 11 });

            icons.Add(new IconInfo() { KeyName = "potGoldenEmpty", CenterYPos = 9 });
            icons.Add(new IconInfo() { KeyName = "pot", CenterYPos = 9 });
            icons.Add(new IconInfo() { KeyName = "woodenPotEmpty", CenterYPos = 9 });            
            icons.Add(new IconInfo() { KeyName = "potGoldenFull", CenterYPos = 9 });
            icons.Add(new IconInfo() { KeyName = "potFull", CenterYPos = 9 });
            icons.Add(new IconInfo() { KeyName = "woodenPotFull", CenterYPos = 9 });

            icons.Add(new IconInfo() { KeyName = "clayPotSmall", CenterYPos = 9 });
            icons.Add(new IconInfo() { KeyName = "clayPot", CenterYPos = 12 });          
            icons.Add(new IconInfo() { KeyName = "drum", CenterYPos = 11 });

           
            return icons;
        }


        #region Resource Types
        protected override List<ResourceType> InitResourceTypes()
        {
            return ResourceLoader.Init();          
        }
#endregion

        #region Food Nutrient Types
        protected override List<FoodNutrientType> InitNutrientTypes()
        {

            List<FoodNutrientType> listOfFoodNutrientTypes = new List<FoodNutrientType>();


            FoodNutrientType nutrient = new FoodNutrientType("protein")
            {
                Name = "Protein", 
             //   DecimalsToShowBulk = 2               
            };
            listOfFoodNutrientTypes.Add(nutrient);

            nutrient = new FoodNutrientType("foodEnergy")
            {
                Name = "Calories", //was: Food energy   but should match needs descriptions on people (calorie deficiency)               
               // DecimalsToShowBulk = 2               
            };
            listOfFoodNutrientTypes.Add(nutrient);

            nutrient = new FoodNutrientType("micronutrients")
            {
                Name = "Micronutrients",
                //DecimalsToShowBulk = 4 
            };
            listOfFoodNutrientTypes.Add(nutrient);


            listOfFoodNutrientTypes.Add(new FoodNutrientType("stimulants")
            {
                Name = "Stimulants",
               // DecimalsToShowBulk = 4 
            });

            return listOfFoodNutrientTypes;

            // SerializeAndDeserializeTypeList(listOfFoodNutrientTypes, GameData.Instance.AllFoodNutrientTypes, "", "foodnutrienttypes.xml", Config.DataType.BaseData);


        }
        #endregion


        protected override List<PolledEventType> InitGlobalConditionalEvents()
        {
            return PolledEventsLoader.Init();
        }

        protected override List<ProcessToolSet> InitProcessToolSets()
        {
            return ToolsLoader.InitProcessToolSets();
        }

        protected override List<ProcessType> InitProcessTypes()
        {
            return ProcessLoader.InitProcessTypes();
        }

        protected override List<ClientSide.Particles.ParticleSystemType> InitParticleSystems()
        {
            return ParticlesLoader.Init();
        }

        /// <summary>
        /// only exists in BaseData!
        /// </summary>
        /// <returns></returns>
        public static List<Hint> InitHints()
        {
            List<Hint> list = new List<Hint>();

            list.Add(new Hint()
            {
                KeyName = "saveHint1",
                Priority = 10, // show first
                Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate." //    //was: "Remember to save the game once in a while. There is no auto-save."        
            });
///ensure that the save tip is shown often, give each a unique key otherwise they will all be skipped after showing one:
            list.Add(new Hint()
            {
                KeyName = "saveHint2",
                Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."            
            });

            list.Add(new Hint()
            {
                KeyName = "saveHint3",
                Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
            });

            list.Add(new Hint()
            {
                KeyName = "saveHint4",
                Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
            });

            list.Add(new Hint()
            {
                KeyName = "saveHint5",
                Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
            });
///////////////////////////

            list.Add(new Hint()
            {
                KeyName = "maintenanceHint",
                Text = "Colonists will automatically do maintenance on structures if the necessary time and tools are available. This will postpone the structure breaking down. However, colonists will not do maintenance on items."
            });
            list.Add(new Hint()
            {
                KeyName = "verminHint",                
                Text = "Some structures can keep out vermin while others can be accessed by animals. The data sheet for each structure type shows the details."
            });

            list.Add(new Hint()
            {
                KeyName = "filterHint",
                Text = "The top part of the Production panel contains filters that can be used for example to find objects that increase the COMFORT rating."
            });

            list.Add(new Hint()
            {
                KeyName = "ammoPolicyHint",
                Text = "The Policy panel has settings that restrict which ammunition types can be used against vermin."
            });

            list.Add(new Hint()
            {
                KeyName = "nestHint",
                Text = "Field quadite nests can be attacked with varmint bombs. This will prevent more field quadites from appearing for a long time."
            });
            

            list.Add(new Hint()
            {
                KeyName = "dogHint",
                Text = "The dog can live off a dead carcass for a long time. It can also consume rotten meat."
            });

            list.Add(new Hint()
            {
                KeyName = "shelterHint",
                Text = "Survival structures need a lot of maintenance. Buildings of a higher tier do not decay as rapidly but often require tools for maintenance - check the maintenance tasks when they appear in the Task Manager to make sure you have the needed tools."
            });

            list.Add(new Hint()
            {
                KeyName = "advancedTradeHint",
                Text = "In the time where 'Fields of Tau Ceti' takes place, Advanced Tech Tier items are rare but are sometimes offered for sale. They can appear and disappear off the market again." //was, but doesnt fit with muckroot mining site:   "In the 'Great Descent Era', advanced trade items are rare. They can appear and dissappear off the market again."
            });

            list.Add(new Hint()
            {
                KeyName = "toolsHint",
                Text = "To increase efficiency, make sure you have enough tools and weapons so colonists don't have to share. Use the Task Manager panel to identify bottlenecks."
            });

            list.Add(new Hint()
            {
                KeyName = "majorityHint",
                Text = "To get a majority for raising the tech tier, colonists must have sufficiently high principles. Their principles will slowly increase if the colony has a higher rating. Read more tooltips on the Policy panel."
            });

            list.Add(new Hint()
            {
                KeyName = "sleepingHint",
                Text = "Characters that sleep outdoors will never have their sleep need fully satisfied and it will also fill at a slower rate than for people sleeping indoors."
            });

            list.Add(new Hint()
            {
                KeyName = "shootingHint",
                Text = "A character with Expert shooting skill gets an extra bonus at hitting a target."
            });

            list.Add(new Hint()
            {
                KeyName = "examineHint",
                Text = "The EXAMINE action is needed to find many types of resources, such as mineral deposits, farm plots, fish spots and vegetables which would otherwise remain hidden."
            });



            return list;

        }

        protected override List<JobType> InitJobTypes()
        {
            List<JobType> list = new List<JobType>();

            list.Add(new StaticJobType()
            {
                KeyName = "examine",           
                StaticJobTypeSetting = StaticJobTypes.Examine,
                LabelType = JobLabelTypes.Red
            });
            list.Add(new StaticJobType()
            {
                KeyName = "scout",
                StaticJobTypeSetting = StaticJobTypes.Scout,
                LabelType = JobLabelTypes.Red
            });
            list.Add(new StaticJobType()
            {
                KeyName = "patrol",
                StaticJobTypeSetting = StaticJobTypes.Patrol,
                LabelType = JobLabelTypes.Red
            });
            list.Add(new StaticJobType()
            {
                KeyName = "attackArea",
                StaticJobTypeSetting = StaticJobTypes.AttackArea,
                LabelType = JobLabelTypes.Red
            });
            list.Add(new StaticJobType()
            {
                Comments = "used for both Hunting and FindPrey jobs. From the player's perspective, they are the same.",
                KeyName = "hunt", // "findPrey",
                StaticJobTypeSetting = StaticJobTypes.Hunt,
                LabelType = JobLabelTypes.Red
            });
            list.Add(new StaticJobType()
            {
                KeyName = "haulToStorage",
                StaticJobTypeSetting = StaticJobTypes.HaulToStorage,
                LabelType = JobLabelTypes.Brown
            });

            list.Add(new StaticJobType()
            {               
                KeyName = "repairingJobType",
                StaticJobTypeSetting = StaticJobTypes.Repairing,
                LabelType = JobLabelTypes.Grey
            });

            //removed as jobtype to limit amount of entries in drop-down menu. instead uses JobTypeKey = "constructionJobType" 
   /*         list.Add(new StaticJobType()
            {
                KeyName = "upgradingJobType",
                StaticJobTypeSetting = StaticJobTypes.Upgrading,
                LabelType = JobLabelTypes.Brown
            });*/

            //removed as jobtype to limit amount of entries in drop-down menu. salvaging is usually a one-time thing.
  /*          list.Add(new StaticJobType()
            {
                KeyName = "salvagingJobType",
                StaticJobTypeSetting = StaticJobTypes.Salvaging,
                LabelType = JobLabelTypes.Brown
            });*/
            list.Add(new ProcessJobType()
            {
                KeyName = "weedingAndFertilizingJobType",
                Name = "Weeding/Fertilizing",
                LabelType = JobLabelTypes.Grey
            });
            //
            list.Add(new ProcessJobType()
            {//used for farm plots. and favorbread cultivation
                KeyName = "sowingAndHarvestingJobType",
                Name = "Sowing/Harvesting",
                LabelType = JobLabelTypes.Green
            });
            list.Add(new ProcessJobType()
            {
                Comments = "does not include stimulants",
                KeyName = "cookingJobType",
                Name = "Cooking",
                LabelType = JobLabelTypes.Blue
            });
            // check if it actualyl works
            list.Add(new ProcessJobType()
            {
                KeyName = "sentryReloadJobType",
                Name = "Reload sentry",
                LabelType = JobLabelTypes.Blue
            });
            list.Add(new ProcessJobType()
            {
                //all kinds of crafting and production
                KeyName = "craftingJobType",
                Name = "Producing",
                LabelType = JobLabelTypes.SteelGrey
            });
            //
            list.Add(new ProcessJobType()
            {                
                KeyName = "butcheringJobType",
                Name = "Butchering",
                LabelType = JobLabelTypes.Green
            });
            list.Add(new ProcessJobType()
            {
                KeyName = "checkTrapsJobType",
                Name = "Check traps",
                LabelType = JobLabelTypes.Grey
            });
            list.Add(new ProcessJobType()
            {
                Comments = "includes turnip hut and fish traps",
                KeyName = "constructionJobType", 
                Name = "Construction",
                LabelType = JobLabelTypes.Brown
            });
            list.Add(new ProcessJobType()
            {               
                KeyName = "gatherFoodJobType",
                Name = "Gather food",
                LabelType = JobLabelTypes.Green
            });
            list.Add(new ProcessJobType()
            {
                KeyName = "gatherMaterialsJobType",
                Name = "Gather materials",
                LabelType = JobLabelTypes.Grey
            });
            list.Add(new ProcessJobType()
            {
                KeyName = "gatherFuelJobType",
                Name = "Gather fuel",
                LabelType = JobLabelTypes.Brown
            });
            list.Add(new ProcessJobType()
            {
                KeyName = "extractFromDepositJobType", //so player can see in task manager which one comes from spread out resources and which ones from deposit.
                // and maybe he wants to control if they should go to a far away pit
                Name = "Extract (deposit)",
                LabelType = JobLabelTypes.Grey
            });
            return list;
        }

        protected override List<ClientSide.Interface.Inventory.FilterSettingType> InitFilterSettingTypes()
        {
            List<FilterSettingType> list = new List<FilterSettingType>();
            
            /*  {"micronutrients", 0.0030f },
              {"protein",0.047f },
              {"foodEnergy",  0.7f },
              {"stimulants",  0.02f }
             */
             #region Nutrients
                list.Add(new NutrientFilterSettingType()
                {
                    KeyName = "highProteinForHumans",
                    ConsumableByEntity = "entity:human",
                    Nutrient = "protein",
                    Limit = 0.047f
                });
             list.Add(new NutrientFilterSettingType()
                {
                    KeyName = "highCaloriesForHumans",
                    ConsumableByEntity = "entity:human",
                    Nutrient = "foodEnergy",
                    Limit = 0.7f
                });
             list.Add(new NutrientFilterSettingType()
             {
                 KeyName = "highMicronientsForHumans",
                 ConsumableByEntity = "entity:human",
                 Nutrient = "micronutrients",
                 Limit = 0.0030f
             });
             list.Add(new NutrientFilterSettingType()
             {
                 KeyName = "highStimulantsForHumans",
                 ConsumableByEntity = "entity:human",
                 Nutrient = "stimulants",
                 Limit = 0.02f 
             });
#endregion

             #region StaticFilterSettings
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "containers",
                 StaticFilterSetting = StaticFilterSettings.Containers             
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "storage",
                 StaticFilterSetting = StaticFilterSettings.Storage
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "fuel"/*Fertilizer"*/,
                 StaticFilterSetting = StaticFilterSettings.Fuel/*Fertilizer*/
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "betterTools",
                 StaticFilterSetting = StaticFilterSettings.BetterTools
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "structures",
                 StaticFilterSetting = StaticFilterSettings.Structures
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "items",
                 StaticFilterSetting = StaticFilterSettings.Items
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "usableAsWeapon",
                 StaticFilterSetting = StaticFilterSettings.UsableAsWeapon
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "affectsFoodRating",
                 StaticFilterSetting = StaticFilterSettings.AffectsFoodRating
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "affectsComfortRating",
                 StaticFilterSetting = StaticFilterSettings.AffectsComfortRating
             });
             list.Add(new StaticFilterSettingType()
             {
                 KeyName = "affectsSecurityRating",
                 StaticFilterSetting = StaticFilterSettings.AffectsSecurityRating
             });
             #endregion

             #region Categories

             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "ammunition",
                 EntityCategory = "ammunition",
             });
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "waste",
                 EntityCategory = "waste",
             });
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "preparedFood",
                 EntityCategory = "preparedFood",
             });
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "ingredients",
                 EntityCategory = "ingredients",
             });
            /*
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "bodies",
                 EntityCategory = "bodies",
             });*/
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "rawMaterials",
                 EntityCategory = "rawMaterials",
             });
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "weapons",
                 EntityCategory = "weapons",
             });
             list.Add(new CategoryFilterSettingType()
             {
                 KeyName = "tools",
                 EntityCategory = "tools",
             });


             #endregion

             #region Tiers

             list.Add(new TierFilterSettingType()
             {
                 KeyName = "survivalTier",
                 Tier = "survival"
             });

             list.Add(new TierFilterSettingType()
             {
                 KeyName = "basicTier",
                 Tier = "basic"
             });

             list.Add(new TierFilterSettingType()
             {
                 KeyName = "mediumTier",
                 Tier = "medium"
             });

             list.Add(new TierFilterSettingType()
             {
                 KeyName = "advancedTier",
                 Tier = "advanced"
             });


             #endregion

             #region Tiers

             list.Add(new PolicyAreaFilterSettingType()
             {
                 KeyName = "comfortPolicy",
                 RatingType = RatingTypes.Comfort
             });

             list.Add(new PolicyAreaFilterSettingType()
             {
                 KeyName = "foodPolicy",
                 RatingType = RatingTypes.Food
             });

             list.Add(new PolicyAreaFilterSettingType()
             {
                 KeyName = "securityPolicy",
                 RatingType = RatingTypes.Security
             });
             #endregion


             return list;


        }

        protected override List<PresentationTypeCategory> InitPresentationTypeCategories()
        {
            List<PresentationTypeCategory> list = new List<PresentationTypeCategory>();

            list.Add(new PresentationTypeCategory()
                        {          
                            KeyName = "occupantsCategory",
                            Name = "OCCUPANTS",
                            PanelSortOrder = 0,
                            Nodes = new[]
                            {
                                new GroupNode()
                                {
                                    SortOrder = 0,
                                     SetCountAsSummary = true, // this will make the number of occupants appear in the bar of the collapsable panel
                                     DynamicList = new DynamicList()
                                     {
                                        HasPropertiesList = "contained",
                                        Filter = new PropertyCondition(){ PropertyKey = "isAgent", BoolValue = true },
                                        Presentation = new Presentation()
                                        {                                           
                                            PropertyNameForValue = "name", 
                                            MakePropertyClickable = true
                                        } 
                                     }
                                }
                            }
                        });          

         list.Add(new PresentationTypeCategory()
                        {
                            KeyName = "healthStatusCategory",
                           // SortingType = WindowSystem.Grid.Sorting.Descending,
                            Name = "HEALTH STATUS",
                            PanelSortOrder = 5,
                            Nodes = new PresentationNode[]
                            {
                                new GroupNode() // ENERGY
                                {
                                    SortOrder = 0,
                                    HasBorder = true,
                                    ShowConnectors = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                        Presentation =  new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "ENERGY" },
                                            CaptionTooltip = new StringSource() { StaticString = "Energy affects work speed, combat ability and other activities." },
                                            PropertyNameForValue = "energyLevel", 
                                            PresentationTypeKey = "energyPresentationSidePanel"
                                        }
                                    },
                                     Nodes = new PresentationNode[]
                                     {                                         
                                         new GroupNode() // NUTRITION - NEEDS
                                         {   
                                               SortOrder = 1,   
                                               ShowConnectors = true,
                                               GroupHeader = new GroupHeader()
                                               {
                                                    Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "NUTRITION" }, 
                                                            CaptionTooltip = new StringSource() { StaticString = "Evaluation of the intake of the 3 nutrient groups: Calories, protein and micronutrients." },
                                                            PropertyNameForValue = "hungerStatus", 
                                                            PresentationTypeKey = "hungerPresentationSidePanel"
                                                        }
                                               },
                                              Nodes = new PresentationNode[]
                                              {         
                                                  // should be Dynamic instead of this...
                                                  new LeafNode()
                                                  {
                                                      SortOrder = 1,                                                      
                                                      Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "Calories" },  
                                                            CaptionTooltip = new StringSource() { StaticString = "Represents the fuel needed by living organisms." },
                                                            PropertyNameForValue = "foodEnergyLevel", 
                                                            PresentationTypeKey = "foodEnergyPresentation"
                                                        }
                                                  },
                                                  new LeafNode()
                                                  {
                                                      SortOrder = 2,
                                                      Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "Protein" },  
                                                            CaptionTooltip = new StringSource() { StaticString = "Protein is the building blocks of living cells. Meat is a primary source of protein." },
                                                            PropertyNameForValue = "proteinLevel", 
                                                            PresentationTypeKey = "proteinPresentation"
                                                        }
                                                  },   
                                                  new LeafNode()
                                                  { 
                                                      SortOrder = 3,
                                                      Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "Micronutrients" },  
                                                            CaptionTooltip = new StringSource() { StaticString = "Important vitamins and minerals that the organism needs in small quantities." },
                                                            PropertyNameForValue = "micronutrientsLevel", 
                                                            PresentationTypeKey = "micronutrientsPresentation"
                                                        }
                                                  }     
                                              }
                                         },
                                         new GroupNode() // SLEEP
                                         {
                                             SortOrder = 2,                                           
                                              GroupHeader = new GroupHeader()
                                              {
                                                   Presentation =  new Presentation()
                                                    {                                                        
                                                        Caption = new StringSource() { StaticString = "SLEEP" },
                                                        CaptionTooltip = new StringSource() { StaticString = "Sleep is needed by most animals. Without it, death will occur." },
                                                        PropertyNameForValue = "sleepStatus", 
                                                        PresentationTypeKey = "sleepNeedPresentationSidePanel"
                                                    }
                                              }                                              
                                         }
                                     }
                                }, // /ENERGY

                                 new GroupNode() // STIMULANTS
                                {
                                    SortOrder = 1,
                                    HasBorder = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                        Presentation =  new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "STIMULANTS" },                                          
                                            CaptionTooltip = new StringSource() { StaticString = "Not an essential need, but used in moderation, stimulants can give a boost to overall efficiency. \nAccess to stimulants improves people's happiness and the colony's comfort conditions" },
                                            PropertyNameForValue = "stimulantsLevel", 
                                            PresentationTypeKey = "stimulantsPresentation"
                                        }
                                    }
                                },
                                new GroupNode() // INJURIES
                                {
                                    SortOrder = 2,
                                    HasBorder = true,
                                    ShowConnectors = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                        Presentation = new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "INJURIES" },
                                            CaptionTooltip = new StringSource() { StaticString = "Diagnosis of any physical injuries suffered." }, // by the person"  ..mp :if used for animals, then cannot mention person
                                            PropertyNameForValue = "hitpointLevel", 
                                            PresentationTypeKey = "hitpointsPresentation"
                                        }
                                    },
                                    DynamicList = new DynamicList()
                                     {
                                            HasPropertiesList = "bodyParts",
                                                Presentation = new Presentation()
                                            {
                                               // CaptionSource = Presentation.CaptionType.Default,
                                                Caption = new StringSource() { UseDefaultName = true },
                                                PropertyNameForValue = "Injuries", 
                                                PresentationTypeKey = "bodyPartCondition"
                                            }
                                     }                                                                        
                                }

                                // TODO!!!
                                ,
 
                                 new GroupNode() // 
                                {
                                    SortOrder = 3,
                                    HasBorder = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                        Presentation =  new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "STANCE" }, 
                                            CaptionTooltip = new StringSource() { StaticString = "Represents the willingness to move near danger or threats. Injuries or low morale can make persons stay away from danger." },
                                            PropertyNameForValue = "threatStance", 
                                            PresentationTypeKey = "threatStancePresentation"
                                        }
                                    }
                                },  

                                 new GroupNode() // 
                                {
                                    SortOrder = 4,
                                    HasBorder = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                        Presentation =  new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "MORALE" },
                                            CaptionTooltip = new StringSource() { StaticString = "Shows the person's morale. Low morale can cause panic and fleeing. High morale is needed to take the 'Fearless' stance and move into danger." },
                                            PropertyNameForValue = "moraleLevel", 
                                            PresentationTypeKey = "moraleLevelPresentation"
                                        }
                                    }
                                }                            
                                                          
                            }
                        });
       
         list.Add(new PresentationTypeCategory()
                        {     
                            KeyName = "cropsCategory",
                            Name = "CROPS", 
                            StartsAsExpanded = false,
                            Nodes = new[]
                            {
                                new LeafNode()
                                {                                   
                                    Presentation = new Presentation()
                                    {            
                                        Caption = new StringSource() {  StaticString = "PLANTED" },
                                        PropertyNameForValue = "cropTypeToSpawn",
                                        PresentationTypeKey = "entityTypePresentation"
                                    },  
                                    SortOrder = 0                                    
                                },
                                new LeafNode()
                                {                                   
                                    Presentation = new Presentation()
                                    {            
                                        Caption = new StringSource() {  StaticString = "" },
                                        PropertyNameForValue = "cropState"                                       
                                    }, 
                                    SortOrder = 5                                    
                                },
                                new LeafNode()
                                {                                   
                                    Presentation = new Presentation()
                                    {                          
                                        Caption = new StringSource() {  StaticString = "CROP SIZE" },
                                        CaptionTooltip = new StringSource() { StaticString = "The current size of crops" },
                                        PropertyNameForValue = "cropGrowthProgress", 
                                        PresentationTypeKey = "cropGrowthPresentation"
                                    }, 
                                    SortOrder = 10                                    
                                },
                                new LeafNode()
                                {                                   
                                    Presentation = new Presentation()
                                    {                
                                        Caption = new StringSource() {  StaticString = "WEEDS SIZE" },                                       
                                        CaptionTooltip = new StringSource() { StaticString = "The field should be weeded regularly to ensure high crop yield" },
                                        PropertyNameForValue = "weedGrowthProgress", 
                                        PresentationTypeKey = "weedGrowthPresentation"
                                    },
                                    SortOrder = 15                                   
                                },
                                new LeafNode()
                                {                                   
                                    Presentation = new Presentation()
                                    {              
                                        Caption = new StringSource() {  StaticString = "SOIL FERTILITY" },
                                        CaptionTooltip = new StringSource() { StaticString = "With every crop cycle, the soil quality will go down if not fertilized. Depleted soil will yield fewer crops" },
                                        PropertyNameForValue = "nutrientLevel", 
                                        PresentationTypeKey = "soilNutrientLevelPresentation"
                                    },
                                    SortOrder = 20                                    
                                },
                                new LeafNode()
                                {      
                                    Presentation = new Presentation()
                                    {                   
                                        Caption = new StringSource() {  StaticString = "HARVEST ON" },
                                        CaptionTooltip = new StringSource() { StaticString = "Estimated date when crops are ready for harvesting" },
                                        PropertyNameForValue = "harvestDate", 
                                       // PresentationTypeKey = "datePresentation"
                                    },
                                    SortOrder = 25                                  
                                },
                               
                            }
                        });

        list.Add( new PresentationTypeCategory()
                        {
                            KeyName = "statusCategory",
                            Name = "STATUS",
                            PanelSortOrder = 15,
                            Nodes = new PresentationNode[]
                            {
                                new GroupNode() // BULK / SUBSTANCES
                                {
                                    SortOrder = 10,
                                    HasBorder = true,                                  
                                     GroupHeader = new GroupHeader()
                                     {
                                        Presentation = new Presentation()
                                        {
                                            Caption = new StringSource() {  StaticString = "SIZE" },
                                            PropertyNameForValue = "itemBulkForPresentation", 
                                            PresentationTypeKey = "itemBulkPresentation"
                                        } 
                                         
                                           
                                     },
                                      Nodes = new PresentationNode[]
                                      {
                                          new GroupNode()
                                          {
                                              /* Presentation = new Presentation()
                                              {
                                                   CaptionData = "SUBSTANCES", // just a header???
                                                   CaptionSource = Presentation.CaptionType.Static
                                              }*/
                                             DynamicList = new DynamicList()
                                             {
                                                HasPropertiesList = "SubstanceTypes",
                                                Presentation = new Presentation()
                                                {
                                                    Caption = new StringSource() { UseDefaultName = true },
                                                    //CaptionSource = Presentation.CaptionType.Default,
                                                    PropertyNameForValue = "substanceLevel", 
                                                    PresentationTypeKey = "substancePresentation"
                                                }
                                             }

                                          }
                                      }
                                     
                                },         
                                new GroupNode()
                                {
                                 //   SubHeader = "NUTRITION:", // nutrition contents of items
                                    SortOrder = 11,
                                    HasBorder = true,
                                    GroupHeader = new GroupHeader()
                                    {
                                          Presentation = new Presentation()
                                          {
                                              Caption = new StringSource() {  StaticString = "NUTRITION" },                                            
                                              CaptionTooltip = new StringSource() { StaticString = "Shows the percentage of an adult's daily needs this item will satisfy" }, 
                                          }                                          
                                    },
                                    DynamicList = new DynamicList()
                                    {
                                        HasPropertiesList = "nutrition",
                                        Presentation = new Presentation()
                                        {
                                            Caption = new StringSource() { UseDefaultName = true },
                                            //CaptionSource = Presentation.CaptionType.Default,
                                            PropertyNameForValue = "satisfiedDailyIntake", // "nutrientLevel",                                    
                                        }
                                    }
                                },
                                
                                new LeafNode()
                                {
                                    SortOrder = 12,                                    
                                    Presentation = new Presentation()
                                    {
                                        KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom, // uses DegradeType key for lookup. Lars: where?
                                       // UsePropertyKeyNameForTypeDependentPresentation = true,                                      
                                        Caption = new StringSource() {  StaticString = "PROGRESS" },
                                        PropertyNameForValue = "ConstructionProgress", 
                                        PresentationTypeKey = "ConstructionProgress"
                                    }                                    
                                },                              
                                new GroupNode()
                                {
                                    SortOrder = 14,
                                    HasBorder = true,     
                                    ShowConnectors = true,                             
                                   GroupHeader = new GroupHeader()
                                   {
                                       Presentation = new Presentation()
                                        {
                                            //orderingNumber = 0,
                                            KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom, // uses DegradeType key for lookup.
                                            Caption = new StringSource() { StaticString = "CONDITION" },
                                            CaptionTooltip = new StringSource() { StaticString = "The overall condition of the item or structure depends on its parts and on its integrity." },
                                            PropertyNameForValue = "itemPartCondition", // fiæing in these is not needed when Static source is used, but will have the effect that the whole header (and the panel in turn) gets suppressed if the display term evaluates to null
                                            ValueTooltip = new StringSource() { PropertyName = "itemPartConditionTooltip" },
                                            PresentationTypeKey = "PartConditions"
                                        } 
                                      
                                   },
                                   Nodes = new PresentationNode[]
                                     {                                          
                                         new GroupNode() 
                                         {   
                                               SortOrder = 1,                                           
                                               GroupHeader = new GroupHeader()
                                               {
                                                    Presentation = new Presentation()
                                                        {                                                           
                                                            Caption = new StringSource() {  StaticString = "INTEGRITY" },
                                                            CaptionTooltip = new StringSource() { StaticString = "The integrity of the item or structure, or how well it is holding its parts together." },
                                                            PropertyNameForValue = "integrity",                                                                                           
                                                            PresentationTypeKey = "integrityPresentation" 
                                                        }
                                               }
                                         },                                        
                                        new GroupNode()
                                        {
                                           SortOrder = 2,        
                                           GroupHeader = new GroupHeader()
                                           {
                                               Presentation = new Presentation()
                                               {
                                                     Caption = new StringSource() {  StaticString = "COMPONENT PARTS" }, 
                                                     CaptionTooltip = new StringSource() { StaticString = "The list of parts that this item or structure is composed of. Can often be retrieved by salvaging it." }
                                               }
                                           },
                                             DynamicList = new DynamicList()
                                             {
                                                HasPropertiesList = "itemParts",
                                                Presentation = new Presentation()
                                                {                                                      
                                                    KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom, // uses DegradeType key for lookup.
                                                    Caption = new StringSource() { UseDefaultName = true },
                                                    PropertyNameForValue = "itemPartCondition", 
                                                    PresentationTypeKey = "PartConditions",  
                                                    ValueTooltip = new StringSource() { PropertyName = "itemPartConditionTooltip" },
                                                    MakePropertyClickable = true
                                                }
                                             } 
                                        }
                                     }
                                                                    
                                },                                
                                new LeafNode()
                                {
                                    SortOrder = 15,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() { StaticString = "DAYS LEFT" },
                                        CaptionTooltip = new StringSource() { StaticString = "How long this item will last when stored under its current conditions" },
                                        PropertyNameForValue = "daysUntilBreakDown",                                                                                           
                                        PresentationTypeKey = "timeInDaysPresentation" 
                                    }
                                },
                                new LeafNode()
                                {
                                    SortOrder = 16,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() {  StaticString = "SHOTS REMAINING" },
                                        PropertyNameForValue = "ammoLevel", 
                                        PresentationTypeKey = null
                                    }                                    
                                },
                                 new LeafNode()
                                {
                                    SortOrder = 17,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() {  StaticString = "STORAGE CAPACITY" },
                                        PropertyNameForValue = "itemStorageFraction", 
                                        CaptionTooltip = new StringSource() { StaticString = "Capacity for stored/carried items" },
                                        PresentationTypeKey = "storageCapacityPresentation",
                                        ValueTooltip = new StringSource() { PropertyName = "storageFractionTooltip" },                 
                                        
                                    }                                    
                                },      
                                new LeafNode()
                                {
                                    SortOrder = 19,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() {  StaticString = "TRADE CAPACITY" },
                                        PropertyNameForValue = "tradeStorageFraction", 
                                        CaptionTooltip = new StringSource() { StaticString = "Storage capacity for items that are set for sale" },
                                        PresentationTypeKey = "storageCapacityPresentation", 
                                        ValueTooltip = new StringSource() { PropertyName = "tradeStorageFractionTooltip" }
                                    }                                    
                                },                              
                                new LeafNode()
                                {
                                    SortOrder = 21,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() {  StaticString = "COMFORT VALUE" }, //"COMFORT LEVEL"
                                        PropertyNameForValue = "homeComfortLevel", 
                                        CaptionTooltip = new StringSource() { StaticString = "The current comfort level of the dwelling, this is affected by its condition as well as any upgrades." },
                                        PresentationTypeKey = "percentagePresentation"
                                    }                                    
                                }
                            }
                        });

                    list.Add(new PresentationTypeCategory()
                    {
                        KeyName = "residentsCategory",
                        Name = "RESIDENTS",
                        PanelSortOrder = 17,
                        Nodes = new[]
                                {
                                    new GroupNode()
                                    {
                                        SortOrder = 0,
                                            SetCountAsSummary = true, // this will make the number of occupants appear in the bar of the collapsable panel
                                            DynamicList = new DynamicList()
                                            {
                                            HasPropertiesList = "residents",
                                            Presentation = new Presentation()
                                            {                                           
                                                PropertyNameForValue = "name", 
                                                MakePropertyClickable = true
                                            } 
                                            }
                                    }
                                }
                    });

                    list.Add(new PresentationTypeCategory()
                    {
                        KeyName = "skillsCategory",
                        Name = "SKILLS",
                        StartsAsExpanded = false,
                        PanelSortOrder = 20,
                        Nodes = new[]
                            {
                                new GroupNode()
                                {
                                    SortOrder = 8, 
                                    DynamicList = new DynamicList()
                                    {
                                        HasPropertiesList = "skills",
                                        Presentation = new Presentation()
                                        {
                                            Caption = new StringSource() { UseDefaultName = true },
                                            //CaptionSource = Presentation.CaptionType.Default,
                                            PropertyNameForValue = "skillLevel",              
                                            CaptionTooltip = new StringSource() { PropertyName = "skillDescription" },
                                            PresentationTypeKey = "skillPresentationSidePanel"
                                        }       
                                    }
                                }
                            }
                    });

                 list.Add(new PresentationTypeCategory()
                         {
                             KeyName = "opinionsCategory",
                             Name = "OPINIONS",
                             StartsAsExpanded = false,
                             PanelSortOrder = 30,
                             Nodes = new PresentationNode[]
                            {                             
                                new LeafNode()
                                { 
                                    SortOrder = 5,
                                     Presentation = new Presentation()
                                     {
                                          Caption = new StringSource(){ StaticString = "RISK OF EMIGRATING" }, //"EMIGRATE RISK"
                                          PropertyNameForValue = "emigrateRiskForPlaySite",
                                          CaptionTooltip = new StringSource(){ StaticString = "The daily chance that the character will decide to leave the colony" },
                                          PresentationTypeKey = "emigrationRiskPresentation", // "percentagePresentation", 
                                          ValueTooltipSettings = new TooltipSettings(){ DisableExpiry = true, TooltipWidth = 260 },
                                          ValueTooltip = new StringSource() { PropertyName = "emigrateRiskTooltip" }
                                     }
                                },
                               
                                new GroupNode() 
                                {
                                    SortOrder = 10,
                                    HasBorder = true, 
                                    ShowConnectors = true,
                                    GroupHeader = new GroupHeader()
                                    {   
                                        Presentation =  new Presentation()
                                        {
                                            Caption = new StringSource() { StaticString = "HAPPINESS" }, 
                                           // CaptionTooltip = new StringSource() { StaticString = "The character's personal experience of their conditions" } //"The character's rating of his/her situation"
                                            PropertyNameForValue = "happiness",
                                            CaptionTooltip = new StringSource(){ StaticString = "Happiness is the difference between the character's principles and their personal conditions. If conditions are worse than their principles, they will be unhappy. If conditions are better, they are happy" },
                                            PresentationTypeKey = "happinessPresentation"                                

                                        }
                                    },
                                     Nodes = new PresentationNode[]
                                     {                                         
                                         new GroupNode() 
                                         {   
                                               SortOrder = 1,                       
                                               
                                               GroupHeader = new GroupHeader()
                                               {                                                    
                                                    Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() {  StaticString = "FOOD" },  
                                                            PropertyNameForValue = "foodPrinciplesAndRating",  
                                                            PresentationTypeKey = "foodHappinessPresentation",
                                                            ValueTooltip = new StringSource()
                                                            {
                                                                  PropertyName = "foodHappinessTooltip"
                                                            },
                                                            ValueTooltipSettings = new TooltipSettings()
                                                            {
                                                                TooltipActivationProperty = "toggleFoodRatingTooltip",
                                                                DisableExpiry = true,
                                                                TooltipWidth = 260
                                                            }                                                            
                                                        }
                                                      
                                               }
                                         },                                        
                                         new GroupNode() 
                                         {   
                                               SortOrder = 2,                                           
                                               GroupHeader = new GroupHeader()
                                               {
                                                    Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "SECURITY" },  
                                                            PropertyNameForValue = "securityPrinciplesAndRating", 
                                                            PresentationTypeKey = "securityHappinessPresentation",  
                                                            ValueTooltip = new StringSource()
                                                            {
                                                                  PropertyName = "securityHappinessTooltip"
                                                            },
                                                            ValueTooltipSettings = new TooltipSettings()
                                                            {
                                                                TooltipActivationProperty = "toggleSecurityRatingTooltip",
                                                                DisableExpiry = true,
                                                                TooltipWidth = 260
                                                            }                                                             
                                                        }
                                               }
                                         },
                                         new GroupNode() 
                                         {   
                                               SortOrder = 3,       
                                               ShowConnectors = true,
                                               GroupHeader = new GroupHeader()
                                               {
                                                    Presentation = new Presentation()
                                                        {
                                                            Caption = new StringSource() { StaticString = "COMFORT" },  
                                                            PropertyNameForValue = "comfortPrinciplesAndRating", 
                                                            PresentationTypeKey = "comfortHappinessPresentation",
                                                            ValueTooltip = new StringSource()
                                                            {
                                                                  PropertyName = "comfortHappinessTooltip"
                                                            },
                                                            ValueTooltipSettings = new TooltipSettings()
                                                            {
                                                                TooltipActivationProperty = "toggleComfortRatingTooltip",
                                                                DisableExpiry = true,
                                                                TooltipWidth = 260
                                                            }
                                                           
                                                        }
                                               },
                                               // display a link to the home entity
                                               // only dynamic lists/children support entity links
                                               DynamicList = new DynamicList()
                                                 {
                                                    HasPropertiesList = "home",
                                                    Presentation = new Presentation()
                                                    {                                                       
                                                        Caption = new StringSource() {  StaticString = "Home" },  
                                                        PropertyNameForValue = "name",                                                      
                                                        MakePropertyClickable = true
                                                    }
                                                 }                                              
                                         }
                                     }
                                }
                            }
                         });


                 list.Add(new PresentationTypeCategory()
                 {
                     KeyName = "effectsCategory",
                     Name = "EFFECTS",
                     StartsAsExpanded = false,
                     PanelSortOrder = 35,
                     Nodes = new[]
                            {
                                new GroupNode()
                                {
                                    SortOrder = 0,                                   
                                     Nodes = new PresentationNode[]
                                     {       
                                         new GroupNode() 
                                         { 
                                              DynamicList = new DynamicList()
                                              {
                                                   HasPropertiesList = "effectProfiles",
                                                  /* Presentation = new Presentation()
                                                   { 
                                                        Caption = new StringSource() { UseDefaultName = true },                                                       
                                                       // PropertyNameForValue = "noOfEffects",              
                                                        CaptionTooltip = new StringSource() { PropertyName = "description" },    
                                                      //  PresentationTypeKey = "effectProfilePresentationSidePanel"
                                                   } */
                                                   Presentation = new Presentation()
                                                   { 
                                                        PropertyNameForValue = "name",                                                                     
                                                        ValueTooltip = new StringSource() { PropertyName = "description" }  
                                                   }
                                              },
                                              SortOrder = 1     
                                         }                                         
                                     }
                                }
                            }
                 });

            return list;
        }

        public static readonly Color ProgressColor = Common.ColorFromHex("#69E590");
        public static readonly Color ConsumeProgressColor = Common.ColorFromHex("#FFAB26");

        protected override List<PresentationType> InitPresentationTypes()
        {
            List<PresentationType> list = new List<PresentationType>();
            
                list.Add(new PresentationType()
                {
                    KeyName = "totalProductivity",  
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.0f /*, Icon = "HUD_icon_status_injury" ,Tint = Common.ColorFromHex("#cac172")*/ },
                            new Threshold(){ Edge = 0.8f  },
                            new Threshold(){ Edge = 1.0f/*, Icon = "HUD_icon_status_injury" ,Tint = Common.ColorFromHex("#cac172")*/ },
                        }
                    },
                });

                Color badStatusColor = Common.ColorFromHex("#c6000e");
                Color lessBadStatusColor = Common.ColorFromHex("#FF4F5D"); //  "#BF6970");
                Color secondBestStatusColor = Common.ColorFromHex("#ffffff");
                Color bestStatusColor = Common.ColorFromHex("#38eaff");

                Color badStatusColorSidePanelBar = Common.ColorFromHex("#C45A63"); // has to be fainter to fit within the LCD sidepanel says Morten.
               // Color normalStatusColorSidepanelBar = Common.ColorFromHex("#3a7177");

               // Color progressColor = Common.ColorFromHex("#69E590");//"#49ACA3");
                Color normalStatusColorSidepanelBar = Common.ColorFromHex("#C3F0F5");

                Color normalStatusColorSidepanel = Common.ColorFromHex("#3a7177"); //mp: this should be same as normal text color in sidemenu               

                Color foodColor = Common.ColorFromHex("#59C24E");
                Color securityColor = Common.ColorFromHex("#559DBA");
                Color comfortColor = Common.ColorFromHex("#C76098");


                Color energyBarColor = Common.ColorFromHex("#9FF0FD");
                Color nutritionBarColor = Common.ColorFromHex("#AAFFD2");
                Color subNutritionBarColor = Common.ColorFromHex("#C3FEE0");
                Color sleepBarColor = Common.ColorFromHex("#B9E1FE");

                Color cropBarColor = Common.ColorFromHex("#AAFFD2");
                Color weedBarColor = Common.ColorFromHex("#C3FEE0");
                Color soilBarColor = Common.ColorFromHex("#CCB390");

            
                list.Add(new PresentationType()
                {
                    KeyName = "toolProductivity",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
#region very old, not used
                            //new Threshold(){ Edge = 0.1f, Term = "Barely helpful", Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#c6000e") },
                            //new Threshold(){ Edge = 0.2f , Term = "Not very efficient",Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ff1a2a") }, 
                            //new Threshold(){ Edge = 0.4f , Term = null,  /* "Not very efficient"  ,*/Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ffffff") },
                            //new Threshold(){ Edge = 0.8f , Term = null, /* "Moderately efficient"  ,*/ Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ffffff") },
                            //new Threshold(){ Edge = 0.95f , Term = "Highly efficient", Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ffffff") },
                            //new Threshold(){ Edge = 1.0f , Term = "Best tool for the job!" ,Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#38eaff")},
#endregion
#region Placeholder edges, made specific for first scenario march 2014
 //                           new Threshold(){ Edge = 0.1f, Term = "Tool is not very efficient", Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ff1a2a") },
 //                           new Threshold(){ Edge = 0.2f , Term = "Tool is quite efficient",Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#ffffff") }, 
 //                           new Threshold(){ Edge = 0.3f , Term = "Best tool(s) for the job!",  Icon = "HUD_icon_productivity_tool" ,Tint = Common.ColorFromHex("#38eaff") },
#endregion New, July 2015 (MP)
                            new Threshold(){ Edge = 0.2f, Term = "Tool is not very efficient", Icon = "HUD_icon_productivity_tool" ,IconTint = badStatusColor },
                            new Threshold(){ Edge = 0.5f , Term = "Tool is moderately efficient",Icon = "HUD_icon_productivity_tool"  }, 
                            new Threshold(){ Edge = 0.75f , Term = "Tool is highly efficient",  Icon = "HUD_icon_productivity_tool" ,IconTint = secondBestStatusColor },
                            new Threshold(){ Edge = 1f , Term = "Best tool(s) for the job!",  Icon = "HUD_icon_productivity_tool" ,IconTint = bestStatusColor },

                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "ConstructionProgress",  //category name defined just below this section
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.0f, Term = "Not started" },
                            new Threshold(){ Edge = 0.1f, Term = "Started" }, 
                            new Threshold(){ Edge = 0.4f, Term = "Shaping up" },
                            new Threshold(){ Edge = 0.6f, Term = "Halfway there" },
                            new Threshold(){ Edge = 0.99f, Term = "Near completion" },
                            new Threshold(){ Edge = 1.0f, Term = null },
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "ProcessProgress",  // all process types - for jobs panel
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.0f, Term = "Not started" },
                            new Threshold(){ Edge = 0.01f, Term = "Just started" }, 
                            new Threshold(){ Edge = 0.3f, Term = "Under way" },
                            new Threshold(){ Edge = 0.6f, Term = "Halfway there" },
                            new Threshold(){ Edge = 0.995f, Term = "Almost finished" },
                            new Threshold(){ Edge = 1.0f, Term = null },
                        }
                    },
                });
//moved above, delete this:
        //        Color badStatusColor = Common.ColorFromHex("#c6000e");
        //        Color lessBadStatusColor = Common.ColorFromHex("#FF4F5D"); //  "#BF6970");
        //        Color secondBestStatusColor = Common.ColorFromHex("#ffffff");
        //        Color bestStatusColor = Common.ColorFromHex("#38eaff");

        //        Color normalStatusColorSidepanel = Common.ColorFromHex("#3a7177"); //mp: this should be same as normal text color in sidemenu

                list.Add(new PresentationType()
                {
                    KeyName = "integrityPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {   
                            new Threshold(){ Edge = 0.001f, TermTooltip = "Broken",  IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                            new Threshold(){ Edge = 0.1f, TermTooltip = "Falling apart",  IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                            new Threshold(){ Edge = 0.2f, TermTooltip = "Damaged", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                            new Threshold(){ Edge = 0.99f, TermTooltip = "Good", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar }
                        }
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f, SuppressIfMaximumValue = true },
                    RightAdjustValue = true,
                    ValueRightPadding = 20 // needsBarPaddingRight

                });
               

                list.Add(new PresentationType()
                {
                    KeyName = "PartConditions",  //http://en.wikipedia.org/wiki/Decomposition#Stages_of_decomposition
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                        ///////
                        //mp feb 2015: this icon does not show up on items, pretty sure. (does it show up on structures? or parts?): Icon = "HUD_icon_status_broken"
                        ////////
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>() //
                        {
                            { "raw seafood",
                                new[]
                                { 
                        //            new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Spoiled" }, //mp feb 2015. is this stage ever seen? doesnt it instantly degrade into another (rotten) item? yes,  Edge = 0.0f is never seen! it either degrades into another item or dissappears if none is set under degrades to.                    
                                    new Threshold(){ Edge = 0.1f, Term = "About to spoil", TermTooltip ="About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = badStatusColorSidePanelBar }, // Common.ColorFromHex("#ff1a2a")},  
                                    new Threshold(){ Edge = 0.4f, Term = "Not fresh", TermTooltip ="Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar },                                    
                                    new Threshold(){ Edge = 1f, Term = "Fresh", TermTooltip ="Fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "raw meat",  //http://en.wikipedia.org/wiki/Meat_spoilage
                                new[]
                                { 
                              //      new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Spoiled"},
                                    new Threshold(){ Edge = 0.1f, Term = "About to spoil", TermTooltip ="About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = badStatusColorSidePanelBar},  
                                    new Threshold(){ Edge = 0.4f, Term = "Not fresh" , TermTooltip ="Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar  },                                    
                                    new Threshold(){ Edge = 1f, Term = "Fresh", TermTooltip ="Fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "cooked food",
                                new[]
                                { 
                              //      new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Spoiled"},
                                    new Threshold(){ Edge = 0.1f, Term = "About to spoil" , TermTooltip ="About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = badStatusColorSidePanelBar },  
                                    new Threshold(){ Edge = 0.4f, Term = "Not fresh" , TermTooltip ="Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar  },                                    
                                    new Threshold(){ Edge = 1f, Term = "Freshly cooked", TermTooltip ="Freshly cooked. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen", TermTint = normalStatusColorSidepanelBar },
                                }
                            },

/* mp feb 2015 presently not used
                            { "bread and vegetables",  //   bread: http://en.wikipedia.org/wiki/Staling
                                new[]
                                { 
                          //          new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Moldy"},
                                    new Threshold(){ Edge = 0.1f, Term = "Getting moldy" , Tint = badStatusColor},  
                                    new Threshold(){ Edge = 0.4f, Term = "Not fresh"  },                                    
                                    new Threshold(){ Edge = 1f, Term = "Fresh"},
                                }
                            },
*/
                             { "perishable",  //http://en.wikipedia.org/wiki/Biodegradation
                                new[]
                                { 
                         //           new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Decomposed"  },
                                    new Threshold(){ Edge = 0.1f, Term = "Starting to decompose", TermTooltip = "Starting to decompose. To preserve its condition, should be frozen or stored in cool, dry and dark surroundings", TermTint = badStatusColorSidePanelBar },                                                                 
                                    new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition. To preserve its condition, should be frozen or stored in cool, dry and dark surroundings", TermTint = normalStatusColorSidepanelBar },
                                }
                            },

                             { "perishable, no freeze",
                                new[]
                                { 
                       //             new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Decomposed" },
                                    new Threshold(){ Edge = 0.1f, Term = "Starting to decompose", TermTooltip = "Starting to decompose. To preserve its condition, should be stored in cool, dry and dark surroundings but NOT frozen" , TermTint = badStatusColorSidePanelBar},                                                                      
                                    new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition. To preserve its condition, should be stored in cool, dry and dark surroundings but NOT frozen", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                             { "biowaste",  //mp i do not color this red, because this is the step after rotten food, and biowaste is not inherently "bad"
                                new[]
                                { 

                                    new Threshold(){ Edge = 0.15f, Term = "Almost fully decomposed", TermTooltip = "Almost fully decomposed. Will decompose quickly in warm, humid conditions and exposed to surroundings.", TermTint = normalStatusColorSidepanelBar },                                                                 
                                    new Threshold(){ Edge = 1f, Term = "Decomposing", TermTooltip = "Decomposing. Will decompose quickly in warm, humid conditions and exposed to surroundings.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

/* mp feb 2015 presently not used.
                              { "live fish",
                                new[]
                                { 
                      //              new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Term = "Dead"  },
                                    new Threshold(){ Edge = 0.1f, Term = "Dying", Tint = badStatusColor },                                                                       
                                    new Threshold(){ Edge = 1f, Term = "Good condition"},
                                }
                            },
*/
                             { "somewhatPreserved",
                                new[]                                { 
                     
                                    new Threshold(){ Edge = 0.1f, Term = "Starting to decompose", TermTooltip = "Starting to decompose. This item has an increased shelf life, but it will not keep indefinitely if stored at room temperature." , TermTint = badStatusColorSidePanelBar},                                                                      
                                    new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition. This item has an increased shelf life, but it will not keep indefinitely if stored at room temperature.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "stored dry",
                                new[]
                                { 
                      //            new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Tint = badStatusColor, Term = "Decayed" },
                                    new Threshold(){ Edge = 0.1f, Term = "Decaying", TermTooltip = "Decaying. This item lasts longest under dry and dark conditions.", TermTint = badStatusColorSidePanelBar  },                                                                       
                                    new Threshold(){ Edge = 1f, Term = "Good condition" , TermTooltip = "Good condition. This item will last long under dry and dark conditions.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "pickledFood",
                                new[]
                                { 
                      //            new Threshold(){ Edge = 0.0f, Icon = "HUD_icon_status_broken", Tint = badStatusColor, Term = "Decayed" },
                                    new Threshold(){ Edge = 0.1f, Term = "Decaying", IconTint = badStatusColor, TermTooltip = "Decaying. This item lasts longest under dark conditions.", TermTint = badStatusColorSidePanelBar  },                                                                       
                                    new Threshold(){ Edge = 1f, Term = "Good condition" , TermTooltip = "Good condition. This item lasts for considerable time under dark conditions.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },                            

                            { "wetDecay", //blackpowder
                                new[]
                                {
                                   
                                    new Threshold(){ Edge = 0.1f, Term = "Almost decayed", TermTooltip = "Almost decayed. This item lasts longest under dry conditions." , TermTint = badStatusColorSidePanelBar },
                                    new Threshold(){ Edge = 0.4f, Term = "Starting to decay", TermTooltip = "Starting to decay. This item lasts longest under dry conditions.", TermTint = normalStatusColorSidepanelBar  },
                                    new Threshold(){ Edge = 1f, Term = "Good condition" , TermTooltip = "Good condition. This item lasts longest under dry conditions.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            
                            { "carcassDecomposing",
                                new[]
                                {
                                    new Threshold(){ Edge = 0.25f, Term = "Heavily decomposed", TermTooltip = "Heavily decomposed. Decomposition happens quicker under hot/humid conditions.", TermTint = badStatusColorSidePanelBar  },
                                    new Threshold(){ Edge = 0.75f, Term = "Somewhat decomposed", TermTooltip = "Somewhat decomposed. Decomposition happens quicker under hot/humid conditions.", TermTint = normalStatusColorSidepanelBar  },
                                    new Threshold(){ Edge = 1f, Term = "Fresh" , TermTooltip = "Fresh. Decomposition happens quicker under hot/humid conditions.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },
                                 { "decomposedFood",  //mp: for decomposed food items, therefore I use bad status color. but also  used for decomposed enzyme, so dont mention food in tooltip
                                new[]
                                { 
                                    
                                    new Threshold(){ Edge = 0.5f, Term = "Heavily decomposed", TermTint = badStatusColorSidePanelBar, TermTooltip = "Heavily decomposed"  },                                                                 
                                    new Threshold(){ Edge = 1f, Term = "Somewhat decomposed", TermTint = badStatusColorSidePanelBar, TermTooltip = "Somewhat decomposed"  },
                                }
                            },

                            { "sun drying",
                                new[]
                                { 
                              //      new Threshold(){ Edge = 0.0f, Term = "Completely dried" },  
                                    new Threshold(){ Edge = 0.1f, Term = "Almost fully dried", TermTooltip = "Almost fully dried. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Somewhat dried", TermTooltip = "Somewhat dried. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar },                                    
                                    new Threshold(){ Edge = 1f, Term = "Moist" , TermTooltip = "Moist. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "fastDrying",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Almost fully dried", TermTooltip = "Almost fully dried. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Somewhat dried", TermTooltip = "Somewhat dried. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar },                                    
                                    new Threshold(){ Edge = 1f, Term = "Moist" , TermTooltip = "Moist. This item should be sun dried by placing it in a dry place with lots of light.", TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                            { "enzyme",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.05f, Term = "Degraded", TermTooltip ="Degraded. The chemical degrades quickly and should be used immediately", TermTint = badStatusColorSidePanelBar },                                 
                                    new Threshold(){ Edge = 0.4f, Term = "Somewhat degraded", TermTooltip ="Somewhat degraded. The chemical degrades quickly and should be used immediately", TermTint = badStatusColorSidePanelBar}, 
                                  
                                    new Threshold(){ Edge = 1f, Term = "Fresh", TermTooltip ="Fresh. The chemical degrades quickly and should be used immediately", TermTint = normalStatusColorSidepanelBar},
                                }
                            },


                            
                             { "proneToInfestation",  //http://www.birdmites.org/infested.html
                                new[]
                                { 
                                                                         // "Covered with scuttlers"
                            //        new Threshold(){ Edge = 0.0f, Term = "Eaten up by scuttlers", Icon = "HUD_icon_status_broken", Tint = badStatusColor }, 
                                    new Threshold(){ Edge = 0.17f, Term = "Scuttler infested", TermTooltip ="Scuttler infested. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.", TermTint = badStatusColorSidePanelBar}, 
                                                                             //"Getting overrun with scuttlers"
                                    new Threshold(){ Edge = 0.5f, Term = "Some scuttler bugs", TermTooltip ="Some scuttler bugs. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.", TermTint = normalStatusColorSidepanelBar },                                    
                                    new Threshold(){ Edge = 1f, Term = "Good condition" , TermTooltip ="Good condition. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.", TermTint = normalStatusColorSidepanelBar },
                                }
                            },


                            { "remainsOfSpoakLeaves",  //the item that spoak leaves degrade to after infestation has this degrade profile.
                                new[]
                                {   
                                     new Threshold(){ Edge = 0f, Term = "Eaten up by scuttlers" , TermTooltip ="Eaten up by scuttlers. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.", Icon = "HUD_icon_status_broken", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },                           
                                    new Threshold(){ Edge = 1f, Term = "Eaten up by scuttlers" , TermTooltip ="Eaten up by scuttlers. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.", Icon = "HUD_icon_status_broken", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                                }
                            },


                               { "dirt",  // http://en.wikipedia.org/wiki/Weathering
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Weathered", TermTint = badStatusColorSidePanelBar, TermTooltip ="Weathered" },                             
                                    new Threshold(){ Edge = 1f, Term = "", TermTint = normalStatusColorSidepanelBar},//This will allow the stones to be shown as a part even though their condition will not be described
                                }
                                },
                                
                               { "neverDegrades",  
                                new[]
                                {                                        
                                    new Threshold(){ Edge = 0.99f, Term = "Signs of decay", TermTooltip = "Signs of decay", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },                                 
                                    new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip ="Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar,  },   
                                 }
                                },
                              { "ricketyConstruction",  //http://en.wikipedia.org/wiki/Home_repair
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "In ruins", IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar  },
                                    new Threshold(){ Edge = 0.1f, Term = "About to fall down", IconTint = badStatusColor, TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar},
                                    new Threshold(){ Edge = 0.2f, Term = "Just holding together", IconTint = normalStatusColorSidepanel, TermTooltip = "Just holding together",  TermTint = badStatusColorSidePanelBar },
                                    new Threshold(){ Edge = 0.4f, Term = "Signs of decay", TermTooltip = "Signs of decay", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },
                                    new Threshold(){ Edge = 1f /*  0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },//new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition", Tint = normalStatusColorSidepanel},
                                }
                            },

                             { "adequateConstruction",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "In ruins" , IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar  },
                                    new Threshold(){ Edge = 0.1f, Term = "About to fall down", IconTint = badStatusColor, TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar },
                                    new Threshold(){ Edge = 0.2f, Term = "Just holding together", TermTooltip = "Just holding together", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },
                                    new Threshold(){ Edge = 0.4f, Term = "Signs of decay", TermTooltip = "Signs of decay", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar  },                            
                                    new Threshold(){ Edge = 1f /* 0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar},//new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition", Tint = normalStatusColorSidepanel },
                                }
                            },

                             { "sturdyConstruction",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "In ruins", IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar  },
                                    new Threshold(){ Edge = 0.1f, Term = "About to fall down", IconTint = badStatusColor, TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar},
                                    new Threshold(){ Edge = 0.2f, Term = "Just holding together", TermTooltip = "Just holding together", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },
                                    new Threshold(){ Edge = 0.4f, Term = "Signs of decay", TermTooltip = "Signs of decay", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },                         
                                    new Threshold(){ Edge = 1f /* 0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar},
                                }
                            },
                             { "advancedConstruction",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "In ruins", IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar  },
                                    new Threshold(){ Edge = 0.1f, Term = "About to fall down", IconTint = badStatusColor, TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar},
                                    new Threshold(){ Edge = 0.2f, Term = "Just holding together", TermTooltip = "Just holding together", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },
                                    new Threshold(){ Edge = 0.4f, Term = "Signs of decay", TermTooltip = "Signs of decay", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },                         
                                    new Threshold(){ Edge = 1f /* 0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar},
                                }
                            },

                             { "improvisedEquipment",  //http://en.wikipedia.org/wiki/Wear    http://thesaurus.com/browse/run-down
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "Broken" , IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar },
                                    new Threshold(){ Edge = 0.1f, Term = "About to break down", IconTint = badStatusColor, TermTooltip = "About to break down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar },
                                    new Threshold(){ Edge = 0.2f, Term = "Heavy wear and tear", TermTooltip = "Heavy wear and tear", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar  },  //getting worn-out
                                    new Threshold(){ Edge = 0.5f, Term = "Showing signs of use", TermTooltip = "Showing signs of use", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar  },
                                    new Threshold(){ Edge = 1f /* 0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar},//new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition"},
                                }
                            },

                            { "equipment",  // polymers http://en.wikipedia.org/wiki/Polymer_degradation
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.0f, Term = "Broken", IconTint = badStatusColor, Icon = "HUD_icon_status_broken", TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar   },
                                    new Threshold(){ Edge = 0.1f, Term = "About to break down", IconTint = badStatusColor, TermTooltip = "About to break down. It may sometimes be possible to salvage and reuse its component parts", TermTint = badStatusColorSidePanelBar }, 
                                    new Threshold(){ Edge = 0.2f, Term = "Heavy wear and tear", TermTooltip = "Heavy wear and tear", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },  // beat-up
                                    new Threshold(){ Edge = 0.5f, Term = "Showing signs of use", TermTooltip = "Showing signs of use", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar },
                                    new Threshold(){ Edge = 1f /* 0.99f*/, Term = "Good condition", TermTooltip = "Good condition", IconTint = normalStatusColorSidepanel, TermTint = normalStatusColorSidepanelBar},// new Threshold(){ Edge = 1f, Term = "Good condition", TermTooltip = "Good condition"},
                                }
                            }
                        }
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f, SuppressIfMaximumValue = false /* true*/ },                    
                    RightAdjustValue = true,
                    ValueRightPadding = 20 // needsBarPaddingRight
                });

             

                list.Add(new PresentationType()
                {
                    KeyName = "energyUsePresentation",  //category name defined just below this section
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.4f, Term = "LOW" },
                            new Threshold(){ Edge = 0.7f, Term = "MEDIUM"}, 
                            new Threshold(){ Edge = 1.0f, Term = "HIGH" },
                        }
                    },
                });



                float skillEdge0 = 0.2f;
                float skillEdge1 = 0.4f;
                float skillEdge2 = 0.8f;
                float skillEdge3 = 0.95f;
                float skillEdge4 = 1;

                list.Add(new PresentationType()
                {
                    KeyName = "skillPresentationStatusIcons",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {   new Threshold(){ Edge = skillEdge0, Term = "Only basic knowledge", Icon = "HUD_icon_productivity_person" , IconTint = badStatusColorSidePanelBar },  // http://en.wikipedia.org/wiki/Dreyfus_model_of_skill_acquisition
                            new Threshold(){ Edge = skillEdge1, Term = "Some skill" }, 
                            new Threshold(){ Edge = skillEdge2, Term = "Competent" },
                            new Threshold(){ Edge = skillEdge3, Term = "Highly skilled", Icon = "HUD_icon_productivity_person" , IconTint = secondBestStatusColor },  
                            new Threshold(){ Edge = skillEdge4, Term = "Expert", Icon = "HUD_icon_productivity_person" , IconTint = bestStatusColor }
                        }
                    },
                });

                list.Add(new PresentationType()
                { 
                    KeyName = "skillPresentationSidePanel", 
                    ValueTooltipTextFormatting = new TextFormatting()
                    {                       
                        NumberFormatString = "F2", 

                        TextWithPlaceholders = "Skill value {1}"
                    },
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[] 
                        {  
                            new Threshold(){ Edge = 0.1f,  TermTint = badStatusColorSidePanelBar },    
                            new Threshold(){ Edge = 1f,  TermTint = normalStatusColorSidepanelBar},                           
                        }
                    },                   
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                    BarPresentation = new BarPresentation()
                    {                        
                         MaxValue = 1f
                    }
                    /*DefaultPresentation = new DefaultPresentation()
                    {
                        
                        Thresholds = new[]
                        {   new Threshold(){ Edge = skillEdge0, Term = "Only basic knowledge" },  // http://en.wikipedia.org/wiki/Dreyfus_model_of_skill_acquisition
                            new Threshold(){ Edge = skillEdge1, Term = "Some skill" }, 
                            new Threshold(){ Edge = skillEdge2, Term = "Competent" },
                            new Threshold(){ Edge = skillEdge3, Term = "Highly skilled" },  
                            new Threshold(){ Edge = skillEdge4, Term = "Expert" }
                        }
                    },*/
                });

                list.Add(new PresentationType()
                {
                    KeyName = "emigrationRiskPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {   
                            new Threshold(){ Edge = -0.01f, Icon = "lcd_icon_noEntry", Term = "NONE", IconTint = badStatusColor, UseValueTextFormatting = false, UseValueTooltipFormatting = false,
                                TermTooltip = "Will not emigrate right now." /* is overridden */ },                             
                            new Threshold(){ Edge = 1f }
                        }
                    },
                    ValueTextFormatting = new TextFormatting()
                    {
                         NumberFactor = 100f,
                         NumberFormatString = "F0",
                         TextWithPlaceholders = "{1} %"
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "happinessPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {                       
                          Thresholds = new[]
                          {   new Threshold(){ Edge = -0.01f, Term = "Unhappy", Icon = "lcd_icon_smiley_unhappy" , IconTint = badStatusColor, TermTint = badStatusColor, TermTooltip = "Unhappy characters have a higher emigration risk" }, //mp: when decision points implemented, change to: "Unhappy characters will have a higher emigration risk but will also work to improve the conditions they are unhappy about by creating decision points."     
                             // new Threshold(){ Edge = 0f, Term = "Content", Icon = "lcd_icon_smiley_content", IconTint = normalStatusColorSidepanel, TermTooltip = "Content characters have less emigration risk" },
                              new Threshold(){ Edge = 1f, Term = "Happy", Icon = "lcd_icon_smiley_happy", IconTint = normalStatusColorSidepanel, TermTooltip = "Happy characters have less emigration risk" }
                          }
                    }
                });

               
                float lowEnergyEdge = 0.2f;
                float almostNoEnergyEdge = 0.4f;
                float normalEnergyEdge1 = 0.6f;
                float normalEnergyEdge2 = 0.98f;
                float fullEnergy = 1f;

                //use the word average or medium or moderate???????? http://www.thefreedictionary.com/average ..." Average and medium apply to what is midway between extremes and imply both sufficiency and lack of distinction"
            //question: in tooltips, should we strive to use same words in the needs scale that feed into overall energy?

               
                list.Add(new PresentationType()
                {
                    KeyName = "energyPresentationSidePanel",  //category name defined just below this section                    
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  // for the sidepanel, don't repeat the caption (energy) in the status 
                            new Threshold(){ Edge = lowEnergyEdge, Term = "Low", TermTooltip = "The character has low energy. This is a result of bad nutrition and/or lack of sleep", Icon = "HUD_icon_status_sun", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                            new Threshold(){ Edge = almostNoEnergyEdge, Term = "Reduced", TermTooltip = "The character has reduced energy. This is a result of insufficient nutrition and/or sleep", Icon = "HUD_icon_status_sun", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar },  
                            new Threshold(){ Edge = normalEnergyEdge1, Term = "Average", TermTooltip = "The character has average energy. This is a result of average nutrition and sleep", Icon = "HUD_icon_status_sun", IconTint = normalStatusColorSidepanel, TermTint = energyBarColor },
                            new Threshold(){ Edge = normalEnergyEdge2, Term = "Average", TermTooltip = "The character has average energy. This is a result of average nutrition and sleep", Icon = "HUD_icon_status_sun", IconTint = normalStatusColorSidepanel, TermTint = energyBarColor },
                            new Threshold(){ Edge = fullEnergy, Term = "High", TermTooltip = "The character has high energy. This is a result of sufficient nutrition and sleep", Icon = "HUD_icon_status_sun", IconTint = normalStatusColorSidepanel, TermTint = energyBarColor},
                        }
                    },
                    BarPresentation = new BarPresentation()
                    {                       
                        MaxValue = 1f
                    },
                    ValueRightPadding = 55, 
                    RightAdjustValue = true
                });

                list.Add(new PresentationType()
                {
                    KeyName = "energyPresentationStatusIcons",  
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = lowEnergyEdge, Term = "Low energy", Icon = "HUD_icon_status_sun", IconTint = badStatusColor},
                            new Threshold(){ Edge = almostNoEnergyEdge, Term = "Reduced energy", Icon = "HUD_icon_status_sun", IconTint = lessBadStatusColor },  
                            new Threshold(){ Edge = normalEnergyEdge1, Term = null },
                            new Threshold(){ Edge = normalEnergyEdge2, Term = null },
                            new Threshold(){ Edge = fullEnergy, Term = "High energy", Icon = "HUD_icon_status_sun", IconTint = bestStatusColor},
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "entityFunctionalStatusIcons", 
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        /*Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0f, Term = "This item or structure is broken", Icon = "HUD_icon_status_broken", Tint = badStatusColor},
                            new Threshold(){ Edge = 1f, Term = null }                            
                        }*/
                        Thresholds = new[] // GetEntityIsNotFunctional()
                        {  
                            new Threshold(){ Edge = 0, Term = "This item or structure is broken", Icon = "HUD_icon_status_broken", IconTint = badStatusColor },
                            new Threshold(){ Edge = 1, Term = null}, 
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "progressStatusIcons",
                    ValueTooltipTextFormatting = new TextFormatting()
                    {
                        NumberFactor = 100f, // show as percent instead of decimals
                        NumberFormatString = "F0",  // no decimals

                        TextWithPlaceholders = "Production progress {1} %"
                    },
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[] 
                        {  
                            new Threshold(){ Edge = 1f, TermTint = ProgressColor /*  IconTint = ProgressColor*/},                           
                        }
                    },
                    BarPresentation = new BarPresentation()
                    {
                        SuppressIfMaximumValue = true,
                        MaxValue = 1f,                      
                    }

                });

                list.Add(new PresentationType()
                {
                    KeyName = "consumeProgressStatusIcons",
                    ValueTooltipTextFormatting = new TextFormatting()
                    {
                        NumberFactor = 100f, // show as percent instead of decimals
                        NumberFormatString = "F0",  // no decimals

                        TextWithPlaceholders = "Consume progress {1} %"
                    },
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[] 
                        {  
                            new Threshold(){ Edge = 1f, TermTint = ConsumeProgressColor },                           
                        }
                    },
                    BarPresentation = new BarPresentation()
                    {
                        SuppressIfMaximumValue = true,
                        MaxValue = 1f,
                    }

                });

                const int needsBarPaddingRight = 10;

                list.Add(new PresentationType()
                {
                    KeyName = "micronutrientsPresentation",  //category name defined just below this section  http://en.wikipedia.org/wiki/Micronutrient
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                       /* Thresholds = new[] // i made these terms on a hunch. not symmetric with calorie because calorie deficiency causes death so is more urgent for the player.
                        {  
                            new Threshold(){ Edge = 0.05f, Term = "Low Intake", TermTooltip = "Person has such a low intake of micronutrients that it can lead to starvation", Tint = badStatusColor}, //"Signs of micronutrient deficiency"// was: Severe micronutrient deficiency //maybe put a header: micronutrients to save space                            
                            new Threshold(){ Edge = 0.3f, Term = "Inadequate Intake" , TermTooltip = "Inadequate Intake. Person must raise intake of micronutrients to remain healthy", Tint = lessBadStatusColor}, //"Lacks some micronutrients"
                            new Threshold(){ Edge = 0.8f, Term = "Adequate Intake", TermTooltip = "Adequate Intake. Person has an adequate, but below recommended intake of micronutrients", Tint = normalStatusColorSidepanel},
                            new Threshold(){ Edge = 1f, Term = "High Intake" , TermTooltip = "Person has the recommended intake of micronutrients", Tint = normalStatusColorSidepanel },
                        }*/
                        Thresholds = new[] // i made these terms on a hunch. not symmetric with calorie because calorie deficiency causes death so is more urgent for the player.
                        {  
                            new Threshold(){ Edge = 0.05f, TermTooltip = "Low intake. Person has such a low intake of micronutrients that it can lead to starvation", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar}, //"Signs of micronutrient deficiency"// was: Severe micronutrient deficiency //maybe put a header: micronutrients to save space                            
                            new Threshold(){ Edge = 0.3f, TermTooltip = "Inadequate intake. Person must raise intake of micronutrients to remain healthy", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar}, //"Lacks some micronutrients"
                            new Threshold(){ Edge = 0.65f, TermTooltip = "Adequate intake. Person has an adequate, but below recommended intake of micronutrients", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor},
                            new Threshold(){ Edge = 1f, TermTooltip = "High intake. Person has the recommended intake of micronutrients", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor },
                        }
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = needsBarPaddingRight
                });
                list.Add(new PresentationType()
                {
                    KeyName = "proteinPresentation",  //category name defined just below this section
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.05f, TermTooltip = "Low intake. Person has such a low protein intake that it can lead to starvation", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar},// was: Severe protein deficiency //maybe put a header: protein to save space                          
                            new Threshold(){ Edge = 0.3f, TermTooltip = "Inadequate intake. Person must raise protein intake to remain healthy", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar }, //"Lacks protein"
                            new Threshold(){ Edge = 0.65f, TermTooltip = "Adequate intake. Person has an adequate, but below recommended protein intake", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor},
                            new Threshold(){ Edge = 1f, TermTooltip = "High intake. Person has the recommended protein intake", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor},
                        }
                        /*
                        Thresholds = new[]
                        {  
                            new Threshold(){ Edge = 0.05f, Term = "Low Intake", TermTooltip = "Person has such a low protein intake that it can lead to starvation", Tint = badStatusColor},// was: Severe protein deficiency //maybe put a header: protein to save space                          
                            new Threshold(){ Edge = 0.3f, Term = "Inadequate Intake", TermTooltip = "Person must raise protein intake to remain healthy", Tint = lessBadStatusColor }, //"Lacks protein"
                            new Threshold(){ Edge = 0.8f, Term = "Adequate Intake", TermTooltip = "Person has an adequate, but below recommended protein intake", Tint = normalStatusColorSidepanel},
                            new Threshold(){ Edge = 1f, Term = "High Intake" , TermTooltip = "Person has the recommended protein intake", Tint = normalStatusColorSidepanel},
                        }*/
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = needsBarPaddingRight
                });
                list.Add(new PresentationType()
                {
                    KeyName = "foodEnergyPresentation",  //category name defined just below this section  //http://www.merckmanuals.com/home/disorders_of_nutrition/undernutrition/undernutrition.html  http://en.wikipedia.org/wiki/Malnutrition#Signs   http://en.wikipedia.org/wiki/Starvation
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        // as of nov 25, this is the only one that can cause death
                        Thresholds = new[]  // this one takes into account the amount of time at zero. 0.5f means that character just reached zero. after that, it's number of days at zero
                        {  
                           new Threshold(){ Edge = 0.15f, TermTooltip = "Very low intake. Person has such a low calorie intake that it can lead to starvation and death", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar }, //"Severe calorie deficiency"
                            new Threshold(){ Edge = 0.3f, TermTooltip = "Low intake. Person has such a low calorie intake that it can lead to starvation", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar },//"Signs of calorie deficiency"
                            new Threshold(){ Edge = 0.5f, TermTooltip = "Inadequate intake. Person must raise calorie intake to remain healthy", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor }, //"Inadequate calorie intake"
                            new Threshold(){ Edge = 0.7f, TermTooltip = "Adequate intake. Person has an adequate, but below recommended calorie intake", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor },
                            new Threshold(){ Edge = 1f, TermTooltip = "High intake. Person has the recommended calorie intake", IconTint = normalStatusColorSidepanel, TermTint = subNutritionBarColor},
                        }
                       /* Thresholds = new[]  // this one takes into account the amount of time at zero. 0.5f means that character just reached zero. after that, it's number of days at zero
                        {  
                           new Threshold(){ Edge = 0.15f, Term = "Very Low Intake" , TermTooltip = "Person has such a low calorie intake that it can lead to starvation and death", Tint = badStatusColor }, //"Severe calorie deficiency"
                            new Threshold(){ Edge = 0.3f, Term = "Low Intake", TermTooltip = "Person has such a low calorie intake that it can lead to starvation", Tint = lessBadStatusColor },//"Signs of calorie deficiency"
                            new Threshold(){ Edge = 0.5f, Term = "Inadequate Intake", TermTooltip = "Person must raise calorie intake to remain healthy", Tint = normalStatusColorSidepanel }, //"Inadequate calorie intake"
                            new Threshold(){ Edge = 0.8f, Term = "Adequate Intake", TermTooltip = "Person has an adequate, but below recommended calorie intake", Tint = normalStatusColorSidepanel },
                            new Threshold(){ Edge = 1f, Term = "High Intake" , TermTooltip = "Person has the recommended calorie intake", Tint = normalStatusColorSidepanel},
                        }*/
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = needsBarPaddingRight
                });



                string couldUseStimulantTooltipPerson = "The person would like to enjoy a stimulant. This would improve their happiness and the colony's comfort conditions";
                string satisfiedStimulantTooltipPerson = "The person has enjoyed a stimulant. This improves their happiness and the colony's comfort conditions";

                list.Add(new PresentationType()
                {
                    KeyName = "stimulantsPresentation",  
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {                              
                            new Threshold(){ Edge = 0.3f, Term = "Could use one", TermTooltip = couldUseStimulantTooltipPerson, IconTint = normalStatusColorSidepanel }, 
                            new Threshold(){ Edge = 1f, Term = "Satisfied", TermTooltip = satisfiedStimulantTooltipPerson, IconTint = normalStatusColorSidepanel }, //mp was Term = null,
                        }
                    },
                });


                string starvingTooltipPerson = "Starving. Person has (or has recently had) insufficient intake of calories, protein or micronutrients. Starving impacts a person's energy and can lead to death. \nNOTE: The effect of starvation can linger for some time after the person ingests food again."; //"Starving. Person is starving because of insufficient intake of one or more of the 3 nutrient groups: calories, protein or micronutrients. Starving has a negative impact on a person's energy and can lead to death in the long term.";
            string severelyUndernourishedTooltipPerson = "Severely undernourished. Person has insufficient intake of one or more of the 3 nutrient groups: calories, protein or micronutrients.";
            string undernourishedTooltipPerson = "Undernourished. Person has an inadequate intake of one or more of the 3 nutrient groups: calories, protein or micronutrients.";
            string adequatelyNourishedPerson = "Adequately nourished. Person has an adequate, but below recommended, intake of the 3 nutrient groups: calories, protein or micronutrients.";

            string starvingTooltipDefault = "Starving. The person or animal is starving because of insufficient food intake. Starving has a negative impact on a character's energy and can lead to death in the long term.";
            string severelyUndernourishedTooltipDefault = "Severely undernourished. The character has insufficient food intake.";
            string undernourishedTooltipDefault = "Undernourished. The character has an inadequate food intake.";
            string adequatelyNourishedDefault = "Adequately nourished. The character has an adequate, but below recommended, food intake.";


            string nutritionTerm0 = "Starving";
            string nutritionTerm1 = "Severely undernourished";

            float nutritionEdge0 = 0.5f;
            float nutritionEdge1 = 0.6f;

            list.Add(new PresentationType()
                {
                    KeyName = "hungerPresentationSidePanel",  //category name defined just below this section
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        { 
                            //Always put the thresholds from lowest edge value to highest. (means that term will go from the specified edge and DOWN)
                            { "entity:human",
                                new[]
                                {  //an equally weighted sum of all food needs (are custom weights needed?) the names do not match vitamin deficiency...
                                    // value means (on average):
                                    // 1: fully sated
                                    // 0.5 means, on average, all food needs are at starvation edge, but starvation may already have begun for some
                                    // 0 means all needs are at death level!

                                    //TODO: truncate words with ... and make tooltip repeat word http://unclaimed.planship.com/issues/198

                                 new Threshold(){ Edge = nutritionEdge0, Term = nutritionTerm0, TermTooltip = starvingTooltipPerson, Icon = "HUD_icon_status_hunger", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar }, //http://en.wikipedia.org/wiki/Malnutrition  http://en.wikipedia.org/wiki/Malnutrition#Definition  
                                new Threshold(){ Edge = nutritionEdge1, Term = nutritionTerm1, TermTooltip = severelyUndernourishedTooltipPerson, Icon = "HUD_icon_status_hunger", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar }, // 0 - 0.2  //was "Ravenously hungry"
                                new Threshold(){ Edge = 0.7f, Term = "Undernourished", TermTooltip = undernourishedTooltipPerson, Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor }, // 0.2 - 0.35 //was "Hungry"
                                new Threshold(){ Edge = 0.8f, Term = "Adequately nourished", TermTooltip = adequatelyNourishedPerson, Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor },// 0.35 - 0.5    //was "Getting peckish"                        
                                new Threshold(){ Edge = 1f, Term = "Fully nourished", TermTooltip = "Fully nourished. Person has the recommended intake of the 3 nutrient groups: calories, protein or micronutrients.", Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor},// 0.75 - 0.9 //sated                        
                                }
                            }
                        }
                    },
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                                {  
                                 new Threshold(){ Edge = nutritionEdge0, Term = nutritionTerm0, TermTooltip = starvingTooltipDefault, Icon = "HUD_icon_status_hunger", IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar },
                                new Threshold(){ Edge = nutritionEdge1, Term = nutritionTerm1, TermTooltip = severelyUndernourishedTooltipDefault, Icon = "HUD_icon_status_hunger", IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar }, 
                                new Threshold(){ Edge = 0.7f, Term = "Undernourished", TermTooltip = undernourishedTooltipDefault, Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor }, 
                                new Threshold(){ Edge = 0.8f, Term = "Adequately nourished", TermTooltip = adequatelyNourishedDefault, Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor },              
                                new Threshold(){ Edge = 1f, Term = "Fully nourished", TermTooltip = "Fully nourished. The character has the recommended food intake.", Icon = "HUD_icon_status_hunger", IconTint = normalStatusColorSidepanel, TermTint = nutritionBarColor}                   
                                }

                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = needsBarPaddingRight
                });

                list.Add(new PresentationType()
                {
                    KeyName = "hungerPresentationStatusIcons",  //category name defined just below this section
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        { 
                            //Always put the thresholds from lowest edge value to highest. (means that term will go from the specified edge and DOWN)
                            { "entity:human",
                                new[]
                                {  //an equally weighted sum of all food needs (are custom weights needed?) the names do not match vitamin deficiency...
                                    // value means (on average):
                                    // 1: fully sated
                                    // 0.5 means, on average, all food needs are at starvation edge, but starvation may already have begun for some
                                    // 0 means all needs are at death level!

                                    //TODO: truncate words with ... and make tooltip repeat word http://unclaimed.planship.com/issues/198

                                 new Threshold(){ Edge = nutritionEdge0, Term = nutritionTerm0, TermTooltip = starvingTooltipPerson, Icon = "HUD_icon_status_hunger", IconTint = badStatusColor}, //http://en.wikipedia.org/wiki/Malnutrition  http://en.wikipedia.org/wiki/Malnutrition#Definition  
                                new Threshold(){ Edge = nutritionEdge1, Term = nutritionTerm1, TermTooltip = severelyUndernourishedTooltipPerson, Icon = "HUD_icon_status_hunger", IconTint = lessBadStatusColor },                                    
                                new Threshold(){ Edge = 1f}                        
                                }
                            }
                        }
                    }, 
                    NumberThresholdPresentation = new NumberThresholdPresentation() // Dogs etc.
                    {
                        Thresholds = new[] 
                        { 
                             new Threshold(){ Edge = nutritionEdge0, Term = nutritionTerm0, TermTooltip = starvingTooltipDefault, Icon = "HUD_icon_status_hunger", IconTint = badStatusColor},
                                new Threshold(){ Edge = nutritionEdge1, Term = nutritionTerm1, TermTooltip = severelyUndernourishedTooltipDefault, Icon = "HUD_icon_status_hunger", IconTint = lessBadStatusColor },                                    
                                new Threshold(){ Edge = 1f}     
                        }
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "moraleLevelPresentation",

                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        { 
                            //We will only use this presentation for agents. Not Sensors. MENTAL STATE    MP removed ACUTE STRESS because it lasts for several days according to wikipedia. we only want short panic http://en.wikipedia.org/wiki/Panic_attack   http://en.wikipedia.org/wiki/Acute_stress_reaction
                            { "entity:human",
                                new[]
                                { 
                                 new Threshold(){ Edge = (int)ThreatStance.Cautious, Term = "LOW!", IconTint = badStatusColor, TermTooltip = "Low morale makes this person anxious towards danger. In severe cases the person can panick, especially when suffering physical injuries"}, //Term = "PANICKING", Tint = badStatusColor, TermTooltip = "Person is suffering a panic attack. This causes debilitation and impaired psychological functioning. Can last up to several hours."
                                  
                                 new Threshold(){ Edge = (int)ThreatStance.Normal, Term = "High", IconTint = normalStatusColorSidepanel, TermTooltip = "High morale makes this person able to carry out dangerous tasks, IF not suffering from physical injuries"}, //oct 13 "Person is calm and composed"  mp: was Balanced . using this below instead
                                  
                                }  
                            }
                        }
                    },
                });

                
              
                list.Add(new PresentationType()
                {
                    KeyName = "threatStancePresentation",

                    TypeDependentPresentation = new TypeDependentPresentation()
                   {
                       ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        { 
                            //We will only use this presentation for agents. Not Sensors.  
                            { "entity:human",
                                new[]
                                {  //jan 2015: color tinting did not work on anxious!  //mp jan 2015 when red tinting works, we don't need such a strong phrasing of the word..
                                 new Threshold(){ Edge = (int)ThreatStance.Cautious, Term = "Cautious" /* "ANXIOUS!"*/, IconTint = badStatusColor, TermTooltip = "Person will stay far away from any sort of danger. (Behavior usually caused by low morale and/or physical injuries)"}, //MP oct 13 now that the above has been changed to morale, i'll write fearful here, possibly anxious ..or hesitant?    //oct 9 "Cautious". I do not want to write anxious because it contradicts Mental state: calm (above) ... http://en.wikipedia.org/wiki/Anxiety 
                                 new Threshold(){ Edge = (int)ThreatStance.Normal, Term = "Vigilant", IconTint = normalStatusColorSidepanel, TermTooltip = "In this stance, a person will keep a moderate distance to danger."}, //oct 13 "Balanced", TermTooltip = "Person has a balanced mood and will go near danger with appropriate watchfulness"
                                 new Threshold(){ Edge = (int)ThreatStance.Bold, Term = "Fearless", IconTint = normalStatusColorSidepanel,  TermTooltip = "Person is willing to enter dangerous areas to fight or do necessary work."}, //mp was Bold  . but this is to ambiguous, not only related to physical danger also people etc.
                                }
                            }
                        }
                   },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "foodItemNutritionPresentation",
                     NumberThresholdPresentation = new NumberThresholdPresentation()
                     {
                          Thresholds = new[]
                          { 
                              new Threshold() { Term = "Low", IconTint = badStatusColor , Edge = 0.3f  }, //, Edge = 0.5f  },
                              new Threshold() { Term = "Medium", Edge = 0.6f  }, //Edge = 0.7f  },
                              new Threshold() { Term = "High", Edge = 0.9f  }   //  , Edge = 1f  }                           
                          }
                     },                     
                     ValueTooltipTextFormatting = new TextFormatting()
                     {
                         NumberFormatString = "F2",  //"N", // decimal, should be integer                          
                         TextWithPlaceholders = "{1}" //mp do not use BULK as unit here. micronutrients are very low size numbers you know...for now, I do not use any unit. TODO: use a term like High (0.5 BLK) : "{0} ({1} BLK)"
                     }
                });

               /* list.Add(new PresentationType()
                {
                    KeyName = "ratingPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        NumberSource = NumberSource.Difference,
                        Thresholds = new[]
                          { 
                              new Threshold() { Term = "Happy", TermTint = normalStatusColorSidepanelBar, IconTint = normalStatusColorSidepanel, Icon = "lcd_icon_nutrition", Edge = -0.01f  }, 
                              new Threshold() { Term = "Content", TermTint = normalStatusColorSidepanelBar, IconTint = normalStatusColorSidepanel, Icon = "lcd_icon_nutrition", Edge = 0f  }, 
                              new Threshold() { Term = "Unhappy", TermTint = badStatusColorSidePanelBar, IconTint = badStatusColor, Icon = "lcd_icon_nutrition", Edge = 2f  } 
                          }
                    },
                     BarPresentation = new BarPresentation()
                     {
                          MaxValue = 1f,
                          SuppressIfMaximumValue = false,  
                          Width = 60
                     },
                    RightAdjustValue = true, 
                    ValueRightPadding = 20,
                });*/

                list.Add(new PresentationType()
                {
                    KeyName = "foodHappinessPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        NumberSource = NumberSource.Difference,
                        Thresholds = new[]
                          { 
                              new Threshold() { Term = "Happy", TermTint = normalStatusColorSidepanelBar, IconTint = foodColor, Icon = "lcd_icon_nutrition", Edge = -0.01f  }, 
                              new Threshold() { Term = "Content", TermTint = normalStatusColorSidepanelBar, IconTint = foodColor, Icon = "lcd_icon_nutrition", Edge = 0f  }, 
                              new Threshold() { Term = "Unhappy", TermTint = badStatusColorSidePanelBar, IconTint = foodColor, Icon = "lcd_icon_nutrition", Edge = 2f  } 
                          }
                    },
                    BarPresentation = new BarPresentation()
                    {
                        MaxValue = 1f,
                        SuppressIfMaximumValue = false,
                        Width = 60
                    },
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                });

                list.Add(new PresentationType()
                {
                    KeyName = "securityHappinessPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        NumberSource = NumberSource.Difference,
                        Thresholds = new[]
                          { 
                              new Threshold() { Term = "Happy", TermTint = normalStatusColorSidepanelBar, IconTint = securityColor, Icon = "lcd_icon_security", Edge = -0.01f  }, 
                              new Threshold() { Term = "Content", TermTint = normalStatusColorSidepanelBar, IconTint = securityColor, Icon = "lcd_icon_security", Edge = 0f  }, 
                              new Threshold() { Term = "Unhappy", TermTint = badStatusColorSidePanelBar, IconTint = securityColor, Icon = "lcd_icon_security", Edge = 2f  } 
                          }
                    },
                    BarPresentation = new BarPresentation()
                    {
                        MaxValue = 1f,
                        SuppressIfMaximumValue = false,
                        Width = 60
                    },
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                });

                list.Add(new PresentationType()
                {
                    KeyName = "comfortHappinessPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        NumberSource = NumberSource.Difference,
                        Thresholds = new[]
                          { 
                              new Threshold() { Term = "Happy", TermTint = normalStatusColorSidepanelBar /* normalStatusColorSidepanelBar*/, IconTint = comfortColor, Icon = "lcd_icon_comfort", Edge = -0.01f  }, 
                              new Threshold() { Term = "Content", TermTint = normalStatusColorSidepanelBar /*normalStatusColorSidepanelBar*/, IconTint = comfortColor, Icon = "lcd_icon_comfort", Edge = 0f  }, 
                              new Threshold() { Term = "Unhappy", TermTint = badStatusColorSidePanelBar, IconTint = comfortColor, Icon = "lcd_icon_comfort", Edge = 2f  } 
                          }
                    },
                    BarPresentation = new BarPresentation()
                    {
                        MaxValue = 1f,
                        SuppressIfMaximumValue = false,
                        Width = 60
                    },
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                });

                list.Add(new PresentationType()
                {
                    KeyName = "percentagePresentation",

                    ValueTextFormatting = new TextFormatting()
                    {
                        NumberFactor = 100f, // show as percent instead of decimals
                        NumberFormatString = "F0",  // no decimals

                        TextWithPlaceholders = "{1} %"
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "timeInDaysPresentation",

                    ValueTextFormatting = new TextFormatting()
                    {                       
                        NumberFormatString = "F1",  // one decimal
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "substancePresentation",

                    ValueTextFormatting = new TextFormatting()
                    {
                        NumberFormatString = "F0",  // no decimals

                        TextWithPlaceholders = "{1}" // why not append "BULK" here?
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "storageCapacityPresentation",
                   // ValueTooltip = new StringSource() { PropertyName = "storageFractionTooltip" },
                   /* ValueTooltipTextFormatting = new TextFormatting()
                    {
                        NumberFactor = 100f, // show as percent instead of decimals
                        NumberFormatString = "F0",  // no decimals
                        TextWithPlaceholders = "Used capacity: {1}%"
                    },*/
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[] 
                        {  
                          
                           // new Threshold(){ Edge = 1f,  TermTint = normalStatusColorSidepanelBar},   
                        
                             new Threshold(){ Edge = 0.98f,  TermTint = normalStatusColorSidepanelBar}, 
                             new Threshold(){ Edge = 1f,  TermTint = badStatusColorSidePanelBar }  
                        }
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = 10
                });
            
                //list.Add(new PresentationType()
                //{
                //    KeyName = "itemBulkPresentation",
                    
                    
                //    TextFormatting = new TextFormatting()
                //    {
                //         NumberFormatString = "F0", // no decimals
                //         TextWithPlaceholders = "{1} BULK" 
                //    }
                //});

                list.Add(new PresentationType()
                {
                    KeyName = "itemBulkPresentation",

                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        { 
                            //Always put the thresholds from lowest edge value to highest
                            new Threshold(){ Edge = 0, TermTooltip = "BULK is a measurement of mass and volume. An average human has a size of 100 BULK"}, 
                            
                        }
                    },
                    ValueTextFormatting = new TextFormatting()
                    {
                        NumberFormatString = "F0", // no decimals
                        TextWithPlaceholders = "{1} BULK"
                    }
                    
                });


                list.Add(new PresentationType()
                {
                    KeyName = "entityTypePresentation",                   
                    RightAdjustValue = true,
                    ValueRightPadding = 4, // 20,
                    EntityTypePresentation = new EntityTypePresentation()                    
                });

                list.Add(new PresentationType()
                {
                    KeyName = "cropGrowthPresentation",

                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        { 
                            //Always put the thresholds from lowest edge value to highest
                            new Threshold(){ Edge = 0.01f, TermTooltip = "None", TermTint = cropBarColor  }, 
                            new Threshold(){ Edge = 0.05f, TermTooltip = "Germinating", TermTint = cropBarColor }, 
                            new Threshold(){ Edge = 0.1f, TermTooltip = "Sprouting", TermTint = cropBarColor }, 
                            new Threshold(){ Edge = 0.4f, TermTooltip = "Small", TermTint = cropBarColor }, 
                            new Threshold(){ Edge = 0.6f, TermTooltip = "Medium", TermTint = cropBarColor }, 
                            new Threshold(){ Edge = 0.95f, TermTooltip = "Large", TermTint = cropBarColor }, 
                            new Threshold(){ Edge = 1, TermTooltip = "Full size reached. The crops will be ready for harvest after ripening", TermTint = cropBarColor },                             
                        }
                    },
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                    BarPresentation = new BarPresentation() { MaxValue = 1f }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "weedGrowthPresentation",

                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        { 
                            //Always put the thresholds from lowest edge value to highest
                            new Threshold(){ Edge = 0.05f, Term = "None", TermTint = weedBarColor, TermTooltip = "We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" }, //mp does the task show up in task manager? if so, tell player he can change prio.
                            new Threshold(){ Edge = 0.1f, Term = "Sprouting", TermTint = weedBarColor, TermTooltip = "We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" }, 
                            new Threshold(){ Edge = 0.4f, Term = "Small", TermTint = weedBarColor, TermTooltip = "Small. Weeds of this size will not affect crop growth too much. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" }, 
                            new Threshold(){ Edge = 0.6f, Term = "Medium", TermTint = weedBarColor, TermTooltip = "Medium. Weeds of this size will affect crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" }, 
                            new Threshold(){ Edge = 0.95f, Term = "Large", TermTint = weedBarColor, TermTooltip = "Large. These weeds will severely hamper crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" }, 
                            new Threshold(){ Edge = 1, Term = "Overgrown", TermTint = badStatusColor, TermTooltip = "Overgrown. These weeds will severely hamper crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds" },                             
                        }
                    },
                    /*
                     *   NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[] 
                        {  
                            new Threshold(){ Edge = 0.1f,  TermTint = badStatusColorSidePanelBar },    
                            new Threshold(){ Edge = 1f,  TermTint = normalStatusColorSidepanelBar},                           
                        }
                    },        
                     */ 
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                    BarPresentation = new BarPresentation() { MaxValue = 1f }
                });
            

                list.Add(new PresentationType()
                {
                    KeyName = "soilNutrientLevelPresentation", //for farm plots

                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        { 
                            //Always put the thresholds from lowest edge value to highest 
                            new Threshold(){ Edge = 0.05f, Term = "Totally depleted", TermTooltip = "The soil needs fertilizer to yield any crops at all", TermTint = badStatusColor }, //no crops will be yielded here. we think.
                            new Threshold(){ Edge = 0.2f, Term = "Heavily depleted" , TermTooltip = "The soil needs fertilizer to yield anything but a minimum amount of crops", TermTint = lessBadStatusColor}, 
                            new Threshold(){ Edge = 0.4f, Term = "Somewhat depleted"  , TermTooltip = "The soil needs fertilizer to ensure a large crop yield", TermTint = soilBarColor }, //mp at 0.45 a fertilizer job will be created.
                            new Threshold(){ Edge = 0.8f, Term = "Quite fertile" , TermTooltip = "The soil is in good condition and not in need of fertilizer", TermTint = soilBarColor }, 
                            new Threshold(){ Edge = 1, Term = "Very fertile", TermTooltip = "The soil has very good quality which makes a high crop yield possible", TermTint = soilBarColor },                             
                        }
                    },
                    RightAdjustValue = true,
                    ValueRightPadding = 20,
                    BarPresentation = new BarPresentation()
                    {
                         MaxValue = 1f
                    }
                });
            

                string sleepNeedTerm0 = "About to collapse";
                string sleepNeedTerm1 = "Very sleepy";

                string sleepNeedTooltip0 = "The character is in such need of sleep that it lowers the person's energy level.";
                string sleepNeedTooltip1 = "The character is in need of sleep which contributes to a lowered energy level.";

                float sleepNeedEdge0 = 0.05f;
                float sleepNeedEdge1 = 0.2f;

                list.Add(new PresentationType()
                {
                    KeyName = "sleepNeedPresentationSidePanel",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        { 
                            //Always put the thresholds from lowest edge value to highest
                            new Threshold(){ Edge = sleepNeedEdge0, TermTooltip = sleepNeedTerm0 + ". " + sleepNeedTooltip0, Icon = "HUD_icon_status_sleep" ,IconTint = badStatusColor, TermTint = badStatusColorSidePanelBar }, 
                            new Threshold(){ Edge = sleepNeedEdge1, TermTooltip = sleepNeedTerm1 + ". " + sleepNeedTooltip1, Icon = "HUD_icon_status_sleep" ,IconTint = lessBadStatusColor, TermTint = badStatusColorSidePanelBar }, 
                            new Threshold(){ Edge = 0.5f, TermTooltip = "Tired. The character needs more sleep", Icon = "HUD_icon_status_sleep", IconTint = normalStatusColorSidepanel, TermTint = sleepBarColor },
                            new Threshold(){ Edge = 1f, TermTooltip = "Well rested. The character gets sufficient sleep", Icon = "HUD_icon_status_sleep", IconTint = normalStatusColorSidepanel, TermTint = sleepBarColor },

//mp january 2015: currently, agents sleep need never goes below Well rested.(they will still take turns sleeping) So for now, until we get this looked at again, I will change the terms so that they reflect the current system...
// Lars: why did you not change the edges then?

//this is how they were:
                      //      new Threshold(){ Edge = 0.05f, Term = "About to collapse", TermTooltip = "Person is in such need of sleep that it lowers the person's energy level.", Icon = "HUD_icon_status_sleep" ,Tint = badStatusColor}, 
                      //      new Threshold(){ Edge = 0.2f, Term = "Very sleepy", TermTooltip = "Person is in need of sleep which contributes to a lowered energy level.", Icon = "HUD_icon_status_sleep" ,Tint = lessBadStatusColor}, 
                      //      new Threshold(){ Edge = 0.35f, Term = "Drowsy" , TermTooltip = "Person will soon need sleep"},
                      //      new Threshold(){ Edge = 0.6f, Term = "Getting tired" , TermTooltip = "Person is getting tired but is in no need of sleep" },
                      //      new Threshold(){ Edge = 1f, Term = "Well rested", TermTooltip = "Person is currently in no need of sleep" },
                        }
                    },
                    BarPresentation = new BarPresentation() { MaxValue = 1f },
                    RightAdjustValue = true,
                    ValueRightPadding = needsBarPaddingRight
                });

                list.Add(new PresentationType()
                {
                    KeyName = "sleepNeedPresentationStatusIcons",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {                            
                            new Threshold(){ Edge = sleepNeedEdge0, Term = sleepNeedTerm0, TermTooltip = sleepNeedTooltip0, Icon = "HUD_icon_status_sleep" ,IconTint = badStatusColor}, 
                            new Threshold(){ Edge = sleepNeedEdge1, Term = sleepNeedTerm1, TermTooltip = sleepNeedTooltip1, Icon = "HUD_icon_status_sleep" ,IconTint = lessBadStatusColor},                          
                            new Threshold(){ Edge = 1f }
                        }
                    }
                });

               /* float hitpointsEdge0 = 0.1f;
                float hitpointsEdge1 = 0.2f;

                string hitpointsHumanTerm0 = "Near death";*/

                // no reason to duplicate this, since icons are shown at all levels
                list.Add(new PresentationType()
                {
                    KeyName = "hitpointsPresentation",  //http://en.wikipedia.org/wiki/Physical_trauma http://en.wikipedia.org/wiki/Abbreviated_injury_scale http://en.wikipedia.org/wiki/Wounds  http://www.aaam1.org/ais/AISClarified2012.pdf
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        {
                            { "entity:human",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.2f, Term = "Near death", Icon = "HUD_icon_status_injury" ,IconTint = badStatusColor}, 
                                    new Threshold(){ Edge = 0.3f, Term = "Critically injured", Icon = "HUD_icon_status_injury" , IconTint = lessBadStatusColor }, 
                                    new Threshold(){ Edge = 0.5f, Term = "Severely injured",  Icon = "HUD_icon_status_injury" ,IconTint = lessBadStatusColor}, 
                                    new Threshold(){ Edge = 0.6f, Term = "Seriously injured", Icon = "HUD_icon_status_injury" ,IconTint = lessBadStatusColor}, 
                                    new Threshold(){ Edge = 0.8f, Term = "Moderately injured", Icon = "HUD_icon_status_injury" ,IconTint = lessBadStatusColor },
                                    new Threshold(){ Edge = 0.95f, Term = "Mild injuries", Icon = "HUD_icon_status_injury" ,IconTint = lessBadStatusColor },
                                    new Threshold(){ Edge = 0.99f, Term = "Superficial", Icon = "HUD_icon_status_injury" ,IconTint = lessBadStatusColor },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:whiteThunderChicken",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Appears mortally wounded" },
                                    new Threshold(){ Edge = 0.6f, Term = "Appears wounded" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:pygmyThunderChicken",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Appears mortally wounded" },
                                    new Threshold(){ Edge = 0.6f, Term = "Appears wounded" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            }
                        }
                    },

                });

          
                list.Add(new PresentationType()
                { 
                    KeyName = "bodyPartCondition",
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {
                       
                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        {
                            { "entity:person:Left leg",
                                new[]
                                { 
                                 
                                    new Threshold(){ Edge = 0.1f, Term = "Open fracture" },  // I don't write specific joint or bone because it will not make sense to be so specific with this system when the character is hit multiple times on same bodypart. Also, chemical spray attack may look stupid with this system...-MP
                                    new Threshold(){ Edge = 0.2f, Term = "Fracture" },
                                    new Threshold(){ Edge = 0.3f, Term = "Deep lacerations" }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Dislocation" },
                                    new Threshold(){ Edge = 0.55f, Term = "Muscle tear" },
                                    new Threshold(){ Edge = 0.65f, Term = "Sprain" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial laceration" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:person:Right leg",
                                new[]
                                { 
                                
                                    new Threshold(){ Edge = 0.1f, Term = "Open fracture" },  // I don't write specific joint or bone because it will not make sense to be so specific with this system when the character is hit multiple times on same bodypart. Also, chemical spray attack may look stupid with this system...-MP
                                    new Threshold(){ Edge = 0.2f, Term = "Fracture" },
                                    new Threshold(){ Edge = 0.3f, Term = "Deep lacerations" }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Dislocation" },
                                    new Threshold(){ Edge = 0.55f, Term = "Muscle tear" },
                                    new Threshold(){ Edge = 0.65f, Term = "Sprain" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial laceration" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                             { "entity:person:Left arm",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Open fracture" },  // I don't write specific joint or bone because it will not make sense to be so specific with this system when the character is hit multiple times on same bodypart. Also, chemical spray attack may look stupid with this system...-MP
                                    new Threshold(){ Edge = 0.2f, Term = "Fracture" },
                                    new Threshold(){ Edge = 0.3f, Term = "Deep lacerations" }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Dislocation" },
                                    new Threshold(){ Edge = 0.55f, Term = "Muscle tear" },
                                    new Threshold(){ Edge = 0.65f, Term = "Sprain" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial laceration" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:person:Right arm",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Open fracture" },  // I don't write specific joint or bone because it will not make sense to be so specific with this system when the character is hit multiple times on same bodypart. Also, chemical spray attack may look stupid with this system...-MP
                                    new Threshold(){ Edge = 0.2f, Term = "Fracture" },
                                    new Threshold(){ Edge = 0.3f, Term = "Deep lacerations" }, 
                                    new Threshold(){ Edge = 0.4f, Term = "Dislocation" },
                                    new Threshold(){ Edge = 0.55f, Term = "Muscle tear" },
                                    new Threshold(){ Edge = 0.65f, Term = "Sprain" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial laceration" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:person:Torso",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Ruptured organ" },  // I don't write specific joint or bone because it will not make sense to be so specific with this system when the character is hit multiple times on same bodypart. Also, chemical spray attack may look stupid with this system...-MP                                                   
                                    new Threshold(){ Edge = 0.2f, Term = "Deep lacerations" },
                                    new Threshold(){ Edge = 0.45f, Term = "Perforation" },
                                    //new Threshold(){ Edge = 0.65f, Term = "Contusions" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial laceration" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:person:Head",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.1f, Term = "Open fracture" },
                                    new Threshold(){ Edge = 0.2f, Term = "Fracture" },
                                    //new Threshold(){ Edge = 0.3f, Term = "Dislocation" },      // "Dislocation"
                                    //new Threshold(){ Edge = 0.4f, Term = "Sprain" },             // "Sprain"
                                    new Threshold(){ Edge = 0.3f, Term = "Deep lacerations" },      // "Muscle tear"
                                    new Threshold(){ Edge = 0.55f, Term = "Perforations" },
                                    new Threshold(){ Edge = 0.75f, Term = "Superficial lacerations" },
                                    new Threshold(){ Edge = 0.85f, Term = "Skin abrasions" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            }
                        }
                    }
                  
                });
                list.Add(new PresentationType()
                {
                    KeyName = "MovementCondition",
                    TypeDependentPresentation = new TypeDependentPresentation()
                    {

                        ThresholdsByType = new SerializableDictionary<string, Threshold[]>()
                        {
                            { "entity:pygmyThunderChicken:Left leg",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Limping" },
                                    //Edge is 0.3 because the locomotion weight of the legs for the thunder chicken is set to 0.7 in the bodyPart creation
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:pygmyThunderChicken:Right leg",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Limping" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },    
                            { "entity:whiteThunderChicken:Left leg",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Limping" },
                                    //Edge is 0.3 because the locomotion weight of the legs for the thunder chicken is set to 0.7 in the bodyPart creation
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },
                            { "entity:whiteThunderChicken:Right leg",
                                new[]
                                { 
                                    new Threshold(){ Edge = 0.3f, Term = "Limping" },
                                    new Threshold(){ Edge = 1f, Term = null },
                                }
                            },    
                        }
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "professionPresentation",
                    IconPresentation = new IconPresentation(),
                    ValueTooltipTextFormatting = new TextFormatting()
                    {
                        TextWithPlaceholders = "Field: {0}  \nThe character has the best skills in this area." // "Profession: {0}  \nThe profession is derived from the best skill the character has." //mp I don't like 'profession', too specific
                    }
                });

                list.Add(new PresentationType()
                {
                    KeyName = "replenishStatusPresentation",                     
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {       
                            new Threshold(){ Edge = 0.5f, Icon = "HUD_icon_status_firewood", IconTint = badStatusColor, Term = "No suitable fuel in inventory. Production orders cannot be completed before we have the correct type of fuel" }, //"Firewood: Not available"              
                            new Threshold(){ Edge = 1f, Icon = null, Term = null }                            
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "ammoStatusPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {       
                            new Threshold(){ Edge = 0.1f, Icon = "HUD_icon_status_ammunition", IconTint = badStatusColor, Term = "Needs a reload, but there is no ammunition in inventory" }, // not implemented yet..
                            new Threshold(){ Edge = 0.5f, Icon = "HUD_icon_status_ammunition", IconTint = lessBadStatusColor, Term = "Needs ammunition reload" }, 
                            new Threshold(){ Edge = 1f, Icon = null, Term = null }                            
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "inAccessiblePresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {       
                            new Threshold(){ Edge = 0.2f, Icon = null, Term = null },
                            new Threshold(){ Edge = 0.5f, Icon = "HUD_icon_status_exclamation", IconTint = badStatusColor, Term = "A colonist needs this item but is unwilling to go near it because a dangerous area/threat is blocking the way. \nUse PATROL or ATTACK to clear the area of threats. Use the THREAT overlay next to the minimap to display danger areas." },  // mp was:  (Required stance: Fearless)      Dangerous area   - term also used on marker window for job icons 
                            new Threshold(){ Edge = 1f, Icon = "HUD_icon_status_noAccess", IconTint = badStatusColor, Term = "Not accessible" }           
                        }
                    },
                });

                list.Add(new PresentationType()
                {
                    KeyName = "hasThreatJobPresentation",
                    NumberThresholdPresentation = new NumberThresholdPresentation()
                    {
                        Thresholds = new[]
                        {       
                            new Threshold(){ Edge = 0.9f, Icon = null, Term = null },    
                            new Threshold(){ Edge = 1f, Icon = "HUD_icon_status_skull",  Term = "This animal is seen as a threat and the colony members will try to eliminate it" }                                                   
                        }
                    },
                });

                return list;
        }

        /// <summary>
        /// shows icons and bars on the HUD Marker Window
        /// </summary>
        /// <returns></returns>
        protected override CustomDataPresentation InitStatusIconPresentation()
        {           
               // GameData.Instance.CustomStatusIconData
            CustomDataPresentation item = new CustomDataPresentation()
                {
                    PresentationTypeCategories = new[]
                    {
                        new PresentationTypeCategory()
                        {
                             PrimarySortingOfItems = new Sorting()
                             {
                                  SortingMethod = WindowSystem.SortingMethod.StaticSortOrder, // first sort by SortOrder, 0 is first
                                  SortingDirection = WindowSystem.Grid.Sorting.Ascending                                 
                             },
                              SecondarySortingOfItems = new Sorting()
                              {
                                   SortingMethod =  WindowSystem.SortingMethod.NumberResultMiddleDistance, // then by how extreme the float value is
                                   SortingDirection = WindowSystem.Grid.Sorting.Descending,
                              },
                         
                            Name = "Status icon presentations", //terms must correspond to captions in side panel
                            Nodes = new[]
                            {                            
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation()
                                    {                                                          
                                        PropertyNameForValue = "hitpointLevel",
                                        PresentationTypeKey = "hitpointsPresentation"
                                    }
                                },
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation() // NUTRITION
                                    {                                                           
                                        PropertyNameForValue = "hungerStatus",
                                        PresentationTypeKey = "hungerPresentationStatusIcons"
                                    }
                                },
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation() // SLEEP
                                    {                                                                
                                        PropertyNameForValue = "sleepStatus",
                                        PresentationTypeKey = "sleepNeedPresentationStatusIcons"
                                    }
                                },                         
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation() // ENERGY
                                    {                                                                 
                                        PropertyNameForValue = "energyLevel",
                                        PresentationTypeKey = "energyPresentationStatusIcons"
                                    }
                                
                                },  
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation =  new Presentation()
                                    {                                   
                                        PropertyNameForValue = "entityIsFunctional",
                                        PresentationTypeKey = "entityFunctionalStatusIcons"
                                    }
                               
                                },     
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() { PropertyName = "replenishTypeName" },                                       
                                        PropertyNameForValue = "replenishStatus",
                                        PresentationTypeKey = "replenishStatusPresentation",
                                        ValueTooltip = new StringSource { PropertyName = "replenishStatusTooltip" }
                                    }                                
                                },  
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation()
                                    {
                                       /* CaptionSource = Presentation.CaptionType.Function, 
                                        CaptionData = "replenishTypeName",*/
                                        PropertyNameForValue = "ammoStatus",
                                        PresentationTypeKey = "ammoStatusPresentation"
                                    }                                
                                },  
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() { UseDefaultName = true },
                                        //CaptionSource = Presentation.CaptionType.Default, 
                                        PropertyNameForValue =  "inAccessible",
                                        PresentationTypeKey = "inAccessiblePresentation"
                                    }                                
                                },
                                new LeafNode()
                                {
                                    SortOrder = 1,
                                    Presentation = new Presentation()
                                    {
                                        Caption = new StringSource() { UseDefaultName = true },
                                        PropertyNameForValue =  "hasThreatJob",
                                        PresentationTypeKey = "hasThreatJobPresentation"
                                    }                                
                                },
                                new LeafNode()
                                {
                                    SortOrder = 0, // always stay to the left
                                    Presentation =  new Presentation()
                                    {                                   
                                        PropertyNameForValue = "progress",
                                        PresentationTypeKey = "progressStatusIcons" // should show a bar instead of icon
                                    }                               
                                },
                                 new LeafNode()
                                {
                                    SortOrder = 0, // always stay to the left
                                    Presentation =  new Presentation()
                                    {                                   
                                        PropertyNameForValue = "consumeProgress",
                                        PresentationTypeKey = "consumeProgressStatusIcons" // should show a bar instead of icon
                                    }
                               
                                },
                                new LeafNode()
                                {
                                    SortOrder = 0, // always stay to the left
                                    Presentation =  new Presentation()
                                    {                                   
                                        PropertyNameForValue = "professionIcon",
                                        ValueTooltip = new StringSource() { PropertyName = "professionDescription" /* Tooltip"*/ },
                                        PresentationTypeKey = "professionPresentation" 
                                    }
                               
                                }
                            }
                        }
                    }
                };

            return item;
        }

        protected override CustomDataPresentation InitActivityPresentation()
        {
           
                //GameData.Instance.CustomEntityActivityData 
                CustomDataPresentation item = new CustomDataPresentation()
                {
                    PresentationTypeCategories = new[]
                {
                    new PresentationTypeCategory()
                    {                                             
                        Name = "Productivity", 
                        Nodes = new[]
                        {
                            new LeafNode()
                            {
                                SortOrder = 0,
                                Presentation = new Presentation()
                                {
                                    Caption = new StringSource() { StaticString = "Productivity" },
                                    PropertyNameForValue = "totalProductivity",
                                    PresentationTypeKey = "totalProductivity"
                                }                                
                            },
                            new LeafNode()
                            {
                                SortOrder = 1,
                                Presentation = new Presentation()
                                {                                   
                                    Caption = new StringSource() {  PropertyName = "skillInUseName" },
                                    PropertyNameForValue = "skillProductivity",
                                    PresentationTypeKey = "skillPresentationStatusIcons" 
                                }                                
                            },
                            new LeafNode()
                            {
                                SortOrder = 2,
                                Presentation = new Presentation()
                                {
                                    Caption = new StringSource() {  PropertyName = "toolInUseName" },
                                    PropertyNameForValue = "toolProductivity",
                                    PresentationTypeKey = "toolProductivity"
                                }                                
                            },
                        },
                    }
                }
                };


                return item;
        }


      

        protected override CustomDataPresentation InitOtherSiteSidePanelPresentation()
        {
            CustomDataPresentation item = new CustomDataPresentation()
               {
                   PresentationTypeCategoryKeys = new[]
                    {                                              
                        "skillsCategory",  
                        "opinionsCategory", 
                    }
               };

            return item;

        }
        

        protected override CustomDataPresentation InitSidePanelPresentation()
        {
            
                //GameData.Instance.CustomSidePanelData

                CustomDataPresentation item = new CustomDataPresentation()
                {
                    PresentationTypeCategoryKeys = new[]
                    {
                        "occupantsCategory", 
                        "residentsCategory", 
                        "healthStatusCategory" ,
                        "skillsCategory",
                        "cropsCategory",
                        "statusCategory",
                        "opinionsCategory",
                        "effectsCategory"
                    }
                };

                return item;
        }

       

     
      

        /// <summary>
        /// all sounds being referenced must appear here
        /// </summary>
        protected override List<SoundData> InitSounds()
        {
            float soundSmallPitchDeviation = 0.03f;
            float soundMediumPitchDeviation = 0.06f;
            float soundBigPitchDeviation = 0.12f;
                // you can add the same sound with different volumes, just change the key name

            List<SoundData>  list = new List<SoundData>();

            #region Sounds
            #region Aliens

            // Alien combat sounds
            #region BushDragon Combat
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/bushdragonPoisonShot",
                Sound = "aliens/alienCombat/bushdragonPoisonShot1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/bushdragonAttack",
                Sound = "aliens/alienCombat/bushdragonAttack1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/bushdragonHit",
                Sound = "aliens/alienCombat/bushdragonHit1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/bushdragonDeath",
                Sound = "aliens/alienCombat/bushdragonDeath1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/poisonAlienImpact",
                Sound = "aliens/alienCombat/poisonAlienImpact1a",
                Volume = 0.30f
            });
            #endregion
            #region Worm Combat
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/wormAttackPart1",
                Sound = "aliens/alienCombat/wormAttackStart1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/wormAttackPart2",
                Sound = "aliens/alienCombat/wormAttackAction1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/wormHitShort",
                Sound = "aliens/alienCombat/wormHit1a",
                Volume = 0.30f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/wormDeathShort",
                Sound = "aliens/alienCombat/wormDeath1a",
                Volume = 0.30f
            });
            #endregion
            #region Snatcher Combat
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/snatcherAttack",
                Sound = "aliens/alienCombat/snatcherAttack1a",
                Volume = 0.22f, //0.3
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundMediumPitchDeviation
                }

            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/snatcherHit",
                Sound = "aliens/alienCombat/snatcherHit1a",
                Volume = 0.22f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/snatcherDeath",
                Sound = "aliens/alienCombat/snatcherDeath1a",
                Volume = 0.22f
            });
            #endregion
            #region Patrician Combat
            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/flutter_Mat43_v2", // when hit.
                Sound = "aliens/alienCombat/flutter_Mat43_v2",
                Volume = 1f,//
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.05f //
                }
            });

            list.Add(new SoundData()
            {
                KeyName = "aliens/alienCombat/strumming_Mat25_v1", // when collapses
                Sound = "aliens/alienCombat/strumming_Mat25_v1",
                Volume = 1f,//
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.05f //
                }
            });

            #endregion

            // Alien interaction sounds
            list.Add(new SoundData()
            {
                KeyName = "aliens/croaker", //MP sounds like indoors
                Sound = "aliens/croaker",
                Volume = 0.0f    //0.25f               
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/binalRatDie",
                Sound = "aliens/binalRatDie1a",
                Volume = 0.0f   //0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/rattleWooden",
                Sound = "aliens/rattleWooden",
                Volume = 0.0f   //0.8f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/rattleWoodenShort",
                Sound = "aliens/rattleWoodenShort",
                Volume = 0.0f   //0.7f                    
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/hummingClickClacking",
                Sound = "aliens/hummingClickClacking",
                Volume = 0.0f
            });

            list.Add(new SoundData()
            {
                KeyName = "aliens/bushdragonCombatIdle",
                Sound = "aliens/bushdragonCombatIdle",
                Volume = 0.0f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/bushdragonHit",
                Sound = "aliens/bushdragonHit",
                Volume = 0.0f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/spacechicken",
                Sound = "aliens/spacechicken",
                Volume = 0.0f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/sliceSnapDouble",
                Sound = "aliens/sliceSnapDouble",
                Volume = 0.12f, //
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = -0.35f,
                    StandardDeviation = 0.1f //
                }
            });

            list.Add(new SoundData()
            {
                KeyName = "aliens/sliceSnap",
                Sound = "aliens/sliceSnap",
                Volume = 0.12f, //
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = -0.35f,
                    StandardDeviation = 0.1f //
                }
            });


            list.Add(new SoundData()
            {
                KeyName = "aliens/twinklerStab",
                Sound = "aliens/twinklerStab",
                Volume = 0.8f //was 0f
            });
            list.Add(new SoundData()
            {
                KeyName = "aliens/bushdragonAttack_lowerVolume",
                Sound = "aliens/bushdragonAttack_lowerVolume",
                Volume = 0.0f
            });



            #endregion

            #region Human Activities
            #region Building
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingMetalMend",
                Sound = "activities/building/buildingMetalMend",
                Volume = 0.25f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingMetalKneelDig",
                Sound = "activities/building/buildingMetalKneelDig",
                Volume = 0.25f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingMetalKneelWaterDevice",
                Sound = "activities/building/buildingMetalKneelWaterDevice",
                Volume = 0.18f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpImprovisedKneelDig",
                Sound = "activities/building/buildingTarpImprovisedKneelDig",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpImprovisedKneelWaterDevice",
                Sound = "activities/building/buildingTarpImprovisedKneelWaterDevice",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpImprovisedMend",
                Sound = "activities/building/buildingTarpImprovisedMend",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpKneelDig",
                Sound = "activities/building/buildingTarpKneelDig",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpKneelWaterDevice",
                Sound = "activities/building/buildingTarpKneelWaterDevice",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingTarpMend",
                Sound = "activities/building/buildingTarpMend",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingImprovisedKneelDig",
                Sound = "activities/building/buildingImprovisedKneelDig",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingImprovisedKneelWaterDevice",
                Sound = "activities/building/buildingImprovisedKneelWaterDevice",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingImprovisedMend",
                Sound = "activities/building/buildingImprovisedMend",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingElectronic1",
                Sound = "activities/building/buildingElectronic1",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingElectronic2",
                Sound = "activities/building/buildingElectronic2",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingHammer",
                Sound = "activities/building/buildingHammer",
                Volume = 0.45f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/building/buildingSteel",
                Sound = "activities/building/buildingSteel",
                Volume = 0.45f
            });
            #endregion
            #region Butcher
            list.Add(new SoundData()
            {
                KeyName = "activities/butcher/butcherChopLow",
                Sound = "activities/butcher/butcherChopLow",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/butcher/butcherCutLow",
                Sound = "activities/butcher/butcherCutLow",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/butcher/butcherConstructPull",
                Sound = "activities/butcher/butcherConstructPull",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/butcher/butcherSharpen",
                Sound = "activities/butcher/butcherSharpen",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/butcher/butcherGather",
                Sound = "activities/butcher/butcherGather",
                Volume = 0.32f
            });
            #endregion
            #region Eating
            list.Add(new SoundData()
            {
                KeyName = "activities/cooking/cookingBoil",
                Sound = "activities/cooking/cookingBoil",
                Volume = 1
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/eating/eatingCrunchy",
                Sound = "activities/eating/eatingCrunchy",
                Volume = 0.6f
            });
            #endregion
            #region Farming
            list.Add(new SoundData()
            {
                KeyName = "activities/farming/farmingHoe1A",
                Sound = "activities/farming/farmingHoeDirtSoil1A",
                Volume = 0.75f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/farming/farmingHoe1B",
                Sound = "activities/farming/farmingHoeDirtSoil1B",
                Volume = 0.75f
            });
            #endregion
            #region Gather
            list.Add(new SoundData()
            {
                KeyName = "activities/gather/gatherChopLow",
                Sound = "activities/gather/gatherChopLow2",
                Volume = 0.55f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/gather/gatherHandGather",
                Sound = "activities/gather/gatherHandGather3",
                Volume = 0.3f,
                PlayMaxOneInstance = true,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.03f
                }

            });
            list.Add(new SoundData()
            {
                KeyName = "activities/gather/gatherHandKneelDig",
                Sound = "activities/gather/gatherHandKneelDig1",
                Volume = 0.55f,
                PlayMaxOneInstance = true,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.03f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/gather/gatherHandGatherStand",
                Sound = "activities/gather/gatherHandGatherStand",
                PlayMaxOneInstance = true,
                Volume = 0.26f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/gather/gatherCutLow",
                Sound = "activities/gather/gatherCutLow",
                PlayMaxOneInstance = true,
                Volume = 0.3f
            });


            #endregion
            #region Melee
            list.Add(new SoundData()
            {
                KeyName = "melee/STAB2_24 - 4 Stabs With Blood",
                Sound = "melee/STAB2_24 - 4 Stabs With Blood",
                Volume = 0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "melee/swoosh",
                Sound = "melee/swoosh",
                Volume = 1
            });
            list.Add(new SoundData()
            {
                KeyName = "melee/PUNCH1_02 - 9 Low Thud Punches",
                Sound = "melee/PUNCH1_02 - 9 Low Thud Punches",
                Volume = 0.21f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/kickHard1B",
                Sound = "activities/melee/kickHard1B",
                Volume = 0.25f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/kickHard2",
                Sound = "activities/melee/kickHard2B",
                Volume = 0.25f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/punchLight1",
                Sound = "activities/melee/punchLight1",
                Volume = 0.25f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/punchLight2",
                Sound = "activities/melee/punchLight2",
                Volume = 0.25f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/punchLight3",
                Sound = "activities/melee/punchLight3",
                Volume = 0.25f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/swooshClothHigh1",
                Sound = "activities/melee/swooshClothHigh1",
                Volume = 0.2f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/swooshClothHigh2",
                Sound = "activities/melee/swooshClothHigh2",
                Volume = 0.2f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/swooshClothHigh3",
                Sound = "activities/melee/swooshClothHigh3",
                Volume = 0.2f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/swooshClothLow1",
                Sound = "activities/melee/swooshClothLow1",
                Volume = 0.2f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/melee/swooshClothLow2",
                Sound = "activities/melee/swooshClothLow2",
                Volume = 0.2f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = soundBigPitchDeviation
                }
            });
            #endregion
            #region Salvage
            #region Salvage Metal
            list.Add(new SoundData()
            {
                KeyName = "activities/salvage/salvageBreakMetal1",
                Sound = "activities/salvage/salvageBreakMetal1",
                Volume = 0.25f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/salvage/salvageBreakMetal2",
                Sound = "activities/salvage/salvageBreakMetal2",
                Volume = 0.25f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/salvage/salvagePullMetal",
                Sound = "activities/salvage/salvagePullMetal",
                Volume = 0.65f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/salvage/salvageMetalCutLow",
                Sound = "activities/salvage/salvageMetalCutLow",
                Volume = 0.65f
            });
            #endregion

            list.Add(new SoundData()
            {
                KeyName = "activities/salvage/salvageAll",
                Sound = "activities/salvage/salvageAllDeeperLong",
                Volume = 0.55f
            });
            #endregion
            #region Weapons
            #region Impacts
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/macheteImpaleHard",
                Sound = "activities/weapons/macheteImpaleHardD",
                Volume = 0.45f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0.05f,
                    StandardDeviation = soundMediumPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/knifeStabBloody1B",
                Sound = "activities/weapons/knifeStabBloody1B",
                Volume = 0.51f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0.05f,
                    StandardDeviation = soundMediumPitchDeviation
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/knifeStab1A",
                Sound = "activities/weapons/knifeStab1A",
                Volume = 0.51f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0.05f,
                    StandardDeviation = soundMediumPitchDeviation
                }
            });

            #endregion
            //// Weapon sounds
                //Coil rifle
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/coilrifleShot",
                Sound = "activities/weapons/coilrifleShot1b",
                Volume = 0.85f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.05f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/coilRifle/coilrifleShotHard",
                Sound = "activities/weapons/coilRifle/coilrifleShotHard",
                Volume = 0.6f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.05f
                }
            });
            //added sound for weapon for the HOUND Robot
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/coilrifleShot1a",
                Sound = "activities/weapons/coilrifleShot1a",
                Volume = 0.6f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.05f
                }
            });

            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/coilrifleReload",
                Sound = "activities/weapons/coilrifleReload1b",
                Volume = 0.2f
            });
            //Fire extinguisher (Boosted)
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/extinguisherBoostShot",
                Sound = "activities/weapons/extinguisherBoostShot1a",
                Volume = 0.95f
            });
            //Bow
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/bow/bowShot2A",
                Sound = "activities/weapons/bow/bowShot2A",
                Volume = 0.85f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/bow/arrowImpact2A",
                Sound = "activities/weapons/bow/arrowImpact2A",
                Volume = 0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/arrowImpact1a",
                Sound = "activities/weapons/arrowImpact1a",
                Volume = 0.3f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/bow/bowAim2A",
                Sound = "activities/weapons/bow/bowAim2A",
                Volume = 0.65f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/bow/bowReload2A",
                Sound = "activities/weapons/bow/bowReload2A",
                Volume = 0.65f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/sentryGun/sentryMedium2Burst",
                Sound = "activities/weapons/sentryGun/sentryMedium2Burst",
                Volume = 0.85f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/sentryGun/sentryBurstCasingsHigh",
                Sound = "activities/weapons/sentryGun/sentryBurstCasingsHigh",
                Volume = 0.32f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.07f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/weapons/sentryGun/sentryBurstCasingsLow",
                Sound = "activities/weapons/sentryGun/sentryBurstCasingsLow",
                Volume = 0.32f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.07f
                }
            });
            #endregion
            #region Crafting
            list.Add(new SoundData()
            {
                KeyName = "activities/crafting/craftingGeneral1",
                Sound = "activities/crafting/craftingGeneral1b",
                Volume = 0.65f
            });
            list.Add(new SoundData()
            {
                KeyName = "activities/crafting/craftingGeneral2",
                Sound = "activities/crafting/craftingGeneral2b",
                Volume = 0.65f
            });
            #endregion
            #region Traps
            list.Add(new SoundData()
            {
                KeyName = "traps/mineExplosionHardwDebris",
                Sound = "traps/mineExplosionHardwDebris",
                Volume = 0.8f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.04f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "traps/mineDetonateBeep",
                Sound = "traps/mineDetonateBeep",
                Volume = 0.6f
            });
            #endregion
            #endregion

            #region Domesticated Animals
            #region Dog
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogAttackSnarl1", //mp jan 2016. randomizes between snarl 1 and 2. this sound here sometimes gets cut off..
                Sound = "domesticated/dog/dogAttackSnarl1",
                Volume = 0.3f,//was 0.4f
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.08f //0.04f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogAttackSnarl2",
                Sound = "domesticated/dog/dogAttackSnarl2",
                Volume = 0.3f, //was 0.4f
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0.2f,
                    StandardDeviation = 0.15f //0.04f //the snarl gets repeated A LOT. especially with 2 dogs. so I vary as much as possible.
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogEat",
                Sound = "domesticated/dog/dogEat",
                Volume = 0.5f,
            });
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogBark1B",
                Sound = "domesticated/dog/dogBark1B",
                Volume = 0.5f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.06f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogHitBark1",
                Sound = "domesticated/dog/dogHitBark1",
                Volume = 0.5f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.06f
                }
            });
            list.Add(new SoundData()
            {
                KeyName = "domesticated/dog/dogHitWhimper2",
                Sound = "domesticated/dog/dogHitWhimper2",
                Volume = 0.5f,
                RandomPitchChange = new NormalDistribution()
                {
                    Mean = 0f,
                    StandardDeviation = 0.06f
                }
            });
            #endregion
            #endregion

            #region Ambient
            #region Weather & Environment ambience
            list.Add(new SoundData()
            {
                KeyName = "ambient/wind/windHighFreq",
                Sound = "ambient/wind/windHighFreq2",
                Volume = 0.11f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/wind/windMidFreq",
                Sound = "ambient/wind/windMidFreq2",
                Volume = 0.08f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/wind/windHighFreqGustyLong1A",
                Sound = "ambient/wind/windHighFreqGustyLong1A",
                Volume = 0.07f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/wind/windHighFreqGustyLong1B",
                Sound = "ambient/wind/windHighFreqGustyLong1B",
                Volume = 0.07f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/wind/windHighFreqGustyLong1C",
                Sound = "ambient/wind/windHighFreqGustyLong1C",
                Volume = 0.07f,
                PlayMaxOneInstance = true

            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalm2A",
                Sound = "ambient/water/shoreWavesCalm2A",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalm2B",
                Sound = "ambient/water/shoreWavesCalm2B",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalm2C",
                Sound = "ambient/water/shoreWavesCalm2C",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalmLonger3A",
                Sound = "ambient/water/shoreWavesCalmLonger3A",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalmLonger3B",
                Sound = "ambient/water/shoreWavesCalmLonger3B",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalmLongerLower3A",
                Sound = "ambient/water/shoreWavesCalmLonger3A",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/shoreWavesCalmLongerLower3B",
                Sound = "ambient/water/shoreWavesCalmLonger3B",
                Volume = 0.10f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterBrook",
                Sound = "ambient/water/waterBrook",
                Volume = 0.12f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterBrookSmallA",
                Sound = "ambient/water/waterBrookSmallA",
                Volume = 0.04f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterBrookSmallB",
                Sound = "ambient/water/waterBrookSmallB",
                Volume = 0.04f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterBrookSmallC",
                Sound = "ambient/water/waterBrookSmallC",
                Volume = 0.04f,
                PlayMaxOneInstance = true
            });


/////////////////////The river medium stream sounds do NOT cycle, there's a noticeable start stop in the source wav. so don't use..
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterRiverMediumStreamA",
                Sound = "ambient/water/waterRiverMediumStreamA",
                Volume = 0.08f, //mp reduced from 0.14f because too dominant. Also gave quesiness.
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterRiverMediumStreamB",
                Sound = "ambient/water/waterRiverMediumStreamB",
                Volume = 0.08f, //mp reduced from 0.14f because too dominant
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/waterRiverMediumStreamC",
                Sound = "ambient/water/waterRiverMediumStreamC",
                Volume = 0.08f, //mp reduced from 0.14f because too dominant
                PlayMaxOneInstance = true
            });
/////////////////////////////////////////////////////

            list.Add(new SoundData()
            {
                KeyName = "ambient/water/mudflats1",
                Sound = "ambient/water/mudflatsCompressed1",
                Volume = 0.3f,
                PlayMaxOneInstance = true
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/water/mudflats2",
                Sound = "ambient/water/mudflatsCompressed2",
                Volume = 0.3f,
                PlayMaxOneInstance = true
            });
            #endregion
            #region Animal Ambience
            #region Insect Ambience
            list.Add(new SoundData()
            {
                KeyName = "ambient/insects/insectFlying1A",
                Sound = "ambient/insects/insectFlying1A",
                Volume = 0.29f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/insects/insectFlying1B",
                Sound = "ambient/insects/insectFlying1B",
                Volume = 0.29f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/insectsSingleA",
                Sound = "ambient/aliens/insectsSingleA",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/insectsSingleB",
                Sound = "ambient/aliens/insectsSingleB",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/insects/insectGroup1A",
                Sound = "ambient/insects/insectGroup1A",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/insects/insectGroup1B",
                Sound = "ambient/insects/insectGroup1B",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/insects/insectGroup1C",
                Sound = "ambient/insects/insectGroup1C",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/insectsMediumGroup",
                Sound = "ambient/aliens/insectsMediumGroup",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/insectsLongConstantwBreak",
                Sound = "ambient/aliens/insectsLongConstantwBreak",
                Volume = 0.16f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\rattleWoodenAmbient",
                Sound = "ambient\\aliens\\rattleWoodenAmbient",
                Volume = 0.65f
            });
            #endregion
            #region Bird-like ambience
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdsMediumGroup",
                Sound = "ambient/birds/birdsMediumGroup",
                Volume = 0.25f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle1A",
                Sound = "ambient/birds/birdSingle1A",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle1B",
                Sound = "ambient/birds/birdSingle1B",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle1C",
                Sound = "ambient/birds/birdSingle1C",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle2A",
                Sound = "ambient/birds/birdSingle2A",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle2B",
                Sound = "ambient/birds/birdSingle2B",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle2C",
                Sound = "ambient/birds/birdSingle2C",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle3A",
                Sound = "ambient/birds/birdSingle3A",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle3B",
                Sound = "ambient/birds/birdSingle3B",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle4A",
                Sound = "ambient/birds/birdSingle4A",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle4B",
                Sound = "ambient/birds/birdSingle4B",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle4C",
                Sound = "ambient/birds/birdSingle4C",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle4D",
                Sound = "ambient/birds/birdSingle4D",
                Volume = 0.32f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle5A",
                Sound = "ambient/birds/birdSingle5A",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle5B",
                Sound = "ambient/birds/birdSingle5B",
                Volume = 0.13f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle5C",
                Sound = "ambient/birds/birdSingle5C",
                Volume = 0.11f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle5D",
                Sound = "ambient/birds/birdSingle5D",
                Volume = 0.12f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/birds/birdSingle5E", //Same as 5B but at lower volume
                Sound = "ambient/birds/birdSingle5B",
                Volume = 0.06f
            });
            #endregion
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/wormAmbienceSingle",
                Sound = "ambient/aliens/wormAmbienceSingle",
                Volume = 0.7f
            });

            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/swampFrogs",
                Sound = "ambient/aliens/swampFrogs",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/swampFrogs2",
                Sound = "ambient/aliens/swampFrogs2",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient/aliens/swampFrogs3",
                Sound = "ambient/aliens/swampFrogs3",
                Volume = 0.15f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\toothCricketsAmbient",
                Sound = "ambient\\aliens\\toothCricketsAmbient",
                Volume = 1f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\angryToyAmbient",
                Sound = "ambient\\aliens\\angryToyAmbient",
                Volume = 1f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\croakerAmbient", // MP sounds like indoors...
                Sound = "ambient\\aliens\\croakerAmbient",
                Volume = 0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\hummingClickClackingAmbient",
                Sound = "ambient\\aliens\\hummingClickClackingAmbient",
                Volume = 1f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\rattleBuzzAmbient",
                Sound = "ambient\\aliens\\rattleBuzzAmbient",
                Volume = 1f
            });
            list.Add(new SoundData()
            {
                KeyName = "ambient\\aliens\\spacechickenAmbient",
                Sound = "ambient\\aliens\\spacechickenAmbient",
                Volume = 1f
            });
            #endregion
            #endregion
            #endregion

            #region Robots
            list.Add(new SoundData()
            {
                KeyName = "robotServoArms",
                Sound = "activities/robots/robotServoArms",
                Volume = 0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "robotServoArms2",
                Sound = "activities/robots/robotServoArms2",
                Volume = 0.5f
            });
            list.Add(new SoundData()
            {
                KeyName = "robotDriveEngineMedium",
                Sound = "activities/robots/robotDriveEngineMedium",
                Volume = 0.5f
            });
            #endregion

            return list;

            }

       

         ///////////////////////// Help Windows:
        protected override List<HelpTopic> InitHelpTopics()
        {
                List<HelpTopic> list = new List<HelpTopic>();

               

                    list.Add(new HelpTopic()
                    {
                        KeyName = "introduction",
                        Name = "0: Introduction",
                       
                         FlowElements = new LayoutElement[]
                         {
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 0: INTRODUCTION",HelpTopic.ColorHeader) +"\n \n You have been issued the Pioneer Planning Unit (PPU) to assist your group of pioneers in managing a settlement. The unit is a combination of instruments designed to coordinate your group's agreements as well as gather information about your collective, inventory and environment. \nIt is recommended that you familiarize yourself with the PPU in order to best organize your group's decisions." } //mp too long:  \nTherefore, study the following sections for instructions on how to use the Pioneer Planning Unit.
                             }
                         }

                    });

                    //ItemType = new ItemType() { Bulk = 0.1f, MayStoreInHome = false, },

                    list.Add(new HelpTopic()
                    {
                        KeyName = "selections",
                        Name = "1: Selections",

                        FlowElements = new LayoutElement[]
                         {

                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 1: SELECTIONS AND MAP",HelpTopic.ColorHeader) + "\n \nAbbreviations used: \nLMB: Left mouse button. RMB: Right mouse button. \n \nTERRAIN VIEW \nRMB-drag on terrain to move the terrain view. \nLMB-click/drag in the mini-map to center the terrain view at the chosen spot. \n \nSELECTION ZONE \nLMB-drag on terrain to make a multi-tile selection zone. \nLMB-click on terrain to make a single-tile selection zone. \nInformation about the zone will appear in the side panel. \n \nRMB-click on terrain to remove selection zone and side panel." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_1_zoneSidePanel" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = "\n \nSELECTING AN ENTITY \n('Entity' is the word used for an individual item, structure, creature or human.) \nTo select an entity, drag a zone around the entity on the terrain. Then, either: \n \nRepeatedly click the 'cycle entity' button at the corner of the zone:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_16_signalPyreCycleButton" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nOR \n \nLook at the side panel, click the ZONE button and expand the menus until you find the name of the entity. \nIf it's a person or creature, click the underlined name. \nIf it's an item, click the number button on the right side of the name. On the appearing pop-up menu, find the item you're looking for and click 'SELECT" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_1_selectItemList" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = "\n \nDISPLAY INFO ABOUT A SELECTED ENTITY OR ZONE \nYou can have both a zone and an entity selected at the same time. Display information about the selected zone by clicking the ZONE button on the side menu. Display information about the selected entity by clicking the ENTITY button:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_1_displayEntity" }
                             },
                         }                      
                    });

                    list.Add(new HelpTopic()
                    {
                        KeyName = "activityZones",
                        Name = "2: Activity zones",

                        FlowElements = new LayoutElement[]
                         {                             
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 2: ACTIVITY ZONES",HelpTopic.ColorHeader) +"\n \nA selected zone can be designated for activities by clicking NEW at the zone corner, then choosing from the dropdown menu. A zone can have several activities assigned at a time. Once an activity zone has been created, it will stay active until all orders have been carried out or you delete it. \nRMB-clicking on the terrain will hide all zones but they can be brought to reappear by LMB-clicking in the terrain view. Each zone can be modified by clicking the Expand arrow next to the zone window, this opens the zone menu giving the option to DELETE the whole zone or modify its activities. \n \nGATHER: Opens a window where you can specify what resources to collect from the zone. LMB-drag the order control slider to the right. The numbers update according to how much is available and how much has been gathered. Reduce or cancel the order by dragging the slider left. \nStanding Gather order: Click the padlock icon that appears to the left of the order control when you move you cursor there. This allows you to automate gathering (see image below)" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tutClayPit_standingGather" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \n \nSCOUT: Designates the area for a brief reconnaissance. \n \nEXAMINE: Designates the zone for a thorough investigation. Performing this activity can reveal previously unseen resources. \n \nPATROL: Each patrol zone will have one (armed) camp member continously patrolling the area as long as the zone exists. The patrolling person will attack any threats that appear (NOTE: The patroller needs to be in 'Fearless' stance to carry out this task) \n \nHUNT: Specifies that hunting should be carried out in the zone. \nIt is also possible to designate a specific creature to be hunted by selecting the animal or, in case it's disappeared from view, the memory image that indicates its last known position. Then click the 'Expand' arrow on its Marker window (the small 'name tag') and select HUNT. \n \nSTOCKPILE: Opens a window that allows you to define the types of  items to store in the zone. It is possible to choose specific item types or use broader item categories." }
                             },
                         }  
                        
                    });

                    list.Add(new HelpTopic()
                    {
                        KeyName = "items",
                        Name = "3: Items and production",

                        FlowElements = new LayoutElement[]
                         {                             
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 3: ITEMS, PRODUCTION AND INVENTORY",HelpTopic.ColorHeader) +"\n \nTo see a list of all items possessed by the colony, click the PRODUCTION MANAGER button:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_inventoryButton" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nClick an item category to expand. Item types written in dark text means that at least one item of that type is owned by the colony - the amount is shown next to the type name. Yellow text means that there's no item of that type in the colony's possession (The amount will say 0) \n \nIf an item can be produced, there will be an order control bar to the right. Drag the slider to request production:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_9_produceMashedOilTubers" }
                             },
                            new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nStanding Production Order:  Click the padlock button as seen in the image below. This allows you to automate production - when your stock falls below the specified amount, your people will automatically resume production. Switch back to 'single order' by clicking the padlock button again." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tutClayPit_standingProduction" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \n \nINFO WINDOWS \nEach item type has two kinds of information: Data info and production info. \nHover with the cursor on the item type to bring up a short info summary. Click the DATA icon to expand the info window with further information of the item type's properties:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_3_dataInfo" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nClick the PRODUCTION icon to see what can be produced with the item type. \n \nClick any item name to open a new window and see how this item is produced. Repeat as many times you like to get an overview of long production chains. \n \nNOTE: Scroll down by hovering the cursor on the scroll arrow at the bottom of the window." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_3_productionInfo" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \n \nTo see information about item entities owned by the colony, click the amount number and select an entity from the menu that appears. This opens the possibility to DISCARD the specific item (useful when moving camp and choosing what items to leave.) The item can be reclaimed by selecting the entity and clicking CLAIM. \nThe SALVAGE option will be available if it's possible to disassemble the item:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_3_salvageItem" }
                             },
                         }  
                        
                    });

                    list.Add(new HelpTopic()
                    {
                        KeyName = "building",
                        Name = "4: Building structures",

                         FlowElements = new LayoutElement[]
                         {                             
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 4: BUILDING STRUCTURES",HelpTopic.ColorHeader) +"\n \nIn the Production Manager, structures that can be built have a 'hammer' button next to them. Click the button, then click on the game area to place the structure." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_buildButton" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nAs is the case with item production, the number of options is dependent on the materials and tools owned by the colony. \nInvestigate the structure types by clicking the name bar. Click the production info icon to get an overview of the production chain, opening as many production info windows as needed." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_8_buildCampfireMenu_Small" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nSALVAGE a structure by selecting it, clicking the 'Expand' arrow on the marker window (the name tag) and select SALVAGE from the menu." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_3b_salvage" }
                             },

                         }  
                    });

                    list.Add(new HelpTopic()
                    {
                        KeyName = "managing",
                        Name = "5: Managing tasks",

                         FlowElements = new LayoutElement[]
                         {                             
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 5: MANAGING TASKS",HelpTopic.ColorHeader) +"\n \nYou can monitor the progress of assignments by clicking the TASK button:" }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_taskManagerButton" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nIf any task orders have been given, they will be listed here. Click the ARROW button at each entry to see status details of materials, tools and manpower needed to carry out the job. \nIf needed, set priorities for each task: Low, Normal or High. \nSome tasks can also be cancelled from the Task manager." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "help_5_taskManager" }
                             },
                         } 
                    });

                    list.Add(new HelpTopic()
                    {
                        KeyName = "moveCamp",
                        Name = "6: Move camp",

                        FlowElements = new LayoutElement[]
                         {                             
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = Label.ToLabel("SECTION 6: MOVE CAMP",HelpTopic.ColorHeader) +"\n \nClick the 'CAMP' marker window you see in the terrain view. Click the 'Expand' arrow and select MOVE CAMP from the drop-down menu, then click somewhere on dry land to designate a new camp site. This indicates a new assembly point for your colony members and a new area for storing supplies." }
                             },
                             new LayoutElement()
                             {
                                  Image = new ImageElement() { Image = "tut_7a1_moveCampMenu" }
                             },
                             new LayoutElement()
                             {
                                  Text = new TextElement() { Text = " \nTo avoid camp members carrying unneeded items to the new site, select the items that you want to leave at the old site and click DISCARD from the item list window (as described in SECTION 1: SELECTIONS AND MAP). The items can later be added to the colony again by selecting them and clicking CLAIM." }
                             },
                         } 
                        
                    });
                    
            

            //    SerializeAndDeserializeTypeList(list, GameData.Instance.AllHelpTopics, "", "helpTopics.xml", Config.DataType.BaseData);

                    return list;

         }

        protected override List<EventActionType> InitEventActionTypes()
        {
            return EventActionsLoader.Init();
        }

        protected override List<ActionSets> InitActionSets()
        {            
            return ActionSetsLoader.Init();        
        }

        protected override List<DamageType> InitDamageTypes()
        {

            List<DamageType> list = new List<DamageType>();

            list.Add(new DamageType()
            {
                 KeyName = "sharp"
            });

            list.Add(new DamageType()
            {
                KeyName = "blunt"
            });

            list.Add(new DamageType()
            {
                KeyName = "piercing"
            });

            list.Add(new DamageType()
            {
                KeyName = "fire"
            });

            list.Add(new DamageType()
            {
                KeyName = "bite"
            });

            list.Add(new DamageType()
            {
                KeyName = "antiTwinkler"
            });

            list.Add(new DamageType()
            {
                KeyName = "smallAnimalGrapple"
            });


            return list;
        }

        protected override List<AttackType> InitAttackTypes()
        {
           
                List<AttackType> list = new List<AttackType>();

                list.Add(new AttackType()
                {
                    KeyName = "trapSmallBluntAttack",
                    Damage = "blunt",
                    DamageMean = 50,
                    DamageStandardDeviation = 1.5f,
                    AccuracyFactor = 0.8f, // there should be a chance for escape...
                    ActionPointSound = GameData.Instance.AllSoundData["activities/melee/kickHard1B"],//something blunt hitting/falling. ..could also introduce 1A in basedataloader.
                 //   ImpactSound = GameData.Instance.AllSoundData[""],
                    SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"], //sound of trigger
                    // MaxRestTimeInSeconds = 3f,
                    // MinRestTimeInSeconds = 1f,
                    //ChanceToRest = 0 0.4f, //0.4f
                    DurationInSeconds = 1f, // 1.6f, //todo
                    ActionPointInSeconds = 0f, //
                    //  AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme },
                });
                list.Add(new AttackType()
                {
                    KeyName = "trapMediumPiercingAttack",
                    Damage = "piercing",
                    DamageMean = 100,
                    DamageStandardDeviation = 1.5f,
                    AccuracyFactor = 0.9f, // there should be a chance for escape...
                    ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/arrowImpact1a"],//a kind of heavy, sharp sound
                 //   ImpactSound = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],
                    SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"], //sound of trigger
                    // MaxRestTimeInSeconds = 3f,
                    // MinRestTimeInSeconds = 1f,
                    //ChanceToRest = 0 0.4f, //0.4f
                    DurationInSeconds = 1f, // 1.6f, //todo
                    ActionPointInSeconds = 0f, //
                    //  AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme },
                });
                list.Add(new AttackType()
                {
                    KeyName = "killAnimal",
                    Damage = "blunt",
                    DamageMean = 5000,
                    DamageStandardDeviation = 0f,
                    AccuracyFactor = 1f, // 
                    //ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
                    //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                    SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],
                    // MaxRestTimeInSeconds = 3f,
                    // MinRestTimeInSeconds = 1f,
                    //ChanceToRest = 0 0.4f, //0.4f
                    DurationInSeconds = 1f, // 1.6f, //todo
                    ActionPointInSeconds = 0f, //
                    //  AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme },
                });
                list.Add(new AttackType()
                {
                    KeyName = "smallExplosionAttack",
                    Damage = "blunt",
                    DamageMean = 500,
                    DamageStandardDeviation = 1.5f,
                    AccuracyFactor = 1f, //bso changed from 0.9 for recording // there should be a chance for escape...
                    AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                    {
                        Length = 48f,
                        WidthInDegrees = 360f
                    },
                    StartEffects = new Effects()
                    {
                        ParticleEmitters = new ParticleEmitterEffect[]
                        {
                            new ParticleEmitterEffect()
                                {
                                    DurationInSeconds=0.1,
                                    AttachToEntity = false,
                                    ParticleSystemKey = "explosionSmokeCloud",
                                },
                                new ParticleEmitterEffect()
                                {
                                    DurationInSeconds=0.1,
                                    AttachToEntity = false,
                                    ParticleSystemKey = "explosion",
                                },
                                new ParticleEmitterEffect()
                                {
                                    DurationInSeconds=0.1,
                                    AttachToEntity = false,
                                    ParticleSystemKey = "mineExplosion",
                                }
                        }
                    },
                   /* EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>()
                    {
                        {AgentActionHooks.HitEnemy,new List<ActionSets>()
                            {
                                {
                                    #region action set
                                    new ActionSets()
                                    {
                                        FireMode = ActionSetsToFire.AllValid,
                                        KeyName = "landMineTriggered",
                                        SetsOfActions = new []
                                            {
                                                new ActionSetType("bso325cd398-2f8a-4cd3-8cd0-4e0cbzxcve3e3e9a1b")
                                                        {
                                                            Actions = new EventActionType[]
                                                            {    
                                                                new EventActionType("bso7f1bfd30330-81d0-4b1e-80e1-7d6dzsaed2w085099040") 
                                                                {
                                                                    //DelayInSeconds = 0.5d,
                                                                    ParticleEffectAction = new ParticleEffectAction()
                                                                    {
                                                                        UseLocationOfEntity = new TargetObject()
                                                                        { 
                                                                            TargetElement = TargetObjectType.TriggeringEntity 
                                                                        },
                                                                        DurationInSeconds = 0.5d,
                                                                        ParticleEmitters = new[]
                                                                        { 
                                                                            new ParticleEmitterEffect()
                                                                            {
                                                                                AttachToEntity = false, // keep emitting after nest destroyed
                                                                                ParticleSystemKey = "explosionSmokeCloud",
                                                                            }
                                                                        }
                                                                    }
                                                                },
                                                                new EventActionType("bso7f1bfd30330-81d0-4b1e-80e1-7d608534r3fa3099040") 
                                                                {
                                                                    ParticleEffectAction = new ParticleEffectAction()
                                                                    {
                                                                        UseLocationOfEntity = new TargetObject()
                                                                        { 
                                                                            TargetElement = TargetObjectType.TriggeringEntity 
                                                                        },
                                                                        DurationInSeconds = 0.1d,
                                                                        ParticleEmitters = new[]
                                                                        { 
                                                                            new ParticleEmitterEffect()
                                                                            {
                                                                                AttachToEntity = false, // keep emitting after nest destroyed
                                                                                ParticleSystemKey = "explosion",
                                                                            }
                                                                        }
                                                                    }
                                                                },
                                                                new EventActionType("bso7f1bfd30330-81d0-4b1e-80e1-7d608509asf23q39040") 
                                                                {
                                                                    ParticleEffectAction = new ParticleEffectAction()
                                                                    {
                                                                        UseLocationOfEntity = new TargetObject()
                                                                        { 
                                                                            TargetElement = TargetObjectType.TriggeringEntity 
                                                                        },
                                                                        DurationInSeconds = 0.1d,
                                                                        ParticleEmitters = new[]
                                                                        { 
                                                                            new ParticleEmitterEffect()
                                                                            {
                                                                                AttachToEntity = false, // keep emitting after nest destroyed
                                                                                ParticleSystemKey = "mineExplosion",
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                            }
                                    }
#endregion
                                }
                            }
                        }


                    },*/
                    ActionPointSound = GameData.Instance.AllSoundData["traps/mineExplosionHardwDebris"], //Explosion sound
                    //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                    SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"], //sound of trigger // this is for lowtech trigger string..  if electronic trigger: "traps/mineDetonateBeep"
                    // MaxRestTimeInSeconds = 3f,
                    // MinRestTimeInSeconds = 1f,
                    //ChanceToRest = 0 0.4f, //0.4f
                    DurationInSeconds = 0f, // 1.6f, //todo
                    ActionPointInSeconds = 0f, //
                    //  AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme },
                });


                list.Add(new AttackType()
                {
                    KeyName = "snatcherGrabAttack", //todo all below
                    Damage = "sharp",
                    DamageMean = 15,
                    DamageStandardDeviation = 1.5f,
                    // AnimationKeyz = "attackHighDouble",

                    //ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
                    //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                    SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/snatcherAttack"],
                    MaxRestTimeInSeconds = 3f,
                    MinRestTimeInSeconds =1f,
                    ChanceToRest = 0.4f, //0.4f
                    DurationInSeconds = 1.6f, //todo
                    ActionPointInSeconds = 0.56f, //todo
                    AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme },
                    RequiredSkill = "unarmedFighting"
                });

                list.Add(new AttackType()
                {
                    KeyName = "snatcherFastAttack",
                    Damage = "sharp",
                    DamageMean = 15,
                    DamageStandardDeviation = 1.5f,
                    // name of anim in Creatureloader.cs: "attackLowRight",

                    //ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnap"],
                    //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                    SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/snatcherAttack"],
                    MaxRestTimeInSeconds = 3f,
                    MinRestTimeInSeconds = 1f,
                    ChanceToRest = 0.2f, //0.4f
                    DurationInSeconds = 0.8f, //done
                    ActionPointInSeconds = 0.32f, //done
                    AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Low, AnimModifier.Right },
                    RequiredSkill = "unarmedFighting"
                });  



                    list.Add(new AttackType()
                    {
                        KeyName = "patricianHighDouble",
                        Damage = "piercing",
                        DamageMean = 17,
                        DamageStandardDeviation = 1.5f,
                       // AnimationKeyz = "attackHighDouble",

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                //        SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],
                        MaxRestTimeInSeconds = 0.6f,
                        MinRestTimeInSeconds = 0.2f,
                        ChanceToRest = 0.4f,
                        DurationInSeconds = 0.92f,
                        ActionPointInSeconds = 0.375f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Extreme }, 
                        RequiredSkill = "unarmedFighting"
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "patricianLowRight",
                        Damage = "piercing",
                        DamageMean = 13,
                        DamageStandardDeviation = 1.5f,
                        // name of anim in Creatureloader.cs: "attackLowRight",

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnap"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
               //         SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],

                        DurationInSeconds = 1f, //25 frames
                        ActionPointInSeconds = 0.54f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Low, AnimModifier.Right },
                        RequiredSkill = "unarmedFighting"
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "demonTreeAttack", //mp new key dec 2
                        Damage = "smallAnimalGrapple",
                        DamageMean = 40,
                        DamageStandardDeviation = 1f,
                      

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                        SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],

                        DurationInSeconds = 3f, //done
                        ActionPointInSeconds = 2f, //done
                        AnimationStates = new AnimModifier[] { },// AnimModifier.Near
                        RequiredSkill = "unarmedFighting"
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "spikePlantAttack", //
                        Damage = "piercing",
                        DamageMean = 10,
                        DamageStandardDeviation = 1.5f,
                        /*MaxRange = 100f,
                        MinRange=-1f,
                        RangeType = AttackType.RangeTypes.Ballistic,*/
                        AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                        {
                            Length = 48f,
                            WidthInDegrees = 360f
                        },

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                        SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],

                        DurationInSeconds = 0.12f, //done
                        ActionPointInSeconds = 0.52f, //done
                        AnimationStates = new AnimModifier[] { AnimModifier.Low },
                        RequiredSkill = "unarmedFighting"
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "wormAttack", //
                        Damage = "piercing",
                        DamageMean = 20,
                        DamageStandardDeviation = 1.5f,


                        ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/wormAttackPart2"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                        SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/wormAttackPart1"],

                        DurationInSeconds = 2.8f, //done
                        ActionPointInSeconds = 1.56f, //done
                        AnimationStates = new AnimModifier[] { },
                        RequiredSkill = "unarmedFighting"
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "dogBiting",
                        DefenseRating = LowDefenseRating, //was MiddleDefenseRating
                        Damage = "bite",
                        DamageMean = 10, // 7, // 12,
                        DamageStandardDeviation = 2f, // 1.5f,
                        // name of anim in Creatureloader.cs: "attack",
                        //the sounds are put in SoundAndAnimationSet to be able to randomize between 2 different sounds.
                    //    SoundAtStart = GameData.Instance.AllSoundData ["domesticated/dog/dogAttackSnarl2"],
                   //     ActionPointSound = GameData.Instance.AllSoundData["aliens/twinklerStab"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact                          

                        MaxRestTimeInSeconds = 0.8f,
                        MinRestTimeInSeconds = 0,

                        DurationInSeconds = 1.36f,
                        ActionPointInSeconds = 0.28f,
                        AnimationStates = new AnimModifier[] {  AnimModifier.Low },
                        RequiredSkill = "unarmedFighting"
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "twinklerLowRight",
                        Damage = "piercing",
                        DamageMean = 8, // 12,
                        DamageStandardDeviation = 1.5f,
                        // name of anim in Creatureloader.cs: "attackLowRight",

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/twinklerStab"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact                          
                        //   SoundAtStart = "aliens/twinklerStab",   didn't work...18th oct
                        MaxRestTimeInSeconds = 0.8f,
                        MinRestTimeInSeconds = 0,
                        
                        DurationInSeconds = 0.68f,
                        ActionPointInSeconds = 0.29f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Low, AnimModifier.Right },
                        RequiredSkill = "unarmedFighting"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/
                    });
                    list.Add(new AttackType()
                    {
                        KeyName = "twinklerHighRight",
                        Damage = "piercing",
                        DamageMean = 8, // 12,
                        DamageStandardDeviation = 1.5f,
                        // name of anim in Creatureloader.cs: "attackHighRight",

                        ActionPointSound = GameData.Instance.AllSoundData["aliens/twinklerStab"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact                          
                   //   SoundAtStart = "aliens/twinklerStab",   didn't work...18th oct 
                        MaxRestTimeInSeconds = 1.1f,
                        MinRestTimeInSeconds = 0.2f,

                        DurationInSeconds = 1f,
                        ActionPointInSeconds = 0.5f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Right },
                        RequiredSkill = "unarmedFighting"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/
                    });


                    float bushDragonAttackDuration = 1f;
                    float bushDragonAttackActionPoint = 0.4f;
                    list.Add(new AttackType()
                    {
                        KeyName = "bushDragonSpray",
                        DefenseRating = MiddleDefenseRating,
                        Damage = "antiTwinkler",
                        DamageMean = 3,//12, changed to test twinkler fleeing.
                        DamageStandardDeviation = 1.5f,
                        ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonPoisonShot"],
                        ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
                        SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonAttack"],
                        DurationInSeconds = 1f,
                        ActionPointInSeconds = 0.4f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Near },
                        RequiredSkill = "unarmedFighting",/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
               /*         BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Yellow,
                            EndColor = Color.LightGreen
                        }*/

                        AccuracyFactor = 2f, // it should be hard to miss with this attack
                        AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                        {
                            Length = 120f, //120f
                            WidthInDegrees = 26f
                        },
                        ActionPointEffects = new Effects()
                        {
                            ParticleEmitters = new ParticleEmitterEffect[]
                              {
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true, // attach to shooter
                                       EmitParticlesInParentDirection = true, // emit particles in the shooter's facing direction
                                       Intensity = 1f,
                                       ParticleSystemKey = "bushDragonSpray",  //"flamePlume", // "waterSpray", 
                                       DurationInSeconds = bushDragonAttackDuration - bushDragonAttackActionPoint // keep emitting in the time from actionpoint to end of attack
                                  }                                    
                              }
                        },




               /*
                        PoisonPlumeEffects = new Effects()
                        {
                            ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true } }
                        }*/
                        // ParticleEmitters = new[]{ new ParticleEmitterEffect(){ ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true }  }
                    });


                    BodyType bodyType = GameData.Instance.AllBodyTypes["humanoid"];
                    //Person Melee Attack Types
                    list.Add(new AttackType()
                    {
                        KeyName = "personPunchHighRight",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "blunt",
                        DamageMean = 3,
                        DamageStandardDeviation = 1,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackPunchHighRight",

                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/punchLight1"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh1"],

                        DurationInSeconds = 0.6f,
                        MissDurationInSeconds = 0.72f,
                        ActionPointInSeconds = 0.41f,  // from 0 , it's frame 10. 9*0,04=0,36.  10*0,04= 0.4
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.High, AnimModifier.Right, AnimModifier.Near },
                        RequiredSkill = "unarmedFighting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    float knifeDamage = 7f;
                    list.Add(new AttackType()
                    {
                        KeyName = "knifeHack", // copied from machete hack
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "sharp",
                        DamageMean = knifeDamage, //mp atm nov 2015 only for diamond knife.
                        DamageStandardDeviation = 2.5f,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackClubHigh",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
                        DurationInSeconds = 0.6f, // from 0 , it's 16. (15*0,04=0,6)
                        ActionPointInSeconds = 0.4f, // from 0 , it's frame 11. 10*0,04=0,4.
                        AnimationStates = new AnimModifier[] { AnimModifier.Right, AnimModifier.Near, AnimModifier.Knife },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });
                    list.Add(new AttackType()
                    {
                        KeyName = "improvisedKnifeHack", // copied from knife hack
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "sharp",
                        DamageMean = 0.66f * knifeDamage, //mp atm nov 2015 all other knives than diamond knife
                        DamageStandardDeviation = 2.5f,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackClubHigh",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
                        DurationInSeconds = 0.6f, // from 0 , it's 16. (15*0,04=0,6)
                        ActionPointInSeconds = 0.4f, // from 0 , it's frame 11. 10*0,04=0,4.
                        AnimationStates = new AnimModifier[] { AnimModifier.Right, AnimModifier.Near, AnimModifier.Knife },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                   list.Add(new AttackType()
                    {
                        KeyName = "personPunchLowRight",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "blunt",
                        DamageMean = 3,
                        DamageStandardDeviation = 1,
                        AccuracyFactor = 1f, //new
                     //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/punchLight2"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
                        // name of anim in Creatureloader.cs: "attackPunchLowRight",
                        DurationInSeconds = 0.8f, // keyCount says 22, but this is wrong...fbx has been exported with one too many still keys at the end...cycle ends at 21. 20*0,04= 0,8
                        MissDurationInSeconds = 0.72f,
                        ActionPointInSeconds = 0.41f,
                        AnimationStates = new AnimModifier[] { /*AnimState.Attacking,*/ AnimModifier.Right, AnimModifier.Near },
                        IsDownAttack = true,
                        RequiredSkill = "unarmedFighting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });
                    list.Add(new AttackType()
                    {
                        KeyName = "personSnapkickRight",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "blunt",
                        DamageMean = 4,
                        DamageStandardDeviation = 1,
                        AccuracyFactor = 0.9f, //new
                        // name of anim in Creatureloader.cs: "attackSnapkickRight",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow1"],
                        DurationInSeconds = 0.52f, // 0.48f, // keycount 13: 25 keys/second
                        ActionPointInSeconds = 0.21f,
                        MissDurationInSeconds = 0.72f, // we are playing the punch miss anim here for now...
                        AnimationStates = new AnimModifier[] { AnimModifier.Low, AnimModifier.Right, AnimModifier.Near },
                        IsDownAttack = true,
                        RequiredSkill = "unarmedFighting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right leg") },
                        ChanceToRest = 0.8,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.4f
                    });
                    list.Add(new AttackType()
                    {
                        KeyName = "personStompRight",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "blunt",
                        DamageMean = 6,
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 0.8f, //new
                        // name of anim in Creatureloader.cs: "attackStompRight",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
                        DurationInSeconds = 0.64f,
                        ActionPointInSeconds = 0.46f,
                        MissDurationInSeconds = 0.72f, // we are playing the punch miss anim here for now...
                        AnimationStates = new AnimModifier[] { AnimModifier.Low,  AnimModifier.Right, AnimModifier.Near, AnimModifier.Extreme },
                        IsDownAttack = true,  //info for the sim, not implemented yet (as opposed to animstate.low which is implemented...for the client)
                        RequiredSkill = "unarmedFighting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right leg") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });
                    list.Add(new AttackType()
                    {
                        KeyName = "personHack",
                        DefenseRating = LowDefenseRating,
                        Damage = "sharp",
                        DamageMean = 20,
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackClubHigh",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
                        DurationInSeconds = 0.6f, // from 0 , it's 16. (15*0,04=0,6)
                        ActionPointInSeconds = 0.4f, // from 0 , it's frame 11. 10*0,04=0,4.
                        AnimationStates = new AnimModifier[] { AnimModifier.Right, AnimModifier.Near, AnimModifier.Machete, AnimModifier.Axe },                      
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "personHammerBlow",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "blunt",
                        DamageMean = 10,
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 0.9f, //new
                        // name of anim in Creatureloader.cs: "attackClubHigh",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
                        DurationInSeconds = 0.6f, // from 0 , it's 16. (15*0,04=0,6)
                        ActionPointInSeconds = 0.4f, // from 0 , it's frame 11. 10*0,04=0,4.
                        AnimationStates = new AnimModifier[] { AnimModifier.Right, AnimModifier.Near, AnimModifier.Hammer },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "personPickaxeHack",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 20, //todo?
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackPickaxe",
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
                        DurationInSeconds = 0.6f, // todo
                        ActionPointInSeconds = 0.4f, // todo
                        AnimationStates = new AnimModifier[] { AnimModifier.Right, AnimModifier.Near, AnimModifier.Pickaxe },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "personCrudeSpearThrustMid",
                        DefenseRating = UnderLowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 12,
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackSpearMid"
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
                        DurationInSeconds = 0.84f,                       
                        ActionPointInSeconds = 0.44f,  //on frame 12(starting from 0), so 11 *0,04 = 0.44 RIGHT??!?
                        AnimationStates = new AnimModifier[] { AnimModifier.Near, AnimModifier.Spear },                    
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });



                    list.Add(new AttackType()
                    {
                        KeyName = "personFlintSpearThrustMid",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 23, //MP sep 2014: was 24 when there was only one type of spear
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackSpearMid"
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh1"],
                        DurationInSeconds = 0.84f,
                        ActionPointInSeconds = 0.44f,  //on frame 12(starting from 0), so 11 *0,04 = 0.44 RIGHT??!?
                        AnimationStates = new AnimModifier[] { AnimModifier.Near, AnimModifier.Spear },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });


                    list.Add(new AttackType()
                    {
                        KeyName = "personMetalSpearThrustMid",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 28, //MP sep 2014: was 24 when there was only one type of spear
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 1f, //new
                        // name of anim in Creatureloader.cs: "attackSpearMid"
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
                        DurationInSeconds = 0.84f,
                        ActionPointInSeconds = 0.44f,  //on frame 12(starting from 0), so 11 *0,04 = 0.44 RIGHT??!?
                        AnimationStates = new AnimModifier[] { AnimModifier.Near, AnimModifier.Spear },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });



                    list.Add(new AttackType()
                    {
                        KeyName = "personHoeHack",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "sharp",
                        DamageMean = 12,
                        DamageStandardDeviation = 2,
                        AccuracyFactor = 0.8f, //new
                        // name of anim in Creatureloader.cs: "attackHoeMid"
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
                        DurationInSeconds = 0.84f, //mp apr 2015: copied this from "personCrudeSpearThrustMid" . check if it's correct.
                        ActionPointInSeconds = 0.44f,  //mp apr 2015: copied this from "personCrudeSpearThrustMid" . check if it's correct.
                        AnimationStates = new AnimModifier[] { AnimModifier.Near, AnimModifier.Hoe },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "personShovelHack",
                        DefenseRating = VeryLowDefenseRating,
                        Damage = "sharp",
                        DamageMean = 9, //a clumsy weapon
                        DamageStandardDeviation = 3,
                        AccuracyFactor = 0.8f, //new
                        // name of anim in Creatureloader.cs: "attackHoeMid"
                        //   ActionPointSound = "",  //no sound when fist is extended if there's no impact.
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],  //sound of impact
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
                        DurationInSeconds = 0.84f, //mp july 2015: copied this from "personHoeHack" . check if it's correct.
                        ActionPointInSeconds = 0.44f,  //mp july  2015: copied this from "personHoeHack" . check if it's correct.
                        AnimationStates = new AnimModifier[] { AnimModifier.Near, AnimModifier.Shovel },
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });




                    list.Add(new AttackType()
                    {
                        KeyName = "hitWithRifleButt",
                        Damage = "blunt",
                        DamageMean = 10, // so that they dont use stomp when they have a gun.
                        DamageStandardDeviation = 1,
                        AccuracyFactor = 0.8f, //new
                        ActionPointInSeconds = 0.44f,
                        DurationInSeconds = 0.88f,
                        ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],
                        SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
                        AnimationStates = new AnimModifier[] { AnimModifier.Near }, // mp removed  AnimModifier.Hoe
                        RequiredSkill = "armedMelee",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        RangeType = AttackType.RangeTypes.Melee,
                        ChanceToRest = 0.3,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    //Ranged AttackTypes:

                    #region shootUnrifledBullet
                    list.Add(new AttackType()
                    {
                        KeyName = "shootUnrifledBullet",
                        DefenseRating = MiddleDefenseRating,
                        Damage = "piercing",
                        DamageMean = 50,
                        DamageStandardDeviation = 4,
                        AccuracyFactor = 0.85f,
                        ActionPointInSeconds = 0.1f,
                        DurationInSeconds = 1.24f,
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 130f,
                        RangeType = AttackType.RangeTypes.Ray,
                       
                        ActionPointEffects = new Effects()
                        {
                            ParticleEmitters = new ParticleEmitterEffect[]
                              {                                 
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true,
                                       EmitParticlesInParentDirection = true,
                                       Intensity = 1f,
                                       ParticleSystemKey = "gunSmoke",
                                       DurationInSeconds = 12
                                  }
                              }
                        },
                        UsesAmmo = "item:blackPowderShotAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8,
                        MinRestTimeInSeconds = 0.75f,
                        MaxRestTimeInSeconds = 2f
                    });
                    #endregion

                    #region shootBlunderbuss
                    list.Add(new AttackType()
                    {
                        KeyName = "shootBlunderbuss",
                        DefenseRating = MiddleDefenseRating,
                        Damage = "piercing",
                        DamageMean = 26,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = 1.5f,
                        ActionPointInSeconds = 0.1f,
                        DurationInSeconds = 1.24f,
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"], 
                        AnimationStates = new AnimModifier[] { AnimModifier.Far }, 
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
                        AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                        {
                            Length = 110f, 
                            WidthInDegrees = 10f,
                        },
                        ActionPointEffects = new Effects()
                        {
                            ParticleEmitters = new ParticleEmitterEffect[]
                              {
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true,
                                       EmitParticlesInParentDirection = true,
                                       Intensity = 1f,
                                       ParticleSystemKey = "buckShotCloud",
                                       DurationInSeconds = 1
                                  },
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true,
                                       EmitParticlesInParentDirection = true,
                                       Intensity = 1f,
                                       ParticleSystemKey = "gunSmoke",
                                       DurationInSeconds = 12
                                  }
                              }
                        },
                        UsesAmmo = "item:blackPowderShotAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8,
                        MinRestTimeInSeconds = 0.75f,
                        MaxRestTimeInSeconds = 2f
                    });
                    #endregion

                    list.Add(new AttackType()
                    {
                        KeyName = "shootShotgun",
                        DefenseRating = HighDefenseRating,
                        Damage = "piercing",
                        DamageMean = 28,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = 1.5f,
                        ActionPointInSeconds = 0.1f,
                        DurationInSeconds = 1.24f,
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 180f, //modern shotgun. mp was 125f but painfully short for a shotgun sentry..it seemed buggy that it didnt shoot. suggestion: add range overlay for the player to see. 
                        RangeType = AttackType.RangeTypes.Ray,
                        AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                        {
                            Length = 140f,
                            WidthInDegrees = 10f,
                        },
                        ActionPointEffects = new Effects()
                        {
                            ParticleEmitters = new ParticleEmitterEffect[]
                              {
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true,
                                       EmitParticlesInParentDirection = true,
                                       Intensity = 1f,
                                       ParticleSystemKey = "goldBuckShotCloud",
                                       DurationInSeconds = 1
                                  },
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true,
                                       EmitParticlesInParentDirection = true,
                                       Intensity = 0.5f,
                                       ParticleSystemKey = "gunSmoke",
                                       DurationInSeconds = 12
                                  }
                              }
                        },
                        UsesAmmo = "item:shotgunAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8,
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });



                    list.Add(new AttackType()
                    {
                        KeyName = "shootCoilRifle",
                        DefenseRating = HighestDefenseRating,
                        Damage = "piercing",
                        DamageMean = 100,
                        DamageStandardDeviation = 10,
                        AccuracyFactor = 1.5f, // modify by 150% chance to hit   also called "To Hit?"
                        ActionPointInSeconds = 0.6f, // Lars: I tweaked this number heavily. The supplied action point appeared FUBARED:  0.9f
                        DurationInSeconds = 1.24f, // 31 * 0,04
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilrifleShot"],
                        //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact
                        // name of anim in Creatureloader.cs: "attackPunchHighRight",
                        AnimationStates = new AnimModifier[] { AnimModifier.Far }, 
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 360f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Gray,
                            EndColor = Color.LightGray
                        },
                        UsesAmmo = "item:coilRifleAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });
                    #region "sentryShootGun" //mp we made burst fire instead on the sentry. this one is only used on some test robots, maybe delete to avoid confusion.
                    list.Add(new AttackType()
                    {
                        KeyName = "sentryShootGun", //mp we made burst fire instead on the sentry. this one is only used on some test robots, maybe delete to avoid confusion.
                        DefenseRating = HighestDefenseRating,
                        Damage = "piercing",
                        DamageMean = 5,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = 0.8f, // also called "To Hit?"
                        ActionPointInSeconds = 0.01f, // mp currently not related to any animation. so I just made it very short. but hard to get very quick fire though.
                        DurationInSeconds = 0.02f, // 
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
                        //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact
                        // name of anim in Creatureloader.cs: "attackPunchHighRight",
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",                      
                        MaxRange = 360f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Yellow,
                            EndColor = Color.LightYellow,
                        },
                    
                        RoundsToSpend = 1,
                        ChanceToRest = 0.02, // 
                        MinRestTimeInSeconds = 0.2f,
                        MaxRestTimeInSeconds = 0.4f
                    });
                    #endregion
                    // for the sentry robot:
                    list.Add(new AttackType()
                    {
                        KeyName = "sentryGunBurst",
                        DefenseRating = HighestDefenseRating,
                        Damage = "piercing",
                        DamageMean = 36, //12,
                        DamageStandardDeviation = 3,       //mp no accuracy factor??                
                        ActionPointInSeconds = 0.04f, //corresponds to sentry_shoot
                        DurationInSeconds = 0.2f, //
                        SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/sentryGun/sentryBurstCasingsLow"],
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/sentryGun/sentryMedium2Burst"],
                        //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact
                        // name of anim in Creatureloader.cs: "attackPunchHighRight",
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        MaxRange = 240f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Yellow,
                            EndColor = Color.LightYellow,
                        },
                        UsesAmmo = "item:sentryGunAmmo",         
                        RoundsToSpend = 3, // burst fire                        
                        ChanceToRest = 0.1, // 
                        MinRestTimeInSeconds = 0.1f,
                        MaxRestTimeInSeconds = 0.15f
                    });
            //New Attack just for the laser. ///////////////////////////////////
                    list.Add(new AttackType()
                    {
                        KeyName = "sentryLaserGunShot",
                        DefenseRating = HighestDefenseRating,
                        Damage = "piercing",
                        DamageMean = 20, //36,//50
                        DamageStandardDeviation = 3,       //mp no accuracy factor??                
                        ActionPointInSeconds = 0.01f, //corresponds to sentry_shoot//0.04f
                        DurationInSeconds = 0.1f, //0.2f
                        
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilrifleShot1a"], // sentryGun/sentryMedium2Burst
                        //ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact
                        // name of anim in Creatureloader.cs: "attackPunchHighRight",
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        MaxRange = 150f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Red,
                            EndColor = Color.Tomato
                        },
                        RoundsToSpend = 0,                       
                        ChanceToRest = 1, 
                        MinRestTimeInSeconds = 3.00f, // was 0.1f
                        MaxRestTimeInSeconds = 3.00f // was 0.15f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "shootCorditeRifledBullet",
                        DefenseRating = HighDefenseRating,
                        Damage = "piercing",
                        DamageMean = 75, //mp was 40
                        DamageStandardDeviation = 5,
                        AccuracyFactor = 1.1f, 
                        ActionPointInSeconds = 0.6f, //  //mp: apr 2015: stefan had set this to 0.8f. should be same as coilrifle.
                        DurationInSeconds = 1.50f, // 31 * 0,04
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],  //sound of impact
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 325f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Gray,
                            EndColor = Color.LightGray
                        },
                        UsesAmmo = "item:corditeAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType() //placeholder - still need to finalize numbers
                    {
                        KeyName = "shootGunpowderRifle", //
                        DefenseRating = AboveMiddleDefenseRating,//was MiddleDefenseRating
                        Damage = "piercing",
                        DamageMean = 55, //mp was 25
                        DamageStandardDeviation = 5,
                        AccuracyFactor = 0.95f, 
                        ActionPointInSeconds = 0.6f, //mp: apr 2015: stefan had set this to 0.8f. should be same as coilrifle.
                        DurationInSeconds = 1.50f, // 31 * 0,04
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
                        ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "shooting",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 200f, //this is longer than bow but much shorter than other rifles
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = 20f,
                            StartColor = Color.Gray,
                            EndColor = Color.LightGray,
                        },
                        ActionPointEffects = new Effects()
                        {
                            ParticleEmitters = new ParticleEmitterEffect[]
                              {
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = false, // attach to shooter
                                       EmitParticlesInParentDirection = true, // emit particles in the shooter's facing direction
                                       Intensity = 1f,
                                       ParticleSystemKey = "gunpowderSmoke",
                                       DurationInSeconds = 5f
                                  }
                              }
                        },
                        UsesAmmo = "item:blackPowderRifleAmmo",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit 
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    float bowMuzzle = 20f;
                    list.Add(new AttackType()
                    {
                        KeyName = "shootImprovisedBasicArrow",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 18,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = 0.85f, // harder to hit with
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
                    //    SoundAtStart = "",

                        DurationInSeconds = 2.24f,
                        ActionPointInSeconds = 0.96f, 
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "archery",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm"), bodyType.FindBodyPart("Left arm") },
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = bowMuzzle,
                            StartColor = Color.Brown,
                            EndColor = Color.SandyBrown
                        },                       
                        UsesAmmo = "item:improvisedBasicArrow",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    float accuracyFactorOfArrowWithFletchings = 1f;
                    list.Add(new AttackType()
                    {
                        KeyName = "shootImprovisedChitinousArrow",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 20,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = accuracyFactorOfArrowWithFletchings, // because of fletchings
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
                        //    SoundAtStart = "",

                        DurationInSeconds = 2.24f,
                        ActionPointInSeconds = 0.96f, 
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "archery",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm"), bodyType.FindBodyPart("Left arm") },
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = bowMuzzle,
                            StartColor = Color.Brown,
                            EndColor = Color.SandyBrown
                        },
                        UsesAmmo = "item:improvisedChitinousArrow",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "shootImprovisedMetalArrow",
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 24,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = accuracyFactorOfArrowWithFletchings, // because of fletchings
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
                        //    SoundAtStart = "",

                        DurationInSeconds = 2.24f,
                        ActionPointInSeconds = 0.96f,
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "archery",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm"), bodyType.FindBodyPart("Left arm") },
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = bowMuzzle,
                            StartColor = Color.Brown,
                            EndColor = Color.SandyBrown
                        },
                        UsesAmmo = "item:improvisedMetalArrow",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

                    list.Add(new AttackType()
                    {
                        KeyName = "shootIronArrow", //mp copypasted from shootImprovisedMetalArrow but buffed the damageMean a bit.
                        DefenseRating = LowDefenseRating,
                        Damage = "piercing",
                        DamageMean = 26,
                        DamageStandardDeviation = 3,
                        AccuracyFactor = accuracyFactorOfArrowWithFletchings, // because of fletchings
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
                        ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
                        //    SoundAtStart = "",

                        DurationInSeconds = 2.24f,
                        ActionPointInSeconds = 0.96f,
                        AnimationStates = new AnimModifier[] { AnimModifier.Far },
                        RequiredSkill = "archery",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm"), bodyType.FindBodyPart("Left arm") },
                        MaxRange = 100f,
                        RangeType = AttackType.RangeTypes.Ray,
                        BulletEffect = new BulletEffect()
                        {
                            MuzzleDistance = bowMuzzle,
                            StartColor = Color.Brown,
                            EndColor = Color.SandyBrown
                        },
                        UsesAmmo = "item:ironArrow",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });


                    float rifleHipActionPoint = 0.18f; // 0.24f seems to take too long?

                
                    float fireExtinguishAttackDuration = 1.2f;
                    list.Add(new AttackType()
                    {
                        KeyName = "shootImprovedFireExtinguisherBushDragonPoison",
                        DefenseRating = MiddleDefenseRating,
                        Damage = "antiTwinkler",  // highly effective but only against twinklers. to code later: Should give morale damage cause them to flee
                        DamageMean = 120,    
                        DamageStandardDeviation = 1.5f,                  
                        DurationInSeconds = fireExtinguishAttackDuration,
                        ActionPointInSeconds = rifleHipActionPoint,
                        ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/extinguisherBoostShot"],
                        ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
                        AnimationStates = new AnimModifier[] { AnimModifier.Far, AnimModifier.Low },
                        RequiredSkill = "menial",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 144f, //75f
                        RangeType = AttackType.RangeTypes.Ray,
                        AccuracyFactor = 2f, // it should be hard to miss with this attack
                        AreaAttack = new ConeAttack() // spray in a cone. needs to correspond closely with the particle anim.
                        {
                            Length = 144f, //120f
                              WidthInDegrees = 26f
                        },
                         ActionPointEffects = new Effects()
                         {
                              ParticleEmitters = new ParticleEmitterEffect[]
                              {
                                  new ParticleEmitterEffect()
                                  {
                                       AttachToEntity = true, // attach to shooter
                                       EmitParticlesInParentDirection = true, // emit particles in the shooter's facing direction
                                       Intensity = 1f,
                                       ParticleSystemKey = "fireExtinguisherPoison",  //"flamePlume", // "waterSpray", 
                                       DurationInSeconds = fireExtinguishAttackDuration - rifleHipActionPoint // keep emitting in the time from actionpoint to end of attack
                                  }                                    
                              }
                         },
                        UsesAmmo = "item:bushDragonCartridge",
                        RoundsToSpend = 1,
                        ChanceToRest = 0.8, // single shot - rest a bit
                        MinRestTimeInSeconds = 0.4f,
                        MaxRestTimeInSeconds = 1.2f
                    });

            
/*
 //HIGHLY BUGGY now (nov 27 2013) ...don't use, postpone!!
                    list.Add(new AttackType()
                    {
                        KeyName = "throwSpear",  //make a smaller throwing spear for this. put it in itemloader MP .. 
                        Damage = "piercing",
                        DamageMean = 6,
                        DamageStandardDeviation = 2,
                   //     AnimationKeyz = "attackPunchHighRight",
                        AnimationStates = new AnimModifier[] { AnimModifier.Far, AnimModifier.Slow, AnimModifier.Right },
                        RequiredSkill = "spearThrowing",
                        DependsOn = new BodyPartType[] { bodyType.FindBodyPart("Right arm") },
                        MaxRange = 60f, // 50f, Lars: kind of short, or..?
                        RangeType = AttackType.RangeTypes.Ballistic
                    });
*/                

                return list;

               // SerializeAndDeserializeTypeList(list, GameData.Instance.AllAttackTypes, "", "attackTypes.xml", Config.DataType.BaseData);

           /* }
            else
            {
                DeserializeAndInitTypeList(GameData.Instance.AllAttackTypes, "AttackTypes");
            }*/
        }


        protected override List<EntityType> InitEntityTypes()
        {
            base.InitEntityTypes();

            List<EntityType> listOfEntityTypes = new List<EntityType>();
           
            // split into files for easier data entry. 
            TerrainFeatureLoader.Init(listOfEntityTypes);

            StructureLoader.Init(listOfEntityTypes);
            CreatureLoader.Init(listOfEntityTypes);
            TreeLoader.Init(listOfEntityTypes);
            ItemLoader.Init(listOfEntityTypes);
            InitMiscEntityTypes(listOfEntityTypes);


            return listOfEntityTypes;

            //string folderName = "Entities";

            // serialize in smaller files?
          //  SerializeAndDeserializeTypeList(listOfEntityTypes, GameData.Instance.AllEntityTypes, folderName, "entities.xml", Config.DataType.BaseData);
            

            // replace type placeholders:
          //  ReplaceEntityTypePlaceholders(listOfEntityTypes);
        }

        protected override List<StancesType> InitStancesTypes()
        {
            List<StancesType> list = new List<StancesType>();

            list.Add(new StancesType()
            {
                KeyName = "humanoid",
                
                Stances = new[]{ "lying", "standing", "kneeling", "sitting" },

               
                IdleStancesAwayFromHome = new[] { new ChanceToTakeStance() {  Stance = "standing", Chance = 1f, AddedChanceToRemainInStance = 0.5f },
                                                  new ChanceToTakeStance() {  Stance = "kneeling", Chance = 0.8f }},

                IdleStancesNearHome = new[] { new ChanceToTakeStance() {  Stance = "standing", Chance = 1f, AddedChanceToRemainInStance = 0.5f },
                                                  new ChanceToTakeStance() {  Stance = "kneeling", Chance = 0.8f }, 
                                                  new ChanceToTakeStance() {  Stance = "sitting", Chance = 0.74f }},

             

                DefaultStanceWhenWorking = "standing",
                DefaultStance = "standing",
                MovingStance = "standing",
                IncapacitatedStance = "lying",
                SleepStances = new[] { new ChanceToTakeStance() { Stance = "lying" } },
                PatrolStancesBriefWait = new[] { new ChanceToTakeStance() { Stance = "standing" } },
                PatrolStancesLongerWait = new[] { new ChanceToTakeStance() { Stance = "kneeling" } },
                SearchStancesBriefWait = new[] { new ChanceToTakeStance() { Stance = "standing" } },
                SearchStancesLongerWait = new[] { new ChanceToTakeStance() { Stance = "kneeling" } },

                StanceChangeDurations = new[] // use this to specify transition anim durations.
                {
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "lying",
                         ToStance = "sitting",
                         Duration = 3.2d
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "lying",
                         ToStance = "standing",
                         Duration = 4.56d
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "sitting",
                         ToStance = "standing",
                         Duration = 1.44
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "kneeling",
                         ToStance = "sitting",
                         Duration = 1.44
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "kneeling",
                         ToStance = "standing",
                         Duration = 1
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "standing",
                         ToStance = "lying",
                         Duration = 5.44
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "standing",
                         ToStance = "sitting",
                         Duration = 1.44
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "standing",
                         ToStance = "kneeling",
                         Duration = 1
                    }

                }


                /*
                 Stances = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Lying, LeggedLocomotor.Stance.Kneeling, LeggedLocomotor.Stance.Sitting },
                        IdleStancesAwayFromHome = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling },
                        IdleStancesNearHome = new LeggedLocomotor.Stance[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling, LeggedLocomotor.Stance.Sitting },
                        
                        IdleChanceToSitFactor = 0.74f, //MP: was 0.42, they never sat at all. 0.8f
                        IdleChanceToKneelFactor = 0.8f,
                        IdleChanceToStandFactor = 1f,
                        IdleRemainInCurrentSitOrStandStanceAddend = 0.5f,


                 */
            });

            list.Add(new StancesType()
            {
                KeyName = "dog",
                Stances = new[] { "lying", "standing" },
                IdleStancesAwayFromHome = new[] { new ChanceToTakeStance() { Stance = "standing", Chance = 0.5f, AddedChanceToRemainInStance = 0.22f },
                                               new ChanceToTakeStance() {  Stance = "lying", Chance = 0.5f, AddedChanceToRemainInStance = 0.22f }  },
//mp if the dog is just idling it makes sense that it lyes down and gets up every so often. to make it look alive.
                IdleStancesNearHome = new[] { new ChanceToTakeStance() {  Stance = "standing", Chance = 0.5f, AddedChanceToRemainInStance = 0.22f },
                                              new ChanceToTakeStance() {  Stance = "lying", Chance = 0.5f, AddedChanceToRemainInStance = 0.22f }},

                DefaultStanceWhenWorking = "standing",
                DefaultStance = "standing",
                MovingStance = "standing",
                IncapacitatedStance = "lying",
                SleepStances = new[] { new ChanceToTakeStance() { Stance = "lying" } },
                StanceChangeDurations = new StancesType.StanceChangeDuration[]
                {
                 
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "standing",
                         ToStance = "lying",
                         Duration = 1.68
                    },
                    new StancesType.StanceChangeDuration()
                    {
                         FromStance = "lying",
                         ToStance = "standing",
                         Duration = 1.8
                    },
                }

            });

            list.Add(new StancesType()
            {
                KeyName = "robot",
                Stances = new[] { "active", "inactive" },
                IdleStancesAwayFromHome = new[] { new ChanceToTakeStance() { Stance = "inactive", Chance = 1f } },
                IdleStancesNearHome = new[] { new ChanceToTakeStance() { Stance = "inactive", Chance = 1f } },

                DefaultStanceWhenWorking = "active",
                DefaultStance = "inactive",
                MovingStance = "inactive",
                IncapacitatedStance = "inactive",
                SleepStances = new[] { new ChanceToTakeStance() { Stance = "inactive" } },
                StanceChangeDurations = new StancesType.StanceChangeDuration[]
                {
                     new StancesType.StanceChangeDuration() 
                    {
                         FromStance = "inactive",
                         ToStance = "active",
                         Duration = 0.8
                    },
                     new StancesType.StanceChangeDuration() 
                    {
                         FromStance = "active",
                         ToStance = "inactive",
                         Duration = 0.8
                    },                    
                 }
            });


            return list;

        }

        protected override List<StanceType> InitStanceTypes()
        {
            List<StanceType> list = new List<StanceType>();

            /*public float Lying = 1.2f;     
        public float Sitting = 1.2f;
        public float Standing = 1.4f;*/

            list.Add(new StanceType()
            {
                KeyName = "lying",
                Number = 0,
                AnimModifier = AnimModifier.Lying,
                IsProne = true,
                IdleExertionLevel = 1.2f
            });


            list.Add(new StanceType()
            {
                KeyName = "sitting",
                Number = 1,
                AnimModifier = AnimModifier.Sitting,
                CanTurnBody = false,
                CanTurnHead = true,
                IdleExertionLevel = 1.2f
            });

            list.Add(new StanceType()
            {
                KeyName = "kneeling",
                Number = 2,
                AnimModifier = AnimModifier.Kneeling,
                CanTurnBody = false,
                CanTurnHead = true,
                IdleExertionLevel = 1.3f
            });

            list.Add(new StanceType()
            {
                KeyName = "standing",
                Number = 3,
                AnimModifier = null,
                CanStartIdleConversation = true,
                CanTurnBody = true,
                CanTurnHead = true,
                IdleExertionLevel = 1.4f
            });

            list.Add(new StanceType()
            {
                KeyName = "inactive",
                Number = 0,
                AnimModifier = AnimModifier.Inactive 
            });

            list.Add(new StanceType()
            {
                KeyName = "active",
                Number = 1,
                AnimModifier = null
            });

           


            return list;

        }

        protected override List<BodyLayerType> InitBodyLayerTypes()
        {
            List<BodyLayerType> list = new List<BodyLayerType>();


            list.Add(new BodyLayerType()
            {
                KeyName = "skinLayer",
                Name = "Skin", //human skin on the head only.
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 4f },
                     {"sharp", 4f },
                     {"blunt", 2f },                   
                     {"fire", 3f },
                     {"piercing", 5f },
                     {"smallAnimalGrapple", 22f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.2f },
                     {"sharp", 0.2f },
                     {"blunt", 0.2f },                 
                     {"fire", 0.3f },
                     {"piercing", 0.3f },
                     {"smallAnimalGrapple", 0.3f },
                     {"antiTwinkler", 0.0f },
                     }
            });


            list.Add(new BodyLayerType()
            {
                KeyName = "clothesLayer",
                Name = "Survival suit",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 4f },
                     {"sharp", 4f },
                     {"blunt", 2f },                   
                     {"fire", 3f },
                     {"piercing", 5f },
                     {"smallAnimalGrapple", 22f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.2f },
                     {"sharp", 0.2f },
                     {"blunt", 0.2f },                 
                     {"fire", 0.3f },
                     {"piercing", 0.3f },
                     {"smallAnimalGrapple", 0.3f },
                     {"antiTwinkler", 0.0f },
                     }
            });


            list.Add(new BodyLayerType()
            {
                KeyName = "thinExoSkeletonLayer",
                Name = "Shell",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 4f },
                     {"sharp", 4f },
                     {"blunt", 1f },                   
                     {"fire", 3f },
                     {"piercing", 4f },
                     {"smallAnimalGrapple", 21f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.2f },                    
                     {"fire", 0.3f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.3f },
                     }
            });

            list.Add(new BodyLayerType()
            {
                KeyName = "twinklerShell",
                Name = "Twinkler shell",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 4f },
                     {"sharp", 4f },
                     {"blunt", 1f },                   
                     {"fire", 3f },
                     {"piercing", 4f },
                     {"smallAnimalGrapple", 21f },
                     {"antiTwinkler", 0f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.2f },//nerfed to make turnips more tanky
                     {"sharp", 0.2f },//nerfed to make turnips more tanky against sharp objects
                     {"blunt", 0.2f },                    
                     {"fire", 0.3f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.0f },
                     }
            });



            list.Add(new BodyLayerType() //patrician
            {
                KeyName = "mediumExoSkeletonLayer",
                Name = "Shell",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 6f },
                     {"sharp", 6f },
                     {"blunt", 2f },                   
                     {"fire", 3f },
                     {"piercing", 5f },
                     {"smallAnimalGrapple", 22f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.2f },                    
                     {"fire", 0.3f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.3f },
                     }
            });

            list.Add(new BodyLayerType() //turnip
            {
                KeyName = "turnipArmorLayer",
                Name = "Turnip shell",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 60f },
                     {"sharp", 60f },
                     {"blunt", 50f },                   
                     {"fire", 90f }, //resistant to brushfires
                     {"piercing", 33f },// impervious to spear and ordinary bow
                     {"smallAnimalGrapple", 80f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.2f },                    
                     {"fire", 0.3f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.3f },
                     }
            });

            list.Add(new BodyLayerType()
            {
                KeyName = "hideLayer",
                Name = "Hide",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 2f },
                     {"sharp", 2f },
                     {"blunt", 2f },                   
                     {"fire", 2f },
                     {"piercing", 2f },
                     {"smallAnimalGrapple", 21f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.1f },                    
                     {"fire", 0.1f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.3f },
                     }
            });

            list.Add(new BodyLayerType()
            {
                KeyName = "smallAnimalHideLayer",
                Name = "SmallAnimalHideLayer",
                DamageReductionConstant = new Dictionary<string, float>{
                     {"bite", 2f },
                     {"sharp", 2f },
                     {"blunt", 2f },                   
                     {"fire", 2f },
                     {"piercing", 2f },
                     {"smallAnimalGrapple", 0 },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.1f },                    
                     {"fire", 0.1f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0.3f },
                     }
            });


            list.Add(new BodyLayerType()
            {
                KeyName = "robotShell", //mp: quite similar to turnip shell values.
                Name = "Robot shell",
                DamageReductionConstant = new Dictionary<string, float>{
                      {"bite", 60f },
                     {"sharp", 60f },
                     {"blunt", 50f },                   
                     {"fire", 70f },
                     {"piercing", 33f },
                     {"smallAnimalGrapple", 80f },
                     {"antiTwinkler", 600f },
                     },
                DamageReductionFactor = new Dictionary<string, float> { 
                     {"bite", 0.1f },
                     {"sharp", 0.1f },
                     {"blunt", 0.2f },                    
                     {"fire", 0.3f },
                     {"piercing", 0f },
                     {"smallAnimalGrapple", 0f },
                     {"antiTwinkler", 0f },
                     }
            });

            return list;

        }

        protected override List<BodyType> InitBodyTypes()
        {
                List<BodyType> listOfBodyTypes = new List<BodyType>();

                
                    

                    string bodyKeyName = "humanoid";
                    BodyType bodyType = new BodyType(bodyKeyName)
                        {
                            BodyPartTypes = new BodyPartType[]{
                            new BiologicalBodyPartType()
                            { 
                                Name = "Torso", BodyKeyName = bodyKeyName,  
                                ArmorLayer = "clothesLayer",
                                OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }} , 
                                ToHitProfileBack = 0.35f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]
                            {
                             new BiologicalBodyPartType()
                             { 
                                 Name = "Head", BodyKeyName = bodyKeyName,
                                 ArmorLayer = "skinLayer", //only human bodypart without clotheslayer. vulnerable to bush dragon (jan 2016)
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}, 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.1f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f, HitpointsFraction = 0.2f,
                             },
                            new BiologicalBodyPartType()
                            {   Name = "Left arm", BodyKeyName = bodyKeyName, 
                                ArmorLayer = "clothesLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.3f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Manipulation, Weight = 0.5f }},
                            },
                            new BiologicalBodyPartType() { Name = "Right arm", BodyKeyName = bodyKeyName, 
                                ArmorLayer = "clothesLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.3f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Manipulation, Weight = 0.5f } },
                            },
                            new BiologicalBodyPartType() { Name = "Left leg", BodyKeyName = bodyKeyName, 
                                ArmorLayer = "clothesLayer", 
                                ToHitProfileBack = 0.25f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0.3f, ToHitProfileRight = 0f, HitpointsFraction = 0.35f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.35f } }, // don't immobilize fully... we need more features
                            },
                            new BiologicalBodyPartType() { Name = "Right leg", BodyKeyName = bodyKeyName, 
                                ArmorLayer = "clothesLayer", 
                                ToHitProfileBack = 0.25f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.3f, HitpointsFraction = 0.35f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.35f } },
                            }
                            } 
                                
                            } 
                        }


                        };
                    listOfBodyTypes.Add(bodyType);

                    bodyKeyName = "patrician";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType()
                        { BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "mediumExoSkeletonLayer",
                                OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }},
                            ToHitProfileBack = 0.2f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.30f,                            
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "mediumExoSkeletonLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }},
                             ToHitProfileBack = 0.1f, ToHitProfileFront = 0.1f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f, HitpointsFraction = 0.10f,                                                        
                             },
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Leg 1", 
                                ArmorLayer = "mediumExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 2", 
                                ArmorLayer = "mediumExoSkeletonLayer", 
                            ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                            Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 3", 
                                ArmorLayer = "mediumExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 4", 
                                ArmorLayer = "mediumExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);
//////////---------------------------

                    bodyKeyName = "snatcher";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType()
                        { BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }},
                            ToHitProfileBack = 0.2f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.30f,                            
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "hideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }},                             
                             ToHitProfileBack = 0.1f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.10f,                                                        
                             },
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Leg 1", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 2", 
                                ArmorLayer = "hideLayer", 
                            ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                            Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 3", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 4", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);


////////---------------------------------------------------

                    bodyKeyName = "dog";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType()
                        { BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            ,
                            ToHitProfileBack = 0.2f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.30f,                            
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "hideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}
                             ,
                             ToHitProfileBack = 0.1f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.10f,                                                        
                             },
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Leg 1", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 2", 
                                ArmorLayer = "hideLayer", 
                            ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                            Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 3", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 4", 
                                ArmorLayer = "hideLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);


                    ////////---------------------------------------------------


                    bodyKeyName = "turnip";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType()
                        { BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "turnipArmorLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            ,
                            ToHitProfileBack = 0.2f, ToHitProfileFront = 0.2f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.30f,                            
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "turnipArmorLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}                             
                             ,
                             ToHitProfileBack = 0.1f, ToHitProfileFront = 0.1f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f, HitpointsFraction = 0.10f,                                                        
                             },
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Leg 1", 
                                ArmorLayer = "turnipArmorLayer", //if we use lighter armor on legs (mediumExoSkeletonLayer), humans would just target those. ..not good..
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 2", 
                                ArmorLayer = "turnipArmorLayer", 
                            ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                            Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 3", 
                                ArmorLayer = "turnipArmorLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 4", 
                                ArmorLayer = "turnipArmorLayer", 
                                ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.25f,
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);

                    //********************
                    bodyKeyName = "twinkler";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "twinklerShell",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            , 
                            ToHitProfileBack = 0.4f, ToHitProfileFront = 0.4f, ToHitProfileLeft = 0.4f, ToHitProfileRight = 0.4f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "twinklerShell",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}, 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.1f, ToHitProfileLeft = 0.1f, ToHitProfileRight = 0.1f, HitpointsFraction = 0.1f,},
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Leg 1", ArmorLayer = "twinklerShell", ToHitProfileBack = 0.12f, ToHitProfileFront = 0.12f, ToHitProfileLeft = 0.12f, ToHitProfileRight = 0.12f, HitpointsFraction = 0.25f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Leg 2", ArmorLayer = "twinklerShell", ToHitProfileBack = 0.12f, ToHitProfileFront = 0.12f, ToHitProfileLeft = 0.12f, ToHitProfileRight = 0.12f, HitpointsFraction = 0.25f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() {BodyKeyName = bodyKeyName, Name = "Leg 3", ArmorLayer = "twinklerShell", ToHitProfileBack = 0.12f, ToHitProfileFront = 0.12f, ToHitProfileLeft = 0.12f, ToHitProfileRight = 0.12f, HitpointsFraction = 0.25f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() {BodyKeyName = bodyKeyName, Name = "Leg 4", ArmorLayer = "twinklerShell", ToHitProfileBack = 0.12f, ToHitProfileFront = 0.12f, ToHitProfileLeft = 0.12f, ToHitProfileRight = 0.12f, HitpointsFraction = 0.25f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);
                    //********************************
            
                    //********************
                    bodyKeyName = "forestguardian";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType()
                        { 
                            BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer", //??
                            ToHitProfileBack = 0.35f, 
                            ToHitProfileFront = 0.35f, 
                            ToHitProfileLeft = 0.35f, 
                            ToHitProfileRight = 0.35f,  
                            HitpointsFraction = 0.5f
                            , 
                            BodyPartTypes = new BodyPartType[]{
                            new BiologicalBodyPartType()
                            { 
                                BodyKeyName = bodyKeyName, Name = "Head",
                                ArmorLayer = "hideLayer", //??
                                ToHitProfileBack = 0.35f, 
                                ToHitProfileFront = 0.35f, 
                                ToHitProfileLeft = 0.35f, 
                                ToHitProfileRight = 0.35f,  
                                HitpointsFraction = 0.5f
                            },
                            
                            new BiologicalBodyPartType()
                            { 
                                BodyKeyName = bodyKeyName, Name = "Leg 1", 
                                ArmorLayer = "thinExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, 
                                ToHitProfileFront = 0.15f, 
                                ToHitProfileLeft = 0.15f, 
                                ToHitProfileRight = 0.15f, 
                                HitpointsFraction = 0.25f, 
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}
                            },
                            
                            new BiologicalBodyPartType() 
                            { 
                                BodyKeyName = bodyKeyName, Name = "Leg 2", ArmorLayer = "thinExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, 
                                ToHitProfileFront = 0.15f, 
                                ToHitProfileLeft = 0.15f, 
                                ToHitProfileRight = 0.15f, 
                                HitpointsFraction = 0.25f, 
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }
                            },
                           
                            new BiologicalBodyPartType() {
                                BodyKeyName = bodyKeyName, Name = "Leg 3", ArmorLayer = "thinExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, 
                                ToHitProfileFront = 0.15f, 
                                ToHitProfileLeft = 0.15f, 
                                ToHitProfileRight = 0.15f, 
                                HitpointsFraction = 0.25f, 
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } 
                            },
                            
                            new BiologicalBodyPartType() 
                            {
                                BodyKeyName = bodyKeyName, Name = "Leg 4", ArmorLayer = "thinExoSkeletonLayer", 
                                ToHitProfileBack = 0.15f, 
                                ToHitProfileFront = 0.15f, 
                                ToHitProfileLeft = 0.15f, 
                                ToHitProfileRight = 0.15f, 
                                HitpointsFraction = 0.25f, 
                                Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}
                            }   
                        } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);
                    //********************************
                    bodyKeyName = "bushdragon";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            , 
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "hideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}   
                                 , 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Left leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Right leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Front leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f } }}
                           
                            } 
                
                    }


                }
                    };
                    listOfBodyTypes.Add(bodyType);

                    #region Demon tree

                    bodyKeyName = "demonTree";
                    listOfBodyTypes.Add(new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            , 
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "hideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}   
                                 , 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Left leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Right leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f } }},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Front leg", ArmorLayer = "hideLayer", ToHitProfileBack = 0f, ToHitProfileFront = 0.25f, ToHitProfileLeft = 0.15f, ToHitProfileRight = 0.15f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.5f } }}
                           
                            }                 
                        }
                        }
                    });

                    #endregion

                    #region sentry

                    bodyKeyName = "sentry";
                    string robotArmor = "robotShell";
                    listOfBodyTypes.Add(new BodyType(bodyKeyName)
                    {
                        Hitpoints = 40, // set fixed hitpoints that don't depend on bulk.
                        BodyPartTypes = new BodyPartType[]{
                        new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Tripod", ArmorLayer = robotArmor,
                            ToHitProfileBack = 0.5f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Gun", ArmorLayer = robotArmor,
                                 ToHitProfileBack = 0.5f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f, HitpointsFraction = 0.5f,},
                            } 
                        }
                    }
                    });

                    #endregion

                    #region TEST robot

                    bodyKeyName = "weedingRobot";
                    robotArmor = "robotShell";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        Hitpoints = 30, // set fixed hitpoints that don't depend on bulk.
                        BodyPartTypes = new BodyPartType[]{
                        new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", ArmorLayer = robotArmor,
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", ArmorLayer = robotArmor,
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Left leg", ArmorLayer = robotArmor, ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f }}},
                            new MachineBodyPartType() { BodyKeyName = bodyKeyName, Name = "Right leg", ArmorLayer = robotArmor, ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f } }}
                            }                 

                        }}
                    };
                    listOfBodyTypes.Add(bodyType);

                    #endregion


                    #region New robot: ANT

                    // TODO: add more robot bodies...
                    bodyKeyName = "robotBody";
                    robotArmor = "robotShell";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        Hitpoints = 30, // set fixed hitpoints that don't depend on bulk.
                        Bulk = 1f, // NEW - for trading robots
                        BodyPartTypes = new BodyPartType[]{
                        new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", ArmorLayer = robotArmor,
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", ArmorLayer = robotArmor,
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Left leg", ArmorLayer = robotArmor, ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f }}},
                            new MachineBodyPartType() { BodyKeyName = bodyKeyName, Name = "Right leg", ArmorLayer = robotArmor, ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f } }}
                            }                 

                        }}
                    };
                    listOfBodyTypes.Add(bodyType);

                    #endregion


                    //********************************
                    bodyKeyName = "spikePlant";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "hideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}, 
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "hideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}, 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            } 
                        }
                        }
                    };
                    listOfBodyTypes.Add(bodyType);
                    //********************************

                    bodyKeyName = "worm";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "mediumExoSkeletonLayer", //for testing..was hideLayer. Worm has pancer plates on its head.
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}, 
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "mediumExoSkeletonLayer", //for testing..was hideLayer. Worm has pancer plates on its head.
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}, 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            } 
                    }

                }
                    };


                    listOfBodyTypes.Add(bodyType);
                    //********************************

                    bodyKeyName = "bird";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head"},
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Wing 1", Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Wing 2", Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }},
                            new BiologicalBodyPartType() {BodyKeyName = bodyKeyName, Name = "Wing 3", Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } } },
                            new BiologicalBodyPartType() {BodyKeyName = bodyKeyName, Name = "Wing 4", Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.4f } }}} } 
                    }


                    };
                    listOfBodyTypes.Add(bodyType);
                    //********************************

                    bodyKeyName = "thunderchicken";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                        new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Torso", 
                            ArmorLayer = "smallAnimalHideLayer",
                            OrganTypes = new OrganType[]{ new OrganType(){ Name = "Heart", IsVital = true }}
                            , 
                            ToHitProfileBack = 0.6f, ToHitProfileFront = 0.35f, ToHitProfileLeft = 0.5f, ToHitProfileRight = 0.5f,  HitpointsFraction = 0.5f,
                            BodyPartTypes = new BodyPartType[]{
                             new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Head", 
                                 ArmorLayer = "smallAnimalHideLayer",
                                 OrganTypes = new OrganType[]{ new OrganType(){ Name = "Brain", IsVital = true }}   
                                 , 
                                 ToHitProfileBack = 0.1f, ToHitProfileFront = 0.5f, ToHitProfileLeft = 0.2f, ToHitProfileRight = 0.2f, HitpointsFraction = 0.1f,},
                            new BiologicalBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Left leg", ArmorLayer = "smallAnimalHideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0.25f, ToHitProfileRight = 0f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f }}},
                            new BiologicalBodyPartType() { BodyKeyName = bodyKeyName, Name = "Right leg", ArmorLayer = "smallAnimalHideLayer", ToHitProfileBack = 0.15f, ToHitProfileFront = 0.15f, ToHitProfileLeft = 0f, ToHitProfileRight = 0.25f, HitpointsFraction = 0.2f, Functions = new BodyPartFunction[]{ new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Locomotion, Weight = 0.7f } }}
                            } 
                

                }}
                    };
                    listOfBodyTypes.Add(bodyType);
                    //********************************

                    bodyKeyName = "house";
                    bodyType = new BodyType(bodyKeyName)
                    {
                        BodyPartTypes = new BodyPartType[]{
                    
                    new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Foundation", 
                        /* MadeOf = new Dictionary<EntityType,int>(){ {CreatePlaceholder("item:cement"), 10}, {CreatePlaceholder("item:metalparts"), 10}},*/
                         Functions = new BodyPartFunction[]{new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Structure, Weight = 1f}
                         }                           
                         , 
                       
                        BodyPartTypes = new BodyPartType[]{
                         
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Walls", 
                               /* MadeOf = new Dictionary<EntityType,int>(){ {CreatePlaceholder("item:cement"), 12}, {CreatePlaceholder("item:metalparts"), 8}}
                            ,*/
                             Functions = new BodyPartFunction[]{new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Structure, Weight = 1f}}, 
     
                             BodyPartTypes = new BodyPartType[]{
                                
                                 new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Roof", /*MadeOf = new Dictionary<EntityType,int>(){ {CreatePlaceholder("item:metalparts"), 8}}
                                 ,*/
                                    // MachineFunctions = new MachineBodyPartFunction[]{new MachineBodyPartFunction(){ Function = MachineBodyPartFunction.MachineFunction.Structure, Weight = 0.3f}}
                                     Functions = new BodyPartFunction[]{new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.Structure, Weight = 0.3f},
                                                                        new BodyPartFunction(){ Function = BodyPartFunction.FunctionType.UserComfort, Weight = 1f}}
                            
                            }
                            }
                         },
                         new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Plumbing",
                       /*  MadeOf = new Dictionary<EntityType,int>(){ { CreatePlaceholder("item:metalparts") , 5}} 
                         ,*/                     
                         }
                    }
                }
            }
                    };
                    listOfBodyTypes.Add(bodyType);


                    bodyKeyName = "mulevehicle";
                    bodyType = new BodyType(bodyKeyName)
                        {
                            Hitpoints = 200,
                            BodyPartTypes = new BodyPartType[]{
                        new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Body", 
                             /*MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:muleVehicleBody"), 1}} 
                            , */
                            BodyPartTypes = new BodyPartType[]{
                             new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front left suspension", 
                                /* MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:suspension"), 1}}
                                ,*/  BodyPartTypes = new BodyPartType[]{
                                    new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front left wheel",
                                       /* MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:wheel"), 1}},*/
                                        
                                        BodyPartTypes = new BodyPartType[]{ 
                                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front left motor",
                                               /* MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:wheelMotor"), 1}}*/},
                                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front left tyre", 
                                              /*  MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:tyre"), 1}}*/}
                                        }
                                    }
                                }
                             },
                             new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front right suspension",
                               /*  MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:suspension"), 1}}
                                ,*/ BodyPartTypes = new BodyPartType[]{
                                    new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front right wheel", 
                                       /* MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:wheel"), 1}},*/
                                       
                                        BodyPartTypes = new BodyPartType[]{
                                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front right motor", 
                                               
                                              /*  MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:wheelMotor") , 1}}*/},
                                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Front right tyre",
                                               /* MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:tyre"), 1}}*/}
                                        }
                                    }
                                }  

                             
                            },
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Control panel",
                                 /*MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:controlPanel"), 1}}*/
                            }
                            ,
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Driver's seat", 
                                 /*MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:vehicleSeat"), 1}}*/
                            }
                            ,
                            new MachineBodyPartType(){ BodyKeyName = bodyKeyName, Name = "Passenger seat", 
                                 /*MadeOf = new Dictionary<EntityType,int>(){{ CreatePlaceholder("item:vehicleSeat"), 1}}*/
                            }
                        }
                        
                    }
                }
                        };

                    listOfBodyTypes.Add(bodyType);

                    return listOfBodyTypes;

               
        }


        #region Events - place any event stuff that is general enough to include across all scenarios here (scenarios kan delete events with a delete record):


       /* protected override List<AgentActionHook> InitAgentActionHooks()
        {
            return EventHooksLoader.InitAgentActionHooks();
        }


        protected override List<ProcessTypeActionHook> InitProcessTypeEventHooks()
        {
            return EventHooksLoader.InitProcessTypeHooks();
        }

        protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
        }

        protected override List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectResourceTypeHooks();
        }*/


      
        /*
        protected override List<ActionSets> InitActionSets()
        {
            return ActionSetsLoader.Init();

        }
        */

        /*
        protected override List<EventActionType> InitEventActionTypes()
        {
            return EventActionsLoader.Init();
        }
        */


        #endregion



       

        protected override List<SkillCategory> InitSkillCategories()
        {

            List<SkillCategory> listOfSkillCategories = new List<SkillCategory>();

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "basic",
                Name = "Basic"
            });

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "combat",
                Name = "Combat"
            });

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "construction",
                Name = "Construction"
            });

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "production",
                Name = "Production"
            });

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "science",
                Name = "Science"
            });

            listOfSkillCategories.Add(new SkillCategory()
            {
                KeyName = "other",
                Name = "Other"
            });

            return listOfSkillCategories;


        }

        protected override List<ProfessionType> InitProfessionTypes()
        {
            List<ProfessionType> list = new List<ProfessionType>();
            //mp i renamed it to 'Field' because profession sounds too formal.
//mp  the fields are linked to the skills  by including the keyname next to each skill in the skill list further below..
            list.Add(new ProfessionType() { KeyName = "securityPerson", Name = "Security", Icon = "lcd_icon_security" });             //
            list.Add(new ProfessionType() { KeyName = "hunter", Name = "Hunting", Icon = "lcd_icon_skill_rifle" });              //
            list.Add(new ProfessionType() { KeyName = "metalWorker", Name = "Blacksmithing", Icon = "lcd_icon_skill_anvil" });      //
            list.Add(new ProfessionType() { KeyName = "farmer", Name = "Farming", Icon = "lcd_icon_skill_pitchfork" });          //

            list.Add(new ProfessionType() { KeyName = "physician", Name = "Medicine", Icon = "lcd_icon_skill_medicineSymbol" });//
            list.Add(new ProfessionType() { KeyName = "cook", Name = "Cooking", Icon = "lcd_icon_skill_cookingpot" });             //
            list.Add(new ProfessionType() { KeyName = "menialWorker", Name = "Menial labor", Icon = "lcd_icon_skill_wheelbarrow" }); //
            list.Add(new ProfessionType() { KeyName = "builder", Name = "Construction", Icon = "lcd_icon_skill_bricks" });//
            list.Add(new ProfessionType() { KeyName = "electricEngineer", Name = "Electrical engineering", Icon = "lcd_icon_skill_spark" });   //      
            list.Add(new ProfessionType() { KeyName = "mechanicalEngineer", Name = "Mechanical engineering", Icon = "lcd_icon_skill_caliper" });
            list.Add(new ProfessionType() { KeyName = "chemist", Name = "Chemistry", Icon = "lcd_icon_skill_conicalFlask" });
            list.Add(new ProfessionType() { KeyName = "scientist", Name = "Science", Icon = "lcd_icon_skill_conicalFlask" }); //  
            list.Add(new ProfessionType() { KeyName = "bushcraft", Name = "Bushcraft", Icon = "lcd_icon_skill_axe" });          //

//mechanical, chemical and electrical engineering: https://en.wikipedia.org/wiki/List_of_engineering_branches

            return list;
        }

        protected override List<SkillType> InitSkillTypes()
        {


            List<SkillType> listOfSkillTypes = new List<SkillType>();

            SkillType skill = new SkillType("hunting", "Hunting") { SortOrder = 20, Description = "Effectiveness at detecting and stalking prey. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["other"], ProfessionKey = "hunter" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("fishing", "Fishing") { SortOrder = 30, Description = "Effectiveness at catching fish. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["other"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("foraging", "Foraging") { SortOrder = 40, Description = "Effectiveness at finding food and useful materials in the wild. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["other"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("cooking", "Cooking") { SortOrder = 60, Description = "Productivity when making food and preparing ingredients. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "cook" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("menial", "Menial") { SortOrder = 70, Description = "Productivity when doing physical tasks that require no special skill. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["other"], ProfessionKey = "menialWorker" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("weaving", "Weaving") { SortOrder = 75, Description = "Productivity when making textile. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("carpentry", "Carpentry") { SortOrder = 78, Description = "Productivity when making items from wood. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("farming", "Farming") { SortOrder = 80, Description = "Ability to grow and harvest crops from fields. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "farmer" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("construction", "Construction") { SortOrder = 90, Description = "Ability to construct houses and buildings. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["construction"], ProfessionKey = "builder" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("smithing", "Smithing") { SortOrder = 100, Description = "Productivity when shaping and joining metal using a forge and hammer. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "metalWorker" };
            listOfSkillTypes.Add(skill);
            /*     skill = new SkillType("engineering", "Engineering") { SortOrder = 105, Description = "Ability to use complex processes and items in the areas of mechanical and electrical engineering and metallurgy. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "engineer" };
                 listOfSkillTypes.Add(skill); */
            //todo: delete "engineering"
            skill = new SkillType("mechanics", "Mechanics") { SortOrder = 110, Description = "Ability to make and operate machines and mechanical systems.\nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "mechanicalEngineer" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("electronics", "Electronics") { SortOrder = 115, Description = "Knowledge about electricity, electronics and electromagnetism.\nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "electricEngineer" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("chemistry", "Chemistry") { SortOrder = 120, Description = "Knowledge about substances and chemicals and how to produce them.\nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "chemist" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("biology", "Biology") { SortOrder = 130, Description = "Knowledge about plants and animals. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["science"], ProfessionKey = "scientist" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("medicine", "Medicine") { SortOrder = 140, Description = "Ability to treat wounds and diseases. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["science"], ProfessionKey = "physician" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("psychology", "Psychology") { SortOrder = 145, Description = "Knowledge about the human mental processes. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["science"], ProfessionKey = "scientist" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("shooting", "Shooting") { SortOrder = 150, GiveExpertSkillBonus = true, Description = "Ability to hit a target with a firearm or energy based weapon. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["combat"], ProfessionKey = "securityPerson" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("archery", "Archery") { SortOrder = 160, GiveExpertSkillBonus = true, Description = "Ability to hit a target with a bow and arrow. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["combat"], ProfessionKey = "securityPerson" };
            listOfSkillTypes.Add(skill);

            skill = new SkillType("armedMelee", "Armed melee") { SortOrder = 170, Description = "Ability to fight in close combat using a non-projectile weapon. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["combat"], ProfessionKey = "securityPerson" };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("butchering", "Butchering") { SortOrder = 50, Description = "Productivity when retrieving meat, organs and hide from a carcass. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("bushcraft", "Bushcraft") { SortOrder = 10, Description = "Making items and structures under primitive conditions using simple tools and materials found in the wild. The skill determines productivity. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "bushcraft" }; //
            listOfSkillTypes.Add(skill);
            skill = new SkillType("sneaking", "Sneaking") { SortOrder = 190, Description = "Ability to avoid detection while moving. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["other"] };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("unarmedFighting", "Unarmed fighting") { SortOrder = 180, Description = "Ability to fight barehanded. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["combat"], ProfessionKey = "securityPerson" };
            listOfSkillTypes.Add(skill);

            skill = new SkillType("grasping", "Grasping") { SortOrder = 200, Description = "Ability to grab and hold objects. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["basic"], SuppressDisplayForPersons = true };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("fruitPicking", "Fruit picking") { SortOrder = 210, Description = "Ability to collect small objects. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["basic"], SuppressDisplayForPersons = true };
            listOfSkillTypes.Add(skill);
            skill = new SkillType("weeding", "Weeding") { SortOrder = 220, Description = "Ability to remove unwanted weeds from fields. \nTo begin a task, a skill of 0.1 is the minimum requirement.", Category = GameData.Instance.AllSkillCategories["production"], ProfessionKey = "farmer", SuppressDisplayForPersons = true };
            listOfSkillTypes.Add(skill);
            /////////////////////////////not in use:
            /*   skill = new SkillType("spearThrowing", "Spear throwing") { Category = GameData.Instance.AllSkillCategories["combat"] }; //attack type is not in use
               listOfSkillTypes.Add(skill);
               skill = new SkillType("geology", "Geology") { Category = GameData.Instance.AllSkillCategories["science"] }; //not in use
               listOfSkillTypes.Add(skill);                    
               skill = new SkillType("mining", "Mining") { Category = GameData.Instance.AllSkillCategories["production"] }; //not in use
               listOfSkillTypes.Add(skill); 
               skill = new SkillType("robotics", "Robotics") { Category = GameData.Instance.AllSkillCategories["production"] }; //not in use
               listOfSkillTypes.Add(skill);
               skill = new SkillType("hydroponics", "Hydroponics") { Category = GameData.Instance.AllSkillCategories["production"] }; //not in use
               listOfSkillTypes.Add(skill);
               skill = new SkillType("animalTraining", "Animal training") { Category = GameData.Instance.AllSkillCategories["other"] }; //not in use
               listOfSkillTypes.Add(skill);
               skill = new SkillType("riding", "Horseback riding") { Category = GameData.Instance.AllSkillCategories["other"] }; //not in use
               listOfSkillTypes.Add(skill); 
               skill = new SkillType("surgery", "Surgery") { Category = GameData.Instance.AllSkillCategories["science"] }; //not in use
               listOfSkillTypes.Add(skill);
               skill = new SkillType("microbiology", "Microbiology") { Category = GameData.Instance.AllSkillCategories["science"] };//not in use
               listOfSkillTypes.Add(skill); 
               skill = new SkillType("meatDrying", "Meat drying") { Category = GameData.Instance.AllSkillCategories["production"] };//not in use
               listOfSkillTypes.Add(skill);
       */


            //  
            return listOfSkillTypes;



        }





        protected override List<ResourceCategory> InitResourceCategories()
        {
           
            
                // *********** Create Categories *****************
                List<ResourceCategory> listOfCategories = new List<ResourceCategory>();

                //the color which is used in the mini-map/resource overlay menu checkboxes. //See OverlaySettings.cs for the rest - search for: Color persons

                ResourceCategory category = new ResourceCategory() { KeyName = "food", Name = "FOOD", Color = Common.ColorFromHex("#0de700") }; //green
                    listOfCategories.Add(category);
                   // category.OverlayOption = ResourceCategory.OverlayOptions.DefaultInMenu;

                    category = new ResourceCategory() { KeyName = "rawMaterials", Name = "RAW MATERIALS", Color = Common.ColorFromHex("#ffde00") }; //yellow
                    listOfCategories.Add(category);

                    category = new ResourceCategory() { KeyName = "carcasses", Name = "CARCASSES" };
                   // category.OverlayOption = ResourceCategory.OverlayOptions.NotInMenu;
                    listOfCategories.Add(category);

                    return listOfCategories;

        }


        protected override List<DefaultStorageSettings> InitDefaultStorageSettings() //this is the default settings for stockpiles in newly built structures
        {
            List<DefaultStorageSettings> list = new List<DefaultStorageSettings>();

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "cooledStorage",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   // basically, it's a fridge
                    { "preparedFood", true },
                    { "ingredients", true },
                    { "rawMaterials", false },
                    { "weapons", false }, // not if you're Cobra
                    { "bodies", false }, // too unhygienic?
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>(){ { "item:thunderChickenCarcass", -1 }}, // added as an example of overriding a category with an entity type
                MayStockpileItemTag = new SerializableDictionary<string, int>(){ { "cookedMeat", -1 } }// added as an example of using an item tag, in this case a food tag

            });
            list.Add(new DefaultStorageSettings()
            {
                KeyName = "workplaceStorage",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", true },
                    { "weapons", false },
                    { "bodies", false },
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", true }
                },
            });
            list.Add(new DefaultStorageSettings()
            {
                KeyName = "homeStorage",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   //default is true, so put in the ones we don't want                 
                    { "rawMaterials", false }, // ??? too broad? use item tags or item types if needed
                    { "waste", false }, //
                    { "bodies", false }                   
                }
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "firewoodstack",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() { { "item:wetFirewood", -1 } }, //note, does not store dry firewood, that's not its purpose.
            });


            list.Add(new DefaultStorageSettings()
            {
                KeyName = "farmToolshed",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", true }
                },
                //excluded the items for the forge:
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                { "item:bellows", -1 }, 
                { "item:blowpipe", -1 }, 
                { "item:blacksmithsToolbox", -1 }, 
                { "item:metalWorkersToolbox", -1 }, 
                { "item:hammer", -1 }, 
                { "item:stoneHammer", -1 }, 
                { "item:file", -1 }, 
                { "item:tongs", -1 }, 
                { "item:handDrill", -1 }, 
                { "item:hacksaw", -1}, 
                { "item:tinnerSnips", -1 }, 
                { "item:advancedSnips", -1 }, 
                { "item:smoothSandstone", -1 },              
                },
            });


            list.Add(new DefaultStorageSettings()
            {
                KeyName = "workbenchStorage", //tools for the workbench. woodworking etc.
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                { "item:advancedMachete", -1 }, 
                { "item:steelMachete", -1 }, 
                { "item:improvisedHandAxe", -1 }, 
                { "item:steelHandAxe", -1 }, 
                { "item:advancedKnife", -1 }, 
                { "item:improvisedKnife", -1 }, 
                { "item:flintKnife", -1 }, 
                { "item:steelKnife", -1 }, 
                { "item:shadeleafResin", -1 }, 
                { "item:advancedString", -1 },   
                { "item:metalWire", -1 }, 
                { "item:rawhideString", -1 }, 
                { "item:exaGlue", -1 },               
                
                },
            });


           

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "uncooledKitchenStorage", //tools for the non-refrigerated kitchen.
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
 
                { "item:advancedKnife", -1 }, 
                { "item:improvisedKnife", -1 }, 
                { "item:flintKnife", -1 }, 
                { "item:steelKnife", -1 }, 

                { "item:vinegar", -1 }, //not sure about this 

                { "item:advancedCookingPot", -1 }, 
                { "item:improvisedCookingPot", -1 },
                { "item:goldPot", -1 },
                { "item:woodenCookingPot", -1 },   
       
                { "item:clayPotUnglazed", -1 },
                { "item:clayJar", -1 },   
                
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "forgeStorage", //tools used with the forge (Also some materials - primarily so we can spawn these items inside storage)
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                    
                { "item:bellows", -1 }, 
                { "item:blowpipe", -1 }, 
                { "item:blacksmithsToolbox", -1 }, 
                { "item:metalWorkersToolbox", -1 }, 
                { "item:hammer", -1 }, 
                { "item:stoneHammer", -1 }, 
                { "item:file", -1 }, 
                { "item:tongs", -1 }, 
                { "item:handDrill", -1 }, 
                { "item:hacksaw", -1 }, 
                { "item:tinnerSnips", -1 }, 
                { "item:advancedSnips", -1 }, 
                { "item:smoothSandstone", -1 }, 

//materials - primarily so we can spawn these items inside storage on new scenarios
                { "item:roughBloomIron", -1 },
                { "item:wroughtIron", -1 }, 
                { "item:blisterSteel", -1 }, 
                { "item:charcoal", -1 }, 


                
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "noStorage", 
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                }
            });


            #region workshop upgrades
            list.Add(new DefaultStorageSettings()
            {
                KeyName = "polymerWorkshopStorage", // "workshopBuildingStorage", //
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {

                { "item:marshcotSap", -1 }, 
                { "item:sulfurPowder", -1 },   
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "carpentersWorkshopStorage", //tools AND materials for the carpenter
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                  //materials:              
                { "item:spoakBranchesTrimmed", -1 }, 
                //tools used:
                { "item:carpentersToolbox", -1 }, 
                
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "textileWorkshopStorage", 
                Comments = "tools AND materials for the weaver",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                    //materials:              
                { "item:cotton", -1 }              
                
                },
            });

            list.Add(new DefaultStorageSettings()
            {               
                KeyName = "metalWorkshopStorage", 
                Comments = "tools AND materials for the metal workshop",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                    //materials for the metal shoplathe:
                { "item:blisterSteel", -1 }, 
                { "item:gunBarrelUnbored", -1 }, 
                { "item:gunBarrelSmoothLong", -1 }, 
                { "item:gunBarrelSmoothShort", -1 }, 
                { "item:gunBarrelRifled", -1 }, 
                //tools used with the metal shop lathe:
                { "item:metalWorkersToolbox", -1 }, 
                
                },
            });

            #endregion

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "metalShopStorage", //tools AND materials for the metal lathe. (it's a building with space for materials)
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() {
                    //materials for the metal shoplathe:
                { "item:blisterSteel", -1 }, 
                { "item:gunBarrelUnbored", -1 }, 
                { "item:gunBarrelSmoothLong", -1 }, 
                { "item:gunBarrelSmoothShort", -1 }, 
                { "item:gunBarrelRifled", -1 }, 
                //tools used with the metal shop lathe:
                { "item:metalWorkersToolbox", -1 }, 
                
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "simplePortLocalStorage", //mp default is nothing. this is an area of the port which holds items NOT for sale.
                Name = "",
                MayStockpileEntityType = new SerializableDictionary<string,int>() // LArs: added marshcot storage here, they have nowhere else to put it.
                {
                    { "item:marshcotSap", -1 },
                },
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false },  
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },

            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "compostBinSettings",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
                MayStockpileEntityType = new SerializableDictionary<string, int>() 
                { 
                    { "item:infestedLeavesRemains", -1 },
                    { "item:rottenVegetables", -1 },
                    { "item:rottenStaple", -1 },
                    // meat products should be removed when/if meat waste gets filtered out of compost fertilizer
                    { "item:twinklerGuts", -1 },
                    { "item:rottenMeat", -1 },
                    { "item:spoiledMeal", -1 },
                    //other
                    { "item:degradedEnzyme", -1 },
                    { "item:degradedChemical", -1 },
                    { "item:organicMatter", -1 },
                },
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "stockpile",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   //default is true, so put in the ones we don't want                 
                    { "rawMaterials", false }, // ??? too broad? use item tags or item types if needed
                    { "waste", false }, // 
                    { "bodies", false }                   
                }
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "darkFoodStorage", // for storing space potatoes
                Name = "",
                MayStockpileEntityType = new SerializableDictionary<string, int>()
                {
                    { "item:salt", -1 } // is raw material
                },
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   //default is true, so put in the ones we don't want  

                    { "rawMaterials", false }, // ??? too broad? use item tags or item types if needed
                    { "waste", false }, //
                    { "bodies", false },
                    { "tools", false }, 
                    { "weapons", false }, 
                    { "ammunition", false }, 
                }
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "kitchenStorage", // refrigerator for food and space for kitchen utensils
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   //default is true, so put in the ones we don't want  

                    { "rawMaterials", false }, // ??? too broad? use item tags or item types if needed
                    { "waste", false }, //
                    { "bodies", false },
                    { "tools", false }, 
                    { "weapons", false }, 
                    { "ammunition", false }, 
                },
                MayStockpileItemTag = new SerializableDictionary<string, int>()
                {
                    { "cookingPot", -1 },
                    { "knife", -1 }
                }
              /*  MayStockpileEntityType = new SerializableDictionary<string, bool>()
                {
                    { "item:advancedKnife", true },
                    { "item:steelKnife", true},
                    { "item:improvisedKnife", true}
                }*/
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "fishTrapCage",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   //default is true, so put in the ones we don't want  
                    { "preparedFood", false },  
                    { "ingredients", false }, 
                    { "bodies", false },
                    { "rawMaterials", false },
                    { "tools", false },
                    { "weapons", false },
                    { "ammunition", false },
                    { "waste", false }
                }/*,
                MayStockpileEntityType = new SerializableDictionary<string, bool>()
                {
                    { "item:impEel", true }
                }*/
            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "trapBaitStorage",
                Name = "",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                }, //mp default should be nothing apart from the bait options in the special action that the player is presented with, so as not to confuse him.
                MayStockpileEntityType = new SerializableDictionary<string, int>() { { "item:binalRatChunk", 1 }, { "item:blackpulp", 1 }, { "item:glassyCreeperPods", 1 } }, 
                //

            });

            list.Add(new DefaultStorageSettings()
            {
                KeyName = "hideRackStorage",
                Name = "Hide Rack Storage",
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                {   
                    { "preparedFood", false },
                    { "ingredients", false },
                    { "rawMaterials", false },
                    { "weapons", false }, 
                    { "bodies", false }, 
                    { "waste", false },
                    { "ammunition", false },
                    { "tools", false }
                },
          //      MayStockpileEntityType = new SerializableDictionary<string, bool>() { { "item:thunderChickenCleanGreenHide", true } } //mp i guess this is no longer needed with the "RequiresWork" code change sep 2015
            });

            return list;
        }


        protected override List<EntityCategory> InitEntityCategories()
        {

            /*
              fra øverst til nederst:
              [16:13:44] Morten Pedersen: Prepared meals- ingredients-carcasses-materials/components-tools-weapons-ammunition-waste*/
            // *********** Create Categories *****************
            List<EntityCategory> list = new List<EntityCategory>();


            EntityCategory category = new EntityCategory() { KeyName = "preparedFood", Name = "PREPARED FOOD", CategoryColor = EntityCategory.CategoryColors.Green, SpriteName = "preparedFood", SortOrder = 5 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "ingredients", Name = "INGREDIENTS", CategoryColor = EntityCategory.CategoryColors.Green, SpriteName = "ingredients", SortOrder = 10 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "bodies", Name = "CARCASSES", CategoryColor = EntityCategory.CategoryColors.Green, SpriteName = "carcasses", SortOrder = 15 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "rawMaterials", Name = "MATERIALS/COMPONENTS", SpriteName = "metal", SortOrder = 20 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "tools", Name = "TOOLS", SpriteName = "equipment", SortOrder = 25 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "weapons", Name = "WEAPONS", CategoryColor = EntityCategory.CategoryColors.Blue, SpriteName = "guns", SortOrder = 30 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "ammunition", Name = "AMMUNITION", CategoryColor = EntityCategory.CategoryColors.Blue, SpriteName = "guns", SortOrder = 35 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "equipment", Name = "EQUIPMENT", SpriteName = "equipment", SortOrder = 40 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "waste", Name = "WASTE", SpriteName = "carcasses", IsWaste = true, SortOrder = 45 };
            list.Add(category);
            category = new EntityCategory() { KeyName = "upgrades", Name = "UPGRADES", SpriteName = "equipment", SortOrder = 55 };
            list.Add(category);
            list.Add(new EntityCategory() { KeyName = "shelter", Name = "SHELTER", CategoryColor = EntityCategory.CategoryColors.Red, SortOrder = 0, DisplayStructureIcon = true });
            list.Add(new EntityCategory() { KeyName = "production", Name = "STORAGE/PRODUCTION", SortOrder = 1, DisplayStructureIcon = true });
            list.Add(new EntityCategory() { KeyName = "defense", Name = "DEFENSE", SortOrder = 2, DisplayStructureIcon = true });
            list.Add(new EntityCategory() { KeyName = "miscellaneous", Name = "MISCELLANEOUS", SortOrder = 3, DisplayStructureIcon = true });
            list.Add(new EntityCategory() { KeyName = "animals", Name = "ANIMALS", SortOrder = 60 });
            list.Add(new EntityCategory() { KeyName = "robots", Name = "ROBOTS", SortOrder = 80 });



            return list;
        }


        protected override List<StorageCondition> InitStorageConditions()
        {
            List<StorageCondition> list = new List<StorageCondition>();

            // public enum Conditions { Exposed, Isolated, EarthCooled, Airconditioning, Refrigerator, Freezer, Aquarium,Dryer,Moist }

            /*   if (storedIn != Storage.Conditions.Aquarium)
               {
                   light = 0f; // darkness
               }
               else
               {
                   light = The.Sim.PlaySite.Weather.SunIntensity;
               }*/


            /*
               public static float GetMoisture(Storage.Conditions StorageConditions) //,float ambientTemperature)
        {
            if (StorageConditions == Conditions.Aquarium)
            {
                return 1f;
            }
            else if(StorageConditions == Conditions.Moist)
            {
                return 0.9f;
            }
            else return 0f;
        }
             */

            /*
              switch (StorageConditions)
            {
                case Conditions.Isolated:
                case Conditions.Aquarium:
                case Conditions.Moist:
                    return ComputeIsolatedTemperature(ambientTemperature); // ambientTemperature;
                case Conditions.Dryer:
                    return Hot;
                case Conditions.EarthCooled:
                    return EarthCooledTemperature;
                case Conditions.Refrigerator:
                    if (isPowered)
                    {
                        return RefrigeratorTemperature;
                    }
                    else return ambientTemperature;
                case Conditions.Freezer:
                    if (isPowered)
                    {
                        return FreezerTemperature;
                    }
                    else return ambientTemperature;
                case Conditions.Airconditioning:
                    if (isPowered)
                    {
                        return RoomTemperature;
                    }
                    else return ambientTemperature;
                default: return ambientTemperature;
            }*/

            /// <summary>
            /// In Kelvin. 21 degree Celsius
            /// </summary>
            const float AirConTemperature = 294f;


            /// <summary>
            /// In Kelvin. 5 degree Celsius
            /// </summary>
            const float RefrigeratorTemperature = 278f;

            /// <summary>
            /// In Kelvin. 11 degree Celsius
            /// </summary>
            const float EarthCooledTemperature = 284f;


            /// <summary>
            /// In Kelvin. -18 degree Celsius
            /// </summary>
            const float FreezerTemperature = 255f;

            /// <summary>
            /// In Kelvin. 20 degree Celsius
            /// </summary>
            const float RoomTemperature = 293f;

            /// <summary>
            /// In Kelvin. 40 degree Celsius
            /// </summary>
            const float Hot = 313f;

            list.Add(new StorageCondition()
                {
                    KeyName = "aquarium",
                    Name = "Aquarium",
                    Description = "The conditions in a water filled tank",
                    FixedMoisture = 1f,
                    IsolatedTemperature = true
                });

            list.Add(new StorageCondition()
            {
                KeyName = "exposed",
                Name = "Outside",
                Description = "Open air conditions, exposed to the elements" //"In open air, exposed to the elements"
            });

            list.Add(new StorageCondition()
            {
                KeyName = "isolated",
                Name = "Inside",
                Description = "Inside conditions with the temperature influenced by the outside temperature", //"Inside. The temperature follows the outside temperature, but is skewed closer to room temperature"
                FixedLightLevel = 0f,
                FixedMoisture = 0f, // should be ambient moisture...
                IsolatedTemperature = true
            });

            list.Add(new StorageCondition()
            {
                KeyName = "moist",
                Name = "Moist",
                Description = "Inside conditions with a high humidity",
                FixedLightLevel = 0f,
                FixedMoisture = 0.9f,
                IsolatedTemperature = true
            });

           /* list.Add(new StorageCondition()
            {
                KeyName = "dryer",              
                FixedMoisture = 0f,
                FixedTemperature = Hot // hack!
            });*/

            list.Add(new StorageCondition()
            {
                KeyName = "earthCooled",
                Name = "Earth cooled",
                Description = "The conditions inside a dry hole, cooled by the earth. The Storage hole as well as other structures can provide this storage",
                FixedLightLevel = 0f,
                FixedMoisture = 0f,
                FixedTemperature = EarthCooledTemperature
            });

            list.Add(new StorageCondition()
            {
                KeyName = "underWater", // LArs: made this copy of earthCooled to give better ui feedback on the fish traps
                Name = "Under water",
                Description = "Immersed in water",
                FixedLightLevel = 0f,
                FixedMoisture = 0f,
                FixedTemperature = EarthCooledTemperature
            });

            list.Add(new StorageCondition()
            {
                KeyName = "refrigerator",
                Name = "Refrigerated",
                Description = "The conditions at a fixed low temperature. This requires cooling elements and a power supply.",
                RequiresPower = true,
                FixedLightLevel = 0f,
                FixedMoisture = 0f,
                FixedTemperature = RefrigeratorTemperature
            });

            list.Add(new StorageCondition()
            {
                KeyName = "freezer",
                Name = "Deep freeze",
                Description = "The conditions of a deep freeze",
                RequiresPower = true,
                FixedLightLevel = 0f,
                FixedMoisture = 0f,
                FixedTemperature = FreezerTemperature
            });

            list.Add(new StorageCondition()
            {
                KeyName = "airconditioning",
                Name = "Airconditioned",
                Description = "Indoor conditions at room temperature",
                RequiresPower = true,
                FixedLightLevel = 0f,
                FixedMoisture = 0f,
                FixedTemperature = RoomTemperature
            });


            return list;
        }


        protected override List<DegradeType> InitDegradeTypes()
        {
       
                List<DegradeType> listOfDegradeTypes = new List<DegradeType>();

                //add this to all structures that are automatically maintained:
                string structureDegradeExplanation = " \nNOTE: Colonists will automatically do maintenance on a structure if they have time and tools available. This will delay its deterioration."; //or, more verbose: "NOTE: Colonists will automatically do maintenance on a structure if they have time and tools available. This will delay its breaking down. 
            //add this to tools, weapons,equipment. not food and ingredients..:
                string itemDegradeExplanation = " \nNOTE: Unlike structures, items will NOT be provided maintenance by colonists. Only proper storage can extend an item's lifespan.";
 

                    DegradeType degradeType = new DegradeType()
                    {
                        Name = "'RAW SEAFOOD'",//"Raw Seafood"
                        KeyName = "raw seafood",
                        Description = "Will decompose very quickly if not stored cool or preferably frozen.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.02f), 
                    new Vector2(DegradeType.Refrigeration, 0.15f), 
                    new Vector2(DegradeType.RoomTemperature, 2.1f) },  // 1.1f  buffed it MP

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'FLESH SHREDS'", //"Flesh Shreds" quick decomposition of shreds  so they dissappear if the animal stops eating also so that the progress bars don't stay and cover the screen 
                        KeyName = "fleshShreds",
                        Description = "Shreds of flesh from animals feeding. Will decompose very quickly",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 2f), 
                    new Vector2(DegradeType.Refrigeration, 5f), 
                    new Vector2(DegradeType.RoomTemperature, 30f) },  

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.4f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'CAN BE SUN DRIED'", //"Sun Drying" with this profile, the food can never spoil, or..? more damage means faster drying.
                        KeyName = "sun drying",
                        Description = "This item can be dried in the open. Quicker options may be possible.",
                        TemperatureDamage = new Vector2[]
                        {
                            new Vector2(DegradeType.DeepFreeze, 0f),
                            new Vector2(DegradeType.Refrigeration, 0.15f),
                            new Vector2(DegradeType.RoomTemperature, 2.7f),//old 2.1
                            new Vector2(DegradeType.Hot, 6f),
                        },

                        MoistureDamage = new Vector2[]
                        { 
                            new Vector2(0f, 0.01f), // LARS: they are data points. X: 0 is dry, 1 is fully immersed  Y: damage amount per day
                            new Vector2(1f, 0f)//bso old:1f,1f
                        }, // moisture should not accelerate drying. it should probably be a negative 'damage' value, but then it may crash...

                        LightDamage = new Vector2[] 
                        { 
                            new Vector2(0f, 0.0f),  //MP: I have absolutely no clue what these "vectors" mean. forgot. I've just copied the raw seafood for now... I want a sun drying process, so sunlight should have a big accelerating effect, but how?
                            new Vector2(1f, 0.4f)
                        },// LARS: they are data points. X: 0 is darkness, 1 is bright sunshine  Y: damage amount per day
                    };
                    listOfDegradeTypes.Add(degradeType);

            

                    degradeType = new DegradeType()
                    {
                        Name = "'ENZYME'",//"Enzyme"
                        KeyName = "enzyme",
                        Description = "Enzymes should be used immediately as they have very limited shelf life",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.1f), 
                    new Vector2(DegradeType.Refrigeration, 2f), 
                    new Vector2(DegradeType.RoomTemperature, 3f) },  // MP feb 2015 I want quick degradation of enzymes, you need to have a constant supply. (for gameplay reasons on twinkler island).
                        //though, if they don't have access to a freezer on twinkler island, we can let enzymes last longer when freezed

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.8f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.4f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);



                    degradeType = new DegradeType()
                    {
                        Name = "'RAW MEAT'",//"Raw Meat"
                        KeyName = "raw meat",
                        Description = "Will decompose quickly unless stored in a cool, dry and dark environment.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.01f), 
                    new Vector2(DegradeType.Refrigeration, 0.15f), 
                    new Vector2(DegradeType.RoomTemperature, 1.7f) },  // 0.9f buffed it MP

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);



                    listOfDegradeTypes.Add(new DegradeType() 
                    {
                        /*
                         * these values are the raw meat values but halfed, 
                         * to simulate the same lenght of degrade time for both carcass and meat.
                         */
                        Name = "'CARCASS DECOMPOSITION'",//"Carcass decomposing"
                        KeyName = "carcassDecomposing",
                        Description = "A carcass will decompose quickly unless stored in a cool, dry and dark environment.",
                        TemperatureDamage = new Vector2[]
                        {
                            new Vector2(DegradeType.DeepFreeze, 0.005f),
                            new Vector2(DegradeType.Refrigeration, 0.075f),
                            new Vector2(DegradeType.RoomTemperature, 0.85f)
                        },
                        MoistureDamage = new Vector2[]
                        {
                            new Vector2(0f, 0.005f),
                            new Vector2(1f, 0.5f)
                        },
                        LightDamage = new Vector2[]
                        {
                            new Vector2(0f, 0.0f),
                            new Vector2(1f, 0.2f)
                        },
                    });

                    degradeType = new DegradeType()
                    {
                        Name = "'DECOMPOSED FOOD'",//"Decomposed food"
                        KeyName = "decomposedFood",
                        Description = "This food item has recently spoiled and will continue to decompose, particularly quickly in the open or in humid conditions.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.01f), 
                    new Vector2(DegradeType.Refrigeration, 0.15f), 
                    new Vector2(DegradeType.RoomTemperature, 1.7f) },  // 0.9f buffed it MP

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'COOKED FOOD'",//"Cooked food"
                        KeyName = "cooked food",
                        Description = "This food item will spoil if not quickly refrigerated or frozen",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.008f), 
                    new Vector2(DegradeType.Refrigeration, 0.1f), 
                    new Vector2(DegradeType.RoomTemperature, 1.3f) },  //0.8f buffed it MP

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

                    degradeType = new DegradeType()
                    {
                        Name = "'HOT FOOD'",  //"Hot food" makes hot food degrade to cold food. 
                        KeyName = "hotFood",
                        Description = "This item is best consumed quickly, before it cools off",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 20f), 
                    new Vector2(DegradeType.Refrigeration, 6f /* 18f*/), 
                    new Vector2(DegradeType.RoomTemperature, 3f /* 14f*/) },  // Degrade slower. so standing orders are not too wasteful

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), 
                    new Vector2(1f, 0f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0f), 
                    new Vector2(1f, 0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

/* mp presently not used
                    degradeType = new DegradeType()
                    {
                        Name = "Bread and Vegetables",
                        KeyName = "bread and vegetables",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.005f), 
                    new Vector2(DegradeType.Refrigeration, 0.04f), 
                    new Vector2(DegradeType.RoomTemperature, 0.5f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);
*/

                    degradeType = new DegradeType()
                    {
                        Name = "'PERISHABLE'", //"Perishable"  for food items
                        KeyName = "perishable",
                        Description = "This item will decay quickly unless it is kept cool, dry and dark, preferably frozen",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.005f), 
                    new Vector2(DegradeType.Refrigeration, 0.04f), 
                    new Vector2(DegradeType.RoomTemperature, 0.5f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'BIOWASTE'", //"Biowaste" for waste. mp feb 2015 I copied stats from Perishable but need different terms and tooltips for biowaste.
                        KeyName = "biowaste",
                        Description = "Organic waste which will quickly decompose in the open in humid conditions",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.005f), 
                    new Vector2(DegradeType.Refrigeration, 0.04f), 
                    new Vector2(DegradeType.RoomTemperature, 0.5f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'PERISHABLE. DO NOT FREEZE'",//"Perishable, no freeze"
                        KeyName = "perishable, no freeze",
                        Description = "This item will quickly decay outside and should be stored in a cool, dark and dry environment - but not frozen",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.6f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.15f), 
                    new Vector2(DegradeType.Refrigeration, 0.1f), 
                    new Vector2(DegradeType.RoomTemperature, 0.5f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'SOMEWHAT PRESERVED'", //"Somewhat preserved" for smoked food items and similar that have had their shelf life increased a bit. should keep 'a month'
                        KeyName = "somewhatPreserved",
                        Description = "This food item has been preserved in a rudimentary way and will last rather long under dry and dark conditions.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.005f), 
                    new Vector2(DegradeType.Refrigeration, 0.01f), 
                    new Vector2(DegradeType.RoomTemperature, 0.1f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(1f, 1f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.1f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

/* mp presently not used.:
                    degradeType = new DegradeType()
                    {
                        Name = "Live fish",
                        KeyName = "live fish",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.6f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.15f), 
                    new Vector2(DegradeType.Refrigeration, 0.1f), 
                    new Vector2(DegradeType.RoomTemperature, 0.5f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 10f),   //needs to be balanced so that they die within a week
                     new Vector2(0.99f, 10f),
                    new Vector2(1f, 0f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.4f)},
                    };
                    listOfDegradeTypes.Add(degradeType);
*/
                    degradeType = new DegradeType() // grains, preserved dry food and so on. ALSO used for many construction materials. AVOID confusion ,so do not use the tooltip: + itemDegradeExplanation, connected to Equipment. because these can be materials that are part of a structure which IS being maintained.
                    {
                        Name = "'BEST STORED DRY'",//"Stored dry"
                        KeyName = "stored dry",
                        Description = "This item will last very long under dry and dark conditions.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.0f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.0f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f),
                    new Vector2(DegradeType.Hot, 0.02f)},

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(0.9f, 0.4f),
                    new Vector2(1f, 0.8f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.1f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType() // food immersed in an acidic liquid, has long lifespan. A bit complicated, because they should be contained in a jar, which is then placed somewhere suitable.
                    //for now, I will just make it so that it really doesn't matter where the food is stored, as long as it's not wet (to avoid them putting the food in a fish tank?)
                    //the pickled items themselves have a RequiredStorageTags which should make agents prefer to put them in closed jars.
                    //..most of this copied from "stored dry"
                    {
                        Name = "'PICKLED FOOD'",//"Pickled food"
                        KeyName = "pickledFood",
                        Description = "Food immersed in an acidic liquid has a long lifespan when placed indoors.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.04f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.01f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f),
                    new Vector2(DegradeType.Hot, 0.02f)},

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), 
                    new Vector2(0.9f, 0.4f), //mp TODO. to avoid them putting the food into a fish tank? or does it not matter?
                    new Vector2(1f, 0.8f)},

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.1f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

                    degradeType = new DegradeType() // used for gunpowder
                    {
                        Name = "'KEEP DRY'", //
                        KeyName = "wetDecay",
                        Description = "This material decays quickly in humid conditions.",
                        TemperatureDamage = new Vector2[]
                        {
                            new Vector2(DegradeType.DeepFreeze, 0.0f),
                            new Vector2(DegradeType.Hot, 0.0f)
                        },
                        MoistureDamage = new Vector2[]
                        { 
                            new Vector2(0f,0f), 
                            new Vector2(0.5f, 0.02f),
                            new Vector2(1f, 2f)// 
                        },
                        LightDamage = new Vector2[]
                        {
                            new Vector2(0f, 0.0f), 
                            new Vector2(1f, 0.0f)
                        },
                    };
                    listOfDegradeTypes.Add(degradeType);

                    degradeType = new DegradeType() // infestation by scuttlers (bugs that live in medium-moisture, temperate areas (firegrass)) ...MP nov 6 th
                    {
                        Name = "'PRONE TO INFESTATION'", //"Prone to infestation"
                        KeyName = "proneToInfestation",
                        Description = "This plant material is vulnerable to the Scuttler bug which over time, will eat it, only leaving a few remains.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.0f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.0f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.14f)},  //   mp jan 2016. was: 0.3f        mp sep 2015 simplified this. I want it to have longer durability...   was:.......temperate area such as firegrass . MP july 2015:was 0.4f. reduced it to make it happen less quick.
                  

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), //
                    new Vector2(0.6f, 1f),  // mostly present in medium moisture areas, not swamps or deserts.
                    new Vector2(1f, 0.1f)}, //

                        LightDamage = new Vector2[] { //scuttlers are not influenced by amount of light
                    new Vector2(0f, 0f), 
                    new Vector2(1f, 0f)}, //mp sep 2015 was 0.1
                    };
                    listOfDegradeTypes.Add(degradeType);

                    #region "Remains of spoak leaves" //not used
/*
                    degradeType = new DegradeType() //the item that scuttler bugs leave behind, remains of spoak leaves,  has this degrade profile. it will degrade to organic matter with this degrade profile. I copied the numbers from the Stored Dry degrade profile
                    {
                        Name = "Remains of spoak leaves",
                        KeyName = "remainsOfSpoakLeaves",
                        TemperatureDamage = new Vector2[] 
                        { 
                            new Vector2(DegradeType.WaterFreezingPoint, 0.0f),
                            new Vector2(DegradeType.Hot, 1.7f)
                        },
                        MoistureDamage = new Vector2[] 
                        { 
                            new Vector2(0f, 0.1f), 
                            new Vector2(0.9f, 0.4f), 
                            new Vector2(1f, 0.8f)}, 

                        LightDamage = new Vector2[] 
                        { 
                            new Vector2(0f, 0.0f), 
                            new Vector2(1f, 0.4f)
                        },
                    };
                    listOfDegradeTypes.Add(degradeType);*/
                    #endregion

                    degradeType = new DegradeType()
                    {
                        Name = "'HIGHLY RESISTANT'", //"Dirt"
                        KeyName = "dirt",
                        Description = "This material will last indefinitely in storage and very long in the open although it will eventually erode and dissappear.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.0f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.0f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), // was (0f, 0.01f)
                    new Vector2(1f, 0f)}, //was (1f, 0.015f)

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

                   
                    listOfDegradeTypes.Add(new DegradeType()
                    {
                        Name = "'NEVER DEGRADES'",//"Never degrades"
                        KeyName = "neverDegrades",
                        Description = "This material will last for centuries, no matter the conditions it is exposed to.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.0f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.0f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), 
                    new Vector2(1f, 0f)}, 

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    });

                    degradeType = new DegradeType()
                    {
                        Name = "'RICKETY CONSTRUCTION'", //"Rickety construction" only for structures.
                        KeyName = "ricketyConstruction",
                        Description = "Low quality structure which requires very frequent maintenance to prevent it breaking down." + structureDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.48f),      //mp june 2016 WAS:0.6f changed until we get REPAIR feature (maintenance is not enough)     //MP: sort of a hack that shows wear and tear from wind and elements, not related to temperature at all.
                    new Vector2(DegradeType.Refrigeration, 0.48f),   //mp june 2016 WAS:0.6f changed until we get REPAIR feature (maintenance is not enough)
                    new Vector2(DegradeType.RoomTemperature, 0.48f) }, //mp june 2016 WAS:0.6f changed until we get REPAIR feature (maintenance is not enough)

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), //mp Sep 2015 removed moisture influence because player gets no feedback about best placement of structures. was (0f, 0.01f)
                    new Vector2(1f, 0f)}, //was (1f, 1f)

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'ADEQUATE CONSTRUCTION'", //"Adequate construction"   only for structures.
                        KeyName = "adequateConstruction", //
                        Description = "Mediocre quality structure which requires regular maintenance to prevent it breaking down." + structureDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.35f),            //mp june 2016 WAS: 0.4f. changed until we get REPAIR feature (maintenance is not enough)        //MP: sort of a hack that shows wear and tear from wind and elements, not related to temperature at all.
                    new Vector2(DegradeType.Refrigeration, 0.35f),        //mp june 2016 WAS: 0.4f. changed until we get REPAIR feature (maintenance is not enough) 
                    new Vector2(DegradeType.RoomTemperature, 0.35f) },    //mp june 2016 WAS: 0.4f. changed until we get REPAIR feature (maintenance is not enough) 

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), //mp Sep 2015 removed moisture influence because player gets no feedback about best placement of structures. was (0f, 0.01f)
                    new Vector2(1f, 0f)},//was (1f, 1f)

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'STURDY CONSTRUCTION'", //"Sturdy construction"   only for structures. (simple structures)
                        KeyName = "sturdyConstruction",
                        Description = "Good quality structure which requires little maintenance to keep it in shape." + structureDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.2f),  //MP: sort of a hack that shows wear and tear from wind and elements, not related to temperature at all.
                    new Vector2(DegradeType.Refrigeration, 0.2f), 
                    new Vector2(DegradeType.RoomTemperature, 0.2f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), //mp Sep 2015 removed moisture influence because player gets no feedback about best placement of structures. was (0f, 0.01f)
                    new Vector2(1f, 0f)},//was (1f, 1f)

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);

                    degradeType = new DegradeType()
                    {
                        Name = "'ADVANCED CONSTRUCTION'", //"Advanced construction" for very durable structures (NOT bushcraft)
                        KeyName = "advancedConstruction",
                        Description = "Very high quality structure which requires minimal maintenance." + structureDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.005f), //MP: sort of a hack that shows wear and tear from wind and elements, not related to temperature at all.
                    new Vector2(DegradeType.Refrigeration, 0.005f), 
                    new Vector2(DegradeType.RoomTemperature, 0.005f) }, 

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0f), //mp Sep 2015 removed moisture influence because player gets no feedback about best placement of structures.
                    new Vector2(1f, 0f)}, 

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.0f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    listOfDegradeTypes.Add(new DegradeType() // non-weather proof. ONLY use for tools, not building materials because the tooltip can lead to confusion when those parts are being maintained as part of a building.
                    {
                        Name = "'BRITTLE EQUIPMENT'",//"Improvised equipment"
                        KeyName = "improvisedEquipment",
                        Description = "Lower quality equipment will degrade relatively quick if left exposed to the elements." + itemDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.01f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.01f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f), //
                    new Vector2(1f, 0.15f)}, //

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.05f)},
                    });

                    degradeType = new DegradeType() // non-weather proof
                    {
                        Name = "'EQUIPMENT'", //"Equipment" ONLY use for tools, not building materials because the tooltip can lead to confusion when those parts are being maintained as part of a building.
                        KeyName = "equipment",
                        Description = "This equipment is reasonably durable but will last longer if stored inside." + itemDegradeExplanation,
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.01f), 
                    new Vector2(DegradeType.WaterFreezingPoint, 0.01f),                     
                    new Vector2(DegradeType.RoomTemperature, 0.0f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.01f),  
                    new Vector2(1f, 0.1f)}, 

                        LightDamage = new Vector2[] { 
                    new Vector2(0f, 0.0f), 
                    new Vector2(1f, 0.02f)},
                    };
                    listOfDegradeTypes.Add(degradeType);


                    degradeType = new DegradeType()
                    {
                        Name = "'DEGRADES WITH TIME'", // "Time"
                        KeyName = "timeDegrading",
                        Description = "This type of object will change or degrade with time, uninfluenced by storage conditions.",
                        TemperatureDamage = new Vector2[] { 
                    new Vector2(DegradeType.DeepFreeze, 0.0f), 
                    new Vector2(DegradeType.Refrigeration, 0.0f), 
                    new Vector2(DegradeType.RoomTemperature, 0f) },

                        MoistureDamage = new Vector2[] { 
                    new Vector2(0f, 0.00f), 
                    new Vector2(1f, 0f)},

                        LightDamage = new Vector2[] 
                        { 
                            new Vector2(0f, 2f), //half a day
                            new Vector2(1f, 2f)
                        },
                    };
                    listOfDegradeTypes.Add(degradeType);

                    return listOfDegradeTypes;
        }


        protected override List<FoodNutrientProfile> InitFoodNutrientProfiles()
        {
           
            List<FoodNutrientProfile> listOfNutrientProfiles = new List<FoodNutrientProfile>();
             
                FoodNutrientType foodEnergy = GameData.Instance.AllFoodNutrientTypes["foodEnergy"];
                FoodNutrientType protein = GameData.Instance.AllFoodNutrientTypes["protein"];
                FoodNutrientType micronutrients = GameData.Instance.AllFoodNutrientTypes["micronutrients"];
                FoodNutrientType stimulants = GameData.Instance.AllFoodNutrientTypes["stimulants"];



            //dec 2014. mp. I now use a tiered approach to fish/meat.  3 tiers.


                FoodNutrientProfile profile = new FoodNutrientProfile()
                {
                    KeyName = "poorMeat", // used for rotten meat etc.
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.4f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.020f },
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0008f}
                    }
                };
                listOfNutrientProfiles.Add(profile);




                profile = new FoodNutrientProfile()
                {
                    KeyName = "mediumMeat", 
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.7f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.048f },
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0024f}
                    }
                };
                listOfNutrientProfiles.Add(profile);



                profile = new FoodNutrientProfile()
                {
                    KeyName = "richMeat",  
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.8f },
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.08f },
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0035f }

                    }
                };
                listOfNutrientProfiles.Add(profile);

                profile = new FoodNutrientProfile()
                {
                    KeyName = "meatSoup", // mp july 2015 new. used for meat and fish soup and stew where some water is added.
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.4f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.020f },
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0008f}
                    }
                };
                listOfNutrientProfiles.Add(profile);


                //mp feb 2015 for gameplay reasons I'm introducing tiers on vegetables: (not to be used for the finished meals)
                profile = new FoodNutrientProfile()
                {
                    KeyName = "poorVegetables",// mp july 2015 changed 
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.1f }, 
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0029f }
                    }
                };
                listOfNutrientProfiles.Add(profile);

                //mp feb 2015 for gameplay reasons I'm introducing tiers on vegetables: (not to be used for the finished meals)
                profile = new FoodNutrientProfile()
                {
                    KeyName = "richVegetables",// mp july 2015 changed 
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.3f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.004f }, //some protein for vegetarians
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0031f }
                    }
                };
                listOfNutrientProfiles.Add(profile);



 //mp original staple  had foodEnergy, Amount = 0.6f

                
                profile = new FoodNutrientProfile()
                {
                    KeyName = "poorStaple",// mp july 2015 changed 
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.52f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.011f }, //some protein for vegetarians
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.0006f }
                    }
                };
                listOfNutrientProfiles.Add(profile);

                profile = new FoodNutrientProfile()
                {
                    KeyName = "richStaple",
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 0.6f },
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.016f }, //some protein for vegetarians
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.001f }
                    }
                };
                listOfNutrientProfiles.Add(profile);



                profile = new FoodNutrientProfile()
                {
                    KeyName = "highEnergy",// mp july 2015 changed. high energy fruit.
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 2f }, // LArs: I have increased the value above 1, it seems to be safe to do so.
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.009f }
                    }
                };
                listOfNutrientProfiles.Add(profile);


                profile = new FoodNutrientProfile()
                {
                    KeyName = "balanced meal",  //meat,vegetables,staple,fruit....(part of the) recommended daily nutrition for a grown man. corresponds to the amounts calculated in creatureloader under needs, 
                                                  // mp july 2015 changed.
                                                  //mp: feb 2015 - I wanna simplify food crafting, so that it's more easy for player to understand, this means a  staple ingredient with richStaple profile will yield a meal with richStaple profile. avoid mixed ingredient meals, it gets too complicated for us and player.
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 1.2f },   // 4*0,15f
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.096f },    //4*0.012f
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount =  0.0048f },  //4*0,0006f
                    }
                };
                listOfNutrientProfiles.Add(profile);


                profile = new FoodNutrientProfile()
                {
                    KeyName = "lowWeightBalancedMeal", 
                    FoodNutrientTypes = new[]{ 
                        new FoodNutrientAmount { Nutrient = foodEnergy, Amount = 1.5f }, 
                        new FoodNutrientAmount { Nutrient = protein, Amount = 0.11f } ,     
                        new FoodNutrientAmount { Nutrient = micronutrients, Amount = 0.006f },  

                    }
                };
                listOfNutrientProfiles.Add(profile);


                listOfNutrientProfiles.Add(new FoodNutrientProfile()
                {
                    KeyName = "lowStimulant",
                    FoodNutrientTypes = new[]{                       
                        new FoodNutrientAmount { Nutrient = stimulants, Amount = 0.005f }
                    }
                });


                return listOfNutrientProfiles;
           
           
        }

        protected override List<AllegianceEventType> InitAllegianceEvents()
        {
            List<AllegianceEventType> list = new List<AllegianceEventType>();

            list.Add(new AllegianceEventType()
            {
                KeyName = "cargoDeliveredToPlayer",
                Event = AllegianceEvents.CargoDeliveredToPlayer,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new ActionSetType[]
                    {
                        new ActionSetType()
                        {
                            KeyName = "sdfdfhdjghjg34",
                            Actions = new EventActionType[]{
                                new EventActionDialog(){
                                    KeyName = "ttytyyt88rtr",
                                   
                                    DisplayText = new DynamicText(){ Text = "New supplies have arrived." },
                                    DisplayImage = "Rescue"                                      
                                }

                            } 
                        }     
                    }
                }
            });


            list.Add(new AllegianceEventType()
            {
                KeyName = "transportAbortedContract",
                Event = AllegianceEvents.TransportToPlayerAborted,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new ActionSetType[]
                    {
                        new ActionSetType()
                        {
                            KeyName = "0cad94b3-ae09-4106-a6dd-0d97b3426972",
                            Actions = new EventActionType[]{
                                new EventActionDialog(){
                                    KeyName = "24a1cb12-11a4-4b83-bd73-2e94445990a2",
                                    
                                    DisplayText = new DynamicText(){ Text = "Mission aborted." },
                                    DisplayImage = "Rescue"                                      
                                }

                            } 
                        }                                

                    }
                }
            });


            return list;

        }

        protected override List<UpgradeCategory> InitUpgradeCategories()
        {
            return UpgradeCategoryLoader.Init();
        }

        protected override List<UpgradeProfile> InitUpgradeProfiles()
        {
            return UpgradeProfileLoader.Init();
        }

        protected override List<Entities.Biological.BioOrderType> InitBioOrderTypes()
        {
            return OrdersLoader.Init();
        }

        protected override List<PersonalityType> InitPersonalityTypes()
        {
            return PersonalityLoader.Init();
        }

        protected override List<RepairProfile> InitRepairProfiles()
        {
            return RepairProfileLoader.Init();
        }

        protected override List<SimEffects.EffectProfileType> InitEffectProfileTypes()
        {
            return EffectProfileLoader.Init();
        }

        protected override List<SimEffects.EffectType> InitEffectTypes()
        {
            return EffectTypeLoader.Init();
        }

        protected override List<SiteData> InitSiteData()
        {
            return SiteDataLoader.Init();
        }

        protected override List<AllegianceData> InitAllegianceData()
        {
            return AllegianceDataLoader.Init();
        }

        protected override List<SiteTemplate> InitSiteTemplates()
        {
            return SiteTemplateLoader.Init();
        }

        protected override List<ExpeditionData> InitExpeditionData()
        {
            return ExpeditionDataLoader.Init();
        }

        protected override List<AllegianceTemplate> InitAllegianceTemplates()
        {
            return AllegianceTemplateLoader.Init();
        }

        protected override List<TradeProfile> InitTradeProfiles()
        {
            return TradeProfileLoader.Init();
        }

        protected override List<PricesProfile> InitPricesProfiles()
        {
            return PricesProfileLoader.Init();
        }

        protected override List<TradeGroup> InitTradeGroups()
        {
            return TradeGroupLoader.Init();
        }

        protected override List<EntityData> InitEntityData()
        {
            return EntityDataLoader.Init();
        }

        protected override List<OfferDemandProfile> InitOfferDemandProfiles()
        {
            return OfferDemandProfileLoader.Init();
        }

        protected override List<VehiclesProfile> InitVehicleProfiles()
        {
            return VehiclesProfileLoader.Init();
        }

        protected override List<StructuresProfile> InitStructureProfiles()
        {
            return StructuresProfileLoader.Init();
        }

        protected override List<TraitTemplate> InitTraitTemplates()
        {
            return TraitTemplateLoader.Init();
        }

        protected override List<CultureTemplate> InitCultureTemplates()
        {
            return CultureTemplateLoader.Init();
        }

        protected override List<TierArea> InitTierAreas()
        {
            List<TierArea> list = new List<TierArea>(); //mp when you write descritptions, take care not to mix in the words basic, and advanced in tiers where they don't belong!

            list.Add(new TierArea()
            {
                KeyName = "comfortSurvival",                                
                Icon = "lcd_icon_tier_comfort1",
                Tier = "survival",
                Area = RatingTypes.Comfort,
                Description = "At this tech level, we only use the barest minimum of shelter against the elements. Our shelters are made quickly with simple materials and require frequent maintenance to remain standing." //"Only the barest minimum of shelter against the elements is provided here. Shelters are made quickly with ready materials and require frequent repairs to remain standing."
            });

            list.Add(new TierArea()
            {
                KeyName = "comfortBasic",
                Icon = "lcd_icon_tier_comfort2",
                Tier = "basic",
                Area = RatingTypes.Comfort,
                Description = "At this tech level, we build sturdy buildings with sparse amenities. We allow ourselves to consume some simple stimulants." //Sturdier buildings but with sparse amenities. Some unsophisticated stimulants are offered for consumption as well.
            });
            list.Add(new TierArea()
            {
                KeyName = "comfortMedium",
                Icon = "lcd_icon_tier_comfort3",
                Tier = "medium",
                Area = RatingTypes.Comfort,
                Description = "At this tech level, we have buildings which are quite comfortable and offer a good range of amenities. We enjoy a variety of good quality stimulants." //"Buildings which are quite comfortable and offer a good range of amenities. A wider range of stimulants are offered too."
            });
            list.Add(new TierArea()
            {
                KeyName = "comfortAdvanced",
                Icon = "lcd_icon_tier_comfort4",
                Tier = "advanced",
                Area = RatingTypes.Comfort,
                Description = "At this tech level, we have advanced habitats manufactured with precision technology, offering us the ultimate in creature comforts." //"Advanced habitats manufactured with precision technology, offering the ultimate in creature comforts."
            });

            list.Add(new TierArea()
            {
                KeyName = "foodSurvival",
                Icon = "lcd_icon_tier_food1",
                Tier = "survival",
                Area = RatingTypes.Food,
                Description = "At this tech level, we gather or hunt food directly from the environment, and only use very simple food preparation and storage." //"Food gathered or hunted directly from the environment, only very basic preparation and storage is available."
            });
            list.Add(new TierArea()
            {
                KeyName = "foodBasic",
                Icon = "lcd_icon_tier_food2",
                Tier = "basic",
                Area = RatingTypes.Food,
                Description = "At this tech level, we can feed a small community by basic plant farming and we maintain reliable food stores by using better food storage and preparation methods." //"Horticulture, better food storage and preparation methods enable larger food stores to be maintained."
            }); 
            list.Add(new TierArea()
            {
                KeyName = "foodMedium",
                Icon = "lcd_icon_tier_food3",
                Tier = "medium",
                Area = RatingTypes.Food,
                Description = "At this tech level, we employ efficient food production and storage methods which make it possible to feed larger colonies."  //"More advanced food production and storage methods make it possible to feed larger colonies efficiently."
            }); 
            list.Add(new TierArea()
            {
                KeyName = "foodAdvanced",
                Icon = "lcd_icon_tier_food4",
                Tier = "advanced",
                Area = RatingTypes.Food,
                Description = "At this tech level, we use technologies to synthesize food which provide the greatest variety and production capacity available." //"Technologies to synthesize food provide the greatest variety and production capacity available."
            });

            list.Add(new TierArea()
            {
                KeyName = "securitySurvival",
                Icon = "lcd_icon_tier_security1",
                Tier = "survival",
                Area = RatingTypes.Security,
                Description = "At this tech level, we make do with simple weapons like spears and bows which provide a minimum of protection against threats." //"Simple weapons like spears and bows provide a minimum of protection against threats."
            });
            list.Add(new TierArea()
            {
                KeyName = "securityBasic",
                Icon = "lcd_icon_tier_security2",
                Tier = "basic",
                Area = RatingTypes.Security,
                Description = "At this tech level, we use simple firearms which give us basic protection and stand-off range." //"Simple firearms offer greater protection and stand-off range."
            }); 
            list.Add(new TierArea()
            {
                KeyName = "securityMedium",
                Icon = "lcd_icon_tier_security3",
                Tier = "medium",
                Area = RatingTypes.Security,
                Description = "At this tech level, we use higher-precision firearms like rifles which give a good chance to eliminate threats in a safe and efficient manner." //"Higher-precision firearms like rifles give a better chance to eliminate threats in a safe and efficient manner."
            }); 
            list.Add(new TierArea()
            {
                KeyName = "securityAdvanced",
                Icon = "lcd_icon_tier_security4",
                Tier = "advanced",
                Area = RatingTypes.Security,
                Description = "At this tech level, we get the best possible protection using ultra-high precision kinetic weapons, lasers and robotic systems." //"Ultra-high precision kinetic weapons, lasers and robotic systems give the best possible protection."
            });


            return list;
        }

        protected override List<Tiers.TierType> InitTiers()
        {
            List<TierType> list = new List<TierType>();
            
            list.Add(new TierType()
            {
                KeyName = "survival",
                Name = "Survival",
                Description = "This policy tier represents the minimum requirements to stay alive.",
                Icon = "lcd_icon_tier_optional1",
                UpperEdge = 0.16f               
            });

            list.Add(new TierType()
            {
                KeyName = "basic",
                Name = "Basic",
                Description = "Moving to this tier requires use of technology, planning and effort beyond the pure survival level. But the rewards are a higher standard of living and reduced uncertainty.",
                Icon = "lcd_icon_tier_optional2",
                UpperEdge = 0.32f               

            });

            list.Add(new TierType()
            {
                KeyName = "medium",
                Name = "Medium",
                Description = "A colony at this development tier has expended even more organizational effort and planning. Resources are used in a sophisticated manner and the colony will experience more safety and comfort as a result.",
                Icon = "lcd_icon_tier_optional3",
                UpperEdge = 0.64f               
            });

            list.Add(new TierType()
            {
                KeyName = "advanced",
                Name = "Advanced",
                Description = "Highly advanced technologies are needed to reach this tier. It is only reachable using tools or materials brought from Earth and sustained or replicated on Antheia. It cannot be reached using Antheia's resources alone.",
                Icon = "lcd_icon_tier_optional4",
                UpperEdge = 1f               
            });

            return list;
        }

        protected override List<TriggerType> InitTriggerTypes()
        {
            List<TriggerType> list = new List<TriggerType>();

           
                list.Add(new TriggerType()
                {
                    KeyName = "prey",
                    Range = 200f,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    IsPrey = true
                });

                list.Add(new TriggerType()
                {
                    KeyName = "smallImprovisedTrapTrigger", //was "springSnare"
                    Range = 20f,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CanTriggerWhenUndetected = true,
                    ActionSetsKey = "springSnareTriggered",
                });
                list.Add(new TriggerType()
                {
                    KeyName = "spikeTrapTrigger",
                    Range = 15f, //make sure that it looks good when animals are near this, they should not walk over it, nor should damage be too spread out
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CanTriggerWhenUndetected = true,
                    ActionSetsKey = "springSnareTriggered",
                });
                list.Add(new TriggerType()
                {
                    KeyName = "mineTrigger",
                    Range = 28f,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CanTriggerWhenUndetected = true,
                    MaxTimesToTriggerBeforeExpiring = 1,
                    ActionSetsKey = "landMineTriggered"
                });
                list.Add(new TriggerType()
                {
                    KeyName = "animalMigrateTrigger", //used for migrating animals to dissappear off map edge                    
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CanTriggerWhenUndetected = true,                    
                    ActionSetsKey = "animalLeavesMapEdge"
                });

                list.Add(new TriggerType()
                {
                    KeyName = "creature", // an interesting creature that the colonists should look at
                    Range = 300f,
                     Interest = new Interest()
                     {
                         InterestLevelMean = 40f, 
                         InterestLevelStdDeviation = 6f
                     }, 
                     Priority = TriggerPriority.Lowest,
                    DurationBetweenTriggerUpdatesInSeconds = 4f,                   
                });

                list.Add(new TriggerType()
                {
                    KeyName = "drivenVehicle",
                    Range = 200f,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    IsDrivenVehicle = true,
                    Interest = new Interest(){ InterestLevelMean = 4f }
                });

              /*  list.Add(new TriggerType()
                {
                    KeyName = "personDying",
                    Range = 200f,
                    LifetimeInSeconds = 2f,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CooldownInSeconds = 5f,
                    Interest = new Interest() { InterestLevel = 1f  }
                });*/

              
                list.Add(new TriggerType()
                {
                    KeyName = "entityDied",
                    Range = 300f,
                    LifetimeInSeconds = 2f,
                    IsEntityDied = true,
                    Priority = TriggerPriority.Normal,
                    DurationBetweenTriggerUpdatesInSeconds = 1f,
                    CooldownInSeconds = 10f,
                    Interest = new Interest(){ InterestLevelMean = 50f, InterestLevelStdDeviation = 5f  }
                    
                });

                return list;

           
        }

        protected override List<DetectionType> InitDetectionTypes()
        {
            List<DetectionType> listOfDetectionTypes = new List<DetectionType>();
            /*
             *  SHARED DETECTION TAGS 
             */ //this tag is used to define how all animals and humans detect the treedemon, primarily:
            DetectionFactor wellHiddenAnimal = new DetectionFactor() { TypeTag = "wellHiddenAnimal", Value = 0.05f, RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["hunting"] };
            DetectionFactor huge = new DetectionFactor() { TypeTag = "huge", Value = 2f, DistanceToAlwaysDetect = 200f };    
            DetectionFactor hardToSpot = new DetectionFactor() { TypeTag = "hardToSpot", Value = 0.01f, DistanceToAlwaysDetect = 48f };    
            DetectionFactor kindaHardToSpot = new DetectionFactor() { TypeTag = "kindaHardToSpot", Value = 0.04f, DistanceToAlwaysDetect = 120f };    
            
            listOfDetectionTypes.Add(new DetectionType()
            {
                KeyName = "human",
                DetectionFactors = new[]
                {   
                    //notifications consist of new log entry (AddLogMessageWhenDetected = true), and later implemented flashing on game area, maybe an event dialog screen is triggered also.
                    //the dialog screen event is independent of having the (AddLogMessageWhenDetected = true/ false), which means I can have the first detection of firewood trigger a dialog screen even though each subsequent detection doesn't  create a log notification..
                    // LOOK: the amount of time an agent spends when foraging is controlled in AIConstants.cs :,  MinimumChanceToStopAndLookWhenSearchingResources     ChanceToStopAndLookWhenSearchingResourcesFactor      TimeToWaitWhenStoppedAndSearchingResourcesMean  TimeToWaitWhenStoppedAndSearchingResourcesStdDev 
                                   
                  
                    // make big stuff appear instantly when in range (DistanceToAlwaysDetect):
                    new DetectionFactor { TypeTag = "largeAboveGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 140f, InterestLevelForSpottedResourceMean = 0f, Value = 0.9f, AddLogMessageWhenDetected = false },
                    new DetectionFactor { TypeTag = "largeOnGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 90f, InterestLevelForSpottedResourceMean = 0f, Value = 0.8f, AddLogMessageWhenDetected = false },
                    new DetectionFactor { TypeTag = "smallAboveGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.3f, AddLogMessageWhenDetected = false, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },
                    new DetectionFactor { TypeTag = "inShallowWater", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = true, },
                    new DetectionFactor { TypeTag = "smallAnimalAboveGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.6f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },              
                    new DetectionFactor { TypeTag = "smallAnimalAboveGroundHardToSee", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = true },  

                    //RequiresExamineAction////
                                         // Lars: with RequiresExamineAction, Value should probably not be less than 0.1. Otherwise it can take 4-10 zones to find stuff, almost impossible.
                    new DetectionFactor { TypeTag = "smallAnimalOnGround", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.15f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, //nov 2015 Value = 0.1f
                    new DetectionFactor { TypeTag = "smallAnimalBelowGround", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.1f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },  //nov 2015  Value = 0.05f              
                    new DetectionFactor { TypeTag = "aboveGroundHardToSee", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.15f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, //nov 2015 Value = 0.1f
                    //for farm spots and land resource deposits: guaranteed detection
                    new DetectionFactor { TypeTag = "farmSpotAndResourceDeposit", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.95f,  AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },//july 2016 was 0.15f 
                    
                     // for fish trap spots: 
                    new DetectionFactor {  TypeTag = "inDeeperWaterFishingSpot", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.2f, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },//july 2016 was 0.15f     //nov 2015 Value = 0.06f
           
                    new DetectionFactor { TypeTag = "inDeeperWater", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.1f,  AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, //nov 2015 Value = 0.028f  had to increase the factor because it is on the coast, it rarely gets detected??
                    new DetectionFactor { TypeTag = "fruitOnGroundHardToFind", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.15f,  AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },//nov 2015 Value = 0.05f     //0.2f  
                    new DetectionFactor { TypeTag = "smallHidden", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.1f , AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, //nov 2015 Value = 0.05f

                    // MP: "smallHidden" DetectionFactor = 0.014f causes them to sometimes need multiple examine attempts to reveal a resource.

                     // old entity detection factors:
                     new DetectionFactor() { TypeKey = "entity:bird", Value = 1f, DistanceToAlwaysDetect = 600f /*detect instantly...*/ },
                     wellHiddenAnimal,
                     huge,
                     hardToSpot,
                     kindaHardToSpot

                  }                
                 

            });

            // when not using the default detectionType remeber to add the demonTree and spikePlant detectionfactors
            listOfDetectionTypes.Add(new DetectionType()
            {
                KeyName = "defaultDetection",
                DetectionFactors = new[]
                {  
                     wellHiddenAnimal,
                     huge,
                     hardToSpot,
                     kindaHardToSpot,
                }
            });

            listOfDetectionTypes.Add(new DetectionType()
           {
               Comments = "This sensor should not have any detecting capabilites other than the ability to report its parent's status.",
               KeyName = "communicationSensor",
               DetectionDisabled = true

           });


            listOfDetectionTypes.Add(new DetectionType()
            {
                KeyName = "motionSensor", // this is how the motion sensor detects. 

                //MP: it has default detection of creatures , so I've not put any special detectionfactors in here:

     /*           EntityTypeDetectionFactors = new[]
                  {
                       new EntityTypeDetectionFactor()
                       {
                            EntityTypeKeyName = "entity:human", DetectionFactor = 2f, DistanceToAlwaysDetect = 200f //MP: set it so that the bushie becomes a bit more active around humans by detecting them.
                       }                   

                  },
*/

                /*
                 TESTS:  Value = 0.05f, skill = 0.8: 
                 *      TEST 1: required 3 forage tasks before detecting. 
                 *      TEST 2: detected after 1 task
                 * 
                 */
                DetectionFactors = new[]{ 
                   // the motion sensor detects small animals (resources) as well:
                    new DetectionFactor { TypeTag = "smallAnimalAboveGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.6f, AddLogMessageWhenDetected = true },  
                    new DetectionFactor { TypeTag = "smallAnimalAboveGroundHardToSee", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = true },                   
                    new DetectionFactor { TypeTag = "smallAnimalOnGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = false },
                    new DetectionFactor { TypeTag = "smallAnimalBelowGround", RequiresExamineAction = false, DistanceToAlwaysDetect = 0f, Value = 0.02f, AddLogMessageWhenDetected = true },  

                     // motion sensor does not detect these. we want it not to see these resources, so i set the skill needed to "forage"..which it doesn't have:
                    new DetectionFactor { TypeTag = "fruitOnGroundHardToFind", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.05f, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },  //0.2f                    
                    new DetectionFactor { TypeTag = "inDeeperWater", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.028f, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, // had to increase the factor because it is on the coast, it rarely gets detected??
                  
                    new DetectionFactor {  TypeTag = "inDeeperWaterFishingSpot", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.06f, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] }, 

                    new DetectionFactor { TypeTag = "largeAboveGround", RequiresExamineAction = true, DistanceToAlwaysDetect = 140f, InterestLevelForSpottedResourceMean = 0f, Value = 0.9f, AddLogMessageWhenDetected = false, SkillToUse = GameData.Instance.AllSkillTypes["foraging"]  },
                    new DetectionFactor { TypeTag = "largeOnGround", RequiresExamineAction = true, DistanceToAlwaysDetect = 90f, InterestLevelForSpottedResourceMean = 0f, Value = 0.8f, AddLogMessageWhenDetected = false, SkillToUse = GameData.Instance.AllSkillTypes["foraging"]  },

                    new DetectionFactor { TypeTag = "smallAboveGround", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.3f, AddLogMessageWhenDetected = false, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },
                    new DetectionFactor { TypeTag = "inShallowWater", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"]  },
                
                    new DetectionFactor { TypeTag = "aboveGroundHardToSee", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.05f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] },
 
                    new DetectionFactor { TypeTag = "smallHidden", RequiresExamineAction = true, DistanceToAlwaysDetect = 0f, Value = 0.014f, AddLogMessageWhenDetected = true, SkillToUse = GameData.Instance.AllSkillTypes["foraging"] } 
               

                 },

            });


            return listOfDetectionTypes;


        }

     /*   protected override List<StructureCategory> InitStructureCategories()
        {
           
                List<StructureCategory> listOfStructureCategories = new List<StructureCategory>();
               
                    listOfStructureCategories.Add(new StructureCategory() { KeyName = "shelter", Name = "Shelter", SortOrder = 0 });
                    listOfStructureCategories.Add(new StructureCategory() { KeyName = "production", Name = "Storage/Production", SortOrder = 5 });  //   combine storage/production?
                    listOfStructureCategories.Add(new StructureCategory() { KeyName = "defense", Name = "Defense", SortOrder = 10 });                  
                    listOfStructureCategories.Add(new StructureCategory() { KeyName = "miscellaneous", Name = "Miscellaneous", SortOrder = 20 });

                    return listOfStructureCategories;
         

        }*/


        /* private static void ReplaceItemTypePlaceholders()
         {
             foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllEntityTypes)
             {
                 if (kvp.Value.Parts != null && kvp.Value.Parts.Count > 0)
                 {
                     Dictionary<EntityType, int> replaced = new Dictionary<EntityType, int>();
                     foreach (KeyValuePair<EntityType, int> part in kvp.Value.Parts)
                     {
                         replaced.Add(GameData.Instance.AllEntityTypes[part.Key.KeyName], part.Value);
                     }

                     kvp.Value.Parts = replaced;
                 }

             }
         }*/

       
    }
}

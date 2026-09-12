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
    public class VehiclesProfileLoader
    {
        public static List<VehiclesProfile> Init()
        {
            List<VehiclesProfile> list = new List<VehiclesProfile>();

            list.Add(new VehiclesProfile()
            {
                KeyName = "bargeProfile",
                VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>()
                {
                    { 
                        "entity:barge", 
                        new VehiclesForHireType()
                        {
                             StartAmount = 1,                             
                             Price = 10,   //was  15                  NOTE: these prices are overridden on the Clay Pit scenario, search "entity:smallBarge"       
                             PricePerKilometer = 0.20f //was 0.30f
                        }
                    }
                }
            });

            list.Add(new VehiclesProfile()
            {
                KeyName = "advancedBargeProfile",
                VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>()
                {
                    { 
                        "entity:advancedBarge", 
                        new VehiclesForHireType()
                        {
                             StartAmount = 1,                             
                             Price = 10,   //was  15                  NOTE: these prices are overridden on the Clay Pit scenario, search "entity:smallBarge"       
                             PricePerKilometer = 0.20f //was 0.30f
                        }
                    }
                }
            });

            list.Add(new VehiclesProfile()
            {
                KeyName = "airliftProfile",
                VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>()
                {
                    { 
                        "entity:skimmer", 
                        new VehiclesForHireType()
                        {
                             StartAmount = 1,
                             Price = 300                    
                        }
                    },
                    { 
                        "entity:harpy", 
                        new VehiclesForHireType()
                        {
                             StartAmount = 1,
                             Price = 500                    
                        }
                    }
                }
            });

           

            return list;

        }

    }
}

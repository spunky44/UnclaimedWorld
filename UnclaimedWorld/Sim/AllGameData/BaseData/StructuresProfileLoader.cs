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
    public class StructuresProfileLoader
    {
        public static List<StructuresProfile> Init()
        {
            List<StructuresProfile> list = new List<StructuresProfile>();

            list.Add(new StructuresProfile()
            {
                Comments = "Low capacity...",
                KeyName = "smallPierProfile",
                StartingStructures = new SerializableDictionary<string, int>()
                {
                    { "structure:simplePort", 1 },
                    { "structure:radioHut", 1 }
                }
            });

            list.Add(new StructuresProfile()
            {
                KeyName = "mediumPierProfile",
                StartingStructures = new SerializableDictionary<string, int>()
                {
                    { "structure:canopyPort", 1 },
                    { "structure:radioHut", 1 }
                }
            });

            list.Add(new StructuresProfile()
            {
                KeyName = "largePierProfile",
                StartingStructures = new SerializableDictionary<string, int>()
                {
                    { "structure:largePier", 1 },
                    { "structure:radioHut", 1 }
                }                       
            });

            list.Add(new StructuresProfile()
            {
                Comments = "very large capacity helipad, for other sites",
                KeyName = "largeHeliportProfile",
                StartingStructures = new SerializableDictionary<string, int>()
                {
                    { "structure:heliportLarge", 1 },
                    { "structure:satelliteGroundStation", 1 }
                }

            });

            list.Add(new StructuresProfile()
            {
                KeyName = "mediumPierSatelliteProfile",
                StartingStructures = new SerializableDictionary<string, int>()
                {
                    { "structure:canopyPort", 1 },
                    { "structure:satelliteGroundStation", 1 }
                }
            });
          

            return list;

        }

    }
}

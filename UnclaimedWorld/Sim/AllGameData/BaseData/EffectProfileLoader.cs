using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AllGameData
{
    public class EffectProfileLoader
    {
        public static List<EffectProfileType> Init()
        {
            List<EffectProfileType> list = new List<EffectProfileType>();

            list.Add(new EffectProfileType()
            {
                    KeyName = "cheapAlcohol",
                    Name = "Alcohol",
                    Effects = new string[] { "stimulantComfortEffect" },
                    SortOrder = 10

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "improvedAlcohol",
                Name = "Alcohol",
                Effects = new string[] { "highStimulantComfortEffect" },
                SortOrder = 20
            });

            list.Add(new EffectProfileType()
            {
                KeyName = "caffeine",
                Name = "Caffeine",
                Description = "Caffeine is a stimulant. It increases the character's comfort level as well as the negative effects from lack of sleep.",
                Effects = new string[] { "stimulantComfortEffect" },
                SortOrder = 30

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "cloaking",
                Name = "Cloaking",
                Description = "Cloaking significantly decreases the chance of being detected.",
                Effects = new string[] { "cloaking" },
                SortOrder = 40

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "nightVision",
                Name = "Night vision",
                Description = "The character can see further at night.",
                Effects = new string[] { "nightVision" },
                SortOrder = 50

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "groundScanner",
                Name = "Ground penetrating sensor",
                Description = "The character can more easily detect lightly buried items and resources",
                Effects = new string[] { "groundScanner" },
                SortOrder = 60

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "leader",
                Name = "Leader",
                Description = "The leader will never leave the site", // and won't complain in group meetings",
                Effects = new string[] { "leaderCannotEmigrate", "leaderCannotComplain" },
                SortOrder = 70

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "mats",
                Name = "Mats",
                Description = "Floor mats give a small increase in comfort",
                Effects = new string[] { "smallHomeComfortEffect" },
                SortOrder = 80

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "simpleBed",
                Name = "Beds",
                Description = "Simple beds give a modest increase in comfort",
                Effects = new string[] { "modestHomeComfortEffect" },
                SortOrder = 90

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "furnitureEffect",
                Name = "Furniture",
                Description = "Chairs, table and shelves give a small increase in comfort",
                Effects = new string[] { "smallHomeComfortEffect" },
                SortOrder = 95

            });

            list.Add(new EffectProfileType()
            {
                KeyName = "openStove",
                Name = "Open stove",
                Description = "An open stove inside decreases comfort",
                Effects = new string[] { "openStoveComfortEffect" },
                SortOrder = 100

            });

            return list;
        }

    }
}

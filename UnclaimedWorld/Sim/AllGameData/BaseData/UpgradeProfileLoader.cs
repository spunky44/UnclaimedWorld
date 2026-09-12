using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData
{
    public class UpgradeProfileLoader
    {
        public static List<UpgradeProfile> Init()
        {
            List<UpgradeProfile> list = new List<UpgradeProfile>();
            list.Add(new UpgradeProfile()
            {
                KeyName = "survivalHome1People",
                UpgradeCategories = new string[] { "mats1People" }
            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "survivalHome2People",
                UpgradeCategories = new string[] { "mats2People" }

            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "survivalHome3People",
                UpgradeCategories = new string[] { "mats3People" }
            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "survivalHome4People",
                UpgradeCategories = new string[] { "mats4People" }
            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "basicHome3People",
                UpgradeCategories = new string[] { "bedsOrMats3People" }
            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "basicHome4People",
                UpgradeCategories = new string[] { "bedsOrMats4People", "furniture4People" }                        
              
            });


            list.Add(new UpgradeProfile()
            {
                KeyName = "workshopProfile",
                UpgradeCategories = new string[] { "workshop" }
            });

            list.Add(new UpgradeProfile()
            {
                KeyName = "cookhouseProfile",
                UpgradeCategories = new string[] { "stove", "communityHall", "smokeOven", "dryingShed" }
            });
            return list;
        }

    }
}

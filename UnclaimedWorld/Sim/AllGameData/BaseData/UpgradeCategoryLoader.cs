using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.AllGameData
{
    public class UpgradeCategoryLoader
    {
        public static List<UpgradeCategory> Init()
        {
            List<UpgradeCategory> list = new List<UpgradeCategory>();

            string matsDescription = "Upgrading the mats will increase the comfort level for the residents.";
            string matsName = "Mats";
            int matsSortOrder = 10;

            string bedsDescription = "Upgrading the beds will increase the comfort level for the residents.";
            string bedsName = "Beds";

            list.Add(new UpgradeCategory()
            {
                KeyName = "mats1People",
                Name = matsName,
                Description = matsDescription,
                SortOrder = matsSortOrder
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "mats2People",
                Name = matsName,
                Description = matsDescription,
                SortOrder = matsSortOrder
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "mats3People",
                Name = matsName,
                Description = matsDescription,
                SortOrder = matsSortOrder
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "mats4People",
                Name = matsName,
                Description = matsDescription,
                SortOrder = matsSortOrder
            });

            int bedsOrMatsSortOrder = 10;
           
            list.Add(new UpgradeCategory()
            {
                KeyName = "bedsOrMats3People",
                Name = bedsName,
                Description = bedsDescription,
                SortOrder = bedsOrMatsSortOrder         
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "bedsOrMats4People",
                Name = bedsName,
                Description = bedsDescription,
                SortOrder = bedsOrMatsSortOrder
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "furniture4People",
                Name = "Furniture",
                Description = "",
                SortOrder = 13
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "stove",
                Name = "Stove",
                Description = "Installing a stove makes it more convenient to cook food",
                SortOrder = 15
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "communityHall",
                Name = "Community hall",
                Description = "",//todo
                SortOrder = 20
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "smokeOven",
                Name = "Smoke oven",
                Description = "",//todo
                SortOrder = 25
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "dryingShed",
                Name = "Drying shed",
                Description = "",//todo
                SortOrder = 30
            });

            list.Add(new UpgradeCategory()
            {
                KeyName = "workshop",
                Name = "Workshop",
                Description = "",
                SortOrder = 40
            });



            return list;
        }

    }
}

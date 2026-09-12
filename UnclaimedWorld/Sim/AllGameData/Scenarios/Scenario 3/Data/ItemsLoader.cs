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
using UWGame.SimSide.Items;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data //TUTORIAL, CASTAWAYS
{
    class ItemsLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {
            //Add items: 
            #region lines

            listOfEntityTypes.Add(new EntityType("item:lines") //MP not a tool because its used as material input for build rope bridge
            {
                Name = "Wire rope", // http://www.steelwirerope.com/WireRopes/Galvanised/index.html#.VCLkIfmSzE0
                SummaryDescription = "Galvanized steel ropes",
                Description = "Strong ropes typically used for sail boat rigging.",
                ItemType = new ItemType() { MaximumBulk = 0.07f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry"
                },
                Category = GameData.Instance.AllEntityCategories["rawMaterials"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wireRope" } } } }
            });

            #endregion     

            #region Add Catamaran parts
            //for tut, to avoid player clicking salvage too early:
            listOfEntityTypes.Add(new EntityType("item:catamaranStart")
            {
                Name = "Catamaran",

                SummaryDescription = "The catamaran hull",
                //                  Description = "N/A",
                Icon = "typeIcon_strippedCatamaran", //todo
                ItemType = new ItemType() { MaximumBulk = 5f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry"
                },
                Category = GameData.Instance.AllEntityCategories["rawMaterials"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranItem" } } } }
            });

            listOfEntityTypes.Add(new EntityType("item:catamaranMastSail")
            {
                Name = "Catamaran mast",
                SummaryDescription = "The mast from the catamaran",
                //                  Description = "N/A",
                Icon = "typeIcon_catamaranMast",
                ItemType = new ItemType() { MaximumBulk = 5f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry"
                },
                Category = GameData.Instance.AllEntityCategories["rawMaterials"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranMastSail" } } } }
            });




            listOfEntityTypes.Add(new EntityType("item:strippedCatamaran")
            {
                Name = "Stripped catamaran",
                SummaryDescription = "We've stripped the catamaran for any useful materials",
                //                  Description = "N/A",
                Icon = "typeIcon_strippedCatamaran",
                ItemType = new ItemType() { MaximumBulk = 5f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry"
                },
                Category = GameData.Instance.AllEntityCategories["rawMaterials"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedCatamaran" } } } }
            });


            listOfEntityTypes.Add(new EntityType("item:catamaranMast")
            {
                Name = "Catamaran mast",
                SummaryDescription = "The mast from the catamaran",
                //                  Description = "N/A",
                Icon = "typeIcon_catamaranMast",
                ItemType = new ItemType() { MaximumBulk = 5f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry"
                },
                Category = GameData.Instance.AllEntityCategories["rawMaterials"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranMast" } } } }
            });
            #endregion

            #region Diamond knife override
            EntityType knife = AllGameData.ItemLoader.CreateDiamondKnife(listOfEntityTypes);
            // override these so they will carry spears instead!
            knife.ItemType.TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None }, 
                        };
            #endregion

            #region DELETE RECORDS/////////////////
            //not necessary to remove things because in TUTORIAL, production manager is on AVAILABLE, and player does not have access to production manager display options , so cannot see steps ahead.
            ////it's enough to remove the process in processloader////



         
            #endregion //////////////////////////







                    
          
        }
    }
}
